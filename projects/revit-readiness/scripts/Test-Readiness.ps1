[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & dotnet build 'tests/core/BimAgentLab.Readiness.Tests.csproj' -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Readiness test build failed.' }
    & '.\tests\core\bin\Release\net48\BimAgentLab.Readiness.Tests.exe'
    if ($LASTEXITCODE -ne 0) { throw 'Readiness behavior tests failed.' }
    & dotnet build 'tests/excel/BimAgentLab.Readiness.Excel.Tests.csproj' -c Release --nologo -p:RestoreLockedMode=true
    if ($LASTEXITCODE -ne 0) { throw 'Excel test build failed.' }
    & '.\tests\excel\bin\Release\net48\BimAgentLab.Readiness.Excel.Tests.exe' 'docs/verification/required-information-evidence' 'tests/excel/bin/outputs'
    if ($LASTEXITCODE -ne 0) { throw 'Excel report tests failed.' }
    & dotnet build 'tests/host-load/BimAgentLab.Excel.HostLoad.csproj' -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Host-load test build failed.' }
    $excelAssembly = (Resolve-Path 'src/excel-report/bin/Release/net48/BimAgentLab.Readiness.Excel.exe').Path
    $report = (Resolve-Path 'docs/verification/required-information-evidence/BAL-info-wall-defects.readiness.json').Path
    $output = [IO.Path]::GetFullPath((Join-Path (Get-Location) 'tests/excel/bin/outputs/isolated-host.xlsx'))
    & '.\tests\host-load\bin\Release\net48\BimAgentLab.Excel.HostLoad.exe' $excelAssembly $report $output
    if ($LASTEXITCODE -ne 0) { throw 'Isolated host-load test failed.' }
} finally { Pop-Location }
