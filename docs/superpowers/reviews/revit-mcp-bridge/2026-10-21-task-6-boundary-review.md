# Task 6 boundary review and author dispositions

**Subsequent independent disposition:** [Task 6 and T6-02 closed at b4cdb1f](2026-10-21-task-6-closure.md); T6-01 shutdown closure at 332934c stands. The submission/findings/dispositions below preserve their original checkpoint. Installation and live acceptance remain outstanding.

Reviewed baseline: `189fd78841f07f3b8b4ad999c7ed32aa3a6a36f1`. This feedback was supplied by the author from an independent review. Original submission: [Task 6 packet](2026-10-21-task-6-review.md). The sections below keep reviewer findings separate from implementation dispositions; a correction is not independent closure.

## Original reviewer feedback

> **Task 6 should remain open at `189fd78`. Two P2 findings remain.**
>
> 1. **Desktop disconnect does not cancel pending work.** The production entry point connects cancellation to Ctrl-C, but stdin EOF does not cancel active handlers. Through the actual executable and a real Windows pipe, five seconds after closing stdin the process remained running, the request remained active, and its final-read guard still permitted execution. This failed with tracing both enabled and disabled. Explicit MCP cancellation made the same setup exit promptly. Connect transport termination to request cancellation before waiting for handlers to finish.
> 2. **Malformed Unicode escapes the argument-error contract.** A `contextToken` containing `"\ud800"` or `"\udc00"` makes `GetString()` throw before the validation catch. The production executable returns JSON-RPC `-32603` instead of a structured, correlated `InvalidRequest`. Include string decoding within argument validation and add raw-wire regressions for both cases.
>
> I independently reran the full MCP/bridge suite, readiness/Excel/isolation checks, and Revit API build; all passed. These two additional reproductions fail. The checkout remains clean; live Revit and desktop acceptance remain unverified.

Original local reproductions: `C:/Users/bring/AppData/Local/Temp/codex-task6-independent-review/README.md`, `eof/Program.cs`, and `arguments_review.py`. Author reruns use unchanged copies in ignored `.superpowers/sdd/2026-10-20-revit-mcp-bridge/task6-review-repro`; they are author verification of reviewer-authored probes, not another independent review.

## Author dispositions

Both findings reproduced against unchanged production code. EOF in both tracing modes kept the context active after five seconds; explicit cancellation passed. Both unpaired surrogate escapes returned JSON-RPC internal errors while wrong-type/oversized controls returned InvalidRequest and valid text reached BridgeUnavailable.

The [execution model](../../../explanation/bridge-execution-and-waiting.md#task-6-review-transport-termination-precedes-handler-draining) records the missing origin and the permitted waiting order before the production correction. Existing cancellation tests covered explicit cancellation/host stop, not transport termination. Existing argument tests covered valid decoded strings and wrong shapes, not decoding failures. The common lesson is to trace each entry/exit boundary through its actual caller; neither a cancellation mechanism nor a validation catch covers paths that never reach it.

- **T6-01:** observe the SDK transport channel's terminal task alongside RunAsync, cancel the existing session before draining handlers, retain exceptional input completion, and preserve existing deferred callback retirement. No stdin wrapper rewrite, protocol implementation or deadline change.
- **T6-02:** contain string-decoding InvalidOperationException before admission. A typed-handler catch alone left all three raw-wire revision cases failing. Temporary diagnostics showed the SDK's JsonNode-to-typed-params conversion fails before handler entry; this refines the reviewer's source-location diagnosis without disputing the reported behavior. An SDK incoming-message filter marks the malformed contextToken for rejection and substitutes null only for SDK conversion. The ordinary handler consumes the server-owned marker and returns correlated InvalidRequest before admission/IPC; valid tokens remain unchanged. A local decoding catch also protects direct typed invocation. No broad catch around downstream execution and no custom protocol response.

The first expanded test run also exposed missing July client-capability metadata in the new test request. The SDK correctly rejected it before tool invocation. The test was corrected using the retained July wire format and rerun against unchanged production code: 37 pass / 13 expected behavioral failures. This failed-test correction and the later 47 pass / three Unicode failures are retained separately from the final successful checks.

Independent follow-up remains **pending**. Task 6 stays open; Task 7, installation, live Autodesk behavior, both desktops' A8 and full R3 remain outstanding. No review was sent automatically.

## Follow-up review packet

[Fresh verification and retained evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-mcp-tool-boundaries.md) record 50 passing Tools cases, unchanged reviewer probes, full regressions and the Autodesk-reference build. Review the correction commit against 189fd78. Challenge EOF and exceptional channel completion before SDK draining, held callbacks, per-message Unicode-marker isolation and rejection before IPC, unchanged valid strings, all compatible revisions and strict July. Re-run the supplied unchanged probes and raw-wire regressions. The initial July fixture mistake and handler-only failed correction remain in the evidence. This is author verification; please provide an independent finding disposition before Task 7.

Subsequent independent follow-up at 332934c: [disconnect closed, complete argument validation still open](2026-10-21-task-6-argument-contract-review.md). The leaf-specific correction above is historical and superseded by the complete argument contract.
