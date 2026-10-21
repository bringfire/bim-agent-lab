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

Current outcome: the compatibility checkpoint failed for both installed desktops. The temporary configuration entries were removed; dependent bridge tasks remain unstarted. The next decision is whether to retain the strict protocol requirement and find supporting client versions, or explicitly revise it and test a compatible exchange. No alternate protocol has yet been demonstrated in both desktops. Find the implementation checkpoint with `git log --oneline -- projects/revit-readiness/src/mcp-server`.
