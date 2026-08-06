SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-DOCUMENTADO-20260806-03';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 14
        THROW 54721, N'Reversión: el lote no contiene exactamente catorce ubicaciones; no se eliminó nada.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 14
        THROW 54722, N'Reversión: no se eliminaron exactamente las catorce ubicaciones del lote.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT ProyectoId, COUNT(*) AS UbicacionesActivasRestantes
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId IN (256, 259, 261, 262, 278, 279)
  AND Activa = 1
GROUP BY ProyectoId
ORDER BY ProyectoId;
