using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;
public sealed class PipeEvent
{
    public long ConnectionId { get; set; }
    public string Stage { get; set; } = "";
    public long Timestamp { get; set; }
    public string RequestId { get; set; } = "";
    public Exception? Error { get; set; }
}
public sealed class PipeEndpoint : IHostBridgeEndpoint
{
    readonly BridgeSessionDescriptor descriptor;
    readonly SessionCatalog catalog;
    readonly IBridgeClock clock;
    readonly Action<PipeEvent>? observe;
    readonly object gate = new object();
    readonly CancellationTokenSource stopping = new CancellationTokenSource();
    Task[] workers = Array.Empty<Task>();
    Task? stop;
    bool started;
    bool ownsDescriptor;
    bool stoppingDisposed;
    Exception? cancellationFailure;
    Exception? callbackFailure; // bounded retention: first asynchronous failure; every failure is emitted
    long connections;
    public PipeEndpoint(BridgeSessionDescriptor descriptor, SessionCatalog catalog, IBridgeClock clock, Action<PipeEvent>? observe = null)
    {
        this.descriptor = BridgeJson.Deserialize<BridgeSessionDescriptor>(BridgeJson.Serialize(descriptor));
        this.catalog = catalog; this.clock = clock; this.observe = observe;
    }
    public void Start(Func<BridgeExecutionContext, DispatchTicket> submit)
    {
        if (submit == null) throw new ArgumentNullException(nameof(submit));
        lock (gate)
        {
            if (stop != null) throw new InvalidOperationException("Create a new endpoint to restart.");
            if (started) return;
            BridgeIdentity.ValidateLive(descriptor);
            var pipes = new NamedPipeServerStream[8];
            try
            {
                for (int i = 0; i < pipes.Length; i++) pipes[i] = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid, i == 0);
                catalog.Publish(descriptor);
                ownsDescriptor = true;
                workers = pipes.Select(pipe => Task.Run(() => Serve(pipe, submit))).ToArray(); started = true;
            }
            catch { foreach (var pipe in pipes) pipe?.Dispose(); RemoveDescriptor(); throw; }
        }
    }
    public Task StopAsync()
    {
        TaskCompletionSource<bool> completion;
        bool cancel;
        lock (gate)
        {
            if (stop != null && !stop.IsFaulted && !(stop.IsCompleted && Volatile.Read(ref callbackFailure) != null)) return stop;
            completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            stop = completion.Task; cancel = !stoppingDisposed;
        }
        // No callback or I/O is invoked while holding the endpoint gate. This source
        // only runs bridge-owned signaling; executor notification has its own worker.
        if (cancel) try { stopping.Cancel(); } catch (Exception error) { cancellationFailure = error; }
        _ = CompleteStop(completion);
        return completion.Task;
    }
    async Task CompleteStop(TaskCompletionSource<bool> completion)
    {
        try { await FinishStop().ConfigureAwait(false); completion.TrySetResult(true); }
        catch (Exception error) { completion.TrySetException(error); }
    }
    async Task FinishStop()
    {
        // Faulted workers have finished borrowing network resources too. Their error
        // must remain observable, but cannot make owned cleanup unreachable.
        var drained = Task.WhenAll(workers); // network workers, never Revit callbacks
        var failures = new List<Exception>();
        if (cancellationFailure != null) failures.Add(cancellationFailure);
        try { await drained.ConfigureAwait(false); }
        catch (Exception error)
        { if (drained.Exception != null) failures.AddRange(drained.Exception.InnerExceptions); else failures.Add(error); }
        var callbackError = Volatile.Read(ref callbackFailure);
        if (callbackError != null) failures.Add(callbackError);
        try { RemoveDescriptor(); } // changed/locked files are preserved; later Stop can retry
        catch (Exception cleanupFailure) { failures.Add(cleanupFailure); }
        finally { stopping.Dispose(); stoppingDisposed = true; }
        if (failures.Count == 1) ExceptionDispatchInfo.Capture(failures[0]).Throw();
        if (failures.Count > 1) throw new AggregateException(failures);
    }
    void RemoveDescriptor()
    {
        // A shared catalog may own another endpoint's descriptor. Failed acquisition
        // grants this endpoint no authority to remove that existing publication.
        if (!ownsDescriptor) return;
        catalog.RemoveOwned(descriptor);
        ownsDescriptor = false;
    }
    async Task Serve(NamedPipeServerStream initial, Func<BridgeExecutionContext, DispatchTicket> submit)
    {
        NamedPipeServerStream? pipe = initial;
        try
        {
            while (pipe != null && !stopping.IsCancellationRequested)
            {
                using (pipe)
                using (stopping.Token.Register(pipe.Dispose))
                {
                    // Owner cancellation closes the handle. Await completion before cleanup;
                    // a competing framework CancelIoEx callback can race that close on net48.
                    await pipe.WaitForConnectionAsync(CancellationToken.None).ConfigureAwait(false);
                    stopping.Token.ThrowIfCancellationRequested();
                    long accepted = clock.Timestamp, id = Interlocked.Increment(ref connections);
                    Emit(id, "Accepted", accepted);
                    try { await Handle(pipe, id, accepted, submit).ConfigureAwait(false); }
                    catch (Exception error) when (Expected(error)) { Emit(id, "Rejected", clock.Timestamp); }
                    finally { pipe.Dispose(); Emit(id, "Closed", clock.Timestamp); }
                }
                pipe = null;
                if (!stopping.IsCancellationRequested) pipe = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid);
            }
        }
        catch (Exception error) when (Expected(error))
        { if (!stopping.IsCancellationRequested) { Emit(0, "ListenerFailed", clock.Timestamp); stopping.Cancel(); } }
        finally { pipe?.Dispose(); }
    }
    async Task Handle(NamedPipeServerStream pipe, long id, long accepted, Func<BridgeExecutionContext, DispatchTicket> submit)
    {
        using var frame = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token);
        TimeSpan receiveRemaining = TimeSpan.FromSeconds(5) - clock.ElapsedSince(accepted);
        if (receiveRemaining <= TimeSpan.Zero) return;
        frame.CancelAfter(TimeSpan.FromMilliseconds(Math.Ceiling(receiveRemaining.TotalMilliseconds)));
        BridgeRequest request;
        RequestBudget budget;
        using (frame.Token.Register(pipe.Dispose))
        {
            NativeWindows.ValidateClient(pipe, descriptor.LogonSid, descriptor.WindowsSessionId);
            var bytes = await FrameCodec.ReadAsync(pipe, BridgeLimits.MaxRequestBytes, CancellationToken.None).ConfigureAwait(false);
            try { request = BridgeJson.Deserialize<BridgeRequest>(bytes); }
            catch (BridgeException error)
            {
                var failure = new BridgeResponse { ContractVersion = 1, RequestId = error.Error.RequestId, BridgeSessionId = descriptor.BridgeSessionId, Error = error.Error };
                await FrameCodec.WriteAsync(pipe, BridgeJson.SerializeResponse(failure), BridgeLimits.MaxResponseBytes, CancellationToken.None).ConfigureAwait(false); return;
            }
            frame.Token.ThrowIfCancellationRequested();
            try { budget = RequestBudget.Receive(clock, accepted, BridgeJson.ParseUtc(request.DeadlineUtc)); }
            catch (BridgeException error)
            {
                error.Error.RequestId = request.RequestId;
                await FrameCodec.WriteAsync(pipe, BridgeJson.SerializeResponse(Failure(error.Error)), BridgeLimits.MaxResponseBytes, CancellationToken.None).ConfigureAwait(false);
                return;
            }
        }
        // Capacity includes lifetimes retained AFTER their network connection closes.
        var retention = RetainedRequestSlot.TryAcquire();
        if (retention == null)
        {
            using (frame.Token.Register(pipe.Dispose))
                await FrameCodec.WriteAsync(pipe, BridgeJson.SerializeResponse(Failure(BridgeFailures.Create(
                    "Busy", "The bridge is retaining unfinished requests. Retry after they finish.", request.RequestId).Error)),
                    BridgeLimits.MaxResponseBytes, CancellationToken.None).ConfigureAwait(false);
            return;
        }
        frame.CancelAfter(Timeout.Infinite);
        using var disconnected = new CancellationTokenSource();
        RequestLifetime lifetime;
        try { lifetime = new RequestLifetime(request, budget, disconnected.Token, stopping.Token,
            error => RecordCancellationFailure(id, request.RequestId, error)); }
        catch { retention.Dispose(); throw; }
        DispatchTicket? ticket = null;
        using var monitorStop = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token);
        var monitor = MonitorDisconnect(pipe, disconnected, monitorStop.Token);
        try
        {
            using var closeOnCancel = lifetime.Context.TransportCancellation.Register(pipe.Dispose);
            BridgeResponse response;
            try
            {
                lifetime.Context.ThrowIfInactive(); ticket = submit(lifetime.Context);
                response = await AwaitResponse(ticket.Response, lifetime.Context.TransportCancellation).ConfigureAwait(false);
            }
            catch (BridgeException error) { response = Failure(new BridgeError { Code = error.Error.Code, Message = error.Error.Message, RequestId = request.RequestId, Retryable = error.Error.Retryable }); }
            catch (Exception error) when (!(error is OperationCanceledException))
            { response = Failure(BridgeFailures.Create("InternalError", "The bridge could not complete this request.", request.RequestId).Error); }
            lifetime.Context.ThrowIfInactive();
            if (response.RequestId != request.RequestId || response.BridgeSessionId != descriptor.BridgeSessionId)
                throw new InvalidDataException("Executor returned a crossed response.");
            byte[] bytes = BridgeJson.SerializeResponse(response);
            lifetime.Context.ThrowIfInactive();
            await FrameCodec.WriteAsync(pipe, bytes, BridgeLimits.MaxResponseBytes, CancellationToken.None).ConfigureAwait(false);
            lifetime.Context.ThrowIfInactive();
        }
        finally
        {
            monitorStop.Cancel(); pipe.Dispose();
            try { await monitor.ConfigureAwait(false); }
            finally
            {
                // A faulted disconnect monitor also finishes borrowing this lifetime.
                Observe(Retire(lifetime, ticket, retention, id, request.RequestId)); // never part of network shutdown
            }
        }
    }
    async Task Retire(RequestLifetime lifetime, DispatchTicket? ticket, RetainedRequestSlot retention, long id, string requestId)
    {
        // A pre-dispatch failure has no executor borrower. Retirement still awaits
        // notification work and disposes origin registrations before releasing capacity.
        ticket ??= new DispatchTicket(Task.FromResult(new BridgeResponse()), Task.CompletedTask);
        try { await lifetime.DisposeAfterQuiescenceAsync(ticket).ConfigureAwait(false); }
        catch (Exception error) { RecordFailure(id, requestId, error, "LifetimeCleanupFailed"); }
        finally
        {
            // A faulted Quiesced task also means the executor finished. Release after
            // successful physical disposal, independently of that logical outcome.
            var disposal = lifetime.DisposalCompleted;
            if (disposal.Status == TaskStatus.RanToCompletion) retention.Dispose();
            // Failed physical cleanup intentionally keeps its permit and reports failure.
        }
    }
    BridgeResponse Failure(BridgeError error) => new BridgeResponse
    { ContractVersion = 1, RequestId = error.RequestId, BridgeSessionId = descriptor.BridgeSessionId, Error = error };
    static async Task MonitorDisconnect(NamedPipeServerStream pipe, CancellationTokenSource disconnected, CancellationToken cancellation)
    {
        try { await pipe.ReadAsync(new byte[1], 0, 1, CancellationToken.None).ConfigureAwait(false); if (!cancellation.IsCancellationRequested) disconnected.Cancel(); }
        catch (Exception error) when (Expected(error)) { if (!cancellation.IsCancellationRequested) disconnected.Cancel(); }
    }
    static async Task<BridgeResponse> AwaitResponse(Task<BridgeResponse> response, CancellationToken cancellation)
    {
        var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellation.Register(() => canceled.TrySetResult(true)))
        {
            await Task.WhenAny(response, canceled.Task).ConfigureAwait(false);
            if (cancellation.IsCancellationRequested) { Observe(response); cancellation.ThrowIfCancellationRequested(); }
            return await response.ConfigureAwait(false);
        }
    }
    static void Observe(Task task) => task.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    static bool Expected(Exception error) => error is IOException || error is InvalidDataException || error is UnauthorizedAccessException ||
        error is OperationCanceledException || error is ObjectDisposedException || error is BridgeException || error is System.ComponentModel.Win32Exception || error is InvalidOperationException;
    void Emit(long id, string stage, long timestamp)
    { try { observe?.Invoke(new PipeEvent { ConnectionId = id, Stage = stage, Timestamp = timestamp }); } catch { /* Diagnostics cannot acquire ownership of an operation. */ } }
    void RecordCancellationFailure(long id, string requestId, Exception error)
        => RecordFailure(id, requestId, error, "CancellationCallbackFailed");
    void RecordFailure(long id, string requestId, Exception error, string stage)
    {
        var failure = new PipeEvent { ConnectionId = id, RequestId = requestId, Stage = stage, Error = error };
        // Retain the error independently of mutable data lent to the diagnostic observer.
        Interlocked.CompareExchange(ref callbackFailure, error, null);
        // This is a local diagnostic, not a late response to the expired request.
        try { failure.Timestamp = clock.Timestamp; observe?.Invoke(failure); } catch { }
    }
}
