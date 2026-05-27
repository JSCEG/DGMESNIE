// Orquestador principal.
import { dataService, dataSource, isOnline } from './data-service.js';
import { renderDashboard } from './dashboard.js?v=charts-v3';
import { renderTemas, openActividadModal } from './temas.js?v=modal-v2';
import * as actividadesModule from './actividades.js?v=etapas-correo-v2';
import { renderKanban, poblarFiltroKanban } from './kanban.js';
import { renderGantt } from './gantt.js';
import { renderCalendario, calPrev, calNext } from './calendario.js';
import { renderResponsables } from './responsables.js?v=reporte-v2';
import { renderAlertas } from './alertas.js?v=recordatorio-v1';
import { setReportesData, wireReportes, renderDeck } from './reportes.js?v=presentation-v7';
import { setPresentacionData, wirePresentacion } from './reportes-presentar.js';
import { wireChartFullscreenButtons } from './charts.js?v=charts-v3';

const state = { temas: [], actividades: [], view: 'dashboard' };

function getTablaFilters() {
    return {
        search: document.getElementById('filtro-tabla')?.value || '',
        temaId: document.getElementById('filtro-tabla-tema')?.value || '', // Holds selected parent Activity ID
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
        state.temas = await dataService.list('temas'); // Child list
        state.actividades = await dataService.list('actividades'); // Parent list
        setPreloader('Listo', `${state.actividades.length} actividades · ${state.temas.length} temas`);
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

function getFilteredTemas() {
    const { temas } = state;
    const desde = document.getElementById('filtro-global-desde')?.value || '';
    const hasta = document.getElementById('filtro-global-hasta')?.value || '';
    
    let list = temas;
    if (desde) {
        list = list.filter(t => t.fechaCompromiso && t.fechaCompromiso >= desde);
    }
    if (hasta) {
        list = list.filter(t => t.fechaCompromiso && t.fechaCompromiso <= hasta);
    }
    return list;
}

function renderCurrent() {
    const { temas, actividades, view } = state;
    const filteredTemas = getFilteredTemas();
    
    poblarFiltroKanban(actividades);
    switch (view) {
        case 'dashboard': renderDashboard(actividades, filteredTemas); break;
        case 'temas': renderTemas(actividades, filteredTemas, document.getElementById('filtro-temas')?.value || '', document.getElementById('filtro-temas-responsable')?.value || ''); break;
        case 'kanban': renderKanban(actividades, filteredTemas, document.getElementById('filtro-kanban-tema').value); break;
        case 'tabla': actividadesModule.renderTabla(filteredTemas, actividades, getTablaFilters()); break;
        case 'gantt': renderGantt(actividades, filteredTemas); break;
        case 'calendario': renderCalendario(actividades, filteredTemas); break;
        case 'responsables': renderResponsables(actividades, filteredTemas); break;
        case 'alertas': renderAlertas(actividades, filteredTemas); break;
        case 'reportes-presentar': setPresentacionData(actividades, filteredTemas); break;
        case 'reportes': setReportesData(actividades, filteredTemas); break;
    }
    // Alertas badge siempre
    renderAlertas(actividades, filteredTemas);
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

    const rerenderTemas = () => {
        renderTemas(
            state.actividades, 
            getFilteredTemas(), 
            document.getElementById('filtro-temas')?.value || '',
            document.getElementById('filtro-temas-responsable')?.value || ''
        );
    };
    if (document.getElementById('filtro-temas')) {
        document.getElementById('filtro-temas').oninput = rerenderTemas;
    }
    if (document.getElementById('filtro-temas-responsable')) {
        document.getElementById('filtro-temas-responsable').onchange = rerenderTemas;
    }
    document.getElementById('btn-nuevo-tema').onclick = () => openActividadModal(null);

    const rerenderTabla = () => {
        if (typeof actividadesModule.resetTablaPage === 'function') {
            actividadesModule.resetTablaPage();
        }
        actividadesModule.renderTabla(getFilteredTemas(), state.actividades, getTablaFilters());
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
        actividadesModule.renderTabla(getFilteredTemas(), state.actividades, getTablaFilters());
    };

    document.getElementById('tabla-page-prev').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(-1);
        }
        actividadesModule.renderTabla(getFilteredTemas(), state.actividades, getTablaFilters());
    };

    document.getElementById('tabla-page-next').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(1);
        }
        actividadesModule.renderTabla(getFilteredTemas(), state.actividades, getTablaFilters());
    };
    document.getElementById('btn-nueva-actividad').onclick = () => actividadesModule.openTemaModal(null, state.actividades);
    document.getElementById('btn-export-excel').onclick = () => actividadesModule.exportarCsv(state.actividades, getFilteredTemas());

    document.getElementById('filtro-kanban-tema').onchange = () => renderKanban(state.actividades, getFilteredTemas(), document.getElementById('filtro-kanban-tema').value);

    document.getElementById('cal-prev').onclick = () => { calPrev(); renderCalendario(state.actividades, getFilteredTemas()); };
    document.getElementById('cal-next').onclick = () => { calNext(); renderCalendario(state.actividades, getFilteredTemas()); };

    // Eventos filtro fechas global
    const inputDesde = document.getElementById('filtro-global-desde');
    const inputHasta = document.getElementById('filtro-global-hasta');
    const selectPeriodo = document.getElementById('filtro-global-periodo');
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

    const handlePeriodoChange = () => {
        const val = selectPeriodo.value;
        if (val === 'personalizado') return;

        const hoy = new Date();
        let desde = null;
        let hasta = null;

        if (val === 'hoy') {
            desde = hoy;
            hasta = hoy;
        } else if (val === 'semana') {
            const currentDay = hoy.getDay();
            const diff = hoy.getDate() - currentDay + (currentDay === 0 ? -6 : 1);
            desde = new Date(hoy.setDate(diff));
            hasta = new Date(hoy.setDate(diff + 6));
        } else if (val === 'mes') {
            desde = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
            hasta = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0);
        } else if (val === 'anio') {
            desde = new Date(hoy.getFullYear(), 0, 1);
            hasta = new Date(hoy.getFullYear(), 11, 31);
        }

        if (desde && hasta) {
            const toISOStringLocalDate = (date) => {
                const offset = date.getTimezoneOffset();
                const localDate = new Date(date.getTime() - (offset * 60 * 1000));
                return localDate.toISOString().slice(0, 10);
            };
            if (inputDesde) inputDesde.value = toISOStringLocalDate(desde);
            if (inputHasta) inputHasta.value = toISOStringLocalDate(hasta);
            handleFechaChange();
        }
    };

    if (selectPeriodo) selectPeriodo.onchange = handlePeriodoChange;

    const resetPeriodoDropdown = () => {
        if (selectPeriodo) selectPeriodo.value = 'personalizado';
        handleFechaChange();
    };

    if (inputDesde) inputDesde.onchange = resetPeriodoDropdown;
    if (inputHasta) inputHasta.onchange = resetPeriodoDropdown;
    if (btnLimpiar) {
        btnLimpiar.onclick = () => {
            if (inputDesde) inputDesde.value = '';
            if (inputHasta) inputHasta.value = '';
            if (selectPeriodo) selectPeriodo.value = 'personalizado';
            handleFechaChange();
        };
    }

    wireReportes();
    wirePresentacion();

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
