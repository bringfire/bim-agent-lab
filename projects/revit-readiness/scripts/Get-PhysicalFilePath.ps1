# Read-only inspection of the actual file opened by this process. It does not
# prove that another application can access the requested path; test that too.
[CmdletBinding()]
param([Parameter(Mandatory)][string[]]$LiteralPath)
$ErrorActionPreference='Stop'
if (-not ('BimAgentLab.FileVisibility' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
namespace BimAgentLab {
    public static class FileVisibility {
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint size, uint flags);
    }
}
'@
}
foreach ($path in $LiteralPath) {
    $requested=[IO.Path]::GetFullPath($path)
    $file=[IO.File]::Open($requested,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
    try {
        $buffer=[Text.StringBuilder]::new(32768)
        $length=[BimAgentLab.FileVisibility]::GetFinalPathNameByHandle($file.SafeFileHandle,$buffer,32768,0)
        if($length -eq 0){throw [ComponentModel.Win32Exception]::new([Runtime.InteropServices.Marshal]::GetLastWin32Error())}
        if($length -ge 32768){throw 'Physical path exceeded the inspection buffer.'}
        $physical=$buffer.ToString()
        $comparison=if($physical.StartsWith('\\?\UNC\')){'\\'+$physical.Substring(8)}elseif($physical.StartsWith('\\?\')){$physical.Substring(4)}else{$physical}
        [pscustomobject]@{Requested=$requested;Physical=$physical;Redirected=(-not $requested.Equals($comparison,[StringComparison]::OrdinalIgnoreCase))}
    } finally {$file.Dispose()}
}
