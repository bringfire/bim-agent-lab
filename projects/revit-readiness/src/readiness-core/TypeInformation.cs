using System;
using System.Collections.Generic;
using System.Linq;

namespace BimAgentLab.Readiness.Core
{
    public sealed class ElementReference
    {
        public string UniqueId { get; set; }
        public string ElementId { get; set; }
        public string CaseId { get; set; }
    }
    public sealed class ParameterObservation
    {
        public string State { get; set; } // Present, Absent, Unsupported
        public string Value { get; set; }
        public string ParameterId { get; set; }
    }
    public sealed class TypeObservation
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<ElementReference> Elements { get; set; } = new List<ElementReference>();
        public Dictionary<string, ParameterObservation> Values { get; set; } = new Dictionary<string, ParameterObservation>();
    }
    public sealed class TypeRequirement
    {
        public string TypeUniqueId { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public bool? CodeRequired { get; set; }
        public bool? FireRatingRequired { get; set; }
        public bool? AssemblyReferenceRequired { get; set; }
    }
    public sealed class FieldResult
    {
        public string RuleId { get; set; }
        public string Field { get; set; }
        public bool? Required { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string ObservedValue { get; set; }
        public string ParameterId { get; set; }
        public string RequirementSource { get; set; }
    }
    public sealed class TypeResult
    {
        public string TypeUniqueId { get; set; }
        public string TypeName { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public List<ElementReference> Elements { get; set; }
        public List<FieldResult> Fields { get; set; } = new List<FieldResult>();
    }
    public sealed class Evaluation
    {
        public List<TypeResult> Types { get; set; } = new List<TypeResult>();
        public int FindingCount { get; set; }
        public List<ElementReference> AffectedElements { get; set; } = new List<ElementReference>();
        public List<ElementReference> UnexaminedElements { get; set; } = new List<ElementReference>();
        public bool Complete { get; set; }
    }
    public static class TypeInformation
    {
        public static Evaluation Evaluate(IList<TypeObservation> observations, IList<TypeRequirement> requirements)
        {
            if (observations == null || requirements == null) throw new ArgumentException("Observations and requirements are required.");
            var rules = new Dictionary<string, TypeRequirement>(StringComparer.Ordinal);
            foreach (var q in requirements)
            {
                if (q == null || string.IsNullOrWhiteSpace(q.TypeUniqueId) || string.IsNullOrWhiteSpace(q.Source) ||
                    (q.Category != "Walls" && q.Category != "Ceilings") || !q.CodeRequired.HasValue ||
                    !q.FireRatingRequired.HasValue || !q.AssemblyReferenceRequired.HasValue || rules.ContainsKey(q.TypeUniqueId))
                    throw new ArgumentException("Each type needs one identified, sourced rule with explicit requirement flags.");
                rules.Add(q.TypeUniqueId, q);
            }
            var result = new Evaluation { Complete = true };
            var typeIds = new HashSet<string>(StringComparer.Ordinal);
            var elementIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var o in observations)
            {
                if (o == null || string.IsNullOrWhiteSpace(o.UniqueId) || !typeIds.Add(o.UniqueId) || o.Elements == null ||
                    o.Elements.Count == 0 || o.Values == null)
                    throw new ArgumentException("Each observed used type needs a unique identity, elements, and values.");
                foreach (var e in o.Elements)
                    if (e == null || string.IsNullOrWhiteSpace(e.UniqueId) || !elementIds.Add(e.UniqueId))
                        throw new ArgumentException("Element identities must be present and unique across the observation set.");
                rules.TryGetValue(o.UniqueId, out var q);
                if (q != null && q.Category != o.Category) q = null;
                var row = new TypeResult { TypeUniqueId = o.UniqueId, TypeName = o.Name, Category = o.Category, Elements = o.Elements };
                string[] fields = { "Code", "FireRating", "AssemblyReference" };
                bool?[] required = { q?.CodeRequired, q?.FireRatingRequired, q?.AssemblyReferenceRequired };
                for (int i = 0; i < fields.Length; i++)
                {
                    o.Values.TryGetValue(fields[i], out var value);
                    var f = new FieldResult { RuleId = "TYPE-INFO-" + fields[i], Field = fields[i], Required = required[i],
                        ObservedValue = value?.Value, ParameterId = value?.ParameterId, RequirementSource = q?.Source };
                    if (!required[i].HasValue) { f.Status = "Unexamined"; f.Reason = "NoMatchingRequirement"; }
                    else if (!required[i].Value) { f.Status = "NotRequired"; f.Reason = "ExplicitChecklistException"; }
                    else if (value == null || value.State == "Absent") { f.Status = "Missing"; f.Reason = "ParameterAbsent"; }
                    else if (value.State != "Present") { f.Status = "Unexamined"; f.Reason = "UnsupportedParameterStorage"; }
                    else if (string.IsNullOrWhiteSpace(value.Value)) { f.Status = "Missing"; f.Reason = "BlankValue"; }
                    else { f.Status = "Present"; f.Reason = "NonemptyText"; }
                    row.Fields.Add(f);
                }
                int missing = row.Fields.Count(f => f.Status == "Missing");
                bool unexamined = row.Fields.Any(f => f.Status == "Unexamined");
                row.Status = unexamined ? "Unexamined" : missing > 0 ? "NeedsReview" : "Pass";
                result.FindingCount += missing;
                if (missing > 0) result.AffectedElements.AddRange(o.Elements);
                if (unexamined) { result.UnexaminedElements.AddRange(o.Elements); result.Complete = false; }
                result.Types.Add(row);
            }
            return result;
        }
    }
}
