(function () {
  'use strict';
  const cache = new Map();
  const point = (lat, lon) => lat != null && lon != null && Number.isFinite(Number(lat)) && Number.isFinite(Number(lon)) && Number(lat) >= 14 && Number(lat) <= 33.5 && Number(lon) >= -119 && Number(lon) <= -86;
  async function kml(url) {
    if (!cache.has(url)) cache.set(url, (async () => {
      const response = await fetch(url, { cache: 'no-store', credentials: 'same-origin', signal: AbortSignal.timeout(45000) });
      if (!response.ok) throw new Error('KML HTTP ' + response.status);
      const xml = new DOMParser().parseFromString(await response.text(), 'text/xml');
      if (xml.querySelector('parsererror')) throw new Error('KML inválido');
      xml.querySelectorAll('name, description').forEach(node => node.remove());
      const layer = new L.KML(xml, { vertexPointThreshold: 50 });
      const geo = layer.toGeoJSON();
      if (!geo.features?.length) throw new Error('KML sin geometría utilizable');
      // Agrupa todos los polígonos y trazos; no descarta componentes ni crea marcadores por vértice.
      return { features: turf.combine(geo).features, reconstructed: !!layer._vertexGeometryReconstructed };
    })().catch(error => { cache.delete(url); throw error; }));
    return cache.get(url);
  }
  async function load(config) {
    const warnings = [], locations = [];
    // Polígono declarado en la solicitud (vértices "lat, lon" del Excel); respaldo cuando el KML falta.
    const polygon = vertices => {
      const ring = (vertices || []).filter(v => Array.isArray(v) && point(v[0], v[1])).map(v => [Number(v[1]), Number(v[0])]);
      if (ring.length < 3) return null;
      const [first, last] = [ring[0], ring[ring.length - 1]];
      if (first[0] !== last[0] || first[1] !== last[1]) ring.push([first[0], first[1]]);
      return { type: 'Polygon', coordinates: [ring] };
    };
    const sources = [
      { name: 'Proyecto', url: config.projectKmlUrl, lat: config.latitud, lon: config.longitud, polygon: polygon(config.expediente?.projectPolygon) },
      { name: 'Subestación / conexión', url: config.substationKmlUrl, lat: config.subestacionLatitud, lon: config.subestacionLongitud, polygon: polygon(config.expediente?.substationPolygon) }
    ];
    await Promise.all(sources.map(async source => {
      let geometries = [], precision = 'Coordenada registrada en la solicitud';
      if (source.url) {
        try {
          const result = await kml(source.url);
          geometries = result.features.map(feature => feature.geometry);
          precision = result.reconstructed ? 'Polígono reconstruido de vértices KML' : 'Geometría KML registrada en ventanilla';
        } catch (error) { warnings.push(source.name + ': KML no disponible; ' + (source.polygon ? 'se usa el polígono de vértices de la solicitud.' : point(source.lat, source.lon) ? 'se usa la coordenada registrada.' : 'sin geometría alternativa.')); }
      }
      if (!geometries.length && source.polygon) { geometries = [source.polygon]; precision = 'Polígono de vértices registrados en la solicitud'; }
      if (!geometries.length && point(source.lat, source.lon)) geometries = [{ type: 'Point', coordinates: [Number(source.lon), Number(source.lat)] }];
      geometries.forEach(geometry => locations.push({
        etiqueta: source.name + ' · ' + config.folio, geometria: geometry,
        entidad: config.entidad || '', municipio: config.municipio || '',
        precisionUbicacion: precision, metodoUbicacion: 'Corte Mixtos II', fuente: config.fuente || 'Mixtos II',
        validada: false, radioSugeridoKm: 20
      }));
    }));
    locations.sort((a,b) => a.etiqueta.localeCompare(b.etiqueta, 'es'));
    // Si la fuente declara la misma geometría para el proyecto y su conexión (coordenada repetida o
    // mismo KML), el análisis se corre una sola vez: dos tarjetas idénticas no aportan información.
    const key = geometry => JSON.stringify(geometry);
    const projectKeys = new Set(locations.filter(l => l.etiqueta.startsWith('Proyecto')).map(l => key(l.geometria)));
    const duplicated = locations.filter(l => !l.etiqueta.startsWith('Proyecto') && projectKeys.has(key(l.geometria)));
    if (duplicated.length) {
      duplicated.forEach(l => locations.splice(locations.indexOf(l), 1));
      locations.filter(l => l.etiqueta.startsWith('Proyecto') && duplicated.some(d => key(d.geometria) === key(l.geometria)))
        .forEach(l => { l.etiqueta = 'Proyecto y conexión · ' + config.folio; l.precisionUbicacion += ' · misma geometría declarada para proyecto y subestación'; });
      warnings.push('La fuente registra la misma ubicación para el proyecto y su subestación; se analiza una sola geometría.');
    }
    if (!locations.length) warnings.push('Sin geometría registrada para el análisis.');
    return { claveProyecto: config.folio, nombreProyecto: config.titulo, gcr: config.gcr, ubicaciones: locations, advertencias: warnings };
  }
  window.ConvocatoriaGeometria = { load, point };
})();
