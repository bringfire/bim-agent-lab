using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.RevitReadiness
{
    public sealed class BridgeApplication : IExternalApplication
    {
        internal static BridgeApplication Current { get; private set; }
        UIControlledApplication application;
        HostBridgeSession session;
        RevitContextCatalog catalog;
        bool hooked;
        bool reportedLifecycleFailure;
        public Result OnStartup(UIControlledApplication app)
        {
            application = app; Current = this;
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Initialize();
            BridgeDiagnostics.HookClosing(app);
#endif
            return Result.Succeeded;
        } // No automatic listener.

        internal void Start()
        {
            if (session != null)
            {
                if (session.IsRunning) return;
                if (!session.PollRetirementInApiContext()) throw new InvalidOperationException("Bridge is still retiring. Retry Start after Stop completes.");
                Unhook();
            }
            var descriptor = BridgeIdentity.CreateCurrent();
            var clock = new SystemBridgeClock();
            var nextCatalog = new RevitContextCatalog(descriptor.BridgeSessionId);
            int reportedNetworkFailure = 0;
            HostBridgeSession next = null;
            RevitReadinessExecutor executor = null;
            next = new HostBridgeSession(descriptor.BridgeSessionId, clock, () =>
            {
                var handler = new ReadinessExternalEventHandler(ui => next.DrainInApiContext(c => executor.Execute(ui, c)));
                var externalEvent = ExternalEvent.Create(handler);
                return new ApiEventLease(() => {
#if BIM_BRIDGE_DIAGNOSTICS
                    BridgeDiagnostics.Record("NativeRaiseBegin");
#endif
                    var result = externalEvent.Raise();
#if BIM_BRIDGE_DIAGNOSTICS
                    BridgeDiagnostics.Record("NativeRaiseEnd", result.ToString());
#endif
                    return Map(result);
                }, () => {
                    externalEvent.Dispose();
#if BIM_BRIDGE_DIAGNOSTICS
                    BridgeDiagnostics.Record("EventDisposed");
#endif
                });
            }, () => {
                var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(), clock, observation =>
            {
#if BIM_BRIDGE_DIAGNOSTICS
                BridgeDiagnostics.RecordPipe(observation);
#endif
                if (observation.Error != null && System.Threading.Interlocked.Exchange(ref reportedNetworkFailure, 1) == 0)
                    Trace.TraceError("BIM bridge asynchronous cleanup failure: {0}", observation.Error.GetType().Name);
            });
#if BIM_BRIDGE_DIAGNOSTICS
                return BridgeDiagnostics.Endpoint(endpoint);
#else
                return endpoint;
#endif
            });
            executor = new RevitReadinessExecutor(descriptor, next, nextCatalog);
            // Retain ownership before acquisition, including failed starts.
            session = next; catalog = nextCatalog; reportedLifecycleFailure = false;
            try { Hook(); next.StartInApiContext(); }
            catch (Exception startFailure)
            {
                try { Stop(); }
                catch (Exception cleanupFailure) { throw new AggregateException(startFailure, cleanupFailure); }
                throw;
            }
        }

        internal void Stop()
        {
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Record("StopEntered");
#endif
            if (session == null) { Unhook(); return; }
            try { Observe(session.StopInApiContext()); }
            finally { catalog.Clear(); }
            if (session.PollRetirementInApiContext()) Unhook();
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Record("StopReturned");
#endif
        }
        void OnIdling(object sender, IdlingEventArgs args)
        {
            try
            {
                if (session == null) { Unhook(); return; }
                if (session.IsRunning) { catalog.InvalidateClosedDocuments(); session.RearmInApiContext(); }
                else if (session.PollRetirementInApiContext()) Unhook();
            }
            catch (Exception error)
            {
                if (!reportedLifecycleFailure) { reportedLifecycleFailure = true; Trace.TraceError("BIM bridge lifecycle: {0}", error.GetType().Name); }
            }
        }
        void OnDocumentClosed(object sender, DocumentClosedEventArgs args) { catalog?.InvalidateClosedDocuments(); }
        void OnDocumentSavedAs(object sender, DocumentSavedAsEventArgs args) { catalog?.InvalidateSavedAs(args.Document); }
        void Hook()
        {
            if (hooked) return;
            application.Idling += OnIdling;
            try
            {
                application.ControlledApplication.DocumentClosed += OnDocumentClosed;
                try { application.ControlledApplication.DocumentSavedAs += OnDocumentSavedAs; hooked = true;
#if BIM_BRIDGE_DIAGNOSTICS
                    BridgeDiagnostics.Record("EventsHooked");
#endif
                }
                catch { application.ControlledApplication.DocumentClosed -= OnDocumentClosed; throw; }
            }
            catch { application.Idling -= OnIdling; throw; }
        }
        void Unhook()
        {
            if (!hooked) return;
            application.Idling -= OnIdling;
            application.ControlledApplication.DocumentClosed -= OnDocumentClosed;
            application.ControlledApplication.DocumentSavedAs -= OnDocumentSavedAs;
            hooked = false;
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Record("EventsUnhooked");
#endif
        }
        public Result OnShutdown(UIControlledApplication app)
        {
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Record("ApplicationShutdownEntered");
#endif
            try { Stop(); }
            catch (Exception error) { Trace.TraceError("BIM bridge shutdown: {0}", error.GetType().Name); }
            finally { Unhook(); Current = null; }
            // No waits on API execution, borrowed cancellation callbacks or network tasks.
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.UnhookClosing(app);
            BridgeDiagnostics.HoldLifecycle("ShutdownAfterStop");
            BridgeDiagnostics.Record("ApplicationShutdownReturned");
#endif
            return Result.Succeeded;
        }
        static void Observe(Task task)
        {
            _ = task.ContinueWith(done => Trace.TraceError("BIM bridge network cleanup: {0}", done.Exception.GetType().Name),
                System.Threading.CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
        static DispatchSignalResult Map(ExternalEventRequest result)
        {
            switch (result)
            {
                case ExternalEventRequest.Accepted: return DispatchSignalResult.Accepted;
                case ExternalEventRequest.Pending: return DispatchSignalResult.Pending;
                case ExternalEventRequest.Denied: return DispatchSignalResult.Denied;
                case ExternalEventRequest.TimedOut: return DispatchSignalResult.TimedOut;
                default: return DispatchSignalResult.Denied;
            }
        }
    }
}
