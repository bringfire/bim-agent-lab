# Task 7 installation/recovery verification

Date: 2026-10-21. Baseline: e25ecb1c888abb3b219b32d55dc4d8306a6ae882. The operation model was committed first at 96669dc. This record initially covers implementation/offline verification and read-only real-install preflight; actual deployment/live observations are recorded separately when performed. Task 7 remains open.

## Observed verification

- `Test-McpBridge.ps1 -Suite All`: exit 0. **41 Deployment cases**, 59 Tools cases including 1,860 generated calls, 87/82 dispatch cases plus ten adapter handoffs, 53 pipe and 33 foundation cases per runtime, protocol/SDK regressions including strict July and legacy rejection.
- `Test-Readiness.ps1`: exit 0, 18 core and seven Excel cases plus export-process isolation.
- `Build-McpBridge.ps1`: exit 0, real Revit API build and locked framework-dependent publish. Windows x64 apphost machine type is checked; no package/project/lockfile changes.
- New official-SDK readiness-driver controls: exact `BridgeUnavailable` on all three revisions; deliberately expecting a report instead is rejected. This proves capture/error assertion behavior, not a model result or desktop acceptance.
- Independent implementing-agent Python comparison of **23** completed restoration trees against C# pre-install file/path snapshots: all match, all receipt states `Restored`. This is independent comparison logic, not an external review claim.
- Read-only preflight against the actual installation: 97 planned file operations, three new installed directories, recognized legacy fixture with no prior inventory. No real installed-target mutation was performed by that preflight.

The [retained evidence](deployment-evidence/inventory.json) contains 277 files: logs, the 23 original/observed hash inventories and recovery metadata/journals. No DLL, working model, staged bytes, backup bytes or actual desktop settings are copied into Git. Temporary recovery paths in synthetic metadata are historical locations; rerunning the tests creates new disposable roots. Actual per-user recovery bytes must remain private and available locally.

## What the matrix establishes

The six successful installation variants cover no fixture, recognized fixture, explicit fixture installation, ordinary installation without MCP, a fresh shared server/recovery parent, and a hash-verified legacy fixture. Each installs and undoes twice in fresh processes, comparing exact prior file bytes/inventories and directory state. Other cases reject invalid fixture identity, unknown legacy bytes, inventory traversal, unowned desired files and forbidden/missing package content before target mutation.

Both caught exceptions and abrupt process exits interrupt first copy, obsolete deletion, fixture core, fixture inventory, manifest, main inventory, after mutation before completion, and before commit. Restoration itself is interrupted and restarted with a trailing incomplete journal record. Additional controls cover missing/changed backups or staged bytes, foreign files/temporary files/directories, unfinished receipts, preparation loss, competing installers and disposed lease ownership. The mutex's global name is opened independently; actual second-logon-session behavior is not claimed.

## Failed attempts retained

The first test compile lacked a helper; that was corrected before behavioral RED. Stubs then produced 26 failures. The first implementation exposed PowerShell converting a null backup argument into an empty path for `File.Replace`; using a true null string corrected atomic replacement. Subsequent tests exposed omitted server license copies, recovery-parent directory ownership, a foreign empty directory mistaken for owned cleanup, and concatenation of a partial journal suffix during recovery. Each failed observation is retained. The initial legacy fixture refusal was intentional; a separately verified migration was added after matching actual DLL hashes to earlier retained evidence. Unknown legacy hashes remain rejected.

The older intermediate logs describe their own candidate and counts. `task7-final-all.txt`, `task7-readiness.txt` and `task7-final-build.txt` are the final offline run for this implementation. Do not substitute an earlier passing count for later verification.

## Provenance and outstanding work

The installer, recovery module, fault harness and SDK verification-client extension are original lab code. SDK 2.2.0 and existing package locks remain unchanged. Existing SDK/source notices are copied with the server; TypeScript SDK 1.26.0 remains precedent-only. The normal readiness installer now uses the same recovery pipeline and requires PowerShell 7.

The [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md), [agent procedure](../mcp-bridge-setup.md) and [operation model](../../../../docs/explanation/recoverable-readiness-installation.md) explain the direction and the concrete guarantees. Real install/undo/reinstall, actual Autodesk scheduling/Start/Stop/shutdown and A5 states, sample comparisons, desktop A8 and independent Task 7/R3 review remain outstanding at this offline checkpoint.
