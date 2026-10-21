# Task 3 exceptional-completion corrections — 2026-10-21

**Later independent review:** the peer-exit P2 was closed at `f79506d`; a new deadline-callback P1 prevented Task 3 closure. See the [deadline correction and fresh evidence](2026-10-21-deadline-cancellation.md). This record and its evidence remain historical.

**Implementing-agent verification; independent follow-up pending.** Baseline: `3e20251a966c826fea4d4c2968b6be9a43e97842`. The correction checkpoint is the commit titled **`fix: preserve bridge cleanup after peer and worker failures`** that adds this record. Locate it with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md`.

The [external feedback and separate disposition](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) retain the P2 finding and its underlying diagnosis. The original [Task 3 results](2026-10-21-local-pipe-transport.md) and all 30 original evidence files remain unchanged. They passed their supplied tests but did not cover this handoff.

## Reproduce before correcting

The first regression starts a real child on each runtime, pauses the existing acceptance observer, lets the child exit, then resumes production identity validation. Both targets failed: the eighth replacement connection timed out, and cleanup logged the same ArgumentException as the reviewer's reproduction. Two separate tests inject an unexpected worker failure through the clock interface, with and without a locked descriptor; both exposed skipped cleanup.

After those corrections, two further tests deliberately throw from a borrowed cancellation callback. Both runtimes exposed skipped deferred lifetime cleanup after disconnect; the owner-stop case also exposed skipped descriptor cleanup on net10. These are author-side extensions of the reviewer's diagnosis. All five new cases subsequently passed on both targets.

| Regression | Required observed behavior |
|---|---|
| Child exits before identity lookup | Zero abandoned-peer submissions; all eight replacement connections accepted concurrently; a valid subsequent request succeeds; two stops complete; replacement is uniquely selected with/without PID and serves a call |
| Unexpected worker fault | Network handles released and owned descriptor removed before original error is reported; repeated stop retains that error; replacement selection succeeds |
| Worker fault plus locked descriptor | First failure retains both causes and preserves the locked file; after unlocking, retry removes it while retaining the worker error; replacement succeeds |
| Cancellation callback throws during stop | Shutdown still releases its descriptor and reports the callback fault; outstanding callback resources survive until Quiesced, then dispose |
| Cancellation callback throws during disconnect | Monitor failure cannot skip deferred lifetime cleanup; resources survive until Quiesced, then dispose; shutdown retains the fault and removes its descriptor |

The new focused tests run as `BimAgentLab.Bridge.Tests.exe --pipe-review` **from the project directory** after building the dual-target test project. They are also part of Pipe and All. No test process is Revit; no models, accounts or client settings are changed.

## Full verification and retained evidence

- `Test-McpBridge.ps1 -Suite All`: **32 pipe cases per runtime**, 64 complete stalled-frame timings, **33 foundation cases per runtime**, bilateral/Python exchanges, 19 C# MCP cases, ten Python protocol/settings methods and four SDK/wire comparisons. Compatible desktop revisions and strict `2026-07-28`/legacy rejection remain tested.
- `Test-Readiness.ps1`: **18 core tests, seven Excel cases and process-isolated export**.
- Independent timing checks recompute all intervals, require every connection and successful successor, and reject six corrupted/incomplete copies. The expected case count is supplied independently: current default 32, `--expected-cases 27` for the untouched original capture.
- [Inventory](local-pipe-transport-review-evidence/inventory.json) retains exact bytes/hashes for the reviewer reproduction, new red/green logs, full regressions, timing summary, current assembly references and fresh exchange/protocol captures. [Timing summary](local-pipe-transport-review-evidence/timing-summary.json) records actual maxima. Original evidence hashes remain valid.

## Scope and lessons

Native identity normalization is narrow; unrelated ArgumentExceptions are not globally swallowed. Shutdown separates cleanup from error reporting: a faulted task has stopped using its resources but may still report an error. Retrying cleanup does not erase that error or grant permission to remove changed files. Monitoring failure likewise cannot bypass the RequestLifetime/Quiesced contract.

No new package, copied upstream code, SDK/protocol change, deployment or desktop registration was introduced. This is Windows IPC evidence with synthetic dispatch, not live Revit or A8 acceptance. Real alternate-logon and remote-machine tests remain unperformed. Task 4 is unstarted; independent Task 3 closure and full R3 remain pending. See the [participant story](../../../../docs/tutorials/09-connect-the-bridge-processes.md#review-the-failure-path-not-just-the-fix) and [agent procedure](../../../../docs/how-to/build-and-verify-local-pipe-transport.md#cleanup-after-exceptional-completion).
