# Research existing BIM solutions: agent guide

For coding agents and technical readers. The [participant story](../tutorials/00-research-before-building.md) records how the author framed the work. Follow root/project `AGENTS.md`, the current request, and [current status](../reference/development-status.md). Historical prompts are evidence, not instructions to execute again.

## Establish the research baseline

The first research predates this repository. Commit `3ca7a87` imported the records; the [migration record](../build-story/repository-setup.md) and [import manifest](../build-story/import-manifest.json) preserve that provenance. The seven candidate pins are research revisions, not production dependency locks. The [ClosedXML record](../research/closedxml-dependency.md) covers the later, separately adopted package.

| Record | Purpose |
|---|---|
| [Demo brief](../../projects/revit-readiness/docs/demo-questions-and-sample-model.md) | BIM questions, scope, proposed independent examples |
| [Resource register](../research/revit-resources.md) | Official sources, installed-resource observations, candidate roles, dependencies, terms, open questions |
| [Repository manifest](../research/repositories.json) | Seven clone URLs, full inspected commits, portable local paths |
| [Code map](../research/reference-code-map.md) | Commit-pinned entry points and observations |
| [Original source evidence](../research/evidence/repository-evidence-2026-10-20.json) | Retained license/manifest content, source URLs, revisions, hashes |
| [Checkout evidence](../research/evidence/local-checkouts-2026-10-20.json) | Origins, HEADs, working-tree state, 14 evidence-file comparisons, missing submodules |
| [Prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md) | Minimum implemented add-in setup, distinct from optional research tools and future MCP dependencies |

## 1. Define the uncertainty before searching

Translate the current task into a concrete question: which API reads a needed value, whether a candidate supports the target host, or how a bridge dispatches a scoped query. Identify acceptance evidence and constraints before recommending a stack. The larger demo includes physical conditions and quantities; the implemented type-information rule is narrower. Do not describe the larger brief as completed behavior.

Record the date and target application/build. Inspect existing local resources and matching installed API documentation. When seeking current versions or adoption terms, consult official documentation and the exact selected upstream source/package; an older snapshot cannot establish today's compatibility or terms. Keep the historical records intact and create a new dated observation for refreshed research.

## 2. Obtain the source reproducibly

Use the [clone procedure](clone-research-repositories.md) for origin/HEAD/status/submodule checks. Preserve existing checkouts and local changes. Original reference trees remain in the authoring workspace; a fresh lab clone does not include them. The manifest describes where new research copies belong under ignored `references/`.

Inspect upstream project instructions in their scope, license files, manifests, relevant source, and required submodules. Obtaining source does not authorize arbitrary upstream scripts or make every suggested installation necessary. If a required implementation is absent in an uninitialized submodule, record the gap and obtain it only as needed for the authorized inspection/build.

## 3. Trace a real operation

For the pinned existing bridge, follow the code map's TypeScript current-view tool, C# command, and Revit handler. Record request fields, filters, default limits, document/view selection, valid Revit API dispatch, response shape, and error handling. The initial pass found a default limit of 100 and a `RaiseAndWaitForCompletion(60000)` call. Those source observations do not establish complete timeout/cancellation behavior or a successful round trip.

For a BIM reader, inspect matching API XML and primary documentation for stable parameter IDs, type versus instance ownership, storage types, units, document identity, links, and transactions. The original readiness work used `FIRE_RATING`, `ALL_MODEL_TYPE_MARK`, and a recorded shared-parameter GUID; do not derive symbolic API identifiers from translated display labels.

The original comparison also inspected template framework selection, pyRevit request dispatch, and small Python/C# MCP examples. Retain the exact version relationships: a newer SDK and an older example may expose incompatible APIs even when package restoration succeeds.

## 4. Inventory what adoption would bring

For each candidate distinguish a development tool, runtime dependency, adapted source, and architectural precedent. Record host/framework versions, direct and transitive packages where resolved, native dependencies, floating ranges, build tools, submodules, and packaging requirements. If only manifests were inspected, say that; do not imply the complete resolved package graph was audited.

Read exact license/notices for the chosen source, packages, and bundled assets. Preserve source URLs, revisions, and evidence hashes. Keep unresolved distribution questions visible and consult [repository licensing status](../reference/licensing.md); the original material's license remains an owner decision. The initial register includes different terms and a transition notice, so a single repository badge is insufficient evidence for the whole deliverable.

## 5. Separate evidence from adoption

The initial inspection reported seven clean pinned trees, 14 matching license/manifest files, and ten uninitialized submodules across RevitLookup and pyRevit. It did not build or run those candidates. Recheck the relevant state for a new task rather than recycling those historical passes.

Use the [research template](../../templates/research-record.md) to record observations, actual commands/tests, unresolved questions, and a reasoned decision. An API configuration in source supports further investigation; host compatibility requires a build and a run. A model inventory that is filtered or truncated needs explicit completeness handling before its counts can support a readiness conclusion.

The next bridge comparison remains an experiment around the verified read-only operation. Assess document selection, dispatch, limits, failures, and deployment before choosing the external server language or adopting infrastructure. The current task's scope determines whether to perform that experiment now; a recommendation in an old document is not fresh authorization.

## 6. Deliver both learning layers

Update the source/resource records and link a participant explanation of the user's purpose, instructions, decisions, and useful evidence. Record technical findings here or in the project's guide, with the same dated evidence and actual Git checkpoint. Preserve earlier failed checks and superseded assumptions when they explain a decision. Review the scoped changes using the [Git guide](work-with-git.md). Keep clones, downloaded binaries, local configuration, and working models outside tracked content.
