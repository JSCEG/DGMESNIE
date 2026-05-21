-- ============================================================
-- GESTOR DE ACTIVIDADES - DDL + SEED
-- Esquema: dgmesnie
-- Ejecutar en la base de datos activa del portal NSIE
-- ============================================================

-- --- Temas ---------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('[dgmesnie].[Gestor_Temas]'))
BEGIN
    CREATE TABLE [dgmesnie].[Gestor_Temas] (
        [TemaId]                    INT             IDENTITY(1,1) NOT NULL,
        [Clave]                     NVARCHAR(20)    NOT NULL,
        [Tema]                      NVARCHAR(300)   NOT NULL,
        [Descripcion]               NVARCHAR(1000)  NULL,
        [Categoria]                 NVARCHAR(100)   NULL,
        [Prioridad]                 NVARCHAR(20)    NOT NULL DEFAULT 'Media',
        [Estatus]                   NVARCHAR(50)    NOT NULL DEFAULT 'Activo',
        [ResponsablePrincipalId]    INT             NULL,
        [FechaInicio]               DATE            NULL,
        [FechaCompromiso]           DATE            NULL,
        [AvanceGeneral]             INT             NOT NULL DEFAULT 0,
        [LigaSharePoint]            NVARCHAR(500)   NULL,
        [ComentariosEjecutivos]     NVARCHAR(2000)  NULL,
        [FechaUltimaActualizacion]  DATETIME2       NOT NULL DEFAULT GETDATE(),
        [FechaCreacion]             DATETIME2       NOT NULL DEFAULT GETDATE(),
        [CreadoPor]                 INT             NULL,
        [Activo]                    BIT             NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Gestor_Temas] PRIMARY KEY ([TemaId]),
        CONSTRAINT [FK_Gestor_Temas_Usuario] FOREIGN KEY ([ResponsablePrincipalId])
            REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

-- --- Actividades ---------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('[dgmesnie].[Gestor_Actividades]'))
BEGIN
    CREATE TABLE [dgmesnie].[Gestor_Actividades] (
        [ActividadId]               INT             IDENTITY(1,1) NOT NULL,
        [Clave]                     NVARCHAR(20)    NOT NULL,
        [TemaId]                    INT             NOT NULL,
        [Actividad]                 NVARCHAR(300)   NOT NULL,
        [Descripcion]               NVARCHAR(1000)  NULL,
        [ResponsableId]             INT             NULL,
        [FechaInicio]               DATE            NULL,
        [FechaCompromiso]           DATE            NULL,
        [Estatus]                   NVARCHAR(50)    NOT NULL DEFAULT 'Pendiente',
        [Prioridad]                 NVARCHAR(20)    NOT NULL DEFAULT 'Media',
        [Avance]                    INT             NOT NULL DEFAULT 0,
        [Bloqueada]                 BIT             NOT NULL DEFAULT 0,
        [MotivoBloqueO]             NVARCHAR(500)   NULL,
        [EvidenciaUrl]              NVARCHAR(500)   NULL,
        [Comentarios]               NVARCHAR(2000)  NULL,
        [FechaUltimaActualizacion]  DATETIME2       NOT NULL DEFAULT GETDATE(),
        [FechaCreacion]             DATETIME2       NOT NULL DEFAULT GETDATE(),
        [CreadoPor]                 INT             NULL,
        [Activo]                    BIT             NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Gestor_Actividades] PRIMARY KEY ([ActividadId]),
        CONSTRAINT [FK_Gestor_Actividades_Tema] FOREIGN KEY ([TemaId])
            REFERENCES [dgmesnie].[Gestor_Temas]([TemaId]),
        CONSTRAINT [FK_Gestor_Actividades_Usuario] FOREIGN KEY ([ResponsableId])
            REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

-- --- Corresponsables -----------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('[dgmesnie].[Gestor_Corresponsables]'))
BEGIN
    CREATE TABLE [dgmesnie].[Gestor_Corresponsables] (
        [CorresponsableId]  INT  IDENTITY(1,1) NOT NULL,
        [TemaId]            INT  NULL,
        [ActividadId]       INT  NULL,
        [IdUsuario]         INT  NOT NULL,
        CONSTRAINT [PK_Gestor_Corresponsables] PRIMARY KEY ([CorresponsableId]),
        CONSTRAINT [FK_Corresponsables_Tema] FOREIGN KEY ([TemaId])
            REFERENCES [dgmesnie].[Gestor_Temas]([TemaId]),
        CONSTRAINT [FK_Corresponsables_Actividad] FOREIGN KEY ([ActividadId])
            REFERENCES [dgmesnie].[Gestor_Actividades]([ActividadId]),
        CONSTRAINT [FK_Corresponsables_Usuario] FOREIGN KEY ([IdUsuario])
            REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [CK_Corresponsables_RefUnica]
            CHECK (([TemaId] IS NOT NULL AND [ActividadId] IS NULL)
                OR ([TemaId] IS NULL AND [ActividadId] IS NOT NULL))
    );

    CREATE INDEX [IX_Corresponsables_Tema]      ON [dgmesnie].[Gestor_Corresponsables]([TemaId]);
    CREATE INDEX [IX_Corresponsables_Actividad] ON [dgmesnie].[Gestor_Corresponsables]([ActividadId]);
END
GO

-- --- Indices adicionales -------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Temas_Activo')
    CREATE INDEX [IX_Gestor_Temas_Activo] ON [dgmesnie].[Gestor_Temas]([Activo]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Actividades_TemaId')
    CREATE INDEX [IX_Gestor_Actividades_TemaId] ON [dgmesnie].[Gestor_Actividades]([TemaId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Actividades_Activo')
    CREATE INDEX [IX_Gestor_Actividades_Activo] ON [dgmesnie].[Gestor_Actividades]([Activo]);
GO

-- --- SEED: 5 Temas -------------------------------------------
-- NOTA: Si los usuarios del sistema no tienen IdUsuario conocido, ajusta
--       los valores NULL o reemplaza por un IdUsuario real de tu entorno.
SET IDENTITY_INSERT [dgmesnie].[Gestor_Temas] ON;
GO

MERGE [dgmesnie].[Gestor_Temas] AS target
USING (VALUES
    (1, 'T-001', N'Actualización del Sistema de Información Energética',
     N'Modernización de plataforma SIE con integración de fuentes oficiales.',
     N'Tecnología', N'Alta', N'Activo', NULL,
     '2026-03-01', '2026-07-30', 45,
     N'https://sharepoint.example.com/sites/SIE',
     N'Avance dentro de plazo. Pendiente validación con DTI.',
     '2026-05-15'),
    (2, 'T-002', N'Reporte Anual de Indicadores Energéticos',
     N'Integración del informe institucional anual 2025.',
     N'Reportes', N'Alta', N'Activo', NULL,
     '2026-04-10', '2026-06-15', 70,
     N'https://sharepoint.example.com/sites/Reportes',
     N'Falta validación de Subsecretaría.',
     '2026-05-18'),
    (3, 'T-003', N'Convocatoria de Proyectos Energéticos 2026',
     N'Seguimiento a la primera convocatoria de proyectos.',
     N'Operativo', N'Media', N'Activo', NULL,
     '2026-02-15', '2026-05-25', 85,
     N'https://sharepoint.example.com/sites/Convocatoria',
     N'Próximo a concluir.',
     '2026-05-17'),
    (4, 'T-004', N'Diagnóstico de Litio en México',
     N'Levantamiento técnico nacional del recurso litio.',
     N'Estudios', N'Alta', N'Activo', NULL,
     '2026-01-20', '2026-09-30', 30,
     N'https://sharepoint.example.com/sites/Litio',
     N'Pendiente coordinación con CFE.',
     '2026-04-30'),
    (5, 'T-005', N'Capacitación Interna en Análisis de Datos',
     N'Programa de formación para personal de la DG.',
     N'Capacitación', N'Baja', N'Pausado', NULL,
     '2026-03-15', '2026-08-15', 20,
     N'',
     N'Pausado por reorganización.',
     '2026-04-01')
) AS source (TemaId, Clave, Tema, Descripcion, Categoria, Prioridad, Estatus,
             ResponsablePrincipalId, FechaInicio, FechaCompromiso, AvanceGeneral,
             LigaSharePoint, ComentariosEjecutivos, FechaUltimaActualizacion)
ON target.TemaId = source.TemaId
WHEN NOT MATCHED THEN
    INSERT (TemaId, Clave, Tema, Descripcion, Categoria, Prioridad, Estatus,
            ResponsablePrincipalId, FechaInicio, FechaCompromiso, AvanceGeneral,
            LigaSharePoint, ComentariosEjecutivos, FechaUltimaActualizacion)
    VALUES (source.TemaId, source.Clave, source.Tema, source.Descripcion,
            source.Categoria, source.Prioridad, source.Estatus, source.ResponsablePrincipalId,
            source.FechaInicio, source.FechaCompromiso, source.AvanceGeneral,
            source.LigaSharePoint, source.ComentariosEjecutivos, source.FechaUltimaActualizacion);
GO

SET IDENTITY_INSERT [dgmesnie].[Gestor_Temas] OFF;
GO

-- --- SEED: 12 Actividades ------------------------------------
SET IDENTITY_INSERT [dgmesnie].[Gestor_Actividades] ON;
GO

MERGE [dgmesnie].[Gestor_Actividades] AS target
USING (VALUES
    (1,  'A-001', 1, N'Levantamiento de requerimientos',         N'Mesas de trabajo con áreas usuarias.',
          NULL,'2026-03-01','2026-04-15',N'Concluida',  N'Alta',  100, 0, NULL, N'https://sharepoint.example.com/doc1', NULL,             '2026-04-15'),
    (2,  'A-002', 1, N'Diseño de arquitectura técnica',          N'Documento de arquitectura aprobado.',
          NULL,'2026-04-15','2026-05-30',N'En proceso', N'Alta',   60, 0, NULL, NULL,                                   N'Validación pendiente.','2026-05-10'),
    (3,  'A-003', 1, N'Desarrollo de módulo de captura',         N'Front + API.',
          NULL,'2026-05-15','2026-07-15',N'Pendiente',  N'Alta',    0, 0, NULL, NULL, NULL,                             '2026-05-01'),
    (4,  'A-004', 2, N'Recolección de datos de Subsecretarías',  NULL,
          NULL,'2026-04-10','2026-05-10',N'Concluida',  N'Alta',  100, 0, NULL, N'https://sharepoint.example.com/doc2', NULL,             '2026-05-10'),
    (5,  'A-005', 2, N'Integración del documento ejecutivo',     N'Compilación final.',
          NULL,'2026-05-10','2026-05-25',N'En proceso', N'Alta',   80, 0, NULL, NULL,                                   N'Revisión interna.','2026-05-18'),
    (6,  'A-006', 2, N'Diseño editorial e impresión',            N'Maquetación institucional.',
          NULL,'2026-05-25','2026-06-15',N'Pendiente',  N'Media',   0, 0, NULL, NULL, NULL,                             '2026-05-01'),
    (7,  'A-007', 3, N'Cierre de registro de participantes',     NULL,
          NULL,'2026-04-20','2026-05-15',N'Vencida',    N'Alta',   90, 1, N'Extensión solicitada por Subsecretaría', NULL, NULL,          '2026-05-12'),
    (8,  'A-008', 3, N'Evaluación técnica de proyectos',         N'Comité revisa propuestas.',
          NULL,'2026-05-15','2026-05-25',N'En proceso', N'Alta',   50, 0, NULL, NULL, NULL,                             '2026-05-19'),
    (9,  'A-009', 4, N'Mapeo de yacimientos',                    N'Coordinación con SGM.',
          NULL,'2026-01-20','2026-06-30',N'En proceso', N'Alta',   40, 0, NULL, NULL, NULL,                             '2026-04-25'),
    (10, 'A-010', 4, N'Análisis de marco regulatorio',           NULL,
          NULL,'2026-03-01','2026-05-22',N'En proceso', N'Media',  60, 0, NULL, NULL, NULL,                             '2026-05-05'),
    (11, 'A-011', 5, N'Diseño de programa',                      NULL,
          NULL,'2026-03-15','2026-04-30',N'Concluida',  N'Baja',  100, 0, NULL, NULL, NULL,                             '2026-04-28'),
    (12, 'A-012', 5, N'Ejecución de talleres',                   N'Pausada.',
          NULL,'2026-05-01','2026-08-15',N'Pendiente',  N'Baja',    0, 1, N'Reorganización interna', NULL, NULL,        '2026-04-01')
) AS source (ActividadId, Clave, TemaId, Actividad, Descripcion,
             ResponsableId, FechaInicio, FechaCompromiso, Estatus, Prioridad,
             Avance, Bloqueada, MotivoBloqueo, EvidenciaUrl, Comentarios,
             FechaUltimaActualizacion)
ON target.ActividadId = source.ActividadId
WHEN NOT MATCHED THEN
    INSERT (ActividadId, Clave, TemaId, Actividad, Descripcion,
            ResponsableId, FechaInicio, FechaCompromiso, Estatus, Prioridad,
            Avance, Bloqueada, MotivoBloqueO, EvidenciaUrl, Comentarios,
            FechaUltimaActualizacion)
    VALUES (source.ActividadId, source.Clave, source.TemaId, source.Actividad, source.Descripcion,
            source.ResponsableId, source.FechaInicio, source.FechaCompromiso, source.Estatus,
            source.Prioridad, source.Avance, source.Bloqueada, source.MotivoBloqueo,
            source.EvidenciaUrl, source.Comentarios, source.FechaUltimaActualizacion);
GO

SET IDENTITY_INSERT [dgmesnie].[Gestor_Actividades] OFF;
GO
