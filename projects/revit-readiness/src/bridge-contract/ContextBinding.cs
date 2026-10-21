using System;

namespace BimAgentLab.RevitBridge.Contract;

public sealed class ContextBinding
{
    public string BridgeSessionId { get; set; } = "";
    public string DocumentLifetimeId { get; set; } = "";
    public string ProjectUniqueId { get; set; } = "";
    public string DocumentPath { get; set; } = "";
    public string ChecklistId { get; set; } = "";
    public string ChecklistPath { get; set; } = "";
    public string ChecklistSha256 { get; set; } = "";
}

public sealed class ContextLease
{
    public string Token { get; }
    public DateTimeOffset ExpiresUtc { get; }
    public ContextLease(string token, DateTimeOffset expiresUtc) { Token = token; ExpiresUtc = expiresUtc; }
}
