IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'Rol_Clave')
BEGIN
    ALTER TABLE [dbo].[Roles] ADD Rol_Clave VARCHAR(50), Rol_Tipo VARCHAR(100), Rol_Grupo VARCHAR(100),
    Rol_NivelAcceso INT, Rol_Ambito VARCHAR(50);
END

IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Rol_ID = 0)
    UPDATE [dbo].[Roles] SET Rol_ID = 0, Rol_Nombre = 'Consulta Pública', Rol_Comentario = 'Acceso general sin privilegios', Rol_Clave = 'PUBLICO', Rol_Tipo = 'Público', Rol_Grupo = 'Público', Rol_NivelAcceso = 5, Rol_Ambito = 'Nacional', Rol_Vigente = 1 WHERE Rol_ID = 0;
ELSE
    INSERT INTO [dbo].[Roles] (Rol_ID, Rol_Nombre, Rol_Comentario, Rol_Clave, Rol_Tipo, Rol_Grupo, Rol_NivelAcceso, Rol_Ambito, Rol_Vigente) VALUES (0, 'Consulta Pública', 'Acceso general sin privilegios', 'PUBLICO', 'Público', 'Público', 5, 'Nacional', 1);

IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Rol_ID = 1)
    UPDATE [dbo].[Roles] SET Rol_ID = 1, Rol_Nombre = 'Secretaría de Energía (SENER)', Rol_Comentario = 'Administrador principal del SNIEr', Rol_Clave = 'SENER', Rol_Tipo = 'Gobierno', Rol_Grupo = 'Normativo', Rol_NivelAcceso = 1, Rol_Ambito = 'Nacional', Rol_Vigente = 1 WHERE Rol_ID = 1;
ELSE
    INSERT INTO [dbo].[Roles] (Rol_ID, Rol_Nombre, Rol_Comentario, Rol_Clave, Rol_Tipo, Rol_Grupo, Rol_NivelAcceso, Rol_Ambito, Rol_Vigente) VALUES (1, 'Secretaría de Energía (SENER)', 'Administrador principal del SNIEr', 'SENER', 'Gobierno', 'Normativo', 1, 'Nacional', 1);

IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Rol_ID = 2)
    UPDATE [dbo].[Roles] SET Rol_ID = 2, Rol_Nombre = 'Unidad del SNIEr', Rol_Comentario = 'Operador técnico designado', Rol_Clave = 'UNIDAD_SNIEr', Rol_Tipo = 'Gobierno', Rol_Grupo = 'Técnico', Rol_NivelAcceso = 1, Rol_Ambito = 'Nacional', Rol_Vigente = 1 WHERE Rol_ID = 2;
ELSE
    INSERT INTO [dbo].[Roles] (Rol_ID, Rol_Nombre, Rol_Comentario, Rol_Clave, Rol_Tipo, Rol_Grupo, Rol_NivelAcceso, Rol_Ambito, Rol_Vigente) VALUES (2, 'Unidad del SNIEr', 'Operador técnico designado', 'UNIDAD_SNIEr', 'Gobierno', 'Técnico', 1, 'Nacional', 1);

IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE Rol_ID = 3)
    UPDATE [dbo].[Roles] SET Rol_ID = 3, Rol_Nombre = 'Dirección General de TICs', Rol_Comentario = 'Soporte y gestión tecnológica', Rol_Clave = 'TICs', Rol_Tipo = 'Gobierno', Rol_Grupo = 'Técnico', Rol_NivelAcceso = 1, Rol_Ambito = 'Nacional', Rol_Vigente = 1 WHERE Rol_ID = 3;
ELSE
    INSERT INTO [dbo].[Roles] (Rol_ID, Rol_Nombre, Rol_Comentario, Rol_Clave, Rol_Tipo, Rol_Grupo, Rol_NivelAcceso, Rol_Ambito, Rol_Vigente) VALUES (3, 'Dirección General de TICs', 'Soporte y gestión tecnológica', 'TICs', 'Gobierno', 'Técnico', 1, 'Nacional', 1);
