[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if (Get-Process Revit -ErrorAction SilentlyContinue) {
    throw 'Close Revit before installing or updating the probe. This script does not close applications.'
}
$sourceDir = Join-Path $PSScriptRoot '..\src\revit-addin'
$dll = Join-Path $sourceDir 'bin\Release\net48\BimAgentLab.RevitProbe.dll'
if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) { throw 'Build the Release probe before installing it.' }
$addinsDir = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
$destinationDir = Join-Path $addinsDir 'BimAgentLab.ReadOnlyProbe'
$manifestPath = Join-Path $addinsDir 'BimAgentLab.ReadOnlyProbe.addin'
if (Test-Path -LiteralPath $manifestPath) {
    [xml]$existing = Get-Content -LiteralPath $manifestPath -Raw
    if ($existing.RevitAddIns.AddIn.AddInId -ne '8B3FA065-26DF-4076-A194-43C1457C81AF') {
        throw 'An unrelated manifest already uses this filename; it has not been overwritten.'
    }
}
New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
$destinationDll = Join-Path $destinationDir 'BimAgentLab.RevitProbe.dll'
Copy-Item -LiteralPath $dll -Destination $destinationDll -Force
$template = Get-Content -LiteralPath (Join-Path $sourceDir 'BimAgentLab.ReadOnlyProbe.addin.template') -Raw
$manifest = $template.Replace('__ASSEMBLY_PATH__', [System.Security.SecurityElement]::Escape($destinationDll))
[System.IO.File]::WriteAllText($manifestPath, $manifest, [System.Text.UTF8Encoding]::new($false))
if ((Get-FileHash -LiteralPath $dll).Hash -ne (Get-FileHash -LiteralPath $destinationDll).Hash) {
    throw 'Installed DLL hash does not match the build output.'
}
Write-Output "Installed manifest: $manifestPath"
Write-Output "Installed DLL: $destinationDll"
Write-Output 'Start Revit 2024, open a project, then use Add-Ins > External Tools > BIM Agent Lab - Read-only probe.'
