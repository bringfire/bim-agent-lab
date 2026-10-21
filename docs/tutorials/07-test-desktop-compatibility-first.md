# Ask the agent to prove the uncertain connection first

Participants: this story follows the [design and external review lesson](06-direct-design-and-external-review.md). Agents: use the [technical procedure](../how-to/verify-desktop-mcp-compatibility.md). Both point to the same [compatibility evidence](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md).

## How the author started execution

After the reviewer closed the plan findings, the author gave this actual instruction:

> Ok, I approve plan revision 2. Let's use Native execution and start with the compatibility check in Codex desktop and Claude Desktop. Keep recording the process in both teaching layers.

This approved a specific reviewed plan and chose an execution method. Native meant the coding agent would implement the connected tasks in this conversation. It did not remove the independent implementation review required later. The [approval record](../superpowers/reviews/revit-mcp-bridge/2026-10-20-plan-approval.md) identifies the exact revision.

The author also chose where to start: prove that the two seminar applications could communicate using the required MCP protocol before connecting the server to Revit. A working SDK example alone would not answer that question.

## What we delegated, and what we checked

The agent built a small separate test server using the selected official C# SDK. Its single tool returns a clearly labeled synthetic success or deliberate error. It has no access to Revit. That let us test the connection without involving a model, checklist or add-in installation.

The agent also refreshed the dependency research, tested shutdown and settings recovery, and prepared reversible temporary entries for both desktop applications. The user reloaded Codex's MCP servers and replied **“reloaded.”** Claude's configuration was reloaded through its desktop menu. The agent captured the messages each application actually sent.

The result was a useful failure:

| Test | Observed result |
|---|---|
| Official SDK test client | Discovery, synthetic success and structured error passed on `2026-07-28` |
| Codex desktop | Sent `2025-06-18`; rejected before tool discovery |
| Claude Desktop | Sent `2025-11-25`; rejected before tool discovery |

The approved plan required `2026-07-28` in both desktops and explicitly excluded older fallback. The agent therefore could not turn the SDK pass into a desktop pass or continue the dependent Revit work. The [recorded result](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) distinguishes the installed app versions, requested protocols, exact error and remaining work.

## What this teaches about directing the agent

Ask for the smallest experiment that can disprove an important assumption. Here, installing a current SDK did not establish what the seminar applications supported. We learned that before building the Revit connection.

Ask the agent to preserve disappointing results. The wire evidence makes this a specific compatibility issue that we can investigate, rather than a vague report that “MCP did not work.” It also protects the decision: changing an acceptance requirement should be a visible author choice.

The version check found another distinction worth teaching. NuGet listed **2.3.0** as the newest stable SDK at execution time; the approved plan selected **2.2.0**. The agent retained that pin and recorded the newer release. “Check the latest” means gather current evidence and explain the selection; it does not mean silently upgrade a reviewed implementation.

The earlier review's shared insight—reason about what happens when work stops halfway through—also helped in practice. Shutdown tests exposed double disposal of the same stream, which the agent corrected and retested. During cleanup, Codex had added `enabled = true` to its temporary entry. The recovery script refused the unexpected edit. The agent inspected it, saved recovery information, and removed only the verified probe entry. These are observed examples of the lifetime and ownership lesson; they do not establish the still-unbuilt Revit bridge's behavior.

## Keep the source-use account current

We consume the official C# SDK **2.2.0** package. We also adapted its small stdin-cancellation helper, with source attribution and retained licenses, to handle reads that can outlive cancellation. The host composition and trace wrapper are original lab code. The older **TypeScript SDK 1.26.0 remains a research precedent**; its code was not copied or installed. The SDK supplies MCP parsing and dispatch. We did not write our own MCP implementation.

## A prompt participants can reuse

This is a **suggested prompt**, not a historical instruction:

> Before building the full integration, identify its least certain assumption and propose the smallest test of it in the applications we will actually use. Record the versions and messages exchanged. Keep a library test separate from a real application test. If it fails, preserve the evidence, explain the choices, and tell me which requirement would need to change. Record how I directed this work and how another agent can reproduce it.

That first checkpoint failed for both installed desktops with their tested settings. Its temporary configuration entries were removed; dependent bridge tasks remained unstarted. The real implementation checkpoint is `9ea3151b323ae50b919fed2e4cb89e10362e598a`. The author's next question led to the investigation below.

## Challenge whether a blocker serves the purpose

The author first asked:

> wait sorry...neither desktop apps support the new MCP protocol?

Then gave this actual instruction and question:

> Ok, check whether newer versions or settings in Codex desktop and Claude Desktop support 2026-07-28 before we change the requirement. Record what you find in both teaching layers., but I still don't fully understand. Is this a footgun and self-imposed restriction or something real?

This is a useful way to direct an agent: ask it to examine its own recommendation before accepting a limitation or changing the goal. The author did not need to know the implementation detail that caused the failure.

The answer is that **the protocol difference is real, but we chose to make it a blocker**. The current SDK already knows how to communicate using both old and new revisions. Our server deliberately accepts only the new one, and the written acceptance criteria require both desktop applications to use it. The agent should have explained the difference between using a modern SDK and requiring every application to speak only the newest protocol before recommending that restriction.

To test that explanation, the agent held the SDK version constant and changed the protocol setting in a separate, small diagnostic. All three tested revisions worked with the SDK's compatibility default; only the newest worked with our strict setting. This is evidence about the cause of the rejection. It is not yet proof that the real desktop demonstrations work, and it does not change the approved requirement.

The update research also narrowed the original claim. Codex's updater offered no eligible update, but its installed engine exposed an experimental setting for the new protocol. The agent prepared a reversible experiment, and the author replied **“reloaded”**, then **“restarted the app.”** Fresh desktop traffic after each action still used the older protocol. The tested setting did not resolve the failure in this installation. Claude's public Windows download matched the installed version. The inspected documentation did not identify a local-server switch or a Desktop version that resolves the failure. A general rollout announcement is not enough to claim success in our particular connection. The agent removed its temporary settings afterward, preserving unrelated changes.

The [dated investigation](../research/mcp-desktop-protocol-investigation.md) preserves the evidence and limits. Keeping SDK 2.2.0 while permitting tested older client exchanges would not mean adopting SDK 1.x, copying the old bridge or writing our own protocol. No new-protocol-only capability has been identified as necessary for these readiness questions. The author can use that information to decide whether the strict restriction serves the demonstration; the agent must not quietly change the contract to get a green result.

### A follow-up prompt to reuse

This is a **suggested prompt**, not a historical instruction:

> Before treating this as a product limitation, separate the library version, the application's actual capabilities and restrictions we added ourselves. Check current releases and settings. Identify which required feature needs the restriction, and test the smallest alternative without changing our approved requirements. Show what the evidence proves and what still needs testing in the real application.

The lesson is broader than MCP: challenge a proposed constraint by asking what purpose it serves and what evidence distinguishes an external limitation from a choice the agent made.

The author also asked **“please explain what is going one here clearly.”** That matters: a series of technical experiments is not a substitute for explaining the decision. The agent's plain-language correction was that the toolkit understands several message formats, while our added rule rejects all but one. At that point its recommendation was to keep SDK 2.2.0, permit its built-in compatibility and verify the actual readiness workflow in both applications. The approved requirement has not been changed; doing so is a separate, explicit decision. The next experiment below adds another option.

The author then challenged a further evasive distinction: whether this meant returning to the protocol generation associated with SDK versions 1.28 or 1.30. The agent had emphasized formal date-based protocol names instead of answering the consequence. Official source confirmed that those Python SDK 1.x releases support the very revisions the desktops requested. **Yes: the proposed desktop demonstration would use a 1.x-era protocol through SDK 2.2.0.** Keeping a newer toolkit does not make that exchange a demonstration of the new protocol. The [research correction](../research/mcp-desktop-protocol-investigation.md) records the exact sources. Teach participants to insist that an agent explain the practical tradeoff, even when its narrower terminology is technically defensible.

## Ask what we lose and test another client

The author's next actual prompt was:

> Ok, what would we give up by using the older protocol? Check whether Claude Code or another agent client can actually use 2026-07-28 before we change the requirement and is this specific to C# or is it across all supported languages?

This challenged the proposed compromise before approving it. The author asked three useful questions: what changes for our work, whether another application can meet the original intent, and whether changing the programming language would help.

The agent found documented support in Claude Code and tested it with the same strict server. An expired login initially allowed discovery but stopped the agent from calling tools. The author signed in and replied **“logged in.”** The next run passed discovery, a successful call and a deliberate error on the new protocol; an independent check confirmed the actual messages. The tested client was **Claude Code 2.1.305**, not Claude Desktop. The [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md) preserves both attempts.

The result changed the choice. We could retain the new protocol by changing the seminar client scope, rather than accepting older exchanges in the original desktops. Our present readiness questions do not require the new mechanisms, but teaching the new protocol itself is a legitimate goal. The [tradeoff comparison](../research/mcp-protocol-tradeoffs-and-agent-clients.md) separates that goal from future remote-service and long-running-work benefits. No requirement changed during the experiment.

The language question also received direct evidence: the alternative client's TypeScript-based runtime worked with our C# server. Changing our server language would not, by itself, change what the original desktops send.

**Suggested follow-up prompt:**

> Before recommending a compromise, tell me what my users would lose and test one concrete alternative that preserves the original goal. Keep the implementation fixed where possible. Show actual success and failure messages, distinguish application versions, and tell me exactly which requirement each option would change.

The teaching point is to make the agent investigate the decision, not merely defend its first recommendation. A failed test narrows what is known about one configuration; it does not eliminate all alternatives.

## Let the audience determine the tradeoff

The author then supplied a decisive constraint: participants are beginners who will not welcome Claude Code's terminal interface. They directed us to use compatible exchanges in the desktop applications while preserving a future move to the new protocol.

That changes the recommendation even though the Claude Code test passed. A technically working option can still be wrong for the audience. The [policy amendment](../superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) keeps the newer SDK, supports both protocol generations through that SDK, and retains a separate new-protocol test. Participants continue with Codex desktop and Claude Desktop. When those applications support the new revision, fresh compatibility and model tests establish the upgraded baseline; the BIM logic does not need a rewrite.

The author selected the direction, and the written requirements now reflect it. At that documentation checkpoint, runtime implementation and both desktop tests were still pending. The next section records their execution. Teach the distinction between choosing a sensible compromise and verifying that it works.


## Ask the agent to implement the choice and preserve the way forward

The author's actual next instruction was:

> Ok, let's apply the compatibility change and test it in Codex desktop and Claude Desktop. Keep the 2026-07-28 regression test and record the process in both teaching layers. Do we need an updated spec and plan?

Yes: the requirement changed, so the specification and plan needed to say what would now count as success. Revision 3 already captured that direction. This explicit instruction authorized implementing it; another request to approve the same choice would have added no useful decision.

The agent kept SDK 2.2.0, enabled its normal compatibility behavior and retained a separate strict new-protocol test. The author reloaded Codex, then handled Claude's tool-permission prompt and replied **“always allowed.”** Both applications discovered the test tool, returned a successful result and correctly handled the deliberate error. Codex used `2025-06-18`; Claude Desktop used `2025-11-25`. The strict `2026-07-28` test also passed. These are older desktop exchanges through the newer SDK, not evidence that either desktop used July's protocol.

Testing the actual application caught something the library test missed: Claude left out an optional empty field when asking for the tool list. Our checker incorrectly required it. The agent checked the source definition, wrote a failing test, corrected the checker and reran it against the unchanged capture. This is why a test failure deserves investigation: the product, the test or the setup can be wrong. Changing evidence to make a test pass would teach the wrong lesson.

The agent also found that a temporary registration was no longer present after an interruption. It saved the current configuration before reapplying that entry, then removed the test entries while preserving unrelated edits. Recovery must work with the settings that exist now, not assume nobody changed them since the experiment began.

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) holds the exact versions, outcomes, evidence and limits. This checkpoint proves communication; it does not prove the still-unbuilt Revit connection. The agent's next implementation task is the bridge's messages, framing and shared request budgets. Full model tests and independent implementation review remain required.

A **suggested prompt** for participants:

> Apply our chosen compatibility policy in the applications our audience will use. Update the spec and plan so the acceptance criteria agree. Keep a regression test for the newer capability, and tell me how we will verify an upgrade later. Show successful and failed calls in the real applications, preserve the evidence, and explain any test or setup problems separately from product failures. Record my decisions for participants and the reproduction steps for agents.

The useful human contribution was deciding whose experience mattered and insisting that today's compromise preserve tomorrow's option. The agent handled the implementation details and supplied evidence the author could judge.
