const sql = require('mssql');
const fs = require('fs');
const path = require('path');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};

async function main() {
    const script = fs.readFileSync(path.join(__dirname, '..', 'SQL_10_RegVistasPvirce.sql'), 'utf8');
    const batches = script.split(/^GO\s*$/m).map(b => b.trim()).filter(Boolean);
    await sql.connect(config);
    for (const batch of batches) {
        if (batch.startsWith('--') && !batch.includes('IF NOT EXISTS')) continue;
        const result = await new sql.Request().query(batch);
        console.log('OK batch.');
    }
    const check = await sql.query`SELECT ModuloId, Title, Orden, Activo FROM dgmesnie.Modulo WHERE SeccionId = 43 AND Controller = 'ProyectosPrivados' ORDER BY Orden`;
    console.log(JSON.stringify(check.recordset, null, 1));
    await sql.close();
}

main().catch(e => { console.error('Error:', e.message); process.exitCode = 1; });
