import { escape, fmtDate, priClass, estClass, semaforoTema, avancePromedio, openModal, toast } from './utils.js';
import { dataService } from './data-service.js';

let _temasState = [];
let _actsState = [];

function normalizeUserName(value) {
    return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .toLowerCase();
}

export function renderTemas(temas, actividades, filtro = '') {
    _temasState = temas; _actsState = actividades;
    const f = (filtro || '').toLowerCase();
    const filtered = temas.filter(t =>
        !f ||
        t.tema.toLowerCase().includes(f) ||
        (t.responsablePrincipal || '').toLowerCase().includes(f) ||
        (t.categoria || '').toLowerCase().includes(f)
    );

    const cont = document.getElementById('cards-temas');
    cont.innerHTML = filtered.length
        ? filtered.map(t => {
            const av = avancePromedio(t, actividades);
            const sem = semaforoTema(t, actividades);
            const numActs = actividades.filter(a => a.temaId === t.id).length;
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';
            // Generate initials from responsible name
            const initials = (t.responsablePrincipal || '??')
                .split(' ')
                .filter(w => w.length > 0)
                .slice(0, 2)
                .map(w => w[0].toUpperCase())
                .join('');
            const calSvg = '<svg viewBox="0 0 16 16"><path d="M5 0v1H3.5A1.5 1.5 0 0 0 2 2.5v11A1.5 1.5 0 0 0 3.5 15h9a1.5 1.5 0 0 0 1.5-1.5v-11A1.5 1.5 0 0 0 12.5 1H11V0h-1v1H6V0H5Zm-2 5h10v8.5a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V5Z"/></svg>';
            const actSvg = '<svg viewBox="0 0 16 16"><path d="M2 2.5A.5.5 0 0 1 2.5 2h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5Zm0 3A.5.5 0 0 1 2.5 5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5Zm.5 2.5a.5.5 0 0 0 0 1h4a.5.5 0 0 0 0-1h-4Zm0 3a.5.5 0 0 0 0 1h3a.5.5 0 0 0 0-1h-3ZM8.5 5a.5.5 0 0 0-.5.5v5a.5.5 0 0 0 .5.5h5a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5h-5Z"/></svg>';
            const hasLink = !!(t.ligaSharePoint);
            return `
                <article class="tema-card s-${sem}" data-id="${t.id}">
                    <div class="tema-card__head">
                        <h3>${escape(t.tema)}</h3>
                        <span class="tema-card__head-actions">
                            ${hasLink ? `<a href="${escape(t.ligaSharePoint)}" target="_blank" rel="noopener" class="tema-card__link-btn" title="Abrir enlace en SharePoint" style="display: inline-flex; align-items: center; justify-content: center;"><i class="fa-solid fa-folder-open" style="font-size: 1.15rem; color: #b48934;"></i></a>` : ''}
                            <span class="semaforo ${sem}" title="Semáforo"></span>
                        </span>
                    </div>
                    <div class="tema-card__meta">
                        <span class="chip ${priClass(t.prioridad)}">${escape(t.prioridad)}</span>
                        <span class="chip">${escape(t.estatus)}</span>
                        <span class="chip">${escape(t.categoria || 'Sin categoría')}</span>
                    </div>
                    <p class="tema-card__desc">${escape(t.descripcion || '')}</p>
                    <div class="tema-card__progress">
                        <div class="track ${trackCls}"><span style="width:${av}%"></span></div>
                        <span class="tema-card__pct">${av}%</span>
                    </div>
                    <div class="tema-card__foot">
                        <span class="tema-card__avatar">
                            <span class="tema-card__avatar-circle">${initials}</span>
                            ${escape(t.responsablePrincipal)}
                        </span>
                        <span class="tema-card__foot-info">
                            ${actSvg} ${numActs}
                            ${calSvg} ${fmtDate(t.fechaCompromiso)}
                        </span>
                    </div>
                </article>`;
        }).join('')
        : '<p style="grid-column:1/-1;text-align:center;color:var(--g-text-soft);padding:2rem">Sin temas que coincidan</p>';

    // Bind card clicks (edit modal)
    cont.querySelectorAll('.tema-card').forEach(card => {
        card.onclick = (e) => {
            // Don't open modal if they clicked the link icon
            if (e.target.closest('.tema-card__link-btn')) return;
            openTemaModal(temas.find(x => x.id === card.dataset.id));
        };
    });
}

export async function openTemaModal(tema) {
    const t = tema || {
        tema: '', descripcion: '', categoria: '', prioridad: 'Media', estatus: 'Activo',
        responsablePrincipal: '', fechaInicio: '', fechaCompromiso: '', avanceGeneral: 0,
        ligaSharePoint: '', comentariosEjecutivos: ''
    };

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es-MX', { sensitivity: 'base' }));

    const hasCurrent = filteredUsers.some(u => u.nombre === t.responsablePrincipal);
    const responsableOptions = [
        '<option value="">Selecciona responsable</option>',
        ...(!hasCurrent && t.responsablePrincipal
            ? [`<option value="${escape(t.responsablePrincipal)}" selected>${escape(t.responsablePrincipal)}</option>`]
            : []),
        ...filteredUsers.map(u => `<option value="${escape(u.nombre)}" ${u.nombre === t.responsablePrincipal ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !tema;
    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Tema *</label><input name="tema" required value="${escape(t.tema)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(t.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Categoría</label><input name="categoria" value="${escape(t.categoria || '')}"></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${t.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Activo', 'Pausado', 'Concluido'].map(p => `<option ${t.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Responsable principal *</label><select name="responsablePrincipal" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${t.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${t.fechaCompromiso || ''}"></div>
                <div class="form-field"><label>Avance (%)</label><input type="number" min="0" max="100" name="avanceGeneral" value="${t.avanceGeneral || 0}"></div>
                <div class="form-field full"><label>Liga SharePoint</label><input type="url" name="ligaSharePoint" value="${escape(t.ligaSharePoint || '')}"></div>
                <div class="form-field full"><label>Comentarios ejecutivos</label><textarea name="comentariosEjecutivos">${escape(t.comentariosEjecutivos || '')}</textarea></div>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-tema">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;
    const close = openModal(isNew ? 'Nuevo tema' : 'Editar tema', html, async (data, close) => {
        data.avanceGeneral = Number(data.avanceGeneral);
        data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
        if (isNew) {
            await dataService.create('temas', data);
            toast('Tema creado', 'ok');
        } else {
            await dataService.update('temas', tema.id, data);
            toast('Tema actualizado', 'ok');
        }
        close();
        window.dispatchEvent(new CustomEvent('gestor:refresh'));
    });
    if (!isNew) {
        document.getElementById('btn-del-tema').onclick = async () => {
            if (!confirm('¿Eliminar tema y sus actividades?')) return;
            const acts = _actsState.filter(a => a.temaId === tema.id);
            for (const a of acts) await dataService.remove('actividades', a.id);
            await dataService.remove('temas', tema.id);
            toast('Tema eliminado', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}
