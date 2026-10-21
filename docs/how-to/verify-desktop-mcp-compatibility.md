# Verify desktop MCP compatibility before connecting Revit

For agents and technical readers. The [participant story](../tutorials/07-test-desktop-compatibility-first.md) explains the author's direction. The [approved plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) Task 1/CP0 is the contract; the [verification record](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) contains observed results. CP0 currently fails. Do not start Task 2 on the strength of the SDK harness alone.

## Sources and implementation boundaries

- `projects/revit-readiness/src/mcp-server`: net10.0 host library, exact `[2.2.0]` package constraint and lockfile; official `McpServer.Create`, `StreamServerTransport`, `McpServerHandlers`, `ProtocolVersion = "2026-07-28"`.
- `tests/mcp-probe`: separate executable exposing only `lab_protocol_probe` with required boolean `fail`. It cannot read Revit. No synthetic backend or probe switch is added to a production Revit server.
- `tests/mcp-client`: official SDK client plus an evidence result explicitly labeled as a harness. `tests/mcp-tests` exercises protocol, stream ownership and actual Windows subprocess shutdown.
- `tests/verify_mcp_transcript.py`: independent assertions over captured JSON lines, not a replacement MCP implementation. It requires discovery, tools/list, correlated success/error calls, exact protocol metadata, structured content, explicit false/true flags and clean stdout.
- [Dependency/provenance record](../research/mcp-sdk-dependencies.md): official package consumption, attributed C# 2.2.0 helper adaptation, original host/trace code, and precedent-only TypeScript 1.26.0.

## Run the offline checkpoint

From the repository root with .NET SDK 10.0.204 and Python 3.12 available:

```powershell
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Protocol
```

`-Suite All` currently runs the same available Task 1 suites; it does not imply future bridge or host acceptance. Builds use locked restore. The script runs 14 C# behavioral tests, five Python test methods with subcases, the separate SDK client, and the independent wire checker. Each run creates fresh ignored evidence under `samples/local/`.

Windows child tests keep stdin open and signal cancellation through a test-only named event. Idle input, partial input and a pending tool must exit naturally within five seconds in both trace modes. EOF or watchdog termination cannot establish a pass. The input adapter owns a private pending-read buffer, cancels waiting, observes late faults, and rejects reuse. The host owns raw streams; the SDK receives wrappers that leave them open. SDK/transport disposal unwinds before trace sinks close. Trace files use CreateNew, flush captured bytes and cap each direction at 8 MiB. Capture failure fails the run visibly.

## Register a temporary probe safely

Inspect existing configuration and current official application documentation. Do not print unrelated entries or credentials. Use the absolute built probe executable and an ignored per-client evidence directory. The supported local registration used here was `%USERPROFILE%/.codex/config.toml` for Codex and `%APPDATA%/Claude/claude_desktop_config.json` for Claude. Current sources: [OpenAI MCP setup](https://learn.chatgpt.com/docs/extend/mcp?surface=cli), [MCP local-server guide](https://modelcontextprotocol.io/docs/2026-07-28/develop/connect-local-servers), [Claude local-server support](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop).

The test utility is `projects/revit-readiness/scripts/manage_mcp_probe_settings.py`; it is not Task 7's future installer. `prepare` saves exact original bytes, proposed bytes, hashes and owned-entry recovery information with flush/fsync before `apply` changes configuration. Use a new private receipt path for each operation. Example for Codex (adapt the root and existing config path):

```powershell
$probe = (Resolve-Path './projects/revit-readiness/tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe').Path
$receipt = './.superpowers/cp0-codex-new-receipt.json'
$config = Join-Path $env:USERPROFILE '.codex/config.toml'
$evidence = Join-Path (Get-Location) 'projects/revit-readiness/samples/local/mcp-desktop-cp0/codex'
python ./projects/revit-readiness/scripts/manage_mcp_probe_settings.py prepare --receipt $receipt --config $config --format toml --command $probe --evidence $evidence
python ./projects/revit-readiness/scripts/manage_mcp_probe_settings.py apply --receipt $receipt
```

Use `--format json` and the existing Claude config for Claude. Disclose the exact temporary entry and reload each actual application. Do not infer activation from a config write, or use a standalone CLI process as desktop proof. If the application window is unavailable to automation, request the user's reload rather than manipulating private app internals.

Run `lab_protocol_probe` with `{"fail":false}` and `{"fail":true}` through each application if discovery succeeds. Capture installed app version, wire client identity/version, config location and owned entry, mode/model/permissions, discovery and both results. If handshake fails first, explicitly mark later stages and model/permission checks as not reached. Do not weaken protocol requirements or rewrite traffic to make the test pass.

```powershell
python ./projects/revit-readiness/tests/verify_mcp_transcript.py <session-directory> --protocol 2026-07-28
python ./projects/revit-readiness/scripts/manage_mcp_probe_settings.py restore --receipt $receipt
```

The restoration checks ownership and preserves concurrent unrelated edits. If the entry itself changed, stop automatic cleanup, inspect the exact delta, preserve current bytes before a reviewed removal, and verify all unrelated settings remain equal. This occurred when Codex added `enabled = true`; do not modify a receipt to conceal that change. Retain private recovery receipts outside Git, and reload the app to refresh its cached registration. Restoration tests cover missing completion markers, new-process recovery, unrelated edits and changed owned entries.

## Record the checkpoint and decide what follows

Retain narrowly scoped, reviewed transcripts and a SHA-256 manifest. The saved lab-only messages can be copied unchanged; omit full settings, unrelated logs and chat content. Normalize personal paths only in diagnostic logs and disclose that transformation. Record exact failure evidence as carefully as a pass.

Both actual desktops must show `2026-07-28` plus discovery, success and structured failure before CP0 passes. In the original run both used legacy `initialize` and received `-32022`. The first search did not find a resolving documented setting; the [follow-up investigation](../research/mcp-desktop-protocol-investigation.md) subsequently found a recognized **experimental** Codex feature flag. Preserve that distinction instead of repeating a blanket incompatibility claim. A new client version or setting requires another test. An alternate protocol requires an explicit specification decision and new evidence. CP0 success would still leave Task 8's live samples, NC1/NC2 and both desktop applications' Revit acceptance outstanding.

## Diagnose a version restriction before changing acceptance

Separate four facts: the installed SDK package, its supported wire revisions, the client exchange actually observed, and the server/acceptance policy. Inspect the pinned SDK source as well as current documentation. In SDK 2.2.0, `McpServerOptions.ProtocolVersion = null` supports its listed old/new revisions; our explicit `2026-07-28` pin rejects legacy initialization. The production host and approved spec remain strict.

Reproduce the isolated SDK comparison from the repository root:

```powershell
dotnet restore ./projects/revit-readiness/tests/mcp-sdk-diagnostic/BimAgentLab.Mcp.SdkDiagnostic.csproj --locked-mode
dotnet build ./projects/revit-readiness/tests/mcp-sdk-diagnostic/BimAgentLab.Mcp.SdkDiagnostic.csproj -c Release --no-restore
& ./projects/revit-readiness/tests/mcp-sdk-diagnostic/bin/Release/net10.0/BimAgentLab.Mcp.SdkDiagnostic.exe
```

The diagnostic self-launches SDK servers and clients. Six comparisons hold SDK 2.2.0 constant, varying strict/default server options and pinned client revisions. Expected rejections must contain the initialization error; accepted cases check exact negotiated revision, tool discovery, structured success/failure and explicit false/true flags. Exit 0 and `allExpectedOutcomesObserved: true` establish these SDK observations only. This separate test is not included in `Test-McpBridge.ps1`, does not call the production host, and is not CP0 or Revit acceptance. Its project reference inherits the existing exact SDK dependency and commits a lockfile.

For Codex, locate the installed engine without assuming the versioned directory name on another machine. The observed command was:

```powershell
& "$env:LOCALAPPDATA/OpenAI/Codex/bin/896a0166b774037a/codex.exe" features list |
    Select-String '^mcp_2026_07_28\s|^codex_apps_mcp_2026_07_28\s'
```

Both were initially false and labeled `under development`. Only `features.mcp_2026_07_28` is the candidate local-server experiment here; do not enable the separate apps flag by analogy. Save durable recovery before any global configuration change, inspect existing overrides, preserve unrelated settings and disclose the exact experiment. A recognized CLI feature and a successful config write do not prove the running desktop applied it. Reload the actual application, inspect fresh wire evidence, then call both probe outcomes through that application. If no new trace appears, activation is unverified, not a protocol rejection.

Use Codex's configured updater result to describe eligible updates, retaining its product/build identifiers separately from Windows package identifiers. For Claude, compare the installed package/product version with the official Windows latest-download redirect; do not download an installer merely to discover its version. Record native updater results only if actually observed. Search absence and cross-product rollout announcements cannot establish universal lack of support.

Retain the original failed captures. Add follow-up results under a separate evidence directory and record which configuration produced them. Restore only owned temporary changes, verify unrelated settings, and distinguish disk restoration from application cache refresh. The [follow-up record](../research/mcp-desktop-protocol-investigation.md) identifies the current experiment and private recovery location.

Observed follow-up outcome: both an actual MCP reload and the author's reported full app restart still produced the older exchange in Codex. Six captures from each stage are retained separately. The temporary setting/probe entry were removed and both feature flags returned to false in the engine listing. Unrelated changes made during the experiment were preserved; exact restoration of the original whole file is deliberately not claimed. Do not repeat the activation experiment or leave it marked pending on the strength of the earlier research checkpoint.

If compatibility is proposed, identify whether any required capability actually needs the newest protocol, then explicitly revise acceptance before changing production policy. SDK 2.2.0's compatibility path is SDK consumption, not an SDK 1.x dependency or hand-rolled protocol. Older-client acceptance must still preserve all readiness, incomplete-coverage, interruption and independent-verification guarantees.

Describe the protocol generation honestly: the observed `2025-06-18` and `2025-11-25` revisions are both implemented by the official Python SDK 1.28.0/1.30.0 line; see the [pinned source comparison](../research/mcp-desktop-protocol-investigation.md). The proposed desktop path therefore uses 1.x-era protocol behavior through SDK 2.2.0. Do not use the package's 2.x version or formal date naming to imply the connection uses `2026-07-28`. Supporting the new protocol in a harness and exercising it in a desktop are separate claims. Wire evidence identifies protocol behavior, not a desktop's internal SDK package version.

## Test an alternative client without changing the server

The [Claude Code verification](../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md) establishes a successful real-agent exchange on the new revision. The [tradeoff research](../research/mcp-protocol-tradeoffs-and-agent-clients.md) explains the user-facing choices. This is an alternative-client result, not a pass for the two applications named in CP0. Do not edit the normative spec, plan, production pin or existing checker to make the alternative count automatically.

1. Check the installed executable's `--version` and current official [runtime documentation](https://code.claude.com/docs/en/mcp#mcp-client-runtimes). Retain the version actually present in the wire messages and executable hash. The successful baseline here is **2.1.305**; an earlier 2.1.239 inventory is not the tested version.
2. Check authentication without printing credentials or account identifiers. If needed, have the user run `claude auth login` themselves. Never request a token in chat. Discovery can succeed while model calls remain blocked by expired authentication, as our first attempt demonstrates.
3. Use a fresh, private MCP configuration and evidence directory. Keep the existing strict C# SDK 2.2.0 probe. Supply runtime settings to the child process only; do not modify either desktop's configuration.
4. Allow only the synthetic probe tool and disable built-in tools. This test invokes the agent as the client under test; it does not delegate research or implementation. Do not bypass the client's permission system.

The following Python invocation, run from the repository root, preserves empty command arguments on Windows. It requires the existing Release probe build and the signed-in native Claude Code executable; adjust the executable path for another machine. Generated configuration, application output and invocation evidence belong in ignored `samples/local/`.

```python
import json, os, pathlib, subprocess, tempfile, uuid

root = pathlib.Path.cwd()
client = pathlib.Path.home() / '.local/bin/claude.exe'
probe = root / 'projects/revit-readiness/tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe'
run = root / 'projects/revit-readiness/samples/local' / ('claude-code-' + uuid.uuid4().hex)
run.mkdir(parents=True)
config = run / 'probe-only.json'
config.write_text(json.dumps({'mcpServers': {'bim-lab-protocol-probe': {
    'type': 'stdio', 'command': str(probe),
    'args': ['--wire-evidence', str(run / 'wire')]
}}}), encoding='utf-8')
env = os.environ.copy()
env.update({
    'MCP_SDK_GENERATION': 'v2', 'MCP_PROTOCOL_NEGOTIATION': 'auto',
    'DISABLE_AUTOUPDATER': '1', 'CLAUDE_CODE_DISABLE_CLAUDE_MDS': '1',
    'CLAUDE_CODE_DISABLE_AUTO_MEMORY': '1'
})
args = [str(client), '--print', '--mcp-config', str(config), '--strict-mcp-config',
        '--setting-sources', '', '--tools', '',
        '--allowedTools', 'mcp__bim-lab-protocol-probe__lab_protocol_probe',
        '--permission-mode', 'dontAsk', '--disable-slash-commands',
        '--no-session-persistence', '--no-chrome', '--max-budget-usd', '0.50',
        '--output-format', 'json', '--system-prompt',
        'You are a client compatibility test. Use only the supplied synthetic MCP probe. '
        'Do not use files, other tools, or delegate any work.',
        'Call mcp__bim-lab-protocol-probe__lab_protocol_probe with fail=false once, '
        'then with fail=true once. The second call is an intentional test error. '
        'Report both observed result codes and whether each was marked as an error. '
        'Do not infer success if a tool cannot be called.']
working = tempfile.mkdtemp(prefix='bim-agent-lab-client-')
result = subprocess.run(args, cwd=working, env=env, capture_output=True,
                        text=True, encoding='utf-8', timeout=90)
(run / 'client-output.json').write_text(result.stdout, encoding='utf-8')
(run / 'client-stderr.txt').write_text(result.stderr, encoding='utf-8')
print('Client exit:', result.returncode, 'Private evidence:', run)
```

The budget is a ceiling, not a promise of free usage. The recorded successful run reported about $0.021 in list-price usage. A timeout or authentication failure is not a pass; retain the partial evidence and inspect any remaining owned probe process. Never terminate unrelated client processes for cleanup.

After the client exits, use the fresh `wire/session-...` directory with the existing checker:

```powershell
python projects/revit-readiness/tests/verify_mcp_transcript.py <fresh-session-directory> --protocol 2026-07-28
```

Require discovery, list, correlated success/failure, exact metadata and explicit error booleans. Read the client's own presentation too: it must identify the deliberate error honestly. In this run all four exchanges occurred in one server session and the checker passed unchanged. If another runtime uses separate discovery and operational processes, preserve the process boundaries and review the verification procedure; do not concatenate captures or silently weaken the checker.

`auto` is a negotiation preference, not proof of the selected revision. Here the strict server advertised only July 2026 and each captured request carried that revision. Do not infer support for optional Tasks, channels or HTTP/OAuth from this tool test. Anthropic documents these runtime controls for Claude Code, not Claude Desktop.

Verify no owned probe remains, retain only reviewed lab traffic and selected non-secret output with clear transformation notes, and hash the retained bytes. No persistent MCP registration or environment override needs restoring with this child-only approach. CLI authentication/cache bookkeeping can still change; do not claim the entire application's state was untouched. The spec/plan and production host remain unchanged until the author chooses a client/protocol contract.
