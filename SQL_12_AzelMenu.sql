SET XACT_ABORT ON;
GO

IF EXISTS
(
    SELECT 1
    FROM dgmesnie.Modulo
    WHERE ModuloId = 180
      AND Controller = N'Atlas'
)
BEGIN
    UPDATE dgmesnie.Modulo
    SET Title = N'Atlas de Zonas con Energías Limpias (AZEL)',
        Action = N'AZEL_Publico',
        Descripcion = N'Consulta el inventario y el potencial territorial de los recursos renovables en México.',
        BotonTexto = N'Abrir atlas',
        Activo = 1
    WHERE ModuloId = 180
      AND Controller = N'Atlas';
END;
ELSE
BEGIN
    THROW 50001, 'No se encontró el módulo AZEL esperado (ModuloId 180).', 1;
END;
GO

SELECT ModuloId, Title, Controller, Action, Activo
FROM dgmesnie.Modulo
WHERE ModuloId = 180;
GO
