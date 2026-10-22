# Revit build and fixture scripts

Run these PowerShell scripts from a local Windows checkout. They require Git, the SDK selected in `../global.json`, installed .NET Framework 4.8 targeting references, and Revit 2024. See the [agent setup guide](../../../docs/how-to/build-read-only-revit-command.md) and [agent readiness guide](../../../docs/how-to/build-required-information-check.md).

| Script | Operation |
|---|---|
| `Check-Environment.ps1` | Report versioned build inputs as JSON; fail if required inputs are missing |
| `Build-Probe.ps1` | Check inputs and compile Release; fail if Autodesk API DLLs appear in output |
| `Install-Probe.ps1` | Require Revit closed; copy the built DLL and create its per-user 2024 manifest; compare hashes |
| `Test-Readiness.ps1` | Build and run readiness, Excel and process-isolation regression checks |
| `Build-Readiness.ps1` | Build the read-only readiness add-in and core; optionally build the fixture editor with `-IncludeFixtureTools` |
| `Install-Readiness.ps1` | Require Revit closed; install the built readiness package; `-IncludeMcpBridge` adds the bridge with prepared recovery; fixture editing tools remain opt-in |
| `Prepare-SampleCopy.ps1` | Copy the installed architectural sample into ignored local sample storage and compare SHA-256; refuse overwrite |

Environment/build scripts accept `-RevitInstallDir` for a non-default installation. None downloads prerequisites or closes Revit. `Check-Environment` is inventory, not a license or runtime test. Installation uses the existing Release output; build before installing an update. Follow [probe acceptance](../tests/read-only-probe.md) or [readiness acceptance](../tests/required-information.md) after restarting Revit. The [sample recipe](../samples/model-recipes/required-information.md) explains opt-in fixture preparation and reset folders.

## MCP bridge and verification

Start with the [agent handoff](../../../docs/how-to/agent-handoff.md) and [installation/recovery guide](../docs/mcp-bridge-setup.md). Use 64-bit PowerShell 7 for bridge build/install/undo. Do not run installation, undo, Deployment or All while Revit is open.

| Script | Operation |
|---|---|
| `Build-McpBridge.ps1` | Build readiness and publish the external server with locked dependencies; does not install |
| `Test-McpBridge.ps1` | Run Protocol, Framing, Pipe, Dispatch, Tools, Deployment or All; offline checks do not replace live host/client acceptance |
| `Undo-McpBridgeInstall.ps1` | Recover an owned installation using the actual private receipt and verified original filesystem context |
| `manage_mcp_probe_settings.py` | Prepare/apply/restore an owned client entry; explicitly select readiness name because the default is the synthetic probe; preserve unrelated settings |
| `Prepare-McpControls.ps1` | Prepare or clean up owned NC1/NC2 copies; independent absence confirmation is a separate NC1 step |
| `Invoke-ReadinessMcp.ps1` | Run one already prepared sample case through the official SDK client, validate context before the check, retain evidence and compare independently |

Python supports the settings, preparation and verification helpers. The runner does not build, install, change settings or replace actual desktop testing. See the [runner procedure](../../../docs/how-to/verify-readiness-through-desktop-agents.md#run-one-prepared-case-through-the-sdk) for required inputs and limits.
