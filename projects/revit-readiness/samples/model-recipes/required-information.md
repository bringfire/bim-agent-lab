# Required-information fixture recipe

Source: the installed **Snowdon Towers Sample Architectural.rvt** for Revit 2024. Obtain it from the [Autodesk sample page](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-61EF2F22-3A1F-4317-B925-1E85F138BE88.htm) if absent. Keep the original unchanged and record its SHA-256.

1. Run `scripts/Prepare-SampleCopy.ps1` from this project to make `samples/local/required-information/Snowdon-BAL-source-copy.rvt`. The script compares source/copy hashes and refuses overwrite.
2. Build with `scripts/Build-Readiness.ps1 -IncludeFixtureTools`, then close Revit and install with `scripts/Install-Readiness.ps1 -IncludeFixtureTools`.
3. Start Revit. Handle any unsigned-add-in prompt yourself. With no project open, invoke **BIM Agent Lab - Prepare teaching sample copies** from External Tools.
4. The tool opens the copy with detach-and-discard-worksets options, inspects its levels/phases and candidate types, and writes `source-inspection.json`. It chooses the lowest level at or above elevation zero (or the lowest available level), the last phase, and alphabetically first basic-wall/ceiling source types.
5. It duplicates four wall types and two ceiling types; creates 12 unjoined, 10-foot-long, 9-foot-high walls and four 10-by-10-foot ceilings at a 9-foot offset in an area starting at internal XY (1000, 1000) feet. All cases are assigned to the selected phase. Case Marks and UniqueIds provide scope; visibility is not the selector.
6. It binds `BAL_AssemblyReference` as a text **type** parameter for Walls and Ceilings, GUID `cd425f17-1bb4-4d3f-a088-902728147491`. The temporary shared-parameter filename is restored afterward. Synthetic assembly data is for teaching only.
7. It creates an isolated 3D view and itemized, case-filtered wall/ceiling schedules. It saves the complete model, blanks Wall B's rating and Wall D's code and saves a second model, then blanks ACT's code and saves a third. Every model gets a checklist and native Revit schedule exports.
8. Run the readiness command in each saved file. Inspect the schedules and compare with the [answer sheet](../expected-results/required-information.md). Record actual values, counts, hashes, and screenshots in the [verification record](../../docs/verification/2026-10-20-required-information.md).

The three names are `BAL-info-complete.rvt`, `BAL-info-wall-defects.rvt`, and `BAL-info-ceiling-negative.rvt`. Their checklists share the requirement meanings but identify their own project filenames. The tool refuses to overwrite these files; choose a fresh local folder for a reset run. Keep a prepared local reset set for the seminar.

The tested source types were `Block 37 Pilaster` and `2' x 2' ACT System`. The six names identify information cases; the fixture does not redesign their physical constructions. The GWB-labeled ceiling therefore still derives from ACT. Do not use these names as evidence of correct material assemblies.

No RVT or Autodesk template is redistributed. This fixture implements only required type information. The larger brief's offsets, geometry controls, measured quantities, and MCP remain separate work.
