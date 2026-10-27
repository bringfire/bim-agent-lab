# Summit workstation intake — October 27, 2026

Inspection only. Source: `c69df1c09a69c4d7e7c77127ef094606b4a0a4f0`, initially clean at `C:\repos\bim-agent-lab`. The author requested finishing the repository before the summit on this machine, including Revit 2026 compatibility. The author then clarified: prove the existing plugin first, and afterward assess extending MCP live versus explaining the agent-driven process interactively, constrained by time. No attendee-package requirement is inferred. No new host support is established. The [proposed compatibility design](../../../../docs/superpowers/specs/2026-10-27-revit-2026-5-compatibility-design.md) scopes the first pass.

## Observed inputs

| Check | Observation | Consequence |
|---|---|---|
| Git | `2.56.0.windows.3`; SSH clone succeeded as bringfire | Source available locally |
| Revit 2024 | Executable `24.3.60.12` | Historical version available; no fresh host test |
| Revit 2026 | Executable and both API DLLs `26.5.10.38` | Immediate target is **2026.5**, not all 2026 releases |
| Installed API/host runtime files | `RevitAPI.runtimeconfig.json` and `RevitNET.runtimeconfig.json` declare `net10.0` | Design the host target against the installed runtime |
| .NET SDK | Only `9.0.319` listed | Does not satisfy `global.json` (`10.0.204`, `latestPatch`) |
| Modern runtime | Core, Windows Desktop and ASP.NET Core include `10.0.10` | Runtime present; compiler SDK still missing |
| Framework references | .NET Framework 4.8 targeting references present | Existing 2024 framework references available |
| Revit processes | None found | Historical PIDs/installation state must not be reused |
| Models | Snowdon architectural sample exists in both installed sample folders; repository `samples/local/` absent | Prepare disposable fixtures here |
| Python | WindowsApps command found; not executed | Confirm a real interpreter before independent checkers |

The unchanged `Check-Environment.ps1` was run against its default 2024 target. It found Revit/API files and framework references, then **failed because the pinned SDK is missing**. No build or automated suite passed on this machine. The script also explicitly rejects Revit major versions other than 24.

Autodesk's [September 29 transition guidance](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Revit--Requirements-for-products-affected-by-the-Microsoft--NET-10-transition.html), checked October 27, states that Revit 2026.5 uses .NET 10. This agrees with the installed configuration. Older [2026 migration documentation](https://help.autodesk.com/cloudhelp/2026/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_NET8_Update_html.html) discusses .NET 8: the year alone does not determine the runtime.

## Source boundaries

- **Host projects:** probe, readiness and optional fixture tools target `net48` and default to 2024 API paths. The core also targets only `net48`. Preserve 2024 while adding a separately verified modern build.
- **JSON:** `src/readiness-core/Checklist.cs` and `src/revit-addin/ReadinessPayloadCodec.cs` use `System.Web.Script.Serialization.JavaScriptSerializer`. The modern implementation must preserve identities, explicit false completeness flags, size/invalid-input behavior and literal date-like strings. `BridgeJson.cs` already has separate legacy/modern paths, but does not resolve these core/adapter dependencies.
- **Bridge:** contract/runtime already target `net48;net10.0`; the external server uses .NET 10. Actual modern Revit compilation and lifecycle/pipe tests remain necessary. No package upgrade is selected by this intake.
- **Deployment:** build/install scripts and `McpDeployment.psm1` fix output paths to `net48` and/or destinations to `Addins/2024`. Changing the API folder alone would leave deployment wrong. Preserve recovery receipts, consuming-application path verification and Autodesk API Copy Local=false checks.
- **Excel:** retain the separate exporter process and its configuration unless tests justify changing it. Its framework runtime is distinct from Revit's runtime; verify invocation and whole-report/workbook agreement from the modern host.
- **Fixtures:** use version-specific disposable copies and fresh independent expectations. Do not overwrite 2024 originals or reuse their saved-file hashes as upgraded-model expectations.

## Recommended order, pending scope/design decisions

1. Supply a compatible pinned SDK, verify Python, run locked builds and current offline suites on this machine.
2. Prove the smallest read-only command in installed 2026.5; then readiness/core/fixture compilation, version-specific outputs and recoverable deployment.
3. Freeze one candidate and test direct JSON/text/Excel plus both actual desktop clients. Record current client/model/permission metadata and installed hashes.
4. Recreate five cases with independent expectations: complete `0/0`, wall defects `2/6`, ceiling negative `3/8`, NC1 false coverage and NC2 both completeness flags false. Repeat unexpired context-switch refusal/recovery, read-only checks, stop/shutdown and rollback within their documented scope.
5. After working-plugin proof, choose and rehearse the demonstration that fits the available time: live MCP extension or an interactive explanation with an agent. Keep reset/fallback and the two-session agenda explicit. Attendee packaging is a later separate decision, not a prerequisite introduced by this intake.

The [R3 ledger](2026-10-22-r3-acceptance.md) and [follow-up](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-follow-up.md) retain historical scope. NC2 presentation, Codex metadata, current-source/live-build continuity, desktop Stop and bounded native/access observations remain open. Physical-condition and quantity checks remain planned. [Workshop materials](../../../../workshops/swacca-2026/README.md) identify October 29–30 and the two sessions planned for October 29; detailed demo scripts, rehearsal and a tagged release are outstanding.

No plugin code, dependencies, models, installed files or application settings changed during this intake. No applications were launched or stopped. This document supplies inspection evidence, not an approved implementation design or release acceptance.
