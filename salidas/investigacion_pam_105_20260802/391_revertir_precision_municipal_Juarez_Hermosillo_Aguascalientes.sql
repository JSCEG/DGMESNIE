SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-PRECISION-MUNICIPAL-20260806-04';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE UbicacionId IN (444, 452, 453)
          AND PrecisionUbicacion = N'municipal'
          AND MetodoUbicacion = N'municipio_area_influencia_documentada'
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 3
        THROW 54751, N'Reversión: no se encontraron exactamente las tres reclasificaciones municipales.', 1;

    UPDATE dgmesnie.PAMProyectoUbicacion
       SET Etiqueta = CASE UbicacionId
               WHEN 444 THEN N'Ciudad Juárez · área de la nueva S.E. Juárez Potencia'
               WHEN 452 THEN N'Hermosillo · referencia regional de P26-NO1'
               WHEN 453 THEN N'Aguascalientes · referencia regional de P26-OC2'
           END,
           PrecisionUbicacion = N'regional',
           MetodoUbicacion = N'localidad_area_influencia_documentada',
           Fuente = CASE UbicacionId
               WHEN 444 THEN N'CENACE · PAMRNT 2025-2039, P25-NT1, alternativa 1; OpenStreetMap Nominatim · Ciudad Juárez'
               WHEN 452 THEN N'PAMRNT 2026-2040 · proyectos identificados, P26-NO1; OpenStreetMap Nominatim · Hermosillo'
               WHEN 453 THEN N'PAMRNT 2026-2040 · proyectos identificados, P26-OC2; OpenStreetMap Nominatim · Aguascalientes'
           END,
           Observaciones = LEFT(Observaciones, CHARINDEX(N' PRECISIÓN MUNICIPAL:', Observaciones + N' PRECISIÓN MUNICIPAL:') - 1)
    WHERE UbicacionId IN (444, 452, 453);

    IF @@ROWCOUNT <> 3
        THROW 54752, N'Reversión: no se revirtieron exactamente las tres referencias.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
