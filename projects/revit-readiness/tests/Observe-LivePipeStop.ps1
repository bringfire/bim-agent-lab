[CmdletBinding()]
param([Parameter(Mandatory)][int]$RevitPid,
      [Parameter(Mandatory)][string]$Evidence)
$ErrorActionPreference = 'Stop'
# Test observer only. No valid bridge request, Revit API access, or process termination.
if ((Get-Process -Id $RevitPid).ProcessName -ne 'Revit') { throw 'Expected a running Revit process.' }
if (Test-Path -LiteralPath $Evidence) { throw 'Use a new evidence directory.' }
$catalog = Join-Path $env:LOCALAPPDATA 'BimAgentLab/RevitBridge/sessions'
$candidates = @(Get-ChildItem -LiteralPath $catalog -Filter '*.json' | ForEach-Object {
    $value = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
    if ($value.RevitPid -eq $RevitPid) { [pscustomobject]@{ Path=$_.FullName; Value=$value } }
})
if ($candidates.Count -ne 1) { throw 'Expected exactly one selected bridge descriptor.' }
$selected = $candidates[0]
New-Item -ItemType Directory -Path $Evidence | Out-Null
$connections = @()
try {
    foreach ($mode in @('no-bytes','partial-header','partial-body')) {
        $pipe = [System.IO.Pipes.NamedPipeClientStream]::new('.', $selected.Value.PipeName,
            [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::Asynchronous)
        $entry = [pscustomobject]@{ Mode=$mode; Pipe=$pipe; Watch=$null; Read=$null; Buffer=[byte[]]::new(1) }
        $connections += $entry
        $pipe.Connect(2000)
        $entry.Watch = [System.Diagnostics.Stopwatch]::StartNew()
        if ($mode -eq 'partial-header') { $pipe.Write([byte[]]@(100),0,1) }
        if ($mode -eq 'partial-body') { $pipe.Write([byte[]]@(100,0,0,0,123),0,5) }
        $entry.Read = $pipe.ReadAsync($entry.Buffer,0,1)
    }
    [ordered]@{utc=[DateTime]::UtcNow.ToString('o');revitPid=$RevitPid;descriptor=$selected.Value;
        modes=@($connections.Mode);instruction='Run native Stop now. Closure at the five-second receive deadline does not satisfy this test.'} |
        ConvertTo-Json -Depth 6 | Set-Content (Join-Path $Evidence 'ready.json')
    $results = @()
    foreach ($entry in $connections) {
        $completed = [System.Threading.Tasks.Task]::WhenAny($entry.Read,[System.Threading.Tasks.Task]::Delay(7000)).GetAwaiter().GetResult()
        if ($completed -ne $entry.Read) { throw "Connection did not close: $($entry.Mode)" }
        $count = $entry.Read.GetAwaiter().GetResult()
        $results += [pscustomobject]@{mode=$entry.Mode;bytesReturned=$count;elapsedMilliseconds=$entry.Watch.ElapsedMilliseconds}
    }
    # EOF precedes descriptor cleanup: Stop drains every network worker first.
    # Observe this separate terminal state with its own bounded wait.
    $cleanupWatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ((Test-Path -LiteralPath $selected.Path) -and $cleanupWatch.ElapsedMilliseconds -lt 2000) { Start-Sleep -Milliseconds 10 }
    $descriptorRemoved = -not (Test-Path -LiteralPath $selected.Path)
    $processAlive = $null -ne (Get-Process -Id $RevitPid -ErrorAction SilentlyContinue)
    $pass = $descriptorRemoved -and $processAlive -and @($results | Where-Object { $_.bytesReturned -ne 0 -or $_.elapsedMilliseconds -ge 4500 }).Count -eq 0
    [ordered]@{utc=[DateTime]::UtcNow.ToString('o');results=$results;descriptorRemoved=$descriptorRemoved;
        descriptorWaitMilliseconds=$cleanupWatch.ElapsedMilliseconds;revitProcessAlive=$processAlive;
        pass=$pass;scope='Observed transport cleanup only; no callback/event-disposal inference.'} |
        ConvertTo-Json -Depth 6 | Set-Content (Join-Path $Evidence 'result.json')
    if (-not $pass) { throw 'Stop was not observed before the frame deadline, or cleanup did not match. Retain this attempt and retry with a new directory.' }
} finally {
    foreach ($entry in $connections) { $entry.Pipe.Dispose() }
}
