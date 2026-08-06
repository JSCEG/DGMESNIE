SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-DOCUMENTADO-20260806-03';

SELECT
    p.ProyectoId,
    v.ClaveProyecto,
    v.NombreProyecto,
    u.UbicacionId,
    u.Etiqueta,
    u.Latitud,
    u.Longitud,
    u.Entidad,
    u.Municipio,
    u.PrecisionUbicacion,
    u.MetodoUbicacion,
    u.RadioSugeridoKm,
    u.EsPrincipal,
    u.Validada,
    u.Activa,
    u.Fuente
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = p.ProyectoId
WHERE u.Observaciones LIKE N'%' + @Lote + N'%'
ORDER BY p.ProyectoId, u.Orden;

SELECT
    COUNT(*) AS UbicacionesLote,
    COUNT(DISTINCT ProyectoId) AS ProyectosCubiertos,
    SUM(CASE WHEN PrecisionUbicacion = N'exacta' THEN 1 ELSE 0 END) AS ReferenciasInfraestructura,
    SUM(CASE WHEN PrecisionUbicacion = N'municipal' THEN 1 ELSE 0 END) AS ReferenciasMunicipales,
    SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) AS Principales,
    SUM(CASE WHEN Validada = 1 AND Activa = 1 THEN 1 ELSE 0 END) AS ActivasValidadas,
    SUM(CASE WHEN ISJSON(GeometriaJson) = 1 THEN 1 ELSE 0 END) AS GeometriasJsonValidas
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%';

SELECT
    u.ProyectoId,
    SUM(CASE WHEN u.EsPrincipal = 1 THEN 1 ELSE 0 END) AS Principales,
    COUNT(*) AS Ubicaciones
FROM dgmesnie.PAMProyectoUbicacion u
WHERE u.Observaciones LIKE N'%' + @Lote + N'%'
GROUP BY u.ProyectoId
ORDER BY u.ProyectoId;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WHERE Observaciones LIKE N'%' + @Lote + N'%') <> 14
    THROW 54731, N'Verificación: el lote no contiene catorce ubicaciones.', 1;

IF (SELECT COUNT(DISTINCT ProyectoId) FROM dgmesnie.PAMProyectoUbicacion WHERE Observaciones LIKE N'%' + @Lote + N'%') <> 6
    THROW 54732, N'Verificación: el lote no cubre seis proyectos.', 1;

IF EXISTS
(
    SELECT ProyectoId
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%'
    GROUP BY ProyectoId
    HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
)
    THROW 54733, N'Verificación: algún PEM no tiene exactamente una ubicación principal.', 1;

IF EXISTS
(
    SELECT 1
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%'
      AND (Validada <> 1 OR Activa <> 1 OR ISJSON(GeometriaJson) <> 1)
)
    THROW 54734, N'Verificación: alguna ubicación no está activa, validada o tiene geometría inválida.', 1;
