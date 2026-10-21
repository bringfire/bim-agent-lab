# Ask for a small, tested step toward the Revit connection

Participants: this follows the [desktop compatibility story](07-test-desktop-compatibility-first.md). Agents: use the [technical guide](../how-to/build-and-verify-bridge-foundations.md). Both link to the same [Task 2 evidence](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations.md).

## How the author directed the next step

After proving the connection in Codex desktop and Claude Desktop, the author gave this actual instruction:

> Ok, let's proceed with Task 2 and start building the actual Revit bridge. Follow the updated plan, keep the compatibility tests in place, and record the process in both teaching layers.

This was a specific next step within an already reviewed plan. It carried earlier decisions forward: keep the desktop applications suitable for beginners, preserve the ability to use the newer protocol, work through small Git checkpoints, and teach both people and future agents. The agent did not need the author to choose these things again.

Task 2 builds the message and timing rules shared by the server and the Revit add-in. Think of it as agreeing on the contents of a request, how to recognize a complete reply, and when work has expired. The actual connection to Revit comes in later tasks. Stating that boundary prevents a successful component test from becoming a misleading claim that the whole demonstration works.

## What the person decided, and what the agent handled

The author selected the task, retained the compatibility requirement and requested the teaching record. The agent translated that direction into code, tests, research into the framework libraries already available, and a reviewable Git checkpoint. No new downloaded library was needed for this step.

The earlier independent review had identified a shared weakness: the plan needed to describe what happens when an operation stops halfway through. The agent carried that insight into this implementation. A request keeps its original time allowance as it moves between components. Canceling a reply does not mean an unfinished callback has stopped using its resources. Cleanup must wait for that work to finish.

These are implementation decisions the agent made under the approved plan, not technical details the author had to prescribe. A useful human question is whether the agent turned the reviewer's insight into observable tests, rather than just more reassuring prose.

## What the tests revealed

The agent first wrote tests against incomplete code and confirmed that they failed on both runtime versions. It then implemented the behavior. This gives evidence that the tests can detect the missing work.

Testing also exposed a library assumption. A framework JSON reader accepted a complete object followed by an extra object. An additional check found that the older runtime's parser could replace a broken Unicode character. Neither behavior was acceptable for these messages. The agent checked where the data changed, used the planned runtime parsers with explicit message validation, and retained the failing inputs as regression cases. It did not weaken the expected answers to get a pass.

The lesson is that “we used an existing library” is a starting point, not proof of the behavior we need. Research identifies useful resources; boundary tests show how those resources behave in our application.

## Evidence the participant can assess

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations.md) records 27 checks passing on each runtime, messages exchanged in both directions, and an independent Python check of their contents. It includes incomplete messages, size limits, expired requests, cancellation and deferred cleanup. The desktop-protocol regression suite still passes, including the newer `2026-07-28` protocol. Existing readiness and Excel tests also pass.

This is offline component evidence. The agent did not install a new Revit add-in, run the readiness tool through MCP, or repeat the desktop UI demonstrations during Task 2. The prior desktop evidence remains preserved. The next step is the local connection between processes; live sample-model checks and independent implementation review remain required.

## Reuse the approach

This is a **suggested prompt**, not an additional historical instruction:

> Implement the next named task in our approved plan. Carry forward the decisions we already made and preserve the earlier regression tests. Show that the new tests fail before the behavior exists, then pass after implementation. Use the reviewer's broader insight to test interruptions between steps. Explain any library assumptions the tests disprove. Record how I directed the work for participants, and give the next agent precise reproduction steps and limits.

For a short exercise, ask your agent to explain the difference between a canceled response and finished work. Have it point to the test that waits to release resources until the callback finishes. A satisfactory answer identifies both moments and explicitly says that this test does not prove the later Revit callback integration.
