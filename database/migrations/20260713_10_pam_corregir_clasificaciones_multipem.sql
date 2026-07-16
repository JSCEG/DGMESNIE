SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @LockResult INT;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'dgmesnie.PAM.correccion-multipem-20260713',
    @LockMode = N'Exclusive',
    @LockOwner = N'Session',
    @LockTimeout = 30000;

IF @LockResult < 0
    THROW 51000, N'No fue posible obtener el bloqueo para corregir las clasificaciones PEM.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Correcciones TABLE
    (
        CambioPropuestoId BIGINT NOT NULL PRIMARY KEY,
        ProyectoRelacionadoId BIGINT NOT NULL
    );

    INSERT @Correcciones (CambioPropuestoId, ProyectoRelacionadoId)
    VALUES
        (1364, 1), (1365, 1), (1366, 1), (1367, 1),
        (1570, 2), (1571, 2), (1572, 2),
        (1573, 2), (1574, 2), (1575, 2),
        (2085, 106), (2285, 64);

    DECLARE @Aplicadas TABLE (RevisionId BIGINT NOT NULL PRIMARY KEY);

    INSERT dgmesnie.PAMDecisionPreliminarHistorial
    (
        RevisionId, ClasificacionPreliminar, ProyectoRelacionadoId,
        NotaPreliminar, UsuarioDecisionPreliminarId, UsuarioDecisionPreliminar
    )
    OUTPUT inserted.RevisionId INTO @Aplicadas (RevisionId)
    SELECT
        revision.RevisionId,
        N'Vincular existente',
        revision.ProyectoRelacionadoPreliminarId,
        N'Criterio corregido tras cotejo del PAM: varias claves PEM de una misma fila oficial pertenecen al mismo proyecto y no acreditan una relacion padre-hijo.',
        revision.UsuarioDecisionPreliminarId,
        COALESCE(revision.UsuarioDecisionPreliminar, N'Corrección asistida PAM')
    FROM dgmesnie.PAMRevisionPendiente revision
    INNER JOIN @Correcciones correccion
        ON correccion.CambioPropuestoId = revision.CambioPropuestoId
       AND correccion.ProyectoRelacionadoId = revision.ProyectoRelacionadoPreliminarId
    WHERE revision.Estado = N'Revisada'
      AND revision.ClasificacionPreliminar = N'Padre-hijo';

    UPDATE revision
    SET ClasificacionPreliminar = N'Vincular existente',
        NotaPreliminar = N'Criterio corregido tras cotejo del PAM: varias claves PEM de una misma fila oficial pertenecen al mismo proyecto y no acreditan una relacion padre-hijo.',
        FechaDecisionPreliminarUtc = SYSUTCDATETIME(),
        Resolucion = N'Clasificacion corregida a Vincular existente; pendiente de aplicacion a la cartera.',
        FechaResolucionUtc = SYSUTCDATETIME()
    FROM dgmesnie.PAMRevisionPendiente revision
    INNER JOIN @Aplicadas aplicada ON aplicada.RevisionId = revision.RevisionId;

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS ClasificacionesCorregidas FROM @Aplicadas;
    SELECT revision.CambioPropuestoId, revision.Estado,
           revision.ClasificacionPreliminar, revision.ProyectoRelacionadoPreliminarId
    FROM dgmesnie.PAMRevisionPendiente revision
    INNER JOIN @Correcciones correccion
        ON correccion.CambioPropuestoId = revision.CambioPropuestoId
    ORDER BY revision.CambioPropuestoId;

    EXEC sys.sp_releaseapplock
        @Resource = N'dgmesnie.PAM.correccion-multipem-20260713',
        @LockOwner = N'Session';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    EXEC sys.sp_releaseapplock
        @Resource = N'dgmesnie.PAM.correccion-multipem-20260713',
        @LockOwner = N'Session';
    THROW;
END CATCH;
