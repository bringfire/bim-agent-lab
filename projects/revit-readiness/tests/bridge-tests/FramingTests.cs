using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static byte[] Framed(byte[] payload)
    {
        // Independent little-endian framing oracle; do not use the production writer.
        int n = payload.Length;
        return new[] { (byte)n, (byte)(n >> 8), (byte)(n >> 16), (byte)(n >> 24) }.Concat(payload).ToArray();
    }
    static async Task FramingTests()
    {
        await Test("Fragmented headers and Unicode bodies larger than 8192 bytes survive", async () =>
        {
            byte[] payload = Utf8("{\"report\":\"" + string.Concat(Enumerable.Repeat("墙α🙂", 1800)) + "\"}");
            foreach (int chunk in new[] { 1, 3, 8192 })
            {
                using var stream = new FragmentedStream(Framed(payload), chunk);
                byte[] actual = await FrameCodec.ReadAsync(stream, 65536, CancellationToken.None);
                Check(payload.SequenceEqual(actual), "Payload altered or truncated");
            }
        });
        await Test("Coalesced frames leave the next frame unread", async () =>
        {
            using var stream = new MemoryStream(Framed(Utf8("{}" )).Concat(Framed(Utf8("[]"))).ToArray());
            byte[] first = await FrameCodec.ReadAsync(stream, 65536, CancellationToken.None);
            Equal("{}", System.Text.Encoding.UTF8.GetString(first));
            Equal(6L, stream.Position);
            byte[] second = await FrameCodec.ReadAsync(stream, 65536, CancellationToken.None);
            Equal("[]", System.Text.Encoding.UTF8.GetString(second));
        });
        await Test("Header and body EOF cannot become a complete payload", async () =>
        {
            foreach (var data in new[] { new byte[0], new byte[] { 2, 0 }, new byte[] { 2, 0, 0, 0, 123 } })
                using (var stream = new MemoryStream(data))
                    await ThrowsAsync<EndOfStreamException>(() => FrameCodec.ReadAsync(stream, 65536, CancellationToken.None));
        });
        await Test("Zero, over-limit and unsigned-overflow lengths fail before body reads", async () =>
        {
            foreach (var header in new[] { new byte[] { 0, 0, 0, 0 }, new byte[] { 1, 0, 1, 0 }, new byte[] { 255, 255, 255, 255 } })
            {
                using var stream = new MemoryStream(header);
                await ThrowsAsync<InvalidDataException>(() => FrameCodec.ReadAsync(stream, 65536, CancellationToken.None));
                Equal(4L, stream.Position);
            }
        });
        await Test("Request and response exact byte limits pass; one byte over writes nothing", async () =>
        {
            foreach (int limit in new[] { 65536, 4194304 })
            {
                var payload = Utf8("\"" + new string('a', limit - 2) + "\"");
                using var stream = new MemoryStream();
                await FrameCodec.WriteAsync(stream, payload, limit, CancellationToken.None);
                Check(stream.ToArray().SequenceEqual(Framed(payload)), "Wrong frame byte length/endian order");
                stream.Position = 0;
                byte[] actual = await FrameCodec.ReadAsync(stream, limit, CancellationToken.None);
                Check(payload.SequenceEqual(actual), "At-limit read truncated");
                using var untouched = new MemoryStream();
                await ThrowsAsync<InvalidDataException>(() => FrameCodec.WriteAsync(untouched, new byte[limit + 1], limit, CancellationToken.None));
                Equal(0L, untouched.Length);
            }
        });
        await Test("Invalid UTF-8 fails without replacement characters", async () =>
        {
            byte[] broken = { 34, 0xc3, 0x28, 34 };
            using var input = new MemoryStream(Framed(broken));
            await ThrowsAsync<InvalidDataException>(() => FrameCodec.ReadAsync(input, 65536, CancellationToken.None));
            using var output = new MemoryStream();
            await ThrowsAsync<InvalidDataException>(() => FrameCodec.WriteAsync(output, broken, 65536, CancellationToken.None));
            Equal(0L, output.Length);
        });
        await Test("Cancellation releases a blocked frame read and write", async () =>
        {
            using var stop = new CancellationTokenSource();
            using var input = new BlockedReadStream(); using var output = new BlockedWriteStream();
            var read = FrameCodec.ReadAsync(input, 65536, stop.Token);
            var write = FrameCodec.WriteAsync(output, Utf8("{}"), 65536, stop.Token);
            stop.Cancel();
            await ThrowsAsync<OperationCanceledException>(() => read);
            await ThrowsAsync<OperationCanceledException>(() => write);
        });
        await Test("Already canceled frame cannot touch output; caller retains stream ownership", async () =>
        {
            using var stream = new MemoryStream();
            await ThrowsAsync<OperationCanceledException>(() => FrameCodec.WriteAsync(stream, Utf8("{}"), 65536, new CancellationToken(true)));
            Equal(0L, stream.Length);
            await FrameCodec.WriteAsync(stream, Utf8("{}"), 65536, CancellationToken.None);
            stream.Position = 0;
            await FrameCodec.ReadAsync(stream, 65536, CancellationToken.None);
            stream.WriteByte(0); Check(stream.CanRead, "Codec disposed its caller-owned stream");
        });
    }
}
