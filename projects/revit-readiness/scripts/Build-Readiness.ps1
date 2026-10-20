[CmdletBinding()]
param([switch]$IncludeFixtureTools, [string]$RevitInstallDir = "$env:ProgramW6432\Autodesk\Revit 2024")
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Check-Environment.ps1') -RevitInstallDir $RevitInstallDir
Push-Location (Join-Path $PSScriptRoot '..')
try {
    $projects = @('src/revit-addin/BimAgentLab.RevitReadiness.csproj')
    if ($IncludeFixtureTools) { $projects += 'tests/fixture-tools/BimAgentLab.FixtureTools.csproj' }
    foreach ($project in $projects) {
        & dotnet build $project -c Release "-p:RevitInstallDir=$RevitInstallDir" --nologo
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
        foreach ($api in 'RevitAPI.dll','RevitAPIUI.dll') {
            if (Test-Path (Join-Path (Split-Path $project) "bin/Release/net48/$api")) { throw "Autodesk API must not be copied to output: $api" }
        }
    }
} finally { Pop-Location }
