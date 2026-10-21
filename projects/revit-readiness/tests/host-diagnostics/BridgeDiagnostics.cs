using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.RevitReadiness
{
    // Compiled only by the separate diagnostic host project, never the production add-in.
    internal static class BridgeDiagnostics
    {
        static DiagnosticRecorder recorder;
        static string directory;
        static int failed;
        [ThreadStatic] static BridgeExecutionContext current;
        internal static void Initialize()
        {
            directory = DiagnosticRunDirectory.CreateProcessDirectory(DiagnosticRunDirectory.Root, Process.GetCurrentProcess().Id);
            recorder = new DiagnosticRecorder(directory);
            Record("ApplicationStartup");
        }
        static void Safe(Action action)
        {
            if (recorder == null || Volatile.Read(ref failed) != 0) return;
            try { action(); }
            catch (Exception error)
            {
                Interlocked.Exchange(ref failed, 1);
                Trace.TraceError("BIM diagnostic capture failed: {0}", error);
                try { File.WriteAllText(Path.Combine(directory, "diagnostic-error.txt"), error.ToString()); } catch { }
                // Missing final markers also fail capture; diagnostics cannot break native cleanup.
            }
        }
        internal static void Record(string stage, string detail = "") => Safe(() => recorder.Record(stage, current, detail));
        internal static void Hold(string stage) => Safe(() => recorder.Hold(stage, current, () => BridgeApplication.Current.Stop()));
        internal static BridgeResponse Execute(BridgeExecutionContext context, Func<BridgeResponse> action)
        {
            current = context;
            try { Record("ExecutorEntered"); return action(); }
            finally { Record("ExecutorExited"); current = null; }
        }
        internal static IHostBridgeEndpoint Endpoint(IHostBridgeEndpoint endpoint) => new ObservedEndpoint(endpoint);
        static void Observe(Task task, string stage, BridgeExecutionContext context = null)
        {
            _ = task.ContinueWith(done => {
                string detail = done.Status.ToString();
                var response = done as Task<BridgeResponse>;
                if (response != null && response.Status == TaskStatus.RanToCompletion)
                    detail += ":" + (response.Result.Error?.Code ?? "Success");
                Safe(() => recorder.Record(stage, context, detail));
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }
        sealed class ObservedEndpoint : IHostBridgeEndpoint
        {
            readonly IHostBridgeEndpoint inner;
            internal ObservedEndpoint(IHostBridgeEndpoint inner) { this.inner = inner; }
            public void Start(Func<BridgeExecutionContext, DispatchTicket> submit) => inner.Start(context => {
                Safe(() => recorder.Record("Submitted", context));
                var ticket = submit(context);
                Observe(ticket.Response, "ResponseCompleted", context);
                Observe(ticket.Quiesced, "QuiescedObserved", context);
                return ticket;
            });
            public Task StopAsync() { var task = inner.StopAsync(); Observe(task, "NetworkStopped"); return task; }
        }
    }
}
