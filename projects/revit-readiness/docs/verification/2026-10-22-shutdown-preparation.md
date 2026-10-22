# Shutdown and recovery preparation — October 22, 2026

The author requested offline preparation with Revit closed before the next human UI step. Preparation was committed at `32c44ec`; the actual queued shutdown/disposal check below subsequently passed. Task 7 and R3 remain open. See the [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#prepare-shutdown-tests-before-asking-the-person-to-open-revit) and [agent procedure](../../tests/host-diagnostics/README.md#actual-application-shutdown-with-queued-work).

## Intermittent notification test: a completed request has no live borrower

The old test registered a public-token callback on a **queued** request and required that callback to run during Stop. `HostBridgeSession.StopInApiContext` first cancels the queue, completing that request's Response and Quiesced tasks. The endpoint can retire it and dispose its lifetime before endpoint Stop signals owner cancellation. Completed work no longer requires notification. This was an incorrect test assumption, not evidence that an executing borrower lost cancellation.

A deterministic test pauses the test endpoint between those production boundaries until physical disposal occurs. Both runtimes failed the old expectation. The corrected test requires canceled response, quiescence, zero executor calls, no notification after disposal, event disposal, removed descriptor and retired host. The held-callback test now enters the production queue's executor before registering its callback and stopping the host. It verifies immediate invalidation/final-guard rejection, network/event retirement while notification remains held, Quiesced after executor return, and physical disposal after callback release. Assertions are rethrown outside the queue, which intentionally maps executor faults.

Initial reproduction logs used the wrong working directory and also failed a source-path check. They are retained; the corrected-directory runs supply behavioral RED evidence. The separate token-only C# SDK cancellation observation remains unresolved. Explicit cancellation-message success does not explain it; no SDK correction is claimed.

## Disposal observation and actual close

The runtime emits correlated `LifetimeDisposed` after physical disposal succeeds and the retained-request permit is released. Cancellation/retirement ordering, original budget and admission bound are unchanged. Twelve endpoint progress combinations within the existing fifteen child-process cases reject disposal while callbacks remain held and require one matching event afterward. The focused child case failed before the event was added.

Revit UI close cannot run concurrently with a synchronous reader on the same API thread. New **diagnostic-only** controls hold actual `ApplicationClosing`, admit three queued requests, release that event into actual `OnShutdown`, then hold briefly after production Stop. The observer requires network completion, descriptor removal and three physical lifetime disposals while Revit remains alive, then releases the event and waits for natural exit.

Each control is consumed once, accepts at most 45 seconds, rejects stale releases and releases automatically if the observer fails. Automatic release means failed evidence. Normal builds contain neither lifecycle gate. A successful live run would prove queued-work shutdown, not concurrent reader execution or in-flight native Raise overlap. Synchronous diagnostic logging cannot establish production timing guarantees.

The observer opens a SYNCHRONIZE-only process handle, never terminates Revit, owns only its controls/SDK children, and preserves unsuccessful captures. Cleanup failure does not skip child cleanup or remaining evidence. The independent checker checks exact request identities, original token arguments, SDK wire exchanges, API-thread ordering, absence of executor/reader entry, disposal and natural exit. Current checker tests use labelled synthetic traces and ownership fixtures; they are not live host proof.

## Offline verification and provenance

Logs and exact-byte source snapshots are listed in [hashes.json](shutdown-preparation-evidence/hashes.json). Initial failures, compilation/syntax mistakes and corrected results are retained separately.

| Check | Observation |
|---|---|
| Full MCP/bridge suite | Pass: 72 Tools, 88/83 Dispatch, ten adapter handoff, 53 Pipe and 33 foundation cases per applicable runtime; protocol/settings/runner tests include strict July and legacy rejection |
| Final focused Dispatch | Pass on both runtimes, ten adapter handoff cases and seven Python shutdown-checker/ownership methods |
| Diagnostic controls/API compilation | 21 controls per runtime; normal and diagnostic Revit API builds pass; helper types absent from normal assembly |
| Readiness/Excel isolation | 18 readiness, seven workbook cases and isolated exporter pass |
| Recovery audit | 97 installed targets match; backup/staged bytes for three shared receipts verify; reverse lineage compatible. Read-only audit is **not actual undo** |

No dependency/protocol change. Official C# MCP SDK 2.2.0 remains consumed by the external server/test client; TypeScript 1.26.0 remains precedent-only. Existing computation is reused. Controls, observation, tests and observer are original lab code. The diagnostic package compiles this increment's add-in and net48 runtime, retaining installed core/contract/exporter/notices and server `d88de67` bytes. Restoration returns the normal host to `9a93f6d` bytes; diagnostics must not silently become the normal installation.

## Next actions and remaining limits

Whole-package preflight covers 97 operations. Only the diagnostic add-in, runtime and main inventory require different content. Omit ordering-only fixture/server inventory differences only after comparing complete entries/hashes. Preserve whole-install verification and durable backups before applying those three targets. Record actual apply/restore/reapply separately rather than counting preparation as execution.

Before requesting Revit startup, prepare the shared run directory and selector under `%USERPROFILE%/.bim-agent-lab/RevitBridge/diagnostics`, preserve previous selector bytes, and verify physical opened-file paths. The human opens the disposable saved-as sample and starts the bridge. Establish a fresh session/positive read; only after the observer writes `ready.json` ask for normal Revit closure. Never reuse the historical PID/session.

At preparation time the live observations were pending. The follow-up below supplies queued shutdown/disposal, positive wrapper, actual diagnostic-layer restoration and normal-host recovery. Token-only SDK cancellation, remaining native-event/reader shutdown observations where feasible, historical shared-layer undo, consolidated acceptance and R3 remain open.

## Actual installation round trip and shared startup preparation

With Revit closed, the agent applied diagnostic source `32c44ec` using the full 97-target preflight and a three-target prepared receipt `c75c9914405e4174a811090a4000f25e`. A separate PowerShell process restored it; all 97 targets matched the original baseline. Repeating that restore also passed. A new receipt `3121de92cda74c6f8d1c9ac0a78967ca` reapplied the same candidate, with all 97 effective target hashes verified. The two unrelated inventories had only ordering differences and were retained after full entry/hash comparison. Active desktop servers and their binaries were preserved.

[Deployment record](shutdown-preparation-evidence/deployment-roundtrip.json) retains both receipt hashes and backup/staged hashes. It records their states at that moment: first Restored, second Committed. This is actual undo of the new diagnostic layer, not of the three earlier whole-install layers. Their audit remains separate.

The agent prepared shared diagnostic run `21cdbb0b351f4e47af1d0031678a51ea`, preserved the old selector bytes before replacement, and verified 99 actual opened-file paths (97 targets, selector and receipt). The human then started Revit normally with the disposable saved-as sample. Startup reached the diagnostic logger without the earlier redirected-filesystem error. Actual consuming-process startup supplements agent-side path checks.

## Actual queued application shutdown and positive wrapper

Revit PID **18564**, session `0fa3b39a68d944e8b0372b1f62858027`, loaded the diagnostic add-in/runtime while the external server retained `d88de67` bytes. A fresh frozen expectation came from the original native schedules/checklists and authored oracle. Initial preparation incorrectly required the saved-as checklist hash to equal the original complete model's hash; before any MCP query, comparison established that only `ProjectFileName` differs. The corrected preparation explicitly binds that filename and checks the entire remaining object. No expected answer was derived from the live report.

`Invoke-ReadinessMcp.ps1` then passed its **first positive live run**: 16 examined instances, zero findings, both completeness flags true, both modified flags false, exact report/wire comparison on `2026-07-28`. The wrapper's frozen inputs and SDK capture are retained under `live/baseline-wrapper/` in the evidence folder. This is diagnostic-host wrapper proof, not a fresh desktop acceptance claim.

After the observer published `ready.json`, the human closed the entire application normally. The independent shutdown checker passed:

- Actual `ApplicationClosing` and `OnShutdown` occurred on API thread 1.
- Three requests were admitted while Closing was held; none entered an executor or reader.
- All three quiesced, returned correlated retryable `BridgeUnavailable`, and recorded physical lifetime disposal while Revit remained alive.
- Stop disposed the ExternalEvent and removed hooks; network cleanup completed and the owned descriptor disappeared before final release.
- Revit exited naturally; all three SDK children exited 0. No forced termination, automatic safety release, diagnostic failure or cleanup-release fallback occurred.

The capture ran from 16:43:15Z to 16:44:08Z. Its before/after prefixes, raw wire, exact request IDs and independent check are under `live/application-close-01/`. A premature checker invocation found no final observation file while the observer was still finishing; only the completed capture was accepted. Physical disposal can precede the asynchronously logged Quiesced observation; the checker does not infer the opposite timestamp ordering.

Observed native Raise during closing returned Accepted before shutdown began. **No in-flight Raise overlap or concurrent executing-reader shutdown is claimed.** The diagnostic holds make cleanup observable; they do not establish how much uninstrumented process-exit time is available to deferred work.

After closure, the second receipt restored the normal host and the previous selector bytes. The final audit again matched all 97 original targets. The actual RVT/checklist bytes are checked separately from in-memory modified flags. Normal-host startup/read recovery is a subsequent check, not inferred from restored hashes.

## Normal-host recovery after actual shutdown

Revit restarted as PID **154284**. The first two SDK context attempts correctly returned retryable `BridgeUnavailable`; no descriptor existed yet. The journal showed the application/command registered, and the human then confirmed starting the readiness bridge. Preserve those attempts as setup evidence, not as successful reads or an unexplained host defect.

The new session `380928ed403d4fbf9731b2bbebb145e3` published after Start. A new independent expectation was frozen against that PID/session before the recovery query; both RVT/checklist hashes still matched the pre-shutdown preparation. The normal-host wrapper passed on `2026-07-28`, examining 16/16 with zero findings, both completeness flags true and both modified flags false. This is fresh normal-host recovery, not reuse of the diagnostic session's token or report.

Current installation is again the original normal host `9a93f6d` plus external server `d88de67`; both new diagnostic receipts are Restored. Source `32c44ec` includes the diagnostic observations/tests. Desktop configuration was not changed to the new PID during this SDK-only check, so the existing desktop entries still require deliberate rebinding for their next live session. Revit remains open at this checkpoint; no additional human UI action is needed for this increment. Future agents must inspect actual process state rather than reuse these PIDs.
