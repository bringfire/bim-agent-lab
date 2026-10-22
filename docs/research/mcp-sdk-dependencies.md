# MCP SDK dependency and source review

Observed 2026-10-20 (America/New_York). The approved plan selects official **ModelContextProtocol 2.2.0** for the external net10.0 host. Its direct constraint is `[2.2.0]`; committed lockfiles and locked restores retain the complete graph. No new dependency is loaded into Revit/net48. This increment creates a host library and separate synthetic probe/client, not the production Revit connection.

## Current versions and the selection

October 22 cancellation follow-up: the [diagnosis](../../projects/revit-readiness/docs/verification/2026-10-22-sdk-cancellation-diagnosis.md) reproduces a C# SDK 2.2.0 client cancellation-order defect on .NET 10.0.13 across all three protocol revisions. Exact-source inspection of upstream main `2bd715b` and tag v2.3.0 `9a32782` still finds the same registration/WaitAsync pattern. This is not a runtime test of 2.3.0 or a new latest-package audit; no upgrade or license/dependency change was made. Retain the failing propagation test and distinguish it from successful explicit SDK cancellation.

The [NuGet registry](https://api.nuget.org/v3-flatcontainer/modelcontextprotocol/index.json) listed **2.3.0** as latest stable when refreshed at `2026-10-21T01:29:12Z`; the [2.2.0 package page](https://www.nuget.org/packages/ModelContextProtocol/2.2.0) also indicated a newer version. GitHub's releases/latest lookup still resolved to [v2.2.0](https://github.com/modelcontextprotocol/csharp-sdk/releases/tag/v2.2.0). Retain the dated discrepancy and identify which source establishes package availability. Do not call 2.2.0 the latest release.

The author-approved pin remains 2.2.0. Upgrading is a separate dependency decision. The required [protocol revision 2026-07-28](https://modelcontextprotocol.io/specification/2026-07-28) is distinct from either package version. The [desktop experiment](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) shows why neither a new protocol publication nor SDK support establishes desktop compatibility.

## Resolved net10.0 graph

| Package | Version | Role | Package license expression |
|---|---|---|---|
| ModelContextProtocol | 2.2.0 | Direct | Apache-2.0 |
| ModelContextProtocol.Core | 2.2.0 | Transitive | Apache-2.0 |
| Microsoft.Extensions.AI.Abstractions | 10.8.3 | Transitive | MIT |
| Microsoft.Extensions.Caching.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Diagnostics.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.FileProviders.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Logging.Abstractions | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Options | 10.0.10 | Transitive | MIT |
| Microsoft.Extensions.Primitives | 10.0.10 | Transitive | MIT |

The [evidence snapshot](evidence/mcp-package-observations-2026-10-20.json) records package archive SHA-256, lockfile SHA-512 content hashes, package source commits, license expressions, notices and build assets. The host uses `McpServer.Create` and owns its lifetime directly; it does not add the full Microsoft.Extensions.Hosting package. Hosting.Abstractions is transitive.

The inspected `.targets`/`.props` files contain target-framework warning logic and, for Logging.Abstractions and Options, analyzer selection/removal logic. No Exec or custom UsingTask was present in those files. Those two packages contain logging/options source-generator/analyzer assemblies and localized resources. ModelContextProtocol.Core contains `ModelContextProtocol.Analyzers.dll`. Analyzer binaries are compiler-time dependencies; this inspection is not a full binary security audit. No package is placed in the Revit process.

`dotnet list <host.csproj> package --include-transitive --vulnerable --format json` reported no listed vulnerabilities at this observation; retained output is in the snapshot. That is a dated advisory result, not a guarantee. Recheck before distribution or a dependency change.

## What code is used

| Material | Actual use in this increment |
|---|---|
| Official C# SDK 2.2.0 packages | MCP parsing, protocol handling, client/server transport and dispatch |
| C# SDK private stdin helper at `6fa3825973949a9c4f0cd8af344e15a8db09dc35` | Attributed source adaptation in `CancellableInputStream.cs`; private buffers, observed late faults and reuse rejection added |
| Lab host/trace/probe/settings code | Original composition and test support; no hand-written MCP implementation |
| Existing lab readiness reader/evaluator | Reuse remains planned in later tasks; not called by the probe |
| TypeScript SDK 1.26.0 / third-party Revit bridge / RevitMCPSDK | Research precedents only; no copied source or installed dependency here |

The exact upstream helper is in [StdioServerTransport.cs](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/ModelContextProtocol.Core/Server/StdioServerTransport.cs). Canceling its await does not necessarily cancel the underlying Windows read. The adaptation gives that read a private buffer and prevents subsequent reuse, so it cannot later overwrite a caller-owned buffer or append to a closed trace.

## Retained terms and distribution work

[Third-party material](../../projects/revit-readiness/third-party/mcp/README.md) retains the SDK's full LICENSE and THIRD-PARTY-NOTICES, package notices, and Microsoft MIT license texts fetched from the package-recorded source commits. The SDK repository records its Apache-2.0 transition and retained MIT contributions; do not discard those contribution terms merely because NuGet's package expression is Apache-2.0. The modified helper carries source attribution. Preserve license, notice, copyright and modification information with redistributed copies. This records observed terms and provenance; original lab licensing remains the owner's release decision.

There is no distributable MCP installer yet. Task 7 must include these notices and the resolved runtime graph, and verify the actual installed file inventory. Refresh this review if source, package versions, target framework or packaging changes. Teach the human-facing lesson through the [compatibility story](../tutorials/07-test-desktop-compatibility-first.md); reproduce it using the [agent procedure](../how-to/verify-desktop-mcp-compatibility.md).

The [desktop follow-up investigation](mcp-desktop-protocol-investigation.md) confirms an additional distinction: SDK 2.2.0 supports old and new wire revisions by default. Our explicit protocol pin disables that compatibility. The separate SDK diagnostic retains the same dependency graph and calls official APIs; it adds no SDK 1.x source or package. Its older-revision success is not actual desktop acceptance and does not change the approved strict requirement.

## Task 6 project-reference integration — 2026-10-21

The executable MCP host now references the lab bridge-runtime and bridge-contract for production calls. Server/probe/tests/SDK-client locks add only Project entries. All external package rows, versions and content hashes remain identical to the Task 5 checkpoint; the official SDK remains exactly 2.2.0. This is no new external dependency adoption or latest-version observation. Program/ReadinessTools/test code is original lab work; existing SDK consumption and the previously attributed stdin adaptation are unchanged. TypeScript SDK 1.26.0 remains precedent-only. See [tool verification](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-readiness-tools.md) and [technical guide](../how-to/build-and-verify-mcp-readiness-tools.md).
