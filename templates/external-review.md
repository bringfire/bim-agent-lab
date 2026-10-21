# External review record template

Copy this form into the relevant feature's review folder when an actual review occurs. The fields below are intentionally unfilled template fields, not pending defects or completed review claims.

## Identity and scope

- Stage: spec / implementation plan / implementation and evidence.
- Reviewer name or tool/model, and whether independent of the artifact author:
- Requested and received dates:
- Exact reviewed full Git commit(s), artifact paths, and spec/plan revision:
- Inputs actually read; commands/tests actually run:
- Inputs not available, assumptions, and runtime checks not performed:
- Recommendation: proceed / revise / insufficient evidence, with reasons.

## Findings as received

| ID | Severity | File/section/line | Failure scenario and impact | Supporting evidence | Smallest recommended correction | Verification method |
|---|---|---|---|---|---|---|

Severity: **blocker** prevents the stated contract or evidence from being reliable; **major** needs correction before the relevant stage can proceed; **minor** is a bounded improvement; **question** asks for information without asserting a defect. Do not inflate a preference into a blocker. If there are no findings, state the inspected scope and limits rather than claiming universal correctness.

## Shared-concept diagnosis, when warranted

Two or three major findings, or more, are a cue for the author to ask the independent reviewer whether a missing concept or reasoning framework connects them. This is a heuristic, not proof of a common cause; one systemic finding can also warrant it. “No supported common cause” is a valid conclusion. Preserve the original findings above.

- Diagnosis author/reviewer, date, and exact artifact/source revision:
- Proposed missing concept or assumption, explained for the coder:
- Finding IDs it connects and evidence for each connection:
- Findings it does not explain, contrary evidence, and uncertainty:
- Reusable reasoning procedure; other relevant places to inspect:
- What changes and verification would demonstrate application of the concept:

## Author/implementer dispositions

Preserve the reviewer's original wording above. This section records the response, not a rewritten review.

| Finding ID | Accept / reject / defer | Reason and evidence | Correction commit/artifact | Verification result | Reviewer follow-up if needed |
|---|---|---|---|---|---|

Rejection requires evidence. Deferral identifies the remaining limitation and whether it blocks the intended use. Do not call a finding fixed because a file was edited; point to the relevant verification.

If a shared-concept diagnosis was supplied, also record the coder's source-based assessment, its application beyond the listed defects, correction/test references, and the reviewer's follow-up assessment. A statement of understanding alone is not evidence of correction. Link the durable teaching or agent note that preserves the insight.

## Gate decision

- Author's explicit decision and its source/date:
- Approved artifact full revision, or requested revisions:
- Unresolved findings and their effect on proceeding:
- For a plan: selected execution method.
- Next action and owner:

Do not infer author approval from silence, an external recommendation, or a successful build. The [worked review hub](../docs/superpowers/reviews/revit-mcp-bridge/README.md) shows the staged handoffs for the Revit bridge.
