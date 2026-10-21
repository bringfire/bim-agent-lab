using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    static async Task PipeTransportTests()
    {
        await Test("Two real pipe clients preserve separate request and session identities", async () =>
        {
            var descriptor = ManualDescriptor(); var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("duplex")), new SystemBridgeClock());
            int submitted = 0;
            endpoint.Start(c => { Interlocked.Increment(ref submitted); return Ticket(c, descriptor.BridgeSessionId); });
            try
            {
                var client = new PipeBridgeClient(descriptor);
                using var a = LiveRequest("client-一"); using var b = LiveRequest("client-β");
                var results = await Task.WhenAll(client.SendAsync(a.Context), client.SendAsync(b.Context));
                Equal("client-一", results[0].RequestId); Equal("client-β", results[1].RequestId);
                foreach (var result in results) { Equal(descriptor.BridgeSessionId, result.BridgeSessionId); Equal(Report, result.ResultJson); }
                Equal(2, submitted);
            }
            finally { await endpoint.StopAsync(); }
        });
        foreach (string mode in new[] { "no-bytes", "partial-header", "partial-body", "trickle" })
            await Test("All eight " + mode + " slots expire once and admit a valid successor", async () =>
            {
                var descriptor = ManualDescriptor(); var events = new ConcurrentQueue<PipeEvent>(); int submitted = 0;
                var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch(mode)), new SystemBridgeClock(), events.Enqueue);
                endpoint.Start(c => { Interlocked.Increment(ref submitted); return Ticket(c, descriptor.BridgeSessionId); });
                var pipes = new NamedPipeClientStream[8]; using var trickling = new CancellationTokenSource();
                Task trickle = Task.CompletedTask;
                try
                {
                    for (int i = 0; i < pipes.Length; i++) { pipes[i] = RawClient(descriptor); await pipes[i].ConnectAsync(2000); }
                    await Until(() => events.Count(e => e.Stage == "Accepted") == 8);
                    using (var ninth = RawClient(descriptor))
                    {
                        var rejected = await ThrowsAsync<Exception>(() => ninth.ConnectAsync(200));
                        Check(rejected is TimeoutException || (rejected is IOException && rejected.HResult == unchecked((int)0x80070079)), "Unexpected ninth-client failure: " + rejected);
                        Check(!ninth.IsConnected, "Ninth connection bypassed the bound");
                    }
                    Equal(8, events.Count(e => e.Stage == "Accepted"));
                    if (mode == "partial-header") foreach (var pipe in pipes) await pipe.WriteAsync(new byte[] { 10, 0 }, 0, 2);
                    if (mode == "partial-body" || mode == "trickle") foreach (var pipe in pipes) await pipe.WriteAsync(new byte[] { 100, 0, 0, 0, 123 }, 0, 5);
                    if (mode == "trickle") trickle = Task.Run(async () =>
                    {
                        try { while (true) { await Task.Delay(300, trickling.Token); foreach (var pipe in pipes) await pipe.WriteAsync(new byte[] { 32 }, 0, 1, trickling.Token); } }
                        catch (Exception e) when (e is IOException || e is OperationCanceledException || e is ObjectDisposedException) { }
                    });
                    await Until(() => events.Count(e => e.Stage == "Closed") == 8, 6500);
                    Equal(0, submitted);
                    foreach (var accepted in events.Where(e => e.Stage == "Accepted"))
                    {
                        var closed = events.Single(e => e.Stage == "Closed" && e.ConnectionId == accepted.ConnectionId);
                        double seconds = (closed.Timestamp - accepted.Timestamp) / (double)Stopwatch.Frequency;
                        Check(seconds >= 4.8 && seconds <= 6.0, "Frame cleanup outside five-second bound: " + seconds);
                        Console.WriteLine($"FRAME_TIMING mode={mode} connection={accepted.ConnectionId} accepted={accepted.Timestamp} closed={closed.Timestamp} frequency={Stopwatch.Frequency} seconds={seconds:F6}");
                    }
                    using var next = LiveRequest("after-" + mode);
                    Equal(next.Context.Request.RequestId, (await new PipeBridgeClient(descriptor).SendAsync(next.Context)).RequestId);
                    Equal(1, submitted);
                }
                finally { trickling.Cancel(); foreach (var pipe in pipes) pipe?.Dispose(); await trickle; await endpoint.StopAsync(); }
            });
        await Test("Disconnect cancels work and frees connection before callback quiescence", async () =>
        {
            var descriptor = ManualDescriptor(); var events = new ConcurrentQueue<PipeEvent>();
            var ready = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
            var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("cancel")), new SystemBridgeClock(), events.Enqueue);
            endpoint.Start(c => { ready.TrySetResult(c); return new DispatchTicket(response.Task, quiesced.Task); });
            using var cancel = new CancellationTokenSource(); using var request = LiveRequest("cancel", cancel.Token);
            try
            {
                var pending = new PipeBridgeClient(descriptor).SendAsync(request.Context);
                var context = await Within(ready.Task); cancel.Cancel();
                await ThrowsAsync<OperationCanceledException>(async () => await Within(pending));
                await Until(() => context.Cancellation.IsCancellationRequested && events.Any(e => e.Stage == "Closed"));
                Check(!quiesced.Task.IsCompleted, "Connection cleanup waited for callback");
                // WaitHandle access throws after CTS disposal; cancellation alone must not dispose it.
                Check(context.Cancellation.WaitHandle.WaitOne(0), "Borrowed lifetime lost its cancellation signal");
                await Within(endpoint.StopAsync());
                quiesced.TrySetResult(true); response.TrySetResult(Reply(context, descriptor.BridgeSessionId));
                await Until(() => { try { _ = context.Cancellation.WaitHandle; return false; } catch (ObjectDisposedException) { return true; } });
            }
            finally { quiesced.TrySetResult(true); await endpoint.StopAsync(); }
        });
        await Test("Stop closes idle and partial receive connections with no dispatch", async () =>
        {
            var descriptor = ManualDescriptor(); var events = new ConcurrentQueue<PipeEvent>(); int submitted = 0;
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("stop")), new SystemBridgeClock(), events.Enqueue);
            endpoint.Start(c => { submitted++; return Ticket(c, descriptor.BridgeSessionId); });
            using var idle = RawClient(descriptor); using var partial = RawClient(descriptor);
            await idle.ConnectAsync(2000); await partial.ConnectAsync(2000); await partial.WriteAsync(new byte[] { 1 }, 0, 1);
            await Until(() => events.Count(e => e.Stage == "Accepted") == 2);
            await Within(endpoint.StopAsync()); await Within(endpoint.StopAsync());
            Equal(0, submitted); Equal(2, events.Count(e => e.Stage == "Closed"));
        });
        await Test("Lazy provider retries before binding and never follows a replacement session", async () =>
        {
            var available = new ConcurrentQueue<BridgeSessionDescriptor>(); int resolves = 0;
            var catalog = new SessionCatalog(() => { Interlocked.Increment(ref resolves); return available.ToArray(); });
            var provider = new BridgeClientProvider(catalog, null, new SystemBridgeClock()); Equal(0, resolves);
            using (var request = LiveRequest("missing"))
                Equal("BridgeUnavailable", (await ThrowsAsync<BridgeException>(() => provider.SendAsync(request.Context))).Error.Code);
            var descriptor = ManualDescriptor(); var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("binding")), new SystemBridgeClock());
            endpoint.Start(c => Ticket(c, descriptor.BridgeSessionId)); available.Enqueue(descriptor);
            try
            {
                using var a = LiveRequest("bind-a"); using var b = LiveRequest("bind-b");
                await Task.WhenAll(provider.SendAsync(a.Context), provider.SendAsync(b.Context)); Equal(2, resolves);
                await endpoint.StopAsync(); available.TryDequeue(out _);
                var replacement = ManualDescriptor(); available.Enqueue(replacement);
                using var next = LiveRequest("no-replay");
                Equal("BridgeUnavailable", (await ThrowsAsync<BridgeException>(() => provider.SendAsync(next.Context))).Error.Code);
                Equal(2, resolves);
                using var canceled = new CancellationTokenSource(); using var canceledRequest = LiveRequest("canceled", canceled.Token); canceled.Cancel();
                await ThrowsAsync<OperationCanceledException>(() => new BridgeClientProvider(catalog, null, new SystemBridgeClock()).SendAsync(canceledRequest.Context));
                Equal(2, resolves);
            }
            finally { await endpoint.StopAsync(); }
        });
        await Test("net48 child endpoint serves simultaneous clients and large Unicode results, then removes its descriptor", async () =>
        {
            string host = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "net48", "BimAgentLab.Bridge.Tests.exe"));
            string directory = Scratch("child");
            using var process = new Process { StartInfo = new ProcessStartInfo(host, "--pipe-host \"" + directory + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true } };
            Check(process.Start(), "Child process did not start");
            var errors = process.StandardError.ReadToEndAsync();
            try
            {
                string? ready = await Within(process.StandardOutput.ReadLineAsync());
                Check(ready != null && ready.StartsWith("READY ", StringComparison.Ordinal), "Child endpoint startup failed: " + ready);
                var descriptor = new SessionCatalog(directory).Resolve(process.Id);
                using var request = LiveRequest("cross-runtime"); using var large = LiveRequest("cross-runtime-large");
                var results = await Task.WhenAll(new PipeBridgeClient(descriptor).SendAsync(request.Context), new PipeBridgeClient(descriptor).SendAsync(large.Context));
                Equal(Report, results[0].ResultJson); Equal(LargeReport, results[1].ResultJson);
                Equal("cross-runtime", results[0].RequestId); Equal("cross-runtime-large", results[1].RequestId);
                Check(Utf8(results[1].ResultJson!).Length > 8192, "Cross-runtime report must exceed the pipe buffer");
                foreach (var result in results) Equal(descriptor.BridgeSessionId, result.BridgeSessionId);
                process.StandardInput.WriteLine("stop"); process.StandardInput.Flush();
                await Until(() => process.HasExited, 5000); Equal(0, process.ExitCode); Equal(0, Directory.GetFiles(directory).Length);
                Equal("", await errors);
            }
            finally { if (!process.HasExited) { process.StandardInput.WriteLine("stop"); if (!process.WaitForExit(5000)) process.Kill(); } }
        });
        await PipeFailureTests();
    }
    static BridgeResponse Reply(BridgeExecutionContext c, string session) => new BridgeResponse
    { ContractVersion = 1, RequestId = c.Request.RequestId, BridgeSessionId = session, ChecklistSha256 = new string('a', 64), ResultJson = Report };
    static DispatchTicket Ticket(BridgeExecutionContext c, string session) => new DispatchTicket(Task.FromResult(Reply(c, session)), Task.CompletedTask);
    static RequestLifetime LiveRequest(string id, CancellationToken cancellation = default)
    {
        var budget = RequestBudget.Admit(new SystemBridgeClock());
        var request = Request(budget.DeadlineUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture)); request.RequestId = id;
        return new RequestLifetime(request, budget, cancellation, CancellationToken.None);
    }
    static NamedPipeClientStream RawClient(BridgeSessionDescriptor descriptor) => new NamedPipeClientStream(".", descriptor.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    static async Task Until(Func<bool> condition, int milliseconds = 3000)
    { var timer = Stopwatch.StartNew(); while (!condition()) { if (timer.ElapsedMilliseconds > milliseconds) throw new TimeoutException("Condition did not settle."); await Task.Delay(10); } }
    static async Task<T> Within<T>(Task<T> task)
    { Check(await Task.WhenAny(task, Task.Delay(5000)) == task, "Operation did not settle"); return await task; }
    static async Task Within(Task task)
    { Check(await Task.WhenAny(task, Task.Delay(5000)) == task, "Operation did not settle"); await task; }
}
