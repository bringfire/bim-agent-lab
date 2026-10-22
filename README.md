# BIM Agent Lab

**R3 feedback returned:** [review and dispositions](docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md) keep acceptance open. The recorded report computations remain supported; Codex presentation evidence, baseline and current-source live verification still need completion. The current task records the review and corrections without deploying or changing application settings.

**Current review checkpoint:** the [R3 packet](docs/superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) and [A1–A9 ledger](projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) identify passed evidence and open items. Independent follow-up and final acceptance remain pending; this is not a finished public release.

Learn to build and extend BIM tools with AI coding agents—from research and Git to working plugins and MCP servers.

BIM Agent Lab is a teaching repository for BIM professionals. It pairs working examples with the prompts, research, decisions and verification needed to understand and extend them. Revit readiness is the first example; the lab can grow to other BIM applications.

## Start here

**Participants:** use the [starting guide](docs/start-here.md). It explains what you can try, how to direct your agent, and how to judge the answer. Reading the lessons requires no Revit installation.

**Agents:** use the [technical handoff](docs/how-to/agent-handoff.md), root [AGENTS.md](AGENTS.md) and project guidance before changing anything. The handoff connects prerequisites, source boundaries, build/install/recovery, samples, verification and remaining work.

Suggested first prompt:

> Read README.md, AGENTS.md, docs/start-here.md and docs/how-to/agent-handoff.md. Explain what works and what remains unverified. Inspect my Git state and prerequisites, then guide me through one useful next step. Distinguish research, source inspection, offline tests and actual Revit results.

## What works now

As of October 22, 2026, the Revit 2024 example checks required wall and ceiling type information against an explicit checklist. The manual command produces saved evidence and a four-sheet Excel workbook. The MCP bridge lets **Codex desktop and Claude Desktop Chat** request a fresh model context and the same read-only calculation; those MCP calls do not edit or export.

All five prepared sample cases—including deliberate defects and incomplete coverage—matched independent expectations through both desktops and a separate SDK client. Both desktops rejected an unexpired context after a model switch and recovered using a fresh context. See the [live evidence matrix](projects/revit-readiness/docs/verification/mcp-readiness.md) and [Excel evidence](projects/revit-readiness/docs/verification/2026-10-20-excel-output.md).

The bridge uses official C# MCP SDK **2.2.0**, with verified compatible desktop revisions and a retained `2026-07-28` regression. This is a dated tested baseline, not a latest-version claim. Positive live capture-wrapper runs and queued application-shutdown/recovery now pass. Remaining host/cancellation/recovery questions, acceptance consolidation and independent R3 review remain open. Physical-condition checks and quantities are future extensions. See [development status](docs/reference/development-status.md).

## Find what you need

| I want to… | Start here |
|---|---|
| Try the example with my agent | [Participant starting guide](docs/start-here.md) |
| Refresh how we engaged the agent | [Learning path](docs/tutorials/README.md), beginning with research and Git |
| Reproduce or extend the implementation | [Agent handoff](docs/how-to/agent-handoff.md) and [extension workflow](docs/how-to/extend-a-project.md) |
| Inspect the Revit project | [Project README](projects/revit-readiness/README.md), [prerequisites](projects/revit-readiness/docs/development-prerequisites.md) and [samples](projects/revit-readiness/samples/README.md) |
| Review research and source provenance | [Research index](docs/research/README.md) and [third-party notices](THIRD_PARTY_NOTICES.md) |
| Understand independent review | [Participant review story](docs/tutorials/06-direct-design-and-external-review.md) and [review records](docs/superpowers/reviews/revit-mcp-bridge/README.md) |
| Revisit the seminar | [SWACCA 2026](workshops/swacca-2026/README.md) |

## Repository map

- `docs/` — participant stories, agent procedures, explanations, reference, research and build history.
- `templates/` — reusable feature, research, verification and lesson records.
- `projects/revit-readiness/` — application source, scripts, tests, sample recipes and evidence.
- `workshops/swacca-2026/` — seminar context and planning materials.
- `references/` — optional local research clones, ignored by Git.

Each application project owns its dependencies and host-specific instructions. SDKs, Autodesk API libraries, working RVT models and reference repositories are acquired separately. The sample recipe is included; Autodesk model binaries are not.

## Development and sharing

See [Contributing](CONTRIBUTING.md) and the [build journal](docs/build-story/journal.md). Inspect the current branch, diff and history before work; record coherent verified steps in Git. The approved [specification and plan](docs/superpowers/reviews/revit-mcp-bridge/README.md) guide the continuing bridge work; historical review closure is scoped to the reviewed revision and evidence.

This checkpoint is local: public hosting, an original-material license and release packaging remain to be selected. See [licensing status](docs/reference/licensing.md) before distribution. The participant materials are being prepared for sharing; this is not a claim that a published release or bundled installer is available.
