using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BimAgentLab.RevitBridge.Runtime;

public static class FrameCodec
{
    internal static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static async Task<byte[]> ReadAsync(Stream stream, int limit, CancellationToken cancellation)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));
        var header = new byte[4];
        await ReadExactlyAsync(stream, header, cancellation).ConfigureAwait(false);
        uint length = (uint)header[0] | ((uint)header[1] << 8) | ((uint)header[2] << 16) | ((uint)header[3] << 24);
        if (length == 0 || length > limit) throw new InvalidDataException("Invalid bridge frame length.");
        var payload = new byte[checked((int)length)];
        await ReadExactlyAsync(stream, payload, cancellation).ConfigureAwait(false);
        ValidateUtf8(payload);
        cancellation.ThrowIfCancellationRequested();
        return payload;
    }

    public static async Task WriteAsync(Stream stream, byte[] payload, int limit, CancellationToken cancellation)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));
        cancellation.ThrowIfCancellationRequested();
        if (payload.Length == 0 || payload.Length > limit) throw new InvalidDataException("Invalid bridge frame length.");
        ValidateUtf8(payload); // Validate the whole payload before writing even its header.
        cancellation.ThrowIfCancellationRequested();
        int length = payload.Length;
        var header = new[] { (byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24) };
        await stream.WriteAsync(header, 0, header.Length, cancellation).ConfigureAwait(false);
        cancellation.ThrowIfCancellationRequested();
        await stream.WriteAsync(payload, 0, payload.Length, cancellation).ConfigureAwait(false);
        cancellation.ThrowIfCancellationRequested();
    }

    static async Task ReadExactlyAsync(Stream stream, byte[] target, CancellationToken cancellation)
    {
        int offset = 0;
        while (offset < target.Length)
        {
            cancellation.ThrowIfCancellationRequested();
            int count = await stream.ReadAsync(target, offset, target.Length - offset, cancellation).ConfigureAwait(false);
            cancellation.ThrowIfCancellationRequested();
            if (count == 0) throw new EndOfStreamException("Incomplete bridge frame.");
            offset += count;
        }
    }

    internal static void ValidateUtf8(byte[] payload)
    {
        try { StrictUtf8.GetCharCount(payload); }
        catch (DecoderFallbackException error) { throw new InvalidDataException("Bridge payload is not valid UTF-8.", error); }
    }
}
