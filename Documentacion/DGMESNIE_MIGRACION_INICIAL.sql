/*
    Migracion inicial dbo -> dgmesnie
    Requiere que el esquema base ya exista.
    Diseno: idempotente y conservando IDs legacy donde sea viable.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dgmesnie.Usuario', 'U') IS NULL
BEGIN
    THROW 50000, 'Primero ejecuta Documentacion/DGMESNIE_ESQUEMA_BASE.sql', 1;
END
GO

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Rol] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Mercado] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Seccion] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Modulo] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Vista] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Usuario] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Notificacion] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[ActividadLog] OFF;
END TRY
BEGIN CATCH
END CATCH;

BEGIN TRY
    SET IDENTITY_INSERT [dgmesnie].[Acceso] OFF;
END TRY
BEGIN CATCH
END CATCH;
GO

DECLARE @MigrarActividadLog BIT = 0;
DECLARE @MigrarAccesos BIT = 0;

BEGIN TRANSACTION;
BEGIN TRY

    /* =========================
       CATALOGOS
       ========================= */

    IF OBJECT_ID('dbo.Roles_SNIEr', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Rol] ON;

        INSERT INTO [dgmesnie].[Rol]
        (
            [RolId], [RolClave], [RolNombre], [RolComentario], [RolTipo], [RolGrupo],
            [RolNivelAcceso], [RolAmbito], [RolVigente], [RolFechaMod]
        )
        SELECT
            r.[Rol_ID],
            r.[Rol_Clave],
            r.[Rol_Nombre],
            r.[Rol_Comentario],
            r.[Rol_Tipo],
            r.[Rol_Grupo],
            r.[Rol_NivelAcceso],
            r.[Rol_Ambito],
            ISNULL(r.[Rol_Vigente], 1),
            ISNULL(CAST(r.[Rol_FechaMod] AS DATETIME2(0)), SYSUTCDATETIME())
        FROM [dbo].[Roles_SNIEr] r
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Rol] d
            WHERE d.[RolId] = r.[Rol_ID]
        );

        SET IDENTITY_INSERT [dgmesnie].[Rol] OFF;
    END
    ELSE IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Rol] ON;

        INSERT INTO [dgmesnie].[Rol]
        (
            [RolId], [RolClave], [RolNombre], [RolVigente], [RolFechaMod]
        )
        SELECT
            r.[Rol_ID],
            CONCAT('ROL_', r.[Rol_ID]),
            r.[Rol_Nombre],
            1,
            SYSUTCDATETIME()
        FROM [dbo].[Roles] r
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Rol] d
            WHERE d.[RolId] = r.[Rol_ID]
        );

        SET IDENTITY_INSERT [dgmesnie].[Rol] OFF;
    END

    IF OBJECT_ID('dbo.Mercados', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Mercado] ON;

        INSERT INTO [dgmesnie].[Mercado]
        (
            [MercadoId], [MercadoNombre], [MercadoVigente], [MercadoFechaMod]
        )
        SELECT
            m.[Mercado_ID],
            m.[Mercado_Nombre],
            1,
            SYSUTCDATETIME()
        FROM [dbo].[Mercados] m
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Mercado] d
            WHERE d.[MercadoId] = m.[Mercado_ID]
        );

        SET IDENTITY_INSERT [dgmesnie].[Mercado] OFF;
    END

    /* =========================
       NAVEGACION
       ========================= */

    IF OBJECT_ID('dbo.Secciones', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Seccion] ON;

        INSERT INTO [dgmesnie].[Seccion]
        (
            [SeccionId], [Titulo], [Articulos], [FundamentoLegal], [Descripcion], [Ayuda],
            [Objetivo], [ResponsableNormativo], [PublicoObjetivo], [Orden], [Activa]
        )
        SELECT
            s.[Id], s.[Titulo], s.[Articulos], s.[FundamentoLegal], s.[Descripcion], s.[Ayuda],
            s.[Objetivo], s.[ResponsableNormativo], s.[PublicoObjetivo], s.[Id], ISNULL(s.[Activo], 1)
        FROM [dbo].[Secciones] s
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Seccion] d
            WHERE d.[SeccionId] = s.[Id]
        );

        SET IDENTITY_INSERT [dgmesnie].[Seccion] OFF;
    END

    IF OBJECT_ID('dbo.Modulos', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Modulo] ON;

        INSERT INTO [dgmesnie].[Modulo]
        (
            [ModuloId], [SeccionId], [Title], [FundamentoLegalModulo], [Perfiles], [Etapa],
            [JustificacionOrden], [AyudaContextual], [Controller], [Action], [Descripcion],
            [Imagen], [BotonTexto], [ElementosUI], [AyudaVista], [Orden], [Activo], [EsExterno]
        )
        SELECT
            m.[Id],
            m.[SeccionId],
            m.[Title],
            m.[FundamentoLegalModulo],
            m.[Perfiles],
            m.[Etapa],
            m.[JustificacionOrden],
            m.[AyudaContextual],
            m.[Controller],
            m.[Action],
            m.[Desc],
            m.[Img],
            m.[Btn],
            m.[ElementosUI],
            m.[AyudaVista],
            ISNULL(m.[Orden], m.[Id]),
            ISNULL(m.[Activo], 1),
            0
        FROM [dbo].[Modulos] m
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Modulo] d
            WHERE d.[ModuloId] = m.[Id]
        );

        SET IDENTITY_INSERT [dgmesnie].[Modulo] OFF;
    END

    IF OBJECT_ID('dbo.Vistas', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Vista] ON;

        INSERT INTO [dgmesnie].[Vista]
        (
            [VistaId], [ModuloId], [Titulo], [Perfiles], [Controller], [Action], [Orden], [Activa], [EsExterno]
        )
        SELECT
            v.[Id],
            v.[ModuloId],
            v.[Titulo],
            v.[Perfiles],
            v.[Controller],
            v.[Action],
            ISNULL(v.[Orden], v.[Id]),
            ISNULL(v.[Activo], 1),
            ISNULL(v.[EsExterno], 0)
        FROM [dbo].[Vistas] v
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Vista] d
            WHERE d.[VistaId] = v.[Id]
        );

        SET IDENTITY_INSERT [dgmesnie].[Vista] OFF;
    END

    /* =========================
       IDENTIDAD
       ========================= */

    IF OBJECT_ID('dbo.USUARIO', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Usuario] ON;

        INSERT INTO [dgmesnie].[Usuario]
        (
            [IdUsuario], [Correo], [ClaveHash], [Nombre], [RFC], [Cargo],
            [UnidadAdscripcion], [ClaveEmpleado], [Vigente], [FechaAlta], [FechaActualizacion]
        )
        SELECT
            u.[IdUsuario],
            u.[Correo],
            u.[Clave],
            u.[Nombre],
            u.[RFC],
            u.[Cargo],
            u.[Unidad_de_Adscripcion],
            u.[ClaveEmpleado],
            ISNULL(u.[Vigente], 1),
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        FROM [dbo].[USUARIO] u
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Usuario] d
            WHERE d.[IdUsuario] = u.[IdUsuario]
        );

        SET IDENTITY_INSERT [dgmesnie].[Usuario] OFF;
    END

    IF OBJECT_ID('dbo.Roles_Usuarios', 'U') IS NOT NULL
    BEGIN
        INSERT INTO [dgmesnie].[UsuarioRol]
        (
            [IdUsuario], [RolId], [MercadoId], [Vigente], [QuienRegistro], [FechaModificacion], [Comentarios]
        )
        SELECT
            ru.[IdUsuario],
            ru.[Rol_ID],
            ru.[Mercado_ID],
            ISNULL(ru.[RolUsuario_Vigente], 1),
            ru.[RolUsuario_QuienRegistro],
            ISNULL(CAST(ru.[RolUsuario_FechaMod] AS DATETIME2(0)), SYSUTCDATETIME()),
            ru.[RolUsuario_Comentarios]
        FROM [dbo].[Roles_Usuarios] ru
        WHERE EXISTS (SELECT 1 FROM [dgmesnie].[Usuario] u WHERE u.[IdUsuario] = ru.[IdUsuario])
          AND EXISTS (SELECT 1 FROM [dgmesnie].[Rol] r WHERE r.[RolId] = ru.[Rol_ID])
          AND NOT EXISTS (
              SELECT 1
              FROM [dgmesnie].[UsuarioRol] d
              WHERE d.[IdUsuario] = ru.[IdUsuario]
                AND d.[RolId] = ru.[Rol_ID]
                AND ISNULL(d.[MercadoId], -1) = ISNULL(ru.[Mercado_ID], -1)
          );
    END

    /* =========================
       PERMISOS NORMALIZADOS
       ========================= */

    IF OBJECT_ID('dbo.Modulos', 'U') IS NOT NULL
    BEGIN
        INSERT INTO [dgmesnie].[RolModulo]
        (
            [RolId], [ModuloId], [MercadoId], [Activa]
        )
        SELECT DISTINCT
            TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT) AS RolId,
            m.[Id] AS ModuloId,
            NULL AS MercadoId,
            1 AS Activa
        FROM [dbo].[Modulos] m
        CROSS APPLY STRING_SPLIT(ISNULL(m.[Roles], ''), ',') splitRole
        WHERE LTRIM(RTRIM(splitRole.[value])) <> ''
          AND TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT) IS NOT NULL
          AND EXISTS (
              SELECT 1
              FROM [dgmesnie].[Rol] r
              WHERE r.[RolId] = TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT)
          )
          AND EXISTS (
              SELECT 1
              FROM [dgmesnie].[Modulo] dm
              WHERE dm.[ModuloId] = m.[Id]
          )
          AND NOT EXISTS (
              SELECT 1
              FROM [dgmesnie].[RolModulo] d
              WHERE d.[RolId] = TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT)
                AND d.[ModuloId] = m.[Id]
                AND d.[MercadoId] IS NULL
          );
    END

    IF OBJECT_ID('dbo.Vistas', 'U') IS NOT NULL
    BEGIN
        INSERT INTO [dgmesnie].[RolVista]
        (
            [RolId], [VistaId], [MercadoId], [Activa]
        )
        SELECT DISTINCT
            TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT) AS RolId,
            v.[Id] AS VistaId,
            NULL AS MercadoId,
            1 AS Activa
        FROM [dbo].[Vistas] v
        CROSS APPLY STRING_SPLIT(ISNULL(v.[Roles], ''), ',') splitRole
        WHERE LTRIM(RTRIM(splitRole.[value])) <> ''
          AND TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT) IS NOT NULL
          AND EXISTS (
              SELECT 1
              FROM [dgmesnie].[Rol] r
              WHERE r.[RolId] = TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT)
          )
          AND EXISTS (
              SELECT 1
              FROM [dgmesnie].[Vista] dv
              WHERE dv.[VistaId] = v.[Id]
          )
          AND NOT EXISTS (
              SELECT 1
              FROM [dgmesnie].[RolVista] d
              WHERE d.[RolId] = TRY_CAST(LTRIM(RTRIM(splitRole.[value])) AS INT)
                AND d.[VistaId] = v.[Id]
                AND d.[MercadoId] IS NULL
          );
    END

    /* =========================
       OPERACION COMPLEMENTARIA
       ========================= */

    IF OBJECT_ID('dbo.Recuperar_contrasena', 'U') IS NOT NULL
    BEGIN
        INSERT INTO [dgmesnie].[RecuperacionContrasena]
        (
            [IdUsuario], [Token], [FechaCreacion], [FechaExpiracion], [Usado], [FechaUso]
        )
        SELECT
            rc.[IdUsuario],
            rc.[Token],
            CAST(rc.[Fecha] AS DATETIME2(0)),
            DATEADD(HOUR, 24, CAST(rc.[Fecha] AS DATETIME2(0))),
            0,
            NULL
        FROM [dbo].[Recuperar_contrasena] rc
        WHERE EXISTS (SELECT 1 FROM [dgmesnie].[Usuario] u WHERE u.[IdUsuario] = rc.[IdUsuario])
          AND NOT EXISTS (
              SELECT 1
              FROM [dgmesnie].[RecuperacionContrasena] d
              WHERE d.[IdUsuario] = rc.[IdUsuario]
                AND d.[Token] = rc.[Token]
          );
    END

    IF OBJECT_ID('dbo.Notificaciones', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Notificacion] ON;

        INSERT INTO [dgmesnie].[Notificacion]
        (
            [IdNotificacion], [GuidNotificacion], [Titulo], [Mensaje], [FechaNotificacion],
            [Link], [IdUsuario], [RolId], [Visto], [FechaVisto], [Imagen], [Activo]
        )
        SELECT
            n.[ID],
            ISNULL(n.[ID_Notificacion], NEWID()),
            n.[Titulo_Notificacion],
            n.[Mensaje],
            CAST(n.[Fecha_Notificacion] AS DATETIME2(0)),
            n.[Link],
            CASE
                WHEN EXISTS
                (
                    SELECT 1
                    FROM [dgmesnie].[Usuario] u
                    WHERE u.[IdUsuario] = n.[ID_Usuario]
                ) THEN n.[ID_Usuario]
                ELSE NULL
            END,
            NULL,
            ISNULL(n.[Visto], 0),
            CAST(n.[Fecha_Visto] AS DATETIME2(0)),
            n.[Imagen],
            1
        FROM [dbo].[Notificaciones] n
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Notificacion] d
            WHERE d.[IdNotificacion] = n.[ID]
        );

        SET IDENTITY_INSERT [dgmesnie].[Notificacion] OFF;
    END

    IF @MigrarActividadLog = 1 AND OBJECT_ID('dbo.UserActivityLog', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[ActividadLog] ON;

        INSERT INTO [dgmesnie].[ActividadLog]
        (
            [IdActividad], [IdUsuario], [NombreUsuario], [Accion], [Controlador], [Pagina],
            [Tipo], [Elemento], [IdElemento], [Valor], [Timestamp], [AdditionalData]
        )
        SELECT
            l.[Id],
            CASE
                WHEN EXISTS
                (
                    SELECT 1
                    FROM [dgmesnie].[Usuario] u
                    WHERE u.[IdUsuario] = TRY_CAST(l.[UserId] AS INT)
                ) THEN TRY_CAST(l.[UserId] AS INT)
                ELSE NULL
            END,
            l.[UserName],
            l.[ActionName],
            l.[ControllerName],
            l.[PageName],
            l.[Tipo],
            l.[Elemento],
            l.[IdElemento],
            l.[Valor],
            CAST(l.[Timestamp] AS DATETIME2(0)),
            l.[AdditionalData]
        FROM [dbo].[UserActivityLog] l
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[ActividadLog] d
            WHERE d.[IdActividad] = l.[Id]
        );

        SET IDENTITY_INSERT [dgmesnie].[ActividadLog] OFF;
    END

    IF @MigrarAccesos = 1 AND OBJECT_ID('dbo.Accesos', 'U') IS NOT NULL
    BEGIN
        SET IDENTITY_INSERT [dgmesnie].[Acceso] ON;

        INSERT INTO [dgmesnie].[Acceso]
        (
            [IdAcceso], [IdUsuario], [Correo], [TipoAcceso], [FechaAcceso], [Ip], [Exitoso], [Observaciones]
        )
        SELECT
            a.[Id],
            CASE
                WHEN EXISTS
                (
                    SELECT 1
                    FROM [dgmesnie].[Usuario] du
                    WHERE du.[IdUsuario] = a.[IdUsuario]
                ) THEN a.[IdUsuario]
                ELSE NULL
            END,
            u.[Correo],
            a.[TipoAcceso],
            CAST(COALESCE(a.[FechaHoraLocal], a.[FechaHora]) AS DATETIME2(0)),
            a.[IP],
            1,
            NULL
        FROM [dbo].[Accesos] a
        LEFT JOIN [dbo].[USUARIO] u ON u.[IdUsuario] = a.[IdUsuario]
        WHERE NOT EXISTS (
            SELECT 1
            FROM [dgmesnie].[Acceso] d
            WHERE d.[IdAcceso] = a.[Id]
        );

        SET IDENTITY_INSERT [dgmesnie].[Acceso] OFF;
    END

    COMMIT TRANSACTION;
    PRINT 'Migracion inicial dbo -> dgmesnie completada.';
END TRY
BEGIN CATCH
    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Rol] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Mercado] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Seccion] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Modulo] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Vista] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Usuario] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Notificacion] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[ActividadLog] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [dgmesnie].[Acceso] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO