# Revit readiness

The first BIM Agent Lab example helps wall and ceiling professionals review whether a model meets an explicit checklist for a stated coordination or quantity-review use. Inventory and quantities provide supporting evidence.

**Status:** research and demo design. There is no installable plugin, implemented MCP server, or verified sample model yet. Revit 2024 is the required first target; broader compatibility is unverified.

## The three questions

1. Which wall and ceiling types lack required information, and how many elements does each gap affect?
2. Which modeled top constraints or height offsets disagree with the declared requirements?
3. What are the quantities by type and level, separated into elements passing the checks and those needing review?

The [demo brief](docs/demo-questions-and-sample-model.md) defines the proposed cases and expected counts. Readiness is evaluated against the supplied checklist. It is not construction certification or a code-compliance conclusion.

## Start here

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

These folders document responsibilities, not a completed implementation. The external server language and whether to adapt an existing bridge are still open decisions.

The next step is a minimal Revit 2024 build-and-read investigation plus sample inspection. That evidence will establish the exact SDK/package versions, commands, and architecture used by the first working example.
