param(
    [string]$Configuration = "Release",
    [string]$PublishPath = (Join-Path (Resolve-Path "$PSScriptRoot\\..").Path "out\\publish\\dgmesnie"),
    [string]$Project = "NSIE.csproj",
    [switch]$BuildFirst,
    [int]$KeepPackages = 3
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path "$PSScriptRoot\..").Path
$publishRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "out\publish"))
$resolvedPublishPath = [System.IO.Path]::GetFullPath($PublishPath)
$allowedPrefix = $publishRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar

if (-not $resolvedPublishPath.StartsWith($allowedPrefix, [System.StringComparison]::OrdinalIgnoreCase))
{
    throw "La salida debe permanecer dentro de $publishRoot"
}

$PublishPath = $resolvedPublishPath
$Project = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $Project))

if ($BuildFirst)
{
    Write-Host "Ejecutando build..."
    dotnet build $Project -c $Configuration --no-restore
}

if (Test-Path -LiteralPath $PublishPath)
{
    Remove-Item -LiteralPath $PublishPath -Recurse -Force
}
New-Item -Path $PublishPath -ItemType Directory -Force | Out-Null

Write-Host "Publicando a: $PublishPath"
dotnet publish $Project -c $Configuration --no-restore -o $PublishPath
if ($LASTEXITCODE -ne 0) { throw "dotnet publish terminó con código $LASTEXITCODE" }

$publishedFiles = Get-ChildItem -LiteralPath $PublishPath -Recurse -File
$publishedBytes = ($publishedFiles | Measure-Object Length -Sum).Sum
$manifest = [ordered]@{
    generatedAt = (Get-Date).ToString("o")
    configuration = $Configuration
    fileCount = $publishedFiles.Count
    sizeBytes = $publishedBytes
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PublishPath "publish-manifest.json") -Encoding UTF8

$zipPath = Join-Path (Split-Path $PublishPath -Parent) "$((Get-Date).ToString('yyyyMMdd_HHmmss'))-dgmesnie-publish.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Generando ZIP para FileZilla: $zipPath"
Compress-Archive -Path (Join-Path $PublishPath '*') -DestinationPath $zipPath -Force

if ($KeepPackages -ge 0)
{
    Get-ChildItem -LiteralPath (Split-Path $PublishPath -Parent) -Filter "*-dgmesnie-publish.zip" -File |
        Sort-Object LastWriteTime -Descending |
        Select-Object -Skip $KeepPackages |
        Remove-Item -Force
}

Write-Host "Listo."
Write-Host "Carpeta de salida: $PublishPath"
Write-Host "ZIP para subir por FileZilla: $zipPath"
Write-Host "Archivos publicados: $($publishedFiles.Count)"
Write-Host "Tamaño: $([math]::Round($publishedBytes / 1MB, 2)) MB"
