-- ========================================================
-- Script SQL para registrar el módulo "Grupos de Interés"
-- y otorgar permisos automáticos a los roles autorizados.
-- ========================================================

-- 1. Insertar en la tabla de Módulos (Sección 43: Seguimiento)
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'GruposInteres' AND Action = 'Index')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno)
    VALUES (43, N'🏢 Grupos de Interés', N'GruposInteres', N'Index', 4, 1, 0);

    DECLARE @newModuloId INT = SCOPE_IDENTITY();

    -- 2. Habilitar el módulo para todos los roles que tienen acceso a la Sección 43
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
    SELECT RolId, @newModuloId, NULL, 1
    FROM dgmesnie.RolSeccion
    WHERE SeccionId = 43 AND Activa = 1;

    PRINT 'Módulo de Grupos de Interés registrado exitosamente con permisos asociados.';
END
ELSE
BEGIN
    PRINT 'El módulo de Grupos de Interés ya se encuentra registrado.';
END
GO
