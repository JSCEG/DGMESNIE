SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PlaneacionInstrumento', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionInstrumento
        (
            InstrumentoId          BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionInstrumento PRIMARY KEY,
            InstrumentoUid         UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PlaneacionInstrumento_Uid DEFAULT NEWSEQUENTIALID(),
            Clave                   NVARCHAR(40) NOT NULL,
            Nombre                  NVARCHAR(300) NOT NULL,
            TipoInstrumento         NVARCHAR(40) NOT NULL,
            Periodicidad            NVARCHAR(30) NOT NULL,
            Descripcion             NVARCHAR(1000) NULL,
            Activo                  BIT NOT NULL
                CONSTRAINT DF_PlaneacionInstrumento_Activo DEFAULT (1),
            FechaRegistroUtc        DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionInstrumento_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro         NVARCHAR(150) NOT NULL,
            VersionFila             ROWVERSION NOT NULL,
            CONSTRAINT UQ_PlaneacionInstrumento_Uid UNIQUE (InstrumentoUid),
            CONSTRAINT UQ_PlaneacionInstrumento_Clave UNIQUE (Clave),
            CONSTRAINT CK_PlaneacionInstrumento_Tipo CHECK
            (
                TipoInstrumento IN
                (N'PROGRAMA_SECTORIAL', N'PLAN_SECTORIAL', N'PROGRAMA_VINCULANTE', N'PROGRAMA_RED', N'FUENTE_OPERATIVA')
            ),
            CONSTRAINT CK_PlaneacionInstrumento_Periodicidad CHECK
            (
                Periodicidad IN (N'ANUAL', N'SEXENAL', N'MENSUAL', N'EVENTUAL', N'CONTINUA')
            )
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PlaneacionVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionVersion
        (
            VersionId              BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionVersion PRIMARY KEY,
            VersionUid             UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PlaneacionVersion_Uid DEFAULT NEWSEQUENTIALID(),
            InstrumentoId          BIGINT NOT NULL,
            ClaveVersion           NVARCHAR(100) NOT NULL,
            Titulo                  NVARCHAR(500) NOT NULL,
            ClaseVersion           NVARCHAR(40) NOT NULL,
            EstadoVersion          NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PlaneacionVersion_Estado DEFAULT N'REGISTRADA',
            EdicionAnio            INT NULL,
            HorizonteInicio        INT NULL,
            HorizonteFin           INT NULL,
            FechaCorte             DATE NULL,
            FechaPublicacion       DATE NULL,
            EsOficial              BIT NOT NULL
                CONSTRAINT DF_PlaneacionVersion_Oficial DEFAULT (0),
            Observaciones          NVARCHAR(MAX) NULL,
            FechaRegistroUtc       DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionVersion_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro        NVARCHAR(150) NOT NULL,
            VersionFila            ROWVERSION NOT NULL,
            CONSTRAINT UQ_PlaneacionVersion_Uid UNIQUE (VersionUid),
            CONSTRAINT UQ_PlaneacionVersion_Clave UNIQUE (InstrumentoId, ClaveVersion),
            CONSTRAINT FK_PlaneacionVersion_Instrumento FOREIGN KEY (InstrumentoId)
                REFERENCES dgmesnie.PlaneacionInstrumento(InstrumentoId),
            CONSTRAINT CK_PlaneacionVersion_Clase CHECK
            (
                ClaseVersion IN
                (N'PUBLICACION_OFICIAL', N'CORTE_BASE', N'BORRADOR_REVISION', N'ACTUALIZACION_OPERATIVA', N'EXTRACCION_ESTRUCTURADA')
            ),
            CONSTRAINT CK_PlaneacionVersion_Estado CHECK
            (
                EstadoVersion IN (N'REGISTRADA', N'EN_REVISION', N'VALIDADA', N'PUBLICADA', N'RECHAZADA')
            ),
            CONSTRAINT CK_PlaneacionVersion_Horizonte CHECK
            (
                (HorizonteInicio IS NULL AND HorizonteFin IS NULL)
                OR (HorizonteInicio BETWEEN 2000 AND 2200
                    AND HorizonteFin BETWEEN HorizonteInicio AND 2200)
            ),
            CONSTRAINT CK_PlaneacionVersion_Edicion CHECK
                (EdicionAnio IS NULL OR EdicionAnio BETWEEN 2000 AND 2200),
            CONSTRAINT CK_PlaneacionVersion_Oficial CHECK
                (EsOficial = 0 OR ClaseVersion = N'PUBLICACION_OFICIAL')
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PlaneacionVersionRelacion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionVersionRelacion
        (
            VersionRelacionId      BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionVersionRelacion PRIMARY KEY,
            VersionOrigenId        BIGINT NOT NULL,
            VersionDestinoId       BIGINT NOT NULL,
            TipoRelacion           NVARCHAR(30) NOT NULL,
            Descripcion            NVARCHAR(1000) NULL,
            FechaRegistroUtc       DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionVersionRelacion_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro        NVARCHAR(150) NOT NULL,
            CONSTRAINT UQ_PlaneacionVersionRelacion UNIQUE
                (VersionOrigenId, VersionDestinoId, TipoRelacion),
            CONSTRAINT FK_PlaneacionVersionRelacion_Origen FOREIGN KEY (VersionOrigenId)
                REFERENCES dgmesnie.PlaneacionVersion(VersionId),
            CONSTRAINT FK_PlaneacionVersionRelacion_Destino FOREIGN KEY (VersionDestinoId)
                REFERENCES dgmesnie.PlaneacionVersion(VersionId),
            CONSTRAINT CK_PlaneacionVersionRelacion_Tipo CHECK
            (
                TipoRelacion IN (N'INTEGRA', N'ACTUALIZA', N'SUSTITUYE', N'DERIVA_DE', N'COMPARA_CON')
            ),
            CONSTRAINT CK_PlaneacionVersionRelacion_Distintas CHECK
                (VersionOrigenId <> VersionDestinoId)
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PlaneacionCarga', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionCarga
        (
            CargaId                 BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionCarga PRIMARY KEY,
            CargaUid                UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PlaneacionCarga_Uid DEFAULT NEWSEQUENTIALID(),
            VersionId               BIGINT NOT NULL,
            NombreArchivo           NVARCHAR(500) NOT NULL,
            HashSha256              CHAR(64) NOT NULL,
            TipoContenido           NVARCHAR(150) NULL,
            HojaPrincipal           NVARCHAR(150) NULL,
            RangoOrigen             NVARCHAR(100) NULL,
            EstadoCarga             NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PlaneacionCarga_Estado DEFAULT N'REGISTRADA',
            TotalFilasFisicas       INT NULL,
            TotalRegistros          INT NULL,
            TotalAdvertencias       INT NOT NULL
                CONSTRAINT DF_PlaneacionCarga_Advertencias DEFAULT (0),
            TotalErrores            INT NOT NULL
                CONSTRAINT DF_PlaneacionCarga_Errores DEFAULT (0),
            PerfilJson              NVARCHAR(MAX) NULL,
            Observaciones           NVARCHAR(MAX) NULL,
            FechaCargaUtc           DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionCarga_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioCarga            NVARCHAR(150) NOT NULL,
            FechaValidacionUtc      DATETIME2(0) NULL,
            UsuarioValidacion       NVARCHAR(150) NULL,
            VersionFila             ROWVERSION NOT NULL,
            CONSTRAINT UQ_PlaneacionCarga_Uid UNIQUE (CargaUid),
            CONSTRAINT UQ_PlaneacionCarga_VersionHash UNIQUE (VersionId, HashSha256),
            CONSTRAINT FK_PlaneacionCarga_Version FOREIGN KEY (VersionId)
                REFERENCES dgmesnie.PlaneacionVersion(VersionId),
            CONSTRAINT CK_PlaneacionCarga_Hash CHECK (LEN(HashSha256) = 64),
            CONSTRAINT CK_PlaneacionCarga_Estado CHECK
            (
                EstadoCarga IN (N'REGISTRADA', N'EN_REVISION', N'VALIDADA', N'PUBLICADA', N'RECHAZADA')
            ),
            CONSTRAINT CK_PlaneacionCarga_Conteos CHECK
            (
                (TotalFilasFisicas IS NULL OR TotalFilasFisicas >= 0)
                AND (TotalRegistros IS NULL OR TotalRegistros >= 0)
                AND TotalAdvertencias >= 0
                AND TotalErrores >= 0
            ),
            CONSTRAINT CK_PlaneacionCarga_PerfilJson CHECK
                (PerfilJson IS NULL OR ISJSON(PerfilJson) = 1),
            CONSTRAINT CK_PlaneacionCarga_Validacion CHECK
            (
                (FechaValidacionUtc IS NULL AND UsuarioValidacion IS NULL)
                OR (FechaValidacionUtc IS NOT NULL AND UsuarioValidacion IS NOT NULL)
            )
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PlaneacionRegistroFuente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionRegistroFuente
        (
            RegistroFuenteId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionRegistroFuente PRIMARY KEY,
            RegistroFuenteUid      UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_PlaneacionRegistroFuente_Uid DEFAULT NEWSEQUENTIALID(),
            CargaId                 BIGINT NOT NULL,
            Hoja                    NVARCHAR(150) NOT NULL,
            NumeroFila              INT NOT NULL,
            ClaseRegistro           NVARCHAR(50) NOT NULL,
            ClaveFuente             NVARCHAR(500) NULL,
            NombreFuente            NVARCHAR(1000) NULL,
            EstadoValidacion        NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PlaneacionRegistroFuente_Estado DEFAULT N'PENDIENTE',
            DatosOrigenJson         NVARCHAR(MAX) NOT NULL,
            IncidenciasJson         NVARCHAR(MAX) NULL,
            HashFila                CHAR(64) NOT NULL,
            FechaRegistroUtc        DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionRegistroFuente_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro         NVARCHAR(150) NOT NULL,
            CONSTRAINT UQ_PlaneacionRegistroFuente_Uid UNIQUE (RegistroFuenteUid),
            CONSTRAINT UQ_PlaneacionRegistroFuente_Fila UNIQUE (CargaId, Hoja, NumeroFila),
            CONSTRAINT FK_PlaneacionRegistroFuente_Carga FOREIGN KEY (CargaId)
                REFERENCES dgmesnie.PlaneacionCarga(CargaId),
            CONSTRAINT CK_PlaneacionRegistroFuente_Fila CHECK (NumeroFila > 0),
            CONSTRAINT CK_PlaneacionRegistroFuente_Clase CHECK
            (
                ClaseRegistro IN
                (N'META', N'ESCENARIO', N'REQUERIMIENTO_CAPACIDAD', N'REQUERIMIENTO_RED',
                 N'PROYECTO', N'CARTERA', N'SUSTITUCION', N'RETIRO', N'OPTIMIZACION', N'OTRO')
            ),
            CONSTRAINT CK_PlaneacionRegistroFuente_Estado CHECK
            (
                EstadoValidacion IN (N'PENDIENTE', N'OBSERVADO', N'VALIDADO', N'DESCARTADO')
            ),
            CONSTRAINT CK_PlaneacionRegistroFuente_DatosJson CHECK (ISJSON(DatosOrigenJson) = 1),
            CONSTRAINT CK_PlaneacionRegistroFuente_IncidenciasJson CHECK
                (IncidenciasJson IS NULL OR ISJSON(IncidenciasJson) = 1),
            CONSTRAINT CK_PlaneacionRegistroFuente_Hash CHECK (LEN(HashFila) = 64)
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PlaneacionCambioRegistro', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PlaneacionCambioRegistro
        (
            CambioRegistroId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PlaneacionCambioRegistro PRIMARY KEY,
            VersionBaseId          BIGINT NOT NULL,
            VersionComparadaId     BIGINT NOT NULL,
            RegistroBaseId         BIGINT NULL,
            RegistroComparadoId    BIGINT NULL,
            TipoCambio             NVARCHAR(40) NOT NULL,
            ClaveComparacion       NVARCHAR(500) NULL,
            ConfianzaCoincidencia  DECIMAL(6,5) NULL,
            DetalleCambioJson      NVARCHAR(MAX) NOT NULL,
            HashCambio             CHAR(64) NOT NULL,
            EstadoRevision         NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PlaneacionCambioRegistro_Estado DEFAULT N'PENDIENTE',
            Observaciones          NVARCHAR(1000) NULL,
            FechaRegistroUtc       DATETIME2(0) NOT NULL
                CONSTRAINT DF_PlaneacionCambioRegistro_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro        NVARCHAR(150) NOT NULL,
            FechaRevisionUtc       DATETIME2(0) NULL,
            UsuarioRevision        NVARCHAR(150) NULL,
            CONSTRAINT UQ_PlaneacionCambioRegistro_Hash UNIQUE
                (VersionBaseId, VersionComparadaId, HashCambio),
            CONSTRAINT FK_PlaneacionCambioRegistro_Base FOREIGN KEY (VersionBaseId)
                REFERENCES dgmesnie.PlaneacionVersion(VersionId),
            CONSTRAINT FK_PlaneacionCambioRegistro_Comparada FOREIGN KEY (VersionComparadaId)
                REFERENCES dgmesnie.PlaneacionVersion(VersionId),
            CONSTRAINT FK_PlaneacionCambioRegistro_RegistroBase FOREIGN KEY (RegistroBaseId)
                REFERENCES dgmesnie.PlaneacionRegistroFuente(RegistroFuenteId),
            CONSTRAINT FK_PlaneacionCambioRegistro_RegistroComparado FOREIGN KEY (RegistroComparadoId)
                REFERENCES dgmesnie.PlaneacionRegistroFuente(RegistroFuenteId),
            CONSTRAINT CK_PlaneacionCambioRegistro_Versiones CHECK (VersionBaseId <> VersionComparadaId),
            CONSTRAINT CK_PlaneacionCambioRegistro_Tipo CHECK
            (
                TipoCambio IN
                (N'ALTA', N'BAJA', N'RENOMBRE', N'REPROGRAMACION', N'CAMBIO_CAPACIDAD',
                 N'CAMBIO_TECNOLOGIA', N'CAMBIO_ESTATUS', N'RECLASIFICACION', N'SIN_CAMBIO', N'OTRO')
            ),
            CONSTRAINT CK_PlaneacionCambioRegistro_Confianza CHECK
                (ConfianzaCoincidencia IS NULL OR ConfianzaCoincidencia BETWEEN 0 AND 1),
            CONSTRAINT CK_PlaneacionCambioRegistro_DetalleJson CHECK (ISJSON(DetalleCambioJson) = 1),
            CONSTRAINT CK_PlaneacionCambioRegistro_Hash CHECK (LEN(HashCambio) = 64),
            CONSTRAINT CK_PlaneacionCambioRegistro_Estado CHECK
                (EstadoRevision IN (N'PENDIENTE', N'VALIDADO', N'RECHAZADO')),
            CONSTRAINT CK_PlaneacionCambioRegistro_Registros CHECK
            (
                RegistroBaseId IS NOT NULL OR RegistroComparadoId IS NOT NULL
            ),
            CONSTRAINT CK_PlaneacionCambioRegistro_Revision CHECK
            (
                (FechaRevisionUtc IS NULL AND UsuarioRevision IS NULL)
                OR (FechaRevisionUtc IS NOT NULL AND UsuarioRevision IS NOT NULL)
            )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PlaneacionVersion')
          AND name = N'IX_PlaneacionVersion_Consulta'
    )
    BEGIN
        CREATE INDEX IX_PlaneacionVersion_Consulta
            ON dgmesnie.PlaneacionVersion(InstrumentoId, EdicionAnio DESC, FechaCorte DESC)
            INCLUDE (ClaveVersion, ClaseVersion, EstadoVersion, HorizonteInicio, HorizonteFin, EsOficial);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PlaneacionCarga')
          AND name = N'IX_PlaneacionCarga_VersionFecha'
    )
    BEGIN
        CREATE INDEX IX_PlaneacionCarga_VersionFecha
            ON dgmesnie.PlaneacionCarga(VersionId, FechaCargaUtc DESC)
            INCLUDE (NombreArchivo, HashSha256, EstadoCarga, TotalRegistros, TotalErrores);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PlaneacionRegistroFuente')
          AND name = N'IX_PlaneacionRegistroFuente_ClaseEstado'
    )
    BEGIN
        CREATE INDEX IX_PlaneacionRegistroFuente_ClaseEstado
            ON dgmesnie.PlaneacionRegistroFuente(CargaId, ClaseRegistro, EstadoValidacion)
            INCLUDE (Hoja, NumeroFila, ClaveFuente, NombreFuente, HashFila);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PlaneacionCambioRegistro')
          AND name = N'IX_PlaneacionCambioRegistro_Comparacion'
    )
    BEGIN
        CREATE INDEX IX_PlaneacionCambioRegistro_Comparacion
            ON dgmesnie.PlaneacionCambioRegistro(VersionBaseId, VersionComparadaId, TipoCambio, EstadoRevision)
            INCLUDE (RegistroBaseId, RegistroComparadoId, ClaveComparacion, ConfianzaCoincidencia);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.PlaneacionInstrumento') AS Instrumentos,
    OBJECT_ID(N'dgmesnie.PlaneacionVersion') AS Versiones,
    OBJECT_ID(N'dgmesnie.PlaneacionVersionRelacion') AS Relaciones,
    OBJECT_ID(N'dgmesnie.PlaneacionCarga') AS Cargas,
    OBJECT_ID(N'dgmesnie.PlaneacionRegistroFuente') AS RegistrosFuente,
    OBJECT_ID(N'dgmesnie.PlaneacionCambioRegistro') AS Cambios;
