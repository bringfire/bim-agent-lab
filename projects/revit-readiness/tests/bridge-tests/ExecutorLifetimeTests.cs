using System;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static async Task ExecutorLifetimeTests()
    {
        foreach (string stage in new[] { "entry", "prepare", "read", "serialize" })
        foreach (bool expire in new[] { false, true })
            await Test("Direct production flow guard " + stage + " expiry=" + expire, () => Sync(() =>
            {
                using var rig = new DispatchRig(); var call = rig.Add();
                Action<string> interrupt = here => { if (here == stage) { if (expire) rig.Clock.Advance(TimeSpan.FromSeconds(30)); else call.Cancel.Cancel(); } };
                interrupt("entry");
                Action run = () => ReadinessExecutionFlow.Run(call.Lifetime.Context,
                    () => { interrupt("prepare"); return 0; }, p => { interrupt("read"); return p; },
                    _ => { interrupt("serialize"); return QueueResponse(call.Lifetime.Context); });
                if (expire) Equal("DeadlineExceeded", Throws<BridgeException>(run).Error.Code);
                else Throws<OperationCanceledException>(run);
            }));
        foreach (string stage in new[] { "entry", "prepare", "read", "serialize" })
        foreach (bool expire in new[] { false, true })
            await Test("Production flow " + stage + (expire ? " expiry" : " cancellation"), async () =>
            {
                using var rig = new DispatchRig(); var call = rig.Add(); int prepares = 0, reads = 0, serializes = 0;
                using var reached = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
                Action interrupt = () => { if (expire) rig.Clock.Advance(TimeSpan.FromSeconds(30)); else call.Cancel.Cancel(); };
                Action<string> pause = here => { if (stage == here) { reached.Set(); Hold(release); } };
                if (stage == "entry") { interrupt(); release.Set(); }
                // Direct production flow is inside production queue, not a recreated guard sequence.
                var drain = Task.Run(() => rig.Queue.Drain(4, c => ReadinessExecutionFlow.Run(c,
                    () => { prepares++; pause("prepare"); return "validated"; },
                    p => { Equal("validated", p); reads++; pause("read"); return Report; },
                    r => { serializes++; pause("serialize"); return QueueResponse(c, r); })));
                try
                {
                    if (stage != "entry") { Check(reached.Wait(2000), "Flow did not reach " + stage); interrupt(); }
                }
                finally { release.Set(); await drain; }
                Equal(stage == "entry" ? 0 : 1, prepares);
                Equal(stage == "entry" || stage == "prepare" ? 0 : 1, reads);
                Equal(stage == "serialize" ? 1 : 0, serializes);
                if (expire) Equal("DeadlineExceeded", (await call.Ticket.Response).Error!.Code);
                else Check(call.Ticket.Response.IsCanceled, "Late success published");
                await Progress(call.Ticket.Quiesced, "flow quiescence");
            });
        await Test("Production flow preserves original context, order and complete report", async () =>
        {
            using var rig = new DispatchRig(); var call = rig.Add(); string order = "";
            rig.Queue.Drain(4, c => ReadinessExecutionFlow.Run(c,
                () => { order += "P"; rig.Clock.Advance(TimeSpan.FromSeconds(7)); return c; },
                p => { Check(ReferenceEquals(call.Lifetime.Context, p), "Context replaced"); Equal(TimeSpan.FromSeconds(23), p.Budget.Remaining); order += "R"; return Report; },
                r => { order += "S"; return QueueResponse(c, r); }));
            Equal("PRS", order); Equal(Report, (await call.Ticket.Response).ResultJson);
        });
        foreach (string failure in new[] { "serialize", "cancel", "expiry", "queue-cancel", "queue-error" })
            await Test("Unpublished context revoked on " + failure, async () =>
            {
                using var rig = new DispatchRig(); var call = rig.Add(); var store = new ContextStore("session-1", rig.Clock);
                string token = ""; var publication = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
                try
                {
                    ReadinessExecutionFlow.IssueContext(call.Lifetime.Context, store, Binding(), publication.Task, lease =>
                    {
                        token = lease.Token;
                        if (failure == "serialize") throw new InvalidOperationException("serialization failure");
                        if (failure == "cancel") call.Cancel.Cancel();
                        if (failure == "expiry") rig.Clock.Advance(TimeSpan.FromSeconds(30));
                        return QueueResponse(call.Lifetime.Context);
                    });
                    Check(failure.StartsWith("queue-"), "Expected synchronous failure");
                }
                catch (Exception error) when (error is InvalidOperationException || error is OperationCanceledException || error is BridgeException) { }
                Check(token.Length > 0, "Lease never issued");
                if (failure == "queue-error") publication.SetResult(new BridgeResponse { Error = new BridgeError { Code = "Busy" } });
                else publication.TrySetCanceled();
                await Until(() => { try { store.Validate(token, Binding()); return false; } catch (BridgeException e) { return e.Error.Code == "ContextExpired"; } });
            });
        await Test("Published context remains usable", async () =>
        {
            using var rig = new DispatchRig(); var call = rig.Add(); var store = new ContextStore("session-1", rig.Clock);
            string token = ""; var result = ReadinessExecutionFlow.IssueContext(call.Lifetime.Context, store, Binding(), call.Ticket.Response,
                lease => { token = lease.Token; return QueueResponse(call.Lifetime.Context); });
            rig.Queue.Drain(4, _ => result); await call.Ticket.Response; store.Validate(token, Binding());
        });
    }
}
