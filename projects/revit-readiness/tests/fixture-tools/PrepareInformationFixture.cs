using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAgentLab.Readiness.Core;

namespace BimAgentLab.FixtureTools
{
    public sealed class FixtureSettings { public string SourceCopy { get; set; } }

    // Test-only editing command. Not included by the normal readiness installation.
    [Transaction(TransactionMode.Manual)]
    public sealed class PrepareInformationFixture : IExternalCommand
    {
        private static readonly Guid AssemblyGuid = new Guid("cd425f17-1bb4-4d3f-a088-902728147491");
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var ui = commandData.Application;
                if (ui.ActiveUIDocument != null) throw new InvalidOperationException("Close the active project before preparing the separate sample copies.");
                var settings = JsonFile.Read<FixtureSettings>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fixture-settings.json"));
                string source = Path.GetFullPath(settings.SourceCopy);
                if (Path.GetFileName(source) != "Snowdon-BAL-source-copy.rvt" || !source.Contains(Path.DirectorySeparatorChar + "local" + Path.DirectorySeparatorChar))
                    throw new InvalidOperationException("Fixture preparation requires the explicitly named source copy inside samples/local.");
                string folder = Path.GetDirectoryName(source);
                string[] names = { "BAL-info-complete", "BAL-info-wall-defects", "BAL-info-ceiling-negative" };
                foreach (string name in names)
                    if (File.Exists(Path.Combine(folder, name + ".rvt"))) throw new InvalidOperationException("Prepared copy already exists: " + name + ". Use a fresh fixture folder.");
                var options = new OpenOptions { DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets };
                var uidoc = ui.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(source), options, false);
                var doc = uidoc.Document;
                var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
                var level = levels.FirstOrDefault(l => l.Elevation >= -0.01) ?? levels[0];
                var phase = doc.Phases.Cast<Phase>().Last();
                var wallSource = new FilteredElementCollector(doc).OfClass(typeof(WallType)).Cast<WallType>().Where(t => t.Kind == WallKind.Basic).OrderBy(t => t.Name).First();
                var ceilingSource = new FilteredElementCollector(doc).OfClass(typeof(CeilingType)).Cast<CeilingType>().OrderBy(t => t.Name).First();
                JsonFile.Write(Path.Combine(folder, "source-inspection.json"), new {
                    SourceCopy = source, RevitBuild = doc.Application.VersionBuild, WasWorksharedAfterDetach = doc.IsWorkshared,
                    Levels = levels.Select(l => new { l.Name, l.UniqueId, ElevationFeet = l.Elevation }).ToArray(),
                    Phases = doc.Phases.Cast<Phase>().Select(p => new { p.Name, p.UniqueId }).ToArray(),
                    BaseWallType = wallSource.Name, BaseCeilingType = ceilingSource.Name,
                    WallTypeMark = Describe(wallSource.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)),
                    WallFireRating = Describe(wallSource.get_Parameter(BuiltInParameter.FIRE_RATING)),
                    CeilingTypeMark = Describe(ceilingSource.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK))
                });
                var cases = new List<ElementReference>();
                var requirements = new List<TypeRequirement>();
                var wallTypes = new List<WallType>();
                var ceilingTypes = new List<CeilingType>();
                View3D view;
                using (var tx = new Transaction(doc, "Prepare isolated BIM Agent Lab information cases"))
                {
                    tx.Start(); BindAssemblyParameter(ui, doc, folder);
                    for (int i = 0; i < 4; i++)
                    {
                        string letter = ((char)('A' + i)).ToString();
                        var type = (WallType)wallSource.Duplicate("BAL INFO Wall " + letter);
                        wallTypes.Add(type);
                        SetText(type, BuiltInParameter.ALL_MODEL_TYPE_MARK, "W-" + letter);
                        SetText(type, BuiltInParameter.FIRE_RATING, i < 2 ? "60 min" : "");
                        SetText(type.get_Parameter(AssemblyGuid), i < 2 ? "DEMO-ASM-01" : "");
                        requirements.Add(Rule(type, "Walls", i < 2));
                        for (int j = 0; j < 3; j++)
                        {
                            double x = 1000 + j * 20, y = 1000 + i * 20;
                            var wall = Wall.Create(doc, Line.CreateBound(new XYZ(x, y, level.Elevation), new XYZ(x + 10, y, level.Elevation)), type.Id, level.Id, 9, 0, false, false);
                            wall.CreatedPhaseId = phase.Id;
                            Mark(wall, "BAL-INFO-W" + letter + "-" + (j + 1).ToString("00"), cases);
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        string label = i == 0 ? "ACT" : "GWB";
                        var type = (CeilingType)ceilingSource.Duplicate("BAL INFO Ceiling " + label);
                        ceilingTypes.Add(type); SetText(type, BuiltInParameter.ALL_MODEL_TYPE_MARK, "C-" + label);
                        requirements.Add(Rule(type, "Ceilings", false));
                        for (int j = 0; j < 2; j++)
                        {
                            double x = 1080 + j * 15, y = 1000 + i * 20;
                            var corners = new[] { new XYZ(x,y,0), new XYZ(x+10,y,0), new XYZ(x+10,y+10,0), new XYZ(x,y+10,0) };
                            var loop = new CurveLoop();
                            for (int k = 0; k < 4; k++) loop.Append(Line.CreateBound(corners[k], corners[(k+1)%4]));
                            var ceiling = Ceiling.Create(doc, new[] { loop }, type.Id, level.Id);
                            ceiling.CreatedPhaseId = phase.Id;
                            ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).Set(9.0);
                            Mark(ceiling, "BAL-INFO-C" + label + "-" + (j+1).ToString("00"), cases);
                        }
                    }
                    doc.Regenerate();
                    CreateSchedule(doc, BuiltInCategory.OST_Walls, "BAL INFO Walls", phase, true);
                    CreateSchedule(doc, BuiltInCategory.OST_Ceilings, "BAL INFO Ceilings", phase, false);
                    var viewType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().First(t => t.ViewFamily == ViewFamily.ThreeDimensional);
                    view = View3D.CreateIsometric(doc, viewType.Id); view.Name = "BAL INFO - Controlled cases";
                    view.SetSectionBox(new BoundingBoxXYZ { Min = new XYZ(995,995,level.Elevation-2), Max = new XYZ(1110,1070,level.Elevation+14) });
                    view.IsSectionBoxActive = true;
                    CommitFixture(tx);
                }
                uidoc.ActiveView = view;
                SaveCase(doc, folder, names[0], cases, requirements, level, phase);
                using (var tx = new Transaction(doc, "Introduce two declared wall information gaps"))
                {
                    tx.Start(); SetText(wallTypes[1], BuiltInParameter.FIRE_RATING, ""); SetText(wallTypes[3], BuiltInParameter.ALL_MODEL_TYPE_MARK, ""); CommitFixture(tx);
                }
                SaveCase(doc, folder, names[1], cases, requirements, level, phase);
                using (var tx = new Transaction(doc, "Introduce the ceiling negative control"))
                {
                    tx.Start(); SetText(ceilingTypes[0], BuiltInParameter.ALL_MODEL_TYPE_MARK, ""); CommitFixture(tx);
                }
                SaveCase(doc, folder, names[2], cases, requirements, level, phase);
                TaskDialog.Show("BIM Agent Lab - Fixture prepared", "Created three sample-derived copies, six duplicated types, 16 marked instances, separate checklists, and itemized Revit schedule exports.\n\nCurrent file: " + doc.PathName);
                return Result.Succeeded;
            }
            catch (Exception e) { message = "Fixture preparation failed: " + e.Message; return Result.Failed; }
        }
        private static object Describe(Parameter p) => p == null ? null : new { Name = p.Definition.Name, Storage = p.StorageType.ToString(), Value = p.StorageType == StorageType.String ? p.AsString() : null };
        private static TypeRequirement Rule(ElementType type, string category, bool rated) => new TypeRequirement {
            TypeUniqueId = type.UniqueId, Category = category, Source = "BAL-TYPE-INFO-v1: " + type.Name,
            CodeRequired = true, FireRatingRequired = rated, AssemblyReferenceRequired = rated
        };
        private static void Mark(Element e, string mark, List<ElementReference> cases)
        {
            SetText(e, BuiltInParameter.ALL_MODEL_MARK, mark);
            cases.Add(new ElementReference { UniqueId = e.UniqueId, ElementId = e.Id.Value.ToString(), CaseId = mark });
        }
        private static void SetText(Element e, BuiltInParameter id, string value) => SetText(e.get_Parameter(id), value);
        private static void SetText(Parameter p, string value)
        {
            if (p == null || p.IsReadOnly || p.StorageType != StorageType.String)
                throw new InvalidOperationException("Fixture text parameter is unavailable, read-only, or not text.");
            // Revit may return false for an unchanged value; verify the resulting text instead.
            if (string.Equals(p.AsString() ?? "", value, StringComparison.Ordinal)) return;
            p.Set(value);
            if (!string.Equals(p.AsString() ?? "", value, StringComparison.Ordinal))
                throw new InvalidOperationException("Fixture text parameter did not retain the requested value.");
        }
        private static void CommitFixture(Transaction tx)
        {
            var options = tx.GetFailureHandlingOptions();
            options.SetForcedModalHandling(true);
            tx.SetFailureHandlingOptions(options);
            var status = tx.Commit();
            if (status != TransactionStatus.Committed)
                throw new InvalidOperationException("Fixture transaction was not committed (" + status + "). No subsequent case will be saved.");
        }
        private static void BindAssemblyParameter(UIApplication ui, Document doc, string folder)
        {
            string original = ui.Application.SharedParametersFilename;
            string path = Path.Combine(folder, "fixture-shared-parameters.txt");
            File.WriteAllText(path, "# BIM Agent Lab teaching parameter\n*META\tVERSION\tMINVERSION\nMETA\t2\t1\n*GROUP\tID\tNAME\nGROUP\t1\tBIM Agent Lab\n*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE\n");
            try
            {
                ui.Application.SharedParametersFilename = path;
                var file = ui.Application.OpenSharedParameterFile();
                if (file == null) throw new InvalidOperationException("Cannot open fixture shared-parameter definitions.");
                var group = file.Groups.get_Item("BIM Agent Lab");
                var definition = group.Definitions.Create(new ExternalDefinitionCreationOptions("BAL_AssemblyReference", SpecTypeId.String.Text) {
                    GUID = AssemblyGuid, Description = "Synthetic workshop reference; presence is not assembly validation.", Visible = true, UserModifiable = true
                });
                var categories = ui.Application.Create.NewCategorySet();
                categories.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls));
                categories.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Ceilings));
                if (!doc.ParameterBindings.Insert(definition, ui.Application.Create.NewTypeBinding(categories), GroupTypeId.IdentityData))
                    throw new InvalidOperationException("Fixture assembly parameter could not be bound.");
                doc.Regenerate();
            }
            finally { ui.Application.SharedParametersFilename = original; }
        }
        private static void CreateSchedule(Document doc, BuiltInCategory category, string name, Phase phase, bool walls)
        {
            var schedule = ViewSchedule.CreateSchedule(doc, new ElementId(category)); schedule.Name = name;
            schedule.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(phase.Id);
            var definition = schedule.Definition; definition.IsItemized = true;
            var available = definition.GetSchedulableFields();
            Func<ElementId, string, ScheduleField> add = (id, heading) => {
                var field = definition.AddField(available.Single(f => f.ParameterId == id)); field.ColumnHeading = heading; return field;
            };
            var mark = add(new ElementId(BuiltInParameter.ALL_MODEL_MARK), "CaseId");
            add(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM), "Type");
            add(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_MARK), "Code");
            if (walls)
            {
                add(new ElementId(BuiltInParameter.FIRE_RATING), "FireRating");
                add(SharedParameterElement.Lookup(doc, AssemblyGuid).Id, "AssemblyReference");
            }
            definition.AddFilter(new ScheduleFilter(mark.FieldId, ScheduleFilterType.BeginsWith, "BAL-INFO-"));
            definition.AddSortGroupField(new ScheduleSortGroupField(mark.FieldId) { SortOrder = ScheduleSortOrder.Ascending });
        }
        private static void SaveCase(Document doc, string folder, string name, List<ElementReference> cases, List<TypeRequirement> requirements, Level level, Phase phase)
        {
            string path = Path.Combine(folder, name + ".rvt");
            doc.SaveAs(path, new SaveAsOptions { OverwriteExistingFile = false, MaximumBackups = 1 });
            var checklist = new Checklist { SchemaVersion = 1, ChecklistId = "BAL-TYPE-INFO-v1", ProjectUniqueId = doc.ProjectInformation.UniqueId,
                ProjectFileName = Path.GetFileName(path), LevelUniqueId = level.UniqueId, PhaseUniqueId = phase.UniqueId, Cases = cases, Requirements = requirements };
            JsonFile.Write(Path.ChangeExtension(path, ".checklist.json"), checklist);
            foreach (var schedule in new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>().Where(s => s.Name == "BAL INFO Walls" || s.Name == "BAL INFO Ceilings"))
                schedule.Export(folder, name + "-" + (schedule.Name.EndsWith("Walls") ? "walls" : "ceilings") + ".tsv", new ViewScheduleExportOptions { FieldDelimiter = "\t", Title = false, ColumnHeaders = ExportColumnHeaders.OneRow });
        }
    }
}
