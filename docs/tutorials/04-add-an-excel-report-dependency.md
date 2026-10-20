# How we directed the agent: adding Excel output

**For seminar participants.** Learn how to ask for a useful change, give the agent room to solve technical problems, and judge its evidence. You can follow this story without writing code or installing Revit. The [agent implementation guide](../how-to/add-a-plugin-dependency.md) contains the commands, source map, dependencies, and verification procedure.

Our starting point was a working, read-only Revit check for missing wall and ceiling type information. We wanted a report BIM staff could review in Excel. The completed change is recorded in commit `4d40788`; its [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) contains the results and screenshots.

## Start with the work people need to do

The author's actual opening question for this increment was:

> What additional libraries or add-ons would we need to produce an excel sheet readable output?

This asked the agent to identify what the new capability would require before implementation. A library is reusable code written by others; adopting one also brings its dependencies and license terms. Participants do not need to arrive knowing a library name. Ask the agent to investigate suitable options and explain the recommendation in terms of your intended use.

The conversation then settled on ClosedXML and four worksheets: Summary, Findings, Affected elements, and Rules and coverage. That supplied enough shared context for the next instruction.

## Give a bounded implementation instruction

The author's actual implementation prompt was:

> Ok, let's add the Excel output using ClosedXML with those four worksheets. Review the dependencies and licenses, verify the output against our sample results, and record the process in bim-agent-lab. This will demonstrate how to add our first plugin dependency. It will teach this.

The important parts of that direction were:

| What the author specified | What it gave the agent to do | What a participant can examine |
|---|---|---|
| Excel output with the agreed sheets | Produce a defined deliverable from the existing check | Open the workbook and identify the purpose of each sheet |
| ClosedXML, dependencies, and licenses | Inspect the selected library and the other code it brings with it | A source-backed [research record](../research/closedxml-dependency.md), exact versions, and retained notices |
| Verify against our sample results | Preserve known BIM answers while adding a new output | Findings and affected-element identities agree with the independent sample evidence |
| Record the process in the teaching repo | Leave reusable knowledge alongside the working change | The lesson, technical guide, evidence, and Git checkpoint |

Earlier direction already established research first, inspecting cloned repositories, using Git to record the build, working on sample copies, and keeping the checker read-only. The short prompt relied on that accumulated context. In a fresh conversation, point the agent to the repository instructions and current evidence so those requirements are available again.

## Delegate the implementation; stay responsible for the meaning

The author chose the BIM purpose, requested the output, and set the evidence standard. The agent inspected source code, selected exact package versions, wrote the exporter and tests, checked deployment, and documented the result. These technical choices did not require the author to prescribe every file or command.

The author still owns what counts as useful information, which requirements apply to the model, and whether the result supports the intended work. A report showing no missing text does not establish that an assembly is correct. Keep that distinction visible when reviewing the agent's result.

Git made the work inspectable: the agent began from a known baseline, worked on a branch, and committed the reviewed change. For a new task, a useful **suggested prompt** is:

> Inspect the repository and current changes first. Preserve existing work, use an appropriate branch, and record a reviewed local checkpoint. Show me what changed and the evidence behind it. Publishing will be a separate instruction.

## Use a failure to test the evidence standard

The first automated Excel tests passed, but the first export inside Revit failed. That exposed a difference between the test environment and the real application. The agent investigated, changed how the exporter ran, and repeated the relevant checks. The final design uses a small separate program to create the workbook reliably alongside Revit's other add-ins.

The author did not have to diagnose library loading. Requiring verification in Revit made the failure visible and gave the agent a reason to continue until the complete workflow worked. Code review also found missing details for unexamined elements and an installation issue; both were corrected.

When the evidence is incomplete, a useful **suggested follow-up**, not a historical quote, is:

> Which checks have actually passed, and which still need the real application? Investigate the failure, preserve the working outputs and models, and show evidence that the fix works in Revit.

Revit's unsigned-add-in prompt needed a human action under the automation rules used in this session. The author allowed the add-in and replied "loaded." The agent continued documentation while waiting, then resumed the application checks. This was a specific handoff, not a requirement to approve every technical step.

## Decide what the result proves

The three prepared model copies gave us known answers:

| Sample | Missing type values | Distinct affected elements |
|---|---:|---:|
| Complete information | 0 | 0 |
| Wall defects | 2 | 6 |
| Wall defects plus ceiling negative control | 3 | 8 |

The final Revit exports matched those answers and the independent native schedule evidence. All four sheets were inspected in Excel. Model files were unchanged. The two numbers matter: one missing value on a wall type can affect several wall instances. A plausible total alone would not prove that the right elements were identified.

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) distinguishes automated tests, actual Revit runs, independent comparisons, and visual review. These establish the stated sample behavior on the tested Revit configuration; additional capabilities and versions need their own evidence.

## Reuse the engagement pattern

A **suggested prompt for another capability** is:

> Read this repo's instructions and current status. We need [output or capability] so [people] can [work task]. Research existing solutions and required resources, including source code, dependencies, and licenses. Propose a bounded change and use [independent examples] to define success. Implement and verify it in the target application. Record how I directed the work for participants, and the technical procedure for future agents, then create a reviewed Git checkpoint.

Supply the business purpose and known examples first. Let the agent investigate how to implement the change. Ask for evidence you can assess, and preserve the reasoning so the next conversation can continue from it.

For practice, ask your agent to explain how one additional report column would help a BIM reviewer, where its value would come from, and how you would check it. Decide whether it is useful before requesting implementation. The agent can then follow the [technical guide](../how-to/add-a-plugin-dependency.md).
