using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace BimAgentLab.RevitBridge.Runtime;

public sealed class ChecklistSnapshot
{
    public const int MaxBytes = 4 * 1024 * 1024;
    readonly byte[] bytes;
    ChecklistSnapshot(byte[] bytes)
    {
        this.bytes = bytes;
        using var hash = SHA256.Create();
        Sha256 = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
    public string Sha256 { get; }
    public string Text => new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF');
    public static ChecklistSnapshot Read(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Capture(stream);
    }
    internal static ChecklistSnapshot Capture(Stream stream)
    {
        if (stream.CanSeek && stream.Length > MaxBytes) throw new InvalidDataException("Checklist exceeds four MiB.");
        using var output = new MemoryStream();
        var buffer = new byte[8192]; int count;
        while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            if (output.Length + count > MaxBytes) throw new InvalidDataException("Checklist grew beyond four MiB.");
            output.Write(buffer, 0, count);
        }
        if (output.Length == 0) throw new InvalidDataException("Checklist is empty.");
        return new ChecklistSnapshot(output.ToArray());
    }
}
