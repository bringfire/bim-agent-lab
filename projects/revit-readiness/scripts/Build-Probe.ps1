[CmdletBinding()]
param([string]$RevitInstallDir,[ValidateSet('2024','2026')][string]$RevitYear='2024')

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RevitTarget.ps1')
$target=Get-RevitTarget -RevitYear $RevitYear -RevitInstallDir $RevitInstallDir
$RevitInstallDir=$target.RevitInstallDir
& (Join-Path $PSScriptRoot 'Check-Environment.ps1') -RevitInstallDir $RevitInstallDir -RevitYear $RevitYear
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & dotnet build 'src/revit-addin/BimAgentLab.RevitProbe.csproj' -c Release "-p:RevitInstallDir=$RevitInstallDir" "-p:RevitYear=$RevitYear" --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }
    foreach ($name in 'RevitAPI.dll', 'RevitAPIUI.dll') {
        if (Test-Path -LiteralPath (Join-Path "src/revit-addin/$($target.OutputRelative)" $name)) {
            throw "Unexpected Autodesk library in output: $name. Check Copy Local settings."
        }
    }
} finally { Pop-Location }
