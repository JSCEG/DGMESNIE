// Gráficos con Highcharts — cargado globalmente en _Layout.cshtml

const C = {
    guinda:    '#9b2247',
    verde:     '#1e5b4f',
    dorado:    '#a57f2c',
    ok:        '#027a48',
    proceso:   '#b54708',
    riesgo:    '#b42318',
    pendiente: '#667085'
};
const ESTATUS_COLOR   = { Pendiente: C.pendiente, 'En proceso': C.proceso, Vencida: C.riesgo, Concluida: C.ok };
const PRIORIDAD_COLOR = { Alta: C.riesgo, Media: C.proceso, Baja: C.ok };

const BASE_CHART = { style: { fontFamily: 'inherit' }, backgroundColor: 'transparent', animation: { duration: 600 } };

// ============ DONUT — Estatus actividades ============
export function donutEstatus(actividades) {
    const cont = document.getElementById('chart-donut-estatus');
    if (!cont) return;
    const counts = actividades.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {});
    const data   = Object.entries(counts).map(([name, y]) => ({ name, y, color: ESTATUS_COLOR[name] || C.guinda }));
    const total  = actividades.length;

    const hc = Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: { pie: { innerSize: '58%', dataLabels: { enabled: false }, showInLegend: true } },
        legend: { enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '12px' } },
        series: [{ name: 'Actividades', data }]
    });
    // Texto central con renderer
    const cx = hc.plotLeft + hc.plotSizeX / 2;
    const cy = hc.plotTop  + hc.plotSizeY / 2 + 10;
    hc.renderer.text(String(total), cx, cy - 8)
        .css({ fontSize: '2rem', fontWeight: '700', color: C.guinda, fontFamily: 'inherit' })
        .attr({ align: 'center', zIndex: 5 }).add();
    hc.renderer.text('ACTIVIDADES', cx, cy + 14)
        .css({ fontSize: '.7rem', fontWeight: '700', color: '#53617a', letterSpacing: '.08em' })
        .attr({ align: 'center', zIndex: 5 }).add();
}

// ============ GAUGE — Avance global ============
export function gaugeAvance(actividades) {
    const cont = document.getElementById('chart-gauge-avance');
    if (!cont) return;
    const avance = actividades.length
        ? Math.round(actividades.reduce((s, a) => s + (a.avance || 0), 0) / actividades.length) : 0;
    const color = avance >= 75 ? C.ok : avance >= 40 ? C.proceso : C.riesgo;

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'solidgauge', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        pane: {
            center: ['50%', '75%'], size: '120%', startAngle: -90, endAngle: 90,
            background: { backgroundColor: '#e8eaf0', innerRadius: '70%', outerRadius: '100%', shape: 'arc', borderWidth: 0 }
        },
        yAxis: {
            min: 0, max: 100, lineWidth: 0, tickWidth: 0, minorTickInterval: null,
            labels: { y: 18, style: { fontSize: '.7rem', color: '#53617a' } }
        },
        tooltip: { enabled: false },
        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 5, borderWidth: 0, useHTML: true,
                    format: `<div style="text-align:center">
                        <span style="font-size:2.5rem;font-weight:700;color:${color}">{y}%</span><br>
                        <span style="font-size:.7rem;letter-spacing:.08em;color:#53617a">AVANCE</span>
                    </div>`
                }
            }
        },
        series: [{ data: [{ y: avance, color }] }]
    });
}

// ============ PIE — Prioridad ============
export function piePrioridad(actividades) {
    const cont = document.getElementById('chart-pie-prioridad');
    if (!cont) return;
    const counts = actividades.reduce((m, a) => { m[a.prioridad || 'Baja'] = (m[a.prioridad || 'Baja'] || 0) + 1; return m; }, {});
    const data   = Object.entries(counts).map(([name, y]) => ({ name, y, color: PRIORIDAD_COLOR[name] || C.guinda }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: {
            pie: {
                dataLabels: { enabled: true, format: '<b>{point.name}</b>: {point.y}',
                    style: { fontWeight: '600', fontSize: '12px' } },
                showInLegend: false
            }
        },
        series: [{ name: 'Actividades', data }]
    });
}

// ============ BARRAS HORIZONTALES — Carga por responsable ============
export function barrasResponsables(actividades) {
    const cont = document.getElementById('chart-barras-responsables');
    if (!cont) return;
    const counts = actividades.filter(a => a.estatus !== 'Concluida')
        .reduce((m, a) => { if (a.responsable) m[a.responsable] = (m[a.responsable] || 0) + 1; return m; }, {});
    const sorted  = Object.entries(counts).sort((a, b) => b[1] - a[1]);
    const palette = [C.guinda, C.verde, C.dorado, C.proceso, C.pendiente, C.ok, C.riesgo];

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'bar', height: Math.max(160, sorted.length * 40 + 60) },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        xAxis: { categories: sorted.map(([k]) => k), labels: { style: { fontWeight: '600', fontSize: '12px' } } },
        yAxis: { title: { text: '' }, allowDecimals: false },
        tooltip: { valueSuffix: ' actividades pendientes' },
        legend: { enabled: false },
        series: [{
            name: 'Pendientes',
            data: sorted.map(([, v], i) => ({ y: v, color: palette[i % palette.length] })),
            dataLabels: { enabled: true, format: '{y}', style: { fontWeight: '700' } }
        }]
    });
}

// ============ BARRAS APILADAS — Por tema ============
export function barrasApiladasTemas(temas, actividades) {
    const cont = document.getElementById('chart-stacked-temas');
    if (!cont) return;
    const claves = ['Concluida', 'En proceso', 'Pendiente', 'Vencida'];
    const cats   = temas.map(t => t.tema.length > 30 ? t.tema.slice(0, 28) + '\u2026' : t.tema);
    const series = claves.map(k => ({
        name: k, color: ESTATUS_COLOR[k],
        data: temas.map(t => actividades.filter(a => a.temaId === t.id && a.estatus === k).length)
    }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'bar', height: Math.max(180, temas.length * 44 + 80) },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        xAxis: { categories: cats, labels: { style: { fontWeight: '600', fontSize: '11px' } } },
        yAxis: { title: { text: '' }, allowDecimals: false },
        plotOptions: { bar: { stacking: 'normal', dataLabels: { enabled: false } } },
        tooltip: { shared: false, valueSuffix: ' actividades' },
        legend: { enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '12px' } },
        series
    });
}

export function renderAllCharts(temas, actividades) {
    donutEstatus(actividades);
    gaugeAvance(actividades);
    piePrioridad(actividades);
    barrasResponsables(actividades);
    barrasApiladasTemas(temas, actividades);
}