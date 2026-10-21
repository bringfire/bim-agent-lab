using System;
using System.Threading;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class BridgeExecutionContext
{
    readonly BridgeRequest request;
    readonly Func<bool> isCancellationRequested;
    internal BridgeExecutionContext(BridgeRequest request, RequestBudget budget, CancellationToken cancellation,
        CancellationToken transportCancellation, Func<bool> isCancellationRequested)
    {
        this.request = Copy(request); Budget = budget; Cancellation = cancellation;
        TransportCancellation = transportCancellation; this.isCancellationRequested = isCancellationRequested;
    }
    // Plain wire DTOs are mutable. Never lend the private snapshot to a consumer.
    public BridgeRequest Request => Copy(request);
    public RequestBudget Budget { get; }
    public CancellationToken Cancellation { get; }
    // Immediate invalidation and cooperative callback notification are separate facts.
    public bool IsCancellationRequested => isCancellationRequested();
    internal CancellationToken TransportCancellation { get; }
    public void ThrowIfInactive()
    {
        if (Budget.IsExpired) throw new BridgeException(new BridgeError
        { Code = "DeadlineExceeded", Message = "The request deadline has expired.", RequestId = request.RequestId, Retryable = false });
        if (IsCancellationRequested) throw new OperationCanceledException("The bridge request was canceled.", Cancellation);
    }
    static BridgeRequest Copy(BridgeRequest source) => new BridgeRequest
    {
        ContractVersion = source.ContractVersion, RequestId = source.RequestId, Operation = source.Operation,
        ContextToken = source.ContextToken, DeadlineUtc = source.DeadlineUtc
    };
}
