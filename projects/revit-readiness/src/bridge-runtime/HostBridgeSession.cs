using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;

public interface IHostBridgeEndpoint
{
    void Start(Func<BridgeExecutionContext, DispatchTicket> submit);
    Task StopAsync();
}

public sealed class HostBridgeSession
{
    readonly int apiThread = Thread.CurrentThread.ManagedThreadId;
    readonly string sessionId;
    readonly Func<ApiEventLease> createEvent;
    readonly Func<IHostBridgeEndpoint> createEndpoint;
    readonly ConcurrentDictionary<BridgeExecutionContext, TaskCompletionSource<BridgeResponse>> responses = new();
    ApiEventLease? signal;
    DispatchQueue? queue;
    IHostBridgeEndpoint? endpoint;
    Task networkStop = Task.CompletedTask;
    bool started, stopped, eventRetired;
    public HostBridgeSession(string sessionId, IBridgeClock clock, Func<ApiEventLease> createEvent,
        Func<IHostBridgeEndpoint> createEndpoint)
    { this.sessionId = sessionId; this.createEvent = createEvent; this.createEndpoint = createEndpoint; Contexts = new ContextStore(sessionId, clock); }
    public ContextStore Contexts { get; }
    public bool IsRunning => started && !stopped;
    public bool IsRetired => stopped && eventRetired && networkStop.Status == TaskStatus.RanToCompletion;
    public void StartInApiContext()
    {
        RequireApiThread();
        if (stopped) throw new InvalidOperationException("Create a new bridge session after retirement.");
        if (started) return;
        try
        {
            signal = createEvent();
            queue = new DispatchQueue(sessionId, signal);
            endpoint = createEndpoint();
            endpoint.Start(Submit);
            started = true;
        }
        catch (Exception startFailure)
        {
            try { StopInApiContext(); }
            catch (Exception cleanupFailure) { throw new AggregateException(startFailure, cleanupFailure); }
            throw;
        }
    }
    public Task StopInApiContext()
    {
        RequireApiThread(); stopped = true;
        queue?.CancelAll(); Contexts.Clear();
        try
        {
            // A faulted network cleanup can be retried, but pending work is never duplicated.
            if (endpoint != null && (networkStop.IsCompleted || !started))
            {
                try { networkStop = endpoint.StopAsync(); }
                catch (Exception error) { networkStop = Task.FromException(error); }
            }
        }
        finally { eventRetired = signal == null || signal.TryRetireInApiContext(); }
        return networkStop;
    }
    public bool PollRetirementInApiContext()
    {
        RequireApiThread();
        if (!stopped) return false;
        if (!eventRetired) eventRetired = signal == null || signal.TryRetireInApiContext();
        if (networkStop.IsFaulted) throw networkStop.Exception!;
        return IsRetired;
    }
    public DispatchTicket Submit(BridgeExecutionContext context)
    {
        var completion = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!responses.TryAdd(context, completion)) throw new InvalidOperationException("Duplicate request context.");
        try
        {
            var ticket = (queue ?? throw new InvalidOperationException("Queue not created.")).Enqueue(context);
            _ = Observe(ticket, context, completion);
            return ticket;
        }
        catch { responses.TryRemove(context, out _); completion.TrySetCanceled(); throw; }
    }
    async Task Observe(DispatchTicket ticket, BridgeExecutionContext context, TaskCompletionSource<BridgeResponse> completion)
    {
        try { completion.TrySetResult(await ticket.Response.ConfigureAwait(false)); }
        catch (OperationCanceledException) { completion.TrySetCanceled(); }
        catch (Exception error) { completion.TrySetException(error); _ = completion.Task.Exception; }
        // Keep the lookup while the synchronous executor can still issue a lease.
        try { await ticket.Quiesced.ConfigureAwait(false); }
        catch { /* endpoint retains exceptional quiescence; this owner removes only its lookup */ }
        finally { responses.TryRemove(context, out _); }
    }
    public Task<BridgeResponse> ResponseFor(BridgeExecutionContext context) => responses[context].Task;
    public void DrainInApiContext(Func<BridgeExecutionContext, BridgeResponse> execute)
    { RequireApiThread(); if (IsRunning) queue!.Drain(4, execute); }
    public void RearmInApiContext() { RequireApiThread(); if (IsRunning) queue!.RearmIfNeeded(); }
    void RequireApiThread()
    { if (Thread.CurrentThread.ManagedThreadId != apiThread) throw new InvalidOperationException("Host lifecycle requires its API thread."); }
}
