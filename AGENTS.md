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
3. Research before choosing dependencies or copying code. Inspect licenses, manifests, actual source, and required SDKs. Record exact revisions and unresolved assumptions.
4. Implement focused changes. Keep each example's dependencies local to that project; avoid premature shared frameworks.
5. Verify against independent evidence. Distinguish source observations, successful builds, and application runtime tests.
6. Update the related lesson and journal when a change teaches something material. Label actual user prompts, excerpts, and suggested teaching prompts accurately.
7. Review the diff and relevant checks before committing a coherent checkpoint. Follow the user's instructions about branches and publishing; do not assume a remote exists.

## Current commands and boundaries

Git orientation: `git status --short --branch`, `git log --oneline -5`, `git diff`, and `git diff --cached`.

From the repository root, Revit scripts live in `projects/revit-readiness/scripts/`. Use `Check-Environment.ps1`, `Test-Readiness.ps1`, `Build-Readiness.ps1`, and `Install-Readiness.ps1` for the readiness check. `Build-Probe.ps1` and `Install-Probe.ps1` retain the original minimal example. Installation requires Revit closed. Follow the [readiness tutorial](docs/tutorials/03-check-required-type-information.md) and [host acceptance procedure](projects/revit-readiness/tests/required-information.md). The automated suite includes 18 core tests, seven Excel report tests, and a process-isolation test. Follow the [Excel dependency lesson](docs/tutorials/04-add-an-excel-report-dependency.md) and its current verification record. CI and MCP tests do not exist yet. Distinguish environment checks, compilation, and observed Revit runtime results; never infer a host pass from a build alone. Fixture editing tools are opt-in and separate from normal readiness installation.

Reference acquisition is described in [the clone guide](docs/how-to/clone-research-repositories.md). Keep local clones under ignored `references/`; inspect upstream instructions within each clone. ClosedXML is the first adopted NuGet dependency, isolated in the local Excel exporter; the original seven reference repositories remain research precedents. Review the [resolved dependencies and licenses](docs/research/closedxml-dependency.md) before changing packages. Commit lock files and use locked restores. Existing optional fixture installations share our core and must stay synchronized.

Keep credentials, local agent configuration, generated outputs, and working model files out of Git. Preserve source notices when adapting material. The license for original lab material remains an owner decision before release.

When someone asks to learn, explain the reason for the next step and what evidence to examine. When they ask to implement, carry out the scoped work and record the lesson without requiring them to memorize command syntax.
