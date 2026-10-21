using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static async Task LifetimeTests()
    {
        await Test("Admission uses one monotonic 30-second budget despite wall-clock jumps", () => Sync(() =>
        {
            var clock = new FakeClock(); var budget = RequestBudget.Admit(clock);
            clock.Advance(TimeSpan.FromSeconds(7)); clock.UtcNow = Epoch.AddDays(-3);
            Equal(TimeSpan.FromSeconds(23), budget.Remaining); Equal(Epoch.AddSeconds(30), budget.DeadlineUtc);
            clock.Advance(TimeSpan.FromSeconds(23)); Check(budget.IsExpired, "Deadline restarted"); Equal(TimeSpan.Zero, budget.Remaining);
            clock.Advance(TimeSpan.FromSeconds(5)); Equal(TimeSpan.Zero, budget.Remaining);
        }));
        await Test("Slow receipt consumes original budget and accept cap cannot be reset", () => Sync(() =>
        {
            var clock = new FakeClock(); long accepted = clock.Timestamp; clock.Advance(TimeSpan.FromSeconds(7));
            var received = RequestBudget.Receive(clock, accepted, Epoch.AddSeconds(30)); Equal(TimeSpan.FromSeconds(23), received.Remaining);
            clock.Advance(TimeSpan.FromSeconds(2)); Equal(TimeSpan.FromSeconds(21), received.Remaining);
            var capped = RequestBudget.Receive(clock, accepted, clock.UtcNow.AddSeconds(30)); Equal(TimeSpan.FromSeconds(21), capped.Remaining);
        }));
        await Test("Expired and beyond-policy deadlines are rejected at receipt", () => Sync(() =>
        {
            var clock = new FakeClock(); long accepted = clock.Timestamp; clock.Advance(TimeSpan.FromSeconds(7));
            Equal("DeadlineExceeded", Throws<BridgeException>(() => RequestBudget.Receive(clock, accepted, Epoch.AddSeconds(6))).Error.Code);
            Equal("InvalidRequest", Throws<BridgeException>(() => RequestBudget.Receive(clock, accepted, clock.UtcNow.AddSeconds(31))).Error.Code);
            clock.Advance(TimeSpan.FromSeconds(24));
            Equal("DeadlineExceeded", Throws<BridgeException>(() => RequestBudget.Receive(clock, accepted, clock.UtcNow.AddSeconds(1))).Error.Code);
        }));
        await Test("Context keeps same budget and a defensive request snapshot", () => Sync(() =>
        {
            var clock = new FakeClock(); var budget = RequestBudget.Admit(clock); var request = Request();
            using var lifetime = new RequestLifetime(request, budget, CancellationToken.None, CancellationToken.None);
            request.RequestId = "mutated"; lifetime.Context.Request.ContextToken = "also mutated";
            Equal("request-一", lifetime.Context.Request.RequestId); Equal("token-α", lifetime.Context.Request.ContextToken);
            Check(ReferenceEquals(budget, lifetime.Context.Budget), "Budget copied/reset");
            clock.Advance(TimeSpan.FromSeconds(7)); Equal(TimeSpan.FromSeconds(23), lifetime.Context.Budget.Remaining);
        }));
        await Test("Borrowed lifetime cannot extend its wire deadline", () => Sync(() =>
        {
            var clock = new FakeClock(); var budget = RequestBudget.Admit(clock);
            Throws<ArgumentException>(() => new RequestLifetime(Request("2026-10-21T00:01:00Z"), budget, CancellationToken.None, CancellationToken.None));
        }));
        foreach (bool stopping in new[] { false, true })
            await Test("Cancellation during preparation prevents a new read: ownerStopping=" + stopping, () => Sync(() =>
            {
                var clock = new FakeClock(); using var cancel = new CancellationTokenSource();
                using var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), stopping ? CancellationToken.None : cancel.Token, stopping ? cancel.Token : CancellationToken.None);
                lifetime.Context.ThrowIfInactive(); cancel.Cancel(); int reads = 0;
                Throws<OperationCanceledException>(() => { lifetime.Context.ThrowIfInactive(); reads++; });
                Equal(0, reads); Check(lifetime.Context.IsCancellationRequested, "Immediate invalidation lost across context");
            }));
        await Test("Expiry during preparation and after execution suppresses new reads and publication", () => Sync(() =>
        {
            var clock = new FakeClock(); using var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), CancellationToken.None, CancellationToken.None);
            lifetime.Context.ThrowIfInactive(); clock.Advance(TimeSpan.FromSeconds(30)); int reads = 0, published = 0;
            Equal("DeadlineExceeded", Throws<BridgeException>(() => { lifetime.Context.ThrowIfInactive(); reads++; }).Error.Code);
            Equal("request-一", Throws<BridgeException>(() => { lifetime.Context.ThrowIfInactive(); published++; }).Error.RequestId);
            Equal(0, reads); Equal(0, published);
        }));
        await Test("Remaining deadline timer wakes blocked work without upstream cancellation", async () =>
        {
            var clock = new SystemBridgeClock(); var budget = RequestBudget.Receive(clock, clock.Timestamp, clock.UtcNow.AddMilliseconds(150));
            using var lifetime = new RequestLifetime(Request(BridgeJson.FormatUtc(budget.DeadlineUtc)), budget, CancellationToken.None, CancellationToken.None);
            var blocked = Task.Delay(Timeout.Infinite, lifetime.Context.Cancellation);
            Check(ReferenceEquals(blocked, await Task.WhenAny(blocked, Task.Delay(3000))), "Deadline did not wake waiter");
            await ThrowsAsync<OperationCanceledException>(() => blocked);
            Equal("DeadlineExceeded", Throws<BridgeException>(lifetime.Context.ThrowIfInactive).Error.Code);
        });
        await Test("Canceled response retains lifetime until callback quiesces", async () =>
        {
            var clock = new FakeClock(); using var cancel = new CancellationTokenSource();
            var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), cancel.Token, CancellationToken.None);
            var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var ticket = new DispatchTicket(response.Task, quiesced.Task);
            Task cleanup = lifetime.DisposeAfterQuiescenceAsync(ticket);
            cancel.Cancel(); response.SetCanceled();
            Check(ticket.Response.IsCanceled && !ticket.Quiesced.IsCompleted && !cleanup.IsCompleted, "Logical finish disposed physical context");
            Check(lifetime.Context.IsCancellationRequested, "Immediate invalidation unavailable");
            _ = lifetime.Context.Cancellation.WaitHandle; // borrowed source remains usable; notification is asynchronous
            Throws<OperationCanceledException>(lifetime.Context.ThrowIfInactive);
            quiesced.SetResult(true); await cleanup;
            Throws<ObjectDisposedException>(() => { var handle = lifetime.Context.Cancellation.WaitHandle; });
            lifetime.Dispose();
        });
        await Test("Cancellation racing quiescence leaves cleanup settled without disposed-source faults", async () =>
        {
            for (int i = 0; i < 25; i++)
            {
                var clock = new FakeClock(); using var cancel = new CancellationTokenSource();
                var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), cancel.Token, CancellationToken.None);
                var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
                var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cleanup = lifetime.DisposeAfterQuiescenceAsync(new DispatchTicket(response.Task, quiesced.Task));
                await Task.WhenAll(Task.Run(cancel.Cancel), Task.Run(() => { response.TrySetCanceled(); quiesced.TrySetResult(true); }));
                await cleanup; lifetime.Dispose();
                Throws<ObjectDisposedException>(() => { var handle = lifetime.Context.Cancellation.WaitHandle; });
            }
        });
    }
}
