using System.Collections.Generic;
using BimAgentLab.Readiness.Core;

// Test-only API surface, never a substitute for Autodesk scheduling or model acceptance.
namespace Autodesk.Revit.DB
{
    public class Element
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
    }
    public sealed class Level : Element { }
    public sealed class Phase : Element { }
    public sealed class Document
    {
        public readonly Dictionary<string, Element> Elements = new Dictionary<string, Element>();
        public bool IsFamilyDocument { get; set; }
        public bool IsLinked { get; set; }
        public bool IsValidObject { get; set; } = true;
        public string PathName { get; set; }
        public Element ProjectInformation { get; set; }
        public Element GetElement(string uniqueId) => Elements.TryGetValue(uniqueId, out var element) ? element : null;
    }
}
namespace Autodesk.Revit.UI
{
    public sealed class UIDocument { public DB.Document Document { get; set; } }
    public sealed class ApplicationDouble { public string VersionBuild => "Autodesk-boundary-double"; }
    public sealed class UIApplication
    {
        public UIDocument ActiveUIDocument { get; set; }
        public ApplicationDouble Application { get; } = new ApplicationDouble();
    }
}
namespace BimAgentLab.RevitReadiness
{
    internal static class RequiredInformationReader
    {
        internal static int Calls;
        internal static InformationReport Read(Autodesk.Revit.DB.Document document, Checklist checklist, string path)
        {
            Calls++;
            return new InformationReport { ChecklistId = checklist.ChecklistId, DocumentPath = document.PathName,
                ChecklistPath = path, CoverageComplete = false, Evaluation = new Evaluation { Complete = false } };
        }
    }
}
