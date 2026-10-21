# Task 5 checklist error correction verification

**Later independent disposition:** [Task 5/T5-01 closed at 5e9a5dd](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-closure.md), implementation/offline scope only. Original evidence below remains historical.

Baseline **f5510ec**, reviewed Task 5 implementation. The author relayed one P2 finding: empty and oversized sidecars became InternalError instead of ChecklistInvalid. [Original feedback and separate implementing-agent disposition](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-checklist-review.md) are preserved. Task 5 remains open pending independent follow-up.

| Evidence | Observation |
|---|---|
| Unchanged reviewer reproduction before fix | net48: empty and oversized inputs reproduce InternalError; missing and valid controls pass |
| New production-linked adapter/queue tests before fix | Six controls pass; four assertions fail across both operations |
| Same ten tests after fix | Ten pass: rejected inputs give ChecklistInvalid with correlated nonretryable errors and zero reader calls; missing/valid/exact-limit controls preserved |
| Unchanged reviewer reproduction after fix | net48 sidecar matrix passes; held-Raise test passes on both net48 and net10.0 |
| Full regressions and build | Pass: 87/82 Dispatch plus ten handoff cases; 53 pipe and 33 foundation cases per runtime; MCP 19 C# / ten Python / four SDK exchanges including strict July; 18 readiness/seven Excel/isolation checks; Autodesk-reference build has zero warnings/errors |

The only production change is the catalog filter's explicit InvalidDataException check. No blanket exception mapping or changes to the reader, evaluator, queue, budgets, SDK/protocol policy, installer or client settings. The new harness uses project references only, with a locked dependency graph; no new package/license adoption.

[Retained evidence](revit-adapter-review-evidence/inventory.json) separates the original reviewer output/source, implementing-agent reruns, new test failures/successes and final verification. The reviewer source copies are evidence only; their source hashes establish that the rerun used unchanged reproductions. Existing retained evidence is not overwritten.

These are offline tests with Autodesk/reader doubles plus a real Autodesk-reference build. No live Revit execution, installation, actual shutdown scheduling, sample-model comparison or desktop A8 verification was performed. The original [live acceptance matrix](../../tests/mcp-host-acceptance.md) remains required. See the [participant lesson](../../../../docs/tutorials/11-connect-the-readiness-reader-to-revit.md#review-follow-up-test-the-returned-error) and [technical procedure](../../../../docs/how-to/build-and-verify-revit-adapter.md#review-follow-up-sidecar-error-handoff).

The new inventory contains **36 evidence files**. All **295** file hashes in retained inventories using this schema were recomputed successfully; prior evidence remains unchanged. The correction commit is the one titled `fix: classify rejected checklist snapshots at the adapter boundary` that adds this record. Independent follow-up is pending.
