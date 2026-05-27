// Reportes institucionales — deck de slides estilo PPT con filtros y descarga PDF/PPT/Excel.

import { escape, fmtDate, daysFromToday, daysBetween, semaforoTema, avancePromedio, toast } from './utils.js';

let _state = { temas: [], actividades: [], filters: {} };

let presentationMode = false;
let presentationIndex = 0;
let presentationSlides = [];
const PRESENTATION_ZOOM_FACTOR = 1.0;

const C_SLIDE = {
    guinda: '#8a0031',
    verde: '#1e5b4f',
    ok: '#027a48',
    proceso: '#b54708',
    riesgo: '#b42318',
    pendiente: '#667085',
    textoSuave: '#6c7a89'
};
const ESTATUS_COLOR = { Pendiente: C_SLIDE.pendiente, 'En proceso': C_SLIDE.proceso, Vencida: C_SLIDE.riesgo, Concluida: C_SLIDE.ok };
const PRIORIDAD_COLOR = { Alta: C_SLIDE.riesgo, Media: C_SLIDE.proceso, Baja: C_SLIDE.ok };

const REPORT_BACKGROUND_ASSETS = [
    '/gestor/img/fondoppt.png',
    '/gestor/img/portada_ppt.png',
    '/gestor/img/logo_gob.png',
    '/gestor/img/logo_sener.png'
];

function setTrackingPreloader(visible, title, sub, isError = false) {
    const pre = document.getElementById('tracking-preloader');
    if (!pre) return;
    pre.classList.toggle('is-hidden', !visible);
    pre.classList.toggle('is-error', !!isError);
    pre.style.display = visible ? 'flex' : '';
    pre.style.visibility = visible ? 'visible' : '';
    pre.style.opacity = visible ? '1' : '';
    pre.style.pointerEvents = visible ? 'auto' : '';
    const t = document.getElementById('tracking-preloader-title');
    const s = document.getElementById('tracking-preloader-sub');
    if (t && title) t.textContent = title;
    if (s && sub) s.textContent = sub;
}

function setReporteButtonsDisabled(disabled) {
    ['rep-refresh', 'rep-pdf', 'rep-ppt', 'rep-excel'].forEach((id) => {
        const btn = document.getElementById(id);
        if (btn) btn.disabled = disabled;
    });
}

function extractBackgroundUrls(styleValue) {
    if (!styleValue || styleValue === 'none') return [];
    const urls = [];
    const re = /url\((['"]?)(.*?)\1\)/g;
    let m;
    while ((m = re.exec(styleValue)) !== null) {
        if (m[2]) urls.push(m[2]);
    }
    return urls;
}

function waitForImageLoad(url, timeoutMs = 12000) {
    return new Promise((resolve) => {
        if (!url) return resolve(false);
        const img = new Image();
        let done = false;
        const finish = (ok) => {
            if (done) return;
            done = true;
            resolve(ok);
        };
        const timer = setTimeout(() => finish(false), timeoutMs);
        img.crossOrigin = 'anonymous';
        img.onload = () => {
            clearTimeout(timer);
            finish(true);
        };
        img.onerror = () => {
            clearTimeout(timer);
            finish(false);
        };
        img.src = url;
        if (img.complete && img.naturalWidth > 0) {
            clearTimeout(timer);
            finish(true);
        }
    });
}

async function waitForSlideAssets(slide) {
    const urls = new Set(REPORT_BACKGROUND_ASSETS);

    [slide, ...slide.querySelectorAll('*')].forEach((el) => {
        if (el.tagName === 'IMG' && el.currentSrc) {
            urls.add(el.currentSrc);
        }
        const bg = getComputedStyle(el).backgroundImage;
        extractBackgroundUrls(bg).forEach((u) => urls.add(u));
    });

    const checks = await Promise.all([...urls].map((u) => waitForImageLoad(u)));
    const okCount = checks.filter(Boolean).length;
    if (okCount < checks.length) {
        console.warn('[Reportes] Algunos assets no cargaron antes de exportar', {
            esperados: checks.length,
            cargados: okCount
        });
    }
}

export function setReportesData(actividades, temas) {
    _state.temas = actividades;
    _state.actividades = temas;
    populateFilterOptions();
    renderDeck();
}

function populateFilterOptions() {
    const selTema = document.getElementById('rep-filtro-tema');
    const selResp = document.getElementById('rep-filtro-resp');
    if (!selTema || !selResp) return;
    const curT = selTema.value, curR = selResp.value;
    selTema.innerHTML = '<option value="">Todos</option>' +
        _state.temas.map(t => `<option value="${t.id}">${escape(t.actividad)}</option>`).join('');
    const set = new Set();
    _state.actividades.forEach(a => {
        if (a.responsable) set.add(a.responsable.trim());
        if (a.corresponsables) a.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    const personas = [...set].sort((a, b) => a.localeCompare(b, 'es-MX', { sensitivity: 'base' }));
    selResp.innerHTML = '<option value="">Todos</option>' +
        personas.map(p => `<option>${escape(p)}</option>`).join('');
    selTema.value = curT; selResp.value = curR;
}

function readFilters() {
    return {
        tema: document.getElementById('rep-filtro-tema')?.value || '',
        resp: document.getElementById('rep-filtro-resp')?.value || '',
        estatus: document.getElementById('rep-filtro-estatus')?.value || '',
        prioridad: document.getElementById('rep-filtro-prioridad')?.value || '',
        periodo: document.getElementById('rep-filtro-periodo')?.value || '',
        q: (document.getElementById('rep-filtro-busqueda')?.value || '').toLowerCase()
    };
}

function getPeriodRange(period) {
    const today = new Date();
    const year = today.getFullYear();
    const month = today.getMonth();

    let start, end;

    switch (period) {
        case 'semana_actual': {
            const day = today.getDay();
            const monday = new Date(today);
            monday.setDate(today.getDate() - (day === 0 ? 6 : day - 1));
            monday.setHours(0, 0, 0, 0);
            const sunday = new Date(monday);
            sunday.setDate(monday.getDate() + 6);
            sunday.setHours(23, 59, 59, 999);
            start = monday;
            end = sunday;
            break;
        }
        case 'mes_actual': {
            start = new Date(year, month, 1);
            end = new Date(year, month + 1, 0, 23, 59, 59, 999);
            break;
        }
        case 'mes_anterior': {
            start = new Date(year, month - 1, 1);
            end = new Date(year, month, 0, 23, 59, 59, 999);
            break;
        }
        case 'dos_meses_atras': {
            start = new Date(year, month - 2, 1);
            end = new Date(year, month - 1, 0, 23, 59, 59, 999);
            break;
        }
        case 'anio_actual': {
            start = new Date(year, 0, 1);
            end = new Date(year, 11, 31, 23, 59, 59, 999);
            break;
        }
        default:
            return null;
    }
    return { start, end };
}

function applyFilters(acts, f) {
    return acts.filter(a => {
        if (f.tema && String(a.actividadId) !== String(f.tema)) return false;
        if (f.resp && a.responsable !== f.resp && !(a.corresponsables && a.corresponsables.some(c => c.nombre === f.resp))) return false;
        if (f.estatus && a.estatus !== f.estatus) return false;
        if (f.prioridad && a.prioridad !== f.prioridad) return false;

        const range = getPeriodRange(f.periodo);
        if (range) {
            const pad = num => String(num).padStart(2, '0');
            const startStr = `${range.start.getFullYear()}-${pad(range.start.getMonth() + 1)}-${pad(range.start.getDate())}`;
            const endStr = `${range.end.getFullYear()}-${pad(range.end.getMonth() + 1)}-${pad(range.end.getDate())}`;
            if (!a.fechaCompromiso || a.fechaCompromiso < startStr || a.fechaCompromiso > endStr) return false;
        }

        if (f.periodo === '7' || f.periodo === '30') {
            const d = daysFromToday(a.fechaCompromiso);
            const lim = Number(f.periodo);
            if (d === null || d < 0 || d > lim) return false;
        }
        if (f.periodo === 'vencidas') {
            if (a.estatus === 'Concluida') return false;
            const d = daysFromToday(a.fechaCompromiso);
            if (d === null || d >= 0) return false;
        }
        if (f.q) {
            const corrNames = a.corresponsables ? a.corresponsables.map(c => c.nombre).join(' ') : '';
            const hay = [a.tema, a.responsable, a.estatus, a.comentarios, a.descripcion, corrNames]
                .map(x => (x || '').toLowerCase()).join(' ');
            if (!hay.includes(f.q)) return false;
        }
        return true;
    });
}

function statusMode(estatus) {
    if (estatus === 'Concluida') return 'complete';
    if (estatus === 'En proceso') return 'progress';
    if (estatus === 'Vencida') return 'issue';
    return 'pending';
}

function percentTone(p) {
    if (p >= 80) return 'complete';
    if (p >= 40) return 'progress';
    if (p > 0) return 'issue';
    return 'pending';
}

function renderSlideHeader(title) {
    return `
        <div class="internal-slide__top">
            <div class="internal-slide__brand">
                <img src="/gestor/img/logo_gob.png" alt="Gobierno de México">
                <img src="/gestor/img/logo_sener.png" alt="Secretaría de Energía">
            </div>
            <div class="internal-slide__title">${escape(title)}</div>
            <div class="internal-slide__unit">DGMESNIE · Subsecretaría de Planeación y Transición Energética</div>
        </div>`;
}
function renderSlideFooter() {
    return '<div class="internal-slide__footer"><span></span><span></span><span></span></div>';
}
function renderSlideKpi(label, value, tone) {
    const cls = tone ? ` class="slide-kpi--${tone}"` : '';
    return `<article${cls}><span>${escape(label)}</span><strong>${escape(value)}</strong></article>`;
}
function renderSlideBar(label, percent, detail, tone) {
    const cls = tone ? `track--${tone}` : '';
    return `
        <div class="bar-row">
            <div class="bar-row__top"><span>${escape(label)}</span><span>${escape(detail || '')}</span></div>
            <div class="track ${cls}"><span style="width:${percent}%"></span></div>
        </div>`;
}
function renderDonut(percent) {
    const v = Math.max(0, Math.min(100, Number(percent) || 0));
    const r = 46, c = 2 * Math.PI * r;
    const offset = c * (1 - v / 100);
    const color = v >= 80 ? '#027a48' : v >= 40 ? '#b54708' : v > 0 ? '#b42318' : '#667085';
    return `
        <div class="slide-donut">
            <svg viewBox="0 0 120 120" width="120" height="120">
                <circle cx="60" cy="60" r="${r}" fill="none" stroke="rgba(15,23,42,0.10)" stroke-width="12"></circle>
                <circle cx="60" cy="60" r="${r}" fill="none" stroke="${color}" stroke-width="12"
                    stroke-dasharray="${c}" stroke-dashoffset="${offset}"
                    transform="rotate(-90 60 60)" stroke-linecap="round"></circle>
                <text x="60" y="66" text-anchor="middle" font-family="Patria, Georgia, serif"
                    font-size="26" font-weight="700" fill="${color}">${v}%</text>
            </svg>
        </div>`;
}
function renderStatusStackedBar(c, p, i, pen) {
    const total = c + p + i + pen;
    if (!total) return '';
    const seg = (n, color, label) => n ? `<span class="slide-stack__seg" style="width:${(n / total * 100)}%;background:${color}" title="${label}: ${n}"></span>` : '';
    return `
        <div class="slide-stack">
            <div class="slide-stack__bar">
                ${seg(c, '#027a48', 'Concluidas')}
                ${seg(p, '#b54708', 'En proceso')}
                ${seg(i, '#b42318', 'Vencidas')}
                ${seg(pen, '#667085', 'Pendientes')}
            </div>
            <div class="slide-stack__legend">
                ${c ? `<span><i style="background:#027a48"></i>${c} Concluidas</span>` : ''}
                ${p ? `<span><i style="background:#b54708"></i>${p} En proceso</span>` : ''}
                ${i ? `<span><i style="background:#b42318"></i>${i} Vencidas</span>` : ''}
                ${pen ? `<span><i style="background:#667085"></i>${pen} Pendientes</span>` : ''}
            </div>
        </div>`;
}

function getTiempoTexto(a) {
    if (a.estatus === 'Concluida') {
        if (a.fechaInicio && a.fechaCompromiso) {
            try {
                const diff = daysBetween(a.fechaInicio, a.fechaCompromiso);
                return `Realizada en ${diff}d`;
            } catch (e) {
                return 'Concluida';
            }
        }
        return 'Concluida';
    }
    const rest = daysFromToday(a.fechaCompromiso);
    if (rest === null) return '—';
    if (rest < 0) return `Vencida ${Math.abs(rest)}d`;
    return `Quedan ${rest}d`;
}

function summary(acts) {
    const total = acts.length;
    const complete = acts.filter(a => a.estatus === 'Concluida').length;
    const progress = acts.filter(a => a.estatus === 'En proceso').length;
    const issue = acts.filter(a => a.estatus === 'Vencida' || (a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0)).length;
    const pending = acts.filter(a => a.estatus === 'Pendiente').length;
    const average = total ? Math.round(acts.reduce((s, a) => s + (a.avance || 0), 0) / total) : 0;
    return { total, complete, progress, issue, pending, average };
}

export function renderDeck() {
    const deck = document.getElementById('internal-report-deck');
    if (!deck) return;
    const f = readFilters();
    const acts = applyFilters(_state.actividades, f);
    const temasFiltrados = f.tema
        ? _state.temas.filter(t => t.id === f.tema)
        : _state.temas.filter(t => acts.some(a => a.actividadId === t.id));

    const s = summary(acts);
    const reportDate = new Date().toLocaleDateString('es-MX', { dateStyle: 'long' });
    document.getElementById('rep-summary').textContent =
        `${acts.length} temas · ${temasFiltrados.length} actividades · ${s.average}% avance promedio`;

    // Top temas con menor avance (de los filtrados)
    const topTemas = temasFiltrados.map(t => {
        const sub = acts.filter(a => a.actividadId === t.id);
        const av = sub.length ? Math.round(sub.reduce((x, a) => x + (a.avance || 0), 0) / sub.length) : 0;
        return { t, av, total: sub.length, completos: sub.filter(a => a.estatus === 'Concluida').length };
    }).sort((a, b) => a.av - b.av).slice(0, 6);

    const atencion = acts.filter(a => a.estatus !== 'Concluida' && (a.estatus === 'Vencida' || daysFromToday(a.fechaCompromiso) <= 7))
        .sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''));

    deck.innerHTML = `
        ${slideCover(s, reportDate, f)}
        ${slideDashboardGraficos(acts, f)}
        ${slideResumen(s, topTemas)}
        ${slideAtencion(atencion)}
        ${slideResponsables(acts)}
        ${slideTemasDashboard(temasFiltrados, acts)}
        ${slideTreemap(temasFiltrados, acts)}
        ${temasFiltrados.map(t => slideTema(t, acts.filter(a => a.actividadId === t.id))).join('')}
    `;

    initSlideDashboard(acts);
    initSlideTreemap(temasFiltrados, acts);
}

function slideCover(s, reportDate, f) {
    let filterDetails = [];
    if (f) {
        if (f.tema) {
            const found = _state.temas.find(t => String(t.id) === String(f.tema));
            if (found) filterDetails.push(`Actividad: <strong>${escape(found.actividad)}</strong>`);
        }
        if (f.resp) filterDetails.push(`Responsable: <strong>${escape(f.resp)}</strong>`);
        if (f.estatus) filterDetails.push(`Estatus: <strong>${escape(f.estatus)}</strong>`);
        if (f.prioridad) filterDetails.push(`Prioridad: <strong>${escape(f.prioridad)}</strong>`);
        if (f.periodo) {
            const periodLabels = {
                '7': 'Próximos 7 días',
                '30': 'Próximos 30 días',
                'vencidas': 'Vencidas',
                'semana_actual': 'Semana en curso',
                'mes_actual': 'Mes en curso',
                'mes_anterior': '1 mes atrás',
                'dos_meses_atras': '2 meses atrás',
                'anio_actual': 'Año en curso'
            };
            const lbl = periodLabels[f.periodo] || f.periodo;
            filterDetails.push(`Periodo: <strong>${escape(lbl)}</strong>`);
        }
        if (f.q) filterDetails.push(`Búsqueda: <strong>"${escape(f.q)}"</strong>`);
    }

    let filterHtml = '';
    if (filterDetails.length > 0) {
        filterHtml = `<div class="cover-filters">Filtrado por: ${filterDetails.join(' &nbsp;·&nbsp; ')}</div>`;
    }

    return `
        <section class="internal-slide internal-slide--cover">
            ${renderSlideHeader('DGMESNIE · Seguimiento')}
            <div class="internal-cover-body">
                <p class="eyebrow">Reporte ejecutivo</p>
                <h2>Seguimiento de Actividades</h2>
                <p class="unit">Dirección General de Metodologías y Estadísticas del Sistema Nacional de Información Energética</p>
                ${filterHtml}
                <div class="internal-cover-meta">
                    <span><strong>Actividades</strong>${s.total}</span>
                    <span><strong>Avance promedio</strong>${s.average}%</span>
                    <span><strong>Fecha del reporte</strong>${escape(reportDate)}</span>
                </div>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideResumen(s, topTemas) {
    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Resumen general')}
            <div class="internal-slide__body">
                <h2>Panel ejecutivo</h2>
                <div class="slide-kpis">
                    ${renderSlideKpi('Temas', s.total)}
                    ${renderSlideKpi('Avance promedio', s.average + '%')}
                    ${renderSlideKpi('Concluidos', s.complete, 'complete')}
                    ${renderSlideKpi('Atención', s.progress + s.issue, 'issue')}
                </div>
                <div class="slide-grid">
                    <div>
                        <h3>Actividades con menor avance</h3>
                        ${topTemas.length
            ? topTemas.map(x => renderSlideBar(x.t.actividad, x.av, `${x.av}% · ${x.completos}/${x.total}`, percentTone(x.av))).join('')
            : '<p class="muted">Sin datos.</p>'}
                    </div>
                    <div>
                        <h3>Distribución por estatus</h3>
                        ${[
            ['Concluidos', s.complete, s.total, 'complete'],
            ['En proceso', s.progress, s.total, 'progress'],
            ['Vencidos', s.issue, s.total, 'issue'],
            ['Pendientes', s.pending, s.total, 'pending']
        ].map(([label, value, total, tone]) =>
            renderSlideBar(label, total ? Math.round(value / total * 100) : 0, value, tone)
        ).join('')}
                    </div>
                </div>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideAtencion(rows) {
    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Detalle de atención')}
            <div class="internal-slide__body">
                <h2>Temas en atención</h2>
                <table class="slide-table">
                    <thead><tr><th>Actividad</th><th>Tema</th><th>Responsable</th><th>Compromiso</th><th>Tiempo</th><th>Estatus</th></tr></thead>
                    <tbody>
                        ${rows.length ? rows.slice(0, 18).map(a => {
        const tema = _state.temas.find(t => t.id === a.actividadId);
        return `
                                <tr>
                                    <td>${escape(tema?.actividad || '')}</td>
                                    <td>
                                        <strong>${escape(a.tema)}</strong>
                                        ${a.evidenciaUrl ? `
                                            <a href="${escape(a.evidenciaUrl)}" target="_blank" rel="noopener" class="slide-evidencia-link" title="Ver evidencia en SharePoint" style="margin-left: 6px; color: #b48934; display: inline-flex; align-items: center; text-decoration: none;">
                                                <i class="fa-solid fa-folder-open"></i>
                                            </a>
                                        ` : ''}
                                    </td>
                                    <td>${escape(a.responsable)}</td>
                                    <td>${fmtDate(a.fechaCompromiso)}</td>
                                    <td><span style="font-size: 0.72rem; font-weight: 600; color: ${a.estatus === 'Vencida' ? C_SLIDE.riesgo : C_SLIDE.pendiente}">${getTiempoTexto(a)}</span></td>
                                    <td><span class="status-pill status-pill--${statusMode(a.estatus)}">${escape(a.estatus)}</span></td>
                                </tr>`;
    }).join('') : '<tr><td colspan="6" class="muted" style="text-align:center">Sin temas en atención</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideResponsables(acts) {
    const setPersonas = new Set();
    acts.forEach(a => {
        if (a.responsable) setPersonas.add(a.responsable.trim());
        if (a.corresponsables) a.corresponsables.forEach(c => { if (c.nombre) setPersonas.add(c.nombre.trim()); });
    });
    const rows = [...setPersonas].map(p => {
        const list = acts.filter(a => a.responsable === p || (a.corresponsables && a.corresponsables.some(c => c.nombre === p)));
        const total = list.length;
        const c = list.filter(a => a.estatus === 'Concluida').length;
        const prog = list.filter(a => a.estatus === 'En proceso').length;
        const pen = list.filter(a => a.estatus === 'Pendiente').length;
        const vencidas = list.filter(a => a.estatus === 'Vencida' || (a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0)).length;
        const av = total ? Math.round(list.reduce((s, a) => s + (a.avance || 0), 0) / total) : 0;
        return { p, total, c, prog, pen, vencidas, av };
    }).sort((a, b) => b.total - a.total);

    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Carga por responsable')}
            <div class="internal-slide__body">
                <h2>Carga de trabajo y estatus de temas por responsable</h2>
                <table class="slide-table">
                    <thead>
                        <tr>
                            <th>Responsable</th>
                            <th>Total Temas</th>
                            <th>Concluidos</th>
                            <th>En proceso</th>
                            <th>Pendientes</th>
                            <th>Vencidos</th>
                            <th>Avance Promedio</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${rows.length ? rows.map(r => `
                            <tr>
                                <td><strong>${escape(r.p)}</strong></td>
                                <td>${r.total}</td>
                                <td><span style="font-weight:600; color:#027a48">${r.c}</span></td>
                                <td><span style="font-weight:600; color:#b54708">${r.prog}</span></td>
                                <td><span style="font-weight:600; color:#667085">${r.pen}</span></td>
                                <td>${r.vencidas ? `<span class="status-pill status-pill--issue" style="font-weight:600">${r.vencidas}</span>` : `<span style="color:#667085">0</span>`}</td>
                                <td><strong>${r.av}%</strong></td>
                            </tr>`).join('') : '<tr><td colspan="7" class="muted" style="text-align:center">Sin datos</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideTemasDashboard(temas, acts) {
    const rows = temas.map(t => {
        const sub = acts.filter(a => a.actividadId === t.id);
        const total = sub.length;
        const c = sub.filter(a => a.estatus === 'Concluida').length;
        const p = sub.filter(a => a.estatus === 'En proceso').length;
        const pen = sub.filter(a => a.estatus === 'Pendiente').length;
        const i = sub.filter(a => a.estatus === 'Vencida' || (a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0)).length;
        const av = total ? Math.round(sub.reduce((s, a) => s + (a.avance || 0), 0) / total) : (t.avanceGeneral || 0);
        const sem = semaforoTema(t, sub);
        return { t, total, c, p, pen, i, av, sem };
    }).sort((a, b) => a.av - b.av);

    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Dashboard general de Actividades')}
            <div class="internal-slide__body">
                <h2>Resumen general y avance de Actividades</h2>
                <table class="slide-table slide-table--themes-dashboard">
                    <thead>
                        <tr>
                            <th>Actividad</th>
                            <th>Responsable Principal</th>
                            <th>Semáforo</th>
                            <th>Temas</th>
                            <th>Concluidos</th>
                            <th>En proceso</th>
                            <th>Pendientes</th>
                            <th>Vencidos</th>
                            <th>Avance General</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${rows.length ? rows.slice(0, 15).map(r => `
                            <tr>
                                <td><strong>${escape(r.t.actividad)}</strong></td>
                                <td>
                                    ${escape(r.t.responsablePrincipal || 'Sin responsable')}
                                    ${r.t.corresponsables && r.t.corresponsables.length 
                                        ? `<br><small style="font-size:0.68rem; color:var(--texto-suave); font-weight:500;">Co: ${escape(r.t.corresponsables.map(c => c.nombre.split(' ')[0]).join(', '))}</small>` 
                                        : ''}
                                </td>
                                <td><span class="semaforo ${r.sem}"></span></td>
                                <td>${r.total}</td>
                                <td><span style="font-weight:600; color:#027a48">${r.c}</span></td>
                                <td><span style="font-weight:600; color:#b54708">${r.p}</span></td>
                                <td><span style="font-weight:600; color:#667085">${r.pen}</span></td>
                                <td>${r.i ? `<span class="status-pill status-pill--issue" style="font-weight:600">${r.i}</span>` : `<span style="color:#667085">0</span>`}</td>
                                <td>
                                    <div style="display:flex; align-items:center; gap:8px">
                                        <span style="font-weight:700; min-width:32px">${r.av}%</span>
                                        <div class="track track--${percentTone(r.av)}" style="width:60px; height:8px; margin:0"><span style="width:${r.av}%"></span></div>
                                    </div>
                                </td>
                            </tr>`).join('') : '<tr><td colspan="9" class="muted" style="text-align:center">Sin datos</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideDashboardGraficos(acts, f) {
    let filterName = 'NACIONAL';
    if (f) {
        if (f.tema) {
            const found = _state.temas.find(t => String(t.id) === String(f.tema));
            if (found) filterName = found.actividad;
        } else if (f.resp) {
            filterName = f.resp;
        }
    }

    const s = summary(acts);
    const pctComplete = s.total ? Math.round((s.complete / s.total) * 100) : 0;
    const temasCount = [...new Set(acts.map(a => a.actividadId))].length;

    // Calc priority counts and dominant
    const priCounts = acts.reduce((m, a) => { m[a.prioridad || 'Baja'] = (m[a.prioridad || 'Baja'] || 0) + 1; return m; }, {});
    let topPrioridad = 'Baja';
    let topPrioridadCount = 0;
    Object.entries(priCounts).forEach(([p, c]) => {
        if (c > topPrioridadCount) {
            topPrioridad = p;
            topPrioridadCount = c;
        }
    });

    // Calc status counts and dominant
    const estCounts = acts.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {});
    let topEstatus = 'Pendiente';
    let topEstatusCount = 0;
    Object.entries(estCounts).forEach(([e, c]) => {
        if (c > topEstatusCount) {
            topEstatus = e;
            topEstatusCount = c;
        }
    });

    const estatusLabels = {
        'Pendiente': 'Pendiente',
        'En proceso': 'En proceso',
        'Concluida': 'Concluida',
        'Vencida': 'Vencida'
    };
    const formattedEstatus = estatusLabels[topEstatus] || topEstatus;

    return `
        <section class="internal-slide internal-slide--content" style="background: #fafbfc;">
            <!-- Header -->
            ${renderSlideHeader('Indicadores ejecutivos')}

            <div class="internal-slide__body" style="padding: 10px 24px; gap: 12px; flex: 1; display: flex; flex-direction: column; overflow: hidden;">
                <!-- Narrative Paragraph -->
                <div class="slide-narrative" style="font-family: var(--font-h), Montserrat, sans-serif; font-size: 0.82rem; line-height: 1.5; color: #2d3748; background: #fff; padding: 10px 14px; border-radius: 6px; border: 1px solid rgba(15,23,42,0.06); box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                    Para <span style="font-weight: 700; color: #8a0031;">${escape(filterName === 'NACIONAL' ? 'el portafolio nacional' : filterName)}</span>, coordinamos <span style="font-weight: 700; color: #8a0031;">${s.total} temas</span> de <span style="font-weight: 700; color: #8a0031;">${temasCount} actividades</span>, alcanzando <span style="font-weight: 700; color: #8a0031;">${pctComplete}% de avance</span> (${s.complete} concluidos). La mayor carga se concentra en prioridad <span style="font-weight: 700; color: #8a0031;">${topPrioridad}</span> (${topPrioridadCount} tareas), con la mayoría de los temas <span style="font-weight: 700; color: #8a0031;">${formattedEstatus === 'Concluida' ? 'concluidos' : formattedEstatus === 'En proceso' ? 'en proceso' : formattedEstatus === 'Vencida' ? 'vencidos' : 'pendientes'}</span> (${topEstatusCount}).
                </div>

                <!-- KPI Cards Row -->
                <div class="slide-kpi-row" style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px;">
                    <!-- Card 1 -->
                    <div style="display: flex; align-items: center; justify-content: space-between; background: #fff; padding: 10px 14px; border-radius: 6px; border: 1px solid rgba(15,23,42,0.06); border-left: 4px solid #8a0031; box-shadow: 0 2px 4px rgba(0,0,0,0.02); height: 58px;">
                        <span style="font-family: Montserrat, sans-serif; font-size: 1.65rem; font-weight: 800; color: #8a0031; line-height: 1;">${s.total}</span>
                        <div style="font-family: Montserrat, sans-serif; font-size: 0.6rem; font-weight: 700; color: #6c7a89; text-transform: uppercase; text-align: right; line-height: 1.2;">
                            TOTAL DE<br>TEMAS
                        </div>
                    </div>
                    <!-- Card 2 -->
                    <div style="display: flex; align-items: center; justify-content: space-between; background: #fff; padding: 10px 14px; border-radius: 6px; border: 1px solid rgba(15,23,42,0.06); border-left: 4px solid #027a48; box-shadow: 0 2px 4px rgba(0,0,0,0.02); height: 58px;">
                        <span style="font-family: Montserrat, sans-serif; font-size: 1.65rem; font-weight: 800; color: #027a48; line-height: 1;">${s.complete}</span>
                        <div style="font-family: Montserrat, sans-serif; font-size: 0.6rem; font-weight: 700; color: #6c7a89; text-transform: uppercase; text-align: right; line-height: 1.2;">
                            TEMAS<br>CONCLUIDOS
                        </div>
                    </div>
                    <!-- Card 3 -->
                    <div style="display: flex; align-items: center; justify-content: space-between; background: #fff; padding: 10px 14px; border-radius: 6px; border: 1px solid rgba(15,23,42,0.06); border-left: 4px solid #b48934; box-shadow: 0 2px 4px rgba(0,0,0,0.02); height: 58px;">
                        <span style="font-family: Montserrat, sans-serif; font-size: 1.65rem; font-weight: 800; color: #b48934; line-height: 1;">${pctComplete}%</span>
                        <div style="font-family: Montserrat, sans-serif; font-size: 0.6rem; font-weight: 700; color: #6c7a89; text-transform: uppercase; text-align: right; line-height: 1.2;">
                            AVANCE PROMEDIO<br>GLOBAL
                        </div>
                    </div>
                    <!-- Card 4 -->
                    <div style="display: flex; align-items: center; justify-content: space-between; background: #fff; padding: 10px 14px; border-radius: 6px; border: 1px solid rgba(15,23,42,0.06); border-left: 4px solid #667085; box-shadow: 0 2px 4px rgba(0,0,0,0.02); height: 58px;">
                        <span style="font-family: Montserrat, sans-serif; font-size: 1.65rem; font-weight: 800; color: #667085; line-height: 1;">${s.progress + s.issue}</span>
                        <div style="font-family: Montserrat, sans-serif; font-size: 0.6rem; font-weight: 700; color: #6c7a89; text-transform: uppercase; text-align: right; line-height: 1.2;">
                            TEMAS<br>EN ATENCIÓN
                        </div>
                    </div>
                </div>

                <!-- 3-Column Layout -->
                <div class="slide-dashboard-3cols" style="display: grid; grid-template-columns: 28% 44% 28%; gap: 14px; flex: 1; min-height: 0; overflow: hidden;">
                    
                    <!-- Column 1 (Left) -->
                    <div style="display: flex; flex-direction: column; gap: 12px; min-height: 0;">
                        <!-- Chart Box -->
                        <div style="background: #fff; border: 1px solid rgba(15,23,42,0.06); border-radius: 6px; padding: 10px; display: flex; flex-direction: column; height: 215px; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                            <h3 style="font-size: 10px; margin: 0 0 5px; color: #8a0031; text-transform: uppercase; font-family: Montserrat, sans-serif; font-weight: 700; display: flex; align-items: center; gap: 5px;">
                                <span style="display: inline-block; width: 5px; height: 5px; background-color: #8a0031; border-radius: 50%;"></span>
                                Estatus por Prioridad
                            </h3>
                            <div id="slide-dash-priority-chart" style="flex: 1; min-height: 0; width: 100%;"></div>
                        </div>
                        
                        <!-- Mini Table Box -->
                        <div style="background: #fff; border: 1px solid rgba(15,23,42,0.06); border-radius: 6px; padding: 10px; display: flex; flex-direction: column; height: 165px; box-shadow: 0 1px 3px rgba(0,0,0,0.02); overflow: hidden;">
                            <h3 style="font-size: 10px; margin: 0 0 8px; color: #b48934; text-transform: uppercase; font-family: Montserrat, sans-serif; font-weight: 700; display: flex; align-items: center; gap: 5px;">
                                <span style="display: inline-block; width: 5px; height: 5px; background-color: #b48934; border-radius: 50%;"></span>
                                Resumen por Prioridad
                            </h3>
                            <div style="flex: 1; overflow-y: auto;">
                                <table style="width: 100%; border-collapse: collapse; font-family: Montserrat, sans-serif; font-size: 0.72rem; line-height: 1.3;">
                                    <thead>
                                        <tr style="border-bottom: 2px solid #b48934; text-align: left;">
                                            <th style="padding: 4px 6px; font-weight: 700; color: #b48934; text-transform: uppercase; font-size: 0.65rem;">Prioridad</th>
                                            <th style="padding: 4px 6px; font-weight: 700; color: #b48934; text-transform: uppercase; font-size: 0.65rem; text-align: right; width: 50px;">Total</th>
                                            <th style="padding: 4px 6px; font-weight: 700; color: #b48934; text-transform: uppercase; font-size: 0.65rem; text-align: right; width: 60px;">Avance</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${['Alta', 'Media', 'Baja'].map(p => {
        const sub = acts.filter(a => (a.prioridad || 'Baja') === p);
        const totalPri = sub.length;
        const pctPri = s.total ? Math.round((totalPri / s.total) * 100) : 0;
        const avPri = totalPri ? Math.round(sub.reduce((acc, x) => acc + (x.avance || 0), 0) / totalPri) : 0;
        return `
                                                <tr style="border-bottom: 1px solid #f0f2f5;">
                                                    <td style="padding: 5px 6px; font-weight: 600; color: #2d3748;">
                                                        <span style="display:inline-block; width: 6px; height: 6px; background-color: ${PRIORIDAD_COLOR[p] || '#667085'}; border-radius: 50%; margin-right: 5px; vertical-align: middle;"></span>
                                                        ${p}
                                                    </td>
                                                    <td style="padding: 5px 6px; text-align: right; font-weight: 700; color: #2d3748;">
                                                        ${totalPri} <span style="font-weight: 500; font-size: 0.6rem; color: #718096;">(${pctPri}%)</span>
                                                    </td>
                                                    <td style="padding: 5px 6px; text-align: right; font-weight: 700; color: ${avPri >= 75 ? C_SLIDE.ok : avPri >= 40 ? C_SLIDE.proceso : C_SLIDE.riesgo};">
                                                        ${avPri}%
                                                    </td>
                                                </tr>
                                            `;
    }).join('')}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>

                    <!-- Column 2 (Center) -->
                    <div style="background: #fff; border: 1px solid rgba(15,23,42,0.06); border-radius: 6px; padding: 10px; display: flex; flex-direction: column; height: 392px; box-shadow: 0 1px 3px rgba(0,0,0,0.02); min-height: 0;">
                        <h3 style="font-size: 10px; margin: 0 0 8px; color: #1e5b4f; text-transform: uppercase; font-family: Montserrat, sans-serif; font-weight: 700; display: flex; align-items: center; gap: 5px; border-bottom: 2px solid #1e5b4f; padding-bottom: 4px;">
                            <span style="display: inline-block; width: 5px; height: 5px; background-color: #1e5b4f; border-radius: 50%;"></span>
                            Top 10 Temas en Atención
                        </h3>
                        <div style="flex: 1; overflow-y: auto; min-height: 0;">
                            <table style="width: 100%; border-collapse: collapse; font-family: Montserrat, sans-serif; font-size: 0.68rem; line-height: 1.3;">
                                <thead style="position: sticky; top: 0; background: #fff; z-index: 2;">
                                    <tr style="border-bottom: 1px solid #edf2f7; text-align: left;">
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem;">Tema</th>
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem; width: 68px;">Compromiso</th>
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem; width: 75px;">Tiempo</th>
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem; width: 90px;">Responsable</th>
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem; width: 45px; text-align: right;">Avance</th>
                                        <th style="padding: 5px 8px; font-weight: 700; color: #4a5568; font-size: 0.65rem; width: 68px; text-align: center;">Estatus</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${(() => {
            const topActs = acts
                .filter(a => a.estatus !== 'Concluida')
                .sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''))
                .slice(0, 10);
            return topActs.length ? topActs.map(a => {
                const parentAct = _state.temas.find(x => x.id === a.actividadId);
                return `
                                            <tr style="border-bottom: 1px solid #f7fafc;">
                                                <td style="padding: 6px 8px; vertical-align: top; max-width: 180px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;" title="${escape(a.tema)}">
                                                    <strong style="color: #2d3748;">${escape(a.tema)}</strong>
                                                    <br><small style="font-size:0.6rem; color:var(--texto-suave); font-weight:500;">${escape(parentAct?.actividad || '')}</small>
                                                    ${a.evidenciaUrl ? `
                                                        <a href="${escape(a.evidenciaUrl)}" target="_blank" rel="noopener" style="color: #b48934; margin-left: 3px; display: inline-flex; align-items: center; text-decoration: none;">
                                                            <i class="fa-solid fa-folder-open" style="font-size: 0.65rem;"></i>
                                                        </a>
                                                    ` : ''}
                                                </td>
                                                <td style="padding: 6px 8px; vertical-align: top; color: #4a5568;">${fmtDate(a.fechaCompromiso)}</td>
                                                <td style="padding: 6px 8px; vertical-align: top; font-weight: 600; color: ${a.estatus === 'Vencida' ? C_SLIDE.riesgo : C_SLIDE.pendiente};">${getTiempoTexto(a)}</td>
                                                <td style="padding: 6px 8px; vertical-align: top; color: #4a5568; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" title="${escape(a.responsable)}">${escape(a.responsable)}</td>
                                                <td style="padding: 6px 8px; vertical-align: top; text-align: right; font-weight: 700; color: #2d3748;">${a.avance || 0}%</td>
                                                <td style="padding: 6px 8px; vertical-align: top; text-align: center;">
                                                    <span class="status-pill status-pill--${statusMode(a.estatus)}" style="font-size: 0.58rem; padding: 1px 5px; min-height: 16px; font-weight: 700;">
                                                        ${escape(a.estatus)}
                                                    </span>
                                                </td>
                                            </tr>`;
            }).join('') : `<tr><td colspan="6" style="padding: 20px; text-align: center; color: #a0aec0; font-style: italic;">Sin temas pendientes en atención</td></tr>`;
        })()}
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <!-- Column 3 (Right) -->
                    <div style="display: flex; flex-direction: column; gap: 12px; min-height: 0;">
                        <!-- Chart Box 1 (Donut) -->
                        <div style="background: #fff; border: 1px solid rgba(15,23,42,0.06); border-radius: 6px; padding: 10px; display: flex; flex-direction: column; height: 190px; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                            <h3 style="font-size: 10px; margin: 0 0 5px; color: #1e5b4f; text-transform: uppercase; font-family: Montserrat, sans-serif; font-weight: 700; display: flex; align-items: center; gap: 5px;">
                                <span style="display: inline-block; width: 5px; height: 5px; background-color: #1e5b4f; border-radius: 50%;"></span>
                                Participación por Estatus
                            </h3>
                            <div id="slide-dash-status-donut" style="flex: 1; min-height: 0; width: 100%;"></div>
                        </div>
                        
                        <!-- Chart Box 2 (Horizontal Workload Bar) -->
                        <div style="background: #fff; border: 1px solid rgba(15,23,42,0.06); border-radius: 6px; padding: 10px; display: flex; flex-direction: column; height: 190px; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                            <h3 style="font-size: 10px; margin: 0 0 5px; color: #667085; text-transform: uppercase; font-family: Montserrat, sans-serif; font-weight: 700; display: flex; align-items: center; gap: 5px;">
                                <span style="display: inline-block; width: 5px; height: 5px; background-color: #667085; border-radius: 50%;"></span>
                                Temas por Responsable
                            </h3>
                            <div id="slide-dash-workload-bar" style="flex: 1; min-height: 0; width: 100%;"></div>
                        </div>
                    </div>

                </div>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function initSlideDashboard(actividades) {
    const priorityCont = document.getElementById('slide-dash-priority-chart');
    const donutCont = document.getElementById('slide-dash-status-donut');
    const workloadCont = document.getElementById('slide-dash-workload-bar');

    if (!priorityCont || !donutCont || !workloadCont || !window.Highcharts) return;

    // 1. Stacked Column Chart for Priority by Status
    const priEst = {
        'Concluida': [0, 0, 0], // [Alta, Media, Baja]
        'En proceso': [0, 0, 0],
        'Vencida': [0, 0, 0],
        'Pendiente': [0, 0, 0]
    };
    const priMap = { 'Alta': 0, 'Media': 1, 'Baja': 2 };
    actividades.forEach(a => {
        const priIdx = priMap[a.prioridad || 'Baja'];
        if (priIdx !== undefined) {
            const est = a.estatus || 'Pendiente';
            if (priEst[est]) {
                priEst[est][priIdx]++;
            }
        }
    });

    const prioritySeries = Object.entries(priEst).map(([name, data]) => ({
        name,
        data,
        color: ESTATUS_COLOR[name] || C_SLIDE.pendiente
    })).filter(s => s.data.some(v => v > 0));

    Highcharts.chart(priorityCont, {
        chart: {
            type: 'column',
            style: { fontFamily: 'Montserrat, sans-serif' },
            backgroundColor: '#ffffff',
            height: 180,
            spacing: [5, 5, 5, 5]
        },
        credits: { enabled: false },
        title: { text: '' },
        exporting: {
            enabled: true,
            buttons: {
                contextButton: {
                    menuItems: ['viewFullscreen']
                }
            }
        },
        xAxis: {
            categories: ['Alta', 'Media', 'Baja'],
            labels: { style: { fontSize: '8px', fontWeight: '600', color: '#2d3748' } }
        },
        yAxis: {
            title: { text: '' },
            allowDecimals: false,
            labels: { style: { fontSize: '8px', color: '#718096' } }
        },
        tooltip: { shared: true },
        plotOptions: {
            column: {
                stacking: 'normal',
                borderRadius: 2,
                borderWidth: 0,
                groupPadding: 0.15,
                pointPadding: 0.05
            }
        },
        legend: {
            enabled: true,
            align: 'center',
            verticalAlign: 'bottom',
            itemStyle: { fontSize: '7.5px', fontWeight: '600', color: '#2d3748' },
            margin: 5,
            padding: 0
        },
        series: prioritySeries
    });

    // 2. Donut for Status Participation
    const estatusCounts = actividades.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {});
    const donutData = Object.entries(estatusCounts).map(([name, y]) => ({
        name,
        y,
        color: ESTATUS_COLOR[name] || C_SLIDE.pendiente
    })).filter(pt => pt.y > 0);

    Highcharts.chart(donutCont, {
        chart: {
            type: 'pie',
            style: { fontFamily: 'Montserrat, sans-serif' },
            backgroundColor: '#ffffff',
            height: 150,
            spacing: [5, 5, 5, 5]
        },
        credits: { enabled: false },
        title: { text: '' },
        exporting: {
            enabled: true,
            buttons: {
                contextButton: {
                    menuItems: ['viewFullscreen']
                }
            }
        },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.1f}%)' },
        plotOptions: {
            pie: {
                innerSize: '55%',
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                    format: '{point.percentage:.0f}%',
                    distance: -15,
                    style: { fontSize: '8px', fontWeight: '700', color: '#ffffff', textOutline: 'none' }
                },
                showInLegend: true
            }
        },
        legend: {
            enabled: true,
            align: 'center',
            verticalAlign: 'bottom',
            itemStyle: { fontSize: '7.5px', fontWeight: '600', color: '#2d3748' },
            margin: 5,
            padding: 0
        },
        series: [{ name: 'Temas', data: donutData }]
    });

    // 3. Stacked Horizontal Bar for Workload by Responsible
    const respMap = {};
    actividades.forEach(a => {
        const r = a.responsable || 'Sin responsable';
        if (!respMap[r]) respMap[r] = { total: 0, Concluida: 0, 'En proceso': 0, Vencida: 0, Pendiente: 0 };
        respMap[r].total++;
        respMap[r][a.estatus]++;
    });

    const topResps = Object.entries(respMap)
        .sort((a, b) => b[1].total - a[1].total)
        .slice(0, 5)
        .map(([name, counts]) => ({ name, counts }));

    const respCategories = topResps.map(r => r.name.length > 12 ? r.name.slice(0, 12) + '…' : r.name);
    const statusKeys = ['Concluida', 'En proceso', 'Vencida', 'Pendiente'];
    const respSeries = statusKeys.map(status => ({
        name: status,
        color: ESTATUS_COLOR[status] || C_SLIDE.pendiente,
        data: topResps.map(r => r.counts[status] || 0)
    })).filter(s => s.data.some(v => v > 0));

    Highcharts.chart(workloadCont, {
        chart: {
            type: 'bar',
            style: { fontFamily: 'Montserrat, sans-serif' },
            backgroundColor: '#ffffff',
            height: 155,
            spacing: [5, 5, 5, 5]
        },
        credits: { enabled: false },
        title: { text: '' },
        exporting: {
            enabled: true,
            buttons: {
                contextButton: {
                    menuItems: ['viewFullscreen']
                }
            }
        },
        xAxis: {
            categories: respCategories,
            labels: { style: { fontSize: '8px', fontWeight: '600', color: '#2d3748' } }
        },
        yAxis: {
            title: { text: '' },
            allowDecimals: false,
            labels: { style: { fontSize: '8px', color: '#718096' } }
        },
        tooltip: { shared: true },
        plotOptions: {
            bar: {
                stacking: 'normal',
                borderRadius: 2,
                borderWidth: 0,
                groupPadding: 0.15,
                pointPadding: 0.05
            }
        },
        legend: { enabled: false }, // Already documented in donut legend
        series: respSeries
    });
}

function slideTreemap(temas, acts) {
    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Estructura jerárquica de actividades y temas')}
            <div class="internal-slide__body" style="gap: 0.5rem;">
                <h2>Mapa de calor general (Treemap)</h2>
                <div id="slide-treemap-container" style="width: 100%; height: 500px; background: #fff; border-radius: 8px; border: 1px solid rgba(15, 23, 42, 0.08); overflow: hidden;"></div>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function initSlideTreemap(temas, actividades) {
    const cont = document.getElementById('slide-treemap-container');
    if (!cont) return;

    const activeActividadIds = new Set(temas.map(t => t.id));
    const filteredTemas = actividades.filter(a => activeActividadIds.has(a.actividadId));

    const data = [];

    temas.forEach(t => {
        const subTemas = filteredTemas.filter(a => a.actividadId === t.id);
        data.push({
            id: `a_${t.id}`,
            name: t.actividad,
            color: 'rgba(138, 0, 49, 0.06)',
            value: subTemas.length || 1
        });
    });

    filteredTemas.forEach(a => {
        data.push({
            id: `t_${a.id}`,
            name: a.tema,
            parent: `a_${a.actividadId}`,
            value: 1,
            colorValue: a.avance || 0,
            responsable: a.responsable || 'Sin responsable',
            estatus: a.estatus || 'Pendiente'
        });
    });

    if (window.Highcharts) {
        Highcharts.chart(cont, {
            chart: {
                style: { fontFamily: 'Montserrat, sans-serif' },
                backgroundColor: '#ffffff',
                type: 'treemap',
                height: 480
            },
            credits: { enabled: false },
            title: { text: '' },
            exporting: { enabled: false },
            colorAxis: {
                min: 0,
                max: 100,
                stops: [
                    [0, '#fee4e2'],
                    [0.5, '#fef0c7'],
                    [1, '#d1fadf']
                ]
            },
            tooltip: {
                useHTML: true,
                pointFormat: `
                    <div style="padding: 6px; font-family: Montserrat, sans-serif;">
                        <b>{point.name}</b><br/>
                        {if point.parent}
                            Responsable: <b>{point.responsable}</b><br/>
                            Progreso: <b>{point.colorValue}%</b><br/>
                            Estatus: <b>{point.estatus}</b>
                        {else}
                            Temas: <b>{point.value}</b>
                        {/if}
                    </div>
                `
            },
            series: [{
                layoutAlgorithm: 'squarified',
                allowDrillToNode: true,
                animationLimit: 120,
                dataLabels: {
                    enabled: true,
                    align: 'left',
                    verticalAlign: 'top',
                    style: {
                        fontSize: '11px',
                        fontWeight: '600',
                        textOutline: 'none',
                        color: '#243444'
                    }
                },
                levelIsConstant: false,
                levels: [{
                    level: 1,
                    dataLabels: {
                        enabled: true,
                        style: {
                            fontSize: '13px',
                            fontWeight: 'bold',
                            color: '#8a0031'
                        }
                    },
                    borderWidth: 2,
                    borderColor: '#8a0031'
                }, {
                    level: 2,
                    dataLabels: {
                        enabled: true,
                        style: {
                            fontSize: '10px',
                            fontWeight: 'normal',
                            color: '#333'
                        }
                    },
                    borderWidth: 1,
                    borderColor: '#ffffff'
                }],
                data
            }]
        });
    }
}

function slideTema(tema, rows) {
    const total = rows.length;
    const c = rows.filter(r => r.estatus === 'Concluida').length;
    const p = rows.filter(r => r.estatus === 'En proceso').length;
    const i = rows.filter(r => r.estatus === 'Vencida' || (r.estatus !== 'Concluida' && daysFromToday(r.fechaCompromiso) < 0)).length;
    const pen = rows.filter(r => r.estatus === 'Pendiente').length;
    const percent = total ? Math.round(rows.reduce((s, r) => s + (r.avance || 0), 0) / total) : 0;
    const sem = semaforoTema(tema, rows);

    return `
        <section class="internal-slide internal-slide--content internal-slide--project">
            ${renderSlideHeader(`Actividad · ${tema.actividad}`)}
            <div class="internal-slide__body">
                <div class="project-slide-head">
                    <div>
                        <p>Actividad · ${escape(tema.categoria || 'Sin categoría')}</p>
                        <h2>${escape(tema.actividad)}</h2>
                        <span>${escape(tema.responsablePrincipal)} · Compromiso ${fmtDate(tema.fechaCompromiso)} · Semáforo <span class="semaforo ${sem}"></span></span>
                        ${renderStatusStackedBar(c, p, i, pen)}
                    </div>
                    ${renderDonut(percent)}
                </div>
                <div class="slide-kpis slide-kpis--project">
                    ${renderSlideKpi('Temas', total)}
                    ${renderSlideKpi('Concluidos', c, 'complete')}
                    ${renderSlideKpi('En proceso', p, 'progress')}
                    ${renderSlideKpi('Atención', i, 'issue')}
                </div>
                <table class="slide-table slide-table--detail">
                    <thead><tr><th>Tema</th><th>Responsable</th><th>Compromiso</th><th>Tiempo</th><th>Avance</th><th>Estatus</th></tr></thead>
                    <tbody>
                        ${rows.length ? rows.slice(0, 12).map(r => `
                            <tr>
                                <td>
                                    <strong>${escape(r.tema)}</strong>
                                    ${r.evidenciaUrl ? `
                                        <a href="${escape(r.evidenciaUrl)}" target="_blank" rel="noopener" class="slide-evidencia-link" title="Ver evidencia en SharePoint" style="margin-left: 6px; color: #b48934; display: inline-flex; align-items: center; text-decoration: none;">
                                            <i class="fa-solid fa-folder-open"></i>
                                        </a>
                                    ` : ''}
                                </td>
                                <td>${escape(r.responsable)}</td>
                                <td>${fmtDate(r.fechaCompromiso)}</td>
                                <td><span style="font-size: 0.72rem; font-weight: 600; color: ${r.estatus === 'Vencida' ? C_SLIDE.riesgo : r.estatus === 'Concluida' ? C_SLIDE.ok : C_SLIDE.pendiente}">${getTiempoTexto(r)}</span></td>
                                <td>${r.avance || 0}%</td>
                                <td><span class="status-pill status-pill--${statusMode(r.estatus)}">${escape(r.estatus)}</span></td>
                            </tr>`).join('') : '<tr><td colspan="6" class="muted" style="text-align:center">Sin temas</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

// ============ DESCARGAS ============

export async function descargarPdf() {
    if (!window.jspdf || !window.html2canvas) { toast('Librerías PDF no cargadas', 'err'); return; }
    const slides = [...document.querySelectorAll('#internal-report-deck .internal-slide')];
    if (!slides.length) { toast('Sin slides para exportar', 'err'); return; }
    toast('Generando PDF…');
    setReporteButtonsDisabled(true);
    setTrackingPreloader(true, 'Generando PDF institucional', `Preparando ${slides.length} láminas…`);

    try {
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({ orientation: 'landscape', unit: 'px', format: [1280, 720] });

        for (let idx = 0; idx < slides.length; idx++) {
            setTrackingPreloader(
                true,
                'Generando PDF institucional',
                `Procesando lámina ${idx + 1} de ${slides.length}…`
            );
            await waitForSlideAssets(slides[idx]);
            const canvas = await html2canvas(slides[idx], {
                scale: 1.6,
                useCORS: true,
                allowTaint: false,
                imageTimeout: 15000,
                backgroundColor: '#ffffff'
            });
            const img = canvas.toDataURL('image/jpeg', 0.94);
            if (idx > 0) pdf.addPage([1280, 720], 'landscape');
            pdf.addImage(img, 'JPEG', 0, 0, 1280, 720);
        }

        pdf.save(`reporte-actividades-${new Date().toISOString().slice(0, 10)}.pdf`);
        toast('PDF descargado', 'ok');
    } catch (err) {
        console.error('[Reportes] Error generando PDF', err);
        toast('No se pudo generar el PDF', 'err');
        setTrackingPreloader(true, 'Error al generar PDF', 'Reintenta nuevamente. Si persiste, revisa conexión al CDN.', true);
        setTimeout(() => setTrackingPreloader(false), 1600);
    } finally {
        setReporteButtonsDisabled(false);
        setTrackingPreloader(false);
    }
}

export async function descargarPpt() {
    if (!window.PptxGenJS) { toast('Librería PPT no cargada', 'err'); return; }
    const slides = [...document.querySelectorAll('#internal-report-deck .internal-slide')];
    if (!slides.length) { toast('Sin slides', 'err'); return; }
    toast('Generando PPT…');
    const pptx = new PptxGenJS();
    pptx.defineLayout({ name: 'INST', width: 13.333, height: 7.5 });
    pptx.layout = 'INST';
    for (const el of slides) {
        const canvas = await html2canvas(el, { scale: 1.4, useCORS: true, backgroundColor: '#ffffff' });
        const data = canvas.toDataURL('image/png');
        const slide = pptx.addSlide();
        slide.addImage({ data, x: 0, y: 0, w: 13.333, h: 7.5 });
    }
    await pptx.writeFile({ fileName: `reporte-actividades-${new Date().toISOString().slice(0, 10)}.pptx` });
    toast('PPT descargado', 'ok');
}

export async function descargarExcel() {
    if (!window.ExcelJS) { toast('Librería Excel no cargada', 'err'); return; }
    const f = readFilters();
    const acts = applyFilters(_state.actividades, f);
    const wb = new ExcelJS.Workbook();
    wb.creator = 'Gestor DG'; wb.created = new Date();

    const ws = wb.addWorksheet('Temas');
    ws.columns = [
        { header: 'Tema', key: 'tema', width: 36 },
        { header: 'Actividad', key: 'actividad', width: 40 },
        { header: 'Responsable', key: 'resp', width: 22 },
        { header: 'Inicio', key: 'inicio', width: 12 },
        { header: 'Compromiso', key: 'comp', width: 12 },
        { header: 'Estatus', key: 'estatus', width: 14 },
        { header: 'Avance', key: 'avance', width: 8 },
        { header: 'Prioridad', key: 'prioridad', width: 10 },
        { header: 'Bloqueada', key: 'bloq', width: 10 },
        { header: 'Evidencia', key: 'evid', width: 40 },
        { header: 'Comentarios', key: 'com', width: 50 }
    ];
    ws.getRow(1).font = { bold: true, color: { argb: 'FFFFFFFF' } };
    ws.getRow(1).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF9B2247' } };

    acts.forEach(a => {
        const t = _state.temas.find(x => x.id === a.actividadId);
        ws.addRow({
            tema: a.tema, actividad: t?.actividad, resp: a.responsable,
            inicio: a.fechaInicio, comp: a.fechaCompromiso, estatus: a.estatus,
            avance: (a.avance || 0) / 100, prioridad: a.prioridad,
            bloq: a.bloqueada ? 'Sí' : 'No', evid: a.evidenciaUrl, com: a.comentarios
        });
    });
    ws.getColumn('avance').numFmt = '0%';

    // Worksheet temas
    const wsT = wb.addWorksheet('Actividades');
    wsT.columns = [
        { header: 'Actividad', key: 'actividad', width: 40 },
        { header: 'Responsable', key: 'resp', width: 22 },
        { header: 'Categoría', key: 'cat', width: 16 },
        { header: 'Prioridad', key: 'pri', width: 10 },
        { header: 'Estatus', key: 'est', width: 12 },
        { header: 'Compromiso', key: 'comp', width: 14 },
        { header: 'Avance', key: 'avance', width: 8 }
    ];
    wsT.getRow(1).font = { bold: true, color: { argb: 'FFFFFFFF' } };
    wsT.getRow(1).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF1E5B4F' } };
    _state.temas.forEach(t => {
        const sub = acts.filter(a => a.actividadId === t.id);
        const av = sub.length ? Math.round(sub.reduce((s, a) => s + (a.avance || 0), 0) / sub.length) : (t.avanceGeneral || 0);
        wsT.addRow({
            actividad: t.actividad, resp: t.responsablePrincipal, cat: t.categoria,
            pri: t.prioridad, est: t.estatus, comp: t.fechaCompromiso, avance: av / 100
        });
    });
    wsT.getColumn('avance').numFmt = '0%';

    const buf = await wb.xlsx.writeBuffer();
    const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `reporte-actividades-${new Date().toISOString().slice(0, 10)}.xlsx`;
    a.click(); URL.revokeObjectURL(url);
    toast('Excel descargado', 'ok');
}

// ============ PRESENTACION CONTROLLER ============

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

export async function enterPresentation(startAt = 0) {
    presentationSlides = [...document.querySelectorAll('#internal-report-deck .internal-slide')];
    if (!presentationSlides.length) return;
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

export async function exitPresentation() {
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

export function nextPresentationSlide() {
    if (!presentationMode) return;
    presentationIndex = Math.min(presentationSlides.length - 1, presentationIndex + 1);
    renderPresentationState();
}

export function prevPresentationSlide() {
    if (!presentationMode) return;
    presentationIndex = Math.max(0, presentationIndex - 1);
    renderPresentationState();
}

// ============ WIRE EVENTOS ============

export function wireReportes() {
    ['rep-filtro-tema', 'rep-filtro-resp', 'rep-filtro-estatus', 'rep-filtro-prioridad', 'rep-filtro-periodo']
        .forEach(id => {
            const el = document.getElementById(id);
            if (el) el.onchange = renderDeck;
        });
    const q = document.getElementById('rep-filtro-busqueda');
    if (q) q.oninput = renderDeck;

    document.getElementById('rep-refresh')?.addEventListener('click', renderDeck);
    document.getElementById('rep-pdf')?.addEventListener('click', descargarPdf);
    document.getElementById('rep-ppt')?.addEventListener('click', descargarPpt);
    document.getElementById('rep-excel')?.addEventListener('click', descargarExcel);

    // Presentation bindings
    document.getElementById('rep-presentar')?.addEventListener('click', () => enterPresentation(0));
    document.getElementById('btnPrevSlide')?.addEventListener('click', prevPresentationSlide);
    document.getElementById('btnNextSlide')?.addEventListener('click', nextPresentationSlide);
    document.getElementById('btnExitPresentation')?.addEventListener('click', exitPresentation);

    window.addEventListener('resize', fitActiveSlide);
    document.addEventListener('fullscreenchange', () => {
        if (presentationMode && !document.fullscreenElement) {
            exitPresentation();
        }
    });
    document.addEventListener('keydown', (ev) => {
        if (!presentationMode) return;
        if (['ArrowRight', 'PageDown', ' '].includes(ev.key)) {
            ev.preventDefault();
            nextPresentationSlide();
        } else if (['ArrowLeft', 'PageUp', 'Backspace'].includes(ev.key)) {
            ev.preventDefault();
            prevPresentationSlide();
        } else if (ev.key === 'Home') {
            ev.preventDefault();
            presentationIndex = 0;
            renderPresentationState();
        } else if (ev.key === 'End') {
            ev.preventDefault();
            presentationIndex = Math.max(0, presentationSlides.length - 1);
            renderPresentationState();
        } else if (ev.key === 'Escape') {
            ev.preventDefault();
            exitPresentation();
        }
    });
}
