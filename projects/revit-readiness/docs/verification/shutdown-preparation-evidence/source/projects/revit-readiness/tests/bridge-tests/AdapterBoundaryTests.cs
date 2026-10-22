using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    sealed class HostEndpointDouble : IHostBridgeEndpoint
    {
        public string Failure = "";
        public bool Published;
        public int Starts, Stops;
        public TaskCompletionSource<bool>? HoldStop;
        public void Start(Func<BridgeExecutionContext, DispatchTicket> submit)
        { Starts++; if (Failure == "listener") throw new IOException("listener"); Published = true; if (Failure == "descriptor") throw new IOException("descriptor"); }
        public Task StopAsync() { Stops++; Published = false; return HoldStop?.Task ?? Task.CompletedTask; }
    }
    sealed class ObservedHostEndpoint : IHostBridgeEndpoint
    {
        readonly PipeEndpoint endpoint;
        readonly Action<BridgeExecutionContext, DispatchTicket> admitted;
        readonly Action? beforeStop;
        public ObservedHostEndpoint(PipeEndpoint endpoint, Action<BridgeExecutionContext, DispatchTicket> admitted, Action? beforeStop = null)
        { this.endpoint = endpoint; this.admitted = admitted; this.beforeStop = beforeStop; }
        public void Start(Func<BridgeExecutionContext, DispatchTicket> submit) => endpoint.Start(c =>
        { var ticket = submit(c); admitted(c, ticket); return ticket; });
        public Task StopAsync() { beforeStop?.Invoke(); return endpoint.StopAsync(); }
    }
    static async Task AdapterBoundaryTests()
    {
        await Test("Queued retirement may precede endpoint cancellation", () => Sync(() =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("queued-retirement");
            var clock = new SystemBridgeClock(); BridgeExecutionContext? context = null;
            using var notification = new ManualResetEventSlim();
            var admitted = new TaskCompletionSource<DispatchTicket>(TaskCreationOptions.RunContinuationsAsynchronously);
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), clock);
            int executed = 0, disposed = 0;
            var host = new HostBridgeSession(descriptor.BridgeSessionId, clock,
                () => new ApiEventLease(() => DispatchSignalResult.Accepted, () => disposed++),
                () => new ObservedHostEndpoint(endpoint, (c, ticket) => { context = c; admitted.TrySetResult(ticket); }, () =>
                {
                    // Force the legal handoff after queue cancellation/quiescence and
                    // before endpoint stopping signals the lifetime's owner token.
                    Until(() => { try { _ = context!.Cancellation.WaitHandle; return false; }
                        catch (ObjectDisposedException) { return true; } }).GetAwaiter().GetResult();
                }));
            host.StartInApiContext(); using var pipe = RawClient(descriptor);
            CancellationTokenRegistration registration = default;
            try
            {
                pipe.ConnectAsync(2000).GetAwaiter().GetResult(); using var request = LiveRequest("queued-retirement");
                FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request.Context.Request), 65536, CancellationToken.None).GetAwaiter().GetResult();
                var ticket = Within(admitted.Task).GetAwaiter().GetResult();
                registration = context!.Cancellation.Register(() => notification.Set());
                host.StopInApiContext().GetAwaiter().GetResult();
                Check(!notification.IsSet, "Already disposed queued work unexpectedly received notification");
                host.DrainInApiContext(c => { executed++; return QueueResponse(c); });
                Check(ticket.Response.IsCanceled && ticket.Quiesced.IsCompleted, "Queued request not retired");
                Equal(0, executed); Equal(1, disposed);
                Check(host.IsRetired && !File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Host resources remained");
                Console.WriteLine("QUEUED_RETIREMENT_PROOF disposedBeforeEndpointStop=true callbacks=0 executorCalls=0 responseCanceled=true");
            }
            finally { registration.Dispose(); host.StopInApiContext().GetAwaiter().GetResult(); }
        }));
        await Test("Production host Stop releases real pipes while executing borrower notification is held", () => Sync(() =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("host-stop"); var clock = new SystemBridgeClock();
            using var notification = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var admitted = new TaskCompletionSource<(BridgeExecutionContext Context, DispatchTicket Ticket)>(TaskCreationOptions.RunContinuationsAsynchronously);
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), clock);
            int disposed = 0;
            var host = new HostBridgeSession(descriptor.BridgeSessionId, clock,
                () => new ApiEventLease(() => DispatchSignalResult.Accepted, () => disposed++),
                () => new ObservedHostEndpoint(endpoint, (context, ticket) => admitted.TrySetResult((context, ticket))));
            host.StartInApiContext();
            using var pipe = RawClient(descriptor); CancellationTokenRegistration registration = default;
            try
            {
                pipe.ConnectAsync(2000).GetAwaiter().GetResult(); using var request = LiveRequest("host-held-notification");
                FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request.Context.Request), 65536, CancellationToken.None).GetAwaiter().GetResult();
                var work = Within(admitted.Task).GetAwaiter().GetResult();
                int executorCalls = 0; Exception? executorAssertion = null;
                host.DrainInApiContext(context =>
                {
                    try
                    {
                    executorCalls++;
                    registration = context.Cancellation.Register(() => { notification.Set(); Hold(release); });
                    var stopped = host.StopInApiContext();
                    Progress(stopped, "host network retirement while callback held").GetAwaiter().GetResult();
                    Check(notification.Wait(2000), "Notification did not run for live executor borrower");
                    Check(context.IsCancellationRequested, "Executing request was not invalidated");
                    Throws<OperationCanceledException>(context.ThrowIfInactive);
                    Check(host.PollRetirementInApiContext(), "Host did not retire while notification held");
                    Equal(1, disposed);
                    Check(work.Ticket.Response.IsCanceled && !work.Ticket.Quiesced.IsCompleted, "Response/quiescence boundary lost");
                    Check(!File.Exists(Path.Combine(directory, descriptor.BridgeSessionId + ".json")), "Descriptor remained");
                    _ = context.Cancellation.WaitHandle;
                    host.StopInApiContext().GetAwaiter().GetResult();
                    return QueueResponse(context); // canceled publication cannot become success
                    }
                    catch (Exception error) { executorAssertion = error; throw; }
                });
                if (executorAssertion != null) throw executorAssertion; // queue deliberately maps executor faults
                Equal(1, executorCalls);
                Check(work.Ticket.Response.IsCanceled && work.Ticket.Quiesced.IsCompleted, "Executor did not quiesce");
                _ = work.Context.Cancellation.WaitHandle; // callback still borrows the source
                release.Set(); registration.Dispose();
                Until(() => { try { _ = work.Context.Cancellation.WaitHandle; return false; } catch (ObjectDisposedException) { return true; } }).GetAwaiter().GetResult();
                Console.WriteLine("HOST_STOP_PROOF networkBeforeNotificationRelease=true descriptorRemoved=true eventDisposed=true");
            }
            finally { release.Set(); registration.Dispose(); host.StopInApiContext().GetAwaiter().GetResult(); }
        }));
        await Test("Event disposal failure cannot skip network cleanup and remains retryable", () => Sync(() =>
        {
            var endpoint = new HostEndpointDouble(); int disposal = 0;
            var host = new HostBridgeSession("session-1", new FakeClock(), () => new ApiEventLease(() => DispatchSignalResult.Accepted,
                () => { disposal++; if (disposal == 1) throw new IOException("dispose once"); }), () => endpoint);
            host.StartInApiContext(); Throws<IOException>(() => host.StopInApiContext());
            Check(!endpoint.Published && endpoint.Stops > 0, "Cleanup skipped network");
            host.StopInApiContext().GetAwaiter().GetResult(); Check(host.IsRetired, "Retry did not retire"); Equal(2, disposal);
        }));
        await Test("Production host publishes a context and revokes a canceled issuance", () => Sync(() =>
        {
            var clock = new FakeClock(); var host = new HostBridgeSession("session-1", clock,
                () => new ApiEventLease(() => DispatchSignalResult.Accepted, () => { }), () => new HostEndpointDouble());
            host.StartInApiContext();
            foreach (bool cancel in new[] { false, true })
            {
                using var source = new CancellationTokenSource(); using var lifetime = new RequestLifetime(Request(), RequestBudget.Admit(clock), source.Token, CancellationToken.None);
                var ticket = host.Submit(lifetime.Context); string token = "";
                host.DrainInApiContext(c => ReadinessExecutionFlow.IssueContext(c, host.Contexts, Binding(), host.ResponseFor(c), lease =>
                { token = lease.Token; if (cancel) source.Cancel(); return QueueResponse(c); }));
                if (cancel)
                {
                    Check(ticket.Response.IsCanceled, "Canceled issuance became success");
                    Equal("ContextExpired", Throws<BridgeException>(() => host.Contexts.Validate(token, Binding())).Error.Code);
                }
                else { Equal("session-1", ticket.Response.GetAwaiter().GetResult().BridgeSessionId); host.Contexts.Validate(token, Binding()); }
                ticket.Quiesced.GetAwaiter().GetResult();
            }
            host.StopInApiContext().GetAwaiter().GetResult();
        }));
        foreach (string fail in new[] { "event", "listener", "descriptor", "none" })
            await Test("Production session acquisition/retirement " + fail, () => Sync(() =>
            {
                var endpoint = new HostEndpointDouble { Failure = fail }; int created = 0, disposed = 0;
                var host = new HostBridgeSession("session-1", new FakeClock(), () =>
                { if (fail == "event") throw new IOException("event"); created++; return new ApiEventLease(() => DispatchSignalResult.Accepted, () => disposed++); }, () => endpoint);
                if (fail == "none") { host.StartInApiContext(); host.StartInApiContext(); Equal(1, endpoint.Starts); }
                else Throws<IOException>(host.StartInApiContext);
                host.StopInApiContext().GetAwaiter().GetResult(); host.StopInApiContext().GetAwaiter().GetResult();
                Check(host.PollRetirementInApiContext() && host.IsRetired, "Session not retired");
                Equal(created, disposed); Check(!endpoint.Published, "Owned descriptor survived");
            }));
        await Test("Event retirement never waits on an in-flight native Raise", async () =>
        {
            using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim(); int disposed = 0;
            var signal = new ApiEventLease(() => { entered.Set(); Hold(release); return DispatchSignalResult.Pending; }, () => disposed++);
            var raising = Task.Run(signal.Raise);
            try { Check(entered.Wait(2000), "Raise not entered"); Check(!signal.TryRetireInApiContext(), "Disposed borrowed event"); Equal(0, disposed); Equal(DispatchSignalResult.Denied, signal.Raise()); }
            finally { release.Set(); await raising; }
            Check(signal.TryRetireInApiContext(), "Retirement failed"); Check(signal.TryRetireInApiContext(), "Repeated retirement failed"); Equal(1, disposed);
        });
        await Test("Session stop clears contexts and returns without network completion", () => Sync(() =>
        {
            var endpoint = new HostEndpointDouble { HoldStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously) };
            int disposed = 0; var host = new HostBridgeSession("session-1", new FakeClock(),
                () => new ApiEventLease(() => DispatchSignalResult.Accepted, () => disposed++), () => endpoint);
            host.StartInApiContext(); var lease = host.Contexts.Issue(Binding()); var stopping = host.StopInApiContext();
            Check(!stopping.IsCompleted, "Test network not held"); Equal(1, disposed); Check(!host.IsRunning && !host.IsRetired, "Retired too early");
            Equal("ContextExpired", Throws<BridgeException>(() => host.Contexts.Validate(lease.Token, Binding())).Error.Code);
            endpoint.HoldStop.SetResult(true); Check(host.PollRetirementInApiContext(), "Stopped network not retired");
        }));
        await Test("Autodesk adapter source has only read-only query dependencies", () => Sync(() =>
        {
            string project = Path.Combine(Directory.GetCurrentDirectory(), "src", "revit-addin");
            string csproj = File.ReadAllText(Path.Combine(project, "BimAgentLab.RevitReadiness.csproj"));
            Check(!csproj.Contains("ModelContextProtocol"), "MCP SDK in Revit");
            foreach (string file in new[] { "BridgeApplication.cs", "BridgeCommands.cs", "ReadinessExternalEventHandler.cs", "RevitReadinessExecutor.cs", "RevitContextCatalog.cs" })
                Check(File.Exists(Path.Combine(project, file)), "Missing adapter " + file);
            string executor = File.ReadAllText(Path.Combine(project, "RevitReadinessExecutor.cs"));
            string app = File.ReadAllText(Path.Combine(project, "BridgeApplication.cs"));
            Check(app.Contains("DocumentSavedAs +=") && app.Contains("DocumentSavedAs -="), "Save As must invalidate even when later saved back to original path");
            foreach (string result in new[] { "Accepted", "Pending", "Denied", "TimedOut" }) Check(app.Contains("case ExternalEventRequest." + result), "Unmapped native result " + result);
            Check(executor.Contains("ReadinessExecutionFlow.Run") && executor.Contains("RequiredInformationReader.Read("), "Production reader/flow not connected");
            foreach (string forbidden in new[] { "RequiredInformationCommand", "Transaction(", "TaskDialog", "JsonFile.Write", "ExcelReportProcess", "Task.Run", "TransportCancellation" })
                Check(!executor.Contains(forbidden), "Forbidden query behavior " + forbidden);
        }));
    }
}
