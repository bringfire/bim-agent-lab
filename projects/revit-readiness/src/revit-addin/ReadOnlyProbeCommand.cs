using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimAgentLab.RevitProbe
{
    [Transaction(TransactionMode.ReadOnly)]
    public sealed class ReadOnlyProbeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument?.Document;
            if (document == null || document.IsFamilyDocument)
            {
                message = "Open a Revit project to run the read-only probe.";
                return Result.Cancelled;
            }

            try
            {
                bool modifiedBefore = document.IsModified;
                int walls = CountInstances(document, BuiltInCategory.OST_Walls);
                int ceilings = CountInstances(document, BuiltInCategory.OST_Ceilings);
                bool modifiedAfter = document.IsModified;
                if (modifiedBefore != modifiedAfter)
                {
                    message = "The document's modified state changed during the read-only probe. Verification failed.";
                    return Result.Failed;
                }

                var application = commandData.Application.Application;
                var dialog = new TaskDialog("BIM Agent Lab - Read-only probe")
                {
                    TitleAutoPrefix = false,
                    MainInstruction = document.Title,
                    MainContent =
                        $"Wall instances: {walls}\nCeiling instances: {ceilings}\n\n" +
                        "Scope: entire active host project. Types and linked documents excluded.\n" +
                        "All phases, design options, and levels; no view filter or result limit.\n\n" +
                        $"Document modified: {modifiedBefore} before / {modifiedAfter} after.\n" +
                        "Read-only command; no model changes.",
                    ExpandedContent =
                        $"Revit: {application.VersionNumber}; build {application.VersionBuild}\n" +
                        $"Project path: {(string.IsNullOrEmpty(document.PathName) ? "(unsaved)" : document.PathName)}\n" +
                        "Category instance counts are not takeoff quantities or readiness findings.",
                    CommonButtons = TaskDialogCommonButtons.Close
                };
                dialog.Show();
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                message = "Read-only probe failed: " + exception.Message;
                return Result.Failed;
            }
        }

        private static int CountInstances(Document document, BuiltInCategory category)
        {
            // The document constructor intentionally avoids active-view visibility filtering.
            using (var collector = new FilteredElementCollector(document))
            {
                return collector.OfCategory(category).WhereElementIsNotElementType().GetElementCount();
            }
        }
    }

    public sealed class ProjectAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            Document document = applicationData.ActiveUIDocument?.Document;
            return document != null && !document.IsFamilyDocument;
        }
    }
}
