[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
if(-not $IsWindows){throw 'Compatibility tests require Windows (net48 and Windows bridge runtime).'}
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    python -m unittest discover -s tests -p test_revit_targets.py -v
    if($LASTEXITCODE -ne 0){throw 'Revit year/host binding checks failed.'}
    dotnet build tests/compatibility-tests/BimAgentLab.RevitCompatibility.Tests.csproj -c Release --nologo
    if($LASTEXITCODE -ne 0){throw 'Compatibility harness build failed.'}
    $evidence=Join-Path (Get-Location) ('samples/local/compatibility-tests/'+[guid]::NewGuid().ToString('N'))
    foreach($target in 'net48','net10.0') {
        & "./tests/compatibility-tests/bin/Release/$target/BimAgentLab.RevitCompatibility.Tests.exe" (Join-Path $evidence $target)
        if($LASTEXITCODE -ne 0){throw "Compatibility behavior failed: $target"}
    }
    python -c 'import json,sys,pathlib; p=pathlib.Path(sys.argv[1]); names=("checklist.json","report.json","adapter-report.json"); assert all(json.loads((p/"net48"/n).read_text(encoding="utf-8-sig"))==json.loads((p/"net10.0"/n).read_text(encoding="utf-8-sig")) for n in names); print("PASS independent net48/net10 semantic JSON comparison")' $evidence
    if($LASTEXITCODE -ne 0){throw 'Independent JSON comparison failed.'}
    Write-Output "COMPATIBILITY_EVIDENCE $evidence"
} finally {Pop-Location}
