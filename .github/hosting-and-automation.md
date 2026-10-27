# Repository hosting and automation

The repository is public at [bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab). No hosted automation workflow is configured yet. When adding CI, use the documented build/test commands and preserve the distinction between offline and live application checks.

Documentation and offline rule tests can be checked separately from Revit integration. Do not present a hosted CI result as a Revit runtime test unless the job actually has the required application environment and executes that check.

Keep these internal notes under a descriptive filename. GitHub prioritizes a README in `.github/` over the root [participant README](../README.md); do not add a competing README here. See the [publication procedure](../docs/how-to/review-and-publish-a-repository.md).
