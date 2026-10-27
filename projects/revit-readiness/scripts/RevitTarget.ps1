function Get-RevitTarget {
    param([ValidateSet('2024','2026')][string]$RevitYear='2024',[string]$RevitInstallDir)
    if(-not $RevitInstallDir){$RevitInstallDir=Join-Path $env:ProgramW6432 "Autodesk/Revit $RevitYear"}
    [pscustomobject]@{
        Year=$RevitYear
        RevitInstallDir=[IO.Path]::GetFullPath($RevitInstallDir)
        Framework=$(if($RevitYear -eq '2024'){'net48'}else{'net10.0-windows'})
        OutputRelative=$(if($RevitYear -eq '2024'){'bin/Release/net48'}else{'bin/Revit2026/Release/net10.0-windows'})
    }
}
