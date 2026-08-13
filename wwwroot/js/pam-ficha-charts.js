(function () {
    "use strict";

    // Gráficos ECharts de la ficha PAM/PAMRNT (dona de cartera y anillo B/C).
    // Los contenedores viven en láminas ocultas (display:none), por lo que cada
    // gráfico se inicializa con dimensiones explícitas: el canvas queda pintado
    // aunque la lámina no esté visible y html2canvas lo captura tal cual al exportar.

    var GUINDA = "#9B2247";
    var CREMA = "#EEE5DC";
    var GRIS = "#6F6B66";
    var VERDE = "#0E8A6E";
    var NEUTROS = ["#C9C4BC", "#D8D2C8", "#B7B1A9", "#E3DED6", "#A69F97", "#DBD4CB", "#CFC8BF"];

    function init() {
        var data = window.pamFichaCharts;
        if (!data || !window.echarts) return;

        var listo = document.fonts && document.fonts.ready ? document.fonts.ready : Promise.resolve();
        listo.then(function () {
            construirDona(data);
            construirAnilloBc(data);
            construirDemanda(data);
        });
    }

    function construirDemanda(data) {
        var el = document.getElementById("pam-chart-demanda");
        var d = data.demanda;
        if (!el || !d || !Array.isArray(d.serie) || d.serie.length === 0) return;

        var chart = echarts.init(el, null, {
            renderer: "canvas",
            width: el.clientWidth || 640,
            height: el.clientHeight || 300
        });

        var series = [{
            type: "line",
            smooth: true,
            symbol: "none",
            data: d.serie,
            lineStyle: { color: GUINDA, width: 4 },
            areaStyle: {
                color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                    { offset: 0, color: "rgba(155,34,71,0.26)" },
                    { offset: 1, color: "rgba(155,34,71,0)" }
                ])
            },
            markLine: d.limite ? {
                symbol: "none",
                lineStyle: { color: "#E0A12E", type: "dashed", width: 2 },
                label: {
                    formatter: "Capacidad sin obra nueva",
                    color: "#9A7A1E",
                    fontFamily: '"Noto Sans PAM", "Noto Sans", sans-serif',
                    fontWeight: 700,
                    fontSize: 11,
                    position: "insideStartTop"
                },
                data: [{ yAxis: d.limite }]
            } : undefined,
            markPoint: d.anioNecesaria ? {
                symbol: "circle",
                symbolSize: 14,
                itemStyle: { color: "#E0A12E", borderColor: "#fff", borderWidth: 3 },
                label: {
                    formatter: d.etiquetaNecesaria || "Fecha necesaria",
                    color: GUINDA,
                    fontFamily: '"Noto Sans PAM", "Noto Sans", sans-serif',
                    fontWeight: 700,
                    fontSize: 12,
                    position: "top",
                    distance: 8
                },
                data: [{ coord: [String(d.anioNecesaria), valorEnAnio(d.serie, d.anioNecesaria)] }]
            } : undefined
        }];

        chart.setOption({
            animation: false,
            grid: { left: 8, right: 20, top: 30, bottom: 26, containLabel: true },
            tooltip: {
                trigger: "axis",
                valueFormatter: function (v) {
                    return Number(v).toLocaleString("es-MX") + " " + (d.unidad || "");
                }
            },
            xAxis: {
                type: "category",
                boundaryGap: false,
                data: d.serie.map(function (p) { return String(p[0]); }),
                axisLine: { lineStyle: { color: "#D8D2C8" } },
                axisTick: { show: false },
                axisLabel: {
                    color: GRIS,
                    fontFamily: '"IBM Plex Mono", monospace',
                    fontSize: 11,
                    interval: function (i, v) { return v === "2025" || v === "2040" || (d.anioNecesaria && v === String(d.anioNecesaria)); }
                }
            },
            yAxis: {
                type: "value",
                name: d.unidad || "",
                nameTextStyle: { color: "#9A958E", fontSize: 10, align: "left" },
                axisLine: { show: false },
                axisTick: { show: false },
                splitLine: { lineStyle: { color: "#EFEAE3" } },
                axisLabel: {
                    color: GRIS,
                    fontFamily: '"IBM Plex Mono", monospace',
                    fontSize: 10,
                    formatter: function (v) { return Number(v).toLocaleString("es-MX"); }
                }
            },
            series: series
        });
    }

    function valorEnAnio(serie, anio) {
        var hit = serie.find(function (p) { return p[0] === anio; });
        return hit ? hit[1] : serie[serie.length - 1][1];
    }

    function construirDona(data) {
        var el = document.getElementById("pam-chart-dona");
        if (!el || !Array.isArray(data.dona) || data.dona.length === 0) return;

        var neutro = 0;
        var items = data.dona.map(function (d) {
            return {
                name: d.clave,
                value: d.valor,
                selected: d.activo,
                itemStyle: { color: d.activo ? GUINDA : NEUTROS[neutro++ % NEUTROS.length] },
                label: d.activo ? { color: GUINDA, fontWeight: "bold" } : {}
            };
        });

        var chart = echarts.init(el, null, {
            renderer: "canvas",
            width: el.clientWidth || 400,
            height: el.clientHeight || 305
        });

        chart.setOption({
            animation: false,
            tooltip: {
                trigger: "item",
                valueFormatter: function (valor) {
                    return "$" + Number(valor).toLocaleString("es-MX", {
                        minimumFractionDigits: 3,
                        maximumFractionDigits: 3
                    }) + " MDP";
                }
            },
            series: [{
                type: "pie",
                radius: ["50%", "76%"],
                center: ["50%", "50%"],
                selectedMode: "single",
                selectedOffset: 9,
                label: {
                    formatter: "{b}",
                    color: GRIS,
                    fontFamily: '"IBM Plex Mono", Consolas, monospace',
                    fontSize: 11,
                    fontWeight: 600
                },
                labelLine: { length: 10, length2: 8, lineStyle: { color: "#D8D2C8" } },
                itemStyle: { borderColor: "#ffffff", borderWidth: 2 },
                data: items
            }],
            graphic: [
                {
                    type: "text",
                    left: "center",
                    top: "43%",
                    style: {
                        text: data.donaCentroValor || "",
                        fill: GUINDA,
                        font: '700 30px Patria, Georgia, serif',
                        textAlign: "center"
                    }
                },
                {
                    type: "text",
                    left: "center",
                    top: "55%",
                    style: {
                        text: data.donaCentroEtiqueta || "",
                        fill: GRIS,
                        font: '600 11px "Noto Sans PAM", "Noto Sans", sans-serif',
                        textAlign: "center"
                    }
                }
            ]
        });
    }

    function construirAnilloBc(data) {
        var el = document.getElementById("pam-chart-bc");
        var valor = Number(data.bc);
        if (!el || !(valor > 0)) return;

        var chart = echarts.init(el, null, {
            renderer: "canvas",
            width: el.clientWidth || 300,
            height: el.clientHeight || 300
        });

        chart.setOption({
            animation: false,
            series: [{
                type: "gauge",
                startAngle: 90,
                endAngle: -270,
                min: 0,
                max: Math.max(6, Math.ceil(valor)),
                pointer: { show: false },
                progress: {
                    show: true,
                    roundCap: true,
                    width: 24,
                    itemStyle: { color: GUINDA }
                },
                axisLine: { lineStyle: { width: 24, color: [[1, CREMA]] } },
                splitLine: { show: false },
                axisTick: { show: false },
                axisLabel: { show: false },
                title: {
                    offsetCenter: [0, "-32%"],
                    color: GRIS,
                    fontSize: 12,
                    fontWeight: 600
                },
                detail: {
                    valueAnimation: false,
                    offsetCenter: [0, "6%"],
                    formatter: function (v) { return v.toFixed(2); },
                    color: GUINDA,
                    fontFamily: "Patria, Georgia, serif",
                    fontSize: 54,
                    fontWeight: 700
                },
                data: [{ value: valor, name: "RELACIÓN B/C" }]
            }],
            graphic: [{
                type: "text",
                left: "center",
                top: "64%",
                style: {
                    text: (el.dataset.etiqueta || data.bcEtiqueta || "").toUpperCase(),
                    fill: VERDE,
                    font: '700 11px "Noto Sans PAM", "Noto Sans", sans-serif',
                    textAlign: "center"
                }
            }]
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
