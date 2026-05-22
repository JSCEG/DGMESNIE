/*
    Módulo: Reuniones / Seguimiento de Asuntos
    Objetivo:
      1. Centralizar el seguimiento operativo de asuntos en el portal.
      2. Homologar cada asunto con su expediente y carpeta SharePoint.
      3. Mantener comentarios tipo chat histórico por usuario.
      4. Controlar el envío obligatorio a Mónica (Asuntos Jurídicos) y registrar quién lo marcó.

    Requisitos previos:
      - Existencia del esquema [dgmesnie].
      - Se recomienda aplicar después de DGMESNIE_ESQUEMA_BASE.sql.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF SCHEMA_ID(N'dgmesnie') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA dgmesnie');
END
GO

IF OBJECT_ID(N'[dgmesnie].[AsuntoSeguimiento]', N'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[AsuntoSeguimiento]
    (
        [AsuntoId] INT IDENTITY(1,1) NOT NULL,
        [GuidAsunto] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_GuidAsunto] DEFAULT (NEWID()),
        [NumeroRegistro] INT NULL,
        [TituloAsunto] NVARCHAR(300) NOT NULL,
        [Descripcion] NVARCHAR(MAX) NULL,
        [Expediente] NVARCHAR(100) NULL,
        [FechaSolicitud] DATE NULL,
        [DiasTranscurridos] INT NULL,
        [Responsable] NVARCHAR(200) NULL,
        [Encargado] NVARCHAR(200) NULL,
        [Minuta] NVARCHAR(200) NULL,
        [FichaInformativaOficio] NVARCHAR(200) NULL,
        [AreaResponsable] NVARCHAR(200) NULL,
        [Estatus] NVARCHAR(50) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_Estatus] DEFAULT (N'Pendiente'),
        [EstadoActual] NVARCHAR(MAX) NULL,
        [Prioridad] NVARCHAR(50) NULL,
        [TipoAsunto] NVARCHAR(100) NULL,
        [SemaforoManual] NVARCHAR(20) NULL,
        [FechaReunion] DATE NULL,
        [FechaCompromiso] DATE NULL,
        [FechaAtencion] DATE NULL,
        [CarpetaSharePointUrl] NVARCHAR(1000) NULL,
        [UbicacionCarpeta] NVARCHAR(300) NULL,
        [DatosContacto] NVARCHAR(MAX) NULL,
        [FechaTextoReunion] NVARCHAR(300) NULL,
        [RequiereEnvioMonica] BIT NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_RequiereEnvioMonica] DEFAULT (1),
        [EnviadoAMonica] BIT NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_EnviadoAMonica] DEFAULT (0),
        [FechaEnvioMonica] DATETIME2(0) NULL,
        [EnviadoAMonicaPorIdUsuario] INT NULL,
        [EnviadoAMonicaPorNombre] NVARCHAR(200) NULL,
        [ObservacionesEnvioMonica] NVARCHAR(1000) NULL,
        [DestinatarioJuridico] NVARCHAR(200) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_DestinatarioJuridico] DEFAULT (N'Mónica - Asuntos Jurídicos'),
        [Activo] BIT NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_Activo] DEFAULT (1),
        [IdUsuarioCreacion] INT NULL,
        [NombreUsuarioCreacion] NVARCHAR(200) NULL,
        [IdUsuarioUltimaActualizacion] INT NULL,
        [NombreUsuarioUltimaActualizacion] NVARCHAR(200) NULL,
        [FechaCreacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_FechaCreacion] DEFAULT (SYSUTCDATETIME()),
        [FechaActualizacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoSeguimiento_FechaActualizacion] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_AsuntoSeguimiento] PRIMARY KEY CLUSTERED ([AsuntoId]),
        CONSTRAINT [FK_dgmesnie_AsuntoSeguimiento_UsuarioEnvioMonica] FOREIGN KEY ([EnviadoAMonicaPorIdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [FK_dgmesnie_AsuntoSeguimiento_UsuarioCreacion] FOREIGN KEY ([IdUsuarioCreacion]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [FK_dgmesnie_AsuntoSeguimiento_UsuarioActualizacion] FOREIGN KEY ([IdUsuarioUltimaActualizacion]) REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'NumeroRegistro') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [NumeroRegistro] INT NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'FechaSolicitud') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [FechaSolicitud] DATE NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'DiasTranscurridos') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [DiasTranscurridos] INT NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'Encargado') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [Encargado] NVARCHAR(200) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'Minuta') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [Minuta] NVARCHAR(200) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'FichaInformativaOficio') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [FichaInformativaOficio] NVARCHAR(200) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'EstadoActual') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [EstadoActual] NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'TipoAsunto') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [TipoAsunto] NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'SemaforoManual') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [SemaforoManual] NVARCHAR(20) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'UbicacionCarpeta') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [UbicacionCarpeta] NVARCHAR(300) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'DatosContacto') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [DatosContacto] NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'dgmesnie.AsuntoSeguimiento', N'FechaTextoReunion') IS NULL
BEGIN
    ALTER TABLE [dgmesnie].[AsuntoSeguimiento] ADD [FechaTextoReunion] NVARCHAR(300) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_dgmesnie_AsuntoSeguimiento_EstatusFecha'
      AND object_id = OBJECT_ID(N'[dgmesnie].[AsuntoSeguimiento]')
)
BEGIN
    CREATE INDEX [IX_dgmesnie_AsuntoSeguimiento_EstatusFecha]
        ON [dgmesnie].[AsuntoSeguimiento]([Activo], [Estatus], [FechaCompromiso]);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_dgmesnie_AsuntoSeguimiento_Expediente'
      AND object_id = OBJECT_ID(N'[dgmesnie].[AsuntoSeguimiento]')
)
BEGIN
    CREATE INDEX [IX_dgmesnie_AsuntoSeguimiento_Expediente]
        ON [dgmesnie].[AsuntoSeguimiento]([Expediente]);
END
GO

IF OBJECT_ID(N'[dgmesnie].[AsuntoComentario]', N'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[AsuntoComentario]
    (
        [ComentarioId] BIGINT IDENTITY(1,1) NOT NULL,
        [AsuntoId] INT NOT NULL,
        [IdUsuario] INT NULL,
        [NombreUsuario] NVARCHAR(200) NOT NULL,
        [Mensaje] NVARCHAR(MAX) NOT NULL,
        [TipoMensaje] NVARCHAR(50) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoComentario_TipoMensaje] DEFAULT (N'Comentario'),
        [EsInterno] BIT NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoComentario_EsInterno] DEFAULT (1),
        [FechaComentario] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoComentario_FechaComentario] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_AsuntoComentario] PRIMARY KEY CLUSTERED ([ComentarioId]),
        CONSTRAINT [FK_dgmesnie_AsuntoComentario_AsuntoSeguimiento] FOREIGN KEY ([AsuntoId]) REFERENCES [dgmesnie].[AsuntoSeguimiento]([AsuntoId]),
        CONSTRAINT [FK_dgmesnie_AsuntoComentario_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_dgmesnie_AsuntoComentario_AsuntoFecha'
      AND object_id = OBJECT_ID(N'[dgmesnie].[AsuntoComentario]')
)
BEGIN
    CREATE INDEX [IX_dgmesnie_AsuntoComentario_AsuntoFecha]
        ON [dgmesnie].[AsuntoComentario]([AsuntoId], [FechaComentario]);
END
GO

IF OBJECT_ID(N'[dgmesnie].[AsuntoMovimiento]', N'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[AsuntoMovimiento]
    (
        [MovimientoId] BIGINT IDENTITY(1,1) NOT NULL,
        [AsuntoId] INT NOT NULL,
        [TipoMovimiento] NVARCHAR(100) NOT NULL,
        [Detalle] NVARCHAR(MAX) NULL,
        [ValorAnterior] NVARCHAR(1000) NULL,
        [ValorNuevo] NVARCHAR(1000) NULL,
        [IdUsuario] INT NULL,
        [NombreUsuario] NVARCHAR(200) NULL,
        [FechaMovimiento] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_AsuntoMovimiento_FechaMovimiento] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_AsuntoMovimiento] PRIMARY KEY CLUSTERED ([MovimientoId]),
        CONSTRAINT [FK_dgmesnie_AsuntoMovimiento_AsuntoSeguimiento] FOREIGN KEY ([AsuntoId]) REFERENCES [dgmesnie].[AsuntoSeguimiento]([AsuntoId]),
        CONSTRAINT [FK_dgmesnie_AsuntoMovimiento_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_dgmesnie_AsuntoMovimiento_AsuntoFecha'
      AND object_id = OBJECT_ID(N'[dgmesnie].[AsuntoMovimiento]')
)
BEGIN
    CREATE INDEX [IX_dgmesnie_AsuntoMovimiento_AsuntoFecha]
        ON [dgmesnie].[AsuntoMovimiento]([AsuntoId], [FechaMovimiento] DESC);
END
GO

CREATE OR ALTER VIEW [dgmesnie].[vw_AsuntoSeguimientoDashboard]
AS
    SELECT
        A.[AsuntoId],
        A.[GuidAsunto],
        A.[NumeroRegistro],
        A.[TituloAsunto],
        A.[Descripcion],
        A.[Expediente],
        A.[FechaSolicitud],
        COALESCE(A.[DiasTranscurridos], CASE WHEN A.[FechaSolicitud] IS NOT NULL THEN DATEDIFF(DAY, A.[FechaSolicitud], CONVERT(DATE, SYSUTCDATETIME())) END) AS [DiasTranscurridos],
        A.[Responsable],
        A.[Encargado],
        A.[Minuta],
        A.[FichaInformativaOficio],
        A.[AreaResponsable],
        A.[Estatus],
        A.[EstadoActual],
        A.[Prioridad],
        A.[TipoAsunto],
        A.[SemaforoManual],
        A.[FechaReunion],
        A.[FechaCompromiso],
        A.[FechaAtencion],
        COALESCE(A.[CarpetaSharePointUrl], A.[UbicacionCarpeta], A.[Expediente]) AS [CarpetaSharePointUrl],
        A.[UbicacionCarpeta],
        A.[DatosContacto],
        A.[FechaTextoReunion],
        A.[RequiereEnvioMonica],
        A.[EnviadoAMonica],
        A.[FechaEnvioMonica],
        A.[EnviadoAMonicaPorIdUsuario],
        A.[EnviadoAMonicaPorNombre],
        A.[ObservacionesEnvioMonica],
        A.[DestinatarioJuridico],
        A.[Activo],
        A.[NombreUsuarioCreacion],
        A.[NombreUsuarioUltimaActualizacion],
        A.[FechaCreacion],
        A.[FechaActualizacion],
        CASE
            WHEN NULLIF(LTRIM(RTRIM(A.[SemaforoManual])), N'') IS NOT NULL THEN A.[SemaforoManual]
            WHEN A.[Estatus] = N'Atendido' THEN N'Verde'
            WHEN A.[FechaCompromiso] IS NOT NULL AND A.[FechaCompromiso] < CONVERT(DATE, SYSUTCDATETIME()) THEN N'Rojo'
            WHEN A.[FechaCompromiso] IS NOT NULL AND A.[FechaCompromiso] <= DATEADD(DAY, 3, CONVERT(DATE, SYSUTCDATETIME())) THEN N'Amarillo'
            ELSE N'Pendiente'
        END AS [Semaforo],
        CASE
            WHEN A.[RequiereEnvioMonica] = 1 AND A.[EnviadoAMonica] = 0 THEN N'Pendiente de envío a Mónica'
            WHEN A.[RequiereEnvioMonica] = 1 AND A.[EnviadoAMonica] = 1 THEN N'Enviado a Mónica'
            ELSE N'No aplica'
        END AS [AlertaEnvioMonica],
        (
            SELECT COUNT(1)
            FROM [dgmesnie].[AsuntoComentario] C
            WHERE C.[AsuntoId] = A.[AsuntoId]
        ) AS [TotalComentarios]
    FROM [dgmesnie].[AsuntoSeguimiento] A
    WHERE A.[Activo] = 1;
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_AsuntoSeguimiento_ListarDashboard]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [dgmesnie].[vw_AsuntoSeguimientoDashboard]
    ORDER BY
        CASE [Semaforo]
            WHEN N'Rojo' THEN 1
            WHEN N'Amarillo' THEN 2
            WHEN N'Pendiente' THEN 3
            WHEN N'Verde' THEN 4
            ELSE 5
        END,
        [FechaCompromiso],
        [AsuntoId];
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_AsuntoSeguimiento_Crear]
    @NumeroRegistro INT = NULL,
    @TituloAsunto NVARCHAR(300),
    @Descripcion NVARCHAR(MAX) = NULL,
    @Expediente NVARCHAR(100) = NULL,
    @FechaSolicitud DATE = NULL,
    @DiasTranscurridos INT = NULL,
    @Responsable NVARCHAR(200) = NULL,
    @Encargado NVARCHAR(200) = NULL,
    @Minuta NVARCHAR(200) = NULL,
    @FichaInformativaOficio NVARCHAR(200) = NULL,
    @AreaResponsable NVARCHAR(200) = NULL,
    @Estatus NVARCHAR(50) = N'Pendiente',
    @EstadoActual NVARCHAR(MAX) = NULL,
    @Prioridad NVARCHAR(50) = NULL,
    @TipoAsunto NVARCHAR(100) = NULL,
    @SemaforoManual NVARCHAR(20) = NULL,
    @FechaReunion DATE = NULL,
    @FechaCompromiso DATE = NULL,
    @FechaAtencion DATE = NULL,
    @CarpetaSharePointUrl NVARCHAR(1000) = NULL,
    @UbicacionCarpeta NVARCHAR(300) = NULL,
    @DatosContacto NVARCHAR(MAX) = NULL,
    @FechaTextoReunion NVARCHAR(300) = NULL,
    @RequiereEnvioMonica BIT = 1,
    @IdUsuarioCreacion INT = NULL,
    @NombreUsuarioCreacion NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dgmesnie].[AsuntoSeguimiento]
    (
        [NumeroRegistro],
        [TituloAsunto],
        [Descripcion],
        [Expediente],
        [FechaSolicitud],
        [DiasTranscurridos],
        [Responsable],
        [Encargado],
        [Minuta],
        [FichaInformativaOficio],
        [AreaResponsable],
        [Estatus],
        [EstadoActual],
        [Prioridad],
        [TipoAsunto],
        [SemaforoManual],
        [FechaReunion],
        [FechaCompromiso],
        [FechaAtencion],
        [CarpetaSharePointUrl],
        [UbicacionCarpeta],
        [DatosContacto],
        [FechaTextoReunion],
        [RequiereEnvioMonica],
        [IdUsuarioCreacion],
        [NombreUsuarioCreacion],
        [IdUsuarioUltimaActualizacion],
        [NombreUsuarioUltimaActualizacion]
    )
    VALUES
    (
        @NumeroRegistro,
        @TituloAsunto,
        NULLIF(LTRIM(RTRIM(@Descripcion)), N''),
        NULLIF(LTRIM(RTRIM(@Expediente)), N''),
        @FechaSolicitud,
        @DiasTranscurridos,
        NULLIF(LTRIM(RTRIM(@Responsable)), N''),
        NULLIF(LTRIM(RTRIM(@Encargado)), N''),
        NULLIF(LTRIM(RTRIM(@Minuta)), N''),
        NULLIF(LTRIM(RTRIM(@FichaInformativaOficio)), N''),
        NULLIF(LTRIM(RTRIM(@AreaResponsable)), N''),
        COALESCE(NULLIF(LTRIM(RTRIM(@Estatus)), N''), N'Pendiente'),
        NULLIF(LTRIM(RTRIM(@EstadoActual)), N''),
        NULLIF(LTRIM(RTRIM(@Prioridad)), N''),
        NULLIF(LTRIM(RTRIM(@TipoAsunto)), N''),
        NULLIF(LTRIM(RTRIM(@SemaforoManual)), N''),
        @FechaReunion,
        @FechaCompromiso,
        @FechaAtencion,
        NULLIF(LTRIM(RTRIM(@CarpetaSharePointUrl)), N''),
        NULLIF(LTRIM(RTRIM(@UbicacionCarpeta)), N''),
        NULLIF(LTRIM(RTRIM(@DatosContacto)), N''),
        NULLIF(LTRIM(RTRIM(@FechaTextoReunion)), N''),
        @RequiereEnvioMonica,
        @IdUsuarioCreacion,
        NULLIF(LTRIM(RTRIM(@NombreUsuarioCreacion)), N''),
        @IdUsuarioCreacion,
        NULLIF(LTRIM(RTRIM(@NombreUsuarioCreacion)), N'')
    );

    DECLARE @AsuntoId INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [IdUsuario],
        [NombreUsuario]
    )
    VALUES
    (
        @AsuntoId,
        N'Creación',
        N'Se creó el asunto de seguimiento.',
        @IdUsuarioCreacion,
        NULLIF(LTRIM(RTRIM(@NombreUsuarioCreacion)), N'')
    );

    SELECT @AsuntoId AS [AsuntoId];
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_AsuntoComentario_Agregar]
    @AsuntoId INT,
    @IdUsuario INT = NULL,
    @NombreUsuario NVARCHAR(200),
    @Mensaje NVARCHAR(MAX),
    @TipoMensaje NVARCHAR(50) = N'Comentario'
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dgmesnie].[AsuntoComentario]
    (
        [AsuntoId],
        [IdUsuario],
        [NombreUsuario],
        [Mensaje],
        [TipoMensaje]
    )
    VALUES
    (
        @AsuntoId,
        @IdUsuario,
        @NombreUsuario,
        @Mensaje,
        COALESCE(NULLIF(LTRIM(RTRIM(@TipoMensaje)), N''), N'Comentario')
    );

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [IdUsuario],
        [NombreUsuario]
    )
    VALUES
    (
        @AsuntoId,
        N'Comentario',
        N'Se agregó un comentario al histórico del asunto.',
        @IdUsuario,
        @NombreUsuario
    );
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_AsuntoSeguimiento_MarcarEnvioMonica]
    @AsuntoId INT,
    @IdUsuario INT = NULL,
    @NombreUsuario NVARCHAR(200),
    @ObservacionesEnvioMonica NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[AsuntoSeguimiento]
    SET
        [EnviadoAMonica] = 1,
        [FechaEnvioMonica] = SYSUTCDATETIME(),
        [EnviadoAMonicaPorIdUsuario] = @IdUsuario,
        [EnviadoAMonicaPorNombre] = @NombreUsuario,
        [ObservacionesEnvioMonica] = NULLIF(LTRIM(RTRIM(@ObservacionesEnvioMonica)), N''),
        [IdUsuarioUltimaActualizacion] = @IdUsuario,
        [NombreUsuarioUltimaActualizacion] = @NombreUsuario,
        [FechaActualizacion] = SYSUTCDATETIME()
    WHERE [AsuntoId] = @AsuntoId;

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [ValorAnterior],
        [ValorNuevo],
        [IdUsuario],
        [NombreUsuario]
    )
    VALUES
    (
        @AsuntoId,
        N'Envío a Mónica',
        N'Se marcó el asunto como enviado a Mónica de Asuntos Jurídicos.',
        N'Pendiente',
        N'Enviado',
        @IdUsuario,
        @NombreUsuario
    );
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_AsuntoSeguimiento_ActualizarEstatus]
    @AsuntoId INT,
    @Estatus NVARCHAR(50),
    @FechaCompromiso DATE = NULL,
    @FechaAtencion DATE = NULL,
    @IdUsuario INT = NULL,
    @NombreUsuario NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EstatusAnterior NVARCHAR(50);

    SELECT @EstatusAnterior = [Estatus]
    FROM [dgmesnie].[AsuntoSeguimiento]
    WHERE [AsuntoId] = @AsuntoId;

    UPDATE [dgmesnie].[AsuntoSeguimiento]
    SET
        [Estatus] = COALESCE(NULLIF(LTRIM(RTRIM(@Estatus)), N''), [Estatus]),
        [FechaCompromiso] = COALESCE(@FechaCompromiso, [FechaCompromiso]),
        [FechaAtencion] = CASE WHEN NULLIF(LTRIM(RTRIM(@Estatus)), N'') = N'Atendido' THEN COALESCE(@FechaAtencion, CONVERT(DATE, SYSUTCDATETIME())) ELSE @FechaAtencion END,
        [IdUsuarioUltimaActualizacion] = @IdUsuario,
        [NombreUsuarioUltimaActualizacion] = @NombreUsuario,
        [FechaActualizacion] = SYSUTCDATETIME()
    WHERE [AsuntoId] = @AsuntoId;

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [ValorAnterior],
        [ValorNuevo],
        [IdUsuario],
        [NombreUsuario]
    )
    VALUES
    (
        @AsuntoId,
        N'Cambio de estatus',
        N'Se actualizó el estatus operativo del asunto.',
        @EstatusAnterior,
        @Estatus,
        @IdUsuario,
        @NombreUsuario
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dgmesnie].[AsuntoSeguimiento])
BEGIN
    DECLARE @AsuntosSemilla TABLE
    (
        [SeedId] INT NOT NULL,
        [GuidAsunto] UNIQUEIDENTIFIER NOT NULL,
        [TituloAsunto] NVARCHAR(300) NOT NULL,
        [FechaSolicitud] NVARCHAR(20) NULL,
        [FechaReunion] NVARCHAR(20) NULL,
        [Responsable] NVARCHAR(200) NULL,
        [Minuta] NVARCHAR(200) NULL,
        [FichaInformativa] NVARCHAR(200) NULL,
        [EstadoActual] NVARCHAR(300) NULL,
        [FechaConclusion] NVARCHAR(20) NULL,
        [Tipo] NVARCHAR(100) NULL,
        [DescripcionGeneral] NVARCHAR(MAX) NULL,
        [Comentarios] NVARCHAR(MAX) NULL,
        [UbicacionCarpeta] NVARCHAR(300) NULL,
        [DatosContacto] NVARCHAR(MAX) NULL,
        [FechaTextoReunion] NVARCHAR(300) NULL,
        [RequiereEnvioMonica] BIT NOT NULL,
        [EnviadoAMonica] BIT NOT NULL,
        [EnviadoAMonicaPorNombre] NVARCHAR(200) NULL,
        [ObservacionesEnvioMonica] NVARCHAR(1000) NULL
    );

    DECLARE @AsuntosInsertados TABLE
    (
        [SeedId] INT NOT NULL,
        [AsuntoId] INT NOT NULL
    );

    INSERT INTO @AsuntosSemilla
    (
        [SeedId],
        [GuidAsunto],
        [TituloAsunto],
        [FechaSolicitud],
        [FechaReunion],
        [Responsable],
        [Minuta],
        [FichaInformativa],
        [EstadoActual],
        [FechaConclusion],
        [Tipo],
        [DescripcionGeneral],
        [Comentarios],
        [UbicacionCarpeta],
        [DatosContacto],
        [FechaTextoReunion],
        [RequiereEnvioMonica],
        [EnviadoAMonica],
        [EnviadoAMonicaPorNombre],
        [ObservacionesEnvioMonica]
    )
    VALUES
        (1, NEWID(), N'Aljaval', N'16/06/2025', NULL, N'Barri', N'Barri', NULL, N'Finalizado', N'10/09/2025', N'Cartera', N'Presentación de cartera de proyectos.', N'Reunión-Minuta.', N'20250616_ALJAVAL', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (2, NEWID(), N'Eólica Tres Palmitas', N'03/06/2025', NULL, N'Barri', NULL, N'Barri', N'Finalizado', N'26/11/2025', N'Cartera', N'Proyecto eólico de 90 MW en Alvarado, Veracruz.', N'Tarjeta informativa.', N'20250707_TRES PALMITAS_OK', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (3, NEWID(), N'EDP', N'31/10/2025', NULL, N'Barri', N'Barri', NULL, N'Finalizado', N'07/11/2025', N'Cartera', N'Presentación de cartera de proyectos.', N'Reunión-Minuta.', N'20253110_EDP', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (4, NEWID(), N'Fisterra', N'24/11/2025', N'26/11/2025', N'Barri', NULL, N'Barri', N'Finalizado', N'03/12/2025', N'Cartera', N'Presentación de cartera de proyectos.', N'Tarjeta informativa.', N'20251124_Fisterra', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (5, NEWID(), N'Agrorenovables', N'31/10/2025', N'28/11/2025', N'Ara', N'Ara', NULL, N'Finalizado', N'24/12/2025', N'Cartera', N'Presentación de cartera de proyectos.', N'Reunión-Minuta.', N'20251031_Agrorenovables', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (6, NEWID(), N'Kenerwatts S.A.P.I. de C.V.', N'08/12/2025', NULL, N'Barri', NULL, NULL, N'Finalizado', N'15/12/2025', N'Convocatoria', N'Solicitó información de otros participantes que tienen el mismo punto de interconexión.', N'Correo electrónico.', N'20251208_ Kenerwatts', N'Raul Santos Coy | RAUL.SANTOSCOY@BAYWA-RE.COM', NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (7, NEWID(), N'Oficio UEL.114.292.2025 / Diputada Alma Marina Vitela Rodríguez / Polos Bienestar', N'25/11/2025', NULL, N'Edgar', NULL, N'Edgar', N'Finalizado', N'24/12/2025', N'Consulta Polos', N'Solicita audiencia para presentar iniciativas y propuestas.', N'Correo electrónico.', N'20251125_Diputada Alma Vitela_Polo Laguna', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (8, NEWID(), N'GEMEX, Energía Renovable', N'15/10/2025', N'09/01/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'14/01/2026', N'Cartera', N'Se envía presentación con información relevante sobre algunos proyectos solares destacados: Los Nogales, Guanajuato; Tebal, Yucatán; Piedras Negras, Veracruz; Las Cañas, Veracruz; Salamanca, Guanajuato; San Cayetano, Jalisco; Laguna OM, Quintana Roo; Arco y Donhi, Hidalgo; y El Higo, Veracruz.', N'Minuta y correo electrónico.', N'20251015_GEMEX', N'Deyanira Terán | Tel. 55 5965 1533 | deyanira.teran@gemex.mx', N'Virtual 9 de enero de 2026 a las 10:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (9, NEWID(), N'Paradise Sands S. de R.L. de C.V. y Terra Innovatum', N'30/10/2025', N'04/05/2026', N'Ara', N'Ara', NULL, N'Se convoca a reunión virtual 04/05/26', NULL, N'Consulta Tecnología', N'Solicita la evaluación del proyecto piloto para la instalación y operación de un reactor nuclear micro modular; envía antecedentes sobre oficio enviado a la Presidencia y la respuesta de la CFE; se envía presentación.', N'Al enviar el correo de invitación a reunión respondieron que ingresaron otra solicitud con fecha del 30 de marzo a nombre de RYZON ENERGY para presentar una nueva tecnología de energía renovable y verde, libre de huella de carbono, cero emisiones atmosféricas y sin uso de combustible.', N'20251030_Paradise', N'Ing. Juan Hernández López | juanjahweh2@gmail.com', N'04/05/2026', 1, 0, NULL, NULL),
        (10, NEWID(), N'NIKO ENERGY', N'11/11/2025', N'08/01/2026', N'Dany', N'Dany', NULL, N'Finalizado', N'09/01/2026', N'Consulta Tecnología', N'Empresa que vende sistemas eléctricos. Solicita audiencia para presentar iniciativas y propuesta de colaboración técnica.', N'Minuta y correo electrónico.', N'20251111_Niko', N'Raffaele Sertorio | Tel. 5523344526 | raffaele@niko.mx', N'Virtual 8 de enero de 2026 a las 10:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (11, NEWID(), N'DIVERXIA ENERGIAS 2, S. DE R.L. DE C.V.', N'27/11/2025', NULL, NULL, NULL, NULL, N'Propuesta de reunión pero el correo del contacto es incorrecto', NULL, N'Cartera', N'Solicitan contemplar la integración en los polos de desarrollo de los proyectos fotovoltaicos LERMA 220 y TOLLOCAN 202 en el Estado de México y Quintana Roo, respectivamente.', N'Correo enviado el 19 de enero (jalcantar@gmail.com). Correo incorrecto con intento de envío de invitación. Se propone dar por cumplida esta solicitud de acuerdo con el numeral 5 de los Lineamientos Generales Relativos a los Procedimientos de Atención de Solicitudes de Acceso a la Información.', N'20251127_Diverxia', N'Jaime Alcantar Ramírez | jalcantar@gmail.com', NULL, 1, 0, NULL, NULL),
        (12, NEWID(), N'COX ENERGY', N'28/11/2025', NULL, NULL, NULL, NULL, N'Propuesta de reunión faltan datos de contacto', NULL, N'Cartera', N'Central eléctrica de generación limpia para el fortalecimiento de la infraestructura eléctrica nacional.', N'No se tuvo respuesta a la invitación de reunión del 16 de enero. Se propone dar por cumplida esta solicitud de acuerdo con el numeral 5 de los Lineamientos Generales Relativos a los Procedimientos de Atención de Solicitudes de Acceso a la Información.', N'20251128_COX', N'Montes Urales 415, Lomas de Chapultepec II Sección, Alcaldía Miguel Hidalgo, C.P. 11000, CDMX | info@coxenergy.com | 52 55 73 16 3174', NULL, 1, 0, NULL, NULL),
        (13, NEWID(), N'PARQUE SOLAR KUKUUL S. DE R.L. DE C.V.', N'28/11/2025', N'12/01/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'14/01/2026', N'Cartera', N'Convocatoria prioritaria de planeación vinculante y permisos de generación de energía eléctrica (proyecto fotovoltaico PSK) en los municipios de Ticul y Sacalum, Yucatán; el proyecto ofrece 71 MW.', N'Minuta y correo electrónico.', N'20251128_PARQUE SOLAR KUKUUL', N'Mario Pani Cusi, representante legal | Mario.Pani@baywa-re.com | Ángel Urraza 314, Col. Del Valle Centro, Alcaldía Benito Juárez, C.P. 03100, CDMX', N'Virtual 12 de enero de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (14, NEWID(), N'Kenergenux', N'03/12/2025', N'13/01/2026', N'Ara', N'Ara', NULL, N'Finalizado', N'15/01/2026', N'Cartera', N'Presentar formalmente a Kenergenux y exponer sus actividades; ponen a disposición información sobre proyectos y presentan dos bloques agrupados de proyectos de generación fotovoltaica con almacenamiento en Estado de México, Zacatecas y otros sitios.', N'Minuta y correo electrónico.', N'20251203_Kenergenux', N'Mariana Chorné | Campos Elíseos 223, piso 5, oficina 501, Polanco IV Sección, CDMX | 5619886236 / 5518494770 | mariana.chorne@genuxpower.com', N'Virtual 13 de enero de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (15, NEWID(), N'Cubico', N'11/12/2025', N'19/01/2026', N'Unidad de Estrategia, Vinculación Interinstitucional y Seguimiento de Proyectos e Inversiones', N'Claudia', NULL, N'Finalizado', N'30/01/2026', N'Convocatoria', N'Solicita el aumento de su capacidad instalada a 110 MW (antes 78 MW) fotovoltaico.', N'Minuta y correo electrónico.', N'20251211_cubico', N'Osvaldo Rance Cachafeiro | Av. Vasco de Quiroga 3900, Torre A, Piso 20, Col. Lomas de Santa Fe, 05348, CDMX | osvaldo.rance@cubicoinvest.com', N'Presencial 19 de enero de 2026 a las 11:00 hrs, Piso 8 Sala SUM', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (16, NEWID(), N'IMPULSORA JALISCIENCE, S.A. DE C.V.', N'12/12/2025', N'16/01/2026', N'Ara', N'Ara', NULL, N'Finalizado', N'26/01/2026', N'Cartera', N'Presenta la intención de construir un proyecto de generación de energía eléctrica fotovoltaica con capacidad instalada de hasta 3.5 MW en Lagos de Moreno, Jalisco (predio Monte Grande).', N'Minuta y correo electrónico.', N'20251212_Impulsora Jaliscience', N'ignaciolopez@enix.com.mx | gbustamante@enix.com.mx | h.alvarez@akron.com.mx | de.garcia@akron.com.mx | s.jaramillon@akron.com.mx', N'Virtual 16 de enero de 2026 a las 10:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (17, NEWID(), N'Comisión Nacional de Energía', N'15/12/2025', NULL, N'Barri', NULL, N'Barri', N'Finalizado', NULL, N'Consulta PV', N'Consulta respecto a la aplicación de los criterios de planeación vinculante.', N'Respuesta a oficio.', N'20251216_Consulta UE CNE_PV hidro CFE', NULL, NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (18, NEWID(), N'LOGISTICA VISTA VERDE S. DE R.L. DE C.V.', N'17/12/2025', NULL, N'Barri', NULL, N'Barri', N'Reunión con el Dr.', N'08/01/2026', N'Proyectos estratégicos', N'Solicita la canalización del proyecto Gasoducto Vista Verde al Consejo de Planeación Energética, solicitando su reconocimiento como infraestructura estratégica y su consideración en los instrumentos de la Planeación Vinculante.', N'Presentación Baja California.', N'20251218_LOGISTICA VISTA VERDE', N'Kirk Sherr | Av. Reynosa 65-A, Zona Centro, C.P. 22800, Ensenada, Baja California | kirk.sherr@tailgrass.com', NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como atendido.'),
        (19, NEWID(), N'BHCE Yucatán 1, S.A.P.I. de C.V.', N'18/12/2025', N'15/01/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'15/01/2026', N'Cartera', N'Manifiesta interés en participar en la planeación vinculante con un proyecto de generación eólica con capacidad instalada de 250 MW en Cansahcab, Yucatán.', N'Minuta y correo electrónico.', N'20251218_BHCE Yucatán 1', N'Israel Sades Mizrahi | israel.sades@energiacb.com.mx | 5530380097 | cynthia.bouchot@energiacb.com.mx | lucero.ortiz@energiacb.com.mx | daniel.segura@energiacb.com.mx', N'Virtual 15 de enero de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (20, NEWID(), N'ENGIE MÉXICO, S.A. DE C.V.', N'19/12/2025', N'19/01/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'26/01/2026', N'Cartera', N'Interés de desarrollar tres proyectos de distintas fuentes de energía limpia: Ranchitos eólico 120 MW, Mina Solar fotovoltaico 149 MW y Chignautla fotovoltaico 80 MW.', N'Minuta y correo electrónico.', N'20251219_ENGIE', N'Eduardo René Narváez Torres | rene.narvaez@engie.com | Alberto Lezama Cortés | alberto.lezama@engie.com | Rosa Alejandrina García Macías | rosa.garciam@engie.com', N'Virtual 19 de enero de 2026 a las 10:30 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (21, NEWID(), N'DESARROLLO ENERGÉTICO SUSTENTABLE ESTADO DE JALISCO', N'19/12/2025', N'13/01/2026', N'Dany', N'Dany', NULL, N'Finalizado', N'26/01/2026', N'Cartera', N'Se entrega para consideración las conclusiones del estudio técnico elaborado por la Secretaría de Desarrollo Energético Sustentable de Jalisco sobre demanda incremental y proyectos de generación alineados a la planeación vinculante.', N'Reunión con el Dr. Islas y minuta.', N'20251219_DESARROLLO ENERGÉTICO', N'Manuel Jesús Herrera Vega | Av. Faro #2350, Col. Verde Valle, Guadalajara, Jalisco, C.P. 44550 | 3330302000 ext. 52422 y 52428', N'Presencial con el Dr. Jorge Islas', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (22, NEWID(), N'GOBERNADOR CONSTITUCIONAL DEL ESTADO DE JALISCO', N'22/12/2025', N'13/01/2026', N'Dany', N'Dany', NULL, N'Finalizado', N'26/01/2026', N'Cartera', N'Se entregan proyectos de generación alineados a la planeación vinculante, relacionados con el estudio presentado por la Secretaría de Desarrollo Energético Sustentable de Jalisco.', N'Reunión con el Dr. Islas y minuta.', N'20251224_GOBIERNO DE JALISCO', NULL, N'Presencial con el Dr. Jorge Islas', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (23, NEWID(), N'BayWa r.e. Desarrollos Solares', N'29/12/2025', N'20/01/2026', N'Ara', N'Ara', NULL, N'Finalizado', N'30/01/2026', N'Cartera', N'Manifiesta interés en participar en la convocatoria de enero 2026 con el proyecto CFV Carbón II y que se incluyan en la planeación vinculante diversos proyectos solares en Yucatán, Hidalgo, Veracruz, Nuevo León y Jalisco.', N'Minuta y correo electrónico.', N'20251229_BayWa', N'Raul Santos Coy | RAUL.SANTOSCOY@BAYWA-RE.COM', N'Virtual 20 de enero de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (24, NEWID(), N'Versalles de las Cuatas Portafolio Durango', N'08/01/2026', N'21/01/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'30/01/2026', N'Cartera', N'Evaluación del portafolio Durango, compuesto por 6 centrales fotovoltaicas.', N'Minuta y correo electrónico.', N'20260108_Durango', N'Luis Arias Osoyo | luis.arias@aindaei.com | Montes Urales 770, 1er. piso, Col. Lomas de Chapultepec, Miguel Hidalgo, C.P. 11000', N'Virtual 21 de enero de 2026 a las 10:30 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (25, NEWID(), N'Cubico', N'27/01/2026', N'30/01/2026', N'Barri', N'Barri', NULL, N'Finalizado', N'30/01/2026', N'Cartera', N'Solicita audiencia del 03 al 06 de febrero de 2026 con el fin de dar continuidad a la estructuración de potenciales proyectos de inversión mixta.', N'Se desahoga con la minuta de la reunión del 19 de enero.', N'20251211_cubico', N'Osvaldo Rance Cachafeiro | Av. Vasco de Quiroga 3900, Torre A, Piso 20, Col. Lomas de Santa Fe, 05348, CDMX | osvaldo.rance@cubicoinvest.com', NULL, 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (26, NEWID(), N'Internovum / Bluemex', N'20/01/2026', N'30/01/2026', N'Dany', N'Dany', NULL, N'Finalizado', N'17/02/2026', N'Cartera', N'Solicitud de reunión respecto al proyecto FV Santa Fe.', N'Minuta y correo electrónico.', N'0260121_Internovum', N'Alejandro Hinojos | ah@internovum-solar.com', N'Virtual 30 de enero de 2026 a las 15:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (27, NEWID(), N'Alion', N'29/01/2026', N'02/03/2026', N'Marian', N'Marian', NULL, N'Finalizado', N'03/03/2026', N'Cartera', N'Solicita una reunión para recibir orientación técnica y regulatoria; buscan dar a conocer un proyecto eólico o fotovoltaico en predios de carácter familiar ubicados en Coahuila.', N'Minuta y correo electrónico.', N'20260129_ALION', N'Gustavo García Romero | gustavo.garcia@alionbs.com | Manuel Pérez Treviño #390, Saltillo, Coahuila', N'Virtual 2 de marzo de 2026 a las 10:00 hrs', 1, 1, N'Carga histórica', N'Registro histórico marcado como concluido.'),
        (28, NEWID(), N'GTE Energy', N'13/02/2026', N'02/03/2026', NULL, NULL, NULL, N'Rechazaron reunión y pidieron reagendar en otra fecha ya que estaban a la espera de una reunión', NULL, N'Cartera', N'Solicitud de reunión para dar a conocer algunos proyectos con inversión extranjera y privada en generación eléctrica fotovoltaica.', NULL, N'20260213_ GTE Energy Mexico', N'Mario S. Urbino | Director General de GTE Energy México | +1 858 692 9835 | urbino9@gmail.com', N'Virtual', 1, 0, NULL, NULL),
        (29, NEWID(), N'LOGISTICA VISTA VERDE S. DE R.L. DE C.V.', N'24/02/2026', N'24/04/2026', NULL, NULL, NULL, N'Se elaboró una nota informativa y minuta de la reunión celebrada el 24 de febrero de 2026.', NULL, N'Proyectos estratégicos', N'Presentan un anexo técnico de la condición estructural del sistema de gas natural en Baja California y solicitan que se incorpore como insumo para la planeación energética vinculante.', N'En espera de instrucciones. Se plantea como alternativa configurar un corredor alterno independiente con acceso a cuencas productoras y capacidad para servicios firmes. Se sugiere solicitar opinión de la Subsecretaría de Hidrocarburos y de la Comisión Nacional de Control de Gas Natural antes de remitir al Consejo de Planeación.', N'20260224_Logistica Vista Verde', N'Kirk Sherr | Av. Reynosa 65-A, Zona Centro, C.P. 22800, Ensenada, Baja California | kirk.sherr@tailgrass.com', N'Virtual', 1, 0, NULL, NULL),
        (30, NEWID(), N'ZUMA ENERGY', N'02/03/2026', NULL, NULL, NULL, NULL, N'Se envía correo a Mónica con calendario de fechas', NULL, N'Esquemas Mixtos', N'Solicitan 8 semanas para incorporar la información solicitada por la VUPE.', N'Se tiene conocimiento que ingresaron proyectos en la VUPE, por lo que se sugiere atender con un correo a Mónica señalando dicho acto. El 27 de marzo ingresaron un escrito alcance.', N'0260227_ZUMA ENERGIA', N'Armando Peñuelas | apeñuelas@zumaenergia.com | 5541945070', NULL, 1, 1, N'Carga histórica', N'Se registró envío histórico a Mónica con calendario de fechas.'),
        (31, NEWID(), N'EL GRITON SOLAR, S.A. DE C.V.', N'04/03/2026', N'28/04/2026', NULL, NULL, NULL, N'Reunión celebrada, se envía correo a Mónica con minuta de reunión', N'29/04/2026', N'Cartera', N'Solicitan considerar e integrar el proyecto La Granja Solar (365.995 MW) en el PLATEASE para evaluación como infraestructura estratégica en la región occidente (Zacatecas).', N'Se propone reunión, solicitar más detalles del proyecto y con la respuesta turnar a CENACE para el análisis correspondiente y con dicho correo reenviar a Mónica para su desahogo.', N'04032026_GRITON SOLAR', N'Víctor Durango Domínguez | Av. Marina Nacional #60, Piso 6, Col. Tacuba, Alcaldía Miguel Hidalgo, C.P. 11410 | vdurango@naturgy.com', N'Virtual 28 de abril de 2026 a las 10:30 hrs', 1, 1, N'Carga histórica', N'Se envió a Mónica la minuta de reunión durante el seguimiento histórico.'),
        (32, NEWID(), N'UNIÓN FENOSA MÉXICO, S.A. DE C.V.', N'04/03/2026', N'28/04/2026', NULL, NULL, NULL, N'Reunión celebrada, se envía correo a Mónica con minuta de reunión', N'29/04/2026', N'Cartera', N'Solicitan considerar e integrar el proyecto Parque Solar Quetzal (160 MW) en el PLATEASE para evaluación como infraestructura estratégica en la región noroccidente (Durango).', N'Se propone reunión, solicitar más detalles del proyecto y con la respuesta turnar a CENACE para el análisis correspondiente y con dicho correo reenviar a Mónica para su desahogo.', N'04032026_UNION FENOSA MEXICO', N'Víctor Durango Domínguez | Av. Marina Nacional #60, Piso 6, Col. Tacuba, Alcaldía Miguel Hidalgo, C.P. 11410 | vdurango@naturgy.com', N'Virtual 28 de abril de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Se envió a Mónica la minuta de reunión durante el seguimiento histórico.'),
        (33, NEWID(), N'GRUPO ESENTIA', N'12/03/2026', NULL, NULL, NULL, NULL, N'Se realiza nota informativa de la solicitud', NULL, N'Cartera', N'Solicitan la confirmación de criterio respecto a que su proyecto esté en posibilidad de ser considerado ante SENER como proyecto estratégico con el carácter de Planeación Vinculante del sector energético nacional.', N'El proyecto consiste en la expansión para incrementar la capacidad de transporte de gas natural por medio de ductos y la optimización de infraestructura desde la región norte hacia centro y occidente del país, mediante reforzamiento de sus sistemas de transporte.', N'20260312_ESENTIA', N'Mildred Fernanda Benítez Moreno | Boulevard Adolfo Ruiz Cortines #3433, Col. San Jerónimo Lídice, Magdalena Contreras, C.P. 10200 | 55 76681887 | regulatorio-mfbm@esentia-energy.com', NULL, 1, 0, NULL, NULL),
        (34, NEWID(), N'CIP', N'13/03/2026', N'31/03/2026', NULL, NULL, NULL, N'Se envía por correo a Mónica minuta de reunión', NULL, N'Consulta', N'Consulta respecto a la acreditación de potencia como unidad de central eléctrica firme del SAE asociado a las centrales eléctricas La Esperanza Solar y Alegría Solar.', NULL, N'20260313_CIP', N'Horacio María de Uriarte Flores | hdeuriarte@macf.com.mx', N'Presencial 31 de marzo de 2026 a las 11:00 hrs', 1, 1, N'Carga histórica', N'Se registró envío histórico a Mónica de la minuta de reunión.'),
        (35, NEWID(), N'INEEL/FONADIN', N'18/03/2026', NULL, NULL, NULL, NULL, N'En revisión para atención de la Dirección General de Desarrollo Tecnológico y Acceso a la Energía', NULL, N'Programa de instalación de paneles solares', N'Programa para la instalación de paneles solares en entidades del sector público.', N'Solicita apoyo para manifestar que el programa es consistente con los programas y metas sectoriales y que se cumple con las disposiciones aplicables.', N'20260318_INEEL-FONADIN PANELES SOLARES', N'Víctor Alejandro Salcido', NULL, 1, 0, NULL, NULL),
        (36, NEWID(), N'Subsecretaría de Electricidad / CENACE', N'27/03/2026', NULL, NULL, NULL, NULL, NULL, NULL, N'De conocimiento', N'Comentarios propuesta PAM RNT RGD 2026-2040', NULL, NULL, NULL, NULL, 1, 0, NULL, NULL),
        (37, NEWID(), N'Energía Solar Herrera', N'06/05/2026', NULL, NULL, NULL, NULL, NULL, NULL, N'De conocimiento', N'Solicita el reconocimiento del proyecto "Energía Solar Herrera" como parte de un grupo de interés económico de capital mexicano ENNOVA AMÉRICA, S.A.P.I. DE C.V.', N'El proyecto "Energía Solar Herrera" cuenta con el respaldo principal de ENNOVA AMÉRICA, S.A.P.I. DE C.V. Empresa mexicana dirigida por Lic. Eduardo Vaudrecourt Salcido, en su carácter de socio mayoritario, asumió el liderazgo institucional, financiero y estratégico del proyecto, así como la responsabilidad sobre su estructuración, continuidad y futura operación.', N'2026_Energía Solar Herrera', N'María Dolores Temoltzin Bustillos | Representante Legal | Energía Solar Herrera | dolores@libienergy.com.mx', NULL, 1, 0, NULL, NULL),
        (38, NEWID(), N'GTE Energy', N'07/05/2026', NULL, NULL, NULL, NULL, NULL, NULL, N'Cartera', NULL, NULL, NULL, N'Mario S. Urbino | Director General de GTE Energy México | +1 858 692 9835 | urbino9@gmail.com', NULL, 1, 0, NULL, NULL);

    INSERT INTO [dgmesnie].[AsuntoSeguimiento]
    (
        [GuidAsunto],
        [NumeroRegistro],
        [TituloAsunto],
        [Descripcion],
        [Expediente],
        [FechaSolicitud],
        [DiasTranscurridos],
        [Responsable],
        [Encargado],
        [Minuta],
        [FichaInformativaOficio],
        [AreaResponsable],
        [Estatus],
        [EstadoActual],
        [Prioridad],
        [TipoAsunto],
        [SemaforoManual],
        [FechaReunion],
        [FechaCompromiso],
        [FechaAtencion],
        [CarpetaSharePointUrl],
        [UbicacionCarpeta],
        [DatosContacto],
        [FechaTextoReunion],
        [RequiereEnvioMonica],
        [EnviadoAMonica],
        [FechaEnvioMonica],
        [EnviadoAMonicaPorNombre],
        [ObservacionesEnvioMonica],
        [NombreUsuarioCreacion],
        [NombreUsuarioUltimaActualizacion]
    )
    SELECT
        S.[GuidAsunto],
        S.[SeedId],
        S.[TituloAsunto],
        NULLIF(LTRIM(RTRIM(S.[DescripcionGeneral])), N''),
        NULLIF(LTRIM(RTRIM(S.[UbicacionCarpeta])), N''),
        TRY_CONVERT(DATE, S.[FechaSolicitud], 103),
        CASE
            WHEN TRY_CONVERT(DATE, S.[FechaSolicitud], 103) IS NOT NULL
                THEN DATEDIFF(DAY, TRY_CONVERT(DATE, S.[FechaSolicitud], 103), CONVERT(DATE, SYSUTCDATETIME()))
            ELSE NULL
        END,
        NULLIF(LTRIM(RTRIM(S.[Responsable])), N''),
        NULLIF(LTRIM(RTRIM(S.[Responsable])), N''),
        NULLIF(LTRIM(RTRIM(S.[Minuta])), N''),
        NULLIF(LTRIM(RTRIM(S.[FichaInformativa])), N''),
        N'DGMESNIE',
        CASE
            WHEN TRY_CONVERT(DATE, S.[FechaConclusion], 103) IS NOT NULL THEN N'Atendido'
            WHEN S.[EstadoActual] LIKE N'%Finalizado%' THEN N'Atendido'
            ELSE N'Pendiente'
        END,
        NULLIF(LTRIM(RTRIM(S.[EstadoActual])), N''),
        NULL,
        NULLIF(LTRIM(RTRIM(S.[Tipo])), N''),
        CASE
            WHEN TRY_CONVERT(DATE, S.[FechaConclusion], 103) IS NOT NULL OR S.[EstadoActual] LIKE N'%Finalizado%' THEN N'Verde'
            ELSE N'Amarillo'
        END,
        TRY_CONVERT(DATE, S.[FechaReunion], 103),
        COALESCE(
            TRY_CONVERT(DATE, S.[FechaReunion], 103),
            TRY_CONVERT(DATE, S.[FechaSolicitud], 103)
        ),
        TRY_CONVERT(DATE, S.[FechaConclusion], 103),
        NULLIF(LTRIM(RTRIM(S.[UbicacionCarpeta])), N''),
        NULLIF(LTRIM(RTRIM(S.[UbicacionCarpeta])), N''),
        NULLIF(LTRIM(RTRIM(S.[DatosContacto])), N''),
        NULLIF(LTRIM(RTRIM(S.[FechaTextoReunion])), N''),
        S.[RequiereEnvioMonica],
        S.[EnviadoAMonica],
        CASE
            WHEN S.[EnviadoAMonica] = 1 AND TRY_CONVERT(DATE, S.[FechaConclusion], 103) IS NOT NULL
                THEN DATEADD(HOUR, 12, CAST(TRY_CONVERT(DATE, S.[FechaConclusion], 103) AS DATETIME2(0)))
            WHEN S.[EnviadoAMonica] = 1 AND TRY_CONVERT(DATE, S.[FechaReunion], 103) IS NOT NULL
                THEN DATEADD(HOUR, 12, CAST(TRY_CONVERT(DATE, S.[FechaReunion], 103) AS DATETIME2(0)))
            WHEN S.[EnviadoAMonica] = 1
                THEN DATEADD(HOUR, 12, CAST(TRY_CONVERT(DATE, S.[FechaSolicitud], 103) AS DATETIME2(0)))
            ELSE NULL
        END,
        NULLIF(LTRIM(RTRIM(S.[EnviadoAMonicaPorNombre])), N''),
        NULLIF(LTRIM(RTRIM(S.[ObservacionesEnvioMonica])), N''),
        N'Carga histórica Excel reuniones',
        N'Carga histórica Excel reuniones'
    FROM @AsuntosSemilla S
    ORDER BY S.[SeedId];

    INSERT INTO @AsuntosInsertados
    (
        [SeedId],
        [AsuntoId]
    )
    SELECT
        S.[SeedId],
        A.[AsuntoId]
    FROM @AsuntosSemilla S
    INNER JOIN [dgmesnie].[AsuntoSeguimiento] A
        ON A.[GuidAsunto] = S.[GuidAsunto];

    INSERT INTO [dgmesnie].[AsuntoComentario]
    (
        [AsuntoId],
        [IdUsuario],
        [NombreUsuario],
        [Mensaje],
        [TipoMensaje]
    )
    SELECT
        I.[AsuntoId],
        NULL,
        COALESCE(NULLIF(LTRIM(RTRIM(S.[Responsable])), N''), N'Carga histórica Excel'),
        S.[EstadoActual],
        N'Estado'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE NULLIF(LTRIM(RTRIM(S.[EstadoActual])), N'') IS NOT NULL;

    INSERT INTO [dgmesnie].[AsuntoComentario]
    (
        [AsuntoId],
        [IdUsuario],
        [NombreUsuario],
        [Mensaje],
        [TipoMensaje]
    )
    SELECT
        I.[AsuntoId],
        NULL,
        COALESCE(NULLIF(LTRIM(RTRIM(S.[Responsable])), N''), N'Carga histórica Excel'),
        S.[Comentarios],
        N'Seguimiento'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE NULLIF(LTRIM(RTRIM(S.[Comentarios])), N'') IS NOT NULL;

    INSERT INTO [dgmesnie].[AsuntoComentario]
    (
        [AsuntoId],
        [IdUsuario],
        [NombreUsuario],
        [Mensaje],
        [TipoMensaje]
    )
    SELECT
        I.[AsuntoId],
        NULL,
        N'Carga histórica Excel',
        S.[DatosContacto],
        N'Contacto'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE NULLIF(LTRIM(RTRIM(S.[DatosContacto])), N'') IS NOT NULL;

    INSERT INTO [dgmesnie].[AsuntoComentario]
    (
        [AsuntoId],
        [IdUsuario],
        [NombreUsuario],
        [Mensaje],
        [TipoMensaje]
    )
    SELECT
        I.[AsuntoId],
        NULL,
        N'Carga histórica Excel',
        S.[FechaTextoReunion],
        N'Agenda'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE NULLIF(LTRIM(RTRIM(S.[FechaTextoReunion])), N'') IS NOT NULL;

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [ValorAnterior],
        [ValorNuevo],
        [NombreUsuario]
    )
    SELECT
        I.[AsuntoId],
        N'Creación',
        N'Se registró el asunto a partir de la carga histórica del Excel de reuniones.',
        NULL,
        CASE
            WHEN TRY_CONVERT(DATE, S.[FechaConclusion], 103) IS NOT NULL OR S.[EstadoActual] LIKE N'%Finalizado%'
                THEN N'Atendido'
            ELSE N'Pendiente'
        END,
        N'Carga histórica Excel reuniones'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId];

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [ValorAnterior],
        [ValorNuevo],
        [NombreUsuario]
    )
    SELECT
        I.[AsuntoId],
        N'Envío a Mónica',
        COALESCE(NULLIF(LTRIM(RTRIM(S.[ObservacionesEnvioMonica])), N''), N'Se registró el envío histórico del asunto a Mónica de Asuntos Jurídicos.'),
        N'Pendiente',
        N'Enviado',
        COALESCE(NULLIF(LTRIM(RTRIM(S.[EnviadoAMonicaPorNombre])), N''), N'Carga histórica')
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE S.[EnviadoAMonica] = 1;

    INSERT INTO [dgmesnie].[AsuntoMovimiento]
    (
        [AsuntoId],
        [TipoMovimiento],
        [Detalle],
        [ValorAnterior],
        [ValorNuevo],
        [NombreUsuario]
    )
    SELECT
        I.[AsuntoId],
        N'Cambio de estatus',
        N'Se marcó el asunto como atendido durante la migración histórica.',
        N'Pendiente',
        N'Atendido',
        N'Carga histórica Excel reuniones'
    FROM @AsuntosSemilla S
    INNER JOIN @AsuntosInsertados I
        ON I.[SeedId] = S.[SeedId]
    WHERE TRY_CONVERT(DATE, S.[FechaConclusion], 103) IS NOT NULL
       OR S.[EstadoActual] LIKE N'%Finalizado%';
END
GO

UPDATE A
SET
    A.[TituloAsunto] = N'INEEL/FONADIN',
    A.[FechaSolicitud] = COALESCE(A.[FechaSolicitud], CONVERT(DATE, '2026-03-18')),
    A.[TipoAsunto] = COALESCE(NULLIF(A.[TipoAsunto], N''), N'Programa de instalación de paneles solares'),
    A.[Descripcion] = COALESCE(NULLIF(A.[Descripcion], N''), N'Programa para la instalación de paneles solares en entidades del sector público.'),
    A.[DatosContacto] = COALESCE(NULLIF(A.[DatosContacto], N''), N'Víctor Alejandro Salcido'),
    A.[Expediente] = COALESCE(NULLIF(A.[Expediente], N''), N'20260318_INEEL-FONADIN PANELES SOLARES'),
    A.[CarpetaSharePointUrl] = COALESCE(NULLIF(A.[CarpetaSharePointUrl], N''), N'20260318_INEEL-FONADIN PANELES SOLARES'),
    A.[NombreUsuarioUltimaActualizacion] = N'Ajuste histórico Excel reuniones',
    A.[FechaActualizacion] = SYSUTCDATETIME()
FROM [dgmesnie].[AsuntoSeguimiento] A
WHERE A.[TituloAsunto] = N'INEEL / FONADIN / Subsecretaría de Electricidad / CENACE';
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dgmesnie].[AsuntoSeguimiento]
    WHERE [TituloAsunto] = N'Subsecretaría de Electricidad / CENACE'
      AND [FechaSolicitud] = CONVERT(DATE, '2026-03-27')
)
BEGIN
    INSERT INTO [dgmesnie].[AsuntoSeguimiento]
    (
        [TituloAsunto],
        [Descripcion],
        [FechaSolicitud],
        [AreaResponsable],
        [Estatus],
        [TipoAsunto],
        [SemaforoManual],
        [RequiereEnvioMonica],
        [EnviadoAMonica],
        [NombreUsuarioCreacion],
        [NombreUsuarioUltimaActualizacion]
    )
    VALUES
    (
        N'Subsecretaría de Electricidad / CENACE',
        N'Comentarios propuesta PAM RNT RGD 2026-2040',
        CONVERT(DATE, '2026-03-27'),
        N'DGMESNIE',
        N'Pendiente',
        N'De conocimiento',
        N'Verde',
        1,
        0,
        N'Ajuste histórico Excel reuniones',
        N'Ajuste histórico Excel reuniones'
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dgmesnie].[AsuntoSeguimiento]
    WHERE [TituloAsunto] = N'Energía Solar Herrera'
      AND [FechaSolicitud] = CONVERT(DATE, '2026-05-06')
)
BEGIN
    INSERT INTO [dgmesnie].[AsuntoSeguimiento]
    (
        [TituloAsunto],
        [Descripcion],
        [FechaSolicitud],
        [AreaResponsable],
        [Estatus],
        [TipoAsunto],
        [SemaforoManual],
        [Expediente],
        [CarpetaSharePointUrl],
        [DatosContacto],
        [RequiereEnvioMonica],
        [EnviadoAMonica],
        [NombreUsuarioCreacion],
        [NombreUsuarioUltimaActualizacion]
    )
    VALUES
    (
        N'Energía Solar Herrera',
        N'Solicita el reconocimiento del proyecto "Energía Solar Herrera" como parte de un grupo de interés económico de capital mexicano ENNOVA AMÉRICA, S.A.P.I. DE C.V.',
        CONVERT(DATE, '2026-05-06'),
        N'DGMESNIE',
        N'Pendiente',
        N'De conocimiento',
        N'Amarillo',
        N'2026_Energía Solar Herrera',
        N'2026_Energía Solar Herrera',
        N'María Dolores Temoltzin Bustillos | Representante Legal | Energía Solar Herrera | dolores@libienergy.com.mx',
        1,
        0,
        N'Ajuste histórico Excel reuniones',
        N'Ajuste histórico Excel reuniones'
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [dgmesnie].[AsuntoSeguimiento]
    WHERE [TituloAsunto] = N'GTE Energy'
      AND [FechaSolicitud] = CONVERT(DATE, '2026-05-07')
)
BEGIN
    INSERT INTO [dgmesnie].[AsuntoSeguimiento]
    (
        [TituloAsunto],
        [FechaSolicitud],
        [AreaResponsable],
        [Estatus],
        [TipoAsunto],
        [SemaforoManual],
        [DatosContacto],
        [RequiereEnvioMonica],
        [EnviadoAMonica],
        [NombreUsuarioCreacion],
        [NombreUsuarioUltimaActualizacion]
    )
    VALUES
    (
        N'GTE Energy',
        CONVERT(DATE, '2026-05-07'),
        N'DGMESNIE',
        N'Pendiente',
        N'Cartera',
        N'Amarillo',
        N'Mario S. Urbino | Director General de GTE Energy México | +1 858 692 9835 | urbino9@gmail.com',
        1,
        0,
        N'Ajuste histórico Excel reuniones',
        N'Ajuste histórico Excel reuniones'
    );
END
GO