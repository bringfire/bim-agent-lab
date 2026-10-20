# Revit add-in

The first implementation is `BimAgentLab.RevitProbe`, a C# x64 `net48` library for Revit 2024. It provides **Add-Ins > External Tools > BIM Agent Lab - Read-only probe**.

The command reports project identity, Revit build, and counts of non-type elements in the Walls and Ceilings categories. It reads the entire active host document, excludes linked documents, and applies no view/selection/phase/design-option/level filter or result limit. `TransactionMode.ReadOnly` enforces read-only execution; the dialog also reports the document's modified flag before and after the reads.

Follow the [tutorial](../../../../docs/tutorials/02-build-a-read-only-revit-command.md) for build and installation, the [acceptance procedure](../../tests/read-only-probe.md) for testing, and the [verification record](../../docs/verification/2026-10-20-read-only-probe.md) for observed results. API references come from the installed Revit 2024 folder with Copy Local disabled. There are no NuGet packages.

Future responsibility: collect normalized element/type data and provide readiness commands and evidence navigation in a valid Revit API context. Counts alone are not takeoff quantities or readiness findings. No bridge or MCP endpoint is present yet.
