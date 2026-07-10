// Update dirigido AsuntoId 23 (6. PLADESE): corrige Fecha de respuesta (21 abr 2026)
// y reemplaza los 3 hitos dummy por la cronología real del oficio.
// NO toca Titulo, Estatus, embeds, SharePointUrl, PresentacionEmbedUrl ni Descripcion.
const sql = require('mssql');
const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};

const ASUNTO_ID = 23;
const FECHA_RESPUESTA = '2026-04-21';
const hitos = [
    { Fecha: '2026-03-20', Titulo: 'Recepción de la solicitud', Descripcion: 'Folio 340026100023726 recibido por la Subsecretaría de Planeación y Transición Energética (SPTE) a través de la Plataforma Nacional de Transparencia (PNT), modalidad de entrega por internet.' },
    { Fecha: '2026-03-23', Titulo: 'Turnado a la DGMESNIE', Descripcion: 'La SPTE turna la solicitud a la Dirección General de Metodologías y Estadísticas del SNIE (DGMESNIE) mediante correo electrónico para su atención.' },
    { Fecha: '2026-03-31', Titulo: 'Análisis y búsqueda exhaustiva', Descripcion: 'La DGMESNIE realiza una búsqueda exhaustiva, razonable y proporcional en sus archivos y elabora los proyectos de respuesta (versión limpia 31/03/2026; revisión V3 21/04/2026).' },
    { Fecha: '2026-04-21', Titulo: 'Emisión de respuesta', Descripcion: 'Respuesta por inexistencia de la documentación específica, al no existir obligación normativa de generarla o resguardarla, con orientación a la versión pública del Anexo A2 del PLADESE (DOF 17/10/2025). No se actualizan los supuestos para intervención del Comité de Transparencia (arts. 16, 17 y 141 LGTAIP).' },
    { Fecha: '2026-04-21', Titulo: 'Inicio de plazo de recurso de revisión', Descripcion: 'El solicitante cuenta con 15 días hábiles para interponer recurso de revisión (art. 144 LGTAIP) ante Transparencia para el Pueblo, vía PNT o la Unidad de Transparencia de la SENER.' }
];

async function main() {
    await sql.connect(config);
    const tx = new sql.Transaction();
    await tx.begin();
    try {
        await new sql.Request(tx).query`
            UPDATE dgmesnie.TransparenciaAsunto
            SET Fecha = ${FECHA_RESPUESTA}, ActualizadoEn = SYSUTCDATETIME(), ActualizadoPor = 'skill-transparencia'
            WHERE AsuntoId = ${ASUNTO_ID}`;
        await new sql.Request(tx).query`DELETE FROM dgmesnie.TransparenciaHito WHERE AsuntoId = ${ASUNTO_ID}`;
        let orden = 0;
        for (const h of hitos) {
            await new sql.Request(tx).query`
                INSERT INTO dgmesnie.TransparenciaHito
                (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
                VALUES (${ASUNTO_ID}, ${h.Fecha}, ${h.Titulo}, ${h.Descripcion}, ${orden++}, 1, SYSUTCDATETIME(), 'skill-transparencia')`;
        }
        await tx.commit();
        console.log(`OK: Fecha=${FECHA_RESPUESTA}, ${hitos.length} hitos reemplazados en AsuntoId=${ASUNTO_ID}.`);
    } catch (e) {
        await tx.rollback();
        console.error('ROLLBACK:', e.message);
        process.exit(1);
    }
    process.exit(0);
}
main().catch(e => { console.error(e.message); process.exit(1); });
