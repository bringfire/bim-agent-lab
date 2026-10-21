using System;
namespace BimAgentLab.RevitBridge.Runtime;

public sealed class ApiEventLease : IDispatchSignal
{
    readonly object gate = new object();
    readonly Func<DispatchSignalResult> raise;
    readonly Action dispose;
    int borrowers;
    bool closing, disposed;
    public ApiEventLease(Func<DispatchSignalResult> raise, Action dispose)
    { this.raise = raise ?? throw new ArgumentNullException(nameof(raise)); this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose)); }
    public DispatchSignalResult Raise()
    {
        lock (gate) { if (closing) return DispatchSignalResult.Denied; borrowers++; }
        try { return raise(); }
        finally { lock (gate) borrowers--; }
    }
    // Sole API-thread owner calls this. Never wait for a transport-thread Raise.
    public bool TryRetireInApiContext()
    {
        lock (gate) { closing = true; if (disposed) return true; if (borrowers != 0) return false; }
        dispose(); // no lock across native code; a failed disposal may be retried
        lock (gate) disposed = true;
        return true;
    }
}
