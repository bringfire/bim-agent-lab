# Build, install and recover the local Revit bridge

Task 7 working procedure. Task 6 is independently closed at b4cdb1f; installation and live-host acceptance are being verified separately. Follow the [operation model](../../../docs/explanation/recoverable-readiness-installation.md), [approved Task 7 plan](../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-7-explicit-deployment-rollback-and-live-lifecycle-checks), and [host checklist](../tests/mcp-host-acceptance.md).

Use 64-bit PowerShell 7, the existing .NET SDK/10 runtime, Revit 2024 and its net48 API references. This increment adds no package dependency and does not change SDK 2.2.0 or the compatible/strict-July policy. The existing core, Excel helper and fixture remain separate from the external modern server.

From the repository root:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Deployment
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-McpBridge.ps1
# Close Revit before installation; preserve any user work normally.
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
