using BimAgentLab.RevitReadiness.Mcp;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.Text.Json;

string? evidence = null;
var policy = McpProtocolPolicy.Compatible;
for (int i = 0; i < args.Length; i += 2)
{
    if (i + 1 >= args.Length) throw new ArgumentException("Probe option requires a value.");
    switch (args[i])
    {
        case "--wire-evidence": evidence = Path.GetFullPath(args[i + 1]); break;
        case "--require-protocol" when args[i + 1] == "2026-07-28": policy = McpProtocolPolicy.Strict20260728; break;
        default: throw new ArgumentException("Usage: probe [--wire-evidence <new-directory>] [--require-protocol 2026-07-28]");
    }
}
if (evidence is not null) evidence = Path.Combine(evidence, "session-" + Guid.NewGuid().ToString("N"));
var handlers = new McpServerHandlers
{
    ListToolsHandler = (_, ct) => ValueTask.FromResult(new ListToolsResult
    {
        Tools = [new Tool
        {
            Name = "lab_protocol_probe",
            Description = "Synthetic compatibility test only. No Revit access. Set fail=true for a deliberate structured error.",
            InputSchema = JsonSerializer.SerializeToElement(new
            {
                type = "object", properties = new { fail = new { type = "boolean" } },
                required = new[] { "fail" }, additionalProperties = false
            }),
            Annotations = new ToolAnnotations { ReadOnlyHint = true, DestructiveHint = false, OpenWorldHint = false }
        }]
    }),
    CallToolHandler = (request, ct) =>
    {
        ct.ThrowIfCancellationRequested();
        var args = request.Params?.Arguments;
        if (request.Params?.Name != "lab_protocol_probe" || args?.Count != 1 || !args.TryGetValue("fail", out var fail)
            || fail.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new ArgumentException("Expected lab_protocol_probe with one boolean fail argument.");
        bool error = fail.GetBoolean();
        var data = JsonSerializer.SerializeToElement(new { synthetic = true, revitAccess = false, code = error ? "ProbeFailure" : "ProbeSuccess" });
        return ValueTask.FromResult(new CallToolResult
        {
            IsError = error, StructuredContent = data,
            Content = [new TextContentBlock { Text = data.GetRawText() }]
        });
    }
};
try { await McpHost.RunStdioAsync(handlers, evidence, CancellationToken.None, policy); }
catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
