using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Data;
using System.Text.Json;

namespace NSIE.Servicios;

public interface IPamConvocatoriaValidationService
{
    Task<PamConvocatoriaValidationSnapshot> ObtenerActualesAsync(
        CancellationToken cancellationToken);

    Task<PamConvocatoriaValidationRecord> GuardarAsync(
        PamConvocatoriaInfrastructureCandidate candidato,
        string decision,
        string observacion,
        PamConvocatoriaValidationActor actor,
        CancellationToken cancellationToken);

    Task<PamConvocatoriaBulkValidationResult>
        ConfirmarCoincidenciasAutomaticasAsync(
            IReadOnlyCollection<PamConvocatoriaInfrastructureCandidate> candidatos,
            PamConvocatoriaValidationActor actor,
            CancellationToken cancellationToken);
}

public sealed class PamConvocatoriaValidationService :
    IPamConvocatoriaValidationService
{
    private const string TableName =
        "dgmesnie.PamConvocatoriaValidacionRed";
    private static readonly SemaphoreSlim StorageLock = new(1, 1);
    private static volatile bool _storageReady;

    private readonly string _connectionString;
    private readonly ILogger<PamConvocatoriaValidationService> _logger;

    public PamConvocatoriaValidationService(
        IConfiguration configuration,
        ILogger<PamConvocatoriaValidationService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection no está configurada.");
        _logger = logger;
    }

    public async Task<PamConvocatoriaValidationSnapshot> ObtenerActualesAsync(
        CancellationToken cancellationToken)
    {
        await EnsureStorageAsync(cancellationToken);

        const string sql = """
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
    FechaDecisionUtc,
    Vigente
FROM dgmesnie.PamConvocatoriaValidacionRed
WHERE Vigente = 1
ORDER BY TipoElemento, FechaDecisionUtc DESC, CandidatoId;
""";

        await using var connection = new SqlConnection(_connectionString);
        var rows = await connection.QueryAsync<ValidationRow>(
            new CommandDefinition(
                sql,
                cancellationToken: cancellationToken));
        var records = rows.Select(Map).ToList();

        return new PamConvocatoriaValidationSnapshot
        {
            Confirmadas = records.Count(record =>
                string.Equals(
                    record.Decision,
                    PamConvocatoriaValidationDecisions.Confirmada,
                    StringComparison.Ordinal)),
            Rechazadas = records.Count(record =>
                string.Equals(
                    record.Decision,
                    PamConvocatoriaValidationDecisions.Rechazada,
                    StringComparison.Ordinal)),
            FaltantesConfirmadas = records.Count(record =>
                string.Equals(
                    record.Decision,
                    PamConvocatoriaValidationDecisions.FaltanteConfirmada,
                    StringComparison.Ordinal)),
            PendientesRegistradas = records.Count(record =>
                string.Equals(
                    record.Decision,
                    PamConvocatoriaValidationDecisions.Pendiente,
                    StringComparison.Ordinal)),
            Validaciones = records
        };
    }

    public async Task<PamConvocatoriaValidationRecord> GuardarAsync(
        PamConvocatoriaInfrastructureCandidate candidato,
        string decision,
        string observacion,
        PamConvocatoriaValidationActor actor,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidato);
        ArgumentNullException.ThrowIfNull(actor);

        var normalizedDecision = decision.Trim().ToLowerInvariant();
        if (!PamConvocatoriaValidationDecisions.Permitidas.Contains(
                normalizedDecision))
        {
            throw new ArgumentOutOfRangeException(
                nameof(decision),
                "La decisión de validación no está permitida.");
        }
        if (string.Equals(
                normalizedDecision,
                PamConvocatoriaValidationDecisions.Confirmada,
                StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(candidato.ClaveCatalogo))
        {
            throw new InvalidOperationException(
                "No se puede confirmar una coincidencia sin elemento de catálogo.");
        }
        if (string.Equals(
                normalizedDecision,
                PamConvocatoriaValidationDecisions.FaltanteConfirmada,
                StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(candidato.ClaveCatalogo))
        {
            throw new InvalidOperationException(
                "El candidato tiene una coincidencia de catálogo; debe confirmarse o rechazarse.");
        }

        await EnsureStorageAsync(cancellationToken);

        const string retireSql = """
UPDATE dgmesnie.PamConvocatoriaValidacionRed
SET Vigente = 0
WHERE CandidatoId = @CandidatoId
  AND Vigente = 1;
""";
        const string insertSql = """
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
OUTPUT INSERTED.ValidacionId
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
""";

        var parameters = new
        {
            candidato.CandidatoId,
            candidato.TipoElemento,
            Decision = normalizedDecision,
            candidato.NombreDeclarado,
            candidato.ClaveCatalogo,
            candidato.CoincidenciaCatalogo,
            Puntaje = Math.Clamp(candidato.Puntaje, 0, 100),
            candidato.TensionKv,
            candidato.Gcr,
            candidato.Entidad,
            candidato.Municipio,
            Observacion = observacion.Trim(),
            candidato.Fuente,
            FoliosJson = JsonSerializer.Serialize(candidato.Folios),
            EvidenciasJson = JsonSerializer.Serialize(candidato.Evidencias),
            actor.UsuarioId,
            UsuarioNombre = actor.Nombre,
            UsuarioUnidad = actor.Unidad
        };

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    retireSql,
                    parameters,
                    transaction,
                    cancellationToken: cancellationToken));
            var validationId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    insertSql,
                    parameters,
                    transaction,
                    cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Validación Conv2 {Decision} registrada para {Candidate} por {UserId} {UserName}.",
                normalizedDecision,
                candidato.CandidatoId,
                actor.UsuarioId,
                actor.Nombre);

            return new PamConvocatoriaValidationRecord
            {
                ValidacionId = validationId,
                CandidatoId = candidato.CandidatoId,
                TipoElemento = candidato.TipoElemento,
                Decision = normalizedDecision,
                NombreDeclarado = candidato.NombreDeclarado,
                ClaveCatalogo = candidato.ClaveCatalogo,
                CoincidenciaCatalogo = candidato.CoincidenciaCatalogo,
                Puntaje = Math.Clamp(candidato.Puntaje, 0, 100),
                TensionKv = candidato.TensionKv,
                Gcr = candidato.Gcr,
                Entidad = candidato.Entidad,
                Municipio = candidato.Municipio,
                Observacion = observacion.Trim(),
                Fuente = candidato.Fuente,
                Folios = candidato.Folios,
                Evidencias = candidato.Evidencias,
                UsuarioId = actor.UsuarioId,
                UsuarioNombre = actor.Nombre,
                UsuarioUnidad = actor.Unidad,
                FechaDecisionUtc = DateTime.UtcNow,
                Vigente = true
            };
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<PamConvocatoriaBulkValidationResult>
        ConfirmarCoincidenciasAutomaticasAsync(
            IReadOnlyCollection<PamConvocatoriaInfrastructureCandidate> candidatos,
            PamConvocatoriaValidationActor actor,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidatos);
        ArgumentNullException.ThrowIfNull(actor);

        var substationCandidates = candidatos
            .Where(candidate => string.Equals(
                candidate.TipoElemento,
                "subestacion",
                StringComparison.Ordinal))
            .ToList();
        var eligible = substationCandidates
            .Where(candidate =>
                candidate.CoincidenciaAutomaticaFirme &&
                (!string.IsNullOrWhiteSpace(candidate.ClaveCatalogo) ||
                 candidate.CoincidenciaTopologicaFirme ||
                 candidate.CoincidenciaFuenteGeorreferenciadaFirme))
            .GroupBy(candidate => candidate.CandidatoId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(candidate => candidate.CandidatoId, StringComparer.Ordinal)
            .ToList();

        if (eligible.Count == 0)
        {
            return new PamConvocatoriaBulkValidationResult
            {
                CandidatosEvaluados = substationCandidates.Count
            };
        }

        await EnsureStorageAsync(cancellationToken);

        const string currentSql = """
SELECT
    CandidatoId,
    Decision,
    ISNULL(ClaveCatalogo, N'') AS ClaveCatalogo
FROM dgmesnie.PamConvocatoriaValidacionRed WITH (UPDLOCK, HOLDLOCK)
WHERE Vigente = 1
  AND CandidatoId IN @CandidateIds;
""";
        const string retireSql = """
UPDATE dgmesnie.PamConvocatoriaValidacionRed
SET Vigente = 0
WHERE CandidatoId = @CandidatoId
  AND Vigente = 1;
""";
        const string insertSql = """
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
OUTPUT INSERTED.ValidacionId
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
""";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var currentRows = (await connection.QueryAsync<CurrentValidationRow>(
                    new CommandDefinition(
                        currentSql,
                        new
                        {
                            CandidateIds = eligible
                                .Select(candidate => candidate.CandidatoId)
                                .ToArray()
                        },
                        transaction,
                        cancellationToken: cancellationToken)))
                .ToDictionary(
                    row => row.CandidatoId,
                    StringComparer.Ordinal);
            var inserted = new List<PamConvocatoriaValidationRecord>();
            var skipped = 0;

            foreach (var candidate in eligible)
            {
                if (currentRows.ContainsKey(candidate.CandidatoId))
                {
                    skipped++;
                    continue;
                }

                var decision = string.IsNullOrWhiteSpace(
                    candidate.ClaveCatalogo)
                        ? PamConvocatoriaValidationDecisions.FaltanteConfirmada
                        : PamConvocatoriaValidationDecisions.Confirmada;
                var observation = candidate.CoincidenciaTopologicaFirme
                    ? "Confirmación automática asistida por criterio compuesto v5: " +
                      "referencia ausente del catálogo puntual, pero confirmada como extremo nominal de línea mediante nombre, GCR, tensión y agrupación geográfica compatibles. " +
                      $"Líneas de soporte: {string.Join(" · ", candidate.LineasSoporte)}."
                    : candidate.CoincidenciaFuenteGeorreferenciadaFirme
                        ? "Confirmación automática asistida por criterio compuesto v5: " +
                          "referencia vigente ausente del catálogo, confirmada como subestación privada o propuesta porque la fuente aporta geometría explícita en los campos de subestación, tensión y GCR consistentes. " +
                          "No confirma conectividad eléctrica ni promueve el punto al catálogo oficial. " +
                          $"Soportes: {string.Join(" · ", candidate.SoportesFuente)}."
                    : "Confirmación automática asistida por criterio compuesto v5: " +
                      candidate.MotivoAutomatizacion + " " +
                      $"{candidate.DistanciasCompatibles} de " +
                      $"{candidate.DistanciasDeclaradas} distancia(s) declarada(s) compatibles; " +
                      $"{candidate.ResolucionesCatalogoDistintas} clave(s) de catálogo resuelta(s); " +
                      $"GCR de catálogo: {candidate.GcrCatalogo}.";
                var evidence = candidate.Evidencias
                    .Append(
                        "confirmación automática asistida autorizada por usuario institucional; criterio compuesto v5")
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var parameters = new
                {
                    candidate.CandidatoId,
                    candidate.TipoElemento,
                    Decision = decision,
                    candidate.NombreDeclarado,
                    candidate.ClaveCatalogo,
                    candidate.CoincidenciaCatalogo,
                    Puntaje = Math.Clamp(candidate.Puntaje, 0, 100),
                    candidate.TensionKv,
                    candidate.Gcr,
                    candidate.Entidad,
                    candidate.Municipio,
                    Observacion = observation,
                    candidate.Fuente,
                    FoliosJson = JsonSerializer.Serialize(candidate.Folios),
                    EvidenciasJson = JsonSerializer.Serialize(evidence),
                    actor.UsuarioId,
                    UsuarioNombre = actor.Nombre,
                    UsuarioUnidad = actor.Unidad
                };

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        retireSql,
                        parameters,
                        transaction,
                        cancellationToken: cancellationToken));
                var validationId =
                    await connection.ExecuteScalarAsync<long>(
                        new CommandDefinition(
                            insertSql,
                            parameters,
                            transaction,
                            cancellationToken: cancellationToken));
                inserted.Add(new PamConvocatoriaValidationRecord
                {
                    ValidacionId = validationId,
                    CandidatoId = candidate.CandidatoId,
                    TipoElemento = candidate.TipoElemento,
                    Decision = decision,
                    NombreDeclarado = candidate.NombreDeclarado,
                    ClaveCatalogo = candidate.ClaveCatalogo,
                    CoincidenciaCatalogo =
                        candidate.CoincidenciaCatalogo,
                    Puntaje = Math.Clamp(candidate.Puntaje, 0, 100),
                    TensionKv = candidate.TensionKv,
                    Gcr = candidate.Gcr,
                    Entidad = candidate.Entidad,
                    Municipio = candidate.Municipio,
                    Observacion = observation,
                    Fuente = candidate.Fuente,
                    Folios = candidate.Folios,
                    Evidencias = evidence,
                    UsuarioId = actor.UsuarioId,
                    UsuarioNombre = actor.Nombre,
                    UsuarioUnidad = actor.Unidad,
                    FechaDecisionUtc = DateTime.UtcNow,
                    Vigente = true
                });
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation(
                "Confirmación automática Conv2: {Inserted} nuevas, {Skipped} omitidas con decisión, {Eligible} firmes, por {UserId} {UserName}.",
                inserted.Count,
                skipped,
                eligible.Count,
                actor.UsuarioId,
                actor.Nombre);

            return new PamConvocatoriaBulkValidationResult
            {
                CandidatosEvaluados = substationCandidates.Count,
                CoincidenciasFirmes = eligible.Count,
                ConfirmadasNuevas = inserted.Count,
                OmitidasConDecision = skipped,
                Validaciones = inserted
            };
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task EnsureStorageAsync(CancellationToken cancellationToken)
    {
        if (_storageReady)
        {
            return;
        }

        await StorageLock.WaitAsync(cancellationToken);
        try
        {
            if (_storageReady)
            {
                return;
            }

            const string sql = """
IF SCHEMA_ID('dgmesnie') IS NULL
BEGIN
    EXEC('CREATE SCHEMA dgmesnie');
END;

IF OBJECT_ID('dgmesnie.PamConvocatoriaValidacionRed', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.PamConvocatoriaValidacionRed
    (
        ValidacionId BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PamConvocatoriaValidacionRed PRIMARY KEY,
        CandidatoId NVARCHAR(80) NOT NULL,
        TipoElemento NVARCHAR(40) NOT NULL,
        Decision NVARCHAR(40) NOT NULL,
        NombreDeclarado NVARCHAR(600) NOT NULL,
        ClaveCatalogo NVARCHAR(180) NULL,
        CoincidenciaCatalogo NVARCHAR(600) NULL,
        Puntaje INT NOT NULL,
        TensionKv DECIMAL(10,3) NULL,
        Gcr NVARCHAR(120) NULL,
        Entidad NVARCHAR(180) NULL,
        Municipio NVARCHAR(240) NULL,
        Observacion NVARCHAR(2000) NULL,
        Fuente NVARCHAR(1200) NOT NULL,
        FoliosJson NVARCHAR(MAX) NOT NULL,
        EvidenciasJson NVARCHAR(MAX) NOT NULL,
        UsuarioId INT NULL,
        UsuarioNombre NVARCHAR(260) NOT NULL,
        UsuarioUnidad NVARCHAR(300) NULL,
        FechaDecisionUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_PamConvocatoriaValidacionRed_Fecha DEFAULT SYSUTCDATETIME(),
        Vigente BIT NOT NULL
            CONSTRAINT DF_PamConvocatoriaValidacionRed_Vigente DEFAULT 1,
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Tipo
            CHECK (TipoElemento IN (N'subestacion', N'linea_transmision')),
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Decision
            CHECK (Decision IN (
                N'confirmada',
                N'rechazada',
                N'faltante_confirmada',
                N'pendiente'
            )),
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Puntaje
            CHECK (Puntaje BETWEEN 0 AND 100)
    );

    CREATE UNIQUE INDEX UX_PamConvocatoriaValidacionRed_Vigente
        ON dgmesnie.PamConvocatoriaValidacionRed(CandidatoId)
        WHERE Vigente = 1;

    CREATE INDEX IX_PamConvocatoriaValidacionRed_TipoDecision
        ON dgmesnie.PamConvocatoriaValidacionRed(
            TipoElemento,
            Decision,
            Vigente
        )
        INCLUDE (FechaDecisionUtc, UsuarioNombre);
END;
""";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken));
            _storageReady = true;
        }
        finally
        {
            StorageLock.Release();
        }
    }

    private static PamConvocatoriaValidationRecord Map(ValidationRow row)
    {
        return new PamConvocatoriaValidationRecord
        {
            ValidacionId = row.ValidacionId,
            CandidatoId = row.CandidatoId,
            TipoElemento = row.TipoElemento,
            Decision = row.Decision,
            NombreDeclarado = row.NombreDeclarado,
            ClaveCatalogo = row.ClaveCatalogo,
            CoincidenciaCatalogo = row.CoincidenciaCatalogo,
            Puntaje = row.Puntaje,
            TensionKv = row.TensionKv,
            Gcr = row.Gcr,
            Entidad = row.Entidad,
            Municipio = row.Municipio,
            Observacion = row.Observacion,
            Fuente = row.Fuente,
            Folios = DeserializeList(row.FoliosJson),
            Evidencias = DeserializeList(row.EvidenciasJson),
            UsuarioId = row.UsuarioId,
            UsuarioNombre = row.UsuarioNombre,
            UsuarioUnidad = row.UsuarioUnidad,
            FechaDecisionUtc = row.FechaDecisionUtc,
            Vigente = row.Vigente
        };
    }

    private static IReadOnlyList<string> DeserializeList(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(value) ??
                new List<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private sealed class ValidationRow
    {
        public long ValidacionId { get; init; }
        public string CandidatoId { get; init; } = string.Empty;
        public string TipoElemento { get; init; } = string.Empty;
        public string Decision { get; init; } = string.Empty;
        public string NombreDeclarado { get; init; } = string.Empty;
        public string ClaveCatalogo { get; init; } = string.Empty;
        public string CoincidenciaCatalogo { get; init; } = string.Empty;
        public int Puntaje { get; init; }
        public double? TensionKv { get; init; }
        public string Gcr { get; init; } = string.Empty;
        public string Entidad { get; init; } = string.Empty;
        public string Municipio { get; init; } = string.Empty;
        public string Observacion { get; init; } = string.Empty;
        public string Fuente { get; init; } = string.Empty;
        public string FoliosJson { get; init; } = "[]";
        public string EvidenciasJson { get; init; } = "[]";
        public int? UsuarioId { get; init; }
        public string UsuarioNombre { get; init; } = string.Empty;
        public string UsuarioUnidad { get; init; } = string.Empty;
        public DateTime FechaDecisionUtc { get; init; }
        public bool Vigente { get; init; }
    }

    private sealed class CurrentValidationRow
    {
        public string CandidatoId { get; init; } = string.Empty;
        public string Decision { get; init; } = string.Empty;
        public string ClaveCatalogo { get; init; } = string.Empty;
    }
}
