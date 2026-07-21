param(
    [string]$InputDirectory = (Join-Path $PSScriptRoot '..\fuentes\texto'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\fuentes\latex')
)

$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

function Convert-ToLatex([string]$Value) {
    $Value = $Value.Replace([char]7, ' ')
    $Value = $Value -replace '[\x00-\x06\x08\x0B\x0E-\x1F]', ''
    $Value = $Value.Replace('\', '\textbackslash{}')
    $Value = $Value.Replace('&', '\&')
    $Value = $Value.Replace('%', '\%')
    $Value = $Value.Replace('$', '\$')
    $Value = $Value.Replace('#', '\#')
    $Value = $Value.Replace('_', '\_')
    $Value = $Value.Replace('{', '\{')
    $Value = $Value.Replace('}', '\}')
    $Value = $Value.Replace('~', '\textasciitilde{}')
    $Value = $Value.Replace('^', '\textasciicircum{}')
    return $Value
}

function Convert-SourceToTex([string]$SourcePath, [string]$TargetPath) {
    $raw = Get-Content -LiteralPath $SourcePath -Raw -Encoding UTF8
    $raw = $raw.TrimStart([char]0xFEFF)
    $raw = $raw.Replace([char]12, "`n")
    $lines = $raw -split "`r?`n"
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            continue
        }
        $escaped = Convert-ToLatex $trimmed
        # La fuente se conserva lineal y literal; la estructura se marca en main.tex.
        $out.Add(($escaped + '\par')) | Out-Null
    }
    $content = New-Object System.Collections.Generic.List[string]
    $content.Add('\begingroup') | Out-Null
    $content.Add('\senertexto') | Out-Null
    foreach ($item in $out) { $content.Add($item) | Out-Null }
    $content.Add('\endgroup') | Out-Null
    Set-Content -LiteralPath $TargetPath -Value $content -Encoding UTF8
}

$ids = @('5770299','5770917','5772392','5774850','5787117','5788509','5789883','5790938','5787666','5788508')
foreach ($id in $ids) {
    Convert-SourceToTex (Join-Path $InputDirectory "$id.txt") (Join-Path $OutputDirectory "$id.tex")
}
