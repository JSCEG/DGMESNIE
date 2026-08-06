SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-08';

SELECT p.ProyectoId, v.ClaveProyecto, v.NombreProyecto, v.EstadoVigenciaCartera,
       u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
       u.PrecisionUbicacion, u.MetodoUbicacion, u.Validada, u.Activa, u.Fuente
FROM dgmesnie.PAMProyecto p
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
LEFT JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = p.ProyectoId AND u.Activa = 1
WHERE p.ProyectoId = 20;

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 20 AND Activa = 1 AND Observaciones LIKE N'%' + @Lote + N'%') <> 1
    THROW 54021, N'Verificación: P17-OR3 no tiene exactamente una ubicación activa del lote.', 1;

IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 20 AND Activa = 1 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND (Validada <> 1 OR PrecisionUbicacion <> N'exacta' OR ISJSON(GeometriaJson) <> 1
           OR Latitud NOT BETWEEN 17.790000 AND 17.797000
           OR Longitud NOT BETWEEN -92.975000 AND -92.968000)
)
    THROW 54022, N'Verificación: metadatos o coordenadas de P17-OR3 no son consistentes.', 1;

SELECT COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva
FROM dgmesnie.PAMProyectoUbicacion u
JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = u.ProyectoId
 AND v.EsVersionVigente = 1 AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
