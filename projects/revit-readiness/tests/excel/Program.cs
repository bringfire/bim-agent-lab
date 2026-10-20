using System;
using System.IO;
using System.Linq;
using BimAgentLab.Readiness.Core;
using BimAgentLab.Readiness.Excel;
using ClosedXML.Excel;

internal static class Program
{
    private static int passed, failed;
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    private static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    private static InformationReport Read(string folder, string name) => JsonFile.Read<InformationReport>(Path.Combine(folder, name + ".readiness.json"));
    private static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("Usage: Excel.Tests <report evidence folder> <output folder>"); return 2; }
        Directory.CreateDirectory(args[1]);
        string[] names = { "BAL-info-complete", "BAL-info-wall-defects", "BAL-info-ceiling-negative" };
        int[] findings = { 0, 2, 3 }, affected = { 0, 6, 8 };
        for (int i = 0; i < names.Length; i++)
        {
            int index = i;
            Test(names[i], () => {
                string path = Path.Combine(args[1], names[index] + ".readiness.xlsx");
                var report = Read(args[0], names[index]);
                InformationWorkbook.Save(report, path);
                using (var workbook = new XLWorkbook(path))
                {
                    Require(workbook.Worksheets.Select(w => w.Name).SequenceEqual(new[] { "Summary", "Findings", "Affected elements", "Rules and coverage" }), "Four ordered worksheets required");
                    var summary = workbook.Worksheet("Summary");
                    Require(summary.Cell("B8").GetValue<int>() == findings[index], "Wrong finding count");
                    Require(summary.Cell("B9").GetValue<int>() == affected[index], "Wrong affected count");
                    Require(summary.Cell("B13").GetString() == "Complete for checklist scope", "Wrong coverage");
                    Require(workbook.Worksheet("Rules and coverage").Tables.Single().DataRange.RowCount() == 18, "All required and optional fields must be retained");
                    Require(workbook.Worksheets.All(s => !s.CellsUsed().Any(c => c.HasFormula)), "Snapshot reports must contain literal values");
                }
            });
        }
        Test("Incomplete coverage stays visible even with zero findings", () => {
            var report = Read(args[0], names[0]);
            report.ScopeIssues.Add(new ScopeIssue { CaseId = "MISSING-CASE", UniqueId = "missing-uid", Status = "Unexamined", Reason = "Element not found" });
            report.RequestedInstances++;
            report.CoverageComplete = false;
            var field = report.Evaluation.Types[0].Fields[0];
            field.Status = "Unexamined"; field.Reason = "NoMatchingRequirement"; field.Required = null;
            report.Evaluation.Types[0].Status = "Unexamined";
            report.Evaluation.UnexaminedElements.AddRange(report.Evaluation.Types[0].Elements);
            report.Evaluation.Complete = false;
            var path = Path.Combine(args[1], "incomplete.xlsx");
            InformationWorkbook.Save(report, path);
            using (var workbook = new XLWorkbook(path)) {
                Require(workbook.Worksheet("Summary").Cell("B13").GetString().StartsWith("INCOMPLETE"), "Incomplete must be explicit");
                Require(workbook.Worksheet("Rules and coverage").CellsUsed().Any(c => c.GetString() == "MISSING-CASE"), "Scope issue lost");
                Require(workbook.Worksheet("Rules and coverage").CellsUsed().Any(c => c.GetString() == "Unknown"), "Unknown requirement lost");
                foreach (var element in report.Evaluation.UnexaminedElements) {
                    Require(workbook.Worksheet("Rules and coverage").CellsUsed().Any(c => c.GetString() == element.UniqueId), "Unexamined element identity lost");
                    Require(workbook.Worksheet("Rules and coverage").CellsUsed().Any(c => c.GetString() == element.CaseId), "Unexamined case identity lost");
                }
            }
        });
        Test("Text resembling formulas and long identifiers remains literal", () => {
            var report = Read(args[0], names[1]);
            var row = report.Evaluation.Types.First(t => t.Fields.Any(f => f.Status == "Missing"));
            row.TypeName = "=1+1"; row.Elements[0].ElementId = "0001234567890123456789";
            row.Fields[0].ObservedValue = "+SUM(1,2)";
            var path = Path.Combine(args[1], "literal-text.xlsx");
            InformationWorkbook.Save(report, path);
            using (var workbook = new XLWorkbook(path)) {
                Require(workbook.Worksheets.All(s => !s.CellsUsed().Any(c => c.HasFormula)), "Unexpected executable formula");
                Require(workbook.Worksheet("Affected elements").CellsUsed().Any(c => c.DataType == XLDataType.Text && c.GetString() == "0001234567890123456789"), "Identifier altered");
                Require(workbook.Worksheet("Findings").CellsUsed().Any(c => c.GetString() == "=1+1"), "Type text altered");
            }
        });
        Test("Locked destination preserves existing workbook and cleans temporary file", () => {
            string path = Path.Combine(args[1], "locked.xlsx");
            var report = Read(args[0], names[0]);
            InformationWorkbook.Save(report, path);
            byte[] before = File.ReadAllBytes(path);
            using (var handle = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)) {
                bool rejected = false;
                try { InformationWorkbook.Save(report, path); } catch (IOException) { rejected = true; }
                Require(rejected, "Locked destination must fail explicitly");
            }
            Require(File.ReadAllBytes(path).SequenceEqual(before), "Previous workbook was damaged");
            Require(!Directory.GetFiles(args[1], "*.tmp.xlsx").Any(), "Temporary workbook left behind");
        });
        Test("Multiple missing fields do not duplicate affected elements", () => {
            var report = Read(args[0], names[1]);
            var row = report.Evaluation.Types.Single(t => t.TypeName == "BAL INFO Wall B");
            var field = row.Fields.Single(f => f.Field == "AssemblyReference");
            field.Status = "Missing"; field.Reason = "BlankValue"; field.ObservedValue = "";
            report.Evaluation.FindingCount++;
            var path = Path.Combine(args[1], "multiple-fields.xlsx");
            InformationWorkbook.Save(report, path);
            using (var workbook = new XLWorkbook(path)) {
                Require(workbook.Worksheet("Affected elements").Tables.Single().DataRange.RowCount() == 6, "Affected elements duplicated");
                Require(workbook.Worksheet("Findings").Tables.Single().DataRange.RowCount() == 3, "Type-field findings lost");
            }
        });
        Console.WriteLine($"{passed} passed; {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
