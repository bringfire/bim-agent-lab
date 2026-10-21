namespace BimAgentLab.RevitBridge.Contract;

public static class BridgeLimits
{
    public const int ContractVersion = 1;
    public const int MaxRequestBytes = 64 * 1024;
    public const int MaxResponseBytes = 4 * 1024 * 1024;
    public const int RequestSeconds = 30;
    public const string GetContext = "revit_get_readiness_context";
    public const string CheckInformation = "revit_check_required_type_information";
}
