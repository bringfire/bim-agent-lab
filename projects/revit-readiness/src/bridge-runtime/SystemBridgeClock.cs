using System;
using System.Diagnostics;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class SystemBridgeClock : IBridgeClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public long Timestamp => Stopwatch.GetTimestamp();
    public TimeSpan ElapsedSince(long timestamp) => TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency);
}
