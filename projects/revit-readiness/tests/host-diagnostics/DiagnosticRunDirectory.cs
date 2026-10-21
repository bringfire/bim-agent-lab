using System;
using System.Globalization;
using System.IO;

namespace BimAgentLab.RevitReadiness
{
    internal static class DiagnosticRunDirectory
    {
        // Shared by the agent and unpackaged Revit. AppData can be a merged,
        // per-package view in the agent's process even when the path looks normal.
        internal static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bim-agent-lab", "RevitBridge", "diagnostics");
        internal static string CreateProcessDirectory(string root, int processId)
        {
            string run = File.ReadAllText(Path.Combine(root, "run.txt")).Trim();
            Guid id;
            if (!Guid.TryParseExact(run, "N", out id)) throw new InvalidDataException("A prepared diagnostic run is required.");
            string prepared = Path.Combine(root, run);
            if (!Directory.Exists(prepared)) throw new DirectoryNotFoundException(prepared);
            string directory = Path.Combine(prepared, processId.ToString(CultureInfo.InvariantCulture));
            if (Directory.Exists(directory)) throw new InvalidOperationException("Diagnostic capture already exists.");
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
