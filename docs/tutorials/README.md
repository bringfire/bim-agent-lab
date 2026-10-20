# Learning path

Begin with the [repository README](../../README.md) and [current status](../reference/development-status.md). Participants follow how the author framed the work, engaged the agent, made decisions, and assessed evidence. Agents use the linked technical guides to reproduce or extend each increment. Reading the stories requires no Revit installation.

Research, Git, setup, readiness, Excel, and the MCP bridge comparison each have two linked layers. Actual prompts and later suggested prompts are labeled. Both layers point to the same research or acceptance evidence and real Git milestones; the [lesson template](../../templates/lesson.md) carries this approach into future work.

| Step | Participant question and story | Technical guide or supporting evidence |
|---|---|---|
| 1. Research existing work | [How did we ask for research before choosing tools?](00-research-before-building.md) | [Agent research guide](../how-to/research-existing-solutions.md), [resource register](../research/revit-resources.md), [code map](../research/reference-code-map.md) |
| 2. Direct an agent with Git | [How did we ask for source inspection and a reviewable history?](01-directing-an-agent-with-git.md) | [Agent Git guide](../how-to/work-with-git.md), [clone procedure](../how-to/clone-research-repositories.md) |
| 3. Define a useful demonstration | Turn the BIM question into explicit scope and expected answers | [Demo brief](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md); larger physical-condition and quantity cases remain planned |
| 4. Understand the decisions | See how direction and evidence changed the work | [Build journal](../build-story/journal.md), including actual prompts and corrections |
| 5. Prove the development setup | [How did we turn prerequisites into a working proof?](02-build-a-read-only-revit-command.md) | [Agent setup guide](../how-to/build-read-only-revit-command.md), [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) |
| 6. Build the first readiness check | [How did we request a meaningful rule and independent verification?](03-check-required-type-information.md) | [Agent readiness guide](../how-to/build-required-information-check.md), [contract](../../projects/revit-readiness/docs/required-type-information.md) |
| 7. Add Excel output | [How did we direct a complete enhancement?](04-add-an-excel-report-dependency.md) | [Agent dependency guide](../how-to/add-a-plugin-dependency.md) |
| 8. Research the MCP connection | [How did we compare bridges and verify the latest versions?](05-research-and-choose-the-mcp-bridge.md) | [Agent bridge guide](../how-to/choose-and-verify-an-mcp-bridge.md), [dated comparison](../research/revit-mcp-bridge-comparison.md); implementation and runtime acceptance remain planned |
| 9. Package and extend | Use the repo with an agent after the seminar | [Extension workflow](../how-to/extend-a-project.md); release packaging remains planned |

Ask your agent to explain one step, identify what the person decided and what was delegated, and show the relevant evidence. Then choose a small next task with an observable answer. Historical results describe the recorded configuration; a new implementation or workstation needs its own verification.
