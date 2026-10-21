using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using BimAgentLab.Readiness.Core;

namespace BimAgentLab.RevitReadiness
{
    internal static class RequiredInformationReader
    {
        internal static readonly Guid AssemblyGuid = new Guid("cd425f17-1bb4-4d3f-a088-902728147491");
        internal static InformationReport Read(Document document, Checklist checklist, string checklistPath)
        {
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Record("ReaderEntered");
#endif
            checklist.Validate();
            if (document.ProjectInformation.UniqueId != checklist.ProjectUniqueId ||
                !string.Equals(Path.GetFileName(document.PathName), checklist.ProjectFileName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The checklist belongs to a different project or file. Verify its identity before using it.");
            var level = document.GetElement(checklist.LevelUniqueId) as Level;
            var phase = document.GetElement(checklist.PhaseUniqueId) as Phase;
            if (level == null || phase == null) throw new ArgumentException("Checklist level or phase is not present in this project.");
            var report = new InformationReport {
                GeneratedUtc = DateTime.UtcNow.ToString("o"), DocumentPath = document.PathName,
                ProjectUniqueId = document.ProjectInformation.UniqueId, RevitBuild = document.Application.VersionBuild,
                ChecklistId = checklist.ChecklistId, ChecklistPath = checklistPath,
                Scope = "Explicit checklist element UniqueIds; host basic walls and ceilings on the declared level, New in the declared phase. Types are checked once; links and other elements excluded. No view/selection filter or result cap.",
                Level = level.Name, Phase = phase.Name, Units = "Instance counts and text; no measured quantities.",
                RequestedInstances = checklist.Cases.Count, ModifiedBefore = document.IsModified
            };
            var types = new Dictionary<string, TypeObservation>(StringComparer.Ordinal);
#if BIM_BRIDGE_DIAGNOSTICS
            BridgeDiagnostics.Hold("Reader"); // Actual reader has already accessed Autodesk data above.
#endif
            foreach (var expected in checklist.Cases)
            {
                var element = document.GetElement(expected.UniqueId);
                string problem = null;
                string category = element is Wall ? "Walls" : element is Ceiling ? "Ceilings" : null;
                if (element == null) problem = "Element not found";
                else if (category == null || (element is Wall wall && wall.WallType.Kind != WallKind.Basic)) problem = "Unsupported category or non-basic wall";
                else if (element.LevelId != level.Id) problem = "Element is outside the declared level";
                else if (element.GetPhaseStatus(phase.Id) != ElementOnPhaseStatus.New) problem = "Element is not New in the declared phase";
                else if (element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.AsString() != expected.CaseId) problem = "Case Mark differs from the checklist";
                if (problem != null)
                {
                    report.ScopeIssues.Add(new ScopeIssue { UniqueId = expected.UniqueId, CaseId = expected.CaseId, Status = "Unexamined", Reason = problem });
                    continue;
                }
                var type = document.GetElement(element.GetTypeId());
                if (type == null) { report.ScopeIssues.Add(new ScopeIssue { UniqueId = expected.UniqueId, CaseId = expected.CaseId, Status = "Unexamined", Reason = "Type unavailable" }); continue; }
                if (!types.TryGetValue(type.UniqueId, out var observed))
                {
                    observed = new TypeObservation { UniqueId = type.UniqueId, Name = type.Name, Category = category };
                    observed.Values["Code"] = ReadText(type.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK), "BuiltInParameter.ALL_MODEL_TYPE_MARK");
                    observed.Values["FireRating"] = ReadText(type.get_Parameter(BuiltInParameter.FIRE_RATING), "BuiltInParameter.FIRE_RATING");
                    observed.Values["AssemblyReference"] = ReadText(type.get_Parameter(AssemblyGuid), "SharedParameter:" + AssemblyGuid);
                    types.Add(type.UniqueId, observed);
                }
                observed.Elements.Add(new ElementReference { UniqueId = element.UniqueId, ElementId = element.Id.Value.ToString(CultureInfo.InvariantCulture), CaseId = expected.CaseId });
            }
            report.ExaminedInstances = types.Values.Sum(t => t.Elements.Count);
            using (var collector = new FilteredElementCollector(document))
            {
                var selected = new HashSet<string>(checklist.Cases.Select(c => c.UniqueId), StringComparer.Ordinal);
                report.OtherHostWallCeilingInstances = collector.WherePasses(new ElementMulticategoryFilter(new[] { BuiltInCategory.OST_Walls, BuiltInCategory.OST_Ceilings }))
                    .WhereElementIsNotElementType().Count(e => !selected.Contains(e.UniqueId));
            }
            using (var collector = new FilteredElementCollector(document)) report.LinkInstancesExcluded = collector.OfClass(typeof(RevitLinkInstance)).GetElementCount();
            report.Evaluation = TypeInformation.Evaluate(types.Values.OrderBy(t => t.Name, StringComparer.Ordinal).ToList(), checklist.Requirements);
            report.CoverageComplete = report.ScopeIssues.Count == 0 && report.Evaluation.Complete;
            report.ModifiedAfter = document.IsModified;
            if (report.ModifiedBefore != report.ModifiedAfter) throw new InvalidOperationException("Document modified state changed during the read.");
            return report;
        }
        private static ParameterObservation ReadText(Parameter parameter, string identity)
        {
            return new ParameterObservation { ParameterId = identity,
                State = parameter == null ? "Absent" : parameter.StorageType == StorageType.String ? "Present" : "Unsupported",
                Value = parameter != null && parameter.StorageType == StorageType.String ? parameter.AsString() : null };
        }
    }
}
