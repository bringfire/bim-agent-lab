# Why the repository is organized this way

BIM Agent Lab serves three needs: use a worked example, learn how it was built, and extend it with an agent. The seminar provides orientation; the repository carries the full learning path afterward.

The author selected `bim-agent-lab` so the resource can grow beyond Revit. Shared documentation teaches research, Git, task definition, evidence, and collaboration. Each folder under `projects/` owns its application-specific dependencies, source, tests, and sample preparation. Revit readiness is the first project.

Keep documentation and code in the same repository so a real commit identifies the matching implementation and explanation. A later tagged seminar release should pair installable artifacts with the demonstrated source and instructions. Future projects can establish their own version and release needs as they appear.

The documentation distinguishes learning sequences, task guides, explanations, and reference, informed by [Diataxis](https://diataxis.fr/). The build journal preserves actual decisions and corrections; tutorials turn those experiences into approachable lessons with clear starting points and verification.

Root AGENTS.md supplies shared guidance. Project AGENTS.md files add local context. Agent-specific entry files point to this common guidance. Historical prompts remain evidence, with suggested or edited teaching prompts explicitly labeled.

No shared multi-platform runtime is needed at this stage. Extract common implementation only after real projects demonstrate a stable need.
