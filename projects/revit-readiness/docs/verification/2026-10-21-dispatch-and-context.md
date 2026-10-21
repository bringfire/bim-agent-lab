# Task 4 dispatch and context verification — 2026-10-21

**Follow-up:** Task 4 is [independently closed at 24a72cf](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-closure.md). The submission observations below are preserved as history. Task 5 implementation/live acceptance is separate.

**Implementing-agent verification; independent Task 4 review pending.** Baseline: af4c67bd4391fd00019799ff1436a4f53b744fe4, [independently closed Task 3](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-closure.md). The Task 4 execution model and closure record were committed at **638e288 before production changes**. The implementation is the commit titled **`feat: coordinate bounded host dispatch and context lifetime`** that adds this record; locate it with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-dispatch-and-context.md`.

## Observed results

- **46 Dispatch cases per runtime** (net48 and net10.0) pass: bounded admission/drain, signal outcomes/recovery, separate response/quiescence, cancellation/publication, context identity/capacity/expiry and fresh values.
- Four cases per runtime connect the actual Windows pipe endpoint to the production queue. Stop/disconnect with queued/executing work release network/descriptor resources while notification callbacks remain held. Disconnect restores all eight simultaneous connections. The executor and signal are test doubles; these are not live Revit calls.
- **53 pipe and 33 foundation cases per runtime** pass, including prior held-callback/exception tests, independent timing/progress checkers, bilateral messages and Python comparisons.
- MCP retains **19 C# cases**, ten Python methods and four SDK/wire comparisons, including compatible July and strict July/legacy rejection.
- Readiness retains **18 core, seven Excel and process-isolation checks**. Builds report zero warnings/errors. No installation, client configuration, model or desktop UI changed.

## Red/green and model checks

The first 39 tests compiled against stubs and failed; the added publication test made 40 failing cases per runtime. An earlier test setup used an offset timestamp where the existing contract requires a UTC Z suffix; this was corrected before the retained stub run and is not counted as a production defect. Stubs are intentionally nonfunctional; their failures establish missing functionality, not a benchmark of partially working behavior.

The first implementation passed 40 cases. Four actual-pipe/production-queue cases also passed. The incomplete-binding test then failed before validation was added; the null/crossed executor-envelope test failed before response guarding was added. Both are author-side findings preserved separately from independent review.

A mutation replaced the publication gate shared with RequestLifetime by an unrelated lock. The targeted test failed on **both runtimes** with `Invalidation crossed a guarded publication`; the mutation was restored in a finally block, then the complete suite rebuilt and passed. This challenges the test's ability to detect the exact missing synchronization, beyond its initial stub failure.

The final result preserves one Context/budget, immediate invalidation, callback containment, deferred disposal and bounded retained work. The queue adds no borrowed executor work to the private transport signal. The final read-preparation guard and actual Revit API scheduling remain Task 5 work.

## Repeat and inspect

From repository root:

```powershell
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Dispatch
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
```

The [evidence inventory](dispatch-context-evidence/inventory.json) records exact bytes/hashes for retained red/green runs, mutation result, final suite output and fresh exchange/protocol captures. Existing Task 3's 151 and the foundation's 47 retained evidence hashes remain unchanged. Retained evidence is revision-specific; rerun the commands after a code change.

See the [execution model](../../../../docs/explanation/dispatch-and-context-lifetime.md), [agent procedure](../../../../docs/how-to/build-and-verify-dispatch-and-context.md), [participant lesson](../../../../docs/tutorials/10-coordinate-requests-and-model-context.md) and [independent-review packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md). This increment adds original lab code and framework APIs only; no dependency or provenance policy changes. Task 5, live Revit, desktop A8 and full R3 acceptance remain outstanding.
