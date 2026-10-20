# Add our first plugin dependency: Excel reports with ClosedXML

The readiness command now creates a four-sheet `.readiness.xlsx` beside its JSON and text reports. This lesson teaches how to select, inspect, pin, install, and verify a third-party dependency. The workbook presents the existing check's results; it does not introduce another evaluator.

## The actual request

> Ok, let's add the Excel output using ClosedXML with those four worksheets. Review the dependencies and licenses, verify the output against our sample results, and record the process in bim-agent-lab. This will demonstrate how to add our first plugin dependency. It will teach this.

This is a useful agent prompt because it names the package and desired output, requires dependency and license research, identifies an independent verification target, and asks for a durable lesson. It follows an earlier discussion of the four sheets, so that context does not need to be repeated. For a new chat, include the sheet names and point to this lesson.

## 1. Establish Git context and acceptance

From your own clone's root:

```powershell
git status --short --branch
git log --oneline -5
git switch -c feat/excel-readiness-report
```

Create a branch only if it does not already exist. Inspect an existing branch instead of resetting it. Our change began from `c7c2ad9` with a clean working tree. No remote or publishing action was needed.

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

After running the command in all three sample models, compare the fresh host outputs:

```powershell
python .\projects\revit-readiness\tests\verify_excel_evidence.py .\projects\revit-readiness\samples\local\required-information .\projects\revit-readiness\samples\local\required-information
```

Then open each prepared RVT in Revit and run **Add-Ins > External Tools > BIM Agent Lab - Required type information**. Confirm the Excel-success message and inspect the workbook in Excel. Compare newly generated outputs, not earlier offline workbooks. The [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) distinguishes offline tests, actual host execution, and visual inspection. Working RVTs and generated workbooks stay under ignored local/build directories.

## 7. Review, teach, and checkpoint

Review the diff, package graph, licenses, expected results, deployment inventory, and host evidence before committing. The separate code review found the dependency-loading gap and missing identities for unexamined elements; both became explicit verification targets.

A suggested prompt for extending this example:

> Read AGENTS.md and the Excel dependency lesson. Add one useful report column without changing the readiness rule. Explain its source and row meaning. Preserve scope, stable identifiers, missing/optional/unexamined distinctions, and distinct affected counts. Verify against our independent sample answers, run the host checks affected by the change, and record the lesson before a Git checkpoint.

The transferable lesson is that **adding a package also adds a resolved dependency graph, redistribution obligations, deployment files, and runtime assumptions to verify**.
