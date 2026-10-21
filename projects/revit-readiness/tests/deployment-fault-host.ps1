param([string]$Root,[string]$Mode,[string]$Scenario='existing',[string]$Boundary='', [string]$Fault='throw')
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$source=Join-Path $Root 'source'
$addins=Join-Path $Root 'addins'
$server=Join-Path $Root 'server'
$recovery=Join-Path $Root 'InstallRecovery'
if($Scenario -eq 'fresh-parent') { $server=Join-Path $Root 'runtime/McpServer'; $recovery=Join-Path $Root 'runtime/InstallRecovery' }
function Write-TestFile($path,$value) {
    [IO.Directory]::CreateDirectory((Split-Path $path)) | Out-Null
    [IO.File]::WriteAllText($path,$value,[Text.UTF8Encoding]::new($false))
}
function Seed-Command($name,$assembly,$id,$class) {
    $folder=Join-Path $addins $name
    $inventory=@()
    foreach($file in $assembly,'BimAgentLab.Readiness.Core.dll','obsolete.dll') {
        Write-TestFile (Join-Path $folder $file) "old bytes: $name/$file"
        $inventory+=@{File=$file;Sha256=(Get-FileHash -LiteralPath (Join-Path $folder $file)).Hash}
    }
    Write-TestFile (Join-Path $folder 'installed-files.json') (ConvertTo-Json -InputObject $inventory)
    Write-TestFile (Join-Path $addins "$name.addin") "<RevitAddIns><AddIn Type=`"Command`"><Assembly>$(Join-Path $folder $assembly)</Assembly><AddInId>$id</AddInId><FullClassName>$class</FullClassName><VendorId>BIAL</VendorId></AddIn></RevitAddIns>"
    Write-TestFile (Join-Path $folder 'unrelated.txt') 'preserve unrelated bytes'
}
if($Mode -eq 'Seed') {
    foreach($name in 'BimAgentLab.RevitReadiness.dll','BimAgentLab.Readiness.Core.dll','BimAgentLab.RevitBridge.Runtime.dll','BimAgentLab.RevitBridge.Contract.dll') {
        Write-TestFile (Join-Path $source "src/revit-addin/bin/Release/net48/$name") "new package: $name"
    }
    foreach($name in 'BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config','ClosedXML.dll') {
        Write-TestFile (Join-Path $source "src/excel-report/bin/Release/net48/$name") "excel: $name"
    }
    foreach($name in 'BimAgentLab.FixtureTools.dll','BimAgentLab.Readiness.Core.dll') {
        Write-TestFile (Join-Path $source "tests/fixture-tools/bin/Release/net48/$name") $(if($name -like '*Core*'){"new package: $name"}else{"fixture: $name"})
    }
    Write-TestFile (Join-Path $source 'src/revit-addin/BimAgentLab.ReadinessBridge.addin.template') ([IO.File]::ReadAllText((Join-Path $project 'src/revit-addin/BimAgentLab.ReadinessBridge.addin.template')))
    Write-TestFile (Join-Path $source 'third-party/dependency-evidence.json') '{}'
    Write-TestFile (Join-Path $source 'third-party/licenses/test.txt') 'test license'
    foreach($notice in 'README.md','csharp-sdk-LICENSE.txt','csharp-sdk-THIRD-PARTY-NOTICES.txt','dotnet-runtime-LICENSE.txt','dotnet-extensions-LICENSE.txt','dotnet-runtime-THIRD-PARTY-NOTICES.txt','package-notices.json','license-sources.json') { Write-TestFile (Join-Path $source ('third-party/mcp/'+$notice)) 'synthetic notice bytes' }
    if($Scenario -eq 'missing-notice') { Remove-Item -LiteralPath (Join-Path $source 'third-party/mcp/csharp-sdk-LICENSE.txt') }
    foreach($name in 'BimAgentLab.RevitReadiness.Mcp.exe','BimAgentLab.RevitReadiness.Mcp.dll','ModelContextProtocol.dll') {
        Write-TestFile (Join-Path $source "samples/local/mcp-publish/$name") "server: $name"
    }
    Write-TestFile (Join-Path $source 'samples/local/mcp-publish/BimAgentLab.RevitReadiness.Mcp.runtimeconfig.json') '{"runtimeOptions":{"tfm":"net10.0","framework":{"name":"Microsoft.NETCore.App","version":"10.0.0"}}}'
    Write-TestFile (Join-Path $source 'samples/local/mcp-publish/BimAgentLab.RevitReadiness.Mcp.deps.json') '{}'
    Write-TestFile (Join-Path $Root 'source-copy.rvt') 'synthetic model, never opened'
    Write-TestFile (Join-Path $addins 'unrelated.addin') 'unrelated registration'
    Write-TestFile (Join-Path $Root 'server-root.txt') $server
    if($Scenario -ne 'fresh-parent') { Write-TestFile (Join-Path $server 'unrelated.txt') 'unrelated server file' }
    Write-TestFile (Join-Path $Root 'client-settings.json') '{"other-server":true}'
    Seed-Command 'BimAgentLab.RequiredInformation' 'BimAgentLab.RevitReadiness.dll' '63D5AF32-6B13-4E91-9C5D-9B76048B4231' 'BimAgentLab.RevitReadiness.RequiredInformationCommand'
    if($Scenario -notin @('none','explicit')) {
        Seed-Command 'BimAgentLab.FixtureTools' 'BimAgentLab.FixtureTools.dll' 'BA818F63-D7CE-43CC-90B9-CE39E2DF6C49' 'BimAgentLab.FixtureTools.PrepareInformationFixture'
    }
    if($Scenario -in @('legacy','invalid-legacy')) {
        Remove-Item -LiteralPath (Join-Path $addins 'BimAgentLab.FixtureTools/installed-files.json')
        Write-TestFile (Join-Path $addins 'BimAgentLab.FixtureTools/fixture-settings.json') (ConvertTo-Json @{SourceCopy=(Join-Path $Root 'source-copy.rvt')})
        $baseline=@()
        foreach($file in 'BimAgentLab.FixtureTools.dll','BimAgentLab.Readiness.Core.dll') {
            $hash=(Get-FileHash -LiteralPath (Join-Path $addins ('BimAgentLab.FixtureTools/'+$file))).Hash
            if($Scenario -eq 'invalid-legacy'){$hash='0'*64}
            $baseline+=@{File=$file;Sha256=$hash}
        }
        Write-TestFile (Join-Path $source 'scripts/legacy-fixture-baseline.json') (ConvertTo-Json -InputObject $baseline)
    }
    if($Scenario -eq 'invalid-fixture') { Write-TestFile (Join-Path $addins 'BimAgentLab.FixtureTools.addin') '<RevitAddIns><AddIn><AddInId>foreign</AddInId></AddIn></RevitAddIns>' }
    if($Scenario -eq 'forbidden') { Write-TestFile (Join-Path $source 'src/excel-report/bin/Release/net48/RevitAPI.dll') 'forbidden Autodesk DLL' }
    if($Scenario -eq 'modern-in-host') { Write-TestFile (Join-Path $source 'src/excel-report/bin/Release/net48/ModelContextProtocol.dll') 'forbidden modern SDK' }
    if($Scenario -eq 'unowned') { Write-TestFile (Join-Path $addins 'BimAgentLab.RequiredInformation/BimAgentLab.RevitBridge.Runtime.dll') 'unowned collision' }
    if($Scenario -eq 'bad-inventory') { Write-TestFile (Join-Path $addins 'BimAgentLab.RequiredInformation/installed-files.json') '[{"File":"../escape.dll","Sha256":"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"}]' }
    exit 0
}
Import-Module (Join-Path $project 'scripts/McpDeployment.psm1') -Force
$module=Get-Module McpDeployment
# These hooks are reachable only from this disposable-root harness, never CLI installation options.
& $module { param($boundary,$fault) $script:TestBoundary={param($name) if($name -eq $boundary) { if($fault -eq 'exit'){[Environment]::Exit(91)}; throw "INJECTED $name" }}.GetNewClosure() } $Boundary $Fault
$lease=$null
try {
    $lease=Enter-McpDeploymentLock
    $options=@{Lease=$lease;SourceRoot=$source;RevitAddinsRoot=$addins;ServerRoot=$server;IncludeMcpBridge=($Scenario -ne 'normal');IncludeFixtureTools=($Scenario -eq 'explicit');FixtureSourceCopy=(Join-Path $Root 'source-copy.rvt')}
    if($Mode -eq 'HoldLease') {
        Write-TestFile (Join-Path $Root 'lease-held.txt') 'held'
        $end=[DateTime]::UtcNow.AddSeconds(15)
        while(-not [IO.File]::Exists((Join-Path $Root 'release-lease.txt'))) {if([DateTime]::UtcNow -gt $end){throw 'Lease test timed out.'};Start-Sleep -Milliseconds 25}
    } elseif($Mode -eq 'DisposedLease') {
        $lease.Dispose()
        try {New-McpDeploymentPlan @options;throw 'Disposed lease accepted'} catch {if($_ -notlike '*live deployment lease*'){throw}}
    } elseif($Mode -eq 'Restore') {
        Restore-McpDeployment -Lease $lease -ReceiptPath ([IO.File]::ReadAllText((Join-Path $Root 'receipt-path.txt')))
    } else {
        $plan=New-McpDeploymentPlan @options
        if($Mode -ne 'Preflight') {
            $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot $recovery
            [IO.File]::WriteAllText((Join-Path $Root 'receipt-path.txt'),$receipt)
            if($Mode -eq 'Apply') { Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt }
        }
    }
} finally { if($lease) {$lease.Dispose()} }
