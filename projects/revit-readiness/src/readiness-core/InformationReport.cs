using System.Collections.Generic;

namespace BimAgentLab.Readiness.Core
{
    public sealed class ScopeIssue
    {
        public string CaseId { get; set; }
        public string UniqueId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
    public sealed class InformationReport
    {
        public string GeneratedUtc { get; set; }
        public string DocumentPath { get; set; }
        public string ProjectUniqueId { get; set; }
        public string RevitBuild { get; set; }
        public string ChecklistId { get; set; }
        public string ChecklistPath { get; set; }
        public string Scope { get; set; }
        public string Level { get; set; }
        public string Phase { get; set; }
        public string Units { get; set; }
        public int RequestedInstances { get; set; }
        public int ExaminedInstances { get; set; }
        public int OtherHostWallCeilingInstances { get; set; }
        public int LinkInstancesExcluded { get; set; }
        public List<ScopeIssue> ScopeIssues { get; set; } = new List<ScopeIssue>();
        public Evaluation Evaluation { get; set; }
        public bool CoverageComplete { get; set; }
        public bool ModifiedBefore { get; set; }
        public bool ModifiedAfter { get; set; }
    }
}
