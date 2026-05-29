const sql = require('mssql');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: {
        encrypt: true,
        transform: true,
        encrypt: true,
        trustServerCertificate: false
    }
};

async function main() {
    try {
        await sql.connect(config);
        console.log("Connected successfully!");

        const modules = await sql.query`
            SELECT ModuloId, SeccionId, Title, Controller, Action, Perfiles, Activo
            FROM [dgmesnie].[Modulo]
            WHERE Activo = 1
        `;
        console.log("\n=== MODULOS ACTIVOS ===");
        console.table(modules.recordset);

        const vistas = await sql.query`
            SELECT VistaId, ModuloId, Titulo, Controller, Action, Perfiles, Activa
            FROM [dgmesnie].[Vista]
            WHERE Activa = 1
        `;
        console.log("\n=== VISTAS ACTIVAS ===");
        console.table(vistas.recordset);

    } catch (err) {
        console.error(err);
    } finally {
        await sql.close();
    }
}

main();
