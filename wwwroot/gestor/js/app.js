// Orquestador principal.
import { dataService, dataSource, isOnline } from './data-service.js';
import { renderDashboard } from './dashboard.js';
import { renderTemas, openTemaModal } from './temas.js';
import * as actividadesModule from './actividades.js?v=tabla-v2';
import { renderKanban, poblarFiltroKanban } from './kanban.js';
import { renderGantt } from './gantt.js';
import { renderCalendario, calPrev, calNext } from './calendario.js';
import { renderResponsables } from './responsables.js';
import { renderAlertas } from './alertas.js';
import { setReportesData, wireReportes, renderDeck } from './reportes.js';
import { wireChartFullscreenButtons } from './charts.js';

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
    if (pre) setTimeout(() => pre.classList.add('is-hidden'), 200);
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
    } catch (e) {
        setPreloader('Error al cargar', e.message || 'Reintenta en un momento', true);
    }
}

function renderCurrent() {
    const { temas, actividades, view } = state;
    poblarFiltroKanban(temas);
    switch (view) {
        case 'dashboard': renderDashboard(temas, actividades); break;
        case 'temas': renderTemas(temas, actividades, document.getElementById('filtro-temas').value); break;
        case 'kanban': renderKanban(temas, actividades, document.getElementById('filtro-kanban-tema').value); break;
        case 'tabla': actividadesModule.renderTabla(temas, actividades, getTablaFilters()); break;
        case 'gantt': renderGantt(temas, actividades); break;
        case 'calendario': renderCalendario(temas, actividades); break;
        case 'responsables': renderResponsables(temas, actividades); break;
        case 'alertas': renderAlertas(temas, actividades); break;
        case 'reportes': setReportesData(temas, actividades); break;
    }
    // Alertas badge siempre
    renderAlertas(temas, actividades);
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
}

function wireEvents() {
    document.querySelectorAll('.gestor-tab').forEach(t => {
        t.onclick = () => switchView(t.dataset.view);
    });

    wireChartFullscreenButtons();

    document.getElementById('filtro-temas').oninput = () => renderTemas(state.temas, state.actividades, document.getElementById('filtro-temas').value);
    document.getElementById('btn-nuevo-tema').onclick = () => openTemaModal(null);

    const rerenderTabla = () => {
        if (typeof actividadesModule.resetTablaPage === 'function') {
            actividadesModule.resetTablaPage();
        }
        actividadesModule.renderTabla(state.temas, state.actividades, getTablaFilters());
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
        actividadesModule.renderTabla(state.temas, state.actividades, getTablaFilters());
    };

    document.getElementById('tabla-page-prev').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(-1);
        }
        actividadesModule.renderTabla(state.temas, state.actividades, getTablaFilters());
    };

    document.getElementById('tabla-page-next').onclick = () => {
        if (typeof actividadesModule.changeTablaPage === 'function') {
            actividadesModule.changeTablaPage(1);
        }
        actividadesModule.renderTabla(state.temas, state.actividades, getTablaFilters());
    };
    document.getElementById('btn-nueva-actividad').onclick = () => actividadesModule.openActividadModal(null, state.temas);
    document.getElementById('btn-export-excel').onclick = () => actividadesModule.exportarCsv(state.temas, state.actividades);

    document.getElementById('filtro-kanban-tema').onchange = () => renderKanban(state.temas, state.actividades, document.getElementById('filtro-kanban-tema').value);

    document.getElementById('cal-prev').onclick = () => { calPrev(); renderCalendario(state.temas, state.actividades); };
    document.getElementById('cal-next').onclick = () => { calNext(); renderCalendario(state.temas, state.actividades); };

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
