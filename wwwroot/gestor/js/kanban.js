import { escape, fmtDate, semaforo, toast } from './utils.js';
import { dataService } from './data-service.js';
import { openTemaModal } from './actividades.js?v=etapas-correo-v2';

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
                    const participantes = participantesPorEtapa(t);
                    const coLabel = participantes
                        ? ` <span title="${escape(participantes)}" style="color:#1e5b4f;font-weight:700;">(+)</span>`
                        : '';
                    const stagesCount = t.etapas && t.etapas.length > 0
                        ? `<span class="badge-etapas" style="font-size:0.68rem;font-weight:700;color:var(--guinda);background:rgba(155, 34, 71,0.06);padding:1px 5px;border-radius:4px;display:inline-flex;align-items:center;gap:3px;margin-left:auto;" title="Este tema tiene ${t.etapas.length} etapas"><i class="fa-solid fa-route" style="font-size:0.62rem;"></i> ${t.etapas.length} etapas</span>`
                        : '';
                    return `
                        <div class="kanban-card" draggable="true" data-id="${t.id}">
                            <h5 style="display:flex;align-items:center;justify-content:space-between;gap:4px;margin:0 0 6px 0;">
                                <span style="flex-grow:1;">${escape(t.tema)}</span>
                                ${stagesCount}
                            </h5>
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
            
            const t = temas.find(x => String(x.id) === String(id));
            if (!t) return;

            const patch = {};
            if (t.etapas && t.etapas.length > 0) {
                const activeStg = t.etapas.find(st => st.avance < 100) || t.etapas[t.etapas.length - 1];
                if (nuevoEstatus === 'Concluida') {
                    activeStg.avance = 100;
                    activeStg.estatus = 'Concluida';
                } else if (nuevoEstatus === 'En proceso') {
                    activeStg.avance = 50;
                    activeStg.estatus = 'En proceso';
                } else {
                    activeStg.avance = 0;
                    activeStg.estatus = nuevoEstatus;
                }
                patch.etapas = t.etapas;
            } else {
                patch.estatus = nuevoEstatus;
                if (nuevoEstatus === 'Concluida') {
                    patch.avance = 100;
                } else if (nuevoEstatus === 'En proceso') {
                    patch.avance = 50;
                } else {
                    patch.avance = 0;
                }
            }

            patch.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
            try {
                await dataService.update('temas', id, patch);
                window.dispatchEvent(new CustomEvent('gestor:refresh'));
            } catch (err) {
                console.error(err);
                toast(err.message || 'No fue posible actualizar el tema.', 'err');
                window.dispatchEvent(new CustomEvent('gestor:refresh'));
            }
        });
    });
}

function participantesPorEtapa(t) {
    if (!Array.isArray(t.etapas)) return '';
    return t.etapas
        .map(e => {
            const items = [];
            if (e.responsableNombre && e.responsableNombre !== t.responsable) {
                items.push(`Resp.: ${e.responsableNombre}`);
            }
            if (Array.isArray(e.corresponsables) && e.corresponsables.length) {
                items.push(`Co.: ${e.corresponsables.map(c => c.nombre).join(', ')}`);
            }
            return items.length ? `${e.nombre || 'Etapa'}: ${items.join(' / ')}` : '';
        })
        .filter(Boolean)
        .join(' · ');
}

export function poblarFiltroKanban(actividades) {
    const sel = document.getElementById('filtro-kanban-tema');
    if (!sel) return;
    const current = sel.value;
    sel.innerHTML = '<option value="">Todas las actividades</option>' +
        actividades.map(a => `<option value="${a.id}">${escape(a.actividad)}</option>`).join('');
    sel.value = current;
}
