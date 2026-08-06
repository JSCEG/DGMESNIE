SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-PRECISION-MUNICIPAL-20260806-04';

SELECT u.UbicacionId, u.ProyectoId, v.ClaveProyecto, u.Etiqueta,
       u.Entidad, u.Municipio, u.Localidad,
       u.Latitud, u.Longitud, u.RadioSugeridoKm,
       u.PrecisionUbicacion, u.MetodoUbicacion,
       u.Validada, u.Activa, u.Fuente
FROM dgmesnie.PAMProyectoUbicacion u
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = u.ProyectoId AND v.EsVersionVigente = 1
WHERE u.UbicacionId IN (444, 452, 453)
ORDER BY u.ProyectoId;

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE UbicacionId IN (444, 452, 453)
      AND PrecisionUbicacion = N'municipal'
      AND MetodoUbicacion = N'municipio_area_influencia_documentada'
      AND NULLIF(LTRIM(RTRIM(Municipio)), N'') IS NOT NULL
      AND Validada = 1 AND Activa = 1
      AND ISJSON(GeometriaJson) = 1
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 3
    THROW 54761, N'Verificación: las tres referencias no quedaron correctamente clasificadas a nivel municipal.', 1;
