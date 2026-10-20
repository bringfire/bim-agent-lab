# Revit add-in

The first implementation is `BimAgentLab.RevitProbe`, a C# x64 `net48` library for Revit 2024. It provides **Add-Ins > External Tools > BIM Agent Lab - Read-only probe**.

The command reports project identity, Revit build, and counts of non-type elements in the Walls and Ceilings categories. It reads the entire active host document, excludes linked documents, and applies no view/selection/phase/design-option/level filter or result limit. `TransactionMode.ReadOnly` enforces read-only execution; the dialog also reports the document's modified flag before and after the reads.

Follow the [tutorial](../../../../docs/tutorials/02-build-a-read-only-revit-command.md) for build and installation, the [acceptance procedure](../../tests/read-only-probe.md) for testing, and the [verification record](../../docs/verification/2026-10-20-read-only-probe.md) for observed results. API references come from the installed Revit 2024 folder with Copy Local disabled. The probe has no NuGet packages.

`BimAgentLab.RevitReadiness` adds **BIM Agent Lab - Required type information** as a separate command and project. Its reader resolves an explicit checklist scope, reads type parameters by built-in IDs/shared GUID, and passes normalized observations to the core. Its command writes local JSON/text evidence, invokes the isolated [Excel exporter](../excel-report/README.md), and displays findings under `TransactionMode.ReadOnly`. Follow the [readiness lesson](../../../../docs/tutorials/03-check-required-type-information.md) and [acceptance procedure](../../tests/required-information.md).

Both project files list their source files explicitly so the original probe remains independently buildable. The fixture editor lives separately under `tests/fixture-tools` and is installed only with an explicit switch. No bridge or MCP endpoint is present yet.
