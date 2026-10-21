using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;
public sealed class BridgeClientProvider : IBridgeClient
{
    readonly IBridgeSessionCatalog catalog;
    readonly int? revitPid;
    readonly SemaphoreSlim binding = new SemaphoreSlim(1, 1);
    IBridgeClient? bound;
    public BridgeClientProvider(IBridgeSessionCatalog catalog, int? revitPid, IBridgeClock clock)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); this.revitPid = revitPid;
        _ = clock ?? throw new ArgumentNullException(nameof(clock)); // caller's existing budget remains authoritative
    }
    public async Task<BridgeResponse> SendAsync(BridgeExecutionContext execution)
    {
        execution.ThrowIfInactive();
        await binding.WaitAsync(execution.TransportCancellation).ConfigureAwait(false);
        IBridgeClient client;
        try
        {
            execution.ThrowIfInactive();
            if (bound == null)
            {
                var descriptor = catalog.Resolve(revitPid);
                execution.ThrowIfInactive(); var candidate = new PipeBridgeClient(descriptor); execution.ThrowIfInactive();
                bound = candidate;
            }
            client = bound;
        }
        catch (BridgeException error)
        {
            execution.ThrowIfInactive();
            throw BridgeFailures.Create(error.Error.Code, error.Error.Message, execution.Request.RequestId);
        }
        finally { binding.Release(); }
        return await client.SendAsync(execution).ConfigureAwait(false);
    }
}
