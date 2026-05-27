import { escape, fmtDate, daysFromToday, openModal, toast } from './utils.js';
import { dataService } from './data-service.js';

export function generarAlertas(actividades, temas) {
    const alertas = [];
    temas.forEach(t => {
        if (t.estatus === 'Concluida') return;
        const dr = daysFromToday(t.fechaCompromiso);
        const actividad = actividades.find(a => a.id === t.actividadId);
        const ctx = `${actividad?.actividad || ''} · ${t.responsable}`;
        if (dr < 0) {
            alertas.push({ tipo: 'vencida', titulo: `Vencida: ${t.tema}`, msg: `${ctx} · venció ${fmtDate(t.fechaCompromiso)} (hace ${Math.abs(dr)} d)`, act: t });
        } else if (dr <= 7) {
            alertas.push({ tipo: 'por-vencer', titulo: `Por vencer: ${t.tema}`, msg: `${ctx} · vence ${fmtDate(t.fechaCompromiso)} (en ${dr} d)`, act: t });
        }
        if (t.bloqueada) {
            alertas.push({ tipo: 'bloqueada', titulo: `Bloqueada: ${t.tema}`, msg: `${ctx} · ${t.motivoBloqueo || 'Sin motivo'}`, act: t });
        }
        if (t.fechaUltimaActualizacion) {
            const sinAct = daysFromToday(t.fechaUltimaActualizacion);
            if (sinAct !== null && sinAct < -14) {
                alertas.push({ tipo: 'sin-actualizar', titulo: `Sin actualizar: ${t.tema}`, msg: `${ctx} · última actualización ${fmtDate(t.fechaUltimaActualizacion)}`, act: t });
            }
        }
    });
    return alertas;
}

export function renderAlertas(actividades, temas) {
    const alertas = generarAlertas(actividades, temas);
    const cont = document.getElementById('alertas-list');
    if (!cont) return;
    document.getElementById('badge-alertas').textContent = alertas.length;

    cont.innerHTML = alertas.length
        ? alertas.map(al => {
            const hasResponsable = al.act && al.act.responsableId;
            return `
            <div class="alerta-item tipo-${al.tipo}">
                <div style="flex-grow: 1;">
                    <h5>${escape(al.titulo)}</h5>
                    <p>${escape(al.msg)}</p>
                </div>
                <div class="alerta-actions">
                    ${hasResponsable ? `
                        <button class="btn-recordatorio internal-button" data-act-id="${al.act.id}" title="Enviar recordatorio por correo" style="min-height: 32px; padding: 0.3rem 0.7rem; font-size: 0.78rem; display: inline-flex; align-items: center; gap: 6px;">
                            <i class="fa-solid fa-paper-plane"></i> Recordatorio
                        </button>
                    ` : ''}
                    <span class="chip">${al.tipo.replace('-', ' ')}</span>
                </div>
            </div>`;
        }).join('')
        : '<p style="text-align:center;color:var(--g-text-soft);padding:2rem">Sin alertas activas</p>';

    // Bind event listeners
    cont.querySelectorAll('.btn-recordatorio').forEach(btn => {
        btn.onclick = () => {
            const actId = btn.dataset.actId;
            const t = temas.find(x => String(x.id) === actId);
            if (t) {
                confirmarYEnviarRecordatorio(t, actividades);
            }
        };
    });
}

async function confirmarYEnviarRecordatorio(tema, actividades) {
    function normalizeUserName(value) {
        return String(value || '')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .trim()
            .toLowerCase();
    }

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(tema.responsableId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === tema.responsable);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? null);

    const html = `
        <form id="recordatorio-form" style="margin: 0;">
            <p style="margin-bottom: 12px; font-size: 0.9rem; color: var(--texto-suave);">
                Seleccione uno o más usuarios para enviar el recordatorio del tema por correo electrónico:
            </p>
            
            <div class="usuarios-check-list" style="max-height: 180px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02); margin-bottom: 15px;">
                ${filteredUsers.map(u => `
                    <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                        <input type="checkbox" name="notificarUsuariosIds" value="${u.idUsuario}" ${u.idUsuario === selectedUserId ? 'checked' : ''} style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                        <span>${escape(u.nombre)} ${u.correo ? `(${escape(u.correo)})` : ''}</span>
                    </label>
                `).join('')}
            </div>

            <div style="background: rgba(138, 0, 49, 0.04); border-left: 4px solid var(--guinda); padding: 12px; margin-bottom: 1.5rem; border-radius: 4px;">
                <div style="font-size: 0.75rem; text-transform: uppercase; color: var(--guinda); font-weight: 700; margin-bottom: 4px;">Tema</div>
                <div style="font-weight: 700; color: var(--texto); font-size: 0.95rem;">${escape(tema.tema)}</div>
                <div style="font-size: 0.85rem; color: var(--texto-suave); margin-top: 4px;">
                    Fecha Compromiso: ${fmtDate(tema.fechaCompromiso)}
                </div>
            </div>
            
            <div style="display: flex; justify-content: flex-end; gap: 10px;">
                <button type="button" id="btn-cancelar-envio" class="internal-button" style="min-height: 36px; padding: 0.5rem 1rem;">Cancelar</button>
                <button type="submit" id="btn-confirmar-envio" class="internal-button internal-button--primary" style="min-height: 36px; padding: 0.5rem 1rem; display: inline-flex; align-items: center; gap: 8px;">
                    Confirmar
                </button>
            </div>
        </form>
    `;

    const close = openModal('Confirmar envío de recordatorio', html, null);

    const btnCancel = document.getElementById('btn-cancelar-envio');
    const btnConfirm = document.getElementById('btn-confirmar-envio');
    const form = document.getElementById('recordatorio-form');

    if (btnCancel) btnCancel.onclick = close;

    if (form) {
        form.onsubmit = async (e) => {
            e.preventDefault();
            
            const checkboxes = form.querySelectorAll('input[name="notificarUsuariosIds"]:checked');
            const userIds = Array.from(checkboxes).map(cb => Number(cb.value));

            if (userIds.length === 0) {
                toast('Debe seleccionar al menos un usuario.', 'err');
                return;
            }

            // Disable buttons and show "Enviando..."
            if (btnCancel) btnCancel.disabled = true;
            if (btnConfirm) {
                btnConfirm.disabled = true;
                btnConfirm.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Enviando...';
            }

            try {
                const token = document.querySelector('meta[name="csrf-token"]')?.content ?? '';
                const res = await fetch(`/Gestor/Api/Temas/${tema.id}/Recordatorio`, {
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

                // Show "Enviado!" success state
                if (btnConfirm) {
                    btnConfirm.innerHTML = '<i class="fa-solid fa-check"></i> ¡Enviado!';
                }
                toast('El recordatorio se ha enviado correctamente por correo.', 'ok');
                
                setTimeout(() => {
                    close();
                }, 1000);
            } catch (err) {
                console.error(err);
                toast(err.message || 'Error al enviar el recordatorio', 'err');
                
                // Restore buttons
                if (btnCancel) btnCancel.disabled = false;
                if (btnConfirm) {
                    btnConfirm.disabled = false;
                    btnConfirm.innerHTML = 'Confirmar';
                }
            }
        };
    }
}
