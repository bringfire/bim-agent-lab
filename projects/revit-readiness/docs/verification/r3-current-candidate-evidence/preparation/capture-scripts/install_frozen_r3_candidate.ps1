# Run only after the author has released the desktop connections. Uses the existing deployment engine.
$ErrorActionPreference='Stop'
$repo='D:/bim-agent-lab'
$activePath=Join-Path $repo '.superpowers/sdd/2026-10-20-revit-mcp-bridge/active-r3-candidate.json'
$active=Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json -AsHashtable
$run=$active.runRoot
if(Test-Path -LiteralPath (Join-Path $run 'installed-freeze.json')){throw 'Candidate already installed: do not repeat or replace acceptance binaries.'}
if(-not (Test-Path -LiteralPath (Join-Path $run 'offline-checks.json'))){throw 'Offline verification is not complete.'}
$serverRoot='C:/Users/bring/.bim-agent-lab/RevitReadiness/McpServer'
function Assert-Released {
 if(Get-Process -Name Revit -ErrorAction SilentlyContinue){throw 'Revit must remain closed.'}
 $busy=@(Get-CimInstance Win32_Process -Filter "Name = 'BimAgentLab.RevitReadiness.Mcp.exe'" | Where-Object {$_.ExecutablePath -and [IO.Path]::GetDirectoryName($_.ExecutablePath) -eq [IO.Path]::GetFullPath($serverRoot)})
 if($busy.Count){throw 'Installed readiness servers are still active; coordinate normal disconnect, never terminate them here.'}
}
Assert-Released
Import-Module (Join-Path $repo 'projects/revit-readiness/scripts/McpDeployment.psm1') -Force
$lease=Enter-McpDeploymentLock
try {
 Assert-Released
 $frozen=Get-Content -LiteralPath (Join-Path $run 'freeze.json') -Raw | ConvertFrom-Json
 if((Get-FileHash -LiteralPath (Join-Path $run 'package-files.json')).Hash -ne $frozen.packageManifestSha256){throw 'Frozen manifest changed.'}
 $package=Get-Content -LiteralPath (Join-Path $run 'package-files.json') -Raw | ConvertFrom-Json
 foreach($file in $package){if((Get-FileHash -LiteralPath (Join-Path $active.sourceRoot $file.path)).Hash -ne $file.sha256){throw "Frozen source changed: $($file.path)"}}
 $prior=Get-Content -LiteralPath (Join-Path $run 'prior-installation.json') -Raw | ConvertFrom-Json
 foreach($file in @($prior.targets)+@($prior.recoveryFiles)){
  $path=if($file.target){$file.target}else{$file.path}
  if((Get-FileHash -LiteralPath $path).Hash -ne $file.sha256){throw "Prior installed/recovery bytes changed: $path"}
 }
 if((Get-FileHash -LiteralPath $prior.receiptPath).Hash -ne $prior.receiptSha256){throw 'Prior receipt changed.'}
 $plan=New-McpDeploymentPlan -Lease $lease -SourceRoot $active.sourceRoot -RevitAddinsRoot (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024') -ServerRoot $serverRoot -IncludeMcpBridge
 $expected=@{};foreach($file in $prior.targets){$expected[$file.target]=$file.sha256}
 if($plan.Operations.Count -ne $expected.Count){throw 'Unexpected target inventory.'}
 foreach($op in $plan.Operations){if(-not $expected.ContainsKey($op.Target) -or $op.Before -ne $expected[$op.Target]){throw 'Plan baseline mismatch.'}}
 $physical=@(& (Join-Path $repo 'projects/revit-readiness/scripts/Get-PhysicalFilePath.ps1') -LiteralPath (@($plan.Operations.Target)+@($prior.receiptPath)))
 if(@($physical | Where-Object Redirected).Count){throw 'Unexpected path redirection before install.'}
 Assert-Released
 $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot (Join-Path (Split-Path $serverRoot) 'InstallRecovery')
 $receiptRoot=Split-Path $receipt
 $active['newReceipt']=$receipt;$active['status']='PreparedRecoveryBeforeApply'
 $active | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $activePath -Encoding utf8
 $receiptHash=(Get-FileHash -LiteralPath $receipt).Hash
 if($receiptHash -ne [IO.File]::ReadAllText((Join-Path $receiptRoot 'receipt.sha256'))){throw 'Prepared receipt hash mismatch.'}
 $prepared=Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
 $verified=0
 foreach($op in $prepared.Operations){
  foreach($pair in @(@('Before','Backup'),@('After','Staged'))){
   if($op.($pair[1])){if((Get-FileHash -LiteralPath (Join-Path $receiptRoot $op.($pair[1]))).Hash -ne $op.($pair[0])){throw 'Prepared recovery bytes mismatch.'};$verified++}
  }
 }
 [ordered]@{verifiedUtc=[DateTime]::UtcNow.ToString('o');receipt=$receipt;sha256=$receiptHash;state='Prepared';backupAndStagedFiles=$verified;installedWritesStarted=$false} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'prepared-barrier.json') -Encoding utf8
 Copy-Item -LiteralPath $receipt -Destination (Join-Path $run 'candidate-receipt.json')
 Assert-Released
 Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
 $installed=@()
 foreach($op in $prepared.Operations){
  if($op.After){if((Get-FileHash -LiteralPath $op.Target).Hash -ne $op.After){throw 'Installed hash mismatch.'}}
  elseif(Test-Path -LiteralPath $op.Target){throw 'Unexpected retained target.'}
  $installed+=@{target=$op.Target;sha256=$op.After;tag=$op.Tag}
 }
 $state=Get-Content -LiteralPath (Join-Path $receiptRoot 'state.json') -Raw | ConvertFrom-Json
 if($state.State -ne 'Committed'){throw 'Install not committed.'}
 $physical=@(& (Join-Path $repo 'projects/revit-readiness/scripts/Get-PhysicalFilePath.ps1') -LiteralPath (@($prepared.Operations | Where-Object After | ForEach-Object Target)+@($receipt)))
 if(@($physical | Where-Object Redirected).Count){throw 'Unexpected installed path redirection.'}
 $physical | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $run 'installed-physical-paths.json') -Encoding utf8
 [ordered]@{buildCommit=$active.buildCommit;normalCandidate=$true;frozenUtc=[DateTime]::UtcNow.ToString('o');receipt=$receipt;receiptSha256=$receiptHash;state=$state.State;targets=$installed;liveAcceptance=$false} | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $run 'installed-freeze.json') -Encoding utf8
 $active['status']='InstalledFrozenAwaitingLiveChecks'
 $active | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $activePath -Encoding utf8
 Write-Output "PASS installed frozen normal candidate: $($installed.Count) targets; receipt $receipt"
} finally { $lease.Dispose() }
