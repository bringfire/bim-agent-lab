[CmdletBinding()]
param([switch]$IncludeMcpBridge,[switch]$IncludeFixtureTools,
    [string]$FixtureSourceCopy,[string]$RuntimeRoot,
    [string]$SourceRoot=(Split-Path $PSScriptRoot -Parent),
    [ValidateSet('2024','2026')][string]$RevitYear='2024',[string]$RevitAddinsRoot)
$ErrorActionPreference='Stop'
if($PSVersionTable.PSVersion.Major -lt 7 -or -not [Environment]::Is64BitProcess){throw 'Run with 64-bit PowerShell 7 (pwsh).'}
. (Join-Path $PSScriptRoot 'RevitTarget.ps1')
$target=Get-RevitTarget -RevitYear $RevitYear
if(-not $RuntimeRoot){$RuntimeRoot=Join-Path ([Environment]::GetFolderPath('UserProfile')) $(if($RevitYear -eq '2024'){'.bim-agent-lab/RevitReadiness'}else{'.bim-agent-lab/RevitReadiness/Revit2026'})}
if(-not $RevitAddinsRoot){$RevitAddinsRoot=Join-Path $env:APPDATA "Autodesk/Revit/Addins/$RevitYear"}
if(-not $FixtureSourceCopy){$FixtureSourceCopy=Join-Path $PSScriptRoot $(if($RevitYear -eq '2024'){'../samples/local/required-information/Snowdon-BAL-source-copy.rvt'}else{'../samples/local/revit2026-source/Snowdon-BAL-source-copy.rvt'})}
Assert-RevitAssembly -LiteralPath (Join-Path $SourceRoot "src/revit-addin/$($target.OutputRelative)/BimAgentLab.RevitReadiness.dll") -RevitYear $RevitYear -RequireRevitApi
Import-Module (Join-Path $PSScriptRoot 'McpDeployment.psm1') -Force
$lease=Enter-McpDeploymentLock
try {
    # Packaged agent processes can redirect AppData into their private cache.
    # Use the shared profile tree; verify actual consuming applications separately.
    $root=[IO.Path]::GetFullPath($RuntimeRoot)
    $plan=New-McpDeploymentPlan -Lease $lease -SourceRoot $SourceRoot -RevitAddinsRoot $RevitAddinsRoot -ServerRoot (Join-Path $root 'McpServer') -IncludeMcpBridge:$IncludeMcpBridge -IncludeFixtureTools:$IncludeFixtureTools -FixtureSourceCopy $FixtureSourceCopy -RevitYear $RevitYear
    $receipt=Save-McpDeploymentRecovery -Lease $lease -Plan $plan -RecoveryRoot (Join-Path $root 'InstallRecovery')
    Write-Output "Prepared recovery receipt: $receipt"
    Invoke-McpDeploymentPlan -Lease $lease -ReceiptPath $receipt
} finally {$lease.Dispose()}
