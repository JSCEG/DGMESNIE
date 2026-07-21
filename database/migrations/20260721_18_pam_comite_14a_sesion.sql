-- ========================================================
-- Registra o actualiza la 14ª Sesión Ordinaria del Comité Técnico CNE
-- con la presentación HTML interactiva embebida en iframe.
-- ========================================================

IF EXISTS (SELECT 1 FROM dgmesnie.ComiteSesion WHERE Titulo LIKE '%14%Ses%n Ordinaria%')
BEGIN
    UPDATE dgmesnie.ComiteSesion
    SET Titulo = N'14ª Sesión Ordinaria del Comité Técnico CNE',
        CanvaEmbedUrl = N'/Presentaciones/14a%20Sesion%20Ordinaria%20CT-CNE.html',
        Resumen = ISNULL(Resumen, N'14ª Sesión Ordinaria del Comité Técnico de la CNE. Presentación ejecutiva e insumos de la sesión.'),
        Activo = 1,
        ActualizadoEn = SYSUTCDATETIME()
    WHERE Titulo LIKE '%14%Ses%n Ordinaria%';
    PRINT '14ª Sesión Ordinaria actualizada con la presentación HTML.';
END
ELSE
BEGIN
    INSERT INTO dgmesnie.ComiteSesion 
    (Titulo, Fecha, Resumen, CanvaEmbedUrl, Activo, CreadoEn, CreadoPor)
    VALUES 
    (
        N'14ª Sesión Ordinaria del Comité Técnico CNE', 
        CAST(SYSUTCDATETIME() AS DATE), 
        N'14ª Sesión Ordinaria del Comité Técnico de la CNE. Presentación ejecutiva interactiva y documentos de apoyo.',
        N'/Presentaciones/14a%20Sesion%20Ordinaria%20CT-CNE.html',
        1, 
        SYSUTCDATETIME(), 
        N'Sistema'
    );
    PRINT '14ª Sesión Ordinaria registrada con la presentación HTML.';
END
GO
