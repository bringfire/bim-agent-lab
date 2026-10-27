# Revit 2026.5 compatibility: first working host

Status: implementation authorized October 27 by the author: get the plugins working in Revit 2026.5 on this laptop, keep Revit 2024 working, and update participant setup instructions as verification proceeds. This does not authorize a release or establish support. Read the [workstation intake](../../../projects/revit-readiness/docs/verification/2026-10-27-summit-workstation-intake.md) for actual observations.

## Purpose and scope

The author's October 27 clarification puts a working existing plugin in Revit 2026 ahead of choosing a summit demonstration. Whether to extend MCP live or explain the process interactively will be assessed afterward, with time as the constraint. An attendee package and a new MCP tool are outside this first pass.

Agent-selected immediate target: installed **Revit 2026.5, build 26.5.10.38, .NET 10, Windows x64**. Preserve the existing Revit 2024/net48 source and build route. Do not claim compatibility with earlier .NET 8 releases of Revit 2026 from a 2026.5 result.

## Approaches

The practical first pass is a version-specific modern build of the existing plugins, with the same deterministic readiness computation, existing external MCP server and isolated Excel exporter. This preserves the existing teaching workflow and gives the earliest useful live proof.

Supporting every 2026 point release immediately would add a .NET 8 matrix and more deployment/testing work before this machine's demo works. Replacing 2024 entirely would discard an explicit project requirement and the verified teaching baseline. Neither fits the stated priority. The agent recommends the narrow 2026.5 addition; that target boundary is a proposal for approval, not an earlier author instruction.

## Implementation boundaries

- Supply the existing pinned SDK `10.0.204` in an isolated local tool directory if available from Microsoft's official release metadata; use it explicitly for builds. Keep the committed pin and existing SDK installation unchanged. Check checksum and selected SDK before execution. Verify an available Python interpreter for evidence scripts.
- Add explicit year/framework selection to the probe, readiness and optional fixture projects and their build scripts. Defaults continue to mean 2024/net48; the modern build references the installed 2026.5 API and a compatible Windows .NET 10 target. Keep output and intermediate paths distinct so one year cannot silently consume the other's binaries.
- Multi-target the existing core for the required host runtimes and add modern JSON implementations in the core and add-in codec. Preserve legacy code behavior. Keep identities, enums, literal strings, exact false flags, input limits and invalid-checklist classification. Reuse the modern serializer already used by bridge libraries without adding an unnecessary production package.
- Keep the bridge contract/runtime and official MCP SDK package pins. Compile actual adapter source against the modern API. Resolve proven API differences locally; do not rewrite scheduling/cancellation as part of migration.
- Extend probe and recoverable readiness deployment to select the correct build output and `Addins/2026` destination. Preserve receipts/undo, preflight, file-hash verification, unrelated add-ins, original samples and cross-application path checks. Separate year-specific host payloads; preserve ownership checks around any shared server files. No installer runs while Revit is open.
- Keep the net48 Excel helper in a separate process, with its dependency configuration. Prove the new host invokes it and receives the same workbook/report values.
- Prepare disposable version-specific fixtures from the installed sample and retain independent model/checklist expectations. Do not upgrade or modify original 2024 samples in place.

## Acceptance in order

1. Selected SDK is correct; locked restores/builds and existing relevant suites pass on this machine. New tests cover framework/year selection, correct payload destination, rollback and modern JSON semantic parity at the changed handoffs.
2. Minimal probe loads in actual Revit 2026.5 and returns independently checked wall/ceiling counts without changing the model. A successful build alone is insufficient.
3. Existing readiness command returns correct JSON/text and Excel on a known fixture; the exported values match independent expectations. No Autodesk API DLLs ship in plugin outputs.
4. Bridge Start/Stop works in the actual host. Existing context/readiness tools return complete and defect results, refuse stale document contexts and preserve incomplete-coverage flags. Verify through an SDK client first, then the actual Codex desktop and Claude Desktop Chat, with client/model/permission metadata. Do not relabel SDK traffic as desktop proof.
5. Record the frozen source/package identity, installed hashes, actual results and outstanding acceptance limits. Revisit the live-demo choice only after these observations; a bounded successful first-host checkpoint may precede the full desktop matrix.

## Failure and recovery

Stop a dependent stage on a failed prerequisite or host test. Preserve failed evidence. Restore only owned installation changes through the documented receipt mechanism with Revit closed. Do not stop user applications, overwrite active server files or change unrelated client configuration to make a test pass. Coordinate user-operated Revit steps when required. Keep model query read-only checks distinct from deliberately editing disposable fixtures.

Historical R3 gaps remain open until fresh relevant evidence closes them; the SDK token-only cancellation defect is retained as a known limitation. Native outcomes not observed remain unverified. This pass neither expands BIM rule scope nor implies public release acceptance.

## Deliverables

Version-specific source/build/install changes with meaningful tests, a Revit 2026.5 verification record, and updates to current guidance, the build journal and both teaching layers. A focused implementation plan follows approval of this design, using the previously selected Native execution method.
