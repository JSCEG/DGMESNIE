SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @LockResult INT;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'dgmesnie.CarteraConvocatoria.migrations',
    @LockMode = N'Exclusive',
    @LockOwner = N'Session',
    @LockTimeout = 30000;

IF @LockResult < 0
    THROW 51000, N'No fue posible obtener el bloqueo de migración de la cartera.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.CatConsideracionConvocatoria', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CatConsideracionConvocatoria
        (
            Clave NVARCHAR(20) NOT NULL CONSTRAINT PK_CatConsideracionConvocatoria PRIMARY KEY,
            Nombre NVARCHAR(80) NOT NULL,
            IncluyeEnCarteraFirme BIT NOT NULL,
            Orden TINYINT NOT NULL,
            Activo BIT NOT NULL CONSTRAINT DF_CatConsideracionConvocatoria_Activo DEFAULT (1)
        );
    END;

    MERGE dgmesnie.CatConsideracionConvocatoria AS target
    USING (VALUES
        (N'firme', N'Firme / considerar', CONVERT(BIT, 1), CONVERT(TINYINT, 1)),
        (N'revision', N'En revisión', CONVERT(BIT, 0), CONVERT(TINYINT, 2)),
        (N'no-va', N'No va', CONVERT(BIT, 0), CONVERT(TINYINT, 3))
    ) AS source(Clave, Nombre, IncluyeEnCarteraFirme, Orden)
    ON target.Clave = source.Clave
    WHEN MATCHED THEN UPDATE SET
        Nombre = source.Nombre,
        IncluyeEnCarteraFirme = source.IncluyeEnCarteraFirme,
        Orden = source.Orden,
        Activo = 1
    WHEN NOT MATCHED THEN INSERT (Clave, Nombre, IncluyeEnCarteraFirme, Orden)
        VALUES (source.Clave, source.Nombre, source.IncluyeEnCarteraFirme, source.Orden);

    IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaCarga', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CarteraConvocatoriaCarga
        (
            CargaId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarteraConvocatoriaCarga PRIMARY KEY,
            NombreArchivo NVARCHAR(260) NOT NULL,
            ArchivoBytes BIGINT NOT NULL,
            Sha256 CHAR(64) NOT NULL,
            VersionFuente NVARCHAR(200) NOT NULL,
            FechaCorte DATE NULL,
            FilasFuente INT NOT NULL,
            FilasCatalogo INT NOT NULL,
            Firmes INT NOT NULL,
            EnRevision INT NOT NULL,
            NoVan INT NOT NULL,
            KmlProyecto INT NOT NULL,
            KmlSubestacion INT NOT NULL,
            Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_CarteraConvocatoriaCarga_Estado DEFAULT N'completada',
            ImportadoUtc DATETIME2(0) NOT NULL CONSTRAINT DF_CarteraConvocatoriaCarga_Fecha DEFAULT SYSUTCDATETIME(),
            ImportadoPor NVARCHAR(150) NULL,
            CONSTRAINT UQ_CarteraConvocatoriaCarga_Sha256 UNIQUE (Sha256),
            CONSTRAINT CK_CarteraConvocatoriaCarga_Estado CHECK (Estado IN (N'completada', N'anulada'))
        );
        CREATE INDEX IX_CarteraConvocatoriaCarga_Fecha
            ON dgmesnie.CarteraConvocatoriaCarga(ImportadoUtc DESC, CargaId DESC);
    END;

    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'ConsideracionClave') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD ConsideracionClave NVARCHAR(20) NOT NULL
            CONSTRAINT DF_CarteraConvocatoriaProyecto_Consideracion DEFAULT N'revision';
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'FolioCanonico') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD FolioCanonico NVARCHAR(60) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'EstatusUniverso') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD EstatusUniverso NVARCHAR(80) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'Municipio') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD Municipio NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'Latitud') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD Latitud DECIMAL(10,7) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'Longitud') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD Longitud DECIMAL(11,7) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'LatitudSubestacion') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD LatitudSubestacion DECIMAL(10,7) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'LongitudSubestacion') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD LongitudSubestacion DECIMAL(11,7) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'UrlKmlProyecto') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD UrlKmlProyecto NVARCHAR(1000) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'UrlKmlSubestacion') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD UrlKmlSubestacion NVARCHAR(1000) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'CostoRedMdd') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD CostoRedMdd DECIMAL(18,4) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'CostoRedMdp') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD CostoRedMdp DECIMAL(18,4) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'NumeroObras') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD NumeroObras INT NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'Obras') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD Obras NVARCHAR(MAX) NULL;
    IF COL_LENGTH(N'dgmesnie.CarteraConvocatoriaProyecto', N'CargaId') IS NULL
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD CargaId BIGINT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CarteraConvocatoriaProyecto_Consideracion')
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD CONSTRAINT FK_CarteraConvocatoriaProyecto_Consideracion
            FOREIGN KEY (ConsideracionClave) REFERENCES dgmesnie.CatConsideracionConvocatoria(Clave);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CarteraConvocatoriaProyecto_Carga')
        ALTER TABLE dgmesnie.CarteraConvocatoriaProyecto ADD CONSTRAINT FK_CarteraConvocatoriaProyecto_Carga
            FOREIGN KEY (CargaId) REFERENCES dgmesnie.CarteraConvocatoriaCarga(CargaId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CarteraConvocatoriaProyecto_Consideracion' AND object_id = OBJECT_ID(N'dgmesnie.CarteraConvocatoriaProyecto'))
        CREATE INDEX IX_CarteraConvocatoriaProyecto_Consideracion
            ON dgmesnie.CarteraConvocatoriaProyecto(Activo, ConsideracionClave, GerenciaControl, EntidadFederativa);

    IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaProyectoVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CarteraConvocatoriaProyectoVersion
        (
            ProyectoVersionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarteraConvocatoriaProyectoVersion PRIMARY KEY,
            CargaId BIGINT NOT NULL,
            Folio NVARCHAR(60) NOT NULL,
            FolioCanonico NVARCHAR(60) NULL,
            Nombre NVARCHAR(500) NOT NULL,
            ConsideracionClave NVARCHAR(20) NOT NULL,
            EstatusUniverso NVARCHAR(80) NULL,
            GerenciaControl NVARCHAR(100) NOT NULL,
            EntidadFederativa NVARCHAR(100) NOT NULL,
            Municipio NVARCHAR(200) NULL,
            Tecnologia NVARCHAR(150) NULL,
            RazonSocial NVARCHAR(500) NULL,
            GrupoInteres NVARCHAR(500) NULL,
            Subestacion NVARCHAR(500) NULL,
            PuntoInterconexion NVARCHAR(500) NULL,
            CapacidadMw DECIMAL(18,3) NOT NULL,
            OrdenPrelacion INT NOT NULL,
            Latitud DECIMAL(10,7) NULL,
            Longitud DECIMAL(11,7) NULL,
            LatitudSubestacion DECIMAL(10,7) NULL,
            LongitudSubestacion DECIMAL(11,7) NULL,
            UrlKmlProyecto NVARCHAR(1000) NULL,
            UrlKmlSubestacion NVARCHAR(1000) NULL,
            CostoRedMdd DECIMAL(18,4) NULL,
            CostoRedMdp DECIMAL(18,4) NULL,
            NumeroObras INT NULL,
            Obras NVARCHAR(MAX) NULL,
            FilaFuente INT NULL,
            RegistradoUtc DATETIME2(0) NOT NULL CONSTRAINT DF_CarteraConvocatoriaProyectoVersion_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT UQ_CarteraConvocatoriaProyectoVersion_CargaFolio UNIQUE (CargaId, Folio),
            CONSTRAINT FK_CarteraConvocatoriaProyectoVersion_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.CarteraConvocatoriaCarga(CargaId),
            CONSTRAINT FK_CarteraConvocatoriaProyectoVersion_Consideracion FOREIGN KEY (ConsideracionClave) REFERENCES dgmesnie.CatConsideracionConvocatoria(Clave)
        );
        CREATE INDEX IX_CarteraConvocatoriaProyectoVersion_Folio
            ON dgmesnie.CarteraConvocatoriaProyectoVersion(Folio, CargaId DESC);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.CarteraConvocatoriaCarga', N'U') AS CargaObjectId,
    OBJECT_ID(N'dgmesnie.CarteraConvocatoriaProyectoVersion', N'U') AS VersionObjectId;
