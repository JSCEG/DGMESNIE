SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-D18-OC8-ANTEA-20260806';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 79
          AND MetodoUbicacion = N'conciliacion_diagrama_cenace_osm'
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 1
        THROW 54641, N'Reversión: no se encontró exactamente el registro esperado de D18-OC8.', 1;

    DELETE dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 79
      AND MetodoUbicacion = N'conciliacion_diagrama_cenace_osm'
      AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 1
        THROW 54642, N'Reversión: no se eliminó exactamente un registro.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT COUNT(*) AS RegistrosRestantesDelLote
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%';
