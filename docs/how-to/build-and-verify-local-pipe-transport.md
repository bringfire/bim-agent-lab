# Build and verify the Windows-local connection

Agent companion to the [participant story](../tutorials/09-connect-the-bridge-processes.md). Follow root/project AGENTS, approved spec/plan revision 3, [research/provenance](../research/revit-local-pipe-transport.md) and the [Task 3 record](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md). Task 2 was [independently closed](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md); this does not close Task 3 or full R3.

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
python projects/revit-readiness/tests/verify_pipe_evidence.py projects/revit-readiness/docs/verification/local-pipe-transport-review-evidence/pipe-output.txt
```

Pipe runs 32 named cases on each target, including a net48 child serving simultaneous clients and a response above 8192 bytes. Each target fills all eight slots separately with no bytes, partial headers, partial bodies and trickles. A ninth connection is denied until capacity recovers. Capture accept/close ticks and frequency; the independent checker recomputes durations, requires every connection and successful successor, and rejects incomplete evidence. The production cap is five seconds; the spec permits one additional second of test scheduling tolerance.

The tests use internal catalog-root injection under ignored `samples/local/`, not the live discovery directory. Output and working artifacts remain ignored until selected, reviewed and copied into a verification record. Foreign-logon DACL denial is a local access-control test, not an actual second-user session. A remote machine and separate-logon client were not tested. Do not create accounts or alter security policy to manufacture them.

All retains 33 foundation cases per target, bilateral/Python exchange, MCP protocol/stream/settings and SDK/wire regressions including strict July and legacy rejection. Readiness retains 18 core, seven Excel and process-isolation checks. None installs an add-in, registers a client, accesses Revit or modifies a model.

## Handoff

Task 4 must implement the real bounded dispatcher and verify cancellation at its own execution boundaries. Task 5 supplies the Revit adapter; Task 6 supplies production MCP tools. Preserve RequestLifetime/DispatchTicket responsibilities, actual-client A8, all three principal sample cases and NC1/NC2. Use the [Task 3 review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) to challenge this increment before treating its evidence as independently reviewed.

## Cleanup after exceptional completion

The [peer-exit review](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) found a missing exceptional-completion path. `NativeWindows.ValidateClient` normalizes ArgumentException only around the connected peer's process lookup; that connection is rejected and its worker returns to admission. Do not treat a disappeared peer as a reason to permanently reduce capacity, and do not globally swallow programming exceptions.

Shutdown drains network workers whether they succeed or fault, attempts owned-descriptor removal, disposes the stop source, then reports retained cancellation/worker/cleanup errors. Multiple errors remain distinguishable. A failed cleanup can be retried without canceling a disposed source. A repeated stop after an unexpected worker fault can still report that original fault even though cleanup is complete; error reporting and resource cleanup are separate outcomes. Peer disappearance itself is recoverable and does not fault shutdown.

The disconnect monitor can fault when a borrowed cancellation callback throws. Register lifetime cleanup in finally around awaiting that monitor. The endpoint's network completion still must not dispose a lifetime borrowed by an unfinished callback; DisposeAfterQuiescenceAsync retains that responsibility.

`PipeReviewTests.cs` adds five regressions to the existing suite: real child exit at the observer barrier, worker fault alone, worker fault plus locked descriptor, and throwing cancellation callbacks during stop/disconnect. Prove all eight replacement connections concurrently; one successful call alone can hide a dead worker. Resolve the replacement with and without explicit PID and call through the actual provider. Preserve worker/callback error identities while testing that cleanup completed.

The [corrected evidence](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) supersedes the original implementation's completeness claim. Current Pipe has 32 cases per target. To verify the unchanged original 27-case timing capture, pass `--expected-cases 27`; never infer an expected case count from the capture being checked. No normative spec change or new dependency was needed. Independent follow-up is pending and Task 4 remains unstarted.
