using System.Globalization;
using BimAgentLab.RevitBridge.Runtime;
using ModelContextProtocol.Server;

namespace BimAgentLab.RevitReadiness.Mcp;
internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        if (!OperatingSystem.IsWindows()) { Console.Error.WriteLine("The Revit bridge requires Windows."); return 1; }
        try
        {
            int? pid = null; string? evidence = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length || !seen.Add(args[i])) throw new ArgumentException("Options require one value and cannot repeat.");
                switch (args[i])
                {
                    case "--revit-pid" when int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out int selected) && selected > 0: pid = selected; break;
                    case "--wire-evidence" when !string.IsNullOrWhiteSpace(args[i + 1]): evidence = Path.GetFullPath(args[i + 1]); break;
                    default: throw new ArgumentException("Usage: readiness [--revit-pid <positive-int>] [--wire-evidence <new-directory>]");
                }
            }
            using var stop = new CancellationTokenSource();
            ConsoleCancelEventHandler onCancel = (_, args) => { args.Cancel = true; stop.Cancel(); };
            Console.CancelKeyPress += onCancel;
            try
            {
                var clock = new SystemBridgeClock();
                var provider = new BridgeClientProvider(new SessionCatalog(), pid, clock);
                var tools = new ReadinessTools(provider, clock, stop.Token);
                await McpHost.RunStdioAsync(new McpServerHandlers { ListToolsHandler = tools.ListAsync, CallToolHandler = tools.CallAsync }, evidence, stop.Token);
            }
            finally { Console.CancelKeyPress -= onCancel; }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine($"Readiness server failed ({error.GetType().Name}): {error.Message}"); return 1; }
    }
}
