# Task 7 real installation and initial Revit observations

Date: 2026-10-21. Installed source checkpoint: `9a93f6d29bf3c46972701db53bfaee598ec4fd0c`. This follows the [offline deployment verification](2026-10-21-recoverable-installation.md). Task 7 remains open until its remaining host checks and independent review are accounted for. SDK-harness observations are not desktop A8 acceptance.

Retained text evidence: [capture index](live-installation-evidence/capture-index.json), [SHA-256 inventory](live-installation-evidence/hashes.json), [installed target hashes](live-installation-evidence/installed-second-verification.json), [original snapshot](live-installation-evidence/before-install.json), [first undo](live-installation-evidence/after-undo-first.json) and [second undo](live-installation-evidence/after-undo-second.json). The index explicitly separates 26 successful captures from the failed/partial attempts. Actual model and backup/staged binary bytes remain outside Git.

From the repository root, reproduce the evidence checks without running Revit:

```powershell
python projects/revit-readiness/tests/verify_live_lifecycle_capture.py projects/revit-readiness/docs/verification/live-installation-evidence
python projects/revit-readiness/tests/test_verify_live_lifecycle_capture.py
```

The first command independently checks actual wire revision, catalog, correlated response uniqueness, text/structured/saved-result agreement, exact expected outcomes, restoration snapshots and session/document identities. Nine checker tests include deliberately missing/duplicate/unsolicited responses, wrong protocol, wrong expected outcome, altered saved/text data, false completion, changed restoration state and lost document lifetime. These checks do not establish unobserved UI actions, reader-entry counts, or the Task 8 BIM oracle.

## Installation and recovery

With Revit closed, the agent rebuilt the reviewed candidate and recorded SHA-256 hashes plus directory inventories for the entire existing per-user Revit 2024 add-ins tree and the initially absent modern-server directory. The snapshot covered 59 original files, including unrelated probe files and the legacy fixture installation.

The first real installation prepared receipt `abc3286c81554bf89841c13a92ed14bc`, committed, and matched all 97 planned target hashes. Undo ran in a new PowerShell process; independent Python snapshot logic matched every original file hash, directory inventory and server-directory absence. A second fresh-process undo produced the same exact result. Reinstallation prepared receipt `2251ddf785b5466ea47ac3935249a4b2`, committed and again matched all 97 targets. No Autodesk API or modern MCP SDK assemblies were present in the Revit-host payload. Original backup/staged bytes remain in private per-user recovery storage, outside Git.

A later [read-only recheck](live-installation-evidence/checkpoint-input-and-binary-recheck.json) again matched all 97 installed targets and the six original model/sidecar source files. Revit was still open; this does not claim final post-close hashes for the two open working models.

## Initial actual-host observations

Revit 2024.3, build `24.3.60.12`, process 82836, was launched through Computer Use. No unsigned-add-in prompt was observed in this launch. At Home, before Start, no bridge descriptor existed and the installed SDK-driven server returned correlated `BridgeUnavailable`.

- Start created exactly one descriptor. A live context request with no project returned `NoProject`.
- Start again retained exactly the same descriptor/session.
- Stop removed the owned descriptor; a subsequent SDK request returned `BridgeUnavailable`.
- Stop again produced no descriptor or observed error dialog.
- Start after Stop created one fresh session, distinct from the first.
- A verified disposable copy of `BAL-info-complete.rvt` returned a full report: 16 requested/examined instances, zero findings and affected elements, both completeness flags true, and `ModifiedBefore`/`ModifiedAfter` false. This initial result is not yet the Task 8 independent three-case comparison.

The first SDK attempt used an incorrect shortened executable filename and failed before connecting. That failed capture is retained. The next attempt used the actual installed `BimAgentLab.RevitReadiness.Mcp.exe` and passed; no production change was needed. The lesson is to distinguish a test-invocation failure from server behavior.

## Checklist and copied-document controls

The complete sample's disposable sidecar was backed up, deliberately changed for each control, then restored in `finally`. Fresh context calls returned `ChecklistMissing` for absence; `ChecklistInvalid` for empty, over-four-MiB, malformed JSON and invalid UTF-8; and `ChecklistMismatch` for a foreign project ID, filename, missing level and missing phase. A prior valid token used after adding only a newline to the sidecar returned `ContextChanged`. After restoration, a fresh full report succeeded. No original sample sidecar was changed.

A further model copy retained identical saved bytes and project/type identities, with only its sidecar filename binding deliberately adjusted. The client acquired context on the first model, paused, then sent its check while Revit's Open dialog was held. There was no response at the recorded observation. Opening the second copy released scheduling; the request returned correlated `ContextChanged`. The test records the actual document transition and wire result; it does not expose an internal queue-admission event.

PowerShell's direct hash read of the opened model was denied by Revit's sharing mode. The new closed copy was instead compared with the retained before-open hash. An initial slash-specific record selector missed the stored Windows path and falsely failed that comparison; using filename parsing corrected the verification script, and the hashes matched. Neither failure justified changing model bytes or weakening the identity assertion.

## Modal delay, reopening and unsupported documents

A readiness request sent while Revit's Open dialog was held returned `DeadlineExceeded` before the dialog closed. After closing the dialog, a fresh request returned a report. This proves deadline response and subsequent recovery. The SDK client had disposed before the dialog closed, so this capture does **not** prove absence of a late reader entry or capture a continuously connected client's later traffic. Those require a coordinated observation at the read boundary, not an inference from an error response.

Closing and reopening the same saved copy rejected its old token with `ContextChanged`. A fresh context retained the same path, project identity and checklist hash but had a new document lifetime. The closed model's hash matched its before-open copy hash. A later request using an actually expired five-minute token returned `ContextExpired`.

A disposable copy of Autodesk's `rac_basic_sample_family.rfa` returned `UnsupportedDocument`; a new unsaved project returned `UnsavedProject`. The first attempted family check ran while the Open dialog was still held after a failed UI action. It returned `DeadlineExceeded`, failing its expected-family assertion. That capture is retained as a setup failure. After observing the family loaded in the Family Editor, a new capture passed. A failed UI action must not silently become evidence for the intended document state.

## Remaining gates

| Gate | Observation at this checkpoint |
|---|---|
| Real installation, exact undo twice, reinstall | Observed; private backup bytes retained |
| Explicit Start, repeated Start/Stop, fresh restart session | Observed; old-context-after-restart check still pending |
| No project, family, unsaved project, checklist controls | Expected error results observed; reader-entry instrumentation not included |
| Copied project switch, close/reopen, changed checklist, expired token | Expected invalidation observed |
| Save As, same-document value edits | Pending |
| Modal request expiry and subsequent request | Observed; continuously connected no-late-success and reader-entry evidence pending |
| Actual cancellation/disconnect during preparation or an executing read | Pending; offline tests are separate evidence |
| Stop/shutdown with queued/executing work, partial/idle connections | Pending |
| Every native Raise outcome, event retirement and Idling unhook | Not instrumented in this production build |
| Two actual Revit instances | Not exercised |
| Manual JSON/text/Excel command after installation | Pending |
| Three samples, NC1/NC2 independent comparison, both desktops | Task 8; not established by this SDK lifecycle run |

Diagnostic instrumentation, if used later, must be identified and hashed separately from this installed production build. No Task 7/R3 closure is asserted here.

See the [host checklist](../../tests/mcp-host-acceptance.md), [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md), and [agent procedure](../mcp-bridge-setup.md).
