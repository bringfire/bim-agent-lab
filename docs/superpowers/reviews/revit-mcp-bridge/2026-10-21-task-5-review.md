# Task 5 focused implementation and CP1 review

**Review follow-up:** Task 5 remains open. One P2 checklist-error mapping finding was reproduced and corrected, with new adapter/queue regression tests. [Feedback, disposition and follow-up packet](2026-10-21-task-5-checklist-review.md) distinguish implementing-agent verification from independent closure. The original submission below is preserved.

Review baseline **24a72cfd28665fd6f9d93875cfabe9621ced940d** through the commit titled **`feat: expose readiness through a read-only Revit event adapter`** adding this packet. Resolve HEAD with `git log --diff-filter=A --format="%H %s" -- docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-review.md`. [Task 4 closure](2026-10-21-task-4-closure.md) is preserved separately. The [execution model](../../../explanation/revit-adapter-lifetime.md) was committed at b44ea23 before production changes.

This increment compiles the Revit adapter and tests production synchronous guard, host ownership and conversion boundaries. Additional small runtime/session/event/snapshot and net48 codec helpers allow testing actual production code. These refine the plan's file list, not its required behavior. No new package, installation, model mutation, SDK policy or client settings change is included.

Read the [verification record](../../../../projects/revit-readiness/docs/verification/2026-10-21-revit-adapter.md), [agent procedure](../../../how-to/build-and-verify-revit-adapter.md), [participant story](../../../tutorials/11-connect-the-readiness-reader-to-revit.md) and [pending live matrix](../../../../projects/revit-readiness/tests/mcp-host-acceptance.md).

## Suggested independent-review request

> Review Task 5 against the approved spec/plan and the prior execution-and-waiting guarantees. Trace explicit startup and failed acquisition, descriptor ownership, event Raise borrowing/disposal, queue admission, preparation, the direct existing reader call, response publication, lease revocation, stop and shutdown. Check locks and permitted waits, exception paths and what happens if cleanup fails. Confirm no Document enters queue/IPC and no executor work runs on the private transport signal.
>
> Challenge document identity across copies, close/reopen and Save As including return to an old filename; hash/parse the same bounded sidecar snapshot; inspect schema/project/file/level/phase validation and canonical paths. Verify all four flow guards use the original context/budget and token cleanup survives cancellation after executor return. Inspect net48 conversion, false flags and original string values. Check actual Revit API usage, thread/context assumptions, Idling registration/retirement and all Raise results. The host-shutdown case with an in-flight native Raise explicitly retains ownership to process exit if no later API callback occurs; determine whether this is an acceptable pending live-host obligation or requires a code correction.
>
> Rerun Dispatch, All, readiness and Autodesk-reference build; check new and prior evidence hashes, mutation logs and actual-pipe/host shutdown evidence. Keep source/build/fake-event results separate from actual Revit behavior. Confirm no MCP SDK in Revit, no Autodesk assemblies in the modern host or output redistribution, and no package/source reuse beyond the documented boundaries. Report actionable findings and whether the increment can proceed toward Task 6/Task 7. Full CP1 live lifecycle verification, both desktop A8 workflows, model comparisons and R3 remain outstanding.

Independent Task 5 disposition: **pending**. Do not infer installation readiness or whole-bridge acceptance from this submission.
