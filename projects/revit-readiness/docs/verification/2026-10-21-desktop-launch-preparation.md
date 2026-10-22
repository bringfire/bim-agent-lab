# Prepare repeatable desktop launches

October 21, 2026 local time. This is setup work toward Task 8, brought forward to finish Task 7's shared-installation handoff. It does not close the remaining host checks or either desktop's A8 acceptance.

The [shared normal installation](2026-10-21-shared-server-installation.md) passed its live SDK/Revit check. Before registering it in the desktop apps, source inspection found a difference from the earlier probe: the production `--wire-evidence` option selected one fixed directory, whereas the probe created a new session directory on each launch. `McpHost` correctly uses exclusive file creation to preserve existing evidence. Reusing a fixed directory therefore rejects a second process. Desktop reconnects and multiple consumers require a parent directory with distinct child captures.

## Source correction and proof

The production entrypoint adds **`--wire-evidence-root <parent>`**, choosing `session-<random GUID>` for each process. Existing `--wire-evidence <new-directory>` semantics remain unchanged for controlled harness captures. Specifying both modes fails before creating evidence. The SDK transport, cancellation, argument validation, Revit code and evidence byte limit are unchanged; no wrapper or protocol translation was added.

Three new actual-executable cases failed before the change with unexpected stdout EOF: [RED](desktop-launch-evidence/desktop-trace-red.txt). Afterward, each tested revision (`2025-06-18`, `2025-11-25`, `2026-07-28`) supports two simultaneous server processes and a subsequent reconnect with identical launch arguments. Each process exits naturally after stdin closes. Separate evidence contains each request and correlated structured `BridgeUnavailable` response for a deliberately nonexistent Revit PID. Prior evidence is unchanged. The independent [wire comparison](desktop-launch-evidence/independent-comparison.json) checks all nine exchanges; this is executable verification, not a call from either desktop application.

Four further process controls verify unchanged fixed-folder refusal, conflicting modes in both orders, and preservation of a file blocking the evidence root. The complete **72-case Tools suite passes**: [GREEN](desktop-launch-evidence/desktop-trace-green.txt). Builds reported existing platform-analysis warnings in `ReadinessArgumentBoundary`; no runtime fix is claimed for those warnings. An initial new test nullability warning was corrected before the green run.

## Recoverable client registration

The existing [settings helper](../../scripts/manage_mcp_probe_settings.py) now also supports entry `bim-agent-lab-revit-readiness`, an explicit positive Revit PID, and the per-session trace root. The original probe behavior and older receipts remain supported. Preparation saves private original bytes before mutation; restoration uses the receipt's entry identity, preserves unrelated edits and refuses foreign changes to that entry.

The new CLI cases failed before the interface existed: [settings RED](desktop-launch-evidence/readiness-settings-red.txt). All **eight settings test methods** now pass, including JSON/TOML preparation without writes, exact arguments, recovery in a new process after a missing completion marker, repeated recovery, coexistence with the probe, preservation of concurrent unrelated edits, refusal of changed owned entries, invalid PIDs and old receipts without an explicit name: [settings GREEN](desktop-launch-evidence/readiness-settings-green.txt).

Codex's documented configuration still uses `[mcp_servers.<name>]` with `command` and `args`; [official configuration reference](https://developers.openai.com/codex/mcp#configure-with-configtoml). The local Claude file retains the `mcpServers` shape used in the earlier actual CP0 run. Read-only inspection found no existing lab entry in either app. Installed candidates are now **Codex 26.1016.6573.0** and **Claude Desktop 3.1623.1.0**, newer than the recorded CP0 versions. Neither new version is accepted merely because the earlier one passed.

**No desktop settings were changed.** The author needed to close Revit for the night; process inspection confirms it is closed. The next run needs a fresh actual Revit PID and both applications' real wire/result evidence. SDK 2.2.0, compatible desktop policy and strict July regression remain required. The participant and agent instructions distinguish this preparation from live acceptance.

The fresh [Protocol suite](desktop-launch-evidence/protocol.txt) also passes, including strict `2026-07-28`, rejection of older versions in strict mode, compatible exchanges and Windows stdin-shutdown checks. [SHA-256 manifest](desktop-launch-evidence/hashes.json) covers the retained evidence. This source checkpoint precedes packaging/installing the new server; installed normal candidate 9a93f6d does not yet implement the new option.
