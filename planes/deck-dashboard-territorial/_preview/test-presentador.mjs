import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import url from 'node:url';

const DIR = path.resolve('planes/deck-dashboard-territorial');
const raw = fs.readFileSync(path.join(DIR, 'presentador.html'), 'utf8');
// Se envuelve igual que al publicar: doctype + head/body alrededor del fragmento.
const wrapped = '<!doctype html><html><head><meta charset="utf-8">' +
  raw.replace(/([\s\S]*?<\/style>)/, '$1@@SPLIT@@').split('@@SPLIT@@')[0] +
  '</head><body>' +
  raw.split('</style>').slice(1).join('</style>') + '</body></html>';
const tmp = path.join(DIR, '_preview', 'presentador-test.html');
fs.writeFileSync(tmp, wrapped, 'utf8');

const errores = [];
const b = await chromium.launch();
const p = await b.newPage({ viewport: { width: 1440, height: 900 } });
p.on('console', m => { if (m.type() === 'error') errores.push(m.text()); });
p.on('pageerror', e => errores.push('pageerror: ' + e.message));
await p.goto(url.pathToFileURL(tmp).href);
await p.waitForTimeout(2200);

const leer = () => p.evaluate(() => ({
  marca: document.getElementById('marca').textContent,
  rotulo: document.getElementById('rotulo-tit').textContent,
  activa: document.querySelector('.lamina.is-activa').id,
  visibles: document.querySelectorAll('.lamina.is-activa').length,
  atrasOff: document.getElementById('btn-atras').disabled,
  adelanteOff: document.getElementById('btn-adelante').disabled,
  avance: document.getElementById('avance').style.width,
  escala: document.getElementById('tarima').style.transform,
  hash: location.hash,
}));

console.log('inicio      ', JSON.stringify(await leer()));
await p.click('#btn-adelante');
await p.waitForTimeout(300);
console.log('tras +1     ', JSON.stringify(await leer()));
await p.keyboard.press('ArrowRight');
await p.keyboard.press('ArrowRight');
await p.waitForTimeout(300);
console.log('tras teclado', JSON.stringify(await leer()));
await p.keyboard.press('End');
await p.waitForTimeout(300);
console.log('fin (End)   ', JSON.stringify(await leer()));
await p.keyboard.press('Home');
await p.waitForTimeout(300);
console.log('Home        ', JSON.stringify(await leer()));

await p.click('#btn-indice');
await p.waitForTimeout(250);
const idx = await p.evaluate(() => ({
  abierto: document.getElementById('indice').classList.contains('is-abierto'),
  items: document.querySelectorAll('.idx-item').length,
  primero: document.querySelector('.idx-item .idx-tit').textContent,
}));
console.log('indice      ', JSON.stringify(idx));
await p.click('.idx-item[data-ir="14"]');
await p.waitForTimeout(300);
console.log('salto a 14  ', JSON.stringify(await leer()));

await p.screenshot({ path: path.join(DIR, '_preview', 'presentador.png') });

// desborde horizontal del documento
const enc = await p.evaluate(() => {
  const t = document.getElementById('tarima').getBoundingClientRect();
  const e = document.querySelector('.escenario').getBoundingClientRect();
  return {
    fuera_izq: Math.round(e.left - t.left), fuera_der: Math.round(t.right - e.right),
    fuera_arr: Math.round(e.top - t.top), fuera_abj: Math.round(t.bottom - e.bottom),
    lamina_ancho: Math.round(t.width), lamina_alto: Math.round(t.height),
    desborde_doc: document.documentElement.scrollWidth - document.documentElement.clientWidth,
  };
});
console.log('encuadre    ', JSON.stringify(enc));
console.log('errores_consola', errores.length ? errores : 'ninguno');
await b.close();
