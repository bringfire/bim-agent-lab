# Task 7 real installation and initial Revit observations

Later scope clarification: the [diagnostic startup investigation](2026-10-21-host-diagnostics.md#physical-path-investigation-established-the-cause) found 30 installed server targets in Codex's private AppData cache. Installation/recovery hash observations below belong to that filesystem view; the Revit add-in itself resolved to its real installed location. These results do not establish that both desktop clients can access the installed server. Preserve all recorded observations, but complete shared-server visibility and recovery-context verification before Task 7 closure and A8.

Date: 2026-10-21. Installed source checkpoint: `9a93f6d29bf3c46972701db53bfaee598ec4fd0c`. This follows the [offline deployment verification](2026-10-21-recoverable-installation.md). Task 7 remains open until its remaining host checks and independent review are accounted for. SDK-harness observations are not desktop A8 acceptance.

Retained evidence: [capture index](live-installation-evidence/capture-index.json), [SHA-256 inventory](live-installation-evidence/hashes.json), [installed target hashes](live-installation-evidence/installed-second-verification.json), [original snapshot](live-installation-evidence/before-install.json), [first undo](live-installation-evidence/after-undo-first.json) and [second undo](live-installation-evidence/after-undo-second.json). The index separates 38 successful SDK captures from failed/partial attempts and failed interpretations of otherwise valid reports. The manual workbook is retained; actual models and backup/staged binary bytes remain outside Git.

From the repository root, reproduce the evidence checks without running Revit:

```powershell
python projects/revit-readiness/tests/verify_live_lifecycle_capture.py projects/revit-readiness/docs/verification/live-installation-evidence
python projects/revit-readiness/tests/test_verify_live_lifecycle_capture.py
```

The first command independently checks actual wire revision, catalog, correlated response uniqueness, text/structured/saved-result agreement, exact expected outcomes, restoration snapshots and session/document identities. Twenty-four checker tests challenge those properties, the connected observation window, edit freshness, token timing, Stop versus frame expiry, workbook freshness and recorded post-close hashes. These checks do not establish unobserved UI actions, reader-entry counts, or the Task 8 BIM oracle.

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

## Save As, fresh values and restored test state

The author reported “BAL-info-saved-as is now saved.” A new context on that file retained the project identity but changed the document path and lifetime; a full report returned zero findings with both modified flags false. The pre-Save As token had expired before the manual step finished. Its `ContextExpired` result is retained but **does not isolate fresh-token Save As invalidation**. See the [observation](live-installation-evidence/save-as-observation.json).

On this disposable copy, the agent used native Type Properties to change wall A's Type Mark from `W-A` to `W-A-LIVE`. The same pre-edit context token returned the new value, with modified state true before and after the query. A Ctrl-Z attempt did not restore it; the resulting report correctly still contained `W-A-LIVE`. Closing without saving this deliberately introduced edit, then reopening the saved file, restored `W-A` and false/false modified flags. Both the unsuccessful attempt and [verified restoration](live-installation-evidence/value-edit-restoration.json) remain recorded. An initial comparison script searched for the UI label `TypeMark`; the report's contract calls that field `Code`. Correcting the selector used the actual declared schema, not a changed expected value.

Stop then removed the session descriptor. Restart issued a new session, and the prior token returned `ContextExpired` **before its recorded expiry time**. Unknown/revoked tokens share this error code by contract. This distinguishes restart invalidation from ordinary five-minute expiry.

## Coordinated fresh-token Save As

The author requested manual UI operation, then replied “ready for save as test” and “saved” at the coordinated checkpoints. This separate run used Revit PID **178136**, build `24.3.60.12`, and session `68aef270a7a04f21991b184b55f6762a`; the earlier PID 82836 observations remain historical. The agent used SDK/file operations only. It prepared the absent `BAL-info-saveas-fresh.rvt` sidecar before context acquisition, changing only `ProjectFileName` from the source `BAL-info-saved-as.rvt` checklist. Both sidecar snapshots and hashes are retained in [the new run](live-installation-evidence/save-as-fresh-20261021T213613Z/preparation.json).

The pre-save context completed at **21:36:14.056 UTC** and expired at **21:41:14.026 UTC**. After the user's save, checking that exact token completed at **21:37:49.604 UTC** with correlated `ContextChanged` and no report: **204.4 seconds before expiry**. This distinguishes document/checklist identity invalidation from ordinary token expiry. It does not isolate the Save As event handler from the other identity guards, because both path and prepared checklist binding changed.

A fresh context/report completed at **21:40:01.946 UTC** on `BAL-info-saveas-fresh.rvt`. The process, session and project ID stayed the same; the document lifetime changed from `7a843d7b448242b396a980c3d0d67a25` to `ceb21d161bba4db1b859b0a8c271f540`. The report contains 16 requested/examined instances, six types, 18 field results, zero findings/affected elements, both completeness flags true and both modified flags false. Every report field matches the earlier restored sample report except generation time and the two expected paths. That is a continuity check, not the independent Task 8 BIM oracle. After the author replied "closed", no Revit process or bridge descriptor remained. The source RVT matched its prior closed-file hash and both checklists matched their prepared bytes. The new saved model hash was recorded as `f55856b361cf92aee57d6ea939523d8f280cd3543ee8afa62ad39d3174601f72`; it is a new post-save baseline, not a claim that Save As preserves the original file bytes. See [normal close and hashes](live-installation-evidence/save-as-fresh-20261021T213613Z/normal-close-and-hashes.json).

The checker validates the original token on the wire, response completion before expiry, stable session/project, changed path/lifetime, exact checklist bytes and the fresh report. Seven corrupted controls initially failed against an unimplemented checker, then all passed after implementation: late response, wrong token, unchanged lifetime, changed session, unchanged path, changed modified state and an extra checklist change despite consistent replacement hashes. All **24 checker tests** and **38 retained SDK capture checks** pass. The earlier expired Save As attempt remains intact. Production source remains `9a93f6d`; no build, install, new package or desktop configuration change was needed.

## Connected timeout, incomplete connections and normal shutdown

The SDK test driver now accepts `--finish-file`: after the expected response it keeps its SDK connection alive until a separate release file appears, observes for at least three further seconds, and verifies the catalog again on the same connection. Two subprocess tests first failed against the missing option and then passed; a wire-path assumption in the test was corrected separately. Protocol regression, including compatible revisions and strict July/legacy rejection, passed. These are test-only changes; the installed production build remains `9a93f6d`.

In [modal-deadline-connected](live-installation-evidence/modal-deadline-connected/sdk-client-result.json), the request expired while Open remained visibly held. After cancellation of that UI dialog, the client stayed connected for 3,005 ms and completed its final catalog call. Independent wire checking found one correlated deadline response and no duplicate or unsolicited late response. This is a bounded traffic observation, **not a reader-entry count**.

The [pipe observer](../../tests/Observe-LivePipeStop.ps1) connected three actual private-pipe clients: no bytes, a partial header, and a partial body. Native Stop produced EOF in 282–286 ms and descriptor cleanup within a further 101 ms, while Revit remained running. The first observer incorrectly checked descriptor absence immediately at EOF and failed; source inspection confirmed descriptor removal follows draining all workers. The corrected observer separately waits up to two seconds for cleanup. The original failed capture stays intact. A no-Stop negative control produced EOF only at 5,004–5,012 ms, retained the descriptor and correctly failed the Stop claim. A subsequent restart and full report passed.

Finally, native Alt-F4 closed the application normally; the process exited and the descriptor was absent. No valid request was deliberately held during this normal-shutdown control, so it does not establish in-flight callback shutdown. Fourteen [post-close hash comparisons](live-installation-evidence/final-closed-model-hashes.json) matched the six original source files, their six working copies, the same-identity copy and the saved-as file's separately recorded closed baseline.

## Existing manual outputs still work

The installed manual Required type information command produced JSON, text and the four-sheet Excel workbook from the restored saved-as copy. JSON matched the prior MCP report except its generation timestamp. The independent Python ZIP/XML reader checked workbook freshness, model/checklist identity, scope counts, exclusions, false modified flags, empty findings/affected sheets and all 18 rule rows against the JSON. Text retained all six types and 16 case IDs. See [comparison and hashes](live-installation-evidence/manual-export/comparison.json) and the [workbook](live-installation-evidence/manual-export/BAL-info-saved-as.readiness.xlsx). The three principal samples and their native-schedule oracle remain Task 8 work.

## Remaining gates at the follow-up checkpoint

| Gate | Observation at this checkpoint |
|---|---|
| Real installation, exact undo twice, reinstall | Observed; private backup bytes retained |
| Explicit Start, repeated Start/Stop, fresh restart session | Observed, including rejection of an unexpired token from the stopped session |
| No project, family, unsaved project, checklist controls | Expected error results observed; reader-entry instrumentation not included |
| Copied project switch, close/reopen, changed checklist, expired token | Expected invalidation observed |
| Save As, same-document value edits | Fresh-token Save As invalidation observed before expiry in a coordinated manual retry; fresh report verified. Same-token fresh value and restoration observed |
| Modal request expiry and subsequent request | Connected three-second post-release window has no late wire result; internal reader-entry evidence pending |
| Actual cancellation/disconnect during preparation or an executing read | Pending; offline tests are separate evidence |
| Stop/shutdown with queued/executing work, partial/idle connections | Stop with three incomplete input states and ordinary application shutdown observed; queued/executing shutdown remains pending |
| Every native Raise outcome, event retirement and Idling unhook | Not instrumented in this production build |
| Two actual Revit instances | Not exercised |
| Manual JSON/text/Excel command after installation | Observed and compared on the restored saved-as complete copy |
| Three samples, NC1/NC2 independent comparison, both desktops | Task 8; not established by this SDK lifecycle run |

Diagnostic instrumentation, if used later, must be identified and hashed separately from this installed production build. No Task 7/R3 closure is asserted here.

See the [host checklist](../../tests/mcp-host-acceptance.md), [participant story](../../../../docs/tutorials/13-install-with-a-recovery-plan.md), and [agent procedure](../mcp-bridge-setup.md).
