[CmdletBinding()]
param([ValidateSet('Protocol','Framing','Pipe','All')][string]$Suite = 'Protocol')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
try {
    if ($Suite -in @('Pipe','All')) {
        & dotnet build 'tests/bridge-tests/BimAgentLab.Bridge.Tests.csproj' -c Release --nologo -p:RestoreLockedMode=true
        if ($LASTEXITCODE -ne 0) { throw 'Bridge dual-target build failed.' }
        $pipeRun = Join-Path (Get-Location) ('samples/local/pipe-evidence/' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $pipeRun -Force | Out-Null
        $pipeLog = Join-Path $pipeRun 'pipe-output.txt'
        $pipeFailures = @()
        foreach ($framework in @('net48','net10.0')) {
            "PIPE_TARGET framework=$framework" | Tee-Object -FilePath $pipeLog -Append
            & "./tests/bridge-tests/bin/Release/$framework/BimAgentLab.Bridge.Tests.exe" --pipe 2>&1 | Tee-Object -FilePath $pipeLog -Append
            if ($LASTEXITCODE -ne 0) { $pipeFailures += $framework }
        }
        if ($pipeFailures.Count -ne 0) { throw "Pipe behavioral tests failed: $($pipeFailures -join ', ')" }
        'PIPE_SUITE_COMPLETE' | Tee-Object -FilePath $pipeLog -Append
        & python tests/verify_pipe_evidence.py $pipeLog
        if ($LASTEXITCODE -ne 0) { throw 'Independent pipe evidence verification failed.' }
        Write-Output "PIPE_EVIDENCE path=$pipeRun"
    }
    if ($Suite -in @('Framing','All')) {
        & dotnet build 'tests/bridge-tests/BimAgentLab.Bridge.Tests.csproj' -c Release --nologo -p:RestoreLockedMode=true
        if ($LASTEXITCODE -ne 0) { throw 'Bridge dual-target build failed.' }
        $failedTargets = @()
        foreach ($framework in @('net48','net10.0')) {
            & "./tests/bridge-tests/bin/Release/$framework/BimAgentLab.Bridge.Tests.exe"
            if ($LASTEXITCODE -ne 0) { $failedTargets += $framework }
        }
        if ($failedTargets.Count -ne 0) { throw "Bridge behavioral tests failed: $($failedTargets -join ', ')" }
        $exchange = Join-Path (Get-Location) ('samples/local/bridge-exchange/' + [guid]::NewGuid().ToString('N'))
        foreach ($framework in @('net48','net10.0')) {
            & "./tests/bridge-tests/bin/Release/$framework/BimAgentLab.Bridge.Tests.exe" --emit (Join-Path $exchange $framework)
            if ($LASTEXITCODE -ne 0) { throw "Bridge fixture emission failed: $framework" }
        }
        & './tests/bridge-tests/bin/Release/net48/BimAgentLab.Bridge.Tests.exe' --consume (Join-Path $exchange 'net10.0')
        if ($LASTEXITCODE -ne 0) { throw 'net48 could not consume net10.0 messages.' }
        & './tests/bridge-tests/bin/Release/net10.0/BimAgentLab.Bridge.Tests.exe' --consume (Join-Path $exchange 'net48')
        if ($LASTEXITCODE -ne 0) { throw 'net10.0 could not consume net48 messages.' }
        & python tests/verify_bridge_exchange.py $exchange
        if ($LASTEXITCODE -ne 0) { throw 'Independent bridge exchange verification failed.' }
        Write-Output "BRIDGE_EXCHANGE_EVIDENCE path=$exchange"
    }
    if ($Suite -in @('Protocol','All')) {
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
    }
    Write-Output 'PASS available offline suites; desktop/Revit acceptance is separate.'
} finally { Pop-Location }
