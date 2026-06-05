const sql = require('mssql');
const fs = require('fs');
const path = require('path');

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

async function main() {
    try {
        const force = process.argv.includes('--force');
        console.log("Connecting to database...");
        await sql.connect(config);
        console.log("Connected successfully.");

        // 1. Create/Alter Tables
        console.log("Creating tables and validating schema...");
        
        await sql.query`
            IF OBJECT_ID('dgmesnie.TransparenciaHito', 'U') IS NULL
            BEGIN
                CREATE TABLE dgmesnie.TransparenciaHito (
                    HitoId INT IDENTITY(1,1) PRIMARY KEY,
                    AsuntoId INT NOT NULL,
                    Fecha DATE NOT NULL,
                    Titulo NVARCHAR(300) NOT NULL,
                    Descripcion NVARCHAR(MAX) NULL,
                    Orden INT NOT NULL DEFAULT 0,
                    Activo BIT NOT NULL DEFAULT 1,
                    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    CreadoPor NVARCHAR(450) NULL
                );
            END
        `;

        await sql.query`
            IF OBJECT_ID('dgmesnie.TransparenciaAsunto', 'U') IS NULL
            BEGIN
                CREATE TABLE dgmesnie.TransparenciaAsunto (
                    AsuntoId INT IDENTITY(1,1) PRIMARY KEY,
                    Titulo NVARCHAR(300) NOT NULL,
                    Fecha DATE NOT NULL,
                    Estatus NVARCHAR(100) NOT NULL DEFAULT 'Pendiente',
                    FolioSolicitud NVARCHAR(100) NULL,
                    NumeroExpediente NVARCHAR(100) NULL,
                    Descripcion NVARCHAR(MAX) NULL,
                    SharePointUrl NVARCHAR(1000) NULL,
                    AudioEmbedUrl NVARCHAR(1000) NULL,
                    InfografiaEmbedUrl NVARCHAR(1000) NULL,
                    PresentacionEmbedUrl NVARCHAR(1000) NULL,
                    Activo BIT NOT NULL DEFAULT 1,
                    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    CreadoPor NVARCHAR(450) NULL,
                    ActualizadoEn DATETIME2 NULL,
                    ActualizadoPor NVARCHAR(450) NULL
                );
            END
        `;

        // Ensure columns exist on dgmesnie.TransparenciaAsunto if it was created previously
        await sql.query`
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dgmesnie.TransparenciaAsunto') AND name = 'FolioSolicitud')
            BEGIN
                ALTER TABLE dgmesnie.TransparenciaAsunto ADD FolioSolicitud NVARCHAR(100) NULL;
                PRINT 'Added column FolioSolicitud';
            END

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dgmesnie.TransparenciaAsunto') AND name = 'NumeroExpediente')
            BEGIN
                ALTER TABLE dgmesnie.TransparenciaAsunto ADD NumeroExpediente NVARCHAR(100) NULL;
                PRINT 'Added column NumeroExpediente';
            END
        `;

        // Add Foreign Key constraint if not exists
        await sql.query`
            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TransparenciaHito_Asunto')
            BEGIN
                ALTER TABLE dgmesnie.TransparenciaHito
                ADD CONSTRAINT FK_TransparenciaHito_Asunto
                FOREIGN KEY (AsuntoId) REFERENCES dgmesnie.TransparenciaAsunto(AsuntoId) ON DELETE CASCADE;
            END
        `;
        
        console.log("Tables structure validated.");

        // Check if data exists or if we should force reload
        const countRes = await sql.query`SELECT COUNT(*) AS count FROM dgmesnie.TransparenciaAsunto`;
        const count = countRes.recordset[0].count;
        console.log(`Current matters count in database: ${count}`);

        if (count === 0 || force) {
            if (force) {
                console.log("Force reload flag active. Deleting existing matters (cascades to hitos)...");
                await sql.query`DELETE FROM dgmesnie.TransparenciaAsunto`;
            }

            console.log("Loading data from JSON file...");
            const jsonPath = path.join(__dirname, '..', 'wwwroot', 'data', 'transparencia_asuntos.json');
            
            if (fs.existsSync(jsonPath)) {
                const fileContent = fs.readFileSync(jsonPath, 'utf8');
                const asuntos = JSON.parse(fileContent);
                console.log(`Found ${asuntos.length} asuntos in JSON to migrate.`);

                for (const asunto of asuntos) {
                    // Insert Asunto
                    const request = new sql.Request();
                    request.input('Titulo', sql.NVarChar(300), asunto.Titulo);
                    request.input('Fecha', sql.Date, asunto.Fecha);
                    request.input('Estatus', sql.NVarChar(100), asunto.Estatus);
                    request.input('FolioSolicitud', sql.NVarChar(100), asunto.FolioSolicitud || null);
                    request.input('NumeroExpediente', sql.NVarChar(100), asunto.NumeroExpediente || null);
                    request.input('Descripcion', sql.NVarChar(sql.MAX), asunto.Descripcion);
                    request.input('SharePointUrl', sql.NVarChar(1000), asunto.SharePointUrl);
                    request.input('AudioEmbedUrl', sql.NVarChar(1000), asunto.AudioEmbedUrl);
                    request.input('InfografiaEmbedUrl', sql.NVarChar(1000), asunto.InfografiaEmbedUrl);
                    request.input('PresentacionEmbedUrl', sql.NVarChar(1000), asunto.PresentacionEmbedUrl);
                    request.input('CreadoPor', sql.NVarChar(450), 'SistemaMigration');

                    const insertAsuntoQuery = `
                        INSERT INTO dgmesnie.TransparenciaAsunto 
                        (Titulo, Fecha, Estatus, FolioSolicitud, NumeroExpediente, Descripcion, SharePointUrl, AudioEmbedUrl, InfografiaEmbedUrl, PresentacionEmbedUrl, Activo, CreadoEn, CreadoPor)
                        VALUES 
                        (@Titulo, @Fecha, @Estatus, @FolioSolicitud, @NumeroExpediente, @Descripcion, @SharePointUrl, @AudioEmbedUrl, @InfografiaEmbedUrl, @PresentacionEmbedUrl, 1, SYSUTCDATETIME(), @CreadoPor);
                        SELECT SCOPE_IDENTITY() AS AsuntoId;
                    `;

                    const result = await request.query(insertAsuntoQuery);
                    const newAsuntoId = result.recordset[0].AsuntoId;
                    console.log(`Inserted asunto "${asunto.Titulo}" with DB ID: ${newAsuntoId}`);

                    // Insert timeline events (hitos)
                    if (asunto.Timeline && asunto.Timeline.length > 0) {
                        for (let index = 0; index < asunto.Timeline.length; index++) {
                            const hito = asunto.Timeline[index];
                            const hitoRequest = new sql.Request();
                            hitoRequest.input('AsuntoId', sql.Int, newAsuntoId);
                            hitoRequest.input('Fecha', sql.Date, hito.Fecha);
                            hitoRequest.input('Titulo', sql.NVarChar(300), hito.Titulo);
                            hitoRequest.input('Descripcion', sql.NVarChar(sql.MAX), hito.Descripcion);
                            hitoRequest.input('Orden', sql.Int, index);

                            const insertHitoQuery = `
                                INSERT INTO dgmesnie.TransparenciaHito 
                                (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
                                VALUES 
                                (@AsuntoId, @Fecha, @Titulo, @Descripcion, @Orden, 1, SYSUTCDATETIME(), 'SistemaMigration');
                            `;
                            await hitoRequest.query(insertHitoQuery);
                        }
                        console.log(`  Inserted ${asunto.Timeline.length} hitos.`);
                    }
                }
                console.log("Migration complete!");
            } else {
                console.log(`JSON file not found at: ${jsonPath}`);
            }
        } else {
            console.log("Database already has records. Run with '--force' to clean and reload.");
        }

    } catch (err) {
        console.error("An error occurred during database migration:", err);
    } finally {
        await sql.close();
    }
}

main();
