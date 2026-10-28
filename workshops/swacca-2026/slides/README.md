# Seminar slides: present, print and revise

[Session 1 deck](session-1.html) | [Session 2 deck](session-2.html) | [Agenda](../agenda.html) | [Handout guide](../handout/README.md)

## Author's choices, October 28

The author chose an HTML deck in the repository, one deck per session, content that follows the printed handout, and evidence screenshots instead of a live Revit demonstration. Revit needs Windows; the slides are presented from a Mac. These were selections from options the agent offered, not free-text instructions.

## Present

Open a deck in Chrome, Edge or Safari from a checkout of the repository. The decks load their shared [styles](deck.css) and [script](deck.js), the [repository QR code](assets/repo-qr.png) and evidence screenshots under `projects/revit-readiness/docs/verification/` by relative path, so keep the folder structure intact. No server is needed. The fonts come from Google Fonts; without internet the decks fall back to system fonts. Open each deck once on the venue network, or check the fallback rendering beforehand.

| Key | Action |
|---|---|
| → Space PageDown Enter | Next slide (a presentation clicker works) |
| ← PageUp Backspace | Previous slide |
| Home / End | First / last slide |
| 12 Enter | Go to slide 12 |
| N | Speaker notes along the bottom, for single-screen rehearsal |
| S | Presenter window: current and next slide, notes, clock, elapsed timer, agenda block and slide list |
| T (presenter window) | Start or reset the elapsed timer |
| L | Light or dark theme (remembered in this browser) |
| F | Fullscreen |
| ? | Key help |

For two screens, press **S**, move the presenter window to the laptop screen and put the deck fullscreen on the projector. Either window can drive the slides; the audience window stays in charge. The presenter timer compares elapsed time with the current slide's agenda block, using the session start in each deck's `data-start` attribute (8:30 and 11:00).

A slide's URL ends with its number (`session-1.html#15`), so a reload stays on the same slide.

## Print or save as PDF

Print from Chrome, choose **Save as PDF**, and turn on background graphics. Each slide prints on its own 16:9 page (960 x 540 points). Speaker notes are not printed. A PDF is a backup if the presenting machine can't open the HTML.

## Structure and sources

Each deck groups slides into `<div class="block">` elements carrying the agenda block's label, start and end minutes, and colour. The script adds the kicker (`Research existing work · 18–33 min`), the footer with the handout page, and slide numbers. Speaker notes are `<aside class="notes">` inside each slide. To revise, edit the HTML directly; there is no build step.

Slide timings follow the [Option B agenda](../agenda.html), which remains the timing source. Wording follows the [handout content](../handout/content.json); footers cite the matching handout page. Additional material comes from:

| Slide content | Source |
|---|---|
| Actual author prompts | Participant stories [00](../../../docs/tutorials/00-research-before-building.md), [01](../../../docs/tutorials/01-directing-an-agent-with-git.md), [02](../../../docs/tutorials/02-build-a-read-only-revit-command.md), [03](../../../docs/tutorials/03-check-required-type-information.md), [04](../../../docs/tutorials/04-add-an-excel-report-dependency.md), [06](../../../docs/tutorials/06-direct-design-and-external-review.md), [07](../../../docs/tutorials/07-test-desktop-compatibility-first.md) |
| Opening AI-experience question | The author's direction, October 28: gauge experience with chat interfaces, desktop agents, and agents working in code repositories. It replaces the October 15 [level-of-engagement ladder](../planning-notes.md) on that slide |
| Revit, Excel and schedule screenshots | [Required-information](../../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md), [Excel](../../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md), [read-only probe](../../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) and [live MCP](../../../projects/revit-readiness/docs/verification/mcp-readiness.md) evidence |
| Claude Desktop answer quotations | [Wall-defects answer](../../../projects/revit-readiness/docs/verification/mcp-readiness-evidence/2026-10-22-wall-defects/claude-wall-defects/answer.txt), pasted by the author |
| Git checkpoints | The repository's own history (`git log --reverse`) |
| Recovery counts, verified/not-verified slide | [Development status](../../../docs/reference/development-status.md), [live installation](../../../projects/revit-readiness/docs/verification/2026-10-21-live-installation.md) |

Prompts are labelled **Actual prompt** (quoted from the build) or **Suggested prompt** (reusable wording). Keep that distinction when editing. The QR code was cropped from page 8 of the delivered handout PDF and decodes to `https://github.com/bringfire/bim-agent-lab`.

The slides state the current limits: Revit 2024 build 24.3.60.12 is the verified host. Revit 2026/2027, physical-condition and quantity checks, a packaged release and full implementation acceptance are not claimed. Page 8 of the printed handout still says the repository is private during preparation; Session 2 tells the room it has been public since October 26.

## Verification, October 28

- Both decks were checked at 1280 x 720 in a browser: no slide content crosses the footer or right margin (29 and 28 slides), and every image loads.
- Selected slides, the notes overlay, the light theme and the standalone presenter view were inspected visually.
- Headless Chrome confirmed presenter/audience sync in both directions with a real popup window, and printed both decks to PDF: 29 and 28 pages at 960 x 540 points.
- The QR image was decoded with macOS Core Image.

Not yet done: a rehearsal against the agenda timings, a test on the venue projector and clicker, and a check of the offline font fallback on the presenting machine.
