import { escape, fmtDate, priClass, estClass, semaforoTema, avancePromedio, openModal, toast } from './utils.js';
import { dataService } from './data-service.js';

let _actsState = [];
let _temasState = [];

function normalizeUserName(value) {
    return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .toLowerCase();
}

export function renderTemas(actividades, temas, filtro = '') {
    _actsState = actividades; _temasState = temas;
    const f = (filtro || '').toLowerCase();
    const filtered = actividades.filter(a =>
        !f ||
        a.actividad.toLowerCase().includes(f) ||
        (a.responsablePrincipal || '').toLowerCase().includes(f) ||
        (a.categoria || '').toLowerCase().includes(f)
    );

    const cont = document.getElementById('cards-temas');
    cont.innerHTML = filtered.length
        ? filtered.map(a => {
            const av = avancePromedio(a, temas);
            const sem = semaforoTema(a, temas);
            const numTemas = temas.filter(t => t.actividadId === a.id).length;
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';
            
            // Generate initials from responsible principal name
            const initials = (a.responsablePrincipal || '??')
                .split(' ')
                .filter(w => w.length > 0)
                .slice(0, 2)
                .map(w => w[0].toUpperCase())
                .join('');
            
            const calSvg = '<svg viewBox="0 0 16 16"><path d="M5 0v1H3.5A1.5 1.5 0 0 0 2 2.5v11A1.5 1.5 0 0 0 3.5 15h9a1.5 1.5 0 0 0 1.5-1.5v-11A1.5 1.5 0 0 0 12.5 1H11V0h-1v1H6V0H5Zm-2 5h10v8.5a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V5Z"/></svg>';
            const actSvg = '<svg viewBox="0 0 16 16"><path d="M2 2.5A.5.5 0 0 1 2.5 2h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5Zm0 3A.5.5 0 0 1 2.5 5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5Zm.5 2.5a.5.5 0 0 0 0 1h4a.5.5 0 0 0 0-1h-4Zm0 3a.5.5 0 0 0 0 1h3a.5.5 0 0 0 0-1h-3ZM8.5 5a.5.5 0 0 0-.5.5v5a.5.5 0 0 0 .5.5h5a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5h-5Z"/></svg>';
            const hasLink = !!(a.ligaSharePoint);
            const corrNames = a.corresponsables && a.corresponsables.length
                ? ' + ' + a.corresponsables.map(c => c.nombre.split(' ')[0]).join(', ')
                : '';
            
            return `
                <article class="tema-card s-${sem}" data-id="${a.id}">
                    <div class="tema-card__head">
                        <h3>${escape(a.actividad)}</h3>
                        <span class="tema-card__head-actions">
                            ${hasLink ? `<a href="${escape(a.ligaSharePoint)}" target="_blank" rel="noopener" class="tema-card__link-btn" title="Abrir enlace en SharePoint" style="display: inline-flex; align-items: center; justify-content: center;"><i class="fa-solid fa-folder-open" style="font-size: 1.15rem; color: #b48934;"></i></a>` : ''}
                            <span class="semaforo ${sem}" title="Semáforo"></span>
                        </span>
                    </div>
                    <div class="tema-card__meta">
                        <span class="chip ${priClass(a.prioridad)}">${escape(a.prioridad)}</span>
                        <span class="chip">${escape(a.estatus)}</span>
                        <span class="chip">${escape(a.categoria || 'Sin categoría')}</span>
                    </div>
                    <p class="tema-card__desc">${escape(a.descripcion || '')}</p>
                    <div class="tema-card__progress">
                        <div class="track ${trackCls}"><span style="width:${av}%"></span></div>
                        <span class="tema-card__pct">${av}%</span>
                    </div>
                    <div class="tema-card__foot">
                        <span class="tema-card__avatar">
                            <span class="tema-card__avatar-circle">${initials}</span>
                            ${escape(a.responsablePrincipal)}<span class="corresponsables-subtext" style="font-size: 0.75rem; color: var(--texto-suave); font-weight: 500;" title="Corresponsables: ${escape(a.corresponsables?.map(c => c.nombre).join(', ') || '')}">${escape(corrNames)}</span>
                        </span>
                        <span class="tema-card__foot-info">
                            ${actSvg} ${numTemas}
                            ${calSvg} ${fmtDate(a.fechaCompromiso)}
                        </span>
                    </div>
                </article>`;
        }).join('')
        : '<p style="grid-column:1/-1;text-align:center;color:var(--g-text-soft);padding:2rem">Sin actividades que coincidan</p>';

    // Bind card clicks (edit modal)
    cont.querySelectorAll('.tema-card').forEach(card => {
        card.onclick = (e) => {
            if (e.target.closest('.tema-card__link-btn')) return;
            openActividadModal(actividades.find(x => x.id === card.dataset.id));
        };
    });
}

export async function openActividadModal(actividad) {
    const a = actividad || {
        actividad: '', descripcion: '', categoria: '', prioridad: 'Media', estatus: 'Activo',
        responsablePrincipal: '', responsablePrincipalId: '', fechaInicio: '', fechaCompromiso: '',
        ligaSharePoint: '', comentariosEjecutivos: '', corresponsablesIds: []
    };
    if (!a.corresponsablesIds) a.corresponsablesIds = [];

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(a.responsablePrincipalId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === a.responsablePrincipal);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? '');

    const responsableOptions = [
        '<option value="">Selecciona responsable principal</option>',
        ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(selectedUserId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !actividad;
    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Actividad *</label><input name="actividad" required value="${escape(a.actividad)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(a.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Categoría</label><input name="categoria" value="${escape(a.categoria || '')}"></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${a.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Activo', 'Pausado', 'Concluido'].map(p => `<option ${a.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Responsable principal *</label><select name="responsablePrincipalId" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${a.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${a.fechaCompromiso || ''}"></div>
                
                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Corresponsables</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label class="corresponsable-label" data-id="${u.idUsuario}" style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="corresponsablesIds" value="${u.idUsuario}" ${a.corresponsablesIds.includes(u.idUsuario) ? 'checked' : ''} style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
                <div class="form-field full"><label>Liga SharePoint</label><input type="url" name="ligaSharePoint" value="${escape(a.ligaSharePoint || '')}"></div>
                <div class="form-field full"><label>Comentarios</label><textarea name="comentariosEjecutivos">${escape(a.comentariosEjecutivos || '')}</textarea></div>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-act">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;

    const close = openModal(isNew ? 'Nueva actividad' : 'Editar actividad', html, async (data, close) => {
        const submitBtn = document.querySelector('#modal-body form button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = isNew ? 'Creando...' : 'Guardando...';
        }
        try {
            data.responsablePrincipalId = Number(data.responsablePrincipalId);
            data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
            
            const corresponsablesCbs = document.querySelectorAll('#modal-body form input[name="corresponsablesIds"]:checked');
            data.corresponsablesIds = Array.from(corresponsablesCbs).map(cb => Number(cb.value));

            if (isNew) {
                await dataService.create('actividades', data);
                toast('Actividad creada', 'ok');
            } else {
                await dataService.update('actividades', actividad.id, data);
                toast('Actividad actualizada', 'ok');
            }
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        } catch (err) {
            console.error(err);
            let userMsg = err.message || 'No fue posible guardar la actividad.';
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

    const respSelect = document.querySelector('#modal-body form select[name="responsablePrincipalId"]');
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
        document.getElementById('btn-del-act').onclick = async () => {
            if (!confirm('¿Eliminar actividad y todos sus temas?')) return;
            const childTemas = _temasState.filter(t => t.actividadId === actividad.id);
            for (const t of childTemas) {
                await dataService.remove('temas', t.id);
            }
            await dataService.remove('actividades', actividad.id);
            toast('Actividad eliminada', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}
