# Task 3 deadline cancellation correction — 2026-10-21

**Later independent review:** deadline P1 closed at b334385; held callbacks still blocked stop/disconnect. The [progress correction](2026-10-21-cancellation-progress.md) supersedes the synchronous notification mechanism. All evidence below is preserved.

**Implementing-agent verification; independent P1 follow-up pending.** Baseline: `f79506defedd020f0ac3c5aaacc76f8c74c510e7`. The correction checkpoint is the commit titled **`fix: contain cancellation callback failures across timer boundaries`** that adds this record. Resolve it with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-deadline-cancellation.md`.

The author forwarded independent closure of the earlier P2 and a new process-terminating P1. [Original feedback and separate disposition](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-deadline-review.md) preserve both. The previous 30 + 32 Task 3 evidence files remain unchanged; their supplied tests passed but omitted deadline-triggered callback failure.

## Reproduction and regression design

Before changing production code, four new child cases ran on each target: normal cancellation and early completion passed; throwing and held callbacks failed. The throwing child terminated with an unhandled AggregateException. The held case also exposed lifetime disposal while a cancellation callback still used its source. After the first correction, deliberately mutating diagnostic data exposed lost error retention. Finally, separately canceling upstream and owner tokens on foreign timer threads reproduced unhandled callback failures through the remaining linked source. Each failed observation is retained; the latter two are implementing-agent extensions of the reviewer's diagnosis.

All six final cases run in **separate processes**. A parent requires exit code zero, empty stderr and a completed proof marker; a timeout fails the case and kills only that owned test child. The child suppresses Windows crash dialogs, not exceptions: there is no global unhandled-exception handler or forced successful exit.

| Child case | Required behavior |
|---|---|
| control | Real pipe request expires on its existing budget; two normal callbacks run; no late response; source lives until executor Quiesced; shutdown/replacement succeed |
| throw | Same real pipe path with two throwing callbacks; process survives, both errors retained, diagnostic emitted, repeated stop cleans up and reports error, replacement succeeds |
| held | Callback remains active after Quiesced and network stop; source remains usable; after release, both errors are retained and late Stop observes them; eventual disposal/replacement succeed |
| complete | Real pipe request completes early; lifetime disposes, original deadline passes without invoking callbacks or emitting a cancellation failure |
| upstream-timer | Direct production lifetime canceled by an upstream source's timer while original request budget is unexpired; throwing held callback survives Quiesced, then cleanup and replacement succeed |
| owner-timer | Same test with the owner's stopping token, independently of request deadline expiry |

The four deadline cases use a short wire deadline and deliberately spend 250 ms before submitting. They assert the same DeadlineUtc, actual budget expiry and DeadlineExceeded, timely wakeup, and no response bytes after expiry. Throwing cases snapshot, mutate and throw from the diagnostic observer, proving retained errors do not depend on borrowed event data. Borrowed-token cases isolate the additional lifetime boundary; they do not claim their initiating cancellation traveled through a pipe.

## Verification and evidence

- The unchanged reviewer reproducer was copied into private scratch, rebuilt against the correction and rerun. Both throwing children and both normal controls reported `CHILD_EXIT=0` and `SURVIVED server deadline`; throwing cases retained the original error through Stop. These are implementing-agent reruns, not a new independent review.
- Six focused child cases pass on each runtime; **Pipe has 38 cases per runtime**. All 64 stalled-frame intervals retain independent timing/completeness checks.
- All retains **33 foundation cases per runtime**, bilateral and Python exchanges, **19 C# MCP cases**, ten Python protocol/settings methods and four SDK/wire comparisons, including strict `2026-07-28` and legacy rejection.
- Readiness retains **18 core tests, seven Excel cases and process-isolated export**.
- [Inventory](deadline-cancellation-evidence/inventory.json) hashes exact reviewer-supplied reproducer/output, red/green observations, full regressions, timing summary/negative controls, environment and fresh protocol/exchange evidence. Reviewer code is retained only as attributed evidence, not production implementation.
- [Timing summary](deadline-cancellation-evidence/timing-summary.json) records measured maxima. The verifier rejects corrupted timing/completion data and requires an independent expected case count: current default 38; historical captures require explicit 27 or 32.

From repository root run `Test-McpBridge.ps1 -Suite All` and `Test-Readiness.ps1` via their project scripts. Focused executable commands and ownership details are in the [agent guide](../../../../docs/how-to/build-and-verify-local-pipe-transport.md#contain-cancellation-at-every-origin). The [participant story](../../../../docs/tutorials/09-connect-the-bridge-processes.md#ask-who-runs-the-callback) explains the author's engagement.

No new dependencies, copied upstream runtime code, SDK/protocol changes, installation, desktop configuration or model edits. Tests use Windows pipes with synthetic executors, not Revit. Actual alternate-logon/remote-machine tests, Task 4 and full live Revit/desktop A8 acceptance remain ahead. The latest independent finding is corrected by the implementing agent, not yet closed by the reviewer.
