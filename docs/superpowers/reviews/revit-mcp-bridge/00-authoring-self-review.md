# Drafting agent's specification self-review

Date: 2026-10-20. Actor: the same agent that wrote the spec. Artifact: [spec revision 1](../../specs/2026-10-20-revit-mcp-bridge-design.md). This is the brainstorming skill's inline self-review, **not an external review, author approval, or runtime verification**.

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

This pass inspects design and baseline source only. It does not establish pipe interoperability, actual net48 dispatch behavior, modern protocol use by the seminar client, SDK package restore success, or live sample equivalence. Those remain acceptance A1-A9. No external reviewer has yet challenged the draft.
