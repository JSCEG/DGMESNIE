SET NOCOUNT ON;

IF OBJECT_ID('dgmesnie.Acceso', 'U') IS NULL
BEGIN
    RAISERROR('La tabla [dgmesnie].[Acceso] no existe.', 16, 1);
    RETURN;
END;

SELECT COUNT(*) AS RegistrosAntes
FROM [dgmesnie].[Acceso]
WHERE [TipoAcceso] = 'Inicio de sesión funcionario CRE';

UPDATE [dgmesnie].[Acceso]
SET [TipoAcceso] = 'Inicio de sesión funcionario SENER'
WHERE [TipoAcceso] = 'Inicio de sesión funcionario CRE';

SELECT COUNT(*) AS RegistrosDespues
FROM [dgmesnie].[Acceso]
WHERE [TipoAcceso] = 'Inicio de sesión funcionario SENER';

SELECT TOP (50)
    [IdAcceso],
    [IdUsuario],
    [Correo],
    [TipoAcceso],
    [FechaAcceso],
    [Ip],
    [Exitoso],
    [Observaciones]
FROM [dgmesnie].[Acceso]
WHERE [TipoAcceso] = 'Inicio de sesión funcionario SENER'
ORDER BY [FechaAcceso] DESC;
