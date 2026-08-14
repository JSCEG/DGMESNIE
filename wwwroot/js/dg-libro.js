/*
 * Modo libro del reporte territorial.
 *
 * El informe en pantalla es un flujo continuo: se lee bien, pero no deja ver
 * cómo va a quedar impreso ni dónde caen los cortes. Este modo compone hojas
 * carta reales a partir del mismo HTML, midiendo: coloca cada bloque, mide la
 * caja y sólo abre hoja nueva cuando deja de caber.
 *
 * El algoritmo viene del paginador editorial de la DGMESNIE (79.-Informes,
 * src/site/paginate.js), pero escrito contra el DOM de este tablero: allá los
 * bloques son [data-topic] y .packed-topic; aquí son .dg-report-section con sus
 * tablas, fichas y gráficas.
 *
 * Lo que el modo garantiza:
 *  - ningún bloque se recorta a media altura;
 *  - las tablas largas se parten por renglón, repitiendo su encabezado;
 *  - un encabezado suelto al pie viaja con el bloque que lo sigue;
 *  - la portada y el índice ocupan hoja propia.
 */
(function () {
  'use strict';

  var CLASE_ACTIVO = 'dg-libro-activo';
  var estado = null;

  function crear(tag, clase) {
    var nodo = document.createElement(tag);
    if (clase) nodo.className = clase;
    return nodo;
  }

  function desborda(caja) {
    // 1 px de tolerancia: el redondeo del navegador provoca desbordes falsos
    // que abrirían una hoja por cada bloque.
    return caja.scrollHeight > caja.clientHeight + 1;
  }

  /* ── Partir bloques ────────────────────────────────────────────────────── */

  // Devuelve la "cola" que no cupo, o null si el bloque no se puede partir.
  function partir(nodo, caja) {
    if (nodo.tagName === 'TABLE') return partirTabla(nodo, caja);
    if (nodo.classList.contains('dg-report-table-wrap')) {
      var tabla = nodo.querySelector('table');
      if (!tabla) return null;
      var colaTabla = partirTabla(tabla, caja);
      if (!colaTabla) return null;
      var envoltura = nodo.cloneNode(false);
      envoltura.appendChild(colaTabla);
      return envoltura;
    }
    if (nodo.tagName === 'UL' || nodo.tagName === 'OL') return partirPorHijos(nodo, nodo, caja);
    // Rejillas: KPI, gráficas, galería de mapas. Se parten por celda.
    if (nodo.classList.contains('dg-report-metrics')
      || nodo.classList.contains('dg-report-chart-grid')
      || nodo.classList.contains('dg-report-map-gallery')) {
      return partirPorHijos(nodo, nodo, caja);
    }
    if (nodo.tagName === 'P') return partirPorOraciones(nodo, caja);
    return partirContenedor(nodo, caja);
  }

  // Ultimo recurso. Las subsecciones son un div con su antetitulo, su titulo y
  // su tabla dentro: no encajan en ningun caso anterior y desbordaban enteras.
  // Se parten por sus hijos -el encabezado se queda arriba y lo de abajo pasa a
  // la hoja siguiente-, y si el contenedor trae un solo hijo se baja a partirlo
  // a el, que es el caso de una envoltura con una tabla larga adentro.
  function partirContenedor(nodo, caja) {
    if (!nodo.children || !nodo.children.length) return null;
    // Hay piezas que no se parten por dentro sin quedar sin sentido: una
    // tarjeta de grafica a la mitad no es media grafica, es una grafica rota.
    if (nodo.classList.contains('dg-report-chart-card')
      || nodo.classList.contains('dg-report-map')
      || nodo.classList.contains('dg-report-kpi')) return null;
    if (nodo.children.length > 1) return partirPorHijos(nodo, nodo, caja);
    var colaHijo = partir(nodo.firstElementChild, caja);
    if (!colaHijo) return null;
    var cola = nodo.cloneNode(false);
    cola.appendChild(colaHijo);
    return cola;
  }

  function partirPorHijos(nodo, contenedor, caja) {
    if (contenedor.children.length < 2) return null;
    var quitados = [];
    while (contenedor.children.length > 1 && desborda(caja)) {
      var ultimo = contenedor.lastElementChild;
      contenedor.removeChild(ultimo);
      quitados.unshift(ultimo);
    }
    if (desborda(caja)) {
      // Queda un solo hijo y sigue sin caber. Rendirse aqui era el error: el
      // contenedor de subsecciones trae tres, y la primera sola ya es mas alta
      // que la hoja. Se baja a partir ese hijo, y lo que sobre de el viaja
      // junto con los hermanos que ya se habian apartado.
      var colaHijo = contenedor.children.length === 1
        ? partir(contenedor.firstElementChild, caja)
        : null;
      if (!colaHijo) {
        quitados.forEach(function (hijo) { contenedor.appendChild(hijo); });
        return null;
      }
      var colaMixta = nodo.cloneNode(false);
      colaMixta.appendChild(colaHijo);
      quitados.forEach(function (hijo) { colaMixta.appendChild(hijo); });
      return colaMixta;
    }
    if (!quitados.length) return null;
    var cola = nodo.cloneNode(false);
    quitados.forEach(function (hijo) { cola.appendChild(hijo); });
    return cola;
  }

  function partirTabla(tabla, caja) {
    var cuerpo = tabla.tBodies && tabla.tBodies[0];
    if (!cuerpo || cuerpo.rows.length < 2) return null;
    var quitadas = [];
    while (cuerpo.rows.length > 1 && desborda(caja)) {
      var ultima = cuerpo.rows[cuerpo.rows.length - 1];
      cuerpo.removeChild(ultima);
      quitadas.unshift(ultima);
    }
    if (desborda(caja)) {
      quitadas.forEach(function (fila) { cuerpo.appendChild(fila); });
      return null;
    }
    if (!quitadas.length) return null;
    // La cola repite el encabezado: una tabla que continúa sin sus títulos
    // obliga a regresar la hoja para saber qué se está leyendo.
    var cola = tabla.cloneNode(false);
    var cabecera = tabla.tHead;
    if (cabecera) cola.appendChild(cabecera.cloneNode(true));
    var cuerpoCola = crear('tbody');
    quitadas.forEach(function (fila) { cuerpoCola.appendChild(fila); });
    cola.appendChild(cuerpoCola);
    return cola;
  }

  function partirPorOraciones(nodo, caja) {
    var texto = nodo.textContent || '';
    var oraciones = texto.match(/[^.!?]+[.!?]*\s*/g);
    if (!oraciones || oraciones.length < 2) return null;
    var original = nodo.textContent;
    for (var corte = oraciones.length - 1; corte >= 1; corte--) {
      nodo.textContent = oraciones.slice(0, corte).join('').trim();
      if (!desborda(caja)) {
        var cola = nodo.cloneNode(false);
        cola.textContent = oraciones.slice(corte).join('').trim();
        return cola;
      }
    }
    nodo.textContent = original;
    return null;
  }

  /* ── Composición ───────────────────────────────────────────────────────── */

  function componer(origen, destino, pie) {
    var hojas = [];
    var actual = null;
    var seccionActual = null;

    function abrirHoja() {
      var hoja = crear('div', 'dg-libro-hoja');
      var caja = crear('div', 'dg-libro-caja');
      hoja.appendChild(caja);
      destino.appendChild(hoja);
      hojas.push({ hoja: hoja, caja: caja });
      actual = hojas[hojas.length - 1];
      return actual;
    }

    // Una hoja propia y completa: portada, índice, aperturas.
    function hojaEntera(nodo, clase) {
      var hoja = crear('div', 'dg-libro-hoja ' + (clase || ''));
      var caja = crear('div', 'dg-libro-caja dg-libro-caja--entera');
      caja.appendChild(nodo);
      hoja.appendChild(caja);
      destino.appendChild(hoja);
      hojas.push({ hoja: hoja, caja: caja, entera: true });
      actual = null;
      return hoja;
    }

    // Un encabezado al pie de la hoja viaja con lo que lo sigue.
    function rescatarEncabezado() {
      if (!actual) return null;
      var ultimo = actual.caja.lastElementChild;
      if (!ultimo) return null;
      var esEncabezado = ultimo.classList.contains('dg-report-section__head')
        || ultimo.classList.contains('dg-report-section__title')
        || /^H[1-4]$/.test(ultimo.tagName);
      if (!esEncabezado) return null;
      actual.caja.removeChild(ultimo);
      return ultimo;
    }

    function colocar(nodo) {
      if (!actual) abrirHoja();
      actual.caja.appendChild(nodo);
      if (!desborda(actual.caja)) return;

      var soloEnLaHoja = actual.caja.children.length === 1;
      var cola = partir(nodo, actual.caja);
      if (cola) {
        abrirHoja();
        colocar(cola);
        return;
      }
      if (soloEnLaHoja) {
        // No cabe ni en hoja limpia y no se puede partir: se deja y se marca,
        // en vez de recortarlo en silencio.
        actual.hoja.setAttribute('data-desborde', '1');
        return;
      }
      actual.caja.removeChild(nodo);
      var huerfano = rescatarEncabezado();
      abrirHoja();
      if (huerfano) actual.caja.appendChild(huerfano);
      colocar(nodo);
    }

    Array.prototype.slice.call(origen.children).forEach(function (bloque) {
      var clon = bloque.cloneNode(true);
      if (clon.classList.contains('dg-report-hero')) { hojaEntera(clon, 'dg-libro-hoja--portada'); return; }
      if (clon.classList.contains('dg-report-indice')) { hojaEntera(clon, 'dg-libro-hoja--indice'); return; }

      if (clon.classList.contains('dg-report-section')) {
        seccionActual = clon;
        // La sección se desarma: su caja no puede saltar de hoja, pero sus
        // bloques sí. Se conserva el marco visual en cada hoja que ocupa.
        var hijos = Array.prototype.slice.call(clon.children);
        abrirHoja();
        hijos.forEach(function (hijo) { colocar(hijo); });
        actual = null;
        return;
      }
      colocar(clon);
    });

    // Pie con folio en cada hoja.
    hojas.forEach(function (item, indice) {
      var pieHoja = crear('div', 'dg-libro-pie');
      var izquierda = crear('span', 'dg-libro-pie__fuente');
      izquierda.textContent = pie || 'DGMESNIE · SENER · CENACE · CFE';
      var derecha = crear('span', 'dg-libro-pie__folio');
      derecha.textContent = (indice + 1) + ' / ' + hojas.length;
      pieHoja.appendChild(izquierda);
      pieHoja.appendChild(derecha);
      item.hoja.appendChild(pieHoja);
    });

    return hojas.length;
  }

  /* ── API ───────────────────────────────────────────────────────────────── */

  // Los mapas y las graficas son <img> con data URI: al momento de componer
  // pueden no haber decodificado y miden cero, asi que la hoja se llena de mas
  // y el sobrante queda recortado. Se espera a que carguen antes de medir.
  function esperarImagenes(raiz) {
    var imagenes = Array.prototype.slice.call(raiz.querySelectorAll('img'));
    var pendientes = imagenes.filter(function (img) { return !img.complete || !img.naturalHeight; });
    if (!pendientes.length) return Promise.resolve();
    return Promise.race([
      Promise.all(pendientes.map(function (img) {
        return new Promise(function (listo) {
          img.addEventListener('load', listo, { once: true });
          img.addEventListener('error', listo, { once: true });
        });
      })),
      // Una imagen rota no debe dejar el modo libro colgado.
      new Promise(function (listo) { setTimeout(listo, 4000); })
    ]);
  }

  function activar() {
    if (estado) return Promise.resolve(estado.hojas);
    var informe = document.querySelector('.dg-report');
    if (!informe) return Promise.resolve(0);
    var flujo = informe.querySelector('.dg-report-content:not(.dg-report-indice)');
    if (!flujo) return Promise.resolve(0);

    var contenedor = crear('div', 'dg-libro');
    // El origen incluye la portada y el índice, que viven fuera del flujo.
    var origen = crear('div');
    var portada = informe.querySelector('.dg-report-hero');
    var indice = informe.querySelector('.dg-report-indice');
    if (portada) origen.appendChild(portada.cloneNode(true));
    if (indice) origen.appendChild(indice.cloneNode(true));
    Array.prototype.slice.call(flujo.children).forEach(function (hijo) {
      origen.appendChild(hijo.cloneNode(true));
    });

    informe.appendChild(contenedor);
    document.documentElement.classList.add(CLASE_ACTIVO);
    estado = { contenedor: contenedor, hojas: 0 };
    // El origen vive fuera del documento visible: se cuelga oculto para que sus
    // imagenes carguen y midan antes de componer.
    origen.style.cssText = 'position:absolute;left:-99999px;top:0;width:8.5in';
    document.body.appendChild(origen);
    return esperarImagenes(origen).then(function () {
      if (origen.parentNode) origen.parentNode.removeChild(origen);
      origen.removeAttribute('style');
      if (!estado) return 0;
      var hojas = componer(origen, contenedor, informe.dataset.pie);
      informe.scrollTop = 0;
      estado.hojas = hojas;
      return hojas;
    });
  }

  function desactivar() {
    if (!estado) return;
    if (estado.contenedor && estado.contenedor.parentNode) {
      estado.contenedor.parentNode.removeChild(estado.contenedor);
    }
    document.documentElement.classList.remove(CLASE_ACTIVO);
    estado = null;
  }

  window.dgLibro = {
    activar: activar,
    desactivar: desactivar,
    activo: function () { return !!estado; },
    alternar: function () { return estado ? (desactivar(), Promise.resolve(0)) : activar(); }
  };
})();
