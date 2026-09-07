const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const turf = require('../wwwroot/js/turf.min.js');

const polygon = x => turf.polygon([[[x,25],[x+0.1,25],[x+0.1,25.1],[x,25]]]);
const calls = [];
const removed = [];
const context = {
  window: {}, turf, AbortSignal, console,
  fetch: async url => {
    calls.push(url);
    return { ok: url !== '/unavailable', status:503, text:async () => url };
  },
  DOMParser: class { parseFromString(value) { return { value, querySelector:() => null, querySelectorAll:() => [{remove:() => removed.push(value)}] }; } },
  L: { KML:class { constructor(xml) { this.xml = xml; } toGeoJSON() { return turf.featureCollection(this.xml.value === '/empty' ? [] : [polygon(-101), polygon(-102), turf.lineString([[-101,25],[-102,25]])]); } } }
};
vm.createContext(context);
vm.runInContext(fs.readFileSync(path.join(__dirname,'../wwwroot/js/convocatoria-geometria.js'),'utf8'),context);
const api = context.window.ConvocatoriaGeometria;
const config = {folio:'TEST-M2', titulo:'Prueba', gcr:'NORESTE', latitud:25, longitud:-101};

(async () => {
  assert.equal(api.point(null,null),false);
  assert.equal(api.point('', ''),false);
  assert.equal(api.point(0,0),false);
  assert.equal(api.point(25,-101),true);
  const empty = await api.load({folio:'EMPTY'});
  assert.equal(empty.ubicaciones.length,0);
  assert.equal(empty.advertencias.length,1);
  const fallback = await api.load({...config,projectKmlUrl:'/unavailable'});
  assert.equal(fallback.ubicaciones[0].geometria.type,'Point');
  assert.equal(fallback.advertencias.length,1);
  const fromKml = await api.load({...config,projectKmlUrl:'/valid'});
  const polygons = fromKml.ubicaciones.find(u => u.geometria.type === 'MultiPolygon');
  assert.equal(polygons.geometria.coordinates.length,2,'No debe perder componentes KML');
  assert.ok(fromKml.ubicaciones.some(u => u.geometria.type === 'MultiLineString'));
  assert.equal(fromKml.ubicaciones.every(u => u.validada === false),true);
  await api.load({...config,projectKmlUrl:'/valid'});
  assert.equal(calls.filter(url => url === '/valid').length,1,'Reutiliza la descarga del KML');
  assert.ok(removed.includes('/valid'),'Elimina descripciones antes de construir popups');
  const emptyKml = await api.load({...config,projectKmlUrl:'/empty'});
  assert.equal(emptyKml.ubicaciones[0].geometria.type,'Point');
  await api.load({...config,projectKmlUrl:'/unavailable'});
  assert.equal(calls.filter(url => url === '/unavailable').length,2,'Los fallos no quedan cacheados');
  console.log('Mixtos II: comprobaciones de geometría, fallback, componentes y caché correctas.');
})().catch(error => { console.error(error); process.exitCode = 1; });
