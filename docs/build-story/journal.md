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

Current next action: inspect the sample in Revit and evaluate the existing bridge's minimum Revit 2024 read workflow. The teaching repository baseline is established; the working demo brief provides the acceptance target for technical investigations.
