import { escape, semaforoTema, avancePromedio, daysFromToday, fmtDate } from './utils.js';
import { renderAllCharts } from './charts.js?v=charts-v3';

function trackClass(estatus) {
    if (estatus === 'Vencida') return 'track--issue';
    if (estatus === 'En proceso') return 'track--progress';
    if (estatus === 'Concluida') return 'track--complete';
    return 'track--pending';
}

function pillClass(sem) {
    if (sem === 'rojo') return 'status-pill--issue';
    if (sem === 'amarillo') return 'status-pill--progress';
    if (sem === 'verde') return 'status-pill--complete';
    return '';
}

export function renderDashboard(actividades, temas) {
    const vencidos = temas.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0);
    const porVencer = temas.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) >= 0 && daysFromToday(t.fechaCompromiso) <= 7);
    const concluidos = temas.filter(t => t.estatus === 'Concluida');
    const avance = temas.length
        ? Math.round(temas.reduce((s, t) => s + (t.avance || 0), 0) / temas.length)
        : 0;

    document.getElementById('kpi-temas').textContent = actividades.filter(a => a.estatus === 'Activo').length;
    document.getElementById('kpi-actividades').textContent = temas.length;
    document.getElementById('kpi-vencidas').textContent = vencidos.length;
    document.getElementById('kpi-por-vencer').textContent = porVencer.length;
    document.getElementById('kpi-concluidas').textContent = concluidos.length;
    document.getElementById('kpi-avance').textContent = avance + '%';

    // Gráficos D3
    renderAllCharts(actividades, temas);

    // Actividades críticas (antes Temas críticos)
    const criticos = actividades
        .map(a => ({ a, sem: semaforoTema(a, temas), avance: avancePromedio(a, temas) }))
        .filter(x => x.sem === 'rojo' || x.sem === 'amarillo')
        .sort((a, b) => (a.sem === 'rojo' ? 0 : 1) - (b.sem === 'rojo' ? 0 : 1));

    const tbody = document.querySelector('#tbl-criticos tbody');
    tbody.innerHTML = criticos.length
        ? criticos.map(({ a, sem, avance }) => {
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : 'track--complete';
            const pillTxt = sem === 'rojo' ? 'Vencido' : sem === 'amarillo' ? 'Por vencer' : 'En curso';
            return `
                <tr>
                    <td><strong>${escape(a.actividad)}</strong><br><small class="muted">${escape(a.categoria || '')}</small></td>
                    <td>${escape(a.responsablePrincipal)}</td>
                    <td><div class="track ${trackCls}" style="width:120px"><span style="width:${avance}%"></span></div><small class="muted">${avance}%</small></td>
                    <td>${fmtDate(a.fechaCompromiso)}</td>
                    <td><span class="status-pill ${pillClass(sem)}">${pillTxt}</span></td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="5" class="muted" style="text-align:center;padding:1rem">Sin actividades críticas</td></tr>';
}
