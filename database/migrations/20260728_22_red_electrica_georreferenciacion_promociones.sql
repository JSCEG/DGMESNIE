SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(
        N'dgmesnie.RedElectricaSubestacionGeorefPromocion',
        N'U') IS NULL
    BEGIN
        CREATE TABLE
            dgmesnie.RedElectricaSubestacionGeorefPromocion
        (
            RegistroClave           NVARCHAR(80) NOT NULL
                CONSTRAINT PK_RedElectricaSubestacionGeorefPromocion
                PRIMARY KEY,
            EjecucionId             BIGINT NOT NULL,
            ReferenciaOsm           NVARCHAR(80) NOT NULL,
            Latitud                 DECIMAL(10,7) NOT NULL,
            Longitud                DECIMAL(11,7) NOT NULL,
            Puntaje                 DECIMAL(5,2) NOT NULL,
            FuenteCoordenadas       NVARCHAR(40) NOT NULL,
            LicenciaCoordenadas     NVARCHAR(100) NOT NULL,
            EstadoValidacion        NVARCHAR(80) NOT NULL,
            ReglaPromocion          NVARCHAR(100) NOT NULL,
            EsOficial               BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionGeorefProm_Oficial
                DEFAULT (0),
            Activa                  BIT NOT NULL
                CONSTRAINT DF_RedElectricaSubestacionGeorefProm_Activa
                DEFAULT (1),
            PromovidaUtc            DATETIME2(3) NOT NULL,
            UltimaAplicacionUtc     DATETIME2(3) NOT NULL,
            CONSTRAINT FK_RedElectricaSubestacionGeorefProm_Candidato
                FOREIGN KEY
                    (EjecucionId, RegistroClave, ReferenciaOsm)
                REFERENCES
                    dgmesnie.RedElectricaSubestacionGeorefCandidato
                    (EjecucionId, RegistroClave, ReferenciaOsm),
            CONSTRAINT CK_RedElectricaSubestacionGeorefProm_Coordenadas
                CHECK
                (
                    Latitud BETWEEN 10 AND 40
                    AND Longitud BETWEEN -125 AND -80
                ),
            CONSTRAINT CK_RedElectricaSubestacionGeorefProm_Puntaje
                CHECK (Puntaje BETWEEN 90 AND 100)
        );

        CREATE INDEX IX_RedElectricaSubestacionGeorefProm_Activa
            ON dgmesnie.RedElectricaSubestacionGeorefPromocion
                (Activa, EjecucionId, Puntaje DESC)
            INCLUDE
                (
                    RegistroClave,
                    ReferenciaOsm,
                    Latitud,
                    Longitud
                );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT OBJECT_ID(
    N'dgmesnie.RedElectricaSubestacionGeorefPromocion',
    N'U') AS PromocionObjectId;
