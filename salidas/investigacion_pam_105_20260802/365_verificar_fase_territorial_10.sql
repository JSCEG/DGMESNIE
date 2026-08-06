SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-10';

SELECT v.ProyectoId, v.ClaveProyecto, v.NombreProyecto, v.EstadoVigenciaCartera,
       u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
       u.PrecisionUbicacion, u.MetodoUbicacion, u.RadioSugeridoKm,
       u.Validada, u.Activa, u.Fuente
FROM dgmesnie.PAMProyectoVersion v
LEFT JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = v.ProyectoId AND u.Activa = 1
WHERE v.ProyectoId IN (73, 79, 81, 105, 106)
  AND v.EsVersionVigente = 1
ORDER BY v.ProyectoId, u.Orden, u.UbicacionId;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (73, 81, 105) AND Activa = 1
      AND Validada = 1 AND Observaciones LIKE N'%' + @Lote + N'%') <> 3
    THROW 54221, N'Verificación: el lote no tiene exactamente tres ubicaciones activas y validadas.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (73, 81, 105) AND Observaciones LIKE N'%' + @Lote + N'%'
      AND (ISJSON(GeometriaJson) <> 1 OR TipoGeometria <> N'Point'
           OR PrecisionUbicacion NOT IN (N'exacta', N'geocodificada')
           OR RadioSugeridoKm <= 0 OR Validada <> 1 OR Activa <> 1)
)
    THROW 54222, N'Verificación: alguna ubicación tiene metadatos incompletos o inconsistentes.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (79, 106) AND Activa = 1
)
    THROW 54223, N'Verificación: Pedregal o el programa multirregional recibieron una ubicación no autorizada.', 1;

SELECT COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva
FROM dgmesnie.PAMProyectoUbicacion u
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = u.ProyectoId
 AND v.EsVersionVigente = 1 AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
