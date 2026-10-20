# ClosedXML: first adopted plugin dependency

Observation date: 2026-10-20. Purpose: produce the four-sheet readiness workbook from the existing report. Target: Revit 2024 / .NET Framework 4.8, with the spreadsheet writer isolated in a local net48 process.

## Source inspection and selection

- Package: [ClosedXML 0.105.1](https://www.nuget.org/packages/ClosedXML/0.105.1), exact direct dependency in the exporter project.
- Research clone: ignored `references/ClosedXML`, origin `https://github.com/ClosedXML/ClosedXML.git`, tag `0.105.1`, commit `b4ebe47bd3ecebf8480dea7422188d16b35b9d9a`.
- Inspected [license](https://github.com/ClosedXML/ClosedXML/blob/b4ebe47bd3ecebf8480dea7422188d16b35b9d9a/LICENSE), [project manifest](https://github.com/ClosedXML/ClosedXML/blob/b4ebe47bd3ecebf8480dea7422188d16b35b9d9a/ClosedXML/ClosedXML.csproj), [cell implementation](https://github.com/ClosedXML/ClosedXML/blob/b4ebe47bd3ecebf8480dea7422188d16b35b9d9a/ClosedXML/Excel/Cells/XLCell.cs), and [graphics engine](https://github.com/ClosedXML/ClosedXML/blob/b4ebe47bd3ecebf8480dea7422188d16b35b9d9a/ClosedXML/Graphics/DefaultGraphicEngine.cs).
- The package targets .NET Standard 2.0/2.1; the net48 project selects the compatible assets. We use its high-level workbook/table API and published package, not copied source.
- CSV would need no extra dependency but cannot retain sheets/styles. Microsoft Open XML SDK is a lower-level alternative already used beneath ClosedXML. No Excel application, Office interop, cloud service, or extra Revit add-in is required to generate the report.
- Source-build tooling such as Fody, Janitor.Fody, SourceLink, and Polyfill is private to upstream's build; it is not part of our resolved runtime package graph.

## Resolved net48 package graph

The authoritative [lock file](../../projects/revit-readiness/src/excel-report/packages.lock.json) includes dependency edges and content hashes. The [evidence manifest](../../projects/revit-readiness/third-party/dependency-evidence.json) includes exact package versions, package metadata, source revisions when available, notice paths, and SHA-256 hashes.

| Package | Version | Role | Observed package terms |
|---|---|---|---|
| ClosedXML | 0.105.1 | Direct workbook API | MIT |
| ClosedXML.Parser | 2.0.0 | Formula parser used by library | MIT |
| DocumentFormat.OpenXml | 3.1.1 | Office XML structures | MIT |
| DocumentFormat.OpenXml.Framework | 3.1.1 | Office package framework | MIT |
| ExcelNumberFormat | 1.1.0 | Excel number formats | MIT |
| Microsoft.Bcl.HashCode | 1.1.1 | Runtime utility | MIT; packaged third-party notices retained |
| RBush.Signed | 4.0.0 | Spatial indexing used by library | MIT |
| SixLabors.Fonts | 1.0.0 | Font metrics | Apache-2.0 |
| System.Buffers | 4.5.1 | Runtime utility | Packaged MIT license; third-party notices retained |
| System.Memory | 4.5.5 | Runtime utility | Packaged MIT license; third-party notices retained |
| System.Numerics.Vectors | 4.5.0 | Runtime utility | Packaged MIT license; third-party notices retained |
| System.Runtime.CompilerServices.Unsafe | 4.7.0 | Runtime utility | MIT; packaged third-party notices retained |

These are the versions actually restored, not the latest possible transitive versions. Do not apply the license of a newer SixLabors release to 1.0.0 or assume future versions retain these terms. Any version change requires a new review of the resolved graph and host behavior.

ClosedXML embeds stripped **CarlitoBare** font resources. Their TTF name table identifies Copyright 2013 The Carlito Project Authors. The included [OFL notice](../../projects/revit-readiness/third-party/Carlito/OFL.txt) was obtained from the named Google Fonts project at the revision recorded in the evidence manifest. That source revision identifies the retrieved license text, not the unknown original font-build revision. We do not modify or install the embedded fonts separately.

## Notices and compatibility findings

Preserve MIT copyright/permission notices, the Apache 2.0 license and applicable notices, the font's OFL/copyright notice, and packaged Microsoft third-party notices. The full retained text is under [third-party](../../projects/revit-readiness/third-party/README.md), and installation copies it with the plugin. No upstream source was modified. This review does not choose a license for original BIM Agent Lab material.

The dated [NuGet audit](../../projects/revit-readiness/docs/verification/excel-evidence/nuget-audit.json) reported no known vulnerable packages from the queried source. This is a point-in-time advisory check, not a guarantee of safety or future compatibility.

The initial in-process Revit attempt failed although console tests passed. The console had binding redirects for System.Memory, Buffers, Vectors, and Unsafe; the shared Revit process did not supply the same environment. The final exporter runs in its own short-lived process with its own generated configuration. [Runtime verification](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) records the actual result. No global assembly resolver or change to Revit.exe.config is used.
