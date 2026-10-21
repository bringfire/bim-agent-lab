# Carry review lessons into the next feature

**Follow-up:** Task 4 is [independently closed at 24a72cf](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-closure.md). The submission observations below are preserved as history. Task 5 implementation/live acceptance is separate.

The bridge could already connect Windows processes. The next question was whether it could coordinate requests without reading the wrong model, starting canceled work, or letting unfinished work block cleanup. This is Task 4 of the approved plan; the actual Revit event adapter comes in Task 5.

## How the person directed the agent

The author returned independent closure of Task 3 at af4c67b and included this direction:

> Task 4 should preserve the model’s explicit boundaries: nonblocking queue admission, authoritative context guards, and no executor work on the private transport signal. Standalone lifetime admission remains a Task 6 responsibility.

That is more useful than simply saying “continue.” It identifies what the next feature must preserve, and what remains a later responsibility. The person did not prescribe the queue's implementation. They supplied the acceptance boundary and used review to carry a hard-earned lesson forward.

The agent recorded the closure separately from its own work, then committed an [execution model](../explanation/dispatch-and-context-lifetime.md) at 638e288 before implementation. It traced admission, scheduling, execution, cancellation, result publication and cleanup. This exposed a further connection: checking cancellation and publishing success as unrelated steps can still race. The implementation therefore coordinates those two decisions, without running model-reading code inside that protected step.

## What participants can judge

| Question to ask the agent | Observable answer required |
|---|---|
| What happens if Revit is busy? | Four requests may wait behind one executing request; additional requests get a clear Busy result. |
| Can canceled work still begin? | Canceled queued work is removed without invoking the reader. An already-running read may finish, but cannot replace a canceled response with success. |
| Can shutdown finish while work remains? | Network shutdown completes while a notification callback is held; resources stay alive until their users finish. |
| How do we avoid checking a different model? | A context reference records the session, open-document identity, saved path and checklist identity. A copied file, reopened document or changed checklist fails validation. |
| Does a context freeze model data? | No. An unchanged model identity is read fresh on each execution. The context expires after five minutes. |

The agent first ran the tests against stubs. It then built the implementation, added actual-pipe checks with the production queue, and deliberately removed the publication guard to verify that the race test detects its absence. It also retained two smaller author-side discoveries: incomplete bindings should not issue usable contexts, and invalid executor responses should return an error without stranding later work. These were implementation findings, not new independent-review findings.

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-dispatch-and-context.md) distinguishes the observed results from what remains unproved. Passing these tests does not demonstrate Revit's actual event scheduling, model reads or either desktop's full workflow. The [review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md) is the next independent checkpoint.

A **suggested prompt** for another project:

> Continue the approved plan, carrying the previous review's execution boundaries into the next feature. Before coding, show how work enters, waits, executes, returns a result and releases resources. Identify decisions that must happen together. Test cancellation and failure between steps, including progress while unfinished work is held. Keep your own findings separate from independent review, and record what still requires the real application.

Agents should use the [technical procedure](../how-to/build-and-verify-dispatch-and-context.md), which carries source boundaries, commands, invariants and the next adapter obligations.
