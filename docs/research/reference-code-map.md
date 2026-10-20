# Reference code map

Date: 2026-10-20
Purpose: first source inspection of the seven research candidates. The original checkouts were clean at inspection; no candidate had been built or installed. Source links point to the inspected upstream commits. Local paths are relative to the repository root after following the [clone guide](../how-to/clone-research-repositories.md).

The [checkout manifest](evidence/local-checkouts-2026-10-20.json) records origins, full commit IDs, evidence-file comparisons, and submodule status. The [resource register](revit-resources.md) records licenses and dependencies.

| Repository | Local checkout | Entry point for this project |
|---|---|---|
| mcp-servers-for-revit | `references/mcp-servers-for-revit` | [Current-view MCP tool](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/tools/get_current_view_elements.ts), [C# command](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/commandset/Commands/Access/GetCurrentViewElementsCommand.cs), [Revit handler](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/commandset/Services/GetCurrentViewElementsEventHandler.cs) |
| Nice3point RevitTemplates | `references/RevitTemplates` | [Version and target-framework selection](https://github.com/Nice3point/RevitTemplates/blob/d703757a01e2cde2eb5392cd59666e6b0e578cf6/source/Nice3point.Revit.Sdk/Sdk/Nice3point.Revit.Common.props) |
| RevitLookup | `references/RevitLookup` | [Repository map and architecture](https://github.com/lookup-foundation/RevitLookup/blob/766aa41a3821b64befc0b81e3c6b164725b26e87/AGENTS.md), [selection command entry point](https://github.com/lookup-foundation/RevitLookup/blob/766aa41a3821b64befc0b81e3c6b164725b26e87/source/RevitLookup/Commands/DecomposeSelectionCommand.cs) |
| Python MCP SDK | `references/python-sdk` | [Small resource registration example](https://github.com/modelcontextprotocol/python-sdk/blob/2af9e3089ac66b35c1550c500aae86a82efb1e17/examples/stories/resources/server.py), [version and dependencies](https://github.com/modelcontextprotocol/python-sdk/blob/2af9e3089ac66b35c1550c500aae86a82efb1e17/pyproject.toml) |
| C# MCP SDK | `references/csharp-sdk` | [Small stdio server example](https://github.com/modelcontextprotocol/csharp-sdk/blob/3d8af7f98a49f55e0ac70e4fcd42df76fe1d1410/samples/QuickstartWeatherServer/Program.cs) |
| pyRevit | `references/pyRevit` | [Routes request queue and ExternalEvent handler](https://github.com/pyrevitlabs/pyRevit/blob/fc524b132f13d2d7cb4d085976efd9a1431d469b/pyrevitlib/pyrevit/routes/server/handler.py) |
| Python Revit MCP precedent | `references/revit-mcp-python` | [MCP view tools](https://github.com/tillbaum/revit-mcp-python/blob/9cc3f195b0e79f511e4ed8bce729402dd659644e/tools/view_tools.py), [Revit view routes](https://github.com/tillbaum/revit-mcp-python/blob/9cc3f195b0e79f511e4ed8bce729402dd659644e/revit_mcp/views.py) |

## Findings from the initial source pass

- The existing bridge's TypeScript tool sends a named command with filters and a default limit of 100. The C# command sets handler parameters and calls `RaiseAndWaitForCompletion(60000)`. This gives us concrete entry points for tracing the round trip; full execution and timeout behavior still need evaluation.
- A current-view query and a bounded result set need explicit treatment in a readiness report. We must establish whether the requested scope was fully examined and prevent a partial inventory from being presented as a complete total.
- Nice3point's SDK properties select `net48` for Revit 2024. This strengthens the earlier finding that its application template includes an R24 configuration; we have now read the corresponding framework-selection logic.
- The pyRevit Routes handler contains a bounded request queue, cancellation handling, and an `IExternalEventHandler`. These are useful concepts to study when designing the bridge. Copying code remains subject to the license decision.
- The Python precedent maps MCP view tools to HTTP routes. Its current-view route uses `FilteredElementCollector(doc, current_view.Id)` and `IntegerValue`, so both scope and cross-version element-ID handling require attention.
- The Python SDK example uses `MCPServer`, while the older Python Revit precedent uses the earlier FastMCP import path. Their versions need to be aligned before trying to combine them.
- The C# SDK example registers tools with stdio transport and directs console logging to standard error. It provides a small alternative server example for comparison.
- RevitLookup's repository map identifies its decomposition engine and UI as submodules. Those directories are not populated in this inspection checkout. pyRevit also declares eight submodules.

## Next source questions

Trace document selection and request dispatch in the existing bridge, inspect the actual `RevitMCPSDK` dependency used by its command base, and determine how timeouts and incomplete results are represented. Then compare the smallest useful build with the custom bridge option.

These source observations support the comparison. They do not establish runtime correctness or constitute a full review of the seven projects.
