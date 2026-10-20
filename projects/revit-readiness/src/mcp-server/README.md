# MCP server

Planned responsibility: expose named, scoped model-readiness operations to an AI client and return structured results with evidence, completeness, units, and useful errors.

The [2026-10-20 comparison](../../../../docs/research/revit-mcp-bridge-comparison.md) recommends a separate C# MCP server using the official `ModelContextProtocol` 2.2.0 package, with a minimal original net48 Revit bridge around the existing reader and evaluator. Target the current protocol revision, 2026-07-28 at that observation, and test the actual client's supported exchange. Recheck versions before adopting the candidate pin.

This folder contains no executable server yet. No SDK has been added, bridge installed, or MCP request tested in Revit. Follow the [agent guide](../../../../docs/how-to/choose-and-verify-an-mcp-bridge.md) for source boundaries, version checks, local IPC/ExternalEvent design, and acceptance gates. The [participant story](../../../../docs/tutorials/05-research-and-choose-the-mcp-bridge.md) explains how the author directed the comparison and current-version research.
