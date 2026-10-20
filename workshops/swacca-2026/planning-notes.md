> These planning notes preserve raw input and earlier proposals. Paths in historical notes refer to the original folder. The [workshop overview](README.md) and [BIM Agent Lab README](../../README.md) describe the current organization.

# Workshop Info Gathering

Running capture of input for the workshop. This feeds the outline (`outline.md`) and
agenda (`agenda.md`). Raw is fine here — organize later.

**Landing tags:** `[session-1]` `[session-2]` `[takeaway]` `[cut?]`

---

## 1. Logistics

- **Format:** 2 hours, split into two 1-hour sessions.
- **Event:** **SWACCA BIM Summit 2026: "Align, Automate, Advance"** (3rd annual).
  [Eventbrite page](https://www.eventbrite.com/e/swacca-bim-summit-2026-align-automate-advance-tickets-1989417992770).
  In person, from $267.54. The page says ~34 attendees, but that may be the organizer's
  lifetime total.
- **Where:** Wall and Ceiling Bureau, 5690 Sonoma Drive, Pleasanton, CA 94566 (the
  "WACA BIM Cube").
- **Attendee mix (per event page):** wall & ceiling contractors, BIM/VDC
  professionals, field staff, union representatives, technology innovators. FAQ:
  "No BIM expertise required — every role brings something essential."
- **Our slots:** both Thursday education sessions before the Challenge build.
  - **Session 1 = Ed Session 1, Thu 10/29 8:30–9:30**, right after the keynote, before
    Challenge kickoff.
  - **Session 2 = Ed Session 2, Thu 10/29 11:00–12:00**, after team formation, before
    lunch and the first build time.
  - _Per the published agenda, Challenge kickoff (9:45–11:00) falls between our
    sessions. Confirm whether that's still the order._
  - The third education session (Fri 8:50) belongs to someone else. "AI, automation,
    and BIM adoption" is the summit's stated theme for education sessions.

### Summit agenda (from Eventbrite, 2026-10-15)

| When | What |
|---|---|
| Wed 10/28, 5:00 PM | Welcome happy hour & networking |
| Thu 10/29, 8:00–8:20 | Opening keynote & breakfast (live vote picks the Challenge topic) |
| Thu 8:30–9:30 | **Education Session 1** — TBA |
| Thu 9:45–11:00 | Summit Challenge kickoff: team formation & brief |
| Thu 11:00–12:00 | **Education Session 2** — TBA |
| Thu 12:00–1:00 | Lunch & team working time |
| Thu 1:00–2:30 | Tech demo showcase: vendor deep-dives (mandatory all-hands) |
| Thu 2:30–3:00 | Challenge build block & Day 1 wrap-up (prize announced) |
| Thu 3:00–4:00 | Break (team build time continues) |
| Thu 4:00–6:00 | Happy hour, dinner, networking |
| Fri 10/30, 8:00–8:20 | Welcome & lightning talk |
| Fri 8:20–8:50 | Final Challenge build time & presentation prep |
| Fri 8:50–9:50 | **Education Session 3** — TBA |
| Fri 10:00–11:30 | Challenge presentations & judging (judges + audience vote) |
| Fri 11:30–12:00 | Winner, feedback survey, 2027 preview, close |

### The Summit Challenge

Teams of mixed attendees solve "a real wall and ceiling industry challenge," build
solutions through Day 1, and present on Day 2. The topic is picked by live vote at
the keynote. Winners are chosen by judges plus audience vote. Prizes are awarded.

- **Opportunity:** If our sessions are Ed 1 and Ed 2 (Thursday), attendees could apply
  what we teach to the Challenge the same day. Session 1 runs *before* the kickoff, and
  Session 2 runs right *after* teams form. Consider designing Session 2 so teams can use
  AI tools on their Challenge problem. _Talk to the organizers?_
- **Host:** [SWACCA](https://swacca.org/), "the national alliance of
  union-signatory wall and ceiling contractors." Members join through regional
  affiliate chapters (e.g. WACA) or directly.
  - Runs an **Innovation & Technology Committee** with three pillars: BIM, Workforce
    Health, and Big Data.
  - Publishes a quarterly Innovation Blog, including "Tech Tool Spotlight" posts
    (Sitelink, Trayd) and "Closing the Gap: Why BIM Coordination Still Struggles to
    Leave the Office" (Aug 10, 2026). That post may be worth reading for audience pain
    points.
  - The summit page says it requires "no advanced BIM experience." Our session is
    dev-oriented, so check how that framing fits.
- **Expected headcount:** _TBD_
- **Attendee setup (laptops? Revit installed? AI tool accounts?):** _TBD_

## 2. Audience

- BIM professionals, at least partially oriented to BIM/Revit development.
- **Attending companies (as of 2026-10-15):**
  - [Anning Johnson](https://anningjohnson.com/)
  - [Double Engineering](https://doublelengineering.com/) — note: URL reads "Double L
    Engineering"; confirm correct name
  - [Valley Interior Systems](https://buildwithvalley.com/)
  - [TJ Wies](https://www.tjwies.com/)
  - [Nevell Group](https://www.nevellgroup.com/)
  - [Raymond Group](https://www.raymondgroup.com/)
- **Domain gap (author, 2026-10-15):** the author's background is specialized
  architecture (Gehry Partners, Safdie Architects). Their wall & ceiling experience
  covers only narrow use cases and is *not* typical of this audience. We have to learn
  the domain: what these contractors do in BIM day to day, and what hurts.
- **Confirmed:** SWACCA members are union-signatory wall & ceiling contractors, not
  designers. Their BIM work is likely framing layout,
  detailing, prefab/panelization, coordination, and takeoffs — and the workshop's
  examples should come from that world.
- Example of member work (complex buildings), LinkedIn post:
  https://lnkd.in/p/g5e7Dkhb
- Skill spread (Dynamo / pyRevit / C# Revit API / none): _TBD_
- Current AI tool usage among attendees: _TBD_
- What they should be able to *do* afterward: _TBD_

## 3. Goals & Outcomes

- Teach how best to use AI tools to develop Revit plugins and other programmatic
  BIM tools.
- Give attendees a takeaway package for continued study and ongoing reference.
- **Author's intended coverage (2026-10-15, brainstorm, not yet prioritized):**
  - Quick intro to what it means to work with an LLM / coding agent
  - Quick scripting in Revit
  - Connecting to Revit (AI ↔ Revit)
  - Resources
  - Best practices
  - Coder/reviewer split
  - Hit all levels, or at least *show* the levels of engagement
- The Eventbrite page and swacca.org are the only published or printed info the author
  has. Nothing more is coming from the organizers.

### Success picture: "Monday morning after the summit" (author, 2026-10-15)

What an attendee should be able to do back at their desk, in order:

1. **Think an idea through with an LLM** and plan it out together.
2. **Research with the LLM:** source and survey what already exists, as a baseline to
   reuse or repurpose, or as a precedent to learn from.
3. **Gather the relevant APIs and SDKs.**
4. **Set up a repo** and get going.
5. **Install and set up Claude Code or Codex.**
6. **Already understand long-term memory strategies** (CLAUDE.md / AGENTS.md), because
   the workshop introduced why they matter and where they fit in the levels of
   engagement.

### Reference plugin: built before the summit, shown at it (author, 2026-10-15)

- **Over the week of 10/19**, the author and Claude build a quick Revit plugin together
  and **document the process** as they go.
- It is **not** built live at the summit. The author shows the process quickly as a
  worked example.
- It becomes a **baseline attendees can start from** back at the office.
- **Deliverable = repo + process document.** Together they should serve all levels:
  read the doc, run the plugin, or fork and extend the code.
- Initial idea: **a plugin that gathers info from a Revit file.** Not settled yet.
- Should apply across the wide range of the audience.
- **Direction (author, 2026-10-15):** agreed with the domain research. The
  **MCP level** (an agent asking the Revit model questions directly) is "the real
  differentiator and the new thing."
- **Version policy (author, 2026-10-15):** reach back **as far as possible**. Assume
  firms are ~3 years behind. Most of the author's own office projects are in Revit 2024.
  The firms' actual versions are unknown and can't be found out before the summit.
  - **Claude's proposed range (not yet confirmed): Revit 2022–2027.**
    - 2022–2024 share .NET Framework 4.8. 2022 is the first version where the old unit
      API (`DisplayUnitType`) is gone, so all builds can use one unit API
      (`ForgeTypeId`).
    - 2021 is possible with small effort. 2020 and earlier would need the legacy unit
      API, which isn't worth it.
    - Version-specific code to expect: `ElementId.IntegerValue` (≤2023) vs.
      `ElementId.Value` (2024+, `IntegerValue` removed in 2026). Handled with
      conditional compilation.
- **Hard requirement:** must work on **pre-2026/2027 Revit**. Most users are on earlier
  versions, so **Revit 2024 support is needed** (feasibility: yes, see
  domain-research.md §8).
- Candidate structure (Claude's suggestion, not decided): the same wall inventory at
  three levels of engagement:
  - pyRevit script (quick scripting)
  - C# add-in (builders)
  - MCP tool (agent asks the model)

### Readiness direction and research emphasis (author, 2026-10-20)

- **Confirmed direction:** "lead with model readiness, with inventory and quantities
  supporting it." `[session-1]` `[session-2]` `[takeaway]`
- **Current task:** define three demo questions and what a sample Revit model needs
  to demonstrate them. The author says we can use sample projects shipped with Revit
  and modify them as necessary for the demonstration. `[session-1]` `[takeaway]`
- **Process capture:** "we need to be taking account of how I'm prompting you, what
  best practices are and what the overall steps are that we want to use to achieve
  our goals." Keep this record as the work happens. `[session-1]` `[takeaway]`
- **Opening lesson:** research existing work and collect required resources before
  implementation: SDKs, APIs, required libraries, dependencies, and GitHub
  repositories. Repositories may be precedents or starting points "if they have the
  proper license structure." `[session-1]` `[takeaway]`
- **Working artifacts:** `demo-questions-and-sample-model.md`,
  `research-and-resources.md`, and `build-journal.md` capture the proposed demos,
  checked resources, and collaboration process. Their detailed fixture design and
  stack recommendations remain proposals.

### Git belongs at the start (author, 2026-10-20)

- Participants need to know "pretty much from the start" what it means to direct
  an agent to work with Git. `[session-1]` `[takeaway]`
- The author wants the researched repositories cloned so the agent can inspect the
  source code. Participants need to know "that they should do this" as part of
  researching existing work. `[session-1]` `[takeaway]`
- Emphasis: knowing when and why to request cloning and Git operations, with the
  agent handling the mechanics. A working participant guide is
  `docs/directing-an-agent-with-git.md`. `[session-1]` `[takeaway]`
- Recorded action: seven candidates cloned into `references/` at the researched
  commits; origins, clean state, and evidence matches checked. Source entry points
  are in `docs/reference-code-map.md`. No candidate has been built or installed.

## 4. Lessons from Rook

Transferable lessons from building Rook, to be reframed for BIM/Revit.

- **RookBIM supports Revit 2024**, but only via **Rhino.Inside.Revit**, which is *not*
  appropriate for this audience. The workshop plugin must be a standalone Revit add-in
  plus MCP server. Rook is a **precedent** (architecture, lessons, patterns) but not the
  codebase. (author, 2026-10-15)
- _More to be filled in_

### Levels of engagement (2026-10-15)

- **Author:** span all levels, but **breeze through the non-scripter**. That person
  is "more of a spectator."
- **Claude's proposed ladder (draft, for author to correct):**
  - **L0 Spectator:** asks the model questions through an AI chat app connected via
    MCP. Never sees code. *Breeze through.*
  - **L1 Scripter:** uses an LLM to write and fix quick scripts (pyRevit / Dynamo
    Python), copy-paste style.
  - **L2 Agent-assisted builder:** Claude Code / Codex in a repo with CLAUDE.md /
    AGENTS.md, building the C# add-in.
  - **L3 Systems builder:** builds MCP tools, coder/reviewer split, multi-version
    builds, tests.
- **Author accepted the ladder as a good start** (2026-10-15). The same wall-inventory
  plugin shows up at every level.
- **Opening calibration `[session-1]`:** start Session 1 by engaging the room right away
  with questions about their average level of experience. Use the answers to pitch the
  presentation: don't go advanced too fast, and don't leave anyone out.

## 5. Topics & Themes (candidate)

### Working Option B structure (author, 2026-10-20)

> Ok, let's update Option B around our current plan, emphasizing research, Git, how we built the plugin, and using bim-agent-lab afterward. Keep it to two one-hour sessions. This will evolve as we proceed, but it is the general structure.

Confirmed direction: use a variation of Option B, “The story of the build,” for
two one-hour sessions. Emphasize research and resource gathering, directing agents
with Git, the actual plugin development process, and using BIM Agent Lab afterward.
The [working agenda](agenda.html) supplies the current proposed minute allocations;
the author explicitly expects them to evolve. `[session-1]` `[session-2]` `[takeaway]`

### Earlier topic collection

Brainstorm of subject matter; not yet prioritized.

- Research existing work before choosing an implementation; compare reuse with
  building the missing pieces. Inspect actual license files and dependency manifests,
  and distinguish documented compatibility from a local test. (author emphasis,
  2026-10-20) `[session-1]` `[takeaway]`
- Use our own prompts and resulting decisions to explain a repeatable process.
  Record corrections and changed assumptions along with successful steps.
  (author, 2026-10-20) `[session-1]` `[takeaway]`
- Introduce directing an agent with Git at the beginning: clone existing work for
  inspection, understand local commits and remote publishing, and request branches,
  diffs, and checkpoints during development. (author emphasis, 2026-10-20)
  `[session-1]` `[takeaway]`

## 6. Tools & Stack

AI tools, IDEs, Revit API versions, frameworks to cover or recommend.

- Initial checked resource register: `docs/research-and-resources.md` (2026-10-20).
  Includes local Revit/API/runtime availability, official SDK sources, repository
  license evidence, and dependencies. `[takeaway]`
- C# inside Revit with an external MCP process remains the proposed separation.
  Python versus TypeScript versus C# outside Revit, and adapting an existing bridge
  versus creating a small one, remain research decisions. `[session-2]` `[takeaway]`

## 7. Demos & Examples

Live demos or worked examples for the sessions.

- Author selected model readiness, with inventory and quantities supporting it
  (2026-10-20). `[session-1]` `[session-2]`
- Agent's three proposed questions: required type information; wall-top and
  ceiling-height conditions; quantities separated by readiness findings.
  See `docs/demo-questions-and-sample-model.md`. `[session-1]` `[takeaway]`
- Snowdon Towers architectural and structural files were found in the local Revit
  2024 Samples folder. Their actual element data still needs inspection. The proposed
  demo uses a working copy with known defects and valid controls. `[takeaway]`

## 8. Takeaway Package Contents

Candidate items for the take-home package.

- **Shared teaching repository (author, 2026-10-20):** "after the seminar the users
  can point their agents to [it] and learn and refresh on how to do this."
  `[takeaway]`
- The seminar is too short for a full walkthrough. Show how the plugin was built;
  give participants the plugin to build on and the information needed to extend
  it and build their own. `[session-1]` `[session-2]` `[takeaway]`
- **Chosen name (author, 2026-10-20):** `bim-agent-lab`, displayed as **BIM Agent
  Lab**. "I think bim-agent-lab keeps it more open and allows it to grow beyond
  revit. We don't want to foreclose the possibility that this will continue to
  evolve." This supersedes the agent's `revit-agent-lab` recommendation. The
  fresh repository has not yet been created. `[takeaway]`
- Prompting and decision journal, including research discoveries, changes of
  direction, and verification evidence. (author, 2026-10-20) `[takeaway]`
- Resource register with versioned sources, license evidence, dependency decisions,
  and the distinction between prerequisites and shipped components. `[takeaway]`
- Demo preparation recipe and an independent expected-results sheet. `[takeaway]`

## 9. Open Questions

- Who gives the third education session and the keynote? We want to avoid overlapping
  with them.
- Which vendors are in the tech demo showcase? Overlap risk again, and a chance to
  connect to their tools.
- Can our content tie into the Summit Challenge? Will teams have laptops and software
  during build time?
- Who are the attendees at these companies — VDC/BIM managers, detailers,
  estimators, in-house developers?
- What tools do these firms already use (Revit, Dynamo, pyRevit, AGACAD/Framing
  add-ins, StrucSoft MWF, Navisworks, Bluebeam)?
- Hands-on vs. presentation/demo — can attendees follow along live?
- Confirm the hosting account and release packaging for `bim-agent-lab`.
  The name and repository delivery are now established; an
  additional offline package or documentation website remains optional.
- Which area, level, and phase in the Revit sample best support the three demos?
  Resolve through inspection; do not infer from filenames.
- Which bridge is the clearest maintainable starting point after a Revit 2024
  build-and-read check?
- Which model and SDK assets can be included in the takeaway, and which should be
  acquired by attendees from their original sources?

## 10. Raw Notes / Parking Lot

Unsorted input goes here until it finds a home.
