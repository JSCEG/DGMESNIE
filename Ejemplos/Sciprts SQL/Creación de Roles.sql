CREATE TABLE [dbo].[Roles_SNIEr] (
    Rol_ID INT PRIMARY KEY,
    Rol_Clave VARCHAR(50) NOT NULL,                  -- Identificador único corto (ej. SENER, TICs)
    Rol_Nombre NVARCHAR(100) NOT NULL,               -- Nombre completo del rol
    Rol_Comentario NVARCHAR(500) NULL,               -- Descripción breve
    Rol_Tipo VARCHAR(100) NULL,                      -- Público, Gobierno, Academia, Privado
    Rol_Grupo VARCHAR(100) NULL,                     -- Consejo, Técnico, Normativo, etc.
    Rol_NivelAcceso INT NULL,                        -- Nivel de jerarquía (1 = más alto)
    Rol_Ambito VARCHAR(50) NULL,                     -- Nacional, Estatal, Municipal
    Rol_Vigente BIT NOT NULL DEFAULT 1,              -- Activo o no
    Rol_FechaMod DATETIME DEFAULT GETDATE()          -- Fecha de creación o actualización
);
