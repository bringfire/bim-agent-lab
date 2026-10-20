# Development status

Baseline: 2026-10-20. Update this page when implementation or verification changes what participants can do.

| Area | Status |
|---|---|
| Teaching repository | Local structure, navigation, agent guidance, and imported materials |
| Research | Seven original precedents plus ClosedXML inspected at pinned commits; license and dependency evidence retained |
| Revit readiness questions | Three proposed demonstrations and controlled acceptance cases |
| Revit add-in | Minimal probe plus read-only required-type-information command built, installed, and run locally |
| Excel reporting | Four-sheet ClosedXML export passed all three actual Revit runs and independent comparisons; native Excel visual review passed. See [acceptance evidence](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) |
| MCP server | Not implemented; language and bridge selection still open |
| Sample models | Three Snowdon-derived copies contain 12 walls/four ceilings with declared type-information cases; larger physical-condition/quantity fixture remains planned |
| Builds and tests | 18 core tests, seven Excel tests, isolated-process and fixture-first load checks; three independent JSON/XLSX/schedule comparisons, unchanged RVT hashes, and source review; no CI yet |
| Compatibility | Probe and first readiness check tested on Revit 2024 build 24.3.60.12, net48, with SDK 10.0.204; other configurations unverified |
| Distribution | Local install script only; no remote, packaged installer, public release, or selected license for original lab materials |

The [resource register](../research/revit-resources.md) records dated local observations and upstream evidence. Installed software on the authoring machine is not a prerequisite guarantee for a participant's machine.

The [first-build tutorial](../tutorials/02-build-a-read-only-revit-command.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) reproduce the working setup. No prerequisite software needed installation on the author's machine; only the new per-user command was installed.

The [readiness tutorial](../tutorials/03-check-required-type-information.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) document the first working rule. The check reports 0/2/3 missing type values affecting 0/6/8 distinct instances in its three declared scenarios. It does not certify rating/assembly correctness or review the whole sample.

Next technical work: evaluate the existing bridge candidate against a minimal custom bridge around the verified read-only operation before choosing the external server language and dependencies. Physical conditions and quantities remain separate increments.

Validation of this documentation baseline is recorded in [repository setup](../build-story/repository-setup.md). It does not establish plugin runtime behavior.
