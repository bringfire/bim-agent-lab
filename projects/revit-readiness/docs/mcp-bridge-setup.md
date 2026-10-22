# Build, install and recover the local Revit bridge

Task 7 working procedure. Task 6 is independently closed at b4cdb1f; installation and live-host acceptance are being verified separately. Follow the [operation model](../../../docs/explanation/recoverable-readiness-installation.md), [approved Task 7 plan](../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-7-explicit-deployment-rollback-and-live-lifecycle-checks), and [host checklist](../tests/mcp-host-acceptance.md).

For the active shutdown increment, use the [preparation/recovery record](verification/2026-10-22-shutdown-preparation.md) and [diagnostic procedure](../tests/host-diagnostics/README.md#actual-application-shutdown-with-queued-work). A temporary diagnostic add-in/runtime must have its own verified restore receipt. Whole-package preflight remains required even when only changed host files are applied. Do not overwrite or terminate active desktop servers to install this instrumentation.

The [real-installation record](verification/2026-10-21-live-installation.md) now distinguishes verified install/undo/reinstall and initial live requests from outstanding lifecycle and desktop checks. Use the actual installed executable name, `BimAgentLab.RevitReadiness.Mcp.exe`; the installer does not create a shorter `BimAgentLab.Mcp.exe` alias.

Use 64-bit PowerShell 7, the existing .NET SDK/10 runtime, Revit 2024 and its net48 API references. This increment adds no package dependency and does not change SDK 2.2.0 or the compatible/strict-July policy. The existing core, Excel helper and fixture remain separate from the external modern server.

**Current deployment status:** the [physical-path investigation](verification/2026-10-21-host-diagnostics.md#physical-path-investigation-established-the-cause) found the old server/recovery files redirected into Codex's private AppData cache. A [new installation outside AppData](verification/2026-10-21-shared-server-installation.md) now preserves the tested binaries and resolves directly in the agent's opened-file checks. Verify access from each intended desktop application before accepting deployment. Preserve old recovery bytes and never replay an old receipt from a different filesystem view. Desktop visibility remains required before A8; agent-side checks alone do not establish it.

From the repository root:

```powershell
# Close Revit before Deployment/All tests and installation; preserve user work normally.
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Deployment
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-McpBridge.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Install-Readiness.ps1 -IncludeMcpBridge
```

Build performs the existing readiness build plus locked framework-dependent publishing on Windows x64; the modern runtime is checked separately. Publishing uses the current Windows SDK apphost and x64 platform target without introducing another package/RID lock graph. The installer preflights everything before preparing recovery, including an already installed fixture core. `-IncludeFixtureTools` is still explicit; ordinary installation does not newly enable an editor.

Installed locations:

- `%APPDATA%/Autodesk/Revit/Addins/2024/BimAgentLab.RequiredInformation`: net48 add-in/core/bridge assemblies, isolated Excel helper and notices.
- The sibling `BimAgentLab.ReadinessBridge.addin`: application plus explicit Start and Stop commands, each with a permanent distinct GUID. Loading the application must not start a listener.
- `%USERPROFILE%/.bim-agent-lab/RevitReadiness/McpServer`: modern executable and SDK dependencies.
- `%USERPROFILE%/.bim-agent-lab/RevitReadiness/InstallRecovery/<transaction>`: immutable prepared receipt, staged bytes, original backup bytes, mutable state and journal. Keep this private local recovery material out of Git.

The runtime/recovery default moved outside AppData after [observed filesystem redirection](verification/2026-10-21-shared-server-installation.md). Revit add-ins and session descriptors retain their existing locations. `-RuntimeRoot <absolute-directory>` explicitly selects another server/recovery parent; `-SourceRoot <absolute-project-layout>` selects a prepared package instead of current build outputs. The same preflight and recovery logic applies. Check actual opened-file paths and the consuming applications; an override is not a guarantee against redirection. Before migrating an existing installation, settle its previous recovery operations in their original filesystem context and preserve the receipts. Changing the default does not migrate or reinterpret old receipts.

The installer prints the receipt path. Retain it even after success. For rollback with Revit closed:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Undo-McpBridgeInstall.ps1 -ReceiptPath '<absolute receipt.json path>'
```

Recovery verifies the whole plan and its backups before writing installed files. It restores original bytes and inventories, removes only recorded new files matching planned bytes and recorded empty directories, and is safe to retry after interruption. A foreign edit produces `RecoveryRequired` and retains evidence/backups; do not delete the receipt or overwrite that edit to force a success. An unfinished published receipt prevents a later install until recovery is resolved. A preparation directory without a published receipt never authorized target mutation.

Source boundaries: `McpDeployment.psm1` owns the lease, package/preflight, Prepared barrier, journaled apply and reverse restoration. Install/Undo wrappers retain the same lease through the operation and dispose in `finally`. The test harness can inject named faults inside the module; production scripts expose no fault flag. The build's SDK/source provenance is unchanged; deployment code is original lab code using PowerShell/.NET file APIs.

Tests use temporary installation/recovery roots and original byte/path snapshots. Exception tests and abrupt child-process exits are separate. Repeated undo runs in fresh processes. These tests do not demonstrate Revit API scheduling or model correctness. Do not present the optional sample editor, synthetic protocol probe or disposable installer fixtures as the production bridge.

After installation, live acceptance must establish no automatic listener, Start twice, Stop twice, restart with a new session, original-context invalidation, model-state errors, queued/modal cancellation and shutdown. Record actual installed hashes and returned errors. Both desktop applications and the five sample/control cases remain separate A8/Task 8 work. This installer never edits desktop client configuration.

## Legacy fixture and lifecycle client

An installed fixture without an inventory is accepted only when its expected manifest and both DLL hashes match `scripts/legacy-fixture-baseline.json`, whose hashes come from the earlier retained verification records. Otherwise stop before installed-file mutation and investigate the installation. The migration creates a journaled inventory; undo restores its original absence. This is not permission to infer ownership from arbitrary current bytes.

The official-SDK verification client now has a basic `readiness` scenario for Task 7. Example, after explicit bridge Start in Revit:

```powershell
pwsh -NoProfile -Command 'dotnet build projects/revit-readiness/tests/mcp-client/BimAgentLab.Mcp.Client.csproj -c Release -p:RestoreLockedMode=true'
# Use the actual current Revit PID and a new capture directory.
projects/revit-readiness/tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe --server '<installed server executable>' --scenario readiness --revit-pid '<PID>' --expected Context --evidence '<new capture directory>'
```

`--expected ReadinessReport` acquires context and requests the full report; it checks transport/result shape, not the independent BIM answer. An exact error-code expectation requires a correlated error with no report. `--context-token` checks a previously captured token without reacquisition. `--pause-file` acquires context, writes `context-ready.json`, and waits for a new release file so explicit test preparation can occur before checking. The SDK driver writes a completion marker only after client disposal. Keep full results, wire traffic and failed captures. Task 8 must still add independent report comparisons and actual desktop acceptance.

## Read the live checkpoint accurately

The first installed-build checkpoint captures 26 successful SDK exchanges plus two failed/partial setup attempts. Run `tests/verify_live_lifecycle_capture.py docs/verification/live-installation-evidence` from this project directory, then `tests/test_verify_live_lifecycle_capture.py` with Python. The checker compares saved results to actual wire data and compares original installation snapshots with both undo snapshots. It checks the captured July traffic; it does not substitute for the existing multi-version regressions or desktop A8.

The modal expiry attempt disposed its client before closing Revit's dialog. Do not reuse it as proof of no later reader entry or no late response on a still-connected client. The follow-up below retains the client after modal release; production read-boundary instrumentation remains pending. Instrumentation must be explicitly diagnostic, identify source/binary hashes, preserve the same guard/queue paths and be kept separate from production-binary observations. During preparation and executing-read controls, record cancellation requested, transport released, reader entered/exited, callback quiescence and disposal as distinct events.

UI setup is part of the test. Confirm the intended model or dialog is actually active before issuing its request. A failed click is not an opened model. Failed/partial captures stay immutable; deliberate retries use a new directory. The working Save As control uses a new model filename with a separately prepared sidecar, never overwrites an original sample, and requires a fresh unexpired pre-change context. All remaining gates are listed in the linked live record; Task 7 is still open.

## Follow-up lifecycle controls

The [updated live record](verification/2026-10-21-live-installation.md) contains 35 successful SDK captures, same-token value freshness/restoration, restart invalidation before token expiry, a connected timeout observation, native Stop with incomplete pipe input, ordinary application shutdown, manual exports and post-close hashes. The original 26-capture checkpoint and its limitations remain historical evidence. Run the capture checker and its now-16 tests using the commands above. Tests also independently decode the retained workbook with Python's standard library; no new runtime dependency was added.

`--finish-file <new absent path>` is a test-driver control, separate from `--pause-file`. After receiving the expected result, the driver writes `response-ready.json` and retains the official SDK connection. Release Revit's modal and observe the actual project UI, record that transition, then create the finish file. The driver waits at least three seconds and lists tools again before disposing and writing `complete.json`. `post-release-observation.json` records the window. Check wire response identity/uniqueness across the full capture. Neither a three-second traffic window nor successful disposal proves zero internal reader entries. The next diagnostic increment must still observe the read boundary. The entire driver remains bounded by its original seven-minute test timeout; file gates do not renew production request deadlines.

Reproduce driver controls with `python tests/test_mcp_readiness_driver.py` after building the client and server; the tests use an absent PID and cannot reach Revit. `Test-McpBridge.ps1 -Suite Protocol` includes them, together with strict July and legacy rejection. The driver preflights an absent finish file to avoid accidentally treating a prior run's release as new preparation. Only disposable failing test-driver subprocesses may be killed by the test watchdog; that is not a graceful-shutdown pass.

`tests/Observe-LivePipeStop.ps1 -RevitPid <PID> -Evidence <new directory>` opens three private-pipe connections without a valid request, then writes `ready.json`. Arrange native Stop immediately after ready; automation should prepare the observed Stop menu first and launch the observer with a hidden subprocess window. The observer requires EOF before 4.5 seconds on all connections, descriptor removal within a separate two-second wait, and Revit still alive. Ordinary five-second frame expiry cannot satisfy it. Run a deliberate no-Stop control and retain its expected failure. The first captured observer failed because it assumed EOF and descriptor removal were simultaneous; the retry corrects the test, not production cleanup. This observer is not an MCP implementation and does not access the Revit API.

For Save As, retain old/new path, project and lifetime identity plus the old token's expiry and actual test time. If manual preparation outlasts expiry, record the limitation and repeat with a fresh token before claiming Save As invalidation. For same-document edits, identify the report's contract field (`Code`, backed by Type Mark), compare observed values using the same token, and restore the disposable copy. After normal closure, compare the original source and working file hashes. Revit's modified flags and post-close disk hashes answer different questions; retain both.

## Coordinate a human-operated Save As test

The author chose to operate Revit. Respect that choice: no Computer Use is needed for this workflow. The agent owns preparation, SDK calls and evidence comparisons; the user owns native UI actions. Use one short instruction per checkpoint.

1. Have the user open the disposable source model and start its bridge, then wait for their ready reply. Identify the current process/session; never reuse the PID from a previous launch.
2. Choose an absent destination model and sidecar beside the source. Prepare the destination checklist **before acquiring context**, changing only `ProjectFileName`. Record both exact sidecar bytes/hashes, paths and intended action. Do not overwrite an existing model or an original sample.
3. Run the SDK client with `--expected Context` into a new capture directory. Confirm the returned source model, session and project identities. Preserve the exact token and its expiry.
4. Give the user the exact Save As filename and a practical local-time deadline before expiry. They save and leave the new project active, then reply `saved`. Keep time-consuming independent work outside this window.
5. Immediately call `--expected ContextChanged --context-token <original token>` in a second capture. Record the check's start and completion timestamps. Both must precede the original expiry; a test started before expiry but completed after it cannot establish the intended timing control. Do not reacquire context before testing the old token.
6. Acquire context and report with `--expected ReadinessReport` in a third capture. Verify the same process/session/project, changed path and document lifetime, prepared checklist hash, exact original-token use, false modified flags and expected sample contents. This continuity comparison supplements, but does not replace, Task 8's independent BIM oracle.

Keep UTC values offset-aware. With PowerShell 7 use `ConvertFrom-Json -DateKind String` before parsing an ISO timestamp as `DateTimeOffset`, or use Python `datetime.fromisoformat(value.replace('Z', '+00:00'))`. An initial display calculation lost the offset through PowerShell's automatic date conversion; it was corrected before any deadline was given to the user. Preserve the original timestamp string in evidence.

The [fresh-token run](verification/2026-10-21-live-installation.md#coordinated-fresh-token-save-as) adds three captures on a new Revit PID. The index explicitly identifies that run; older captures retain their original process. The checker now runs 24 tests, including seven corrupted Save As controls that failed before its new assertions were implemented and pass afterward. No production code, dependency, desktop settings or protocol policy changed.

## Diagnostic preparation for the remaining host checks

Use the [separate diagnostic host procedure](../tests/host-diagnostics/README.md). The approved host checklist permits explicit instrumentation; it does not permit treating an error response as proof of no reader entry. The diagnostic project links the add-in source with compile-time hooks; the normal project excludes the helper. Preserve original request identity/deadline, all production guards, bounded waits, and separate response, executor, network and cleanup observations. Source-linked instrumentation changes the diagnostic binary and its timing, so record exact hashes and restore the normal installation before final desktop/sample acceptance. No new SDK or third-party library is needed.

The corrected shared-directory startup and first preparation/started-reader deadline controls now have [actual Revit evidence](verification/2026-10-21-host-diagnostics.md#first-live-diagnostic-deadline-controls). Use the bounded observer and independent checker described in that procedure. Preserve the captured context to bind session/checklist identity: correlated errors expose request ID/code/message/retryable, not the successful result's session fields. Verify the checker with both unmodified evidence and corrupted copies. Do not infer physical disposal or other cancellation origins from these deadline outcomes. The normal-build restoration and shared server/recovery deployment correction remain prerequisites for final desktop acceptance.

The next [actual interruption record](verification/2026-10-21-host-interruptions.md) covers explicit cancellation/EOF before and during reads, three queued interruptions, and Stop while a reader is held. Tools now includes six driver controls, bringing it to 65 cases. The driver owns its child process streams and uses official SDK 2.2.0 typed APIs; EOF closes stdin without pre-canceling or disposing the SDK. Explicit cancellation targets an owned RPC ID through SDK notification serialization. Initial token-only calls failed to transmit cancellation in the recorded attempts and remain unexplained; preserve that limitation. Do not substitute token-local cancellation, process termination, deadline expiry or an earlier restart for the requested boundary. Normal restoration, shared-server/recovery identity and live desktop acceptance remain outstanding.

[Fresh restart and queued Stop](verification/2026-10-21-host-stop-recovery.md) now have separate evidence. `Observe-QueuedStop.py` uses the same client/server/PID/trace/evidence arguments as the earlier observers and requires exclusive operation. Its two existing stage arms hold Preparation to admit waiting work, then execute Reader/Stop. Wait for network completion and queued quiescence before releasing the reader; require final hook removal afterward, since it can require Revit's next Idling callback. The initial observer incorrectly waited for that API-dependent cleanup while holding the API thread; retain the failed run. The corrected run and subsequent idle shutdown/restoration pass 31 trace/wire/state checker tests, and arm cleanup passes five ownership tests. Both diagnostic receipts are now restored in their original verified filesystem view; normal candidate 9a93f6d is installed with Revit closed. The old server remains redirected; correct shared installation/recovery visibility before desktop acceptance. This does not establish queued application shutdown, native Raise in flight at shutdown or physical lifetime disposal.

## Repeated desktop launches and settings recovery

The [desktop-launch preparation](verification/2026-10-21-desktop-launch-preparation.md) adds `--wire-evidence-root <absolute-parent>` to the production server. Each process creates an independent `session-<guid>` capture. Use this mode for desktop entries; reserve `--wire-evidence <new-directory>` for a deliberately single-use harness capture. The options are mutually exclusive. Confirm the installed candidate includes this change before registering it; an older executable rejects the new option.

The existing `scripts/manage_mcp_probe_settings.py` accepts `--name bim-agent-lab-revit-readiness --revit-pid <verified-current-pid>` during `prepare`. Its other inputs remain `--config`, `--format json|toml`, `--command <absolute-installed-executable>`, `--evidence <absolute-per-client-parent>` and `--receipt <new-private-path>`. The resulting arguments include `--wire-evidence-root` automatically. Keep receipts private: they contain complete original settings. Inspect the prepared entry and physical config path before `apply --receipt <path>`. No settings mutation happens during prepare.

Choose a fresh evidence parent per application/test run and acquire the PID from the actual Revit process. Re-read app versions and installed executable hashes. Do not pin tomorrow's connection to yesterday's PID. After applying, the user reloads the desktop MCP connections; actual wire traffic must establish which app, version and protocol used the tools. For removal, `restore --receipt <path>` removes only that receipt's unchanged owned entry and preserves unrelated edits. A changed owned entry is a conflict to inspect, not permission to restore a whole old config file. Earlier probe receipts without a name remain supported. Full A8 still requires both real desktops, three principal samples, NC1/NC2 and a structured failure.

## Retain the actual initial desktop exchange

The [October 22 Codex record](verification/2026-10-22-desktop-readiness.md#codex-desktop-initial-real-readiness-exchange-passes) is the first production-tool desktop continuity check. Invoke context, then the check with the exact returned token through the desktop's own tool connection. Find the matching request IDs in **one** process capture; do not combine a discovery-only session with a different process's responses. Read file bytes, not only directory-entry lengths: a live response file here had a stale zero-length listing but yielded its complete contents when opened.

Copy the two raw streams unchanged into a new immutable evidence directory only when each has complete JSONL lines. Record the source session, snapshot timestamp, byte lengths/hashes, client/version, negotiated revision and request IDs. Retain actual tool returns as `context.json` and `check.json`. A snapshot of an active process is not a completed shutdown capture. The live parent may continue receiving traffic.

For the narrow initial exchange (initialize, initialized notification, catalog, one or more complete context/report pairs), run from the repository root:

```powershell
python projects/revit-readiness/tests/verify_desktop_continuity.py projects/revit-readiness/docs/verification/desktop-readiness-evidence/2026-10-22/codex-initial
python projects/revit-readiness/tests/verify_desktop_continuity.py projects/revit-readiness/docs/verification/desktop-readiness-evidence/2026-10-22/claude-initial
python projects/revit-readiness/tests/test_verify_desktop_continuity.py
```

This checker proves request correlation, unchanged accepted legacy negotiation, text/structured/saved-result agreement, context/report binding and whole-report continuity with the current-session SDK capture except generation time. It requires an immutable snapshot manifest and is deliberately narrower than a general desktop session parser. Do not trim away failed attempts to fit it; retain those separately and report the failed attempt. Its six test methods include 33 negative controls. It is **not** `verify_mcp_evidence.py`, the planned Task 8 independent oracle, and must not replace native-schedule/checklist comparisons, NC1/NC2 presentation checks or the stale-token-after-document-switch control.

Current Codex result: app 26.1016.6573.0, wire client 0.163.0-alpha.11.1, protocol 2025-06-18, installed server d88de67/host9a93f6d. Claude 3.1623.4.0 now also passes initial continuity on 2025-11-25. The author identified the first Claude run as Desktop Code tab and the second as Chat. Both pairs arrived through one `local-agent-mode-bim-agent-lab-revit-readiness` / `1.0.0` session; do not infer the visible UI from a wire client name. Retain the author's mode statement and answer alongside the trace. The manifest's `exchanges` array binds every pair to its context/report files and request IDs. Legitimate roots-change notifications are retained, and all exchanges are checked; the other Claude session is discovery-only.

SDK 2.2.0 and strict July testing remain unchanged. Revit is still open; no installation/Deployment/All runs. Claude's selected model/permission choice have been requested for the baseline. Both five-case/error workflows, independent oracle and the remaining Task 7/R3 items stay open. See the [Claude record](verification/2026-10-22-desktop-readiness.md#claude-desktop-code-and-chat-answers-with-actual-tool-traffic).
