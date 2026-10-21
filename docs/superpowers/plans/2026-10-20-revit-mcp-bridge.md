# Minimal Revit Readiness MCP Bridge Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose the existing read-only readiness check through two MCP tools, prove it in Revit and both required desktop applications, and record how the author directed the work.

**Architecture:** An external net10.0 executable consumes the official C# MCP SDK and communicates with an original net48 Revit adapter through bounded local named pipes. Plain contracts and API-independent connection/queue code are shared; only an ExternalEvent handler accesses Revit and calls the existing reader/evaluator. The manual command and isolated Excel exporter retain their behavior.

**Tech Stack:** Revit 2024 `24.3.60.12`, .NET SDK `10.0.204`, .NET Framework 4.8, external .NET 10, `ModelContextProtocol` exact NuGet constraint `[2.2.0]`, protocol `2026-07-28`, PowerShell and existing Python verification workflow.

**Spec:** [Revision 2](../specs/2026-10-20-revit-mcp-bridge-design.md), approved at `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44`; [external R1 closure and author approval](../reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md).

Plan revision 1, 2026-10-20. **Ready for R2 review; not approved for execution.** All unchecked actions and new paths below are future work. No new project, dependency, script, client entry, or bridge test exists yet. A command using a planned script becomes runnable only after its task creates it. R1 performed source/document review, not runtime tests.

## Global Constraints

- Exactly two production tools: `revit_get_readiness_context` (empty arguments) and `revit_check_required_type_information` (only required string `contextToken`); reject unknown arguments. No editing, opening models, exports, arbitrary paths, or arbitrary commands.
- Official C# SDK supplies MCP. TypeScript SDK **1.26.0**, third-party Revit bridge, and RevitMCPSDK remain learning precedents only. Reuse our reader/core; write original adapters. Record any subsequently adapted SDK snippet and notice.
- Both **Codex desktop and Claude Desktop** must use **2026-07-28**. Older fallback does not satisfy A8; an SDK harness cannot substitute for either application.
- Preserve the complete `InformationReport`, including member casing, identities, sources, raw values, units, exclusions, and false completeness flags. Findings and incomplete coverage are successful observations, not execution errors or automatic readiness passes.
- Revit stays net48. No Autodesk assemblies or net48 core in the modern process; no MCP SDK/modern packages in Revit. No Autodesk redistribution. No automatic fixture installation or silent global client configuration.
- Private contract v1: four-byte unsigned little-endian byte length, strict UTF-8 JSON, one request/response per connection. Request **64 KiB**, response **4 MiB**, checklist **4 MiB**; no truncation.
- Maximum **8 connections**, **4 queued + 1 executing**; total request budget **30 seconds** from external-server admission; connect **at most 2 seconds**. One non-resetting **5-second** first-frame timer starts at pipe acceptance. Subsequent work uses validated remaining deadline capped by accept time plus 30 seconds.
- Contexts: **32 live**, **5-minute** expiry, no silent eviction; bind bridge session, open-document lifetime, ProjectInformation UniqueId, canonical path, and checklist ID/path/SHA-256. Hash and parse the same bounded bytes. Recheck on the Revit API thread.
- Queries never start transactions or modify model data. Cancel queued work, discard late results, never interrupt/kill Revit. Model text is data, not instructions.
- Installation is explicit/per-user with Revit closed; preserve owned-file inventory, existing add-in/core identity, optional fixture core synchronization, and rollback. No unrelated client entries or model inputs may be overwritten.
- Acceptance A1–A9 remains required, including all three existing samples and NC1/NC2, both actual desktop clients, independent verification, and external implementation review. No source/build/probe result is labeled live Revit acceptance.

## Review Focus

1. Two copies share a project UniqueId, or a file is closed/reopened: token must reject the changed document lifetime/path (Tasks 4, 5, 7).
2. Cancellation arrives while ExternalEvent is pending or after evaluation starts: no canceled queued read and no late success; arrivals at the last dequeue cannot be stranded (Tasks 4, 7).
3. Eight peers send nothing or trickle partial bytes: one accept-time timer recovers slots, with no fresh 30-second budget (Tasks 2, 3).
4. Zero findings accompany incomplete coverage: preserve exact unexamined identities and distinct false flags, and require truthful client presentation (Tasks 6, 8).
5. Non-ASCII model paths/parameter values cross fragmented byte frames: preserve UTF-8 and existing report keys without counting characters as bytes (Tasks 2, 6, 8).

---

## Source map, file ownership, and execution rules

Source, test, script, third-party, and verification/setup paths in Tasks 1–8 are relative to `projects/revit-readiness/`; the research document in Task 1 is explicitly repository-root. Task 9 spells out project paths and otherwise uses repository-root teaching/status paths. Run commands from repository root unless a step explicitly changes directory. Read root/project AGENTS and the approved spec first. Preserve unrelated changes; establish the execution checkout using the selected execution skill. Keep the reviewed design commit accessible even if implementation uses a new branch/worktree.

| Unit | Planned files and responsibility |
|---|---|
| Official SDK host | `src/mcp-server/BimAgentLab.RevitReadiness.Mcp.csproj`, `McpHost.cs`, `Program.cs`, `ReadinessTools.cs`, `TraceStream.cs`, `packages.lock.json`: protocol, stdio, tool mapping, optional local wire evidence |
| Plain contract | `src/bridge-contract/BimAgentLab.RevitBridge.Contract.csproj`, `BridgeMessages.cs`, `BridgeLimits.cs`: dependency-free DTOs/constants, multi-target net48/net10.0 |
| Original connection runtime | `src/bridge-runtime/BimAgentLab.RevitBridge.Runtime.csproj`, `FrameCodec.cs`, `RequestBudget.cs`, `LocalPipeFactory.cs`, `SessionCatalog.cs`, `PipeEndpoint.cs`, `PipeBridgeClient.cs`, `DispatchQueue.cs`, `ContextStore.cs`: multi-target net48/net10.0, no Revit/SDK references |
| Revit adapter | Add `BridgeApplication.cs`, `BridgeCommands.cs`, `ReadinessExternalEventHandler.cs`, `RevitReadinessExecutor.cs`, `RevitContextCatalog.cs` in existing `src/revit-addin/`; update its explicit compile items/project references |
| Verification executables | `tests/mcp-tests/` (net10.0), `tests/bridge-tests/` (net48 and net10.0), `tests/mcp-client/` (official SDK client), `tests/mcp-probe/` (separate test-only synthetic server). Each has a correspondingly named `.csproj` and focused test/entry files named below |
| Operations and evidence | New scripts named in tasks; extend existing installer/build only where required; retained sanitized results in `docs/verification/`, generated raw evidence and working copies under ignored `samples/local/` |

The runtime assembly factors original API-independent work out of the add-in for net48/net10 integration tests; it is not a shared framework for other BIM products. Runtime serialization adapters use framework `JavaScriptSerializer` on net48 and `System.Text.Json` on net10; no serializer package enters Revit. Existing core/reader/evaluator semantics remain authoritative.

Use existing console-test conventions (named tests, nonzero failure exit), avoiding a new test-framework dependency. Each red step first creates compilable stubs and a meaningful behavioral assertion; a missing project or compiler error is not its intended red result. A task can contain several focused red/green cycles. Tests shown below specify required assertions; helper setup belongs in that task's test file. Do not add dummy passing tests or literal-output implementations.

New `scripts/Test-McpBridge.ps1 -Suite Protocol|Framing|Pipe|Dispatch|Tools|Deployment|All` selects named test executables; print named outcomes and return nonzero on failure. Introduce each suite in its owning task. `All` covers offline/Windows integration checks, never claims desktop/Revit acceptance. Build scripts use locked restore after each intentional initial dependency resolution. Stage only the task's listed changes and related evidence, inspect `git diff --cached`, then make the stated coherent commit; do not use blanket `git add .`.

### Task 1: Official SDK host and early desktop compatibility checkpoint

**Files:** Create SDK project (initially a library), `McpHost.cs`, `TraceStream.cs`; `tests/mcp-probe/BimAgentLab.Mcp.Probe.csproj`, `Program.cs`; `tests/mcp-client/BimAgentLab.Mcp.Client.csproj`, `Program.cs`; `tests/mcp-tests/BimAgentLab.Mcp.Tests.csproj`, `ProtocolTests.cs`; `tests/verify_mcp_transcript.py`; `scripts/Test-McpBridge.ps1`; `docs/verification/mcp-client-compatibility.md`; `third-party/mcp/` notices, and repository-root `docs/research/mcp-sdk-dependencies.md`. Do not yet expose production Revit tools.

**Interfaces:** `McpHost.RunAsync(McpServerHandlers handlers, Stream input, Stream output, CancellationToken cancellationToken): Task` owns SDK server lifetime. `TraceStream(Stream inner, Stream evidence, bool leaveOpen)` transparently copies bytes without rewriting. Probe exposes only `lab_protocol_probe` with boolean `fail`; results explicitly say synthetic/no Revit access. Client entry supports `--server <exe> --server-arg <value>` (repeatable), `--scenario probe`, `--evidence <directory>`.

- [ ] Refresh official NuGet/release/protocol evidence, license/SPDX/package contents and transitive advisories at execution date. Reconcile with [dated research](../../research/revit-mcp-bridge-comparison.md). Keep the author-selected pin; report conflicts instead of silently upgrading/downgrading. Resolve `[2.2.0]` once, review graph/notices, commit lockfiles, then prove locked restore. Record package/source hashes and why no extra hosting package is needed.
- [ ] Write `ProtocolTests` for SDK discovery, structured success/failure, clean stdout, shutdown, and byte-identical trace streams:

```csharp
Equal("2026-07-28", observedProtocol);
Equal("lab_protocol_probe", discovered.Single().Name);
Equal(true, failure.IsError);
Equal("ProbeFailure", failure.StructuredContent.Value.GetProperty("code").GetString());
SequenceEqual(originalBytes, tracedBytes);
Equal(0, nonProtocolStdoutLines);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Protocol`; expect behavioral failure for missing discovery/result handling in stubs.
- [ ] Implement with official `McpServer.Create`, `McpServerOptions.ProtocolVersion = "2026-07-28"`, `StreamServerTransport`, `RunAsync`, and `McpServerHandlers`. Pin the official client's `McpClientOptions.ProtocolVersion` too. Inspect pinned SDK source at `6fa3825973949a9c4f0cd8af344e15a8db09dc35`; do not reference internal `McpProtocolVersions`. SDK owns protocol parsing/dispatch. Trace collection is opt-in, bounded, local/ignored, stderr diagnostics only; trace failure fails an evidence run visibly. Do not ship probe code, a synthetic backend, or a probe mode in production.
- [ ] Rerun Protocol; expect all named tests pass and recorded wire `2026-07-28`. Independently check the transcript with `python projects/revit-readiness/tests/verify_mcp_transcript.py <evidence-directory> --protocol 2026-07-28`; the checker inspects captured traffic, it does not implement MCP.
- [ ] **CP0: before Task 2**, test the probe in actual Codex desktop and Claude Desktop. Inspect supported configuration locally/current official docs; prepare a temporary entry `bim-agent-lab-protocol-probe`, disclose exact change, back up the affected file and change only that entry through the supported configuration mechanism. Follow any necessary user approval/UI interaction. Capture versions, effective executable/arguments/config location, model/mode/tool permissions, discovery, success, `ProbeFailure`, and unmodified wire traffic. Initial baselines are spec §8.1; record replacement versions explicitly. No proxy may rewrite the protocol. Remove the probe entry afterward and preserve unrelated settings.
- [ ] Record **pass only if both desktop traces show 2026-07-28** and successful/error tool exchange. If either fails, investigate supported configuration/version differences, retain evidence and pause dependent bridge work for resolution; an SDK pass or fallback is insufficient. This is compatibility proof only, not A8 live-model acceptance. Commit `feat: prove the pinned MCP host and desktop protocol compatibility` only with truthful pass/failure evidence; a failed checkpoint may be committed but does not permit Task 2.

### Task 2: Plain messages, framing, and shared request budgets

**Files:** Create contract/runtime projects and `BridgeMessages.cs`, `BridgeLimits.cs`, `FrameCodec.cs`, `RequestBudget.cs`, `BridgeJson.cs`; `tests/bridge-tests/BimAgentLab.Bridge.Tests.csproj`, `FramingTests.cs`; extend test script Framing suite.

**Interfaces:** `BridgeRequest` has `ContractVersion:int`, `RequestId:string`, `Operation:string`, `ContextToken:string?`, `DeadlineUtc:string` (validated invariant ISO-8601 UTC). `BridgeResponse` has `ContractVersion:int`, `RequestId:string`, `BridgeSessionId:string`, `ChecklistSha256:string?`, and exactly one of `ResultJson:string` or `Error:BridgeError`. `BridgeError` has `Code`, `Message`, `RequestId` strings and `Retryable:bool`. Operations are the two production tool names. `ReadinessContextResult` in `BridgeMessages.cs` has `RevitPid:int`, and strings `RevitBuild`, `DocumentLifetimeId`, `ProjectUniqueId`, `DocumentPath`, `ChecklistId`, `ChecklistPath`, `Scope`, `Level`, `Phase`, `ContextToken`, `ExpiresUtc` (ISO-8601 UTC); checklist hash comes from the response envelope. ResultJson serializes this context DTO or the complete existing InformationReport according to operation. `BridgeJson.Serialize<T>(T):byte[]` / `Deserialize<T>(byte[]):T` preserve the contract across frameworks. `FrameCodec.ReadAsync(Stream,int,CancellationToken):Task<byte[]>` / `WriteAsync(Stream,byte[],int,CancellationToken):Task`. `IBridgeClock` exposes `UtcNow`, monotonic `Timestamp`, `ElapsedSince(long):TimeSpan`; `RequestBudget` exposes `Remaining`, `IsExpired`, with factories `Admit(IBridgeClock)` and `Receive(IBridgeClock,long acceptedTimestamp,DateTimeOffset deadlineUtc)`.

- [ ] Write `FramingTests` for split/coalesced reads, strict UTF-8, premature EOF, zero/oversized unsigned lengths, request and response bounds, invalid envelopes, and remaining-budget propagation:

```csharp
SequenceEqual(utf8ReportLargerThan8192Bytes, roundTripBytes);
Equal(65536, BridgeLimits.MaxRequestBytes);
Equal(4194304, BridgeLimits.MaxResponseBytes);
Throws<InvalidDataException>(() => DecodeLength(65537, 65536));
Equal(TimeSpan.FromSeconds(23), budgetAfterSevenSeconds.Remaining);
Equal(true, budgetAtThirtySeconds.IsExpired);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Framing`; expect the named fragmented-read or budget assertion to fail against stubs in both target frameworks.
- [ ] Implement bounded exact-read loops, checked length arithmetic before allocation, strict UTF-8 decoding, and contract/schema validation. Private-envelope timestamps use ISO-8601 UTC strings in both serializers, parsed once into DateTimeOffset for budget validation; do not rely on incompatible framework date defaults. Keep `ResultJson` opaque on the private wire, then parse it in the modern tool adapter; existing report date representation stays unchanged. Use an injectable monotonic clock for local elapsed time; reject past or beyond-policy supplied deadlines, retaining the earlier original deadline/accept cap. Frame timer belongs to connection acceptance in Task 3, not to the first byte or parsed request. Build valid small `ResponseTooLarge` errors instead of truncating results.
- [ ] Rerun Framing; all cases pass on net48/net10.0, including non-ASCII strings split within a multi-byte character, exactly-at-limit versus one-byte-over lengths, and expired deadline after slow receive. Commit `feat: define bounded Revit bridge messages and framing`.

### Task 3: Windows-local pipe and explicit instance selection

**Files:** Add runtime `LocalPipeFactory.cs`, `SessionCatalog.cs`, `PipeEndpoint.cs`, `PipeBridgeClient.cs`; contract `BridgeSessionDescriptor.cs`; tests `PipeTests.cs`, `PipeHost.cs`; test script Pipe suite.

**Interfaces:** Descriptor includes contract version, Revit PID/start UTC, Windows session ID, logon SID, random bridge session ID, and pipe name. `SessionCatalog.Publish(descriptor):void`, `Resolve(int? revitPid):BridgeSessionDescriptor`, `RemoveOwned(descriptor):void`. `LocalPipeFactory.Create(string pipeName,string logonSid):NamedPipeServerStream` owns native handle/DACL setup. `PipeEndpoint.Start(Func<BridgeRequest,CancellationToken,Task<BridgeResponse>> submit):void` and `StopAsync():Task`. `IBridgeClient.SendAsync(BridgeRequest,CancellationToken):Task<BridgeResponse>` is implemented by `PipeBridgeClient`, constructed with one validated descriptor and clock; it never silently retargets.

- [ ] Write real Windows `PipeTests` with net48 endpoint/net10 client, two simultaneous clients, correlation, process/start/logon mismatches, stale/multiple descriptors, permission/cleanup failures, and all eight stalled slots:

```csharp
Equal("AmbiguousRevitInstance", resolveWithoutSelector.Error.Code);
Equal(request.RequestId, reply.RequestId);
Equal(expectedSession, reply.BridgeSessionId);
Equal(0, queuedAfterPartialFrame);
Equal(8, recoveredSlots);
Equal(true, cleanupSeconds <= 6.0); // 5s limit + spec's 1s scheduling allowance
Equal(true, subsequentValidRequestSucceeded);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Pipe`; expect identity/admission/cleanup assertions to fail, not merely failure to launch a test executable.
- [ ] Implement asynchronous byte-mode pipes with native `FILE_FLAG_OVERLAPPED`, `PIPE_REJECT_REMOTE_CLIENTS` and an intended-logon DACL; review `SafePipeHandle` ownership/failure cleanup on net48. Atomically publish restricted descriptors under the exact spec path, using invariant ISO-8601 UTC process-start strings. Validate PID/start/logon/session before connecting; explicit PID must also identify a valid bridge. Client connect is at most two seconds within its admitted budget. Verify response version/request/session IDs; malformed/crossed replies cannot yield a report.
- [ ] Implement one non-resetting five-second cancellation timer at **accept**, max eight active connections, one request/reply, bounded writes, and release in every exit path. Observe disconnect while work is queued so it cancels the submitted item. Once a valid frame arrives, use Task 2's remaining budget, never renew it. Reject remote/foreign identity before data access. Stop removes only its owned descriptor and cannot delete another session's record.
- [ ] Rerun Pipe with no bytes, partial headers, partial bodies, and slow trickles, each filling eight slots; retain monotonic accept/cleanup timestamps and subsequent valid request. Exercise actual descriptor ACLs and native flags; test available alternate-logon denial without creating accounts or changing security policy. Record any unavailable cross-logon/cross-machine test as unperformed; source inspection alone is not that test. Commit `feat: add restricted local Revit pipe transport`.

### Task 4: Bounded dispatch, cancellation, and context lifetime

**Files:** Add runtime `DispatchQueue.cs`, `ContextStore.cs`, contract `ContextBinding.cs`; tests `DispatchTests.cs`, `ContextTests.cs`; Dispatch suite.

**Interfaces:** `IDispatchSignal.Raise():DispatchSignalResult` maps Accepted/Pending/Denied/TimedOut. `DispatchQueue.EnqueueAsync(BridgeRequest,CancellationToken):Task<BridgeResponse>`, `Drain(int maxBatch,Func<BridgeRequest,BridgeResponse>):void`, `RearmIfNeeded():void`, `CancelAll():void`. Queue owns synchronization and signal state. `ContextBinding` stores bridge session, document-lifetime ID, project ID/canonical path, checklist ID/path/hash. `ContextStore.Issue(ContextBinding):ContextLease`, `Validate(string token,ContextBinding current):void`, `Clear():void`; `ContextLease` has opaque token and expiry. Failures use the stable spec codes via `BridgeException` defined here.

- [ ] Write tests using fake clock/signal and execution counters; include capacity, exact expiry boundary, copied ID/different path, reopen lifetime, checklist change, and every Raise result:

```csharp
Equal("Busy", fifthWaitingRequest.Error.Code); // one already executing
Equal(0, canceledQueuedReaderCalls);
Equal(0, latePublishedSuccesses);
Equal(1, lastDequeueArrivalExecutionCount);
Equal("ContextCapacity", issueThirtyThirdContextError.Code);
Equal("ContextExpired", validateAtFiveMinutesError.Code);
Equal("ContextChanged", copiedProjectPathError.Code);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Dispatch`; expect bounded-admission, cancellation, or context assertions to fail against stubs.
- [ ] Implement at most four waiting/one executing; copy immutable request data at admission. Check cancellation/deadline before execute and publication. Purge expired contexts, never evict live ones, generate cryptographically random tokens; distinguish expired/unknown and changed bindings per spec. Use one queue owner plus Task 5's valid-context Idling re-arm to recover Pending/final-drain races; Denied/TimedOut become Busy and cannot leave orphan requests. Drain a bounded batch of at most four per handler invocation, with later arrivals re-signaled.
- [ ] Rerun Dispatch including deterministic arrival just after final dequeue, canceled items in the middle, stop with queued/executing work, token expiry at 4:59/5:00, restart invalidation, and same-document fresh value changes (token does not freeze content). Commit `feat: coordinate bounded host dispatch and context lifetime`.

### Task 5: Revit adapter around the existing reader

**Files:** Add the five Revit files in the source map; update `src/revit-addin/BimAgentLab.RevitReadiness.csproj` explicit compile/project references; add `tests/mcp-host-acceptance.md` and source guard tests in `tests/bridge-tests/AdapterBoundaryTests.cs` (Dispatch suite). Preserve `RequiredInformationCommand.cs` and evaluator behavior.

**Interfaces:** `BridgeApplication:IExternalApplication` registers lifecycle/Idling hooks but never auto-starts listening. `StartBridgeCommand` / `StopBridgeCommand:IExternalCommand` explicitly create/dispose the ExternalEvent in valid context. `ReadinessExternalEventHandler:IExternalEventHandler.Execute(UIApplication):void` calls queue drain; `RevitReadinessExecutor.Execute(UIApplication,BridgeRequest):BridgeResponse` resolves/validates then reads. `RevitContextCatalog.Capture(UIApplication):RevitContextSnapshot` / `InvalidateClosedDocuments():void` are API-context-only. Snapshot holds plain context metadata plus a local Document reference used only within the executor, never enqueued or serialized.

- [ ] Write boundary assertions and host acceptance steps before adapter implementation:

```csharp
Equal(false, adapterReferencesMcpSdk);
Equal(false, queryPathCallsManualCommandExecute);
Equal(false, queryPathStartsTransaction);
Equal("ContextChanged", changedLifetimeValidation.Code);
```

  Source guards are regression hints only; they cannot prove read-only API behavior. Host steps explicitly require NoProject, family/unsaved cases, checklist missing/invalid/mismatch, queued document switch, reopen, Save As, expiry, modal delay, stop and Revit shutdown.
- [ ] Run Dispatch with AdapterBoundaryTests; expect missing required adapter boundary assertions to fail. Build with `& ./projects/revit-readiness/scripts/Build-Readiness.ps1` only after the adapter stubs compile against the recorded Revit API.
- [ ] Implement Start as idempotent, including with no active document; Stop/shutdown invalidate tokens, cancel requests, close pipes and clean descriptors. Map all actual Revit Raise enum values, subscribe/unsubscribe Idling re-arm safely, avoid UI-thread waits. Track document open lifetimes in API context, invalidating on close/reopen/Save As, and compare actual active document identity at execution. Do not assume ProjectInformation UniqueId or a path proves an open-document lifetime.
- [ ] Implement bounded sidecar read (detect growth past 4 MiB), SHA-256 and deserialization from the same snapshot, current checklist schema/project/file/level/phase validation. Call existing `RequiredInformationReader.Read(Document, Checklist, string):InformationReport` directly, serialize the full report on net48. No dialogs/exports or modifying APIs on this route. Same-document value edits are evaluated fresh.
- [ ] Run `Test-Readiness.ps1`, `Build-Readiness.ps1`, and Dispatch from their project scripts paths; require existing core/Excel/process passes, clean build and boundary passes. Review API thread ownership and source read-only path. **CP1:** confirm lifecycle/interop/dependency boundaries before installation; record remaining host tests for Task 7. Commit `feat: expose readiness through a read-only Revit event adapter`.

### Task 6: Two production MCP tools with intact evidence

**Files:** Add server `Program.cs`, `ReadinessTools.cs`; update SDK csproj executable/project references; tests `mcp-tests/ToolTests.cs`; Tools suite. Probe remains a separate project.

**Interfaces:** `ReadinessTools(IBridgeClient client, IBridgeClock clock)` produces SDK handlers `ListAsync(RequestContext<ListToolsRequestParams>,CancellationToken):ValueTask<ListToolsResult>` and `CallAsync(RequestContext<CallToolRequestParams>,CancellationToken):ValueTask<CallToolResult>`. Program accepts optional `--revit-pid <int>` and opt-in `--wire-evidence <directory>`, using Task 1 host and Task 3 session selection; production has no synthetic backend selector. Tool successes expose camel-case envelope identity/checklist fields and untouched existing report members under `report`.

- [ ] Write SDK-driven tests through stdio with a test-only injected `IBridgeClient`, strict argument checking, read-only annotations, stable errors, corrupted correlation, full report equivalence, false coverage and Unicode values:

```csharp
SequenceEqual(new[] { "revit_check_required_type_information", "revit_get_readiness_context" }, sortedToolNames);
Equal(true, unknownArgumentResult.IsError);
Equal(false, incompleteReport.GetProperty("CoverageComplete").GetBoolean());
Equal(false, incompleteReport.GetProperty("Evaluation").GetProperty("Complete").GetBoolean());
Equal(expectedUnicodeValue, returnedUnicodeValue);
Equal(expectedReportJsonTree, returnedReportJsonTree);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Tools`; expect schema/report/error mapping failures against stubs.
- [ ] Implement exact SDK schemas (`additionalProperties:false`) plus explicit runtime argument validation. Parse opaque ResultJson using `JsonDocument`/cloned `JsonElement`; do not reevaluate, flatten, fill nulls, or drop false values. Context fields map from the shared DTO to camel-case MCP fields; check results retain the original report tree under `report`. Both success envelopes include `contractVersion`, `requestId`, `bridgeSessionId`, `checklistSha256` from the validated response, never a separately reread checklist. Return SDK `StructuredContent` and concise text with truthful coverage; errors have `isError=true`, code/message/requestId/retryable without stack traces. Cancellation follows SDK semantics. Bind instance once, correlate all replies, admit a 30-second budget before IPC and never replay. Stdout remains SDK-only, diagnostics stderr.
- [ ] Rerun Tools and Protocol; test NC1-like overall false/evaluator true, NC2 both false, schema-required token, unknown args, failure/no-report, larger-than-8192-byte result, prompt-like model data, legacy-protocol rejection and orderly shutdown. Commit `feat: expose two MCP readiness tools with complete reports`.

### Task 7: Explicit deployment, rollback, and live lifecycle checks

**Files:** Create `scripts/Build-McpBridge.ps1`, `scripts/Undo-McpBridgeInstall.ps1`, `scripts/McpDeployment.psm1`, `tests/mcp-tests/DeploymentTests.cs`, `docs/mcp-bridge-setup.md`; modify `scripts/Install-Readiness.ps1` with opt-in `-IncludeMcpBridge`, existing build script as required, and host acceptance procedure. Add `src/revit-addin/BimAgentLab.ReadinessBridge.addin.template` with application/Start/Stop entries and three distinct permanent GUIDs generated once during this task; keep existing readiness/fixture GUIDs unchanged.

**Interfaces:** Build script calls locked readiness build and `dotnet publish` for net10.0/win-x64 framework-dependent server. `New-McpDeploymentPlan -SourceRoot <path> -RevitAddinsRoot <path> -ServerRoot <path>` returns owned source/target/hash entries without writing. Install adds bridge registration only with the explicit switch, uses one known main/core installation, and saves a local ignored backup receipt. `Undo-McpBridgeInstall.ps1 -ReceiptPath <path>` restores that receipt's prior files/manifest/config ownership state with Revit closed, refusing unknown/modified targets. Runtime prerequisite is separately checked, not silently installed.

- [ ] Test generated manifests/owned-file plans in temporary directories, plus a Windows Revit-open refusal; no real install needed for red tests:

```csharp
Equal(0, autodeskDllsInPackage);
Equal(0, modernSdkDllsInRevitDirectory);
Equal(1, installedReadinessCoreIdentityCount);
Equal(true, optionalFixtureCoreIsSynchronized);
SequenceEqual(beforeInstallOwnedHashes, afterRollbackOwnedHashes);
Equal(true, unrelatedFilesAndClientEntriesUnchanged);
```

- [ ] Run `& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Deployment`; expect the ownership/rollback assertions to fail against stubs.
- [ ] Implement plan/apply separation, root-containment checks and hash-verified cleanup. Install host files under existing `%APPDATA%\Autodesk\Revit\Addins\2024\BimAgentLab.RequiredInformation`; modern server under `%LOCALAPPDATA%\BimAgentLab\RevitReadiness\McpServer`. Keep SDK notices with server, manual Excel process/notices with exporter. No dependency crosses into Revit through installation. Receipt records created versus replaced files and prior hashes; never recursively remove a computed unverified path. Do not auto-register desktop entries.
- [ ] Run Deployment, `Build-McpBridge.ps1`, then `Install-Readiness.ps1 -IncludeMcpBridge` from the project scripts directory with Revit closed. Inspect installed hash inventory; manually resolve any unsigned-add-in dialog via the authorized UI process. Run Start twice/Stop/restart, proving no auto-listener and new session on restart. Test uninstall/rollback in a disposable install tree first; verify actual owned installation cleanup/restoration during acceptance and reinstall the tested build.
- [ ] Execute `tests/mcp-host-acceptance.md` using the SDK harness against live Revit. Record exact errors for every A5 state and actual queue/modal cancellation (request times out, releasing modal does not publish late success), copied project/reopen, fresh same-document edits on a disposable copy, stop/shutdown and restart. Arrange mutations as explicit test preparation, never inside query tools. Test two actual Revit instances when available; otherwise leave that host-specific observation unverified. Record modified-state and file hashes, and rerun affected manual JSON/text/Excel command checks. Commit `feat: install and verify the local Revit bridge lifecycle` only with honest observed/unverified evidence.

### Task 8: Independent sample comparisons and both desktop clients

**Files:** Extend `tests/mcp-client/Program.cs` with `--scenario readiness --revit-pid <pid> --case <name> --evidence <directory>`; create `scripts/Invoke-ReadinessMcp.ps1`, `scripts/Prepare-McpControls.ps1`, `tests/verify_mcp_evidence.py`, `tests/test_verify_mcp_evidence.py`, `docs/verification/mcp-readiness-evidence/README.md`, `docs/verification/mcp-readiness.md`; extend host procedure. Preserve existing `tests/verify_information_evidence.py` completeness assertions.

**Interfaces:** Invoke script runs official SDK client against the installed server, acquires fresh context and calls check; user/authorized UI opens the intended model separately. `Prepare-McpControls.ps1 -SourceDirectory <path> -OutputDirectory <new-path>` copies only the complete sample and sidecars to isolated NC1/NC2 folders retaining basename, prepares valid controls, and writes input hashes/expected identities before any context call. `verify_mcp_evidence.py <evidence-directory> --case complete|wall-defects|ceiling-negative|nc1|nc2` compares actual reports with independently prepared expectations and exits nonzero on any mismatch.

- [ ] Write comparator tests with synthetic fixtures that deliberately drop one identity, rule source, raw value, unit, exclusion, or false flag; each corrupted fixture must be rejected. Derive expected rule values from the existing answer sheet and native schedule/checklist identity evidence, not production evaluation. Assert:

```python
assert principal_counts == [(0, 0), (2, 6), (3, 8)]
assert nc1_counts == (16, 15, 0, 0, 1197, 6)  # requested, examined, findings, affected, other-host, links
assert nc1_flags == (False, True)             # coverage, evaluator
assert nc2_counts == (16, 16, 0, 0, 1196, 6)
assert nc2_flags == (False, False)
assert nc1_scope_issue == expected_absent_identity_and_element_not_found
assert nc2_unexamined_identities == expected_cact_01_and_cact_02
```

- [ ] Run `python projects/revit-readiness/tests/test_verify_mcp_evidence.py`; expect meaningful corrupted-report rejection failures against an initial comparator stub.
- [ ] Implement independent comparisons: all principal cases have six used types/18 field rows/16 declared instances, 1,196 excluded host instances/six links; preserve optional Wall C controls. NC1 replaces only `BAL-INFO-WA-01` UniqueId with a valid absent ID before context; exact ScopeIssue is Unexamined/Element not found, evaluator unexamined list empty, Wall A count two. NC2 removes only the `BAL INFO Ceiling ACT` requirement before context; no ScopeIssues, exact `BAL-INFO-CACT-01`/`BAL-INFO-CACT-02` IDs unexamined, all three ACT fields Unexamined/NoMatchingRequirement, required/source null and observed values preserved. Confirm independent ElementId/UniqueId/type/case/parameter mappings from schedules/checklists/UI, not a returned report. Prepare/reset instructions refuse existing output folders and only clean recorded owned paths after hash checks.
- [ ] Rerun comparator tests; then record fresh source/copy/sidecar hashes and inspect native schedules. Run all five cases through live SDK-client → server → pipe → Revit → evaluator, capturing fresh request/session/context IDs, timestamps, full results and protocol traffic. Independently compare each; current-view screenshots/counts alone cannot pass. After normal close compare model hashes and modified-state evidence. No fixture re-preparation is part of a query.
- [ ] **CP2/A8:** Configure the real entry `bim-agent-lab-revit-readiness` in **each desktop application**, using Task 1's reviewed configuration procedure, absolute installed executable, explicit current Revit PID and the same tested build. Capture actual versions/configuration and unmodified wire `2026-07-28`; exercise discovery, context, all three principal samples, both controls, and a structured failure (stale token after switching documents). Retain each application's answer showing incomplete coverage is not a readiness pass. No standalone CLI or probe substitution. Record large Unicode-bearing output in the synthetic transport tests and, if a valid sample has such text, verify it without editing the original samples just for transport coverage.
- [ ] Compare every desktop report independently with the same oracle. If a client fails or returns fallback, A8 remains unmet; report evidence rather than weakening the spec. Commit `test: verify live readiness evidence through both desktop clients` when the claimed tests actually pass, recording any remaining failures explicitly.

### Task 9: Final provenance, teaching, and external implementation review

**Files:** Complete `projects/revit-readiness/docs/verification/mcp-readiness.md`, source/dependency notices and `projects/revit-readiness/docs/mcp-bridge-setup.md`; update root/project README/AGENTS, `docs/reference/development-status.md`, `docs/tutorials/06-direct-design-and-external-review.md`, `docs/how-to/run-spec-plan-and-implementation-reviews.md`, `docs/build-story/journal.md`, and [R3 packet](../reviews/revit-mcp-bridge/03-implementation-review.md). Add `docs/tutorials/07-build-and-verify-the-mcp-bridge.md` and `docs/how-to/build-and-verify-the-revit-mcp-bridge.md`, paired in learning path.

**Interfaces:** A1–A9 evidence ledger links exact Git revisions, installed hashes, commands/results, protocol/client records, independent comparisons, source reuse audit, review findings and dispositions. Participant story answers how the person directed the agent; agent guide supplies reproducible technical procedure and limits.

- [ ] Reconcile every row below with actual retained evidence, not a checklist tick or a prior build. Audit final dependency graph/source: no TypeScript SDK 1.26.0 dependency, copied third-party transport/dispatch, or modern SDK in Revit; name actual reused lab files and any adapted official snippets. Verify licenses/notices and disclose unresolved release-license ownership.
- [ ] Run `Test-McpBridge.ps1 -Suite All`, `Test-Readiness.ps1`, and locked `Build-McpBridge.ps1` on the final candidate once. Run Python evidence comparison for every retained live case/client and the transcript checker. If code changed after runtime capture, rerun affected live cases on the new installed build; do not present stale binary evidence as current. Check Markdown links and `git diff --check`.
- [ ] Write both teaching layers using actual prompts, corrections, original choices, and evidence. Explain CP0's purpose, source provenance, meaningful red/green tests, external-review boundaries, and why zero findings may be inconclusive. Do not invent user prompts or completed review.
- [ ] Commit `docs: record Revit MCP implementation and acceptance evidence`; prepare R3 at the resulting full SHA, with baseline, exact source diff and sanitized evidence/read set. Author hands it to the external reviewer; no unsolicited transmission. Preserve returned findings separately from dispositions, fix accepted defects, run affected verification and record follow-up. Request author disposition only after concrete reviewable evidence exists. No completion claim while required acceptance or blocking review findings remain.

## Acceptance trace and review checkpoints

| Spec | Tasks | Evidence and interpretation |
|---|---|---|
| A1 | 1, 5, 7, 9 | Registry/package/license/advisory record, lockfiles, runtime boundaries, installed inventory, actual provenance audit |
| A2 | 1, 6, 8 | SDK client and actual wire revision, two production tool schemas, error/success/shutdown; probe alone is insufficient |
| A3 | 2–4 | Both-framework framing/queue tests, Windows net48↔net10 IPC, eight-slot accept/cleanup timing and valid subsequent admission |
| A4 | 3, 7 | DACL/remote flag/handle review, Windows identity/descriptor checks, owned cleanup and actual host lifecycle; list unperformed tests |
| A5 | 4, 5, 7 | Token/queue unit tests plus documented live host error/lifecycle matrix |
| A6 | 8 | Three principal + NC1/NC2 through live pipeline, full independent comparisons and fresh identities/results |
| A7 | 5, 7–9 | Source/API review, modified-state and hashes, existing tests/manual exports and fixture core synchronization |
| A8 | 1, 8 | CP0 finds client/protocol incompatibility early; only Task 8's full evidence in **both desktops** satisfies A8 |
| A9 | 7, 9 | Setup/reset/rollback, two teaching layers, real commits, independent R3 and dispositions |

**R2 before execution:** external reviewer checks this plan against approved spec/source; author reviews the revised plan and selects execution method. **CP0:** both real desktop clients use the requested protocol before building the connection. **CP1:** API/lifecycle/dependency boundaries reviewed before deployment. **CP2:** installed full pipeline through both desktops. **R3:** external implementation/evidence review and corrections. CP checkpoints are implementation checks, not invented external review sessions.

## Drafting-agent self-review and execution handoff

Self-review performed at plan revision 1: traced spec §§1–8 and A1–A9 to tasks; kept exact bounds/provenance/three principal expectations; mapped all five Review Focus cases to tests; checked DTO/interface names across tasks; separated probe evidence from production acceptance; included remaining desktop risk, rollback and both teaching layers. This is a document/source consistency check, not an external R2 report, build or runtime test. The approved spec's normative sections are unchanged.

The plan supplies signatures, behavioral assertions and expected results rather than complete implementation bodies. Planned commands are explicitly future work. Implementation-time API/compiler issues must be resolved against the pinned SDK/Revit source and recorded; acceptance changes require spec disposition, not silent plan edits.

Recommend **Native execution** for this connected sequence: one implementer can maintain the pipe/queue/document-lifetime interfaces across nine tasks, while the requested external R2/R3 sessions retain independent review. The alternative is **Subagent-driven execution**, with a fresh implementer and reviewer per task plus final branch review, at greater context cost. This recommendation is not a selection. Await the author's review/execution choice and R2 dispositions before implementation.
