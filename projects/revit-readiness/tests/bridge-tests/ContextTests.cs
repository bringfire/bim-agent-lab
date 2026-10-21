using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static ContextBinding Binding() => new ContextBinding
    {
        BridgeSessionId = "session-1", DocumentLifetimeId = "open-1", ProjectUniqueId = "project-1",
        DocumentPath = @"D:\samples\model.rvt", ChecklistId = "check-1",
        ChecklistPath = @"D:\samples\model.checklist.json", ChecklistSha256 = new string('a', 64)
    };
    static async Task ContextTests()
    {
        await Test("Incomplete bindings cannot issue a reusable context", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock());
            foreach (Action<ContextBinding> change in new Action<ContextBinding>[]
            {
                b => b.DocumentLifetimeId = "", b => b.ProjectUniqueId = " ", b => b.ChecklistId = "",
                b => b.DocumentPath = "model.rvt", b => b.DocumentPath = @"D:\samples\..\model.rvt",
                b => b.ChecklistPath = "model.checklist.json", b => b.ChecklistSha256 = "abc", b => b.ChecklistSha256 = new string('x', 64)
            })
            { var binding = Binding(); change(binding); Throws<ArgumentException>(() => store.Issue(binding)); }
        }));
        await Test("Context capacity rejects 33rd without evicting live tokens", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock());
            var leases = Enumerable.Range(0, 32).Select(_ => store.Issue(Binding())).ToArray();
            Equal(32, leases.Select(l => l.Token).Distinct().Count());
            Check(leases.All(l => Convert.FromBase64String(l.Token).Length >= 32), "Token lacks 256 random bits");
            Equal("ContextCapacity", Throws<BridgeException>(() => store.Issue(Binding())).Error.Code);
            foreach (var l in leases) store.Validate(l.Token, Binding());
        }));
        await Test("Context is valid at 4:59 and expires exactly at 5:00", () => Sync(() =>
        {
            var clock = new FakeClock(); var store = new ContextStore("session-1", clock); var lease = store.Issue(Binding());
            Equal(Epoch.AddMinutes(5), lease.ExpiresUtc); clock.Advance(TimeSpan.FromSeconds(299)); store.Validate(lease.Token, Binding());
            clock.Advance(TimeSpan.FromSeconds(1));
            Equal("ContextExpired", Throws<BridgeException>(() => store.Validate(lease.Token, Binding())).Error.Code);
        }));
        await Test("Context expiry ignores wall-clock jumps", () => Sync(() =>
        {
            var clock = new FakeClock(); var store = new ContextStore("session-1", clock); var lease = store.Issue(Binding());
            clock.UtcNow += TimeSpan.FromDays(10); store.Validate(lease.Token, Binding());
            clock.UtcNow -= TimeSpan.FromDays(20); clock.Advance(TimeSpan.FromMinutes(5));
            Equal("ContextExpired", Throws<BridgeException>(() => store.Validate(lease.Token, Binding())).Error.Code);
        }));
        await Test("Expired contexts free capacity but newer live contexts remain valid", () => Sync(() =>
        {
            var clock = new FakeClock(); var store = new ContextStore("session-1", clock);
            for (int i = 0; i < 31; i++) store.Issue(Binding());
            clock.Advance(TimeSpan.FromSeconds(10)); var live = store.Issue(Binding());
            clock.Advance(TimeSpan.FromSeconds(290));
            for (int i = 0; i < 31; i++) store.Issue(Binding());
            store.Validate(live.Token, Binding());
            Equal("ContextCapacity", Throws<BridgeException>(() => store.Issue(Binding())).Error.Code);
        }));
        var changes = new Dictionary<string, Action<ContextBinding>>
        {
            ["session"] = b => b.BridgeSessionId = "session-2",
            ["reopened document"] = b => b.DocumentLifetimeId = "open-2",
            ["project ID"] = b => b.ProjectUniqueId = "project-2",
            ["copied ID different path"] = b => b.DocumentPath = @"D:\copies\model.rvt",
            ["checklist ID"] = b => b.ChecklistId = "check-2",
            ["checklist path"] = b => b.ChecklistPath = @"D:\copies\model.checklist.json",
            ["checklist bytes"] = b => b.ChecklistSha256 = new string('b', 64)
        };
        foreach (var change in changes)
            await Test("Context rejects changed " + change.Key, () => Sync(() =>
            {
                var store = new ContextStore("session-1", new FakeClock()); var token = store.Issue(Binding()).Token;
                var current = Binding(); change.Value(current);
                Equal("ContextChanged", Throws<BridgeException>(() => store.Validate(token, current)).Error.Code);
                store.Validate(token, Binding()); // mismatch never silently retargets the lease
            }));
        await Test("Context snapshots mutable input and accepts equivalent Windows casing", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock()); var input = Binding(); var token = store.Issue(input).Token;
            input.DocumentPath = @"D:\other.rvt"; var current = Binding();
            current.DocumentPath = current.DocumentPath.ToUpperInvariant(); current.ChecklistPath = current.ChecklistPath.ToUpperInvariant();
            current.ChecklistSha256 = current.ChecklistSha256.ToUpperInvariant(); store.Validate(token, current);
            Equal("ContextChanged", Throws<BridgeException>(() => store.Validate(token, input)).Error.Code);
        }));
        await Test("Revoking an unpublished lease is idempotent and restores one slot", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock()); var token = store.Issue(Binding()).Token;
            for (int i = 1; i < 32; i++) store.Issue(Binding()); store.Revoke(token); store.Revoke(token);
            Equal("ContextExpired", Throws<BridgeException>(() => store.Validate(token, Binding())).Error.Code);
            store.Issue(Binding()); Equal("ContextCapacity", Throws<BridgeException>(() => store.Issue(Binding())).Error.Code);
        }));
        await Test("Clear and replacement store invalidate old tokens", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock()); var token = store.Issue(Binding()).Token;
            store.Clear(); store.Clear();
            Equal("ContextExpired", Throws<BridgeException>(() => store.Validate(token, Binding())).Error.Code);
            var replacement = new ContextStore("session-2", new FakeClock()); var next = Binding(); next.BridgeSessionId = "session-2";
            Equal("ContextExpired", Throws<BridgeException>(() => replacement.Validate(token, next)).Error.Code);
            Equal("ContextChanged", Throws<BridgeException>(() => replacement.Issue(Binding())).Error.Code);
        }));
        await Test("Concurrent context issuance preserves the capacity bound", async () =>
        {
            var store = new ContextStore("session-1", new FakeClock()); var leases = new ConcurrentBag<ContextLease>(); var errors = new ConcurrentBag<string>();
            await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => Task.Run(() =>
            { try { leases.Add(store.Issue(Binding())); } catch (BridgeException error) { errors.Add(error.Error.Code); } })));
            Equal(32, leases.Count); Equal(32, errors.Count); Check(errors.All(e => e == "ContextCapacity"), "Unexpected capacity error");
            foreach (var lease in leases) store.Validate(lease.Token, Binding());
        });
        await Test("Unknown context cannot select a different model", () => Sync(() =>
        {
            var store = new ContextStore("session-1", new FakeClock());
            Equal("ContextExpired", Throws<BridgeException>(() => store.Validate("unissued", Binding())).Error.Code);
        }));
    }
}
