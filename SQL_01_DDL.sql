/* ==========================================================
DG MESNIE - Esquema SQL Server para CRUD de Proyectos,
Trámites Ambientales, Bitácora, Seguimiento y SharePoint
Esquema: dgmesnie
Generado desde workbook al 2026-05-24
========================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dgmesnie')
    EXEC('CREATE SCHEMA dgmesnie');
GO

CREATE TABLE dgmesnie.CatClasificacion (
    ClasificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dgmesnie.CatPrioridad (
    PrioridadId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Orden INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dgmesnie.CatSemaforo (
    SemaforoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    ColorHex NVARCHAR(10) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dgmesnie.CatTecnologia (
    TecnologiaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dgmesnie.CatEstatusTramite (
    EstatusTramiteId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dgmesnie.CatTipoDocumento (
    TipoDocumentoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE dgmesnie.CatValoracionMinuta (
    ValoracionMinutaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE dgmesnie.Empresa (
    EmpresaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL UNIQUE,
    GrupoEconomico NVARCHAR(250) NULL,
    PaisOrigen NVARCHAR(100) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL
);
GO

CREATE TABLE dgmesnie.Proyecto (
    ProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL,
    NombreNormalizado AS UPPER(LTRIM(RTRIM(Nombre))) PERSISTED,
    EmpresaId INT NULL,
    Promovente NVARCHAR(250) NULL,
    TecnologiaId INT NULL,
    CapacidadMW DECIMAL(12,3) NULL,
    Renovable BIT NULL,
    EntidadFederativa NVARCHAR(150) NULL,
    Municipio NVARCHAR(250) NULL,
    Latitud DECIMAL(18,10) NULL,
    Longitud DECIMAL(18,10) NULL,
    NumeroPermiso NVARCHAR(150) NULL,
    FolioEvIS_MISSE NVARCHAR(250) NULL,
    FechaInicioObras DATE NULL,
    FechaTerminacionObras DATE NULL,
    FechaEntradaOperacion DATE NULL,
    EstadoProgramaObras NVARCHAR(MAX) NULL,
    TramiteCNE NVARCHAR(MAX) NULL,
    ObservacionesCNE NVARCHAR(MAX) NULL,
    InteresadaEnContinuar BIT NULL,
    RequiereAlmacenamiento NVARCHAR(250) NULL,
    ResumenCaso NVARCHAR(MAX) NULL,
    PropuestaAtencion NVARCHAR(MAX) NULL,
    SiguientesPasos NVARCHAR(MAX) NULL,
    RiesgosObservaciones NVARCHAR(MAX) NULL,
    RazonesBreves NVARCHAR(MAX) NULL,
    ClasificacionId INT NULL,
    PrioridadId INT NULL,
    SemaforoId INT NULL,
    FuenteActual NVARCHAR(250) NULL,
    FechaUltimaActualizacion DATE NULL,
    FuenteUltimaActualizacion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Proyecto_Empresa FOREIGN KEY (EmpresaId) REFERENCES dgmesnie.Empresa(EmpresaId),
    CONSTRAINT FK_Proyecto_Tecnologia FOREIGN KEY (TecnologiaId) REFERENCES dgmesnie.CatTecnologia(TecnologiaId),
    CONSTRAINT FK_Proyecto_Clasificacion FOREIGN KEY (ClasificacionId) REFERENCES dgmesnie.CatClasificacion(ClasificacionId),
    CONSTRAINT FK_Proyecto_Prioridad FOREIGN KEY (PrioridadId) REFERENCES dgmesnie.CatPrioridad(PrioridadId),
    CONSTRAINT FK_Proyecto_Semaforo FOREIGN KEY (SemaforoId) REFERENCES dgmesnie.CatSemaforo(SemaforoId)
);
GO
CREATE INDEX IX_Proyecto_Nombre ON dgmesnie.Proyecto(Nombre);
CREATE INDEX IX_Proyecto_Empresa ON dgmesnie.Proyecto(EmpresaId);
CREATE INDEX IX_Proyecto_Clasificacion_Prioridad ON dgmesnie.Proyecto(ClasificacionId, PrioridadId, SemaforoId);
GO

CREATE TABLE dgmesnie.TramiteProyecto (
    TramiteProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    TipoTramite NVARCHAR(150) NOT NULL,
    Folio NVARCHAR(250) NULL,
    EstatusTramiteId INT NULL,
    EstatusTexto NVARCHAR(300) NULL,
    FechaIngreso DATE NULL,
    FechaResolucion DATE NULL,
    FechaVencimiento DATE NULL,
    Observaciones NVARCHAR(MAX) NULL,
    Autoridad NVARCHAR(150) NULL,
    Fuente NVARCHAR(300) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_TramiteProyecto_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.Proyecto(ProyectoId),
    CONSTRAINT FK_TramiteProyecto_Estatus FOREIGN KEY (EstatusTramiteId) REFERENCES dgmesnie.CatEstatusTramite(EstatusTramiteId)
);
GO
CREATE INDEX IX_TramiteProyecto_Proyecto ON dgmesnie.TramiteProyecto(ProyectoId);
GO

CREATE TABLE dgmesnie.Documento (
    DocumentoId INT IDENTITY(1,1) PRIMARY KEY,
    TipoDocumentoId INT NOT NULL,
    Titulo NVARCHAR(300) NOT NULL,
    SharePointUrl NVARCHAR(1000) NULL,
    SharePointItemId NVARCHAR(300) NULL,
    SharePointDriveId NVARCHAR(300) NULL,
    NombreArchivo NVARCHAR(300) NULL,
    FechaDocumento DATE NULL,
    Descripcion NVARCHAR(MAX) NULL,
    SubidoPor NVARCHAR(450) NULL,
    SubidoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Documento_Tipo FOREIGN KEY (TipoDocumentoId) REFERENCES dgmesnie.CatTipoDocumento(TipoDocumentoId)
);
GO

CREATE TABLE dgmesnie.Reunion (
    ReunionId INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(300) NOT NULL,
    FechaReunion DATE NOT NULL,
    Modalidad NVARCHAR(100) NULL,
    Lugar NVARCHAR(300) NULL,
    Objetivo NVARCHAR(MAX) NULL,
    DocumentoId INT NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Reunion_Documento FOREIGN KEY (DocumentoId) REFERENCES dgmesnie.Documento(DocumentoId)
);
GO

CREATE TABLE dgmesnie.BitacoraProyecto (
    BitacoraProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    ReunionId INT NULL,
    DocumentoId INT NULL,
    FechaEvento DATE NOT NULL,
    ValoracionMinutaId INT NULL,
    ValoracionTexto NVARCHAR(150) NULL,
    ResumenAcuerdos NVARCHAR(MAX) NULL,
    CompromisosSiguientesPasos NVARCHAR(MAX) NULL,
    RiesgosObservaciones NVARCHAR(MAX) NULL,
    ProcesadoPor NVARCHAR(450) NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Bitacora_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.Proyecto(ProyectoId),
    CONSTRAINT FK_Bitacora_Reunion FOREIGN KEY (ReunionId) REFERENCES dgmesnie.Reunion(ReunionId),
    CONSTRAINT FK_Bitacora_Documento FOREIGN KEY (DocumentoId) REFERENCES dgmesnie.Documento(DocumentoId),
    CONSTRAINT FK_Bitacora_Valoracion FOREIGN KEY (ValoracionMinutaId) REFERENCES dgmesnie.CatValoracionMinuta(ValoracionMinutaId)
);
GO
CREATE INDEX IX_Bitacora_Proyecto_Fecha ON dgmesnie.BitacoraProyecto(ProyectoId, FechaEvento DESC);
GO

CREATE TABLE dgmesnie.AccionSeguimiento (
    AccionId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    BitacoraProyectoId INT NULL,
    Titulo NVARCHAR(300) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    ResponsableUsuarioId NVARCHAR(450) NULL,
    ResponsableNombre NVARCHAR(250) NULL,
    FechaCompromiso DATE NULL,
    FechaCierre DATE NULL,
    Estatus NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    SemaforoId INT NULL,
    Comentarios NVARCHAR(MAX) NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Accion_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.Proyecto(ProyectoId),
    CONSTRAINT FK_Accion_Bitacora FOREIGN KEY (BitacoraProyectoId) REFERENCES dgmesnie.BitacoraProyecto(BitacoraProyectoId),
    CONSTRAINT FK_Accion_Semaforo FOREIGN KEY (SemaforoId) REFERENCES dgmesnie.CatSemaforo(SemaforoId)
);
GO

CREATE TABLE dgmesnie.HistorialProyecto (
    HistorialProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    BitacoraProyectoId INT NULL,
    DocumentoId INT NULL,
    Campo NVARCHAR(150) NOT NULL,
    ValorAnterior NVARCHAR(MAX) NULL,
    ValorNuevo NVARCHAR(MAX) NULL,
    MotivoCambio NVARCHAR(MAX) NULL,
    CambiadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CambiadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Historial_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.Proyecto(ProyectoId),
    CONSTRAINT FK_Historial_Bitacora FOREIGN KEY (BitacoraProyectoId) REFERENCES dgmesnie.BitacoraProyecto(BitacoraProyectoId),
    CONSTRAINT FK_Historial_Documento FOREIGN KEY (DocumentoId) REFERENCES dgmesnie.Documento(DocumentoId)
);
GO
CREATE INDEX IX_Historial_Proyecto ON dgmesnie.HistorialProyecto(ProyectoId, CambiadoEn DESC);
GO
