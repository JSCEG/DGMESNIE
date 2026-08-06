SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-09';

BEGIN TRY
    BEGIN TRANSACTION;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 32 AND Observaciones LIKE N'%' + @Lote + N'%') <> 1
        THROW 54111, N'Reversa: se esperaba exactamente una ubicación del lote para M18-SIN1.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 32 AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 1
        THROW 54112, N'Reversa: no se eliminó exactamente una ubicación.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
