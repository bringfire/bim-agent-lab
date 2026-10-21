using System;
using System.Threading;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class BridgeExecutionContext
{
    readonly BridgeRequest request;
    internal BridgeExecutionContext(BridgeRequest request, RequestBudget budget, CancellationToken cancellation)
    { this.request = Copy(request); Budget = budget; Cancellation = cancellation; }
    // Plain wire DTOs are mutable. Never lend the private snapshot to a consumer.
    public BridgeRequest Request => Copy(request);
    public RequestBudget Budget { get; }
    public CancellationToken Cancellation { get; }
    public void ThrowIfInactive()
    {
        if (Budget.IsExpired) throw new BridgeException(new BridgeError
        { Code = "DeadlineExceeded", Message = "The request deadline has expired.", RequestId = request.RequestId, Retryable = false });
        Cancellation.ThrowIfCancellationRequested();
    }
    static BridgeRequest Copy(BridgeRequest source) => new BridgeRequest
    {
        ContractVersion = source.ContractVersion, RequestId = source.RequestId, Operation = source.Operation,
        ContextToken = source.ContextToken, DeadlineUtc = source.DeadlineUtc
    };
}
