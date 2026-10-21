# Run specification, plan, and implementation reviews

For agents and technical readers. The [participant story](../tutorials/06-direct-design-and-external-review.md) explains the human's direction. Follow root/project `AGENTS.md`. The worked example starts from `7d3ae71` on `docs/revit-mcp-spec`; revision 1 was committed as `b56c0dc`. [R1 feedback](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md) now informs [spec revision 2](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) and the completed [follow-up/author approval](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md). Revision 2 was approved at `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44`. The [implementation plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) is now ready for external R2; author plan review/execution choice and implementation remain pending.

## 1. Identify the authorized task and current stage

The original task is still to implement the minimal bridge, verify three cases through MCP/Revit, and record both teaching layers. The author subsequently requested superpowers brainstorming with an explicit spec, implementation plan, and external review sessions. Resume at the earliest unfinished stage; do not restart the earlier research or substitute process documents for eventual runtime acceptance.

The installed skill inspected for this task was superpowers **6.4.2**, `skills/brainstorming/SKILL.md`. Authoring-machine path:

```text
C:\Users\bring\.codex\plugins\cache\openai-curated-remote\superpowers\6.4.2\skills\brainstorming\SKILL.md
SHA-256: a32d2255354775aa124855aa7100cf276bea096fff4ebb3a0edf57be216e6c72
```

Its architectural gate says: **“the human partner reviews and approves the written spec, then reviews the written implementation plan and selects its execution method.”** It also says written-spec approval permits invoking writing-plans. The author explicitly requested this protocol, so the stage boundary is intentional. This path is local provenance, not a portable installation prerequisite: future agents should discover and read their installed skill version and disclose changed requirements. This guide preserves the workflow without vendoring third-party skill source.

Do not ask again about decisions already made. The author has selected the custom bridge, C# SDK 2.2.0, and protocol 2026-07-28. Reflect the purpose and constraints, identify agent-proposed details, and write the artifact necessary for the next review. Read-only exploration and documentation can continue while a review is pending; product scaffolding, package installation, deployment, and implementation wait for the requested gates.

## 2. Ground the spec in source and evidence

Inspect Git status/branch/history and applicable instructions before editing. Read the current source and its existing tests, not only research summaries. For this example the relevant boundaries are:

| Input | Why it matters |
|---|---|
| [Research comparison](../research/revit-mcp-bridge-comparison.md) and [dated evidence](../research/evidence/mcp-version-observations-2026-10-20.json) | Proposed architecture, exact SDK/protocol observations, third-party pins, and license/provenance questions |
| [Reader](../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs) and [checklist](../../projects/revit-readiness/src/readiness-core/Checklist.cs) | Actual scope/identity checks and Revit access that we will reuse |
| [Report](../../projects/revit-readiness/src/readiness-core/InformationReport.cs) and [core project](../../projects/revit-readiness/src/readiness-core/BimAgentLab.Readiness.Core.csproj) | Full evidence shape and net48/System.Web.Extensions boundary |
| [Manual command](../../projects/revit-readiness/src/revit-addin/RequiredInformationCommand.cs) and [installer](../../projects/revit-readiness/scripts/Install-Readiness.ps1) | Dialog/export side effects, existing deployment identities, and shared-core synchronization |
| [Independent answers](../../projects/revit-readiness/samples/expected-results/required-information.md) and [comparator](../../projects/revit-readiness/tests/verify_information_evidence.py) | Literal sample values and the existing comparison's actual coverage |

Compare two or three approaches, then preserve the selected direction and tradeoffs. Write a focused contract: purpose, scope, data flow, components, external interfaces, states/errors, bounds, deployment, provenance, and acceptance. Mark proposed values as design choices and observed behavior as dated evidence. The spec is not a list of implementation commands. Avoid pretending a runtime uncertainty has been solved by choosing an architecture.

For the bridge, use the official SDK for MCP, original code for the private Revit connection, and our existing reader/evaluator. Do not adopt the precedent's old TypeScript SDK or dispatch helper. A final implementation provenance audit must substantiate those intentions. Refresh official package/registry/migration evidence when adopting dependencies; current-version research does not mean using floating package versions.

## 3. Self-review and checkpoint the artifact

Perform the skill's placeholder, consistency, scope, and ambiguity checks. Fix substantive gaps before handoff. Keep a short [self-review record](../superpowers/reviews/revit-mcp-bridge/00-authoring-self-review.md) that labels its author and scope. It is not an independent reviewer report. Verify links and expected numbers against the source/oracle; do not run application tests for a documentation-only change and call that new bridge acceptance.

Review the complete diff, including new files. Stage only the intended documents and commit a coherent checkpoint. Use the resulting full Git hash in the actual reviewer request. A moving branch or a filename without revision is not a reproducible review target. Keep working models, ignored clones, binaries, local client configuration, and credentials out of the packet.

## 4. Conduct R1: specification review

Hand over [R1](../superpowers/reviews/revit-mcp-bridge/01-spec-review.md) and its read set at the pinned revision. Preparing a packet does not authorize an unsolicited email, external upload, or message to an unnamed reviewer. The author can hand it to their chosen person/agent; if asked to transmit it, establish the recipient/destination and authorized contents first.

Use [the review form](../../templates/external-review.md). Record reviewer identity/type, date, exact revisions, inspected inputs, tests actually run, findings, and limitations. Return findings as evidence-backed issues rather than an unstructured rewrite. The author/implementer records accept/reject/defer decisions separately and links corrections and verification. Preserve original feedback. A rejected finding needs a reason supported by evidence; an unresolved blocker prevents the relevant stage from passing.

Revise and self-check the spec as needed, then obtain the author's approval of the actual written revision. A review recommendation is not author approval. The author returned an external Codex review of revision 1 with three findings and a revise recommendation. Its wording is preserved separately from the drafting agent's dispositions; do not present those dispositions as reviewer closure.

### Worked R1 response

The agent used the installed superpowers receiving-code-review skill to read, understand, and verify the feedback against source before editing. All three findings were accepted at the spec level. The author chose **Codex desktop and Claude Desktop** after clarification of the word client. Section 8.1 now pins the observed initial Windows package versions, intended stdio/instance selection, and a strict protocol rule: 2026-07-28 is required in each application; fallback cannot pass A8. Actual configurations and traces remain future evidence.

For R1-02, inspect `Checklist.Validate`, `RequiredInformationReader.Read`, and `TypeInformation.Evaluate`. Removing one used type's requirement remains schema-valid and reaches evaluation. An unresolved declared element enters `ScopeIssues` and leaves the evaluator complete for the observed subset; a missing requirement enters `Evaluation.UnexaminedElements` and makes evaluator completeness false. Section 8.2 states exact NC1/NC2 results and independent identities. Prepare isolated sidecars before fresh context acquisition; otherwise the expected result is a context-change failure, not the coverage behavior under test. Keep the complete-case comparator's assertions and add separate control comparisons. Each desktop client must present incomplete checking as incomplete even with zero findings.

For R1-03, section 6 starts a single five-second monotonic timer at pipe acceptance for the complete header/body. No-byte, partial-header, partial-body, and trickle tests fill all eight slots, prove cleanup, and admit a later valid request. The validated remaining deadline then caps processing; receiving a frame does not reset the end-to-end lifetime. Section 8.3 distinguishes the five-second production limit from a one-second measured test scheduling allowance.

Retain the [feedback/dispositions and client baseline evidence](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md). The source checks and installed-version observations do not establish runtime behavior. Ask the reviewer to assess the three corrections at the new full Git revision and record that response separately. The two-hour seminar need not reproduce every technical check; the participant lesson explains how the review changed the work.

## 5. Write and review the implementation plan

After written-spec approval, invoke and read the installed **superpowers writing-plans** skill. Use its task-level format and review/execution handoff. Save the actual plan in `docs/superpowers/plans/`, linking the approved spec and R1 disposition record.

An implementation plan must identify exact new/changed files, relevant APIs, dependency pins/locks/notices, behavioral tests with meaningful first failures, implementation order, and verification commands/expected outcomes. Include isolated tests for wire framing/queue/context lifetime, SDK protocol checks, actual Revit lifecycle checks, three independent live sample comparisons, and actual seminar-client evidence. Specify install/rollback and current shared-core/exporter interactions. Map tasks to spec A1-A9. No invented command or unimplemented script should be presented as already runnable.

Attach the real plan and approved spec to [R2](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md), then preserve findings and dispositions. Ask the author to review the revised written plan and select execution method as required by the skill. Do not silently select external reviewer identity or misrepresent a drafting subagent as the requested external review.

### Current plan and early compatibility checkpoint

The author explicitly said “Approve revision 2 and write the plan.” The drafting agent used installed superpowers 6.4.2 `skills/writing-plans/SKILL.md`, mapped source ownership/interfaces, wrote nine tasks with behavioral tests and commands, and self-reviewed A1–A9 coverage. The [plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) and [concrete R2 packet](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md) are the current handoff; no implementation has started.

Task 1 first refreshes registry/license/advisory evidence and resolves the selected exact package pin. Inspected SDK source exposes public `McpServerOptions.ProtocolVersion` and `McpClientOptions.ProtocolVersion`; require the dated revision explicitly. The SDK host and separate synthetic probe establish discovery, structured success/failure, clean stdio and actual protocol behavior. Opt-in passive byte capture supplies wire evidence without a proxy rewriting the exchange. Both real desktop applications must pass CP0 before dependent pipe/Revit work; a standalone harness or fallback is insufficient. Preserve configuration ownership and disclose actual settings changes. Full A8 acceptance remains Task 8, with every live sample/control in both applications.

The plan separates plain shared DTO/runtime code from Revit API access and SDK consumption, uses explicit private timestamp encoding across serializers, and retains the original independent comparator's complete-case requirements. Tasks name setup/rollback, bounded connection/queue/context tests, real-host lifecycle checks, separate control assertions, and provenance/teaching records. New paths/commands in the plan are future deliverables, not installed capabilities. The recommendation is Native execution because the interfaces are closely coupled; the author still selects the method after plan review. Do not infer that choice from spec approval.

## 6. Implement, verify, and conduct R3

Execute the approved plan with meaningful behavioral tests and scoped Git checkpoints. Use actual builds and installed binaries for host verification. SDK 2.2.0 in a package file is not proof of protocol 2026-07-28 on the wire. An SDK client harness is not proof that the intended seminar application works. Record both distinctly.

The three live sample outcomes must remain 0/0, 2/6, and 3/8 findings/distinct affected instances, with exact identities, rule sources, scope, exclusions, and completeness. Add NC1/NC2 from revision 2 as distinct incomplete-coverage controls. Run A8 through both required desktop applications using protocol 2026-07-28. Extend independent comparison coverage where required; do not generate expected values with production code. Inspect the Revit schedules, retain fresh model hashes and modified-state evidence, and keep failure-case preparation separate from normal queries.

Provide the exact diff and evidence listed in [R3](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md). Review source provenance against the actual package graph and all copied/adapted code. Address each accepted finding, rerun affected checks, retain limitations, and update the review log. Source review, automated tests, real-host checks, and independent BIM comparisons establish different things.

## 7. Maintain both teaching layers and honest status

Update the participant story with actual prompts, what the person decided, delegated choices, review findings, corrections, and assessable evidence. Keep commands, technical decisions, dependencies, and reproducibility in the linked agent guide. Label suggested prompts and planned sessions. Link both to the same real review records, acceptance evidence, and commits.

Update current status, root/project agent guidance, source entry points, learning path, and journal at each real milestone. Preserve earlier journal entries as dated history while correcting current status. Never mark implementation ready because the spec or packet exists. Revision 2 is externally closed and author-approved; the written plan is ready for R2. Author plan review/execution selection, implementation, and end-to-end acceptance remain subsequent work.
