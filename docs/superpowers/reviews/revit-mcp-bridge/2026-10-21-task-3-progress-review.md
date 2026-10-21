# Task 3 progress review and implementing-agent response

**Subsequent independent closure:** [Task 3 closed at af4c67b](2026-10-21-task-3-closure.md). The packet below preserves the implementing-agent submission before that closure.

Baseline: **`b334385f1c1b74b8f204058e1c2b98c092f818d8`**. The author relayed independent closure of the deadline P1 and this new P2. Reviewer identity was not supplied. The implementing agent accepted the diagnosis, committed an execution/waiting model **before production changes** at `373e5a8`, then derived the correction and tests from it. **Independent P2 follow-up remains pending; Task 4 is unstarted.**

## Feedback supplied by the author

The finding is retained below with source/evidence links made repository-relative. Line references refer to the reviewed baseline, not the correction.

> **The deadline P1 is independently closed at `b334385`. Task 3 still has one P2 finding.**
>
> My unchanged crash reproducer now exits successfully on **both runtimes**, retaining the original callback error for reporting through `StopAsync`.
>
> **[P2] Held cancellation callbacks still block network shutdown and connection-slot recovery.** [RequestLifetime.cs](../../../../projects/revit-readiness/src/bridge-runtime/RequestLifetime.cs) invokes callbacks synchronously. During stop, that happens inside [StopAsync](../../../../projects/revit-readiness/src/bridge-runtime/PipeEndpoint.cs). During disconnect, the endpoint awaits the monitor that is executing those callbacks.
>
> I reproduced both paths on net48 and net10.0, with the executor already `Quiesced`:
>
> - **Stop:** a held callback blocked `StopAsync` before it returned its task.
> - **Disconnect:** only seven replacement connections could connect; the eighth timed out.
> - Shutdown and descriptor removal completed only after releasing the callback.
>
> [Reproducer](../../../../projects/revit-readiness/docs/verification/cancellation-progress-evidence/reviewer-Program.cs), [net48 results](../../../../projects/revit-readiness/docs/verification/cancellation-progress-evidence/reviewer-net48.txt), [net10 results](../../../../projects/revit-readiness/docs/verification/cancellation-progress-evidence/reviewer-net10.0.txt).
>
> The deadline-held test passes because its callbacks run on a separate task. The remaining requirement is to **release network resources independently of borrowed callback completion**, while retaining the lifetime until those callbacks finish. Extend the held-callback regression through actual endpoint stop and disconnect.
>
> All supplied checks passed: **38 pipe and 33 foundation tests per runtime**, MCP—including strict July—readiness, Excel and isolation. I also verified all **104 retained evidence hashes**, timing checks and negative controls.
>
> Repository unchanged and clean. Task 4 and live acceptance remain ahead.

## Diagnosis and direction supplied with the review

The author also supplied the following diagnosis and requested direction. These are reviewer/author observations, separate from the coder's response below.

> **The coder has developed a model of resource ownership, but still lacks a complete model of execution and waiting.** That is my diagnosis from the code and reproductions—not a claim about their intentions.
>
> Resource ownership asks: **“Who keeps this object alive, and who disposes it?”**
>
> Execution ownership asks: **“Who runs this code, on which thread, and whose progress depends on it returning?”**
>
> These are connected by one assumption: **a cancellation or cleanup step is treated as a small operation that will return.** In reality, calling cancellation executes borrowed callbacks. Those callbacks can throw or remain running.
>
> We keep finding these because the verification is following the reported examples more closely than the underlying combinations.
>
> **The missing distinction is safety versus progress:**
> - Safety: resources remain alive while callbacks use them; failures stay contained.
> - Progress: connection slots, shutdown callers and descriptor cleanup can finish while those callbacks remain running.
>
> Before patching again, map every cancellation origin through the real endpoint. Show which thread executes each callback, every lock held, and every completion that shutdown waits for. Treat cancellation requested, network released, executor finished, callbacks finished and lifetime disposed as separate facts.
>
> Demonstrate that network shutdown can progress while callbacks throw or remain held. Preserve immediate invalidation, the original deadline, bounded retained work and eventual disposal. Derive the regression cases from that model, including held callbacks through actual stop and disconnect—not just helper-level tests.
>
> Simply moving a call onto another task would need the same scrutiny. **The design needs explicit limits on what shutdown is allowed to wait for.**

These are labeled excerpts of the longer conceptual explanation; the complete actionable finding and direction are preserved above.

## Implementing-agent disposition

Accepted after the unchanged reproducer showed both failures on both runtimes. The earlier fixes protected resources and contained exceptions, but left borrowed callbacks on network control paths. The coder also used the callback token as the transport signal, so callback ordering could obstruct cleanup even after moving Cancel to a different thread. This was an incomplete execution model, not a missing catch clause.

The [execution/waiting model](../../../explanation/bridge-execution-and-waiting.md) records origins, threads, locks, permitted waits, terminal states and bounded retention. It was committed at `373e5a8` before production changes. Twelve origin/outcome/quiescence combinations plus capacity and client cases ran first: four deadline cases passed, ten cases failed per runtime. The separation is now explicit: immediate context invalidation, private transport cancellation, one owned notification worker, and deferred physical retirement. Eight retained-request permits are shared across endpoint restarts in the runtime/AppDomain; overload gets correlated retryable Busy without dispatch. Stop releases its gate before signaling, and never awaits executor/notification retirement.

The model audit added a fifteenth case for exceptional Quiesced completion. It initially exposed a permit leak in the first correction: successful disposal followed by rethrowing Quiesced's error skipped permit release. Retirement now reports the error separately and releases after successful physical disposal. This adjacent observation belongs to the implementing agent, not the reviewer. Failed physical disposal retains its permit and emits a local failure.

Two prior assertions changed deliberately: immediate invalidation is checked through Context.IsCancellationRequested/ThrowIfInactive, not the asynchronously notified token; shutdown reports already-known errors and later Stop reports callback errors arriving afterward. Zero reader calls, retained error identity, source lifetime and eventual disposal remain asserted. Their initial failed runs are preserved so this contract refinement is reviewable.

The unchanged reviewer reproducer now reports StopAsync returned, network shutdown complete, descriptor absent **before release**, and eight simultaneous disconnect replacements, on both runtimes. [Fresh evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md) records full regressions and independent capture checks. No package, copied SDK source, protocol, model or installation changes. Prior P1 closure stands; this P2 is not independently closed by these author-side results.

## Follow-up packet

Review `b334385..` the commit titled **`fix: separate bridge cancellation signaling from callback execution`** that adds this packet. This range includes the pre-implementation model commit `373e5a8`. Resolve the final hash with `git log --diff-filter=A --format="%H %s" -- docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md`.

Suggested request:

> Review the execution/waiting model before the patch. Trace stop, disconnect, deadline and external client cancellation to each actual wait and lock. Verify immediate invalidation without waiting for notification; ensure no borrowed callback controls transport wakeup, StopAsync task return, network drain or descriptor removal. Re-run the unchanged reproducer and all fifteen process cases with callbacks held throughout progress assertions. Challenge repeated/concurrent origins, callback throw/report timing, physical disposal, exceptional Quiesced, eight-permit saturation and restart bypass. Inspect the two updated prior assertions against the model. Verify retained evidence and both independent capture checkers, run All/readiness, and distinguish this P2 from the already-closed P1. No Task 4, live Revit or desktop A8 acceptance is claimed.
