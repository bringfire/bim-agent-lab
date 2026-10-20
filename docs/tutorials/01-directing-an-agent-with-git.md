# Directing an agent with Git

Participants should learn early that they can ask an agent to obtain existing source code, inspect it, and maintain a reviewable history of their own work. Git belongs in the first research and setup steps of an AI-assisted project.

The immediate habit is: **when research identifies a serious candidate repository, ask the agent to clone it and inspect the relevant implementation.** Reading a project page can identify a candidate; having the source locally lets the agent trace behavior, search related files, and compare versions.

## The first demonstration

Give the agent the repository URLs, a local destination, and the purpose of the inspection:

> Clone these repositories into a references folder. Keep them as clean research copies. Record each source URL and the exact commit inspected. Read the project instructions, license, dependency files, and code relevant to our Revit 2024 readiness tool. Explain what we can reuse, what is missing, and what still needs a build or runtime test. Keep this pass to source inspection.

The agent should return the local paths, recorded revisions, a map of relevant code, and a recommendation supported by that code.

During the initial research on 2026-10-20, seven repositories were cloned in the authoring workspace at recorded commits. At inspection, their working trees were clean and 14 license/manifest files matched the saved evidence by SHA-256 hash. Those are historical observations; the clones are not bundled with BIM Agent Lab. Use the [clone guide](../how-to/clone-research-repositories.md) to obtain your own copies. See the [source map](../research/reference-code-map.md) and [inspection snapshot](../research/evidence/local-checkouts-2026-10-20.json).

Reference checkouts are separate Git repositories. BIM Agent Lab ignores the root `references/` directory so it can hold local research copies without tracking other projects' repositories.

## The concepts participants need

| Concept | Meaning | A useful instruction to the agent |
|---|---|---|
| Repository | A project with tracked files and change history | “Identify the repository we are working in and inspect its current state.” |
| Clone | Create a local Git copy of an existing repository | “Clone this repository so you can inspect the implementation.” |
| Commit | A recorded checkpoint of selected changes | “Review the changes, run the relevant checks, and commit this completed step with an explanatory message.” |
| Branch | A named line of development | “Create a branch for this feature before implementing it.” |
| Diff | The actual changes between versions or working states | “Show me the staged and unstaged changes and explain their effect.” |
| Remote and push | The remote is a configured repository location; pushing transfers commits to it | “Confirm the destination and branch, then push the reviewed work to our repository.” |
| Fork | A hosted copy under another account or organization | “Create our own fork when we decide to maintain an adaptation.” |
| Tag or commit ID | A reference to a particular version | “Record the exact version we tested so the workshop can reproduce it.” |

A clone creates local files and an `origin` remote. A commit records local history. Publishing is a separate operation. [Git clone documentation](https://git-scm.com/docs/git-clone), [recording changes](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository).

Participants can express these intentions in ordinary language while the agent handles command syntax. They should still understand which project is being changed, what is being saved, and where work is being shared.

## Our own project needs Git early too

Cloning reference repositories does not put a surrounding folder under version control. BIM Agent Lab has its own Git history, beginning with the imported teaching materials and repository structure. The journal preserves the earlier research that predates this history.

When beginning our own project, use a prompt such as:

> Check whether this folder is already a Git repository. Preserve any existing work. Set up local version control if needed, exclude reference checkouts, local configuration, secrets, and generated files, and record a baseline of the project notes. Report what is tracked and whether a remote is configured.

Ask the agent to checkpoint useful decisions and working increments. A checkpoint can contain a design document, sample preparation recipe, source code, or installation instructions. Review should establish what the checkpoint contains and how it was verified.

For a later feature:

> Inspect the current Git state, create a branch for the agreed change, implement it, and show the resulting diff and verification evidence. Commit the completed change. Report the branch and commit so we can return to this version.

Creating a branch and inspecting a fixed revision are distinct operations. A fixed inspection checkout may be in “detached HEAD” state, meaning Git is positioned directly at a commit; create a branch when beginning development there. [Git switch documentation](https://git-scm.com/docs/git-switch).

## What we should show on stage

Spend a few minutes on a real sequence:

1. Identify an existing repository during research.
2. Ask the agent to clone it and record the version.
3. Ask it to locate the code that handles a Revit query.
4. Open the cited source and explain what that evidence changes in our design.
5. Show a checkpoint and diff in our own project using `git log` and `git show`.

One example is enough for the live explanation; the takeaway can contain the full reference list. Avoid making attendees memorize Git commands before they have seen why the operations help.

The first source pass already supplies a concrete lesson: the existing MCP bridge's current-view tool defaults to a 100-element limit. Our readiness report must define scope and completeness explicitly. Finding an existing query does not establish that it answers our entire business question.

## Details for the takeaway

Our clone commands used a partial-clone filter, `--filter=blob:none`, and then checked out the recorded commits. This downloads file contents as needed, including the files in the selected working tree. The basic operation participants need to recognize remains `git clone`.

Some repositories reference additional repositories through Git submodules. RevitLookup and pyRevit declare ten submodules in total; these were identified but not fetched during the initial source inspection. A build preparation step must determine which submodules and packages it needs. A successful clone is evidence that source was obtained, not that the project builds or runs.

Keep the source license and provenance when adopting code. Cloning a public repository is a research action; the license review determines how particular code and assets may be reused and distributed.
