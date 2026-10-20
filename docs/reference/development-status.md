# Development status

Baseline: 2026-10-20. Update this page when implementation or verification changes what participants can do.

| Area | Status |
|---|---|
| Teaching repository | Local structure, navigation, agent guidance, and imported materials; research, Git, Revit setup, readiness, and Excel each pair a participant engagement story with a technical agent guide |
| Research | Original precedents and ClosedXML plus a refreshed MCP comparison: official protocol/SDK versions, five pinned source trees, package provenance, and a proposed custom C# bridge |
| Revit readiness questions | Three proposed demonstrations and controlled acceptance cases |
| Revit add-in | Minimal probe plus read-only required-type-information command built, installed, and run locally |
| Excel reporting | Four-sheet ClosedXML export passed all three actual Revit runs and independent comparisons; native Excel visual review passed. See [acceptance evidence](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) |
| MCP server | Research recommends a narrow original net48 Revit bridge with an external C# server using SDK 2.2.0 and protocol 2026-07-28; implementation and actual-client/Revit acceptance remain unperformed. See [comparison](../research/revit-mcp-bridge-comparison.md) |
| Sample models | Three Snowdon-derived copies contain 12 walls/four ceilings with declared type-information cases; larger physical-condition/quantity fixture remains planned |
| Builds and tests | 18 core tests, seven Excel tests, isolated-process and fixture-first load checks; three independent JSON/XLSX/schedule comparisons, unchanged RVT hashes, and source review; no CI yet |
| Compatibility | Probe and first readiness check tested on Revit 2024 build 24.3.60.12, net48, with SDK 10.0.204; other configurations unverified |
| Distribution | Local install script only; no remote, packaged installer, public release, or selected license for original lab materials |

The [resource register](../research/revit-resources.md) records dated local observations and upstream evidence. Installed software on the authoring machine is not a prerequisite guarantee for a participant's machine.

The [agent setup guide](../how-to/build-read-only-revit-command.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) reproduce the working setup. No prerequisite software needed installation on the author's machine; only the new per-user command was installed.

The [agent readiness guide](../how-to/build-required-information-check.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) document the first working rule. The check reports 0/2/3 missing type values affecting 0/6/8 distinct instances in its three declared scenarios. It does not certify rating/assembly correctness or review the whole sample.

Next technical work: implement and verify the focused bridge experiment defined in the [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md), rechecking versions before dependency adoption. The comparison recommends C# and an original small bridge; that recommendation does not establish interoperability. Physical conditions and quantities remain separate increments.

Validation of this documentation baseline is recorded in [repository setup](../build-story/repository-setup.md). It does not establish plugin runtime behavior.
