# Drafting agent's specification self-review

Date: 2026-10-20. Actor: the same agent that wrote the spec. Initial artifact: spec revision 1 at `b56c0dc219e1269f5b90542004267a2d388f338d`; the [current spec](../../specs/2026-10-20-revit-mcp-bridge-design.md) is revision 2. This is the brainstorming skill's inline self-review, **not an external review, author approval, or runtime verification**.

## Checks and decisions before handoff

| Check | Evidence examined | Result incorporated into the draft |
|---|---|---|
| Purpose and scope | Actual user prompts, research comparison, existing information-check source and answer sheet | Distinguish three information-check samples from the larger three-question demo; preserve original build objective |
| Provenance ambiguity | Prior bridge lockfile observations, official C# release, existing lab code | Explicitly distinguish SDK dependency, precedent-only TypeScript 1.26.0/helper code, reused lab code, and original adapter |
| Runtime boundary | Current core project and serializer | Keep net48 core/System.Web.Extensions inside Revit; do not load it into the modern server |
| API lifecycle | Existing command attribute and official ExternalEvent guide | Create event in a start command's valid API context; explicitly state that read-only command attributes do not protect the new handler |
| Context ambiguity | Reader's project-ID/filename validation and sample copies | Add open-document lifetime, full path, session and checklist hash to binding; clarify that a token does not freeze model values |
| Timeout/queue ambiguity | Prior integration gates and proposed pipe flow | Concrete frame/size/queue/deadline bounds, cancellation of queued work, discard of late results, Raise-result/race tests |
| Oracle coverage | Existing independent comparator | Require added full identity/rule/source/coverage checks; current expected totals alone cannot certify transport fidelity |
| Status consistency | Spec, participant story, packet states | External sessions explicitly pending; implementation plan is a later artifact; no new build, installation or Revit pass claimed |

The draft was checked for unfinished placeholders, contradicting scope, and ambiguity. Operational limits remain explicit design proposals for reviewer challenge, not missing values. Detailed file/task execution belongs in the subsequent plan. Local link, prompt, and Markdown-only change checks are recorded with this checkpoint in the journal after verification.

## Limits

This initial pass inspected design and baseline source only. It did not establish pipe interoperability, actual net48 dispatch behavior, modern protocol use by the seminar client, SDK package restore success, or live sample equivalence. Those remain acceptance A1-A9. At the revision 1 checkpoint no external review had yet been returned; the subsequent [R1 feedback](2026-10-20-r1-feedback.md) is now retained separately.

## Revision 2 self-review after external feedback

Checked R1-01–03 against the actual source before revising the contract. Confirmed that the two incomplete controls remain valid checklists, use separate unexamined collections, and have different evaluator completeness flags. Kept the original three sample expectations and complete-case comparator intact. Required each author-selected desktop application to run the modern-protocol acceptance sequence, with installed version observations distinguished from compatibility evidence. Checked the five-second frame timer and subsequent budget against the original 30-second total lifetime; no timer resets on partial progress or complete receipt.

Checked current status/navigation against the received review and updated spec revision, while preserving historical journal entries and original reviewer wording. No external follow-up, author approval, model edits, builds, or MCP/Revit tests are claimed by this documentation revision.
