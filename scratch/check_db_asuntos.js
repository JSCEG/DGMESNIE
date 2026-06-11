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

async function main() {
    try {
        console.log("Connecting to database...");
        await sql.connect(config);
        console.log("Connected successfully.");

        const result = await sql.query`
            SELECT AsuntoId, Titulo, FolioSolicitud, NumeroExpediente, AudioEmbedUrl, InfografiaEmbedUrl, PresentacionEmbedUrl, Activo 
            FROM dgmesnie.TransparenciaAsunto
        `;
        console.log("Database Records:");
        console.log(JSON.stringify(result.recordset, null, 2));

    } catch (err) {
        console.error("Error:", err);
    } finally {
        await sql.close();
    }
}

main();
