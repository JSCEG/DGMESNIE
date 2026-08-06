SET NOCOUNT ON;
DECLARE @LoteId BIGINT = 4;
DECLARE @AnalisisId BIGINT;
DECLARE @PaqueteId BIGINT;
DECLARE @HashActual NVARCHAR(MAX);
DECLARE @HashPaquete NVARCHAR(64);

SELECT TOP (1) @AnalisisId = e.AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.LoteId = @LoteId
ORDER BY e.AnalisisId DESC;

SELECT TOP (1) @PaqueteId = p.PaqueteAplicacionId,
                  @HashPaquete = p.HashContenido
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.AnalisisId = @AnalisisId
ORDER BY p.PaqueteAplicacionId DESC;

IF OBJECT_ID('tempdb..#AccionesPreparacion') IS NOT NULL DROP TABLE #AccionesPreparacion;
IF OBJECT_ID('tempdb..#CamposPreparacion') IS NOT NULL DROP TABLE #CamposPreparacion;

SELECT r.RevisionId, r.Estado AS EstadoRevision, r.ClasificacionPreliminar,
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
       r.ProyectoRelacionadoPreliminarId AS ProyectoRelacionadoId
INTO #AccionesPreparacion
FROM dgmesnie.PAMRevisionPendiente r
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = r.HallazgoId
WHERE r.AnalisisId = @AnalisisId
  AND h.ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave');

SELECT cp.CambioPropuestoId,
       decision.CambioDecisionId,
       decision.DecisionCampo
INTO #CamposPreparacion
FROM dgmesnie.PAMCambioPropuesto cp
INNER JOIN dgmesnie.PAMHallazgoProyecto h ON h.HallazgoId = cp.HallazgoId
OUTER APPLY (
    SELECT TOP (1) d.CambioDecisionId, d.DecisionCampo
    FROM dgmesnie.PAMCambioDecisionHistorial d
    WHERE d.CambioPropuestoId = cp.CambioPropuestoId
    ORDER BY d.CambioDecisionId DESC
) decision
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio <> N'Alta'
  AND h.ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave');

SET @HashActual =
    COALESCE((SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo FROM #CamposPreparacion ORDER BY CambioPropuestoId FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]')
    + N'|'
    + COALESCE((SELECT RevisionId, EstadoRevision, ClasificacionPreliminar, TipoAccion, ProyectoRelacionadoId FROM #AccionesPreparacion ORDER BY RevisionId FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]');

SELECT @AnalisisId AS AnalisisId, @PaqueteId AS PaqueteId, @HashPaquete AS HashPaquete, @HashActual AS HashActual,
       CAST(CASE WHEN @PaqueteId IS NOT NULL AND @HashPaquete = @HashActual THEN 1 ELSE 0 END AS INT) AS EsVigente;
