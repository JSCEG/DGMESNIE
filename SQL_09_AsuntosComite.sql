-- ========================================================
-- Script SQL para crear la tabla de sesiones del comité
-- y registrar el módulo "Comité Técnico CNE" con permisos.
-- ========================================================

-- 1. Crear tabla de sesiones del comité
IF OBJECT_ID('dgmesnie.ComiteSesion', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.ComiteSesion (
        SesionId INT IDENTITY(1,1) PRIMARY KEY,
        Titulo NVARCHAR(300) NOT NULL,
        Fecha DATE NOT NULL,
        Resumen NVARCHAR(MAX) NULL,
        PdfUrl NVARCHAR(1000) NULL,
        PptUrl NVARCHAR(1000) NULL,
        CanvaEmbedUrl NVARCHAR(1000) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreadoPor NVARCHAR(450) NULL,
        ActualizadoEn DATETIME2 NULL,
        ActualizadoPor NVARCHAR(450) NULL
    );
    PRINT 'Tabla dgmesnie.ComiteSesion creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla dgmesnie.ComiteSesion ya existe.';
END
GO

-- 2. Registrar el módulo bajo la Sección 43 (Seguimiento)
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'AsuntosComite' AND Action = 'Index')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno)
    VALUES (43, N'🤝 Comité Técnico CNE', N'AsuntosComite', N'Index', 6, 1, 0);

    DECLARE @newModuloId INT = SCOPE_IDENTITY();

    -- Habilitar el módulo para todos los roles que tienen acceso a la Sección 43
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
    SELECT RolId, @newModuloId, NULL, 1
    FROM dgmesnie.RolSeccion
    WHERE SeccionId = 43 AND Activa = 1;

    PRINT 'Módulo de Comité Técnico CNE registrado exitosamente con permisos asociados.';
END
ELSE
BEGIN
    PRINT 'El módulo de Comité Técnico CNE ya se encuentra registrado.';
END
GO
