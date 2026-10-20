using System;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAgentLab.Readiness.Core;

namespace BimAgentLab.RevitReadiness
{
    [Transaction(TransactionMode.ReadOnly)]
    public sealed class RequiredInformationCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var document = commandData.Application.ActiveUIDocument?.Document;
            if (document == null || document.IsFamilyDocument) return Result.Cancelled;
            try
            {
                if (string.IsNullOrWhiteSpace(document.PathName)) throw new ArgumentException("Save the project and supply its checklist before checking readiness.");
                string checklistPath = Path.ChangeExtension(document.PathName, ".checklist.json");
                if (!File.Exists(checklistPath)) throw new ArgumentException("No checklist found: " + checklistPath);
                var report = RequiredInformationReader.Read(document, JsonFile.Read<Checklist>(checklistPath), checklistPath);
                string output = Path.ChangeExtension(document.PathName, ".readiness.json");
                JsonFile.Write(output, report);
                var details = new StringBuilder();
                foreach (var row in report.Evaluation.Types)
                {
                    details.AppendLine($"{row.Category} / {row.TypeName}: {row.Elements.Count} instances; {row.Status}");
                    foreach (var field in row.Fields.Where(f => f.Status == "Missing" || f.Status == "Unexamined"))
                        details.AppendLine($"  {field.Field}: {field.Status} ({field.Reason}); rule {field.RuleId}");
                    details.AppendLine("  Cases: " + string.Join(", ", row.Elements.Select(e => e.CaseId)));
                }
                foreach (var issue in report.ScopeIssues) details.AppendLine($"{issue.CaseId}: {issue.Status} - {issue.Reason}");
                File.WriteAllText(Path.ChangeExtension(document.PathName, ".readiness.txt"), details.ToString(), new UTF8Encoding(false));
                new TaskDialog("BIM Agent Lab - Required type information") {
                    TitleAutoPrefix = false,
                    MainInstruction = $"{report.Evaluation.FindingCount} missing required type values",
                    MainContent = $"{report.Evaluation.AffectedElements.Count} affected instances; {report.Evaluation.Types.Count} used types.\n" +
                        $"Examined {report.ExaminedInstances} of {report.RequestedInstances} declared cases. Coverage: {(report.CoverageComplete ? "complete for this checklist scope" : "INCOMPLETE; inspect unexamined cases")}.\n" +
                        $"Level: {report.Level}; phase: {report.Phase} (New).\n" +
                        $"Outside scope: {report.OtherHostWallCeilingInstances} host wall/ceiling instances and {report.LinkInstancesExcluded} link instances.\n\n" +
                        $"Document modified: {report.ModifiedBefore} before / {report.ModifiedAfter} after.\nRead-only model check; local reports written.\n\n" + details,
                    ExpandedContent = $"Checklist: {report.ChecklistId}\nReport: {output}\nRevit build: {report.RevitBuild}\nCompleteness of supplied text only; no rating, assembly, quantity, or code-compliance validation.",
                    CommonButtons = TaskDialogCommonButtons.Close
                }.Show();
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                message = "Required-information check failed: " + exception.Message;
                return Result.Failed;
            }
        }
    }
    public sealed class ProjectAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet categories)
        {
            var document = applicationData.ActiveUIDocument?.Document;
            return document != null && !document.IsFamilyDocument;
        }
    }
}
