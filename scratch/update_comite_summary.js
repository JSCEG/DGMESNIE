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
        console.log("Conectado a la base de datos.");

        const query = `
            UPDATE dgmesnie.ComiteSesion
            SET Resumen = N'En esta sesión del <strong>Comité Técnico</strong> se revisaron las <strong>agendas y compromisos pendientes</strong> relacionados con la <strong>planeación de la infraestructura</strong> del sector energético. Se presentaron los <strong>principales indicadores operativos</strong>, los <strong>avances de los proyectos estratégicos</strong> y la propuesta de actualización de los <strong>marcos de cumplimiento y normatividad</strong>.'
            WHERE Titulo = '11a Sesión Ordinaria';
        `;

        await sql.query(query);
        console.log("Registro actualizado con negritas (HTML) correctamente.");

    } catch (err) {
        console.error("Error al actualizar registro:", err);
    } finally {
        await sql.close();
    }
}

main();
