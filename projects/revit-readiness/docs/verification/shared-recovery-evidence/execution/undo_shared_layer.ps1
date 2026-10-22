param([Parameter(Mandatory)][ValidateRange(0,2)][int]$Index)
$ErrorActionPreference='Stop'
$repo='D:/bim-agent-lab'
$workspace=Join-Path $repo '.superpowers/sdd/2026-10-20-revit-mcp-bridge'
$active=Get-Content (Join-Path $workspace 'active-shared-recovery.json') -Raw | ConvertFrom-Json
$prepared=Get-Content (Join-Path $active.runRoot 'preparation.json') -Raw | ConvertFrom-Json
$evidence=Join-Path $active.runRoot 'execution'
$record=$prepared.receipts[$Index]
$receipt=Join-Path 'C:/Users/bring/.bim-agent-lab/RevitReadiness/InstallRecovery' ($record.transaction+'/receipt.json')
foreach($pass in @('undo','repeat')) {
 if(@(Get-Process Revit,'BimAgentLab.RevitReadiness.Mcp' -ErrorAction SilentlyContinue).Count){throw 'Revit/readiness server restarted.'}
 $log=Join-Path $evidence "layer-$Index-$pass.txt"
 if(Test-Path -LiteralPath $log){throw 'Do not overwrite evidence.'}
 & pwsh -NoProfile -File "$repo/projects/revit-readiness/scripts/Undo-McpBridgeInstall.ps1" -ReceiptPath $receipt *> $log
 $code=$LASTEXITCODE
 Get-Content -LiteralPath $log
 if($code -ne 0){throw "Undo child failed: $code. Preserve receipt and inspect log."}
 & python (Join-Path $workspace 'audit_shared_recovery.py') $Index (Join-Path $evidence "layer-$Index-$pass.json")
 if($LASTEXITCODE -ne 0){throw 'Independent state audit failed; do not proceed.'}
}
