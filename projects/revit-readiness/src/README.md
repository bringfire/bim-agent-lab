# Implementation boundaries

The first read-only Revit command is implemented under `revit-addin`. The readiness engine, communication bridge, and external MCP server are still to be implemented.

- [readiness-core](readiness-core/README.md): rules, findings, and quantity classification.
- [revit-addin](revit-addin/README.md): host API access and commands.
- [mcp-server](mcp-server/README.md): agent-facing tool interface.

Keep the three demonstration questions as the initial product scope. The [first-build tutorial](../../../docs/tutorials/02-build-a-read-only-revit-command.md) documents the smaller setup proof, SDK pin, and repeatable commands.
