// Gráficos D3.js v7 — institucional SENER.
// D3 se carga vía script tag global en index.html (window.d3).

const COLORS = {
    guinda: '#9b2247',
    verde: '#1e5b4f',
    dorado: '#a57f2c',
    ok: '#027a48',
    proceso: '#b54708',
    riesgo: '#b42318',
    pendiente: '#667085',
    textoSuave: '#53617a',
    borde: 'rgba(15,23,42,.08)'
};

const ESTATUS_COLOR = {
    'Pendiente': COLORS.pendiente,
    'En proceso': COLORS.proceso,
    'Vencida': COLORS.riesgo,
    'Concluida': COLORS.ok
};

const PRIORIDAD_COLOR = {
    'Alta': COLORS.riesgo,
    'Media': COLORS.proceso,
    'Baja': COLORS.ok
};

function clear(id) {
    const el = document.getElementById(id);
    if (el) el.innerHTML = '';
    return el;
}

// ============ DONUT — Estatus actividades ============
export function donutEstatus(actividades) {
    const cont = clear('chart-donut-estatus');
    if (!cont) return;
    const data = Object.entries(
        actividades.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {})
    ).map(([k, v]) => ({ label: k, value: v }));

    const W = cont.clientWidth, H = 240, R = Math.min(W, H) / 2 - 12;
    const svg = d3.select(cont).append('svg').attr('viewBox', `0 0 ${W} ${H}`).attr('width', '100%').attr('height', H);
    const g = svg.append('g').attr('transform', `translate(${W/2},${H/2})`);

    const pie = d3.pie().value(d => d.value).sort(null);
    const arc = d3.arc().innerRadius(R * 0.58).outerRadius(R);
    const arcs = pie(data);
    const total = d3.sum(data, d => d.value);

    g.selectAll('path').data(arcs).enter().append('path')
        .attr('d', arc)
        .attr('fill', d => ESTATUS_COLOR[d.data.label] || COLORS.guinda)
        .attr('stroke', '#fff').attr('stroke-width', 2)
        .style('cursor', 'pointer')
        .on('mouseenter', function() { d3.select(this).transition().duration(150).attr('transform', 'scale(1.04)'); })
        .on('mouseleave', function() { d3.select(this).transition().duration(150).attr('transform', 'scale(1)'); })
        .append('title').text(d => `${d.data.label}: ${d.data.value} (${((d.data.value/total)*100).toFixed(0)}%)`);

    // Texto central
    g.append('text').attr('text-anchor', 'middle').attr('dy', '-.1em')
        .style('font-family', 'Patria, Georgia, serif')
        .style('font-size', '2rem').style('font-weight', '700')
        .style('fill', COLORS.guinda).text(total);
    g.append('text').attr('text-anchor', 'middle').attr('dy', '1.2em')
        .style('font-size', '.7rem').style('font-weight', '700')
        .style('fill', COLORS.textoSuave).style('letter-spacing', '.08em').text('ACTIVIDADES');

    // Leyenda
    const leg = d3.select(cont).append('div').attr('class', 'chart-legend');
    data.forEach(d => {
        leg.append('span').html(`<i style="background:${ESTATUS_COLOR[d.label] || COLORS.guinda}"></i>${d.label} <strong>${d.value}</strong>`);
    });
}

// ============ GAUGE — Avance global ============
export function gaugeAvance(actividades) {
    const cont = clear('chart-gauge-avance');
    if (!cont) return;
    const avance = actividades.length
        ? Math.round(actividades.reduce((s, a) => s + (a.avance || 0), 0) / actividades.length) : 0;

    const W = cont.clientWidth, H = 240, R = Math.min(W, H * 1.6) / 2 - 16;
    const svg = d3.select(cont).append('svg').attr('viewBox', `0 0 ${W} ${H}`).attr('width', '100%').attr('height', H);
    const g = svg.append('g').attr('transform', `translate(${W/2},${H * 0.72})`);

    // Arco fondo
    const arcBg = d3.arc().innerRadius(R * 0.7).outerRadius(R).startAngle(-Math.PI/2).endAngle(Math.PI/2);
    g.append('path').attr('d', arcBg).attr('fill', COLORS.borde);

    // Arco color por nivel
    const color = avance >= 75 ? COLORS.ok : avance >= 40 ? COLORS.proceso : COLORS.riesgo;
    const arcVal = d3.arc().innerRadius(R * 0.7).outerRadius(R)
        .startAngle(-Math.PI/2).endAngle(-Math.PI/2 + (avance/100) * Math.PI);
    g.append('path').attr('d', arcVal).attr('fill', color);

    // Texto
    g.append('text').attr('text-anchor', 'middle').attr('dy', '-.5em')
        .style('font-family', 'Patria, Georgia, serif')
        .style('font-size', '3rem').style('font-weight', '700').style('fill', color)
        .text(avance + '%');
    g.append('text').attr('text-anchor', 'middle').attr('dy', '1em')
        .style('font-size', '.7rem').style('font-weight', '700')
        .style('letter-spacing', '.08em').style('fill', COLORS.textoSuave)
        .text('AVANCE PROMEDIO');

    // Ticks 0 / 50 / 100
    [0, 50, 100].forEach(v => {
        const a = -Math.PI/2 + (v/100) * Math.PI;
        const x = Math.cos(a) * (R + 8);
        const y = Math.sin(a) * (R + 8);
        g.append('text').attr('x', x).attr('y', y)
            .attr('text-anchor', 'middle').attr('dy', '.3em')
            .style('font-size', '.7rem').style('fill', COLORS.textoSuave).text(v);
    });
}

// ============ PIE — Prioridad ============
export function piePrioridad(actividades) {
    const cont = clear('chart-pie-prioridad');
    if (!cont) return;
    const data = Object.entries(
        actividades.reduce((m, a) => { m[a.prioridad || 'Baja'] = (m[a.prioridad || 'Baja'] || 0) + 1; return m; }, {})
    ).map(([k, v]) => ({ label: k, value: v }));

    const W = cont.clientWidth, H = 240, R = Math.min(W, H) / 2 - 12;
    const svg = d3.select(cont).append('svg').attr('viewBox', `0 0 ${W} ${H}`).attr('width', '100%').attr('height', H);
    const g = svg.append('g').attr('transform', `translate(${W/2},${H/2})`);

    const pie = d3.pie().value(d => d.value).sort(null);
    const arc = d3.arc().innerRadius(0).outerRadius(R);
    const total = d3.sum(data, d => d.value);

    g.selectAll('path').data(pie(data)).enter().append('path')
        .attr('d', arc)
        .attr('fill', d => PRIORIDAD_COLOR[d.data.label] || COLORS.guinda)
        .attr('stroke', '#fff').attr('stroke-width', 2)
        .append('title').text(d => `${d.data.label}: ${d.data.value} (${((d.data.value/total)*100).toFixed(0)}%)`);

    g.selectAll('text').data(pie(data)).enter().append('text')
        .attr('transform', d => `translate(${arc.centroid(d)})`)
        .attr('text-anchor', 'middle').attr('dy', '.3em')
        .style('fill', '#fff').style('font-size', '.85rem').style('font-weight', '700')
        .text(d => d.data.value);

    const leg = d3.select(cont).append('div').attr('class', 'chart-legend');
    data.forEach(d => {
        leg.append('span').html(`<i style="background:${PRIORIDAD_COLOR[d.label] || COLORS.guinda}"></i>${d.label} <strong>${d.value}</strong>`);
    });
}

// ============ BARRAS HORIZONTALES — Carga por responsable ============
export function barrasResponsables(actividades) {
    const cont = clear('chart-barras-responsables');
    if (!cont) return;
    const data = Object.entries(
        actividades.filter(a => a.estatus !== 'Concluida')
            .reduce((m, a) => { m[a.responsable] = (m[a.responsable] || 0) + 1; return m; }, {})
    ).map(([k, v]) => ({ label: k, value: v })).sort((a, b) => b.value - a.value);

    const margin = { top: 8, right: 30, bottom: 8, left: 130 };
    const W = cont.clientWidth, rowH = 28, H = Math.max(120, data.length * rowH + margin.top + margin.bottom);
    const innerW = W - margin.left - margin.right;

    const svg = d3.select(cont).append('svg').attr('viewBox', `0 0 ${W} ${H}`).attr('width', '100%').attr('height', H);
    const g = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`);

    const x = d3.scaleLinear().domain([0, d3.max(data, d => d.value) || 1]).range([0, innerW]);
    const y = d3.scaleBand().domain(data.map(d => d.label)).range([0, H - margin.top - margin.bottom]).padding(0.25);

    g.selectAll('rect.bg').data(data).enter().append('rect').attr('class', 'bg')
        .attr('x', 0).attr('y', d => y(d.label))
        .attr('width', innerW).attr('height', y.bandwidth())
        .attr('rx', 4).attr('fill', COLORS.borde);

    const palette = [COLORS.guinda, COLORS.verde, COLORS.dorado, COLORS.proceso, COLORS.pendiente, COLORS.ok, COLORS.riesgo];
    g.selectAll('rect.val').data(data).enter().append('rect').attr('class', 'val')
        .attr('x', 0).attr('y', d => y(d.label))
        .attr('width', 0).attr('height', y.bandwidth())
        .attr('rx', 4).attr('fill', (_, i) => palette[i % palette.length])
        .transition().duration(700).attr('width', d => x(d.value));

    g.selectAll('text.label').data(data).enter().append('text').attr('class', 'label')
        .attr('x', -10).attr('y', d => y(d.label) + y.bandwidth() / 2)
        .attr('text-anchor', 'end').attr('dy', '.35em')
        .style('font-size', '.8rem').style('font-weight', '700').style('fill', '#1f2937')
        .text(d => d.label);

    g.selectAll('text.val').data(data).enter().append('text').attr('class', 'val')
        .attr('x', d => x(d.value) + 6).attr('y', d => y(d.label) + y.bandwidth() / 2)
        .attr('dy', '.35em')
        .style('font-size', '.8rem').style('font-weight', '700').style('fill', COLORS.textoSuave)
        .text(d => d.value);
}

// ============ BARRAS APILADAS — Por tema ============
export function barrasApiladasTemas(temas, actividades) {
    const cont = clear('chart-stacked-temas');
    if (!cont) return;
    const claves = ['Concluida', 'En proceso', 'Pendiente', 'Vencida'];
    const data = temas.map(t => {
        const row = { tema: t.tema };
        claves.forEach(k => row[k] = 0);
        actividades.filter(a => a.temaId === t.id).forEach(a => {
            if (claves.includes(a.estatus)) row[a.estatus]++;
        });
        return row;
    });

    const margin = { top: 16, right: 16, bottom: 30, left: 200 };
    const W = cont.clientWidth, rowH = 32, H = Math.max(160, data.length * rowH + margin.top + margin.bottom);
    const innerW = W - margin.left - margin.right;
    const innerH = H - margin.top - margin.bottom;

    const svg = d3.select(cont).append('svg').attr('viewBox', `0 0 ${W} ${H}`).attr('width', '100%').attr('height', H);
    const g = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`);

    const stack = d3.stack().keys(claves)(data);
    const max = d3.max(stack[stack.length - 1], d => d[1]) || 1;
    const x = d3.scaleLinear().domain([0, max]).range([0, innerW]);
    const y = d3.scaleBand().domain(data.map(d => d.tema)).range([0, innerH]).padding(0.2);

    const layer = g.selectAll('g.layer').data(stack).enter().append('g').attr('class', 'layer')
        .attr('fill', d => ESTATUS_COLOR[d.key] || COLORS.guinda);

    layer.selectAll('rect').data(d => d).enter().append('rect')
        .attr('y', d => y(d.data.tema))
        .attr('x', d => x(d[0]))
        .attr('width', d => x(d[1]) - x(d[0]))
        .attr('height', y.bandwidth())
        .append('title').text(function() { return d3.select(this.parentNode.parentNode).datum().key + ': ' + (d3.select(this.parentNode).datum()[1] - d3.select(this.parentNode).datum()[0]); });

    // Labels Y (temas)
    g.selectAll('text.tema-label').data(data).enter().append('text').attr('class', 'tema-label')
        .attr('x', -10).attr('y', d => y(d.tema) + y.bandwidth() / 2)
        .attr('text-anchor', 'end').attr('dy', '.35em')
        .style('font-size', '.75rem').style('font-weight', '600').style('fill', '#1f2937')
        .text(d => d.tema.length > 32 ? d.tema.slice(0, 30) + '…' : d.tema);

    // Eje X
    const ax = d3.axisBottom(x).ticks(Math.min(max, 6)).tickFormat(d3.format('d'));
    g.append('g').attr('transform', `translate(0,${innerH})`).call(ax)
        .selectAll('text').style('font-size', '.7rem').style('fill', COLORS.textoSuave);

    // Leyenda
    const leg = d3.select(cont).append('div').attr('class', 'chart-legend');
    claves.forEach(k => {
        leg.append('span').html(`<i style="background:${ESTATUS_COLOR[k]}"></i>${k}`);
    });
}

export function renderAllCharts(temas, actividades) {
    donutEstatus(actividades);
    gaugeAvance(actividades);
    piePrioridad(actividades);
    barrasResponsables(actividades);
    barrasApiladasTemas(temas, actividades);
}
