[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'Use PowerShell 7.' }
if (Get-Process Revit -ErrorAction SilentlyContinue) { throw 'Close Revit before preparing the diagnostic installation.' }
$project=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$installed=Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2024/BimAgentLab.RequiredInformation'
$server=Join-Path ([Environment]::GetFolderPath('UserProfile')) '.bim-agent-lab/RevitReadiness/McpServer'
foreach($folder in @($installed,$server)) {
    foreach($entry in (Get-Content -LiteralPath (Join-Path $folder 'installed-files.json') -Raw | ConvertFrom-Json)) {
        $file=[IO.Path]::GetFullPath((Join-Path $folder $entry.File))
        if (-not $file.StartsWith($folder+'\',[StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid baseline inventory path.' }
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $entry.Sha256) { throw "Installed baseline changed: $file" }
    }
}
$package=Join-Path $project ('samples/local/diagnostic-packages/'+[guid]::NewGuid().ToString('N'))
& dotnet build (Join-Path $PSScriptRoot 'BimAgentLab.RevitReadiness.Diagnostics.csproj') -c Release --nologo -p:RestoreLockedMode=true
if ($LASTEXITCODE -ne 0) { throw 'Diagnostic build failed.' }
if (Test-Path -LiteralPath $package) { throw 'Use a new package directory.' }
New-Item -ItemType Directory -Path $package | Out-Null
# Diagnostic add-in plus the runtime's disposal observation are compiled together.
# Core, contract, exporter, external server and notices retain installed bytes.
$files=@{}
foreach($name in 'BimAgentLab.RevitReadiness.dll','BimAgentLab.Readiness.Core.dll','BimAgentLab.RevitBridge.Runtime.dll','BimAgentLab.RevitBridge.Contract.dll') {
    $files["src/revit-addin/bin/Release/net48/$name"] = if($name -in @('BimAgentLab.RevitReadiness.dll','BimAgentLab.RevitBridge.Runtime.dll')) { Join-Path $PSScriptRoot "bin/Release/net48/$name" } else { Join-Path $installed $name }
}
foreach($relative in 'src/revit-addin/BimAgentLab.ReadinessBridge.addin.template','scripts/legacy-fixture-baseline.json') {
    $files[$relative]=Join-Path $project $relative
}
foreach($pair in @(
    @('src/excel-report/bin/Release/net48',(Join-Path $installed 'Excel')),
    @('samples/local/mcp-publish',$server),
    @('third-party',(Join-Path $installed 'third-party'))
)) {
    $relative=$pair[0]; $folder=$pair[1]
    if (-not (Test-Path -LiteralPath $folder)) { throw "Build the existing release package first: $relative" }
    foreach($file in Get-ChildItem -LiteralPath $folder -File -Recurse) {
        if ($file.Name -eq 'installed-files.json' -and $file.DirectoryName -eq $folder) { continue }
        $child=[IO.Path]::GetRelativePath($folder,$file.FullName)
        # The standard installer adds these from third-party/mcp itself.
        if ($relative -eq 'samples/local/mcp-publish' -and $child.StartsWith('third-party\',[StringComparison]::OrdinalIgnoreCase)) { continue }
        $files[$relative+'/'+$child]=$file.FullName
    }
}
$copied=@()
foreach($relative in $files.Keys) {
    $destination=Join-Path $package $relative
    New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
    Copy-Item -LiteralPath $files[$relative] -Destination $destination
    $before=(Get-FileHash -LiteralPath $files[$relative] -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $before) { throw "Package copy mismatch: $relative" }
    $copied+= [ordered]@{relative=$relative;source=$files[$relative];sha256=$before}
}
$sources=@(Get-ChildItem -LiteralPath (Join-Path $project 'src/revit-addin') -File -Filter '*.cs') + @(Get-ChildItem -LiteralPath $PSScriptRoot -File -Filter '*.cs')
$sources+=@(Get-ChildItem -LiteralPath (Join-Path $project 'src/bridge-runtime') -File -Filter '*.cs')
$sourceHashes=@($sources | ForEach-Object { [ordered]@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash} })
$manifest=[ordered]@{diagnosticOnly=$true;preparedUtc=[DateTimeOffset]::UtcNow.ToString('o');sourceRoot=$package;files=$copied;sources=$sourceHashes;complete=$true}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $package 'diagnostic-package.json') -Encoding utf8
Write-Output "DIAGNOSTIC_PACKAGE=$package"
Write-Output 'Prepared only. Install with the existing New/Save/Invoke-McpDeploymentPlan functions and retain its undo receipt.'
