using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Runtime;
namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    sealed class GrowingChecklist : MemoryStream
    {
        public GrowingChecklist(byte[] bytes) : base(bytes) { }
        public override long Length => 1; // observed length is stale, actual read still exceeds limit
    }
    static async Task ChecklistSnapshotTests()
    {
        await Test("Checklist snapshot hashes precisely the bytes decoded, including BOM", () => Sync(() =>
        {
            byte[] bytes = new UTF8Encoding(true).GetPreamble(); byte[] text = Utf8("{\"value\":\"墙 /Date(0)/\"}");
            var input = new byte[bytes.Length + text.Length]; bytes.CopyTo(input, 0); text.CopyTo(input, bytes.Length);
            using var stream = new MemoryStream(input); var captured = ChecklistSnapshot.Capture(stream);
            Equal(Encoding.UTF8.GetString(text), captured.Text);
            using var sha = SHA256.Create(); Equal(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant(), captured.Sha256);
            input[0] = 0; Equal(Encoding.UTF8.GetString(text), captured.Text);
        }));
        await Test("Checklist rejects initial oversize and growth beyond four MiB", () => Sync(() =>
        {
            byte[] bytes = new byte[ChecklistSnapshot.MaxBytes + 1];
            using var large = new MemoryStream(bytes); Throws<InvalidDataException>(() => ChecklistSnapshot.Capture(large));
            using var growing = new GrowingChecklist(bytes); Throws<InvalidDataException>(() => ChecklistSnapshot.Capture(growing));
            using var empty = new MemoryStream(); Throws<InvalidDataException>(() => ChecklistSnapshot.Capture(empty));
        }));
        await Test("Checklist UTF-8 is strict and exact size boundary remains legal", () => Sync(() =>
        {
            using var valid = new MemoryStream(new byte[ChecklistSnapshot.MaxBytes]); Equal(ChecklistSnapshot.MaxBytes, ChecklistSnapshot.Capture(valid).Text.Length);
            using var invalid = new MemoryStream(new byte[] { 0xff }); Throws<DecoderFallbackException>(() => { _ = ChecklistSnapshot.Capture(invalid).Text; });
        }));
    }
}
