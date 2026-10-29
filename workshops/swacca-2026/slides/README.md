# Seminar slides: present, print and revise

[Session 1 deck](session-1.html) | [Session 2 deck](session-2.html) | [Agenda](../agenda.html) | [Handout guide](../handout/README.md)

## Author's choices, October 28

The author chose an HTML deck in the repository, one deck per session, content that follows the printed handout, and evidence screenshots instead of a live Revit demonstration. Revit needs Windows; the slides are presented from a Mac. These were selections from options the agent offered, not free-text instructions.

## Present

Open a deck in Chrome, Edge or Safari from a checkout of the repository. The decks load their shared [styles](deck.css) and [script](deck.js), the [repository QR code](assets/repo-qr.png) and evidence screenshots under `projects/revit-readiness/docs/verification/` by relative path, so keep the folder structure intact. No server is needed. The fonts come from Google Fonts; without internet the decks fall back to system fonts. Open each deck once on the venue network, or check the fallback rendering beforehand.

**Presenting from Windows.** Clone with long paths enabled (`git clone -c core.longpaths=true https://github.com/bringfire/bim-agent-lab.git`), or pull an existing checkout, then open `workshops\swacca-2026\slides\session-1.html` in Chrome or Edge. Without the Google Fonts, Windows uses Bahnschrift (semi-condensed) for headings, Segoe UI for text and Cascadia Mono or Consolas for code. On October 28 both decks were checked with every font replaced by Arial, which is wider than Bahnschrift: nothing crossed the footer, the right margin or a diagram box. For two screens, set Windows to **Extend** (Windows key + P), put the deck fullscreen (F) on the projector and drag the presenter window (S) to the laptop.

| Key | Action |
|---|---|
| → Space PageDown Enter | Next build, then next slide (a presentation clicker works) |
| ← PageUp Backspace | Back one build or slide |
| Home / End | First / last slide |
| 12 Enter | Go to slide 12 |
| N | Speaker notes along the bottom, for single-screen rehearsal |
| S | Presenter window: current and next slide, notes, clock, elapsed timer, agenda block and slide list |
| T (presenter window) | Start or reset the elapsed timer |
| L | Light or dark theme (remembered in this browser) |
| F | Fullscreen |
| ? | Key help |

For two screens, press **S**, move the presenter window to the laptop screen and put the deck fullscreen on the projector. Either window can drive the slides; the audience window stays in charge. The presenter timer compares elapsed time with the current slide's agenda block, using the session start in each deck's `data-start` attribute (8:30 and 11:00).

A slide's URL ends with its number and build (`session-1.html#15.2`), so a reload stays where you were.

## Print or save as PDF

Print from Chrome, choose **Save as PDF**, and turn on background graphics. Each slide prints on its own 16:9 page (960 x 540 points). Speaker notes are not printed. A PDF is a backup if the presenting machine can't open the HTML.

## Structure and sources

Each deck groups slides into `<div class="block">` elements carrying the agenda block's label, start and end minutes, and colour. The script adds the kicker (`Research existing work · 18–33 min`), the footer with the handout page, and slide numbers. Speaker notes are `<aside class="notes">` inside each slide. To revise, edit the HTML directly; there is no build step.

Most slides build click by click, and many carry diagrams in inline SVG or HTML: the session timeline, the branching example, the Git and commit-line diagrams, the 100-element dot field, versions at every boundary, the sample and coverage squares, the two-lane division of work, the build staircase, the MCP round trip, the protocol handshake, the recovery timeline, the stale-context sequence, the review loop, the evidence ladder and the two-reader repository map. Screenshot slides outline the relevant lines in the real dialogs and schedules as you click. A slide marked `data-optional` is flagged in the presenter view and slide list as safe to skip. Inside a slide, `data-step="n"` appears on the nth click, `data-hide="n"` disappears from it, and `data-mark="n"` (used for table rows) stays visible but lights up from it, so a slide builds with the clicker before the deck moves on; the presenter view shows the next build. [diagrams.js](diagrams.js) draws the repeated dots and instance squares. Printing shows every slide fully built, and animation is off for anyone whose system asks for reduced motion.

Slide timings follow the [Option B agenda](../agenda.html), which remains the timing source. Wording follows the [handout content](../handout/content.json); footers cite the matching handout page. Additional material comes from:

| Slide content | Source |
|---|---|
| Actual author prompts | Participant stories [00](../../../docs/tutorials/00-research-before-building.md), [01](../../../docs/tutorials/01-directing-an-agent-with-git.md), [02](../../../docs/tutorials/02-build-a-read-only-revit-command.md), [03](../../../docs/tutorials/03-check-required-type-information.md), [04](../../../docs/tutorials/04-add-an-excel-report-dependency.md), [06](../../../docs/tutorials/06-direct-design-and-external-review.md), [07](../../../docs/tutorials/07-test-desktop-compatibility-first.md) |
| Opening AI-experience question | The author's direction, October 28: gauge experience with chat interfaces, desktop agents, and agents working in code repositories. It replaces the October 15 [level-of-engagement ladder](../planning-notes.md) on that slide |
| Revit, Excel and schedule screenshots | [Required-information](../../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md), [Excel](../../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md), [read-only probe](../../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) and [live MCP](../../../projects/revit-readiness/docs/verification/mcp-readiness.md) evidence |
| Claude Desktop answer quotations | [Wall-defects answer](../../../projects/revit-readiness/docs/verification/mcp-readiness-evidence/2026-10-22-wall-defects/claude-wall-defects/answer.txt), pasted by the author |
| Git checkpoints | The repository's own history (`git log --reverse`) |
| Diagram facts | Sample rules and the five cases from handout page 5 and the live MCP evidence; the three plan-review findings from the [review story](../../../docs/tutorials/06-direct-design-and-external-review.md); protocol revisions from the [compatibility story](../../../docs/tutorials/07-test-desktop-compatibility-first.md). The dot field and the Git branch and diff are labelled illustrations |
| Superpowers brainstorming slide | The [agent review procedure](../../../docs/how-to/run-spec-plan-and-implementation-reviews.md) records the installed skill (Superpowers 6.4.2, `brainstorming`) and its approval gates; the [project page](https://github.com/obra/superpowers) was checked on October 28 for the source, license, supported agents and current release |
| "Where agents can plug into your tools" and "One base, extended tool by tool" | The [integration research](../../../docs/research/wall-ceiling-tool-integrations-2026-10-28.md) (October 28, from vendor pages) and the [extension roadmap](../../../docs/explanation/mcp-extension-roadmap.md); everything past stage 0 is a plan, not support |
| Recovery counts, verified/not-verified slide | [Development status](../../../docs/reference/development-status.md), [live installation](../../../projects/revit-readiness/docs/verification/2026-10-21-live-installation.md) |

Prompts are labelled **Actual prompt** (quoted from the build) or **Suggested prompt** (reusable wording). Keep that distinction when editing. The QR code was cropped from page 8 of the delivered handout PDF and decodes to `https://github.com/bringfire/bim-agent-lab`.

The slides state the current limits: Revit 2024 build 24.3.60.12 is the verified host. Revit 2026/2027, physical-condition and quantity checks, a packaged release and full implementation acceptance are not claimed. Page 8 of the printed handout still says the repository is private during preparation; Session 2 tells the room it has been public since October 26.

## Verification, October 28

- Both decks were checked at 1280 x 720 in a browser: no slide content crosses the footer or right margin (29 and 28 slides), and every image loads.
- Selected slides, the notes overlay, the light theme and the standalone presenter view were inspected visually.
- Headless Chrome confirmed presenter/audience sync in both directions with a real popup window, and printed both decks to PDF: 29 and 28 pages at 960 x 540 points.
- The QR image was decoded with macOS Core Image.

After the diagrams were added the same day: with every build shown, neither deck has content past the footer or right margin. Headless Chrome confirmed build-level sync (two presenter clicks on the Git slide put the audience at `#10.2`; stepping back into a built slide lands on its last build), and printed pages show finished diagrams.

After the slide-by-slide pass (also October 28): with every build shown, neither deck has content past the footer or right margin and every image loads. Headless Chrome again confirmed presenter/audience sync through builds in both directions and printed 29 and 28 pages at 960 x 540 points. Screenshot outlines were placed from measured pixel positions and checked visually.

Not yet done: a rehearsal against the agenda timings, a test on the venue projector and clicker, and a check of the offline font fallback on the presenting machine.
