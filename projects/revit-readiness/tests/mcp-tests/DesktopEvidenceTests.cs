using System.Diagnostics;
using System.Text.Json;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    static async Task TraceSession(RawToolChild child, string revision, int id)
    {
        await child.Connect();
        await child.Send(CallWire(id, BridgeLimits.GetContext, "{}", revision));
        var response = await child.Receive(id);
        Check(response.GetProperty("result").GetProperty("isError").GetBoolean(), "Expected absent selected Revit PID");
        Check(response.GetProperty("result").GetProperty("structuredContent").GetProperty("code").GetString() == "BridgeUnavailable", "Wrong correlated error");
        child.Process.StandardInput.Close();
        await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Check(child.Process.ExitCode == 0, "Trace session did not exit naturally");
    }

    static async Task DesktopEvidenceSessions(string revision)
    {
        string root = Path.Combine(Evidence, "desktop-root-" + Guid.NewGuid().ToString("N"));
        string prior = Path.Combine(root, "session-existing", "server-to-client.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(prior)!);
        byte[] original = [0, 255, 13, 10, 42]; File.WriteAllBytes(prior, original);
        // Two simultaneous consumers followed by a reconnect using identical command arguments.
        await using (var first = new RawToolChild(int.MaxValue, false, revision, "desktop-first", ["--wire-evidence-root", root]))
        await using (var second = new RawToolChild(int.MaxValue, false, revision, "desktop-second", ["--wire-evidence-root", root]))
            await Task.WhenAll(TraceSession(first, revision, 101), TraceSession(second, revision, 102));
        await using (var third = new RawToolChild(int.MaxValue, false, revision, "desktop-reconnect", ["--wire-evidence-root", root]))
            await TraceSession(third, revision, 103);
        var sessions = Directory.GetDirectories(root).Where(p => Path.GetFileName(p) != "session-existing").ToArray();
        Check(sessions.Length == 3, "Each process must own a separate evidence directory");
        var observed = new HashSet<int>();
        foreach (string session in sessions)
        {
            Check(Guid.TryParseExact(Path.GetFileName(session)["session-".Length..], "N", out _), "Invalid session directory identity");
            var input = File.ReadAllLines(Path.Combine(session, "client-to-server.jsonl")).Select(s => JsonDocument.Parse(s).RootElement.Clone()).ToArray();
            var output = File.ReadAllLines(Path.Combine(session, "server-to-client.jsonl")).Select(s => JsonDocument.Parse(s).RootElement.Clone()).ToArray();
            int id = input.Single(r => r.GetProperty("method").GetString() == "tools/call").GetProperty("id").GetInt32();
            Check(observed.Add(id), "Mixed/repeated session evidence");
            Check(output.Count(r => r.TryGetProperty("id", out var v) && v.GetInt32() == id) == 1, "Missing or duplicate correlated response");
            Check(output.Length == 2, "Unexpected stdout or cross-session output");
        }
        Check(observed.SetEquals([101, 102, 103]), "Session identities lost");
        Check(File.ReadAllBytes(prior).SequenceEqual(original), "Existing evidence changed");
        Console.WriteLine("DESKTOP_TRACE_EVIDENCE protocol=" + revision + " path=" + root);
    }

    static async Task FixedEvidenceStillRefusesReuse()
    {
        string root = Path.Combine(Evidence, "fixed-root-" + Guid.NewGuid().ToString("N"));
        await using (var first = new RawToolChild(int.MaxValue, false, "2026-07-28", "fixed-first", ["--wire-evidence", root]))
            await TraceSession(first, "2026-07-28", 104);
        var before = Directory.GetFiles(root).ToDictionary(p => Path.GetFileName(p)!, File.ReadAllBytes);
        await using (var second = new RawToolChild(int.MaxValue, false, "2026-07-28", "fixed-refused", ["--wire-evidence", root]))
        {
            await second.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Check(second.Process.ExitCode == 1, "Fixed capture silently reused existing evidence");
        }
        Check(Directory.GetFiles(root).Length == before.Count && before.All(p => File.ReadAllBytes(Path.Combine(root, p.Key!)).SequenceEqual(p.Value)), "Failed reuse changed retained evidence");
    }

    static async Task InvalidEvidenceArguments(bool reverse)
    {
        string root = Path.Combine(Evidence, "conflicting-evidence-" + Guid.NewGuid().ToString("N"));
        string fixedPath = Path.Combine(root, "fixed"), shared = Path.Combine(root, "shared");
        string[] options = reverse ? ["--wire-evidence-root", shared, "--wire-evidence", fixedPath] : ["--wire-evidence", fixedPath, "--wire-evidence-root", shared];
        await using var child = new RawToolChild(int.MaxValue, false, "2026-07-28", "conflicting", options);
        await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Check(child.Process.ExitCode == 1 && !Directory.Exists(root), "Conflicting options created evidence or started server");
    }

    static async Task BlockedEvidenceRoot()
    {
        string root = Path.Combine(Evidence, "blocked-root-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(root, "existing file");
        await using var child = new RawToolChild(int.MaxValue, false, "2026-07-28", "blocked-root", ["--wire-evidence-root", root]);
        await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Check(child.Process.ExitCode == 1 && File.ReadAllText(root) == "existing file", "Trace construction failure was hidden or replaced existing file");
    }
}
