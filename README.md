# BIM Agent Lab

Learn to build and extend BIM tools with AI coding agents—from research and Git to working plugins, automation, and MCP servers.

BIM Agent Lab is a teaching repository for BIM professionals. It pairs worked examples with the prompts, research, decisions, and verification needed to understand and extend them. The first example is a Revit model-readiness tool for wall and ceiling workflows, supported by inventory and quantities.

**Current status:** the first read-only Revit 2024 command has been built, installed locally, and verified against an empty project and a controlled four-wall model. Follow the [first-build tutorial](docs/tutorials/02-build-a-read-only-revit-command.md). Readiness rules, the larger demo fixture, MCP, and release packaging remain to be implemented. Git history records the build from the teaching baseline onward; earlier research is preserved in the build journal.

## Choose your starting point

| I want to… | Start here |
|---|---|
| Use the plugin | [Revit readiness project](projects/revit-readiness/README.md): intended behavior and current availability |
| Learn how we build it | [Learning path](docs/tutorials/README.md), beginning with directing an agent with Git |
| Extend it with my agent | [Agent instructions](AGENTS.md), [project guidance](projects/revit-readiness/AGENTS.md), and [extension workflow](docs/how-to/extend-a-project.md) |
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

See [Contributing](CONTRIBUTING.md), [development status](docs/reference/development-status.md), and the [build journal](docs/build-story/journal.md). The [first-build tutorial](docs/tutorials/02-build-a-read-only-revit-command.md) documents environment, build, install, and host verification commands. Read-only Git orientation commands are:

```powershell
git status --short --branch
git log --oneline -5
git show --stat HEAD
```

The repository starts locally with no remote. Hosting, an original-material license, and release packaging remain to be selected. See [licensing status](docs/reference/licensing.md) and [third-party notices](THIRD_PARTY_NOTICES.md) before distribution.
