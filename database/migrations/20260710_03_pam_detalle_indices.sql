SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMCambio')
          AND name = N'IX_PAMCambio_ProyectoFecha'
    )
    BEGIN
        CREATE INDEX IX_PAMCambio_ProyectoFecha
            ON dgmesnie.PAMCambio(ProyectoId, FechaRegistroUtc DESC)
            INCLUDE (TipoCambio, Campo, CargaId, ProyectoVersionAnteriorId, ProyectoVersionNuevaId);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

EXEC sys.sp_refreshview N'dgmesnie.vw_PAMProyectoHistorico';

SELECT
    i.name AS Indice,
    OBJECT_SCHEMA_NAME(i.object_id) + N'.' + OBJECT_NAME(i.object_id) AS Tabla
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'dgmesnie.PAMCambio')
  AND i.name = N'IX_PAMCambio_ProyectoFecha';
