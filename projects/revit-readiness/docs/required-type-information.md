# Required type information: first implementation contract

This implements the first question in the demo brief for Revit 2024. The user authorized building the check, preparing a copy of a sample project, verifying independently, and recording the work. Model edits belong only to a separately installed fixture-preparation tool; the readiness command is read-only.

## Checklist and scope

Use a local copy of Autodesk's Snowdon Towers Architectural sample. Add a controlled area containing 12 basic walls (three each of A, B, C, D) and four ceilings (two each of ACT and GWB). Duplicate types before changing them. Revit's instance Mark identifies teaching cases; a sidecar checklist records their UniqueIds, level, phase, type requirements, and project identity. Scope is this explicit case list, not the building or current view. Exclude links and elements outside that list; report unavailable, unsupported, or changed-scope cases explicitly.

| Field | Revit source | Requirement |
|---|---|---|
| Partition/ceiling code | Type Mark, `ALL_MODEL_TYPE_MARK` | All six fixture types |
| Fire-rating text | `FIRE_RATING` on the type | Walls A and B only |
| Assembly reference | Text type parameter `BAL_AssemblyReference`, GUID `cd425f17-1bb4-4d3f-a088-902728147491` | Walls A and B only |

These are explicit workshop requirements, not inferred design intent. Blank or whitespace required text is missing. An absent parameter is distinguishable from an empty value; an unsupported storage type is unexamined. Non-required fields are labeled NotRequired. Unknown type requirements never produce a pass. Nonempty text is a completeness result only; `60 min` and synthetic `DEMO-ASM-01` are teaching data, not assembly certification.

## Observable acceptance

1. Complete baseline: six used types, 16 instances, zero findings.
2. Wall-defect copy: Wall B rating blank and Wall D code blank; two type-field findings, six distinct affected wall instances.
3. Ceiling-negative copy: additionally clear ACT's code; three findings, eight distinct affected instances, including two ceilings.
4. Repeat reads: same evidence, no change to Revit's modified flag or saved RVT bytes.
5. Independent verification: itemized Revit wall/ceiling schedules with case Mark, type, Type Mark, fire rating, and assembly reference; inspect displayed schedules and compare exported cells with a separately authored answer sheet. The comparison must not call the readiness evaluator.

The larger demo's top offsets, quantities, geometry controls, and MCP are outside this increment.

## Implementation and verification sequence

- [x] Add a host-independent evaluator and meaningful failing tests for missing/optional fields, ceilings, unique affected counts, unknown requirements, unsupported storage, and malformed input; implement until these pass.
- [x] Add a Revit adapter and read-only command. Validate the checklist, gather scoped instances/types by stable identifiers, and retain raw values, rule IDs, element identities, scope exclusions, and completeness in a JSON report.
- [x] Add an opt-in fixture tool separate from the normal add-in. Prepare complete, wall-defect, and ceiling-negative copies and independent Revit schedules. Preserve the original sample hash.
- [x] Build/install, run the three cases and repeated reads in Revit, inspect independent schedules, and compare results.
- [x] Record tutorial, source/API evidence, exact versions, hashes, corrections, and limitations; review and commit the checkpoint.

Use the existing net48/x64 toolchain. The evaluator and its console test harness require no Revit process or third-party packages. JSON uses the .NET Framework serializer. Build/test/install scripts remain project-local. The normal installation does not expose fixture editing commands.
