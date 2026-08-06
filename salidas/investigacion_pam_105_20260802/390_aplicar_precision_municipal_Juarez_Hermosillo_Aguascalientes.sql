SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @LoteAnterior NVARCHAR(80) = N'PAM-TERRITORIAL-DOCUMENTADO-20260806-03';
DECLARE @Lote NVARCHAR(80) = N'PAM-PRECISION-MUNICIPAL-20260806-04';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-06';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Esperados TABLE
    (
        UbicacionId BIGINT NOT NULL PRIMARY KEY,
        ProyectoId BIGINT NOT NULL,
        EtiquetaAnterior NVARCHAR(300) NOT NULL,
        EtiquetaNueva NVARCHAR(300) NOT NULL,
        Cvegeo NVARCHAR(5) NOT NULL,
        FuenteMunicipal NVARCHAR(300) NOT NULL
    );

    INSERT @Esperados VALUES
        (444, 259,
         N'Ciudad Juárez · área de la nueva S.E. Juárez Potencia',
         N'Juárez · cobertura municipal de la nueva S.E. Juárez Potencia',
         N'08037', N'DGMESNIE · municipios.geojson CVEGEO 08037'),
        (452, 278,
         N'Hermosillo · referencia regional de P26-NO1',
         N'Hermosillo · cobertura municipal de P26-NO1',
         N'26030', N'DGMESNIE · municipios.geojson CVEGEO 26030'),
        (453, 279,
         N'Aguascalientes · referencia regional de P26-OC2',
         N'Aguascalientes · cobertura municipal de P26-OC2',
         N'01001', N'DGMESNIE · municipios.geojson CVEGEO 01001');

    IF
    (
        SELECT COUNT(*)
        FROM @Esperados e
        INNER JOIN dgmesnie.PAMProyectoUbicacion u WITH (UPDLOCK, HOLDLOCK)
            ON u.UbicacionId = e.UbicacionId
           AND u.ProyectoId = e.ProyectoId
           AND u.Etiqueta = e.EtiquetaAnterior
           AND u.PrecisionUbicacion = N'regional'
           AND u.MetodoUbicacion = N'localidad_area_influencia_documentada'
           AND u.Validada = 1
           AND u.Activa = 1
           AND u.Observaciones LIKE N'%' + @LoteAnterior + N'%'
    ) <> 3
        THROW 54741, N'Preflight: cambiaron las tres referencias regionales esperadas; no se actualizó nada.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54742, N'Preflight: la reclasificación municipal ya fue aplicada.', 1;

    UPDATE u
       SET u.Etiqueta = e.EtiquetaNueva,
           u.PrecisionUbicacion = N'municipal',
           u.MetodoUbicacion = N'municipio_area_influencia_documentada',
           u.Fuente = LEFT(CONCAT(u.Fuente, N'; ', e.FuenteMunicipal), 500),
           u.UsuarioValidacion = @Usuario,
           u.FechaValidacionUtc = SYSUTCDATETIME(),
           u.Observaciones = LEFT(CONCAT(
               u.Observaciones,
               N' PRECISIÓN MUNICIPAL: el nombre del proyecto y la capa municipal confirman el municipio CVEGEO ',
               e.Cvegeo,
               N'; el punto sigue siendo representativo y no equivale a la coordenada de una subestación. Lote=',
               @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoUbicacion u
    INNER JOIN @Esperados e ON e.UbicacionId = u.UbicacionId;

    IF @@ROWCOUNT <> 3
        THROW 54743, N'Aplicación: no se actualizaron exactamente las tres referencias municipales.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE UbicacionId IN (444, 452, 453)
          AND PrecisionUbicacion = N'municipal'
          AND MetodoUbicacion = N'municipio_area_influencia_documentada'
          AND Validada = 1 AND Activa = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 3
        THROW 54744, N'Aplicación: la verificación interna de precisión municipal falló.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT UbicacionId, ProyectoId, Etiqueta, Municipio,
       PrecisionUbicacion, MetodoUbicacion, RadioSugeridoKm,
       Validada, Activa, Fuente
FROM dgmesnie.PAMProyectoUbicacion
WHERE UbicacionId IN (444, 452, 453)
ORDER BY ProyectoId;
