# Build and verify the production MCP readiness tools

Read the [approved plan, Task 6](../superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-6-two-production-mcp-tools-with-intact-evidence), [execution/waiting extension](../explanation/bridge-execution-and-waiting.md#task-6-mcp-tool-lifetime-and-permitted-waits), [participant story](../tutorials/12-expose-readiness-to-an-agent.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-readiness-tools.md). The extension was committed at `a8cab35` before production tool implementation.

## Source and provenance

`src/mcp-server/Program.cs` is the executable entry; `ReadinessTools.cs` registers exactly two tools using the existing SDK host. It references the existing net10.0 bridge runtime/contract, never the net48 add-in, evaluator or Autodesk API. Runtime internal access lets the server reuse the existing retained-slot and disposal-completion mechanisms. Test-only internal access permits the actual SessionCatalog selection algorithm with a controlled validated-descriptor enumerator. No runtime algorithm was copied or changed to enable that seam.

SDK **2.2.0** and its dependency versions/hashes remain unchanged. Lockfiles add only lab Project entries. The prior [dependency research](../research/mcp-sdk-dependencies.md) remains the adoption record, including its dated observation of 2.3.0; this task is not a new latest-version claim. MCP parsing, dispatch and negotiation come from the official SDK. The previously attributed C# 2.2.0 stdin helper is unchanged. New tool/entry/test code is original lab work; TypeScript SDK 1.26.0 remains precedent-only. No new third-party package or source was adopted.

## Tool and lifetime boundaries

- `revit_get_readiness_context` accepts an empty object. It returns camel-case context/checklist/model fields plus contract/request/session/hash identity.
- `revit_check_required_type_information` accepts only a nonblank `contextToken`. It returns the full original report tree under `report`, preserving casing, nulls, numbers, false flags and all arrays. Both coverage indicators must be present as booleans. The server does not re-evaluate BIM rules.
- Both declare read-only intent. That annotation is descriptive; actual read-only enforcement remains the reviewed Revit query path. Model text is data, not instructions. Neither tool produces an export.
- The executable accepts only optional `--revit-pid <positive-int>` and `--wire-evidence <new-directory>`. It uses SDK-default compatibility; strict July is a test policy in the same host path. There is no production synthetic backend or protocol selector. Discovery does not select Revit; the provider binds lazily once and never retargets after restart.
- Admission creates one request ID and 30-second budget. Reserve retained capacity before constructing the lifetime. One runtime-wide pool bounds eight unfinished requests, including those awaiting callback retirement. Full capacity returns Busy without starting another lifetime.
- Recheck the same context after IPC, after JSON mapping, and immediately before returning success. Explicit SDK/host cancellation propagates; expired transport cancellation maps to DeadlineExceeded. Expected bridge errors preserve code/message/retryability with this request ID; unexpected failures return a sanitized InternalError. Failures have no report or stack trace.
- Handler completion waits for the production client's SendAsync to unwind network work, never for borrowed callbacks. Deferred retirement disposes the lifetime, observes disposal completion and releases the slot only on success. Callback/cleanup diagnostics contain request ID and exception type, not model data. Failed physical cleanup retains capacity.

## Reproduce verification

From the repository root on Windows with the documented .NET/Revit API prerequisites:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Tools
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-Readiness.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-Readiness.ps1
```

Tools builds locked dependencies and runs **59 cases**. Seven SDK stdio exchanges compare original synthetic report trees on June/November/July, including strict July. Two further cases reject older revisions under strict policy. Selection tests use the production provider and actual SessionCatalog.Resolve; no/multiple candidates, retry before binding, concurrent first calls and restart without retargeting are exercised. Positive binding uses real Windows pipes with a synthetic endpoint, not Revit. The production executable itself is tested with an absent PID.

Negative result tests reject crossed identities, malformed hashes/JSON, missing completeness and mixed success/error envelopes. Domain failures preserve their contract. Direct handler and SDK stdio tests interrupt after IPC, with a monotonic fake clock that would expose a reset budget. An intentional final-guard mutation causes four failures and is restored before the final suite.

Eight held callbacks prove prompt handler cancellation, Busy across new tools instances, and eventual recovery of all eight admissions. Actual Windows subprocesses prove the production tools' host can stop with callbacks held and stdin still open, with tracing both off and on; the process exits naturally after the test releases callbacks. A forced kill exists only as failed-test watchdog cleanup. The earlier erroneous EOF-wait test and its diagnostic markers are preserved transparently.

Captures under ignored `samples/local/mcp-tools-tests` contain model-like synthetic strings only. Retained traces and hashes are linked from the verification record. These tests establish application transport/mapping behavior, not actual model results. Do not modify expected reports to match returned values.

## Remaining gates

Obtain the [focused Task 6 review](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-review.md), implement Task 7 recovery before installing, then verify live Revit lifecycle and all Task 8 cases in both desktops. Do not use the old installer for this increment, change desktop registration, or treat the separate probe as production acceptance. No new spec approval is needed for the already planned tools; acceptance requirements and compatibility policy remain unchanged.

## Task 6 review follow-up: complete argument contract

The [latest independent follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-argument-contract-review.md) closes disconnect handling at 332934c and leaves the full argument contract under review. Read the [boundary model](../explanation/readiness-argument-boundary.md), written before the production correction, and [fresh verification](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-argument-contract.md). Earlier leaf-specific filter instructions are superseded; historical evidence remains unchanged.

McpHost still races SDK RunAsync against MessageReader.Completion, canceling the existing token before draining handlers on EOF/read failure. This independently closed code is unchanged. Exceptional input remains observable and borrowed callbacks remain outside shutdown's wait path.

ReadinessArgumentBoundary.Prepare owns the full two-tool policy before SDK typed conversion. Context acquisition accepts an empty object or omitted optional arguments; explicit null is rejected. Checking information accepts exactly one ordinally named contextToken with a nonblank valid Unicode string that fits the existing complete serialized bridge-request limit. JsonNode Count/enumeration/name access and string decoding can fail; contain those representation failures inside preparation. Never rely on inherited JsonNode case-insensitive lookup options for application field names.

The filter creates one ID/budget and stores a private PreparedReadinessCall in per-message SDK Items. Accepted outcomes hold the validated BridgeRequest and unchanged original budget; caller arguments are untouched. Rejected outcomes hold only safe correlation/budget state and no caller tree. Replace the ENTIRE rejected arguments member with a fresh empty JsonObject; never clone, repair, serialize or traverse its rejected descendants afterward. CallAsync consumes the owned result, rejecting before retained-slot acquisition or IPC. Direct typed callers use the same preparation policy. SDK metadata/envelope validation precedes the filter; protocol dispatch and version-specific result construction remain SDK-owned. Unknown operation/envelope failures and actual downstream faults are outside the argument catch.

Why the handoff is safe: an accepted tree is empty or contains one exactly named decoded string that already passed real request serialization. There are no unexamined application members left for conversion. A rejected tree is detached; conversion and response creation see only safe owned data. Mutation is confined to rejected application arguments, preserving the MCP envelope. The raw trace still contains original input bytes.

Tools/All now run 59 cases. Six new matrix exchanges run 310 generated argument cases each across all three revisions and both trace modes: 299 rejections and 11 accepted controls, 1,860 total. Each rejected call requires correlated InvalidRequest and zero calls into the real pipe endpoint's counting synthetic executor. Each accepted call requires one admission and exact token preservation. Cases cover omitted/null/every JSON kind, wrong/missing/extra/duplicate/escaped/undecodable names, nested malformed data, paired defects in both orders, invalid Unicode and size, and valid calls afterward. A separate unknown-method control per exchange checks SDK ownership. Three handoff cases prove accepted subtree identity, one original admission/budget including conversion delay, zero bridge calls on expiry, and unchanged propagation of a downstream InvalidOperationException.

The original 50 cases, unchanged original and supplementary reviewer reproducers, strict July and the other regressions remain required. Read the failed records too: 221 matrix failures per exchange before correction; then one case per exchange exposed SDK case-insensitive lookup; then the corrected matrix passed. Do not add a field-specific branch when a new case reveals an incomplete invariant.

Provenance remains original application code using the official SDK's existing filter API. No new package, lock change, protocol implementation or copied upstream source. Reviewer sources are attributed evidence only. Task 6 remains open for independent validation follow-up; Task 7, installation and live Revit/desktop acceptance remain ahead.
