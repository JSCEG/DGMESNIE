SET NOCOUNT ON;
DECLARE @LoteId BIGINT = 4;
DECLARE @AnalisisId BIGINT;
DECLARE @PaquetePreflightId BIGINT;
DECLARE @PaqueteEstado NVARCHAR(20);

SELECT TOP (1) @AnalisisId = e.AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.LoteId = @LoteId
ORDER BY e.AnalisisId DESC;

PRINT CONCAT('ANALISIS_ID=', COALESCE(CONVERT(NVARCHAR(30), @AnalisisId), 'NULL'));

IF @AnalisisId IS NULL
BEGIN
    PRINT 'No se encontró análisis para este lote';
    RETURN;
END

IF OBJECT_ID('tempdb..#AccionesPreparacion') IS NOT NULL DROP TABLE #AccionesPreparacion;
IF OBJECT_ID('tempdb..#CamposPreparacion') IS NOT NULL DROP TABLE #CamposPreparacion;

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

SELECT
    @LoteId as LoteId,
    @AnalisisId as AnalisisId,
    (SELECT COUNT(*) FROM #AccionesPreparacion WHERE EstadoRevision <> N'Revisada' OR TipoAccion IN (N'Pendiente', N'Relación por validar')) AS AccionesPendientes,
    (SELECT COUNT(*) FROM #AccionesPreparacion WHERE TipoAccion IN (N'Pendiente', N'Relación por validar')) AS AccionesRelacionPendiente,
    (SELECT COUNT(*) FROM #CamposPreparacion WHERE DecisionCampo IS NULL) AS CamposPendientes,
    (SELECT COUNT(*) FROM #CamposPreparacion WHERE DecisionCampo = N'Aplicar' AND Campo = N'MontoProyectoMdp' AND (TRY_CONVERT(DECIMAL(20,3), ValorPropuesto) IS NULL OR TRY_CONVERT(DECIMAL(20,3), ValorPropuesto) < 0)) AS BloqueoMontosInvalidos,
    (SELECT COUNT(*) FROM #CamposPreparacion WHERE DecisionCampo = N'Aplicar' AND Campo = N'NombreProyecto' AND NULLIF(LTRIM(RTRIM(ValorPropuesto)), N'') IS NULL) AS BloqueoNombresVacios,
    (SELECT COUNT(*) FROM #CamposPreparacion WHERE DecisionCampo = N'Aplicar' AND Campo IN (N'FechaNecesaria', N'FeoFactible') AND NULLIF(LTRIM(RTRIM(ValorPropuesto)), N'') IS NOT NULL AND TRY_CONVERT(DATE, ValorPropuesto) IS NULL) AS BloqueoFechasHitos,
    (SELECT COUNT(*) FROM #AccionesPreparacion WHERE TipoAccion IN (N'Vincular clave', N'Cancelar existente') AND ProyectoRelacionadoId IS NULL) AS BloqueoVinculosSinDestino,
    (SELECT COUNT(*) FROM #AccionesPreparacion WHERE TipoAccion IN (N'Pendiente', N'Relación por validar')) AS BloqueoRelacionVal;

SELECT TOP (1)
    @PaquetePreflightId = p.PaqueteAplicacionId
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.AnalisisId = @AnalisisId
ORDER BY p.PaqueteAplicacionId DESC;

SELECT TOP (1) @PaqueteEstado = e.TipoEvento
FROM dgmesnie.PAMAplicacionPaqueteEvento e
WHERE e.PaqueteAplicacionId = @PaquetePreflightId
ORDER BY e.PaqueteEventoId DESC;

SELECT
    @PaquetePreflightId AS PaquetePreflightId,
    COALESCE(@PaqueteEstado, 'SIN_ESTADO') AS EstadoActual,
    p.PaqueteUid,
    p.HashContenido,
    p.TotalDetalles, p.TotalCambiosCampo, p.TotalAccionesClave,
    p.UsuarioCreacion, p.FechaCreacionUtc
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.PaqueteAplicacionId = @PaquetePreflightId;

SELECT
    (SELECT COUNT(*) FROM #CamposPreparacion) AS TotalCambios,
    (SELECT COUNT(*) FROM #AccionesPreparacion) AS TotalAcciones,
    CASE WHEN @PaquetePreflightId IS NOT NULL
         AND NOT EXISTS (
            SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p
            WHERE p.PaqueteAplicacionId = @PaquetePreflightId
              AND p.HashContenido = (COALESCE((SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo FROM #CamposPreparacion ORDER BY CambioPropuestoId FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]') + N'|' + COALESCE((SELECT RevisionId, EstadoRevision, ClasificacionPreliminar, TipoAccion, ProyectoRelacionadoId FROM #AccionesPreparacion ORDER BY RevisionId FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]'))
        ) THEN 0 ELSE 1 END AS PaqueteNoCoincide;
