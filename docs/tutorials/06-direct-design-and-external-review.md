# Ask for a design, a plan, and independent review

This participant story explains how the author changed the next step from immediate bridge construction into a reviewable design process. Its [agent guide](../how-to/run-spec-plan-and-implementation-reviews.md) provides the technical procedure. An external review of the first [spec](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) has now returned three findings; revision 2 addresses them and awaits follow-up and author approval. The implementation plan and bridge remain pending.

## Begin with the work already done

The lab already had a verified Revit readiness check, three sample cases, and Excel output. The preceding [MCP research lesson](05-research-and-choose-the-mcp-bridge.md) compared an existing bridge with a small original one and distinguished current protocol versions from SDK package versions. That work was committed as `7d3ae71`.

The author then selected the minimal custom bridge and supplied the target SDK/protocol. These were actual prompts from this conversation:

> Ok, let's build the minimal custom bridge with C# MCP SDK 2.2.0 and protocol 2026-07-28. Verify our three sample cases end to end and record the process in both teaching layers in bim-agent-lab. Also, one important thing to emphasize is whether you are using any code from the SDK 1.26.0, whether you're just using it as a learning precedent, or whether you're going handroll your own from scratch.

Before implementation, the author added:

> I also want you to write a spec and implementation plan, with review sessions that we hand to an external reviewer, so that we can teach that skill...use the superpowers brainstorming skill protocol.

The second prompt preserves the build objective and changes how we proceed. The requested deliverable now includes the reasoning and review process that participants should be able to repeat with their own agents.

## Ask for artifacts that answer different questions

A **specification** states what the tool must do, the important boundaries, and the evidence that would count as success. An **implementation plan** turns the approved spec into ordered, testable tasks. A **review** challenges a particular artifact or implementation and returns findings with evidence.

The agent classified this as architectural work because the server and Revit connection introduce a new subsystem. Under the requested brainstorming protocol, the written spec is reviewed before the detailed plan is written. The written plan then receives review before implementation. The next artifact is determined by the current stage; asking for the overall build does not turn a draft into an approved spec.

| Review session | What the person wants to learn | What they hand over |
|---|---|---|
| Spec review | Are we solving the right problem, with clear boundaries and a credible way to verify it? | The written spec, research, relevant existing source, and expected sample answers |
| Plan review | Can an agent carry this out in small, checkable steps? | The approved spec, proposed tasks/tests, deployment procedure, and evidence plan |
| Code and evidence review | Does the delivered result meet the agreed contract? | The exact code change, dependencies, test results, and real Revit/MCP evidence |

These are development sessions, not additional seminar hours. During the two-hour seminar we can show selected handoffs, findings, and corrections. The repo contains the complete workflow for later use.

## Make source reuse a question the agent must answer

The author's provenance question prevents “custom bridge” from hiding several different choices:

- The **official C# SDK 2.2.0** will supply the MCP implementation as a library dependency.
- The **older TypeScript SDK 1.26.0 and third-party bridge** are learning precedents; their implementation is not selected for copying or installation.
- Our **existing readiness reader and evaluator** will be reused.
- The **small Revit connection and tool adapter** will be original lab code.

This is the intended design, not a completed audit of code that does not yet exist. The implementation review must check the actual code and dependencies. If the agent later adapts an upstream snippet, it must record the source and license rather than continue saying everything was written independently. Using an SDK is different from copying its source, and writing an adapter does not mean writing a protocol from scratch.

## Give a reviewer enough context to disagree usefully

The author asked for an external reviewer. The agent prepared a [handoff packet](../superpowers/reviews/revit-mcp-bridge/01-spec-review.md) that names the artifact, supplies background, links the exact source and expected answers, and asks for specific failure scenarios. It includes a review form that separates the reviewer's findings from the author's decisions.

A useful finding explains what could go wrong, where the problem is, the consequence, a small correction, and how to verify that correction. “Looks good” cannot establish that the right Revit document was queried or that a timeout will not produce a late answer. The reviewer must also say what they did not inspect or run.

The author returned feedback from Codex in a separate review context, which identified the exact reviewed commit `b56c0dc219e1269f5b90542004267a2d388f338d`. Its recommendation was **“revise before approval.”** The [original feedback and separate dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md) preserve what the reviewer said and how the drafting agent responded. The reviewer inspected source and documents but explicitly ran no builds or MCP/Revit tests.

## Use review to improve the definition of success

The review accepted the architecture and provenance boundaries but found three gaps:

| Finding | Why it matters to the person directing the work | What changed in the spec |
|---|---|---|
| Which agent application must work? | A developer's test client could pass while the application used on stage does not | The author selected Codex desktop and Claude Desktop; each must pass using the requested protocol, with its exact version/configuration recorded |
| Can zero findings still mean incomplete checking? | A missing checklist rule or unresolved model element must not become a reassuring readiness answer | Two temporary negative controls now prove the report preserves what was not examined and that each client explains the incomplete coverage |
| What if a connection sends nothing? | A connection can occupy a slot before a normal request timeout exists | A five-second limit now starts when the connection is accepted, with recovery checks |

The agent asked which application should satisfy acceptance. The author first clarified the meaning of “client” with this actual reply:

> are you asking about the agent desktop app like Codex or Claude code?

After the agent explained the distinction between the AI application and the MCP server, the author's actual selection was:

> Codex desktop and Claude Desktop

This is a useful division of responsibility: the person chooses the applications participants need; the agent inspects installed versions and writes the technical evidence requirements. A package version alone does not prove the application can use the requested protocol. Older-protocol fallback remains a compatibility observation rather than a pass for this demonstration.

The agent checked the incomplete-coverage finding against the source before changing the spec. A missing declared element and a missing type requirement are reported in different parts of the result. Revision 2 preserves that distinction and requires both controls to run after their test inputs are prepared and a fresh context is acquired. The original three sample answers remain unchanged. These are additional required checks, not runtime results already obtained.

All three findings were accepted and addressed in the specification. The follow-up packet asks the reviewer to check those corrections specifically. Editing a document does not close a review by itself, and returning feedback does not approve a revised document that the author has not yet reviewed. The drafting agent's self-review, external recommendation, and author's decision remain separately recorded.

## Distinguish the human's direction from delegated choices

The author chose the BIM purpose, custom-bridge direction, versions, three-case verification, provenance emphasis, two teaching layers, staged external review, and subsequently both desktop clients. The agent proposed the two tool names, process split, local connection, context validation, timeout/bounds, and evidence matrix. Those engineering choices belong in the spec where a reviewer can challenge them.

The spec gives the author practical questions to assess: Does the operation remain limited to our declared checklist? Does it reject a changed document? Do missing values remain findings rather than errors? Do failures remain distinguishable from a zero-finding result? Will the same 0/0, 2/6, and 3/8 sample answers be verified independently?

## Reuse this way of directing an agent

This is a **suggested teaching prompt**, not another historical instruction:

> Use the brainstorming protocol for this new subsystem. Read our existing research and source, reflect the intended outcome, and separate my requirements from your proposed choices. Write a spec with explicit boundaries, source provenance, failures, and independently checkable acceptance. Prepare a handoff for an external reviewer at a pinned Git revision. After we review the spec, write the implementation plan and prepare its review. Keep original reviewer findings and our dispositions. Capture how I directed the work for participants and the reproducible procedure for future agents.

As an exercise, open the spec-review packet and identify one way a check could return the wrong model's data. Ask a reviewer to show whether the spec prevents it and what test would demonstrate that. Then compare the response with the written requirement rather than accepting a confident summary.

Current evidence includes the external R1 review, source-backed dispositions, observed client versions, and spec revision 2. Follow-up review, author approval, implementation, and runtime acceptance remain pending. Find the design and revision checkpoints using `git log --oneline -- docs/superpowers/specs/2026-10-20-revit-mcp-bridge-design.md`; do not use earlier runtime passes as proof that the bridge works.
