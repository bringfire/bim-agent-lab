namespace BimAgentLab.RevitReadiness.Mcp;

internal sealed class CancellableInputStream(Stream inner) : WrappedStream(inner)
{
    // Adapted from C# MCP SDK v2.2.0 StdioServerTransport.CancellableStdinStream
    // at 6fa3825973949a9c4f0cd8af344e15a8db09dc35. See third-party/mcp/README.md.
    // Unlike the upstream helper, an abandoned read owns a private buffer and
    // permanently closes admission to further reads. Canceling waiting is not
    // cancellation of the underlying Windows console I/O.
    int reading;
    bool abandoned;
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (abandoned || Interlocked.CompareExchange(ref reading, 1, 0) != 0)
            throw new IOException("Input has an abandoned or concurrent read.");
        Task<int>? pending = null;
        try
        {
            if (buffer.IsEmpty) return 0;
            byte[] owned = new byte[buffer.Length];
            pending = Inner.ReadAsync(owned.AsMemory(), CancellationToken.None).AsTask();
            int count = await pending.WaitAsync(ct).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            owned.AsMemory(0, count).CopyTo(buffer);
            return count;
        }
        catch
        {
            abandoned = true;
            if (pending is not null)
                _ = pending.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            throw;
        }
        finally { Volatile.Write(ref reading, 0); }
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
