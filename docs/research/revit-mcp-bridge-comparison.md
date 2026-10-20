# Revit MCP bridge comparison and version review

Observed: **2026-10-20**. Lab baseline: `4784b24`. This is source and package research, not a built or host-tested MCP integration. The [participant story](../tutorials/05-research-and-choose-the-mcp-bridge.md) explains the author's direction; the [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md) gives the research procedure and proposed implementation gates.

**Recommendation from the agent:** develop a small original Revit bridge around our existing readiness reader, with a separate C# MCP server using `ModelContextProtocol` **2.2.0**, targeting protocol **2026-07-28** and testing compatibility with the actual seminar client. Keep the existing bridge as a source precedent. This selects a direction for the first experiment; successful deployment, client interoperability, and Revit execution remain unverified. No upstream bridge was upgraded or installed in this research pass.

## What “latest MCP” means here

The official [latest specification](https://modelcontextprotocol.io/specification/latest) redirected to [2026-07-28](https://modelcontextprotocol.io/specification/2026-07-28). The [release announcement](https://blog.modelcontextprotocol.io/posts/2026-07-28/) describes the move to a stateless core and per-request information. Protocol revision, SDK package version, Revit bridge version, and JSON-RPC 2.0 are separate identifiers. Do not set a protocol field to `2.0` merely because the SDK is v2.

The [machine-readable observation](evidence/mcp-version-observations-2026-10-20.json) retains GitHub release metadata and resolved commits, npm/PyPI/NuGet responses, inspected package manifests, hashes, and five clean source-checkout observations. These are dated observations, not floating installation instructions.

| Component | Observed released/package version | Meaning for this lab |
|---|---|---|
| MCP protocol | `2026-07-28` | Latest official revision checked; wire behavior must be tested |
| TypeScript server SDK | `@modelcontextprotocol/server` `2.3.1` | Stable v2 uses split packages; Node >=20 in the package metadata |
| Older TypeScript package name | `@modelcontextprotocol/sdk` `1.32.1` | Latest under the old name remains the v1 line |
| Python SDK | `mcp` `2.3.0` | Published package and release manifest require Python >=3.10; the previously inspected main branch required >=3.11 |
| C# SDK | `ModelContextProtocol` `2.2.0` | NuGet package and GitHub release agree; target frameworks include net10.0, net9.0, net8.0, netstandard2.0 |
| Existing Revit bridge release | `v1.0.0` | Its own product version, unrelated to protocol or SDK numbering |
| Existing bridge's MCP dependency | Manifest `^1.7.0`; lockfile **1.26.0** | Source still imports v1 APIs; no v2 upgrade is present at the inspected main commit |
| RevitMCPSDK helper library | Latest listed overall `2027.0.0.5`; applicable 2024 line `2024.0.0.5` | A Revit-year-specific dispatch helper, not the official MCP protocol SDK |

Primary release sources: [TypeScript](https://github.com/modelcontextprotocol/typescript-sdk/releases/tag/v2.3.1), [Python](https://github.com/modelcontextprotocol/python-sdk/releases/tag/v2.3.0), [C#](https://github.com/modelcontextprotocol/csharp-sdk/releases/tag/v2.2.0), [bridge](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/releases/tag/v1.0.0). Exact registry URLs are in the observation JSON. We inspected stable release checkouts in addition to current main metadata; main can differ from what users install.

## Existing bridge: traced implementation

The previously inspected main commit `86cf7057785f2423fc0d0892c9a5d9c93deb1aa3` still matched upstream main at this check. Its latest release resolves to a different commit, `1a52de93d9f4565f94d0d1541186c9cf18f3648f`; this comparison examines main, not a claim that release binaries match it.

| Boundary | Source observation | Consequence for adaptation |
|---|---|---|
| MCP process | [Entry point](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/index.ts) imports the v1 server and stdio transport | Migrate package names, registration APIs, schema handling, and protocol entry point |
| Tool surface | [Registration](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/tools/register.ts) loads tool modules dynamically; the tree includes editing and dynamic-code tools | Define an explicit readiness-only allowlist for our deliverable |
| Revit selection | [Current-view handler](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/commandset/Services/GetCurrentViewElementsEventHandler.cs) reads the active document/view at execution and limits returned elements | Add our named checklist operation and bind requests to a specific document context; existing inventory is not our readiness rule |
| Errors | [Current-view tool](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/tools/get_current_view_elements.ts) catches failures and returns text without `isError` | Preserve machine-readable failure states instead of making failure look like a successful tool answer |
| Local connection | [Connection manager](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/utils/ConnectionManager.ts) serializes requests within one Node process and connects to localhost:8080 | Check multiple clients/processes and multiple Revit instances; this mutex is not a global request queue |
| Listener and framing | [Socket service](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/plugin/Core/SocketService.cs) binds `IPAddress.Any`, reads up to 8192 bytes, and parses each read as one JSON request; no authentication check was found in this path | Review connection restrictions and explicit framing; TCP read boundaries do not establish complete message boundaries |
| Responses and timeout | [Socket client](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/server/src/utils/SocketClient.ts) accumulates a JSON response and rejects after 120 seconds; the [query command](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/commandset/Commands/Access/GetCurrentViewElementsCommand.cs) waits 60 seconds | Define one deadline and cancellation lifecycle; timeout alone does not cancel an already queued Revit event |
| Host build | [Command project](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/commandset/RevitMCPCommandSet.csproj) supports R24/net48 and uses floating year-specific packages, Roslyn, Newtonsoft.Json, and toolkit packages | Resolve and lock a deliberate graph; inspect build deployment targets before running them |

The server also declares `better-sqlite3` (locked to 12.8.0), `ws`, and Zod 3 (locked to 3.25.76). Some support features outside our first operation and could be removed in a focused adaptation. The comparison does not assume every package is intrinsically required by a bridge.

We inspected the dispatch dependency rather than stopping at its package name. The published `RevitMCPSDK` 2024.0.0.5 nuspec points to commit `2d46e0c4965fff73f3677ab8eccadd21c1edd769`. Its [command base](https://github.com/DTDucas/RevitMCPSDK/blob/2d46e0c4965fff73f3677ab8eccadd21c1edd769/RevitMCPSDK/API/Base/ExternalEventCommandBase.cs) creates an ExternalEvent, raises it, and waits on the supplied handler. The inspected method does not inspect `Raise()`'s result or cancel a timed-out event. These are source-level acceptance concerns, not locally reproduced runtime failures. A complete candidate build has not resolved the bridge's floating NuGet graph.

## Minimal custom bridge around the existing check

Proposed flow:

```text
Agent client -- MCP/stdio --> separate C# server (modern .NET)
                              |
                         framed local IPC
                              |
                   Revit 2024 add-in (net48)
                   bounded queue -> ExternalEvent
                              |
                  existing reader -> evaluator -> report
```

Use the official MCP SDK for the protocol. The original code would be the small Revit integration and local request contract. An external process avoids loading the new SDK/dependency graph into Revit's shared process, applying the lesson from Excel. The SDK's netstandard2.0 asset does not by itself prove safe coexistence inside Revit.

Prefer C# for the first experiment because the reader, rules, scripts, and installed toolchain already use .NET. The [release source](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/Common/McpProtocolVersions.cs) contains both 2026-07-28 and legacy revision handling, and the [stdio sample](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/samples/QuickstartWeatherServer/Program.cs) keeps logs on stderr. This supports choosing a test candidate, not a claim that our client already uses the latest revision.

A Windows named pipe is the preferred IPC experiment. Specify user/session access, per-Revit-instance selection, bounded messages, request IDs, and shutdown; named pipes can be network-accessible, so the implementation must enforce the intended local boundary. Validate net48-to-modern-.NET interoperability. A framed, restricted loopback socket remains an alternative if the pipe experiment exposes deployment problems. See [Microsoft's pipe API](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream?view=netframework-4.8.1) and [Autodesk's ExternalEvent guide](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Advanced_Topics/Revit_API_Revit_API_Developers_Guide_Advanced_Topics_External_Events_html.html).

| Choice | What we retain | What we must implement or verify |
|---|---|---|
| Adapt existing bridge | Existing server, command registry, UI, multi-year configurations, dispatch pattern | v2/protocol migration, focused tool surface, framing and access, per-request identity/deadline, pinned packages, our readiness command, host acceptance |
| Minimal original bridge | Our tested reader/evaluator/report; official SDK protocol implementation | Startup/shutdown, local IPC, queue and cancellation, document identity, client setup, focused integration tests |
| Python external server alternative | Official SDK 2.3.0; approachable Python examples | Another runtime/package workflow and Windows IPC integration; still needs the same C# Revit handler |

The existing bridge remains reasonable for a broader editing platform or a team already invested in that ecosystem. For this two-hour teaching example, a focused original bridge appears easier to explain and constrain. This is an engineering judgment based on inspected scope, not a measured claim that custom development will be faster.

## Upgrade and license decisions

1. Target the current official protocol revision and a released SDK, with actual-client compatibility tests. Record which revision is really used; legacy fallback must be visible in evidence.
2. If adapting TypeScript, follow both the pinned [v1-to-v2 migration](https://github.com/modelcontextprotocol/typescript-sdk/blob/fcef852f6cdafea07f36cf6fae69b37c8adf85be/docs/migration/upgrade-to-v2.md) and [2026 protocol support guide](https://github.com/modelcontextprotocol/typescript-sdk/blob/fcef852f6cdafea07f36cf6fae69b37c8adf85be/docs/migration/support-2026-07-28.md). Changing a version number alone is insufficient. The latter explicitly says manually constructed clients/servers retain older behavior unless the appropriate protocol entry is used.
3. Use `ModelContextProtocol` 2.2.0 as the C# experiment's candidate pin, then restore, review all resolved dependencies/notices, lock them, and test. Modern server runtime/publishing prerequisites must be recorded separately from Revit's net48 runtime.
4. The existing bridge and inspected RevitMCPSDK source carry MIT notices. Python's inspected release declares MIT. TypeScript/C# v2 packages declare Apache-2.0; their repository LICENSE files retain a transition statement covering earlier MIT contributions and separate documentation terms. Preserve the actual notices; do not carry an old “all MIT” assumption into an upgrade. Full resolved dependency/license review remains an implementation gate.

No precedent implementation was copied into lab source. The evidence JSON retains metadata and package notice material for research. Current source-checkout hashes distinguish working-file bytes from Git-blob bytes, avoiding line-ending ambiguity.

## What is established and what remains

Established: live version metadata checked against primary sources/registries, five pinned source trees inspected clean, 29 relevant files hashed, RevitMCPSDK package-to-source provenance traced, existing bridge and minimal custom responsibilities compared, and a proposed first experiment defined. No candidate packages were installed into the lab project; no client configuration or Revit installation was changed.

Remaining: dependency resolution/audit, modern and legacy protocol interoperability, IPC framing and cancellation tests, and all three readiness cases through the actual Revit bridge. The [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md) defines those gates. Existing 0/0, 2/6, and 3/8 command results are the comparison oracle, not evidence that MCP already works.
