# Laptop travel preparation — October 27, 2026

The author requested a complete local build chain and dependencies because this laptop will travel to the summit. This authorizes machine preparation separately from the proposed Revit 2026.5 compatibility changes. Initial observations are retained in the [workstation intake](2026-10-27-summit-workstation-intake.md).

## Local tools and repeatable entry point

Launch `C:\repos\Start-BimAgentLab.cmd`. It opens a standalone PowerShell shell at the repository with the pinned SDK, a dedicated NuGet cache and the existing Python interpreter selected for that shell. No global SDK pin or machine PATH was changed.

Machine-local tool root: `%USERPROFILE%\.bim-agent-lab\toolchain`.

| Input | Prepared state |
|---|---|
| .NET SDK | `10.0.204`, extracted from the official Microsoft Windows x64 archive; SHA-512 matched official release metadata |
| SDK-local runtime | Core, Windows Desktop and ASP.NET Core `10.0.8`; system installation separately contains `10.0.10` |
| PowerShell | Standalone Windows x64 `7.6.9`; archive SHA-256 matched Microsoft's PowerShell GitHub release asset digest |
| Python | Existing app-managed `3.12.14`; actual verification/handout imports `tomllib`, `reportlab`, `docx`, `PIL` and `pypdf` passed |
| Git/OpenSSH | Existing Git `2.56.0.windows.3`; SSH clone verified earlier in this session |
| Revit API/framework references | 2024 and 2026.5 API files and .NET Framework 4.8 targeting references present |
| Package cache | Dedicated `toolchain\nuget-packages`; all 19 C# projects passed locked cache-only restores |
| Retained recovery inputs | SDK and PowerShell archives retained under `toolchain\downloads`, with manifest and checksums |

Python was not independently installed. Automatic approval review rejected the standalone installer command without a detailed reason; the command did not execute. The existing app-managed interpreter satisfies the checks performed here, and the launcher selects its exact local folder. Its lifecycle remains tied to that application installation. An initial overly broad import check asked for `qrcode` and failed; source inspection showed the handouts use ReportLab's QR implementation instead. All actual handout imports then passed; no unused library was installed.

## Dependency verification

The first all-project locked restore failed only in `tests/mcp-sdk-diagnostic/packages.lock.json`: its recorded project graph omitted the bridge runtime/contract now referenced by the production server. A targeted lock regeneration restored those project edges. Every previously recorded external package version and content hash was compared before/after and remained identical. This is a lock-graph repair, not a package upgrade.

All **19 projects** subsequently restored successfully with:

```powershell
dotnet restore <project.csproj> --force --no-http-cache `
  --configfile "$env:USERPROFILE\.bim-agent-lab\toolchain\NuGet.Offline.Config" `
  -p:RestoreLockedMode=true -p:NuGetAudit=false --nologo
```

That local configuration clears remote package and audit sources and names an empty local feed. Required packages came from the dedicated global-packages cache. Audit was disabled only for this cache-availability check; this is not a new vulnerability audit or a test with Windows networking disconnected. The ordinary build/test scripts were run separately and may perform their normal restore checks.

Probe, readiness (including optional fixture tools) and MCP server publish passed on the existing **Revit 2024/net48** source. The local `Verify-CachedBuild.ps1` then ran cache-only locked restores followed by `dotnet build -c Release --no-restore` for **all 19 projects**, successfully, from standalone PowerShell 7.6.9. The earlier ordinary baseline used the agent's PowerShell 7.6.5 process; its child shell selection used the prepared PATH. Existing CA1416 warnings in the external server were retained rather than suppressed.

| Regression scope | Observed result |
|---|---|
| Core / Excel / isolated exporter | 18 / 7 tests and process-isolation check passed |
| Deployment and recovery | 41 passed |
| Production tools | 72 passed |
| Dispatch | 88 net48 / 83 net10 passed |
| Source-linked adapter handoff | 10 passed; Autodesk/reader doubles remain explicit |
| Python shutdown checker | Seven passed |
| Windows pipes | 53 per runtime passed, with independent timing/progress checkers |
| Framing | 33 per runtime passed, with bilateral/independent exchange checks |

The first `-Suite All` invocation did **not** pass: the protocol section reached Python runner tests and failed two of 34 methods. Those tests hard-coded `powershell` (Windows PowerShell 5.1), whose effective script policy blocked execution on this laptop. PowerShell 7's existing RemoteSigned policy allowed the local scripts. The harness now invokes `pwsh`, matching documented prerequisites, for both wrapper tests and its owned-process check. No execution policy was changed and no bypass option was added. The failed log is retained; the affected Protocol suite is rerun separately rather than rewriting the original invocation as a pass. Final follow-up results are recorded below.

### Final component verification

The fresh `Test-McpBridge.ps1 -Suite Protocol` follow-up exited **0** after that correction. All **34 Python methods** passed, including the two previously blocked wrapper cases. Compatible protocol exchanges on `2025-06-18`, `2025-11-25` and `2026-07-28`, strict July success/legacy refusal, stream ownership and shutdown controls passed. The four separate SDK-client captures (three compatible, one strict) passed independent wire checks. The full sequence is retained in `protocol-followup.log`.

All component scopes now have passing observations across the original baseline and targeted Protocol follow-up. A second monolithic `-Suite All` run was not performed: the change affected Python shell selection, while Deployment/Tools/Dispatch/Pipe/Framing had already passed. The original All failure remains part of the evidence. The all-project cache-only/no-restore build log is `cached-build-20261027T215725Z.log`; a fresh standalone shell also verified SDK 10.0.204, Python 3.12.14 and PowerShell 7.6.9 resolution.

## Demo inputs available locally

- Hash-verified copies of the installed Snowdon architectural sources are under ignored `samples/local/required-information/` (2024) and `samples/local/revit2026-source/` (2026). Original installed samples were not edited. These are source copies, not a prepared five-case demo set or a successful model-upgrade test.
- The final PDF and editable Word handouts, source JSON/generators and HTML agenda are in the checkout. Local file hashes are retained in the machine-local tool manifest folder.
- Dynamo's 2026 installation folder exists. No pyRevit entry was found in the inspected per-user/all-user 2024/2026 add-in folders; no optional scripting component was installed or rehearsed.
- The HTML agenda references Google Fonts. Its offline typography has not been rehearsed; the supplied PDF handout is already local. No approved presentation artifact was changed during setup.

## Remaining travel gates

### Follow-up: SDK 9 and the 2024 host

The author asked whether SDK 9 is sufficient for Revit 2024 and why a fresh Windows laptop needed SDK 10. To distinguish a host requirement from repository policy, the agent copied the unchanged `BimAgentLab.RevitProbe.csproj` and `ReadOnlyProbeCommand.cs` into ignored `.superpowers/sdk9-revit2024-proof/`, outside the project's global.json ancestry. System `C:\Program Files\dotnet\dotnet.exe` selected **9.0.319**; restore used the empty-source offline configuration, followed by a Release `--no-restore` build. The probe compiled for **net48** against the installed 2024 APIs with **zero warnings and zero errors**. No Revit application was run and no source or SDK pin was changed. The SDK's first-use initialization reported creating a development HTTPS certificate; no trust command was invoked.

Thus SDK 9 can compile this standalone 2024 probe. It is insufficient for the full current lab's net10.0 MCP server and pinned SDK selection. The [participant version/download matrix](../../../../docs/reference/participant-install-checklist.md#revits-target-versus-this-repositorys-compiler-choice) now makes that distinction and the 2026.5 runtime transition explicit.

### Before departure

1. Implement and prove the existing plugins in actual Revit 2026.5 under the [proposed design](../../../../docs/superpowers/specs/2026-10-27-revit-2026-5-compatibility-design.md). Current builds do not establish 2026 compatibility.
2. Prepare the five controlled fixtures and a reset set on this laptop; repeat independent host/Excel/desktop checks on one frozen candidate.
3. Confirm Revit can launch with the intended account/license and test the actual desktop client connections. Authentication/entitlement and a disconnected-license rehearsal were not performed here.
4. Rehearse the selected demo with its intended connectivity and fallback. Local compilation/cache availability does not establish that a hosted agent session will work without connectivity.

The machine-local `travel-manifest.json`, `restore-online.log`, `restore-offline.log`, `baseline-checks.log`, sample-copy receipts and archived tool downloads retain detailed local evidence. Keep credentials, client configuration and models out of Git. This preparation is not overall R3 closure or release acceptance.
