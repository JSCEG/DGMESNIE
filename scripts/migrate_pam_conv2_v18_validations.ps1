[CmdletBinding()]
param(
    [switch]$Execute,
    [string]$CoverageUri =
        'http://127.0.0.1:5099/DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Resumen',
    [string]$ConfigPath =
        (Join-Path $PSScriptRoot '..\appsettings.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-Condition {
    param(
        [Parameter(Mandatory)]
        [bool]$Condition,
        [Parameter(Mandatory)]
        [string]$Message
    )

    if (-not $Condition) {
        throw "Precondición incumplida: $Message"
    }
}

function Add-SqlParameter {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlCommand]$Command,
        [Parameter(Mandatory)]
        [string]$Name,
        [Parameter(Mandatory)]
        [System.Data.SqlDbType]$Type,
        [AllowNull()]
        [object]$Value,
        [int]$Size = 0
    )

    $parameter = if ($Size -gt 0) {
        $Command.Parameters.Add($Name, $Type, $Size)
    }
    else {
        $Command.Parameters.Add($Name, $Type)
    }
    $parameter.Value = if ($null -eq $Value) {
        [DBNull]::Value
    }
    else {
        $Value
    }
    return $parameter
}

function New-SqlCommand {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlTransaction]$Transaction,
        [Parameter(Mandatory)]
        [string]$Sql
    )

    $command = $Connection.CreateCommand()
    $command.Transaction = $Transaction
    $command.CommandText = $Sql
    $command.CommandTimeout = 60
    return $command
}

function Get-CurrentValidation {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlTransaction]$Transaction,
        [Parameter(Mandatory)]
        [string]$CandidateId
    )

    $sql = @'
SELECT
    ValidacionId,
    CandidatoId,
    TipoElemento,
    Decision,
    NombreDeclarado,
    ISNULL(ClaveCatalogo, N'') AS ClaveCatalogo,
    ISNULL(CoincidenciaCatalogo, N'') AS CoincidenciaCatalogo,
    Puntaje,
    TensionKv,
    ISNULL(Gcr, N'') AS Gcr,
    ISNULL(Entidad, N'') AS Entidad,
    ISNULL(Municipio, N'') AS Municipio,
    ISNULL(Observacion, N'') AS Observacion,
    Fuente,
    FoliosJson,
    EvidenciasJson,
    UsuarioId,
    UsuarioNombre,
    ISNULL(UsuarioUnidad, N'') AS UsuarioUnidad,
    FechaDecisionUtc
FROM dgmesnie.PamConvocatoriaValidacionRed WITH (UPDLOCK, HOLDLOCK)
WHERE CandidatoId = @CandidatoId
  AND Vigente = 1;
'@

    $command = New-SqlCommand $Connection $Transaction $sql
    [void](Add-SqlParameter $command '@CandidatoId' NVarChar $CandidateId 80)
    $reader = $command.ExecuteReader()
    try {
        if (-not $reader.Read()) {
            return $null
        }

        $row = [pscustomobject][ordered]@{
            ValidacionId = $reader.GetInt64(0)
            CandidatoId = $reader.GetString(1)
            TipoElemento = $reader.GetString(2)
            Decision = $reader.GetString(3)
            NombreDeclarado = $reader.GetString(4)
            ClaveCatalogo = $reader.GetString(5)
            CoincidenciaCatalogo = $reader.GetString(6)
            Puntaje = $reader.GetInt32(7)
            TensionKv = if ($reader.IsDBNull(8)) {
                $null
            }
            else {
                $reader.GetDecimal(8)
            }
            Gcr = $reader.GetString(9)
            Entidad = $reader.GetString(10)
            Municipio = $reader.GetString(11)
            Observacion = $reader.GetString(12)
            Fuente = $reader.GetString(13)
            FoliosJson = $reader.GetString(14)
            EvidenciasJson = $reader.GetString(15)
            UsuarioId = if ($reader.IsDBNull(16)) {
                $null
            }
            else {
                $reader.GetInt32(16)
            }
            UsuarioNombre = $reader.GetString(17)
            UsuarioUnidad = $reader.GetString(18)
            FechaDecisionUtc = $reader.GetDateTime(19)
        }

        Assert-Condition (-not $reader.Read()) (
            "hay más de una validación vigente para $CandidateId")
        return $row
    }
    finally {
        $reader.Dispose()
        $command.Dispose()
    }
}

function Get-DecisionCounts {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlTransaction]$Transaction
    )

    $sql = @'
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN Decision = N'confirmada' THEN 1 ELSE 0 END) AS Confirmadas,
    SUM(CASE WHEN Decision = N'rechazada' THEN 1 ELSE 0 END) AS Rechazadas,
    SUM(CASE WHEN Decision = N'faltante_confirmada' THEN 1 ELSE 0 END) AS Faltantes,
    SUM(CASE WHEN Decision = N'pendiente' THEN 1 ELSE 0 END) AS Pendientes
FROM dgmesnie.PamConvocatoriaValidacionRed WITH (UPDLOCK, HOLDLOCK)
WHERE Vigente = 1;
'@

    $command = New-SqlCommand $Connection $Transaction $sql
    $reader = $command.ExecuteReader()
    try {
        [void]$reader.Read()
        return [pscustomobject][ordered]@{
            Total = $reader.GetInt32(0)
            Confirmadas = $reader.GetInt32(1)
            Rechazadas = $reader.GetInt32(2)
            Faltantes = $reader.GetInt32(3)
            Pendientes = $reader.GetInt32(4)
        }
    }
    finally {
        $reader.Dispose()
        $command.Dispose()
    }
}

function Retire-CurrentValidation {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlTransaction]$Transaction,
        [Parameter(Mandatory)]
        [long]$ValidationId
    )

    $command = New-SqlCommand $Connection $Transaction @'
UPDATE dgmesnie.PamConvocatoriaValidacionRed
SET Vigente = 0
WHERE ValidacionId = @ValidacionId
  AND Vigente = 1;
'@
    [void](Add-SqlParameter $command '@ValidacionId' BigInt $ValidationId)
    try {
        Assert-Condition ($command.ExecuteNonQuery() -eq 1) (
            "no se pudo retirar la validación $ValidationId")
    }
    finally {
        $command.Dispose()
    }
}

function Insert-CandidateValidation {
    param(
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)]
        [System.Data.SqlClient.SqlTransaction]$Transaction,
        [Parameter(Mandatory)]
        [object]$Candidate,
        [Parameter(Mandatory)]
        [string]$Decision,
        [Parameter(Mandatory)]
        [string]$Observation,
        [AllowNull()]
        [int]$UserId,
        [Parameter(Mandatory)]
        [string]$UserName,
        [AllowEmptyString()]
        [string]$UserUnit
    )

    $sql = @'
INSERT INTO dgmesnie.PamConvocatoriaValidacionRed
(
    CandidatoId,
    TipoElemento,
    Decision,
    NombreDeclarado,
    ClaveCatalogo,
    CoincidenciaCatalogo,
    Puntaje,
    TensionKv,
    Gcr,
    Entidad,
    Municipio,
    Observacion,
    Fuente,
    FoliosJson,
    EvidenciasJson,
    UsuarioId,
    UsuarioNombre,
    UsuarioUnidad,
    FechaDecisionUtc,
    Vigente
)
VALUES
(
    @CandidatoId,
    @TipoElemento,
    @Decision,
    @NombreDeclarado,
    NULLIF(@ClaveCatalogo, N''),
    NULLIF(@CoincidenciaCatalogo, N''),
    @Puntaje,
    @TensionKv,
    NULLIF(@Gcr, N''),
    NULLIF(@Entidad, N''),
    NULLIF(@Municipio, N''),
    NULLIF(@Observacion, N''),
    @Fuente,
    @FoliosJson,
    @EvidenciasJson,
    @UsuarioId,
    @UsuarioNombre,
    NULLIF(@UsuarioUnidad, N''),
    SYSUTCDATETIME(),
    1
);
'@

    $command = New-SqlCommand $Connection $Transaction $sql
    $foliosJson = ConvertTo-Json -InputObject @($Candidate.folios) -Compress
    $evidencesJson =
        ConvertTo-Json -InputObject @($Candidate.evidencias) -Compress
    $catalogKey = [string]$Candidate.claveCatalogo
    $catalogName = [string]$Candidate.coincidenciaCatalogo
    $tension = if ($null -eq $Candidate.tensionKv) {
        $null
    }
    else {
        [decimal]$Candidate.tensionKv
    }

    [void](Add-SqlParameter $command '@CandidatoId' NVarChar (
        [string]$Candidate.candidatoId) 80)
    [void](Add-SqlParameter $command '@TipoElemento' NVarChar (
        [string]$Candidate.tipoElemento) 40)
    [void](Add-SqlParameter $command '@Decision' NVarChar $Decision 40)
    [void](Add-SqlParameter $command '@NombreDeclarado' NVarChar (
        [string]$Candidate.nombreDeclarado) 600)
    [void](Add-SqlParameter $command '@ClaveCatalogo' NVarChar $catalogKey 180)
    [void](Add-SqlParameter $command '@CoincidenciaCatalogo' NVarChar (
        $catalogName) 600)
    [void](Add-SqlParameter $command '@Puntaje' Int (
        [Math]::Clamp([int]$Candidate.puntaje, 0, 100)))
    [void](Add-SqlParameter $command '@TensionKv' Decimal $tension)
    $command.Parameters['@TensionKv'].Precision = 10
    $command.Parameters['@TensionKv'].Scale = 3
    [void](Add-SqlParameter $command '@Gcr' NVarChar (
        [string]$Candidate.gcr) 120)
    [void](Add-SqlParameter $command '@Entidad' NVarChar (
        [string]$Candidate.entidad) 180)
    [void](Add-SqlParameter $command '@Municipio' NVarChar (
        [string]$Candidate.municipio) 240)
    [void](Add-SqlParameter $command '@Observacion' NVarChar (
        $Observation.Substring(0, [Math]::Min($Observation.Length, 2000))) 2000)
    [void](Add-SqlParameter $command '@Fuente' NVarChar (
        [string]$Candidate.fuente) 1200)
    [void](Add-SqlParameter $command '@FoliosJson' NVarChar $foliosJson -1)
    [void](Add-SqlParameter $command '@EvidenciasJson' NVarChar (
        $evidencesJson) -1)
    [void](Add-SqlParameter $command '@UsuarioId' Int $UserId)
    [void](Add-SqlParameter $command '@UsuarioNombre' NVarChar $UserName 260)
    [void](Add-SqlParameter $command '@UsuarioUnidad' NVarChar $UserUnit 300)

    try {
        Assert-Condition ($command.ExecuteNonQuery() -eq 1) (
            "no se pudo insertar la validación de $($Candidate.candidatoId)")
    }
    finally {
        $command.Dispose()
    }
}

$migrations = @(
    [pscustomobject]@{
        OldId = 'C2-SE-66313ED90B5D33BB'
        NewId = 'C2-SE-AA166FFFB86FC369'
        Decision = 'confirmada'
        CatalogKey = 'SE-B246597CF7AD39D9'
    },
    [pscustomobject]@{
        OldId = 'C2-SE-21D04ABE947C7428'
        NewId = 'C2-SE-77BD83304EE2D9C6'
        Decision = 'confirmada'
        CatalogKey = 'SE-A25ADC71E44C4096'
    },
    [pscustomobject]@{
        OldId = 'C2-SE-AA9F181BC6D9F738'
        NewId = 'C2-SE-216123CA29CF6ED7'
        Decision = 'confirmada'
        CatalogKey = 'SE-B44DB1435936D974'
    },
    [pscustomobject]@{
        OldId = 'C2-SE-C66590D33498AC2C'
        NewId = 'C2-SE-3BBD113A9EFAA3BE'
        Decision = 'faltante_confirmada'
        CatalogKey = ''
    },
    [pscustomobject]@{
        OldId = 'C2-SE-52250B6BC1E44667'
        NewId = 'C2-SE-A319B79B5FB23645'
        Decision = 'confirmada'
        CatalogKey = 'SE-5997184292EBFC14'
    },
    [pscustomobject]@{
        OldId = 'C2-SE-7916F95191EAEF58'
        NewId = 'C2-SE-E3786F94C0C2619E'
        Decision = 'confirmada'
        CatalogKey = 'SE-1D36E3942D410BF2'
    }
)

$duplicateOldId = 'C2-SE-377929129AD0F0B2'
$duplicateDestinationId = 'C2-SE-E985BAFE0DE6237B'
$newDecisionSpecs = @(
    [pscustomobject]@{
        CandidateId = 'C2-SE-01085C0AC146043F'
        Decision = 'confirmada'
        CatalogKey = 'SE-6EDC39564DFA552F'
        Observation = 'Validación manual autorizada el 28/07/2026 con contraste CFE RGD 2026: ORIZABA 02-ORI-115-1/2 (115/13.8 kV) respalda el nodo SE ORIZABA; se confirma la homologación documental.'
    },
    [pscustomobject]@{
        CandidateId = 'C2-SE-6201B09FAD8914B7'
        Decision = 'confirmada'
        CatalogKey = 'SE-242C1430855DB7C1'
        Observation = 'Validación manual autorizada el 28/07/2026 con contraste CFE RGD 2026: SANTA FE VERACRUZ 02-SNF-115-1 (115/13.8 kV) respalda el nodo SE SANTA FE en la GCR Oriental; se confirma la homologación documental.'
    },
    [pscustomobject]@{
        CandidateId = 'C2-SE-C22A57913B629DC0'
        Decision = 'faltante_confirmada'
        CatalogKey = ''
        Observation = 'Corrección manual autorizada el 28/07/2026 con contraste CFE RGD 2026: SAN FELIPE DEL PROGRESO 01-FPG-115-1 (115/13.8 kV) confirma el nombre y la tensión en el territorio declarado y descarta la homologación previa con el homónimo de Baja California; se registra como faltante de catálogo.'
    }
)

$resolvedConfigPath = (Resolve-Path -LiteralPath $ConfigPath).Path
$configuration =
    Get-Content -LiteralPath $resolvedConfigPath -Raw | ConvertFrom-Json
$connectionString = [string]$configuration.ConnectionStrings.DefaultConnection
Assert-Condition (-not [string]::IsNullOrWhiteSpace($connectionString)) (
    'ConnectionStrings:DefaultConnection no está configurada')

$coverage = Invoke-RestMethod -Uri $CoverageUri
$candidateById = @{}
foreach ($candidate in @($coverage.candidatos)) {
    $candidateById[[string]$candidate.candidatoId] = $candidate
}

$requiredCandidateIds = @(
    $migrations.NewId
    $newDecisionSpecs.CandidateId
    $duplicateDestinationId
)
foreach ($candidateId in $requiredCandidateIds) {
    Assert-Condition $candidateById.ContainsKey($candidateId) (
        "el candidato vigente $candidateId no aparece en la cobertura v1.18")
}

foreach ($migration in $migrations) {
    $candidate = $candidateById[$migration.NewId]
    Assert-Condition (
        [string]$candidate.claveCatalogo -eq $migration.CatalogKey) (
        "la clave vigente de $($migration.NewId) cambió")
}

foreach ($spec in $newDecisionSpecs) {
    $candidate = $candidateById[$spec.CandidateId]
    Assert-Condition (
        [string]$candidate.claveCatalogo -eq $spec.CatalogKey) (
        "la clave vigente de $($spec.CandidateId) cambió")
    Assert-Condition (
        [string]$candidate.auditoriaCfeRgd.estado -eq 'respaldada') (
        "CFE RGD ya no respalda $($spec.CandidateId)")
}

$connection =
    [System.Data.SqlClient.SqlConnection]::new($connectionString)
$connection.Open()
$transaction = $connection.BeginTransaction(
    [System.Data.IsolationLevel]::Serializable)

try {
    $before = Get-DecisionCounts $connection $transaction

    $alreadyApplied = (
        $before.Total -eq 108 -and
        $before.Confirmadas -eq 79 -and
        $before.Rechazadas -eq 0 -and
        $before.Faltantes -eq 28 -and
        $before.Pendientes -eq 1
    )
    if ($alreadyApplied) {
        foreach ($migration in $migrations) {
            Assert-Condition (
                $null -eq (
                    Get-CurrentValidation (
                        $connection) $transaction $migration.OldId)) (
                "el origen ya migrado $($migration.OldId) reapareció")
            $current = Get-CurrentValidation (
                $connection) $transaction $migration.NewId
            Assert-Condition (
                $null -ne $current -and
                $current.Decision -eq $migration.Decision -and
                $current.ClaveCatalogo -eq $migration.CatalogKey) (
                "el destino aplicado $($migration.NewId) no coincide")
        }

        Assert-Condition (
            $null -eq (
                Get-CurrentValidation (
                    $connection) $transaction $duplicateOldId)) (
            "el duplicado territorial $duplicateOldId reapareció")
        $duplicateDestination = Get-CurrentValidation (
            $connection) $transaction $duplicateDestinationId
        Assert-Condition (
            $null -ne $duplicateDestination -and
            $duplicateDestination.Decision -eq 'confirmada' -and
            $duplicateDestination.ClaveCatalogo -eq 'SE-984A9F94A34504F0') (
            "la validación territorial correcta $duplicateDestinationId cambió")

        foreach ($spec in $newDecisionSpecs) {
            $current = Get-CurrentValidation (
                $connection) $transaction $spec.CandidateId
            Assert-Condition (
                $null -ne $current -and
                $current.Decision -eq $spec.Decision -and
                $current.ClaveCatalogo -eq $spec.CatalogKey) (
                "la decisión aplicada $($spec.CandidateId) no coincide")
        }

        $transaction.Rollback()
        [pscustomobject][ordered]@{
            Mode = 'already-applied'
            Current = $before
            Migrations = $migrations.Count
            RetiredDuplicate = $duplicateOldId
            ManualDecisions = $newDecisionSpecs.Count
            Committed = $false
        } | ConvertTo-Json -Depth 5
        return
    }

    Assert-Condition (
        $before.Total -eq 107 -and
        $before.Confirmadas -eq 79 -and
        $before.Rechazadas -eq 0 -and
        $before.Faltantes -eq 27 -and
        $before.Pendientes -eq 1) (
        'los totales vigentes cambiaron respecto de la línea base 107/79/0/27/1')

    $migrationRows = @()
    foreach ($migration in $migrations) {
        $oldRow = Get-CurrentValidation (
            $connection) $transaction $migration.OldId
        $newRow = Get-CurrentValidation (
            $connection) $transaction $migration.NewId
        Assert-Condition ($null -ne $oldRow) (
            "falta la validación origen $($migration.OldId)")
        Assert-Condition ($null -eq $newRow) (
            "ya existe una validación destino $($migration.NewId)")
        Assert-Condition ($oldRow.Decision -eq $migration.Decision) (
            "la decisión de $($migration.OldId) cambió")
        Assert-Condition ($oldRow.ClaveCatalogo -eq $migration.CatalogKey) (
            "la clave de $($migration.OldId) cambió")

        $migrationRows += [pscustomobject]@{
            Spec = $migration
            Old = $oldRow
        }
    }

    $duplicateOld =
        Get-CurrentValidation $connection $transaction $duplicateOldId
    $duplicateDestination =
        Get-CurrentValidation $connection $transaction $duplicateDestinationId
    Assert-Condition ($null -ne $duplicateOld) (
        "falta el duplicado territorial $duplicateOldId")
    Assert-Condition (
        $duplicateOld.Decision -eq 'confirmada' -and
        $duplicateOld.ClaveCatalogo -eq 'SE-242C1430855DB7C1') (
        "el duplicado territorial $duplicateOldId cambió")
    Assert-Condition (
        $null -ne $duplicateDestination -and
        $duplicateDestination.Decision -eq 'confirmada' -and
        $duplicateDestination.ClaveCatalogo -eq 'SE-984A9F94A34504F0') (
        "la validación territorial correcta $duplicateDestinationId cambió")

    $newDecisionRows = @()
    foreach ($spec in $newDecisionSpecs) {
        $row =
            Get-CurrentValidation $connection $transaction $spec.CandidateId
        if ($spec.CandidateId -eq 'C2-SE-C22A57913B629DC0') {
            Assert-Condition (
                $null -ne $row -and
                $row.Decision -eq 'confirmada' -and
                $row.ClaveCatalogo -eq 'SE-3CDB0C11B6891ED1') (
                'la validación anterior de San Felipe del Progreso cambió')
        }
        else {
            Assert-Condition ($null -eq $row) (
                "ya existe una validación para $($spec.CandidateId)")
        }

        $newDecisionRows += [pscustomobject]@{
            Spec = $spec
            Old = $row
        }
    }

    $actorCommand = New-SqlCommand $connection $transaction @'
SELECT TOP (1)
    UsuarioId,
    UsuarioNombre,
    ISNULL(UsuarioUnidad, N'') AS UsuarioUnidad
FROM dgmesnie.PamConvocatoriaValidacionRed WITH (UPDLOCK, HOLDLOCK)
WHERE Vigente = 1
  AND UsuarioNombre = N'Javier Sasso Celaya'
ORDER BY FechaDecisionUtc DESC, ValidacionId DESC;
'@
    $actorReader = $actorCommand.ExecuteReader()
    try {
        Assert-Condition $actorReader.Read() (
            'no se encontró el actor Javier Sasso Celaya')
        $actor = [pscustomobject]@{
            UserId = if ($actorReader.IsDBNull(0)) {
                $null
            }
            else {
                $actorReader.GetInt32(0)
            }
            UserName = $actorReader.GetString(1)
            UserUnit = $actorReader.GetString(2)
        }
    }
    finally {
        $actorReader.Dispose()
        $actorCommand.Dispose()
    }

    if (-not $Execute) {
        $transaction.Rollback()
        [pscustomobject][ordered]@{
            Mode = 'dry-run'
            Before = $before
            MigrationsReady = $migrationRows.Count
            DuplicateReady = $duplicateOldId
            ManualDecisionsReady = $newDecisionRows.Count
            ExpectedAfter = [pscustomobject]@{
                Total = 108
                Confirmadas = 79
                Rechazadas = 0
                Faltantes = 28
                Pendientes = 1
            }
            Committed = $false
        } | ConvertTo-Json -Depth 5
        return
    }

    foreach ($item in $migrationRows) {
        Retire-CurrentValidation (
            $connection) $transaction $item.Old.ValidacionId
        $migrationObservation = (
            $item.Old.Observacion.Trim() +
            ' | Migración territorial PAM-CONV2-v1.18 autorizada el 28/07/2026; se conserva la decisión y el actor originales.'
        ).Trim(' ', '|')
        Insert-CandidateValidation (
            $connection) $transaction $candidateById[$item.Spec.NewId] (
            $item.Spec.Decision) $migrationObservation (
            $item.Old.UsuarioId) $item.Old.UsuarioNombre (
            $item.Old.UsuarioUnidad)
    }

    Retire-CurrentValidation (
        $connection) $transaction $duplicateOld.ValidacionId

    foreach ($item in $newDecisionRows) {
        if ($null -ne $item.Old) {
            Retire-CurrentValidation (
                $connection) $transaction $item.Old.ValidacionId
        }
        Insert-CandidateValidation (
            $connection) $transaction (
            $candidateById[$item.Spec.CandidateId]) $item.Spec.Decision (
            $item.Spec.Observation) $actor.UserId $actor.UserName (
            $actor.UserUnit)
    }

    $after = Get-DecisionCounts $connection $transaction
    Assert-Condition (
        $after.Total -eq 108 -and
        $after.Confirmadas -eq 79 -and
        $after.Rechazadas -eq 0 -and
        $after.Faltantes -eq 28 -and
        $after.Pendientes -eq 1) (
        'los totales resultantes no coinciden con 108/79/0/28/1')

    foreach ($migration in $migrations) {
        Assert-Condition (
            $null -eq (
                Get-CurrentValidation (
                    $connection) $transaction $migration.OldId)) (
            "el origen $($migration.OldId) sigue vigente")
        Assert-Condition (
            $null -ne (
                Get-CurrentValidation (
                    $connection) $transaction $migration.NewId)) (
            "el destino $($migration.NewId) no quedó vigente")
    }
    Assert-Condition (
        $null -eq (
            Get-CurrentValidation (
                $connection) $transaction $duplicateOldId)) (
        "el duplicado $duplicateOldId sigue vigente")
    foreach ($spec in $newDecisionSpecs) {
        $current = Get-CurrentValidation (
            $connection) $transaction $spec.CandidateId
        Assert-Condition (
            $null -ne $current -and
            $current.Decision -eq $spec.Decision -and
            $current.ClaveCatalogo -eq $spec.CatalogKey) (
            "la decisión final de $($spec.CandidateId) no coincide")
    }

    $transaction.Commit()
    [pscustomobject][ordered]@{
        Mode = 'execute'
        Before = $before
        Migrated = @($migrations | ForEach-Object {
            "$($_.OldId) -> $($_.NewId)"
        })
        RetiredDuplicate = $duplicateOldId
        ManualDecisions = @($newDecisionSpecs | ForEach-Object {
            "$($_.CandidateId):$($_.Decision)"
        })
        After = $after
        Committed = $true
    } | ConvertTo-Json -Depth 5
}
catch {
    if ($null -ne $transaction.Connection) {
        $transaction.Rollback()
    }
    throw
}
finally {
    $transaction.Dispose()
    $connection.Dispose()
}
