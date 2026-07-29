SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaEjecucion',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionLineaEjecucion
        (
            EjecucionId             BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_RedElectricaSubestacionLineaEjecucion
                PRIMARY KEY,
            GeneradoUtc             DATETIME2(3) NOT NULL,
            Regla                   NVARCHAR(100) NOT NULL,
            RutaReporte             NVARCHAR(1000) NOT NULL,
            SubestacionesObjetivo   INT NOT NULL,
            ElementosLinea          INT NOT NULL,
            SubestacionesTocadas    INT NOT NULL,
            CoincidenciasFuertes    INT NOT NULL,
            CoincidenciasGeometricas INT NOT NULL,
            LineasCercanas          INT NOT NULL,
            RevisionEspacial        INT NOT NULL,
            SinLineaCercana         INT NOT NULL,
            GrafoModificado         BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubLineaEjec_Grafo
                DEFAULT (0),
            CreadoUtc               DATETIME2(3) NOT NULL
                CONSTRAINT DF_RedElectricaSubLineaEjec_Creado
                DEFAULT (SYSUTCDATETIME())
        );
    END;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaCandidato',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionLineaCandidato
        (
            EjecucionId             BIGINT NOT NULL,
            RegistroClave           NVARCHAR(80) NOT NULL,
            AristaClave             NVARCHAR(80) NOT NULL,
            ElementoCatalogoClave   NVARCHAR(80) NOT NULL,
            NombreSubestacion       NVARCHAR(500) NOT NULL,
            NombreLinea             NVARCHAR(500) NOT NULL,
            TensionKv               DECIMAL(9,3) NOT NULL,
            DistanciaKm             DECIMAL(10,4) NOT NULL,
            CoincidenciaNombre      BIT NOT NULL,
            ReglaNombre             NVARCHAR(80) NOT NULL,
            EstadoPropuesta         NVARCHAR(80) NOT NULL,
            EsPromovible            BIT NOT NULL,
            EvidenciasJson          NVARCHAR(MAX) NOT NULL,
            CreadoUtc               DATETIME2(3) NOT NULL
                CONSTRAINT DF_RedElectricaSubLineaCand_Creado
                DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT PK_RedElectricaSubestacionLineaCandidato
                PRIMARY KEY
                    (EjecucionId, RegistroClave, AristaClave),
            CONSTRAINT FK_RedElectricaSubLineaCand_Ejecucion
                FOREIGN KEY (EjecucionId)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionLineaEjecucion
                    (EjecucionId),
            CONSTRAINT FK_RedElectricaSubLineaCand_Inventario
                FOREIGN KEY (RegistroClave)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionInventario
                    (RegistroClave),
            CONSTRAINT CK_RedElectricaSubLineaCand_Distancia
                CHECK (DistanciaKm BETWEEN 0 AND 5),
            CONSTRAINT CK_RedElectricaSubLineaCand_Evidencias
                CHECK (ISJSON(EvidenciasJson) = 1)
        );

        CREATE INDEX IX_RedElectricaSubLineaCand_Revision
            ON dgmesnie.RedElectricaSubestacionLineaCandidato
                (
                    EjecucionId,
                    EstadoPropuesta,
                    EsPromovible,
                    RegistroClave
                )
            INCLUDE
                (
                    ElementoCatalogoClave,
                    AristaClave,
                    NombreLinea,
                    DistanciaKm
                );
    END;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaPromocion',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionLineaPromocion
        (
            RegistroClave           NVARCHAR(80) NOT NULL,
            ElementoCatalogoClave   NVARCHAR(80) NOT NULL,
            EjecucionId             BIGINT NOT NULL,
            AristaClave             NVARCHAR(80) NOT NULL,
            NombreLinea             NVARCHAR(500) NOT NULL,
            TensionKv               DECIMAL(9,3) NOT NULL,
            DistanciaKm             DECIMAL(10,4) NOT NULL,
            EstadoValidacion        NVARCHAR(80) NOT NULL,
            ReglaPromocion          NVARCHAR(100) NOT NULL,
            EsOficial               BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubLineaProm_Oficial
                DEFAULT (0),
            Activa                  BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubLineaProm_Activa
                DEFAULT (1),
            PromovidaUtc            DATETIME2(3) NOT NULL,
            CONSTRAINT PK_RedElectricaSubestacionLineaPromocion
                PRIMARY KEY
                    (RegistroClave, ElementoCatalogoClave),
            CONSTRAINT FK_RedElectricaSubLineaProm_Candidato
                FOREIGN KEY (EjecucionId, RegistroClave, AristaClave)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionLineaCandidato
                    (EjecucionId, RegistroClave, AristaClave)
        );

        CREATE INDEX IX_RedElectricaSubLineaProm_Activa
            ON dgmesnie.RedElectricaSubestacionLineaPromocion
                (Activa, RegistroClave)
            INCLUDE
                (
                    ElementoCatalogoClave,
                    AristaClave,
                    NombreLinea,
                    TensionKv,
                    DistanciaKm
                );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaEjecucion',
        N'U') AS EjecucionObjectId,
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaCandidato',
        N'U') AS CandidatoObjectId,
    OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionLineaPromocion',
        N'U') AS PromocionObjectId;
