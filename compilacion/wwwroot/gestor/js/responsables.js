import { escape, fmtDate, semaforo, daysFromToday, uniqueResponsables, toast, parseDate, getPeriodRange, getPeriodLabel } from './utils.js';
import {
    cleanupPresentationSlides,
    enterFullscreen,
    exitFullscreen,
    fitSlideToViewport,
    isBackwardPresentationKey,
    isForwardPresentationKey,
    renderPresentationSlides,
    setPresentationControls
} from './presentation-utils.js';

const PRESENTATION_ZOOM_FACTOR = 0.92;
let responsablePresentationMode = false;
let responsablePresentationIndex = 0;
let responsablePresentationSlides = [];
let responsablePresentationWired = false;
let responsableChartData = { rows: [], actividades: [] };

export function renderResponsables(actividades, temas) {
    const cont = document.getElementById('responsables-grid');
    if (!cont) return;
    const personas = uniqueResponsables(actividades, temas);

    if (window.currentUser && !window.currentUser.esAdmin) {
        const currentName = (window.currentUser.nombre || '').trim();
        let targetPersona = personas.find(p => p.toLowerCase() === currentName.toLowerCase());
        if (!targetPersona) {
            targetPersona = personas[0] || currentName;
        }
        if (targetPersona) {
            const ts = temas.filter(t => participaEnTema(t, targetPersona));
            mostrarReporteResponsable(targetPersona, ts, actividades);
            return;
        }
    }

    cont.innerHTML = personas.map(p => {
        const ts = temas.filter(t => participaEnTema(t, p));
        const activas = ts.filter(t => t.estatus !== 'Concluida');
        const completadas = ts.filter(t => t.estatus === 'Concluida');
        const vencidas = activas.filter(t => daysFromToday(t.fechaCompromiso) < 0);
        const porVencer = activas.filter(t => { const d = daysFromToday(t.fechaCompromiso); return d >= 0 && d <= 7; });
        const coCount = ts.filter(t => rolEnTema(t, p) !== 'Responsable').length;
        
        // Sort: active ones first (sorted by compromiso), then completed ones
        const sortedTs = [...activas].sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''))
            .concat(completadas.sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || '')));

        return `
            <div class="resp-card">
                <h4>${escape(p)}</h4>
                <div class="stats" style="margin-bottom: 0.75rem; flex-wrap: wrap;">
                    <span><strong>${activas.length}</strong> activas</span>
                    <span style="color:var(--ok)"><strong>${completadas.length}</strong> concluidas</span>
                    <span style="color:var(--riesgo)"><strong>${vencidas.length}</strong> vencidas</span>
                    <span style="color:var(--proceso)"><strong>${porVencer.length}</strong> por vencer</span>
                    ${coCount > 0 ? `<span style="color:var(--guinda)"><strong>${coCount}</strong> co-responsable</span>` : ''}
                </div>
                <ul>
                    ${sortedTs.length
                        ? sortedTs.map(t => {
                            const isComp = t.estatus === 'Concluida';
                            const isCo = rolEnTema(t, p) !== 'Responsable';
                            return `<li style="${isComp ? 'opacity: 0.65;' : ''}">
                                <span>
                                    <span class="semaforo ${semaforo(t)}"></span> 
                                    ${isComp ? `<del>${escape(t.tema)}</del>` : escape(t.tema)}
                                    ${isCo ? ' <span class="badge badge-co" style="font-size: 0.65rem; background: rgba(138, 0, 49, 0.08); color: var(--guinda); padding: 1px 4px; border-radius: 4px;">Co</span>' : ''}
                                </span>
                                <small>${fmtDate(t.fechaCompromiso)}</small>
                            </li>`;
                          }).join('')
                        : '<li style="color:var(--g-text-soft);justify-content:center">Sin temas registrados</li>'
                    }
                </ul>
                <div style="margin-top: 1rem; border-top: 1px solid var(--borde); padding-top: 0.75rem; display: flex; justify-content: flex-end;">
                    <button class="internal-button btn-ver-reporte" data-resp="${escape(p)}" style="min-height: 32px; padding: 0.3rem 0.8rem; font-size: 0.78rem; display: inline-flex; align-items: center; gap: 6px;">
                        <i class="fa-solid fa-file-lines"></i> Ver reporte
                    </button>
                </div>
            </div>`;
    }).join('');

    // Bind event listeners
    cont.querySelectorAll('.btn-ver-reporte').forEach(btn => {
        btn.onclick = () => {
            const respName = btn.dataset.resp;
            const ts = temas.filter(t => participaEnTema(t, respName));
            mostrarReporteResponsable(respName, ts, actividades);
        };
    });
}

function participaEnTema(t, nombre) {
    if (!nombre) return false;
    if (t.responsable === nombre) return true;
    if (Array.isArray(t.corresponsables) && t.corresponsables.some(c => c.nombre === nombre)) return true;
    return Array.isArray(t.etapas) && t.etapas.some(e =>
        e.responsableNombre === nombre ||
        (Array.isArray(e.corresponsables) && e.corresponsables.some(c => c.nombre === nombre)));
}

function rolEnTema(t, nombre) {
    if (t.responsable === nombre) return 'Responsable';
    if (Array.isArray(t.etapas) && t.etapas.some(e => e.responsableNombre === nombre)) return 'Responsable de etapa';
    return 'Corresponsable';
}

function etapasParticipacion(t, nombre) {
    if (!Array.isArray(t.etapas)) return '';
    return t.etapas
        .filter(e => e.responsableNombre === nombre ||
            (Array.isArray(e.corresponsables) && e.corresponsables.some(c => c.nombre === nombre)))
        .map(e => {
            const rol = e.responsableNombre === nombre ? 'responsable' : 'corresponsable';
            return `${e.nombre || 'Etapa'} (${rol})`;
        })
        .join(', ');
}

function periodoLabel(value) {
    return value === 'vencidas' ? 'Solo vencidos' : getPeriodLabel(value);
}

function filtrarTemasPeriodo(ts, periodo) {
    if (!periodo) return ts;
    if (periodo === '30') {
        return ts.filter(t => {
            const d = daysFromToday(t.fechaCompromiso);
            return typeof d === 'number' && d >= 0 && d <= 30;
        });
    }
    if (periodo === 'vencidas') {
        return ts.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0);
    }
    const range = getPeriodRange(periodo);
    if (!range) return ts;
    return ts.filter(t => t.fechaCompromiso && t.fechaCompromiso >= range.start && t.fechaCompromiso <= range.end);
}

function actividadNombre(actividades, tema) {
    return actividades.find(a => String(a.id) === String(tema.actividadId))?.actividad || tema.actividadNombre || 'Sin actividad';
}

function etapaActual(t) {
    if (!Array.isArray(t.etapas) || !t.etapas.length) return null;
    return t.etapas.find(e => Number(e.avance || 0) < 100) || t.etapas[t.etapas.length - 1];
}

function resumenResponsable(responsableName, ts) {
    const total = ts.length;
    const concluidas = ts.filter(t => t.estatus === 'Concluida');
    const activas = ts.filter(t => t.estatus !== 'Concluida');
    const vencidas = activas.filter(t => daysFromToday(t.fechaCompromiso) < 0);
    const porVencer = activas.filter(t => { const d = daysFromToday(t.fechaCompromiso); return d >= 0 && d <= 7; });
    const coCount = ts.filter(t => rolEnTema(t, responsableName) !== 'Responsable').length;
    const avanceProm = total > 0 ? Math.round(ts.reduce((s, t) => s + (t.avance || 0), 0) / total) : 0;
    return { total, concluidas, activas, vencidas, porVencer, coCount, avanceProm };
}

function renderReporteResponsable(responsableName, ts, actividades, periodo) {
    const rows = filtrarTemasPeriodo(ts, periodo);
    const r = resumenResponsable(responsableName, rows);
    const fecha = new Date().toLocaleDateString('es-MX', { day: '2-digit', month: 'long', year: 'numeric' });
    const sorted = rows.slice().sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''));
    const ganttSlide = renderGanttResponsableSlide(responsableName, rows, periodo);
    const detailSlides = renderDetalleResponsableSlides(responsableName, sorted, actividades, periodo);

    return `
        <div id="responsable-report-sheet" class="internal-report-deck resp-report-deck" style="display:flex;flex-direction:column;gap:14px;font-family:Montserrat,sans-serif;color:var(--texto);">
            <section class="internal-slide internal-slide--cover resp-report-slide">
                <div class="internal-cover-body" style="padding-top:4.8rem;">
                    <p class="eyebrow">Reporte por responsable</p>
                    <h2>${escape(responsableName)}</h2>
                    <p class="unit">Dirección General de Metodologías y Estadísticas del Sistema Nacional de Información Energética</p>
                    <div class="cover-filters" style="margin-top:.4rem;margin-bottom:0;">
                        Periodo: <strong>${escape(periodoLabel(periodo))}</strong> · Fecha: <strong>${escape(fecha)}</strong>
                    </div>
                    <div class="internal-cover-meta" style="margin-top:2.6rem;">
                        <span><strong>Temas</strong>${r.total}</span>
                        <span><strong>Vencidos</strong>${r.vencidas.length}</span>
                        <span><strong>Avance</strong>${r.avanceProm}%</span>
                    </div>
                </div>
                <div class="internal-slide__footer"><span></span><span></span><span></span></div>
            </section>

            ${ganttSlide}

            <section class="internal-slide internal-slide--content resp-report-slide">
                <div class="internal-slide__top">
                    <div class="internal-slide__brand">
                        <img src="/img/sideo/logo_gob.png" alt="Gobierno de México">
                        <div class="dv"></div>
                        <img src="/img/sideo/logo_sener.png" alt="Secretaría de Energía">
                    </div>
                    <div class="internal-slide__title">Resumen del periodo</div>
                    <div class="internal-slide__unit">DGMESNIE · Seguimiento de actividades</div>
                </div>
                <div class="internal-slide__body">
                    <div class="slide-kpis" style="grid-template-columns:repeat(6,minmax(0,1fr));">
                        ${miniKpi('Temas', r.total)}
                        ${miniKpi('Activos', r.activas.length)}
                        ${miniKpi('Concluidos', r.concluidas.length, '#027a48')}
                        ${miniKpi('Por vencer', r.porVencer.length, '#b48934')}
                        ${miniKpi('Vencidos', r.vencidas.length, '#8a0031')}
                        ${miniKpi('Avance', `${r.avanceProm}%`, '#1e5b4f')}
                    </div>
                    <div style="display:grid;grid-template-columns:34% 33% 33%;gap:14px;align-items:stretch;min-height:390px;">
                        <div style="border:1px solid var(--borde);border-radius:12px;background:#fff;padding:12px;">
                            <h3 style="margin:0 0 8px;">Estado de los temas</h3>
                            <div id="resp-status-chart" style="height:320px;width:100%;"></div>
                        </div>
                        <div style="border:1px solid var(--borde);border-radius:12px;background:#fff;padding:12px;">
                            <h3 style="margin:0 0 8px;">Temas por actividad</h3>
                            <div id="resp-activity-chart" style="height:320px;width:100%;"></div>
                        </div>
                        <div style="border:1px solid var(--borde);border-radius:12px;background:#fff;padding:12px;">
                            <h3 style="margin:0 0 8px;">Avance de temas</h3>
                            <div id="resp-progress-chart" style="height:320px;width:100%;"></div>
                        </div>
                    </div>
                </div>
                <div class="internal-slide__footer"><span></span><span></span><span></span></div>
            </section>

            <section class="internal-slide internal-slide--content resp-report-slide">
                <div class="internal-slide__top">
                    <div class="internal-slide__brand">
                        <img src="/img/sideo/logo_gob.png" alt="Gobierno de México">
                        <div class="dv"></div>
                        <img src="/img/sideo/logo_sener.png" alt="Secretaría de Energía">
                    </div>
                    <div class="internal-slide__title">Distribución de temas</div>
                    <div class="internal-slide__unit">${escape(responsableName)} · ${escape(periodoLabel(periodo))}</div>
                </div>
                <div class="internal-slide__body">
                    <div style="display:grid;grid-template-columns:33% 34% 33%;gap:14px;align-items:stretch;min-height:430px;">
                        <div class="resp-chart-card">
                            <h3>Papel en los temas</h3>
                            <div id="resp-role-chart" style="height:350px;width:100%;"></div>
                        </div>
                        <div class="resp-chart-card">
                            <h3>Fechas de atención</h3>
                            <div id="resp-due-chart" style="height:350px;width:100%;"></div>
                        </div>
                        <div class="resp-chart-card">
                            <h3>Prioridad</h3>
                            <div id="resp-priority-chart" style="height:350px;width:100%;"></div>
                        </div>
                    </div>
                    ${renderAtencionResponsable(rows)}
                </div>
                <div class="internal-slide__footer"><span></span><span></span><span></span></div>
            </section>

            ${detailSlides}
        </div>`;
}

function renderGanttResponsableSlide(responsableName, rows, periodo) {
    const withDates = rows.filter(t => t.fechaInicio && t.fechaCompromiso).length;
    return `
        <section class="internal-slide internal-slide--content resp-report-slide">
            <div class="internal-slide__top">
                <div class="internal-slide__brand">
                    <img src="/img/sideo/logo_gob.png" alt="Gobierno de México">
                    <div class="dv"></div>
                    <img src="/img/sideo/logo_sener.png" alt="Secretaría de Energía">
                </div>
                <div class="internal-slide__title">Cronograma del periodo</div>
                <div class="internal-slide__unit">${escape(responsableName)} · ${escape(periodoLabel(periodo))}</div>
            </div>
            <div class="internal-slide__body" style="gap:10px;">
                <div class="slide-kpis" style="grid-template-columns:repeat(3,minmax(0,1fr));">
                    ${miniKpi('Con fechas', withDates)}
                    ${miniKpi('Sin fecha', rows.length - withDates, '#667085')}
                    ${miniKpi('Total', rows.length)}
                </div>
                <div class="resp-chart-card" style="height:480px;padding:10px;">
                    <div id="resp-gantt-chart" style="height:455px;width:100%;"></div>
                </div>
            </div>
            <div class="internal-slide__footer"><span></span><span></span><span></span></div>
        </section>`;
}

function renderAtencionResponsable(rows) {
    const focus = rows
        .filter(t => t.estatus !== 'Concluida')
        .sort((a, b) => daysFromToday(a.fechaCompromiso) - daysFromToday(b.fechaCompromiso))
        .slice(0, 5);
    return `
        <table class="slide-table slide-table--responsable-focus">
            <thead>
                <tr>
                    <th>Tema</th>
                    <th>Compromiso</th>
                    <th>Estado</th>
                    <th>Avance</th>
                </tr>
            </thead>
            <tbody>
                ${focus.length ? focus.map(t => {
                    const statusCls = t.estatus === 'Concluida' ? 'status-pill--complete' : t.estatus === 'En proceso' ? 'status-pill--progress' : t.estatus === 'Vencida' ? 'status-pill--issue' : '';
                    return `
                    <tr>
                        <td><strong>${escape(t.tema)}</strong></td>
                        <td>${fmtDate(t.fechaCompromiso)}</td>
                        <td><span class="status-pill ${statusCls}">${escape(t.estatus || 'Pendiente')}</span></td>
                        <td><strong>${Number(t.avance || 0)}%</strong></td>
                    </tr>`;
                }).join('') : '<tr><td colspan="4">Sin temas pendientes en el periodo seleccionado</td></tr>'}
            </tbody>
        </table>`;
}

function renderDetalleResponsableSlides(responsableName, rows, actividades, periodo) {
    const perSlide = 9;
    const pages = rows.length ? chunkArray(rows, perSlide) : [[]];
    return pages.map((page, idx) => `
        <section class="internal-slide internal-slide--content resp-report-slide">
            <div class="internal-slide__top">
                <div class="internal-slide__brand">
                    <img src="/img/sideo/logo_gob.png" alt="Gobierno de México">
                    <div class="dv"></div>
                    <img src="/img/sideo/logo_sener.png" alt="Secretaría de Energía">
                </div>
                <div class="internal-slide__title">Detalle de temas${pages.length > 1 ? ` ${idx + 1}/${pages.length}` : ''}</div>
                <div class="internal-slide__unit">${escape(responsableName)} · ${escape(periodoLabel(periodo))}</div>
            </div>
            <div class="internal-slide__body" style="gap:10px;">
                <table class="slide-table slide-table--responsable-detail">
                    <thead>
                        <tr>
                            <th>Actividad</th>
                            <th>Tema</th>
                            <th>Papel</th>
                            <th>Etapas donde participa</th>
                            <th>Compromiso</th>
                            <th>Estado</th>
                            <th>Avance</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${page.length ? page.map(t => renderDetalleResponsableRow(t, responsableName, actividades)).join('') : '<tr><td colspan="7">Sin temas para el periodo seleccionado</td></tr>'}
                    </tbody>
                </table>
            </div>
            <div class="internal-slide__footer"><span></span><span></span><span></span></div>
        </section>`).join('');
}

function renderDetalleResponsableRow(t, responsableName, actividades) {
    const etapa = etapaActual(t);
    const papel = rolEnTema(t, responsableName);
    const etapas = etapasParticipacion(t, responsableName) || etapa?.nombre || 'Seguimiento simple';
    const statusCls = t.estatus === 'Concluida' ? 'status-pill--complete' : t.estatus === 'En proceso' ? 'status-pill--progress' : t.estatus === 'Vencida' ? 'status-pill--issue' : '';
    return `
        <tr>
            <td>${escape(actividadNombre(actividades, t))}</td>
            <td><strong>${escape(t.tema)}</strong></td>
            <td>${papel}</td>
            <td>${escape(etapas)}</td>
            <td>${fmtDate(t.fechaCompromiso)}</td>
            <td><span class="status-pill ${statusCls}">${escape(t.estatus || 'Pendiente')}</span></td>
            <td><strong>${t.avance || 0}%</strong></td>
        </tr>`;
}

function chunkArray(items, size) {
    const chunks = [];
    for (let i = 0; i < items.length; i += size) chunks.push(items.slice(i, i + size));
    return chunks;
}

function miniKpi(label, value, color = '#1f2937') {
    let bg = '#ffffff';
    let borderLeft = '1px solid var(--borde)';
    
    const labelLower = label.toLowerCase();
    if (labelLower.includes('concluido') || labelLower.includes('activa')) {
        bg = 'rgba(2, 122, 72, 0.04)';
        borderLeft = '4px solid #027a48';
        color = '#027a48';
    } else if (labelLower.includes('vencido')) {
        bg = 'rgba(138, 0, 49, 0.04)';
        borderLeft = '4px solid #8a0031';
        color = '#8a0031';
    } else if (labelLower.includes('vencer')) {
        bg = 'rgba(180, 137, 52, 0.04)';
        borderLeft = '4px solid #b48934';
        color = '#b48934';
    } else if (labelLower.includes('avance')) {
        bg = 'rgba(30, 91, 79, 0.04)';
        borderLeft = '4px solid #1e5b4f';
        color = '#1e5b4f';
    } else if (labelLower.includes('activo')) {
        bg = 'rgba(102, 112, 133, 0.04)';
        borderLeft = '4px solid #667085';
        color = '#667085';
    } else {
        bg = '#ffffff';
        borderLeft = '4px solid #b48934'; // Golden/primary brand left border
    }

    return `
        <article style="border:1px solid var(--borde); border-left:${borderLeft}; border-radius:10px; padding:9px 12px; background:${bg}; box-shadow: 0 1px 3px rgba(0,0,0,0.02); transition: all 0.2s ease;">
            <div style="font-size:.7rem; color:var(--texto-suave); font-weight:800; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 2px;">${escape(label)}</div>
            <div style="font-size:1.4rem; font-weight:900; color:${color}; line-height: 1.2;">${escape(value)}</div>
        </article>`;
}

function mostrarReporteResponsable(responsableName, ts, actividades) {
    const view = document.getElementById('view-responsable-reporte');
    const titulo = document.getElementById('resp-reporte-titulo');
    const subtitulo = document.getElementById('resp-reporte-subtitulo');
    const resumen = document.getElementById('resp-reporte-resumen');
    const periodo = document.getElementById('resp-reporte-periodo');
    const cont = document.getElementById('resp-reporte-contenido');
    if (!view || !periodo || !cont) {
        toast('No se encontró la vista del reporte.', 'err');
        return;
    }

    if (titulo) titulo.textContent = `Reporte de ${responsableName}`;
    if (subtitulo) subtitulo.textContent = 'Detalle de temas, avance y pendientes';
    periodo.value = '';

    const openView = () => {
        document.querySelectorAll('.gestor-tab').forEach(t => t.classList.toggle('active', t.dataset.view === 'responsables'));
        document.querySelectorAll('.gestor-view').forEach(v => {
            const isActive = v.id === 'view-responsable-reporte';
            v.classList.toggle('active', isActive);
            v.hidden = !isActive;
            v.setAttribute('aria-hidden', String(!isActive));
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const render = () => {
        const rows = filtrarTemasPeriodo(ts, periodo.value);
        const r = resumenResponsable(responsableName, rows);
        cont.innerHTML = renderReporteResponsable(responsableName, ts, actividades, periodo.value);
        responsableChartData = { rows, actividades, responsableName };
        requestAnimationFrame(() => renderResponsableCharts(rows, actividades, responsableName));
        if (resumen) {
            resumen.textContent = `${r.total} temas · ${r.vencidas.length} vencidos · ${r.avanceProm}% de avance`;
        }
    };

    render();
    openView();

    periodo.onchange = render;
    const volverResponsables = () => {
        document.querySelector('.gestor-tab[data-view="responsables"]')?.click();
    };
    const volverBtn = document.getElementById('resp-reporte-volver');
    const volverTopBtn = document.getElementById('resp-reporte-volver-top');
    if (volverBtn) {
        volverBtn.onclick = volverResponsables;
        volverBtn.style.display = (window.currentUser && !window.currentUser.esAdmin) ? 'none' : '';
    }
    if (volverTopBtn) {
        volverTopBtn.onclick = volverResponsables;
        volverTopBtn.style.display = (window.currentUser && !window.currentUser.esAdmin) ? 'none' : '';
    }
    document.getElementById('resp-reporte-presentar').onclick = () => presentarReporteResponsable();
    document.getElementById('resp-reporte-pdf').onclick = () => descargarReporteResponsablePdf(responsableName);
    document.getElementById('resp-reporte-ppt').onclick = () => descargarReporteResponsablePpt(responsableName);
    document.getElementById('resp-reporte-excel').onclick = () => descargarReporteResponsableExcel(responsableName, filtrarTemasPeriodo(ts, periodo.value), actividades, periodo.value);
}

function renderResponsableCharts(rows, actividades, responsableName = '') {
    if (!window.Highcharts) return;
    const statusCont = document.getElementById('resp-status-chart');
    const activityCont = document.getElementById('resp-activity-chart');
    const progressCont = document.getElementById('resp-progress-chart');
    const ganttCont = document.getElementById('resp-gantt-chart');
    const roleCont = document.getElementById('resp-role-chart');
    const dueCont = document.getElementById('resp-due-chart');
    const priorityCont = document.getElementById('resp-priority-chart');
    if (!statusCont || !activityCont || !progressCont) return;

    const concluidos = rows.filter(t => t.estatus === 'Concluida').length;
    const vencidos = rows.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0).length;
    const porVencer = rows.filter(t => {
        const d = daysFromToday(t.fechaCompromiso);
        return t.estatus !== 'Concluida' && d >= 0 && d <= 7;
    }).length;
    const activos = Math.max(0, rows.length - concluidos - vencidos - porVencer);
    const statusData = [
        { name: 'Concluidos', y: concluidos, color: '#027a48' },
        { name: 'Activos', y: activos, color: '#667085' },
        { name: 'Por vencer', y: porVencer, color: '#b48934' },
        { name: 'Vencidos', y: vencidos, color: '#8a0031' }
    ].filter(x => x.y > 0);

    const byActivity = actividades.map(a => {
        const list = rows.filter(t => String(t.actividadId) === String(a.id));
        return { name: a.actividad, y: list.length };
    }).filter(x => x.y > 0).sort((a, b) => b.y - a.y).slice(0, 8);

    const roleData = [
        { name: 'Responsable', y: rows.filter(t => rolEnTema(t, responsableName) === 'Responsable').length, color: '#8a0031' },
        { name: 'Responsable de etapa', y: rows.filter(t => rolEnTema(t, responsableName) === 'Responsable de etapa').length, color: '#027a48' },
        { name: 'Corresponsable', y: rows.filter(t => rolEnTema(t, responsableName) === 'Corresponsable').length, color: '#b48934' }
    ].filter(x => x.y > 0);
    const dueData = [
        { name: 'Vencidos', y: vencidos, color: '#8a0031' },
        { name: '7 días', y: porVencer, color: '#b48934' },
        { name: '30 días', y: rows.filter(t => {
            const d = daysFromToday(t.fechaCompromiso);
            return t.estatus !== 'Concluida' && d > 7 && d <= 30;
        }).length, color: '#b48934' },
        { name: 'Sin fecha', y: rows.filter(t => !t.fechaCompromiso).length, color: '#667085' }
    ].filter(x => x.y > 0);
    const priorityNames = ['Alta', 'Media', 'Baja'];
    const priorityColors = { Alta: '#8a0031', Media: '#b48934', Baja: '#027a48' };
    const priorityData = priorityNames.map(name => ({
        name,
        y: rows.filter(t => (t.prioridad || '').toLowerCase() === name.toLowerCase()).length,
        color: priorityColors[name]
    })).filter(x => x.y > 0);

    const baseOptions = {
        credits: { enabled: false },
        title: { text: null },
        exporting: { enabled: false },
        chart: {
            backgroundColor: 'transparent',
            style: { fontFamily: "'Libre Franklin', sans-serif" },
            animation: true
        },
        lang: { noData: 'Sin temas para graficar' },
        noData: { style: { color: '#667085', fontWeight: '700' } }
    };

    Highcharts.chart(statusCont, {
        ...baseOptions,
        chart: { ...baseOptions.chart, type: 'pie' },
        tooltip: { pointFormat: '<b>{point.y}</b> temas' },
        plotOptions: {
            pie: {
                innerSize: '56%',
                dataLabels: {
                    enabled: true,
                    distance: 14,
                    format: '{point.name}<br><b>{point.y}</b>',
                    style: { fontSize: '10px', textOutline: 'none' }
                }
            }
        },
        series: [{ name: 'Temas', data: statusData }]
    });

    Highcharts.chart(activityCont, {
        ...baseOptions,
        chart: { ...baseOptions.chart, type: 'bar' },
        xAxis: {
            categories: byActivity.map(x => x.name.length > 24 ? `${x.name.slice(0, 24)}...` : x.name),
            labels: { style: { fontSize: '10px', color: '#334155' } },
            lineColor: '#e5e7eb'
        },
        yAxis: { min: 0, allowDecimals: false, title: { text: null }, gridLineColor: '#edf2f7' },
        legend: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> temas' },
        plotOptions: { bar: { color: '#8a0031', borderRadius: 4, dataLabels: { enabled: true } } },
        series: [{ name: 'Temas', data: byActivity.map(x => x.y) }]
    });

    const avgProgress = rows.length ? Math.round(rows.reduce((sum, t) => sum + Number(t.avance || 0), 0) / rows.length) : 0;
    const progressColor = avgProgress >= 75 ? '#027a48' : avgProgress >= 40 ? '#b48934' : '#8a0031';

    Highcharts.chart(progressCont, {
        ...baseOptions,
        chart: { ...baseOptions.chart, type: 'solidgauge' },
        pane: {
            center: ['50%', '72%'],
            size: '112%',
            startAngle: -90,
            endAngle: 90,
            background: { backgroundColor: '#edf2f7', innerRadius: '68%', outerRadius: '100%', shape: 'arc', borderWidth: 0 }
        },
        yAxis: {
            min: 0,
            max: 100,
            lineWidth: 0,
            tickWidth: 0,
            minorTickInterval: null,
            labels: { y: 16, style: { fontSize: '10px', color: '#667085' } }
        },
        tooltip: { enabled: false },
        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 4,
                    borderWidth: 0,
                    useHTML: true,
                    format: `<div style="text-align:center">
                        <span style="font-size:34px;font-weight:900;color:${progressColor}">{y}%</span><br>
                        <span style="font-size:10px;font-weight:800;color:#667085;">AVANCE</span>
                    </div>`
                }
            }
        },
        series: [{ name: 'Avance', data: [{ y: avgProgress, color: progressColor }] }]
    });

    if (roleCont) {
        Highcharts.chart(roleCont, {
            ...baseOptions,
            chart: { ...baseOptions.chart, type: 'pie' },
            tooltip: { pointFormat: '<b>{point.y}</b> temas' },
            plotOptions: {
                pie: {
                    innerSize: '62%',
                    dataLabels: {
                        enabled: true,
                        format: '{point.name}<br><b>{point.y}</b>',
                        style: { fontSize: '10px', textOutline: 'none' }
                    }
                }
            },
            series: [{ name: 'Temas', data: roleData }]
        });
    }

    if (dueCont) {
        Highcharts.chart(dueCont, {
            ...baseOptions,
            chart: { ...baseOptions.chart, type: 'column' },
            xAxis: {
                categories: dueData.map(x => x.name),
                labels: { style: { fontSize: '10px', color: '#334155' } },
                lineColor: '#e5e7eb'
            },
            yAxis: { min: 0, allowDecimals: false, title: { text: null }, gridLineColor: '#edf2f7' },
            legend: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y}</b> temas' },
            plotOptions: { column: { borderRadius: 5, dataLabels: { enabled: true } } },
            series: [{ name: 'Temas', data: dueData.map(x => ({ y: x.y, color: x.color })) }]
        });
    }

    if (priorityCont) {
        Highcharts.chart(priorityCont, {
            ...baseOptions,
            chart: { ...baseOptions.chart, type: 'pie' },
            tooltip: { pointFormat: '<b>{point.y}</b> temas' },
            plotOptions: {
                pie: {
                    innerSize: '48%',
                    dataLabels: {
                        enabled: true,
                        format: '{point.name}<br><b>{point.y}</b>',
                        style: { fontSize: '10px', textOutline: 'none' }
                    }
                }
            },
            series: [{ name: 'Temas', data: priorityData }]
        });
    }

    if (ganttCont) {
        renderResponsableGantt(ganttCont, rows);
    }
}

function renderResponsableGantt(cont, rows) {
    const data = [];
    const C = { ok: '#027a48', proceso: '#b48934', riesgo: '#8a0031', pendiente: '#667085' };
    rows.filter(t => t.fechaInicio && t.fechaCompromiso)
        .sort((a, b) => (a.fechaInicio || '').localeCompare(b.fechaInicio || ''))
        .slice(0, 18)
        .forEach(t => {
            const start = parseDate(t.fechaInicio)?.getTime();
            const end = parseDate(t.fechaCompromiso)?.getTime();
            if (!start || !end) return;
            const sem = semaforo(t);
            const color = sem === 'rojo' ? C.riesgo : sem === 'amarillo' ? C.proceso : sem === 'verde' ? C.ok : C.pendiente;
            data.push({
                name: t.tema?.length > 42 ? `${t.tema.slice(0, 42)}...` : t.tema,
                start,
                end,
                completed: { amount: Number(t.avance || 0) / 100 },
                color,
                custom: { estatus: t.estatus || 'Pendiente', avance: Number(t.avance || 0) }
            });
        });

    if (cont._hc) {
        try { cont._hc.destroy(); } catch (_) { }
    }

    if (!data.length) {
        cont.innerHTML = '<div style="height:100%;display:grid;place-items:center;color:#667085;font-weight:800;">Sin temas con fechas para mostrar</div>';
        return;
    }

    cont._hc = Highcharts.ganttChart(cont, {
        chart: {
            backgroundColor: 'transparent',
            style: { fontFamily: "'Libre Franklin', sans-serif" },
            animation: { duration: 700 }
        },
        credits: { enabled: false },
        title: { text: null },
        exporting: { enabled: false },
        navigator: { enabled: false },
        scrollbar: { enabled: data.length > 10 },
        rangeSelector: { enabled: false },
        xAxis: [{ currentDateIndicator: { color: '#8a0031', label: { format: 'Hoy' } } }],
        yAxis: { labels: { style: { fontWeight: '700', fontSize: '10px', color: '#334155' } } },
        tooltip: {
            useHTML: true,
            formatter: function () {
                const p = this.point;
                const ini = Highcharts.dateFormat('%d/%m/%Y', p.start);
                const fin = Highcharts.dateFormat('%d/%m/%Y', p.end);
                return `<b>${escape(p.name)}</b><br>${ini} - ${fin}<br>Avance: <b>${p.custom?.avance || 0}%</b><br>Estado: <b>${escape(p.custom?.estatus || '')}</b>`;
            }
        },
        plotOptions: {
            gantt: {
                borderRadius: 4,
                dataLabels: {
                    enabled: true,
                    format: '{point.custom.avance}%',
                    style: { fontWeight: '800', fontSize: '10px', textOutline: 'none' }
                }
            }
        },
        series: [{ name: 'Temas', data }]
    });
}

async function presentarReporteResponsable() {
    responsablePresentationSlides = [...document.querySelectorAll('#responsable-report-sheet .internal-slide')];
    if (!responsablePresentationSlides.length) {
        toast('No hay láminas para presentar.', 'err');
        return;
    }
    renderResponsableCharts(responsableChartData.rows, responsableChartData.actividades, responsableChartData.responsableName);
    ensureResponsablePresentationEvents();
    responsablePresentationMode = true;
    responsablePresentationIndex = 0;
    document.body.classList.add('presentation-mode');
    setPresentationControls(true);
    renderResponsablePresentationState();
    await enterFullscreen();
    requestAnimationFrame(fitResponsableActiveSlide);
}

async function exitResponsablePresentacion() {
    if (!responsablePresentationMode) return;
    responsablePresentationMode = false;
    document.body.classList.remove('presentation-mode');
    cleanupPresentationSlides(responsablePresentationSlides);
    setPresentationControls(false);
    await exitFullscreen();
}

function reflowChartsInSlide(slide) {
    if (!window.Highcharts || !slide) return;
    Highcharts.charts.forEach(chart => {
        if (chart?.renderTo && slide.contains(chart.renderTo)) chart.reflow();
    });
}

function fitResponsableActiveSlide() {
    if (!responsablePresentationMode || !responsablePresentationSlides.length) return;
    fitSlideToViewport(responsablePresentationSlides[responsablePresentationIndex], PRESENTATION_ZOOM_FACTOR, reflowChartsInSlide);
}

function renderResponsablePresentationState() {
    renderPresentationSlides(responsablePresentationSlides, responsablePresentationIndex, {
        zoomFactor: PRESENTATION_ZOOM_FACTOR,
        afterFit: reflowChartsInSlide
    });
}

function nextResponsableSlide() {
    if (!responsablePresentationMode) return;
    responsablePresentationIndex = Math.min(responsablePresentationSlides.length - 1, responsablePresentationIndex + 1);
    renderResponsablePresentationState();
}

function prevResponsableSlide() {
    if (!responsablePresentationMode) return;
    responsablePresentationIndex = Math.max(0, responsablePresentationIndex - 1);
    renderResponsablePresentationState();
}

function ensureResponsablePresentationEvents() {
    if (responsablePresentationWired) return;
    document.getElementById('btnPrevSlide')?.addEventListener('click', prevResponsableSlide);
    document.getElementById('btnNextSlide')?.addEventListener('click', nextResponsableSlide);
    document.getElementById('btnExitPresentation')?.addEventListener('click', exitResponsablePresentacion);
    document.addEventListener('fullscreenchange', () => {
        if (responsablePresentationMode && !document.fullscreenElement) {
            exitResponsablePresentacion();
        }
    });
    window.addEventListener('resize', fitResponsableActiveSlide);
    document.addEventListener('keydown', event => {
        if (!responsablePresentationMode) return;
        if (isForwardPresentationKey(event.key)) {
            event.preventDefault();
            nextResponsableSlide();
        } else if (isBackwardPresentationKey(event.key, false)) {
            event.preventDefault();
            prevResponsableSlide();
        } else if (event.key === 'Escape') {
            event.preventDefault();
            exitResponsablePresentacion();
        }
    });
    responsablePresentationWired = true;
}

async function descargarReporteResponsablePdf(responsableName) {
    const slides = [...document.querySelectorAll('#responsable-report-sheet .internal-slide')];
    if (!slides.length || !window.html2canvas || !window.jspdf) {
        toast('No fue posible preparar el PDF.', 'err');
        return;
    }
    setReporteResponsableButtons(true);
    setReporteResponsablePreloader(true, 'Generando PDF', `Preparando ${slides.length} láminas...`);
    try {
        renderResponsableCharts(responsableChartData.rows, responsableChartData.actividades, responsableChartData.responsableName);
        await delay(250);
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF({ orientation: 'landscape', unit: 'pt', format: [1280, 720] });
        for (let i = 0; i < slides.length; i++) {
            setReporteResponsablePreloader(true, 'Generando PDF', `Procesando lámina ${i + 1} de ${slides.length}...`);
            await waitForResponsableAssets(slides[i]);
            const canvas = await html2canvas(slides[i], {
                scale: 2,
                backgroundColor: '#ffffff',
                useCORS: true,
                allowTaint: true,
                logging: false
            });
            if (i > 0) pdf.addPage([1280, 720], 'landscape');
            pdf.addImage(canvas.toDataURL('image/jpeg', 0.94), 'JPEG', 0, 0, 1280, 720);
        }
        pdf.save(`reporte-${responsableName.replace(/\s+/g, '-').toLowerCase()}.pdf`);
        toast('PDF generado', 'ok');
    } catch (err) {
        console.error(err);
        toast('No fue posible preparar el PDF.', 'err');
        setReporteResponsablePreloader(true, 'Error al generar PDF', 'Intente nuevamente en unos momentos.', true);
        setTimeout(() => setReporteResponsablePreloader(false), 2500);
    } finally {
        setReporteResponsableButtons(false);
        setTimeout(() => setReporteResponsablePreloader(false), 500);
    }
}

async function descargarReporteResponsablePpt(responsableName) {
    const slides = [...document.querySelectorAll('#responsable-report-sheet .internal-slide')];
    if (!slides.length || !window.PptxGenJS || !window.html2canvas) {
        toast('No fue posible preparar el PPT.', 'err');
        return;
    }
    setReporteResponsableButtons(true);
    setReporteResponsablePreloader(true, 'Generando PPT', `Preparando ${slides.length} láminas...`);
    try {
        renderResponsableCharts(responsableChartData.rows, responsableChartData.actividades, responsableChartData.responsableName);
        await delay(250);
        const pptx = new window.PptxGenJS();
        pptx.layout = 'LAYOUT_WIDE';
        pptx.author = 'DGMESNIE';
        pptx.subject = 'Seguimiento de actividades';
        pptx.title = `Reporte de ${responsableName}`;
        pptx.company = 'Secretaría de Energía';
        for (let i = 0; i < slides.length; i++) {
            const slideNode = slides[i];
            setReporteResponsablePreloader(true, 'Generando PPT', `Procesando lámina ${i + 1} de ${slides.length}...`);
            await waitForResponsableAssets(slideNode);
            const canvas = await html2canvas(slideNode, {
                scale: 2,
                backgroundColor: '#ffffff',
                useCORS: true,
                allowTaint: true,
                logging: false
            });
            const slide = pptx.addSlide();
            slide.addImage({ data: canvas.toDataURL('image/png'), x: 0, y: 0, w: 13.333, h: 7.5 });
        }
        await pptx.writeFile({ fileName: `reporte-${responsableName.replace(/\s+/g, '-').toLowerCase()}.pptx` });
        toast('PPT generado', 'ok');
    } catch (err) {
        console.error(err);
        toast('No fue posible preparar el PPT.', 'err');
        setReporteResponsablePreloader(true, 'Error al generar PPT', 'Intente nuevamente en unos momentos.', true);
        setTimeout(() => setReporteResponsablePreloader(false), 2500);
    } finally {
        setReporteResponsableButtons(false);
        setTimeout(() => setReporteResponsablePreloader(false), 500);
    }
}

function setReporteResponsableButtons(disabled) {
    ['resp-reporte-presentar', 'resp-reporte-pdf', 'resp-reporte-ppt', 'resp-reporte-excel', 'resp-reporte-volver', 'resp-reporte-volver-top'].forEach(id => {
        const btn = document.getElementById(id);
        if (btn) btn.disabled = disabled;
    });
}

function setReporteResponsablePreloader(visible, title, sub, isError = false) {
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

function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function waitForResponsableAssets(slide) {
    const urls = new Set(['/gestor/img/fondoppt.png', '/gestor/img/logo_gob.png', '/gestor/img/logo_sener.png']);
    slide.querySelectorAll('img').forEach(img => {
        if (img.currentSrc || img.src) urls.add(img.currentSrc || img.src);
    });
    return Promise.all([...urls].map(url => waitForResponsableImage(url)));
}

function waitForResponsableImage(url, timeout = 10000) {
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
        img.onload = () => {
            clearTimeout(timer);
            finish(true);
        };
        img.onerror = () => {
            clearTimeout(timer);
            finish(false);
        };
        img.src = url;
    });
}

async function descargarReporteResponsableExcel(responsableName, rows, actividades, periodo) {
    if (!window.ExcelJS) {
        toast('No se encontró el generador de Excel.', 'err');
        return;
    }
    const wb = new ExcelJS.Workbook();
    const ws = wb.addWorksheet('Reporte');
    ws.addRow(['Responsable', responsableName]);
    ws.addRow(['Periodo', periodoLabel(periodo)]);
    ws.addRow([]);
    ws.addRow(['Actividad', 'Tema', 'Papel', 'Etapas donde participa', 'Fecha compromiso', 'Estatus', 'Avance']);
    rows.forEach(t => {
        const etapa = etapaActual(t);
        ws.addRow([
            actividadNombre(actividades, t),
            t.tema,
            rolEnTema(t, responsableName),
            etapasParticipacion(t, responsableName) || etapa?.nombre || 'Seguimiento simple',
            t.fechaCompromiso || '',
            t.estatus || 'Pendiente',
            Number(t.avance || 0)
        ]);
    });
    ws.columns.forEach(c => { c.width = 22; });
    const buffer = await wb.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `reporte-${responsableName.replace(/\s+/g, '-').toLowerCase()}.xlsx`;
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}
