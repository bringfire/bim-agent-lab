# Contributing to BIM Agent Lab

Start with [AGENTS.md](AGENTS.md), the [current status](docs/reference/development-status.md), and the relevant project README. Contributions should improve either a working example or someone's ability to understand and verify it.

## A useful change

Describe the BIM problem, intended audience, scope, and expected result using the [feature brief](templates/feature-brief.md). Research existing approaches before adding dependencies. Put project-specific behavior in that project's directory and transferable lessons in `docs/`.

Use a branch for a new feature after checking for existing work. Make coherent commits with messages that explain the change. Review `git diff` and `git diff --cached`; record checks and limitations. A successful source inspection is not a successful build or Revit test.

For documentation-only changes, check relative links, command accuracy, and consistency with current status. For implementation changes, use the relevant project test guidance; the [Revit test strategy](projects/revit-readiness/tests/README.md) distinguishes offline suites from licensed-host checks. Add meaningful tests where the change requires them and record exact commands beside the affected project.

## Teaching changes

Pair an extension with its purpose, prompt, explanation, verification, and a small exercise. Preserve actual prompts and corrections in the build journal; identify edited teaching examples as such. Link lessons to real commits after those milestones exist. Do not reconstruct fictional development history.

Use [verification records](templates/verification-record.md) to distinguish tests that need a licensed host application from checks that run without it. Prefer small, independently checkable sample cases.

## Distribution

Original code and teaching material use the [MIT License](LICENSE). The [GitHub repository](https://github.com/bringfire/bim-agent-lab) is private as of October 26, 2026; public-distribution review, public access and release packaging remain pending. See [sharing status](docs/reference/repository-sharing.md). Preserve required third-party notices and record the provenance of adapted files. Keep model files and Autodesk SDK/API binaries outside Git unless their distribution has been deliberately addressed.
