# Development status

Baseline: 2026-10-20. Update this page when implementation or verification changes what participants can do.

| Area | Status |
|---|---|
| Teaching repository | Local structure, navigation, agent guidance, and imported materials |
| Research | Seven repositories inspected at pinned commits; license and dependency evidence retained |
| Revit readiness questions | Three proposed demonstrations and controlled acceptance cases |
| Revit add-in | Minimal read-only command built, registered, and run locally; readiness rules remain planned |
| MCP server | Not implemented; language and bridge selection still open |
| Sample models | Controlled four-wall fixture created and checked; larger readiness fixture and Autodesk sample inspection remain pending |
| Builds and tests | Repeatable PowerShell check/build/install scripts and manual host acceptance; no automated test runner or CI yet |
| Compatibility | Probe tested on Revit 2024 build 24.3.60.12, net48, with SDK 10.0.204; other configurations unverified |
| Distribution | Local install script only; no remote, packaged installer, public release, or selected license for original lab materials |

The [resource register](../research/revit-resources.md) records dated local observations and upstream evidence. Installed software on the authoring machine is not a prerequisite guarantee for a participant's machine.

The [first-build tutorial](../tutorials/02-build-a-read-only-revit-command.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) reproduce the working setup. No prerequisite software needed installation on the author's machine; only the new per-user command was installed.

Next technical work: inspect a working copy of the readiness sample and establish one deterministic check. Evaluate the existing bridge candidate against a minimal custom bridge before choosing the external server language and dependencies.

Validation of this documentation baseline is recorded in [repository setup](../build-story/repository-setup.md). It does not establish plugin runtime behavior.
