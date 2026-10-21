# MCP desktop compatibility checkpoint

Observed 2026-10-20, America/New_York (some evidence timestamps fall on October 21 UTC). **CP0 fails for both installed desktop applications. Task 2 and dependent Revit bridge work have not started.** Author approved [plan revision 2](../../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md) at `cfbfe043bd6c4d0627424ea3b099d0007459cb91` and selected Native execution; see [approval](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-20-plan-approval.md). Implementation started from `ff8a4f359fd0a9ff9147cab366a9cb81683a98cc` on `feat/revit-mcp-bridge`.

## Observed results

This section preserves the original default-settings run. The [follow-up investigation](../../../../docs/research/mcp-desktop-protocol-investigation.md) checks updates, finds an experimental Codex flag and demonstrates SDK 2.2.0's compatibility default separately. It qualifies the original research conclusion without changing these captured failures or claiming CP0 acceptance.

| Caller | Baseline | Actual exchange | Outcome |
|---|---|---|---|
| Official C# SDK test client | ModelContextProtocol 2.2.0; .NET 10 | `server/discover`, tools/list and two calls with `2026-07-28` metadata | Pass: synthetic success and `ProbeFailure`, explicit false/true isError, structured content; independent comparator passed |
| Codex desktop | Windows package `26.1013.4894.0`; wire client `codex-mcp-client` `0.163.0-alpha.10.2` | `initialize`, requested `2025-06-18` | Fail before tool discovery: server error `-32022` |
| Claude Desktop | Windows package `3.1623.0.0`; wire client `claude-ai` `0.1.0` | `initialize`, requested `2025-11-25` | Fail before tool discovery: server error `-32022` |

The exact server error in both desktop traces is: `Protocol version '2026-07-28' is not available through the initialize handshake.` Its data has `supported: []` and `requested: "2026-07-28"`. That requested field refers to the server's configured revision; the incoming messages above establish the clients' different requested revisions. No alternate negotiation was accepted. The approved spec explicitly says older fallback does not satisfy acceptance.

Both applications launched the same built probe at `D:/bim-agent-lab/projects/revit-readiness/tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe`, with `--wire-evidence` and separate ignored client directories. The [manifest](mcp-client-compatibility-evidence/manifest.json) records its hash, configuration locations, application identities and activation. Codex was reloaded by the author, who replied “reloaded.” Claude used Developer > Reload MCP Configuration. Discovery failed before either model could invoke the tool, so success/error calls and model/tool permission behavior were **not reached**, not passed. Claude's additional local-agent-mode attempt also sent the older revision; it is not substituted for the retained desktop `claude-ai` trace.

## Retained evidence and verification

- [Manifest and SHA-256 inventory](mcp-client-compatibility-evidence/manifest.json).
- SDK [incoming](mcp-client-compatibility-evidence/sdk/client-to-server.jsonl), [outgoing](mcp-client-compatibility-evidence/sdk/server-to-client.jsonl), [result](mcp-client-compatibility-evidence/sdk/sdk-client-result.json).
- Codex [incoming](mcp-client-compatibility-evidence/codex/client-to-server.jsonl), [outgoing](mcp-client-compatibility-evidence/codex/server-to-client.jsonl).
- Claude [incoming](mcp-client-compatibility-evidence/claude/client-to-server.jsonl), [outgoing](mcp-client-compatibility-evidence/claude/server-to-client.jsonl).
- [Offline test output](mcp-client-compatibility-evidence/offline-tests.txt) and [existing readiness baseline](mcp-client-compatibility-evidence/readiness-baseline.txt); local paths normalized, protocol bytes unchanged.

`Test-McpBridge.ps1 -Suite All` passed 14 C# behavioral tests and five Python test methods with subcases, then the official SDK client and independent transcript comparison. Builds completed with zero warnings/errors. Windows subprocess tests retained open stdin for idle/partial/pending-tool shutdown in both trace modes and required natural exit. Fake uncooperative reads confirmed no late caller-buffer modification or trace append. Stream disposal tests caught double disposal during implementation; borrowing wrappers fixed ownership and the tests then passed. Settings recovery was tested in new processes, including lost completion markers and foreign edits.

Independent transcript verification passes on the retained SDK folder and rejects both desktop folders with `Older handshake cannot satisfy CP0`. The existing readiness baseline passed 18 core tests, seven Excel tests and process isolation. No Revit build, installation, live model query, sample comparison or A8 acceptance was performed for this increment.

## Dependency and source review

The host pins official SDK 2.2.0 and a 12-package graph; see [dependency review](../../../../docs/research/mcp-sdk-dependencies.md). Fresh NuGet registry evidence listed **2.3.0** as latest stable. The approved pin was retained, rather than silently changing the reviewed plan. The old TypeScript SDK 1.26.0 remains precedent only. The stdin adapter is explicitly adapted from official **C# 2.2.0** source; licenses/notices and modifications are retained. MCP itself comes from the official package.

## Configuration cleanup and remaining decision

Both temporary entries were removed from disk. Claude's recovery followed the saved receipt and its MCP configuration was reloaded afterward. Codex had added `enabled = true`; automatic recovery refused that changed owned entry. After inspection, a separate durable cleanup receipt captured current bytes, removed only that exact probe block, and verified all unrelated parsed settings were unchanged. Private backups remain ignored under `.superpowers/sdd/2026-10-20-revit-mcp-bridge/`. Codex's cached registration can be refreshed with its next MCP reload; disk cleanup is not proof that every cached connection refreshed.

The agent inspected local configuration/help and [OpenAI's MCP setup](https://learn.chatgpt.com/docs/extend/mcp?surface=cli), [Claude's local-server support](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop), and the [MCP local-server guide](https://modelcontextprotocol.io/docs/2026-07-28/develop/connect-local-servers). No documented protocol override resolving this failure was found there. This does not establish that every available or future client release is incompatible. No application upgrade or protocol fallback was attempted.

The author can retain the strict requirement and select/test supporting client versions, or explicitly revise protocol acceptance and then test a compatible exchange with SDK 2.2.0. No alternate revision has yet been proved in both desktops. The architecture and Revit boundaries remain unchanged; CP0 does not permit Task 2 until resolved. R3 implementation review remains outstanding.

Teaching layers: [participant story](../../../../docs/tutorials/07-test-desktop-compatibility-first.md) and [agent procedure](../../../../docs/how-to/verify-desktop-mcp-compatibility.md).
