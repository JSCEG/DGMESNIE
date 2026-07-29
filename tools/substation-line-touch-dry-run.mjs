import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";
import process from "node:process";

const dashboardBaseUrl =
  process.env.DGMESNIE_DASHBOARD_URL ?? "http://127.0.0.1:5155";
const outputDirectory =
  process.env.DGMESNIE_LINE_TOUCH_OUTPUT ??
  path.resolve("out", "red-electrica", "conexiones-candidatas");
const targetScope =
  process.env.DGMESNIE_LINE_TOUCH_SCOPE ?? "promoted";

const stationsUrl = new URL(
  "/DashboardProyectos/RedElectrica/InventarioSubestaciones/GeoJson",
  dashboardBaseUrl,
);
stationsUrl.searchParams.set("networkLevel", "transmision");
const linesUrl = new URL(
  "/DashboardProyectos/RedElectrica/Grafo/GeoJson/Aristas",
  dashboardBaseUrl,
);

const touchDistanceKm = 0.5;
const nearDistanceKm = 1.5;
const reviewDistanceKm = 5;

function normalizeName(value) {
  return String(value ?? "")
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .toUpperCase()
    .replace(/^(?:CFE\s+)?(?:S\s*E|SET|SUBESTACION(?:\s+ELECTRICA)?)\s+/u, "")
    .replace(/^LT\s+/u, "")
    .replace(/\bPOT\b/gu, "POTENCIA")
    .replace(/\bUNO\b|\bI\b/gu, "1")
    .replace(/\bDOS\b|\bII\b/gu, "2")
    .replace(/\bTRES\b|\bIII\b/gu, "3")
    .replace(/\bCUATRO\b|\bIV\b/gu, "4")
    .replace(/[^A-Z0-9]+/gu, " ")
    .replace(/\s+/gu, " ")
    .trim();
}

function significantTokens(value) {
  const ignored = new Set([
    "DE",
    "DEL",
    "LA",
    "EL",
    "LOS",
    "LAS",
    "Y",
    "CFE",
    "SE",
    "LT",
  ]);
  return normalizeName(value)
    .split(" ")
    .filter((token) => token.length > 1 && !ignored.has(token));
}

function nameEvidence(stationName, line) {
  const station = normalizeName(stationName);
  const endpoints = [
    normalizeName(line.properties.from_name),
    normalizeName(line.properties.to_name),
  ].filter(Boolean);
  if (endpoints.includes(station)) {
    return {
      matches: true,
      rule: "nombre_extremo_exacto",
    };
  }

  const stationTokens = significantTokens(station);
  const lineTokens = new Set([
    ...significantTokens(line.properties.name),
    ...significantTokens(line.properties.from_name),
    ...significantTokens(line.properties.to_name),
  ]);
  const tokenMatches =
    stationTokens.length > 0 &&
    stationTokens.every((token) => lineTokens.has(token));
  return tokenMatches
    ? { matches: true, rule: "nombre_contenido_en_linea" }
    : { matches: false, rule: "sin_coincidencia_nominal" };
}

function pointToSegmentDistanceKm(point, start, end) {
  const latitudeRadians = (point.latitude * Math.PI) / 180;
  const longitudeScale = 111.32 * Math.cos(latitudeRadians);
  const latitudeScale = 110.574;
  const startX = (start[0] - point.longitude) * longitudeScale;
  const startY = (start[1] - point.latitude) * latitudeScale;
  const endX = (end[0] - point.longitude) * longitudeScale;
  const endY = (end[1] - point.latitude) * latitudeScale;
  const deltaX = endX - startX;
  const deltaY = endY - startY;
  const denominator = deltaX * deltaX + deltaY * deltaY;
  const fraction =
    denominator === 0
      ? 0
      : Math.max(
          0,
          Math.min(1, -(startX * deltaX + startY * deltaY) / denominator),
        );
  const closestX = startX + fraction * deltaX;
  const closestY = startY + fraction * deltaY;
  return Math.hypot(closestX, closestY);
}

function geometrySegments(geometry) {
  const lines =
    geometry?.type === "LineString"
      ? [geometry.coordinates]
      : geometry?.type === "MultiLineString"
        ? geometry.coordinates
        : [];
  const output = [];
  for (const coordinates of lines) {
    for (let index = 1; index < coordinates.length; index += 1) {
      output.push([coordinates[index - 1], coordinates[index]]);
    }
  }
  return output;
}

function minimumDistanceKm(point, feature) {
  let distance = Number.POSITIVE_INFINITY;
  for (const [start, end] of geometrySegments(feature.geometry)) {
    distance = Math.min(
      distance,
      pointToSegmentDistanceKm(point, start, end),
    );
  }
  return distance;
}

function collapseLineSegments(candidates) {
  const byLine = new Map();
  for (const candidate of candidates) {
    const key = candidate.catalogElementKey || candidate.edgeId;
    const current = byLine.get(key);
    if (!current) {
      byLine.set(key, {
        ...candidate,
        segmentEdgeIds: [candidate.edgeId],
      });
      continue;
    }
    current.segmentEdgeIds.push(candidate.edgeId);
    if (candidate.distanceKm < current.distanceKm) {
      const segmentEdgeIds = current.segmentEdgeIds;
      Object.assign(current, candidate, { segmentEdgeIds });
    }
  }
  return [...byLine.values()];
}

function csvCell(value) {
  const text = String(value ?? "");
  return /[",\r\n]/u.test(text) ? `"${text.replaceAll('"', '""')}"` : text;
}

async function fetchJson(url) {
  const response = await fetch(url, {
    headers: {
      Accept: "application/json",
      "User-Agent": "DGMESNIE-Substation-Line-Audit/1.0",
    },
  });
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}: ${url}`);
  }
  return response.json();
}

const [stationsGeoJson, linesGeoJson] = await Promise.all([
  fetchJson(stationsUrl),
  fetchJson(linesUrl),
]);

const connectedCanonicalNodes = new Set();
for (const line of linesGeoJson.features ?? []) {
  if (line.properties?.from_id) {
    connectedCanonicalNodes.add(String(line.properties.from_id));
  }
  if (line.properties?.to_id) {
    connectedCanonicalNodes.add(String(line.properties.to_id));
  }
}
const stationFeatures = (stationsGeoJson.features ?? []).filter(
  (feature) => {
    const properties = feature.properties ?? {};
    if (targetScope === "unconnected") {
      const canonicalConnected =
        properties.node_id &&
        connectedCanonicalNodes.has(String(properties.node_id));
      const promotedConnected =
        Number(properties.connection_count || 0) > 0;
      return !canonicalConnected && !promotedConnected;
    }
    return (
      properties.validation_state ===
      "validada_automatica_fuente_abierta"
    );
  },
);
const stations = stationFeatures
  .map((feature) => ({
    recordKey: feature.properties.record_key,
    universeKey: feature.properties.universe_key,
    name: feature.properties.name,
    voltageKv: Number(feature.properties.voltage_kv),
    source: feature.properties.source,
    coordinateSource: feature.properties.coordinate_source,
    reconciliationState: feature.properties.reconciliation_state,
    validationState: feature.properties.validation_state,
    latitude: Number(feature.geometry.coordinates[1]),
    longitude: Number(feature.geometry.coordinates[0]),
  }));
const lines = (linesGeoJson.features ?? []).filter((feature) =>
  [230, 400].includes(Number(feature.properties?.voltage_kv)),
);

const results = stations.map((station) => {
  const segmentCandidates = lines
    .filter(
      (line) =>
        Number(line.properties.voltage_kv) === station.voltageKv,
    )
    .map((line) => {
      const distanceKm = minimumDistanceKm(station, line);
      const names = nameEvidence(station.name, line);
      return {
        edgeId: line.properties.edge_id,
        catalogElementKey: line.properties.catalog_element_key,
        name: line.properties.name,
        fromId: line.properties.from_id,
        toId: line.properties.to_id,
        fromName: line.properties.from_name,
        toName: line.properties.to_name,
        voltageKv: Number(line.properties.voltage_kv),
        distanceKm: Number(distanceKm.toFixed(4)),
        connectionState: line.properties.connection_state,
        nameMatch: names.matches,
        nameRule: names.rule,
      };
    })
    .filter((candidate) => candidate.distanceKm <= reviewDistanceKm);
  const candidates = collapseLineSegments(segmentCandidates)
    .sort(
      (left, right) =>
        left.distanceKm - right.distanceKm ||
        Number(right.nameMatch) - Number(left.nameMatch) ||
        left.edgeId.localeCompare(right.edgeId),
    );

  const touching = candidates.filter(
    (candidate) => candidate.distanceKm <= touchDistanceKm,
  );
  const near = candidates.filter(
    (candidate) =>
      candidate.distanceKm > touchDistanceKm &&
      candidate.distanceKm <= nearDistanceKm,
  );
  const matchingName = candidates.filter(
    (candidate) =>
      candidate.distanceKm <= nearDistanceKm && candidate.nameMatch,
  );

  let proposalState = "sin_linea_cercana";
  if (matchingName.length > 0)
    proposalState = "conexion_nombre_tension_geometria";
  else if (touching.length > 0)
    proposalState = "linea_toca_geometricamente";
  else if (near.length > 0)
    proposalState = "linea_cercana_misma_tension";
  else if (candidates.length > 0)
    proposalState = "revision_espacial";

  return {
    ...station,
    proposalState,
    touchingLines: touching.length,
    nearbyLines: near.length,
    matchingNameLines: matchingName.length,
    closestDistanceKm: candidates[0]?.distanceKm ?? null,
    candidates,
  };
});

const stateCounts = Object.fromEntries(
  [...new Set(results.map((result) => result.proposalState))]
    .sort()
    .map((state) => [
      state,
      results.filter((result) => result.proposalState === state).length,
    ]),
);
const generatedUtc = new Date().toISOString();
const report = {
  generatedUtc,
  rule: "DGMESNIE-SUBSTATION-LINE-TOUCH-DRYRUN-v1",
  targetScope,
  mutatesGraph: false,
  thresholdsKm: {
    touch: touchDistanceKm,
    near: nearDistanceKm,
    review: reviewDistanceKm,
  },
  sources: {
    substations: stationsUrl.toString(),
    lines: linesUrl.toString(),
  },
  summary: {
    targetSubstations: stations.length,
    promotedSubstations:
      targetScope === "promoted" ? stations.length : undefined,
    transmissionLineFeatures230Or400Kv: lines.length,
    transmissionAssets230Or400Kv: new Set(
      lines.map(
        (line) =>
          line.properties.catalog_element_key ??
          line.properties.edge_id,
      ),
    ).size,
    stationsWithTouchingLine: results.filter(
      (result) => result.touchingLines > 0,
    ).length,
    stationsWithNameVoltageGeometry: results.filter(
      (result) => result.matchingNameLines > 0,
    ).length,
    candidateAssociationsWithin500m: results.reduce(
      (total, result) => total + result.touchingLines,
      0,
    ),
    ...stateCounts,
  },
  results,
};

await mkdir(outputDirectory, { recursive: true });
const stamp = generatedUtc.replace(/[-:]/gu, "").replace(/\.\d{3}Z$/u, "Z");
const jsonPath = path.join(
  outputDirectory,
  `subestaciones-lineas-dry-run-${stamp}.json`,
);
const csvPath = path.join(
  outputDirectory,
  `subestaciones-lineas-dry-run-${stamp}.csv`,
);
await writeFile(jsonPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");

const header = [
  "universo_clave",
  "subestacion",
  "tension_kv",
  "estado_propuesta",
  "lineas_que_tocan_500m",
  "lineas_cercanas_1500m",
  "lineas_con_nombre",
  "distancia_minima_km",
  "linea_mas_cercana",
  "linea_id",
  "desde",
  "hasta",
  "coincidencia_nombre",
];
const rows = results.map((result) => {
  const candidate = result.candidates[0] ?? {};
  return [
    result.universeKey,
    result.name,
    result.voltageKv,
    result.proposalState,
    result.touchingLines,
    result.nearbyLines,
    result.matchingNameLines,
    result.closestDistanceKm,
    candidate.name,
    candidate.edgeId,
    candidate.fromName,
    candidate.toName,
    candidate.nameMatch,
  ]
    .map(csvCell)
    .join(",");
});
await writeFile(
  csvPath,
  `${header.join(",")}\n${rows.join("\n")}\n`,
  "utf8",
);

console.log(
  JSON.stringify(
    {
      ...report.summary,
      jsonPath,
      csvPath,
    },
    null,
    2,
  ),
);
