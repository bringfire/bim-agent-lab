# How we directed the agent: proving the Revit setup

**For seminar participants.** Learn to turn a prerequisite question into a small, observable proof that an agent can build a working Revit command. The [agent setup guide](../how-to/build-read-only-revit-command.md) contains the environment checks, commands, source map, installation, and host verification procedure.

The [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) were recorded in `88ac2df`; the working probe and [acceptance evidence](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) followed in `f025fce`. Reading this story requires no development installation. Its results describe the author's tested Revit 2024 configuration.

## Ask what a new user actually needs

The October 27 move to the summit machine repeated this lesson. The author asked to finish the repository, including Revit 2026 compatibility. Inspection found a missing compiler SDK and a specific installed release, **2026.5**, whose runtime files declare .NET 10. Earlier verification on another machine did not prove this setup. The [dated intake](../../projects/revit-readiness/docs/verification/2026-10-27-summit-workstation-intake.md) records what was found and what remains untested; the [agent follow-through](../how-to/build-read-only-revit-command.md#october-27-workstation-intake) identifies the source and installation boundaries. The next practical proof is a small command in the intended Revit build, followed by the full readiness workflow.

The author then emphasized that this laptop would travel and must carry the build chain and dependencies. The agent supplied the missing SDK, selected local tools through a launcher, cached packages and tested restores with remote feeds removed. That test found a stale diagnostic lockfile before departure. The [travel record](../../projects/revit-readiness/docs/verification/2026-10-27-travel-toolchain.md) distinguishes local build readiness from live Revit proof, account/connectivity checks and a rehearsed demonstration. Having a sample source on disk also differs from having the five prepared cases and a reset set.

The author then identified the participant-facing consequence: people need the installation list before attempting the exercise. They had expected the needed SDK to come with Revit. The agent explained runtime versus compiler SDK and made the existing exact SDK requirement visible in an [advance checklist](../reference/participant-install-checklist.md), linked from the entry points. This turns a presenter-machine surprise into a pre-session check without claiming that the unverified 2026 workflow is ready.

The author's follow-up caught another distinction: Revit 2024 itself does not require SDK 10 merely because this lab does. A separate compilation of the unchanged 2024 probe with SDK 9 succeeded; the lab's external .NET 10 MCP server and project pin explain its newer SDK requirement. The [version matrix](../reference/participant-install-checklist.md#revits-target-versus-this-repositorys-compiler-choice) also separates earlier Revit 2026 from 2026.5. Ask an agent to distinguish the host's framework, the compiler it selects and the dependencies of the complete workflow.

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

## Keep an existing Revit version working during a port

On October 27, the author asked for working plugins in Revit 2026.5 on the summit laptop while retaining Revit 2024. A useful constraint was explicit: keep the known route available while proving the new one. The agent selected separate build outputs and installation folders, kept the 2024 default, tested equivalent report data on both runtimes, and prepared recovery before installing the modern candidate.

A successful build was only the first result. Revit then asked the author whether to load the unsigned local add-in. That trust decision belongs to the person using the laptop. Actual command execution, Excel output and agent calls still need observation after startup; preserving an installation folder does not establish that a plugin has run. Follow the [current verification record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md) for the latest observed scope.

The laptop follow-up exposed an instruction error: “use Add-Ins from Home” omitted a navigation step. After the author challenged it, the agent inspected the real screen, used the top-left Home toggle to expose the empty ribbon workspace, and opened External Tools there. The preparation command was present without a project. The instruction was corrected without changing plugin code. A manifest or successful startup is not evidence that a participant can follow a particular UI sequence.

The corrected route then produced the three 2026.5.1 fixtures. Actual Revit commands, native schedule comparisons, Excel XML checks and SDK calls agreed on 0/0, 2/6 and 3/8 findings/affected instances. An old context was rejected after a model switch, and Stop made the bridge unavailable. This established a useful working host example while preserving a separate checklist for actual desktop-agent use and fresh 2024 checks. The [current record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md) distinguishes those claims.

Before packing the laptop, the same three manual/Excel and SDK cases also passed in a fresh Revit 2024 session. Both installations retained their recorded file hashes and both source copies were unchanged. The author chose to rehearse the desktop-agent interaction the next day, so the checkpoint records working host tools separately from that pending rehearsal.

After arriving in Pleasanton, the author resumed final preparation. The next gap was the connection to the desktop agents. Revit could already run the commands and answer an SDK test, but this laptop's Codex and Claude clients had no BIM Agent Lab entry. Claude's own Edit config button also revealed a different settings location from the usual Windows example. The agent used that observed location, saved private recovery backups and registered the installed server without changing other settings. The person then needed to reload the clients before an actual desktop request could be tested.

A **suggested rehearsal prompt** is:

> Use the connected BIM Agent Lab tools to get fresh context and check required type information in the active Revit model. Name the model and checklist, state the examined scope and completeness, and explain any findings. Distinguish the 16 checklist instances from the rest of the building. Do not change the model. If the tool is unavailable, show the actual failure and identify the missing connection step.

That prompt tests what participants will see, including the agent's explanation of the tool result. It does not ask the agent to add another tool before the working path has been rehearsed. Follow the [verification record](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#october-28-desktop-connection-preparation) for the observed result rather than assuming registration alone completes the demonstration.

The author then reported “mcp restarted.” Codex acquired fresh context and checked the complete sample through its own newly available tools. It reported all 16 checklist instances across six types examined, no missing required values, complete coverage and no modification. Its explanation also named the 1,196 other host instances and six links outside that scope. The recorded exchange matched the SDK report except for the timestamp. This proves the first actual desktop interaction; a good explanation still has to bound what “pass” means. Claude discovered the tools and the author was asked to run the same bounded question in Chat before changing models.

Claude's answer instead reported BridgeUnavailable and listed possible causes. The agent checked the actual requests and processes before acting on that list: both clients selected the one open Revit instance. A physical-file check then exposed the difference. Revit had been started through Codex, and its bridge file was inside Codex's private AppData cache. Codex could see its own view; that did not establish that Claude could see it. The earlier server/config checks had missed the session file itself.

The next controlled step was to launch Revit from Windows and repeat the two-client check. The agent restored a temporary diagnostic configuration, retained the failed attempts and asked the author to perform the normal relaunch after the minimized window could not be selected. No plugin was rebuilt or permission weakened. At this checkpoint the cause was identified, while the launch correction still needed verification. This is a useful follow-up question for a demonstration: “Which application can actually see the file, and what evidence establishes that?”

The next observed Revit process had Windows Explorer as its parent. After reopening the same sample and starting the bridge, its session file resolved to the normal shared location and an SDK query returned the same verified answer. The agent refreshed both desktop entries for the new process while preserving an unrelated client setting changed during restart. The remaining question is an actual desktop retry after those clients reload; a corrected file location alone is not the final participant-facing result.
