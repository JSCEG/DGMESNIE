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

export function populateActividadesResponsablesFilter(actividades) {
    const sel = document.getElementById('filtro-temas-responsable');
    if (!sel) return;
    const prevValue = sel.value;
    
    const set = new Set();
    actividades.forEach(a => {
        if (a.responsablePrincipal) set.add(a.responsablePrincipal.trim());
        if (a.corresponsables) a.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    
    const responsibles = [...set].sort((x, y) => x.localeCompare(y, 'es-MX', { sensitivity: 'base' }));
    sel.innerHTML = '<option value="">Responsable</option>' +
        responsibles.map(r => `<option value="${escape(r)}">${escape(r)}</option>`).join('');
        
    if (prevValue && responsibles.includes(prevValue)) {
        sel.value = prevValue;
    }
}

export function renderTemas(actividades, temas, filtro = '', responsableFilter = '') {
    _actsState = actividades; _temasState = temas;
    populateActividadesResponsablesFilter(actividades);

    const f = (filtro || '').toLowerCase();
    let filtered = actividades;

    if (f) {
        filtered = filtered.filter(a =>
            a.actividad.toLowerCase().includes(f) ||
            (a.responsablePrincipal || '').toLowerCase().includes(f) ||
            (a.categoria || '').toLowerCase().includes(f)
        );
    }

    if (responsableFilter) {
        filtered = filtered.filter(a =>
            a.responsablePrincipal === responsableFilter ||
            (a.corresponsables && a.corresponsables.some(c => c.nombre === responsableFilter))
        );
    }

    const cont = document.getElementById('cards-temas');
    cont.innerHTML = filtered.length
        ? filtered.map(a => {
            const av = avancePromedio(a, temas);
            const sem = semaforoTema(a, temas);
            const numTemas = temas.filter(t => t.actividadId === a.id).length;
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';
            
            // Initials avatar
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

            // WhatsApp & Email share text
            const shareText = encodeURIComponent(
                `📋 Actividad: ${a.actividad}\n` +
                `👤 Responsable: ${a.responsablePrincipal || '—'}\n` +
                `📅 Compromiso: ${a.fechaCompromiso || '—'}\n` +
                `📊 Avance: ${av}%\n` +
                `🔴🟡🟢 Estatus: ${a.estatus || '—'}`
            );
            const waUrl  = `https://wa.me/?text=${shareText}`;
            const mailUrl = `mailto:?subject=${encodeURIComponent('Actividad: ' + a.actividad)}&body=${shareText.replace(/%0A/g, '%0D%0A')}`;
            
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

                    <!-- ── Barra de acciones ── -->
                    <div class="tema-card__actions" style="display: flex; align-items: center; justify-content: flex-end; gap: 4px; padding: 8px 14px 10px; border-top: 1px solid rgba(138,0,49,0.07); margin-top: 6px;">
                        <button type="button" class="tca-btn tca-view" data-id="${a.id}"
                            title="Ver detalle"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(10,94,149,.08);color:#0a5e95;">
                            <i class="fa-solid fa-eye"></i> Ver
                        </button>
                        <button type="button" class="tca-btn tca-edit" data-id="${a.id}"
                            title="Editar actividad"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(138,0,49,.08);color:#8a0031;">
                            <i class="fa-solid fa-pen-to-square"></i> Editar
                        </button>
                        <button type="button" class="tca-btn tca-delete" data-id="${a.id}"
                            title="Eliminar actividad"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(192,34,42,.08);color:#c0222a;">
                            <i class="fa-solid fa-trash"></i> Eliminar
                        </button>
                        <a href="${mailUrl}" class="tca-btn"
                            title="Compartir por correo"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(71,85,105,.08);color:#475569;text-decoration:none;">
                            <i class="fa-solid fa-envelope"></i>
                        </a>
                        <a href="${waUrl}" target="_blank" rel="noopener" class="tca-btn"
                            title="Compartir por WhatsApp"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(37,211,102,.1);color:#128C7E;text-decoration:none;">
                            <i class="fa-brands fa-whatsapp"></i>
                        </a>
                    </div>
                </article>`;
        }).join('')
        : '<p style="grid-column:1/-1;text-align:center;color:var(--g-text-soft);padding:2rem">Sin actividades que coincidan</p>';

    // ── Bind action buttons ──
    cont.querySelectorAll('.tca-view').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openActividadDetalle(act, temas);
        };
    });

    cont.querySelectorAll('.tca-edit').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openActividadModal(act);
        };
    });

    cont.querySelectorAll('.tca-delete').forEach(btn => {
        btn.onclick = async (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (!act) return;
            if (!confirm(`¿Eliminar la actividad "${act.actividad}" y todos sus temas? Esta acción no se puede deshacer.`)) return;
            try {
                const childTemas = _temasState.filter(t => t.actividadId === act.id);
                for (const t of childTemas) await dataService.remove('temas', t.id);
                await dataService.remove('actividades', act.id);
                toast('Actividad eliminada', 'ok');
                window.dispatchEvent(new CustomEvent('gestor:refresh'));
            } catch (err) {
                toast(err.message || 'Error al eliminar', 'err');
            }
        };
    });

    // Click en la tarjeta (fuera de botones) → editar
    cont.querySelectorAll('.tema-card').forEach(card => {
        card.onclick = (e) => {
            if (e.target.closest('.tca-btn, .tema-card__link-btn, a')) return;
            openActividadModal(actividades.find(x => x.id === card.dataset.id));
        };
    });
}

/* ── Modal de detalle (solo lectura) ── */
export function openActividadDetalle(a, temas) {
    const av = avancePromedio(a, temas);
    const sem = semaforoTema(a, temas);
    const semLabels = { verde: '🟢 A tiempo', amarillo: '🟡 Por vencer', rojo: '🔴 Vencido', gris: '⚫ Bloqueado/Sin fecha' };
    const numTemas = temas.filter(t => t.actividadId === a.id).length;
    const corrList = a.corresponsables && a.corresponsables.length
        ? a.corresponsables.map(c => `<span style="display:inline-flex;align-items:center;gap:5px;background:rgba(30,91,79,.08);border-radius:6px;padding:2px 8px;font-size:0.78rem;color:#1e5b4f;font-weight:600;">${escape(c.nombre)}</span>`).join(' ')
        : '<span style="color:var(--texto-suave);font-size:0.82rem;">Sin corresponsables</span>';

    const html = `
        <div style="display:flex;flex-direction:column;gap:1rem;">
            <div style="display:flex;align-items:center;gap:10px;padding:10px 14px;border-radius:10px;background:rgba(138,0,49,0.04);border-left:4px solid #8a0031;">
                <span class="semaforo ${sem}" style="width:12px;height:12px;flex-shrink:0;margin-top:0;"></span>
                <div>
                    <div style="font-size:0.7rem;text-transform:uppercase;font-weight:700;color:#8a0031;letter-spacing:.05em;">Actividad</div>
                    <div style="font-weight:700;font-size:1rem;color:#0f172a;">${escape(a.actividad)}</div>
                </div>
                <span style="margin-left:auto;font-size:0.78rem;font-weight:700;color:#8a0031;">${semLabels[sem] || sem}</span>
            </div>

            ${a.descripcion ? `<p style="margin:0;font-size:0.87rem;color:#475569;line-height:1.55;">${escape(a.descripcion)}</p>` : ''}

            <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Responsable</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(a.responsablePrincipal || '—')}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Categoría</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(a.categoria || '—')}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Inicio</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${fmtDate(a.fechaInicio)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Compromiso</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${fmtDate(a.fechaCompromiso)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Prioridad · Estatus</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(a.prioridad)} · ${escape(a.estatus)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Temas</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${numTemas} tema${numTemas !== 1 ? 's' : ''}</div>
                </div>
            </div>

            <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:6px;">Avance</div>
                <div style="display:flex;align-items:center;gap:10px;">
                    <div style="flex:1;height:8px;border-radius:99px;background:rgba(138,0,49,.1);overflow:hidden;">
                        <div style="height:100%;width:${av}%;border-radius:99px;background:${sem==='rojo'?'#c0222a':sem==='amarillo'?'#d97706':'#027a48'};transition:width .4s;"></div>
                    </div>
                    <span style="font-weight:700;font-size:0.9rem;color:#0f172a;">${av}%</span>
                </div>
            </div>

            <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:6px;">Corresponsables</div>
                <div style="display:flex;flex-wrap:wrap;gap:5px;">${corrList}</div>
            </div>

            ${a.comentariosEjecutivos ? `
            <div style="background:#fffbeb;border-radius:8px;padding:10px 14px;border-left:3px solid #d97706;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#d97706;margin-bottom:4px;">Comentarios</div>
                <div style="font-size:0.85rem;color:#475569;line-height:1.5;">${escape(a.comentariosEjecutivos)}</div>
            </div>` : ''}

            ${a.ligaSharePoint ? `
            <a href="${escape(a.ligaSharePoint)}" target="_blank" rel="noopener"
               style="display:inline-flex;align-items:center;gap:8px;padding:8px 14px;border-radius:8px;background:rgba(180,137,52,.1);color:#b48934;font-size:0.82rem;font-weight:700;text-decoration:none;align-self:flex-start;">
                <i class="fa-solid fa-folder-open"></i> Abrir en SharePoint
            </a>` : ''}
        </div>`;

    openModal(`Detalle: ${a.actividad}`, html, null);
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
