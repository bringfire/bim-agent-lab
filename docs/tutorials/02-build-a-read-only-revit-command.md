# How we directed the agent: proving the Revit setup

**For seminar participants.** Learn to turn a prerequisite question into a small, observable proof that an agent can build a working Revit command. The [agent setup guide](../how-to/build-read-only-revit-command.md) contains the environment checks, commands, source map, installation, and host verification procedure.

The [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) were recorded in `88ac2df`; the working probe and [acceptance evidence](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) followed in `f025fce`. Reading this story requires no development installation. Its results describe the author's tested Revit 2024 configuration.

## Ask what a new user actually needs

The author's actual starting question was:

> first things..What are the minimal requirements for developing a plugin for Revit?  This includes all software, libraries or other dependencies that will allow a new user to build a plugin with their agent.

This brought the discussion back to the participant's machine. Research had found many possible tools, but the first command needed only a workable build setup and access to Revit's programming interface. The agent separated required inputs from optional investigation tools and future MCP dependencies.

Participants need to understand three roles: tools that compile the source, reference libraries that describe the interfaces it can use, and Revit as the application that runs the command. The agent can identify exact compatible versions and paths. Installing a newer compiler does not change the version of Revit the plugin must work inside.

## Request proof in the real application

The next **actual prompt** was:

> Ok, let's check this machine against the prerequisites, install anything missing, and build a minimal read-only Revit command to prove the setup works. Record the steps and lessons in the teaching repo.

Earlier context supplied Revit 2024, Git, the readiness goal, and the teaching repository. This prompt added a bounded outcome: inspect the actual machine, fill genuine gaps, and make a small read-only command work. It also required a record that someone else could follow.

The author authorized missing installations; inspection found that every required input was already present. **No prerequisite software needed installation or upgrading.** The agent built and installed the new command itself. That distinction keeps the lesson useful for a participant whose machine may have different gaps.

## Delegate mechanics while keeping the purpose visible

| What the author directed | What the agent handled | Evidence a participant can assess |
|---|---|---|
| Check this machine against the prerequisites | Inspect software, matching API libraries, and build references | An inventory stating what was present and what was missing |
| Build a minimal read-only command | Write the source, build it, register it for the local user | The command appears and runs in the intended Revit version |
| Prove the setup works | Define small known-count cases and compare actual results | Revit's own selection count agrees with the command |
| Record steps and lessons | Save source, procedure, screenshots, and verification in Git | A reproducible guide linked to the working checkpoint |

The author did not specify the compiler flags, project files, or installer script. The agent chose those implementation details. The human-facing requirement remained understandable: read the active model and return an answer we can independently check.

## Choose an answer that is easy to verify

The agent defined an empty-project case and a four-wall case before writing the command. It prepared a simple model with four deliberately drawn wall segments and checked Revit's own Properties palette, which showed **Walls (4)**.

| Case | Expected answer | Recorded result |
|---|---|---|
| Empty project | 0 walls, 0 ceilings | Matched |
| Saved four-wall model | 4 walls, 0 ceilings | Matched on two runs |
| Read-only behavior | No change during the query | Revit's modified flag stayed false and saved file hashes matched |

A file hash is a fingerprint of the saved bytes. It helps show that the file stayed unchanged, but cannot alone detect an unsaved edit in memory. The agent combined it with Revit's modified flag and the command's enforced read-only execution. The [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) includes the evidence and the untested cases.

One check initially failed because Revit held the model file open while the agent tried to hash it. The agent investigated that file-access problem and confirmed the same hash after closing the project. A failed verification command was recorded and resolved; it was not treated as proof that the plugin had modified the model.

## Judge what the milestone proves

Successful compilation showed that source and references agreed. Running in Revit established that the command loaded and returned the known result. Independent counts and read-only evidence addressed behavior. Each answered a different question.

The result proved the local development workflow and a narrow count operation. It did not establish model readiness, measured quantities, an MCP server, or support for other Revit versions. Even positive ceiling counts were not part of this first probe's host acceptance.

A **suggested prompt for your own setup** is:

> Read the repo instructions and inspect my workstation against the target Revit prerequisites. Identify missing inputs and obtain what is needed within this task. Build a minimal read-only command and verify it in Revit against a small example with an independently known answer. Show what passed and what remains untested. Record the participant story, technical procedure, and reviewed Git checkpoint.

For practice, ask your agent to explain the existing environment report and name the next independent case that would strengthen the probe's evidence. Have it separate inspection from installation and build success from host success. Use the [technical guide](../how-to/build-read-only-revit-command.md) when executing the work.
