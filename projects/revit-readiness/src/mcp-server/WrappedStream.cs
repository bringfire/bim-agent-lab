namespace BimAgentLab.RevitReadiness.Mcp;

internal class WrappedStream(Stream inner, bool leaveOpen = false) : Stream
{
    protected readonly Stream Inner = inner;
    public override bool CanRead => Inner.CanRead;
    public override bool CanWrite => Inner.CanWrite;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Flush() => Inner.Flush();
    public override Task FlushAsync(CancellationToken ct) => Inner.FlushAsync(ct);
    public override int Read(byte[] buffer, int offset, int count) => Inner.Read(buffer, offset, count);
    public override void Write(byte[] buffer, int offset, int count) => Inner.Write(buffer, offset, count);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        => ReadAsync(buffer.AsMemory(offset, count), ct).AsTask();
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) => Inner.ReadAsync(buffer, ct);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        => WriteAsync(buffer.AsMemory(offset, count), ct).AsTask();
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) => Inner.WriteAsync(buffer, ct);
    protected override void Dispose(bool disposing) { if (disposing && !leaveOpen) Inner.Dispose(); base.Dispose(disposing); }
}
