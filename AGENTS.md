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

From the repository root, the Revit probe uses `& .\projects\revit-readiness\scripts\Check-Environment.ps1`, `Build-Probe.ps1`, and `Install-Probe.ps1` in that same script directory. Installation requires Revit to be closed. Follow the [first-build tutorial](docs/tutorials/02-build-a-read-only-revit-command.md) and [host acceptance procedure](projects/revit-readiness/tests/read-only-probe.md). There is no automated plugin test suite or CI workflow yet. Distinguish environment checks, compilation, and observed Revit runtime results; never infer a host pass from a build alone.

Reference acquisition is described in [the clone guide](docs/how-to/clone-research-repositories.md). Keep local clones under ignored `references/`; inspect upstream instructions within each clone. No reference code has been adopted as a dependency yet.

Keep credentials, local agent configuration, generated outputs, and working model files out of Git. Preserve source notices when adapting material. The license for original lab material remains an owner decision before release.

When someone asks to learn, explain the reason for the next step and what evidence to examine. When they ask to implement, carry out the scoped work and record the lesson without requiring them to memorize command syntax.
