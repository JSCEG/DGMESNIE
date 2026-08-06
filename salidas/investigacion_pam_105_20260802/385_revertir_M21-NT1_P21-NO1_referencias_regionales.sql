SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-REGIONAL-20260806-02';

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (175, 199)
          AND MetodoUbicacion = N'localidad_area_influencia_documentada'
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 3
        THROW 54671, N'Reversión: no se encontraron exactamente los tres registros esperados.', 1;

    DELETE dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId IN (175, 199)
      AND MetodoUbicacion = N'localidad_area_influencia_documentada'
      AND Observaciones LIKE N'%' + @Lote + N'%';

    IF @@ROWCOUNT <> 3
        THROW 54672, N'Reversión: no se eliminaron exactamente tres registros.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT COUNT(*) AS RegistrosRestantesDelLote
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%';
