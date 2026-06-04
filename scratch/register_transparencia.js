const fs = require('fs');
const path = require('path');
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
        const sqlFilePath = path.join(__dirname, '..', 'SQL_08_RegTransparencia.sql');
        let sqlContent = fs.readFileSync(sqlFilePath, 'utf8');
        
        // Remove GO statements, as the mssql library doesn't support them in direct query execution
        sqlContent = sqlContent.replace(/\bGO\b/gi, '');

        console.log("Connecting to SQL Server...");
        await sql.connect(config);
        console.log("Connected! Executing registration script...");

        const result = await sql.query(sqlContent);
        console.log("Script executed successfully!");
        
        // Query database again to confirm it is now registered
        const checkResult = await sql.query`SELECT * FROM dgmesnie.Modulo WHERE Controller = 'Transparencia'`;
        console.log("Module registration status in database:", checkResult.recordset);
        
    } catch (err) {
        console.error("Error executing script:", err);
    } finally {
        await sql.close();
    }
}

main();
