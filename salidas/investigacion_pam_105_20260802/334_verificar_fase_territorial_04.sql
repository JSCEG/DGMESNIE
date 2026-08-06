SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-04';
DECLARE @Punto geography = geography::Point(22.094586, -100.907603, 4326);

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       PrecisionUbicacion, MetodoUbicacion, Validada, Activa,
       Fuente, FechaCorte, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 39 AND Observaciones LIKE N'%' + @Lote + N'%';

SELECT RegistroClave, Nombre, TensionKv, Latitud, Longitud,
       CONVERT(decimal(10,3), geography::Point(Latitud, Longitud, 4326).STDistance(@Punto)) AS DistanciaMetros
FROM dgmesnie.RedElectricaSubestacionInventario
WHERE RegistroClave = N'dgmesnie_geojson:se:b83bfc7077e57d646d9a' AND Activa = 1;

SELECT DISTINCT a.Nombre, a.TensionKv, a.EstadoConexion, a.NodoOrigenClave, a.NodoDestinoClave
FROM dgmesnie.RedElectricaArista a
WHERE a.VersionId = (SELECT MAX(VersionId) FROM dgmesnie.RedElectricaVersion WHERE Activa = 1)
  AND (a.NodoOrigenClave = N'se:b83bfc7077e57d646d9a'
       OR a.NodoDestinoClave = N'se:b83bfc7077e57d646d9a')
  AND a.NombreNormalizado IN (N'SAN LUIS I LA PILA', N'EL POTOSI SAN LUIS I');

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
    WHERE ProyectoId = 39 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'exacta'
      AND MetodoUbicacion = N'conciliacion_documental_oficial_topologica'
      AND Latitud = CONVERT(decimal(9,6), 22.094586)
      AND Longitud = CONVERT(decimal(10,6), -100.907603)
      AND ISJSON(GeometriaJson) = 1
) <> 1
    THROW 53101, N'Verificación: la ubicación conciliada de P18-OC1 no está íntegra.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dgmesnie.RedElectricaSubestacionInventario
    WHERE RegistroClave = N'dgmesnie_geojson:se:b83bfc7077e57d646d9a'
      AND Activa = 1
      AND geography::Point(Latitud, Longitud, 4326).STDistance(@Punto) < 50
)
    THROW 53102, N'Verificación: el punto oficial ya no concuerda con el inventario canónico.', 1;

SELECT N'OK' AS EstadoVerificacion;
