SET XACT_ABORT ON;
GO

IF SCHEMA_ID(N'dgmesnie') IS NULL
    EXEC(N'CREATE SCHEMA dgmesnie');
GO

IF OBJECT_ID(N'dgmesnie.AzelPermisoElectricidadPublico', N'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.AzelPermisoElectricidadPublico
    (
        AzelPermisoId BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_AzelPermisoElectricidadPublico PRIMARY KEY,
        NumeroPermiso NVARCHAR(100) NULL,
        EfId NVARCHAR(206) NULL,
        MpoId NVARCHAR(308) NULL,
        RazonSocial NVARCHAR(2000) NULL,
        FechaOtorgamiento DATE NULL,
        LatitudGeo FLOAT NULL,
        LongitudGeo FLOAT NULL,
        Estatus NVARCHAR(100) NULL,
        EstatusInstalacion NVARCHAR(100) NULL,
        TipoPermiso NVARCHAR(100) NULL,
        InicioVigencia DATE NULL,
        InicioOperaciones DATE NULL,
        CapacidadAutorizadaMW FLOAT NULL,
        GeneracionEstimadaAnual FLOAT NULL,
        ActividadEconomica NVARCHAR(100) NULL,
        Tecnologia NVARCHAR(100) NULL,
        FuenteEnergia NVARCHAR(100) NULL,
        Clasificacion NVARCHAR(100) NULL,
        FechaSincronizacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_AzelPermisoElectricidadPublico_FechaSincronizacion DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_AzelPermisoElectricidadPublico_Clasificacion
        ON dgmesnie.AzelPermisoElectricidadPublico (Clasificacion);

    CREATE INDEX IX_AzelPermisoElectricidadPublico_NumeroPermiso
        ON dgmesnie.AzelPermisoElectricidadPublico (NumeroPermiso);
END;
GO

IF OBJECT_ID(N'dgmesnie.AzelCampoPublico', N'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.AzelCampoPublico
    (
        NombreCampo NVARCHAR(100) NOT NULL
            CONSTRAINT PK_AzelCampoPublico PRIMARY KEY,
        Orden SMALLINT NOT NULL,
        Activo BIT NOT NULL
            CONSTRAINT DF_AzelCampoPublico_Activo DEFAULT (1)
    );
END;
GO

MERGE dgmesnie.AzelCampoPublico AS destino
USING (VALUES
    (N'NumeroPermiso', 1),
    (N'RazonSocial', 2),
    (N'EfId', 3),
    (N'MpoId', 4),
    (N'FechaOtorgamiento', 5),
    (N'Estatus', 6),
    (N'EstatusInstalacion', 7),
    (N'TipoPermiso', 8),
    (N'InicioVigencia', 9),
    (N'InicioOperaciones', 10),
    (N'CapacidadAutorizadaMW', 11),
    (N'Generación_estimada_anual', 12),
    (N'Actividad_economica', 13),
    (N'Tecnología', 14),
    (N'FuenteEnergía', 15)
) AS fuente (NombreCampo, Orden)
ON destino.NombreCampo = fuente.NombreCampo
WHEN MATCHED THEN
    UPDATE SET Orden = fuente.Orden, Activo = 1
WHEN NOT MATCHED THEN
    INSERT (NombreCampo, Orden, Activo)
    VALUES (fuente.NombreCampo, fuente.Orden, 1);
GO

CREATE OR ALTER PROCEDURE dgmesnie.sp_AzelPublico_Sincronizar
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    TRUNCATE TABLE dgmesnie.AzelPermisoElectricidadPublico;

    INSERT INTO dgmesnie.AzelPermisoElectricidadPublico
    (
        NumeroPermiso,
        EfId,
        MpoId,
        RazonSocial,
        FechaOtorgamiento,
        LatitudGeo,
        LongitudGeo,
        Estatus,
        EstatusInstalacion,
        TipoPermiso,
        InicioVigencia,
        InicioOperaciones,
        CapacidadAutorizadaMW,
        GeneracionEstimadaAnual,
        ActividadEconomica,
        Tecnologia,
        FuenteEnergia,
        Clasificacion
    )
    SELECT
        NumeroPermiso,
        EfId,
        MpoId,
        RazonSocial,
        FechaOtorgamiento,
        LatitudGeo,
        LongitudGeo,
        Estatus,
        EstatusInstalacion,
        TipoPermiso,
        InicioVigencia,
        InicioOperaciones,
        CapacidadAutorizadaMW,
        [Generación_estimada_anual],
        [Actividad_economica],
        [Tecnología],
        [FuenteEnergía],
        [Clasifica_Menú]
    FROM dbo.vElectricidad_autorizado_mapa;

    COMMIT TRANSACTION;

    SELECT COUNT_BIG(*) AS FilasSincronizadas
    FROM dgmesnie.AzelPermisoElectricidadPublico;
END;
GO

CREATE OR ALTER PROCEDURE dgmesnie.sp_AzelPublico_ObtenerCampos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT NombreCampo
    FROM dgmesnie.AzelCampoPublico
    WHERE Activo = 1
    ORDER BY Orden;
END;
GO

CREATE OR ALTER PROCEDURE dgmesnie.sp_AzelPublico_ObtenerPermisos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NumeroPermiso,
        EfId,
        MpoId,
        RazonSocial,
        FechaOtorgamiento,
        LatitudGeo,
        LongitudGeo,
        Estatus,
        EstatusInstalacion,
        TipoPermiso,
        InicioVigencia,
        InicioOperaciones,
        CapacidadAutorizadaMW,
        GeneracionEstimadaAnual,
        ActividadEconomica,
        Tecnologia,
        FuenteEnergia,
        Clasificacion
    FROM dgmesnie.AzelPermisoElectricidadPublico
    WHERE LatitudGeo IS NOT NULL
      AND LongitudGeo IS NOT NULL;
END;
GO

EXEC dgmesnie.sp_AzelPublico_Sincronizar;
GO
