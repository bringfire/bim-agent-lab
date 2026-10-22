using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Verification-only process ownership; MCP bytes and cancellation are produced by SDK 2.2.0.
internal static class InterruptionScenario
{
    public static async Task<int> Run(string server, List<string> args, string evidence, string protocol,
        int? revitPid, string? token, string? mode, string? interrupt, string? finish)
    {
        if (mode is not ("cancel" or "eof") || interrupt is null || finish is null || !revitPid.HasValue)
            throw new ArgumentException("Interrupt scenario needs mode, interrupt-file, finish-file and revit-pid.");
        if (interrupt == finish || File.Exists(interrupt) || File.Exists(finish) || Directory.Exists(evidence))
            throw new ArgumentException("Distinct absent controls and a new capture directory are required.");
        if (args.Contains("--wire-evidence")) throw new ArgumentException("Driver owns its wire directory.");
        Directory.CreateDirectory(evidence);
        void Save(string name, object value)
        {
            string path = Path.Combine(evidence, name);
            File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(path + ".tmp", path); // publish complete observation, never overwrite a prior attempt
        }
        Save("partial.json", new { startedUtc = DateTime.UtcNow, mode, protocol, revitPid, tokenProvided = token is not null });
        var start = new ProcessStartInfo(server) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        foreach (string arg in new[] { "--revit-pid", revitPid.Value.ToString(), "--wire-evidence", Path.Combine(evidence, "wire") }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Server did not start.");
        Save("server-process.json", new { pid = process.Id, startedUtc = process.StartTime.ToUniversalTime() });
        var stderr = process.StandardError.ReadToEndAsync();
        using var watchdog = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        McpClient? client = null;
        bool pass = false, forcedTermination = false;
        try
        {
            // Official SDK transport over explicitly owned streams; no alternate protocol implementation.
            client = await McpClient.CreateAsync(new StreamClientTransport(process.StandardInput.BaseStream,
                process.StandardOutput.BaseStream), new McpClientOptions { ProtocolVersion = protocol }, cancellationToken: watchdog.Token);
            var tools = await client.ListToolsAsync(cancellationToken: watchdog.Token);
            var names = tools.Select(t => t.Name).Order(StringComparer.Ordinal).ToArray();
            if (client.NegotiatedProtocolVersion != protocol || !names.SequenceEqual(new[] {
                "revit_check_required_type_information", "revit_get_readiness_context" })) throw new InvalidDataException("Wrong protocol/catalog");
            Save("catalog.json", new { protocol = client.NegotiatedProtocolVersion, names });
            if (token is null)
            {
                var context = await client.CallToolAsync("revit_get_readiness_context", new Dictionary<string, object?>(), cancellationToken: watchdog.Token);
                Save("context.json", context);
                if (context.IsError != false) throw new InvalidDataException("Context acquisition failed.");
                token = context.StructuredContent?.GetProperty("contextToken").GetString() ?? throw new InvalidDataException("Missing token");
            }
            // Own this request ID so the explicit cancellation stimulus cannot
            // accidentally target context acquisition or another SDK request.
            var requestId = new RequestId("interrupt-" + Guid.NewGuid().ToString("N"));
            var pending = client.SendRequestAsync(new JsonRpcRequest {
                Id = requestId, Method = RequestMethods.ToolsCall,
                Params = JsonSerializer.SerializeToNode(new CallToolRequestParams {
                    Name = "revit_check_required_type_information",
                    Arguments = new Dictionary<string, JsonElement> { ["contextToken"] = JsonSerializer.SerializeToElement(token) }
                }, McpJsonUtilities.DefaultOptions)
            }, watchdog.Token);
            Save("check-started.json", new { startedUtc = DateTime.UtcNow, rpcRequestId = requestId.ToString() });
            while (!File.Exists(interrupt))
            {
                if (pending.IsCompleted) throw new InvalidOperationException("Call completed before interruption; invalid capture.");
                await Task.Delay(20, watchdog.Token);
            }
            if (pending.IsCompleted) throw new InvalidOperationException("Call already complete at interruption.");
            DateTime interruptedUtc = DateTime.UtcNow;
            var timing = Stopwatch.StartNew();
            if (mode == "cancel") await client.SendMessageAsync(new JsonRpcNotification {
                Method = NotificationMethods.CancelledNotification,
                Params = JsonSerializer.SerializeToNode(new CancelledNotificationParams { RequestId = requestId }, McpJsonUtilities.DefaultOptions)
            }, watchdog.Token);
            else process.StandardInput.Close(); // exact EOF; don't pre-cancel or dispose the SDK
            string? exceptionType = null;
            try
            {
                var result = await pending.WaitAsync(TimeSpan.FromSeconds(5), watchdog.Token);
                Save("unexpected-result.json", result);
                throw new InvalidDataException("Interrupted SDK call returned a result.");
            }
            catch (Exception error) when (error is not TimeoutException && error is not InvalidDataException && !watchdog.IsCancellationRequested)
            {
                if (mode == "cancel" && error is not OperationCanceledException) throw;
                exceptionType = error.GetType().FullName;
            }
            if (mode == "eof") await process.WaitForExitAsync(watchdog.Token).WaitAsync(TimeSpan.FromSeconds(5));
            if ((mode == "eof" && (!process.HasExited || process.ExitCode != 0)) || (mode == "cancel" && process.HasExited))
                throw new InvalidDataException("Wrong server lifetime after interruption.");
            Save("interrupted.json", new { mode, interruptedUtc, observedUtc = DateTime.UtcNow,
                elapsedMilliseconds = timing.ElapsedMilliseconds, exceptionType, serverExited = process.HasExited,
                exitCode = process.HasExited ? (int?)process.ExitCode : null });
            while (!File.Exists(finish)) await Task.Delay(20, watchdog.Token);
            DateTime releasedUtc = DateTime.UtcNow;
            var observed = Stopwatch.StartNew();
            await Task.Delay(3000, watchdog.Token);
            int? catalogCount = null;
            if (mode == "cancel")
            {
                var after = await client.ListToolsAsync(cancellationToken: watchdog.Token);
                if (!after.Select(t => t.Name).Order(StringComparer.Ordinal).SequenceEqual(names)) throw new InvalidDataException("Catalog changed");
                catalogCount = after.Count;
            }
            Save("post-release-observation.json", new { releasedUtc, observedUntilUtc = DateTime.UtcNow,
                elapsedMilliseconds = observed.ElapsedMilliseconds, catalogCount, serverExited = process.HasExited });
            pass = true;
        }
        finally
        {
            // On failed capture, close input and recover only this owned test process.
            // A forced recovery is evidence of failure, never natural-shutdown acceptance.
            process.StandardInput.Close();
            try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException) { forcedTermination = true; pass = false; process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            if (client is not null) await client.DisposeAsync();
            File.WriteAllText(Path.Combine(evidence, "server-stderr.txt"), await stderr);
            Save("server-exit.json", new { process.ExitCode, forcedTermination, observedUtc = DateTime.UtcNow });
        }
        pass &= !forcedTermination && process.ExitCode == 0;
        Save("complete.json", new { pass, scope = "SDK interruption driver; not desktop A8 or host-read proof" });
        Console.WriteLine($"{(pass ? "PASS" : "FAIL")} SDK interruption capture: {mode}");
        return pass ? 0 : 1;
    }
}
