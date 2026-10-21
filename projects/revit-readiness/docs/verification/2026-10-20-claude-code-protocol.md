# Claude Code on protocol 2026-07-28

Observed 2026-10-20, America/New_York. This is an alternative-client compatibility experiment, supplementing the [failed desktop CP0](mcp-client-compatibility.md). It is not Revit acceptance or approval to replace either required desktop.

## Actual configuration and evidence

- Client: Claude Code CLI **2.1.305**, print mode, signed in by the author before the complete run.
- Server: the existing separate synthetic probe, official **C# SDK 2.2.0**, unchanged explicit `2026-07-28` pin. No Revit access.
- Child-process settings: `MCP_SDK_GENERATION=v2`, `MCP_PROTOCOL_NEGOTIATION=auto`.
- Isolation: explicit probe-only MCP config with `--strict-mcp-config`; empty built-in tools/settings sources; only the probe tool allowed; `dontAsk` permissions; skills/Chrome/session persistence disabled. No delegation was requested or performed. Budget cap $0.50; client reported $0.0205338 list-price usage for the completed run.
- Private scratch retained under ignored `.superpowers/sdd/2026-10-20-revit-mcp-bridge/claude-code-research/`. Authentication, complete application logs and local settings are not committed.

The [observation manifest](mcp-client-compatibility-evidence/claude-code-2026-07-28/observations.json) records executable hashes, configuration, limitations and seven evidence-file hashes. Wire files are unchanged bytes. Client result files contain explicitly selected fields from the private JSON output, with result text unchanged; they are not represented as complete raw logs.

| Stage | Observed result |
|---|---|
| Before login | `server/discover` and `tools/list` succeeded on July 2026. Expired authentication prevented model calls; CLI exit 1. |
| After author reported “logged in” | Fresh CLI process exited 0; discovery, list and both calls used July 2026. |
| Success | `fail=false` returned `ProbeSuccess`, `isError=false`, `synthetic=true`, `revitAccess=false`. |
| Deliberate error | `fail=true` returned `ProbeFailure`, `isError=true`, with the same synthetic/no-Revit flags. |
| Client presentation | Correctly described success and intentional error separately. |
| Cleanup | No probe process remained. No global MCP entry or persistent runtime override was added; test overrides belonged to the child process. |

The unchanged [independent checker](../../tests/verify_mcp_transcript.py) passed against the authenticated capture. It matched request/response identities, exact protocol metadata and explicit boolean flags, and rejected the possibility of satisfying the exchange with an older initialization handshake. [Saved check result](mcp-client-compatibility-evidence/claude-code-2026-07-28/verification.json).

```powershell
python projects/revit-readiness/tests/verify_mcp_transcript.py projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/claude-code-2026-07-28/authenticated --protocol 2026-07-28
```

An initial inventory showed 2.1.239; the tested process and subsequent executable inspection both showed 2.1.305. The update mechanism was not established. Acceptance evidence belongs to **2.1.305**, not the earlier inventory. No application installer was run by this experiment.

## Limits and continuation

The client is Claude Code, not Claude Desktop. Optional Tasks, elicitation, channels, HTTP/OAuth, streaming recovery and live Revit operations were not tested. SDK 2.2.0 and existing source/provenance remain unchanged. The approved spec, plan and production host were not edited. A future run must create new evidence rather than overwrite these captures.

The [research comparison](../../../../docs/research/mcp-protocol-tradeoffs-and-agent-clients.md) explains the choices. The [agent procedure](../../../../docs/how-to/verify-desktop-mcp-compatibility.md#test-an-alternative-client-without-changing-the-server) reproduces this bounded test; the [participant story](../../../../docs/tutorials/07-test-desktop-compatibility-first.md#ask-what-we-lose-and-test-another-client) explains the author's direction.
