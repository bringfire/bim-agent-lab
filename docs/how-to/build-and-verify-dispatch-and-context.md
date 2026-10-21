# Build and verify dispatch and context lifetime

Task 4 follows [independent Task 3 closure](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-closure.md). Read the [participant story](../tutorials/10-coordinate-requests-and-model-context.md), [pre-implementation model](../explanation/dispatch-and-context-lifetime.md), approved [plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) and [current verification record](../../projects/revit-readiness/docs/verification/2026-10-21-dispatch-and-context.md). Native execution remains selected. No new package, SDK upgrade, copied precedent code or Autodesk reference is introduced by this increment.

## Source and interfaces

- [ContextBinding / ContextLease](../../projects/revit-readiness/src/bridge-contract/ContextBinding.cs): plain identity fields and opaque lease/UTC expiry. These are not new MCP arguments or wire operations.
- [DispatchQueue](../../projects/revit-readiness/src/bridge-runtime/DispatchQueue.cs): construct with the bridge session ID and a prompt `IDispatchSignal`. `Enqueue(Context)` returns `DispatchTicket`; `Drain(maxBatch, execute)` belongs to the host's valid execution context. `RearmIfNeeded` belongs to Idling and drain completion; `CancelAll` permanently closes this queue's admission.
- [ContextStore](../../projects/revit-readiness/src/bridge-runtime/ContextStore.cs): construct with bridge session ID and clock. Issue/Validate/Revoke/Clear own a bounded identity table, not a model snapshot.
- [BridgeExecutionContext](../../projects/revit-readiness/src/bridge-runtime/BridgeExecutionContext.cs) / [RequestLifetime](../../projects/revit-readiness/src/bridge-runtime/RequestLifetime.cs): original lifetime/cancellation semantics retained; a narrow internal publication operation now shares the lifetime activity gate.
- [Dispatch tests](../../projects/revit-readiness/tests/bridge-tests/DispatchTests.cs) / [context tests](../../projects/revit-readiness/tests/bridge-tests/ContextTests.cs): real queue/store/lifetime/pipe code, deterministic clocks and host-signal/API callback doubles. Fake host callbacks are not live Revit evidence.

## Execution obligations

Enqueue never invokes the supplied executor or waits for it. Four entries may wait; one Drain owns execution, including reentrant/concurrent Drain attempts. Even a larger batch argument executes at most four entries. Signal attempts are serialized outside the queue lock. Accepted/Pending retain work; Denied/TimedOut/throwing signals give correlated retryable Busy and release waiting entries. Drain generations prevent stale signal completion from claiming a newer queue. Idling retries pending work without a synchronous retry loop.

Cancellation registration contains only bounded bookkeeping on the private transport signal. Public-token notification may be held without delaying response cancellation. Register before making an entry executable; finish admission before allowing Quiesced. Remove canceled waiting entries, but keep an executing entry physically owned until its callback finally and registration release. Dispose registrations outside the queue lock. Completion sources use asynchronous continuations. The queue never disposes a borrowed RequestLifetime.

Check the authoritative Context before invocation and publication. The final successful completion uses the same activity gate that marks cancellation; queue gate precedes activity gate, and cancellation releases activity gate before taking the queue gate through private bookkeeping. No reader/serialization/preparation/borrowed callback runs under the activity gate. A later cancellation can still invalidate network/MCP publication, which must retain their existing guards. Stop cannot replace an already canceled response with success. Late executor errors/results do not change a completed response.

Trusted clock, signal and diagnostic adapters must return promptly. A signal adapter must schedule a later host callback, not execute a reader inline. Arbitrarily blocked adapters are outside this admission guarantee. Task 5 must coordinate actual ExternalEvent lifetime/disposal and map every Revit Raise result.

## Context obligations

Issue snapshots complete binding fields, requires canonical absolute paths and SHA-256 hex, and verifies the store's session. The adapter derives these identities; callers never supply arbitrary model or checklist paths. IDs compare ordinally; canonical Windows paths and hex hashes ignore case. The store performs no file I/O or filesystem-alias resolution.

Use cryptographically random 256-bit tokens; retain at most 32 live leases. Monotonic expiry is exactly five minutes, with UTC expiry reported for display. Purge expired entries without evicting live ones. Unknown/revoked/expired tokens give ContextExpired; known-token identity mismatch gives ContextChanged without retargeting it. Revoke is idempotent and restores an unpublished lease's slot; Clear invalidates every token. Model values are not stored in the binding.

Task 5 must validate actual active-document/session/lifetime/path/checklist identity on the API thread, hash and parse the same bounded bytes, carry the same Context through preparation and the final read boundary, and revoke an unpublished context lease after interruption. Task 6 still owns standalone RequestLifetime admission limits. The endpoint's eight retained-request permits do not replace the queue's four-waiting/one-executing limit.

## Verify and hand off

From repository root:

```powershell
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Dispatch
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
```

Dispatch currently runs 46 cases per target. All also retains 53 pipe and 33 foundation cases per target, independent pipe evidence checks, bilateral exchange checks and MCP protocol/SDK regressions including strict July. Readiness retains core, Excel and process-isolation checks. The [evidence inventory](../../projects/revit-readiness/docs/verification/dispatch-context-evidence/inventory.json) preserves red/green results and a publication-gate mutation that failed on both targets. Check byte hashes before using a retained result as evidence; use fresh commands for a changed revision.

The [Task 4 packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md) requests independent review of admission/publication/retirement and context binding. Task 3 remains closed at its reviewed revision. Task 4 implementation verification is separate from its independent closure; Task 5, live Revit, desktop A8 and full R3 remain ahead.
