import { escape, fmtDate, daysFromToday, toast } from './utils.js';

let _actividades = [];
let _temas = [];
let presentationMode = false;
let presentationIndex = 0;
let presentationSlides = [];
const PRESENTATION_ZOOM_FACTOR = 1.0;

const COLORS = {
    guinda: '#8a0031',
    dorado: '#b48934',
    texto: '#1f2937',
    suave: '#667085',
    ok: '#027a48',
    aviso: '#d97706',
    riesgo: '#c0222a',
    gris: '#667085'
};

const ASSETS = [
    '/gestor/img/fondoppt.png',
    '/gestor/img/portada_ppt.png',
    '/gestor/img/logo_gob.png',
    '/gestor/img/logo_sener.png'
];

export function setPresentacionData(actividades, temas) {
    _actividades = Array.isArray(actividades) ? actividades : [];
    _temas = Array.isArray(temas) ? temas : [];
    populatePresentacionFilters();
    renderPresentacionDeck();
}

export function wirePresentacion() {
    ['pres-filtro-actividad', 'pres-filtro-responsable', 'pres-filtro-periodo'].forEach(id => {
        document.getElementById(id)?.addEventListener('change', renderPresentacionDeck);
    });
    document.getElementById('pres-filtro-busqueda')?.addEventListener('input', renderPresentacionDeck);
    document.getElementById('pres-refresh')?.addEventListener('click', renderPresentacionDeck);
    document.getElementById('pres-enviar-semanal')?.addEventListener('click', enviarReporteSemanal);
    document.getElementById('pres-presentar')?.addEventListener('click', () => enterPresentacion(0));
    document.getElementById('pres-pdf')?.addEventListener('click', descargarPresentacionPdf);
    document.getElementById('pres-ppt')?.addEventListener('click', descargarPresentacionPpt);
    document.getElementById('btnPrevSlide')?.addEventListener('click', prevPresentacionSlide);
    document.getElementById('btnNextSlide')?.addEventListener('click', nextPresentacionSlide);
    document.getElementById('btnExitPresentation')?.addEventListener('click', exitPresentacion);
    window.addEventListener('resize', fitActiveSlide);
    document.addEventListener('fullscreenchange', () => {
        if (presentationMode && !document.fullscreenElement) exitPresentacion();
    });
    document.addEventListener('keydown', (ev) => {
        if (!presentationMode) return;
        if (['ArrowRight', 'PageDown', ' '].includes(ev.key)) {
            ev.preventDefault();
            nextPresentacionSlide();
        } else if (['ArrowLeft', 'PageUp', 'Backspace'].includes(ev.key)) {
            ev.preventDefault();
            prevPresentacionSlide();
        } else if (ev.key === 'Escape') {
            ev.preventDefault();
            exitPresentacion();
        }
    });
}

function populatePresentacionFilters() {
    const actSel = document.getElementById('pres-filtro-actividad');
    const respSel = document.getElementById('pres-filtro-responsable');
    if (!actSel || !respSel) return;

    const labelResp = respSel.closest('label');
    if (window.currentUser && !window.currentUser.esAdmin) {
        if (labelResp) labelResp.style.display = 'none';
    } else {
        if (labelResp) labelResp.style.display = '';
    }

    const currentAct = actSel.value;
    const currentResp = respSel.value;

    actSel.innerHTML = '<option value="">Todas</option>' +
        _actividades
            .slice()
            .sort((a, b) => String(a.actividad || '').localeCompare(String(b.actividad || ''), 'es-MX', { sensitivity: 'base' }))
            .map(a => `<option value="${escape(a.id)}">${escape(a.actividad || 'Sin nombre')}</option>`)
            .join('');

    const responsables = new Set();
    _temas.forEach(t => {
        if (t.responsable) responsables.add(t.responsable.trim());
        if (t.corresponsables) t.corresponsables.forEach(c => { if (c.nombre) responsables.add(c.nombre.trim()); });
        if (t.etapas) t.etapas.forEach(e => { if (e.responsableNombre) responsables.add(e.responsableNombre.trim()); });
    });

    respSel.innerHTML = '<option value="">Todos</option>' +
        [...responsables]
            .filter(Boolean)
            .sort((a, b) => a.localeCompare(b, 'es-MX', { sensitivity: 'base' }))
            .map(r => `<option>${escape(r)}</option>`)
            .join('');

    actSel.value = currentAct;
    respSel.value = currentResp;
}

function readFilters() {
    return {
        actividad: document.getElementById('pres-filtro-actividad')?.value || '',
        responsable: document.getElementById('pres-filtro-responsable')?.value || '',
        periodo: document.getElementById('pres-filtro-periodo')?.value || '',
        q: (document.getElementById('pres-filtro-busqueda')?.value || '').trim().toLowerCase()
    };
}

function describeFilters(filters) {
    const parts = [];
    const actividad = filters.actividad ? getActividadName(filters.actividad) : 'Todas las actividades';
    parts.push(`Actividad: ${actividad}`);
    parts.push(`Responsable: ${filters.responsable || 'Todos'}`);
    const periodos = {
        semana_actual: 'Semana en curso',
        mes_actual: 'Mes en curso',
        '30': 'Próximos 30 días',
        vencidas: 'Solo vencidas'
    };
    parts.push(`Periodo: ${periodos[filters.periodo] || 'Todo'}`);
    if (filters.q) parts.push(`Búsqueda: ${filters.q}`);
    return parts;
}

function applyFilters(temas, filters) {
    return temas.filter(t => {
        if (filters.actividad && String(t.actividadId) !== String(filters.actividad)) return false;
        if (filters.responsable) {
            const inTema = t.responsable === filters.responsable ||
                (t.corresponsables && t.corresponsables.some(c => c.nombre === filters.responsable)) ||
                (t.etapas && t.etapas.some(e => e.responsableNombre === filters.responsable));
            if (!inTema) return false;
        }

        if (filters.periodo === '30') {
            const d = daysFromToday(t.fechaCompromiso);
            if (d === null || d < 0 || d > 30) return false;
        }
        if (filters.periodo === 'vencidas') {
            if (!isVencido(t)) return false;
        }
        if (filters.periodo === 'semana_actual' || filters.periodo === 'mes_actual') {
            const range = getNamedRange(filters.periodo);
            if (!range || !t.fechaCompromiso || t.fechaCompromiso < range.start || t.fechaCompromiso > range.end) return false;
        }

        if (filters.q) {
            const actividad = getActividadName(t.actividadId);
            const texto = [
                actividad,
                t.tema,
                t.descripcion,
                t.responsable,
                t.estatus,
                t.prioridad,
                t.comentarios,
                ...(t.etapas || []).map(e => `${e.nombre} ${e.responsableNombre}`)
            ].join(' ').toLowerCase();
            if (!texto.includes(filters.q)) return false;
        }

        return true;
    });
}

function getNamedRange(name) {
    const today = new Date();
    const pad = n => String(n).padStart(2, '0');
    const toDateOnly = d => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

    if (name === 'semana_actual') {
        const day = today.getDay();
        const monday = new Date(today);
        monday.setDate(today.getDate() - (day === 0 ? 6 : day - 1));
        const sunday = new Date(monday);
        sunday.setDate(monday.getDate() + 6);
        return { start: toDateOnly(monday), end: toDateOnly(sunday) };
    }

    if (name === 'mes_actual') {
        const first = new Date(today.getFullYear(), today.getMonth(), 1);
        const last = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        return { start: toDateOnly(first), end: toDateOnly(last) };
    }

    return null;
}

function isVencido(t) {
    if (t.estatus === 'Concluida') return false;
    const d = daysFromToday(t.fechaCompromiso);
    return typeof d === 'number' && d < 0;
}

function isPorVencer(t) {
    if (t.estatus === 'Concluida') return false;
    const d = daysFromToday(t.fechaCompromiso);
    return typeof d === 'number' && d >= 0 && d <= 7;
}

function getActividadName(id) {
    return _actividades.find(a => String(a.id) === String(id))?.actividad || 'Sin actividad';
}

function getActiveStage(t) {
    if (!Array.isArray(t.etapas) || !t.etapas.length) return null;
    return t.etapas.find(e => Number(e.avance || 0) < 100) || t.etapas[t.etapas.length - 1];
}

function calculateSummary(temas) {
    const total = temas.length;
    const vencidas = temas.filter(isVencido).length;
    const porVencer = temas.filter(isPorVencer).length;
    const concluidas = temas.filter(t => t.estatus === 'Concluida').length;
    const avance = total ? Math.round(temas.reduce((s, t) => s + Number(t.avance || 0), 0) / total) : 0;
    return { total, vencidas, porVencer, concluidas, avance };
}

function byActividad(temas) {
    return _actividades.map(a => {
        const rows = temas.filter(t => String(t.actividadId) === String(a.id));
        const resumen = calculateSummary(rows);
        return {
            id: a.id,
            actividad: a.actividad,
            responsable: a.responsablePrincipal || 'Sin responsable',
            ...resumen
        };
    }).filter(x => x.total > 0).sort((a, b) => b.vencidas - a.vencidas || b.porVencer - a.porVencer || b.total - a.total);
}

function byResponsable(temas) {
    const map = new Map();
    temas.forEach(t => {
        const stage = getActiveStage(t);
        const name = stage?.responsableNombre || t.responsable || 'Sin responsable';
        if (!map.has(name)) map.set(name, []);
        map.get(name).push(t);
        if (Array.isArray(t.etapas)) {
            t.etapas.forEach(e => {
                if (Array.isArray(e.corresponsables)) {
                    e.corresponsables.forEach(c => {
                        if (!c.nombre) return;
                        if (!map.has(c.nombre)) map.set(c.nombre, []);
                        if (!map.get(c.nombre).some(x => x.id === t.id)) map.get(c.nombre).push(t);
                    });
                }
            });
        }
    });
    return [...map.entries()].map(([responsable, rows]) => ({
        responsable,
        ...calculateSummary(rows)
    })).sort((a, b) => b.vencidas - a.vencidas || b.porVencer - a.porVencer || b.total - a.total);
}

function corresponsablesEtapa(t) {
    if (!Array.isArray(t.etapas)) return '';
    return t.etapas
        .filter(e => Array.isArray(e.corresponsables) && e.corresponsables.length)
        .map(e => `${e.nombre || 'Etapa'}: ${e.corresponsables.map(c => c.nombre).join(', ')}`)
        .join(' · ');
}

function renderPresentacionDeck() {
    const deck = document.getElementById('presentation-report-deck');
    if (!deck) return;

    const filters = readFilters();
    const temas = applyFilters(_temas, filters);
    const summary = calculateSummary(temas);
    const actividades = byActividad(temas);
    const responsables = byResponsable(temas);
    const atencion = temas
        .filter(t => isVencido(t) || isPorVencer(t) || t.bloqueada)
        .sort((a, b) => Number(isVencido(b)) - Number(isVencido(a)) || String(a.fechaCompromiso || '').localeCompare(String(b.fechaCompromiso || '')))
        .slice(0, 10);

    const date = new Date().toLocaleDateString('es-MX', { day: '2-digit', month: 'long', year: 'numeric' });

    deck.innerHTML = [
        slideCover(summary, date, describeFilters(filters)),
        slideResumen(summary, actividades.slice(0, 6)),
        slideAtencion(atencion),
        slideActividades(actividades.slice(0, 8)),
        slideResponsables(responsables.slice(0, 8))
    ].join('');

    const summaryEl = document.getElementById('pres-summary');
    if (summaryEl) {
        summaryEl.textContent = `${summary.total} temas · ${summary.vencidas} vencidos · ${summary.porVencer} por vencer`;
    }
}

function slideTop(title) {
    return `
        <div class="internal-slide__top">
            <div class="internal-slide__brand">
                <img src="/gestor/img/logo_gob.png" alt="Gobierno de México">
                <img src="/gestor/img/logo_sener.png" alt="Secretaría de Energía">
            </div>
            <div class="internal-slide__title">${escape(title)}</div>
            <div class="internal-slide__unit">DGMESNIE · Seguimiento de actividades</div>
        </div>`;
}

function slideFooter() {
    return '<div class="internal-slide__footer"><span></span><span></span><span></span></div>';
}

function slideCover(summary, date, filterParts) {
    return `
        <section class="internal-slide internal-slide--cover">
            ${slideTop('DGMESNIE · Seguimiento')}
            <div class="internal-cover-body">
                <p class="eyebrow">Seguimiento de actividades</p>
                <h2>Resumen para presentar</h2>
                <p class="unit">Dirección General de Metodologías y Estadísticas del Sistema Nacional de Información Energética</p>
                <div class="cover-filters" style="margin-top:.4rem;margin-bottom:0;">
                    Estado al <strong>${escape(date)}</strong> · ${filterParts.map(x => escape(x)).join(' · ')}
                </div>
                <div class="internal-cover-meta" style="margin-top:2.6rem;">
                    <span><strong>Temas</strong>${summary.total}</span>
                    <span><strong>Vencidos</strong>${summary.vencidas}</span>
                    <span><strong>Por vencer</strong>${summary.porVencer}</span>
                </div>
            </div>
            ${slideFooter()}
        </section>`;
}

function slideResumen(summary, actividades) {
    return `
        <section class="internal-slide internal-slide--content">
            ${slideTop('Estado actual')}
            <div class="internal-slide__body">
                <div class="slide-kpis">
                    ${kpi('Temas revisados', summary.total)}
                    ${kpi('Concluidos', summary.concluidas, 'ok')}
                    ${kpi('Por vencer', summary.porVencer, 'warn')}
                    ${kpi('Vencidos', summary.vencidas, 'issue')}
                    ${kpi('Avance promedio', `${summary.avance}%`)}
                </div>
                <div class="slide-grid">
                    <div>
                        <h3>Actividades con más atención</h3>
                        <table class="slide-table">
                            <thead><tr><th>Actividad</th><th>Temas</th><th>Por vencer</th><th>Vencidos</th><th>Avance</th></tr></thead>
                            <tbody>${actividades.map(a => `
                                <tr>
                                    <td>${escape(a.actividad)}</td>
                                    <td>${a.total}</td>
                                    <td>${a.porVencer}</td>
                                    <td>${a.vencidas}</td>
                                    <td>${a.avance}%</td>
                                </tr>`).join('') || emptyRow(5)}
                            </tbody>
                        </table>
                    </div>
                    <div>
                        <h3>Lectura rápida</h3>
                        <p class="slide-note">${buildReading(summary)}</p>
                    </div>
                </div>
            </div>
            ${slideFooter()}
        </section>`;
}

function slideAtencion(rows) {
    return `
        <section class="internal-slide internal-slide--content">
            ${slideTop('Asuntos por atender')}
            <div class="internal-slide__body">
                <table class="slide-table">
                    <thead><tr><th>Actividad</th><th>Tema</th><th>Responsable</th><th>Fecha</th><th>Estado</th><th>Etapa</th></tr></thead>
                    <tbody>${rows.map(t => {
                        const stage = getActiveStage(t);
                        return `
                            <tr>
                                <td>${escape(getActividadName(t.actividadId))}</td>
                                <td>${escape(t.tema)}</td>
                                <td>${escape(stage?.responsableNombre || t.responsable || 'Sin responsable')}</td>
                                <td>${fmtDate(t.fechaCompromiso)}</td>
                                <td>${statusPill(t)}</td>
                                <td>
                                    ${escape(stage?.nombre || 'Seguimiento simple')}
                                    ${corresponsablesEtapa(t) ? `<br><small style="color:#1e5b4f;">Co: ${escape(corresponsablesEtapa(t))}</small>` : ''}
                                </td>
                            </tr>`;
                    }).join('') || emptyRow(6)}</tbody>
                </table>
            </div>
            ${slideFooter()}
        </section>`;
}

function slideActividades(rows) {
    return `
        <section class="internal-slide internal-slide--content">
            ${slideTop('Avance por actividad')}
            <div class="internal-slide__body">
                <table class="slide-table">
                    <thead><tr><th>Actividad</th><th>Responsable</th><th>Temas</th><th>Concluidos</th><th>Por vencer</th><th>Vencidos</th><th>Avance</th></tr></thead>
                    <tbody>${rows.map(a => `
                        <tr>
                            <td>${escape(a.actividad)}</td>
                            <td>${escape(a.responsable)}</td>
                            <td>${a.total}</td>
                            <td>${a.concluidas}</td>
                            <td>${a.porVencer}</td>
                            <td>${a.vencidas}</td>
                            <td>${progressBar(a.avance)}</td>
                        </tr>`).join('') || emptyRow(7)}</tbody>
                </table>
            </div>
            ${slideFooter()}
        </section>`;
}

function slideResponsables(rows) {
    return `
        <section class="internal-slide internal-slide--content">
            ${slideTop('Avance por responsable')}
            <div class="internal-slide__body">
                <table class="slide-table">
                    <thead><tr><th>Responsable</th><th>Temas</th><th>Concluidos</th><th>Por vencer</th><th>Vencidos</th><th>Avance</th></tr></thead>
                    <tbody>${rows.map(r => `
                        <tr>
                            <td>${escape(r.responsable)}</td>
                            <td>${r.total}</td>
                            <td>${r.concluidas}</td>
                            <td>${r.porVencer}</td>
                            <td>${r.vencidas}</td>
                            <td>${progressBar(r.avance)}</td>
                        </tr>`).join('') || emptyRow(6)}</tbody>
                </table>
            </div>
            ${slideFooter()}
        </section>`;
}

function kpi(label, value, tone = '') {
    const cls = tone ? ` class="slide-kpi--${tone === 'warn' ? 'progress' : tone === 'issue' ? 'issue' : 'complete'}"` : '';
    return `<article${cls}><span>${escape(label)}</span><strong>${escape(value)}</strong></article>`;
}

function statusPill(t) {
    if (isVencido(t)) return '<span style="color:#c0222a;font-weight:700;">Vencido</span>';
    if (isPorVencer(t)) return '<span style="color:#d97706;font-weight:700;">Por vencer</span>';
    if (t.estatus === 'Concluida') return '<span style="color:#027a48;font-weight:700;">Concluido</span>';
    return `<span>${escape(t.estatus || 'Pendiente')}</span>`;
}

function progressBar(value) {
    const v = Math.max(0, Math.min(100, Number(value || 0)));
    const color = v >= 80 ? COLORS.ok : v >= 40 ? COLORS.aviso : COLORS.riesgo;
    return `
        <div style="display:flex;align-items:center;gap:8px;">
            <div style="height:7px;flex:1;background:#e5e7eb;border-radius:999px;overflow:hidden;min-width:80px;">
                <span style="display:block;height:100%;width:${v}%;background:${color};"></span>
            </div>
            <strong style="font-size:.78rem;">${v}%</strong>
        </div>`;
}

function buildReading(summary) {
    if (!summary.total) return 'No hay temas para el filtro seleccionado.';
    if (summary.vencidas > 0) return `Se recomienda revisar primero los ${summary.vencidas} temas vencidos y confirmar fechas de atención.`;
    if (summary.porVencer > 0) return `Hay ${summary.porVencer} temas próximos a vencer; conviene confirmar avances y responsables.`;
    return 'No se observan vencimientos inmediatos con los filtros actuales.';
}

function emptyRow(cols) {
    return `<tr><td colspan="${cols}" style="text-align:center;color:#667085;">Sin registros para mostrar</td></tr>`;
}

function setButtons(disabled) {
    ['pres-refresh', 'pres-presentar', 'pres-pdf', 'pres-ppt'].forEach(id => {
        const btn = document.getElementById(id);
        if (btn) btn.disabled = disabled;
    });
}

function setPresentationControls(visible) {
    const controls = document.getElementById('presentationControls');
    if (!controls) return;
    controls.style.display = visible ? 'flex' : 'none';
    controls.setAttribute('aria-hidden', visible ? 'false' : 'true');
}

function fitActiveSlide() {
    if (!presentationMode || !presentationSlides.length) return;
    const active = presentationSlides[presentationIndex];
    if (!active) return;
    const baseW = active.offsetWidth || 1280;
    const baseH = active.offsetHeight || 720;
    const baseScale = Math.min(window.innerWidth / baseW, window.innerHeight / baseH);
    const scale = Math.max(0.2, baseScale * PRESENTATION_ZOOM_FACTOR);
    active.style.setProperty('--presentation-scale', scale.toFixed(4));
}

function renderPresentationState() {
    if (!presentationSlides.length) return;
    presentationSlides.forEach((slide, idx) => {
        slide.classList.toggle('active', idx === presentationIndex);
        if (idx !== presentationIndex) slide.style.removeProperty('--presentation-scale');
    });
    const counter = document.getElementById('presentationCounter');
    if (counter) counter.textContent = `${presentationIndex + 1}/${presentationSlides.length}`;
    fitActiveSlide();
}

async function enterPresentacion(startAt = 0) {
    presentationSlides = [...document.querySelectorAll('#presentation-report-deck .internal-slide')];
    if (!presentationSlides.length) {
        toast('No hay láminas para presentar.', 'err');
        return;
    }
    presentationMode = true;
    presentationIndex = Math.min(Math.max(startAt, 0), presentationSlides.length - 1);
    document.body.classList.add('presentation-mode');
    setPresentationControls(true);
    renderPresentationState();
    if (!document.fullscreenElement && document.documentElement.requestFullscreen) {
        try { await document.documentElement.requestFullscreen(); } catch (_) { }
    }
    requestAnimationFrame(fitActiveSlide);
}

async function exitPresentacion() {
    if (!presentationMode) return;
    presentationMode = false;
    document.body.classList.remove('presentation-mode');
    presentationSlides.forEach(slide => {
        slide.classList.remove('active');
        slide.style.removeProperty('--presentation-scale');
    });
    setPresentationControls(false);
    if (document.fullscreenElement && document.exitFullscreen) {
        try { await document.exitFullscreen(); } catch (_) { }
    }
}

function nextPresentacionSlide() {
    if (!presentationMode) return;
    presentationIndex = Math.min(presentationSlides.length - 1, presentationIndex + 1);
    renderPresentationState();
}

function prevPresentacionSlide() {
    if (!presentationMode) return;
    presentationIndex = Math.max(0, presentationIndex - 1);
    renderPresentationState();
}

function setPreloader(visible, title, sub, isError = false) {
    const pre = document.getElementById('tracking-preloader');
    if (!pre) return;
    pre.classList.toggle('is-hidden', !visible);
    pre.classList.toggle('is-error', !!isError);
    pre.style.display = visible ? 'flex' : '';
    const t = document.getElementById('tracking-preloader-title');
    const s = document.getElementById('tracking-preloader-sub');
    if (t && title) t.textContent = title;
    if (s && sub) s.textContent = sub;
}

function waitForImage(url, timeout = 10000) {
    return new Promise(resolve => {
        if (!url) return resolve(false);
        const img = new Image();
        let done = false;
        const finish = ok => {
            if (done) return;
            done = true;
            resolve(ok);
        };
        const timer = setTimeout(() => finish(false), timeout);
        img.onload = () => { clearTimeout(timer); finish(true); };
        img.onerror = () => { clearTimeout(timer); finish(false); };
        img.src = url;
    });
}

async function waitForAssets(slide) {
    const urls = new Set(ASSETS);
    slide.querySelectorAll('img').forEach(img => {
        if (img.currentSrc || img.src) urls.add(img.currentSrc || img.src);
    });
    await Promise.all([...urls].map(url => waitForImage(url)));
}

async function descargarPresentacionPdf() {
    const slides = [...document.querySelectorAll('#presentation-report-deck .internal-slide')];
    if (!slides.length) {
        toast('No hay láminas para exportar.', 'err');
        return;
    }

    setButtons(true);
    setPreloader(true, 'Generando PDF', `Preparando ${slides.length} láminas...`);

    try {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({ orientation: 'landscape', unit: 'pt', format: [1280, 720] });
        for (let i = 0; i < slides.length; i++) {
            setPreloader(true, 'Generando PDF', `Procesando lámina ${i + 1} de ${slides.length}...`);
            await waitForAssets(slides[i]);
            const canvas = await html2canvas(slides[i], {
                scale: 2,
                backgroundColor: '#ffffff',
                useCORS: true,
                allowTaint: true,
                logging: false
            });
            const img = canvas.toDataURL('image/jpeg', 0.94);
            if (i > 0) pdf.addPage([1280, 720], 'landscape');
            pdf.addImage(img, 'JPEG', 0, 0, 1280, 720);
        }
        pdf.save(`para-presentar-${new Date().toISOString().slice(0, 10)}.pdf`);
        toast('PDF generado', 'ok');
    } catch (err) {
        console.error(err);
        toast('No fue posible generar el PDF.', 'err');
        setPreloader(true, 'Error al generar PDF', 'Revisa la consola para más detalle.', true);
        setTimeout(() => setPreloader(false), 2500);
    } finally {
        setButtons(false);
        setTimeout(() => setPreloader(false), 500);
    }
}

async function descargarPresentacionPpt() {
    const slides = [...document.querySelectorAll('#presentation-report-deck .internal-slide')];
    if (!slides.length) {
        toast('No hay láminas para exportar.', 'err');
        return;
    }
    if (!window.PptxGenJS) {
        toast('No se encontró el generador PPT.', 'err');
        return;
    }

    setButtons(true);
    setPreloader(true, 'Generando PPT', `Preparando ${slides.length} láminas...`);

    try {
        const pptx = new window.PptxGenJS();
        pptx.layout = 'LAYOUT_WIDE';
        pptx.author = 'DGMESNIE';
        pptx.subject = 'Seguimiento de actividades';
        pptx.title = 'Para presentar';
        pptx.company = 'Secretaría de Energía';

        for (let i = 0; i < slides.length; i++) {
            setPreloader(true, 'Generando PPT', `Procesando lámina ${i + 1} de ${slides.length}...`);
            await waitForAssets(slides[i]);
            const canvas = await html2canvas(slides[i], {
                scale: 2,
                backgroundColor: '#ffffff',
                useCORS: true,
                allowTaint: true,
                logging: false
            });
            const slide = pptx.addSlide();
            slide.addImage({ data: canvas.toDataURL('image/png'), x: 0, y: 0, w: 13.333, h: 7.5 });
        }
        await pptx.writeFile({ fileName: `para-presentar-${new Date().toISOString().slice(0, 10)}.pptx` });
        toast('PPT generado', 'ok');
    } catch (err) {
        console.error(err);
        toast('No fue posible generar el PPT.', 'err');
        setPreloader(true, 'Error al generar PPT', 'Revisa la consola para más detalle.', true);
        setTimeout(() => setPreloader(false), 2500);
    } finally {
        setButtons(false);
        setTimeout(() => setPreloader(false), 500);
    }
}

function svgToPngBase64(svgEl, width = 350, height = 260) {
    return new Promise((resolve) => {
        try {
            const clone = svgEl.cloneNode(true);
            clone.setAttribute('width', width);
            clone.setAttribute('height', height);
            clone.style.width = width + 'px';
            clone.style.height = height + 'px';

            const svgString = new XMLSerializer().serializeToString(clone);
            const svgBlob = new Blob([svgString], { type: 'image/svg+xml;charset=utf-8' });
            
            const reader = new FileReader();
            reader.onloadend = function () {
                const base64Svg = reader.result;
                const img = new Image();
                img.onload = function () {
                    const canvas = document.createElement('canvas');
                    canvas.width = width;
                    canvas.height = height;
                    const ctx = canvas.getContext('2d');
                    
                    ctx.fillStyle = '#ffffff';
                    ctx.fillRect(0, 0, width, height);
                    
                    ctx.drawImage(img, 0, 0, width, height);
                    resolve(canvas.toDataURL('image/png'));
                };
                img.onerror = function () {
                    resolve(base64Svg);
                };
                img.src = base64Svg;
            };
            reader.onerror = function () {
                resolve('');
            };
            reader.readAsDataURL(svgBlob);
        } catch (e) {
            console.error('Error al convertir SVG a PNG:', e);
            resolve('');
        }
    });
}

async function enviarReporteSemanal() {
    const btn = document.getElementById('pres-enviar-semanal');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Enviando...';
    }

    try {
        const kpis = {
            actividadesActivas: document.getElementById('kpi-temas')?.textContent || '0',
            temasRegistrados: document.getElementById('kpi-actividades')?.textContent || '0',
            temasConcluidos: document.getElementById('kpi-concluidas')?.textContent || '0',
            temasPorVencer: document.getElementById('kpi-por-vencer')?.textContent || '0',
            temasVencidos: document.getElementById('kpi-vencidas')?.textContent || '0',
            avanceGlobal: document.getElementById('kpi-avance')?.textContent || '0%'
        };

        const filters = readFilters();
        const temasFiltrados = applyFilters(_temas, filters);

        const actividadesData = byActividad(temasFiltrados).map(a => ({
            actividad: a.actividad || 'Sin nombre',
            responsable: a.responsable || 'Sin responsable',
            total: Number(a.total || 0),
            concluidos: Number(a.concluidas || 0),
            porVencer: Number(a.porVencer || 0),
            vencidos: Number(a.vencidas || 0),
            avance: Number(a.avance || 0)
        }));

        const responsablesData = byResponsable(temasFiltrados).map(r => ({
            responsable: r.responsable || 'Sin responsable',
            total: Number(r.total || 0),
            concluidos: Number(r.concluidas || 0),
            porVencer: Number(r.porVencer || 0),
            vencidos: Number(r.vencidas || 0),
            avance: Number(r.avance || 0)
        }));

        const getPng = async (selector) => {
            const svgEl = document.querySelector(`${selector} svg`);
            if (!svgEl) return '';
            return await svgToPngBase64(svgEl, 350, 260);
        };

        const payload = {
            estatusSvg: await getPng('#chart-donut-estatus'),
            avanceSvg: await getPng('#chart-gauge-avance'),
            prioridadSvg: await getPng('#chart-pie-prioridad'),
            responsablesSvg: await getPng('#chart-barras-responsables'),
            temasSvg: await getPng('#chart-stacked-temas'),
            treemapSvg: await getPng('#chart-treemap-temas'),
            kpis: kpis,
            actividades: actividadesData,
            responsables: responsablesData
        };

        const csrfToken = document.querySelector('meta[name="csrf-token"]')?.content ?? '';

        const res = await fetch('/Gestor/Api/EnviarReporteSemanal', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || `Error ${res.status}`);
        }

        const data = await res.json();
        toast(data.mensaje || 'Reporte semanal enviado correctamente', 'ok');
    } catch (err) {
        console.error(err);
        toast('Error al enviar: ' + err.message, 'err');
    } finally {
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<i class="fa-solid fa-envelope me-1"></i> Enviar reporte semanal';
        }
    }
}
