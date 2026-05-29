-- =====================================================================
-- SCRIPT DE LIMPIEZA TOTAL (TRUNCATE/RESET) PARA EL ESQUEMA CORE Y STAGING
-- =====================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;


-- 1. Deshabilitar temporalmente todas las restricciones de llaves foráneas para evitar errores
DECLARE @SqlDisable NVARCHAR(MAX) = N'';
SELECT @SqlDisable += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ALL; '
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('core', 'staging');

EXEC sp_executesql @SqlDisable;

-- 2. Limpiar datos de las tablas de core y staging
DELETE FROM core.HistorialProyecto;
DELETE FROM core.ProyectoBitacoraCarga;
DELETE FROM core.ProyectoIdentificador;
DELETE FROM core.ProyectoActor;
DELETE FROM core.ProyectoUbicacion;
DELETE FROM core.ProyectoCoordenada;
DELETE FROM core.ProyectoGeometria;
DELETE FROM core.ProyectoDatosTecnicos;
DELETE FROM core.ProyectoDatosFinancieros;
DELETE FROM core.ProyectoTramite;
DELETE FROM core.ProyectoHito;
DELETE FROM core.BitacoraProyecto;
DELETE FROM core.AccionSeguimiento;
DELETE FROM core.Proyecto;
DELETE FROM core.Actor;
DELETE FROM core.GrupoInteresEconomico;
DELETE FROM core.CatTecnologia;
DELETE FROM staging.ExcelRaw_VUPEWorksheet;

-- 3. Reiniciar los contadores IDENTITY (Auto-incrementables) de forma dinámica
DECLARE @TableName NVARCHAR(256), @SchemaName NVARCHAR(256), @Sql NVARCHAR(MAX);
DECLARE db_cursor CURSOR FOR 
SELECT s.name, t.name
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.identity_columns c ON t.object_id = c.object_id
WHERE s.name IN ('core', 'staging');

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @SchemaName, @TableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = 'DBCC CHECKIDENT (''' + @SchemaName + '.' + @TableName + ''', RESEED, 0);';
    BEGIN TRY
        EXEC sp_executesql @Sql;
    END TRY
    BEGIN CATCH
        -- Ignorar error
    END CATCH
    FETCH NEXT FROM db_cursor INTO @SchemaName, @TableName;
END
CLOSE db_cursor;
DEALLOCATE db_cursor;

-- 4. Volver a habilitar todas las restricciones de llaves foráneas y validar integridad
DECLARE @SqlEnable NVARCHAR(MAX) = N'';
SELECT @SqlEnable += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' WITH CHECK CHECK CONSTRAINT ALL; '
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('core', 'staging');

EXEC sp_executesql @SqlEnable;

PRINT 'Limpieza de base de datos canónica completada exitosamente.';
