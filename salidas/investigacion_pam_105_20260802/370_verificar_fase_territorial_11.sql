SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-11';

SELECT v.ProyectoId, v.ClaveProyecto, v.NombreProyecto, v.EstadoVigenciaCartera,
       u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
       u.PrecisionUbicacion, u.MetodoUbicacion, u.RadioSugeridoKm,
       u.Validada, u.Activa, u.Fuente
FROM dgmesnie.PAMProyectoVersion v
LEFT JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = v.ProyectoId AND u.Activa = 1
WHERE v.ProyectoId IN (151, 174, 175, 199, 249)
  AND v.EsVersionVigente = 1
ORDER BY v.ProyectoId, u.Orden, u.UbicacionId;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (174, 249) AND Activa = 1
      AND Validada = 1 AND Observaciones LIKE N'%' + @Lote + N'%') <> 2
    THROW 54321, N'Verificación: el lote no tiene exactamente dos ubicaciones activas y validadas.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (174, 249) AND Observaciones LIKE N'%' + @Lote + N'%'
      AND (ISJSON(GeometriaJson) <> 1 OR TipoGeometria <> N'Point'
           OR PrecisionUbicacion <> N'exacta'
           OR RadioSugeridoKm <= 0 OR Validada <> 1 OR Activa <> 1)
)
    THROW 54322, N'Verificación: alguna ubicación tiene metadatos incompletos o inconsistentes.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (151, 175, 199) AND Activa = 1
)
    THROW 54323, N'Verificación: Monte Real/Buenavista, La Cruz o Cuauhtémoc–Maniobras 34 recibieron una ubicación no autorizada.', 1;

SELECT COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva
FROM dgmesnie.PAMProyectoUbicacion u
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = u.ProyectoId
 AND v.EsVersionVigente = 1 AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
