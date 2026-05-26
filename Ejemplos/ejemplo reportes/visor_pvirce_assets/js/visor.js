const DATA = window.PVIRCE_DATA || { proyectos: [], resumenEF: [], rntContexto: [] };

const state = {
  entidad: "Todas",
  fuente: "Todas",
  anio: "Todos",
  tecnologia: "Todas",
  participacion: "Todas",
  desglose: "total",
  limiteEntidades: 10,
  busqueda: "",
  pvirceBusqueda: "",
  gatBusqueda: "",
  entidadBusqueda: "",
  pagePvirce: 1,
  pageGat: 1,
  pageEntidades: 1,
};

const fmt = new Intl.NumberFormat("es-MX", { maximumFractionDigits: 1 });
const fmtInt = new Intl.NumberFormat("es-MX", { maximumFractionDigits: 0 });
const PAGE_SIZE = 25;
const ENTITY_PAGE_SIZE = 12;
const chartOptionsById = {};
const chartsById = {};
const EXPORT_SCALE = 3;
let currentPvirce = [];
let currentGat = [];
let currentEntities = [];
let fullscreenChart = null;
let fullscreenChartId = "";
const chartTableRows = {};

const TECH_COLORS = {
  FV: "#F2C94C",
  FOTOVOLTAICA: "#F2C94C",
  EO: "#56A3A6",
  "EÓLICO": "#56A3A6",
  BAT: "#6C5CE7",
  HID: "#2D9CDB",
  CC: "#828282",
  "CC/HIDN": "#27AE60",
  "CC/COG_EF": "#4F4F4F",
  BIO: "#219653",
  CI: "#B97A20",
  TC: "#A0A0A0",
  TG: "#BDBDBD",
  GEO: "#EB5757",
  CSP: "#F2994A",
  H2: "#2F80ED",
  HIBRIDOS: "#1E5B4F",
  "Sin dato": "#98989A",
};

const PARTICIPATION_COLORS = {
  CFE: "#1E5B4F",
  Particulares: "#9B2247",
  "Por Definir": "#A57F2C",
  "Privada / GAT": "#6F2B45",
  "Sin dato": "#98989A",
};

function sum(items, key = "mw") {
  return items.reduce((acc, item) => acc + (Number(item[key]) || 0), 0);
}

function unique(items, key) {
  return [...new Set(items.map((item) => item[key]).filter(Boolean))].sort((a, b) => String(a).localeCompare(String(b), "es"));
}

function groupSum(items, key) {
  const map = new Map();
  items.forEach((item) => {
    const label = item[key] || "Sin dato";
    map.set(label, (map.get(label) || 0) + (Number(item.mw) || 0));
  });
  return [...map.entries()].map(([name, y]) => ({ name, y })).sort((a, b) => b.y - a.y);
}

function groupCount(items, key) {
  const map = new Map();
  items.forEach((item) => {
    const label = item[key] || "Sin dato";
    map.set(label, (map.get(label) || 0) + 1);
  });
  return [...map.entries()].map(([name, y]) => ({ name, y })).sort((a, b) => b.y - a.y);
}

function stackedBy(items, categoryKey, stackKey, limit = null, sortByTotal = true) {
  const categoryTotals = new Map();
  const stacks = [...new Set(items.map((item) => item[stackKey] || "Sin dato"))].sort((a, b) => String(a).localeCompare(String(b), "es"));
  items.forEach((item) => {
    const category = item[categoryKey] || "Sin dato";
    categoryTotals.set(category, (categoryTotals.get(category) || 0) + (Number(item.mw) || 0));
  });
  let categories = [...categoryTotals.keys()];
  if (sortByTotal) {
    categories.sort((a, b) => categoryTotals.get(b) - categoryTotals.get(a));
  } else {
    categories.sort((a, b) => Number(a) - Number(b));
  }
  if (limit) categories = categories.slice(0, limit);
  const series = stacks.map((stack) => ({
    name: stack,
    data: categories.map((category) => {
      const value = items
        .filter((item) => (item[categoryKey] || "Sin dato") === category && (item[stackKey] || "Sin dato") === stack)
        .reduce((acc, item) => acc + (Number(item.mw) || 0), 0);
      return Number(value.toFixed(2));
    }),
  })).filter((serie) => serie.data.some((value) => value > 0));
  return { categories, series };
}

function colorFor(name) {
  return TECH_COLORS[name] || PARTICIPATION_COLORS[name] || "#6C757D";
}

function applySeriesColors(series) {
  const palette = state.desglose === "tipo" ? TECH_COLORS : PARTICIPATION_COLORS;
  return series.map((serie) => ({ ...serie, color: palette[serie.name] || colorFor(serie.name) }));
}

function applyPointColors(points) {
  return points.map((point) => ({ ...point, color: colorFor(point.name) }));
}

function stackLabels(enabled) {
  return {
    title: { text: "MW" },
    stackLabels: {
      enabled,
      crop: false,
      overflow: "allow",
      formatter: function () {
        return this.total > 0 ? `${fmt.format(this.total)} MW` : "";
      },
      style: {
        color: "#1A1A1A",
        fontSize: "11px",
        fontWeight: "700",
        textOutline: "2px #FFFFFF",
      },
    },
  };
}

function tableIdForChart(chartId) {
  return `table${chartId.charAt(0).toUpperCase()}${chartId.slice(1)}`;
}

function renderChartDataTable(chartId, data) {
  const target = document.getElementById(tableIdForChart(chartId));
  if (!target) return;
  if (!data || !data.categories || !data.series) {
    target.innerHTML = "";
    return;
  }
  const header = ["Categoría", ...data.series.map((serie) => serie.name), "Total"];
  const rows = data.categories.map((category, idx) => {
    const values = data.series.map((serie) => Number(serie.data[idx]) || 0);
    const total = values.reduce((acc, value) => acc + value, 0);
    return [category, ...values, total];
  });
  chartTableRows[chartId] = rows.map((row) => {
    const obj = { Categoria: row[0] };
    data.series.forEach((serie, idx) => {
      obj[serie.name] = row[idx + 1];
    });
    obj["Total MW"] = row[row.length - 1];
    return obj;
  });
  target.innerHTML = `
    <div class="chart-table-actions"><button type="button" data-chart-table="${chartId}">Bajar Excel</button></div>
    <table>
      <thead><tr>${header.map((h, idx) => `<th>${idx > 0 && idx <= data.series.length ? `<span class="swatch" style="background:${colorFor(h)}"></span>` : ""}${h}</th>`).join("")}</tr></thead>
      <tbody>${rows.map((row) => `<tr>${row.map((cell, idx) => `<td>${idx === 0 ? cell : `${fmt.format(cell)} MW`}</td>`).join("")}</tr>`).join("")}</tbody>
    </table>
  `;
}

function renderPieDataTable(chartId, points) {
  const target = document.getElementById(tableIdForChart(chartId));
  if (!target) return;
  const total = points.reduce((acc, point) => acc + (Number(point.y) || 0), 0);
  chartTableRows[chartId] = points.map((point) => {
    const value = Number(point.y) || 0;
    return {
      Categoria: point.name,
      "MW": value,
      "Porcentaje": total ? value / total * 100 : 0,
    };
  });
  target.innerHTML = `
    <div class="chart-table-actions"><button type="button" data-chart-table="${chartId}">Bajar Excel</button></div>
    <table>
      <thead><tr><th>Categoría</th><th>MW</th><th>%</th></tr></thead>
      <tbody>${points.map((point) => {
        const value = Number(point.y) || 0;
        const pct = total ? value / total * 100 : 0;
        return `<tr><td><span class="swatch" style="background:${point.color || colorFor(point.name)}"></span>${point.name}</td><td>${fmt.format(value)} MW</td><td>${pct.toFixed(1)}%</td></tr>`;
      }).join("")}</tbody>
    </table>
  `;
}

function chartDataByMode(items, categoryKey, sourceLabel, limit = null, sortByTotal = true) {
  if (state.desglose === "total") {
    const grouped = groupSum(items, categoryKey);
    let rows = sortByTotal ? grouped : grouped.sort((a, b) => Number(a.name) - Number(b.name));
    if (limit) rows = rows.slice(0, limit);
    return {
      categories: rows.map((x) => x.name),
      series: [{ name: sourceLabel, data: rows.map((x) => Number(x.y.toFixed(2))), color: sourceLabel.includes("GAT") ? "#1E5B4F" : "#9B2247" }],
      stacking: false,
      suffix: "total",
    };
  }
  const stackKey = state.desglose === "participacion" ? "participacion" : "tipo";
  const stacked = stackedBy(items, categoryKey, stackKey, limit, sortByTotal);
  return {
    ...stacked,
    stacking: true,
    suffix: state.desglose === "participacion" ? "por participación" : "por tecnología",
  };
}

function filtered() {
  const q = state.busqueda.toLowerCase().trim();
  return DATA.proyectos.filter((p) => {
    if (state.entidad !== "Todas" && p.entidad !== state.entidad) return false;
    if (state.fuente !== "Todas" && p.fuente !== state.fuente) return false;
    if (state.anio !== "Todos" && String(p.anio || "") !== state.anio) return false;
    if (state.tecnologia !== "Todas" && p.tipo !== state.tecnologia) return false;
    if (state.participacion !== "Todas" && p.participacion !== state.participacion) return false;
    if (q) {
      const haystack = [p.proyecto, p.entidad, p.municipio, p.tipo, p.estatus, p.propietario, p.region].join(" ").toLowerCase();
      if (!haystack.includes(q)) return false;
    }
    return true;
  });
}

function filteredNationalBase() {
  const originalEntidad = state.entidad;
  state.entidad = "Todas";
  const items = filtered();
  state.entidad = originalEntidad;
  return items;
}

function fillSelect(id, values, allLabel) {
  const select = document.getElementById(id);
  select.innerHTML = "";
  const opt = document.createElement("option");
  opt.value = allLabel;
  opt.textContent = allLabel;
  select.appendChild(opt);
  values.forEach((value) => {
    const item = document.createElement("option");
    item.value = value;
    item.textContent = value;
    select.appendChild(item);
  });
}

function initFilters() {
  fillSelect("filtroEntidad", DATA.resumenEF.map((item) => item.entidad), "Todas");
  fillSelect("filtroFuente", unique(DATA.proyectos, "fuente"), "Todas");
  fillSelect("filtroAnio", unique(DATA.proyectos, "anio").map(String), "Todos");
  fillSelect("filtroTecnologia", unique(DATA.proyectos, "tipo"), "Todas");
  fillSelect("filtroParticipacion", unique(DATA.proyectos, "participacion"), "Todas");

  [
    ["filtroEntidad", "entidad"],
    ["filtroFuente", "fuente"],
    ["filtroAnio", "anio"],
    ["filtroTecnologia", "tecnologia"],
    ["filtroParticipacion", "participacion"],
  ].forEach(([id, key]) => {
    document.getElementById(id).addEventListener("change", (ev) => {
      state[key] = ev.target.value;
      state.pagePvirce = 1;
      state.pageGat = 1;
      state.pageEntidades = 1;
      render();
    });
  });

  document.getElementById("busqueda").addEventListener("input", (ev) => {
    state.busqueda = ev.target.value;
    state.pagePvirce = 1;
    state.pageGat = 1;
    render();
  });

  document.getElementById("desgloseGraficas").addEventListener("change", (ev) => {
    state.desglose = ev.target.value;
    renderCharts(filtered());
  });

  document.getElementById("limiteEntidades").addEventListener("change", (ev) => {
    state.limiteEntidades = ev.target.value === "all" ? null : Number(ev.target.value);
    renderCharts(filtered());
  });

  document.getElementById("buscarPvirce").addEventListener("input", (ev) => {
    state.pvirceBusqueda = ev.target.value;
    state.pagePvirce = 1;
    renderPvirceTable(currentPvirce);
  });

  document.getElementById("buscarGat").addEventListener("input", (ev) => {
    state.gatBusqueda = ev.target.value;
    state.pageGat = 1;
    renderGatTable(currentGat);
  });

  document.getElementById("buscarEntidades").addEventListener("input", (ev) => {
    state.entidadBusqueda = ev.target.value;
    state.pageEntidades = 1;
    renderEntityTable(filtered());
  });

  document.getElementById("limpiar").addEventListener("click", () => {
    state.entidad = "Todas";
    state.fuente = "Todas";
    state.anio = "Todos";
    state.tecnologia = "Todas";
    state.participacion = "Todas";
    state.desglose = "total";
    state.limiteEntidades = 10;
    state.busqueda = "";
    state.pvirceBusqueda = "";
    state.gatBusqueda = "";
    state.entidadBusqueda = "";
    state.pagePvirce = 1;
    state.pageGat = 1;
    state.pageEntidades = 1;
    document.getElementById("filtroEntidad").value = "Todas";
    document.getElementById("filtroFuente").value = "Todas";
    document.getElementById("filtroAnio").value = "Todos";
    document.getElementById("filtroTecnologia").value = "Todas";
    document.getElementById("filtroParticipacion").value = "Todas";
    document.getElementById("desgloseGraficas").value = "total";
    document.getElementById("limiteEntidades").value = "10";
    document.getElementById("busqueda").value = "";
    document.getElementById("buscarPvirce").value = "";
    document.getElementById("buscarGat").value = "";
    document.getElementById("buscarEntidades").value = "";
    render();
  });

  document.querySelectorAll("[data-chart]").forEach((button) => {
    button.addEventListener("click", () => handleChartAction(button.dataset.chart, button.dataset.action));
  });

  document.querySelectorAll("[data-export]").forEach((button) => {
    button.addEventListener("click", () => exportTable(button.dataset.export));
  });

  document.addEventListener("click", (ev) => {
    const button = ev.target.closest("[data-chart-table]");
    if (button) exportChartTable(button.dataset.chartTable);
  });

  document.getElementById("cerrarModal").addEventListener("click", closeFullscreen);
  document.getElementById("descargarModalPng").addEventListener("click", () => {
    if (fullscreenChart?.exportChartLocal) {
      exportChartAsPng(fullscreenChart, `${fullscreenChartId}_fullscreen_${Date.now()}`);
    }
  });
  document.getElementById("chartModal").addEventListener("click", (ev) => {
    if (ev.target.id === "chartModal") closeFullscreen();
  });
}

function setKpis(items) {
  const pvirce = items.filter((p) => p.fuente === "PVIRCE");
  const gat = items.filter((p) => p.fuente === "GAT");
  const renov = pvirce.filter((p) => String(p.renovable).toLowerCase().includes("renovable") || String(p.renovable).toLowerCase().includes("bater"));
  const cfe = pvirce.filter((p) => p.estatus === "CFE");
  const entidades = new Set(items.map((p) => p.entidad).filter(Boolean)).size;
  const nationalItems = filteredNationalBase();
  const nationalTotal = sum(nationalItems);
  const selectedShare = state.entidad === "Todas" ? 100 : (nationalTotal ? sum(items) / nationalTotal * 100 : 0);

  document.getElementById("kpiPvirceMw").textContent = `${fmt.format(sum(pvirce))} MW`;
  document.getElementById("kpiPvirceReg").textContent = fmtInt.format(pvirce.length);
  document.getElementById("kpiGatMw").textContent = `${fmt.format(sum(gat))} MW`;
  document.getElementById("kpiGatReg").textContent = fmtInt.format(gat.length);
  document.getElementById("kpiParticipacionNacional").textContent = `${selectedShare.toFixed(2)}%`;
  document.getElementById("kpiEntidades").textContent = `${entidades}`;
  document.getElementById("kpiRenovable").textContent = `${fmt.format(sum(renov))} MW`;
  document.getElementById("kpiCfe").textContent = `${fmt.format(sum(cfe))} MW`;
  document.getElementById("kpiPeriodo").textContent = periodLabel(items);
}

function periodLabel(items) {
  const years = items.map((p) => Number(p.anio)).filter(Boolean);
  if (!years.length) return "Sin año";
  return `${Math.min(...years)}-${Math.max(...years)}`;
}

function chartBase(id, options) {
  chartOptionsById[id] = options;
  if (!window.Highcharts) {
    const el = document.getElementById(id);
    if (el) {
      el.innerHTML = '<div class="empty">No se pudo cargar Highcharts. Verifica conexión a internet o abre el visor con acceso al CDN.</div>';
    }
    return;
  }
  const optionConfig = { ...(options.config || {}) };
  const customPlotOptions = optionConfig.plotOptions || {};
  delete optionConfig.plotOptions;
  const basePlotOptions = getBasePlotOptions();
  const config = {
    chart: { backgroundColor: "transparent", style: { fontFamily: "Noto Sans" } },
    title: { text: options.title, align: "left", style: { color: "#1A1A1A", fontWeight: "700" } },
    credits: { enabled: false },
    accessibility: { enabled: false },
    legend: { enabled: options.legend || false },
    colors: ["#9B2247", "#1E5B4F", "#A57F2C", "#6C757D", "#C8B273", "#4E6E81"],
    exporting: { enabled: false, fallbackToExportServer: false },
    plotOptions: mergePlotOptions(basePlotOptions, customPlotOptions),
    ...optionConfig,
  };
  chartsById[id] = Highcharts.chart(id, config);
}

function getBasePlotOptions() {
  return {
    series: {
      dataLabels: {
        enabled: true,
        allowOverlap: false,
        crop: false,
        overflow: "justify",
        style: { fontSize: "10px", fontWeight: "700", textOutline: "none" },
        formatter: function () {
          if (typeof this.y !== "number" || this.y <= 0) return "";
          if (this.series.chart.options.plotOptions?.series?.stacking && this.y < 300) return "";
          return fmt.format(this.y);
        },
      },
    },
    bar: {
        dataLabels: {
          enabled: true,
          allowOverlap: false,
          crop: false,
          overflow: "allow",
          inside: false,
          backgroundColor: "rgba(255,255,255,0.86)",
          borderColor: "#DDDDDD",
          borderWidth: 1,
          borderRadius: 3,
          padding: 2,
          style: { fontSize: "10px", fontWeight: "700", textOutline: "none", color: "#1A1A1A" },
        formatter: function () {
          if (typeof this.y !== "number" || this.y <= 0) return "";
          if (this.series.chart.options.plotOptions?.series?.stacking && this.y < 300) return "";
          return fmt.format(this.y);
        },
      },
    },
    column: {
        dataLabels: {
          enabled: true,
          allowOverlap: false,
          crop: false,
          overflow: "allow",
          backgroundColor: "rgba(255,255,255,0.86)",
          borderColor: "#DDDDDD",
          borderWidth: 1,
          borderRadius: 3,
          padding: 2,
          style: { fontSize: "10px", fontWeight: "700", textOutline: "none", color: "#1A1A1A" },
        formatter: function () {
          if (typeof this.y !== "number" || this.y <= 0) return "";
          if (this.series.chart.options.plotOptions?.series?.stacking && this.y < 300) return "";
          return fmt.format(this.y);
        },
      },
    },
    pie: {
      size: "70%",
      showInLegend: true,
      dataLabels: {
        enabled: true,
        allowOverlap: false,
        crop: false,
        overflow: "allow",
        distance: 28,
        connectorWidth: 1,
        connectorColor: "#666666",
        backgroundColor: "rgba(255,255,255,0.92)",
        borderColor: "#DDDDDD",
        borderWidth: 1,
        borderRadius: 4,
        padding: 4,
        style: { fontSize: "11px", fontWeight: "700", textOutline: "none", color: "#1A1A1A" },
        formatter: function () {
          const y = typeof this.y === "number" ? this.y : 0;
          const pct = typeof this.percentage === "number" ? this.percentage : 0;
          return y > 0 ? `${this.point.name}<br>${pct.toFixed(1)}% · ${fmt.format(y)} MW` : "";
        },
      },
    },
  };
}

function mergePlotOptions(base, custom) {
  const merged = { ...base };
  Object.keys(custom || {}).forEach((key) => {
    merged[key] = { ...(base[key] || {}), ...(custom[key] || {}) };
    if (base[key]?.dataLabels || custom[key]?.dataLabels) {
      merged[key].dataLabels = { ...(base[key]?.dataLabels || {}), ...(custom[key]?.dataLabels || {}) };
    }
  });
  return merged;
}

function handleChartAction(id, action) {
  const chart = chartsById[id];
  if (!chart) return;
  if (action === "png") {
    if (chart.exportChartLocal) {
      exportChartAsPng(chart, `${id}_${Date.now()}`);
    }
    return;
  }
  if (action === "fullscreen") {
    openFullscreen(id);
  }
}

function exportChartAsPng(chart, filename) {
  chart.exportChartLocal({
    type: "image/png",
    filename,
    sourceWidth: Math.round(chart.chartWidth),
    sourceHeight: Math.round(chart.chartHeight),
    scale: EXPORT_SCALE,
  });
}

function openFullscreen(id) {
  const modal = document.getElementById("chartModal");
  const target = document.getElementById("chartFullscreen");
  target.innerHTML = "";
  modal.classList.add("open");
  modal.setAttribute("aria-hidden", "false");
  fullscreenChartId = id;
  const options = chartOptionsById[id];
  if (window.Highcharts && options) {
    const optionConfig = { ...(options.config || {}) };
    const customPlotOptions = optionConfig.plotOptions || {};
    delete optionConfig.plotOptions;
    fullscreenChart = Highcharts.chart("chartFullscreen", {
      chart: { backgroundColor: "transparent", style: { fontFamily: "Noto Sans" } },
      title: { text: options.title, align: "left", style: { color: "#1A1A1A", fontWeight: "700" } },
      credits: { enabled: false },
      accessibility: { enabled: false },
      legend: { enabled: options.legend || false },
      colors: ["#9B2247", "#1E5B4F", "#A57F2C", "#6C757D", "#C8B273", "#4E6E81"],
      exporting: { enabled: false, fallbackToExportServer: false },
      plotOptions: mergePlotOptions(getBasePlotOptions(), customPlotOptions),
      ...optionConfig,
      chart: { ...(optionConfig.chart || {}), backgroundColor: "transparent", style: { fontFamily: "Noto Sans" } },
    });
  }
}

function closeFullscreen() {
  const modal = document.getElementById("chartModal");
  modal.classList.remove("open");
  modal.setAttribute("aria-hidden", "true");
  if (fullscreenChart) {
    fullscreenChart.destroy();
    fullscreenChart = null;
  }
  fullscreenChartId = "";
  document.getElementById("chartFullscreen").innerHTML = "";
}

function renderCharts(items) {
  const pvirce = items.filter((p) => p.fuente === "PVIRCE");
  const gat = items.filter((p) => p.fuente === "GAT");
  const pvirceByEntidad = chartDataByMode(pvirce, "entidad", "MW PVIRCE", state.limiteEntidades, true);
  chartBase("chartPvirceEntidad", {
    title: `PVIRCE: capacidad por Entidad Federativa (${pvirceByEntidad.suffix})`,
    config: {
      chart: { type: "bar" },
      xAxis: { categories: pvirceByEntidad.categories, title: { text: null } },
      yAxis: stackLabels(pvirceByEntidad.stacking),
      plotOptions: { series: { stacking: pvirceByEntidad.stacking ? "normal" : undefined } },
      legend: { enabled: true },
      series: pvirceByEntidad.stacking ? applySeriesColors(pvirceByEntidad.series) : pvirceByEntidad.series,
    },
  });
  renderChartDataTable("chartPvirceEntidad", pvirceByEntidad);

  const gatByEntidad = chartDataByMode(gat, "entidad", "MW GAT", state.limiteEntidades, true);
  chartBase("chartGatEntidad", {
    title: `GAT Mixtos: capacidad por Entidad Federativa (${gatByEntidad.suffix})`,
    config: {
      chart: { type: "bar" },
      xAxis: { categories: gatByEntidad.categories, title: { text: null } },
      yAxis: stackLabels(gatByEntidad.stacking),
      plotOptions: { series: { stacking: gatByEntidad.stacking ? "normal" : undefined } },
      legend: { enabled: true },
      series: gatByEntidad.stacking ? applySeriesColors(gatByEntidad.series) : gatByEntidad.series,
    },
  });
  renderChartDataTable("chartGatEntidad", gatByEntidad);

  const pvirceParticipacion = applyPointColors(groupSum(pvirce, "participacion"));
  chartBase("chartPvirceParticipacion", {
    title: "PVIRCE: generación por participación",
    config: {
      chart: { type: "pie", spacing: [18, 28, 18, 28] },
      tooltip: { pointFormat: "<b>{point.y:,.1f} MW</b>" },
      plotOptions: { pie: { innerSize: "45%" } },
      series: [{ name: "MW", data: pvirceParticipacion }],
    },
  });
  renderPieDataTable("chartPvirceParticipacion", pvirceParticipacion);

  const byCapital = groupSum(gat, "origenCapital");
  const gatCapitalPoints = byCapital.length ? applyPointColors(byCapital) : [{ name: "Sin GAT", y: 0, color: "#98989A" }];
  chartBase("chartGatCapital", {
    title: "GAT Mixtos: capacidad por origen de capital",
    config: {
      chart: { type: "pie", spacing: [18, 28, 18, 28] },
      tooltip: { pointFormat: "<b>{point.y:,.1f} MW</b>" },
      plotOptions: { pie: { innerSize: "45%" } },
      series: [{ name: "MW", data: gatCapitalPoints }],
    },
  });
  renderPieDataTable("chartGatCapital", gatCapitalPoints);

  const pvirceYear = chartDataByMode(pvirce, "anio", "MW PVIRCE", null, false);
  chartBase("chartPvirceAnio", {
    title: `PVIRCE: trayectoria anual (${pvirceYear.suffix})`,
    config: {
      chart: { type: "column" },
      xAxis: { categories: pvirceYear.categories },
      yAxis: stackLabels(pvirceYear.stacking),
      plotOptions: { series: { stacking: pvirceYear.stacking ? "normal" : undefined } },
      legend: { enabled: true },
      series: pvirceYear.stacking ? applySeriesColors(pvirceYear.series) : pvirceYear.series,
    },
  });
  renderChartDataTable("chartPvirceAnio", pvirceYear);

  const gatYear = chartDataByMode(gat, "anio", "MW GAT", null, false);
  chartBase("chartGatAnio", {
    title: `GAT Mixtos: trayectoria anual EOC (${gatYear.suffix})`,
    config: {
      chart: { type: "column" },
      xAxis: { categories: gatYear.categories },
      yAxis: stackLabels(gatYear.stacking),
      plotOptions: { series: { stacking: gatYear.stacking ? "normal" : undefined } },
      legend: { enabled: true },
      series: gatYear.stacking ? applySeriesColors(gatYear.series) : gatYear.series,
    },
  });
  renderChartDataTable("chartGatAnio", gatYear);
}

function renderNarrative(items) {
  const entidad = state.entidad === "Todas" ? "el conjunto nacional" : state.entidad;
  const pvirce = items.filter((p) => p.fuente === "PVIRCE");
  const gat = items.filter((p) => p.fuente === "GAT");
  const privadaPvirce = pvirce.filter((p) => p.estatus === "Particulares");
  const cfe = pvirce.filter((p) => p.estatus === "CFE");
  const indef = pvirce.filter((p) => p.estatus === "Por Definir");
  const topPv = groupSum(pvirce, "tipo")[0];
  const topGat = groupSum(gat, "tipo")[0];

  document.getElementById("narrativaGeneracion").innerHTML = `
    <div class="source-card">
      <h3>Generación eléctrica<br>Inversión privada</h3>
      <p>En PVIRCE, la participación de particulares suma <strong>${fmt.format(sum(privadaPvirce))} MW</strong> en <strong>${fmtInt.format(privadaPvirce.length)}</strong> proyectos. GAT Mixtos se presenta aparte como cartera privada en seguimiento, con <strong>${fmt.format(sum(gat))} MW</strong> y campos de GIE, capital, FEOC y RTB.</p>
    </div>
    <div class="source-card">
      <h3>Generación eléctrica<br>Inversión público-privada</h3>
      <p>PVIRCE permite segmentar la cartera por CFE, particulares y por definir: CFE suma <strong>${fmt.format(sum(cfe))} MW</strong>, particulares <strong>${fmt.format(sum(privadaPvirce))} MW</strong> y por definir <strong>${fmt.format(sum(indef))} MW</strong>. No se suman con GAT hasta validar coincidencias proyecto por proyecto.</p>
    </div>
    <div class="source-card">
      <h3>Generación eléctrica<br>Análisis generación vs demanda</h3>
      <p>La capacidad programada se presenta por año y tecnología. La demanda específica se extrae del programa RNT/RGD cuando existe para el sistema correspondiente; si no está asociada a la entidad, se marca como pendiente de extracción puntual.</p>
    </div>
  `;

  document.getElementById("narrativaEntidad").innerHTML = `
    <strong>${entidad}</strong>: PVIRCE reporta ${fmtInt.format(pvirce.length)} proyectos y ${fmt.format(sum(pvirce))} MW; GAT Mixtos reporta ${fmtInt.format(gat.length)} proyectos y ${fmt.format(sum(gat))} MW.
    ${topPv ? `En PVIRCE lidera <strong>${topPv.name}</strong> con ${fmt.format(topPv.y)} MW.` : ""}
    ${topGat ? `En GAT lidera <strong>${topGat.name}</strong> con ${fmt.format(topGat.y)} MW.` : ""}
  `;

  const rnt = DATA.rntContexto.map((card) => `
    <div class="source-card">
      <h3>${card.titulo}</h3>
      <p>${card.texto}</p>
    </div>
  `).join("");
  document.getElementById("rntContexto").innerHTML = rnt;
}

function textIncludes(row, query) {
  if (!query) return true;
  return Object.values(row).join(" ").toLowerCase().includes(query.toLowerCase().trim());
}

function renderPager(id, page, total, pageSize, onPage) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const pager = document.getElementById(id);
  pager.innerHTML = "";
  const prev = document.createElement("button");
  prev.textContent = "Anterior";
  prev.disabled = page <= 1;
  prev.addEventListener("click", () => onPage(page - 1));
  const label = document.createElement("span");
  label.textContent = `Página ${page} de ${totalPages} · ${fmtInt.format(total)} proyectos`;
  const next = document.createElement("button");
  next.textContent = "Siguiente";
  next.disabled = page >= totalPages;
  next.addEventListener("click", () => onPage(page + 1));
  pager.append(prev, label, next);
}

function renderEntityTable(items) {
  const entityMap = new Map(DATA.resumenEF.map((row) => [row.entidad, {
    entidad: row.entidad,
    proyectos: 0,
    pvirceMw: 0,
    gatMw: 0,
  }]));
  items.forEach((item) => {
    if (!entityMap.has(item.entidad)) {
      entityMap.set(item.entidad, { entidad: item.entidad, proyectos: 0, pvirceMw: 0, gatMw: 0 });
    }
    const row = entityMap.get(item.entidad);
    row.proyectos += 1;
    if (item.fuente === "PVIRCE") row.pvirceMw += Number(item.mw) || 0;
    if (item.fuente === "GAT") row.gatMw += Number(item.mw) || 0;
  });
  currentEntities = [...entityMap.values()]
    .filter((r) => state.entidad === "Todas" || r.entidad === state.entidad)
    .filter((r) => textIncludes(r, state.entidadBusqueda))
    .sort((a, b) => (b.pvirceMw + b.gatMw) - (a.pvirceMw + a.gatMw));
  const totalPages = Math.max(1, Math.ceil(currentEntities.length / ENTITY_PAGE_SIZE));
  state.pageEntidades = Math.min(state.pageEntidades, totalPages);
  const start = (state.pageEntidades - 1) * ENTITY_PAGE_SIZE;
  const pageRows = currentEntities.slice(start, start + ENTITY_PAGE_SIZE);
  const rows = pageRows
    .map((r) => `
      <tr>
        <td>${r.entidad}</td>
        <td>${fmtInt.format(r.proyectos)}</td>
        <td>${fmt.format(r.pvirceMw)}</td>
        <td>${fmt.format(r.gatMw)}</td>
      </tr>
    `).join("");
  document.getElementById("tablaEntidades").innerHTML = rows || `<tr><td colspan="4" class="empty">Sin proyectos.</td></tr>`;
  renderPager("pagerEntidades", state.pageEntidades, currentEntities.length, ENTITY_PAGE_SIZE, (page) => {
    state.pageEntidades = page;
    renderEntityTable(items);
  });
}

function renderPvirceTable(items) {
  currentPvirce = items.filter((p) => p.fuente === "PVIRCE");
  const filteredRows = currentPvirce.filter((p) => textIncludes(p, state.pvirceBusqueda));
  const totalPages = Math.max(1, Math.ceil(filteredRows.length / PAGE_SIZE));
  state.pagePvirce = Math.min(state.pagePvirce, totalPages);
  const start = (state.pagePvirce - 1) * PAGE_SIZE;
  const pageRows = filteredRows
    .sort((a, b) => (b.mw || 0) - (a.mw || 0))
    .slice(start, start + PAGE_SIZE);
  const rows = pageRows
    .map((p) => `
      <tr>
        <td><span class="badge pvirce">PVIRCE</span></td>
        <td>${p.proyecto || ""}</td>
        <td>${p.entidad || ""}</td>
        <td>${p.municipio || ""}</td>
        <td>${p.anio || ""}</td>
        <td>${p.tipo || ""}</td>
        <td>${fmt.format(p.mw || 0)}</td>
        <td>${p.participacion || ""}</td>
        <td>${p.estatus || ""}</td>
        <td>${p.region || ""}</td>
        <td>${p.propietario || ""}</td>
        <td>${p.origenCapital || ""}</td>
        <td>${p.paisCapital || ""}</td>
      </tr>
    `).join("");
  document.getElementById("tablaPvirce").innerHTML = rows || `<tr><td colspan="13" class="empty">Sin proyectos PVIRCE para la selección actual.</td></tr>`;
  document.getElementById("tableCountPvirce").textContent = `${fmtInt.format(filteredRows.length)} proyectos`;
  renderPager("pagerPvirce", state.pagePvirce, filteredRows.length, PAGE_SIZE, (page) => {
    state.pagePvirce = page;
    renderPvirceTable(items);
  });
}

function renderGatTable(items) {
  currentGat = items.filter((p) => p.fuente === "GAT");
  const filteredRows = currentGat.filter((p) => textIncludes(p, state.gatBusqueda));
  const totalPages = Math.max(1, Math.ceil(filteredRows.length / PAGE_SIZE));
  state.pageGat = Math.min(state.pageGat, totalPages);
  const start = (state.pageGat - 1) * PAGE_SIZE;
  const pageRows = filteredRows
    .sort((a, b) => (b.mw || 0) - (a.mw || 0))
    .slice(start, start + PAGE_SIZE);
  const rows = pageRows.map((p) => `
    <tr>
      <td>${p.proyecto || ""}</td>
      <td>${p.entidad || ""}</td>
      <td>${p.anio || ""}</td>
      <td>${p.tipo || ""}</td>
      <td>${fmt.format(p.mw || 0)}</td>
      <td>${p.estatus || ""}</td>
      <td>${p.region || ""}</td>
      <td>${p.propietario || ""}</td>
      <td>${p.origenCapital || ""}</td>
      <td>${p.paisCapital || ""}</td>
      <td>${p.feoc || ""}</td>
      <td>${p.rtb || ""}</td>
    </tr>
  `).join("");
  document.getElementById("tablaGat").innerHTML = rows || `<tr><td colspan="12" class="empty">Sin proyectos GAT para la selección actual.</td></tr>`;
  document.getElementById("tableCountGat").textContent = `${fmtInt.format(filteredRows.length)} proyectos`;
  renderPager("pagerGat", state.pageGat, filteredRows.length, PAGE_SIZE, (page) => {
    state.pageGat = page;
    renderGatTable(items);
  });
}

function exportTable(kind) {
  if (!window.XLSX) {
    alert("No se pudo cargar la librería XLSX. Revisa la conexión al CDN.");
    return;
  }
  const rows = kind === "entidades"
    ? currentEntities
    : kind === "pvirce"
      ? currentPvirce.filter((p) => textIncludes(p, state.pvirceBusqueda))
      : currentGat.filter((p) => textIncludes(p, state.gatBusqueda));
  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, kind === "entidades" ? "Entidades" : kind === "pvirce" ? "PVIRCE" : "GAT");
  XLSX.writeFile(wb, `visor_pvirce_${kind}_${Date.now()}.xlsx`);
}

function exportChartTable(chartId) {
  if (!window.XLSX) {
    alert("No se pudo cargar la librería XLSX. Revisa la conexión al CDN.");
    return;
  }
  const rows = chartTableRows[chartId] || [];
  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, chartId.slice(0, 31));
  XLSX.writeFile(wb, `${chartId}_datos_${Date.now()}.xlsx`);
}

function render() {
  const items = filtered();
  setKpis(items);
  renderCharts(items);
  renderNarrative(items);
  renderEntityTable(items);
  renderPvirceTable(items);
  renderGatTable(items);
}

document.addEventListener("DOMContentLoaded", () => {
  initFilters();
  render();
});
