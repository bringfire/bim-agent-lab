# Independent follow-up at 332934c

Reviewed commit: 332934cb144148a9d8af79fe57cd745ac110f6a0. Repository remained clean.

## Original findings and unchanged reproducers

The three original source files (arguments_review.py, eof/Program.cs, eof/EofReview.csproj) match the retained reviewer-source evidence byte-for-byte (SHA-256 checked). No original probe source was changed.

- Original EOF probe: PASS, cancellation and final-read rejection while the response/executor are held; process exits in approximately 15ms.
- Original EOF probe with tracing: PASS, approximately 16ms.
- Original explicit MCP cancellation control: PASS.
- Original Unicode probe: PASS, six controls, zero assertion failures. Both lone surrogates produce correlated InvalidRequest. Process exits naturally with code 0.

Evidence: eof-output.txt, eof-trace-output.txt, cancel-output.txt, arguments-output.txt.
T6-01 is independently closed. The SDK-before-typed-handler diagnosis is confirmed; the original two T6-02 string cases are fixed.

## Remaining P2: rejected argument shapes can still fail before validation

Run from this directory: python shape_unicode_review.py

The supplementary probe invokes the actual production executable over raw MCP stdio (2025-11-25), without Revit. Five of eight cases fail the expected domain-error contract:

- contextToken is an array containing an unpaired surrogate.
- contextToken is an object containing an unpaired surrogate.
- Valid string contextToken plus an extra argument containing an unpaired surrogate.
- Context-acquisition tool receives an extra argument containing an unpaired surrogate.
- Both the string contextToken and an extra argument contain unpaired surrogates.

All five return JSON-RPC -32603 instead of structured InvalidRequest. Wrong-type/extra-argument controls with well-formed text return InvalidRequest, and a subsequent valid call reaches BridgeUnavailable. The process survives and exits 0; the harness exits 1 with five assertion failures.

ReadinessTools.ValidateIncomingArguments only preflights a string contextToken of the check tool. The SDK subsequently converts the entire parameter object before CallAsync, so other rejected shapes and malformed values can still fail before ordinary validation. Even marking and replacing the bad contextToken does not make the rest of a rejected arguments object safe to convert. The boundary needs to account for all rejected argument shapes of both tools before SDK conversion, while preserving valid input unchanged and rejecting before bridge admission.

Evidence: shape-unicode-output.txt, shape-unicode-results.json. Original reproducer files remain unchanged.

## Verification

Test-McpBridge.ps1 -Suite All completed with exit 0: Tools 50, Dispatch 87 net48 / 82 net10, adapter handoffs 10, Pipe 53 per runtime, foundation 33 per runtime, compatible and strict July protocol regressions. Test-Readiness.ps1 and Build-Readiness.ps1 passed; the Autodesk-reference build had zero warnings/errors. All 438 retained evidence hashes and lengths across eleven matching inventories verified. git diff --check passed.

Task 6 remains open for the remaining P2. This review does not establish installation, live Revit behavior or desktop acceptance.
