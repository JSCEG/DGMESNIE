SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-D18-OC8-ANTEA-20260806';

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 79
      AND Activa = 1
      AND Validada = 1
      AND EsPrincipal = 1
      AND PrecisionUbicacion = N'geocodificada'
      AND MetodoUbicacion = N'conciliacion_diagrama_cenace_osm'
      AND ABS(Latitud - CONVERT(decimal(9,6), 20.674820)) < 0.000001
      AND ABS(Longitud - CONVERT(decimal(10,6), -100.431820)) < 0.000001
      AND ISJSON(GeometriaJson) = 1
      AND JSON_VALUE(GeometriaJson, '$.type') = N'Point'
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 1
    THROW 54651, N'Verificación: D18-OC8 no tiene exactamente el extremo Antea esperado.', 1;

SELECT
    v.ProyectoId, v.ClaveProyecto, v.NombreProyecto,
    u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
    u.Entidad, u.Municipio, u.PrecisionUbicacion,
    u.Validada, u.Fuente, u.Observaciones
FROM dgmesnie.PAMProyectoVersion v
INNER JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = v.ProyectoId
WHERE v.ProyectoId = 79
  AND v.EsVersionVigente = 1
  AND u.Observaciones LIKE N'%' + @Lote + N'%';
