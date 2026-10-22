# Install the tested server in a shared location

October 21, 2026 local time. This continues Task 7 after [normal-build restoration](2026-10-21-host-stop-recovery.md). It is not desktop A8 acceptance or Task 7 closure.

The author confirmed Revit was closed. The diagnostic recoveries had restored normal candidate **9a93f6d**, including all 97 target hashes. Opened-file inspection still showed the old server and recovery records under Codex's MSIX cache. The next installation therefore uses **`%USERPROFILE%/.bim-agent-lab/RevitReadiness`** as the server/recovery parent, the same shared user-profile approach already observed successfully for diagnostic controls.

## Preserve the tested binaries while changing location

A read-only preflight against current build-output folders proposed different add-in, core, runtime, contract and Excel binaries. Those outputs were not deployed. The implementing agent instead prepared an immutable local package from the restored, verified installed bytes and the unchanged manifest template from **9a93f6d**. The package contains **83 files**; [package.json](shared-installation-evidence/package.json) identifies each source and hash. This is installation of the tested build, not a rebuild or a new SDK version.

The first package also included the installed server's license-notice folder inside its publish input. The existing installer separately adds those same notices and rejected the duplicate deployment target before writing anything. A new package omitted the duplicate input; the original rejected package was preserved privately. Initial inventory comparison also treated list order as meaningful; exact file-to-hash maps established that the planned inventory differences were ordering/formatting only. The package and planning corrections did not change any executable.

The existing deployment module installed the package under receipt **a88dbf5d84b94188a695e2dc73b6e954**. Before target mutation, opened-file handles showed the prepared receipt and backup file directly under the shared root. After installation, all **97 targets** matched their planned hashes; **96 prior target files** retained exact bytes, and the remaining prior inventory retained its exact file-to-hash map. The old redirected server and old recovery bytes remain intact. They were not deleted or silently repurposed.

Official **C# SDK 2.2.0** and all resolved dependencies/licenses are unchanged. Third-party notices accompany the relocated server. TypeScript SDK 1.26.0 remains precedent-only; no SDK implementation was copied or hand-written.

## Keep the public installer consistent

[Install-Readiness.ps1](../../scripts/Install-Readiness.ps1) now defaults its runtime/recovery root to the shared profile location. Optional `RuntimeRoot` and `SourceRoot` parameters allow an explicit destination and immutable package. The deployment module, locks, preflight, backup, apply and recovery algorithms are unchanged. Revit add-in and descriptor locations are unchanged.

The original entrypoint rejected the explicit immutable source before making writes; [RED](shared-installation-evidence/entrypoint-red.txt) retains that interface failure. The updated entrypoint was then run against the same pinned package, relying on its **default destination**. It produced direct shared receipt **2ccfb37b641e4a7f93391f6788dde437** and all 97 expected target hashes. [GREEN](shared-installation-evidence/entrypoint-green.txt), [actual paths](shared-installation-evidence/entrypoint-physical-paths.json), and [comparison](shared-installation-evidence/entrypoint-comparison.json) establish the result. This is an actual setup check on the seminar machine; the disposable Deployment suite exercises the underlying module separately.

Do not run an old redirected receipt from a different filesystem view. The two diagnostic receipts **74e482...** and **b0d820...** were already restored. For the current relocation, reverse the new layers in order: **2ccfb37...**, then **a88dbf5...**, using the recorded receipts and normal whole-plan checks with Revit closed. The first returns to the first shared installation; the second returns to the pre-relocation state and removes only its owned new files/directories. This new pair has not itself been undone live; earlier live recovery and the disposable matrix do not substitute for that observation.

## Startup and compatibility evidence

The installed shared executable was called through the official SDK using `2025-06-18`, `2025-11-25`, and `2026-07-28`, with Revit closed. All three discovered exactly the two readiness tools and returned a correlated structured `BridgeUnavailable` result. Separate raw-message comparisons verified protocol selection, request/response identities, catalog and error content.

The first comparison incorrectly demanded whole-object equality between legacy wire data and a serialized SDK DTO. Inspection showed two additional null representation fields (`_meta`, `resultType`) in the DTO. The corrected comparison preserves every actual wire member and permits only those additional null members; application content and errors are unchanged. This correction is recorded in the comparison, not treated as a server defect.

Actual opened-file handles for the shared executable, SDK DLL and both new recovery receipts resolve directly, without AppData redirection. These observations are from the agent's process context. **Codex desktop and Claude Desktop must still exercise the installed tools themselves.** The three SDK checks establish startup/discovery/error behavior, not successful live reading, the five-case BIM oracle or desktop acceptance.

Fresh **41-case Deployment** and **Protocol** regressions pass, including compatible older revisions, compatible July, strict `2026-07-28` and legacy rejection. Results are retained in [deployment-tests.txt](shared-installation-evidence/deployment-tests.txt) and [protocol-tests.txt](shared-installation-evidence/protocol-tests.txt). No readiness/Excel computation changed; those suites were not rerun for this relocation.

## Normal Revit follow-up

The author replied **“ready”** after opening the normal add-in, the disposable `BAL-info-saveas-fresh.rvt` model and its bridge. Actual Revit PID **149000**, session **9d81e5ef4dd844aca731d5ed5fa6746b**, then served a fresh context and readiness request through the installed **shared** executable and official SDK on `2026-07-28`. Its [descriptor](shared-installation-evidence/live/session.json) resolves directly in the unchanged session location; [opened-file evidence](shared-installation-evidence/live/descriptor-physical-path.json) confirms that observation.

The existing independent Python wire checker verified discovery, both tool calls, unique response identities, and text/structured/saved-result agreement. A separate whole-report comparison with the retained interruption baseline matched **every field except `GeneratedUtc`**. Both contexts identify the same model, project, checklist and scope; the new bridge session differs. All **16 declared instances** were examined, with **zero findings**, both completeness flags true and both modified flags false. See [comparison](shared-installation-evidence/live/comparison.json) and [full result](shared-installation-evidence/live/sdk/check.json). This establishes continuity after relocation, not the separate Task 8 BIM oracle or either desktop application's acceptance.

A read-only post-run check matched all **97 installed targets** to the latest committed receipt; [installed verification and source hashes](shared-installation-evidence/live/installed-verification.json) retain the comparison. Revit remains open on the normal candidate. No installation, recovery or Deployment suite may run until it is closed. The [evidence manifest](shared-installation-evidence/hashes.json) covers the retained files byte-for-byte.

The [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#change-one-thing-at-a-time-during-setup) explains the human/agent division. The [setup guide](../mcp-bridge-setup.md) supplies current commands. Remaining Task 7 host observations, unresolved earlier notification/token-only findings, actual desktop access and R3 remain open.
