(function () {
  'use strict';
  const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const fmt = (value, decimals = 0) => {
    const numeric = typeof value === 'boolean' || value == null || String(value).trim() === '' ? NaN : Number(String(value).replace(/,/g,''));
    return !Number.isFinite(numeric) ? 'No disponible' : numeric.toLocaleString('es-MX',{maximumFractionDigits:Math.max(0,Math.min(4,decimals))});
  };
  const normalizedKey = value => String(value).normalize('NFD').replace(/[\u0300-\u036f]/g,'');
  const prop = (feature, pattern, fallback = 'No informado') => {
    const p = feature?.properties || {};
    const key = Object.keys(p).find(key => pattern.test(normalizedKey(key)) && p[key] != null && String(p[key]).trim() !== '' && !/^(NULL|S\/D|N\/A)$/i.test(String(p[key]).trim()));
    return key ? p[key] : fallback;
  };
  // Igual nombre o identificador no implica igual tramo. Sólo se elimina el mismo
  // registro completo, aunque llegue como otro objeto desde proyecto y conexión.
  const stableJson = value => JSON.stringify(value, (_,item) => item && !Array.isArray(item) && typeof item === 'object'
    ? Object.fromEntries(Object.keys(item).sort().map(key=>[key,item[key]])) : item);
  const distinct = items => {
    const seen = new Set(), cache = new WeakMap();
    return items.map(item=>item.feature || item).filter(Boolean).filter(feature=>{
      let key = cache.get(feature);
      if (!key) { key = stableJson(feature); cache.set(feature,key); }
      if (seen.has(key)) return false;
      seen.add(key); return true;
    });
  };

  document.addEventListener('convocatoria:territorial-ready', event => {
    const result = event.detail.resultado, sources = event.detail.fuentes;
    const template = document.getElementById('convocatoria-detail-slide-template');
    let anchor = document.querySelector('[data-convocatoria-analysis-evidence]')?.closest('[data-slide]');
    if (!template || !anchor) return;

    function newSlide(title, context, note, part) {
      const slide = template.content.firstElementChild.cloneNode(true);
      slide.dataset.label = title + (part ? ' · '+part : '');
      const heading = slide.querySelector('[data-detail-title]');
      heading.textContent = slide.dataset.label;
      heading.classList.add('is-md');
      slide.querySelector('[data-detail-context]').textContent = context;
      slide.querySelector('[data-detail-note]').textContent = note;
      slide.style.display = 'flex';
      slide.style.visibility = 'hidden';
      anchor.before(slide);
      return slide;
    }
    function finish(slide) { slide.style.removeProperty('display'); slide.style.removeProperty('visibility'); }
    function tablePages(title, headers, rows, context, note, empty, firstColumnWidth) {
      let slide, body, tbody, part = 0;
      // firstColumnWidth (%) ensancha la columna de identificación cuando lleva nombres largos.
      const colgroup = firstColumnWidth ? '<colgroup><col style="width:'+Number(firstColumnWidth)+'%"></colgroup>' : '';
      function page() {
        slide = newSlide(title,context,note,++part);
        body = slide.querySelector('[data-detail-body]');
        body.innerHTML = '<table>'+colgroup+'<thead><tr>'+headers.map(header=>'<th>'+esc(header)+'</th>').join('')+'</tr></thead><tbody></tbody></table>';
        tbody = body.querySelector('tbody');
      }
      page();
      if (!rows.length) rows = [[empty || 'Sin coincidencias en la cobertura.']];
      // Cifras y cantidades con unidad se alinean a la derecha; texto a la izquierda.
      const isNumeric = cell => /^[\s$€]*-?[\d.,]+\s*(%|MW|MWh|GWh|MDD|MDP|USD|MXN|kV|km|ha|m|años|hab\.?|usuarios)?\s*$/i.test(String(cell ?? '').trim()) && /\d/.test(String(cell ?? ''));
      rows.forEach(row => {
        const tr = document.createElement('tr');
        tr.innerHTML = row.map(cell=>'<td'+(isNumeric(cell)?' class="is-num"':'')+'>'+esc(cell)+'</td>').join('');
        if (row.length === 1) tr.firstElementChild.colSpan = headers.length;
        tbody.append(tr);
        if (tbody.children.length > 1 && (body.scrollHeight > body.clientHeight + 2 || tbody.children.length > 8)) {
          tr.remove(); finish(slide); page(); tbody.append(tr);
        }
      });
      finish(slide);
    }
    function sourceEmpty(source) { return !result.elementos.length?'Sin geometría disponible para el análisis.':!source?.disponible?'Fuente no disponible en la consulta.':'Sin coincidencias en el área de análisis.'; }

    const dossier = window.convocatoriaFichaMap?.expediente;
    if (dossier) {
      const territorialAnchor = anchor;
      const dossierAnchor = document.querySelector('[data-convocatoria-expediente-anchor]') || anchor;
      anchor = document.querySelector('[data-convocatoria-costos-anchor]') || dossierAnchor;
      const records = sheet => (dossier.records || []).filter(r=>r.sheet===sheet);
      const val = (record,name) => record.fields.find(f=>f.name===name)?.value;
      const money = (record,name) => { const v=val(record,name); return v==null || String(v).trim()===''?'No informado':fmt(v,4); };
      const works = records('BD_COSTOS_OBRAS');
      const note = 'Fuente: ficha de costos CFE · Convocatoria Mixtos II.';
      if (works.length) {
        // Cada obra se identifica por su número y nombre; el número solo no dice qué obra es.
        const workLabel = r => { const name = String(val(r,'Nombre de la obra') ?? '').trim(); const short = name.length > 72 ? name.slice(0,72).trimEnd()+'…' : name; return String(val(r,'No.') ?? '')+(short ? ' · '+short : ''); };
        const rate = fmt(dossier.exchangeRate,2)+' MXN/USD';
        const rateNote = ' * Importes convertidos de MXN a USD (y de MDD a MDP) con el tipo de cambio de la ficha CFE, '+rate+', aplicado a todas las obras.';
        tablePages('Desglose de obras',['Obra / fuente','Tipo','Descripción','Responsable'],works.map(r=>[
          String(val(r,'No.') ?? '')+(val(r,'Lamina')?'\nLámina CFE '+val(r,'Lamina'):''),val(r,'Tipo de obra'),val(r,'Nombre de la obra'),val(r,'A cargo del solicitante')==='Sí'?'Solicitante':'No informado'
        ]),'Interconexión y refuerzo',note);
        if (window.convocatoriaFichaMap.conCostos) {
          tablePages('Costos de construcción',['Obra','Obra civil USD','Electromecánica USD','Suministros USD','Puesta en servicio USD','Construcción total USD'],works.map(r=>[
            workLabel(r),...['Obra civil (USD)','Obra electromecánica (USD)','Suministros (USD)','Puesta en servicio (USD)','Total construcción (USD)'].map(n=>money(r,n))
          ]),'Importes en USD conforme a la ficha CFE',note+' Los importes de construcción se reportan en dólares.',undefined,30);
          tablePages('Actividades previas y servicios',['Obra','Previas MXN','CPTT MXN','Previas USD *','CPTT USD *','Total MDD','Total MDP *'],works.map(r=>[
            workLabel(r),...['Actividades previas (MXN)','Servicios CPTT (MXN)','Actividades previas (USD)','Servicios CPTT (USD)','Costo de red de la obra (MDD)','Costo de red de la obra (MDP)'].map(n=>money(r,n))
          ]),'Tipo de cambio de la ficha CFE: '+rate,note+rateNote+' MDP y MDD expresan el mismo costo en pesos y en dólares.',undefined,28);
        }
      }
      const factColumns = new Set(['C','G','K','L','P','Q','R','S','T','U','V','W','X','Y','Z','AA']);
      [['BD_IMPACTO_SOCIAL','Detalle social y ambiental','social',new Set(['I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'])],
       ['BD_FACTIBILIDAD','Evaluación técnica','factibilidad',factColumns]].forEach(([sheet,title,section,columns])=>{
        anchor = document.querySelector('[data-convocatoria-'+section+'-anchor]') || dossierAnchor;
        records(sheet).forEach((record,index)=>{
          const rows = record.fields.filter(f=>columns.has(f.column)).flatMap(field=>{
            const chunks = String(field.value ?? 'No informado').match(/[\s\S]{1,440}/g) || ['No informado'];
            return chunks.map((text,i)=>[field.name+(i?' · continuación':''),text,field.column+record.row]);
          });
          tablePages(title+(records(sheet).length>1?' · registro '+(index+1):''),['Aspecto','Evaluación documental','Referencia'],rows,
            sheet==='BD_IMPACTO_SOCIAL'?'Evaluación social y ambiental':'Evaluación técnica','Fuente: evaluación documental de la Convocatoria Mixtos II.');
        });
      });
      const observationKey = value => String(value ?? '').normalize('NFC').trim().replace(/\s+/g,' ');
      const evaluations = new Set(records('BD_FACTIBILIDAD').flatMap(record=>record.fields.filter(field=>factColumns.has(field.column)).map(field=>observationKey(field.value))).filter(Boolean));
      records('CATALOGO DE PROYECTOS').forEach(record=>{
        const observation = record.fields.find(field=>normalizedKey(field.name).trim().toUpperCase()==='OBSERVACION DEL AREA');
        if (!observationKey(observation?.value) || evaluations.has(observationKey(observation.value))) return;
        anchor = document.querySelector('[data-convocatoria-factibilidad-anchor]') || dossierAnchor;
        const chunks = String(observation.value).match(/[\s\S]{1,440}/g) || [];
        tablePages('Observación del área',['Aspecto','Observación','Referencia'],chunks.map((text,index)=>[
          index?'Observación · continuación':'Observación del área',text,observation.column+record.row
        ]),'Catálogo de proyectos','Fuente: catálogo de proyectos de la Convocatoria Mixtos II.');
      });
      anchor = territorialAnchor;
    }

    // Mostrar todos los indicadores y desgloses devueltos por INEGI, sin repetir el municipio.
    const municipalities = new Map();
    result.elementos.forEach(element => {
      if (element.inegi) municipalities.set(element.inegi.geographicCode,{data:element.inegi,name:element.municipio.nombre});
    });
    municipalities.forEach(({data,name}) => {
      const rows = (data.indicators || []).map(i=>[i.label,fmt(i.value,i.decimals)+' '+(i.unit || ''),i.period,[i.source,i.detail].filter(Boolean).join('\n')]);
      (data.breakdowns || []).forEach(group => (group.items || []).forEach(item => rows.push([group.title+' · '+item.label,fmt(item.value,group.decimals)+' '+(group.unit || ''),group.period,[group.source,group.subtitle].filter(Boolean).join('\n')])));
      tablePages('INEGI · '+name,['Indicador / desglose','Valor y unidad','Período','Fuente y alcance'],rows,'Municipio '+data.geographicCode,'Indicadores municipales de referencia · INEGI.');
    });
    if (!municipalities.size) tablePages('Contexto municipal INEGI',['Disponibilidad'],[], 'Contexto social y económico','Indicadores municipales de referencia · INEGI.','Sin municipio identificado para la consulta.');

    const plans = distinct(result.elementos.flatMap(e=>e.intensivo?.pam || []));
    const planRows = plans.map(feature => {
      const p = feature.properties || {};
      return [p.claveProyecto || p.proyectoId || 'No informado',p.nombreProyecto,[p.etapaProyecto,p.estatusLicitacion].filter(Boolean).join('\n'),p.validada?'Ubicación validada':p.esAsociacionSugerida?'Ubicación aproximada':'Ubicación del inventario'];
    });
    tablePages('Proyectos PAM / PAMRNT',['Clave PAM','Proyecto de red','Etapa / licitación','Calidad de la coincidencia'],planRows,'Proximidad y obras habilitantes','Proyectos del PAM/PAMRNT dentro del área de análisis; una clave puede incluir varios tramos.',sourceEmpty(sources.pam));
    const poles = distinct(result.elementos.flatMap(e=>e.intensivo?.podecobi || []));
    tablePages('PODECOBI',['Polo','Estado / municipio','Etapa','Superficie oficial'],poles.map(f=>[prop(f,/NombreOficial|^nombre$/i),[prop(f,/^Estado$/i),prop(f,/^Municipio$/i)].join(' · '),prop(f,/^Etapa$/i),fmt(prop(f,/AreaOficialHa/i,null),2)+' ha']),'Polos de desarrollo','Polos de desarrollo dentro del área de análisis.',sourceEmpty(sources.podecobi));

    [['Subestaciones de transmisión y distribución','seDentro'],['Líneas de transmisión','ltDentro']].forEach(([title,key]) => {
      const features = distinct(result.elementos.flatMap(e=>e[key] || []));
      tablePages(title,['Identificador / nombre','Tensión','Tipo / función','Fuente'],features.map(f=>[prop(f,/^nombre$|^name$|nombre|subestacion/i),prop(f,/tension|volt|kv/i),prop(f,/networkLevel|tipo|funcion/i),prop(f,/^fuente$|^source$/i,'Inventario / grafo eléctrico DGMESNIE')]),'Infraestructura dentro del área de análisis','Infraestructura eléctrica dentro del área de análisis de 20 km.',sourceEmpty(key==='seDentro'?result.capas.subestacionesTransmision:result.capas.lineas));
    });
    [['electricidad','Permisos eléctricos','permisosElectricidad'],['gasNatural','Permisos de gas natural','permisosGasNatural'],['gasLp','Permisos de gas LP','permisosGasLp'],['petroliferos','Permisos de petrolíferos','permisosPetroliferos']].forEach(([key,title,sourceKey]) => {
      const features = distinct(result.elementos.flatMap(e=>e.permisos?.[key] || []));
      tablePages(title,['Permiso','Titular / instalación','Estatus / actividad','Capacidad reportada'],features.map(f=>[prop(f,/numeroPermiso|^permiso$|^folio$/i),prop(f,/^nombre$|razon|titular/i),[prop(f,/^estatus$/i),prop(f,/tipoPermiso|tecnologia/i)].join('\n'),prop(f,/^capacidad$/i,null)==null?'No disponible':fmt(prop(f,/^capacidad$/i),2)+' '+prop(f,/unidadCapacidad/i,'')]),'Inventario en el área de análisis','Permisos vigentes dentro del área de análisis de 20 km.',sourceEmpty(result.capas[sourceKey]));
    });

    // Contexto regional y estatal: se conserva el período de cada fuente y no se
    // suma ni se atribuye la demanda, energía o capacidad al predio del proyecto.
    tablePages('Cobertura por gerencia',['Elemento','GCR','Estado / municipio','Geometría / radio'],result.elementos.map(element=>[
      element.ubicacion?.etiqueta || 'Elemento del proyecto',element.gcr || 'No informada',
      [element.municipio?.entidadNombre,element.municipio?.nombre].filter(Boolean).join(' · ') || 'No informado',
      (element.feature?.geometry?.type || 'No informada')+' · '+fmt(element.radioKm,2)+' km'
    ]),'Ubicación del proyecto y su conexión','Fuente: geometrías registradas en ventanilla, gerencias de control regional del CENACE y cartografía municipal.','Sin geometría disponible.');

    const demand = distinct(result.elementos.map(element=>element.demanda).filter(Boolean));
    demand.forEach(feature=>{
      const date = prop(feature,/^operating_date$/i,'Fecha no informada'), hour = prop(feature,/^hour$/i,'Hora no informada');
      const region = prop(feature,/^region$|^nombre$/i,'Región CENACE'), gcr = prop(feature,/^gerencia$/i,'GCR no informada');
      const metrics = [['Demanda','demand_mw'],['Generación reportada','generation_mw'],['Pronóstico de demanda','forecast_mw'],['Balance generación − demanda','balance_mw'],['Desviación demanda − pronóstico','forecast_deviation_mw']];
      tablePages('Demanda y generación · '+region,['Indicador','Valor y unidad','Fecha / hora'],metrics.map(([label,key])=>[
        label,fmt(feature.properties?.[key],2)+' MW',date+' · '+hour
      ]),String(gcr),'Fuente: '+prop(feature,/^source$|^fuente$/i,'CENACE · Atlas SEN')+'. Actualización: '+prop(feature,/^updated_at$/i,'No informada')+'. Valores regionales del día de consulta.');
    });
    if (!demand.length) tablePages('Demanda y generación',['Disponibilidad'],[],'Contexto regional CENACE','Balance regional del CENACE.',sourceEmpty(result.capas.demanda));

    const tariffs = distinct(result.elementos.map(element=>element.tarifa).filter(Boolean));
    tariffs.forEach(feature=>{
      const year = prop(feature,/^reference_year$/i,'No informado');
      tablePages('División CFE · '+prop(feature,/^division$/i),['Indicador','Valor y unidad','Período'],[
        ['Usuarios',fmt(prop(feature,/^users_total$/i,null))+' usuarios',year],
        ['Energía',fmt(prop(feature,/^energy_gwh$/i,null),3)+' GWh',year]
      ],'Contexto de la división tarifaria','Fuente: '+prop(feature,/^source$|^fuente$/i,'CFE · Atlas SEN')+'. Valores por división tarifaria.');
    });
    if (!tariffs.length) tablePages('Divisiones CFE',['Disponibilidad'],[],'Contexto tarifario','Usuarios y energía por división tarifaria.',sourceEmpty(result.capas.tarifas));

    const centralFeatures = distinct(result.elementos.flatMap(element=>element.intensivo?.centrales || []));
    tablePages('Centrales eléctricas',['Permiso / instalación','Tecnología / estatus','Potencia autorizada MW','Potencia en operación MW','Período'],centralFeatures.map(feature=>[
      [prop(feature,/^NumeroPermiso$/i,''),prop(feature,/^nombre$|^name$|^razon_social$/i)].filter(Boolean).join('\n'),
      [prop(feature,/^tecnologia$/i),prop(feature,/^estatus_instalacion$/i)].join('\n'),
      fmt(prop(feature,/^capacidad_autorizada_mw$/i,null),3),fmt(prop(feature,/^capacidad_operacion_mw$/i,null),3),
      prop(feature,/^periodo$|^reference_year$/i)
    ]),'Potencia reportada en el inventario','Fuente: inventario de centrales eléctricas privadas y de CFE dentro del área de análisis.',sourceEmpty(sources.centrales));

    const distributed = distinct(result.elementos.flatMap(element=>element.intensivo?.generacionDistribuida || []));
    tablePages('Generación distribuida',['Estado','Capacidad MW','Contratos','Período'],distributed.map(feature=>[
      prop(feature,/^nombre$/i),fmt(prop(feature,/^dg_mw$/i,null),3),fmt(prop(feature,/^dg_contracts$/i,null)),prop(feature,/^reference_period$/i)
    ]),'Contexto estatal','Fuente: CNE · Atlas SEN. Totales estatales de generación distribuida.',sourceEmpty(sources.generacionDistribuida));

    const works = window.convocatoriaFichaMap?.obrasDetalle;
    const hasWorkRecords = dossier?.records?.some(record=>record.sheet==='BD_COSTOS_OBRAS');
    if (!hasWorkRecords && works && works.length > 600) {
      // Mantener el texto completo del corte en páginas separadas, sin inventar costos unitarios.
      anchor = document.querySelector('[data-convocatoria-costos-anchor]') || anchor;
      const lines = works.split(/\r?\n/).flatMap(line=>line.match(/[\s\S]{1,420}/g) || ['']);
      tablePages('Desglose de obras',['Descripción de la obra'],lines.map(line=>[line]),'Obras de interconexión y refuerzo','Descripción de obras conforme a la ficha CFE.');
    }
    document.querySelector('.convocatoria-ficha-page').dataset.detailsState = 'complete';
  }, {once:true});
})();
