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

## Review follow-up: ask what actually reaches the safeguard

The author brought back an independent review that kept Task 6 open. Although the existing tests passed, closing the desktop connection left a pending request able to read, and two malformed text inputs produced an internal error instead of our useful argument error. The author supplied working reproductions, giving the coding agent something concrete to challenge.

The productive direction was to follow the operation through its real entry and exit points. A test that presses Cancel does not prove what happens when the desktop disconnects. A validation check cannot protect a conversion that happens before it. The agent reproduced both failures before editing, kept the reviewer's probes unchanged, and added cases through the actual executable.

The first text correction was still too late: an SDK conversion failed before our command received the request. Temporary diagnostics revealed that. The agent moved rejection to the SDK's earlier message-filter boundary, then verified the useful error reached the caller. It did not upgrade the SDK or replace MCP with a custom protocol.

Suggested prompt for participants, not a quotation from the author:

> Reproduce the reviewer's failure before changing anything. Show me where the input or shutdown signal first becomes meaningful, and whether it reaches the safeguard you propose. Test through the actual program, keep valid controls, and retain failed attempts as well as the final evidence. Ask for independent follow-up before calling the task closed.

The [follow-up record](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-boundary-review.md) separates the reviewer's findings from the coding agent's corrections. The lesson is to investigate a recurring class of failure across paths, rather than count passing examples. At that checkpoint, Task 6 remained open pending independent follow-up; these tests did not prove live Revit or desktop acceptance.

## When the reviewer keeps finding variants

The next independent review closed the disconnect issue but found five more malformed-input cases. We had found the correct SDK boundary. We still allowed rejected data to continue through it. Fixing one bad field left other bad fields able to break the rejection itself.

The author's direction changed the unit of reasoning to the complete argument object. Before handing it onward, the agent must establish one of two outcomes: the whole object is safe and valid, or rejection can finish using safe information alone. That instruction led to a written contract for both tools and a generated test matrix, including combinations the reviewer had not supplied.

This was the useful intervention: ask the reviewer to identify the missing guarantee, then have the coding agent explain how every path preserves it. The generated cases also caught a case-insensitive property lookup that violated the exact field name. The agent corrected the general comparison rule, preserving the failed attempt in the record.

Suggested reusable prompt, based on the author's supplied guidance:

> Stop patching individual examples. Write the full contract for this handoff: what may be accepted, what must be rejected, and how rejection finishes safely. Derive a test matrix from that contract, include combinations and valid controls, and show that rejected work never reaches the application. Explain why the accepted and rejected paths are safe before requesting review again.

The [correction review record](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-argument-contract-review.md) preserves the checkpoint where shutdown was closed and validation still needed review. Participants can use the [boundary explanation](../explanation/readiness-argument-boundary.md) to see the reasoning; agents should follow the [technical guide](../how-to/build-and-verify-mcp-readiness-tools.md). Independent closure and live demonstration evidence are still separate obligations.

## What allowed us to close this step

The author brought back [independent closure at b4cdb1f](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-closure.md). The reviewer reran the original reproductions and full suites, then challenged the boundary with 120 additional concurrent invalid calls and five valid follow-up controls. No actionable findings remained. The review also confirmed the reasoning: accepted input is fully checked before handoff; rejected input cannot be revisited by later conversion; the original request identity and deadline survive.

For participants, the useful action was to direct a change in reasoning and then seek an independent challenge to it. More passing examples alone did not close the earlier gaps. A complete rule explained what all paths must preserve, and new tests challenged that rule beyond the examples used to build it. This supports the scoped closure without claiming every possible defect has been disproved.

Ask the agent to record the reviewed commit and the limits of the conclusion. Here Task 6 is closed for implementation and offline verification. Installation with recoverable failure handling, actual Revit behavior, and the demonstration in both desktop applications still need their own evidence. The next planned step is Task 7 installation/recovery.
