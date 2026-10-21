using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;

public static class ReadinessExecutionFlow
{
    public static BridgeResponse Run<TPrepared, TReport>(BridgeExecutionContext context,
        Func<TPrepared> prepare, Func<TPrepared, TReport> read, Func<TReport, BridgeResponse> serialize)
    {
        context.ThrowIfInactive();
        var prepared = prepare();
        context.ThrowIfInactive();
        var report = read(prepared);
        context.ThrowIfInactive();
        var response = serialize(report);
        context.ThrowIfInactive();
        return response;
    }

    public static BridgeResponse IssueContext(BridgeExecutionContext context, ContextStore store,
        ContextBinding binding, Task<BridgeResponse> publication, Func<ContextLease, BridgeResponse> serialize)
    {
        context.ThrowIfInactive();
        var lease = store.Issue(binding);
        try
        {
            context.ThrowIfInactive();
            var response = serialize(lease);
            context.ThrowIfInactive();
            // Only plain store/token data is captured, never a Document. Queue publication
            // can still lose to cancellation after this synchronous method returns.
            _ = publication.ContinueWith(done =>
            {
                if (done.Status != TaskStatus.RanToCompletion || done.Result.Error != null)
                { _ = done.Exception; store.Revoke(lease.Token); }
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
            return response;
        }
        catch { store.Revoke(lease.Token); throw; }
    }
}
