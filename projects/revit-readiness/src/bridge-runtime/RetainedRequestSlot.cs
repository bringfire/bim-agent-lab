using System;
using System.Threading;

namespace BimAgentLab.RevitBridge.Runtime;

// Shared across endpoint restarts in this runtime/AppDomain. Network slots are separate.
internal sealed class RetainedRequestSlot : IDisposable
{
    const int Capacity = 8;
    static int retained;
    int released;
    RetainedRequestSlot() { }
    internal static RetainedRequestSlot? TryAcquire()
    {
        while (true)
        {
            int current = Volatile.Read(ref retained);
            if (current >= Capacity) return null;
            if (Interlocked.CompareExchange(ref retained, current + 1, current) == current)
                return new RetainedRequestSlot();
        }
    }
    public void Dispose()
    { if (Interlocked.Exchange(ref released, 1) == 0) Interlocked.Decrement(ref retained); }
}
