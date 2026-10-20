# Four-wall setup fixture

Purpose: prove that the minimal command builds, loads, and reads the active Revit project. This is separate from the planned 12-wall/four-ceiling readiness demonstration.

Tested on Revit 2024, build `24.3.60.12`, on 2026-10-20.

1. In Revit, create a **Project** using **Imperial Multi-discipline**. The author used the installed template `C:\ProgramData\Autodesk\RVT 2024\Templates\English-Imperial\Default-Multi-discipline.rte`.
2. Run the probe before adding geometry. Expected and observed: **0 walls, 0 ceilings**. If your template contains geometry, establish its actual baseline or choose an empty template.
3. Open **L1 - Architectural**. Use **Architecture > Wall**, type **Basic Wall: Generic - 8"**, unconnected height **20 feet**, and the rectangle drawing tool. Draw one rectangle. Dimensions are arbitrary for this count-only test.
4. Exit the wall tool. Select the four segments and verify **Walls (4)** in the Properties palette. This provides an independent count. Do not create a ceiling.
5. Save as `projects/revit-readiness/samples/local/read-only-probe.rvt`. Create the local folder if needed. Clear the selection.
6. Establish the saved file's SHA-256 before running the command. If the open file is locked for normal hashing, close it, hash it, and reopen it.
7. Run **Add-Ins > External Tools > BIM Agent Lab - Read-only probe** twice. Expected: **4 walls, 0 ceilings**, `False before / False after` for the modified flag, and the correct project/build in details.
8. Close the project without saving again and compare its hash with the baseline. The command should be unavailable with no active project.

Reset by creating a fresh project from the same template and repeating the recipe. A newly created RVT need not have the author's byte hash; compare each fixture's own before/after hashes. Keep a local reset copy if useful.

The model and Revit sidecars stay in ignored `samples/local/`. No Autodesk template or RVT is redistributed. The [verification record](../../docs/verification/2026-10-20-read-only-probe.md) retains screenshots, expected/observed results, and the author's file hash.
