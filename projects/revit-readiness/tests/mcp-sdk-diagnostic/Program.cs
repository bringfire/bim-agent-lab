using ModelContextProtocol.Client;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.Text.Json;

// Isolated SDK behavior experiment, not a desktop or Revit acceptance test.
// The SDK package is inherited from the locked host project; McpHost is never called.

if (args.Length == 2 && args[0] == "--server")
{
    var options = new McpServerOptions
    {
        ProtocolVersion = args[1] == "strict" ? "2026-07-28" : null,
        ServerInfo = new Implementation { Name = "sdk-default-diagnostic", Version = "1.0" },
        Capabilities = new ServerCapabilities { Tools = new ToolsCapability() },
        Handlers = new McpServerHandlers
        {
            ListToolsHandler = (_, _) => ValueTask.FromResult(new ListToolsResult { Tools = [new Tool { Name = "diagnostic", InputSchema = JsonSerializer.SerializeToElement(new { type = "object", properties = new { fail = new { type = "boolean" } }, required = new[] { "fail" }, additionalProperties = false }) }] }),
            CallToolHandler = (r, _) =>
            {
                bool fail = r.Params!.Arguments!["fail"].GetBoolean();
                var data = JsonSerializer.SerializeToElement(new { synthetic = true, revitAccess = false, code = fail ? "ProbeFailure" : "ProbeSuccess" });
                return ValueTask.FromResult(new CallToolResult { IsError = fail, StructuredContent = data, Content = [new TextContentBlock { Text = data.GetRawText() }] });
            }
        }
    };
    await using var transport = new StdioServerTransport(options);
    await using var server = McpServer.Create(transport, options);
    await server.RunAsync(); return;
}
var rows = new List<object>(); bool all = true;
foreach (string mode in new[] { "strict", "default" })
    foreach (string version in new[] { "2025-06-18", "2025-11-25", "2026-07-28" })
    {
        bool expected = mode == "default" || version == "2026-07-28";
        try
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var transport = new StdioClientTransport(new StdioClientTransportOptions { Command = Environment.ProcessPath!, Arguments = ["--server", mode], Name = "isolated SDK diagnostic" });
            await using var client = await McpClient.CreateAsync(transport, new McpClientOptions { ProtocolVersion = version }, cancellationToken: deadline.Token);
            var list = await client.ListToolsAsync(cancellationToken: deadline.Token);
            var success = await client.CallToolAsync("diagnostic", new Dictionary<string, object?> { { "fail", false } }, cancellationToken: deadline.Token);
            var failure = await client.CallToolAsync("diagnostic", new Dictionary<string, object?> { { "fail", true } }, cancellationToken: deadline.Token);
            bool correct = expected && client.NegotiatedProtocolVersion == version && list.Count == 1 && list[0].Name == "diagnostic" && success.IsError == false && failure.IsError == true && success.StructuredContent?.GetProperty("code").GetString() == "ProbeSuccess" && failure.StructuredContent?.GetProperty("code").GetString() == "ProbeFailure";
            rows.Add(new { server = mode, requested = version, negotiated = client.NegotiatedProtocolVersion, connected = true, successIsError = success.IsError, failureIsError = failure.IsError, expectedOutcomeObserved = correct }); all &= correct;
        }
        catch (Exception e)
        {
            bool correct = !expected && e.ToString().Contains("initialize handshake");
            rows.Add(new { server = mode, requested = version, connected = false, error = e.Message, expectedOutcomeObserved = correct }); all &= correct;
        }
    }
Console.WriteLine(JsonSerializer.Serialize(new { client = "Isolated official SDK diagnostic; not a desktop acceptance run", package = "ModelContextProtocol 2.2.0", allExpectedOutcomesObserved = all, rows }, new JsonSerializerOptions { WriteIndented = true }));
Environment.ExitCode = all ? 0 : 1;
