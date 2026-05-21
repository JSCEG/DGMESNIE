import { escape, fmtDate, semaforo, openModal, toast, downloadCsv } from './utils.js';
import { dataService } from './data-service.js';

export function renderTabla(temas, actividades, filtro = '') {
    const f = (filtro || '').toLowerCase();
    const filtered = actividades.filter(a => {
        if (!f) return true;
        const tema = temas.find(t => t.id === a.temaId);
        return [a.actividad, a.responsable, a.estatus, a.prioridad, tema?.tema]
            .some(x => (x || '').toLowerCase().includes(f));
    });

    const tbody = document.querySelector('#tbl-actividades tbody');
    tbody.innerHTML = filtered.length
        ? filtered.map(a => {
            const tema = temas.find(t => t.id === a.temaId);
            const sem = semaforo(a);
            return `
                <tr data-id="${a.id}">
                    <td>${escape(tema?.tema || '—')}</td>
                    <td>${escape(a.actividad)}</td>
                    <td>${escape(a.responsable)}</td>
                    <td>${fmtDate(a.fechaInicio)}</td>
                    <td>${fmtDate(a.fechaCompromiso)}</td>
                    <td><span class="status-pill ${a.estatus === 'Concluida' ? 'status-pill--complete' : a.estatus === 'En proceso' ? 'status-pill--progress' : a.estatus === 'Vencida' ? 'status-pill--issue' : ''}">${escape(a.estatus)}</span></td>
                    <td><div class="track ${sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending'}" style="width:90px"><span style="width:${a.avance || 0}%"></span></div><small class="muted">${a.avance || 0}%</small></td>
                    <td><span class="chip ${a.prioridad === 'Alta' ? 'p-alta' : a.prioridad === 'Media' ? 'p-media' : 'p-baja'}">${escape(a.prioridad)}</span></td>
                    <td><span class="semaforo ${sem}"></span></td>
                    <td><button class="internal-button" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem" data-edit="${a.id}">Editar</button></td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="10" style="text-align:center;color:var(--g-text-soft);padding:1.5rem">Sin actividades</td></tr>';

    tbody.querySelectorAll('[data-edit]').forEach(btn => {
        btn.onclick = () => openActividadModal(actividades.find(a => a.id === btn.dataset.edit), temas);
    });
}

export function openActividadModal(actividad, temas) {
    const a = actividad || {
        temaId: temas[0]?.id || '', actividad: '', descripcion: '', responsable: '',
        fechaInicio: '', fechaCompromiso: '', estatus: 'Pendiente', prioridad: 'Media',
        avance: 0, bloqueada: false, motivoBloqueo: '', evidenciaUrl: '', comentarios: ''
    };
    const isNew = !actividad;
    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Tema *</label>
                    <select name="temaId" required>${temas.map(t => `<option value="${t.id}" ${a.temaId === t.id ? 'selected' : ''}>${escape(t.tema)}</option>`).join('')}</select>
                </div>
                <div class="form-field full"><label>Actividad *</label><input name="actividad" required value="${escape(a.actividad)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(a.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Responsable *</label><input name="responsable" required value="${escape(a.responsable)}"></div>
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
