# Excel readiness report

`BimAgentLab.Readiness.Excel.exe` is a local net48 console exporter, not a service. It accepts `<report.json> <output.xlsx>` and creates exactly Summary, Findings, Affected elements, and Rules and coverage. It uses ClosedXML 0.105.1 and the locked transitive package graph. No Revit objects, Office interop, or installed Excel are needed.

The Revit command calls this helper in a hidden child process after saving its report. Isolating the exporter avoids loading third-party spreadsheet assemblies into Revit's shared process. Keep the EXE, generated `.exe.config`, core DLL, and runtime dependencies together. The local install script handles that and ships notices. The dependency is project-local; core rules and the original probe stay free of NuGet dependencies.

See the [tutorial](../../../../docs/tutorials/04-add-an-excel-report-dependency.md), [dependency review](../../../../docs/research/closedxml-dependency.md), and [verification](../../docs/verification/2026-10-20-excel-output.md). Workbooks are static evidence, not a way to edit the model or rerun checks. The writer uses literal strings and numeric counts, explicit incomplete coverage, distinct affected instances, and atomic destination replacement.
