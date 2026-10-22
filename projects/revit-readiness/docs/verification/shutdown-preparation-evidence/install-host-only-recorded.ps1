$ErrorActionPreference='Stop'
Import-Module 'D:/bim-agent-lab/projects/revit-readiness/scripts/McpDeployment.psm1' -Force
$shutdownPackage='D:/bim-agent-lab/projects/revit-readiness/samples/local/diagnostic-packages/7b344ab607d84bd99edcb0cd990f47ce'
$shutdownEvidence='D:/bim-agent-lab/.superpowers/sdd/2026-10-20-revit-mcp-bridge'
$shutdownManifest=Get-Content -LiteralPath (Join-Path $shutdownPackage 'diagnostic-package.json') -Raw | ConvertFrom-Json
foreach($entry in $shutdownManifest.files) {
 if((Get-FileHash -LiteralPath (Join-Path $shutdownPackage $entry.relative)).Hash -ne $entry.sha256){throw 'Prepared package changed.'}
}
foreach($entry in $shutdownManifest.sources) {
 if((Get-FileHash -LiteralPath $entry.path).Hash -ne $entry.sha256){throw 'Diagnostic source changed after preparation.'}
}
$shutdownLease=Enter-McpDeploymentLock
try {
 $plan=New-McpDeploymentPlan -Lease $shutdownLease -SourceRoot $shutdownPackage -RevitAddinsRoot (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024') -ServerRoot 'C:/Users/bring/.bim-agent-lab/RevitReadiness/McpServer' -IncludeMcpBridge
 $full=@($plan.Operations)
 if($full.Count -ne 97 -or $plan.CreatedDirectories.Count -ne 0){throw 'Unexpected baseline inventory.'}
 # Retain byte-identical unrelated inventories despite ordering-only serialization differences.
 foreach($op in $full) {
  if($op.Tag -in @('FixtureInventory','ServerInventory') -and $op.Before -ne $op.After) {
   $old=@(Get-Content -LiteralPath $op.Target -Raw | ConvertFrom-Json)
   $new=@([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($op.Data)) | ConvertFrom-Json)
   if($old.Count -ne $new.Count -or @(Compare-Object $old $new -Property File,Sha256).Count -ne 0){throw 'Unrelated inventory content changed.'}
   $op.After=$op.Before
  }
 }
 $changed=@($full | Where-Object {$_.Before -ne $_.After})
 $expected=@('BimAgentLab.RevitReadiness.dll','BimAgentLab.RevitBridge.Runtime.dll','installed-files.json')
 if($changed.Count -ne 3){throw 'Expected exactly two host binaries and their inventory.'}
 foreach($op in $changed) {
  if($op.Tag -notin @('Main','MainInventory') -or (Split-Path $op.Target -Leaf) -notin $expected){throw 'Unexpected changed target.'}
 }
 $plan.Operations=[Collections.Generic.List[object]]::new()
 foreach($op in $changed){$plan.Operations.Add($op)}
 $receipt=Save-McpDeploymentRecovery -Lease $shutdownLease -Plan $plan -RecoveryRoot $plan.RecoveryRoot
 $record=[ordered]@{receipt=$receipt;transaction=$plan.Transaction;sourceCommit=(& git -C D:/bim-agent-lab rev-parse HEAD);sourcePackage=$shutdownPackage;wholePreflightCount=$full.Count;changedCount=$changed.Count;files=@($full | ForEach-Object {[ordered]@{Target=$_.Target;Before=$_.Before;After=$_.After;Tag=$_.Tag}})}
 $recordPath=Join-Path $shutdownEvidence ('shutdown-install-'+$plan.Transaction+'.json')
 $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding utf8
 Write-Output "PREPARED_RECEIPT=$receipt"
 Invoke-McpDeploymentPlan -Lease $shutdownLease -ReceiptPath $receipt
 foreach($op in $full){if((Get-FileHash -LiteralPath $op.Target).Hash -ne $op.After){throw 'Whole-install verification failed.'}}
 Write-Output "VERIFIED_TARGETS=$($full.Count) CHANGED=$($changed.Count) RECORD=$recordPath"
} finally {$shutdownLease.Dispose()}
