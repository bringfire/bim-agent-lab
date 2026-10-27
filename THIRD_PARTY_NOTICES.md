# Third-party research and notices

This baseline contains research, teaching material, and source-inspection evidence. It does not include the seven upstream working trees. The Revit Excel exporter now adopts ClosedXML and its resolved NuGet dependencies.

The [repository evidence snapshot](docs/research/evidence/repository-evidence-2026-10-20.json) preserves upstream license texts and selected dependency manifests with original URLs, inspected commits, and SHA-256 hashes. Those embedded materials retain their upstream notices and terms. The [resource register](docs/research/revit-resources.md) describes the different observed licenses; no blanket license is asserted over all research candidates.

Autodesk SDKs, API binaries, and sample models are linked as resources and are not bundled. The archived workshop HTML references Google Fonts over the network; the font files are not vendored.

The first adopted dependency is documented in the [ClosedXML review](docs/research/closedxml-dependency.md). The exporter pins ClosedXML 0.105.1; [retained notices and metadata](projects/revit-readiness/third-party/README.md) cover all twelve resolved packages and the embedded Carlito font. Installation ships these notices alongside the exporter. Original upstream source is not modified.

When further source, packages, images, or other assets are adopted, add their source revision, affected local paths, applicable terms, and required notices here. Inspect the chosen dependency versions and transitive packages before release.

Original lab code and teaching material use the root [MIT License](LICENSE), selected by the owner on October 26, 2026. That grant does not replace the terms of the upstream materials described here. See the [licensing guide](docs/reference/licensing.md).

The MCP host now consumes official ModelContextProtocol 2.2.0 and its locked dependencies. [Dependency review](docs/research/mcp-sdk-dependencies.md) records package/source provenance. [Retained MCP material](projects/revit-readiness/third-party/mcp/README.md) includes SDK source LICENSE/THIRD-PARTY-NOTICES, Microsoft license texts and package notices. `CancellableInputStream.cs` adapts the official C# 2.2.0 stdin helper with documented modifications. TypeScript SDK 1.26.0 remains precedent only. The future MCP installer must carry these notices; no MCP distribution package exists yet.
