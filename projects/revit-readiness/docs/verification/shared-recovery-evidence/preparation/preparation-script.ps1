$ErrorActionPreference='Stop'
$repo='D:/bim-agent-lab'
$active=Get-Content -LiteralPath "$repo/.superpowers/sdd/2026-10-20-revit-mcp-bridge/active-shared-recovery.json" -Raw | ConvertFrom-Json
$run=$active.runRoot
$prepared=Get-Content -LiteralPath (Join-Path $run 'preparation.json') -Raw | ConvertFrom-Json
Import-Module "$repo/projects/revit-readiness/scripts/McpDeployment.psm1" -Force
$lease=Enter-McpDeploymentLock
try {
 $plan=New-McpDeploymentPlan -Lease $lease -SourceRoot $active.sourceRoot -RevitAddinsRoot (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024') -ServerRoot 'C:/Users/bring/.bim-agent-lab/RevitReadiness/McpServer' -IncludeMcpBridge
 if($plan.Operations.Count -ne $prepared.baselineTargets.Count){throw 'Unexpected target count.'}
 $expected=@{};foreach($row in $prepared.baselineTargets){$expected[$row.target]=$row}
 $inventoryDifferences=@()
 foreach($operation in $plan.Operations) {
  $row=$expected[$operation.Target]
  if(-not $row -or $operation.Before -ne $row.sha256){throw "Baseline changed: $($operation.Target)"}
  if($operation.After -ne $row.sha256) {
   if($operation.Tag -notin @('MainInventory','FixtureInventory','ServerInventory')){throw "Candidate differs: $($operation.Target)"}
   $old=@(Get-Content -LiteralPath (Join-Path $run $row.snapshot) -Raw | ConvertFrom-Json)
   $new=@([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($operation.Data)) | ConvertFrom-Json)
   if($old.Count -ne $new.Count -or @(Compare-Object $old $new -Property File,Sha256).Count -ne 0){throw 'Inventory semantics differ.'}
   $inventoryDifferences+=@{target=$operation.Target;before=$operation.Before;generatedAfter=$operation.After;exactMapPreserved=$true}
  }
 }
 $paths=@($prepared.baselineTargets.target)
 foreach($receipt in $prepared.receipts){$paths+=Join-Path 'C:/Users/bring/.bim-agent-lab/RevitReadiness/InstallRecovery' ($receipt.transaction+'/receipt.json')}
 $resolved=@(& "$repo/projects/revit-readiness/scripts/Get-PhysicalFilePath.ps1" -LiteralPath $paths)
 if(@($resolved | Where-Object Redirected).Count -ne 0){throw 'Unexpected filesystem redirection; do not recover.'}
 $resolved | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $run 'physical-paths.json') -Encoding utf8
 $processes=@(Get-CimInstance Win32_Process -Filter "Name = 'BimAgentLab.RevitReadiness.Mcp.exe'" | Select-Object ProcessId,ExecutablePath)
 [ordered]@{checkedUtc=[DateTime]::UtcNow.ToString('o');wholePlanOperations=$plan.Operations.Count;packageMatchesBaseline=$true;inventoryRepresentationDifferences=$inventoryDifferences;physicalFilesVerified=$resolved.Count;activeServers=$processes;actualUndoPerformed=$false;installedWrites=$false} | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $run 'preflight.json') -Encoding utf8
 Write-Output "PASS read-only whole-install preflight: $($plan.Operations.Count) targets, $($resolved.Count) direct paths; $($processes.Count) active servers. No receipt prepared or installation changed."
} finally {$lease.Dispose()}
