// Update dirigido AsuntoId 22 (5. Recurso de revisión PODECOBI GTO, exp. SENER2603242):
// el recurso concluyó (resolución 28/05 revocó + cumplimiento 08/06). Actualiza Estatus y Fecha
// y reemplaza los hitos con la cronología completa. NO toca Titulo, embeds, Canva ni SharePoint.
const sql = require('mssql');
const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};

const ASUNTO_ID = 22;
const FECHA = '2026-06-08';
const ESTATUS = 'Concluido';
const hitos = [
    { Fecha: '2026-02-06', Titulo: 'Recepción de la solicitud original', Descripcion: 'El solicitante presenta la solicitud folio 340026100011826 vía PNT, requiriendo toda la información técnica, presupuestal y de gestión del PODECOBI Puerta Logística del Bajío.' },
    { Fecha: '2026-03-05', Titulo: 'Respuesta original (inexistencia y orientación)', Descripcion: 'La SENER, a través de la DGMESNIE (oficio SPTE/DGMESNIE/222.003.2026) y la Subsecretaría de Electricidad, responde declarando la inexistencia de la información y orientando al solicitante a otras autoridades.' },
    { Fecha: '2026-03-06', Titulo: 'Interposición del recurso de revisión', Descripcion: 'El recurrente interpone el recurso SENER2603242, alegando falta de búsqueda exhaustiva y transversal, omisión de unidades competentes y orientación errónea.' },
    { Fecha: '2026-03-13', Titulo: 'Acuerdo de admisión del recurso', Descripcion: 'El órgano garante Transparencia para el Pueblo admite a trámite el recurso de revisión.' },
    { Fecha: '2026-03-27', Titulo: 'Alegatos del sujeto obligado', Descripcion: 'La DGMESNIE (oficio SPTE/DGMESNIE/222.005.2026) y la Subsecretaría de Electricidad rinden alegatos solicitando confirmar la respuesta emitida.' },
    { Fecha: '2026-05-28', Titulo: 'Resolución: revocación', Descripcion: 'El órgano garante resuelve REVOCAR la respuesta de la SENER e instruir una nueva búsqueda exhaustiva, fundada y motivada, con sometimiento al Comité de Transparencia.' },
    { Fecha: '2026-06-08', Titulo: 'Cumplimiento de la resolución', Descripcion: 'La SENER cumple declarando incompetencia respecto de la información de otras autoridades (Gobierno de Guanajuato, Secretaría de Economía, CENACE, CFE, CENAGAS) e inexistencia de los registros de su competencia, sometiendo el asunto al Comité de Transparencia.' }
];

async function main() {
    await sql.connect(config);
    const tx = new sql.Transaction();
    await tx.begin();
    try {
        await new sql.Request(tx).query`
            UPDATE dgmesnie.TransparenciaAsunto
            SET Estatus = ${ESTATUS}, Fecha = ${FECHA}, ActualizadoEn = SYSUTCDATETIME(), ActualizadoPor = 'skill-transparencia'
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
        console.log(`OK: AsuntoId ${ASUNTO_ID} -> ${ESTATUS}, Fecha ${FECHA}, ${hitos.length} hitos.`);
    } catch (e) {
        await tx.rollback();
        console.error('ROLLBACK:', e.message);
        process.exit(1);
    }
    process.exit(0);
}
main().catch(e => { console.error(e.message); process.exit(1); });
