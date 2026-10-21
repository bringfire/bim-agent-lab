[CmdletBinding()]
param([switch]$IncludeFixtureTools,[string]$RevitInstallDir="$env:ProgramW6432\Autodesk\Revit 2024")
$ErrorActionPreference='Stop'
if(-not [Environment]::Is64BitProcess -or -not $IsWindows){throw 'Use 64-bit PowerShell 7 on Windows.'}
& (Join-Path $PSScriptRoot 'Build-Readiness.ps1') -IncludeFixtureTools:$IncludeFixtureTools -RevitInstallDir $RevitInstallDir
if($LASTEXITCODE -ne 0){throw 'Readiness build failed.'}
$project=Split-Path $PSScriptRoot -Parent
$publish=Join-Path $project 'samples/local/mcp-publish'
& dotnet publish (Join-Path $project 'src/mcp-server/BimAgentLab.RevitReadiness.Mcp.csproj') -c Release --self-contained false -p:PlatformTarget=x64 -p:RestoreLockedMode=true -o $publish --nologo
if($LASTEXITCODE -ne 0){throw 'Locked framework-dependent MCP publish failed.'}
$apphost=[IO.File]::ReadAllBytes((Join-Path $publish 'BimAgentLab.RevitReadiness.Mcp.exe'))
$pe=[BitConverter]::ToInt32($apphost,0x3c)
if([BitConverter]::ToUInt16($apphost,$pe+4) -ne 0x8664){throw 'Published apphost must be Windows x64.'}
$runtime=& dotnet --list-runtimes
if($LASTEXITCODE -ne 0 -or -not ($runtime -match '^Microsoft.NETCore.App 10\.')){throw '.NET 10 runtime is required to run the published server.'}
Write-Output "Published Windows x64 framework-dependent server: $publish"
