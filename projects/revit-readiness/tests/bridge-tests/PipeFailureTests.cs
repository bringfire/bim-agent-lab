using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    static async Task PipeFailureTests()
    {
        await Test("Native pipe is asynchronous byte mode and a foreign-logon ACL denies access", async () =>
        {
            var descriptor = ManualDescriptor();
            using var allowed = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid);
            Check(allowed.IsAsync, "Native handle must support overlapped I/O"); Equal(PipeTransmissionMode.Byte, allowed.TransmissionMode);
            using var denied = LocalPipeFactory.Create(descriptor.PipeName + ".denied", "S-1-5-5-0-0");
            using var client = new NamedPipeClientStream(".", descriptor.PipeName + ".denied", PipeDirection.InOut, PipeOptions.Asynchronous);
            await ThrowsAsync<UnauthorizedAccessException>(() => client.ConnectAsync(1000));
        });
        await Test("Failed descriptor publication unwinds every acquired native pipe", () => Sync(() =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("start-failure");
            string path = Path.Combine(directory, descriptor.BridgeSessionId + ".json"); File.WriteAllText(path, "foreign");
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), new SystemBridgeClock());
            Throws<IOException>(() => endpoint.Start(c => Ticket(c, descriptor.BridgeSessionId)));
            Equal("foreign", File.ReadAllText(path));
            using var available = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid, true);
            Equal(1, Directory.GetFiles(directory).Length);
        }));
        await Test("Stop releases network even if descriptor cleanup fails and can retry cleanup", async () =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("cleanup-failure");
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), new SystemBridgeClock());
            endpoint.Start(c => Ticket(c, descriptor.BridgeSessionId));
            string path = Path.Combine(directory, descriptor.BridgeSessionId + ".json");
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                await ThrowsAsync<IOException>(() => endpoint.StopAsync());
            using (var available = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid, true)) { }
            await endpoint.StopAsync(); Check(!File.Exists(path), "Retry did not finish cleanup");
        });
        await Test("Valid expired and beyond-policy requests get correlated failures without dispatch", async () =>
        {
            var descriptor = ManualDescriptor(); int submitted = 0;
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("deadlines")), new SystemBridgeClock());
            endpoint.Start(c => { submitted++; return Ticket(c, descriptor.BridgeSessionId); });
            try
            {
                foreach (var entry in new[] { (Seconds: -1, Code: "DeadlineExceeded"), (Seconds: 60, Code: "InvalidRequest") })
                {
                    using var pipe = RawClient(descriptor); await pipe.ConnectAsync(2000);
                    var request = Request(DateTime.UtcNow.AddSeconds(entry.Seconds).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
                    await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request), 65536, CancellationToken.None);
                    var response = BridgeJson.Deserialize<BridgeResponse>(await Within(FrameCodec.ReadAsync(pipe, 4194304, CancellationToken.None)));
                    Equal(entry.Code, response.Error!.Code); Equal(request.RequestId, response.Error.RequestId); Equal(descriptor.BridgeSessionId, response.BridgeSessionId);
                }
                Equal(0, submitted);
            }
            finally { await endpoint.StopAsync(); }
        });
        await Test("Submit errors retain correlation and unexpected errors expose no stack or message", async () =>
        {
            var descriptor = ManualDescriptor(); var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("submit-error")), new SystemBridgeClock());
            endpoint.Start(c => c.Request.RequestId == "busy" ? throw new BridgeException(new BridgeError { Code = "Busy", Message = "Queue full", RequestId = c.Request.RequestId, Retryable = true }) : throw new Exception("private failure detail"));
            try
            {
                foreach (string id in new[] { "busy", "unexpected" })
                {
                    using var request = LiveRequest(id); var response = await new PipeBridgeClient(descriptor).SendAsync(request.Context);
                    Equal(id == "busy" ? "Busy" : "InternalError", response.Error!.Code); Equal(id, response.Error.RequestId);
                    Check(!response.Error.Message.Contains("private failure detail"), "Internal details leaked");
                }
            }
            finally { await endpoint.StopAsync(); }
        });
        await Test("Client rejects wrong response request, session, contract and malformed JSON", async () =>
        {
            foreach (string fault in new[] { "request", "session", "version", "json" })
            {
                var descriptor = ManualDescriptor(); using var pipe = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid);
                var server = Task.Run(async () =>
                {
                    await pipe.WaitForConnectionAsync();
                    var request = BridgeJson.Deserialize<BridgeRequest>(await FrameCodec.ReadAsync(pipe, 65536, CancellationToken.None));
                    var response = Response(); response.RequestId = request.RequestId; response.BridgeSessionId = descriptor.BridgeSessionId;
                    if (fault == "request") response.RequestId = "wrong"; if (fault == "session") response.BridgeSessionId = "wrong";
                    byte[] bytes = BridgeJson.SerializeResponse(response);
                    if (fault == "version") bytes = Utf8(System.Text.Encoding.UTF8.GetString(bytes).Replace("\"ContractVersion\":1", "\"ContractVersion\":2"));
                    if (fault == "json") bytes = Utf8("{broken");
                    await FrameCodec.WriteAsync(pipe, bytes, 4194304, CancellationToken.None);
                });
                using var call = LiveRequest("correlation");
                Equal("BridgeUnavailable", (await ThrowsAsync<BridgeException>(() => new PipeBridgeClient(descriptor).SendAsync(call.Context))).Error.Code);
                await Within(server);
            }
        });
        await Test("Client connect is capped at two seconds and respects a shorter original budget", async () =>
        {
            var descriptor = ManualDescriptor(); var client = new PipeBridgeClient(descriptor);
            var timer = Stopwatch.StartNew(); using var request = LiveRequest("connect");
            Equal("BridgeUnavailable", (await ThrowsAsync<BridgeException>(() => client.SendAsync(request.Context))).Error.Code);
            Check(timer.Elapsed.TotalSeconds <= 3, "Connect exceeded two seconds plus scheduling allowance");
            var clock = new SystemBridgeClock(); var budget = RequestBudget.Receive(clock, clock.Timestamp, clock.UtcNow.AddMilliseconds(300));
            var shortRequest = Request(budget.DeadlineUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
            using var lifetime = new RequestLifetime(shortRequest, budget, CancellationToken.None, CancellationToken.None); timer.Restart();
            Equal("DeadlineExceeded", (await ThrowsAsync<BridgeException>(() => client.SendAsync(lifetime.Context))).Error.Code);
            Check(timer.Elapsed.TotalSeconds < 1, "Connect restarted the original budget");
        });
        await Test("Frame receipt consumes the request's existing budget", async () =>
        {
            var descriptor = ManualDescriptor(); double remaining = 99;
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("budget")), new SystemBridgeClock());
            endpoint.Start(c => { remaining = c.Budget.Remaining.TotalSeconds; return Ticket(c, descriptor.BridgeSessionId); });
            try
            {
                using var pipe = RawClient(descriptor); await pipe.ConnectAsync(2000);
                var request = Request(DateTime.UtcNow.AddSeconds(2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
                byte[] bytes = BridgeJson.Serialize(request); byte[] framed = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
                await pipe.WriteAsync(framed, 0, framed.Length - 1); await Task.Delay(700);
                await pipe.WriteAsync(framed, framed.Length - 1, 1);
                var response = BridgeJson.Deserialize<BridgeResponse>(await Within(FrameCodec.ReadAsync(pipe, 4194304, CancellationToken.None)));
                Equal(request.RequestId, response.RequestId); Check(remaining > 0 && remaining < 1.5, "Receive reset the deadline: " + remaining);
            }
            finally { await endpoint.StopAsync(); }
        });
        await Test("Stop during a blocked large response write releases the network worker", async () =>
        {
            var descriptor = ManualDescriptor(); var prepared = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("write-stop")), new SystemBridgeClock());
            endpoint.Start(c => { var response = Reply(c, descriptor.BridgeSessionId); response.ResultJson = new string('x', 1024 * 1024); prepared.SetResult(true); return new DispatchTicket(Task.FromResult(response), Task.CompletedTask); });
            using var pipe = RawClient(descriptor); endpoint.Start(c => Ticket(c, descriptor.BridgeSessionId));
            await pipe.ConnectAsync(2000); using var request = LiveRequest("write-stop");
            await FrameCodec.WriteAsync(pipe, BridgeJson.Serialize(request.Context.Request), 65536, CancellationToken.None);
            await Within(prepared.Task); // read header only; body exceeds the pipe's output buffer
            var header = new byte[4]; int n = await pipe.ReadAsync(header, 0, 4); Equal(4, n);
            await Within(endpoint.StopAsync());
        });
        await Test("Cancellation and expiry during selection prevent binding or a connection", async () =>
        {
            using var canceled = new CancellationTokenSource(); var descriptor = ManualDescriptor();
            using var request = LiveRequest("selection", canceled.Token);
            var provider = new BridgeClientProvider(new SessionCatalog(() => { canceled.Cancel(); return new[] { descriptor }; }), null, new SystemBridgeClock());
            await ThrowsAsync<OperationCanceledException>(() => provider.SendAsync(request.Context));
            var clock = new FakeClock(); var budget = RequestBudget.Admit(clock); using var lifetime = new RequestLifetime(Request(budget.DeadlineUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'")), budget, CancellationToken.None, CancellationToken.None);
            var expired = new BridgeClientProvider(new SessionCatalog(() => { clock.Advance(TimeSpan.FromSeconds(31)); return new[] { descriptor }; }), null, clock);
            Equal("DeadlineExceeded", (await ThrowsAsync<BridgeException>(() => expired.SendAsync(lifetime.Context))).Error.Code);
        });
        await Test("Actual foreign server PID is rejected before any request bytes", async () =>
        {
            var descriptor = ManualDescriptor(); string directory = Scratch("imposter"); string file = Path.Combine(directory, "descriptor.json");
            File.WriteAllBytes(file, BridgeJson.Serialize(descriptor));
            string host = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "net48", "BimAgentLab.Bridge.Tests.exe"));
            using var process = new Process { StartInfo = new ProcessStartInfo(host, "--pipe-imposter \"" + file + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true } };
            Check(process.Start(), "Imposter test process did not start"); var errors = process.StandardError.ReadToEndAsync();
            try
            {
                Equal("READY", await Within(process.StandardOutput.ReadLineAsync()));
                using var call = LiveRequest("foreign-server");
                Equal("BridgeUnavailable", (await ThrowsAsync<BridgeException>(() => new PipeBridgeClient(descriptor).SendAsync(call.Context))).Error.Code);
                Equal("BYTES 0", await Within(process.StandardOutput.ReadLineAsync()));
                await Until(() => process.HasExited); Equal(0, process.ExitCode); Equal("", await errors);
            }
            finally { if (!process.HasExited) process.Kill(); }
        });
        await Test("Provider rejects ambiguity through its real call path and retries only before binding", async () =>
        {
            var first = ManualDescriptor(); var second = ManualDescriptor(); var candidates = new[] { first, second };
            var provider = new BridgeClientProvider(new SessionCatalog(() => candidates), null, new SystemBridgeClock());
            using var ambiguous = LiveRequest("ambiguous");
            var error = await ThrowsAsync<BridgeException>(() => provider.SendAsync(ambiguous.Context));
            Equal("AmbiguousRevitInstance", error.Error.Code); Equal("ambiguous", error.Error.RequestId);
            var endpoint = new PipeEndpoint(first, new SessionCatalog(Scratch("retry-selection")), new SystemBridgeClock());
            endpoint.Start(c => Ticket(c, first.BridgeSessionId)); candidates = new[] { first };
            try { using var call = LiveRequest("deliberate-retry"); Equal(first.BridgeSessionId, (await provider.SendAsync(call.Context)).BridgeSessionId); }
            finally { await endpoint.StopAsync(); }
        });
        await Test("Stop at the acceptance handoff prevents parsing and submission", async () =>
        {
            var descriptor = ManualDescriptor(); int submitted = 0; PipeEndpoint? endpoint = null; Task? stop = null;
            endpoint = new PipeEndpoint(descriptor, new SessionCatalog(Scratch("accept-stop")), new SystemBridgeClock(), e =>
            { if (e.Stage == "Accepted") stop = endpoint!.StopAsync(); });
            endpoint.Start(c => { submitted++; return Ticket(c, descriptor.BridgeSessionId); });
            using var pipe = RawClient(descriptor); await pipe.ConnectAsync(2000);
            await Until(() => stop != null); await Within(stop!); Equal(0, submitted);
        });
        await Test("Failed duplicate start cannot remove the live owner's descriptor", async () =>
        {
            var descriptor = ManualDescriptor(); var catalog = new SessionCatalog(Scratch("duplicate-start"));
            var first = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock());
            var duplicate = new PipeEndpoint(descriptor, catalog, new SystemBridgeClock());
            first.Start(c => Ticket(c, descriptor.BridgeSessionId));
            try
            {
                Throws<System.ComponentModel.Win32Exception>(() => duplicate.Start(c => Ticket(c, descriptor.BridgeSessionId)));
                Equal(descriptor.BridgeSessionId, catalog.Resolve(null).BridgeSessionId);
                await duplicate.StopAsync();
                Equal(descriptor.BridgeSessionId, catalog.Resolve(null).BridgeSessionId);
                using var call = LiveRequest("existing-owner");
                Equal(descriptor.BridgeSessionId, (await new PipeBridgeClient(catalog.Resolve(null)).SendAsync(call.Context)).BridgeSessionId);
            }
            finally { await duplicate.StopAsync(); await first.StopAsync(); }
        });
    }
}
