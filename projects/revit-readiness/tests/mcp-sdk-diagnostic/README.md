# SDK compatibility diagnostic

This isolated experiment compares the official **ModelContextProtocol 2.2.0** server's default protocol option with an explicit `2026-07-28` pin. It is original lab test code using official SDK APIs. The project reference inherits the host's exact package pin; the diagnostic never calls `McpHost` or Revit and is not registered in a desktop application.

Run from the repository root:

```powershell
dotnet restore ./projects/revit-readiness/tests/mcp-sdk-diagnostic/BimAgentLab.Mcp.SdkDiagnostic.csproj --locked-mode
dotnet build ./projects/revit-readiness/tests/mcp-sdk-diagnostic/BimAgentLab.Mcp.SdkDiagnostic.csproj -c Release --no-restore
& ./projects/revit-readiness/tests/mcp-sdk-diagnostic/bin/Release/net10.0/BimAgentLab.Mcp.SdkDiagnostic.exe
```

Run the built executable as shown: it self-launches through `Environment.ProcessPath`. Each SDK client/server pair has a ten-second operation budget. Six combinations cover two server options and three pinned client revisions. Accepted connections must discover the expected tool, negotiate the exact revision, and return synthetic success and structured failure with explicit false/true error flags. Rejected cases must contain the expected initialization error. Output is a JSON report; exit 0 requires every expected outcome.

This tests SDK behavior, not the production host's shutdown/recovery guarantees, actual desktop behavior, or live Revit results. It is separate from `Test-McpBridge.ps1` and cannot satisfy CP0. See the [research record](../../../../docs/research/mcp-desktop-protocol-investigation.md), [participant lesson](../../../../docs/tutorials/07-test-desktop-compatibility-first.md), and [agent procedure](../../../../docs/how-to/verify-desktop-mcp-compatibility.md).
