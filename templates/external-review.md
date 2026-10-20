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

## Author/implementer dispositions

Preserve the reviewer's original wording above. This section records the response, not a rewritten review.

| Finding ID | Accept / reject / defer | Reason and evidence | Correction commit/artifact | Verification result | Reviewer follow-up if needed |
|---|---|---|---|---|---|

Rejection requires evidence. Deferral identifies the remaining limitation and whether it blocks the intended use. Do not call a finding fixed because a file was edited; point to the relevant verification.

## Gate decision

- Author's explicit decision and its source/date:
- Approved artifact full revision, or requested revisions:
- Unresolved findings and their effect on proceeding:
- For a plan: selected execution method.
- Next action and owner:

Do not infer author approval from silence, an external recommendation, or a successful build. The [worked review hub](../docs/superpowers/reviews/revit-mcp-bridge/README.md) shows the staged handoffs for the Revit bridge.
