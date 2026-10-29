# Agent integration surfaces in wall-and-ceiling tools

**Question and date:** what can an AI agent (a coding agent, an MCP server, a Dynamo or pyRevit script) actually connect to in the tools wall-and-ceiling contractors use? Observed on October 28, 2026, the evening before the SWACCA seminar, after attendees from two member firms (a large interior systems contractor and a prefab operation) named StrucSoft MWF, AGACAD framing tools, HP SitePrint and Dynamo.

**Method:** web research only. Four parallel research passes covered StrucSoft, AGACAD/ARKANCE, HP SitePrint and the Revit/Dynamo platform layer; the most consequential claims were then re-read on their primary pages. Nothing was downloaded, installed or run. Several vendor pages blocked automated reading (HTTP 403) or have moved; where a point rests on a search excerpt rather than the page itself, it says so.

**Labels:** **Documented** (vendor or official page), **Reported** (third party or search excerpt), **Not found**, **Inference** (our reasoning, not a vendor statement).

## Summary

None of the three vendor products publishes an API, SDK or MCP server for third parties. Each one still leaves something an agent can use today:

| Product | Public API | What an agent can already reach | Vendor AI or MCP |
|---|---|---|---|
| StrucSoft MWF (Graitec) | Not found | Native Revit framing families and parameters; free Dynamo scripts; BTLx and Howick CSV machine files | Graitec Assistant (chat) in the Revit 2027 release |
| AGACAD Metal Framing (ARKANCE Be.Smart) | Not found | Native Revit framing families, Parts and Assemblies; XML rule and CNC configuration files; agent-readable docs | Not found for framing |
| HP SitePrint | Not found | The 2D DXF input (print elements, obstacles, control points) | Not found for outside agents |
| Dynamo | Not applicable | `.dyn` graphs are JSON; DynamoCLI runs graphs without the interface | DynamoMCP with Autodesk Assistant (Dynamo 4.2 Sandbox) |

**Inference:** because the framing tools write ordinary Revit elements and the other tools read or write text files, a read-only Revit MCP server and file-checking tools can reach all four without vendor permission. Regenerating framing or driving the vendors' own commands still needs the vendors' tools.

## StrucSoft MWF (Strucsoft Metal, Wood, Metal+)

- **Product and versions — Documented.** StrucSoft (Quebec, part of Graitec) frames light-gauge steel and wood inside Revit and produces shop drawings, cut lists and machine output. The Revit 2026 line is version 2.43 (builds 9370–9613, with the Panel Checker validating panels before shop drawings or export; [release notes, 2025-09-03](https://strucsoftsolutions.com/news/whats-new-in-strucsoft-framing-software-for-revit-2026/)). Revit 2027 support was announced on [2026-09-09](https://strucsoftsolutions.com/news/new-release-strucsoft-for-revit-2027-01/), which calls the release both 2027.0.0 and 2026.3.
- **API — Not found.** The vendor offers paid custom programming services instead. Graitec's new Open API (June 2026) is for a different product, Advance Design.
- **Dynamo — Documented.** Free `.dyn` downloads such as unjoining framing and copying or clearing Panel IDs ([free apps page](https://strucsoftsolutions.com/free-revit-apps-dynamo-scripts/)). **Inference:** they use stock Dynamo nodes on ordinary Revit parameters; no MWF node package was found.
- **Data in the model — Documented and Reported.** Members are native Revit framing family types; release notes name parameters such as Panel ID, Group ID, Framing ID and `BIMSF_Container`. Vendor guidance seen only in a search excerpt says items prefixed `MWF_`, `BIMSF_` or `Panel.Params` are plugin-generated. **Inference:** a generic Revit reader sees members and parameters but not MWF's templates or rules.
- **Machine output — Documented and Reported.** [Onyx](https://strucsoftsolutions.com/onyx/) is StrucSoft's separate cloud CAM tool; no Onyx API was found. Output formats include BTLx, an open format with [published schemas](https://design2machine.com/btlx/schema.html) (v2.3.1, 2025-07-08), and Howick CSV, for which no public specification was found.
- **AI — Documented (marketing).** The 2027 release adds Graitec Assistant: it builds panel templates from wall schedules and searches, regenerates or renumbers panels by chat. Its licensing FAQ answer was a placeholder on the observation date. No MCP or agent API was found.

## AGACAD Metal Framing (ARKANCE Be.Smart)

- **Product and versions — Documented, with conflicts.** AGACAD's Metal Collection frames light-gauge steel walls, floors, roofs and trusses, splits sheathing and drywall into panels, numbers parts and produces assembly shop drawings ([Metal Collection](https://agacad.com/products/metal-collection); agacad.com pages now redirect to [ARKANCE](https://arkance.world/us-en/products/be-smart/building/metal-framing)). An AGACAD announcement lists Revit 2026 support; other pages list up to 2025, and a Revit 2027 launcher exists. Metal Framing on Revit 2027 is not confirmed.
- **API — Not found.** The [documentation index](https://docs.besmart.software/llms.txt) has no API or developer section. An old blog post shows only launching an add-in command from Dynamo by its command ID (Reported).
- **Configuration — Documented.** Framing, sheathing, paneling, numbering and drawing rules are XML files, one folder per rule set, under the user's application data, per Revit year. Cloud folders are not supported and shared server folders are discouraged. No schema is published.
- **CNC export — Documented.** The CNC Metal Exporter writes Howick (CSV 1.0, CSV2, 4200), Scottsdale ScotSim, Arkitech, AMS/Royal, Knudson, Pinnacle and Metroll formats; each machine setup is an editable XML file.
- **Data in the model — Documented.** Studs and tracks are Structural Framing families named `MF …` with type parameters such as `FM Gauge`; numbering writes `FM SortMark`, `CNC Part Name` and `CNC Number`; sheathing and drywall are native Revit Parts; shop drawings use Assemblies.
- **AI — Not found for framing.** ARKANCE resells a separate natural-language Revit tool (BIMLOGIQ Copilot, announced December 2025; Reported). The documentation site publishes `llms.txt`, which makes it easy for agents to read.

## HP SitePrint

- **Input — Documented.** The robot prints lines, arcs, text and points from a **2D DXF** that holds the print elements, known obstacles and control points for the robotic total station ([FAQ](https://www.hp.com/us-en/printers/site-print/layout-robot/faq.html)). The current guide groups layers into print, obstacles, control and others, and imports control points as CSV (Reported from search excerpts of HP's guide; check against the full guide).
- **Plug-ins — Reported.** An AutoCAD plug-in sends files to the SitePrint cloud. HP announced a Revit plug-in in 2024 ([Value Pack 2.0](https://www.forconstructionpros.com/construction-technology/site-layout/product/22892444/hp-inc-hp-unveils-hp-siteprint-value-pack-20-upgrade)); a later [AEC Magazine article](https://aecmag.com/collaboration/hp-build-workspace-connecting-site-with-office/) and a dealer describe one, but no official listing or supported Revit years were found.
- **API — Not found.** No public API, SDK or partner program for SitePrint Cloud or HP Build Workspace.
- **Recent features — Reported.** A browser-based layout editor and improved obstacle navigation ([AEC Magazine, 2026-09-15](https://aecmag.com/construction/hp-improves-automation-in-siteprint-and-build-workspace)); AI vectorisation of drawings in Build Workspace.
- **Competitors — Documented or claimed.** Dusty Robotics publishes an [AutoCAD plug-in](https://marketplace.autodesk.com/apps/9250b580-7af6-4f64-b3b5-fafd0d12cac8) and claims a native Revit plug-in; no public API was found.

## Revit and Dynamo platform layer

- **Autodesk Revit MCP server — Documented.** A Technical Preview requiring Revit 2027 with the 2027.2 Update, in Read, Write and Experimental installers, running locally over stdio; the installer configures Claude Desktop ([Autodesk, 2026-09-29](https://blog.autodesk.io/revit-mcp-server-update)). The first preview (April 2026) was read-only ([release post](https://blog.autodesk.io/revit-2027-mcp-server-release)). It does not run on Revit 2024–2026.
- **Autodesk Platform Services — Documented.** Revit, Product Help and Fusion MCP servers in tech preview; open-source MCP samples only for ACC and the AEC Data Model ([APS DevCon 2026](https://aps.autodesk.com/blog/building-agentic-ai-whats-new-autodesk-platform-services)).
- **Dynamo — Documented.** Dynamo 4.2 builds Autodesk Assistant and DynamoMCP into the Sandbox; it needs an Autodesk sign-in, does not work offline, and cannot change Revit documents directly ([release post, 2026-08-26](https://dynamobim.org/dynamo-core-4-2-release)). Connecting outside clients is not documented. `.dyn` files have been JSON since Dynamo 2.0, and DynamoCLI runs graphs without the interface, though Revit nodes still need Revit.
- **pyRevit Agent Runtime — Documented.** An experimental, off-by-default runtime that serves MCP over stdio and runs read-only or approved Python in Revit; validated on Revit 2024 and 2025 ([docs](https://docs.pyrevitlabs.io/agent-runtime)). Its documentation notes that model data goes to the agent's model provider and is stored locally for up to 14 days.
- **Community Revit MCP servers — Documented.** [bimwright/rvt-mcp](https://github.com/bimwright/rvt-mcp) (Apache-2.0, Revit 2022–2027, read-only mode) and [mcp-servers-for-revit](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit) (MIT, Revit 2020–2026). See the earlier [bridge comparison](revit-mcp-bridge-comparison.md).

## Potential role and decision

- **Role:** precedents and integration targets, not dependencies. No vendor code or package is adopted.
- **Decision:** investigate further through the lab's own server. The [extension roadmap](../explanation/mcp-extension-roadmap.md) turns these surfaces into planned tools.

## Outstanding checks

- Read HP's full SitePrint guide for layer, control-point and clearance rules before building a pre-flight check.
- Confirm Metal Framing on Revit 2027 and Strucsoft's supported installer range.
- Inspect a framed sample model (MWF and AGACAD) with a read-only reader: categories, families, parameters, Assemblies, Parts, and extensible-storage schemas with their access levels.
- Ask StrucSoft, ARKANCE and HP which parameter names, schemas and file formats they consider stable for third parties, and whether APIs exist under agreement.
