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
        console.log("Connected to database successfully!");

        // Query columns for all tables in schema 'dgmesnie'
        const columnsRes = await sql.query`
            SELECT 
                t.TABLE_NAME, 
                c.COLUMN_NAME, 
                c.DATA_TYPE, 
                c.CHARACTER_MAXIMUM_LENGTH, 
                c.IS_NULLABLE
            FROM INFORMATION_SCHEMA.TABLES t
            JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_SCHEMA = 'dgmesnie'
            ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION
        `;

        // Group by table
        const tables = {};
        for (const col of columnsRes.recordset) {
            if (!tables[col.TABLE_NAME]) {
                tables[col.TABLE_NAME] = [];
            }
            tables[col.TABLE_NAME].push({
                name: col.COLUMN_NAME,
                type: col.DATA_TYPE,
                length: col.CHARACTER_MAXIMUM_LENGTH,
                nullable: col.IS_NULLABLE
            });
        }

        console.log("--- FULL SCHEMA FOR 'dgmesnie' ---");
        for (const tableName in tables) {
            console.log(`\nTable: ${tableName}`);
            for (const col of tables[tableName]) {
                const lenStr = col.length ? `(${col.length})` : '';
                console.log(`  - ${col.name}: ${col.type}${lenStr} ${col.nullable === 'YES' ? 'NULL' : 'NOT NULL'}`);
            }
        }

    } catch (err) {
        console.error(err);
    } finally {
        await sql.close();
    }
}

main();
