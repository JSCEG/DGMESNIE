SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @LockResult INT;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'dgmesnie.CarteraConvocatoria.migrations',
    @LockMode = N'Exclusive',
    @LockOwner = N'Session',
    @LockTimeout = 30000;

IF @LockResult < 0
    THROW 51000, N'No fue posible obtener el bloqueo de migración de la cartera.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'dgmesnie') IS NULL
        EXEC(N'CREATE SCHEMA dgmesnie');

    IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaProyecto', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CarteraConvocatoriaProyecto
        (
            ProyectoCarteraId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_CarteraConvocatoriaProyecto PRIMARY KEY,
            ProyectoCoreId          INT NULL,
            Folio                   NVARCHAR(60) NOT NULL,
            Nombre                  NVARCHAR(500) NOT NULL,
            Tipo                    NVARCHAR(40) NOT NULL,
            GerenciaControl         NVARCHAR(100) NOT NULL,
            EntidadFederativa       NVARCHAR(100) NOT NULL,
            Tecnologia              NVARCHAR(150) NULL,
            RazonSocial             NVARCHAR(500) NULL,
            GrupoInteres            NVARCHAR(500) NULL,
            Subestacion             NVARCHAR(500) NULL,
            PuntoInterconexion      NVARCHAR(500) NULL,
            CapacidadMw             DECIMAL(18,3) NOT NULL,
            OrdenPrelacion          INT NOT NULL,
            Prioridad               TINYINT NOT NULL,
            FuentePrioridad         NVARCHAR(500) NULL,
            EstadoSeguimiento       NVARCHAR(20) NOT NULL
                CONSTRAINT DF_CarteraConv_Estado DEFAULT N'revision',
            Fuente                  NVARCHAR(150) NULL,
            FilaFuente              INT NULL,
            VersionFuente           NVARCHAR(150) NULL,
            Activo                  BIT NOT NULL
                CONSTRAINT DF_CarteraConv_Activo DEFAULT (1),
            CreadoUtc               DATETIME2(0) NOT NULL
                CONSTRAINT DF_CarteraConv_Creado DEFAULT SYSUTCDATETIME(),
            CreadoPor               NVARCHAR(150) NULL,
            ActualizadoUtc          DATETIME2(0) NULL,
            ActualizadoPor          NVARCHAR(150) NULL,
            Version                 ROWVERSION NOT NULL,
            CONSTRAINT UQ_CarteraConvocatoriaProyecto_Folio UNIQUE (Folio),
            CONSTRAINT FK_CarteraConvocatoriaProyecto_Core
                FOREIGN KEY (ProyectoCoreId) REFERENCES core.Proyecto(ProyectoId),
            CONSTRAINT CK_CarteraConvocatoriaProyecto_Tipo
                CHECK (Tipo IN (N'Estratégico', N'Particular 2')),
            CONSTRAINT CK_CarteraConvocatoriaProyecto_Prioridad
                CHECK (Prioridad BETWEEN 1 AND 4),
            CONSTRAINT CK_CarteraConvocatoriaProyecto_Estado
                CHECK (EstadoSeguimiento IN (N'continua', N'revision', N'no-continua')),
            CONSTRAINT CK_CarteraConvocatoriaProyecto_Capacidad
                CHECK (CapacidadMw >= 0)
        );

        CREATE INDEX IX_CarteraConvocatoriaProyecto_Prelacion
            ON dgmesnie.CarteraConvocatoriaProyecto(Activo, OrdenPrelacion);
        CREATE INDEX IX_CarteraConvocatoriaProyecto_Gerencia
            ON dgmesnie.CarteraConvocatoriaProyecto(Activo, GerenciaControl, EntidadFederativa);
    END;

    IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaComentario', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CarteraConvocatoriaComentario
        (
            ComentarioId            BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_CarteraConvocatoriaComentario PRIMARY KEY,
            ProyectoCarteraId       BIGINT NOT NULL,
            Sesion                  NVARCHAR(250) NOT NULL,
            FechaSesion             DATE NOT NULL,
            Comentario              NVARCHAR(4000) NOT NULL,
            UsuarioRegistro         NVARCHAR(150) NOT NULL,
            FechaRegistroUtc        DATETIME2(0) NOT NULL
                CONSTRAINT DF_CarteraConvComentario_Fecha DEFAULT SYSUTCDATETIME(),
            OrigenClave             NVARCHAR(200) NULL,
            CONSTRAINT FK_CarteraConvComentario_Proyecto
                FOREIGN KEY (ProyectoCarteraId)
                REFERENCES dgmesnie.CarteraConvocatoriaProyecto(ProyectoCarteraId)
        );

        CREATE INDEX IX_CarteraConvocatoriaComentario_ProyectoFecha
            ON dgmesnie.CarteraConvocatoriaComentario(ProyectoCarteraId, FechaSesion DESC, ComentarioId DESC);
        CREATE UNIQUE INDEX UX_CarteraConvocatoriaComentario_Origen
            ON dgmesnie.CarteraConvocatoriaComentario(OrigenClave)
            WHERE OrigenClave IS NOT NULL;
    END;

    IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaEstadoHistorial', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.CarteraConvocatoriaEstadoHistorial
        (
            EstadoHistorialId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_CarteraConvocatoriaEstadoHistorial PRIMARY KEY,
            ProyectoCarteraId       BIGINT NOT NULL,
            EstadoAnterior          NVARCHAR(20) NULL,
            EstadoNuevo             NVARCHAR(20) NOT NULL,
            UsuarioRegistro         NVARCHAR(150) NOT NULL,
            FechaRegistroUtc        DATETIME2(0) NOT NULL
                CONSTRAINT DF_CarteraConvEstado_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_CarteraConvEstado_Proyecto
                FOREIGN KEY (ProyectoCarteraId)
                REFERENCES dgmesnie.CarteraConvocatoriaProyecto(ProyectoCarteraId),
            CONSTRAINT CK_CarteraConvEstado_Nuevo
                CHECK (EstadoNuevo IN (N'continua', N'revision', N'no-continua'))
        );

        CREATE INDEX IX_CarteraConvocatoriaEstado_ProyectoFecha
            ON dgmesnie.CarteraConvocatoriaEstadoHistorial(ProyectoCarteraId, FechaRegistroUtc DESC);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.CarteraConvocatoriaProyecto', N'U') AS ProyectoObjectId,
    OBJECT_ID(N'dgmesnie.CarteraConvocatoriaComentario', N'U') AS ComentarioObjectId,
    OBJECT_ID(N'dgmesnie.CarteraConvocatoriaEstadoHistorial', N'U') AS HistorialObjectId;
