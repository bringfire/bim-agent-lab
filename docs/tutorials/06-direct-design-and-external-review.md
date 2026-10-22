# Ask for a design, a plan, and independent review

**Reading this history:** the story below preserves the spec/plan review sequence. The author later approved Native execution and the bridge was implemented. Continue with [preparing the R3 evidence review](15-prepare-an-evidence-based-review.md) for the current participant lesson and [acceptance ledger](../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) for what passed and what remains open.

This participant story explains how the author directed a reviewable design process and then challenged the reasoning behind several review findings. Its [agent guide](../how-to/run-spec-plan-and-implementation-reviews.md) gives the technical procedure. Spec revision 2 is reviewed and approved. External R2 found three major gaps and one minor inconsistency in the first plan; [plan revision 2](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) now has all four findings externally closed at the plan level on `cfbfe04`. At that planning checkpoint, author approval and execution-method selection were still pending and the bridge was unimplemented.

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

## Approve the artifact, then ask for the next one

The [follow-up reviewer](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md) closed all three findings at the specification level and recommended proceeding to planning after author approval. The author then gave this actual instruction:

> Approve revision 2 and write the plan

This separates two decisions participants can reuse: the reviewer assesses whether the corrections resolve the issues; the person directing the work decides to proceed with the written design. Neither decision demonstrates that software works yet.

The agent turned the approved design into a [nine-task implementation plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md). Participants need not review every method signature. Look for whether the plan tests the riskiest assumption early, protects the existing plugin, defines independently checkable results, and provides a way to undo installation.

Here, the reviewer identified the remaining uncertainty: can **both desktop applications actually use the required protocol?** The plan begins with a clearly labeled test-only probe in Codex desktop and Claude Desktop before building the Revit connection. A probe pass would establish communication compatibility; later tests still have to demonstrate the real model check, including incomplete results. Ask the agent what each piece of evidence proves before accepting a confident summary.

The [R2 packet](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md) gives an external reviewer the plan, approved spec, relevant source, and questions that can expose missing steps. The agent recommends implementing in the current session because the connection, queue, and document checks share interfaces; the author has not yet chosen an execution method. External plan and implementation reviews remain part of either approach. This demonstrates directing the agent to make a recommendation while retaining the human decision.

## Ask what connects several findings

**When an independent reviewer reports two or three major findings, or more, ask whether the coder is missing a shared concept or way of reasoning.** Treat the cluster as a signal to investigate. The count alone cannot establish a common cause: findings can be unrelated, and even one finding can expose a deeper gap. This is a practical teaching heuristic, not a measured rule about how often coders make mistakes.

The person directing the work does not need to diagnose the technical issue themselves. Ask the **independent reviewer** to look across the findings and explain any shared weakness in the coder's current approach. A useful explanation connects the evidence, teaches the missing concept, and helps the coder recognize other places where it applies. A vague label such as “needs better error handling” gives the coder little guidance.

This is a **suggested prompt to the reviewer**, not a historical quotation:

> You found several major issues. Before we ask the coding agent to fix them, do they point to an underlying concept, assumption, or framework it is missing? Explain which findings that would connect and what evidence supports it. If they are separate problems, say so. If there is a shared gap, explain the concept in plain language and give the coder a way to apply it, including where else to look and how to verify the correction.

Then pass the reviewer's explanation, along with the original findings, back to the **coding agent**. This is a suggested follow-up:

> Read the reviewer's diagnosis and check it against the actual work. Explain the missing concept in your own words, show how it connects the findings, and inspect other relevant parts for the same mistake. Revise the plan or implementation using that understanding. Map each correction to evidence or a test, and identify any part of the diagnosis you disagree with and why.

Ask the reviewer to check the revised work for both the individual fixes and application of the concept elsewhere. The evidence of learning is a better artifact and appropriate verification, not the coding agent saying “I understand.” Keep the insight in the repo so the next agent can use it without needing this conversation. A concept or framework here means a way to reason about the problem; it does not necessarily mean adding a software library or redesigning the architecture.

### Our example: what remains safe when work stops halfway through?

The [external R2 review](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-feedback.md) recommended **revise before implementation**. It found three major gaps: cancellation could be lost before a Revit read, stopping the server could leave it waiting on stdin, and an interrupted installation might lack the backups needed to recover. A smaller inconsistency concerned how instance-selection errors reached the agent.

The author did more than ask for four local fixes. The accompanying message identified a common reasoning gap:

> The common gap is reasoning about what remains safe when an operation stops halfway through.

It then supplied this direction:

> Walk each operation from admission through completion, including cancellation, timeout, disconnect, shutdown, and partial failure. For every stage, identify who owns the resources, which conditions must still hold before the next action, and how the system reaches a safe final state. Carry those responsibilities into the interfaces and test failures at the transitions.

This is a reusable way to engage an agent. Several findings can reveal a shared blind spot. Ask the agent to explain that pattern, check it against source, and revise the reasoning that generated the plan. The original self-review had checked whether tasks covered the requirements; it had not established that those guarantees survived the handoffs between tasks.

The revised plan starts with ownership across the whole operation. In plain terms: stopping your wait does not necessarily stop the Revit read; the read still needs its resources until it finishes, and its late result must not be presented as current. Similarly, an installation needs verified backups **before** replacing anything. A list of the files it intends to install cannot restore the files it already overwrote.

The plan now calls for tests that deliberately interrupt preparation, reads, shutdown, installation and recovery. Participants can ask to see those test definitions and, later, their actual results. At this checkpoint they are planned tests, not demonstrations that the bridge is safe.

The review also changed a provenance detail. The agent now proposes adapting a small stdin-cancellation helper from the **official C# SDK 2.2.0**, with source attribution and license/notice review. That is different from merely consuming its package. The old **TypeScript SDK 1.26.0 remains a learning precedent**, with no code selected for reuse. At that review checkpoint no helper code had been copied. The later [execution lesson](07-test-desktop-compatibility-first.md) records the actual attributed adaptation and tests. Asking the agent to keep its provenance account current is part of directing the work.

The author returned the [external R2 follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) on plan revision 2 at `cfbfe043bd6c4d0627424ea3b099d0007459cb91`. The reviewer closed all four findings **at the plan level** and explicitly said the underlying planning problem was addressed. It checked that ownership responsibilities appeared in interfaces and interruption tests, including settings changes and evidence capture. No builds, installation, desktop compatibility checks or Revit tests were performed.

## Close the review loop without skipping the human decision

The useful engagement pattern is now visible: the author asked what connected several findings; the agent revised its account of the whole operation; an external reviewer checked both the common reasoning and each correction. Keeping the original findings, revised plan and returned review makes that progress assessable. The agent cannot close a reviewer finding just by saying it has made a change.

The reviewer's recommendation is to proceed to **author plan approval and execution-method selection**. That was the next decision, distinct from the already-approved specification. Native execution means this agent implements the connected tasks here; Subagent-driven execution assigns fresh implementers and reviewers per task. The agent recommends Native because these tasks share important interfaces. The author subsequently approved revision 2 and selected Native execution; see the [approval record](../superpowers/reviews/revit-mcp-bridge/2026-10-20-plan-approval.md). Both retain the requested independent implementation/evidence review.

A useful next prompt for participants to adapt is: **“Show me which reviewed revision I am approving, recommend an execution method, and explain what evidence still has to be produced.”** This is a suggested prompt, not a historical author instruction. Review closure establishes that the plan addresses the findings; the planned tests must still establish the software's behavior.

## Reuse this way of directing an agent

This is a **suggested teaching prompt**, not another historical instruction:

> Use the brainstorming protocol for this new subsystem. Read our existing research and source, reflect the intended outcome, and separate my requirements from your proposed choices. Write a spec with explicit boundaries, source provenance, failures, and independently checkable acceptance. Prepare a handoff for an external reviewer at a pinned Git revision. After we review the spec, write the implementation plan and prepare its review. Keep original reviewer findings and our dispositions. Capture how I directed the work for participants and the reproducible procedure for future agents.

As an exercise, open the spec-review packet and identify one way a check could return the wrong model's data. Ask a reviewer to show whether the spec prevents it and what test would demonstrate that. Then compare the response with the written requirement rather than accepting a confident summary.

Current evidence includes approved spec revision 2, external R2 findings, the author's lifetime/ownership direction, source-backed dispositions, plan revision 2 and external closure of all four findings at the plan level. The author subsequently approved the plan and chose Native execution. The [next lesson](07-test-desktop-compatibility-first.md) records implemented probe tests and a failed desktop compatibility checkpoint; Revit connection work and live acceptance remain pending. Find actual checkpoints using `git log --oneline -- docs/superpowers/`; earlier readiness passes do not prove the bridge works.
