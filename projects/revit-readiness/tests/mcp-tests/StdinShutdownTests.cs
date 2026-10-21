using System.Diagnostics;
using System.Text.Json;
using BimAgentLab.RevitReadiness.Mcp;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    static async Task<int> ShutdownChild(string[] args)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var stopEvent = EventWaitHandle.OpenExisting(args[1]);
        using var stop = new CancellationTokenSource();
        // Background wait has no resource ownership beyond this child process.
        _ = Task.Run(() => { stopEvent.WaitOne(); stop.Cancel(); });
        var handlers = new McpServerHandlers
        {
            ListToolsHandler = (_, _) => ValueTask.FromResult(new ListToolsResult()),
            CallToolHandler = async (_, token) =>
            {
                Console.Error.WriteLine("CALL_STARTED");
                await Task.Delay(Timeout.Infinite, token);
                return new CallToolResult();
            }
        };
        Console.Error.WriteLine("READY");
        try
        {
            await McpHost.RunStdioAsync(handlers, args[2] == "off" ? null : args[2], stop.Token);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.GetType().Name); return 2; }
    }

    static async Task OpenStdinShutdown(bool tracing, string scenario)
    {
        string eventName = "Local\\BAL-Mcp-Test-" + Guid.NewGuid().ToString("N");
        using var stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);
        var folder = Path.Combine(Evidence, "shutdown-" + Guid.NewGuid().ToString("N"));
        if (scenario == "trace-failure")
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "server-to-client.jsonl"), "existing evidence must survive");
        }
        var start = new ProcessStartInfo(Environment.ProcessPath!)
        { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in new[] { "--shutdown-child", eventName, tracing ? folder : "off" }) start.ArgumentList.Add(arg);
        using var child = Process.Start(start)!;
        McpClient? client = null;
        Task<CallToolResult>? pending = null;
        Task<string>? stdout = null;
        try
        {
            Check(await child.StandardError.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)) == "READY", "Child did not become ready");
            if (scenario == "partial")
            { await child.StandardInput.WriteAsync("{\"jsonrpc\":"); await child.StandardInput.FlushAsync(); }
            if (scenario == "pending")
            {
                client = await McpClient.CreateAsync(new StreamClientTransport(child.StandardInput.BaseStream, child.StandardOutput.BaseStream),
                    new McpClientOptions { ProtocolVersion = "2026-07-28" });
                pending = client.CallToolAsync("wait", new Dictionary<string, object?>()).AsTask();
                Check(await child.StandardError.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)) == "CALL_STARTED", "No pending tool");
            }
            else stdout = child.StandardOutput.ReadToEndAsync();
            var clock = Stopwatch.StartNew();
            stopEvent.Set();
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Check(clock.Elapsed < TimeSpan.FromSeconds(5), "Shutdown exceeded bound");
            Check(child.ExitCode == (scenario == "trace-failure" ? 2 : 0), "Unexpected child exit code");
            // Parent has not closed stdin, disposed the client, sent EOF or terminated the child.
            Check(child.StandardInput.BaseStream.CanWrite, "Parent closed stdin before child exit");
            if (pending is not null)
            {
                bool success = false;
                try { success = (await pending.WaitAsync(TimeSpan.FromSeconds(2))).IsError != true; }
                catch (Exception error) when (error is not TimeoutException) { }
                Check(!success, "Pending canceled call published success");
            }
            else Check(await stdout! == "", "Shutdown emitted non-protocol stdout");
        }
        finally
        {
            if (!child.HasExited) { child.Kill(entireProcessTree: true); await child.WaitForExitAsync(); }
            // Kill is watchdog cleanup for a FAILED test, never the successful exit path.
            if (client is not null) await client.DisposeAsync();
        }
    }

    static async Task OwnedConstructionCleanup(bool failDuringSetup)
    {
        var input = new CountedStream(); var output = new CountedStream();
        var folder = Path.Combine(Evidence, "ownership-" + Guid.NewGuid().ToString("N"));
        if (failDuringSetup)
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "server-to-client.jsonl"), "preserve");
        }
        bool failed = false;
        try { await McpHost.RunOwnedStreamsAsync(new McpServerHandlers(), input, output, failDuringSetup ? folder : null, CancellationToken.None); }
        catch (IOException) { failed = true; }
        Check(failed == failDuringSetup, "Unexpected construction outcome");
        Check(input.Disposals == 1 && output.Disposals == 1, $"Resource disposed {input.Disposals}/{output.Disposals} times instead of once");
        if (failDuringSetup)
        {
            using var reopened = new FileStream(Path.Combine(folder, "client-to-server.jsonl"), FileMode.Open, FileAccess.Write, FileShare.None);
            Check(File.ReadAllText(Path.Combine(folder, "server-to-client.jsonl")) == "preserve", "Existing evidence overwritten");
        }
    }
    sealed class CountedStream : MemoryStream
    {
        internal int Disposals;
        protected override void Dispose(bool disposing) { if (disposing) Disposals++; base.Dispose(disposing); }
    }
}
