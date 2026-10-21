# Run in a fresh Windows PowerShell 5.1 process. This is an outside-Revit smoke
# test of the installed replacement, not independent host/desktop acceptance.
$ErrorActionPreference='Stop'
$dll=Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'Autodesk/Revit/Addins/2024/BimAgentLab.RequiredInformation/BimAgentLab.RevitReadiness.dll'
$assembly=[Reflection.Assembly]::LoadFrom($dll)
$flags=[Reflection.BindingFlags]'Static,NonPublic'
$root=$assembly.GetType('BimAgentLab.RevitReadiness.DiagnosticRunDirectory',$true).GetProperty('Root',$flags).GetValue($null)
$expected=Join-Path ([Environment]::GetFolderPath('UserProfile')) '.bim-agent-lab/RevitBridge/diagnostics'
if([IO.Path]::GetFullPath($root) -ne [IO.Path]::GetFullPath($expected)){throw 'Installed candidate uses the wrong shared root'}
$selector=Join-Path $root 'run.txt'
$run=[IO.File]::ReadAllText($selector).Trim()
$assembly.GetType('BimAgentLab.RevitReadiness.BridgeDiagnostics',$true).GetMethod('Initialize',$flags).Invoke($null,@()) | Out-Null
$events=Join-Path (Join-Path (Join-Path $root $run) $PID) 'events.tsv'
[ordered]@{
    Scope='Installed replacement Initialize outside Revit; not host acceptance'
    CapturedUtc=[DateTimeOffset]::UtcNow.ToString('o')
    ProcessId=$PID
    Runtime=[Environment]::Version.ToString()
    AssemblyPath=$dll
    AssemblySha256=(Get-FileHash -LiteralPath $dll).Hash.ToLowerInvariant()
    Root=$root
    Selector=$selector
    Run=$run
    EventsPath=$events
    Events=[IO.File]::ReadAllText($events)
    Result='Initialize returned successfully'
} | ConvertTo-Json -Depth 4
