# Run specification, plan, and implementation reviews

For agents and technical readers. The [participant story](../tutorials/06-direct-design-and-external-review.md) explains the human's direction. Follow root/project `AGENTS.md`. The current worked example starts from `7d3ae71` on `docs/revit-mcp-spec` and produces a [draft spec](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) and [review packets](../superpowers/reviews/revit-mcp-bridge/README.md). The plan, external reviews, and implementation remain pending.

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

Revise and self-check the spec as needed, then obtain the author's approval of the actual written revision. A review recommendation is not author approval. The current R1 packet has not been sent and has no returned findings; do not create a fictional review to move on.

## 5. Write and review the implementation plan

After written-spec approval, invoke and read the installed **superpowers writing-plans** skill. Use its task-level format and review/execution handoff. Save the actual plan in `docs/superpowers/plans/`, linking the approved spec and R1 disposition record.

The future plan must identify exact new/changed files, relevant APIs, dependency pins/locks/notices, behavioral tests with meaningful first failures, implementation order, and verification commands/expected outcomes. Include isolated tests for wire framing/queue/context lifetime, SDK protocol checks, actual Revit lifecycle checks, three independent live sample comparisons, and actual seminar-client evidence. Specify install/rollback and current shared-core/exporter interactions. Map tasks to spec A1-A9. No invented command or unimplemented script should be presented as already runnable.

Attach the real plan and approved spec to [R2](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md), then preserve findings and dispositions. Ask the author to review the revised written plan and select execution method as required by the skill. Do not silently select external reviewer identity or misrepresent a drafting subagent as the requested external review.

## 6. Implement, verify, and conduct R3

Execute the approved plan with meaningful behavioral tests and scoped Git checkpoints. Use actual builds and installed binaries for host verification. SDK 2.2.0 in a package file is not proof of protocol 2026-07-28 on the wire. An SDK client harness is not proof that the intended seminar application works. Record both distinctly.

The three live sample outcomes must remain 0/0, 2/6, and 3/8 findings/distinct affected instances, with exact identities, rule sources, scope, exclusions, and completeness. Extend independent comparison coverage where required; do not generate expected values with production code. Inspect the Revit schedules, retain fresh model hashes and modified-state evidence, and keep failure-case preparation separate from normal queries.

Provide the exact diff and evidence listed in [R3](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md). Review source provenance against the actual package graph and all copied/adapted code. Address each accepted finding, rerun affected checks, retain limitations, and update the review log. Source review, automated tests, real-host checks, and independent BIM comparisons establish different things.

## 7. Maintain both teaching layers and honest status

Update the participant story with actual prompts, what the person decided, delegated choices, review findings, corrections, and assessable evidence. Keep commands, technical decisions, dependencies, and reproducibility in the linked agent guide. Label suggested prompts and planned sessions. Link both to the same real review records, acceptance evidence, and commits.

Update current status, root/project agent guidance, source entry points, learning path, and journal at each real milestone. Never mark implementation ready because the spec or packet exists. This checkpoint prepares R1; the approved plan, implementation, and end-to-end acceptance remain subsequent work.
