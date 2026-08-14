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

  function componer(origen, destino, pie, fecha) {
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
    //
    // Reconocerlo por clase no basta: en este informe los antetitulos y los
    // titulos de subseccion son divs con estilo en linea, sin clase que los
    // delate. Se reconocen por como se ven -sin hijos, texto corto y con peso,
    // versalitas o espaciado de titulo-, que es lo que los hace titulo.
    function pareceEncabezado(nodo) {
      if (!nodo || nodo.nodeType !== 1) return false;
      if (nodo.children.length) return false;
      if (nodo.classList.contains('dg-report-section__head')
        || nodo.classList.contains('dg-report-section__title')) return true;
      if (/^H[1-6]$/.test(nodo.tagName)) return true;
      var texto = (nodo.textContent || '').trim();
      if (!texto || texto.length > 120) return false;
      var estilo = window.getComputedStyle(nodo);
      var pesado = parseInt(estilo.fontWeight, 10) >= 600;
      var versalitas = estilo.textTransform === 'uppercase';
      var espaciado = parseFloat(estilo.letterSpacing) >= 0.5;
      return pesado && (versalitas || espaciado || parseFloat(estilo.fontSize) >= 15);
    }

    // Se rescatan todos los encabezados encadenados al pie, no solo el ultimo:
    // un antetitulo, su titulo y su entrada suelen ir juntos y dejar dos de
    // ellos arriba es el mismo huerfano con otro nombre.
    function rescatarEncabezado() {
      if (!actual) return null;
      var rescatados = [];
      while (actual.caja.children.length > 1 && pareceEncabezado(actual.caja.lastElementChild)) {
        var ultimo = actual.caja.lastElementChild;
        actual.caja.removeChild(ultimo);
        rescatados.unshift(ultimo);
        if (rescatados.length >= 3) break;
      }
      return rescatados.length ? rescatados : null;
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
      var huerfanos = rescatarEncabezado();
      abrirHoja();
      if (huerfanos) huerfanos.forEach(function (h) { actual.caja.appendChild(h); });
      colocar(nodo);
    }

    Array.prototype.slice.call(origen.children).forEach(function (bloque) {
      var clon = bloque.cloneNode(true);
      if (clon.classList.contains('dg-report-hero')) {
        hojaEntera(construirPortada(clon, fecha), 'dg-libro-hoja--portada');
        return;
      }
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

    // La contraportada cierra el documento, como en el PDF.
    hojaEntera(construirContraportada(), 'dg-libro-hoja--contra');

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



  /* ── Portada y contraportada ───────────────────────────────────────────── */
  //
  // Se arman con la misma anatomia de la portada del PDF -logos, antetitulo,
  // titulo serif, cobertura con filete dorado, imagen, unidad responsable y
  // banda de corte- para que el libro en pantalla y el documento impreso sean
  // el mismo objeto. Los datos salen de la portada del informe, no de un
  // segundo origen que pudiera desfasarse.

  var TEXTO_GRACIAS = 'A todas y todos los que hacen posible, con energía y compromiso, '
    + 'construir un México más justo, soberano y sostenible.';

  function leerPortada(hero) {
    var datos = { antetitulo: '', titulo: 'Reporte de análisis territorial', meta: [] };
    if (!hero) return datos;
    var titulo = hero.querySelector('.dg-report__title');
    if (titulo) datos.titulo = titulo.textContent.trim();
    // El antetitulo es el bloque que precede al titulo. Buscarlo por texto en
    // mayusculas no sirve: van en minusculas y las sube el CSS.
    var previo = titulo && titulo.previousElementSibling;
    if (previo && !previo.children.length) datos.antetitulo = (previo.textContent || '').trim();

    // Cada chip es un <span> con dos <span> hijos. Hay que mirar los hijos
    // directos: querySelectorAll tambien devuelve los nietos y el conteo daba
    // cuatro, con lo que ningun chip se reconocia.
    Array.prototype.slice.call(hero.querySelectorAll('span')).forEach(function (chip) {
      var partes = Array.prototype.slice.call(chip.children).filter(function (n) { return n.tagName === 'SPAN'; });
      if (partes.length !== 2) return;
      var etiqueta = (partes[0].textContent || '').trim();
      var valor = (partes[1].textContent || '').trim();
      if (etiqueta && valor) datos.meta.push({ etiqueta: etiqueta, valor: valor });
    });
    return datos;
  }

  function busca(meta, patron) {
    var hit = meta.filter(function (m) { return patron.test(m.etiqueta); })[0];
    return hit ? hit.valor : '';
  }

  function construirPortada(hero, fecha) {
    var d = leerPortada(hero);
    var cobertura = busca(d.meta, /cobertura/i);
    var objetivo = busca(d.meta, /objetivo evaluado/i) || busca(d.meta, /tipo de objetivo/i);
    var ambito = busca(d.meta, /^tipo de objetivo/i) || busca(d.meta, /estado/i) || '—';

    var portada = crear('div', 'dg-libro-portada');
    portada.innerHTML =
      '<div class="dg-libro-portada__logos">'
      + '<img src="/img/pamrnt/logo_gob.png" alt="Gobierno de México">'
      + '<i></i>'
      + '<img src="/img/pamrnt/logo_sener.png" alt="Secretaría de Energía">'
      + '</div>'
      + '<div class="dg-libro-portada__antetitulo">' + (d.antetitulo || 'Subsecretaría de Planeación y Transición Energética') + '</div>'
      + '<h1 class="dg-libro-portada__titulo">' + d.titulo + '</h1>'
      + '<div class="dg-libro-portada__cobertura">'
      + (cobertura ? '<span class="dg-libro-portada__radio">' + cobertura + '</span><i></i>' : '')
      + '<span class="dg-libro-portada__objetivo">' + (objetivo || '') + '</span>'
      + '</div>'
      + '<figure class="dg-libro-portada__imagen"><img src="/tablero/assets/portada_energia.png" alt="Infraestructura energética"></figure>'
      + '<div class="dg-libro-portada__unidad">'
      + '<span>Unidad responsable</span>'
      + '<strong>DGMESNIE · Dirección General de Metodología y Estadísticas del Sistema Nacional de Información Energética</strong>'
      + '</div>'
      + '<div class="dg-libro-portada__banda">'
      + '<div><span>Corte</span><strong>' + (fecha || '') + '</strong></div>'
      + '<div><span>Ámbito</span><strong>' + ambito + '</strong></div>'
      + '<div><span>Fuente</span><strong>SENER</strong></div>'
      + '</div>';
    return portada;
  }

  function construirContraportada() {
    var contra = crear('div', 'dg-libro-contra');
    contra.innerHTML =
      '<div class="dg-libro-portada__logos">'
      + '<img src="/img/pamrnt/logo_gob.png" alt="Gobierno de México">'
      + '<i></i>'
      + '<img src="/img/pamrnt/logo_sener.png" alt="Secretaría de Energía">'
      + '</div>'
      + '<div class="dg-libro-contra__escena">'
      + '<div class="dg-libro-contra__banda"><span>Gracias</span></div>'
      + '<img class="dg-libro-contra__figura" src="/img/pamrnt/mujer.png" alt="">'
      + '</div>'
      + '<div class="dg-libro-contra__cierre">'
      + '<div class="dg-libro-contra__filete"><i></i><b></b><i></i></div>'
      + '<p>' + TEXTO_GRACIAS + '</p>'
      + '</div>';
    return contra;
  }

  /* ── Lectura por hojas ─────────────────────────────────────────────────── */
  //
  // Pasar hojas, no desplazar. En pantalla ancha se muestran dos como un
  // pliego abierto -la portada va sola, como en un libro impreso- y en
  // pantalla angosta una a la vez. El zoom se calcula para que la hoja quepa
  // entera: media hoja obliga a desplazar dentro de la hoja, que es justo lo
  // que este modo viene a evitar.

  var ANCHO_HOJA = 816;   // 8.5 in a 96 dpi
  var ALTO_HOJA = 1056;   // 11 in

  function esPliego() {
    return window.matchMedia('(min-width: 1180px)').matches;
  }

  function hojasVisibles(indice) {
    var total = estado.lista.length;
    var actual = Math.max(0, Math.min(total - 1, indice));
    if (!esPliego()) return [actual];
    // La portada abre sola: en un libro, la primera hoja no tiene pareja.
    if (actual <= 0) return [0];
    var inicio = actual % 2 === 1 ? actual : actual - 1;
    return [inicio, inicio + 1].filter(function (i) { return i < total; });
  }

  function zoom(cuantas) {
    var caja = estado.contenedor.getBoundingClientRect();
    var anchoUtil = Math.max(360, caja.width - (cuantas === 2 ? 92 : 76));
    var altoUtil = Math.max(420, caja.height - 128);
    return Math.min(1, Math.max(.42, Math.min(anchoUtil / (ANCHO_HOJA * cuantas + (cuantas === 2 ? 24 : 0)), altoUtil / ALTO_HOJA)));
  }

  function pintarLectura() {
    if (!estado || !estado.lista) return;
    var visibles = hojasVisibles(estado.indice);
    estado.lista.forEach(function (hoja, i) {
      var visible = !estado.lectura || visibles.indexOf(i) >= 0;
      hoja.hidden = !visible;
      hoja.classList.toggle('is-izquierda', estado.lectura && visibles.length === 2 && i === visibles[0]);
      hoja.classList.toggle('is-derecha', estado.lectura && visibles.length === 2 && i === visibles[1]);
    });
    estado.pliego.style.setProperty('--dg-zoom', estado.lectura ? zoom(visibles.length) : 1);
    if (estado.indicador) {
      estado.indicador.textContent = visibles.length < 2
        ? ('Hoja ' + (visibles[0] + 1) + ' de ' + estado.lista.length)
        : ('Hojas ' + (visibles[0] + 1) + '–' + (visibles[1] + 1) + ' de ' + estado.lista.length);
    }
    if (estado.anterior) estado.anterior.disabled = visibles[0] <= 0;
    if (estado.siguiente) estado.siguiente.disabled = visibles[visibles.length - 1] >= estado.lista.length - 1;
  }

  function reduceMovimiento() {
    return window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }

  // El giro se anima sobre la hoja que se abandona: es la que el lector ve
  // levantarse. Si no hay hoja de la que salir, o el usuario pidio menos
  // movimiento, se cambia sin animar.
  function irA(indice, direccion) {
    if (!estado) return;
    var destino = Math.max(0, Math.min(estado.lista.length - 1, indice));
    if (destino === estado.indice) return;
    // Se cambia de pagina PRIMERO y se despliega la hoja que llega. Animar la
    // saliente obligaba a cambiar al terminar, y ese cambio de golpe era el
    // parpadeo: aqui el contenido ya es el correcto cuando arranca el
    // movimiento, asi que no hay corte.
    estado.indice = destino;
    pintarLectura();
    estado.contenedor.scrollTop = 0;

    if (reduceMovimiento() || !estado.lectura) return;

    var entrantes = hojasVisibles(destino)
      .map(function (i) { return estado.lista[i]; })
      .filter(Boolean);
    var hoja = direccion > 0 ? entrantes[entrantes.length - 1] : entrantes[0];
    if (!hoja) return;

    var clase = direccion > 0 ? 'esta-girando' : 'esta-girando-atras';
    // El contenedor recorta para que la tira no se desborde; mientras la hoja
    // se despliega tiene que dejarla salir, o se ve cortada a media vuelta.
    estado.contenedor.classList.add('esta-pasando');
    hoja.classList.add(clase);
    var limpiar = function () {
      hoja.classList.remove(clase);
      estado.contenedor.classList.remove('esta-pasando');
      hoja.removeEventListener('animationend', limpiar);
    };
    hoja.addEventListener('animationend', limpiar);
    // Red de seguridad: sin el fin de la animacion la hoja se quedaria plegada.
    setTimeout(function () { if (hoja.classList.contains(clase)) limpiar(); }, 620);
  }

  function pasar(direccion) {
    var visibles = hojasVisibles(estado.indice);
    var salto = visibles.length === 2 ? 2 : 1;
    irA(direccion > 0 ? visibles[visibles.length - 1] + 1 : visibles[0] - salto, direccion);
  }

  function alTeclado(evento) {
    if (!estado || !estado.lectura) return;
    if (evento.key === 'ArrowRight' || evento.key === 'PageDown') { evento.preventDefault(); pasar(1); }
    else if (evento.key === 'ArrowLeft' || evento.key === 'PageUp') { evento.preventDefault(); pasar(-1); }
  }

  function montarLectura() {
    var barra = crear('div', 'dg-libro-barra');
    estado.anterior = crear('button', 'dg-libro-barra__paso');
    estado.anterior.type = 'button';
    estado.anterior.setAttribute('aria-label', 'Hoja anterior');
    estado.anterior.innerHTML = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>';
    estado.anterior.addEventListener('click', function () { pasar(-1); });

    estado.indicador = crear('span', 'dg-libro-barra__indicador');

    estado.siguiente = crear('button', 'dg-libro-barra__paso');
    estado.siguiente.type = 'button';
    estado.siguiente.setAttribute('aria-label', 'Hoja siguiente');
    estado.siguiente.innerHTML = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>';
    estado.siguiente.addEventListener('click', function () { pasar(1); });

    var modo = crear('button', 'dg-libro-barra__modo');
    modo.type = 'button';
    modo.textContent = 'Desplazar';
    modo.title = 'Ver todas las hojas en una tira continua';
    modo.addEventListener('click', function () {
      estado.lectura = !estado.lectura;
      modo.textContent = estado.lectura ? 'Desplazar' : 'Pasar hojas';
      estado.contenedor.classList.toggle('is-lectura', estado.lectura);
      pintarLectura();
    });

    barra.appendChild(estado.anterior);
    barra.appendChild(estado.indicador);
    barra.appendChild(estado.siguiente);
    barra.appendChild(modo);
    estado.contenedor.appendChild(barra);
    estado.contenedor.classList.add('is-lectura');

    estado.alRedimensionar = function () { pintarLectura(); };
    window.addEventListener('resize', estado.alRedimensionar);
    document.addEventListener('keydown', alTeclado);
    pintarLectura();
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

    var pliego = crear('div', 'dg-libro-pliego');
    contenedor.appendChild(pliego);
    informe.appendChild(contenedor);
    document.documentElement.classList.add(CLASE_ACTIVO);
    estado = { contenedor: contenedor, pliego: pliego, hojas: 0, indice: 0, lectura: true };
    // El origen vive fuera del documento visible: se cuelga oculto para que sus
    // imagenes carguen y midan antes de componer.
    origen.style.cssText = 'position:absolute;left:-99999px;top:0;width:8.5in';
    document.body.appendChild(origen);
    return esperarImagenes(origen).then(function () {
      if (origen.parentNode) origen.parentNode.removeChild(origen);
      origen.removeAttribute('style');
      if (!estado) return 0;
      var fecha = (informe.querySelector('.dg-report-toolbar__identity span') || {}).textContent || '';
      var hojas = componer(origen, pliego, informe.dataset.pie, fecha.trim());
      informe.scrollTop = 0;
      estado.hojas = hojas;
      estado.lista = Array.prototype.slice.call(pliego.querySelectorAll('.dg-libro-hoja'));
      montarLectura();
      return hojas;
    });
  }

  function desactivar() {
    if (!estado) return;
    document.removeEventListener('keydown', alTeclado);
    if (estado.alRedimensionar) window.removeEventListener('resize', estado.alRedimensionar);
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
