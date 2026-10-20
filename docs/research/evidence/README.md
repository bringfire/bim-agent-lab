# Research evidence

The [MCP version/source observation](mcp-version-observations-2026-10-20.json) was collected during the later bridge comparison. It records official release and registry metadata, inspected package manifests/notices and hashes, five clean source checkouts, and 29 source-file hashes. It establishes research provenance, not a successful dependency restore, build, or Revit integration. See the [comparison](../revit-mcp-bridge-comparison.md).

## Original imported snapshots

These two snapshots were imported byte-for-byte from the original authoring folder:

- [Repository evidence](repository-evidence-2026-10-20.json): full inspected license texts and selected dependency manifests, with source URLs, revisions, and content hashes.
- [Local checkout observations](local-checkouts-2026-10-20.json): the origin, HEAD, cleanliness, evidence comparisons, and submodule state observed during the original source inspection.

The local-path fields describe the original research layout. They do not assert that a new clone of BIM Agent Lab contains those repositories or that their state has been checked again. Use the [current manifest](../repositories.json) and [clone guide](../../how-to/clone-research-repositories.md) to obtain fresh reference copies.

Research pins identify the evidence reviewed; they are not a dependency lockfile for an implementation. Preserve these historical files and create a new dated record when refreshing the evidence.
