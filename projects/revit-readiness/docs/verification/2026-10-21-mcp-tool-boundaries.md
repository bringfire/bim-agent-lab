# Task 6 review corrections: transport termination and argument decoding

Baseline: `189fd78841f07f3b8b4ad999c7ed32aa3a6a36f1`. Author verification on Windows, 2026-10-21. [Original feedback and separate dispositions](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-boundary-review.md). **Task 6 remains open pending independent follow-up.** This record does not claim desktop A8, actual Autodesk scheduling/shutdown or model acceptance.

## Evidence and sequence

Retained evidence: [inventory](mcp-tool-boundary-evidence/inventory.json), 81 files. All 438 retained matching-schema evidence hashes were checked, including 357 earlier files. Earlier Task 6 evidence remains unchanged. Reviewer-authored source is retained byte-for-byte as attributed `.txt` evidence; local reruns use unchanged copies, not edited reproductions.

| Check | Observation |
|---|---|
| Unchanged review EOF probes, trace off/on | Both failed before changes: process still alive, request active, final-read guard allowed execution after five seconds |
| Review explicit-cancellation control | Passed before changes |
| Unchanged review raw Unicode probe | Both unpaired surrogate escapes returned JSON-RPC -32603; wrong-type/oversized and valid controls behaved as expected |
| First new test run | 37 pass / 13 fail; July cases incorrectly omitted required client-capability metadata and were fixed before assessing production behavior |
| Corrected red suite against unchanged production | 37 pass / 13 fail: six EOF, three Unicode, two input-fault and two EOF/held-callback cases |
| Handler-only Unicode catch plus shutdown correction | 47 pass / three fail. Raw malformed text still failed before handler entry during SDK typed conversion |
| Final focused Tools suite | 50 pass / zero fail |
| Unchanged reviewer probes after correction | EOF trace off/on, explicit cancellation and raw Unicode controls all exit zero |
| Full MCP/bridge regression | Tools 50; Dispatch 87 net48 / 82 net10.0 plus ten adapter handoffs; Pipe 53 and foundation 33 per runtime; protocol 19 C# / ten Python / four standalone SDK exchanges |
| Existing readiness regression / API build | All pass: 18 evaluator, seven Excel and process-isolation checks; real Autodesk-reference build with zero warnings/errors |

`task6-review-tests-green.txt` is the **unsuccessful handler-only attempt**, not final passing evidence. `task6-review-final-tools.txt` and `task6-review-all.txt` are the final checks. The Unicode diagnostic run deliberately printed handler-entry markers: four of six calls entered; the two malformed escapes did not. Diagnostics were removed before final tests. The input-fault test uses a controlled stream; EOF tests run the actual production executable against a real Windows pipe with a synthetic executor.

## Observed guarantees

Six EOF observations cross June 2025, November 2025 and July 2026 with tracing off/on. Each requires natural process exit, cancellation, rejected final-read guard, unchanged deadline, more than 15 seconds of original budget remaining, and both Response and Quiesced still held. Two explicit-cancellation controls use the same executable/pipe. Shutdown tests separately hold borrowed callbacks until HOST_STOPPED is observed; they cover EOF and the earlier explicit-stop/open-stdin path. Forced process termination is failure cleanup only.

Three raw-wire sessions each send high surrogate, low surrogate, wrong type, oversized text, valid ASCII and a valid surrogate pair. They assert JSON-RPC response identity, tool isError, stable domain code, unique request correlation, retryability, matching text/structured payloads, no report, and natural process exit. Subsequent valid calls in the same process reach ordinary BridgeUnavailable, demonstrating that the per-message rejection does not leak. [Independent transcript assertions](mcp-tool-boundary-evidence/transcript-check.json) check all eight process observations and 18 raw-wire results against literal expected values. These assertions were run by the authoring agent, not an independent reviewer.

## Implementation and provenance

Production changes are confined to McpHost, Program and ReadinessTools. EOF/read failure is observed via the official transport's channel completion; cancellation precedes waiting for SDK handlers. A terminal input failure remains observable. There is no second input reader or new timeout, and existing stdin buffer ownership remains intact.

The narrow SDK incoming-message filter identifies an undecodable contextToken before SDK JsonNode-to-typed-params conversion. It records a server-owned object marker and replaces only the invalid value with null for that conversion. The normal tool handler rejects the marker before admission/IPC. It never executes using a replacement token. Raw traces preserve original bytes; valid strings are untouched. SDK metadata validation precedes the filter, and response generation/version fields remain SDK-owned. A direct typed-call decoding catch also remains at the argument boundary.

[Pinned source records](mcp-tool-boundary-evidence/environment.json) identify C# SDK commit `6fa3825973949a9c4f0cd8af344e15a8db09dc35`: StreamServerTransport completion, McpSessionHandler draining, RequestHandlers conversion and McpServerImpl filter ordering. These were inspected as evidence; no SDK source was copied for this correction. SDK 2.2.0, dependency locks and the earlier attributed stdin adaptation are unchanged. TypeScript 1.26.0 remains precedent-only. No Revit/runtime/evaluator/installer/configuration/model changes.

## Reproduce

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Tools
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-Readiness.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-Readiness.ps1
```

The reviewer reproduction README is retained under `reviewer-source/README.md.txt`; its absolute local paths describe this machine. Preserve those sources unchanged when repeating the original probes. Task 7 recovery, installation, live samples/NC1/NC2, both desktop applications and full R3 review remain separate gates.
