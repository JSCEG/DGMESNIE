-- ════════════════════════════════════════════════════════════════════════════
-- dgmesnie.PODECOBI — esquema canónico para el dashboard y el informe LaTeX
-- Convención: esquema dgmesnie, tablas prefijadas PODECOBI_, columnas en
-- PascalCase, FK con índice, columnas de auditoría (FechaRegistro, FechaActualizacion,
-- UsuarioRegistro, UsuarioActualizacion).
--
-- Reglas de carga:
--   - Una fila por polo en PODECOBI_Polo (llave estable Numero, '01'..'14').
--   - Superficie jurídica (OficialAreaHa) prevalece sobre GeoJSON. La geometría
--     se conserva aparte para visualización; nunca se sobreescribe el polígono
--     jurídico.
--   - Contactos federales/estatales se separan por ámbito (Ambito: 'FEDERAL'|'ESTATAL').
--   - Vocaciones productivas se guardan en tabla aparte (1 fila por vocación).
--   - Fuentes: cada hecho lleva su origen y fecha de corte. No se mezclan.
-- ════════════════════════════════════════════════════════════════════════════

IF SCHEMA_ID('dgmesnie') IS NULL EXEC('CREATE SCHEMA dgmesnie;');

-- ── Catálogo: catálogos cerrados para etapa y verificación ─────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_CatalogoEtapa') IS NULL
CREATE TABLE dgmesnie.PODECOBI_CatalogoEtapa (
    EtapaId        INT IDENTITY(1,1) PRIMARY KEY,
    Clave          VARCHAR(40)  NOT NULL UNIQUE,    -- 'PREINVERSION','EJECUCION','OPERACION'
    Nombre         VARCHAR(120) NOT NULL,
    Orden          INT          NOT NULL DEFAULT 0
);

IF OBJECT_ID('dgmesnie.PODECOBI_CatalogoVerificacion') IS NULL
CREATE TABLE dgmesnie.PODECOBI_CatalogoVerificacion (
    VerificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Clave          VARCHAR(40)  NOT NULL UNIQUE,    -- 'VERIFICADO','PENDIENTE','NO_PUBLICADO','NO_APLICA','CONTRADICTORIO'
    Nombre         VARCHAR(120) NOT NULL
);

-- ── Polo: tabla maestra, una fila por PODECOBI declarado ────────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_Polo') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Polo (
    PoloId                INT IDENTITY(1,1) PRIMARY KEY,
    Numero                VARCHAR(2)   NOT NULL UNIQUE,   -- '01'..'14'
    NombreOficial         VARCHAR(400) NOT NULL,
    NombreManual          VARCHAR(400) NULL,
    Estado                VARCHAR(120) NOT NULL,
    Municipio             VARCHAR(200) NULL,
    Activo                BIT          NOT NULL DEFAULT 1,

    -- Jurídico
    FechaDeclaracion      DATE         NULL,
    UrlDeclaracion        VARCHAR(600) NULL,
    Modificacion          VARCHAR(400) NULL,
    UrlModificacion       VARCHAR(600) NULL,
    Convenio              VARCHAR(400) NULL,
    UrlConvenio           VARCHAR(600) NULL,
    ComiteSesion          VARCHAR(400) NULL,

    -- Superficie jurídica (DOF/SIDOF) — fuente prevaleciente
    AreaOficialHa         DECIMAL(18,4) NULL,
    AreaManualRaw         VARCHAR(80)   NULL,
    AreaGeojsonHa         DECIMAL(18,4) NULL,
    GeojsonDeltaHa        DECIMAL(18,4) NULL,
    GeojsonDeltaPct       DECIMAL(9,4)  NULL,

    -- Geoespacial
    GeojsonFeatureCount   INT           NULL,
    GeojsonValid          BIT           NULL,
    GeojsonValidity       VARCHAR(400)  NULL,
    CentroidLon           DECIMAL(18,8) NULL,
    CentroidLat           DECIMAL(18,8) NULL,

    -- Operativo
    Etapa                 VARCHAR(40)   NULL,           -- FK lógica a PODECOBI_CatalogoEtapa.Clave
    Subetapa              VARCHAR(120)  NULL,
    FechaRevisionPublica  DATE          NULL,
    UrlProyectosMexico    VARCHAR(600)  NULL,
    AvanceManualPct       DECIMAL(5,2)  NULL,

    -- Inversión y empleo (sólo si publicados)
    Inversion             VARCHAR(400)  NULL,
    Empleos               VARCHAR(400)  NULL,

    -- Electricidad
    DemandaElectrica      VARCHAR(80)   NULL,
    DemandaElectricaNota  VARCHAR(400)  NULL,
    DemandaMaxima         VARCHAR(80)   NULL,
    DemandaMaximaNota     VARCHAR(400)  NULL,
    Tension               VARCHAR(80)   NULL,
    Conexion              VARCHAR(800)  NULL,

    -- Gas
    GasDisponibilidad     VARCHAR(200)  NULL,
    GasNota               VARCHAR(400)  NULL,
    Ducto                 VARCHAR(400)  NULL,

    -- Trazabilidad
    CorteFuente           VARCHAR(40)   NULL,           -- fecha de corte ISO 'YYYY-MM-DD'
    Verificacion          VARCHAR(40)   NULL,           -- FK lógica a PODECOBI_CatalogoVerificacion.Clave
    ComentarioVerificacion VARCHAR(2000) NULL,

    -- Auditoría
    FechaRegistro         DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaActualizacion    DATETIME2(0)  NULL,
    UsuarioRegistro       VARCHAR(120)  NULL,
    UsuarioActualizacion  VARCHAR(120)  NULL,

    CONSTRAINT CK_PODECOBI_Polo_Numero_Rango CHECK (Numero BETWEEN '01' AND '99')
);
CREATE INDEX IX_PODECOBI_Polo_Estado  ON dgmesnie.PODECOBI_Polo(Estado);
CREATE INDEX IX_PODECOBI_Polo_Etapa   ON dgmesnie.PODECOBI_Polo(Etapa);

-- ── Vocaciones productivas: 1 fila por vocación ────────────────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_Vocacion') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Vocacion (
    VocacionId   INT IDENTITY(1,1) PRIMARY KEY,
    PoloId       INT NOT NULL,
    Vocacion     VARCHAR(200) NOT NULL,
    Orden        INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_PODECOBI_Vocacion_Polo FOREIGN KEY (PoloId)
        REFERENCES dgmesnie.PODECOBI_Polo(PoloId) ON DELETE CASCADE
);
CREATE INDEX IX_PODECOBI_Vocacion_Polo ON dgmesnie.PODECOBI_Vocacion(PoloId);

-- ── Contactos por ámbito (FEDERAL/ESTATAL/MUNICIPAL) ───────────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_Contacto') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Contacto (
    ContactoId          INT IDENTITY(1,1) PRIMARY KEY,
    PoloId              INT NOT NULL,
    Ambito              VARCHAR(20)  NOT NULL,           -- 'FEDERAL' | 'ESTATAL' | 'MUNICIPAL'
    Nombre              VARCHAR(250) NULL,
    Cargo               VARCHAR(200) NULL,
    Correo              VARCHAR(250) NULL,
    Telefono            VARCHAR(120) NULL,
    Notas               VARCHAR(500) NULL,
    FechaRegistro       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaActualizacion  DATETIME2(0) NULL,
    CONSTRAINT FK_PODECOBI_Contacto_Polo FOREIGN KEY (PoloId)
        REFERENCES dgmesnie.PODECOBI_Polo(PoloId) ON DELETE CASCADE,
    CONSTRAINT CK_PODECOBI_Contacto_Ambito CHECK (Ambito IN ('FEDERAL','ESTATAL','MUNICIPAL'))
);
CREATE INDEX IX_PODECOBI_Contacto_Polo  ON dgmesnie.PODECOBI_Contacto(PoloId);
CREATE INDEX IX_PODECOBI_Contacto_Ambito ON dgmesnie.PODECOBI_Contacto(PoloId, Ambito);

-- ── Geometría del polígono (almacenada como GeoJSON NVARCHAR(MAX)) ─────────
IF OBJECT_ID('dgmesnie.PODECOBI_Geometria') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Geometria (
    GeometriaId         INT IDENTITY(1,1) PRIMARY KEY,
    PoloId              INT NOT NULL,
    FeatureIndex        INT NOT NULL,                    -- índice dentro del FeatureCollection
    GeometryType        VARCHAR(40) NULL,
    GeometryJson        NVARCHAR(MAX) NOT NULL,           -- GeoJSON geometry crudo
    PropertiesJson      NVARCHAR(MAX) NULL,               -- properties crudo
    IsValid             BIT NULL,
    ValidityNote        VARCHAR(400) NULL,
    AreaHa              DECIMAL(18,4) NULL,
    PerimetroM          DECIMAL(18,4) NULL,
    SourceUrl           VARCHAR(600) NULL,
    Sha256              VARCHAR(64) NULL,
    FechaDescarga       DATETIME2(0) NULL,
    FechaRegistro       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PODECOBI_Geometria_Polo FOREIGN KEY (PoloId)
        REFERENCES dgmesnie.PODECOBI_Polo(PoloId) ON DELETE CASCADE,
    CONSTRAINT UQ_PODECOBI_Geometria UNIQUE (PoloId, FeatureIndex)
);
CREATE INDEX IX_PODECOBI_Geometria_Polo ON dgmesnie.PODECOBI_Geometria(PoloId);

-- ── Fuente verificada por polo (trazabilidad del informe) ──────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_Fuente') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Fuente (
    FuenteId     INT IDENTITY(1,1) PRIMARY KEY,
    PoloId       INT NOT NULL,
    Tipo         VARCHAR(40)  NOT NULL,    -- 'DOF' | 'SIDOF' | 'PROYECTOS_MX' | 'PRESIDENCIA' | 'GEOJSON' | 'MANUAL' | 'OTRO'
    Url          VARCHAR(800) NULL,
    Fecha        DATE         NULL,
    Descripcion  VARCHAR(800) NULL,
    Sha256       VARCHAR(64)  NULL,
    FechaRegistro DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PODECOBI_Fuente_Polo FOREIGN KEY (PoloId)
        REFERENCES dgmesnie.PODECOBI_Polo(PoloId) ON DELETE CASCADE,
    CONSTRAINT CK_PODECOBI_Fuente_Tipo CHECK (Tipo IN ('DOF','SIDOF','PROYECTOS_MX','PRESIDENCIA','GEOJSON','MANUAL','OTRO'))
);
CREATE INDEX IX_PODECOBI_Fuente_Polo ON dgmesnie.PODECOBI_Fuente(PoloId);

-- ── Catálogo: corte vigente (último corte cargado en BD) ───────────────────
IF OBJECT_ID('dgmesnie.PODECOBI_Corte') IS NULL
CREATE TABLE dgmesnie.PODECOBI_Corte (
    CorteId        INT IDENTITY(1,1) PRIMARY KEY,
    FechaCorte     DATE          NOT NULL UNIQUE,
    VersionInforme VARCHAR(20)   NULL,        -- p.ej. '1.2'
    PoloCount      INT           NOT NULL,
    HashInventario VARCHAR(64)   NULL,        -- SHA256 del inventario_maestro.json cargado
    CargadoPor     VARCHAR(120)  NULL,
    FechaRegistro  DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
    Notas          VARCHAR(1000) NULL
);

-- ════════════════════════════════════════════════════════════════════════════
-- Catálogos semilla
-- ════════════════════════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM dgmesnie.PODECOBI_CatalogoEtapa)
INSERT INTO dgmesnie.PODECOBI_CatalogoEtapa (Clave, Nombre, Orden) VALUES
 ('PREINVERSION', 'Preinversión', 1),
 ('EJECUCION',    'Ejecución',     2),
 ('OPERACION',    'Operación',     3);

IF NOT EXISTS (SELECT 1 FROM dgmesnie.PODECOBI_CatalogoVerificacion)
INSERT INTO dgmesnie.PODECOBI_CatalogoVerificacion (Clave, Nombre) VALUES
 ('VERIFICADO',     'Verificado'),
 ('PENDIENTE',      'Pendiente'),
 ('NO_PUBLICADO',   'No publicado'),
 ('NO_APLICA',      'No aplica'),
 ('CONTRADICTORIO', 'Contradictorio');

PRINT 'ddl_podecobi.sql — esquema dgmesnie.PODECOBI_* listo.';
GO
