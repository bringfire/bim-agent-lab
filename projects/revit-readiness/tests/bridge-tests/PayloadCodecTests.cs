using System;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
#if NET48
using BimAgentLab.Readiness.Core;
using BimAgentLab.RevitReadiness;
using System.Web.Script.Serialization;
#endif
namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    static async Task PayloadCodecTests()
    {
#if NET48
        await Test("Actual net48 checklist codec classifies invalid UTF-8", () => Sync(() =>
        {
            using var stream = new System.IO.MemoryStream(new byte[] { 0xff });
            var snapshot = BimAgentLab.RevitBridge.Runtime.ChecklistSnapshot.Capture(stream);
            Equal("ChecklistInvalid", Throws<BridgeException>(() => ReadinessPayloadCodec.DecodeChecklist(snapshot)).Error.Code);
        }));
        const string checklist = "{\"SchemaVersion\":1,\"ChecklistId\":\"/Date(0)/\",\"ProjectUniqueId\":\"p\",\"ProjectFileName\":\"sample.rvt\",\"LevelUniqueId\":\"l\",\"PhaseUniqueId\":\"h\",\"Cases\":[{\"UniqueId\":\"e\",\"CaseId\":\"墙\"}],\"Requirements\":[{\"TypeUniqueId\":\"t\",\"Category\":\"Walls\",\"Source\":\"/Date(0)/\",\"CodeRequired\":true,\"FireRatingRequired\":false,\"AssemblyReferenceRequired\":false}]}";
        await Test("Actual net48 checklist codec preserves typed date-shaped strings", () => Sync(() =>
        {
            foreach (string input in new[] { checklist, checklist.Replace("/Date(0)/", "\\/Date(0)\\/") })
            {
                var parsed = ReadinessPayloadCodec.ParseChecklist(input); Equal("/Date(0)/", parsed.ChecklistId);
                Equal("/Date(0)/", parsed.Requirements[0].Source); Equal("墙", parsed.Cases[0].CaseId);
                Equal(false, parsed.Requirements[0].FireRatingRequired);
            }
        }));
        await Test("Actual net48 checklist codec normalizes syntax and schema failures", () => Sync(() =>
        {
            foreach (string input in new[] { "null", "{}", checklist + "{}", checklist.Replace("sample.rvt", "bad\nname"), checklist.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":2"), checklist.Replace("\"CodeRequired\":true,", "") })
                Equal("ChecklistInvalid", Throws<BridgeException>(() => ReadinessPayloadCodec.ParseChecklist(input)).Error.Code);
        }));
        await Test("Actual net48 report codec preserves full field names and false completeness", () => Sync(() =>
        {
            var report = new InformationReport { CoverageComplete = false, Evaluation = new Evaluation { Complete = false }, Level = "/Date(0)/", Units = "instance counts", ModifiedBefore = false, ModifiedAfter = false };
            report.ScopeIssues.Add(new ScopeIssue { UniqueId = "墙", CaseId = "C1", Reason = "Element not found", Status = "Unexamined" });
            string json = ReadinessPayloadCodec.SerializeReport(report);
            Check(json.Contains("\"CoverageComplete\":false") && json.Contains("\"Complete\":false"), "Flags renamed or dropped");
            var parsed = new JavaScriptSerializer().Deserialize<InformationReport>(json);
            Equal(report.Level, parsed.Level); Equal("墙", parsed.ScopeIssues[0].UniqueId); Equal("Element not found", parsed.ScopeIssues[0].Reason);
            Equal(report.Units, parsed.Units); Equal(false, parsed.ModifiedAfter);
        }));
        await Test("Actual net48 oversized report fails without truncation", () => Sync(() =>
        {
            var report = new InformationReport { Level = new string('x', 4 * 1024 * 1024) };
            Equal("ResponseTooLarge", Throws<BridgeException>(() => ReadinessPayloadCodec.SerializeReport(report)).Error.Code);
        }));
#else
        await Task.CompletedTask; // The production report/core remains net48-only.
#endif
    }
}
