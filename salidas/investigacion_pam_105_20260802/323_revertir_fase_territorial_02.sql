SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-02';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 52
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND Latitud = CONVERT(decimal(9,6), 28.166052)
          AND Longitud = CONVERT(decimal(10,6), -105.445552)
    ) <> 1
        THROW 52401, N'Reversor: no se encontró exactamente la ubicación aplicada de Francisco Villa.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 52
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'exacta'
      AND Latitud = CONVERT(decimal(9,6), 28.166052)
      AND Longitud = CONVERT(decimal(10,6), -105.445552);

    IF @@ROWCOUNT <> 1
        THROW 52402, N'Reversor: no se eliminó exactamente una ubicación.', 1;

    COMMIT TRANSACTION;
    SELECT N'Fase territorial 02 revertida' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
