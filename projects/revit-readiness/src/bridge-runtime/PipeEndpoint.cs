using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;
public sealed class PipeEvent
{
    public long ConnectionId { get; set; }
    public string Stage { get; set; } = "";
    public long Timestamp { get; set; }
}
public sealed class PipeEndpoint
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
        lock (gate)
        {
            if (stop == null || stop.IsFaulted) { stopping.Cancel(); stop = FinishStop(); }
            return stop;
        }
    }
    async Task FinishStop()
    {
        await Task.WhenAll(workers).ConfigureAwait(false); // network workers, never Revit callbacks
        RemoveDescriptor(); // failure stays observable; foreign descriptor is preserved
        stopping.Dispose();
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
                    try { await Handle(pipe, accepted, submit).ConfigureAwait(false); }
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
    async Task Handle(NamedPipeServerStream pipe, long accepted, Func<BridgeExecutionContext, DispatchTicket> submit)
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
        frame.CancelAfter(Timeout.Infinite);
        using var disconnected = new CancellationTokenSource();
        var lifetime = new RequestLifetime(request, budget, disconnected.Token, stopping.Token);
        DispatchTicket? ticket = null;
        using var monitorStop = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token);
        var monitor = MonitorDisconnect(pipe, disconnected, monitorStop.Token);
        try
        {
            using var closeOnCancel = lifetime.Context.Cancellation.Register(pipe.Dispose);
            BridgeResponse response;
            try
            {
                lifetime.Context.ThrowIfInactive(); ticket = submit(lifetime.Context);
                response = await AwaitResponse(ticket.Response, lifetime.Context.Cancellation).ConfigureAwait(false);
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
            monitorStop.Cancel(); pipe.Dispose(); await monitor.ConfigureAwait(false);
            if (ticket == null) lifetime.Dispose();
            else Observe(lifetime.DisposeAfterQuiescenceAsync(ticket)); // callback owns its remaining use, not network slot
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
}
