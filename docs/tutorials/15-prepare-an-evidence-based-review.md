# Ask for evidence before accepting the whole system

The bridge had answered the demonstration questions in both desktop applications. It had also recovered after installation changes and a real Revit shutdown. The author then asked:

> Ok, let's consolidate the acceptance evidence and prepare the R3 review packet. Clearly identify what passed and what remains unverified, and record the process in both teaching layers.

This is the actual instruction. It asks the agent to make its work assessable before another reviewer decides whether the whole implementation meets the requirements. Participants do not need to read every test log. They need to know which claims the agent is making, what supports them and where the limits are.

## Ask the agent to connect claims to proof

The agent made an [acceptance ledger](../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md), with one row for each written requirement. It distinguished fresh automated tests from earlier recordings of real Revit and desktop sessions. It also kept independent review separate from the implementing agent's own checks.

That distinction matters in ordinary work. “The tests passed” does not tell you whether the test used the real application, the file you intended, or the build you will give participants. Ask the agent to name those things rather than interpreting a confident summary as proof.

For this example, all five prepared cases had matching results in Codex desktop and Claude Desktop Chat. The deliberate gaps were particularly useful: a result with zero missing fields still could not pass when an element was absent or a type had no rule. Both applications also rejected an old context after a model switch and recovered with a new one. Those are concrete demonstrations of the tool's boundaries.

## Keep the gaps visible

Consolidation exposed details that a simple pass/fail label would hide. The normal binaries used in Revit came from an earlier source revision than the current checkout. The current build passed automated checks, but that alone did not prove it had run in Revit. The agent put the source difference in the packet instead of quietly treating earlier live evidence as current.

The earlier SDK cancellation defect also remained open. Explicit cancellation messages worked; canceling a local SDK wait did not reliably send one. The author's decision to continue with the verified approach was recorded as a decision, not as a fix. Actual desktop Stop-button behavior and some native shutdown observations still needed verification. Claude's AI model and permission mode had been recorded, while the equivalent Codex details were missing.

This is useful participant behavior: ask the agent to preserve uncertainty in a form that someone can act on. A limitation with a location, reason and next verification is more useful than either hiding it or stopping all work without explanation.

## Hand over a concrete version

The [R3 packet](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) names the implementation commit, the installed versions, the approved requirements, the source comparison and the evidence to read. Its suggested reviewer prompt asks what can close and what cannot. The agent prepared the packet; it did not invent a reviewer, send a message or award itself approval.

**Suggested follow-up prompt for participants** — not another historical instruction:

> Prepare a review packet at a specific Git commit. For each requirement, show the evidence and whether it came from an automated test, the real application or a prior review. Identify differences between the tested build and current source. List known defects separately from things we have not tested. Give the independent reviewer enough context to challenge the claims without our chat history.

When the review comes back, preserve the original findings, ask about any shared missing concept, and record decisions and correction tests separately. A clean review recommendation still does not replace your decision about accepting the result. Here, the packet is ready to hand over with open items; full R3 and final acceptance have not happened.

The [technical companion](../how-to/prepare-r3-evidence.md) gives agents the exact process. The [starting guide](../start-here.md) remains the participant entry point.
