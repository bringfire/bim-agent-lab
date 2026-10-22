using ModelContextProtocol.Client;
using System.Text.Json;

string? server = null, evidence = null;
string protocol = "2026-07-28";
string scenario = "probe", expected = "ReadinessReport";
string? contextToken = null, pauseFile = null, finishFile = null;
string? interruptMode = null, interruptFile = null;
int? revitPid = null;
List<string> serverArgs = [];
for (int i = 0; i < args.Length; i += 2)
{
    if (i + 1 >= args.Length) throw new ArgumentException("Option needs a value.");
    switch (args[i])
    {
        case "--server": server = Path.GetFullPath(args[i + 1]); break;
        case "--server-arg": serverArgs.Add(args[i + 1]); break;
        case "--scenario" when args[i + 1] is "probe" or "readiness" or "interrupt": scenario = args[i + 1]; break;
        case "--interrupt-mode" when args[i + 1] is "cancel" or "eof": interruptMode = args[i + 1]; break;
        case "--interrupt-file": interruptFile = Path.GetFullPath(args[i + 1]); break;
        case "--expected": expected = args[i + 1]; break;
        case "--context-token": contextToken = args[i + 1]; break;
        case "--pause-file": pauseFile = Path.GetFullPath(args[i + 1]); break;
        case "--finish-file": finishFile = Path.GetFullPath(args[i + 1]); break;
        case "--revit-pid": revitPid = int.Parse(args[i + 1]); if(revitPid <= 0) throw new ArgumentException("Positive Revit PID required."); break;
        case "--evidence": evidence = Path.GetFullPath(args[i + 1]); break;
        case "--protocol" when args[i + 1] is "2025-06-18" or "2025-11-25" or "2026-07-28": protocol = args[i + 1]; break;
        default: throw new ArgumentException("Expected --server, --server-arg, --scenario probe, --evidence, --protocol (accepted revision).");
    }
}
if (server is null || evidence is null) throw new ArgumentException("--server and --evidence are required.");
if (scenario == "interrupt") return await InterruptionScenario.Run(server, serverArgs, evidence, protocol, revitPid,
    contextToken, interruptMode, interruptFile, finishFile);
if (interruptMode is not null || interruptFile is not null) throw new ArgumentException("Interruption options require the interrupt scenario.");
if (scenario == "readiness") return await ReadinessScenario.Run(server,serverArgs,evidence,protocol,expected,revitPid,contextToken,pauseFile,finishFile);
if (finishFile is not null) throw new ArgumentException("--finish-file requires --scenario readiness.");
Directory.CreateDirectory(evidence);
using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(20));
var transport = new StdioClientTransport(new StdioClientTransportOptions { Command = server, Arguments = serverArgs, Name = "lab SDK verification client" });
await using var client = await McpClient.CreateAsync(transport, new McpClientOptions { ProtocolVersion = protocol }, cancellationToken: stop.Token);
var tools = await client.ListToolsAsync(cancellationToken: stop.Token);
var success = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = false }, cancellationToken: stop.Token);
var failure = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = true }, cancellationToken: stop.Token);
bool pass = client.NegotiatedProtocolVersion == protocol && tools.Count == 1 && tools[0].Name == "lab_protocol_probe"
    && success.IsError == false && success.StructuredContent?.GetProperty("code").GetString() == "ProbeSuccess"
    && failure.IsError == true && failure.StructuredContent?.GetProperty("code").GetString() == "ProbeFailure";
File.WriteAllText(Path.Combine(evidence, "sdk-client-result.json"), JsonSerializer.Serialize(new
{
    clientKind = "SDK harness, not a desktop acceptance run", protocol = client.NegotiatedProtocolVersion,
    tools = tools.Select(t => t.Name), success = success.StructuredContent, failure = failure.StructuredContent, pass
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(pass ? "PASS SDK probe (desktop verification remains separate)" : "FAIL SDK probe");
return pass ? 0 : 1;
