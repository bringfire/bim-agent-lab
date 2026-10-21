using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
namespace BimAgentLab.RevitBridge.Runtime;
public interface IBridgeClient
{
    Task<BridgeResponse> SendAsync(BridgeExecutionContext execution);
}
public sealed class PipeBridgeClient : IBridgeClient
{
    readonly BridgeSessionDescriptor descriptor;
    public PipeBridgeClient(BridgeSessionDescriptor descriptor)
    {
        BridgeIdentity.ValidateLive(descriptor);
        this.descriptor = BridgeJson.Deserialize<BridgeSessionDescriptor>(BridgeJson.Serialize(descriptor));
    }
    public async Task<BridgeResponse> SendAsync(BridgeExecutionContext execution)
    {
        execution.ThrowIfInactive(); var request = execution.Request;
        try
        {
            BridgeIdentity.ValidateLive(descriptor); execution.ThrowIfInactive();
            using var pipe = new NamedPipeClientStream(".", descriptor.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Identification);
            // The existing lifetime cancels sooner when its budget is shorter. Avoid
            // converting that deadline to a second, coarse Windows connect timeout.
            await pipe.ConnectAsync(2000, execution.TransportCancellation).ConfigureAwait(false);
            execution.ThrowIfInactive();
            // One cancellation owner after connect. Do not race close with a second
            // framework CancelIoEx callback on the same handle (net48).
            using var cancelPipe = execution.TransportCancellation.Register(pipe.Dispose);
            NativeWindows.ValidateServer(pipe, descriptor.RevitPid); BridgeIdentity.ValidateLive(descriptor);
            byte[] requestBytes = BridgeJson.Serialize(request); execution.ThrowIfInactive();
            await FrameCodec.WriteAsync(pipe, requestBytes, BridgeLimits.MaxRequestBytes, CancellationToken.None).ConfigureAwait(false);
            var bytes = await FrameCodec.ReadAsync(pipe, BridgeLimits.MaxResponseBytes, CancellationToken.None).ConfigureAwait(false);
            var response = BridgeJson.Deserialize<BridgeResponse>(bytes);
            if (response.RequestId != request.RequestId || response.BridgeSessionId != descriptor.BridgeSessionId)
                throw new InvalidDataException("Crossed bridge response.");
            execution.ThrowIfInactive(); return response;
        }
        catch (Exception error) when (error is IOException || error is InvalidDataException || error is TimeoutException ||
            error is UnauthorizedAccessException || error is ObjectDisposedException || error is System.ComponentModel.Win32Exception || error is BridgeException)
        {
            execution.ThrowIfInactive();
            throw BridgeFailures.Create("BridgeUnavailable", "The bound bridge did not return a valid correlated response. Reconnect explicitly if it restarted.", request.RequestId);
        }
        catch (OperationCanceledException) { execution.ThrowIfInactive(); throw; }
    }
}
