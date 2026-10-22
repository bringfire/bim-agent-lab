# Night checkpoint — October 21, 2026

The author asked the agent to finish independent work and stop for the night. **Revit is closed. No further user action is needed tonight.** Neither Codex nor Claude Desktop settings were changed in this increment.

## What is established

- The normal bridge served the disposable `BAL-info-saveas-fresh.rvt` through the shared server path: 16 examined elements, zero findings, both completeness flags true and no model changes. The full report matched the earlier baseline except its timestamp. [Live continuity evidence](../../projects/revit-readiness/docs/verification/2026-10-21-shared-server-installation.md#normal-revit-follow-up).
- The external server now supports separate evidence folders for repeated/concurrent desktop launches. Source **d88de67** passes **72 Tools cases**, **eight settings test methods**, and Protocol checks including strict `2026-07-28`. SDK **2.2.0** is unchanged. [Source and evidence](../../projects/revit-readiness/docs/verification/2026-10-21-desktop-launch-preparation.md).
- That server update is installed at `C:/Users/bring/.bim-agent-lab/RevitReadiness/McpServer/BimAgentLab.RevitReadiness.Mcp.exe`. The Revit add-in remains the normal **9a93f6d** build. All 97 installed targets and recovery bytes verified; post-install startup/discovery/error checks pass. The earlier successful live read predates this last tracing update.

## Resume with the human

1. Open Revit 2024 and the disposable model below, then choose **Add-Ins → External Tools → BIM Agent Lab – Start readiness bridge**. Record the new actual Revit PID/session; do not reuse PID 149000.
2. Have the agent run one fresh live continuity check against the installed update. Then prepare recoverable `bim-agent-lab-revit-readiness` entries for **Codex desktop and Claude Desktop**, using that PID, the installed absolute path and separate `--wire-evidence-root` parents. Current observed app versions are Codex **26.1016.6573.0** and Claude Desktop **3.1623.1.0**; re-read them at acceptance.
3. The human reloads each app's MCP connections. Actual discovery/context/readiness/error traffic and application answers must be retained. Task 8 still requires the three principal samples and NC1/NC2 with independent comparisons; the single continuity report is not the five-case oracle or A8 acceptance.

Disposable model:

```text
D:\bim-agent-lab\projects\revit-readiness\samples\local\mcp-live-task7\a10630e39dae448ba55abbfe0bc7ab6b\models\BAL-info-saveas-fresh.rvt
```

## Agent continuation and recovery

Use [the setup guide](../../projects/revit-readiness/docs/mcp-bridge-setup.md#repeated-desktop-launches-and-settings-recovery), the approved [plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md), root/project AGENTS and the private plan ledger. Native execution remains selected. The human operates Revit; no Computer Use is needed. Do not repeat completed diagnostic tests or reopen old review findings without new evidence.

Newest installation receipt:

```text
C:\Users\bring\.bim-agent-lab\RevitReadiness\InstallRecovery\68dac9adc1c04f0694a12acfda4e9a6c\receipt.json
```

If recovery is intentionally needed, close Revit first. Reverse newest **68dac9...**, then **2ccfb37...**, then **a88dbf5...** only to unwind those shared installation layers. None has been undone live. Older diagnostic receipts are already Restored; preserve their original filesystem view and backups. Never replay an old redirected receipt in a different view or overwrite installed DLLs manually.

Task 7 remains open for recorded outstanding host observations, including in-flight application shutdown/native-event paths and physical disposal, plus the unresolved token-only SDK cancellation and intermittent offline notification observations. Actual desktop A8 and full R3 remain ahead. Passing setup tests do not close those gates. No remote push or public release was performed.
