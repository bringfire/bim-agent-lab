[CmdletBinding()]
param([switch]$IncludeFixtureTools, [string]$FixtureSourceCopy = (Join-Path $PSScriptRoot '..\samples\local\required-information\Snowdon-BAL-source-copy.rvt'))
$ErrorActionPreference = 'Stop'
if (Get-Process Revit -ErrorAction SilentlyContinue) { throw 'Close Revit before installing or updating readiness commands.' }
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$addins = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
function Install-LabCommand($name, $assemblyName, $source, $id, $className, $text, $availability) {
    $manifest = Join-Path $addins "$name.addin"
    if (Test-Path -LiteralPath $manifest) {
        [xml]$old = Get-Content -LiteralPath $manifest -Raw
        if ($old.RevitAddIns.AddIn.AddInId -ne $id) { throw "Unrelated manifest exists: $manifest" }
    }
    $destination = Join-Path $addins $name
    foreach ($dll in $assemblyName, 'BimAgentLab.Readiness.Core.dll') {
        if (-not (Test-Path -LiteralPath (Join-Path $source $dll))) { throw "Build before installing: $dll" }
    }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    foreach ($dll in $assemblyName, 'BimAgentLab.Readiness.Core.dll') {
        Copy-Item -LiteralPath (Join-Path $source $dll) -Destination (Join-Path $destination $dll) -Force
        if ((Get-FileHash -LiteralPath (Join-Path $source $dll)).Hash -ne (Get-FileHash -LiteralPath (Join-Path $destination $dll)).Hash) { throw "Installed DLL mismatch: $dll" }
    }
    $assemblyPath = [System.Security.SecurityElement]::Escape((Join-Path $destination $assemblyName))
    $xml = "<?xml version=`"1.0`" encoding=`"utf-8`"?><RevitAddIns><AddIn Type=`"Command`"><Assembly>$assemblyPath</Assembly><AddInId>$id</AddInId><FullClassName>$className</FullClassName><Text>$text</Text><VendorId>BIAL</VendorId><VendorDescription>BIM Agent Lab</VendorDescription>$availability<AllowLoadIntoExistingSession>false</AllowLoadIntoExistingSession></AddIn></RevitAddIns>"
    [System.IO.File]::WriteAllText($manifest,$xml,[System.Text.UTF8Encoding]::new($false))
    Write-Host "Installed: $manifest"
}
Install-LabCommand 'BimAgentLab.RequiredInformation' 'BimAgentLab.RevitReadiness.dll' (Join-Path $projectRoot 'src/revit-addin/bin/Release/net48') '63D5AF32-6B13-4E91-9C5D-9B76048B4231' 'BimAgentLab.RevitReadiness.RequiredInformationCommand' 'BIM Agent Lab - Required type information' '<AvailabilityClassName>BimAgentLab.RevitReadiness.ProjectAvailable</AvailabilityClassName><VisibilityMode>NotVisibleInFamily</VisibilityMode><VisibilityMode>NotVisibleWhenNoActiveDocument</VisibilityMode>'
if ($IncludeFixtureTools) {
    $sourceCopy = (Resolve-Path -LiteralPath $FixtureSourceCopy).Path
    Install-LabCommand 'BimAgentLab.FixtureTools' 'BimAgentLab.FixtureTools.dll' (Join-Path $projectRoot 'tests/fixture-tools/bin/Release/net48') 'BA818F63-D7CE-43CC-90B9-CE39E2DF6C49' 'BimAgentLab.FixtureTools.PrepareInformationFixture' 'BIM Agent Lab - Prepare teaching sample copies' ''
    @{SourceCopy=$sourceCopy} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $addins 'BimAgentLab.FixtureTools/fixture-settings.json') -Encoding UTF8
}
