const fs = require('fs');
const sql = require('mssql');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: {
        encrypt: true,
        trustServerCertificate: false
    }
};

const ddl = `
-- Drop existing tables if they exist in correct order
IF OBJECT_ID('dgmesnie.HistorialProyecto', 'U') IS NOT NULL DROP TABLE dgmesnie.HistorialProyecto;
IF OBJECT_ID('dgmesnie.AccionSeguimiento', 'U') IS NOT NULL DROP TABLE dgmesnie.AccionSeguimiento;
IF OBJECT_ID('dgmesnie.BitacoraProyecto', 'U') IS NOT NULL DROP TABLE dgmesnie.BitacoraProyecto;
IF OBJECT_ID('dgmesnie.Reunion', 'U') IS NOT NULL DROP TABLE dgmesnie.Reunion;
IF OBJECT_ID('dgmesnie.Documento', 'U') IS NOT NULL DROP TABLE dgmesnie.Documento;
IF OBJECT_ID('dgmesnie.TramiteProyecto', 'U') IS NOT NULL DROP TABLE dgmesnie.TramiteProyecto;
IF OBJECT_ID('dgmesnie.Proyecto', 'U') IS NOT NULL DROP TABLE dgmesnie.Proyecto;
IF OBJECT_ID('dgmesnie.Empresa', 'U') IS NOT NULL DROP TABLE dgmesnie.Empresa;
IF OBJECT_ID('dgmesnie.CatValoracionMinuta', 'U') IS NOT NULL DROP TABLE dgmesnie.CatValoracionMinuta;
IF OBJECT_ID('dgmesnie.CatTipoDocumento', 'U') IS NOT NULL DROP TABLE dgmesnie.CatTipoDocumento;
IF OBJECT_ID('dgmesnie.CatEstatusTramite', 'U') IS NOT NULL DROP TABLE dgmesnie.CatEstatusTramite;
IF OBJECT_ID('dgmesnie.CatTecnologia', 'U') IS NOT NULL DROP TABLE dgmesnie.CatTecnologia;
IF OBJECT_ID('dgmesnie.CatSemaforo', 'U') IS NOT NULL DROP TABLE dgmesnie.CatSemaforo;
IF OBJECT_ID('dgmesnie.CatPrioridad', 'U') IS NOT NULL DROP TABLE dgmesnie.CatPrioridad;
IF OBJECT_ID('dgmesnie.CatClasificacion', 'U') IS NOT NULL DROP TABLE dgmesnie.CatClasificacion;

-- Create catalogs
CREATE TABLE dgmesnie.CatClasificacion (
    ClasificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE dgmesnie.CatPrioridad (
    PrioridadId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Orden INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE dgmesnie.CatSemaforo (
    SemaforoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    ColorHex NVARCHAR(10) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE dgmesnie.CatTecnologia (
    TecnologiaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE dgmesnie.CatEstatusTramite (
    EstatusTramiteId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(250) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE dgmesnie.CatTipoDocumento (
    TipoDocumentoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE dgmesnie.CatValoracionMinuta (
    ValoracionMinutaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

-- Create principal tables
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

CREATE TABLE dgmesnie.Proyecto (
    ProyectoId INT IDENTITY(1,1) PRIMARY KEY,
    Status NVARCHAR(250) NULL,
    Nombre NVARCHAR(250) NOT NULL,
    NombreNormalizado AS UPPER(LTRIM(RTRIM(Nombre))) PERSISTED,
    EmpresaId INT NULL,
    Promovente NVARCHAR(250) NULL,
    GrupoEconomico NVARCHAR(250) NULL,
    Origen NVARCHAR(250) NULL,
    Anio INT NULL,
    AdicionesSustituciones NVARCHAR(500) NULL,
    ContratoUnidad NVARCHAR(500) NULL,
    Tipo NVARCHAR(100) NULL,
    Renovable BIT NULL,
    CapacidadMW DECIMAL(12,3) NULL,
    Mes NVARCHAR(50) NULL,
    GerenciaControl NVARCHAR(250) NULL,
    RegionTransmision NVARCHAR(250) NULL,
    EntidadFederativa NVARCHAR(150) NULL,
    Municipio NVARCHAR(250) NULL,
    Longitud DECIMAL(18,10) NULL,
    Latitud DECIMAL(18,10) NULL,
    Firmes NVARCHAR(250) NULL,
    PorcentajeConstruccion DECIMAL(5,2) NULL,
    FolioEvIS_MISSE NVARCHAR(250) NULL,
    StatusEvIS_MISSE NVARCHAR(250) NULL,
    StatusCPLI NVARCHAR(250) NULL,
    ConflictosSociales NVARCHAR(MAX) NULL,
    NumeroPermiso NVARCHAR(150) NULL,
    FechaInicioObras DATE NULL,
    FechaTerminacionObras DATE NULL,
    FechaEntradaOperacion DATE NULL,
    EstadoProgramaObras NVARCHAR(MAX) NULL,
    TramiteCNE NVARCHAR(MAX) NULL,
    ObservacionesCNE NVARCHAR(MAX) NULL,
    Categoria NVARCHAR(150) NULL,
    InteresadaEnContinuar BIT NULL,
    ObservacionesUEVISPI NVARCHAR(MAX) NULL,
    ResumenCaso NVARCHAR(MAX) NULL,
    PropuestaAtencion NVARCHAR(MAX) NULL,
    RequiereAlmacenamiento NVARCHAR(250) NULL,
    SiguientesPasos NVARCHAR(MAX) NULL,
    ClasificacionId INT NULL,
    PrioridadId INT NULL,
    SemaforoId INT NULL,
    RazonesBreves NVARCHAR(MAX) NULL,
    TramitesSemarnat NVARCHAR(MAX) NULL,
    EstatusSemarnat NVARCHAR(500) NULL,
    ObservacionesSemarnat NVARCHAR(MAX) NULL,
    Fuente NVARCHAR(250) NULL,
    SemaforoPPT NVARCHAR(50) NULL,
    FechaUltimaActualizacion DATE NULL,
    FuenteUltimaActualizacion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    ActualizadoEn DATETIME2 NULL,
    ActualizadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Proyecto_Empresa FOREIGN KEY (EmpresaId) REFERENCES dgmesnie.Empresa(EmpresaId),
    CONSTRAINT FK_Proyecto_Clasificacion FOREIGN KEY (ClasificacionId) REFERENCES dgmesnie.CatClasificacion(ClasificacionId),
    CONSTRAINT FK_Proyecto_Prioridad FOREIGN KEY (PrioridadId) REFERENCES dgmesnie.CatPrioridad(PrioridadId),
    CONSTRAINT FK_Proyecto_Semaforo FOREIGN KEY (SemaforoId) REFERENCES dgmesnie.CatSemaforo(SemaforoId)
);

CREATE INDEX IX_Proyecto_Nombre ON dgmesnie.Proyecto(Nombre);
CREATE INDEX IX_Proyecto_Empresa ON dgmesnie.Proyecto(EmpresaId);

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

CREATE INDEX IX_TramiteProyecto_Proyecto ON dgmesnie.TramiteProyecto(ProyectoId);

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

CREATE TABLE dgmesnie.Reunion (
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
    CONSTRAINT FK_Reunion_Documento FOREIGN KEY (DocumentoId) REFERENCES dgmesnie.Documento(DocumentoId)
);

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
    SnapshotProyectoJSON NVARCHAR(MAX) NULL,
    ProcesadoPor NVARCHAR(450) NULL,
    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreadoPor NVARCHAR(450) NULL,
    CONSTRAINT FK_Bitacora_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.Proyecto(ProyectoId),
    CONSTRAINT FK_Bitacora_Reunion FOREIGN KEY (ReunionId) REFERENCES dgmesnie.Reunion(ReunionId),
    CONSTRAINT FK_Bitacora_Documento FOREIGN KEY (DocumentoId) REFERENCES dgmesnie.Documento(DocumentoId),
    CONSTRAINT FK_Bitacora_Valoracion FOREIGN KEY (ValoracionMinutaId) REFERENCES dgmesnie.CatValoracionMinuta(ValoracionMinutaId)
);

CREATE INDEX IX_Bitacora_Proyecto_Fecha ON dgmesnie.BitacoraProyecto(ProyectoId, FechaEvento DESC);

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

CREATE INDEX IX_Historial_Proyecto ON dgmesnie.HistorialProyecto(ProyectoId, CambiadoEn DESC);
`;

const seedCatalogsSql = `
-- Seeds
INSERT INTO dgmesnie.CatClasificacion (Nombre, Descripcion) VALUES
(N'Ernesto / Mesas especiales DG', N'Seguimiento reservado a Ernesto/DG o instrucción especial.'),
(N'Mesas especiales DG', N'Seguimiento reservado a DG; equivalente operativo usado en Excel.'),
(N'Ruta crítica', N'Proyecto con bloqueo habilitante, riesgo regulatorio, ambiental, social o de interconexión.'),
(N'Autoconsumo', N'Proyecto de abasto aislado, cogeneración, autoconsumo o regularización relacionada.'),
(N'Migraciones', N'Ruta de migración LIE/LSE/MEM o ajuste de modalidad.'),
(N'Ventanilla', N'Entrada inicial para confirmar ruta, expediente o factibilidad.'),
(N'Inviables', N'Sin interés, permiso terminado, no factible o sin ruta activa.');

INSERT INTO dgmesnie.CatPrioridad (Nombre, Orden) VALUES
(N'Alta', 1), (N'Media', 2), (N'Baja', 3), (N'Sin prioridad', 4);

INSERT INTO dgmesnie.CatSemaforo (Nombre, ColorHex, Descripcion) VALUES
(N'Rojo', '#C00000', N'Riesgo crítico, vencido o bloqueo relevante.'),
(N'Amarillo', '#FFC000', N'Pendiente relevante o vencimiento próximo.'),
(N'Verde', '#00B050', N'En tiempo o sin bloqueo actual.'),
(N'Gris', '#808080', N'Sin información suficiente.'),
(N'Punto', '#808080', N'Indicador importado desde Excel cuando solo existe símbolo ●.');

INSERT INTO dgmesnie.CatTipoDocumento (Nombre) VALUES
(N'Minuta'), (N'Ficha técnica'), (N'PPT'), (N'Oficio'), (N'Anexo'), (N'Otro');

INSERT INTO dgmesnie.CatValoracionMinuta (Nombre) VALUES
(N'EN CONSTRUCCIÓN'), (N'DETENIDO'), (N'PRUEBAS'), (N'OPERACIÓN'), (N'SIN AVANCE'), (N'EN DESARROLLO'), (N'REUNIÓN ESPECIAL');

INSERT INTO dgmesnie.CatEstatusTramite (Nombre) VALUES
(N'Autorizada'), (N'En evaluación'), (N'No ingresada'), (N'No presentado'),
(N'Por confirmar'), (N'Vigencia por confirmar'), (N'Por iniciar / vigencia por confirmar'),
(N'Pendiente / según ruta de modificación'), (N'El promovente No ha presentado a SEMARNAT el trámite');
`;

function parseDate(val) {
    if (!val) return null;
    const d = new Date(val);
    if (isNaN(d.getTime())) {
        return null;
    }
    return d;
}

async function main() {
    try {
        await sql.connect(config);
        console.log("Connected to database successfully!");

        // Execute DDL
        console.log("Running DDL statements...");
        await sql.query(ddl);
        console.log("DDL executed successfully!");

        // Seed catalogs
        console.log("Seeding catalogs...");
        await sql.query(seedCatalogsSql);
        console.log("Catalogs seeded successfully!");

        // Load JSON data
        const rawData = fs.readFileSync('excel_data.json', 'utf8');
        const data = JSON.parse(rawData);

        // 1. Technologies
        const techMap = {};
        for (const tName of data.technologies) {
            try {
                const res = await sql.query`INSERT INTO dgmesnie.CatTecnologia (Nombre) VALUES (${tName}); SELECT SCOPE_IDENTITY() AS id;`;
                techMap[tName.toUpperCase()] = res.recordset[0].id;
            } catch (err) {
                // If it already exists, select it
                const res = await sql.query`SELECT TecnologiaId FROM dgmesnie.CatTecnologia WHERE Nombre = ${tName};`;
                techMap[tName.toUpperCase()] = res.recordset[0].TecnologiaId;
            }
        }
        console.log("Technologies loaded.");

        // 2. Companies
        const companyMap = {};
        for (const cName of data.companies) {
            try {
                const res = await sql.query`INSERT INTO dgmesnie.Empresa (Nombre, CreadoPor) VALUES (${cName}, 'MigracionExcel'); SELECT SCOPE_IDENTITY() AS id;`;
                companyMap[cName.toUpperCase()] = res.recordset[0].id;
            } catch (err) {
                const res = await sql.query`SELECT EmpresaId FROM dgmesnie.Empresa WHERE Nombre = ${cName};`;
                companyMap[cName.toUpperCase()] = res.recordset[0].EmpresaId;
            }
        }
        console.log("Companies loaded.");

        // Fetch other catalogs maps
        const clasifRes = await sql.query`SELECT ClasificacionId, Nombre FROM dgmesnie.CatClasificacion`;
        const clasifMap = {};
        for (const row of clasifRes.recordset) {
            clasifMap[row.Nombre.toUpperCase()] = row.ClasificacionId;
        }

        const priorRes = await sql.query`SELECT PrioridadId, Nombre FROM dgmesnie.CatPrioridad`;
        const priorMap = {};
        for (const row of priorRes.recordset) {
            priorMap[row.Nombre.toUpperCase()] = row.PrioridadId;
        }

        const semRes = await sql.query`SELECT SemaforoId, Nombre FROM dgmesnie.CatSemaforo`;
        const semMap = {};
        for (const row of semRes.recordset) {
            semMap[row.Nombre.toUpperCase()] = row.SemaforoId;
        }

        // 3. Projects
        const projectMap = {}; // Key: Name -> ProjectId
        console.log(`Inserting ${data.projects.length} projects...`);
        for (const p of data.projects) {
            const techKey = p.Tipo ? p.Tipo.toUpperCase().trim() : null;
            const techId = techKey ? techMap[techKey] : null;

            const empKey = (p["Grupo Económico"] || p["Promovente"]) ? (p["Grupo Económico"] || p["Promovente"]).toUpperCase().trim() : null;
            const empId = empKey ? companyMap[empKey] : null;

            const clasifKey = p["Clasificación"] ? p["Clasificación"].toUpperCase().trim() : null;
            const clasifId = clasifKey ? clasifMap[clasifKey] : null;

            const priorKey = p["Prioridad"] ? p["Prioridad"].toUpperCase().trim() : null;
            const priorId = priorKey ? priorMap[priorKey] : null;

            // Map semaphore color or name to SemaforoId
            let semId = null;
            const semVal = p["Semáforo"] ? p["Semáforo"].trim() : null;
            if (semVal) {
                if (semVal === '●' || semVal === 'Punto') {
                    semId = semMap['PUNTO'];
                } else {
                    semId = semMap[semVal.toUpperCase()] || null;
                }
            }

            const pInsert = await sql.query`
                INSERT INTO dgmesnie.Proyecto (
                    Status, Nombre, EmpresaId, Promovente, GrupoEconomico, Origen, Anio,
                    AdicionesSustituciones, ContratoUnidad, Tipo, Renovable, CapacidadMW, Mes,
                    GerenciaControl, RegionTransmision, EntidadFederativa, Municipio, Longitud, Latitud,
                    Firmes, PorcentajeConstruccion, FolioEvIS_MISSE, StatusEvIS_MISSE, StatusCPLI,
                    ConflictosSociales, NumeroPermiso, FechaInicioObras, FechaTerminacionObras,
                    FechaEntradaOperacion, EstadoProgramaObras, TramiteCNE, ObservacionesCNE,
                    Categoria, InteresadaEnContinuar, ObservacionesUEVISPI, ResumenCaso,
                    PropuestaAtencion, RequiereAlmacenamiento, SiguientesPasos, ClasificacionId,
                    PrioridadId, SemaforoId, RazonesBreves, TramitesSemarnat, EstatusSemarnat,
                    ObservacionesSemarnat, Fuente, SemaforoPPT, FechaUltimaActualizacion,
                    FuenteUltimaActualizacion, CreadoPor
                ) VALUES (
                    ${p["Status"]}, ${p["Nombre real"]}, ${empId}, ${p["Promovente"]}, ${p["Grupo Económico"]}, ${p["Origen"]}, ${p["Año"]},
                    ${p["Adiciones o sustituciones"]}, ${p["Contrato o unidad"]}, ${p["Tipo"]}, ${p["Renovable"] === 'SÍ' || p["Renovable"] === 'S' || p["Renovable"] === true ? 1 : 0}, 
                    ${p["MW"] ? parseFloat(p["MW"]) : null}, ${p["Mes"]}, ${p["Gerencia de control"]}, ${p["Región de transmisión"]}, ${p["Entidad Federativa"]}, ${p["Municipio"]}, 
                    ${p["Longitud"] ? parseFloat(p["Longitud"]) : null}, ${p["Latitud"] ? parseFloat(p["Latitud"]) : null},
                    ${p["Firmes"]}, ${p["(%) Construcción"] ? parseFloat(p["(%) Construcción"]) : null}, ${p["Folio EvIS/MISSE"]}, ${p["Status EvIS/MISSE"]}, ${p["Status CPLI"]},
                    ${p["Conflictos sociales detectados"]}, ${p["Número de Permiso"]}, 
                    ${parseDate(p["Inicio de Obras"])}, 
                    ${parseDate(p["Terminación de Obras"])},
                    ${parseDate(p["Entrada en Operación"])}, 
                    ${p["Estado actual del Programa de Obras"]}, ${p["Trámite en proceso con CNE"]}, ${p["Observaciones CNE"]},
                    ${p["Categoría"]}, ${p["Interesada en Continuar"] === 'SÍ' || p["Interesada en Continuar"] === true ? 1 : 0}, ${p["Observaciones UEVISPI"]}, ${p["Resumen del Caso"]},
                    ${p["Propuesta de Atención"]}, ${p["Requiere Almacenamiento"]}, ${p["Siguientes Pasos"]}, ${clasifId},
                    ${priorId}, ${semId}, ${p["Razones (Breves)"]}, ${p["Trámites SEMARNAT"]}, ${p["Estatus SEMARNAT"]},
                    ${p["Observaciones SEMARNAT"]}, ${p["Fuente"]}, ${p["Semáforo PPT"]}, 
                    ${parseDate(p["Última actualización"])},
                    ${p["Fuente última actualización"]}, 'MigracionExcel'
                );
                SELECT SCOPE_IDENTITY() AS id;
            `;
            const pId = pInsert.recordset[0].id;
            projectMap[p["Nombre real"].toUpperCase().trim()] = pId;
        }
        console.log("Projects loaded successfully.");

        // 4. Document / Reunion / Bitacora
        // Retrieve valoracion minuta mapping
        const valRes = await sql.query`SELECT ValoracionMinutaId, Nombre FROM dgmesnie.CatValoracionMinuta`;
        const valMap = {};
        for (const row of valRes.recordset) {
            valMap[row.Nombre.toUpperCase()] = row.ValoracionMinutaId;
        }

        console.log(`Inserting ${data.bitacoras.length} bitacora entries...`);
        
        // Cache created reunions and documents
        const reunionCache = {}; // key: date + filename -> ReunionId
        const docCache = {}; // key: filename -> DocumentoId

        for (const b of data.bitacoras) {
            const pName = b["Proyecto"] ? b["Proyecto"].trim() : null;
            const pId = pName ? projectMap[pName.toUpperCase()] : null;

            if (!pId) {
                console.log(`Warning: Project '${pName}' from bitacora not matched to any project.`);
                continue;
            }

            const meetingDateStr = b["Fecha reunión"];
            const filename = b["Minuta (archivo)"] ? b["Minuta (archivo)"].trim() : "Minuta Sin Nombre";
            const meetingDate = parseDate(meetingDateStr) || new Date();

            // Resolve Documento
            let docId = null;
            if (filename) {
                const docKey = filename.toUpperCase();
                if (docCache[docKey]) {
                    docId = docCache[docKey];
                } else {
                    const docInsert = await sql.query`
                        INSERT INTO dgmesnie.Documento (TipoDocumentoId, Titulo, NombreArchivo, FechaDocumento, SubidoPor)
                        VALUES (1, ${filename}, ${filename}, ${meetingDate}, 'MigracionExcel');
                        SELECT SCOPE_IDENTITY() AS id;
                    `;
                    docId = docInsert.recordset[0].id;
                    docCache[docKey] = docId;
                }
            }

            // Resolve Reunion
            let reunionId = null;
            const reunionKey = `${meetingDate.toISOString()}_${filename.toUpperCase()}`;
            if (reunionCache[reunionKey]) {
                reunionId = reunionCache[reunionKey];
            } else {
                const reunionInsert = await sql.query`
                    INSERT INTO dgmesnie.Reunion (Titulo, FechaReunion, DocumentoId, CreadoPor)
                    VALUES (${filename}, ${meetingDate}, ${docId}, 'MigracionExcel');
                    SELECT SCOPE_IDENTITY() AS id;
                `;
                reunionId = reunionInsert.recordset[0].id;
                reunionCache[reunionKey] = reunionId;
            }

            // Resolve Valoracion Minuta
            const valText = b["Valoración minuta"] ? b["Valoración minuta"].toUpperCase().trim() : null;
            const valId = valText ? valMap[valText] : null;

            // Fetch the project data to create the initial snapshot JSON
            const projDataRes = await sql.query`SELECT * FROM dgmesnie.Proyecto WHERE ProyectoId = ${pId}`;
            const projObj = projDataRes.recordset[0];
            const snapshotJson = JSON.stringify(projObj);

            // Insert BitacoraProyecto
            await sql.query`
                INSERT INTO dgmesnie.BitacoraProyecto (
                    ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionMinutaId, ValoracionTexto,
                    ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones, SnapshotProyectoJSON,
                    ProcesadoPor, CreadoPor
                ) VALUES (
                    ${pId}, ${reunionId}, ${docId}, ${meetingDate}, ${valId}, ${b["Valoración minuta"]},
                    ${b["Resumen acuerdos"]}, ${b["Compromisos / Siguientes pasos"]}, ${b["Riesgos / Observaciones"]},
                    ${snapshotJson}, ${b["Procesado por"]}, 'MigracionExcel'
                )
            `;
        }
        console.log("Bitacora entries loaded successfully.");

        // 5. Register the new Module in dgmesnie.Modulo under section 43
        console.log("Registering module in dgmesnie.Modulo...");
        
        // Let's check if the module already exists
        const checkMod = await sql.query`
            SELECT ModuloId FROM dgmesnie.Modulo WHERE Controller = 'ProyectosPrivados' AND Action = 'Index'
        `;
        let modId;
        if (checkMod.recordset.length === 0) {
            const insertMod = await sql.query`
                INSERT INTO dgmesnie.Modulo (
                    SeccionId, Title, FundamentoLegalModulo, Perfiles, Etapa, JustificacionOrden,
                    AyudaContextual, Controller, Action, Descripcion, Imagen, BotonTexto, ElementosUI,
                    AyudaVista, Orden, Activo, EsExterno
                ) VALUES (
                    43, N'Proyectos Privados Firmes', N'Seguimiento interno de proyectos energéticos privados',
                    N'Administrador,Consulta,Captura', N'Seguimiento', N'Módulo para el control de la cartera de proyectos firmes',
                    N'Acceso al dashboard y CRUD de proyectos privados', 'ProyectosPrivados', 'Index',
                    N'Seguimiento, bitácora de reuniones y trazabilidad de proyectos privados', 'proyecto.png',
                    N'Ver Proyectos', NULL, NULL, 3, 1, 0
                );
                SELECT SCOPE_IDENTITY() AS id;
            `;
            modId = insertMod.recordset[0].id;
            console.log(`Module registered with ModuloId: ${modId}`);

            // Associate with roles (RolId: 1 for Administrador, 2 for Consulta, 3 for Captura, etc.)
            // Let's query active roles
            const rolesRes = await sql.query`SELECT RolId FROM dgmesnie.Rol WHERE RolVigente = 1`;
            for (const row of rolesRes.recordset) {
                await sql.query`
                    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, Activa)
                    VALUES (${row.RolId}, ${modId}, 1)
                `;
            }
            console.log("Module mapped to active roles successfully.");
        } else {
            console.log("Module already registered.");
        }

        console.log("DATABASE SETUP COMPLETED SUCCESSFULLY!");
    } catch (err) {
        console.error("Database setup failed: ", err);
    } finally {
        await sql.close();
    }
}

main();
