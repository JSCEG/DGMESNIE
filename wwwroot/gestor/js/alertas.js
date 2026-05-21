import { escape, fmtDate, daysFromToday } from './utils.js';

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
    document.getElementById('badge-alertas').textContent = alertas.length;

    cont.innerHTML = alertas.length
        ? alertas.map(al => `
            <div class="alerta-item tipo-${al.tipo}">
                <div>
                    <h5>${escape(al.titulo)}</h5>
                    <p>${escape(al.msg)}</p>
                </div>
                <span class="chip">${al.tipo.replace('-', ' ')}</span>
            </div>`).join('')
        : '<p style="text-align:center;color:var(--g-text-soft);padding:2rem">Sin alertas activas</p>';
}
