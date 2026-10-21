# Development status

Baseline: 2026-10-20. Update this page when implementation or verification changes what participants can do.

| Area | Status |
|---|---|
| Teaching repository | Local structure, navigation, agent guidance, and imported materials; research, Git, Revit setup, readiness, and Excel each pair a participant engagement story with a technical agent guide |
| Research | Original precedents and ClosedXML plus a refreshed MCP comparison: official protocol/SDK versions, five pinned source trees, package provenance, and a proposed custom C# bridge |
| Revit readiness questions | Three proposed demonstrations and controlled acceptance cases |
| Revit add-in | Minimal probe plus read-only required-type-information command built, installed, and run locally |
| Excel reporting | Four-sheet ClosedXML export passed all three actual Revit runs and independent comparisons; native Excel visual review passed. See [acceptance evidence](../../projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) |
| MCP server | Author selected a narrow original net48 Revit bridge with an external C# server using SDK 2.2.0/protocol 2026-07-28. [Spec revision 2](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md) requires Codex desktop and Claude Desktop plus incomplete-coverage controls; the host/probe now passes offline tests, but [CP0](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md) fails before tool discovery in both actual desktops; Revit connection work remains unstarted |
| Design and external review | External [R1 feedback/dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-feedback.md) retained: two major findings and one minor accepted and addressed in spec revision 2. [Two teaching layers](../tutorials/06-direct-design-and-external-review.md) updated. [External follow-up and author approval](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r1-follow-up.md) close R1 at `0ef8cda`. [R2 feedback](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-feedback.md) found three major gaps and one minor. [Plan revision 2](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) adds cross-cutting lifetime/ownership, interface and interruption-test corrections; [external follow-up](../superpowers/reviews/revit-mcp-bridge/2026-10-20-r2-follow-up.md) closes all four findings at plan level on `cfbfe04`; author approved revision 2 and selected Native execution; [approval record](../superpowers/reviews/revit-mcp-bridge/2026-10-20-plan-approval.md) |
| Sample models | Three Snowdon-derived copies contain 12 walls/four ceilings with declared type-information cases; larger physical-condition/quantity fixture remains planned |
| Builds and tests | 18 core tests, seven Excel tests, isolated-process and fixture-first load checks; three independent JSON/XLSX/schedule comparisons, unchanged RVT hashes, and source review; MCP adds 14 C# behavioral tests, five Python test methods and SDK/wire comparison; no CI yet |
| Compatibility | Probe and first readiness check tested on Revit 2024 build 24.3.60.12, net48, with SDK 10.0.204; other configurations unverified |
| Distribution | Local install script only; no remote, packaged installer, public release, or selected license for original lab materials |

The [resource register](../research/revit-resources.md) records dated local observations and upstream evidence. Installed software on the authoring machine is not a prerequisite guarantee for a participant's machine.

The [agent setup guide](../how-to/build-read-only-revit-command.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-read-only-probe.md) reproduce the working setup. No prerequisite software needed installation on the author's machine; only the new per-user command was installed.

The [agent readiness guide](../how-to/build-required-information-check.md) and [verification record](../../projects/revit-readiness/docs/verification/2026-10-20-required-information.md) document the first working rule. The check reports 0/2/3 missing type values affecting 0/6/8 distinct instances in its three declared scenarios. It does not certify rating/assembly correctness or review the whole sample.

Next step: resolve the failed CP0 requirement. Both installed desktop clients sent older initialize handshakes; no success/error tool calls were reached. Retain strict 2026-07-28 and test supporting client versions, or obtain an explicit protocol-contract revision and new compatibility evidence. Native execution is selected and author plan approval is recorded. Task 2 remains unstarted; later live tests still require all three samples, NC1/NC2 and both clients. [Dependency review](../research/mcp-sdk-dependencies.md) records actual package/source use. Physical conditions and quantities remain separate increments.

Validation of this documentation baseline is recorded in [repository setup](../build-story/repository-setup.md). It does not establish plugin runtime behavior.

Protocol follow-up: [update/settings research](../research/mcp-desktop-protocol-investigation.md) separates SDK compatibility from the strict server/acceptance policy. No eligible Codex update or newer public Claude Windows download was found. Codex's experimental feature still produced the old handshake after both MCP reload and the author's reported full app restart. Temporary settings were removed, preserving unrelated changes. Six isolated SDK comparisons confirm that SDK 2.2.0's default accepts all three tested revisions. The agent recommends retaining that SDK while explicitly revising client acceptance to allow verified compatible exchanges. This is a recommendation; it does not satisfy CP0 or change the approved contract.
