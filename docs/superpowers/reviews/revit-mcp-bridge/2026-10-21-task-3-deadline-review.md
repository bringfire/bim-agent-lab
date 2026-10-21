# Task 3 deadline review and implementing-agent response

Reviewed baseline: **`f79506defedd020f0ac3c5aaacc76f8c74c510e7`**. Reviewer identity was not supplied in the author's message. The original peer-exit P2 is independently closed. The new P1 is corrected and verified by the implementing agent below; **independent follow-up remains pending. Task 4 is unstarted.**

## Feedback supplied by the author

The review wording is retained below with repository-relative evidence/source links.

> **The original P2 finding is closed at `f79506d`. One additional P1 finding prevents Task 3 closure.**
>
> **[P1] A cancellation callback throwing during deadline expiry terminates the host process.** [RequestLifetime.cs](../../../../projects/revit-readiness/src/bridge-runtime/RequestLifetime.cs) uses `CancelAfter`, which invokes callbacks on a timer thread. Exceptions there escape the endpoint's cleanup handlers.
>
> I submitted a valid short-deadline request through the real pipe endpoint, registered the same kind of throwing callback used by the new tests, and allowed the deadline to expire. **Both net48 and net10.0 terminated with an unhandled `AggregateException`.** Both survived when the callback returned normally. [Reproducer](../../../../projects/revit-readiness/docs/verification/deadline-cancellation-evidence/reviewer-Program.cs), [net48 output](../../../../projects/revit-readiness/docs/verification/deadline-cancellation-evidence/reviewer-net48.txt), [net10 output](../../../../projects/revit-readiness/docs/verification/deadline-cancellation-evidence/reviewer-net10.txt).
>
> This predates this correction commit. The expanded callback tests cover stop and disconnect but omit deadline expiry. Deadline-triggered cancellation needs contained error reporting and cleanup, preserving the original budget and lifetime ownership until `Quiesced`. Add a child-process regression that proves survival and cleanup before integrating this lifetime into Revit.
>
> Otherwise, verification passed:
>
> - Original peer-exit reproduction, all eight replacement slots, repeated shutdown and replacement selection.
> - All five new regressions; **32 pipe and 33 foundation tests per runtime**.
> - MCP—including strict July—readiness, Excel and process isolation.
> - All **62 retained evidence hashes**, timing checks and checker negative controls.
>
> Repository unchanged and clean. Live Revit and desktop acceptance remain outside this review.

## Implementing-agent disposition

Accepted after reproducing the timer-thread crash in disposable child processes on both targets. Existing tests protected two initiators of cancellation, but the same callback executed from a runtime timer had no containing exception boundary. A separate held-callback test exposed premature disposal when the executor became Quiesced while cancellation was still executing.

The first correction contained the owned deadline task. Applying the diagnosis across all cancellation origins then exposed the same crash through linked upstream and owner tokens when **their** owner used CancelAfter. Those additional findings and a diagnostic-observer mutation defect are implementing-agent observations, not claims attributed to the external reviewer. Red evidence is retained for each.

RequestLifetime now owns an ordinary CTS. The deadline task and explicit upstream/owner registrations all invoke one exception-containing method. It retains callback errors and reports them locally; the deadline loop always rechecks the original budget. Cleanup waits for executor Quiesced, deadline work and all active cancellation invocations before disposing registrations/sources. Logical cancellation alone is not physical completion. Dispose initiates cleanup without blocking the callback that may itself be calling it.

PipeEndpoint emits `CancellationCallbackFailed` with request/connection identity and retains the first exception independently of mutable observer data. An observer failure cannot escape. Every callback failure is emitted; bounded retained state lets Stop report an error after owned network/descriptor cleanup. A later Stop observes a failure reported after an earlier successful stop. No exception stack or late success is sent to the expired request.

The [verification record](../../../../projects/revit-readiness/docs/verification/2026-10-21-deadline-cancellation.md) supplies six new child cases per runtime and full regressions. [API research](../../../research/revit-cancellation-boundaries.md) records the official sources and provenance. No normative spec, package, SDK/protocol, installation or desktop policy changed. Correction evidence does not declare this P1 closed.

## Follow-up packet

Review from `f79506defedd020f0ac3c5aaacc76f8c74c510e7` through the commit titled **`fix: contain cancellation callback failures across timer boundaries`** that adds this file. Resolve it with `git log --diff-filter=A --format="%H %s" -- docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-deadline-review.md`.

Suggested reviewer request:

> Recheck the deadline P1 with a real pipe and disposable child processes on net48/net10.0. Verify survival, original-budget expiry, all registered callbacks attempted, no late response, local retained errors, repeated stop, descriptor cleanup and usable replacement. Hold a cancellation callback after executor Quiesced and after network stop; prove its source remains alive until it returns. Repeat with upstream and owner tokens canceled on foreign timer threads before the request budget expires. Challenge diagnostic mutation/failure and normal early disposal. Inspect races between registration, cancellation, disposal and late error reporting. Run All and readiness, verify new and historical hashes and timing controls. Task 4, live Revit and full desktop A8 remain outside this increment.
