SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.Seccion', N'U') IS NULL
       OR OBJECT_ID(N'dgmesnie.Modulo', N'U') IS NULL
        THROW 51001, N'No existen las tablas de configuracion del menu.', 1;

    DECLARE @ModuloId INT;
    DECLARE @Coincidencias INT;

    SELECT
        @ModuloId = m.ModuloId,
        @Coincidencias = COUNT(*) OVER ()
    FROM dgmesnie.Modulo AS m
    INNER JOIN dgmesnie.Seccion AS s
        ON s.SeccionId = m.SeccionId
    WHERE s.Titulo = N'Consultas'
      AND
      (
          (m.Controller = N'InformePormenorizado' AND m.Action = N'Index')
          OR
          (m.Controller = N'PamrntProyectos' AND m.Action = N'Index')
      );

    IF ISNULL(@Coincidencias, 0) <> 1
        THROW 51002, N'No fue posible identificar de forma unica el modulo PAM del menu Consultas.', 1;

    UPDATE dgmesnie.Modulo
    SET Title = N'PAM / PAMRNT',
        Controller = N'PamrntProyectos',
        Action = N'Index',
        Activo = 1
    WHERE ModuloId = @ModuloId;

    COMMIT TRANSACTION;

    SELECT
        s.Titulo AS Seccion,
        m.ModuloId,
        m.Title,
        m.Controller,
        m.Action,
        m.Activo
    FROM dgmesnie.Modulo AS m
    INNER JOIN dgmesnie.Seccion AS s
        ON s.SeccionId = m.SeccionId
    WHERE m.ModuloId = @ModuloId;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
