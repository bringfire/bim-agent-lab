using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitReadiness;

namespace BimAgentLab.AdapterHandoff.Tests;

internal static class Program
{
    const string Valid = "{\"SchemaVersion\":1,\"ChecklistId\":\"handoff\",\"ProjectUniqueId\":\"project\",\"ProjectFileName\":\"model.rvt\",\"LevelUniqueId\":\"level\",\"PhaseUniqueId\":\"phase\",\"Cases\":[{\"UniqueId\":\"element\",\"CaseId\":\"W01\"}],\"Requirements\":[]}";
    static int Main()
    {
        int passed = 0, failed = 0;
        Console.WriteLine("EXCEPTION_BASE InvalidDataException=" + typeof(InvalidDataException).BaseType.FullName);
        foreach (string operation in new[] { BridgeLimits.GetContext, BridgeLimits.CheckInformation })
        foreach (string input in new[] { "missing", "empty", "oversize", "valid", "valid-at-limit" })
        {
            try { Run(operation, input); passed++; Console.WriteLine("PASS adapter handoff operation=" + operation + " input=" + input); }
            catch (Exception error) { failed++; Console.WriteLine("FAIL adapter handoff operation=" + operation + " input=" + input + ": " + error.Message); }
        }
        Console.WriteLine($"Adapter handoff tests: {passed} passed; {failed} failed.");
        Console.WriteLine("BOUNDARY production catalog/executor/codec/queue/flow; Autodesk objects and reader are doubles; no live Revit acceptance");
        return failed == 0 ? 0 : 1;
    }
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Run(string operation, string input)
    {
        using var fixture = new Fixture();
        // Acquire a real production context before preparing a check's negative control.
        // GetContext cases go straight through their sidecar preparation path.
        string token = null;
        if (operation == BridgeLimits.CheckInformation)
        {
            fixture.Write(Valid);
            var contextResponse = fixture.Call(BridgeLimits.GetContext, null);
            Require(contextResponse.Error == null, "Context setup failed");
            token = BridgeJson.Deserialize<ReadinessContextResult>(Encoding.UTF8.GetBytes(contextResponse.ResultJson)).ContextToken;
        }
        switch (input)
        {
            case "missing": if (File.Exists(fixture.Sidecar)) File.Delete(fixture.Sidecar); break;
            case "empty": fixture.Write(""); break;
            case "oversize": fixture.Write(new string(' ', ChecklistSnapshot.MaxBytes + 1)); break;
            case "valid": fixture.Write(Valid); break;
            case "valid-at-limit":
                fixture.Write(Valid.PadRight(ChecklistSnapshot.MaxBytes, ' '));
                // A valid changed snapshot requires a fresh context before its check.
                if (operation == BridgeLimits.CheckInformation)
                {
                    var fresh = fixture.Call(BridgeLimits.GetContext, null);
                    Require(fresh.Error == null, "Exact-limit context setup failed");
                    token = BridgeJson.Deserialize<ReadinessContextResult>(Encoding.UTF8.GetBytes(fresh.ResultJson)).ContextToken;
                }
                break;
        }
        RequiredInformationReader.Calls = 0;
        var result = fixture.Call(operation, token);
        bool valid = input.StartsWith("valid", StringComparison.Ordinal);
        string expected = valid ? "success" : input == "missing" ? "ChecklistMissing" : "ChecklistInvalid";
        string actual = result.Error?.Code ?? "success";
        Console.WriteLine($"HANDOFF input={input} operation={operation} expected={expected} actual={actual} readerCalls={RequiredInformationReader.Calls}");
        Require(actual == expected, "Expected " + expected + ", got " + actual);
        Require(RequiredInformationReader.Calls == (valid && operation == BridgeLimits.CheckInformation ? 1 : 0), "Unexpected reader execution");
        if (!valid)
        {
            Require(result.ResultJson == null && result.Error.RequestId == result.RequestId && !result.Error.Retryable, "Error contract lost at queue handoff");
            Require(!string.IsNullOrWhiteSpace(result.Error.Message), "Missing remediation message");
        }
        else if (operation == BridgeLimits.GetContext)
            Require(BridgeJson.Deserialize<ReadinessContextResult>(Encoding.UTF8.GetBytes(result.ResultJson)).ContextToken.Length > 0, "No context issued");
        else Require(result.ResultJson.Contains("\"CoverageComplete\":false") && result.ResultJson.Contains("\"Complete\":false"), "Reader result not preserved");
    }
    sealed class Fixture : IDisposable
    {
        internal readonly string Sidecar;
        readonly HostBridgeSession host;
        readonly RevitReadinessExecutor executor;
        readonly UIApplication ui;
        readonly SystemBridgeClock clock = new SystemBridgeClock();
        internal Fixture()
        {
            string directory = Path.GetFullPath(Path.Combine("samples", "local", "adapter-handoff", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(directory);
            var document = new Document { PathName = Path.Combine(directory, "model.rvt"), ProjectInformation = new Element { UniqueId = "project" } };
            Sidecar = Path.ChangeExtension(document.PathName, ".checklist.json");
            document.Elements.Add("level", new Level { Name = "Level 1" });
            document.Elements.Add("phase", new Phase { Name = "New" });
            ui = new UIApplication { ActiveUIDocument = new UIDocument { Document = document } };
            host = new HostBridgeSession("handoff-session", clock,
                () => new ApiEventLease(() => DispatchSignalResult.Accepted, () => { }), () => new EndpointDouble());
            executor = new RevitReadinessExecutor(new BridgeSessionDescriptor { BridgeSessionId = "handoff-session", RevitPid = 73 }, host, new RevitContextCatalog("handoff-session"));
            host.StartInApiContext();
        }
        internal void Write(string content) => File.WriteAllText(Sidecar, content, new UTF8Encoding(false, true));
        internal BridgeResponse Call(string operation, string token)
        {
            var budget = RequestBudget.Admit(clock);
            var request = new BridgeRequest { ContractVersion = 1, RequestId = Guid.NewGuid().ToString("N"), Operation = operation,
                ContextToken = token, DeadlineUtc = BridgeJson.FormatUtc(budget.DeadlineUtc) };
            using var lifetime = new RequestLifetime(request, budget, CancellationToken.None, CancellationToken.None);
            var ticket = host.Submit(lifetime.Context);
            try
            {
                host.DrainInApiContext(context => executor.Execute(ui, context));
                var response = ticket.Response.GetAwaiter().GetResult();
                Require(response.RequestId == request.RequestId && response.BridgeSessionId == "handoff-session", "Lost envelope correlation");
                return response;
            }
            finally { lifetime.DisposeAfterQuiescenceAsync(ticket).GetAwaiter().GetResult(); }
        }
        public void Dispose() => host.StopInApiContext().GetAwaiter().GetResult();
    }
    sealed class EndpointDouble : IHostBridgeEndpoint
    {
        public void Start(Func<BridgeExecutionContext, DispatchTicket> submit) { }
        public Task StopAsync() => Task.CompletedTask;
    }
}
