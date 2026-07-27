/*
    DGMESNIE · Validación humana de evidencia de red de Segunda Convocatoria
    Historial inmutable: cada decisión nueva desactiva la vigente anterior y
    agrega un registro con usuario, fecha, fuente y fotografía de la evidencia.
*/

IF SCHEMA_ID('dgmesnie') IS NULL
BEGIN
    EXEC('CREATE SCHEMA dgmesnie');
END;

IF OBJECT_ID('dgmesnie.PamConvocatoriaValidacionRed', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.PamConvocatoriaValidacionRed
    (
        ValidacionId BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PamConvocatoriaValidacionRed PRIMARY KEY,
        CandidatoId NVARCHAR(80) NOT NULL,
        TipoElemento NVARCHAR(40) NOT NULL,
        Decision NVARCHAR(40) NOT NULL,
        NombreDeclarado NVARCHAR(600) NOT NULL,
        ClaveCatalogo NVARCHAR(180) NULL,
        CoincidenciaCatalogo NVARCHAR(600) NULL,
        Puntaje INT NOT NULL,
        TensionKv DECIMAL(10,3) NULL,
        Gcr NVARCHAR(120) NULL,
        Entidad NVARCHAR(180) NULL,
        Municipio NVARCHAR(240) NULL,
        Observacion NVARCHAR(2000) NULL,
        Fuente NVARCHAR(1200) NOT NULL,
        FoliosJson NVARCHAR(MAX) NOT NULL,
        EvidenciasJson NVARCHAR(MAX) NOT NULL,
        UsuarioId INT NULL,
        UsuarioNombre NVARCHAR(260) NOT NULL,
        UsuarioUnidad NVARCHAR(300) NULL,
        FechaDecisionUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_PamConvocatoriaValidacionRed_Fecha DEFAULT SYSUTCDATETIME(),
        Vigente BIT NOT NULL
            CONSTRAINT DF_PamConvocatoriaValidacionRed_Vigente DEFAULT 1,
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Tipo
            CHECK (TipoElemento IN (N'subestacion', N'linea_transmision')),
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Decision
            CHECK (Decision IN (
                N'confirmada',
                N'rechazada',
                N'faltante_confirmada',
                N'pendiente'
            )),
        CONSTRAINT CK_PamConvocatoriaValidacionRed_Puntaje
            CHECK (Puntaje BETWEEN 0 AND 100)
    );

    CREATE UNIQUE INDEX UX_PamConvocatoriaValidacionRed_Vigente
        ON dgmesnie.PamConvocatoriaValidacionRed(CandidatoId)
        WHERE Vigente = 1;

    CREATE INDEX IX_PamConvocatoriaValidacionRed_TipoDecision
        ON dgmesnie.PamConvocatoriaValidacionRed(TipoElemento, Decision, Vigente)
        INCLUDE (FechaDecisionUtc, UsuarioNombre);
END;
