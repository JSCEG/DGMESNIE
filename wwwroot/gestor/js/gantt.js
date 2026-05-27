import { escape, parseDate, fmtDate, semaforo } from './utils.js';

export function renderGantt(actividades, temas) {
    const cont = document.getElementById('gantt-wrap');
    if (!cont) return;
    const ts = temas.filter(t => t.fechaInicio && t.fechaCompromiso);
    if (!ts.length) {
        cont.innerHTML = '<p style="padding:1rem;color:#667085">Sin temas con fechas de inicio y compromiso</p>';
        return;
    }

    const C = { ok: '#027a48', proceso: '#b54708', riesgo: '#b42318', pendiente: '#667085', guinda: '#9b2247' };

    const data = ts.map(t => {
        const actividad = actividades.find(a => a.id === t.actividadId);
        const sem = semaforo(t);
        const color = sem === 'rojo' ? C.riesgo : sem === 'amarillo' ? C.proceso : sem === 'verde' ? C.ok : C.pendiente;
        return {
            name: t.tema,
            id: String(t.id),
            start: parseDate(t.fechaInicio)?.getTime(),
            end: parseDate(t.fechaCompromiso)?.getTime(),
            completed: { amount: (t.avance || 0) / 100 },
            color,
            custom: { tema: actividad?.actividad || '', responsable: t.responsable || '', estatus: t.estatus }
        };
    }).filter(d => d.start && d.end);

    // Destruir instancia previa si existe
    if (cont._hc) { try { cont._hc.destroy(); } catch(e){} }

    cont._hc = Highcharts.ganttChart(cont, {
        chart: {
            style: { fontFamily: 'inherit' },
            backgroundColor: 'transparent',
            animation: { duration: 600 }
        },
        credits: { enabled: false },
        title: { text: '' },
        exporting: { enabled: false },
        navigator: { enabled: true, liveRedraw: true, series: { type: 'gantt' } },
        scrollbar: { enabled: true },
        rangeSelector: {
            enabled: true,
            selected: 0,
            buttons: [
                { type: 'month', count: 1, text: '1M' },
                { type: 'month', count: 3, text: '3M' },
                { type: 'month', count: 6, text: '6M' },
                { type: 'all', text: 'Todo' }
            ]
        },
        xAxis: [{
            currentDateIndicator: { color: '#9b2247', label: { format: 'Hoy' } }
        }],
        yAxis: { labels: { style: { fontWeight: '600', fontSize: '11px', maxWidth: '220px' } } },
        tooltip: {
            useHTML: true,
            formatter: function() {
                const p = this.point;
                const ini = Highcharts.dateFormat('%d/%m/%Y', p.start);
                const fin = Highcharts.dateFormat('%d/%m/%Y', p.end);
                return `<b>${escape(p.name)}</b><br>
                    <small>${escape(p.custom?.tema || '')}</small><br>
                    📅 ${ini} → ${fin}<br>
                    👤 ${escape(p.custom?.responsable || '')}<br>
                    📊 Avance: <b>${Math.round((p.completed?.amount || 0) * 100)}%</b><br>
                    Estado: <b>${escape(p.custom?.estatus || '')}</b>`;
            }
        },
        plotOptions: {
            gantt: {
                dataLabels: {
                    enabled: true,
                    format: '{point.custom.estatus}',
                    style: { fontWeight: '600', fontSize: '10px', textOutline: 'none' }
                },
                borderRadius: 4
            }
        },
        series: [{ name: 'Temas', data }]
    });
}