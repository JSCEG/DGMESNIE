const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/pam-ficha-envio.js'), 'utf8');

async function submitFixture({ generationError = false } = {}) {
  const handlers = {}, requests = [], feedback = {};
  const bytes = Buffer.from('fixture-de-transporte-no-es-un-PDF');
  const recipient = { value: '123', checked: true };
  const form = {
    dataset: { folio: 'CFE-CM2-0400-2026', endpoint: '/DashboardProyectos/Convocatorias/Ficha/Enviar' },
    addEventListener: (name, handler) => { handlers[name] = handler; },
    querySelectorAll: () => [recipient],
    querySelector: selector => selector.includes('formato') ? { value: 'pdf' }
      : selector.includes('RequestVerificationToken') ? { value: 'test-anti-csrf' } : null
  };
  const elements = {
    'pam-envio-modal': { hidden: false, querySelectorAll: () => [] },
    'pam-btn-enviar': { addEventListener() {} },
    'pam-envio-form': form,
    'pam-envio-buscar': { value: '', focus() {}, addEventListener() {} },
    'pam-envio-lista': { querySelectorAll: () => [] },
    'pam-envio-feedback': feedback,
    'pam-envio-enviar': {},
    'pam-envio-mensaje': { value: 'Prueba aislada sin correo' }
  };
  vm.runInNewContext(source, {
    document: { readyState: 'complete', getElementById: id => elements[id], addEventListener() {} },
    window: { pamFichaGenerar: async () => {
      if (generationError) throw new Error('Error de captura simulado');
      return { base64: 'data:application/pdf;base64,' + bytes.toString('base64'), nombre: 'Ficha_validacion.pdf' };
    } },
    fetch: async (url, options) => {
      requests.push({ url, options });
      return { ok: true, json: async () => ({ ok: true, mensaje: 'Simulado' }) };
    },
    setTimeout() {},
    console: { error() {} }
  });
  await handlers.submit({ preventDefault() {} });
  return { requests, bytes, feedback };
}

test('Mixtos II envía los bytes generados, folio, destinatarios y antiforgery al endpoint correcto', async () => {
  const { requests, bytes } = await submitFixture();
  assert.equal(requests.length, 1);
  const { url, options } = requests[0];
  const body = JSON.parse(options.body);
  assert.equal(url, '/DashboardProyectos/Convocatorias/Ficha/Enviar');
  assert.equal(options.headers.RequestVerificationToken, 'test-anti-csrf');
  assert.equal(body.folio, 'CFE-CM2-0400-2026');
  assert.equal(body.formato, 'pdf');
  assert.deepEqual(body.usuarioIds, [123]);
  assert.equal(body.nombreArchivo, 'Ficha_validacion.pdf');
  assert.deepEqual(Buffer.from(body.archivoBase64.split(',')[1], 'base64'), bytes);
});

test('Si falla la generación no se intenta enviar correo', async () => {
  const { requests, feedback } = await submitFixture({ generationError: true });
  assert.equal(requests.length, 0);
  assert.match(feedback.innerHTML, /No se pudo generar el archivo adjunto/);
});
