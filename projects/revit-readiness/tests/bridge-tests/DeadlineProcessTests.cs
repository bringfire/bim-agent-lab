using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    static async Task DeadlineProcessTests()
    {
        foreach (string mode in new[] { "control", "throw", "held", "complete", "upstream-timer", "owner-timer" })
            await Test("Deadline child survives and cleans up: " + mode, async () =>
            {
                string exe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BimAgentLab.Bridge.Tests.exe");
                using var child = new Process { StartInfo = new ProcessStartInfo(exe, "--deadline-child " + mode)
                    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true } };
                Check(child.Start(), "Deadline child did not start");
                var stdout = child.StandardOutput.ReadToEndAsync(); var stderr = child.StandardError.ReadToEndAsync();
                try
                {
                    await Until(() => child.HasExited, 15000);
                    string output = await stdout, error = await stderr;
                    Console.WriteLine("DEADLINE_CHILD mode=" + mode + " exit=" + child.ExitCode);
                    Console.Write(output); Console.Write(error);
                    Equal(0, child.ExitCode); Equal("", error);
                    Check(output.Contains("DEADLINE_PROOF mode=" + mode + " survived=true cleanup=true replacement=true"), "Child proof missing");
                }
                finally { if (!child.HasExited) { child.Kill(); child.WaitForExit(5000); } }
            });
    }
    [DllImport("kernel32.dll")] static extern uint SetErrorMode(uint mode);
    static async Task<int> DeadlineChild(string mode)
    {
        // Only this test child suppresses crash dialogs; no global exception handler or host policy change.
        SetErrorMode(3);
        try
        {
            if (mode.EndsWith("-timer", StringComparison.Ordinal)) await BorrowedTimerChild(mode);
            else await RunDeadlineChild(mode);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    static async Task RunDeadlineChild(string mode)
    {
        bool throws = mode == "throw" || mode == "held", held = mode == "held", completed = mode == "complete";
        var descriptor = ManualDescriptor(); string directory = Scratch("deadline-" + mode);
        var catalog = new SessionCatalog(directory); var events = new ConcurrentQueue<PipeEvent>();
        var ready = new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        int callbacks = 0; bool callbackKeptLifetime = false;
        var first = new InvalidOperationException("deadline callback first");
        var second = new ArgumentException("deadline callback second");
        var endpoint = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock(), e =>
        {
            if (e.Stage != "CancellationCallbackFailed") { events.Enqueue(e); return; }
            // Snapshot what was reported, then challenge the diagnostic ownership boundary.
            events.Enqueue(new PipeEvent { Stage = e.Stage, ConnectionId = e.ConnectionId, RequestId = e.RequestId, Error = e.Error });
            e.Error = null; e.RequestId = "mutated by observer";
            throw new Exception("A failing diagnostic observer must also be contained");
        });
        endpoint.Start(c =>
        {
            c.Cancellation.Register(() => { Interlocked.Increment(ref callbacks); if (throws) throw second; });
            c.Cancellation.Register(() =>
            {
                Interlocked.Increment(ref callbacks); entered.TrySetResult(true);
                if (held)
                {
                    Check(release.Wait(8000), "Cancellation callback release timed out");
                    callbackKeptLifetime = c.Cancellation.WaitHandle.WaitOne(0);
                }
                if (throws) throw first;
            });
            ready.TrySetResult(c);
            if (completed) { quiesced.TrySetResult(true); return Ticket(c, descriptor.BridgeSessionId); }
            return new DispatchTicket(response.Task, quiesced.Task);
        });
        using var pipe = RawClient(descriptor);
        try
        {
            await pipe.ConnectAsync(2000);
            DateTimeOffset originalDeadline = DateTimeOffset.UtcNow.AddMilliseconds(1100);
            var request = Request(BridgeJson.FormatUtc(originalDeadline)); request.RequestId = "deadline-" + mode;
            await Task.Delay(250); // already-spent time must not become a fresh lifetime
            await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request), BridgeLimits.MaxRequestBytes, CancellationToken.None);
            var context = await Within(ready.Task); Equal(originalDeadline, context.Budget.DeadlineUtc);
            Console.WriteLine("DEADLINE_SUBMITTED mode=" + mode + " originalBudgetPreserved=true"); Console.Out.Flush();
            if (completed)
            {
                var result = BridgeJson.Deserialize<BridgeResponse>(await Within(FrameCodec.ReadAsync(pipe, BridgeLimits.MaxResponseBytes, CancellationToken.None)));
                Equal(request.RequestId, result.RequestId);
                await Until(() => IsLifetimeDisposed(context));
                await Task.Delay(1200); Equal(0, callbacks); Check(!events.Any(e => e.Stage == "CancellationCallbackFailed"), "Disposed deadline fired");
            }
            else
            {
                await Within(entered.Task); Check(context.Budget.IsExpired, "Timer canceled before original monotonic budget expired");
                Equal("DeadlineExceeded", Throws<BridgeException>(context.ThrowIfInactive).Error.Code);
                Check(DateTimeOffset.UtcNow < originalDeadline.AddSeconds(2), "Deadline was reset or did not wake work promptly");
                // Response completion cannot publish a result after expiry, even before Quiesced.
                response.TrySetResult(Reply(context, descriptor.BridgeSessionId));
                if (held) quiesced.TrySetResult(true);
                await Until(() => events.Any(e => e.Stage == "Closed"));
                Equal(0, await Within(pipe.ReadAsync(new byte[1], 0, 1)));
                Check(context.Cancellation.WaitHandle.WaitOne(0), "Timer or unfinished executor lost its borrowed lifetime");
                if (held)
                {
                    // Stop releases network resources without waiting for this blocked callback.
                    await Within(endpoint.StopAsync());
                    Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Stop retained descriptor");
                    release.Set();
                }
                if (throws)
                {
                    await Until(() => events.Any(e => e.Stage == "CancellationCallbackFailed"));
                    var diagnostic = events.Single(e => e.Stage == "CancellationCallbackFailed");
                    Equal(request.RequestId, diagnostic.RequestId); Equal(1L, diagnostic.ConnectionId);
                    Check(diagnostic.Error is AggregateException, "Diagnostic lost callback errors");
                    var failure = await ThrowsAsync<AggregateException>(() => Within(endpoint.StopAsync()));
                    var causes = failure.Flatten().InnerExceptions;
                    Check(causes.Any(e => ReferenceEquals(first, e)) && causes.Any(e => ReferenceEquals(second, e)), "Deadline failures were lost");
                }
                Equal(2, callbacks);
                if (held) Check(callbackKeptLifetime, "Cleanup disposed the source during its deadline callback");
                else Check(!quiesced.Task.IsCompleted, "Test failed to keep executor borrowed");
                quiesced.TrySetResult(true);
                await Until(() => IsLifetimeDisposed(context));
            }
            for (int i = 0; i < 2; i++)
                if (throws) await ThrowsAsync<AggregateException>(() => Within(endpoint.StopAsync()));
                else await Within(endpoint.StopAsync());
            Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Deadline left an owned descriptor");
            await VerifyReplacement(catalog);
            Console.WriteLine("DEADLINE_PROOF mode=" + mode + " survived=true cleanup=true replacement=true callbacks=" + callbacks);
        }
        finally
        {
            release.Set(); quiesced.TrySetResult(true); response.TrySetCanceled();
            try { await Within(endpoint.StopAsync()); } catch (AggregateException) when (throws) { }
        }
    }
    static bool IsLifetimeDisposed(BridgeExecutionContext context)
    { try { _ = context.Cancellation.WaitHandle; return false; } catch (ObjectDisposedException) { return true; } }
    static async Task BorrowedTimerChild(string mode)
    {
        var clock = new SystemBridgeClock(); var budget = RequestBudget.Admit(clock);
        using var trigger = new CancellationTokenSource(); using var release = new ManualResetEventSlim();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var failure = new InvalidOperationException("borrowed timer callback"); Exception? recorded = null;
        using var lifetime = new RequestLifetime(Request(BridgeJson.FormatUtc(budget.DeadlineUtc)), budget,
            mode == "upstream-timer" ? trigger.Token : CancellationToken.None,
            mode == "owner-timer" ? trigger.Token : CancellationToken.None, error => recorded = error);
        bool callbackKeptLifetime = false;
        lifetime.Context.Cancellation.Register(() =>
        {
            entered.TrySetResult(true); Check(release.Wait(5000), "Borrowed callback release timed out");
            callbackKeptLifetime = lifetime.Context.Cancellation.WaitHandle.WaitOne(0);
            throw failure;
        });
        Task cleanup = lifetime.DisposeAfterQuiescenceAsync(new DispatchTicket(Task.FromResult(Response()), quiesced.Task));
        try
        {
            trigger.CancelAfter(150); await Within(entered.Task);
            Check(!budget.IsExpired, "Borrowed timer accidentally tested own deadline");
            Throws<OperationCanceledException>(lifetime.Context.ThrowIfInactive);
            quiesced.TrySetResult(true); await Task.Delay(100);
            Check(!cleanup.IsCompleted, "Cleanup ignored an active borrowed cancellation callback");
            Check(lifetime.Context.Cancellation.WaitHandle.WaitOne(0), "Borrowed callback lost its source");
            release.Set(); await Within(cleanup);
            Check(callbackKeptLifetime, "Source disposed while borrowed callback used it");
            Check(recorded is AggregateException aggregate && aggregate.Flatten().InnerExceptions.Any(e => ReferenceEquals(e, failure)), "Borrowed timer error was not reported");
            Check(IsLifetimeDisposed(lifetime.Context), "Borrowed source not released");
            // Also prove a fresh local session remains usable after this cancellation path.
            await VerifyReplacement(new SessionCatalog(Scratch(mode)));
            Console.WriteLine("DEADLINE_PROOF mode=" + mode + " survived=true cleanup=true replacement=true originalBudgetUnexpired=true");
        }
        finally { release.Set(); quiesced.TrySetResult(true); }
    }
}
