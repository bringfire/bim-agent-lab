# Before you build or extend BIM Agent Lab

Use this list **before** planning a hands-on build. It describes the current source checkout as of October 28, 2026. Both Revit 2024 and Revit 2026.5.1 (API build 26.5.10.38) passed the three principal command/Excel and SDK-MCP cases on the laptop. Actual complete-case exchanges now pass in both Codex and Claude against the same Revit 2026.5.1 session. The remaining desktop workflows and a fresh participant setup still need verification. Reading the lessons or watching the seminar does not require installing this build chain.

**Verification update:** both 2024 and 2026.5 probe/readiness/fixture builds now pass, with year-specific installation/undo checks. The corrected 2026 candidate is installed on the laptop, with receipt hashes verified and the 2024 add-in folder preserved. Live 2026.5.1 command/Excel and SDK-MCP proof now passes for the three principal cases; the same fresh 2024 principal host/Excel/SDK-MCP checks also pass. The [current verification record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md) provides the actual scope and commands. Select `-RevitYear 2024` or `-RevitYear 2026`; the latter means **2026.5+ on .NET 10**, not earlier 2026 releases. The existing SDK 10.0.204 plus Framework 4.8 development references remain the build prerequisites; no additional SDK was introduced by the port.

## Revit's target versus this repository's compiler choice

The compiler SDK version and the framework a plugin targets are different settings. **Revit 2024 does not itself require Microsoft .NET SDK 10.** This repository requires it because the external MCP server targets `net10.0`, shared libraries include a modern target, and `global.json` pins SDK 10.0.204.

| Intended Revit host | Plugin target/runtime family | What the developer needs |
|---|---|---|
| Revit 2024 | .NET Framework 4.8 (`net48`) | A compiler capable of targeting net48, the 4.8 development references and matching Revit API DLLs. SDK 9 can compile a suitable standalone add-in; SDK 10 can too |
| Revit 2026 before 2026.5 | .NET 8 (`net8.0-windows` for the Autodesk migration pattern) | A capable SDK with .NET 8 targeting assets and matching Revit API DLLs; SDK 8 or a newer compatible SDK can build suitable projects |
| Revit 2026.5 and later | .NET 10 | SDK 10 or a later compatible toolchain with the required targeting assets, and matching Revit API DLLs. SDK 9 cannot build a net10.0 project |

Sources: Autodesk's [2024 development requirements](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Development_Requirements_html.html), original [modern Revit migration guide](https://help.autodesk.com/cloudhelp/2026/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_NET8_Update_html.html), subsequent [2026.5 .NET 10 transition](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Revit--Requirements-for-products-affected-by-the-Microsoft--NET-10-transition.html), and Microsoft's [SDK/framework selection guidance](https://learn.microsoft.com/en-us/dotnet/core/versions/selection). These are platform/build distinctions, not a claim that this lab's current source supports every row.

On October 27, the unchanged Revit 2024 read-only probe was copied outside this project's SDK pin and compiled with the laptop's SDK **9.0.319**, installed 4.8 development references and 2024 API DLLs: **zero warnings, zero errors**. That was compilation proof only, not a fresh Revit host run or a full-repository SDK 9 pass. The full repository had separately built with SDK 10.0.204. The project pin was not changed.

For someone following **this lab** for both 2024 and the proposed 2026.5 extension, use SDK **10.0.204 once**, plus the **4.8 Developer Pack** for the legacy targets. There is no need to install SDK 9 just for the 2024 example. The older .NET 8/Revit 2026 route is a separate target requiring its own verification; installing SDK 10 alone does not port the plugin source.

## Choose the activity

| Activity | Preparation |
|---|---|
| Watch the demonstration or read the lessons | Access to the seminar/materials; no compiler SDK required |
| Build or extend the source with an agent | Complete the checklist below and have the agent verify it before the session |
| Run a prebuilt plugin | Follow that package's version-specific runtime/install instructions. A compiler SDK is generally unnecessary, but this repository does not yet offer an accepted downloadable plugin release |

## Source-build installation checklist

| Obtain or confirm | Current requirement | Official source / check |
|---|---|---|
| Windows and full Revit | Windows x64 and the full intended Revit installation, with working sign-in/entitlement. Laptop host evidence covers Revit 2024 build 24.3.60.12 and Revit 2026.5.1 build 26.5.10.38 | [Detailed prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md); other builds need verification |
| **Microsoft .NET SDK** | **Windows x64 SDK 10.0.204**. An installed runtime or SDK 9 is insufficient for this checkout | [Exact SDK installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-10.0.204-windows-x64-installer). Check SDK resolution inside `projects/revit-readiness`, not only at the repository root |
| .NET Framework 4.8 development references | Needed for the current Revit 2024 host/core/exporter projects; confirm existing references or install the Developer Pack | [4.8 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48). The current projects use installed targeting references; adding a reference-assembly package would be a project change |
| PowerShell 7, x64 | Use `pwsh`; laptop verification used 7.6.9 | [Install PowerShell on Windows](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows). Windows PowerShell 5.1 is not the supported full verification/install host |
| Git for Windows | Available to the agent | [Git for Windows](https://git-scm.com/install/windows). Public HTTPS cloning does not require a GitHub account or an SSH key |
| Python for full verification | A working Python 3.12 interpreter; recorded runs used 3.12.10 and 3.12.14. Have the agent verify the actual executable and checker imports | [Python Windows downloads](https://www.python.org/downloads/windows/). This is separate from Dynamo's Python engine. Do not assume another participant has the presenter's app-bundled interpreter |
| Local coding/desktop agent | Installed, authenticated and able to edit local files and run the build; verify the intended MCP-capable desktop client for agent-tool use | [Starting guide](../start-here.md) and [technical handoff](../how-to/agent-handoff.md) |
| Source, packages and sample | Local checkout, locked NuGet restore and a disposable copy of the documented Autodesk sample with the intended checklist | [Sample preparation](../../projects/revit-readiness/samples/README.md). Do this while connectivity is available; cloning source does not fetch every build dependency or working model |

The SDK pin lives in [global.json](../../projects/revit-readiness/global.json): `10.0.204` with `latestPatch`. A later compatible patch in the same `10.0.2xx` feature band can be selected. Installing an arbitrary newer feature band, such as `10.0.4xx`, does not satisfy this policy. The exact installer link avoids that ambiguity. See Microsoft's [SDK selection rules](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json).

## Why Revit is not the whole build setup

The **runtime** executes .NET programs. The **Microsoft .NET SDK** supplies the compiler, MSBuild and development commands. Revit supplies the runtime it needs and its API DLLs; their presence does not establish that the separate compiler SDK is installed. Visual Studio may supply a suitable SDK, but verify its version. Microsoft's [Windows installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/windows) explains SDK versus runtime installation.

The **Revit SDK** is another resource: Autodesk examples, documentation and development materials. It is not the Microsoft .NET SDK. The API DLLs used by this lab are available in the installed Revit folder.

On the summit laptop, .NET 10 runtimes and SDK 9 were already installed. The **full lab source** still needed SDK 10.0.204 for its net10.0 server and SDK pin. A fresh Windows installation does not supply that repository-specific compiler setup; Revit's runtime installation does not supply it either. This requirement already existed in the project pin; the laptop inspection exposed a missing input rather than adding a new package dependency. See the [actual travel setup](../../projects/revit-readiness/docs/verification/2026-10-27-travel-toolchain.md). The inspection does not identify which earlier installer supplied SDK 9.

## Verify before the hands-on session

Give your agent this **suggested prompt**:

> Read the participant install checklist and project prerequisites. Inspect my exact Revit build, selected .NET SDK inside the project, Framework 4.8 targeting references, PowerShell 7, Git, Python and agent access. Report missing inputs before installing anything. After approved setup, run the environment check, locked restore and source build. Prepare a disposable sample and prove the read-only command in the intended Revit build. Explain separately what is installed, what compiles and what has actually run in Revit. Check client sign-in/connectivity before a live agent demonstration.

Run `Check-Environment.ps1 -RevitYear 2024` or `-RevitYear 2026` in `projects/revit-readiness/scripts`; omitting the year still selects 2024. The modern check requires the installed 2026 API/runtime to match the .NET 10 route. This script does not inventory all Python/MCP/client requirements. A successful inventory is followed by compilation and actual host proof.

NuGet restores this lab's pinned C# libraries automatically; participants do not manually install the MCP SDK or ClosedXML. RevitLookup and pyRevit remain optional unless the selected exercise explicitly needs them. A separate Python installation for repository verification is different from running the beginner Dynamo Python example.

Before distributing a final 2026 setup notice, verify the intended Revit point release and candidate, freeze the exact exercise, and repeat the checklist on a fresh participant setup. The delivered PDF/Word handouts have not been regenerated by this documentation correction.

## Revit Home versus the ribbon workspace

For the opt-in teaching-sample preparation, close projects first. **Switch from Home to the ribbon workspace using the Home toggle at the top left**, then select Add-Ins → External Tools → BIM Agent Lab - Prepare teaching sample copies. Home itself does not show that menu. This navigation was observed on the laptop's Revit 2026.5.1; the preparation command opens its configured disposable source copy. Once preparation opens a project, the probe, required-information and bridge commands can be used from the ribbon. Do not run the read-only project commands from Home or infer their availability from a manifest alone.

## Connect the desktop agent before the live demonstration

An installed Revit add-in does not register its tools with Codex or Claude Desktop. Open the intended prepared model, start its readiness bridge from External Tools, then have the agent register the installed external server in the intended client. Keep a recovery copy of the existing client settings and preserve other servers and preferences. The [technical setup procedure](../../projects/revit-readiness/docs/mcp-bridge-setup.md#repeated-desktop-launches-and-settings-recovery) provides the existing helper and ownership rules.

Locate Claude's configuration through **Settings → Developer → Edit config**, rather than assuming a path. On this laptop, Claude 4.29 opened `%LOCALAPPDATA%\Claude-Data\claude_pzs8sxrjxfjjc\claude_desktop_config.json`; the usual Roaming path was absent. Codex uses `%USERPROFILE%\.codex\config.toml`. Reload the client after saving its entry and verify an actual context/readiness tool exchange. A successful SDK test is a separate check. See the official [Codex MCP configuration guide](https://learn.chatgpt.com/docs/extend/mcp?surface=cli) and [Claude local-server guide](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop).

The current recovery helper binds the entry to a verified Revit process ID. After restarting Revit, have the agent refresh that owned entry for the new process before reloading the client. Do not copy the presenter's PID or private recovery files. On October 28, both entries were saved with verified recovery backups. After the client reload, Codex's actual complete-case check passed: 16/16 instances, six types, no findings, complete coverage and no model modification. Claude discovery was observed; its actual Chat answer and the remaining desktop cases were still pending at this checkpoint.

The subsequent Claude attempt returned `BridgeUnavailable`. Investigation found that Revit, launched through Codex, had written its bridge descriptor into Codex's private AppData cache. Codex could read that view, but its success did not establish a shared connection. **Launch Revit from the Windows Start menu or its desktop shortcut**, then have the agent verify the actual bridge descriptor location and refresh the selected PID. On this laptop, the Windows relaunch produced a nonredirected descriptor and a fresh successful SDK report. Both client entries were refreshed; actual desktop requests after that relaunch are still pending the client reload. Do not copy a session file or loosen its permissions to make discovery work. The [failure and follow-up](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#claude-failure-exposes-a-redirected-bridge-descriptor) preserve the distinct observations.

Deleting Claude's config did not reconstruct the lab entry: this laptop regenerated preferences only. Ask the agent to back up the actual file and restore the entry while preserving other settings. The latest laptop entries for **both Claude and Codex** use the server's existing automatic choice of a **single active bridge**, avoiding a fixed PID in those entries; multiple active bridges produce an ambiguity error. Reload the MCP connection after a bound Revit/bridge session is restarted. A correctly written config still needs an open project and a started bridge before the model check can succeed.

When another agent suggests a PID, have it inspect the current process and actual call before applying that advice. In the latest retry, Claude had loaded the correct automatic-selection entry while Revit was at Home without a bridge. Opening the prepared project and Start produced a shared session and a successful SDK read; the actual desktop retry remains separate. A historical PID and a folder-access problem in a chat do not establish that the MCP server configuration is stale.

**Current laptop complete-case checkpoint:** both actual Claude and Codex requests now pass against the same Revit 2026.5.1 session. Both examined all 16 checklist instances across six types, returned no findings, complete coverage and false modification flags; their complete reports agree with the verified SDK result except the timestamp. This verifies the connection and complete sample. Defect/incomplete-input desktop demonstrations and a fresh participant setup still require their own checks. See the [actual two-client record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#both-actual-desktop-clients-pass-on-the-current-shared-session).

**Current rehearsal follow-up:** complete, wall-defects and ceiling-negative now have successful actual checks and reviewed explanations in both desktop clients on the laptop's Revit2026.5.1 session. Ceiling-negative returns three missing type fields/eight affected instances, all16 checklist elements examined and unchanged saved-model bytes. ACT Code is required even though its rating and assembly fields are exempt. This required no further installation. The [ceiling rehearsal record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#ceiling-negative-rehearsal) retains the evidence; incomplete-input checks, fresh participant setup and release packaging remain separate.
