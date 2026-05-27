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
        temaId: String(filters.temaId || '').trim(), // This holds selected Activity ID
        estatus: (filters.estatus || '').trim(),
        prioridad: (filters.prioridad || '').trim(),
        responsable: (filters.responsable || '').trim()
    };
}

function populateTemasFilter(actividades) {
    const sel = document.getElementById('filtro-tabla-tema');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const opts = actividades
        .slice()
        .sort((a, b) => String(a.actividad || '').localeCompare(String(b.actividad || ''), 'es-MX', { sensitivity: 'base' }))
        .map(a => `<option value="${a.id}">${escape(a.actividad || 'Sin actividad')}</option>`)
        .join('');

    sel.innerHTML = '<option value="">Actividad</option>' + opts;

    if (prevValue && actividades.some(a => String(a.id) === prevValue)) {
        sel.value = prevValue;
    }
}

function populateResponsablesFilter(temas) {
    const sel = document.getElementById('filtro-tabla-responsable');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const set = new Set();
    temas.forEach(t => {
        if (t.responsable) set.add(t.responsable.trim());
        if (t.corresponsables) t.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    const responsables = [...set].sort((a, b) => a.localeCompare(b, 'es-MX', { sensitivity: 'base' }));

    sel.innerHTML = '<option value="">Responsable</option>'
        + responsables.map(r => `<option value="${escape(r)}">${escape(r)}</option>`).join('');

    if (prevValue && responsables.includes(prevValue)) {
        sel.value = prevValue;
    }
}

function filterActividades(actividades, temas, rawFilters = {}) {
    const filters = buildFilters(rawFilters);

    return temas.filter(t => {
        const actividad = actividades.find(a => a.id === t.actividadId);
        const corrSearchNames = t.corresponsables ? t.corresponsables.map(c => c.nombre).join(' ') : '';
        const matchesSearch = !filters.search || [t.tema, t.responsable, t.estatus, t.prioridad, actividad?.actividad, corrSearchNames]
            .some(x => (x || '').toLowerCase().includes(filters.search));
        const matchesActividad = !filters.temaId || String(t.actividadId) === filters.temaId;
        const matchesEstatus = !filters.estatus || t.estatus === filters.estatus;
        const matchesPrioridad = !filters.prioridad || t.prioridad === filters.prioridad;
        const matchesResponsable = !filters.responsable || t.responsable === filters.responsable || (t.corresponsables && t.corresponsables.some(c => c.nombre === filters.responsable));

        return matchesSearch && matchesActividad && matchesEstatus && matchesPrioridad && matchesResponsable;
    });
}

function tiempoSemaforoData(tema) {
    const sem = semaforo(tema);
    const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';

    const rest = daysFromToday(tema.fechaCompromiso);
    const hasRange = !!tema.fechaInicio && !!tema.fechaCompromiso;
    const totalDays = hasRange ? Math.max(1, daysBetween(tema.fechaInicio, tema.fechaCompromiso)) : null;

    let percent = 0;
    if (typeof totalDays === 'number' && totalDays > 0 && typeof rest === 'number') {
        const elapsed = Math.min(totalDays, Math.max(0, totalDays - rest));
        percent = Math.round((elapsed / totalDays) * 100);
    }

    let label = 'Sin fecha';
    if (tema.estatus === 'Concluida') {
        label = 'Concluida';
    } else if (tema.bloqueada) {
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
        info.textContent = `Mostrando ${start} a ${end} de ${total} temas`;
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
    // Note: We swap the arguments inside app.js call as well
    populateTemasFilter(actividades);
    populateResponsablesFilter(temas);

    const filtered = filterActividades(actividades, temas, filters);
    const totalPages = Math.max(1, Math.ceil(filtered.length / tablaPageSize));
    tablaPage = Math.min(Math.max(1, tablaPage), totalPages);

    const startIndex = (tablaPage - 1) * tablaPageSize;
    const pageItems = filtered.slice(startIndex, startIndex + tablaPageSize);

    const tbody = document.querySelector('#tbl-actividades tbody');
    tbody.innerHTML = pageItems.length
        ? pageItems.map(t => {
            const actividad = actividades.find(a => a.id === t.actividadId);
            const sem = semaforo(t);
            const tiempo = tiempoSemaforoData(t);
            const coText = t.corresponsables && t.corresponsables.length
                ? `<br><small class="muted" style="font-size: 0.72rem; display: block; margin-top: 2px;">Co: ${escape(t.corresponsables.map(c => c.nombre.split(' ')[0]).join(', '))}</small>`
                : '';
            return `
                <tr data-id="${t.id}">
                    <td>${escape(actividad?.actividad || '—')}</td>
                    <td>${escape(t.tema)}</td>
                    <td>${escape(t.responsable)}${coText}</td>
                    <td>${fmtDate(t.fechaInicio)}</td>
                    <td>${fmtDate(t.fechaCompromiso)}</td>
                    <td><span class="status-pill ${t.estatus === 'Concluida' ? 'status-pill--complete' : t.estatus === 'En proceso' ? 'status-pill--progress' : t.estatus === 'Vencida' ? 'status-pill--issue' : ''}">${escape(t.estatus)}</span></td>
                    <td>
                        <div class="tiempo-cell">
                            <div class="track ${tiempo.trackCls}" style="width:110px"><span style="width:${tiempo.percent}%"></span></div>
                            <small class="muted">${escape(tiempo.label)} · ${escape(tiempo.detail)}</small>
                        </div>
                    </td>
                    <td><div class="track ${sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending'}" style="width:90px"><span style="width:${t.avance || 0}%"></span></div><small class="muted">${t.avance || 0}%</small></td>
                    <td><span class="chip ${t.prioridad === 'Alta' ? 'p-alta' : t.prioridad === 'Media' ? 'p-media' : 'p-baja'}">${escape(t.prioridad)}</span></td>
                    <td><span class="semaforo ${sem}"></span></td>
                    <td>
                        <span class="tabla-actions-cell">
                            ${t.evidenciaUrl ? `<a href="${escape(t.evidenciaUrl)}" target="_blank" rel="noopener" class="tabla-link-btn" title="Abrir evidencia" style="margin-right: 8px; display: inline-flex; align-items: center;"><i class="fa-solid fa-folder-open" style="font-size: 1.15rem; color: #b48934;"></i></a>` : ''}
                            <button class="tabla-action-btn" data-email="${t.id}" title="Enviar por correo" style="background: none; border: none; padding: 4px 8px; cursor: pointer; margin-right: 4px; display: inline-flex; align-items: center; border-radius: 4px; transition: background 0.2s;">
                                <i class="fa-solid fa-envelope" style="font-size: 1.15rem; color: #8a0031;"></i>
                            </button>
                            <button class="internal-button" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem" data-edit="${t.id}">Editar</button>
                        </span>
                    </td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="11" style="text-align:center;color:var(--g-text-soft);padding:1.5rem">Sin temas</td></tr>';

    tablaPage = renderPagination(filtered.length, tablaPage, tablaPageSize);

    tbody.querySelectorAll('[data-edit]').forEach(btn => {
        btn.onclick = () => openTemaModal(temas.find(t => t.id === btn.dataset.edit), actividades);
    });

    tbody.querySelectorAll('[data-email]').forEach(btn => {
        btn.onclick = () => openSendEmailModal(temas.find(t => t.id === btn.dataset.email));
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

export async function openTemaModal(tema, actividades) {
    const t = tema || {
        actividadId: actividades[0]?.id || '', tema: '', descripcion: '', responsable: '', responsableId: '',
        fechaInicio: '', fechaCompromiso: '', estatus: 'Pendiente', prioridad: 'Media',
        avance: 0, bloqueada: false, motivoBloqueo: '', evidenciaUrl: '', comentarios: '', corresponsablesIds: []
    };
    if (!t.corresponsablesIds) t.corresponsablesIds = [];

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(t.responsableId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === t.responsable);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? '');
    
    const responsableOptions = [
        '<option value="">Selecciona responsable</option>',
        ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(selectedUserId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !tema;
    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Actividad *</label>
                    <select name="actividadId" required>${actividades.map(a => `<option value="${a.id}" ${t.actividadId === a.id ? 'selected' : ''}>${escape(a.actividad)}</option>`).join('')}</select>
                </div>
                <div class="form-field full"><label>Tema *</label><input name="tema" required value="${escape(t.tema)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(t.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Responsable *</label><select name="responsableId" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${t.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${t.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${t.fechaCompromiso || ''}"></div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Pendiente', 'En proceso', 'Concluida', 'Vencida'].map(p => `<option ${t.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Avance (%)</label><input type="number" min="0" max="100" name="avance" value="${t.avance || 0}"></div>
                <div class="form-field"><label>Bloqueada</label>
                    <select name="bloqueada"><option value="false" ${!t.bloqueada ? 'selected' : ''}>No</option><option value="true" ${t.bloqueada ? 'selected' : ''}>Sí</option></select>
                </div>
                <div class="form-field"><label>Motivo bloqueo</label><input name="motivoBloqueo" value="${escape(t.motivoBloqueo || '')}"></div>
                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Corresponsables</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label class="corresponsable-label" data-id="${u.idUsuario}" style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="corresponsablesIds" value="${u.idUsuario}" ${t.corresponsablesIds.includes(u.idUsuario) ? 'checked' : ''} style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
                <div class="form-field full"><label>Evidencia URL</label><input type="url" name="evidenciaUrl" value="${escape(t.evidenciaUrl || '')}"></div>
                <div class="form-field full"><label>Comentarios</label><textarea name="comentarios">${escape(t.comentarios || '')}</textarea></div>
                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Enviar notificación por correo a:</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="notificarUsuariosIds" value="${u.idUsuario}" style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-tema">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;

    const close = openModal(isNew ? 'Nuevo tema' : 'Editar tema', html, async (data, close) => {
        const submitBtn = document.querySelector('#modal-body form button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = isNew ? 'Creando...' : 'Guardando...';
        }
        try {
            data.avance = Number(data.avance);
            data.bloqueada = data.bloqueada === 'true';
            data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
            if (data.avance === 100) data.estatus = 'Concluida';

            const corresponsablesCbs = document.querySelectorAll('#modal-body form input[name="corresponsablesIds"]:checked');
            data.corresponsablesIds = Array.from(corresponsablesCbs).map(cb => Number(cb.value));

            const checkboxes = document.querySelectorAll('#modal-body form input[name="notificarUsuariosIds"]:checked');
            data.notificarUsuariosIds = Array.from(checkboxes).map(cb => Number(cb.value));

            if (isNew) {
                await dataService.create('temas', data);
                toast('Tema creado', 'ok');
            } else {
                await dataService.update('temas', tema.id, data);
                toast('Tema actualizado', 'ok');
            }
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        } catch (err) {
            console.error(err);
            let userMsg = err.message || 'No fue posible guardar el tema.';
            if (userMsg.includes('blocked') || userMsg.includes('bloqueada')) {
                userMsg = 'Solicitud bloqueada por validación de seguridad (ej. liga con caracteres no permitidos).';
            } else if (userMsg.includes('{')) {
                try {
                    const jsonStr = userMsg.substring(userMsg.indexOf('{'));
                    const parsed = JSON.parse(jsonStr);
                    if (parsed.message) userMsg = parsed.message;
                } catch (_) {}
            }
            toast(userMsg, 'err');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = isNew ? 'Crear' : 'Guardar';
            }
        }
    });

    const respSelect = document.querySelector('#modal-body form select[name="responsableId"]');
    const updateCorresponsablesChecklist = () => {
        const selectedId = respSelect ? respSelect.value : '';
        const labels = document.querySelectorAll('#modal-body form .corresponsable-label');
        labels.forEach(label => {
            const labelId = label.dataset.id;
            if (labelId === selectedId) {
                label.style.display = 'none';
                const input = label.querySelector('input[type="checkbox"]');
                if (input) input.checked = false;
            } else {
                label.style.display = 'flex';
            }
        });
    };
    
    if (respSelect) {
        respSelect.addEventListener('change', updateCorresponsablesChecklist);
        updateCorresponsablesChecklist();
    }

    if (!isNew) {
        document.getElementById('btn-del-tema').onclick = async () => {
            if (!confirm('¿Eliminar tema?')) return;
            await dataService.remove('temas', tema.id);
            toast('Tema eliminado', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}

export function exportarCsv(actividades, temas) {
    const rows = [['Actividad', 'Tema', 'Responsable', 'Inicio', 'Compromiso', 'Estatus', 'Avance', 'Prioridad', 'Bloqueada', 'Evidencia']];
    temas.forEach(t => {
        const a = actividades.find(x => x.id === t.actividadId);
        rows.push([a?.actividad, t.tema, t.responsable, t.fechaInicio, t.fechaCompromiso, t.estatus, t.avance, t.prioridad, t.bloqueada ? 'Sí' : 'No', t.evidenciaUrl]);
    });
    downloadCsv('temas.csv', rows);
    toast('CSV descargado', 'ok');
}

export async function openSendEmailModal(tema) {
    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const html = `
        <form id="compartir-email-form">
            <div class="form-row">
                <div class="form-field full">
                    <p style="margin-bottom: 12px; font-size: 0.9rem; color: var(--texto-suave);">
                        Seleccione uno o más usuarios para enviar los detalles del tema <strong>${escape(tema.clave)} - ${escape(tema.tema)}</strong> por correo electrónico:
                    </p>
                    <div class="usuarios-check-list" style="max-height: 220px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="notificarUsuariosIds" value="${u.idUsuario}" style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)} ${u.correo ? `(${escape(u.correo)})` : ''}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
            </div>
            <div class="form-actions" style="margin-top: 15px;">
                <button type="button" class="internal-button" id="btn-cancel-share">Cancelar</button>
                <button type="submit" class="internal-button internal-button--primary">Enviar por correo</button>
            </div>
        </form>
    `;

    const close = openModal(`Enviar tema por correo`, html, null);
    
    document.getElementById('btn-cancel-share').onclick = close;

    const form = document.getElementById('compartir-email-form');
    form.onsubmit = async (e) => {
        e.preventDefault();
        const checkboxes = form.querySelectorAll('input[name="notificarUsuariosIds"]:checked');
        const userIds = Array.from(checkboxes).map(cb => Number(cb.value));

        if (userIds.length === 0) {
            toast('Debe seleccionar al menos un usuario.', 'err');
            return;
        }

        try {
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Enviando...';

            const token = document.querySelector('meta[name="csrf-token"]')?.content ?? '';

            const res = await fetch(`/Gestor/Api/Temas/${tema.id}/Notificar`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ usuarioIds: userIds })
            });

            if (!res.ok) {
                const body = await res.json().catch(() => null);
                throw new Error(body?.error || `Error ${res.status}`);
            }

            toast('Correo(s) enviado(s) con éxito', 'ok');
            close();
        } catch (err) {
            toast(err.message || 'No fue posible enviar el correo', 'err');
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.textContent = 'Enviar por correo';
        }
    };
}
