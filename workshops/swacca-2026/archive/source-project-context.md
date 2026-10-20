> Historical authoring context imported on 2026-10-20. Active repository instructions are in [AGENTS.md](../../../AGENTS.md).

# SWACCA BIM Workshop — AI-Assisted BIM Tool Development

## What this project is

Materials for a workshop teaching BIM professionals how to use AI tools to build
Revit plugins and other programmatic BIM tools. Content draws on lessons learned
building **Rook** (the author's AI ↔ Rhino/Grasshopper/Revit bridge), reframed for a
BIM/Revit audience.

This is a **content project**, not a code project (yet). Deliverables are documents,
an agenda, and a takeaway package. Example code may be added later as part of the
takeaway kit.

## Constraints that shape every decision

- **Time:** 2 hours total, delivered as two 1-hour sessions. Far too short to cover
  the subject comprehensively — so live sessions must prioritize *orientation and
  momentum* over completeness.
- **Takeaway package is the real product.** The sessions are the on-ramp; the package
  is what attendees bring back to their offices for self-study and ongoing reference.
  When in doubt, push depth into the package and keep the sessions focused.
- **The shared teaching repository must work after the seminar.** Participants
  should be able to point their agents at it to learn the process, refresh their
  knowledge, extend the supplied plugin, and build their own tools. The author
  selected **BIM Agent Lab** (`bim-agent-lab`) as its name so it can grow beyond
  Revit. The Revit readiness plugin is the first worked example. The fresh
  repository's structure is being designed; it has not yet been created.
- **Audience:** BIM/VDC staff at **wall & ceiling specialty contractors** (SWACCA is
  the national alliance of union-signatory wall & ceiling contractors). They are
  builders, not designers, and work on framing, drywall, ceilings, prefab,
  coordination and takeoffs. They are at least partially development-oriented
  (Dynamo/pyRevit/C# Revit API exposure varies). Assume BIM domain fluency; do not
  assume software-engineering fluency.
- **Event:** SWACCA BIM Summit 2026, "Align, Automate, Advance," Oct 29–30, 2026, at
  the Wall and Ceiling Bureau in Pleasanton, CA. Our sessions are its first two 1-hour
  education sessions on Thursday 10/29 (8:30 and 11:00), before teams start building. The summit is built around a team competition, the
  Summit Challenge, that runs alongside the sessions. Look for ways to tie into it.
  Full agenda is in `docs/info-gathering.md`.

## Repo layout

- `CLAUDE.md` — this file; project context and conventions.
- `docs/info-gathering.md` — running capture of raw input from the author. Source of
  truth for everything that becomes the outline and agenda.
- `docs/domain-research.md` — wall & ceiling contractor domain research (firms,
  workflow, pain points, tool landscape, plugin candidates), with sources.
- `docs/demo-questions-and-sample-model.md` — three proposed readiness demonstrations,
  controlled sample conditions, and expected results.
- `docs/research-and-resources.md` — checked SDK/API/runtime resources, repository
  precedents, licenses, dependencies, and outstanding compatibility checks.
- `docs/build-journal.md` — actual prompts, decisions, corrections, evidence, and
  teaching lessons captured throughout the work.
- `docs/directing-an-agent-with-git.md` — early workshop lesson and participant
  prompts for cloning, source inspection, branches, diffs, and checkpoints.
- `docs/reference-code-map.md` — source entry points in the local research clones.
- `references/` — seven independent, pinned Git checkouts for source inspection;
  ignored by the workshop's `.gitignore`. Treat these as clean reference copies.
- `docs/research/repository-evidence-2026-10-20.json` — pinned license and dependency
  manifest snapshots; research evidence, not adopted dependencies.
- `docs/organizer-brief.html` — draft proposal for the organizer meeting (Rev A,
  2026-10-15), published as an Artifact: https://claude.ai/artifact/Db9sRcjxYLna1iAhqkvVMK.
  Keeps all three agenda options (A/B/C) open; we decide after organizer input.
- (planned) `docs/outline.md` — workshop outline derived from info-gathering.
- (planned) `docs/agenda.md` — timed agenda for the two sessions.
- Outline and presentation may be developed **in HTML first, then turned into
  slides**. The author approved this approach.
- `.superpowers/` — brainstorming visual-companion mockups (scratch; not deliverables).
- (planned) `takeaway/` — the take-home package.

## Working conventions

- **Capture first, organize later.** When the author gives information, add it to
  `docs/info-gathering.md` under the best-fitting section, preserving their wording
  and intent. Don't prematurely turn notes into outline prose.
- Record open questions in the Open Questions section rather than guessing.
- **Confirmed direction (2026-10-20):** lead with model readiness; inventory and
  quantities support it. Shipped Revit samples may be adapted for demonstrations.
- **Research comes before implementation.** The author wants attendees to learn how
  to find existing work and collect SDKs, APIs, libraries, dependencies, and suitable
  licensed precedents or starting points. Inspect sources and record versions.
- **Teach Git from the start.** Participants should know to ask an agent to clone
  serious research candidates and inspect their actual source. Record origin and
  commit, distinguish reference checkouts from our own project, and teach branches,
  diffs, local checkpoints, and remote publishing as deliberate instructions.
- **Keep the build story as we work.** Add significant prompts, decisions, discoveries,
  corrections, and verification evidence to `docs/build-journal.md`. Distinguish
  the author's decisions, agent proposals, documented claims, and tested facts.
- Tag each captured item with where it should land when known:
  `[session-1]`, `[session-2]`, `[takeaway]`, or `[cut?]`.
- Keep Rook-specific details only where they illustrate a transferable lesson;
  the audience builds for Revit, not Rhino. RookBIM runs through Rhino.Inside.Revit,
  so it is a precedent only. The workshop plugin is a standalone Revit add-in
  (+ MCP server) that must run on Revit 2024 and reach back as far as practical
  (assume firms are ~3 years behind; proposed range 2022–2027).
- Prefer concrete, BIM-flavored examples (Revit API, Dynamo, pyRevit, IFC,
  schedules, parameters) over generic software examples.
