namespace BimAgentLab.RevitBridge.Contract;

public sealed class BridgeRequest
{
    public int ContractVersion { get; set; }
    public string RequestId { get; set; } = "";
    public string Operation { get; set; } = "";
    public string? ContextToken { get; set; }
    public string DeadlineUtc { get; set; } = "";
}

public sealed class BridgeResponse
{
    public int ContractVersion { get; set; }
    public string RequestId { get; set; } = "";
    public string BridgeSessionId { get; set; } = "";
    public string? ChecklistSha256 { get; set; }
    public string? ResultJson { get; set; }
    public BridgeError? Error { get; set; }
}

public sealed class BridgeError
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string RequestId { get; set; } = "";
    public bool Retryable { get; set; }
}

public sealed class ReadinessContextResult
{
    public int RevitPid { get; set; }
    public string RevitBuild { get; set; } = "";
    public string DocumentLifetimeId { get; set; } = "";
    public string ProjectUniqueId { get; set; } = "";
    public string DocumentPath { get; set; } = "";
    public string ChecklistId { get; set; } = "";
    public string ChecklistPath { get; set; } = "";
    public string Scope { get; set; } = "";
    public string Level { get; set; } = "";
    public string Phase { get; set; } = "";
    public string ContextToken { get; set; } = "";
    public string ExpiresUtc { get; set; } = "";
}
