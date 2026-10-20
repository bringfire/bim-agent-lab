# Revit readiness

The first BIM Agent Lab example helps wall and ceiling professionals review whether a model meets an explicit checklist for a stated coordination or quantity-review use. Inventory and quantities provide supporting evidence.

**Status:** the minimal read-only probe is implemented, locally installable, and host-verified on Revit 2024 build `24.3.60.12`. It reports project identity and wall/ceiling counts. Readiness evaluation, quantities, MCP, and the larger demo fixture remain planned; broader compatibility is unverified.

## The three questions

1. Which wall and ceiling types lack required information, and how many elements does each gap affect?
2. Which modeled top constraints or height offsets disagree with the declared requirements?
3. What are the quantities by type and level, separated into elements passing the checks and those needing review?

The [demo brief](docs/demo-questions-and-sample-model.md) defines the proposed cases and expected counts. Readiness is evaluated against the supplied checklist. It is not construction certification or a code-compliance conclusion.

## Start here

- [Build and run the read-only probe](../../docs/tutorials/02-build-a-read-only-revit-command.md), with [actual verification results](docs/verification/2026-10-20-read-only-probe.md).
- [Development prerequisites](docs/development-prerequisites.md) for the minimum agent-assisted Revit 2024 setup, optional tools, and later MCP dependencies.
- [Project agent guidance](AGENTS.md) for scope and implementation boundaries.
- [Resources and precedents](../../docs/research/revit-resources.md) for SDKs, API evidence, and bridge candidates.
- [Sample preparation](samples/README.md) for controlled model cases.
- [Verification strategy](tests/README.md) for independent answers and host-dependent checks.
- [Build story](../../docs/build-story/journal.md) for the actual prompts and decisions.

## Intended components

| Location | Responsibility |
|---|---|
| [src/readiness-core](src/readiness-core/README.md) | Deterministic rules, findings, and aggregation over normalized inputs |
| [src/revit-addin](src/revit-addin/README.md) | Read Revit data in a valid API context and provide application commands |
| [src/mcp-server](src/mcp-server/README.md) | Expose scoped operations to an agent and return structured evidence |

Only the minimal command in `revit-addin` is implemented. The external server language and whether to adapt an existing bridge are still open decisions.

The next step is sample inspection and one deterministic readiness check with independent expected answers. The probe establishes a working local build and API read; it does not select or validate an MCP bridge.
