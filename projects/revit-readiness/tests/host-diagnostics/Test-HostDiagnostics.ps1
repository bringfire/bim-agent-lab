[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
& dotnet build (Join-Path $project 'tests/diagnostic-tests/BimAgentLab.Diagnostics.Tests.csproj') -c Release --nologo -p:RestoreLockedMode=true
if ($LASTEXITCODE -ne 0) { throw 'Diagnostic control test build failed.' }
& (Join-Path $project 'tests/diagnostic-tests/bin/Release/net48/BimAgentLab.Diagnostics.Tests.exe')
if ($LASTEXITCODE -ne 0) { throw 'net48 diagnostic controls failed.' }
& dotnet (Join-Path $project 'tests/diagnostic-tests/bin/Release/net10.0/BimAgentLab.Diagnostics.Tests.dll')
if ($LASTEXITCODE -ne 0) { throw 'net10 diagnostic controls failed.' }
& dotnet build (Join-Path $PSScriptRoot 'BimAgentLab.RevitReadiness.Diagnostics.csproj') -c Release --nologo -p:RestoreLockedMode=true
if ($LASTEXITCODE -ne 0) { throw 'Diagnostic Revit API build failed.' }
& dotnet build (Join-Path $project 'src/revit-addin/BimAgentLab.RevitReadiness.csproj') -c Release --nologo -p:RestoreLockedMode=true
if ($LASTEXITCODE -ne 0) { throw 'Normal Revit API build failed.' }
$normal=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes((Join-Path $project 'src/revit-addin/bin/Release/net48/BimAgentLab.RevitReadiness.dll')))
$diagnostic=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes((Join-Path $PSScriptRoot 'bin/Release/net48/BimAgentLab.RevitReadiness.dll')))
foreach($type in 'BridgeDiagnostics','DiagnosticRecorder','DiagnosticRunDirectory') {
    if ($normal.Contains($type) -or -not $diagnostic.Contains($type)) { throw 'Diagnostic type leaked into normal build or is missing from diagnostic build.' }
}
Write-Output 'PASS: diagnostic types only in separate diagnostic assembly; normal build has no diagnostic control helper.'
