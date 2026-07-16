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
        THROW 51001, N'No existe dgmesnie.PAMRevisionPendiente.', 1;

    IF OBJECT_ID(N'dgmesnie.CK_PAMRevision_Estado', N'C') IS NOT NULL
        ALTER TABLE dgmesnie.PAMRevisionPendiente DROP CONSTRAINT CK_PAMRevision_Estado;

    ALTER TABLE dgmesnie.PAMRevisionPendiente WITH CHECK
        ADD CONSTRAINT CK_PAMRevision_Estado CHECK
        (
            Estado IN (N'Pendiente', N'Revisada', N'Aprobada', N'Rechazada')
        );

    UPDATE dgmesnie.PAMRevisionPendiente
    SET Estado = N'Revisada',
        Resolucion = COALESCE(Resolucion, N'Clasificación preliminar registrada; pendiente de aplicación a la cartera.'),
        FechaResolucionUtc = COALESCE(FechaResolucionUtc, FechaDecisionPreliminarUtc, SYSUTCDATETIME()),
        UsuarioResolucion = COALESCE(UsuarioResolucion, UsuarioDecisionPreliminar)
    WHERE Estado = N'Pendiente'
      AND ClasificacionPreliminar IS NOT NULL;

    COMMIT TRANSACTION;

    EXEC sys.sp_releaseapplock
        @Resource = N'dgmesnie.PAM.migrations',
        @LockOwner = N'Session';

    SELECT Estado, COUNT(*) AS Revisiones
    FROM dgmesnie.PAMRevisionPendiente
    GROUP BY Estado
    ORDER BY Estado;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    EXEC sys.sp_releaseapplock
        @Resource = N'dgmesnie.PAM.migrations',
        @LockOwner = N'Session';

    THROW;
END CATCH;
