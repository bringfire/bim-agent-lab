# Research and resources

The [participant research story](../tutorials/00-research-before-building.md) explains how the author requested this work; the [agent research guide](../how-to/research-existing-solutions.md) gives the technical procedure.

Research precedes selecting a stack or implementing a tool. Evaluate the BIM fit, inspect source, record dependencies and terms, and test the smallest uncertain assumption.

- [MCP bridge comparison](revit-mcp-bridge-comparison.md): current protocol/SDK releases, existing bridge migration work, and a minimal custom alternative; [source/version evidence](evidence/mcp-version-observations-2026-10-20.json).
- [Desktop protocol investigation](mcp-desktop-protocol-investigation.md): update checks, an experimental Codex setting, and the distinction between SDK compatibility and our strict acceptance policy.
- [Protocol tradeoffs and alternative clients](mcp-protocol-tradeoffs-and-agent-clients.md): what older exchanges would cost, cross-language support, and an actual Claude Code 2.1.305 pass on `2026-07-28`.
- [ClosedXML dependency review](closedxml-dependency.md): the first adopted package, full resolved graph, licenses, embedded font, and runtime findings.
- [Revit resource register](revit-resources.md): SDK/API sources, local observations, candidate roles, and open compatibility questions.
- [Reference code map](reference-code-map.md): useful source entry points linked to inspected commits.
- [Repository manifest](repositories.json): exact sources, commits, and relative checkout paths.
- [Clone guide](../how-to/clone-research-repositories.md): reproduce the source-inspection setup.
- [Evidence snapshots](evidence/README.md): original license/manifests and checkout observations.
- [Earlier domain research](archive/wall-and-ceiling-domain-research.md): historical wall and ceiling workflow research, with later qualifications called out.

Use the [research record](../../templates/research-record.md) for a new candidate or a version update. Record why it is a development tool, a dependency, adapted code, or a precedent. State what has actually been tested.

[MCP adopted dependency review](mcp-sdk-dependencies.md) records the actual 12-package graph, helper adaptation, licenses and current registry check. The [desktop compatibility checkpoint](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) follows the comparison with observed failures in both installed clients.
