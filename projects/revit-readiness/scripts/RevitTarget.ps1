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

function Get-RevitAssemblyInfo {
    param([Parameter(Mandatory)][string]$LiteralPath)
    # Inspect PE metadata only. Never load Autodesk or plugin code into the installer.
    $stream=[IO.File]::OpenRead([IO.Path]::GetFullPath($LiteralPath));$pe=$null
    try {
        $pe=[Reflection.PortableExecutable.PEReader]::new($stream)
        $metadata=[Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        $framework=$null;$apiMajor=$null
        foreach($handle in $metadata.CustomAttributes) {
            $attribute=$metadata.GetCustomAttribute($handle)
            if($attribute.Constructor.Kind -ne [Reflection.Metadata.HandleKind]::MemberReference){continue}
            $constructor=$metadata.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
            if($constructor.Parent.Kind -ne [Reflection.Metadata.HandleKind]::TypeReference){continue}
            $type=$metadata.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
            if($metadata.GetString($type.Name) -eq 'TargetFrameworkAttribute' -and $metadata.GetString($type.Namespace) -eq 'System.Runtime.Versioning') {
                $blob=$metadata.GetBlobReader($attribute.Value)
                if($blob.ReadUInt16() -ne 1){throw 'Invalid framework attribute prolog.'}
                $framework=$blob.ReadSerializedString()
            }
        }
        foreach($handle in $metadata.AssemblyReferences) {
            $reference=$metadata.GetAssemblyReference($handle)
            if($metadata.GetString($reference.Name) -eq 'RevitAPI'){$apiMajor=$reference.Version.Major}
        }
        if(-not $framework){throw "Assembly has no target framework: $LiteralPath"}
        [pscustomobject]@{TargetFramework=$framework;RevitApiMajor=$apiMajor}
    } finally {if($pe){$pe.Dispose()};$stream.Dispose()}
}

function Assert-RevitAssembly {
    param([Parameter(Mandatory)][string]$LiteralPath,[ValidateSet('2024','2026')][string]$RevitYear='2024',[switch]$RequireRevitApi)
    $info=Get-RevitAssemblyInfo -LiteralPath $LiteralPath
    $expected=if($RevitYear -eq '2024'){'.NETFramework,Version=v4.8'}else{'.NETCoreApp,Version=v10.0'}
    if($info.TargetFramework -ne $expected){throw "Assembly framework target does not match Revit $RevitYear : $LiteralPath"}
    if($RequireRevitApi -and $info.RevitApiMajor -ne ([int]$RevitYear-2000)){throw "Assembly API year does not match Revit $RevitYear : $LiteralPath"}
}
