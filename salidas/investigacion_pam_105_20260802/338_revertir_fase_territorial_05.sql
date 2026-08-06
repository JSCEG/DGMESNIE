SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-05';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 68
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Latitud = CONVERT(decimal(9,6), 24.046717)
          AND Longitud = CONVERT(decimal(10,6), -110.294003)
          AND MetodoUbicacion = N'ubicacion_documental_oficial_utm'
    ) <> 1
        THROW 53301, N'Reversión detenida: el lote no identifica exactamente una fila esperada.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 68
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Latitud = CONVERT(decimal(9,6), 24.046717)
      AND Longitud = CONVERT(decimal(10,6), -110.294003)
      AND MetodoUbicacion = N'ubicacion_documental_oficial_utm';

    IF @@ROWCOUNT <> 1
        THROW 53302, N'Reversión: no se eliminó exactamente una fila.', 1;

    COMMIT TRANSACTION;
    SELECT N'OK' AS EstadoReversion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
