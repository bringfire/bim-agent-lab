# Check required wall and ceiling type information

This increment turns the first API read into a declared readiness check. It targets Revit 2024 and reuses the established net48 toolchain. See the [implementation contract](../../projects/revit-readiness/docs/required-type-information.md) for the exact rules and the [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) for what has actually been exercised.

Actual author prompt:

> Ok, let's build the first readiness check for missing required wall and ceiling type information. Use a copy of a Revit sample project, verify the results independently, and record the process in bim-agent-lab.

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

The test executable is a small net48 console harness with a nonzero exit status on any failed assertion. It runs the actual evaluator and requires no Revit process or test-library packages. It is not a `dotnet test` project. The build uses .NET Framework's JSON serializer; no NuGet dependency was added.

## Install the read-only command

Close Revit, then run:

```powershell
& .\projects\revit-readiness\scripts\Install-Readiness.ps1
```

This installs **BIM Agent Lab - Required type information** under the current user's Revit 2024 Addins folder. It has its own manifest and folder; the original probe remains available. The installation includes the add-in DLL and the host-independent core DLL, and verifies their hashes.

Revit may show an unsigned-add-in prompt for locally built code. Inspect the command name and local DLL path before choosing **Load Once**. When the agent's computer-use policy prevents it from acting on a security prompt, the person must make that choice. Do not change security settings to avoid this step.

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

Open a prepared RVT and choose **Add-Ins > External Tools > BIM Agent Lab - Required type information**. It reads `<model-name>.checklist.json` beside the saved project and writes `<model-name>.readiness.json` plus a text detail report. The checklist contains a schema version, checklist identity, project identity/filename, level/phase UniqueIds, case identities, and explicit requirements per type.

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

## Preserve the lesson

Commit source, tests, the declared answer sheet, exported evidence, and the observed verification record. Keep RVTs, Autodesk binaries, generated build output, and local sidecars out of Git. Record failures and corrections without rewriting them as first-attempt successes.

The user supplies purpose and constraints; the agent supplies a concrete checklist, bounded scope, implementation, and evidence. A passing completeness check says the required text is present. It does not validate the fire rating, the assembly, physical top conditions, quantities, or construction readiness. MCP remains a later layer around this deterministic operation.
