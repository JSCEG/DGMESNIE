import { escape, semaforoTema, avancePromedio, daysFromToday, fmtDate } from './utils.js';
import { renderAllCharts } from './charts.js';

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

export function renderDashboard(temas, actividades) {
    const vencidas = actividades.filter(a => a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0);
    const porVencer = actividades.filter(a => a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) >= 0 && daysFromToday(a.fechaCompromiso) <= 7);
    const concluidas = actividades.filter(a => a.estatus === 'Concluida');
    const avance = actividades.length
        ? Math.round(actividades.reduce((s, a) => s + (a.avance || 0), 0) / actividades.length)
        : 0;

    document.getElementById('kpi-temas').textContent = temas.filter(t => t.estatus === 'Activo').length;
    document.getElementById('kpi-actividades').textContent = actividades.length;
    document.getElementById('kpi-vencidas').textContent = vencidas.length;
    document.getElementById('kpi-por-vencer').textContent = porVencer.length;
    document.getElementById('kpi-concluidas').textContent = concluidas.length;
    document.getElementById('kpi-avance').textContent = avance + '%';

    // Gráficos D3
    renderAllCharts(temas, actividades);

    // Temas críticos
    const criticos = temas
        .map(t => ({ t, sem: semaforoTema(t, actividades), avance: avancePromedio(t, actividades) }))
        .filter(x => x.sem === 'rojo' || x.sem === 'amarillo')
        .sort((a, b) => (a.sem === 'rojo' ? 0 : 1) - (b.sem === 'rojo' ? 0 : 1));

    const tbody = document.querySelector('#tbl-criticos tbody');
    tbody.innerHTML = criticos.length
        ? criticos.map(({ t, sem, avance }) => {
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : 'track--complete';
            const pillTxt = sem === 'rojo' ? 'Vencido' : sem === 'amarillo' ? 'Por vencer' : 'En curso';
            return `
                <tr>
                    <td><strong>${escape(t.tema)}</strong><br><small class="muted">${escape(t.categoria || '')}</small></td>
                    <td>${escape(t.responsablePrincipal)}</td>
                    <td><div class="track ${trackCls}" style="width:120px"><span style="width:${avance}%"></span></div><small class="muted">${avance}%</small></td>
                    <td>${fmtDate(t.fechaCompromiso)}</td>
                    <td><span class="status-pill ${pillClass(sem)}">${pillTxt}</span></td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="5" class="muted" style="text-align:center;padding:1rem">Sin temas críticos</td></tr>';
}
