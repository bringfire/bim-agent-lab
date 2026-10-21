using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class DispatchTicket
{
    public Task<BridgeResponse> Response { get; }
    public Task Quiesced { get; }
    internal DispatchTicket(Task<BridgeResponse> response, Task quiesced)
    { Response = response; Quiesced = quiesced; }
}
