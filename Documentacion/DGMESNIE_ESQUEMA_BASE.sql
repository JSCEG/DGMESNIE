/*
    Script base de arquitectura relacional para DGMESNIE
    Objetivo: aislar seguridad, sesiones, navegación autorizada, notificaciones y auditoría
    Motor: Azure SQL / SQL Server
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dgmesnie')
BEGIN
    EXEC('CREATE SCHEMA [dgmesnie]');
END
GO

/* =========================
   CATALOGOS BASE
   ========================= */

IF OBJECT_ID('dgmesnie.Rol', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Rol]
    (
        [RolId] INT IDENTITY(1,1) NOT NULL,
        [RolClave] VARCHAR(50) NOT NULL,
        [RolNombre] NVARCHAR(100) NOT NULL,
        [RolComentario] NVARCHAR(500) NULL,
        [RolTipo] VARCHAR(100) NULL,
        [RolGrupo] VARCHAR(100) NULL,
        [RolNivelAcceso] INT NULL,
        [RolAmbito] VARCHAR(50) NULL,
        [RolVigente] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Rol_RolVigente] DEFAULT (1),
        [RolFechaMod] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Rol_RolFechaMod] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_Rol] PRIMARY KEY CLUSTERED ([RolId]),
        CONSTRAINT [UQ_dgmesnie_Rol_RolClave] UNIQUE ([RolClave])
    );
END
GO

IF OBJECT_ID('dgmesnie.Mercado', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Mercado]
    (
        [MercadoId] INT IDENTITY(1,1) NOT NULL,
        [MercadoClave] VARCHAR(50) NULL,
        [MercadoNombre] NVARCHAR(150) NOT NULL,
        [MercadoComentario] NVARCHAR(500) NULL,
        [MercadoVigente] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Mercado_MercadoVigente] DEFAULT (1),
        [MercadoFechaMod] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Mercado_MercadoFechaMod] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_Mercado] PRIMARY KEY CLUSTERED ([MercadoId])
    );
END
GO

/* =========================
   SEGURIDAD OPERATIVA
   ========================= */

IF OBJECT_ID('dgmesnie.Usuario', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Usuario]
    (
        [IdUsuario] INT IDENTITY(1,1) NOT NULL,
        [Correo] NVARCHAR(256) NOT NULL,
        [ClaveHash] NVARCHAR(500) NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [RFC] NVARCHAR(20) NULL,
        [Cargo] NVARCHAR(150) NULL,
        [UnidadAdscripcion] NVARCHAR(200) NULL,
        [ClaveEmpleado] NVARCHAR(50) NULL,
        [Vigente] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Usuario_Vigente] DEFAULT (1),
        [FechaAlta] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Usuario_FechaAlta] DEFAULT (SYSUTCDATETIME()),
        [FechaActualizacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Usuario_FechaActualizacion] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_dgmesnie_Usuario] PRIMARY KEY CLUSTERED ([IdUsuario]),
        CONSTRAINT [UQ_dgmesnie_Usuario_Correo] UNIQUE ([Correo])
    );
END
GO

IF OBJECT_ID('dgmesnie.UsuarioRol', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[UsuarioRol]
    (
        [UsuarioRolId] INT IDENTITY(1,1) NOT NULL,
        [IdUsuario] INT NOT NULL,
        [RolId] INT NOT NULL,
        [MercadoId] INT NULL,
        [Vigente] BIT NOT NULL CONSTRAINT [DF_dgmesnie_UsuarioRol_Vigente] DEFAULT (1),
        [QuienRegistro] INT NULL,
        [FechaModificacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_UsuarioRol_FechaModificacion] DEFAULT (SYSUTCDATETIME()),
        [Comentarios] NVARCHAR(500) NULL,
        CONSTRAINT [PK_dgmesnie_UsuarioRol] PRIMARY KEY CLUSTERED ([UsuarioRolId]),
        CONSTRAINT [FK_dgmesnie_UsuarioRol_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [FK_dgmesnie_UsuarioRol_Rol] FOREIGN KEY ([RolId]) REFERENCES [dgmesnie].[Rol]([RolId]),
        CONSTRAINT [FK_dgmesnie_UsuarioRol_Mercado] FOREIGN KEY ([MercadoId]) REFERENCES [dgmesnie].[Mercado]([MercadoId])
    );
END
GO

IF OBJECT_ID('dgmesnie.Sesion', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Sesion]
    (
        [IdSesion] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_dgmesnie_Sesion_IdSesion] DEFAULT (NEWID()),
        [IdUsuario] INT NOT NULL,
        [SessionKey] NVARCHAR(200) NOT NULL,
        [FechaInicio] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Sesion_FechaInicio] DEFAULT (SYSUTCDATETIME()),
        [UltimaActividad] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Sesion_UltimaActividad] DEFAULT (SYSUTCDATETIME()),
        [FechaExpiracion] DATETIME2(0) NOT NULL,
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Sesion_Activa] DEFAULT (1),
        [Ip] NVARCHAR(64) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [OrigenAcceso] NVARCHAR(50) NULL,
        CONSTRAINT [PK_dgmesnie_Sesion] PRIMARY KEY CLUSTERED ([IdSesion]),
        CONSTRAINT [FK_dgmesnie_Sesion_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [UQ_dgmesnie_Sesion_SessionKey] UNIQUE ([SessionKey])
    );
END
GO

IF OBJECT_ID('dgmesnie.RecuperacionContrasena', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[RecuperacionContrasena]
    (
        [IdRecuperacion] INT IDENTITY(1,1) NOT NULL,
        [IdUsuario] INT NOT NULL,
        [Token] NVARCHAR(250) NOT NULL,
        [FechaCreacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_RecuperacionContrasena_FechaCreacion] DEFAULT (SYSUTCDATETIME()),
        [FechaExpiracion] DATETIME2(0) NOT NULL,
        [Usado] BIT NOT NULL CONSTRAINT [DF_dgmesnie_RecuperacionContrasena_Usado] DEFAULT (0),
        [FechaUso] DATETIME2(0) NULL,
        CONSTRAINT [PK_dgmesnie_RecuperacionContrasena] PRIMARY KEY CLUSTERED ([IdRecuperacion]),
        CONSTRAINT [FK_dgmesnie_RecuperacionContrasena_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [UQ_dgmesnie_RecuperacionContrasena_Token] UNIQUE ([Token])
    );
END
GO

IF OBJECT_ID('dgmesnie.Notificacion', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Notificacion]
    (
        [IdNotificacion] INT IDENTITY(1,1) NOT NULL,
        [GuidNotificacion] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_dgmesnie_Notificacion_GuidNotificacion] DEFAULT (NEWID()),
        [Titulo] NVARCHAR(250) NOT NULL,
        [Mensaje] NVARCHAR(MAX) NOT NULL,
        [FechaNotificacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Notificacion_FechaNotificacion] DEFAULT (SYSUTCDATETIME()),
        [Link] NVARCHAR(500) NULL,
        [IdUsuario] INT NULL,
        [RolId] INT NULL,
        [Visto] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Notificacion_Visto] DEFAULT (0),
        [FechaVisto] DATETIME2(0) NULL,
        [Imagen] NVARCHAR(500) NULL,
        [Activo] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Notificacion_Activo] DEFAULT (1),
        CONSTRAINT [PK_dgmesnie_Notificacion] PRIMARY KEY CLUSTERED ([IdNotificacion]),
        CONSTRAINT [FK_dgmesnie_Notificacion_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [FK_dgmesnie_Notificacion_Rol] FOREIGN KEY ([RolId]) REFERENCES [dgmesnie].[Rol]([RolId])
    );
END
GO

IF OBJECT_ID('dgmesnie.ActividadLog', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[ActividadLog]
    (
        [IdActividad] BIGINT IDENTITY(1,1) NOT NULL,
        [IdUsuario] INT NULL,
        [NombreUsuario] NVARCHAR(200) NULL,
        [Accion] NVARCHAR(200) NOT NULL,
        [Controlador] NVARCHAR(150) NULL,
        [Pagina] NVARCHAR(250) NULL,
        [Tipo] NVARCHAR(100) NULL,
        [Elemento] NVARCHAR(150) NULL,
        [IdElemento] NVARCHAR(150) NULL,
        [Valor] NVARCHAR(MAX) NULL,
        [Timestamp] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_ActividadLog_Timestamp] DEFAULT (SYSUTCDATETIME()),
        [AdditionalData] NVARCHAR(MAX) NULL,
        [Ip] NVARCHAR(64) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        CONSTRAINT [PK_dgmesnie_ActividadLog] PRIMARY KEY CLUSTERED ([IdActividad]),
        CONSTRAINT [FK_dgmesnie_ActividadLog_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

IF OBJECT_ID('dgmesnie.Acceso', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Acceso]
    (
        [IdAcceso] BIGINT IDENTITY(1,1) NOT NULL,
        [IdUsuario] INT NULL,
        [Correo] NVARCHAR(256) NULL,
        [TipoAcceso] NVARCHAR(100) NOT NULL,
        [FechaAcceso] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_Acceso_FechaAcceso] DEFAULT (SYSUTCDATETIME()),
        [Ip] NVARCHAR(64) NULL,
        [Exitoso] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Acceso_Exitoso] DEFAULT (1),
        [Observaciones] NVARCHAR(500) NULL,
        CONSTRAINT [PK_dgmesnie_Acceso] PRIMARY KEY CLUSTERED ([IdAcceso]),
        CONSTRAINT [FK_dgmesnie_Acceso_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario])
    );
END
GO

/* =========================
   NAVEGACION Y PERMISOS
   ========================= */

IF OBJECT_ID('dgmesnie.Seccion', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Seccion]
    (
        [SeccionId] INT IDENTITY(1,1) NOT NULL,
        [Titulo] NVARCHAR(150) NOT NULL,
        [Articulos] NVARCHAR(150) NULL,
        [FundamentoLegal] NVARCHAR(MAX) NULL,
        [Descripcion] NVARCHAR(MAX) NULL,
        [Ayuda] NVARCHAR(MAX) NULL,
        [Objetivo] NVARCHAR(MAX) NULL,
        [ResponsableNormativo] NVARCHAR(MAX) NULL,
        [PublicoObjetivo] NVARCHAR(MAX) NULL,
        [Orden] INT NOT NULL CONSTRAINT [DF_dgmesnie_Seccion_Orden] DEFAULT (1),
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Seccion_Activa] DEFAULT (1),
        CONSTRAINT [PK_dgmesnie_Seccion] PRIMARY KEY CLUSTERED ([SeccionId])
    );
END
GO

IF OBJECT_ID('dgmesnie.Modulo', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Modulo]
    (
        [ModuloId] INT IDENTITY(1,1) NOT NULL,
        [SeccionId] INT NOT NULL,
        [Title] NVARCHAR(150) NOT NULL,
        [FundamentoLegalModulo] NVARCHAR(MAX) NULL,
        [Perfiles] NVARCHAR(150) NULL,
        [Etapa] NVARCHAR(100) NULL,
        [JustificacionOrden] NVARCHAR(MAX) NULL,
        [AyudaContextual] NVARCHAR(MAX) NULL,
        [Controller] NVARCHAR(100) NULL,
        [Action] NVARCHAR(100) NULL,
        [Descripcion] NVARCHAR(MAX) NULL,
        [Imagen] NVARCHAR(200) NULL,
        [BotonTexto] NVARCHAR(100) NULL,
        [ElementosUI] NVARCHAR(MAX) NULL,
        [AyudaVista] NVARCHAR(MAX) NULL,
        [Orden] INT NOT NULL CONSTRAINT [DF_dgmesnie_Modulo_Orden] DEFAULT (1),
        [Activo] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Modulo_Activo] DEFAULT (1),
        [EsExterno] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Modulo_EsExterno] DEFAULT (0),
        CONSTRAINT [PK_dgmesnie_Modulo] PRIMARY KEY CLUSTERED ([ModuloId]),
        CONSTRAINT [FK_dgmesnie_Modulo_Seccion] FOREIGN KEY ([SeccionId]) REFERENCES [dgmesnie].[Seccion]([SeccionId])
    );
END
GO

IF OBJECT_ID('dgmesnie.Vista', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[Vista]
    (
        [VistaId] INT IDENTITY(1,1) NOT NULL,
        [ModuloId] INT NOT NULL,
        [Titulo] NVARCHAR(150) NOT NULL,
        [Perfiles] NVARCHAR(150) NULL,
        [Controller] NVARCHAR(100) NULL,
        [Action] NVARCHAR(100) NULL,
        [Orden] INT NOT NULL CONSTRAINT [DF_dgmesnie_Vista_Orden] DEFAULT (1),
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Vista_Activa] DEFAULT (1),
        [EsExterno] BIT NOT NULL CONSTRAINT [DF_dgmesnie_Vista_EsExterno] DEFAULT (0),
        CONSTRAINT [PK_dgmesnie_Vista] PRIMARY KEY CLUSTERED ([VistaId]),
        CONSTRAINT [FK_dgmesnie_Vista_Modulo] FOREIGN KEY ([ModuloId]) REFERENCES [dgmesnie].[Modulo]([ModuloId])
    );
END
GO

IF OBJECT_ID('dgmesnie.RolSeccion', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[RolSeccion]
    (
        [RolSeccionId] INT IDENTITY(1,1) NOT NULL,
        [RolId] INT NOT NULL,
        [SeccionId] INT NOT NULL,
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_RolSeccion_Activa] DEFAULT (1),
        CONSTRAINT [PK_dgmesnie_RolSeccion] PRIMARY KEY CLUSTERED ([RolSeccionId]),
        CONSTRAINT [FK_dgmesnie_RolSeccion_Rol] FOREIGN KEY ([RolId]) REFERENCES [dgmesnie].[Rol]([RolId]),
        CONSTRAINT [FK_dgmesnie_RolSeccion_Seccion] FOREIGN KEY ([SeccionId]) REFERENCES [dgmesnie].[Seccion]([SeccionId]),
        CONSTRAINT [UQ_dgmesnie_RolSeccion] UNIQUE ([RolId], [SeccionId])
    );
END
GO

IF OBJECT_ID('dgmesnie.RolModulo', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[RolModulo]
    (
        [RolModuloId] INT IDENTITY(1,1) NOT NULL,
        [RolId] INT NOT NULL,
        [ModuloId] INT NOT NULL,
        [MercadoId] INT NULL,
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_RolModulo_Activa] DEFAULT (1),
        CONSTRAINT [PK_dgmesnie_RolModulo] PRIMARY KEY CLUSTERED ([RolModuloId]),
        CONSTRAINT [FK_dgmesnie_RolModulo_Rol] FOREIGN KEY ([RolId]) REFERENCES [dgmesnie].[Rol]([RolId]),
        CONSTRAINT [FK_dgmesnie_RolModulo_Modulo] FOREIGN KEY ([ModuloId]) REFERENCES [dgmesnie].[Modulo]([ModuloId]),
        CONSTRAINT [FK_dgmesnie_RolModulo_Mercado] FOREIGN KEY ([MercadoId]) REFERENCES [dgmesnie].[Mercado]([MercadoId]),
        CONSTRAINT [UQ_dgmesnie_RolModulo] UNIQUE ([RolId], [ModuloId], [MercadoId])
    );
END
GO

IF OBJECT_ID('dgmesnie.RolVista', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[RolVista]
    (
        [RolVistaId] INT IDENTITY(1,1) NOT NULL,
        [RolId] INT NOT NULL,
        [VistaId] INT NOT NULL,
        [MercadoId] INT NULL,
        [Activa] BIT NOT NULL CONSTRAINT [DF_dgmesnie_RolVista_Activa] DEFAULT (1),
        CONSTRAINT [PK_dgmesnie_RolVista] PRIMARY KEY CLUSTERED ([RolVistaId]),
        CONSTRAINT [FK_dgmesnie_RolVista_Rol] FOREIGN KEY ([RolId]) REFERENCES [dgmesnie].[Rol]([RolId]),
        CONSTRAINT [FK_dgmesnie_RolVista_Vista] FOREIGN KEY ([VistaId]) REFERENCES [dgmesnie].[Vista]([VistaId]),
        CONSTRAINT [FK_dgmesnie_RolVista_Mercado] FOREIGN KEY ([MercadoId]) REFERENCES [dgmesnie].[Mercado]([MercadoId]),
        CONSTRAINT [UQ_dgmesnie_RolVista] UNIQUE ([RolId], [VistaId], [MercadoId])
    );
END
GO

IF OBJECT_ID('dgmesnie.UsuarioVistaOverride', 'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[UsuarioVistaOverride]
    (
        [UsuarioVistaOverrideId] INT IDENTITY(1,1) NOT NULL,
        [IdUsuario] INT NOT NULL,
        [VistaId] INT NOT NULL,
        [Permitida] BIT NOT NULL,
        [FechaModificacion] DATETIME2(0) NOT NULL CONSTRAINT [DF_dgmesnie_UsuarioVistaOverride_FechaModificacion] DEFAULT (SYSUTCDATETIME()),
        [Comentarios] NVARCHAR(500) NULL,
        CONSTRAINT [PK_dgmesnie_UsuarioVistaOverride] PRIMARY KEY CLUSTERED ([UsuarioVistaOverrideId]),
        CONSTRAINT [FK_dgmesnie_UsuarioVistaOverride_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [dgmesnie].[Usuario]([IdUsuario]),
        CONSTRAINT [FK_dgmesnie_UsuarioVistaOverride_Vista] FOREIGN KEY ([VistaId]) REFERENCES [dgmesnie].[Vista]([VistaId]),
        CONSTRAINT [UQ_dgmesnie_UsuarioVistaOverride] UNIQUE ([IdUsuario], [VistaId])
    );
END
GO

/* =========================
   INDICES
   ========================= */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Usuario_Correo_Vigente' AND object_id = OBJECT_ID('dgmesnie.Usuario'))
    CREATE INDEX [IX_dgmesnie_Usuario_Correo_Vigente] ON [dgmesnie].[Usuario] ([Correo], [Vigente]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_UsuarioRol_IdUsuario_Vigente' AND object_id = OBJECT_ID('dgmesnie.UsuarioRol'))
    CREATE INDEX [IX_dgmesnie_UsuarioRol_IdUsuario_Vigente] ON [dgmesnie].[UsuarioRol] ([IdUsuario], [Vigente]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Sesion_IdUsuario_Activa' AND object_id = OBJECT_ID('dgmesnie.Sesion'))
    CREATE INDEX [IX_dgmesnie_Sesion_IdUsuario_Activa] ON [dgmesnie].[Sesion] ([IdUsuario], [Activa], [FechaExpiracion]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_RecuperacionContrasena_Token_Usado' AND object_id = OBJECT_ID('dgmesnie.RecuperacionContrasena'))
    CREATE INDEX [IX_dgmesnie_RecuperacionContrasena_Token_Usado] ON [dgmesnie].[RecuperacionContrasena] ([Token], [Usado], [FechaExpiracion]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Notificacion_IdUsuario_Visto' AND object_id = OBJECT_ID('dgmesnie.Notificacion'))
    CREATE INDEX [IX_dgmesnie_Notificacion_IdUsuario_Visto] ON [dgmesnie].[Notificacion] ([IdUsuario], [Visto], [FechaNotificacion]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_ActividadLog_IdUsuario_Timestamp' AND object_id = OBJECT_ID('dgmesnie.ActividadLog'))
    CREATE INDEX [IX_dgmesnie_ActividadLog_IdUsuario_Timestamp] ON [dgmesnie].[ActividadLog] ([IdUsuario], [Timestamp]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Acceso_IdUsuario_FechaAcceso' AND object_id = OBJECT_ID('dgmesnie.Acceso'))
    CREATE INDEX [IX_dgmesnie_Acceso_IdUsuario_FechaAcceso] ON [dgmesnie].[Acceso] ([IdUsuario], [FechaAcceso]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Modulo_SeccionId_Activo_Orden' AND object_id = OBJECT_ID('dgmesnie.Modulo'))
    CREATE INDEX [IX_dgmesnie_Modulo_SeccionId_Activo_Orden] ON [dgmesnie].[Modulo] ([SeccionId], [Activo], [Orden]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_Vista_ModuloId_Activa_Orden' AND object_id = OBJECT_ID('dgmesnie.Vista'))
    CREATE INDEX [IX_dgmesnie_Vista_ModuloId_Activa_Orden] ON [dgmesnie].[Vista] ([ModuloId], [Activa], [Orden]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_RolModulo_RolId_Activa' AND object_id = OBJECT_ID('dgmesnie.RolModulo'))
    CREATE INDEX [IX_dgmesnie_RolModulo_RolId_Activa] ON [dgmesnie].[RolModulo] ([RolId], [Activa], [MercadoId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dgmesnie_RolVista_RolId_Activa' AND object_id = OBJECT_ID('dgmesnie.RolVista'))
    CREATE INDEX [IX_dgmesnie_RolVista_RolId_Activa] ON [dgmesnie].[RolVista] ([RolId], [Activa], [MercadoId]);
GO

PRINT 'Esquema base dgmesnie creado o validado correctamente.';
GO