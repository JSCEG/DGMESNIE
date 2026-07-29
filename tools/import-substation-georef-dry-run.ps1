param(
    [Parameter(Mandatory = $true)]
    [string] $ReportPath,

    [string] $ConfigurationPath = "appsettings.Development.json"
)

$ErrorActionPreference = "Stop"

$resolvedReport = (Resolve-Path -LiteralPath $ReportPath).Path
$resolvedConfiguration = (Resolve-Path -LiteralPath $ConfigurationPath).Path
$configuration = Get-Content -LiteralPath $resolvedConfiguration -Raw |
    ConvertFrom-Json
$connectionString = $configuration.ConnectionStrings.DefaultConnection
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "No se encontró ConnectionStrings:DefaultConnection."
}

$report = Get-Content -LiteralPath $resolvedReport -Raw | ConvertFrom-Json
if ($report.mutatesInventory -ne $false) {
    throw "El archivo no corresponde a un dry-run seguro."
}
$generatedUtc = if ($report.generatedUtc -is [DateTime]) {
    $report.generatedUtc.ToUniversalTime()
}
else {
    [DateTime]::Parse(
        [string]$report.generatedUtc,
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
}

$connection = [System.Data.SqlClient.SqlConnection]::new(
    $connectionString)
$connection.Open()
$transaction = $connection.BeginTransaction()

function Add-Parameter {
    param(
        [System.Data.SqlClient.SqlCommand] $Command,
        [string] $Name,
        [System.Data.SqlDbType] $Type,
        [object] $Value,
        [int] $Size = 0
    )

    $parameter = if ($Size -ne 0) {
        $Command.Parameters.Add($Name, $Type, $Size)
    }
    else {
        $Command.Parameters.Add($Name, $Type)
    }
    if ($Type -eq [System.Data.SqlDbType]::Decimal) {
        $parameter.Precision = 18
        $parameter.Scale = 7
    }
    $parameter.Value = if (
        $null -eq $Value -or
        ($Value -is [string] -and [string]::IsNullOrWhiteSpace($Value))
    ) {
        [DBNull]::Value
    }
    else {
        $Value
    }
}

try {
    $executionCommand = $connection.CreateCommand()
    $executionCommand.Transaction = $transaction
    $executionCommand.CommandText = @"
INSERT INTO dgmesnie.RedElectricaSubestacionGeorefEjecucion
(
    GeneradoUtc,
    Regla,
    RutaReporte,
    RegistrosObjetivo,
    IdentidadesObjetivo,
    ElementosOsm,
    CandidatosOsmConNombre,
    AltaConfianza,
    Revision,
    Ambiguas,
    Debiles,
    SinCoincidencia,
    FuenteOsm,
    FuenteGerencias,
    InventarioModificado
)
OUTPUT INSERTED.EjecucionId
VALUES
(
    @GeneradoUtc,
    @Regla,
    @RutaReporte,
    @RegistrosObjetivo,
    @IdentidadesObjetivo,
    @ElementosOsm,
    @CandidatosOsmConNombre,
    @AltaConfianza,
    @Revision,
    @Ambiguas,
    @Debiles,
    @SinCoincidencia,
    @FuenteOsm,
    @FuenteGerencias,
    0
);
"@
    Add-Parameter $executionCommand "@GeneradoUtc" DateTime2 $generatedUtc
    Add-Parameter $executionCommand "@Regla" NVarChar $report.rule 100
    Add-Parameter $executionCommand "@RutaReporte" NVarChar (
        $resolvedReport) 1000
    Add-Parameter $executionCommand "@RegistrosObjetivo" Int (
        [int]$report.summary.targetRecords)
    Add-Parameter $executionCommand "@IdentidadesObjetivo" Int (
        [int]$report.summary.uniqueTargetIdentities)
    Add-Parameter $executionCommand "@ElementosOsm" Int (
        [int]$report.summary.osmElements)
    Add-Parameter $executionCommand "@CandidatosOsmConNombre" Int (
        [int]$report.summary.namedOsmCandidates)
    Add-Parameter $executionCommand "@AltaConfianza" Int (
        [int]$report.summary.propuesta_alta_confianza)
    Add-Parameter $executionCommand "@Revision" Int (
        [int]$report.summary.propuesta_revision)
    Add-Parameter $executionCommand "@Ambiguas" Int (
        [int]$report.summary.coincidencia_ambigua)
    Add-Parameter $executionCommand "@Debiles" Int (
        [int]$report.summary.coincidencia_debil)
    Add-Parameter $executionCommand "@SinCoincidencia" Int (
        [int]$report.summary.sin_coincidencia)
    Add-Parameter $executionCommand "@FuenteOsm" NVarChar (
        $report.sources.openStreetMap) 1000
    Add-Parameter $executionCommand "@FuenteGerencias" NVarChar (
        $report.sources.gerenciasControl) 1000
    $executionId = [long]$executionCommand.ExecuteScalar()

    $candidateCommand = $connection.CreateCommand()
    $candidateCommand.Transaction = $transaction
    $candidateCommand.CommandText = @"
INSERT INTO dgmesnie.RedElectricaSubestacionGeorefCandidato
(
    EjecucionId,
    RegistroClave,
    ReferenciaOsm,
    IdentidadObjetivo,
    NombreAtlas,
    TensionAtlasKv,
    RegionAtlas,
    ZonaAtlas,
    NombreOsm,
    TensionOsmKv,
    RegionOsm,
    OperadorOsm,
    Latitud,
    Longitud,
    Puntaje,
    Margen,
    EstadoPropuesta,
    EsPrincipal,
    EvidenciasJson
)
VALUES
(
    @EjecucionId,
    @RegistroClave,
    @ReferenciaOsm,
    @IdentidadObjetivo,
    @NombreAtlas,
    @TensionAtlasKv,
    @RegionAtlas,
    @ZonaAtlas,
    @NombreOsm,
    @TensionOsmKv,
    @RegionOsm,
    @OperadorOsm,
    @Latitud,
    @Longitud,
    @Puntaje,
    @Margen,
    @EstadoPropuesta,
    @EsPrincipal,
    @EvidenciasJson
);
"@

    $inserted = 0
    foreach ($result in $report.results) {
        $principalReference = if ($null -ne $result.selectedCandidate) {
            $result.selectedCandidate.referenceKey
        }
        else {
            ""
        }

        foreach ($candidate in $result.candidates) {
            $candidateCommand.Parameters.Clear()
            Add-Parameter $candidateCommand "@EjecucionId" BigInt (
                $executionId)
            Add-Parameter $candidateCommand "@RegistroClave" NVarChar (
                $result.recordKey) 80
            Add-Parameter $candidateCommand "@ReferenciaOsm" NVarChar (
                $candidate.referenceKey) 80
            Add-Parameter $candidateCommand "@IdentidadObjetivo" NVarChar (
                $result.targetIdentityKey) 1000
            Add-Parameter $candidateCommand "@NombreAtlas" NVarChar (
                $result.name) 500
            Add-Parameter $candidateCommand "@TensionAtlasKv" Decimal (
                $result.voltageKv)
            Add-Parameter $candidateCommand "@RegionAtlas" NVarChar (
                $result.region) 150
            Add-Parameter $candidateCommand "@ZonaAtlas" NVarChar (
                $result.zone) 200
            Add-Parameter $candidateCommand "@NombreOsm" NVarChar (
                $candidate.name) 500
            Add-Parameter $candidateCommand "@TensionOsmKv" Decimal (
                $candidate.voltageKv)
            Add-Parameter $candidateCommand "@RegionOsm" NVarChar (
                $candidate.region) 200
            Add-Parameter $candidateCommand "@OperadorOsm" NVarChar (
                $candidate.operator) 200
            Add-Parameter $candidateCommand "@Latitud" Decimal (
                $candidate.latitude)
            Add-Parameter $candidateCommand "@Longitud" Decimal (
                $candidate.longitude)
            Add-Parameter $candidateCommand "@Puntaje" Decimal (
                $candidate.score)
            Add-Parameter $candidateCommand "@Margen" Decimal (
                $result.margin)
            Add-Parameter $candidateCommand "@EstadoPropuesta" NVarChar (
                $result.proposalState) 60
            Add-Parameter $candidateCommand "@EsPrincipal" Bit (
                $candidate.referenceKey -eq $principalReference)
            Add-Parameter $candidateCommand "@EvidenciasJson" NVarChar (
                ($candidate.evidence | ConvertTo-Json -Compress)) -1
            [void]$candidateCommand.ExecuteNonQuery()
            $inserted += 1
        }
    }

    $transaction.Commit()
    [pscustomobject]@{
        EjecucionId = $executionId
        RegistrosObjetivo = [int]$report.summary.targetRecords
        CandidatosInsertados = $inserted
        InventarioModificado = $false
    } | ConvertTo-Json -Compress
}
catch {
    $transaction.Rollback()
    throw
}
finally {
    $transaction.Dispose()
    $connection.Dispose()
}
