// Orquestador principal.
import { dataService, dataSource, isOnline } from './data-service.js';
import { renderDashboard } from './dashboard.js?v=charts-v3';
import { renderTemas, openTemaModal } from './temas.js';
import * as actividadesModule from './actividades.js?v=tabla-v3';
import { renderKanban, poblarFiltroKanban } from './kanban.js';
import { renderGantt } from './gantt.js';
import { renderCalendario, calPrev, calNext } from './calendario.js';
import { renderResponsables } from './responsables.js?v=performance-v1';
import { renderAlertas } from './alertas.js?v=recordatorio-v1';
import { setReportesData, wireReportes, renderDeck } from './reportes.js?v=presentation-v7';
import { wireChartFullscreenButtons } from './charts.js?v=charts-v3';

const state = { temas: [], actividades: [], view: 'dashboard' };

function getTablaFilters() {
    return {
        search: document.getElementById('filtro-tabla')?.value || '',
        temaId: document.getElementById('filtro-tabla-tema')?.value || '',
        estatus: document.getElementById('filtro-tabla-estatus')?.value || '',
        prioridad: document.getElementById('filtro-tabla-prioridad')?.value || '',
        responsable: document.getElementById('filtro-tabla-responsable')?.value || ''
    };
}

function setPreloader(title, sub, err = false) {
    const pre = document.getElementById('tracking-preloader');
    if (!pre) return;
    if (title) document.getElementById('tracking-preloader-title').textContent = title;
    if (sub) document.getElementById('tracking-preloader-sub').textContent = sub;
    pre.classList.toggle('is-error', err);
}
function hidePreloader() {
    const pre = document.getElementById('tracking-preloader');
    if (pre) {
        setTimeout(() => {
            pre.classList.add('is-hidden');
            // Reflow charts once layout is fully settled
            setTimeout(() => {
                if (window.Highcharts) {
                    Highcharts.charts.forEach(chart => {
                        if (chart) chart.reflow();
                    });
                }
            }, 300);
        }, 200);
    }
}
function showPreloader() {
    const pre = document.getElementById('tracking-preloader');
    if (pre) pre.classList.remove('is-hidden');
}

async function loadAll() {
    setPreloader('Cargando seguimiento de actividades', `Sincronizando ${dataSource}…`);
    try {
        state.temas = await dataService.list('temas');
        state.actividades = await dataService.list('actividades');
        setPreloader('Listo', `${state.temas.length} temas · ${state.actividades.length} actividades`);
        renderCurrent();
        hidePreloader();
        setTimeout(() => {
            if (window.Highcharts) {
                Highcharts.charts.forEach(chart => {
                    if (chart) chart.reflow();
                });
            }
        }, 100);
    } catch (e) {
        setPreloader('Error al cargar', e.message || 'Reintenta en un momento', true);
    }
}

function getFilteredActividades() {
    const { actividades } = state;
    const desde = document.getElementById('filtro-global-desde')?.value || '';
    const hasta = document.getElementById('filtro-global-hasta')?.value || '';
    
    let list = actividades;
    if (desde) {
        list = list.filter(a => a.fechaCompromiso && a.fechaCompromiso >= desde);
    }
    if (hasta) {
        list = list.filter(a => a.fechaCompromiso && a.fechaCompromiso <= hasta);
    }
    return list;
}

function renderCurrent() {
    const { temas, view } = state;
    const filteredActividades = getFilteredActividades();
    
    poblarFiltroKanban(temas);
    switch (view) {
        case 'dashboard': renderDashboard(temas, filteredActividades); break;
        case 'temas': renderTemas(temas, filteredActividades, document.getElementById('filtro-temas').value); break;
        case 'kanban': renderKanban(temas, filteredActividades, document.getElementById('filtro-kanban-tema').value); break;
        case 'tabla': actividadesModule.renderTabla(temas, filteredActividades, getTablaFilters()); break;
        case 'gantt': renderGantt(temas, filteredActividades); break;
        case 'calendario': renderCalendario(temas, filteredActividades); break;
        case 'responsables': renderResponsables(temas, filteredActividades); break;
        case 'alertas': renderAlertas(temas, filteredActividades); break;
        case 'reportes': setReportesData(temas, filteredActividades); break;
    }
    // Alertas badge siempre
    renderAlertas(temas, filteredActividades);
}

function switchView(view) {
    state.view = view;
    document.querySelectorAll('.gestor-tab').forEach(t => t.classList.toggle('active', t.dataset.view === view));
    document.querySelectorAll('.gestor-view').forEach(v => {
        const isActive = v.id === `view-${view}`;
        v.classList.toggle('active', isActive);
        v.hidden = !isActive;
        v.setAttribute('aria-hidden', String(!isActive));
    });
    renderCurrent();
    
    // Multiple delayed reflows to ensure correct container size calculation on mobile
    [50, 150, 300, 500].forEach(delay => {
        setTimeout(() => {
            if (window.Highcharts) {
                Highcharts.charts.forEach(chart => {
                    if (chart) chart.reflow();
                });
            }
        }, delay);
    });
}

function wireEvents() {
    document.querySelectorAll('.gestor-tab').forEach(t => {
        t.onclick = () => switchView(t.dataset.view);
    });

    wireChartFullscreenButtons();

    document.getElementById('filtro-temas').oninput = () => renderTemas(state.temas, getFilteredActividades(), document.getElementById('filtro-temas').value);
    document.getElementById('btn-nuevo-tema').onclick = () => openTemaModal(null);

    const rerenderTabla = () => {
        if (typeof actividadesModule.resetTablaPage === 'function') {
            actividadesModule.resetTablaPage();
        }
        actividadesModule.renderTabla(state.temas, getFilteredActividades(), getTablaFilters());
    };

    document.getElementById('filtro-tabla').oninput = rerenderTabla;
    document.getElementById('filtro-tabla-tema').onchange = rerenderTabla;
    document.getElementById('filtro-tabla-estatus').onchange = rerenderTabla;
    document.getElementById('filtro-tabla-prioridad').onchange = rerenderTabla;
    document.getElementById('filtro-tabla-responsable').onchange = rerenderTabla;
    document.getElementById('tabla-page-size').onchange = (e) => {
        if (typeof actividadesModule.setTablaPageSize === 'function') {
            actividadesModule.setTablaPageSize(e.target.value);
        }
        actividadesModule.renderTabla(state.temas, getFilteredActividades(), getTablaFilters());
    };

    document.getElementById('tabla-page-prev').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(-1);
        }
        actividadesModule.renderTabla(state.temas, getFilteredActividades(), getTablaFilters());
    };

    document.getElementById('tabla-page-next').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(1);
        }
        actividadesModule.renderTabla(state.temas, getFilteredActividades(), getTablaFilters());
    };
    document.getElementById('btn-nueva-actividad').onclick = () => actividadesModule.openActividadModal(null, state.temas);
    document.getElementById('btn-export-excel').onclick = () => actividadesModule.exportarCsv(state.temas, getFilteredActividades());

    document.getElementById('filtro-kanban-tema').onchange = () => renderKanban(state.temas, getFilteredActividades(), document.getElementById('filtro-kanban-tema').value);

    document.getElementById('cal-prev').onclick = () => { calPrev(); renderCalendario(state.temas, getFilteredActividades()); };
    document.getElementById('cal-next').onclick = () => { calNext(); renderCalendario(state.temas, getFilteredActividades()); };

    // Eventos filtro fechas global
    const inputDesde = document.getElementById('filtro-global-desde');
    const inputHasta = document.getElementById('filtro-global-hasta');
    const btnLimpiar = document.getElementById('btn-limpiar-fechas');

    const handleFechaChange = () => {
        renderCurrent();
        // Forzar redibujado/reflow de gráficos tras aplicar filtros
        setTimeout(() => {
            if (window.Highcharts) {
                Highcharts.charts.forEach(chart => {
                    if (chart) chart.reflow();
                });
            }
        }, 150);
    };

    if (inputDesde) inputDesde.onchange = handleFechaChange;
    if (inputHasta) inputHasta.onchange = handleFechaChange;
    if (btnLimpiar) {
        btnLimpiar.onclick = () => {
            if (inputDesde) inputDesde.value = '';
            if (inputHasta) inputHasta.value = '';
            handleFechaChange();
        };
    }

    wireReportes();

    window.addEventListener('gestor:refresh', () => { showPreloader(); loadAll(); });

    // Indicador fuente
    const lbl = document.getElementById('source-label');
    if (lbl) {
        lbl.textContent = dataSource;
        if (isOnline) lbl.classList.add('online');
    }
}

wireEvents();
await loadAll();
