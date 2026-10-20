# Model readiness demos and sample model

Date: 2026-10-20
Status: working demo design. Model readiness is the author's confirmed direction; the questions, rules, and controlled examples below are proposed. No Revit model has been opened or modified in this work.

The workshop example helps wall and ceiling BIM staff decide what needs attention before using an incoming model for coordination and quantity review. Inventory and quantities supply the evidence. Readiness means meeting an explicit project checklist for a stated use; it does not certify construction readiness or code compliance.

The three demos tell one story: identify information gaps, investigate modeling conditions, and report the effect on usable quantities.

## Three demonstration questions

### 1. Do we have the information we need?

> Review the walls and ceilings in the SWACCA demo area for the chosen phase. Which types are missing information required by our readiness checklist, and how many elements does each gap affect?

The answer should identify the document, scope, phase, included categories, checklist version, and units. It should summarize types and instances, then report each missing field with its source and affected elements.

The proposed checklist requires a partition code for each in-scope basic wall type. A separate requirement table identifies which types need a fire-rating value and an assembly reference. Non-rated partitions explicitly marked as not requiring these fields pass those checks. STC can become a later optional rule when an acoustic requirement is provided.

Expected answer for the controlled examples: **two type-level findings affecting six walls**. One type lacks a required fire-rating value; another lacks its partition code. The valid non-rated type is not flagged for a rating that is not required. The two ceiling types have complete required data.

This teaches the distinction between a type and its instances: one incomplete type can affect several elements. Missing data must stay distinguishable from “not required.”

### 2. Which modeling conditions need review?

> Show me the walls and ceilings whose top constraints or height offsets disagree with our demo requirements. Explain what was expected, what the model contains, and which elements I should inspect.

The answer should provide an element list, observed values, expected values, rule identifiers, and the ability to select or zoom to each finding. Requirements must come from the checklist, not from an agent guessing the designer's intent.

Expected answer: **one wall with the wrong top offset and one ceiling with the wrong height offset**. An intentionally unconnected partition that satisfies its stated height requirement must pass.

For this first example, “top condition” means checking the modeled constraint and offset against a declared requirement. Confirming physical contact with a slab or deck would require additional geometry and link analysis and is a separate feature.

This teaches evidence-based explanations and a useful agent interaction: a professional can move from a summary to the specific element behind it.

### 3. Which quantities can we use, and which need review?

> Summarize wall counts, lengths, and modeled areas, plus ceiling counts and modeled areas, by level and type. Separate elements that pass our readiness checks from those needing review, explain the exclusions, and export the report.

The answer should retain both sets of quantities. A readiness finding does not make a measured area disappear. Each row should identify its source elements, units, quantity basis, and review reasons.

Expected classification for the controlled examples:

| Category | Pass the demo checks | Need review | In-scope total |
|---|---:|---:|---:|
| Basic walls | 5 | 7 | 12 |
| Ceilings | 3 | 1 | 4 |

These are planned fixture counts, not observations about Snowdon Towers. Numeric totals will be established from the prepared model and independently checked before the demo.

Use “Revit modeled wall area” for the initial area field. Do not label it board area per face, procurement quantity, or universally correct net takeoff. A separate board calculation would need explicit rules for layers, faces, openings, joins, waste, and special cases.

This teaches how to request a result that is traceable and testable rather than accepting a plausible total.

## Sample model choice

Use a working copy of the installed **Snowdon Towers Sample Architectural.rvt** for the building context. Autodesk describes Snowdon Towers as a coordinated project split across architectural, structural, and MEP files, with walls, ceilings, schedules, and other commonly used features. [Autodesk's project guide](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-5DA26FF7-85B3-4E9D-A9F2-FADDD02D88D6.htm)

The following files were found locally:

| Role | Verified local source |
|---|---|
| Primary candidate | `C:\Program Files\Autodesk\Revit 2024\Samples\Snowdon Towers Sample Architectural.rvt` |
| Later link/structure example | `C:\Program Files\Autodesk\Revit 2024\Samples\Snowdon Towers Sample Structural.rvt` |

The local files' particular types, parameters, phases, links, and quantities still require inspection inside Revit. Their filenames and the project guide do not establish those details.

The [Revit 2024 sample download page](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-61EF2F22-3A1F-4317-B925-1E85F138BE88.htm) also supplies `rac_basic_sample_project.rvt` as a smaller fallback. That project was not found in the local Samples folder.

Work from a copy and give the demo area its own views and explicit scope. Duplicate any types that we change so that demonstration defects do not alter unrelated sample elements. The report must say whether it covers the demo area or the entire building.

## Controlled examples to prepare

Create or adapt a compact area on one selected level, in one selected phase. Use a documented instance identifier such as `SW_DemoCaseId` to define the fixture membership independently of view visibility. The exact level and phase IDs are recorded during preparation.

### Types and elements

| Type | Instances | Required information | Deliberate condition |
|---|---:|---|---|
| Wall A | 3 | Partition code, rating, assembly reference | Information complete; one instance has the wrong top offset |
| Wall B | 3 | Partition code, rating, assembly reference | Required fire-rating value is blank |
| Wall C | 3 | Partition code; rating and assembly reference explicitly not required | Valid non-rated control; includes intentionally unconnected walls |
| Wall D | 3 | Partition code; other requirements stated in checklist | Partition code is blank; other checks pass |
| Ceiling ACT | 2 | Ceiling code and expected height offset | One instance at 8 ft instead of the specified 9 ft |
| Ceiling GWB | 2 | Ceiling code and expected height offset | Both pass |

Use synthetic assembly references such as `DEMO-ASM-01`, clearly labeled as teaching data. A reference being present is a completeness check, not validation of a tested assembly.

The wall-top defect should have a declared target level and an expected offset of 0 ft, with an actual offset of -1 ft. Compare offsets in explicit units with a documented tolerance, initially 1/16 inch. Every other wall must satisfy its own stated condition, including the allowed unconnected partitions.

Where custom parameters are needed, record their GUIDs, storage types, category bindings, and whether they belong to types or instances. Keep the checklist requirements outside the observed parameter values: deleting a value must not also delete the rule requiring it.

### Quantity controls

Use two of Wall C's instances as isolated, unjoined rectangular controls:

- A wall 10 ft long and 9 ft high, with no opening: geometric reference area 90 sq ft.
- A matching wall with a 3 ft by 7 ft rectangular opening: simple geometric reference area 69 sq ft after subtraction.

These are deliberately simple analytical checks. Verify the actual opening geometry and Revit's reported values rather than assuming every door family's nominal dimensions create that exact cut.

Use simple rectangular ceilings with measured boundaries for an independent area check. Compare tool totals against schedules with identical scope, phase, category, type grouping, and inclusion rules. For the controlled rectangles, target agreement within 0.01 ft for lengths and 0.01 sq ft for areas before display rounding; explain any discrepancy before using the result on stage.

### Controls for scope and unsupported elements

Include examples outside the main 16-element set:

- One wall and one ceiling on another level.
- One wall outside the selected phase's inclusion rules.
- One curtain wall or other special wall condition outside the initial basic-wall quantity method.
- A visible linked-model entry, if present, whose inclusion status is reported explicitly.

These controls must not silently enter the main totals. Unloaded links and unsupported cases should be reported as unexamined or excluded with reasons, never as zero quantities. The first three demos can use host-only quantities while exposing this limitation in the scope summary.

## Preparation and verification sequence

1. Inspect a copy of the architectural sample in Revit and select a legible demo area, level, and phase.
2. Inspect representative elements using Revit properties, schedules, and a Revit 2024 build of RevitLookup.
3. Record existing parameters and map them to the checklist before adding custom fields.
4. Prepare a complete baseline with the 12 walls and four ceilings. Record case IDs, element UniqueIds, type IDs, requirements, and expected quantities.
5. Save a second copy with the four deliberate findings: two type defects, one wall-top defect, and one ceiling-height defect.
6. Create a manual answer sheet. Record four findings, eight distinct affected elements, and the ready/review counts above. Type findings must not be double-counted as extra elements.
7. Verify the plugin and MCP outputs against that same answer sheet. Repeating a query must not change the model.
8. Keep a prepared reset copy and a short recorded demonstration for the workshop.

The answer sheet must be authored from controlled preparation and independent checks, not generated solely by the tool being tested.

## Relationship to the implementation

Each question needs a small set of deterministic operations: establish scope, read elements and types, evaluate configured rules, aggregate quantities, and return source references. The MCP layer exposes those operations to an agent; it should not invent the rules or perform the quantity arithmetic independently.

The immediate research and resource decisions are recorded in [Research and resources](../../../docs/research/revit-resources.md). The prompts and lessons from this design work are recorded in [Build journal](../../../docs/build-story/journal.md).
