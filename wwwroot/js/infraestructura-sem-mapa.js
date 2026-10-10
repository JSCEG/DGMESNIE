/* Mapa de Infraestructura del Sector Energético (Map/Infraestructura_SEM).
 *
 * Consume las mismas fuentes que el Dashboard de Proyectos:
 *   GET /DashboardProyectos/PermisosEnergeticos?tipo=…        (GeoJSON por capa)
 *   GET /DashboardProyectos/PermisosEnergeticos/Buscar?q=…    (permiso, razón social, entidad, municipio)
 *   GET /DashboardProyectos/PermisosEnergeticos/Ficha?…       (ficha con PDF y envío por correo)
 * Los campos visibles por rol se resuelven en el servidor, igual que en el dashboard.
 *
 * El JS anterior (infraestructura_sem.js) se queda para las vistas de huracanes.
 */
(function () {
    'use strict';

    const API = '/DashboardProyectos/PermisosEnergeticos';
    const MEXICO = [[14.3, -118.4], [32.8, -86.6]];

    const MERCADOS = [
        { key: 'petroliferos', code: 'PT', label: 'Petrolíferos', sub: 'Expendio, almacenamiento y distribución', color: '#C0552E', on: true },
        { key: 'gas-lp', code: 'LP', label: 'Gas LP', sub: 'Plantas, estaciones y distribución', color: '#E0A12E', on: false },
        { key: 'gas-natural', code: 'GN', label: 'Gas natural', sub: 'Expendio y distribución', color: '#2E6FB0', on: false },
        { key: 'electricidad', code: 'PG', label: 'Generación eléctrica', sub: 'Centrales con permiso', color: '#9B2247', on: false }
    ];
    const MERCADO = Object.fromEntries(MERCADOS.map(m => [m.key, m]));

    const state = {
        map: null,
        capas: {},          // key → { data, grupo, indice, on, cargando, meta }
        estatus: 'vigentes',   // vigentes | no-vigentes | todos
        soloExactas: false,
        actualizacion: null,
        ductos: {},
        estadosLayer: null,
        municipiosListos: false,
        radar: null,
        base: null,
        hits: null,
        resaltado: null,
        medir: null,
        busqueda: null,      // último resultado completo
        filtroRes: new Set()
    };

    const $ = sel => document.querySelector(sel);
    const esc = v => String(v ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const norm = v => String(v ?? '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
    const fmtNum = v => (v == null || v === '' || isNaN(v)) ? '' : Number(v).toLocaleString('es-MX', { maximumFractionDigits: 2 });
    const fmtFecha = v => {
        if (!v) return '';
        const d = new Date(v);
        // 1900-01-01 es el valor de relleno de la fuente cuando no hay fecha.
        return isNaN(d) || d.getFullYear() < 1950 ? '' : d.toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' });
    };
    // Filtros de la capa: estatus (vigente según la carga CNE) y precisión de la coordenada.
    const pasaEstatus = p => state.estatus === 'todos' || (state.estatus === 'vigentes' ? p.vigente !== false : p.vigente === false);
    const pasaFiltros = p => pasaEstatus(p) && (!state.soloExactas || ['exacta', 'central'].includes(p.precision || 'exacta'));
    const TEXTO_PRECISION = {
        exacta: 'Ubicación del permiso (fuente oficial)',
        calle: 'Ubicación aproximada por dirección',
        municipio: 'Ubicación aproximada (centro del municipio)',
        'sin-coordenadas': 'Ubicación aproximada (centro del municipio): el permiso no trae coordenadas',
        'sin-coordenadas-estado': 'Ubicación aproximada (centro del estado): el permiso no trae coordenadas',
        central: 'Ubicación de la central en operación (el permiso no trae coordenadas)'
    };
    const ligaPermisoCne = numero => `${API}/DocumentoCne?numero=${encodeURIComponent(numero || '')}`;
    const TABLA_MERCADO = { electricidad: 'Electricidad_autorizado', petroliferos: 'Expendios_autorizado', 'gas-lp': 'GasLP_autorizado', 'gas-natural': 'GasNatural_autorizado' };
    const esCelular = () => window.matchMedia('(max-width: 768px)').matches;

    // ---------- tipo de infraestructura → ícono ----------
    // Petrolíferos y gas LP por tipo de permiso; electricidad por tecnología.
    const TECNOLOGIAS = [
        ['fotovolt', 'bi-sun', 'Fotovoltaica'],
        ['eolic', 'bi-wind', 'Eólica'],
        ['hidro', 'bi-water', 'Hidroeléctrica'],
        ['ciclo combinado', 'bi-arrow-repeat', 'Ciclo combinado'],
        ['turbogas', 'bi-fan', 'Turbogás'],
        ['combustion interna', 'bi-gear-wide-connected', 'Combustión interna'],
        ['termoelectrica', 'bi-thermometer-high', 'Termoeléctrica convencional'],
        ['cogeneracion', 'bi-recycle', 'Cogeneración'],
        ['bioenerg', 'bi-tree', 'Bioenergía'],
        ['geoterm', 'bi-thermometer-sun', 'Geotérmica'],
        ['carbo', 'bi-fire', 'Carboeléctrica'],
        ['lecho', 'bi-fire', 'Lecho fluidizado'],
        ['nucle', 'bi-radioactive', 'Nucleoeléctrica'],
        ['importacion', 'bi-arrow-left-right', 'Importación'],
        ['cinetica', 'bi-speedometer2', 'Energía cinética']
    ];
    function categoria(key, p) {
        const t = norm(p.tipoPermiso);
        if (key === 'electricidad') {
            const tec = norm(p.tecnologia);
            const hit = TECNOLOGIAS.find(([k]) => tec.includes(k));
            return hit ? { icon: hit[1], label: hit[2] } : { icon: 'bi-lightning-charge', label: p.tecnologia || 'Otra tecnología' };
        }
        if (key === 'gas-natural') {
            return t.includes('distribu') ? { icon: 'bi-truck', label: 'Distribución' } : { icon: 'bi-fuel-pump', label: 'Expendio de GNC' };
        }
        if (t.includes('aerodromo')) return { icon: 'bi-airplane', label: 'Almacenamiento en aeródromos' };
        if (t.includes('almacenamiento')) return { icon: 'bi-database', label: 'Almacenamiento' };
        if (t.includes('autoconsumo')) return { icon: 'bi-fuel-pump-diesel', label: 'Autoconsumo' };
        if (t.includes('planta')) return { icon: 'bi-building-gear', label: 'Planta de distribución' };
        if (t.includes('distribu')) return { icon: 'bi-truck', label: 'Distribución' };
        if (t.includes('expendio')) return { icon: 'bi-fuel-pump', label: key === 'gas-lp' ? 'Estación de carburación' : 'Estación de servicio' };
        return { icon: 'bi-geo-alt', label: p.tipoPermiso || 'Sin clasificar' };
    }

    const iconosCache = new Map();
    function iconoPin(m, cat, mods) {
        const extra = mods ? ` ${mods}` : '';
        const k = m.key + cat.icon + extra;
        if (!iconosCache.has(k)) {
            iconosCache.set(k, L.divIcon({
                className: 'sem-pin-wrap',
                html: `<span class="sem-pin${extra}" style="--c:${m.color}"><i class="bi ${cat.icon}"></i></span>`,
                iconSize: [26, 26],
                iconAnchor: [13, 13],
                popupAnchor: [0, -12]
            }));
        }
        return iconosCache.get(k);
    }

    // ---------- entidades (de la capa de estados del CDN) ----------
    const ENTIDADES = {};
    const MUNICIPIOS = {};   // "09017" → nombre, cuando ya cargó la capa de municipios
    function nombreEntidad(valor) {
        const clave = String(valor ?? '').trim().slice(0, 2);
        return ENTIDADES[clave] || String(valor ?? '').replace(/^\d+[\s-]*/, '');
    }
    function nombreMunicipio(valor, entidad) {
        const texto = String(valor ?? '').trim();
        const nombre = texto.replace(/^\d+[\s-]*/, '');
        if (nombre) return nombre;
        const clave = String(entidad ?? '').trim().slice(0, 2) + texto.slice(0, 3);
        return MUNICIPIOS[clave] || '';
    }

    // ---------- estado ----------
    let statusTimer = null;
    function status(msg, ms) {
        const el = $('#semStatus');
        if (!el) return;
        clearTimeout(statusTimer);
        if (!msg) { el.hidden = true; return; }
        el.textContent = msg;
        el.hidden = false;
        if (ms) statusTimer = setTimeout(() => { el.hidden = true; }, ms);
    }

    // ---------- mapa base ----------
    // Mismas bases que el Dashboard de Proyectos: Carto ya pone marca de agua y
    // Esri World_Imagery pide token, así que se usan EOX y OSM (gratuitos).
    const BASES = {
        claro: () => L.tileLayer('https://tiles.maps.eox.at/wmts/1.0.0/terrain-light_3857/default/g/{z}/{y}/{x}.jpg', { maxZoom: 19, maxNativeZoom: 16, attribution: 'Terrain Light &copy; EOX · &copy; OpenStreetMap contributors' }),
        oscuro: () => L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { subdomains: 'abc', maxZoom: 19, className: 'sem-base-oscuro', attribution: '&copy; OpenStreetMap contributors' }),
        calles: () => L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { subdomains: 'abc', maxZoom: 19, attribution: '&copy; OpenStreetMap contributors' }),
        satelite: () => L.tileLayer('https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2021_3857/default/g/{z}/{y}/{x}.jpg', { maxZoom: 19, maxNativeZoom: 16, attribution: 'Sentinel-2 cloudless 2021 &copy; EOX (datos Copernicus)' })
    };
    function setBase(nombre) {
        if (state.base) state.map.removeLayer(state.base);
        state.base = (BASES[nombre] || BASES.claro)();
        state.base.addTo(state.map);
        state.base.bringToBack();
        document.querySelectorAll('input[name="semBase"]').forEach(r => { r.checked = r.value === nombre; });
    }

    // ---------- capas de permisos ----------
    function crearGrupo(m) {
        return L.markerClusterGroup({
            chunkedLoading: true,
            showCoverageOnHover: false,
            spiderfyOnMaxZoom: true,
            disableClusteringAtZoom: 15,
            maxClusterRadius: z => (z < 8 ? 60 : 40),
            iconCreateFunction: cluster => {
                const n = cluster.getChildCount();
                const size = n < 50 ? 30 : n < 500 ? 36 : 44;
                return L.divIcon({
                    html: `<div class="sem-cluster" style="width:${size}px;height:${size}px;background:${m.color}">${n >= 1000 ? Math.round(n / 100) / 10 + 'k' : n}</div>`,
                    className: '',
                    iconSize: [size, size]
                });
            }
        });
    }

    async function cargarCapa(key) {
        const capa = state.capas[key];
        if (capa.data || capa.cargando) return capa.cargando;
        const m = MERCADO[key];
        status(`Cargando ${m.label.toLowerCase()}…`);
        // estatus=todos: el filtro Vigente / No vigente se aplica en pantalla.
        const url = `${API}?tipo=${encodeURIComponent(key)}&minLat=14&minLon=-118.5&maxLat=33&maxLon=-86.5&estatus=todos`;
        // Un reintento: la primera consulta en frío puede tardar y cortarse.
        const pedir = () => fetch(url, { credentials: 'same-origin' }).then(r => {
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            return r.json();
        });
        capa.cargando = pedir()
            .catch(() => new Promise(res => setTimeout(res, 1500)).then(pedir))
            .then(json => {
                capa.data = json.features || [];
                capa.meta = json.meta || {};
                status('');
            })
            .catch(err => {
                console.error('No se pudo cargar la capa', key, err);
                status(`No se pudo cargar ${m.label.toLowerCase()}. Intenta de nuevo.`, 5000);
                capa.on = false;
                const chk = document.querySelector(`input[data-capa="${key}"]`);
                if (chk) chk.checked = false;
            })
            .finally(() => { capa.cargando = null; });
        return capa.cargando;
    }

    function pintarCapa(key) {
        const capa = state.capas[key];
        const m = MERCADO[key];
        // Grupo nuevo en cada repintado: vaciar uno que sigue cargando por bloques
        // (chunkedLoading) deja a MarkerCluster con límites corruptos y truena al hacer zoom.
        const enMapa = capa.grupo && state.map.hasLayer(capa.grupo);
        if (capa.grupo) state.map.removeLayer(capa.grupo);
        capa.grupo = crearGrupo(m);
        if (enMapa) state.map.addLayer(capa.grupo);
        capa.indice = new Map();
        if (!capa.data) return;

        const marcadores = [];
        for (const f of capa.data) {
            const p = f.properties || {};
            if (!pasaFiltros(p)) continue;
            const [lon, lat] = f.geometry.coordinates;
            const cat = categoria(key, p);
            const prec = p.precision || 'exacta';
            const aprox = prec !== 'exacta' && prec !== 'central';
            const mods = [p.vigente === false ? 'sem-pin--novig' : '', aprox ? 'sem-pin--aprox' : '', p.operacion ? 'sem-pin--oper' : ''].filter(Boolean).join(' ');
            const titulo = aprox ? TEXTO_PRECISION[prec] : `${p.operacion ? 'En operación · ' : ''}${cat.label} · ${p.nombre}`;
            const mk = L.marker([lat, lon], { icon: iconoPin(m, cat, mods), title: titulo, keyboard: false });
            mk.feature = f;
            mk._semTipo = key;
            mk.bindPopup(() => popupHtml(key, p, lat, lon), { className: 'sem-popup', maxWidth: 340 });
            mk.on('popupopen', ev => completarLigas(ev.popup, p.numeroPermiso));
            capa.indice.set(p.numeroPermiso, mk);
            marcadores.push(mk);
        }
        capa.grupo.addLayers(marcadores);
        actualizarConteo(key, marcadores.length);
    }

    async function toggleCapa(key, on) {
        const capa = state.capas[key];
        capa.on = on;
        if (!on) {
            if (capa.grupo) state.map.removeLayer(capa.grupo);
            actualizarConteo(key);
            return;
        }
        await cargarCapa(key);
        if (!capa.on || !capa.data) return;
        if (!capa.indice) pintarCapa(key);
        state.map.addLayer(capa.grupo);
        actualizarConteo(key);
        const previo = key === 'electricidad' ? marcarEnOperacion() : Promise.resolve();
        Promise.all([previo, cargarSinUbicacion(key)]).then(() => { pintarContadorSinUbicacion(key); return aproximarSinCoordenadas(key); });
    }

    function actualizarConteo(key, n) {
        const capa = state.capas[key];
        const m = MERCADO[key];
        const el = document.querySelector(`[data-conteo="${key}"]`);
        const lista = document.querySelector(`[data-tipos="${key}"]`);
        // Cualquier cambio de capas o filtros vuelve a contar análisis y coropleta.
        programarAnalisis();
        if (!el) return;
        if (!capa.data) { el.textContent = ''; if (lista) lista.innerHTML = ''; return; }
        const visibles = n ?? (capa.indice ? capa.indice.size : 0);
        el.textContent = visibles.toLocaleString('es-MX');

        if (!lista) return;
        if (!capa.on) { lista.innerHTML = ''; return; }
        const porTipo = new Map();
        for (const f of capa.data) {
            const p = f.properties || {};
            if (!pasaFiltros(p)) continue;
            const cat = categoria(key, p);
            const actual = porTipo.get(cat.label) || { cat, n: 0 };
            actual.n++;
            porTipo.set(cat.label, actual);
        }
        lista.innerHTML = [...porTipo.values()]
            .sort((a, b) => b.n - a.n)
            .slice(0, 10)
            .map(({ cat, n }) => `<li><span title="${esc(cat.label)}"><i class="bi ${cat.icon}" style="color:${m.color}"></i> ${esc(cat.label)}</span><span>${n.toLocaleString('es-MX')}</span></li>`)
            .join('');
    }

    function fechaLarga(v) {
        if (!v) return '';
        const d = new Date(v);
        return isNaN(d) ? '' : d.toLocaleDateString('es-MX', { day: 'numeric', month: 'long', year: 'numeric' });
    }

    async function cargarActualizacion() {
        try {
            const r = await fetch(`${API}/Actualizacion`, { credentials: 'same-origin' });
            state.actualizacion = r.ok ? await r.json() : { disponible: false };
        } catch (e) {
            state.actualizacion = { disponible: false };
        }
        actualizarCorte();
    }

    function actualizarCorte() {
        const a = state.actualizacion || {};
        const general = a.disponible && a.general ? `Datos actualizados al ${fechaLarga(a.general)} · Fuente: CNE` : 'Fecha de actualización no disponible';
        document.querySelectorAll('[data-sem-actualizacion]').forEach(el => { el.textContent = general; });
        for (const m of MERCADOS) {
            const el = document.querySelector(`[data-fecha="${m.key}"]`);
            if (!el) continue;
            const f = a.porTabla ? a.porTabla[TABLA_MERCADO[m.key]] : null;
            el.textContent = f ? `Actualizado al ${fmtFecha(f)}` : 'Fecha de actualización no disponible';
        }
    }

    // ---------- centrales en operación (se cruzan con los permisos de generación) ----------
    // El archivo de centrales del dashboard es un subconjunto de los permisos de generación
    // (las 908 están en Electricidad_autorizado). No se pinta aparte: marca los permisos que
    // operan y agrega sus datos de generación; su coordenada sirve si el permiso no trae.
    const URL_CENTRALES = 'https://cdn.sassoapps.com/geojson/Centrales_El%C3%A9ctricas_privadas_y_de_CFE.geojson';
    let centralesPromesa = null;
    function cargarCentrales() {
        if (!centralesPromesa) {
            centralesPromesa = fetch(URL_CENTRALES)
                .then(r => r.ok ? r.json() : { features: [] })
                .catch(() => ({ features: [] }))
                .then(geo => {
                    const mapa = new Map();
                    (geo.features || []).forEach(f => {
                        const p = f.properties || {};
                        const num = String(p.NumeroPermiso || '').trim().toUpperCase();
                        if (!num) return;
                        const c = f.geometry && f.geometry.coordinates;
                        mapa.set(num, {
                            empresa: p['Tipo_Empresa'],
                            tecnologia: p['Tecnología_1'] || p['Tecnología'],
                            capacidadOperacion: p['Capacidad_operacion_MW'],
                            generacionNeta: p['Generacion_neta_gwh'],
                            combustible: p['Combustible_autorizado'] || p['Energetico_primario'],
                            factorPlanta: p['factor_planta_calculado'] || p['Factor_planta_reportado'],
                            lat: c ? c[1] : null,
                            lon: c ? c[0] : null
                        });
                    });
                    return mapa;
                });
        }
        return centralesPromesa;
    }

    async function marcarEnOperacion() {
        const capa = state.capas.electricidad;
        if (!capa.data || capa.conOperacion) return;
        const centrales = await cargarCentrales();
        if (capa.conOperacion) return;
        capa.conOperacion = true;
        capa.data.forEach(f => {
            const p = f.properties || {};
            const c = centrales.get(String(p.numeroPermiso || '').trim().toUpperCase());
            if (c) p.operacion = c;
        });
        pintarCapa('electricidad');
    }

    // ---------- permisos sin ubicación (no se pintan; se listan) ----------
    const sinUbicacion = {};
    async function cargarSinUbicacion(key) {
        if (sinUbicacion[key]) return sinUbicacion[key];
        try {
            const r = await fetch(`${API}/SinUbicacion?tipo=${encodeURIComponent(key)}&estatus=todos`, { credentials: 'same-origin' });
            sinUbicacion[key] = r.ok ? await r.json() : [];
        } catch (e) {
            sinUbicacion[key] = [];
        }
        return sinUbicacion[key];
    }
    // Los permisos sin coordenadas se colocan en el centro de su municipio (o de su estado)
    // y se marcan como ubicación aproximada. Se agregan a la capa como un punto más, así los
    // cuentan el análisis, el buscador y los filtros ("Solo ubicaciones exactas" los oculta).
    const centros = new Map();
    function centroDe(feature, clave) {
        if (!centros.has(clave)) {
            try { centros.set(clave, L.geoJSON(feature).getBounds().getCenter()); } catch (e) { centros.set(clave, null); }
        }
        return centros.get(clave);
    }

    async function aproximarSinCoordenadas(key) {
        const capa = state.capas[key];
        if (!capa.data || capa.aproximados) return;
        const lista = await cargarSinUbicacion(key);
        if (!lista.length) return;
        await cargarMunicipios();
        if (capa.aproximados) return;
        capa.aproximados = true;
        const mpios = new Map();
        if (typeof municipios_mapa !== 'undefined') municipios_mapa.features.forEach(f => mpios.set(f.properties.CVE_ENT + f.properties.CVE_MUN, f));
        const centrales = key === 'electricidad' ? await cargarCentrales() : new Map();
        const porLugar = new Map();
        const nuevos = [];
        for (const x of lista) {
            const ent = String(x.entidadClave || '').trim().padStart(2, '0');
            const mun = String(x.municipioClave || '').trim().padStart(3, '0');
            let centro = null;
            let precision = 'sin-coordenadas';
            const central = centrales.get(String(x.numeroPermiso || '').trim().toUpperCase());
            if (central && central.lat >= 14 && central.lat <= 33 && central.lon >= -118.5 && central.lon <= -86.5) {
                nuevos.push({
                    type: 'Feature',
                    geometry: { type: 'Point', coordinates: [central.lon, central.lat] },
                    properties: {
                        nombre: x.nombre, numeroPermiso: x.numeroPermiso, estatus: x.estatus, vigente: x.esVigente,
                        tipoPermiso: x.tipoPermiso, clasificacion: x.tipoPermiso, fechaOtorgamiento: x.fechaOtorgamiento,
                        entidad: ent, municipio: mun, entidadNombre: x.entidad, municipioNombre: x.municipio,
                        tecnologia: central.tecnologia, operacion: central, precision: 'central'
                    }
                });
                continue;
            }
            const fm = mpios.get(ent + mun);
            if (fm) centro = centroDe(fm, ent + mun);
            if (!centro) {
                const fe = featureEntidad(ent);
                if (fe) { centro = centroDe(fe, ent); precision = 'sin-coordenadas-estado'; }
            }
            if (!centro) continue;
            // Espiral pequeña para que los del mismo municipio no queden encimados.
            const k = `${centro.lat},${centro.lng}`;
            const i = porLugar.get(k) || 0;
            porLugar.set(k, i + 1);
            const ang = i * 2.399963, r = i ? 0.004 * Math.sqrt(i) : 0;
            nuevos.push({
                type: 'Feature',
                geometry: { type: 'Point', coordinates: [centro.lng + r * Math.cos(ang), centro.lat + r * Math.sin(ang)] },
                properties: {
                    nombre: x.nombre, numeroPermiso: x.numeroPermiso, estatus: x.estatus, vigente: x.esVigente,
                    tipoPermiso: x.tipoPermiso, clasificacion: x.tipoPermiso, fechaOtorgamiento: x.fechaOtorgamiento,
                    entidad: ent, municipio: mun, entidadNombre: x.entidad, municipioNombre: x.municipio,
                    precision
                }
            });
        }
        if (!nuevos.length) return;
        capa.data = capa.data.concat(nuevos);
        pintarCapa(key);
    }

    const sinUbicacionFiltrados = key => (sinUbicacion[key] || []).filter(x => pasaEstatus({ vigente: x.esVigente }));

    function pintarContadorSinUbicacion(key) {
        const el = document.querySelector(`[data-sinubic="${key}"]`);
        if (!el) return;
        if (!sinUbicacion[key]) { el.hidden = true; return; }
        const n = sinUbicacionFiltrados(key).length;
        el.hidden = false;
        el.textContent = `Sin coordenadas: ${n.toLocaleString('es-MX')} · ubicación aproximada`;
        el.disabled = n === 0;
    }

    function mostrarSinUbicacion(key) {
        $('#semResMapa').onclick = null;
        const lista = sinUbicacionFiltrados(key);
        const m = MERCADO[key];
        state.busqueda = null;
        if (state.hits) { state.map.removeLayer(state.hits); state.hits = null; }
        $('#semResTitulo').textContent = `Sin ubicación · ${m.label} (${lista.length.toLocaleString('es-MX')})`;
        $('#semResResumen').innerHTML = '<span class="sem-res-nota" style="padding:0">Sus coordenadas faltan, valen 0 o caen fuera de México. En el mapa se ubican en el centro de su municipio (o de su estado) con anillo punteado.</span>';
        const grupos = new Map();
        lista.forEach(x => {
            const k = `${x.municipio || 'Municipio sin dato'}, ${x.entidad || 'Estado sin dato'}`;
            if (!grupos.has(k)) grupos.set(k, []);
            grupos.get(k).push(x);
        });
        $('#semResLista').innerHTML = [...grupos.entries()].map(([g, xs]) => `
            <div class="sem-sinubic-grupo">${esc(g)} <span>${xs.length}</span></div>
            ${xs.map(x => `<div class="sem-res-item is-static"><span class="sem-chip sem-chip--${key}">${m.code}</span>
                <span><strong>${esc(x.nombre)}</strong><small>${esc(x.numeroPermiso)} · ${esc(x.estatus)} · <a href="${ligaPermisoCne(x.numeroPermiso)}" target="_blank" rel="noopener">Ver permiso</a></small></span></div>`).join('')}`).join('')
            || '<div class="sem-res-nota">Todos los permisos de esta capa tienen ubicación.</div>';
        $('#semResNota').textContent = '';
        $('#semResMapa').hidden = true;
        $('#semResCsv').onclick = () => {
            const q = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
            const filas = lista.map(x => [x.numeroPermiso, x.nombre, x.entidad, x.municipio, x.estatus].map(q).join(','));
            const blob = new Blob(['\ufeff' + ['Permiso', 'RazonSocial', 'Estado', 'Municipio', 'Estatus'].join(',') + '\n' + filas.join('\n')], { type: 'text/csv;charset=utf-8' });
            const a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = `permisos_sin_ubicacion_${key}.csv`;
            a.click();
            setTimeout(() => URL.revokeObjectURL(a.href), 1000);
        };
        abrirPanel('resultados');
    }

    // Ligas oficiales desde dbo.Ligas_Permisos_CNE (vía /Ligas); se piden al abrir el popup.
    // El contenido del popup es una función y popup.update() la vuelve a ejecutar: las ligas
    // entran por esta caché, no inyectándolas en el DOM (update las borraría).
    const ligasCache = new Map();
    function ligasHtml(numero) {
        const l = ligasCache.get(numero);
        // Sin liga en Ligas_Permisos_CNE (p. ej. ~113 expendios): el popup queda sin botones.
        if (!l || !l.encontrado) return '';
        const boton = (href, icono, texto, primario) => href
            ? `<a class="${primario ? 'is-primary' : ''}" href="${esc(href)}" target="_blank" rel="noopener"><i class="bi ${icono}"></i> ${texto}</a>` : '';
        const res = l.resoluciones || [];
        return boton(l.ligaPermiso, 'bi-file-earmark-text', 'Ver permiso', true)
            + boton(l.ligaResolucion, 'bi-journal-text', 'Ver resolución', false)
            + (res.length > 1 ? `<details class="sem-pop-res"><summary>Resoluciones (${res.length})</summary><ul>${res
                .map(x => `<li><a href="${esc(x.liga)}" target="_blank" rel="noopener">${esc(x.numero)}</a></li>`).join('')}</ul></details>` : '');
    }
    async function completarLigas(popup, numero) {
        if (ligasCache.has(numero)) return;
        try {
            const r = await fetch(`${API}/Ligas?numero=${encodeURIComponent(numero)}`, { credentials: 'same-origin' });
            ligasCache.set(numero, r.ok ? await r.json() : null);
            if (popup.isOpen()) popup.update();
        } catch (e) { ligasCache.set(numero, null); /* sin ligas: el popup sigue funcionando */ }
    }

    function popupHtml(key, p, lat, lon) {
        const m = MERCADO[key];
        const cat = categoria(key, p);
        const filas = [];
        const add = (k, v) => { if (v) filas.push(`<dt>${k}</dt><dd>${esc(v)}</dd>`); };
        add('Mercado', m.label);
        add('Infraestructura', cat.label);
        add('Tipo', p.tipoPermiso);
        add('Tecnología', p.tecnologia);
        add('Estatus', `${p.estatus || ''}${p.vigente === false ? ' · no vigente' : ''}`);
        add('Otorgamiento', fmtFecha(p.fechaOtorgamiento));
        const cap = p.capacidad > 0 ? `${fmtNum(p.capacidad)} ${p.unidadCapacidad || ''}`.trim()
            : (p.capacidadTexto && p.capacidadTexto !== '0' ? p.capacidadTexto : '');
        add('Capacidad', cap);
        add('Ubicación', [p.municipioNombre || nombreMunicipio(p.municipio, p.entidad), p.entidadNombre || nombreEntidad(p.entidad)].filter(Boolean).join(', '));
        add('Precisión', TEXTO_PRECISION[p.precision || 'exacta']);
        if (p.operacion) {
            const o = p.operacion;
            filas.push('<dt class="sem-pop-sep">En operación</dt><dd class="sem-pop-sep"><span class="sem-tag-oper">Central con datos de generación</span></dd>');
            add('Empresa', o.empresa);
            add('Capacidad en operación', o.capacidadOperacion ? `${fmtNum(o.capacidadOperacion)} MW` : '');
            add('Generación neta', o.generacionNeta ? `${fmtNum(o.generacionNeta)} GWh/año` : '');
            add('Combustible', o.combustible && o.combustible !== 'NULL' ? o.combustible : '');
        }

        const ficha = `${API}/Ficha?tipo=${encodeURIComponent(key)}&numeroPermiso=${encodeURIComponent(p.numeroPermiso)}`;
        const street = `https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=${lat},${lon}`;
        return `<div class="sem-pop">
            <div class="sem-pop-head"><span class="sem-pin" style="--c:${m.color}" title="${esc(cat.label)}"><i class="bi ${cat.icon}"></i></span>
                <div><strong>${esc(p.nombre)}</strong><div class="sem-pop-permiso">${esc(p.numeroPermiso)}</div></div></div>
            <dl>${filas.join('')}</dl>
            <div class="sem-pop-actions">
                <span class="sem-pop-ligas">${ligasHtml(p.numeroPermiso)}</span>
                <a href="${ficha}" target="_blank" rel="noopener"><i class="bi bi-file-earmark-pdf"></i> Ficha · PDF · correo</a>
                ${String(p.precision || 'exacta').startsWith('sin-coordenadas') ? '' : `<a href="${street}" target="_blank" rel="noopener"><i class="bi bi-person-walking"></i> Street View</a>`}
            </div></div>`;
    }

    // ---------- infraestructura complementaria ----------
    function crearDuctos() {
        const estilo = color => ({ color, weight: 2.5, opacity: .9 });
        const popup = f => {
            const p = f.properties || {};
            const km = p.longitud_ ?? p.lon;
            return `<div class="sem-pop"><strong>${esc(p.ducto)}</strong><dl>
                <dt>Servicio</dt><dd>${esc(p.servicio)}</dd><dt>Región</dt><dd>${esc(p.regin)}</dd>
                ${km ? `<dt>Longitud</dt><dd>${fmtNum(km)} km</dd>` : ''}
                ${p.capa_o ? `<dt>Capacidad</dt><dd>${esc(p.capa_o)} b/d</dd>` : ''}</dl></div>`;
        };
        if (typeof json_ductos_petroliferos_4326_0 !== 'undefined') {
            state.ductos.pet = L.geoJSON(json_ductos_petroliferos_4326_0, {
                style: estilo('#7A2E14'),
                onEachFeature: (f, l) => l.bindPopup(popup(f), { className: 'sem-popup' })
            });
        }
        if (typeof json_ductos_glp_4326_0 !== 'undefined') {
            state.ductos.glp = L.geoJSON(json_ductos_glp_4326_0, {
                style: Object.assign(estilo('#A57F2C'), { dashArray: '6 4' }),
                onEachFeature: (f, l) => l.bindPopup(popup(f), { className: 'sem-popup' })
            });
        }
    }

    function crearEstados() {
        if (typeof estados === 'undefined') return;
        for (const f of estados.features) {
            ENTIDADES[f.properties.CVE_ENT] = f.properties.NOMGEO;
        }
        state.estadosLayer = L.geoJSON(estados, {
            style: { color: '#5E5A55', weight: 1.6, fill: false, opacity: .8 },
            interactive: false
        });
    }

    async function toggleRadar(on) {
        if (!on) {
            if (state.radar) state.map.removeLayer(state.radar);
            return;
        }
        try {
            const r = await fetch('https://api.rainviewer.com/public/weather-maps.json');
            const data = await r.json();
            const frames = (data.radar && data.radar.past) || [];
            const ultimo = frames[frames.length - 1];
            if (!ultimo) throw new Error('sin cuadros');
            // RainViewer sólo sirve tejas hasta zoom 7; más cerca devuelve "Zoom Level
            // Not Supported". Con maxNativeZoom Leaflet amplía las de zoom 7.
            state.radar = L.tileLayer(`${data.host}${ultimo.path}/256/{z}/{x}/{y}/3/1_1.png`, { opacity: .7, maxNativeZoom: 7, maxZoom: 19 });
            state.radar.addTo(state.map);
            status(`Radar de lluvia · ${new Date(ultimo.time * 1000).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' })}`, 3500);
        } catch (err) {
            console.error(err);
            status('El radar de lluvia no está disponible.', 4000);
            const chk = document.querySelector('input[data-extra="radar"]');
            if (chk) chk.checked = false;
        }
    }

    function toggleExtra(clave, on) {
        const capa = clave === 'estados' ? state.estadosLayer
            : clave === 'ductos-pet' ? state.ductos.pet
            : clave === 'ductos-glp' ? state.ductos.glp
            : null;
        if (clave === 'radar') { toggleRadar(on); return; }
        if (clave === 'municipios') { toggleMunicipios(on); return; }
        if (clave === 'coropleta') { toggleCoropleta(on); return; }
        if (REDES[clave]) { toggleRed(clave, on); return; }
        if (clave === 'huracanes') { toggleHuracanes(on); return; }
        if (clave === 'viento') { toggleViento(on); return; }
        if (!capa) return;
        if (on) capa.addTo(state.map); else state.map.removeLayer(capa);
    }

    // ---------- ductos y red eléctrica (mismas fuentes que el dashboard territorial) ----------
    // Se descargan la primera vez que se encienden.
    const CDN_MAPAS = 'https://cdn.sassoapps.com/Mapas/Electricidad/';
    const lineaPopup = (titulo, filas) => `<div class="sem-pop"><strong>${esc(titulo)}</strong><dl>${filas
        .filter(([, v]) => v !== undefined && v !== null && v !== '')
        .map(([k, v]) => `<dt>${k}</dt><dd>${esc(v)}</dd>`).join('')}</dl></div>`;
    // Código cromático de tensión de las DACG/DOF, idéntico al dashboard territorial.
    const KV_COLORS = [
        { min: 400, color: '#00A9CE' },
        { min: 230, color: '#F2C94C' },
        { min: 138, color: '#3DAA5D' },
        { min: 60, color: '#C84FA3' },
        { min: 13.2, color: '#FFFFFF' },
        { min: 0, color: '#F28C38' }
    ];
    const colorKv = kv => (KV_COLORS.find(k => kv >= k.min) || KV_COLORS[KV_COLORS.length - 1]).color;
    const grosorKv = kv => kv >= 400 ? 3 : kv >= 230 ? 2.4 : kv >= 138 ? 1.8 : 1.3;

    // Demanda en tiempo real (misma escala que el dashboard) y colores institucionales de GCR.
    const colorDemanda = mw => !Number.isFinite(mw) ? '#C7C2BA' : mw < 2000 ? '#D9EFF5' : mw < 4000 ? '#83C5D2'
        : mw < 6500 ? '#2D9AB3' : mw < 9000 ? '#2E6FB0' : '#9B2247';
    const GCR_COLORS = { noreste: '#3E8174', peninsular: '#A33052', oriental: '#6FA89A', central: '#B24C6C', occidental: '#A57F2C', bcalifornia: '#7E3B52', noroeste: '#1E5B4F', norte: '#9B2247', bcsur: '#A9CDC3', mulege: '#E0CA8E' };
    const claveGcr = nombre => {
        const n = norm(nombre).replace(/[^a-z]/g, '');
        if (n.includes('bajacaliforniasur')) return 'bcsur';
        if (n.includes('mulege')) return 'mulege';
        if (n.includes('bajacalifornia')) return 'bcalifornia';
        return ['noroeste', 'noreste', 'norte', 'peninsular', 'oriental', 'occidental', 'central'].find(k => n.includes(k)) || '';
    };
    const etiquetaPoligono = (capa, html) => {
        try {
            return L.marker(capa.getBounds().getCenter(), {
                icon: L.divIcon({ className: 'sem-etiqueta', html, iconSize: null }),
                interactive: false,
                keyboard: false
            });
        } catch (e) { return null; }
    };
    const grupoConEtiquetas = (geo, etiqueta) => {
        const grupo = L.featureGroup([geo]);
        geo.eachLayer(l => { const m = etiquetaPoligono(l, etiqueta(l.feature.properties || {})); if (m) grupo.addLayer(m); });
        return grupo;
    };
    const REDES = {
        'gas-sistrangas': {
            url: CDN_MAPAS + 'Ductos%20integrados%20a%20SISTRANGAS.geojson',
            crear: data => L.geoJSON(data, { style: { color: '#2E6FB0', weight: 3, opacity: .9 } })
                .bindPopup(lineaPopup('Gasoducto integrado al SISTRANGAS', []), { className: 'sem-popup' })
        },
        'gas-importacion': {
            url: CDN_MAPAS + 'Ductos%20de%20Importacion.geojson',
            crear: data => L.geoJSON(data, { style: { color: '#6F7681', weight: 3, opacity: .9, dashArray: '8 4' } })
                .bindPopup(lineaPopup('Gasoducto de importación', []), { className: 'sem-popup' })
        },
        'red-lineas': {
            url: '/DashboardProyectos/RedElectrica/Grafo/GeoJson/Aristas',
            crear: data => {
                const renderer = L.canvas({ padding: .3, tolerance: 6 });
                const kv = f => Number(f.properties.voltage_kv) || 0;
                // Contorno oscuro debajo: el blanco (44–13.2 kV) y el amarillo se leen sobre el mapa claro.
                const contorno = L.geoJSON(data, { renderer, interactive: false, style: f => ({ color: '#2B2724', weight: grosorKv(kv(f)) + 2, opacity: .45 }) });
                const lineas = L.geoJSON(data, {
                renderer,
                style: f => ({ color: colorKv(kv(f)), weight: grosorKv(kv(f)), opacity: 1 }),
                onEachFeature: (f, l) => {
                    const p = f.properties || {};
                    l.bindPopup(lineaPopup(p.name || `${p.from_name || ''} – ${p.to_name || ''}`, [
                        ['Tensión', p.voltage_kv ? `${p.voltage_kv} kV` : ''],
                        ['Nivel', p.network_level_label],
                        ['De', p.from_name],
                        ['A', p.to_name],
                        ['Circuitos', p.circuits],
                        ['Longitud', p.length_km ? `${fmtNum(p.length_km)} km` : '']
                    ]), { className: 'sem-popup' });
                }
                });
                return L.featureGroup([contorno, lineas]);
            }
        },
        'gcr': {
            url: '/DashboardProyectos/AtlasSen/Demanda/GeoJson',
            crear: data => grupoConEtiquetas(L.geoJSON(data, {
                style: f => { const c = GCR_COLORS[claveGcr(f.properties.region)] || '#9B2247'; return { color: c, weight: 1.6, fillColor: c, fillOpacity: .28 }; },
                onEachFeature: (f, l) => l.bindTooltip(`Gerencia de Control Regional ${esc(f.properties.region)}`, { sticky: true, className: 'sem-tip' })
            }), p => `<span class="sem-etiqueta-gcr">GCR ${esc(p.region)}</span>`)
        },
        'demanda': {
            url: '/DashboardProyectos/AtlasSen/Demanda/GeoJson',
            refrescarMin: 10,
            crear: data => grupoConEtiquetas(L.geoJSON(data, {
                style: f => ({ color: '#ffffff', weight: 1.2, fillColor: colorDemanda(Number(f.properties.demand_mw)), fillOpacity: .62 }),
                onEachFeature: (f, l) => {
                    const p = f.properties || {};
                    const mw = v => v == null || v === '' ? '' : `${Number(v).toLocaleString('es-MX')} MW`;
                    l.bindPopup(lineaPopup(`Demanda · ${p.region}`, [
                        ['Demanda', mw(p.demand_mw)],
                        ['Generación', mw(p.generation_mw)],
                        ['Pronóstico', mw(p.forecast_mw)],
                        ['Balance', mw(p.balance_mw)],
                        ['Hora', p.hour != null ? `${p.hour}:00 h (${p.operating_date || ''})` : ''],
                        ['Fuente', 'CENACE · demanda en tiempo real']
                    ]), { className: 'sem-popup' });
                }
            }), p => `<span class="sem-etiqueta-dem"><b>${esc(p.region)}</b>${p.demand_mw != null ? Number(p.demand_mw).toLocaleString('es-MX') + ' MW' : ''}</span>`)
        },
        'presas': {
            url: CDN_MAPAS + 'presas.geojson',
            crear: data => L.geoJSON(data, {
                pointToLayer: (f, ll) => L.marker(ll, { icon: iconoPin({ key: 'presa', color: '#1E9CB8' }, { icon: 'bi-water' }), keyboard: false }),
                onEachFeature: (f, l) => l.bindPopup(lineaPopup(`Presa ${f.properties.id || ''}`, []), { className: 'sem-popup' })
            })
        },
        'anp': {
            url: 'https://cdn.sassoapps.com/Mapas/ANP2025.geojson',
            crear: data => L.geoJSON(data, {
                renderer: L.canvas({ padding: .3 }),
                style: { color: '#0E8A6E', weight: 1, fillColor: '#0E8A6E', fillOpacity: .18 },
                onEachFeature: (f, l) => {
                    const p = f.properties || {};
                    l.bindPopup(lineaPopup(p.NOMBRE || 'Área natural protegida', [
                        ['Categoría', p.CAT_MANEJO],
                        ['Estados', p.ESTADOS],
                        ['Superficie', p.SUPERFICIE ? `${fmtNum(p.SUPERFICIE)} ha` : '']
                    ]), { className: 'sem-popup' });
                }
            })
        },
        'red-se-trans': {
            url: '/DashboardProyectos/RedElectrica/InventarioSubestaciones/GeoJson?networkLevel=transmision',
            crear: data => capaSubestaciones(data, '#3C4859', 4.5)
        },
        'red-se-dist': {
            url: '/DashboardProyectos/RedElectrica/InventarioSubestaciones/GeoJson?networkLevel=subtransmision%2Cdistribucion%2Cindeterminado',
            crear: data => capaSubestaciones(data, '#FF9652', 3.5)
        }
    };

    function capaSubestaciones(data, color, radio) {
        const renderer = L.canvas({ padding: .3, tolerance: 4 });
        return L.geoJSON(data, {
            pointToLayer: (f, ll) => L.circleMarker(ll, { renderer, radius: radio, color: '#ffffff', weight: 1, fillColor: color, fillOpacity: .95 }),
            onEachFeature: (f, l) => {
                const p = f.properties || {};
                l.bindPopup(lineaPopup(p.name || 'Subestación', [
                    ['Tensión', p.voltage_kv ? `${p.voltage_kv} kV` : ''],
                    ['Nivel', p.network_level],
                    ['Fuente', p.source]
                ]), { className: 'sem-popup' });
            }
        });
    }

    const redesCapas = {};
    const redesTimers = {};
    async function toggleRed(clave, on) {
        const def = REDES[clave];
        clearInterval(redesTimers[clave]);
        if (!on) {
            if (redesCapas[clave]) state.map.removeLayer(redesCapas[clave]);
            return;
        }
        if (def.refrescarMin) {
            // Datos en vivo (demanda CENACE): se vuelven a pedir mientras la capa esté encendida.
            redesTimers[clave] = setInterval(async () => {
                try {
                    const r = await fetch(def.url, { credentials: 'same-origin', cache: 'no-store' });
                    if (!r.ok) return;
                    const nueva = def.crear(await r.json());
                    if (redesCapas[clave]) state.map.removeLayer(redesCapas[clave]);
                    redesCapas[clave] = nueva;
                    const chk = document.querySelector(`input[data-extra="${clave}"]`);
                    if (chk && chk.checked) nueva.addTo(state.map);
                } catch (e) { /* se reintenta en el siguiente ciclo */ }
            }, def.refrescarMin * 60 * 1000);
        }
        if (!redesCapas[clave]) {
            const chk = document.querySelector(`input[data-extra="${clave}"]`);
            status('Cargando capa…');
            try {
                const r = await fetch(def.url, { credentials: def.url.startsWith('/') ? 'same-origin' : 'omit' });
                if (!r.ok) throw new Error(`HTTP ${r.status}`);
                redesCapas[clave] = def.crear(await r.json());
                status('');
            } catch (err) {
                console.error('No se pudo cargar', clave, err);
                status('No se pudo cargar la capa. Intenta de nuevo.', 4000);
                if (chk) chk.checked = false;
                return;
            }
            if (chk && !chk.checked) return;   // la apagaron mientras cargaba
        }
        redesCapas[clave].addTo(state.map);
    }

    // ---------- municipios (8.8 MB: se cargan en segundo plano) ----------
    let municipiosPromesa = null;
    function cargarMunicipios() {
        if (typeof municipios_mapa !== 'undefined') { state.municipiosListos = true; indexarMunicipios(); return Promise.resolve(); }
        if (municipiosPromesa) return municipiosPromesa;
        municipiosPromesa = new Promise(resolve => {
            const s = document.createElement('script');
            s.src = (window.SEM_CDN || 'https://cdn.sassoapps.com') + '/Geovisualizador/shapes/municipiosminlight.js';
            s.async = true;
            s.onload = () => { state.municipiosListos = true; indexarMunicipios(); resolve(); };
            s.onerror = () => resolve();
            document.body.appendChild(s);
        });
        return municipiosPromesa;
    }

    function indexarMunicipios() {
        if (typeof municipios_mapa === 'undefined') return;
        for (const f of municipios_mapa.features) {
            MUNICIPIOS[f.properties.CVE_ENT + f.properties.CVE_MUN] = f.properties.NOM_MUN;
        }
    }

    function lugaresLocales(q) {
        const nq = norm(q);
        if (nq.length < 2) return [];
        const out = [];
        if (typeof estados !== 'undefined') {
            for (const f of estados.features) {
                if (norm(f.properties.NOMGEO).includes(nq)) {
                    out.push({ tipo: 'estado', nombre: f.properties.NOMGEO, detalle: 'Entidad federativa', feature: f });
                }
            }
        }
        if (state.municipiosListos && typeof municipios_mapa !== 'undefined') {
            for (const f of municipios_mapa.features) {
                if (out.length >= 8) break;
                const p = f.properties;
                if (norm(`${p.NOM_MUN} ${p.NOMGEO}`).includes(nq) || norm(p.NOM_MUN).includes(nq)) {
                    out.push({ tipo: 'municipio', nombre: p.NOM_MUN, detalle: p.NOMGEO, feature: f });
                }
            }
        }
        return out.slice(0, 8);
    }

    function irALugar(lugar) {
        state.map.invalidateSize({ pan: false });
        if (state.resaltado) state.map.removeLayer(state.resaltado);
        state.resaltado = L.geoJSON(lugar.feature, {
            style: { color: '#9B2247', weight: 2.5, fillColor: '#9B2247', fillOpacity: .12 },
            interactive: false
        }).addTo(state.map);
        state.map.fitBounds(state.resaltado.getBounds(), { padding: [30, 30] });
        const capa = state.resaltado;
        setTimeout(() => { if (state.resaltado === capa) { state.map.removeLayer(capa); state.resaltado = null; } }, 6000);
    }

    // ---------- ir a un permiso ----------
    async function irAPermiso(item) {
        const key = item.tipo;
        const chk = document.querySelector(`input[data-capa="${key}"]`);
        if (!state.capas[key].on) {
            if (chk) chk.checked = true;
            await toggleCapa(key, true);
        } else if (!state.capas[key].indice) {
            // La capa sigue descargándose: se espera para volar ya con el pin.
            status(`Cargando ${MERCADO[key].label.toLowerCase()}…`);
            state.map.setView([item.latitud || 23.6, item.longitud || -102.5], item.latitud ? 14 : 5);
            await toggleCapa(key, true);
        }
        if (item.latitud == null) {
            // Sin coordenadas: se espera a que quede ubicado en el centro de su municipio.
            await aproximarSinCoordenadas(key);
        }
        let mk = state.capas[key].indice && state.capas[key].indice.get(item.numeroPermiso);
        if (!mk && (state.estatus !== 'todos' || state.soloExactas)) {
            // El permiso queda fuera por filtros (no vigente o ubicación aproximada): se abren.
            setFiltros({ estatus: 'todos', soloExactas: false });
            mk = state.capas[key].indice && state.capas[key].indice.get(item.numeroPermiso);
        }
        if (mk) {
            // zoomToShowLayer de MarkerCluster espera un _icon y los circleMarker (canvas)
            // no lo tienen; a zoom 16 ya no hay agrupación, así que basta con centrar.
            state.map.invalidateSize({ pan: false });
            const ll = mk.getLatLng();
            const abrir = () => setTimeout(() => mk.openPopup(), 60);
            if (state.map.getZoom() >= 16 && state.map.getCenter().distanceTo(ll) < 5) abrir();
            else { state.map.once('moveend', abrir); state.map.setView(ll, Math.max(state.map.getZoom(), 16)); }
        } else if (item.latitud && item.longitud) {
            state.map.setView([item.latitud, item.longitud], 16);
            status('El permiso no tiene punto en el mapa con los filtros actuales.', 4000);
        } else {
            status('Este permiso no tiene coordenadas registradas.', 4000);
        }
        if (esCelular()) cerrarPanel('resultados');
    }

    // ---------- buscador ----------
    let sugTimer = null;
    let sugCtrl = null;
    let sugItems = [];
    let sugActivo = -1;

    function renderSugerencias(q, lugares, busqueda, cargando) {
        const box = $('#semSuggest');
        sugItems = [];
        let html = '';
        if (lugares.length) {
            html += '<div class="sem-suggest-group">Lugares</div>';
            for (const l of lugares) {
                sugItems.push({ lugar: l });
                html += `<button type="button" class="sem-suggest-item" data-i="${sugItems.length - 1}">
                    <span class="sem-chip sem-chip--lugar"><i class="bi bi-geo-alt"></i></span>
                    <span class="sem-suggest-main">${esc(l.nombre)}<small>${esc(l.detalle)}</small></span></button>`;
            }
        }
        if (busqueda && busqueda.resultados.length) {
            html += `<div class="sem-suggest-group">Permisos</div>`;
            for (const r of busqueda.resultados) {
                sugItems.push({ permiso: r });
                html += `<button type="button" class="sem-suggest-item" data-i="${sugItems.length - 1}">
                    <span class="sem-chip sem-chip--${r.tipo}">${MERCADO[r.tipo].code}</span>
                    <span class="sem-suggest-main">${esc(r.nombre)}<small>${esc(r.numeroPermiso)} · ${esc([r.municipio, r.entidad].filter(Boolean).join(', '))}</small></span></button>`;
            }
            html += `<button type="button" class="sem-suggest-all" data-todos="1">Ver los ${busqueda.total.toLocaleString('es-MX')} permisos que coinciden con “${esc(q)}” →</button>`;
        } else if (cargando) {
            html += '<div class="sem-suggest-empty">Buscando permisos…</div>';
        } else if (!lugares.length) {
            html += '<div class="sem-suggest-empty">Sin coincidencias. Prueba con número de permiso, razón social, estado o municipio.</div>';
        }
        box.innerHTML = html;
        box.hidden = false;
        sugActivo = -1;
    }

    function onInput() {
        const q = $('#semQ').value.trim();
        $('#semQClear').hidden = !q;
        clearTimeout(sugTimer);
        if (q.length < 2) { $('#semSuggest').hidden = true; return; }
        const lugares = lugaresLocales(q);
        renderSugerencias(q, lugares, null, true);
        sugTimer = setTimeout(async () => {
            if (sugCtrl) sugCtrl.abort();
            sugCtrl = new AbortController();
            try {
                const r = await fetch(`${API}/Buscar?q=${encodeURIComponent(q)}&limite=8`, { signal: sugCtrl.signal, credentials: 'same-origin' });
                if (!r.ok) throw new Error(`HTTP ${r.status}`);
                const data = await r.json();
                if ($('#semQ').value.trim() !== q) return;
                renderSugerencias(q, lugares, data, false);
            } catch (err) {
                if (err.name !== 'AbortError') renderSugerencias(q, lugares, null, false);
            }
        }, 260);
    }

    function elegirSugerencia(i) {
        const it = sugItems[i];
        if (!it) return;
        $('#semSuggest').hidden = true;
        if (it.lugar) irALugar(it.lugar);
        if (it.permiso) irAPermiso(it.permiso);
    }

    function onKey(e) {
        const box = $('#semSuggest');
        const botones = [...box.querySelectorAll('.sem-suggest-item')];
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            if (box.hidden || !botones.length) return;
            e.preventDefault();
            sugActivo = (sugActivo + (e.key === 'ArrowDown' ? 1 : -1) + botones.length) % botones.length;
            botones.forEach((b, i) => b.classList.toggle('is-active', i === sugActivo));
            botones[sugActivo].scrollIntoView({ block: 'nearest' });
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (sugActivo >= 0) elegirSugerencia(sugActivo);
            else buscarTodo();
        } else if (e.key === 'Escape') {
            box.hidden = true;
        }
    }

    async function buscarTodo() {
        const q = $('#semQ').value.trim();
        if (q.length < 2) return;
        $('#semSuggest').hidden = true;
        status(`Buscando “${q}”…`);
        try {
            const r = await fetch(`${API}/Buscar?q=${encodeURIComponent(q)}&limite=500`, { credentials: 'same-origin' });
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            state.busqueda = await r.json();
            state.filtroRes = new Set();
            $('#semResMapa').hidden = false;
            $('#semResMapa').onclick = null;   // listas del cono / sin ubicación ponen el suyo
            $('#semResCsv').onclick = null;
            status('');
            renderResultados();
            mostrarHits(true);
        } catch (err) {
            console.error(err);
            status('No fue posible completar la búsqueda.', 4000);
        }
    }

    function resultadosFiltrados() {
        if (!state.busqueda) return [];
        return state.busqueda.resultados.filter(r => !state.filtroRes.has(r.tipo));
    }

    function renderResultados() {
        const b = state.busqueda;
        const panel = $('#semResultados');
        if (!b) { panel.hidden = true; return; }
        $('#semResTitulo').textContent = `${b.total.toLocaleString('es-MX')} permisos · “${b.termino}”`;
        $('#semResResumen').innerHTML = b.porTipo
            .slice()
            .sort((a, c) => c.total - a.total)
            .map(c => `<button type="button" data-filtro="${c.tipo}" class="${state.filtroRes.has(c.tipo) ? 'is-off' : ''}">
                <span class="sem-chip sem-chip--${c.tipo}">${MERCADO[c.tipo].code}</span>${c.total.toLocaleString('es-MX')}</button>`)
            .join('');
        const lista = resultadosFiltrados();
        $('#semResLista').innerHTML = lista.length
            ? lista.map((r, i) => `<div class="sem-res-fila"><a class="sem-res-doc" href="${ligaPermisoCne(r.numeroPermiso)}" target="_blank" rel="noopener" title="Ver permiso en la CNE"><i class="bi bi-file-earmark-text"></i></a><button type="button" class="sem-res-item" data-res="${i}">
                <span class="sem-chip sem-chip--${r.tipo}">${MERCADO[r.tipo].code}</span>
                <span><strong>${esc(r.nombre)}</strong><small>${esc(r.numeroPermiso)}</small>
                <small>${esc([r.municipio, r.entidad].filter(Boolean).join(', '))}${r.estatus ? ' · ' + esc(r.estatus) : ''}${r.vigente === false ? ' <span class="sem-tag-novig">No vigente</span>' : ''}${r.latitud == null ? ' <span class="sem-tag-novig">Ubicación aproximada</span>' : ''}</small></span></button></div>`).join('')
            : '<div class="sem-res-nota">Sin resultados con los filtros actuales.</div>';
        $('#semResNota').textContent = b.total > b.resultados.length
            ? `Se muestran los primeros ${b.resultados.length} de ${b.total.toLocaleString('es-MX')}. Agrega otra palabra (estado, municipio) para afinar.`
            : '';
        abrirPanel('resultados');
    }

    function mostrarHits(ajustar) {
        if (state.hits) state.map.removeLayer(state.hits);
        const pts = resultadosFiltrados().filter(r => r.latitud && r.longitud);
        if (!pts.length) return;
        state.hits = L.layerGroup(pts.map(r => L.marker([r.latitud, r.longitud], {
            icon: L.divIcon({ className: '', html: '<div class="sem-hit" style="width:22px;height:22px"></div>', iconSize: [22, 22] }),
            keyboard: false,
            zIndexOffset: 1000
        }).on('click', () => irAPermiso(r)))).addTo(state.map);
        if (ajustar) {
            const bounds = L.latLngBounds(pts.map(r => [r.latitud, r.longitud]));
            state.map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 });
        }
    }

    function limpiarBusqueda() {
        $('#semQ').value = '';
        $('#semQClear').hidden = true;
        $('#semSuggest').hidden = true;
        cerrarPanel('resultados');
        state.busqueda = null;
        if (state.hits) { state.map.removeLayer(state.hits); state.hits = null; }
    }

    function exportarCsv() {
        const lista = resultadosFiltrados();
        if (!lista.length) return;
        const cols = ['Mercado', 'NumeroPermiso', 'RazonSocial', 'TipoPermiso', 'Estatus', 'Entidad', 'Municipio', 'Latitud', 'Longitud'];
        const q = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
        const filas = lista.map(r => [MERCADO[r.tipo].label, r.numeroPermiso, r.nombre, r.tipoPermiso, r.estatus, r.entidad, r.municipio, r.latitud, r.longitud].map(q).join(','));
        const blob = new Blob(['﻿' + cols.join(',') + '\n' + filas.join('\n')], { type: 'text/csv;charset=utf-8' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = `permisos_${norm(state.busqueda.termino).replace(/[^a-z0-9]+/g, '_')}.csv`;
        a.click();
        setTimeout(() => URL.revokeObjectURL(a.href), 1000);
    }

    // ---------- filtro de estatus ----------
    function setFiltros(cambios) {
        Object.assign(state, cambios);
        document.querySelectorAll('[data-estatus]').forEach(b => b.classList.toggle('is-on', b.dataset.estatus === state.estatus));
        const ex = document.querySelector('[data-exactas]');
        if (ex) ex.classList.toggle('is-on', state.soloExactas);
        for (const m of MERCADOS) {
            const capa = state.capas[m.key];
            if (!capa.data) continue;
            pintarCapa(m.key);
            if (!capa.on) actualizarConteo(m.key);
            pintarContadorSinUbicacion(m.key);
        }
    }

    // ---------- medir distancia ----------
    function toggleMedir() {
        const btn = $('#semMedir');
        if (state.medir) {
            state.map.off('click', state.medir.onClick);
            state.map.off('dblclick', state.medir.onDbl);
            state.map.doubleClickZoom.enable();
            state.map.getContainer().style.cursor = '';
            state.medir = null;
            btn.classList.remove('is-on');
            return;
        }
        const capa = L.layerGroup().addTo(state.map);
        const puntos = [];
        let linea = null;
        let etiqueta = null;
        const onClick = e => {
            puntos.push(e.latlng);
            L.circleMarker(e.latlng, { radius: 4, color: '#9B2247', weight: 2, fillColor: '#fff', fillOpacity: 1 }).addTo(capa);
            if (linea) linea.setLatLngs(puntos); else linea = L.polyline(puntos, { color: '#9B2247', weight: 2.5, dashArray: '6 4' }).addTo(capa);
            let km = 0;
            for (let i = 1; i < puntos.length; i++) km += puntos[i - 1].distanceTo(puntos[i]) / 1000;
            if (etiqueta) capa.removeLayer(etiqueta);
            if (puntos.length > 1) {
                etiqueta = L.marker(e.latlng, {
                    icon: L.divIcon({ className: '', html: `<span class="sem-measure-label">${fmtNum(km)} km</span>`, iconAnchor: [-8, 10] }),
                    interactive: false
                }).addTo(capa);
            }
        };
        const onDbl = () => toggleMedir();
        state.map.on('click', onClick);
        state.map.on('dblclick', onDbl);
        state.map.doubleClickZoom.disable();
        state.map.getContainer().style.cursor = 'crosshair';
        state.medir = { onClick, onDbl, capa };
        state.medirCapas = state.medirCapas || [];
        state.medirCapas.push(capa);
        btn.classList.add('is-on');
        status('Haz clic para trazar; doble clic para terminar.', 4000);
    }

    function limpiarHerramientas() {
        if (state.medir) toggleMedir();
        (state.medirCapas || []).forEach(c => state.map.removeLayer(c));
        state.medirCapas = [];
        if (state.resaltado) { state.map.removeLayer(state.resaltado); state.resaltado = null; }
    }

    // ---------- pantalla completa ----------
    function togglePantalla() {
        const app = $('#semApp');
        if (document.fullscreenElement) document.exitFullscreen();
        else if (app.requestFullscreen) app.requestFullscreen().catch(() => status('Tu navegador no permitió pantalla completa.', 3000));
    }

    // ---------- análisis por entidad / municipio ----------
    // Cuenta los permisos de las capas encendidas (respetando "solo vigentes").
    // Nacional: barras por entidad. Con una entidad elegida: barras por municipio.
    const RAMPA = ['#F6E3E9', '#E7B3C3', '#CF7C97', '#B0476B', '#8A1F44'];   // guinda, claro → oscuro
    const analisis = { entidad: null, verTodo: false, timer: null, coropleta: false, capaCoro: null, municipiosLayer: null, ultimo: null };

    function claveEnt(p) { return String(p.entidad ?? '').trim().slice(0, 2); }
    function claveMun(p) { return claveEnt(p) + String(p.municipio ?? '').trim().slice(0, 3); }

    function filasActivas() {
        const filas = [];
        for (const m of MERCADOS) {
            const capa = state.capas[m.key];
            if (!capa.on || !capa.data) continue;
            for (const f of capa.data) {
                const p = f.properties || {};
                if (!pasaFiltros(p)) continue;
                filas.push({ key: m.key, ent: claveEnt(p), mun: claveMun(p), munRaw: p.municipio, entRaw: p.entidad, cat: categoria(m.key, p) });
            }
        }
        return filas;
    }

    function agrupar(filas, campo) {
        const grupos = new Map();
        for (const r of filas) {
            const k = r[campo];
            if (!grupos.has(k)) grupos.set(k, { clave: k, total: 0, por: {}, muestra: r });
            const g = grupos.get(k);
            g.total++;
            g.por[r.key] = (g.por[r.key] || 0) + 1;
        }
        return [...grupos.values()].sort((a, b) => b.total - a.total);
    }

    function nombreGrupo(g, campo) {
        if (campo === 'ent') return ENTIDADES[g.clave] || nombreEntidad(g.muestra.entRaw) || 'Sin entidad';
        return MUNICIPIOS[g.clave] || nombreMunicipio(g.muestra.munRaw, g.muestra.entRaw) || `Municipio ${g.clave.slice(2)}`;
    }

    function programarAnalisis() {
        if ((clima.conos || []).length) programarCono();
        clearTimeout(analisis.timer);
        analisis.timer = setTimeout(() => {
            if (!$('#semAnalisis').hidden) renderAnalisis();
            if (analisis.coropleta) pintarCoropleta();
        }, 120);
    }

    function barraApilada(g, max, campo) {
        const presentes = MERCADOS.filter(m => g.por[m.key]);
        const segs = presentes
            .map(m => `<span class="sem-bar-seg" style="width:${(g.por[m.key] / max) * 100}%;background:var(--sem-${m.key})"></span>`)
            .join('');
        const detalle = presentes.map(m => `${m.label}: ${g.por[m.key].toLocaleString('es-MX')}`).join(' · ');
        const nombre = nombreGrupo(g, campo);
        return `<button type="button" class="sem-bar-row" data-${campo}="${esc(g.clave)}" title="${esc(nombre)} — ${esc(detalle)}">
            <span class="sem-bar-label">${esc(nombre)}</span>
            <span class="sem-bar-track">${segs}</span>
            <span class="sem-bar-val">${g.total.toLocaleString('es-MX')}</span></button>`;
    }

    function renderAnalisis() {
        const cuerpo = $('#semAnaCuerpo');
        const activas = MERCADOS.filter(m => state.capas[m.key].on && state.capas[m.key].data);
        $('#semAnaRuta').innerHTML = analisis.entidad
            ? `<button type="button" data-ana-nacional="1">México</button> <i class="bi bi-chevron-right"></i> <strong>${esc(ENTIDADES[analisis.entidad] || analisis.entidad)}</strong>`
            : '<strong>México</strong> <span>· toca una entidad para ver sus municipios</span>';
        if (!activas.length) {
            cuerpo.innerHTML = htmlConoAnalisis() + '<div class="sem-res-nota">Enciende al menos una capa de permisos para ver el análisis.</div>';
            analisis.ultimo = null;
            return;
        }

        let filas = filasActivas();
        if (analisis.entidad) filas = filas.filter(r => r.ent === analisis.entidad);
        const porMercado = {};
        filas.forEach(r => { porMercado[r.key] = (porMercado[r.key] || 0) + 1; });

        const campo = analisis.entidad ? 'mun' : 'ent';
        const grupos = agrupar(filas, campo);
        const limite = analisis.verTodo ? grupos.length : 12;
        const max = grupos.length ? grupos[0].total : 1;

        const tipos = new Map();
        for (const r of filas) {
            const k = r.key + '|' + r.cat.label;
            if (!tipos.has(k)) tipos.set(k, { key: r.key, cat: r.cat, n: 0 });
            tipos.get(k).n++;
        }
        const listaTipos = [...tipos.values()].sort((a, b) => b.n - a.n);
        const maxTipo = listaTipos.length ? listaTipos[0].n : 1;

        const kpis = activas.map(m => `<div class="sem-kpi"><span>${(porMercado[m.key] || 0).toLocaleString('es-MX')}</span>
            <small><i class="sem-dot" style="background:var(--sem-${m.key})"></i>${m.label}</small></div>`).join('');
        const barras = grupos.slice(0, limite).map(g => barraApilada(g, max, campo)).join('')
            || '<div class="sem-res-nota">Sin permisos en este ámbito.</div>';
        const mas = grupos.length > 12
            ? `<button type="button" class="sem-ana-mas" data-ana-todo="1">${analisis.verTodo ? 'Ver solo los 12 primeros' : `Ver los ${grupos.length}`}</button>`
            : '';
        const filasTipo = listaTipos.map(t => `
            <div class="sem-bar-row is-static" title="${esc(MERCADO[t.key].label)} · ${esc(t.cat.label)}: ${t.n.toLocaleString('es-MX')}">
                <span class="sem-bar-label"><i class="bi ${t.cat.icon}"></i> ${esc(t.cat.label)}</span>
                <span class="sem-bar-track"><span class="sem-bar-seg" style="width:${(t.n / maxTipo) * 100}%;background:var(--sem-${t.key})"></span></span>
                <span class="sem-bar-val">${t.n.toLocaleString('es-MX')}</span></div>`).join('');

        cuerpo.innerHTML = htmlConoAnalisis() + `
            <div class="sem-kpis">
                <div class="sem-kpi sem-kpi--total"><span>${filas.length.toLocaleString('es-MX')}</span><small>permisos</small></div>
                ${kpis}
            </div>
            <h4 class="sem-ana-h">Permisos por ${analisis.entidad ? 'municipio' : 'entidad'}</h4>
            <div class="sem-bars">${barras}</div>
            ${mas}
            <h4 class="sem-ana-h">Tipos de infraestructura</h4>
            <div class="sem-bars">${filasTipo}</div>
            <div class="sem-ana-pie">
                <button type="button" class="sem-btn" data-ana-csv="1"><i class="bi bi-filetype-csv"></i><span>Descargar tabla</span></button>
                <span>Según capas encendidas · ${{ vigentes: 'solo vigentes', 'no-vigentes': 'solo no vigentes', todos: 'vigentes y no vigentes' }[state.estatus]}${state.soloExactas ? ' · ubicaciones exactas' : ''}.</span>
            </div>`;
        analisis.ultimo = { grupos, campo };
    }

    function descargarTablaAnalisis() {
        const u = analisis.ultimo;
        if (!u) return;
        const q = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
        const cols = [u.campo === 'ent' ? 'Entidad' : 'Municipio', ...MERCADOS.map(m => m.label), 'Total'];
        const filas = u.grupos.map(g => [nombreGrupo(g, u.campo), ...MERCADOS.map(m => g.por[m.key] || 0), g.total].map(q).join(','));
        const blob = new Blob(['﻿' + cols.map(q).join(',') + '\n' + filas.join('\n')], { type: 'text/csv;charset=utf-8' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = analisis.entidad ? `permisos_municipios_${analisis.entidad}.csv` : 'permisos_por_entidad.csv';
        a.click();
        setTimeout(() => URL.revokeObjectURL(a.href), 1000);
    }

    function featureEntidad(clave) {
        return typeof estados !== 'undefined' ? estados.features.find(f => f.properties.CVE_ENT === clave) : null;
    }

    async function elegirEntidad(clave) {
        analisis.entidad = clave;
        analisis.verTodo = false;
        const f = clave && featureEntidad(clave);
        if (f) irALugar({ feature: f });
        else if (!clave) state.map.fitBounds(MEXICO);
        renderAnalisis();
        if (clave) {
            await cargarMunicipios();
            renderAnalisis();   // ya con nombres de municipio
        }
        if (analisis.coropleta) pintarCoropleta();
    }

    function elegirMunicipio(clave) {
        if (typeof municipios_mapa === 'undefined') return;
        const f = municipios_mapa.features.find(x => x.properties.CVE_ENT + x.properties.CVE_MUN === clave);
        if (f) irALugar({ feature: f });
    }

    // ---------- coropleta (estados, o municipios de la entidad elegida) ----------
    function colorRampa(n, max) {
        if (!n) return null;
        const i = Math.min(RAMPA.length - 1, Math.floor(Math.sqrt(n / max) * RAMPA.length));
        return RAMPA[i];
    }

    function pintarCoropleta() {
        if (analisis.capaCoro) { state.map.removeLayer(analisis.capaCoro); analisis.capaCoro = null; }
        const leyenda = $('#semCoroLeyenda');
        if (!analisis.coropleta) { leyenda.hidden = true; return; }

        let filas = filasActivas();
        let features, campo, claveDe;
        if (analisis.entidad && typeof municipios_mapa !== 'undefined') {
            filas = filas.filter(r => r.ent === analisis.entidad);
            features = municipios_mapa.features.filter(f => f.properties.CVE_ENT === analisis.entidad);
            campo = 'mun';
            claveDe = f => f.properties.CVE_ENT + f.properties.CVE_MUN;
        } else if (typeof estados !== 'undefined') {
            features = estados.features;
            campo = 'ent';
            claveDe = f => f.properties.CVE_ENT;
        } else {
            return;
        }

        const conteo = new Map(agrupar(filas, campo).map(g => [g.clave, g.total]));
        const max = Math.max(1, ...conteo.values());
        analisis.capaCoro = L.geoJSON({ type: 'FeatureCollection', features }, {
            renderer: L.canvas({ padding: .3 }),
            style: f => {
                const c = colorRampa(conteo.get(claveDe(f)) || 0, max);
                return { color: '#ffffff', weight: .8, fillColor: c || '#000000', fillOpacity: c ? .62 : 0 };
            },
            onEachFeature: (f, l) => {
                const n = conteo.get(claveDe(f)) || 0;
                const nombre = campo === 'ent' ? f.properties.NOMGEO : `${f.properties.NOM_MUN}, ${f.properties.NOMGEO}`;
                l.bindTooltip(`${esc(nombre)}: <strong>${n.toLocaleString('es-MX')}</strong> permisos`, { sticky: true, className: 'sem-tip' });
                if (campo === 'ent') l.on('click', () => { abrirPanel('analisis'); elegirEntidad(f.properties.CVE_ENT); });
            }
        }).addTo(state.map);
        analisis.capaCoro.bringToBack();

        const corte = Math.max(1, Math.round(max * Math.pow(1 / RAMPA.length, 2)));
        leyenda.innerHTML = `<strong>Permisos por ${campo === 'ent' ? 'entidad' : 'municipio'}</strong>
            <div class="sem-coro-escala">${RAMPA.map(c => `<i style="background:${c}"></i>`).join('')}</div>
            <div class="sem-coro-ejes"><span>1–${corte.toLocaleString('es-MX')}</span><span>${max.toLocaleString('es-MX')}</span></div>`;
        leyenda.hidden = false;
    }

    async function toggleCoropleta(on) {
        analisis.coropleta = on;
        if (on && analisis.entidad) await cargarMunicipios();
        pintarCoropleta();
    }

    async function toggleMunicipios(on) {
        if (!on) {
            if (analisis.municipiosLayer) state.map.removeLayer(analisis.municipiosLayer);
            return;
        }
        status('Cargando municipios…');
        await cargarMunicipios();
        status('');
        if (typeof municipios_mapa === 'undefined') {
            status('No se pudo cargar la capa de municipios.', 4000);
            const chk = document.querySelector('input[data-extra="municipios"]');
            if (chk) chk.checked = false;
            return;
        }
        if (!analisis.municipiosLayer) {
            analisis.municipiosLayer = L.geoJSON(municipios_mapa, {
                renderer: L.canvas({ padding: .3 }),
                style: { color: '#6F6B66', weight: .9, fill: false, opacity: .75, dashArray: '3 3' },
                interactive: false
            });
        }
        analisis.municipiosLayer.addTo(state.map);
    }

    // ---------- paneles: en pantallas angostas sólo uno a la vez ----------
    const PANELES = { capas: ['#semCapas', '#semCapasBtn'], resultados: ['#semResultados', null], analisis: ['#semAnalisis', '#semAnaBtn'] };
    const esAngosta = () => window.matchMedia('(max-width: 1023px)').matches;
    function abrirPanel(nombre) {
        const [sel, btn] = PANELES[nombre];
        $(sel).hidden = false;
        if (btn) $(btn).classList.add('is-on');
        // Resultados y análisis comparten el costado derecho.
        const choca = nombre === 'resultados' ? ['analisis'] : nombre === 'analisis' ? ['resultados'] : [];
        const cerrar = esAngosta() ? Object.keys(PANELES).filter(k => k !== nombre) : choca;
        cerrar.forEach(cerrarPanel);
        if (nombre === 'analisis') renderAnalisis();
    }
    function cerrarPanel(nombre) {
        const [sel, btn] = PANELES[nombre];
        $(sel).hidden = true;
        if (btn) $(btn).classList.remove('is-on');
    }
    function togglePanel(nombre) {
        if ($(PANELES[nombre][0]).hidden) abrirPanel(nombre); else cerrarPanel(nombre);
    }

    // ---------- clima: huracanes (NOAA NHC) y viento (Open-Meteo) ----------
    const KT_KMH = 1.852;
    const CLASES = { HU: 'Huracán', MH: 'Huracán mayor', TS: 'Tormenta tropical', TD: 'Depresión tropical', STS: 'Tormenta subtropical', STD: 'Depresión subtropical', PTC: 'Potencial ciclón tropical', PC: 'Postciclón', DB: 'Perturbación', LO: 'Baja' };
    const colorCiclon = (tipo, kt) => {
        if (kt >= 96 || tipo === 'MH') return '#8A1F44';   // categoría 3+
        if (kt >= 64 || tipo === 'HU') return '#C0552E';   // huracán 1–2
        if (kt >= 34 || /TS|STS/.test(tipo || '')) return '#E0A12E';
        return '#4A8FD6';
    };
    const categoriaSS = kt => kt >= 137 ? 5 : kt >= 113 ? 4 : kt >= 96 ? 3 : kt >= 83 ? 2 : kt >= 64 ? 1 : 0;
    const clima = { huracanes: null, timerHuracanes: null, viento: null, vientoCtrl: null, vientoTimer: null };

    async function cargarHuracanes() {
        const r = await fetch('/Clima/Huracanes', { credentials: 'same-origin' });
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        const data = await r.json();
        const grupo = L.featureGroup();
        const capas = { cono: [], historico: [], pronostico: [], punto: [] };
        (data.features || []).forEach(f => (capas[f.properties.capa] || []).push(f));

        L.geoJSON(capas.cono, { style: { color: '#5E5A55', weight: 1.2, dashArray: '4 3', fillColor: '#ffffff', fillOpacity: .28 }, interactive: false }).addTo(grupo);
        L.geoJSON(capas.historico, { style: { color: '#5E5A55', weight: 2, opacity: .8 }, interactive: false }).addTo(grupo);
        L.geoJSON(capas.pronostico, { style: { color: '#1c1b1a', weight: 2, dashArray: '6 4' }, interactive: false }).addTo(grupo);
        L.geoJSON(capas.punto, {
            pointToLayer: (f, ll) => {
                const p = f.properties;
                return L.circleMarker(ll, { radius: 6, color: '#ffffff', weight: 1.5, fillColor: colorCiclon(p.stormtype, p.maxwind), fillOpacity: 1 });
            },
            onEachFeature: (f, l) => {
                const p = f.properties;
                l.bindPopup(lineaPopup(`${p.stormname || ''} · ${p.datelbl || ''}`, [
                    ['Pronóstico', p.tau ? `+${p.tau} h` : 'Posición actual'],
                    ['Tipo', p.tcdvlp],
                    ['Viento máx.', p.maxwind ? `${Math.round(p.maxwind * KT_KMH)} km/h` : ''],
                    ['Ráfagas', p.gust ? `${Math.round(p.gust * KT_KMH)} km/h` : ''],
                    ['Presión', p.mslp && p.mslp < 9999 ? `${p.mslp} mb` : '']
                ]), { className: 'sem-popup' });
            }
        }).addTo(grupo);

        (data.tormentas || []).forEach(t => {
            if (t.latitud == null || t.longitud == null) return;
            const kmh = t.intensidadKt ? Math.round(t.intensidadKt * KT_KMH) : null;
            const cat = categoriaSS(t.intensidadKt || 0);
            const clase = CLASES[t.clasificacion] || t.clasificacion || 'Ciclón';
            L.marker([t.latitud, t.longitud], {
                icon: L.divIcon({
                    className: 'sem-pin-wrap',
                    html: `<span class="sem-ciclon" style="--c:${colorCiclon(t.clasificacion, t.intensidadKt)}"><i class="bi bi-hurricane"></i>${cat ? `<b>${cat}</b>` : ''}</span>
                           <span class="sem-ciclon-nombre">${esc(t.nombre)}</span>`,
                    iconSize: [34, 34],
                    iconAnchor: [17, 17]
                }),
                zIndexOffset: 2000
            }).bindPopup(lineaPopup(`${clase} ${t.nombre}`, [
                ['Categoría', cat ? `${cat} (Saffir-Simpson)` : ''],
                ['Viento sostenido', kmh ? `${kmh} km/h` : ''],
                ['Presión', t.presionMb ? `${t.presionMb} mb` : ''],
                ['Movimiento', t.velocidadMph ? `${Math.round(t.velocidadMph * 1.609)} km/h hacia ${t.rumbo}°` : ''],
                ['Actualizado', t.actualizado ? new Date(t.actualizado).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' }) : '']
            ]) + (t.aviso ? `<div class="sem-pop-actions"><a href="${esc(t.aviso)}" target="_blank" rel="noopener"><i class="bi bi-box-arrow-up-right"></i> Aviso NHC</a></div>` : ''),
            { className: 'sem-popup' }).addTo(grupo);
        });

        const nombres = (data.tormentas || []).map(t => `${t.nombre} (${CLASES[t.clasificacion] || t.clasificacion})`);
        const tormentaDe = f => (data.tormentas || []).find(t => t.id === f.properties.tormentaId)
            || (data.tormentas || []).find(t => norm(f.properties.stormname).includes(norm(t.nombre))) || {};
        const conos = capas.cono.map(f => {
            const t = tormentaDe(f);
            return {
                feature: f,
                nombre: t.nombre || f.properties.stormname || 'Ciclón',
                clase: CLASES[t.clasificacion] || t.clasificacion || '',
                categoria: categoriaSS(t.intensidadKt || 0)
            };
        });
        return { grupo, conos, resumen: nombres.length ? `Ciclones activos: ${nombres.join(' · ')}` : 'Sin ciclones tropicales activos en este momento.' };
    }

    async function toggleHuracanes(on) {
        clearInterval(clima.timerHuracanes);
        if (clima.huracanes) { state.map.removeLayer(clima.huracanes); clima.huracanes = null; }
        if (!on) { clima.conos = []; calcularEnCono(); return; }
        const pintar = async (avisar) => {
            try {
                const { grupo, resumen, conos } = await cargarHuracanes();
                if (!document.querySelector('input[data-extra="huracanes"]').checked) return;
                if (clima.huracanes) state.map.removeLayer(clima.huracanes);
                clima.huracanes = grupo.addTo(state.map);
                clima.conos = conos;
                calcularEnCono();
                if (avisar) status(resumen, 6000);
            } catch (err) {
                console.error(err);
                if (avisar) status('No fue posible consultar al National Hurricane Center.', 5000);
            }
        };
        status('Consultando huracanes activos…');
        await pintar(true);
        clima.timerHuracanes = setInterval(() => pintar(false), 10 * 60 * 1000);
    }

    // ---------- infraestructura dentro del cono de pronóstico ----------
    // Cruza los permisos de los cuatro mercados (con el filtro de estatus vigente) contra el
    // cono de cada ciclón activo. Si un cono toca México se descargan los mercados que falten.
    const MEXICO_BBOX = [-118.5, 14, -86.5, 33];
    const tocaMexico = b => !(b[2] < MEXICO_BBOX[0] || b[0] > MEXICO_BBOX[2] || b[3] < MEXICO_BBOX[1] || b[1] > MEXICO_BBOX[3]);
    let conoTimer = null;
    function programarCono() { clearTimeout(conoTimer); conoTimer = setTimeout(calcularEnCono, 300); }

    async function calcularEnCono() {
        const conos = (clima.conos || []).filter(c => window.turf && c.feature && c.feature.geometry);
        clima.enCono = [];
        if (!conos.length) { pintarAvisoCono(); return; }
        const cajas = conos.map(c => turf.bbox(c.feature));
        if (cajas.some(tocaMexico)) {
            await Promise.all(MERCADOS.map(m => state.capas[m.key].data ? null : cargarCapa(m.key)));
        }
        clima.enCono = conos.map((c, i) => {
            const caja = cajas[i];
            const items = [];
            const porMercado = {};
            for (const m of MERCADOS) {
                for (const f of state.capas[m.key].data || []) {
                    const p = f.properties || {};
                    if (!pasaEstatus(p)) continue;
                    const [lon, lat] = f.geometry.coordinates;
                    if (lon < caja[0] || lon > caja[2] || lat < caja[1] || lat > caja[3]) continue;
                    if (!turf.booleanPointInPolygon([lon, lat], c.feature)) continue;
                    items.push({ key: m.key, p, lat, lon });
                    porMercado[m.key] = (porMercado[m.key] || 0) + 1;
                }
            }
            return { ...c, items, porMercado };
        });
        pintarAvisoCono();
        if (!$('#semAnalisis').hidden) renderAnalisis();
    }

    const resumenMercados = pm => MERCADOS.filter(m => pm[m.key]).map(m => `${m.code} ${pm[m.key].toLocaleString('es-MX')}`).join(' · ');

    function pintarAvisoCono() {
        const el = $('#semCiclonAviso');
        if (!el) return;
        const con = (clima.enCono || []).filter(c => c.items.length);
        if (!con.length) { el.hidden = true; return; }
        el.innerHTML = `<i class="bi bi-hurricane"></i> ${con.map(c => `<span><b>${esc(c.nombre)}</b>: ${c.items.length.toLocaleString('es-MX')} permisos en el cono <small>(${resumenMercados(c.porMercado)})</small></span>`).join('')}`;
        el.hidden = false;
    }

    function htmlConoAnalisis() {
        const lista = clima.enCono || [];
        if (!lista.length) return '';
        return `<h4 class="sem-ana-h"><i class="bi bi-hurricane"></i> Infraestructura en el cono de pronóstico</h4>
            <div class="sem-cono-lista">${lista.map((c, i) => `
                <div class="sem-cono-item">
                    <div><strong>${esc(c.nombre)}</strong> <small>${esc(c.clase)}${c.categoria ? ` · cat. ${c.categoria}` : ''}</small></div>
                    <div class="sem-cono-num">${c.items.length.toLocaleString('es-MX')} <small>permisos</small></div>
                    <div class="sem-cono-mercados">${c.items.length ? resumenMercados(c.porMercado) : 'Sin permisos dentro del cono'}</div>
                    ${c.items.length ? `<div class="sem-cono-acciones">
                        <button type="button" class="sem-btn" data-cono-lista="${i}"><i class="bi bi-list-ul"></i><span>Ver lista</span></button>
                        <button type="button" class="sem-btn" data-cono-mapa="${i}"><i class="bi bi-pin-map"></i><span>Resaltar</span></button></div>` : ''}
                </div>`).join('')}</div>
            <p class="sem-ana-nota">Cono del National Hurricane Center (NOAA). Permisos ${{ vigentes: 'vigentes', 'no-vigentes': 'no vigentes', todos: 'vigentes y no vigentes' }[state.estatus]} con ubicación.</p>`;
    }

    function resaltarCono(i) {
        const c = (clima.enCono || [])[i];
        if (!c || !c.items.length) return;
        if (state.hits) state.map.removeLayer(state.hits);
        state.hits = L.layerGroup(c.items.map(x => L.marker([x.lat, x.lon], {
            icon: L.divIcon({ className: '', html: '<div class="sem-hit" style="width:22px;height:22px"></div>', iconSize: [22, 22] }),
            keyboard: false
        }).on('click', () => irAPermiso({ tipo: x.key, numeroPermiso: x.p.numeroPermiso, latitud: x.lat, longitud: x.lon })))).addTo(state.map);
        state.map.fitBounds(L.latLngBounds(c.items.map(x => [x.lat, x.lon])), { padding: [40, 40], maxZoom: 11 });
    }

    function listaCono(i) {
        const c = (clima.enCono || [])[i];
        if (!c) return;
        state.busqueda = null;
        $('#semResTitulo').textContent = `En el cono de ${c.nombre} (${c.items.length.toLocaleString('es-MX')})`;
        $('#semResResumen').innerHTML = `<span class="sem-res-nota" style="padding:0">${esc(resumenMercados(c.porMercado))}</span>`;
        const orden = MERCADOS.map(m => m.key);
        const items = c.items.slice().sort((a, b) => orden.indexOf(a.key) - orden.indexOf(b.key) || String(a.p.nombre).localeCompare(String(b.p.nombre)));
        clima.listaCono = items;
        $('#semResLista').innerHTML = items.map((x, j) => `<div class="sem-res-fila">
            <a class="sem-res-doc" href="${ligaPermisoCne(x.p.numeroPermiso)}" target="_blank" rel="noopener" title="Ver permiso"><i class="bi bi-file-earmark-text"></i></a>
            <button type="button" class="sem-res-item" data-cono-ir="${j}"><span class="sem-chip sem-chip--${x.key}">${MERCADO[x.key].code}</span>
            <span><strong>${esc(x.p.nombre)}</strong><small>${esc(x.p.numeroPermiso)} · ${esc([x.p.municipioNombre, x.p.entidadNombre].filter(Boolean).join(', '))}</small></span></button></div>`).join('');
        $('#semResNota').textContent = '';
        $('#semResMapa').hidden = false;
        $('#semResMapa').onclick = () => resaltarCono(i);
        $('#semResCsv').onclick = () => {
            const q = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
            const filas = items.map(x => [c.nombre, MERCADO[x.key].label, x.p.numeroPermiso, x.p.nombre, x.p.estatus, x.p.entidadNombre, x.p.municipioNombre, x.lat, x.lon].map(q).join(','));
            const blob = new Blob(['\ufeff' + ['Ciclon', 'Mercado', 'Permiso', 'RazonSocial', 'Estatus', 'Estado', 'Municipio', 'Latitud', 'Longitud'].join(',') + '\n' + filas.join('\n')], { type: 'text/csv;charset=utf-8' });
            const a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = `permisos_cono_${norm(c.nombre).replace(/[^a-z0-9]+/g, '_')}.csv`;
            a.click();
            setTimeout(() => URL.revokeObjectURL(a.href), 1000);
        };
        abrirPanel('resultados');
    }

    const colorViento = kmh => kmh >= 90 ? '#8A1F44' : kmh >= 60 ? '#C0552E' : kmh >= 40 ? '#E0A12E' : kmh >= 20 ? '#2E6FB0' : '#7FA7D4';

    async function pintarViento() {
        if (clima.vientoCtrl) clima.vientoCtrl.abort();
        clima.vientoCtrl = new AbortController();
        const b = state.map.getBounds();
        const q = `minLat=${Math.max(-85, b.getSouth()).toFixed(3)}&minLon=${Math.max(-180, b.getWest()).toFixed(3)}&maxLat=${Math.min(85, b.getNorth()).toFixed(3)}&maxLon=${Math.min(180, b.getEast()).toFixed(3)}`;
        try {
            const r = await fetch(`/Clima/Viento?${q}`, { credentials: 'same-origin', signal: clima.vientoCtrl.signal });
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            const puntos = await r.json();
            if (!document.querySelector('input[data-extra="viento"]').checked) return;
            const grupo = L.layerGroup(puntos.filter(p => p.velocidad != null).map(p => {
                const rot = ((p.direccion || 0) + 180) % 360;   // la dirección es "de dónde viene"
                const c = colorViento(p.velocidad);
                return L.marker([p.lat, p.lon], {
                    icon: L.divIcon({
                        className: 'sem-pin-wrap',
                        html: `<span class="sem-viento" style="--c:${c}"><i class="bi bi-arrow-up" style="transform:rotate(${rot}deg)"></i><b>${Math.round(p.velocidad)}</b></span>`,
                        iconSize: [44, 22],
                        iconAnchor: [22, 11]
                    }),
                    keyboard: false
                }).bindTooltip(`Viento ${Math.round(p.velocidad)} km/h · ráfagas ${Math.round(p.rafagas || 0)} km/h · de ${Math.round(p.direccion || 0)}°`, { className: 'sem-tip' });
            }));
            if (clima.viento) state.map.removeLayer(clima.viento);
            clima.viento = grupo.addTo(state.map);
        } catch (err) {
            if (err.name !== 'AbortError') {
                console.error(err);
                status('No fue posible consultar el viento.', 4000);
            }
        }
    }

    function programarViento() {
        clearTimeout(clima.vientoTimer);
        clima.vientoTimer = setTimeout(pintarViento, 500);
    }

    function toggleViento(on) {
        state.map.off('moveend', programarViento);
        if (clima.viento) { state.map.removeLayer(clima.viento); clima.viento = null; }
        $('#semVientoLeyenda').hidden = !on;
        if (!on) return;
        state.map.on('moveend', programarViento);
        status('Viento en superficie · Open-Meteo (km/h)', 3500);
        pintarViento();
    }

    // ---------- arranque ----------
    function construirPanelCapas() {
        const html = MERCADOS.map(m => `
            <label class="sem-layer">
                <input type="checkbox" data-capa="${m.key}" ${m.on ? 'checked' : ''}>
                <span class="sem-chip sem-chip--${m.key}">${m.code}</span>
                <span class="sem-layer-name">${m.label}<small>${m.sub}</small></span>
                <span class="sem-layer-count" data-conteo="${m.key}"></span>
            </label>
            <div class="sem-layer-meta"><span data-fecha="${m.key}"></span>
                <button type="button" class="sem-sinubic" data-sinubic="${m.key}" hidden></button></div>
            <ul class="sem-layer-tipos" data-tipos="${m.key}"></ul>`).join('');
        $('#semCapasPermisos').innerHTML = html;
    }

    function init() {
        if (!window.L || !document.getElementById('map')) return;

        MERCADOS.forEach(m => { state.capas[m.key] = { on: m.on }; });
        construirPanelCapas();

        state.map = L.map('map', { zoomControl: false, preferCanvas: true, minZoom: 4, maxZoom: 19, worldCopyJump: false });
        L.control.zoom({ position: 'bottomright' }).addTo(state.map);
        // Si el contenedor cambia de tamaño (animación del Home, scrollbar, paneles,
        // pantalla completa) Leaflet conserva el tamaño viejo y "centra" mal: el pin
        // queda en la orilla o tapado. Se le avisa cada vez que cambia.
        if (window.ResizeObserver) {
            new ResizeObserver(() => state.map.invalidateSize({ pan: false })).observe(document.getElementById('map'));
        }
        L.control.scale({ position: 'bottomright', imperial: false }).addTo(state.map);
        // La posición vive en el hash (#zoom/lat/lon) para compartir la vista, como antes.
        if (!/^#\d+\/-?[\d.]+\/-?[\d.]+/.test(location.hash)) state.map.fitBounds(MEXICO);
        if (L.Hash) new L.Hash(state.map);

        const oscuro = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        setBase(oscuro ? 'oscuro' : 'claro');

        crearEstados();
        crearDuctos();
        // Capas complementarias que vienen marcadas en el panel (límites estatales y huracanes).
        document.querySelectorAll('input[data-extra]:checked').forEach(c => toggleExtra(c.dataset.extra, true));

        // eventos: capas
        document.addEventListener('change', e => {
            const t = e.target;
            if (t.matches('input[data-capa]')) toggleCapa(t.dataset.capa, t.checked);
            else if (t.matches('input[data-extra]')) toggleExtra(t.dataset.extra, t.checked);
            else if (t.matches('input[name="semBase"]')) setBase(t.value);
        });
        document.querySelectorAll('[data-estatus]').forEach(b => b.addEventListener('click', () => setFiltros({ estatus: b.dataset.estatus })));
        const exactas = document.querySelector('[data-exactas]');
        if (exactas) exactas.addEventListener('click', () => setFiltros({ soloExactas: !state.soloExactas }));
        document.getElementById('semCapasPermisos').addEventListener('click', e => {
            const b = e.target.closest('[data-sinubic]');
            if (b) { e.preventDefault(); mostrarSinUbicacion(b.dataset.sinubic); }
        });
        // El botón "Ver en mapa" se oculta en la lista de sin ubicación; vuelve con cada búsqueda.
        $('#semQBtn').addEventListener('click', () => { $('#semResMapa').hidden = false; });
        cargarActualizacion();

        // eventos: barra
        $('#semCapasBtn').addEventListener('click', () => togglePanel('capas'));
        $('#semAnaBtn').addEventListener('click', () => togglePanel('analisis'));
        $('#semCapasCerrar').addEventListener('click', () => cerrarPanel('capas'));
        $('#semAnaCerrar').addEventListener('click', () => cerrarPanel('analisis'));
        $('#semResCerrar').addEventListener('click', () => cerrarPanel('resultados'));
        // En pantallas angostas el panel taparía el mapa: arranca cerrado.
        if (esAngosta()) cerrarPanel('capas');

        // Ligas externas del mapa (Street View, permiso, resolución, ficha): siempre en pestaña
        // nueva para no perder el mapa, aunque el navegador ignore target="_blank".
        $('#semApp').addEventListener('click', e => {
            const a = e.target.closest('a[target="_blank"]');
            if (!a || e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;
            e.preventDefault();
            window.open(a.href, '_blank', 'noopener');   // con noopener devuelve null: no se usa
        });

        // eventos: análisis
        $('#semAnalisis').addEventListener('click', e => {
            const ent = e.target.closest('[data-ent]');
            const mun = e.target.closest('[data-mun]');
            const cl = e.target.closest('[data-cono-lista]');
            const cm = e.target.closest('[data-cono-mapa]');
            if (cl) { listaCono(Number(cl.dataset.conoLista)); return; }
            if (cm) { resaltarCono(Number(cm.dataset.conoMapa)); return; }
            if (e.target.closest('[data-ana-nacional]')) elegirEntidad(null);
            else if (e.target.closest('[data-ana-todo]')) { analisis.verTodo = !analisis.verTodo; renderAnalisis(); }
            else if (e.target.closest('[data-ana-csv]')) descargarTablaAnalisis();
            else if (ent) elegirEntidad(ent.dataset.ent);
            else if (mun) elegirMunicipio(mun.dataset.mun);
        });
        $('#semMedir').addEventListener('click', toggleMedir);
        $('#semLimpiar').addEventListener('click', limpiarHerramientas);
        $('#semMexico').addEventListener('click', () => state.map.fitBounds(MEXICO));
        $('#semPantalla').addEventListener('click', togglePantalla);
        document.addEventListener('fullscreenchange', () => {
            $('#semPantalla').classList.toggle('is-on', !!document.fullscreenElement);
            setTimeout(() => state.map.invalidateSize(), 150);
        });

        // eventos: buscador
        const q = $('#semQ');
        q.addEventListener('input', onInput);
        q.addEventListener('keydown', onKey);
        let indiceCaliente = false;
        q.addEventListener('focus', () => {
            cargarMunicipios();
            // El servidor arma el índice de búsqueda en la primera consulta (~2 s); se adelanta.
            if (!indiceCaliente) { indiceCaliente = true; fetch(`${API}/Buscar?q=zz&limite=1`, { credentials: 'same-origin' }).catch(() => { }); }
            if (q.value.trim().length >= 2) onInput();
        });
        $('#semQBtn').addEventListener('click', buscarTodo);
        $('#semQClear').addEventListener('click', limpiarBusqueda);
        $('#semSuggest').addEventListener('click', e => {
            const todos = e.target.closest('[data-todos]');
            if (todos) { buscarTodo(); return; }
            const b = e.target.closest('[data-i]');
            if (b) elegirSugerencia(Number(b.dataset.i));
        });
        document.addEventListener('click', e => {
            if (!e.target.closest('.sem-search')) $('#semSuggest').hidden = true;
        });

        // eventos: resultados
        $('#semResResumen').addEventListener('click', e => {
            const b = e.target.closest('[data-filtro]');
            if (!b) return;
            const t = b.dataset.filtro;
            if (state.filtroRes.has(t)) state.filtroRes.delete(t); else state.filtroRes.add(t);
            renderResultados();
            mostrarHits(false);
        });
        $('#semCiclonAviso').addEventListener('click', () => abrirPanel('analisis'));
        $('#semResLista').addEventListener('click', e => {
            const ci = e.target.closest('[data-cono-ir]');
            if (ci) {
                const x = clima.listaCono[Number(ci.dataset.conoIr)];
                if (x) irAPermiso({ tipo: x.key, numeroPermiso: x.p.numeroPermiso, latitud: x.lat, longitud: x.lon });
                return;
            }
            const b = e.target.closest('[data-res]');
            if (b) irAPermiso(resultadosFiltrados()[Number(b.dataset.res)]);
        });
        $('#semResMapa').addEventListener('click', () => { if (state.busqueda) mostrarHits(true); });
        $('#semResCsv').addEventListener('click', () => { if (state.busqueda) exportarCsv(); });

        window.addEventListener('themechange', () => {
            const dark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
            const actual = document.querySelector('input[name="semBase"]:checked');
            if (actual && (actual.value === 'claro' || actual.value === 'oscuro')) setBase(dark ? 'oscuro' : 'claro');
        });

        // capas encendidas por omisión
        MERCADOS.filter(m => m.on).forEach(m => toggleCapa(m.key, true));

        // Municipios en segundo plano para que el buscador ya los tenga.
        const idle = window.requestIdleCallback || (fn => setTimeout(fn, 2500));
        idle(() => cargarMunicipios());

        // Si se llega con ?q=… se ejecuta la búsqueda.
        const qs = new URLSearchParams(location.search).get('q');
        if (qs) { q.value = qs; $('#semQClear').hidden = false; buscarTodo(); }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
