SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-09';

SELECT p.ProyectoId, v.ClaveProyecto, v.NombreProyecto, v.EstadoVigenciaCartera,
       u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
       u.PrecisionUbicacion, u.MetodoUbicacion, u.Validada, u.Activa, u.Fuente
FROM dgmesnie.PAMProyecto p
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
LEFT JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = p.ProyectoId AND u.Activa = 1
WHERE p.ProyectoId = 32;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 32 AND Activa = 1 AND Observaciones LIKE N'%' + @Lote + N'%') <> 1
    THROW 54121, N'Verificación: M18-SIN1 no tiene exactamente una ubicación activa del lote.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 32 AND Activa = 1 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND (Validada <> 1 OR PrecisionUbicacion <> N'exacta' OR ISJSON(GeometriaJson) <> 1
           OR Latitud NOT BETWEEN 18.250000 AND 18.258000
           OR Longitud NOT BETWEEN -96.395000 AND -96.387000)
)
    THROW 54122, N'Verificación: metadatos o coordenadas de M18-SIN1 no son consistentes.', 1;

SELECT COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva
FROM dgmesnie.PAMProyectoUbicacion u
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = u.ProyectoId
 AND v.EsVersionVigente = 1 AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
