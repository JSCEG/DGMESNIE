SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-06';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 72
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Latitud = CONVERT(decimal(9,6), 19.024860)
          AND Longitud = CONVERT(decimal(10,6), -104.311040)
          AND MetodoUbicacion = N'referencia_localidad_oficial_y_red'
    ) <> 1
        THROW 53601, N'Reversión detenida: el lote no identifica exactamente una fila esperada.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 72
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Latitud = CONVERT(decimal(9,6), 19.024860)
      AND Longitud = CONVERT(decimal(10,6), -104.311040)
      AND MetodoUbicacion = N'referencia_localidad_oficial_y_red';

    IF @@ROWCOUNT <> 1
        THROW 53602, N'Reversión: no se eliminó exactamente una fila.', 1;

    COMMIT TRANSACTION;
    SELECT N'OK' AS EstadoReversion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
