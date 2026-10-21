# Task 6: complete argument-contract verification

**Subsequent independent disposition:** [T6-02 and Task 6 closed at b4cdb1f](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-closure.md); shutdown closure stands. The author-verification record below remains historical. The reviewer separately verified the retained evidence and additional concurrent controls; installation and live acceptance remain outstanding.

Baseline: `332934cb144148a9d8af79fe57cd745ac110f6a0`. Author verification on Windows, 2026-10-21. [Independent feedback and author dispositions](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-argument-contract-review.md) close disconnect T6-01 at the baseline and keep T6-02 open. **These implementation checks do not independently close Task 6.**

## Contract and scope

The [boundary explanation](../../../../docs/explanation/readiness-argument-boundary.md) was written before production changes. It explains why each accepted path has no unsafe unexamined application data, and why each rejected path continues without its original argument tree. The only production changes are ReadinessTools and the new ReadinessArgumentBoundary helper. Existing shutdown, runtime, evaluator, Revit, SDK 2.2.0, package locks and desktop settings are unchanged.

Prepared acceptance owns the actual validated bridge request and its original ID/budget before SDK typed conversion. Accepted caller arguments are unchanged. Rejection owns only safe correlation/budget state, replaces the complete arguments member with an empty object, and returns InvalidRequest before retained admission/IPC. SDK protocol/envelope handling remains outside the application gate. No SDK source was copied; the previously attributed stdin helper is unchanged. Reviewer source is retained as attributed evidence only.

## Results

| Verification | Observed result |
|---|---|
| Unchanged supplementary reviewer probe before edits | Five of eight cases fail with JSON-RPC internal error; process survives |
| New matrix against unchanged code | Each exchange: 310 cases, 221 failures; all six matrix groups fail while original 50 tests pass |
| First complete-gate implementation | One remaining failure per exchange: inherited case-insensitive JsonNode lookup accepts ContextToken |
| Corrected exact property-name validation | All 310 cases pass in each of six exchanges |
| Full final Tools suite | 59 pass: original 50 plus six matrix exchanges and three handoff tests |
| Application matrix totals | 1,860 calls: 1,794 rejected with zero bridge admissions; 66 accepted with one admission and exact token preservation |
| SDK controls | One unknown-method call per exchange remains JSON-RPC -32601 with zero bridge admissions |
| Handoff tests | Accepted subtree identity and one admission; deadline expires during handoff without restarting budget or entering bridge; downstream InvalidOperationException propagates unchanged |
| Unchanged reviewer probes after correction | Supplementary eight-case shape probe and original six-case Unicode, EOF off/on and explicit cancellation probes all pass |
| Full MCP/bridge regression | Tools 59; Dispatch 87 net48 / 82 net10.0 plus ten adapter handoffs; Pipe 53 and foundation 33 per runtime; protocol 19 C# / ten Python / four SDK exchanges, including strict July |
| Readiness/Excel/isolation and Revit API build | 18 evaluator and seven Excel checks plus process isolation pass; real Autodesk-reference build passes with zero warnings/errors |

The matrix runs the actual production executable against a real Windows pipe with a counting synthetic executor. Each reply is checked for JSON-RPC correlation, structured/text equality, request identity, correct error/success state, no report on failure, and the executor call-count delta. Valid controls after the defects prove the process and subsequent requests still work. Tracing is crossed off/on with June 2025, November 2025 and July 2026. The synthetic endpoint is not Revit acceptance.

The generator crosses both tools with omitted arguments and every JSON kind; missing/extra/wrong/duplicate/escaped/undecodable properties; malformed scalar and nested values; two simultaneous defects in both orders; blank, oversized and invalid Unicode strings; and valid Unicode, escaped control characters, date-shaped strings, surrogate pairs and large allowed values. Expected outcomes are declared by construction from the approved schemas, not read from the production validator. Explicit null is rejected; omitted optional arguments retain existing no-argument context-acquisition behavior.

## Retained evidence

[Inventory](mcp-argument-contract-evidence/inventory.json) records exact hashes and lengths for 62 new files. All 500 retained matching-schema evidence hashes were checked, including the 438 earlier files. [Environment and counts](mcp-argument-contract-evidence/environment.json) distinguish scope from live acceptance. Red logs and summaries retain the failures, including the case-insensitive lookup defect discovered by the new tests. The unchanged reviewer source and outputs are retained separately.

Bulk sender/receiver transcripts and actual trace files are losslessly gzip-compressed to keep the teaching repository small. [Uncompressed lengths and hashes](mcp-argument-contract-evidence/transcripts.json) verify their original bytes; matrix summaries remain plain JSON. Sender/receiver logs normalize line endings; files under each `wire/` subdirectory retain actual trace bytes. They contain synthetic test text only.

[Author capture checks](mcp-argument-contract-evidence/capture-check.json) separately parse all 1,860 application replies and six method errors, compare their correlation/content against the declared matrix oracle and call-count observations, and verify all 18 decompressed transcript hashes. This is author verification, not another independent review.

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Tools
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-Readiness.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-Readiness.ps1
```

To inspect a compressed transcript, use Python's standard-library `gzip.open(path, 'rt', encoding='utf-8')`; no new dependency is required. Matrix reproduction output lives under ignored `samples/local/mcp-tools-tests/argument-contract-*`.

Installation, live Revit scheduling/shutdown, the three samples/NC1/NC2, both desktop applications' A8 and full R3 remain unverified. Task 7 has not started. Request independent T6-02 follow-up against the correction commit before proceeding.
