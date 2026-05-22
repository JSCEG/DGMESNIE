// Reportes institucionales — deck de slides estilo PPT con filtros y descarga PDF/PPT/Excel.

import { escape, fmtDate, daysFromToday, semaforoTema, avancePromedio, toast } from './utils.js';

let _state = { temas: [], actividades: [], filters: {} };

export function setReportesData(temas, actividades) {
    _state.temas = temas;
    _state.actividades = actividades;
    populateFilterOptions();
    renderDeck();
}

function populateFilterOptions() {
    const selTema = document.getElementById('rep-filtro-tema');
    const selResp = document.getElementById('rep-filtro-resp');
    if (!selTema || !selResp) return;
    const curT = selTema.value, curR = selResp.value;
    selTema.innerHTML = '<option value="">Todos</option>' +
        _state.temas.map(t => `<option value="${t.id}">${escape(t.tema)}</option>`).join('');
    const personas = [...new Set(_state.actividades.map(a => a.responsable).filter(Boolean))].sort();
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

function applyFilters(acts, f) {
    return acts.filter(a => {
        if (f.tema && a.temaId !== f.tema) return false;
        if (f.resp && a.responsable !== f.resp) return false;
        if (f.estatus && a.estatus !== f.estatus) return false;
        if (f.prioridad && a.prioridad !== f.prioridad) return false;
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
            const hay = [a.actividad, a.responsable, a.estatus, a.comentarios, a.descripcion]
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
                <img src="../Estilos Institucionales/img/logo_gob.png" alt="Gobierno de México">
                <img src="../Estilos Institucionales/img/logo_sener.png" alt="Secretaría de Energía">
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
    const seg = (n, color, label) => n ? `<span class="slide-stack__seg" style="width:${(n/total*100)}%;background:${color}" title="${label}: ${n}"></span>` : '';
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
        : _state.temas.filter(t => acts.some(a => a.temaId === t.id));

    const s = summary(acts);
    const reportDate = new Date().toLocaleDateString('es-MX', { dateStyle: 'long' });
    document.getElementById('rep-summary').textContent =
        `${acts.length} actividades · ${temasFiltrados.length} temas · ${s.average}% avance promedio`;

    // Top temas con menor avance (de los filtrados)
    const topTemas = temasFiltrados.map(t => {
        const sub = acts.filter(a => a.temaId === t.id);
        const av = sub.length ? Math.round(sub.reduce((x, a) => x + (a.avance || 0), 0) / sub.length) : 0;
        return { t, av, total: sub.length, completos: sub.filter(a => a.estatus === 'Concluida').length };
    }).sort((a, b) => a.av - b.av).slice(0, 6);

    const atencion = acts.filter(a => a.estatus !== 'Concluida' && (a.estatus === 'Vencida' || daysFromToday(a.fechaCompromiso) <= 7))
        .sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''));

    deck.innerHTML = `
        ${slideCover(s, reportDate)}
        ${slideResumen(s, topTemas)}
        ${slideAtencion(atencion)}
        ${slideResponsables(acts)}
        ${temasFiltrados.map(t => slideTema(t, acts.filter(a => a.temaId === t.id))).join('')}
    `;
}

function slideCover(s, reportDate) {
    return `
        <section class="internal-slide internal-slide--cover">
            <img class="internal-cover-bg" src="../Estilos Institucionales/img/portada_ppt.png" alt="">
            ${renderSlideHeader('DGMESNIE · Seguimiento')}
            <div class="internal-cover-body">
                <p class="eyebrow">Reporte ejecutivo</p>
                <h2>Seguimiento de Actividades</h2>
                <p class="unit">Dirección General de Metodologías y Estadísticas del Sistema Nacional de Información Energética</p>
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
                    ${renderSlideKpi('Actividades', s.total)}
                    ${renderSlideKpi('Avance promedio', s.average + '%')}
                    ${renderSlideKpi('Concluidas', s.complete, 'complete')}
                    ${renderSlideKpi('Atención', s.progress + s.issue, 'issue')}
                </div>
                <div class="slide-grid">
                    <div>
                        <h3>Temas con menor avance</h3>
                        ${topTemas.length
                            ? topTemas.map(x => renderSlideBar(x.t.tema, x.av, `${x.av}% · ${x.completos}/${x.total}`, percentTone(x.av))).join('')
                            : '<p class="muted">Sin datos.</p>'}
                    </div>
                    <div>
                        <h3>Distribución por estatus</h3>
                        ${[
                            ['Concluidas', s.complete, s.total, 'complete'],
                            ['En proceso', s.progress, s.total, 'progress'],
                            ['Vencidas', s.issue, s.total, 'issue'],
                            ['Pendientes', s.pending, s.total, 'pending']
                        ].map(([label, value, total, tone]) =>
                            renderSlideBar(label, total ? Math.round(value/total*100) : 0, value, tone)
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
                <h2>Actividades en atención</h2>
                <table class="slide-table">
                    <thead><tr><th>Tema</th><th>Actividad</th><th>Responsable</th><th>Compromiso</th><th>Estatus</th></tr></thead>
                    <tbody>
                        ${rows.length ? rows.slice(0, 18).map(a => {
                            const tema = _state.temas.find(t => t.id === a.temaId);
                            return `
                                <tr>
                                    <td>${escape(tema?.tema || '')}</td>
                                    <td><strong>${escape(a.actividad)}</strong></td>
                                    <td>${escape(a.responsable)}</td>
                                    <td>${fmtDate(a.fechaCompromiso)}</td>
                                    <td><span class="status-pill status-pill--${statusMode(a.estatus)}">${escape(a.estatus)}</span></td>
                                </tr>`;
                        }).join('') : '<tr><td colspan="5" class="muted" style="text-align:center">Sin actividades en atención</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
}

function slideResponsables(acts) {
    const map = new Map();
    acts.forEach(a => {
        if (!map.has(a.responsable)) map.set(a.responsable, []);
        map.get(a.responsable).push(a);
    });
    const rows = [...map.entries()].map(([p, list]) => {
        const total = list.length;
        const c = list.filter(a => a.estatus === 'Concluida').length;
        const av = total ? Math.round(list.reduce((s, a) => s + (a.avance || 0), 0) / total) : 0;
        const vencidas = list.filter(a => a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0).length;
        return { p, total, c, av, vencidas };
    }).sort((a, b) => b.total - a.total);

    return `
        <section class="internal-slide internal-slide--content">
            ${renderSlideHeader('Carga por responsable')}
            <div class="internal-slide__body">
                <h2>Carga de trabajo</h2>
                <table class="slide-table">
                    <thead><tr><th>Responsable</th><th>Actividades</th><th>Concluidas</th><th>Vencidas</th><th>Avance</th></tr></thead>
                    <tbody>
                        ${rows.length ? rows.map(r => `
                            <tr>
                                <td><strong>${escape(r.p)}</strong></td>
                                <td>${r.total}</td>
                                <td>${r.c}</td>
                                <td>${r.vencidas ? `<span class="status-pill status-pill--issue">${r.vencidas}</span>` : r.vencidas}</td>
                                <td>${r.av}%</td>
                            </tr>`).join('') : '<tr><td colspan="5" class="muted" style="text-align:center">Sin datos</td></tr>'}
                    </tbody>
                </table>
            </div>
            ${renderSlideFooter()}
        </section>`;
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
            ${renderSlideHeader(`Tema · ${tema.tema}`)}
            <div class="internal-slide__body">
                <div class="project-slide-head">
                    <div>
                        <p>Tema · ${escape(tema.categoria || 'Sin categoría')}</p>
                        <h2>${escape(tema.tema)}</h2>
                        <span>${escape(tema.responsablePrincipal)} · Compromiso ${fmtDate(tema.fechaCompromiso)} · Semáforo <span class="semaforo ${sem}"></span></span>
                        ${renderStatusStackedBar(c, p, i, pen)}
                    </div>
                    ${renderDonut(percent)}
                </div>
                <div class="slide-kpis slide-kpis--project">
                    ${renderSlideKpi('Actividades', total)}
                    ${renderSlideKpi('Concluidas', c, 'complete')}
                    ${renderSlideKpi('En proceso', p, 'progress')}
                    ${renderSlideKpi('Atención', i, 'issue')}
                </div>
                <table class="slide-table slide-table--detail">
                    <thead><tr><th>Actividad</th><th>Responsable</th><th>Compromiso</th><th>Avance</th><th>Estatus</th></tr></thead>
                    <tbody>
                        ${rows.length ? rows.slice(0, 12).map(r => `
                            <tr>
                                <td><strong>${escape(r.actividad)}</strong></td>
                                <td>${escape(r.responsable)}</td>
                                <td>${fmtDate(r.fechaCompromiso)}</td>
                                <td>${r.avance || 0}%</td>
                                <td><span class="status-pill status-pill--${statusMode(r.estatus)}">${escape(r.estatus)}</span></td>
                            </tr>`).join('') : '<tr><td colspan="5" class="muted" style="text-align:center">Sin actividades</td></tr>'}
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
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF({ orientation: 'landscape', unit: 'px', format: [1280, 720] });
    for (let idx = 0; idx < slides.length; idx++) {
        const canvas = await html2canvas(slides[idx], { scale: 1.4, useCORS: true, backgroundColor: '#ffffff' });
        const img = canvas.toDataURL('image/jpeg', 0.92);
        if (idx > 0) pdf.addPage([1280, 720], 'landscape');
        pdf.addImage(img, 'JPEG', 0, 0, 1280, 720);
    }
    pdf.save(`reporte-actividades-${new Date().toISOString().slice(0,10)}.pdf`);
    toast('PDF descargado', 'ok');
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
    await pptx.writeFile({ fileName: `reporte-actividades-${new Date().toISOString().slice(0,10)}.pptx` });
    toast('PPT descargado', 'ok');
}

export async function descargarExcel() {
    if (!window.ExcelJS) { toast('Librería Excel no cargada', 'err'); return; }
    const f = readFilters();
    const acts = applyFilters(_state.actividades, f);
    const wb = new ExcelJS.Workbook();
    wb.creator = 'Gestor DG'; wb.created = new Date();

    const ws = wb.addWorksheet('Actividades');
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
        const t = _state.temas.find(x => x.id === a.temaId);
        ws.addRow({
            tema: t?.tema, actividad: a.actividad, resp: a.responsable,
            inicio: a.fechaInicio, comp: a.fechaCompromiso, estatus: a.estatus,
            avance: (a.avance || 0) / 100, prioridad: a.prioridad,
            bloq: a.bloqueada ? 'Sí' : 'No', evid: a.evidenciaUrl, com: a.comentarios
        });
    });
    ws.getColumn('avance').numFmt = '0%';

    // Worksheet temas
    const wsT = wb.addWorksheet('Temas');
    wsT.columns = [
        { header: 'Tema', key: 'tema', width: 40 },
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
        const sub = acts.filter(a => a.temaId === t.id);
        const av = sub.length ? Math.round(sub.reduce((s, a) => s + (a.avance || 0), 0) / sub.length) : (t.avanceGeneral || 0);
        wsT.addRow({
            tema: t.tema, resp: t.responsablePrincipal, cat: t.categoria,
            pri: t.prioridad, est: t.estatus, comp: t.fechaCompromiso, avance: av / 100
        });
    });
    wsT.getColumn('avance').numFmt = '0%';

    const buf = await wb.xlsx.writeBuffer();
    const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `reporte-actividades-${new Date().toISOString().slice(0,10)}.xlsx`;
    a.click(); URL.revokeObjectURL(url);
    toast('Excel descargado', 'ok');
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
}
