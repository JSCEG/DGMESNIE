SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-02';
DECLARE @Punto geography = geography::Point(28.166052, -105.445552, 4326);

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       PrecisionUbicacion, MetodoUbicacion, Validada, Activa,
       Fuente, FechaCorte, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 52 AND Observaciones LIKE N'%' + @Lote + N'%';

SELECT n.NodoClave, n.Nombre, n.TipoNodo, n.TensionKv, n.EsVirtual,
       n.Latitud, n.Longitud,
       CONVERT(decimal(10,3), geography::Point(n.Latitud, n.Longitud, 4326).STDistance(@Punto)) AS DistanciaMetros
FROM dgmesnie.RedElectricaNodo n
JOIN dgmesnie.RedElectricaVersion rv
  ON rv.VersionId = n.VersionId AND rv.Activa = 1
WHERE n.NombreNormalizado = N'FRANCISCO VILLA'
  AND geography::Point(n.Latitud, n.Longitud, 4326).STDistance(@Punto) <= 500
ORDER BY DistanciaMetros;

SELECT
    COUNT(DISTINCT p.ProyectoId) AS CatalogoVigente,
    COUNT(DISTINCT CASE WHEN u.ProyectoId IS NOT NULL THEN p.ProyectoId END) AS ConUbicacionActiva,
    COUNT(DISTINCT CASE WHEN u.Validada = 1 THEN p.ProyectoId END) AS ConUbicacionValidada,
    COUNT(DISTINCT CASE WHEN u.ProyectoId IS NULL THEN p.ProyectoId END) AS SinUbicacionActiva
FROM dgmesnie.vw_PAMProyectoVigente p
LEFT JOIN dgmesnie.PAMProyectoUbicacion u
  ON u.ProyectoId = p.ProyectoId AND u.Activa = 1
WHERE p.EstadoVigenciaCartera = N'Vigente';

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 52 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'exacta'
      AND Latitud = CONVERT(decimal(9,6), 28.166052)
      AND Longitud = CONVERT(decimal(10,6), -105.445552)
      AND ISJSON(GeometriaJson) = 1
) <> 1
    THROW 52501, N'Verificación: la ubicación exacta de Francisco Villa no está íntegra.', 1;

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.RedElectricaNodo n
    JOIN dgmesnie.RedElectricaVersion rv
      ON rv.VersionId = n.VersionId AND rv.Activa = 1
    WHERE n.NombreNormalizado = N'FRANCISCO VILLA'
      AND geography::Point(n.Latitud, n.Longitud, 4326).STDistance(@Punto) <= 500
) < 4
    THROW 52502, N'Verificación: faltan extremos canónicos concordantes en el grafo.', 1;

SELECT N'OK' AS EstadoVerificacion;
