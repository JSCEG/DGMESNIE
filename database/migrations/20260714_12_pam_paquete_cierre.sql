SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMAplicacionPaquete', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMAplicacionPaquete
        (
            PaqueteAplicacionId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMAplicacionPaquete PRIMARY KEY,
            PaqueteUid UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PAMAplicacionPaquete_Uid DEFAULT NEWSEQUENTIALID(),
            LoteId BIGINT NOT NULL,
            AnalisisId BIGINT NOT NULL,
            HashContenido CHAR(64) NOT NULL,
            TotalDetalles INT NOT NULL,
            TotalCambiosCampo INT NOT NULL,
            TotalAccionesClave INT NOT NULL,
            ResumenJson NVARCHAR(MAX) NOT NULL,
            UsuarioCreacionId INT NULL,
            UsuarioCreacion NVARCHAR(150) NOT NULL,
            FechaCreacionUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMAplicacionPaquete_Fecha DEFAULT SYSUTCDATETIME(),
            VersionPaquete ROWVERSION NOT NULL,
            CONSTRAINT UQ_PAMAplicacionPaquete_Uid UNIQUE (PaqueteUid),
            CONSTRAINT UQ_PAMAplicacionPaquete_AnalisisHash UNIQUE (AnalisisId, HashContenido),
            CONSTRAINT FK_PAMAplicacionPaquete_Lote
                FOREIGN KEY (LoteId) REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT FK_PAMAplicacionPaquete_Analisis
                FOREIGN KEY (AnalisisId) REFERENCES dgmesnie.PAMAnalisisEjecucion(AnalisisId),
            CONSTRAINT CK_PAMAplicacionPaquete_Hash CHECK (LEN(HashContenido) = 64),
            CONSTRAINT CK_PAMAplicacionPaquete_Totales
                CHECK (TotalDetalles >= 0 AND TotalCambiosCampo >= 0 AND TotalAccionesClave >= 0
                    AND TotalDetalles = TotalCambiosCampo + TotalAccionesClave),
            CONSTRAINT CK_PAMAplicacionPaquete_ResumenJson CHECK (ISJSON(ResumenJson) = 1)
        );

        CREATE INDEX IX_PAMAplicacionPaquete_LoteFecha
            ON dgmesnie.PAMAplicacionPaquete(LoteId, FechaCreacionUtc DESC, PaqueteAplicacionId DESC)
            INCLUDE (AnalisisId, HashContenido, TotalDetalles, UsuarioCreacion);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMAplicacionPaqueteDetalle', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMAplicacionPaqueteDetalle
        (
            PaqueteDetalleId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMAplicacionPaqueteDetalle PRIMARY KEY,
            PaqueteAplicacionId BIGINT NOT NULL,
            TipoElemento NVARCHAR(20) NOT NULL,
            ReferenciaId BIGINT NOT NULL,
            Orden INT NOT NULL,
            Accion NVARCHAR(40) NOT NULL,
            ProyectoId BIGINT NULL,
            ProyectoVersionBaseId BIGINT NULL,
            ProyectoRelacionadoId BIGINT NULL,
            ClaveProyecto NVARCHAR(300) NULL,
            NombreProyecto NVARCHAR(1000) NULL,
            Campo NVARCHAR(150) NULL,
            ValorAnteriorJson NVARCHAR(MAX) NULL,
            ValorNuevoJson NVARCHAR(MAX) NULL,
            FuenteDocumento NVARCHAR(500) NULL,
            UbicacionFuente NVARCHAR(500) NULL,
            HashDetalle CHAR(64) NOT NULL,
            FechaRegistroUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMAplicacionPaqueteDetalle_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT UQ_PAMAplicacionPaqueteDetalle_Referencia
                UNIQUE (PaqueteAplicacionId, TipoElemento, ReferenciaId),
            CONSTRAINT FK_PAMAplicacionPaqueteDetalle_Paquete
                FOREIGN KEY (PaqueteAplicacionId)
                REFERENCES dgmesnie.PAMAplicacionPaquete(PaqueteAplicacionId),
            CONSTRAINT CK_PAMAplicacionPaqueteDetalle_Tipo
                CHECK (TipoElemento IN (N'CambioCampo', N'AccionClave')),
            CONSTRAINT CK_PAMAplicacionPaqueteDetalle_Orden CHECK (Orden > 0),
            CONSTRAINT CK_PAMAplicacionPaqueteDetalle_Hash CHECK (LEN(HashDetalle) = 64),
            CONSTRAINT CK_PAMAplicacionPaqueteDetalle_AnteriorJson
                CHECK (ValorAnteriorJson IS NULL OR ISJSON(ValorAnteriorJson) = 1),
            CONSTRAINT CK_PAMAplicacionPaqueteDetalle_NuevoJson
                CHECK (ValorNuevoJson IS NULL OR ISJSON(ValorNuevoJson) = 1)
        );

        CREATE INDEX IX_PAMAplicacionPaqueteDetalle_PaqueteOrden
            ON dgmesnie.PAMAplicacionPaqueteDetalle(PaqueteAplicacionId, Orden)
            INCLUDE (TipoElemento, ReferenciaId, Accion, Campo, HashDetalle);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMAplicacionPaqueteEvento', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMAplicacionPaqueteEvento
        (
            PaqueteEventoId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMAplicacionPaqueteEvento PRIMARY KEY,
            PaqueteAplicacionId BIGINT NOT NULL,
            TipoEvento NVARCHAR(20) NOT NULL,
            Nota NVARCHAR(1000) NULL,
            DatosJson NVARCHAR(MAX) NULL,
            UsuarioEventoId INT NULL,
            UsuarioEvento NVARCHAR(150) NOT NULL,
            FechaEventoUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMAplicacionPaqueteEvento_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_PAMAplicacionPaqueteEvento_Paquete
                FOREIGN KEY (PaqueteAplicacionId)
                REFERENCES dgmesnie.PAMAplicacionPaquete(PaqueteAplicacionId),
            CONSTRAINT CK_PAMAplicacionPaqueteEvento_Tipo
                CHECK (TipoEvento IN (N'Congelado', N'Aprobado', N'Rechazado', N'Aplicado', N'Anulado')),
            CONSTRAINT CK_PAMAplicacionPaqueteEvento_DatosJson
                CHECK (DatosJson IS NULL OR ISJSON(DatosJson) = 1)
        );

        CREATE INDEX IX_PAMAplicacionPaqueteEvento_PaqueteFecha
            ON dgmesnie.PAMAplicacionPaqueteEvento(PaqueteAplicacionId, PaqueteEventoId DESC)
            INCLUDE (TipoEvento, UsuarioEvento, FechaEventoUtc);
    END;

    EXEC sys.sp_executesql N'
        CREATE OR ALTER TRIGGER dgmesnie.TR_PAMAplicacionPaquete_AppendOnly
        ON dgmesnie.PAMAplicacionPaquete
        INSTEAD OF UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            THROW 51030, N''El paquete de cierre es inmutable; cree un paquete nuevo si cambian las decisiones.'', 1;
        END;';

    EXEC sys.sp_executesql N'
        CREATE OR ALTER TRIGGER dgmesnie.TR_PAMAplicacionPaqueteDetalle_AppendOnly
        ON dgmesnie.PAMAplicacionPaqueteDetalle
        INSTEAD OF UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            THROW 51031, N''El detalle del paquete de cierre es de solo anexado.'', 1;
        END;';

    EXEC sys.sp_executesql N'
        CREATE OR ALTER TRIGGER dgmesnie.TR_PAMAplicacionPaqueteEvento_AppendOnly
        ON dgmesnie.PAMAplicacionPaqueteEvento
        INSTEAD OF UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            THROW 51032, N''El historial del paquete de cierre es de solo anexado.'', 1;
        END;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.PAMAplicacionPaquete') AS Paquete,
    OBJECT_ID(N'dgmesnie.PAMAplicacionPaqueteDetalle') AS Detalle,
    OBJECT_ID(N'dgmesnie.PAMAplicacionPaqueteEvento') AS Evento;
