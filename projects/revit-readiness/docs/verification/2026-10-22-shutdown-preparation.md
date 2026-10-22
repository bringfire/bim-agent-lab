# Shutdown and recovery preparation — October 22, 2026

The author requested offline preparation with Revit closed before the next human UI step. This record is not a new Autodesk shutdown observation. Task 7 and R3 remain open. See the [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#prepare-shutdown-tests-before-asking-the-person-to-open-revit) and [agent procedure](../../tests/host-diagnostics/README.md#actual-application-shutdown-with-queued-work).

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

Pending: actual queued shutdown/disposal, positive live wrapper, token-only SDK cancellation diagnosis, remaining native-event/reader shutdown observations where feasible, historical shared-layer undo, normal-host restoration after diagnostics, consolidated acceptance and external R3.
