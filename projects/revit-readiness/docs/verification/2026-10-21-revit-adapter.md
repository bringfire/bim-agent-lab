# Task 5 Revit adapter verification

**Later independent disposition:** [Task 5/T5-01 closed at 5e9a5dd](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-closure.md), implementation/offline scope only. Original evidence below remains historical.

**Review follow-up:** Task 5 remains open. One P2 checklist-error mapping finding was reproduced and corrected, with new adapter/queue regression tests. [Feedback, disposition and follow-up packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-checklist-review.md) distinguish implementing-agent verification from independent closure. The original submission below is preserved.

Date: 2026-10-21. Implementing-agent observations; independent Task 5/CP1 review is pending. Baseline: **24a72cfd28665fd6f9d93875cfabe9621ced940d**, independently closed Task 4. Ownership model committed before code at **b44ea23**. The implementation checkpoint is the commit titled `feat: expose readiness through a read-only Revit event adapter` adding this record; resolve with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-revit-adapter.md`.

## Observed checks

| Check | Observation |
|---|---|
| Initial production-flow/lifecycle stubs | 48 pass / 20 fail per runtime; failures identify missing flow, issuance, retirement and adapter behavior. Two entry cases were already blocked by the reviewed queue, so direct flow assertions were added separately. |
| Snapshot stubs | 67 pass / four fail per runtime, including missing adapter and three snapshot tests |
| Extra handoff checks | Missing Save As hook fails on both runtimes; missing invalid-UTF8 normalization fails in actual net48 codec. Test-file placement and wrong API member compilation errors were corrected separately and are not behavioral defect evidence. |
| Final focused Dispatch | 87 pass net48; 82 pass net10.0. Difference is five actual net48 codec cases. |
| Guard mutations | Removing the immediate pre-read guard fails canceled and expired preparation cases on both targets (reader count becomes one). Removing the final flow guard fails both direct serialization interruption cases. Both mutations restored before final regressions. |
| Real endpoint + production host stop | Both runtimes release network and owned descriptor, retire the fake event, and keep request lifetime alive while public notification remains held; final disposal follows release. |
| Full regression/build | All passes: Dispatch 87/82; Pipe 53 each; Foundation 33 each; independent timing/progress and bilateral checks; MCP 19 C# / ten Python / four SDK exchanges including strict July. Readiness passes 18 core, seven Excel and export isolation. Autodesk-reference build passes with zero warnings/errors and no Autodesk output copies. No live Revit result is implied. |

The test endpoint fault double injects failure at event, listener and descriptor acquisition; actual PipeEndpoint ownership remains covered by its prior real-pipe tests. The event double cannot prove Autodesk scheduling/disposal. Actual net48 codec tests load production conversion without loading Autodesk API classes; typed date-shaped strings, malformed schema/JSON, strict UTF-8, false flags and oversize results are checked. No report truncation is accepted.

The [evidence inventory](revit-adapter-evidence/inventory.json) retains 32 evidence files with raw logs and environment/source hashes. All 259 file hashes in retained inventories using this schema were recomputed successfully. Older evidence inventories remain unchanged. Native API XML and DLLs are inspected locally and hashed, not redistributed. No new package is adopted; lock changes add only project references. Manual command, reader, evaluator, SDK/client compatibility policy, installer, client settings and sample models are unchanged.

## What is still unverified?

No installation, actual ExternalEvent execution, real model read through this adapter, changed-document/Save As runtime demonstration, final shutdown scheduling or full desktop A8 workflow was performed. The [live acceptance matrix](../../tests/mcp-host-acceptance.md) lists these obligations, including three principal samples and NC1/NC2. Compiling against Revit is not running in Revit. A synthetic desktop probe and a fake API event do not establish live acceptance.

Read the [participant story](../../../../docs/tutorials/11-connect-the-readiness-reader-to-revit.md), [technical procedure](../../../../docs/how-to/build-and-verify-revit-adapter.md), and [focused review packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-review.md).
