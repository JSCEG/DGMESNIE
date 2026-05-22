SET NOCOUNT ON;
SET XACT_ABORT ON;

IF SCHEMA_ID('dgmesnie') IS NULL
BEGIN
    EXEC('CREATE SCHEMA dgmesnie');
END
GO

IF OBJECT_ID('dgmesnie.PvirseCentralesElectricas', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.PvirseCentralesElectricas
    (
        PvirseRegistroId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Status NVARCHAR(200) NULL,
        StatusVf NVARCHAR(200) NULL,
        NombreReal NVARCHAR(500) NOT NULL,
        NoConsiderar NVARCHAR(100) NULL,
        Anio INT NULL,
        AdicionesOSustituciones NVARCHAR(100) NULL,
        ContratoOUnidad NVARCHAR(100) NULL,
        Tipo NVARCHAR(100) NULL,
        TipoVf NVARCHAR(100) NULL,
        Renovable NVARCHAR(100) NULL,
        Mw DECIMAL(18,2) NULL,
        Mes NVARCHAR(50) NULL,
        GerenciaDeControl NVARCHAR(100) NULL,
        RegionDeTransmision NVARCHAR(150) NULL,
        EntidadFederativa NVARCHAR(150) NULL,
        Municipio NVARCHAR(150) NULL,
        Firmes NVARCHAR(20) NULL,
        FuenteArchivo NVARCHAR(260) NULL,
        FechaCarga DATETIME2 NOT NULL CONSTRAINT DF_PvirseCentralesElectricas_FechaCarga DEFAULT SYSUTCDATETIME(),
        Activo BIT NOT NULL CONSTRAINT DF_PvirseCentralesElectricas_Activo DEFAULT 1
    );

    CREATE INDEX IX_PvirseCentralesElectricas_Entidad_Status
        ON dgmesnie.PvirseCentralesElectricas(EntidadFederativa, StatusVf, Status);
END
GO