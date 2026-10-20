# Learning path

Begin with the [repository README](../../README.md) and [current status](../reference/development-status.md). The materials below cover the research and design work completed so far. Later implementation chapters will be written alongside the working code.

Before building the Revit example, use the [development prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) to have your agent inspect your workstation and identify missing tools. Reading the teaching materials does not require a development installation.

| Step | Learning outcome | Available material |
|---|---|---|
| 1. Direct an agent with Git | Identify the working repository; clone, inspect, review, and checkpoint | [Git lesson](01-directing-an-agent-with-git.md) |
| 2. Research existing work | Evaluate SDKs, APIs, dependencies, source behavior, and reuse | [Resource register](../research/revit-resources.md), [code map](../research/reference-code-map.md) |
| 3. Define a useful demonstration | Turn BIM questions into explicit scope and expected answers | [Demo brief](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md) |
| 4. Understand the decisions | Recognize how user direction and evidence change the plan | [Build journal](../build-story/journal.md) |
| 5. Prepare a sample and prove feasibility | Inspect actual Revit data; establish the smallest working query | Next investigation; [sample plan](../../projects/revit-readiness/samples/README.md) |
| 6. Build and expose a readiness check | Trace an operation from model data to a deterministic result and MCP | Planned after feasibility evidence |
| 7. Verify, package, and extend | Reproduce results, install a release, and add a new capability | [Extension workflow](../how-to/extend-a-project.md); executable walkthrough still to be developed |

Ask your agent to explain one step, perform the relevant inspection, and show the evidence. Use the [lesson template](../../templates/lesson.md) when a completed implementation becomes a new chapter.
