[CmdletBinding()]
param([ValidateSet('2024','2026')][string]$RevitYear='2024',[string]$RevitAddinsRoot,[string]$RecoveryRoot)
$ErrorActionPreference='Stop'
if($PSVersionTable.PSVersion.Major -lt 7 -or -not [Environment]::Is64BitProcess){throw 'Run with 64-bit PowerShell 7 (pwsh).'}
if(-not $RevitAddinsRoot){$RevitAddinsRoot=Join-Path $env:APPDATA "Autodesk/Revit/Addins/$RevitYear"}
if(-not $RecoveryRoot){$RecoveryRoot=Join-Path ([Environment]::GetFolderPath('UserProfile')) ".bim-agent-lab/RevitReadiness/ProbeRecovery/$RevitYear"}
Import-Module (Join-Path $PSScriptRoot 'McpDeployment.psm1') -Force
$lease=Enter-McpDeploymentLock
try {
    $plan=New-RevitProbeDeploymentPlan -Lease $lease -SourceRoot (Split-Path $PSScriptRoot -Parent) -RevitAddinsRoot $RevitAddinsRoot -RecoveryRoot $RecoveryRoot -RevitYear $RevitYear
    $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot $RecoveryRoot
    Write-Output "Prepared probe recovery receipt: $receipt"
    Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
    Write-Output "Start Revit $RevitYear, open a project, then use Add-Ins > External Tools > BIM Agent Lab - Read-only probe."
} finally {$lease.Dispose()}
