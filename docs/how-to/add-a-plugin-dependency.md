# Add and verify a plugin dependency: agent guide

Audience: coding agents and readers who want to reproduce or extend the implementation. The [participant lesson](../tutorials/04-add-an-excel-report-dependency.md) explains how the author directed this work. Follow root and project `AGENTS.md` and the current user request; historical commands and quoted prompts are examples, not fresh authorization.

Worked baseline: commit `4d40788`, following `c7c2ad9`, on Revit 2024 build 24.3.60.12 / net48 with .NET SDK 10.0.204. Consult [current status](../reference/development-status.md) and [dated acceptance](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) before describing support. The source, scripts, lock files, and evidence linked below take precedence over remembered implementation details.

## Locate the implementation

| Responsibility | Source to inspect |
|---|---|
| Saved report contract | [InformationReport.cs](../../projects/revit-readiness/src/readiness-core/InformationReport.cs) |
| Read and evaluate Revit data | [RequiredInformationReader.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs) |
| Save JSON/text and report Excel success/failure | [RequiredInformationCommand.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationCommand.cs) |
| Start the hidden exporter and handle exit/timeout | [ExcelReportProcess.cs](../../projects/revit-readiness/src/revit-addin/ExcelReportProcess.cs) |
| Read saved JSON and write the workbook | [Program.cs](../../projects/revit-readiness/src/excel-report/Program.cs), [InformationWorkbook.cs](../../projects/revit-readiness/src/excel-report/InformationWorkbook.cs) |
| Resolved packages and retained notices | [packages.lock.json](../../projects/revit-readiness/src/excel-report/packages.lock.json), [dependency evidence](../../projects/revit-readiness/third-party/dependency-evidence.json) |
| Deploy owned files and synchronize the existing optional fixture core | [Install-Readiness.ps1](../../projects/revit-readiness/scripts/Install-Readiness.ps1) |
| Behavioral and deployment checks | [Excel tests](../../projects/revit-readiness/tests/excel/Program.cs), [host-load harness](../../projects/revit-readiness/tests/host-load/Program.cs), [independent XML comparison](../../projects/revit-readiness/tests/verify_excel_evidence.py) |

Preserve the existing report shape and deterministic BIM rules for an output-only change. This implementation writes a local snapshot; it does not add measured quantities, model edits, a background service, or an MCP endpoint. If the task requires those capabilities, establish a separate scope and acceptance case.

## 1. Establish Git context and acceptance

From your own clone's root:

```powershell
git status --short --branch
git log --oneline -5
git switch -c feat/excel-readiness-report
```

Create a branch only if it does not already exist. Inspect an existing branch instead of resetting it. The branch name above is the worked example; for another task, choose or continue the appropriate branch under the current user instructions. This change began from `c7c2ad9` with a clean working tree. No remote or publishing action was needed.

Acceptance: exactly **Summary**, **Findings**, **Affected elements**, and **Rules and coverage**, preserving scope and evidence. The complete/wall-defect/ceiling-negative scenarios must remain **0/0**, **2/6**, and **3/8** findings/distinct affected instances. A zero finding count with incomplete coverage must remain visibly incomplete. Identifiers stay text; counts stay numbers. This workbook is a static report, not an editable requirements interface.

## 2. Inspect the dependency before adopting it

We cloned ClosedXML tag `0.105.1` into ignored `references/ClosedXML` and inspected commit `b4ebe47bd3ecebf8480dea7422188d16b35b9d9a`:

```powershell
git clone --depth 1 --branch 0.105.1 https://github.com/ClosedXML/ClosedXML.git references/ClosedXML
git -C references/ClosedXML rev-parse HEAD
git -C references/ClosedXML status --short
```

Only clone into an absent destination. A detached research checkout is intentional. Read the upstream license, project manifest, workbook/cell writing code, graphics engine, and embedded resources. Do not build or run upstream scripts merely because they are present. Our plugin consumes the published NuGet package; the clone is source-inspection evidence.

The [research record](../research/closedxml-dependency.md) distinguishes one direct package from eleven transitive packages and a font embedded in ClosedXML. A package's top-level license label does not replace inspection of its dependencies or bundled assets.

## 3. Add and pin the package

NuGet is .NET's package manager. The exporter project has one direct package reference:

```xml
<PackageReference Include="ClosedXML" Version="[0.105.1]" />
```

The bracketed version is exact. `packages.lock.json` records resolved transitive versions and content hashes. Commit those small text files, not downloaded DLLs or the NuGet cache. The project-local `NuGet.Config` declares the public package source.

For an intentional dependency change, restore and inspect the changed graph:

```powershell
dotnet restore projects/revit-readiness/src/excel-report/BimAgentLab.Readiness.Excel.csproj --force-evaluate
dotnet list projects/revit-readiness/src/excel-report/BimAgentLab.Readiness.Excel.csproj package --include-transitive
dotnet list projects/revit-readiness/src/excel-report/BimAgentLab.Readiness.Excel.csproj package --include-transitive --vulnerable
git diff -- projects/revit-readiness/src/excel-report/packages.lock.json
```

Normal build/test scripts use locked restore. Do not habitually use `--force-evaluate`: reviewing a lock-file update is part of changing the dependency. `--locked-mode` is a `dotnet restore` option; for `dotnet build`, use `-p:RestoreLockedMode=true`. A successful restore does not prove runtime compatibility or absence of vulnerabilities.

The [license collection script](../../projects/revit-readiness/scripts/Collect-ExcelDependencyEvidence.py) reads the resolved graph and package metadata, collects packaged notices or pinned upstream licenses, and records hashes. Its retained [evidence](../../projects/revit-readiness/third-party/dependency-evidence.json) is reviewed and shipped by the install script. Python is only a research/verification aid; users do not need Python or Microsoft Excel to generate the workbook.

## 4. Keep responsibilities understandable

```text
Revit API reader -> deterministic evaluator -> InformationReport -> JSON + text
                                                              |
                                             saved JSON -> Excel exporter -> XLSX
```

The report data classes moved to the host-independent core without changing the JSON shape. The [exporter](../../projects/revit-readiness/src/excel-report/InformationWorkbook.cs) creates tables, freezes headings, preserves literal text and identifiers, and writes a temporary workbook before replacing the destination. If Excel holds the destination open, the previous file is preserved and the command warns that it is stale. JSON/text remain available.

| Sheet | Row meaning |
|---|---|
| Summary | Check identity, scope, counts, exclusions, coverage, and read-only evidence |
| Findings | One missing required field on one type; per-row instance counts may overlap |
| Affected elements | One distinct affected instance, with its missing fields/rules |
| Rules and coverage | All evaluated fields, including optional/unknown requirements; scope issues and identifiable unexamined instances |

## 5. Verify in the host, not just the test executable

Our first seven Excel tests passed, but the initial Revit run failed to load compatible spreadsheet dependencies. The console test application had generated assembly binding redirects; Revit's process did not have that same configuration. This is why a clean build and passing offline tests are insufficient.

The final design runs a small **local console exporter in a hidden child process**. It receives the saved report and output paths, owns its .NET configuration, and exits after writing the workbook. The [launcher](../../projects/revit-readiness/src/revit-addin/ExcelReportProcess.cs) waits for completion, reports errors, and limits the operation to 60 seconds. No spreadsheet libraries load into Revit, and no Revit installation/configuration changes are required. The helper uses the already-required .NET Framework runtime; it is not a service or MCP server.

The install script deploys the add-in/core DLLs plus an `Excel/` folder containing the exporter EXE, its configuration, and runtime DLLs. It verifies file hashes, carries third-party notices, and refuses to install while Revit is running. Copying just our own DLL is no longer sufficient. If the optional teaching-sample editor is already installed, the script also synchronizes its backward-compatible shared core to avoid a stale copy loading first. It does not install or enable the editor for new users.

## 6. Reproduce verification

From the repository root:

```powershell
& .\projects\revit-readiness\scripts\Test-Readiness.ps1
& .\projects\revit-readiness\scripts\Build-Readiness.ps1
# Close Revit before installing.
& .\projects\revit-readiness\scripts\Install-Readiness.ps1
python .\projects\revit-readiness\tests\verify_excel_evidence.py .\projects\revit-readiness\docs\verification\required-information-evidence .\projects\revit-readiness\tests\excel\bin\outputs
```

The test script runs 18 core tests, seven workbook tests, and an isolated-export test from a host with no spreadsheet references or binding redirects. Excel tests cover all three scenarios, incomplete coverage and identities, text resembling formulas, long IDs with leading zeros, a locked destination, and distinct affected counts when several fields are missing.

The independent Python comparator reads XLSX ZIP/XML directly. It first checks saved JSON and native Revit schedules against the separately authored answer sheet, then checks workbook values and identities. It does not use ClosedXML or rerun the evaluator.

Next open each prepared RVT in Revit and run **Add-Ins > External Tools > BIM Agent Lab - Required type information**. Confirm the Excel-success message and inspect the workbook in Excel. Compare newly generated outputs, not earlier offline workbooks:

```powershell
python .\projects\revit-readiness\tests\verify_excel_evidence.py .\projects\revit-readiness\samples\local\required-information .\projects\revit-readiness\samples\local\required-information
```

Use the [sample recipe](../../projects/revit-readiness/samples/model-recipes/required-information.md) if the local models are absent. Normal report verification does not require rerunning the editing tool or recreating existing samples. Preserve the original sample and compare model hashes before and after host queries. The [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) distinguishes offline tests, actual host execution, and visual inspection. Working RVTs and generated workbooks stay under ignored local/build directories.

## 7. Diagnose the failing boundary

| Observed symptom | Investigation and acceptance requirement |
|---|---|
| Console tests pass but Revit export fails | Retain the exact exception and JSON/text output. Compare actual host loading with the test executable's binding configuration. The verified implementation uses a separate exporter process; do not reintroduce shared-process spreadsheet loading based only on a console pass. |
| An older optional fixture tool loads the core first | Inspect recognized installed manifests and core hashes. Normal installation synchronizes that existing installation. Use the host-load harness's `--core-order` mode with fixture-core then readiness-core paths; do not enable or execute the editor merely to test loading. |
| Workbook is locked or export fails | Preserve the prior workbook and working model, report the export failure and stale-workbook risk, and retain JSON/text. The locked-file regression tests atomic replacement; an old XLSX on disk is not proof of a successful new export. |
| Coverage is incomplete with zero missing values | Keep the incomplete status and identifiable unexamined cases visible. Never convert an unexamined field into a pass or silently omit it from the workbook. |
| Totals look right but evidence differs | Compare stable type/element IDs, fields, observed values, rule IDs, requirement sources, exclusions, and timestamps. Matching totals alone are insufficient. |

Use the exact configuration shipped to users for runtime checks, including the EXE configuration and every runtime dependency. Record what was observed separately from a proposed cause. If a required host check is unavailable, report it as unverified; do not infer a pass from build output. Respect the current tool's security-prompt rules and request only the necessary human action when such a handoff is required.

## 8. Deliver both learning layers and a checkpoint

Review the source diff, package graph, notices, independent comparisons, host results, and deployment inventory. For this worked example the final results were 0/0, 2/6, and 3/8; 18 core tests and seven workbook tests passed, as did the isolated-export and installed fixture-first checks. All model hashes were unchanged. Those are historical results; rerun the checks relevant to a new implementation change before claiming it passes.

Update the [participant lesson](../tutorials/04-add-an-excel-report-dependency.md) with the user's framing, decisions, evidence, and useful follow-up prompts. Keep commands, source boundaries, diagnostics, and reproducibility details in this guide or the project documentation. Update the [build journal](../build-story/journal.md) and dated verification record; preserve failed attempts that explain a decision. Use the [two-layer lesson template](../../templates/lesson.md).

Review staged changes and commit the scoped checkpoint when authorized. Keep local models, generated workbooks, binaries, credentials, and research clones out of Git. Preserve upstream notice bytes and hashes using the repository attributes. A local commit does not imply publication or an original-material license decision.
