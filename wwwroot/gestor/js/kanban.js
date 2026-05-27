import { escape, fmtDate, semaforo } from './utils.js';
import { dataService } from './data-service.js';
import { openTemaModal } from './actividades.js';

const COLS = ['Pendiente', 'En proceso', 'Vencida', 'Concluida'];

export function renderKanban(actividades, temas, actividadIdFilter = '') {
    const board = document.getElementById('kanban-board');
    const ts = actividadIdFilter ? temas.filter(t => t.actividadId === actividadIdFilter) : temas;

    board.innerHTML = COLS.map(col => {
        const items = ts.filter(t => t.estatus === col);
        return `
            <div class="kanban-col" data-col="${col}">
                <h4>${col} <span class="count">${items.length}</span></h4>
                ${items.map(t => {
                    const actividad = actividades.find(a => a.id === t.actividadId);
                    const coLabel = t.corresponsables && t.corresponsables.length
                        ? ` (+${t.corresponsables.length})`
                        : '';
                    return `
                        <div class="kanban-card" draggable="true" data-id="${t.id}">
                            <h5>${escape(t.tema)}</h5>
                            <div style="font-size:.72rem;color:var(--g-text-soft)">${escape(actividad?.actividad || '')}</div>
                            <div class="meta">
                                <span>${escape(t.responsable)}${coLabel}</span>
                                <span><span class="semaforo ${semaforo(t)}"></span> ${fmtDate(t.fechaCompromiso)}</span>
                            </div>
                        </div>`;
                }).join('')}
            </div>`;
    }).join('');

    // Drag and drop
    let dragging = null;
    board.querySelectorAll('.kanban-card').forEach(card => {
        card.addEventListener('dragstart', () => { dragging = card; card.classList.add('dragging'); });
        card.addEventListener('dragend', () => { card.classList.remove('dragging'); dragging = null; });
        card.addEventListener('click', () => {
            const t = temas.find(x => x.id === card.dataset.id);
            openTemaModal(t, actividades);
        });
    });
    board.querySelectorAll('.kanban-col').forEach(col => {
        col.addEventListener('dragover', e => { e.preventDefault(); col.classList.add('drop-target'); });
        col.addEventListener('dragleave', () => col.classList.remove('drop-target'));
        col.addEventListener('drop', async e => {
            e.preventDefault();
            col.classList.remove('drop-target');
            if (!dragging) return;
            const nuevoEstatus = col.dataset.col;
            const id = dragging.dataset.id;
            const patch = { estatus: nuevoEstatus, fechaUltimaActualizacion: new Date().toISOString().slice(0, 10) };
            if (nuevoEstatus === 'Concluida') patch.avance = 100;
            await dataService.update('temas', id, patch);
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        });
    });
}

export function poblarFiltroKanban(actividades) {
    const sel = document.getElementById('filtro-kanban-tema');
    if (!sel) return;
    const current = sel.value;
    sel.innerHTML = '<option value="">Todas las actividades</option>' +
        actividades.map(a => `<option value="${a.id}">${escape(a.actividad)}</option>`).join('');
    sel.value = current;
}
