# Revit build and fixture scripts

Run these PowerShell scripts from a local Windows checkout. They require Git, the SDK selected in `../global.json`, installed .NET Framework 4.8 targeting references, and Revit 2024. See the [agent setup guide](../../../docs/how-to/build-read-only-revit-command.md) and [agent readiness guide](../../../docs/how-to/build-required-information-check.md).

| Script | Operation |
|---|---|
| `Check-Environment.ps1` | Report versioned build inputs as JSON; fail if required inputs are missing |
| `Build-Probe.ps1` | Check inputs and compile Release; fail if Autodesk API DLLs appear in output |
| `Install-Probe.ps1` | Require Revit closed; copy the built DLL and create its per-user 2024 manifest; compare hashes |
| `Test-Readiness.ps1` | Build and execute the 18-test host-independent console suite |
| `Build-Readiness.ps1` | Build the read-only readiness add-in and core; optionally build the fixture editor with `-IncludeFixtureTools` |
| `Install-Readiness.ps1` | Require Revit closed; install readiness/core DLLs and manifest; optionally install fixture editing tools |
| `Prepare-SampleCopy.ps1` | Copy the installed architectural sample into ignored local sample storage and compare SHA-256; refuse overwrite |

Environment/build scripts accept `-RevitInstallDir` for a non-default installation. None downloads prerequisites or closes Revit. `Check-Environment` is inventory, not a license or runtime test. Installation uses the existing Release output; build before installing an update. Follow [probe acceptance](../tests/read-only-probe.md) or [readiness acceptance](../tests/required-information.md) after restarting Revit. The [sample recipe](../samples/model-recipes/required-information.md) explains opt-in fixture preparation and reset folders.
