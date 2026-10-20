using System;
using System.Reflection;
using BimAgentLab.RevitReadiness;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 3 && args[0] == "--core-order")
            {
                Assembly.LoadFrom(args[1]);
                var core = Assembly.LoadFrom(args[2]);
                if (core.GetType("BimAgentLab.Readiness.Core.InformationReport", false) == null)
                    throw new InvalidOperationException("Fixture-first core loading hid the report contract: " + core.Location);
                Console.WriteLine("PASS fixture-first core exposes the readiness report contract: " + core.Location);
                return 0;
            }
            if (args.Length != 3) throw new ArgumentException("Usage: HostLoad <Excel.exe> <report.json> <output.xlsx>");
            ExcelReportProcess.Save(args[1], args[2], args[0]);
            Console.WriteLine("PASS process-isolated export from a host with no spreadsheet references or binding redirects");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
