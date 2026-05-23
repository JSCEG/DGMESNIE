import { escape, fmtDate, semaforo, daysBetween, daysFromToday, openModal, toast, downloadCsv } from './utils.js';
import { dataService } from './data-service.js';

const DEFAULT_PAGE_SIZE = 8;
let tablaPage = 1;
let tablaPageSize = DEFAULT_PAGE_SIZE;

function normalizeUserName(value) {
    return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .toLowerCase();
}

function buildFilters(filters = {}) {
    return {
        search: (filters.search || '').trim().toLowerCase(),
        temaId: String(filters.temaId || '').trim(),
        estatus: (filters.estatus || '').trim(),
        prioridad: (filters.prioridad || '').trim(),
        responsable: (filters.responsable || '').trim()
    };
}

function populateTemasFilter(temas) {
    const sel = document.getElementById('filtro-tabla-tema');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const opts = temas
        .slice()
        .sort((a, b) => String(a.tema || '').localeCompare(String(b.tema || ''), 'es-MX', { sensitivity: 'base' }))
        .map(t => `<option value="${t.id}">${escape(t.tema || 'Sin tema')}</option>`)
        .join('');

    sel.innerHTML = '<option value="">Tema</option>' + opts;

    if (prevValue && temas.some(t => String(t.id) === prevValue)) {
        sel.value = prevValue;
    }
}

function populateResponsablesFilter(actividades) {
    const sel = document.getElementById('filtro-tabla-responsable');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const responsables = [...new Set(actividades.map(a => (a.responsable || '').trim()).filter(Boolean))]
        .sort((a, b) => a.localeCompare(b, 'es-MX', { sensitivity: 'base' }));

    sel.innerHTML = '<option value="">Responsable</option>'
        + responsables.map(r => `<option value="${escape(r)}">${escape(r)}</option>`).join('');

    if (prevValue && responsables.includes(prevValue)) {
        sel.value = prevValue;
    }
}

function filterActividades(temas, actividades, rawFilters = {}) {
    const filters = buildFilters(rawFilters);

    return actividades.filter(a => {
        const tema = temas.find(t => t.id === a.temaId);

        const matchesSearch = !filters.search || [a.actividad, a.responsable, a.estatus, a.prioridad, tema?.tema]
            .some(x => (x || '').toLowerCase().includes(filters.search));
        const matchesTema = !filters.temaId || String(a.temaId) === filters.temaId;
        const matchesEstatus = !filters.estatus || a.estatus === filters.estatus;
        const matchesPrioridad = !filters.prioridad || a.prioridad === filters.prioridad;
        const matchesResponsable = !filters.responsable || a.responsable === filters.responsable;

        return matchesSearch && matchesTema && matchesEstatus && matchesPrioridad && matchesResponsable;
    });
}

function tiempoSemaforoData(actividad) {
    const sem = semaforo(actividad);
    const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';

    const rest = daysFromToday(actividad.fechaCompromiso);
    const hasRange = !!actividad.fechaInicio && !!actividad.fechaCompromiso;
    const totalDays = hasRange ? Math.max(1, daysBetween(actividad.fechaInicio, actividad.fechaCompromiso)) : null;

    let percent = 0;
    if (typeof totalDays === 'number' && totalDays > 0 && typeof rest === 'number') {
        const elapsed = Math.min(totalDays, Math.max(0, totalDays - rest));
        percent = Math.round((elapsed / totalDays) * 100);
    }

    let label = 'Sin fecha';
    if (actividad.estatus === 'Concluida') {
        label = 'Concluida';
    } else if (actividad.bloqueada) {
        label = 'Bloqueada';
    } else if (typeof rest === 'number') {
        label = rest < 0 ? `Vencida ${Math.abs(rest)}d` : `Quedan ${rest}d`;
    }

    const detail = typeof totalDays === 'number'
        ? `${percent}% del plazo`
        : 'Sin rango completo';

    return { sem, trackCls, percent, label, detail };
}

function renderPagination(total, currentPage, pageSize) {
    const totalPages = Math.max(1, Math.ceil(total / pageSize));
    const page = Math.min(Math.max(1, currentPage), totalPages);
    const info = document.getElementById('tabla-pagination-info');
    const label = document.getElementById('tabla-page-current');
    const prev = document.getElementById('tabla-page-prev');
    const next = document.getElementById('tabla-page-next');

    const start = total ? ((page - 1) * pageSize) + 1 : 0;
    const end = total ? Math.min(page * pageSize, total) : 0;

    if (info) {
        info.textContent = `Mostrando ${start} a ${end} de ${total} actividades`;
    }
    if (label) {
        label.textContent = `${page} / ${totalPages}`;
    }
    if (prev) {
        prev.disabled = page <= 1;
    }
    if (next) {
        next.disabled = page >= totalPages;
    }

    return page;
}

export function renderTabla(temas, actividades, filters = {}) {
    populateTemasFilter(temas);
    populateResponsablesFilter(actividades);

    const filtered = filterActividades(temas, actividades, filters);
    const totalPages = Math.max(1, Math.ceil(filtered.length / tablaPageSize));
    tablaPage = Math.min(Math.max(1, tablaPage), totalPages);

    const startIndex = (tablaPage - 1) * tablaPageSize;
    const pageItems = filtered.slice(startIndex, startIndex + tablaPageSize);

    const tbody = document.querySelector('#tbl-actividades tbody');
    tbody.innerHTML = pageItems.length
        ? pageItems.map(a => {
            const tema = temas.find(t => t.id === a.temaId);
            const sem = semaforo(a);
            const tiempo = tiempoSemaforoData(a);
            return `
                <tr data-id="${a.id}">
                    <td>${escape(tema?.tema || '—')}</td>
                    <td>${escape(a.actividad)}</td>
                    <td>${escape(a.responsable)}</td>
                    <td>${fmtDate(a.fechaInicio)}</td>
                    <td>${fmtDate(a.fechaCompromiso)}</td>
                    <td><span class="status-pill ${a.estatus === 'Concluida' ? 'status-pill--complete' : a.estatus === 'En proceso' ? 'status-pill--progress' : a.estatus === 'Vencida' ? 'status-pill--issue' : ''}">${escape(a.estatus)}</span></td>
                    <td>
                        <div class="tiempo-cell">
                            <div class="track ${tiempo.trackCls}" style="width:110px"><span style="width:${tiempo.percent}%"></span></div>
                            <small class="muted">${escape(tiempo.label)} · ${escape(tiempo.detail)}</small>
                        </div>
                    </td>
                    <td><div class="track ${sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending'}" style="width:90px"><span style="width:${a.avance || 0}%"></span></div><small class="muted">${a.avance || 0}%</small></td>
                    <td><span class="chip ${a.prioridad === 'Alta' ? 'p-alta' : a.prioridad === 'Media' ? 'p-media' : 'p-baja'}">${escape(a.prioridad)}</span></td>
                    <td><span class="semaforo ${sem}"></span></td>
                    <td><button class="internal-button" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem" data-edit="${a.id}">Editar</button></td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="11" style="text-align:center;color:var(--g-text-soft);padding:1.5rem">Sin actividades</td></tr>';

    tablaPage = renderPagination(filtered.length, tablaPage, tablaPageSize);

    tbody.querySelectorAll('[data-edit]').forEach(btn => {
        btn.onclick = () => openActividadModal(actividades.find(a => a.id === btn.dataset.edit), temas);
    });

}

export function changeTablaPage(step) {
    tablaPage = Math.max(1, tablaPage + step);
}

export function resetTablaPage() {
    tablaPage = 1;
}

export function setTablaPageSize(size) {
    const parsed = Number(size);
    if (!Number.isFinite(parsed) || parsed <= 0) {
        tablaPageSize = DEFAULT_PAGE_SIZE;
    } else {
        tablaPageSize = parsed;
    }
    tablaPage = 1;
}

export async function openActividadModal(actividad, temas) {
    const a = actividad || {
        temaId: temas[0]?.id || '', actividad: '', descripcion: '', responsable: '', responsableId: '',
        fechaInicio: '', fechaCompromiso: '', estatus: 'Pendiente', prioridad: 'Media',
        avance: 0, bloqueada: false, motivoBloqueo: '', evidenciaUrl: '', comentarios: ''
    };

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(a.responsableId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === a.responsable);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? '');
    const responsableOptions = [
        '<option value="">Selecciona responsable</option>',
        ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(selectedUserId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !actividad;
    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Tema *</label>
                    <select name="temaId" required>${temas.map(t => `<option value="${t.id}" ${a.temaId === t.id ? 'selected' : ''}>${escape(t.tema)}</option>`).join('')}</select>
                </div>
                <div class="form-field full"><label>Actividad *</label><input name="actividad" required value="${escape(a.actividad)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(a.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Responsable *</label><select name="responsableId" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${a.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${a.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${a.fechaCompromiso || ''}"></div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Pendiente', 'En proceso', 'Concluida', 'Vencida'].map(p => `<option ${a.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Avance (%)</label><input type="number" min="0" max="100" name="avance" value="${a.avance || 0}"></div>
                <div class="form-field"><label>Bloqueada</label>
                    <select name="bloqueada"><option value="false" ${!a.bloqueada ? 'selected' : ''}>No</option><option value="true" ${a.bloqueada ? 'selected' : ''}>Sí</option></select>
                </div>
                <div class="form-field"><label>Motivo bloqueo</label><input name="motivoBloqueo" value="${escape(a.motivoBloqueo || '')}"></div>
                <div class="form-field full"><label>Evidencia URL</label><input type="url" name="evidenciaUrl" value="${escape(a.evidenciaUrl || '')}"></div>
                <div class="form-field full"><label>Comentarios</label><textarea name="comentarios">${escape(a.comentarios || '')}</textarea></div>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-act">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;
    const close = openModal(isNew ? 'Nueva actividad' : 'Editar actividad', html, async (data, close) => {
        data.avance = Number(data.avance);
        data.bloqueada = data.bloqueada === 'true';
        data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
        if (data.avance === 100) data.estatus = 'Concluida';
        if (isNew) {
            await dataService.create('actividades', data);
            toast('Actividad creada', 'ok');
        } else {
            await dataService.update('actividades', actividad.id, data);
            toast('Actividad actualizada', 'ok');
        }
        close();
        window.dispatchEvent(new CustomEvent('gestor:refresh'));
    });
    if (!isNew) {
        document.getElementById('btn-del-act').onclick = async () => {
            if (!confirm('¿Eliminar actividad?')) return;
            await dataService.remove('actividades', actividad.id);
            toast('Actividad eliminada', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}

export function exportarCsv(temas, actividades) {
    const rows = [['Tema', 'Actividad', 'Responsable', 'Inicio', 'Compromiso', 'Estatus', 'Avance', 'Prioridad', 'Bloqueada', 'Evidencia']];
    actividades.forEach(a => {
        const t = temas.find(x => x.id === a.temaId);
        rows.push([t?.tema, a.actividad, a.responsable, a.fechaInicio, a.fechaCompromiso, a.estatus, a.avance, a.prioridad, a.bloqueada ? 'Sí' : 'No', a.evidenciaUrl]);
    });
    downloadCsv('actividades.csv', rows);
    toast('CSV descargado', 'ok');
}
