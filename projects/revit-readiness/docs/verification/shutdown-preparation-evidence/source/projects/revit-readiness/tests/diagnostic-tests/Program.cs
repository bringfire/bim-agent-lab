using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitReadiness;

#if NET10_0_OR_GREATER
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

internal static partial class Program
{
    static int Main()
    {
        var tests = new Action[] { RecordsIdentity, ReleasesExplicitly, PreparationCancellation, PreparationExpiry,
            ReaderCancellation, BoundedHold, RejectsStaleRelease, RejectsInvalidControl, SingleUse, StopWhileHeld,
            RootIsOutsideAppData, SharedFileResolvesToRequestedPath, ValidPreparedRun,
            MissingSelector, InvalidSelector, MissingPreparedDirectory, ExistingProcessDirectory,
            LifecycleRelease, LifecycleBounded, LifecycleInvalidControls, PipeIdentity };
        int failed = 0;
        foreach (var test in tests)
        {
            try { test(); Console.WriteLine("PASS " + test.Method.Name); }
            catch (Exception error) { failed++; Console.WriteLine("FAIL " + test.Method.Name + ": " + error.Message); }
        }
        Console.WriteLine($"Diagnostic controls: {tests.Length-failed} passed; {failed} failed. No live Revit proof.");
        return failed == 0 ? 0 : 1;
    }
    static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    static void LifecycleRelease()
    {
        foreach (string stage in new[] { "ApplicationClosing", "ShutdownAfterStop" })
        {
            using var f = new Fixture();
            File.WriteAllText(Path.Combine(f.Root, "arm-" + stage + ".txt"), "3000");
            var task = Task.Run(() => f.Recorder.HoldLifecycle(stage));
            try
            {
                Wait(() => File.Exists(Path.Combine(f.Root, "held-" + stage + ".txt")));
                Require(!task.IsCompleted, "Lifecycle not held");
                File.WriteAllText(Path.Combine(f.Root, "release-" + stage + ".signal"), "test");
                Require(task.Wait(2000), "Lifecycle did not release");
                f.Recorder.HoldLifecycle(stage);
                Require(f.Events.Split('\n').Count(s => s.Contains("\tLifecycleHeld\t")) == 1, "Not single consumption");
                Require(f.Events.Contains("LifecycleReleased") && !f.Events.Contains("AutomaticRelease"), "Release proof missing");
            }
            finally { File.WriteAllText(Path.Combine(f.Root, "release-" + stage + ".signal"), "cleanup"); task.Wait(4000); }
        }
    }
    static void LifecycleBounded()
    {
        using var f = new Fixture(); File.WriteAllText(Path.Combine(f.Root, "arm-ApplicationClosing.txt"), "20");
        var watch = Stopwatch.StartNew(); f.Recorder.HoldLifecycle("ApplicationClosing");
        Require(watch.ElapsedMilliseconds < 2000 && f.Events.Contains("LifecycleAutomaticRelease"), "Missing bounded failure release");
    }
    static void LifecycleInvalidControls()
    {
        foreach (string value in new[] { "0", "45001", "abc", "10\nstop", "10" })
        {
            using var f = new Fixture(); File.WriteAllText(Path.Combine(f.Root, "arm-ApplicationClosing.txt"), value);
            if (value == "10") File.WriteAllText(Path.Combine(f.Root, "release-ApplicationClosing.signal"), "stale");
            f.Recorder.HoldLifecycle("ApplicationClosing");
            Require(f.Events.Contains("ControlRejected") && !f.Events.Contains("LifecycleHeld"), "Invalid lifecycle control accepted");
        }
    }
    static void PipeIdentity()
    {
        using var f = new Fixture(); f.Recorder.Record("Pipe.LifetimeDisposed", null, "42", "request-retired");
        Require(f.Events.Contains("\tPipe.LifetimeDisposed\trequest-retired\t"), "Pipe request identity lost");
    }
    static void Wait(Func<bool> ready) { Require(SpinWait.SpinUntil(ready, 2000), "Expected observation was not reached"); }
    static void RecordsIdentity()
    {
        using var f = new Fixture(); f.Recorder.Record("Test", f.Context);
        Require(f.Events.Contains(f.Context.Request.RequestId), "Original request identity missing");
        Require(f.Events.Contains("\t" + Thread.CurrentThread.ManagedThreadId + "\tTest\t"), "Execution thread missing");
    }
    static void ReleasesExplicitly()
    {
        using var f = new Fixture(); f.Arm("Preparation");
        var task = Task.Run(() => f.Recorder.Hold("Preparation", f.Context, null));
        try { f.WaitHeld(); Require(!task.IsCompleted, "Hold did not hold"); f.Release(); Require(task.Wait(2000), "Release did not progress"); }
        finally { f.Release(); task.Wait(4000); }
        Require(f.Events.Contains("GateReleased"), "Release observation absent");
    }
    static void PreparationCancellation() => Interrupted(false, false);
    static void PreparationExpiry() => Interrupted(false, true);
    static void ReaderCancellation() => Interrupted(true, false);
    static void Interrupted(bool reader, bool expiry)
    {
        using var f = new Fixture(); f.Arm(reader ? "Reader" : "Preparation"); int calls = 0;
        var originalDeadline = f.Context.Budget.DeadlineUtc;
        var task = Task.Run(() => {
            try {
                ReadinessExecutionFlow.Run(f.Context, () => { if (!reader) f.Recorder.Hold("Preparation", f.Context, null); return 1; },
                    _ => { Interlocked.Increment(ref calls); if (reader) f.Recorder.Hold("Reader", f.Context, null); return 1; },
                    _ => new BridgeResponse());
                return "success";
            } catch (OperationCanceledException) { return "canceled"; }
              catch (BridgeException e) { return e.Error.Code; }
        });
        try {
            f.WaitHeld(); if (expiry) f.Clock.Offset = TimeSpan.FromSeconds(31); else f.Cancel.Cancel();
            Wait(() => f.Events.Contains("InactiveObserved")); Require(!task.IsCompleted, "Borrower completed before release");
            f.Release(); Require(task.Wait(2000), "Flow did not finish");
            Require(task.Result == (expiry ? "DeadlineExceeded" : "canceled"), "Incorrect interrupted outcome");
            Require(calls == (reader ? 1 : 0), "Reader-entry boundary crossed incorrectly");
            Require(f.Context.Budget.DeadlineUtc == originalDeadline, "Original deadline was changed");
        } finally { f.Release(); task.Wait(4000); }
    }
    static void BoundedHold()
    {
        using var f = new Fixture(); f.Arm("Preparation", 100); var timer = Stopwatch.StartNew();
        f.Recorder.Hold("Preparation", f.Context, null);
        Require(timer.ElapsedMilliseconds >= 90 && timer.ElapsedMilliseconds < 2000, "Hold did not honor its own bound");
        Require(f.Events.Contains("AutomaticRelease"), "Automatic release not distinguished");
    }
    static void RejectsStaleRelease()
    {
        using var f = new Fixture(); f.Arm("Preparation"); f.Release();
        f.Recorder.Hold("Preparation", f.Context, null);
        Require(f.Events.Contains("ControlRejected") && !f.Events.Contains("GateHeld"), "Stale release accepted as a new hold");
    }
    static void RejectsInvalidControl()
    {
        using var f = new Fixture(); f.Arm("Preparation", 45001);
        f.Recorder.Hold("Preparation", f.Context, null);
        Require(f.Events.Contains("ControlRejected") && !f.Events.Contains("GateHeld"), "Unbounded control accepted");
    }
    static void SingleUse()
    {
        using var f = new Fixture(); f.Arm("Reader", 30); f.Recorder.Hold("Reader", f.Context, null); f.Recorder.Hold("Reader", f.Context, null);
        Require(f.Events.Split('\n').Count(s => s.Contains("\tGateHeld\t")) == 1, "Arm was not consumed exactly once");
    }
    static void StopWhileHeld()
    {
        using var f = new Fixture(); f.Arm("Reader", 3000, "stop");
        var task = Task.Run(() => f.Recorder.Hold("Reader", f.Context, f.Cancel.Cancel));
        try { f.WaitHeld(); Wait(() => f.Context.IsCancellationRequested); Require(!task.IsCompleted, "Stop waited for the held reader"); }
        finally { f.Release(); task.Wait(4000); }
        Require(f.Events.Contains("StopInjected"), "Injected Stop was not labelled");
    }
    sealed class Clock : IBridgeClock
    {
        internal TimeSpan Offset;
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public long Timestamp => Stopwatch.GetTimestamp();
        public TimeSpan ElapsedSince(long started) => TimeSpan.FromSeconds((Stopwatch.GetTimestamp()-started)/(double)Stopwatch.Frequency)+Offset;
    }
    sealed class Fixture : IDisposable
    {
        internal readonly string Root = Path.Combine(Path.GetTempPath(), "bal-diagnostic-" + Guid.NewGuid().ToString("N"));
        internal readonly Clock Clock = new Clock();
        internal readonly CancellationTokenSource Cancel = new CancellationTokenSource();
        internal readonly RequestLifetime Lifetime;
        internal readonly DiagnosticRecorder Recorder;
        internal BridgeExecutionContext Context => Lifetime.Context;
        string stage;
        internal Fixture()
        {
            Directory.CreateDirectory(Root); Recorder = new DiagnosticRecorder(Root);
            var budget = RequestBudget.Admit(Clock);
            Lifetime = new RequestLifetime(new BridgeRequest { ContractVersion=1, RequestId=Guid.NewGuid().ToString("N"),
                Operation=BridgeLimits.CheckInformation, ContextToken="test", DeadlineUtc=BridgeJson.FormatUtc(budget.DeadlineUtc) }, budget, Cancel.Token, CancellationToken.None);
        }
        internal string Events { get { var p=Path.Combine(Root,"events.tsv"); if(!File.Exists(p))return ""; using(var s=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))using(var r=new StreamReader(s))return r.ReadToEnd(); } }
        internal void Arm(string value, int milliseconds=3000, string mode="hold") { stage=value; File.WriteAllText(Path.Combine(Root,"arm-"+stage+".txt"), milliseconds.ToString(CultureInfo.InvariantCulture)+"\n"+mode); }
        internal void WaitHeld() => Wait(() => Events.Contains("\tGateHeld\t"));
        internal void Release() { if(stage!=null)File.WriteAllText(Path.Combine(Root,"release-"+Context.Request.RequestId+"-"+stage+".signal"),""); }
        public void Dispose() { Lifetime.Dispose(); Cancel.Dispose(); /* Keep small temporary logs for failed-test diagnosis. */ }
    }
}
