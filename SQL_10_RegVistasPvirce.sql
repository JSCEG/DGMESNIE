-- ========================================================
-- Registro de 3 módulos PVIRCE en Sección 43 (Seguimientos)
-- Energía Limpia, Producción Energética Anual y
-- Evolución Prevalencia (Generación GWh).
-- Visibilidad: Rol 1 (el controlador además restringe a
-- Javier, Claudia, Raúl y Nahúm).
-- ========================================================

IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'ProyectosPrivados' AND Action = 'EnergiaLimpia')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno, Descripcion)
    VALUES (43, N'🌱 Energía Limpia', N'ProyectosPrivados', N'EnergiaLimpia', 7, 1, 0, N'Reporte de energía limpia del sector energético nacional.');

    DECLARE @m1 INT = SCOPE_IDENTITY();
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa) VALUES (1, @m1, NULL, 1);
    PRINT 'Módulo Energía Limpia registrado.';
END
ELSE
    PRINT 'Módulo Energía Limpia ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'ProyectosPrivados' AND Action = 'ProduccionEnergetica')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno, Descripcion)
    VALUES (43, N'⚡ Producción Energética Anual', N'ProyectosPrivados', N'ProduccionEnergetica', 8, 1, 0, N'Reporte de la producción energética anual.');

    DECLARE @m2 INT = SCOPE_IDENTITY();
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa) VALUES (1, @m2, NULL, 1);
    PRINT 'Módulo Producción Energética Anual registrado.';
END
ELSE
    PRINT 'Módulo Producción Energética Anual ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'ProyectosPrivados' AND Action = 'EvolucionPrevalencia')
BEGIN
    INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno, Descripcion)
    VALUES (43, N'📈 Evolución Prevalencia', N'ProyectosPrivados', N'EvolucionPrevalencia', 9, 1, 0, N'Reporte de evolución y prevalencia de la generación eléctrica (GWh).');

    DECLARE @m3 INT = SCOPE_IDENTITY();
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa) VALUES (1, @m3, NULL, 1);
    PRINT 'Módulo Evolución Prevalencia registrado.';
END
ELSE
    PRINT 'Módulo Evolución Prevalencia ya existe.';
GO
