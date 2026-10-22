# R3 acceptance evidence ledger — October 22, 2026

**Presentation evidence correction:** a correct tool report does not establish a correct participant-facing answer. [R3-01](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md#r3-01--p2-incomplete-coverage-answers-were-not-retained) remains open. The [presentation gate](r3-followup-evidence/README.md#presentation-completeness-gate) is NOT MET even though report replay passes. No original case capture or old evidence manifest was changed.

**Independent R3 returned: keep full acceptance open.** See [original feedback and separate implementer dispositions](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md), including new P2 R3-01. This ledger consolidates evidence rather than changing the approved requirements. The [R3 packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) supplies the exact review scope and questions. The review supports scoped technical and installed-candidate results; it does not close A8/A9, Tasks 7–9 or author acceptance.

## Review identity and evidence boundaries

| Item | Exact reference |
|---|---|
| Implementation baseline, before Task 1 | `ff8a4f359fd0a9ff9147cab366a9cb81683a98cc` |
| Implementation source under review | `627e227c8054feaf91539976b32dd09228c4e93f` |
| Normative documents | [Spec revision 3](../../../../docs/superpowers/specs/2026-10-20-revit-mcp-bridge-design.md), [plan revision 3](../../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md), [desktop compatibility amendment](../../../../docs/superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) at that source commit |
| Installed normal Revit host source | `9a93f6d29bf3c46972701db53bfaee598ec4fd0c` |
| Installed external server source | `d88de673996520d86c3502f357880865d1037fb9` |
| Latest instrumented host source | `32c44eccc3b544eeed64aff5c9ac963d8a137105` |
| Host used in live evidence | Revit 2024, build `24.3.60.12`; .NET Framework 4.8 host and separate .NET 10 server |
| Current recovery owner | Committed transaction `ef71fb608bba47f49a43ee75fb226bec`; previous three transactions are Restored |

The later packet commit contains documentation and verification evidence only. Resolve its full SHA from Git when handing it over; do not silently substitute it for the installed source pins. The [source inventory](r3-evidence/source-inventory.json) and [installed-source comparison](r3-evidence/installed-source-delta.txt) make the distinction inspectable.

Revit was closed during consolidation; the installed desktop servers were left untouched. No new live Revit or desktop calls were made for this packet. Historical PIDs and sessions are evidence identities, never instructions to reconnect to those processes.

## A1–A9 ledger

“Passed” below describes the stated evidence scope. It is not an independent reviewer disposition. A retained capture replay checks recorded data, whereas a fresh suite executes the current source; neither substitutes for a new live host run.

| Requirement | Passed evidence | Remaining qualification / R3 question |
|---|---|---|
| **A1 — dependencies, runtime boundary and provenance** | Locked current-source build and package audit in [fresh checks](r3-evidence/README.md). [Dependency/license record](../../../../docs/research/mcp-sdk-dependencies.md), [notices](../../third-party/mcp/README.md), lockfiles and attributed stdin helper. Server uses official C# SDK 2.2.0 outside Revit; host remains net48. | Review source provenance and notice completeness. The package observation is dated, not a permanent safety guarantee. Original-material license/ownership and public distribution remain undecided. |
| **A2 — MCP contract and transport** | Exactly two production tools, schemas, structured/correlated success and errors, clean stdio, idle-open-stdin shutdown and EOF cancellation in current offline suites. Real desktop wire exchanges use the approved older revisions; compatible July plus strict-July/legacy-rejection regressions remain. | SDK **client token-only cancellation is a known reproduced defect**, not a server fix or passing propagation test. Explicit notifications and EOF are verified. Actual desktop Stop buttons remain unverified; see O2. |
| **A3 — IPC, budgets and progress** | Foundation and Windows Pipe suites on net48/net10; bilateral framing and reports larger than 8,192 bytes, correlation, bounded admission, cancellation, throwing/held callbacks, original budgets, no late publication, eight-slot recovery. Timing evidence includes no-byte/partial/trickle cases. Tasks 2–4 have scoped independent closures. | These tests prove Windows processes and production queue/lifetime code. They do not by themselves prove Autodesk scheduling. Preserve network completion, executor quiescence, callback completion and physical disposal as distinct facts. |
| **A4 — identity, access and restart** | Windows DACL/identity/descriptor tests, lazy zero/multiple-instance selection, peer-exit cleanup, repeated Stop and restart. [Live lifecycle](2026-10-21-live-installation.md) and [recovery](2026-10-22-shared-recovery-execution.md) demonstrate actual host session replacement and descriptor cleanup. | Actual two-Revit-instance selection, a separate interactive logon and a remote-machine attempt were not demonstrated. Local access controls and remote-rejection configuration/source are evidence of narrower scope. See O4. |
| **A5 — host guards, errors and shutdown** | [38 lifecycle captures](live-installation-evidence/capture-index.json): no project, family/unsaved project, invalid/missing/mismatched checklists, expiry, close/reopen, queued document switch, Save As, modal deadlines and fresh-value reads. [Read-boundary deadlines](2026-10-21-host-diagnostics.md), [interruption controls](2026-10-21-host-interruptions.md), [Stop/recovery](2026-10-21-host-stop-recovery.md), and [actual queued application close](2026-10-22-shutdown-preparation.md) with three physical disposals. | Native Raise Accepted/Pending observed; Denied/TimedOut and an in-flight Raise overlapping final shutdown were not observed. Actual queued close passed; concurrent synchronous reader/OnShutdown is not established on Revit's single API thread. Diagnostic timing is not uninstrumented exit-time proof. See O3. |
| **A6 — BIM results and incomplete coverage** | All five cases through **Codex desktop, Claude Desktop Chat and the July SDK client**, 15 full report/wire comparisons. Independent authored oracle/native schedules/checklists; NC1 exact missing identity and NC2 exact unexamined identities/reasons, false flags preserved. Both desktops reject an unexpired switched-model token and recover with a fresh context. | Passed for the recorded installed candidate/configuration. A current-source build is different; see O1. The expired-token attempt is retained but does not prove model-switch detection. |
| **A7 — read-only behavior and existing exports** | Report modified flags false, source/API inspection, model/checklist hashes after closure, manual JSON/text/Excel comparison. Fresh readiness/Excel/process-isolation regressions; [initial live evidence](2026-10-21-live-installation.md) and [all five post-close model/checklist hashes](mcp-readiness-evidence/2026-10-22-nc2/all-samples-post-close-hashes.json). | Scope is the declared read-only tools and tested manual command. There is no claim about all elements, links, physical conditions or quantities. |
| **A8 — both required desktop applications** | [Five-case matrix](mcp-readiness.md), raw wire and retained Claude answers, independent report comparisons, initial [desktop continuity](desktop-readiness-evidence/2026-10-22/hashes.json), structured errors and unexpired switch/recovery. Actual Chat results are distinct from the earlier Claude Code-tab run and optional CLI experiment. | Report/wire evidence passed for recorded builds. R3-01: Codex NC1/NC2 public answers were absent from the packet; NC1 is now recovered with linkage, NC2 remains unproven. Codex AI model/permission metadata is missing; no current-source desktop run. The reviewer found no technical objection to the amendment. See O1/O5 and the returned review. |
| **A9 — reproducibility, teaching, recovery and review** | [Participant starting guide](../../../../docs/start-here.md), [technical handoff](../../../../docs/how-to/agent-handoff.md), both teaching layers, original reviews and separate dispositions, durable installation recovery and exact reinstall followed by fresh live read. This packet supplies an indexed evidence/read set. | **R3 feedback is returned; finding closure, remaining acceptance evidence and author disposition are pending.** Preparation is complete only as a review handoff; Tasks 7–9/overall acceptance are not closed by this document. |

## Five-case outcome, independent of presentation

| Case | Requested / found | Missing fields / affected instances | CoverageComplete | Evaluation.Complete | Readiness verdict |
|---|---:|---:|---|---|---|
| Complete | 16 / 16 | 0 / 0 | true | true | Declared scope passes |
| Wall defects | 16 / 16 | 2 / 6 | true | true | Fails required information |
| Ceiling negative | 16 / 16 | 3 / 8 | true | true | Fails required information |
| NC1: unresolved declared element | 16 / 15 | 0 / 0 | false | true | Inconclusive/incomplete; cannot pass |
| NC2: used type without a requirement | 16 / 16 | 0 / 0 | false | false | Inconclusive/incomplete; cannot pass |

NC1 records WA-01 with UniqueId `6e5a3689-c170-4e43-b22b-e80a27806cdb-7fffffff`, reason `Element not found`, and 1,197 excluded host instances. Its evaluator unexamined list is empty because the missing identity is a scope issue. NC2 records CACT-01/02, ElementIds 2492802/2492809, reason `No matching requirement`, no scope issues and 1,196 excluded host instances. Six link instances are excluded in both. Zero findings is never treated as readiness when either completeness requirement fails.

Both desktops' unexpired switch tests returned `ContextChanged`, then a fresh complete-model check passed. The earlier Claude `ContextExpired` attempt remains separate evidence of expiry; it is not counted as a switch test.

## Client and version record

| Application | Recorded application/wire version | Negotiated protocol | UI/model/permissions |
|---|---|---|---|
| Codex desktop | App `26.1016.6573.0`; `codex-mcp-client` `0.163.0-alpha.11.1` | `2025-06-18` | This local desktop conversation; exact selected AI model and permission-mode baseline not retained for the five-case run |
| Claude Desktop | App `3.1623.4.0`; `local-agent-mode-bim-agent-lab-revit-readiness` `1.0.0` | `2025-11-25` | Human-confirmed **Chat**, **Opus 5.5**, **Auto**; individual earlier permission-click choices not reconstructed |
| Additional SDK client | Official C# SDK `2.2.0` | `2026-07-28` | Test harness, not a substitute for either desktop |

See [initial client baseline](desktop-readiness-evidence/2026-10-22/baseline.json), [Claude UI confirmation](mcp-readiness-evidence/2026-10-22-nc2/claude-ui-baseline.json) and case capture/session files. Initial baseline JSON intentionally preserves its then-pending status; later captures establish success. Configurations and launch arguments are sanitized in evidence; original private config backups are not published. The SDK package version and the dated protocol revision are separate version systems. The amendment permits these observed desktop exchanges while retaining July support and strict regression; it does not adopt a 1.x SDK or rewrite protocol traffic.

## Open items for review, not invented reviewer findings

**O1 — Current source versus live binaries.** Relative to installed host source 9a93f6d, current source changes `PipeEndpoint.cs`, `BridgeApplication.cs`, `RequiredInformationReader.cs` and `RevitReadinessExecutor.cs`. The server comparison from d88de67 changes `PipeEndpoint.cs`. Much host instrumentation is behind `BIM_BRIDGE_DIAGNOSTICS`, but delegate refactoring and optional lifetime-disposal observation also exist outside that condition. Do not claim byte identity or infer a normal-host live pass for 627e227. Diagnostic source 32c44ec has live evidence; the restored normal installation retains older pins. Plan Task 9 requires affected live cases on a newly installed candidate if that candidate is to be accepted/released. This handoff deliberately exposes the gap rather than installing without coordinating applications.

**O2 — SDK client cancellation defect.** [Diagnosis](2026-10-22-sdk-cancellation-diagnosis.md) and [author disposition](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-cancellation-disposition.md): on SDK 2.2.0/.NET 10.0.13, token-only cancellation can cancel the local wait without notifying the remote request. The characterization passes by reproducing that defect across three revisions; the require-propagation probe fails as expected. Explicit `notifications/cancelled` and EOF reach the real pipe/host guard; they do not repair the SDK. No fork, patch, upgrade or upstream submission occurred. Reassess after any SDK change and separately test desktop Stop controls before claiming them. The author accepted continuing with explicit cancellation; that is not independent defect closure.

**O3 — Native scheduling limits.** Review which remaining native observations the approved spec/plan require before closure. Keep actual queued application close, injected Stop during a reader and offline held-Raise tests distinct. An unavailable native outcome needs a supported limitation/disposition; it cannot be marked passed. No normative waiver is made here.

**O4 — Access/instance scope.** Synthetic Windows peers and local access checks do not establish two simultaneously running Revit hosts, remote access denial from another machine or an actual alternate interactive user. Review the code and evidence against the precise A4 obligations before assigning acceptance.

**O5 — Desktop reproduction metadata.** The five-case functional results have wire proof, but the exact Codex AI model and tool-permission mode are missing from the retained baseline. Do not infer historical settings from today's application. Complete a properly recorded future desktop run if needed for A8 closure. The latest recovery changed only PID bindings and preserved the user-disabled Codex setting; it did not repeat desktop A8.

**O6 — Retained test/evidence failures.** The recovery Protocol run had a 15-second `test_existing_release_is_rejected_before_capture` timeout. An unchanged isolated rerun and full repeat passed; root cause remains undiagnosed. The auxiliary `reinstall-plan.json` has null fields and is unusable as a plan summary; checksum-verified installer receipts and independent before/after comparisons supply the recovery proof. Preserve both failures and their limits. The fresh All run passed but emitted four CA1416 platform-analysis warnings in `ReadinessArgumentBoundary.cs`; these were not suppressed or fixed. Fresh consolidation results are recorded separately in [fresh checks](r3-evidence/README.md).

**O7 — Review and distribution.** R3 has returned for packet 0489c54; the compatibility amendment received no technical objection. Full acceptance remains open, including R3-01 and the existing source/live and metadata gaps. Original-material licensing, public remote and release packaging are separate author decisions. No distribution readiness is asserted.

## Provenance audit

The production MCP dependency is **ModelContextProtocol 2.2.0**, with **ModelContextProtocol.Core 2.2.0**, consumed through locked NuGet packages. MCP parsing, protocol negotiation and dispatch are supplied by the SDK. `src/mcp-server/CancellableInputStream.cs` adapts the official C# 2.2.0 stdin helper from source commit `6fa3825973949a9c4f0cd8af344e15a8db09dc35`; its attribution and notices are retained. This is actual adapted code, not merely research.

The lab's pipe/framing, lifetimes, dispatcher, context binding, argument boundary and Revit adapter are original implementation. Existing lab computation is reused: `RequiredInformationReader.cs`, `Checklist.cs`, and `TypeInformation.cs` (see exact locations in the source inventory). **TypeScript SDK 1.26.0, existing third-party Revit bridge code and RevitMCPSDK are research precedents only**, with no selected source/dependency adopted. SDK 2.3/main inspection was diagnostic research, not runtime adoption. The source inventory, project references, lock graph and Git diff support review of this claim; a text search alone cannot prove originality.

The MCP notices retain the SDK Apache-2.0 license including its MIT contribution notices and Microsoft dependency license sources. ClosedXML belongs to the separately verified Excel exporter process; modern MCP assemblies are not loaded into the net48 Revit add-in. A dated advisory check does not guarantee absence of vulnerabilities or authorize redistribution of Autodesk/sample material.

## Replay and fresh checks

The [read-only replay script](r3-evidence/replay.py) verifies original evidence manifests, the 15 case comparisons, both unexpired switch/recovery cases, initial desktop wire continuity, the latest live wrapper capture, host lifecycle/shutdown captures and SDK defect characterization. [Replay output](r3-evidence/replay.json) is supplied evidence verification, not a new host run. Hashes establish retained-byte consistency; they do not independently establish that a claim is correct.

[Fresh check commands, results and limitations](r3-evidence/README.md) cover the current source. The packet's [input inventory](r3-evidence/source-inventory.json) pins production/test/lock source bytes at 627e227; its [local evidence manifest](r3-evidence/hashes.json) protects this consolidation's retained files. The returned reviewer report states which checks it personally ran and which historical live captures it inspected; no new host run is inferred from that review.
