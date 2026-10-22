# Current normal candidate for R3 follow-up

**Status: candidate built, frozen and checked offline; installation awaits released desktop connections. Live acceptance remains pending.** The [independent follow-up](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-follow-up.md) closes only the historical NC1 presentation portion of R3-01. NC2, Codex baseline, current-build acceptance and the separately disclosed native/desktop Stop limits remain open. The [preparation evidence index](r3-current-candidate-evidence/preparation/README.md) links 49 retained files with hashes; no installed files were changed.

## Candidate identity before acceptance

The clean build source commit is **`9c27d50b5ce2f7763093abf2cbb2db0916ce05d6`**. Production source is unchanged from reviewed implementation `627e227c8054feaf91539976b32dd09228c4e93f`; recording only that earlier implementation SHA would omit the actual checkout used for this build.

The normal net48 host and modern server were rebuilt with locked dependencies, then the supported `Build-McpBridge.ps1` procedure published the server. The diagnostic project was not selected. The normal host project does not define `BIM_BRIDGE_DIAGNOSTICS`; current production source still contains the previously reviewed optional observer boundaries. This record does not claim those source boundaries are absent.

An independent source package was frozen at `samples/local/r3-current-candidate/b583cc4e037846afb02233d213bed2e0/source`. It contains 84 files. The source record identifies 131 tracked production/build/license inputs by worktree SHA-256 and Git blob; normal newline conversion is accounted for. Subsequent builds in the working checkout cannot replace this package. Do not rebuild or replace the installed candidate once acceptance begins; a candidate change requires a new run and explicit disposition of previous evidence.

The project `global.json` is retained as an additional build-input identity. The captured SDK is 10.0.204. The build reports zero errors and retains the four previously disclosed CA1416 warnings in `ReadinessArgumentBoundary.cs`; no warning fix or dependency change is claimed.

## Fresh offline verification

The implementing agent ran `Test-McpBridge.ps1 -Suite All` and `Test-Readiness.ps1`. They passed: 41 deployment tests, 72 tool tests, 88/83 dispatch tests, ten adapter tests, 53 pipe and 33 foundation tests per runtime, bilateral/Python checks, compatible protocol and strict-July/legacy-rejection controls; 18 readiness tests, seven Excel tests and isolated export. Logs retain the exact commands' output and counts.

The official SDK driver separately launched the **frozen package's executable** with Revit closed. On `2025-06-18`, `2025-11-25` and `2026-07-28`, it verified both tool names, the negotiated revision and a structured, correlated `BridgeUnavailable` with no report. These are offline failure/discovery controls, not successful Revit reads or desktop acceptance. The main suite's strict policy tests remain separate from these compatible production-server calls.

Afterward, all 84 package files, 131 production/build/license inputs, 97 existing installed targets, 164 recovery files and the original 53 follow-up evidence files still matched their recorded hashes. The new 49-file preparation snapshot includes the five independent expectations, capture checklist and exact local helpers. The installation helper was syntax-checked but not executed. No new current-candidate runtime acceptance result is implied.

## Installation preparation

The existing installation has 97 verified targets. All 164 backup/staged files belonging to current receipt `ef71fb608bba47f49a43ee75fb226bec` verify. Its state is Committed. Existing receipts and private backup bytes are preserved; historical restored receipts must not be replayed against this installation.

The supported deployment module produced a read-only whole-install plan for the frozen package: 97 targets, 17 byte changes, with 98 existing target/receipt paths verified as directly opened rather than redirected. No recovery receipt has yet been prepared for this candidate and no installed files have changed. Two installed readiness server processes were observed. The author was asked to close Claude Desktop and disable only the readiness MCP connection in Codex, while keeping Revit closed, so installation can proceed without replacing active files.

Before mutation, repeat the package/target/process/path checks under the deployment lease. Publish and verify the new durable recovery receipt before applying the plan. After apply, record all installed hashes, receipt checksum and actual opened-file paths. This **installed freeze**, not the earlier source-package freeze alone, is the prerequisite for live acceptance. Do not silently mark the existing installation as the new build.

## Prepared live sequence

The five existing disposable model/sidecar pairs were hashed without editing or opening them. Expected answers were regenerated from authored checklist identities, native schedule TSVs and the independent comparator. Principal sidecars match their authored data; NC1 changes only the declared WA-01 identity and NC2 removes only the Ceiling ACT requirement. Both control RVT files match the complete RVT byte-for-byte. Fresh Revit PID/session binding and the NC1 native absence confirmation remain pending before their first calls.

| Case | Requested / examined | Findings / affected | Coverage / evaluation complete |
|---|---:|---:|---|
| Complete | 16 / 16 | 0 / 0 | true / true |
| Wall defects | 16 / 16 | 2 / 6 | true / true |
| Ceiling negative | 16 / 16 | 3 / 8 | true / true |
| NC1 | 16 / 15 | 0 / 0 | false / true |
| NC2 | 16 / 16 | 0 / 0 | false / false |

These are **expected outcomes**, not new observed results. For each required desktop, capture its application version, actual AI model label, UI surface, permission mode, relevant launch configuration, negotiated protocol and installed candidate identity. Capture discovery and each actual prompt/public answer alongside request identity, wire/tool data, independent comparison and a separate assessment of the answer. Do not expose expected answers in the neutral Claude prompt.

Then retain an unexpired stale-context rejection and fresh-context recovery in both applications. Keep the July SDK regression and supplemental live calls separate from desktop acceptance. Compare model/sidecar bytes after close and record modification flags separately. Use the [desktop procedure](../../../../docs/how-to/verify-readiness-through-desktop-agents.md).

Normal candidate shutdown/restart evidence must identify what was directly observed. Previously captured diagnostic ordering/disposal controls remain useful, but cannot prove uninstrumented shutdown timing. Unavailable native Raise observations require explicit supported disposition, not an inferred pass. Desktop Stop remains unverified until its actual cancellation arrival is observed; passing explicit SDK notifications does not establish it.

## Teaching and review boundaries

The [participant story](../../../../docs/tutorials/15-prepare-an-evidence-based-review.md#freeze-the-example-you-are-testing) explains how the person directed this step. The [agent procedure](../../../../docs/how-to/prepare-r3-evidence.md#freeze-the-current-normal-candidate) gives the technical handoff. No new architecture, dependency version, cancellation implementation or acceptance waiver is introduced. Full R3 acceptance remains open.
