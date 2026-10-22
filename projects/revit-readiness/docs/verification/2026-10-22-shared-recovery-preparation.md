# Prepare full shared-install recovery without interrupting active clients

October 22, 2026. Preparation only; no installation, recovery receipt state or desktop setting was changed. Actual restoration of these historical layers remains pending.

The author directed us to [continue with explicit cancellation](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-cancellation-disposition.md). The next recovery item is the three shared installation layers that have been audited but never actually undone. Fresh inspection found **Revit closed** and **five installed readiness-server processes active** for the desktop clients.

Before asking for an application action, the agent verified the exact return package and original backups. The author then clarified: **“I can't stop Claude right now...but I can reset Codex.”** The agent left both applications alone. Resetting Codex alone would not release Claude's running server files. This acceptance check can wait while the user's other work is active.

## Read-only preparation that passed

- All **97 installed targets** match the latest committed shared-install receipt.
- All backup and staged bytes in **three historical receipts** match their recorded hashes; their receipt checksums and states also verify.
- A modeled reverse traversal accepts the recorded before/after states in order: **68dac9... → 2ccfb37... → a88dbf5...**. This is a lineage check, not executed recovery.
- All **83 source files** in the archived immutable desktop-server package match the original package record. A new private copy is prepared, together with exact snapshots of the 97 current target files.
- The production installer's read-only whole-plan preflight covers **97 targets**. Executable and dependency bytes match the current baseline. Any generated inventory representation differences preserve the exact filename/hash maps; an eventual reinstall must retain the captured baseline inventory bytes to claim byte-for-byte restoration.
- Opened-file inspection resolves **100 paths** directly: 97 targets and three receipts. This checks this process's filesystem view; it does not replace the earlier actual Revit/desktop access evidence.

The installed host remains **9a93f6d** and the external server **d88de67**. Current build outputs were not substituted for those tested binaries. An initial inspection looked at the archived package's parent instead of its `source` subfolder; the correct package then verified all 83 hashes. No missing-file observation from that wrong path is treated as a damaged installation.

[Retained preparation evidence](shared-recovery-evidence/preparation/hashes.json) contains the audit summary, package manifest, preflight, physical paths and the exact read-only preparation script. Private baseline bytes, staged packages and original recovery backups remain outside Git. Current private run: `samples/local/shared-recovery/d80b639db55c4a39a69c9f15cc8feca0`; the active pointer is in the plan's ignored workspace. These machine-specific paths describe this capture, not participant setup defaults.

## Resume safely when the clients are available

1. Recheck process state. Revit must be closed and every process using the installed readiness server must have stopped normally. Coordinate with the user; do not terminate Claude, reset unrelated MCP connections, or replace files beneath active processes.
2. Recheck all baseline, backup and package hashes and actual opened-file locations. Preparation can become stale while waiting. Preserve foreign changes and investigate mismatches; do not overwrite them to force the historical baseline.
3. Run the existing `Undo-McpBridgeInstall.ps1 -ReceiptPath <exact receipt>` in separate PowerShell processes, in the verified reverse order above. After each step compare actual bytes/absences with that layer's prior state, inspect the receipt state, repeat recovery to verify its no-op behavior, and preserve output. Keep the original receipts and backup bytes.
4. Recompute a fresh whole-install plan from the immutable package against the restored state. Verify all desired executable/dependency targets against the 97-file baseline and every inventory map. If only inventory ordering/representation differs, use the captured baseline inventory bytes after that semantic comparison. Prepare durable recovery through the existing installer module before applying; do not replay an old receipt as a new installation or copy DLLs directly.
5. Apply through the existing lease/preflight/recovery mechanism, verify all 97 final target hashes and inventories, and record the new receipt. Keep recovery available if an intermediate step fails.
6. Exercise the restored installed server with the official SDK on all three revisions, retaining discovery and the expected unavailable-host result while Revit is closed. Reconnect desktop clients when convenient. The next actual Revit session needs a fresh PID/binding and context, then a normal startup/read check; old process IDs are not reusable.

No spec requirement is weakened by deferring this test while Claude is busy. The [R3 packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) must continue to distinguish verified preparation from actual historical-layer recovery. The existing queued-shutdown and diagnostic-layer restoration evidence remains valid for its stated scope. Native Raise overlap/final executing-reader limits and desktop Stop behavior remain separate observations.

The [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#continue-with-a-tested-alternative) explains the human decision; this record supplies the technical continuation procedure.

## Execution follow-up

The author subsequently closed Claude and disabled Codex readiness MCP. [Actual recovery and exact reinstall](2026-10-22-shared-recovery-execution.md) now pass after fresh preflight. The preparation-only observations above remain historical. All three historical receipts are Restored; the new current committed receipt is ef71fb608bba47f49a43ee75fb226bec. Preserve backups and use the current receipt for any future undo. Fresh post-reinstall Revit startup/read is pending.

The [fresh Revit follow-up](2026-10-22-shared-recovery-execution.md#fresh-revit-recovery-passes) subsequently passed after the author reopened the specified model and started the bridge. The pending-live statement above describes the earlier file-recovery checkpoint.
