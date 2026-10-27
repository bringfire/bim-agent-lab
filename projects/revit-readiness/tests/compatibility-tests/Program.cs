using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BimAgentLab.Readiness.Core;
using BimAgentLab.RevitReadiness;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

#if NET10_0_OR_GREATER
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

internal static class Program
{
    static int failed, total;
    static string root;
    static int Main(string[] args)
    {
        root = Path.GetFullPath(args[0]); Directory.CreateDirectory(root);
        Test("core checklist roundtrip preserves identities and explicit requirements", () => {
            var path = Path.Combine(root, "checklist.json"); JsonFile.Write(path, Checklist());
            var value = JsonFile.Read<Checklist>(path); value.Validate();
            Equal("case-1", value.Cases.Single().CaseId); Equal(false, value.Requirements.Single().FireRatingRequired);
            Equal("Mür – 墙 😀 /Date(0)/", value.ChecklistId);
        });
        Test("adapter parses the same core checklist", () => {
            var parsed = ReadinessPayloadCodec.ParseChecklist(File.ReadAllText(Path.Combine(root, "checklist.json")));
            Equal("project-1", parsed.ProjectUniqueId); Equal("type-1", parsed.Requirements.Single().TypeUniqueId);
        });
        Test("adapter preserves escaped date-like string without coercion", () => {
            var text = File.ReadAllText(Path.Combine(root, "checklist.json"));
            text = text.Replace("Mür – 墙 😀 /Date(0)/", "\\/Date(9999999999999999999)\\/");
            // Write a separate simple identity so this input does not depend on Unicode escaping choices.
            var simple = Checklist(); simple.ChecklistId = "date-placeholder";
            JsonFile.Write(Path.Combine(root, "date.json"), simple);
            text = File.ReadAllText(Path.Combine(root, "date.json")).Replace("date-placeholder", "\\/Date(9999999999999999999)\\/");
            Equal("/Date(9999999999999999999)/", ReadinessPayloadCodec.ParseChecklist(text).ChecklistId);
        });
        Test("report roundtrip preserves false completeness, counts and lists", () => {
            var report = Report(); var path = Path.Combine(root, "report.json");
            JsonFile.Write(path, report); var parsed = JsonFile.Read<InformationReport>(path);
            Equal(false, parsed.CoverageComplete); Equal(false, parsed.Evaluation.Complete);
            Equal(false, parsed.ModifiedBefore); Equal(false, parsed.ModifiedAfter);
            Equal(16, parsed.RequestedInstances); Equal(15, parsed.ExaminedInstances);
            Equal("Element not found", parsed.ScopeIssues.Single().Reason);
            Equal("case-1", parsed.Evaluation.UnexaminedElements.Single().CaseId);
            var text = File.ReadAllText(path);
            if (!text.Contains("\"CoverageComplete\":false") || !text.Contains("\"Complete\":false")) throw new Exception("False flags were omitted.");
        });
        Test("adapter report serializes the same semantic report", () => {
            var text = ReadinessPayloadCodec.SerializeReport(Report());
            var path = Path.Combine(root, "adapter-report.json"); File.WriteAllText(path, text, new UTF8Encoding(false));
            var parsed = JsonFile.Read<InformationReport>(path);
            Equal("Mür – 墙 😀 /Date(0)/", parsed.ChecklistId); Equal(false, parsed.CoverageComplete);
            Equal("element-missing", parsed.ScopeIssues.Single().UniqueId);
        });
        Test("trailing JSON cannot reach checklist evaluation", () => Invalid(File.ReadAllText(Path.Combine(root, "checklist.json")) + "{}"));
        Test("malformed JSON is classified ChecklistInvalid", () => Invalid("{not-json}"));
        Test("missing identity is classified ChecklistInvalid", () => { var value = Checklist(); value.ProjectUniqueId = null; var p = Path.Combine(root,"missing.json"); JsonFile.Write(p,value); Invalid(File.ReadAllText(p)); });
        Test("null requirement flags cannot become false defaults", () => { var value = Checklist(); value.Requirements[0].CodeRequired = null; var p = Path.Combine(root,"null.json"); JsonFile.Write(p,value); Invalid(File.ReadAllText(p)); });
        Test("valid checklist at four MiB remains accepted", () => {
            var text = File.ReadAllText(Path.Combine(root,"date.json"));
            text += new string(' ', ChecklistSnapshot.MaxBytes - Encoding.UTF8.GetByteCount(text));
            var p = Path.Combine(root,"limit.json"); File.WriteAllText(p,text,new UTF8Encoding(false));
            Equal("date-placeholder", ReadinessPayloadCodec.DecodeChecklist(ChecklistSnapshot.Read(p)).ChecklistId);
        });
        Test("oversized checklist cannot be parsed directly", () => Invalid(File.ReadAllText(Path.Combine(root,"date.json")) + new string(' ', ChecklistSnapshot.MaxBytes)));
        Test("invalid UTF8 is classified ChecklistInvalid", () => {
            var p=Path.Combine(root,"invalid-utf8.json"); File.WriteAllBytes(p,new byte[]{0xff,0xfe,0x80});
            try { ReadinessPayloadCodec.DecodeChecklist(ChecklistSnapshot.Read(p)); throw new Exception("Invalid UTF8 accepted."); }
            catch(BridgeException e) { Equal("ChecklistInvalid",e.Error.Code); }
        });
        Test("oversized report is classified ResponseTooLarge", () => {
            var report=Report(); report.Scope=new string('x', ChecklistSnapshot.MaxBytes);
            try { ReadinessPayloadCodec.SerializeReport(report); throw new Exception("Oversized report accepted."); }
            catch(BridgeException e) { Equal("ResponseTooLarge",e.Error.Code); }
        });
        Console.WriteLine("Compatibility JSON: " + (total-failed) + "/" + total + " passed; " + failed + " failed.");
        return failed == 0 ? 0 : 1;
    }
    static Checklist Checklist() { return new Checklist {
        SchemaVersion=1,ChecklistId="Mür – 墙 😀 /Date(0)/",ProjectUniqueId="project-1",ProjectFileName="test.rvt",LevelUniqueId="level-1",PhaseUniqueId="phase-1",
        Cases=new List<ElementReference>{new ElementReference{CaseId="case-1",UniqueId="element-1",ElementId="123"}},
        Requirements=new List<TypeRequirement>{new TypeRequirement{TypeUniqueId="type-1",Category="Walls",Source="independent requirement",CodeRequired=true,FireRatingRequired=false,AssemblyReferenceRequired=false}}
    }; }
    static InformationReport Report() { return new InformationReport {
        GeneratedUtc="2026-10-27T00:00:00Z",DocumentPath="C:\\sample\\test.rvt",ChecklistId="Mür – 墙 😀 /Date(0)/",RequestedInstances=16,ExaminedInstances=15,CoverageComplete=false,ModifiedBefore=false,ModifiedAfter=false,
        ScopeIssues=new List<ScopeIssue>{new ScopeIssue{CaseId="missing-case",UniqueId="element-missing",Status="Unexamined",Reason="Element not found"}},
        Evaluation=new Evaluation{Complete=false,UnexaminedElements=new List<ElementReference>{new ElementReference{CaseId="case-1",UniqueId="element-1"}}}
    }; }
    static void Invalid(string text) { try { ReadinessPayloadCodec.ParseChecklist(text); throw new Exception("Invalid checklist accepted."); } catch(BridgeException e) { Equal("ChecklistInvalid",e.Error.Code); } }
    static void Equal<T>(T expected,T actual) { if(!Equals(expected,actual)) throw new Exception("Expected " + expected + "; got " + actual); }
    static void Test(string name,Action run) { total++; try { run(); Console.WriteLine("PASS " + name); } catch(Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e); } }
}
