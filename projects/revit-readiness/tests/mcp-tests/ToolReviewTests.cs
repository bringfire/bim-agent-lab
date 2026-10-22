using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitReadiness.Mcp;
using ModelContextProtocol.Server;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    // Review-derived cases: real executable + Windows pipe; synthetic executor only.
    static async Task ProductionEof(string revision, bool tracing, bool explicitCancel = false)
    {
        var descriptor = BridgeIdentity.CreateCurrent();
        var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(), new SystemBridgeClock());
        var admitted = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quiesced = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        endpoint.Start(context => { admitted.TrySetResult(context); return new DispatchTicket(response.Task, quiesced.Task); });
        try
        {
            await using var child = new RawToolChild(Environment.ProcessId, tracing, revision);
            await child.Connect();
            await child.Send(CallWire(2, BridgeLimits.GetContext, "{}", revision));
            var context = await admitted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var originalDeadline = context.Request.DeadlineUtc;
            if (explicitCancel)
                await child.Send("{\"jsonrpc\":\"2.0\",\"method\":\"notifications/cancelled\",\"params\":{\"requestId\":2}}");
            var elapsed = Stopwatch.StartNew();
            child.Process.StandardInput.Close();
            bool exited = false;
            try { await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); exited = true; }
            catch (TimeoutException) { }
            // Peer observation may follow process exit, but never release the executor to help it.
            for (int i = 0; exited && !context.IsCancellationRequested && i < 100; i++) await Task.Delay(10);
            bool guardRejected = false;
            try { context.ThrowIfInactive(); } catch (OperationCanceledException) { guardRejected = true; }
            var observation = new { revision, tracing, explicitCancel, exited,
                canceled = context.IsCancellationRequested, guardRejected, elapsedMs = elapsed.ElapsedMilliseconds,
                remainingMs = context.Budget.Remaining.TotalMilliseconds,
                responseHeld = !response.Task.IsCompleted, quiescedHeld = !quiesced.Task.IsCompleted,
                deadlineUnchanged = originalDeadline == context.Request.DeadlineUtc };
            child.Record("observation.json", JsonSerializer.Serialize(observation));
            Console.WriteLine("EOF_OBSERVATION " + JsonSerializer.Serialize(observation));
            Check(exited && child.Process.ExitCode == 0, "EOF did not naturally stop the production process before executor release");
            Check(observation.canceled && guardRejected && observation.remainingMs > 15000,
                "EOF did not invalidate final-read guard within the original unexpired budget");
            Check(observation.responseHeld && observation.quiescedHeld && observation.deadlineUnchanged, "Executor/budget control lost");
        }
        finally
        {
            response.TrySetCanceled(); quiesced.TrySetResult();
            await endpoint.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    static string Meta(string revision) => revision == "2026-07-28"
        ? ",\"_meta\":{\"io.modelcontextprotocol/protocolVersion\":\"2026-07-28\",\"io.modelcontextprotocol/clientInfo\":{\"name\":\"task6-review-test\",\"version\":\"1\"},\"io.modelcontextprotocol/clientCapabilities\":{}}" : "";
    static string CallWire(int id, string tool, string arguments, string revision) =>
        "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"method\":\"tools/call\",\"params\":{\"name\":\"" + tool + "\",\"arguments\":" + arguments + Meta(revision) + "}}";

    static async Task UnicodeArguments(string revision)
    {
        await using var child = new RawToolChild(int.MaxValue, true, revision);
        await child.Connect();
        int id = 2;
        var requestIds = new HashSet<string>();
        var errors = new List<string>();
        foreach (var (name, raw, expected) in new[] {
            ("high", "\"\\ud800\"", "InvalidRequest"), ("low", "\"\\udc00\"", "InvalidRequest"),
            ("wrong-type", "42", "InvalidRequest"), ("oversized", JsonSerializer.Serialize(new string('x', 70000)), "InvalidRequest"),
            ("ascii-after-invalid", "\"opaque\"", "BridgeUnavailable"),
            ("valid-pair-after-invalid", "\"\\ud83d\\ude00\"", "BridgeUnavailable") })
        {
            // Do not serialize the unpaired surrogate: send its original escape on the wire.
            await child.Send(CallWire(id, BridgeLimits.CheckInformation, "{\"contextToken\":" + raw + "}", revision));
            var reply = await child.Receive(id++);
            try
            {
                Check(!reply.TryGetProperty("error", out _), name + " escaped as JSON-RPC error");
                var result = reply.GetProperty("result");
                var data = result.GetProperty("structuredContent");
                Check(result.GetProperty("isError").GetBoolean() && data.GetProperty("code").GetString() == expected, name + " wrong domain error");
                var requestId = data.GetProperty("requestId").GetString()!;
                Check(Guid.TryParseExact(requestId, "N", out _) && requestIds.Add(requestId), name + " missing/duplicate request correlation");
                Check(data.GetProperty("retryable").GetBoolean() == (expected == "BridgeUnavailable") && !data.TryGetProperty("report", out _), name + " wrong failure metadata");
                using var text = JsonDocument.Parse(result.GetProperty("content")[0].GetProperty("text").GetString()!);
                Check(JsonElement.DeepEquals(text.RootElement, data), name + " text/structured mismatch");
            }
            catch (Exception error) { errors.Add(error.Message); }
        }
        child.Process.StandardInput.Close();
        await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Check(child.Process.ExitCode == 0, "Unicode cases terminated production process");
        Check(errors.Count == 0, string.Join("; ", errors));
    }

    sealed class RawToolChild : IAsyncDisposable
    {
        internal readonly Process Process;
        readonly string revision, folder;
        readonly Task<string> stderr;
        readonly List<string> sent = new(), received = new();
        internal RawToolChild(int pid, bool tracing, string revision, string label = "review", IReadOnlyList<string>? extraArgs = null)
        {
            this.revision = revision;
            folder = Path.Combine(Evidence, label + "-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
            var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "BimAgentLab.RevitReadiness.Mcp.exe")) {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("--revit-pid"); start.ArgumentList.Add(pid.ToString());
            if (tracing) { start.ArgumentList.Add("--wire-evidence"); start.ArgumentList.Add(Path.Combine(folder, "wire")); }
            if (extraArgs is not null) foreach (string arg in extraArgs) start.ArgumentList.Add(arg);
            Process = Process.Start(start)!; stderr = Process.StandardError.ReadToEndAsync();
        }
        internal void Record(string name, string text) => File.WriteAllText(Path.Combine(folder, name), text);
        internal async Task Send(string raw)
        { sent.Add(raw); await Process.StandardInput.WriteLineAsync(raw); await Process.StandardInput.FlushAsync(); }
        internal async Task<JsonElement> Receive(int id)
        {
            while (true)
            {
                var raw = await Process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5));
                Check(raw is not null, "Unexpected stdout EOF"); received.Add(raw!);
                using var document = JsonDocument.Parse(raw!);
                if (document.RootElement.TryGetProperty("id", out var actual) && actual.GetInt32() == id) return document.RootElement.Clone();
            }
        }
        internal async Task Connect()
        {
            if (revision == "2026-07-28")
            {
                await Send("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"server/discover\",\"params\":{" + Meta(revision)[1..] + "}}");
                Check((await Receive(1)).GetProperty("result").GetProperty("supportedVersions").EnumerateArray().Any(v => v.GetString() == revision), "Wrong discovery revision");
            }
            else
            {
                await Send(JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 1, method = "initialize", @params = new {
                    protocolVersion = revision, capabilities = new { }, clientInfo = new { name = "task6-review-test", version = "1" } } }));
                Check((await Receive(1)).GetProperty("result").GetProperty("protocolVersion").GetString() == revision, "Wrong initialization revision");
                await Send("{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}");
            }
        }
        public async ValueTask DisposeAsync()
        {
            // Watchdog only on failure; a passing observation requires natural exit before disposal.
            if (!Process.HasExited) { Process.Kill(true); await Process.WaitForExitAsync(); }
            var tail = await Process.StandardOutput.ReadToEndAsync();
            Record("sent.jsonl", string.Join("\n", sent) + "\n");
            Record("received.jsonl", string.Join("\n", received) + "\n" + tail);
            Record("stderr.txt", await stderr); Process.Dispose();
        }
    }

    static async Task FaultedInputCancelsHandler(bool tracing)
    {
        using var input = new FaultAfterRequestInput();
        using var output = new MemoryStream();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var canceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var watchdog = new CancellationTokenSource();
        var handlers = new McpServerHandlers { CallToolHandler = async (_, token) => {
            started.TrySetResult();
            try { await Task.Delay(Timeout.Infinite, token); }
            catch (OperationCanceledException) { canceled.TrySetResult(); throw; }
            throw new Exception("No result expected");
        } };
        Task run = McpHost.RunOwnedStreamsAsync(handlers, input, output,
            tracing ? Path.Combine(Evidence, "read-fault-" + Guid.NewGuid().ToString("N")) : null, watchdog.Token);
        Exception? observed = null;
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            input.Fail();
            try { await run.WaitAsync(TimeSpan.FromSeconds(3)); } catch (Exception error) { observed = error; }
            Check(observed is IOException && observed.ToString().Contains("review-input-fault"), "Input failure lost or shutdown waited for handler");
            Check(canceled.Task.IsCompleted, "Transport failure did not cancel active handler");
        }
        finally { watchdog.Cancel(); try { await run.WaitAsync(TimeSpan.FromSeconds(5)); } catch (IOException) { } }
    }
    sealed class FaultAfterRequestInput : WrappedStream
    {
        readonly byte[] request = Encoding.UTF8.GetBytes(CallWire(1, BridgeLimits.GetContext, "{}", "2026-07-28") + "\n");
        readonly TaskCompletionSource<int> end = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int offset;
        internal FaultAfterRequestInput() : base(Stream.Null, true) { }
        internal void Fail() => end.TrySetException(new IOException("review-input-fault"));
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            int count = Math.Min(buffer.Length, request.Length - offset);
            if (count == 0) return new(end.Task);
            request.AsMemory(offset, count).CopyTo(buffer); offset += count; return ValueTask.FromResult(count);
        }
    }
}
