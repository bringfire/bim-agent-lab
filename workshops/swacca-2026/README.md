# SWACCA BIM Summit 2026

The seminar introduces BIM professionals at wall and ceiling contractors to directing AI coding agents. Two one-hour sessions provide orientation and selected demonstrations; BIM Agent Lab provides the continuing learning resource and eventual plugin takeaway.

The recorded event dates are October 29–30, 2026, in Pleasanton, California. The author's two sessions are planned for October 29. See [planning notes](planning-notes.md) for the source context and agenda details.

## Current teaching direction

**October 23 participant packet:** [eight-page Letter PDF](../../output/pdf/swacca-2026-participant-handout-oct29-30.pdf), [editable content and agent rebuild guide](handout/README.md), and [participant account of preparing it](../../docs/tutorials/16-prepare-the-participant-packet.md). It preserves the two one-hour structure and fits the quick Dynamo/pyRevit introductions into the existing first-working-step block. pyRevit is an optional separate install, not an added attendee requirement. These demos await rehearsal. The private [GitHub repository](https://github.com/bringfire/bim-agent-lab) provides the stable QR/link destination; original material uses MIT (selected October 26); public access and release packaging remain pending. Historical development notes below are snapshots; use the root README and acceptance ledger for current status.

**Working agenda:** [Option B — the story of the build, revision B](agenda.html).
It sets out two 60-minute sessions: first, the BIM question, Git, research,
requirements, and the first working step; second, the complete plugin/MCP flow,
verification, and continued learning with BIM Agent Lab. Minute allocations and
examples can evolve as implementation and rehearsal develop. The HTML is the
current source for session timing; each hour includes five minutes for questions.

- Begin with audience, intended BIM outcome, and directing an agent with Git.
- Research existing work; clone and inspect source, dependencies, SDKs/APIs, and terms.
- Show how a declared requirement becomes a model-readiness check with independent acceptance evidence.
- Use the [three Revit demonstrations](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md) to connect findings, inventory, and quantities.
- Point participants to the [learning path](../../docs/tutorials/README.md) and the evolving [build story](../../docs/build-story/journal.md).

Use the [participant learning path](../../docs/tutorials/README.md) for the whole build: research, Git, Revit setup, the readiness check, and Excel. Each story focuses on the author's direction and links a technical agent guide for use afterward.

For the Excel increment, use the [participant story](../../docs/tutorials/04-add-an-excel-report-dependency.md) to show how the author engaged the agent: the initial capability question, the bounded implementation prompt, the failed first host run, and the evidence behind acceptance. The [agent guide](../../docs/how-to/add-a-plugin-dependency.md) is the technical follow-through for use with the repo afterward. During the seminar, focus on what the person asked, what the agent handled, and how the person could judge the result. The two one-hour sessions remain as allocated in the agenda.

Use the [MCP comparison story](../../docs/tutorials/05-research-and-choose-the-mcp-bridge.md) to teach asking for current-version checks: the protocol revision, SDK release, package name, lockfile, and actual-client compatibility are separate evidence. Present its bridge recommendation as research pending implementation.

Use the [specification and review story](../../docs/tutorials/06-direct-design-and-external-review.md) to show the author's follow-up: request a written spec and plan, then hand reviewable artifacts to an external reviewer. The [review record/packets](../../docs/superpowers/reviews/revit-mcp-bridge/README.md) now include actual R1 findings and revision 2 dispositions: identify both desktop clients, test incomplete coverage, and bound incomplete requests. R1 follow-up closed all findings and the author approved revision 2. The [implementation plan](../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md) is now revision 2 after external R2 findings. The participant lesson shows asking what connects several defects: lifetime/ownership across interruption. [Follow-up review](../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) closes all four findings and the underlying concern at the plan level; the early compatibility checkpoint still tests both desktop applications before connection work. The author subsequently approved the plan and selected Native execution. The [compatibility story](../../docs/tutorials/07-test-desktop-compatibility-first.md) shows an SDK test passing while both actual desktop apps fail the required protocol; this stops dependent bridge work early. Live Revit/MCP proof remains pending. These development sessions do not add time to the two one-hour seminar sessions.

Seminar takeaway for the review segment: **two or three major findings are a cue to ask the independent reviewer whether a shared concept is missing.** Ask for an explanation the coding agent can apply, then return it with the original findings and have the reviewer assess the revisions. Use the [paired reviewer/coder prompts and R2 example](../../docs/tutorials/06-direct-design-and-external-review.md#ask-what-connects-several-findings). The count prompts investigation; it does not prove a common cause. Teach this within the existing review block, with no additional session time.

## Planning archive

- [Raw planning notes](planning-notes.md) retain earlier proposals and author input.
- [Organizer brief](archive/organizer-brief.html) and [proposal PDF](archive/SWACCA-Workshop-Proposal.pdf) are historical drafts, not the final agenda or current product specification.
- [Original project context](archive/source-project-context.md) preserves the authoring conventions from which this repository grew.

The working session timings are now in the agenda. Final slides, detailed demonstration scripts, rehearsal evidence, and a tagged seminar release are still to be produced. HTML may be used as the initial presentation format before slides.
