[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Import-Module (Join-Path $PSScriptRoot 'McpDeployment.psm1') -Force
$lease=Enter-McpDeploymentLock
try {Restore-McpDeployment -Lease $lease -ReceiptPath $ReceiptPath;Write-Output "Restored: $ReceiptPath"} finally {$lease.Dispose()}
