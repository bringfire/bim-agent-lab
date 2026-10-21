# Expose the readiness check to an agent

## How we engaged the agent

The author had already chosen two desktop applications, an approved specification and plan, Native execution, and independent review. The instruction was to keep teaching both audiences throughout the build. After the reviewer closed Task 5, the next planned step was to give an agent two ways to use that work: identify the current model/checklist, then ask for its required-information report.

Actual earlier direction:

> “Ok, I approve plan revision 2. Let's use Native execution and start with the compatibility check in Codex desktop and Claude Desktop. Keep recording the process in both teaching layers.”

The later compatibility direction also required keeping the `2026-07-28` regression. Task 6 carries those decisions forward; no new choice of desktop client or SDK was requested.

## What the person needs to judge

A useful result needs more than a short answer. The agent must receive the model/checklist identity, the complete evidence, and a clear distinction between a successful check and a complete check. Zero findings can still mean some elements were not examined. The first tool acquires context; the second uses its token. Neither tool opens, changes or exports the model.

We asked the code to preserve a deliberately awkward test report: non-English names, a date-shaped string, null values, false completeness flags, precise numbers, a long value and text that looks like an instruction. These are test data. The same complete content must reach the agent through older compatible exchanges and July 2026. The SDK handles MCP; we are building the application connection around it.

The agent first ran tests against empty implementations and observed failures. It then implemented the tools and challenged cancellation after the bridge had returned its result. Removing the last cancellation/deadline guard made four checks fail, including actual SDK exchanges. Restoring it removed those failures. This is a way to ask whether a test can detect the bug it claims to prevent.

## A useful correction during testing

An additional test exposed a real error-classification gap: a deadline that interrupted transport could escape as ordinary cancellation. The tool now keeps the original deadline and returns its deadline error while preserving explicit caller cancellation behavior.

Two shutdown tests initially timed out too. The trace showed the host had stopped while the callback was still held; the test was waiting for the client to see EOF from a child it deliberately kept alive. We corrected the test's waiting sequence, preserving the stronger requirement: prove host shutdown before releasing the callback, then prove natural process exit with stdin still open. The failed record is retained alongside the diagnosis.

Suggested follow-up prompt, not a quotation from the author:

> Show me that the agent receives the whole report, knows when coverage is incomplete, and cannot receive a successful result after cancellation. Keep the test fixture separate from proof that Revit actually ran. Record what was reused, what is new and what still requires live testing.

## Where this leaves the demonstration

[Task 5's independent closure](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-closure.md) covers its implementation and offline checks. Task 6's [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-readiness-tools.md) records these new tool checks. Installation/recovery, actual Revit scheduling/shutdown, the three sample cases and incomplete-coverage controls in both desktops, and final independent review still have their own gates.

Future agents should use the [technical procedure](../how-to/build-and-verify-mcp-readiness-tools.md). Participants can use the [review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-review.md) to ask for an independent challenge before deployment.
