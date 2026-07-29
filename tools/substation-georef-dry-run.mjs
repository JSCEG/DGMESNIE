import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";
import process from "node:process";

const dashboardBaseUrl =
  process.env.DGMESNIE_DASHBOARD_URL ?? "http://127.0.0.1:5155";
const overpassUrls = process.env.DGMESNIE_OVERPASS_URL
  ? [process.env.DGMESNIE_OVERPASS_URL]
  : [
      "https://overpass-api.de/api/interpreter",
      "https://overpass.kumi.systems/api/interpreter",
    ];
const outputDirectory =
  process.env.DGMESNIE_GEOREF_OUTPUT ??
  path.resolve("out", "red-electrica", "georreferenciacion");
const gcrUrl =
  process.env.DGMESNIE_GCR_URL ??
  "https://cdn.sassoapps.com/Mapas/gerencias_javs_2.geojson";

const pendingStates = new Set([
  "pendiente_georreferenciacion",
  "coincidencia_ambigua",
]);

const inventoryUrl = new URL(
  "/DashboardProyectos/RedElectrica/InventarioSubestaciones/Registros",
  dashboardBaseUrl,
);
inventoryUrl.searchParams.set("source", "atlas_sen");
inventoryUrl.searchParams.set("limit", "5000");

const overpassQuery = `
[out:json][timeout:180];
nwr
  ["power"="substation"]
  ["voltage"~"230000|400000"]
  (14,-119,33,-86);
out center tags;
`.trim();

const stopWords = new Set([
  "CFE",
  "DE",
  "DEL",
  "LA",
  "EL",
  "LOS",
  "LAS",
  "Y",
  "EN",
  "SUBESTACION",
  "ELECTRICA",
  "SE",
  "SET",
]);

const ordinalTokens = new Map([
  ["UNO", "1"],
  ["UN", "1"],
  ["I", "1"],
  ["DOS", "2"],
  ["II", "2"],
  ["TRES", "3"],
  ["III", "3"],
  ["CUATRO", "4"],
  ["IV", "4"],
  ["CINCO", "5"],
  ["V", "5"],
]);

function normalizeName(value, voltageKv = null) {
  let normalized = String(value ?? "")
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .toUpperCase()
    .replace(/^(?:CFE\s+)?(?:S\s*E|SET|SUBESTACION(?:\s+ELECTRICA)?)\s+/u, "")
    .replace(/\s+DE\s+LA\s+CFE$/u, "")
    .replace(/\bPOT\b/gu, "POTENCIA")
    .replace(/[^A-Z0-9]+/gu, " ")
    .replace(/\s+/gu, " ")
    .trim();

  if (voltageKv) {
    const voltageToken = String(Math.round(voltageKv));
    normalized = normalized.replace(
      new RegExp(`(?:^|\\s)${voltageToken}(?:\\s*KV)?$`, "u"),
      "",
    );
  }
  normalized = normalized
    .replace(/\b\d+\s*X\s*\d+(?:\.\d+)?\b/gu, "")
    .replace(/\s+/gu, " ")
    .trim();

  return normalized
    .split(" ")
    .map((token) => ordinalTokens.get(token) ?? token)
    .join(" ");
}

function significantTokens(value) {
  return normalizeName(value)
    .split(" ")
    .filter((token) => token.length > 1 && !stopWords.has(token));
}

function functionalName(value) {
  return value
    .replace(/^(?:C C|C H|C T|CENTRAL)\s+/u, "")
    .replace(/\s+/gu, " ")
    .trim();
}

function explicitOrdinal(value) {
  const tokens = value.split(" ");
  const last = tokens.at(-1) ?? "";
  return /^[1-5]$/u.test(last) ? last : "";
}

function jaccard(left, right) {
  const a = new Set(significantTokens(left));
  const b = new Set(significantTokens(right));
  if (!a.size || !b.size) return 0;
  let intersection = 0;
  for (const token of a) {
    if (b.has(token)) intersection += 1;
  }
  return intersection / (a.size + b.size - intersection);
}

function levenshtein(left, right) {
  if (!left.length) return right.length;
  if (!right.length) return left.length;
  let previous = Array.from({ length: right.length + 1 }, (_, index) => index);
  for (let row = 1; row <= left.length; row += 1) {
    const current = [row];
    for (let column = 1; column <= right.length; column += 1) {
      current[column] = Math.min(
        current[column - 1] + 1,
        previous[column] + 1,
        previous[column - 1] +
          (left[row - 1] === right[column - 1] ? 0 : 1),
      );
    }
    previous = current;
  }
  return previous[right.length];
}

function nameEvidence(target, candidate) {
  const declared = normalizeName(target.name, target.voltageKv);
  const catalog = normalizeName(candidate.name, target.voltageKv);
  if (!declared || !catalog) {
    return { score: 0, rule: "sin_nombre", similarity: 0 };
  }
  if (declared === catalog) {
    return { score: 72, rule: "nombre_normalizado_exacto", similarity: 1 };
  }

  const declaredOrdinal = explicitOrdinal(declared);
  const catalogOrdinal = explicitOrdinal(catalog);
  if (
    declaredOrdinal &&
    catalogOrdinal &&
    declaredOrdinal !== catalogOrdinal
  ) {
    return {
      score: 0,
      rule: "conflicto_numero_unidad",
      similarity: 0,
    };
  }

  const declaredFunctional = functionalName(declared);
  const catalogFunctional = functionalName(catalog);
  if (
    declaredFunctional &&
    declaredFunctional === catalogFunctional &&
    (declaredFunctional !== declared || catalogFunctional !== catalog)
  ) {
    return {
      score: 58,
      rule: "calificador_central_controlado",
      similarity: 1,
    };
  }

  const tokenScore = jaccard(declared, catalog);
  const distance = levenshtein(declared, catalog);
  const editScore =
    1 - distance / Math.max(declared.length, catalog.length, 1);
  const containment =
    declared.length >= 5 &&
    catalog.length >= 5 &&
    (declared.includes(catalog) || catalog.includes(declared));

  if (tokenScore >= 0.8 && editScore >= 0.8) {
    return {
      score: 58,
      rule: "tokens_y_ortografia",
      similarity: Math.max(tokenScore, editScore),
    };
  }
  if (containment && tokenScore >= 0.5) {
    return {
      score: 52,
      rule: "nombre_extendido",
      similarity: Math.max(tokenScore, editScore),
    };
  }
  if (editScore >= 0.88) {
    return {
      score: 50,
      rule: "variante_ortografica",
      similarity: editScore,
    };
  }
  return {
    score: 0,
    rule: "sin_coincidencia_nominal",
    similarity: Math.max(tokenScore, editScore),
  };
}

function parseVoltages(raw) {
  return String(raw ?? "")
    .split(/[;,/]/u)
    .map((value) => Number(value.trim()))
    .filter((value) => Number.isFinite(value) && value > 0)
    .map((value) => (value > 1000 ? value / 1000 : value));
}

function elementCoordinates(element) {
  const latitude = element.lat ?? element.center?.lat;
  const longitude = element.lon ?? element.center?.lon;
  return Number.isFinite(latitude) && Number.isFinite(longitude)
    ? { latitude, longitude }
    : null;
}

function distanceKm(left, right) {
  const radians = (degrees) => (degrees * Math.PI) / 180;
  const latitudeDelta = radians(right.latitude - left.latitude);
  const longitudeDelta = radians(right.longitude - left.longitude);
  const firstLatitude = radians(left.latitude);
  const secondLatitude = radians(right.latitude);
  const a =
    Math.sin(latitudeDelta / 2) ** 2 +
    Math.cos(firstLatitude) *
      Math.cos(secondLatitude) *
      Math.sin(longitudeDelta / 2) ** 2;
  return 6371 * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function pointInRing(longitude, latitude, ring) {
  let inside = false;
  for (
    let current = 0, previous = ring.length - 1;
    current < ring.length;
    previous = current, current += 1
  ) {
    const [currentLongitude, currentLatitude] = ring[current];
    const [previousLongitude, previousLatitude] = ring[previous];
    const intersects =
      currentLatitude > latitude !== previousLatitude > latitude &&
      longitude <
        ((previousLongitude - currentLongitude) *
          (latitude - currentLatitude)) /
          (previousLatitude - currentLatitude || Number.EPSILON) +
          currentLongitude;
    if (intersects) inside = !inside;
  }
  return inside;
}

function pointInPolygon(longitude, latitude, polygon) {
  if (!polygon.length || !pointInRing(longitude, latitude, polygon[0])) {
    return false;
  }
  return !polygon
    .slice(1)
    .some((hole) => pointInRing(longitude, latitude, hole));
}

function pointInGeometry(longitude, latitude, geometry) {
  if (geometry?.type === "Polygon") {
    return pointInPolygon(longitude, latitude, geometry.coordinates);
  }
  if (geometry?.type === "MultiPolygon") {
    return geometry.coordinates.some((polygon) =>
      pointInPolygon(longitude, latitude, polygon),
    );
  }
  return false;
}

function normalizeRegion(value) {
  return String(value ?? "")
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .toUpperCase()
    .replace(/^GERENCIA DE CONTROL REGIONAL\s+/u, "")
    .replace(/[^A-Z0-9]+/gu, " ")
    .trim();
}

function resolveRegion(candidate, gcrFeatures) {
  const feature = gcrFeatures.find((item) =>
    pointInGeometry(
      candidate.longitude,
      candidate.latitude,
      item.geometry,
    ),
  );
  const rawName =
    feature?.properties?.Field1 ??
    feature?.properties?.nombre ??
    feature?.properties?.name ??
    "";
  return {
    raw: rawName,
    normalized: normalizeRegion(rawName),
  };
}

function collapsePhysicalCandidates(candidates) {
  const output = [];
  for (const candidate of candidates) {
    const duplicate = output.find(
      (existing) =>
        existing.normalizedName === candidate.normalizedName &&
        distanceKm(existing, candidate) <= 0.25,
    );
    if (!duplicate) {
      output.push({ ...candidate, alternateReferences: [] });
      continue;
    }
    duplicate.alternateReferences.push(candidate.referenceKey);
    if (candidate.score > duplicate.score) {
      const alternateReferences = duplicate.alternateReferences;
      Object.assign(duplicate, candidate, { alternateReferences });
    }
  }
  return output;
}

function scoreCandidate(target, candidate) {
  const name = nameEvidence(target, candidate);
  if (!name.score) return null;

  const voltages = parseVoltages(candidate.voltageRaw);
  const voltageMatch =
    target.voltageKv == null ||
    voltages.some((voltage) => Math.abs(voltage - target.voltageKv) <= 0.5);
  const voltageScore =
    target.voltageKv == null || !voltages.length ? 5 : voltageMatch ? 20 : -30;
  const operatorScore = /CFE|COMISION FEDERAL/iu.test(candidate.operator)
    ? 5
    : 0;
  const targetRegion = normalizeRegion(target.region);
  const candidateRegion = normalizeRegion(candidate.region);
  const regionMatch =
    targetRegion && candidateRegion
      ? targetRegion === candidateRegion
      : null;
  const regionScore = regionMatch === true ? 8 : regionMatch === false ? -25 : 0;
  const score = Math.max(
    0,
    Math.min(
      100,
      name.score + voltageScore + operatorScore + regionScore,
    ),
  );

  return {
    sourceKey: "openstreetmap",
    referenceKey: candidate.referenceKey,
    name: candidate.name,
    normalizedName: normalizeName(candidate.name, target.voltageKv),
    latitude: candidate.latitude,
    longitude: candidate.longitude,
    voltageKv: voltages.length ? Math.max(...voltages) : null,
    voltagesKv: voltages,
    operator: candidate.operator,
    region: candidate.region,
    score,
    evidence: {
      nameRule: name.rule,
      nameSimilarity: Number(name.similarity.toFixed(4)),
      voltageMatch,
      operatorCfe: operatorScore > 0,
      regionMatch,
    },
  };
}

function csvCell(value) {
  const text = String(value ?? "");
  return /[",\r\n]/u.test(text) ? `"${text.replaceAll('"', '""')}"` : text;
}

async function fetchJson(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      Accept: "application/json",
      "User-Agent": "DGMESNIE-Substation-Audit/1.0",
      ...options.headers,
    },
  });
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}: ${url}`);
  }
  return response.json();
}

const inventory = await fetchJson(inventoryUrl);
const targets = inventory.filter(
  (record) =>
    pendingStates.has(record.reconciliationState) &&
    record.latitude == null &&
    record.longitude == null,
);
const gcr = await fetchJson(gcrUrl);
const gcrFeatures = gcr.features ?? [];

let overpass = null;
let selectedOverpassUrl = "";
let lastOverpassError = null;
for (const endpoint of overpassUrls) {
  const overpassRequest = new URL(endpoint);
  overpassRequest.searchParams.set("data", overpassQuery);
  try {
    overpass = await fetchJson(overpassRequest);
    selectedOverpassUrl = endpoint;
    break;
  } catch (error) {
    lastOverpassError = error;
  }
}
if (!overpass) {
  throw lastOverpassError ?? new Error("Overpass no disponible");
}
const osmCandidates = (overpass.elements ?? [])
  .map((element) => {
    const coordinates = elementCoordinates(element);
    const name =
      element.tags?.name ??
      element.tags?.["name:es"] ??
      element.tags?.ref ??
      "";
    if (!coordinates || !name) return null;
    const candidate = {
      referenceKey: `${element.type}/${element.id}`,
      name,
      latitude: coordinates.latitude,
      longitude: coordinates.longitude,
      voltageRaw: element.tags?.voltage ?? "",
      operator: element.tags?.operator ?? "",
    };
    const region = resolveRegion(candidate, gcrFeatures);
    return {
      ...candidate,
      region: region.raw,
      normalizedRegion: region.normalized,
    };
  })
  .filter(Boolean);

const results = targets.map((target) => {
  const scoredCandidates = osmCandidates
    .map((candidate) => scoreCandidate(target, candidate))
    .filter(Boolean)
    .sort(
      (left, right) =>
        right.score - left.score ||
        right.evidence.nameSimilarity - left.evidence.nameSimilarity ||
        left.referenceKey.localeCompare(right.referenceKey),
    );
  const candidates = collapsePhysicalCandidates(scoredCandidates)
    .sort(
      (left, right) =>
        right.score - left.score ||
        right.evidence.nameSimilarity - left.evidence.nameSimilarity ||
        left.referenceKey.localeCompare(right.referenceKey),
    )
    .slice(0, 5);

  const first = candidates[0] ?? null;
  const second = candidates[1] ?? null;
  const margin = first ? first.score - (second?.score ?? 0) : 0;
  const ambiguous =
    first != null &&
    second != null &&
    margin < 8 &&
    second.score >= 70;
  const highConfidence =
    first?.score >= 90 &&
    margin >= 8 &&
    first.evidence.nameRule === "nombre_normalizado_exacto" &&
    first.evidence.voltageMatch === true &&
    first.evidence.regionMatch === true;
  let proposalState = "sin_coincidencia";
  if (ambiguous) proposalState = "coincidencia_ambigua";
  else if (highConfidence) proposalState = "propuesta_alta_confianza";
  else if (first?.score >= 75) proposalState = "propuesta_revision";
  else if (first) proposalState = "coincidencia_debil";

  return {
    recordKey: target.recordKey,
    universeKey: target.universeKey,
    name: target.name,
    voltageKv: target.voltageKv,
    region: target.region,
    zone: target.zone,
    targetIdentityKey: [
      normalizeName(target.name, target.voltageKv),
      target.voltageKv ?? "",
      normalizeName(target.zone),
    ].join("|"),
    currentReconciliationState: target.reconciliationState,
    proposalState,
    score: first?.score ?? 0,
    margin,
    selectedCandidate: ambiguous ? null : first,
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
const highConfidenceResults = results.filter(
  (result) => result.proposalState === "propuesta_alta_confianza",
);
const generatedUtc = new Date().toISOString();
const report = {
  generatedUtc,
  rule: "DGMESNIE-SUBSTATION-GEOREF-DRYRUN-v1",
  mutatesInventory: false,
  sources: {
    inventory: inventoryUrl.toString(),
    openStreetMap: selectedOverpassUrl,
    gerenciasControl:
      "https://cdn.sassoapps.com/Mapas/gerencias_javs_2.geojson",
    license: "ODbL 1.0",
    overpassQuery,
  },
  summary: {
    inventoryAtlasRecords: inventory.length,
    targetRecords: targets.length,
    uniqueTargetIdentities: new Set(
      results.map((result) => result.targetIdentityKey),
    ).size,
    osmElements: overpass.elements?.length ?? 0,
    namedOsmCandidates: osmCandidates.length,
    highConfidenceDistinctOsmFeatures: new Set(
      highConfidenceResults.map(
        (result) => result.selectedCandidate?.referenceKey,
      ),
    ).size,
    ...stateCounts,
  },
  results,
};

await mkdir(outputDirectory, { recursive: true });
const stamp = generatedUtc.replace(/[-:]/gu, "").replace(/\.\d{3}Z$/u, "Z");
const jsonPath = path.join(
  outputDirectory,
  `subestaciones-georef-dry-run-${stamp}.json`,
);
const csvPath = path.join(
  outputDirectory,
  `subestaciones-georef-dry-run-${stamp}.csv`,
);
await writeFile(jsonPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");

const csvHeader = [
  "registro_clave",
  "nombre_atlas",
  "tension_kv",
  "region",
  "zona",
  "estado_propuesta",
  "puntaje",
  "margen",
  "nombre_osm",
  "latitud",
  "longitud",
  "referencia_osm",
  "evidencia",
];
const csvRows = results.map((result) => {
  const candidate = result.selectedCandidate ?? result.candidates[0] ?? {};
  return [
    result.recordKey,
    result.name,
    result.voltageKv,
    result.region,
    result.zone,
    result.proposalState,
    result.score,
    result.margin,
    candidate.name,
    candidate.latitude,
    candidate.longitude,
    candidate.referenceKey,
    candidate.evidence ? JSON.stringify(candidate.evidence) : "",
  ]
    .map(csvCell)
    .join(",");
});
await writeFile(
  csvPath,
  `${csvHeader.join(",")}\n${csvRows.join("\n")}\n`,
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
