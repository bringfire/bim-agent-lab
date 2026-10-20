using System;
using System.Collections.Generic;
using System.Linq;
using BimAgentLab.Readiness.Core;

internal static class Program
{
    private static int failures;
    private static int total;
    private static void Main()
    {
        Run("blank required rating affects every instance of its type", () => {
            var o = Observation("Walls", 3); o.Values["FireRating"].Value = "";
            var result = Evaluate(o, Requirement("Walls", true));
            Equal(1, result.FindingCount); Equal(3, result.AffectedElements.Count);
            Equal("Missing", result.Types[0].Fields.Single(f => f.Field == "FireRating").Status);
        });
        Run("whitespace is missing required code", () => {
            var o = Observation("Walls", 1); o.Values["Code"].Value = " \t ";
            Equal(1, Evaluate(o, Requirement("Walls")).FindingCount);
        });
        Run("explicit non-rated control passes with blank rating and assembly", () => {
            var o = Observation("Walls", 3); o.Values["FireRating"].Value = ""; o.Values["AssemblyReference"].Value = "";
            var r = Evaluate(o, Requirement("Walls")); Equal(0, r.FindingCount); Equal("Pass", r.Types[0].Status);
            Equal(2, r.Types[0].Fields.Count(f => f.Status == "NotRequired"));
        });
        Run("two missing fields do not double count affected instances", () => {
            var o = Observation("Walls", 3); o.Values["Code"].Value = ""; o.Values["FireRating"].Value = "";
            var r = Evaluate(o, Requirement("Walls", true)); Equal(2, r.FindingCount); Equal(3, r.AffectedElements.Count);
        });
        Run("missing ceiling code affects two ceilings", () => {
            var o = Observation("Ceilings", 2); o.Values["Code"].Value = null;
            var r = Evaluate(o, Requirement("Ceilings")); Equal(1, r.FindingCount); Equal(2, r.AffectedElements.Count);
        });
        Run("missing parameter differs from blank value", () => {
            var o = Observation("Walls", 1); o.Values.Remove("Code");
            var f = Evaluate(o, Requirement("Walls")).Types[0].Fields.Single(x => x.Field == "Code");
            Equal("Missing", f.Status); Equal("ParameterAbsent", f.Reason);
        });
        Run("unsupported storage cannot pass", () => {
            var o = Observation("Walls", 1); o.Values["Code"].State = "Unsupported";
            var r = Evaluate(o, Requirement("Walls")); Equal(false, r.Complete); Equal(1, r.UnexaminedElements.Count); Equal(0, r.FindingCount);
        });
        Run("unknown type requirements remain unexamined", () => {
            var r = TypeInformation.Evaluate(new[] { Observation("Walls", 3) }, new TypeRequirement[0]);
            Equal(false, r.Complete); Equal(3, r.UnexaminedElements.Count); Equal("Unexamined", r.Types[0].Status);
        });
        Run("category mismatch cannot borrow a different rule", () => {
            var r = Evaluate(Observation("Ceilings", 2), Requirement("Walls")); Equal(false, r.Complete); Equal(2, r.UnexaminedElements.Count);
        });
        Run("missing requirement flags fail validation", () => {
            var q = Requirement("Walls"); q.CodeRequired = null;
            Throws(() => Evaluate(Observation("Walls", 1), q));
        });
        Run("duplicate rule identity fails validation", () => {
            Throws(() => TypeInformation.Evaluate(new[] { Observation("Walls", 1) }, new[] { Requirement("Walls"), Requirement("Walls") }));
        });
        Run("duplicate element identity fails validation", () => {
            var o = Observation("Walls", 1); o.Elements.Add(o.Elements[0]); Throws(() => Evaluate(o, Requirement("Walls")));
        });
        Run("complete values preserve raw evidence and source", () => {
            var r = Evaluate(Observation("Walls", 1), Requirement("Walls", true));
            Equal(true, r.Complete); Equal(0, r.FindingCount); Equal("Pass", r.Types[0].Status);
            var f = r.Types[0].Fields.Single(x => x.Field == "FireRating"); Equal("60 min", f.ObservedValue); Equal("test checklist", f.RequirementSource);
        });
        Run("six complete types cover 16 instances with no findings", () => CheckDemonstration(0, 0, 0));
        Run("six types with two wall gaps affect six walls", () => CheckDemonstration(1, 2, 6));
        Run("ceiling negative control adds one finding and two affected ceilings", () => CheckDemonstration(2, 3, 8));
        Run("unsupported type preserves other findings and marks coverage incomplete", () => {
            var observations = Demonstration(2); observations[5].Values["Code"].State = "Unsupported";
            var r = TypeInformation.Evaluate(observations, DemonstrationRequirements(observations));
            Equal(false, r.Complete); Equal(3, r.FindingCount); Equal(8, r.AffectedElements.Count); Equal(2, r.UnexaminedElements.Count);
        });
        Run("one instance cannot belong to two observed types", () => {
            var observations = Demonstration(0); observations[1].Elements[0].UniqueId = observations[0].Elements[0].UniqueId;
            Throws(() => TypeInformation.Evaluate(observations, DemonstrationRequirements(observations)));
        });
        Console.WriteLine($"{total - failures}/{total} passed; {failures} failed.");
        Environment.ExitCode = failures == 0 ? 0 : 1;
    }
    private static void CheckDemonstration(int scenario, int findings, int affected)
    {
        var observations = Demonstration(scenario);
        var r = TypeInformation.Evaluate(observations, DemonstrationRequirements(observations));
        Equal(true, r.Complete); Equal(6, r.Types.Count); Equal(16, r.Types.Sum(t => t.Elements.Count));
        Equal(findings, r.FindingCount); Equal(affected, r.AffectedElements.Count);
        Equal(affected, r.AffectedElements.Select(e => e.UniqueId).Distinct().Count());
        var expected = scenario == 0 ? new string[0] : scenario == 1 ? new[] { "type-1", "type-3" } : new[] { "type-1", "type-3", "type-4" };
        Equal(string.Join(",", expected), string.Join(",", r.Types.Where(t => t.Status == "NeedsReview").Select(t => t.TypeUniqueId)));
    }
    private static List<TypeObservation> Demonstration(int scenario)
    {
        var observations = Enumerable.Range(0, 6).Select(i => {
            var o = Observation(i < 4 ? "Walls" : "Ceilings", i < 4 ? 3 : 2);
            o.UniqueId = "type-" + i;
            foreach (var e in o.Elements) { e.UniqueId = o.UniqueId + ":" + e.UniqueId; e.CaseId = e.UniqueId; }
            if (i >= 2) { o.Values["FireRating"].Value = ""; o.Values["AssemblyReference"].Value = ""; }
            return o;
        }).ToList();
        if (scenario >= 1) { observations[1].Values["FireRating"].Value = ""; observations[3].Values["Code"].Value = ""; }
        if (scenario >= 2) observations[4].Values["Code"].Value = "";
        return observations;
    }
    private static List<TypeRequirement> DemonstrationRequirements(IList<TypeObservation> observations) =>
        observations.Select((o, i) => { var q = Requirement(o.Category, i < 2); q.TypeUniqueId = o.UniqueId; return q; }).ToList();
    private static TypeObservation Observation(string category, int count)
    {
        return new TypeObservation {
            UniqueId = "type-1", Name = "Controlled type", Category = category,
            Elements = Enumerable.Range(1, count).Select(n => new ElementReference { UniqueId = "element-" + n, ElementId = n.ToString(), CaseId = "case-" + n }).ToList(),
            Values = new Dictionary<string, ParameterObservation> {
                ["Code"] = new ParameterObservation { State = "Present", Value = "A", ParameterId = "type-mark" },
                ["FireRating"] = new ParameterObservation { State = "Present", Value = "60 min", ParameterId = "fire-rating" },
                ["AssemblyReference"] = new ParameterObservation { State = "Present", Value = "DEMO-ASM-01", ParameterId = "assembly-guid" }
            }
        };
    }
    private static TypeRequirement Requirement(string category, bool rated = false) => new TypeRequirement {
        TypeUniqueId = "type-1", Category = category, Source = "test checklist", CodeRequired = true,
        FireRatingRequired = rated, AssemblyReferenceRequired = rated
    };
    private static Evaluation Evaluate(TypeObservation o, TypeRequirement q) => TypeInformation.Evaluate(new[] { o }, new[] { q });
    private static void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}; got {actual}."); }
    private static void Throws(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Expected ArgumentException."); }
    private static void Run(string name, Action action) {
        total++; try { action(); Console.WriteLine("PASS " + name); } catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
}
