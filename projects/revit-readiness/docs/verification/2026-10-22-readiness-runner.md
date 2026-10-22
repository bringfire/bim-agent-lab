# Reusable readiness capture runner

Date: 2026-10-22. Scope: the first requested follow-up increment only. Baseline live checkpoint: `2b15e3459d4004644ae3cf6275166ff0b0563f0c`. Revit is closed. No production bridge, SDK package, installed file, desktop configuration or sample was changed.

## Implementation and ownership

`scripts/Invoke-ReadinessMcp.ps1` calls `scripts/invoke_readiness_mcp.py`. The helper consumes the existing official-SDK readiness driver, including its already implemented pause/release facility, and the independent five-case comparator. Case verification remains outside protocol implementation, following the Task 8 ruling. No package, SDK implementation or TypeScript SDK 1.26.0 code was added or copied.

Preflight validates independent expectations and hashes before creating output or launching a client. A new attempt owns its frozen expectation, prepared record, release file, child process, logs and nested capture directory. Only a validated context releases the readiness call. Input bytes are checked before launch, before release and after capture/comparison. The same monotonic budget applies through context acquisition, SDK completion and final publication. SDK transport completion precedes the independent success marker. Failure evidence must remain reachable even if process cleanup fails. Existing attempts are never overwritten or deleted.

Timeout cleanup targets only this invocation's SDK client. Its server receives transport EOF and owns cancellation of admitted work. This runner does not claim synchronous completion of Revit callbacks or replace the remaining host lifetime/shutdown tests. Synchronous file hashing/comparison can finish after the deadline, but cannot publish success after it.

## Test boundaries

`test_mcp_readiness_runner.py` runs a labelled subprocess producer (`runner_fixture_client.py`) in disposable directories. It preserves real orchestration, file ownership, process waits and the independent comparator. The producer adapts retained SDK report fixtures to synthetic paths/times and uses fake model bytes; these are explicitly **not** additional live MCP/Revit captures.

The ten test methods cover five expected outcomes; changed model/checklist/oracle/PID/proof and future preparation rejection; unconfirmed NC1; preservation of existing attempts; wrong document/session/hash, failed context and changed input before read; failed SDK exit, corrupt report, missing wire/completion, false completion, changed input after read and stale report; held-client timeout/retirement; verification exceeding the original budget; failure evidence despite a cleanup exception; and both explicit and default PowerShell paths with spaces.

Initial tests failed because the runner did not exist. Additional targeted tests demonstrated actual failures before their corrections: comparison could publish after its original budget, the normal PowerShell default used `$PSScriptRoot` before it was available, and a cleanup exception could prevent writing the primary failure record. The corrected runner preserves the original budget, resolves defaults in the script body, and retains both failure causes. No claim of live positive wrapper acceptance follows from these offline tests.

## Verified results and limits

[Retained logs, actual unavailable-host capture and hashes](readiness-runner-evidence/2026-10-22/manifest.json) establish:

- Ten focused runner test methods pass, including five positive synthetic cases and the failure/ownership controls above. The full Protocol discovery run includes them.
- The actual PowerShell command with default paths launches the real official-SDK client against the unchanged installed server while Revit is closed. It receives structured `BridgeUnavailable`, returns a nonzero exit, keeps the prepared record/context/wire/logs/failure, and produces neither a readiness call nor a successful comparison. This is an expected-failure control, not a positive model check. A prior default-path attempt failed before launch and is retained separately.
- `Test-McpBridge.ps1 -Suite All` passes: Deployment 41, Tools 72, Dispatch 87/net48 and 82/net10, adapter handoff ten, Pipe 53 per runtime, foundation 33 per runtime, independent exchange/timing checks, Protocol and its 34 Python test methods, compatible revisions, strict July and legacy rejection. Four CA1416 platform-analysis warnings in unchanged `ReadinessArgumentBoundary.cs` are retained; this was not a warning-free build.
- `Test-Readiness.ps1` passes 18 core tests, seven Excel tests and process isolation.
- Six focused Python suites pass. All 15 retained live report comparisons and both desktop transition/recovery comparisons pass again; their 214 evidence hashes remain unchanged. These are replays of earlier live captures, not newly executed desktop runs.

No fresh positive Revit run through this new wrapper is claimed. Revit remains closed; existing desktop configurations still refer to the historical PID until the next deliberately prepared session. Broader handoff refinement, remaining host acceptance and R3 are outside this first increment.

Participant layer: [how the author directed this increment](../../../../docs/tutorials/14-test-readiness-through-desktop-agents.md). Agent layer: [command, preparation and failure handling](../../../../docs/how-to/verify-readiness-through-desktop-agents.md#run-one-prepared-case-through-the-sdk). Earlier live evidence remains in the [sample matrix](mcp-readiness.md); remaining Task 7/Task 8 consolidation and R3 are not closed by this wrapper.
