# Verify readiness through both desktop agents

Participant companion: [Test the answer, not just the connection](../tutorials/14-test-readiness-through-desktop-agents.md). Binding requirements: [plan Task 8](../superpowers/plans/2026-10-20-revit-mcp-bridge.md) and [specification](../superpowers/specs/2026-10-20-revit-mcp-bridge-design.md). Results: [live comparison record](../../projects/revit-readiness/docs/verification/mcp-readiness.md).

## Preserve the installed baseline

Use the existing installed server, explicit current Revit PID and per-application wire capture directories. Verify the actual version/configuration baseline; do not infer UI mode from MCP `clientInfo`. The author used Claude Desktop's Code tab for the first continuity answer and Chat for the second, both with the same wire client name. The five-case workflow targets Chat as the beginner-facing interface.

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

## Coordinate the live calls

1. Ask the person to open the exact prepared path, keeping the bridge running and the model unchanged. Prefer pasting the full path into Open over a recent-file tile. Acquire context, then compare its path, checklist hash, PID and session to the frozen expectation. If they differ, retain the context as an incomplete setup attempt; do not quietly change the expected path or count it as acceptance.
2. Call the check with that fresh context. Retain complete tool results and unchanged wire bytes. Create an attempt record before calls and a successful comparison marker only after the request/response, wire and independent report checks finish. Retry in a new directory, retaining earlier failed attempts.
3. Ask the person to run a neutral prompt in Claude Chat, without supplying expected findings. Obtain both the actual traffic and their pasted answer. Check the report independently and check whether the answer describes its scope and completeness faithfully.
4. Also run the existing official SDK readiness client with `--protocol 2026-07-28` against the same installed server/model. This is additional protocol evidence, never a substitute for either desktop.
5. Repeat the three principal samples and NC1/NC2. After normal close, compare RVT hashes with pre-call values; retain modified-before/after flags as separate evidence.
6. For stale context, acquire a context in each desktop, switch to a different document before token expiry, and call with the old token without reacquiring. Require correlated `ContextChanged`, no report, then fresh-context recovery. Record times so expiry is not mistaken for document-switch protection.

The raw streams may still be open. Snapshot complete-line byte prefixes and label them as prefixes, not EOF evidence. Use actual byte reads, not potentially stale directory-entry lengths. Match the chosen calls by their structured request IDs; a session can contain earlier calls. Preserve the original bytes and record hashes.

## Verification and limits

```powershell
python projects/revit-readiness/tests/test_verify_mcp_evidence.py
python projects/revit-readiness/tests/test_mcp_case_wire.py
python projects/revit-readiness/tests/test_prepare_mcp_controls.py
python projects/revit-readiness/tests/verify_mcp_evidence.py <capture-directory> --case wall-defects
```

Capture directories contain frozen `expectation.json`, `capture.json` selecting context/report request IDs and wire hashes, `context.json`, `check.json`, and both `wire/*.jsonl` files. Comparison rejects unknown protocol revisions, unmatched wire results, wrong token use, a changed frozen oracle and incomplete NC1 absence confirmation. Stale-context wire checking alone is insufficient: the coordinated switch/timing and subsequent recovery must also be evidenced.

Principal expectations are 0/0, 2/6 and 3/8 findings/affected instances. NC1 has 16 requested, 15 examined, zero findings, 1,197 excluded host instances and flags `(CoverageComplete=false, Evaluation.Complete=true)`. NC2 has 16 examined, zero findings, two exact ACT instances unexamined, 1,196 excluded host instances and both flags false. Each excludes six links. Neither incomplete case is a readiness pass.

Update both teaching layers and the evidence matrix as observations arrive. Keep Task 7 gaps, five-case A8 and full R3 status separate. Do not convert a partial live checkpoint into task closure.
