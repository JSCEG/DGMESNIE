/*
    Reinicio operativo del modulo de notificaciones.
    Uso: ejecutar antes de una nueva ronda de publicaciones para eliminar
    todas las notificaciones existentes y reiniciar el identity.

    Importante:
    - Este script elimina historico de la tabla [dgmesnie].[Notificacion].
    - No elimina archivos fisicos cargados en wwwroot/img/notificaciones.
    - Si deseas limpiar imagenes antiguas, hazlo aparte en disco.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dgmesnie.Notificacion', 'U') IS NULL
BEGIN
    THROW 50000, 'No existe la tabla [dgmesnie].[Notificacion]. Ejecuta primero el esquema base.', 1;
END
GO

DECLARE @TotalAntes INT;

SELECT @TotalAntes = COUNT(*)
FROM [dgmesnie].[Notificacion];

PRINT 'Total de notificaciones antes del reinicio: ' + CAST(@TotalAntes AS NVARCHAR(20));
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM [dgmesnie].[Notificacion];

    DBCC CHECKIDENT ('[dgmesnie].[Notificacion]', RESEED, 0) WITH NO_INFOMSGS;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

DECLARE @TotalDespues INT;

SELECT @TotalDespues = COUNT(*)
FROM [dgmesnie].[Notificacion];

PRINT 'Total de notificaciones despues del reinicio: ' + CAST(@TotalDespues AS NVARCHAR(20));
GO

SELECT TOP (20)
    [IdNotificacion],
    [Titulo],
    [IdUsuario],
    [RolId],
    [Visto],
    [FechaNotificacion],
    [Activo]
FROM [dgmesnie].[Notificacion]
ORDER BY [IdNotificacion] DESC;
GO

/*
    Verificacion sugerida para el buzon del usuario 1:

    SELECT COUNT(*) AS TotalUsuario1
    FROM [dgmesnie].[Notificacion]
    WHERE [IdUsuario] = 1 AND [Activo] = 1;
*/