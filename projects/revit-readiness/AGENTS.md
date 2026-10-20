# Revit readiness guidance

Follow the root [AGENTS.md](../../AGENTS.md), then read this project's [README](README.md) and [demo brief](docs/demo-questions-and-sample-model.md). Technical procedures are in the [setup guide](../../docs/how-to/build-read-only-revit-command.md) and [readiness guide](../../docs/how-to/build-required-information-check.md). Their linked participant stories explain the human's direction; retain both layers when extending this example.

## Requirements

- Lead with model readiness. Inventory and quantities explain the findings.
- Target a standalone Revit add-in plus an MCP server. Rook and Rhino.Inside are precedents, not required runtime dependencies.
- Revit 2024 is required. Other versions and point releases must be verified before being described as supported.
- Begin with read-only model queries. A query must not change the model. Use separate working copies when preparing deliberate sample defects.
- Define document, categories, level/phase scope, linked-model treatment, exclusions, and completeness. A current-view query with a result limit is not automatically a whole-model inventory.
- Evaluate declared requirements deterministically. Do not infer design intent from a missing parameter.
- Retain units, document identity, element identity, observed values, requirement sources, and rule IDs with findings.
- Distinguish missing, not applicable, excluded, unsupported, and unexamined cases. Preserve quantities for elements that need review.
- Use built-in parameter identifiers or recorded shared-parameter GUIDs where applicable. Keep type and instance data distinct.

## Design and verification

Keep core evaluation independent of live Revit objects where practical. Revit access belongs in valid API execution context; the external server requests named operations rather than directly manipulating application objects. Research dispatch, request scope, timeout behavior, and cancellation before selecting a bridge.

The larger demo targets 12 basic walls and four ceilings, with four findings affecting eight distinct elements across information and physical-condition checks. That combined result remains planned. The implemented [information-check contract](docs/required-type-information.md) has three verified cases: 0 findings/0 affected, 2/6, and 3/8 with an additional ceiling negative control. Keep these scopes distinct. Read the complete brief before extending the fixture or rules.

The [test strategy](tests/README.md) separates offline rule tests from host integration and native schedule checks. From this project directory, run `& .\scripts\Test-Readiness.ps1`, `& .\scripts\Build-Readiness.ps1`, then `& .\scripts\Install-Readiness.ps1` with Revit closed. Follow [host acceptance](tests/required-information.md) in Revit. Add `-IncludeFixtureTools` to build/install only when intentionally preparing local sample copies. The test script runs 18 core tests, seven Excel tests, and a no-redirect host test. The [agent dependency guide](../../docs/how-to/add-a-plugin-dependency.md) documents locked packages, helper-process deployment, source boundaries, and license evidence. Its [participant companion](../../docs/tutorials/04-add-an-excel-report-dependency.md) explains how the author directed the work and assessed the result; preserve these two levels when recording an extension. These tests require no running Revit instance; MCP remains unimplemented. The [readiness verification record](docs/verification/2026-10-20-required-information.md) establishes the tested configuration; do not infer broader support from project settings alone.

Do not copy SDK binaries or working RVT files into Git. Record sample provenance, setup, reset instructions, and independent expected results. Keep the root journal and the related teaching lesson current as design decisions are resolved.
