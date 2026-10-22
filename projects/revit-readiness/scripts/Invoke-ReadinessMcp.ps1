[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('complete','wall-defects','ceiling-negative','nc1','nc2')][string]$Case,
    [Parameter(Mandatory)][string]$ExpectationPath,
    [Parameter(Mandatory)][string]$EvidenceDirectory,
    [Parameter(Mandatory)][ValidateRange(1,2147483647)][int]$RevitPid,
    [string]$ServerPath,
    [string]$ClientPath,
    [ValidateSet('2025-06-18','2025-11-25','2026-07-28')][string]$Protocol = '2026-07-28',
    [ValidateRange(1,600)][int]$TimeoutSeconds = 450
)
$ErrorActionPreference = 'Stop'
if (-not $ServerPath) {
    $ServerPath = Join-Path $env:USERPROFILE '.bim-agent-lab/RevitReadiness/McpServer/BimAgentLab.RevitReadiness.Mcp.exe'
}
if (-not $ClientPath) {
    $ClientPath = Join-Path $PSScriptRoot '../tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe'
}
& python (Join-Path $PSScriptRoot 'invoke_readiness_mcp.py') `
    --case $Case --expectation $ExpectationPath --output $EvidenceDirectory `
    --server $ServerPath --client $ClientPath --revit-pid $RevitPid `
    --protocol $Protocol --timeout-seconds $TimeoutSeconds
if ($LASTEXITCODE -ne 0) {
    throw 'Readiness capture did not pass. Inspect the retained attempt/logs; use a new directory for retry.'
}
