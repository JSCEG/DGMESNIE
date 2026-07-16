SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @LockResult INT;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'dgmesnie.PAM.migrations',
    @LockMode = N'Exclusive',
    @LockOwner = N'Session',
    @LockTimeout = 30000;

IF @LockResult < 0
    THROW 51000, N'No fue posible obtener el bloqueo de migración PAM.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMAnalisisEjecucion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMAnalisisEjecucion
        (
            AnalisisId                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMAnalisisEjecucion PRIMARY KEY,
            AnalisisUid               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PAMAnalisis_Uid DEFAULT NEWSEQUENTIALID(),
            LoteId                    BIGINT NOT NULL,
            NumeroIntento             INT NOT NULL,
            VersionMotor              NVARCHAR(30) NOT NULL,
            HuellaEntrada             CHAR(64) NOT NULL,
            Estado                    NVARCHAR(40) NOT NULL CONSTRAINT DF_PAMAnalisis_Estado DEFAULT N'Pendiente',
            DisponibleDesdeUtc        DATETIME2(0) NOT NULL CONSTRAINT DF_PAMAnalisis_Disponible DEFAULT SYSUTCDATETIME(),
            FechaSolicitudUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMAnalisis_Solicitud DEFAULT SYSUTCDATETIME(),
            FechaInicioUtc            DATETIME2(0) NULL,
            FechaHeartbeatUtc         DATETIME2(0) NULL,
            FechaFinUtc               DATETIME2(0) NULL,
            LeaseUid                  UNIQUEIDENTIFIER NULL,
            EsReintentable            BIT NOT NULL CONSTRAINT DF_PAMAnalisis_Reintentable DEFAULT (1),
            TotalFuentes              INT NOT NULL CONSTRAINT DF_PAMAnalisis_TotalFuentes DEFAULT (0),
            FuentesProcesadas         INT NOT NULL CONSTRAINT DF_PAMAnalisis_FuentesProcesadas DEFAULT (0),
            FuentesError              INT NOT NULL CONSTRAINT DF_PAMAnalisis_FuentesError DEFAULT (0),
            RegistrosDetectados       INT NOT NULL CONSTRAINT DF_PAMAnalisis_Registros DEFAULT (0),
            ProyectosCoincidentes     INT NOT NULL CONSTRAINT DF_PAMAnalisis_Coincidentes DEFAULT (0),
            ProyectosNoEncontrados    INT NOT NULL CONSTRAINT DF_PAMAnalisis_NoEncontrados DEFAULT (0),
            CambiosPropuestos         INT NOT NULL CONSTRAINT DF_PAMAnalisis_Cambios DEFAULT (0),
            TotalObservados           INT NOT NULL CONSTRAINT DF_PAMAnalisis_Observados DEFAULT (0),
            CodigoError               NVARCHAR(100) NULL,
            MensajeResultado          NVARCHAR(2000) NULL,
            UsuarioId                 INT NOT NULL,
            UsuarioNombre             NVARCHAR(150) NOT NULL,
            VersionFila               ROWVERSION,
            CONSTRAINT UQ_PAMAnalisis_Uid UNIQUE (AnalisisUid),
            CONSTRAINT UQ_PAMAnalisis_LoteIntento UNIQUE (LoteId, NumeroIntento),
            CONSTRAINT FK_PAMAnalisis_Lote FOREIGN KEY (LoteId) REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT CK_PAMAnalisis_Huella CHECK (LEN(HuellaEntrada) = 64),
            CONSTRAINT CK_PAMAnalisis_Intento CHECK (NumeroIntento > 0),
            CONSTRAINT CK_PAMAnalisis_Estado CHECK
                (Estado IN (N'Pendiente', N'Ejecutando', N'Completado', N'Completado con observaciones', N'Error', N'Cancelado', N'Obsoleto')),
            CONSTRAINT CK_PAMAnalisis_Contadores CHECK
                (TotalFuentes >= 0 AND FuentesProcesadas >= 0 AND FuentesError >= 0
                 AND RegistrosDetectados >= 0 AND ProyectosCoincidentes >= 0
                 AND ProyectosNoEncontrados >= 0 AND CambiosPropuestos >= 0 AND TotalObservados >= 0),
            CONSTRAINT CK_PAMAnalisis_Lease CHECK
                (Estado <> N'Ejecutando' OR (LeaseUid IS NOT NULL AND FechaInicioUtc IS NOT NULL AND FechaHeartbeatUtc IS NOT NULL))
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMAnalisisEjecucion') AND name = N'IX_PAMAnalisis_Cola'
    )
        CREATE INDEX IX_PAMAnalisis_Cola
            ON dgmesnie.PAMAnalisisEjecucion(Estado, DisponibleDesdeUtc, FechaSolicitudUtc, AnalisisId)
            INCLUDE (LoteId, FechaHeartbeatUtc, VersionMotor);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMAnalisisEjecucion') AND name = N'UX_PAMAnalisis_LoteEjecutando'
    )
        CREATE UNIQUE INDEX UX_PAMAnalisis_LoteEjecutando
            ON dgmesnie.PAMAnalisisEjecucion(LoteId)
            WHERE Estado = N'Ejecutando';

    IF OBJECT_ID(N'dgmesnie.PAMHallazgoProyecto', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMHallazgoProyecto
        (
            HallazgoId                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMHallazgoProyecto PRIMARY KEY,
            AnalisisId                BIGINT NOT NULL,
            LoteFuenteId              BIGINT NOT NULL,
            MetodoDeteccion           NVARCHAR(40) NOT NULL,
            TipoHallazgo              NVARCHAR(40) NOT NULL,
            ClaveDetectada            NVARCHAR(200) NULL,
            ClaveNormalizada          NVARCHAR(200) NULL,
            NombreDetectado           NVARCHAR(500) NULL,
            HojaPaginaSeccion         NVARCHAR(250) NOT NULL,
            NumeroReferencia          INT NULL,
            FragmentoEvidencia        NVARCHAR(2000) NULL,
            DatosExtraidosJson        NVARCHAR(MAX) NULL,
            HashHallazgo              CHAR(64) NOT NULL,
            ResultadoCotejo           NVARCHAR(30) NOT NULL,
            NumeroCandidatos          INT NOT NULL CONSTRAINT DF_PAMHallazgo_Candidatos DEFAULT (0),
            ProyectoIdCoincidente     BIGINT NULL,
            ProyectoVersionIdCoincidente BIGINT NULL,
            CandidatosJson            NVARCHAR(MAX) NULL,
            Confianza                 DECIMAL(5,4) NULL,
            FechaCotejoUtc            DATETIME2(0) NULL,
            FechaRegistroUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_PAMHallazgo_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT UQ_PAMHallazgo UNIQUE (AnalisisId, LoteFuenteId, HashHallazgo),
            CONSTRAINT FK_PAMHallazgo_Analisis FOREIGN KEY (AnalisisId) REFERENCES dgmesnie.PAMAnalisisEjecucion(AnalisisId),
            CONSTRAINT FK_PAMHallazgo_LoteFuente FOREIGN KEY (LoteFuenteId) REFERENCES dgmesnie.PAMLoteFuente(LoteFuenteId),
            CONSTRAINT FK_PAMHallazgo_Proyecto FOREIGN KEY (ProyectoIdCoincidente) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMHallazgo_Version FOREIGN KEY (ProyectoVersionIdCoincidente) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT CK_PAMHallazgo_Tipo CHECK
                (TipoHallazgo IN (N'Fila estructurada', N'Referencia documental', N'Incidencia')),
            CONSTRAINT CK_PAMHallazgo_Resultado CHECK
                (ResultadoCotejo IN (N'Exacta', N'Normalizada', N'Ambigua', N'No encontrada', N'Inválida', N'Sin clave', N'Observado')),
            CONSTRAINT CK_PAMHallazgo_Confianza CHECK (Confianza IS NULL OR Confianza BETWEEN 0 AND 1),
            CONSTRAINT CK_PAMHallazgo_DatosJson CHECK (DatosExtraidosJson IS NULL OR ISJSON(DatosExtraidosJson) = 1),
            CONSTRAINT CK_PAMHallazgo_CandidatosJson CHECK (CandidatosJson IS NULL OR ISJSON(CandidatosJson) = 1)
        );

        CREATE INDEX IX_PAMHallazgo_AnalisisResultado
            ON dgmesnie.PAMHallazgoProyecto(AnalisisId, ResultadoCotejo, HallazgoId)
            INCLUDE (ProyectoIdCoincidente, ClaveNormalizada, LoteFuenteId);
        CREATE INDEX IX_PAMHallazgo_Clave
            ON dgmesnie.PAMHallazgoProyecto(ClaveNormalizada, AnalisisId)
            WHERE ClaveNormalizada IS NOT NULL;
    END;

    IF OBJECT_ID(N'dgmesnie.PAMCambioPropuesto', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMCambioPropuesto
        (
            CambioPropuestoId         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMCambioPropuesto PRIMARY KEY,
            AnalisisId                BIGINT NOT NULL,
            HallazgoId                BIGINT NOT NULL,
            ProyectoId                BIGINT NULL,
            ProyectoVersionBaseId     BIGINT NULL,
            TipoCambio                NVARCHAR(30) NOT NULL,
            Campo                     NVARCHAR(150) NULL,
            TipoDato                  NVARCHAR(30) NULL,
            ValorActualJson           NVARCHAR(MAX) NULL,
            ValorPropuestoJson        NVARCHAR(MAX) NOT NULL,
            Motivo                    NVARCHAR(1000) NULL,
            Confianza                 DECIMAL(5,4) NULL,
            HashPropuesta             CHAR(64) NOT NULL,
            EstadoRevision            NVARCHAR(30) NOT NULL CONSTRAINT DF_PAMCambioPropuesto_Estado DEFAULT N'Pendiente',
            Resolucion                NVARCHAR(1000) NULL,
            FechaRegistroUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_PAMCambioPropuesto_Fecha DEFAULT SYSUTCDATETIME(),
            FechaResolucionUtc        DATETIME2(0) NULL,
            UsuarioResolucionId       INT NULL,
            UsuarioResolucion         NVARCHAR(150) NULL,
            CONSTRAINT UQ_PAMCambioPropuesto UNIQUE (AnalisisId, HashPropuesta),
            CONSTRAINT FK_PAMCambioPropuesto_Analisis FOREIGN KEY (AnalisisId) REFERENCES dgmesnie.PAMAnalisisEjecucion(AnalisisId),
            CONSTRAINT FK_PAMCambioPropuesto_Hallazgo FOREIGN KEY (HallazgoId) REFERENCES dgmesnie.PAMHallazgoProyecto(HallazgoId),
            CONSTRAINT FK_PAMCambioPropuesto_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMCambioPropuesto_Version FOREIGN KEY (ProyectoVersionBaseId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT CK_PAMCambioPropuesto_Tipo CHECK
                (TipoCambio IN (N'Alta', N'Modificación', N'Baja', N'Reactivación', N'Observación')),
            CONSTRAINT CK_PAMCambioPropuesto_Estado CHECK
                (EstadoRevision IN (N'Pendiente', N'Requiere revisión', N'Aprobada', N'Rechazada', N'Conflicto', N'Obsoleta')),
            CONSTRAINT CK_PAMCambioPropuesto_ActualJson CHECK (ValorActualJson IS NULL OR ISJSON(ValorActualJson) = 1),
            CONSTRAINT CK_PAMCambioPropuesto_NuevoJson CHECK (ISJSON(ValorPropuestoJson) = 1),
            CONSTRAINT CK_PAMCambioPropuesto_Confianza CHECK (Confianza IS NULL OR Confianza BETWEEN 0 AND 1),
            CONSTRAINT CK_PAMCambioPropuesto_Hash CHECK (LEN(HashPropuesta) = 64)
        );

        CREATE INDEX IX_PAMCambioPropuesto_AnalisisEstado
            ON dgmesnie.PAMCambioPropuesto(AnalisisId, EstadoRevision, CambioPropuestoId)
            INCLUDE (ProyectoId, ProyectoVersionBaseId, TipoCambio, Campo, Confianza);
    END;

    IF EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente')
          AND name = N'CargaId' AND is_nullable = 0
    )
        ALTER TABLE dgmesnie.PAMRevisionPendiente ALTER COLUMN CargaId BIGINT NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'AnalisisId') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD AnalisisId BIGINT NULL;
    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'HallazgoId') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD HallazgoId BIGINT NULL;
    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'CambioPropuestoId') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD CambioPropuestoId BIGINT NULL;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente') AND name = N'FK_PAMRevision_Analisis'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT FK_PAMRevision_Analisis FOREIGN KEY (AnalisisId)
            REFERENCES dgmesnie.PAMAnalisisEjecucion(AnalisisId);';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente') AND name = N'FK_PAMRevision_Hallazgo'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT FK_PAMRevision_Hallazgo FOREIGN KEY (HallazgoId)
            REFERENCES dgmesnie.PAMHallazgoProyecto(HallazgoId);';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente') AND name = N'FK_PAMRevision_CambioPropuesto'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT FK_PAMRevision_CambioPropuesto FOREIGN KEY (CambioPropuestoId)
            REFERENCES dgmesnie.PAMCambioPropuesto(CambioPropuestoId);';

    IF OBJECT_ID(N'dgmesnie.CK_PAMRevision_Origen', N'C') IS NULL
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT CK_PAMRevision_Origen CHECK (CargaId IS NOT NULL OR AnalisisId IS NOT NULL);';

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente') AND name = N'IX_PAMRevision_AnalisisEstado'
    )
        EXEC sys.sp_executesql N'CREATE INDEX IX_PAMRevision_AnalisisEstado
            ON dgmesnie.PAMRevisionPendiente(AnalisisId, Estado)
            INCLUDE (HallazgoId, CambioPropuestoId, TipoRevision);';

    COMMIT TRANSACTION;
    EXEC sys.sp_releaseapplock @Resource = N'dgmesnie.PAM.migrations', @LockOwner = N'Session';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    EXEC sys.sp_releaseapplock @Resource = N'dgmesnie.PAM.migrations', @LockOwner = N'Session';
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.PAMAnalisisEjecucion') AS AnalisisConfigurado,
    OBJECT_ID(N'dgmesnie.PAMHallazgoProyecto') AS HallazgosConfigurados,
    OBJECT_ID(N'dgmesnie.PAMCambioPropuesto') AS CambiosConfigurados;
