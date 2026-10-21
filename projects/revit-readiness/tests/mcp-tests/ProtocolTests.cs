using System.Text.Json;
using ModelContextProtocol.Client;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    static int failures;
    static string Probe = "";
    static string Evidence = "";

    static async Task<int> Main(string[] args)
    {
        if (args[0] == "--shutdown-child") return await ShutdownChild(args);
        Probe = Path.GetFullPath(args[0]);
        Evidence = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(Evidence);
        await Test("Pinned SDK discovers only the synthetic probe and preserves structured success/failure", ProbeExchange);
        await Test("Trace preserves fragmented Unicode bytes and stream ownership", TraceBytes);
        await Test("Memory read cancellation retains pending buffer and suppresses late evidence", () => CancelPendingRead(false));
        await Test("Array read cancellation retains pending buffer and suppresses late evidence", () => CancelPendingRead(true));
        await Test("Trace size bound fails visibly without silent truncation", TraceLimit);
        await Test("Host releases each owned stream once", () => OwnedConstructionCleanup(false));
        await Test("Partial trace construction releases streams and preserves existing evidence", () => OwnedConstructionCleanup(true));
        foreach (bool tracing in new[] { false, true })
            foreach (string scenario in new[] { "idle", "partial", "pending" })
                await Test($"Windows open-stdin shutdown: trace={tracing}, {scenario}", () => OpenStdinShutdown(tracing, scenario));
        await Test("Windows open-stdin shutdown after trace initialization failure", () => OpenStdinShutdown(true, "trace-failure"));
        return failures == 0 ? 0 : 1;
    }

    static async Task Test(string name, Func<Task> test)
    {
        try { await test(); Console.WriteLine("PASS " + name); }
        catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + ": " + e.GetType().Name + " " + e.Message); }
    }
    internal static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    static async Task ProbeExchange()
    {
        var trace = Path.Combine(Evidence, "sdk-" + Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = Probe, Arguments = ["--wire-evidence", trace],
            Name = "test-only-protocol-probe"
        });
        await using (var client = await McpClient.CreateAsync(transport,
            new McpClientOptions { ProtocolVersion = "2026-07-28" }, cancellationToken: timeout.Token))
        {
            Check(client.NegotiatedProtocolVersion == "2026-07-28", "Expected exact dated protocol");
            var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
            Check(tools.Count == 1 && tools[0].Name == "lab_protocol_probe", "Expected only synthetic discovery");
            var success = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = false }, cancellationToken: timeout.Token);
            Check(success.IsError != true && success.StructuredContent!.Value.GetProperty("synthetic").GetBoolean(), "Missing synthetic success");
            var failure = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = true }, cancellationToken: timeout.Token);
            Check(failure.IsError == true && failure.StructuredContent!.Value.GetProperty("code").GetString() == "ProbeFailure", "Missing structured failure");
        }
        // Independent traffic assertions; SDK owns every protocol exchange above.
        trace = Directory.GetDirectories(trace).Single();
        foreach (var path in new[] { "client-to-server.jsonl", "server-to-client.jsonl" })
        {
            var lines = File.ReadAllLines(Path.Combine(trace, path));
            Check(lines.Length >= 3, "Missing wire evidence");
            foreach (var line in lines) using (var doc = JsonDocument.Parse(line))
                Check(doc.RootElement.GetProperty("jsonrpc").GetString() == "2.0", "Non-protocol stdout");
        }
        Console.WriteLine("EVIDENCE " + trace);
    }
}
