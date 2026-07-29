param(
    [Parameter(Mandatory = $true)]
    [string] $MigrationPath,

    [string] $ConfigurationPath = "appsettings.Development.json"
)

$ErrorActionPreference = "Stop"

$resolvedMigration = (Resolve-Path -LiteralPath $MigrationPath).Path
$resolvedConfiguration = (Resolve-Path -LiteralPath $ConfigurationPath).Path
$configuration = Get-Content -LiteralPath $resolvedConfiguration -Raw |
    ConvertFrom-Json
$connectionString = $configuration.ConnectionStrings.DefaultConnection
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "No se encontró ConnectionStrings:DefaultConnection."
}

$sql = Get-Content -LiteralPath $resolvedMigration -Raw
$connection = [System.Data.SqlClient.SqlConnection]::new(
    $connectionString)
$command = $connection.CreateCommand()
$command.CommandText = $sql
$command.CommandTimeout = 120

try {
    $connection.Open()
    [void]$command.ExecuteNonQuery()
    [pscustomobject]@{
        Migration = $resolvedMigration
        Applied = $true
    } | ConvertTo-Json -Compress
}
finally {
    $command.Dispose()
    $connection.Dispose()
}
