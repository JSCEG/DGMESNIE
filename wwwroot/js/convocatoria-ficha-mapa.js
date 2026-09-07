(function () {
  'use strict';
  const config = window.convocatoriaFichaMap;
  if (!config) return;
  const projectPromise = window.ConvocatoriaGeometria.load(config);
  window.pamFichaTerritorialSource = () => projectPromise;
  let map, features, analysisReady, mapReady;
  const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const recordKey = feature => JSON.stringify(feature,(_,value)=>value && typeof value==='object' && !Array.isArray(value)
    ? Object.fromEntries(Object.keys(value).sort().map(key=>[key,value[key]])) : value);
  function status(message) {
    document.querySelectorAll('[data-convocatoria-geometry-status]').forEach(node => node.textContent = message);
  }
  function fit() {
    if (!map) return;
    map.invalidateSize({ animate: false, pan: false });
    if (features.getBounds().isValid()) map.fitBounds(features.getBounds(), { padding: [24,24], maxZoom: 13, animate: false });
  }
  function initMap() {
    if (mapReady) return mapReady;
    mapReady = projectPromise.then(project => {
      const container = document.getElementById('convocatoria-ficha-map');
      if (!container) return;
      if (!project.ubicaciones.length) {
        container.textContent = 'Sin geometría registrada para este proyecto.';
        status(project.advertencias.join(' '));
        return;
      }
      map = L.map(container, { preferCanvas:true, scrollWheelZoom:false, zoomAnimation:false, fadeAnimation:false }).setView([23.6,-102],5);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom:19, crossOrigin:'anonymous', attribution:'&copy; OpenStreetMap contributors'
      }).addTo(map);
      features = L.featureGroup().addTo(map);
      project.ubicaciones.forEach(location => {
        const color = location.etiqueta.startsWith('Proyecto') ? '#0e735c' : '#9b2247';
        L.geoJSON({ type:'Feature', geometry:location.geometria, properties:{} }, {
          style:{color,weight:3,fillColor:color,fillOpacity:.16},
          pointToLayer:(_,ll) => L.circleMarker(ll,{radius:7,color:'#fff',weight:2,fillColor:color,fillOpacity:1})
        }).bindPopup('<strong>'+esc(location.etiqueta)+'</strong><br>'+esc(location.precisionUbicacion)).addTo(features);
      });
      // Origen de cada geometría (KML del corte, trazo reconstruido, polígono de vértices o coordenada).
      const origins = [...new Map(project.ubicaciones.map(location => [location.etiqueta.split(' · ')[0], location.precisionUbicacion])).entries()]
        .map(([name, precision]) => name + ': ' + precision).join(' · ');
      status((project.advertencias.length ? project.advertencias.join(' ') + ' ' : '') + origins + ' · OpenStreetMap');
      fit();
    });
    return mapReady;
  }
  function startAnalysis() {
    if (!analysisReady) analysisReady = initMap().then(() => window.pamFichaTerritorial()).catch(error => {
      const evidence = document.querySelector('[data-convocatoria-analysis-evidence]');
      if (evidence) evidence.textContent = 'No fue posible completar el análisis geoespacial.';
      throw error;
    });
    return analysisReady;
  }
  document.addEventListener('convocatoria:territorial-ready', event => {
    const result = event.detail.resultado;
    const sources = Object.values({...result.capas,...event.detail.fuentes}).filter(source => !source.omitida);
    const missing = sources.filter(source => !source.disponible);
    const warnings = result.proyecto.advertencias || [];
    const partial = missing.length || warnings.length || !result.elementos.length;
    const inegiMissing = result.elementos.filter(element => !elementoInegi(element)).length;
    const extraContainer = document.querySelector('[data-convocatoria-extra-analysis]');
    if (extraContainer) {
      const keys = ['nucleosAgrarios','atlasIndigena','lenguasIndigenas','localidadesIndigenas','rutaWixarika','divisionesTarifarias','generacionDistribuida'];
      extraContainer.innerHTML = keys.map(key => {
        const source = event.detail.fuentes[key];
        // Una misma geometría de origen puede coincidir con proyecto y conexión; contarla una vez.
        const candidates = result.elementos.flatMap(element => (element.intensivo?.[key] || []).map(item => item.feature));
        const items = [...new Map(candidates.map(feature=>[recordKey(feature),feature])).values()];
        const names = [...new Set(items.map(feature => {
          const p = feature.properties || {}, k = Object.keys(p).find(k => /^(nombre|name|nomgeo|nom_nuc|nom_loc|localidad|divisi[oó]n)$/i.test(k)) || Object.keys(p).find(k => /nombre|nom_nuc|nom_loc/i.test(k)) || Object.keys(p).find(k => /municipio/i.test(k));
          return k ? String(p[k]) : '';
        }).filter(Boolean))].slice(0,4);
        let detail = names.join(' · ') || (items.length?'Registros coincidentes sin nombre.':'Sin coincidencias en el área de análisis.');
        if (key === 'generacionDistribuida' && items.length) detail = items.map(feature => {
          const p = feature.properties || {};
          return (p.nombre || 'Estado')+': '+(p.dg_mw == null ? 'MW no disponibles' : Number(p.dg_mw).toLocaleString('es-MX')+' MW estatales')+' · '+p.reference_period;
        }).join(' · ');
        return '<article><span>'+esc(source?.nombre || key)+'</span><strong>'+(!result.elementos.length?'Pendiente':!source?.disponible?'No disponible':items.length)+'</strong><p>'+esc(!result.elementos.length?'Sin geometría disponible.':!source?.disponible?'Fuente no disponible en la consulta.':detail)+'</p><small>'+esc(key === 'generacionDistribuida'?'Totales estatales de generación distribuida.':key === 'divisionesTarifarias'?'Divisiones dentro del área de análisis.':'Registros dentro del área de análisis de 20 km.')+'</small></article>';
      }).join('');
    }
    const evidence = document.querySelector('[data-convocatoria-analysis-evidence]');
    if (evidence) evidence.innerHTML =
      '<p><strong>'+ (partial || inegiMissing ? 'Análisis con fuentes no disponibles' : 'Análisis ejecutado')+'</strong> · '+result.elementos.length+' geometría(s) · '+sources.filter(s=>s.disponible).length+' de '+sources.length+' fuentes consultadas · '+esc(new Date().toLocaleString('es-MX'))+'</p>'+
      '<div class="convocatoria-source-grid">'+sources.map(source => '<div class="'+(source.disponible?'':'is-missing')+'"><span>'+esc(source.nombre)+'</span><strong>'+(source.disponible?'Consultada':'No disponible')+'</strong></div>').join('')+'</div>'+
      '<p>'+esc(warnings.join(' '))+' Contexto municipal INEGI: '+(!result.elementos.length?'sin geometría disponible.':inegiMissing?inegiMissing+' ubicación(es) sin contexto municipal.':'consultado para las ubicaciones identificadas.')+'</p>'+
      '<p>Área de análisis: 20 km alrededor de las geometrías registradas en la ventanilla. Los conteos corresponden a registros únicos de cada fuente.</p>'+
      '<p>Análisis complementario elaborado por la SENER; no sustituye los dictámenes de las autoridades competentes.</p>';
    document.querySelectorAll('[data-territorial-status]').forEach(node => {
      node.textContent = (partial || inegiMissing ? 'Fuentes no disponibles · ' : 'Ejecutado · ')+result.elementos.length+' geometrías · '+sources.filter(s=>s.disponible).length+'/'+sources.length+' fuentes';
    });
    document.querySelector('.convocatoria-ficha-page').dataset.analysisState = partial || inegiMissing ? 'partial' : 'complete';
    function elementoInegi(element) { return element.inegi; }
  });
  window.convocatoriaFichaPreparar = async slide => {
    await startAnalysis();
    if (slide.querySelector('#convocatoria-ficha-map')) {
      await initMap();
      fit();
      await new Promise(resolve => setTimeout(resolve, 200));
    }
    if (slide.querySelector('[data-convocatoria-analysis-evidence]')) await window.pamFichaTerritorial();
  };
  document.addEventListener('pam:slide-shown', () => {
    if (document.querySelector('.is-active #convocatoria-ficha-map')) initMap().then(() => requestAnimationFrame(fit));
  });
  function start() { startAnalysis().catch(error => console.error('Análisis Mixtos II', error)); }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
  else start();
})();
