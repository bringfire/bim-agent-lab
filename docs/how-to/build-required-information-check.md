# Build and verify the required-information check: agent guide

For coding agents and technical readers. The [participant story](../tutorials/03-check-required-type-information.md) explains the author's direction and the evidence a BIM professional can assess. Read root/project `AGENTS.md` and [current status](../reference/development-status.md); follow the current request, not quoted historical prompts.

The first rule increment started at `f025fce` and was recorded in `c7c2ad9`. Its [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) establishes the three Revit 2024 build 24.3.60.12 cases below. The later `4d40788` checkpoint adds Excel reporting. Commands below operate on the current checkout, which includes that exporter; original no-package and 18-test observations describe the rule milestone only.

## Source and contract map

| Responsibility | Inspect |
|---|---|
| Explicit requirements and scope | [Contract](../../projects/revit-readiness/docs/required-type-information.md), [Checklist.cs](../../projects/revit-readiness/src/readiness-core/Checklist.cs) |
| Host-independent evaluation | [TypeInformation.cs](../../projects/revit-readiness/src/readiness-core/TypeInformation.cs), [core behavior tests](../../projects/revit-readiness/tests/core/Program.cs) |
| Revit observations and read-only command | [RequiredInformationReader.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs), [RequiredInformationCommand.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationCommand.cs) |
| Current saved-report contract | [InformationReport.cs](../../projects/revit-readiness/src/readiness-core/InformationReport.cs) |
| Deliberately editing fixture preparation | [Fixture tool](../../projects/revit-readiness/tests/fixture-tools/PrepareInformationFixture.cs), [recipe](../../projects/revit-readiness/samples/model-recipes/required-information.md) |
| Independent expected answers and comparison | [Answer sheet](../../projects/revit-readiness/samples/expected-results/required-information.md), [Python comparator](../../projects/revit-readiness/tests/verify_information_evidence.py), [host procedure](../../projects/revit-readiness/tests/required-information.md) |

Establish the current Git state before building or changing files. Use the [Git guide](work-with-git.md) to preserve existing work and choose a task-appropriate branch. Keep fixture editing separate from normal installation and read-only verification.

## Translate the BIM request into explicit requirements

“Required” needs a source. A blank fire-rating value alone does not tell us whether a wall should be rated. Our checklist explicitly requires a code on every fixture type and rating/assembly text on Walls A and B. Walls C and D and both ceiling types explicitly do not require those latter fields.

The code lives in **Type Mark**. Rating uses the built-in **Fire Rating** parameter. A GUID-identified text type parameter, **BAL_AssemblyReference**, holds the synthetic assembly reference. These mappings were checked against the installed Revit 2024 API documentation. The symbolic identifier is `FIRE_RATING`; do not invent API names from display labels. The [Autodesk shared-parameter guide](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Basic_Interaction_with_Revit_Elements/Parameters/Revit_API_Revit_API_Developers_Guide_Basic_Interaction_with_Revit_Elements_Parameters_Shared_Parameters_html.html) explains the definition and binding model.

Scope is a list of element UniqueIds in the checklist, on one declared level and New in one declared phase. The report states that scope, counts other host walls/ceilings outside it, and excludes linked documents. It does not present a check of 16 teaching cases as a review of the entire building.

## Put the rules where they can be tested

The [core evaluator](../../projects/revit-readiness/src/readiness-core/TypeInformation.cs) consumes plain type observations and separate requirements. It has no Revit API reference. The [Revit reader](../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs) supplies those observations using built-in parameter IDs and the assembly parameter GUID.

The first 13 behavior tests were run against a stub and all failed because evaluation was not implemented. They then passed against the implementation. Tests address required blanks, whitespace, optional blanks, ceilings, one type affecting multiple instances, multiple missing fields without double counting, absent parameters, unsupported storage, unknown requirements, category mismatch, invalid rules, and preserved evidence. Code review led to five more regressions for the full six-type counts, mixed unsupported data, and duplicate instances across types; the suite now has 18 tests.

From the repository root:

```powershell
& .\projects\revit-readiness\scripts\Test-Readiness.ps1
& .\projects\revit-readiness\scripts\Build-Readiness.ps1
```

The core test executable is a small net48 console harness with a nonzero exit status on any failed assertion. It runs the actual evaluator and requires no Revit process or test-library packages. It is not a `dotnet test` project. The original `c7c2ad9` implementation used .NET Framework's JSON serializer and added no NuGet dependency. Current scripts also build/test the later Excel exporter using locked packages and a separate host-load test; follow the [dependency guide](add-a-plugin-dependency.md) for those additional checks. Do not describe the current full readiness build as package-free.

## Install the read-only command

Close Revit, then run:

```powershell
& .\projects\revit-readiness\scripts\Install-Readiness.ps1
```

This installs **BIM Agent Lab - Required type information** under the current user's Revit 2024 Addins folder. It has its own manifest and folder; the original probe remains available. The original installation included the add-in DLL and the host-independent core DLL. Current installation also deploys the isolated Excel exporter, runtime packages, configuration, and notices, verifies hashes, and synchronizes a recognized existing fixture core. Use the [current install script](../../projects/revit-readiness/scripts/Install-Readiness.ps1); copying only the two original DLLs is now insufficient.

Revit may show an unsigned-add-in prompt for locally built code. Inspect the command name and local DLL path. Respect the current tool's security-prompt rules and request the necessary human action when required. The author allowed both commands in the original session. This handoff is not a reason to request approval for every routine technical step.

## Prepare reproducible sample copies

Use the [fixture recipe](../../projects/revit-readiness/samples/model-recipes/required-information.md). The sample is Autodesk's installed Snowdon Towers Architectural model, also listed on the [official Revit 2024 sample page](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-61EF2F22-3A1F-4317-B925-1E85F138BE88.htm).

A separate, opt-in fixture tool creates three saved copies and two itemized native Revit schedules in each. It duplicates source types and creates 16 marked instances in an isolated area. It introduces the project-wide assembly-parameter binding, then changes values only on the duplicated teaching types. The original installed model is never opened for editing.

The editing tool is not part of normal readiness installation. Build and install it only to reproduce the teaching fixture:

```powershell
& .\projects\revit-readiness\scripts\Prepare-SampleCopy.ps1
& .\projects\revit-readiness\scripts\Build-Readiness.ps1 -IncludeFixtureTools
# Revit must be closed:
& .\projects\revit-readiness\scripts\Install-Readiness.ps1 -IncludeFixtureTools
```

The copy script refuses to overwrite an existing source copy. For a second run, supply a fresh folder under `samples/local/`, and pass the resulting source-copy path as `-FixtureSourceCopy` when installing the fixture tool. Preparing the fixtures also refuses to overwrite any of the three output RVTs.

Start Revit, close any active project, return from Home to the ribbon, and choose **Add-Ins > External Tools > BIM Agent Lab - Prepare teaching sample copies**. This is an explicitly editing preparation operation. The later required-information command uses `TransactionMode.ReadOnly`.

## Read, inspect, and compare

Open a prepared RVT and choose **Add-Ins > External Tools > BIM Agent Lab - Required type information**. It reads `<model-name>.checklist.json` beside the saved project and writes `<model-name>.readiness.json` plus a text detail report. Current source additionally writes `.readiness.xlsx` through the isolated exporter; that later behavior is verified in the [Excel acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md). The checklist contains a schema version, checklist identity, project identity/filename, level/phase UniqueIds, case identities, and explicit requirements per type.

Each type is checked once. A missing type field creates one finding and lists every affected in-scope instance. Unknown rules and unreadable fields remain unexamined; missing or changed case identities make coverage incomplete. The output retains raw values, parameter identity, rule/source, type identity, and case/element identities. It distinguishes absent parameters from blank values and preserves explicit exceptions.

The [independent answer sheet](../../projects/revit-readiness/samples/expected-results/required-information.md) expects:

| Copy | Findings | Distinct affected instances |
|---|---:|---:|
| Complete | 0 | 0 |
| Wall defects | 2 | 6 |
| Additional ceiling gap | 3 | 8 |

Inspect **BAL INFO Walls** and **BAL INFO Ceilings** in Revit's Project Browser. They display the case Marks, type names, codes, and applicable rating/assembly values. Their exported cells are independent of the readiness reader and evaluator. Compare the cells with the answer sheet, then run the optional standard-library Python aid:

```powershell
python .\projects\revit-readiness\tests\verify_information_evidence.py .\projects\revit-readiness\samples\local\required-information
```

Python is needed only for this comparison aid, not to build or run the add-in. Repeat the readiness command and compare results. Hash saved RVTs before and after the reads; also verify Revit's modified flag remains false. Hashing an open RVT may need a shared read handle or closing the project first.

## Diagnose failures at the relevant boundary

- **Input or coverage failure:** inspect checklist schema, model identity, case UniqueIds, level/phase, category, and field storage. Unknown or unreadable data is unexamined, not a pass. Do not silently broaden the declared scope.
- **Fixture preparation failure:** inspect API return semantics and transaction status before saving. Review found that `Parameter.Set` returning `false` for an unchanged value need not indicate failure, and a transaction may fail to commit. Those paths were corrected before the original fixture execution; inspect the retained source and dated review evidence when extending them.
- **Correct totals, wrong instances:** compare every native schedule row and literal expected case, then check report IDs, rule sources, values, and distinct affected counts. Counting the same instance twice can conceal aggregation errors.
- **Workbook or upgraded installation failure:** use the [Excel diagnostics](add-a-plugin-dependency.md). Preserve JSON/text and report export failures explicitly; a stale workbook is not fresh evidence.

Retain actual errors and separate observations from proposed causes. Do not regenerate existing fixtures just to investigate a read-only report failure.

## Preserve both learning layers

Review and checkpoint source, tests, the declared answer sheet, exported evidence, and the observed verification record. Keep RVTs, Autodesk binaries, generated build output, and local sidecars out of Git. Record failures and corrections without rewriting them as first-attempt successes. Update the [participant story](../tutorials/03-check-required-type-information.md), this guide, the journal, and dated acceptance together using the [lesson template](../../templates/lesson.md). Historic passes do not verify a new change.

The user supplies purpose and constraints; the agent supplies a concrete checklist, bounded scope, implementation, and evidence. A passing completeness check says the required text is present. It does not validate the fire rating, the assembly, physical top conditions, quantities, or construction readiness. MCP remains a later layer around this deterministic operation.
