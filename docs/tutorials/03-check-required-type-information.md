# How we directed the agent: the first readiness check

**For seminar participants.** Learn how a short BIM request becomes an explicit rule, controlled examples, and evidence you can judge. The [agent readiness guide](../how-to/build-required-information-check.md) contains the implementation, sample preparation, commands, and independent verification procedure.

We started from the working probe in `f025fce`. The first readiness check and its [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) were recorded in `c7c2ad9`. That milestone produced JSON and text reports; the [next lesson](04-add-an-excel-report-dependency.md) covers the later Excel output.

## Carry earlier context into a bounded request

The author's actual prompt was:

> Ok, let's build the first readiness check for missing required wall and ceiling type information. Use a copy of a Revit sample project, verify the results independently, and record the process in bim-agent-lab.

This did not start a new project from scratch. Earlier direction had already established model readiness as the lead workflow, Revit 2024, research before implementation, Git checkpoints, and a reusable teaching repository. The prompt selected one useful increment and specified how to demonstrate it.

| Direction | How it shaped the work |
|---|---|
| First check for missing required type information | Define the applicable requirements before deciding what counts as a defect |
| Use a sample copy | Prepare known cases while preserving the original supplied model |
| Verify independently | Compare the answer with evidence outside the new checker |
| Record the process | Retain source, expected answers, results, and lessons together |

The author supplied the professional purpose and these constraints. The agent developed the detailed teaching checklist, scoped cases, implementation, and verification procedure. The exact fields and sample values were engineering choices for this example, not a record of the author issuing an industry-wide standard.

## Make the meaning of “required” explicit

A blank fire-rating value does not by itself mean a wall is wrong. The teaching checklist requires a code on all six sample types, and rating and assembly-reference text on Walls A and B. The remaining wall and ceiling types explicitly allow those latter fields to be blank.

This makes both failures and valid exceptions testable. The check covers 16 identified teaching instances on a stated level and phase; it reports other host elements and links as outside its scope. Participants should ask whether the requirements and scope match their own work before applying a similar rule to a live project.

For a new rule, a useful **suggested follow-up** is:

> What makes this field required, where does that requirement come from, and which cases are valid exceptions? Show the proposed scope and expected answers before implementing the check.

That question asks for a domain decision a BIM professional can assess. It does not require the person to select API parameter identifiers or design the software classes.

## Give the agent known examples and require a separate check

The agent used a copy of Autodesk's Snowdon Towers Architectural sample. A separate preparation command duplicated teaching types, added 12 walls and four ceilings, and saved three scenarios. Preparing examples deliberately edited copies; running the readiness command was read-only. The installed original remained unchanged.

The expected answers were written independently of the evaluator. Native Revit wall and ceiling schedules displayed the relevant case labels and type values. A comparison then checked those exported schedule rows and the checker report against the answer sheet, without using the checker's own evaluation code.

| Prepared scenario | Missing required type values | Distinct affected instances |
|---|---:|---:|
| Complete information | 0 | 0 |
| Wall defects | 2 | 6 |
| Wall defects plus a ceiling gap | 3 | 8 |

A missing value on one type can affect several instances. Counting findings and counting affected elements answer different questions; several missing fields on the same type must not count its instances twice. The agent verified identities and field values as well as totals.

## Expect investigation and review, not a flawless first attempt

The agent tested the rule logic separately, reviewed the source, built the commands, and then exercised them in Revit. Review found two issues in the sample-preparation code involving unchanged values and whether edits had actually committed. The agent corrected them before running that preparation and added further tests for multi-type counts and identity handling.

Revit's unsigned-add-in prompt required a human action under the tools used in that session. The author allowed the commands and reported choosing Always Load. The agent resumed the application checks afterward. This was a specific handoff, not the author having to approve each implementation choice.

All three Revit scenarios matched the [answer sheet](../../projects/revit-readiness/samples/expected-results/required-information.md) and independent schedules. Eighteen rule tests passed. Repeated reads agreed apart from timestamps, modified flags stayed false, and model-file hashes matched after closing. The [dated evidence](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) records the precise configuration and boundaries.

## Decide whether the evidence answers the BIM question

The result establishes text completeness against this declared checklist. It does not establish that a fire rating is valid, that an assembly performs as intended, or that the entire building is ready for construction. Unreadable or unexamined cases must stay visible instead of being counted as passing. The person owns whether the checklist is meaningful for the intended work.

A **suggested prompt for another readiness rule** is:

> Read the existing contract and verification evidence. We need to check [condition] for [defined scope], using [requirement source]. Make the requirements and valid exceptions explicit. Prepare sample copies with independently known passing, failing, and excluded cases. Implement the check, verify values and affected identities against native BIM evidence, and show what the result does and does not prove. Record both learning layers and a reviewed Git checkpoint.

For practice, ask your agent to propose one valid exception to a future rule and explain how it would prove that the exception is handled correctly. Review the domain meaning before implementing it. The [technical guide](../how-to/build-required-information-check.md) gives the next agent the reproducible procedure.
