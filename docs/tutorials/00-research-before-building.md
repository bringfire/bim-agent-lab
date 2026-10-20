# How we directed the agent: research before building

**For seminar participants.** Learn to make research and resource collection part of the assignment before asking an agent to build a BIM tool. The [agent research guide](../how-to/research-existing-solutions.md) contains the inspection procedure, source records, and technical questions.

This work began before BIM Agent Lab had its own Git history. The research was brought into baseline commit `3ca7a87`; the [build journal](../build-story/journal.md), [resource register](../research/revit-resources.md), and [dated evidence](../research/evidence/README.md) preserve what was observed. The original seven candidates were inspected, not built or adopted. The later Excel lesson records our first adopted library separately.

## Give the agent a professional purpose

The author's actual opening prompt was:

> Ok, take a look at the context of this folder and let's start figuring out what plugin we will create for Revit as an example and how we proceed to creating an MCP server for Revit that is geared toward BIM professionals.

The folder already contained audience and seminar context. Asking the agent to read it gave the conversation a starting point: a useful tool for wall and ceiling professionals, a short seminar, and a takeaway people could extend. The agent compared possible workflows rather than treating its first architecture suggestion as settled.

The author then chose the direction. This is an **excerpt from the actual follow-up**:

> Ok, let's lead with model readiness, with inventory and quantities supporting it. Let's define the three demo questions and what we'd need in a sample Revit model to demonstrate them, we can use the sample Revit projects that ship with Revit and if necessary we can modify them to suit our needs for demonstration.

That connected research to questions people would ask of a model and examples that could show whether the answers were useful. The author supplied the work priority; the agent developed the proposed demonstration and investigated how to support it.

## Ask for existing work and the resources it needs

The same prompt explicitly required research. Another **actual excerpt** is:

> The first thing I want to impress on people is that research for work that has already been done by others and collecting the required resources are necessary. SDK's, API's, libraries that are required, dependencies, and GitHub repos that might already exist that can be used as precedents, or as starting points if they have the proper license structure.

An API is the interface code uses to work with an application; an SDK supplies development resources and examples; a library is reusable code. A dependency is something our solution needs in order to build or run. Participants need to ask what role each resource serves, without knowing every package name beforehand.

The agent investigated official Revit resources, local software and samples, and seven source repositories. It recorded exact versions, relevant code, dependencies, and license evidence. The author then directed the agent to clone repositories for inspection, as told in the [Git lesson](01-directing-an-agent-with-git.md).

## Look for findings that change the plan

| Research finding | What it meant for our work |
|---|---|
| An existing query looked only at the current view and defaulted to a 100-element limit | A readiness report would need to say what it examined and whether anything was omitted |
| Revit API libraries and sample projects were already installed | Resource gathering could begin with what was available on the workstation |
| Candidate tools brought different packages, runtimes, and license terms | A promising repository still needed compatibility and reuse assessment |
| Some repositories had additional source components that were not downloaded | Obtaining the main repository did not prove it was ready to build |

The [source map](../research/reference-code-map.md) connects these findings to the inspected code. The useful deliverable was a reasoned comparison and a resource list, with open questions clearly marked. Research did not commit us to installing every tool or copying a particular bridge.

## Know what the person and the agent each decide

The author chose the professional audience, readiness as the lead workflow, the teaching purpose, and the need to investigate reuse. The agent located sources, traced behavior, recorded evidence, and identified unresolved choices. Bridge selection and the external server language remained open; the first local command could proceed without settling them.

A participant can assess the research by asking whether each recommendation answers the BIM question, shows its source and version, explains what it would add to the setup, and distinguishes inspected code from a tested solution. License labels are recorded evidence to review, not a blanket conclusion that every bundled component can be redistributed.

The later [MCP comparison story](05-research-and-choose-the-mcp-bridge.md) shows another research habit: explicitly ask the agent to check current versions and compatibility when returning to an older candidate. A saved source version preserves what we learned; a fresh version check establishes what is available now.

## Reuse the engagement pattern

This is a **suggested prompt**, not a historical quote:

> Read our project context. We need [BIM capability] for [people and task]. Before selecting tools, research existing solutions and collect the required SDK, API, library, sample, and dependency resources. Inspect relevant source at recorded versions and retain license evidence. Explain each resource's role, what has actually been tested, and what remains uncertain. Recommend the smallest useful next experiment and record the research in this repo.

For practice, ask your agent to explain one recorded candidate's value and one unanswered question. Have it point to the supporting source and propose a small check that would resolve the uncertainty. Use the [technical guide](../how-to/research-existing-solutions.md) when carrying out that inspection.
