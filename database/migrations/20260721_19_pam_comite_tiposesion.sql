-- ========================================================
-- Agrega el campo TipoSesion a la tabla ComiteSesion
-- ========================================================

IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE Name = N'TipoSesion' 
    AND Object_ID = Object_ID(N'dgmesnie.ComiteSesion')
)
BEGIN
    ALTER TABLE dgmesnie.ComiteSesion
    ADD TipoSesion NVARCHAR(50) NOT NULL DEFAULT 'Ordinaria';
    
    PRINT 'Columna TipoSesion agregada a dgmesnie.ComiteSesion';
END
ELSE
BEGIN
    PRINT 'La columna TipoSesion ya existe en dgmesnie.ComiteSesion';
END
GO
