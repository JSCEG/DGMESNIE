const sql = require('mssql');

const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};

const asunto = {
    Titulo: "15. Recurso de revisión PODECOBI Huamantla",
    Fecha: "2026-06-25",
    Estatus: "En revisión",
    FolioSolicitud: "340026100031426",
    NumeroExpediente: "SENER2603244",
    Descripcion: "Recurso de revisión interpuesto en contra de la respuesta del folio 340026100031426 relativo al Polo de Desarrollo para el Bienestar (PODECOBI) Huamantla (Xicoténcatl II), en Tlaxcala.",
    SharePointUrl: "https://sener.sharepoint.com/sites/Transparencia/RecursoHuamantla",
    AudioEmbedUrl: "https://cdn.sassoapps.com/dgmesnie/trasnparencia/audios/audio_T15.m4a",
    InfografiaEmbedUrl: "https://cdn.sassoapps.com/dgmesnie/trasnparencia/infografias/infografia_T15.png",
    PresentacionEmbedUrl: "https://www.canva.com/design/DAHMqT15CAN/view?embed"
};

const hitos = [
    { Fecha: '2026-05-04', Titulo: 'Interposición de Recurso', Descripcion: 'El particular interpone el recurso de revisión inconforme con la declaración de incompetencia sobre convenios de delimitación del polo.' },
    { Fecha: '2026-05-18', Titulo: 'Admisión del Recurso', Descripcion: 'El órgano garante admite a trámite el recurso de revisión SENER2603244.' },
    { Fecha: '2026-06-01', Titulo: 'Rendición de Alegatos', Descripcion: 'La DGMESNIE rinde manifestaciones técnicas indicando la competencia local y del Fideicomiso estatal.' },
    { Fecha: '2026-06-25', Titulo: 'En desahogo', Descripcion: 'El expediente se encuentra en estudio y resolución del pleno.' }
];

async function main() {
    console.log(`Connecting to SQL Database...`);
    await sql.connect(config);
    console.log(`Connected. Checking if Case 15 already exists...`);

    const check = await sql.query`
        SELECT AsuntoId FROM dgmesnie.TransparenciaAsunto
        WHERE Activo = 1 AND Titulo = ${asunto.Titulo} AND FolioSolicitud = ${asunto.FolioSolicitud}`;

    let asuntoId;
    if (check.recordset.length > 0) {
        asuntoId = check.recordset[0].AsuntoId;
        console.log(`Found existing Case 15 (AsuntoId: ${asuntoId}). Updating...`);
        await sql.query`
            UPDATE dgmesnie.TransparenciaAsunto SET
                Fecha = ${asunto.Fecha}, Estatus = ${asunto.Estatus},
                NumeroExpediente = ${asunto.NumeroExpediente}, Descripcion = ${asunto.Descripcion},
                SharePointUrl = ${asunto.SharePointUrl}, AudioEmbedUrl = ${asunto.AudioEmbedUrl},
                InfografiaEmbedUrl = ${asunto.InfografiaEmbedUrl}, PresentacionEmbedUrl = ${asunto.PresentacionEmbedUrl},
                ActualizadoEn = SYSUTCDATETIME(), ActualizadoPor = 'skill-transparencia'
            WHERE AsuntoId = ${asuntoId}`;
        
        await sql.query`DELETE FROM dgmesnie.TransparenciaHito WHERE AsuntoId = ${asuntoId}`;
        console.log(`Deleted old hitos for AsuntoId: ${asuntoId}`);
    } else {
        console.log(`Case 15 does not exist. Inserting new matter...`);
        const ins = await sql.query`
            INSERT INTO dgmesnie.TransparenciaAsunto
            (Titulo, Fecha, Estatus, FolioSolicitud, NumeroExpediente, Descripcion,
             SharePointUrl, AudioEmbedUrl, InfografiaEmbedUrl, PresentacionEmbedUrl,
             Activo, CreadoEn, CreadoPor)
            VALUES (${asunto.Titulo}, ${asunto.Fecha}, ${asunto.Estatus}, ${asunto.FolioSolicitud}, ${asunto.NumeroExpediente},
                    ${asunto.Descripcion}, ${asunto.SharePointUrl}, ${asunto.AudioEmbedUrl},
                    ${asunto.InfografiaEmbedUrl}, ${asunto.PresentacionEmbedUrl},
                    1, SYSUTCDATETIME(), 'skill-transparencia');
            SELECT SCOPE_IDENTITY() AS Id;`;
        asuntoId = ins.recordset[0].Id;
        console.log(`Successfully inserted new Case 15. DB AsuntoId: ${asuntoId}`);
    }

    console.log(`Inserting ${hitos.length} hitos...`);
    let orden = 0;
    for (const h of hitos) {
        await sql.query`
            INSERT INTO dgmesnie.TransparenciaHito
            (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
            VALUES (${asuntoId}, ${h.Fecha}, ${h.Titulo}, ${h.Descripcion}, ${orden++}, 1, SYSUTCDATETIME(), 'skill-transparencia')`;
    }
    console.log(`Hitos successfully inserted. Done!`);
    process.exit(0);
}

main().catch(e => {
    console.error("An error occurred:", e);
    process.exit(1);
});
