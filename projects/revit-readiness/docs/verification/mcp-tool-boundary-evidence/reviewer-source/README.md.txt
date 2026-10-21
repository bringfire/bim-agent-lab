# Independent Task 6 review reproductions

Reviewed repository: D:/bim-agent-lab at 189fd78841f07f3b8b4ad999c7ed32aa3a6a36f1.
The repository was not modified. These are isolated reviewer fixtures, not live Revit evidence.

## MCP input EOF while a bridge request remains pending

Build: dotnet build ./eof/EofReview.csproj -c Release
Run: ./eof/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe eof
Tracing control: ./eof/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe eof trace
Positive cancellation control: ./eof/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe cancel

The harness publishes its own restricted Windows pipe descriptor and starts the actual production MCP executable with that explicit PID. After initialize and a tools/call reach the endpoint, it closes the MCP child's stdin while retaining Response and Quiesced. It checks process exit and the actual runtime request's final-read guard before releasing synthetic execution. Finally it releases the ticket, waits for natural child exit, stops the endpoint and removes its own descriptor. No Revit instance is used.

Observed at reviewed HEAD:
- EOF: no exit after 5013ms; request not canceled; ThrowIfInactive permits execution; about 24.8 seconds of the original budget remain. Releasing the response lets the child exit 0.
- EOF with tracing: same failure after 5007ms.
- Explicit MCP cancellation before EOF: natural exit within 15ms; bridge context canceled; final-read guard rejects execution while response is still held.

Evidence: eof/eof-output.txt, eof/eof-trace-output.txt, eof/cancel-output.txt.
The failing cases exit 1; the explicit cancellation control exits 0.

## Invalid Unicode contextToken

Run: python ./arguments_review.py

Uses the actual production MCP executable with an absent explicit PID and raw JSON stdio messages. Expected tool-level InvalidRequest for lone high and low Unicode surrogate escapes; observed JSON-RPC -32603 instead, without structuredContent or bridge request correlation. Wrong-type and oversized inputs correctly return InvalidRequest. Opaque ASCII and a valid surrogate pair reach BridgeUnavailable as expected. Process remains alive and exits 0; harness exits 1 with two assertion failures.

Evidence: arguments-rerun-output.txt and arguments-wire-results.json.

## Existing verification

Test-McpBridge.ps1 -Suite All passed, including 35 Tools cases and all compatible/strict-July, dispatch, pipe and foundation suites. Test-Readiness.ps1 passed, including Excel and process isolation. Build-Readiness.ps1 completed with zero warnings/errors. Retained Task 6 evidence hashes matched. Git diff --check passed; checkout remained clean.

