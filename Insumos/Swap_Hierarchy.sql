-- ============================================================
-- SCRIPT DE MIGRACIÓN: INVERSIÓN DE JERARQUÍA
-- De: Temas (Padre) -> Actividades (Hijo)
-- A: Actividades (Padre) -> Temas (Hijo)
-- Esquema: dgmesnie
-- ============================================================

BEGIN TRANSACTION;
BEGIN TRY

    -- 1. Eliminar llaves foráneas antiguas
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Gestor_Actividades_Tema')
        ALTER TABLE [dgmesnie].[Gestor_Actividades] DROP CONSTRAINT [FK_Gestor_Actividades_Tema];
        
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Gestor_Actividades_Usuario')
        ALTER TABLE [dgmesnie].[Gestor_Actividades] DROP CONSTRAINT [FK_Gestor_Actividades_Usuario];

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Gestor_Temas_Usuario')
        ALTER TABLE [dgmesnie].[Gestor_Temas] DROP CONSTRAINT [FK_Gestor_Temas_Usuario];

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Corresponsables_Tema')
        ALTER TABLE [dgmesnie].[Gestor_Corresponsables] DROP CONSTRAINT [FK_Corresponsables_Tema];

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Corresponsables_Actividad')
        ALTER TABLE [dgmesnie].[Gestor_Corresponsables] DROP CONSTRAINT [FK_Corresponsables_Actividad];

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Corresponsables_Usuario')
        ALTER TABLE [dgmesnie].[Gestor_Corresponsables] DROP CONSTRAINT [FK_Corresponsables_Usuario];

    -- 2. Eliminar índices asociados antiguas
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Actividades_TemaId' AND object_id = OBJECT_ID('[dgmesnie].[Gestor_Actividades]'))
        DROP INDEX [IX_Gestor_Actividades_TemaId] ON [dgmesnie].[Gestor_Actividades];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Actividades_Activo' AND object_id = OBJECT_ID('[dgmesnie].[Gestor_Actividades]'))
        DROP INDEX [IX_Gestor_Actividades_Activo] ON [dgmesnie].[Gestor_Actividades];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gestor_Temas_Activo' AND object_id = OBJECT_ID('[dgmesnie].[Gestor_Temas]'))
        DROP INDEX [IX_Gestor_Temas_Activo] ON [dgmesnie].[Gestor_Temas];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Corresponsables_Tema' AND object_id = OBJECT_ID('[dgmesnie].[Gestor_Corresponsables]'))
        DROP INDEX [IX_Corresponsables_Tema] ON [dgmesnie].[Gestor_Corresponsables];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Corresponsables_Actividad' AND object_id = OBJECT_ID('[dgmesnie].[Gestor_Corresponsables]'))
        DROP INDEX [IX_Corresponsables_Actividad] ON [dgmesnie].[Gestor_Corresponsables];

    -- 3. Renombrar las tablas principales
    EXEC sp_rename '[dgmesnie].[Gestor_Actividades]', 'Gestor_Temas_Temp';
    EXEC sp_rename '[dgmesnie].[Gestor_Temas]', 'Gestor_Actividades';
    EXEC sp_rename '[dgmesnie].[Gestor_Temas_Temp]', 'Gestor_Temas';

    -- 4. Renombrar columnas e índices en la nueva tabla de Actividades (ex-Temas, Padre)
    EXEC sp_rename '[dgmesnie].[Gestor_Actividades].[TemaId]', 'ActividadId', 'COLUMN';
    EXEC sp_rename '[dgmesnie].[Gestor_Actividades].[Tema]', 'Actividad', 'COLUMN';

    -- 5. Renombrar columnas en la nueva tabla de Temas (ex-Actividades, Hijo) usando un nombre temporal para evitar colisiones
    EXEC sp_rename '[dgmesnie].[Gestor_Temas].[TemaId]', 'ActividadId_Temp', 'COLUMN';
    EXEC sp_rename '[dgmesnie].[Gestor_Temas].[ActividadId]', 'TemaId', 'COLUMN';
    EXEC sp_rename '[dgmesnie].[Gestor_Temas].[ActividadId_Temp]', 'ActividadId', 'COLUMN';
    EXEC sp_rename '[dgmesnie].[Gestor_Temas].[Actividad]', 'Tema', 'COLUMN';

    -- 6. Renombrar restricciones de llave primaria secuencialmente para evitar colisiones
    EXEC sp_rename '[dgmesnie].[PK_Gestor_Actividades]', 'PK_Gestor_Temas_Temp', 'OBJECT';
    EXEC sp_rename '[dgmesnie].[PK_Gestor_Temas]', 'PK_Gestor_Actividades', 'OBJECT';
    EXEC sp_rename '[dgmesnie].[PK_Gestor_Temas_Temp]', 'PK_Gestor_Temas', 'OBJECT';

    -- 6.5 Intercambiar los IDs de corresponsables para alinearse con los nuevos roles de tablas
    -- (Los corresponsables del antiguo Tema ahora corresponden a la nueva Actividad y viceversa)
    UPDATE [dgmesnie].[Gestor_Corresponsables]
    SET TemaId = ActividadId, ActividadId = TemaId;

    -- 7. Reconstruir llaves foráneas e índices con la nueva lógica
    ALTER TABLE [dgmesnie].[Gestor_Actividades] ADD CONSTRAINT [FK_Gestor_Actividades_Usuario] FOREIGN KEY ([ResponsablePrincipalId])
        REFERENCES [dgmesnie].[Usuario]([IdUsuario]);

    ALTER TABLE [dgmesnie].[Gestor_Temas] ADD CONSTRAINT [FK_Gestor_Temas_Actividad] FOREIGN KEY ([ActividadId])
        REFERENCES [dgmesnie].[Gestor_Actividades]([ActividadId]);

    ALTER TABLE [dgmesnie].[Gestor_Temas] ADD CONSTRAINT [FK_Gestor_Temas_Usuario] FOREIGN KEY ([ResponsableId])
        REFERENCES [dgmesnie].[Usuario]([IdUsuario]);

    -- Llaves de corresponsabilidad
    ALTER TABLE [dgmesnie].[Gestor_Corresponsables] ADD CONSTRAINT [FK_Corresponsables_Tema] FOREIGN KEY ([TemaId])
        REFERENCES [dgmesnie].[Gestor_Temas]([TemaId]);

    ALTER TABLE [dgmesnie].[Gestor_Corresponsables] ADD CONSTRAINT [FK_Corresponsables_Actividad] FOREIGN KEY ([ActividadId])
        REFERENCES [dgmesnie].[Gestor_Actividades]([ActividadId]);

    ALTER TABLE [dgmesnie].[Gestor_Corresponsables] ADD CONSTRAINT [FK_Corresponsables_Usuario] FOREIGN KEY ([IdUsuario])
        REFERENCES [dgmesnie].[Usuario]([IdUsuario]);

    -- Índices
    CREATE INDEX [IX_Gestor_Actividades_Activo] ON [dgmesnie].[Gestor_Actividades]([Activo]);
    CREATE INDEX [IX_Gestor_Temas_ActividadId] ON [dgmesnie].[Gestor_Temas]([ActividadId]);
    CREATE INDEX [IX_Gestor_Temas_Activo] ON [dgmesnie].[Gestor_Temas]([Activo]);
    CREATE INDEX [IX_Corresponsables_Tema] ON [dgmesnie].[Gestor_Corresponsables]([TemaId]);
    CREATE INDEX [IX_Corresponsables_Actividad] ON [dgmesnie].[Gestor_Corresponsables]([ActividadId]);

    -- 8. Actualizar prefijos de las claves en las tablas para que los padres tengan 'A-' y los hijos tengan 'T-'
    UPDATE [dgmesnie].[Gestor_Actividades] SET Clave = REPLACE(Clave, 'T-', 'A-');
    UPDATE [dgmesnie].[Gestor_Temas] SET Clave = REPLACE(Clave, 'A-', 'T-');

    COMMIT TRANSACTION;
    PRINT '¡Inversión de jerarquía en base de datos completada con éxito!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO
