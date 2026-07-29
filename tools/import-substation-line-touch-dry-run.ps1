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
if ($report.mutatesGraph -ne $false) {
    throw "El archivo no corresponde a una auditoría sin mutación del grafo."
}
$promotionValidationState = if (
    [string]::IsNullOrWhiteSpace(
        [string]$report.promotion.validationState)
) {
    "asociacion_automatica_fuente_abierta"
}
else {
    [string]$report.promotion.validationState
}
$promotionRule = if (
    [string]::IsNullOrWhiteSpace([string]$report.promotion.rule)
) {
    "DGMESNIE-SUBSTATION-LINE-PROMOTION-v1"
}
else {
    [string]$report.promotion.rule
}
$promotionOfficial = if ($null -eq $report.promotion.official) {
    $false
}
else {
    [bool]$report.promotion.official
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
INSERT INTO dgmesnie.RedElectricaSubestacionLineaEjecucion
(
    GeneradoUtc,
    Regla,
    RutaReporte,
    SubestacionesObjetivo,
    ElementosLinea,
    SubestacionesTocadas,
    CoincidenciasFuertes,
    CoincidenciasGeometricas,
    LineasCercanas,
    RevisionEspacial,
    SinLineaCercana,
    GrafoModificado
)
OUTPUT INSERTED.EjecucionId
VALUES
(
    @GeneradoUtc,
    @Regla,
    @RutaReporte,
    @SubestacionesObjetivo,
    @ElementosLinea,
    @SubestacionesTocadas,
    @CoincidenciasFuertes,
    @CoincidenciasGeometricas,
    @LineasCercanas,
    @RevisionEspacial,
    @SinLineaCercana,
    0
);
"@
    Add-Parameter $executionCommand "@GeneradoUtc" DateTime2 $generatedUtc
    Add-Parameter $executionCommand "@Regla" NVarChar $report.rule 100
    Add-Parameter $executionCommand "@RutaReporte" NVarChar (
        $resolvedReport) 1000
    Add-Parameter $executionCommand "@SubestacionesObjetivo" Int (
        [int](
            $report.summary.targetSubstations ??
            $report.summary.promotedSubstations))
    Add-Parameter $executionCommand "@ElementosLinea" Int (
        [int]$report.summary.transmissionAssets230Or400Kv)
    Add-Parameter $executionCommand "@SubestacionesTocadas" Int (
        [int]$report.summary.stationsWithTouchingLine)
    Add-Parameter $executionCommand "@CoincidenciasFuertes" Int (
        [int]$report.summary.conexion_nombre_tension_geometria)
    Add-Parameter $executionCommand "@CoincidenciasGeometricas" Int (
        [int]$report.summary.linea_toca_geometricamente)
    Add-Parameter $executionCommand "@LineasCercanas" Int (
        [int]$report.summary.linea_cercana_misma_tension)
    Add-Parameter $executionCommand "@RevisionEspacial" Int (
        [int]$report.summary.revision_espacial)
    Add-Parameter $executionCommand "@SinLineaCercana" Int (
        [int]$report.summary.sin_linea_cercana)
    $executionId = [long]$executionCommand.ExecuteScalar()

    $resolveRecord = $connection.CreateCommand()
    $resolveRecord.Transaction = $transaction
    $resolveRecord.CommandText = @"
SELECT TOP (1) RegistroClave
FROM dgmesnie.RedElectricaSubestacionInventario
WHERE Activa = 1
  AND UniversoClave = @UniversoClave
  AND EstadoValidacion =
        N'validada_automatica_fuente_abierta'
ORDER BY RegistroClave;
"@

    $candidateCommand = $connection.CreateCommand()
    $candidateCommand.Transaction = $transaction
    $candidateCommand.CommandText = @"
INSERT INTO dgmesnie.RedElectricaSubestacionLineaCandidato
(
    EjecucionId,
    RegistroClave,
    AristaClave,
    ElementoCatalogoClave,
    NombreSubestacion,
    NombreLinea,
    TensionKv,
    DistanciaKm,
    CoincidenciaNombre,
    ReglaNombre,
    EstadoPropuesta,
    EsPromovible,
    EvidenciasJson
)
VALUES
(
    @EjecucionId,
    @RegistroClave,
    @AristaClave,
    @ElementoCatalogoClave,
    @NombreSubestacion,
    @NombreLinea,
    @TensionKv,
    @DistanciaKm,
    @CoincidenciaNombre,
    @ReglaNombre,
    @EstadoPropuesta,
    @EsPromovible,
    @EvidenciasJson
);
"@

    $inserted = 0
    foreach ($result in $report.results) {
        $recordKey = [string]$result.recordKey
        if ([string]::IsNullOrWhiteSpace($recordKey)) {
            $resolveRecord.Parameters.Clear()
            Add-Parameter $resolveRecord "@UniversoClave" NVarChar (
                $result.universeKey) 80
            $recordKey = $resolveRecord.ExecuteScalar()
        }
        if ($null -eq $recordKey -or $recordKey -is [DBNull]) {
            throw "No se encontró el registro de inventario $($result.universeKey)."
        }

        foreach ($candidate in $result.candidates) {
            $promotable =
                $result.proposalState -eq
                    "conexion_nombre_tension_geometria" -and
                $candidate.nameMatch -eq $true -and
                [decimal]$candidate.distanceKm -le
                    [decimal]$report.thresholdsKm.touch
            $evidence = [ordered]@{
                nameMatch = [bool]$candidate.nameMatch
                nameRule = [string]$candidate.nameRule
                sameVoltage = (
                    [decimal]$candidate.voltageKv -eq
                    [decimal]$result.voltageKv)
                touchThresholdKm =
                    [decimal]$report.thresholdsKm.touch
                sourceRule = [string]$report.rule
            } | ConvertTo-Json -Compress

            $candidateCommand.Parameters.Clear()
            Add-Parameter $candidateCommand "@EjecucionId" BigInt (
                $executionId)
            Add-Parameter $candidateCommand "@RegistroClave" NVarChar (
                [string]$recordKey) 80
            Add-Parameter $candidateCommand "@AristaClave" NVarChar (
                $candidate.edgeId) 80
            Add-Parameter $candidateCommand "@ElementoCatalogoClave" NVarChar (
                $candidate.catalogElementKey) 80
            Add-Parameter $candidateCommand "@NombreSubestacion" NVarChar (
                $result.name) 500
            Add-Parameter $candidateCommand "@NombreLinea" NVarChar (
                $candidate.name) 500
            Add-Parameter $candidateCommand "@TensionKv" Decimal (
                $candidate.voltageKv)
            Add-Parameter $candidateCommand "@DistanciaKm" Decimal (
                $candidate.distanceKm)
            Add-Parameter $candidateCommand "@CoincidenciaNombre" Bit (
                [bool]$candidate.nameMatch)
            Add-Parameter $candidateCommand "@ReglaNombre" NVarChar (
                $candidate.nameRule) 80
            Add-Parameter $candidateCommand "@EstadoPropuesta" NVarChar (
                $result.proposalState) 80
            Add-Parameter $candidateCommand "@EsPromovible" Bit $promotable
            Add-Parameter $candidateCommand "@EvidenciasJson" NVarChar (
                $evidence) -1
            [void]$candidateCommand.ExecuteNonQuery()
            $inserted += 1
        }
    }

    $promotionCommand = $connection.CreateCommand()
    $promotionCommand.Transaction = $transaction
    $promotionCommand.CommandText = @"
MERGE dgmesnie.RedElectricaSubestacionLineaPromocion AS destino
USING
(
    SELECT
        EjecucionId,
        RegistroClave,
        AristaClave,
        ElementoCatalogoClave,
        NombreLinea,
        TensionKv,
        DistanciaKm
    FROM dgmesnie.RedElectricaSubestacionLineaCandidato
    WHERE EjecucionId = @EjecucionId
      AND EsPromovible = 1
) AS fuente
  ON fuente.RegistroClave = destino.RegistroClave
 AND fuente.ElementoCatalogoClave =
        destino.ElementoCatalogoClave
WHEN MATCHED THEN UPDATE SET
    EjecucionId = fuente.EjecucionId,
    AristaClave = fuente.AristaClave,
    NombreLinea = fuente.NombreLinea,
    TensionKv = fuente.TensionKv,
    DistanciaKm = fuente.DistanciaKm,
    EstadoValidacion = @PromotionValidationState,
    ReglaPromocion = @PromotionRule,
    EsOficial = @PromotionOfficial,
    Activa = 1,
    PromovidaUtc = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT
    (
        RegistroClave,
        ElementoCatalogoClave,
        EjecucionId,
        AristaClave,
        NombreLinea,
        TensionKv,
        DistanciaKm,
        EstadoValidacion,
        ReglaPromocion,
        EsOficial,
        Activa,
        PromovidaUtc
    )
    VALUES
    (
        fuente.RegistroClave,
        fuente.ElementoCatalogoClave,
        fuente.EjecucionId,
        fuente.AristaClave,
        fuente.NombreLinea,
        fuente.TensionKv,
        fuente.DistanciaKm,
        @PromotionValidationState,
        @PromotionRule,
        @PromotionOfficial,
        1,
        SYSUTCDATETIME()
    );

SELECT
    COUNT(DISTINCT RegistroClave) AS Subestaciones,
    COUNT(*) AS Asociaciones
FROM dgmesnie.RedElectricaSubestacionLineaPromocion
WHERE Activa = 1
  AND EjecucionId = @EjecucionId;
"@
    Add-Parameter $promotionCommand "@EjecucionId" BigInt $executionId
    Add-Parameter $promotionCommand "@PromotionValidationState" NVarChar (
        $promotionValidationState) 80
    Add-Parameter $promotionCommand "@PromotionRule" NVarChar (
        $promotionRule) 100
    Add-Parameter $promotionCommand "@PromotionOfficial" Bit (
        $promotionOfficial)
    $reader = $promotionCommand.ExecuteReader()
    $promotedStations = 0
    $promotedAssociations = 0
    if ($reader.Read()) {
        $promotedStations = $reader.GetInt32(0)
        $promotedAssociations = $reader.GetInt32(1)
    }
    $reader.Dispose()

    $transaction.Commit()
    [pscustomobject]@{
        EjecucionId = $executionId
        CandidatosInsertados = $inserted
        SubestacionesPromovidas = $promotedStations
        AsociacionesPromovidas = $promotedAssociations
        GrafoCanonicoModificado = $false
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
