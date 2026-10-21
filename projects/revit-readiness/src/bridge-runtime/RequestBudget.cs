using System;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public interface IBridgeClock
{
    DateTimeOffset UtcNow { get; }
    long Timestamp { get; }
    TimeSpan ElapsedSince(long timestamp);
}

public sealed class RequestBudget
{
    readonly IBridgeClock clock;
    readonly long started;
    readonly TimeSpan allowance;
    RequestBudget(IBridgeClock clock, long started, TimeSpan allowance, DateTimeOffset deadline)
    { this.clock = clock; this.started = started; this.allowance = allowance; DeadlineUtc = deadline; }
    public TimeSpan Remaining
    {
        get
        {
            TimeSpan remaining = allowance - clock.ElapsedSince(started);
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
    public bool IsExpired => Remaining <= TimeSpan.Zero;
    public DateTimeOffset DeadlineUtc { get; }
    public static RequestBudget Admit(IBridgeClock clock)
    {
        if (clock == null) throw new ArgumentNullException(nameof(clock));
        long started = clock.Timestamp;
        var allowance = TimeSpan.FromSeconds(BridgeLimits.RequestSeconds);
        return new RequestBudget(clock, started, allowance, clock.UtcNow + allowance);
    }
    public static RequestBudget Receive(IBridgeClock clock, long acceptedTimestamp, DateTimeOffset deadlineUtc)
    {
        if (clock == null) throw new ArgumentNullException(nameof(clock));
        long started = clock.Timestamp;
        TimeSpan policy = TimeSpan.FromSeconds(BridgeLimits.RequestSeconds);
        TimeSpan elapsed = clock.ElapsedSince(acceptedTimestamp);
        TimeSpan supplied = deadlineUtc - clock.UtcNow;
        if (deadlineUtc.Offset != TimeSpan.Zero || supplied > policy || elapsed < TimeSpan.Zero)
            throw Failure("InvalidRequest", "Deadline is outside the bridge policy.");
        TimeSpan cap = policy - elapsed;
        if (supplied <= TimeSpan.Zero || cap <= TimeSpan.Zero)
            throw Failure("DeadlineExceeded", "The request deadline has expired.");
        return new RequestBudget(clock, started, supplied < cap ? supplied : cap, deadlineUtc);
    }
    // At admission/receipt the caller adds its validated request identity before publication.
    static BridgeException Failure(string code, string message) => new BridgeException(new BridgeError
    { Code = code, Message = message, RequestId = "", Retryable = false });
}
