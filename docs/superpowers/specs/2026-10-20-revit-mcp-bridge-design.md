# Minimal Revit readiness MCP bridge: design specification

Date: 2026-10-20. Revision: 1. Status: **draft for external and author review; implementation has not begun**. Source baseline: `7d3ae71`. Work branch: `docs/revit-mcp-spec`.

This is architectural work under the superpowers brainstorming protocol. The author selected the minimal custom C# bridge after the [comparison](../../research/revit-mcp-bridge-comparison.md), then requested a written spec, implementation plan, and external review sessions. The [participant story](../../tutorials/06-direct-design-and-external-review.md) preserves those prompts. The [review packet](../reviews/revit-mcp-bridge/01-spec-review.md) is ready for handoff; no external review or approval is claimed here.

## 1. Purpose and success

BIM professionals should be able to ask an agent to check required wall and ceiling type information in the intended open Revit sample, and receive the same evidence as our verified command. The teaching example must explain research, source provenance, specification, planning, review, implementation, and independent verification in two layers: a participant engagement story and a reproducible agent procedure.

The author's explicit constraints are the official C# MCP SDK **2.2.0**, protocol **2026-07-28**, a minimal custom bridge, all three existing sample cases verified end to end, and an explicit account of whether older code is used. The agent proposes the detailed process, IPC, and lifecycle choices below; those choices are reviewable, not additional instructions attributed to the author.

Acceptance requires fresh calls through a real MCP client, the server, local IPC, Revit API execution, and the existing evaluator. A successful build, stored JSON read, or simulated Revit response cannot establish host acceptance. The three cases are three variants of the existing information check, not the larger demo's three BIM questions. Physical-condition checks and measured quantities remain separate increments.

## 2. Options and selected direction

| Approach | Benefit | Cost or constraint | Decision |
|---|---|---|---|
| Original narrow Revit adapter plus official C# MCP server | Reuses our tested BIM logic and installed .NET workflow; small explicit tool surface | We own IPC, lifecycle, identity, deadlines, and verification | Selected by the author after research; specified here |
| Adapt the inspected third-party Revit bridge | Existing routing, command catalog, and Revit integration | Requires MCP migration and changes to access, framing, scope, errors, and readiness integration | Retain as research precedent |
| Original Revit adapter plus official Python server | Viable SDK and approachable Python workflow | Adds another runtime/package workflow and still needs the C# Revit adapter | Retain as an alternative, outside this increment |

The dated [source evidence](../../research/evidence/mcp-version-observations-2026-10-20.json) supports this comparison. It does not prove our proposed runtime behavior.

## 3. Code provenance and dependency policy

| Material | Intended use | What we must be able to demonstrate |
|---|---|---|
| Official `ModelContextProtocol` 2.2.0 and resolved dependencies | Consume published NuGet packages, exact direct pin `[2.2.0]`, committed lockfile | Package graph, package hashes/notices, license and advisory review, locked restore, actual protocol trace |
| Official C# SDK documentation and examples | Learn supported registration, transport, result, and client APIs | Cite inspected release/commit; record any copied or adapted nontrivial snippet with its notice |
| Existing bridge using TypeScript `@modelcontextprotocol/sdk` 1.26.0 | Learning precedent only | No v1 package dependency, vendored source, copied handler, or copied transport in the implementation |
| RevitMCPSDK 2024 helper source | Learning precedent only | No runtime dependency or copied dispatch base class |
| Our `RequiredInformationReader`, core evaluator, and `InformationReport` | Reuse existing lab code | Source map and equivalence tests; preserve the existing rule and report semantics |
| New local connection, dispatch, context validation, and tool adapter | Original lab implementation | Reviewable source with tests and a final provenance audit |

**We are writing the Revit adapter, not implementing MCP from scratch.** The official SDK owns MCP parsing, transport behavior, protocol handling, discovery, and tool dispatch. Our private IPC contract is not a replacement MCP implementation. A protocol-test probe, if used, remains test code and is labeled accordingly.

The previous research inspected the C# release at commit `6fa3825973949a9c4f0cd8af344e15a8db09dc35`. The [release page](https://github.com/modelcontextprotocol/csharp-sdk/releases/tag/v2.2.0) and [dated specification](https://modelcontextprotocol.io/specification/2026-07-28) were reopened while drafting this spec. Refresh official release, registry, migration, and license evidence at adoption. If a newer release exists, record it and explain why the requested compatible pin is retained; do not silently change it. If the requested package cannot meet this contract, return the concrete discrepancy for a design decision.

SDK version, dated MCP revision, our private contract version, and JSON-RPC 2.0 are distinct. Do not label TypeScript SDK 1.26.0 as a C# SDK or MCP protocol version. Preserve actual SDK license/NOTICE material, including any retained earlier contribution terms; review the full resolved graph rather than inferring all licenses from the top-level package. The original lab's distribution license remains an owner decision before release.

## 4. Components and deployment boundary

```mermaid
flowchart LR
    A[Agent or test client] -->|MCP over stdio| B[C# server / net10.0]
    B -->|Framed local named pipe| C[Revit adapter / net48]
    C --> D[Bounded queue and ExternalEvent]
    D --> E[Existing reader and evaluator]
    E -->|Fresh InformationReport| B
```

| Component | Responsibility | Dependencies and source boundary |
|---|---|---|
| External server | Two tools, SDK protocol integration, IPC client, result/error mapping | New code in `projects/revit-readiness/src/mcp-server/`; net10.0 executable and official SDK. No Autodesk references or net48 core assembly loaded here |
| Wire contract | Plain request/response DTOs, operation names, version and bounds | Proposed `src/bridge-contract/` under that project; dependency-free code shared across net48/net10.0. Serialization stays in each runtime adapter |
| Revit bridge | Start/stop, session discovery, framed pipe server, queue, ExternalEvent, context validation | New focused files in existing `src/revit-addin/`; Revit 2024/net48 and Windows APIs; explicit project compile items |
| BIM computation | Read intended live document; evaluate declared requirements | Existing [reader](../../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs), [checklist](../../../projects/revit-readiness/src/readiness-core/Checklist.cs), [evaluator](../../../projects/revit-readiness/src/readiness-core/TypeInformation.cs), [report](../../../projects/revit-readiness/src/readiness-core/InformationReport.cs) |
| Existing manual command | JSON/text/Excel output and user dialog | Existing command and isolated ClosedXML exporter; preserve their behavior |

Use the already recorded Revit 2024 build `24.3.60.12` and .NET SDK `10.0.204` for initial acceptance. The external server needs its matching .NET runtime; document its install/publish prerequisite separately from Revit's .NET Framework 4.8. Other host versions are unverified.

The MCP route calls the reader, not `RequiredInformationCommand.Execute()`. The server carries the report as JSON without reimplementing its evaluation or loading `System.Web.Extensions`. Preserve the report's member names and meanings across serialization. Infrastructure may write session descriptors and bounded diagnostics; the readiness tool itself creates no JSON, text, or Excel export. Evidence files are written by the verification client separately.

Bridge installation is explicit and per-user, with Revit closed. Extend the known lab installation rather than create conflicting copies of the add-in/core. Keep new MCP runtime packages in the external server directory. Retain installer identity checks, file hash inventory, and existing optional-fixture core synchronization. Do not enable fixture editing, redistribute Autodesk binaries, alter Revit configuration, or register a global agent-client configuration silently. The implementation plan will specify exact scripts, manifests, and rollback commands.

## 5. Tool contract and document binding

Expose exactly these tools; no arbitrary command, code, path, model-opening, editing, or export tool is included.

| Tool | Arguments | Successful structured result |
|---|---|---|
| `revit_get_readiness_context` | Empty object | Contract version, request/session identity, selected Revit process/build, active saved project identity/path, checklist ID/path/SHA-256, declared scope, opaque context token, expiry |
| `revit_check_required_type_information` | Required `contextToken` string; no other arguments | Contract version, request/session identity, checklist SHA-256, and complete fresh `InformationReport` under `report` |

Declare read-only intent using SDK tool annotations and reject unknown arguments. Annotations do not enforce read-only behavior. A finding is a successful observation: a report with missing information is not an execution error. Incomplete coverage remains an explicit report state, never a readiness pass. A tool failure has `isError=true` and structured `code`, `message`, `requestId`, and `retryable`; protocol-level malformed requests are handled by the SDK. Cancellation follows SDK semantics and cannot return a late success.

The trusted Revit side derives the `.checklist.json` sidecar from the active saved project using the existing convention. No caller controls that path. Validate the existing checklist schema and project/file/level/phase identities. Capture its bounded bytes once, hash and deserialize those same bytes; at execution reread bounded bytes, compare the hash and parse that same snapshot to avoid using a different checklist after validation.

An opaque random token is a server-side context reference, not an authorization credential. Bind it to the bridge session, open-document lifetime, `ProjectInformation.UniqueId`, canonical full project path, checklist ID/path/hash, and a five-minute expiry. Keep at most 32 live contexts per Revit bridge; purge expired records and return a capacity error if still full. Never silently evict an unexpired context. These are proposed operational bounds, not measured performance claims.

Both tools execute document access in valid Revit API context. Before reading, validate that the same document is currently active and that the session, document lifetime, path, and checklist still match. A copied project can share project UniqueId; that ID alone is insufficient. Closing/reopening a document, Save As, changed checklist bytes, restart, expiry, or a different active document invalidates the check. An active-document mismatch does not switch the model on the user's behalf. Obtain a new context after resolving it. A context does not freeze model contents: edits to values in the same still-open document are read fresh, with report timestamp and modified-state evidence.

## 6. Host lifecycle, IPC, and limits

The user starts the bridge through a named Revit command. Create the ExternalEvent there in valid API context; an application lifecycle hook owns shutdown cleanup. A stop command cancels pending work, closes connections, disposes the event in valid context, invalidates tokens, and removes only its own session descriptor. Startup registration alone does not begin listening. Repeated start is idempotent; restart gets a new session identity. The [Autodesk ExternalEvent guide](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Advanced_Topics/Revit_API_Revit_API_Developers_Guide_Advanced_Topics_External_Events_html.html) establishes API scheduling, not a guarantee of prompt execution during modal activity.

Use a Windows byte-mode named pipe unique to Revit PID, process start identity, Windows logon session, and random bridge-session identifier. Publish an atomically written discovery descriptor under `%LOCALAPPDATA%\BimAgentLab\RevitBridge\sessions`. Restrict descriptor and pipe access to the intended user/logon session; enforce rejection of remote pipe clients. The native [CreateNamedPipe contract](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createnamedpipea) provides `PIPE_REJECT_REMOTE_CLIENTS`. A documented, narrowly scoped interop wrapper may be needed on net48; review handle ownership and failure cleanup. A randomized pipe name or loopback client choice alone is not access enforcement.

This is a same-user local integration, not a sandbox against malicious software already running as that user or an administrator. No TCP listener, remote service, or authentication system is included. Validate descriptor process/start/session identity before connecting. A server startup selector may identify the Revit PID; absent a selector, auto-select only one validated live bridge. Zero candidates yields unavailable, multiple candidates yields an explicit selection error. Do not pick whichever model was opened most recently. Bind the server to the selected bridge session; restart requires reconnecting explicitly, not silent retargeting.

Private contract v1 uses a four-byte unsigned little-endian payload length followed by exactly that many UTF-8 JSON bytes. Each connection carries one request and one response, then closes. Requests contain contract version, unique request ID, allowlisted operation, optional context token, and a deadline. Responses repeat version/request/session IDs and contain either a result or an error, never both. Close malformed connections without treating partial bytes as complete JSON.

| Resource | Proposed bound and behavior |
|---|---|
| Request payload | 64 KiB; reject invalid/oversize lengths before allocation |
| Response payload | 4 MiB; return a small `ResponseTooLarge` error, never truncate a report |
| Checklist file | Existing 4 MiB limit, checked before reading; reject growth beyond the limit |
| Concurrent pipe connections | At most eight per Revit bridge; bounded connection handling |
| BIM work | One executing request plus at most four queued; excess yields `Busy` |
| Request lifetime | 30 seconds from server admission, including connect, queue, execution, and response; no automatic replay |
| Connect attempt | At most two seconds, within the overall remaining deadline |
| Contexts | Five-minute expiry, 32 live contexts, as above |

Use deadline-derived monotonic timers for elapsed-time enforcement in each process; reject expired or beyond-policy deadlines. Propagate only remaining budget rather than resetting a timeout at each layer. Bound reads/writes and release resources on partial frame, timeout, disconnect, stop, or shutdown. Cancellation/disconnect invalidates queued work; the handler checks cancellation and deadline immediately before invoking the reader and again before publishing a result. An already executing Revit API read may finish after cancellation; its late result is discarded, and Revit is never killed or forcibly interrupted.

The pipe listener may parse, validate, and enqueue plain data only. A single owner coordinates `ExternalEvent.Raise()` and queue draining so requests are neither stranded nor run twice. Handle every Raise result: an accepted signal schedules work; a pending signal can serve the same owned queue; a rejected/timed-out signal must not be treated as success. Drain only the bounded batch with per-request checks; arrivals during execution must be re-signaled reliably. The implementation plan must provide adversarial tests for those races before relying on live Revit.

The existing command's `[Transaction(TransactionMode.ReadOnly)]` does not protect the new ExternalEvent handler. Its query path must contain no transaction or modifying API call. Review source, record modified state, and compare sample/original file hashes independently; unchanged disk bytes alone do not prove absence of unsaved in-memory changes.

## 7. Failure behavior

| Condition | Stable error/result behavior |
|---|---|
| Revit/bridge not available, stopped, stale descriptor | `BridgeUnavailable`; no report |
| Several eligible instances without selection | `AmbiguousRevitInstance`; require explicit configuration |
| No active project, family document, unsaved project | `NoProject`, `UnsupportedDocument`, or `UnsavedProject` |
| Missing/invalid/foreign checklist | `ChecklistMissing`, `ChecklistInvalid`, or `ChecklistMismatch` |
| Expired/unknown token or changed document/checklist/session | `ContextExpired` or `ContextChanged`; obtain a fresh context |
| Context table full | `ContextCapacity`; retry after contexts expire |
| Queue full or rejected host dispatch | `Busy`; retry later with context validity checked |
| Deadline/cancellation | `DeadlineExceeded` or SDK cancellation; no stale successful result |
| Private frame/schema/version/operation invalid | `InvalidRequest`, `UnsupportedContract`, or `UnsupportedOperation`; close connection if framing prevents safe correlation |
| Pipe peer access/identity rejected | Reject connection; do not return document data |
| Result over bound or unexpected failure | `ResponseTooLarge` or `InternalError`; preserve diagnostic correlation, exclude stack traces from tool content |
| Missing required type value or unexamined declared case | Successful report with findings/coverage state; never replace it with a zero-filled success |

Retryable means a caller may deliberately retry after the condition changes; the server itself does not replay a check. Return concise remediation without raw stack traces or session secrets. Treat model names and parameter text as data, not agent instructions. Logs identify failures and requests without routinely copying model contents.

## 8. Acceptance and evidence

All acceptance below is **required future work**, not a claimed pass. Retain exact tested revisions, binaries, SDK/client versions, selected protocol, commands, timestamps, raw results, and limitations in a new MCP verification record.

| ID | Required demonstration | Evidence |
|---|---|---|
| A1 | Requested SDK resolves and locks; modern server builds; Revit stays net48; no SDK 1.26.0 or upstream bridge/helper source is adopted | Fresh version/dependency/license/advisory record, notices, provenance audit, locked build |
| A2 | Exactly two tools; correct schemas, structured success/error, clean stdio and graceful shutdown | SDK client plus captured protocol exchange using **2026-07-28**. Older fallback, if tested, is recorded separately |
| A3 | Framing, limits, split/coalesced reads, a report larger than 8192 bytes, correlation, two clients, queue saturation, cancellation, deadline and late-result rejection | Offline tests and net48-to-net10 IPC integration tests; no mocked test labeled Revit acceptance |
| A4 | Local user/session restriction, remote-rejection configuration, specific instance selection, stop/restart, invalid descriptors and failed cleanup | Windows integration checks and reviewed access/handle code; explicitly disclose any cross-machine test not performed |
| A5 | No/family/unsaved document; checklist errors; document switch while queued; token expiry and close/reopen; modal/busy Revit; shutdown | Reproducible real-host steps/results, including observed error codes |
| A6 | All three sample reports match independent evidence through live MCP | Per-case context/call transcript, fresh report, native schedule comparison, literal answer sheet, exact identities and coverage |
| A7 | Queries do not modify models or regress existing exports | Source review, clean sample modified-state evidence, before/after RVT hashes after normal close; existing core/Excel/process tests and manual-command regression |
| A8 | Intended seminar agent client discovers and calls the server | Actual client version/configuration and selected protocol. A custom SDK harness proves that harness only; client incompatibility remains an explicit release blocker |
| A9 | Teaching, review, and reproducibility are complete | Both teaching layers, review dispositions, actual Git checkpoints, setup/reset/rollback, current status and evidence links |

Use the existing [answer sheet](../../../projects/revit-readiness/samples/expected-results/required-information.md) as the independent oracle:

| Existing sample | Expected missing type-field findings | Distinct affected instances |
|---|---:|---:|
| `BAL-info-complete.rvt` | 0 | 0 |
| `BAL-info-wall-defects.rvt` | 2 | 6 walls |
| `BAL-info-ceiling-negative.rvt` | 3 | 6 walls and 2 ceilings |

Each case covers 16 declared instances, six used types, and 18 type-field evaluations. Wall C's optional blank rating/assembly must remain a passing control. Preserve raw values, rule/requirement source, parameter/type/element/case identities, scope/level/phase, units, exclusions, unexamined cases, coverage flags, and timestamps. The original samples reported 1,196 other host wall/ceiling instances and six links outside scope; freshly verify these too. No measured quantity or assembly/rating correctness claim is added.

Compare the MCP result's report using independent native schedules and literal expected values; the current [Python comparator](../../../projects/revit-readiness/tests/verify_information_evidence.py) can be reused/extended without importing production evaluation. Supplement it with complete identity/rule/source/coverage checks: counts alone are insufficient. Do not regenerate the oracle from returned reports. Reuse existing sample copies, record fresh hashes, inspect native schedules, and do not rerun fixture editing as part of a read-only test. Safely arrange failure cases on disposable inputs/copies and restore them explicitly.

## 9. Review stages and completion boundaries

1. **R1 — specification:** external reviewer challenges scope, provenance, design, failure semantics, and acceptance. Author/agent record each disposition and revised spec; the author approves the written spec.
2. **R2 — implementation plan:** use the superpowers writing-plans skill after R1/author approval. Write exact tasks, file paths, behavioral tests, expected failing/passing results, dependencies, checkpoints, deployment/rollback, and evidence capture. External reviewer checks feasibility and traceability against this spec; author reviews the revised written plan and selects execution method.
3. **R3 — implementation and evidence:** external reviewer receives the exact code diff, dependency graph/provenance, tests, installed-file inventory, and actual MCP/Revit acceptance. Address findings with evidence; record unresolved limits before declaring the increment ready.

The [review hub](../reviews/revit-mcp-bridge/README.md) supplies handoff prompts and a findings form. Review packets are not reviews. No external reviewer has yet been named, no request has been sent, no response has been received, and no approval is inferred. Self-review is labeled separately. Do not mark a stage passed merely because a reviewer or build produced a reassuring summary.

The implementation plan is a subsequent artifact, not the component outline in this spec. Bridge code, dependency adoption, deployment, live calls, and final evidence remain pending the requested review process. This sequence preserves the original build objective while making the design and review skill teachable.
