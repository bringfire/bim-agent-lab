# Verify readiness through both desktop agents

Participant companion: [Test the answer, not just the connection](../tutorials/14-test-readiness-through-desktop-agents.md). Binding requirements: [plan Task 8](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) and [specification](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md). Results: [live comparison record](../../projects/revit-readiness/docs/verification/mcp-readiness.md).

## Preserve the installed baseline

Use the existing installed server, explicit current Revit PID and per-application wire capture directories. Verify the actual version/configuration baseline; do not infer UI mode from MCP `clientInfo`. The author used Claude Desktop's Code tab for the first continuity answer and Chat for the second, both with the same wire client name. The five-case workflow targets Chat as the beginner-facing interface.

For this run the author reports Claude Desktop Chat's AI model as **Opus 5.5**, permissions **Auto**. Preserve these as human-reported UI settings, distinct from the wire client `local-agent-mode` and negotiated protocol. Auto is not evidence that every individual tool grant was Always allow. Ask for the **AI model label in Claude**, not merely “the model,” which is ambiguous in a Revit workflow.

Do not install, undo, or run Deployment/All while Revit is open. Native execution and the compatibility amendment are already authorized. Official C# SDK 2.2.0 remains pinned. Production accepts the verified desktop revisions, and separate July `2026-07-28` SDK tests remain required. No TypeScript SDK 1.26.0 code or package is introduced here.

## Freeze expectations before calling the tools

`projects/revit-readiness/tests/verify_mcp_evidence.py` builds the oracle from authored rule values, original checklist identities, and native schedule TSVs. It does not call the production evaluator. Historical reports appear only as observed fixtures in comparator tests.

Preserve raw empty strings separately from null. Native schedule display alone cannot distinguish them. The fixture's `SetText` leaves an already-empty optional parameter untouched, but explicitly clears populated required values with `Set("")`; the oracle records that source-supported distinction. Compare full reports, including all identities, rule/parameter identifiers, requirement sources, units, exclusions, raw values and both completeness flags.

Record model/checklist paths and SHA-256 values, expected report, source hashes, preparation time, current PID and bridge session before any new readiness call. Controls must be prepared before acquiring context. Use a new output directory:

```powershell
& ./projects/revit-readiness/scripts/Prepare-McpControls.ps1 `
  -SourceDirectory <prepared-principal-models> -OutputDirectory <new-control-directory>
```

The helper writes a Prepared ownership receipt before copying, verifies both model copies and changed sidecars, then writes a Ready marker. NC1 replaces only the WA-01 declared UniqueId; independently confirm the injected identity is absent in Revit before marking its expectation confirmed. NC2 removes only the ACT requirement. Cleanup uses `-Cleanup` with the same directory and checks owned paths/hashes. Changed or foreign files prevent cleanup; preserve them for inspection. Interrupted preparation and cleanup are covered by subprocess tests.

For the human absence check, provide the complete Revit 2024 procedure: **Manage → Inquiry → Select by ID**, enter `2147483647`, then **Show** ([Autodesk instructions](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-Troubleshooting/files/GUID-2B1CC22C-CB1F-45DA-B57B-62C36013D9E0.htm)). Record the exact result; inability to find a useful view would not establish absence. On October 22 Revit said, “The following Element IDs are invalid and won't be used: 2147483647;”. Preserve the author-supplied screenshot unchanged, its limits, the coordinated model context and matching source/control hashes. Keep the original Prepared receipt immutable because the Ready marker hashes it. Add the later confirmation as separate evidence and update the NC1 expectation before its first context call; retain the prior expectation too.

## Coordinate the live calls

1. Ask the person to open the exact prepared path, keeping the bridge running and the model unchanged. Prefer pasting the full path into Open over a recent-file tile. Acquire context, then compare its path, checklist hash, PID and session to the frozen expectation. If they differ, retain the context as an incomplete setup attempt; do not quietly change the expected path or count it as acceptance.
2. Call the check with that fresh context. Retain complete tool results and unchanged wire bytes. Create an attempt record before calls and a successful comparison marker only after the request/response, wire and independent report checks finish. Retry in a new directory, retaining earlier failed attempts.
3. Ask the person to run a neutral prompt in Claude Chat, without supplying expected findings. Obtain both the actual traffic and their pasted answer. Check the report independently and check whether the answer describes its scope and completeness faithfully.
4. Also run the existing official SDK readiness client with `--protocol 2026-07-28` against the same installed server/model. This is additional protocol evidence, never a substitute for either desktop.
5. Repeat the three principal samples and NC1/NC2. After normal close, compare RVT hashes with pre-call values; retain modified-before/after flags as separate evidence.
6. For stale context, acquire a context in each desktop, switch to a different document before token expiry, and call with the old token without reacquiring. Require correlated `ContextChanged`, no report, then fresh-context recovery. Record times so expiry is not mistaken for document-switch protection.

Open both models before starting this timer, then have the person use Revit's **Switch Windows** menu to activate a view in the other document. Use the acquired context's actual full path as the source identity. If it differs from the intended source, preserve the unsuccessful setup and record any revised direction before the switch. The live Claude repeat went ceiling → complete after a purported complete activation still yielded ceiling. Keep the first `ContextExpired` attempt without a comparison-complete marker; expiry does not prove document-switch detection.

The raw streams may still be open. Snapshot complete-line byte prefixes and label them as prefixes, not EOF evidence. Use actual byte reads, not potentially stale directory-entry lengths. Match the chosen calls by their structured request IDs; a session can contain earlier calls. Preserve the original bytes and record hashes.

Retain human-supplied native schedule images unchanged and hash them. Record only visible observations and their limits. The wall screenshot corroborates six case rows with blank required values; it does not display the full document path, element UniqueIds, ceiling coverage or API null semantics. Tie it to the coordinated inspection and separate path/identity evidence rather than inventing information from the image.

Keep the human-facing answer separate from the structured tool result. Both must be assessed: wire/report equality proves data arrived intact; the pasted response establishes whether the agent interpreted completeness, missing values and exclusions correctly. The wall and ceiling Chat answers now have both forms of evidence. Do not count a plausible pasted answer as proof of a new tool call without its matching recorded exchange.

## Verification and limits

### Run one prepared case through the SDK

`projects/revit-readiness/scripts/Invoke-ReadinessMcp.ps1` now packages the capture and independent comparison into one command. This is a repository verification tool for the five authored sample cases, not a general-purpose readiness verdict for arbitrary projects. It consumes the existing official C# SDK client; it does not implement MCP or replace either actual desktop test.

Before running, build the existing SDK client with locked dependencies, have the person open the intended prepared sample and start the installed bridge, and prepare an independent expectation for that **current** PID/session. The expectation contains case, preparation UTC, model/checklist absolute paths and hashes, current PID/session, source hashes and the full `expectedReport`. Use the authored `expected_report` oracle and original native/checklist sources, never a newly returned report. NC1 additionally needs `absenceConfirmed=true` and `absenceEvidence` with the independent observation's absolute path/hash. Do not reuse October 22's historical PID/session after restarting Revit.

Example, with an already prepared expectation and a **new** output directory:

```powershell
& ./projects/revit-readiness/scripts/Invoke-ReadinessMcp.ps1 `
  -Case complete `
  -ExpectationPath 'D:\my-captures\complete-expectation.json' `
  -EvidenceDirectory 'D:\my-captures\complete-attempt-01' `
  -RevitPid <current-Revit-process-ID>
```

Replace the example paths and PID; they are not supplied files. `-Protocol` defaults to `2026-07-28` and also accepts the two verified older revisions. `-ServerPath` defaults to the installed shared server under `%USERPROFILE%\.bim-agent-lab\RevitReadiness\McpServer`; `-ClientPath` defaults to this checkout's Release/net10.0 SDK client. Neither is built or installed implicitly. `-TimeoutSeconds` defaults to 450, with a 600-second maximum. Run ordinary Python 3 without `-O`/`PYTHONOPTIMIZE`: the independent verifier uses assertions, and the runner explicitly refuses optimized Python.

The PowerShell wrapper forwards argument values without building a shell command. Its Python helper performs these stages:

1. Validate the prepared oracle, current PID binding, source/model/checklist/proof hashes, executable paths and fresh output directory. Invalid preparation produces no capture and launches no client. It does not silently update a mismatched expectation.
2. Write exact frozen expectation bytes and a durable `prepared.json` before launch. Record command, original budget, selected revision, and executable/adjacent application-DLL hashes. These identify the invoked applications; they are not a replacement for the full installation/dependency inventory.
3. Start only its own SDK client, using the existing `--pause-file` support. The SDK captures context and waits. Check actual document/checklist paths, hash, session/PID, context semantics and expiry against the prepared expectation; recheck input bytes. Only then write the owned release file to permit the readiness call. A wrong active model cannot reach that call through this runner.
4. Wait for SDK exit and successful `complete.json`, which the driver writes after transport disposal. Recheck inputs, construct capture metadata, and run the unchanged full report/wire verifier. Require a report generated during this invocation; a prior capture cannot become a fresh result merely by copying it.
5. Write `capture/comparison-complete.json` only after verification and before the original budget expires. SDK wait time is not restarted after acquisition. Synchronous hashing/comparison is not forcibly interrupted; an expired budget prevents success publication. On error/cancellation, retire the owned SDK process and retain failure stage/cause plus any cleanup error. No Revit or desktop process is terminated. An abruptly killed runner may leave only its prepared/partial records; those are not success and must not be overwritten by a retry.

The output root keeps `expectation.json`, `prepared.json`, SDK stdout/stderr, context-validation/release records and, on handled failure, `failed.json`. Its `capture/` subdirectory uses the existing evidence layout and can be replayed with `verify_mcp_evidence.py <attempt>/capture --case <case>`. `complete.json` means the SDK exchange passed; `comparison-complete.json` means the independent expected result matched. For wall defects that means expected missing fields were found; for NC1/NC2 it means incomplete coverage was preserved. Neither is a model-readiness pass. Saved-file checks after human close remain a separate step.

Runner tests use a clearly labelled subprocess fixture derived from retained captures. They exercise the real orchestration and comparator, while the subprocess substitutes for live Revit output. A separate real PowerShell/SDK unavailable-host control runs with Revit closed. Those offline checks alone do not establish live success. Subsequent [shutdown/recovery verification](../../projects/revit-readiness/docs/verification/2026-10-22-shutdown-preparation.md) supplies fresh positive wrapper comparisons on diagnostic and restored normal hosts. See the original [runner verification record](../../projects/revit-readiness/docs/verification/2026-10-22-readiness-runner.md) for the earlier scope.

```powershell
python projects/revit-readiness/tests/test_mcp_readiness_runner.py
```

`Test-McpBridge.ps1 -Suite Protocol` and `-Suite All` discover this test module through their existing Python test pattern. Keep the actual desktop workflow above separate.

```powershell
python projects/revit-readiness/tests/test_verify_mcp_evidence.py
python projects/revit-readiness/tests/test_mcp_case_wire.py
python projects/revit-readiness/tests/test_prepare_mcp_controls.py
python projects/revit-readiness/tests/test_mcp_stale_evidence.py
python projects/revit-readiness/tests/verify_mcp_evidence.py <capture-directory> --case wall-defects
```

Capture directories contain frozen `expectation.json`, `capture.json` selecting context/report request IDs and wire hashes, `context.json`, `check.json`, and both `wire/*.jsonl` files. Comparison rejects unknown protocol revisions, unmatched wire results, wrong token use, a changed frozen oracle and incomplete NC1 absence confirmation. Stale-context wire checking alone is insufficient: the coordinated switch/timing and subsequent recovery must also be evidenced.

For a stale capture, `verify_stale(directory)` additionally requires the original frozen expectation; `transition.json` with `contextReceivedUtc`, `switchConfirmedUtc`, `checkStartedUtc`, `checkFinishedUtc`, `humanAction` and `recoveryCase`; and a `recovery/` directory that independently passes the ordinary report verifier. Record UTC observations as they happen, not retrospectively from protocol lines. The observer's finish time must precede the original token expiry; the recovered report must follow the refusal and use a different document path, lifetime and token within the same Revit PID/bridge session. Synthetic checker tests are explicitly labelled and cannot stand in for actual desktop calls.

Use the real error contract when verifying refusals. Successful results have `contractVersion`; domain errors from `ReadinessTools.Failure` have exactly `code`, `message`, `requestId`, and `retryable`. The original synthetic stale fixture mistakenly added the success member to its error. Live evidence exposed that checker assumption; the corrected positive fixture first failed, then the checker was corrected at the error boundary. For the `ContextChanged` case require `retryable=false`, matching structured/text content, the original request correlation and no report. This was a verification-code correction, not a production server change.

Principal expectations are 0/0, 2/6 and 3/8 findings/affected instances. NC1 has 16 requested, 15 examined, zero findings, 1,197 excluded host instances and flags `(CoverageComplete=false, Evaluation.Complete=true)`. NC2 has 16 examined, zero findings, two exact ACT instances unexamined, 1,196 excluded host instances and both flags false. Each excludes six links. Neither incomplete case is a readiness pass.

Update both teaching layers and the evidence matrix as observations arrive. Keep Task 7 gaps, five-case A8 and full R3 status separate. Do not convert a partial live checkpoint into task closure.
