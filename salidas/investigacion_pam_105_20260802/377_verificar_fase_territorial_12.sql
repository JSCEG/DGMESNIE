SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-12';
DECLARE @Metodo NVARCHAR(80) = N'conciliacion_pamrnt_2025_grafo_canonico';

SELECT
    v.ProyectoId,
    v.ClaveProyecto,
    v.NombreProyecto,
    u.UbicacionId,
    u.Etiqueta,
    u.TipoGeometria,
    u.Latitud,
    u.Longitud,
    u.PrecisionUbicacion,
    u.MetodoUbicacion,
    u.Orden,
    u.EsPrincipal,
    u.Validada,
    u.Activa,
    u.Fuente
FROM dgmesnie.PAMProyectoVersion v
JOIN dgmesnie.PAMProyectoUbicacion u
  ON u.ProyectoId = v.ProyectoId
 AND u.Observaciones LIKE N'%' + @Lote + N'%'
WHERE v.ProyectoId IN (252, 253, 254, 257, 265)
  AND v.EsVersionVigente = 1
ORDER BY v.ProyectoId, u.Orden, u.UbicacionId;

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (252, 253, 254, 257, 265)
      AND Activa = 1
      AND Validada = 1
      AND MetodoUbicacion = @Metodo
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 14
    THROW 54431, N'Verificación: el lote no tiene exactamente catorce geometrías activas y validadas.', 1;

IF
(
    SELECT COUNT(DISTINCT ProyectoId)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (252, 253, 254, 257, 265)
      AND Activa = 1
      AND Validada = 1
      AND MetodoUbicacion = @Metodo
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 5
    THROW 54432, N'Verificación: no quedaron representados exactamente los cinco PEM.', 1;

IF EXISTS
(
    SELECT 1
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (252, 253, 254, 257, 265)
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND
      (
          ISJSON(GeometriaJson) <> 1
          OR JSON_VALUE(GeometriaJson, '$.type') <> TipoGeometria
          OR PrecisionUbicacion <> N'geocodificada'
          OR RadioSugeridoKm <= 0
          OR Validada <> 1
          OR Activa <> 1
      )
)
    THROW 54433, N'Verificación: alguna geometría tiene metadatos incompletos o inconsistentes.', 1;

IF EXISTS
(
    SELECT ProyectoId
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (252, 253, 254, 257, 265)
      AND Activa = 1
      AND Observaciones LIKE N'%' + @Lote + N'%'
    GROUP BY ProyectoId
    HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
)
    THROW 54434, N'Verificación: algún PEM no tiene exactamente una geometría principal.', 1;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
    WHERE TipoGeometria = N'LineString'
      AND Observaciones LIKE N'%' + @Lote + N'%') <> 2
    THROW 54435, N'Verificación: el lote no contiene exactamente las dos trazas autorizadas.', 1;

SELECT
    COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva,
    COUNT(*) AS GeometriasActivasVigentes
FROM dgmesnie.PAMProyectoUbicacion u
JOIN dgmesnie.PAMProyectoVersion v
  ON v.ProyectoId = u.ProyectoId
 AND v.EsVersionVigente = 1
 AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
