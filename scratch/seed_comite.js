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
        console.log("Conectando a la base de datos...");
        await sql.connect(config);
        console.log("Conexión exitosa.");

        // 1. Crear la tabla si no existe
        console.log("Creando tabla dgmesnie.ComiteSesion si no existe...");
        await sql.query`
            IF OBJECT_ID('dgmesnie.ComiteSesion', 'U') IS NULL
            BEGIN
                CREATE TABLE dgmesnie.ComiteSesion (
                    SesionId INT IDENTITY(1,1) PRIMARY KEY,
                    Titulo NVARCHAR(300) NOT NULL,
                    Fecha DATE NOT NULL,
                    Resumen NVARCHAR(MAX) NULL,
                    PdfUrl NVARCHAR(1000) NULL,
                    PptUrl NVARCHAR(1000) NULL,
                    CanvaEmbedUrl NVARCHAR(1000) NULL,
                    Activo BIT NOT NULL DEFAULT 1,
                    CreadoEn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    CreadoPor NVARCHAR(450) NULL,
                    ActualizadoEn DATETIME2 NULL,
                    ActualizadoPor NVARCHAR(450) NULL
                );
                PRINT 'Tabla dgmesnie.ComiteSesion creada.';
            END
            ELSE
            BEGIN
                PRINT 'La tabla dgmesnie.ComiteSesion ya existe.';
            END
        `;

        // 2. Registrar el módulo bajo la Sección 43 (Seguimiento) si no existe
        console.log("Registrando módulo Comité Técnico CNE...");
        const resultModulo = await sql.query`
            IF NOT EXISTS (SELECT 1 FROM dgmesnie.Modulo WHERE Controller = 'AsuntosComite' AND Action = 'Index')
            BEGIN
                INSERT INTO dgmesnie.Modulo (SeccionId, Title, Controller, Action, Orden, Activo, EsExterno)
                VALUES (43, N'🤝 Comité Técnico CNE', N'AsuntosComite', N'Index', 6, 1, 0);

                DECLARE @newModuloId INT = SCOPE_IDENTITY();

                -- Habilitar el módulo para todos los roles que tienen acceso a la Sección 43
                INSERT INTO dgmesnie.RolModulo (RolId, ModuloId, MercadoId, Activa)
                SELECT RolId, @newModuloId, NULL, 1
                FROM dgmesnie.RolSeccion
                WHERE SeccionId = 43 AND Activa = 1;

                SELECT @newModuloId AS ModuloId;
            END
            ELSE
            BEGIN
                SELECT ModuloId FROM dgmesnie.Modulo WHERE Controller = 'AsuntosComite' AND Action = 'Index';
            END
        `;
        const moduloId = resultModulo.recordset[0]?.ModuloId;
        console.log(`Módulo registrado/existente con ID: ${moduloId}`);

        // 3. Insertar la 11a Sesión como registro inicial si no existe
        console.log("Insertando sesión inicial (11a Sesión Ordinaria)...");
        const checkSesion = await sql.query`SELECT 1 FROM dgmesnie.ComiteSesion WHERE Titulo = '11a Sesión Ordinaria'`;
        
        if (checkSesion.recordset.length === 0) {
            const titulo = '11a Sesión Ordinaria';
            const fecha = '2026-06-09';
            const resumen = 'En esta sesión del Comité Técnico se revisaron las agendas y compromisos pendientes relacionados con la planeación de la infraestructura del sector energético. Se presentaron los principales indicadores operativos, los avances de los proyectos estratégicos y la propuesta de actualización de los marcos de cumplimiento y normatividad.';
            const pdfUrl = 'https://cdn.sassoapps.com/dgmesnie/asuntos_comite/20260609_11a%20Sesi%C3%B3n%20Ordinaria_Resumen%20de%20asuntos_V2.pdf';
            const pptUrl = 'https://cdn.sassoapps.com/dgmesnie/asuntos_comite/20260609_11a%20Sesi%C3%B3n%20Ordinaria_Resumen%20de%20asuntos_V2.pptx';
            const canvaUrl = 'https://www.canva.com/design/DAFv5d2Wf1k/view?embed'; // Presentación Canva placeholder

            const request = new sql.Request();
            request.input('Titulo', sql.NVarChar(300), titulo);
            request.input('Fecha', sql.Date, fecha);
            request.input('Resumen', sql.NVarChar(sql.MAX), resumen);
            request.input('PdfUrl', sql.NVarChar(1000), pdfUrl);
            request.input('PptUrl', sql.NVarChar(1000), pptUrl);
            request.input('CanvaUrl', sql.NVarChar(1000), canvaUrl);
            
            await request.query`
                INSERT INTO dgmesnie.ComiteSesion (Titulo, Fecha, Resumen, PdfUrl, PptUrl, CanvaEmbedUrl, Activo, CreadoEn, CreadoPor)
                VALUES (@Titulo, @Fecha, @Resumen, @PdfUrl, @PptUrl, @CanvaUrl, 1, SYSUTCDATETIME(), 'SistemaMigration');
            `;
            console.log("Sesión inicial insertada correctamente.");
        } else {
            console.log("La sesión inicial ya existe en la base de datos.");
        }

        console.log("Seeding completado exitosamente.");

    } catch (err) {
        console.error("Error al ejecutar el seeding:", err);
    } finally {
        await sql.close();
    }
}

main();
