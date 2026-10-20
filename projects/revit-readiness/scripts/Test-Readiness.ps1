[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & dotnet build 'tests/core/BimAgentLab.Readiness.Tests.csproj' -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Readiness test build failed.' }
    & '.\tests\core\bin\Release\net48\BimAgentLab.Readiness.Tests.exe'
    if ($LASTEXITCODE -ne 0) { throw 'Readiness behavior tests failed.' }
} finally { Pop-Location }
