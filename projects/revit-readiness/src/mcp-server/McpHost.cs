using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;

namespace BimAgentLab.RevitReadiness.Mcp;

public enum McpProtocolPolicy { Compatible, Strict20260728 }

public static class McpHost
{
    public static Task RunStdioAsync(McpServerHandlers handlers, string? evidenceDirectory, CancellationToken hostStopping,
        McpProtocolPolicy protocolPolicy = McpProtocolPolicy.Compatible, McpMessageFilter? incomingFilter = null)
    {
        var input = Console.OpenStandardInput();
        Stream output;
        try { output = Console.OpenStandardOutput(); }
        catch { input.Dispose(); throw; }
        return RunOwnedStreamsAsync(handlers, input, output, evidenceDirectory, hostStopping, protocolPolicy, incomingFilter);
    }

    internal static async Task RunOwnedStreamsAsync(McpServerHandlers handlers, Stream input, Stream output,
        string? evidenceDirectory, CancellationToken hostStopping, McpProtocolPolicy protocolPolicy = McpProtocolPolicy.Compatible,
        McpMessageFilter? incomingFilter = null)
    {
        using var ownedInput = new CancellableInputStream(input);
        using var ownedOutput = output;
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(hostStopping);
        FileStream? incoming = null, outgoing = null;
        Exception? evidenceError = null;
        void FailEvidence(Exception error)
        {
            Interlocked.CompareExchange(ref evidenceError, error, null);
            stopping.Cancel();
        }
        try
        {
            // SDK transport disposes the streams it receives. It borrows these
            // wrappers; this scope alone releases the raw streams after quiescence.
            Stream serverInput = new WrappedStream(ownedInput, true), serverOutput = new WrappedStream(ownedOutput, true);
            if (evidenceDirectory is not null)
            {
                Directory.CreateDirectory(evidenceDirectory);
                incoming = new FileStream(Path.Combine(evidenceDirectory, "client-to-server.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                outgoing = new FileStream(Path.Combine(evidenceDirectory, "server-to-client.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                serverInput = new TraceStream(ownedInput, incoming, true, FailEvidence);
                serverOutput = new TraceStream(ownedOutput, outgoing, true, FailEvidence);
            }
            var options = new McpServerOptions
            {
                // Let the official SDK accept both generations in production.
                // The separate probe also exercises an explicit strict regression policy.
                ProtocolVersion = protocolPolicy switch
                {
                    McpProtocolPolicy.Compatible => null,
                    McpProtocolPolicy.Strict20260728 => "2026-07-28",
                    _ => throw new ArgumentOutOfRangeException(nameof(protocolPolicy))
                },
                ServerInfo = new Implementation { Name = "bim-agent-lab", Version = "0.1.0" },
                Capabilities = new ServerCapabilities { Tools = new ToolsCapability() },
                Handlers = handlers
            };
            if (incomingFilter is not null) options.Filters.Message.IncomingFilters.Add(incomingFilter);
            await using var transport = new StreamServerTransport(serverInput, serverOutput, "bim-agent-lab");
            await using var server = McpServer.Create(transport, options);
            var running = server.RunAsync(stopping.Token);
            var disconnected = transport.MessageReader.Completion;
            // SDK 2.2.0 drains handlers after EOF/read failure without canceling them.
            // Signal their existing tokens BEFORE waiting for that drain. If the run
            // ends first (e.g. explicit stop), proceed to disposal without waiting for EOF.
            await Task.WhenAny(running, disconnected).ConfigureAwait(false);
            Exception? inputError = null;
            if (disconnected.IsCompleted)
            {
                try { await disconnected.ConfigureAwait(false); }
                catch (Exception error) { inputError = error; }
                stopping.Cancel();
            }
            try { await running.ConfigureAwait(false); }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
            // Cancellation can win the SDK read-loop race; retain the transport's cause.
            if (inputError is not null) throw new IOException("MCP input transport failed.", inputError);
        }
        finally
        {
            stopping.Cancel();
            // Server/transport disposal has finished before trace sinks close.
            try { incoming?.Dispose(); }
            finally { outgoing?.Dispose(); }
        }
        if (evidenceError is not null) throw new IOException("Wire evidence capture failed; this session is not valid evidence.", evidenceError);
    }
}
