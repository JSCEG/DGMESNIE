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
        const result = await sql.query`
            SELECT s.SeccionId, s.Titulo AS Seccion, s.Orden AS SeccionOrden,
                   m.ModuloId, m.Title AS Modulo, m.Controller AS ModuloController, m.Action AS ModuloAction, m.EsExterno AS ModuloExterno,
                   v.VistaId, v.Titulo AS Vista, v.Controller, v.Action, v.EsExterno
            FROM dgmesnie.Seccion s
            JOIN dgmesnie.Modulo m ON m.SeccionId = s.SeccionId AND m.Activo = 1
            LEFT JOIN dgmesnie.Vista v ON v.ModuloId = m.ModuloId AND v.Activa = 1
            WHERE s.Activa = 1
            ORDER BY s.Orden, m.Orden, v.Orden
        `;
        console.log(JSON.stringify(result.recordset, null, 1));
        console.log(`-- Total filas: ${result.recordset.length}`);
    } catch (err) {
        console.error('Error:', err.message);
        process.exitCode = 1;
    } finally {
        await sql.close();
    }
}

main();
