SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-10';

BEGIN TRY
    BEGIN TRANSACTION;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (73, 81, 105) AND Observaciones LIKE N'%' + @Lote + N'%') <> 3
        THROW 54211, N'Reversa: se esperaban exactamente tres ubicaciones del lote.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
          AND ProyectoId NOT IN (73, 81, 105)
    )
        THROW 54212, N'Reversa: el lote contiene un proyecto inesperado.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (73, 81, 105) AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 3
        THROW 54213, N'Reversa: no se eliminaron exactamente tres ubicaciones.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
