# BIM Agent Lab build journal

> Entries before repository creation describe the original authoring folder. Paths and proposals in those entries are historical; see the [migration record](repository-setup.md) for the current layout.

This journal records how the author and coding agent develop the workshop example, beginning on 2026-10-20. It preserves prompts, decisions, research evidence, and practical lessons so the process can be taught alongside the resulting tool.

The primary outcome is a model-readiness workflow for wall and ceiling BIM professionals, supported by inventory and quantities. The workshop takeaway includes the tool's source and the reasoning needed to extend it.

## 2026-10-20 Establish the problem and context

### Author prompt

> Ok, take a look at the context of this folder and let's start figuring out what plugin we will create for Revit as an example and how we proceed to creating an MCP server for Revit that is geared toward BIM professionals.

### What the prompt accomplishes

It directs the agent to existing project context, identifies Revit and MCP as the intended environment, and names the professional audience. Reading the folder establishes constraints that would otherwise have to be repeated: wall and ceiling contractors, two one-hour sessions, a reusable takeaway, and required Revit 2024 support.

### Agent response and evidence

The agent compared model inventory/readiness, revision comparison, and parameter cleanup. It proposed a read-only wall and ceiling inspector with shared operations behind a Revit command and an MCP server. Local inspection found Revit 2024 and its API assemblies. Official documentation and community repositories supported further investigation of a standalone bridge.

The C#/Python stack, exact tool list, and broader version range were proposals, not settled implementation decisions.

### Lesson to show

Give the agent the audience, intended outcome, constraints, and existing materials before requesting code. Ask it to state what it learned and distinguish confirmed requirements from its suggestions.

## 2026-10-20 Choose the workflow and require research

### Author prompt

> Ok, let's lead with model readiness, with inventory and quantities supporting it. Let's define the three demo questions and what we'd need in a sample Revit model to demonstrate them, we can use the sample Revit projects that ship with Revit and if necessary we can modify them to suit our needs for demonstration. As we do this, we need to be taking account of how I'm prompting you, what best practices are and what the overall steps are that we want to use to achieve our goals. The first thing I want to impress on people is that research for work that has already been done by others and collecting the required resources are necessary. SDK's, API's, libraries that are required, dependencies, and GitHub repos that might already exist that can be used as precedents, or as starting points if they have the proper license structure.

### Confirmed decisions

- Lead with model readiness; inventory and quantities are supporting evidence.
- Define three demo questions and the sample conditions needed to answer them.
- Use shipped Revit sample projects where practical, modifying working copies as needed.
- Record prompting, best practices, and the overall development sequence as we work.
- Make prior-work research, resource gathering, dependency review, and license-aware reuse a central early lesson.

### What the prompt adds

The author makes the main workflow explicit and asks for observable demonstrations before implementation. The prompt also defines a second deliverable: an account of the collaboration process. The licensing requirement gives the agent a concrete criterion for evaluating reuse.

### Research that changed the discussion

- Snowdon Towers architectural and structural samples are installed locally; the specific demonstration elements still need inspection inside Revit.
- The installed Revit 2024 API XML documents the layer, unit, element-identity, area, and constraint methods needed for the proposed readers.
- The existing MIT MCP bridge has a Revit 2024/net48 configuration, making it worth evaluating before writing a replacement.
- The current Nice3point application template includes an R24 configuration.
- License inspection found MIT candidates, GPL text in pyRevit, and a licensing transition in the C# MCP SDK. Badge summaries alone do not capture all of this.
- Dependency manifests expose practical constraints: a native SQLite dependency in one bridge, floating Revit package ranges, and an old Python example whose unbounded MCP requirement could resolve to a newer major API.
- The RevitLookup release includes a 2024 installer even though the release's title is 2027.0.4. Inspect the actual asset.

Source URLs, inspected commits, and qualifications are preserved in [Research and resources](../research/revit-resources.md). The proposed questions and expected results are in [Model readiness demos](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md).

### Lessons to show

1. **Define success in terms of questions and expected answers.** It creates a useful target for research and later tests.
2. **Research before selecting the stack.** A plausible first architecture is a hypothesis; existing work can change it.
3. **Inspect the primary evidence.** Read licenses, dependency manifests, versioned API docs, and release assets.
4. **Create both failing and passing examples.** A tool must find known defects and avoid flagging valid exceptions.
5. **Keep provenance with the result.** This applies to borrowed code and to facts extracted from a BIM model.
6. **Record decisions as they happen.** Preserve why a choice changed instead of reconstructing a tidy story afterward.

The agent also encountered a shell-formatting error while extracting XML documentation and retried with a corrected read-only command. The retry returned the requested API members. This is a small example of treating tool output as evidence: a failed command is not a successful check.

## 2026-10-20 Bring Git into the opening lesson

### Author prompt

> One thing that participants need to know pretty much from the start is what it means to direct an agent in regards to working with git. I am going to ask you to clone the repos that you found, so that you can inspect the code for the researched resources. Participants will need to know how to do this...meaning "that they should do this"

### What this changes

Git becomes part of the opening research workflow. Participants should recognize when to ask an agent to obtain local source, inspect a particular revision, create a branch, review a diff, and save a checkpoint. The emphasis is knowing what to direct the agent to do.

### Actions and evidence

The agent cloned the seven candidates under `references/`, checked out the previously recorded commits, and verified each origin, HEAD, and clean working tree. Fourteen license and manifest files matched the saved research evidence by SHA-256 hash. The clones use `--filter=blob:none`; the selected working trees are checked out while other file contents can be fetched on demand.

An initial source pass located the MCP query, command, and Revit handler, the template's framework-selection rules, pyRevit's request queue, and small MCP SDK examples. It also identified ten uninitialized submodules across RevitLookup and pyRevit. Cloning and reading source have been completed; builds, package restoration, and runtime checks remain separate work.

The agent added a `.gitignore` for local reference checkouts and session scratch. The surrounding workshop folder is not yet a Git repository. Creating local history for our own material is a distinct setup step from cloning other repositories.

### Lesson to show

Ask an agent to trace a real operation through the source and cite the relevant files. One discovery is the existing current-view tool's default limit of 100 elements: a reusable API call still needs a completeness check before it supports a readiness report.

The participant-facing prompt and Git vocabulary are in [Directing an agent with Git](../tutorials/01-directing-an-agent-with-git.md). The [reference code map](../research/reference-code-map.md) gives the source paths for this demonstration.

## 2026-10-20 Design the repository as a lasting teaching resource

### Author prompt

> We will be sharing the repo with the participants of the seminar, so we want to make sure we have a "teaching repo" that after the seminar the users can point their agents to and learn and refresh on how to do this. The seminar is too short to do a full walkthrough, so we will be showing how we built the plugin, they will have the plugin as a takeaway to build on and the repo will have all of the information needed to guide them in the process of extending the plugin and "teaching" how we built this plugin so that they can build their own. To that end, we should probably start a fresh repo with this in mind. What do you think the name and structure of the repo should be for maximum utility and lasting value?

### Confirmed purpose

The shared repository is a continuing teaching resource. It must support using the
takeaway plugin, learning how it was built, extending it with a coding agent, and
applying the process to another tool. The seminar provides orientation and selected
demonstrations; the repository carries the full learning path.

### Initial agent proposal (name superseded below)

- Start one fresh repository called `revit-agent-lab`, with the display title
  "Revit Agent Lab" and a description about building and extending Revit tools with
  AI coding agents. Keep the event identity under `workshops/swacca-2026/`.
- Keep the working add-in, MCP server, shared readiness logic, tests, and teaching
  materials versioned together. The component folders describe responsibilities;
  they do not settle the outstanding server-language or bridge-reuse decision.
- Make the README route readers to using the plugin, learning the build process,
  or extending it. Use a concise `AGENTS.md` for project context, navigation,
  verified commands, and development conventions. Agent-specific entry files can
  point to the shared instructions.
- Organize documentation into tutorials, task guides, technical reference,
  explanations, source-backed research, and the actual build story. Keep a
  reusable prompt with its lesson and explain the intent and evidence to check.
- Give each learning chapter a starting point, objective, prompt, explanation,
  observed outcome, verification, and a small extension exercise. Label actual
  prompts separately from edited teaching examples. Link milestones to real
  commits as the work occurs; do not manufacture an earlier Git history.
- Include model preparation recipes, independent expected results, and explicit
  tested versions. Store reproducible reference-clone instructions and a pinned
  source manifest; keep the reference repositories outside tracked content.
- Publish a tagged seminar release with matching source, instructions, and
  installable artifacts after verification. Continue development on the main
  branch. Keep Revit-dependent acceptance checks distinct from checks that can
  run without Revit.

The current folder remains the authoring workspace. Migration would select and
adapt its useful research, demo brief, Git lesson, and journal, replacing
machine-specific paths and adding commit-pinned upstream links. Organizer drafts
and local session state do not belong in the participant learning path.

### Research and evidence

The agent reviewed [Diataxis](https://diataxis.fr/) for documentation organized by
learning and task needs, the [AGENTS.md format](https://agents.md/) for a shared
agent entry point, and [GitHub releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases)
for packaging a version tied to a Git tag. These inform the proposal; they do not
establish that our future repository or plugin has been built or tested.

### Lesson to show

Describe who must be able to use the result after the conversation ends. A
deliverable intended to teach needs navigation, context, reproducible examples,
and evidence alongside its implementation. A useful acceptance exercise is to
start with a clean clone and ask a fresh agent to locate the prerequisites,
explain a readiness rule, guide a small extension, and verify the result.

No new repository, scaffold, commit, or remote was created during this design
discussion.

## 2026-10-20 Choose BIM Agent Lab and allow growth beyond Revit

### Author prompt

> I think bim-agent-lab keeps it more open and allows it to grow beyond revit. We don't want to foreclose the possibility that this will continue to evolve.

### Confirmed decision

The repository name is **`bim-agent-lab`**, displayed as **BIM Agent Lab**. Its
teaching purpose extends to building BIM tools with coding agents. Revit model
readiness remains the first worked example and the immediate implementation scope.

### Structural implication proposed by the agent

Keep transferable lessons, research methods, and templates at repository level.
Place the first example under `projects/revit-readiness/`, with its own source,
tests, sample recipes, setup instructions, and agent guidance. Future worked
examples can have sibling project folders. Create shared implementation libraries
only when actual projects demonstrate a common need; the broader name does not
require building a multi-platform framework now.

Proposed description: "Learn to build and extend BIM tools with AI coding
agents—from research and Git to working plugins, automation, and MCP servers."

### Lesson to show

Name the teaching resource for the durable purpose and organize concrete examples
so they can grow independently. Preserve a focused first deliverable while leaving
space for future applications. This records the name decision and layout proposal;
no new repository or remote has been created yet.

## 2026-10-20 Create the local teaching repository

### Author prompt

> Ok, let's create bim-agent-lab with that structure, bring over the research and teaching materials we've developed, and initialize Git so we can record the build process from here. Do this in the D: drive

### Action and lesson

Created `D:\bim-agent-lab` with the accepted project layout and a local `main`
branch. Imported the earlier research and teaching materials, retained source
provenance, and supplied participant navigation plus shared and Revit-specific
agent instructions. The [setup record](repository-setup.md) describes the migration
and verification; the [import manifest](import-manifest.json) maps source files to
the teaching repository.

This is the beginning of the repository's Git history. Earlier research stays in
the journal and dated evidence snapshots. The original authoring folder and its
reference clones remain available. No plugin implementation or remote publication
is implied by the new structure.

## 2026-10-20 Develop the selected Option B seminar structure

### Author prompt

> Ok, let's update Option B around our current plan, emphasizing research, Git, how we built the plugin, and using bim-agent-lab afterward. Keep it to two one-hour sessions. This will evolve as we proceed, but it is the general structure.

### Decision and working allocation

The author confirmed a variation of Option B, “The story of the build.” The agent
created a [current HTML agenda](../../workshops/swacca-2026/agenda.html) while
preserving the original organizer brief and alternative outlines in the archive.

Session 1 allocates 5 minutes to the room and problem, 5 to a result preview,
8 to Git, 15 to research and resources, 10 to requirements and sample design,
12 to the first working build step, and 5 to questions. Session 2 allocates
5 minutes to recap, 18 to the complete plugin/MCP operation, 12 to verification
and corrections, 15 to using BIM Agent Lab afterward, 5 to participants' own
ideas, and 5 to questions. Each session totals 60 minutes.

These timings are a working proposal within the author's selected structure.
Examples will come from actual research, code changes, and verification as the
plugin develops. The agenda distinguishes planned seminar demonstrations from
the current research-only implementation status. A separate scripting walkthrough
is optional; the core story follows the standalone add-in and MCP workflow.

### Teaching lesson

Use the live sessions to establish a method and show its evidence. Make the
repository handoff an explicit lesson: participants should know how to direct a
fresh agent to the project context, relevant source, expected answers, and the
next small extension. Keep the timing source and evolving build record linked.

### Verification

Checked that each session's elapsed blocks are consecutive and total 60 minutes,
and that the visual timelines match their tables. All 15 agenda links and 128
relative Markdown file links resolve. The original HTML archive matches the
previous commit byte-for-byte. Viewed the page header and both session tables in
the browser, correcting the table-column spacing before the final check. These
are documentation checks; they do not validate the planned plugin demonstrations.

## 2026-10-20 Establish the participant development requirements

### Author prompt

> first things..What are the minimal requirements for developing a plugin for Revit?  This includes all software, libraries or other dependencies that will allow a new user to build a plugin with their agent.

### Action and teaching lesson

Added the [development prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md), grounded in Autodesk and Microsoft documentation and linked from the participant learning path. The guide separates the minimum C# add-in setup from research/debugging resources and the later MCP layer.

The central distinction is between tools that compile code, references that define the target APIs, and Revit as the runtime host. A modern .NET SDK can build an SDK-style project targeting .NET Framework 4.8; its own version is not the add-in's runtime target. The required framework references can come from the Developer Pack or Microsoft's build-time NuGet package. A full Visual Studio editor and third-party plugin frameworks are optional for the first command.

The author's request exposes a prerequisite that experienced developers may overlook: participants need to know what their agent can actually do on their machine. A reusable inspection prompt asks for existing software and missing requirements before installation. The next proof is a minimal read-only command that builds, loads in the target Revit release, and returns a result checked independently.

### Evidence and remaining work

Rechecked the installed Revit/API file versions, .NET SDKs, Git, and the presence of .NET Framework 4.8 reference files. These are inventory observations, not a successful compilation or host test. Exact lab build-tool pins and the MCP dependency selection remain pending the first working build. No software was installed and no Revit add-in was created or run for this documentation step.

## 2026-10-20 Build and verify the first Revit command

### Author prompt

> Ok, let's check this machine against the prerequisites, install anything missing, and build a minimal read-only Revit command to prove the setup works. Record the steps and lessons in the teaching repo.

### Decisions and actions

The agent inspected the clean teaching repository and created `feat/revit-read-only-probe` from `88ac2df`. The machine already had Revit 2024, matching API DLLs, .NET Framework 4.8 references, .NET SDK 10.0.204, and Git. No prerequisite software needed installation. Keeping the minimum setup small was a result of inspection, not an assumption that every participant has these tools.

A host acceptance procedure was written before the command. The first implementation uses an original C# `IExternalCommand` targeting x64/net48, local Revit API references with Copy Local disabled, and `TransactionMode.ReadOnly`. It reports the active project, Revit build, wall/ceiling category-instance counts, explicit scope, and the document's modified flag before and after. It introduces no NuGet packages or MCP dependency.

The agent added repeatable environment, build, and per-user install scripts, built successfully with zero warnings/errors, verified the installed DLL hash, and launched Revit. The Revit journal confirmed command registration. A new project from the installed Imperial Multi-discipline template returned zero walls and zero ceilings.

The agent then used Revit's rectangle wall tool to draw four basic wall segments. Revit's own Properties palette showed **Walls (4)**. After saving the fixture, two command invocations both returned **4 walls and 0 ceilings**, with the modified flag **False before / False after**. The saved file's SHA-256 remained identical before the reads, after the reads, and after closing without another save. With no project open, External Tools was disabled. The family-editor case was not exercised.

### Corrections and verification boundaries

Normal `Get-FileHash` initially could not read the model while Revit held it open. A read-only shared file stream allowed hashing; a normal hash after closing confirmed the same result. This was a file-access limitation, not a plugin failure. The model fixture was deliberately edited during preparation; the read-only command itself did not edit it.

The UI automation's accessibility tree sometimes retained stale dialog information while screenshots showed the current window. The agent used fresh screenshots to ground subsequent UI actions and retained actual result images. The teaching record does not equate a successful click, compile, or file inventory with a passing host test.

The [first-build lesson](../tutorials/02-build-a-read-only-revit-command.md), its [agent procedure](../how-to/build-read-only-revit-command.md), [fixture recipe](../../projects/revit-readiness/samples/model-recipes/read-only-probe.md), and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) preserve commands, versions, hashes, evidence, and limitations. Source and evidence are checkpointed together; working models, Autodesk binaries, and generated output stay outside Git.

### Teaching lesson

The author's prompt successfully carries forward earlier context while specifying the next observable outcome. Participants should direct their agent to inspect before installing, use Git deliberately, make one small operation work in its actual host, and compare the answer with independent BIM evidence. Research and resources remain inputs to the work; installing every tool discovered during research is unnecessary.

This milestone proves local API access and the build/install workflow. It does not complete model readiness, quantity review, or MCP. The next increment should establish one real readiness check and its sample answer before adding remote dispatch and more dependencies.

## 2026-10-20 First required-type-information check

### Author prompt

> Ok, let's build the first readiness check for missing required wall and ceiling type information. Use a copy of a Revit sample project, verify the results independently, and record the process in bim-agent-lab.

### Decisions and actions

The agent started `feat/required-type-information` from the clean, verified probe checkpoint `f025fce`. The request retained the earlier model-readiness goal while specifying a useful first rule, safe sample handling, independent verification, and a teaching record. Those requirements became a [concrete contract](../../projects/revit-readiness/docs/required-type-information.md) before implementation.

“Required” was made explicit: all six teaching types need a code in Type Mark; Walls A and B additionally need Fire Rating and a GUID-identified assembly-reference text field. Optional blanks remain valid. Scope is 16 identified cases on one level/phase, with host/linked exclusions reported. Nonempty text is a completeness answer, not proof of a valid rated assembly. Physical top conditions, measured quantities, and MCP remain separate work.

Research used installed Revit 2024 XML API documentation plus Autodesk's sample, shared-parameter, and parameter guides. It established the actual `FIRE_RATING` identifier, string-storage handling, GUID binding, schedule fields/exports, and transaction behavior. The existing net48 toolchain was enough; no new packages or prerequisites were installed. No precedent code was copied into this implementation.

The evaluator consumes plain observations and requirements, independently of Revit. Thirteen behavior tests first failed against a stub, then passed after implementation. A separate Revit reader collects scoped type values using stable identifiers; a `TransactionMode.ReadOnly` command writes local reports and displays the result. An explicitly installed fixture tool prepares model copies in transactions. Keeping those two operations separate makes their permissions and purposes visible to participants.

The installed Snowdon Towers Architectural sample was copied and hash-checked. The fixture command opened only that copy, duplicated six types, added 12 walls and four ceilings in an isolated area, and created itemized native schedules. It saved a complete baseline, a wall-defect copy, and a ceiling-negative copy. Its ACT/GWB labels are data-case labels; the copied physical assemblies were not redesigned.

### Review, host verification, and corrections

A separate code reviewer found two fixture failure paths: treating an unchanged `Parameter.Set(false)` as a failure, and ignoring non-committed transaction statuses before saving. The agent checked those findings against API documentation, fixed them before fixture execution, and added five multi-type/identity regressions. The final suite passed 18/18; readiness and fixture builds had zero warnings/errors. Follow-up review found both issues resolved.

The unsigned-add-in prompt required the user's action under the agent's computer-use policy. The user allowed both commands and reported choosing Always Load. The agent restarted Revit after installing the reviewed build, then prepared the samples and executed the read-only check.

All three host cases matched the independently authored answer sheet: **0 findings/0 affected instances**, **2/6**, and **3/8**. Each covered exactly 16 cases/six types on L1 - Block 43, New Construction (New), while reporting 1,196 other host wall/ceiling instances and six links outside scope. The native wall and ceiling schedules were opened and inspected. A standard-library Python comparator checked every exported row, expected field value/status, and affected case identity without importing the evaluator or fixture code; all three comparisons passed.

Two ceiling-negative reads were identical except for their timestamps. Every command reported modified False before/False after. Hashes taken before queries and after closing Revit matched for the three prepared models; the initial copy and installed original sample also remained unchanged. [Retained evidence](../../projects/revit-readiness/docs/verification/required-information-evidence/README.md) lets participants reproduce the text comparison without owning the RVT files.

### Teaching lesson

The useful agent workflow is observable: establish Git context, translate the BIM question into declared requirements, research actual API contracts, write expected answers, test a small deterministic core, build/install, review, execute in the host, compare against independent BIM evidence, and checkpoint the process. Unit tests, a clean build, a source review, and a native schedule comparison answer different questions; none substitutes for all the others.

The [readiness tutorial](../tutorials/03-check-required-type-information.md), its [agent procedure](../how-to/build-required-information-check.md), [sample recipe](../../projects/revit-readiness/samples/model-recipes/required-information.md), and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) preserve commands, requirements, observations, limits, and corrections. Participants can direct their agents through the same sequence when adding another check. An MCP bridge should expose this verified operation while preserving its scope, identity, and completeness evidence.

## 2026-10-20 First plugin dependency: ClosedXML Excel output

### Author prompt

> Ok, let's add the Excel output using ClosedXML with those four worksheets. Review the dependencies and licenses, verify the output against our sample results, and record the process in bim-agent-lab. This will demonstrate how to add our first plugin dependency. It will teach this.

The preceding discussion defined Summary, Findings, Affected elements, and Rules and coverage. The agent used that approved design, created `feat/excel-readiness-report` from clean `c7c2ad9`, cloned ClosedXML at a pinned tag/commit, and inspected source, package metadata, transitive dependencies, and embedded resources. The chosen package is ClosedXML 0.105.1. The resolved graph has twelve packages total; the font embedded in ClosedXML adds a separate OFL notice. Source/package/license evidence and notices are retained, while source clones and binaries remain ignored.

The first seven workbook tests failed against a stub, then passed. A separate XML reader compares workbook data with the already independently verified JSON/native schedules and literal expected answers. The workbook retains all eighteen type-field evaluations and explicitly identifies unexamined cases. A separate review caught a missing-identity gap, and a targeted red/green assertion now protects it.

The first Revit export failed despite passing console tests. Its dialog preserved the successful model check and JSON/text output and clearly reported the Excel error. Investigation found dependency binding assumptions hidden by the test executable's configuration. A private AppDomain attempt also failed its host-like test and was removed. The final approach is a short-lived local exporter process with its own configuration and dependency directory; Revit configuration remains untouched. A host-like harness with no spreadsheet references/redirects verifies the exact process launcher against both built and installed exporters. The installer now deploys the full runtime and notices, not just the add-in DLL.

Follow-up review found a shared-core load-order issue when the older optional fixture tool is used first. The installer now synchronizes the backward-compatible core in an existing recognized fixture installation without enabling the editor for new users. This is a concrete example of how adding a dependency can reveal deployment assumptions beyond the new library.

The [lesson](../tutorials/04-add-an-excel-report-dependency.md), [research/license review](../research/closedxml-dependency.md), and [acceptance record](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) distinguish code, offline checks, and actual host results. The user handled Revit's unsigned-add-in prompt and replied "loaded"; the agent continued documentation and independent checks during that handoff. All three final Revit exports passed with 0/0, 2/6, and 3/8 findings/affected instances. Fresh XLSX data matched native schedules and independent expected answers, all four sheets were inspected in Excel, and original/source/demo model hashes remained unchanged. Normal installation then synchronized the existing fixture core; the fixture-first load test passed. Final source/deployment review found no remaining substantive issues, including matching hashes for all 51 installed files.

### Teaching value

The prompt specifies purpose, output, research, verification, and documentation rather than asking only to install a package. The sequence is reproducible: inspect Git and source, inspect the resolved dependency graph and notices, pin versions, write failing tests, implement, package all runtime files, verify in the actual host, independently inspect output, review and record the checkpoint. The runtime failure is useful teaching evidence: successful unit tests did not establish that a dependency would load inside Revit.

## 2026-10-20 Capture the engagement story separately from implementation

### Author prompt

> good.  Now we need to capture that as learning for participants and also for agents that read the repo.  Participants are the highest level, "how did we engage the agent?"  agents get the lower level description.

The author clarified the teaching hierarchy after the Excel implementation checkpoint `4d40788`. The original lesson mixed the engagement story with technical instructions. The agent rewrote the [participant lesson](../tutorials/04-add-an-excel-report-dependency.md) around the actual questions and implementation prompt, prior context, human judgments, delegated work, the host failure, evidence to assess, and clearly labeled suggested follow-up prompts. The [companion agent guide](../how-to/add-a-plugin-dependency.md) retains the technical procedure and adds a source map, failure diagnostics, and explicit acceptance boundaries.

Root/project agent instructions, navigation, and the lesson template now make both audiences explicit. Participants get the purpose, decisions, and evidence first; agents get enough detail to reproduce or extend the work. Earlier lessons retain their combined format until revised. The two-hour seminar timing remains unchanged.

This increment changes documentation and agent guidance only. Link resolution, referenced paths, historical prompt wording, and source/evidence consistency are the relevant checks; it does not rerun or extend the plugin's runtime acceptance. The underlying Revit and workbook results remain those in the dated Excel verification record.

## 2026-10-20 Apply both learning layers to the earlier work

### Author prompt

> Ok, let's apply that same two-level approach to the earlier lessons on research, Git, setting up Revit development, and building the readiness check. Keep the participant story focused on how we engaged the agent.

Starting from `b82288a`, the agent added a [research participant story](../tutorials/00-research-before-building.md) and rewrote the [Git](../tutorials/01-directing-an-agent-with-git.md), [setup](../tutorials/02-build-a-read-only-revit-command.md), and [readiness](../tutorials/03-check-required-type-information.md) lessons around actual direction, prior context, human judgments, delegated implementation, corrections, and assessable evidence. Excerpts and suggested prompts are explicitly labeled. Research and Git point to the initial import checkpoint because that work began before this repository existed.

Companion guides now cover [research](../how-to/research-existing-solutions.md), [Git](../how-to/work-with-git.md), [setup](../how-to/build-read-only-revit-command.md), and [readiness](../how-to/build-required-information-check.md). The existing clone procedure remains their shared acquisition reference. Setup and readiness retain the earlier technical detail, with source maps and clearer boundaries between original milestones and current code: the probe remains package-free, while current readiness scripts also build and test the later Excel exporter.

The learning path, root/project agent guidance, technical entry points, and seminar navigation now expose all five lesson pairs, including Excel. The original two-hour agenda, archival evidence, implementation, and model files are unchanged. This completes the earlier lessons' conversion noted as pending in the preceding journal entry.

Documentation checks resolved local links, confirmed cited commits exist, matched all eight historical prompt quotations/excerpts in the four stories against the prior journal, and confirmed every command/code block from the original setup and readiness lessons is retained in its agent guide. Technical paths and current script behavior were inspected, and the change contains only Markdown. Existing dated build and Revit results remain historical evidence; no new runtime pass is claimed by this revision.

## 2026-10-20 Compare MCP bridges and check current versions

### Author prompt

> Ok, let's move on to the MCP server. Compare the existing Revit bridge with a minimal custom bridge around our readiness check, and record the research and decisions in both teaching layers. We need to update he MCP bridge to MCP 2.0 or whatever the latest is and this teaching note about checking for the latest versions is important.  Start that research now.

Starting from `4784b24`, the agent created `docs/mcp-bridge-research`, rechecked official protocol/releases and package registries, inspected three stable SDK checkouts, reused the unchanged original bridge checkout, and traced its RevitMCPSDK dispatch dependency to the source commit recorded in the published 2024 package. Five trees were clean; 29 relevant files were hashed. The [evidence snapshot](../research/evidence/mcp-version-observations-2026-10-20.json) records observations and package provenance.

The current official protocol revision was 2026-07-28. Stable SDK observations were TypeScript 2.3.1, Python 2.3.0, and C# 2.2.0. The existing bridge still imported the v1 TypeScript SDK, declared ^1.7.0, and locked 1.26.0. The old npm package's latest remained on 1.x; v2 uses split package names. The migration documentation also distinguishes installing v2 from enabling the latest protocol exchange. Python's release/runtime requirement differed from the earlier main-branch inspection. RevitMCPSDK uses Revit-year versions, so the highest overall package is not the appropriate 2024 target.

The [comparison](../research/revit-mcp-bridge-comparison.md) records both adaptation benefits and the work it would inherit: protocol/API migration, tool-surface selection, socket framing/access, document identity, deadlines, packages, and readiness integration. The agent recommends a small original Revit bridge and a separate C# server, retaining the existing reader/evaluator/report. This is the agent's proposed next experiment, not an architecture the author explicitly prescribed or a claim of runtime compatibility.

The [participant story](../tutorials/05-research-and-choose-the-mcp-bridge.md) focuses on how the author asked for comparison and current-version research. The [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md) records repeatable version checks, source locations, proposed boundaries, and protocol/IPC/host acceptance gates. Root/project guidance and the research template now require current-version checks while retaining reproducible compatible pins.

This increment performs research and records the decision basis. It does not install dependencies into the lab, modify the readiness command, upgrade an upstream bridge, register a server with the agent client, or claim new Revit results. The next proof is a live MCP call yielding the same independently verified readiness evidence, with the actual protocol revision and error behavior recorded.

Documentation/evidence checks passed: 529 local Markdown links resolved; five inspection trees remained clean at their recorded commits; all 29 working-file and Git-blob hashes matched; the bridge's manifest/lockfile and the three SDK release/registry versions agreed with the snapshot. The participant prompt matches the recorded instruction. The reviewed change contains Markdown and one research JSON snapshot, with source, packages, and Revit models unchanged.

## 2026-10-20 Specify the bridge and prepare external review

### Author prompts

> Ok, let's build the minimal custom bridge with C# MCP SDK 2.2.0 and protocol 2026-07-28. Verify our three sample cases end to end and record the process in both teaching layers in bim-agent-lab. Also, one important thing to emphasize is whether you are using any code from the SDK 1.26.0, whether you're just using it as a learning precedent, or whether you're going handroll your own from scratch.

Before product implementation, the author added:

> I also want you to write a spec and implementation plan, with review sessions that we hand to an external reviewer, so that we can teach that skill...use the superpowers brainstorming skill protocol.

The author selected the direction proposed by the earlier research, then explicitly requested a staged design/review process. Starting from clean `7d3ae71`, the agent created `docs/revit-mcp-spec`, read the installed superpowers 6.4.2 brainstorming skill, and classified the new server/host connection as architectural work. The skill requires review of the actual written spec before the implementation plan, and review of that plan before execution. Existing decisions were retained rather than asking the author to repeat the brief.

The agent inspected the reader, manual command, report, serializer, project boundaries, installer, and independent comparator, and reopened the official C# release, dated protocol, Autodesk ExternalEvent guide, and Microsoft pipe documentation. The [spec](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) defines two named tools, context/document/checklist binding, a separate modern server, original local IPC and dispatch, finite queue/time/size bounds, structured failures, and acceptance A1-A9. It explicitly distinguishes adopting the official C# SDK from using the older TypeScript SDK/bridge as a precedent, reusing lab BIM code, and writing the connection ourselves. No SDK or bridge code was installed or copied during this design increment.

Three [review handoffs](../superpowers/reviews/revit-mcp-bridge/README.md) cover spec, implementation plan, and code/runtime evidence. The spec packet is ready for an external reviewer; the other packets are instructions for later artifacts. A [review template](../../templates/external-review.md) records reviewer identity, exact revision, findings, evidence, limitations, author dispositions, corrections, and approval. The agent's [self-review](../superpowers/reviews/revit-mcp-bridge/00-authoring-self-review.md) is explicitly separate. No external reviewer was named or contacted, no findings were received, and no approval is claimed.

The [participant story](../tutorials/06-direct-design-and-external-review.md) teaches how the author engaged the agent and why spec, plan, and review answer different questions. The [agent procedure](../how-to/run-spec-plan-and-implementation-reviews.md) records the skill version/hash, source boundaries, exact stage sequence, review handoffs, provenance checks, and later implementation/evidence requirements. The two one-hour seminar timings remain unchanged; development review sessions can be demonstrated selectively and repeated after the seminar.

The original build objective remains active in the project, with implementation deliberately following the requested written-artifact reviews. The current checkpoint is documentation only: spec revision 1 and R1 handoff prepared; implementation plan, external reviews, bridge deployment, and new MCP/Revit acceptance still pending.

Documentation verification resolved 624 local Markdown links with no missing targets, confirmed both new prompt quotations match the journal, checked all nine acceptance IDs and the three literal sample names, and found no unfinished spec placeholders. The scoped change contains Markdown only. No product test or host execution was represented as performed for this documentation checkpoint.

## 2026-10-20 Respond to external R1 and clarify the seminar clients

The author supplied external reviewer feedback headed **“R1 recommendation: revise before approval.”** The reviewer identified itself as Codex in a separate review context that did not author the spec, and named reviewed revision 1 at `b56c0dc219e1269f5b90542004267a2d388f338d`. It inspected documents/source and explicitly ran no builds or MCP/Revit runtime tests. The [feedback record](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md) preserves the reviewer wording and original revision, separately from the drafting agent's dispositions.

The agent used superpowers receiving-code-review, checked the spec against the reader/evaluator/checklist/comparator source, and accepted all three findings. R1-01 required a named application/version/configuration baseline and an explicit fallback policy; R1-02 required successful but incomplete results through the whole pipeline; R1-03 required an accept-time limit before a full pipe frame/deadline exists.

When asked to choose the required client scope, the author clarified:

> are you asking about the agent desktop app like Codex or Claude code?

The agent explained the difference between the AI client application and the C# MCP server. Asked which application must work for the first seminar demonstration, the author chose:

> Codex desktop and Claude Desktop

The agent observed installed Windows package versions `OpenAI.Codex 26.1013.4894.0` and `Claude 3.1623.0.0`, recorded available engine/executable metadata, and proposed the shared local stdio configuration without changing client settings. These are initial test baselines, not compatibility passes. [Spec revision 2](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) requires protocol 2026-07-28 in **each** desktop application; an SDK harness or older fallback cannot satisfy A8. Each must demonstrate discovery/context, the three principal cases, a structured failure, and both incomplete controls, including truthful presentation of zero findings with incomplete coverage.

Source inspection established a useful nuance: an unresolved declared element makes overall coverage false through `ScopeIssues` while the evaluator can remain complete for its observed subset. A missing used-type requirement makes both completeness flags false and retains exact evaluator-unexamined identities/reasons. Revision 2 specifies both outcomes without changing the principal 0/0, 2/6, and 3/8 answers or weakening the comparator. Control sidecars must be prepared on disposable copies before a fresh context, keeping context-change failures distinct from evaluation.

The timeout correction starts a non-resetting five-second header/body timer at pipe acceptance, releases stalled slots, and then uses only the validated remaining processing budget. Its acceptance includes filling all eight slots with no-byte, partial-header, and partial-body connections, trickled input, measured cleanup, and a subsequent valid request.

The [participant story](../tutorials/06-direct-design-and-external-review.md) now shows the real review and client clarification; the [agent guide](../how-to/run-spec-plan-and-implementation-reviews.md) explains source validation and the exact corrections. The [updated R1 packet](../superpowers/reviews/revit-mcp-bridge/01-spec-review.md) requests focused follow-up on the three findings. No reviewer closure or author spec approval is inferred. This revision is documentation only; product code, dependencies, models, and client settings remain unchanged, and no new host/runtime test was performed.

Documentation checks resolved 650 local Markdown links without missing targets, retained all nine acceptance IDs, mapped all three R1 findings to corrections, confirmed the three principal sample rows are unchanged, and checked current status text against the received review. The diff contains Markdown only; whitespace checks passed. The negative-control expectations were checked against reader/evaluator semantics, including NC1's additional other-host instance, without running or altering production code.

## 2026-10-20 — Close specification review and turn approval into an implementation plan

The author supplied the external R1 follow-up on spec revision 2 at `0ef8cdacb5112545a9aa6fbc59e0823df52f2e44`. It closed all three findings at the specification level, found no new blocker, and recommended proceeding to planning after author approval. It identified both desktop applications' untested protocol behavior as the remaining risk to check early. The reviewer inspected documents/diff/source and ran no builds or runtime tests. The [separate follow-up record](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md) preserves that response; the original R1 feedback/dispositions remain unchanged.

The author's actual approval was:

> Approve revision 2 and write the plan

The agent recorded approval of that exact written spec and used superpowers writing-plans 6.4.2. It wrote a [nine-task implementation plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) with files/interfaces, behavioral tests, exact limits, expected results, deployment/rollback, independent evidence, and A1–A9 traceability. It inspected the pinned official C# SDK server/client options and handler/transport interfaces to make the first checkpoint concrete. The spec's normative sections 1–8 were preserved; only status/review metadata changed.

The first execution checkpoint will use a separate synthetic SDK probe in **both actual desktop applications**, capturing protocol 2026-07-28 and structured success/failure before implementing the Revit connection. An SDK harness pass or older fallback cannot substitute. A probe pass would prove protocol compatibility only; live Revit evaluation, three principal cases, both incomplete controls, independent comparison and truthful client presentation remain later required work.

The participant layer explains how review, exact-revision approval, and asking for a plan shaped the agent's next action. The agent layer describes the task interfaces, checkable tests, package adoption, evidence boundaries and handoff. The [R2 packet](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md) now has the real plan and read set. The agent recommends Native execution because the tasks share tightly coupled interfaces, leaving selection to the author and retaining the requested external review sessions.

This checkpoint changes documentation only. No SDK dependency was installed, no client configuration changed, no bridge built, and no model/runtime test performed. Document checks resolved 712 local Markdown links without missing targets, confirmed approved spec sections 1–8 and the original R1 feedback unchanged, and checked nine tasks/A1–A9 coverage with no placeholders. Whitespace checks passed. Document self-review supports the planning artifact; external R2 recommendation, author plan review/execution choice and all implementation acceptance remain pending. Use `git log --oneline -- docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md` for the actual plan checkpoint rather than inventing a hash inside its own commit.

## 2026-10-20 — Connect R2 findings through operation lifetime and ownership

The author supplied [external R2 feedback](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-feedback.md) on plan revision 1 at `17108c239e3aa6f384c89f15d2fd623dbce60760`: **revise before implementation**. The reviewer found three major gaps (cancellation/budget lost before the Revit reader, lost SDK stdin shutdown behavior, and incomplete partial-install recovery), plus inconsistent instance-selection errors. It inspected source/documents and performed no builds, installs, desktop or Revit tests. The attachment did not name the reviewer; its wording and hash are retained without inventing an identity.

The author's accompanying direction identified a common cause. This is an actual excerpt:

> The common gap is reasoning about what remains safe when an operation stops halfway through.

The author asked for an operation-by-operation account of admission, cancellation, timeout, disconnect, shutdown and partial failure, naming resource owners, conditions before each next action and safe terminal states. The drafting agent checked the actual signatures, pinned SDK StdioServerTransport/StreamServerTransport, and installer. The diagnosis held: the first plan's task-level coverage checks had not proved that guarantees survived the handoffs.

[Plan revision 2](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) now starts with a shared lifetime/ownership table. Requests carry the established budget and cancellation into preparation/read/publication; response cancellation is distinguished from a physically finished callback. The SDK host retains cancellation-aware stdin in both trace modes and requires a real subprocess test with stdin kept open. Installation requires whole-plan preflight, verified backup bytes and a durable recovery record before first mutation, including deleted files and fixture inventories; interruption tests cover apply and recovery. Instance selection now has a defined exception contract and production tool mapping before any pipe client exists.

The provenance account also becomes more precise: a small stdin helper is **planned to be adapted from official C# SDK 2.2.0**, with source/notice review and attribution. No code was copied in this revision. TypeScript SDK 1.26.0 remains precedent-only; official packages supply MCP, and original IPC/dispatch/tool code reuses lab BIM computation. This follows the spec's requirement to disclose adaptations instead of keeping an inaccurate all-original claim.

The [participant story](../tutorials/06-direct-design-and-external-review.md) teaches how the author challenged the common reasoning gap; the [agent guide](../how-to/run-spec-plan-and-implementation-reviews.md) teaches lifetime tracing and transition tests. The [R2 follow-up packet](../superpowers/reviews/revit-mcp-bridge/02-plan-review.md) requests assessment of that shared model and per-finding closure. The architecture, approved spec and principal/negative-control expectations remain unchanged. This is a documentation revision: no product edits, dependency adoption, installs, model/config changes or runtime tests. External closure and author plan review/execution choice remain pending. Document checks resolved 737 local Markdown links, confirmed the full approved-spec file and prior R1 records unchanged, preserved the received R2 wording/hash and sample assertion block, and found no obsolete request-only executor signatures or placeholders. Whitespace checks passed. These checks validate the revision artifact, not its future runtime behavior.

## 2026-10-20 — Record external R2 closure and the next author decision

The author supplied the [external R2 follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) on plan revision 2 at `cfbfe043bd6c4d0627424ea3b099d0007459cb91`. The reviewer closed R2-01 through R2-04 **at the plan level**, found no new blocking findings, and confirmed that the underlying operation-lifetime/ownership problem was addressed. The review inspected documents and source, including the pinned SDK and existing reader/installer. It performed no builds, installation, desktop compatibility checks or Revit runtime tests. The supplied message did not name the reviewer; the record does not invent an identity.

The participant story now completes the engagement loop: the author challenged the common reasoning behind several findings, the agent revised the whole-operation account, and an external reviewer checked both the common correction and individual findings. The agent guide records exact revision/state, how to preserve original feedback separately, and where a future agent should resume. The approved spec and reviewed implementation tasks remain unchanged; current-status metadata now records closure.

The next decision is **author approval of the written plan and execution-method selection**. Native execution is recommended because the tasks share connection/queue/document-lifetime interfaces; it is not selected. The earlier author approval concerned the specification and authorized writing the plan. Returning an external recommendation does not substitute for approval of that later artifact. No extra R2 review is needed simply to record this closure.

This is a documentation checkpoint. No product code, dependency adoption, installation, client settings or runtime tests are included. CP0 still precedes connection work; both desktop applications, all three live samples, NC1/NC2, interruption/recovery tests and A1–A9 still need implementation evidence before R3 closure. Documentation checks resolved 757 local Markdown links, confirmed the approved specification and three historical review records unchanged, and compared the reviewed plan’s substantive content against `cfbfe04` without differences. Only Markdown files changed; whitespace checks passed.

## A reusable research prompt

The following is a teaching template, not an additional instruction from the author:

> We want a Revit 2024 tool for wall and ceiling BIM staff that checks model readiness and reports supporting inventory and quantities. Before proposing implementation, research existing tools and repositories. Prefer original documentation and source repositories. Clone serious candidates into a references folder and record each origin and inspected commit. Inspect the license, supported Revit versions, runtime, dependencies, and relevant code. Explain whether we should use it as a tool, adapt it, or learn from it. Separate documented claims, source observations, and locally tested behavior. Identify the SDKs, API documentation, libraries, sample models, and development tools we need, and record the remaining verification work.

The prompt works because it supplies purpose, host version, audience, evidence standards, evaluation criteria, and a concrete deliverable. Follow it with a focused question about the weakest assumption; repeating the entire brief is unnecessary when project notes preserve it.

## Overall sequence to teach

| Step | Work | Concrete output |
|---|---|---|
| 1. Frame the task and establish Git | Audience, problem, constraints, three useful questions; inspect or initialize our own local repository | Brief, project instructions, and a recorded baseline |
| 2. Survey and inspect existing work | Find candidates; clone them, record revisions, read licenses, dependencies, and relevant source | Source-backed comparison and reuse recommendation |
| 3. Gather resources | Versioned SDK/API docs, runtimes, libraries, sample files, inspection tools | Resource register and acquired build inputs |
| 4. Check feasibility | Build the selected precedent or a minimal probe; read a model value | Evidence supporting the architecture choice |
| 5. Define the design | Data scope, rule meanings, outputs, error handling, sample answer sheet | Reviewable design |
| 6. Set up the implementation | Development branch, updated project instructions, pinned dependencies, repeatable build | Working development baseline |
| 7. Build one complete workflow | Revit query through readiness output, then expose it through MCP | A small end-to-end demonstration |
| 8. Verify and review | Compare against controlled cases, schedules, and manual inspection | Reproducible results and documented limits |
| 9. Package and rehearse | Installation, notices, examples, reset copies, process guide | Takeaway kit and dependable demonstration |

Research can reveal that the problem or architecture needs revision. Return to the affected decision and record the reason. The journal continues through implementation rather than ending after the initial planning.

## Template for later entries

Record the author's prompt or a clearly labeled excerpt; the decision requested; constraints supplied; the agent's actions; the evidence; any correction or rejected approach; the outcome; and the lesson worth showing on stage. Label a suggested prompt as a suggestion, never as a historical prompt.

Current next action: obtain author approval of [plan revision 2](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) reviewed at `cfbfe04` and an execution-method choice. [External R2 follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) has closed all four findings and the underlying planning concern at plan level. Approved spec `0ef8cda` remains the contract. CP0, three live samples, incomplete controls and both desktop clients remain required; no runtime proof follows from plan revision alone. Current-version checks remain required. Physical-condition/quantity checks remain separate increments.