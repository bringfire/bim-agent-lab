[CmdletBinding()]
param([ValidateSet('Protocol','All')][string]$Suite = 'Protocol')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
try {
    foreach ($project in @('tests/mcp-probe/BimAgentLab.Mcp.Probe.csproj','tests/mcp-tests/BimAgentLab.Mcp.Tests.csproj','tests/mcp-client/BimAgentLab.Mcp.Client.csproj')) {
        & dotnet build $project -c Release --nologo -p:RestoreLockedMode=true
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
    }
    & './tests/mcp-tests/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe' './tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe' './samples/local/mcp-protocol-tests'
    if ($LASTEXITCODE -ne 0) { throw 'MCP Protocol behavioral tests failed.' }
    & python -m unittest discover -s tests -p 'test_mcp_*.py' -v
    if ($LASTEXITCODE -ne 0) { throw 'MCP independent checker/settings recovery tests failed.' }
    $runRoot = Join-Path (Get-Location) ('samples/local/mcp-sdk-client/' + [guid]::NewGuid().ToString('N'))
    $wire = Join-Path $runRoot 'wire'
    $probe = (Resolve-Path 'tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe').Path
    & './tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe' --server $probe --server-arg '--wire-evidence' --server-arg $wire --scenario probe --evidence $runRoot
    if ($LASTEXITCODE -ne 0) { throw 'Standalone SDK client failed.' }
    $sessions = @(Get-ChildItem -LiteralPath $wire -Directory)
    if ($sessions.Count -ne 1) { throw 'Expected one captured SDK session.' }
    & python tests/verify_mcp_transcript.py $sessions[0].FullName --protocol 2026-07-28
    if ($LASTEXITCODE -ne 0) { throw 'Independent wire comparison failed.' }
    Write-Output "SDK_CLIENT_EVIDENCE $runRoot"
    Write-Output 'PASS available offline suites; desktop/Revit acceptance is separate.'
} finally { Pop-Location }
