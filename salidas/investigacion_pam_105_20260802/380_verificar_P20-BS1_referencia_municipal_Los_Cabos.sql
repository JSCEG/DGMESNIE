SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-P20-BS1-20260806';

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 151
      AND Activa = 1
      AND Validada = 1
      AND EsPrincipal = 1
      AND PrecisionUbicacion = N'municipal'
      AND MetodoUbicacion = N'centroide_municipal_documentado'
      AND Municipio = N'Los Cabos'
      AND Entidad = N'Baja California Sur'
      AND ABS(Latitud - CONVERT(decimal(9,6), 23.276622)) < 0.000001
      AND ABS(Longitud - CONVERT(decimal(10,6), -109.753272)) < 0.000001
      AND RadioSugeridoKm = CONVERT(decimal(8,2), 50.00)
      AND ISJSON(GeometriaJson) = 1
      AND JSON_VALUE(GeometriaJson, '$.type') = N'Point'
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 1
    THROW 54621, N'Verificación: P20-BS1 no tiene exactamente una referencia municipal consistente.', 1;

SELECT
    v.ProyectoId,
    v.ClaveProyecto,
    v.NombreProyecto,
    u.UbicacionId,
    u.Etiqueta,
    u.Latitud,
    u.Longitud,
    u.Entidad,
    u.Municipio,
    u.PrecisionUbicacion,
    u.RadioSugeridoKm,
    u.Validada,
    u.Fuente,
    u.Observaciones
FROM dgmesnie.PAMProyectoVersion v
INNER JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = v.ProyectoId
WHERE v.ProyectoId = 151
  AND v.EsVersionVigente = 1
  AND u.Observaciones LIKE N'%' + @Lote + N'%';

SELECT
    COUNT(DISTINCT u.ProyectoId) AS ProyectosVigentesConUbicacionActiva,
    COUNT(*) AS GeometriasActivasVigentes
FROM dgmesnie.PAMProyectoUbicacion u
INNER JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = u.ProyectoId
   AND v.EsVersionVigente = 1
   AND v.EstadoVigenciaCartera = N'Vigente'
WHERE u.Activa = 1;
