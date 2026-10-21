# Dispatch and context lifetime

Task 4 implementation model, written before code against independently closed Task 3 at af4c67b. It carries the [reviewed execution/waiting model](bridge-execution-and-waiting.md) into the queue. This is an implementation argument and test design, not live Revit evidence.

## Owners, threads and permitted waits

| Operation | Execution and locks | Completion and interruption |
|---|---|---|
| Enqueue | Pipe worker; short queue lock; registers only bounded bookkeeping on the private transport signal | Never invokes executor or waits for an event handler. Four waiting entries maximum. The signal adapter is a trusted prompt Raise call, made outside the queue lock. |
| Raise | Single reserved signal attempt outside queue lock | Accepted/Pending retain queued work. Denied/TimedOut/exception reject waiting work with correlated retryable Busy. A generation counter prevents a stale Raise return from claiming an already-drained signal. |
| Drain | Future Revit API thread; one drain owner; no queue lock across executor | At most four entries per call, even if a larger batch is requested. Pass the identical Context; guard immediately before invocation and before publication. Reentrant/concurrent Drain cannot execute a second callback. |
| Queued cancellation | Private transport callback; short queue lock, no executor/borrowed notification | Remove waiting entry and settle Response. Release the registration outside the lock, then finish Quiesced when admission no longer uses Context. |
| Executing cancellation | Same bounded bookkeeping | Settle Response immediately; executor continues unwinding. Quiesced completes only after its finally and registration release. No queue-owned lifetime disposal. |
| CancelAll | Lifecycle caller; marks queue stopped, cancels waiting/executing responses | Never awaits executor or public cancellation notification; later admission is unavailable. Executing Quiesced stays pending. |
| RearmIfNeeded | Future Idling callback, or drain completion | Revisit pending work without assuming a prior Pending means another event will run. Do not spin or recursively wait for Revit. |
| Physical retirement | Existing endpoint owner | Wait for Quiesced plus owned notification/deadline work before disposing lifetime and returning retained capacity. |

The queue lock protects entry membership, one drain owner and response selection. Registration disposal occurs outside that lock: disposal can briefly wait for a concurrent bookkeeping callback that needs the lock. Completion sources use asynchronous continuations, so completing them under a lock cannot invoke arbitrary consumer code inline. No borrowed callback is registered on the private transport token.

Admission must publish its registration before an entry becomes executable, and must mark admission finished before Quiesced is allowed to complete. A cancellation observed during registration can settle the response, but cannot declare the still-running admission finished. A removed queued entry has no future executor. An executing entry remains owned until callback finally, regardless of whether its Response is already canceled or faulted.

## Publication is a state transition

A check followed by success publication on an unrelated lock leaves a cancellation race. Task 4 will add a narrow internal Context publication operation: under the same activity gate used by RequestLifetime to invalidate, recheck the original budget and cancellation state, then settle the asynchronous response completion source. It may only publish the response; never run preparation, serialization or a reader under that gate.

Lock order is queue gate then activity gate. Cancellation releases the activity gate before invoking private bookkeeping that takes the queue gate. No opposite nested order is allowed. Publication linearizes at the final active check; cancellation after that point is later, and endpoint/MCP publication guards still apply. Queue stop also holds the queue gate, so it cannot turn a previously canceled response into success. Expiry always uses the original monotonic budget, not notification timing.

## Scheduling without lost or duplicate work

One signal attempt can be in flight. Accepted/Pending coalesce ordinary arrivals until Drain begins; Idling can explicitly retry either state. Drain clears that claim and advances a generation. If Raise returns after that generation changed, its result cannot mark the new queue as already scheduled. Drain completion re-arms waiting arrivals. If a signal attempt is still returning, Idling supplies the next retry; there is no blocking retry loop.

Denied/TimedOut/throwing Raise rejects waiting entries, including arrivals that depended on the in-flight attempt. It cannot revoke an executing callback. A later new request can make a fresh attempt. The Task 5 signal adapter must map every actual Revit result and coordinate event disposal in valid API context; this queue does not create, dispose or own an ExternalEvent.

## Context references

The context store is per bridge session, capped at 32 live leases. It snapshots binding data, uses cryptographically random opaque tokens, and measures five-minute expiry monotonically while exposing a UTC expiry for display. Issue/validate purge expired entries, without evicting live ones. Unknown/revoked/expired tokens give ContextExpired; a known token with changed session, open-document lifetime, project ID/path, checklist ID/path/hash gives ContextChanged.

The trusted adapter supplies canonical absolute file paths; Windows paths compare ordinally ignoring case, IDs compare ordinally, and SHA-256 hexadecimal compares ignoring case. The store does not open files or resolve filesystem aliases. Task 5 must obtain canonical paths and hash/parse the same bounded checklist bytes. Mutable caller bindings are copied, so changing an object after issue cannot retarget its token. A token stores identity, not model values; same-document edits remain fresh reads.

Revoke is idempotent and restores capacity for an unpublished lease. Clear removes all tokens at stop; a replacement store/session does not recognize an old token. Context capacity and queue capacity are separate from the endpoint's eight retained-lifetime permits. Standalone lifetime admission is still Task 6's responsibility.

## Tests derived before implementation

- Four waiting plus one held execution; excess Busy with request/session identity; restored admission after removal. Enqueue must not invoke the executor.
- Each Raise result, thrown Raise, recovery after rejection, Pending re-arm, arrivals during the last executor and after final dequeue, stale Raise completion, concurrent/reentrant Drain, and four-item batch cap.
- Queued and executing cancellation with public notification held; middle-item removal; exact request expiry before read and during execution; no late success; original Context/budget unchanged.
- Stop during queued/executing work, repeated stop, admission after stop, callback exceptions, cancellation/publication races, registration release and deferred physical lifetime disposal.
- All binding members independently changed; copied project ID/different path; close/reopen lifetime; changed checklist; snapshot mutation; 32/33 capacity, revoke/clear/restart, 4:59/5:00 expiry, wall-clock changes, concurrent issuance, and fresh values under unchanged identity.

Test doubles stand only at the host signal, clock and API callback boundaries. Production queue/lifetime/store paths are exercised. Controlled barriers occur in those doubles, not test hooks in production. Tests assert progress before held work is released. Actual Revit scheduling/modal behavior remains a later host acceptance requirement.

## Implementation refinements

The original model above was committed at 638e288 before production code. During implementation, failing tests established two additional handoff guards: Issue rejects incomplete/noncanonical bindings before creating a usable lease; a null or crossed executor response becomes a correlated InternalError and does not strand the next entry. A deliberate mutation replacing the shared publication gate by an unrelated lock was rejected on both runtimes, then restored before the final full regression run. These are implementing-agent observations; [Task 4 independent review](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md) remains pending.
