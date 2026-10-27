[CmdletBinding()]
param([switch]$IncludeFixtureTools,[string]$RevitInstallDir,[ValidateSet('2024','2026')][string]$RevitYear='2024')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RevitTarget.ps1')
$target=Get-RevitTarget -RevitYear $RevitYear -RevitInstallDir $RevitInstallDir
$RevitInstallDir=$target.RevitInstallDir
& (Join-Path $PSScriptRoot 'Check-Environment.ps1') -RevitInstallDir $RevitInstallDir -RevitYear $RevitYear
Push-Location (Join-Path $PSScriptRoot '..')
try {
    $projects = @('src/revit-addin/BimAgentLab.RevitReadiness.csproj')
    if ($IncludeFixtureTools) { $projects += 'tests/fixture-tools/BimAgentLab.FixtureTools.csproj' }
    foreach ($project in $projects) {
        $restoreOptions = @()
        if ($project -like '*RevitReadiness.csproj') { $restoreOptions += '-p:RestoreLockedMode=true' }
        & dotnet build $project -c Release "-p:RevitInstallDir=$RevitInstallDir" "-p:RevitYear=$RevitYear" --nologo @restoreOptions
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
        foreach ($api in 'RevitAPI.dll','RevitAPIUI.dll') {
            if (Test-Path (Join-Path (Split-Path $project) "$($target.OutputRelative)/$api")) { throw "Autodesk API must not be copied to output: $api" }
        }
    }
} finally { Pop-Location }
