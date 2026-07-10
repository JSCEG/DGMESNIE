// Update dirigido AsuntoId 24 (7. SNIE): reemplaza los embeds placeholder (7_snie.mp3, Google Slides
// 2PACX-...) por las rutas CDN reales T7. NO toca Titulo, Fecha, Estatus, NumeroExpediente,
// Descripcion, SharePointUrl, PresentacionEmbedUrl ni los hitos.
const sql = require('mssql');
const config = {
    server: 'servidorsqljavidev.database.windows.net',
    port: 1433,
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'Javiereg32',
    options: { encrypt: true, trustServerCertificate: false }
};
const AUDIO = 'https://cdn.sassoapps.com/dgmesnie/trasnparencia/audios/T7.m4a';
const INFOG = 'https://cdn.sassoapps.com/dgmesnie/trasnparencia/infografias/T7.png';

async function main() {
    await sql.connect(config);
    const r = await sql.query`
        UPDATE dgmesnie.TransparenciaAsunto
        SET AudioEmbedUrl = ${AUDIO}, InfografiaEmbedUrl = ${INFOG},
            ActualizadoEn = SYSUTCDATETIME(), ActualizadoPor = 'skill-transparencia'
        WHERE AsuntoId = 24 AND Activo = 1`;
    console.log('Filas actualizadas:', r.rowsAffected[0]);
    const a = (await sql.query`SELECT Titulo,AudioEmbedUrl,InfografiaEmbedUrl,PresentacionEmbedUrl,SharePointUrl FROM dgmesnie.TransparenciaAsunto WHERE AsuntoId=24`).recordset[0];
    console.log('Audio:', a.AudioEmbedUrl);
    console.log('Infog:', a.InfografiaEmbedUrl);
    console.log('Presentacion (sin tocar):', a.PresentacionEmbedUrl);
    console.log('SharePoint (sin tocar):', a.SharePointUrl);
    process.exit(0);
}
main().catch(e => { console.error(e.message); process.exit(1); });
