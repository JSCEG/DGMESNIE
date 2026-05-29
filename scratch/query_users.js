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
        console.log("Connected successfully!");

        const users = await sql.query`
            SELECT IdUsuario, Nombre, Correo, Cargo, Vigente
            FROM [dgmesnie].[Usuario]
        `;
        console.log("\n=== USUARIOS ===");
        console.table(users.recordset);

        const roles = await sql.query`
            SELECT RolId, RolClave, RolNombre, RolComentario, RolVigente
            FROM [dgmesnie].[Rol]
        `;
        console.log("\n=== ROLES ===");
        console.table(roles.recordset);

        const userRoles = await sql.query`
            SELECT ur.IdUsuario, u.Nombre as UsuarioNombre, ur.RolId, r.RolNombre
            FROM [dgmesnie].[UsuarioRol] ur
            JOIN [dgmesnie].[Usuario] u ON u.IdUsuario = ur.IdUsuario
            JOIN [dgmesnie].[Rol] r ON r.RolId = ur.RolId
            WHERE ur.Vigente = 1
        `;
        console.log("\n=== ROLES ASIGNADOS ===");
        console.table(userRoles.recordset);

    } catch (err) {
        console.error(err);
    } finally {
        await sql.close();
    }
}

main();
