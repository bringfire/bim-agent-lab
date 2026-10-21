# Connect the existing check to Revit's event system

This lesson continues the [queue and context story](10-coordinate-requests-and-model-context.md). The purpose is to let the future agent connection request our existing readiness check while respecting Revit's rules about when model data can be read.

## How did we engage the agent?

The author had already approved the plan, selected Native execution, and required independent review and two teaching layers. At this checkpoint the author relayed the independent reviewer's conclusion:

> **No actionable findings. Task 4 can close at `24a72cf`.**

The same feedback explicitly separated the finished queue from the next work: actual Revit scheduling, shutdown integration and final read guards belonged to Task 5. See the [preserved closure](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-closure.md). The agent continued that approved scope; the author did not need to prescribe methods or repeat permission for each file.

The practical instruction carried forward was: keep the reviewed guarantees when connecting another component. A request that was valid when it arrived may be canceled while its checklist is being prepared. The agent therefore wrote down the transitions before implementation, then tested interruptions at those transitions. This is how the earlier review insight became a working habit rather than another paragraph of advice.

## What the agent handled

The agent connected explicit Start/Stop commands, Revit's event callback, the document/context checks, and the existing readiness reader. The new route returns the report to the bridge. The earlier manual command still writes its JSON, text and Excel reports.

It also checked the installed Autodesk API documentation and compiled against the actual Revit 2024 assemblies. One detail matters to the user: Save As must invalidate an old context even if someone later saves back to the original name. The implementation listens for that event as well as comparing the current document and path.

The agent introduced no new third-party package. The official C# MCP SDK remains in the separate server; the new Revit adapter is original lab code around our existing computation. The TypeScript 1.26.0 SDK and other bridges remain research precedents.

## What evidence should the participant inspect?

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-revit-adapter.md) separates three kinds of evidence: a successful Autodesk-reference build, executable boundary tests, and live tests still to perform. The new tests cancel or expire requests during preparation, reading and serialization. Another test holds a cancellation callback while the real Windows pipe endpoint shuts down. Deliberately removing two guards makes the tests fail, which helps show that they detect the intended mistakes.

A build is not a demonstration in Revit. These checks do not show the real application scheduling an event, reading the three sample models, or returning results to Codex desktop and Claude Desktop. Installation/recovery, those model comparisons, NC1/NC2 and the desktop workflow remain separate acceptance steps. The next independent review receives that boundary explicitly.

## A prompt to reuse

**Suggested prompt, not historical dialogue:**

> The reviewer has closed the previous increment. Continue the next approved task. Before coding, show how its new boundaries preserve cancellation, identity and cleanup. Reuse the existing computation. Test interruptions in the production path, and tell me which claims still require the actual application. Record the result for participants and future agents, then prepare the focused review packet.

The participant's job is to set the purpose and evidence standard, recognize the limits of a passing check, and use independent review at the next boundary. The [agent procedure](../how-to/build-and-verify-revit-adapter.md) carries the implementation detail.
