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
    $probe = (Resolve-Path 'tests/mcp-probe/bin/Release/net10.0/BimAgentLab.Mcp.Probe.exe').Path
    foreach ($case in @(
        @{ Protocol = '2025-06-18'; Strict = $false },
        @{ Protocol = '2025-11-25'; Strict = $false },
        @{ Protocol = '2026-07-28'; Strict = $false },
        @{ Protocol = '2026-07-28'; Strict = $true }
    )) {
        $runRoot = Join-Path (Get-Location) ('samples/local/mcp-sdk-client/' + [guid]::NewGuid().ToString('N'))
        $wire = Join-Path $runRoot 'wire'
        $clientArguments = @('--server', $probe, '--server-arg', '--wire-evidence', '--server-arg', $wire,
            '--scenario', 'probe', '--evidence', $runRoot, '--protocol', $case.Protocol)
        if ($case.Strict) { $clientArguments += @('--server-arg', '--require-protocol', '--server-arg', '2026-07-28') }
        & './tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe' @clientArguments
        if ($LASTEXITCODE -ne 0) { throw 'Standalone SDK client failed.' }
        $sessions = @(Get-ChildItem -LiteralPath $wire -Directory)
        if ($sessions.Count -ne 1) { throw 'Expected one captured SDK session.' }
        & python tests/verify_mcp_transcript.py $sessions[0].FullName --protocol $case.Protocol
        if ($LASTEXITCODE -ne 0) { throw 'Independent wire comparison failed.' }
        Write-Output "SDK_CLIENT_EVIDENCE protocol=$($case.Protocol) strict=$($case.Strict) path=$runRoot"
    }
    Write-Output 'PASS available offline suites; desktop/Revit acceptance is separate.'
} finally { Pop-Location }
