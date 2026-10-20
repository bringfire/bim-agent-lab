# Establishing the teaching repository

Date: 2026-10-20.

## Author's instruction

> Ok, let's create bim-agent-lab with that structure, bring over the research and teaching materials we've developed, and initialize Git so we can record the build process from here. Do this in the D: drive

## Scope and implementation

The author approved the name and structure in the preceding discussion. The repository is created at `D:\bim-agent-lab` with a `main` branch. It begins with the teaching baseline and a Revit readiness project. This setup does not implement the plugin, select its external server language, or publish a remote repository.

The initial authoring folder is retained at `D:\SWACCA_BIM`. These are migration provenance paths; a participant can clone the lab to a different location. Repository links are relative, and source-code links point to the inspected upstream commits.

## Imported materials

Twelve files were imported: the Git lesson, resource register, source map, earlier domain research, build journal, demo brief, raw workshop notes, organizer HTML and PDF, original project context, and two evidence snapshots. The [import manifest](import-manifest.json) records original and baseline destination hashes and the source-to-destination map.

Evidence JSON, HTML, and PDF are preserved byte-for-byte. Markdown is adapted for its new location, with historical context marked and current links repaired. Reference working trees, local session files, SDK binaries, and sample models are not imported. A [portable reference manifest](../research/repositories.json) and [clone guide](../how-to/clone-research-repositories.md) support future source inspection.

The README offers three paths: use the plugin, learn the build process, and extend it with an agent. Current status explicitly identifies implementation work still to come. Shared AGENTS.md guidance is supplemented by Revit project guidance; templates preserve the reusable teaching process.

## Verification record

The checks below passed against the staged baseline before the initial commit:

| Check | Result |
|---|---|
| Repository contents | 50 files staged; shared teaching materials and Revit project structure present |
| Relative Markdown links | All 125 file targets resolve |
| Structured records | All four JSON files parse |
| Import provenance | All 12 original source hashes and final imported hashes verified, including staged Git blobs |
| Exact archival copies | Both evidence snapshots, the organizer HTML, and the PDF match the originals byte-for-byte |
| Reference provenance | Seven manifest commits match the evidence; 12 code-map links point to files verified at those commits |
| Ignore behavior | Eight local/generated/model paths excluded; three intended source/example paths remain trackable |
| Repository boundaries | No nested repositories, bundled model files, or application binaries |
| Git review | Staged whitespace check clean; no unstaged changes at the check; branch `main`; no remote |

Git attributes preserve archived evidence bytes, including their original Windows line endings. New documentation uses LF line endings. This avoids changing historical evidence merely to normalize text formatting.

No plugin build or Revit runtime tests apply yet because this baseline has no application implementation. The initial commit can be located with `git rev-list --max-parents=0 HEAD`; its reviewed contents can be inspected with `git show`.

## Teaching observation

The first migration attempt used an inline script and failed with a syntax error before any files were written. The agent reran it from a saved script, then checked the resulting files. Success is based on those checks, not on an intention to copy the materials.

The first Git checkpoint imports earlier work honestly. The journal records the preceding research; the repository history begins here. Hosting and a license for original material remain release decisions.
