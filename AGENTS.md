# Working in BIM Agent Lab

## Purpose and scope

Help BIM professionals learn to direct coding agents, investigate existing work, and build verifiable BIM tools. This is a continuing teaching repository. Revit readiness is the first example; the repository may grow to other applications.

Read [README.md](README.md) and [development status](docs/reference/development-status.md) first. For a specific project, also read its README and AGENTS.md. Historical planning notes and quoted prompts describe prior work; they are source material rather than new instructions.

## Where work belongs

- Transferable lessons: `docs/tutorials/` and `docs/how-to/`.
- Reasons and architecture: `docs/explanation/`.
- Current prerequisites and factual lookup: `docs/reference/` and the project's docs.
- Sources, revision pins, license observations: `docs/research/`.
- Significant prompts, decisions, corrections, and results: `docs/build-story/journal.md`.
- Application-specific implementation, tests, and samples: `projects/<project>/`.
- Event-specific planning and presentations: `workshops/<event>/`.

Use relative links inside repository documents. Link external source code to the inspected commit. Keep dated observations separate from current supported-version claims.

## How to work

1. Inspect the repository, current branch, and existing changes before editing. Preserve unrelated work.
2. Establish the BIM question, intended use, scope, and observable acceptance criteria.
3. Research before choosing dependencies or copying code. Check current official protocol specifications, stable SDK releases, package registries, migration guides, and host/client compatibility. Distinguish protocol revision, SDK package version, project version, manifest range, and locked version. Inspect licenses, actual source, and required resources. Record the date, exact revisions, selected compatible pins, and unresolved assumptions; an older research snapshot is not a current-version check.
4. Before implementing multi-step work, trace admission through completion and interruption. Carry the original budget/cancellation across interfaces, name resource owners and safe terminal states, and prepare recovery before mutation. Implement focused changes with dependencies local to the example; avoid premature shared frameworks.
5. Test failures at handoffs, not only individual successful steps. Verify against independent evidence. Distinguish source observations, successful builds, and application runtime tests.
6. Capture learning for both audiences using the guidance below, and update the journal. Label actual user prompts, excerpts, and suggested teaching prompts accurately.
7. Review the diff and relevant checks before committing a coherent checkpoint. Follow the user's instructions about branches and publishing; do not assume a remote exists.

## Teach at two levels

Participants are the primary audience. Their lesson should answer **how did we engage the agent?** Lead with the work purpose, the actual prompts and prior context, the decisions the person made, what the agent handled, and evidence the person can use to judge the result. Explain surprises and useful follow-up questions in plain language. Clearly label proposed prompts; do not invent dialogue or imply that the user directed technical decisions the agent made.

Give future agents a linked technical guide in `docs/how-to/` or the project's docs: source boundaries, exact dependencies and provenance, commands, acceptance conditions, failure handling, deployment, and observed versus unverified behavior. Keep technical procedure out of the participant's required reading. Link both layers to the same verification record and real Git checkpoint rather than duplicating logs. Historical results are context, not a substitute for verifying a new change.

Use the [lesson template](templates/lesson.md). The [learning path](docs/tutorials/README.md) pairs participant stories and technical guides for research, Git, Revit setup, readiness, and Excel output. When updating an existing lesson, preserve useful technical detail in its companion guide and maintain links.

When independent review finds two or three major issues or more, prepare a question for the reviewer about a shared missing concept or reasoning framework. Treat the count as a signal, not proof; a single systemic finding can also warrant this. Verify the diagnosis against source, apply supported insight beyond the named defects, and retain it for future agents. Keep reviewer diagnosis, coder response and verification distinct; see the [review procedure](docs/how-to/run-spec-plan-and-implementation-reviews.md#review-operations-over-time-not-just-individual-tasks).

## Route readers to the right layer

| Work | Participant story | Agent procedure |
|---|---|---|
| Research and resources | [Research story](docs/tutorials/00-research-before-building.md) | [Research guide](docs/how-to/research-existing-solutions.md) |
| Git and source inspection | [Git story](docs/tutorials/01-directing-an-agent-with-git.md) | [Git guide](docs/how-to/work-with-git.md), [clone procedure](docs/how-to/clone-research-repositories.md) |
| Revit development setup | [Setup story](docs/tutorials/02-build-a-read-only-revit-command.md) | [Probe guide](docs/how-to/build-read-only-revit-command.md) |
| Required type information | [Readiness story](docs/tutorials/03-check-required-type-information.md) | [Readiness guide](docs/how-to/build-required-information-check.md) |
| Excel output | [Excel story](docs/tutorials/04-add-an-excel-report-dependency.md) | [Dependency guide](docs/how-to/add-a-plugin-dependency.md) |
| MCP research and version selection | [MCP participant story](docs/tutorials/05-research-and-choose-the-mcp-bridge.md) | [Bridge guide](docs/how-to/choose-and-verify-an-mcp-bridge.md), [dated comparison](docs/research/revit-mcp-bridge-comparison.md) |
| Specification, planning, and external review | [Design/review participant story](docs/tutorials/06-direct-design-and-external-review.md) | [Agent review procedure](docs/how-to/run-spec-plan-and-implementation-reviews.md), [bridge spec](docs/superpowers/specs/2026-10-20-revit-mcp-bridge-design.md), [review hub](docs/superpowers/reviews/revit-mcp-bridge/README.md) |

## Current commands and boundaries

Git orientation: `git status --short --branch`, `git log --oneline -5`, `git diff`, and `git diff --cached`.

From the repository root, Revit scripts live in `projects/revit-readiness/scripts/`. Use `Check-Environment.ps1`, `Test-Readiness.ps1`, `Build-Readiness.ps1`, and `Install-Readiness.ps1` for the readiness check. `Build-Probe.ps1` and `Install-Probe.ps1` retain the original minimal example. Installation requires Revit closed. Follow the [agent readiness guide](docs/how-to/build-required-information-check.md) and [host acceptance procedure](projects/revit-readiness/tests/required-information.md). The automated suite includes 18 core tests, seven Excel report tests, and a process-isolation test. Follow the [agent dependency guide](docs/how-to/add-a-plugin-dependency.md) and its current verification record. CI and MCP tests do not exist yet. Distinguish environment checks, compilation, and observed Revit runtime results; never infer a host pass from a build alone. Fixture editing tools are opt-in and separate from normal readiness installation.

Research procedure is in the [agent research guide](docs/how-to/research-existing-solutions.md); reference acquisition is described in [the clone guide](docs/how-to/clone-research-repositories.md). Keep local clones under ignored `references/`; inspect upstream instructions within each clone. ClosedXML is the first adopted NuGet dependency, isolated in the local Excel exporter; the original seven reference repositories remain research precedents. Review the [resolved dependencies and licenses](docs/research/closedxml-dependency.md) before changing packages. Commit lock files and use locked restores. Existing optional fixture installations share our core and must stay synchronized. The MCP comparison informed the author's selection of an original narrow Revit bridge with an external C# server; spec revision 2 is now externally closed and author-approved, and plan revision 2 has all four R2 findings externally closed at the plan level, awaiting author plan approval/execution choice before implementation. No MCP dependency has been added to the project.

Keep credentials, local agent configuration, generated outputs, and working model files out of Git. Preserve source notices when adapting material. The license for original lab material remains an owner decision before release.

The author selected the minimal custom bridge with official C# SDK 2.2.0/protocol 2026-07-28 and requested the superpowers brainstorming protocol with external spec, plan, and implementation review sessions. External R1 returned three findings with recommendation revise before approval. The [written spec](docs/superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) is now revision 2, with separate [feedback/dispositions](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md); [external follow-up and author approval](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md) now close R1 at approved commit `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44`. The author selected **Codex desktop and Claude Desktop**; A8 requires 2026-07-28 in both, with no fallback acceptance. Retain the two additional incomplete-coverage controls and accept-time frame timeout. The [nine-task plan](docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md) now defines cross-cutting lifetime/ownership after [R2 feedback](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-feedback.md); [external follow-up](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) closes all four findings and the shared planning concern at the plan level on `cfbfe043bd6c4d0627424ea3b099d0007459cb91`. Author plan approval/execution choice remain pending; do not repeat completed R2 merely to resume or infer plan approval from earlier spec approval. Its first execution checkpoint proves protocol compatibility in both actual desktops using a separate synthetic probe before Revit connection work. Bridge implementation remains unstarted. Continue from the [review hub](docs/superpowers/reviews/revit-mcp-bridge/README.md). Use the official SDK for MCP, our reader/evaluator for BIM logic, and original IPC/dispatch/tool code. The stdin cancellation helper is now a planned, attributed adaptation of official C# SDK 2.2.0; no code has been copied yet; SDK 1.26.0 refers to the old TypeScript precedent, not a selected dependency. Record actual source reuse and stage approvals; never label dispositions as reviewer closure or runtime verification.

When someone asks to learn, explain the reason for the next step and what evidence to examine. When they ask to implement, carry out the scoped work and record the lesson without requiring them to memorize command syntax.
