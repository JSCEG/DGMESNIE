SET NOCOUNT ON;
DECLARE @LoteId BIGINT = 4;
DECLARE @AnalisisId BIGINT;
DECLARE @PaquetePreflightId BIGINT;

SELECT TOP (1) @AnalisisId = e.AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.LoteId = @LoteId
ORDER BY e.AnalisisId DESC;

IF OBJECT_ID('tempdb..#AccionesPreparacion') IS NOT NULL DROP TABLE #AccionesPreparacion;
IF OBJECT_ID('tempdb..#CamposPreparacion') IS NOT NULL DROP TABLE #CamposPreparacion;

-- Acciones y cambios de revisión
SELECT r.RevisionId, r.ClaveRecibida, r.NombreRecibido,
       r.Estado AS EstadoRevision, r.ClasificacionPreliminar,
       CASE
           WHEN r.Estado <> N''Revisada'' OR r.ClasificacionPreliminar IS NULL THEN N''Pendiente''
           WHEN r.ClasificacionPreliminar = N''Descartar'' THEN N''No aplicar''
           WHEN h.DatosExtraidosJson LIKE N''%Cancel%'' AND r.ClasificacionPreliminar = N''Alta real'' THEN N''Antecedente cancelado''
           WHEN h.DatosExtraidosJson LIKE N''%Cancel%'' AND r.ClasificacionPreliminar = N''Vincular existente'' THEN N''Cancelar existente''
           WHEN r.ClasificacionPreliminar = N''Alta real'' THEN N''Alta vigente''
           WHEN r.ClasificacionPreliminar = N''Vincular existente'' THEN N''Vincular clave''
           WHEN r.ClasificacionPreliminar = N''Padre-hijo'' THEN N''Relación por validar''
           ELSE N''Pendiente''
       END AS TipoAccion,
       r.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId,
       relaciona.ClaveProyecto AS ProyectoRelacionadoClave,
       relaciona.NombreProyecto AS ProyectoRelacionadoNombre,
       f.NombreDocumento AS FuenteDocumento,
       h.HojaPaginaSeccion AS UbicacionFuente,
       CAST(CASE WHEN h.DatosExtraidosJson LIKE N''%Cancel%'' THEN 1 ELSE 0 END AS BIT) AS EsCancelacion
INTO #AccionesPreparacion
FROM dgmesnie.PAMRevisionPendiente r
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
LEFT JOIN dgmesnie.PAMProyectoVersion relaciona
  ON relaciona.ProyectoId = r.ProyectoRelacionadoPreliminarId
 AND relaciona.EsVersionVigente = 1
WHERE r.AnalisisId = @AnalisisId
  AND h.ResultadoCotejo IN (N''No encontrada'', N''Ambigua'', N''Inválida'', N''Sin clave'');

SELECT cp.CambioPropuestoId, cp.ProyectoVersionBaseId, cp.Campo,
       cp.ValorActualJson, cp.ValorPropuestoJson,
       decisionActual.CambioDecisionId,
       decisionActual.DecisionCampo
INTO #CamposPreparacion
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
INNER JOIN dgmesnie.PAMLoteFuente lf ON lf.LoteFuenteId = h.LoteFuenteId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = lf.FuenteId
OUTER APPLY
(
    SELECT TOP (1) decision.CambioDecisionId, decision.DecisionCampo
    FROM dgmesnie.PAMCambioDecisionHistorial decision
    WHERE decision.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY decision.CambioDecisionId DESC
) decisionActual
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio <> N''Alta'';

SELECT TOP (1) @PaquetePreflightId = p.PaqueteAplicacionId
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.AnalisisId = @AnalisisId
ORDER BY p.PaqueteAplicacionId DESC;

DECLARE @HashActual NVARCHAR(MAX) =
    COALESCE((SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo FROM #CamposPreparacion ORDER BY CambioPropuestoId FOR JSON PATH, INCLUDE_NULL_VALUES), N''[]'')
    + N''|'' +
    COALESCE((SELECT RevisionId, EstadoRevision, ClasificacionPreliminar, TipoAccion, ProyectoRelacionadoId FROM #AccionesPreparacion ORDER BY RevisionId FOR JSON PATH, INCLUDE_NULL_VALUES), N''[]'');

DECLARE @EstadoActual NVARCHAR(20) = (SELECT TOP (1) e.TipoEvento FROM dgmesnie.PAMAplicacionPaqueteEvento e WHERE e.PaqueteAplicacionId = @PaquetePreflightId ORDER BY e.PaqueteEventoId DESC);

IF @PaquetePreflightId IS NULL
BEGIN
    SELECT 'PAQUETE_MISSING' AS Codigo, 1 AS Total;
END
ELSE
BEGIN
    SELECT * FROM (
      SELECT N'PAQUETE_DESACTUALIZADO' AS Codigo, CAST(CASE WHEN NOT EXISTS (SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p WHERE p.PaqueteAplicacionId = @PaquetePreflightId AND p.HashContenido = @HashActual) THEN 1 ELSE 0 END AS INT) AS Total
      UNION ALL
      SELECT N'DETALLE_INCOMPLETO', CAST(CASE WHEN EXISTS (SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p WHERE p.PaqueteAplicacionId = @PaquetePreflightId AND p.TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d WHERE d.PaqueteAplicacionId = p.PaqueteAplicacionId)) THEN 1 ELSE 0 END AS INT)
      UNION ALL
      SELECT N'INTEGRIDAD_DETALLE', CAST((
          SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
          OUTER APPLY
          (
              SELECT TOP (1) cd.CambioDecisionId
              FROM dgmesnie.PAMCambioDecisionHistorial cd
              WHERE cd.CambioPropuestoId = d.ReferenciaId
              ORDER BY cd.CambioDecisionId DESC
          ) decisionCampo
          LEFT JOIN dgmesnie.PAMRevisionPendiente revision ON d.TipoElemento = N''AccionClave'' AND revision.RevisionId = d.ReferenciaId
          WHERE d.PaqueteAplicacionId = @PaquetePreflightId
            AND d.HashDetalle <>
              CASE d.TipoElemento
                   WHEN N''CambioCampo'' THEN CONVERT(CHAR(64), HASHBYTES(''SHA2_256'',
                                  CONCAT(CONVERT(NVARCHAR(MAX), N''CambioCampo|''), d.ReferenciaId, N''|'', decisionCampo.CambioDecisionId, N''|'', d.Accion, N''|'', COALESCE(d.ValorAnteriorJson, N''''), N''|'', COALESCE(d.ValorNuevoJson)) ), 2)
                   ELSE CONVERT(CHAR(64), HASHBYTES(''SHA2_256'',
                                CONCAT(CONVERT(NVARCHAR(MAX), N''AccionClave|''), d.ReferenciaId, N''|', sys.fn_varbintohexstr(revision.VersionDecision), N''|'', d.Accion, N''|'', COALESCE(CONVERT(NVARCHAR(30), d.ProyectoRelacionadoId), N''''))), 2)
              END
      ) AS INT)
      UNION ALL
      SELECT N'DERIVA_VERSION', CAST((
        SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d
        INNER JOIN dgmesnie.PAMCambioPropuesto cp ON cp.CambioPropuestoId = d.ReferenciaId
        INNER JOIN dgmesnie.PAMProyectoVersion pv ON pv.ProyectoVersionId = cp.ProyectoVersionBaseId
        WHERE d.PaqueteAplicacionId = @PaquetePreflightId
          AND d.TipoElemento = N''CambioCampo'
          AND d.Accion = N''Aplicar''
          AND (pv.EsVigente <> 1 OR pv.ProyectoVersionId <> (SELECT TOP (1) pv2.ProyectoVersionId FROM dgmesnie.PAMProyectoVersion pv2 WHERE pv2.ClaveProyecto = pv.ClaveProyecto AND pv2.EsVersionVigente = 1))
      ) AS INT)
      UNION ALL
      SELECT N'CAMPOS_NO_SOPORTADOS', CAST((
        SELECT COUNT(*)
        FROM dgmesnie.PAMAplicacionPaqueteDetalle d
        LEFT JOIN dgmesnie.PAMProyectoVersion pv
          ON d.TipoElemento = N''CambioCampo''
         AND d.ProyectoBaseClave = pv.ClaveProyecto
        WHERE d.PaqueteAplicacionId = @PaquetePreflightId
          AND d.TipoElemento = N''CambioCampo''
          AND (d.Campo IS NULL OR NOT EXISTS (
                SELECT 1
                FROM (VALUES (N''NombreProyecto''),(N''GRT''),(N''EtapaProyecto''),(N''FechaNecesaria''),(N''FeoFactible''),(N''MontoProyectoMdp''),(N''EstadoRealProyecto''),(N''AnioInstruccion'')) m(CampoPermitido)
                WHERE m.CampoPermitido = d.Campo))
      ) AS INT)
      UNION ALL
      SELECT N'VALORES_NO_CONVERTIBLES', CAST((
          SELECT COUNT(*)
          FROM dgmesnie.PAMAplicacionPaqueteDetalle d
          WHERE d.PaqueteAplicacionId = @PaquetePreflightId
            AND d.TipoElemento = N''CambioCampo''
            AND d.Accion = N''Aplicar''
            AND (
                (d.Campo = N''MontoProyectoMdp'' AND TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(d.ValorNuevoJson, ''$.value'')) IS NULL)
             OR (d.Campo = N''AnioInstruccion'' AND TRY_CONVERT(INT, JSON_VALUE(d.ValorNuevoJson, ''$.value'')) IS NULL)
            )
      ) AS INT)
      UNION ALL
      SELECT N'CLAVES_SIN_DESTINO', CAST((
          SELECT COUNT(*)
          FROM dgmesnie.PAMAplicacionPaqueteDetalle d
          WHERE d.PaqueteAplicacionId = @PaquetePreflightId
            AND d.Accion = N''Vincular clave'
            AND d.ProyectoRelacionadoId IS NULL
      ) AS INT)
      UNION ALL
      SELECT N'CONFLICTO_CLAVE', CAST((
          SELECT COUNT(*)
          FROM dgmesnie.PAMAplicacionPaqueteDetalle d
          WHERE d.PaqueteAplicacionId = @PaquetePreflightId
            AND d.Accion = N''Vincular clave''
            AND EXISTS (
                SELECT 1
                FROM dgmesnie.PAMProyectoVersion vigente
                WHERE vigente.ClaveProyecto = d.ClaveProyecto
                  AND vigente.EsVigente = 1
                  AND vigente.ProyectoId <> d.ProyectoRelacionadoId
            )
      ) AS INT)
      UNION ALL
      SELECT N'ALTAS_SIN_DATOS', CAST((
          SELECT COUNT(*)
          FROM dgmesnie.PAMAplicacionPaqueteDetalle d
          WHERE d.PaqueteAplicacionId = @PaquetePreflightId
            AND d.Accion IN (N''Alta vigente'', N''Antecedente cancelado')
            AND (d.ClaveProyecto IS NULL OR d.NombreProyecto IS NULL OR LTRIM(RTRIM(COALESCE(d.NombreProyecto, N''))) = N'''')
      ) AS INT)
    ) v
    ORDER BY v.Codigo;
END

SELECT TOP (1) p.PaqueteAplicacionId, p.HashContenido, p.TotalDetalles,
               (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d WHERE d.PaqueteAplicacionId = p.PaqueteAplicacionId) AS DetallesEnTabla,
               COALESCE(@EstadoActual, N'') AS EstadoActual
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.PaqueteAplicacionId = @PaquetePreflightId;
