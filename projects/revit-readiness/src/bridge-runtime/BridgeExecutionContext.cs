using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class BridgeExecutionContext
{
    readonly BridgeRequest request;
    readonly Func<bool> isCancellationRequested;
    readonly object activityGate;
    internal BridgeExecutionContext(BridgeRequest request, RequestBudget budget, CancellationToken cancellation,
        CancellationToken transportCancellation, Func<bool> isCancellationRequested, object activityGate)
    {
        this.request = Copy(request); Budget = budget; Cancellation = cancellation;
        TransportCancellation = transportCancellation; this.isCancellationRequested = isCancellationRequested; this.activityGate = activityGate;
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
    // Only bounded completion publication belongs here; never execute reader code.
    internal void PublishIfActive(TaskCompletionSource<BridgeResponse> completion, BridgeResponse response)
    {
        lock (activityGate)
        {
            ThrowIfInactive();
            completion.TrySetResult(response);
        }
    }
    static BridgeRequest Copy(BridgeRequest source) => new BridgeRequest
    {
        ContractVersion = source.ContractVersion, RequestId = source.RequestId, Operation = source.Operation,
        ContextToken = source.ContextToken, DeadlineUtc = source.DeadlineUtc
    };
}
