SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-05';

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       Direccion, Entidad, Municipio, Localidad,
       PrecisionUbicacion, MetodoUbicacion, Validada, Activa,
       Fuente, FechaCorte, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 68 AND Observaciones LIKE N'%' + @Lote + N'%';

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
    WHERE ProyectoId = 68 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'geocodificada'
      AND MetodoUbicacion = N'ubicacion_documental_oficial_utm'
      AND Latitud = CONVERT(decimal(9,6), 24.046717)
      AND Longitud = CONVERT(decimal(10,6), -110.294003)
      AND Entidad = N'Baja California Sur'
      AND Municipio = N'La Paz'
      AND ISJSON(GeometriaJson) = 1
) <> 1
    THROW 53401, N'Verificación: la ubicación documental de P16-BS2 no está íntegra.', 1;

SELECT N'OK' AS EstadoVerificacion;
