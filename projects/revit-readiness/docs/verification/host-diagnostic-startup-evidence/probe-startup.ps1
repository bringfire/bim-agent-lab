# Investigation probe: run in a NEW Windows PowerShell 5.1 process (net48).
# Invokes the installed diagnostic initializer without Revit or model access.
# It creates that probe process's own PID subdirectory in the prepared run.
$ErrorActionPreference = 'Stop'
$root = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'BimAgentLab/RevitBridge/diagnostics'
$selector = Join-Path $root 'run.txt'
$run = [IO.File]::ReadAllText($selector).Trim()
$dll = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'Autodesk/Revit/Addins/2024/BimAgentLab.RequiredInformation/BimAgentLab.RevitReadiness.dll'
$assembly = [Reflection.Assembly]::LoadFrom($dll)
$type = $assembly.GetType('BimAgentLab.RevitReadiness.BridgeDiagnostics', $true)
$method = $type.GetMethod('Initialize', [Reflection.BindingFlags]'Static,NonPublic')
$method.Invoke($null, @()) | Out-Null
$events = Join-Path (Join-Path (Join-Path $root $run) $PID) 'events.tsv'
[ordered]@{
    Scope = 'Installed diagnostic Initialize outside Revit; not host acceptance'
    CapturedUtc = [DateTimeOffset]::UtcNow.ToString('o')
    ProcessId = $PID
    Identity = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    Runtime = [Environment]::Version.ToString()
    AssemblyPath = $dll
    AssemblySha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant()
    Selector = $selector
    SelectorCreatedUtc = (Get-Item -LiteralPath $selector).CreationTimeUtc.ToString('o')
    Run = $run
    EventsPath = $events
    Events = [IO.File]::ReadAllText($events)
    Result = 'Initialize returned successfully'
} | ConvertTo-Json -Depth 4
