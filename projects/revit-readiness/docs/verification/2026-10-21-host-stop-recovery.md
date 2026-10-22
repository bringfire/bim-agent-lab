# Fresh recovery and Stop with queued requests

Local date: October 21, 2026; raw timestamps are October 22 UTC. This is an implementing-agent verification checkpoint. Task 7, desktop A8 and external R3 review remain open.

## Recovery after the preceding Stop

The author replied “started.” Actual Revit PID **198452** created session **440e441f0ca74e04b4435bd62db85c8b**, replacing the stopped session **366f623ba0754e7a9cb2d1dd2d90d7ce**. A fresh official-SDK request returned a report matching every previous baseline field except `GeneratedUtc`; both modified flags remained false. The trace records new hooks, context acquisition, reader entry/exit and quiescence. The old token had expired, so this proves fresh-session recovery, **not isolated restart-token invalidation**. The earlier unexpired-token test remains separate evidence.

The installed diagnostic assembly remains source **7b09879**, SHA-256 **432c5337625124a0ebdebba9d5dc98526379506e73a7f16d5b806e237101b057**. The production host, SDK **2.2.0**, compatible protocol policy and strict July tests were unchanged. All calls use the existing official-SDK readiness driver; no SDK source, dependency or hand-written MCP transport was added. TypeScript SDK 1.26.0 remains precedent-only.

## An observer can make the same waiting mistake

The existing diagnostic controls support two stages. The new [queued-Stop observer](../../tests/host-diagnostics/Observe-QueuedStop.py) first holds a request at Preparation, admits three additional requests with an already acquired context, then releases Preparation into Reader/Stop. The unchanged Stop runs on Revit's API thread while the first reader remains held. Original deadlines and 45-second safety holds are preserved.

The first attempt failed the observer's expectation. All three queued requests were canceled without execution, and network cleanup completed, but the observer waited for `EventsUnhooked` while still holding the API thread. Its bounded failure cleanup eventually released that hold; hook removal then followed. This capture remains under [failed-queued-stop](host-stop-recovery-evidence/failed-queued-stop/observation.json), with [eventual trace](host-stop-recovery-evidence/failed-queued-stop/eventual-trace.tsv) and the [original observer source](host-stop-recovery-evidence/observer-before-correction.py). It is **failed capture evidence**, not a retrospectively relabeled pass.

Inspection of unchanged [BridgeApplication.Stop/OnIdling](../../src/revit-addin/BridgeApplication.cs) and [HostBridgeSession retirement](../../src/bridge-runtime/HostBridgeSession.cs) explains the dependency. Stop polls retirement without waiting. Retirement includes network-task completion. If that task is still pending when Stop polls, the existing Idling callback finishes retirement and removes hooks when API execution resumes. In the failed trace, event disposal and Stop return preceded network completion. Requiring hook removal before releasing the held API callback made the observer block the completion it required.

The correction changes only the observer: require network completion, event disposal and queued quiescence while held; release the reader; then require hook removal in API context before finishing the connected-client observation. Hook removal may also finish during Stop if its initial retirement poll succeeds. This is a waiting-order correction supported by source and trace, not a relaxed requirement to omit cleanup.

## Repeated capture

The author started the bridge again, producing session **35592d2a41eb459da61c4a6341e08acc**. The corrected capture records:

- Three queued requests, each with zero executor and reader entries and quiescence before the blocking reader was released.
- Network completion and native event disposal while the reader remained held; Stop returned without waiting for that read.
- Explicit reader release before its original deadline, followed by reader/executor exit, quiescence and hook removal on the API thread. No automatic safety release or failure cleanup was used.
- Four correlated, retryable `BridgeUnavailable` results. All four SDK sessions remained connected for at least three seconds after release, successfully listed the tools, and received no late or duplicate result.
- An absent descriptor with Revit still alive after Stop.

The API-thread hook removal is directly observed. Its execution through the existing Idling retirement path is inferred from the source and ordering; the trace does not name the event callback. This test invokes Stop through diagnostic code. It does not establish application shutdown with queued work, an in-flight native Raise at final shutdown, or physical request-lifetime disposal.

## Normal shutdown and restoration

The author then reported “revit is closed.” The diagnostic trace records `ApplicationShutdownEntered` and `ApplicationShutdownReturned` on the API thread; the process and descriptor are absent. This was **idle shutdown after Stop**, not shutdown with queued work. Four saved model/checklist files match their previously recorded closed-file hashes, and no diagnostic arm or recorder error remains.

Before restoring, opened-file handles verified the original filesystem context: both recovery receipts, backup bytes and the old server still resolve into Codex's package cache, while the add-in DLL resolves directly to Roaming AppData. Both private receipts match their pinned records; all backup/staged hashes were checked before mutation. Recovery ran in that same view, newest receipt **74e482b3f82e491283e66ed12eb6b8d9** first, then **b0d82098d961433f9945d10fc9ebfa55**. Each stage independently matched all 97 target hashes, including inventories. Both receipts now say `Restored`; no directory deletion was needed. Original recovery bytes remain intact.

The installed add-in is again the normal **9a93f6d** candidate, DLL SHA-256 **ff8a2d12c89aa15f44aea6ab6943a047d9482a98c23e34fe6b44e116e876fdd4**. Revit remains closed. This restoration does not correct the old server's redirected location or establish desktop access to it. The [physical-path check](host-stop-recovery-evidence/pre-restore-physical-paths.json), [first restoration](host-stop-recovery-evidence/after-first-undo.json), and [normal restoration](host-stop-recovery-evidence/after-normal-restoration.json) preserve that distinction.

## Verification and remaining work

[verify_host_stop_recovery.py](../../tests/verify_host_stop_recovery.py) compares complete raw SDK exchanges, original context/session identity, baseline report data, trace prefixes, exact request counts, thread identity, original deadline and Stop/cleanup ordering. It is separate verification code by the implementing agent, not an independent reviewer or the Task 8 BIM oracle.

The restart checker first rejected none of six corrupted cases; implementation then passed those controls. Two further controls change report values consistently in both saved data and raw wire. Fifteen queued-Stop corruptions then failed before their checks were added, followed by seven shutdown/restoration corruptions before their comparisons were added. All **31 evidence tests** pass, including 30 negative controls. Five observer ownership tests cover claimed and unclaimed arms, same-named foreign replacements and preserving an existing release; four failed against the initial no-op cleanup, then all five passed. [Retained logs and hashes](host-stop-recovery-evidence/hashes.json) preserve those stages.

Run from the repository root:

```powershell
python projects/revit-readiness/tests/verify_host_stop_recovery.py projects/revit-readiness/docs/verification/host-stop-recovery-evidence
python -m unittest discover -s projects/revit-readiness/tests -p test_verify_host_stop_recovery.py
python -m unittest discover -s projects/revit-readiness/tests -p test_queued_stop_observer.py
```

Source changes in this increment are limited to diagnostic observers, evidence checks and teaching documents. Installed files changed only through the two verified restorations above. The previous strict-July/compatibility results remain retained; they are not represented as new desktop runs. Preserve the earlier intermittent notification test and token-only SDK cancellation observations as unresolved. Shared server/recovery visibility, remaining host-shutdown observations, five sample comparisons, both desktop applications and R3 remain ahead. Do not replay old redirected recovery receipts from a different filesystem view.

The [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#question-the-test-as-well-as-the-plugin) focuses on directing the agent; the [diagnostic procedure](../../tests/host-diagnostics/README.md) gives the execution and ownership rules.
