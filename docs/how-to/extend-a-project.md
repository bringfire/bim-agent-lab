# Extend a worked example with an agent

Begin with the [participant starting guide](../start-here.md) and [agent handoff](agent-handoff.md) for current capabilities and remaining verification. This is the contribution process. The Excel enhancement provides a worked [participant account](../tutorials/04-add-an-excel-report-dependency.md) and [technical guide for agents](add-a-plugin-dependency.md).

1. Read the project README, AGENTS.md, and current verification evidence. Inspect the Git state.
2. Describe the BIM question, inputs, scope, and expected output in a [feature brief](../../templates/feature-brief.md). For a readiness rule, state the source of its requirements and its valid exceptions.
3. Ask the agent to locate the existing flow and research the missing API or dependency before changing code. Review its evidence and proposed boundary.
4. Create a feature branch and define independent passing, failing, and excluded examples. Identify which tests need the host application.
5. Implement one complete change through data reading, rule evaluation, result reporting, and any MCP exposure. Let the project architecture determine the affected files.
6. Compare results against the expected cases and source data. Capture the actual commands, application version, and outcomes in a [verification record](../../templates/verification-record.md).
7. Review the diff and notices, capture learning in both layers, and commit a coherent checkpoint. The participant lesson explains the author's framing, prompts, decisions, and evidence; the linked agent guide records the implementation procedure and technical constraints. Use the [lesson template](../../templates/lesson.md). Publish only to the intended destination as directed.

Suggested prompt:

> Read this project's instructions and current implementation. I want to add [BIM capability] for [intended use]. Establish the required inputs and independent acceptance cases. Identify the smallest complete change, explain the evidence behind it, implement it, and show me the verification and diff. Record anything you could not test.

To start a different BIM tool, create a sibling project with its own brief, prerequisites, implementation boundaries, and tests. Reuse the research and teaching process, then justify any code reuse from the actual requirements.

For the Revit readiness server, the [extension roadmap](../explanation/mcp-extension-roadmap.md) lists planned stages (more Revit years, more readiness rules, framing output, file checks, exports, and model changes last) and what each must prove before it counts as supported.
