# Build, install and recover the local Revit bridge

Task 7 working procedure. Task 6 is independently closed at b4cdb1f; installation and live-host acceptance are being verified separately. Follow the [operation model](../../../docs/explanation/recoverable-readiness-installation.md), [approved Task 7 plan](../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-7-explicit-deployment-rollback-and-live-lifecycle-checks), and [host checklist](../tests/mcp-host-acceptance.md).

The [real-installation record](verification/2026-10-21-live-installation.md) now distinguishes verified install/undo/reinstall and initial live requests from outstanding lifecycle and desktop checks. Use the actual installed executable name, `BimAgentLab.RevitReadiness.Mcp.exe`; the installer does not create a shorter `BimAgentLab.Mcp.exe` alias.

Use 64-bit PowerShell 7, the existing .NET SDK/10 runtime, Revit 2024 and its net48 API references. This increment adds no package dependency and does not change SDK 2.2.0 or the compatible/strict-July policy. The existing core, Excel helper and fixture remain separate from the external modern server.

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
- `%LOCALAPPDATA%/BimAgentLab/RevitReadiness/McpServer`: modern executable and SDK dependencies.
- `%LOCALAPPDATA%/BimAgentLab/RevitReadiness/InstallRecovery/<transaction>`: immutable prepared receipt, staged bytes, original backup bytes, mutable state and journal. Keep this private local recovery material out of Git.

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
