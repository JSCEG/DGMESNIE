import { escape, fmtDate, daysFromToday, openModal, toast } from './utils.js';

export function generarAlertas(temas, actividades) {
    const alertas = [];
    actividades.forEach(a => {
        if (a.estatus === 'Concluida') return;
        const dr = daysFromToday(a.fechaCompromiso);
        const tema = temas.find(t => t.id === a.temaId);
        const ctx = `${tema?.tema || ''} · ${a.responsable}`;
        if (dr < 0) {
            alertas.push({ tipo: 'vencida', titulo: `Vencida: ${a.actividad}`, msg: `${ctx} · venció ${fmtDate(a.fechaCompromiso)} (hace ${Math.abs(dr)} d)`, act: a });
        } else if (dr <= 7) {
            alertas.push({ tipo: 'por-vencer', titulo: `Por vencer: ${a.actividad}`, msg: `${ctx} · vence ${fmtDate(a.fechaCompromiso)} (en ${dr} d)`, act: a });
        }
        if (a.bloqueada) {
            alertas.push({ tipo: 'bloqueada', titulo: `Bloqueada: ${a.actividad}`, msg: `${ctx} · ${a.motivoBloqueo || 'Sin motivo'}`, act: a });
        }
        if (a.fechaUltimaActualizacion) {
            const sinAct = daysFromToday(a.fechaUltimaActualizacion);
            if (sinAct !== null && sinAct < -14) {
                alertas.push({ tipo: 'sin-actualizar', titulo: `Sin actualizar: ${a.actividad}`, msg: `${ctx} · última actualización ${fmtDate(a.fechaUltimaActualizacion)}`, act: a });
            }
        }
    });
    return alertas;
}

export function renderAlertas(temas, actividades) {
    const alertas = generarAlertas(temas, actividades);
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
            const act = actividades.find(a => String(a.id) === actId);
            if (act) {
                confirmarYEnviarRecordatorio(act);
            }
        };
    });
}

function confirmarYEnviarRecordatorio(act) {
    const html = `
        <form id="recordatorio-form" style="margin: 0;">
            <p style="margin-bottom: 1.25rem; line-height: 1.5; color: var(--texto);">
                ¿Está seguro de enviar un recordatorio por correo electrónico a <strong>${escape(act.responsable)}</strong> para esta actividad?
            </p>
            <div style="background: rgba(138, 0, 49, 0.04); border-left: 4px solid var(--guinda); padding: 12px; margin-bottom: 1.5rem; border-radius: 4px;">
                <div style="font-size: 0.75rem; text-transform: uppercase; color: var(--guinda); font-weight: 700; margin-bottom: 4px;">Actividad</div>
                <div style="font-weight: 700; color: var(--texto); font-size: 0.95rem;">${escape(act.actividad)}</div>
                <div style="font-size: 0.85rem; color: var(--texto-suave); margin-top: 4px;">
                    Fecha Compromiso: ${fmtDate(act.fechaCompromiso)}
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
            
            // Disable buttons and show "Enviando..."
            if (btnCancel) btnCancel.disabled = true;
            if (btnConfirm) {
                btnConfirm.disabled = true;
                btnConfirm.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Enviando...';
            }

            try {
                const token = document.querySelector('meta[name="csrf-token"]')?.content ?? '';
                const res = await fetch(`/Gestor/Api/Actividades/${act.id}/Recordatorio`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    }
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
