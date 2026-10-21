# Task 4 focused implementation review

Baseline: **af4c67bd4391fd00019799ff1436a4f53b744fe4**, the [independently closed Windows IPC increment](2026-10-21-task-3-closure.md). Review through the commit titled **`feat: coordinate bounded host dispatch and context lifetime`** that adds this packet. Resolve its hash with `git log --diff-filter=A --format="%H %s" -- docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md`.

The [Task 4 execution model](../../../explanation/dispatch-and-context-lifetime.md) was committed at **638e288 before production changes**. Native execution continues the approved plan. This is implementing-agent evidence, not independent closure.

## Scope and changes

The original dual-target runtime now owns four waiting/one executing admission, bounded batches, signal/result handling and separate Response/Quiesced completion. A per-session context store provides 32 random, five-minute identity references. It does not store model values or perform file I/O. The internal Context publication method shares RequestLifetime's activity gate so cancellation and successful completion have a defined order. No new package, copied upstream source, SDK policy, installation, model or global configuration change is included.

The model-to-code refinements are explicit: queue/store constructors receive session identity; incomplete/noncanonical bindings cannot issue a lease; a null or crossed executor envelope becomes correlated InternalError without stranding later work. These author-side discoveries have failing tests before their corrections. The prior Task 3 reviewed ownership/progress boundaries remain binding.

## Evidence and boundaries

Read the [verification record](../../../../projects/revit-readiness/docs/verification/2026-10-21-dispatch-and-context.md), [technical procedure](../../../how-to/build-and-verify-dispatch-and-context.md) and [participant story](../../../tutorials/10-coordinate-requests-and-model-context.md). Tests run through real production queue/store/lifetimes; four integrate the actual pipe endpoint with the production queue and held notification/executor cases. The Revit signal and API callback remain test doubles. No actual ExternalEvent, live model or desktop A8 result is claimed.

## Suggested independent-review request

> Review af4c67b through the Task 4 implementation commit against the approved spec/plan and pre-implementation execution model. Trace registration/admission, signal attempt, drain ownership, cancellation/publication, retirement and context expiry. Challenge both correctness and progress: cancellation during admission, simultaneous stop/drain/publication, held callbacks, lock order, stale Raise completion, Pending/Idling recovery and exceptional completion. Verify four-waiting/one-executing capacity, four-per-drain cap, all Raise results and no orphan entries. Confirm invalidation remains immediate while public notification is asynchronous, and that Quiesced never precedes the queue's last use of Context.
>
> Check every identity binding, copied-ID/different-path and reopened-document cases, mutable-input snapshots, 32/33 capacity, revoke/clear/restart, exact monotonic expiry and fresh values. Rerun Dispatch, All and readiness; inspect the actual-endpoint cases, publication mutation and retained evidence hashes. Keep Task 3 closure separate from this new increment. Report Task 4 findings and whether it can proceed to the Task 5 Revit adapter. Live Revit, desktop A8 and full R3 remain outstanding.

Independent Task 4 disposition: **pending**. Task 5 is unstarted.
