# Task 3 peer-exit review and implementing-agent response

**Later independent review:** the P2 below is closed at `f79506d`. The [new deadline P1 feedback and correction packet](2026-10-21-task-3-deadline-review.md) govern the remaining Task 3 follow-up. The original feedback/disposition below are preserved.

Reviewed revision: **`3e20251a966c826fea4d4c2968b6be9a43e97842`**. Reviewer identity was not supplied in the author's message. This records one P2 finding; **independent follow-up remains pending**. Task 4 is unstarted.

## Feedback supplied by the author

Wording is retained below; source links are made repository-relative and the reproduction is retained as exact bytes in the new evidence directory.

> **One P2 finding remains. I would not close Task 3 yet.**
>
> **[P2] A client exiting during identity validation breaks listener recovery and shutdown.** In [NativeWindows.cs](../../../../projects/revit-readiness/src/bridge-runtime/NativeWindows.cs), `Process.GetProcessById` throws `ArgumentException` if the connected client has exited. The endpoint's exception filters exclude that exception, so the worker faults permanently instead of releasing and replacing its connection slot.
>
> The failure then propagates into [PipeEndpoint.cs](../../../../projects/revit-readiness/src/bridge-runtime/PipeEndpoint.cs): awaiting the faulted workers prevents descriptor removal. Retrying `StopAsync` encounters the same fault again.
>
> I reproduced this on **net48 and net10.0**, using the existing acceptance observer to pause validation while a connected child process exited:
>
> - The listener worker faulted.
> - Both shutdown attempts failed.
> - The owned descriptor remained.
> - Starting a replacement endpoint caused `AmbiguousRevitInstance`, even with explicit PID selection.
>
> Handle peer disappearance as a recoverable connection failure, and ensure worker faults cannot skip owned-descriptor cleanup. Add a regression covering client exit at this handoff, restored listener capacity, repeated shutdown, and successful selection after restart. [Reproduction output](../../../../projects/revit-readiness/docs/verification/local-pipe-transport-review-evidence/reviewer-peer-exit.txt)
>
> The underlying gap is specific: **cleanup still depends on preceding work completing successfully.** The operation-lifetime model needs to cover exceptional completion too.
>
> Otherwise, my rerun of `-Suite All` and the readiness suite passed, including both runtime suites, protocol regressions, Excel and process isolation. I also verified all **30 retained evidence hashes**, the timing evidence, and six negative controls for its checker. No additional actionable findings surfaced in instance selection, timeout recovery or cancellation handoffs.
>
> Reviewed commit `3e20251`. Repository unchanged and clean; live Revit and desktop acceptance remain outside this review.

## Implementing-agent disposition

Accepted after reproduction. The initial three new tests failed on both targets: a real exited child left only seven usable workers; a deliberately faulted worker skipped descriptor removal; and a simultaneous worker/file-lock failure hid cleanup behind the worker error.

`NativeWindows.ValidateClient` now normalizes only the process lookup's disappearance exception to a recoverable connection I/O failure. The endpoint replaces that slot. Shutdown separately drains workers, attempts owned-descriptor cleanup, disposes the stop source, then reports retained errors. A fault is observable even after cleanup succeeds. A later Stop can retry a locked descriptor without canceling a disposed source; combined worker/cleanup errors retain both causes.

Applying the reviewer's diagnosis beyond the named line exposed two additional cases in author-written tests: a throwing cancellation callback could bypass shutdown cleanup, or fault the disconnect monitor before deferred lifetime disposal was registered. Cancellation errors are now retained while shutdown continues; lifetime cleanup runs in a finally after awaiting the monitor. It still waits for Quiesced before disposal. These adjacent findings are the implementing agent's observations, not additional findings attributed to the reviewer.

The [corrected verification record](../../../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) contains red/green evidence, all regressions and preserved original captures. No spec, SDK, protocol, dependency or installation policy changed. This response supplies implementation evidence; it does not close the independent review.

## Follow-up packet

Review from `3e20251a966c826fea4d4c2968b6be9a43e97842` through the commit titled **`fix: preserve bridge cleanup after peer and worker failures`** that adds this file. Resolve its full hash with `git log --diff-filter=A --format="%H %s" -- docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md`.

Suggested reviewer request:

> Recheck the peer-exit reproduction and Task 3 cleanup independently. Verify all eight slots recover, no abandoned peer is submitted, repeated stop removes only owned descriptors, and a replacement is uniquely selectable with and without explicit PID. Fault a worker independently of peer validation; then combine that fault with a locked descriptor. Check cleanup retries and retained error causes. Check throwing cancellation callbacks on both stop and disconnect, including lifetime preservation until Quiesced and disposal afterward. Run All and readiness, verify new and original evidence hashes, and distinguish correction evidence from independent closure. Task 4, live Revit and desktop A8 remain outside this increment.
