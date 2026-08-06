(function () {
  'use strict';

  const root = document.getElementById('carteraApp');
  if (!root) return;

  const $ = (selector) => root.querySelector(selector);
  const $$ = (selector) => [...root.querySelectorAll(selector)];
  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
  const trackingModal = document.getElementById('trackingModal');
  const projectDialog = document.getElementById('projectDialog');
  const noteForm = document.getElementById('noteForm');
  const projectForm = document.getElementById('projectForm');
  let selectedFolio = null;
  let lastTrigger = null;
  let state = { projects: [], notes: [], session: {} };

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
    const decision = $('#decisionFilter').value;
    const analysis = $('#analysisFilter').value;

    return state.projects
      .filter((project) =>
        (!query || [project.folio, project.name, project.state, project.substation, project.company]
          .join(' ').toLowerCase().includes(query)) &&
        (!type || project.type === type) &&
        (!region || project.region === region) &&
        (!entity || project.state === entity) &&
        (!decision || project.decision === decision) &&
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

  function render() {
    fillSelect($('#regionFilter'), state.projects.map((project) => project.region));
    fillSelect($('#stateFilter'), state.projects.map((project) => project.state));
    fillSelect($('#analysisFilter'), state.projects.map((project) => project.analysisClassification));

    const rows = filtered();
    const totalMw = state.projects.reduce((sum, project) => sum + Number(project.mw || 0), 0);
    $('#resultCount').textContent = `${rows.length} visibles`;
    $('#mapCount').textContent = `${rows.length} proyectos visibles`;
    $('#kpiTotal').textContent = state.projects.length.toLocaleString('es-MX');
    $('#kpiMw').textContent = totalMw.toLocaleString('es-MX', { maximumFractionDigits: 3 });
    $('#kpiStrategic').textContent = state.projects.filter((project) => project.type === 'Estratégico').length;
    $('#kpiPrivate').textContent = state.projects.filter((project) => project.type === 'Particular 2').length;
    $('#kpiFeasible').textContent = state.projects.filter((project) => project.analysisClassification === 'Factible').length;
    $('#kpiDecision').textContent = state.projects.filter((project) => project.decision !== 'revision').length;

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
            <span class="chip ${project.decision === 'continua' ? 'chip--yes' : project.decision === 'no-continua' ? 'chip--no' : ''}">${decisionLabel(project.decision).toUpperCase()}</span>
          </div>
        </div>
        <div class="project-card__actions">
          <button class="icon-button" type="button" data-note="${esc(project.folio)}">Seguimiento</button>
          <button class="icon-button" type="button" data-ficha="${esc(project.folio)}">Ficha</button>
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
    if (!window.GCR_PATHS) {
      element.innerHTML = '<div class="gcr-map__fallback">No fue posible cargar la base de Gerencias de Control Regional.</div>';
      return;
    }

    const totals = {};
    Object.keys(window.GCR_PATHS).forEach((id) => { totals[id] = { count: 0, mw: 0, strategic: 0, private: 0 }; });
    rows.forEach((project) => {
      const id = gcrId(project.region);
      if (!totals[id]) return;
      totals[id].count += 1;
      totals[id].mw += Number(project.mw || 0);
      if (project.type === 'Estratégico') totals[id].strategic += 1;
      else totals[id].private += 1;
    });

    const active = gcrId($('#regionFilter').value);
    const paths = Object.keys(window.GCR_PATHS).map((id) => {
      const total = totals[id];
      const position = GCR_POS[id] || [500, 300];
      const selected = active === id;
      const markers = rows.filter((project) => gcrId(project.region) === id).slice(0, 7).map((project, index) => {
        const angle = (index / Math.max(1, Math.min(7, total.count))) * Math.PI * 2;
        const radius = total.count > 1 ? 18 : 0;
        const x = position[0] + Math.cos(angle) * radius;
        const y = position[1] + 28 + Math.sin(angle) * radius;
        return `<circle class="gcr-project ${project.type === 'Estratégico' ? 'gcr-project--strategic' : 'gcr-project--private'} ${project.decision === 'no-continua' ? 'gcr-project--no' : ''}" cx="${x.toFixed(1)}" cy="${y.toFixed(1)}" r="6"/>`;
      }).join('');
      return `<g data-gcr="${id}"><path tabindex="0" role="button" aria-label="${GCR_NAMES[id]}: ${total.count} proyectos, ${total.mw.toLocaleString('es-MX')} MW" class="gcr-region ${total.count ? '' : 'is-empty'} ${selected ? 'is-selected' : ''}" data-gcr="${id}" d="${window.GCR_PATHS[id]}" fill="${GCR_COLORS[id] || '#B9B3AB'}" stroke="#fff" stroke-width="1.25"></path><text class="gcr-label" x="${position[0]}" y="${position[1]}">${GCR_NAMES[id]}</text><text class="gcr-label__value" text-anchor="middle" x="${position[0]}" y="${position[1] + 18}">${total.count} proy. · ${total.mw.toLocaleString('es-MX')} MW</text>${markers}</g>`;
    }).join('');

    element.innerHTML = `<svg class="gcr-map" viewBox="70 45 900 535" role="img" aria-label="Mapa de las diez Gerencias de Control Regional con proyectos de la cartera">${paths}</svg><div class="gcr-tooltip" id="gcrTooltip"></div>`;
    const tooltip = element.querySelector('#gcrTooltip');
    element.querySelectorAll('.gcr-region').forEach((path) => {
      const id = path.dataset.gcr;
      const total = totals[id];
      const show = (event) => {
        const box = element.getBoundingClientRect();
        tooltip.innerHTML = `<strong>${GCR_NAMES[id]}</strong><span>${total.count} proyecto(s) · ${total.mw.toLocaleString('es-MX')} MW</span><span>${total.strategic} estratégicos · ${total.private} particulares 2</span>`;
        tooltip.style.left = `${Math.min(event.clientX - box.left + 12, box.width - 255)}px`;
        tooltip.style.top = `${Math.max(8, event.clientY - box.top - 18)}px`;
        tooltip.classList.add('is-visible');
      };
      path.addEventListener('mousemove', show);
      path.addEventListener('mouseleave', () => tooltip.classList.remove('is-visible'));
      path.addEventListener('click', () => { $('#regionFilter').value = GCR_NAMES[id]; render(); });
      path.addEventListener('keydown', (event) => {
        if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); path.click(); }
      });
    });
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
    document.getElementById('trackingInterconnection').textContent = project.interconnectionPoint || project.substation || '—';
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
      button.onclick = () => alert(`La ficha institucional de ${button.dataset.ficha} se conectará al generador PAM.`);
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

  trackingModal.querySelectorAll('[data-close-tracking]').forEach((button) => button.addEventListener('click', closeTracking));
  trackingModal.querySelectorAll('[data-status]').forEach((button) => button.addEventListener('click', () => saveStatus(button)));
  document.querySelectorAll('[data-close-project]').forEach((button) => button.addEventListener('click', () => projectDialog.close()));
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && !trackingModal.hidden) closeTracking();
  });

  ['#projectSearch', '#typeFilter', '#regionFilter', '#stateFilter', '#decisionFilter', '#analysisFilter']
    .forEach((selector) => $(selector).addEventListener('input', render));

  $$('[data-view]').forEach((button) => {
    button.onclick = () => {
      $$('[data-view]').forEach((item) => item.classList.toggle('is-active', item === button));
      $$('[data-panel]').forEach((panel) => { panel.hidden = panel.dataset.panel !== button.dataset.view; });
    };
  });

  root.addEventListener('click', (event) => {
    const button = event.target.closest('[data-action]');
    if (!button) return;
    const action = button.dataset.action;
    if (action === 'new-project') projectDialog.showModal();
    if (action === 'clear-filters') {
      $$('#projectSearch,#typeFilter,#regionFilter,#stateFilter,#decisionFilter,#analysisFilter').forEach((field) => { field.value = ''; });
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
        projects: data.projects || [],
        notes: data.notes || [],
        session: data.session || {}
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
