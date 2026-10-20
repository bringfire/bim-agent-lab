# Read-only probe: host acceptance procedure

Define the expected behavior before writing the command. Run these checks inside Revit 2024; a successful compile is a separate result.

## Contract

`BIM Agent Lab - Read-only probe` reports the active project title, path (or unsaved), Revit version/build, and counts of non-type elements in the Walls and Ceilings categories. Scope is the entire active host document, with no view, phase, design-option, or level filter. Linked documents are excluded. Counts are category-instance counts, not quantities or readiness findings. No result limit is applied.

The command uses `TransactionMode.ReadOnly`, opens no transaction, makes no model edits, and reports the document's modified flag before and after the reads. It is unavailable without an active project or in the family editor. API errors return a failure rather than an apparent zero result.

## Cases and independent expectations

| Case | Expected result | Independent basis |
|---|---|---|
| No open project | Command unavailable | Revit Home/no active document |
| Fresh empty project | Walls 0; ceilings 0; correct document title | No wall or ceiling has been placed |
| Controlled project containing four wall segments | Walls 4; ceilings 0 | Four walls drawn using Revit's rectangle wall tool; inspect selection/filter in Revit |
| Saved controlled project, repeated invocation | Identical counts; modified flag false before and after | Saved baseline, repeat command, compare source RVT SHA-256 before/after |

If a template already contains geometry, use an empty project or document its actual baseline rather than assuming zero. A disk hash alone does not establish unchanged in-memory state; combine it with read-only API enforcement, the modified flag, and inspection of command source.

Record actual host/build, commands, results, and deviations in `docs/verification/`. The broader readiness fixture remains a separate task.
