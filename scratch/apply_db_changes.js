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

const spSql = `
CREATE OR ALTER PROCEDURE [dgmesnie].[sp_ObtenerSeccionesYModulosPorUsuario]
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RolesUsuarioActivos AS
    (
        SELECT DISTINCT
            ur.[RolId],
            ur.[MercadoId]
        FROM [dgmesnie].[UsuarioRol] ur
        WHERE ur.[IdUsuario] = @IdUsuario
          AND ur.[Vigente] = 1
    ),
    ModulosPermitidos AS
    (
        SELECT DISTINCT
            rm.[ModuloId]
        FROM [dgmesnie].[RolModulo] rm
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rm.[RolId]
           AND (rm.[MercadoId] IS NULL OR rm.[MercadoId] = rua.[MercadoId])
        WHERE rm.[Activa] = 1

        UNION

        SELECT DISTINCT
            v.[ModuloId]
        FROM [dgmesnie].[RolVista] rv
        INNER JOIN [dgmesnie].[Vista] v ON v.[VistaId] = rv.[VistaId]
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rv.[RolId]
           AND (rv.[MercadoId] IS NULL OR rv.[MercadoId] = rua.[MercadoId])
        WHERE rv.[Activa] = 1
    ),
    VistasPermitidasBase AS
    (
        SELECT DISTINCT
            rv.[VistaId]
        FROM [dgmesnie].[RolVista] rv
        INNER JOIN RolesUsuarioActivos rua
            ON rua.[RolId] = rv.[RolId]
           AND (rv.[MercadoId] IS NULL OR rv.[MercadoId] = rua.[MercadoId])
        WHERE rv.[Activa] = 1
    ),
    VistasPermitidas AS
    (
        SELECT vp.[VistaId]
        FROM VistasPermitidasBase vp
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dgmesnie].[UsuarioVistaOverride] uvo
            WHERE uvo.[IdUsuario] = @IdUsuario
              AND uvo.[VistaId] = vp.[VistaId]
              AND uvo.[Permitida] = 0
        )

        UNION

        SELECT uvo.[VistaId]
        FROM [dgmesnie].[UsuarioVistaOverride] uvo
        WHERE uvo.[IdUsuario] = @IdUsuario
          AND uvo.[Permitida] = 1
    )
    SELECT
        s.[SeccionId] AS [Id],
        s.[Titulo] AS [Titulo],
        s.[Articulos] AS [Articulos],
        s.[FundamentoLegal] AS [FundamentoLegal],
        s.[Descripcion] AS [Descripcion],
        s.[Ayuda] AS [Ayuda],
        s.[Objetivo] AS [Objetivo],
        s.[ResponsableNormativo] AS [ResponsableNormativo],
        s.[PublicoObjetivo] AS [PublicoObjetivo],
        s.[Activa] AS [SeccionActiva],
        s.[Orden] AS [Orden],
        m.[ModuloId] AS [ModuloId],
        m.[SeccionId] AS [SeccionId],
        m.[Title] AS [Title],
        m.[FundamentoLegalModulo] AS [FundamentoLegalModulo],
        NULL AS [Roles],
        NULL AS [NombresRoles],
        m.[Perfiles] AS [Perfiles],
        m.[Etapa] AS [Etapa],
        m.[JustificacionOrden] AS [JustificacionOrden],
        m.[AyudaContextual] AS [AyudaContextual],
        m.[Controller] AS [Controller],
        m.[Action] AS [Action],
        m.[Descripcion] AS [Desc],
        m.[Imagen] AS [Img],
        m.[BotonTexto] AS [Btn],
        m.[ElementosUI] AS [ElementosUI],
        m.[AyudaVista] AS [AyudaVista],
        m.[Orden] AS [Orden],
        m.[Activo] AS [ModuloActivo],
        m.[EsExterno] AS [EsExterno],
        v.[VistaId] AS [VistaId],
        v.[Titulo] AS [VistaTitle],
        v.[Controller] AS [VistaController],
        v.[Action] AS [VistaAction],
        v.[EsExterno] AS [EsExterno],
        v.[Orden] AS [VistaOrden],
        v.[Activa] AS [VistaActivo]
    FROM [dgmesnie].[Seccion] s
    INNER JOIN [dgmesnie].[Modulo] m
        ON m.[SeccionId] = s.[SeccionId]
       AND m.[Activo] = 1
    INNER JOIN ModulosPermitidos mp
        ON mp.[ModuloId] = m.[ModuloId]
    LEFT JOIN [dgmesnie].[Vista] v
        ON v.[ModuloId] = m.[ModuloId]
       AND v.[Activa] = 1
       AND EXISTS (
            SELECT 1
            FROM VistasPermitidas vp
            WHERE vp.[VistaId] = v.[VistaId]
       )
    WHERE s.[Activa] = 1
      AND (
          m.[Perfiles] IS NULL 
          OR LTRIM(RTRIM(m.[Perfiles])) = '' 
          OR @IdUsuario = 1 
          OR @IdUsuario = 86
          OR CHARINDEX(',' + CAST(@IdUsuario AS VARCHAR) + ',', ',' + REPLACE(m.[Perfiles], ' ', '') + ',') > 0
          OR PATINDEX('%[0-9]%', LEFT(m.[Perfiles], 1)) = 0
      )
      AND (
          v.[VistaId] IS NULL
          OR v.[Perfiles] IS NULL 
          OR LTRIM(RTRIM(v.[Perfiles])) = '' 
          OR @IdUsuario = 1 
          OR @IdUsuario = 86
          OR CHARINDEX(',' + CAST(@IdUsuario AS VARCHAR) + ',', ',' + REPLACE(v.[Perfiles], ' ', '') + ',') > 0
          OR PATINDEX('%[0-9]%', LEFT(v.[Perfiles], 1)) = 0
      )
    ORDER BY s.[Orden], m.[Orden], v.[Orden];
END
`;

async function main() {
    try {
        await sql.connect(config);
        console.log("Connected successfully to DB!");

        // 1. Update stored procedure
        console.log("Updating sp_ObtenerSeccionesYModulosPorUsuario...");
        await sql.query(spSql);
        console.log("Stored procedure updated successfully!");

        // 2. Register PVIRCE module (PermisosPV/MenuPV) if not exists
        console.log("Registering PVIRCE module...");
        let pvirceModId = null;
        const checkPvirce = await sql.query`SELECT ModuloId FROM [dgmesnie].[Modulo] WHERE Controller = 'PermisosPV'`;
        if (checkPvirce.recordset.length === 0) {
            const insRes = await sql.query`
                INSERT INTO [dgmesnie].[Modulo] (SeccionId, Title, Controller, Action, Perfiles, Orden, Activo, EsExterno)
                VALUES (43, N'PVIRCE y Proyectos Mixtos', 'PermisosPV', 'MenuPV', '1,86', 10, 1, 0);
                SELECT SCOPE_IDENTITY() AS id;
            `;
            pvirceModId = insRes.recordset[0].id;
            console.log(`PVIRCE registered with ModuloId: ${pvirceModId}`);
        } else {
            pvirceModId = checkPvirce.recordset[0].ModuloId;
            console.log(`PVIRCE already exists with ModuloId: ${pvirceModId}`);
        }

        // 3. Register PlanMexico (Plan_Polos) module if not exists
        console.log("Registering PlanMexico (Polos de Desarrollo) module...");
        let planMexicoModId = null;
        const checkPlan = await sql.query`SELECT ModuloId FROM [dgmesnie].[Modulo] WHERE Controller = 'PlanMexico' AND Action = 'Plan_Polos'`;
        if (checkPlan.recordset.length === 0) {
            const insRes = await sql.query`
                INSERT INTO [dgmesnie].[Modulo] (SeccionId, Title, Controller, Action, Perfiles, Orden, Activo, EsExterno)
                VALUES (43, N'Polos de Desarrollo', 'PlanMexico', 'Plan_Polos', '1,86,87,91,94', 11, 1, 0);
                SELECT SCOPE_IDENTITY() AS id;
            `;
            planMexicoModId = insRes.recordset[0].id;
            console.log(`PlanMexico registered with ModuloId: ${planMexicoModId}`);
        } else {
            planMexicoModId = checkPlan.recordset[0].ModuloId;
            console.log(`PlanMexico already exists with ModuloId: ${planMexicoModId}`);
        }

        // Map roles to new modules if we just created them
        const roles = await sql.query`SELECT RolId FROM [dgmesnie].[Rol] WHERE RolVigente = 1`;
        for (const row of roles.recordset) {
            // Map PVIRCE
            const checkPvirceMap = await sql.query`SELECT 1 FROM [dgmesnie].[RolModulo] WHERE RolId = ${row.RolId} AND ModuloId = ${pvirceModId}`;
            if (checkPvirceMap.recordset.length === 0) {
                await sql.query`INSERT INTO [dgmesnie].[RolModulo] (RolId, ModuloId, Activa) VALUES (${row.RolId}, ${pvirceModId}, 1)`;
            }
            // Map PlanMexico
            const checkPlanMap = await sql.query`SELECT 1 FROM [dgmesnie].[RolModulo] WHERE RolId = ${row.RolId} AND ModuloId = ${planMexicoModId}`;
            if (checkPlanMap.recordset.length === 0) {
                await sql.query`INSERT INTO [dgmesnie].[RolModulo] (RolId, ModuloId, Activa) VALUES (${row.RolId}, ${planMexicoModId}, 1)`;
            }
        }
        console.log("Assigned roles to modules successfully.");

        // 4. Update Perfiles user ID list for existing modules
        console.log("Updating Perfiles list for existing modules...");
        
        // PAM (InformePormenorizado)
        await sql.query`UPDATE [dgmesnie].[Modulo] SET Perfiles = '1,86,87,89' WHERE Controller = 'InformePormenorizado'`;
        
        // Proyectos Privados
        await sql.query`UPDATE [dgmesnie].[Modulo] SET Perfiles = '1,86,87' WHERE Controller = 'ProyectosPrivados'`;
        
        // PODECOBIS
        await sql.query`UPDATE [dgmesnie].[Modulo] SET Perfiles = '1,86,87,91,94' WHERE Controller = 'PODECOBIS'`;

        // PVIRCE (In case it existed)
        await sql.query`UPDATE [dgmesnie].[Modulo] SET Perfiles = '1,86' WHERE Controller = 'PermisosPV'`;

        // PlanMexico Plan_Polos (In case it existed)
        await sql.query`UPDATE [dgmesnie].[Modulo] SET Perfiles = '1,86,87,91,94' WHERE Controller = 'PlanMexico' AND Action = 'Plan_Polos'`;

        console.log("All DB changes applied successfully!");
    } catch (err) {
        console.error(err);
    } finally {
        await sql.close();
    }
}
main();
