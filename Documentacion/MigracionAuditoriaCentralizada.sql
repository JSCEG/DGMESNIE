-- Script para migrar de auditoría por campos a bitácora centralizada
-- Tabla: FuentesdeInformacion

-- 1. Eliminar las columnas de auditoría que ya no se usarán
BEGIN TRANSACTION;

-- Verificar si las columnas existen antes de eliminarlas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'FuentesdeInformacion' AND COLUMN_NAME = 'UsuarioAccion')
BEGIN
    ALTER TABLE [dbo].[FuentesdeInformacion] DROP COLUMN [UsuarioAccion];
    PRINT 'Columna UsuarioAccion eliminada';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'FuentesdeInformacion' AND COLUMN_NAME = 'TipoAccion')
BEGIN
    ALTER TABLE [dbo].[FuentesdeInformacion] DROP COLUMN [TipoAccion];
    PRINT 'Columna TipoAccion eliminada';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'FuentesdeInformacion' AND COLUMN_NAME = 'FechaAccion')
BEGIN
    ALTER TABLE [dbo].[FuentesdeInformacion] DROP COLUMN [FechaAccion];
    PRINT 'Columna FechaAccion eliminada';
END

COMMIT TRANSACTION;

-- 2. Verificar la estructura final de la tabla
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FuentesdeInformacion'
ORDER BY ORDINAL_POSITION;

PRINT 'Migración completada: Las columnas de auditoría han sido eliminadas.';
PRINT 'La auditoría ahora se maneja centralmente a través de la tabla UserActivityLog.';
