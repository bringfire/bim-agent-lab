# Build and verify the Revit readiness adapter

Companion to the [participant story](../tutorials/11-connect-the-readiness-reader-to-revit.md). Follow approved [Task 5](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) and the [pre-implementation execution model](../explanation/revit-adapter-lifetime.md), committed at b44ea23. Baseline is independently closed Task 4, 24a72cf. Native execution remains selected.

## Production boundaries

| Source under `projects/revit-readiness/src/` | Responsibility |
|---|---|
| `bridge-runtime/HostBridgeSession.cs` | API-thread lifecycle, acquisition/unwind, production queue/store ownership, response observation through Quiesced, nonblocking network stop |
| `bridge-runtime/ApiEventLease.cs` | Short ownership lock around native Raise borrowing; disposal only by API owner after borrowers return, with no UI wait |
| `bridge-runtime/ReadinessExecutionFlow.cs` | Identical execution context through prepare/read/serialize; guards at all four boundaries; revoke unpublished leases |
| `bridge-runtime/ChecklistSnapshot.cs` | Four MiB bounded capture including growth check; SHA-256 and strict UTF-8 from identical retained bytes |
| `revit-addin/BridgeApplication.cs`, `BridgeCommands.cs` | Explicit application lifecycle and Start/Stop; no listener on load; Idling, close and Save As hooks |
| `revit-addin/ReadinessExternalEventHandler.cs` | Valid API-context drain; four-request batch maximum remains in the reviewed queue |
| `revit-addin/RevitContextCatalog.cs` | Actual active saved host project; random open-document lifetime, canonical paths, sidecar schema/project/file/level/phase validation |
| `revit-addin/RevitReadinessExecutor.cs` | Calls existing RequiredInformationReader directly through the guarded flow; no command Execute, transaction, dialog or export |
| `revit-addin/ReadinessPayloadCodec.cs` | Actual net48 checklist/report conversion, isolated from Autodesk types for executable offline conversion tests |

The extra lifecycle, snapshot and codec helpers refine the plan's file list to test production ownership/conversion without a fake implementation. No protocol or acceptance requirement changed. PipeEndpoint only adds the lifecycle interface; its reviewed transport behavior is unchanged.

The API owner is the Revit thread that creates the session/catalog. Thread checks catch misuse; they are not proof that an arbitrary same-thread call has a valid API context. The actual callers are ExternalCommand, ExternalEvent and lifecycle/Idling callbacks. Queue/transport receive only plain data and execution contexts; Document references stay inside the API catalog and synchronous prepared object.

## Preparation, identity and publication

Capture the active document at execution, derive its sidecar path, read bounded bytes once, hash and parse that snapshot, validate declared identities, then validate the token binding. Revit Document equality identifies the same currently open document; invalid wrappers are removed. Save As explicitly retires the catalog identity, including a later return to the old path. Value edits do not invalidate identity and the reader runs freshly.

The original RequestBudget and immediate context invalidation remain authoritative. The synchronous flow checks entry, immediately after preparation, after read and after serialization. Queue publication still uses its reviewed atomic gate. A running read can finish but cannot publish late success. No executor callback uses the private transport token.

Context creation checks before/after issuance and serialization. A production response task remains available through Quiesced; a canceled/faulted/error queue response revokes an unpublished lease even if cancellation occurs after the executor returns. Cleanup captures only the store and token, never a Document. A published response that the consumer never receives cannot be recalled reliably; its context expires normally.

Stop closes admission and clears tokens before stopping the endpoint. Network retirement does not wait for borrowed notifications. Event retirement prevents new raises and polls existing native borrowers on the API thread; Idling finishes disposal without blocking on them. During final Revit shutdown there may be no later API callback: if Raise is still borrowed, retain the event safely until process exit. This explicit host assumption remains in live acceptance, rather than being reported as proven by a fake event.

## Dependencies and provenance

No new NuGet package, copied upstream source or SDK upgrade. Add-in locks add only local Runtime/Contract project edges. The test project's net48 target also references the add-in to run the actual codec; its net10 target does not load the add-in/core. This means compiling these tests now requires the recorded installed Revit API references, though no running Revit process is needed. Autodesk DLLs remain Private=false and are not copied or redistributed.

Local primary API evidence: installed RevitAPIUI.xml documents ExternalEvent.Raise and all four outcomes, and UIControlledApplication.Idling as valid API context; RevitAPI.xml documents Document.Equals/GetHashCode, ControlledApplication.DocumentSavedAs, and RevitAPIPostDocEventArgs.Document. Hashes and paths are in the verification inventory. These are version-specific source observations, not runtime proof. Existing dependency/license research and the SDK 2.2.0 / TypeScript 1.26.0 provenance distinctions remain unchanged.

## Reproduce

From repository root in PowerShell:

```powershell
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Dispatch
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
& ./projects/revit-readiness/scripts/Build-Readiness.ps1
git diff --check
```

Dispatch includes production flow/lifecycle/snapshot cases on both runtimes and five actual-codec cases only on net48. All retains real pipe tests, independent evidence checks, bilateral framing, SDK/stdio compatibility and strict July regression. Readiness retains 18 core, seven Excel and exporter isolation checks. Inspect [fresh verification](../../projects/revit-readiness/docs/verification/2026-10-21-revit-adapter.md) instead of assuming historic pass counts are a current result.

Inspect source boundaries and all local lock changes. Do not install this increment through the old installer: Task 7 must deploy the application/commands and new runtime dependencies with durable recovery. Run the [live acceptance matrix](../../projects/revit-readiness/tests/mcp-host-acceptance.md) after CP1 and installation work. Task 6 production tools, Task 7 lifecycle/deployment evidence, Task 8 full report/client evidence and R3 remain ahead.
