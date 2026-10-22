$ErrorActionPreference='Stop'
$repo='D:/bim-agent-lab'
$active=Get-Content "$repo/.superpowers/sdd/2026-10-20-revit-mcp-bridge/active-shared-recovery.json" -Raw | ConvertFrom-Json
$run=$active.runRoot
$evidence=Join-Path $run 'execution'
$prepared=Get-Content (Join-Path $run 'preparation.json') -Raw | ConvertFrom-Json
function Assert-ClientsStopped {
 if(@(Get-Process Revit,'BimAgentLab.RevitReadiness.Mcp' -ErrorAction SilentlyContinue).Count){throw 'Revit/readiness server restarted; leave files alone.'}
}
Assert-ClientsStopped
Import-Module "$repo/projects/revit-readiness/scripts/McpDeployment.psm1" -Force
$lease=Enter-McpDeploymentLock
try {
 $plan=New-McpDeploymentPlan -Lease $lease -SourceRoot $active.sourceRoot -RevitAddinsRoot (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024') -ServerRoot 'C:/Users/bring/.bim-agent-lab/RevitReadiness/McpServer' -IncludeMcpBridge
 if($plan.Operations.Count -ne 97){throw 'Unexpected whole-plan size.'}
 $expected=@{};foreach($row in $prepared.baselineTargets){$expected[$row.target]=$row}
 $inventoryRepresentations=@()
 foreach($op in $plan.Operations) {
  $row=$expected[$op.Target]
  if(-not $row){throw 'Unexpected planned target.'}
  $baseline=Join-Path $run $row.snapshot
  if((Get-FileHash -LiteralPath $baseline).Hash -ne $row.sha256){throw 'Snapshot mismatch.'}
  if($op.After -ne $row.sha256) {
   if($op.Tag -notin @('MainInventory','FixtureInventory','ServerInventory')){throw "Candidate mismatch: $($op.Target)"}
   $old=@(Get-Content -LiteralPath $baseline -Raw | ConvertFrom-Json)
   $new=@([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($op.Data)) | ConvertFrom-Json)
   if($old.Count -ne $new.Count -or @(Compare-Object $old $new -Property File,Sha256).Count){throw 'Inventory maps differ.'}
   $inventoryRepresentations+=@{target=$op.Target;generatedHash=$op.After;baselineHash=$row.sha256;exactFileHashMap=$true}
   $op.Data=[Convert]::ToBase64String([IO.File]::ReadAllBytes($baseline));$op.After=$row.sha256
  }
 }
 [ordered]@{checkedUtc=[DateTime]::UtcNow.ToString('o');operations=$plan.Operations | Select-Object Target,Before,After,Tag;createdDirectories=$plan.CreatedDirectories;inventoryRepresentations=$inventoryRepresentations;transaction=$plan.Transaction} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $evidence 'reinstall-plan.json') -Encoding utf8
 Assert-ClientsStopped
 $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot 'C:/Users/bring/.bim-agent-lab/RevitReadiness/InstallRecovery'
 [ordered]@{receipt=$receipt;preparedUtc=[DateTime]::UtcNow.ToString('o');state=Get-Content (Join-Path (Split-Path $receipt) 'state.json') -Raw | ConvertFrom-Json} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $evidence 'reinstall-prepared.json') -Encoding utf8
 # Prepared is durable and its path retained even if a client restart now prevents apply.
 Assert-ClientsStopped
 Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
 [ordered]@{receipt=$receipt;finishedUtc=[DateTime]::UtcNow.ToString('o');state=Get-Content (Join-Path (Split-Path $receipt) 'state.json') -Raw | ConvertFrom-Json} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $evidence 'reinstall-committed.json') -Encoding utf8
} finally {$lease.Dispose()}
