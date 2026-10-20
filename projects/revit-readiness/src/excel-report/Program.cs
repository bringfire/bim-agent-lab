using System;
using BimAgentLab.Readiness.Core;

namespace BimAgentLab.Readiness.Excel
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length != 2) throw new ArgumentException("Usage: BimAgentLab.Readiness.Excel <report.json> <output.xlsx>");
                InformationWorkbook.Save(JsonFile.Read<InformationReport>(args[0]), args[1]);
                return 0;
            }
            catch (Exception exception) { Console.Error.WriteLine(exception.ToString()); return 1; }
        }
    }
}
