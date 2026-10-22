Source: pasted by the author from Claude Desktop Chat, 2026-10-22. Answer follows as supplied.

**The declared scope does not pass.** Coverage is complete, but three required fields are missing, and they affect 8 of the 16 instances. Nothing was edited or exported, and the model was unmodified before and after the check.

**Active model:** `D:\bim-agent-lab\projects\revit-readiness\samples\local\mcp-live-task7\a10630e39dae448ba55abbfe0bc7ab6b\models\BAL-info-ceiling-negative.rvt`. The active file has changed again since the last run. It comes with its own checklist file and hash, though the checklist ID is still BAL-TYPE-INFO-v1.

**Declared scope:** The explicit checklist element UniqueIds, limited to host basic walls and ceilings on **L1 - Block 43** that are New in **New Construction**. Each type is checked once. Links and other elements are excluded, and there is no view or selection filter or result cap.

**Examined:** All 16 of 16 requested instances, across 6 types. There were no scope issues.

**Missing fields (3 findings):**

| Type                 | Field            | Revit parameter       | Reason      | Affected instances (ElementId, case)              |
| -------------------- | ---------------- | --------------------- | ----------- | ------------------------------------------------- |
| BAL INFO Ceiling ACT | Code (Type Mark) | `ALL_MODEL_TYPE_MARK` | Blank value | 2492802 (CACT-01), 2492809 (CACT-02)              |
| BAL INFO Wall B      | Fire Rating      | `FIRE_RATING`         | Blank value | 2492785 (WB-01), 2492786 (WB-02), 2492787 (WB-03) |
| BAL INFO Wall D      | Code (Type Mark) | `ALL_MODEL_TYPE_MARK` | Blank value | 2492795 (WD-01), 2492796 (WD-02), 2492797 (WD-03) |

Compared with the previous model, the Wall B and Wall D gaps are still present, and the Ceiling ACT gap is new.

**Passing types:** Wall A, Wall C and Ceiling GWB.

**Completeness flags:** Evaluation `Complete: true` and `CoverageComplete: true`.

**Unexamined cases:** None.

The verdict covers only the declared 16 instances. Another 1,196 host wall and ceiling instances fall outside that scope and were not evaluated, and 6 link instances were excluded.
