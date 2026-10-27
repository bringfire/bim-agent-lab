[CmdletBinding()]
param([string]$RevitInstallDir,[ValidateSet('2024','2026')][string]$RevitYear='2024')

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RevitTarget.ps1')
$target=Get-RevitTarget -RevitYear $RevitYear -RevitInstallDir $RevitInstallDir
$RevitInstallDir=$target.RevitInstallDir
$missing = @()
$frameworkDir = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
$files = @(
    (Join-Path $RevitInstallDir 'Revit.exe'),
    (Join-Path $RevitInstallDir 'RevitAPI.dll'),
    (Join-Path $RevitInstallDir 'RevitAPIUI.dll'),
    (Join-Path $frameworkDir 'mscorlib.dll'),
    (Join-Path $frameworkDir 'System.dll'),
    (Join-Path $frameworkDir 'System.Core.dll')
)
$observed = foreach ($path in $files) {
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $item = Get-Item -LiteralPath $path
        [ordered]@{ path = $item.FullName; fileVersion = $item.VersionInfo.FileVersion; exists = $true }
    } else {
        $missing += $path
        [ordered]@{ path = $path; exists = $false }
    }
}
foreach ($command in 'git', 'dotnet') {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) { $missing += $command }
}
$sdk = $null
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Push-Location (Join-Path $PSScriptRoot '..')
    try {
        $sdk = (& dotnet --version 2>&1 | Out-String).Trim()
        if ($LASTEXITCODE -ne 0) { $missing += 'SDK matching projects/revit-readiness/global.json' }
    } finally { Pop-Location }
}
foreach($revitFile in $files[0..2]) {
    if((Test-Path -LiteralPath $revitFile) -and (Get-Item -LiteralPath $revitFile).VersionInfo.FileMajorPart -ne ([int]$RevitYear-2000)) {
        $missing += "RevitInstallDir must point to Revit $RevitYear with matching API DLLs"
    }
}
$revitRuntimeFramework=$null
if($RevitYear -eq '2026') {
    $runtimePath=Join-Path $RevitInstallDir 'RevitAPI.runtimeconfig.json'
    if(Test-Path -LiteralPath $runtimePath) {
        try {$revitRuntimeFramework=(Get-Content -LiteralPath $runtimePath -Raw | ConvertFrom-Json).runtimeOptions.tfm}
        catch {$missing+='Invalid Revit runtime configuration'}
    }
    if($revitRuntimeFramework -ne 'net10.0') {$missing+='Revit 2026.5+ with net10.0 runtime is required; earlier Revit 2026 is outside this target'}
}
[ordered]@{
    checkedUtc = [DateTime]::UtcNow.ToString('o')
    selectedSdk = $sdk
    revitYear = $RevitYear
    hostTargetFramework = $target.Framework
    revitRuntimeFramework = $revitRuntimeFramework
    git = $(if (Get-Command git -ErrorAction SilentlyContinue) { & git --version } else { $null })
    files = @($observed)
    missing = @($missing)
    note = 'Inventory only. Build and Revit host acceptance are separate checks.'
} | ConvertTo-Json -Depth 5
if ($missing.Count -gt 0) { throw 'Required development inputs are missing; see the report above.' }
