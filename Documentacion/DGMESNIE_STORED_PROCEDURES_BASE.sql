/*
    Stored procedures base para autenticacion, sesion y navegacion autorizada en dgmesnie.
    Este script no toca dbo. La idea es permitir migracion gradual del codigo hacia dgmesnie.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================
   VALIDACION DE USUARIO
   ========================= */

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_ValidarUsuario]
    @CorreoRFC NVARCHAR(256),
    @Clave NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        u.[IdUsuario]
    FROM [dgmesnie].[Usuario] u
    WHERE u.[Vigente] = 1
      AND u.[ClaveHash] = @Clave
      AND (
            u.[Correo] = @CorreoRFC
            OR u.[RFC] = @CorreoRFC
      )
    ORDER BY u.[IdUsuario];
END
GO

/* =========================
   PERFIL DE SESION
   ========================= */

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_ObtenerPerfilSesion]
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RolActivo AS
    (
        SELECT TOP (1)
            ur.[IdUsuario],
            ur.[RolId],
            ur.[MercadoId],
            ur.[Vigente],
            ur.[QuienRegistro],
            ur.[FechaModificacion],
            ur.[Comentarios]
        FROM [dgmesnie].[UsuarioRol] ur
        WHERE ur.[IdUsuario] = @IdUsuario
          AND ur.[Vigente] = 1
        ORDER BY ur.[FechaModificacion] DESC, ur.[UsuarioRolId] DESC
    )
    SELECT
        CAST(u.[IdUsuario] AS NVARCHAR(50)) AS [IdUsuario],
        u.[Correo] AS [Correo],
        u.[ClaveHash] AS [Clave],
        u.[Nombre] AS [Nombre],
        u.[UnidadAdscripcion] AS [Unidad_de_Adscripcion],
        u.[Cargo] AS [Cargo],
        CAST(CASE WHEN s.[Activa] = 1 THEN 1 ELSE 0 END AS NVARCHAR(10)) AS [SesionActiva],
        CONVERT(NVARCHAR(30), s.[UltimaActividad], 120) AS [UltimaActualizacion],
        u.[RFC] AS [RFC],
        CAST(u.[Vigente] AS NVARCHAR(10)) AS [Vigente],
        u.[ClaveEmpleado] AS [ClaveEmpleado],
        CONVERT(NVARCHAR(30), s.[FechaInicio], 120) AS [HoraInicioSesion],
        r.[RolNombre] AS [Rol],
        CAST(ra.[MercadoId] AS NVARCHAR(50)) AS [Mercado_ID],
        CAST(ra.[Vigente] AS NVARCHAR(10)) AS [RolUsuario_Vigente],
        CAST(ra.[QuienRegistro] AS NVARCHAR(50)) AS [RolUsuario_QuienRegistro],
        CONVERT(NVARCHAR(30), ra.[FechaModificacion], 120) AS [RolUsuario_FechaMod],
        ra.[Comentarios] AS [RolUsuario_Comentarios],
        CAST(r.[RolId] AS NVARCHAR(50)) AS [Rol_ID],
        r.[RolNombre] AS [Rol_Nombre],
        CAST(r.[RolVigente] AS NVARCHAR(10)) AS [Rol_Vigente],
        CONVERT(NVARCHAR(30), r.[RolFechaMod], 120) AS [Rol_FechaMod],
        r.[RolComentario] AS [Rol_Comentario],
        m.[MercadoNombre] AS [Mercado_Nombre],
        CAST(m.[MercadoVigente] AS NVARCHAR(10)) AS [Mercado_Vigente],
        CONVERT(NVARCHAR(30), m.[MercadoFechaMod], 120) AS [Mercado_FechaMod],
        m.[MercadoComentario] AS [Mercado_Comentario]
    FROM [dgmesnie].[Usuario] u
    LEFT JOIN RolActivo ra ON ra.[IdUsuario] = u.[IdUsuario]
    LEFT JOIN [dgmesnie].[Rol] r ON r.[RolId] = ra.[RolId]
    LEFT JOIN [dgmesnie].[Mercado] m ON m.[MercadoId] = ra.[MercadoId]
    OUTER APPLY
    (
        SELECT TOP (1)
            se.[Activa],
            se.[FechaInicio],
            se.[UltimaActividad]
        FROM [dgmesnie].[Sesion] se
        WHERE se.[IdUsuario] = u.[IdUsuario]
        ORDER BY se.[Activa] DESC, se.[UltimaActividad] DESC
    ) s
    WHERE u.[IdUsuario] = @IdUsuario;
END
GO

/* =========================
   SESION
   ========================= */

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_RegistrarSesion]
    @IdUsuario INT,
    @SessionKey NVARCHAR(200),
    @FechaExpiracion DATETIME2(0),
    @Ip NVARCHAR(64) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @OrigenAcceso NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[Sesion]
    SET [Activa] = 0,
        [UltimaActividad] = SYSUTCDATETIME()
    WHERE [IdUsuario] = @IdUsuario
      AND [Activa] = 1
      AND [SessionKey] <> @SessionKey;

    IF EXISTS (SELECT 1 FROM [dgmesnie].[Sesion] WHERE [SessionKey] = @SessionKey)
    BEGIN
        UPDATE [dgmesnie].[Sesion]
        SET [IdUsuario] = @IdUsuario,
            [FechaInicio] = SYSUTCDATETIME(),
            [UltimaActividad] = SYSUTCDATETIME(),
            [FechaExpiracion] = @FechaExpiracion,
            [Activa] = 1,
            [Ip] = @Ip,
            [UserAgent] = @UserAgent,
            [OrigenAcceso] = @OrigenAcceso
        WHERE [SessionKey] = @SessionKey;

        SELECT TOP (1) [IdSesion]
        FROM [dgmesnie].[Sesion]
        WHERE [SessionKey] = @SessionKey;

        RETURN;
    END

    INSERT INTO [dgmesnie].[Sesion]
    (
        [IdUsuario], [SessionKey], [FechaInicio], [UltimaActividad], [FechaExpiracion],
        [Activa], [Ip], [UserAgent], [OrigenAcceso]
    )
    VALUES
    (
        @IdUsuario, @SessionKey, SYSUTCDATETIME(), SYSUTCDATETIME(), @FechaExpiracion,
        1, @Ip, @UserAgent, @OrigenAcceso
    );

    SELECT TOP (1) [IdSesion]
    FROM [dgmesnie].[Sesion]
    WHERE [SessionKey] = @SessionKey
    ORDER BY [FechaInicio] DESC;
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_CerrarSesion]
    @SessionKey NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[Sesion]
    SET [Activa] = 0,
        [UltimaActividad] = SYSUTCDATETIME()
    WHERE [SessionKey] = @SessionKey
      AND [Activa] = 1;
END
GO

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_ActualizarActividadSesion]
    @SessionKey NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dgmesnie].[Sesion]
    SET [UltimaActividad] = SYSUTCDATETIME()
    WHERE [SessionKey] = @SessionKey
      AND [Activa] = 1;
END
GO

/* =========================
   MENU AUTORIZADO
   ========================= */

CREATE OR ALTER PROCEDURE [dgmesnie].[sp_ObtenerSeccionesYModulosPorUsuario]
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RolesUsuarioActivos AS
    (
        SELECT DISTINCT
            ur.[RolId],
            ur.[MercadoId]
        FROM [dgmesnie].[UsuarioRol] ur
        WHERE ur.[IdUsuario] = @IdUsuario
          AND ur.[Vigente] = 1
    ),
    ModulosPermitidos AS
    (
        SELECT DISTINCT
            rm.[ModuloId]
        FROM [dgmesnie].[RolModulo] rm
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rm.[RolId]
           AND (rm.[MercadoId] IS NULL OR rm.[MercadoId] = rua.[MercadoId])
        WHERE rm.[Activa] = 1

        UNION

        SELECT DISTINCT
            v.[ModuloId]
        FROM [dgmesnie].[RolVista] rv
        INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = rv.[VistaId]
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rv.[RolId]
           AND (rv.[MercadoId] IS NULL OR rv.[MercadoId] = rua.[MercadoId])
        WHERE rv.[Activa] = 1
    ),
    VistasPermitidasBase AS
    (
        SELECT DISTINCT
            rv.[VistaId]
        FROM [dgmesnie].[RolVista] rv
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rv.[RolId]
           AND (rv.[MercadoId] IS NULL OR rv.[MercadoId] = rua.[MercadoId])
        WHERE rv.[Activa] = 1
    ),
    VistasPermitidas AS
    (
        SELECT vp.[VistaId]
        FROM VistasPermitidasBase vp
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dgmesnie].[UsuarioVistaOverride] uvo
            WHERE uvo.[IdUsuario] = @IdUsuario
              AND uvo.[VistaId] = vp.[VistaId]
              AND uvo.[Permitida] = 0
        )

        UNION

        SELECT uvo.[VistaId]
        FROM [dgmesnie].[UsuarioVistaOverride] uvo
        WHERE uvo.[IdUsuario] = @IdUsuario
          AND uvo.[Permitida] = 1
    )
    SELECT
        s.[SeccionId] AS [Id],
        s.[Titulo] AS [Titulo],
        s.[Articulos] AS [Articulos],
        s.[FundamentoLegal] AS [FundamentoLegal],
        s.[Descripcion] AS [Descripcion],
        s.[Ayuda] AS [Ayuda],
        s.[Objetivo] AS [Objetivo],
        s.[ResponsableNormativo] AS [ResponsableNormativo],
        s.[PublicoObjetivo] AS [PublicoObjetivo],
        s.[Activa] AS [SeccionActiva],
        s.[Orden] AS [Orden],
        m.[ModuloId] AS [ModuloId],
        m.[SeccionId] AS [SeccionId],
        m.[Title] AS [Title],
        m.[FundamentoLegalModulo] AS [FundamentoLegalModulo],
        NULL AS [Roles],
        NULL AS [NombresRoles],
        m.[Perfiles] AS [Perfiles],
        m.[Etapa] AS [Etapa],
        m.[JustificacionOrden] AS [JustificacionOrden],
        m.[AyudaContextual] AS [AyudaContextual],
        m.[Controller] AS [Controller],
        m.[Action] AS [Action],
        m.[Descripcion] AS [Desc],
        m.[Imagen] AS [Img],
        m.[BotonTexto] AS [Btn],
        m.[ElementosUI] AS [ElementosUI],
        m.[AyudaVista] AS [AyudaVista],
        m.[Orden] AS [Orden],
        m.[Activo] AS [ModuloActivo],
        m.[EsExterno] AS [EsExterno],
        v.[VistaId] AS [VistaId],
        v.[Titulo] AS [VistaTitle],
        v.[Controller] AS [VistaController],
        v.[Action] AS [VistaAction],
        v.[EsExterno] AS [EsExterno],
        v.[Orden] AS [VistaOrden],
        v.[Activa] AS [VistaActivo]
    FROM [dgmesnie].[Seccion] s
    INNER JOIN [dgmesnie].[Modulo] m
        ON m.[SeccionId] = s.[SeccionId]
       AND m.[Activo] = 1
    INNER JOIN ModulosPermitidos mp
        ON mp.[ModuloId] = m.[ModuloId]
    LEFT JOIN [dgmesnie].[Vista] v
        ON v.[ModuloId] = m.[ModuloId]
       AND v.[Activa] = 1
       AND EXISTS (
            SELECT 1
            FROM VistasPermitidas vp
            WHERE vp.[VistaId] = v.[VistaId]
       )
    WHERE s.[Activa] = 1
    ORDER BY s.[Orden], m.[Orden], v.[Orden];
END
GO