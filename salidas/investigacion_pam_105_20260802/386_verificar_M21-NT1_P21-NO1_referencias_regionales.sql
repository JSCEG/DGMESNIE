SET NOCOUNT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-REGIONAL-20260806-02';

IF
(
    SELECT COUNT(*)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (175, 199)
      AND Activa = 1
      AND Validada = 1
      AND PrecisionUbicacion = N'regional'
      AND MetodoUbicacion = N'localidad_area_influencia_documentada'
      AND ISJSON(GeometriaJson) = 1
      AND JSON_VALUE(GeometriaJson, '$.type') = N'Point'
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 3
    THROW 54681, N'Verificación: no existen exactamente las tres referencias regionales consistentes.', 1;

IF
(
    SELECT COUNT(DISTINCT ProyectoId)
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (175, 199)
      AND Activa = 1
      AND Validada = 1
      AND Observaciones LIKE N'%' + @Lote + N'%'
) <> 2
    THROW 54682, N'Verificación: no quedaron representados exactamente los dos PEM.', 1;

IF EXISTS
(
    SELECT ProyectoId
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (175, 199)
      AND Activa = 1
      AND Observaciones LIKE N'%' + @Lote + N'%'
    GROUP BY ProyectoId
    HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
)
    THROW 54683, N'Verificación: cada PEM debe tener exactamente una referencia principal.', 1;

SELECT
    v.ProyectoId, v.ClaveProyecto, v.NombreProyecto,
    u.UbicacionId, u.Etiqueta, u.Latitud, u.Longitud,
    u.Entidad, u.Municipio, u.Localidad,
    u.PrecisionUbicacion, u.RadioSugeridoKm,
    u.EsPrincipal, u.Validada, u.Fuente
FROM dgmesnie.PAMProyectoVersion v
INNER JOIN dgmesnie.PAMProyectoUbicacion u ON u.ProyectoId = v.ProyectoId
WHERE v.ProyectoId IN (175, 199)
  AND v.EsVersionVigente = 1
  AND u.Observaciones LIKE N'%' + @Lote + N'%'
ORDER BY v.ProyectoId, u.Orden;
