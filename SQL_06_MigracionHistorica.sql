/* ==========================================================
   DG MESNIE - SISTEMA DE CONSOLIDACIÓN DE PROYECTOS ENERGÉTICOS
   SCRIPT SQL DE MIGRACIÓN: TRANSFERENCIA DE DGMESNIE A CORE
   ========================================================== */

BEGIN TRANSACTION;
BEGIN TRY

    PRINT 'Iniciando migración de datos históricos de dgmesnie a core...';

    -- 1. Insertar Grupos de Interés Económico (GIE)
    PRINT 'Migrando Grupos Económicos...';
    INSERT INTO core.GrupoInteresEconomico (Nombre)
    SELECT DISTINCT UPPER(RTRIM(LTRIM(GrupoEconomico)))
    FROM dgmesnie.Empresa
    WHERE GrupoEconomico IS NOT NULL AND RTRIM(LTRIM(GrupoEconomico)) <> ''
      AND UPPER(RTRIM(LTRIM(GrupoEconomico))) NOT IN (SELECT Nombre FROM core.GrupoInteresEconomico);

    -- 2. Migrar Empresas / Actores
    PRINT 'Migrando Actores / Empresas...';
    SET IDENTITY_INSERT core.Actor ON;
    
    INSERT INTO core.Actor (ActorId, RazonSocial, RFC, PaisOrigen, GrupoEconomicoId, Observaciones, Activo)
    SELECT 
        e.EmpresaId,
        e.Nombre,
        NULL, -- RFC no estaba en Empresa
        e.PaisOrigen,
        g.GrupoEconomicoId,
        e.Observaciones,
        e.Activo
    FROM dgmesnie.Empresa e
    LEFT JOIN core.GrupoInteresEconomico g ON g.Nombre = UPPER(RTRIM(LTRIM(e.GrupoEconomico)))
    WHERE e.Nombre NOT IN (SELECT RazonSocial FROM core.Actor);

    SET IDENTITY_INSERT core.Actor OFF;

    -- 3. Mapear Estados INEGI (Semillas para CatEntidadFederativa y CatMunicipio)
    PRINT 'Pre-sembrando Catálogos de Ubicaciones (Estados de México)...';
    INSERT INTO core.CatEntidadFederativa (EntidadFederativaId, Nombre)
    VALUES 
    (1, N'Aguascalientes'), (2, N'Baja California'), (3, N'Baja California Sur'), 
    (4, N'Campeche'), (5, N'Coahuila'), (6, N'Colima'), (7, N'Chiapas'), 
    (8, N'Chihuahua'), (9, N'Ciudad de México'), (10, N'Durango'), 
    (11, N'Guanajuato'), (12, N'Guerrero'), (13, N'Hidalgo'), (14, N'Jalisco'), 
    (15, N'México'), (16, N'Michoacán'), (17, N'Morelos'), (18, N'Nayarit'), 
    (19, N'Nuevo León'), (20, N'Oaxaca'), (21, N'Puebla'), (22, N'Querétaro'), 
    (23, N'Quintana Roo'), (24, N'San Luis Potosí'), (25, N'Sinaloa'), 
    (26, N'Sonora'), (27, N'Tabasco'), (28, N'Tamaulipas'), (29, N'Tlaxcala'), 
    (30, N'Veracruz'), (31, N'Yucatán'), (32, N'Zacatecas'),
    (99, N'No Especificado'); -- Para valores sucios o nulos

    -- Inserción de Municipios genéricos por estado para evitar violaciones de llave foránea
    PRINT 'Sembrando municipios genéricos...';
    INSERT INTO core.CatMunicipio (MunicipioId, EntidadFederativaId, Nombre)
    SELECT e.EntidadFederativaId * 1000 + 1, e.EntidadFederativaId, N'Municipio General ' + e.Nombre
    FROM core.CatEntidadFederativa e
    WHERE NOT EXISTS (SELECT 1 FROM core.CatMunicipio m WHERE m.EntidadFederativaId = e.EntidadFederativaId);

    -- 4. Migrar Proyectos
    PRINT 'Migrando Proyectos...';
    SET IDENTITY_INSERT core.Proyecto ON;

    INSERT INTO core.Proyecto (
        ProyectoId, Status, NombreOficial, NombreCorto, Descripcion, TecnologiaId, EstatusProyectoId,
        NivelMadurezId, ClasificacionId, PrioridadId, SemaforoId, Renovable, CapacidadMW, Activo, 
        CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
    )
    SELECT 
        p.ProyectoId,
        p.Status,
        p.Nombre,
        p.NombreNormalizado, -- nombre corto temporal
        p.ResumenCaso,
        -- Homologar catálogos
        (SELECT TOP 1 coreT.TecnologiaId 
         FROM core.CatTecnologia coreT 
         WHERE coreT.Nombre = p.Tipo),
        -- Estatus proyecto (mapear a estatus por defecto o buscar coincidencia)
        (SELECT TOP 1 EstatusProyectoId FROM core.CatEstatusProyecto WHERE Nombre = N'En Desarrollo'), 
        1, -- Nivel de madurez inicial por defecto
        p.ClasificacionId,
        p.PrioridadId,
        p.SemaforoId,
        p.Renovable,
        p.CapacidadMW,
        p.Activo,
        p.CreadoEn,
        p.CreadoPor,
        p.ActualizadoEn,
        p.ActualizadoPor
    FROM dgmesnie.Proyecto p;

    SET IDENTITY_INSERT core.Proyecto OFF;

    -- 5. Crear relación Proyecto - Actor (Promovente)
    PRINT 'Creando asociaciones Proyecto - Actor (Promovente)...';
    INSERT INTO core.ProyectoActor (ProyectoId, ActorId, TipoActorId)
    SELECT 
        p.ProyectoId,
        p.EmpresaId,
        1 -- Promovente
    FROM dgmesnie.Proyecto p
    WHERE p.EmpresaId IS NOT NULL;

    -- 6. Insertar ubicaciones
    PRINT 'Creando ubicaciones de proyectos...';
    INSERT INTO core.ProyectoUbicacion (
        ProyectoId, EntidadFederativaId, MunicipioId, Localidad, EsPrincipal, RegionTransmision
    )
    SELECT 
        p.ProyectoId,
        -- Intentar resolver el ID de la Entidad Federativa
        ISNULL((SELECT TOP 1 EntidadFederativaId FROM core.CatEntidadFederativa WHERE Nombre LIKE '%' + p.EntidadFederativa + '%'), 99),
        -- Asignar el municipio genérico del estado resuelto
        ISNULL((SELECT TOP 1 MunicipioId FROM core.CatMunicipio WHERE EntidadFederativaId = (SELECT TOP 1 EntidadFederativaId FROM core.CatEntidadFederativa WHERE Nombre LIKE '%' + p.EntidadFederativa + '%')), 99001),
        p.Municipio,
        1, -- EsPrincipal
        p.RegionTransmision
    FROM dgmesnie.Proyecto p
    WHERE p.EntidadFederativa IS NOT NULL;

    -- 7. Insertar Coordenadas
    PRINT 'Creando coordenadas de proyectos...';
    INSERT INTO core.ProyectoCoordenada (ProyectoId, Latitud, Longitud, Secuencia, Descripcion)
    SELECT 
        p.ProyectoId,
        p.Latitud,
        p.Longitud,
        1,
        N'Coordenada base'
    FROM dgmesnie.Proyecto p
    WHERE p.Latitud IS NOT NULL AND p.Longitud IS NOT NULL;

    -- 8. Crear Datos Técnicos (Almacenamiento / BESS e Hibridación)
    PRINT 'Creando detalles técnicos...';
    INSERT INTO core.ProyectoDatosTecnicos (
        ProyectoId, CapacidadInstaladaMW, AlmacenamientoBess, CapacidadBessMW, CapacidadBessMWh, HorasAlmacenamiento, EsHibrido, ComentariosTecnicos
    )
    SELECT 
        p.ProyectoId,
        ISNULL(p.CapacidadMW, 0.0),
        CASE WHEN p.RequiereAlmacenamiento IS NOT NULL AND p.RequiereAlmacenamiento <> '' THEN 1 ELSE 0 END,
        NULL,
        NULL,
        NULL,
        0, -- EsHibrido por defecto
        p.EstadoProgramaObras
    FROM dgmesnie.Proyecto p;

    -- 9. Crear Datos Financieros
    PRINT 'Creando detalles financieros...';
    INSERT INTO core.ProyectoDatosFinancieros (
        ProyectoId, CAPEX, MonedaId, FuenteFinanciamiento, NombreEPC, ProveedoresPrincipales, TipoFinanciamiento
    )
    SELECT 
        p.ProyectoId,
        NULL, -- CAPEX no estaba en el modelo original
        1, -- MonedaId = 1 (MXN por defecto)
        p.Fuente,
        p.Promovente,
        NULL,
        NULL
    FROM dgmesnie.Proyecto p;

    -- 10. Migrar Trámites
    PRINT 'Migrando Trámites...';
    SET IDENTITY_INSERT core.ProyectoTramite ON;
    
    INSERT INTO core.ProyectoTramite (
        TramiteId, ProyectoId, AutoridadId, TipoTramiteId, EstatusTramiteId, Folio,
        FechaIngreso, FechaResolucion, FechaVencimiento, Observaciones, Activo,
        CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
    )
    SELECT 
        t.TramiteProyectoId,
        t.ProyectoId,
        -- Resolver Autoridad
        ISNULL((SELECT TOP 1 AutoridadId FROM core.CatAutoridad WHERE Acronimo = t.Autoridad), 7), -- CFE por defecto si es nulo
        -- Resolver Tipo de trámite
        ISNULL((SELECT TOP 1 TipoTramiteId FROM core.CatTipoTramite WHERE Nombre LIKE '%' + t.TipoTramite + '%'), 4), -- Permiso Generación
        -- Estatus
        ISNULL(t.EstatusTramiteId, 1),
        t.Folio,
        t.FechaIngreso,
        t.FechaResolucion,
        t.FechaVencimiento,
        t.Observaciones,
        t.Activo,
        t.CreadoEn,
        t.CreadoPor,
        t.ActualizadoEn,
        t.ActualizadoPor
    FROM dgmesnie.TramiteProyecto t;

    SET IDENTITY_INSERT core.ProyectoTramite OFF;

    -- 11. Migrar Documentos (SharePoint links)
    PRINT 'Migrando metadatos de Documentos...';
    SET IDENTITY_INSERT core.Documento ON;

    INSERT INTO core.Documento (
        DocumentoId, TipoDocumentoId, Titulo, SharePointUrl, SharePointItemId,
        SharePointDriveId, NombreArchivo, FechaDocumento, Descripcion, SubidoPor, SubidoEn, Activo
    )
    SELECT 
        d.DocumentoId,
        d.TipoDocumentoId,
        d.Titulo,
        d.SharePointUrl,
        d.SharePointItemId,
        d.SharePointDriveId,
        d.NombreArchivo,
        d.FechaDocumento,
        d.Descripcion,
        d.SubidoPor,
        d.SubidoEn,
        d.Activo
    FROM dgmesnie.Documento d;

    SET IDENTITY_INSERT core.Documento OFF;

    -- 12. Migrar Reuniones
    PRINT 'Migrando Reuniones...';
    SET IDENTITY_INSERT core.Reunion ON;

    INSERT INTO core.Reunion (
        ReunionId, Titulo, FechaReunion, Modalidad, Lugar, Objetivo, Asistentes, DocumentoId, CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
    )
    SELECT 
        r.ReunionId,
        r.Titulo,
        r.FechaReunion,
        r.Modalidad,
        r.Lugar,
        r.Objetivo,
        r.Asistentes,
        r.DocumentoId,
        r.CreadoEn,
        r.CreadoPor,
        r.ActualizadoEn,
        r.ActualizadoPor
    FROM dgmesnie.Reunion r;

    SET IDENTITY_INSERT core.Reunion OFF;

    -- 13. Migrar Bitácoras
    PRINT 'Migrando Entradas de Bitácora...';
    SET IDENTITY_INSERT core.BitacoraProyecto ON;

    INSERT INTO core.BitacoraProyecto (
        BitacoraProyectoId, ProyectoId, ReunionId, DocumentoId, FechaEvento, ValoracionMinutaId,
        ValoracionTexto, ResumenAcuerdos, CompromisosSiguientesPasos, RiesgosObservaciones,
        SnapshotProyectoJSON, ProcesadoPor, CreadoEn, CreadoPor
    )
    SELECT 
        b.BitacoraProyectoId,
        b.ProyectoId,
        b.ReunionId,
        b.DocumentoId,
        b.FechaEvento,
        b.ValoracionMinutaId,
        b.ValoracionTexto,
        b.ResumenAcuerdos,
        b.CompromisosSiguientesPasos,
        b.RiesgosObservaciones,
        b.SnapshotProyectoJSON,
        b.ProcesadoPor,
        b.CreadoEn,
        b.CreadoPor
    FROM dgmesnie.BitacoraProyecto b;

    SET IDENTITY_INSERT core.BitacoraProyecto OFF;

    -- 14. Migrar Acciones de Seguimiento (Compromisos)
    PRINT 'Migrando Compromisos de Seguimiento...';
    SET IDENTITY_INSERT core.AccionSeguimiento ON;

    INSERT INTO core.AccionSeguimiento (
        AccionId, ProyectoId, BitacoraProyectoId, Titulo, Descripcion, ResponsableUsuarioId,
        ResponsableNombre, FechaCompromiso, FechaCierre, Estatus, SemaforoId, Comentarios, CreadoEn, CreadoPor, ActualizadoEn, ActualizadoPor
    )
    SELECT 
        a.AccionId,
        a.ProyectoId,
        a.BitacoraProyectoId,
        a.Titulo,
        a.Descripcion,
        a.ResponsableUsuarioId,
        a.ResponsableNombre,
        a.FechaCompromiso,
        a.FechaCierre,
        a.Estatus,
        a.SemaforoId,
        a.Comentarios,
        a.CreadoEn,
        a.CreadoPor,
        a.ActualizadoEn,
        a.ActualizadoPor
    FROM dgmesnie.AccionSeguimiento a;

    SET IDENTITY_INSERT core.AccionSeguimiento OFF;

    -- 15. Migrar Historial de Cambios
    PRINT 'Migrando Historial de Cambios...';
    SET IDENTITY_INSERT core.HistorialProyecto ON;

    INSERT INTO core.HistorialProyecto (
        HistorialProyectoId, ProyectoId, BitacoraProyectoId, DocumentoId, Campo, ValorAnterior, ValorNuevo, MotivoCambio, CambiadoEn, CambiadoPor
    )
    SELECT 
        h.HistorialProyectoId,
        h.ProyectoId,
        h.BitacoraProyectoId,
        h.DocumentoId,
        h.Campo,
        h.ValorAnterior,
        h.ValorNuevo,
        h.MotivoCambio,
        h.CambiadoEn,
        h.CambiadoPor
    FROM dgmesnie.HistorialProyecto h;

    SET IDENTITY_INSERT core.HistorialProyecto OFF;

    -- 16. Insertar Identificadores iniciales
    PRINT 'Creando Identificadores Únicos base...';
    INSERT INTO core.ProyectoIdentificador (ProyectoId, OrigenDatosId, ClaveExterna, NombreEnOrigen)
    SELECT 
        p.ProyectoId,
        4, -- PVIRCE_Oficial
        N'MIGRACION-' + CAST(p.ProyectoId AS NVARCHAR(30)),
        p.Nombre
    FROM dgmesnie.Proyecto p;

    COMMIT TRANSACTION;
    PRINT 'MIGRACIÓN COMPLETADA CON ÉXITO!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR DETECTADO EN LA MIGRACIÓN: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
