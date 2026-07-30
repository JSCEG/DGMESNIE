param(
    [string]$Configuration = "Release",
    [string]$PublishPath = (Join-Path (Resolve-Path "$PSScriptRoot\\..").Path "out\\publish\\dgmesnie"),
    [string]$Project = "NSIE.csproj",
    [switch]$BuildFirst
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($BuildFirst)
{
    Write-Host "Ejecutando build..."
    dotnet build $Project -c $Configuration --no-restore
}

if (Test-Path $PublishPath)
{
    Remove-Item -Path (Join-Path $PublishPath '*') -Recurse -Force
}
New-Item -Path $PublishPath -ItemType Directory -Force | Out-Null

Write-Host "Publicando a: $PublishPath"
dotnet publish $Project -c $Configuration --no-restore -o $PublishPath

$zipPath = Join-Path (Split-Path $PublishPath -Parent) "$((Get-Date).ToString('yyyyMMdd_HHmmss'))-dgmesnie-publish.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Generando ZIP para FileZilla: $zipPath"
Compress-Archive -Path (Join-Path $PublishPath '*') -DestinationPath $zipPath -Force

Write-Host "Listo."
Write-Host "Carpeta de salida: $PublishPath"
Write-Host "ZIP para subir por FileZilla: $zipPath"
