SET NOCOUNT ON;
DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-01';

SELECT
    COUNT(*) AS UbicacionesDelLote,
    COUNT(DISTINCT ProyectoId) AS ProyectosDelLote,
    SUM(CASE WHEN Activa = 1 AND Validada = 1 THEN 1 ELSE 0 END) AS ActivasValidadas,
    SUM(CASE WHEN ProyectoId = 192 AND Observaciones LIKE N'%COBERTURA PARCIAL:%' THEN 1 ELSE 0 END) AS ParcialesExplicitamenteRotuladas
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%'
  AND ProyectoId IN (141,160,192,306);

SELECT ProyectoId, Etiqueta, Latitud, Longitud, PrecisionUbicacion,
       Validada, Activa, Fuente, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE (Observaciones LIKE N'%' + @Lote + N'%' AND ProyectoId IN (141,160,192,306))
   OR UbicacionId = 4
ORDER BY ProyectoId, Orden, UbicacionId;

SELECT ProyectoPadreId, ProyectoHijoId, TipoRelacion, EstadoValidacion,
       EsRelacionVigente, Observaciones
FROM dgmesnie.PAMProyectoRelacionVersion
WHERE Observaciones LIKE N'%' + @Lote + N'%'
ORDER BY ProyectoPadreId, ProyectoHijoId;

SELECT
    COUNT(DISTINCT p.ProyectoId) AS CatalogoVigente,
    COUNT(DISTINCT CASE WHEN u.ProyectoId IS NOT NULL THEN p.ProyectoId END) AS ConUbicacionActiva,
    COUNT(DISTINCT CASE WHEN u.Validada = 1 THEN p.ProyectoId END) AS ConUbicacionValidada,
    COUNT(DISTINCT CASE WHEN u.ProyectoId IS NULL THEN p.ProyectoId END) AS SinUbicacionActiva
FROM dgmesnie.vw_PAMProyectoVigente p
LEFT JOIN dgmesnie.PAMProyectoUbicacion u
  ON u.ProyectoId = p.ProyectoId AND u.Activa = 1
WHERE p.EstadoVigenciaCartera = N'Vigente';

IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WHERE Observaciones LIKE N'%' + @Lote + N'%' AND ProyectoId IN (141,160,192,306)) <> 4
    THROW 52201, N'Verificación: faltan ubicaciones del lote.', 1;
IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoRelacionVersion WHERE Observaciones LIKE N'%' + @Lote + N'%') <> 3
    THROW 52202, N'Verificación: faltan relaciones del lote.', 1;
IF NOT EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WHERE UbicacionId = 4 AND ProyectoId = 64 AND Validada = 1 AND Activa = 1)
    THROW 52203, N'Verificación: Cerro del Mercado legado no quedó validado.', 1;
IF EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%'
      AND (Latitud NOT BETWEEN 14 AND 33.5 OR Longitud NOT BETWEEN -118 AND -86 OR ISJSON(GeometriaJson) <> 1)
)
    THROW 52204, N'Verificación: existe una geometría fuera de México o un GeoJSON inválido.', 1;

SELECT N'OK' AS EstadoVerificacion;
