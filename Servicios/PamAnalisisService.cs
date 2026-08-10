using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IPamAnalisisService
    {
        Task<long> SolicitarAnalisisAsync(long loteId, int usuarioId, string usuarioNombre, CancellationToken cancellationToken);
        Task<PamAnalisisClaim> TomarSiguienteAsync(CancellationToken cancellationToken);
        Task ProcesarAsync(long analisisId, Guid leaseUid, CancellationToken cancellationToken);
        Task ReencolarAsync(long analisisId, Guid leaseUid, CancellationToken cancellationToken);
        Task<PamLoteAnalisisViewModel> ObtenerDetalleAsync(long loteId, PamAnalisisFiltro filtro, CancellationToken cancellationToken);
        Task<PamAnalisisEstadoDto> ObtenerEstadoAsync(long loteId, CancellationToken cancellationToken);
        Task GuardarDecisionPreliminarAsync(
            long loteId,
            PamGuardarDecisionPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
        Task GuardarDecisionHallazgoAsync(
            long loteId,
            PamGuardarHallazgoPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
        Task GuardarDecisionesCampoAsync(
            long loteId,
            PamGuardarDecisionesCampoInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
        Task ReabrirDecisionPreliminarAsync(
            long loteId,
            PamReabrirDecisionPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
        Task<PamPreparacionAplicacionViewModel> ObtenerPreparacionAplicacionAsync(
            long loteId,
            CancellationToken cancellationToken);
        Task<PamPaqueteCierreResultado> CongelarPaqueteCierreAsync(
            long loteId,
            PamCongelarPaqueteCierreInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
        Task<PamAplicacionResultadoFinal> AplicarPaqueteAsync(
            long loteId,
            PamAplicarPaqueteInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken);
    }

    public sealed class PamAnalisisService : IPamAnalisisService
    {
        public const string VersionMotor = "1.1.0";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _connectionString;
        private readonly IPamFuenteExtractionService _extractor;
        private readonly ILogger<PamAnalisisService> _logger;

        public PamAnalisisService(
            IConfiguration configuration,
            IPamFuenteExtractionService extractor,
            ILogger<PamAnalisisService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _extractor = extractor;
            _logger = logger;
        }

        public async Task<long> SolicitarAnalisisAsync(
            long loteId,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var lote = await connection.QuerySingleOrDefaultAsync<LoteAnalisisContexto>(new CommandDefinition(@"
SELECT Estado, TipoActualizacion
FROM dgmesnie.PAMLoteActualizacion WITH (UPDLOCK, HOLDLOCK)
WHERE LoteId = @LoteId;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken));
                if (lote == null) throw new InvalidOperationException("El lote solicitado no existe.");
                if (lote.TipoActualizacion != PamTiposActualizacion.InformePormenorizado)
                    throw new InvalidOperationException("Los lotes de seguimiento se incorporan desde su vista previa y no se envían al comparador de la cartera maestra.");
                if (lote.Estado is "Aplicado" or "Rechazado")
                    throw new InvalidOperationException("El lote ya está cerrado. Registra una actualización nueva para conservar la trazabilidad.");

                var fuentes = (await connection.QueryAsync<FuenteHuella>(new CommandDefinition(@"
SELECT lf.LoteFuenteId, f.HashSha256
FROM dgmesnie.PAMLoteFuente lf
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
WHERE lf.LoteId = @LoteId
ORDER BY lf.LoteFuenteId;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken))).ToList();
                if (fuentes.Count == 0) throw new InvalidOperationException("El lote no contiene archivos para analizar.");

                var activeId = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(@"
SELECT TOP (1) AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion
WHERE LoteId = @LoteId AND Estado IN (N'Pendiente', N'Ejecutando')
ORDER BY AnalisisId DESC;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken));
                if (activeId.HasValue)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return activeId.Value;
                }

                var fingerprint = CrearHuellaEntrada(fuentes.Select(item => item.HashSha256));
                var completedId = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(@"
SELECT TOP (1) AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion
WHERE LoteId = @LoteId AND VersionMotor = @VersionMotor AND HuellaEntrada = @HuellaEntrada
  AND Estado IN (N'Completado', N'Completado con observaciones')
  AND FuentesError = 0
ORDER BY AnalisisId DESC;", new { LoteId = loteId, VersionMotor, HuellaEntrada = fingerprint }, transaction, cancellationToken: cancellationToken));
                if (completedId.HasValue)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return completedId.Value;
                }

                var attempt = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT ISNULL(MAX(NumeroIntento), 0) + 1
FROM dgmesnie.PAMAnalisisEjecucion
WHERE LoteId = @LoteId;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken));

                var analysisId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(@"
INSERT dgmesnie.PAMAnalisisEjecucion
    (LoteId, NumeroIntento, VersionMotor, HuellaEntrada, Estado, TotalFuentes, UsuarioId, UsuarioNombre)
VALUES
    (@LoteId, @NumeroIntento, @VersionMotor, @HuellaEntrada, N'Pendiente', @TotalFuentes, @UsuarioId, @UsuarioNombre);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", new
                {
                    LoteId = loteId,
                    NumeroIntento = attempt,
                    VersionMotor,
                    HuellaEntrada = fingerprint,
                    TotalFuentes = fuentes.Count,
                    UsuarioId = usuarioId,
                    UsuarioNombre = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}"
                }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(new CommandDefinition(@"
UPDATE dgmesnie.PAMLoteActualizacion
SET Estado = N'Extrayendo', FechaActualizacionUtc = SYSUTCDATETIME(),
    TotalRegistrosDetectados = 0, TotalCambiosPropuestos = 0, TotalObservados = 0
WHERE LoteId = @LoteId;

UPDATE dgmesnie.PAMLoteFuente
SET EstadoExtraccion = CASE WHEN Extension IN (N'.xls', N'.ppt') THEN N'Requiere conversión' ELSE N'Pendiente' END,
    MensajeExtraccion = CASE WHEN Extension IN (N'.xls', N'.ppt')
        THEN N'Original preservado; requiere conversión interna.'
        ELSE N'En espera del motor de análisis.' END
WHERE LoteId = @LoteId;", new { LoteId = loteId }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
                return analysisId;
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task<PamAnalisisClaim> TomarSiguienteAsync(CancellationToken cancellationToken)
        {
            var leaseUid = Guid.NewGuid();
            const string sql = @"
UPDATE dgmesnie.PAMAnalisisEjecucion
SET Estado = N'Pendiente', LeaseUid = NULL, FechaInicioUtc = NULL, FechaHeartbeatUtc = NULL,
    DisponibleDesdeUtc = SYSUTCDATETIME(), MensajeResultado = N'Reanudado después de perder el heartbeat.'
WHERE Estado = N'Ejecutando'
  AND FechaHeartbeatUtc < DATEADD(HOUR, -2, SYSUTCDATETIME())
  AND EsReintentable = 1;

;WITH Siguiente AS
(
    SELECT TOP (1) e.*
    FROM dgmesnie.PAMAnalisisEjecucion e WITH (UPDLOCK, READPAST, ROWLOCK)
    WHERE e.Estado = N'Pendiente'
      AND e.DisponibleDesdeUtc <= SYSUTCDATETIME()
      AND NOT EXISTS
      (
          SELECT 1 FROM dgmesnie.PAMAnalisisEjecucion activa
          WHERE activa.LoteId = e.LoteId AND activa.Estado = N'Ejecutando'
      )
    ORDER BY e.FechaSolicitudUtc, e.AnalisisId
)
UPDATE Siguiente
SET Estado = N'Ejecutando', LeaseUid = @LeaseUid,
    FechaInicioUtc = COALESCE(FechaInicioUtc, SYSUTCDATETIME()),
    FechaHeartbeatUtc = SYSUTCDATETIME(), MensajeResultado = N'Extrayendo y cotejando fuentes.'
OUTPUT inserted.AnalisisId, inserted.LeaseUid;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<PamAnalisisClaim>(new CommandDefinition(
                sql, new { LeaseUid = leaseUid }, cancellationToken: cancellationToken));
        }

        public async Task ProcesarAsync(long analisisId, Guid leaseUid, CancellationToken cancellationToken)
        {
            try
            {
                var context = await CargarContextoAsync(analisisId, leaseUid, cancellationToken);
                if (context == null) return;

                var projectsByKey = context.Proyectos
                    .Where(project => !string.IsNullOrWhiteSpace(project.ClaveProyecto))
                    .GroupBy(project => NormalizarClave(project.ClaveProyecto), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

                var processed = 0;
                var errors = 0;
                foreach (var source in context.Fuentes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await MarcarFuenteExtrayendoAsync(analisisId, leaseUid, source.LoteFuenteId, cancellationToken);

                    PamFuenteExtractionResult extraction;
                    string finalStatus;
                    string finalMessage;
                    try
                    {
                        extraction = await _extractor.ExtraerAsync(source, cancellationToken);
                        finalStatus = "Extraído";
                        finalMessage = extraction.Advertencias.Count == 0
                            ? $"{extraction.Registros.Count:N0} hallazgo(s) extraídos con {extraction.Extractor} {extraction.VersionExtractor}."
                            : Limitar(string.Join(" ", extraction.Advertencias), 1000);
                    }
                    catch (PamFuenteNoSoportadaException ex)
                    {
                        extraction = new PamFuenteExtractionResult { Extractor = "Conversión pendiente", VersionExtractor = VersionMotor };
                        extraction.Registros.Add(PamRegistroExtraido.Incidencia(source.NombreOriginal, ex.Message));
                        extraction.Advertencias.Add(ex.Message);
                        finalStatus = "Requiere conversión";
                        finalMessage = ex.Message;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error analizando la fuente {LoteFuenteId} del análisis {AnalisisId}.", source.LoteFuenteId, analisisId);
                        extraction = new PamFuenteExtractionResult { Extractor = "Error de extracción", VersionExtractor = VersionMotor };
                        extraction.Registros.Add(PamRegistroExtraido.Incidencia(source.NombreOriginal, ex.Message));
                        finalStatus = "Error";
                        finalMessage = Limitar(ex.Message, 1000);
                        errors++;
                    }

                    var findings = ConstruirHallazgos(source, extraction.Registros, projectsByKey);
                    processed++;
                    await PersistirFuenteAsync(
                        analisisId, leaseUid, source, findings, finalStatus, finalMessage,
                        processed, errors, cancellationToken);
                }

                await FinalizarAsync(analisisId, leaseUid, processed, errors, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "El análisis PAM {AnalisisId} terminó con error.", analisisId);
                await MarcarErrorAsync(analisisId, leaseUid, ex, cancellationToken);
            }
        }

        public async Task ReencolarAsync(long analisisId, Guid leaseUid, CancellationToken cancellationToken)
        {
            const string sql = @"
UPDATE dgmesnie.PAMAnalisisEjecucion
SET Estado = N'Pendiente', LeaseUid = NULL, FechaInicioUtc = NULL, FechaHeartbeatUtc = NULL,
    DisponibleDesdeUtc = SYSUTCDATETIME(), MensajeResultado = N'Análisis pausado por reinicio de la aplicación.'
WHERE AnalisisId = @AnalisisId AND LeaseUid = @LeaseUid AND Estado = N'Ejecutando';";
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(new CommandDefinition(sql, new { AnalisisId = analisisId, LeaseUid = leaseUid }, cancellationToken: cancellationToken));
        }

        public async Task<PamLoteAnalisisViewModel> ObtenerDetalleAsync(
            long loteId,
            PamAnalisisFiltro filtro,
            CancellationToken cancellationToken)
        {
            filtro ??= new PamAnalisisFiltro();
            filtro.Normalizar();
            const string sql = @"
SELECT LoteId, LoteUid, Nombre, FechaCorte, Notas, Estado, TotalArchivos, FechaRegistroUtc, UsuarioNombre
FROM dgmesnie.PAMLoteActualizacion
WHERE LoteId = @LoteId;

SELECT lf.LoteFuenteId, lf.NombreOriginal, lf.Extension, lf.TamanoBytes,
       lf.EstadoExtraccion, lf.MensajeExtraccion, lf.FirmaValidada
FROM dgmesnie.PAMLoteFuente lf
WHERE lf.LoteId = @LoteId
ORDER BY lf.LoteFuenteId;

SELECT TOP (1)
    AnalisisId, Estado, FechaSolicitudUtc, FechaInicioUtc, FechaFinUtc,
    TotalFuentes, FuentesProcesadas,
    CAST(CASE WHEN TotalFuentes = 0 THEN 0 ELSE FuentesProcesadas * 100.0 / TotalFuentes END AS DECIMAL(6,2)) AS ProgresoPorcentaje,
    MensajeResultado AS Mensaje
FROM dgmesnie.PAMAnalisisEjecucion
WHERE LoteId = @LoteId
ORDER BY AnalisisId DESC;";

            await using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { LoteId = loteId }, cancellationToken: cancellationToken));
            var lot = await multi.ReadSingleOrDefaultAsync<PamLoteAnalisisCabecera>();
            if (lot == null) return null;
            var sources = (await multi.ReadAsync<PamAnalisisFuenteItem>()).ToList();
            var execution = await multi.ReadSingleOrDefaultAsync<PamAnalisisEjecucionItem>();

            var model = new PamLoteAnalisisViewModel
            {
                Lote = lot,
                Fuentes = sources,
                EjecucionActual = execution,
                Filtro = filtro
            };
            if (execution == null) return model;

            var offset = (filtro.Pagina - 1) * filtro.TamanoPagina;
            const string resultsSql = @"
SET NOCOUNT ON;

SELECT
    e.RegistrosDetectados,
    e.ProyectosCoincidentes AS CoincidenciasExactas,
    e.CambiosPropuestos,
    (SELECT COUNT(*) FROM dgmesnie.PAMCambioPropuesto cp WHERE cp.AnalisisId = e.AnalisisId AND cp.TipoCambio = N'Alta') AS AltasPropuestas,
    (SELECT COUNT(*)
     FROM dgmesnie.PAMCambioPropuesto cp
     INNER JOIN dgmesnie.PAMRevisionPendiente r
         ON r.AnalisisId = cp.AnalisisId AND r.CambioPropuestoId = cp.CambioPropuestoId
     WHERE cp.AnalisisId = e.AnalisisId AND cp.TipoCambio = N'Alta'
       AND r.Estado = N'Pendiente') AS AltasPendientes,
    (SELECT COUNT(*)
     FROM dgmesnie.PAMCambioPropuesto cp
     INNER JOIN dgmesnie.PAMRevisionPendiente r
         ON r.AnalisisId = cp.AnalisisId AND r.CambioPropuestoId = cp.CambioPropuestoId
     WHERE cp.AnalisisId = e.AnalisisId AND cp.TipoCambio = N'Alta'
       AND r.Estado = N'Revisada') AS AltasRevisadas,
    (SELECT COUNT(DISTINCT cp.ProyectoVersionBaseId)
     FROM dgmesnie.PAMCambioPropuesto cp
     WHERE cp.AnalisisId = e.AnalisisId
       AND cp.TipoCambio <> N'Alta'
       AND cp.ProyectoVersionBaseId IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM dgmesnie.PAMCambioDecisionHistorial decision
           WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
       )) AS ProyectosCamposPendientes,
    (SELECT COUNT(DISTINCT cp.ProyectoVersionBaseId)
     FROM dgmesnie.PAMCambioPropuesto cp
     WHERE cp.AnalisisId = e.AnalisisId
       AND cp.TipoCambio <> N'Alta'
       AND cp.ProyectoVersionBaseId IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM dgmesnie.PAMCambioPropuesto pendiente
           WHERE pendiente.AnalisisId = cp.AnalisisId
             AND pendiente.TipoCambio <> N'Alta'
             AND pendiente.ProyectoVersionBaseId = cp.ProyectoVersionBaseId
             AND NOT EXISTS
             (
                 SELECT 1
                 FROM dgmesnie.PAMCambioDecisionHistorial decisionPendiente
                 WHERE decisionPendiente.CambioPropuestoId = pendiente.CambioPropuestoId
             )
       )) AS ProyectosCamposRevisados,
    (SELECT COUNT(*)
     FROM dgmesnie.PAMCambioPropuesto cp
     WHERE cp.AnalisisId = e.AnalisisId
       AND cp.TipoCambio <> N'Alta'
       AND NOT EXISTS
       (
           SELECT 1 FROM dgmesnie.PAMCambioDecisionHistorial decision
           WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
       )) AS CamposPendientes,
    (SELECT COUNT(*)
     FROM dgmesnie.PAMCambioPropuesto cp
     WHERE cp.AnalisisId = e.AnalisisId
       AND cp.TipoCambio <> N'Alta'
       AND EXISTS
       (
           SELECT 1 FROM dgmesnie.PAMCambioDecisionHistorial decision
           WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
       )) AS CamposRevisados,
    e.TotalObservados AS Observados,
    e.ProyectosNoEncontrados AS ClavesNoEncontradas,
    (SELECT COUNT(*)
     FROM dgmesnie.PAMProyectoRelacionVersion relacion
     WHERE relacion.EsRelacionVigente = 1
       AND relacion.EstadoValidacion = N'Validada') AS RelacionesJerarquiaValidadas
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.AnalisisId = @AnalisisId;

SELECT cp.CambioPropuestoId, cp.ProyectoId, cp.ProyectoVersionBaseId,
       CASE
           -- Cada alta recibida es una revisión independiente. Dos fases pueden
           -- compartir clave/nombre, pero no deben ocultarse entre sí al filtrar
           -- Pendientes o Revisadas.
           WHEN cp.TipoCambio = N'Alta'
               THEN CONCAT(N'A:', CONVERT(NVARCHAR(30), cp.CambioPropuestoId))
           WHEN cp.ProyectoVersionBaseId IS NOT NULL
               THEN CONCAT(N'V:', CONVERT(NVARCHAR(30), cp.ProyectoVersionBaseId))
           ELSE CONCAT(
               N'K:',
               COALESCE(
                   NULLIF(UPPER(LTRIM(RTRIM(h.ClaveNormalizada))), N''),
                   NULLIF(UPPER(LTRIM(RTRIM(h.ClaveDetectada))), N''),
                   CONCAT(N'H:', CONVERT(NVARCHAR(30), h.HallazgoId))))
       END AS GrupoProyecto,
       COALESCE(v.ClaveProyecto, h.ClaveDetectada) AS ClaveProyecto,
       COALESCE(v.NombreProyecto, h.NombreDetectado) AS NombreProyecto,
       cp.TipoCambio, cp.Campo, cp.ValorActualJson, cp.ValorPropuestoJson,
       cp.Confianza, f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente, cp.EstadoRevision AS Estado,
       r.RevisionId,
       r.ClasificacionPreliminar,
       r.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId,
       rv.ClaveProyecto AS ProyectoRelacionadoClave,
       rv.NombreProyecto AS ProyectoRelacionadoNombre,
       r.NotaPreliminar,
       r.UsuarioDecisionPreliminarId,
       r.UsuarioDecisionPreliminar,
       r.FechaDecisionPreliminarUtc,
       sys.fn_varbintohexstr(r.VersionDecision) AS VersionDecision,
       decisionCampo.CambioDecisionId,
       decisionCampo.DecisionCampo,
       decisionCampo.NotaProyecto AS NotaDecisionCampo,
       decisionCampo.UsuarioDecisionId AS UsuarioDecisionCampoId,
       decisionCampo.UsuarioDecision AS UsuarioDecisionCampo,
       decisionCampo.FechaDecisionUtc AS FechaDecisionCampoUtc,
       CASE
           WHEN cp.TipoCambio = N'Alta' THEN COALESCE(r.Estado, N'Pendiente')
           WHEN decisionCampo.CambioDecisionId IS NULL THEN N'Pendiente'
           ELSE N'Revisada'
       END AS EstadoDecision,
       CASE
           WHEN cp.TipoCambio IN (N'Alta', N'Baja', N'Reactivación')
                OR cp.Campo = N'EstadoVigenciaCartera'
               THEN N'Prioritario'
           WHEN cp.Campo = N'MontoProyectoMdp'
                AND TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(cp.ValorActualJson, '$.value')) IS NOT NULL
                AND TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(cp.ValorPropuestoJson, '$.value')) IS NOT NULL
                AND ABS(
                    TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(cp.ValorPropuestoJson, '$.value'))
                    - TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(cp.ValorActualJson, '$.value'))
                ) < 0.01
               THEN N'Equivalente'
           WHEN cp.Campo = N'EtapaProyecto'
                AND JSON_VALUE(cp.ValorActualJson, '$.value') IS NOT NULL
                AND
                (
                    JSON_VALUE(cp.ValorPropuestoJson, '$.value') LIKE
                        N'%' + JSON_VALUE(cp.ValorActualJson, '$.value') + N'%'
                    OR
                    (
                        JSON_VALUE(cp.ValorActualJson, '$.value') LIKE N'%acuerdo%Cancelaci%'
                        AND JSON_VALUE(cp.ValorPropuestoJson, '$.value') LIKE N'%solicitud%Cancelaci%'
                    )
                )
               THEN N'Equivalente'
           WHEN cp.Campo IN (N'FechaNecesaria', N'FeoFactible')
                AND NULLIF(LTRIM(RTRIM(JSON_VALUE(cp.ValorActualJson, '$.value'))), N'') IS NULL
               THEN N'Completar'
           WHEN cp.Campo IN (N'EtapaProyecto', N'FechaNecesaria', N'FeoFactible', N'MontoProyectoMdp', N'AnioInstruccion')
               THEN N'Prioritario'
           WHEN cp.Campo IN (N'NombreProyecto', N'GRT', N'EstadoRealProyecto')
               THEN N'Informativo'
           ELSE N'Otros'
       END AS Categoria
INTO #CambiosBase
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoVersionId = cp.ProyectoVersionBaseId
LEFT JOIN dgmesnie.PAMRevisionPendiente r
    ON r.AnalisisId = cp.AnalisisId AND r.CambioPropuestoId = cp.CambioPropuestoId
LEFT JOIN dgmesnie.PAMProyecto rp
    ON rp.ProyectoId = r.ProyectoRelacionadoPreliminarId
LEFT JOIN dgmesnie.PAMProyectoVersion rv
    ON rv.ProyectoId = rp.ProyectoId AND rv.EsVersionVigente = 1
OUTER APPLY
(
    SELECT TOP (1)
           decision.CambioDecisionId, decision.DecisionCampo, decision.NotaProyecto,
           decision.UsuarioDecisionId, decision.UsuarioDecision, decision.FechaDecisionUtc
    FROM dgmesnie.PAMCambioDecisionHistorial decision
    WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY decision.CambioDecisionId DESC
) decisionCampo
WHERE cp.AnalisisId = @AnalisisId;

SELECT conteo.EstadoRevision, conteo.Filtro, COUNT(DISTINCT conteo.GrupoProyecto) AS TotalProyectos
FROM
(
    SELECT EstadoDecision AS EstadoRevision, N'Todos' AS Filtro, GrupoProyecto
    FROM #CambiosBase

    UNION ALL

    SELECT EstadoDecision AS EstadoRevision, Categoria AS Filtro, GrupoProyecto
    FROM #CambiosBase

    UNION ALL

    SELECT EstadoDecision AS EstadoRevision, N'Alta' AS Filtro, GrupoProyecto
    FROM #CambiosBase
    WHERE TipoCambio = N'Alta'
) conteo
GROUP BY conteo.EstadoRevision, conteo.Filtro
ORDER BY conteo.EstadoRevision, conteo.Filtro;

SELECT DISTINCT GrupoProyecto
INTO #ProyectosFiltrados
FROM #CambiosBase
WHERE (@Categoria IS NULL OR Categoria = @Categoria)
  AND (@Tipo IS NULL OR TipoCambio = @Tipo)
  AND (@EstadoRevision = N'Todas' OR EstadoDecision = @EstadoRevision)
  AND
  (
      @PatronBusqueda IS NULL
      OR ClaveProyecto LIKE @PatronBusqueda
      OR NombreProyecto LIKE @PatronBusqueda
  );

SELECT
    base.GrupoProyecto,
    ROW_NUMBER() OVER
    (
        ORDER BY
            MAX(CASE WHEN base.Categoria = N'Prioritario' THEN 1 ELSE 0 END) DESC,
            MAX(CASE WHEN base.TipoCambio = N'Alta' THEN 1 ELSE 0 END) DESC,
            CASE
                WHEN MAX(CASE WHEN base.TipoCambio = N'Alta' THEN 1 ELSE 0 END) = 1
                    THEN MIN(COALESCE(base.NombreProyecto, N''))
                ELSE N''
            END,
            MIN(CASE base.Estado WHEN N'Requiere revisión' THEN 0 WHEN N'Conflicto' THEN 1 ELSE 2 END),
            MIN(COALESCE(base.ClaveProyecto, N'')),
            base.GrupoProyecto
    ) AS OrdenPagina,
    COUNT(*) AS TotalCambiosProyecto
INTO #ProyectosOrdenados
FROM #CambiosBase base
INNER JOIN #ProyectosFiltrados filtrado ON filtrado.GrupoProyecto = base.GrupoProyecto
GROUP BY base.GrupoProyecto;

SELECT
    COUNT(*) AS TotalProyectos,
    COALESCE(SUM(TotalCambiosProyecto), 0) AS TotalCambios
FROM #ProyectosOrdenados;

SELECT base.CambioPropuestoId, base.ProyectoId, base.ProyectoVersionBaseId,
       base.GrupoProyecto, base.ClaveProyecto, base.NombreProyecto,
       base.TipoCambio, base.Campo, base.Categoria,
       base.ValorActualJson, base.ValorPropuestoJson, base.Confianza,
       base.FuenteDocumento, base.UbicacionFuente, base.Estado,
       base.RevisionId, base.ClasificacionPreliminar,
       base.ProyectoRelacionadoId, base.ProyectoRelacionadoClave,
       base.ProyectoRelacionadoNombre, base.NotaPreliminar,
       base.UsuarioDecisionPreliminarId, base.UsuarioDecisionPreliminar,
       base.FechaDecisionPreliminarUtc, base.VersionDecision,
       base.CambioDecisionId, base.DecisionCampo, base.NotaDecisionCampo,
       base.UsuarioDecisionCampoId, base.UsuarioDecisionCampo, base.FechaDecisionCampoUtc,
       base.EstadoDecision
FROM #CambiosBase base
INNER JOIN #ProyectosOrdenados proyecto ON proyecto.GrupoProyecto = base.GrupoProyecto
WHERE proyecto.OrdenPagina > @Offset
  AND proyecto.OrdenPagina <= @Offset + @PageSize
ORDER BY proyecto.OrdenPagina,
         CASE base.Categoria WHEN N'Prioritario' THEN 0 WHEN N'Informativo' THEN 1 ELSE 2 END,
         CASE base.Estado WHEN N'Requiere revisión' THEN 0 WHEN N'Conflicto' THEN 1 ELSE 2 END,
         base.TipoCambio, base.Campo, base.CambioPropuestoId;

SELECT TOP (100)
       h.HallazgoId, alta.CambioPropuestoId, revision.RevisionId,
       h.ClaveDetectada AS ClaveRecibida, h.NombreDetectado AS NombreRecibido,
       f.NombreDocumento AS FuenteDocumento, h.HojaPaginaSeccion AS UbicacionFuente,
       CASE h.ResultadoCotejo
           WHEN N'No encontrada' THEN N'La clave no existe en la cartera vigente.'
           WHEN N'Ambigua' THEN N'La clave coincide con más de un proyecto.'
           WHEN N'Inválida' THEN N'La clave detectada no tiene un formato reconocible.'
           ELSE COALESCE(h.FragmentoEvidencia, N'Requiere revisión manual.')
       END AS Motivo,
       h.Confianza,
       CASE
           WHEN h.DatosExtraidosJson LIKE N'%Cancel%' THEN N'Cancelación reportada'
           WHEN alta.CambioPropuestoId IS NOT NULL THEN N'Alta potencial'
           ELSE N'Sin coincidencia'
       END AS Clasificacion,
       COALESCE(revision.Estado, N'Pendiente') AS EstadoRevision,
       revision.ClasificacionPreliminar,
       revision.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId,
       relacionada.ClaveProyecto AS ProyectoRelacionadoClave,
       relacionada.NombreProyecto AS ProyectoRelacionadoNombre,
       revision.NotaPreliminar,
       sys.fn_varbintohexstr(revision.VersionDecision) AS VersionDecision
FROM dgmesnie.PAMHallazgoProyecto h
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
OUTER APPLY
(
    SELECT TOP (1) cp.CambioPropuestoId
    FROM dgmesnie.PAMCambioPropuesto cp
    WHERE cp.HallazgoId = h.HallazgoId AND cp.TipoCambio = N'Alta'
    ORDER BY cp.CambioPropuestoId DESC
) alta
OUTER APPLY
(
    SELECT TOP (1)
           r.RevisionId, r.Estado, r.ClasificacionPreliminar,
           r.ProyectoRelacionadoPreliminarId, r.NotaPreliminar, r.VersionDecision
    FROM dgmesnie.PAMRevisionPendiente r
    WHERE r.AnalisisId = h.AnalisisId
      AND r.HallazgoId = h.HallazgoId
    ORDER BY r.RevisionId DESC
) revision
LEFT JOIN dgmesnie.PAMProyectoVersion relacionada
    ON relacionada.ProyectoId = revision.ProyectoRelacionadoPreliminarId
   AND relacionada.EsVersionVigente = 1
WHERE h.AnalisisId = @AnalisisId
  AND h.ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave')
ORDER BY h.HallazgoId;

SELECT p.ProyectoId, v.ProyectoVersionId, v.ClaveProyecto, v.NombreProyecto,
       v.GRT, v.EtapaProyecto
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
WHERE p.Activo = 1 AND v.EstadoVigenciaCartera = N'Vigente';

SELECT
    COALESCE(
        NULLIF(LTRIM(RTRIM(h.ClaveNormalizada)), N''),
        NULLIF(LTRIM(RTRIM(h.ClaveDetectada)), N'')
    ) AS ClaveProyecto,
    h.NombreDetectado AS NombreProyecto
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio = N'Alta'
  AND NULLIF(LTRIM(RTRIM(h.NombreDetectado)), N'') IS NOT NULL;";

            using var results = await connection.QueryMultipleAsync(new CommandDefinition(resultsSql, new
            {
                AnalisisId = execution.AnalisisId,
                Tipo = NormalizarTipo(filtro.Tipo),
                Categoria = filtro.Categoria == "Todos" ? null : filtro.Categoria,
                EstadoRevision = filtro.EstadoRevision,
                PatronBusqueda = CrearPatronBusqueda(filtro.Busqueda),
                Offset = offset,
                PageSize = filtro.TamanoPagina
            }, cancellationToken: cancellationToken));
            model.Resumen = await results.ReadSingleAsync<PamAnalisisResumen>();
            model.ConteosFiltros = (await results.ReadAsync<PamFiltroRevisionConteo>()).ToList();
            var conteo = await results.ReadSingleAsync<ProyectoResultadoConteo>();
            model.TotalProyectos = conteo.TotalProyectos;
            model.TotalCambios = conteo.TotalCambios;
            var rawChanges = (await results.ReadAsync<CambioPropuestoRow>()).ToList();
            model.NoEncontrados = (await results.ReadAsync<PamHallazgoVista>()).ToList();
            var candidatePool = (await results.ReadAsync<ProyectoCandidatoRow>()).ToList();
            model.ProyectosVigentes = candidatePool
                .OrderBy(row => row.ClaveProyecto, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.NombreProyecto, StringComparer.OrdinalIgnoreCase)
                .Select(row => new PamProyectoOpcionVista
                {
                    ProyectoId = row.ProyectoId,
                    ClaveProyecto = row.ClaveProyecto,
                    NombreProyecto = row.NombreProyecto
                })
                .ToList();
            var familyRows = (await results.ReadAsync<AltaFamiliaRow>()).ToList();
            var familiasAltas = familyRows
                .Where(row => !string.IsNullOrWhiteSpace(row.NombreProyecto))
                .GroupBy(row => NormalizarComparacion(row.NombreProyecto), StringComparer.Ordinal)
                .Where(grupo => !string.IsNullOrWhiteSpace(grupo.Key))
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo =>
                    {
                        var claves = grupo
                            .Where(row => !string.IsNullOrWhiteSpace(row.ClaveProyecto))
                            .GroupBy(row => NormalizarClave(row.ClaveProyecto), StringComparer.OrdinalIgnoreCase)
                            .Where(clavesGrupo => !string.IsNullOrWhiteSpace(clavesGrupo.Key))
                            .Select(clavesGrupo => clavesGrupo.First().ClaveProyecto.Trim())
                            .OrderBy(clave => clave, StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        return new AltaFamiliaInfo
                        {
                            ClaveNormalizada = grupo.Key,
                            Claves = claves
                        };
                    },
                    StringComparer.Ordinal);
            model.Proyectos = rawChanges
                .GroupBy(cambio => cambio.GrupoProyecto, StringComparer.OrdinalIgnoreCase)
                .Select(grupo =>
                {
                    var primero = grupo.First();
                    var alta = grupo.FirstOrDefault(cambio => cambio.TipoCambio == "Alta");
                    var altaDatos = MapearAltaDatos(alta);
                    if (altaDatos != null)
                    {
                        var claveFamilia = NormalizarComparacion(altaDatos.NombreProyecto);
                        if (!string.IsNullOrWhiteSpace(claveFamilia)
                            && familiasAltas.TryGetValue(claveFamilia, out var familia))
                        {
                            altaDatos.ClaveFamiliaProbable = familia.ClaveNormalizada;
                            altaDatos.ClavesMismoNombre = familia.Claves;
                            altaDatos.TotalClavesMismoNombre = Math.Max(1, familia.Claves.Count);
                        }
                        else
                        {
                            altaDatos.ClaveFamiliaProbable = claveFamilia;
                            altaDatos.ClavesMismoNombre = string.IsNullOrWhiteSpace(altaDatos.ClaveProyecto)
                                ? new List<string>()
                                : new List<string> { altaDatos.ClaveProyecto };
                        }
                    }
                    return new PamProyectoCambiosVista
                    {
                        GrupoProyecto = primero.GrupoProyecto,
                        ProyectoId = primero.ProyectoId,
                        ProyectoVersionBaseId = primero.ProyectoVersionBaseId,
                        ClaveProyecto = primero.ClaveProyecto,
                        NombreProyecto = primero.NombreProyecto,
                        Cambios = grupo.Select(MapearCambioVista).ToList(),
                        AltaDatos = altaDatos,
                        Candidatos = altaDatos == null
                            ? new List<PamProyectoCandidatoVista>()
                            : CrearCandidatos(altaDatos, candidatePool),
                        Decision = MapearDecision(alta)
                    };
                })
                .ToList();
            model.Cambios = model.Proyectos.SelectMany(proyecto => proyecto.Cambios).ToList();
            return model;
        }

        public async Task<PamAnalisisEstadoDto> ObtenerEstadoAsync(long loteId, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT TOP (1)
    AnalisisId, Estado, TotalFuentes, FuentesProcesadas,
    CAST(CASE WHEN TotalFuentes = 0 THEN 0 ELSE FuentesProcesadas * 100.0 / TotalFuentes END AS DECIMAL(6,2)) AS ProgresoPorcentaje,
    MensajeResultado AS Mensaje
FROM dgmesnie.PAMAnalisisEjecucion
WHERE LoteId = @LoteId
ORDER BY AnalisisId DESC;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<PamAnalisisEstadoDto>(new CommandDefinition(
                sql, new { LoteId = loteId }, cancellationToken: cancellationToken));
        }

        public async Task GuardarDecisionPreliminarAsync(
            long loteId,
            PamGuardarDecisionPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (input.CambioPropuestoId <= 0)
                throw new PamDecisionPreliminarException("La propuesta de alta no es válida. Recarga la página e intenta nuevamente.");

            var clasificacion = NormalizarClasificacionPreliminar(input.Clasificacion)
                ?? throw new PamDecisionPreliminarException("Selecciona cómo debe tratarse este registro.");
            var requiereRelacionado = clasificacion is "Vincular existente" or "Padre-hijo";
            if (requiereRelacionado && !input.ProyectoRelacionadoId.HasValue)
                throw new PamDecisionPreliminarException("Selecciona el proyecto vigente con el que se relacionará el registro.");

            var proyectoRelacionadoId = requiereRelacionado ? input.ProyectoRelacionadoId : null;
            byte[] versionEsperada;
            try
            {
                versionEsperada = LeerVersionDecision(input.VersionDecision);
            }
            catch (ArgumentException)
            {
                throw new PamDecisionPreliminarException("La revisión cambió o perdió su versión. Recarga la página antes de intentarlo nuevamente.");
            }
            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            var nota = Limitar(input.Nota, 1000);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var target = await connection.QuerySingleOrDefaultAsync<DecisionTargetRow>(new CommandDefinition(@"
SELECT r.RevisionId, r.VersionDecision
FROM dgmesnie.PAMRevisionPendiente r WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMCambioPropuesto cp ON cp.CambioPropuestoId = r.CambioPropuestoId
INNER JOIN dgmesnie.PAMAnalisisEjecucion e ON e.AnalisisId = r.AnalisisId
WHERE cp.CambioPropuestoId = @CambioPropuestoId
  AND cp.AnalisisId = r.AnalisisId
  AND cp.TipoCambio = N'Alta'
  AND cp.EstadoRevision IN (N'Pendiente', N'Requiere revisión', N'Conflicto')
  AND r.Estado = N'Pendiente'
  AND e.LoteId = @LoteId
  AND e.Estado IN (N'Completado', N'Completado con observaciones')
  AND e.AnalisisId =
  (
      SELECT TOP (1) reciente.AnalisisId
      FROM dgmesnie.PAMAnalisisEjecucion reciente
      WHERE reciente.LoteId = @LoteId
      ORDER BY reciente.AnalisisId DESC
  );", new
                {
                    input.CambioPropuestoId,
                    LoteId = loteId
                }, transaction, cancellationToken: cancellationToken));

                if (target == null)
                    throw new PamDecisionPreliminarException("La propuesta ya no pertenece al análisis vigente o dejó de estar pendiente.");
                if (target.VersionDecision == null || !target.VersionDecision.SequenceEqual(versionEsperada))
                    throw new DBConcurrencyException("La decisión cambió mientras la revisabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                if (proyectoRelacionadoId.HasValue)
                {
                    var vigente = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT COUNT(*)
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
WHERE p.ProyectoId = @ProyectoId
  AND p.Activo = 1
  AND v.EstadoVigenciaCartera = N'Vigente';",
                        new { ProyectoId = proyectoRelacionadoId.Value }, transaction, cancellationToken: cancellationToken));
                    if (vigente != 1)
                        throw new PamDecisionPreliminarException("El proyecto relacionado no existe o ya no está vigente.");
                }

                await connection.ExecuteAsync(new CommandDefinition(@"
INSERT dgmesnie.PAMDecisionPreliminarHistorial
(
    RevisionId, ClasificacionPreliminar, ProyectoRelacionadoId,
    NotaPreliminar, UsuarioDecisionPreliminarId, UsuarioDecisionPreliminar
)
VALUES
(
    @RevisionId, @Clasificacion, @ProyectoRelacionadoId,
    @Nota, @UsuarioId, @UsuarioNombre
);", new
                {
                    target.RevisionId,
                    Clasificacion = clasificacion,
                    ProyectoRelacionadoId = proyectoRelacionadoId,
                    Nota = nota,
                    UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                    UsuarioNombre = nombreUsuario
                }, transaction, cancellationToken: cancellationToken));

                var nuevaVersion = await connection.QuerySingleOrDefaultAsync<byte[]>(new CommandDefinition(@"
UPDATE dgmesnie.PAMRevisionPendiente
SET ClasificacionPreliminar = @Clasificacion,
    ProyectoRelacionadoPreliminarId = @ProyectoRelacionadoId,
    NotaPreliminar = @Nota,
    FechaDecisionPreliminarUtc = SYSUTCDATETIME(),
    UsuarioDecisionPreliminarId = @UsuarioId,
    UsuarioDecisionPreliminar = @UsuarioNombre,
    Estado = N'Revisada',
    Resolucion = N'Clasificación preliminar registrada; pendiente de aplicación a la cartera.',
    FechaResolucionUtc = SYSUTCDATETIME(),
    UsuarioResolucion = @UsuarioNombre
OUTPUT inserted.VersionDecision
WHERE RevisionId = @RevisionId
  AND Estado = N'Pendiente'
  AND VersionDecision = @VersionDecision;", new
                {
                    target.RevisionId,
                    Clasificacion = clasificacion,
                    ProyectoRelacionadoId = proyectoRelacionadoId,
                    Nota = nota,
                    UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                    UsuarioNombre = nombreUsuario,
                    VersionDecision = versionEsperada
                }, transaction, cancellationToken: cancellationToken));

                if (nuevaVersion == null)
                    throw new DBConcurrencyException("La decisión cambió mientras la revisabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task GuardarDecisionHallazgoAsync(
            long loteId,
            PamGuardarHallazgoPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (input.HallazgoId <= 0 || input.RevisionId <= 0)
                throw new PamDecisionPreliminarException("El hallazgo ya no es válido. Recarga la página e intenta nuevamente.");

            var clasificacion = NormalizarClasificacionPreliminar(input.Clasificacion)
                ?? throw new PamDecisionPreliminarException("Selecciona cómo debe tratarse el hallazgo reportado.");
            if (clasificacion is not ("Alta real" or "Vincular existente" or "Descartar"))
                throw new PamDecisionPreliminarException("La clasificación seleccionada no aplica a este hallazgo.");

            var requiereRelacionado = clasificacion == "Vincular existente";
            if (requiereRelacionado && !input.ProyectoRelacionadoId.HasValue)
                throw new PamDecisionPreliminarException("Selecciona el proyecto vigente que deberá marcarse como cancelado.");

            var proyectoRelacionadoId = requiereRelacionado ? input.ProyectoRelacionadoId : null;
            byte[] versionEsperada;
            try
            {
                versionEsperada = LeerVersionDecision(input.VersionDecision);
            }
            catch (ArgumentException)
            {
                throw new PamDecisionPreliminarException("La revisión cambió o perdió su versión. Recarga la página antes de intentarlo nuevamente.");
            }

            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            var nota = Limitar(input.Nota, 1000);
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var target = await connection.QuerySingleOrDefaultAsync<DecisionTargetRow>(new CommandDefinition(@"
SELECT r.RevisionId, r.VersionDecision,
       CAST(CASE WHEN h.DatosExtraidosJson LIKE N'%Cancel%' THEN 1 ELSE 0 END AS BIT) AS EsCancelacion
FROM dgmesnie.PAMRevisionPendiente r WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
INNER JOIN dgmesnie.PAMAnalisisEjecucion e ON e.AnalisisId = r.AnalisisId
WHERE r.RevisionId = @RevisionId
  AND r.HallazgoId = @HallazgoId
  AND r.CambioPropuestoId IS NULL
  AND r.Estado = N'Pendiente'
  AND e.LoteId = @LoteId
  AND e.Estado IN (N'Completado', N'Completado con observaciones')
  AND e.AnalisisId =
  (
      SELECT TOP (1) reciente.AnalisisId
      FROM dgmesnie.PAMAnalisisEjecucion reciente
      WHERE reciente.LoteId = @LoteId
      ORDER BY reciente.AnalisisId DESC
  );", new
                {
                    input.RevisionId,
                    input.HallazgoId,
                    LoteId = loteId
                }, transaction, cancellationToken: cancellationToken));

                if (target == null)
                    throw new PamDecisionPreliminarException("El hallazgo ya no pertenece al análisis vigente o dejó de estar pendiente.");
                if (target.VersionDecision == null || !target.VersionDecision.SequenceEqual(versionEsperada))
                    throw new DBConcurrencyException("La decisión cambió mientras la revisabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                if (proyectoRelacionadoId.HasValue)
                {
                    var vigente = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT COUNT(*)
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
WHERE p.ProyectoId = @ProyectoId
  AND p.Activo = 1
  AND v.EstadoVigenciaCartera = N'Vigente';",
                        new { ProyectoId = proyectoRelacionadoId.Value }, transaction, cancellationToken: cancellationToken));
                    if (vigente != 1)
                        throw new PamDecisionPreliminarException("El proyecto seleccionado no existe o ya no está vigente.");
                }

                await connection.ExecuteAsync(new CommandDefinition(@"
INSERT dgmesnie.PAMDecisionPreliminarHistorial
(
    RevisionId, ClasificacionPreliminar, ProyectoRelacionadoId,
    NotaPreliminar, UsuarioDecisionPreliminarId, UsuarioDecisionPreliminar
)
VALUES
(
    @RevisionId, @Clasificacion, @ProyectoRelacionadoId,
    @Nota, @UsuarioId, @UsuarioNombre
);", new
                {
                    target.RevisionId,
                    Clasificacion = clasificacion,
                    ProyectoRelacionadoId = proyectoRelacionadoId,
                    Nota = nota,
                    UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                    UsuarioNombre = nombreUsuario
                }, transaction, cancellationToken: cancellationToken));

                var resolucion = clasificacion switch
                {
                    "Alta real" => target.EsCancelacion
                        ? "Cancelación clasificada para registrar el proyecto como antecedente cancelado; pendiente de aplicación."
                        : "Hallazgo sin propuesta automática clasificado como alta real; pendiente de aplicación.",
                    "Vincular existente" => target.EsCancelacion
                        ? "Cancelación vinculada a un proyecto vigente; pendiente de aplicación."
                        : "Hallazgo sin propuesta automática vinculado a un proyecto vigente; pendiente de aplicación.",
                    _ => target.EsCancelacion
                        ? "Reporte de cancelación descartado; no se aplicará a la cartera."
                        : "Hallazgo sin propuesta automática descartado; no se aplicará a la cartera."
                };
                var nuevaVersion = await connection.QuerySingleOrDefaultAsync<byte[]>(new CommandDefinition(@"
UPDATE dgmesnie.PAMRevisionPendiente
SET ClasificacionPreliminar = @Clasificacion,
    ProyectoRelacionadoPreliminarId = @ProyectoRelacionadoId,
    NotaPreliminar = @Nota,
    FechaDecisionPreliminarUtc = SYSUTCDATETIME(),
    UsuarioDecisionPreliminarId = @UsuarioId,
    UsuarioDecisionPreliminar = @UsuarioNombre,
    Estado = N'Revisada',
    Resolucion = @Resolucion,
    FechaResolucionUtc = SYSUTCDATETIME(),
    UsuarioResolucion = @UsuarioNombre
OUTPUT inserted.VersionDecision
WHERE RevisionId = @RevisionId
  AND Estado = N'Pendiente'
  AND VersionDecision = @VersionDecision;", new
                {
                    target.RevisionId,
                    Clasificacion = clasificacion,
                    ProyectoRelacionadoId = proyectoRelacionadoId,
                    Nota = nota,
                    UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                    UsuarioNombre = nombreUsuario,
                    Resolucion = resolucion,
                    VersionDecision = versionEsperada
                }, transaction, cancellationToken: cancellationToken));

                if (nuevaVersion == null)
                    throw new DBConcurrencyException("La decisión cambió mientras la revisabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task GuardarDecisionesCampoAsync(
            long loteId,
            PamGuardarDecisionesCampoInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (input.ProyectoVersionBaseId <= 0 || input.Cambios == null || input.Cambios.Count == 0)
                throw new PamDecisionPreliminarException("La revisión del proyecto no contiene campos válidos. Recarga la página.");

            var idsRecibidos = input.Cambios.Select(cambio => cambio.CambioPropuestoId).ToList();
            if (idsRecibidos.Any(id => id <= 0) || idsRecibidos.Distinct().Count() != idsRecibidos.Count)
                throw new PamDecisionPreliminarException("La revisión contiene campos repetidos o inválidos. Recarga la página.");

            var decisiones = input.Cambios.ToDictionary(
                cambio => cambio.CambioPropuestoId,
                cambio => new
                {
                    Decision = NormalizarDecisionCampo(cambio.Decision)
                        ?? throw new PamDecisionPreliminarException("Selecciona Aplicar, Mantener SQL u Omitir para cada campo."),
                    cambio.UltimaDecisionId
                });
            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            var notaProyecto = Limitar(input.NotaProyecto, 1000);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var targets = (await connection.QueryAsync<CampoDecisionTargetRow>(new CommandDefinition(@"
SELECT cp.CambioPropuestoId,
       decisionActual.CambioDecisionId
FROM dgmesnie.PAMCambioPropuesto cp WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMAnalisisEjecucion e ON e.AnalisisId = cp.AnalisisId
OUTER APPLY
(
    SELECT TOP (1) decision.CambioDecisionId
    FROM dgmesnie.PAMCambioDecisionHistorial decision
    WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY decision.CambioDecisionId DESC
) decisionActual
WHERE cp.ProyectoVersionBaseId = @ProyectoVersionBaseId
  AND cp.TipoCambio <> N'Alta'
  AND e.LoteId = @LoteId
  AND e.Estado IN (N'Completado', N'Completado con observaciones')
  AND e.AnalisisId =
  (
      SELECT TOP (1) reciente.AnalisisId
      FROM dgmesnie.PAMAnalisisEjecucion reciente
      WHERE reciente.LoteId = @LoteId
      ORDER BY reciente.AnalisisId DESC
  )
ORDER BY cp.CambioPropuestoId;", new
                {
                    input.ProyectoVersionBaseId,
                    LoteId = loteId
                }, transaction, cancellationToken: cancellationToken))).ToList();

                if (targets.Count == 0)
                    throw new PamDecisionPreliminarException("El proyecto ya no pertenece al análisis vigente.");
                if (targets.Count != decisiones.Count
                    || targets.Any(target => !decisiones.ContainsKey(target.CambioPropuestoId)))
                    throw new PamDecisionPreliminarException("La lista de campos cambió. Recarga la página antes de guardar.");

                foreach (var target in targets)
                {
                    var recibida = decisiones[target.CambioPropuestoId];
                    if (target.CambioDecisionId != recibida.UltimaDecisionId)
                        throw new DBConcurrencyException("Otra persona actualizó una decisión de este proyecto. Recarga la pantalla.");
                }

                foreach (var target in targets)
                {
                    var recibida = decisiones[target.CambioPropuestoId];
                    await connection.ExecuteAsync(new CommandDefinition(@"
INSERT dgmesnie.PAMCambioDecisionHistorial
(
    CambioPropuestoId, DecisionCampo, NotaProyecto,
    UsuarioDecisionId, UsuarioDecision
)
VALUES
(
    @CambioPropuestoId, @DecisionCampo, @NotaProyecto,
    @UsuarioDecisionId, @UsuarioDecision
);", new
                    {
                        target.CambioPropuestoId,
                        DecisionCampo = recibida.Decision,
                        NotaProyecto = notaProyecto,
                        UsuarioDecisionId = usuarioId > 0 ? usuarioId : (int?)null,
                        UsuarioDecision = nombreUsuario
                    }, transaction, cancellationToken: cancellationToken));
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task ReabrirDecisionPreliminarAsync(
            long loteId,
            PamReabrirDecisionPreliminarInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (input.CambioPropuestoId <= 0 && input.RevisionId <= 0)
                throw new PamDecisionPreliminarException("La revisión no es válida. Recarga la página e intenta nuevamente.");

            byte[] versionEsperada;
            try
            {
                versionEsperada = LeerVersionDecision(input.VersionDecision);
            }
            catch (ArgumentException)
            {
                throw new PamDecisionPreliminarException("La revisión cambió o perdió su versión. Recarga la página antes de intentarlo nuevamente.");
            }

            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var target = await connection.QuerySingleOrDefaultAsync<DecisionTargetRow>(new CommandDefinition(@"
SELECT r.RevisionId, r.VersionDecision
FROM dgmesnie.PAMRevisionPendiente r WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMAnalisisEjecucion e ON e.AnalisisId = r.AnalisisId
LEFT JOIN dgmesnie.PAMCambioPropuesto cp
    ON cp.CambioPropuestoId = r.CambioPropuestoId AND cp.AnalisisId = r.AnalisisId
WHERE
  (
      (@RevisionId > 0 AND r.RevisionId = @RevisionId)
      OR
      (@RevisionId <= 0 AND cp.CambioPropuestoId = @CambioPropuestoId AND cp.TipoCambio = N'Alta')
  )
  AND r.Estado = N'Revisada'
  AND r.ClasificacionPreliminar IS NOT NULL
  AND e.LoteId = @LoteId
  AND e.AnalisisId =
  (
      SELECT TOP (1) reciente.AnalisisId
      FROM dgmesnie.PAMAnalisisEjecucion reciente
      WHERE reciente.LoteId = @LoteId
      ORDER BY reciente.AnalisisId DESC
  );", new
                {
                    input.CambioPropuestoId,
                    input.RevisionId,
                    LoteId = loteId
                }, transaction, cancellationToken: cancellationToken));

                if (target == null)
                    throw new PamDecisionPreliminarException("La revisión ya no está cerrada o dejó de pertenecer al análisis vigente.");
                if (target.VersionDecision == null || !target.VersionDecision.SequenceEqual(versionEsperada))
                    throw new DBConcurrencyException("La revisión cambió mientras la consultabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                var nuevaVersion = await connection.QuerySingleOrDefaultAsync<byte[]>(new CommandDefinition(@"
UPDATE dgmesnie.PAMRevisionPendiente
SET Estado = N'Pendiente',
    Resolucion = N'Revisión reabierta para corregir la clasificación preliminar; la cartera maestra permanece sin cambios.',
    FechaResolucionUtc = SYSUTCDATETIME(),
    UsuarioResolucion = @UsuarioNombre
OUTPUT inserted.VersionDecision
WHERE RevisionId = @RevisionId
  AND Estado = N'Revisada'
  AND VersionDecision = @VersionDecision;", new
                {
                    target.RevisionId,
                    UsuarioNombre = nombreUsuario,
                    VersionDecision = versionEsperada
                }, transaction, cancellationToken: cancellationToken));

                if (nuevaVersion == null)
                    throw new DBConcurrencyException("La revisión cambió mientras la consultabas. Actualiza la pantalla antes de intentarlo nuevamente.");

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task<PamPreparacionAplicacionViewModel> ObtenerPreparacionAplicacionAsync(
            long loteId,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));

            const string sql = @"
SET NOCOUNT ON;

SELECT l.LoteId, l.LoteUid, l.Nombre, l.FechaCorte, l.Notas, l.Estado,
       l.TotalArchivos, l.FechaRegistroUtc, l.UsuarioNombre
FROM dgmesnie.PAMLoteActualizacion l
WHERE l.LoteId = @LoteId;

DECLARE @AnalisisId BIGINT =
(
    SELECT TOP (1) e.AnalisisId
    FROM dgmesnie.PAMAnalisisEjecucion e
    WHERE e.LoteId = @LoteId
    ORDER BY e.AnalisisId DESC
);

SELECT COALESCE(@AnalisisId, 0);

SELECT r.RevisionId, r.ClaveRecibida, r.NombreRecibido,
       r.Estado AS EstadoRevision, r.ClasificacionPreliminar,
       CASE
           WHEN r.Estado <> N'Revisada' OR r.ClasificacionPreliminar IS NULL THEN N'Pendiente'
           WHEN r.ClasificacionPreliminar = N'Descartar' THEN N'No aplicar'
           WHEN h.DatosExtraidosJson LIKE N'%Cancel%' AND r.ClasificacionPreliminar = N'Alta real' THEN N'Antecedente cancelado'
           WHEN h.DatosExtraidosJson LIKE N'%Cancel%' AND r.ClasificacionPreliminar = N'Vincular existente' THEN N'Cancelar existente'
           WHEN r.ClasificacionPreliminar = N'Alta real' THEN N'Alta vigente'
           WHEN r.ClasificacionPreliminar = N'Vincular existente' THEN N'Vincular clave'
           WHEN r.ClasificacionPreliminar = N'Padre-hijo' THEN N'Relación por validar'
           ELSE N'Pendiente'
       END AS TipoAccion,
       r.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId,
       relacionada.ClaveProyecto AS ProyectoRelacionadoClave,
       relacionada.NombreProyecto AS ProyectoRelacionadoNombre,
       f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente,
       CAST(CASE WHEN h.DatosExtraidosJson LIKE N'%Cancel%' THEN 1 ELSE 0 END AS BIT) AS EsCancelacion
INTO #AccionesPreparacion
FROM dgmesnie.PAMRevisionPendiente r
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion relacionada
    ON relacionada.ProyectoId = r.ProyectoRelacionadoPreliminarId
   AND relacionada.EsVersionVigente = 1
WHERE r.AnalisisId = @AnalisisId
  AND h.ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave');

SELECT RevisionId, ClaveRecibida, NombreRecibido, EstadoRevision,
       ClasificacionPreliminar, TipoAccion, ProyectoRelacionadoId,
       ProyectoRelacionadoClave, ProyectoRelacionadoNombre,
       FuenteDocumento, UbicacionFuente, EsCancelacion
FROM #AccionesPreparacion
ORDER BY
    CASE WHEN EstadoRevision = N'Revisada' THEN 1 ELSE 0 END,
    RevisionId;

SELECT cp.CambioPropuestoId, cp.ProyectoVersionBaseId,
       v.ClaveProyecto, v.NombreProyecto, cp.Campo,
       JSON_VALUE(cp.ValorActualJson, '$.value') AS ValorActual,
       JSON_VALUE(cp.ValorPropuestoJson, '$.value') AS ValorPropuesto,
       cp.ValorActualJson, cp.ValorPropuestoJson,
       decisionActual.CambioDecisionId,
       decisionActual.DecisionCampo,
       f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente
INTO #CamposPreparacion
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoVersionId = cp.ProyectoVersionBaseId
OUTER APPLY
(
    SELECT TOP (1) decision.CambioDecisionId, decision.DecisionCampo
    FROM dgmesnie.PAMCambioDecisionHistorial decision
    WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY decision.CambioDecisionId DESC
) decisionActual
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio <> N'Alta';

SELECT COUNT(*) AS Total,
       SUM(CASE WHEN DecisionCampo = N'Aplicar' THEN 1 ELSE 0 END) AS Aplicar,
       SUM(CASE WHEN DecisionCampo = N'Mantener SQL' THEN 1 ELSE 0 END) AS MantenerSql,
       SUM(CASE WHEN DecisionCampo = N'Omitir' THEN 1 ELSE 0 END) AS Omitir,
       SUM(CASE WHEN DecisionCampo IS NULL THEN 1 ELSE 0 END) AS Pendientes,
       COUNT(DISTINCT CASE WHEN DecisionCampo IS NULL THEN ProyectoVersionBaseId END) AS ProyectosPendientes,
       (SELECT COUNT(*)
        FROM
        (
            SELECT revisado.ProyectoVersionBaseId
            FROM #CamposPreparacion revisado
            GROUP BY revisado.ProyectoVersionBaseId
            HAVING SUM(CASE WHEN revisado.DecisionCampo IS NULL THEN 1 ELSE 0 END) = 0
        ) proyectosRevisados) AS ProyectosRevisados
FROM #CamposPreparacion;

SELECT Campo,
       CASE
           WHEN Campo IN (N'NombreProyecto', N'EtapaProyecto', N'EstadoRealProyecto', N'MontoProyectoMdp', N'AnioInstruccion')
               THEN N'Alto'
           ELSE N'Medio'
       END AS NivelRiesgo,
       COUNT(*) AS Total,
       SUM(CASE WHEN DecisionCampo = N'Aplicar' THEN 1 ELSE 0 END) AS Aplicar,
       SUM(CASE WHEN DecisionCampo = N'Mantener SQL' THEN 1 ELSE 0 END) AS MantenerSql,
       SUM(CASE WHEN DecisionCampo = N'Omitir' THEN 1 ELSE 0 END) AS Omitir
FROM #CamposPreparacion
GROUP BY Campo,
         CASE
             WHEN Campo IN (N'NombreProyecto', N'EtapaProyecto', N'EstadoRealProyecto', N'MontoProyectoMdp', N'AnioInstruccion')
                 THEN N'Alto'
             ELSE N'Medio'
         END
ORDER BY CASE
             WHEN Campo IN (N'NombreProyecto', N'EtapaProyecto', N'EstadoRealProyecto', N'MontoProyectoMdp', N'AnioInstruccion')
                 THEN 0
             ELSE 1
         END,
         Total DESC, Campo;

SELECT validacion.Severidad, validacion.Codigo, validacion.Titulo,
       validacion.Detalle, validacion.Total
FROM
(
    SELECT N'Bloqueo' AS Severidad, N'DECISIONES_PENDIENTES' AS Codigo,
           N'Existen decisiones pendientes' AS Titulo,
           N'Cada campo debe tener una decisión antes de congelar el paquete.' AS Detalle,
           (SELECT COUNT(*) FROM #CamposPreparacion WHERE DecisionCampo IS NULL) AS Total

    UNION ALL

    SELECT N'Advertencia', N'FECHAS_CON_HITOS', N'Fechas con hitos o texto',
           N'Se conservarán completas como texto en Fecha necesaria o entrada en operación; no se perderán etapas ni fases.',
           (SELECT COUNT(*) FROM #CamposPreparacion
            WHERE DecisionCampo = N'Aplicar'
              AND Campo IN (N'FechaNecesaria', N'FeoFactible')
              AND NULLIF(LTRIM(RTRIM(ValorPropuesto)), N'') IS NOT NULL
              AND TRY_CONVERT(DATE, ValorPropuesto) IS NULL)

    UNION ALL

    SELECT N'Bloqueo', N'MONTOS_INVALIDOS', N'Montos propuestos no válidos',
           N'Los montos por aplicar deben ser numéricos y no negativos.',
           (SELECT COUNT(*) FROM #CamposPreparacion
            WHERE DecisionCampo = N'Aplicar' AND Campo = N'MontoProyectoMdp'
              AND (TRY_CONVERT(DECIMAL(20,3), ValorPropuesto) IS NULL
                   OR TRY_CONVERT(DECIMAL(20,3), ValorPropuesto) < 0))

    UNION ALL

    SELECT N'Bloqueo', N'NOMBRES_VACIOS', N'Nombres propuestos vacíos',
           N'No se puede aplicar un nombre de proyecto vacío.',
           (SELECT COUNT(*) FROM #CamposPreparacion
            WHERE DecisionCampo = N'Aplicar' AND Campo = N'NombreProyecto'
              AND NULLIF(LTRIM(RTRIM(ValorPropuesto)), N'') IS NULL)

    UNION ALL

    SELECT N'Bloqueo', N'VINCULOS_SIN_DESTINO', N'Vínculos sin proyecto destino',
           N'Cada vínculo o cancelación de un existente debe señalar el proyecto relacionado.',
           (SELECT COUNT(*) FROM #AccionesPreparacion
            WHERE TipoAccion IN (N'Vincular clave', N'Cancelar existente')
              AND ProyectoRelacionadoId IS NULL)

    UNION ALL

    SELECT N'Bloqueo', N'REVISIONES_CLAVE_PENDIENTES', N'Claves sin clasificación cerrada',
           N'Las altas, vínculos y cancelaciones deben permanecer en estado Revisada.',
           (SELECT COUNT(*) FROM #AccionesPreparacion
            WHERE EstadoRevision <> N'Revisada' OR TipoAccion IN (N'Pendiente', N'Relación por validar'))

    UNION ALL

    SELECT N'Advertencia', N'TODOS_LOS_CAMPOS_APLICAN', N'Todas las diferencias se aplicarán',
           N'No existen decisiones Mantener SQL u Omitir; confirme que este criterio masivo es intencional.',
           CASE
               WHEN EXISTS (SELECT 1 FROM #CamposPreparacion)
                AND NOT EXISTS (SELECT 1 FROM #CamposPreparacion WHERE DecisionCampo <> N'Aplicar' OR DecisionCampo IS NULL)
                   THEN (SELECT COUNT(*) FROM #CamposPreparacion)
               ELSE 0
           END

    UNION ALL

    SELECT N'Advertencia', N'CAMBIOS_ALTO_IMPACTO', N'Campos de alto impacto por aplicar',
           N'Incluye nombres, etapas, estados, montos o años de instrucción; revíselos en el resumen agrupado.',
           (SELECT COUNT(*) FROM #CamposPreparacion
            WHERE DecisionCampo = N'Aplicar'
              AND Campo IN (N'NombreProyecto', N'EtapaProyecto', N'EstadoRealProyecto', N'MontoProyectoMdp', N'AnioInstruccion'))
) validacion
WHERE validacion.Total > 0
ORDER BY CASE validacion.Severidad WHEN N'Bloqueo' THEN 0 ELSE 1 END,
         validacion.Codigo;

SELECT TOP (100)
       CambioPropuestoId, ProyectoVersionBaseId, ClaveProyecto, NombreProyecto,
       Campo, ValorActual, ValorPropuesto, DecisionCampo,
       FuenteDocumento, UbicacionFuente
FROM #CamposPreparacion
WHERE DecisionCampo = N'Aplicar'
ORDER BY NombreProyecto, ClaveProyecto, Campo, CambioPropuestoId;

DECLARE @ContenidoActual NVARCHAR(MAX) =
    COALESCE(
        (SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo
         FROM #CamposPreparacion
         ORDER BY CambioPropuestoId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]')
    + N'|'
    + COALESCE(
        (SELECT RevisionId, EstadoRevision, ClasificacionPreliminar,
                TipoAccion, ProyectoRelacionadoId
         FROM #AccionesPreparacion
         ORDER BY RevisionId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]');

DECLARE @HashActual CHAR(64) =
    CONVERT(CHAR(64), HASHBYTES('SHA2_256', @ContenidoActual), 2);

SELECT @HashActual AS HashActual,
       paquete.PaqueteAplicacionId, paquete.PaqueteUid, paquete.HashContenido,
       COALESCE(evento.TipoEvento, N'Congelado') AS EstadoActual,
       paquete.TotalDetalles, paquete.TotalCambiosCampo, paquete.TotalAccionesClave,
       paquete.UsuarioCreacion, paquete.FechaCreacionUtc,
       CAST(CASE WHEN paquete.HashContenido = @HashActual THEN 1 ELSE 0 END AS BIT) AS EsVigente
FROM (SELECT 1 AS Ancla) ancla
OUTER APPLY
(
    SELECT TOP (1) p.PaqueteAplicacionId, p.PaqueteUid, p.HashContenido,
           p.TotalDetalles, p.TotalCambiosCampo, p.TotalAccionesClave,
           p.UsuarioCreacion, p.FechaCreacionUtc
    FROM dgmesnie.PAMAplicacionPaquete p
    WHERE p.AnalisisId = @AnalisisId
    ORDER BY p.PaqueteAplicacionId DESC
) paquete
OUTER APPLY
(
    SELECT TOP (1) e.TipoEvento
    FROM dgmesnie.PAMAplicacionPaqueteEvento e
    WHERE e.PaqueteAplicacionId = paquete.PaqueteAplicacionId
    ORDER BY e.PaqueteEventoId DESC
) evento;

DECLARE @PaquetePreflightId BIGINT =
(
    SELECT TOP (1) p.PaqueteAplicacionId
    FROM dgmesnie.PAMAplicacionPaquete p
    WHERE p.AnalisisId = @AnalisisId
    ORDER BY p.PaqueteAplicacionId DESC
);
DECLARE @PaqueteEstado NVARCHAR(20) =
(
    SELECT TOP (1) e.TipoEvento
    FROM dgmesnie.PAMAplicacionPaqueteEvento e
    WHERE e.PaqueteAplicacionId = @PaquetePreflightId
    ORDER BY e.PaqueteEventoId DESC
);

SELECT
    (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar') AS TotalCambiosCampo,
    (SELECT COUNT(DISTINCT d.ProyectoVersionBaseId) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar') AS TotalVersionesNuevas,
    (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.Accion = N'Vincular clave') AS TotalClavesAlternas,
    (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.Accion = N'Alta vigente') AS TotalAltasVigentes,
    (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.Accion = N'Antecedente cancelado') AS TotalAntecedentes,
    (SELECT COUNT(DISTINCT lf.FuenteId)
     FROM dgmesnie.PAMAplicacionPaqueteDetalle d
     LEFT JOIN dgmesnie.PAMCambioPropuesto cp
       ON d.TipoElemento = N'CambioCampo' AND cp.CambioPropuestoId = d.ReferenciaId
     LEFT JOIN dgmesnie.PAMRevisionPendiente r
       ON d.TipoElemento = N'AccionClave' AND r.RevisionId = d.ReferenciaId
     INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = COALESCE(cp.HallazgoId, r.HallazgoId)
     INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
     WHERE d.PaqueteAplicacionId = @PaquetePreflightId) AS TotalFuentes,
    (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionResultado ar
     WHERE ar.PaqueteAplicacionId = @PaquetePreflightId) AS TotalResultados,
    CAST(CASE WHEN @PaqueteEstado = N'Aplicado' THEN 1 ELSE 0 END AS BIT) AS YaAplicado;

SELECT validacion.Severidad, validacion.Codigo, validacion.Titulo,
       validacion.Detalle, validacion.Total
FROM
(
    SELECT N'Bloqueo' AS Severidad, N'PAQUETE_DESACTUALIZADO' AS Codigo,
           N'El paquete no coincide con la revisión actual' AS Titulo,
           N'Las decisiones cambiaron después del congelado; genere un paquete nuevo.' AS Detalle,
           CASE WHEN @PaquetePreflightId IS NOT NULL
                  AND NOT EXISTS
                      (SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p
                       WHERE p.PaqueteAplicacionId = @PaquetePreflightId AND p.HashContenido = @HashActual)
                THEN 1 ELSE 0 END AS Total

    UNION ALL

    SELECT N'Bloqueo', N'DETALLE_INCOMPLETO', N'El detalle del paquete está incompleto',
           N'La cantidad de elementos no coincide con la cabecera inmutable.',
           CASE WHEN EXISTS
                (
                    SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p
                    WHERE p.PaqueteAplicacionId = @PaquetePreflightId
                      AND p.TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d WHERE d.PaqueteAplicacionId = p.PaqueteAplicacionId)
                ) THEN 1 ELSE 0 END

    UNION ALL

    SELECT N'Bloqueo', N'INTEGRIDAD_DETALLE', N'Falló la verificación de integridad',
           N'Una o más huellas de los elementos congelados no coincide.',
           (SELECT COUNT(*)
            FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            OUTER APPLY
            (
                SELECT TOP (1) cd.CambioDecisionId
                FROM dgmesnie.PAMCambioDecisionHistorial cd
                WHERE cd.CambioPropuestoId = d.ReferenciaId
                ORDER BY cd.CambioDecisionId DESC
            ) decisionCampo
            LEFT JOIN dgmesnie.PAMRevisionPendiente revision
              ON d.TipoElemento = N'AccionClave' AND revision.RevisionId = d.ReferenciaId
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId
              AND d.HashDetalle <>
                  CASE d.TipoElemento
                      WHEN N'CambioCampo' THEN CONVERT(CHAR(64), HASHBYTES('SHA2_256',
                          CONCAT(CONVERT(NVARCHAR(MAX), N'CambioCampo|'), d.ReferenciaId, N'|',
                                 decisionCampo.CambioDecisionId, N'|', d.Accion, N'|',
                                 COALESCE(d.ValorAnteriorJson, N''), N'|', d.ValorNuevoJson)), 2)
                      ELSE CONVERT(CHAR(64), HASHBYTES('SHA2_256',
                          CONCAT(CONVERT(NVARCHAR(MAX), N'AccionClave|'), d.ReferenciaId, N'|',
                                 sys.fn_varbintohexstr(revision.VersionDecision), N'|', d.Accion, N'|',
                                 COALESCE(CONVERT(NVARCHAR(30), d.ProyectoRelacionadoId), N''))), 2)
                  END)

    UNION ALL

    SELECT N'Bloqueo', N'DERIVA_VERSION', N'La cartera cambió después de la revisión',
           N'Alguna versión base ya no es la vigente; vuelva a cotejar antes de aplicar.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId
              AND d.TipoElemento = N'CambioCampo'
              AND NOT EXISTS
                  (SELECT 1 FROM dgmesnie.PAMProyectoVersion v
                   WHERE v.ProyectoVersionId = d.ProyectoVersionBaseId AND v.EsVersionVigente = 1))

    UNION ALL

    SELECT N'Bloqueo', N'CAMPOS_NO_SOPORTADOS', N'Existen campos sin regla de aplicación',
           N'Cada campo debe tener un mapeo explícito hacia la versión PAM.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId
              AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar'
              AND d.Campo NOT IN (N'NombreProyecto', N'GRT', N'EtapaProyecto', N'FechaNecesaria',
                                  N'FeoFactible', N'MontoProyectoMdp', N'EstadoRealProyecto', N'AnioInstruccion'))

    UNION ALL

    SELECT N'Bloqueo', N'VALORES_NO_CONVERTIBLES', N'Existen valores numéricos no convertibles',
           N'Los montos y años deben poder almacenarse sin pérdida en la cartera.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar'
              AND ((d.Campo = N'MontoProyectoMdp' AND TRY_CONVERT(DECIMAL(18,3), JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)
                OR (d.Campo = N'AnioInstruccion' AND TRY_CONVERT(INT, JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)))

    UNION ALL

    SELECT N'Bloqueo', N'CLAVES_SIN_DESTINO', N'Existen claves alternas sin proyecto destino',
           N'Cada clave vinculada debe apuntar a un proyecto vigente.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.Accion = N'Vincular clave'
              AND (d.ProyectoRelacionadoId IS NULL OR NOT EXISTS
                  (SELECT 1 FROM dgmesnie.PAMProyectoVersion v
                   WHERE v.ProyectoId = d.ProyectoRelacionadoId AND v.EsVersionVigente = 1)))

    UNION ALL

    SELECT N'Bloqueo', N'CONFLICTO_CLAVE', N'Una clave ya pertenece a otro proyecto',
           N'No se aplicarán claves duplicadas ni se moverán entre proyectos de forma implícita.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId
              AND d.Accion IN (N'Vincular clave', N'Alta vigente', N'Antecedente cancelado')
              AND
              (
                  EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoVersion v
                          WHERE v.EsVersionVigente = 1 AND LTRIM(RTRIM(v.ClaveProyecto)) = LTRIM(RTRIM(d.ClaveProyecto))
                            AND (d.ProyectoRelacionadoId IS NULL OR v.ProyectoId <> d.ProyectoRelacionadoId))
                  OR EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoClaveVersion clave
                             WHERE clave.EsVigente = 1 AND LTRIM(RTRIM(clave.ClaveProyecto)) = LTRIM(RTRIM(d.ClaveProyecto))
                              AND (d.ProyectoRelacionadoId IS NULL OR clave.ProyectoId <> d.ProyectoRelacionadoId))
              ))

    UNION ALL

    SELECT N'Bloqueo', N'CONFLICTO_CLAVE_PAQUETE', N'Una clave apunta a varios proyectos en el paquete',
           N'Revisa las filas repetidas: una misma clave alterna no puede vincularse a destinos distintos.',
           (SELECT COUNT(*)
            FROM
            (
                SELECT LTRIM(RTRIM(d.ClaveProyecto)) AS ClaveProyecto
                FROM dgmesnie.PAMAplicacionPaqueteDetalle d
                WHERE d.PaqueteAplicacionId = @PaquetePreflightId
                  AND d.Accion = N'Vincular clave'
                  AND NULLIF(LTRIM(RTRIM(d.ClaveProyecto)), N'') IS NOT NULL
                GROUP BY LTRIM(RTRIM(d.ClaveProyecto))
                HAVING COUNT(DISTINCT d.ProyectoRelacionadoId) > 1
            ) conflicto)

    UNION ALL

    SELECT N'Bloqueo', N'ALTAS_SIN_DATOS', N'Faltan datos fuente para crear proyectos',
           N'Las altas y antecedentes requieren clave, nombre y evidencia extraída válida.',
           (SELECT COUNT(*)
            FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            LEFT JOIN dgmesnie.PAMRevisionPendiente r ON r.RevisionId = d.ReferenciaId
            LEFT JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId
              AND d.Accion IN (N'Alta vigente', N'Antecedente cancelado')
              AND (NULLIF(LTRIM(RTRIM(d.ClaveProyecto)), N'') IS NULL
                   OR NULLIF(LTRIM(RTRIM(d.NombreProyecto)), N'') IS NULL
                   OR ISJSON(h.DatosExtraidosJson) <> 1))

    UNION ALL

    SELECT N'Advertencia', N'FECHAS_TEXTO', N'Las fechas con hitos se conservarán como texto',
           N'Fecha necesaria y entrada en operación son campos textuales en PAM; no se descartarán etapas ni fases.',
           (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.Accion = N'Aplicar'
              AND d.Campo IN (N'FechaNecesaria', N'FeoFactible')
              AND TRY_CONVERT(DATE, JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)

    UNION ALL

    SELECT N'Advertencia', N'VERSIONADO_MASIVO', N'Se crearán nuevas versiones de la cartera',
           N'Las versiones actuales se cerrarán y permanecerán disponibles en el historial.',
           (SELECT COUNT(DISTINCT d.ProyectoVersionBaseId) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
            WHERE d.PaqueteAplicacionId = @PaquetePreflightId AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar')
) validacion
WHERE validacion.Total > 0
ORDER BY CASE validacion.Severidad WHEN N'Bloqueo' THEN 0 ELSE 1 END, validacion.Codigo;";

            await using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                sql, new { LoteId = loteId }, cancellationToken: cancellationToken));
            var lote = await multi.ReadSingleOrDefaultAsync<PamLoteAnalisisCabecera>();
            if (lote == null) return null;
            var analisisId = await multi.ReadSingleAsync<long>();
            var items = (await multi.ReadAsync<PamPreparacionAplicacionItem>()).ToList();
            var resumenCampos = await multi.ReadSingleAsync<PamPreparacionCamposResumen>();
            var gruposCampos = (await multi.ReadAsync<PamPreparacionCampoGrupo>()).ToList();
            var validaciones = (await multi.ReadAsync<PamPreparacionValidacionItem>()).ToList();
            var camposAplicables = (await multi.ReadAsync<PamPreparacionCambioCampoItem>()).ToList();
            var paqueteCierre = await multi.ReadSingleAsync<PamPaqueteCierreVista>();
            var aplicacion = await multi.ReadSingleAsync<PamAplicacionPreflightViewModel>();
            aplicacion.Validaciones = (await multi.ReadAsync<PamPreparacionValidacionItem>()).ToList();
            aplicacion.FraseConfirmacion = paqueteCierre.PaqueteUid.HasValue
                ? $"APLICAR {paqueteCierre.PaqueteUid.Value.ToString()[..8].ToUpperInvariant()}"
                : null;
            aplicacion.PuedeAplicar = paqueteCierre.PaqueteAplicacionId.HasValue
                && paqueteCierre.EsVigente
                && string.Equals(paqueteCierre.EstadoActual, "Congelado", StringComparison.OrdinalIgnoreCase)
                && aplicacion.TotalBloqueos == 0
                && !aplicacion.YaAplicado;

            return new PamPreparacionAplicacionViewModel
            {
                Lote = lote,
                AnalisisId = analisisId,
                ResumenCampos = resumenCampos,
                GruposCampos = gruposCampos,
                Validaciones = validaciones,
                CamposAplicables = camposAplicables,
                PaqueteCierre = paqueteCierre,
                Aplicacion = aplicacion,
                Pendientes = items
                    .Where(item => !string.Equals(item.EstadoRevision, "Revisada", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrWhiteSpace(item.ClasificacionPreliminar)
                        || string.Equals(item.TipoAccion, "Relación por validar", StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                Acciones = items
                    .Where(item => string.Equals(item.EstadoRevision, "Revisada", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(item.ClasificacionPreliminar)
                        && !string.Equals(item.TipoAccion, "Relación por validar", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            };
        }

        public async Task<PamPaqueteCierreResultado> CongelarPaqueteCierreAsync(
            long loteId,
            PamCongelarPaqueteCierreInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (!input.Confirmado)
                throw new PamPaqueteCierreException("Debes confirmar que revisaste el resumen final antes de congelar el paquete.");

            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            var nota = Limitar(input.Nota, 1000);

            const string sql = @"
SET NOCOUNT ON;

DECLARE @AnalisisId BIGINT =
(
    SELECT TOP (1) e.AnalisisId
    FROM dgmesnie.PAMAnalisisEjecucion e WITH (UPDLOCK, HOLDLOCK)
    WHERE e.LoteId = @LoteId
      AND e.Estado IN (N'Completado', N'Completado con observaciones')
    ORDER BY e.AnalisisId DESC
);

IF @AnalisisId IS NULL
    THROW 51040, N'El lote no tiene un análisis concluido para congelar.', 1;

SELECT cp.CambioPropuestoId, cp.ProyectoId, cp.ProyectoVersionBaseId,
       v.ClaveProyecto, v.NombreProyecto, cp.Campo,
       cp.ValorActualJson, cp.ValorPropuestoJson,
       decisionActual.CambioDecisionId, decisionActual.DecisionCampo,
       f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente
INTO #CamposCierre
FROM dgmesnie.PAMCambioPropuesto cp WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoVersionId = cp.ProyectoVersionBaseId
OUTER APPLY
(
    SELECT TOP (1) decision.CambioDecisionId, decision.DecisionCampo
    FROM dgmesnie.PAMCambioDecisionHistorial decision WITH (UPDLOCK, HOLDLOCK)
    WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY decision.CambioDecisionId DESC
) decisionActual
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio <> N'Alta';

SELECT r.RevisionId, r.ClaveRecibida, r.NombreRecibido,
       r.Estado AS EstadoRevision, r.ClasificacionPreliminar,
       CASE
           WHEN r.Estado <> N'Revisada' OR r.ClasificacionPreliminar IS NULL THEN N'Pendiente'
           WHEN r.ClasificacionPreliminar = N'Descartar' THEN N'No aplicar'
           WHEN h.DatosExtraidosJson LIKE N'%Cancel%' AND r.ClasificacionPreliminar = N'Alta real' THEN N'Antecedente cancelado'
           WHEN h.DatosExtraidosJson LIKE N'%Cancel%' AND r.ClasificacionPreliminar = N'Vincular existente' THEN N'Cancelar existente'
           WHEN r.ClasificacionPreliminar = N'Alta real' THEN N'Alta vigente'
           WHEN r.ClasificacionPreliminar = N'Vincular existente' THEN N'Vincular clave'
           WHEN r.ClasificacionPreliminar = N'Padre-hijo' THEN N'Relación por validar'
           ELSE N'Pendiente'
       END AS TipoAccion,
       r.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId,
       relacionada.ClaveProyecto AS ProyectoRelacionadoClave,
       relacionada.NombreProyecto AS ProyectoRelacionadoNombre,
       f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente,
       sys.fn_varbintohexstr(r.VersionDecision) AS VersionDecision
INTO #AccionesCierre
FROM dgmesnie.PAMRevisionPendiente r WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion relacionada
    ON relacionada.ProyectoId = r.ProyectoRelacionadoPreliminarId
   AND relacionada.EsVersionVigente = 1
WHERE r.AnalisisId = @AnalisisId
  AND h.ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave');

IF EXISTS (SELECT 1 FROM #CamposCierre WHERE CambioDecisionId IS NULL OR DecisionCampo IS NULL)
    THROW 51041, N'Todavía existen decisiones de campo pendientes.', 1;

IF EXISTS
(
    SELECT 1 FROM #AccionesCierre
    WHERE EstadoRevision <> N'Revisada' OR TipoAccion IN (N'Pendiente', N'Relación por validar')
)
    THROW 51042, N'Todavía existen altas, vínculos o cancelaciones pendientes.', 1;

IF EXISTS
(
    SELECT 1 FROM #CamposCierre
    WHERE DecisionCampo = N'Aplicar' AND Campo = N'MontoProyectoMdp'
      AND (TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(ValorPropuestoJson, '$.value')) IS NULL
           OR TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(ValorPropuestoJson, '$.value')) < 0)
)
    THROW 51044, N'Existen montos propuestos inválidos.', 1;

IF EXISTS
(
    SELECT 1 FROM #AccionesCierre
    WHERE TipoAccion IN (N'Vincular clave', N'Cancelar existente')
      AND ProyectoRelacionadoId IS NULL
)
    THROW 51045, N'Existen vínculos sin proyecto destino.', 1;

DECLARE @Contenido NVARCHAR(MAX) =
    COALESCE(
        (SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo
         FROM #CamposCierre
         ORDER BY CambioPropuestoId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]')
    + N'|'
    + COALESCE(
        (SELECT RevisionId, EstadoRevision, ClasificacionPreliminar,
                TipoAccion, ProyectoRelacionadoId
         FROM #AccionesCierre
         ORDER BY RevisionId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]');

DECLARE @HashContenido CHAR(64) =
    CONVERT(CHAR(64), HASHBYTES('SHA2_256', @Contenido), 2);
DECLARE @TotalCampos INT = (SELECT COUNT(*) FROM #CamposCierre);
DECLARE @TotalAcciones INT = (SELECT COUNT(*) FROM #AccionesCierre);
DECLARE @ResumenJson NVARCHAR(MAX) =
(
    SELECT @TotalCampos AS cambiosCampo,
           @TotalAcciones AS accionesClave,
           (SELECT COUNT(*) FROM #CamposCierre WHERE DecisionCampo = N'Aplicar') AS aplicar,
           (SELECT COUNT(*) FROM #CamposCierre WHERE DecisionCampo = N'Mantener SQL') AS mantenerSql,
           (SELECT COUNT(*) FROM #CamposCierre WHERE DecisionCampo = N'Omitir') AS omitir,
           (SELECT COUNT(*) FROM #AccionesCierre WHERE TipoAccion = N'Alta vigente') AS altas,
           (SELECT COUNT(*) FROM #AccionesCierre WHERE TipoAccion = N'Vincular clave') AS vinculos,
           (SELECT COUNT(*) FROM #AccionesCierre WHERE TipoAccion = N'Antecedente cancelado') AS antecedentesCancelados
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
);

DECLARE @PaqueteAplicacionId BIGINT;
DECLARE @Existente BIT = 0;

SELECT @PaqueteAplicacionId = p.PaqueteAplicacionId
FROM dgmesnie.PAMAplicacionPaquete p WITH (UPDLOCK, HOLDLOCK)
WHERE p.AnalisisId = @AnalisisId AND p.HashContenido = @HashContenido;

IF @PaqueteAplicacionId IS NULL
BEGIN
    INSERT dgmesnie.PAMAplicacionPaquete
    (
        LoteId, AnalisisId, HashContenido,
        TotalDetalles, TotalCambiosCampo, TotalAccionesClave,
        ResumenJson, UsuarioCreacionId, UsuarioCreacion
    )
    VALUES
    (
        @LoteId, @AnalisisId, @HashContenido,
        @TotalCampos + @TotalAcciones, @TotalCampos, @TotalAcciones,
        @ResumenJson, @UsuarioId, @UsuarioNombre
    );

    SET @PaqueteAplicacionId = SCOPE_IDENTITY();

    INSERT dgmesnie.PAMAplicacionPaqueteDetalle
    (
        PaqueteAplicacionId, TipoElemento, ReferenciaId, Orden, Accion,
        ProyectoId, ProyectoVersionBaseId, ProyectoRelacionadoId,
        ClaveProyecto, NombreProyecto, Campo,
        ValorAnteriorJson, ValorNuevoJson,
        FuenteDocumento, UbicacionFuente, HashDetalle
    )
    SELECT @PaqueteAplicacionId, N'CambioCampo', campo.CambioPropuestoId,
           ROW_NUMBER() OVER (ORDER BY campo.CambioPropuestoId), campo.DecisionCampo,
           campo.ProyectoId, campo.ProyectoVersionBaseId, NULL,
           campo.ClaveProyecto, campo.NombreProyecto, campo.Campo,
           campo.ValorActualJson, campo.ValorPropuestoJson,
           campo.FuenteDocumento, campo.UbicacionFuente,
           CONVERT(CHAR(64), HASHBYTES('SHA2_256',
               CONCAT(CONVERT(NVARCHAR(MAX), N'CambioCampo|'), campo.CambioPropuestoId, N'|',
                      campo.CambioDecisionId, N'|', campo.DecisionCampo, N'|',
                      COALESCE(campo.ValorActualJson, N''), N'|', campo.ValorPropuestoJson)), 2)
    FROM #CamposCierre campo;

    INSERT dgmesnie.PAMAplicacionPaqueteDetalle
    (
        PaqueteAplicacionId, TipoElemento, ReferenciaId, Orden, Accion,
        ProyectoId, ProyectoVersionBaseId, ProyectoRelacionadoId,
        ClaveProyecto, NombreProyecto, Campo,
        ValorAnteriorJson, ValorNuevoJson,
        FuenteDocumento, UbicacionFuente, HashDetalle
    )
    SELECT @PaqueteAplicacionId, N'AccionClave', accion.RevisionId,
           @TotalCampos + ROW_NUMBER() OVER (ORDER BY accion.RevisionId), accion.TipoAccion,
           NULL, NULL, accion.ProyectoRelacionadoId,
           accion.ClaveRecibida, accion.NombreRecibido, NULL,
           NULL, NULL,
           accion.FuenteDocumento, accion.UbicacionFuente,
           CONVERT(CHAR(64), HASHBYTES('SHA2_256',
               CONCAT(CONVERT(NVARCHAR(MAX), N'AccionClave|'), accion.RevisionId, N'|',
                      accion.VersionDecision, N'|', accion.TipoAccion, N'|',
                      COALESCE(CONVERT(NVARCHAR(30), accion.ProyectoRelacionadoId), N''))), 2)
    FROM #AccionesCierre accion;

    INSERT dgmesnie.PAMAplicacionPaqueteEvento
    (
        PaqueteAplicacionId, TipoEvento, Nota, DatosJson,
        UsuarioEventoId, UsuarioEvento
    )
    VALUES
    (
        @PaqueteAplicacionId, N'Congelado', @Nota, @ResumenJson,
        @UsuarioId, @UsuarioNombre
    );
END
ELSE
BEGIN
    SET @Existente = 1;
END;

SELECT p.PaqueteAplicacionId, p.PaqueteUid, p.HashContenido,
       @Existente AS Existente, p.TotalDetalles
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.PaqueteAplicacionId = @PaqueteAplicacionId;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var result = await connection.QuerySingleAsync<PamPaqueteCierreResultado>(new CommandDefinition(
                    sql,
                    new
                    {
                        LoteId = loteId,
                        UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                        UsuarioNombre = nombreUsuario,
                        Nota = nota
                    },
                    transaction,
                    cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (SqlException ex) when (ex.Number is >= 51040 and <= 51045)
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw new PamPaqueteCierreException(ex.Message);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        public async Task<PamAplicacionResultadoFinal> AplicarPaqueteAsync(
            long loteId,
            PamAplicarPaqueteInput input,
            int usuarioId,
            string usuarioNombre,
            CancellationToken cancellationToken)
        {
            if (loteId <= 0) throw new ArgumentOutOfRangeException(nameof(loteId));
            ArgumentNullException.ThrowIfNull(input);
            if (input.PaqueteAplicacionId <= 0)
                throw new PamAplicacionException("No fue posible identificar el paquete de cierre.");
            if (!input.Confirmado)
                throw new PamAplicacionException("Debes confirmar expresamente la aplicación definitiva.");

            var nombreUsuario = Limitar(usuarioNombre, 150) ?? $"Usuario {usuarioId}";
            var confirmacion = Limitar(input.Confirmacion, 80)?.Trim();

            const string sql = @"
SET NOCOUNT ON;

DECLARE @PaqueteUid UNIQUEIDENTIFIER;
DECLARE @HashContenido CHAR(64);
DECLARE @AnalisisId BIGINT;
DECLARE @TotalDetalles INT;

SELECT @PaqueteUid = p.PaqueteUid,
       @HashContenido = p.HashContenido,
       @AnalisisId = p.AnalisisId,
       @TotalDetalles = p.TotalDetalles
FROM dgmesnie.PAMAplicacionPaquete p WITH (UPDLOCK, HOLDLOCK)
WHERE p.PaqueteAplicacionId = @PaqueteAplicacionId
  AND p.LoteId = @LoteId;

IF @PaqueteUid IS NULL
    THROW 51060, N'El paquete de cierre no existe o no pertenece al lote.', 1;

DECLARE @FraseEsperada NVARCHAR(80) =
    CONCAT(N'APLICAR ', UPPER(LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)));

IF UPPER(LTRIM(RTRIM(COALESCE(@Confirmacion, N'')))) <> @FraseEsperada
    THROW 51061, N'La frase de confirmación no coincide con el paquete.', 1;

DECLARE @EstadoActual NVARCHAR(20) =
(
    SELECT TOP (1) e.TipoEvento
    FROM dgmesnie.PAMAplicacionPaqueteEvento e WITH (UPDLOCK, HOLDLOCK)
    WHERE e.PaqueteAplicacionId = @PaqueteAplicacionId
    ORDER BY e.PaqueteEventoId DESC
);

IF @EstadoActual = N'Aplicado'
BEGIN
    IF @TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionResultado ar WHERE ar.PaqueteAplicacionId = @PaqueteAplicacionId)
        THROW 51070, N'El paquete figura como aplicado, pero su bitácora de resultados está incompleta.', 1;

    UPDATE dgmesnie.PAMLoteActualizacion
    SET Estado = N'Aplicado',
        FechaActualizacionUtc = SYSUTCDATETIME()
    WHERE LoteId = @LoteId
      AND Estado <> N'Aplicado';

    SELECT @PaqueteAplicacionId AS PaqueteAplicacionId,
           @PaqueteUid AS PaqueteUid,
           CAST(1 AS BIT) AS Existente,
           COUNT(DISTINCT CASE WHEN ar.ProyectoVersionNuevaId IS NOT NULL THEN ar.ProyectoVersionNuevaId END) AS TotalVersionesNuevas,
           COUNT(DISTINCT CASE WHEN ar.TipoResultado IN (N'Proyecto creado', N'Antecedente creado') THEN ar.ProyectoId END) AS TotalProyectosNuevos,
           COUNT(CASE WHEN ar.TipoResultado = N'Clave vinculada' THEN 1 END) AS TotalClavesAlternas,
           COUNT(CASE WHEN ar.CambioId IS NOT NULL THEN 1 END) AS TotalCambios,
           COUNT(*) AS TotalResultados
    FROM dgmesnie.PAMAplicacionResultado ar
    WHERE ar.PaqueteAplicacionId = @PaqueteAplicacionId;
    RETURN;
END;

IF @EstadoActual <> N'Congelado'
    THROW 51062, N'El paquete no se encuentra en estado Congelado.', 1;

IF @TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d WITH (UPDLOCK, HOLDLOCK) WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId)
    THROW 51063, N'El paquete está incompleto y no puede aplicarse.', 1;

DECLARE @ContenidoActual NVARCHAR(MAX) =
    COALESCE(
        (SELECT d.ReferenciaId AS CambioPropuestoId,
                decisionActual.CambioDecisionId,
                d.Accion AS DecisionCampo
         FROM dgmesnie.PAMAplicacionPaqueteDetalle d
         OUTER APPLY
         (
             SELECT TOP (1) cd.CambioDecisionId
             FROM dgmesnie.PAMCambioDecisionHistorial cd WITH (UPDLOCK, HOLDLOCK)
             WHERE cd.CambioPropuestoId = d.ReferenciaId
             ORDER BY cd.CambioDecisionId DESC
         ) decisionActual
         WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
           AND d.TipoElemento = N'CambioCampo'
         ORDER BY d.ReferenciaId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]')
    + N'|'
    + COALESCE(
        (SELECT d.ReferenciaId AS RevisionId,
                revision.Estado AS EstadoRevision,
                revision.ClasificacionPreliminar,
                d.Accion AS TipoAccion,
                d.ProyectoRelacionadoId
         FROM dgmesnie.PAMAplicacionPaqueteDetalle d
         INNER JOIN dgmesnie.PAMRevisionPendiente revision WITH (UPDLOCK, HOLDLOCK)
           ON revision.RevisionId = d.ReferenciaId
         WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
           AND d.TipoElemento = N'AccionClave'
         ORDER BY d.ReferenciaId
         FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]');

IF CONVERT(CHAR(64), HASHBYTES('SHA2_256', @ContenidoActual), 2) <> @HashContenido
    THROW 51064, N'Las decisiones cambiaron después del congelado. Genere un paquete nuevo.', 1;

IF EXISTS
(
    SELECT 1
    FROM dgmesnie.PAMAplicacionPaqueteDetalle d
    OUTER APPLY
    (
        SELECT TOP (1) cd.CambioDecisionId
        FROM dgmesnie.PAMCambioDecisionHistorial cd WITH (UPDLOCK, HOLDLOCK)
        WHERE cd.CambioPropuestoId = d.ReferenciaId
        ORDER BY cd.CambioDecisionId DESC
    ) decisionCampo
    LEFT JOIN dgmesnie.PAMRevisionPendiente revision WITH (UPDLOCK, HOLDLOCK)
      ON d.TipoElemento = N'AccionClave' AND revision.RevisionId = d.ReferenciaId
    WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
      AND d.HashDetalle <>
          CASE d.TipoElemento
              WHEN N'CambioCampo' THEN CONVERT(CHAR(64), HASHBYTES('SHA2_256',
                  CONCAT(CONVERT(NVARCHAR(MAX), N'CambioCampo|'), d.ReferenciaId, N'|',
                         decisionCampo.CambioDecisionId, N'|', d.Accion, N'|',
                         COALESCE(d.ValorAnteriorJson, N''), N'|', d.ValorNuevoJson)), 2)
              ELSE CONVERT(CHAR(64), HASHBYTES('SHA2_256',
                  CONCAT(CONVERT(NVARCHAR(MAX), N'AccionClave|'), d.ReferenciaId, N'|',
                         sys.fn_varbintohexstr(revision.VersionDecision), N'|', d.Accion, N'|',
                         COALESCE(CONVERT(NVARCHAR(30), d.ProyectoRelacionadoId), N''))), 2)
          END
)
    THROW 51065, N'Falló la verificación de integridad de los elementos congelados.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMAplicacionPaqueteDetalle d
    WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
      AND d.TipoElemento = N'CambioCampo'
      AND d.Accion = N'Aplicar'
      AND (d.Campo NOT IN (N'NombreProyecto', N'GRT', N'EtapaProyecto', N'FechaNecesaria',
                           N'FeoFactible', N'MontoProyectoMdp', N'EstadoRealProyecto', N'AnioInstruccion')
           OR NOT EXISTS
              (SELECT 1 FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
               WHERE v.ProyectoVersionId = d.ProyectoVersionBaseId AND v.EsVersionVigente = 1)
           OR (d.Campo = N'MontoProyectoMdp' AND TRY_CONVERT(DECIMAL(18,3), JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)
           OR (d.Campo = N'AnioInstruccion' AND TRY_CONVERT(INT, JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL))
)
    THROW 51066, N'Existen campos no aplicables, valores inválidos o deriva en la versión base.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMAplicacionPaqueteDetalle d
    WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId AND d.Accion = N'Vincular clave'
      AND (d.ProyectoRelacionadoId IS NULL
           OR NOT EXISTS
              (SELECT 1 FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
               WHERE v.ProyectoId = d.ProyectoRelacionadoId AND v.EsVersionVigente = 1))
)
    THROW 51067, N'Existen claves alternas sin proyecto destino vigente.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMAplicacionPaqueteDetalle d
    WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
      AND d.Accion IN (N'Vincular clave', N'Alta vigente', N'Antecedente cancelado')
      AND
      (
          EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
                  WHERE v.EsVersionVigente = 1 AND LTRIM(RTRIM(v.ClaveProyecto)) = LTRIM(RTRIM(d.ClaveProyecto))
                    AND (d.ProyectoRelacionadoId IS NULL OR v.ProyectoId <> d.ProyectoRelacionadoId))
          OR EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoClaveVersion clave WITH (UPDLOCK, HOLDLOCK)
                     WHERE clave.EsVigente = 1 AND LTRIM(RTRIM(clave.ClaveProyecto)) = LTRIM(RTRIM(d.ClaveProyecto))
                       AND (d.ProyectoRelacionadoId IS NULL OR clave.ProyectoId <> d.ProyectoRelacionadoId))
      )
)
    THROW 51068, N'Una clave del paquete ya pertenece a otro proyecto.', 1;

CREATE TABLE #DetalleFuente
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    LoteFuenteId BIGINT NOT NULL,
    FuenteId BIGINT NOT NULL
);

INSERT #DetalleFuente (PaqueteDetalleId, LoteFuenteId, FuenteId)
SELECT d.PaqueteDetalleId, lf.LoteFuenteId, lf.FuenteId
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
LEFT JOIN dgmesnie.PAMCambioPropuesto cp
  ON d.TipoElemento = N'CambioCampo' AND cp.CambioPropuestoId = d.ReferenciaId
LEFT JOIN dgmesnie.PAMRevisionPendiente revision
  ON d.TipoElemento = N'AccionClave' AND revision.RevisionId = d.ReferenciaId
INNER JOIN dgmesnie.PAMHallazgoProyecto hallazgo
  ON hallazgo.HallazgoId = COALESCE(cp.HallazgoId, revision.HallazgoId)
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = hallazgo.LoteFuenteId
WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId;

IF (SELECT COUNT(*) FROM #DetalleFuente) <> @TotalDetalles
    THROW 51069, N'No fue posible resolver la fuente de todos los elementos.', 1;

CREATE TABLE #FuenteCarga
(
    FuenteId BIGINT NOT NULL PRIMARY KEY,
    LoteFuenteId BIGINT NOT NULL,
    CargaId BIGINT NOT NULL
);

MERGE dgmesnie.PAMCarga AS destino
USING
(
    SELECT df.FuenteId, MIN(df.LoteFuenteId) AS LoteFuenteId
    FROM #DetalleFuente df
    GROUP BY df.FuenteId
) AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (FuenteId, TipoCarga, EstadoCarga, UsuarioCarga, MensajeResultado)
    VALUES (fuente.FuenteId, N'Aplicación paquete PAM', N'Preliminar', @UsuarioNombre,
            CONCAT(N'Aplicación transaccional del paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)))
OUTPUT fuente.FuenteId, fuente.LoteFuenteId, inserted.CargaId
INTO #FuenteCarga (FuenteId, LoteFuenteId, CargaId);

DECLARE @FechaAplicacion DATE = CONVERT(DATE, SYSUTCDATETIME());

SELECT d.PaqueteDetalleId, d.ReferenciaId AS CambioPropuestoId,
       COALESCE(d.ProyectoId, versionBase.ProyectoId) AS ProyectoId,
       d.ProyectoVersionBaseId, d.Campo,
       d.ValorAnteriorJson, d.ValorNuevoJson,
       JSON_VALUE(d.ValorNuevoJson, '$.value') AS ValorNuevo,
       df.FuenteId, fc.CargaId
INTO #CambiosAplicar
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
INNER JOIN dgmesnie.PAMProyectoVersion versionBase
  ON versionBase.ProyectoVersionId = d.ProyectoVersionBaseId AND versionBase.EsVersionVigente = 1
INNER JOIN #DetalleFuente df ON df.PaqueteDetalleId = d.PaqueteDetalleId
INNER JOIN #FuenteCarga fc ON fc.FuenteId = df.FuenteId
WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
  AND d.TipoElemento = N'CambioCampo' AND d.Accion = N'Aplicar';

SELECT cambios.ProyectoVersionBaseId,
       MAX(CASE WHEN cambios.Campo = N'NombreProyecto' THEN cambios.ValorNuevo END) AS NombreProyecto,
       MAX(CASE WHEN cambios.Campo = N'NombreProyecto' THEN 1 ELSE 0 END) AS TieneNombreProyecto,
       MAX(CASE WHEN cambios.Campo = N'GRT' THEN cambios.ValorNuevo END) AS GRT,
       MAX(CASE WHEN cambios.Campo = N'GRT' THEN 1 ELSE 0 END) AS TieneGRT,
       MAX(CASE WHEN cambios.Campo = N'EtapaProyecto' THEN cambios.ValorNuevo END) AS EtapaProyecto,
       MAX(CASE WHEN cambios.Campo = N'EtapaProyecto' THEN 1 ELSE 0 END) AS TieneEtapaProyecto,
       MAX(CASE WHEN cambios.Campo = N'FechaNecesaria' THEN cambios.ValorNuevo END) AS FechaNecesaria,
       MAX(CASE WHEN cambios.Campo = N'FechaNecesaria' THEN 1 ELSE 0 END) AS TieneFechaNecesaria,
       MAX(CASE WHEN cambios.Campo = N'FeoFactible' THEN cambios.ValorNuevo END) AS FeoFactible,
       MAX(CASE WHEN cambios.Campo = N'FeoFactible' THEN 1 ELSE 0 END) AS TieneFeoFactible,
       MAX(CASE WHEN cambios.Campo = N'MontoProyectoMdp' THEN cambios.ValorNuevo END) AS MontoProyectoMdp,
       MAX(CASE WHEN cambios.Campo = N'MontoProyectoMdp' THEN 1 ELSE 0 END) AS TieneMontoProyectoMdp,
       MAX(CASE WHEN cambios.Campo = N'EstadoRealProyecto' THEN cambios.ValorNuevo END) AS EstadoRealProyecto,
       MAX(CASE WHEN cambios.Campo = N'EstadoRealProyecto' THEN 1 ELSE 0 END) AS TieneEstadoRealProyecto,
       MAX(CASE WHEN cambios.Campo = N'AnioInstruccion' THEN cambios.ValorNuevo END) AS AnioInstruccion,
       MAX(CASE WHEN cambios.Campo = N'AnioInstruccion' THEN 1 ELSE 0 END) AS TieneAnioInstruccion,
       MIN(cambios.FuenteId) AS FuenteId,
       MIN(cambios.CargaId) AS CargaId
INTO #CambiosPivote
FROM #CambiosAplicar cambios
GROUP BY cambios.ProyectoVersionBaseId;

SELECT versionBase.*, pivote.FuenteId AS NuevaFuenteId, pivote.CargaId AS NuevaCargaId,
       pivote.NombreProyecto AS NuevoNombreProyecto, pivote.TieneNombreProyecto,
       pivote.GRT AS NuevoGRT, pivote.TieneGRT,
       pivote.EtapaProyecto AS NuevaEtapaProyecto, pivote.TieneEtapaProyecto,
       pivote.FechaNecesaria AS NuevaFechaNecesaria, pivote.TieneFechaNecesaria,
       pivote.FeoFactible AS NuevoFeoFactible, pivote.TieneFeoFactible,
       pivote.MontoProyectoMdp AS NuevoMontoProyectoMdp, pivote.TieneMontoProyectoMdp,
       pivote.EstadoRealProyecto AS NuevoEstadoRealProyecto, pivote.TieneEstadoRealProyecto,
       pivote.AnioInstruccion AS NuevoAnioInstruccion, pivote.TieneAnioInstruccion
INTO #VersionesBase
FROM dgmesnie.PAMProyectoVersion versionBase WITH (UPDLOCK, HOLDLOCK)
INNER JOIN #CambiosPivote pivote ON pivote.ProyectoVersionBaseId = versionBase.ProyectoVersionId
WHERE versionBase.EsVersionVigente = 1;

UPDATE versionActual
SET EsVersionVigente = 0,
    VigenteHasta = CASE
        WHEN versionActual.VigenteDesde < @FechaAplicacion THEN DATEADD(DAY, -1, @FechaAplicacion)
        ELSE versionActual.VigenteDesde
    END
FROM dgmesnie.PAMProyectoVersion versionActual
INNER JOIN #VersionesBase base ON base.ProyectoVersionId = versionActual.ProyectoVersionId;

CREATE TABLE #NuevasVersiones
(
    ProyectoVersionAnteriorId BIGINT NOT NULL PRIMARY KEY,
    ProyectoId BIGINT NOT NULL,
    ProyectoVersionNuevaId BIGINT NOT NULL
);

MERGE dgmesnie.PAMProyectoVersion AS destino
USING #VersionesBase AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT
    (
        ProyectoId, NumeroVersion, FuenteId, CargaId, VigenteDesde, VigenteHasta,
        EsVersionVigente, OrigenPrograma, Programa, AnioPrograma, TipoProyecto,
        EstadoVigenciaCartera, MotivoCambio, ClaveProyecto, Numero, NumeroOriginal,
        GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion, EtapaProyecto,
        MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio,
        FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion,
        CircunstanciasAtrasos, AccionesMitigacionCorreccion, EstadoRealProyecto,
        ComentariosNivelPriorizacion, ClasificacionSener, FechaProgramacionTrimestre,
        QuincenaPublicacion, UniversoPresentacionPresidencia, Mva, Mvar, KmC,
        ActivoEnFuente, HashContenido, UsuarioRegistro, PrioridadPrograma,
        ZonaAtendida, FechaNecesaria
    )
    VALUES
    (
        fuente.ProyectoId, fuente.NumeroVersion + 1, fuente.NuevaFuenteId, fuente.NuevaCargaId,
        CASE WHEN fuente.VigenteDesde > @FechaAplicacion THEN fuente.VigenteDesde ELSE @FechaAplicacion END, NULL,
        1, fuente.OrigenPrograma, fuente.Programa, fuente.AnioPrograma, fuente.TipoProyecto,
        fuente.EstadoVigenciaCartera,
        LEFT(CONCAT(N'Aplicación del paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8), N'; versión anterior ', fuente.ProyectoVersionId), 500),
        fuente.ClaveProyecto, fuente.Numero, fuente.NumeroOriginal,
        CASE WHEN fuente.TieneGRT = 1 THEN fuente.NuevoGRT ELSE fuente.GRT END,
        CASE WHEN fuente.TieneNombreProyecto = 1 THEN fuente.NuevoNombreProyecto ELSE fuente.NombreProyecto END,
        fuente.TipoFinanciamiento,
        CASE WHEN fuente.TieneAnioInstruccion = 1 THEN TRY_CONVERT(INT, fuente.NuevoAnioInstruccion) ELSE fuente.AnioInstruccion END,
        CASE WHEN fuente.TieneEtapaProyecto = 1 THEN fuente.NuevaEtapaProyecto ELSE fuente.EtapaProyecto END,
        CASE WHEN fuente.TieneMontoProyectoMdp = 1 THEN TRY_CONVERT(DECIMAL(18,3), fuente.NuevoMontoProyectoMdp) ELSE fuente.MontoProyectoMdp END,
        fuente.ElementosEquiposAsociados, fuente.FechaEstimadaInicio,
        fuente.FeoIndicadaOficioSener,
        CASE WHEN fuente.TieneFeoFactible = 1 THEN fuente.NuevoFeoFactible ELSE fuente.FeoFactible END,
        fuente.PorcentajeAvanceEjecucion, fuente.CircunstanciasAtrasos,
        fuente.AccionesMitigacionCorreccion,
        CASE WHEN fuente.TieneEstadoRealProyecto = 1 THEN fuente.NuevoEstadoRealProyecto ELSE fuente.EstadoRealProyecto END,
        fuente.ComentariosNivelPriorizacion, fuente.ClasificacionSener,
        fuente.FechaProgramacionTrimestre, fuente.QuincenaPublicacion,
        fuente.UniversoPresentacionPresidencia, fuente.Mva, fuente.Mvar, fuente.KmC,
        1,
        CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(@HashContenido, N'|', fuente.ProyectoId, N'|', fuente.NumeroVersion + 1)), 2),
        @UsuarioNombre, fuente.PrioridadPrograma, fuente.ZonaAtendida,
        CASE WHEN fuente.TieneFechaNecesaria = 1 THEN fuente.NuevaFechaNecesaria ELSE fuente.FechaNecesaria END
    )
OUTPUT fuente.ProyectoVersionId, fuente.ProyectoId, inserted.ProyectoVersionId
INTO #NuevasVersiones (ProyectoVersionAnteriorId, ProyectoId, ProyectoVersionNuevaId);

CREATE TABLE #CambiosInsertados
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    CambioId BIGINT NOT NULL
);

MERGE dgmesnie.PAMCambio AS destino
USING
(
    SELECT cambio.PaqueteDetalleId, cambio.CargaId, cambio.ProyectoId,
           cambio.ProyectoVersionBaseId, nueva.ProyectoVersionNuevaId,
           cambio.Campo, cambio.ValorAnteriorJson, cambio.ValorNuevoJson
    FROM #CambiosAplicar cambio
    INNER JOIN #NuevasVersiones nueva
      ON nueva.ProyectoVersionAnteriorId = cambio.ProyectoVersionBaseId
) AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CargaId, ProyectoId, ProyectoVersionAnteriorId, ProyectoVersionNuevaId,
            TipoCambio, Campo, ValorAnterior, ValorNuevo, UsuarioRegistro)
    VALUES (fuente.CargaId, fuente.ProyectoId, fuente.ProyectoVersionBaseId, fuente.ProyectoVersionNuevaId,
            N'Modificacion', fuente.Campo, fuente.ValorAnteriorJson, fuente.ValorNuevoJson, @UsuarioNombre)
OUTPUT fuente.PaqueteDetalleId, inserted.CambioId
INTO #CambiosInsertados (PaqueteDetalleId, CambioId);

INSERT dgmesnie.PAMProyectoVersionFuente
    (ProyectoVersionId, FuenteId, Papel, CampoRespaldado, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
SELECT nueva.ProyectoVersionNuevaId, cambio.FuenteId, N'Primaria', cambio.Campo,
       MIN(detalle.UbicacionFuente),
       CONCAT(N'Aplicado desde paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)), @UsuarioNombre
FROM #CambiosAplicar cambio
INNER JOIN #NuevasVersiones nueva ON nueva.ProyectoVersionAnteriorId = cambio.ProyectoVersionBaseId
INNER JOIN dgmesnie.PAMAplicacionPaqueteDetalle detalle ON detalle.PaqueteDetalleId = cambio.PaqueteDetalleId
GROUP BY nueva.ProyectoVersionNuevaId, cambio.FuenteId, cambio.Campo;

INSERT dgmesnie.PAMCambioFuente
    (CambioId, FuenteId, Papel, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
SELECT insertado.CambioId, cambio.FuenteId, N'Primaria', detalle.UbicacionFuente,
       CONCAT(N'Aplicado desde paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)), @UsuarioNombre
FROM #CambiosInsertados insertado
INNER JOIN #CambiosAplicar cambio ON cambio.PaqueteDetalleId = insertado.PaqueteDetalleId
INNER JOIN dgmesnie.PAMAplicacionPaqueteDetalle detalle ON detalle.PaqueteDetalleId = cambio.PaqueteDetalleId;

INSERT dgmesnie.PAMAplicacionResultado
    (PaqueteAplicacionId, PaqueteDetalleId, TipoResultado, ProyectoId,
     ProyectoVersionAnteriorId, ProyectoVersionNuevaId, CambioId,
     UsuarioAplicacionId, UsuarioAplicacion)
SELECT @PaqueteAplicacionId, cambio.PaqueteDetalleId, N'Cambio aplicado', cambio.ProyectoId,
       cambio.ProyectoVersionBaseId, nueva.ProyectoVersionNuevaId, insertado.CambioId,
       @UsuarioId, @UsuarioNombre
FROM #CambiosAplicar cambio
INNER JOIN #NuevasVersiones nueva ON nueva.ProyectoVersionAnteriorId = cambio.ProyectoVersionBaseId
INNER JOIN #CambiosInsertados insertado ON insertado.PaqueteDetalleId = cambio.PaqueteDetalleId;

SELECT d.PaqueteDetalleId, d.ClaveProyecto, d.ProyectoRelacionadoId AS ProyectoId,
       df.FuenteId, fc.CargaId
INTO #ClavesAlternas
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
INNER JOIN #DetalleFuente df ON df.PaqueteDetalleId = d.PaqueteDetalleId
INNER JOIN #FuenteCarga fc ON fc.FuenteId = df.FuenteId
WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId AND d.Accion = N'Vincular clave';

IF EXISTS
(
    SELECT 1
    FROM #ClavesAlternas clave
    WHERE NULLIF(LTRIM(RTRIM(clave.ClaveProyecto)), N'') IS NOT NULL
    GROUP BY LTRIM(RTRIM(clave.ClaveProyecto))
    HAVING COUNT(DISTINCT clave.ProyectoId) > 1
)
    THROW 51071, N'Una clave alterna apunta a varios proyectos dentro del paquete.', 1;

CREATE TABLE #ClavesAlternasUnicas
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    ClaveProyecto NVARCHAR(200) NOT NULL,
    ProyectoId BIGINT NOT NULL,
    FuenteId BIGINT NOT NULL,
    CargaId BIGINT NOT NULL
);

INSERT #ClavesAlternasUnicas (PaqueteDetalleId, ClaveProyecto, ProyectoId, FuenteId, CargaId)
SELECT MIN(clave.PaqueteDetalleId), LTRIM(RTRIM(clave.ClaveProyecto)), clave.ProyectoId,
       MIN(clave.FuenteId), MIN(clave.CargaId)
FROM #ClavesAlternas clave
GROUP BY clave.ProyectoId, LTRIM(RTRIM(clave.ClaveProyecto));

CREATE TABLE #ClavesInsertadas
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    ProyectoId BIGINT NOT NULL,
    ProyectoClaveVersionId BIGINT NOT NULL
);

MERGE dgmesnie.PAMProyectoClaveVersion AS destino
USING #ClavesAlternasUnicas AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (ProyectoId, ClaveProyecto, TipoClave, FuenteId, CargaId,
            PaqueteDetalleId, VigenteDesde, VigenteHasta, EsVigente, UsuarioRegistro)
    VALUES (fuente.ProyectoId, LTRIM(RTRIM(fuente.ClaveProyecto)), N'Alterna', fuente.FuenteId, fuente.CargaId,
            fuente.PaqueteDetalleId, @FechaAplicacion, NULL, 1, @UsuarioNombre)
OUTPUT fuente.PaqueteDetalleId, fuente.ProyectoId, inserted.ProyectoClaveVersionId
INTO #ClavesInsertadas (PaqueteDetalleId, ProyectoId, ProyectoClaveVersionId);

CREATE TABLE #ClavesInsertadasTodas
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    ProyectoId BIGINT NOT NULL,
    ProyectoClaveVersionId BIGINT NOT NULL
);

INSERT #ClavesInsertadasTodas (PaqueteDetalleId, ProyectoId, ProyectoClaveVersionId)
SELECT clave.PaqueteDetalleId, clave.ProyectoId, insertada.ProyectoClaveVersionId
FROM #ClavesAlternas clave
INNER JOIN #ClavesAlternasUnicas unica
    ON unica.PaqueteDetalleId =
       (
           SELECT MIN(otra.PaqueteDetalleId)
           FROM #ClavesAlternas otra
           WHERE otra.ProyectoId = clave.ProyectoId
             AND LTRIM(RTRIM(otra.ClaveProyecto)) = LTRIM(RTRIM(clave.ClaveProyecto))
       )
INNER JOIN #ClavesInsertadas insertada ON insertada.PaqueteDetalleId = unica.PaqueteDetalleId;

INSERT dgmesnie.PAMAplicacionResultado
    (PaqueteAplicacionId, PaqueteDetalleId, TipoResultado, ProyectoId,
     ProyectoClaveVersionId, UsuarioAplicacionId, UsuarioAplicacion)
SELECT @PaqueteAplicacionId, clave.PaqueteDetalleId, N'Clave vinculada', clave.ProyectoId,
       clave.ProyectoClaveVersionId, @UsuarioId, @UsuarioNombre
FROM #ClavesInsertadasTodas clave;

SELECT d.PaqueteDetalleId, d.Accion, d.ClaveProyecto, d.NombreProyecto,
       hallazgo.DatosExtraidosJson, df.FuenteId, fc.CargaId,
       valorGcr.Valor AS GRT,
       valorEstatus.Valor AS EtapaProyecto,
       valorFechaNecesaria.Valor AS FechaNecesaria,
       valorFeo.Valor AS FeoFactible,
       TRY_CONVERT(INT, LEFT(valorAnio.Valor, 4)) AS AnioInstruccion,
       TRY_CONVERT(DECIMAL(18,3), REPLACE(valorMonto.Valor, N',', N'')) AS MontoProyectoMdp,
       valorEstatusFinanciamiento.Valor AS EstadoRealProyecto
INTO #ProyectosNuevos
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
INNER JOIN dgmesnie.PAMRevisionPendiente revision ON revision.RevisionId = d.ReferenciaId
INNER JOIN dgmesnie.PAMHallazgoProyecto hallazgo ON hallazgo.HallazgoId = revision.HallazgoId
INNER JOIN #DetalleFuente df ON df.PaqueteDetalleId = d.PaqueteDetalleId
INNER JOIN #FuenteCarga fc ON fc.FuenteId = df.FuenteId
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] = N'GCR') valorGcr
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] LIKE N'Estatus%PAMRNT 2026') valorEstatus
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] = N'Fecha Necesaria') valorFechaNecesaria
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] = N'Fecha de entrada en operación') valorFeo
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] = N'Año de Instrucción') valorAnio
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] LIKE N'Monto de proyecto%') valorMonto
OUTER APPLY (SELECT TOP (1) CONVERT(NVARCHAR(MAX), j.value) AS Valor FROM OPENJSON(hallazgo.DatosExtraidosJson) j WHERE j.[key] LIKE N'Estatus%CFE - Financiamiento%') valorEstatusFinanciamiento
WHERE d.PaqueteAplicacionId = @PaqueteAplicacionId
  AND d.Accion IN (N'Alta vigente', N'Antecedente cancelado');

IF EXISTS (SELECT 1 FROM #ProyectosNuevos WHERE NULLIF(LTRIM(RTRIM(ClaveProyecto)), N'') IS NULL OR NULLIF(LTRIM(RTRIM(NombreProyecto)), N'') IS NULL)
    THROW 51070, N'Faltan datos mínimos para crear uno o más proyectos.', 1;

CREATE TABLE #ProyectosInsertados
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    ProyectoId BIGINT NOT NULL
);

MERGE dgmesnie.PAMProyecto AS destino
USING #ProyectosNuevos AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (FechaAltaUtc, FechaBajaUtc, Activo)
    VALUES (SYSUTCDATETIME(),
            CASE WHEN fuente.Accion = N'Antecedente cancelado' THEN SYSUTCDATETIME() ELSE NULL END,
            CASE WHEN fuente.Accion = N'Antecedente cancelado' THEN 0 ELSE 1 END)
OUTPUT fuente.PaqueteDetalleId, inserted.ProyectoId
INTO #ProyectosInsertados (PaqueteDetalleId, ProyectoId);

CREATE TABLE #VersionesProyectosNuevos
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    ProyectoId BIGINT NOT NULL,
    ProyectoVersionNuevaId BIGINT NOT NULL
);

MERGE dgmesnie.PAMProyectoVersion AS destino
USING
(
    SELECT nuevo.*, proyecto.ProyectoId
    FROM #ProyectosNuevos nuevo
    INNER JOIN #ProyectosInsertados proyecto ON proyecto.PaqueteDetalleId = nuevo.PaqueteDetalleId
) AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT
    (
        ProyectoId, NumeroVersion, FuenteId, CargaId, VigenteDesde, VigenteHasta,
        EsVersionVigente, OrigenPrograma, Programa, AnioPrograma, TipoProyecto,
        EstadoVigenciaCartera, MotivoCambio, ClaveProyecto, GRT, NombreProyecto,
        TipoFinanciamiento, AnioInstruccion, EtapaProyecto, MontoProyectoMdp,
        FeoFactible, EstadoRealProyecto, ClasificacionSener, ActivoEnFuente,
        HashContenido, UsuarioRegistro, FechaNecesaria
    )
    VALUES
    (
        fuente.ProyectoId, 1, fuente.FuenteId, fuente.CargaId, @FechaAplicacion, NULL,
        1, N'PAM', N'PAM - actualización RNT 24-mar-2026', 2026, N'Transmisión',
        CASE WHEN fuente.Accion = N'Antecedente cancelado' THEN N'Cancelado' ELSE N'Vigente' END,
        LEFT(CONCAT(fuente.Accion, N' incorporado desde paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)), 500),
        fuente.ClaveProyecto, fuente.GRT, fuente.NombreProyecto,
        fuente.EstadoRealProyecto, fuente.AnioInstruccion, fuente.EtapaProyecto,
        fuente.MontoProyectoMdp, fuente.FeoFactible, fuente.EstadoRealProyecto,
        CASE WHEN fuente.Accion = N'Antecedente cancelado' THEN N'Antecedente cancelado' ELSE N'Alta vigente validada' END,
        CASE WHEN fuente.Accion = N'Antecedente cancelado' THEN 0 ELSE 1 END,
        CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(@HashContenido, N'|Nuevo|', fuente.PaqueteDetalleId, N'|', fuente.ClaveProyecto)), 2),
        @UsuarioNombre, fuente.FechaNecesaria
    )
OUTPUT fuente.PaqueteDetalleId, fuente.ProyectoId, inserted.ProyectoVersionId
INTO #VersionesProyectosNuevos (PaqueteDetalleId, ProyectoId, ProyectoVersionNuevaId);

CREATE TABLE #CambiosProyectosNuevos
(
    PaqueteDetalleId BIGINT NOT NULL PRIMARY KEY,
    CambioId BIGINT NOT NULL
);

MERGE dgmesnie.PAMCambio AS destino
USING
(
    SELECT nuevo.PaqueteDetalleId, nuevo.CargaId, version.ProyectoId,
           version.ProyectoVersionNuevaId, nuevo.DatosExtraidosJson
    FROM #ProyectosNuevos nuevo
    INNER JOIN #VersionesProyectosNuevos version ON version.PaqueteDetalleId = nuevo.PaqueteDetalleId
) AS fuente
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CargaId, ProyectoId, ProyectoVersionNuevaId, TipoCambio, Campo, ValorNuevo, UsuarioRegistro)
    VALUES (fuente.CargaId, fuente.ProyectoId, fuente.ProyectoVersionNuevaId,
            N'Alta', N'Registro completo', fuente.DatosExtraidosJson, @UsuarioNombre)
OUTPUT fuente.PaqueteDetalleId, inserted.CambioId
INTO #CambiosProyectosNuevos (PaqueteDetalleId, CambioId);

INSERT dgmesnie.PAMProyectoVersionFuente
    (ProyectoVersionId, FuenteId, Papel, CampoRespaldado, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
SELECT version.ProyectoVersionNuevaId, nuevo.FuenteId, N'Primaria', NULL,
       detalle.UbicacionFuente,
       CONCAT(N'Proyecto incorporado desde paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)), @UsuarioNombre
FROM #VersionesProyectosNuevos version
INNER JOIN #ProyectosNuevos nuevo ON nuevo.PaqueteDetalleId = version.PaqueteDetalleId
INNER JOIN dgmesnie.PAMAplicacionPaqueteDetalle detalle ON detalle.PaqueteDetalleId = version.PaqueteDetalleId;

INSERT dgmesnie.PAMCambioFuente
    (CambioId, FuenteId, Papel, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
SELECT cambio.CambioId, nuevo.FuenteId, N'Primaria', detalle.UbicacionFuente,
       CONCAT(N'Proyecto incorporado desde paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8)), @UsuarioNombre
FROM #CambiosProyectosNuevos cambio
INNER JOIN #ProyectosNuevos nuevo ON nuevo.PaqueteDetalleId = cambio.PaqueteDetalleId
INNER JOIN dgmesnie.PAMAplicacionPaqueteDetalle detalle ON detalle.PaqueteDetalleId = cambio.PaqueteDetalleId;

INSERT dgmesnie.PAMAplicacionResultado
    (PaqueteAplicacionId, PaqueteDetalleId, TipoResultado, ProyectoId,
     ProyectoVersionNuevaId, CambioId, UsuarioAplicacionId, UsuarioAplicacion)
SELECT @PaqueteAplicacionId, nuevo.PaqueteDetalleId,
       CASE WHEN nuevo.Accion = N'Antecedente cancelado' THEN N'Antecedente creado' ELSE N'Proyecto creado' END,
       version.ProyectoId, version.ProyectoVersionNuevaId, cambio.CambioId,
       @UsuarioId, @UsuarioNombre
FROM #ProyectosNuevos nuevo
INNER JOIN #VersionesProyectosNuevos version ON version.PaqueteDetalleId = nuevo.PaqueteDetalleId
INNER JOIN #CambiosProyectosNuevos cambio ON cambio.PaqueteDetalleId = nuevo.PaqueteDetalleId;

-- Las decisiones que conservan u omiten información no generan una nueva versión,
-- pero cada detalle congelado debe conservar su resultado trazable.
INSERT dgmesnie.PAMAplicacionResultado
    (PaqueteAplicacionId, PaqueteDetalleId, TipoResultado, ProyectoId,
     ProyectoVersionAnteriorId, UsuarioAplicacionId, UsuarioAplicacion)
SELECT @PaqueteAplicacionId, detalle.PaqueteDetalleId,
       CASE detalle.Accion
           WHEN N'Mantener SQL' THEN N'Cambio conservado'
           WHEN N'Omitir' THEN N'Cambio omitido'
           ELSE N'Acción descartada'
       END,
       COALESCE(detalle.ProyectoId, versionBase.ProyectoId, detalle.ProyectoRelacionadoId),
       CASE WHEN detalle.TipoElemento = N'CambioCampo' THEN detalle.ProyectoVersionBaseId END,
       @UsuarioId, @UsuarioNombre
FROM dgmesnie.PAMAplicacionPaqueteDetalle detalle
LEFT JOIN dgmesnie.PAMProyectoVersion versionBase
    ON versionBase.ProyectoVersionId = detalle.ProyectoVersionBaseId
WHERE detalle.PaqueteAplicacionId = @PaqueteAplicacionId
  AND detalle.Accion IN (N'Mantener SQL', N'Omitir', N'No aplicar')
  AND NOT EXISTS
      (SELECT 1 FROM dgmesnie.PAMAplicacionResultado resultado
       WHERE resultado.PaqueteDetalleId = detalle.PaqueteDetalleId);

UPDATE carga
SET EstadoCarga = N'Aplicada',
    FechaFinUtc = SYSUTCDATETIME(),
    RegistrosLeidos = conteo.RegistrosLeidos,
    RegistrosNuevos = conteo.RegistrosNuevos,
    RegistrosModificados = conteo.RegistrosModificados,
    RegistrosSinCambio = 0,
    RegistrosObservados = 0,
    MensajeResultado = CONCAT(N'Paquete ', LEFT(CONVERT(NVARCHAR(36), @PaqueteUid), 8),
                              N' aplicado con versionado e historial por campo.')
FROM dgmesnie.PAMCarga carga
INNER JOIN #FuenteCarga fc ON fc.CargaId = carga.CargaId
CROSS APPLY
(
    SELECT COUNT(*) AS RegistrosLeidos,
           COUNT(CASE WHEN detalle.Accion IN (N'Alta vigente', N'Antecedente cancelado') THEN 1 END) AS RegistrosNuevos,
           COUNT(DISTINCT CASE WHEN detalle.TipoElemento = N'CambioCampo' THEN detalle.ProyectoVersionBaseId END) AS RegistrosModificados
    FROM dgmesnie.PAMAplicacionPaqueteDetalle detalle
    INNER JOIN #DetalleFuente df ON df.PaqueteDetalleId = detalle.PaqueteDetalleId AND df.FuenteId = fc.FuenteId
    WHERE detalle.PaqueteAplicacionId = @PaqueteAplicacionId
) conteo;

UPDATE loteFuente
SET CargaId = fc.CargaId
FROM dgmesnie.PAMLoteFuente loteFuente
INNER JOIN #FuenteCarga fc ON fc.LoteFuenteId = loteFuente.LoteFuenteId;

DECLARE @ResumenAplicacion NVARCHAR(MAX) =
(
    SELECT
        (SELECT COUNT(*) FROM #NuevasVersiones) AS versionesActualizadas,
        (SELECT COUNT(*) FROM #VersionesProyectosNuevos) AS proyectosNuevos,
        (SELECT COUNT(*) FROM #ClavesInsertadasTodas) AS clavesAlternas,
        (SELECT COUNT(*) FROM #CambiosInsertados) + (SELECT COUNT(*) FROM #CambiosProyectosNuevos) AS cambiosRegistrados,
        (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionResultado ar WHERE ar.PaqueteAplicacionId = @PaqueteAplicacionId) AS resultados
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
);

IF @TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionResultado ar WHERE ar.PaqueteAplicacionId = @PaqueteAplicacionId)
    THROW 51070, N'La aplicación no produjo una bitácora completa de resultados.', 1;

INSERT dgmesnie.PAMAplicacionPaqueteEvento
    (PaqueteAplicacionId, TipoEvento, Nota, DatosJson, UsuarioEventoId, UsuarioEvento)
VALUES
    (@PaqueteAplicacionId, N'Aprobado', N'Confirmación final validada antes de la transacción.', @ResumenAplicacion, @UsuarioId, @UsuarioNombre),
    (@PaqueteAplicacionId, N'Aplicado', N'Cartera versionada y trazabilidad registrada.', @ResumenAplicacion, @UsuarioId, @UsuarioNombre);

UPDATE dgmesnie.PAMLoteActualizacion
SET Estado = N'Aplicado',
    FechaActualizacionUtc = SYSUTCDATETIME()
WHERE LoteId = @LoteId
  AND Estado <> N'Aplicado';

SELECT @PaqueteAplicacionId AS PaqueteAplicacionId,
       @PaqueteUid AS PaqueteUid,
       CAST(0 AS BIT) AS Existente,
       (SELECT COUNT(*) FROM #NuevasVersiones) + (SELECT COUNT(*) FROM #VersionesProyectosNuevos) AS TotalVersionesNuevas,
       (SELECT COUNT(*) FROM #VersionesProyectosNuevos) AS TotalProyectosNuevos,
       (SELECT COUNT(*) FROM #ClavesInsertadasTodas) AS TotalClavesAlternas,
       (SELECT COUNT(*) FROM #CambiosInsertados) + (SELECT COUNT(*) FROM #CambiosProyectosNuevos) AS TotalCambios,
       (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionResultado ar WHERE ar.PaqueteAplicacionId = @PaqueteAplicacionId) AS TotalResultados;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var result = await connection.QuerySingleAsync<PamAplicacionResultadoFinal>(new CommandDefinition(
                    sql,
                    new
                    {
                        LoteId = loteId,
                        input.PaqueteAplicacionId,
                        Confirmacion = confirmacion,
                        UsuarioId = usuarioId > 0 ? usuarioId : (int?)null,
                        UsuarioNombre = nombreUsuario
                    },
                    transaction,
                    commandTimeout: 180,
                    cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (SqlException ex) when (ex.Number is >= 51060 and <= 51079)
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw new PamAplicacionException(ex.Message);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        private async Task<AnalisisContext> CargarContextoAsync(long analisisId, Guid leaseUid, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT AnalisisId, LoteId, UsuarioId, UsuarioNombre
FROM dgmesnie.PAMAnalisisEjecucion
WHERE AnalisisId = @AnalisisId AND LeaseUid = @LeaseUid AND Estado = N'Ejecutando';

SELECT lf.LoteFuenteId, lf.FuenteId, lf.NombreOriginal, lf.Extension,
       f.RutaArchivo, f.HashSha256
FROM dgmesnie.PAMAnalisisEjecucion e
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteId = e.LoteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
WHERE e.AnalisisId = @AnalisisId
ORDER BY lf.LoteFuenteId;

SELECT p.ProyectoId, v.ProyectoVersionId, v.ClaveProyecto, v.NombreProyecto,
       v.GRT, v.EtapaProyecto, v.EstadoVigenciaCartera, v.MontoProyectoMdp,
       v.FechaNecesaria, v.AnioInstruccion, v.EstadoRealProyecto, v.FeoFactible
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1

UNION ALL

SELECT p.ProyectoId, v.ProyectoVersionId, clave.ClaveProyecto, v.NombreProyecto,
       v.GRT, v.EtapaProyecto, v.EstadoVigenciaCartera, v.MontoProyectoMdp,
       v.FechaNecesaria, v.AnioInstruccion, v.EstadoRealProyecto, v.FeoFactible
FROM dgmesnie.PAMProyectoClaveVersion clave
INNER JOIN dgmesnie.PAMProyecto p ON p.ProyectoId = clave.ProyectoId
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
WHERE clave.EsVigente = 1;";
            await using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new
            {
                AnalisisId = analisisId,
                LeaseUid = leaseUid
            }, cancellationToken: cancellationToken));
            var execution = await multi.ReadSingleOrDefaultAsync<AnalisisExecution>();
            if (execution == null) return null;
            return new AnalisisContext
            {
                Ejecucion = execution,
                Fuentes = (await multi.ReadAsync<PamFuenteAnalisisDescriptor>()).ToList(),
                Proyectos = (await multi.ReadAsync<ProjectSnapshot>()).ToList()
            };
        }

        private List<ProcessedFinding> ConstruirHallazgos(
            PamFuenteAnalisisDescriptor source,
            IReadOnlyCollection<PamRegistroExtraido> records,
            IReadOnlyDictionary<string, List<ProjectSnapshot>> projectsByKey)
        {
            var findings = new List<ProcessedFinding>();
            foreach (var record in records)
            {
                var dataJson = JsonSerializer.Serialize(record.Datos ?? new Dictionary<string, string>(), JsonOptions);
                var normalizedKey = NormalizarClave(record.Clave);
                var recoveredDocumentKey = false;
                if (string.Equals(record.TipoHallazgo, "Referencia documental", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(normalizedKey)
                    && !projectsByKey.ContainsKey(normalizedKey))
                {
                    var shortened = normalizedKey;
                    while (shortened.Length > 1 && char.IsLetter(shortened[^1]))
                    {
                        shortened = shortened[..^1];
                        if (!projectsByKey.ContainsKey(shortened)) continue;
                        normalizedKey = shortened;
                        recoveredDocumentKey = true;
                        break;
                    }
                }
                var finding = new ProcessedFinding
                {
                    SourceId = source.LoteFuenteId,
                    Record = record,
                    NormalizedKey = normalizedKey,
                    DataJson = dataJson
                };

                if (string.Equals(record.TipoHallazgo, "Incidencia", StringComparison.OrdinalIgnoreCase))
                {
                    finding.MatchStatus = "Observado";
                }
                else if (string.IsNullOrWhiteSpace(normalizedKey))
                {
                    finding.MatchStatus = "Sin clave";
                }
                else if (projectsByKey.TryGetValue(normalizedKey, out var candidates) && candidates.Count > 0)
                {
                    if (candidates.Count > 1)
                    {
                        finding.MatchStatus = "Ambigua";
                        finding.CandidatesJson = JsonSerializer.Serialize(candidates.Select(item => new
                        {
                            item.ProyectoId,
                            item.ProyectoVersionId,
                            item.ClaveProyecto,
                            item.NombreProyecto
                        }), JsonOptions);
                        finding.CandidateCount = candidates.Count;
                    }
                    else
                    {
                        finding.Project = candidates[0];
                        finding.CandidateCount = 1;
                        finding.Confidence = recoveredDocumentKey
                            ? .85m
                            : string.Equals(record.ClaveOriginal?.Trim(), candidates[0].ClaveProyecto?.Trim(), StringComparison.OrdinalIgnoreCase)
                                ? 1m : .97m;
                        finding.MatchStatus = finding.Confidence == 1m ? "Exacta" : "Normalizada";
                        if (string.Equals(record.TipoHallazgo, "Fila estructurada", StringComparison.OrdinalIgnoreCase))
                            finding.Changes.AddRange(CompararRegistro(record, candidates[0]));
                    }
                }
                else if (!EsFormatoClaveValido(normalizedKey))
                {
                    finding.MatchStatus = "Inválida";
                }
                else
                {
                    finding.MatchStatus = "No encontrada";
                    if (string.Equals(record.TipoHallazgo, "Fila estructurada", StringComparison.OrdinalIgnoreCase)
                        && !IndicaCancelacion(record.Etapa))
                        finding.Changes.Add(CrearAltaPropuesta(record, dataJson));
                }

                finding.Hash = CrearHash(string.Join("|", new[]
                {
                    source.LoteFuenteId.ToString(CultureInfo.InvariantCulture),
                    record.TipoHallazgo, record.Ubicacion, normalizedKey, record.Nombre, dataJson
                }));
                findings.Add(finding);
            }
            return findings;
        }

        private static List<ProposedChange> CompararRegistro(PamRegistroExtraido record, ProjectSnapshot project)
        {
            var changes = new List<ProposedChange>();

            AddTextChange(changes, "NombreProyecto", project.NombreProyecto, record.Nombre, .96m, "Nombre informado en fila con clave exacta.");
            AddTextChange(changes, "GRT", project.GRT, record.Grt, .98m, "Región/GCR informada en el archivo estructurado.");
            AddTextChange(changes, "EtapaProyecto", project.EtapaProyecto, record.Etapa, .98m, "Estatus CFE informado para el corte.");
            AddTextChange(changes, "FechaNecesaria", project.FechaNecesaria, record.FechaNecesaria, .95m, "Fecha necesaria informada en el corte.");
            AddTextChange(changes, "FeoFactible", project.FeoFactible, record.FechaEntradaOperacion, .90m, "Fecha de entrada en operación informada en el corte.");
            AddTextChange(changes, "EstadoRealProyecto", project.EstadoRealProyecto, record.EstadoDetalle, .95m, "Estatus, financiamiento y avance informados por CFE.");

            if (record.AnioInstruccion.HasValue && project.AnioInstruccion != record.AnioInstruccion)
                changes.Add(ProposedChange.Modification("AnioInstruccion", project.AnioInstruccion, record.AnioInstruccion, "Número", .98m, "Año único de instrucción informado en el Excel."));

            if (record.MontoMdp.HasValue && (!project.MontoProyectoMdp.HasValue || Math.Abs(project.MontoProyectoMdp.Value - record.MontoMdp.Value) >= .001m))
                changes.Add(ProposedChange.Modification("MontoProyectoMdp", project.MontoProyectoMdp, record.MontoMdp, "Decimal", .98m, "Monto pormenorizado CFE informado en mdp."));

            var normalizedStage = NormalizarComparacion(record.Etapa);
            if (normalizedStage == "CANCELADO" && !string.Equals(project.EstadoVigenciaCartera, "Cancelado", StringComparison.OrdinalIgnoreCase))
                changes.Add(ProposedChange.Status("Baja", "EstadoVigenciaCartera", project.EstadoVigenciaCartera, "Cancelado", 1m, "El documento marca expresamente el proyecto como Cancelado."));
            else if ((normalizedStage.Contains("REQUIERECANCELACION", StringComparison.Ordinal)
                      || normalizedStage.Contains("SOLICITUDDECANCELACION", StringComparison.Ordinal))
                     && !string.Equals(project.EstadoVigenciaCartera, "Cancelado", StringComparison.OrdinalIgnoreCase))
                changes.Add(ProposedChange.Status("Baja", "EstadoVigenciaCartera", project.EstadoVigenciaCartera, "Cancelado", .65m, "El documento informa una solicitud o requerimiento de cancelación; necesita validación humana."));

            return changes;
        }

        private async Task PersistirFuenteAsync(
            long analisisId,
            Guid leaseUid,
            PamFuenteAnalisisDescriptor source,
            IReadOnlyCollection<ProcessedFinding> findings,
            string finalStatus,
            string finalMessage,
            int processed,
            int errors,
            CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var ownsLease = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT COUNT(*) FROM dgmesnie.PAMAnalisisEjecucion WITH (UPDLOCK, HOLDLOCK)
WHERE AnalisisId = @AnalisisId AND LeaseUid = @LeaseUid AND Estado = N'Ejecutando';",
                    new { AnalisisId = analisisId, LeaseUid = leaseUid }, transaction, cancellationToken: cancellationToken));
                if (ownsLease != 1) throw new InvalidOperationException("La ejecución perdió el control exclusivo del análisis.");

                await connection.ExecuteAsync(new CommandDefinition(@"
DELETE r
FROM dgmesnie.PAMRevisionPendiente r
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
WHERE h.AnalisisId = @AnalisisId AND h.LoteFuenteId = @LoteFuenteId;

DELETE cp
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
WHERE h.AnalisisId = @AnalisisId AND h.LoteFuenteId = @LoteFuenteId;

DELETE FROM dgmesnie.PAMHallazgoProyecto
WHERE AnalisisId = @AnalisisId AND LoteFuenteId = @LoteFuenteId;",
                    new { AnalisisId = analisisId, source.LoteFuenteId }, transaction, cancellationToken: cancellationToken));

                foreach (var finding in findings)
                {
                    var hallazgoId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(@"
INSERT dgmesnie.PAMHallazgoProyecto
(
    AnalisisId, LoteFuenteId, MetodoDeteccion, TipoHallazgo,
    ClaveDetectada, ClaveNormalizada, NombreDetectado,
    HojaPaginaSeccion, NumeroReferencia, FragmentoEvidencia,
    DatosExtraidosJson, HashHallazgo, ResultadoCotejo, NumeroCandidatos,
    ProyectoIdCoincidente, ProyectoVersionIdCoincidente,
    CandidatosJson, Confianza, FechaCotejoUtc
)
VALUES
(
    @AnalisisId, @LoteFuenteId, @MetodoDeteccion, @TipoHallazgo,
    @ClaveDetectada, @ClaveNormalizada, @NombreDetectado,
    @Ubicacion, @NumeroReferencia, @Evidencia,
    @DatosJson, @Hash, @MatchStatus, @CandidateCount,
    @ProyectoId, @ProyectoVersionId, @CandidatesJson, @Confidence, SYSUTCDATETIME()
);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", new
                    {
                        AnalisisId = analisisId,
                        source.LoteFuenteId,
                        finding.Record.MetodoDeteccion,
                        finding.Record.TipoHallazgo,
                        ClaveDetectada = Limitar(finding.Record.ClaveOriginal ?? finding.Record.Clave, 200),
                        ClaveNormalizada = Limitar(finding.NormalizedKey, 200),
                        NombreDetectado = Limitar(finding.Record.Nombre, 500),
                        Ubicacion = Limitar(finding.Record.Ubicacion, 250) ?? "Archivo completo",
                        finding.Record.NumeroReferencia,
                        Evidencia = Limitar(finding.Record.Evidencia, 2000),
                        DatosJson = finding.DataJson,
                        finding.Hash,
                        finding.MatchStatus,
                        finding.CandidateCount,
                        ProyectoId = finding.Project?.ProyectoId,
                        ProyectoVersionId = finding.Project?.ProyectoVersionId,
                        finding.CandidatesJson,
                        finding.Confidence
                    }, transaction, cancellationToken: cancellationToken));

                    var createdRevisionForChange = false;
                    foreach (var change in finding.Changes)
                    {
                        change.Hash = CrearHash($"{finding.Hash}|{change.TipoCambio}|{change.Campo}|{change.ValorPropuestoJson}");
                        var changeId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(@"
INSERT dgmesnie.PAMCambioPropuesto
(
    AnalisisId, HallazgoId, ProyectoId, ProyectoVersionBaseId,
    TipoCambio, Campo, TipoDato, ValorActualJson, ValorPropuestoJson,
    Motivo, Confianza, HashPropuesta, EstadoRevision
)
VALUES
(
    @AnalisisId, @HallazgoId, @ProyectoId, @ProyectoVersionBaseId,
    @TipoCambio, @Campo, @TipoDato, @ValorActualJson, @ValorPropuestoJson,
    @Motivo, @Confianza, @Hash, @EstadoRevision
);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", new
                        {
                            AnalisisId = analisisId,
                            HallazgoId = hallazgoId,
                            ProyectoId = finding.Project?.ProyectoId,
                            ProyectoVersionBaseId = finding.Project?.ProyectoVersionId,
                            change.TipoCambio,
                            change.Campo,
                            change.TipoDato,
                            change.ValorActualJson,
                            change.ValorPropuestoJson,
                            Motivo = Limitar(change.Motivo, 1000),
                            change.Confianza,
                            change.Hash,
                            change.EstadoRevision
                        }, transaction, cancellationToken: cancellationToken));

                        if (change.EstadoRevision == "Requiere revisión")
                        {
                            await InsertarRevisionAsync(connection, transaction, analisisId, hallazgoId, changeId,
                                finding, change.TipoCambio == "Alta" ? "Alta potencial" : "Cambio de baja confianza",
                                change.Motivo, cancellationToken);
                            createdRevisionForChange = true;
                        }
                    }

                    if (!createdRevisionForChange && finding.MatchStatus is "No encontrada" or "Ambigua" or "Inválida" or "Sin clave" or "Observado")
                    {
                        var type = finding.MatchStatus switch
                        {
                            "No encontrada" => "Clave no encontrada",
                            "Ambigua" => "Coincidencia ambigua",
                            "Inválida" => "Clave inválida",
                            "Sin clave" => "Hallazgo sin clave",
                            _ => "Incidencia de extracción"
                        };
                        await InsertarRevisionAsync(connection, transaction, analisisId, hallazgoId, null,
                            finding, type, finding.Record.Evidencia ?? "Requiere revisión manual.", cancellationToken);
                    }
                }

                await connection.ExecuteAsync(new CommandDefinition(@"
UPDATE dgmesnie.PAMLoteFuente
SET EstadoExtraccion = @Estado, MensajeExtraccion = @Mensaje
WHERE LoteFuenteId = @LoteFuenteId;

UPDATE dgmesnie.PAMAnalisisEjecucion
SET FuentesProcesadas = @Procesadas, FuentesError = @Errores,
    FechaHeartbeatUtc = SYSUTCDATETIME(), MensajeResultado = @MensajeAnalisis
WHERE AnalisisId = @AnalisisId AND LeaseUid = @LeaseUid AND Estado = N'Ejecutando';", new
                {
                    Estado = finalStatus,
                    Mensaje = Limitar(finalMessage, 1000),
                    source.LoteFuenteId,
                    Procesadas = processed,
                    Errores = errors,
                    MensajeAnalisis = $"Procesadas {processed:N0} fuente(s)."
                        + (errors > 0 ? $" {errors:N0} con error." : string.Empty),
                    AnalisisId = analisisId,
                    LeaseUid = leaseUid
                }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        private static async Task InsertarRevisionAsync(
            SqlConnection connection,
            System.Data.Common.DbTransaction transaction,
            long analisisId,
            long hallazgoId,
            long? changeId,
            ProcessedFinding finding,
            string type,
            string reason,
            CancellationToken cancellationToken)
        {
            const string sql = @"
INSERT dgmesnie.PAMRevisionPendiente
(
    CargaId, AnalisisId, HallazgoId, CambioPropuestoId, ProyectoIdSugerido,
    ClaveRecibida, NombreRecibido, TipoRevision, Motivo, DatosRecibidosJson, Estado
)
VALUES
(
    NULL, @AnalisisId, @HallazgoId, @CambioPropuestoId, @ProyectoId,
    @Clave, @Nombre, @TipoRevision, @Motivo, @DatosJson, N'Pendiente'
);";
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                AnalisisId = analisisId,
                HallazgoId = hallazgoId,
                CambioPropuestoId = changeId,
                ProyectoId = finding.Project?.ProyectoId,
                Clave = Limitar(finding.Record.ClaveOriginal ?? finding.Record.Clave, 200),
                Nombre = Limitar(finding.Record.Nombre, 500),
                TipoRevision = Limitar(type, 50),
                Motivo = Limitar(reason, 1000) ?? "Requiere revisión manual.",
                DatosJson = finding.DataJson
            }, transaction, cancellationToken: cancellationToken));
        }

        private async Task MarcarFuenteExtrayendoAsync(long analisisId, Guid leaseUid, long sourceId, CancellationToken cancellationToken)
        {
            const string sql = @"
UPDATE lf
SET EstadoExtraccion = N'Extrayendo', MensajeExtraccion = N'Extrayendo contenido y cotejando claves.'
FROM dgmesnie.PAMLoteFuente lf
WHERE lf.LoteFuenteId = @LoteFuenteId
  AND EXISTS
  (
      SELECT 1 FROM dgmesnie.PAMAnalisisEjecucion e
      WHERE e.AnalisisId = @AnalisisId AND e.LeaseUid = @LeaseUid AND e.Estado = N'Ejecutando'
  );";
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                AnalisisId = analisisId,
                LeaseUid = leaseUid,
                LoteFuenteId = sourceId
            }, cancellationToken: cancellationToken));
        }

        private async Task FinalizarAsync(long analisisId, Guid leaseUid, int processed, int errors, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                var counts = await connection.QuerySingleAsync<AnalysisCounts>(new CommandDefinition(@"
SELECT
    (SELECT COUNT(*) FROM dgmesnie.PAMHallazgoProyecto h WHERE h.AnalisisId = @AnalisisId) AS Registros,
    (SELECT COUNT(*) FROM dgmesnie.PAMHallazgoProyecto h WHERE h.AnalisisId = @AnalisisId AND h.ResultadoCotejo IN (N'Exacta', N'Normalizada')) AS Coincidentes,
    (SELECT COUNT(*) FROM dgmesnie.PAMHallazgoProyecto h WHERE h.AnalisisId = @AnalisisId AND h.ResultadoCotejo = N'No encontrada') AS NoEncontrados,
    (SELECT COUNT(*) FROM dgmesnie.PAMCambioPropuesto c WHERE c.AnalisisId = @AnalisisId) AS Cambios,
    (SELECT COUNT(*) FROM dgmesnie.PAMRevisionPendiente r WHERE r.AnalisisId = @AnalisisId AND r.Estado = N'Pendiente') AS Observados;",
                    new { AnalisisId = analisisId }, transaction, cancellationToken: cancellationToken));

                var allSourcesFailed = processed > 0 && processed == errors;
                var status = allSourcesFailed
                    ? "Error"
                    : errors > 0 || counts.Observados > 0 ? "Completado con observaciones" : "Completado";
                var message = allSourcesFailed
                    ? "No fue posible analizar ninguna fuente. Puede solicitar un nuevo intento después de revisar los archivos."
                    : $"Análisis concluido: {counts.Registros:N0} hallazgo(s), {counts.Coincidentes:N0} coincidencia(s) y {counts.Cambios:N0} cambio(s) propuesto(s).";
                var updated = await connection.ExecuteAsync(new CommandDefinition(@"
UPDATE dgmesnie.PAMAnalisisEjecucion
SET Estado = @Estado, FechaFinUtc = SYSUTCDATETIME(), FechaHeartbeatUtc = SYSUTCDATETIME(),
    LeaseUid = NULL, FuentesProcesadas = @Procesadas, FuentesError = @Errores,
    RegistrosDetectados = @Registros, ProyectosCoincidentes = @Coincidentes,
    ProyectosNoEncontrados = @NoEncontrados, CambiosPropuestos = @Cambios,
    TotalObservados = @Observados, MensajeResultado = @Mensaje
WHERE AnalisisId = @AnalisisId AND LeaseUid = @LeaseUid AND Estado = N'Ejecutando';", new
                {
                    Estado = status,
                    Procesadas = processed,
                    Errores = errors,
                    counts.Registros,
                    counts.Coincidentes,
                    counts.NoEncontrados,
                    counts.Cambios,
                    counts.Observados,
                    Mensaje = message,
                    AnalisisId = analisisId,
                    LeaseUid = leaseUid
                }, transaction, cancellationToken: cancellationToken));
                if (updated != 1) throw new InvalidOperationException("La ejecución perdió su lease antes de finalizar.");

                await connection.ExecuteAsync(new CommandDefinition(@"
UPDATE anterior
SET Estado = N'Obsoleto'
FROM dgmesnie.PAMAnalisisEjecucion anterior
INNER JOIN dgmesnie.PAMAnalisisEjecucion actual ON actual.AnalisisId = @AnalisisId AND actual.LoteId = anterior.LoteId
WHERE anterior.AnalisisId <> actual.AnalisisId
  AND anterior.Estado IN (N'Completado', N'Completado con observaciones');

UPDATE propuesta
SET EstadoRevision = N'Obsoleta', Resolucion = N'Sustituida por un análisis posterior.'
FROM dgmesnie.PAMCambioPropuesto propuesta
INNER JOIN dgmesnie.PAMAnalisisEjecucion anterior ON anterior.AnalisisId = propuesta.AnalisisId
INNER JOIN dgmesnie.PAMAnalisisEjecucion actual ON actual.AnalisisId = @AnalisisId AND actual.LoteId = anterior.LoteId
WHERE anterior.AnalisisId <> actual.AnalisisId
  AND propuesta.EstadoRevision IN (N'Pendiente', N'Requiere revisión', N'Conflicto', N'Aprobada');

UPDATE revision
SET Estado = N'Rechazada', Resolucion = N'Sustituida por un análisis posterior.',
    FechaResolucionUtc = SYSUTCDATETIME(), UsuarioResolucion = N'Sistema PAM'
FROM dgmesnie.PAMRevisionPendiente revision
INNER JOIN dgmesnie.PAMAnalisisEjecucion anterior ON anterior.AnalisisId = revision.AnalisisId
INNER JOIN dgmesnie.PAMAnalisisEjecucion actual ON actual.AnalisisId = @AnalisisId AND actual.LoteId = anterior.LoteId
WHERE anterior.AnalisisId <> actual.AnalisisId AND revision.Estado = N'Pendiente';

UPDATE lote
SET Estado = CASE WHEN @Procesadas = @Errores AND @Errores > 0 THEN N'Error' ELSE N'En revisión' END,
    TotalRegistrosDetectados = @Registros,
    TotalCambiosPropuestos = @Cambios,
    TotalObservados = @Observados,
    FechaActualizacionUtc = SYSUTCDATETIME()
FROM dgmesnie.PAMLoteActualizacion lote
INNER JOIN dgmesnie.PAMAnalisisEjecucion actual ON actual.AnalisisId = @AnalisisId AND actual.LoteId = lote.LoteId;", new
                {
                    AnalisisId = analisisId,
                    Procesadas = processed,
                    Errores = errors,
                    counts.Registros,
                    counts.Cambios,
                    counts.Observados
                }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                try { await transaction.RollbackAsync(cancellationToken); } catch { }
                throw;
            }
        }

        private async Task MarcarErrorAsync(long analisisId, Guid leaseUid, Exception exception, CancellationToken cancellationToken)
        {
            const string sql = @"
UPDATE e
SET Estado = N'Error', FechaFinUtc = SYSUTCDATETIME(), FechaHeartbeatUtc = SYSUTCDATETIME(),
    LeaseUid = NULL, CodigoError = @Codigo, MensajeResultado = @Mensaje
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.AnalisisId = @AnalisisId AND e.LeaseUid = @LeaseUid;

UPDATE lote
SET Estado = N'Error', FechaActualizacionUtc = SYSUTCDATETIME()
FROM dgmesnie.PAMLoteActualizacion lote
INNER JOIN dgmesnie.PAMAnalisisEjecucion e ON e.AnalisisId = @AnalisisId AND e.LoteId = lote.LoteId
WHERE e.Estado = N'Error';";
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.ExecuteAsync(new CommandDefinition(sql, new
                {
                    AnalisisId = analisisId,
                    LeaseUid = leaseUid,
                    Codigo = exception.GetType().Name,
                    Mensaje = Limitar(exception.Message, 2000)
                }, cancellationToken: cancellationToken));
            }
            catch (Exception logException)
            {
                _logger.LogError(logException, "No fue posible registrar el error del análisis PAM {AnalisisId}.", analisisId);
            }
        }

        private static void AddTextChange(
            ICollection<ProposedChange> changes,
            string field,
            string current,
            string proposed,
            decimal confidence,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(proposed) || TextoEquivalente(current, proposed)) return;
            changes.Add(ProposedChange.Modification(field, current, proposed, "Texto", confidence, reason));
        }

        private static bool IndicaCancelacion(string value)
        {
            var normalized = NormalizarComparacion(value);
            return normalized == "CANCELADO"
                || normalized.Contains("REQUIERECANCELACION", StringComparison.Ordinal)
                || normalized.Contains("SOLICITUDDECANCELACION", StringComparison.Ordinal);
        }

        private static ProposedChange CrearAltaPropuesta(PamRegistroExtraido record, string dataJson)
        {
            return new ProposedChange
            {
                TipoCambio = "Alta",
                TipoDato = "Objeto",
                ValorActualJson = null,
                ValorPropuestoJson = dataJson,
                Motivo = "La clave estructurada no existe en la cartera vigente; debe validarse como alta potencial.",
                Confianza = .75m,
                EstadoRevision = "Requiere revisión"
            };
        }

        private static PamAltaDatosRecibidos MapearAltaDatos(CambioPropuestoRow row)
        {
            if (row == null || row.TipoCambio != "Alta") return null;

            var values = DeserializarDatosRecibidos(row.ValorPropuestoJson);
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string Find(params string[] names)
            {
                foreach (var pair in values)
                {
                    var normalized = NormalizarComparacion(pair.Key);
                    if (!names.Contains(normalized, StringComparer.Ordinal)) continue;
                    used.Add(pair.Key);
                    return pair.Value;
                }
                return null;
            }

            var yearRaw = Find("ANOINSTRUCCION", "ANODEINSTRUCCION");
            var amountRaw = Find(
                "MONTOPORMENORIZADOCFEMDP",
                "MONTODEPROYECTOPORMENORIZADOCFEMDP",
                "MONTODEPROYECTOPORMENORIZADOCFEMMDP",
                "MONTODELPROYECTOMDP",
                "MONTO");
            var claveFuente = Find("PEM", "CLAVEPEM", "CLAVEPROYECTO");

            return new PamAltaDatosRecibidos
            {
                CambioPropuestoId = row.CambioPropuestoId,
                ClaveProyecto = row.ClaveProyecto ?? claveFuente,
                NombreProyecto = Find("PROYECTO", "NOMBREDELPROYECTO") ?? row.NombreProyecto,
                GRT = Find("GCR", "GRT", "REGION"),
                EtapaProyecto = Find("ESTATUSCFEPAMRNT2026", "ETAPADELPROYECTO", "ETAPA"),
                FechaNecesaria = Find("FECHANECESARIA"),
                FeoFactible = Find("FECHAENTRADAOPERACION", "FECHADEENTRADAENOPERACION", "FEOFACTIBLE"),
                MontoProyectoMdp = LeerDecimalFlexible(amountRaw),
                EstadoRealProyecto = Find(
                    "ESTATUSCFEFINANCIAMIENTOAVANCE",
                    "ESTADOREALPROYECTO",
                    "FINANCIAMIENTOAVANCE"),
                AnioInstruccion = LeerAnio(yearRaw),
                FuenteDocumento = row.FuenteDocumento,
                UbicacionFuente = row.UbicacionFuente,
                Confianza = row.Confianza,
                CamposAdicionales = values
                    .Where(pair => !used.Contains(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                    .Select(pair => new PamCampoRecibidoVista { Etiqueta = pair.Key, Valor = pair.Value })
                    .ToList()
            };
        }

        private static PamDecisionPreliminarVista MapearDecision(CambioPropuestoRow row)
        {
            if (row?.RevisionId is null) return null;
            return new PamDecisionPreliminarVista
            {
                RevisionId = row.RevisionId.Value,
                Clasificacion = row.ClasificacionPreliminar,
                ProyectoRelacionadoId = row.ProyectoRelacionadoId,
                ProyectoRelacionadoClave = row.ProyectoRelacionadoClave,
                ProyectoRelacionadoNombre = row.ProyectoRelacionadoNombre,
                Nota = row.NotaPreliminar,
                UsuarioId = row.UsuarioDecisionPreliminarId,
                UsuarioNombre = row.UsuarioDecisionPreliminar,
                FechaDecisionUtc = row.FechaDecisionPreliminarUtc,
                VersionDecision = row.VersionDecision,
                Estado = row.EstadoDecision
            };
        }

        private static List<PamProyectoCandidatoVista> CrearCandidatos(
            PamAltaDatosRecibidos alta,
            IReadOnlyCollection<ProyectoCandidatoRow> pool)
        {
            if (alta == null || pool == null || pool.Count == 0) return new();

            return pool
                .Select(candidate => CalcularCandidato(alta, candidate))
                .Where(candidate => candidate.Score >= 15m)
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.ClaveProyecto, StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();
        }

        private static PamProyectoCandidatoVista CalcularCandidato(
            PamAltaDatosRecibidos alta,
            ProyectoCandidatoRow candidate)
        {
            var nameScore = SimilitudNombre(alta.NombreProyecto, candidate.NombreProyecto);
            var candidateKeys = ExtraerClavesProyecto(candidate.ClaveProyecto);
            var normalizedIncomingKey = NormalizarClave(alta.ClaveProyecto);
            var exactKey = !string.IsNullOrWhiteSpace(normalizedIncomingKey)
                && candidateKeys.Any(key => string.Equals(
                    NormalizarClave(key), normalizedIncomingKey, StringComparison.Ordinal));
            var exactName = TextoEquivalente(alta.NombreProyecto, candidate.NombreProyecto)
                && !string.IsNullOrWhiteSpace(alta.NombreProyecto);
            var keyScore = candidateKeys.Count > 0
                ? candidateKeys.Max(key => SimilitudClave(alta.ClaveProyecto, key))
                : SimilitudClave(alta.ClaveProyecto, candidate.ClaveProyecto);
            var regionScore = TextoEquivalente(alta.GRT, candidate.GRT)
                && !string.IsNullOrWhiteSpace(alta.GRT) ? 100m : 0m;
            var score = decimal.Round((nameScore * .70m) + (keyScore * .20m) + (regionScore * .10m), 1);
            if (exactKey && exactName) score = 100m;
            else if (exactKey) score = Math.Max(score, 95m);
            else if (exactName) score = Math.Max(score, 90m);
            var matches = new List<string>();
            if (exactName) matches.Add("Mismo proyecto por nombre");
            else if (nameScore >= 75m) matches.Add("Nombre muy similar");
            else if (nameScore >= 45m) matches.Add("Coincidencia parcial del nombre");
            if (exactKey) matches.Add("La clave ya pertenece a este proyecto");
            else if (keyScore >= 70m) matches.Add("Clave de la misma familia");
            else if (keyScore >= 40m) matches.Add("Clave parcialmente similar");
            if (regionScore == 100m) matches.Add("Misma región");
            if (matches.Count == 0) matches.Add("Semejanza débil; requiere validación manual");

            return new PamProyectoCandidatoVista
            {
                ProyectoId = candidate.ProyectoId,
                ProyectoVersionId = candidate.ProyectoVersionId,
                ClaveProyecto = candidate.ClaveProyecto,
                NombreProyecto = candidate.NombreProyecto,
                GRT = candidate.GRT,
                EtapaProyecto = candidate.EtapaProyecto,
                Score = score,
                Motivo = string.Join(" · ", matches),
                Coincidencias = matches,
                CoincidenciaClaveExacta = exactKey,
                CoincidenciaNombreExacta = exactName,
                ClasificacionSugerida = exactKey || exactName ? "Vincular existente" : null
            };
        }

        private static decimal SimilitudNombre(string left, string right)
        {
            var leftTokens = Tokenizar(left);
            var rightTokens = Tokenizar(right);
            var tokenScore = 0m;
            if (leftTokens.Count > 0 && rightTokens.Count > 0)
            {
                var common = leftTokens.Intersect(rightTokens, StringComparer.Ordinal).Count();
                tokenScore = 200m * common / (leftTokens.Count + rightTokens.Count);
            }

            var compactLeft = NormalizarComparacion(left);
            var compactRight = NormalizarComparacion(right);
            var charScore = SimilitudEdicion(compactLeft, compactRight);
            if (!string.IsNullOrWhiteSpace(compactLeft)
                && !string.IsNullOrWhiteSpace(compactRight)
                && (compactLeft.Contains(compactRight, StringComparison.Ordinal)
                    || compactRight.Contains(compactLeft, StringComparison.Ordinal)))
            {
                charScore = Math.Max(charScore, 85m);
            }
            return Math.Max(tokenScore, charScore);
        }

        private static decimal SimilitudClave(string left, string right)
        {
            var normalizedLeft = NormalizarClave(left);
            var normalizedRight = NormalizarClave(right);
            if (string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight)) return 0m;
            if (string.Equals(normalizedLeft, normalizedRight, StringComparison.Ordinal)) return 100m;

            var score = SimilitudEdicion(
                NormalizarComparacion(normalizedLeft),
                NormalizarComparacion(normalizedRight));
            var leftFamily = normalizedLeft.Split('-', 2)[0];
            var rightFamily = normalizedRight.Split('-', 2)[0];
            if (string.Equals(leftFamily, rightFamily, StringComparison.Ordinal)) score = Math.Max(score, 70m);
            return score;
        }

        private static List<string> ExtraerClavesProyecto(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new();
            var normalized = value
                .Replace('‐', '-').Replace('‑', '-').Replace('‒', '-')
                .Replace('–', '-').Replace('—', '-').Replace('−', '-')
                .ToUpperInvariant();
            return Regex.Matches(
                    normalized,
                    @"(?<![A-Z0-9])[A-Z]{1,5}\d{2}-[A-Z0-9]{2,12}(?:-[A-Z0-9]{1,12})*(?![A-Z0-9])",
                    RegexOptions.CultureInvariant)
                .Select(match => match.Value)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static decimal SimilitudEdicion(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return 0m;
            if (string.Equals(left, right, StringComparison.Ordinal)) return 100m;
            var previous = Enumerable.Range(0, right.Length + 1).ToArray();
            var current = new int[right.Length + 1];
            for (var i = 1; i <= left.Length; i++)
            {
                current[0] = i;
                for (var j = 1; j <= right.Length; j++)
                {
                    var substitution = previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
                    current[j] = Math.Min(Math.Min(previous[j] + 1, current[j - 1] + 1), substitution);
                }
                (previous, current) = (current, previous);
            }
            var longest = Math.Max(left.Length, right.Length);
            return decimal.Round(100m * (longest - previous[right.Length]) / longest, 1);
        }

        private static HashSet<string> Tokenizar(string value)
        {
            var ignored = new HashSet<string>(StringComparer.Ordinal)
            {
                "DE", "DEL", "LA", "LAS", "EL", "LOS", "Y", "EN", "PARA", "POR", "AL"
            };
            if (string.IsNullOrWhiteSpace(value)) return new(StringComparer.Ordinal);
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
                builder.Append(char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ');
            }
            return builder.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(token => token.Length > 1 && !ignored.Contains(token))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static Dictionary<string, string> DeserializarDatosRecibidos(string json)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json)) return values;
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Object) return values;
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var value = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                            ? null
                            : property.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(value)) values[property.Name] = value.Trim();
                }
            }
            catch (JsonException) { }
            return values;
        }

        private static decimal? LeerDecimalFlexible(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var cleaned = value.Replace("$", string.Empty, StringComparison.Ordinal).Trim();
            if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)) return invariant;
            return decimal.TryParse(cleaned, NumberStyles.Number, new CultureInfo("es-MX"), out var local) ? local : null;
        }

        private static int? LeerAnio(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var matches = Regex.Matches(value, @"\b(?:19|20)\d{2}\b", RegexOptions.CultureInvariant)
                .Select(match => match.Value)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            return matches.Count == 1 && int.TryParse(matches[0], out var year) ? year : null;
        }

        private static PamCambioPropuestoVista MapearCambioVista(CambioPropuestoRow row)
        {
            return new PamCambioPropuestoVista
            {
                CambioPropuestoId = row.CambioPropuestoId,
                ProyectoId = row.ProyectoId,
                ProyectoVersionBaseId = row.ProyectoVersionBaseId,
                ClaveProyecto = row.ClaveProyecto,
                NombreProyecto = row.NombreProyecto,
                TipoCambio = row.TipoCambio,
                Campo = row.Campo,
                Categoria = row.Categoria,
                ValorActual = FormatearJsonEscalar(row.ValorActualJson),
                ValorPropuesto = FormatearJsonEscalar(row.ValorPropuestoJson),
                Confianza = row.Confianza,
                FuenteDocumento = row.FuenteDocumento,
                UbicacionFuente = row.UbicacionFuente,
                Estado = row.Estado,
                CambioDecisionId = row.CambioDecisionId,
                DecisionCampo = row.DecisionCampo,
                NotaDecisionCampo = row.NotaDecisionCampo,
                UsuarioDecisionCampoId = row.UsuarioDecisionCampoId,
                UsuarioDecisionCampo = row.UsuarioDecisionCampo,
                FechaDecisionCampoUtc = row.FechaDecisionCampoUtc,
                EstadoDecision = row.EstadoDecision
            };
        }

        private static string FormatearJsonEscalar(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("value", out var value)
                    && root.EnumerateObject().Count() == 1)
                {
                    return value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString(),
                        JsonValueKind.Null => null,
                        _ => value.GetRawText()
                    };
                }

                return root.ValueKind == JsonValueKind.String
                    ? root.GetString()
                    : root.GetRawText();
            }
            catch { return json; }
        }

        private static string SerializarValorJson(object value)
            => value == null ? null : JsonSerializer.Serialize(new { value }, JsonOptions);

        private static string CrearHuellaEntrada(IEnumerable<string> hashes)
            => CrearHash($"{VersionMotor}|{string.Join("|", hashes.Select(hash => hash?.Trim().ToUpperInvariant()).OrderBy(hash => hash, StringComparer.Ordinal))}");

        private static string CrearHash(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));

        private static string CrearPatronBusqueda(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var escaped = value.Trim()
                .Replace("[", "[[]", StringComparison.Ordinal)
                .Replace("%", "[%]", StringComparison.Ordinal)
                .Replace("_", "[_]", StringComparison.Ordinal);
            return $"%{escaped}%";
        }

        private static string NormalizarClave(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return Regex.Replace(value
                .Replace('‐', '-').Replace('‑', '-').Replace('‒', '-')
                .Replace('–', '-').Replace('—', '-').Replace('−', '-')
                .ToUpperInvariant(), @"\s+", string.Empty).Trim();
        }

        private static bool EsFormatoClaveValido(string key)
            => Regex.IsMatch(key ?? string.Empty, @"^(?:[A-Z]{1,5}\d{2})-[A-Z0-9]{2,12}$", RegexOptions.CultureInvariant);

        private static bool TextoEquivalente(string left, string right)
            => string.Equals(NormalizarComparacion(left), NormalizarComparacion(right), StringComparison.Ordinal);

        private static string NormalizarComparacion(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(character)) builder.Append(char.ToUpperInvariant(character));
            }
            return builder.ToString();
        }

        private static string NormalizarTipo(string value)
        {
            var allowed = new[] { "Alta", "Modificación", "Baja", "Reactivación", "Observación" };
            return allowed.FirstOrDefault(item => string.Equals(item, value?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizarClasificacionPreliminar(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return NormalizarComparacion(value) switch
            {
                "ALTAREAL" => "Alta real",
                "VINCULAREXISTENTE" => "Vincular existente",
                "PADREHIJO" => "Padre-hijo",
                "DESCARTAR" => "Descartar",
                _ => null
            };
        }

        private static string NormalizarDecisionCampo(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return NormalizarComparacion(value) switch
            {
                "APLICAR" => "Aplicar",
                "MANTENERSQL" => "Mantener SQL",
                "OMITIR" => "Omitir",
                _ => null
            };
        }

        private static byte[] LeerVersionDecision(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("La versión de la decisión es obligatoria.", nameof(value));
            var hex = value.Trim();
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex[2..];
            if (hex.Length != 16)
                throw new ArgumentException("La versión de la decisión no tiene un formato válido.", nameof(value));
            try
            {
                var bytes = Convert.FromHexString(hex);
                if (bytes.Length != 8) throw new FormatException();
                return bytes;
            }
            catch (FormatException)
            {
                throw new ArgumentException("La versión de la decisión no tiene un formato válido.", nameof(value));
            }
        }

        private static string Limitar(string value, int length)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Length <= length ? trimmed : trimmed[..length];
        }

        private sealed class FuenteHuella
        {
            public long LoteFuenteId { get; set; }
            public string HashSha256 { get; set; }
        }

        private sealed class AnalisisContext
        {
            public AnalisisExecution Ejecucion { get; set; }
            public List<PamFuenteAnalisisDescriptor> Fuentes { get; set; } = new();
            public List<ProjectSnapshot> Proyectos { get; set; } = new();
        }

        private sealed class AnalisisExecution
        {
            public long AnalisisId { get; set; }
            public long LoteId { get; set; }
            public int UsuarioId { get; set; }
            public string UsuarioNombre { get; set; }
        }

        private sealed class ProjectSnapshot
        {
            public long ProyectoId { get; set; }
            public long ProyectoVersionId { get; set; }
            public string ClaveProyecto { get; set; }
            public string NombreProyecto { get; set; }
            public string GRT { get; set; }
            public string EtapaProyecto { get; set; }
            public string EstadoVigenciaCartera { get; set; }
            public decimal? MontoProyectoMdp { get; set; }
            public string FechaNecesaria { get; set; }
            public int? AnioInstruccion { get; set; }
            public string EstadoRealProyecto { get; set; }
            public string FeoFactible { get; set; }
        }

        private sealed class ProcessedFinding
        {
            public long SourceId { get; set; }
            public PamRegistroExtraido Record { get; set; }
            public string NormalizedKey { get; set; }
            public string DataJson { get; set; }
            public string Hash { get; set; }
            public string MatchStatus { get; set; }
            public int CandidateCount { get; set; }
            public string CandidatesJson { get; set; }
            public decimal? Confidence { get; set; }
            public ProjectSnapshot Project { get; set; }
            public List<ProposedChange> Changes { get; } = new();
        }

        private sealed class ProposedChange
        {
            public string TipoCambio { get; set; }
            public string Campo { get; set; }
            public string TipoDato { get; set; }
            public string ValorActualJson { get; set; }
            public string ValorPropuestoJson { get; set; }
            public string Motivo { get; set; }
            public decimal Confianza { get; set; }
            public string EstadoRevision { get; set; }
            public string Hash { get; set; }

            public static ProposedChange Modification(string field, object current, object proposed, string dataType, decimal confidence, string reason)
                => Status("Modificación", field, current, proposed, confidence, reason, dataType);

            public static ProposedChange Status(string type, string field, object current, object proposed, decimal confidence, string reason, string dataType = "Texto")
                => new()
                {
                    TipoCambio = type,
                    Campo = field,
                    TipoDato = dataType,
                    ValorActualJson = SerializarValorJson(current),
                    ValorPropuestoJson = SerializarValorJson(proposed)
                        ?? JsonSerializer.Serialize(new { value = (object)null }, JsonOptions),
                    Motivo = reason,
                    Confianza = confidence,
                    EstadoRevision = confidence < .85m ? "Requiere revisión" : "Pendiente"
                };
        }

        private sealed class AnalysisCounts
        {
            public int Registros { get; set; }
            public int Coincidentes { get; set; }
            public int NoEncontrados { get; set; }
            public int Cambios { get; set; }
            public int Observados { get; set; }
        }

        private sealed class LoteAnalisisContexto
        {
            public string Estado { get; set; }
            public string TipoActualizacion { get; set; }
        }

        private sealed class CambioPropuestoRow
        {
            public long CambioPropuestoId { get; set; }
            public long? ProyectoId { get; set; }
            public long? ProyectoVersionBaseId { get; set; }
            public string GrupoProyecto { get; set; }
            public string ClaveProyecto { get; set; }
            public string NombreProyecto { get; set; }
            public string TipoCambio { get; set; }
            public string Campo { get; set; }
            public string Categoria { get; set; }
            public string ValorActualJson { get; set; }
            public string ValorPropuestoJson { get; set; }
            public decimal? Confianza { get; set; }
            public string FuenteDocumento { get; set; }
            public string UbicacionFuente { get; set; }
            public string Estado { get; set; }
            public long? RevisionId { get; set; }
            public string ClasificacionPreliminar { get; set; }
            public long? ProyectoRelacionadoId { get; set; }
            public string ProyectoRelacionadoClave { get; set; }
            public string ProyectoRelacionadoNombre { get; set; }
            public string NotaPreliminar { get; set; }
            public int? UsuarioDecisionPreliminarId { get; set; }
            public string UsuarioDecisionPreliminar { get; set; }
            public DateTime? FechaDecisionPreliminarUtc { get; set; }
            public string VersionDecision { get; set; }
            public long? CambioDecisionId { get; set; }
            public string DecisionCampo { get; set; }
            public string NotaDecisionCampo { get; set; }
            public int? UsuarioDecisionCampoId { get; set; }
            public string UsuarioDecisionCampo { get; set; }
            public DateTime? FechaDecisionCampoUtc { get; set; }
            public string EstadoDecision { get; set; }
        }

        private sealed class ProyectoCandidatoRow
        {
            public long ProyectoId { get; set; }
            public long ProyectoVersionId { get; set; }
            public string ClaveProyecto { get; set; }
            public string NombreProyecto { get; set; }
            public string GRT { get; set; }
            public string EtapaProyecto { get; set; }
        }

        private sealed class AltaFamiliaRow
        {
            public string ClaveProyecto { get; set; }
            public string NombreProyecto { get; set; }
        }

        private sealed class AltaFamiliaInfo
        {
            public string ClaveNormalizada { get; set; }
            public List<string> Claves { get; set; } = new();
        }

        private sealed class DecisionTargetRow
        {
            public long RevisionId { get; set; }
            public byte[] VersionDecision { get; set; }
            public bool EsCancelacion { get; set; }
        }

        private sealed class CampoDecisionTargetRow
        {
            public long CambioPropuestoId { get; set; }
            public long? CambioDecisionId { get; set; }
        }

        private sealed class ProyectoResultadoConteo
        {
            public int TotalProyectos { get; set; }
            public int TotalCambios { get; set; }
        }
    }

    public sealed class PamDecisionPreliminarException : InvalidOperationException
    {
        public PamDecisionPreliminarException(string message) : base(message)
        {
        }
    }

    public sealed class PamPaqueteCierreException : InvalidOperationException
    {
        public PamPaqueteCierreException(string message) : base(message)
        {
        }
    }

    public sealed class PamAplicacionException : InvalidOperationException
    {
        public PamAplicacionException(string message) : base(message)
        {
        }
    }

    public sealed class PamAnalisisClaim
    {
        public long AnalisisId { get; set; }
        public Guid LeaseUid { get; set; }
    }

    public sealed class PamAnalisisEstadoDto
    {
        public long AnalisisId { get; set; }
        public string Estado { get; set; }
        public int TotalFuentes { get; set; }
        public int FuentesProcesadas { get; set; }
        public decimal ProgresoPorcentaje { get; set; }
        public string Mensaje { get; set; }
    }
}
