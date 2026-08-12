// Gráficos con Highcharts — cargado globalmente en _Layout.cshtml
// Paleta dual: sigue a data-bs-theme (claro/oscuro). El fondo es transparente
// para que mande la superficie de la tarjeta (.chart-box) vía CSS.

const PALETA_CLARA = {
    guinda: '#9B2247',
    verde: '#1e5b4f',
    dorado: '#245b8f',
    ok: '#0E7C5A',
    proceso: '#9B2247',
    riesgo: '#a14d6a',
    pendiente: '#6F6B66',
    texto: '#243444',
    textoSuave: '#6c7a89',
    linea: '#d9e0e7',
    grid: '#edf0f3',
    fondo: 'transparent',
    tooltipFondo: '#ffffff',
    gaugeTrack: '#e8eaf0',
    borde: '#ffffff',
    treemapStops: [[0, '#fbe3e1'], [0.35, '#f8ecd4'], [0.7, '#eaf6e1'], [1, '#d6f0df']],
    treemapMin: '#fdecec',
    treemapMax: '#dff7ea',
    treemapL1: '#6f1233',
    treemapL2: '#22313f'
};

const PALETA_OSCURA = {
    guinda: '#d4537e',
    verde: '#4ccfae',
    dorado: '#d4ab52',
    ok: '#4ccfae',
    proceso: '#d4537e',
    riesgo: '#e08098',
    pendiente: '#a39aa8',
    texto: '#ece7e2',
    textoSuave: '#a39aa8',
    linea: 'rgba(255, 255, 255, 0.16)',
    grid: 'rgba(255, 255, 255, 0.08)',
    fondo: 'transparent',
    tooltipFondo: '#241f2e',
    gaugeTrack: 'rgba(255, 255, 255, 0.10)',
    borde: '#1d1925',
    treemapStops: [[0, '#54222f'], [0.35, '#544527'], [0.7, '#2c4a3b'], [1, '#1f5244']],
    treemapMin: '#54222f',
    treemapMax: '#1f5244',
    treemapL1: '#f2d9a6',
    treemapL2: '#ece7e2'
};

function isDarkTheme() {
    return document.documentElement.getAttribute('data-bs-theme') === 'dark';
}

const C = { ...PALETA_CLARA };
const ESTATUS_COLOR = {};
const PRIORIDAD_COLOR = {};

const BASE_CHART = {
    style: { fontFamily: 'Montserrat, sans-serif' },
    backgroundColor: 'transparent',
    animation: { duration: 600 },
    spacingTop: 12,
    spacingRight: 12,
    spacingBottom: 12,
    spacingLeft: 12
};

function syncPalette() {
    Object.assign(C, isDarkTheme() ? PALETA_OSCURA : PALETA_CLARA);
    Object.assign(ESTATUS_COLOR, { Pendiente: C.pendiente, 'En proceso': C.proceso, Vencida: C.riesgo, Concluida: C.ok });
    Object.assign(PRIORIDAD_COLOR, { Alta: C.riesgo, Media: C.proceso, Baja: C.ok });
    BASE_CHART.backgroundColor = C.fondo;
}
syncPalette();

let appliedTheme = null;
let fullscreenWired = false;

function activeFullscreenElement() {
    return document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement || null;
}

function applyInstitutionalTheme() {
    if (!window.Highcharts) return;
    const mode = isDarkTheme() ? 'dark' : 'light';
    if (appliedTheme === mode) return;
    syncPalette();
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
            backgroundColor: C.tooltipFondo,
            borderColor: C.linea,
            style: {
                color: C.texto
            }
        },
        credits: {
            enabled: false
        }
    });
    appliedTheme = mode;
}

// ============ DONUT — Estatus actividades ============
export function donutEstatus(temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-donut-estatus');
    if (!cont) return;
    const counts = temas.reduce((m, t) => { m[t.estatus] = (m[t.estatus] || 0) + 1; return m; }, {});
    const data = Object.entries(counts).map(([name, y]) => ({ name, y, color: ESTATUS_COLOR[name] || C.guinda }));
    const total = temas.length;

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false },
        title: {
            text: `<div style="text-align:center;margin-top:14px;">
                <span style="font-size:2.2rem;font-weight:800;color:${C.guinda};font-family:Montserrat,sans-serif;line-height:1;">${total}</span><br>
                <span style="font-size:.65rem;font-weight:700;color:${C.textoSuave};letter-spacing:.08em;line-height:1.2;">TEMAS</span>
            </div>`,
            align: 'center',
            verticalAlign: 'middle',
            y: -24, // Offset slightly upward to account for bottom legend pushing the plot area up
            useHTML: true
        },
        exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: {
            pie: {
                innerSize: '62%',
                dataLabels: {
                    enabled: true,
                    format: '{point.y} ({point.percentage:.0f}%)',
                    distance: 8,
                    style: { fontSize: '9px', fontWeight: '700', textOutline: 'none', color: C.texto }
                },
                showInLegend: true
            }
        },
        legend: {
            enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '11px' }
        },
        series: [{ name: 'Temas', data }]
    });
}

// ============ GAUGE — Avance global ============
export function gaugeAvance(temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-gauge-avance');
    if (!cont) return;
    const avance = temas.length
        ? Math.round(temas.reduce((s, t) => s + (t.avance || 0), 0) / temas.length) : 0;
    const color = avance >= 75 ? C.ok : avance >= 40 ? C.proceso : C.riesgo;

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'solidgauge', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        pane: {
            center: ['50%', '75%'], size: '120%', startAngle: -90, endAngle: 90,
            background: { backgroundColor: C.gaugeTrack, innerRadius: '70%', outerRadius: '100%', shape: 'arc', borderWidth: 0 }
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
export function piePrioridad(temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-pie-prioridad');
    if (!cont) return;
    const counts = temas.reduce((m, t) => { m[t.prioridad || 'Baja'] = (m[t.prioridad || 'Baja'] || 0) + 1; return m; }, {});
    const data = Object.entries(counts).map(([name, y]) => ({ name, y, color: PRIORIDAD_COLOR[name] || C.guinda }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'pie', height: 260 },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
        plotOptions: {
            pie: {
                dataLabels: {
                    enabled: true,
                    format: '{point.y} ({point.percentage:.0f}%)',
                    distance: 8,
                    style: { fontSize: '9px', fontWeight: '700', textOutline: 'none', color: C.texto }
                },
                showInLegend: true
            }
        },
        legend: {
            enabled: true, align: 'center', verticalAlign: 'bottom',
            itemStyle: { fontWeight: '600', fontSize: '11px' }
        },
        series: [{ name: 'Temas', data }]
    });
}

// ============ BARRAS HORIZONTALES — Carga por responsable ============
export function barrasResponsables(temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-barras-responsables');
    if (!cont) return;
    const counts = {};
    temas.filter(t => t.estatus !== 'Concluida').forEach(t => {
        if (t.responsable) {
            counts[t.responsable] = (counts[t.responsable] || 0) + 1;
        }
        if (t.corresponsables) {
            t.corresponsables.forEach(c => {
                if (c.nombre) {
                    counts[c.nombre] = (counts[c.nombre] || 0) + 1;
                }
            });
        }
    });
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
        tooltip: { valueSuffix: ' temas pendientes' },
        legend: { enabled: false },
        series: [{
            name: 'Pendientes',
            data: sorted.map(([, v], i) => ({ y: v, color: palette[i % palette.length] })),
            borderRadius: 8,
            dataLabels: { enabled: true, format: '{y}', style: { fontWeight: '700', color: C.texto } }
        }]
    });
}

// ============ BARRAS APILADAS — Por actividad ============
export function barrasApiladasTemas(actividades, temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-stacked-temas');
    if (!cont) return;
    const claves = ['Concluida', 'En proceso', 'Pendiente', 'Vencida'];
    const isMobile = window.innerWidth < 600;
    const cats = actividades.map(a => {
        const maxLen = isMobile ? 15 : 30;
        return a.actividad.length > maxLen ? a.actividad.slice(0, maxLen - 2) + '…' : a.actividad;
    });
    const series = claves.map(k => ({
        name: k, color: ESTATUS_COLOR[k],
        data: actividades.map(a => temas.filter(t => t.actividadId === a.id && t.estatus === k).length)
    }));

    Highcharts.chart(cont, {
        chart: { ...BASE_CHART, type: 'bar', height: Math.max(180, actividades.length * 44 + 80) },
        credits: { enabled: false }, title: { text: '' }, exporting: { enabled: false },
        xAxis: {
            categories: cats,
            labels: { style: { fontWeight: '600', fontSize: isMobile ? '9px' : '11px' } }
        },
        yAxis: { title: { text: '' }, allowDecimals: false },
        plotOptions: {
            bar: {
                stacking: 'normal',
                borderRadius: 8,
                dataLabels: {
                    enabled: true,
                    formatter: function() { return this.y > 0 ? this.y : null; },
                    style: { fontSize: '9px', fontWeight: '700', color: '#ffffff', textOutline: 'none' }
                }
            }
        },
        tooltip: { shared: false, valueSuffix: ' temas' },
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

export function treemapTemas(actividades, temas) {
    applyInstitutionalTheme();
    const cont = document.getElementById('chart-treemap-temas');
    if (!cont) return;

    const activeActividadIds = new Set(actividades.map(a => a.id));
    const filteredTemas = temas.filter(t => activeActividadIds.has(t.actividadId));

    const data = [];

    actividades.forEach(a => {
        const subTemas = filteredTemas.filter(t => t.actividadId === a.id);
        data.push({
            id: `a_${a.id}`,
            name: a.actividad,
            color: Highcharts.color(C.guinda).setOpacity(0.08).get(),
            value: subTemas.length || 1
        });
    });

    filteredTemas.forEach(t => {
        data.push({
            id: `t_${t.id}`,
            name: t.tema,
            parent: `a_${t.actividadId}`,
            value: 1,
            colorValue: t.avance || 0,
            responsable: t.responsable || 'Sin responsable',
            estatus: t.estatus || 'Pendiente'
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
            minColor: C.treemapMin,
            maxColor: C.treemapMax,
            stops: C.treemapStops
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
                        Temas: <b>{point.value}</b>
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
                        color: C.treemapL1
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
                        color: C.treemapL2
                    }
                },
                borderWidth: 1,
                borderColor: C.borde
            }],
            data
        }]
    });
}

export function renderAllCharts(actividades, temas) {
    donutEstatus(temas);
    gaugeAvance(temas);
    piePrioridad(temas);
    barrasResponsables(temas);
    barrasApiladasTemas(actividades, temas);
    treemapTemas(actividades, temas);
}