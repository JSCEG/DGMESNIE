SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-07';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 78
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Latitud = CONVERT(decimal(9,6), 25.841410)
          AND Longitud = CONVERT(decimal(10,6), -109.019200)
          AND MetodoUbicacion = N'referencia_localidad_oficial_y_diagrama_red'
    ) <> 1
        THROW 53901, N'Reversión detenida: el lote no identifica exactamente una fila esperada.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 78
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Latitud = CONVERT(decimal(9,6), 25.841410)
      AND Longitud = CONVERT(decimal(10,6), -109.019200)
      AND MetodoUbicacion = N'referencia_localidad_oficial_y_diagrama_red';

    IF @@ROWCOUNT <> 1
        THROW 53902, N'Reversión: no se eliminó exactamente una fila.', 1;

    COMMIT TRANSACTION;
    SELECT N'OK' AS EstadoReversion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
