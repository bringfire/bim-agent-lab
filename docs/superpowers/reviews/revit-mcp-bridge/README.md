# Revit MCP bridge review sessions

**Task 2 follow-up:** [independent review and separate author dispositions](2026-10-21-task-2-review.md) record T2-01/02/03 on `eefb156`. All reproduced; corrections pass [fresh offline checks](../../../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md). [Independent follow-up](2026-10-21-task-2-follow-up.md) closes all three at `3cefe51`. Task 3 now has [Windows IPC evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport.md) and an [external P2 finding with correction/follow-up packet](2026-10-21-task-3-review.md). Corrections pass [fresh verification](../../../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md); independent follow-up remains pending. Neither focused checkpoint closes full R3.

**Current policy amendment:** the author selected [desktop-first compatibility](../../specs/2026-10-20-desktop-compatibility-amendment.md) for novice participants after the successful Claude Code experiment. Spec/plan revision 3 retain SDK 2.2.0 and strict July tests while accepting specifically verified older exchanges in both required desktops. [Runtime implementation and fresh compatible-desktop evidence](../../../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) now pass amended CP0. R1/R2 closures below remain attached to revision 2; no external review of this amendment or live Revit runtime pass is claimed. Include the amendment and its assertions in the external implementation-review packet.

Status: **R1 findings closed at the specification level; author approved revision 2 at `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44`; external R2 has closed all four findings at the plan level on revision 2 at `cfbfe043bd6c4d0627424ea3b099d0007459cb91`; the author subsequently approved revision 2 and chose Native execution in the [approval record](2026-10-20-plan-approval.md). Task 1 now has offline and [compatible-desktop CP0 evidence](../../../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md); [Task 2 corrected offline evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) now passes; Task 2 is independently closed; Task 3 peer-exit/exceptional-cleanup corrections pass Windows checks and await independent follow-up. Task 4 is unstarted. [Original strict failures](../../../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) remain preserved. [R2 follow-up](2026-10-20-r2-follow-up.md) records closure of the shared lifetime model as well as the individual findings. [R2 feedback/dispositions](2026-10-20-r2-feedback.md) preserve three major findings, one minor and the author's cross-cutting lifetime direction.** [Original feedback/dispositions](2026-10-20-r1-feedback.md) and [follow-up/author approval](2026-10-20-r1-follow-up.md) remain separate records. Baseline before design: `7d3ae71`. These sessions support the [written spec](../../specs/2026-10-20-revit-mcp-bridge-design.md), [plan](../../plans/2026-10-20-revit-mcp-bridge.md), [participant story](../../../tutorials/06-direct-design-and-external-review.md), and [agent procedure](../../../how-to/run-spec-plan-and-implementation-reviews.md). No live Revit bridge result or external R3 closure is claimed.

An external reviewer is a person or separately run agent who did not author this artifact. Their review is an independent challenge, not proof that the software works. The author decides whether to accept findings and proceed. The drafting agent's self-review is a different activity and must not be labeled external review.

| Session | Hand over | Reviewer evaluates | Exit evidence | Current state |
|---|---|---|---|---|
| R1: spec | [Spec packet](01-spec-review.md), spec, baseline source/research/oracle, [feedback/dispositions](2026-10-20-r1-feedback.md) | Correct problem, architecture, provenance, failure behavior, independently checkable acceptance | Review report, dispositions, revised spec revision/commit, explicit author approval | All three findings externally closed; revision 2 author-approved at 0ef8cda |
| R2: plan | [Plan review instructions](02-plan-review.md), approved spec, actual written plan | Feasible tasks, tests, order, deployment, rollback, review/evidence checkpoints | Review report, dispositions, revised plan commit, author approval and execution choice | All four findings externally closed at plan level on cfbfe04; author approved revision 2 and selected Native |
| R3: code and evidence | [Implementation review instructions](03-implementation-review.md), exact diff and runtime evidence | Implementation meets approved contract, licensing/provenance, regression and host evidence | Review report, correction tests/evidence, remaining limits, author disposition | Not ready for full R3: Task 1/amended CP0 and Task 2 offline checks passed; Tasks 3–9/live evidence outstanding |

These are build-process sessions outside the two one-hour seminar agenda. The seminar can show selected prompts, findings, and corrections; attendees can repeat the full workflow afterward. No external message, public upload, new chat, or reviewer assignment is implied by preparing these files.

## Handoff procedure

Finish the artifact and commit before handing it off. From the lab root capture:

```powershell
git status --short --branch
git rev-parse HEAD
git log --oneline -5
git diff --stat 7d3ae71..HEAD
```

Put the full reviewed commit hash, spec revision, and the relevant packet in the actual reviewer request. Give the reviewer that Git tree or an approved local copy containing the packet's read set. If using a patch, include the baseline source they need; a patch alone is insufficient for a reviewer without this repo. Local ignored clones and RVT files are not included in a checkout. The pinned upstream URLs and retained textual evidence allow a spec review without Revit or those clones.

Use a new review context to reduce reliance on the implementer's conversation. Ask the reviewer to inspect artifacts and support findings, not simply endorse a preferred answer. Do not invent a reviewer name, returned findings, approval, or a full commit hash before it exists. Record missing reviewer inputs as limitations. Return findings into the [findings template](../../../../templates/external-review.md); preserve the original review and record dispositions separately.

## Review log

| Event | Reviewer / actor | Artifact | Outcome |
|---|---|---|---|
| Draft prepared, 2026-10-20 | Drafting agent | Spec revision 1 and packets | Ready for author/external review; no approval claimed |
| External R1 received, 2026-10-20 | Codex in separate review context, supplied by author | Spec revision 1 at `b56c0dc219e1269f5b90542004267a2d388f338d` | Revise before approval; R1-01/02 major, R1-03 minor; no runtime tests |
| R1 dispositions/revision, 2026-10-20 | Drafting agent; author selected both desktop clients | Spec revision 2 and feedback record | Three findings accepted and addressed in text; follow-up/author approval pending |
| External R1 follow-up, 2026-10-20 | External reviewer feedback supplied by author | Spec revision 2 at `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44` | All three findings closed at specification level; no runtime tests |
| Author spec decision, 2026-10-20 | Author | Same revision 2 | “Approve revision 2 and write the plan” |
| Plan drafted, 2026-10-20 | Drafting agent | Implementation plan revision 1 | Self-reviewed; CP0 requires both desktop protocols before connection work |
| External R2 received, 2026-10-20 | Reviewer not named in supplied attachment | Plan revision 1 at `17108c239e3aa6f384c89f15d2fd623dbce60760` | Revise before implementation; three major/one minor; no runtime tests |
| Cross-cutting direction, 2026-10-20 | Author | Operation lifetime/ownership across task boundaries | Trace admission, interruption, ownership and safe terminal states before local fixes |
| R2 dispositions/revision, 2026-10-20 | Drafting agent | Plan revision 2 | Four findings accepted and addressed in text; follow-up and author plan decision pending |
| External R2 follow-up, 2026-10-20 | Reviewer not named in supplied message | Plan revision 2 at `cfbfe043bd6c4d0627424ea3b099d0007459cb91` | All four findings and underlying planning concern closed at plan level; author approval/execution choice pending; no runtime tests |
| Author plan decision, 2026-10-20 | Author | Plan revision 2 at `cfbfe043bd6c4d0627424ea3b099d0007459cb91` | Approved; Native execution, start with both desktop clients |
| CP0 execution, 2026-10-20 | Implementing agent and author reload | Strict SDK 2.2.0/protocol 2026-07-28 probe | Offline tests pass; both desktops send older initialize handshakes, fail before discovery; Task 2 not started |
| Compatibility execution, 2026-10-20 | Author instruction; Native implementing agent | Revision 3 compatible host plus strict July regression | Both actual desktops passed synthetic CP0; Task 2 next; full live acceptance pending |
| External R3 | Unassigned | Future code and evidence | Not ready |

The drafting agent's pre-handoff checks are recorded in [self-review](00-authoring-self-review.md). Update this log with actual dates, identities, full reviewed revisions, findings, and dispositions as the sessions happen. Do not substitute a change of status text for evidence of review.

Task 2 checkpoint, 2026-10-21: the author requested the next task under the updated plan and Native execution. [Foundation evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations.md) records both targets, independent exchange, retained compatibility and readiness regression. The implementing agent inspected source and results; this is not an independent R3 review. Include the bounded framework serializer refinement and quiescence cleanup helper in the eventual implementation review.
