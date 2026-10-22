# Fresh R3 checks and retained-evidence replay

October 22, 2026. Implementation head `627e227c8054feaf91539976b32dd09228c4e93f`, Windows x64, Revit closed. No production source, dependency, installed file or client setting was changed. The installed desktop servers were not stopped. Build outputs in ignored local folders are not installed/live acceptance.

Commands run sequentially from the repo root using PowerShell 7:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-Readiness.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-McpBridge.ps1
dotnet list projects/revit-readiness/src/mcp-server/BimAgentLab.RevitReadiness.Mcp.csproj package --include-transitive --vulnerable --format json
python projects/revit-readiness/docs/verification/r3-evidence/replay.py
```

All five commands exited **0**. `All` includes temporary installer fault/recovery cases and requires Revit closed. Do not run it in parallel with another build or against an uncoordinated open Revit session. The build requires the existing documented Autodesk API/.NET prerequisites; it performs locked restore and publishes a Windows x64 framework-dependent server, without installing it.

| Check | Result and retained log |
|---|---|
| Full offline bridge/MCP | [all.txt](all.txt): Deployment 41, Tools 72, Dispatch 88 net48 / 83 net10, adapter handoff 10, Pipe 53 per runtime, Foundation 33 per runtime, bilateral exchange, 7 adapter-evidence Python checks and 34 protocol/driver Python checks. Compatible three-revision and strict-July/legacy-rejection checks pass. |
| Existing command/export regression | [readiness.txt](readiness.txt): 18 core, 7 Excel, and process-isolated export pass. |
| Locked build | [build.txt](build.txt): Revit API/net48 build and net10 server publish pass. No installation. |
| NuGet advisory query | [advisories.json](advisories.json): no vulnerable packages listed in the CLI's dated response; [package graph](package-graph.json) separately enumerates all 12 locked runtime packages. No upgrade performed; no perpetual security guarantee. |
| Retained evidence replay | [replay.json](replay.json): **1,838 hash entries across 21 original manifests**, 15 full case comparisons, two stale-context/recovery comparisons, two initial desktop continuity captures, fresh-post-reinstall wrapper capture, six host/SDK diagnostic checker invocations. This rechecks saved evidence, not new host execution. |

The All build emitted **four CA1416 platform-analysis warnings** in `ReadinessArgumentBoundary.cs` (Windows-only budget/JSON APIs called from a site the analyzer treats as cross-platform). These are retained, not fixed or suppressed in this documentation checkpoint. The Windows-only build/tests passed; a warning-free clean build is not claimed. Subsequent incremental build output has zero warnings but does not erase the earlier diagnostics.

The earlier recovery run's 15-second driver timeout remains in [its original record](../2026-10-22-shared-recovery-execution.md). This fresh full run passed without that timeout. No cause or repair has been established.

## Audit inputs and limits

- [source-inventory.json](source-inventory.json): 244 tracked source/test/script/notice files, original Git blob IDs plus actual checkout-byte SHA-256 hashes. Paths and the full implementation commit identify the source behind the fresh checks. Checkout line endings can change byte hashes; use the pinned Git blob identities for repository comparisons.
- [provenance-check.json](provenance-check.json): all 244 source entries still match; two license-source records and nine retained package-notice records match their declared hashes. This is consistency verification, not independent provenance or legal approval.
- [installed-source-delta.txt](installed-source-delta.txt): complete relevant host/server diffs from installed pins to implementation head. Read [O1](../2026-10-22-r3-acceptance.md#open-items-for-review-not-invented-reviewer-findings) before interpreting live evidence.
- [hashes.json](hashes.json): this folder's retained evidence integrity, excluding the manifest itself. It is not an independent correctness attestation.
- [replay.py](replay.py): read-only assertion script, no app launch or file mutation. It verifies only the explicitly selected original manifests and captures; it does not claim every historical file in the repository is part of those 1,838 entries.

The known SDK client defect is checked by replaying its characterized observations; the final All suite is not a passing require-propagation test. Independent R3 and author disposition remain pending. No Revit UI operation or desktop tool call is needed to inspect this packet.
