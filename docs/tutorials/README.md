# Learning path

Begin with the [repository README](../../README.md) and [current status](../reference/development-status.md). The materials cover research, design, and the first working Revit command. Further implementation chapters will be written alongside the code.

Before building the Revit example, use the [development prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) to have your agent inspect your workstation and identify missing tools. Reading the teaching materials does not require a development installation.

| Step | Learning outcome | Available material |
|---|---|---|
| 1. Direct an agent with Git | Identify the working repository; clone, inspect, review, and checkpoint | [Git lesson](01-directing-an-agent-with-git.md) |
| 2. Research existing work | Evaluate SDKs, APIs, dependencies, source behavior, and reuse | [Resource register](../research/revit-resources.md), [code map](../research/reference-code-map.md) |
| 3. Define a useful demonstration | Turn BIM questions into explicit scope and expected answers | [Demo brief](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md) |
| 4. Understand the decisions | Recognize how user direction and evidence change the plan | [Build journal](../build-story/journal.md) |
| 5. Prove the development setup | Check prerequisites, build/register a command, and verify its answer in Revit | [First-build tutorial](02-build-a-read-only-revit-command.md), [fixture recipe](../../projects/revit-readiness/samples/model-recipes/read-only-probe.md) |
| 6. Build and expose a readiness check | Trace an operation from model data to a deterministic result and MCP | Planned after feasibility evidence |
| 7. Verify, package, and extend | Reproduce results, install a release, and add a new capability | [Extension workflow](../how-to/extend-a-project.md); executable walkthrough still to be developed |

Ask your agent to explain one step, perform the relevant inspection, and show the evidence. Use the [lesson template](../../templates/lesson.md) when a completed implementation becomes a new chapter.
