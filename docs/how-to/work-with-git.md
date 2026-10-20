# Work with Git in BIM Agent Lab: agent guide

For coding agents and technical readers. The [participant story](../tutorials/01-directing-an-agent-with-git.md) explains the author's intentions and useful vocabulary. Follow current user instructions and root/project `AGENTS.md`; examples below do not direct a reset, new repository, or publication in an existing checkout.

## 1. Inspect the correct repository

Run from the intended working folder:

```powershell
git rev-parse --show-toplevel
git status --short --branch
git log --oneline -5
git remote -v
git diff --stat
git diff --cached --stat
```

Inspect staged and unstaged changes before editing. Preserve unrelated work and identify the appropriate existing branch or create a task-specific branch when needed. Never use a reset or blanket clean to obtain an apparently clean starting point. A read-only inspection does not need a new branch.

The author requested `D:\bim-agent-lab`, but a participant's clone can live elsewhere. Do not use the original authoring folder `D:\SWACCA_BIM` as a current target merely because a historical record mentions it. The [setup record](../build-story/repository-setup.md) and [import manifest](../build-story/import-manifest.json) explain those provenance paths.

## 2. Keep research copies separate

Use the [clone guide](clone-research-repositories.md) and [research manifest](../research/repositories.json) for source candidates. Each clone has its own history and `origin`; cloning one does not initialize the surrounding folder as a repository. The root `references/` directory is ignored. Preserve existing origins and modifications; do not reset a research checkout to a pin without accounting for its current state.

The original commands used `--filter=blob:none` and a detached commit checkout. A partial clone fetches contents as needed, including the selected working tree. Detached HEAD is suitable for a fixed source inspection. Deliberate source adaptation needs an appropriate development branch or separate maintained project. A hosted fork and a local clone are different operations; neither automatically authorizes copying code into the lab.

The initial seven clones were clean, with 14 license/manifest files matching retained hashes. RevitLookup and pyRevit declared ten uninitialized submodules. Those observations are in the [dated evidence](../research/evidence/local-checkouts-2026-10-20.json). Restore/build/runtime work remained separate; recheck whatever a new task needs.

## 3. Initialize only a new, authorized project

The original baseline was created after the author requested a new teaching repository. For a genuinely new folder, first confirm it is not already within another repository and inventory its files. Only then initialize local history:

```powershell
git init -b main
git config --get user.name
git config --get user.email
```

Use the user's existing identity or obtain missing identity information; do not invent an author or change global settings as a convenience. Define exclusions before staging. The lab's [.gitignore](../../.gitignore) keeps clones, local configuration, secrets, generated outputs, and working BIM files outside tracked content. [.gitattributes](../../.gitattributes) preserves archival and notice bytes where required.

Use `git check-ignore -v -- <path>` to inspect an intended exclusion and `git ls-files` to inspect tracked files. Ignoring a path does not remove already tracked content. Do not remove unrelated files from the index as part of an ordinary setup check.

For a migration, retain source-to-destination provenance and hashes, repair relative links, and verify byte-preserved evidence. The original migration's 50 staged files and 125 resolving Markdown links describe `3ca7a87`, not the current repo. Earlier research predates that commit; do not manufacture an earlier Git history.

## 4. Review and checkpoint a coherent increment

Use an appropriate branch under the current instructions. When creating one, `git switch -c <task-branch>` creates a branch from the current commit; inspect the starting commit first. Do not blindly recreate historical branch names from a lesson.

After the change, run checks appropriate to its scope: source/build/host checks for implementation, link/provenance/status checks for documentation. Stage explicit reviewed paths using `git add -- <paths>`, then inspect what the commit will actually contain:

```powershell
git diff --check
git diff --cached --check
git diff --cached --stat
git diff --cached
```

Check new files as well as tracked diffs. Include source, tests, teaching material, independent expected answers, and permitted evidence. Exclude downloaded dependencies, Autodesk binaries, working RVTs, generated builds, credentials, and local sidecars. Review licensed/adapted material and preserve required notices.

When a local checkpoint is authorized, commit with a message describing the completed change, then inspect `git status --short --branch` and `git log -1 --oneline`. A pre-existing unrelated change may properly remain; do not claim a clean tree unless verified. A commit records the files, not a Revit installation or a model backup.

## 5. Inspect real milestones

| Commit | What it records |
|---|---|
| `3ca7a87` | Teaching baseline and imported research |
| `88ac2df` | Development prerequisites before the first build |
| `f025fce` | Built and host-verified read-only probe |
| `c7c2ad9` | First required-type-information check |
| `4d40788` | ClosedXML reporting and its acceptance evidence |

Read-only examples:

```powershell
git show --stat 3ca7a87
git show --stat f025fce
git diff 88ac2df f025fce -- projects/revit-readiness
```

A source diff explains the change; its linked dated evidence establishes what was tested. Do not equate a commit's existence with a passing build or host result. Historical commits can be inspected without checking them out over current work.

## 6. Preserve the teaching record and publication boundary

Record how the human framed the task in the [participant layer](../tutorials/01-directing-an-agent-with-git.md); keep commands, source state, review, and results in the technical layer and journal. Use the [lesson template](../../templates/lesson.md), labeling actual versus suggested prompts accurately.

The initial repo had no remote; inspect present configuration instead of assuming it is unchanged. A clone usually creates an `origin`, while local initialization does not. A local commit does not push it. Follow the user's intended destination, branch, and publication instruction; do not infer sharing authorization from an old teaching prompt. The [licensing status](../reference/licensing.md) and release work also remain relevant before distributing the lab.
