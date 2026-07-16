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

    IF OBJECT_ID(N'dgmesnie.PAMRevisionPendiente', N'U') IS NULL
        THROW 51001, N'No existe dgmesnie.PAMRevisionPendiente. Ejecute primero las migraciones PAM anteriores.', 1;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'ClasificacionPreliminar') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD ClasificacionPreliminar NVARCHAR(40) NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'NotaPreliminar') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD NotaPreliminar NVARCHAR(1000) NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'FechaDecisionPreliminarUtc') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD FechaDecisionPreliminarUtc DATETIME2(0) NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'UsuarioDecisionPreliminarId') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD UsuarioDecisionPreliminarId INT NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'UsuarioDecisionPreliminar') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD UsuarioDecisionPreliminar NVARCHAR(150) NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'ProyectoRelacionadoPreliminarId') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD ProyectoRelacionadoPreliminarId BIGINT NULL;

    IF COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'VersionDecision') IS NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente ADD VersionDecision ROWVERSION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente')
          AND name = N'FK_PAMRevision_ProyectoRelacionadoPreliminar'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT FK_PAMRevision_ProyectoRelacionadoPreliminar
            FOREIGN KEY (ProyectoRelacionadoPreliminarId)
            REFERENCES dgmesnie.PAMProyecto(ProyectoId);';

    IF OBJECT_ID(N'dgmesnie.CK_PAMRevision_ClasificacionPreliminar', N'C') IS NULL
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
            ADD CONSTRAINT CK_PAMRevision_ClasificacionPreliminar CHECK
            (
                ClasificacionPreliminar IS NULL OR ClasificacionPreliminar IN
                (N''Alta real'', N''Vincular existente'', N''Padre-hijo'', N''Descartar'')
            );';

    IF OBJECT_ID(N'dgmesnie.CK_PAMRevision_DecisionRelacionada', N'C') IS NOT NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente
            DROP CONSTRAINT CK_PAMRevision_DecisionRelacionada;

    EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
        ADD CONSTRAINT CK_PAMRevision_DecisionRelacionada CHECK
        (
            (ClasificacionPreliminar IS NULL AND ProyectoRelacionadoPreliminarId IS NULL)
            OR
            (
                ClasificacionPreliminar IN (N''Vincular existente'', N''Padre-hijo'')
                AND ProyectoRelacionadoPreliminarId IS NOT NULL
            )
            OR
            (
                ClasificacionPreliminar IN (N''Alta real'', N''Descartar'')
                AND ProyectoRelacionadoPreliminarId IS NULL
            )
        );';

    IF EXISTS
    (
        SELECT CambioPropuestoId
        FROM dgmesnie.PAMRevisionPendiente
        WHERE CambioPropuestoId IS NOT NULL
        GROUP BY CambioPropuestoId
        HAVING COUNT(*) > 1
    )
        THROW 51003, N'Hay más de una revisión para una misma propuesta. Corrija los duplicados antes de continuar.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMRevisionPendiente')
          AND name = N'UX_PAMRevision_CambioPropuesto'
    )
        CREATE UNIQUE INDEX UX_PAMRevision_CambioPropuesto
            ON dgmesnie.PAMRevisionPendiente(CambioPropuestoId)
            WHERE CambioPropuestoId IS NOT NULL;

    IF OBJECT_ID(N'dgmesnie.PAMDecisionPreliminarHistorial', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMDecisionPreliminarHistorial
        (
            DecisionHistorialId             BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMDecisionPreliminarHistorial PRIMARY KEY,
            RevisionId                      BIGINT NOT NULL,
            ClasificacionPreliminar         NVARCHAR(40) NOT NULL,
            ProyectoRelacionadoId           BIGINT NULL,
            NotaPreliminar                  NVARCHAR(1000) NULL,
            UsuarioDecisionPreliminarId     INT NULL,
            UsuarioDecisionPreliminar       NVARCHAR(150) NOT NULL,
            FechaDecisionPreliminarUtc      DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMDecisionHistorial_Fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_PAMDecisionHistorial_Revision FOREIGN KEY (RevisionId)
                REFERENCES dgmesnie.PAMRevisionPendiente(RevisionId),
            CONSTRAINT FK_PAMDecisionHistorial_Proyecto FOREIGN KEY (ProyectoRelacionadoId)
                REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT CK_PAMDecisionHistorial_Clasificacion CHECK
            (
                ClasificacionPreliminar IN
                (N'Alta real', N'Vincular existente', N'Padre-hijo', N'Descartar')
            ),
            CONSTRAINT CK_PAMDecisionHistorial_Relacion CHECK
            (
                (ClasificacionPreliminar IN (N'Vincular existente', N'Padre-hijo')
                 AND ProyectoRelacionadoId IS NOT NULL)
                OR
                (ClasificacionPreliminar IN (N'Alta real', N'Descartar')
                 AND ProyectoRelacionadoId IS NULL)
            )
        );
    END;

    IF COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'DecisionHistorialId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'RevisionId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'ClasificacionPreliminar') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'ProyectoRelacionadoId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'NotaPreliminar') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'UsuarioDecisionPreliminarId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'UsuarioDecisionPreliminar') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMDecisionPreliminarHistorial', N'FechaDecisionPreliminarUtc') IS NULL
        THROW 51004, N'PAMDecisionPreliminarHistorial existe con una estructura incompatible.', 1;

    IF OBJECT_ID(N'dgmesnie.PK_PAMDecisionPreliminarHistorial', N'PK') IS NULL
        ALTER TABLE dgmesnie.PAMDecisionPreliminarHistorial
            ADD CONSTRAINT PK_PAMDecisionPreliminarHistorial PRIMARY KEY (DecisionHistorialId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMDecisionPreliminarHistorial')
          AND name = N'FK_PAMDecisionHistorial_Revision'
    )
        ALTER TABLE dgmesnie.PAMDecisionPreliminarHistorial WITH CHECK
            ADD CONSTRAINT FK_PAMDecisionHistorial_Revision FOREIGN KEY (RevisionId)
            REFERENCES dgmesnie.PAMRevisionPendiente(RevisionId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dgmesnie.PAMDecisionPreliminarHistorial')
          AND name = N'FK_PAMDecisionHistorial_Proyecto'
    )
        ALTER TABLE dgmesnie.PAMDecisionPreliminarHistorial WITH CHECK
            ADD CONSTRAINT FK_PAMDecisionHistorial_Proyecto FOREIGN KEY (ProyectoRelacionadoId)
            REFERENCES dgmesnie.PAMProyecto(ProyectoId);

    IF OBJECT_ID(N'dgmesnie.CK_PAMDecisionHistorial_Clasificacion', N'C') IS NULL
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMDecisionPreliminarHistorial WITH CHECK
            ADD CONSTRAINT CK_PAMDecisionHistorial_Clasificacion CHECK
            (
                ClasificacionPreliminar IN
                (N''Alta real'', N''Vincular existente'', N''Padre-hijo'', N''Descartar'')
            );';

    IF OBJECT_ID(N'dgmesnie.CK_PAMDecisionHistorial_Relacion', N'C') IS NULL
        EXEC sys.sp_executesql N'ALTER TABLE dgmesnie.PAMDecisionPreliminarHistorial WITH CHECK
            ADD CONSTRAINT CK_PAMDecisionHistorial_Relacion CHECK
            (
                (ClasificacionPreliminar IN (N''Vincular existente'', N''Padre-hijo'')
                 AND ProyectoRelacionadoId IS NOT NULL)
                OR
                (ClasificacionPreliminar IN (N''Alta real'', N''Descartar'')
                 AND ProyectoRelacionadoId IS NULL)
            );';

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMDecisionPreliminarHistorial')
          AND name = N'IX_PAMDecisionHistorial_RevisionFecha'
    )
        CREATE INDEX IX_PAMDecisionHistorial_RevisionFecha
            ON dgmesnie.PAMDecisionPreliminarHistorial
                (RevisionId, FechaDecisionPreliminarUtc DESC, DecisionHistorialId DESC)
            INCLUDE
                (ClasificacionPreliminar, ProyectoRelacionadoId,
                 UsuarioDecisionPreliminarId, UsuarioDecisionPreliminar);

    EXEC sys.sp_executesql N'
            CREATE OR ALTER TRIGGER dgmesnie.TR_PAMDecisionPreliminarHistorial_AppendOnly
            ON dgmesnie.PAMDecisionPreliminarHistorial
            AFTER UPDATE, DELETE
            AS
            BEGIN
                SET NOCOUNT ON;
                THROW 51002, N''PAMDecisionPreliminarHistorial es append-only; no admite actualizaciones ni eliminaciones.'', 1;
            END;';

    COMMIT TRANSACTION;
    EXEC sys.sp_releaseapplock @Resource = N'dgmesnie.PAM.migrations', @LockOwner = N'Session';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    EXEC sys.sp_releaseapplock @Resource = N'dgmesnie.PAM.migrations', @LockOwner = N'Session';
    THROW;
END CATCH;

SELECT
    COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'ClasificacionPreliminar') AS ClasificacionConfigurada,
    COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'ProyectoRelacionadoPreliminarId') AS RelacionConfigurada,
    COL_LENGTH(N'dgmesnie.PAMRevisionPendiente', N'VersionDecision') AS VersionDecisionConfigurada,
    OBJECT_ID(N'dgmesnie.PAMDecisionPreliminarHistorial') AS HistorialConfigurado;
