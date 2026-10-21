using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class RequestLifetime : IDisposable
{
    readonly CancellationTokenSource cancellation;
    readonly CancellationTokenSource deadlineStopping = new CancellationTokenSource();
    readonly TaskCompletionSource<bool> disposalCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly object gate = new object();
    readonly TaskCompletionSource<bool> cancellationReleased = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly Action<Exception>? reportCancellationFailure;
    CancellationTokenRegistration upstreamRegistration, ownerRegistration;
    Task deadlineWork = Task.CompletedTask;
    Exception? cancellationFailure;
    int activeCancellations;
    int disposed;
    public RequestLifetime(BridgeRequest request, RequestBudget budget, CancellationToken upstreamCancellation, CancellationToken ownerStopping)
        : this(request, budget, upstreamCancellation, ownerStopping, null) { }
    internal RequestLifetime(BridgeRequest request, RequestBudget budget, CancellationToken upstreamCancellation, CancellationToken ownerStopping, Action<Exception>? reportCancellationFailure)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (budget == null) throw new ArgumentNullException(nameof(budget));
        BridgeJson.ValidateRequest(request);
        if (BridgeJson.ParseUtc(request.DeadlineUtc) != budget.DeadlineUtc)
            throw new ArgumentException("The wire deadline must come from the supplied request budget.", nameof(request));
        cancellation = new CancellationTokenSource();
        this.reportCancellationFailure = reportCancellationFailure;
        try
        {
            Context = new BridgeExecutionContext(request, budget, cancellation.Token);
            // Explicit registrations contain borrowed callback failures before they can
            // escape onto an upstream owner's thread (which may itself be a timer).
            upstreamRegistration = upstreamCancellation.Register(CancelOwned);
            ownerRegistration = ownerStopping.Register(CancelOwned);
            Context.ThrowIfInactive();
            // CancelAfter would run borrowed callbacks directly on a timer thread.
            // Own the asynchronous deadline operation so its exceptions are contained.
            deadlineWork = WatchDeadlineAsync();
        }
        catch { Dispose(); throw; }
    }
    public BridgeExecutionContext Context { get; }
    // Kept as data, never an unobserved faulted timer task. Available after disposal.
    public Exception? CancellationFailure => Volatile.Read(ref cancellationFailure);
    void CancelOwned()
    {
        lock (gate)
        {
            if (Volatile.Read(ref disposed) != 0) return;
            activeCancellations++;
        }
        try { cancellation.Cancel(); }
        catch (Exception error) { ReportFailure(error); }
        finally
        {
            lock (gate)
            {
                activeCancellations--;
                if (disposed != 0 && activeCancellations == 0) cancellationReleased.TrySetResult(true);
            }
        }
    }
    void ReportFailure(Exception error)
    {
        Interlocked.CompareExchange(ref cancellationFailure, error, null);
        try { reportCancellationFailure?.Invoke(error); }
        catch (Exception reportingError)
        { Volatile.Write(ref cancellationFailure, new AggregateException(error, reportingError)); }
    }
    async Task WatchDeadlineAsync()
    {
        try
        {
            while (true)
            {
                deadlineStopping.Token.ThrowIfCancellationRequested();
                TimeSpan remaining = Context.Budget.Remaining;
                if (remaining <= TimeSpan.Zero) break;
                // Recheck the SAME budget after wakeup; never create a new allowance.
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Ceiling(remaining.TotalMilliseconds)), deadlineStopping.Token).ConfigureAwait(false);
            }
            deadlineStopping.Token.ThrowIfCancellationRequested();
            CancelOwned(); // all callbacks run; their aggregate stays inside this lifetime
        }
        catch (OperationCanceledException) when (deadlineStopping.IsCancellationRequested) { }
        catch (Exception error) { ReportFailure(error); }
    }
    public async Task DisposeAfterQuiescenceAsync(DispatchTicket ticket)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));
        // Call from the owner's final cleanup, after its own IPC/publication work.
        // Response may already be canceled while its executor still borrows Context.
        try { await ticket.Quiesced.ConfigureAwait(false); }
        finally { Dispose(); await disposalCompleted.Task.ConfigureAwait(false); }
    }
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        lock (gate) { if (activeCancellations == 0) cancellationReleased.TrySetResult(true); }
        deadlineStopping.Cancel(); // only the owned delay is registered on this source
        _ = FinishDisposalAsync(); // nonblocking even if called by a deadline callback itself
    }
    async Task FinishDisposalAsync()
    {
        try
        {
            // Quiesced releases the executor; cancellationReleased releases borrowed callbacks.
            // Neither logical completion alone permits destroying the shared source.
            await deadlineWork.ConfigureAwait(false);
            await cancellationReleased.Task.ConfigureAwait(false);
            upstreamRegistration.Dispose(); ownerRegistration.Dispose();
            cancellation.Dispose(); deadlineStopping.Dispose();
            disposalCompleted.TrySetResult(true);
        }
        catch (Exception error) { disposalCompleted.TrySetException(error); }
    }
}
