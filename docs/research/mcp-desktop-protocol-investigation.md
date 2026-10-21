# Is the desktop protocol blocker necessary?

Observed **2026-10-20, America/New_York**. This follow-up qualifies the [original CP0 failure](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md), preserved at commit `9ea3151b323ae50b919fed2e4cb89e10362e598a`. It does not revise the approved specification, change the production host's protocol pin, or authorize Task 2. See the [participant story](../tutorials/07-test-desktop-compatibility-first.md) and [agent procedure](../how-to/verify-desktop-mcp-compatibility.md).

## Conclusion and correction

**Later alternative-client result:** [Claude Code 2.1.305 passed](mcp-protocol-tradeoffs-and-agent-clients.md) discovery, successful and deliberate-error calls on `2026-07-28` against the unchanged strict C# server. This preserves a new-protocol option if the author changes the seminar client scope. It does not overturn the recorded desktop failures or satisfy their acceptance requirement.

The protocol difference is real; making it block our demonstration is a policy we chose. Official C# SDK **2.2.0** can serve both the older desktop exchanges and `2026-07-28`. Our host explicitly sets `McpServerOptions.ProtocolVersion = "2026-07-28"`, disabling that normal compatibility. The approved acceptance contract separately requires the new revision in both desktops. A single configuration change would therefore not, by itself, satisfy the reviewed contract.

The agent should have separated **use a modern SDK**, **support the current protocol**, and **require every client to use only that protocol** before recommending the strict requirement. The first two do not imply the third. Calling both desktop products unsupported without qualifying their versions, settings and tested connection path would also overstate the original result.

**Further correction about the 1.x generation:** the author challenged the agent's claim that there was no direct correspondence between SDK major versions and protocol dates, citing prior use of 1.28 and 1.30. Official Python SDK [1.28.0 types](https://github.com/modelcontextprotocol/python-sdk/blob/32d32908feb7b15eddeb46872774bf95869cc5f0/src/mcp/types.py) and [1.30.0 types](https://github.com/modelcontextprotocol/python-sdk/blob/8c2fa6eabacde869d75568e33b9d466b58c2f512/src/mcp/types.py) both identify `2025-11-25` as their latest protocol. Their supported lists ([1.28.0](https://github.com/modelcontextprotocol/python-sdk/blob/32d32908feb7b15eddeb46872774bf95869cc5f0/src/mcp/shared/version.py), [1.30.0](https://github.com/modelcontextprotocol/python-sdk/blob/8c2fa6eabacde869d75568e33b9d466b58c2f512/src/mcp/shared/version.py)) also include `2025-06-18`. Thus **the proposed desktop compatibility exchanges would indeed use protocols from the SDK 1.x generation, implemented by our SDK 2.2.0 server**. They would not demonstrate the newer `2026-07-28` protocol behavior. The formal date-based naming does not negate this relationship. The agent's earlier naming-focused answer obscured the practical consequence and was misleading. These source checks do not identify which SDK package the author previously used or which library is inside either desktop.

No feature exclusive to the new revision has been identified as necessary for the approved readiness workflow: named read-only tools, structured results, explicit failures and our own document-context checks. That is a scope assessment, not proof that every future BIM feature works on an older revision. Compatibility would still need the real desktop, error, completeness and live Revit tests. It would not require SDK 1.x, copying the old TypeScript bridge, or implementing MCP ourselves.

## Updates and settings checked

| Application | Observed baseline / update check | Setting or supporting documentation | What this establishes |
|---|---|---|---|
| Codex desktop | App updater reports version `26.1013.40934`, build `20770`, distribution `prod`, status `up_to_date`. Installed Windows package identifies itself as `26.1013.4894.0`; wire engine is `0.163.0-alpha.10.2`. Keep these identifiers distinct. | Installed engine's `features list` exposes `mcp_2026_07_28`, **under development**, initially false. A separate `codex_apps_mcp_2026_07_28` flag also exists; it is not the local-server flag we are testing. | No update is eligible through the configured updater. This is not a claim about every distribution or beta. The recognized experimental switch merits a desktop test, not a declaration of stable support. |
| Claude Desktop | Installed Windows package `3.1623.0.0`, executable product `3.1623.0`. The official Windows x64 latest-download endpoint redirects to release `3.1623.0`. | Official local-server, release-note and Windows deployment documentation inspected; no documented local-stdio protocol override or minimum Desktop version for this revision found. Anthropic's July announcement describes a rollout across products. | No newer public Windows x64 download was found at that endpoint. Neither the announcement nor an absent search result proves the installed local-server path can use the new revision, or that all Claude releases cannot. |

Codex's updater check did not download, install or restart anything. Claude's Help > Check for Updates was invoked once and displayed a checking state; a final native updater result was not captured. The public download comparison is the retained update evidence for Claude. Its HTTP endpoint was queried for redirect headers only; no installer was downloaded. A HEAD request returned 405; a GET with automatic redirects disabled returned 307 and the versioned location.

Codex's ordinary MCP/config-reference documentation did not document the dated switch in the pages inspected. The switch was found by asking the **installed engine** to list its features. Follow-up inspection of OpenAI's [app-server README at `d571eaf`](https://github.com/openai/codex/blob/d571eaf1f61d0d93a797506335a99a9950ffe37b/codex-rs/app-server/README.md#hosted-codex-apps-mcp-protocol) confirms the distinction between the local/other-server flag and the hosted Apps flag. This is current source evidence, not proof that its main-branch behavior matches the installed desktop. Do not confuse Claude Code with Claude Desktop, or OpenAI's cloud MCP Events documentation with Codex's local stdio client.

## Why the strict server rejects the old clients

At the inspected SDK commit [`6fa3825`](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/ModelContextProtocol.Core/Server/McpServerOptions.cs), the options documentation explicitly defines both behaviors. A null protocol option supports the SDK's listed revisions. Pinning the new revision rejects the older `initialize` handshake. The implementation follows that branch in [`McpServerImpl.cs`](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/ModelContextProtocol.Core/Server/McpServerImpl.cs). The [public API reference](https://csharp.sdk.modelcontextprotocol.io/v2/api/ModelContextProtocol.Server.McpServerOptions.html) documents the same distinction.

The [new protocol](https://modelcontextprotocol.io/specification/2026-07-28) uses per-request metadata and optional server discovery instead of the older initialization exchange. It is a meaningful message/lifecycle change. Changing the date string in a client's old handshake would not implement it. The SDK's compatibility path handles the actual formats.

## Controlled diagnostic: SDK behavior, not desktop acceptance

The separate [diagnostic source](../../projects/revit-readiness/tests/mcp-sdk-diagnostic/Program.cs) uses official SDK 2.2.0 clients and servers. It changes only the diagnostic server's protocol option, then pins each diagnostic client's revision. It never calls the production host or Revit. It checks discovery, negotiated revision, synthetic success, structured failure and explicit false/true error flags.

| Diagnostic server option | SDK client `2025-06-18` | SDK client `2025-11-25` | SDK client `2026-07-28` |
|---|---|---|---|
| Explicit `2026-07-28` | Expected rejection at initialization | Expected rejection at initialization | Pass |
| SDK default (`null`) | Pass | Pass | Pass |

All six expected outcomes were observed. The [retained result](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/sdk-default-comparison.json) is labeled as an SDK diagnostic. It demonstrates that the strict pin causes these rejections, not that either actual desktop has passed with a different configuration. The diagnostic adds no SDK 1.x dependency or copied legacy source. The existing host project reference supplies the exact locked SDK package; original diagnostic code calls the official APIs.

## Actual Codex experimental-setting test

Before changing configuration, the agent saved exact original/proposed bytes, hashes and recovery data privately. It temporarily enabled **only** `features.mcp_2026_07_28 = true` and added the existing strict synthetic probe with a fresh evidence directory. The engine then reported the local-server flag true; the separate apps flag remained false. The host's strict protocol setting was unchanged.

The author replied **“reloaded.”** Six fresh desktop connections all still sent `initialize` requesting `2025-06-18`, with wire client `codex-mcp-client` `0.163.0-alpha.10.2`, and received error `-32022`. No tool became available to this conversation; success/error tool calls were not reached. The [separate captures](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/codex-feature-reload/) preserve that outcome.

The author then reported **“restarted the app.”** Six additional fresh connections, retained under [codex-feature-restart](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/codex-feature-restart/), again sent `2025-06-18` and received `-32022`. The engine's feature listing still reported the temporary flag true at that point. No probe tool became available; calls remained not reached.

This establishes that **the setting plus both MCP reload and a reported full application restart did not resolve the failure in this installed desktop**. We have not established why the desktop continued using the old exchange, whether a different configuration scope or distribution would behave differently, or that the feature is universally ineffective. OpenAI's [experimental-feature command guidance](https://learn.chatgpt.com/docs/developer-commands) describes applying choices on restart in its terminal workflow; that motivated this extra activation test, without documenting this exact desktop setting. No CLI harness is substituted for desktop proof.

Cleanup removed only the temporary feature and owned probe entry after saving a separate durable cleanup record. The engine again listed both relevant flags false, and no probe process remained. Other settings changed during the experiment, so the final file is not byte-for-byte or semantically identical to the original snapshot; the cleanup verified all unrelated current settings were preserved. It did not overwrite those changes with an old full-file backup. Disk cleanup is verified; a further application-cache refresh is not claimed.

Private recovery remains ignored at `.superpowers/sdd/2026-10-20-revit-mcp-bridge/codex-feature-settings.json` and `codex-feature-cleanup.json`. Do not commit settings or recovery receipts. The temporary experiment is finished; it did not demonstrate the required new-protocol desktop exchange.

## Decision remains with the author

The strict requirement remains unchanged. After the desktop experiments, the agent recommended keeping SDK 2.2.0 while explicitly accepting and testing those clients' supported revisions, with a strict new-protocol harness as an additional check. The subsequent [Claude Code experiment](mcp-protocol-tradeoffs-and-agent-clients.md) adds a verified alternative that preserves the new protocol by changing client scope. Neither option has been approved. Revise the relevant spec/plan acceptance openly before changing it; do not silently mark CP0 passed. Actual readiness-tool acceptance remains outstanding in either path.

The teaching lesson is to ask **which user-visible capability requires a version restriction, what compatibility the SDK already provides, and what fails when that restriction is removed in an isolated experiment**. “Latest” is a research question, not automatically an exclusive wire-version requirement.

## Primary sources inspected

- [OpenAI MCP setup](https://learn.chatgpt.com/docs/extend/mcp) and [configuration reference](https://learn.chatgpt.com/docs/config-file/config-reference); local engine feature listing and app updater observation provide the machine-specific findings.
- [Claude Windows deployment](https://support.claude.com/en/articles/12622703-deploy-claude-desktop-for-windows), including its public latest-download link; [local MCP setup](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop) and [release notes](https://support.claude.com/en/articles/12138966-release-notes).
- [Anthropic's protocol announcement](https://claude.com/resources/articles/bringing-mcp-2026-07-28-to-claude). Product rollout statements are not a local-stdio compatibility test.
- [Pinned SDK versioning guidance](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/docs/versioning.md), options source, implementation and public API reference above.

The [observation manifest](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/observations.json) separates machine observations, research and unperformed tests. It supplements, rather than rewrites, the original evidence manifest.
