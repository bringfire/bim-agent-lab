param([Parameter(Mandatory)][string]$Root,[ValidateSet('roundtrip','wrong-year','metadata','probe','mixed','probe-interrupted','probe-template','reinstall')][string]$Scenario='roundtrip')
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
Import-Module (Join-Path $project 'scripts/McpDeployment.psm1') -Force
. (Join-Path $project 'scripts/RevitTarget.ps1')
$addins=Join-Path $Root 'Addins/2026'
$sentinel=Join-Path $Root 'Addins/2024/user.addin'
New-Item -ItemType Directory -Path (Split-Path $sentinel) -Force | Out-Null
[IO.File]::WriteAllText($sentinel,'preserve 2024 user addin')
$before=(Get-FileHash -LiteralPath $sentinel).Hash
$lease=Enter-McpDeploymentLock
try {
    if($Scenario -eq 'probe-template') {
        $fake=Join-Path $Root 'bad-probe-source'
        $badDll=Join-Path $fake 'src/revit-addin/bin/Release/net48/BimAgentLab.RevitProbe.dll'
        New-Item -ItemType Directory -Path (Split-Path $badDll) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $project 'src/revit-addin/bin/Release/net48/BimAgentLab.RevitProbe.dll') -Destination $badDll
        $template=[IO.File]::ReadAllText((Join-Path $project 'src/revit-addin/BimAgentLab.ReadOnlyProbe.addin.template'))
        [IO.File]::WriteAllText((Join-Path $fake 'src/revit-addin/BimAgentLab.ReadOnlyProbe.addin.template'),$template.Replace('BimAgentLab.RevitProbe.ReadOnlyProbeCommand','Wrong.Class'))
        try {$null=New-RevitProbeDeploymentPlan -Lease $lease -SourceRoot $fake -RevitAddinsRoot (Split-Path $sentinel) -RecoveryRoot (Join-Path $Root 'ProbeRecovery') -RevitYear 2024;throw 'Bad probe template accepted'}
        catch {if($_ -notlike '*Probe template identity mismatch*'){throw}}
        if((Get-FileHash -LiteralPath $sentinel).Hash -ne $before){throw 'Invalid template changed 2024.'}
        'PASS probe template mismatch is a preflight failure'; return
    }
    if($Scenario -eq 'metadata') {
        foreach($year in '2024','2026') {
            $target=Get-RevitTarget -RevitYear $year
            $path=Join-Path $project "src/revit-addin/$($target.OutputRelative)/BimAgentLab.RevitReadiness.dll"
            $info=Get-RevitAssemblyInfo -LiteralPath $path
            $expected=if($year -eq '2024'){'.NETFramework,Version=v4.8'}else{'.NETCoreApp,Version=v10.0'}
            if($info.TargetFramework -ne $expected -or $info.RevitApiMajor -ne ([int]$year-2000)){throw 'Wrong compiled host framework/API metadata.'}
        }
        'PASS PE metadata identifies both real host targets without loading Autodesk'; return
    }
    $options=@{Lease=$lease;SourceRoot=$project;RevitAddinsRoot=$addins;ServerRoot=(Join-Path $Root 'Runtime2026/McpServer');RevitYear='2026';IncludeMcpBridge=$true;IncludeFixtureTools=$true;FixtureSourceCopy=(Join-Path $project 'samples/local/revit2026-source/Snowdon-BAL-source-copy.rvt')}
    if($Scenario -eq 'mixed') {
        $options.SourceRoot=Join-Path $Root 'mixed-source'
        $wrongDll=Join-Path $options.SourceRoot 'src/revit-addin/bin/Revit2026/Release/net10.0-windows/BimAgentLab.RevitReadiness.dll'
        New-Item -ItemType Directory -Path (Split-Path $wrongDll) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $project 'src/revit-addin/bin/Release/net48/BimAgentLab.RevitReadiness.dll') -Destination $wrongDll
        try {$null=New-McpDeploymentPlan @options;throw 'Mixed-framework payload accepted'}
        catch {if($_ -notlike '*framework target does not match*'){throw}}
        if(Test-Path -LiteralPath $addins){throw 'Invalid payload mutated installation root.'}
        if((Get-FileHash -LiteralPath $sentinel).Hash -ne $before){throw 'Mixed payload changed 2024.'}
        'PASS wrong-framework binary rejected despite modern folder name'; return
    }
    if($Scenario -eq 'wrong-year') {
        $options.RevitAddinsRoot=Split-Path $sentinel
        try {$null=New-McpDeploymentPlan @options;throw 'Wrong-year destination accepted'}
        catch {if($_ -notlike '*destination year*'){throw}}
        if((Get-FileHash -LiteralPath $sentinel).Hash -ne $before){throw 'Wrong-year preflight changed 2024.'}
        'PASS wrong-year destination rejected before mutations'; return
    }
    if($Scenario -in @('probe','probe-interrupted')) {
        $recovery=Join-Path $Root 'ProbeRecovery'
        $plan=New-RevitProbeDeploymentPlan -Lease $lease -SourceRoot $project -RevitAddinsRoot $addins -RecoveryRoot $recovery -RevitYear 2026
        $dll=Join-Path $addins 'BimAgentLab.ReadOnlyProbe/BimAgentLab.RevitProbe.dll'
    } else {
        $recovery=Join-Path $Root 'Runtime2026/InstallRecovery'
        $plan=New-McpDeploymentPlan @options
        $dll=Join-Path $addins 'BimAgentLab.RequiredInformation/BimAgentLab.RevitReadiness.dll'
    }
    $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot $recovery
    if($Scenario -eq 'probe-interrupted') {
        & (Get-Module McpDeployment) {$script:TestBoundary={param($name) if($name -eq 'Manifest'){throw 'INJECTED probe manifest boundary'}}}
        try {Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt;throw 'Probe fault was not injected'}
        catch {if($_ -notlike '*INJECTED probe manifest boundary*'){throw}}
        & (Get-Module McpDeployment) {$script:TestBoundary=$null}
        Restore-McpDeployment -Lease $lease -ReceiptPath $receipt
        if(Test-Path -LiteralPath $dll){throw 'Interrupted probe retained its DLL.'}
        if((Get-FileHash -LiteralPath $sentinel).Hash -ne $before){throw 'Interrupted probe changed 2024.'}
        'PASS interrupted single-root probe installation rolls back exactly'; return
    }
    Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
    if((Get-RevitAssemblyInfo -LiteralPath $dll).TargetFramework -ne '.NETCoreApp,Version=v10.0'){throw 'Installed wrong framework.'}
    $state=Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
    foreach($op in $state.Operations){if($op.After -and (Get-FileHash -LiteralPath $op.Target).Hash -ne $op.After){throw 'Installed hash mismatch.'}}
    if($Scenario -eq 'reinstall') {
        $firstHash=(Get-FileHash -LiteralPath $dll).Hash
        $secondPlan=New-McpDeploymentPlan @options
        $secondReceipt=Save-McpDeploymentRecovery -Lease $lease -Plan $secondPlan -RecoveryRoot $recovery
        Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $secondReceipt
        Restore-McpDeployment -Lease $lease -ReceiptPath $secondReceipt
        Restore-McpDeployment -Lease $lease -ReceiptPath $secondReceipt
        if((Get-FileHash -LiteralPath $dll).Hash -ne $firstHash){throw 'Undo of reinstall did not restore first candidate.'}
    }
    Restore-McpDeployment -Lease $lease -ReceiptPath $receipt
    Restore-McpDeployment -Lease $lease -ReceiptPath $receipt
    if(Test-Path -LiteralPath $dll){throw 'Fresh install was not restored to absence.'}
    if((Get-FileHash -LiteralPath $sentinel).Hash -ne $before){throw '2024 installation changed.'}
    "PASS $Scenario modern installed hashes, exact undo twice, 2024 preserved"
} finally {$lease.Dispose()}
