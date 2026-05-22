import { escape, parseDate, fmtDate, semaforo } from './utils.js';

export function renderGantt(temas, actividades) {
    const cont = document.getElementById('gantt-wrap');
    const acts = actividades.filter(a => a.fechaInicio && a.fechaCompromiso);
    if (!acts.length) { cont.innerHTML = '<p style="color:var(--g-text-soft)">Sin actividades con fechas</p>'; return; }

    const min = new Date(Math.min(...acts.map(a => parseDate(a.fechaInicio))));
    const max = new Date(Math.max(...acts.map(a => parseDate(a.fechaCompromiso))));
    const total = (max - min) / 86400000;

    // Generar marcas de mes
    const months = [];
    const cur = new Date(min.getFullYear(), min.getMonth(), 1);
    while (cur <= max) {
        months.push(new Date(cur));
        cur.setMonth(cur.getMonth() + 1);
    }
    const headTicks = months.map(m => {
        const left = ((m - min) / 86400000) / total * 100;
        return `<span style="position:absolute;left:${left}%;font-size:.7rem;color:var(--g-text-soft)">${m.toLocaleDateString('es-MX', { month: 'short', year: '2-digit' })}</span>`;
    }).join('');

    const rows = acts.map(a => {
        const start = parseDate(a.fechaInicio);
        const end = parseDate(a.fechaCompromiso);
        const left = ((start - min) / 86400000) / total * 100;
        const width = Math.max(0.5, ((end - start) / 86400000) / total * 100);
        const sem = semaforo(a);
        const cls = sem === 'rojo' ? 'r-riesgo' : sem === 'amarillo' ? 'r-proceso' : sem === 'verde' ? 'r-ok' : '';
        const tema = temas.find(t => t.id === a.temaId);
        return `
            <div class="gantt-row">
                <div title="${escape(tema?.tema || '')}">${escape(a.actividad)}</div>
                <div class="gantt-track">
                    <div class="gantt-bar ${cls}" style="left:${left}%;width:${width}%" title="${escape(a.actividad)} · ${fmtDate(a.fechaInicio)} → ${fmtDate(a.fechaCompromiso)}">
                        <span>${a.avance || 0}%</span>
                    </div>
                </div>
            </div>`;
    }).join('');

    cont.innerHTML = `
        <div class="gantt-row head">
            <div>Actividad</div>
            <div style="position:relative;height:18px">${headTicks}</div>
        </div>
        ${rows}`;
}
