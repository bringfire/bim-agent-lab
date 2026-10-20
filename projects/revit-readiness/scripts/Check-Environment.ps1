[CmdletBinding()]
param([string]$RevitInstallDir = "$env:ProgramW6432\Autodesk\Revit 2024")

$ErrorActionPreference = 'Stop'
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
$revitExe = Join-Path $RevitInstallDir 'Revit.exe'
if ((Test-Path -LiteralPath $revitExe) -and (Get-Item -LiteralPath $revitExe).VersionInfo.FileMajorPart -ne 24) {
    $missing += 'RevitInstallDir must point to Revit 2024'
}
[ordered]@{
    checkedUtc = [DateTime]::UtcNow.ToString('o')
    selectedSdk = $sdk
    git = $(if (Get-Command git -ErrorAction SilentlyContinue) { & git --version } else { $null })
    files = @($observed)
    missing = @($missing)
    note = 'Inventory only. Build and Revit host acceptance are separate checks.'
} | ConvertTo-Json -Depth 5
if ($missing.Count -gt 0) { throw 'Required development inputs are missing; see the report above.' }
