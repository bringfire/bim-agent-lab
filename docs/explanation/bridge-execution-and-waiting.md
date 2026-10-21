# Bridge execution and waiting model

Written before the held-callback correction, against `b334385`, following the author's independent review. The deadline P1 is closed; a new P2 shows that safety alone did not establish progress. This model guides the correction and its tests; it is not independent acceptance or live Revit evidence.

## Facts that must remain separate

| Fact | Owner and meaning | Does not imply |
|---|---|---|
| Cancellation requested | RequestLifetime atomically makes the request inactive; original budget remains authoritative | Registered callbacks have started or finished |
| Network released | Endpoint worker has completed its I/O, closed its connection and returned its slot | Executor or cancellation callbacks have finished |
| Executor finished | DispatchTicket.Quiesced says no executor can use Context again | Cancellation callbacks have finished |
| Callbacks finished | The lifetime's notification worker has returned from cancellation, including error reporting | Network/executor has finished |
| Lifetime disposed | All owners have released their uses; registrations and sources disposed; retention permit released | Every earlier operation succeeded |

```mermaid
flowchart LR
  O[Stop / disconnect / deadline] --> I[Invalidate request immediately]
  I --> N[Wake transport using private control signal]
  I --> C[Notify executor callbacks on owned worker]
  N --> R[Release connection and network shutdown]
  R --> D[Deferred disposal]
  C --> D
  Q[Executor Quiesced] --> D
  D --> P[Release retained-request permit]
```

There is deliberately no dependency from network release to callback completion. Physical disposal depends on both, plus executor quiescence. Cancellation cannot interrupt a callback that never returns; bounded retention and explicit Busy responses are the response to that condition.

## Why the existing paths fail

| Origin at b334385 | Execution path and locks | Waiting dependency that must be removed |
|---|---|---|
| Endpoint stop | Calling thread holds endpoint gate; stopping.Cancel invokes lifetime.CancelOwned, which runs borrowed callbacks synchronously | StopAsync cannot even return its task until the callback returns |
| Peer disconnect or extra bytes | Monitor continuation invokes disconnected.Cancel, then lifetime.CancelOwned and borrowed callbacks | Handle finally awaits the monitor; Serve cannot replace its slot |
| Request deadline | Owned delay continuation invokes lifetime.CancelOwned | Network can progress in the existing test because this origin already runs separately; that test did not prove the other paths |
| Upstream/owner token in a standalone lifetime | Token initiator, possibly its timer thread, invokes CancelOwned synchronously | Error containment is present, but progress still depends on callback return |
| Callback notification token | Transport close/wait callbacks and arbitrary executor callbacks share a token | Callback ordering can delay transport wakeup even if cancellation is moved to another task |

## Corrected execution contract

1. **Immediate invalidation is a state transition, not callback execution.** Context.ThrowIfInactive checks the same budget and authoritative cancellation state. Expose IsCancellationRequested for polling. The public Cancellation token remains cooperative executor notification; its notification may lag the immediate state transition. net48 cannot atomically mark a standard CTS canceled without invoking its callbacks. Do not pretend a background Cancel preserves synchronous token notification.
2. **A private transport signal is never lent to executor code.** Endpoint response waiting/pipe close, client connect/pipe close and provider binding wait use this signal. Its registrations contain only bridge-owned wakeup/close operations and asynchronous completion sources. No Revit read, executor callback or diagnostic consumer belongs on it.
3. **One notification worker per canceled lifetime.** After marking inactive and signaling transport, schedule at most one owned background thread that invokes the borrowed token's Cancel, contains exceptions and records/reports them. Repeated and concurrent origins do not create more workers. No endpoint/lifetime gate is held while borrowed callbacks execute. Using a dedicated worker avoids held callbacks occupying the pool used by network continuations. No global exception handler, forced thread abort or new request budget.
4. **Bound before admission, not after cancellation.** All PipeEndpoint instances sharing this runtime/AppDomain share eight retained-request permits. Acquire a permit before constructing/submitting an admitted lifetime. Retain it through network completion, executor Quiesced, callback completion and physical disposal. At capacity, validated requests receive correlated retryable Busy without dispatch, queued notification work or a new lifetime. Network connections remain bounded separately at eight per endpoint and continue to accept/reject requests. Restarting an endpoint does not reset retained capacity. Standalone lifetime users must bound their own admissions; the later MCP tool layer must retain that obligation.
5. **StopAsync publishes its shared stop task under the endpoint gate, then releases the gate before signaling or draining.** Stop may await network workers/I/O and attempt owned-descriptor cleanup. It must not await callback notification, executor Quiesced or lifetime disposal. Error reporting is a snapshot of known errors; a later callback error is recorded locally and remains observable by a later Stop. Descriptor cleanup retries retain prior errors.
6. **Disconnect monitor owns only network observation and immediate signaling.** It must return after signaling, even if a borrowed callback stays held. Handle may await this monitor after closing its pipe because the monitor cannot execute borrowed callbacks. Registration disposal on this path must likewise only wait for bounded bridge-owned signaling, never borrowed notification work.
7. **Retirement owns the physical wait.** After network cleanup, deferred retirement waits for executor Quiesced, the owned deadline operation, notification completion and origin-registration release; then disposes sources and releases the permit. A held callback or executor keeps its permit. Faulted Quiesced is still terminal: report its error and release capacity after successful physical disposal. Failed physical disposal remains observed/reported and cannot silently return an unsafe permit. Release is idempotent.

Submit is a trusted, nonblocking queue-admission handoff, not executor execution; the future dispatcher must preserve that distinction. Diagnostic sinks must enqueue or return promptly; arbitrary blocked diagnostic sinks are outside this cancellation-progress guarantee. Registering on an already-canceled public token can synchronously invoke code on the registering executor's thread: that work belongs to executor Quiesced, never the transport path. The Revit API reader remains confined to the later ExternalEvent executor.

## Allowed waits and locks

| Participant | May wait for | Must not wait for / run inline |
|---|---|---|
| Stop caller | Brief endpoint state lock to publish/reuse task | Network drain, borrowed callbacks, executor, retirement |
| Stop task | Network workers, owned-descriptor cleanup attempt | Executor Quiesced, notification worker, retained permits |
| Pipe worker | Bounded framing/I/O; response or private cancellation; short disconnect monitor | Borrowed callbacks or physical retirement |
| Cancellation origin | Brief lifetime state lock; bridge-owned control signaling; worker start | Notification completion or executor |
| Notification worker | Its registered callbacks and local failure reporting | No endpoint/lifetime gate held during callbacks |
| Retired lifetime | Network done, Quiesced, deadline worker, notification worker, bounded origin-registration release | No UI thread and no endpoint gate held while waiting |
| Capacity counter | Atomic permit acquire/release | No waiting queue; full capacity rejects promptly |

## Tests derived from the model

Cross actual endpoint origins **stop, disconnect, deadline** with **held-return / held-throw** callbacks and **Quiesced-before / Quiesced-after-network-release**. In every case, hold the callback throughout the progress assertion: StopAsync returns its task promptly, repeated stop completes and descriptor disappears; disconnect/deadline recover all eight simultaneous connection slots. Observe immediate inactivity, unchanged deadline, suppressed late results, retained callback resources, error reporting after release and eventual disposal. Record what runs before release, not merely eventual success.

Saturate all eight retained permits with held callbacks, free their network connections, then prove eight connections still open, valid work returns Busy without dispatch, stop/restart cannot bypass the cap, and releasing callbacks restores service. Also run more exceptional Quiesced completions than the capacity limit to prove failed execution cannot leak successfully cleaned permits. Include a held callback on a client lifetime to challenge provider/connect/response cancellation separately. Keep earlier exception/peer-exit/strict-protocol regressions; update any test that incorrectly equates immediate invalidation with notification completion or demands future callback errors before Stop may finish.

API basis: Microsoft documents [synchronous Cancel callbacks and aggregated errors](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource.cancel?view=net-10.0), and distinguishes immediate state transition from asynchronous notification in modern [CancelAsync](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource.cancelasync?view=net-10.0). The split above is original application code shared with net48, not copied framework code or a new SDK dependency.

## Task 6: MCP tool lifetime and permitted waits

Before implementing production handlers, extend the same model to the external host. The SDK owns MCP parsing, dispatch and client cancellation. Tool admission creates one request ID and 30-second budget; validation rejects caller-controlled paths/unknown arguments before invoking a bridge. Discovery never selects Revit. The production entry creates a lazy provider, so errors occur through the ordinary tool call.

A tool reserves one of the runtime's eight retained-request slots before constructing RequestLifetime. The server assembly receives internal access to the existing runtime retention/disposal primitives rather than duplicating their ownership rules or exposing a new public API. The test assembly receives access to the real SessionCatalog's validated-descriptor seam. This changes no runtime policy or package versions.

| Stage | Permitted wait | Exit / retained responsibility |
|---|---|---|
| MCP handler | Existing provider/IPC task under the original context | Real client returns only after its pipe I/O and private registrations unwind; no borrowed callback waits |
| Mapping / publication | Bounded validation and JSON mapping; no await after the final activity guard | Recheck after IPC, after parsing and immediately before returning success; canceled/expired output cannot publish success |
| Handler finally | Start retirement; do not await notification callbacks | IPC/context consumers are quiescent when SendAsync has returned or thrown; retain the slot until physical disposal completes |
| Deferred retirement | Owned deadline/notification/registration disposal | Observe failure; release capacity only after successful disposal; report callback failures locally with request correlation and no model data |
| SDK shutdown | Cancel handlers and release streams | No wait for held borrowed callbacks; background notification remains bounded until it ends or the process exits |

Tests must challenge full stdio discovery/argument/report/error behavior, real provider selection and binding, and late cancellation/expiry at each mapping guard. Standalone direct-handler tests hold eight cancellation callbacks while asserting prompt handler completion, Busy on a ninth request even with a new tools instance, and eventual service after release. SDK/subprocess tests remain distinct from live Revit and desktop acceptance. Tests use original synthetic reports as transport oracles, never as BIM sample evidence.

### Task 6 review: transport termination precedes handler draining

The review of `189fd78` exposed a missing cancellation origin. Ctrl-C, explicit MCP cancellation and stdin termination are different paths. In pinned C# SDK 2.2.0, StreamServerTransport completes its message channel on EOF or read failure; McpSessionHandler then waits for in-flight handlers in its finally block. Channel completion alone does not cancel their linked tokens. Canceling only after RunAsync returns creates a circular wait: the host waits for handlers that still believe the client is connected.

The host must race the SDK run task against its transport's MessageReader.Completion. If the transport completes, observe its terminal cause and cancel the same host token BEFORE awaiting the run task. SDK-linked request cancellation immediately invalidates the existing bridge lifetime and closes IPC; the remote endpoint then invalidates its own context without waiting for Response or Quiesced. No deadline restarts. No extra cancellation worker or competing reader is needed. The normal run-completion path proceeds to owned server/transport disposal; it must not wait for a still-open transport first. Input faults remain observable after draining, rather than becoming successful shutdown. Trace failure retains its existing explicit failure path.

| Origin / owner | Work before draining | Allowed wait / test observation |
|---|---|---|
| SDK transport channel completion, EOF or read failure | Host observes completion and cancels its existing run token | SDK handlers unwind their IPC; no executor or borrowed callback wait |
| Explicit stop / MCP cancellation | Existing SDK/lifetime cancellation path | Keep open-stdin and explicit-cancellation controls |
| Held public-token callback | Existing bounded notification worker, outside SDK/transport path | HOST_STOPPED must occur before releasing the callback, for EOF as well as explicit stop |
| JSON argument decoding | Decode and validate caller text inside the argument boundary, before retained admission/IPC | Unpaired surrogate escapes return correlated InvalidRequest; valid pairs still reach ordinary selection |

Regression cases cross the actual production executable's EOF with tracing off/on and all three compatible protocol revisions, holding both synthetic executor completions. They require natural process exit, immediate inactivity and a rejected final-read guard while the original budget is still live. Separate tests cover read faults and held callbacks, so EOF success cannot conceal an exceptional-completion or waiting dependency. Raw-wire Unicode cases must bypass serializer repair and keep valid controls in the same surviving process. These are offline transport proofs, not desktop or Autodesk acceptance.

Implementation refinement from those tests: catching GetString inside the typed handler alone still left all three raw-wire Unicode cases failing. Temporary entry diagnostics proved neither malformed call reached that handler. SDK RequestHandlers.SetWithAlternate deserializes JsonNode params first, decoding the bad value during that conversion. The SDK's incoming message filter now preflights only the readiness contextToken. An undecodable value receives a server-owned per-message rejection marker and is replaced by null solely to let the SDK convert the rejected request. The typed handler checks that marker before argument handling, retained admission or IPC and returns the ordinary correlated InvalidRequest. Valid values are never replaced; the raw trace retains the original bytes. The marker is an object identity in SDK request context, not a client-supplied field. SDK protocol-metadata validation precedes this filter; negotiation, result envelopes, version-specific fields and dispatch remain SDK-owned. Direct typed calls also catch string-decoding errors locally. No broad catch around downstream execution and no handcrafted JSON-RPC response.
