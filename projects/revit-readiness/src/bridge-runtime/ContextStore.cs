using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public sealed class ContextStore
{
    readonly string session;
    readonly IBridgeClock clock;
    readonly object gate = new object();
    readonly Dictionary<string, Record> records = new Dictionary<string, Record>(StringComparer.Ordinal);
    static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    sealed class Record
    {
        internal readonly ContextBinding Binding;
        internal readonly long Issued;
        internal Record(ContextBinding binding, long issued) { Binding = binding; Issued = issued; }
    }
    public ContextStore(string bridgeSessionId, IBridgeClock clock)
    {
        if (string.IsNullOrWhiteSpace(bridgeSessionId)) throw new ArgumentException("A bridge session is required.", nameof(bridgeSessionId));
        session = bridgeSessionId; this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
    public ContextLease Issue(ContextBinding binding)
    {
        if (binding == null) throw new ArgumentNullException(nameof(binding));
        var snapshot = Copy(binding);
        ValidateBinding(snapshot);
        lock (gate)
        {
            Purge();
            if (snapshot.BridgeSessionId != session) throw Changed();
            if (records.Count >= 32) throw BridgeFailures.Create("ContextCapacity", "All context slots are in use. Retry after a context expires.");
            string token;
            using (var random = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                do { random.GetBytes(bytes); token = Convert.ToBase64String(bytes); } while (records.ContainsKey(token));
            }
            records.Add(token, new Record(snapshot, clock.Timestamp));
            return new ContextLease(token, clock.UtcNow + Lifetime);
        }
    }
    public void Validate(string token, ContextBinding current)
    {
        if (current == null) throw new ArgumentNullException(nameof(current));
        lock (gate)
        {
            Purge();
            if (token == null || !records.TryGetValue(token, out var record))
                throw BridgeFailures.Create("ContextExpired", "The context is unknown or expired. Obtain a fresh context.");
            if (!Same(record.Binding, current)) throw Changed();
        }
    }
    public void Revoke(string token) { lock (gate) { if (token != null) records.Remove(token); } }
    public void Clear() { lock (gate) records.Clear(); }
    void Purge()
    {
        foreach (string key in records.Where(pair => clock.ElapsedSince(pair.Value.Issued) >= Lifetime).Select(pair => pair.Key).ToArray())
            records.Remove(key);
    }
    static BridgeException Changed() => BridgeFailures.Create("ContextChanged", "The document or checklist identity changed. Obtain a fresh context.");
    static void ValidateBinding(ContextBinding binding)
    {
        if (new[] { binding.BridgeSessionId, binding.DocumentLifetimeId, binding.ProjectUniqueId, binding.ChecklistId,
            binding.DocumentPath, binding.ChecklistPath, binding.ChecklistSha256 }.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Context identity fields are required.", nameof(binding));
        if (!Canonical(binding.DocumentPath) || !Canonical(binding.ChecklistPath) ||
            binding.ChecklistSha256.Length != 64 || !binding.ChecklistSha256.All(Uri.IsHexDigit))
            throw new ArgumentException("Context paths must be canonical absolute paths and the checklist hash must be SHA-256 hex.", nameof(binding));
    }
    static bool Canonical(string path) => Path.IsPathRooted(path) && StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(path), path);
    static bool Same(ContextBinding left, ContextBinding right) =>
        left.BridgeSessionId == right.BridgeSessionId && left.DocumentLifetimeId == right.DocumentLifetimeId &&
        left.ProjectUniqueId == right.ProjectUniqueId && left.ChecklistId == right.ChecklistId &&
        StringComparer.OrdinalIgnoreCase.Equals(left.DocumentPath, right.DocumentPath) &&
        StringComparer.OrdinalIgnoreCase.Equals(left.ChecklistPath, right.ChecklistPath) &&
        StringComparer.OrdinalIgnoreCase.Equals(left.ChecklistSha256, right.ChecklistSha256);
    static ContextBinding Copy(ContextBinding b) => new ContextBinding
    {
        BridgeSessionId = b.BridgeSessionId, DocumentLifetimeId = b.DocumentLifetimeId, ProjectUniqueId = b.ProjectUniqueId,
        DocumentPath = b.DocumentPath, ChecklistId = b.ChecklistId, ChecklistPath = b.ChecklistPath, ChecklistSha256 = b.ChecklistSha256
    };
}
