# Protocol tradeoffs and an agent client that uses the new revision

Observed **2026-10-20, America/New_York**. This follows the [desktop investigation](mcp-desktop-protocol-investigation.md). Research checkpoint `e5cb2ac` preserved the then-approved SDK, protocol and client requirements. The author subsequently selected the [desktop-first compatibility amendment](../superpowers/specs/2026-10-20-desktop-compatibility-amendment.md). The results below remain dated research. [Subsequent implementation and both actual desktop tests](../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) now pass amended CP0; this research alone did not establish that pass.

**Claude Code 2.1.305 passed a real agent exchange on `2026-07-28` with our unchanged strict C# SDK 2.2.0 probe.** Discovery, tool listing, synthetic success and deliberate structured failure passed the existing independent transcript checker. Claude Code's response correctly distinguished the two outcomes. See the [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md).

This changes the available choices: keeping the new protocol is feasible with a verified alternative client. Claude Code is not Claude Desktop, so this result does not satisfy the approved requirement for both desktop applications. The previous recommendation to permit older exchanges is one option, not the only way forward.

## What would older exchanges give up?

The two desktops requested different older revisions: Codex `2025-06-18`, Claude Desktop `2025-11-25`. Both belong to the SDK 1.x-era family discussed in the [earlier source correction](mcp-desktop-protocol-investigation.md). Do not treat every older revision as having identical features.

| Concern | Consequence for the lab |
|---|---|
| Required wall/ceiling information, scope, identities, counts and incomplete coverage | No required capability has been identified that needs July's protocol. These meanings belong to our evaluator and result contract. Older-protocol desktop execution would still need independent verification. |
| Structured tool results and explicit errors | Already available in June 2025. We would retain them, with application tests to confirm preservation. |
| Excel output | Produced by our local exporter, independently of the MCP message version. A version choice does not remove ClosedXML or its four worksheets. |
| Teaching the latest protocol | An older exchange cannot demonstrate July's lifecycle. A newer SDK package does not change that fact. |

The [June 2025 tools specification](https://modelcontextprotocol.io/specification/2025-06-18/server/tools) documents structured results, schemas and tool errors. The conclusions about our readiness and Excel behavior are an assessment of this repository's scope, not a completed legacy-client Revit test.

The newer revision offers meaningful differences for future work:

| July 2026 mechanism absent from the older exchange | Why it could matter later |
|---|---|
| Self-contained requests without initialization or protocol sessions | Changes lifecycle handling and makes shared remote services easier to distribute across instances. Our current local process gains less from load balancing. |
| Multi Round-Trip Requests | A new contract for requesting additional input during an operation; older elicitation is a different mechanism. |
| Redesigned Tasks extension | A standard extension contract for deferred work and later input. Long jobs were already possible; November 2025 had experimental tasks. |
| Catalog caching hints and HTTP routing headers | More useful for larger catalogs, gateways and remote BIM services than our two-tool local bridge. |
| Revised authorization requirements | Relevant to remote OAuth integrations; the current stdio experiment does not exercise them. |

Sources: [July changelog](https://modelcontextprotocol.io/specification/2026-07-28/changelog), [release explanation](https://blog.modelcontextprotocol.io/posts/2026-07-28/), and [November 2025 changelog](https://modelcontextprotocol.io/specification/2025-11-25/changelog). This table identifies new contracts, not proof that a chosen client implements every optional extension. Our probe tested none of those extensions.

Newer also entails changed recovery behavior: July removes HTTP stream resumption/redelivery, requiring a fresh request after a broken response stream. Our cancellation, deadlines and safe retry design still need testing. Do not equate a new protocol date with automatic reliability. Its [versioning contract](https://modelcontextprotocol.io/specification/2026-07-28/basic/versioning) also requires agreement on optional extensions separately from the core revision. Claude Code documents that its channel messages cannot use the newer revision; a future channel workflow needs its own compatibility decision. These limitations are outside the readiness probe.

## Why Claude Code was a concrete candidate

Anthropic's [MCP client runtime documentation](https://code.claude.com/docs/en/mcp#mcp-client-runtimes) distinguishes a TypeScript SDK 1.x runtime from its SDK 2.0 runtime. The latter supports July 2026. It documents rollout thresholds of Claude Code 2.1.232 for sessions fetching feature flags and 2.1.274 for sessions without them. A process chooses its runtime at startup.

For a reproducible experiment, we used documented child-process overrides:

```text
MCP_SDK_GENERATION=v2
MCP_PROTOCOL_NEGOTIATION=auto
```

The [environment-variable reference](https://code.claude.com/docs/en/env-vars) specifies those values. `auto` alone permits fallback in general; our unchanged strict server and captured request metadata establish that this particular connection used July 2026. These are Claude Code settings, not documented Claude Desktop settings.

The initial connection retrieved the catalog but could not make model calls because authentication had expired. After the author replied **“logged in”**, a fresh process made both calls. We kept the partial and complete evidence separately.

An early inventory printed version 2.1.239. By the actual probe run, both the executable's version command and the wire identity reported **2.1.305**. No installation/update command was issued for this experiment, and the transition mechanism was not established. The retained executable hash and actual tested version are the baseline; do not describe 2.1.239 as tested successfully. The official [2.1.305 release](https://github.com/anthropics/claude-code/releases/tag/v2.1.305) was also identified by the releases API during research.

## Is this specific to C#?

No. The protocol defines the messages exchanged, independently of implementation language. The July release announcement explicitly identifies updated **TypeScript, Python, Go and C#** SDKs. The [current official SDK directory](https://modelcontextprotocol.io/docs/sdk) lists other implementations and their support tiers; exact releases and optional features still need checking individually. A language being listed does not prove every historical package implements every revision.

Our runtime result provides a direct cross-language example: Claude Code's documented TypeScript-based v2 client talked successfully to our C# SDK 2.2.0 server. Rewriting that server in Python or TypeScript would not make the previously observed desktop handshakes become July requests. Client runtime/version/configuration and server support must agree. This experiment adds no SDK dependency, copied client source or custom MCP implementation to the lab.

## Options presented and the author's subsequent choice

1. Keep July 2026 and explicitly revise the seminar client scope to use the verified Claude Code baseline. Establish the corresponding review/acceptance changes before continuing; the full Revit workflow still needs all three samples and NC1/NC2.
2. Keep both desktop applications and explicitly accept their verified older exchanges, retaining new-protocol tests separately. Real desktop success/error and Revit acceptance remain necessary.
3. Keep both original requirements and continue investigating supporting desktop versions/settings. The retained failures remain the result for the configurations tested.

The research supported considering Claude Code if demonstrating the new protocol were the priority. The author then clarified that beginners need the desktop interfaces and selected **option 2**, preserving the possibility of adopting July as those applications support it. The [recorded amendment](../superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) keeps SDK 2.2.0 and new-protocol regression tests while allowing SDK-provided compatibility. This is a deliberate audience decision, not a claim that Claude Code failed. **The later compatible-host implementation and both desktop synthetic tests passed. Task 2 is eligible and unstarted; full live Revit acceptance remains pending.**
