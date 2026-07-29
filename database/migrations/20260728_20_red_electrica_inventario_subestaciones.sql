SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'dgmesnie') IS NULL
    BEGIN
        EXEC(N'CREATE SCHEMA dgmesnie');
    END;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionInventario',
        N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.RedElectricaSubestacionInventario
        (
            RegistroClave           NVARCHAR(80) NOT NULL
                CONSTRAINT PK_RedElectricaSubestacionInventario
                PRIMARY KEY,
            UniversoClave           NVARCHAR(80) NOT NULL,
            FuenteClave             NVARCHAR(40) NOT NULL,
            ReferenciaClave         NVARCHAR(160) NOT NULL,
            VersionIdCanonica       BIGINT NULL,
            NodoCanonicoClave       NVARCHAR(64) NULL,
            Nombre                  NVARCHAR(500) NOT NULL,
            NombreNormalizado       NVARCHAR(500) NOT NULL,
            TensionKv               DECIMAL(9,3) NULL,
            NivelRed                NVARCHAR(30) NOT NULL,
            Latitud                 DECIMAL(10,7) NULL,
            Longitud                DECIMAL(11,7) NULL,
            Region                  NVARCHAR(150) NULL,
            Zona                    NVARCHAR(200) NULL,
            DivisionTarifaria       NVARCHAR(150) NULL,
            Operador                NVARCHAR(200) NULL,
            Fuente                  NVARCHAR(1000) NOT NULL,
            FuenteCoordenadas       NVARCHAR(40) NULL,
            Licencia                NVARCHAR(100) NULL,
            EstadoConciliacion      NVARCHAR(60) NOT NULL,
            EstadoValidacion        NVARCHAR(80) NOT NULL,
            MetadatosJson           NVARCHAR(MAX) NOT NULL,
            Activa                  BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionInv_Activa
                DEFAULT (1),
            PrimeraObservacionUtc   DATETIME2(3) NOT NULL,
            UltimaObservacionUtc    DATETIME2(3) NOT NULL,
            CONSTRAINT FK_RedElectricaSubestacionInv_Nodo
                FOREIGN KEY (VersionIdCanonica, NodoCanonicoClave)
                REFERENCES dgmesnie.RedElectricaNodo
                    (VersionId, NodoClave),
            CONSTRAINT CK_RedElectricaSubestacionInv_Fuente
                CHECK (FuenteClave IN
                    (
                        N'dgmesnie_geojson',
                        N'atlas_sen',
                        N'openstreetmap'
                    )),
            CONSTRAINT CK_RedElectricaSubestacionInv_Nivel
                CHECK (NivelRed IN
                    (
                        N'transmision',
                        N'subtransmision',
                        N'distribucion',
                        N'indeterminado'
                    )),
            CONSTRAINT CK_RedElectricaSubestacionInv_Coordenadas
                CHECK
                (
                    (Latitud IS NULL AND Longitud IS NULL)
                    OR
                    (
                        Latitud BETWEEN 10 AND 40
                        AND Longitud BETWEEN -125 AND -80
                    )
                ),
            CONSTRAINT CK_RedElectricaSubestacionInv_Metadatos
                CHECK (ISJSON(MetadatosJson) = 1)
        );

        CREATE INDEX IX_RedElectricaSubestacionInv_Universo
            ON dgmesnie.RedElectricaSubestacionInventario
                (Activa, UniversoClave);

        CREATE INDEX IX_RedElectricaSubestacionInv_Fuente
            ON dgmesnie.RedElectricaSubestacionInventario
                (Activa, FuenteClave, NivelRed);

        CREATE INDEX IX_RedElectricaSubestacionInv_Nombre
            ON dgmesnie.RedElectricaSubestacionInventario
                (Activa, NombreNormalizado, TensionKv);

        CREATE INDEX IX_RedElectricaSubestacionInv_Pendiente
            ON dgmesnie.RedElectricaSubestacionInventario
                (Activa, EstadoConciliacion)
            INCLUDE
                (
                    FuenteClave,
                    Nombre,
                    TensionKv,
                    Latitud,
                    Longitud
                );
    END;

    IF COL_LENGTH(
        N'dgmesnie.RedElectricaSubestacionInventario',
        N'FuenteCoordenadas') IS NULL
    BEGIN
        ALTER TABLE dgmesnie.RedElectricaSubestacionInventario
            ADD FuenteCoordenadas NVARCHAR(40) NULL;
    END;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionInventarioEjecucion',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionInventarioEjecucion
        (
            EjecucionId             BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_RedElectricaSubestacionInvEjecucion
                PRIMARY KEY,
            VersionIdCanonica       BIGINT NOT NULL,
            GeneradoUtc             DATETIME2(3) NOT NULL,
            RegistrosFuente         INT NOT NULL,
            RegistrosDgmesnie       INT NOT NULL,
            RegistrosAtlasSen       INT NOT NULL,
            RegistrosOpenStreetMap  INT NOT NULL,
            CONSTRAINT FK_RedElectricaSubestacionInvEjec_Version
                FOREIGN KEY (VersionIdCanonica)
                REFERENCES dgmesnie.RedElectricaVersion(VersionId)
        );

        CREATE INDEX IX_RedElectricaSubestacionInvEjec_Fecha
            ON dgmesnie.RedElectricaSubestacionInventarioEjecucion
                (GeneradoUtc DESC);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionInventario',
        N'U') AS InventarioObjectId,
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionInventarioEjecucion',
        N'U') AS EjecucionObjectId;
