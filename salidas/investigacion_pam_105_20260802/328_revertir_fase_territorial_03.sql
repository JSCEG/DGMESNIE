SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-03';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 294
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Observaciones LIKE N'%COBERTURA PARCIAL:%'
          AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND Latitud = CONVERT(decimal(9,6), 16.985489)
          AND Longitud = CONVERT(decimal(10,6), -93.160639)
    ) <> 1
        THROW 52701, N'Reversor: no se encontró exactamente el punto parcial de Chicoasén II.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 294
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Observaciones LIKE N'%COBERTURA PARCIAL:%'
      AND Activa = 1 AND Validada = 1
      AND PrecisionUbicacion = N'exacta'
      AND Latitud = CONVERT(decimal(9,6), 16.985489)
      AND Longitud = CONVERT(decimal(10,6), -93.160639);

    IF @@ROWCOUNT <> 1
        THROW 52702, N'Reversor: no se eliminó exactamente una ubicación.', 1;

    COMMIT TRANSACTION;
    SELECT N'Fase territorial 03 revertida' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
