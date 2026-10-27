#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RevitTarget.ps1')
$script:Utf8=[Text.UTF8Encoding]::new($false)
$script:TestBoundary=$null
class McpDeploymentLease : System.IDisposable {
    [Threading.Mutex]$Mutex
    [bool]$Active
    [int]$Thread
    McpDeploymentLease() {
        $sid=[Security.Principal.WindowsIdentity]::GetCurrent().User.Value
        $this.Mutex=[Threading.Mutex]::new($false,"Global\BimAgentLab.Install.$sid")
        try {$this.Active=$this.Mutex.WaitOne(0)} catch [Threading.AbandonedMutexException] {$this.Active=$true}
        if(-not $this.Active) {$this.Mutex.Dispose();throw 'Another BIM Agent Lab installer/recovery owns the lease.'}
        $this.Thread=[Threading.Thread]::CurrentThread.ManagedThreadId
    }
    [void] Dispose() { if($this.Active) { $this.Mutex.ReleaseMutex();$this.Active=$false;$this.Mutex.Dispose() } }
}
function Enter-McpDeploymentLock { [McpDeploymentLease]::new() }
function Assert-Lease($Lease) {
    if($Lease -isnot [McpDeploymentLease] -or -not $Lease.Active -or $Lease.Thread -ne [Threading.Thread]::CurrentThread.ManagedThreadId) {throw 'A live deployment lease on this thread is required.'}
}
function Assert-RevitClosed { if(Get-Process Revit -ErrorAction SilentlyContinue) {throw 'Close Revit before installation or recovery.'} }
function Invoke-Boundary($Name) {if($script:TestBoundary) {& $script:TestBoundary $Name}}
function Get-Canonical($Path) {
    if(-not [IO.Path]::IsPathFullyQualified($Path)) {throw "Absolute path required: $Path"}
    $result=[IO.Path]::GetFullPath($Path).TrimEnd('\','/')
    if($result -eq [IO.Path]::GetPathRoot($result).TrimEnd('\','/')) {throw 'A drive root cannot be an installation root.'}
    $result
}
function Assert-PlainPath($Path) {
    $p=$Path
    while($p) {
        if(Test-Path -LiteralPath $p) {
            if(([IO.File]::GetAttributes($p) -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw "Reparse path is not allowed: $p"}
        }
        $p=Split-Path $p -Parent
    }
}
function Test-InRoot($Path,$Root) { $Path.StartsWith($Root+'\',[StringComparison]::OrdinalIgnoreCase) }
function Join-Safe($Root,$Relative) {
    if([IO.Path]::IsPathRooted($Relative) -or $Relative -match '(^|[\\/])\.\.?([\\/]|$)|:') {throw "Invalid relative path: $Relative"}
    $p=[IO.Path]::GetFullPath((Join-Path $Root $Relative))
    if(-not (Test-InRoot $p $Root)) {throw 'Path escaped its owner root.'}
    Assert-PlainPath $p
    $p
}
function Get-Hash($Path) {
    Assert-PlainPath $Path
    if(Test-Path -LiteralPath $Path) {
        if(-not [IO.File]::Exists($Path)) {throw "Expected file: $Path"}
        (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
    } else {$null}
}
function Get-BytesHash([byte[]]$Bytes) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Get-JsonBytes($Value) { ,$script:Utf8.GetBytes((ConvertTo-Json -InputObject $Value -Depth 25 -Compress)) }
function Write-Durable($Path,[byte[]]$Bytes) {
    $s=[IO.FileStream]::new($Path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None,4096,[IO.FileOptions]::WriteThrough)
    try {$s.Write($Bytes,0,$Bytes.Length);$s.Flush($true)} finally {$s.Dispose()}
}
function Write-Atomic($Path,[byte[]]$Bytes,$Temporary) {
    Assert-PlainPath $Path;Assert-PlainPath $Temporary
    Write-Durable $Temporary $Bytes
    if((Get-Hash $Temporary) -ne (Get-BytesHash $Bytes)) {throw 'Temporary file verification failed.'}
    if([IO.File]::Exists($Path)) {[IO.File]::Replace($Temporary,$Path,[NullString]::Value)} else {[IO.File]::Move($Temporary,$Path)}
}
function Write-State($Folder,$State) {
    Write-Atomic (Join-Path $Folder 'state.json') (Get-JsonBytes @{State=$State;Utc=[DateTime]::UtcNow.ToString('O')}) (Join-Path $Folder ('.state-'+[guid]::NewGuid().ToString('N')+'.tmp'))
}
function Write-Journal($Folder,$Event,$Path) {
    $bytes=$script:Utf8.GetBytes((ConvertTo-Json -Compress @{Event=$Event;Path=$Path;Utc=[DateTime]::UtcNow.ToString('O')})+"`n")
    $journal=Join-Path $Folder 'journal.jsonl'
    $s=[IO.FileStream]::new($journal,[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
    try {
        # An interrupted final record was never committed. Remove only that suffix
        # before appending, or the next valid record would make it corrupt history.
        $end=$s.Length
        while($end -gt 0){$s.Position=$end-1;if($s.ReadByte() -eq 10){break};$end--}
        if($end -ne $s.Length){$s.SetLength($end);$s.Flush($true)}
        $s.Position=$s.Length;$s.Write($bytes,0,$bytes.Length);$s.Flush($true)
    }finally{$s.Dispose()}
}
function Assert-NoPending($Root) {
    if(-not [IO.Directory]::Exists($Root)) {return}
    Assert-PlainPath $Root
    foreach($directory in Get-ChildItem -LiteralPath $Root -Directory -Force) {
        Assert-PlainPath $directory.FullName
        $receipt=Join-Path $directory.FullName 'receipt.json'
        if([IO.File]::Exists($receipt)) {
            $state=Get-Content -LiteralPath (Join-Path $directory.FullName 'state.json') -Raw | ConvertFrom-Json
            if($state.State -notin @('Committed','Restored')) {throw "RecoveryRequired: unfinished transaction $receipt"}
        }
    }
}
function Read-Inventory($Folder) {
    $map=@{};$p=Join-Path $Folder 'installed-files.json'
    if([IO.File]::Exists($p)) {
        $values=@(Get-Content -LiteralPath $p -Raw | ConvertFrom-Json)
        foreach($entry in $values) {
            if($entry.File -isnot [string] -or $entry.Sha256 -notmatch '^[A-Fa-f0-9]{64}$' -or $map.ContainsKey($entry.File)) {throw 'Invalid prior inventory.'}
            $target=Join-Safe $Folder $entry.File
            if((Get-Hash $target) -ne $entry.Sha256) {throw "Owned file changed: $target"}
            $map[$entry.File]=$entry.Sha256
        }
    }
    $map
}
function Assert-Manifest($Path,$Ids,$Classes,$Assembly) {
    if(-not [IO.File]::Exists($Path)) {return $false}
    [xml]$xml=Get-Content -LiteralPath $Path -Raw
    $entries=@($xml.RevitAddIns.AddIn)
    if($entries.Count -ne $Ids.Count) {throw "Unexpected manifest entries: $Path"}
    for($i=0;$i -lt $Ids.Count;$i++) {
        $e=$entries[$i]
        if($e.AddInId -ne $Ids[$i] -or $e.FullClassName -ne $Classes[$i] -or $e.Assembly -ne $Assembly -or $e.VendorId -ne 'BIAL') {throw "Unexpected manifest identity/path: $Path"}
    }
    $true
}
function Add-Operation($Plan,$Target,[byte[]]$Bytes,$Tag,[bool]$Owned) {
    Assert-PlainPath $Target
    if(@($Plan.Operations | Where-Object Target -EQ $Target).Count) {throw 'Duplicate deployment target.'}
    $before=Get-Hash $Target
    if($before -and -not $Owned) {throw "Unowned target exists: $Target"}
    $after=if($null -ne $Bytes) {Get-BytesHash $Bytes} else {$null}
    $Plan.Operations.Add([ordered]@{Target=$Target;Before=$before;After=$after;Tag=$Tag;Data=$(if($null -ne $Bytes){[Convert]::ToBase64String($Bytes)}else{$null});Temporary=$Target+'.bal-'+$Plan.Transaction+'.tmp'})
}
function Add-Payload($Map,$Relative,$Source) {
    if(-not [IO.File]::Exists($Source)) {throw "Build before installing: $Source"}
    Assert-PlainPath $Source
    if($Map.Contains($Relative)) {throw 'Duplicate package path.'}
    $Map[$Relative]=[IO.File]::ReadAllBytes($Source)
}
function Add-FolderPlan($Plan,$Folder,$Payload,$Tag,[hashtable]$ExtraOwned=@{}) {
    $previous=Read-Inventory $Folder
    foreach($relative in $ExtraOwned.Keys) {
        if((Get-Hash (Join-Safe $Folder $relative)) -ne $ExtraOwned[$relative]){throw 'Verified legacy bytes changed.'}
        if($previous.ContainsKey($relative) -and $previous[$relative] -ne $ExtraOwned[$relative]){throw 'Legacy inventory conflicts.'}
        $previous[$relative]=$ExtraOwned[$relative]
    }
    $inventory=[Collections.Generic.List[object]]::new()
    foreach($relative in $Payload.Keys) {
        $kind=$Tag
        if($Tag -eq 'Fixture' -and $relative -eq 'BimAgentLab.Readiness.Core.dll') {$kind='FixtureCore'}
        Add-Operation $Plan (Join-Safe $Folder $relative) $Payload[$relative] $kind $previous.ContainsKey($relative)
        $inventory.Add(@{File=$relative;Sha256=(Get-BytesHash $Payload[$relative])})
    }
    foreach($relative in $previous.Keys) {
        if(-not $Payload.Contains($relative)) {Add-Operation $Plan (Join-Safe $Folder $relative) $null 'ObsoleteDelete' $true}
    }
    Add-Operation $Plan (Join-Path $Folder 'installed-files.json') (Get-JsonBytes @($inventory.ToArray())) $(if($Tag -eq 'Fixture'){'FixtureInventory'}elseif($Tag -eq 'Main'){'MainInventory'}else{'ServerInventory'}) $true
}
function New-CommandXml($Assembly,$Id,$Class,$Text,$Availability='') {
    $a=[Security.SecurityElement]::Escape($Assembly)
    $script:Utf8.GetBytes("<?xml version=`"1.0`" encoding=`"utf-8`"?><RevitAddIns><AddIn Type=`"Command`"><Assembly>$a</Assembly><AddInId>$Id</AddInId><FullClassName>$Class</FullClassName><Text>$Text</Text><VendorId>BIAL</VendorId><VendorDescription>BIM Agent Lab</VendorDescription>$Availability<AllowLoadIntoExistingSession>false</AllowLoadIntoExistingSession></AddIn></RevitAddIns>")
}
function Assert-DestinationYear($Root,$Year) {
    $leaf=Split-Path $Root -Leaf
    if($leaf -match '^\d{4}$' -and $leaf -ne $Year){throw 'Revit destination year differs from the selected payload year.'}
}
function New-McpDeploymentPlan {
    param($Lease,[string]$SourceRoot,[string]$RevitAddinsRoot,[string]$ServerRoot,[switch]$IncludeMcpBridge,[switch]$IncludeFixtureTools,[string]$FixtureSourceCopy,[ValidateSet('2024','2026')][string]$RevitYear='2024')
    Assert-Lease $Lease;Assert-RevitClosed
    $source=Get-Canonical $SourceRoot;$addins=Get-Canonical $RevitAddinsRoot;$server=Get-Canonical $ServerRoot
    Assert-DestinationYear $addins $RevitYear
    $target=Get-RevitTarget -RevitYear $RevitYear
    $recovery=Get-Canonical (Join-Path (Split-Path $server) 'InstallRecovery')
    $roots=@($source,$addins,$server,$recovery)
    foreach($r in $roots) {Assert-PlainPath $r;foreach($other in $roots){if($r -ne $other -and (Test-InRoot $r $other)){throw 'Source, installation and recovery roots must be disjoint.'}}}
    if(@($roots | Select-Object -Unique).Count -ne 4){throw 'Roots must be distinct.'}
    Assert-NoPending $recovery
    $plan=[ordered]@{Version=1;Transaction=[guid]::NewGuid().ToString('N');Roots=@($addins,$server);RecoveryRoot=$recovery;Operations=[Collections.Generic.List[object]]::new();CreatedDirectories=@()}
    $main=Join-Path $addins 'BimAgentLab.RequiredInformation'
    $mainDll=Join-Path $main 'BimAgentLab.RevitReadiness.dll'
    $mainManifest=Join-Path $addins 'BimAgentLab.RequiredInformation.addin'
    $mainId='63D5AF32-6B13-4E91-9C5D-9B76048B4231';$mainClass='BimAgentLab.RevitReadiness.RequiredInformationCommand'
    $owned=Assert-Manifest $mainManifest @($mainId) @($mainClass) $mainDll
    if([IO.File]::Exists((Join-Path $main 'installed-files.json')) -and -not $owned) {throw 'Main inventory lacks recognized manifest.'}
    $payload=[ordered]@{}
    foreach($name in 'BimAgentLab.RevitReadiness.dll','BimAgentLab.Readiness.Core.dll','BimAgentLab.RevitBridge.Runtime.dll','BimAgentLab.RevitBridge.Contract.dll') {
        $file=Join-Path $source "src/revit-addin/$($target.OutputRelative)/$name"
        if($RevitYear -eq '2026'){Assert-RevitAssembly -LiteralPath $file -RevitYear $RevitYear -RequireRevitApi:($name -eq 'BimAgentLab.RevitReadiness.dll')}
        Add-Payload $payload $name $file
    }
    if($RevitYear -eq '2026'){Add-Payload $payload 'BimAgentLab.RevitReadiness.deps.json' (Join-Path $source "src/revit-addin/$($target.OutputRelative)/BimAgentLab.RevitReadiness.deps.json")}
    $excel=Join-Path $source 'src/excel-report/bin/Release/net48'
    foreach($required in 'BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config','ClosedXML.dll') {if(-not [IO.File]::Exists((Join-Path $excel $required))) {throw "Missing Excel dependency: $required"}}
    foreach($file in Get-ChildItem -LiteralPath $excel -File) {
        if($file.Name -match '^(RevitAPI|ModelContextProtocol|Microsoft.Extensions\.)') {throw 'Forbidden Autodesk/modern SDK dependency in Revit package.'}
        if($file.Extension -eq '.dll' -or $file.Name -in @('BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config')) {Add-Payload $payload ('Excel/'+$file.Name) $file.FullName}
    }
    $notices=Join-Path $source 'third-party'
    if(-not [IO.File]::Exists((Join-Path $notices 'dependency-evidence.json'))) {throw 'Dependency notices are required.'}
    foreach($file in Get-ChildItem -LiteralPath $notices -File -Recurse) {Add-Payload $payload ('third-party/'+[IO.Path]::GetRelativePath($notices,$file.FullName)) $file.FullName}
    Add-FolderPlan $plan $main $payload 'Main'
    $availability='<AvailabilityClassName>BimAgentLab.RevitReadiness.ProjectAvailable</AvailabilityClassName><VisibilityMode>NotVisibleInFamily</VisibilityMode><VisibilityMode>NotVisibleWhenNoActiveDocument</VisibilityMode>'
    Add-Operation $plan $mainManifest (New-CommandXml $mainDll $mainId $mainClass 'BIM Agent Lab - Required type information' $availability) 'Manifest' $owned
    $fixture=Join-Path $addins 'BimAgentLab.FixtureTools';$fixtureManifest=Join-Path $addins 'BimAgentLab.FixtureTools.addin'
    $fixtureId='BA818F63-D7CE-43CC-90B9-CE39E2DF6C49';$fixtureClass='BimAgentLab.FixtureTools.PrepareInformationFixture'
    $fixtureOwned=Assert-Manifest $fixtureManifest @($fixtureId) @($fixtureClass) (Join-Path $fixture 'BimAgentLab.FixtureTools.dll')
    if($fixtureOwned -or $IncludeFixtureTools) {
        $prior=Read-Inventory $fixture;$legacy=@{};$f=[ordered]@{}
        if($fixtureOwned -and -not [IO.File]::Exists((Join-Path $fixture 'installed-files.json'))) {
            $baselinePath=Join-Path $source 'scripts/legacy-fixture-baseline.json'
            Assert-PlainPath $baselinePath
            $baseline=@(Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json)
            if($baseline.Count -ne 2){throw 'A recognized legacy fixture baseline is required.'}
            foreach($entry in $baseline) {
                if($entry.File -notin @('BimAgentLab.FixtureTools.dll','BimAgentLab.Readiness.Core.dll') -or $legacy.ContainsKey($entry.File) -or $entry.Sha256 -notmatch '^[A-F0-9]{64}$'){throw 'Invalid legacy fixture baseline.'}
                if((Get-Hash (Join-Path $fixture $entry.File)) -ne $entry.Sha256){throw 'Unrecognized legacy fixture bytes; preserve and investigate before installation.'}
                $legacy[$entry.File]=$entry.Sha256;$prior[$entry.File]=$entry.Sha256
            }
        }
        foreach($relative in $prior.Keys) {Add-Payload $f $relative (Join-Safe $fixture $relative)}
        if($IncludeFixtureTools) {
            $model=Get-Canonical $FixtureSourceCopy;Assert-PlainPath $model
            if(-not [IO.File]::Exists($model)){throw 'Fixture source copy is missing.'}
            $fixtureDll=Join-Path $source "tests/fixture-tools/$($target.OutputRelative)/BimAgentLab.FixtureTools.dll"
            if($RevitYear -eq '2026'){Assert-RevitAssembly -LiteralPath $fixtureDll -RevitYear $RevitYear -RequireRevitApi}
            $f['BimAgentLab.FixtureTools.dll']=[IO.File]::ReadAllBytes($fixtureDll)
            if($RevitYear -eq '2026'){Add-Payload $f 'BimAgentLab.FixtureTools.deps.json' (Join-Path $source "tests/fixture-tools/$($target.OutputRelative)/BimAgentLab.FixtureTools.deps.json")}
            $f['fixture-settings.json']=Get-JsonBytes @{SourceCopy=$model}
            # Existing legacy settings were not inventoried: their recognized manifest owns this one known path.
            if([IO.File]::Exists((Join-Path $fixture 'fixture-settings.json')) -and -not $prior.ContainsKey('fixture-settings.json')) {
                if(-not $fixtureOwned){throw 'Unowned fixture settings.'}
                $old=Get-Content -LiteralPath (Join-Path $fixture 'fixture-settings.json') -Raw | ConvertFrom-Json
                if($old.SourceCopy -isnot [string]){throw 'Invalid existing fixture settings.'}
            }
        }
        $f['BimAgentLab.Readiness.Core.dll']=$payload['BimAgentLab.Readiness.Core.dll']
        # A legacy settings path is handled separately so ownership does not require rewriting the old inventory.
        $legacySettings=$IncludeFixtureTools -and [IO.File]::Exists((Join-Path $fixture 'fixture-settings.json')) -and -not $prior.ContainsKey('fixture-settings.json')
        if($legacySettings) {
            $settings=$f['fixture-settings.json'];$f.Remove('fixture-settings.json')
            Add-Operation $plan (Join-Path $fixture 'fixture-settings.json') $settings 'FixtureSettings' $true
        }
        Add-FolderPlan $plan $fixture $f 'Fixture' $legacy
        if($IncludeFixtureTools) {Add-Operation $plan $fixtureManifest (New-CommandXml (Join-Path $fixture 'BimAgentLab.FixtureTools.dll') $fixtureId $fixtureClass 'BIM Agent Lab - Prepare teaching sample copies') 'Manifest' $fixtureOwned}
    }
    if($IncludeMcpBridge) {
        $runtime=& dotnet --list-runtimes
        if($LASTEXITCODE -ne 0 -or -not ($runtime -match '^Microsoft.NETCore.App 10\.')){throw '.NET 10 runtime is required.'}
        $publish=Join-Path $source 'samples/local/mcp-publish';$s=[ordered]@{}
        foreach($required in 'BimAgentLab.RevitReadiness.Mcp.exe','BimAgentLab.RevitReadiness.Mcp.dll','BimAgentLab.RevitReadiness.Mcp.runtimeconfig.json','BimAgentLab.RevitReadiness.Mcp.deps.json','ModelContextProtocol.dll'){if(-not [IO.File]::Exists((Join-Path $publish $required))){throw "Publish the MCP server first: $required"}}
        foreach($file in Get-ChildItem -LiteralPath $publish -File -Recurse) {
            if($file.Name -match '^(RevitAPI|BimAgentLab\.RevitReadiness\.dll$|BimAgentLab\.Readiness\.(Core|Excel))') {throw 'Revit/Excel payload cannot enter the server.'}
            Add-Payload $s ([IO.Path]::GetRelativePath($publish,$file.FullName)) $file.FullName
        }
        $mcpNotices=Join-Path $source 'third-party/mcp'
        foreach($required in 'README.md','csharp-sdk-LICENSE.txt','csharp-sdk-THIRD-PARTY-NOTICES.txt','dotnet-runtime-LICENSE.txt','dotnet-extensions-LICENSE.txt','dotnet-runtime-THIRD-PARTY-NOTICES.txt','package-notices.json','license-sources.json') {
            Add-Payload $s ('third-party/'+$required) (Join-Path $mcpNotices $required)
        }
        Add-FolderPlan $plan $server $s 'Server'
        $ids=@('1C77DD07-259D-44DF-86FF-F6D859E95569','1F280D99-D9EF-4DB0-94B3-CC3D13990E99','EFEE79D7-E46B-4AD9-ADA0-83D1B5DB8FB8')
        $classes=@('BimAgentLab.RevitReadiness.BridgeApplication','BimAgentLab.RevitReadiness.StartReadinessBridgeCommand','BimAgentLab.RevitReadiness.StopReadinessBridgeCommand')
        $manifest=Join-Path $addins 'BimAgentLab.ReadinessBridge.addin'
        $bridgeOwned=Assert-Manifest $manifest $ids $classes $mainDll
        $template=Join-Path $source 'src/revit-addin/BimAgentLab.ReadinessBridge.addin.template'
        Assert-PlainPath $template
        $xml=[IO.File]::ReadAllText($template).Replace('{{ASSEMBLY}}',[Security.SecurityElement]::Escape($mainDll))
        [xml]$check=$xml
        $entries=@($check.RevitAddIns.AddIn)
        if($entries.Count -ne 3){throw 'Invalid bridge manifest template.'}
        for($i=0;$i -lt 3;$i++) {
            if($entries[$i].AddInId -ne $ids[$i] -or $entries[$i].FullClassName -ne $classes[$i] -or $entries[$i].Assembly -ne $mainDll -or $entries[$i].VendorId -ne 'BIAL' -or $entries[$i].Type -ne $(if($i -eq 0){'Application'}else{'Command'})){throw 'Bridge template identity mismatch.'}
        }
        Add-Operation $plan $manifest $script:Utf8.GetBytes($xml) 'Manifest' $bridgeOwned
    }
    $dirs=@{}
    foreach($op in $plan.Operations) {
        if(Test-Path -LiteralPath $op.Temporary){throw 'Planned temporary file already exists.'}
        $p=Split-Path $op.Target
        while($p -and -not [IO.Directory]::Exists($p)) {Assert-PlainPath $p;if(-not (Test-InRoot $recovery $p)){$dirs[$p]=$true};$p=Split-Path $p}
    }
    $plan.CreatedDirectories=@($dirs.Keys | Sort-Object Length)
    $plan
}
function New-RevitProbeDeploymentPlan {
    param($Lease,[string]$SourceRoot,[string]$RevitAddinsRoot,[string]$RecoveryRoot,[ValidateSet('2024','2026')][string]$RevitYear='2024')
    Assert-Lease $Lease;Assert-RevitClosed
    $source=Get-Canonical $SourceRoot;$addins=Get-Canonical $RevitAddinsRoot;$recovery=Get-Canonical $RecoveryRoot
    Assert-DestinationYear $addins $RevitYear
    $roots=@($source,$addins,$recovery)
    foreach($r in $roots){Assert-PlainPath $r;foreach($other in $roots){if($r -ne $other -and (Test-InRoot $r $other)){throw 'Source, probe and recovery roots must be disjoint.'}}}
    if(@($roots|Select-Object -Unique).Count -ne 3){throw 'Probe roots must be distinct.'}
    Assert-NoPending $recovery
    $target=Get-RevitTarget -RevitYear $RevitYear
    $folder=Join-Path $addins 'BimAgentLab.ReadOnlyProbe';$dll=Join-Path $folder 'BimAgentLab.RevitProbe.dll'
    $manifest=Join-Path $addins 'BimAgentLab.ReadOnlyProbe.addin'
    $owned=Assert-Manifest $manifest @('8B3FA065-26DF-4076-A194-43C1457C81AF') @('BimAgentLab.RevitProbe.ReadOnlyProbeCommand') $dll
    if([IO.File]::Exists((Join-Path $folder 'installed-files.json')) -and -not $owned){throw 'Probe inventory lacks recognized manifest.'}
    $built=Join-Path $source "src/revit-addin/$($target.OutputRelative)/BimAgentLab.RevitProbe.dll"
    Assert-RevitAssembly -LiteralPath $built -RevitYear $RevitYear -RequireRevitApi
    $plan=[ordered]@{Version=1;Transaction=[guid]::NewGuid().ToString('N');Roots=@($addins);RecoveryRoot=$recovery;Operations=[Collections.Generic.List[object]]::new();CreatedDirectories=@()}
    $payload=[ordered]@{};Add-Payload $payload 'BimAgentLab.RevitProbe.dll' $built
    if($RevitYear -eq '2026'){Add-Payload $payload 'BimAgentLab.RevitProbe.deps.json' (Join-Path $source "src/revit-addin/$($target.OutputRelative)/BimAgentLab.RevitProbe.deps.json")}
    Add-FolderPlan $plan $folder $payload 'Main'
    $xml=[IO.File]::ReadAllText((Join-Path $source 'src/revit-addin/BimAgentLab.ReadOnlyProbe.addin.template')).Replace('__ASSEMBLY_PATH__',[Security.SecurityElement]::Escape($dll))
    try {
        [xml]$check=$xml;$entries=@($check.RevitAddIns.AddIn)
        if($entries.Count -ne 1 -or $entries[0].Type -ne 'Command' -or $entries[0].AddInId -ne '8B3FA065-26DF-4076-A194-43C1457C81AF' -or $entries[0].FullClassName -ne 'BimAgentLab.RevitProbe.ReadOnlyProbeCommand' -or $entries[0].Assembly -ne $dll -or $entries[0].VendorId -ne 'BIAL'){throw 'Unexpected probe template.'}
    } catch {throw 'Probe template identity mismatch.'}
    Add-Operation $plan $manifest $script:Utf8.GetBytes($xml) 'Manifest' $owned
    $dirs=@{}
    foreach($op in $plan.Operations){
        if(Test-Path -LiteralPath $op.Temporary){throw 'Planned temporary file already exists.'}
        $p=Split-Path $op.Target
        while($p -and -not [IO.Directory]::Exists($p)){Assert-PlainPath $p;if(-not (Test-InRoot $recovery $p)){$dirs[$p]=$true};$p=Split-Path $p}
    }
    $plan.CreatedDirectories=@($dirs.Keys|Sort-Object Length)
    $plan
}
function Save-McpDeploymentRecovery {
    param($Lease,$Plan,[string]$RecoveryRoot)
    Assert-Lease $Lease;Assert-RevitClosed
    $root=Get-Canonical $RecoveryRoot
    if($root -ne $Plan.RecoveryRoot){throw 'Recovery root differs from preflight.'}
    Assert-PlainPath $root;Assert-NoPending $root
    $folder=Join-Path $root $Plan.Transaction
    if(Test-Path -LiteralPath $folder){throw 'Transaction already exists.'}
    [IO.Directory]::CreateDirectory((Join-Path $folder 'backup'))|Out-Null
    [IO.Directory]::CreateDirectory((Join-Path $folder 'staged'))|Out-Null
    $ops=[Collections.Generic.List[object]]::new();$n=0
    foreach($op in $Plan.Operations) {
        if((Get-Hash $op.Target) -ne $op.Before){throw "Target changed during preparation: $($op.Target)"}
        $copy=[ordered]@{};foreach($key in $op.Keys){if($key -ne 'Data'){$copy[$key]=$op[$key]}}
        $copy['Backup']=$null;$copy['Staged']=$null
        if($op.Before){$copy.Backup="backup/$n.bin";Write-Durable (Join-Path $folder $copy.Backup) ([IO.File]::ReadAllBytes($op.Target));if((Get-Hash (Join-Path $folder $copy.Backup)) -ne $op.Before){throw 'Backup changed during capture.'};Invoke-Boundary 'Backup'}
        if($op.After){$copy.Staged="staged/$n.bin";Write-Durable (Join-Path $folder $copy.Staged) ([Convert]::FromBase64String($op.Data));if((Get-Hash (Join-Path $folder $copy.Staged)) -ne $op.After){throw 'Staged bytes mismatch.'}}
        $ops.Add($copy);$n++
    }
    $receipt=[ordered]@{Version=1;Transaction=$Plan.Transaction;Roots=$Plan.Roots;RecoveryRoot=$root;Operations=$ops.ToArray();CreatedDirectories=$Plan.CreatedDirectories}
    $bytes=Get-JsonBytes $receipt
    Write-Durable (Join-Path $folder 'receipt.sha256') $script:Utf8.GetBytes((Get-BytesHash $bytes))
    Write-State $folder 'Prepared'
    Write-Durable (Join-Path $folder 'receipt.preparing') $bytes
    [IO.File]::Move((Join-Path $folder 'receipt.preparing'),(Join-Path $folder 'receipt.json'))
    $path=Join-Path $folder 'receipt.json'
    $null=Read-Receipt $path
    $path
}
function Read-Receipt($Path) {
    $path=Get-Canonical $Path;Assert-PlainPath $path;$folder=Split-Path $path
    $bytes=[IO.File]::ReadAllBytes($path)
    if((Get-BytesHash $bytes) -ne [IO.File]::ReadAllText((Join-Path $folder 'receipt.sha256'))){throw 'RecoveryRequired: receipt checksum mismatch.'}
    $r=$script:Utf8.GetString($bytes)|ConvertFrom-Json -AsHashtable
    if($r.Version -ne 1 -or $r.Transaction -notmatch '^[a-f0-9]{32}$' -or $folder -ne (Join-Path (Get-Canonical $r.RecoveryRoot) $r.Transaction) -or (Split-Path $path -Leaf) -ne 'receipt.json'){throw 'Invalid prepared receipt identity.'}
    if($r.Roots.Count -notin @(1,2)){throw 'Invalid roots.'}
    foreach($root in $r.Roots){if((Get-Canonical $root) -ne $root){throw 'Noncanonical root.'};Assert-PlainPath $root;if($root -eq $r.RecoveryRoot -or (Test-InRoot $root $r.RecoveryRoot) -or (Test-InRoot $r.RecoveryRoot $root)){throw 'Recovery overlaps installed files.'}}
    if($r.Roots.Count -eq 2 -and ($r.Roots[0] -eq $r.Roots[1] -or (Test-InRoot $r.Roots[0] $r.Roots[1]) -or (Test-InRoot $r.Roots[1] $r.Roots[0]))){throw 'Installation roots overlap.'}
    $seen=@{}
    foreach($op in $r.Operations) {
        $target=Get-Canonical $op.Target
        if($target -ne $op.Target -or -not (@($r.Roots|Where-Object {Test-InRoot $target $_}).Count) -or $seen.ContainsKey($target)){throw 'Invalid/duplicate recovery target.'}
        $seen[$target]=$true;Assert-PlainPath $target
        if($op.Temporary -ne $target+'.bal-'+$r.Transaction+'.tmp'){throw 'Invalid temporary path.'}
        foreach($pair in @(@('Before','Backup'),@('After','Staged'))) {
            $hash=$op[$pair[0]];$relative=$op[$pair[1]]
            if($hash) {if($hash -notmatch '^[A-F0-9]{64}$' -or -not $relative){throw 'Invalid recovery hash.'};$file=Join-Safe $folder $relative;if((Get-Hash $file) -ne $hash){throw "RecoveryRequired: missing or changed recovery bytes: $relative"}}
            elseif($relative){throw 'Unexpected recovery bytes.'}
        }
    }
    foreach($dir in $r.CreatedDirectories) {
        if((Get-Canonical $dir) -ne $dir -or -not (@($r.Roots|Where-Object {$dir -eq $_ -or (Test-InRoot $dir $_) -or (Test-InRoot $_ $dir)}).Count)){throw 'Invalid created directory.'}
        # Ancestors can be newly created only up to the nearest existing parent recorded at preflight.
        Assert-PlainPath $dir
    }
    $r
}
function Read-DirectoryClaims($Folder,$Receipt) {
    $claims=@{};$journal=Join-Path $Folder 'journal.jsonl'
    if(-not [IO.File]::Exists($journal)){return $claims}
    $text=[IO.File]::ReadAllText($journal);$lines=$text.Split("`n")
    for($i=0;$i -lt $lines.Length;$i++) {
        if([string]::IsNullOrWhiteSpace($lines[$i])){continue}
        try {$entry=$lines[$i]|ConvertFrom-Json -AsHashtable} catch {
            if($i -eq $lines.Length-1 -and -not $text.EndsWith("`n")){break}
            throw 'RecoveryRequired: damaged complete journal record.'
        }
        if($entry.Event -eq 'CreateDirectoryIntent') {
            if($entry.Path -notin $Receipt.CreatedDirectories){throw 'RecoveryRequired: journal directory is outside the prepared plan.'}
            $claims[$entry.Path]=$true
        }
    }
    $claims
}
function Restore-McpDeployment {
    param($Lease,[string]$ReceiptPath)
    Assert-Lease $Lease;Assert-RevitClosed
    $r=Read-Receipt $ReceiptPath;$folder=Split-Path $ReceiptPath
    $claims=Read-DirectoryClaims $folder $r
    $ownedPaths=@{};foreach($op in $r.Operations){$ownedPaths[$op.Target]=$true;$ownedPaths[$op.Temporary]=$true}
    foreach($dir in $r.CreatedDirectories) {
        if([IO.Directory]::Exists($dir)) {
            if(-not $claims.ContainsKey($dir)){throw "RecoveryRequired: directory was not created by this apply: $dir"}
            foreach($entry in Get-ChildItem -LiteralPath $dir -Recurse -Force) {
                Assert-PlainPath $entry.FullName
                if($entry.PSIsContainer) {if($entry.FullName -notin $r.CreatedDirectories){throw 'RecoveryRequired: foreign directory.'}}
                elseif(-not $ownedPaths.ContainsKey($entry.FullName)){throw 'RecoveryRequired: foreign file in a created directory.'}
            }
        }
    }
    # Whole-plan classification precedes any installed-file recovery write.
    foreach($op in $r.Operations) {
        $actual=Get-Hash $op.Target
        if($actual -ne $op.Before -and $actual -ne $op.After){Write-State $folder 'RecoveryRequired';throw "RecoveryRequired: foreign modification $($op.Target)"}
        $temp=Get-Hash $op.Temporary
        if($temp -and $temp -ne $op.Before -and $temp -ne $op.After){Write-State $folder 'RecoveryRequired';throw 'RecoveryRequired: unexpected temporary bytes.'}
    }
    $state=(Get-Content -LiteralPath (Join-Path $folder 'state.json') -Raw|ConvertFrom-Json).State
    if($state -eq 'Restored'){foreach($op in $r.Operations){if((Get-Hash $op.Target) -ne $op.Before -or [IO.File]::Exists($op.Temporary)){throw 'RecoveryRequired: restored state changed.'}};foreach($dir in $r.CreatedDirectories){if(Test-Path -LiteralPath $dir){throw 'RecoveryRequired: restored directory state changed.'}};return}
    Write-State $folder 'Restoring'
    $reverse=@($r.Operations);[array]::Reverse($reverse)
    foreach($op in $reverse) {
        Assert-RevitClosed
        $actual=Get-Hash $op.Target
        if($actual -ne $op.Before -and $actual -ne $op.After){throw 'RecoveryRequired: target changed during restoration.'}
        Write-Journal $folder 'RestoreIntent' $op.Target
        $temp=Get-Hash $op.Temporary
        if($temp){if($temp -ne $op.Before -and $temp -ne $op.After){throw 'RecoveryRequired: temporary changed during restoration.'};[IO.File]::Delete($op.Temporary)}
        if($actual -ne $op.Before) {
            if($op.Before){$bytes=[IO.File]::ReadAllBytes((Join-Path $folder $op.Backup));if((Get-BytesHash $bytes) -ne $op.Before){throw 'RecoveryRequired: backup changed during restoration.'};Write-Atomic $op.Target $bytes $op.Temporary}
            else {[IO.File]::Delete($op.Target)}
            Invoke-Boundary 'RestoreMutation'
        }
        Write-Journal $folder 'RestoredFile' $op.Target
    }
    foreach($dir in @($r.CreatedDirectories|Sort-Object Length -Descending)) {
        if([IO.Directory]::Exists($dir)) {
            if(@(Get-ChildItem -LiteralPath $dir -Force).Count){throw "RecoveryRequired: created directory contains unexpected files: $dir"}
            Write-Journal $folder 'RemoveDirectoryIntent' $dir
            [IO.Directory]::Delete($dir,$false)
        }
    }
    foreach($op in $r.Operations){if((Get-Hash $op.Target) -ne $op.Before){throw 'Recovery verification failed.'}}
    Write-State $folder 'Restored'
}
function Invoke-McpDeploymentPlan {
    param($Lease,[string]$ReceiptPath)
    Assert-Lease $Lease;Assert-RevitClosed
    $r=Read-Receipt $ReceiptPath;$folder=Split-Path $ReceiptPath
    if((Get-Content -LiteralPath (Join-Path $folder 'state.json') -Raw|ConvertFrom-Json).State -ne 'Prepared'){throw 'Apply requires Prepared; recover interrupted work first.'}
    foreach($op in $r.Operations){if((Get-Hash $op.Target) -ne $op.Before -or (Test-Path -LiteralPath $op.Temporary)){throw 'Pre-apply state changed.'}}
    foreach($dir in $r.CreatedDirectories){if(Test-Path -LiteralPath $dir){throw 'Pre-apply directory state changed.'}}
    try {
        Write-State $folder 'Applying'
        foreach($dir in $r.CreatedDirectories) {Write-Journal $folder 'CreateDirectoryIntent' $dir;Assert-PlainPath $dir;[IO.Directory]::CreateDirectory($dir)|Out-Null;Write-Journal $folder 'CreatedDirectory' $dir}
        $first=$true
        foreach($op in $r.Operations) {
            Assert-RevitClosed
            if((Get-Hash $op.Target) -ne $op.Before){throw 'Target changed while applying.'}
            Write-Journal $folder 'ApplyIntent' $op.Target
            if($op.After){$bytes=[IO.File]::ReadAllBytes((Join-Path $folder $op.Staged));if((Get-BytesHash $bytes) -ne $op.After){throw 'Staged bytes changed during apply.'};Write-Atomic $op.Target $bytes $op.Temporary}
            elseif($op.Before){[IO.File]::Delete($op.Target)}
            Invoke-Boundary 'AfterMutation'
            Write-Journal $folder 'AppliedFile' $op.Target
            if($first -and $op.After){$first=$false;Invoke-Boundary 'FirstCopy'}
            Invoke-Boundary $op.Tag
        }
        foreach($op in $r.Operations){if((Get-Hash $op.Target) -ne $op.After){throw 'Final installation hash mismatch.'}}
        Invoke-Boundary 'BeforeCommit'
        Write-State $folder 'Committed'
        Write-Output "Installed; recovery receipt: $ReceiptPath"
    } catch {
        $failure=$_
        try {Restore-McpDeployment -Lease $Lease -ReceiptPath $ReceiptPath} catch {throw "Installation failed: $failure; recovery incomplete: $_. Receipt: $ReceiptPath"}
        throw $failure
    }
}
Export-ModuleMember -Function Enter-McpDeploymentLock,New-McpDeploymentPlan,New-RevitProbeDeploymentPlan,Save-McpDeploymentRecovery,Invoke-McpDeploymentPlan,Restore-McpDeployment
