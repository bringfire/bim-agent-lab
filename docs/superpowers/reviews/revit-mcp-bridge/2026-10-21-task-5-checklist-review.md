# Task 5 checklist error review and correction

**Later disposition:** [T5-01 and Task 5 independently closed at 5e9a5dd](2026-10-21-task-5-closure.md) for implementation/offline checks. The original record below is historical.

The author relayed the following independent review of the Task 5 checkpoint at **f5510ec**. Reviewer identity was not supplied. **Task 5 remains open**; implementing-agent verification below is not independent closure. Tracking label **T5-01** is assigned here for this single P2 finding.

## Original feedback

> **One P2 finding; Task 5 should remain open.**
>
> **Empty and oversized checklists return the wrong error.** In RevitContextCatalog.cs, the filter misses InvalidDataException, which does not inherit from IOException.
>
> I reproduced both cases through the unchanged catalog/executor source and production queue, using Autodesk boundary doubles:
>
> - Empty sidecar → InternalError
> - Sidecar exceeding four MiB → InternalError
>
> Both require ChecklistInvalid. Missing-file and valid-file controls passed. Catch this exception explicitly and test the resulting error through the adapter/queue handoff.
>
> Otherwise, **87/82 dispatch tests, prior regressions, and the Revit API build pass**. An additional held-Raise shutdown test passes on both runtimes. I found no further actionable defects in scheduling ownership or the final read guards.
>
> Actual Autodesk scheduling, final shutdown behavior, installation, and desktop acceptance remain unverified. The checkout is clean.

The supplied inline comment is titled **“P2 Map snapshot validation failures to ChecklistInvalid”**, targeting catalog lines 49–53. The reviewer cited [Microsoft's InvalidDataException reference](https://learn.microsoft.com/en-us/dotnet/api/system.io.invaliddataexception?view=netframework-4.8). The supplied original output is retained in the [new evidence folder](../../../../projects/revit-readiness/docs/verification/revit-adapter-review-evidence/reviewer-original-net48.txt); the original local location was `C:/Users/bring/AppData/Local/Temp/codex-task5-independent-review/net48.txt`.

## Implementing-agent disposition

Reproduced before modifying production code. The unchanged reviewer harness returned InternalError for both inputs. Local reflection and the production-linked net48 harness confirm InvalidDataException's SystemException base; namespace similarity does not establish an IOException inheritance relationship. The direct Microsoft 4.8 page was unavailable through the browsing tool; its indexed [4.8.1 reference](https://learn.microsoft.com/en-us/dotnet/api/system.io.invaliddataexception?view=netframework-4.8.1) also records the class declarations. The actual target-runtime result is retained.

The production correction is one explicit exception-filter member. Missing-file branches remain ChecklistMissing; unexpected exceptions still reach the queue's InternalError handling. No catch-all, result fabrication, reader/evaluator, lifetime or dispatch-policy change was introduced.

The coverage gap was between previously tested components: snapshot tests proved rejection, and codec tests proved parsing/error behavior, but no retained test ran bad sidecar bytes through the real catalog, executor and queue. The new original `adapter-handoff-tests` harness links those production source files rather than copying their logic. Its test-only Autodesk objects, final reader and native event/endpoint doubles establish an offline error contract, not live Revit behavior. The reviewer's harness was used unchanged as a separate reproduction and as a test-design precedent; it is not compiled into the production add-in or adopted test suite.

Ten new cases exercise both named operations with missing, empty, oversized, valid and exactly-four-MiB valid sidecars. Before the fix, six controls passed and four error assertions failed. After the fix, all ten pass, checking error code, request/session correlation, retryability, absent result on error and zero reader calls for rejected inputs. Valid readiness calls reach the reader double once and preserve its false completeness flags. The unchanged reviewer harness passes on net48; its held-Raise test also passes on net10.0.

See [fresh verification](../../../../projects/revit-readiness/docs/verification/2026-10-21-revit-adapter-review.md), [participant follow-up](../../../tutorials/11-connect-the-readiness-reader-to-revit.md#review-follow-up-test-the-returned-error), and [agent procedure](../../../how-to/build-and-verify-revit-adapter.md#review-follow-up-sidecar-error-handoff).

## Suggested independent follow-up

> Review f5510ec through the commit titled `fix: classify rejected checklist snapshots at the adapter boundary`. Confirm the narrow InvalidDataException mapping, run the unchanged reproduction, and inspect the source-linked handoff harness. Verify both operations reject empty/oversized inputs as ChecklistInvalid with no reader call, preserve missing/valid/exact-limit controls and error envelope semantics, and run Dispatch/All plus readiness/build checks. Confirm the doubles and observed scope are described accurately. Report whether T5-01 and Task 5 can close; actual Autodesk scheduling, shutdown, installation and desktop acceptance remain separate.

Independent follow-up: **pending**. Task 6 is unstarted.
