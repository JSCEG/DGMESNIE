# -*- coding: utf-8 -*-
"""Arma el presentador HTML autónomo a partir de las láminas .dc.html.

Salida: presentador.html — un solo archivo, sin dependencias externas salvo
Google Fonts. Botones anterior/siguiente, teclado, índice y pantalla completa.

Uso:  python presenter.py   (correr después de build.py)
"""
import base64
import html
import json
import os
import re

OUT = os.path.dirname(os.path.abspath(__file__))
W, H = 1600, 900

ORDER = [a["file"] for a in json.load(
    open(os.path.join(OUT, "canvas.json"), encoding="utf-8"))["artboards"]]

IMGS = {n: "data:image/png;base64," + base64.b64encode(
    open(os.path.join(OUT, n), "rb").read()).decode()
    for n in ("logo_gob.png", "logo_sener.png")}


def extraer(nombre):
    src = open(os.path.join(OUT, nombre), encoding="utf-8").read()
    cuerpo = re.search(r"<x-dc>(.*?)</x-dc>", src, re.S).group(1)
    cuerpo = re.sub(r"<helmet>.*?</helmet>", "", cuerpo, flags=re.S).strip()
    for k, v in IMGS.items():
        cuerpo = cuerpo.replace('src="./%s"' % k, 'src="%s"' % v)
    h1 = re.search(r"<h1[^>]*>(.*?)</h1>", cuerpo, re.S)
    crudo = h1.group(1) if h1 else nombre
    # los saltos de línea del título son separadores de palabra, no vacío
    crudo = re.sub(r"<br\s*/?>", " ", crudo)
    titulo = re.sub(r"\s+", " ", re.sub(r"<[^>]+>", "", crudo)).strip().rstrip(".")
    eyebrow = re.search(
        r'text-transform:uppercase;color:#[0-9A-Fa-f]{6}">(.*?)</div>', cuerpo)
    kicker = re.sub(r"<[^>]+>", "", eyebrow.group(1)).strip() if eyebrow else ""
    return cuerpo, titulo, kicker


laminas = [extraer(n) for n in ORDER]

secciones = "\n".join(
    '<section class="lamina%s" id="lam-%d" aria-hidden="%s">%s</section>'
    % (" is-activa" if i == 0 else "", i + 1, "false" if i == 0 else "true", cuerpo)
    for i, (cuerpo, _t, _k) in enumerate(laminas))

indice = "\n".join(
    '<button type="button" class="idx-item" data-ir="%d">'
    '<span class="idx-num">%02d</span>'
    '<span class="idx-txt"><span class="idx-tit">%s</span>'
    '<span class="idx-kick">%s</span></span></button>'
    % (i + 1, i + 1, html.escape(t), html.escape(k))
    for i, (_c, t, k) in enumerate(laminas))

TITULOS_JS = json.dumps([t for _c, t, _k in laminas], ensure_ascii=False)

DOC = """<title>Tablero Energético en 20 Láminas</title>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Noto+Sans:wght@400;600;700;800&family=IBM+Plex+Mono:wght@500;600&display=swap">
<style>
  :root {
    --sala: #17161A;
    --sala-alta: #201E24;
    --cromo: #B9B3AD;
    --cromo-tenue: #6E6862;
    --filete: rgba(255, 255, 255, .14);
    --guinda: #9B2247;
    --papel: #FAF8F5;
    --mono: 'IBM Plex Mono', Consolas, monospace;
    --sans: 'Noto Sans', 'Segoe UI', system-ui, sans-serif;
  }

  * { box-sizing: border-box; }

  body {
    margin: 0;
    background: var(--sala);
    color: var(--cromo);
    font-family: var(--sans);
    height: 100vh;
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }

  /* ── filete de avance ───────────────────────────────────────── */
  .avance {
    flex: none;
    height: 3px;
    background: rgba(255, 255, 255, .08);
  }
  .avance i {
    display: block;
    height: 100%;
    width: 5%;
    background: var(--guinda);
    transition: width .25s ease;
  }

  /* ── escenario ──────────────────────────────────────────────── */
  .escenario {
    flex: 1;
    position: relative;
    min-height: 0;
    overflow: hidden;
  }
  .tarima {
    position: absolute;
    left: 50%;
    top: 50%;
    width: 1600px;
    height: 900px;
    /* el centrado lo hace el translate; la escala la calcula el guion */
    transform: translate(-50%, -50%);
    transform-origin: center center;
    box-shadow: 0 24px 70px rgba(0, 0, 0, .55);
    /* la lámina es material proyectado: nada dentro de ella recibe el ratón,
       para que nunca le robe un clic a la barra de control */
    pointer-events: none;
  }
  .lamina {
    position: absolute;
    inset: 0;
    opacity: 0;
    visibility: hidden;
    transition: opacity .18s ease;
  }
  .lamina.is-activa { opacity: 1; visibility: visible; }

  /* estilos heredados de las láminas, acotados para no tocar el marco */
  .lamina a { color: var(--guinda); text-decoration: none; }
  .lamina code {
    font-family: var(--mono);
    font-size: .92em;
    background: rgba(155, 34, 71, .07);
    padding: 1px 5px;
  }

  /* ── barra de control ───────────────────────────────────────── */
  .control {
    flex: none;
    position: relative;
    z-index: 10;
    display: flex;
    align-items: center;
    gap: 18px;
    padding: 12px 22px 14px;
    border-top: 1px solid var(--filete);
    background: var(--sala-alta);
  }
  .rotulo { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
  .rotulo b {
    font-size: 12px;
    font-weight: 700;
    color: var(--cromo);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
  .rotulo span {
    font-family: var(--mono);
    font-size: 10px;
    letter-spacing: .14em;
    text-transform: uppercase;
    color: var(--cromo-tenue);
  }
  .empuje { flex: 1; }

  button {
    font: inherit;
    cursor: pointer;
    border: 1px solid var(--filete);
    background: transparent;
    color: var(--cromo);
    border-radius: 9px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 9px 13px;
    transition: border-color .15s ease, color .15s ease, background .15s ease;
  }
  button:hover:not(:disabled) {
    border-color: var(--guinda);
    color: #fff;
    background: rgba(155, 34, 71, .22);
  }
  button:disabled { opacity: .32; cursor: default; }
  button:focus-visible { outline: 2px solid var(--guinda); outline-offset: 2px; }
  .paso { width: 42px; height: 40px; padding: 0; }
  .marca {
    font-family: var(--mono);
    font-size: 13px;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    color: #fff;
    min-width: 72px;
    text-align: center;
  }
  .marca em { font-style: normal; color: var(--cromo-tenue); }
  .texto-btn { font-size: 12px; font-weight: 600; }

  /* ── índice ─────────────────────────────────────────────────── */
  .indice {
    position: fixed;
    inset: 0;
    z-index: 30;
    background: rgba(12, 11, 14, .93);
    padding: 46px 40px;
    overflow-y: auto;
    display: none;
  }
  .indice.is-abierto { display: block; }
  .indice h2 {
    margin: 0 0 26px;
    font-family: var(--mono);
    font-size: 12px;
    font-weight: 600;
    letter-spacing: .2em;
    text-transform: uppercase;
    color: var(--guinda);
  }
  .idx-rejilla {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 10px;
    max-width: 1400px;
  }
  .idx-item {
    justify-content: flex-start;
    text-align: left;
    padding: 13px 15px;
    gap: 14px;
  }
  .idx-item.is-actual { border-color: var(--guinda); background: rgba(155, 34, 71, .18); }
  .idx-num {
    font-family: var(--mono);
    font-size: 12px;
    font-weight: 600;
    color: var(--guinda);
    flex: none;
  }
  .idx-txt { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
  .idx-tit { font-size: 13.5px; font-weight: 700; color: #fff; line-height: 1.3; }
  .idx-kick {
    font-family: var(--mono);
    font-size: 9.5px;
    letter-spacing: .12em;
    text-transform: uppercase;
    color: var(--cromo-tenue);
  }

  @media (prefers-reduced-motion: reduce) {
    .lamina, .avance i { transition: none; }
  }
  @media (max-width: 720px) {
    .rotulo, .texto-btn { display: none; }
    .control { gap: 10px; padding: 10px 12px 12px; }
  }
</style>

<div class="avance"><i id="avance"></i></div>

<main class="escenario">
  <div class="tarima" id="tarima">
@@LAMINAS@@
  </div>
</main>

<div class="control">
  <div class="rotulo">
    <b id="rotulo-tit">&nbsp;</b>
    <span>Dashboard de Planeación Energética &middot; DGMESNIE</span>
  </div>
  <div class="empuje"></div>

  <button type="button" class="paso" id="btn-atras" aria-label="Lámina anterior" title="Anterior (flecha izquierda)">
    <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
  </button>
  <div class="marca" id="marca" aria-live="polite">01<em> / 20</em></div>
  <button type="button" class="paso" id="btn-adelante" aria-label="Lámina siguiente" title="Siguiente (flecha derecha)">
    <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
  </button>

  <div class="empuje"></div>
  <button type="button" id="btn-indice" title="Índice de láminas (tecla I)">
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 6h16M4 12h16M4 18h16"/></svg>
    <span class="texto-btn">Índice</span>
  </button>
  <button type="button" id="btn-pantalla" title="Pantalla completa (tecla F)">
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3H5a2 2 0 0 0-2 2v3M16 3h3a2 2 0 0 1 2 2v3M8 21H5a2 2 0 0 1-2-2v-3M16 21h3a2 2 0 0 0 2-2v-3"/></svg>
    <span class="texto-btn">Pantalla completa</span>
  </button>
</div>

<div class="indice" id="indice" role="dialog" aria-label="Índice de láminas">
  <h2>Índice &middot; 20 láminas</h2>
  <div class="idx-rejilla">
@@INDICE@@
  </div>
</div>

<script>
(function () {
  var TITULOS = @@TITULOS@@;
  var laminas = Array.prototype.slice.call(document.querySelectorAll('.lamina'));
  var total = laminas.length;
  var actual = 0;

  var tarima = document.getElementById('tarima');
  var marca = document.getElementById('marca');
  var avance = document.getElementById('avance');
  var rotulo = document.getElementById('rotulo-tit');
  var atras = document.getElementById('btn-atras');
  var adelante = document.getElementById('btn-adelante');
  var indice = document.getElementById('indice');

  function ajustar() {
    var caja = tarima.parentElement.getBoundingClientRect();
    var margen = 44;
    var escala = Math.min((caja.width - margen) / 1600, (caja.height - margen) / 900);
    tarima.style.transform =
      'translate(-50%, -50%) scale(' + Math.max(escala, 0.05) + ')';
  }

  function ir(n) {
    n = Math.max(0, Math.min(total - 1, n));
    laminas[actual].classList.remove('is-activa');
    laminas[actual].setAttribute('aria-hidden', 'true');
    actual = n;
    laminas[actual].classList.add('is-activa');
    laminas[actual].setAttribute('aria-hidden', 'false');

    marca.innerHTML = ('0' + (n + 1)).slice(-2) + '<em> / ' + total + '</em>';
    avance.style.width = (((n + 1) / total) * 100) + '%';
    rotulo.textContent = TITULOS[n] || '';
    atras.disabled = n === 0;
    adelante.disabled = n === total - 1;
    history.replaceState(null, '', '#' + (n + 1));

    var items = indice.querySelectorAll('.idx-item');
    for (var i = 0; i < items.length; i++) {
      items[i].classList.toggle('is-actual', i === n);
    }
  }

  atras.addEventListener('click', function () { ir(actual - 1); });
  adelante.addEventListener('click', function () { ir(actual + 1); });

  document.getElementById('btn-indice').addEventListener('click', function () {
    indice.classList.toggle('is-abierto');
  });
  indice.addEventListener('click', function (e) {
    var b = e.target.closest('.idx-item');
    if (b) { ir(parseInt(b.dataset.ir, 10) - 1); indice.classList.remove('is-abierto'); }
    else if (e.target === indice) { indice.classList.remove('is-abierto'); }
  });

  document.getElementById('btn-pantalla').addEventListener('click', function () {
    if (document.fullscreenElement) { document.exitFullscreen(); }
    else if (document.documentElement.requestFullscreen) {
      document.documentElement.requestFullscreen();
    }
  });

  document.addEventListener('keydown', function (e) {
    if (e.metaKey || e.ctrlKey || e.altKey) { return; }
    var k = e.key;
    if (k === 'ArrowRight' || k === 'PageDown' || k === ' ' || k === 'Enter') {
      e.preventDefault(); ir(actual + 1);
    } else if (k === 'ArrowLeft' || k === 'PageUp' || k === 'Backspace') {
      e.preventDefault(); ir(actual - 1);
    } else if (k === 'Home') { e.preventDefault(); ir(0); }
    else if (k === 'End') { e.preventDefault(); ir(total - 1); }
    else if (k === 'i' || k === 'I') { indice.classList.toggle('is-abierto'); }
    else if (k === 'f' || k === 'F') { document.getElementById('btn-pantalla').click(); }
    else if (k === 'Escape') { indice.classList.remove('is-abierto'); }
  });

  window.addEventListener('resize', ajustar);
  ajustar();

  var inicial = parseInt((location.hash || '').replace('#', ''), 10);
  ir(isNaN(inicial) ? 0 : inicial - 1);
})();
</script>
"""

doc = (DOC.replace("@@LAMINAS@@", secciones)
          .replace("@@INDICE@@", indice)
          .replace("@@TITULOS@@", TITULOS_JS))

destino = os.path.join(OUT, "presentador.html")
with open(destino, "w", encoding="utf-8") as fh:
    fh.write(doc)

print("presentador.html: %d laminas, %.1f KB"
      % (len(laminas), os.path.getsize(destino) / 1024))
