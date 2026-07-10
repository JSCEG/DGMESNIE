// Inserta/actualiza un asunto de Transparencia + hitos desde un JSON.
// Uso: node insert_transparencia_asunto.js <ruta_asunto.json> [--dry-run]
// Si ya existe un asunto activo con el mismo FolioSolicitud y Titulo, actualiza en vez de duplicar.
const sql = require('mssql');
const fs = require('fs');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};

async function main() {
    const jsonPath = process.argv[2];
    const dryRun = process.argv.includes('--dry-run');
    if (!jsonPath) { console.error('Falta ruta del JSON'); process.exit(1); }

    const data = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
    const a = data.asunto;
    const hitos = data.hitos || [];

    console.log(`Asunto: ${a.Titulo} | Folio: ${a.FolioSolicitud} | Hitos: ${hitos.length}${dryRun ? ' [DRY RUN]' : ''}`);

    await sql.connect(config);

    // Folios relacionados ya en BD (mismo folio = se agrupan en la vista)
    const rel = await sql.query`
        SELECT AsuntoId, Titulo, FolioSolicitud FROM dgmesnie.TransparenciaAsunto
        WHERE Activo = 1 AND FolioSolicitud = ${a.FolioSolicitud}`;
    if (rel.recordset.length) {
        console.log('Asuntos existentes con mismo folio (se agruparán):');
        rel.recordset.forEach(r => console.log(`  - [${r.AsuntoId}] ${r.Titulo}`));
    }

    if (dryRun) { console.log('Dry run, sin cambios.'); process.exit(0); }

    const existing = rel.recordset.find(r => r.Titulo === a.Titulo);
    let asuntoId;

    if (existing) {
        asuntoId = existing.AsuntoId;
        await sql.query`
            UPDATE dgmesnie.TransparenciaAsunto SET
                Fecha = ${a.Fecha}, Estatus = ${a.Estatus},
                NumeroExpediente = ${a.NumeroExpediente}, Descripcion = ${a.Descripcion},
                SharePointUrl = ${a.SharePointUrl || null},
                AudioEmbedUrl = ${a.AudioEmbedUrl || null},
                InfografiaEmbedUrl = ${a.InfografiaEmbedUrl || null},
                PresentacionEmbedUrl = ${a.PresentacionEmbedUrl || null},
                ActualizadoEn = SYSUTCDATETIME(), ActualizadoPor = 'skill-transparencia'
            WHERE AsuntoId = ${asuntoId}`;
        await sql.query`DELETE FROM dgmesnie.TransparenciaHito WHERE AsuntoId = ${asuntoId}`;
        console.log(`Actualizado AsuntoId=${asuntoId}`);
    } else {
        const ins = await sql.query`
            INSERT INTO dgmesnie.TransparenciaAsunto
            (Titulo, Fecha, Estatus, FolioSolicitud, NumeroExpediente, Descripcion,
             SharePointUrl, AudioEmbedUrl, InfografiaEmbedUrl, PresentacionEmbedUrl,
             Activo, CreadoEn, CreadoPor)
            VALUES (${a.Titulo}, ${a.Fecha}, ${a.Estatus}, ${a.FolioSolicitud}, ${a.NumeroExpediente},
                    ${a.Descripcion}, ${a.SharePointUrl || null}, ${a.AudioEmbedUrl || null},
                    ${a.InfografiaEmbedUrl || null}, ${a.PresentacionEmbedUrl || null},
                    1, SYSUTCDATETIME(), 'skill-transparencia');
            SELECT SCOPE_IDENTITY() AS Id;`;
        asuntoId = ins.recordset[0].Id;
        console.log(`Insertado AsuntoId=${asuntoId}`);
    }

    let orden = 0;
    for (const h of hitos) {
        await sql.query`
            INSERT INTO dgmesnie.TransparenciaHito
            (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
            VALUES (${asuntoId}, ${h.Fecha}, ${h.Titulo}, ${h.Descripcion || null}, ${orden++}, 1, SYSUTCDATETIME(), 'skill-transparencia')`;
    }
    console.log(`${hitos.length} hitos insertados. Listo.`);
    process.exit(0);
}

main().catch(e => { console.error(e.message); process.exit(1); });
