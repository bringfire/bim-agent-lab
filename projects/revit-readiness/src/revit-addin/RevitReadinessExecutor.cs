using System;
using System.Text;
using Autodesk.Revit.UI;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.RevitReadiness
{
    internal sealed class RevitReadinessExecutor
    {
        readonly BridgeSessionDescriptor descriptor;
        readonly HostBridgeSession session;
        readonly RevitContextCatalog catalog;
        internal RevitReadinessExecutor(BridgeSessionDescriptor descriptor, HostBridgeSession session, RevitContextCatalog catalog)
        { this.descriptor = descriptor; this.session = session; this.catalog = catalog; }
        internal BridgeResponse Execute(UIApplication application, BridgeExecutionContext context)
        {
            if (context.Request.Operation == BridgeLimits.GetContext)
                return ReadinessExecutionFlow.Run(context, () => catalog.Capture(application), prepared => prepared,
                    prepared => ReadinessExecutionFlow.IssueContext(context, session.Contexts, prepared.Binding, session.ResponseFor(context), lease =>
                        Response(context, prepared.Binding.ChecklistSha256, Encoding.UTF8.GetString(BridgeJson.Serialize(new ReadinessContextResult {
                            RevitPid = descriptor.RevitPid, RevitBuild = prepared.RevitBuild,
                            DocumentLifetimeId = prepared.Binding.DocumentLifetimeId, DocumentPath = prepared.Binding.DocumentPath,
                            ProjectUniqueId = prepared.Binding.ProjectUniqueId, ChecklistId = prepared.Binding.ChecklistId,
                            ChecklistPath = prepared.Binding.ChecklistPath, Level = prepared.Level, Phase = prepared.Phase,
                            Scope = "Explicit checklist element UniqueIds; host basic walls and ceilings on the declared level, New in the declared phase. Types are checked once; links and other elements excluded. No view/selection filter or result cap.",
                            ContextToken = lease.Token, ExpiresUtc = BridgeJson.FormatUtc(lease.ExpiresUtc)
                        })))));
            if (context.Request.Operation != BridgeLimits.CheckInformation)
                throw RevitContextCatalog.Failure("UnsupportedOperation", "Use a named readiness operation.");
            RevitContextSnapshot snapshot = null;
            return ReadinessExecutionFlow.Run(context, () =>
                {
                    snapshot = catalog.Capture(application);
                    session.Contexts.Validate(context.Request.ContextToken, snapshot.Binding);
                    return snapshot;
                },
                prepared => RequiredInformationReader.Read(prepared.Document, prepared.Checklist, prepared.Binding.ChecklistPath),
                report => Response(context, snapshot.Binding.ChecklistSha256, ReadinessPayloadCodec.SerializeReport(report)));
        }
        BridgeResponse Response(BridgeExecutionContext context, string hash, string result) => new BridgeResponse
        { ContractVersion = BridgeLimits.ContractVersion, RequestId = context.Request.RequestId,
            BridgeSessionId = descriptor.BridgeSessionId, ChecklistSha256 = hash, ResultJson = result };
    }
}
