IF SCHEMA_ID(N'dgmesnie') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA dgmesnie');
END
GO

IF OBJECT_ID(N'[dgmesnie].[PODECOBISAgenda]', N'U') IS NULL
BEGIN
    CREATE TABLE [dgmesnie].[PODECOBISAgenda]
    (
        [AgendaId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Numero] INT NOT NULL,
        [Polo] NVARCHAR(200) NOT NULL,
        [NombreOficialDeclaratoria] NVARCHAR(400) NOT NULL,
        [OrganismoSeguimiento] NVARCHAR(200) NULL,
        [NombreContacto] NVARCHAR(250) NULL,
        [Cargo] NVARCHAR(150) NULL,
        [Correo] NVARCHAR(250) NULL,
        [NumeroTelefonico] NVARCHAR(120) NULL,
        [Activo] BIT NOT NULL CONSTRAINT [DF_PODECOBISAgenda_Activo] DEFAULT (1),
        [FechaRegistro] DATETIME2(0) NOT NULL CONSTRAINT [DF_PODECOBISAgenda_FechaRegistro] DEFAULT (SYSDATETIME()),
        [FechaActualizacion] DATETIME2(0) NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_PODECOBISAgenda_Numero'
      AND object_id = OBJECT_ID(N'[dgmesnie].[PODECOBISAgenda]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_PODECOBISAgenda_Numero]
        ON [dgmesnie].[PODECOBISAgenda]([Numero]);
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_PODECOBISAgenda_Listar]
    @Busqueda NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BusquedaNormalizada NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');

    SELECT
        [AgendaId],
        [Numero],
        [Polo],
        [NombreOficialDeclaratoria],
        [OrganismoSeguimiento],
        [NombreContacto],
        [Cargo],
        [Correo],
        [NumeroTelefonico],
        [Activo],
        [FechaRegistro],
        [FechaActualizacion]
    FROM [dgmesnie].[PODECOBISAgenda]
    WHERE [Activo] = 1
      AND (
            @BusquedaNormalizada IS NULL
            OR [Polo] LIKE N'%' + @BusquedaNormalizada + N'%'
            OR [NombreOficialDeclaratoria] LIKE N'%' + @BusquedaNormalizada + N'%'
            OR ISNULL([OrganismoSeguimiento], N'') LIKE N'%' + @BusquedaNormalizada + N'%'
            OR ISNULL([NombreContacto], N'') LIKE N'%' + @BusquedaNormalizada + N'%'
            OR ISNULL([Cargo], N'') LIKE N'%' + @BusquedaNormalizada + N'%'
            OR ISNULL([Correo], N'') LIKE N'%' + @BusquedaNormalizada + N'%'
            OR ISNULL([NumeroTelefonico], N'') LIKE N'%' + @BusquedaNormalizada + N'%'
          )
    ORDER BY [Numero];
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_PODECOBISAgenda_ObtenerPorId]
    @AgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [AgendaId],
        [Numero],
        [Polo],
        [NombreOficialDeclaratoria],
        [OrganismoSeguimiento],
        [NombreContacto],
        [Cargo],
        [Correo],
        [NumeroTelefonico],
        [Activo],
        [FechaRegistro],
        [FechaActualizacion]
    FROM [dgmesnie].[PODECOBISAgenda]
    WHERE [AgendaId] = @AgendaId;
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_PODECOBISAgenda_Crear]
    @Numero INT,
    @Polo NVARCHAR(200),
    @NombreOficialDeclaratoria NVARCHAR(400),
    @OrganismoSeguimiento NVARCHAR(200) = NULL,
    @NombreContacto NVARCHAR(250) = NULL,
    @Cargo NVARCHAR(150) = NULL,
    @Correo NVARCHAR(250) = NULL,
    @NumeroTelefonico NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dgmesnie].[PODECOBISAgenda]
    (
        [Numero],
        [Polo],
        [NombreOficialDeclaratoria],
        [OrganismoSeguimiento],
        [NombreContacto],
        [Cargo],
        [Correo],
        [NumeroTelefonico]
    )
    VALUES
    (
        @Numero,
        @Polo,
        @NombreOficialDeclaratoria,
        NULLIF(LTRIM(RTRIM(@OrganismoSeguimiento)), N''),
        NULLIF(LTRIM(RTRIM(@NombreContacto)), N''),
        NULLIF(LTRIM(RTRIM(@Cargo)), N''),
        NULLIF(LTRIM(RTRIM(@Correo)), N''),
        NULLIF(LTRIM(RTRIM(@NumeroTelefonico)), N'')
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_PODECOBISAgenda_Actualizar]
    @AgendaId INT,
    @Numero INT,
    @Polo NVARCHAR(200),
    @NombreOficialDeclaratoria NVARCHAR(400),
    @OrganismoSeguimiento NVARCHAR(200) = NULL,
    @NombreContacto NVARCHAR(250) = NULL,
    @Cargo NVARCHAR(150) = NULL,
    @Correo NVARCHAR(250) = NULL,
    @NumeroTelefonico NVARCHAR(120) = NULL,
    @Activo BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[PODECOBISAgenda]
    SET
        [Numero] = @Numero,
        [Polo] = @Polo,
        [NombreOficialDeclaratoria] = @NombreOficialDeclaratoria,
        [OrganismoSeguimiento] = NULLIF(LTRIM(RTRIM(@OrganismoSeguimiento)), N''),
        [NombreContacto] = NULLIF(LTRIM(RTRIM(@NombreContacto)), N''),
        [Cargo] = NULLIF(LTRIM(RTRIM(@Cargo)), N''),
        [Correo] = NULLIF(LTRIM(RTRIM(@Correo)), N''),
        [NumeroTelefonico] = NULLIF(LTRIM(RTRIM(@NumeroTelefonico)), N''),
        [Activo] = @Activo,
        [FechaActualizacion] = SYSDATETIME()
    WHERE [AgendaId] = @AgendaId;
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_PODECOBISAgenda_Eliminar]
    @AgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[PODECOBISAgenda]
    SET
        [Activo] = 0,
        [FechaActualizacion] = SYSDATETIME()
    WHERE [AgendaId] = @AgendaId;
END
GO

IF NOT EXISTS (SELECT 1 FROM [dgmesnie].[PODECOBISAgenda])
BEGIN
    INSERT INTO [dgmesnie].[PODECOBISAgenda]
    (
        [Numero],
        [Polo],
        [NombreOficialDeclaratoria],
        [OrganismoSeguimiento],
        [NombreContacto],
        [Cargo],
        [Correo],
        [NumeroTelefonico]
    )
    VALUES
        (1, N'Seybaplaya, Campeche', N'Polo de Desarrollo Económico para el Bienestar Seybaplaya I, Campeche', N'SEDECO Campeche', N'Jorge Lavalle', N'Titular', N'sedeco.os@campeche.gob.mx', N'5554345617'),
        (2, N'Juárez, Chihuahua', N'Polo de Desarrollo Económico para el Bienestar San Jerónimo, Chihuahua', N'SEDECO Chihuahua', N'Ulises Alejandro Fernández Gamboa', N'Titular', N'ulises.fernandez@chihuahua.com.mx', N'6692276618'),
        (3, N'Topolobampo, Sinaloa', N'Polo de Desarrollo Económico para el Bienestar Topolobampo, Sinaloa', N'SEDECO Sinaloa', N'Roberto Sánchez', NULL, NULL, N'6671033156'),
        (4, N'Tuxpan, Veracruz', N'Polo de Desarrollo Económico para el Bienestar Tuxpan, Veracruz', N'SEDECO Veracruz', N'Ernesto Astorga', N'Titular', NULL, N'2288241320'),
        (5, N'Chetumal, Quintana Roo', N'Desarrollo Económico para el Bienestar Chetumal, Quintana Roo', NULL, NULL, NULL, NULL, NULL),
        (6, N'San José Chiapa, Puebla', N'Polo de Desarrollo Económico para el Bienestar Futura Capital de la Tecnología y la Sostenibilidad, Puebla', N'SEDECO Puebla', N'Victor Gabriel Chedraui / Hector Juarez', N'Enlace', NULL, N'2221991898 / 222 155 3520'),
        (7, N'Nezahualcóyotl, EdoMex', N'Polo de Desarrollo Económico para el Bienestar Nezahualcóyotl, Estado de Mexico', N'FIDEPAR', N'Marco Antonio González', N'Titular', NULL, N'7228630589'),
        (8, N'Huamantla, Tlaxcala', N'Polo de Desarrollo Económico para el Bienestar Huamantla, Tlaxcala', N'FIDEICOMISO TLAXCALA', N'Alejandro de los Monteros', N'Titular', NULL, N'2213595328'),
        (9, N'Hermosillo, Sonora', N'Desarrollo Económico para el Bienestar e Innovación de Hermosillo, Sonora', NULL, NULL, NULL, NULL, NULL),
        (10, N'Durango, Durango', N'Polo de Desarrollo Económico para el Bienestar Centro Logístico e Industrial deDurango', N'SEDECO Durango', N'Fernando Rosas', N'Titular', NULL, N'5543599772'),
        (11, N'Altamira, Tamaulipas', N'Polo de Desarrollo Económico para el Bienestar Altamira, Tamaulipas.', N'Tamaulipas', N'Victor Landa', N'Enlace', NULL, N'8334621427'),
        (12, N'Morelia, Michoacán', N'Polo de Desarrollo Económico para el Bienestar Parque Industrial Bajío, Michoacán.', N'SEDECO Michoacán', N'Claudio Méndez', NULL, N'claudio.mendez@sedeco.michoacan.gob.mx', N'4433951427'),
        (13, N'AIFA, Hidalgo', N'Polo de Desarrollo Económico para el Bienestar Reserva Zapotlán, Hidalgo', N'SEDECO de Hidalgo', N'Ernesto Cadena', N'Enlace', NULL, N'7711300984'),
        (14, N'Celaya, Guanajuato', N'Polo de Desarrollo Económico para el Bienestar Puerta Logística del Bajío, Guanajuato', N'SEDECO Guanajuato', N'David Aguilera', N'Enlace', NULL, N'4611500812');
END
GO