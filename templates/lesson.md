# Lesson template: participants first, agents next

Use this for a completed increment. Create or update a participant lesson in `docs/tutorials/` and a linked technical guide in `docs/how-to/` or the project's docs. Keep both tied to the same dated research or verification evidence and the real Git checkpoint. For work that predates this repo, identify the import checkpoint and original evidence rather than inventing earlier history. The [Excel participant lesson](../docs/tutorials/04-add-an-excel-report-dependency.md) and [agent guide](../docs/how-to/add-a-plugin-dependency.md) are the worked example.

## Participant lesson: how did we engage the agent?

- **Work purpose:** who needed the capability, what task it supports, and what the participant will learn about directing an agent.
- **Starting context:** what already existed and which earlier instructions mattered. Reading the story should not require build tools.
- **Actual prompts:** quote or excerpt accurately, label the source, and distinguish later suggestions. Do not invent dialogue to make the story smoother.
- **Why the direction helped:** show how purpose, scope, research, resources, Git, independent examples, and requested deliverables shaped the work.
- **Who decided what:** identify the human's domain judgments and constraints, and the implementation decisions delegated to the agent. Do not credit the human with prescribing choices the agent made.
- **Surprise and response:** explain a failure or correction at the level of work and evidence. Show a useful follow-up question; label it as suggested if it was not actually asked.
- **Evidence the person can assess:** show the expected answer, observed result, relevant artifact, and limits. Explain what a passing test does and does not establish.
- **Reuse:** provide one adaptable prompt and a small exercise with a checkable result. Link the companion agent guide for technical execution.

Use plain language and a short narrative. Include commands only when understanding the command is itself the participant's learning objective. Keep detailed logs, package inventories, and debugging instructions in the technical guide or linked evidence.

## Agent guide: how can the next agent reproduce or extend it?

- **Context and authority:** link applicable `AGENTS.md`, current status, source commit, and dated acceptance. Historical prompts/results do not authorize new actions or prove a new change passes.
- **Source map and boundaries:** identify the files, contracts, data flow, preserved behavior, and explicit scope of the increment.
- **Research and resources:** record source repositories and revisions, SDK/API/runtime requirements, dependency versions, licenses/notices, and unresolved assumptions.
- **Procedure:** give ordered commands or supported tool operations, working directory, required inputs, install/upgrade behavior, and prerequisites for host actions.
- **Acceptance:** specify independent cases, expected values and identities, units/scope/coverage, and which checks require the real host.
- **Failure handling:** retain observed errors, identify the failing boundary, preserve useful outputs, and document the implemented correction and regression evidence.
- **Completion:** identify the evidence, relevant checks, documentation updates, review, and Git checkpoint; label unverified behavior explicitly.

## Link and check the record

Add the participant lesson to the learning path and expose the technical guide through agent instructions or project navigation. Link each to the other and to the same evidence. Update the build journal. Check links, prompt provenance, status claims, and agreement with the source. Do not present a planned feature as a completed walkthrough or reuse a historical pass as fresh verification.
