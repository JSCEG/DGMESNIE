/* ==========================================================
   DG MESNIE - SISTEMA DE CONSOLIDACIÓN DE PROYECTOS ENERGÉTICOS
   SCRIPT SQL DDL: CREACIÓN DE ESQUEMAS CORE Y STAGING E INSERCIÓN DE CATÁLOGOS
   ========================================================== */

-- Crear esquemas
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'core')
    EXEC('CREATE SCHEMA core');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'staging')
    EXEC('CREATE SCHEMA staging');
GO

-- Drop staging tables
IF OBJECT_ID('staging.ExcelRaw_VUPEWorksheet', 'U') IS NOT NULL DROP TABLE staging.ExcelRaw_VUPEWorksheet;
IF OBJECT_ID('staging.ExcelRaw_BDCompleta', 'U') IS NOT NULL DROP TABLE staging.ExcelRaw_BDCompleta;
IF OBJECT_ID('staging.ExcelRaw_PVIRCE', 'U') IS NOT NULL DROP TABLE staging.ExcelRaw_PVIRCE;
IF OBJECT_ID('staging.ExcelRaw_CNERPyT', 'U') IS NOT NULL DROP TABLE staging.ExcelRaw_CNERPyT;
IF OBJECT_ID('staging.ExcelRaw_InformePormenorizado', 'U') IS NOT NULL DROP TABLE staging.ExcelRaw_InformePormenorizado;

-- Drop core operational tables
IF OBJECT_ID('core.ProyectoBitacoraCarga', 'U') IS NOT NULL DROP TABLE core.ProyectoBitacoraCarga;
IF OBJECT_ID('core.HistorialProyecto', 'U') IS NOT NULL DROP TABLE core.HistorialProyecto;
IF OBJECT_ID('core.AccionSeguimiento', 'U') IS NOT NULL DROP TABLE core.AccionSeguimiento;
IF OBJECT_ID('core.BitacoraProyecto', 'U') IS NOT NULL DROP TABLE core.BitacoraProyecto;
IF OBJECT_ID('core.Reunion', 'U') IS NOT NULL DROP TABLE core.Reunion;
IF OBJECT_ID('core.Documento', 'U') IS NOT NULL DROP TABLE core.Documento;
IF OBJECT_ID('core.ProyectoHito', 'U') IS NOT NULL DROP TABLE core.ProyectoHito;
IF OBJECT_ID('core.ProyectoTramite', 'U') IS NOT NULL DROP TABLE core.ProyectoTramite;
IF OBJECT_ID('core.ProyectoDatosFinancieros', 'U') IS NOT NULL DROP TABLE core.ProyectoDatosFinancieros;
IF OBJECT_ID('core.ProyectoDatosTecnicos', 'U') IS NOT NULL DROP TABLE core.ProyectoDatosTecnicos;
IF OBJECT_ID('core.ProyectoGeometria', 'U') IS NOT NULL DROP TABLE core.ProyectoGeometria;
IF OBJECT_ID('core.ProyectoCoordenada', 'U') IS NOT NULL DROP TABLE core.ProyectoCoordenada;
IF OBJECT_ID('core.ProyectoUbicacion', 'U') IS NOT NULL DROP TABLE core.ProyectoUbicacion;
IF OBJECT_ID('core.ProyectoActor', 'U') IS NOT NULL DROP TABLE core.ProyectoActor;
IF OBJECT_ID('core.Actor', 'U') IS NOT NULL DROP TABLE core.Actor;
IF OBJECT_ID('core.GrupoInteresEconomico', 'U') IS NOT NULL DROP TABLE core.GrupoInteresEconomico;
IF OBJECT_ID('core.ProyectoIdentificador', 'U') IS NOT NULL DROP TABLE core.ProyectoIdentificador;
IF OBJECT_ID('core.Proyecto', 'U') IS NOT NULL DROP TABLE core.Proyecto;

-- Drop core catalog tables
IF OBJECT_ID('core.CatMunicipio', 'U') IS NOT NULL DROP TABLE core.CatMunicipio;
IF OBJECT_ID('core.CatEntidadFederativa', 'U') IS NOT NULL DROP TABLE core.CatEntidadFederativa;
IF OBJECT_ID('core.CatMoneda', 'U') IS NOT NULL DROP TABLE core.CatMoneda;
IF OBJECT_ID('core.CatTipoActor', 'U') IS NOT NULL DROP TABLE core.CatTipoActor;
IF OBJECT_ID('core.CatEstatusTramite', 'U') IS NOT NULL DROP TABLE core.CatEstatusTramite;
IF OBJECT_ID('core.CatTipoTramite', 'U') IS NOT NULL DROP TABLE core.CatTipoTramite;
IF OBJECT_ID('core.CatAutoridad', 'U') IS NOT NULL DROP TABLE core.CatAutoridad;
IF OBJECT_ID('core.CatSemaforo', 'U') IS NOT NULL DROP TABLE core.CatSemaforo;
IF OBJECT_ID('core.CatPrioridad', 'U') IS NOT NULL DROP TABLE core.CatPrioridad;
IF OBJECT_ID('core.CatClasificacion', 'U') IS NOT NULL DROP TABLE core.CatClasificacion;
IF OBJECT_ID('core.CatNivelMadurez', 'U') IS NOT NULL DROP TABLE core.CatNivelMadurez;
IF OBJECT_ID('core.CatEstatusProyecto', 'U') IS NOT NULL DROP TABLE core.CatEstatusProyecto;
IF OBJECT_ID('core.CatTecnologia', 'U') IS NOT NULL DROP TABLE core.CatTecnologia;
IF OBJECT_ID('core.CatOrigenDatos', 'U') IS NOT NULL DROP TABLE core.CatOrigenDatos;
IF OBJECT_ID('core.CatValoracionMinuta', 'U') IS NOT NULL DROP TABLE core.CatValoracionMinuta;
IF OBJECT_ID('core.CatTipoDocumento', 'U') IS NOT NULL DROP TABLE core.CatTipoDocumento;
GO

/* ==========================================================
   1. TABLAS DE CATÁLOGOS (core)
   ========================================================== */

CREATE TABLE core.CatTecnologia (
    TecnologiaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    EsRenovable BIT NOT NULL DEFAULT 1,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.CatEstatusProyecto (
    EstatusProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(300) NULL
);

CREATE TABLE core.CatNivelMadurez (
    NivelMadurezId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Orden INT NOT NULL
);

CREATE TABLE core.CatClasificacion (
    ClasificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.CatPrioridad (
    PrioridadId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Orden INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.CatSemaforo (
    SemaforoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    ColorHex NVARCHAR(10) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.CatAutoridad (
    AutoridadId INT IDENTITY(1,1) PRIMARY KEY,
    Acronimo NVARCHAR(30) NOT NULL UNIQUE,
    NombreCompleto NVARCHAR(250) NOT NULL
);

CREATE TABLE core.CatTipoTramite (
    TipoTramiteId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL
);

CREATE TABLE core.CatEstatusTramite (
    EstatusTramiteId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.CatTipoActor (
    TipoActorId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE core.CatMoneda (
    MonedaId INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(3) NOT NULL UNIQUE,
    Nombre NVARCHAR(50) NOT NULL
);

CREATE TABLE core.CatEntidadFederativa (
    EntidadFederativaId INT PRIMARY KEY, -- Clave INEGI (e.g. 19 para Nuevo León)
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE core.CatMunicipio (
    MunicipioId INT PRIMARY KEY, -- Clave INEGI compuesta (e.g. 19039)
    EntidadFederativaId INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    CONSTRAINT FK_CatMunicipio_Entidad FOREIGN KEY (EntidadFederativaId) REFERENCES core.CatEntidadFederativa(EntidadFederativaId)
);

CREATE TABLE core.CatOrigenDatos (
    OrigenDatosId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL UNIQUE,
    TipoFuente NVARCHAR(100) NOT NULL
);

CREATE TABLE core.CatValoracionMinuta (
    ValoracionMinutaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE core.CatTipoDocumento (
    TipoDocumentoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

/* ==========================================================
   2. ENTIDADES OPERATIVAS MAESTRAS (core)
   ========================================================== */

-- Entidad Central de Proyecto
CREATE TABLE core.Proyecto (
    ProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    Status NVARCHAR(250) NULL,
    NombreOficial NVARCHAR(250) NOT NULL,
    NombreCorto NVARCHAR(150) NULL,
    NombreNormalizado AS UPPER(LTRIM(RTRIM(NombreOficial))) PERSISTED,
    Descripcion NVARCHAR(MAX) NULL,
    TecnologiaId INT NULL,
    EstatusProyectoId INT NULL,
    NivelMadurezId INT NULL,
    ClasificacionId INT NULL,
    PrioridadId INT NULL,
    SemaforoId INT NULL,
    Renovable BIT NULL,
    CapacidadMW DECIMAL(12,3) NULL,
    
    -- Trazabilidad de registro
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(250) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(250) NULL,
    
    CONSTRAINT FK_Proyecto_Tecnologia FOREIGN KEY (TecnologiaId) REFERENCES core.CatTecnologia(TecnologiaId),
    CONSTRAINT FK_Proyecto_Estatus FOREIGN KEY (EstatusProyectoId) REFERENCES core.CatEstatusProyecto(EstatusProyectoId),
    CONSTRAINT FK_Proyecto_Nivel FOREIGN KEY (NivelMadurezId) REFERENCES core.CatNivelMadurez(NivelMadurezId),
    CONSTRAINT FK_Proyecto_Clasificacion FOREIGN KEY (ClasificacionId) REFERENCES core.CatClasificacion(ClasificacionId),
    CONSTRAINT FK_Proyecto_Prioridad FOREIGN KEY (PrioridadId) REFERENCES core.CatPrioridad(PrioridadId),
    CONSTRAINT FK_Proyecto_Semaforo FOREIGN KEY (SemaforoId) REFERENCES core.CatSemaforo(SemaforoId)
);
CREATE INDEX IX_Proyecto_NombreOficial ON core.Proyecto(NombreOficial);
CREATE INDEX IX_Proyecto_ClasifPriorSema ON core.Proyecto(ClasificacionId, PrioridadId, SemaforoId);
GO

-- Claves y Nombres Alternos de Múltiples Orígenes (Mapeo N-a-1)
CREATE TABLE core.ProyectoIdentificador (
    IdentificadorId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    OrigenDatosId INT NOT NULL,
    ClaveExterna NVARCHAR(150) NOT NULL, -- e.g. Folio VUPE, CNE o ID SICE
    NombreEnOrigen NVARCHAR(250) NULL,
    CONSTRAINT FK_Identificador_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Identificador_Origen FOREIGN KEY (OrigenDatosId) REFERENCES core.CatOrigenDatos(OrigenDatosId)
);
CREATE UNIQUE INDEX UK_ProyectoIdentificador ON core.ProyectoIdentificador(OrigenDatosId, ClaveExterna);
GO

-- Grupos de Interés Económico (GIE)
CREATE TABLE core.GrupoInteresEconomico (
    GrupoEconomicoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL UNIQUE,
    Descripcion NVARCHAR(MAX) NULL
);

-- Actores / Empresas
CREATE TABLE core.Actor (
    ActorId INT IDENTITY(1,1) PRIMARY KEY,
    RazonSocial NVARCHAR(250) NOT NULL UNIQUE,
    RFC NVARCHAR(13) NULL,
    PaisOrigen NVARCHAR(100) NULL,
    GrupoEconomicoId INT NULL,
    DomicilioFiscal NVARCHAR(500) NULL,
    EmailContacto NVARCHAR(250) NULL,
    TelefonoContacto NVARCHAR(50) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Actor_Grupo FOREIGN KEY (GrupoEconomicoId) REFERENCES core.GrupoInteresEconomico(GrupoEconomicoId)
);
CREATE INDEX IX_Actor_RazonSocial ON core.Actor(RazonSocial);
GO

-- Relación N a N: Proyecto <-> Actor
CREATE TABLE core.ProyectoActor (
    ProyectoActorId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    ActorId INT NOT NULL,
    TipoActorId INT NOT NULL,
    CONSTRAINT FK_ProyActor_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_ProyActor_Actor FOREIGN KEY (ActorId) REFERENCES core.Actor(ActorId),
    CONSTRAINT FK_ProyActor_Tipo FOREIGN KEY (TipoActorId) REFERENCES core.CatTipoActor(TipoActorId)
);
GO

-- Ubicaciones (Nubicaciones por Proyecto)
CREATE TABLE core.ProyectoUbicacion (
    UbicacionId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    EntidadFederativaId INT NOT NULL,
    MunicipioId INT NOT NULL,
    Localidad NVARCHAR(250) NULL,
    Predio NVARCHAR(500) NULL,
    EsPrincipal BIT NOT NULL DEFAULT 0,
    SubestacionAsociada NVARCHAR(250) NULL,
    PuntoInterconexion NVARCHAR(250) NULL,
    RegionTransmision NVARCHAR(150) NULL,
    CONSTRAINT FK_Ubicacion_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Ubicacion_Entidad FOREIGN KEY (EntidadFederativaId) REFERENCES core.CatEntidadFederativa(EntidadFederativaId),
    CONSTRAINT FK_Ubicacion_Municipio FOREIGN KEY (MunicipioId) REFERENCES core.CatMunicipio(MunicipioId)
);
GO

-- Coordenadas
CREATE TABLE core.ProyectoCoordenada (
    CoordenadaId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    UbicacionId INT NULL,
    Latitud DECIMAL(18, 10) NOT NULL,
    Longitud DECIMAL(18, 10) NOT NULL,
    Secuencia INT NOT NULL DEFAULT 1,
    Descripcion NVARCHAR(150) NULL,
    CONSTRAINT FK_Coordenada_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Coordenada_Ubicacion FOREIGN KEY (UbicacionId) REFERENCES core.ProyectoUbicacion(UbicacionId)
);
GO

-- Geometría espacial
CREATE TABLE core.ProyectoGeometria (
    GeometriaId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL UNIQUE,
    Poligono GEOGRAPHY NOT NULL,
    WktRepresentation NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Geometria_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId)
);
GO

-- Datos Técnicos
CREATE TABLE core.ProyectoDatosTecnicos (
    ProyectoId INT PRIMARY KEY,
    CapacidadInstaladaMW DECIMAL(12,3) NOT NULL,
    PotenciaAC_MW DECIMAL(12,3) NULL,
    PotenciaDC_MW DECIMAL(12,3) NULL,
    AlmacenamientoBess BIT NOT NULL DEFAULT 0,
    CapacidadBessMW DECIMAL(12,3) NULL,
    CapacidadBessMWh DECIMAL(12,3) NULL,
    HorasAlmacenamiento DECIMAL(6,2) NULL,
    ProduccionAnualEsperadaGWh DECIMAL(12,3) NULL,
    NivelTensionKV DECIMAL(6,2) NULL,
    EsHibrido BIT NOT NULL DEFAULT 0,
    ComentariosTecnicos NVARCHAR(MAX) NULL,
    CONSTRAINT FK_DatosTecnicos_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId)
);
GO

-- Datos Financieros
CREATE TABLE core.ProyectoDatosFinancieros (
    ProyectoId INT PRIMARY KEY,
    CAPEX DECIMAL(18, 2) NULL,
    MonedaId INT NOT NULL,
    FuenteFinanciamiento NVARCHAR(250) NULL,
    NombreEPC NVARCHAR(250) NULL,
    ProveedoresPrincipales NVARCHAR(MAX) NULL,
    TipoFinanciamiento NVARCHAR(150) NULL,
    MontoPresupuestalMDP DECIMAL(18, 4) NULL,
    EquiposAsociadosNarrativo NVARCHAR(MAX) NULL,
    CONSTRAINT FK_DatosFinancieros_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_DatosFinancieros_Moneda FOREIGN KEY (MonedaId) REFERENCES core.CatMoneda(MonedaId)
);
GO

-- Trámites por Autoridad
CREATE TABLE core.ProyectoTramite (
    TramiteId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    AutoridadId INT NOT NULL,
    TipoTramiteId INT NOT NULL,
    EstatusTramiteId INT NOT NULL,
    Folio NVARCHAR(150) NULL,
    FechaIngreso DATE NULL,
    FechaResolucion DATE NULL,
    FechaVencimiento DATE NULL,
    Resultado NVARCHAR(250) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    DocumentoUrl NVARCHAR(1000) NULL,
    
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(250) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(250) NULL,
    
    CONSTRAINT FK_Tramite_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Tramite_Autoridad FOREIGN KEY (AutoridadId) REFERENCES core.CatAutoridad(AutoridadId),
    CONSTRAINT FK_Tramite_Tipo FOREIGN KEY (TipoTramiteId) REFERENCES core.CatTipoTramite(TipoTramiteId),
    CONSTRAINT FK_Tramite_Estatus FOREIGN KEY (EstatusTramiteId) REFERENCES core.CatEstatusTramite(EstatusTramiteId)
);
CREATE INDEX IX_Tramite_Proyecto ON core.ProyectoTramite(ProyectoId);
GO

-- Hitos y Cronogramas
CREATE TABLE core.ProyectoHito (
    HitoId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    NombreHito NVARCHAR(200) NOT NULL,
    FechaProgramada DATE NOT NULL,
    FechaReprogramada DATE NULL,
    FechaReal DATE NULL,
    AvancePorcentaje DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    EsHitoCritico BIT NOT NULL DEFAULT 0,
    Observaciones NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Hito_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId)
);
GO

-- Ligas de Documentos SharePoint
CREATE TABLE core.Documento (
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
    CONSTRAINT FK_Documento_Tipo FOREIGN KEY (TipoDocumentoId) REFERENCES core.CatTipoDocumento(TipoDocumentoId)
);

-- Reunión / Minutas
CREATE TABLE core.Reunion (
    ReunionId INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(300) NOT NULL,
    FechaReunion DATE NOT NULL,
    Modalidad NVARCHAR(100) NULL,
    Lugar NVARCHAR(300) NULL,
    Objetivo NVARCHAR(MAX) NULL,
    Asistentes NVARCHAR(MAX) NULL,
    DocumentoId INT NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Reunion_Documento FOREIGN KEY (DocumentoId) REFERENCES core.Documento(DocumentoId)
);

-- Bitácora de Reunión / Acuerdos
CREATE TABLE core.BitacoraProyecto (
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
    SnapshotProyectoJSON NVARCHAR(MAX) NULL,
    ProcesadoPor NVARCHAR(450) NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Bitacora_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Bitacora_Reunion FOREIGN KEY (ReunionId) REFERENCES core.Reunion(ReunionId),
    CONSTRAINT FK_Bitacora_Documento FOREIGN KEY (DocumentoId) REFERENCES core.Documento(DocumentoId),
    CONSTRAINT FK_Bitacora_Valoracion FOREIGN KEY (ValoracionMinutaId) REFERENCES core.CatValoracionMinuta(ValoracionMinutaId)
);
CREATE INDEX IX_Bitacora_Proyecto_Fecha ON core.BitacoraProyecto(ProyectoId, FechaEvento DESC);

-- Acciones de Seguimiento (Compromisos)
CREATE TABLE core.AccionSeguimiento (
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
    CONSTRAINT FK_Accion_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Accion_Bitacora FOREIGN KEY (BitacoraProyectoId) REFERENCES core.BitacoraProyecto(BitacoraProyectoId),
    CONSTRAINT FK_Accion_Semaforo FOREIGN KEY (SemaforoId) REFERENCES core.CatSemaforo(SemaforoId)
);

-- Historial del Cambios del Proyecto
CREATE TABLE core.HistorialProyecto (
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
    CONSTRAINT FK_Historial_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_Historial_Bitacora FOREIGN KEY (BitacoraProyectoId) REFERENCES core.BitacoraProyecto(BitacoraProyectoId),
    CONSTRAINT FK_Historial_Documento FOREIGN KEY (DocumentoId) REFERENCES core.Documento(DocumentoId)
);
CREATE INDEX IX_Historial_Proyecto ON core.HistorialProyecto(ProyectoId, CambiadoEn DESC);
GO

/* ==========================================================
   3. TABLAS DE TRAZABILIDAD E ETL INGESTIÓN (core / staging)
   ========================================================== */

-- Bitácora de Cargas Físicas (Deduplicación)
CREATE TABLE core.ProyectoBitacoraCarga (
    BitacoraCargaId INT IDENTITY(1,1) PRIMARY KEY,
    ProyectoId INT NOT NULL,
    OrigenDatosId INT NOT NULL,
    ArchivoOrigen NVARCHAR(250) NOT NULL,
    HojaOrigen NVARCHAR(100) NOT NULL,
    FilaOrigen INT NOT NULL,
    FechaCarga DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UsuarioCarga NVARCHAR(250) NOT NULL,
    HashDeduplicacion NVARCHAR(64) NOT NULL,
    DatosRawJson NVARCHAR(MAX) NOT NULL,
    EstatusValidacion NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_BitacoraCarga_Proyecto FOREIGN KEY (ProyectoId) REFERENCES core.Proyecto(ProyectoId),
    CONSTRAINT FK_BitacoraCarga_Origen FOREIGN KEY (OrigenDatosId) REFERENCES core.CatOrigenDatos(OrigenDatosId)
);
CREATE INDEX IX_BitacoraCarga_Proyecto ON core.ProyectoBitacoraCarga(ProyectoId);
GO

-- Tablas de Staging (Volcado 1:1 de Excels)
CREATE TABLE staging.ExcelRaw_VUPEWorksheet (
    StagingId INT IDENTITY(1,1) PRIMARY KEY,
    IDRaw NVARCHAR(MAX) NULL,
    PreFolioRaw NVARCHAR(MAX) NULL,
    FolioProyectoRaw NVARCHAR(MAX) NULL,
    RFCRaw NVARCHAR(MAX) NULL,
    NombreRaw NVARCHAR(MAX) NULL,
    ProyectoRaw NVARCHAR(MAX) NULL,
    DescripcionProyectoRaw NVARCHAR(MAX) NULL,
    GrupoInteresRaw NVARCHAR(MAX) NULL,
    EsHibridaRaw NVARCHAR(MAX) NULL,
    TipoTecnologiaRaw NVARCHAR(MAX) NULL,
    FechaImportacion DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE staging.ExcelRaw_BDCompleta (
    StagingId INT IDENTITY(1,1) PRIMARY KEY,
    EstatusRaw NVARCHAR(MAX) NULL,
    IDRaw NVARCHAR(MAX) NULL,
    PreFolioRaw NVARCHAR(MAX) NULL,
    FolioProyectoRaw NVARCHAR(MAX) NULL,
    NoCFERaw NVARCHAR(MAX) NULL,
    RFCRaw NVARCHAR(MAX) NULL,
    NombreRaw NVARCHAR(MAX) NULL,
    SubgrupoRaw NVARCHAR(MAX) NULL,
    SustitutosRaw NVARCHAR(MAX) NULL,
    FechaImportacion DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE staging.ExcelRaw_PVIRCE (
    StagingId INT IDENTITY(1,1) PRIMARY KEY,
    StatusRaw NVARCHAR(MAX) NULL,
    StatusVfRaw NVARCHAR(MAX) NULL,
    NombreRealRaw NVARCHAR(MAX) NULL,
    NoConsiderarRaw NVARCHAR(MAX) NULL,
    AnioRaw NVARCHAR(MAX) NULL,
    AdicionesSustitucionesRaw NVARCHAR(MAX) NULL,
    ContratoUnidadRaw NVARCHAR(MAX) NULL,
    TipoRaw NVARCHAR(MAX) NULL,
    TipoVfRaw NVARCHAR(MAX) NULL,
    RenovableRaw NVARCHAR(MAX) NULL,
    MWRaw NVARCHAR(MAX) NULL,
    MesRaw NVARCHAR(MAX) NULL,
    GerenciaControlRaw NVARCHAR(MAX) NULL,
    RegionTransmisionRaw NVARCHAR(MAX) NULL,
    EntidadFederativaRaw NVARCHAR(MAX) NULL,
    MunicipioRaw NVARCHAR(MAX) NULL,
    FirmesRaw NVARCHAR(MAX) NULL,
    FechaImportacion DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE staging.ExcelRaw_CNERPyT (
    StagingId INT IDENTITY(1,1) PRIMARY KEY,
    EstatusRaw NVARCHAR(MAX) NULL,
    IDRaw NVARCHAR(MAX) NULL,
    PreFolioRaw NVARCHAR(MAX) NULL,
    FolioProyectoRaw NVARCHAR(MAX) NULL,
    RFCRaw NVARCHAR(MAX) NULL,
    NombreRaw NVARCHAR(MAX) NULL,
    NombreProyectoRaw NVARCHAR(MAX) NULL,
    TipoTecnologiaRaw NVARCHAR(MAX) NULL,
    GIERaw NVARCHAR(MAX) NULL,
    FechaImportacion DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE staging.ExcelRaw_InformePormenorizado (
    StagingId INT IDENTITY(1,1) PRIMARY KEY,
    NoRaw NVARCHAR(MAX) NULL,
    GRTRaw NVARCHAR(MAX) NULL,
    NombreProyectoRaw NVARCHAR(MAX) NULL,
    TipoFinanciamientoRaw NVARCHAR(MAX) NULL,
    AnioInstruccionRaw NVARCHAR(MAX) NULL,
    EtapaProyectoRaw NVARCHAR(MAX) NULL,
    MontoProyectoMDPRaw NVARCHAR(MAX) NULL,
    EquiposAsociadosRaw NVARCHAR(MAX) NULL,
    FechaEstimadaInicioRaw NVARCHAR(MAX) NULL,
    FechaImportacion DATETIME2 DEFAULT SYSUTCDATETIME()
);
GO

/* ==========================================================
   4. SEMILLADO DE CATÁLOGOS BÁSICOS
   ========================================================== */

-- Tecnologías
INSERT INTO core.CatTecnologia (Nombre, EsRenovable) VALUES
(N'FV', 1), (N'FOTOVOLTAICA', 1), (N'ENERGÍA SOLAR', 1), (N'Solar', 1),
(N'EO', 1), (N'Eólica', 1),
(N'Hidro', 1), (N'Hidroeléctrica', 1),
(N'COG', 1), (N'CI/COG', 1), (N'Cogeneración', 1),
(N'Baterías / BESS', 1), (N'Almacenamiento / BESS', 1),
(N'Híbrida', 1),
(N'Bioenergía', 1),
(N'Termoeléctrica Convencional', 0),
(N'Ciclo Combinado', 0);

-- Estatus generales
INSERT INTO core.CatEstatusProyecto (Nombre, Descripcion) VALUES
(N'Idea Inicial', N'Planificación preliminar o idea conceptual.'),
(N'Estudios de Factibilidad', N'Fase de estudios técnicos, ambientales y sociales.'),
(N'En Desarrollo', N'Fase activa de obtención de permisos y financiamiento.'),
(N'Por concursar', N'Listo para asignación presupuestal u obra pública.'),
(N'En Construcción', N'Obras electromecánicas o civiles activas.'),
(N'Pruebas', N'Período de pruebas de interconexión con el SEN.'),
(N'Operación Comercial', N'Generando y entregando energía activa.'),
(N'En Operación', N'Operación activa (obra pública o CFE).'),
(N'Suspendido', N'Detenido temporalmente por causas regulatorias, financieras o sociales.'),
(N'Cancelado', N'Cancelación definitiva.');

-- Nivel de madurez
INSERT INTO core.CatNivelMadurez (Nombre, Orden) VALUES
(N'Idea Inicial', 1),
(N'Prefactibilidad / Estudios', 2),
(N'Listo para Construcción (RTB)', 3),
(N'Construcción Avanzada', 4),
(N'Operación Comercial (FEO)', 5);

-- Clasificaciones de Control
INSERT INTO core.CatClasificacion (Nombre, Descripcion) VALUES
(N'Ernesto / Mesas especiales DG', N'Seguimiento reservado a Ernesto/DG o instrucción especial.'),
(N'Mesas especiales DG', N'Seguimiento reservado a DG; equivalente operativo usado en Excel.'),
(N'Ruta crítica', N'Proyecto con bloqueo habilitante, riesgo regulatorio, ambiental, social o de interconexión.'),
(N'Autoconsumo', N'Proyecto de abasto aislado, cogeneración, autoconsumo o regularización relacionada.'),
(N'Migraciones', N'Ruta de migración LIE/LSE/MEM o ajuste de modalidad.'),
(N'Ventanilla', N'Entrada inicial para confirmar ruta, expediente o factibilidad.'),
(N'Inviables', N'Sin interés, permiso terminado, no factible o sin ruta activa.');

-- Prioridades de Control
INSERT INTO core.CatPrioridad (Nombre, Orden) VALUES
(N'Alta', 1), (N'Media', 2), (N'Baja', 3), (N'Sin prioridad', 4);

-- Semáforos de Control
INSERT INTO core.CatSemaforo (Nombre, ColorHex, Descripcion) VALUES
(N'Rojo', '#C00000', N'Riesgo crítico, vencido o bloqueo relevante.'),
(N'Amarillo', '#FFC000', N'Pendiente relevante o vencimiento próximo.'),
(N'Verde', '#00B050', N'En tiempo o sin bloqueo actual.'),
(N'Gris', '#808080', N'Sin información suficiente.'),
(N'Punto', '#808080', N'Indicador importado desde Excel cuando solo existe símbolo ●.');

-- Autoridades
INSERT INTO core.CatAutoridad (Acronimo, NombreCompleto) VALUES
(N'SEMARNAT', N'Secretaría de Medio Ambiente y Recursos Naturales'),
(N'CENACE', N'Centro Nacional de Control de Energía'),
(N'CRE', N'Comisión Reguladora de Energía'),
(N'SENER', N'Secretaría de Energía'),
(N'ASEA', N'Agencia de Seguridad, Energía y Ambiente'),
(N'CONAGUA', N'Comisión Nacional del Agua'),
(N'CFE', N'Comisión Federal de Electricidad');

-- Tipos de trámites
INSERT INTO core.CatTipoTramite (Nombre, Descripcion) VALUES
(N'MIA / Impacto Ambiental', N'Manifestación de Impacto Ambiental ante SEMARNAT.'),
(N'EVIS / Impacto Social', N'Evaluación de Impacto Social ante la SENER.'),
(N'Estudio de Interconexión', N'Estudios de conexión a la red de transmisión ante CENACE.'),
(N'Permiso de Generación', N'Autorización para producir energía eléctrica otorgado por la CRE.');

-- Estatus de trámites
INSERT INTO core.CatEstatusTramite (Nombre) VALUES
(N'Autorizada'), (N'En evaluación'), (N'No ingresada'), (N'No presentado'),
(N'Por confirmar'), (N'Vigencia por confirmar'), (N'Por iniciar / vigencia por confirmar'),
(N'Pendiente / según ruta de modificación'), (N'El promovente No ha presentado a SEMARNAT el trámite');

-- Tipos de actores
INSERT INTO core.CatTipoActor (Nombre) VALUES
(N'Promovente / Desarrollador'), (N'Representante Legal'), (N'Socio Financiero'), (N'Contratista EPC');

-- Monedas
INSERT INTO core.CatMoneda (Codigo, Nombre) VALUES
(N'MXN', N'Peso Mexicano'), (N'USD', N'Dólar Americano');

-- Origen de datos
INSERT INTO core.CatOrigenDatos (Nombre, TipoFuente) VALUES
(N'VUPE_Ventanilla', N'Excel'),
(N'CNE_CFE', N'Excel'),
(N'SICE_SENER', N'Excel'),
(N'PVIRCE_Oficial', N'Excel'),
(N'InformePormenorizado', N'Excel');

-- Valoración de minutas
INSERT INTO core.CatValoracionMinuta (Nombre) VALUES
(N'EN CONSTRUCCIÓN'), (N'DETENIDO'), (N'PRUEBAS'), (N'OPERACIÓN'), (N'SIN AVANCE'), (N'EN DESARROLLO'), (N'REUNIÓN ESPECIAL');

-- Tipos de documentos
INSERT INTO core.CatTipoDocumento (Nombre) VALUES
(N'Minuta'), (N'Ficha técnica'), (N'PPT'), (N'Oficio'), (N'Anexo'), (N'Otro');
GO
