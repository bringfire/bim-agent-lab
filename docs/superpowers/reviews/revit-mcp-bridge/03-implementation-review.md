# R3: Revit MCP implementation and acceptance review packet

**Independent R3 has returned for packet 0489c54708737488b1bc7ae0e4279960bbda759c: keep full acceptance open.** Read [original feedback and separate implementer dispositions](2026-10-22-r3-feedback.md) before preparing follow-up. R3-01 concerns missing Codex presentation evidence; NC1 recovery is partial and not independent closure. Original review scope/read set below remains applicable; no final acceptance or release approval is implied.

## Exact review target

- Implementation baseline: `ff8a4f359fd0a9ff9147cab366a9cb81683a98cc` (before Task 1).
- Implementation head: `627e227c8054feaf91539976b32dd09228c4e93f`.
- Packet head: the full SHA of the commit containing this completed packet, supplied by the author. Resolve it with `git rev-parse HEAD`; no self-referential or invented SHA is embedded here.
- Normative target: spec/plan **revision 3** at the implementation head, including the [author-directed compatibility amendment](../../specs/2026-10-20-desktop-compatibility-amendment.md). R1/R2 external closures apply to revision 2; the amendment is an explicit R3 input.
- Installed normal host `9a93f6d29bf3c46972701db53bfaee598ec4fd0c`; installed server `d88de673996520d86c3502f357880865d1037fb9`; latest diagnostic source `32c44eccc3b544eeed64aff5c9ac963d8a137105`. **Current source differs from the live normal build.**

Use the [A1–A9 ledger](../../../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) as the claim index. It identifies passed scopes, known defects and unverified observations. It does not close Tasks 7–9 or replace source review.

## Required read set

1. [Spec](../../specs/2026-10-20-revit-mcp-bridge-design.md), [plan](../../plans/2026-10-20-revit-mcp-bridge.md), amendment above, [R1 follow-up](2026-10-20-r1-follow-up.md), [R2 follow-up](2026-10-20-r2-follow-up.md), [author plan approval](2026-10-20-plan-approval.md).
2. [Execution and waiting model](../../../explanation/bridge-execution-and-waiting.md), current `projects/revit-readiness/src/`, references/locks, installer/setup/recovery scripts and tests. Include production validation before SDK conversion, not just schemas.
3. [Ledger](../../../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md), [fresh checks/replay](../../../../projects/revit-readiness/docs/verification/r3-evidence/README.md), [source inventory](../../../../projects/revit-readiness/docs/verification/r3-evidence/source-inventory.json), [installed-source delta](../../../../projects/revit-readiness/docs/verification/r3-evidence/installed-source-delta.txt).
4. [Dependency/provenance record](../../../research/mcp-sdk-dependencies.md), [notices](../../../../projects/revit-readiness/third-party/mcp/README.md), attributed `CancellableInputStream.cs` and upstream pins. Distinguish adapted C# 2.2 code, reused lab computation, original adapter code and unused research precedents.
5. [Five-case live matrix](../../../../projects/revit-readiness/docs/verification/mcp-readiness.md), independent oracle/comparators, raw traffic, human answers, file hashes and [setup guide](../../../../projects/revit-readiness/docs/mcp-bridge-setup.md). Optional Claude Code/SDK harness results do not replace either required desktop.
6. [Initial live lifecycle](../../../../projects/revit-readiness/docs/verification/2026-10-21-live-installation.md), host interruption/Stop records linked by the ledger, [actual queued shutdown](../../../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md), [three-layer recovery/reinstall and fresh read](../../../../projects/revit-readiness/docs/verification/2026-10-22-shared-recovery-execution.md).
7. [SDK cancellation diagnosis](../../../../projects/revit-readiness/docs/verification/2026-10-22-sdk-cancellation-diagnosis.md), [author disposition](2026-10-22-cancellation-disposition.md), failing propagation probe and successful explicit-notification/EOF controls.
8. Scoped independent closures: [Task 2](2026-10-21-task-2-follow-up.md), [Task 3](2026-10-21-task-3-closure.md), [Task 4](2026-10-21-task-4-closure.md), [Task 5](2026-10-21-task-5-closure.md), [Task 6](2026-10-21-task-6-closure.md). Original findings and failed reproductions remain linked. These are not full R3 acceptance.
9. [Participant starting guide](../../../start-here.md), [participant review story](../../../tutorials/15-prepare-an-evidence-based-review.md), [agent handoff](../../../how-to/agent-handoff.md), [technical consolidation procedure](../../../how-to/prepare-r3-evidence.md), journal and current status.

## Reproduce the scope

From the repository root, without altering the checkout:

```powershell
git status --short --branch
git rev-parse HEAD
git diff --stat ff8a4f359fd0a9ff9147cab366a9cb81683a98cc 627e227c8054feaf91539976b32dd09228c4e93f
git diff ff8a4f359fd0a9ff9147cab366a9cb81683a98cc 627e227c8054feaf91539976b32dd09228c4e93f -- projects/revit-readiness/src projects/revit-readiness/scripts projects/revit-readiness/tests
python projects/revit-readiness/docs/verification/r3-evidence/replay.py
```

Fresh-suite commands and prerequisites are in the linked check record. `-Suite All` includes disposable installation/recovery tests and requires **Revit closed**. It is not an instruction to stop applications or deploy. Builds require Windows/.NET/Revit API prerequisites. Textual evidence replay needs Python, not a running Revit.

Supply the complete committed Git tree/history, not a patch without its baseline. Proprietary SDK binaries, RVT samples, caches, private configs and durable backup bytes are ignored. Sanitized reports, identities, traffic, assertions and hashes are retained. A reviewer without the licensed host cannot independently rerun live tests and must state that limit. Machine-specific recovery scripts/receipts are audit evidence, not portable installation instructions.

## Questions requiring a disposition

- Assess every A1–A9 claim and **O1–O7 in the ledger**: source/live-build mismatch, SDK defect, native scheduling limits, access/instance scope, missing Codex UI metadata, retained intermittent test/evidence failures, review/distribution limits.
- State what can close for the **tested installed candidate**, and what live checks a build of 627e227 still needs. Do not waive Task 9's affected-live-test requirement because most differences are diagnostic.
- Check multi-version compatibility without wire rewriting, weakening coverage/lifetimes/errors or dropping strict July regression.
- Assess the author-accepted explicit cancellation approach within its demonstrated scope. A defect-characterization pass is not a fix. Do not infer desktop Stop-button behavior.
- Check that teaching material distinguishes real prompts, suggested prompts, self-checks, independent review and author acceptance.

Trace these operations before reviewing isolated methods:

1. Shared project UniqueId, copied file or close/reopen: binding must reject changed lifetime/path.
2. Cancellation/expiry during preparation/evaluation: original lifetime reaches the final read guard, prevents new reads, rejects late success and retains resources until their last user finishes.
3. Shutdown with idle-open stdin, partial pipes, held callbacks or work in progress: network cleanup progresses independently, budgets stay fixed, retirement stays bounded.
4. Zero findings with incomplete coverage: exact unexamined identities, distinct false flags and honest client presentation survive the full pipeline.
5. Failure after first installation mutation, deletion, fixture update or interrupted restore: durable pre-mutation recovery restores owned state and preserves foreign edits.

These are questions, not new requirements or invented findings. Support findings with concrete source/scenarios/evidence; separate unavailable observations. If multiple findings share a missing concept, explain that reasoning gap and request a correction across the relevant model, not more example-specific patches.

## Suggested handoff prompt

> Independently review this Revit MCP bridge at the supplied full packet SHA, implementation head 627e227c8054feaf91539976b32dd09228c4e93f and baseline ff8a4f359fd0a9ff9147cab366a9cb81683a98cc. Follow this packet's read set, approved spec/plan revision 3 and compatibility amendment. Inspect source and retained evidence against A1–A9, address O1–O7 and trace the five operations. This handoff has open items; it does not assert full acceptance. Distinguish checks you run from supplied captures. Return prioritized findings, locations, failure scenarios, minimal corrections, verification and limitations. State what can close and what cannot. Do not change requirements or edit the repository during review.

Use the [external review form](../../../../templates/external-review.md). Record reviewer identity/type, date, exact commits, read set and checks actually performed. Preserve the original returned report, then record author accept/reject/defer dispositions separately. An independent recommendation is not author approval. The original R3 report is retained separately; no independent follow-up closure or author approval is implied by the implementer response.
