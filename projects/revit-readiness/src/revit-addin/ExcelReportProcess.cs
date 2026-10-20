using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BimAgentLab.RevitReadiness
{
    public static class ExcelReportProcess
    {
        public static void Save(string reportPath, string workbookPath, string exporterPath = null)
        {
            // The exporter owns its .NET configuration. No spreadsheet assemblies load
            // into Revit, and no global redirects or Revit.exe.config edits are needed.
            string executable = exporterPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Excel", "BimAgentLab.Readiness.Excel.exe");
            if (!File.Exists(executable) || !File.Exists(executable + ".config"))
                throw new FileNotFoundException("Excel exporter or dependency configuration is missing. Reinstall the add-in.", executable);
            var start = new ProcessStartInfo {
                FileName = executable,
                Arguments = "\"" + Path.GetFullPath(reportPath) + "\" \"" + Path.GetFullPath(workbookPath) + "\"",
                WorkingDirectory = Path.GetDirectoryName(executable),
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true
            };
            using (var process = Process.Start(start))
            {
                var error = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(60000))
                {
                    process.Kill();
                    process.WaitForExit();
                    throw new TimeoutException("Excel export exceeded 60 seconds. JSON/text remain available; the previous workbook may be stale.");
                }
                string details = error.GetAwaiter().GetResult();
                if (process.ExitCode != 0) throw new IOException("Excel exporter failed: " + details);
                if (!File.Exists(workbookPath)) throw new IOException("Excel exporter returned without creating a workbook.");
            }
        }
    }
}
