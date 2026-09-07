// Exporta el deck a PDF 16:9 y a PNG por lamina, como respaldo offline.
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import url from 'node:url';

const DIR = path.resolve('planes/deck-dashboard-territorial');
const PREV = path.join(DIR, '_preview');
const PNGS = path.join(DIR, 'png');
fs.mkdirSync(PNGS, { recursive: true });

const ORDER = JSON.parse(fs.readFileSync(path.join(DIR, 'canvas.json'), 'utf8'))
  .artboards.map(a => a.file);

const imgs = {};
for (const f of ['logo_gob.png', 'logo_sener.png']) {
  imgs[f] = 'data:image/png;base64,' + fs.readFileSync(path.join(DIR, f)).toString('base64');
}

function extract(file) {
  const s = fs.readFileSync(path.join(DIR, file), 'utf8');
  const helmet = s.match(/<helmet>([\s\S]*?)<\/helmet>/)[1];
  let body = s.match(/<x-dc>([\s\S]*?)<\/x-dc>/)[1].replace(/<helmet>[\s\S]*?<\/helmet>/, '');
  for (const [k, v] of Object.entries(imgs)) body = body.split('src="./' + k + '"').join('src="' + v + '"');
  return { helmet, body };
}

const first = extract(ORDER[0]);
const link = first.helmet.match(/<link[^>]*fonts\.googleapis[^>]*>/)[0];
const style = first.helmet.match(/<style>([\s\S]*?)<\/style>/)[1];

const pages = ORDER.map(f => '<div class="pg">' + extract(f).body + '</div>').join('\n');
const doc = `<!doctype html><html><head><meta charset="utf-8">${link}<style>${style}
  @page { size: 1600px 900px; margin: 0; }
  html, body { margin:0; padding:0; background:#fff; }
  .pg { width:1600px; height:900px; overflow:hidden; break-after:page; }
  .pg:last-child { break-after:auto; }
</style></head><body>${pages}</body></html>`;

const out = path.join(PREV, 'deck-print.html');
fs.writeFileSync(out, doc, 'utf8');

const b = await chromium.launch();
const p = await b.newPage({ viewport: { width: 1600, height: 900 } });
await p.goto(url.pathToFileURL(out).href);
await p.waitForTimeout(3000);

const cards = await p.$$('.pg');
for (let i = 0; i < cards.length; i++) {
  const name = ORDER[i].replace('.dc.html', '');
  await cards[i].screenshot({
    path: path.join(PNGS, String(i + 1).padStart(2, '0') + '-' + name + '.png'),
  });
}

await p.pdf({
  path: path.join(DIR, 'Dashboard-Planeacion-Energetica-2026-08-20.pdf'),
  width: '1600px', height: '900px', printBackground: true, pageRanges: '1-' + ORDER.length,
});
await b.close();
console.log('PDF + ' + cards.length + ' PNG generados');
