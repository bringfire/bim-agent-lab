using System.Text.Json;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitReadiness.Mcp;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BimAgentLab.Mcp.Tests;

internal static partial class Program
{
    record ArgumentCase(string Name, string Tool, string? Raw, bool Accepted = false, string? Token = null);

    // Expected outcomes come from the approved two schemas, not production validation.
    static IEnumerable<ArgumentCase> ArgumentCases()
    {
        string get = BridgeLimits.GetContext, check = BridgeLimits.CheckInformation;
        string[] scalars = ["null", "true", "false", "0", "1.25", "\"text\"", "\"\\ud800\"", "\"\\udc00\"", "[]", "{}"];
        string[] badTrees = ["\"\\ud800\"", "\"\\udc00\"", "[\"\\ud800\"]", "{\"text\":\"\\udc00\"}",
            "{\"\\ud800\":\"text\"}", "[{\"a\":[{\"b\":\"\\udc00\"}]}]", "{\"same\":1,\"same\":2}"];
        foreach (string tool in new[] { get, check })
        {
            yield return new("omitted", tool, null, tool == get);
            foreach (string raw in scalars)
                yield return new("root-kind-" + raw, tool, raw, tool == get && raw == "{}");
            foreach (string tree in scalars.Concat(badTrees))
            {
                yield return new("token-kind-" + tree, tool, "{\"contextToken\":" + tree + "}", tool == check && tree == "\"text\"", tool == check && tree == "\"text\"" ? "text" : null);
                yield return new("unknown-property-" + tree, tool, "{\"extra\":" + tree + "}");
            }
            foreach (string tree in badTrees)
            {
                yield return new("root-array-" + tree, tool, "[" + tree + "]");
                foreach (string token in new[] { "\"opaque\"", "\"\"", "null", "\"\\ud800\"", "[\"\\udc00\"]" })
                {
                    yield return new("extra-after-" + token + tree, tool, "{\"contextToken\":" + token + ",\"extra\":" + tree + "}");
                    yield return new("extra-before-" + token + tree, tool, "{\"extra\":" + tree + ",\"contextToken\":" + token + "}");
                }
            }
            foreach (string name in new[] { "\"wrong\"", "\"ContextToken\"", "\"contextToken \"", "\"\\ud800\"", "\"\\udc00\"", "\"context\\ud800Token\"" })
                foreach (string value in new[] { "\"opaque\"", "\"\\ud800\"", "[\"\\udc00\"]" })
                    yield return new("property-name-" + name + value, tool, "{" + name + ":" + value + "}");
            yield return new("duplicate-valid-name", tool, "{\"contextToken\":\"one\",\"contextToken\":\"two\"}");
            yield return new("duplicate-escaped-name", tool, "{\"contextToken\":\"opaque\",\"context\\u0054oken\":\"\\ud800\"}");
            yield return new("duplicate-extra", tool, "{\"extra\":\"one\",\"extra\":\"\\udc00\"}");
            foreach (string token in new[] { "", " \t\r\n", new string('x', 70000) })
                yield return new("empty-blank-oversize-" + token.Length, tool, "{\"contextToken\":" + JsonSerializer.Serialize(token) + "}");
            foreach (string token in new[] { "opaque", "墙 / سقف / Café / 🧱", "line\n\t\0quote\"slash\\", "/Date(0)/", new string('x', 60000) })
                yield return new("valid-string-control-" + token.Length, tool, "{\"contextToken\":" + JsonSerializer.Serialize(token) + "}", tool == check, tool == check ? token : null);
            yield return new("escaped-key-and-pair", tool, "{\"context\\u0054oken\":\"\\ud83d\\ude00\"}", tool == check, tool == check ? "😀" : null);
            yield return new("unpaired-high-followed-by-high", tool, "{\"contextToken\":\"\\ud800\\ud800\"}");
            yield return new("low-then-high", tool, "{\"contextToken\":\"\\udc00\\ud800\"}");
        }
        yield return new("valid-context-after-all-errors", get, "{}", true);
        yield return new("valid-token-after-all-errors", check, "{\"contextToken\":\"opaque-after\"}", true, "opaque-after");
    }

    static async Task ArgumentBoundaryMatrix(string revision, bool tracing)
    {
        var descriptor = BridgeIdentity.CreateCurrent();
        var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(), new SystemBridgeClock());
        int calls = 0;
        string? observedToken = null;
        endpoint.Start(context => {
            observedToken = context.Request.ContextToken; Interlocked.Increment(ref calls);
            var result = SyntheticResponse(context); result.BridgeSessionId = descriptor.BridgeSessionId;
            return new DispatchTicket(Task.FromResult(result), Task.CompletedTask);
        });
        try
        {
            await using var child = new RawToolChild(Environment.ProcessId, tracing, revision, "argument-contract");
            await child.Connect();
            var failures = new List<string>(); var results = new List<object>(); var ids = new HashSet<string>();
            int id = 2, rejected = 0, accepted = 0;
            foreach (var test in ArgumentCases())
            {
                int before = Volatile.Read(ref calls);
                string raw = test.Raw is null
                    ? "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"method\":\"tools/call\",\"params\":{\"name\":\"" + test.Tool + "\"" + Meta(revision) + "}}"
                    : CallWire(id, test.Tool, test.Raw, revision);
                await child.Send(raw);
                var reply = await child.Receive(id++);
                int delta = Volatile.Read(ref calls) - before;
                string? error = null;
                try
                {
                    Check(!reply.TryGetProperty("error", out _), "JSON-RPC error escaped application boundary");
                    var result = reply.GetProperty("result"); var data = result.GetProperty("structuredContent");
                    string correlation = data.GetProperty("requestId").GetString()!;
                    Check(Guid.TryParseExact(correlation, "N", out _) && ids.Add(correlation), "Missing/duplicate correlation");
                    using var text = JsonDocument.Parse(result.GetProperty("content")[0].GetProperty("text").GetString()!);
                    Check(JsonElement.DeepEquals(data, text.RootElement), "Text/structured mismatch");
                    if (test.Accepted)
                    {
                        Check(!result.GetProperty("isError").GetBoolean() && delta == 1, "Valid input did not execute exactly once");
                        Check(observedToken == test.Token, "Valid token changed before bridge execution");
                    }
                    else
                    {
                        Check(result.GetProperty("isError").GetBoolean() && data.GetProperty("code").GetString() == "InvalidRequest", "Wrong argument rejection");
                        Check(!data.GetProperty("retryable").GetBoolean() && !data.TryGetProperty("report", out _), "Unsafe failure content");
                        Check(delta == 0, "Rejected input reached bridge admission");
                    }
                }
                catch (Exception failure) { error = failure.Message; failures.Add(test.Name + ": " + error); }
                if (test.Accepted) accepted++; else rejected++;
                results.Add(new { id = id - 1, test.Name, test.Tool, test.Accepted, bridgeCalls = delta, pass = error is null, error });
            }
            // MCP envelope/method failure remains SDK-owned, not a tool InvalidRequest.
            int beforeProtocol = Volatile.Read(ref calls);
            await child.Send("{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"method\":\"lab/nonexistent\",\"params\":{" + (Meta(revision).Length == 0 ? "" : Meta(revision)[1..]) + "}}");
            var protocolError = await child.Receive(id++);
            Check(protocolError.GetProperty("error").GetProperty("code").GetInt32() == -32601 && Volatile.Read(ref calls) == beforeProtocol,
                "Application gate took over SDK method errors");
            child.Record("matrix.json", JsonSerializer.Serialize(new { revision, tracing, accepted, rejected, results }));
            child.Process.StandardInput.Close();
            await child.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Check(child.Process.ExitCode == 0, "Matrix process did not exit naturally");
            Console.WriteLine($"ARGUMENT_MATRIX revision={revision} trace={tracing} accepted={accepted} rejected={rejected} failures={failures.Count}");
            Check(failures.Count == 0, string.Join("; ", failures.Take(12)));
        }
        finally { await endpoint.StopAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
    }

    static async Task ArgumentHandoff(bool expire)
    {
        var clock = new ArgumentClock(); int sends = 0;
        var tools = new ReadinessTools(new DelegateBridge(context => {
            sends++;
            Check(context.Request.ContextToken == "unchanged" && context.Request.DeadlineUtc == "2026-10-21T00:00:30.0000000Z", "Prepared token/deadline changed");
            return Task.FromResult(SyntheticResponse(context));
        }), clock, CancellationToken.None);
        await using var transport = new StreamServerTransport(new MemoryStream(), new MemoryStream(), "argument-handoff");
        await using var server = McpServer.Create(transport, new McpServerOptions { ServerInfo = new Implementation { Name = "test", Version = "1" } });
        var raw = new JsonRpcRequest { Id = new RequestId(1), Method = "tools/call",
            Params = JsonNode.Parse("{\"name\":\"revit_check_required_type_information\",\"arguments\":{\"contextToken\":\"unchanged\"}}") };
        var original = raw.Params!["arguments"];
        CallToolResult? result = null;
        var filtered = tools.ValidateIncomingArguments(async (message, cancellation) => {
            Check(ReferenceEquals(original, raw.Params["arguments"]), "Accepted input subtree replaced");
            if (expire) clock.Seconds = 31;
            var typed = raw.Params.Deserialize<CallToolRequestParams>()!;
            result = await tools.CallAsync(new RequestContext<CallToolRequestParams>(server, raw, typed), cancellation);
        });
        await filtered(new MessageContext(server, raw), CancellationToken.None);
        if (expire) { Error(result!, "DeadlineExceeded", false); Check(sends == 0, "Expired prepared request entered bridge"); }
        else Check(result!.IsError == false && sends == 1, "Accepted preparation failed");
        Check(clock.Admissions == 1, "Typed dispatch restarted admission/budget");
    }

    static async Task ArgumentContinuationFailure()
    {
        var tools = new ReadinessTools(new DelegateBridge(_ => throw new Exception("Unexpected IPC")), new SystemBridgeClock(), CancellationToken.None);
        await using var transport = new StreamServerTransport(new MemoryStream(), new MemoryStream(), "argument-failure");
        await using var server = McpServer.Create(transport, new McpServerOptions { ServerInfo = new Implementation { Name = "test", Version = "1" } });
        var raw = new JsonRpcRequest { Id = new RequestId(1), Method = "tools/call",
            Params = JsonNode.Parse("{\"name\":\"revit_get_readiness_context\",\"arguments\":{}}") };
        var sentinel = new InvalidOperationException("Downstream, not argument decoding");
        var filtered = tools.ValidateIncomingArguments((_, _) => throw sentinel);
        Exception? observed = null;
        try { await filtered(new MessageContext(server, raw), CancellationToken.None); } catch (Exception error) { observed = error; }
        Check(ReferenceEquals(sentinel, observed), "Downstream failure swallowed or classified as an argument error");
    }

    sealed class ArgumentClock : IBridgeClock
    {
        internal int Seconds, Admissions;
        public DateTimeOffset UtcNow => new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero).AddSeconds(Seconds);
        public long Timestamp { get { Admissions++; return Seconds; } }
        public TimeSpan ElapsedSince(long timestamp) => TimeSpan.FromSeconds(Seconds - timestamp);
    }
}
