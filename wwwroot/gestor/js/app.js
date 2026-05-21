// Orquestador principal.
import { dataService, dataSource, isOnline } from './data-service.js';
import { renderDashboard } from './dashboard.js';
import { renderTemas, openTemaModal } from './temas.js';
import { renderTabla, openActividadModal, exportarCsv } from './actividades.js';
import { renderKanban, poblarFiltroKanban } from './kanban.js';
import { renderGantt } from './gantt.js';
import { renderCalendario, calPrev, calNext } from './calendario.js';
import { renderResponsables } from './responsables.js';
import { renderAlertas } from './alertas.js';
import { setReportesData, wireReportes, renderDeck } from './reportes.js';

const state = { temas: [], actividades: [], view: 'dashboard' };

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
        case 'tabla': renderTabla(temas, actividades, document.getElementById('filtro-tabla').value); break;
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
    document.querySelectorAll('.gestor-view').forEach(v => v.classList.toggle('active', v.id === `view-${view}`));
    renderCurrent();
}

function wireEvents() {
    document.querySelectorAll('.gestor-tab').forEach(t => {
        t.onclick = () => switchView(t.dataset.view);
    });

    document.getElementById('filtro-temas').oninput = () => renderTemas(state.temas, state.actividades, document.getElementById('filtro-temas').value);
    document.getElementById('btn-nuevo-tema').onclick = () => openTemaModal(null);

    document.getElementById('filtro-tabla').oninput = () => renderTabla(state.temas, state.actividades, document.getElementById('filtro-tabla').value);
    document.getElementById('btn-nueva-actividad').onclick = () => openActividadModal(null, state.temas);
    document.getElementById('btn-export-excel').onclick = () => exportarCsv(state.temas, state.actividades);

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
