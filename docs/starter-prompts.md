# Starter prompts for wall-and-ceiling tools

These are **suggested prompts** for starting your own tool with a coding agent such as Claude Code or Codex. They are written for wall-and-ceiling contractors who use tools such as StrucSoft MWF, AGACAD Metal Framing, HP SitePrint and Dynamo. They are not quotations from the original build. Fill in the brackets for your firm.

They follow the [extension roadmap](explanation/mcp-extension-roadmap.md), which explains how the lab's Revit MCP server grows stage by stage, and the [integration research](research/wall-ceiling-tool-integrations-2026-10-28.md), which records what each of those tools exposes to agents. Start with the [participant guide](start-here.md) if you have not used the repository before.

## The moves every prompt makes

1. **Point the agent at the context:** the repository, its AGENTS.md, and the relevant roadmap stage.
2. **State the BIM purpose and the handoff:** what decision or downstream tool the answer is for.
3. **Research before code:** current versions, vendor documentation and file formats, with sources.
4. **Expected answers before code:** passing, failing and excluded sample cases.
5. **Read-only first, on a disposable copy:** exports and model changes are separate, explicit steps.
6. **Check against something independent:** a native Revit schedule, the source file, or a hand count.
7. **Record it:** a Git checkpoint and, for the lab, both teaching layers.

Use disposable copies of models and files. Vendor parameter names and file formats are not published as stable interfaces, so ask the agent to confirm them in your own model or files before relying on them.

## 0 · Get oriented (anyone)

Use when you are deciding where to start.

> Clone https://github.com/bringfire/bim-agent-lab. Read README.md, AGENTS.md, docs/explanation/mcp-extension-roadmap.md and docs/research/wall-ceiling-tool-integrations-2026-10-28.md. We're a wall-and-ceiling contractor using [MWF / AGACAD / SitePrint / Dynamo] on Revit [year]. Explain which roadmap stage fits our work, what already exists, and the smallest first tool we could build. Don't change anything yet.

## 1 · Our Revit year (roadmap stage 1)

Use when your firm runs a Revit version the lab has not verified yet.

> We're on Revit [2025]. Read the roadmap's stage 1 and the existing add-in. Check my machine's Revit version, API libraries and .NET SDK, then propose how to build the readiness add-in for our year, explaining what changes between .NET Framework 4.8 and the .NET version our Revit release uses (.NET 8 for 2025 and early 2026 releases; .NET 10 for 2026.5 and later). Follow how the repository already builds for 2024 and 2026.5. Build it, and verify the prepared sample cases in Revit before calling it supported.

## 2 · "Ready for framing" (stage 2)

Use before a model goes to a framing add-in.

> Using the existing readiness check as the pattern, add a "ready for framing" rule: before walls go to [MWF / AGACAD], every wall type in scope must have [our framing fields, e.g. stud gauge, stud spacing, framing configuration]. Read the existing checklist and evaluator first. Propose the rule, valid exceptions, and sample cases (complete, missing field, excluded type) with expected answers before writing code. Keep it read-only and check it against a native Revit schedule.

The same pattern covers any other type-information rule, for example a required STC rating on rated partitions. This is the general version from the [readiness lesson](tutorials/03-check-required-type-information.md):

> Read the existing contract and verification evidence. We need to check [condition] for [defined scope], using [requirement source]. Make the requirements and valid exceptions explicit. Prepare sample copies with independently known passing, failing, and excluded cases. Implement the check, verify values and affected identities against native BIM evidence, and show what the result does and does not prove. Record both learning layers and a reviewed Git checkpoint.

## 3 · Inventory MWF framing (stage 3)

Use when you want to check what StrucSoft MWF produced.

> On a disposable copy of a model framed with StrucSoft MWF, without changing it, list framing members by panel (for example a Panel ID parameter) with family, type and length, and report what you couldn't read. Confirm the actual parameter names in my model before relying on them, and keep them in a small profile file, not in code. Compare your counts with a schedule I'll export.

## 4 · QA AGACAD framing (stage 3)

Use when you want to check what AGACAD Metal Framing produced.

> Check stud spacing on AGACAD-framed walls in this sample model, read-only. Group the Structural Framing members (for example families starting "MF") by wall, and flag walls where spacing exceeds [16 in] or members lack [FM Gauge]. Tell me how you matched members to walls, and what that method could miss.

## 5 · SitePrint pre-flight (stage 4, outside Revit)

Use before layout files go to the robot.

> Write a pre-flight checker for HP SitePrint layout files. First find HP's current file-preparation guidance and summarise its rules with sources; don't assume layer names. Then check a DXF against them: print elements on the right layers, obstacles closed, at least two named control points. Make sample files that should pass and fail, and build it as a command-line tool we could later expose through our MCP server.

## 6 · Compare CNC exports (stage 4)

Use when fabrication files change between model revisions.

> Compare two Howick CSV exports of the same panel from different model revisions: which members were added, removed or changed length, and do the totals match the model's framing schedule? Don't guess the format. Infer it from the samples, show your assumptions, and stop if something doesn't fit.

For BTLx exports, ask instead for validation against the published BTLx schema, with the schema version and source recorded.

## 7 · For Dynamo users

Use when you already have useful graphs.

> Here are our Dynamo graphs (.dyn). Put them in a Git repository, then explain what each one reads or changes in the model, which inputs it expects, and which could become a read-only tool in our MCP server. Pick one and plan that conversion with expected results, but don't change the graphs.

## 8 · Ask the vendor the right questions

Use before building on a vendor's undocumented names or formats.

> Draft questions for [StrucSoft / ARKANCE / HP] about building agent tools on their product: which parameter names and file formats are stable for third parties, whether an API is available under agreement, and how [Onyx exports / CNC setups / SitePrint plug-in files] are specified. Base them on what our research note found and didn't find.

## 9 · A new idea or the Summit Challenge

Use for anything else.

> Our problem is [X]. Before building anything, research what already exists for it in Revit, Dynamo and the tools our firms use. Propose the smallest read-only check that would help, with an answer we can verify by hand, and tell me what you'd need from me.
