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

## 2026-10-20 — Teach participants to ask the reviewer for the missing concept

Before execution, the author asked to make the general lesson explicit: when independent review produces “2-3 or more major findings,” the user should have the reviewer investigate whether the coder is missing an underlying concept or framework, then pass that insight back to the coder. This extends the R2 example into a reusable way to direct agents.

The [participant lesson](../tutorials/06-direct-design-and-external-review.md#ask-what-connects-several-findings) now gives paired, clearly labeled suggested prompts: first ask the independent reviewer for an evidence-backed diagnosis; then ask the coding agent to assess and apply it, inspect related work, and show verification. The number of findings is presented as a practical signal to investigate, not a measured frequency claim or proof that all defects share one cause. The human can request the diagnosis without already knowing the technical answer.

The [agent procedure](../how-to/run-spec-plan-and-implementation-reviews.md#review-operations-over-time-not-just-individual-tasks), root instructions and [review template](../../templates/external-review.md) capture the lower-level responsibilities: exact revisions, supporting and contrary evidence, finding-to-concept mapping, broader application, test references and reviewer follow-up. The workshop guide and existing review block in the HTML agenda point to this lesson; both one-hour timings remain unchanged. The R2 records retain their actual attribution and wording; the new reusable prompts are not presented as historical messages.

This is a teaching-material update before implementation. It does not reopen the closed R2 review, change the reviewed plan or specification, or supply runtime evidence. Author plan approval and execution-method selection remain pending. Documentation checks passed for 764 local Markdown links and agenda links/section targets. Both session timings remain 60 minutes; the specification, plan and nine review records are unchanged. Whitespace checks passed.

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

Current next action: resolve CP0 using the [desktop compatibility evidence](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md). Plan revision 2 is author-approved and Native execution selected. Both installed desktop clients fail the strict protocol requirement; Task 2 and dependent Revit work remain unstarted. An SDK pass does not change the approved acceptance contract.

Later alternative-client finding: [Claude Code 2.1.305 passed the new-protocol probe](../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md). The current choice now includes preserving the protocol with a different seminar client scope. No scope/protocol revision has been approved; the original desktop CP0 remains failed.

## 2026-10-20 — Execute the compatibility checkpoint and retain a failed result

Actual author prompt:

> Ok, I approve plan revision 2. Let's use Native execution and start with the compatibility check in Codex desktop and Claude Desktop. Keep recording the process in both teaching layers.

The agent recorded approval of reviewed commit `cfbfe043bd6c4d0627424ea3b099d0007459cb91`, retained the existing checkout on branch `feat/revit-mcp-bridge` from `ff8a4f3`, and started Task 1. The existing readiness suite passed before changes. Fresh registry research found SDK 2.3.0; the approved 2.2.0 pin was kept and its resolved graph/licenses reviewed. The official C# 2.2.0 stdin helper was adapted with attribution and notices; TypeScript 1.26.0 stayed precedent-only.

The separate synthetic probe and official SDK test client proved discovery, structured success/error and protocol 2026-07-28. Fourteen C# tests and five Python test methods passed, including actual Windows shutdown with stdin still open and recovery in new processes. A disposal test found double stream disposal; host-owned raw streams and borrowed SDK wrappers corrected it. Settings tests also caught Windows newline normalization during unrelated edits.

The agent prepared durable backups before temporary client entries, then tested both actual desktops. Claude's Developer menu reloaded its configuration. The author reloaded Codex and replied “reloaded.” Captured traffic showed Codex requesting 2025-06-18 and Claude requesting 2025-11-25. Both received SDK error -32022 before tool discovery. The agent retained unchanged lab-only transcripts, marked subsequent calls as not reached, and did not start Task 2 or change the protocol to obtain a pass.

Both temporary entries were removed. Codex had added enabled=true; recovery refused the changed owned entry. The agent inspected the delta, prepared separate recovery bytes and removed only the verified probe block, checking all unrelated settings remained equal. Claude's MCP configuration was reloaded after cleanup; Codex's cached registration refresh remains a separate UI step.

Outcome: CP0 fails on the recorded installed versions. No alternate protocol, production Revit tool, bridge installation, live sample acceptance or R3 review is claimed. The author can keep the strict requirement and test supporting client versions or explicitly revise protocol acceptance before testing an alternative. The [participant story](../tutorials/07-test-desktop-compatibility-first.md), [agent procedure](../how-to/verify-desktop-mcp-compatibility.md), [dependency review](../research/mcp-sdk-dependencies.md) and [verification record](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) preserve both the technical evidence and how the author directed the work.

## 2026-10-20 — Challenge the version restriction before changing the requirement

Actual author prompt:

> Ok, check whether newer versions or settings in Codex desktop and Claude Desktop support 2026-07-28 before we change the requirement. Record what you find in both teaching layers., but I still don't fully understand. Is this a footgun and self-imposed restriction or something real?

The agent checked the installed Codex engine, app updater, Claude's official Windows latest-download endpoint, official product documentation and pinned SDK source. Codex's configured updater reported up to date. Claude's latest-download redirect named the same product version installed locally; its native updater's final result was not captured. These are bounded observations, not a claim that every distribution is current or incompatible.

The important correction was conceptual: SDK 2.2.0 supports older and newer protocols by default. The lab's explicit `ProtocolVersion = "2026-07-28"` setting and approved acceptance criterion make older exchanges a blocker. The agent should have explained that adopting a modern SDK does not require excluding older clients. No required readiness capability exclusive to the new revision has been identified. The protocol/lifecycle difference is real, but the exclusivity is our choice. The user challenged that choice without needing to know the configuration detail behind it.

A separate, original SDK diagnostic held package 2.2.0 constant and compared strict/default server options against three pinned client revisions. All six expected outcomes were observed: two strict-server legacy rejections and four successful discovery/success/error exchanges. Locked restore/build passed with zero warnings/errors; the diagnostic exited 0. This is SDK evidence only, with no copied legacy TypeScript source, SDK downgrade, production host change or live Revit test.

Codex's installed engine exposed an under-development `mcp_2026_07_28` flag. After saving durable private recovery, the agent temporarily enabled that flag and registered the original strict probe. The author replied **“reloaded.”** Six fresh desktop connections still used `2025-06-18` and failed before discovery. Current OpenAI source confirms the flag's role; a CLI listing does not establish that the running desktop loaded it. A full application restart remains the next activation test. The temporary flag/probe entry and private recovery remain available for that test. The agent found no documented resolving Claude Desktop local-stdio setting in the sources inspected.

The [participant story](../tutorials/07-test-desktop-compatibility-first.md) focuses on asking the agent whether a blocker serves the user's purpose and challenging an overbroad conclusion. The [agent guide](../how-to/verify-desktop-mcp-compatibility.md) gives the SDK diagnostic, feature/update inspection, activation and recovery procedure. Both link to the same [research record](../research/mcp-desktop-protocol-investigation.md) and [follow-up evidence](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/observations.json), supplementing original checkpoint `9ea3151b323ae50b919fed2e4cb89e10362e598a`. The approved spec/plan and production pin remain unchanged. Task 2, actual desktop success/error acceptance, live Revit cases and R3 remain outstanding.

Before saving this research checkpoint, checks resolved 192 local links in changed/new Markdown files, verified fourteen evidence hashes and the exact SDK lock, and confirmed the approved spec, plan, production host and original strict probe were unchanged. Whitespace checks passed. These checks support the recorded investigation; the remaining desktop restart test is not a pass.

## 2026-10-20 — Finish the restart experiment and explain the choice plainly

After research checkpoint `32db550`, the author reported **“restarted the app.”** Six additional fresh Codex connections still requested `2025-06-18` and received `-32022`, while the installed engine listed the temporary feature true. No probe tool reached the conversation. The setting, reload and reported full restart therefore did not resolve this installed desktop's exchange. The agent has not established why the desktop did not use the new revision and does not generalize the result to every distribution or configuration.

The agent removed the temporary feature and probe entry with durable cleanup recovery, verified both flags now list false and no probe process remained, and preserved all unrelated current settings. Other settings changed during the experiment, so neither exact original bytes nor identical original parsed settings are claimed. This illustrates why recovery removes owned changes rather than overwriting concurrent work with an old snapshot.

The author then asked **“please explain what is going one here clearly.”** The agent acknowledged that it had conflated choosing a modern SDK with requiring the newest protocol exclusively. SDK 2.2.0 understands multiple message formats; the lab's added restriction rejects the desktop clients' older formats. The agent recommends keeping the SDK, allowing its built-in compatibility and verifying the real workflow in both desktops. This avoids treating an unnecessary exclusivity rule as an inherent Revit limitation. The approved requirement and production host remain unchanged pending an explicit decision; Task 2 stays unstarted.

Both teaching layers and current agent/status guidance now show the completed experiment and cleanup. The [follow-up evidence](../../projects/revit-readiness/docs/verification/mcp-client-compatibility-evidence/follow-up/observations.json) retains twelve connections across the two stages and twenty-seven hashed evidence files overall. No newer SDK, application installer, alternate production protocol, live Revit test or R3 review is included in this follow-up.

Follow-up verification resolved 153 local links in changed Markdown files, verified all twenty-seven evidence hashes, and independently rejected each of the six restart transcripts for the expected older-handshake reason. The approved spec, plan and production host remain identical to the original CP0 implementation checkpoint; whitespace checks passed.

## 2026-10-20 — Answer the protocol-generation question directly

The author asked whether compatibility meant falling back to a 1.x protocol despite using SDK 2.2.0, then challenged the agent's naming-focused reply and explicitly requested web verification, citing prior versions 1.28 and 1.30. The agent's reply had obscured the practical consequence by emphasizing that protocol revisions have date labels.

Official Python SDK 1.28.0 and 1.30.0 source both identify `2025-11-25` as latest and include `2025-06-18` in their supported revisions. The corrected answer is **yes: the proposed desktop exchanges would use a 1.x-era protocol through a 2.2.0 SDK**. They would not demonstrate the newer protocol behavior. The [research record](../research/mcp-desktop-protocol-investigation.md) pins the sources and avoids inferring either desktop's internal SDK or the exact package previously used by the author. Both teaching layers now explain that accurate terminology must still answer the user's practical question. This correction changes documentation only; the approved contract and runtime remain unchanged.
## 2026-10-20 — Test another agent client before accepting a compromise

Actual author prompt:

> Ok, what would we give up by using the older protocol? Check whether Claude Code or another agent client can actually use 2026-07-28 before we change the requirement and is this specific to C# or is it across all supported languages?

The author challenged the proposed fallback by requesting its practical cost and a concrete alternative. The agent researched official protocol changes and Claude Code's documented runtime selection, then tested the existing strict C# SDK 2.2.0 probe with child-process settings only. No production code, SDK pin or acceptance requirement changed.

The first attempt reached discovery and the catalog using July 2026, but expired authentication prevented model calls. The author signed in and replied **“logged in.”** A fresh Claude Code **2.1.305** process then called the synthetic tool successfully and exercised its deliberate structured error. The existing independent transcript checker passed unchanged; the client's answer distinguished both outcomes. No probe process remained. An earlier inventory had shown 2.1.239; the actual exchange and executable inspection establish 2.1.305 as the tested version. The transition mechanism was not established, and no update command was issued by this experiment.

This result demonstrates a viable new-protocol alternative and cross-language communication between the client's documented TypeScript runtime and our C# server. It does not satisfy the existing requirement for Codex desktop and Claude Desktop. Keeping the protocol while changing client scope is now an evidence-backed option; accepting older desktop exchanges remains a different option. Neither was selected. Current readiness and Excel functionality has no identified dependency on July's new mechanisms, but teaching the new protocol can itself justify choosing a supporting client. Optional extensions and future remote workflows need separate tests.

The [participant story](../tutorials/07-test-desktop-compatibility-first.md#ask-what-we-lose-and-test-another-client) emphasizes how the author's questions changed the investigation. The [agent procedure](../how-to/verify-desktop-mcp-compatibility.md#test-an-alternative-client-without-changing-the-server) records a bounded, isolated invocation and verification. The [research comparison](../research/mcp-protocol-tradeoffs-and-agent-clients.md) and [runtime evidence](../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md) separate documented support, actual traffic and untested features. Raw lab traffic is retained with hashes; only labeled selected fields from the application output are public. SDK 1.x code was neither installed nor copied, and no MCP implementation was hand-written. Task 2 remains gated by the unresolved approved client contract.

## 2026-10-20 — Choose the desktop experience for beginners and preserve an upgrade path

Actual author clarification:

> just FYI, the participants are very green and will not like the Claude Code terminal style interface. We should fallback in such a way that we don't kill the possibility that we can switch when Codex App and Claude Desktop get their shit together.

The author chose the audience's familiar desktop workflow over the technically successful CLI alternative. The agent recorded this as a requirement change, not another compatibility failure. The [policy amendment](../superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) and spec/plan revision 3 retain C# SDK 2.2.0 and both required desktop applications. They direct SDK-provided multi-version production behavior, explicit verification of the observed older revisions, and a separate strict July test through the same host composition. No SDK downgrade or custom protocol translation is needed.

When a supporting desktop release/settings combination arrives, the agent must first verify the strict probe in that actual application, then the full A8 workflow on the compatible production build, and record the new baseline. Participants do not need protocol controls or a different server language. No recurring monitor or automatic upgrade was configured. The earlier strict desktop failures and successful Claude Code evidence remain intact at their real checkpoints; the research checkpoint is `e5cb2ac`.

The [participant story](../tutorials/07-test-desktop-compatibility-first.md#let-the-audience-determine-the-tradeoff) explains how audience knowledge changed the decision. The agent guidance and external implementation-review packet now carry the precise changed obligations. Prior R1/R2 closure remains attached to revision 2; this author-directed amendment is not mislabeled as independently reviewed. Runtime changes and fresh compatible-desktop CP0 remain pending, so Task 2 has not begun. The direction is settled; future work should implement and verify it, rather than ask the author to choose a terminal client again.


## 2026-10-20 — Apply compatibility and prove both desktop exchanges

Actual author instruction:

> Ok, let's apply the compatibility change and test it in Codex desktop and Claude Desktop. Keep the 2026-07-28 regression test and record the process in both teaching layers. Do we need an updated spec and plan?

Spec/plan revision 3 and the amendment already defined the changed acceptance. The author explicitly authorized applying it; Native execution continued without reopening approval or client choice. The host now uses SDK 2.2.0's built-in compatibility by default and retains strict July policy through the same composition path. No new dependency, SDK 1.x code, translation proxy or Revit behavior was added.

Meaningful red tests preceded the host/checker changes. Nineteen C# behavioral tests, four standalone SDK/wire comparisons and the existing readiness regression passed. The actual Codex desktop conversation passed June's exchange after the author's reload. A fresh regular Claude-tab chat passed November's exchange after the author allowed the synthetic tool. Claude's local-agent connection had a different identity from its discovery-only session; the evidence retains both and counts only the complete exchange. Its omitted optional listing params exposed an over-strict checker; a new failing regression then the minimal correction brought the Python suite to ten passing methods, with the raw capture unchanged.

Both temporary config entries were removed while preserving unrelated settings. Claude's entry had disappeared after the initial write; its cause was not established, so a new receipt preceded reapplication. Codex added enabled=true, handled through separately prepared reviewed cleanup. These are concrete examples of the earlier operation-lifetime and ownership lesson.

The [fresh verification record](../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) and hashed lab-only evidence establish amended CP0. The [participant story](../tutorials/07-test-desktop-compatibility-first.md#ask-the-agent-to-implement-the-choice-and-preserve-the-way-forward) focuses on the author's direction; the [agent guide](../how-to/verify-desktop-mcp-compatibility.md#completed-compatibility-increment-and-future-upgrades) provides exact verification and upgrade steps. Earlier strict desktop failures and Claude Code proof remain preserved. R1/R2 closure still applies to revision 2; R3 must review the amendment and implementation. Task 2 is now eligible but unstarted. No new live Revit acceptance is claimed.


## 2026-10-21 — Build the bridge foundation and carry review insight into tests

Actual author instruction:

> Ok, let's proceed with Task 2 and start building the actual Revit bridge. Follow the updated plan, keep the compatibility tests in place, and record the process in both teaching layers.

Native execution continued from `9b4eb516819f4c6fc5d90ccdefc7751b0705e694` on the existing feature branch. The author scoped a concrete increment and retained prior client/protocol decisions. No repeat approval or desktop reload was needed. Task 2 implements plain contracts, byte framing, strict envelope validation, original request budgets and lifetime ownership; actual pipe/queue/Revit integration stays in later tasks.

The agent wrote compilable stubs and observed all 27 behavioral cases fail on both runtime targets before implementing the behavior. A preceding test-source compiler error was fixed as setup work and was not treated as the red proof. During implementation, tests exposed the framework JSON infoset reader accepting a trailing object. Complete-input framework parsers now supplement explicit shape validation. Another case caught the older parser normalizing an unpaired Unicode surrogate; validation now examines the original value before DTO conversion. A final timestamp check also caught a permissive format accepting a decimal point with no fractional digits; explicit supported formats now reject it. The implementation uses existing platform APIs and adds no NuGet package or copied reference source.

The independent review's broader insight now has concrete tests: the original deadline survives handoffs, cancellation during preparation prevents the next guarded action, and a canceled response does not release resources still used by an unfinished callback. The last distinction is embodied in separate Response/Quiesced tasks and a cleanup helper. These are component tests, not yet tests of an executing Revit callback.

The completed offline suites pass 27 bridge cases on each target, bidirectional message exchange, independent Python value checks, 19 MCP behavioral cases, ten protocol/settings Python methods, four SDK/wire comparisons and the existing 18 core/seven Excel/isolated-host regression. Compatible old revisions and strict July support remain tested. No Revit deployment or fresh desktop UI test occurred during this increment; prior CP0 evidence remains preserved.

The [participant story](../tutorials/08-build-and-test-the-bridge-foundation.md) focuses on the author's direction and evidence the person can assess. The [agent guide](../how-to/build-and-verify-bridge-foundations.md) records source/dependencies, reproduction, caller obligations and limits. Both reference the [Task 2 evidence and Git checkpoint](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations.md). Task 3 is next. R3 remains a future independent implementation/evidence review; the coder's own source inspection does not close it.


## 2026-10-21 — Revise Task 2 after independent implementation review

The author returned independent reviewer feedback on `eefb1563fdf7073dc7065668da355b0642f99709`, recommending revision before Task 3. The reviewer had rerun every supplied offline suite successfully, then demonstrated a net48 date-string conversion defect, a malformed-input exception leak, and the wrong nested completeness name in both C# and Python fixtures. The [original feedback and separate dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-review.md) preserve that distinction. No additional framing or lifetime defect was reported.

The agent reproduced the date conversion before changing production code: two new cases failed on net48 while net10 preserved the strings. It inspected Microsoft's pinned reference implementation, then separated whole-input syntax checks from constructing DTO values. Both runtimes now use the validated original string tokens; the net48 syntax-only input avoids the framework's special date inference. No returned data or capture is normalized. Negative, offset and out-of-range date-shaped values and both slash spellings are tested, including cross-runtime exchanges.

Next, three raw-control tests reproduced the FormatException leak on each runtime. The minimal correction normalizes that parser failure while retaining the existing domain-error path; escaped control characters still pass. For completeness, the agent checked actual core definitions and retained native-Revit output. The corrected independent oracle rejected the original fixture because Evaluation.Complete was missing. Both fixture producers and expected values now use CoverageComplete and Evaluation.Complete, and fresh evidence was generated instead of editing the old captures.

The [new verification record](../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) records 33 bridge cases per target, bidirectional/date-string exchanges, independent comparisons and all prior MCP/readiness/Excel regressions. [Participant story](../tutorials/08-build-and-test-the-bridge-foundation.md#use-independent-review-to-challenge-the-tests) explains the author's use of independent review; [agent guidance](../how-to/build-and-verify-bridge-foundations.md#task-2-review-corrections) records exact boundaries and reproduction. The lesson is to distinguish validity from preservation, and independent implementation from independently grounded expected answers. This is the implementing agent's synthesis, not a diagnosis attributed to the reviewer.

Task 3 remains unstarted. The corrections and follow-up packet are ready for independent review; the coder's passing tests do not close the review or establish live Revit acceptance. No package, SDK/protocol policy, client registration or installed add-in changed.

## 2026-10-21 — Close Task 2 independently and prove Windows IPC

The author forwarded the reviewer's recommendation to proceed to Task 3, closing T2-01/02/03 at `3cefe5116ec1368e0bfdcc387209bbb1e176df66` after independent reproductions, regressions and evidence-hash checks. The [unaltered follow-up and separate disposition](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md) supersede the prior pending status. The agent resumed the already-approved plan under Native execution; it did not reopen the SDK or client choice.

Task 3 adds original Windows pipes, restricted descriptors, actual process/logon/session checks and lazy binding to one bridge session. The agent researched Microsoft API contracts before implementing native interop, then used compilable stubs and failing catalog/transport tests. No new package or upstream implementation was adopted. The [research record](../research/revit-local-pipe-transport.md) distinguishes documentation, pinned reference source and observed runtime behavior.

Actual Windows tests found assumptions that in-memory tests could not settle. WindowsIdentity.Groups omitted the logon SID; a separate native query established the oracle. Combining explicit close with a framework cancellation callback raced a SafeHandle on net48. Removing explicit close left an operation pending. The final strategy gives connected I/O one cancellation owner, closes the pipe to wake it and awaits completion before releasing the worker. Existing lifetimes still outlive canceled responses until callbacks quiesce. Only owned test processes were stopped during diagnosis; Revit was untouched.

Additional red cases required correlated admission/submit errors and retryable descriptor cleanup after a lock. The agent also corrected a test assumption about net48's ninth-client timeout exception and strengthened identity cases so inconsistent naming could not mask missing OS checks. Final author-side review exposed a failed duplicate start removing the original session's descriptor; a failing test preceded the correction that ties cleanup authority to successful publication by that endpoint. These implementation choices and their limits are in the [agent procedure](../how-to/build-and-verify-local-pipe-transport.md).

The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport.md) records 27 pipe cases on each runtime, 64 stalled-connection timings and a net48 child serving simultaneous clients and a large Unicode result. All foundation, MCP compatibility/strict July, readiness and Excel regressions pass. An independent Python checker recalculates timings and rejects corrupted/incomplete evidence. Actual alternate-logon clients and remote-machine connections remain unperformed; the tests did not create accounts or change policy.

The [participant story](../tutorials/09-connect-the-bridge-processes.md) focuses on the author's review handoff and asking for proof at the boundary being built. The [focused Task 3 packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review-packet.md) is ready for independent review. Task 4's production dispatcher, the Revit adapter, production tools, all live sample/negative-control calls and both desktops' A8 remain ahead. No independent Task 3 or full R3 closure is claimed.


## 2026-10-21 — Independent review exposes cleanup conditional on success

The author forwarded a P2 review of `3e20251` declining Task 3 closure. The reviewer had rerun all supplied suites and verified all 30 evidence hashes, then paused native validation while a connected child exited. A worker fault reduced listener capacity; shutdown skipped its descriptor, leaving restart ambiguous. The [feedback and separate response](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) preserve the diagnosis: cleanup still depended on preceding work succeeding.

The agent reproduced three failing cases on both runtimes before changing production code. It normalized peer disappearance narrowly, then separated worker error reporting from owned cleanup. A distinct unexpected-worker test prevents the peer fix from concealing a still-broken shutdown path; a locked-file variant proves cleanup retry and retention of both errors.

Applying the diagnosis to adjacent boundaries exposed two author-side failures with throwing cancellation callbacks: shutdown or deferred lifetime cleanup could be skipped. New failing tests preceded retaining cancellation errors while completing cleanup and using finally around the disconnect monitor. Resources still remain available until the unfinished callback quiesces. This extends the reviewer's insight without attributing these additional observations to them.

The [fresh evidence](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) records five new regressions, 32 Pipe cases per target, all foundation/MCP/strict July checks, readiness/Excel regression and untouched original evidence. The [participant story](../tutorials/09-connect-the-bridge-processes.md#review-the-failure-path-not-just-the-fix) emphasizes asking for the missing concept even when one finding is enough to reveal it; the [agent guide](../how-to/build-and-verify-local-pipe-transport.md#cleanup-after-exceptional-completion) states ownership and error semantics. No new dependency, protocol policy, installed add-in, client settings or model change was made. Correction evidence is ready for independent follow-up; Task 4 remains unstarted.


## 2026-10-21 — Review the thread that initiates cancellation

The author forwarded independent P2 closure at f79506d and a new P1: a valid pipe request expiring with a throwing cancellation callback terminated net48 and net10.0. Existing stop/disconnect callback tests had passed. The agent used receiving-code-review, systematic-debugging, test-first and Native execution guidance to reproduce the crash in disposable children before changing production code.

Initial four-case runs passed normal cancellation/early completion and failed throwing/held callbacks on both targets. A held callback exposed source disposal after executor Quiesced but before cancellation finished. After the first correction, diagnostic mutation tests failed because retained event data could be erased. Retaining the exception separately corrected that. Applying the execution-origin question to upstream/owner tokens reproduced the crash again through their own timers; explicit contained registrations replaced linked-source propagation. These adjacent observations came from the implementing agent, not the reviewer.

The final correction uses one contained cancellation path, the original budget, active-callback accounting and deferred physical disposal. All six child cases pass per target, Pipe passes 38 per target, foundation remains 33 per target, and All retains MCP/strict July/exchange checks. Readiness/Excel/process isolation pass. No package, SDK/protocol, model, installation or desktop setting changed. The peer-exit P2 stays closed; the deadline P1 requires independent follow-up before Task 4.

The [participant story](../tutorials/09-connect-the-bridge-processes.md#ask-who-runs-the-callback) explains how the author engaged the agent and labels a suggested follow-up prompt. The [agent procedure](../how-to/build-and-verify-local-pipe-transport.md#contain-cancellation-at-every-origin) records owners, failure boundaries and repeat commands. [Evidence](../../projects/revit-readiness/docs/verification/2026-10-21-deadline-cancellation.md) preserves red/green observations separately from [reviewer feedback and implementing-agent disposition](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-deadline-review.md).


## 2026-10-21 — Require an execution model before another cancellation patch

The author forwarded independent closure of the deadline P1 at b334385, a new held-callback P2, and the diagnosis that resource ownership had not been matched by execution/waiting reasoning. The author asked for a complete model before another local correction. The unchanged reviewer reproducer confirmed stop blocked before returning its task and disconnect recovered only seven connections on both runtimes.

The agent committed the [model](../explanation/bridge-execution-and-waiting.md) at 373e5a8 before production changes. The initial fourteen-case matrix failed ten cases per target; four deadline cases passed, illustrating why previous examples missed real waiting dependencies. The correction separates immediate invalidation, private transport wakeup, owned callback notification and deferred retirement, with eight retained-request permits across endpoint restarts. A fifteenth test caught exceptional Quiesced leaking a permit after successful disposal; that author-side issue was corrected and its red evidence retained.

Two older test assertions were revised explicitly: immediate state is checked through Context rather than asynchronous token notification, and Stop reports known errors without waiting for future callback failures. The zero-read, original-budget, retained-error and physical-lifetime guarantees remain tested. The complete progress matrix now passes; full All and readiness regressions are recorded in the [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-cancellation-progress.md). Both unchanged reviewer stop/disconnect reproductions now finish network shutdown and descriptor cleanup before callback release, with all eight replacement connections.

The [participant story](../tutorials/09-connect-the-bridge-processes.md#ask-what-must-progress-while-work-is-still-running) explains how the author demanded the missing framework; the [agent guide](../how-to/build-and-verify-local-pipe-transport.md#prove-progress-separately-from-safety) specifies interfaces and tests. [Independent feedback and coder disposition](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-progress-review.md) remain distinct. No package, copied framework source, SDK/protocol, installation, model or desktop-setting changes. Deadline P1 is closed; new P2 follow-up, Task 4 and live acceptance remain ahead.

## 2026-10-21 — Carry the reviewed execution model into dispatch and contexts

The author relayed independent closure of Task 3 at af4c67b, with the explicit direction that Task 4 preserve nonblocking admission, authoritative context guards and separation from borrowed callbacks. [Original follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-closure.md) is separate from the coder's response. The agent continued the approved Native plan and committed an [execution/waiting model](../explanation/dispatch-and-context-lifetime.md) at 638e288 before production edits.

The model identified a cancellation/publication gap: a final check and response completion need the same invalidation gate. The new queue coordinates only bounded completion there, with no reader inside; registration ownership and admission finish also precede Quiesced. The code adds four-waiting/one-executing dispatch, all signal-result paths, bounded draining and a 32-entry, five-minute context store. Existing Revit/SDK/provenance boundaries remain unchanged.

The retained first stub run had 39 failures per target; a publication case made 40. An initial timestamp-format error in test setup was corrected before that retained run, not counted as production evidence. The first implementation passed 40, and four production-queue/real-pipe cases then passed. Added negative tests exposed incomplete context bindings and invalid executor envelopes; both were corrected. A deliberately unrelated publication lock failed the race test on both targets, then was restored. Final All passed 46 Dispatch, 53 Pipe and 33 foundation cases per runtime, MCP/strict July regressions and independent existing checkers; readiness/Excel/isolation also passed.

[Participant story](../tutorials/10-coordinate-requests-and-model-context.md) explains how the person used independent closure to carry constraints forward. [Agent procedure](../how-to/build-and-verify-dispatch-and-context.md) records the interfaces, ownership, commands and adapter obligations. [Fresh evidence](../../projects/revit-readiness/docs/verification/2026-10-21-dispatch-and-context.md) and [Task 4 review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-review.md) separate implementing-agent checks from pending independent review. Task 5, live Revit, desktop A8 and full R3 remain ahead.

## 2026-10-21 — Task 4 closes; connect the reader to Revit

Actual author-relayed reviewer excerpt: **“No actionable findings. Task 4 can close at `24a72cf`.”** The reviewer separately verified 46/53/33 cases per runtime, 100 admission/cancellation races and 500 concurrent rounds (4,000 requests) per runtime, prior reproducers and 29 evidence hashes. Preserve the [closure](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-4-closure.md); these are reviewer observations, not newly authored tests.

Under the existing Native authorization, the implementing agent continued Task 5 and committed the [execution model](../explanation/revit-adapter-lifetime.md) at b44ea23 before code. The model carries safety and progress across event creation/borrowing, preparation/read, publication and stop. Additional small production helpers make lifecycle, byte capture and actual net48 conversion testable without Autodesk doubles pretending to be runtime evidence.

The adapter compiles against installed Revit 2024 references and calls the existing reader. It creates no listener until Start, re-arms through Idling, invalidates context on Save As/close/reopen, hashes and parses the same bounded bytes, and uses the same execution context immediately before the read. Start/Stop commands are compiled, not installed. No new package or copied upstream source was introduced. Lock changes are local project edges only; original SDK/protocol/precedent distinctions remain.

Fresh evidence: 87 Dispatch cases on net48 and 82 on net10.0 (five actual net48 codec cases account for the difference), 53 Pipe and 33 Foundation per runtime, MCP 19 C# / ten Python / four SDK wire exchanges including strict July, plus 18 core/seven Excel/isolation tests and a clean Autodesk-reference build. The initial stubs failed; focused pre-read and final-publication mutations also failed on both runtimes, then were restored. A real-endpoint/production-host case releases networking before a held notification finishes. A wrong test-file insertion and assumed GetDocument API member caused compilation errors during development; these were corrected using the installed XML documentation, and were not counted as behavioral evidence.

[Participant lesson](../tutorials/11-connect-the-readiness-reader-to-revit.md) focuses on carrying an evidence standard across increments. [Agent procedure](../how-to/build-and-verify-revit-adapter.md) records interfaces, ownership, provenance and commands. [Verification](../../projects/revit-readiness/docs/verification/2026-10-21-revit-adapter.md) and [Task 5/CP1 packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-review.md) distinguish the pending independent review from live Revit, installation, three model cases, NC1/NC2, both desktops and R3. No live acceptance or public release is claimed.

## 2026-10-21 — Review catches an error mapping between tested components

Actual author-relayed reviewer excerpt: **“One P2 finding; Task 5 should remain open.”** The reviewer reproduced empty and oversized checklist inputs returning InternalError, while the contract requires ChecklistInvalid. Existing tests and the Autodesk-reference build passed. The [original feedback](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-checklist-review.md) remains distinct from the coder's response.

The implementing agent reran the unchanged source-linked reproduction before editing production code. It confirmed both failures. The overlooked fact was precise: InvalidDataException is not an IOException; the catalog's filter omitted it. The testing gap was also precise: snapshot rejection and codec conversion had tests, but the returned error had not been asserted through catalog, executor and queue together.

The new original net48 harness links the actual production catalog/executor/codec and references the real runtime/core, with Autodesk objects and the final reader replaced at the API boundary. Ten cases cover both operations with missing, empty, oversized, valid and exactly-four-MiB input. Six controls passed and four assertions failed before the one-line filter correction; all ten passed afterward, retaining request/session identity, retryability, null error results and zero reader invocations for rejected inputs. The unchanged reviewer reproduction passes after correction, including its held-Raise test on both runtimes. These are implementing-agent reruns, not independent closure or live Autodesk evidence.

The [participant follow-up](../tutorials/11-connect-the-readiness-reader-to-revit.md#review-follow-up-test-the-returned-error) teaches asking what the user receives, not only whether the bad input was rejected. The [agent guide](../how-to/build-and-verify-revit-adapter.md#review-follow-up-sidecar-error-handoff) records exception inheritance, the narrow mapping, source-linked tests and provenance. New tests use the reviewer's method as a learning precedent; unchanged reviewer source is retained as attributed evidence only. No package or production copied code was added. Task 5 remains open pending independent follow-up; Task 6 and live acceptance have not begun.

## Task 5 independent implementation closure — 2026-10-21

The author relayed: “No new findings. T5-01 is closed, and Task 5 passes independent implementation review at `5e9a5dd`.” The reviewer reran the unchanged reproduction, ten handoff cases, full regressions and API build, and verified 295 evidence hashes. [Original follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-5-closure.md) is separate from our disposition. The participant lesson is to name the scope of approval; the agent instruction is to preserve the boundaries and continue Task 6 without claiming installation or live acceptance.

## Task 6 — production MCP tools, 2026-10-21

Continuing the approved Native plan after the author relayed Task 5 closure, the agent added the two named production tools. The [execution/waiting extension](../explanation/bridge-execution-and-waiting.md#task-6-mcp-tool-lifetime-and-permitted-waits) was committed at a8cab35 before implementation. New tests first failed against empty stubs. One additional deadline case exposed a real classification gap; two apparent shutdown failures were traced to a test waiting for EOF after the host had already stopped. Both the failures and diagnosis are retained. A deliberate final-guard mutation fails four direct/SDK cases.

The project-reference addition also exposed a stale lock in the separate SDK client during the first full run. Locked restore correctly refused it. The remaining lock was refreshed and its external package graph compared with the prior commit; only lab Project entries changed. This is a dependency-graph propagation lesson, not a new SDK upgrade.

The [participant layer](../tutorials/12-expose-readiness-to-an-agent.md) focuses on asking for complete evidence, cancellation behavior and explicit proof boundaries. The [agent layer](../how-to/build-and-verify-mcp-readiness-tools.md) gives the concrete source, dependencies, tests and retirement contract. [Verification](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-readiness-tools.md) and the [external review packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-review.md) keep synthetic report transport separate from live Revit acceptance. No installation, client configuration or model changes.

## Task 6 independent review corrections — 2026-10-21

The author supplied two P2 findings at 189fd78 and unchanged production-executable reproductions. Existing explicit cancellation tests did not cover stdin termination; existing argument tests did not cover malformed Unicode decoding. The agent reproduced both before changes and expanded the execution/waiting model before patching. The SDK's channel completion must cancel the session before waiting for handlers.

New raw-wire tests corrected an initial July fixture missing client-capability metadata. After that fixture correction, 37 tests passed and 13 failed against unchanged production. The first implementation fixed transport cases but left three Unicode cases failing: the SDK decoded JsonNode parameters before entering the typed handler. Temporary diagnostics established the earlier boundary. An SDK message filter now records a server-owned rejection marker and removes only the undecodable value from conversion; the ordinary handler returns InvalidRequest without bridge admission. No valid token is changed and no custom protocol response is generated.

The [participant follow-up](../tutorials/12-expose-readiness-to-an-agent.md#review-follow-up-ask-what-actually-reaches-the-safeguard) focuses on asking the agent to trace real callers and retain failed attempts. The [technical follow-up](../how-to/build-and-verify-mcp-readiness-tools.md#task-6-review-follow-up-complete-argument-contract) explains SDK conversion, waiting order, test matrix and provenance. [Original reviewer feedback/dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-boundary-review.md) remain separate. These corrections do not close Task 6 independently or authorize bypassing Task 7 recovery/live acceptance.

## Task 6: move from examples to a complete argument contract — 2026-10-21

The author relayed independent closure of disconnect handling at 332934c, together with five more invalid-argument variants. They explicitly directed a full accepted/rejected handoff contract. The earlier correction had identified SDK conversion correctly but still allowed rejected values elsewhere in the tree to reach it.

The agent reproduced all five failures unchanged, wrote the [contract](../explanation/readiness-argument-boundary.md) before production changes, and generated 310 cases for both tools. Six exchanges across protocols/tracing each found 221 failures before the correction. The first complete gate left one failure per exchange because SDK JsonNode lookup was case-insensitive. Comparing the actual field name ordinally completed the schema rule. This failed attempt remains retained.

The prepared outcome carries one real ID/budget and either a validated request or safe rejection state. Accepted trees stay unchanged; rejected trees are replaced wholesale before conversion. The [participant story](../tutorials/12-expose-readiness-to-an-agent.md#when-the-reviewer-keeps-finding-variants) focuses on asking for the missing guarantee; the [agent guide](../how-to/build-and-verify-mcp-readiness-tools.md#task-6-review-follow-up-complete-argument-contract) explains interfaces, ownership and reproduction. [Fresh evidence](../../projects/revit-readiness/docs/verification/2026-10-21-mcp-argument-contract.md) records 59 Tools tests including 1,860 generated application calls, unchanged probes, full regressions and API build. Disconnect remains independently closed; validation awaits independent follow-up. No installation or live acceptance claim.

## Task 6 independent closure — 2026-10-21

The author relayed [independent closure of T6-02 and Task 6 at b4cdb1f](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-6-closure.md). T6-01 shutdown closure at 332934c stands. Beyond unchanged reproductions and supplied suites, the reviewer exercised 120 concurrent invalid calls and five valid follow-up controls, finding no actionable issues. All 500 retained evidence hashes matched at the reviewed commit.

The teaching point is how the person changed the coding agent's reasoning: ask the reviewer for the missing guarantee, require a complete accepted/rejected handoff contract, then request independent challenges beyond the named examples. The [participant story](../tutorials/12-expose-readiness-to-an-agent.md#what-allowed-us-to-close-this-step) records that engagement. The [agent guide](../how-to/build-and-verify-mcp-readiness-tools.md) preserves the concrete contract, original identity/deadline, zero rejected admission and regression obligations.

This entry records supplied independent results; no runtime suite was rerun and no production code, installation or client configuration changed to record closure. Task 7 installation/recovery is next under the approved plan and Native execution choice. Live Revit, both desktops' A8 and full R3 remain outstanding.

## Task 7 — recoverable installation, 2026-10-21 (in progress)

After Task 6 independently closed, the author said “ok, let's keep going”. The approved Native plan already named installation/recovery next. The agent committed the [ownership model](../explanation/recoverable-readiness-installation.md) at 96669dc before production code. Tests first failed against stubs, then challenged caught exceptions and abrupt process exits with original file/path snapshots as their oracle.

The work exposed a PowerShell null-conversion issue during atomic replacement; a fresh recovery/server parent ownership conflict; and an empty-directory ownership gap. Reading a partial final journal record also required discarding that uncommitted suffix before appending during recovery. Failed and corrected runs are retained. A separate test requires SDK license/notices beside the installed server; no new package or SDK source was adopted.

Read-only inspection found that the actual older fixture had no inventory. Its DLL hashes match retained required-information/Excel evidence from c7c2ad9 and 4d40788. The agent added a narrow manifest/hash-verified migration and tests that undo restores the inventory's original absence; unknown bytes still fail preflight. This applies the earlier research/evidence lesson to an actual installation decision.

The [participant layer](../tutorials/13-install-with-a-recovery-plan.md) focuses on asking what is recoverable after the first write. The [agent layer](../../projects/revit-readiness/docs/mcp-bridge-setup.md) provides exact commands, source boundaries, ownership and recovery behavior. A basic official-SDK readiness scenario was brought forward from Task 8 because Task 7 needs it for live lifecycle checks; it does not replace independent sample comparisons or desktop A8. No Task 7 closure is claimed by this progress entry.

## Task 7 — first actual installation and live Revit evidence, 2026-10-21

With the 41 deployment tests and full regressions passing at 9a93f6d, the agent installed the bridge, undid it in a new process twice, independently compared all 59 original files and directory inventories, then reinstalled and verified all 97 planned targets. Private recovery bytes were retained. Actual Revit 2024.3 loaded without starting a listener; repeated Start reused one session, Stop removed it, and restart produced a fresh session.

Twenty-six retained SDK captures include the first complete-model report, model-state/checklist errors, changed-sidecar and copied-project invalidation, close/reopen identity changes, token expiry, and a modal deadline. A fresh request after modal release succeeded. The [live record](../../projects/revit-readiness/docs/verification/2026-10-21-live-installation.md) states the exact remaining gates: it is not proof of every read-boundary cancellation, shutdown race, full sample comparison or desktop A8.

Two failed/partial attempts remain visible: an incorrect server filename failed before connection, and a failed family-open UI action left a modal dialog blocking execution, producing timeout instead of the intended family error. Each retry used a new capture after correcting/observing setup. A direct hash read of an open model was blocked by file sharing; the copy was instead matched to its retained before-open hash. The Windows automation repeatedly dismissed Save As, so the agent requested a manual Save As on the disposable copy while documenting the independent work. That check remains pending at this entry.

The participant lesson asks how to direct the agent to distinguish intended setup from observed setup and partial evidence from an operation-wide guarantee. The agent procedure records the wire/snapshot checker, negative controls, diagnostic requirements and remaining acceptance matrix. No new package, SDK source, production bridge behavior or desktop registration changed in this evidence increment. Task 7 remains open.

## Task 7 — saved model, fresh values and interruption controls, 2026-10-21

The author replied, “BAL-info-saved-as is now saved.” The saved file returned a complete zero-finding report with a new document lifetime and path. The previous token had expired during the manual step, so the agent explicitly declined to count its rejection as isolated Save As invalidation. A separate restart test did reject an unexpired token. Native editing of one wall Type Mark showed that reusing a context token reads the new value; an unsuccessful Undo attempt was exposed by a follow-up report, then the saved baseline was restored by closing without saving the test edit and reopening.

The SDK driver gained a test-only finish gate, verified RED→GREEN in subprocess tests, to retain its connection after a timeout while Revit's dialog was released. The connected three-second window contained no late wire result and completed a catalog call. Internal reader-entry proof remains pending. Protocol tests, including strict July and legacy rejection, passed without changing SDK 2.2.0 or the installed production build.

Three actual incomplete private-pipe connections closed promptly under native Stop. The first observer checked descriptor removal too early at EOF and failed; source inspection confirmed cleanup follows all workers draining. A bounded separate cleanup observation passed, while a deliberate no-Stop control failed after the ordinary five-second receive timeout. After restart, another full report passed. Native application exit removed the descriptor; fourteen source/working-file hash comparisons matched after close. No executing valid request was arranged during that normal application exit.

The installed manual command's JSON, text and four-sheet workbook matched the restored model's report, including all 18 rules. Thirty-five SDK captures now pass the independent lifecycle checker; sixteen checker tests reject corrupted evidence. The [participant story](../tutorials/13-install-with-a-recovery-plan.md) teaches asking what other explanation could produce the same result, while the [agent procedure](../../projects/revit-readiness/docs/mcp-bridge-setup.md) specifies gates, bounds and reproducible comparisons. Failed attempts remain retained. Fresh-token Save As, instrumented read-boundary cancellation, queued/executing shutdown, both desktop A8 workflows and full R3 remain open.

## Task 7 — coordinate the person and the agent, 2026-10-21

Actual author prompt: “what do you need me to do to facilitate this, rather than you trying to use ‘computer use’ to accomplish this task?” The author then replied “ready for save as test” and “saved”. We split responsibilities: the person used Revit's native Save As, while the agent prepared the new checklist, confirmed source context, tested the old token immediately and acquired the new report. No UI automation was used for this retry.

Waiting for readiness before acquiring the five-minute token resolved the earlier timing problem. This time the exact pre-save token returned `ContextChanged` 204.4 seconds before expiry. A fresh report confirmed the new path/lifetime in the same project/session, 16 examined instances, zero findings, complete coverage and false modified flags. Its content matches the previously restored sample apart from generation time and expected paths. The earlier expired attempt remains retained; the new run explicitly records its different Revit process.

Seven meaningful negative controls first failed against the new checker stub, then passed after its timing, identity, token, checklist and report assertions were implemented. All 24 checker tests and 38 SDK captures pass. The [participant story](../tutorials/13-install-with-a-recovery-plan.md#let-the-person-operate-the-application-when-that-helps) teaches how to direct this collaboration; the [agent procedure](../../projects/revit-readiness/docs/mcp-bridge-setup.md#coordinate-a-human-operated-save-as-test) supplies the sequence and offset-preserving timestamp handling. No production code, SDK, installation or desktop settings changed. Instrumented cancellation/shutdown and full Task 8/A8/R3 remain open.

## Task 7 — prepare an observable cancellation test, 2026-10-21

The author replied “closed”. The agent confirmed Revit/process-descriptor cleanup, compared the source model and checklist hashes, and recorded the new saved-model baseline. It then prepared the diagnostic increment already required by the plan: observe whether reading starts after cancellation and whether an already-started read can publish success.

The execution/waiting model was committed before implementation. A separate diagnostic build links the real add-in source with bounded file-controlled pauses and lifecycle observations; normal builds exclude the helper. Ten control tests failed against a stub, then passed on both runtimes. The full regression run, readiness/Excel/isolation checks and both API builds passed. An existing net48 notification test failed once before passing later; that observation is retained and remains undiagnosed, rather than silently erased by the green run.

Package preparation exposed two useful distinctions: unchanged source can produce different bytes because the build embeds the current Git commit, and an installed payload includes installer-owned inventory/notices that cannot simply be treated as publish output. The final package preserved the installed dependency bytes and changed only the diagnostic add-in and its inventory through the recoverable installer. All 97 installed targets and backup hashes matched independently. The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-host-diagnostics.md) distinguishes this installed-but-not-yet-run candidate from future live proof. The person still operates Revit; the agent handles the diagnostic controls. No new dependency or protocol change was introduced.

## Task 7 — diagnostic startup failure, 2026-10-21

Actual author prompt: “getting this”, accompanied by Revit's external-application failure dialog. The diagnostic initializer threw DirectoryNotFoundException reading diagnostics/run.txt in Revit PID203368. The exact selector was subsequently present with a creation time before Revit startup; the installed DLL retained its recorded hash. Invoking that same installed initializer in a new same-account .NET Framework process succeeded. These are different execution environments; the external probe is not a live Revit pass or an explanation of the failure. No application source, installed bytes, selector, permissions or model was changed. The agent requested a normal user-operated close before a controlled restart. Cause and live diagnostic acceptance remain unresolved. The [verification record](../../projects/revit-readiness/docs/verification/2026-10-21-host-diagnostics.md#first-revit-launch-failed-cause-still-under-investigation) retains evidence and the participant/agent lessons distinguish recovery from a demonstrated correction.

After the author replied “closed”, fresh comparisons confirmed normal Revit exit, no active bridge descriptor or diagnostic arm, all97 installed files unchanged, and all four model/checklist files unchanged. The agent preserved the completed journal and requested Revit startup to Home only, before loading a model or starting the bridge. No installation/configuration correction was made; the cause remains under investigation. [Close evidence](../../projects/revit-readiness/docs/verification/host-diagnostic-startup-evidence/after-normal-close.json).

## Task 7 — shared paths can name different files, 2026-10-21

The author reported the same startup error after opening Revit to Home without a model, then closed it again. Inspecting open file handles established that the diagnostic selector was physically inside Codex's MSIX LocalCache. The successful net48 child probe shared that redirected view; it was not independent of the agent's environment. Microsoft documents this AppData merging behavior. An audit of97 installed files plus selector/receipt found67 direct and32 redirected locations; the redirected set includes30 server files and the receipt. Existing byte checks remain observations within that view, not cross-desktop installation proof.

The narrow diagnostic correction moves controls to the user's .bim-agent-lab folder outside AppData. Two meaningful tests failed on both runtimes at the old location, including physical opened-file resolution. With the correction,17 diagnostic cases per runtime and both Revit API builds pass; the normal assembly excludes all three diagnostic helper classes. The original redirected run and recovery bytes are retained. Source review and additional deployment/protocol/readiness regressions precede reinstalling the diagnostic add-in; actual Revit startup remains to be observed.

The [participant story](../tutorials/13-install-with-a-recovery-plan.md) teaches asking whether applications access the same physical object, and the [agent procedure](../../projects/revit-readiness/docs/mcp-bridge-setup.md) now requires physical-path and consumer-side verification. The wider server/recovery-context issue remains explicitly open before Task8/A8. Do not move/delete the old cache, assume a child process is independent, or replay old logical-path recovery records in another environment. No SDK/package/protocol change or permission relaxation is involved.

The corrected diagnostic source7b09879 was installed with prepared recovery74e482... while Revit was closed. Preflight first stopped on an extra fixture-inventory byte change; explicit comparison established identical file/hash mappings with reordered entries. The accepted scope changed only the diagnostic DLL and two inventories. All97 targets/backups and four model/checklist files compare correctly. Installed net48 initialization succeeds and physical paths are direct for its DLL, selector and probe trace. The person is now asked to open Revit to Home again. [Installed correction evidence](../../projects/revit-readiness/docs/verification/2026-10-21-host-diagnostics.md#corrected-diagnostic-candidate-installed) keeps that probe distinct from the pending actual host result, and preserves the separate server/recovery-view deployment gap.


### 2026-10-21 — Verify the corrected startup and observe work after a timeout

Actual author reply: “ready...no errors during loading.” The agent verified Revit PID198452 initialization using the actual application journal, matching installed DLL hash and a Revit-created trace whose physical path is outside the Codex cache. The author then confirmed the disposable model was open and the bridge started. No Computer Use was involved.

Two bounded diagnostic holds exercised the unchanged production deadline/guard flow in Revit: preparation expiry caused zero model-reader entries; an already-started reader finished after release without publishing late success. Both official-SDK connections remained open for at least three seconds after release and still answered catalog requests. A fresh readiness report matched the baseline model data with both modified flags false. Native Accepted and Pending were observed. These facts do not prove physical lifetime disposal, other cancellation origins, every native event outcome or desktop A8.

The independent trace/wire checker started with sixteen failing negative controls against an accepting stub. Its first implementation expected fields absent from the actual error DTO; the positive capture exposed that mistake. After using the real error contract and context binding, all17checker tests pass, as do the24existing lifecycle-checker tests. The [technical record](../../projects/revit-readiness/docs/verification/2026-10-21-host-diagnostics.md#first-live-diagnostic-deadline-controls) preserves raw observations, the checker failure, provenance and remaining work. The [participant story](../tutorials/13-install-with-a-recovery-plan.md#ask-what-happened-after-the-timeout) explains how to ask what happened inside the application after a visible timeout. Shared-server/recovery visibility and Task7/A8/R3 remain open; no normal runtime, SDK, package, client settings or model contents changed.


### 2026-10-21 — Verify that interruptions reach the receiving application

Actual author direction: “please proceed.” Native execution continued with a verification-only SDK driver and offline production-executable/Windows-pipe tests before live use. Six unsupported-scenario tests failed first. The first implementation passed EOF but failed all three token-only cancellation variants: the local wait canceled, no notification appeared on the wire, and remote work remained active. That observation is retained without an unproven SDK root-cause claim. An explicit, correlated cancellation notification sent through official SDK APIs then passed all six driver cases. No SDK implementation was copied and no production dependency changed.

Actual Revit then passed four preparation/started-reader cancellation/EOF controls, three queued interruption controls, and injected Stop during a held read. Captures distinguish network completion, external-event disposal, hook removal, executor completion and Quiesced; they do not claim physical request-lifetime disposal or application shutdown. Fresh reports after the interruption batches matched the baseline. After Stop the descriptor was gone, Revit remained alive, and a fresh SDK call returned BridgeUnavailable. The person was asked to Start again for current-session recovery verification.

The [new technical record](../../projects/revit-readiness/docs/verification/2026-10-21-host-interruptions.md) retains exact source/provenance, raw evidence, limitations, thirty checker negative controls and the original failed cancellation stimulus. The [participant layer](../tutorials/13-install-with-a-recovery-plan.md#check-that-the-interruption-reached-revit) focuses on asking for receiving-side proof instead of trusting a local cancellation message. Tools65 and Protocol including strict July pass; installed97targets remain byte-identical. Task7, shared installation/recovery visibility, normal-build desktopA8 and R3 remain open.

## October 21, 2026 — Verify recovery, and question the observer

Actual author reply: “started.” The new Revit session returned the same report as the earlier baseline. The agent then combined existing diagnostic stages to hold one request, admit three waiting requests, and invoke Stop. Its first observer failed because it waited for hook removal while holding the API thread that the existing Idling cleanup needed. The agent inspected unchanged source and traces, preserved the failed attempt, corrected the observer waiting order and asked for one Start. The author replied “started” again. The repeated capture shows zero queued executor/reader entries, network completion while the blocker is held, and eventual API-thread hook removal. No runtime or dependency change was needed.

The [participant lesson](../tutorials/13-install-with-a-recovery-plan.md#question-the-test-as-well-as-the-plugin) explains how to hold the agent accountable for its tests. The [technical evidence](../../projects/revit-readiness/docs/verification/2026-10-21-host-stop-recovery.md) separates recovery, failed capture and corrected observation, with 31 evidence checks and five observer ownership tests. The author then reported “revit is closed.” Idle shutdown and four saved-file hashes were verified; both diagnostic receipts were restored in the original filesystem view, with all 97 targets independently matching the normal candidate. Task 7 remains open, particularly shared server/recovery visibility and remaining host observations.

## 2026-10-21 — Preserve the tested build while fixing installation visibility

After the author closed Revit, the agent restored both diagnostic installation layers using their recorded recovery data in the original filesystem view. The old MCP server still resolved inside Codex's private AppData cache. The agent rejected a preflight that would also change executable versions, prepared an immutable package from the verified normal candidate, and relocated that package to `%USERPROFILE%/.bim-agent-lab/RevitReadiness`. The public installer now uses this location by default and accepts an explicit immutable source. Old cache/recovery records remain intact; no SDK or dependency changed.

The author reopened the disposable model and started its bridge. A fresh July SDK exchange through the shared executable matched the entire prior report except its timestamp: 16 examined elements, zero findings, complete coverage, modified flags false. All 97 installed targets matched the latest receipt. Fresh Deployment 41 and Protocol checks passed before reopening Revit. [Technical evidence](../../projects/revit-readiness/docs/verification/2026-10-21-shared-server-installation.md) and the [participant story](../tutorials/13-install-with-a-recovery-plan.md#change-one-thing-at-a-time-during-setup) distinguish agent-process path inspection, SDK-to-Revit continuity and still-pending desktop acceptance. Task 7 remains open; the new shared receipt pair has not itself been undone live.

## 2026-10-21 — Reconnect-safe evidence before desktop configuration

Source inspection before registration found the production server's fixed evidence folder differed from the probe's per-launch behavior. Three actual-process tests failed before adding `--wire-evidence-root`; the full 72-case Tools suite then passed. Nine separate sessions across the three supported revisions were checked independently, and fixed-folder overwrite refusal remains intact. The settings helper now supports the readiness entry and explicit PID, with eight passing methods covering recovery and unrelated settings preservation. Protocol/strict July regressions also pass. [Evidence and limits](../../projects/revit-readiness/docs/verification/2026-10-21-desktop-launch-preparation.md).

The author asked to close Revit and stop for the night after independent work. Revit is confirmed closed. No desktop configuration has been changed; actual desktop calls remain tomorrow's work. This checkpoint records source/tests before packaging the updated external server. It does not close Task 7 or A8.

Night installation follow-up: source d88de67 was published and installed through a new prepared receipt 68dac9adc1c04f0694a12acfda4e9a6c while Revit was closed. Only external-server binaries changed; host remains normal9a93. All97targets/backups/staged bytes match. Installed SDK startup/discovery/error controls pass on allthree revisions, plus two separate session-root EOF startups. Preflight observer representation/order mistakes stopped before mutation and are recorded. Actual desktop settings remain untouched. [Next-session handoff](2026-10-21-night-checkpoint.md). Stop for the night as requested.

## 2026-10-22 — Resume actual desktop testing

The author resumed from the night checkpoint and manually opened the disposable model/started the bridge. Fresh normal Revit149972/sessiona3163353950948eabd8cfb8a4955d860 served the updated installed serverd88de67; its entire SDKJuly report matches the earlier baseline except generation time, with16examined/0findings/bothcomplete/modifiedfalse. All97installed target hashes match. Codex26.1016.6573.0 and Claude3.1623.4.0 were re-read locally; Claude updated overnight.

The agent verified direct physical config/server paths and used the tested helper to prepare both private recovery receipts before applying either readiness entry. Both entries select149972 and separate per-session evidence roots; unrelated settings compare equal. The author was asked to reload CodexMCP and quit/reopenClaude while retaining the open Revit model. Actual desktop exchanges remain pending. [Technical checkpoint](../../projects/revit-readiness/docs/verification/2026-10-22-desktop-readiness.md).

The author reported restarting Codex. The actual production tools in this desktop conversation returned a fresh context and successful report on the open disposable model. One session's raw request/response bytes establish client0.163.0-alpha.11.1 and protocol2025-06-18; all report fields equal the current-session SDKJuly baseline except GeneratedUtc. Result:16examined/0findings/bothcomplete/modifiedfalse. An independent Python evidence checker passes four methods with23negative controls; no runtime, dependency, installed-file or settings change was needed. This is initial desktop continuity, not five-case A8 or an independent BIM oracle.

The author said “claude is working right now...we'll have to wait.” The agent left Claude/Revit running and completed Codex evidence and both teaching layers. Claude's actual result remains pending; no restart or further Revit step is requested while it is working. [Actual Codex exchange and limits](../../projects/revit-readiness/docs/verification/2026-10-22-desktop-readiness.md#codex-desktop-initial-real-readiness-exchange-passes).

## 2026-10-22 — Claude results and interface provenance

The author restarted Claude and supplied a successful answer, then clarified it came from the Code tab in Claude Desktop, not terminal Claude Code. They supplied a second successful answer explicitly from Chat. Both real context/report pairs used2025-11-25 and matched the current-session SDK/Codex reports except generation time. Chat explicitly limited its pass to the16declared instances. The agent corrected an unsupported inference from wire client names: both pairs reached the same local-agent-mode server connection, while a claude-ai session remained discovery-only. Human UI attribution and passive traffic evidence are recorded separately.

The existing one-pair checker failed the new real capture, then was extended to verify every declared pair and retain legitimate roots notifications. Six methods/33negativecontrols pass, including hidden/omitted exchanges and reused-token rejection. No runtime/dependency/settings/install change. Claude model/permission choice was requested for the reproducible baseline; five-case A8 and independent oracle remain ahead. [Claude evidence and both supplied answers](../../projects/revit-readiness/docs/verification/2026-10-22-desktop-readiness.md#claude-desktop-code-and-chat-answers-with-actual-tool-traffic).

## 2026-10-22 — Test defects, incomplete coverage and stale contexts

The author asked: “Ok, let's test the deliberate defects, incomplete coverage, and stale contexts in both desktop apps. Tell me what to do in Revit and keep recording the process in both teaching layers.” The agent prepared five independent expectations from native schedule/checklist evidence and the authored answer sheet, then prepared NC1/NC2 with durable ownership records and tested interruption/cleanup. Historical returned reports are comparator test inputs, not the expected-answer source. The null-versus-empty-string distinction was corrected from fixture source rather than normalized away.

Two attempted opens selected an earlier folder with the same filename. Context paths and Revit's journal agreed; no check used either context. Pasting the full path into Revit's Open dialog resolved it. Codex, Claude Desktop Chat and a separate official-SDK July run then independently matched the wall-defect expectation: two missing fields, six affected walls, all 16 examined, both completeness flags true and modified flags false. Claude's pasted answer correctly says the scope does not pass. Native schedule observation and a post-close hash are still pending at this checkpoint, as are the other four cases and stale-context workflow.

Comparator tests pass four methods/147 negative controls; wire/context tests pass four methods/20 negative controls; control-preparation tests pass seven methods. A new context-semantic rejection test first failed six variants before its guard was added. Protocol regressions, including strict July and legacy rejection, pass. No installed code, dependency or settings change; Deployment/All was intentionally not run with Revit open. [Participant story](../tutorials/14-test-readiness-through-desktop-agents.md), [agent procedure](../how-to/verify-readiness-through-desktop-agents.md), [evidence and remaining acceptance](../../projects/revit-readiness/docs/verification/mcp-readiness.md).

The author supplied a native `BAL INFO Walls` screenshot after the wall checks. It visibly confirms the three missing Wall B fire ratings and three missing Wall D codes. The original PNG is retained with its hash. Both teaching layers now distinguish corroborating visible native values from proving model identity or full scope. The next requested action is close without saving and open the prepared ceiling-negative model; post-close wall hashing and subsequent cases remain pending.

The author reported “ceiling model open.” Codex verified the exact prepared path/checklist before reading; its full report and a separate July SDK report match the frozen three-finding/eight-affected expectation. The author was asked to run the same neutral prompt in Claude Chat; that result remains pending. The closed wall RVT's SHA-256 matches its original bytes. No model, installation or desktop settings changes occurred.

Preparation for the next human switch adds a synthetic stale-context evidence check: old-context refusal must occur before expiry, then a fresh independently verified report must come from a different document lifetime/path in the same bridge session. The initial stub failed twelve assertions; the implemented checker passes three methods/eleven negative controls. This is verification tooling, not a new bridge behavior or a claim that the live switch has happened. Both teaching layers explain why a timeout cannot substitute for proving model-switch protection.

Claude Chat's ceiling answer was supplied and its actual context30786f8/check50d1d1b verified independently on2025-11-25. Both desktop applications and July SDK now match the expected ceiling-defect report, including all eight affected identities. The answer correctly separates coverage completeness from readiness and excludes the1,196otherhost/sixlinks from its verdict. The agent requested a native ceiling schedule observation before starting the timed model switch; the model remains open. No new runtime or configuration change.

## 2026-10-22 — Verify the reason for refusal, then test incomplete coverage

All three principal reports now match the independent oracle in Codex desktop, Claude Desktop Chat and the official July SDK client. Both actual desktops also reject an unexpired ceiling context after the human activates the complete document, then recover with a fresh complete report. Claude's first attempt expired before the call; it remains an unsuccessful switch test. Another intended complete acquisition returned the still-active ceiling model. We retained that setup mismatch, opened both documents before the timer and gave an explicit Switch Windows instruction. A refusal only proves the intended guard when identity and timing evidence establish its cause.

The live error revealed a checker mistake: its synthetic error fixture had copied contractVersion from successful results. The corrected positive fixture failed, then the checker was corrected to require the production domain-error contract (code, message, requestId, retryable) separately. Real wire results remain unchanged. Three stale-checker methods/11 negative controls and both actual transition/recovery comparisons pass; this is evidence-validation work, not a runtime fix.

For NC1 the author replied, “I'm not a Revit expert...how do I execute that command in Revit?” to the agent's bare Select by ID request. The corrected instructions named Manage → Inquiry → Select by ID, the number, Show and the exact observation to report. Their screenshot shows Revit rejecting 2147483647. Separate context and matching model bytes bind that observation to the prepared control. The later confirmation was added before NC1 calls; the original ownership receipt was not rewritten.

Both desktops and the separate July client independently match NC1 (16 requested/15 examined, zero findings, coverage false/evaluator true, exact absent WA-01 scope issue) and NC2 (16/16, zero findings, both false, exact two ACT instances and three fields unexamined because no requirement matches). Claude's supplied answers correctly reject readiness in both cases and distinguish scope failure from evaluation failure. Its proposed remedies were not executed. The person closed both remaining models without saving; all five RVT and five checklist hashes match their pre-test values.

The author identified Claude Chat's AI model as Opus 5.5 and corrected the permission setting to Auto. Asking merely “which model” caused confusion with the open Revit document; name the application and setting explicitly. No production code, dependencies, installed files or desktop settings changed. Protocol regressions, including strict July and legacy rejection, pass again. Task8 reusable wrapper/workflow consolidation, remaining Task7 gaps and R3 remain open. [Participant account](../tutorials/14-test-readiness-through-desktop-agents.md), [agent procedure](../how-to/verify-readiness-through-desktop-agents.md), [full evidence matrix](../../projects/revit-readiness/docs/verification/mcp-readiness.md).


## 2026-10-22 — First follow-up only: make capture repeatable

The author said, “ok, let's take these one at a time, then we can refine things.” We limited this increment to the planned reusable SDK capture runner and its two teaching layers. The original Task8 ruling stands: case metadata/comparison stay in Python and PowerShell; the official SDK driver and production bridge remain unchanged. No new dependencies or copied SDK source.

The runner freezes expected answers before launch, validates the SDK-acquired context before releasing its existing pause gate, retains logs and failed attempts, and publishes success only after SDK transport completion and the full independent comparison. A single capture budget extends through publication. The offline tests exercise the actual process/file orchestration with a labelled synthetic producer; they are not live Revit proof.

Tests first failed for the missing runner. Additional tests exposed and corrected three handoff errors before the checkpoint: the comparison stage initially could publish after the capture deadline; Windows PowerShell evaluated the default SDK path before PSScriptRoot was available; and a cleanup exception could skip the primary failure record. The real command now reaches the installed server and correctly fails with BridgeUnavailable while Revit is closed, preserving evidence and issuing no readiness read. The earlier prelaunch failure is retained.

Ten focused tests, the full MCP/bridge suite (including strict July), 18 readiness/seven Excel/process-isolation checks and all retained live comparisons pass. Four existing platform-analysis warnings in unchanged C# source remain recorded. No installed file, desktop setting or sample changed. This completes the first offline implementation increment; a fresh positive wrapper run is still unverified. We stop here for the author's refinement rather than move to the next work item. [Runner evidence](../../projects/revit-readiness/docs/verification/2026-10-22-readiness-runner.md), [participant story](../tutorials/14-test-readiness-through-desktop-agents.md), [technical procedure](../how-to/verify-readiness-through-desktop-agents.md#run-one-prepared-case-through-the-sdk).


## 2026-10-22 — Give participants and agents a starting point

The author requested: “Ok, let's do the participant starting guide and agent handoff next. We'll refine things after we've worked through the remaining steps.” This corrects the earlier interpretation that each increment should pause for refinement. Continue the authorized sequence; no extra approval gate is implied.

The [participant starting guide](../start-here.md) focuses on directing the agent: give it the repo, ask for research and Git/source inspection, prepare a workstation and disposable sample, judge scoped results, and define one extension for independent review. Its reusable prompts are explicitly suggested prompts, not invented quotations. It carries the lesson that repeated major findings may expose a shared missing reasoning model.

The [agent handoff](../how-to/agent-handoff.md) links the exact technical procedures: tested versions, source/SDK provenance, locked builds, recoverable installation/settings, independent sample expectations, desktop and SDK evidence, cancellation/validation boundaries and unfinished acceptance. It separates repository source from installed binaries and historical PID/session bindings. The runner's offline success is not recast as a positive live run.

Entry-point READMEs and prerequisite summaries still described MCP as planned; they now reflect the recorded live desktop results and remaining gaps. Chronological lessons and evidence are retained. Ruling: use unnumbered onboarding guides instead of another tutorial 07 or duplicate bridge walkthrough; existing lessons 08–14 already tell that story. Root/project guidance, documentation navigation, script index and status point to the same pair. This is documentation work only, with no runtime, dependency, installation, client-settings or model changes; Tasks 7–9 and R3 remain open.

Documentation verification passed on the 14 changed/new Markdown files: UTF-8 decoding, 605 relative link targets, 53 section anchors, navigation to both layers from six entry/guidance files, source/script paths, pinned SDK/package declarations, recorded Git checkpoints and `git diff --check`. The first link check found an obsolete Task 6 journal anchor; it now points to the complete-argument-contract section. These checks validate documentation, not fresh runtime behavior. No runtime suite, installation or live application test was rerun for this documentation-only change.

## 2026-10-22 — Prepare shutdown and recovery before the next UI step

Actual author direction: “Let's move on to the remaining shutdown and recovery checks. Prepare what you can with Revit closed, and tell me when you need me to open it. Keep recording the process in both teaching layers.” Process inspection confirmed Revit closed. The agent continued Native execution, retaining the existing source/dependency/desktop policy and preparing the next short human-led Revit session.

Investigation reproduced the retained intermittent notification failure: queued work could physically retire before endpoint owner cancellation, so the old test required a callback after its borrower had gone away. The correction separates completed queued work from a live executing borrower. It does not change cancellation behavior. New runtime observation reports physical lifetime disposal after cleanup/permit release. Diagnostic-only, bounded lifecycle holds and an SDK observer prepare an actual queued-work application-close test, without pretending that two operations execute simultaneously on Revit's API thread.

Full offline bridge/protocol regressions, focused Dispatch/checker tests, diagnostic/API builds and readiness/Excel/isolation passed. A read-only audit verified 97 targets and three shared recovery layers, but did not execute their undo. Token-only SDK cancellation remains unexplained. No new live Revit result is claimed. The agent is preparing a host-only diagnostic installation/restore round trip; exact execution will be appended to the record after verification.

Participant learning and exact agent procedure link to the same [preparation record](../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md). Failed reproductions, wrong-working-directory attempts and a checker syntax mistake remain distinct from behavioral RED/GREEN evidence. Human UI actions come after observer and recovery preparation; broader Task 7/R3 closure remains pending.
Shutdown follow-up: source/preparation committed at `32c44ec`. With Revit closed, actual three-target diagnostic installation, separate-process restore, repeat restore and reapply all passed with 97-target verification. The agent preserved the unchanged desktop server, prepared the shared selector and verified 99 actual opened-file paths before requesting human startup. The author opened the saved-as sample and started the bridge.

The first positive live capture-wrapper run passed against independently prepared native/checklist expectations. An initial preparation assertion rejected the legitimate saved-as filename change before any MCP query; the corrected binding compares the entire checklist after that explicit filename substitution and freezes its actual hash. Expected BIM values still come from the original oracle, not the live response.

After observer readiness, the author closed Revit normally. Actual ApplicationClosing and OnShutdown events admitted and retired three queued requests without executor/reader entry. Network shutdown, descriptor removal, native event/hook cleanup and all three physical lifetime disposals completed before process exit. The independent checker passed; no safety-timeout release or forced termination was used. This is queued-work shutdown evidence; no concurrent reader/native Raise overlap claim follows.

The second diagnostic receipt and the selector were restored. Revit then restarted with the normal host. Two early context calls returned BridgeUnavailable before a bridge session existed; the author confirmed “ok, I started the rediness bridge,” and a new session appeared. The fresh normal-host wrapper check passed 16/16, zero findings, both completeness flags true and no model modifications. Saved model/checklist hashes remained identical. Revit remains open; desktop settings were unchanged during this SDK-only session.

The [record and both teaching layers](../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md) retain the original failures, actual restore receipts, raw live exchanges and remaining limits. Both diagnostic receipts are Restored; historical whole-install undo, token-only SDK cancellation, remaining native shutdown limits and external R3 stay open. All 153 retained file hashes and replayed shutdown/two full-report comparisons are checked before the evidence checkpoint.

## October 22 — Diagnose cancellation at both ends of the connection

The author said **“yes proceed to diagnose”** after the token-only SDK cancellation observation was identified as the next remaining item. The agent reproduced it outside Revit using official C# SDK 2.2.0, an owned production server and a real Windows pipe with a held synthetic executor. On all three protocol revisions, token cancellation stopped the local wait but sent no notification and left the receiving guard active. A subsequent explicit SDK notification invalidated the same request before its original deadline; the connection remained usable.

Inspection of the pinned SDK and matched .NET 10.0.13 source, plus a reduced ordering experiment, explains the failure: canceling the separate wait can unwind the SDK method and dispose its earlier notification callback before it runs. The inspected upstream test asserts only local cancellation, which cannot establish remote invalidation. Current main/v2.3.0 source still has the same ordering; no new package was installed or runtime-tested.

The agent retained a failing propagation probe and a separate known-failure characterization. Green characterization means the limitation is reproduced, not repaired. The Python comparison checks wire prefixes, identities, deadlines, recovery and natural cleanup; seven altered-evidence controls verify that it rejects misleading captures. Fresh Tools72 and Protocol/strict-July checks pass. No production code, package, installed host, client setting or model changed. The limitation is diagnosed and must travel into R3; desktop Stop behavior and remaining host/recovery acceptance are separate.

The [participant account](../tutorials/13-install-with-a-recovery-plan.md#diagnose-the-dependency-before-changing-the-plugin) focuses on asking the agent to show both sides of an operation. The [technical diagnosis](../../projects/revit-readiness/docs/verification/2026-10-22-sdk-cancellation-diagnosis.md) retains the reproducible commands, source provenance, earlier harness mistakes and acceptance limits.


## October 22 — Continue with explicit cancellation and preserve active work

The author asked whether the SDK defect blocked further work, then said **“follow your recommendations.”** The agent recorded the decision to continue using the verified explicit notification mechanism, retain the failing token-only probe and carry the limitation into R3. No SDK fork, upgrade or acceptance waiver was introduced. The desktop apps' Stop buttons remain a separate unverified behavior.

The next recovery preparation verified all 97 installed files, the backups and staged bytes for three historical receipts, 83 immutable package source files, reverse lineage and 100 direct opened-file paths. Revit was closed, but five installed readiness servers were still active. The agent prepared the exact package and baseline snapshots before asking the person to disconnect clients. When the author said Claude could not stop now, the agent left both applications running; resetting Codex alone would not release Claude's server files. Actual undo/reinstall remains pending.

The [participant story](../tutorials/13-install-with-a-recovery-plan.md#continue-with-a-tested-alternative), [decision record](../superpowers/reviews/revit-mcp-bridge/2026-10-22-cancellation-disposition.md) and [technical recovery preparation](../../projects/revit-readiness/docs/verification/2026-10-22-shared-recovery-preparation.md) distinguish useful independent preparation from a completed restore. No installed file, recovery receipt state, client configuration or model was changed.

## October 22 — Execute the three historical recovery layers

The author reported Claude closed and Codex readiness MCP disabled. Fresh inspection found no Revit or installed readiness-server process. The agent revalidated 97 baseline targets/snapshots, 83 package files, three receipt/backup sets and 100 direct paths, then ran the existing undo script in six separate PowerShell processes: undo/repeat for each layer in verified reverse order.

Independent comparisons passed all 97 target states after each process, inventories and expected directory absence. A fresh durable Prepared record preceded reinstall through the existing module. New receipt ef71fb608bba47f49a43ee75fb226bec is Committed; old68dac9/2ccfb37/a88dbf are Restored. All 97 final bytes match baseline, 164 new backup/staged files verify and 98 final paths resolve directly. Host9a93f6d/serverd88de67 are unchanged; current build outputs were not deployed. No dependency/product-code/client-setting change.

The auxiliary plan-summary capture serialized dictionary fields as null; its original bytes remain, and the independently checked immutable installer receipt supplies the actual plan. Installed-server SDK checks on all three revisions passed discovery and expected BridgeUnavailable with correlated wire evidence. The broader Protocol run timed out in the driver existing-release rejection test; the unchanged focused test passed in 2.297 seconds, and the full repeat passed including strict July/legacy rejection. The initial failure remains unresolved, not silently treated as fixed. Fresh Revit startup/read was requested; it remains separate from file recovery. [Execution record and both teaching layers](../../projects/revit-readiness/docs/verification/2026-10-22-shared-recovery-execution.md).

## October 22 — Verify the recovered build in a fresh Revit session

Author replied **“ready.”** Revit178492 published session705886aad28f45cf9b22df62da642aae. A newly frozen expectation from the original authored oracle/native schedules preceded the SDK query. The supported wrapper checked model/checklist identity, then the independent full report/wire comparison passed16/16,0findings,bothcomplete,no unexamined cases and modifiedfalse before/after on protocol2026-07-28. Saved input bytes and all97installed files remained unchanged; new receipt backup/staged bytes verified.

Both desktop configs still selected149972. Two private durable backups preceded PID-only edits149972→178492; full parsed settings and exact bytes outside the quoted PID were preserved, including Codex enabled=false. Claude was not launched and no new desktop call is claimed. Both configuration paths resolve directly. The human can reconnect normally. The [recovery record](../../projects/revit-readiness/docs/verification/2026-10-22-shared-recovery-execution.md#fresh-revit-recovery-passes) links27new exact evidence files; earlier66file records remain intact. No product/package/install change this follow-up. Host recovery closes this sequence; native limits, diagnosed SDK defect and consolidated R3 remain separate.


## October 22 — Consolidate proof for independent R3

Actual author instruction: “Ok, let's consolidate the acceptance evidence and prepare the R3 review packet. Clearly identify what passed and what remains unverified, and record the process in both teaching layers.” The agent prepared the [R3 packet](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) and [A1–A9 ledger](../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md), without sending it to a reviewer or changing requirements.

Implementation head is 627e227; installed host 9a93f6d/server d88de67 and diagnostic 32c44ec remain separate identities. With Revit closed, fresh All/readiness/Excel/isolation/locked build passed; four CA1416 platform warnings are retained. A dated NuGet advisory query listed no vulnerable MCP packages. Read-only replay verified 1,838 hash entries across 21 manifests, 15 case/client comparisons, both unexpired desktop switch/recovery cases, initial desktop continuity and the host/recovery/SDK evidence. This is saved-evidence replay, not another live run.

Consolidation made gaps explicit: the current normal build has not had the affected live tests; the SDK token-only cancellation defect remains; desktop Stop and some native/access/instance scenarios are unverified; Codex's exact AI model/permission baseline was not retained. The earlier driver timeout and malformed auxiliary recovery-plan capture remain visible. Independent full R3 and author disposition are pending. No production/dependency/install/settings changes occurred; existing servers were left alone.

The [participant story](../tutorials/15-prepare-an-evidence-based-review.md) explains how the author asked the agent to make its claims assessable. The [technical procedure](../how-to/prepare-r3-evidence.md) supplies exact scope, replay commands, evidence ownership and review boundaries. The packet's later Git commit identifies this documentation/evidence handoff, not a newly tested live binary.


## October 22 — Respond to independent R3 without extending the claims

Actual author message: **“code review:”**, with the returned independent report for packet0489c54. The [original report and separate implementer dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md) preserve the recommendation to keep acceptance open. Technical compatibility, offline checks and recorded installed-candidate computations remain supported. The reviewer independently reproduced the known SDK client defect; it found no new production-code defect.

R3-01 identified missing Codex participant-facing NC1/NC2 answers despite correct retained tool results. The agent verified the gap, qualified the matrix/A8 claims and recovered the original NC1 commentary through public session records with exact request linkage. It did not treat NC2's later summary of Claude or a broad completion claim as equivalent evidence. R3-01, historical Codex baseline and current-source live acceptance remain open. Original case captures/manifests are unchanged.

Both teaching layers now carry the reviewer's shared concept: proof must reach the last consumer. The presentation gate requires actual prompt/answer, request/candidate linkage, baseline and semantic assessment for both applications. Fresh current-candidate A8 is the next coordinated evidence step, not an excuse to reconstruct historical metadata. This response made no production, dependency, installation, settings or application-process changes; no external follow-up or author approval is invented.

## 2026-10-22 — Freeze the normal candidate before repeating acceptance

The author supplied independent follow-up on `9c27d50`: “The recovered answer satisfies the historical NC1 presentation portion of R3-01.” The same feedback kept NC2, Codex baseline and current-build acceptance open and directed us to freeze the actual candidate before testing. It requested no architecture change or cancellation rewrite. See the [preserved follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-follow-up.md).

The agent continued the approved Native sequence. With Revit closed, it rebuilt the normal candidate from clean commit `9c27d50b5ce2f7763093abf2cbb2db0916ce05d6`, froze an 84-file package, identified the source inputs and verified the prior installation's 97 targets and 164 recovery files. Read-only whole-install preflight identifies 17 changes. The two active readiness servers were left running, and the author was asked to release them normally before installation.

Fresh full offline suites, strict July/legacy rejection, readiness/Excel/isolation and three-protocol frozen-server discovery/error controls passed. Four known CA1416 warnings remain. The five expectations were prepared independently; no new Revit or desktop case has run. All original 53 follow-up files remain unchanged. The [49-file preparation snapshot and candidate record](../../projects/revit-readiness/docs/verification/2026-10-22-r3-current-candidate.md) distinguish package freeze, future installed freeze and live acceptance.

Participant lesson: direct the agent to hold the tested version fixed, prepare recovery and retain each application's actual answer. Agent lesson: source/installed identity, data/presentation and diagnostic/normal timing each require distinct evidence. Both [teaching](../tutorials/15-prepare-an-evidence-based-review.md#freeze-the-example-you-are-testing) [layers](../how-to/prepare-r3-evidence.md#freeze-the-current-normal-candidate) now record this. Installation awaits released connections; full acceptance remains open.


## 2026-10-23 - Prepare the printed packet and private teaching remote

The author redirected work to the printed seminar handout, specified **US Letter, black-and-white friendly, about eight pages**, then asked for a remote repository that could later be made public and disseminated. The agent created private `bringfire/bim-agent-lab`, inspected the tracked history before upload, and prepared the same current checkpoint as remote main and the continuing feature branch without rewriting local history. Public access and the original-material license remain separate pending decisions.

The [participant PDF](../../output/pdf/swacca-2026-participant-handout.pdf) preserves two one-hour sessions and emphasizes prompts, research, Git, the two beginner scripting routes, readiness coverage, dependency choice and independent review. It includes the missing-concept lesson from repeated review findings, a real repository link/QR and writing space. It labels the packet as a draft and pyRevit as optional; newly discussed scripting demos have not been rehearsed. The five-case table states independently prepared expectations, not current-candidate acceptance.

The agent retained editable content and a local ReportLab generator, corrected initial pagination, rendered and visually inspected all eight Letter pages, and checked text bounds, embedded fonts and links. No package was installed and no Revit/model/client setting or frozen candidate changed. The [participant account](../tutorials/16-prepare-the-participant-packet.md) and [technical print/rebuild guide](../../workshops/swacca-2026/handout/README.md) link the source records and verification. Physical printing/scanning, scripting rehearsal and final public distribution remain separate checks; full R3 remains open.


## 2026-10-23 - Supply an editable Word handout for organizer branding

The author said the packet was good, requested removing Draft, confirmed October 29-30 and asked for a Word document formatted like the PDF for the organizer's marketing team. The agent changed only the shared footer text, retained the approved content and added a python-docx generator. Native editable paragraphs/tables/headers/footers and a QR image preserve the eight-page design. Updated PDF and DOCX use an `oct29-30` filename suffix; the earlier PDF was open in a viewer and remains the historical copy.

The packaged renderer could not find LibreOffice; the agent used installed Word in an owned hidden instance to export for visual inspection, without installing software. The initial Word render exposed template residue and merged note rules, corrected in the generator. The current [verification record](../../workshops/swacca-2026/handout/verification-word-handoff.json) identifies files, checks and remaining physical-print limitations. Both teaching layers describe the editable handoff. No external message, public visibility change, plugin/candidate mutation or new acceptance claim occurred.

## 2026-10-26 - Select MIT and prepare the repository introduction

The author reported that the handouts had been sent and the organizers liked them. After the agent checked GitHub and confirmed the repository remained private, the author directed: **"License is MIT, and let's create a Readme"**.

The agent added the root MIT LICENSE for original lab code and teaching materials, using the project-contributor copyright notice and the standard OSI text. It revised the existing README to lead with the teaching purpose, participant/agent entry points, clone instructions, an example first prompt, the readiness/Excel/MCP behavior, compatibility limits and MIT/third-party boundaries. Current licensing, sharing, contributor and agent guidance were synchronized; earlier dated journal entries remain historical records.

The [participant story](../tutorials/16-prepare-the-participant-packet.md#choose-the-license-and-make-the-readme-useful) explains how the owner directed the decision; the [agent procedure](../reference/licensing.md#agent-procedure-for-keeping-the-grant-accurate) covers the grant, notices and publication checks. This documentation change leaves the distributed handouts, installed software, frozen candidate and acceptance status unchanged. Public visibility, history/distribution review and release packaging remain separate outstanding work.

Verification: all 45 local link targets in the revised README and added/changed Markdown lines resolved, including the new teaching anchors. The MIT grant matched GitHub's standard MIT template after whitespace normalization; the project copyright notice was checked separately. Diff whitespace checks passed. GitHub still reported PRIVATE, and both remote branches matched the starting checkpoint before the documentation update. No software build or runtime test was needed or claimed for these documentation-only changes.

## 2026-10-26 - Review the public distribution before changing visibility

The author directed: **“Ok, let's do the public-distribution review and make the repo public once it's ready. Keep recording the process in both teaching layers.”** This supplied publication authorization conditional on readiness. The agent inspected all 79 reachable commits at `9a4ac92`, decoded document/compressed evidence, reviewed screenshots and archived planning, checked upstream notices and inspected GitHub's other content surfaces.

The [review record](../reference/public-distribution-review-2026-10-26.md) preserves the scanner's actual findings and bounded dispositions: expired local read contexts and file hashes, not account credentials. It also records byte-versus-text distinctions in upstream evidence. The agent added a notice to the planning archive because its intended version/capability claims exceed the verified implementation. Original evidence, supplied PDF/Word files, source binaries and installed applications were left intact. The agent prepared [participant](../tutorials/17-review-before-publication.md) and [agent](../how-to/review-and-publish-a-repository.md) accounts and a reusable read-only history inventory helper.

During review, the author said **“we will add revit 2026 tomorrow.”** This is recorded as the next planned addition for October 27, not current support. The publication scope is a teaching-source preview; packaged distribution and open R3 acceptance are unchanged. Visibility change and anonymous verification follow the reviewed checkpoint and will be recorded below.

Publication completed October 26 Eastern at reviewed commit `77397bb`, with both remote branches updated and MIT recognized by GitHub. Unauthenticated API/raw access and a full HTTPS clone verified the public state and source identity. The first clone downloaded successfully but checkout hit Windows path limits in the deep audit folder; a new clone with `core.longpaths=true` passed. The README and both teaching layers now record that option. A working-copy LICENSE byte assertion was corrected to distinguish Git-object identity from native newline conversion; the clean checkout and normalized text agree. The [publication receipt](../reference/public-distribution/publication-2026-10-26.json) preserves these results and limitations. Current visibility guidance was updated; the delivered handouts and historical evidence were not changed. The final documentation checkpoint is a follow-up to the inspected publication commit, not a new implementation release.

## 2026-10-26 - Correct the README actually shown by GitHub

The author supplied a screenshot of the online README showing the old hosting placeholder. The agent reproduced it: GitHub selected `.github/README.md`, which takes precedence over the root participant README. Previous checks verified public access, root-file contents and cloning, but missed this presentation boundary.

The agent renamed the internal note to `.github/hosting-and-automation.md`, corrected its obsolete hosting status and added selected-README/rendered-page checks to the [agent publication procedure](../how-to/review-and-publish-a-repository.md). The [participant story](../tutorials/17-review-before-publication.md) records how the author's screenshot exposed that missing verification. The root introduction and implementation are unchanged. The [review follow-up](../reference/public-distribution-review-2026-10-26.md#landing-page-correction-after-author-feedback) links the before/after evidence rather than rewriting the earlier raw-file result as if it had covered presentation.
