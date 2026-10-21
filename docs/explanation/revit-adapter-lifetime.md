# Revit adapter execution and retirement

Task 5 design notes, written before implementation against Task 4's independent closure at 24a72cf. The approved plan supplies the scope; this maps its responsibilities to production interfaces and tests.

| Stage | Owner and execution | Interruption and permitted waits |
|---|---|---|
| Application load | Revit API thread registers application identity; no listener | Start remains an explicit command, also available without a document. |
| Start | API thread creates event, queue/store and endpoint, then starts the listener | Endpoint publishes only after event and pipes exist. Retain the session before acquisition so failed starts can retire it. Unwind network and event even after an exception. |
| Raise | Transport thread borrows a narrow event lease | No document access. Map all four native results. Retirement closes new raises immediately; API thread polls in-flight Raise completion rather than waiting for it. |
| Drain | ExternalEvent handler in valid API context | Original queue admits four waiting/one executing. Only this path calls the executor. No model objects enter transport/queue DTOs. |
| Prepare | Same synchronous API callback | Capture actual active open-document identity, canonical path and bounded sidecar bytes. Hash and parse that snapshot. Validate schema, project/file, level/phase, then token binding. |
| Read and serialize | Same API callback, original execution context | Guard on entry, immediately after prepare/before read, after read and after serialization. No new budget, await, transaction, dialog or export. An existing read may finish; a late result cannot publish. |
| Context issuance | API callback issues a lease after preparation | Guard before issue and serialization; revoke on failure. Track queue response separately so cancellation between executor return and publication also revokes an unpublished lease. Delivery to a remote consumer cannot be guaranteed; a successfully published but undelivered token still expires normally. |
| Stop | API thread closes queue admission and clears tokens before endpoint stop | Endpoint drains network independently of borrowed notification and executor completion. Try event disposal only in API context after in-flight raises finish. Idling completes retirement; no UI wait on request callbacks. |
| Shutdown | API context requests the same stop | No synchronous task wait. Dispose event immediately if unborrowed, otherwise retain safe ownership; process exit is the final boundary if no further API callback occurs. Record this live-host acceptance risk. |

`HostBridgeSession` and `ApiEventLease` will contain production lifecycle/retirement logic independent of Autodesk, so failure-after-acquisition tests exercise the actual ownership code. This adds two runtime helpers beyond the plan's named flow file; it avoids testing a duplicate lifecycle model. The private transport token remains exclusively owned by existing runtime bookkeeping.

Open-document lifetimes use Revit's documented Document equality (same currently open document), plus a random lifetime identifier and canonical path. A close invalidates the old wrapper; reopening or Save As cannot reuse the old binding. Same-document value edits do not invalidate identity and are read afresh. Document references remain solely in the API-context catalog and synchronous prepared object.

Required offline interruptions: cancel and expire independently at entry, during preparation, inside read and during serialization; successful unchanged flow; changed binding; unpublished lease revocation; event factory failure; endpoint failure before/after publication; repeated stop; shutdown with held event Raise and held borrowed cancellation notification. Use the production queue/flow/session, with doubles only for native scheduling, clock and endpoint acquisition faults. Source checks and a real Autodesk-reference build complement these tests; neither establishes live Revit behavior.
