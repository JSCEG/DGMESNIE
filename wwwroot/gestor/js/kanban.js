import { escape, fmtDate, semaforo } from './utils.js';
import { dataService } from './data-service.js';
import { openActividadModal } from './actividades.js';

const COLS = ['Pendiente', 'En proceso', 'Vencida', 'Concluida'];

export function renderKanban(temas, actividades, temaIdFilter = '') {
    const board = document.getElementById('kanban-board');
    const acts = temaIdFilter ? actividades.filter(a => a.temaId === temaIdFilter) : actividades;

    board.innerHTML = COLS.map(col => {
        const items = acts.filter(a => a.estatus === col);
        return `
            <div class="kanban-col" data-col="${col}">
                <h4>${col} <span class="count">${items.length}</span></h4>
                ${items.map(a => {
                    const tema = temas.find(t => t.id === a.temaId);
                    return `
                        <div class="kanban-card" draggable="true" data-id="${a.id}">
                            <h5>${escape(a.actividad)}</h5>
                            <div style="font-size:.72rem;color:var(--g-text-soft)">${escape(tema?.tema || '')}</div>
                            <div class="meta">
                                <span>${escape(a.responsable)}</span>
                                <span><span class="semaforo ${semaforo(a)}"></span> ${fmtDate(a.fechaCompromiso)}</span>
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
            const a = actividades.find(x => x.id === card.dataset.id);
            openActividadModal(a, temas);
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
            await dataService.update('actividades', id, patch);
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        });
    });
}

export function poblarFiltroKanban(temas) {
    const sel = document.getElementById('filtro-kanban-tema');
    const current = sel.value;
    sel.innerHTML = '<option value="">Todos los temas</option>' +
        temas.map(t => `<option value="${t.id}">${escape(t.tema)}</option>`).join('');
    sel.value = current;
}
