SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-03';
DECLARE @Punto geography = geography::Point(16.985489, -93.160639, 4326);

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       PrecisionUbicacion, MetodoUbicacion, Validada, Activa,
       Fuente, FechaCorte, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 294 AND Observaciones LIKE N'%' + @Lote + N'%';

SELECT RegistroClave, Nombre, TensionKv, Latitud, Longitud,
       CONVERT(decimal(10,3), geography::Point(Latitud, Longitud, 4326).STDistance(@Punto)) AS DistanciaMetros
FROM dgmesnie.RedElectricaSubestacionInventario
WHERE RegistroClave = N'dgmesnie_geojson:se:2caeebfaebcb38adb3fd' AND Activa = 1;

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
    WHERE ProyectoId = 294 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Observaciones LIKE N'%COBERTURA PARCIAL:%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'exacta'
      AND MetodoUbicacion = N'documental_oficial_cfe_cobertura_parcial'
      AND Latitud = CONVERT(decimal(9,6), 16.985489)
      AND Longitud = CONVERT(decimal(10,6), -93.160639)
      AND ISJSON(GeometriaJson) = 1
) <> 1
    THROW 52801, N'Verificación: el punto oficial parcial de Chicoasén II no está íntegro.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dgmesnie.PAMProyectoRelacionVersion
    WHERE ProyectoPadreId = 274 AND ProyectoHijoId = 294
      AND TipoRelacion = N'Complementario'
      AND EstadoValidacion = N'Pendiente'
      AND EsRelacionVigente = 1
)
    THROW 52802, N'Verificación: cambió la relación pendiente 274→294; revisar trazabilidad.', 1;

SELECT N'OK' AS EstadoVerificacion;
