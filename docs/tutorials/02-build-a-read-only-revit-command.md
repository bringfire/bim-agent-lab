# Build and prove a read-only Revit command

This chapter takes an agent from a machine inspection to a command running inside Revit. The worked example targets **Revit 2024 and .NET Framework 4.8**. It reports the active project and counts wall and ceiling instances. The [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) contains the actual results and screenshots.

## 1. Give the agent a bounded outcome

Actual author prompt:

> Ok, let's check this machine against the prerequisites, install anything missing, and build a minimal read-only Revit command to prove the setup works. Record the steps and lessons in the teaching repo.

This prompt supplies an observable outcome, permission to fill prerequisite gaps, a read-only boundary, and a documentation requirement. Repository instructions supply the target Revit year and the wider model-readiness goal. The agent still has to inspect the machine and establish what a passing result means.

Suggested participant prompt:

> Read the repository and project instructions. Inspect my machine against the Revit prerequisites. Identify missing inputs before installing anything, and use official sources for required downloads. Work on a development branch. Build the read-only probe, install it for my user, and verify it in Revit against a small model with an independently known count. Record the commands, exact versions, results, and limits. Commit the reviewed source and evidence; keep binaries and model files out of Git.

## 2. Establish the Git checkpoint

From the repository root:

```powershell
git status --short --branch
git log --oneline -5
git switch -c feat/revit-read-only-probe
```

The first implementation began from clean commit `88ac2df`. Do not rerun branch creation if the branch already exists. Ask the agent to preserve unrelated changes and choose a new branch name when extending this example.

Git records the source, decisions, instructions, and evidence together. It does not record the Revit installation or replace a model backup.

## 3. Inspect before installing

Read the [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md), then run:

```powershell
& .\projects\revit-readiness\scripts\Check-Environment.ps1
```

The script checks Git, the SDK selected by the project's `global.json`, Revit 2024 and both API DLLs, and three .NET Framework 4.8 reference assemblies. It reports missing inputs and fails if they are absent. Activation, entitlement, and actual application behavior still require opening Revit.

On the author's machine, every required input was already present. **No prerequisite software was installed or upgraded.** The build used .NET SDK `10.0.204`, installed .NET Framework 4.8 references, and Revit/API build `24.3.60.12`.

The SDK supplies the compiler; `net48` selects the framework the add-in targets. Installing a modern SDK does not change Revit 2024's runtime. Our `global.json` selects SDK `10.0.204` with `latestPatch` roll-forward within the `10.0.2xx` feature band. An SDK in a different feature band alone will not satisfy it. Install the matching SDK or deliberately change the pin and repeat verification.

This project uses installed targeting references and **zero NuGet packages**. The full Revit SDK, RevitLookup, Visual Studio, Python, Node.js, MCP libraries, and an AI API key are not required to build or run this command. They may serve later tasks. The coding agent has its own setup and authentication.

## 4. Define proof, then build

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

## 5. Register and run

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

## 6. Prove the result independently

Follow the [fixture recipe](../../projects/revit-readiness/samples/model-recipes/read-only-probe.md). Our empty project returned 0 walls and 0 ceilings. We then drew a rectangle of four basic walls and verified **Walls (4)** in Revit's Properties palette before running the probe. Both saved-model runs returned **4 walls and 0 ceilings**, with `IsModified` false before and after.

The saved RVT's SHA-256 was also identical before the reads, after the reads, and after closing the project without another save. A normal `Get-FileHash` initially encountered Revit's file-sharing lock; a read-only shared stream worked, and a normal hash after closing confirmed the result. On your machine, a simple alternative is to hash the saved closed file before opening it, run the command, then close without saving and hash again:

```powershell
Get-FileHash -LiteralPath .\projects\revit-readiness\samples\local\read-only-probe.rvt -Algorithm SHA256
```

Use the hash together with Revit's modified flag, API-enforced read-only mode, and source review. A disk hash alone cannot detect unsaved in-memory edits. An unchanged `IsModified` value alone would also be weak evidence if the model were already modified.

This is host acceptance testing, not a unit-test suite. Positive ceiling counts, linked models, design options, worksharing, large models, and the family-editor guard remain untested. Category counts do not establish quantities or model readiness.

## 7. Review and record the checkpoint

Ask the agent to review the source and scripts, verify the install matches the tested build, check documentation links, and inspect what Git will include:

```powershell
git status --short
git diff --check
git diff
# Stage only the source, instructions, and evidence reviewed for this change.
git diff --cached --stat
git diff --cached
git commit -m "Add and verify the first read-only Revit 2024 command"
```

Keep generated binaries, Autodesk DLLs, working RVT files, and local sidecars out of Git. This checkpoint stays on the development branch until integration is chosen. It does not publish a release or push to a remote.

The next useful increment is to inspect the readiness sample and define one deterministic check with independent expected answers. The external MCP bridge is a separate architectural decision; this command proves that the local Revit API and build setup work.
