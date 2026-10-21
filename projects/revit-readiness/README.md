# Revit readiness

The first BIM Agent Lab example helps wall and ceiling professionals review whether a model meets an explicit checklist for a stated coordination or quantity-review use. Inventory and quantities provide supporting evidence.

**Status:** the first required-type-information check is implemented, locally installable, and verified on Revit 2024 build `24.3.60.12`. It reads an explicit checklist, identifies missing required type text, and reports affected instances with evidence and exclusions. Eighteen offline tests and all three sample-based acceptance cases passed. The command also writes a four-sheet Excel workbook through a local ClosedXML exporter; see the [dependency lesson](../../docs/tutorials/04-add-an-excel-report-dependency.md) and [current Excel verification status](docs/verification/2026-10-20-excel-output.md). The original minimal probe remains available. Physical-condition checks, quantities, MCP, and broader compatibility remain unverified or planned.

## The three questions

1. Which wall and ceiling types lack required information, and how many elements does each gap affect?
2. Which modeled top constraints or height offsets disagree with the declared requirements?
3. What are the quantities by type and level, separated into elements passing the checks and those needing review?

The [demo brief](docs/demo-questions-and-sample-model.md) defines the proposed cases and expected counts. Readiness is evaluated against the supplied checklist. It is not construction certification or a code-compliance conclusion.

## Start here

For the current MCP increment, read [spec revision 2](../../docs/superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) and the [review hub](../../docs/superpowers/reviews/revit-mcp-bridge/README.md). The author selected the custom C# bridge and requested spec, plan, and implementation reviews. R1 findings are externally closed and revision 2 is author-approved at `0ef8cda`. Both Codex desktop and Claude Desktop must pass the stated protocol/host acceptance. The [implementation plan](../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md) is ready for external R2 and author review/execution choice. It begins with desktop protocol compatibility, then builds the connection and verifies live results. No bridge code exists yet.

- Read the [readiness participant story](../../docs/tutorials/03-check-required-type-information.md), use the [agent build/verification guide](../../docs/how-to/build-required-information-check.md), and inspect [actual results and evidence](docs/verification/2026-10-20-required-information.md).
- Read the [setup participant story](../../docs/tutorials/02-build-a-read-only-revit-command.md), use the [agent setup guide](../../docs/how-to/build-read-only-revit-command.md), and inspect [actual verification results](docs/verification/2026-10-20-read-only-probe.md).
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
| [src/excel-report](src/excel-report/README.md) | Generate a static Excel workbook from saved report data in a separate local process |
| [src/mcp-server](src/mcp-server/README.md) | Expose scoped operations to an agent and return structured evidence |

The core evaluator and Revit command/reader are implemented. The [MCP comparison](../../docs/research/revit-mcp-bridge-comparison.md) recommends a narrow original bridge plus an external C# server using the current official SDK. This is the proposed first experiment; the server is not implemented or host-tested.

The first deterministic check now has independent expected answers, native schedules, and three sample-derived copies. Next, complete the requested [spec and plan review stages](../../docs/superpowers/reviews/revit-mcp-bridge/README.md), then implement and verify the proposed connection using the approved plan and [bridge integration gates](../../docs/how-to/choose-and-verify-an-mcp-bridge.md). Check current versions before adding dependencies. The [larger demo brief](docs/demo-questions-and-sample-model.md) still requires physical-condition and quantity checks.
