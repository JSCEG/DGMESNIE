-- ========================================================
-- Script SQL para registrar el módulo "Transparencia"
-- y otorgar permisos automáticos a los roles autorizados.
-- ========================================================

DECLARE @SeccionId INT = NULL;

-- 1. Intentar buscar una sección existente que contenga "Datos Públicos"
SELECT TOP 1 @SeccionId = SeccionId 
FROM dgmesnie.Seccion 
WHERE Titulo LIKE N'%Públicos%' OR Titulo LIKE N'%Transparencia%';

-- Si no existe, usar la sección 43 (Seguimiento) como respaldo
IF @SeccionId IS NULL
BEGIN
    SET @SeccionId = 43;
END

-- 2. Insertar en la tabla de Módulos
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'Transparencia' AND Action = 'Index')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno)
    VALUES (@SeccionId, N'🌐 Repositorio de Transparencia', N'Transparencia', N'Index', 5, 1, 0);

    DECLARE @newModuloId INT = SCOPE_IDENTITY();

    -- 3. Habilitar el módulo para todos los roles que tienen acceso a otro módulo de la misma sección (ej. ModuloId = 8)
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
    SELECT RolId, @newModuloId, NULL, 1
    FROM dgmesnie.RolModulo
    WHERE ModuloId = 8 AND Activa = 1;

    PRINT 'Módulo de Repositorio de Transparencia registrado exitosamente en la Sección ID: ' + CAST(@SeccionId AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'El módulo de Repositorio de Transparencia ya se encuentra registrado.';
END
GO
