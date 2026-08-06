SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-11';

BEGIN TRY
    BEGIN TRANSACTION;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (174, 249) AND Observaciones LIKE N'%' + @Lote + N'%') <> 2
        THROW 54311, N'Reversa: se esperaban exactamente dos ubicaciones del lote.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
          AND ProyectoId NOT IN (174, 249)
    )
        THROW 54312, N'Reversa: el lote contiene un proyecto inesperado.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (174, 249) AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 2
        THROW 54313, N'Reversa: no se eliminaron exactamente dos ubicaciones.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
