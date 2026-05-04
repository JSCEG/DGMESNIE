IF SCHEMA_ID('dgmesnie') IS NULL
BEGIN
    EXEC('CREATE SCHEMA dgmesnie');
END
GO

IF OBJECT_ID('dgmesnie.InformePormenorizadoModernizacion', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.InformePormenorizadoModernizacion
    (
        ProyectoModernizacionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Numero INT NOT NULL,
        NumeroOriginal INT NULL,
        GRT NVARCHAR(100) NULL,
        NombreProyecto NVARCHAR(500) NOT NULL,
        TipoFinanciamiento NVARCHAR(150) NULL,
        AnioInstruccion INT NULL,
        EtapaProyecto NVARCHAR(150) NULL,
        MontoProyectoMdp DECIMAL(18,2) NULL,
        ElementosEquiposAsociados NVARCHAR(MAX) NULL,
        FechaEstimadaInicio NVARCHAR(100) NULL,
        FeoIndicadaOficioSener NVARCHAR(150) NULL,
        FeoFactible NVARCHAR(150) NULL,
        PorcentajeAvanceEjecucion DECIMAL(9,2) NULL,
        CircunstanciasAtrasos NVARCHAR(MAX) NULL,
        AccionesMitigacionCorreccion NVARCHAR(MAX) NULL,
        EstadoRealProyecto NVARCHAR(MAX) NULL,
        ComentariosNivelPriorizacion NVARCHAR(200) NULL,
        ClavePem NVARCHAR(200) NULL,
        ClasificacionSener NVARCHAR(200) NULL,
        FechaProgramacionTrimestre NVARCHAR(50) NULL,
        QuincenaPublicacion NVARCHAR(50) NULL,
        UniversoPresentacionPresidencia NVARCHAR(50) NULL,
        Mva DECIMAL(18,2) NULL,
        Mvar DECIMAL(18,2) NULL,
        KmC DECIMAL(18,2) NULL,
        FuenteArchivo NVARCHAR(260) NULL,
        FechaCarga DATETIME2(0) NOT NULL CONSTRAINT DF_InformePormenorizadoModernizacion_FechaCarga DEFAULT SYSUTCDATETIME(),
        FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_InformePormenorizadoModernizacion_FechaRegistro DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2(0) NULL,
        Activo BIT NOT NULL CONSTRAINT DF_InformePormenorizadoModernizacion_Activo DEFAULT 1
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_InformePormenorizadoModernizacion_Numero'
      AND object_id = OBJECT_ID('dgmesnie.InformePormenorizadoModernizacion')
)
BEGIN
    CREATE INDEX IX_InformePormenorizadoModernizacion_Numero
        ON dgmesnie.InformePormenorizadoModernizacion (Numero);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_InformePormenorizadoModernizacion_Etapa'
      AND object_id = OBJECT_ID('dgmesnie.InformePormenorizadoModernizacion')
)
BEGIN
    CREATE INDEX IX_InformePormenorizadoModernizacion_Etapa
        ON dgmesnie.InformePormenorizadoModernizacion (EtapaProyecto, TipoFinanciamiento, UniversoPresentacionPresidencia);
END
GO