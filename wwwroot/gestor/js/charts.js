// Gráficos con Highcharts — cargado globalmente en _Layout.cshtml

const C = {
    guinda: '#8a0031',
    verde: '#1e5b4f',
    dorado: '#245b8f',
    ok: '#027a48',
    proceso: '#8a0031',
    riesgo: '#a14d6a',
    pendiente: '#667085',
    texto: '#243444',
    textoSuave: '#6c7a89',
    linea: '#d9e0e7',
    grid: '#edf0f3',
    fondo: '#ffffff'
};
const ESTATUS_COLOR = { Pendiente: C.pendiente, 'En proceso': C.proceso, Vencida: C.riesgo, Concluida: C.ok };
const PRIORIDAD_COLOR = { Alta: C.riesgo, Media: C.proceso, Baja: C.ok };

const BASE_CHART = {
    style: { fontFamily: 'Montserrat, sans-serif' },
    backgroundColor: C.fondo,
    animation: { duration: 600 },
    spacingTop: 12,
    spacingRight: 12,
    spacingBottom: 12,
    spacingLeft: 12
};

let themeApplied = false;
let fullscreenWired = false;

function activeFullscreenElement() {
    return document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement || null;
}

function applyInstitutionalTheme() {
    if (themeApplied || !window.Highcharts) return;
    Highcharts.setOptions({
        chart: {
            backgroundColor: C.fondo,
            style: {
                fontFamily: 'Montserrat, sans-serif'
            }
        },
        title: {
            style: {
                color: C.texto,
                fontWeight: '700'
            }
        },
        xAxis: {
            lineColor: C.linea,
            tickColor: C.linea,
            labels: {
                style: {
                    color: C.texto,
                    fontWeight: '600'
                }
            }
        },
        yAxis: {
            lineColor: C.linea,
            tickColor: C.linea,
            gridLineColor: C.grid,
            labels: {
                style: {
                    color: C.texto
                }
            },
            title: {
                style: {
                    color: C.textoSuave
                }
            }
        },
        legend: {
            itemStyle: {
                color: C.texto,
                fontWeight: '600'
            },
            itemHoverStyle: {
                color: C.guinda
            }
        },
        tooltip: {
            backgroundColor: '#ffffff',
            borderColor: C.linea,
            style: {
                color: C.texto
            }
        },
        credits: {
            enabled: false
        }
    });
    themeApplied = true;
}

// ============ DONUT — Estatus actividades ============
export function donutEstatus(actividades) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-donut-estatus');
    if (!cont) return;
    const counts = actividades.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {});
    const data = Object.entries(counts).map(([name, y]) => ({ name, y, color: ESTATUS_COLOR[name] || C.guinda }));
    const total = actividades.length;

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false },
        title: {
            text: `<div style="text-align:center;margin-top:14px;">
                <span style="font-size:2.2rem;font-weight:800;color:${C.guinda};font-family:Montserrat,sans-serif;line-height:1;">${total}</span><br>
                <span style="font-size:.65rem;font-weight:700;color:${C.textoSuave};letter-spacing:.08em;line-height:1.2;">ACTIVIDADES</span>
            </div>`,
            align: 'center',
            verticalAlign: 'middle',
            y: -24, // Offset slightly upward to account for bottom legend pushing the plot area up
            useHTML: true
        },
        exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: { pie: { innerSize: '62%', dataLabels: { enabled: false }, showInLegend: true } },
        legend: {
            enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '11px' }
        },
        series: [{ name: 'Actividades', data }]
    });
}

// ============ GAUGE — Avance global ============
export function gaugeAvance(actividades) {
    applyInstitutionalTheme();
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
            labels: { y: 18, style: { fontSize: '.7rem', color: C.textoSuave } }
        },
        tooltip: { enabled: false },
        plotOptions: {
            solidgauge: {
                animation: {
                    duration: 1200
                },
                dataLabels: {
                    y: 5, borderWidth: 0, useHTML: true,
                    format: `<div style="text-align:center">
                        <span style="font-size:2.5rem;font-weight:700;color:${color}">{y}%</span><br>
                        <span style="font-size:.7rem;letter-spacing:.08em;color:${C.textoSuave}">AVANCE</span>
                    </div>`
                }
            }
        },
        series: [{
            name: 'Avance',
            data: [{ y: avance, color }],
            animation: {
                duration: 1200,
                defer: 150
            }
        }]
    });
}

// ============ PIE — Prioridad ============
export function piePrioridad(actividades) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-pie-prioridad');
    if (!cont) return;
    const counts = actividades.reduce((m, a) => { m[a.prioridad || 'Baja'] = (m[a.prioridad || 'Baja'] || 0) + 1; return m; }, {});
    const data = Object.entries(counts).map(([name, y]) => ({ name, y, color: PRIORIDAD_COLOR[name] || C.guinda }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: {
            pie: {
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        legend: {
            enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '11px' }
        },
        series: [{ name: 'Actividades', data }]
    });
}

// ============ BARRAS HORIZONTALES — Carga por responsable ============
export function barrasResponsables(actividades) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-barras-responsables');
    if (!cont) return;
    const counts = actividades.filter(a => a.estatus !== 'Concluida')
        .reduce((m, a) => { if (a.responsable) m[a.responsable] = (m[a.responsable] || 0) + 1; return m; }, {});
    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]);
    const palette = [C.guinda, C.verde, C.dorado, C.proceso, C.pendiente, C.ok, C.riesgo];
    const isMobile = window.innerWidth < 600;

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'bar', height: Math.max(160, sorted.length * 40 + 60) },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        xAxis: {
            categories: sorted.map(([k]) => {
                const maxLen = isMobile ? 12 : 25;
                return k.length > maxLen ? k.slice(0, maxLen - 2) + '…' : k;
            }),
            labels: { style: { fontWeight: '600', fontSize: isMobile ? '10px' : '12px' } }
        },
        yAxis: { title: { text: '' }, allowDecimals: false },
        tooltip: { valueSuffix: ' actividades pendientes' },
        legend: { enabled: false },
        series: [{
            name: 'Pendientes',
            data: sorted.map(([, v], i) => ({ y: v, color: palette[i % palette.length] })),
            borderRadius: 8,
            dataLabels: { enabled: true, format: '{y}', style: { fontWeight: '700', color: C.texto } }
        }]
    });
}

// ============ BARRAS APILADAS — Por tema ============
export function barrasApiladasTemas(temas, actividades) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-stacked-temas');
    if (!cont) return;
    const claves = ['Concluida', 'En proceso', 'Pendiente', 'Vencida'];
    const isMobile = window.innerWidth < 600;
    const cats = temas.map(t => {
        const maxLen = isMobile ? 15 : 30;
        return t.tema.length > maxLen ? t.tema.slice(0, maxLen - 2) + '…' : t.tema;
    });
    const series = claves.map(k => ({
        name: k, color: ESTATUS_COLOR[k],
        data: temas.map(t => actividades.filter(a => a.temaId === t.id && a.estatus === k).length)
    }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'bar', height: Math.max(180, temas.length * 44 + 80) },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        xAxis: {
            categories: cats,
            labels: { style: { fontWeight: '600', fontSize: isMobile ? '9px' : '11px' } }
        },
        yAxis: { title: { text: '' }, allowDecimals: false },
        plotOptions: { bar: { stacking: 'normal', borderRadius: 8, dataLabels: { enabled: false } } },
        tooltip: { shared: false, valueSuffix: ' actividades' },
        legend: {
            enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '12px' }
        },
        series
    });
}

function getFullscreenPanel(button) {
    const chartId = button?.dataset?.chartFullscreen;
    if (!chartId) {
        return null;
    }

    const chartBox = document.getElementById(chartId);
    return chartBox ? chartBox.closest('.dashboard-panel') : null;
}

function syncFullscreenButtons() {
    const buttons = document.querySelectorAll('[data-chart-fullscreen]');
    buttons.forEach((button) => {
        const panel = getFullscreenPanel(button);
        const isFull = !!panel && activeFullscreenElement() === panel;

        button.classList.toggle('is-active', isFull);
        button.textContent = isFull ? 'Salir de vista completa' : 'Vista completa';
        if (panel) {
            panel.classList.toggle('is-fullscreen', isFull);
        }
    });
}

export function wireChartFullscreenButtons() {
    if (fullscreenWired || typeof document === 'undefined') {
        return;
    }

    const buttons = Array.from(document.querySelectorAll('[data-chart-fullscreen]'));
    if (!buttons.length) {
        return;
    }

    const toggleFullscreen = async (panel) => {
        if (!panel) {
            return;
        }

        if (activeFullscreenElement() === panel) {
            if (document.exitFullscreen) {
                await document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
        } else if (panel.requestFullscreen) {
            await panel.requestFullscreen();
        } else if (panel.webkitRequestFullscreen) {
            panel.webkitRequestFullscreen();
        } else if (panel.msRequestFullscreen) {
            panel.msRequestFullscreen();
        }

        window.requestAnimationFrame(() => window.dispatchEvent(new Event('resize')));
    };

    buttons.forEach((button) => {
        button.addEventListener('click', () => toggleFullscreen(getFullscreenPanel(button)));
    });

    const handleFullscreenChange = () => {
        const fullscreenElement = activeFullscreenElement();
        syncFullscreenButtons();

        window.setTimeout(() => {
            Highcharts.charts.forEach((chart) => {
                if (!chart) {
                    return;
                }

                if (!fullscreenElement && chart?.renderTo) {
                    chart.renderTo.style.height = '';
                    chart.setSize(null, null, false);
                }

                chart.reflow();
            });
        }, 120);
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    document.addEventListener('webkitfullscreenchange', handleFullscreenChange);
    document.addEventListener('msfullscreenchange', handleFullscreenChange);

    syncFullscreenButtons();
    fullscreenWired = true;
}

export function treemapTemas(temas, actividades) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-treemap-temas');
    if (!cont) return;

    const activeTemaIds = new Set(temas.map(t => t.id));
    const filteredActs = actividades.filter(a => activeTemaIds.has(a.temaId));

    const data = [];

    temas.forEach(t => {
        const subActs = filteredActs.filter(a => a.temaId === t.id);
        data.push({
            id: `t_${t.id}`,
            name: t.tema,
            color: Highcharts.color(C.guinda).setOpacity(0.08).get(),
            value: subActs.length || 1
        });
    });

    filteredActs.forEach(a => {
        data.push({
            id: `a_${a.id}`,
            name: a.actividad,
            parent: `t_${a.temaId}`,
            value: 1,
            colorValue: a.avance || 0,
            responsable: a.responsable || 'Sin responsable',
            estatus: a.estatus || 'Pendiente'
        });
    });

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'treemap', height: 400 },
        credits: { enabled: false },
        title: { text: '' },
        exporting: { enabled: false },
        colorAxis: {
            min: 0,
            max: 100,
            minColor: '#fdecec',
            maxColor: '#dff7ea',
            stops: [
                [0, '#fbe3e1'],
                [0.35, '#f8ecd4'],
                [0.7, '#eaf6e1'],
                [1, '#d6f0df']
            ]
        },
        tooltip: {
            useHTML: true,
            pointFormat: `
                <div style="padding: 6px; font-family: Montserrat, sans-serif;">
                    <b>{point.name}</b><br/>
                    {if point.parent}
                        Responsable: <b>{point.responsable}</b><br/>
                        Progreso: <b>{point.colorValue}%</b><br/>
                        Estatus: <b>{point.estatus}</b>
                    {else}
                        Actividades: <b>{point.value}</b>
                    {/if}
                </div>
            `
        },
        series: [{
            layoutAlgorithm: 'squarified',
            allowDrillToNode: true,
            animationLimit: 120,
            dataLabels: {
                enabled: true,
                align: 'left',
                verticalAlign: 'top',
                style: {
                    fontSize: '11px',
                    fontWeight: '600',
                    textOutline: 'none',
                    color: C.texto
                }
            },
            levelIsConstant: false,
            levels: [{
                level: 1,
                dataLabels: {
                    enabled: true,
                    style: {
                        fontSize: '13px',
                        fontWeight: 'bold',
                        color: '#6f1233'
                    }
                },
                borderWidth: 2,
                borderColor: C.guinda
            }, {
                level: 2,
                dataLabels: {
                    enabled: true,
                    style: {
                        fontSize: '10px',
                        fontWeight: '600',
                        color: '#22313f'
                    }
                },
                borderWidth: 1,
                borderColor: '#ffffff'
            }],
            data
        }]
    });
}

export function renderAllCharts(temas, actividades) {
    donutEstatus(actividades);
    gaugeAvance(actividades);
    piePrioridad(actividades);
    barrasResponsables(actividades);
    barrasApiladasTemas(temas, actividades);
    treemapTemas(temas, actividades);
}