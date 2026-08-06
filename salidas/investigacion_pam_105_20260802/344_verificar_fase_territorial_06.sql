SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-06';

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       Direccion, Entidad, Municipio, Localidad,
       PrecisionUbicacion, MetodoUbicacion, RadioSugeridoKm,
       Validada, Activa, Fuente, FechaCorte, Observaciones
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 72 AND Observaciones LIKE N'%' + @Lote + N'%';

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
    WHERE ProyectoId = 72 AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'geocodificada'
      AND MetodoUbicacion = N'referencia_localidad_oficial_y_red'
      AND RadioSugeridoKm = CONVERT(decimal(8,2), 1.50)
      AND Latitud = CONVERT(decimal(9,6), 19.024860)
      AND Longitud = CONVERT(decimal(10,6), -104.311040)
      AND Entidad = N'Colima'
      AND Municipio = N'Manzanillo'
      AND Localidad = N'Campos'
      AND ISJSON(GeometriaJson) = 1
) <> 1
    THROW 53701, N'Verificación: la referencia territorial de D18-OC6 no está íntegra.', 1;

SELECT N'OK' AS EstadoVerificacion;
