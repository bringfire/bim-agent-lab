using ModelContextProtocol.Client;
using System.Text.Json;

string? server = null, evidence = null;
List<string> serverArgs = [];
for (int i = 0; i < args.Length; i += 2)
{
    if (i + 1 >= args.Length) throw new ArgumentException("Option needs a value.");
    switch (args[i])
    {
        case "--server": server = Path.GetFullPath(args[i + 1]); break;
        case "--server-arg": serverArgs.Add(args[i + 1]); break;
        case "--scenario" when args[i + 1] == "probe": break;
        case "--evidence": evidence = Path.GetFullPath(args[i + 1]); break;
        default: throw new ArgumentException("Expected --server, --server-arg, --scenario probe, --evidence.");
    }
}
if (server is null || evidence is null) throw new ArgumentException("--server and --evidence are required.");
Directory.CreateDirectory(evidence);
using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(20));
var transport = new StdioClientTransport(new StdioClientTransportOptions { Command = server, Arguments = serverArgs, Name = "lab SDK verification client" });
await using var client = await McpClient.CreateAsync(transport, new McpClientOptions { ProtocolVersion = "2026-07-28" }, cancellationToken: stop.Token);
var tools = await client.ListToolsAsync(cancellationToken: stop.Token);
var success = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = false }, cancellationToken: stop.Token);
var failure = await client.CallToolAsync("lab_protocol_probe", new Dictionary<string, object?> { ["fail"] = true }, cancellationToken: stop.Token);
bool pass = client.NegotiatedProtocolVersion == "2026-07-28" && tools.Count == 1 && tools[0].Name == "lab_protocol_probe"
    && success.IsError != true && success.StructuredContent?.GetProperty("code").GetString() == "ProbeSuccess"
    && failure.IsError == true && failure.StructuredContent?.GetProperty("code").GetString() == "ProbeFailure";
File.WriteAllText(Path.Combine(evidence, "sdk-client-result.json"), JsonSerializer.Serialize(new
{
    clientKind = "SDK harness, not a desktop acceptance run", protocol = client.NegotiatedProtocolVersion,
    tools = tools.Select(t => t.Name), success = success.StructuredContent, failure = failure.StructuredContent, pass
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(pass ? "PASS SDK probe (desktop verification remains separate)" : "FAIL SDK probe");
return pass ? 0 : 1;
