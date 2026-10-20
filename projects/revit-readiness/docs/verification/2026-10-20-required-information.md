# Required type information: verification record

Date: 2026-10-20. Implementation branch: `feat/required-type-information`, based on `f025fceffa108fb096941eb1da4486a48eda89c7`.

**Result: passed on Revit 2024 build 24.3.60.12.** All three saved sample-derived projects returned the declared findings and exact affected cases. Native schedule exports matched the independent answer sheet. The saved RVTs and original source sample remained byte-identical through the readiness reads and application close.

## Build and source evidence

- Copied the installed Snowdon Towers Architectural sample to `samples/local/required-information/Snowdon-BAL-source-copy.rvt`. Both were 94,531,584 bytes and had SHA-256 `690A69B77BA4000DFC4BD04E9CF52E9885BFE3986F4149659EE6EE24B03300E5`.
- Inspected the installed Revit 2024 XML API documentation for ceiling/wall creation, parameter identifiers, shared-parameter binding, phase status, and schedule export. `FIRE_RATING`, `ALL_MODEL_TYPE_MARK`, and the recorded shared GUID identify the type fields without localized-name lookup.
- The first 13 behavior tests compiled and then failed against the evaluator stub (0/13), before evaluation was implemented. They subsequently passed (13/13). Review added five aggregation/identity regressions; the final suite passed **18/18**.
- Readiness and opt-in fixture-tool Release builds passed with zero warnings/errors using .NET SDK 10.0.204, net48/x64, and Revit/API build 24.3.60.12. No NuGet packages or prerequisite installations were added.
- Installed separate per-user manifests and matching DLLs. The original probe installation was retained. [Assembly hashes](required-information-evidence/assembly-hashes.json) tie the tested installed files to the built output.

## Observed host acceptance

| Case | Expected findings | Expected affected instances | Observed |
|---|---:|---:|---|
| Complete data | 0 | 0 | 0 findings / 0 affected; all six types pass |
| Two wall-type gaps | 2 | 6 | Wall B rating and Wall D code missing; three walls each |
| Additional ACT ceiling-code gap | 3 | 8 | Same six walls plus two ACT ceilings |
| Repeat query and saved-file integrity | Same findings; modified false/false; unchanged RVT hash | — | Pass: two ceiling-negative reads identical except GeneratedUtc |
| Native schedules against independent answer sheet | 16 exact cases and declared values | — | Pass: all rows, field values/statuses, and affected case IDs in all three files |

Each run examined 16/16 cases, six used types, no unexamined cases, and complete coverage for the supplied checklist. Scope was **L1 - Block 43**, **New Construction (New)**. Each report excluded 1,196 other host wall/ceiling instances and six link instances. Every observed command run reported modified **False before / False after**.

The [answer sheet](../../samples/expected-results/required-information.md) was authored from the declared cases before executing the command. The [comparison script](../../tests/verify_information_evidence.py) reads Revit schedule exports independently of the evaluator. It returned [three PASS records](required-information-evidence/independent-comparison.json). The native wall and ceiling schedules were also opened and visually inspected in Revit. [Retained evidence](required-information-evidence/README.md) includes the schedules, reports, checklists, source inspection, screenshots, repeat comparison, and hashes; participants can rerun the comparison without Revit against these text records.

## Sample preparation observations

The installed sample and initial copy were identical before preparation. The separate fixture command opened that copy with detach-and-discard-worksets, duplicated `Block 37 Pilaster` and `2' x 2' ACT System` as teaching types, bound the shared text type parameter, and created 12 basic walls plus four ceilings. [Source inspection](required-information-evidence/source-inspection.json) records actual levels, phases, parameter storage, and values. The initial wall Type Mark/Fire Rating were blank; ceiling Type Mark was C03.

The six labels identify data cases; the duplicated physical constructions were not redesigned to represent the names. In particular, the GWB-labeled ceiling still derives from the ACT source type. This is a text-information fixture, not a physical assembly or quantity fixture. No claims about the original sample's design intent were inferred from its blanks.

The tool saved three RVTs and exported itemized native schedules before readiness execution. [Before-read hashes](required-information-evidence/hashes-before-readiness.json) and [after-close hashes](required-information-evidence/hashes-after-close.json) agree for all prepared files. The source copy and installed Autodesk sample both retained SHA-256 `690A69B77BA4000DFC4BD04E9CF52E9885BFE3986F4149659EE6EE24B03300E5`. Revit closed normally after testing, with no further model save.

## Review and interaction lessons

A separate source review found two fixture failure paths before fixture execution. First, an unchanged `Parameter.Set` can return false; the corrected setter accepts already-equal values and checks the resulting text, following the [Autodesk parameter contract](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Basic_Interaction_with_Revit_Elements/Parameters/Revit_API_Revit_API_Developers_Guide_Basic_Interaction_with_Revit_Elements_Parameters_Parameter_html.html). Second, `Transaction.Commit` can return a non-committed status without throwing; every fixture transaction now requires Committed before saving. Forced modal failure handling prevents saving while failure processing remains pending. The follow-up source review reported both findings resolved. These were review-discovered risks, not failed host runs.

Revit initially displayed its unsigned-add-in security prompt. The agent handed that choice to the user, who allowed the commands and reported choosing Always Load. After installing the reviewed fixture build with Revit closed, the restarted host loaded without another prompt. There were no fixture-preparation or readiness-command failures in the executed host cases.

## Limits

Repository checks also passed: 240 relative Markdown links resolved, JSON and PowerShell/project XML parsed, the retained original probe still built, and the staged diff contained no working RVT files or Autodesk/build binaries. The original probe's project now lists its source explicitly so adding the readiness command does not change that earlier example.

The implementation checks text completeness for an explicit host scope, not the entire sample or linked models. Nonempty rating/assembly text is not validated for correctness. Geometry, quantities, physical top conditions, other Revit builds, and MCP are not covered. Unknown requirements and unsupported values were exercised offline; malformed sidecars and changed-scope cases were not separately exercised inside Revit in this increment. The copied models remain local and ignored, with recipes provided for reproduction.
