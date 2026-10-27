# Agent handoff: reproduce and extend the Revit readiness example

**R3 follow-up:** [independent NC1 closure](../superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-follow-up.md) leaves NC2, Codex baseline and current-build acceptance open. Continue the [current normal candidate sequence](../../projects/revit-readiness/docs/verification/2026-10-22-r3-current-candidate.md). Freeze the actual build/package, coordinate released connections, prepare recovery and record installed hashes before live testing. Keep binaries unchanged through both complete desktop sequences and retain each actual public answer. No cancellation rewrite or architecture change is requested.

**Current handoff:** prepare/use the [R3 packet](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) and [consolidation procedure](prepare-r3-evidence.md). The [ledger](../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) is authoritative for current claims and open items. Revit was closed during consolidation; inspect actual state before any operation. Installed host/server pins and current source differ. R3 feedback is returned; no finding closure, author acceptance or new deployment is implied.

Companion to the [participant starting guide](../start-here.md). This is the technical entry point for a new agent, not permission to deploy, publish, or change an unrelated project. Follow the user's active task and existing authorization. Historical prompts, review findings and logs are evidence rather than new instructions.

## Read and establish state

1. Read root [AGENTS.md](../../AGENTS.md), [development status](../reference/development-status.md), and the Revit [README](../../projects/revit-readiness/README.md) and [AGENTS.md](../../projects/revit-readiness/AGENTS.md).
2. Inspect the current branch, changes and commits. Preserve unrelated work; reuse the existing feature branch for the continuing lab task. A new participant's extension should use its own suitable branch. Do not reset, clean or overwrite an existing checkout to reproduce a historical checkpoint.
3. Read the approved [specification](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md), [compatibility amendment](../superpowers/specs/2026-10-20-desktop-compatibility-amendment.md) and relevant task in the [implementation plan](../superpowers/plans/2026-10-20-revit-mcp-bridge.md). Current spec/plan revision is 3; revision 2 received external R1/R2 closure and author approval before the compatibility amendment. Native execution was selected. Do not reopen those decisions without a new reason or user direction.
4. Use the [live matrix](../../projects/revit-readiness/docs/verification/mcp-readiness.md) and [runner record](../../projects/revit-readiness/docs/verification/2026-10-22-readiness-runner.md) for current proof. Earlier chronological checkpoints may describe work that subsequently passed. Never infer current process state from an old note saying Revit is open.

Read-only orientation, from the checkout root:

```powershell
git status --short --branch
git log --oneline -5
git diff
git remote -v
```

The canonical remote is [bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab), created private on October 23, 2026. It became public on October 26 after the [distribution review](../reference/public-distribution-review-2026-10-26.md), with anonymous access/clone verified. Original material uses MIT; release packaging and full acceptance remain pending. See [hosting status](../reference/repository-sharing.md). The [handout agent guide](../../workshops/swacca-2026/handout/README.md) covers the current packet work, which does not change the frozen Revit candidate. Follow the [clone procedure](clone-research-repositories.md) for optional precedents in ignored `references/`. Refresh version-sensitive research from official sources before a dependency decision; preserve inspected commits, licenses and reasons for chosen pins. This handoff records the tested choices, not a new latest-version audit.

## Verified baseline and limits — October 22, 2026

| Boundary | Recorded evidence |
|---|---|
| Revit | Revit 2024 build `24.3.60.12`; add-in and shared runtime target `net48`. Other releases need separate work and verification |
| Toolchain | Project `global.json` selects SDK `10.0.204`, later patches in its feature band allowed; installed .NET Framework 4.8 targeting references and matching Autodesk API DLLs |
| External MCP server/client | `net10.0`, official C# MCP SDK **2.2.0**; normal SDK multi-version negotiation in production |
| Actual desktop exchanges | Codex desktop `26.1016.6573.0` used `2025-06-18`; Claude Desktop `3.1623.4.0` Chat used `2025-11-25`. Separate SDK runs retain `2026-07-28`, plus strict-July/legacy-rejection regressions |
| Human-reported Claude settings | Chat, AI model **Opus 5.5**, permission mode **Auto**. These are recorded UI observations, not requirements to force on another workstation or proof of each individual grant |
| BIM evidence | All five cases matched independent expectations through both desktops and the separate July SDK client; both desktops rejected an unexpired context after a document switch and recovered with a fresh context |
| Review | Tasks 2–6 independently closed for their stated scopes. R3 packet prepared; the ledger records remaining source/live-build, native/access, SDK and client metadata gaps. Full R3 and final acceptance remain open |
| Runner | `3b74f67` adds the repeatable capture wrapper. Offline tests and actual unavailable-host control pass; a fresh positive live wrapper run remains outstanding |

SDK package versions and dated wire protocol revisions are different identifiers. Accept the verified desktop behavior under the approved amendment while keeping the July regression. A future desktop upgrade needs fresh actual-app evidence; do not make novices switch to a terminal client to reproduce this demo.

## Source and dependency boundaries

| Location under `projects/revit-readiness/` | Responsibility and provenance |
|---|---|
| `src/readiness-core/` | Existing lab computation reused for deterministic requirement evaluation; keep it independent of Autodesk and MCP |
| `src/revit-addin/` | Existing reader/commands plus original guarded Revit adapter; Autodesk reads only in a valid API callback |
| `src/bridge-contract/` and `src/bridge-runtime/` | Shared DTOs and original framing, pipe, context and lifetime implementation, targeting both `net48` and `net10.0` |
| `src/mcp-server/` | External production tools and host support; official SDK supplies MCP parsing/dispatch/transport. `CancellableInputStream.cs` is an attributed adaptation of C# SDK 2.2.0 code |
| `src/excel-report/` | Separate local ClosedXML exporter for the manual command; workbook output is not an MCP tool capability |
| `scripts/` and `tests/` | Original orchestration, recovery and evidence checking; SDK test client consumes the official package |

Read [MCP provenance](../research/mcp-sdk-dependencies.md), [Excel dependencies](../research/closedxml-dependency.md), [third-party notices](../../THIRD_PARTY_NOTICES.md) and project notices/locks before modifying packages. TypeScript SDK **1.26.0 is a learning precedent only**: it is not copied or installed as a bridge dependency. Do not describe all code as handwritten from scratch or all code as SDK reuse. Keep the modern SDK and ClosedXML dependency graph out of Revit's shared load context; preserve the exporter process boundary and do not redistribute Autodesk API DLLs or RVT assets.

## Reproduce in stages

### Inspect and build

Use the [prerequisites](../../projects/revit-readiness/docs/development-prerequisites.md). Full Revit 2024, Git, the selected .NET SDK, net48 references, a .NET 10 runtime and 64-bit PowerShell 7 are needed for the full local workflow. Python is required by the independent verifiers, settings helpers and runner; Python 3.12.10 is the recorded local interpreter, not a claimed minimum. Do not use optimized Python (`-O`/`PYTHONOPTIMIZE`) for assertion-based evidence checks.

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Check-Environment.ps1
python --version
pwsh -NoProfile -Command '$PSVersionTable.PSVersion; [Environment]::Is64BitProcess'
dotnet --list-runtimes
```

`Check-Environment` checks the basic Revit build inputs, not every Python, desktop, permissions or runtime prerequisite. Verify the selected desktop applications separately. Obtain missing software from official sources within the user's authorization. Inventory is not host acceptance.

With Revit closed, verify the intended source candidate:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-McpBridge.ps1 -Suite All
pwsh -NoProfile -File projects/revit-readiness/scripts/Test-Readiness.ps1
pwsh -NoProfile -File projects/revit-readiness/scripts/Build-McpBridge.ps1
```

`All` includes Deployment, Tools, Dispatch, Pipe, Framing and Protocol work. Do not run installation, undo, Deployment or All while Revit is open. Use focused suites when investigating an isolated boundary; a build or offline test is not proof of actual Autodesk scheduling or desktop interpretation. Locked restores preserve the selected dependency graph. A build updates outputs, not the installed candidate.

### Prepare installation, recovery and client settings

Follow the [installation/recovery procedure](../../projects/revit-readiness/docs/mcp-bridge-setup.md) and [host acceptance checklist](../../projects/revit-readiness/tests/mcp-host-acceptance.md). After selecting and verifying a candidate, with Revit closed:

```powershell
pwsh -NoProfile -File projects/revit-readiness/scripts/Install-Readiness.ps1 -IncludeMcpBridge
```

This mutates the local installation; do not execute it merely to explain the repo. Keep the prepared recovery receipt and backup bytes private. `-IncludeFixtureTools` is a separate opt-in editing capability needed only for sample preparation. For an existing lab installation, inspect receipt lineage, source package and installed hashes first; do not silently replace a verified installed build with current build outputs. Undo uses `Undo-McpBridgeInstall.ps1 -ReceiptPath <actual-receipt>` under PowerShell 7, with Revit closed and the original verified filesystem view.

The server defaults to `%USERPROFILE%/.bim-agent-lab/RevitReadiness/McpServer/BimAgentLab.RevitReadiness.Mcp.exe`. The add-in remains under the Revit 2024 per-user Addins directory. Earlier packaged-app filesystem redirection made apparently identical AppData paths refer to different files. Verify actual opened-file locations and access from Revit and both desktop consumers; same-view hashes alone are insufficient. Never reinterpret an old recovery receipt in a different filesystem view.

Configure the owned `bim-agent-lab-revit-readiness` entry using the prepared/apply/restore workflow in `scripts/manage_mcp_probe_settings.py`; inspect `--help` and the [desktop launch record](../../projects/revit-readiness/docs/verification/2026-10-21-desktop-launch-preparation.md). Select the readiness name explicitly: its default is the synthetic probe. Preserve unrelated entries and credentials. Keep full settings backups out of Git. Use an observed current Revit PID and new per-client wire-evidence roots; configuration is not an installer side effect. A restarted Revit process needs a fresh binding and context, not the historical PID.

### Prepare samples and independent answers

Use the [required-information fixture recipe](../../projects/revit-readiness/samples/model-recipes/required-information.md) and original Autodesk sample. Keep originals unchanged; prepare disposable copies, matching sidecars and native schedule exports. The fixture command edits these copies deliberately; it is separate from normal read-only readiness. Reuse an existing verified sample set when appropriate, checking its bytes and identities first.

Follow [desktop verification](verify-readiness-through-desktop-agents.md) to prepare NC1/NC2 and freeze the expected answer before querying. NC1 substitutes an unresolved declared identity; NC2 removes the ACT type requirement. They are valid incomplete checklists, not malformed-input tests. Record independent NC1 absence evidence before acquiring the test context. Do not manufacture expectations from the report being tested. Bind each expectation to current PID/session, model/checklist paths and hashes, native evidence and the authored oracle. On a new fixture, derive new identities from that fixture; the author's captured IDs are not portable inputs.

Have the human open the exact full path and choose **Add-Ins → External Tools → BIM Agent Lab – Start readiness bridge**. Acquire context and verify the returned path before a check. Provide one concrete UI action with expected observation at a time. The user operates Revit and trust prompts; Computer Use is unnecessary for this workflow.

### Capture and interpret

Use the same neutral prompt in the [participant guide](../start-here.md#4-ask-a-useful-question-and-check-the-answer) for both actual desktop applications. Retain tool discovery, context, result/error, negotiated revision and the human-facing answer. Wire client names do not identify Claude's visible Chat/Code interface; record UI facts separately.

For a separate SDK check, build `tests/mcp-client/BimAgentLab.Mcp.Client.csproj` with `-c Release -p:RestoreLockedMode=true`, then use `scripts/Invoke-ReadinessMcp.ps1` with `-Case`, `-ExpectationPath`, a new `-EvidenceDirectory`, and the current `-RevitPid`. See the [exact runner procedure](verify-readiness-through-desktop-agents.md#run-one-prepared-case-through-the-sdk). It checks acquired context before releasing the SDK reader, then verifies the whole report/wire exchange. It is not a replacement for actual desktop acceptance. Keep failed attempts; retry in a new directory.

Principal expectations are 0/2/3 findings affecting 0/6/8 instances. NC1 requires `CoverageComplete=false`, `Evaluation.Complete=true`, 15 of 16 examined and the exact absent-element scope issue. NC2 requires both flags false and the exact two ACT instances unexamined for `NoMatchingRequirement`. Preserve false flags, raw values, requirements, exclusions and element identities. A test comparison can pass because defects or incomplete coverage were correctly detected; that does not make the model ready.

For stale-context evidence, open both documents before starting the five-minute token window. Capture the old context, record the human's actual document switch, use that exact old token without refreshing, and require `ContextChanged` before its expiry. Then verify fresh-context recovery independently. `ContextExpired` is a different result and cannot prove the switch guard. Retain UTC transition observations, failures and raw traffic. Prefix snapshots from open streams are not EOF evidence. After human closure without saving, compare model/checklist hashes separately from in-memory modified flags.

## Preserve the reasoning learned in review

- **Operation lifetime:** carry the original deadline and authoritative invalidation through the final Revit read guard. Admission, response completion, executor quiescence, callback completion and disposal are different facts.
- **Execution and waiting:** cancellation can run borrowed callbacks that throw or remain held. Network cleanup must progress independently while retained work stays bounded and resources remain alive. Do not register executor work on the private transport signal. Keep exception cleanup reachable. See the [execution model](../explanation/bridge-execution-and-waiting.md).
- **Argument handoff:** recognized-tool arguments must be fully accepted or safely rejected before SDK conversion. Rejection cannot revisit any rejected tree. Preserve request identity/deadline, zero bridge admission on rejection, and SDK ownership of protocol-envelope errors. See the [argument boundary](../explanation/readiness-argument-boundary.md).
- **Evidence:** distinguish a passing fake, Windows process test, Autodesk-boundary double, live host observation and actual desktop exchange. If several major review findings recur, ask the independent reviewer whether a shared missing model explains them, verify that diagnosis, and test transitions and combinations beyond the reported examples. See the [review procedure](run-spec-plan-and-implementation-reviews.md).

## Resume the author's remaining work

Latest follow-up: [shutdown/recovery](../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md) passed actual queued application shutdown, three physical disposals, diagnostic-layer restore/reapply/restore and fresh normal-host wrapper recovery. Revit was left open with PID `154284`, session `380928ed403d4fbf9731b2bbebb145e3`; inspect actual state before reuse. Desktop bindings were not changed for this SDK-only session. The installed normal host is again `9a93f6d`, server `d88de67`; runner source is `3b74f67`, diagnostic preparation source `32c44ec`. Both new diagnostic receipts are Restored. These identify different artifacts; inspect hashes/lineage before deployment. The [night checkpoint](../build-story/2026-10-21-night-checkpoint.md) retains earlier shared recovery history; its pause instruction was superseded. Private `.superpowers` state is not a participant prerequisite.

| Next work | Completion evidence still needed |
|---|---|
| Positive runner observation | Passed on fresh diagnostic and restored normal hosts; retained full report/wire comparisons and original independent oracle |
| Task 7 host lifetime gaps | Queued actual shutdown and three physical disposals now observed; concurrent reader/native Raise overlap at final shutdown remains unproven and must respect Revit's single API thread |
| Task 7 cancellation limitation | Intermittent queued-notification test explained and corrected in the [shutdown preparation](../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md). [Token-only SDK cancellation is now diagnosed](../../projects/revit-readiness/docs/verification/2026-10-22-sdk-cancellation-diagnosis.md): a cancelable wait can remove the SDK notification callback before it runs. Not fixed; retain the failing propagation probe and successful explicit/EOF controls. Desktop Stop behavior is unverified |
| Recovery lineage | New three-target diagnostic layers actually restored, including repeat restore; original three shared whole-install layers now actually restored and repeated, followed by exact reinstall. New receipt ef71fb608bba47f49a43ee75fb226bec is Committed; prior three are Restored. Preserve every backup/filesystem context; fresh post-reinstall Revit read passed16/0/bothcomplete/no edits |
| Acceptance and R3 | Consolidate A1–A9 against exact source/installed hashes and fresh required checks; prepare the external [R3 packet](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) for the author, without claiming or sending external review |
| Distribution | MIT and public source access verified October 26 after distribution review; release packaging remains open |

Complete the authorized work sequentially; the author wants refinement after the remaining steps, not an extra approval pause after every increment. Prepare offline work before requesting the next short Revit session. Do not mark Tasks 7–9 or overall acceptance complete from these onboarding documents.

October 22 author decision: [continue with explicit cancellation](../superpowers/reviews/revit-mcp-bridge/2026-10-22-cancellation-disposition.md), retain the SDK defect/probe for R3 and future upgrades, and do not fork/patch or silently upgrade the SDK. This is not a waiver of cancellation acceptance or evidence for desktop Stop buttons. Revit was subsequently observed closed; five installed readiness-server processes remained active when full shared-layer recovery preparation began. Verify fresh process state before any installation mutation.

The author subsequently closed Claude and disabled Codex readiness MCP. [Actual shared recovery](../../projects/revit-readiness/docs/verification/2026-10-22-shared-recovery-execution.md) restores all three historical layers, repeats each undo in a fresh process, and reinstalls the immutable package. All 97 final hashes and new recovery bytes verify; installed-server SDK discovery/BridgeUnavailable pass on all three revisions. New committed receipt ef71fb608bba47f49a43ee75fb226bec supersedes the old three for current recovery. Fresh Revit startup/read now passes with PID178492/session705886aad28f45cf9b22df62da642aae; inspect actual state before future reuse. Preserve the first Protocol test-driver timeout, the unchanged focused pass and full passing repeat; do not label a retest a fix. After the live pass, both readiness entries were rebound from149972 to178492 with private durable backups; only the quoted PID bytes changed, with unrelated parsed settings and Codex enabled=false preserved. Claude was not launched; no post-rebind desktop call is claimed. The user can reconnect normally. Treat the new PID-edit receipts separately from older registration/removal receipts; do not replay the latter over a changed entry.

For any extension, use the [extension workflow](extend-a-project.md): a scoped BIM question, source research, independent expected cases, reviewed spec/plan, focused implementation, appropriate tests, evidence and Git checkpoint. Record participant prompts/decisions in one layer and exact technical procedure/provenance in the other. Distinguish actual quotations from suggested prompts, update navigation and the build journal, and report material unverified behavior plainly.
