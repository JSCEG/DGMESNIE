SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-04';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 39
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND Latitud = CONVERT(decimal(9,6), 22.094586)
          AND Longitud = CONVERT(decimal(10,6), -100.907603)
          AND MetodoUbicacion = N'conciliacion_documental_oficial_topologica'
    ) <> 1
        THROW 53001, N'Reversión detenida: el lote no identifica exactamente una fila esperada.', 1;

    DELETE FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 39
      AND Observaciones LIKE N'%' + @Lote + N'%'
      AND Latitud = CONVERT(decimal(9,6), 22.094586)
      AND Longitud = CONVERT(decimal(10,6), -100.907603)
      AND MetodoUbicacion = N'conciliacion_documental_oficial_topologica';

    IF @@ROWCOUNT <> 1
        THROW 53002, N'Reversión: no se eliminó exactamente una fila.', 1;

    COMMIT TRANSACTION;
    SELECT N'OK' AS EstadoReversion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
