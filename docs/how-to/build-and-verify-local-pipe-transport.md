# Build and verify the Windows-local connection

Agent companion to the [participant story](../tutorials/09-connect-the-bridge-processes.md). Follow root/project AGENTS, approved spec/plan revision 3, [research/provenance](../research/revit-local-pipe-transport.md) and the [Task 3 record](../../projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md). Task 2 was [independently closed](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md); this does not close Task 3 or full R3.

## Source and ownership

Paths are relative to `projects/revit-readiness/`.

| Component | Responsibility |
|---|---|
| `src/bridge-contract/BridgeSessionDescriptor.cs` | Version, PID, process-start UTC, Windows session, logon SID, random bridge-session ID and exact pipe name |
| Runtime `BridgeIdentity.cs`, `NativeWindows.cs` | Actual process/token/peer identity; protected descriptor creation and exclusive compare/delete; original Win32 interop |
| `SessionCatalog.cs` | Atomic publication at `%LOCALAPPDATA%\BimAgentLab\RevitBridge\sessions`, bounded descriptor reads, ACL and live-identity validation, explicit selection, ownership-aware removal |
| `LocalPipeFactory.cs` | Overlapped byte-mode native pipes, eight-instance cap, intended-logon DACL, remote-client rejection and SafePipeHandle transfer |
| `PipeEndpoint.cs` | Eight accept workers, one five-second frame timer, one receive budget, disconnect monitoring, submission, publication, connection cleanup and deferred lifetime disposal |
| `PipeBridgeClient.cs`, `BridgeClientProvider.cs` | Two-second connect within the existing lifetime, actual server identity, correlated replies, lazy single binding and no replay/retargeting |
| `tests/bridge-tests/Pipe*.cs` | Actual Windows integration and child-process tests; synthetic executor only |
| `tests/verify_pipe_evidence.py` | Independent completeness/timing checks on retained test output |

No new package is added. Both libraries remain net48/net10.0; the runtime/test assemblies declare Windows platform support for platform analysis. Contract types retain no Autodesk or SDK dependency. BridgeJson adds the bounded descriptor type to its existing allowlist; original-token string preservation and strict envelope validation remain intact.

Publish only from the owning process. A restricted temporary descriptor is flushed and renamed without overwriting; publish comes after all native pipe acquisitions. Discovery rejects malformed, stale, broadly accessible or foreign descriptors. Resolve returns a defensive descriptor or BridgeException. Zero candidates yields BridgeUnavailable; multiple valid candidates yield AmbiguousRevitInstance even when an explicit PID still matches multiple sessions.

The provider performs no selection at construction. Each first-call attempt uses the caller's original context and cancellation. Failed selection may be retried deliberately before binding. After binding, missing/restarted endpoints fail without re-enumerating or replaying. A new provider represents explicit reconfiguration/restart. Task 6 must map these errors through the real MCP tool path; these tests do not claim that mapping already exists.

## Lifetime rules

The frame timer starts from the recorded accept timestamp and never resets on progress. No partial frame reaches submission. A complete validated frame creates one receive budget using its UTC deadline and the original accept-time cap. Deadline rejection returns a correlated error under the still-bounded admission timer. Valid work borrows one RequestLifetime through submission, serialization and publication.

Connected I/O has one cancellation strategy: the owner's cancellation closes the pipe. Native reads/writes/waits receive CancellationToken.None to avoid competing framework callbacks; the owner still enforces frame/deadline/stop limits. Await I/O completion before releasing the network worker. Do not remove the owner-close registrations because the operation calls use None. Client connect uses its two-second bound plus the caller's existing cancellation; it does not create a new request budget.

Disconnect or extra post-frame input cancels the request. A canceled Response does not mean the callback is Quiesced. Close the connection and free its slot promptly, then defer RequestLifetime disposal through DispatchTicket.Quiesced. Stop waits for network workers, not unfinished Revit callbacks. Cleanup failure remains observable and a later StopAsync can retry descriptor removal. Changed descriptor bytes remain intact. A restart uses a new endpoint/session identity.

## Reproduce

From repository root on Windows:

```powershell
git status --short --branch
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Pipe
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
python projects/revit-readiness/tests/verify_pipe_evidence.py projects/revit-readiness/docs/verification/cancellation-progress-evidence/pipe-output.txt
```

Pipe runs 53 named cases on each target, including a net48 child serving simultaneous clients and a response above 8192 bytes. Each target fills all eight slots separately with no bytes, partial headers, partial bodies and trickles. A ninth connection is denied until capacity recovers. Capture accept/close ticks and frequency; the independent checker recomputes durations, requires every connection and successful successor, and rejects incomplete evidence. The production cap is five seconds; the spec permits one additional second of test scheduling tolerance.

The tests use internal catalog-root injection under ignored `samples/local/`, not the live discovery directory. Output and working artifacts remain ignored until selected, reviewed and copied into a verification record. Foreign-logon DACL denial is a local access-control test, not an actual second-user session. A remote machine and separate-logon client were not tested. Do not create accounts or alter security policy to manufacture them.

All retains 33 foundation cases per target, bilateral/Python exchange, MCP protocol/stream/settings and SDK/wire regressions including strict July and legacy rejection. Readiness retains 18 core, seven Excel and process-isolation checks. None installs an add-in, registers a client, accesses Revit or modifies a model.

## Handoff

Task 4 must implement the real bounded dispatcher and verify cancellation at its own execution boundaries. Task 5 supplies the Revit adapter; Task 6 supplies production MCP tools. Preserve RequestLifetime/DispatchTicket responsibilities, actual-client A8, all three principal sample cases and NC1/NC2. Use the [Task 3 review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md) to challenge this increment before treating its evidence as independently reviewed.

## Cleanup after exceptional completion

The [peer-exit review](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) found a missing exceptional-completion path. `NativeWindows.ValidateClient` normalizes ArgumentException only around the connected peer's process lookup; that connection is rejected and its worker returns to admission. Do not treat a disappeared peer as a reason to permanently reduce capacity, and do not globally swallow programming exceptions.

Shutdown drains network workers whether they succeed or fault, attempts owned-descriptor removal, disposes the stop source, then reports retained cancellation/worker/cleanup errors. Multiple errors remain distinguishable. A failed cleanup can be retried without canceling a disposed source. A repeated stop after an unexpected worker fault can still report that original fault even though cleanup is complete; error reporting and resource cleanup are separate outcomes. Peer disappearance itself is recoverable and does not fault shutdown.

The earlier correction handled a disconnect monitor fault caused by a throwing borrowed cancellation callback. The later deadline correction contains that callback failure inside RequestLifetime and reports it separately. Register lifetime cleanup in finally around awaiting that monitor. The endpoint's network completion still must not dispose a lifetime borrowed by an unfinished callback; DisposeAfterQuiescenceAsync retains that responsibility.

`PipeReviewTests.cs` adds five regressions to the existing suite: real child exit at the observer barrier, worker fault alone, worker fault plus locked descriptor, and throwing cancellation callbacks during stop/disconnect. Prove all eight replacement connections concurrently; one successful call alone can hide a dead worker. Resolve the replacement with and without explicit PID and call through the actual provider. Preserve worker/callback error identities while testing that cleanup completed.

The [peer-exit correction evidence](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) had 32 cases per target; the subsequent deadline correction had 38. Both findings are independently closed. The current held-callback correction below has 53 cases per target and awaits its own follow-up. Historical timing captures require their explicit expected case counts; never infer expectations from the capture being checked. Task 4 remains unstarted.


## Contain cancellation at every origin

The deadline P1 correction is [independently closed](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md) at b334385. Its six child regressions remain. The subsequent held-callback P2 showed that contained failures and retained sources did not prove network progress. The earlier synchronous CancelOwned implementation is superseded by the execution model below; do not copy that historical mechanism into new work.

## Prove progress separately from safety

Read the [execution/waiting model](../explanation/bridge-execution-and-waiting.md), committed at 373e5a8 before production changes. It specifies five separate facts, origin/thread/lock paths, allowed waits and a regression matrix. The [latest evidence](../../projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md) and [review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md) distinguish implementation proof from independent closure.

RequestLifetime marks authoritative cancellation state synchronously. Context.ThrowIfInactive uses that state and the same original budget; Context.IsCancellationRequested supports immediate polling. The public Cancellation token provides asynchronous cooperative executor notification. Runtime transport uses a PRIVATE cancellation source for response wakeup, pipe close, connect and provider-binding waits. Do not register executor callbacks on it or infer immediate state from notification completion. Queue cancellation must settle its completion source from authoritative state (for example, parameterless TrySetCanceled); do not require the public notification token to be canceled first, as Task.FromCanceled(token) would.

One owned background thread per canceled lifetime invokes borrowed callbacks without holding either gate, contains their errors and reports locally. Repeated origins share the same notification completion. StopAsync publishes its shared task under the endpoint gate, releases the gate, signals trusted transport operations and drains network workers. Disconnect's monitor performs only immediate signaling. Neither path waits for executor Quiesced or notification completion.

RetainedRequestSlot bounds admitted endpoint lifetimes at eight across all endpoints sharing the runtime/AppDomain, including stopped endpoints. Acquire before dispatch; release only after physical lifetime disposal. Capacity exhaustion returns correlated retryable Busy, without queuing or dispatching another request. Network slots are a separate bound. Deferred retirement waits for executor Quiesced, the original deadline task, notification and origin registrations. Exceptional Quiesced is terminal too: retain/report the error and release the permit after successful disposal. Failed physical disposal retains its permit and emits LifetimeCleanupFailed. A callback that never returns keeps its permit; never abort it or dispose its resources to manufacture progress. Standalone lifetime owners must impose their own admission bound.

CancellationCallbackFailed and LifetimeCleanupFailed are local diagnostics with request/connection identity. The first error is retained independently of mutable event data; all errors are emitted. Stop reports known failures after network/descriptor cleanup; a later Stop exposes failures arriving after an earlier successful stop. An expired client never receives a late result or stack trace. Trusted queue admission and diagnostic sinks must return promptly. The future Revit dispatcher must use private runtime signaling for its bounded control operations and keep actual executor work behind ExternalEvent.

From the project directory after the dual-target build:

```powershell
dotnet build tests/bridge-tests/BimAgentLab.Bridge.Tests.csproj -c Release -p:RestoreLockedMode=true
& ./tests/bridge-tests/bin/Release/net48/BimAgentLab.Bridge.Tests.exe --progress-review
& ./tests/bridge-tests/bin/Release/net10.0/BimAgentLab.Bridge.Tests.exe --progress-review
```

Fifteen child cases cross actual stop/disconnect/deadline paths with held-return/held-throw callbacks and Quiesced before/after network completion, then cover capacity/restart, client cancellation and exceptional Quiesced. Every progress assertion happens BEFORE releasing the callback. Require task return, network shutdown, owned-descriptor removal, all eight replacement connections where applicable, unchanged budget, no late success, eventual disposal and retained errors. Reject child timeouts, nonzero exits and incomplete proof markers.

Pipe/All now run 53 cases per target. The timing checker validates 64 stalled connections; verify_cancellation_progress.py separately requires the full held-callback matrix and capacity evidence. Corrupt copies are negative controls. Historical timing captures require explicit --expected-cases 27, 32 or 38; preserve their bytes. Full All retains foundation/MCP/strict July checks and readiness remains separate. No package or normative acceptance-policy change was needed. Task 4 is unstarted; independent P2 follow-up is pending.
