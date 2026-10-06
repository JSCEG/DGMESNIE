-- Borra los rastros de uso de una cuenta antes de entregarla a su titular:
-- accesos, sesiones y bitácora de actividad generados durante las pruebas.
-- No toca la cuenta, sus roles ni su contraseña.
--
-- Uso:  sqlcmd ... -v IdUsuario=96 -i database\mantenimiento\reiniciar_rastros_usuario.sql
-- El borrado es definitivo: revise el conteo "antes" que imprime el script.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @IdUsuario INT = $(IdUsuario);

IF NOT EXISTS (SELECT 1 FROM dgmesnie.Usuario WHERE IdUsuario = @IdUsuario)
BEGIN
    RAISERROR('No existe el usuario %d en dgmesnie.Usuario.', 16, 1, @IdUsuario);
    RETURN;
END;

SELECT 'antes' AS momento,
       (SELECT COUNT(*) FROM dgmesnie.Acceso       WHERE IdUsuario = @IdUsuario) AS Acceso,
       (SELECT COUNT(*) FROM dgmesnie.Sesion       WHERE IdUsuario = @IdUsuario) AS Sesion,
       (SELECT COUNT(*) FROM dgmesnie.ActividadLog WHERE IdUsuario = @IdUsuario) AS ActividadLog;

BEGIN TRANSACTION;

DELETE FROM dgmesnie.Acceso       WHERE IdUsuario = @IdUsuario;
DELETE FROM dgmesnie.Sesion       WHERE IdUsuario = @IdUsuario;
DELETE FROM dgmesnie.ActividadLog WHERE IdUsuario = @IdUsuario;

COMMIT TRANSACTION;

SELECT 'despues' AS momento,
       (SELECT COUNT(*) FROM dgmesnie.Acceso       WHERE IdUsuario = @IdUsuario) AS Acceso,
       (SELECT COUNT(*) FROM dgmesnie.Sesion       WHERE IdUsuario = @IdUsuario) AS Sesion,
       (SELECT COUNT(*) FROM dgmesnie.ActividadLog WHERE IdUsuario = @IdUsuario) AS ActividadLog;
