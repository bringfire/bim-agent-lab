# Revit readiness

The first BIM Agent Lab example helps wall and ceiling professionals check whether a declared set of model elements meets a written information checklist. Start with the [participant guide](../../docs/start-here.md); an agent reproducing or extending the example should read the [technical handoff](../../docs/how-to/agent-handoff.md) and [project guidance](AGENTS.md).

## Current behavior

The read-only **required-type-information command** reads the active model and its matching checklist, identifies missing type values and reports affected instances, evidence, exclusions and completeness. Its separate local ClosedXML exporter writes a four-sheet workbook. The original minimal probe remains available as a first-build lesson.

The **MCP bridge** exposes two tools: `revit_get_readiness_context` and `revit_check_required_type_information`. They return model/checklist identity and the readiness result to an agent. They do not edit the model or export files. The bridge starts only through the explicit Revit Start command. Optional fixture tools deliberately edit disposable sample copies and are installed separately.

Verified baseline: Revit 2024 build `24.3.60.12`, official C# MCP SDK 2.2.0 in an external .NET 10 process, and a net48 Revit add-in. Actual Codex desktop and Claude Desktop Chat passed the five sample cases and timely model-switch refusal/recovery on October 22, 2026. Their negotiated revisions were `2025-06-18` and `2025-11-25`; separate SDK testing retains `2026-07-28`. See the [live matrix](docs/verification/mcp-readiness.md), [Excel evidence](docs/verification/2026-10-20-excel-output.md) and [compatibility policy](../../docs/superpowers/specs/2026-10-20-desktop-compatibility-amendment.md).

Positive live capture-wrapper runs and [queued application shutdown/recovery](docs/verification/2026-10-22-shutdown-preparation.md) now pass. Remaining host/cancellation/recovery questions, acceptance consolidation and external R3 remain open. Other Revit versions are unverified. This is a working teaching example with local setup scripts; it is not a completed public release.

## The three BIM questions

| Question | Status |
|---|---|
| Which wall and ceiling types lack required information, and how many instances does each gap affect? | Implemented and verified on the declared demonstration cases |
| Which modeled top constraints or height offsets disagree with declared requirements? | Planned |
| What are the quantities by type and level, separated into elements passing checks and those needing review? | Planned |

The [demo brief](docs/demo-questions-and-sample-model.md) describes the larger direction. Readiness is evaluated against the supplied checklist and scope. It is not construction certification or a code-compliance conclusion. Zero findings with incomplete coverage is not a readiness pass.

## Reproduce or learn

- [Participant starting guide](../../docs/start-here.md) and [learning path](../../docs/tutorials/README.md): how the person directed the work and judged evidence.
- [Agent handoff](../../docs/how-to/agent-handoff.md): procedure, source provenance, current checkpoints and outstanding work.
- [Development prerequisites](docs/development-prerequisites.md): build inputs versus runtime and verification requirements.
- [Build/install/recovery](docs/mcp-bridge-setup.md) and [scripts index](scripts/README.md): exact local operations; installation and Deployment/All require Revit closed.
- [Sample recipes](samples/README.md) and [desktop verification](../../docs/how-to/verify-readiness-through-desktop-agents.md): independently prepared complete, defect and incomplete-coverage cases.
- [Tests](tests/README.md), [review hub](../../docs/superpowers/reviews/revit-mcp-bridge/README.md) and [build journal](../../docs/build-story/journal.md): evidence, review scope and actual decisions.

## Implementation boundaries

| Location | Responsibility |
|---|---|
| [src/readiness-core](src/readiness-core/README.md) | Deterministic evaluation over normalized inputs |
| [src/revit-addin](src/revit-addin/README.md) | Revit commands, guarded API reads and event scheduling |
| `src/bridge-contract/`, `src/bridge-runtime/` | Shared messages, framing, Windows pipes, context and lifetime handling |
| [src/mcp-server](src/mcp-server/README.md) | External SDK host and the two production tools |
| [src/excel-report](src/excel-report/README.md) | Static workbook generation in an isolated process |

The bridge reuses the existing lab computation and reader, consumes the official C# SDK, includes an attributed C# SDK stdin adaptation, and adds original connection/adapter code. TypeScript SDK 1.26.0 remains a learning precedent only. Inspect the [MCP dependency record](../../docs/research/mcp-sdk-dependencies.md) and [Excel dependency record](../../docs/research/closedxml-dependency.md) before changing packages. Keep Autodesk binaries, sample RVTs, generated output and private recovery/settings bytes out of Git.
