# Task 3 cancellation progress correction — 2026-10-21

**Implementing-agent verification; independent P2 follow-up pending.** Baseline `b334385f1c1b74b8f204058e1c2b98c092f818d8`; the deadline P1 is independently closed there. Before production changes, the coder committed the [execution/waiting model](../../../../docs/explanation/bridge-execution-and-waiting.md) at `373e5a8`. The correction is the commit titled **`fix: separate bridge cancellation signaling from callback execution`** that adds this record; locate it with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md`.

## What was reproduced

The unchanged reviewer program, rebuilt privately, showed StopAsync blocking before returning its task and disconnect recovering only seven slots on net48/net10.0. Both cleared only after callback release. The [review packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md) separates reviewer feedback, conceptual diagnosis, coder response and requested independent follow-up.

The initial 14-case matrix failed ten cases on each runtime; four deadline cases passed. The matrix tests real endpoints across stop/disconnect/deadline, held-return/held-throw, and Quiesced before/after network completion, plus capacity and client paths. It requires progress while the release gate is still closed. The additional exceptional-Quiesced test first failed with Busy on the ninth request despite physical disposal; it then passed after separating the execution error from permit release.

The original foundation/throwing-stop tests initially failed two assumptions in the revised design. Their logs are retained. Immediate rejection of a new read remains mandatory, now through authoritative Context state; public-token callback notification can follow asynchronously. Stop cannot be required to wait for a future callback error; tests now require retained error identity when that notification reports. These are explicit interface/ordering refinements, not silently removed correctness checks.

## Result and repeat commands

The unchanged reviewer reproducer now reports, **before callback release**, StopAsyncReturned=True, StopCompleted=True, ConnectionClosedEvent=True and DescriptorExists=False for both origins on both runtimes. Disconnect restores eight simultaneous connections.

The full offline run passes **53 Pipe and 33 foundation cases per runtime**, bilateral/Python exchanges, **19 C# MCP cases**, ten Python protocol/settings methods and four SDK/wire comparisons including strict July and legacy rejection. Readiness passes **18 core, seven Excel and process-isolation checks**. No Revit, desktop UI, model, configuration or installation change occurred.

From repository root:

```powershell
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
python projects/revit-readiness/tests/verify_pipe_evidence.py projects/revit-readiness/docs/verification/cancellation-progress-evidence/pipe-output.txt
python projects/revit-readiness/tests/verify_cancellation_progress.py projects/revit-readiness/docs/verification/cancellation-progress-evidence/pipe-output.txt
```

From the project directory after the normal build, run each target's `BimAgentLab.Bridge.Tests.exe --progress-review` for the fifteen child cases. Parents require exit 0, empty stderr and completed proof markers. Timeout cleanup kills only the owned failed test child; it is never a successful progress result. The independent progress checker requires the whole matrix and all before-release/overload/error evidence; corrupted copies must fail. Timing remains a separate checker for the 64 stalled-frame intervals.

The [inventory](cancellation-progress-evidence/inventory.json) retains exact bytes and hashes for original reviewer materials, coder red/green reruns, initial contract-assertion failures, exceptional-completion red evidence, full regressions, independent summaries/negative controls and fresh protocol/exchange captures. Prior **104 Task 3 evidence hashes** remain unchanged, as do the 47 foundation evidence hashes. Reviewer code is copied only as attributed evidence/private reproduction, not production code.

## Boundaries and handoff

Eight retained requests per shared runtime/AppDomain bound endpoint notification workers and deferred lifetimes even across endpoint restart. When all remain held, network connections can still be accepted and rejected with retryable Busy. The system cannot safely force arbitrary callbacks to return; capacity recovers when physical users release their resources. Standalone RequestLifetime owners must bound their own admissions; Task 6 must preserve that obligation.

Context.ThrowIfInactive/IsCancellationRequested are authoritative immediately. The public Cancellation token serves cooperative executor notification. Runtime transport uses a private signal so notification order cannot obstruct network progress. Trusted queue admission and diagnostic sinks must remain nonblocking. These implementation limits are documented in the model and [agent guide](../../../../docs/how-to/build-and-verify-local-pipe-transport.md#prove-progress-separately-from-safety).

No new dependencies, copied framework implementation, SDK/protocol changes or normative acceptance-policy changes. The new implementation details and changed ordering require independent review. The [participant story](../../../../docs/tutorials/09-connect-the-bridge-processes.md#ask-what-must-progress-while-work-is-still-running) explains how the author demanded the missing model. Task 4 and full Revit/desktop A8 acceptance remain ahead.
