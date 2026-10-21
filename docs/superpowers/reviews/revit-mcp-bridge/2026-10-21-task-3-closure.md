# Task 3 independent closure

The author supplied this independent follow-up for **af4c67bd4391fd00019799ff1436a4f53b744fe4**. Reviewer identity was not supplied. This closes the Windows IPC increment, not live Revit, desktop A8, or full R3 acceptance.

> **Task 3 is independently closed at `af4c67b`.** The held-callback P2 is resolved, prior P1 closure stands, and I found no new actionable issues.
>
> The correction addresses the underlying execution model: immediate invalidation, transport cleanup and callback execution now have separate paths. Deferred disposal preserves resource safety, while the eight-request limit bounds unfinished work across endpoint restarts.
>
> I independently verified:
>
> - My **unchanged reproducer** completes shutdown and restores all eight connections **while callbacks remain held**, on both runtimes.
> - The earlier deadline-crash reproducer still survives and retains its error.
> - All **15 new process cases**, **53 pipe tests** and **33 foundation tests per runtime** pass.
> - MCP—including strict July—readiness, Excel and isolation regressions pass.
> - All **151 retained evidence hashes**, both evidence checkers and 18 negative controls pass.
>
> The changed test assertions match the documented distinction between immediate invalidation and asynchronous notification; they retain the important safety checks.
>
> Task 4 should preserve the model’s explicit boundaries: nonblocking queue admission, authoritative context guards, and no executor work on the private transport signal. Standalone lifetime admission remains a Task 6 responsibility.
>
> Repository unchanged and clean. This closes the Windows IPC increment; live Revit, desktop A8 and full R3 acceptance remain ahead.

Implementing-agent disposition: record closure and continue the author-approved plan using Native execution. Task 4 implements and tests the API-independent queue and context store; Task 5 owns the actual ExternalEvent adapter. Preserve the [reviewed execution model](../../../explanation/bridge-execution-and-waiting.md), [original feedback and correction](2026-10-21-task-3-progress-review.md), and [retained evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md).
