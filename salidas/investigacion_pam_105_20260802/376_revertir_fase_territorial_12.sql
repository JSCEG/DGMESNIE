SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-12';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (252, 253, 254, 257, 265)
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 14
        THROW 54421, N'Reversa: se esperaban exactamente catorce geometrías del lote.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
          AND ProyectoId NOT IN (252, 253, 254, 257, 265)
    )
        THROW 54422, N'Reversa: el lote contiene un proyecto inesperado.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (252, 253, 254, 257, 265)
      AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 14
        THROW 54423, N'Reversa: no se eliminaron exactamente catorce geometrías.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
