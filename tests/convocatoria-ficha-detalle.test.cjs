const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = name => fs.readFileSync(path.join(__dirname, '..', 'wwwroot', 'js', name), 'utf8');
const field = (name, value, column = 'P') => ({ name, value, column });
const record = (sheet, row, fields) => ({ sheet, row, fields });
const feature = (properties, x = -100) => ({ type: 'Feature', properties, geometry: { type: 'Point', coordinates: [x, 25] } });
const wrapped = f => ({ feature: f });
const copy = value => JSON.parse(JSON.stringify(value));

// Minimal DOM for table ordering, row limits, escaping, and data contract tests.
// Browser/PDF geometry and visual clipping require the separate live QA pass.
function render(records = [], options = {}) {
  const slides = [], listeners = {}, principal = { dataset: {} };
  function plain() { return { textContent: '', classList: { add() {} } }; }
  function slide() {
    const tbody = { children: [], append(tr) { this.children.push(tr); tr.parent = this; } };
    const body = { innerHTML: '', clientHeight: 480, get scrollHeight() { return tbody.children.length * 45; }, querySelector() { return tbody; } };
    const nodes = { '[data-detail-title]': plain(), '[data-detail-context]': plain(), '[data-detail-note]': plain(), '[data-detail-body]': body };
    return { dataset: {}, nodes, tbody, style: { removeProperty(name) { delete this[name]; } }, querySelector(selector) { return nodes[selector]; } };
  }
  const anchors = {};
  for (const name of ['costos', 'factibilidad', 'social', 'expediente', 'evidence']) {
    anchors[name] = { before(item) { item.anchor = name; slides.push(item); } };
  }
  const document = {
    addEventListener(name, callback) { listeners[name] = callback; },
    getElementById() { return { content: { firstElementChild: { cloneNode: slide } } }; },
    querySelector(selector) {
      if (selector === '.convocatoria-ficha-page') return principal;
      if (selector === '[data-convocatoria-analysis-evidence]') return { closest() { return anchors.evidence; } };
      const match = selector.match(/^\[data-convocatoria-(\w+)-anchor\]$/);
      return match ? anchors[match[1]] : null;
    },
    createElement() {
      return { innerHTML: '', firstElementChild: {}, remove() { this.parent.children.splice(this.parent.children.indexOf(this), 1); } };
    }
  };
  const result = { elementos: [], capas: {}, ...options.result };
  const context = { console, document, window: { convocatoriaFichaMap: {
    conCostos: options.conCostos !== false, obrasDetalle: options.obrasDetalle || '',
    expediente: { fileName: 'corte-prueba.xlsx', exchangeRate: 18.7, records }
  } } };
  vm.runInNewContext(source('convocatoria-ficha-detalle.js'), context);
  listeners['convocatoria:territorial-ready']({ detail: { resultado: result, fuentes: options.sources || {} } });
  return { slides, principal };
}
const rows = slide => slide.tbody.children.map(tr => [...tr.innerHTML.matchAll(/<td>([\s\S]*?)<\/td>/g)].map(match => match[1]));
const selected = (slides, title) => slides.filter(slide => slide.dataset.label.startsWith(title));
const values = slides => slides.flatMap(rows);
const oneWork = () => record('BD_COSTOS_OBRAS', 354, [
  field('No.', 1), field('Tipo de obra', 'Interconexión'), field('Nombre de la obra', 'Línea <prueba>'),
  field('Lamina', 3), field('A cargo del solicitante', 'Sí'), field('Obra civil (USD)', 0),
  field('Total construcción (USD)', null), field('Costo de red de la obra (MDD)', 1), field('Costo de red de la obra (MDP)', 18.7)
]);

test('ubica obras, costos, evaluación técnica y social en sus secciones; escapa HTML', () => {
  const { slides, principal } = render([
    oneWork(), record('BD_FACTIBILIDAD', 9, [field('Factibilidad', 'Factible')]),
    record('BD_IMPACTO_SOCIAL', 57, [field('Observaciones', 'Consulta pendiente', 'I')])
  ]);
  for (const title of ['Desglose de obras', 'Costos de construcción', 'Actividades previas y servicios']) {
    assert.equal(selected(slides, title)[0].anchor, 'costos');
  }
  assert.equal(selected(slides, 'Evaluación técnica')[0].anchor, 'factibilidad');
  assert.equal(selected(slides, 'Evaluación social')[0].anchor, 'social');
  assert.match(values(selected(slides, 'Desglose de obras'))[0][2], /&lt;prueba&gt;/);
  assert.equal(values(selected(slides, 'Costos de construcción'))[0][1], '0');
  assert.equal(values(selected(slides, 'Costos de construcción'))[0][5], 'No informado');
  assert.match(selected(slides, 'Actividades previas')[0].nodes['[data-detail-context]'].textContent, /18\.7 MXN\/USD/);
  assert.equal(principal.dataset.detailsState, 'complete');
});

test('conserva completas las observaciones largas y pagina sin eliminar registros distintos', () => {
  const text = 'Evaluación territorial del proyecto. '.repeat(90);
  const { slides } = render([
    record('CATALOGO DE PROYECTOS', 221, [field('OBSERVACIÓN DEL ÁREA', text, 'AX')]),
    record('BD_IMPACTO_SOCIAL', 57, [field('Observación', text, 'I')]),
    record('BD_FACTIBILIDAD', 9, [field('Dictamen', 'Primera evaluación')]),
    record('BD_FACTIBILIDAD', 10, [field('Dictamen', 'Segunda evaluación')])
  ]);
  assert.equal(values(selected(slides, 'Observación del área')).map(row => row[1]).join(''), text);
  assert.equal(values(selected(slides, 'Evaluación social')).map(row => row[1]).join(''), text);
  assert.equal(selected(slides, 'Evaluación técnica').length, 2);
  assert.ok(slides.every(slide => slide.tbody.children.length <= 8));
  assert.ok(selected(slides, 'Observación del área').every(slide => slide.anchor === 'factibilidad'));
});

test('omite sólo observación del catálogo ya presentada; no colapsa evaluaciones por folio', () => {
  const fields = [field('Comentario', 'La misma evaluación')];
  const { slides } = render([
    record('CATALOGO DE PROYECTOS', 221, [field('OBSERVACIÓN DEL ÁREA', 'La misma evaluación', 'AX')]),
    record('BD_FACTIBILIDAD', 9, fields), record('BD_FACTIBILIDAD', 10, fields)
  ]);
  assert.equal(selected(slides, 'Observación del área').length, 0);
  assert.equal(selected(slides, 'Evaluación técnica').length, 2);
  const notDisplayed = render([
    record('CATALOGO DE PROYECTOS', 221, [field('OBSERVACIÓN DEL ÁREA', 'Comentario no visible', 'AX')]),
    record('BD_FACTIBILIDAD', 9, [field('Otra columna', 'Comentario no visible', 'B')])
  ]);
  assert.equal(selected(notDisplayed.slides, 'Observación del área').length, 1);
});

test('no repite obras del catálogo cuando ya existe desglose; respeta variante sin costos', () => {
  const obrasDetalle = 'Descripción heredada extensa. '.repeat(40);
  const { slides } = render([oneWork()], { obrasDetalle, conCostos: false });
  assert.equal(selected(slides, 'Desglose de obras').length, 1);
  assert.equal(selected(slides, 'Costos de construcción').length, 0);
  assert.equal(selected(slides, 'Actividades previas').length, 0);
  const fallback = render([], { obrasDetalle });
  assert.ok(selected(fallback.slides, 'Desglose de obras').every(slide => slide.anchor === 'costos'));
  assert.equal(values(selected(fallback.slides, 'Desglose de obras')).map(row => row[0]).join(''), obrasDetalle);
});

test('conserva una observación exclusiva del catálogo real aunque no exista BD_FACTIBILIDAD', () => {
  const observation = 'Obras condicionadas a la validación del área responsable.';
  const { slides } = render([
    record('CATALOGO DE PROYECTOS', 259, [field('OBSERVACIÓN DEL ÁREA', observation, 'AX')])
  ]);
  const pages = selected(slides, 'Observación del área');
  assert.equal(pages.length, 1);
  assert.equal(pages[0].anchor, 'factibilidad');
  assert.equal(values(pages)[0][1], observation);
  assert.equal(values(pages)[0][2], 'AX259');
  assert.match(pages[0].nodes['[data-detail-context]'].textContent, /CATALOGO DE PROYECTOS · fila 259/);
});

function element(overrides = {}) {
  return { ubicacion: { etiqueta: 'Proyecto' }, municipio: { nombre: 'Municipio', entidadNombre: 'Estado' },
    feature: feature({}), radioKm: 20, gcr: 'OC', intensivo: {}, permisos: {}, ...overrides };
}

test('consolida registros idénticos entre áreas pero preserva dos tramos con igual nombre y clave', () => {
  const first = feature({ nombre: 'Línea A', claveProyecto: 'PAM-1', nombreProyecto: 'Proyecto A' });
  const other = feature({ nombre: 'Línea A', claveProyecto: 'PAM-1', nombreProyecto: 'Proyecto A' }, -101);
  const result = { elementos: [element({ ltDentro: [wrapped(first), wrapped(other)], intensivo: { pam: [wrapped(first), wrapped(other)] } }),
    element({ ltDentro: [wrapped(copy(first))], intensivo: { pam: [wrapped(copy(first))] } })] };
  const { slides } = render([], { result });
  assert.equal(values(selected(slides, 'Líneas de transmisión')).length, 2);
  assert.equal(values(selected(slides, 'Proyectos PAM')).length, 2);
});

test('demanda conserva fecha, hora, pronóstico, nulos y cero; tarifas conservan GWh y período', () => {
  const demand = feature({ region: 'Occidental', gerencia: 'OC', operating_date: '2026-09-04', hour: 18,
    demand_mw: 0, generation_mw: null, forecast_mw: 2000, balance_mw: null, forecast_deviation_mw: -2000,
    updated_at: '2026-09-04T18:05:00', source: 'CENACE · prueba' });
  const tarifa = feature({ division: 'Bajío', reference_year: 2024, users_total: 10000, energy_gwh: 22.5, energy_mwh: 22500, source: 'CFE · prueba' });
  const { slides } = render([], { result: { elementos: [element({ demanda: demand, tarifa }), element({ demanda: copy(demand), tarifa: copy(tarifa) })] } });
  const demandSlides = selected(slides, 'Demanda y generación ·');
  assert.equal(demandSlides.length, 1);
  assert.deepEqual(values(demandSlides)[0], ['Demanda', '0 MW', '2026-09-04 · 18']);
  assert.equal(values(demandSlides)[1][1], 'No disponible MW');
  assert.deepEqual(values(demandSlides)[2].slice(0, 2), ['Pronóstico de demanda', '2,000 MW']);
  assert.match(demandSlides[0].nodes['[data-detail-note]'].textContent, /2026-09-04T18:05:00/);
  const tariffSlides = selected(slides, 'División CFE ·');
  assert.equal(tariffSlides.length, 1);
  assert.deepEqual(values(tariffSlides)[1], ['Energía', '22.5 GWh', '2024']);
});

test('potencias de centrales y GD conservan unidades, períodos y estado; no usan volumen de presas', () => {
  const central = feature({ NumeroPermiso: 'E/1', 'Razón_social': 'Central A', 'Tecnología': 'Solar',
    Estatus_instalacion: 'En operación', Capacidad_autorizada_MW: 50, Capacidad_operacion_MW: '', Periodo: 2024 });
  const gd = feature({ nombre: 'Estado', dg_mw: 0, dg_contracts: null, reference_period: '2025-S2' });
  const { slides } = render([], { result: { elementos: [element({ intensivo: {
    centrales: [wrapped(central)], generacionDistribuida: [wrapped(gd)], presas: [wrapped(feature({ id: 'Presa', capacidad: 2000 }))]
  } })] } });
  const centralRows = values(selected(slides, 'Centrales eléctricas'));
  assert.deepEqual(centralRows[0], ['E/1\nCentral A', 'Solar\nEn operación', '50', 'No disponible', '2024']);
  assert.deepEqual(values(selected(slides, 'Generación distribuida'))[0], ['Estado', '0', 'No disponible', '2025-S2']);
});

function pamApi(mixtos) {
  const code = source('pam-ficha-territorial.js').replace(/\}\)\(\);\s*$/, 'window.testApi = { claveFeature, nombreFeature, renderEjecutivo, guardarEncuadreMixtos, restaurarEncuadreMixtos, redibujarMapasTerritoriales, setMap: value => { mapa = value; } };\n})();');
  const context = { console, document: { readyState: 'loading', addEventListener() {} }, window: { convocatoriaFichaMap: mixtos ? {} : null } };
  vm.runInNewContext(code, context);
  return context.window.testApi;
}

test('deduplicación del motor queda condicionada a Mixtos II; PAM conserva su contrato', () => {
  const a = feature({ id: 1, nombre: 'Tramo A' });
  const b = feature({ id: 1, nombre: 'Tramo A' }, -101);
  const mixtos = pamApi(true), pam = pamApi(false);
  assert.equal(mixtos.claveFeature(a, 0, 'lt'), mixtos.claveFeature(copy(a), 1, 'lt'));
  assert.notEqual(mixtos.claveFeature(a, 0, 'lt'), mixtos.claveFeature(b, 1, 'lt'));
  assert.equal(pam.claveFeature(a, 0, 'lt'), pam.claveFeature(b, 1, 'lt'));
  assert.equal(mixtos.nombreFeature(feature({ 'Razón_social': 'Central identificada' }), ''), 'Central identificada');
});

test('hallazgos no repiten etiquetas entre catálogos; no eliminan los registros de origen', async () => {
  const api = pamApi(true), container = { dataset: {}, innerHTML: '' };
  const result = { gcrs: ['OC'], intensivoPromise: Promise.resolve({}), elementos: [element({
    intensivo: { centrales: [wrapped(feature({ nombre: 'Central Álamo' }))], presas: [], generacionPrivada: [],
      conservacion: [wrapped(feature({ nombre: 'Central Alamo' }, -101))], sitiosArqueologicos: [], zonasArqueologicas: [], zonasHistoricas: [] }
  })] };
  await api.renderEjecutivo(result, container);
  assert.equal(container.dataset.rendered, 'true');
  const chips = container.innerHTML.match(/<div class="pam-ejecutivo-hallazgos">([\s\S]*?)<\/div>/)[1];
  assert.equal((chips.match(/<span>/g) || []).length, 1);
  assert.equal(result.elementos[0].intensivo.conservacion.length, 1);
});

test('restaura encuadre de Mixtos después de invalidar tamaño y antes de redibujar para exportar', () => {
  const api = pamApi(true), calls = [], bounds = { isValid: () => true };
  const container = { clientWidth: 777, clientHeight: 493 };
  const map = {
    getContainer: () => container,
    invalidateSize(options) { calls.push(['size', options.pan]); },
    fitBounds(actual, options) { calls.push(['fit', actual, options.animate, options.maxZoom]); },
    eachLayer(callback) { callback({ redraw() { calls.push(['draw']); } }); }
  };
  api.setMap(map);
  api.guardarEncuadreMixtos(map, bounds, { padding: [12, 12], maxZoom: 9 });
  api.redibujarMapasTerritoriales();
  assert.deepEqual(calls.map(call => call[0]), ['size', 'fit', 'draw']);
  assert.equal(calls[1][1], bounds, 'Conserva los límites del proyecto/buffer');
  assert.equal(calls[1][2], false, 'No anima durante la captura');
  assert.equal(calls[1][3], 9);
  calls.length = 0;
  container.clientWidth = 0;
  api.redibujarMapasTerritoriales();
  assert.equal(calls.length, 0, 'No calcula encuadre de una lámina oculta');
});

test('reencuadre de exportación no modifica PAM ni mapas sin geometría válida', () => {
  const pam = pamApi(false), mixtos = pamApi(true), calls = [];
  const map = { fitBounds() { calls.push('fit'); } };
  pam.guardarEncuadreMixtos(map, { isValid: () => true }, {});
  pam.restaurarEncuadreMixtos(map);
  mixtos.guardarEncuadreMixtos(map, { isValid: () => false }, {});
  mixtos.restaurarEncuadreMixtos(map);
  assert.equal(calls.length, 0);
});
