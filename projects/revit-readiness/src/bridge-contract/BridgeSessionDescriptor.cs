namespace BimAgentLab.RevitBridge.Contract;

public sealed class BridgeSessionDescriptor
{
    public int ContractVersion { get; set; }
    public int RevitPid { get; set; }
    public string ProcessStartUtc { get; set; } = "";
    public int WindowsSessionId { get; set; }
    public string LogonSid { get; set; } = "";
    public string BridgeSessionId { get; set; } = "";
    public string PipeName { get; set; } = "";
}
