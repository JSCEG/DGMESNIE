SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dgmesnie.PAMLoteActualizacion', N'LoteUid') IS NULL
    BEGIN
        ALTER TABLE dgmesnie.PAMLoteActualizacion ADD LoteUid UNIQUEIDENTIFIER NULL;
        EXEC sys.sp_executesql N'UPDATE dgmesnie.PAMLoteActualizacion SET LoteUid = NEWID() WHERE LoteUid IS NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteActualizacion ALTER COLUMN LoteUid UNIQUEIDENTIFIER NOT NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteActualizacion
            ADD CONSTRAINT DF_PAMLote_LoteUid DEFAULT NEWSEQUENTIALID() FOR LoteUid;';
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMLoteActualizacion')
          AND name = N'UX_PAMLote_LoteUid'
    )
        EXEC sys.sp_executesql N'CREATE UNIQUE INDEX UX_PAMLote_LoteUid ON dgmesnie.PAMLoteActualizacion(LoteUid);';

    IF COL_LENGTH(N'dgmesnie.PAMLoteFuente', N'FirmaValidada') IS NULL
    BEGIN
        ALTER TABLE dgmesnie.PAMLoteFuente ADD FirmaValidada BIT NULL;
        EXEC sys.sp_executesql N'UPDATE dgmesnie.PAMLoteFuente SET FirmaValidada = 0 WHERE FirmaValidada IS NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteFuente ALTER COLUMN FirmaValidada BIT NOT NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteFuente
            ADD CONSTRAINT DF_PAMLoteFuente_Firma DEFAULT (0) FOR FirmaValidada;';
    END;

    IF COL_LENGTH(N'dgmesnie.PAMLoteFuente', N'EstadoExtraccion') IS NULL
    BEGIN
        ALTER TABLE dgmesnie.PAMLoteFuente ADD EstadoExtraccion NVARCHAR(30) NULL;
        EXEC sys.sp_executesql N'UPDATE dgmesnie.PAMLoteFuente SET EstadoExtraccion = N''Pendiente'' WHERE EstadoExtraccion IS NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteFuente ALTER COLUMN EstadoExtraccion NVARCHAR(30) NOT NULL;';
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteFuente
            ADD CONSTRAINT DF_PAMLoteFuente_EstadoExtraccion DEFAULT N''Pendiente'' FOR EstadoExtraccion;';
    END;

    IF COL_LENGTH(N'dgmesnie.PAMLoteFuente', N'MensajeExtraccion') IS NULL
        ALTER TABLE dgmesnie.PAMLoteFuente ADD MensajeExtraccion NVARCHAR(1000) NULL;

    IF OBJECT_ID(N'dgmesnie.CK_PAMLoteFuente_EstadoExtraccion', N'C') IS NULL
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMLoteFuente WITH CHECK
            ADD CONSTRAINT CK_PAMLoteFuente_EstadoExtraccion
            CHECK (EstadoExtraccion IN
                (N''Pendiente'', N''Extrayendo'', N''Extraído'', N''Requiere conversión'', N''Error''));';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    COL_LENGTH(N'dgmesnie.PAMLoteActualizacion', N'LoteUid') AS LoteUidConfigurado,
    COL_LENGTH(N'dgmesnie.PAMLoteFuente', N'EstadoExtraccion') AS EstadoExtraccionConfigurado;
