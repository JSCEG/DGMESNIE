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
        await sql.connect(config);
        const col = await sql.query`
            SELECT COLUMNPROPERTY(OBJECT_ID('dgmesnie.Modulo'), 'ModuloId', 'IsIdentity') AS IsIdentity
        `;
        console.log("Is ModuloId Identity?", col.recordset[0].IsIdentity);
    } catch (err) {
        console.error(err);
    } finally {
        await sql.close();
    }
}

main();
