using System;

namespace BimAgentLab.RevitBridge.Contract;

public sealed class BridgeException : Exception
{
    public BridgeError Error { get; }
    public BridgeException(BridgeError error) : base(error?.Message)
    { Error = error ?? throw new ArgumentNullException(nameof(error)); }
}
