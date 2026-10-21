using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class RequestLifetime : IDisposable
{
    readonly CancellationTokenSource cancellation;
    int disposed;
    public RequestLifetime(BridgeRequest request, RequestBudget budget, CancellationToken upstreamCancellation, CancellationToken ownerStopping)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (budget == null) throw new ArgumentNullException(nameof(budget));
        BridgeJson.ValidateRequest(request);
        if (BridgeJson.ParseUtc(request.DeadlineUtc) != budget.DeadlineUtc)
            throw new ArgumentException("The wire deadline must come from the supplied request budget.", nameof(request));
        cancellation = CancellationTokenSource.CreateLinkedTokenSource(upstreamCancellation, ownerStopping);
        try
        {
            Context = new BridgeExecutionContext(request, budget, cancellation.Token);
            Context.ThrowIfInactive();
            // CancelAfter truncates fractional milliseconds. Round up so its wakeup
            // cannot precede the monotonic deadline and masquerade as caller cancellation.
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(Math.Ceiling(budget.Remaining.TotalMilliseconds)));
        }
        catch { cancellation.Dispose(); throw; }
    }
    public BridgeExecutionContext Context { get; }
    public async Task DisposeAfterQuiescenceAsync(DispatchTicket ticket)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));
        // Call from the owner's final cleanup, after its own IPC/publication work.
        // Response may already be canceled while its executor still borrows Context.
        try { await ticket.Quiesced.ConfigureAwait(false); }
        finally { Dispose(); }
    }
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0) cancellation.Dispose();
    }
}
