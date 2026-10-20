# Development status

Baseline: 2026-10-20. Update this page when implementation or verification changes what participants can do.

| Area | Status |
|---|---|
| Teaching repository | Local structure, navigation, agent guidance, and imported materials |
| Research | Seven repositories inspected at pinned commits; license and dependency evidence retained |
| Revit readiness questions | Three proposed demonstrations and controlled acceptance cases |
| Revit add-in and MCP server | Not implemented or built in this repository |
| Sample models | Installed samples identified on the author's machine; no model opened or modified during this work |
| Builds and tests | No plugin build system, test runner, or CI workflow yet |
| Compatibility | Revit 2024 is the required first target; no tested support matrix yet |
| Distribution | No remote, installer, public release, or selected license for original lab materials |

The [resource register](../research/revit-resources.md) records dated local observations and upstream evidence. Installed software on the authoring machine is not a prerequisite guarantee for a participant's machine.

Next technical work: inspect a working copy of the sample, test the smallest Revit 2024 read operation using the existing bridge candidate, and compare it with a minimal custom bridge. Choose the server language and adopted dependencies from that evidence.

Validation of this documentation baseline is recorded in [repository setup](../build-story/repository-setup.md). It does not establish plugin runtime behavior.
