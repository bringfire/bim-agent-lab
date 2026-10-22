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
    static async Task CancellationProgressTests()
    {
        var modes = from origin in new[] { "stop", "disconnect", "deadline" }
                    from outcome in new[] { "return", "throw" }
                    from timing in new[] { "before", "after" }
                    select origin + "-" + outcome + "-" + timing;
        foreach (string mode in modes.Concat(new[] { "capacity", "client", "quiesced-fault" }))
            await Test("Cancellation progress child: " + mode, async () =>
            {
                using var child = new Process { StartInfo = new ProcessStartInfo(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BimAgentLab.Bridge.Tests.exe"), "--progress-child " + mode)
                    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true } };
                Check(child.Start(), "Progress child failed to start");
                var stdout = child.StandardOutput.ReadToEndAsync(); var stderr = child.StandardError.ReadToEndAsync();
                try
                {
                    await Until(() => child.HasExited, 25000);
                    string output = await stdout, error = await stderr;
                    Console.WriteLine("PROGRESS_CHILD mode=" + mode + " exit=" + child.ExitCode); Console.Write(output); Console.Write(error);
                    Equal(0, child.ExitCode); Equal("", error);
                    Check(output.Contains("PROGRESS_PROOF mode=" + mode), "Missing completed progress proof");
                }
                finally { if (!child.HasExited) { child.Kill(); child.WaitForExit(5000); } }
            });
    }
    static async Task<int> ProgressChild(string mode)
    {
        SetErrorMode(3);
        try
        {
            if (mode == "capacity") await RetainedCapacityChild();
            else if (mode == "client") await ClientProgressChild();
            else if (mode == "quiesced-fault") await FaultedQuiescenceChild();
            else await EndpointProgressChild(mode);
            Console.WriteLine("PROGRESS_PROOF mode=" + mode); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    static async Task Progress(Task task, string boundary)
    { Check(await Task.WhenAny(task, Task.Delay(2000)) == task, "Progress blocked at " + boundary); await task; }
    // A separate caller lets the test detect StopAsync blocking BEFORE returning its Task.
    static Task InvokeStop(PipeEndpoint endpoint, TaskCompletionSource<bool> returned)
        => Task.Factory.StartNew(async () => { Task stop = endpoint.StopAsync(); returned.TrySetResult(true); await stop; },
            CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    static async Task EightOpenSlots(BridgeSessionDescriptor descriptor)
    {
        var pipes = new NamedPipeClientStream[8];
        try
        {
            for (int i = 0; i < pipes.Length; i++) { pipes[i] = RawClient(descriptor); await pipes[i].ConnectAsync(500); }
            Console.WriteLine("WHILE_HELD openConnections=8");
        }
        finally { foreach (var pipe in pipes) pipe?.Dispose(); }
    }
    static async Task EndpointProgressChild(string mode)
    {
        string[] parts = mode.Split('-'); string origin = parts[0]; bool throws = parts[1] == "throw", early = parts[2] == "before";
        var descriptor = ManualDescriptor(); string directory = Scratch(mode); var catalog = new SessionCatalog(directory);
        var events = new ConcurrentQueue<PipeEvent>();
        var ready = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var returned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim(); int callbacks = 0; bool borrowedAlive = false;
        var failure = new InvalidOperationException("held progress callback");
        var endpoint = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock(), events.Enqueue);
        endpoint.Start(c =>
        {
            c.Cancellation.Register(() =>
            {
                Interlocked.Increment(ref callbacks); entered.TrySetResult(true);
                Check(release.Wait(15000), "Harness did not release callback");
                borrowedAlive = c.Cancellation.WaitHandle.WaitOne(0);
                if (throws) throw failure;
            });
            ready.TrySetResult(c); return new DispatchTicket(response.Task, quiesced.Task);
        });
        using var pipe = RawClient(descriptor); Task? stop = null; BridgeExecutionContext? context = null;
        try
        {
            await pipe.ConnectAsync(2000);
            var deadline = DateTimeOffset.UtcNow.AddMilliseconds(origin == "deadline" ? 700 : 25000);
            var request = Request(BridgeJson.FormatUtc(deadline)); request.RequestId = mode;
            await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request), BridgeLimits.MaxRequestBytes, CancellationToken.None);
            context = await Within(ready.Task); if (early) quiesced.TrySetResult(true);
            if (origin == "stop")
            {
                stop = InvokeStop(endpoint, returned);
                await Progress(returned.Task, "StopAsync returning task");
                Throws<OperationCanceledException>(context.ThrowIfInactive); // immediate, not eventual notification
            }
            else if (origin == "disconnect") pipe.Dispose();
            await Progress(entered.Task, "notification start");
            Check(!events.Any(e => e.Stage == "LifetimeDisposed" && e.RequestId == mode), "Disposal reported with callback held");
            Equal(deadline, context.Budget.DeadlineUtc);
            if (origin == "deadline") Equal("DeadlineExceeded", Throws<BridgeException>(context.ThrowIfInactive).Error.Code);
            else Throws<OperationCanceledException>(context.ThrowIfInactive);
            await Progress(Until(() => events.Any(e => e.Stage == "Closed" && e.ConnectionId == 1)), "connection release");
            if (origin != "stop") await EightOpenSlots(descriptor);
            if (stop == null) stop = InvokeStop(endpoint, returned);
            await Progress(returned.Task, "StopAsync returning task"); await Progress(stop, "network shutdown");
            await Progress(endpoint.StopAsync(), "repeated network shutdown");
            Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Descriptor retained behind callback");
            Check(!release.IsSet && callbacks == 1, "Callback was not held throughout progress proof");
            Check(context.Cancellation.WaitHandle.WaitOne(0), "Lifetime released while callback held");
            Console.WriteLine("WHILE_HELD mode=" + mode + " stopReturned=true networkReleased=true descriptorRemoved=true quiesced=" + quiesced.Task.IsCompleted);
            // Attempt a late successful response while notification is still held.
            response.TrySetResult(Reply(context, descriptor.BridgeSessionId));
            if (origin != "disconnect") Equal(0, await Within(pipe.ReadAsync(new byte[1], 0, 1)));
            quiesced.TrySetResult(true); release.Set();
            await Until(() => IsLifetimeDisposed(context)); Check(borrowedAlive, "Callback touched a disposed source"); Equal(1, callbacks);
            if (throws)
            {
                await Until(() => events.Any(e => e.Stage == "CancellationCallbackFailed"));
                var error = await ThrowsAsync<AggregateException>(() => endpoint.StopAsync());
                Check(error.Flatten().InnerExceptions.Any(e => ReferenceEquals(e, failure)), "Late callback error lost");
            }
            await VerifyReplacement(catalog);
        }
        finally
        {
            release.Set(); quiesced.TrySetResult(true); response.TrySetCanceled(); pipe.Dispose();
            if (stop != null) { try { await Within(stop); } catch (AggregateException) when (throws) { } }
            try { await Within(endpoint.StopAsync()); } catch (AggregateException) when (throws) { }
            if (context != null)
            {
                await Until(() => IsLifetimeDisposed(context));
                await Until(() => events.Any(e => e.Stage == "LifetimeDisposed" && e.RequestId == mode && e.ConnectionId == 1));
                Equal(1, events.Count(e => e.Stage == "LifetimeDisposed" && e.RequestId == mode));
            }
        }
    }
    static async Task<BridgeResponse> RawCall(BridgeSessionDescriptor descriptor, string id)
    {
        using var pipe = RawClient(descriptor); await pipe.ConnectAsync(1000);
        var request = Request(BridgeJson.FormatUtc(DateTimeOffset.UtcNow.AddSeconds(10))); request.RequestId = id;
        await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request), BridgeLimits.MaxRequestBytes, CancellationToken.None);
        return BridgeJson.Deserialize<BridgeResponse>(await Within(FrameCodec.ReadAsync(pipe, BridgeLimits.MaxResponseBytes, CancellationToken.None)));
    }
    static async Task RetainedCapacityChild()
    {
        var descriptor = ManualDescriptor(); var catalog = new SessionCatalog(Scratch("retained-capacity"));
        var contexts = new ConcurrentQueue<BridgeExecutionContext>(); int entered = 0, submitted = 0;
        using var release = new ManualResetEventSlim(); var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var endpoint = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock());
        endpoint.Start(c =>
        {
            Interlocked.Increment(ref submitted); contexts.Enqueue(c);
            c.Cancellation.Register(() => { Interlocked.Increment(ref entered); Check(release.Wait(20000), "Capacity harness timeout"); _ = c.Cancellation.WaitHandle; });
            return new DispatchTicket(response.Task, Task.CompletedTask);
        });
        PipeEndpoint? replacement = null;
        try
        {
            for (int i = 1; i <= 8; i++)
            {
                using (var pipe = RawClient(descriptor))
                {
                    await pipe.ConnectAsync(1000); var request = Request(BridgeJson.FormatUtc(DateTimeOffset.UtcNow.AddSeconds(25))); request.RequestId = "held-" + i;
                    await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request), BridgeLimits.MaxRequestBytes, CancellationToken.None);
                    await Until(() => submitted == i);
                }
                await Until(() => entered == i);
            }
            await EightOpenSlots(descriptor);
            var busy = await RawCall(descriptor, "at-capacity"); Equal("at-capacity", busy.RequestId); Equal("Busy", busy.Error!.Code); Check(busy.Error.Retryable, "Busy must allow deliberate retry"); Equal(8, submitted);
            await Progress(endpoint.StopAsync(), "full-capacity shutdown");
            var next = ManualDescriptor(); int nextSubmitted = 0;
            replacement = new PipeEndpoint(next, catalog, new SystemBridgeClock()); replacement.Start(c => { nextSubmitted++; return Ticket(c, next.BridgeSessionId); });
            Equal("Busy", (await RawCall(next, "restart-capacity")).Error!.Code); Equal(0, nextSubmitted); Equal(8, entered);
            Check(contexts.All(c => !IsLifetimeDisposed(c)), "Cap released a still-held lifetime");
            Console.WriteLine("WHILE_HELD capacity=8 overload=Busy restartBypass=false submissions=8");
            release.Set(); response.TrySetCanceled(); await Until(() => contexts.All(IsLifetimeDisposed));
            BridgeResponse result; var timer = Stopwatch.StartNew();
            do { result = await RawCall(next, "after-release"); if (result.Error == null) break; await Task.Delay(10); } while (timer.ElapsedMilliseconds < 2000);
            Check(result.Error == null, "Capacity failed to recover after physical release"); Equal(1, nextSubmitted);
            await replacement.StopAsync();
        }
        finally { release.Set(); response.TrySetCanceled(); await Within(endpoint.StopAsync()); if (replacement != null) await Within(replacement.StopAsync()); }
    }
    static async Task ClientProgressChild()
    {
        var descriptor = ManualDescriptor(); var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var catalog = new SessionCatalog(Scratch("client-progress"));
        var endpoint = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock());
        endpoint.Start(c => { ready.TrySetResult(true); return new DispatchTicket(response.Task, Task.CompletedTask); });
        using var upstream = new CancellationTokenSource(); using var lifetime = LiveRequest("client-held", upstream.Token);
        using var release = new ManualResetEventSlim(); var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = new BridgeClientProvider(catalog, null, new SystemBridgeClock());
        Task<BridgeResponse> call = provider.SendAsync(lifetime.Context); Task? cancel = null;
        try
        {
            await Within(ready.Task);
            lifetime.Context.Cancellation.Register(() => { entered.TrySetResult(true); Check(release.Wait(10000), "Client callback timed out"); _ = lifetime.Context.Cancellation.WaitHandle; });
            cancel = Task.Factory.StartNew(upstream.Cancel, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            await Progress(cancel, "upstream cancellation return"); Throws<OperationCanceledException>(lifetime.Context.ThrowIfInactive);
            await Progress(entered.Task, "client notification start");
            await Progress(ThrowsAsync<OperationCanceledException>(() => call), "client network release");
            await Progress(endpoint.StopAsync(), "server shutdown with client callback held");
            lifetime.Dispose(); Check(!IsLifetimeDisposed(lifetime.Context), "Client lifetime disposed before notification ended");
            Console.WriteLine("WHILE_HELD clientNetworkReleased=true immediateInvalidation=true");
            release.Set(); await Until(() => IsLifetimeDisposed(lifetime.Context));
        }
        finally
        {
            release.Set(); response.TrySetCanceled(); if (cancel != null) await Within(cancel);
            try { await Within(call); } catch (Exception e) when (e is BridgeException || e is OperationCanceledException) { }
            await Within(endpoint.StopAsync());
        }
    }
    static async Task FaultedQuiescenceChild()
    {
        var descriptor = ManualDescriptor(); var events = new ConcurrentQueue<PipeEvent>();
        var contexts = new ConcurrentQueue<BridgeExecutionContext>();
        var failure = new InvalidOperationException("executor quiesced exceptionally");
        var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("quiesced-fault")), new SystemBridgeClock(), events.Enqueue);
        endpoint.Start(c =>
        {
            contexts.Enqueue(c);
            return new DispatchTicket(Task.FromException<BridgeResponse>(failure), Task.FromException(failure));
        });
        try
        {
            // More completions than retained capacity: terminal errors must not leak permits.
            for (int i = 0; i < 9; i++)
            {
                Equal("InternalError", (await RawCall(descriptor, "fault-" + i)).Error!.Code);
                await Until(() => contexts.All(IsLifetimeDisposed));
            }
            await Until(() => events.Count(e => e.Stage == "LifetimeCleanupFailed") == 9);
            var error = await ThrowsAsync<InvalidOperationException>(() => endpoint.StopAsync());
            Check(ReferenceEquals(failure, error), "Quiesced failure was not retained after cleanup");
            Console.WriteLine("EXCEPTIONAL_QUIESCENCE disposed=9 capacityRecovered=true errorRetained=true");
        }
        finally { try { await endpoint.StopAsync(); } catch (InvalidOperationException error) when (ReferenceEquals(error, failure)) { } }
    }
}
