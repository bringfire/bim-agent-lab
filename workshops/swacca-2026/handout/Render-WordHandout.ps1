param(
    [string]$DocumentPath = (Join-Path $PSScriptRoot '../../../output/docx/swacca-2026-participant-handout-oct29-30.docx'),
    [string]$PdfPath = (Join-Path $PSScriptRoot '../../../tmp/pdfs/word-handout/word-render.pdf')
)

# Native Word rendering fallback for Windows without bundled LibreOffice.
# Creates an owned, hidden Word automation instance and opens only this file.
$ErrorActionPreference = 'Stop'
$inputFile = (Resolve-Path -LiteralPath $DocumentPath).Path
$outputFile = [IO.Path]::GetFullPath($PdfPath)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($outputFile)) | Out-Null
$handoutWord = $null
$handoutDocument = $null
try {
    Write-Output 'Starting owned Word renderer.'
    $handoutWord = New-Object -ComObject Word.Application
    $handoutWord.Visible = $false
    $handoutWord.DisplayAlerts = 0
    $handoutDocument = $handoutWord.Documents.Open($inputFile, $false, $true)
    $handoutDocument.Repaginate()
    $handoutDocument.ExportAsFixedFormat($outputFile, 17)
    [pscustomobject]@{
        WordVersion = $handoutWord.Version
        WordBuild = $handoutWord.Build
        Pages = $handoutDocument.ComputeStatistics(2)
        Output = $outputFile
    } | ConvertTo-Json
}
finally {
    if ($null -ne $handoutDocument) { $handoutDocument.Close(0) }
    if ($null -ne $handoutWord) { $handoutWord.Quit(0) }
}
