# Execute the historical shared-install recovery

October 22, 2026. The three historical shared installation layers were actually restored, each undo was repeated successfully in a new process, and the exact tested build was reinstalled. All **97 final target hashes** match the captured baseline. This advances the [read-only preparation](2026-10-22-shared-recovery-preparation.md); it does not by itself prove startup inside Revit or close R3.

## Preconditions and ownership

The author reported **“claude closed Cdeox MCP disbaled.”** Fresh process inspection found no Revit or installed readiness-server process. The agent rechecked the 97 current targets and their baseline snapshots, all 83 archived package files, all three receipt checksums and backup/staged files, and 100 opened-file locations. All locations resolved directly. No application was forcibly stopped.

The source package remained the archived normal host **9a93f6d** and external server **d88de673996520d86c3502f357880865d1037fb9**. Current build outputs were not deployed. The official C# SDK remains **2.2.0**; TypeScript SDK 1.26.0 remains precedent only. No dependency or product-code change accompanied this recovery.

## Actual restore and reinstall

The existing `Undo-McpBridgeInstall.ps1` ran in a separate PowerShell 7 process for each undo and repeat, under its exclusive installer lease. Independent Python comparisons checked all 97 target states after every process, including expected absence, inventory payload hashes, temporary-file absence, receipt states and created-directory removal.

| Order | Historical transaction | Verified outcome |
|---|---|---|
| 1 | `68dac9adc1c04f0694a12acfda4e9a6c` | Restored; repeat leaves target bytes and receipt state unchanged |
| 2 | `2ccfb37b641e4a7f93391f6788dde437` | Restored; repeat leaves target bytes and receipt state unchanged |
| 3 | `a88dbf5d84b94188a695e2dc73b6e954` | Restored; repeat leaves target bytes and receipt state unchanged; owned shared server directory and its `third-party` directory absent |

The installer then calculated a **new** whole-install plan against that restored state. Desired bytes were compared with the captured baseline, rather than replaying a historical receipt. Generated inventory maps were checked; this run needed no inventory representation substitution. `Save-McpDeploymentRecovery` durably prepared backups/staged bytes before `Invoke-McpDeploymentPlan` applied the installation, using the same lease.

New committed recovery transaction: **`ef71fb608bba47f49a43ee75fb226bec`**, beneath `%USERPROFILE%/.bim-agent-lab/RevitReadiness/InstallRecovery`. Its 97 operations include **32 changed targets**, including **30 previously absent files**. Independent inspection verifies all **164 backup/staged files**, exact before-state agreement with the final undo, all 97 final target hashes and all three installed inventories. Opened-file checks cover the 97 final targets and new receipt: **98 direct locations**.

Keep all original receipts and their private bytes. The three historical transactions are now `Restored`; they are historical evidence and must not be replayed against this new installation. The new committed receipt owns the current installation's recovery. No client settings were edited by the agent during this sequence.

## Verification and capture limits

The actual installed server was launched by the official SDK client on **2025-06-18, 2025-11-25 and 2026-07-28**. Each run completed discovery, listed the two production tools, and returned correlated, retryable `BridgeUnavailable` with no report while Revit was closed. An independent Python read of raw traffic matched the saved structured/text results, protocol negotiation and request/response identities. These are SDK checks, not new desktop A8 evidence.

The first broader Protocol regression run passed its C# checks but timed out after 15 seconds in `test_existing_release_is_rejected_before_capture`. The unchanged test passed alone in 2.297 seconds. Preserve both logs; this observation does not establish a root cause or a fix. The full unchanged repeat passed, including strict July/legacy rejection, 34 independent Python tests and the three standalone SDK probe exchanges. This is successful retesting, not an explanation or repair of the intermittent timeout.

One auxiliary capture was defective: `reinstall-plan.json` used PowerShell `Select-Object` on dictionaries and recorded null operation fields. It is retained as unusable plan-summary evidence. The production plan was unaffected: the checksum-verified immutable installer receipt contains every actual target, before/after hash and backup path. The independent audit compares that receipt against the original preparation maps and installed bytes. A successful command or an apparently populated JSON array is insufficient evidence unless its fields are inspected.

The [retained execution files](shared-recovery-evidence/execution/hashes.json) include original/repeated undo logs and audits, immutable receipts with state/journals, exact one-off orchestration scripts, protocol traffic and verification summaries. Binary packages and recovery bytes remain private and ignored. These machine-specific scripts describe this run; participants should use the supported setup/undo scripts and their own receipts, not replay these paths.

## Next live handoff

The author has been asked to open Revit 2024, open the prepared `BAL-info-saveas-fresh.rvt`, and explicitly start the readiness bridge. A fresh PID/session and context are required. Keep the desktop connection disabled until any intended binding refresh is recorded; an old configured PID cannot identify a restarted Revit process.

The next section supplies the fresh startup/read follow-up. Native-event/final executing-reader overlap limits, the diagnosed token-only SDK cancellation limitation, desktop Stop behavior and consolidated independent R3 remain separate. The [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#prove-recovery-when-the-applications-are-available) explains how the person coordinated this work; the [agent handoff](../../../../docs/how-to/agent-handoff.md) carries the current recovery ownership.

## Fresh Revit recovery passes

After checkpoint `ef47b20`, the author replied **“ready.”** Actual Revit PID **178492** published bridge session **`705886aad28f45cf9b22df62da642aae`**. The new expectation was frozen before any readiness call, using the authored independent oracle and original native schedules/checklists. Model/checklist and oracle-source hashes matched their recorded values. The Save As checklist was compared completely with the original complete-case checklist after changing only its declared filename.

The supported `Invoke-ReadinessMcp.ps1` wrapper then ran against the unchanged installed server, with the fresh PID/session and protocol **2026-07-28**. Its context guard checked the active full path and checklist before releasing the readiness call. The independent full report/wire comparison passed: **16 requested/examined, zero findings, zero affected instances, both completeness flags true**, no scope issues or unexamined cases, and both modified flags false. The saved RVT/checklist bytes remained unchanged; all 97 installed targets and the new receipt's backup/staged bytes still verified. This proves actual startup and a fresh read after the historical recovery, not merely file equality.

Both desktop entries still selected historical PID149972. The agent prepared two private durable backups before either edit and changed only the unique quoted PID to178492. Parsed full configurations were compared with an expected object containing exactly that change; all other bytes were unchanged. Codex's user-selected `enabled=false` stayed false. Claude was not launched. Opened-file verification confirmed both configuration paths were direct. These receipts describe a PID edit, not the older helper's registration/removal operation; do not use its removal restore on them. Preserve any concurrent edits during future recovery.

The [27 exact live evidence files](shared-recovery-evidence/live/hashes.json) include the frozen expectation, SDK wire/report/comparison, read-only post-run audit, saved-input hashes and a sanitized binding-change summary. Full client configuration backups remain private. The original [66-file execution record](shared-recovery-evidence/execution/hashes.json) is unchanged, including its historically pending live field and initial regression failure.

No new desktop call after this PID refresh is claimed. The human can reopen Claude and enable Codex's readiness MCP when convenient. Revit remains open with the sample and bridge active at this capture. A future Revit restart needs a fresh binding; do not reuse these numeric identities. Both teaching layers now distinguish successful installation recovery, actual host recovery and desktop reconnection. Full Task7/R3 closure is still separate.
