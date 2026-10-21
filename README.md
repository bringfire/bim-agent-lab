# BIM Agent Lab

Learn to build and extend BIM tools with AI coding agents—from research and Git to working plugins, automation, and MCP servers.

BIM Agent Lab is a teaching repository for BIM professionals. It pairs worked examples with the prompts, research, decisions, and verification needed to understand and extend them. The first example is a Revit model-readiness tool for wall and ceiling workflows, supported by inventory and quantities.

**Current status:** a read-only Revit 2024 command checks required wall and ceiling type information against an explicit checklist. It passed 18 core tests and independent native-schedule comparisons on three copies of Autodesk's Snowdon sample. Follow the [readiness tutorial](docs/tutorials/03-check-required-type-information.md), or begin with the [first-build tutorial](docs/tutorials/02-build-a-read-only-revit-command.md). The [Excel dependency lesson](docs/tutorials/04-add-an-excel-report-dependency.md) adds four-sheet workbook output with ClosedXML, verified in all three Revit scenarios and inspected in Excel; see the [acceptance evidence](projects/revit-readiness/docs/verification/2026-10-20-excel-output.md). The [MCP research lesson](docs/tutorials/05-research-and-choose-the-mcp-bridge.md) compares bridges and checks current protocol/SDK versions; it recommends a minimal original bridge with an external C# server, pending implementation and acceptance. The MCP host/probe now passes offline tests and its amended compatibility checkpoint in both actual desktop clients; the [compatibility lesson](docs/tutorials/07-test-desktop-compatibility-first.md) explains the evidence. Task 2 now adds tested bridge messages, framing and request lifetimes; the [bridge foundation lesson](docs/tutorials/08-build-and-test-the-bridge-foundation.md) explains this step. Live Revit connection, physical-condition checks, quantities and release packaging remain planned. Git history and the build journal preserve the implementation and research evidence.

## Choose your starting point

The next bridge increment has an [approved specification and external-review record](docs/superpowers/reviews/revit-mcp-bridge/README.md). The reviewer closed all three R1 findings on spec revision 2, and the author approved it. The [nine-task implementation plan](docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md) reached revision 2 after [external R2 findings](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-feedback.md). Its shared lifetime/ownership rules address cancellation, shutdown, partial installation recovery and selection errors; the [R2 follow-up](docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) closes all four findings at the plan level on `cfbfe04`. The [participant story](docs/tutorials/06-direct-design-and-external-review.md) explains that process and why protocol compatibility in both Codex desktop and Claude Desktop is the first execution checkpoint. The author approved plan revision 2 and selected Native execution. The author-directed [revision 3 amendment](docs/superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) now permits verified compatible desktop exchanges while retaining strict July tests. [Fresh CP0 evidence](projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) passes in both desktops. [Task 2 review corrections](projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) now pass offline checks; Task 2 is [independently closed](docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md). Task 3 now has [Windows pipe evidence](projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) and a [participant lesson](docs/tutorials/09-connect-the-bridge-processes.md); its peer-exit P2 correction awaits independent follow-up. Task 4 is next. The live Revit connection and full acceptance remain outstanding.

| I want to… | Start here |
|---|---|
| Learn how to engage an agent | [Participant learning path](docs/tutorials/README.md): research, Git, Revit setup, readiness, and Excel, with actual prompts, decisions, and evidence |
| Use the plugin | [Revit readiness project](projects/revit-readiness/README.md): intended behavior and current availability |
| Learn how we build it | [Learning path](docs/tutorials/README.md), beginning with research and directing an agent with Git |
| Extend it with my agent | [Agent instructions](AGENTS.md), [project guidance](projects/revit-readiness/AGENTS.md), [extension workflow](docs/how-to/extend-a-project.md), and the worked [dependency implementation guide](docs/how-to/add-a-plugin-dependency.md) |
| Review research and resources | [Research index](docs/research/README.md) with pinned repositories, license evidence, and source findings |
| Revisit the seminar | [SWACCA 2026](workshops/swacca-2026/README.md) |

Open your local copy in a coding agent and try:

> Read README.md and AGENTS.md. Explain this project's current state and guide me through the learning path. Start with research and Git. Distinguish what has been observed, what is proposed, and what has been tested. Help me choose one useful next step.

Reading the materials needs no Revit installation. Building and running the Revit example requires the [development prerequisites](projects/revit-readiness/docs/development-prerequisites.md) documented by that project. SDKs, host applications, model files, and the seven reference repositories are obtained separately.

## Repository map

- `docs/` — tutorials, task guides, explanations, technical reference, research, and build history.
- `templates/` — reusable feature, research, verification, and lesson records.
- `projects/revit-readiness/` — the first example's source boundaries, test strategy, sample recipes, and demo brief.
- `scripts/` — guidance for repository utilities as repeatable operations are developed.
- `workshops/swacca-2026/` — seminar context and historical planning materials.
- `references/` — optional local research clones, ignored by Git.

Projects can target other BIM applications as the lab grows. Each owns its dependencies and host-specific instructions. Shared implementation libraries will be introduced when multiple examples demonstrate a need.

## Development and sharing

See [Contributing](CONTRIBUTING.md), [development status](docs/reference/development-status.md), and the [build journal](docs/build-story/journal.md). The [agent setup guide](docs/how-to/build-read-only-revit-command.md) documents environment, build, install, and host verification commands; its [participant story](docs/tutorials/02-build-a-read-only-revit-command.md) explains how the work was directed. Read-only Git orientation commands are:

```powershell
git status --short --branch
git log --oneline -5
git show --stat HEAD
```

The repository starts locally with no remote. Hosting, an original-material license, and release packaging remain to be selected. See [licensing status](docs/reference/licensing.md) and [third-party notices](THIRD_PARTY_NOTICES.md) before distribution.
