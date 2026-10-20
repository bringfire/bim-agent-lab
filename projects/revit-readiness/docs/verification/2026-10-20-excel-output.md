# Excel output verification

Date: 2026-10-20. Branch: `feat/excel-readiness-report`, base `c7c2ad9`. Target: Revit 2024 build 24.3.60.12 / net48 / .NET SDK 10.0.204.

**Status: passed on the stated configuration.** All three actual Revit runs exported successfully, independent workbook comparisons passed, and all four sheets of the ceiling-negative report were inspected in Microsoft Excel. The first in-process Revit export failed and drove the process-isolation correction below.

## Evidence and results

- The exporter adds exactly Summary, Findings, Affected elements, and Rules and coverage. It consumes the same report saved by the checker, with no new BIM rule or quantity calculation.
- [Initial red tests](excel-evidence/tests-red.txt): all seven workbook tests failed against the unimplemented writer. The [current test output](excel-evidence/tests-green.txt) has 18 core tests, seven workbook tests, and a separate process-isolation test passing.
- [Independent XML comparison](excel-evidence/offline-comparison.json): all three offline exports match the saved reports and independently checked native schedule/oracle evidence. Expected results remain 0/0, 2/6, and 3/8 missing type values/distinct affected instances. All 18 field-rule rows and stable affected element identities are retained in each workbook.
- [Build output](excel-evidence/build.txt): zero warnings/errors. Normal scripts use locked package restore. [Installed file inventory](excel-evidence/installed-files.json) includes deployed binaries/configuration and retained notices with hashes.
- [NuGet advisory query](excel-evidence/nuget-audit.json): no known vulnerable packages were reported by the configured source at this observation. [Package/license evidence](../../third-party/dependency-evidence.json) records twelve resolved NuGet packages and the embedded font notice.
- [Host workbook comparison](excel-evidence/host-comparison.json): fresh JSON/XLSX from all three Revit runs matched the native schedules and literal oracle. The corresponding reports are retained in `excel-evidence/`; native TSVs remain in [the prior acceptance evidence](required-information-evidence/README.md). Workbook/report timestamps were compared to reject stale output.
- Revit success dialogs: [complete: 0/0](excel-evidence/revit-complete.jpg), [wall defects: 2/6](excel-evidence/revit-wall-defects.jpg), and [ceiling negative: 3/8](excel-evidence/revit-ceiling-negative.jpg). All examined 16 declared instances across six types, with complete checklist coverage and `False` before/after modification flags.
- Excel opened the ceiling-negative workbook without a repair prompt. Visual evidence: [Summary](excel-evidence/excel-summary.jpg), [Findings](excel-evidence/excel-findings.jpg), [Affected elements](excel-evidence/excel-affected-elements.jpg), and [Rules and coverage](excel-evidence/excel-rules-coverage.jpg). Headings, wrapped identifiers, counts, optional rules, exclusions, and frozen panes were readable. Excel's number-stored-as-text indicator on element IDs is intentional: IDs are identifiers, not quantities. Wide tables require horizontal scrolling on smaller displays.
- [Before-run hashes](excel-evidence/hashes-before.json) and [after-run comparison](excel-evidence/hashes-after.json): the original sample, source copy, and all three demonstration RVTs remained byte-identical after Revit closed normally without saving.

The standard-library [XML verifier](../../tests/verify_excel_evidence.py) reads XLSX directly instead of using the writer's library. Its first implementation expected only `frozen`; inspecting the workbook XML established that ClosedXML emits the valid `frozenSplit` form. The verifier now accepts either while still requiring frozen row headings. This was a verifier assumption, not a malformed workbook.

## Runtime issue and correction

The initial command ran inside Revit and correctly returned zero findings for the complete model, but Excel export failed with `AstFactory.ErrorValue` / method-implementation mismatch. JSON/text remained available and the dialog explicitly reported the Excel failure. The [captured dialog](excel-evidence/initial-host-failure.jpg) records that failed attempt.

A separate review reproduced dependency-version differences outside Revit: the console executable's generated redirects masked differences among requested/shipped Memory, Buffers, Vectors, and Unsafe assembly versions. Revit's shared process did not provide the same binding environment. A private AppDomain experiment also failed a proxy cast in the host-load test and was removed before acceptance.

The final design launches a short-lived hidden net48 exporter process, with its own generated EXE configuration and dependency folder. It accepts only saved JSON and XLSX paths, returns an exit code/error text, and is limited to 60 seconds. The Revit add-in has no spreadsheet assembly references and no global resolver/configuration changes. Both built and installed helpers passed the same launcher invoked from a no-redirect test host.

The separate review also found missing identities for elements whose type fields were unexamined. A targeted assertion failed, then passed after Rules and coverage gained explicit case/element/type/rule/reason rows. Missing and unexamined counts remain separate.

An upgrade/load-order review found that the older installed fixture tool could load an older same-identity core DLL first. The [reproduction](excel-evidence/core-order-red.txt) failed to find the new report contract. The install script now synchronizes the backward-compatible core into an existing recognized fixture installation without newly installing/enabling that optional editing command. After Revit closed, normal installation synchronized that core and the [fixture-first load test passed](excel-evidence/core-order-green.txt). Separate review verified that all three installed core copies match and all 51 installed files match the retained inventory. This upgrade case was tested with the load-order harness; the editing command was not rerun because doing so would recreate the samples.

## User handoff and final review

The user handled Revit's unsigned-add-in prompt and replied "loaded." The agent continued documentation while waiting, then completed host execution, comparisons, visual review, and the install upgrade check. The final test/build run reported 18 core tests, seven workbook tests, and the isolated-export harness passing, with zero build warnings/errors. Final targeted code review reported no remaining substantive findings. No remote publish or distribution was performed.

## Limits

The report is a static snapshot of text completeness within the declared checklist. No physical-condition, measured-quantity, certification, MCP, or other-Revit-version behavior is established. Long identifiers and formula-looking strings are tested as literal cell text; locked-file atomic replacement and mixed incomplete cases are tested offline. Timeout behavior has source review but has not been induced inside Revit. Working models and generated XLSX files remain ignored; participants can recreate workbooks from retained JSON using the test script without Revit or Excel.
