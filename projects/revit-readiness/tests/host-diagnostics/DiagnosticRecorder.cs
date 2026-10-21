using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.RevitReadiness
{
    internal sealed class DiagnosticRecorder
    {
        readonly string root;
        readonly object writing = new object();
        int sequence;
        internal DiagnosticRecorder(string root)
        {
            this.root = Path.GetFullPath(root);
            if (!Directory.Exists(this.root)) throw new DirectoryNotFoundException(this.root);
        }
        internal void Record(string stage, BridgeExecutionContext context = null, string detail = "")
        {
            // This diagnostic build does synchronous, bounded-size file I/O. It must
            // not be used for production timing claims or made part of a cancellation callback.
            lock (writing)
            {
                if (sequence >= 4096) throw new InvalidOperationException("Diagnostic event limit reached; capture is incomplete.");
                var line = string.Join("\t", ++sequence, DateTimeOffset.UtcNow.ToString("o"), Stopwatch.GetTimestamp(),
                    Thread.CurrentThread.ManagedThreadId, Clean(stage), context?.Request.RequestId ?? "",
                    context?.IsCancellationRequested.ToString() ?? "", context?.Budget.IsExpired.ToString() ?? "", Clean(detail));
                using (var file = new FileStream(Path.Combine(root, "events.tsv"), FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(line + "\n");
                    file.Write(bytes, 0, bytes.Length); file.Flush(true);
                }
            }
        }
        static string Clean(string value) => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        internal void Hold(string stage, BridgeExecutionContext context, Action stop)
        {
            if (context == null || context.Request.Operation != BridgeLimits.CheckInformation) return;
            if (stage != "Preparation" && stage != "Reader") throw new ArgumentException("Unknown diagnostic boundary.");
            string arm = Path.Combine(root, "arm-" + stage + ".txt");
            if (!File.Exists(arm)) return;
            string key = context.Request.RequestId + "-" + stage;
            string consumed = Path.Combine(root, "used-" + key + ".txt");
            File.Move(arm, consumed); // claim once, with request/stage-specific release identity
            string release = Path.Combine(root, "release-" + key + ".signal");
            var parts = File.ReadAllLines(consumed);
            int milliseconds;
            if (parts.Length != 2 || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out milliseconds)
                || milliseconds < 1 || milliseconds > 45000 || (parts[1] != "hold" && parts[1] != "stop")
                || File.Exists(release) || (parts[1] == "stop" && stop == null))
            { Record("ControlRejected", context, stage); return; }
            var timer = Stopwatch.StartNew();
            Record("GateHeld", context, stage);
            File.WriteAllText(Path.Combine(root, "held-" + key + ".txt"), context.Request.DeadlineUtc);
            if (parts[1] == "stop") { Record("StopInjected", context, stage); stop(); }
            bool observed = false;
            while (!File.Exists(release) && timer.ElapsedMilliseconds < milliseconds)
            {
                if (!observed && (context.IsCancellationRequested || context.Budget.IsExpired))
                { observed = true; Record("InactiveObserved", context, stage); }
                Thread.Sleep(10);
            }
            Record(File.Exists(release) ? "GateReleased" : "AutomaticRelease", context, stage);
        }
    }
}
