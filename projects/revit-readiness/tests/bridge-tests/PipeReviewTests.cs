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
    static async Task PipeReviewTests()
    {
        await Test("Client exit before identity validation restores all eight slots and permits shutdown and restart", async () =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("peer-exit");
            var catalog = new SessionCatalog(directory); var events = new ConcurrentQueue<PipeEvent>();
            var accepted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var resume = new ManualResetEventSlim(); int barrierExpired = 0, submitted = 0;
            var endpoint = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock(), e =>
            {
                events.Enqueue(e);
                if (e.Stage == "Accepted" && e.ConnectionId == 1)
                {
                    accepted.TrySetResult(true);
                    if (!resume.Wait(5000)) Interlocked.Exchange(ref barrierExpired, 1);
                }
            });
            endpoint.Start(c => { Interlocked.Increment(ref submitted); return Ticket(c, descriptor.BridgeSessionId); });
            string host = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BimAgentLab.Bridge.Tests.exe");
            using var child = new Process { StartInfo = new ProcessStartInfo(host, "--pipe-connect-exit " + descriptor.PipeName)
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true } };
            Check(child.Start(), "Peer test process did not start"); var errors = child.StandardError.ReadToEndAsync();
            var held = new NamedPipeClientStream[8];
            try
            {
                Equal("CONNECTED", await Within(child.StandardOutput.ReadLineAsync()));
                await Within(accepted.Task);
                child.StandardInput.WriteLine("exit"); child.StandardInput.Flush();
                await Until(() => child.HasExited); Equal(0, child.ExitCode); Equal("", await errors);
                Console.WriteLine("PEER_EXIT exited before identity validation; runtime=" + Environment.Version);
                resume.Set(); Equal(0, barrierExpired);
                await Until(() => events.Any(e => e.Stage == "Closed" && e.ConnectionId == 1));
                Equal(0, submitted);
                // Hold all eight concurrently: a single later successful call can hide a lost worker.
                for (int i = 0; i < held.Length; i++) { held[i] = RawClient(descriptor); await held[i].ConnectAsync(1000); }
                await Until(() => events.Count(e => e.Stage == "Accepted") == 9);
                Equal(0, submitted); Check(!events.Any(e => e.Stage == "ListenerFailed"), "Peer exit stopped the listener");
                foreach (var pipe in held) pipe.Dispose();
                await Until(() => events.Count(e => e.Stage == "Closed") == 9);
                using var request = LiveRequest("after-peer-exit");
                Equal(request.Context.Request.RequestId, (await new PipeBridgeClient(descriptor).SendAsync(request.Context)).RequestId);
                Equal(1, submitted);
                await Within(endpoint.StopAsync()); await Within(endpoint.StopAsync());
                Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Peer exit left an owned descriptor");
                await VerifyReplacement(catalog);
                Console.WriteLine("PEER_EXIT recoveredSlots=8 submissionsBeforeValidRequest=0 stops=2 replacementSelected=true");
            }
            finally
            {
                resume.Set(); foreach (var pipe in held) pipe?.Dispose();
                if (!child.HasExited) { child.StandardInput.WriteLine("exit"); child.StandardInput.Flush(); if (!child.WaitForExit(5000)) child.Kill(); }
                // Preserve the behavioral assertion when reproducing the old shutdown defect.
                try { await Within(endpoint.StopAsync()); }
                catch (Exception error) { Console.WriteLine("PEER_EXIT cleanup failure: " + error); }
            }
        });
        foreach (bool ownerStops in new[] { true, false })
            await Test("Throwing cancellation callback cannot skip " + (ownerStops ? "shutdown cleanup" : "deferred lifetime cleanup on disconnect"), async () =>
            {
                var descriptor = ManualDescriptor(); string directory = Scratch("callback-fault");
                var ready = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
                var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
                var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var failure = new InjectedWorkerException(); var events = new ConcurrentQueue<PipeEvent>();
                var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), new SystemBridgeClock(), events.Enqueue);
                endpoint.Start(c =>
                {
                    c.Cancellation.Register(() => throw failure);
                    ready.TrySetResult(c); return new DispatchTicket(response.Task, quiesced.Task);
                });
                using var caller = new CancellationTokenSource(); using var request = LiveRequest("callback-fault", caller.Token);
                Task<BridgeResponse>? call = null;
                try
                {
                    call = new PipeBridgeClient(descriptor).SendAsync(request.Context);
                    var context = await Within(ready.Task);
                    if (ownerStops)
                    {
                        var error = await ThrowsAsync<AggregateException>(() => Within(endpoint.StopAsync()));
                        Check(error.Flatten().InnerExceptions.Any(e => ReferenceEquals(e, failure)), "Cancellation failure was lost");
                        Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Cancellation failure skipped shutdown cleanup");
                    }
                    else caller.Cancel();
                    await Until(() => events.Any(e => e.Stage == "Closed"));
                    Check(context.Cancellation.WaitHandle.WaitOne(0), "Unfinished callback lost its borrowed lifetime");
                    Check(!quiesced.Task.IsCompleted, "Test did not retain a physical callback");
                    quiesced.TrySetResult(true); response.TrySetCanceled();
                    await Until(() => { try { _ = context.Cancellation.WaitHandle; return false; } catch (ObjectDisposedException) { return true; } });
                    var stopped = await ThrowsAsync<AggregateException>(() => Within(endpoint.StopAsync()));
                    Check(stopped.Flatten().InnerExceptions.Any(e => ReferenceEquals(e, failure)), "Shutdown lost the callback fault");
                    Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Callback fault left a descriptor");
                    Console.WriteLine("CALLBACK_FAULT ownerStop=" + ownerStops + " descriptorRemoved=true lifetimeDisposedAfterQuiescence=true errorRetained=true");
                }
                finally
                {
                    caller.Cancel(); quiesced.TrySetResult(true); response.TrySetCanceled();
                    if (call != null) { try { await Within(call); } catch (Exception error) when (error is BridgeException || error is OperationCanceledException) { } }
                    try { await Within(endpoint.StopAsync()); } catch (AggregateException) { }
                }
            });
        foreach (bool lockDescriptor in new[] { false, true })
            await Test("Unexpected worker fault still cleans owned descriptor" + (lockDescriptor ? " after a retryable cleanup failure" : " and preserves its error"), async () =>
            {
                var descriptor = ManualDescriptor(); string directory = Scratch("worker-fault");
                string path = Path.Combine(directory, descriptor.BridgeSessionId + ".json");
                var catalog = new SessionCatalog(directory); var events = new ConcurrentQueue<PipeEvent>();
                var clock = new FailingReceiveClock(); int submitted = 0;
                var endpoint = new PipeEndpoint(descriptor, catalog, clock, events.Enqueue);
                endpoint.Start(c => { submitted++; return Ticket(c, descriptor.BridgeSessionId); });
                try
                {
                    using (var pipe = RawClient(descriptor))
                    {
                        await pipe.ConnectAsync(2000);
                        await Until(() => events.Any(e => e.Stage == "Closed"));
                    }
                    Equal(1, clock.Faults); Equal(0, submitted);
                    if (lockDescriptor)
                    {
                        using var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                        var combined = await ThrowsAsync<AggregateException>(() => Within(endpoint.StopAsync()));
                        var causes = combined.Flatten().InnerExceptions;
                        Equal(2, causes.Count); Check(causes.Any(e => ReferenceEquals(e, clock.Failure)), "Worker failure was lost");
                        Check(causes.Any(e => e is IOException), "Cleanup failure was lost");
                        Check(File.Exists(path), "Locked descriptor unexpectedly removed");
                    }
                    for (int attempt = 0; attempt < 2; attempt++)
                    {
                        var error = await ThrowsAsync<InjectedWorkerException>(() => Within(endpoint.StopAsync()));
                        Check(ReferenceEquals(clock.Failure, error), "Shutdown replaced the original worker error");
                        Check(!File.Exists(path), "Worker fault skipped descriptor cleanup");
                    }
                    using (var released = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid, true)) { }
                    await VerifyReplacement(catalog);
                    Console.WriteLine("WORKER_FAULT cleanupRetried=" + lockDescriptor + " descriptorRemoved=true originalErrorRetained=true replacementSelected=true");
                }
                finally
                {
                    try { await Within(endpoint.StopAsync()); }
                    catch (InjectedWorkerException) { } // the deliberate worker error remains observable
                }
            });
    }
    static async Task VerifyReplacement(SessionCatalog catalog)
    {
        var replacement = ManualDescriptor(); var next = new PipeEndpoint(replacement, catalog, new SystemBridgeClock());
        next.Start(c => Ticket(c, replacement.BridgeSessionId));
        try
        {
            Equal(replacement.BridgeSessionId, catalog.Resolve(null).BridgeSessionId);
            Equal(replacement.BridgeSessionId, catalog.Resolve(replacement.RevitPid).BridgeSessionId);
            using var call = LiveRequest("replacement");
            Equal(replacement.BridgeSessionId, (await new BridgeClientProvider(catalog, replacement.RevitPid, new SystemBridgeClock()).SendAsync(call.Context)).BridgeSessionId);
        }
        finally { await Within(next.StopAsync()); }
    }
    sealed class InjectedWorkerException : Exception { }
    sealed class FailingReceiveClock : IBridgeClock
    {
        readonly SystemBridgeClock actual = new SystemBridgeClock();
        internal readonly InjectedWorkerException Failure = new InjectedWorkerException();
        internal int Faults;
        public DateTimeOffset UtcNow => actual.UtcNow;
        public long Timestamp => actual.Timestamp;
        public TimeSpan ElapsedSince(long timestamp)
        { if (Interlocked.CompareExchange(ref Faults, 1, 0) == 0) throw Failure; return actual.ElapsedSince(timestamp); }
    }
}
