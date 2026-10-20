# R1: external specification review packet

Ready for handoff after the design checkpoint is committed. Not yet sent or reviewed. Use the full Git revision from the [handoff procedure](README.md), not just a moving branch name.

## Reviewer brief

We are extending a teaching repo for BIM professionals with a minimal read-only Revit MCP bridge. Existing Revit 2024 code checks missing required wall/ceiling type text and has three independently verified sample variants. We want official C# MCP SDK 2.2.0 and protocol 2026-07-28, original narrow Revit integration, and transparent provenance. The implementation has not started. Review the design before a detailed implementation plan is written.

Read this packet with:

- [Spec revision 1](../../specs/2026-10-20-revit-mcp-bridge-design.md).
- [Root agent guidance](../../../../AGENTS.md), [project guidance](../../../../projects/revit-readiness/AGENTS.md), and [current status](../../../reference/development-status.md).
- [Bridge comparison](../../../research/revit-mcp-bridge-comparison.md), its [source/version evidence](../../../research/evidence/mcp-version-observations-2026-10-20.json), and [prior integration gates](../../../how-to/choose-and-verify-an-mcp-bridge.md).
- Existing [reader](../../../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs), [command](../../../../projects/revit-readiness/src/revit-addin/RequiredInformationCommand.cs), [report](../../../../projects/revit-readiness/src/readiness-core/InformationReport.cs), and [checklist](../../../../projects/revit-readiness/src/readiness-core/Checklist.cs).
- Independent [expected answers](../../../../projects/revit-readiness/samples/expected-results/required-information.md), [verification script](../../../../projects/revit-readiness/tests/verify_information_evidence.py), and [prior host evidence](../../../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md).

## Suggested handoff prompt

The following is a reusable prompt, not a review already performed:

> Independently review the Revit readiness bridge spec in this packet at the supplied full Git revision. You did not author it. Read the spec, baseline source, research, and independent expected results. Find concrete defects, missing decisions, contradictions, unsupported compatibility assumptions, or unnecessary complexity that could prevent its acceptance. Do not implement or rewrite the project. Focus on document identity at execution, ExternalEvent lifecycle and read-only behavior, local pipe restrictions/framing, bounded queues and cancellation, SDK/protocol distinctions, source provenance, and proof across all three samples. Separate blockers from suggestions. For each finding, cite the spec section/source, describe a realistic failure scenario and impact, recommend the smallest correction, and state how to verify it. State inputs you could not inspect and whether runtime claims were actually tested. Return findings in the supplied format and recommend proceed, revise, or insufficient evidence. Do not treat earlier research or your source review as a new runtime pass.

## Questions that need particular scrutiny

1. Can the proposed context identify the correct open document when copies share a project ID, a file is saved under another name, or Revit changes documents while work waits?
2. Does the proposed queue/ExternalEvent lifecycle avoid invalid API calls, stranded requests, stale success, and blocked UI shutdown? Are bounds proportionate to this small example?
3. Is the local pipe boundary enforceable on net48, including remote rejection and handle cleanup? What must be proved in Windows integration tests?
4. Does the report cross the runtime boundary without a second evaluator, stale exported JSON, or loss of rule/identity/coverage evidence?
5. Are official SDK integration, original adapter code, and precedent-only source use unambiguous? Is it clear that SDK 1.26.0 refers to the old TypeScript dependency?
6. Would acceptance distinguish SDK tests, modern protocol use, actual seminar-client compatibility, genuine Revit calls, and independent BIM results?
7. Does the spec contain enough decisions for a task-level plan while keeping physical conditions, quantities, editing, and report exports out of this increment?

Return the [review form](../../../../templates/external-review.md) with stage R1, reviewer identity/type, exact inspected revision, findings, limitations, and recommendation. The author/implementer will maintain a separate disposition record; a reviewer recommendation does not itself approve implementation.
