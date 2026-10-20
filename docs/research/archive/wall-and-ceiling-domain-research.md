> Historical research imported from the authoring folder. Broad market and version-support claims are not established compatibility results. Consult the [checked resource register](../revit-resources.md) for qualifications and evidence.

# Domain Research: Wall & Ceiling Contractors and BIM

Research done 2026-10-15 by three parallel AI research agents and synthesized by
Claude. Purpose: learn the domain, because the author is not a wall & ceiling insider,
and pick a reference plugin that fits.

**Confidence labels:** **[R]** = read on the source page. **[S]** = seen only in a
search-result summary (the page itself was blocked or not fetched). **[I]** = inference,
not stated by any source. Several vendor items are marketing claims.

> **Process note for the workshop:** this file is itself an example of step 2 in the
> "Monday morning" sequence (research what exists before building). It took ~4 minutes
> of wall-clock time with three agents in parallel. Worth showing.

---

## 1. Headline findings

1. **SWACCA has already endorsed in-house plugin development.** The 2025 BIM Summit
   ("Learn, Build, Solve," La Vergne, TN) had sessions on automation, AI and **custom
   plug-in development**. Its first takeaway: *"Having software development skills
   in-house can be a major differentiator."* [R]
   https://swacca.org/3-key-takeaways-from-the-2025-bim-summit-learn-build-solve/
2. **Their #1 stated challenge is office → field.** Past summit attendees ranked
   "getting BIM data and coordination decisions to field crews" top by impact and
   feasibility. The 2026 Summit Challenge is expected to tackle real coordination
   problems. [R] https://swacca.org/closing-the-gap/
3. **The #1 recurring technical pain is the architect's wall types.** Design models
   arrive with generic or performance-only partition types (LOD 200–300). Subs remap
   them by hand to real assemblies, then struggle to get usable quantities out of
   Revit. Wall layers can't be scheduled, and material takeoffs go wrong at layer
   joins. (Sources in §4.)
4. **Nothing free fills the gap.** Takeoff tools (On Center, Togal, Kreo) work from 2D
   PDFs. Framing add-ins (MWF, AGACAD) are expensive. ClarkDietrich's free Wall Type
   Creator looks stuck at Revit 2022. No open-source wall & ceiling takeoff or audit tool
   was found. [R/I]
5. **Autodesk shipped an official read-only Revit MCP server (Revit 2027.2+, April
   2026).** It has 7 generic tools and auto-configures Claude Desktop and Cursor. Several
   community MCP servers also exist. "Connecting AI to Revit" is now a real,
   demonstrable thing, not a Rook-only capability. [R]
   https://blog.autodesk.io/revit-2027-mcp-server-release/
6. **No one in the trade seems to be writing their own code with AI agents yet.**
   Automation today = vendor add-ins + some Dynamo. AI adoption = PDF takeoff, progress
   tracking, robots. The workshop would be early. [R/I]

---

## 2. The audience: what the attending firms do

| Firm | Scope | BIM/VDC work | Tools named |
|---|---|---|---|
| **Anning-Johnson** | Interior/exterior walls, ceilings, decking, roofing, fireproofing, insulation, prefab | 3D modeling, layout verification, shop drawings, in-wall blocking models, LGS panelization, coordination [R] | Revit, Navisworks, AutoCAD [S] |
| **Double L Engineering** (part of Double L Management, OH, with Acoustic Ceiling & Partition and others) | Framing, ceilings | BIM coordination, **Dusty FieldPrinter robotic layout**, in-house stud/track roll forming, kitting by room [R] | Dusty |
| **Valley Interior Systems** (Cincinnati, union) | Drywall, framing, ceilings, plaster | Prefab walls, soffits, column wraps, blocking, exterior CFMF panels. BIM standards/QC, **2D for HP SitePrint**, GC coordination [R/S] | Revit, AutoCAD, SitePrint |
| **TJ Wies** (St. Louis) | Framing, drywall, insulation, carpentry, ceilings | 23,000 sq ft prefab shop. BIM shop drawings, **quantities from models**. 277 bathroom pods + 224 headwalls for BJC (SFIA 2024 award) [R] | StoPanel |
| **Nevell Group** (Brea, CA) | Framing, lath/plaster, drywall, EIFS, ceilings, prefab panels | Updates the architect's Revit model, sends Navisworks clash XMLs before coordination meetings, shop drawings [R] | Revit, ACA, Navisworks Manage, BIM 360, Rhino, **Dynamo, Grasshopper**, Bluebeam, Smartsheet, OST/Quick Bid |
| **Raymond Group** (Western US, est. 1936) | Framing, drywall, plaster, EIFS, panels, ceilings, fireproofing | VE, complex assemblies, prefab + MEP integration, 4D/5D, **"panel tickets"** per prefab panel, robotic layout [R/S] | Revit, Rhino, ACA, Navisworks Manage, BIM 360, Bluebeam, Point Layout |

**Common stack:** Revit + Navisworks + AutoCAD + Bluebeam. Nevell already lists Dynamo
and Grasshopper, so at least some attendees will be scripting-literate.

**Every firm does prefab/panelization. Every firm does robotic or total-station layout.**

LinkedIn post shared by organizer: SWACCA's 10th-anniversary post on PCI's work at the
Las Vegas Sphere (3.3M sq ft drywall, 240K sq ft ceilings, prefab). Not BIM-specific.

---

## 3. The typical sub workflow (design model → field)

1. **Brought in late; receive the architect's Revit model**, plus drawings, RFIs and
   specs via Procore/Prolog. [R]
   https://www.wconline.com/articles/93648-bim-and-the-contractor
2. **Interpret partition types.** Architects define walls by generic layers or
   performance (fire, STC). The sub converts them to real assemblies, and the format
   differs on every project. PARTie, a free library of 500+ standard wall types, exists
   for this reason. [R/S]
3. **Rebuild for framing.** Split walls where framing ends, fix joins, set the structure
   layer. Framing goes in via add-ins (MWF, ClarkDietrich, AGACAD), since Revit can't
   frame inside a wall natively. LOD 300 → 400. [S]
   https://arkance.world/global/resources/read/blogs/best-practices-for-framing-in-revit
4. **Coordinate with MEP.** Link trade models, place openings and penetrations, carry
   firestop, clash in Navisworks. Their role is reactive: their walls get clashed. Settle
   "priority walls." In-wall blocking keeps coming up. [R]
   https://www.wconline.com/articles/87856-what-s-bim-to-you
5. **Produce quantities, shop drawings and prefab output** (panel tickets, CNC, cut
   lists). [S/R]
6. **Lay out in the field.** Previously X/Y/Z points to a total station. Now HP SitePrint
   or Dusty printing on the slab from coordinated 2D. A multi-trade layout took 3–4 file
   revisions to agree. [R]
   https://www.wconline.com/articles/96189-drywall-and-framing-contractor-leads-multi-trade-layout-process
7. **Revisions, addenda, as-builts, progress billing.** Change narratives live in Excel.
   One estimator reports ~2 hours to update a model through 8 addenda. [S]

---

## 4. Pain points (ranked by how many sources raise them)

1. **Generic, inconsistent architect wall types → manual remapping.** [R/S, 5+ sources]
   Design models lack framing, so framing quantities can't be taken off for purchasing.
   https://itcon.org/paper/2020/30
2. **MEP conflicts, late involvement, top-of-wall and penetration rework.** GCs leave
   drywall out of BIM; above-ceiling space is never enough. [R/S]
3. **Getting usable quantities out of Revit.** [S]
   - Wall layers can't be scheduled, so people keep text parameters and check them by
     hand.
     https://forums.autodesk.com/t5/revit-ideas/schedule-wall-layers-for-wall-type-schedules/idc-p/9833627
   - Material takeoffs are wrong at layer joins, and Parts overcount layer area.
   - Board counts get done in Excel, split by fire-rated, moisture-resistant and
     standard board.
4. **Prep before automated framing:** splitting walls, cleaning joins, top-of-wall
   constraints (to deck vs. to ceiling). [S]
5. **Model to field without losing it.** SWACCA's own #1. [R]
6. **Tracking revisions and addenda for change orders.** CMs won't pay for
   design-driven BIM changes. [S/R]
7. **Ceilings:** native ceiling grids can't carry suspension parts. Reflected ceiling
   plans must show hangers and seismic bracing. [S]

### Data they repeatedly need out of a Revit model

- **Per wall:** type mark, fire rating (UL), STC, length, height, gross and net area
  (after openings), top-of-wall condition, board layers per side and type.
- **Framing:** stud size, gauge, spacing, limiting height, deflection track,
  header/jamb/sill at openings.
- **Openings and penetrations:** location, firestop requirement.
- **Layout:** track start/end points, radii, penetration coordinates.
- **Ceilings:** type, height, ceiling-mounted items, hanger and bracing locations.
- **[I]** All grouped **by level and area/room**, matching how they price, procure and
  bill.

---

## 5. Tool landscape (what exists, so we complement it)

### Commercial

- **Framing:**
  - **StrucSoft MWF (Pro Metal / Advanced Metal):** CFS framing in Revit, with shop
    drawings, cut lists, CNC output and ClarkDietrich/CEMCO/SCAFCO libraries.
  - **AGACAD Metal Framing.**
  - **Vertex BD:** standalone, not Revit.
  - **ClarkDietrich Wall Type Creator:** free, but appears stuck at Revit 2018–2022.
- **Ceilings:**
  - **USG Ceiling Technix:** free add-in for specialty ceiling layouts.
  - **Armstrong:** Revit content plus a free modeling service (PROJECTWORKS).
  - **Rockfon:** no plugin found.
- **Estimating and takeoff:**
  - **On Center OST + Quick Bid:** PDF-based. No Revit input found.
  - **On Center Takeoff Boost:** adds AI takeoff.
  - **Togal.AI:** AI drywall "Assemblies."
  - **Kreo.**
  - **Bluebeam:** has a Revit plugin.
- **Coordination:** Navisworks + ACC Model Coordination, Revizto.
- **Field layout:**
  - **Trimble Field Points:** Revit plugin.
  - **HP SitePrint.**
  - **Dusty FieldPrinter:** has a Revit plug-in.

### Open source and free precedent

- **pyRevit** (and EF-Tools, pyRevitMEP); Dynamo packages Clockwork, Rhythm and
  Data-Shapes.
- **RevitLookup** (MIT, current to 2027): essential for exploring the API.
- **No wall & ceiling-specific open-source takeoff or audit tool found.**

### Starter templates and learning

- **Nice3point RevitTemplates** (best fit): v6.x targets .NET 10 / Revit 2027 and
  includes multi-version builds, MVVM, DI, installer and GitHub CI.
  https://github.com/Nice3point/RevitTemplates
- **ricaun.Revit.Templates and ricaun.RevitTest:** NUnit tests that run inside Revit.
- **Docs:**
  - revitapidocs.com (through 2027)
  - rvtdocs.com (API diffs between versions)
  - The Building Coder (Jeremy Tammik retired; Autodesk's developer team now runs it)

### AI ↔ Revit

- **Autodesk Revit MCP Server, read tools** (tech preview, Revit 2027.2+). 7 tools:
  `get_running_revit_instances`, `query_model`, `get_element_data`, `open_view`,
  `export_views` (schedule → CSV), `select_elements`, `zoom_to_elements`.
  https://blog.autodesk.io/revit-2027-mcp-server-release/
- **Autodesk Assistant in Revit 2027:** a tech preview that can modify the model.
  **AU 2026** previewed agentic Assistant, Assistant Builder and AI Orchestrator, for
  2027.
- **Community MCP servers:**
  - **mcp-servers-for-revit:** MIT, ~380★, Revit 2020–2026, 26 tools including "send C#
    code." https://github.com/mcp-servers-for-revit/mcp-servers-for-revit
  - pyRevit-Routes-based servers, revit-claude-mcp, and LuDattilo/revit-mcp-server
    (CSV export).

### Revit version and API reality (late 2026)

- **Revit 2027 → .NET 10.** Revit 2025/2026 → .NET 8, with Autodesk migrating them to
  .NET 10 (.NET 8 support ends Nov 10, 2026). Revit ≤2024 → .NET Framework 4.8.
- **[I] Recommended target:** Revit 2025–2027 via Nice3point multi-config. Add a 4.8
  build for 2024 only if attendees need it.
- **Open question:** which Revit versions are the attending firms on?

---

## 6. Gaps a free reference plugin could fill [I]

- **Trade-scoped quantity takeoff from the model:** wall types with layers, board area
  per side, net area after openings, ceiling area by type, grouped by level, exported
  to Excel or CSV in an estimator-friendly shape.
- **"Is this model usable for my trade?" audit:** flag generic or unassigned wall types,
  missing fire/STC/UL data, walls not reaching the deck, ceilings without heights,
  in-place families.
- **A bridge to tools they already use:** CSV shaped like OST/Quick Bid line items.
- **Agent-friendly structured output:** wall & ceiling JSON that an AI agent (or
  Autodesk's MCP server) can consume, where Autodesk's tools are generic.
- **Current on Revit 2025–2027.**

## 7. Candidate reference plugins

| Candidate | Pain addressed | Fit for workshop | Risk |
|---|---|---|---|
| **A. Wall & Ceiling Model Audit + Takeoff Export** (read-only) | #1, #3, #4 | Works on the architect model *every* sub receives. Safe (read-only). Value is visible to estimators, BIM staff and managers. Endless extension prompts ("add soffits," "group by rating," "diff two versions") | Least novel-sounding. Must be shaped like real takeoffs to impress |
| **B. Layout export for robotic layout** | #5 (SWACCA #1) | Hits the summit's stated top pain | SitePrint/Dusty/Trimble already ship Revit plugins. Needs hardware to demo |
| **C. Clash XML triage** (Navisworks XML → tag/color Revit walls) | #2 | Strong for coordinators | Navisworks dependency. Narrower audience |
| **D. Prefab panel tickets** | prefab | Mirrors Raymond's real practice | Needs framed models (MWF etc.). Narrow |
| **E. Revision delta report** (quantities by wall type between two versions) | #6 | Very relatable (change orders) | Best as an *extension* of A |

---

## 8. Can an MCP connection support Revit 2024 and earlier? (checked 2026-10-15)

**Short answer: yes.** It's been done in the open, and the architecture makes it
straightforward.

- **Why it's possible:** an AI ↔ Revit MCP setup has two halves.
  1. **The MCP server** runs *outside* Revit as a normal process (Node, Python or .NET).
     The AI client (Claude Desktop, Claude Code, Codex, Cursor) talks to it over stdio.
     It doesn't care which Revit version is running.
  2. **A thin add-in inside Revit** listens on a local port (WebSocket or HTTP) and
     runs the requests through the Revit API. Only this half is version-specific.
- **Proof it works on older versions:** `mcp-servers-for-revit` (MIT) supports **Revit
  2020–2026**. Its add-in builds on **.NET Framework 4.8 for 2020–2024** and **.NET 8
  for 2025–2026**, from one solution with per-version build configurations (R20…R26)
  and a release ZIP per version. Its server is TypeScript/Node over stdio and talks to
  the add-in over WebSocket. [R]
  https://github.com/mcp-servers-for-revit/mcp-servers-for-revit
- **pyRevit route:** `revit-mcp-python` uses pyRevit's Routes HTTP server
  (`localhost:48884`) plus a Python FastMCP server. It isn't tied to one Revit
  version, but the author calls it "more of a demonstration," and the Routes API is
  still "in draft form." [R] https://github.com/tillbaum/revit-mcp-python
- **Autodesk's official MCP server needs Revit 2027.2+.** That rules it out for most
  of this audience, which is why a 2024-compatible version is the differentiator.
- **Build targets we'd need:**

  | Revit | Runtime |
  |---|---|
  | ≤2024 | .NET Framework 4.8 |
  | 2025–2026 | .NET 8 (migrating to .NET 10) |
  | 2027 | .NET 10 |

  One solution with multiple build configurations handles this; it's a known pattern.
- **Constraint to design around:** the Revit API only accepts calls on Revit's main
  thread. The add-in must queue incoming requests and run them via `ExternalEvent`.
  This is the classic gotcha and worth teaching. [I]
- **Not yet verified:** whether the current Nice3point RevitTemplates (v6, .NET 10)
  still scaffold a net48 / Revit 2024 configuration, or whether we'd need an older
  template version or a hand-rolled multi-target setup.

Sources from the three research reports are cited inline above. Raw agent reports are
not preserved; re-run the research if deeper sourcing is needed.
