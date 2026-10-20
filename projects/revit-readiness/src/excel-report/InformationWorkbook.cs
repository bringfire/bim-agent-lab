using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BimAgentLab.Readiness.Core;
using ClosedXML.Excel;

namespace BimAgentLab.Readiness.Excel
{
    public static class InformationWorkbook
    {
        public static void Save(InformationReport report, string path)
        {
            if (report?.Evaluation == null) throw new ArgumentException("An evaluated report is required.");
            if (!string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Excel output must use the .xlsx extension.");
            string fullPath = Path.GetFullPath(path);
            string temporary = Path.Combine(Path.GetDirectoryName(fullPath), Guid.NewGuid().ToString("N") + ".tmp.xlsx");
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    workbook.Properties.Title = "BIM Agent Lab - Required type information";
                    workbook.Properties.Author = "BIM Agent Lab";
                    Summary(workbook, report);
                    Findings(workbook, report);
                    Affected(workbook, report);
                    Rules(workbook, report);
                    workbook.SaveAs(temporary);
                }
                // Publish only a complete workbook; a locked destination keeps the previous bytes intact.
                if (File.Exists(fullPath)) File.Replace(temporary, fullPath, null);
                else File.Move(temporary, fullPath);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        private static string Coverage(InformationReport r) => r.CoverageComplete
            ? "Complete for checklist scope" : "INCOMPLETE - inspect Rules and coverage";

        private static IXLWorksheet Sheet(XLWorkbook workbook, string name, int columns, string subtitle)
        {
            var sheet = workbook.AddWorksheet(name);
            sheet.Style.Font.FontName = "Calibri";
            sheet.Style.Font.FontSize = 11;
            sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            sheet.ShowGridLines = false;
            sheet.Range(1, 1, 1, columns).Merge().Value = "BIM AGENT LAB | " + name;
            sheet.Range(1, 1, 1, columns).Style.Fill.BackgroundColor = XLColor.FromHtml("#17324D");
            sheet.Range(1, 1, 1, columns).Style.Font.FontColor = XLColor.White;
            sheet.Range(1, 1, 1, columns).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, columns).Style.Font.FontSize = 18;
            sheet.Row(1).Height = 36;
            sheet.Range(2, 1, 2, columns).Merge().Value = subtitle;
            sheet.Range(2, 1, 2, columns).Style.Alignment.WrapText = true;
            sheet.Row(2).Height = 34;
            sheet.Columns(1, columns).Width = 24;
            sheet.SheetView.FreezeRows(4);
            sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            return sheet;
        }

        private static void Put(IXLCell cell, object value)
        {
            if (value is int number) { cell.Value = number; cell.Style.NumberFormat.Format = "#,##0"; }
            else if (value is bool flag) cell.Value = flag;
            // Assign strings as values, never formulas; preserve codes/IDs as text, including leading zeros.
            else cell.Value = value?.ToString() ?? "";
        }

        private static void Header(IXLWorksheet sheet, int row, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++) Put(sheet.Cell(row, i + 1), headers[i]);
            var range = sheet.Range(row, 1, row, headers.Length);
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.White;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#215F73");
            range.Style.Alignment.WrapText = true;
            sheet.Row(row).Height = 32;
        }

        private static void Table(IXLWorksheet sheet, int header, string name, string[] headers, IList<object[]> rows)
        {
            Header(sheet, header, headers);
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Length; j++) Put(sheet.Cell(header + 1 + i, j + 1), rows[i][j]);
                sheet.Row(header + 1 + i).Height = 60;
            }
            if (rows.Count == 0) { sheet.Cell(header + 2, 1).Value = "No rows in this scope."; return; }
            var table = sheet.Range(header, 1, header + rows.Count, headers.Length).CreateTable(name);
            table.Theme = XLTableTheme.TableStyleMedium2;
            table.ShowAutoFilter = true;
            table.DataRange.Style.Alignment.WrapText = true;
        }

        private static void Summary(XLWorkbook workbook, InformationReport r)
        {
            var sheet = Sheet(workbook, "Summary", 2, "Required type information | Snapshot of a read-only check. Text completeness only; no certification or measured quantities.");
            sheet.Column(1).Width = 34; sheet.Column(2).Width = 110;
            Header(sheet, 4, "Metric / context", "Result");
            var unexamined = r.Evaluation.UnexaminedElements.Select(e => e.UniqueId)
                .Concat(r.ScopeIssues.Select(e => e.UniqueId)).Distinct(StringComparer.Ordinal).Count();
            var values = new object[][] {
                new object[] { "Generated (UTC)", r.GeneratedUtc },
                new object[] { "Model", Path.GetFileName(r.DocumentPath) },
                new object[] { "Checklist", r.ChecklistId },
                new object[] { "Missing required type values", r.Evaluation.FindingCount },
                new object[] { "Distinct affected elements", r.Evaluation.AffectedElements.Count },
                new object[] { "Used types examined", r.Evaluation.Types.Count },
                new object[] { "Requested instances", r.RequestedInstances },
                new object[] { "Examined instances", r.ExaminedInstances },
                new object[] { "Coverage", Coverage(r) },
                new object[] { "Distinct unexamined elements", unexamined },
                new object[] { "Other host wall/ceiling instances", r.OtherHostWallCeilingInstances },
                new object[] { "Link instances excluded", r.LinkInstancesExcluded },
                new object[] { "Level", r.Level },
                new object[] { "Phase (New)", r.Phase },
                new object[] { "Units", r.Units },
                new object[] { "Model modified before read", r.ModifiedBefore },
                new object[] { "Model modified after read", r.ModifiedAfter },
                new object[] { "Model path", r.DocumentPath },
                new object[] { "Project unique ID", r.ProjectUniqueId },
                new object[] { "Checklist path", r.ChecklistPath },
                new object[] { "Revit build", r.RevitBuild }
            };
            for (int i = 0; i < values.Length; i++)
            {
                Put(sheet.Cell(i + 5, 1), values[i][0]); Put(sheet.Cell(i + 5, 2), values[i][1]);
                sheet.Row(i + 5).Height = i == 17 || i == 19 ? 44 : 26;
                if (i % 2 == 0) sheet.Range(i + 5, 1, i + 5, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F5F8");
            }
            sheet.Range("A5:B25").Style.Alignment.WrapText = true;
            sheet.Range("B8:B9").Style.Font.Bold = true;
            sheet.Cell("B13").Style.Fill.BackgroundColor = XLColor.FromHtml(r.CoverageComplete ? "#DDEDE7" : "#FFE1AF");
            sheet.Cell("B13").Style.Font.Bold = true;
            sheet.Range("A27:B29").Merge().Value = r.Scope + "\nThis is a saved snapshot. Editing cells does not rerun Revit checks; rerun the command to refresh.";
            sheet.Range("A27:B29").Style.Alignment.WrapText = true;
            sheet.Rows(27, 29).Height = 24;
        }

        private static void Findings(XLWorkbook workbook, InformationReport r)
        {
            var sheet = Sheet(workbook, "Findings", 11, "One row per missing type field. Affected counts overlap when one type has multiple gaps; use Summary for the distinct count. " + Coverage(r));
            var rows = r.Evaluation.Types.SelectMany(t => t.Fields.Where(f => f.Status == "Missing").Select(f => new object[] {
                t.Category, t.TypeName, t.TypeUniqueId, f.Field, f.Status, f.Reason, f.ObservedValue, f.RuleId,
                f.RequirementSource, f.ParameterId, t.Elements.Count })).ToList();
            Table(sheet, 4, "MissingInformation", new[] { "Category", "Type", "Type unique ID", "Field", "Status", "Reason", "Observed value", "Rule ID", "Requirement source", "Parameter ID", "Affected instances" }, rows);
            sheet.Column(2).Width = 30; sheet.Column(3).Width = 44; sheet.Column(9).Width = 44; sheet.Column(10).Width = 44;
            sheet.SheetView.FreezeColumns(2);
            if (rows.Count > 0) sheet.Range(5, 5, rows.Count + 4, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FBE3DF");
        }

        private static void Affected(XLWorkbook workbook, InformationReport r)
        {
            var sheet = Sheet(workbook, "Affected elements", 9, "One row per distinct instance with a missing required type value. Unexamined cases are listed on Rules and coverage. " + Coverage(r));
            var affected = new HashSet<string>(r.Evaluation.AffectedElements.Select(e => e.UniqueId), StringComparer.Ordinal);
            var rows = r.Evaluation.Types.SelectMany(t => t.Elements.Where(e => affected.Contains(e.UniqueId)).Select(e => new object[] {
                e.CaseId, e.ElementId, e.UniqueId, t.Category, t.TypeName, t.TypeUniqueId, t.Status,
                string.Join("; ", t.Fields.Where(f => f.Status == "Missing").Select(f => f.Field)),
                string.Join("; ", t.Fields.Where(f => f.Status == "Missing").Select(f => f.RuleId)) })).ToList();
            Table(sheet, 4, "AffectedInstances", new[] { "Case ID", "Element ID", "Element unique ID", "Category", "Type", "Type unique ID", "Type status", "Missing fields", "Rule IDs" }, rows);
            sheet.Column(3).Width = 44; sheet.Column(5).Width = 30; sheet.Column(6).Width = 44; sheet.Column(9).Width = 42;
            sheet.SheetView.FreezeColumns(2);
        }

        private static void Rules(XLWorkbook workbook, InformationReport r)
        {
            var sheet = Sheet(workbook, "Rules and coverage", 12, "Required, optional and unexamined fields are retained. Nonempty text does not validate rating, assembly, quantity or code compliance.");
            string[] metadata = { "Coverage", "Scope", "Checklist ID", "Checklist path", "Level / phase", "Units", "Exclusions" };
            string[] contents = { Coverage(r), r.Scope, r.ChecklistId, r.ChecklistPath, r.Level + " / " + r.Phase + " (New)", r.Units,
                r.OtherHostWallCeilingInstances + " other host wall/ceiling instances; " + r.LinkInstancesExcluded + " link instances" };
            for (int i = 0; i < metadata.Length; i++)
            {
                sheet.Cell(i + 4, 1).Value = metadata[i]; sheet.Cell(i + 4, 1).Style.Font.Bold = true;
                sheet.Range(i + 4, 2, i + 4, 6).Merge().Value = contents[i];
                sheet.Range(i + 4, 2, i + 4, 6).Style.Alignment.WrapText = true;
                sheet.Row(i + 4).Height = 32;
            }
            var rows = r.Evaluation.Types.SelectMany(t => t.Fields.Select(f => new object[] {
                t.Category, t.TypeName, t.TypeUniqueId, f.RuleId, f.Field,
                f.Required.HasValue ? (f.Required.Value ? "Yes" : "No") : "Unknown", f.Status,
                f.Reason, f.ObservedValue, f.ParameterId, f.RequirementSource, t.Status })).ToList();
            Table(sheet, 14, "EvaluatedRules", new[] { "Category", "Type", "Type unique ID", "Rule ID", "Field", "Required", "Status", "Reason", "Observed value", "Parameter ID", "Requirement source", "Type status" }, rows);
            int issueRow = 18 + rows.Count;
            sheet.Cell(issueRow - 1, 1).Value = "Scope issues (unexamined cases)";
            Header(sheet, issueRow, "Case ID", "Element unique ID", "Status", "Reason");
            for (int i = 0; i < r.ScopeIssues.Count; i++) {
                var issue = r.ScopeIssues[i];
                object[] cells = { issue.CaseId, issue.UniqueId, issue.Status, issue.Reason };
                for (int j = 0; j < cells.Length; j++) Put(sheet.Cell(issueRow + i + 1, j + 1), cells[j]);
                sheet.Row(issueRow + i + 1).Height = 60;
                sheet.Range(issueRow + i + 1, 1, issueRow + i + 1, 4).Style.Alignment.WrapText = true;
            }
            if (r.ScopeIssues.Count == 0) sheet.Cell(issueRow + 1, 1).Value = "No scope issues.";
            int unexaminedRow = issueRow + Math.Max(1, r.ScopeIssues.Count) + 4;
            sheet.Cell(unexaminedRow - 1, 1).Value = "Elements with unexamined type fields";
            Header(sheet, unexaminedRow, "Case ID", "Element unique ID", "Element ID", "Type", "Type unique ID", "Category", "Unexamined fields", "Reasons", "Rule IDs");
            var unexamined = new HashSet<string>(r.Evaluation.UnexaminedElements.Select(e => e.UniqueId), StringComparer.Ordinal);
            int outputRow = unexaminedRow + 1;
            foreach (var type in r.Evaluation.Types)
            foreach (var element in type.Elements.Where(e => unexamined.Contains(e.UniqueId)))
            {
                var fields = type.Fields.Where(f => f.Status == "Unexamined").ToList();
                object[] cells = { element.CaseId, element.UniqueId, element.ElementId, type.TypeName, type.TypeUniqueId, type.Category,
                    string.Join("; ", fields.Select(f => f.Field)), string.Join("; ", fields.Select(f => f.Reason)), string.Join("; ", fields.Select(f => f.RuleId)) };
                for (int j = 0; j < cells.Length; j++) Put(sheet.Cell(outputRow, j + 1), cells[j]);
                sheet.Range(outputRow, 1, outputRow, cells.Length).Style.Alignment.WrapText = true;
                sheet.Row(outputRow++).Height = 60;
            }
            if (unexamined.Count == 0) sheet.Cell(outputRow, 1).Value = "No unexamined type fields.";
            sheet.Column(2).Width = 44; sheet.Column(3).Width = 44; sheet.Column(4).Width = 34;
            sheet.Column(10).Width = 44; sheet.Column(11).Width = 44;
            sheet.SheetView.FreezeRows(14); sheet.SheetView.FreezeColumns(2);
        }
    }
}
