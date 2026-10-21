namespace BimAgentLab.RevitReadiness.Mcp;

internal sealed class TraceStream(Stream inner, Stream evidence, bool leaveOpen,
    Action<Exception>? evidenceFailed = null) : WrappedStream(inner, leaveOpen)
{
    internal const long MaxEvidenceBytes = 8 * 1024 * 1024;
    long bytes;
    async ValueTask Capture(ReadOnlyMemory<byte> buffer, CancellationToken ct)
    {
        try
        {
            if (buffer.Length > MaxEvidenceBytes - bytes) throw new IOException("Wire evidence limit exceeded.");
            await evidence.WriteAsync(buffer, ct).ConfigureAwait(false);
            await evidence.FlushAsync(ct).ConfigureAwait(false);
            bytes += buffer.Length;
        }
        catch (Exception error) { evidenceFailed?.Invoke(error); throw; }
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        int count = await Inner.ReadAsync(buffer, ct).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        if (count > 0) await Capture(buffer[..count], ct).ConfigureAwait(false);
        return count;
    }
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
    {
        // Check capacity before emitting any new output, then retain exactly the bytes written.
        if (buffer.Length > MaxEvidenceBytes - bytes)
        {
            var error = new IOException("Wire evidence limit exceeded.");
            evidenceFailed?.Invoke(error);
            throw error;
        }
        await Inner.WriteAsync(buffer, ct).ConfigureAwait(false);
        await Capture(buffer, ct).ConfigureAwait(false);
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
