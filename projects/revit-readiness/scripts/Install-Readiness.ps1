[CmdletBinding()]
param([switch]$IncludeMcpBridge,[switch]$IncludeFixtureTools,
    [string]$FixtureSourceCopy=(Join-Path $PSScriptRoot '../samples/local/required-information/Snowdon-BAL-source-copy.rvt'),
    [string]$RuntimeRoot=(Join-Path ([Environment]::GetFolderPath('UserProfile')) '.bim-agent-lab/RevitReadiness'),
    [string]$SourceRoot=(Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference='Stop'
if($PSVersionTable.PSVersion.Major -lt 7 -or -not [Environment]::Is64BitProcess){throw 'Run with 64-bit PowerShell 7 (pwsh).'}
Import-Module (Join-Path $PSScriptRoot 'McpDeployment.psm1') -Force
$lease=Enter-McpDeploymentLock
try {
    # Packaged agent processes can redirect AppData into their private cache.
    # Use the shared profile tree; verify actual consuming applications separately.
    $root=[IO.Path]::GetFullPath($RuntimeRoot)
    $plan=New-McpDeploymentPlan -Lease $lease -SourceRoot $SourceRoot -RevitAddinsRoot (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024') -ServerRoot (Join-Path $root 'McpServer') -IncludeMcpBridge:$IncludeMcpBridge -IncludeFixtureTools:$IncludeFixtureTools -FixtureSourceCopy $FixtureSourceCopy
    $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot (Join-Path $root 'InstallRecovery')
    Write-Output "Prepared recovery receipt: $receipt"
    Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
} finally {$lease.Dispose()}
