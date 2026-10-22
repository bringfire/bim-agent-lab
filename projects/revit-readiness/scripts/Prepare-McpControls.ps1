[CmdletBinding()]
param(
    [string]$SourceDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [switch]$Cleanup
)
$ErrorActionPreference = 'Stop'
$helper = Join-Path $PSScriptRoot 'prepare_mcp_controls.py'
if ($Cleanup) {
    & python $helper cleanup --output $OutputDirectory
} else {
    if (-not $SourceDirectory) { throw 'SourceDirectory is required for preparation.' }
    & python $helper prepare --source $SourceDirectory --output $OutputDirectory
}
if ($LASTEXITCODE -ne 0) { throw 'MCP control preparation/cleanup failed; inspect the retained ownership record.' }
