using System.Diagnostics;
using System.Text.Json;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    static async Task<int> InterruptDriverTests(string[] args)
    {
        Evidence = Path.GetFullPath(args[1]); Directory.CreateDirectory(Evidence);
        foreach (string revision in new[] { "2025-06-18", "2025-11-25", "2026-07-28" })
            foreach (string mode in new[] { "cancel", "eof" })
                await Test($"SDK interruption driver: {mode} {revision}", () => InterruptDriver(mode, revision));
        Console.WriteLine($"Interruption driver: {6 - failures} passed; {failures} failed.");
        return failures == 0 ? 0 : 1;
    }

    static async Task InterruptDriver(string mode, string revision)
    {
        string folder = Path.Combine(Evidence, "driver-" + mode + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string capture = Path.Combine(folder, "capture"), interrupt = Path.Combine(folder, "interrupt"), finish = Path.Combine(folder, "finish");
        string project = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        string client = Path.Combine(project, "tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe");
        var descriptor = BridgeIdentity.CreateCurrent();
        var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(), new SystemBridgeClock());
        var admitted = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quiesced = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        endpoint.Start(context => {
            if (context.Request.Operation == BridgeLimits.GetContext)
            {
                var value = SyntheticResponse(context); value.BridgeSessionId = descriptor.BridgeSessionId;
                return new DispatchTicket(Task.FromResult(value), Task.CompletedTask);
            }
            admitted.TrySetResult(context);
            return new DispatchTicket(response.Task, quiesced.Task);
        });
        var start = new ProcessStartInfo(client) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string item in new[] { "--scenario", "interrupt", "--interrupt-mode", mode, "--interrupt-file", interrupt,
            "--finish-file", finish, "--server", Path.Combine(AppContext.BaseDirectory, "BimAgentLab.RevitReadiness.Mcp.exe"),
            "--revit-pid", Environment.ProcessId.ToString(), "--evidence", capture, "--protocol", revision }) start.ArgumentList.Add(item);
        using var child = Process.Start(start)!;
        var output = child.StandardOutput.ReadToEndAsync(); var errors = child.StandardError.ReadToEndAsync();
        try
        {
            var first = await Task.WhenAny(admitted.Task, child.WaitForExitAsync(), Task.Delay(8000));
            Check(admitted.Task.IsCompleted, "Driver did not admit a real pipe request: " + (child.HasExited ? await errors : "timeout"));
            var context = await admitted.Task;
            string originalDeadline = context.Request.DeadlineUtc;
            File.WriteAllText(interrupt, "interrupt after actual admission");
            string interrupted = Path.Combine(capture, "interrupted.json");
            for (int i = 0; i < 400 && !File.Exists(interrupted) && !child.HasExited; i++) await Task.Delay(20);
            Check(File.Exists(interrupted), "No completed interruption observation: " + (child.HasExited ? await errors : "timeout"));
            for (int i = 0; i < 100 && !context.IsCancellationRequested; i++) await Task.Delay(10);
            Check(context.IsCancellationRequested && !context.Budget.IsExpired && context.Budget.Remaining > TimeSpan.FromSeconds(15), "Must cancel before deadline");
            bool guardRejected = false;
            try { context.ThrowIfInactive(); } catch (OperationCanceledException) { guardRejected = true; }
            Check(guardRejected && originalDeadline == context.Request.DeadlineUtc, "Guard or budget changed");
            Check(!response.Task.IsCompleted && !quiesced.Task.IsCompleted && !child.HasExited, "Borrowed executor/finish gate not held");
            using var observed = JsonDocument.Parse(File.ReadAllText(interrupted));
            Check(observed.RootElement.GetProperty("serverExited").GetBoolean() == (mode == "eof"), "Wrong server lifetime boundary");
            File.WriteAllText(Path.Combine(folder, "held-observation.json"), JsonSerializer.Serialize(new {
                mode, revision, context.Request.RequestId, originalDeadline, canceled = context.IsCancellationRequested,
                expired = context.Budget.IsExpired, guardRejected, responseHeld = !response.Task.IsCompleted,
                quiescedHeld = !quiesced.Task.IsCompleted, remainingMilliseconds = context.Budget.Remaining.TotalMilliseconds }));
            response.TrySetCanceled(); quiesced.TrySetResult();
            File.WriteAllText(finish, "executor released");
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
            Check(child.ExitCode == 0, "Driver failed: " + await errors);
            using var complete = JsonDocument.Parse(File.ReadAllText(Path.Combine(capture, "complete.json")));
            Check(complete.RootElement.GetProperty("pass").GetBoolean(), "Failed capture marked incomplete");
            var incoming = File.ReadAllLines(Path.Combine(capture, "wire/client-to-server.jsonl"));
            Check(incoming.Any(s => s.Contains("notifications/cancelled")) == (mode == "cancel"), "EOF must not be preceded by SDK cancellation");
        }
        finally
        {
            response.TrySetCanceled(); quiesced.TrySetResult();
            File.WriteAllText(finish, "cleanup");
            if (!child.HasExited) { child.Kill(entireProcessTree: true); await child.WaitForExitAsync(); }
            File.WriteAllText(Path.Combine(folder, "driver-stdout.txt"), await output);
            File.WriteAllText(Path.Combine(folder, "driver-stderr.txt"), await errors);
            await endpoint.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));
        }
    }
}
