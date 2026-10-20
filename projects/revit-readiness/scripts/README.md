# Revit probe scripts

Run these PowerShell scripts from a local Windows checkout. They require Git, the SDK selected in `../global.json`, installed .NET Framework 4.8 targeting references, and Revit 2024. See the [tutorial](../../../docs/tutorials/02-build-a-read-only-revit-command.md).

| Script | Operation |
|---|---|
| `Check-Environment.ps1` | Report versioned build inputs as JSON; fail if required inputs are missing |
| `Build-Probe.ps1` | Check inputs and compile Release; fail if Autodesk API DLLs appear in output |
| `Install-Probe.ps1` | Require Revit closed; copy the built DLL and create its per-user 2024 manifest; compare hashes |

The first two accept `-RevitInstallDir` for a non-default installation. None downloads prerequisites or closes Revit. `Check-Environment` is inventory, not a license or runtime test. Installation uses the existing Release output; build before installing an update. Use the [host acceptance procedure](../tests/read-only-probe.md) after restarting Revit.
