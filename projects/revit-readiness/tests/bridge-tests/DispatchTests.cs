using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    sealed class TestSignal : IDispatchSignal
    {
        public DispatchSignalResult Result = DispatchSignalResult.Accepted;
        public Func<DispatchSignalResult>? OnRaise;
        public DispatchSignalResult Raise() => OnRaise?.Invoke() ?? Result;
    }
    sealed class QueueCall
    {
        public CancellationTokenSource Cancel = new CancellationTokenSource();
        public RequestLifetime Lifetime = null!;
        public DispatchTicket Ticket = null!;
    }
    sealed class DispatchRig : IDisposable
    {
        public readonly FakeClock Clock = new FakeClock();
        public readonly TestSignal Signal = new TestSignal();
        public readonly DispatchQueue Queue;
        readonly List<QueueCall> calls = new List<QueueCall>();
        public DispatchRig() { Queue = new DispatchQueue("session-1", Signal); }
        public QueueCall Add()
        {
            var call = new QueueCall(); var budget = RequestBudget.Admit(Clock);
            var request = Request(BridgeJson.FormatUtc(budget.DeadlineUtc)); request.RequestId = Guid.NewGuid().ToString("N");
            call.Lifetime = new RequestLifetime(request, budget, call.Cancel.Token, CancellationToken.None);
            calls.Add(call); call.Ticket = Queue.Enqueue(call.Lifetime.Context); return call;
        }
        public void Dispose()
        {
            try { Queue.CancelAll(); }
            finally { foreach (var call in calls) { call.Lifetime.Dispose(); call.Cancel.Dispose(); } }
        }
    }
    static BridgeResponse QueueResponse(BridgeExecutionContext context, string result = "{}") => new BridgeResponse
    { ContractVersion = 1, RequestId = context.Request.RequestId, BridgeSessionId = "session-1", ResultJson = result };
    static void Hold(ManualResetEventSlim release)
    { Check(release.Wait(5000), "Test release barrier timed out"); }
    static async Task DispatchTests()
    {
        await DispatchPipeTests();
        await Test("Publication and immediate cancellation share one linearization gate", async () =>
        {
            var clock = new FakeClock(); using var cancel = new CancellationTokenSource();
            using var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), cancel.Token, CancellationToken.None);
            using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim(); using var attempt = new ManualResetEventSlim();
            var completion = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            clock.BeforeElapsed = () => { entered.Set(); Hold(release); };
            var publishing = Task.Run(() => lifetime.Context.PublishIfActive(completion, Response()));
            var cancelThread = new Thread(() => { attempt.Set(); cancel.Cancel(); }) { IsBackground = true };
            bool started = false;
            try
            {
                Check(entered.Wait(2000), "Publication did not reach its final guard"); cancelThread.Start(); started = true;
                Check(attempt.Wait(2000), "Cancel not attempted");
                Check(SpinWait.SpinUntil(() => lifetime.Context.IsCancellationRequested ||
                    (cancelThread.ThreadState & ThreadState.WaitSleepJoin) != 0, 2000), "Cancellation did not reach publication handoff");
                Check(!lifetime.Context.IsCancellationRequested, "Invalidation crossed a guarded publication");
            }
            finally { clock.BeforeElapsed = null; release.Set(); if (started) Check(cancelThread.Join(2000), "Cancel remained blocked"); }
            await publishing; Check(completion.Task.Status == TaskStatus.RanToCompletion, "Earlier publication lost its linearization");
            Check(lifetime.Context.IsCancellationRequested, "Later cancellation did not invalidate");
            var late = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            Throws<OperationCanceledException>(() => lifetime.Context.PublishIfActive(late, Response())); Check(!late.Task.IsCompleted, "Late publication succeeded");
        });
        await Test("Admission defers execution and passes the original context and budget", async () =>
        {
            using var rig = new DispatchRig(); var call = rig.Add(); int reads = 0;
            Check(!call.Ticket.Response.IsCompleted && !call.Ticket.Quiesced.IsCompleted, "Admission executed work");
            rig.Queue.Drain(4, context =>
            {
                Check(ReferenceEquals(context, call.Lifetime.Context), "Context was replaced");
                Check(ReferenceEquals(context.Budget, call.Lifetime.Context.Budget), "Budget was replaced"); reads++; return QueueResponse(context);
            });
            Equal(1, reads); Equal(call.Lifetime.Context.Request.RequestId, (await call.Ticket.Response).RequestId);
            await Progress(call.Ticket.Quiesced, "successful quiescence");
            _ = call.Lifetime.Context.Cancellation.WaitHandle; // queue did not dispose its borrowed lifetime
        });
        await Test("One executing plus four waiting rejects fifth and admits after progress", async () =>
        {
            using var rig = new DispatchRig(); var first = rig.Add(); using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var drain = Task.Run(() => rig.Queue.Drain(1, c => { entered.Set(); Hold(release); return QueueResponse(c); }));
            try
            {
                Check(entered.Wait(2000), "Executor not entered"); var waiting = Enumerable.Range(0, 4).Select(_ => rig.Add()).ToArray(); var rejected = rig.Add();
                var error = await rejected.Ticket.Response; Equal("Busy", error.Error!.Code); Check(error.Error.Retryable, "Busy is not retryable");
                Equal(rejected.Lifetime.Context.Request.RequestId, error.Error.RequestId); Equal("session-1", error.BridgeSessionId);
                Check(rejected.Ticket.Quiesced.IsCompleted && !first.Ticket.Quiesced.IsCompleted, "Wrong owner completed");
                release.Set(); await Progress(drain, "first drain"); int count = 0;
                rig.Queue.Drain(4, c => { count++; return QueueResponse(c); }); Equal(4, count);
                foreach (var item in waiting) Check((await item.Ticket.Response).Error == null, "Waiting work failed");
                var next = rig.Add(); rig.Queue.Drain(4, c => QueueResponse(c)); Check((await next.Ticket.Response).Error == null, "Capacity did not recover");
            }
            finally { release.Set(); await drain; }
        });
        foreach (var outcome in new[] { DispatchSignalResult.Accepted, DispatchSignalResult.Pending, DispatchSignalResult.Denied, DispatchSignalResult.TimedOut })
            await Test("Raise " + outcome + " leaves no orphan work", async () =>
            {
                using var rig = new DispatchRig(); rig.Signal.Result = outcome; var call = rig.Add(); int reads = 0;
                rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); });
                var response = await call.Ticket.Response;
                if (outcome == DispatchSignalResult.Accepted || outcome == DispatchSignalResult.Pending)
                { Equal(1, reads); Check(response.Error == null, "Successful scheduling rejected"); }
                else { Equal(0, reads); Equal("Busy", response.Error!.Code); Check(response.Error.Retryable, "Signal failure not retryable"); }
                await Progress(call.Ticket.Quiesced, "signal result quiescence");
            });
        await Test("Throwing Raise rejects waiting work and a later request recovers", async () =>
        {
            using var rig = new DispatchRig(); rig.Signal.OnRaise = () => throw new InvalidOperationException("host signal failed"); var failed = rig.Add();
            Equal("Busy", (await failed.Ticket.Response).Error!.Code); await Progress(failed.Ticket.Quiesced, "rejected signal");
            rig.Signal.OnRaise = null; var next = rig.Add(); rig.Queue.Drain(4, c => QueueResponse(c)); Check((await next.Ticket.Response).Error == null, "Queue poisoned");
        });
        await Test("Pending signal is rearmed by Idling and executes once", async () =>
        {
            using var rig = new DispatchRig(); bool scheduled = false; rig.Signal.Result = DispatchSignalResult.Pending; var call = rig.Add();
            rig.Signal.OnRaise = () => { scheduled = true; return DispatchSignalResult.Accepted; };
            rig.Queue.RearmIfNeeded(); int reads = 0;
            if (scheduled) rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); });
            Equal(1, reads); await Progress(call.Ticket.Response, "Idling recovery");
            rig.Queue.RearmIfNeeded(); rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); }); Equal(1, reads);
        });
        await Test("Arrival after final dequeue is re-signaled without exceeding batch", async () =>
        {
            using var rig = new DispatchRig(); var first = rig.Add(); QueueCall? arrival = null; bool rearmed = false;
            rig.Signal.OnRaise = () => { rearmed = true; return DispatchSignalResult.Accepted; };
            rig.Queue.Drain(1, c => { arrival = rig.Add(); return QueueResponse(c); });
            Check(rearmed, "Final-dequeue arrival stranded"); Check(!arrival!.Ticket.Response.IsCompleted, "Batch limit ignored"); int calls = 0;
            rig.Queue.Drain(1, c => { calls++; return QueueResponse(c); }); await Progress(arrival.Ticket.Response, "last-dequeue arrival"); Equal(1, calls);
        });
        await Test("Raise returning after drain cannot strand a new generation", async () =>
        {
            using var rig = new DispatchRig(); QueueCall? arrival = null; int reads = 0;
            rig.Signal.OnRaise = () =>
            {
                rig.Queue.Drain(1, c => { reads++; return QueueResponse(c); });
                arrival = rig.Add(); return DispatchSignalResult.Pending;
            };
            var first = rig.Add(); rig.Signal.OnRaise = null;
            rig.Queue.RearmIfNeeded(); rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); });
            await Progress(first.Ticket.Response, "first generation"); await Progress(arrival!.Ticket.Response, "new generation"); Equal(2, reads);
        });
        await Test("Drain caps a handler at four even with a larger requested batch", () => Sync(() =>
        {
            using var rig = new DispatchRig(); rig.Add(); int reads = 0;
            rig.Queue.Drain(100, c => { reads++; if (reads < 6) rig.Add(); return QueueResponse(c); }); Equal(4, reads);
            rig.Queue.Drain(4, c => { reads++; if (reads < 6) rig.Add(); return QueueResponse(c); }); Equal(6, reads);
        }));
        await Test("Reentrant and concurrent Drain never run a second executor", async () =>
        {
            using var rig = new DispatchRig(); rig.Add(); rig.Add(); int reads = 0;
            using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var drain = Task.Run(() => rig.Queue.Drain(1, c =>
            {
                Interlocked.Increment(ref reads); rig.Queue.Drain(4, other => { Interlocked.Increment(ref reads); return QueueResponse(other); });
                entered.Set(); Hold(release); return QueueResponse(c);
            }));
            try { Check(entered.Wait(2000), "Executor not entered"); rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); }); Equal(1, reads); }
            finally { release.Set(); await drain; }
            rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); }); Equal(2, reads);
        });
        await Test("Queued cancellation removes the middle item and frees capacity", async () =>
        {
            using var rig = new DispatchRig(); var calls = Enumerable.Range(0, 4).Select(_ => rig.Add()).ToArray();
            calls[1].Cancel.Cancel(); Check(calls[1].Ticket.Response.IsCanceled, "Queued response did not cancel immediately");
            await Progress(calls[1].Ticket.Quiesced, "removed queued item"); var extra = rig.Add(); var seen = new List<string>();
            rig.Queue.Drain(4, c => { seen.Add(c.Request.RequestId); return QueueResponse(c); });
            Equal(4, seen.Count); Check(!seen.Contains(calls[1].Lifetime.Context.Request.RequestId), "Canceled reader invoked");
            Check((await extra.Ticket.Response).Error == null, "Removed slot not reusable");
        });
        foreach (bool executing in new[] { false, true })
            await Test("Held notification cannot delay " + (executing ? "executing" : "queued") + " response cancellation", async () =>
            {
                using var rig = new DispatchRig(); var call = rig.Add(); using var notification = new ManualResetEventSlim(); using var releaseNotification = new ManualResetEventSlim();
                using var reader = new ManualResetEventSlim(); using var releaseReader = new ManualResetEventSlim();
                using var registration = call.Lifetime.Context.Cancellation.Register(() => { notification.Set(); Hold(releaseNotification); });
                int reads = 0; Task drain = Task.CompletedTask;
                if (executing) drain = Task.Run(() => rig.Queue.Drain(1, c => { reads++; reader.Set(); Hold(releaseReader); return QueueResponse(c); }));
                try
                {
                    if (executing) Check(reader.Wait(2000), "Reader not entered");
                    call.Cancel.Cancel(); Check(notification.Wait(2000), "Notification not entered");
                    Check(call.Lifetime.Context.IsCancellationRequested && call.Ticket.Response.IsCanceled, "Immediate invalidation lost");
                    Equal(!executing, call.Ticket.Quiesced.IsCompleted);
                    var cleanup = call.Lifetime.DisposeAfterQuiescenceAsync(call.Ticket); Check(!cleanup.IsCompleted, "Disposed held resources");
                    rig.Queue.CancelAll(); Check(!cleanup.IsCompleted, "Stop waited or discarded callback ownership");
                    releaseReader.Set(); await drain; await Progress(call.Ticket.Quiesced, "reader release");
                    Check(!cleanup.IsCompleted, "Held notification source disposed");
                    releaseNotification.Set(); await Progress(cleanup, "physical cleanup");
                    Check(call.Ticket.Response.IsCanceled, "Late success replaced cancellation"); Equal(executing ? 1 : 0, reads);
                    Throws<ObjectDisposedException>(() => { var h = call.Lifetime.Context.Cancellation.WaitHandle; });
                }
                finally { releaseReader.Set(); releaseNotification.Set(); await drain; }
            });
        await Test("Already canceled admission cannot execute or borrow a disposed registration", async () =>
        {
            using var rig = new DispatchRig(); var budget = RequestBudget.Admit(rig.Clock);
            using var cancel = new CancellationTokenSource(); using var active = new RequestLifetime(Request(), budget, cancel.Token, CancellationToken.None);
            cancel.Cancel(); var ticket = rig.Queue.Enqueue(active.Context); int reads = 0;
            rig.Queue.Drain(4, c => { reads++; return QueueResponse(c); }); Equal(0, reads); Check(ticket.Response.IsCanceled, "Canceled admission accepted");
            await Progress(ticket.Quiesced, "pre-canceled admission");
        });
        foreach (bool during in new[] { false, true })
            await Test("Original deadline expires " + (during ? "during execution" : "before read") + " without late success", async () =>
            {
                using var rig = new DispatchRig(); var call = rig.Add(); int reads = 0;
                if (!during) rig.Clock.Advance(TimeSpan.FromSeconds(30));
                rig.Queue.Drain(4, c => { reads++; rig.Clock.Advance(TimeSpan.FromSeconds(30)); return QueueResponse(c); });
                Equal(during ? 1 : 0, reads); Equal("DeadlineExceeded", (await call.Ticket.Response).Error!.Code);
                Equal(Epoch.AddSeconds(30), call.Lifetime.Context.Budget.DeadlineUtc); await Progress(call.Ticket.Quiesced, "expired request");
            });
        await Test("Stop settles queued and executing responses without awaiting the executor", async () =>
        {
            using var rig = new DispatchRig(); var running = rig.Add(); using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var drain = Task.Run(() => rig.Queue.Drain(4, c => { entered.Set(); Hold(release); return QueueResponse(c); }));
            try
            {
                Check(entered.Wait(2000), "Reader not entered"); var waiting = rig.Add();
                rig.Queue.CancelAll(); rig.Queue.CancelAll();
                Check(running.Ticket.Response.IsCanceled && waiting.Ticket.Response.IsCanceled, "Stop did not cancel responses");
                Check(!running.Ticket.Quiesced.IsCompleted && waiting.Ticket.Quiesced.IsCompleted, "Stop confused logical and physical completion");
                Equal("BridgeUnavailable", (await rig.Add().Ticket.Response).Error!.Code);
            }
            finally { release.Set(); await drain; }
            Check(running.Ticket.Response.IsCanceled && running.Ticket.Quiesced.IsCompleted, "Late stop result changed");
        });
        await Test("Executor errors remain correlated and do not orphan later queued work", async () =>
        {
            using var rig = new DispatchRig(); var first = rig.Add(); var second = rig.Add(); int count = 0;
            rig.Queue.Drain(4, c => { if (++count == 1) throw new InvalidOperationException("private path must not leak"); return QueueResponse(c); });
            var failed = await first.Ticket.Response; Equal("InternalError", failed.Error!.Code); Equal(first.Lifetime.Context.Request.RequestId, failed.Error.RequestId);
            Check(!failed.Error.Message.Contains("private path"), "Internal diagnostic leaked");
            Check((await second.Ticket.Response).Error == null, "Error stranded next request");
            Check(first.Ticket.Quiesced.IsCompleted && second.Ticket.Quiesced.IsCompleted, "Error skipped completion");
        });
        await Test("Invalid executor envelopes cannot publish success or strand the next request", async () =>
        {
            foreach (string mode in new[] { "null", "request", "session", "version" })
            {
                using var rig = new DispatchRig(); var invalid = rig.Add(); var next = rig.Add(); int calls = 0;
                rig.Queue.Drain(4, c =>
                {
                    var result = QueueResponse(c); if (++calls != 1) return result;
                    if (mode == "null") return null!;
                    if (mode == "request") result.RequestId = "foreign";
                    if (mode == "session") result.BridgeSessionId = "other-session";
                    if (mode == "version") result.ContractVersion = 99;
                    return result;
                });
                var failed = await invalid.Ticket.Response;
                Check(failed?.Error?.Code == "InternalError", "Invalid executor envelope published: " + mode);
                Equal(invalid.Lifetime.Context.Request.RequestId, failed!.RequestId);
                Check((await next.Ticket.Response).Error == null, "Invalid response stranded next request");
                await Progress(invalid.Ticket.Quiesced, "invalid executor envelope");
            }
        });
        await Test("Declared executor error retains code with current request identity", async () =>
        {
            using var rig = new DispatchRig(); var call = rig.Add();
            rig.Queue.Drain(4, _ => throw new BridgeException(new BridgeError { Code = "ContextChanged", Message = "Get a fresh context.", RequestId = "foreign", Retryable = false }));
            var error = (await call.Ticket.Response).Error!; Equal("ContextChanged", error.Code); Equal(call.Lifetime.Context.Request.RequestId, error.RequestId);
        });
        await Test("Cancellation inside executor wins response publication", async () =>
        {
            for (int i = 0; i < 25; i++)
            {
                using var rig = new DispatchRig(); var call = rig.Add();
                rig.Queue.Drain(4, c => { call.Cancel.Cancel(); return QueueResponse(c); });
                Check(call.Ticket.Response.IsCanceled, "Cancellation became success"); await Progress(call.Ticket.Quiesced, "cancel versus complete");
            }
        });
        await Test("Unchanged context identity reads fresh values on each execution", async () =>
        {
            using var rig = new DispatchRig(); var store = new ContextStore("session-1", rig.Clock); var token = store.Issue(Binding()).Token;
            string value = "missing"; Func<BridgeExecutionContext, BridgeResponse> read = c => { store.Validate(token, Binding()); return QueueResponse(c, value); };
            var first = rig.Add(); rig.Queue.Drain(4, read); Equal("missing", (await first.Ticket.Response).ResultJson);
            value = "populated"; var second = rig.Add(); rig.Queue.Drain(4, read); Equal("populated", (await second.Ticket.Response).ResultJson);
        });
    }
    static async Task DispatchPipeTests()
    {
        foreach (string origin in new[] { "stop", "disconnect" })
        foreach (bool executing in new[] { false, true })
            await Test("Real endpoint " + origin + " with production " + (executing ? "executing" : "queued") + " dispatcher", async () =>
            {
                var descriptor = ManualDescriptor(); string directory = Scratch("dispatch-" + origin);
                var queue = new DispatchQueue(descriptor.BridgeSessionId, new TestSignal());
                var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), new SystemBridgeClock());
                var admitted = new TaskCompletionSource<(BridgeExecutionContext Context, DispatchTicket Ticket)>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var reader = new ManualResetEventSlim(); using var releaseReader = new ManualResetEventSlim();
                using var notification = new ManualResetEventSlim(); using var releaseNotification = new ManualResetEventSlim();
                endpoint.Start(c => { var ticket = queue.Enqueue(c); admitted.TrySetResult((c, ticket)); return ticket; });
                Task drain = Task.CompletedTask; CancellationTokenRegistration registration = default;
                using var pipe = RawClient(descriptor); int reads = 0;
                try
                {
                    await pipe.ConnectAsync(2000); using var client = LiveRequest("production-dispatch");
                    await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(client.Context.Request), 65536, CancellationToken.None);
                    var work = await Within(admitted.Task);
                    registration = work.Context.Cancellation.Register(() => { notification.Set(); Hold(releaseNotification); });
                    if (executing)
                    {
                        drain = Task.Run(() => queue.Drain(4, c => { reads++; reader.Set(); Hold(releaseReader); return Reply(c, descriptor.BridgeSessionId); }));
                        Check(reader.Wait(2000), "Production queue did not enter reader");
                    }
                    if (origin == "stop") { queue.CancelAll(); await Progress(endpoint.StopAsync(), "production queue network stop"); }
                    else
                    {
                        pipe.Dispose(); await Until(() => work.Ticket.Response.IsCanceled);
                        var replacements = new List<System.IO.Pipes.NamedPipeClientStream>();
                        try
                        {
                            for (int i = 0; i < 8; i++) { var next = RawClient(descriptor); replacements.Add(next); await next.ConnectAsync(2000); }
                            Equal(8, replacements.Count);
                        }
                        finally { foreach (var next in replacements) next.Dispose(); }
                        queue.CancelAll(); await Progress(endpoint.StopAsync(), "disconnected queue network stop");
                    }
                    Check(notification.Wait(2000), "Held notification did not start");
                    Check(work.Context.IsCancellationRequested && work.Ticket.Response.IsCanceled, "Cancellation boundary lost");
                    Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Owned descriptor retained");
                    Equal(!executing, work.Ticket.Quiesced.IsCompleted);
                    queue.Drain(4, c => { reads++; return Reply(c, descriptor.BridgeSessionId); }); Equal(executing ? 1 : 0, reads);
                    _ = work.Context.Cancellation.WaitHandle;
                    releaseReader.Set(); await drain; await Progress(work.Ticket.Quiesced, "production executor release");
                    _ = work.Context.Cancellation.WaitHandle; // notification is still held
                    releaseNotification.Set();
                    await Until(() => { try { _ = work.Context.Cancellation.WaitHandle; return false; } catch (ObjectDisposedException) { return true; } });
                    Check(work.Ticket.Response.IsCanceled, "Late production response escaped");
                    Console.WriteLine("DISPATCH_PIPE_PROOF origin=" + origin + " executing=" + executing + " networkBeforeRelease=true lateSuccess=false");
                }
                finally { releaseReader.Set(); releaseNotification.Set(); await drain; registration.Dispose(); queue.CancelAll(); await endpoint.StopAsync(); }
            });
    }
}
