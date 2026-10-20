# How we directed the agent: working with Git

**For seminar participants.** Learn when to ask an agent to obtain source, preserve existing work, and record a reviewable checkpoint. The [agent Git guide](../how-to/work-with-git.md) carries the commands and repository checks; the [clone procedure](../how-to/clone-research-repositories.md) covers research copies.

This story began during [research](00-research-before-building.md), before BIM Agent Lab existed. Its first local checkpoint is `3ca7a87`. The [build journal](../build-story/journal.md), [checkout evidence](../research/evidence/local-checkouts-2026-10-20.json), and [repository setup record](../build-story/repository-setup.md) distinguish the earlier inspections from the start of our own Git history.

## Ask the agent to inspect the implementation

The author's actual direction was:

> One thing that participants need to know pretty much from the start is what it means to direct an agent in regards to working with git. I am going to ask you to clone the repos that you found, so that you can inspect the code for the researched resources. Participants will need to know how to do this...meaning "that they should do this"

The key instruction is knowing **that you should ask for a source inspection**. Participants do not need to memorize the clone command. A project page can identify a candidate; a local copy lets the agent follow related files and explain what the implementation actually does.

The agent obtained seven research repositories, recorded their exact revisions, and checked their state against saved evidence. It located the code behind a current-view query and found a default limit of 100 elements. That finding mattered to our BIM question: a partial list cannot silently stand in for a complete model inventory.

## Understand the operations you are directing

These are **suggested instructions**, not quotations from the original conversation:

| Concept | What the participant is asking for | Example direction |
|---|---|---|
| Repository | A project with tracked files and history | Identify which repository we are working in and inspect its current state. |
| Clone | A local copy of an existing repository | Obtain this source so you can inspect the relevant implementation. |
| Branch | A named line of development | Use an appropriate branch for this change and preserve existing work. |
| Diff | The actual changes between versions | Explain what changed and what effect it has. |
| Commit | A local checkpoint of selected changes | Review and verify this step, then record it with an explanatory message. |
| Remote and push | A destination and an operation that transfers commits there | When I ask to publish, verify the intended repository and branch first. |
| Fork | A hosted copy maintained separately | Assess whether we need our own fork when adapting this project. |
| Tag or commit ID | A reference to a particular version | Record the exact version inspected or tested. |

Knowing these intentions helps the person control what is being changed, saved, and shared. The agent handles command syntax and reports the result.

## Give our own work a history too

Cloning someone else's repository did not put the surrounding workshop folder under version control. The author wanted a lasting teaching resource, chose the broader name BIM Agent Lab, and then gave this **actual implementation instruction**:

> Ok, let's create bim-agent-lab with that structure, bring over the research and teaching materials we've developed, and initialize Git so we can record the build process from here. Do this in the D: drive

The human selected the purpose, name, location, and scope of the migration. The agent created the structure, adapted navigation, recorded where imported material came from, configured exclusions, and checked the staged baseline before committing. Reference repositories and working models stayed outside the tracked teaching material. No remote was created or publication performed.

The first migration script failed before writing files. The agent corrected the method and checked the resulting files and import hashes. The lesson is to ask what was verified, rather than accepting a statement that a copy operation was attempted.

## Ask for a checkpoint you can understand

The setup record reports 50 staged files, 125 working local Markdown links, verified import provenance, and preserved archival evidence at the initial checkpoint. Those are historical checks of the teaching baseline, not tests of a plugin. Later commits record the setup proof, readiness rule, and Excel feature with their corresponding evidence.

For a seminar demonstration, ask the agent to show one cited source file from a research copy, explain its consequence for our design, then show a checkpoint and diff from our own repo. This connects Git to a useful decision without turning the session into a command tutorial.

A **suggested prompt for future work** is:

> Inspect the current repository, branch, and changes. Preserve existing work. Use an appropriate branch for the agreed task. Show me the resulting changes and relevant verification evidence, then record a reviewed local checkpoint and report its commit. Keep research copies, credentials, generated files, and working models out of Git. Publishing will be a separate instruction.

For practice, ask your agent to explain the difference between the initial teaching baseline and the first working Revit command, using their actual commits. It should show what each checkpoint proves and what it leaves unverified. The [technical guide](../how-to/work-with-git.md) provides the inspection commands.
