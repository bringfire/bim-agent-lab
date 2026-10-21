using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public interface IBridgeSessionCatalog
{
    BridgeSessionDescriptor Resolve(int? revitPid);
}

public sealed class SessionCatalog : IBridgeSessionCatalog
{
    readonly string? directory;
    readonly Func<IReadOnlyList<BridgeSessionDescriptor>> discover;
    readonly Dictionary<string, byte[]> owned = new Dictionary<string, byte[]>();
    public SessionCatalog() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BimAgentLab", "RevitBridge", "sessions")) { }
    internal SessionCatalog(string directory) { this.directory = Path.GetFullPath(directory); discover = Discover; }
    internal SessionCatalog(Func<IReadOnlyList<BridgeSessionDescriptor>> discoverValidated) { discover = discoverValidated; }
    public BridgeSessionDescriptor Resolve(int? revitPid)
    {
        var candidates = discover().Where(d => !revitPid.HasValue || d.RevitPid == revitPid.Value).ToArray();
        if (candidates.Length == 0) throw BridgeFailures.Create("BridgeUnavailable", "Start a bridge in the selected Revit instance.");
        if (candidates.Length != 1) throw BridgeFailures.Create("AmbiguousRevitInstance", "Select exactly one active Revit bridge instance.");
        return BridgeJson.Deserialize<BridgeSessionDescriptor>(BridgeJson.Serialize(candidates[0]));
    }
    IReadOnlyList<BridgeSessionDescriptor> Discover()
    {
        var found = new List<BridgeSessionDescriptor>();
        if (!Directory.Exists(directory)) return found;
        try
        {
            foreach (string path in Directory.EnumerateFiles(directory!, "*.json"))
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (stream.Length == 0 || stream.Length > 8192) continue;
                    var bytes = new byte[(int)stream.Length]; int offset = 0;
                    while (offset < bytes.Length) { int n = stream.Read(bytes, offset, bytes.Length - offset); if (n == 0) break; offset += n; }
                    if (offset != bytes.Length) continue;
                    var descriptor = BridgeJson.Deserialize<BridgeSessionDescriptor>(bytes);
                    if (Path.GetFileName(path) != descriptor.BridgeSessionId + ".json") continue;
                    NativeWindows.ValidateFileAccess(path, descriptor.LogonSid);
                    BridgeIdentity.ValidateLive(descriptor); found.Add(descriptor);
                }
                catch (Exception error) when (error is IOException || error is InvalidDataException || error is UnauthorizedAccessException || error is BridgeException || error is System.Security.SecurityException) { }
            }
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
        { throw BridgeFailures.Create("BridgeUnavailable", "Cannot inspect the local bridge catalog."); }
        return found;
    }
    public void Publish(BridgeSessionDescriptor descriptor)
    {
        BridgeIdentity.ValidateLive(descriptor);
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        if (descriptor.RevitPid != process.Id || directory == null) throw new InvalidOperationException("Only the owning process can publish a session.");
        byte[] bytes = BridgeJson.Serialize(descriptor);
        string name = descriptor.BridgeSessionId + ".json";
        lock (owned)
        {
            Directory.CreateDirectory(directory);
            string pending = Path.Combine(directory, ".pending-" + Guid.NewGuid().ToString("N"));
            try
            {
                NativeWindows.CreateRestrictedFile(pending, bytes, descriptor.LogonSid);
                File.Move(pending, Path.Combine(directory, name)); // no overwrite; published last, atomically
                owned.Add(name, bytes);
            }
            finally { if (File.Exists(pending)) File.Delete(pending); }
        }
    }
    public void RemoveOwned(BridgeSessionDescriptor descriptor)
    {
        BridgeIdentity.ValidateShape(descriptor);
        string name = descriptor.BridgeSessionId + ".json";
        lock (owned)
        {
            if (!owned.TryGetValue(name, out var expected)) return;
            NativeWindows.DeleteOwnedFile(Path.Combine(directory!, name), expected);
            owned.Remove(name);
        }
    }
}
