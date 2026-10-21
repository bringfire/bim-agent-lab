# Build and verify the private bridge foundation

This is the agent companion to the [participant story](../tutorials/08-build-and-test-the-bridge-foundation.md). Follow root/project `AGENTS.md`, [current status](../reference/development-status.md), [spec revision 3](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) and [Task 2 of the plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-2-plain-messages-framing-and-shared-request-budgets). The author explicitly requested this task under Native execution. Historical evidence does not establish a new build's result.

The [dated verification record](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) identifies the baseline, checkpoint, retained evidence and acceptance limits. This increment supplies API-independent components for later IPC, queue and Revit adapters. Do not expose synthetic fixtures as production tools or claim live-model acceptance.

## Source and dependency boundaries

Paths below are relative to `projects/revit-readiness/`:

| Unit | Responsibility |
|---|---|
| `src/bridge-contract/BridgeMessages.cs`, `BridgeLimits.cs`, `BridgeException.cs` | Plain DTOs, two operation names, private version 1, limits and stable domain-error carrier |
| `src/bridge-runtime/FrameCodec.cs` | Four-byte unsigned little-endian length, exact reads/writes, strict UTF-8, caller-owned stream |
| `BridgeJson.cs` in runtime | Bounded encoding, explicit envelope validation, complete-input syntax checks and literal token-to-DTO mapping |
| `RequestBudget.cs`, `SystemBridgeClock.cs` | Admission/receive budget factories and monotonic elapsed time |
| `RequestLifetime.cs`, `BridgeExecutionContext.cs`, `DispatchTicket.cs` | Linked cancellation/timer ownership, defensive request snapshot, distinct Response/Quiesced tasks |
| `tests/bridge-tests/` | Console tests targeting both net48 and net10.0; literal fixtures and independent framing oracle |
| `tests/verify_bridge_exchange.py` | Python standard-library check of independently parsed emitted JSON; no C# codec reference |

All three new projects target net48 and net10.0 with committed lockfiles and **zero external NuGet packages**. Contract assemblies reference only their framework base. Runtime references the contract and framework assemblies, with no Autodesk, MCP SDK or readiness-core references. `System.Web.Extensions` is conditional on net48; it is absent from the modern runtime assembly's references. No SDK binaries or generated executables are committed.

Encoding uses framework `DataContractJsonSerializer` into a byte-limited stream. A shared `JsonReaderWriterFactory`/XML infoset pass validates original field presence, duplicates, names, primitive types and Unicode. DTO construction copies values from those validated tokens, without string coercion. `JavaScriptSerializer` on net48 and built-in `System.Text.Json` on net10.0 provide complete-input syntax checks only; their inferred values never supply DTO fields. The review correction below documents the net48 date-prefix handling. This is private envelope handling, **not an MCP implementation**. C# SDK 2.2.0 remains responsible for MCP in the unchanged external host. Its attributed stdin adaptation remains separate; TypeScript SDK 1.26.0 and third-party bridges remain precedent-only.

Official API resources inspected on 2026-10-21:

- [DataContractJsonSerializer](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.serialization.json.datacontractjsonserializer) and [stand-alone JSON serialization](https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/stand-alone-json-serialization).
- [JsonReaderWriterFactory](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.serialization.json.jsonreaderwriterfactory) for the JSON infoset; observed parser behavior is established by our tests, not assumed from the API name.
- [JavaScriptSerializer](https://learn.microsoft.com/en-us/dotnet/api/system.web.script.serialization.javascriptserializer) for the .NET Framework parser.
- [System.Text.Json overview](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) for modern framework availability. A separate NuGet package exists for older runtimes, but was not adopted: the plan keeps serializer packages out of Revit.

These are platform APIs, not copied library source or a new redistributable dependency. Retain the existing [SDK provenance/license review](../research/mcp-sdk-dependencies.md). The repository owner's original-material distribution license remains a separate pre-release decision.

## Reproduce the checks

Use the already verified Windows/.NET Framework 4.8 targeting/runtime environment, .NET SDK 10.0.204 and Python from the [prerequisite guide](../../projects/revit-readiness/docs/development-prerequisites.md). A running Revit instance is not required. From the repository root:

```powershell
git status --short --branch
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite Framing
& ./projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
& ./projects/revit-readiness/scripts/Test-Readiness.ps1
```

`Framing` performs a locked multi-target build, 33 named tests per target, emission of five fixtures by each target, consumption by the opposite target, and the independent Python comparison. It fails if either runtime fails. Fresh generated files are under ignored `samples/local/bridge-exchange/<run-id>/`. `All` also runs the retained protocol/stream/settings and SDK/wire checks; it does not install or register anything in the desktop applications. The existing readiness suite covers core evaluation and Excel isolation, not the new live connection.

To independently inspect a retained or new exchange:

```powershell
python projects/revit-readiness/tests/verify_bridge_exchange.py projects/revit-readiness/docs/verification/bridge-foundations-review-evidence/exchange
```

Inspect hashes in the evidence inventory as well as test summaries. The fixture contains synthetic incomplete-coverage data with Unicode and prompt-like text; it is not an NC1/NC2 Revit result. The runtime keeps its report string opaque. Parsing that string in the independent verifier is an evidence assertion, not production evaluation.

## Contract and interruption rules for the next task

Request and response limits count UTF-8 **bytes**, excluding the four-byte header: 65,536 and 4,194,304. Zero/over-limit lengths fail before body allocation. Reads tolerate fragmentation and consume exactly one frame; coalescing tests do not authorize multiple requests per connection. A canceled or failed partial write leaves a possibly incomplete connection: Task 3 must close it, never retry a frame on the same stream. The stream must honor cancellation at blocking operations; real Windows pipe cancellation is Task 3 acceptance. Codec methods do not dispose a caller's stream or take ownership of mutable buffers after their calls finish.

Private DTO names are case-sensitive. Reject unknown/duplicate/missing members, wrong primitive types, invalid Unicode, invalid timestamps, unknown contract/operation, and response result/error ambiguity. Optional fields may be absent/null; a readiness-check request still requires a nonblank token. Acquisition must carry no token. Error request ID must match its envelope. An explicit false `Retryable` member is required in wire errors. `ChecklistSha256`, when present, is 64 hexadecimal characters. Context identities/paths/scope/level/phase are required and process ID is positive. UTC timestamps use `Z` with zero to seven fractional digits; emission uses seven. No string in ResultJson is interpreted as an instruction.

`SerializeResponse` produces a correlated `ResponseTooLarge` error if bounded encoding exceeds 4 MiB. It does not truncate, mutate the input or send a partial success. Session/request identities must already have been validated/generated by their owners. Do not use this helper to rescue a malformed envelope.

| Boundary | Owner and obligation |
|---|---|
| MCP admission | Task 6 creates one `RequestBudget.Admit(clock)` and derives the wire deadline from it. Other stages borrow it. |
| Pipe receipt | Task 3 retains the acceptance monotonic timestamp and enforces its separate non-resetting five-second frame timer. After a complete validated request, `Receive` rejects expired/beyond-policy deadlines and caps remaining work by original UTC deadline and accept-plus-30-seconds. |
| Within either process | Keep the same budget object; elapsed time is monotonic after validation. Do not reconstruct a new allowance during preparation, execution, serialization or publication. |
| Request lifetime | Admission/endpoint owns linked upstream/stop cancellation plus a timer derived from remaining time. Round timer delay up to the next millisecond to avoid firing before expiry; guards use the precise monotonic budget. Wire deadline must equal the supplied budget's original deadline. |
| Borrowing executor | Context exposes copies of its request snapshot and the same budget/token. Call `ThrowIfInactive` at the plan's handoffs, especially immediately before the Revit read and before publishing success. |
| Callback cleanup | Queue alone completes `DispatchTicket.Response` and `Quiesced`. Endpoint can await `DisposeAfterQuiescenceAsync`; a canceled Response alone never authorizes disposing a context still borrowed by a callback. |

`Receive` retains the original wire deadline even if the acceptance cap is earlier; its `Remaining` enforces the earlier bound. Guards prioritize `DeadlineExceeded` with the request identity over generic cancellation. Factory errors occur before attaching a request context and have an empty internal RequestId: the boundary must correlate the validated request identity before publishing a domain error. Malformed data is closed as specified, not converted into a success. Task 2 does not implement that endpoint mapping.

Invoke the cleanup helper from the owner's final cleanup after its own IPC/publication work has stopped. Callback quiescence does not authorize disposal while another owner-side operation still borrows the context. A `using` scope must not dispose the lifetime before awaited cleanup finishes.

## Observed corrections and remaining proof

Initial stubs compiled, then all 27 behavioral cases failed on both targets. A test-source compiler failure before that was setup work, not the red behavioral proof. The first implementation exposed that the JSON infoset reader accepts trailing objects and commas; a minimal framework experiment reproduced it. Complete-input runtime parsers now supplement the shape check. Another negative input showed net48's parser replacing an unpaired surrogate; checking the original infoset string before conversion prevents that data change. A final negative case found optional fractional timestamp formatting accepting a decimal point with no digits; explicit supported formats now reject it. Tests also require integer tokens and preserve explicit false values rather than accepting DTO defaults. Bounded DataContract encoding is an implementation refinement to the plan's serializer description; no package or contract change was needed.

The added `DisposeAfterQuiescenceAsync` helper makes the reviewed ownership obligation executable. Its tests control callback completion explicitly. They prove lifetime cleanup behavior, not a production dispatch queue or Revit read: those tests belong to Tasks 4–5. The same distinction applies to the fake-clock preparation tests and real-stream cancellation doubles. Task 3 must prove actual Windows pipes, eight-slot recovery and instance selection; Tasks 4–8 must prove queueing, read-boundary guards, all samples/negative controls, installation recovery and both desktops' A8 calls. R3 remains open.


## Task 2 review corrections

The [independent review and separate dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-review.md) found a semantic conversion defect, an error-normalization gap and a mistaken fixture field. All supplied tests had passed at the reviewed commit. Treat the new cases as evidence about what the earlier tests omitted, not as reason to relax expected values.

T2-01: use validated original infoset strings to construct every DTO on both targets. Framework parsers check whole-input syntax only. In the net48 syntax-only copy, change the special escaped-slash date prefix to its equivalent plain-slash spelling before `DeserializeObject`; discard that parser's result. This prevents implicit date conversion and out-of-range date exceptions without changing any returned value or original evidence bytes. The source/token stream used for DTO mapping is never rewritten. Negative cases still reject extra objects and trailing commas. Tests cover both slash spellings, negative/offset/large date-shaped strings, literal backslashes/quoted dates, opaque result strings and nested errors. Cross-runtime fixtures include `/Date(0)/` and a value outside the DateTime range. The dated evidence links the inspected Microsoft implementation; no upstream code was copied.

T2-02: normalize `FormatException` from malformed JSON to `InvalidDataException`, retaining its inner exception. Raw newline, tab and NUL each reproduced the leak on both targets; escaped forms remain accepted. Do not broaden this to catching every exception: stable domain failures such as `UnsupportedContract` still use `BridgeException`.

T2-03: derive field names from `InformationReport.CoverageComplete` and `Evaluation.Complete`, checked in core source and retained native-Revit output. C# and Python fixtures now use those exact names. The corrected Python oracle rejects the initial retained fixture; fresh fixtures come from rerunning emission on both targets. The original logs and fixtures remain historical and must not be presented as evidence for the two real completeness members.

The [new verification record](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) and separate hashed evidence supersede the initial checkpoint's acceptance claims for these boundaries. Keep syntax, schema, value preservation and oracle provenance distinct when extending the bridge. Author-side corrections are ready for reviewer follow-up; no independent closure, Task 3 implementation or live Revit pass is implied.

Current handoff: [Task 2 independent follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md) closed the findings at `3cefe51`. The [Task 3 guide](build-and-verify-local-pipe-transport.md) now records actual Windows pipes and instance selection. Its review is pending; production dispatch and live Revit acceptance remain later tasks.
