const sql = require('mssql');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: {
        encrypt: true,
        trustServerCertificate: false
    }
};

const migrationSql = `
-- Check if the module is already registered
IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'ProyectosPrivados' AND Action = 'SegundaConvocatoria')
BEGIN
    PRINT 'Registering Segunda Convocatoria module...';
    
    INSERT INTO dgmesnie.Modulo (
        SeccionId, Title, Controller, Action, Perfiles, Orden, Activo, EsExterno, Imagen, BotonTexto, Descripcion
    )
    VALUES (
        43, N'📋 Segunda Convocatoria', 'ProyectosPrivados', 'SegundaConvocatoria', '1,86,87,89', 12, 1, 0, 'proyecto.png', N'Ver Reporte', N'Reporte de la segunda convocatoria de proyectos particulares.'
    );
    
    DECLARE @newModuloId INT = SCOPE_IDENTITY();
    
    -- Map permissions to roles that have access to Section 43
    INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
    SELECT RolId, @newModuloId, NULL, 1
    FROM dgmesnie.RolSeccion
    WHERE SeccionId = 43 AND Activa = 1;
    
    PRINT 'Module registered successfully with ID: ' + CAST(@newModuloId AS VARCHAR);
END
ELSE
BEGIN
    PRINT 'Module is already registered. Updating Perfiles and metadata...';
    
    UPDATE dgmesnie.Modulo 
    SET Perfiles = '1,86,87,89',
        Title = N'📋 Segunda Convocatoria',
        Orden = 12,
        Activo = 1,
        EsExterno = 0,
        Imagen = 'proyecto.png',
        BotonTexto = N'Ver Reporte',
        Descripcion = N'Reporte de la segunda convocatoria de proyectos particulares.'
    WHERE Controller = 'ProyectosPrivados' AND Action = 'SegundaConvocatoria';
    
    PRINT 'Module updated successfully.';
END
`;

async function main() {
    try {
        await sql.connect(config);
        console.log("Connected to database successfully!");
        
        const result = await sql.query(migrationSql);
        console.log("Database update completed.");
    } catch (err) {
        console.error("Migration error:", err);
    } finally {
        await sql.close();
    }
}

main();
