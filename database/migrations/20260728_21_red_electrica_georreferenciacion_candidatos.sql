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
        N'dgmesnie.RedElectricaSubestacionGeorefEjecucion',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionGeorefEjecucion
        (
            EjecucionId             BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_RedElectricaSubestacionGeorefEjecucion
                PRIMARY KEY,
            GeneradoUtc             DATETIME2(3) NOT NULL,
            Regla                   NVARCHAR(100) NOT NULL,
            RutaReporte             NVARCHAR(1000) NOT NULL,
            RegistrosObjetivo       INT NOT NULL,
            IdentidadesObjetivo     INT NOT NULL,
            ElementosOsm            INT NOT NULL,
            CandidatosOsmConNombre  INT NOT NULL,
            AltaConfianza           INT NOT NULL,
            Revision                INT NOT NULL,
            Ambiguas                INT NOT NULL,
            Debiles                 INT NOT NULL,
            SinCoincidencia         INT NOT NULL,
            FuenteOsm               NVARCHAR(1000) NOT NULL,
            FuenteGerencias         NVARCHAR(1000) NOT NULL,
            InventarioModificado    BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionGeorefEjec_Mod
                DEFAULT (0),
            CreadoUtc               DATETIME2(3) NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionGeorefEjec_Creado
                DEFAULT (SYSUTCDATETIME())
        );

        CREATE INDEX IX_RedElectricaSubestacionGeorefEjec_Fecha
            ON dgmesnie.RedElectricaSubestacionGeorefEjecucion
                (GeneradoUtc DESC);
    END;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionGeorefCandidato',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionGeorefCandidato
        (
            EjecucionId             BIGINT NOT NULL,
            RegistroClave           NVARCHAR(80) NOT NULL,
            ReferenciaOsm           NVARCHAR(80) NOT NULL,
            IdentidadObjetivo       NVARCHAR(1000) NOT NULL,
            NombreAtlas             NVARCHAR(500) NOT NULL,
            TensionAtlasKv          DECIMAL(9,3) NULL,
            RegionAtlas             NVARCHAR(150) NULL,
            ZonaAtlas               NVARCHAR(200) NULL,
            NombreOsm               NVARCHAR(500) NOT NULL,
            TensionOsmKv            DECIMAL(9,3) NULL,
            RegionOsm               NVARCHAR(200) NULL,
            OperadorOsm             NVARCHAR(200) NULL,
            Latitud                 DECIMAL(10,7) NOT NULL,
            Longitud                DECIMAL(11,7) NOT NULL,
            Puntaje                 DECIMAL(5,2) NOT NULL,
            Margen                  DECIMAL(5,2) NOT NULL,
            EstadoPropuesta         NVARCHAR(60) NOT NULL,
            EsPrincipal             BIT NOT NULL,
            EvidenciasJson          NVARCHAR(MAX) NOT NULL,
            CreadoUtc               DATETIME2(3) NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionGeorefCand_Creado
                DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT PK_RedElectricaSubestacionGeorefCandidato
                PRIMARY KEY
                    (EjecucionId, RegistroClave, ReferenciaOsm),
            CONSTRAINT FK_RedElectricaSubestacionGeorefCand_Ejec
                FOREIGN KEY (EjecucionId)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionGeorefEjecucion
                    (EjecucionId),
            CONSTRAINT FK_RedElectricaSubestacionGeorefCand_Inventario
                FOREIGN KEY (RegistroClave)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionInventario
                    (RegistroClave),
            CONSTRAINT CK_RedElectricaSubestacionGeorefCand_Coordenadas
                CHECK
                (
                    Latitud BETWEEN 10 AND 40
                    AND Longitud BETWEEN -125 AND -80
                ),
            CONSTRAINT CK_RedElectricaSubestacionGeorefCand_Puntaje
                CHECK (Puntaje BETWEEN 0 AND 100),
            CONSTRAINT CK_RedElectricaSubestacionGeorefCand_Evidencias
                CHECK (ISJSON(EvidenciasJson) = 1)
        );

        CREATE INDEX IX_RedElectricaSubestacionGeorefCand_Revision
            ON dgmesnie.RedElectricaSubestacionGeorefCandidato
                (EjecucionId, EstadoPropuesta, EsPrincipal, Puntaje DESC)
            INCLUDE
                (
                    RegistroClave,
                    NombreAtlas,
                    NombreOsm,
                    Latitud,
                    Longitud
                );

        CREATE INDEX IX_RedElectricaSubestacionGeorefCand_Referencia
            ON dgmesnie.RedElectricaSubestacionGeorefCandidato
                (ReferenciaOsm, EjecucionId DESC);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionGeorefEjecucion',
        N'U') AS EjecucionObjectId,
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionGeorefCandidato',
        N'U') AS CandidatoObjectId;
