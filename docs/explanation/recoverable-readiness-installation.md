# Recoverable readiness installation

Task 7 implementation model, written before production changes. The approved plan requires recovery from process loss, not only exceptions caught by the installer. Task 6 independently closed at b4cdb1f; this is the next increment under Native execution.

## Admission and ownership

One per-user mutex lease covers preflight, preparation, apply and recovery. Every public module operation checks that its lease is still held on the owning thread. Revit must be closed. Installation never edits desktop client settings. Source, add-in, server and recovery roots are canonical, disjoint and free of reparse points; file paths must stay inside their declared roots.

Preflight enumerates the entire change, including an existing fixture's core and inventory. An existing manifest must have the expected GUID, assembly path and class. Inventories establish owned paths and hashes. Unknown files are preserved; a desired path with unexpected contents is a conflict. Sources are explicit net48 add-in files, the isolated Excel payload/notices and, only when requested, the published modern server. Autodesk DLLs and modern SDK files never enter the Revit payload. Preflight creates no installed files or directories.

## Durable states and transitions

| State | Durable evidence | Permitted next work |
|---|---|---|
| Preparing | Private transaction directory only | Stage desired bytes and backups; no installed-target mutation |
| Prepared | Complete immutable receipt, verified before/after bytes, planned paths and directories, atomically published and read back | Recheck every before-state and begin apply |
| Applying | Flushed intent before each directory/file mutation; completion afterward | Continue or recover after any failure |
| Committed | Every planned after-state verified | Explicit undo remains available |
| Restoring | Recovery preflight verified all backups and current before/after states; flushed restoration intents | Restore in reverse order; restart after interruption |
| Restored | Before-state files/inventories verified, created owned files/directories removed | Repeated recovery is a verified no-op |
| RecoveryRequired | Receipt/backups retained; unexpected bytes reported | Resolve the conflict deliberately, then retry recovery |

Prepared is the authority even if the last journal record is incomplete. Completion markers are evidence, never the only indication that a mutation happened. Recovery compares actual bytes with both recorded states. It does not overwrite a foreign modification to finish rollback.

Every file replacement uses a unique, planned same-directory temporary path, a durable write and an atomic replace/move. Recovery checks temporary paths too; unknown partial/foreign bytes are retained as a conflict. A receipt records newly absent directories from child through parent; recovery removes only recorded empty directories, never recursively. All backups and desired staging bytes are verified before any apply or restoration write. Catchable apply failures attempt the same recovery procedure used by a new process. Unexpected process loss leaves its durable receipt for explicit recovery; later installs refuse an unfinished transaction.

The recovery plan is immutable; state changes live in a separate atomically replaced state file and flushed append-only journal. Recovery must not require successful parsing of the final incomplete journal line. A live lease and canonical validated receipt constrain every mutation. Preparation failures and corrupted/missing backups must cause zero installed-target changes.

## Verification derived from transitions

Use disposable roots and synthetic package bytes, retaining the exact original bytes and path inventory as an independent oracle. Cover absent/existing/explicit fixture installs, bad fixture identity, changed sources/targets, missing backup/staged bytes, foreign modifications, expired leases, unfinished receipts and package separation. Interrupt actual child processes after first copy, obsolete deletion, fixture core/inventory, manifest/main inventory, mutation before completion and before commit. Interrupt restoration too; run the real undo wrapper twice in new processes and compare all original files, inventories, directories and unrelated files. Exception injection is a separate matrix. Fault hooks exist only as module-internal test instrumentation; no production CLI fault switch is exposed.

Only after this matrix passes may the real installation be changed. Record installed hashes, receipts and real undo/reinstall observations separately. Actual Revit Start/Stop/restart, guarded reads and shutdown require live-host evidence; synthetic files and process interruption tests cannot establish them.
