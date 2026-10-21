# Compatible desktop checkpoint — 2026-10-20

**Amended CP0 passed in both actual desktop applications.** SDK **2.2.0** is unchanged. The same host build accepts both desktop revisions and passes a separate strict **2026-07-28** regression. This is synthetic connection evidence, not live Revit acceptance or R3 closure. Task 2 may now begin; it has not been implemented in this increment.

The author explicitly requested applying the [revision 3 amendment](../../../../docs/superpowers/specs/2026-10-20-desktop-compatibility-amendment.md), testing both desktops, keeping the new-protocol regression and recording both teaching layers. Native execution continued on `feat/revit-mcp-bridge` from documentation checkpoint `a4c0ee3`. The [original strict failures](mcp-client-compatibility.md), update/settings investigation and [Claude Code pass](2026-10-20-claude-code-protocol.md) are preserved.

## Actual clients and results

| Client | Installed Windows package | Observed wire identity | Revision | Result |
|---|---|---|---|---|
| Codex desktop | 26.1013.4894.0 | `codex-mcp-client` 0.163.0-alpha.10.2 | 2025-06-18 | Initialization, catalog, success and intentional error passed |
| Claude Desktop | 3.1623.0.0 | `local-agent-mode-bim-agent-lab-protocol-probe` 1.0.0 | 2025-11-25 | Initialization, catalog, success and intentional error passed |
| Official SDK harness, compatible host | C# SDK 2.2.0 | `BimAgentLab.Mcp.Client` | All three targeted revisions | Each full exchange passed |
| Official SDK harness, strict host | C# SDK 2.2.0 | `BimAgentLab.Mcp.Client` | 2026-07-28 | Modern discovery, catalog and both outcomes passed; both old revisions separately rejected with `-32022` |

Codex calls were made through this running desktop conversation after the author's MCP reload, not through a separate CLI. No model override was applied; its precise model build was not separately established. Claude was tested in its regular **Claude** tab, fresh chat titled **BIM Agent Lab compatibility test**, displaying **Opus 5.5 High / Manual** and a local-device connection. The author chose **Always allow** for the probe. This is not the earlier standalone Claude Code terminal test. Claude also opened a discovery-only connection identified as `claude-ai` 0.1.0; that connection did not make the test calls and is not counted as the complete exchange.

Both complete sessions returned `ProbeSuccess` with explicit `isError=false`, then `ProbeFailure` with `isError=true`; both payloads have `synthetic=true` and `revitAccess=false`. Claude displayed the successful result as no error and identified the second as the intended test failure. Its red “1 failed” tool label is expected for this negative control. The desktop result presentation is recorded in [selected observations](mcp-client-compatibility-evidence/compatible-desktops/presentation.md).

The [evidence inventory](mcp-client-compatibility-evidence/compatible-desktops/observations.json) retains six Codex and two Claude sessions. Exactly one session per application contains the complete sequence. Partial startup sessions are labeled incomplete; no exchange is assembled across sessions. Full independent checks:

```powershell
python projects/revit-readiness/tests/verify_mcp_transcript.py projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/compatible-desktops/codex/session-59c32b70ebf9422d9042e4a98c70fe22 --protocol 2025-06-18
python projects/revit-readiness/tests/verify_mcp_transcript.py projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/compatible-desktops/claude/session-b983f9d6c8f94a689548b4b4389d538d --protocol 2025-11-25
python projects/revit-readiness/tests/verify_mcp_transcript.py projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/compatible-desktops/sdk/july-strict --protocol 2026-07-28
```

## Implementation and verification

`McpHost` now defaults to `McpProtocolPolicy.Compatible`, mapping to the SDK's `ProtocolVersion=null`. The explicit `Strict20260728` test policy uses the same transport, stream ownership and shutdown path. The separate probe accepts `--require-protocol 2026-07-28`; desktop entries omit it. There is no participant protocol selector, message translation proxy, SDK downgrade, new package or copied legacy code.

The [dependency/provenance record](../../../../docs/research/mcp-sdk-dependencies.md) still applies: official C# SDK 2.2.0 packages; attributed adaptation of its stdin-cancellation helper; original lab host/trace code; TypeScript SDK 1.26.0 remains precedent-only. Existing reader/evaluator reuse belongs to later bridge work.

The extended tests failed against the old strict default before the change. The completed `Test-McpBridge.ps1 -Suite All` run passed **19 C# behavioral tests**, including unchanged real Windows open-stdin shutdown tests in both trace modes, and four standalone SDK/wire comparisons. Python coverage subsequently increased to **10 test methods**, all passing, including settings recovery and the Claude checker regression. The completed readiness regression passed **18 core tests, seven Excel tests and the isolated export-host test**. Builds recorded zero warnings/errors. A separate readiness run interrupted by the MCP reload is not counted as proof. Retained diagnostic logs identify the completed runs.

Actual Claude traffic exposed a checker assumption: legacy `tools/list` may omit the optional `params` object. The checker originally raised `KeyError` despite valid SDK responses. The pinned [official schema fixture](https://github.com/modelcontextprotocol/typescript-sdk/blob/fcef852f6cdafea07f36cf6fae69b37c8adf85be/packages/core-internal/test/corpus/schema-twins/2025-11-25.schema.json) requires only `id`, `jsonrpc` and `method` for that request. A focused test reproduced the error before correction. Only legacy listing accepts absent params; modern metadata, initialization versions, lifecycle, identities and exact success/error assertions remain required. All retained captures are unchanged bytes.

## Configuration, cleanup and evidence integrity

Each app used entry `bim-agent-lab-protocol-probe`, executable `D:/bim-agent-lab/projects/revit-readiness/tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe`, with `--wire-evidence` pointing to its separate ignored `samples/local/mcp-compatible-desktops/<client>` directory. Codex configuration was `%USERPROFILE%/.codex/config.toml`; Claude configuration was `%APPDATA%/Claude/claude_desktop_config.json`. Executable, host and SDK hashes are in the inventory. No experimental protocol flag was added for this test.

Durable private receipts preceded each configuration mutation. After the initial registration, Claude's entry was found absent and unrelated settings had changed; the cause was not established. The agent preserved that current state, prepared a new receipt and reapplied only the probe entry. It did not restore an old whole-file backup. Cleanup restored this later Claude baseline. Codex had added `enabled=true` to the owned entry; a separately prepared cleanup receipt retained current bytes and verified that removing the reviewed block preserved all unrelated parsed settings. Original receipts were not rewritten to hide these changes. The author-selected Claude permission was not reset by the agent.

Both temporary entries were removed from disk. Claude's MCP configuration was reloaded after removal. The author reloaded Codex and supplied a screenshot showing the probe absent; its tool namespace also disappeared. Claude retained one idle probe child after configuration reload. After verifying its exact executable, command line and Claude parent, the agent stopped only that test child; no probe process remained. This forced cleanup is not counted as graceful-shutdown proof: the separate Windows subprocess tests establish that behavior. Private recovery bytes remain ignored under `.superpowers/sdd/2026-10-20-revit-mcp-bridge/compatibility-2026-10-20/` and are not distributed.

Wire files are copied byte-for-byte; diagnostic logs normalize repository/home paths and line endings as disclosed in the inventory. No credentials, full settings, unrelated conversations, binaries or RVT files are included. [SHA-256 manifest](mcp-client-compatibility-evidence/compatible-desktops/manifest.json) covers the retained evidence.

## What remains

The connection checkpoint is complete under author-directed spec/plan revision 3. Earlier external R1/R2 closures apply to revision 2. R3 must review this amendment, implementation and evidence along with the later bridge. IPC, valid Revit execution, installation/recovery, three principal live samples, NC1/NC2, both desktops' full A8 workflow and live July SDK coverage remain outstanding. No model was opened, changed or queried by this increment.
