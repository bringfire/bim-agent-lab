[CmdletBinding()]
param([string]$RevitInstallDir = "$env:ProgramW6432\Autodesk\Revit 2024")

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Check-Environment.ps1') -RevitInstallDir $RevitInstallDir
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & dotnet build 'src/revit-addin/BimAgentLab.RevitProbe.csproj' -c Release "-p:RevitInstallDir=$RevitInstallDir" --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }
    foreach ($name in 'RevitAPI.dll', 'RevitAPIUI.dll') {
        if (Test-Path -LiteralPath "src/revit-addin/bin/Release/net48/$name") {
            throw "Unexpected Autodesk library in output: $name. Check Copy Local settings."
        }
    }
} finally { Pop-Location }
