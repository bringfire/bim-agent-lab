using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    // Diagnostic characterization, deliberately separate from product acceptance.
    // No Revit process/API, installed files, or desktop configuration are used.
    static async Task<int> SdkCancellationDiagnosis(string[] args)
    {
        if (args.Length != 3 || args[2] is not ("--require-propagation" or "--characterize"))
            throw new ArgumentException("--sdk-cancel-diagnosis <new-evidence-directory> --require-propagation|--characterize");
        Evidence = Path.GetFullPath(args[1]);
        if (Directory.Exists(Evidence)) throw new ArgumentException("Evidence directory must be new.");
        Directory.CreateDirectory(Evidence);
        Console.WriteLine(args[2] == "--characterize"
            ? "CHARACTERIZATION ONLY: token cases must reproduce the known missing notification; this is not a fix."
            : "ACCEPTANCE PROBE: token cancellation must propagate remotely (expected to fail on the pinned SDK).");
        foreach (var protocol in new[] { "2025-06-18", "2025-11-25", "2026-07-28" })
            foreach (var mode in new[] { "token", "explicit" })
                await Test($"SDK cancellation {protocol} {mode}", () => SdkCancellationExchange(protocol, mode, args[2] == "--require-propagation"));
        await Test("Cancellation callback order mechanism", CancellationOrderMechanism);
        return failures == 0 ? 0 : 1;
    }

    static async Task SdkCancellationExchange(string protocol, string mode, bool requirePropagation)
    {
        string folder = Path.Combine(Evidence, protocol + "-" + mode);
        Directory.CreateDirectory(folder);
        string wire = Path.Combine(folder, "wire");
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
        var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "BimAgentLab.RevitReadiness.Mcp.exe")) {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in new[] { "--revit-pid", Environment.ProcessId.ToString(), "--wire-evidence", wire }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var stderr = process.StandardError.ReadToEndAsync();
        using var watchdog = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var callCancellation = new CancellationTokenSource();
        McpClient? client = null;
        bool forced = false;
        try
        {
            client = await McpClient.CreateAsync(new StreamClientTransport(process.StandardInput.BaseStream, process.StandardOutput.BaseStream),
                new McpClientOptions { ProtocolVersion = protocol }, cancellationToken: watchdog.Token);
            Check(client.NegotiatedProtocolVersion == protocol, "Unexpected negotiation");
            var contextResult = await client.CallToolAsync("revit_get_readiness_context", new Dictionary<string, object?>(), cancellationToken: watchdog.Token);
            Check(contextResult.IsError == false, "Context failed");
            var token = contextResult.StructuredContent!.Value.GetProperty("contextToken").GetString();
            var pending = client.CallToolAsync("revit_check_required_type_information",
                new Dictionary<string, object?> { ["contextToken"] = token }, cancellationToken: callCancellation.Token).AsTask();
            var context = await admitted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            // Admission proves the request reached the real endpoint. Keep the connection,
            // handler, executor and original budget alive throughout the observation.
            await Task.Delay(50, watchdog.Token);
            string originalDeadline = context.Request.DeadlineUtc;
            string[] ReadWire()
            {
                using var file = new FileStream(Path.Combine(wire, "client-to-server.jsonl"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(file);
                return reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            }
            var checkMessage = ReadWire()
                .Select(line => JsonSerializer.Deserialize<JsonRpcMessage>(line, McpJsonUtilities.DefaultOptions))
                .OfType<JsonRpcRequest>().Last(r => r.Method == RequestMethods.ToolsCall);
            async Task SendExplicit() => await client.SendMessageAsync(new JsonRpcNotification {
                Method = NotificationMethods.CancelledNotification,
                Params = JsonSerializer.SerializeToNode(new CancelledNotificationParams { RequestId = checkMessage.Id }, McpJsonUtilities.DefaultOptions)
            }, watchdog.Token);
            DateTime stimulusUtc = DateTime.UtcNow;
            if (mode == "token") callCancellation.Cancel(); else await SendExplicit();
            bool localCanceled = false;
            try { await pending.WaitAsync(TimeSpan.FromSeconds(3)); }
            catch (OperationCanceledException) { localCanceled = true; }
            await Task.Delay(750, watchdog.Token);
            int Notifications() => ReadWire()
                .Count(line => line.Contains("\"notifications/cancelled\"", StringComparison.Ordinal));
            int notifications = Notifications();
            bool remoteCanceled = context.IsCancellationRequested;
            bool guardRejected = false;
            try { context.ThrowIfInactive(); } catch (OperationCanceledException) { guardRejected = true; }
            bool held = !response.Task.IsCompleted && !quiesced.Task.IsCompleted;
            bool connected = !process.HasExited;
            double remaining = context.Budget.Remaining.TotalMilliseconds;
            DateTime observedUtc = DateTime.UtcNow;
            File.WriteAllLines(Path.Combine(folder, "wire-before-rescue.jsonl"), ReadWire());
            // Rescue token-only attempts with an explicit SDK notification *after* recording
            // the observation. The watchdog, EOF and executor release cannot explain it.
            bool rescueSent = mode == "token" && notifications == 0 && !remoteCanceled;
            if (rescueSent) await SendExplicit();
            for (int i = 0; i < 100 && !context.IsCancellationRequested; i++) await Task.Delay(10, watchdog.Token);
            bool rescued = context.IsCancellationRequested;
            bool rescueGuardRejected = false;
            try { context.ThrowIfInactive(); } catch (OperationCanceledException) { rescueGuardRejected = true; }
            var followup = await client.ListToolsAsync(cancellationToken: watchdog.Token);
            File.WriteAllText(Path.Combine(folder, "observation.json"), JsonSerializer.Serialize(new {
                protocol, mode, stimulusUtc, observedUtc, rpcRequestId = checkMessage.Id.ToString(),
                localCanceled, notificationsBeforeRescue = notifications, remoteCanceledBeforeRescue = remoteCanceled,
                guardRejectedBeforeRescue = guardRejected, connected, held, remainingMilliseconds = remaining,
                originalDeadline, finalDeadline = context.Request.DeadlineUtc,
                rescueSent, rescued, rescueGuardRejected, finalNotifications = Notifications(), followupTools = followup.Count,
                sdkAssembly = typeof(McpClient).Assembly.FullName, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            }, new JsonSerializerOptions { WriteIndented = true }));
            Check(localCanceled && held && connected && remaining > 15000 && originalDeadline == context.Request.DeadlineUtc,
                "Invalid cancellation observation boundary");
            Check(rescued && rescueGuardRejected && followup.Count == 2 && Notifications() == 1, "Explicit cancellation/recovery failed");
            bool expected = mode == "explicit" || requirePropagation;
            Check(remoteCanceled == expected && guardRejected == expected && notifications == (expected ? 1 : 0),
                $"Expected remote propagation={expected}; actual remote={remoteCanceled}, guard={guardRejected}, wire notifications={notifications}");
        }
        finally
        {
            response.TrySetCanceled(); quiesced.TrySetResult();
            process.StandardInput.Close();
            try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException) { forced = true; process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            if (client is not null) await client.DisposeAsync();
            await endpoint.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));
            File.WriteAllText(Path.Combine(folder, "server-stderr.txt"), await stderr);
            File.WriteAllText(Path.Combine(folder, "cleanup.json"), JsonSerializer.Serialize(new { forced, process.ExitCode }));
        }
        Check(!forced && process.ExitCode == 0, "Server did not shut down naturally");
    }

    static async Task CancellationOrderMechanism()
    {
        var observations = new List<object>();
        foreach (bool cancelWaitSeparately in new[] { true, false })
        {
            using var cts = new CancellationTokenSource();
            var response = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var events = new System.Collections.Concurrent.ConcurrentQueue<string>();
            async Task Wait()
            {
                var registration = cts.Token.Register(() => { events.Enqueue("send-callback"); response.TrySetCanceled(); });
                try
                {
                    try { await (cancelWaitSeparately ? response.Task.WaitAsync(cts.Token) : response.Task).ConfigureAwait(false); }
                    finally { events.Enqueue("wait-unwound"); }
                }
                finally { registration.Dispose(); events.Enqueue("registration-disposed"); }
            }
            var pending = Wait();
            events.Enqueue("cancel-enter");
            cts.Cancel();
            events.Enqueue("cancel-return");
            try { await pending; } catch (OperationCanceledException) { }
            var ordered = events.ToArray();
            observations.Add(new { cancelWaitSeparately, events = ordered });
            Check(ordered.Contains("send-callback") == !cancelWaitSeparately, "Callback ordering hypothesis did not reproduce");
        }
        File.WriteAllText(Path.Combine(Evidence, "mechanism.json"), JsonSerializer.Serialize(observations, new JsonSerializerOptions { WriteIndented = true }));
    }
}
