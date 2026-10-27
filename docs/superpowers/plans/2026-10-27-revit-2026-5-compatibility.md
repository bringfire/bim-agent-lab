# Revit 2026.5 Compatibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans for the author's already selected Native execution. Steps use checkbox syntax for tracking.

**Goal:** Run the existing probe, readiness/Excel and bridge workflows in installed Revit 2026.5 while retaining the 2024 build and installation route.

**Architecture:** Share deterministic code across net48 and net10.0. Select the host/API/output/install paths by explicit Revit year, defaulting to 2024. Retain the external net10.0 MCP server and isolated net48 Excel helper with existing package pins and recoverable deployment.

**Tech Stack:** C#, pinned SDK 10.0.204, Revit 2024/net48 and installed 2026.5/net10.0-windows APIs, PowerShell 7, existing locked NuGet packages and Python verification.

**Spec:** `docs/superpowers/specs/2026-10-27-revit-2026-5-compatibility-design.md`.

## Global Constraints

- Immediate modern host: Revit 2026.5 build 26.5.10.38, Windows x64. Earlier 2026/net8 is outside this pass.
- Keep the 2024/net48 source and default route, SDK 10.0.204 pin, package pins, notices and separate Excel process.
- No model edits by readiness queries. Deliberate fixture preparation operates on disposable local copies only.
- No install/undo/Deployment/All with Revit open; preserve recovery before mutation and all unrelated files/settings.
- A successful build does not establish host/client acceptance. Keep historical R3 limits visible.

## Review Focus

- Wrong-year outputs or stale intermediates must not cross into another host's installation.
- Modern JSON must preserve literal date-like strings, Unicode, null/false flags, missing identities and report lists.
- Invalid/oversized/trailing checklist JSON must fail before reading Revit data; valid at-limit input must remain valid.
- 2026 installation/undo must preserve the 2024 installation and keep per-year recovery/server ownership separate.
- Separate helper runtimes must not cause spreadsheet/Autodesk/MCP dependency leakage into the wrong process.

### Task 1: Modern core and payload conversion

**Files:** core csproj/Checklist.cs; add-in ReadinessPayloadCodec.cs; new `tests/compatibility-tests/` dual-target console harness; `scripts/Test-RevitCompatibility.ps1`.

**Interfaces:** Preserve `JsonFile.Read<T>(string)`, `Write(string,object)`, `ReadinessPayloadCodec.ParseChecklist(string)` and `SerializeReport(InformationReport)`. Core gains net10.0 alongside net48; codec uses existing DataContract typed conversion with runtime-specific syntax checking/serialization.

- [ ] Add dual-target behavioral tests for actual core/codec JSON, false flags, Unicode/date-like values, missing identities, trailing JSON and valid/oversized input. Run modern target first: expect incompatibility with net48-only core.
- [ ] Add modern framework/JSON path without changing legacy behavior. Run the same harness on both targets; compare semantic report/checklist results and existing readiness/Excel checks.
- [ ] Commit the passing focused change and record evidence in the ledger.

### Task 2: Year-specific compilation

**Files:** project-level Directory.Build.props/targets or equivalent early properties; probe/readiness/fixture csproj; environment and Build-Probe/Build-Readiness/Build-McpBridge scripts; compatibility script/tests.

**Interfaces:** Add `[ValidateSet('2024','2026')][string]$RevitYear='2024'` to public build/environment scripts. Derive the default API directory and selected host framework from that year; preserve explicit API path overrides. Separate modern outputs and intermediates, leaving existing 2024 output locations compatible with the test/install contract.

- [ ] Add failing checks for 2026 environment selection, rejected wrong API major/runtime, and distinct probe/readiness/fixture output/intermediate paths.
- [ ] Implement selection and compile against real installed 2026.5 APIs. Resolve only demonstrated API differences. Keep Autodesk DLLs out of outputs.
- [ ] Build both years, including opt-in fixture tools and unchanged helper/server, and run serializer/adapter regressions. Commit and ledger actual results.

### Task 3: Year-specific recoverable installation

**Files:** Install-Probe.ps1, Install-Readiness.ps1, McpDeployment.psm1; compatibility/deployment tests; setup documentation.

**Interfaces:** Add `RevitYear='2024'` to installer/plan selection with matching source payload and destination. Keep old module callers working. Use a year-specific default runtime/recovery root for 2026, preserving the 2024 default. Allow explicit add-in root when actual consuming paths require it.

- [ ] Add disposable-root tests that choose modern DLLs, preserve a sentinel 2024 tree, verify apply/undo hashes, and reject a wrong-year target before mutation.
- [ ] Implement installer selection and retain ownership/preflight/receipt/lease behavior. Prove the selected DLL framework/API binding before live install.
- [ ] Run focused new tests and existing Deployment tests with Revit closed. Freeze source/candidate identity, install with receipts and compare installed hashes. Commit implementation/evidence separately from live acceptance.

### Task 4: Actual host and participant verification

**Files:** new dated 2026.5 verification record, participant checklist/prerequisites/current status, both teaching layers and journal.

- [ ] Launch actual Revit 2026.5; coordinate any user-only sign-in/trust step. Run the probe on a known project and retain counts/read-only evidence.
- [ ] Prepare disposable 2026 fixtures; run readiness JSON/text/Excel and compare independent expected results. Exercise bridge Start/Stop and SDK context/check calls before desktop configuration.
- [ ] Verify actual desktop workflows and record current metadata, incomplete cases, stale context behavior and remaining limitations. Prepare user-operated steps when authentication/client reload prevents autonomous continuation.
- [ ] Rebuild/check 2024 and verify it remains usable without modifying original 2024 samples. Update participant requirements only to the observed scope. Run focused independent review before claiming completion; preserve any unverified live gates.

## Execution state

Use the ignored ledger `.superpowers/sdd/2026-10-27-revit-2026-5/progress.md`. The prior setup checkpoint is 4ee766b; its passing component and cache-only build evidence is retained under the machine-local toolchain directory. The author authorized implementation; do not reopen the agreed host/client/Native choices. Continue dependent steps only after their checks pass.
