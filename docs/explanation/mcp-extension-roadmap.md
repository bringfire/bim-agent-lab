# One base, extended tool by tool: the MCP server roadmap

The lab's Revit MCP server is meant to be a base that firms extend for their own work, on the Revit versions they actually run. Autodesk's official Revit MCP server is a Technical Preview that needs Revit 2027 with the 2027.2 Update; most wall-and-ceiling firms are on earlier versions and should not need to upgrade to let an agent check their models. This page explains how the existing server grows to reach framing, layout and fabrication tools, and what each step must prove.

Everything past stage 0 is a **plan**, not supported behaviour. Supported versions and verified results are recorded in the [development status](../reference/development-status.md) and the project's verification records.

## Why the current design can grow

The readiness server already separates the parts an extension needs ([source layout](../../projects/revit-readiness/src/README.md)):

| Part | Folder | Runs | Role |
|---|---|---|---|
| MCP server | `projects/revit-readiness/src/mcp-server` | Separate process, .NET 10, official C# MCP SDK | Defines the tools agents see and validates their arguments |
| Bridge | `bridge-contract`, `bridge-runtime` | Both sides of a local Windows pipe | Bounded requests, cancellation, and context tokens tied to the open model |
| Revit add-in | `revit-addin` | Inside Revit | Reads the model on Revit's thread through an external event |
| Rules | `readiness-core` | Inside Revit, no Revit API calls | Applies a written checklist and reports findings and coverage |

That gives two kinds of tools:

- **Model tools** go through the bridge into Revit. A new one needs a reader in the add-in, rules in the core, and a tool definition in the server.
- **File tools** run entirely in the MCP server and never touch Revit: they read, check or compare files such as DXF layouts, CNC exports or Dynamo graphs.

## Rules every new tool keeps

1. **A fresh context bound to the open model.** Results about a model that is no longer open are rejected, as today.
2. **Read-only by default.** Exports and model changes are separate, explicit and recorded.
3. **Say what was not examined.** Every result reports requested and examined counts and whether evaluation was complete.
4. **Independent expected answers first.** Prepared passing, failing and incomplete cases, checked against native Revit evidence or the source file.
5. **Focused tools, no general code execution.** Unlike some community servers, the base does not offer a "run any script" tool; each capability is a named, testable tool.
6. **Vendor-specific names live in data, not code.** Parameter names and family patterns for a particular add-in go in a profile file that a firm can edit.
7. **A lesson in both layers.** A participant story and a technical guide, linked to the same verification record and commit.

## Stages

| Stage | What it adds | Kind | Serves |
|---|---|---|---|
| **0 · Today** | Required wall and ceiling type information, read-only, verified on Revit 2024 build 24.3.60.12 | Model | Everyone |
| **1 · The versions firms run** | Build the add-in for each Revit year: .NET Framework 4.8 for 2022–2024, .NET 8 for 2025–2026, and .NET 10 for 2027 (reported). The same MCP server serves all of them | Platform | No forced upgrade |
| **2 · More readiness rules** | "Ready for framing" and "ready for layout" checks before the model goes to a framing add-in or a layout robot, using the same checklist pattern | Model | Everyone |
| **3 · Read framing output** | A framing reader independent of the vendor: Structural Framing members, Parts, Assemblies and parameters, with StrucSoft and AGACAD naming in profile files. Enables framing QA such as stud counts and spacing, missing fields and numbering | Model | StrucSoft MWF and AGACAD users |
| **4 · File checks** | SitePrint DXF pre-flight; CNC file checks (Howick CSV comparisons, BTLx against its public schema); Dynamo graph review; vendor configuration comparisons | File | SitePrint, fabrication, Dynamo users |
| **5 · Exports on request** | SitePrint-ready DXF per level with mapped layers and a control-point CSV. Explicit, approved and recorded, like the existing Excel export | Model to file | SitePrint users |
| **6 · Model changes, last** | For example, fill missing Type Marks from a schedule, only with approval in Revit, a single undo step and recovery evidence | Model, write | Later, if ever by default |

The order matters: each stage reuses what the previous one proved. Stage 1 comes first because a tool that only runs on one Revit year does not help most firms. Writes come last because they need the most evidence.

## What each stage must prove

- **Stage 1:** the same readiness results on each Revit year as on 2024, from the same prepared samples, with installation and recovery verified per year. Build success alone is not support.
- **Stages 2 and 3:** prepared sample models with known framing or layout defects and valid controls, checked against native schedules. For stage 3, sample models framed with each vendor's tool, and a record of which elements and parameters a generic reader can and cannot see.
- **Stage 4:** sample files with deliberate defects, and the vendor's documented requirements cited for every rule. HP's full SitePrint guide must be read before writing its rules.
- **Stage 5:** an exported file that the vendor's own software accepts, plus proof that the model was not changed.
- **Stage 6:** the [recoverable installation](recoverable-readiness-installation.md) standard applied to model edits: approval, undo, and evidence that a rejected or interrupted change leaves the model as it was.

## Working alongside other servers

- **Autodesk's Revit MCP server** (Revit 2027.2 and later, Technical Preview) offers general model access. On those versions both can be installed; the base keeps providing wall-and-ceiling checks with evidence, and it keeps serving earlier years.
- **Community servers** such as `rvt-mcp` and `mcp-servers-for-revit` cover broad, general tools across many Revit years. They are precedents to learn from; see the [bridge comparison](../research/revit-mcp-bridge-comparison.md).
- **Vendor AI** such as Graitec Assistant works inside one product. The base complements it by checking what goes into and comes out of that product.

## What would make this easier

Firms can ask their vendors for stable parameter names, configuration schemas and file-format specifications, and whether an API is available under agreement. Those are what make the stage 3 profiles and stage 4 checks dependable. The [integration research](../research/wall-ceiling-tool-integrations-2026-10-28.md) records what each vendor publishes today.

## How to start a stage

The [starter prompts](../starter-prompts.md) give a suggested opening prompt for each stage. Then follow the [extension workflow](../how-to/extend-a-project.md): write a feature brief, research the missing API or file format, prepare independent examples, implement one complete tool, verify it in the real application, and record the lesson.
