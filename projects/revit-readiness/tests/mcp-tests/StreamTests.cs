using BimAgentLab.RevitReadiness.Mcp;
using System.Text;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    static async Task TraceBytes()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{\"text\":\"墙面 – ceiling\"}\n");
        using var evidence = new MemoryStream();
        using var output = new MemoryStream();
        using (var trace = new TraceStream(output, evidence, true))
        {
            await trace.WriteAsync(bytes.AsMemory(0, 7));
            await trace.WriteAsync(bytes, 7, bytes.Length - 7);
            await trace.FlushAsync();
        }
        Check(bytes.SequenceEqual(output.ToArray()), "Tracing changed output bytes");
        Check(bytes.SequenceEqual(evidence.ToArray()), "Tracing dropped or changed evidence");
        Check(output.CanWrite && evidence.CanWrite, "Wrong evidence/stream owner");
        using var input = new MemoryStream(bytes);
        using var inputEvidence = new MemoryStream();
        using var reader = new TraceStream(input, inputEvidence, true);
        using var read = new MemoryStream();
        await reader.CopyToAsync(read);
        Check(bytes.SequenceEqual(read.ToArray()) && bytes.SequenceEqual(inputEvidence.ToArray()), "Input trace differs");
    }

    static async Task CancelPendingRead(bool arrayOverload)
    {
        using var raw = new UncooperativeInput();
        using var evidence = new MemoryStream();
        using var adapter = new CancellableInputStream(raw);
        using var trace = new TraceStream(adapter, evidence, true);
        using var stop = new CancellationTokenSource();
        var buffer = Enumerable.Repeat((byte)7, 4).ToArray();
        Task<int> read = arrayOverload ? trace.ReadAsync(buffer, 0, buffer.Length, stop.Token)
            : trace.ReadAsync(buffer.AsMemory(), stop.Token).AsTask();
        await raw.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        stop.Cancel();
        var canceled = false;
        try { await read.WaitAsync(TimeSpan.FromMilliseconds(400)); }
        catch (OperationCanceledException) { canceled = true; }
        catch (TimeoutException) { }
        // Finish the real outstanding read even on the expected red run.
        raw.Finish();
        try { await read; } catch (OperationCanceledException) { }
        Check(canceled, "Canceled waiting remained blocked on raw input");
        Check(buffer.All(b => b == 7), "Abandoned read wrote into caller-owned buffer");
        Check(evidence.Length == 0, "Late canceled input was written to evidence");
        try { _ = await adapter.ReadAsync(new byte[4]); throw new Exception("Abandoned input reused"); }
        catch (IOException) { }
    }

    static async Task TraceLimit()
    {
        using var sink = new MemoryStream();
        using var trace = new TraceStream(Stream.Null, sink, true);
        byte[] chunk = new byte[1024 * 1024];
        for (int i = 0; i < 8; i++) await trace.WriteAsync(chunk);
        bool failed = false;
        try { await trace.WriteAsync(new byte[1]); } catch (IOException) { failed = true; }
        Check(failed && sink.Length == TraceStream.MaxEvidenceBytes, "Full trace was silently truncated or unbounded");
    }

    sealed class UncooperativeInput : WrappedStream
    {
        readonly TaskCompletionSource<int> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Memory<byte> pending;
        public UncooperativeInput() : base(Stream.Null, true) { }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        { pending = buffer; Started.TrySetResult(); return new(result.Task); }
        internal void Finish() { pending.Span.Fill(99); result.TrySetResult(pending.Length); }
    }
}
