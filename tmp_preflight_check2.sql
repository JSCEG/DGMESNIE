SET NOCOUNT ON;
DECLARE @LoteId BIGINT = 4;
DECLARE @AnalisisId BIGINT;
DECLARE @PaqueteId BIGINT;
DECLARE @HashContenido NVARCHAR(64);
DECLARE @TotalDetallesPaquete INT;
DECLARE @HashActual NVARCHAR(MAX);

SELECT TOP (1) @AnalisisId = e.AnalisisId
FROM dgmesnie.PAMAnalisisEjecucion e
WHERE e.LoteId = @LoteId
ORDER BY e.AnalisisId DESC;

SELECT TOP (1) @PaqueteId = p.PaqueteAplicacionId, @HashContenido = p.HashContenido,
               @TotalDetallesPaquete = p.TotalDetalles
FROM dgmesnie.PAMAplicacionPaquete p
WHERE p.AnalisisId = @AnalisisId
ORDER BY p.PaqueteAplicacionId DESC;

SELECT @PaqueteId AS PaqueteId, @HashContenido AS HashContenido, @TotalDetallesPaquete AS TotalDetallesPaquete;

IF OBJECT_ID('tempdb..#A') IS NOT NULL DROP TABLE #A;
IF OBJECT_ID('tempdb..#B') IS NOT NULL DROP TABLE #B;

SELECT RevisionId, Estado, ClasificacionPreliminar
INTO #A
FROM dgmesnie.PAMRevisionPendiente
WHERE AnalisisId = @AnalisisId
  AND HallazgoId IN (SELECT HallazgoId FROM dgmesnie.PAMHallazgoProyecto WHERE ResultadoCotejo IN (N'No encontrada', N'Ambigua', N'Inválida', N'Sin clave'));

SELECT cp.CambioPropuestoId, cp.ProyectoVersionBaseId, cp.Campo, cp.ValorPropuestoJson, cp.ValorActualJson,
       ISNULL(CAST(cp.CambioPropuestoId AS NVARCHAR(20)), '') AS CPK,
       decision.CambioDecisionId, decision.DecisionCampo
INTO #B
FROM dgmesnie.PAMCambioPropuesto cp
OUTER APPLY (SELECT TOP (1) d.CambioDecisionId, d.DecisionCampo FROM dgmesnie.PAMCambioDecisionHistorial d WHERE d.CambioPropuestoId = cp.CambioPropuestoId ORDER BY d.CambioDecisionId DESC) decision
WHERE cp.AnalisisId = @AnalisisId
  AND cp.TipoCambio <> 'Alta';

SET @HashActual =
    COALESCE((SELECT CambioPropuestoId, CambioDecisionId, DecisionCampo FROM #B ORDER BY CambioPropuestoId FOR JSON PATH, INCLUDE_NULL_VALUES), '[]')
    + '|' +
    COALESCE((SELECT r.RevisionId, r.Estado, r.ClasificacionPreliminar FROM #A r ORDER BY r.RevisionId FOR JSON PATH, INCLUDE_NULL_VALUES), '[]');

SELECT 'PAQUETE_DESACTUALIZADO' AS Bloqueo, CASE WHEN @PaqueteId IS NULL OR NOT EXISTS(SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p WHERE p.PaqueteAplicacionId = @PaqueteId AND p.HashContenido = @HashActual) THEN 1 ELSE 0 END AS Total
UNION ALL
SELECT 'DETALLE_INCOMPLETO', CASE WHEN @PaqueteId IS NULL OR EXISTS(SELECT 1 FROM dgmesnie.PAMAplicacionPaquete p WHERE p.PaqueteAplicacionId = @PaqueteId AND p.TotalDetalles <> (SELECT COUNT(*) FROM dgmesnie.PAMAplicacionPaqueteDetalle d WHERE d.PaqueteAplicacionId = p.PaqueteAplicacionId)) THEN 1 ELSE 0 END
UNION ALL
SELECT 'DERIVA_VERSION', COUNT(*)
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
INNER JOIN dgmesnie.PAMCambioPropuesto cp ON cp.CambioPropuestoId = d.ReferenciaId
LEFT JOIN dgmesnie.PAMProyectoVersion pv ON pv.ProyectoVersionId = cp.ProyectoVersionBaseId
LEFT JOIN dgmesnie.PAMProyectoVersion pvVigente ON pvVigente.ClaveProyecto = pv.ClaveProyecto AND pvVigente.EsVersionVigente = 1
WHERE d.PaqueteAplicacionId = @PaqueteId
  AND d.TipoElemento = 'CambioCampo'
  AND d.Accion = 'Aplicar'
  AND (pvVigente.ProyectoVersionId IS NULL OR pv.EsVersionVigente <> 1);

SELECT 'INTEGRIDAD_DETALLE', COUNT(*) AS Total
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
OUTER APPLY (SELECT TOP (1) cd.CambioDecisionId FROM dgmesnie.PAMCambioDecisionHistorial cd WHERE cd.CambioPropuestoId = d.ReferenciaId ORDER BY cd.CambioDecisionId DESC) decisionCampo
LEFT JOIN dgmesnie.PAMRevisionPendiente revision ON d.TipoElemento = 'AccionClave' AND revision.RevisionId = d.ReferenciaId
WHERE d.PaqueteAplicacionId = @PaqueteId
  AND d.HashDetalle <>
      CASE d.TipoElemento
          WHEN 'CambioCampo' THEN CONVERT(CHAR(64), HASHBYTES('SHA2_256',
               CONCAT(CONVERT(NVARCHAR(MAX), 'CambioCampo|'), d.ReferenciaId, '|', decisionCampo.CambioDecisionId, '|', d.Accion, '|', COALESCE(d.ValorAnteriorJson, ''), '|', COALESCE(d.ValorNuevoJson, ''))), 2)
          ELSE CONVERT(CHAR(64), HASHBYTES('SHA2_256',
               CONCAT(CONVERT(NVARCHAR(MAX), 'AccionClave|'), d.ReferenciaId, '|', sys.fn_varbintohexstr(revision.VersionDecision), '|', d.Accion, '|', COALESCE(CONVERT(NVARCHAR(30), d.ProyectoRelacionadoId), ''))), 2)
      END;

SELECT 'VALORES_NO_CONVERTIBLES', COUNT(*)
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
WHERE d.PaqueteAplicacionId = @PaqueteId
  AND d.TipoElemento = 'CambioCampo'
  AND d.Accion = 'Aplicar'
  AND (
      (d.Campo = 'MontoProyectoMdp' AND TRY_CONVERT(DECIMAL(20,3), JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)
   OR (d.Campo = 'AnioInstruccion' AND TRY_CONVERT(INT, JSON_VALUE(d.ValorNuevoJson, '$.value')) IS NULL)
  );

SELECT 'ALTAS_SIN_DATOS', COUNT(*)
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
WHERE d.PaqueteAplicacionId = @PaqueteId
  AND d.Accion IN ('Alta vigente', 'Antecedente cancelado')
  AND (d.ClaveProyecto IS NULL OR NULLIF(LTRIM(RTRIM(d.NombreProyecto)), '') IS NULL);

SELECT 'CLAVES_SIN_DESTINO', COUNT(*)
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
WHERE d.PaqueteAplicacionId = @PaqueteId AND d.Accion = 'Vincular clave' AND d.ProyectoRelacionadoId IS NULL;

SELECT 'CONFLICTO_CLAVE', COUNT(*)
FROM dgmesnie.PAMAplicacionPaqueteDetalle d
INNER JOIN dgmesnie.PAMProyectoVersion vigente
  ON vigente.ClaveProyecto = d.ClaveProyecto AND vigente.EsVersionVigente = 1
WHERE d.PaqueteAplicacionId = @PaqueteId
  AND d.Accion = 'Vincular clave'
  AND vigente.ProyectoId <> d.ProyectoRelacionadoId;
