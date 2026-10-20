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
    $copy = @($assemblyName, 'BimAgentLab.Readiness.Core.dll') | ForEach-Object {
        [pscustomobject]@{Source=(Join-Path $source $_); Relative=$_}
    }
    if ($name -eq 'BimAgentLab.RequiredInformation') {
        $excelSource = Join-Path $projectRoot 'src/excel-report/bin/Release/net48'
        foreach ($required in 'BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config','ClosedXML.dll') {
            if (-not (Test-Path -LiteralPath (Join-Path $excelSource $required))) { throw "Excel exporter file missing. Rebuild: $required" }
        }
        $files = Get-ChildItem -LiteralPath $excelSource -File | Where-Object {$_.Extension -eq '.dll' -or $_.Name -in @('BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config')}
        if ($files.Name -contains 'RevitAPI.dll' -or $files.Name -contains 'RevitAPIUI.dll') { throw 'Autodesk API DLLs must not be distributed.' }
        foreach ($file in $files) { $copy += [pscustomobject]@{Source=$file.FullName;Relative=('Excel/' + $file.Name)} }
        $noticeSource = Join-Path $projectRoot 'third-party'
        if (-not (Test-Path (Join-Path $noticeSource 'dependency-evidence.json'))) { throw 'Dependency notices are required.' }
        foreach ($notice in Get-ChildItem -LiteralPath $noticeSource -File -Recurse) {
            $relative = $notice.FullName.Substring($noticeSource.Length + 1)
            $copy += [pscustomobject]@{Source=$notice.FullName;Relative=('third-party/' + $relative)}
        }
    }
    $previous = @()
    $inventory = Join-Path $destination 'installed-files.json'
    if (Test-Path -LiteralPath $inventory) { $previous = @(Get-Content -LiteralPath $inventory -Raw | ConvertFrom-Json) }
    $installed = @()
    foreach ($file in $copy) {
        $target = Join-Path $destination $file.Relative
        New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
        Copy-Item -LiteralPath $file.Source -Destination $target -Force
        $hash = (Get-FileHash -LiteralPath $file.Source).Hash
        if ($hash -ne (Get-FileHash -LiteralPath $target).Hash) { throw "Installed file mismatch: $($file.Relative)" }
        $installed += [pscustomobject]@{File=$file.Relative;Sha256=$hash}
    }
    # Remove only obsolete files this installer previously recorded, still byte-identical.
    foreach ($oldFile in $previous | Where-Object {$_.File -notin $installed.File}) {
        $obsolete = [IO.Path]::GetFullPath((Join-Path $destination $oldFile.File))
        if (-not $obsolete.StartsWith($destination.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid prior install path.' }
        if ((Test-Path -LiteralPath $obsolete -PathType Leaf) -and (Get-FileHash -LiteralPath $obsolete).Hash -eq $oldFile.Sha256) { Remove-Item -LiteralPath $obsolete }
    }
    $installed | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $destination 'installed-files.json') -Encoding UTF8
    $assemblyPath = [System.Security.SecurityElement]::Escape((Join-Path $destination $assemblyName))
    $xml = "<?xml version=`"1.0`" encoding=`"utf-8`"?><RevitAddIns><AddIn Type=`"Command`"><Assembly>$assemblyPath</Assembly><AddInId>$id</AddInId><FullClassName>$className</FullClassName><Text>$text</Text><VendorId>BIAL</VendorId><VendorDescription>BIM Agent Lab</VendorDescription>$availability<AllowLoadIntoExistingSession>false</AllowLoadIntoExistingSession></AddIn></RevitAddIns>"
    [System.IO.File]::WriteAllText($manifest,$xml,[System.Text.UTF8Encoding]::new($false))
    Write-Host "Installed: $manifest"
}
Install-LabCommand 'BimAgentLab.RequiredInformation' 'BimAgentLab.RevitReadiness.dll' (Join-Path $projectRoot 'src/revit-addin/bin/Release/net48') '63D5AF32-6B13-4E91-9C5D-9B76048B4231' 'BimAgentLab.RevitReadiness.RequiredInformationCommand' 'BIM Agent Lab - Required type information' '<AvailabilityClassName>BimAgentLab.RevitReadiness.ProjectAvailable</AvailabilityClassName><VisibilityMode>NotVisibleInFamily</VisibilityMode><VisibilityMode>NotVisibleWhenNoActiveDocument</VisibilityMode>'
if (-not $IncludeFixtureTools) {
    # The optional editor and checker share a backward-compatible core. Revit may
    # load either first, so update an existing recognized editor without enabling it anew.
    $fixtureManifest = Join-Path $addins 'BimAgentLab.FixtureTools.addin'
    if (Test-Path -LiteralPath $fixtureManifest) {
        [xml]$fixture = Get-Content -LiteralPath $fixtureManifest -Raw
        $fixtureFolder = Join-Path $addins 'BimAgentLab.FixtureTools'
        $expectedAssembly = Join-Path $fixtureFolder 'BimAgentLab.FixtureTools.dll'
        if ($fixture.RevitAddIns.AddIn.AddInId -ne 'BA818F63-D7CE-43CC-90B9-CE39E2DF6C49' -or
            $fixture.RevitAddIns.AddIn.Assembly -ne $expectedAssembly) { throw 'Existing fixture installation has an unexpected identity/path; coordinate its core upgrade before running Revit.' }
        $coreSource = Join-Path $projectRoot 'src/revit-addin/bin/Release/net48/BimAgentLab.Readiness.Core.dll'
        $coreTarget = Join-Path $fixtureFolder 'BimAgentLab.Readiness.Core.dll'
        Copy-Item -LiteralPath $coreSource -Destination $coreTarget -Force
        $coreHash = (Get-FileHash -LiteralPath $coreSource).Hash
        if ($coreHash -ne (Get-FileHash -LiteralPath $coreTarget).Hash) { throw 'Fixture core upgrade hash mismatch.' }
        $fixtureInventory = Join-Path $fixtureFolder 'installed-files.json'
        if (Test-Path -LiteralPath $fixtureInventory) {
            $entries = @(Get-Content -LiteralPath $fixtureInventory -Raw | ConvertFrom-Json)
            foreach ($entry in $entries) { if ($entry.File -eq 'BimAgentLab.Readiness.Core.dll') { $entry.Sha256 = $coreHash } }
            $entries | ConvertTo-Json | Set-Content -LiteralPath $fixtureInventory -Encoding UTF8
        }
        Write-Host 'Updated the shared core in the existing teaching-sample tool.'
    }
}
if ($IncludeFixtureTools) {
    $sourceCopy = (Resolve-Path -LiteralPath $FixtureSourceCopy).Path
    Install-LabCommand 'BimAgentLab.FixtureTools' 'BimAgentLab.FixtureTools.dll' (Join-Path $projectRoot 'tests/fixture-tools/bin/Release/net48') 'BA818F63-D7CE-43CC-90B9-CE39E2DF6C49' 'BimAgentLab.FixtureTools.PrepareInformationFixture' 'BIM Agent Lab - Prepare teaching sample copies' ''
    @{SourceCopy=$sourceCopy} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $addins 'BimAgentLab.FixtureTools/fixture-settings.json') -Encoding UTF8
}
