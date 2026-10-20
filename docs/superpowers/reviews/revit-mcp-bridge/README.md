# Revit MCP bridge review sessions

Status: **R1 received with recommendation revise before approval; spec revision 2 addresses all three findings; reviewer follow-up and author approval pending.** [Original feedback and separate dispositions](2026-10-20-r1-feedback.md) retain the review of `b56c0dc219e1269f5b90542004267a2d388f338d`. Baseline before design: `7d3ae71`. These sessions support the [written spec](../../specs/2026-10-20-revit-mcp-bridge-design.md), [participant story](../../../tutorials/06-direct-design-and-external-review.md), and [agent procedure](../../../how-to/run-spec-plan-and-implementation-reviews.md).

An external reviewer is a person or separately run agent who did not author this artifact. Their review is an independent challenge, not proof that the software works. The author decides whether to accept findings and proceed. The drafting agent's self-review is a different activity and must not be labeled external review.

| Session | Hand over | Reviewer evaluates | Exit evidence | Current state |
|---|---|---|---|---|
| R1: spec | [Spec packet](01-spec-review.md), spec, baseline source/research/oracle, [feedback/dispositions](2026-10-20-r1-feedback.md) | Correct problem, architecture, provenance, failure behavior, independently checkable acceptance | Review report, dispositions, revised spec revision/commit, explicit author approval | Revise recommendation received; revision 2 ready for follow-up, not approved |
| R2: plan | [Plan review instructions](02-plan-review.md), approved spec, actual written plan | Feasible tasks, tests, order, deployment, rollback, review/evidence checkpoints | Review report, dispositions, revised plan commit, author approval and execution choice | Instructions prepared; plan not written, session not ready |
| R3: code and evidence | [Implementation review instructions](03-implementation-review.md), exact diff and runtime evidence | Implementation meets approved contract, licensing/provenance, regression and host evidence | Review report, correction tests/evidence, remaining limits, author disposition | Instructions prepared; implementation not started |

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
| External R2 | Unassigned | Future implementation plan | Not ready |
| External R3 | Unassigned | Future code and evidence | Not ready |

The drafting agent's pre-handoff checks are recorded in [self-review](00-authoring-self-review.md). Update this log with actual dates, identities, full reviewed revisions, findings, and dispositions as the sessions happen. Do not substitute a change of status text for evidence of review.
