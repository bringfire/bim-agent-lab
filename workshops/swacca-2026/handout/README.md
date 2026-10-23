# Participant packet: source, printing and agent handoff

[Print PDF](../../../output/pdf/swacca-2026-participant-handout.pdf) | [Participant story](../../../docs/tutorials/16-prepare-the-participant-packet.md) | [Source content](content.json) | [Generator](build_handout.py)

## Author's requirements

US Letter, black-and-white friendly, about eight pages. Participants are beginners: focus on **how to engage the agent**, useful prompts, decisions and independent verification. Retain two one-hour sessions. The repository carries technical procedures. The author separately installed pyRevit; do not turn that into an attendee prerequisite or claim a successful demo rehearsal.

## Deliverable

Eight portrait Letter pages (612 x 792 points), with grayscale styling, embedded text fonts, live PDF resource links, a repository QR code, reusable prompts and note lines. Body text is 10.5 points; table text 9.4 points. Print at actual size on US Letter. Duplex uses the long edge and produces four sheets; single-sided produces eight. This is ordinary page order, not booklet imposition.

The footer deliberately says **Participant draft**. Page 8 accurately says the GitHub destination is private during preparation. Before the print run, confirm the access/release decision, update that sentence and edition if appropriate, and regenerate/review. Do not promise a publicly downloadable plugin or a closed acceptance gate. Full R3 remains open.

## Rebuild

`content.json` is the editorial source of truth. Page grouping is explicit. `build_handout.py` handles layout only. Work from the repository root with Python 3 and `reportlab` plus `pypdf` available:

```powershell
python workshops/swacca-2026/handout/build_handout.py
pdftoppm -scale-to 1200 -png output/pdf/swacca-2026-participant-handout.pdf tmp/pdfs/handout
```

Create `tmp/pdfs` before rendering. It is ignored; the final PDF is intentionally tracked. The generator uses locally installed Segoe UI/Consolas on Windows or DejaVu Sans/Mono on Linux. It does not vendor fonts. Font substitutions can change pagination: every rebuild needs the page-count check and visual review. The tool requires exactly eight Letter pages with the expected page titles and fails otherwise. There is no Revit/API/MCP dependency in document generation.

For this preparation, Codex's `load_workspace_dependencies` located bundled Python, ReportLab, pypdf, pdfplumber and Poppler. No package was installed. The generator's dependency versions and final PDF hash are in [verification.json](verification.json). Use the runtime discovery tool again on another machine; do not copy the author's user-specific paths into a participant's setup.

Review all eight rendered PNGs, then use `pdfplumber` to check text boundaries and `pypdf` to check page size, links and fonts. Rendered layout is authoritative for clipping/overlap. Also compare the five-case table to the canonical independent expectations, confirm both session totals remain 60 minutes, and verify the actual remote visibility before publication. No new product tests are required for these documentation changes.

## Source map and claims

| Packet | Source |
|---|---|
| Timing and audience | [Option B agenda](../agenda.html), [planning context](../planning-notes.md); summarized timings retain two hours |
| Prompting, research and Git | [Starting guide](../../../docs/start-here.md), [research story](../../../docs/tutorials/00-research-before-building.md), [Git story](../../../docs/tutorials/01-directing-an-agent-with-git.md) |
| Five readiness cases | [Sample specification](../../../projects/revit-readiness/docs/required-type-information.md), [live evidence](../../../projects/revit-readiness/docs/verification/mcp-readiness.md); packet table states expectations, not current-build acceptance |
| Excel and provenance | [Dependency story](../../../docs/tutorials/04-add-an-excel-report-dependency.md), [MCP research](../../../docs/tutorials/05-research-and-choose-the-mcp-bridge.md), [notices](../../../THIRD_PARTY_NOTICES.md) |
| Review lessons | [Spec/plan review](../../../docs/tutorials/06-direct-design-and-external-review.md), [evidence review](../../../docs/tutorials/15-prepare-an-evidence-based-review.md) |
| Current limits | [R3 ledger](../../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md), [follow-up](../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-follow-up.md) |

Official scripting sources consulted October 23, 2026:

1. [Autodesk: Dynamo for Revit 2.19.3](https://help.autodesk.com/cloudhelp/2024/ENU/RevitDynamo/files/RevitDynamo_Whats_New_in_Dynamo_for_Revit_html.html) - automatic inclusion with Revit 2024.
2. [Dynamo: Python 3 installation](https://github.com/DynamoDS/Dynamo/wiki/Customizing-Dynamo's-Python-3-installation) - embedded Python, not a separate PATH installation.
3. [pyRevit project](https://github.com/pyrevitlabs/pyRevit) and [extension structure](https://docs.pyrevitlabs.io/extensions/) - separate add-in, script bundles and engines.
4. [Dynamo Primer: Python](https://primer2.dynamobim.org/8_coding_in_dynamo/8-3_python/1-python) - Python Script node/editor/output workflow.

The two-line Dynamo snippet is only a runtime smoke check. No newly authored inventory script, Dynamo graph or pyRevit extension was executed in this increment. Those rehearsals remain separate work. Prompts are labeled suggested; do not turn them into purported historical quotations.

## Verification and boundaries

All eight final page images were visually inspected. Page size/count, text bounds, expected page titles, link destinations and embedded fonts were checked. A first layout overflow was corrected before delivery. No production code, installed dependency, application settings, candidate binaries or acceptance evidence was changed. QR generation uses the exact real repository URL; physical paper scanning and printer output remain untested. Hosting is separate from public access and plugin release; see [sharing status](../../../docs/reference/repository-sharing.md).
