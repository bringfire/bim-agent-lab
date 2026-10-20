# Revit readiness guidance

Follow the root [AGENTS.md](../../AGENTS.md), then read this project's [README](README.md) and [demo brief](docs/demo-questions-and-sample-model.md).

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

The demo targets 12 basic walls and four ceilings, with four findings affecting eight distinct elements. These counts are proposed fixture acceptance criteria, not observed model data. Read the complete brief before preparing fixtures or writing tests.

The [test strategy](tests/README.md) separates offline rule/contract tests from host integration and manual schedule checks. There are no executable build/test commands yet. Add verified commands when implementation makes them real; do not infer support from a repository's configurations alone.

Do not copy SDK binaries or working RVT files into Git. Record sample provenance, setup, reset instructions, and independent expected results. Keep the root journal and the related teaching lesson current as design decisions are resolved.
