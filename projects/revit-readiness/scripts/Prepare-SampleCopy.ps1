[CmdletBinding()]
param(
    [string]$SourceSample = "$env:ProgramW6432\Autodesk\Revit 2024\Samples\Snowdon Towers Sample Architectural.rvt",
    [string]$DestinationFolder = (Join-Path $PSScriptRoot '..\samples\local\required-information')
)
$ErrorActionPreference = 'Stop'
$source = (Resolve-Path -LiteralPath $SourceSample).Path
$destination = [System.IO.Path]::GetFullPath($DestinationFolder)
$target = Join-Path $destination 'Snowdon-BAL-source-copy.rvt'
if (Test-Path -LiteralPath $target) { throw 'Source copy already exists. Use a fresh destination folder; existing fixtures are not overwritten.' }
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -LiteralPath $source -Destination $target
$sourceHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $sourceHash) { throw 'Copied sample hash mismatch.' }
[ordered]@{Source=$source; Copy=$target; Sha256=$sourceHash; CopiedUtc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json
