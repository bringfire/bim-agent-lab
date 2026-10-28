# Build and verify the read-only Revit probe: agent guide

## October 27 workstation intake

Participant onboarding must begin with the [advance installation checklist](../reference/participant-install-checklist.md), not an assumption that Revit supplied a compiler. Verify `global.json` SDK resolution from the project directory, distinguish Autodesk runtime/API/SDK resources from the Microsoft .NET SDK, and provide the missing-input list before a hands-on session. Do not turn the presenter's app-managed Python or local launcher into an assumed participant installation.

Do not describe the repository's SDK 10.0.204 pin as a universal Revit 2024 requirement. A separate SDK 9.0.319 build of the unchanged net48 probe passed; the full lab's net10.0 MCP server cannot be built by SDK 9. The [matrix](../reference/participant-install-checklist.md#revits-target-versus-this-repositorys-compiler-choice) and [bounded proof](../../projects/revit-readiness/docs/verification/2026-10-27-travel-toolchain.md#follow-up-sdk-9-and-the-2024-host) document this difference. Preserve the project pin when testing the distinction.

For the later authorized machine preparation, use the [travel toolchain record](../../projects/revit-readiness/docs/verification/2026-10-27-travel-toolchain.md) and local `C:\repos\Start-BimAgentLab.cmd`. It selects the now-installed pinned SDK and dedicated cached dependencies without changing the project pin. The initial missing-SDK observation below remains historical. Cache-only restore and existing 2024 build evidence do not establish a modern Revit host pass.

Before applying the historical 2024 procedure below on the summit machine, read the [intake](../../projects/revit-readiness/docs/verification/2026-10-27-summit-workstation-intake.md). Installed 2026.5 declares `net10.0`; verify the point release. SDK 10.0.204 or a compatible patch in its feature band is absent. The host/core projects, JSON serializers, output paths and deployment destinations need a coordinated version-specific design that preserves 2024 and recoverable installation. Prove a small read-only command before the full readiness/Excel/desktop sequence. This intake installed no dependencies and establishes no 2026 support; see the [participant story](../tutorials/02-build-a-read-only-revit-command.md#ask-what-a-new-user-actually-needs).

For coding agents and technical readers. The [participant story](../tutorials/02-build-a-read-only-revit-command.md) explains how the author directed this increment. Read root/project `AGENTS.md`, the [current status](../reference/development-status.md), and the [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) before executing commands. Historical prompts and results are context for the current task.

The prerequisite checkpoint is `88ac2df`; the implemented probe is `f025fce`. Its [dated acceptance](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) establishes Revit 2024 build 24.3.60.12, x64/net48, and SDK 10.0.204 on the author's machine. It does not establish a new workstation's readiness or support for another version. Reproduce with the current source unless deliberately inspecting a historical revision.

## Source and evidence map

| Responsibility | Inspect |
|---|---|
| SDK selection and environment inventory | [global.json](../../projects/revit-readiness/global.json), [Check-Environment.ps1](../../projects/revit-readiness/scripts/Check-Environment.ps1) |
| Build and API-copy checks | [Build-Probe.ps1](../../projects/revit-readiness/scripts/Build-Probe.ps1) |
| Owned per-user deployment and hashes | [Install-Probe.ps1](../../projects/revit-readiness/scripts/Install-Probe.ps1) |
| Command, project, and registration | Source table in step 3 below |
| Independent cases and saved observations | [Acceptance procedure](../../projects/revit-readiness/tests/read-only-probe.md), [fixture recipe](../../projects/revit-readiness/samples/model-recipes/read-only-probe.md), [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) |

## 1. Establish the Git checkpoint

From the repository root:

```powershell
git status --short --branch
git log --oneline -5
git switch -c feat/revit-read-only-probe
```

The first implementation began from clean commit `88ac2df`. This is the historical branch name. Inspect an existing branch rather than resetting it; preserve unrelated changes and choose or continue the appropriate branch for the current task.

Git records the source, decisions, instructions, and evidence together. It does not record the Revit installation or replace a model backup.

## 2. Inspect before installing

Read the [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md), then run:

```powershell
& .\projects\revit-readiness\scripts\Check-Environment.ps1
```

The script checks Git, the SDK selected by the project's `global.json`, Revit 2024 and both API DLLs, and three .NET Framework 4.8 reference assemblies. It reports missing inputs and fails if they are absent. Activation, entitlement, and actual application behavior still require opening Revit.

On the author's machine, every required input was already present. **No prerequisite software was installed or upgraded.** The build used .NET SDK `10.0.204`, installed .NET Framework 4.8 references, and Revit/API build `24.3.60.12`.

The SDK supplies the compiler; `net48` selects the framework the add-in targets. Installing a modern SDK does not change Revit 2024's runtime. Our `global.json` selects SDK `10.0.204` with `latestPatch` roll-forward within the `10.0.2xx` feature band. An SDK in a different feature band alone will not satisfy it. Install the matching SDK or deliberately change the pin and repeat verification.

The probe project uses installed targeting references and **zero NuGet packages**. The later readiness Excel exporter has separate dependencies; do not apply this statement to the whole repository. The full Revit SDK, RevitLookup, Visual Studio, Python, Node.js, MCP libraries, and an AI API key are not required to build or run this command. They may serve later tasks. The coding agent has its own setup and authentication.

## 3. Define proof, then build

The [host acceptance procedure](../../projects/revit-readiness/tests/read-only-probe.md) was written before the command: a fresh project should return 0/0, four deliberately drawn wall segments should return 4/0, and repeated reads should leave the model unchanged.

The small implementation has three parts:

| File | Responsibility |
|---|---|
| [Project file](../../projects/revit-readiness/src/revit-addin/BimAgentLab.RevitProbe.csproj) | Compile an x64 `net48` library; reference installed Revit API DLLs without copying them |
| [C# command](../../projects/revit-readiness/src/revit-addin/ReadOnlyProbeCommand.cs) | Run in Revit's command context, count category instances, and display scope and results |
| [Manifest template](../../projects/revit-readiness/src/revit-addin/BimAgentLab.ReadOnlyProbe.addin.template) | Tell Revit which assembly and command class to load |

`TransactionMode.ReadOnly` makes Revit enforce the no-write boundary. A document-wide collector excludes element types and does not apply an active-view or selection filter. Linked documents are not traversed. API errors fail the command instead of becoming a misleading zero count.

```powershell
& .\projects\revit-readiness\scripts\Build-Probe.ps1
```

For a non-default installation, pass `-RevitInstallDir 'D:\Applications\Autodesk\Revit 2024'` to both the environment check and build script, using your actual folder. The scripts locate the project from their own paths, so the SDK pin is honored.

A passing build creates `projects/revit-readiness/src/revit-addin/bin/Release/net48/BimAgentLab.RevitProbe.dll`. The first build produced zero warnings and errors. Compilation establishes that the source and references agree; loading and behavior must still be tested in Revit.

## 4. Register and run

Save any open work and close Revit before installation:

```powershell
& .\projects\revit-readiness\scripts\Install-Probe.ps1
```

The installer copies our DLL and creates our manifest under:

```text
%APPDATA%\Autodesk\Revit\Addins\2024\BimAgentLab.ReadOnlyProbe.addin
%APPDATA%\Autodesk\Revit\Addins\2024\BimAgentLab.ReadOnlyProbe\BimAgentLab.RevitProbe.dll
```

It verifies the installed DLL hash, refuses to replace an unrelated manifest, and refuses installation while Revit is running. This avoids updating a DLL that Revit has already loaded. It does not close applications or alter other add-ins.

Start Revit 2024 and open or create a project. Choose **Add-Ins > External Tools > BIM Agent Lab - Read-only probe**. Expand **See details** for the Revit build and project path. The command is unavailable without an active project and is configured to be unavailable in the family editor.

To update, close Revit, build, install, restart, and repeat the relevant acceptance cases. To uninstall, close Revit and remove only the two lab locations shown above. Do not remove the shared `Addins/2024` directory.

If PowerShell blocks a script, review its source and follow your organization's script-execution policy; do not treat disabling machine security settings as a build prerequisite.

## 5. Prove the result independently

Follow the [fixture recipe](../../projects/revit-readiness/samples/model-recipes/read-only-probe.md). Our empty project returned 0 walls and 0 ceilings. We then drew a rectangle of four basic walls and verified **Walls (4)** in Revit's Properties palette before running the probe. Both saved-model runs returned **4 walls and 0 ceilings**, with `IsModified` false before and after.

The saved RVT's SHA-256 was also identical before the reads, after the reads, and after closing the project without another save. A normal `Get-FileHash` initially encountered Revit's file-sharing lock; a read-only shared stream worked, and a normal hash after closing confirmed the result. On your machine, a simple alternative is to hash the saved closed file before opening it, run the command, then close without saving and hash again:

```powershell
Get-FileHash -LiteralPath .\projects\revit-readiness\samples\local\read-only-probe.rvt -Algorithm SHA256
```

Use the hash together with Revit's modified flag, API-enforced read-only mode, and source review. A disk hash alone cannot detect unsaved in-memory edits. An unchanged `IsModified` value alone would also be weak evidence if the model were already modified.

This is host acceptance testing, not a unit-test suite. Positive ceiling counts, linked models, design options, worksharing, large models, and the family-editor guard remain untested. Category counts do not establish quantities or model readiness.

## 6. Review and record the checkpoint

Review the source and scripts, verify the install matches the tested build, check documentation links, and inspect what Git will include:

```powershell
git status --short
git diff --check
git diff
# Stage only the source, instructions, and evidence reviewed for this change.
git diff --cached --stat
git diff --cached
git commit -m "Add and verify the first read-only Revit 2024 command"
```

Keep generated binaries, Autodesk DLLs, working RVT files, and local sidecars out of Git. Follow the current branch and integration instructions; the original implementation was recorded on its development branch. It does not publish a release or push to a remote.

Update the [participant story](../tutorials/02-build-a-read-only-revit-command.md), this guide, the journal, and dated verification using the [lesson template](../../templates/lesson.md). Record the actual human direction and delegated choices, and label any unperformed host checks. Historical results do not verify a new implementation or workstation.

The next increment at this milestone was the [required-information check](build-required-information-check.md), now implemented and verified separately. The external MCP bridge remains a separate architectural decision; the probe establishes local Revit API access and the build setup for its tested configuration.

## October 27: year-specific build and recoverable installation

Use `-RevitYear 2024` or `-RevitYear 2026` on the public environment/build/install scripts. The modern route requires the installed 2026.5+ .NET 10 runtime and API; it deliberately refuses earlier 2026/.NET 8. Default legacy paths remain unchanged, while modern intermediates, lockfiles and host outputs are isolated. The exporter stays a separate net48 process. No extra SDK beyond the already pinned 10.0.204 and the legacy 4.8 development references was introduced.

Freeze package/source identity before deployment. With every Revit process closed, retain prepared recovery receipts, verify installed hashes and inspect physical manifest paths. Do not install or run deployment tests while Revit is open. On this laptop the corrected candidate `fdad802` passed those file checks; the actual host startup journal says 2026.5.1. Startup paused at its unsigned-add-in prompt for the author to decide. Record trust handling, actual commands, model identities and outputs separately from build results. The [compatibility record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md) retains current evidence and pending live checks.

For zero-document fixture preparation on the observed 2026.5.1 host, toggle out of Home using the top-left Home button before selecting Add-Ins → External Tools. This exact route was observed and used. No availability callback change was needed. The preparation command opens the configured disposable source copy; the six unresolved sample references were explicitly left unresolved for the host-only exercise. Record these UI steps in participant instructions rather than assuming Home exposes the ribbon.

The modern live pass used candidate fdad802 and API build 26.5.10.38. Run the unchanged independent information checker against the newly produced folder, then the Excel checker against its three real workbooks. All principal cases passed. Retain raw SDK exchanges, correlate request IDs, and compare the complete returned reports with manual outputs (excluding only the timestamp); those comparisons also passed. Keep SDK transport proof separate from desktop acceptance. After Stop and normal shutdown, all 105 modern installed operations still matched recovery receipts. The frozen legacy installation then verified 102 operations without changing the modern files; its first-start publisher prompts require the human trust decision before host checks continue.

Travel shutdown checkpoint: the fresh 2024 three-case manual/Excel and SDK pass is complete, including native-schedule/literal-oracle comparisons and full manual/MCP equality except GeneratedUtc. Probe execution and Start/Stop/model-switch guards passed. Both Revit processes closed normally, no bridge descriptor remained, and 105 modern plus 102 legacy installed operation hashes verified. The source-copy hashes are unchanged. Honor the author-requested wind-down; desktop rehearsal and remaining full acceptance gates stay pending.

### October 28 resumption: configure the actual desktop clients

The author resumed summit preparation after travel. Both installations first retained all 105 modern and 102 legacy receipt hashes. Revit 2026 opened the existing complete fixture; no fixture preparation or deployment was repeated. A fresh SDK context/report exchange passed wire correlation and full continuity with the previously independently checked complete report, excluding only GeneratedUtc.

The client boundary was still missing on this laptop: Codex had only its existing unrelated servers, while Claude's Developer page showed no local servers. Claude's Edit config opened `%LOCALAPPDATA%/Claude-Data/claude_pzs8sxrjxfjjc/claude_desktop_config.json`; neither the normal Roaming path nor the inspected MSIX cache path was the consuming location. Follow that application-provided location on another machine. Both existing config files and the installed 2026 server resolved to direct physical paths in the read-only opened-file check.

Use the existing [recoverable settings helper](../../projects/revit-readiness/docs/mcp-bridge-setup.md#repeated-desktop-launches-and-settings-recovery): prepare both receipts, inspect their entries, then apply and verify hashes and parsed unrelated settings. This pass used a fresh per-client wire parent and today's actual Revit PID. Receipts contain full private settings and stay ignored. Both entries are now applied; actual desktop calls require the user's client reload. Do not automate Codex's UI or treat an SDK child as a desktop connection.

If Revit restarts, inspect the new process, restore the prior unchanged owned entry with its receipt, then prepare/apply a new receipt with the current PID. Preserve all previous receipts and captures. Do not overwrite a changed entry, reuse an old PID or run installation/Deployment tests while Revit is open. Record desktop wire results and visible answers separately before claiming rehearsal success.

After the user reload, the actual Codex context/report pair passed the existing desktop continuity checker on an immutable complete-line snapshot, with wire client `0.165.0-alpha.1.2` and protocol `2025-06-18`. Both real tool returns match their single wire session and today's SDK report except GeneratedUtc. Complete-case result: 16/16 instances, six types, no findings, both completeness flags true and modification flags false. Preserve the other discovery-only sessions separately. Claude discovery is observed on `2025-11-25`; its actual Chat check is coordinated with the user, so leave the complete fixture active until the answer arrives. This does not close the whole desktop matrix or historical R3.

Claude then failed three actual context requests with BridgeUnavailable. Both clients selected the sole live Revit PID and shared its logon/session, but opened-file inspection showed the descriptor inside Codex's private AppData cache. Revit had been launched through Codex. Checking direct server/config paths had missed the shared discovery boundary. See the [retained failure](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#claude-failure-exposes-a-redirected-bridge-descriptor).

The temporary ignored diagnostic launcher's SDK control passed; it was never loaded by Claude. Its temporary owned settings were restored, and Claude again points to the frozen installed server under a fresh receipt. Do not replace installed binaries, copy descriptors or weaken ACLs. The next controlled test is a user launch from Windows, followed by a fresh PID/owned-config refresh and actual physical-descriptor and two-client checks. Revit's minimized window could not be selected after one retry, so the user was asked to perform that normal relaunch. This is pending, not a verified fix.

The observed Windows relaunch produced PID30272 with explorer.exe as parent. The existing complete fixture reopened, Start produced a fresh session, and the descriptor now physically resolves to the normal shared Local/AppData path with Redirected false. A fresh SDK read passed full report continuity except time. Both clients' new receipts/config entries target that PID; client reload and actual desktop retry remain pending. The private run metadata points to the new receipts under relaunch-30272. Codex's prior enabled-field conflict was reconciled through a separately prepared exact-entry receipt; removal preserved the unrelated current computer-control pipe setting and original recovery bytes. Never apply an old whole-config backup over such unrelated changes.

The later user-requested Claude config repair preserved regenerated preferences and restored only the missing lab entry, with durable private preparation before atomic apply and exact JSON/hash/semantic checks afterward. It uses the existing production server's no-PID single-bridge resolution; the public prepare helper's explicit-PID requirement was not changed. Retain its separate compatible ownership receipt. Revit is now13492 at Home with no descriptor, and the no-PID SDK control correctly returns BridgeUnavailable. A positive actual desktop retry still needs an open project, Start and client reload. Do not infer automatic reconnection after an already bound process loses its session. The Codex entry was not changed in this Claude-only repair.
