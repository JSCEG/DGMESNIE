(function () {
  'use strict';

  const root = document.getElementById('carteraApp');
  if (!root) return;

  const $ = (selector) => root.querySelector(selector);
  const $$ = (selector) => [...root.querySelectorAll(selector)];
  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
  const trackingModal = document.getElementById('trackingModal');
  const projectDialog = document.getElementById('projectDialog');
  const importDialog = document.getElementById('importDialog');
  const noteForm = document.getElementById('noteForm');
  const projectForm = document.getElementById('projectForm');
  const importForm = document.getElementById('importForm');
  let selectedFolio = null;
  let lastTrigger = null;
  let state = { projects: [], notes: [], session: {} };
  let portfolioMap = null;
  let portfolioMarkers = null;
  let portfolioGeometry = null;

  // El shell institucional usa isolation en .main-content. Elevar el modal al
  // body evita que su posición fija se calcule dentro de ese contenedor.
  if (trackingModal && trackingModal.parentElement !== document.body) {
    document.body.appendChild(trackingModal);
  }

  const GCR_COLORS = {
    noreste: '#3E8174', peninsular: '#A33052', oriental: '#6FA89A', central: '#B24C6C',
    occidental: '#A57F2C', bcalifornia: '#7E3B52', noroeste: '#1E5B4F', norte: '#9B2247',
    bcsur: '#A9CDC3', mulege: '#E0CA8E'
  };
  const GCR_POS = {
    noreste: [600, 250], peninsular: [912, 448], oriental: [700, 512], central: [583, 452],
    occidental: [495, 392], bcalifornia: [150, 118], noroeste: [300, 178], norte: [430, 206],
    bcsur: [212, 318], mulege: [150, 250]
  };
  const GCR_NAMES = {
    noreste: 'Noreste', peninsular: 'Peninsular', oriental: 'Oriental', central: 'Central',
    occidental: 'Occidental', bcalifornia: 'B. California', noroeste: 'Noroeste', norte: 'Norte',
    bcsur: 'BC Sur', mulege: 'Mulegé'
  };
  const STATUS_LABELS = {
    continua: 'Continúa', revision: 'En revisión', 'no-continua': 'No continúa'
  };
  const CONSIDERATION_LABELS = {
    firme: 'Firme', revision: 'En revisión', 'no-va': 'No va'
  };

  function esc(value) {
    return String(value ?? '').replace(/[&<>"']/g, (character) => ({
      '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[character]));
  }

  function dateOnly(value) {
    if (!value) return '';
    return String(value).slice(0, 10);
  }

  function displayDate(value) {
    const date = dateOnly(value);
    if (!date) return 'Sin fecha';
    return new Date(`${date}T12:00:00`).toLocaleDateString('es-MX', {
      day: 'numeric', month: 'long', year: 'numeric'
    });
  }

  function decisionLabel(value) {
    return STATUS_LABELS[value] || 'En revisión';
  }

  function considerationLabel(value) {
    return CONSIDERATION_LABELS[value] || 'En revisión';
  }

  async function api(url, options = {}) {
    const headers = {
      Accept: 'application/json',
      'X-Requested-With': 'XMLHttpRequest',
      ...(options.headers || {})
    };
    if (options.body) headers['Content-Type'] = 'application/json';
    if ((options.method || 'GET').toUpperCase() !== 'GET' && token) {
      headers.RequestVerificationToken = token;
    }

    const response = await fetch(url, { cache: 'no-store', ...options, headers });
    const contentType = response.headers.get('content-type') || '';
    const payload = contentType.includes('application/json') ? await response.json() : null;
    if (response.redirected || (!contentType.includes('application/json') && response.ok)) {
      throw new Error('Tu sesión expiró. Inicia sesión nuevamente; el comentario permanece en el formulario.');
    }
    if (!response.ok) throw new Error(payload?.error || `Error HTTP ${response.status}`);
    if (payload == null) throw new Error('El servidor no devolvió una confirmación válida.');
    return payload;
  }

  function filtered() {
    const query = $('#projectSearch').value.trim().toLowerCase();
    const type = $('#typeFilter').value;
    const region = $('#regionFilter').value;
    const entity = $('#stateFilter').value;
    const consideration = $('#considerationFilter').value;
    const analysis = $('#analysisFilter').value;

    return state.projects
      .filter((project) =>
        (!query || [project.folio, project.name, project.state, project.substation, project.company]
          .join(' ').toLowerCase().includes(query)) &&
        (!type || project.type === type) &&
        (!region || project.region === region) &&
        (!entity || project.state === entity) &&
        (!consideration || project.consideration === consideration) &&
        (!analysis || project.analysisClassification === analysis))
      .sort((a, b) => a.rank - b.rank);
  }

  function fillSelect(select, values) {
    const selected = select.value;
    [...new Set(values.filter(Boolean))].sort((a, b) => a.localeCompare(b, 'es'))
      .forEach((value) => {
        if (![...select.options].some((option) => option.value === value)) {
          select.add(new Option(value, value));
        }
      });
    select.value = selected;
  }

  function groupProjects(rows, valueSelector) {
    const groups = new Map();
    rows.forEach((project) => {
      const label = String(valueSelector(project) || 'Sin dato').trim() || 'Sin dato';
      const current = groups.get(label) || { label, count: 0, mw: 0 };
      current.count += 1;
      current.mw += Number(project.mw || 0);
      groups.set(label, current);
    });
    return [...groups.values()];
  }

  function analysisColor(label) {
    const value = String(label || '').toLowerCase();
    if (value.includes('factible') && !value.includes('no')) return '#0e735c';
    if (value.includes('condicion')) return '#a57f2c';
    if (value.includes('revisi') || value.includes('pend')) return '#237f91';
    if (value.includes('no factible') || value.includes('descart')) return '#9b2247';
    return '#706b65';
  }

  function renderBars(container, items, options) {
    if (!container) return;
    const settings = options || {};
    const valueKey = settings.valueKey || 'mw';
    const values = [...items].sort((a, b) => Number(b[valueKey] || 0) - Number(a[valueKey] || 0));
    const maximum = Math.max(1, ...values.map((item) => Number(item[valueKey] || 0)));
    container.replaceChildren();
    if (!values.length) {
      const empty = document.createElement('p');
      empty.className = 'cartera-bars__empty';
      empty.textContent = 'Sin proyectos para los filtros seleccionados.';
      container.appendChild(empty);
      return;
    }

    values.slice(0, settings.limit || 10).forEach((item) => {
      const value = Number(item[valueKey] || 0);
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'cartera-bar';
      button.title = settings.filterId ? 'Filtrar por ' + item.label : item.label;

      const heading = document.createElement('span');
      heading.className = 'cartera-bar__heading';
      const label = document.createElement('strong');
      label.textContent = item.label;
      const amount = document.createElement('span');
      amount.textContent = value.toLocaleString('es-MX', { maximumFractionDigits: 1 }) + (settings.unit ? ' ' + settings.unit : '');
      heading.append(label, amount);

      const track = document.createElement('span');
      track.className = 'cartera-bar__track';
      const fill = document.createElement('i');
      fill.style.width = Math.max(2, (value / maximum) * 100) + '%';
      fill.style.backgroundColor = settings.color ? settings.color(item.label) : '#9b2247';
      track.appendChild(fill);

      const detail = document.createElement('small');
      detail.textContent = item.count.toLocaleString('es-MX') + (item.count === 1 ? ' proyecto' : ' proyectos');
      button.append(heading, track, detail);
      if (settings.filterId) {
        button.addEventListener('click', () => {
          const filter = $('#' + settings.filterId);
          if (!filter) return;
          filter.value = filter.value === item.label ? '' : item.label;
          render();
        });
      }
      container.appendChild(button);
    });
  }

  function renderAnalytics(rows) {
    const capacity = rows.reduce((sum, project) => sum + Number(project.mw || 0), 0);
    const caption = $('#analyticsCaption');
    if (caption) caption.textContent = rows.length.toLocaleString('es-MX') + ' proyectos · ' + capacity.toLocaleString('es-MX', { maximumFractionDigits: 1 }) + ' MW';
    renderBars($('#regionChart'), groupProjects(rows, (project) => project.region), {
      valueKey: 'mw', unit: 'MW', filterId: 'regionFilter',
      color: (label) => GCR_COLORS[gcrId(label)] || '#9b2247'
    });
    renderBars($('#analysisChart'), groupProjects(rows, (project) => project.analysisClassification || 'Sin análisis'), {
      valueKey: 'count', unit: '', filterId: 'analysisFilter', color: analysisColor
    });
    renderBars($('#technologyChart'), groupProjects(rows, (project) => project.technology || 'Sin tecnología'), {
      valueKey: 'mw', unit: 'MW', limit: 8, color: () => '#a57f2c'
    });
  }

  function render() {
    fillSelect($('#regionFilter'), state.projects.map((project) => project.region));
    fillSelect($('#stateFilter'), state.projects.map((project) => project.state));
    fillSelect($('#analysisFilter'), state.projects.map((project) => project.analysisClassification));

    const rows = filtered();
    const firmProjects = state.projects.filter((project) => project.consideration === 'firme');
    const totalMw = firmProjects.reduce((sum, project) => sum + Number(project.mw || 0), 0);
    $('#resultCount').textContent = `${rows.length} visibles`;
    $('#mapCount').textContent = `${rows.length} proyectos visibles`;
    $('#kpiTotal').textContent = state.projects.length.toLocaleString('es-MX');
    $('#kpiMw').textContent = totalMw.toLocaleString('es-MX', { maximumFractionDigits: 3 });
    $('#kpiFirm').textContent = firmProjects.length.toLocaleString('es-MX');
    $('#kpiReview').textContent = state.projects.filter((project) => project.consideration === 'revision').length.toLocaleString('es-MX');
    $('#kpiRejected').textContent = state.projects.filter((project) => project.consideration === 'no-va').length.toLocaleString('es-MX');
    $('#kpiKml').textContent = firmProjects.filter((project) => project.hasProjectKml).length.toLocaleString('es-MX');
    const latest = state.latestImport;
    $('#sourceCaption').textContent = latest
      ? `MIXTOS II · CORTE ${displayDate(latest.cutoffDate).toUpperCase()} · ${latest.fileName}`
      : `CATÁLOGO INSTITUCIONAL · ${state.sourceVersion || 'SIN CARGA EXCEL'}`;

    $('#projectList').innerHTML = rows.map((project) => `
      <article class="project-card">
        <div class="project-card__rank">${project.rank}</div>
        <div>
          <h3>${esc(project.name)}</h3>
          <p>${esc(project.folio)} · ${esc(project.state)} · ${Number(project.mw || 0).toLocaleString('es-MX')} MW</p>
          <p>${esc(project.interconnectionPoint || project.substation || 'Punto de interconexión por confirmar')}</p>
          <div class="project-card__tags">
            <span class="chip ${project.type === 'Estratégico' ? 'chip--strategic' : 'chip--private'}">${esc(project.type)}</span>
            <span class="chip">${esc(project.region)}</span>
            <span class="chip ${analysisChipClass(project.analysisClassification)}">${esc(project.analysisClassification || 'Sin análisis')}</span>
            <span class="chip ${project.consideration === 'firme' ? 'chip--yes' : project.consideration === 'no-va' ? 'chip--no' : 'chip--pending'}">${considerationLabel(project.consideration).toUpperCase()}</span>
          </div>
        </div>
        <div class="project-card__actions">
          <button class="icon-button" type="button" data-note="${esc(project.folio)}">Seguimiento</button>
          <button class="icon-button" type="button" data-ficha="${esc(project.folio)}">Mapa / KML</button>
          ${project.consideration === 'firme' ? `<a class="icon-button icon-button--link" href="/DashboardProyectos/Index?mixtosFolio=${encodeURIComponent(project.folio)}" target="_blank" rel="noopener">Analizar · elegir capas</a>` : ''}
          ${project.consideration === 'firme' ? `<a class="icon-button icon-button--link" href="/DashboardProyectos/Convocatorias/Ficha?folio=${encodeURIComponent(project.folio)}" target="_blank" rel="noopener">Ver ficha</a>` : ''}
        </div>
      </article>`).join('') || '<p style="padding:30px;color:#6f6b66">No hay proyectos con los filtros seleccionados.</p>';

    $('#rankingBody').innerHTML = rows.map((project) => `
      <tr>
        <td><strong>${project.rank}</strong></td>
        <td>${esc(project.name)}<br><small>${esc(project.folio)}</small></td>
        <td>${esc(project.region)}<br><small>${esc(project.state)}</small></td>
        <td>${esc(project.type)}</td>
        <td><span class="chip ${analysisChipClass(project.analysisClassification)}">${esc(project.analysisClassification || 'Sin análisis')}</span></td>
        <td><select class="priority-select" data-priority="${esc(project.folio)}">
          ${[1, 2, 3, 4].map((priority) => `<option value="${priority}" ${project.priority === priority ? 'selected' : ''}>${priority}</option>`).join('')}
        </select></td>
        <td><select class="decision-select" data-decision="${esc(project.folio)}">
          ${Object.entries(STATUS_LABELS).map(([value, label]) => `<option value="${value}" ${project.decision === value ? 'selected' : ''}>${label}</option>`).join('')}
        </select></td>
        <td><button class="icon-button" type="button" data-note="${esc(project.folio)}">Comentario</button></td>
      </tr>`).join('');

    renderAnalytics(rows);
    renderGcrMap(rows);
    renderSessions();
    bindDynamic();
  }

  function renderSessions() {
    const groups = Object.values(state.notes.reduce((accumulator, note) => {
      const key = `${note.session}|${dateOnly(note.date)}`;
      (accumulator[key] ??= { session: note.session, date: dateOnly(note.date), notes: [] }).notes.push(note);
      return accumulator;
    }, {})).sort((a, b) => b.date.localeCompare(a.date));

    $('#sessionTimeline').innerHTML = groups.map((group) => `
      <article class="session-item">
        <h3>${esc(group.session)}</h3>
        <p>${displayDate(group.date)} · ${group.notes.length} registro(s)</p>
        ${group.notes.map((note) => `<div class="session-note"><strong>${esc(note.folio)}</strong><br>${esc(note.text)}${note.user ? `<br><small>${esc(note.user)}</small>` : ''}</div>`).join('')}
      </article>`).join('') || '<p>No hay sesiones registradas.</p>';
  }

  function gcrId(value) {
    const normalized = String(value || '').toLowerCase().normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/regional|gerencia|de control|gcr/g, '')
      .replace(/[^a-z]/g, '');
    return ({
      bajacalifornia: 'bcalifornia', bc: 'bcalifornia', bajacaliforniasur: 'bcsur', bs: 'bcsur',
      mulege: 'mulege', noreste: 'noreste', ne: 'noreste', noroeste: 'noroeste', no: 'noroeste',
      norte: 'norte', nt: 'norte', occidental: 'occidental', oc: 'occidental', oriental: 'oriental',
      or: 'oriental', central: 'central', ce: 'central', peninsular: 'peninsular', pe: 'peninsular'
    })[normalized] || normalized;
  }

  function renderGcrMap(rows) {
    const element = $('#portfolioMap');
    if (!window.L) {
      element.innerHTML = '<div class="gcr-map__fallback">No fue posible iniciar el mapa territorial.</div>';
      return;
    }
    if (!portfolioMap) {
      element.innerHTML = '';
      portfolioMap = L.map(element, { preferCanvas: true, zoomControl: true }).setView([23.6, -102], 5);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '&copy; OpenStreetMap'
      }).addTo(portfolioMap);
      portfolioMarkers = L.layerGroup().addTo(portfolioMap);
      portfolioGeometry = L.featureGroup().addTo(portfolioMap);
    }

    portfolioMarkers.clearLayers();
    portfolioGeometry.clearLayers();
    const bounds = [];
    rows.forEach((project) => {
      const latitude = Number(project.latitude);
      const longitude = Number(project.longitude);
      if (project.latitude == null || project.longitude == null || !Number.isFinite(latitude) || !Number.isFinite(longitude)) return;
      const color = project.consideration === 'firme' ? '#0e735c'
        : project.consideration === 'no-va' ? '#6f6b66' : '#237f91';
      const marker = L.circleMarker([latitude, longitude], {
        radius: 6,
        color: '#fff',
        weight: 2,
        fillColor: color,
        fillOpacity: .92
      });
      marker.bindTooltip(`<strong>${esc(project.name)}</strong><br>${esc(project.folio)} · ${Number(project.mw || 0).toLocaleString('es-MX')} MW`);
      const popup = document.createElement('div');
      popup.innerHTML = `<strong>${esc(project.name)}</strong><p>${esc(project.folio)} · ${Number(project.mw||0).toLocaleString('es-MX')} MW<br>${esc(project.region)} · ${esc(project.technicalViability||'Factibilidad no informada')}<br>Considerar: ${esc(considerationLabel(project.consideration))}<br>Costo red: ${project.networkCostMxn==null?'No informado':Number(project.networkCostMxn).toLocaleString('es-MX')+' MDP'}</p>`;
      const dataButton=document.createElement('button'); dataButton.type='button'; dataButton.textContent='Ver datos';
      dataButton.addEventListener('click',()=>openTracking(project.folio,element)); popup.appendChild(dataButton);
      if(project.consideration==='firme') {
        for(const [label,url] of [['Analizar · elegir capas','/DashboardProyectos/Index?mixtosFolio='],['Ficha completa','/DashboardProyectos/Convocatorias/Ficha?folio=']]) {
          const link=document.createElement('a');link.textContent=label;link.href=url+encodeURIComponent(project.folio);link.target='_blank';link.rel='noopener';link.style.cssText='display:block;margin-top:8px;color:#9b2247;font-weight:700';popup.appendChild(link);
        }
      }
      marker.bindPopup(popup);
      marker.on('click', () => showProjectGeometry(project));
      marker.addTo(portfolioMarkers);
      bounds.push([latitude, longitude]);
    });
    $('#mapCount').textContent = `${bounds.length} de ${rows.length} proyectos ubicados`;
    window.setTimeout(() => {
      portfolioMap.invalidateSize();
      if (bounds.length) portfolioMap.fitBounds(bounds, { padding: [22, 22], maxZoom: 8 });
      else portfolioMap.setView([23.6, -102], 5);
    }, 0);
  }

  async function showProjectGeometry(project) {
    if (!portfolioMap || !portfolioGeometry) return;
    portfolioGeometry.clearLayers();
    const resources = [];
    if (project.hasProjectKml) resources.push({ kind: 'proyecto', color: '#0e735c' });
    if (project.hasSubstationKml && !project.sameKmlReference) resources.push({ kind: 'subestacion', color: '#9b2247' });
    if (!resources.length) {
      $('#mapCount').textContent = `${project.name}: sin KML registrado`;
      return;
    }

    $('#mapCount').textContent = `Cargando geometría de ${project.name}…`;
    const results = await Promise.allSettled(resources.map(async (resource) => {
      const url = `${root.dataset.apiKml}/${encodeURIComponent(project.folio)}/${resource.kind}`;
      const response = await fetch(url, { cache: 'no-store', headers: { Accept: 'application/vnd.google-earth.kml+xml' } });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const xml = new DOMParser().parseFromString(await response.text(), 'text/xml');
      if (xml.querySelector('parsererror')) throw new Error('KML inválido');
      xml.querySelectorAll('name, description').forEach((node) => node.remove());
      const layer = new L.KML(xml, { vertexPointThreshold: 50 });
      layer.eachLayer((item) => {
        if (item.setStyle) item.setStyle({ color: resource.color, weight: 3, fillColor: resource.color, fillOpacity: .16 });
      });
      portfolioGeometry.addLayer(layer);
      return layer;
    }));

    const loaded = results.filter((result) => result.status === 'fulfilled').length;
    if (loaded && portfolioGeometry.getBounds().isValid()) {
      portfolioMap.fitBounds(portfolioGeometry.getBounds(), { padding: [28, 28], maxZoom: 13 });
      const suppressed = results.reduce((total, result) => total +
        (result.status === 'fulfilled' ? Number(result.value?._vertexPointsSuppressed || 0) : 0), 0);
      $('#mapCount').textContent = `${loaded} geometría(s) KML · ${project.name}` +
        (suppressed ? ` · ${suppressed.toLocaleString('es-MX')} vértices sin marcadores` : '');
    } else {
      $('#mapCount').textContent = `No fue posible cargar el KML de ${project.name}`;
    }
  }

  function commentsFor(folio) {
    return state.notes.filter((note) => note.folio === folio)
      .sort((a, b) => `${dateOnly(b.date)}-${b.commentId || 0}`.localeCompare(`${dateOnly(a.date)}-${a.commentId || 0}`));
  }

  function analysisChipClass(value) {
    return ({
      'Factible': 'chip--feasible',
      'Excluyente preseleccionado': 'chip--exclusive',
      'En análisis': 'chip--pending',
      'Por analizar': 'chip--review',
      'No viable': 'chip--unfeasible'
    })[value] || '';
  }

  function renderTrackingComments() {
    const notes = commentsFor(selectedFolio);
    document.getElementById('trackingCommentCount').textContent = `${notes.length} comentario${notes.length === 1 ? '' : 's'}`;
    document.getElementById('trackingComments').innerHTML = notes.map((note) => `
      <article class="tracking-comment">
        <p>${esc(note.text)}</p>
        <footer><span>${displayDate(note.date)}</span><span>·</span><span>${esc(note.session)}</span>${note.user ? `<span>·</span><span>${esc(note.user)}</span>` : ''}</footer>
      </article>`).join('') || '<p class="tracking-comments__empty">Sin comentarios todavía.</p>';
  }

  function updateTrackingStatus(project) {
    trackingModal.querySelectorAll('[data-status]').forEach((button) => {
      button.classList.toggle('is-active', button.dataset.status === project.decision);
    });
    document.getElementById('trackingStatusHint').textContent = `${decisionLabel(project.decision)} · estado guardado en la BD.`;
  }

  function openTracking(folio, trigger) {
    const project = state.projects.find((item) => item.folio === folio);
    if (!project) return;
    selectedFolio = folio;
    lastTrigger = trigger || document.activeElement;
    const regionProjects = state.projects.filter((item) => item.region === project.region);
    const regionMw = regionProjects.reduce((sum, item) => sum + Number(item.mw || 0), 0);

    document.getElementById('trackingTitle').textContent = project.name;
    document.getElementById('trackingFolio').textContent = `${project.folio} · ${project.type} · ${Number(project.mw || 0).toLocaleString('es-MX')} MW`;
    document.getElementById('trackingCompany').textContent = project.company || '—';
    document.getElementById('trackingGroup').textContent = project.interestGroup || '—';
    document.getElementById('trackingRegion').textContent = `GCR ${project.region} · ${regionProjects.length} proyectos · ${regionMw.toLocaleString('es-MX', { maximumFractionDigits: 3 })} MW`;
    document.getElementById('trackingState').textContent = project.state || '—';
    document.getElementById('trackingMunicipality').textContent = project.municipality || '—';
    document.getElementById('trackingInterconnection').textContent = project.interconnectionPoint || project.substation || '—';
    document.getElementById('trackingConsideration').textContent = considerationLabel(project.consideration);
    const network = [];
    if (project.worksCount) network.push(`${project.worksCount} obra(s)`);
    if (project.networkCostMxn) network.push(`${Number(project.networkCostMxn).toLocaleString('es-MX')} MDP`);
    if (project.networkCostUsd) network.push(`${Number(project.networkCostUsd).toLocaleString('es-MX')} MDD`);
    document.getElementById('trackingNetwork').textContent = network.join(' · ') || project.worksDescription || '—';
    document.getElementById('trackingRank').textContent = `${project.rank} · prioridad ${project.priority}`;
    document.getElementById('trackingAnalysisClass').textContent = project.analysisClassification || '—';
    document.getElementById('trackingViability').textContent = project.technicalViability || '—';
    document.getElementById('trackingAnalysis').textContent = project.technicalAnalysis || 'Sin observaciones adicionales registradas por la mesa técnica.';
    const duplicateRow = document.getElementById('trackingDuplicateRow');
    duplicateRow.hidden = !project.duplicateGroup;
    document.getElementById('trackingDuplicate').textContent = project.duplicateGroup
      ? `${project.duplicateGroup} · revisar las inscripciones asociadas`
      : '—';
    noteForm.elements.folio.value = project.folio;
    noteForm.elements.session.value = $('#sessionName').value || state.session?.name || 'Sesión de trabajo';
    noteForm.elements.date.value = $('#sessionDate').value || state.session?.date || new Date().toISOString().slice(0, 10);
    document.getElementById('noteFormStatus').textContent = 'El comentario se guardará en la BD.';
    document.getElementById('noteFormStatus').className = '';
    updateTrackingStatus(project);
    renderTrackingComments();
    const modalBody = trackingModal.querySelector('.tracking-modal__body');
    modalBody.scrollTop = 0;
    trackingModal.hidden = false;
    document.body.classList.add('tracking-modal-open');
    requestAnimationFrame(() => {
      modalBody.scrollTop = 0;
      trackingModal.querySelector('.tracking-modal__header [data-close-tracking]').focus({ preventScroll: true });
    });
  }

  function closeTracking() {
    trackingModal.hidden = true;
    document.body.classList.remove('tracking-modal-open');
    noteForm.elements.text.value = '';
    selectedFolio = null;
    lastTrigger?.focus?.();
  }

  async function saveStatus(button) {
    const project = state.projects.find((item) => item.folio === selectedFolio);
    if (!project || project.decision === button.dataset.status) return;
    const buttons = [...trackingModal.querySelectorAll('[data-status]')];
    buttons.forEach((item) => { item.disabled = true; });
    document.getElementById('trackingStatusHint').textContent = 'Guardando estado…';
    try {
      const response = await api(root.dataset.apiStatus, {
        method: 'PUT', body: JSON.stringify({ folio: project.folio, estado: button.dataset.status })
      });
      project.decision = response.decision;
      project.consideration = response.decision === 'continua' ? 'firme' : response.decision === 'no-continua' ? 'no-va' : 'revision';
      updateTrackingStatus(project);
      render();
    } catch (error) {
      document.getElementById('trackingStatusHint').textContent = error.message;
    } finally {
      buttons.forEach((item) => { item.disabled = false; });
    }
  }

  async function savePriority(select) {
    const project = state.projects.find((item) => item.folio === select.dataset.priority);
    if (!project) return;
    const previous = project.priority;
    select.disabled = true;
    try {
      await api(root.dataset.apiPriority, {
        method: 'PUT', body: JSON.stringify({ folio: project.folio, prioridad: Number(select.value) })
      });
      project.priority = Number(select.value);
    } catch (error) {
      select.value = previous;
      alert(error.message);
    } finally {
      select.disabled = false;
    }
  }

  function bindDynamic() {
    $$('[data-note]').forEach((button) => { button.onclick = () => openTracking(button.dataset.note, button); });
    $$('[data-ficha]').forEach((button) => {
      button.onclick = () => {
        const project = state.projects.find((item) => item.folio === button.dataset.ficha);
        if (!project) return;
        showProjectGeometry(project);
        $('#portfolioMap').scrollIntoView({ behavior: 'smooth', block: 'center' });
      };
    });
    $$('[data-priority]').forEach((select) => { select.onchange = () => savePriority(select); });
    $$('[data-decision]').forEach((select) => {
      select.onchange = async () => {
        const project = state.projects.find((item) => item.folio === select.dataset.decision);
        const previous = project.decision;
        select.disabled = true;
        try {
          await api(root.dataset.apiStatus, {
            method: 'PUT', body: JSON.stringify({ folio: project.folio, estado: select.value })
          });
          project.decision = select.value;
          project.consideration = select.value === 'continua' ? 'firme' : select.value === 'no-continua' ? 'no-va' : 'revision';
          render();
        } catch (error) {
          select.value = previous;
          alert(error.message);
        } finally {
          select.disabled = false;
        }
      };
    });
  }

  noteForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    if (!selectedFolio || !noteForm.reportValidity()) return;
    const submit = noteForm.querySelector('button[type="submit"]');
    const feedback = document.getElementById('noteFormStatus');
    submit.disabled = true;
    feedback.className = '';
    feedback.textContent = 'Guardando comentario…';
    try {
      const response = await api(root.dataset.apiComment, {
        method: 'POST',
        body: JSON.stringify({
          folio: selectedFolio,
          sesion: noteForm.elements.session.value,
          fecha: noteForm.elements.date.value,
          comentario: noteForm.elements.text.value
        })
      });
      state.notes.unshift(response.note);
      noteForm.elements.text.value = '';
      feedback.className = 'is-success';
      feedback.textContent = 'Comentario guardado en la base de datos.';
      renderTrackingComments();
      renderSessions();
      noteForm.elements.text.focus();
    } catch (error) {
      feedback.className = 'is-error';
      feedback.textContent = error.message;
    } finally {
      submit.disabled = false;
    }
  });

  projectForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    if (!projectForm.reportValidity()) return;
    const data = Object.fromEntries(new FormData(projectForm));
    const submit = projectForm.querySelector('button[type="submit"]');
    const feedback = document.getElementById('projectFormStatus');
    submit.disabled = true;
    feedback.className = 'cartera-form-status';
    feedback.textContent = 'Guardando proyecto…';
    try {
      await api(root.dataset.apiProject, {
        method: 'POST',
        body: JSON.stringify({
          folio: data.folio,
          nombre: data.name,
          tipo: data.type,
          gerencia: data.region,
          entidad: data.state,
          tecnologia: data.technology,
          razonSocial: data.company,
          grupoInteres: data.interestGroup,
          puntoInterconexion: data.substation,
          capacidadMw: Number(data.mw || 0),
          prioridad: Number(data.priority || 4)
        })
      });
      projectForm.reset();
      projectDialog.close();
      await loadData();
    } catch (error) {
      feedback.className = 'cartera-form-status is-error';
      feedback.textContent = error.message;
    } finally {
      submit.disabled = false;
    }
  });

  importForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    if (!importForm.reportValidity()) return;
    const submit = importForm.querySelector('button[type="submit"]');
    const feedback = document.getElementById('importFormStatus');
    submit.disabled = true;
    feedback.className = 'cartera-form-status';
    feedback.textContent = 'Validando catálogo, coordenadas y folios…';
    try {
      const response = await fetch(root.dataset.apiImport, {
        method: 'POST',
        headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest', RequestVerificationToken: token },
        body: new FormData(importForm)
      });
      const contentType = response.headers.get('content-type') || '';
      const payload = contentType.includes('application/json') ? await response.json() : null;
      if (!response.ok) throw new Error(payload?.error || `Error HTTP ${response.status}`);
      const result = payload.result;
      feedback.className = 'cartera-form-status is-success';
      feedback.textContent = result.alreadyImported
        ? `Este archivo ya estaba cargado como ${result.sourceVersion}. No se duplicaron datos.`
        : `Carga completa: ${result.total} proyectos, ${result.firm} firmes, ${result.rejected} no van; ${result.projectKml} KML de proyecto.`;
      await loadData();
      if (!result.alreadyImported) importForm.reset();
    } catch (error) {
      feedback.className = 'cartera-form-status is-error';
      feedback.textContent = error.message;
    } finally {
      submit.disabled = false;
    }
  });

  trackingModal.querySelectorAll('[data-close-tracking]').forEach((button) => button.addEventListener('click', closeTracking));
  trackingModal.querySelectorAll('[data-status]').forEach((button) => button.addEventListener('click', () => saveStatus(button)));
  document.querySelectorAll('[data-close-project]').forEach((button) => button.addEventListener('click', () => projectDialog.close()));
  document.querySelectorAll('[data-close-import]').forEach((button) => button.addEventListener('click', () => importDialog.close()));
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && !trackingModal.hidden) closeTracking();
  });

  ['#projectSearch', '#typeFilter', '#regionFilter', '#stateFilter', '#considerationFilter', '#analysisFilter']
    .forEach((selector) => $(selector).addEventListener('input', render));

  $$('[data-view]').forEach((button) => {
    button.onclick = () => {
      $$('[data-view]').forEach((item) => item.classList.toggle('is-active', item === button));
      $$('[data-panel]').forEach((panel) => { panel.hidden = panel.dataset.panel !== button.dataset.view; });
      if (button.dataset.view === 'portfolio' && portfolioMap) window.setTimeout(() => portfolioMap.invalidateSize(), 0);
    };
  });

  root.addEventListener('click', (event) => {
    const button = event.target.closest('[data-action]');
    if (!button) return;
    const action = button.dataset.action;
    if (action === 'new-project') projectDialog.showModal();
    if (action === 'import') importDialog.showModal();
    if (action === 'clear-filters') {
      $$('#projectSearch,#typeFilter,#regionFilter,#stateFilter,#analysisFilter').forEach((field) => { field.value = ''; });
      $('#considerationFilter').value = 'firme';
      render();
    }
    if (action === 'report') window.location.assign(root.dataset.reportUrl);
    if (action === 'save-ranking') alert('La prioridad se guarda automáticamente en la base de datos al cambiar cada proyecto.');
    if (action === 'new-session') {
      const first = filtered()[0] || state.projects[0];
      if (first) openTracking(first.folio, button);
    }
  });

  async function loadData() {
    try {
      const data = await api(root.dataset.apiData);
      state = {
        sourceVersion: data.sourceVersion,
        source: data.source,
        projects: (data.projects || []).filter(project => project.source === 'Mixtos II'),
        notes: data.notes || [],
        session: data.session || {},
        latestImport: data.latestImport || null
      };
      if (state.session.name) $('#sessionName').value = state.session.name;
      if (state.session.date) $('#sessionDate').value = dateOnly(state.session.date);
      render();
    } catch (error) {
      $('#projectList').innerHTML = `<p style="padding:30px;color:#9b2247"><strong>No fue posible consultar la base de datos.</strong><br>${esc(error.message)}</p>`;
      console.error(error);
    }
  }

  loadData();
})();
