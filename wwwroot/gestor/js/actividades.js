import { escape, fmtDate, semaforo, daysBetween, daysFromToday, openModal, toast, downloadCsv } from './utils.js';
import { dataService } from './data-service.js';

const DEFAULT_PAGE_SIZE = 8;
let tablaPage = 1;
let tablaPageSize = DEFAULT_PAGE_SIZE;

function normalizeUserName(value) {
    return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .toLowerCase();
}

function buildFilters(filters = {}) {
    return {
        search: (filters.search || '').trim().toLowerCase(),
        temaId: String(filters.temaId || '').trim(), // This holds selected Activity ID
        estatus: (filters.estatus || '').trim(),
        prioridad: (filters.prioridad || '').trim(),
        responsable: (filters.responsable || '').trim()
    };
}

function populateTemasFilter(actividades) {
    const sel = document.getElementById('filtro-tabla-tema');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const opts = actividades
        .slice()
        .sort((a, b) => String(a.actividad || '').localeCompare(String(b.actividad || ''), 'es-MX', { sensitivity: 'base' }))
        .map(a => `<option value="${a.id}">${escape(a.actividad || 'Sin actividad')}</option>`)
        .join('');

    sel.innerHTML = '<option value="">Actividad</option>' + opts;

    if (prevValue && actividades.some(a => String(a.id) === prevValue)) {
        sel.value = prevValue;
    }
}

function populateResponsablesFilter(temas) {
    const sel = document.getElementById('filtro-tabla-responsable');
    if (!sel) {
        return;
    }

    const prevValue = sel.value;
    const set = new Set();
    temas.forEach(t => {
        if (t.responsable) set.add(t.responsable.trim());
        if (t.corresponsables) t.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    const responsables = [...set].sort((a, b) => a.localeCompare(b, 'es-MX', { sensitivity: 'base' }));

    sel.innerHTML = '<option value="">Responsable</option>'
        + responsables.map(r => `<option value="${escape(r)}">${escape(r)}</option>`).join('');

    if (prevValue && responsables.includes(prevValue)) {
        sel.value = prevValue;
    }
}

function filterActividades(actividades, temas, rawFilters = {}) {
    const filters = buildFilters(rawFilters);

    return temas.filter(t => {
        const actividad = actividades.find(a => a.id === t.actividadId);
        const corrSearchNames = t.corresponsables ? t.corresponsables.map(c => c.nombre).join(' ') : '';
        const matchesSearch = !filters.search || [t.tema, t.responsable, t.estatus, t.prioridad, actividad?.actividad, corrSearchNames]
            .some(x => (x || '').toLowerCase().includes(filters.search));
        const matchesActividad = !filters.temaId || String(t.actividadId) === filters.temaId;
        const matchesEstatus = !filters.estatus || t.estatus === filters.estatus;
        const matchesPrioridad = !filters.prioridad || t.prioridad === filters.prioridad;
        const matchesResponsable = !filters.responsable || t.responsable === filters.responsable || (t.corresponsables && t.corresponsables.some(c => c.nombre === filters.responsable));

        return matchesSearch && matchesActividad && matchesEstatus && matchesPrioridad && matchesResponsable;
    });
}

function tiempoSemaforoData(tema) {
    const sem = semaforo(tema);
    const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';

    const rest = daysFromToday(tema.fechaCompromiso);
    const hasRange = !!tema.fechaInicio && !!tema.fechaCompromiso;
    const totalDays = hasRange ? Math.max(1, daysBetween(tema.fechaInicio, tema.fechaCompromiso)) : null;

    let percent = 0;
    if (typeof totalDays === 'number' && totalDays > 0 && typeof rest === 'number') {
        const elapsed = Math.min(totalDays, Math.max(0, totalDays - rest));
        percent = Math.round((elapsed / totalDays) * 100);
    }

    let label = 'Sin fecha';
    if (tema.estatus === 'Concluida') {
        label = 'Concluida';
    } else if (tema.bloqueada) {
        label = 'Bloqueada';
    } else if (typeof rest === 'number') {
        label = rest < 0 ? `Vencida ${Math.abs(rest)}d` : `Quedan ${rest}d`;
    }

    const detail = typeof totalDays === 'number'
        ? `${percent}% del plazo`
        : 'Sin rango completo';

    return { sem, trackCls, percent, label, detail };
}

function renderPagination(total, currentPage, pageSize) {
    const totalPages = Math.max(1, Math.ceil(total / pageSize));
    const page = Math.min(Math.max(1, currentPage), totalPages);
    const info = document.getElementById('tabla-pagination-info');
    const label = document.getElementById('tabla-page-current');
    const prev = document.getElementById('tabla-page-prev');
    const next = document.getElementById('tabla-page-next');

    const start = total ? ((page - 1) * pageSize) + 1 : 0;
    const end = total ? Math.min(page * pageSize, total) : 0;

    if (info) {
        info.textContent = `Mostrando ${start} a ${end} de ${total} temas`;
    }
    if (label) {
        label.textContent = `${page} / ${totalPages}`;
    }
    if (prev) {
        prev.disabled = page <= 1;
    }
    if (next) {
        next.disabled = page >= totalPages;
    }

    return page;
}

function renderEtapasDetailRow(t, colSpan) {
    if (!t.etapas || !t.etapas.length) return '';
    
    const points = t.etapas.map((e, idx) => {
        const isCompleted = e.avance === 100 || e.estatus === 'Concluida';
        const isActive = t.etapas.find(x => x.avance < 100) === e || (t.etapas.every(x => x.avance === 100) && idx === t.etapas.length - 1);
        
        let colorClass = 'timeline-pending';
        let badgeText = 'Pendiente';
        let badgeBg = '#e2e8f0';
        let badgeColor = '#475569';
        
        if (isCompleted) {
            colorClass = 'timeline-completed';
            badgeText = 'Concluida';
            badgeBg = 'rgba(2, 122, 72, 0.1)';
            badgeColor = '#027a48';
        } else if (isActive) {
            colorClass = 'timeline-active';
            badgeText = 'En curso';
            badgeBg = 'rgba(217, 119, 6, 0.1)';
            badgeColor = '#d97706';
        }
        
        let daysInfo = '';
        if (isCompleted) {
            daysInfo = 'Concluida';
        } else {
            const rest = daysFromToday(e.fechaCompromiso);
            if (typeof rest === 'number') {
                daysInfo = rest < 0 ? `Vencida hace ${Math.abs(rest)}d` : `Quedan ${rest}d`;
            }
        }

        const dateRange = e.fechaInicio 
            ? `${fmtDate(e.fechaInicio)} al ${fmtDate(e.fechaCompromiso)}`
            : `Compromiso: ${fmtDate(e.fechaCompromiso)}`;

        return `
            <div class="timeline-step ${colorClass}" style="flex: 1; min-width: 190px; position: relative; padding: 12px; border-radius: 10px; background: #fff; border: 1px solid rgba(138,0,49,0.1); box-shadow: 0 4px 10px rgba(0,0,0,0.02); display: flex; flex-direction: column; justify-content: space-between;">
                <div>
                    <div class="timeline-step-badge" style="display:inline-block; font-size:0.68rem; font-weight:700; padding: 2px 8px; border-radius: 6px; margin-bottom: 8px; background: ${badgeBg}; color: ${badgeColor};">${badgeText}</div>
                    <h5 style="margin: 0 0 6px 0; font-size: 0.82rem; font-weight: 700; color: var(--texto); font-family: Montserrat, sans-serif; line-height: 1.2;">${escape(e.nombre)}</h5>
                    <div style="font-size: 0.76rem; color: var(--texto-suave); display: flex; align-items: center; gap: 6px; margin-bottom: 4px;">
                        <i class="fa-solid fa-user-check" style="font-size: 0.72rem; color: var(--guinda);"></i>
                        <span style="font-weight: 500;">${escape(e.responsableNombre || 'Sin asignar')}</span>
                    </div>
                    <div style="font-size: 0.74rem; color: var(--texto-suave); margin-bottom: 8px;">${dateRange}</div>
                </div>
                <div>
                    <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 4px;">
                        <div class="track" style="flex:1; height: 6px; background:#e2e8f0; border-radius:3px; overflow:hidden;">
                            <span style="display:block; height:100%; width:${e.avance}%; background: ${isCompleted ? '#027a48' : isActive ? '#d97706' : '#667085'};"></span>
                        </div>
                        <span style="font-size: 0.74rem; font-weight: 700; color: var(--texto);">${e.avance}%</span>
                    </div>
                    <div style="font-size: 0.72rem; font-weight: 700; color: ${isCompleted ? '#027a48' : isActive ? (daysInfo.startsWith('Vencida') ? '#c0222a' : '#d97706') : '#667085'};">${daysInfo}</div>
                </div>
            </div>
        `;
    }).join(`
        <div class="timeline-connector" style="width: 28px; display: flex; align-items: center; justify-content: center; flex-shrink: 0;">
            <i class="fa-solid fa-chevron-right" style="font-size: 0.8rem; color: var(--guinda); opacity: 0.6;"></i>
        </div>
    `);

    return `
        <tr class="etapas-detail-row" id="etapas-detail-${t.id}" style="display:none; background: rgba(138, 0, 49, 0.015);">
            <td colspan="${colSpan}" style="padding: 16px 24px; border-bottom: 1px solid rgba(138,0,49,0.08);">
                <div style="font-weight: 700; font-size: 0.8rem; color: var(--guinda); text-transform: uppercase; margin-bottom: 12px; display: flex; align-items: center; gap: 8px; font-family: Montserrat, sans-serif; letter-spacing: 0.5px;">
                    <i class="fa-solid fa-route" style="font-size: 0.9rem;"></i> Secuencia y Trazabilidad de Etapas (${t.etapas.length})
                </div>
                <div class="timeline-container" style="display: flex; gap: 6px; overflow-x: auto; padding: 4px 0 12px 0; align-items: stretch;">
                    ${points}
                </div>
            </td>
        </tr>
    `;
}

export function renderTabla(temas, actividades, filters = {}) {
    // Note: We swap the arguments inside app.js call as well
    populateTemasFilter(actividades);
    populateResponsablesFilter(temas);

    const filtered = filterActividades(actividades, temas, filters);
    const totalPages = Math.max(1, Math.ceil(filtered.length / tablaPageSize));
    tablaPage = Math.min(Math.max(1, tablaPage), totalPages);

    const startIndex = (tablaPage - 1) * tablaPageSize;
    const pageItems = filtered.slice(startIndex, startIndex + tablaPageSize);

    const tbody = document.querySelector('#tbl-actividades tbody');
    tbody.innerHTML = pageItems.length
        ? pageItems.map(t => {
            const actividad = actividades.find(a => a.id === t.actividadId);
            const sem = semaforo(t);
            const tiempo = tiempoSemaforoData(t);
            const coText = t.corresponsables && t.corresponsables.length
                ? `<br><small class="muted" style="font-size: 0.72rem; display: block; margin-top: 2px;">Co: ${escape(t.corresponsables.map(c => c.nombre.split(' ')[0]).join(', '))}</small>`
                : '';
            
            const hasEtapas = t.etapas && t.etapas.length > 0;
            const expandBtn = hasEtapas 
                ? `<button class="btn-toggle-etapas" data-id="${t.id}" style="background:none; border:none; padding: 2px 6px; cursor:pointer; font-size: 0.8rem; color: var(--guinda); display: inline-flex; align-items: center; margin-right: 4px;" title="Ver etapas"><i class="fa-solid fa-chevron-right" style="transition: transform 0.2s; display: inline-block;"></i></button>`
                : '';

            const mainRow = `
                <tr data-id="${t.id}">
                    <td>${escape(actividad?.actividad || '—')}</td>
                    <td>${expandBtn} <span>${escape(t.tema)}</span></td>
                    <td>${escape(t.responsable)}${coText}</td>
                    <td>${fmtDate(t.fechaInicio)}</td>
                    <td>${fmtDate(t.fechaCompromiso)}</td>
                    <td><span class="status-pill ${t.estatus === 'Concluida' ? 'status-pill--complete' : t.estatus === 'En proceso' ? 'status-pill--progress' : t.estatus === 'Vencida' ? 'status-pill--issue' : ''}">${escape(t.estatus)}</span></td>
                    <td>
                        <div class="tiempo-cell">
                            <div class="track ${tiempo.trackCls}" style="width:110px"><span style="width:${tiempo.percent}%"></span></div>
                            <small class="muted">${escape(tiempo.label)} · ${escape(tiempo.detail)}</small>
                        </div>
                    </td>
                    <td><div class="track ${sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending'}" style="width:90px"><span style="width:${t.avance || 0}%"></span></div><small class="muted">${t.avance || 0}%</small></td>
                    <td><span class="chip ${t.prioridad === 'Alta' ? 'p-alta' : t.prioridad === 'Media' ? 'p-media' : 'p-baja'}">${escape(t.prioridad)}</span></td>
                    <td><span class="semaforo ${sem}"></span></td>
                    <td>
                        <span class="tabla-actions-cell">
                            ${t.evidenciaUrl ? `<a href="${escape(t.evidenciaUrl)}" target="_blank" rel="noopener" class="tabla-link-btn" title="Abrir evidencia" style="margin-right: 8px; display: inline-flex; align-items: center;"><i class="fa-solid fa-folder-open" style="font-size: 1.15rem; color: #b48934;"></i></a>` : ''}
                            <button class="tabla-action-btn" data-email="${t.id}" title="Enviar por correo" style="background: none; border: none; padding: 4px 8px; cursor: pointer; margin-right: 4px; display: inline-flex; align-items: center; border-radius: 4px; transition: background 0.2s;">
                                <i class="fa-solid fa-envelope" style="font-size: 1.15rem; color: #8a0031;"></i>
                            </button>
                            <button class="tabla-action-btn" data-whatsapp="${t.id}" title="Enviar por WhatsApp" style="background: none; border: none; padding: 4px 8px; cursor: pointer; margin-right: 4px; display: inline-flex; align-items: center; border-radius: 4px; transition: background 0.2s;">
                                <i class="fa-brands fa-whatsapp" style="font-size: 1.15rem; color: #128C7E;"></i>
                            </button>
                            <button class="internal-button" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem;margin-right: 4px;" data-view="${t.id}">Ver</button>
                            <button class="internal-button" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem" data-edit="${t.id}">Editar</button>
                        </span>
                    </td>
                </tr>`;
            
            return mainRow + renderEtapasDetailRow(t, 11);
        }).join('')
        : '<tr><td colspan="11" style="text-align:center;color:var(--g-text-soft);padding:1.5rem">Sin temas</td></tr>';

    tablaPage = renderPagination(filtered.length, tablaPage, tablaPageSize);

    tbody.querySelectorAll('[data-view]').forEach(btn => {
        btn.onclick = () => openTemaDetalle(temas.find(t => t.id === btn.dataset.view), actividades);
    });

    tbody.querySelectorAll('[data-edit]').forEach(btn => {
        btn.onclick = () => openTemaModal(temas.find(t => t.id === btn.dataset.edit), actividades);
    });

    tbody.querySelectorAll('[data-email]').forEach(btn => {
        btn.onclick = () => openSendEmailModal(temas.find(t => t.id === btn.dataset.email));
    });

    tbody.querySelectorAll('[data-whatsapp]').forEach(btn => {
        btn.onclick = () => {
            const tema = temas.find(t => t.id === btn.dataset.whatsapp);
            if (tema) openTemaWhatsAppModal(tema, actividades);
        };
    });

    tbody.querySelectorAll('.btn-toggle-etapas').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const id = btn.dataset.id;
            const detailRow = document.getElementById(`etapas-detail-${id}`);
            const icon = btn.querySelector('i');
            if (detailRow) {
                const isHidden = detailRow.style.display === 'none';
                detailRow.style.display = isHidden ? 'table-row' : 'none';
                if (icon) {
                    icon.style.transform = isHidden ? 'rotate(90deg)' : 'rotate(0deg)';
                }
            }
        };
    });
}

function buildTemaWhatsAppText(tema, actividad) {
    const gestorUrl = `${window.location.origin}/Gestor/Index`;
    const sem = semaforo(tema);
    const av = tema.avance || 0;

    const total = 10;
    const activeCount = Math.round(Math.max(0, Math.min(100, av)) / 10);
    const inactiveCount = total - activeCount;
    let emoji = '🟩';
    if (sem === 'rojo') emoji = '🟥';
    else if (sem === 'amarillo') emoji = '🟨';
    else if (sem === 'gris') emoji = '⬜';
    const bar = emoji.repeat(activeCount) + '⬜'.repeat(inactiveCount);

    const fCompromiso = fmtDate(tema.fechaCompromiso);
    const fInicio = fmtDate(tema.fechaInicio);

    const corresponsables = tema.corresponsables?.length
        ? tema.corresponsables.map(c => c.nombre).join(', ')
        : '';

    return [
        '🏛️ *SENER | Gestor de Actividades*',
        '━━━━━━━━━━━━━━━━━━━━',
        `📋 *Tema:* ${tema.tema}`,
        `📌 *Actividad:* ${actividad?.actividad || '—'}`,
        `👤 *Responsable:* ${tema.responsable || 'Sin responsable'}`,
        corresponsables ? `👥 *Corresponsables:* ${corresponsables}` : '',
        `📅 *Inicio:* ${fInicio}`,
        `📅 *Compromiso:* ${fCompromiso}`,
        `⚡ *Prioridad:* ${tema.prioridad || '—'}`,
        `🔄 *Estatus:* ${tema.estatus || '—'}`,
        `📊 *Avance:* ${bar} (${av}%)`,
        tema.descripcion ? `📝 *Descripción:* ${tema.descripcion}` : '',
        tema.comentarios ? `💬 *Comentarios:* ${tema.comentarios}` : '',
        '━━━━━━━━━━━━━━━━━━━━',
        '🔗 *Ver en el Gestor:*',
        gestorUrl
    ].filter(x => x !== '').join('\n');
}

function wrapCanvasText(ctx, text, x, y, maxWidth, lineHeight, maxLines = 3) {
    const words = String(text || '').split(/\s+/);
    let line = '';
    let lines = 0;
    for (const word of words) {
        const testLine = line ? `${line} ${word}` : word;
        if (ctx.measureText(testLine).width > maxWidth && line) {
            ctx.fillText(line, x, y);
            y += lineHeight;
            lines++;
            line = word;
            if (lines >= maxLines - 1) break;
        } else {
            line = testLine;
        }
    }
    if (line && lines < maxLines) ctx.fillText(line, x, y);
}

function downloadTemaShareImage(tema, actividad) {
    const rows = [
        ['Actividad', actividad?.actividad || '—'],
        ['Tema', tema.tema],
        ['Responsable', tema.responsable || 'Sin responsable'],
        ['Compromiso', tema.fechaCompromiso || 'Sin fecha'],
        ['Estatus', tema.estatus || '—'],
        ['Prioridad', tema.prioridad || '—'],
        ['Avance', `${tema.avance || 0}%`]
    ];

    const canvas = document.createElement('canvas');
    canvas.width = 1200;
    canvas.height = 760;
    const ctx = canvas.getContext('2d');

    ctx.fillStyle = '#f5f1ea';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(56, 56, 1088, 648);
    ctx.fillStyle = '#8a0031';
    ctx.fillRect(56, 56, 1088, 104);
    ctx.fillStyle = '#b48934';
    ctx.fillRect(56, 160, 1088, 8);

    ctx.fillStyle = '#ffffff';
    ctx.font = '700 30px Arial';
    ctx.fillText('SENER | Gestor de Actividades DGMESNIE', 92, 116);
    ctx.font = '600 18px Arial';
    ctx.fillText('Ficha institucional de tema', 92, 144);

    ctx.fillStyle = '#111827';
    ctx.font = '700 28px Arial';
    wrapCanvasText(ctx, tema.tema || 'Tema sin nombre', 92, 220, 1016, 34, 2);

    let y = 310;
    rows.filter(([label]) => label !== 'Tema').forEach(([label, value]) => {
        ctx.fillStyle = '#f7ecf1';
        ctx.fillRect(92, y - 26, 300, 42);
        ctx.fillStyle = '#6b1034';
        ctx.font = '700 20px Arial';
        ctx.fillText(label, 112, y);
        ctx.fillStyle = '#1f2937';
        ctx.font = '500 20px Arial';
        wrapCanvasText(ctx, String(value || '—'), 420, y, 660, 24, 2);
        y += 70;
    });

    ctx.fillStyle = '#8a0031';
    ctx.font = '700 19px Arial';
    ctx.fillText(`${window.location.origin}/Gestor/Index`, 92, 668);
    ctx.fillStyle = '#6b7280';
    ctx.font = '500 16px Arial';
    ctx.fillText('Generado automáticamente desde el Gestor de Actividades', 92, 694);

    downloadCanvasAsPng(canvas, `tema-${tema.id || 'gestor'}.png`);
}

function downloadCanvasAsPng(canvas, filename) {
    canvas.toBlob((blob) => {
        if (!blob) {
            toast('No fue posible generar la imagen.', 'err');
            return;
        }

        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename.endsWith('.png') ? filename : `${filename}.png`;
        link.type = 'image/png';
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }, 'image/png');
}

function openTemaWhatsAppModal(tema, actividades) {
    const actividad = actividades.find(a => a.id === tema.actividadId);
    const text = buildTemaWhatsAppText(tema, actividad);
    const waUrl = `https://wa.me/?text=${encodeURIComponent(text)}`;

    const html = `
        <div style="display:flex;flex-direction:column;gap:14px;">
            <div style="border:1px solid rgba(138,0,49,.16);border-radius:10px;overflow:hidden;background:#fff;">
                <div style="background:#8a0031;color:#fff;padding:12px 14px;font-weight:800;">SENER | Gestor de Actividades DGMESNIE</div>
                <table style="width:100%;border-collapse:collapse;font-size:.84rem;">
                    <tbody>
                        <tr><th style="width:34%;text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Tema</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;font-weight:700;">${escape(tema.tema)}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Actividad</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad?.actividad || '—')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Responsable</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(tema.responsable || 'Sin responsable')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Compromiso</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(tema.fechaCompromiso || 'Sin fecha')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Estatus</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(tema.estatus || '—')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;">Avance</th><td style="padding:8px 10px;">${tema.avance || 0}%</td></tr>
                    </tbody>
                </table>
            </div>
            <textarea readonly style="width:100%;min-height:150px;border:1px solid rgba(138,0,49,.18);border-radius:8px;padding:10px;font-size:.82rem;resize:vertical;box-sizing:border-box;">${escape(text)}</textarea>
            <div style="display:flex;justify-content:flex-end;gap:10px;flex-wrap:wrap;">
                <button type="button" id="btn-descargar-wa-tema-img" class="internal-button">Descargar imagen</button>
                <a href="${waUrl}" target="_blank" rel="noopener" class="internal-button internal-button--primary" style="text-decoration:none;">Abrir WhatsApp</a>
            </div>
        </div>`;

    openModal('Compartir tema por WhatsApp', html, null);
    document.getElementById('btn-descargar-wa-tema-img').onclick = () => downloadTemaShareImage(tema, actividad);
}

export function changeTablaPage(step) {
    tablaPage = Math.max(1, tablaPage + step);
}

export function resetTablaPage() {
    tablaPage = 1;
}

export function setTablaPageSize(size) {
    const parsed = Number(size);
    if (!Number.isFinite(parsed) || parsed <= 0) {
        tablaPageSize = DEFAULT_PAGE_SIZE;
    } else {
        tablaPageSize = parsed;
    }
    tablaPage = 1;
}

export async function openTemaModal(tema, actividades) {
    const t = tema || {
        actividadId: actividades[0]?.id || '', tema: '', descripcion: '', responsable: '', responsableId: '',
        fechaInicio: '', fechaCompromiso: '', estatus: 'Pendiente', prioridad: 'Media',
        avance: 0, bloqueada: false, motivoBloqueo: '', evidenciaUrl: '', comentarios: '', corresponsablesIds: [],
        etapas: []
    };
    if (!t.corresponsablesIds) t.corresponsablesIds = [];
    if (!t.etapas) t.etapas = [];

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(t.responsableId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === t.responsable);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? '');
    
    const responsableOptions = [
        '<option value="">Selecciona responsable</option>',
        ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(selectedUserId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !tema;

    // Etapas state
    let currentEtapas = t.etapas ? JSON.parse(JSON.stringify(t.etapas)) : [];
    if (!currentEtapas.length && !isNew) {
        currentEtapas.push({
            etapaId: null,
            nombre: 'Etapa Inicial',
            responsableId: selectedUserId || '',
            fechaInicio: t.fechaInicio || '',
            fechaCompromiso: t.fechaCompromiso || '',
            avance: t.avance ?? 0,
            estatus: t.estatus || 'Pendiente'
        });
    }

    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Actividad *</label>
                    <select name="actividadId" required>${actividades.map(a => `<option value="${a.id}" ${t.actividadId === a.id ? 'selected' : ''}>${escape(a.actividad)}</option>`).join('')}</select>
                </div>
                <div class="form-field full"><label>Tema *</label><input name="tema" required value="${escape(t.tema)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(t.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Responsable *</label><select name="responsableId" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${t.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${t.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${t.fechaCompromiso || ''}"></div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Pendiente', 'En proceso', 'Concluida', 'Vencida'].map(p => `<option ${t.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Bloqueada</label>
                    <select name="bloqueada"><option value="false" ${!t.bloqueada ? 'selected' : ''}>No</option><option value="true" ${t.bloqueada ? 'selected' : ''}>Sí</option></select>
                </div>
                <div class="form-field"><label>Motivo bloqueo</label><input name="motivoBloqueo" value="${escape(t.motivoBloqueo || '')}"></div>
                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Corresponsables</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label class="corresponsable-label" data-id="${u.idUsuario}" style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="corresponsablesIds" value="${u.idUsuario}" ${t.corresponsablesIds.includes(u.idUsuario) ? 'checked' : ''} style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
                <div class="form-field full"><label>Evidencia URL</label><input type="url" name="evidenciaUrl" value="${escape(t.evidenciaUrl || '')}"></div>
                <div class="form-field full"><label>Comentarios</label><textarea name="comentarios">${escape(t.comentarios || '')}</textarea></div>
                
                <!-- Dynamic Stages Subform -->
                <div class="form-field full" style="margin-top: 15px; border-top: 1px solid rgba(138, 0, 49, 0.15); padding-top: 15px;">
                    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:12px;">
                        <label style="font-weight: 700; color: var(--guinda); font-size:0.95rem; margin:0;">Etapas del Tema</label>
                        <button type="button" class="internal-button" id="btn-add-etapa" style="padding: 2px 10px; min-height: 28px; font-size: 0.75rem;">+ Agregar Etapa</button>
                    </div>
                    <div id="etapas-list-container" style="display: flex; flex-direction: column; gap: 10px;">
                        <!-- Stages render dynamically here -->
                    </div>
                </div>

                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Enviar notificación por correo a:</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="notificarUsuariosIds" value="${u.idUsuario}" style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
            </div>
            <!-- Error Alert Area -->
            <div id="tema-error-alert" style="display:none; color:var(--riesgo); background:rgba(192,34,42,0.06); border:1px solid rgba(192,34,42,0.15); border-radius:8px; padding:10px 14px; font-size:0.82rem; font-weight:600; margin-top:15px; margin-bottom:5px; align-items:center; gap:8px;">
                <i class="fa-solid fa-triangle-exclamation" style="color:var(--riesgo);"></i>
                <span class="error-msg"></span>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-tema">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;

    const close = openModal(isNew ? 'Nuevo tema' : 'Editar tema', html, async (data, close) => {
        const submitBtn = document.querySelector('#modal-body form button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = isNew ? 'Creando...' : 'Guardando...';
        }
        try {
            const errorAlert = document.getElementById('tema-error-alert');
            if (errorAlert) {
                errorAlert.style.display = 'none';
            }
            saveCurrentInputsState();
            
            if (currentEtapas.length === 0) {
                throw new Error('Debe agregar al menos una etapa para el tema.');
            }

             for (let i = 0; i < currentEtapas.length; i++) {
                const stg = currentEtapas[i];
                if (!stg.nombre.trim()) throw new Error(`La Etapa ${i + 1} debe tener un nombre.`);
                if (!stg.responsableId) throw new Error(`La Etapa ${i + 1} (${stg.nombre || 'sin nombre'}) debe tener un responsable asignado.`);
                if (!stg.fechaCompromiso) throw new Error(`La Etapa ${i + 1} (${stg.nombre}) debe tener una fecha compromiso.`);

                if (stg.fechaInicio && stg.fechaCompromiso && stg.fechaInicio > stg.fechaCompromiso) {
                    throw new Error(`La fecha de inicio de la Etapa ${i + 1} (${stg.nombre}) no puede ser posterior a su propia fecha compromiso.`);
                }

                if (i > 0) {
                    const prev = currentEtapas[i - 1];
                    // 1. Validar fechas secuenciales (fecha de inicio de la etapa actual no puede ser menor a la fecha compromiso de la anterior)
                    if (stg.fechaInicio && prev.fechaCompromiso && stg.fechaInicio < prev.fechaCompromiso) {
                        throw new Error(`La fecha de inicio de la Etapa ${i + 1} (${stg.nombre}) no puede ser anterior a la fecha compromiso de la Etapa ${i} (${prev.nombre}).`);
                    }
                    // 2. Validar estatus secuencial en cascada (etapa actual no puede estar En proceso o Concluida si la anterior no está Concluida)
                    if (stg.estatus !== 'Pendiente' && prev.estatus !== 'Concluida') {
                        throw new Error(`La Etapa ${i + 1} (${stg.nombre}) no puede iniciar (en proceso/concluida) si la Etapa ${i} (${prev.nombre}) no está Concluida.`);
                    }
                }
            }

            data.etapas = currentEtapas;

            // Recalculate main properties on submit
            const sumAv = currentEtapas.reduce((s, st) => s + (st.avance || 0), 0);
            data.avance = Math.round(sumAv / currentEtapas.length);

            const startDates = currentEtapas.map(st => st.fechaInicio).filter(Boolean);
            const compDates = currentEtapas.map(st => st.fechaCompromiso).filter(Boolean);
            data.fechaInicio = startDates.length ? startDates.sort()[0] : null;
            data.fechaCompromiso = compDates.length ? compDates.sort().reverse()[0] : null;

            const activeStg = currentEtapas.find(st => st.avance < 100) || currentEtapas[currentEtapas.length - 1];
            if (activeStg) {
                data.responsableId = activeStg.responsableId;
            }

            let overallEst = 'En proceso';
            if (currentEtapas.every(st => st.avance === 100 || st.estatus === 'Concluida')) {
                overallEst = 'Concluida';
            } else if (currentEtapas.every(st => st.avance === 0 && st.estatus === 'Pendiente')) {
                overallEst = 'Pendiente';
            } else {
                const todayStr = new Date().toISOString().slice(0, 10);
                if (activeStg && activeStg.fechaCompromiso && activeStg.fechaCompromiso < todayStr) {
                    overallEst = 'Vencida';
                }
            }
            data.estatus = overallEst;

            data.bloqueada = data.bloqueada === 'true';
            data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);

            const corresponsablesCbs = document.querySelectorAll('#modal-body form input[name="corresponsablesIds"]:checked');
            data.corresponsablesIds = Array.from(corresponsablesCbs).map(cb => Number(cb.value));

            const checkboxes = document.querySelectorAll('#modal-body form input[name="notificarUsuariosIds"]:checked');
            data.notificarUsuariosIds = Array.from(checkboxes).map(cb => Number(cb.value));

            if (isNew) {
                await dataService.create('temas', data);
                toast('Tema creado', 'ok');
            } else {
                await dataService.update('temas', tema.id, data);
                toast('Tema actualizado', 'ok');
            }
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        } catch (err) {
            console.error(err);
            let userMsg = err.message || 'No fue posible guardar el tema.';
            if (userMsg.includes('blocked') || userMsg.includes('bloqueada')) {
                userMsg = 'Solicitud bloqueada por validación de seguridad.';
            }
            
            const errorAlert = document.getElementById('tema-error-alert');
            if (errorAlert) {
                errorAlert.querySelector('.error-msg').textContent = userMsg;
                errorAlert.style.display = 'flex';
                const container = errorAlert.closest('.modal-card');
                if (container) {
                    setTimeout(() => {
                        container.scrollTo({ top: container.scrollHeight, behavior: 'smooth' });
                    }, 50);
                }
            } else {
                toast(userMsg, 'err');
            }

            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = isNew ? 'Crear' : 'Guardar';
            }
        }
    });

    const respSelect = document.querySelector('#modal-body form select[name="responsableId"]');
    const fInicioInput = document.querySelector('#modal-body form input[name="fechaInicio"]');
    const fCompromisoInput = document.querySelector('#modal-body form input[name="fechaCompromiso"]');
    const estatusSelect = document.querySelector('#modal-body form select[name="estatus"]');

    let previousResponsibleId = respSelect ? respSelect.value : '';
    const updateCorresponsablesChecklist = () => {
        const selectedId = respSelect ? respSelect.value : '';
        if (previousResponsibleId && previousResponsibleId !== selectedId) {
            const prevLabel = document.querySelector(`#modal-body form .corresponsable-label[data-id="${previousResponsibleId}"]`);
            if (prevLabel) {
                const cb = prevLabel.querySelector('input[type="checkbox"]');
                if (cb) cb.checked = true;
            }
        }
        
        const labels = document.querySelectorAll('#modal-body form .corresponsable-label');
        labels.forEach(label => {
            const labelId = label.dataset.id;
            if (labelId === selectedId) {
                label.style.display = 'none';
                const input = label.querySelector('input[type="checkbox"]');
                if (input) input.checked = false;
            } else {
                label.style.display = 'flex';
            }
        });

        previousResponsibleId = selectedId;
    };

    const syncStagesToMain = () => {
        if (!currentEtapas.length) return;

        const sumAv = currentEtapas.reduce((s, st) => s + (st.avance || 0), 0);
        const overallAv = Math.round(sumAv / currentEtapas.length);

        const startDates = currentEtapas.map(st => st.fechaInicio).filter(Boolean);
        const compDates = currentEtapas.map(st => st.fechaCompromiso).filter(Boolean);
        const overallIni = startDates.length ? startDates.sort()[0] : '';
        const overallComp = compDates.length ? compDates.sort().reverse()[0] : '';

        const activeStg = currentEtapas.find(st => st.avance < 100) || currentEtapas[currentEtapas.length - 1];
        const overallResp = activeStg ? activeStg.responsableId : '';

        let overallEst = 'En proceso';
        if (currentEtapas.every(st => st.avance === 100 || st.estatus === 'Concluida')) {
            overallEst = 'Concluida';
        } else if (currentEtapas.every(st => st.avance === 0 && st.estatus === 'Pendiente')) {
            overallEst = 'Pendiente';
        } else {
            const todayStr = new Date().toISOString().slice(0, 10);
            if (activeStg && activeStg.fechaCompromiso && activeStg.fechaCompromiso < todayStr) {
                overallEst = 'Vencida';
            }
        }

        if (respSelect) {
            respSelect.value = overallResp;
            respSelect.disabled = currentEtapas.length > 1;
        }
        if (fInicioInput) {
            fInicioInput.value = overallIni;
            fInicioInput.readOnly = currentEtapas.length > 1;
        }
        if (fCompromisoInput) {
            fCompromisoInput.value = overallComp;
            fCompromisoInput.readOnly = currentEtapas.length > 1;
        }
        if (estatusSelect) {
            estatusSelect.value = overallEst;
            estatusSelect.disabled = currentEtapas.length > 1;
        }

        updateCorresponsablesChecklist();
    };

    const syncMainToStage = () => {
        if (currentEtapas.length !== 1) return;
        const first = currentEtapas[0];
        if (respSelect) first.responsableId = respSelect.value;
        if (fInicioInput) first.fechaInicio = fInicioInput.value || null;
        if (fCompromisoInput) first.fechaCompromiso = fCompromisoInput.value || null;
        if (estatusSelect) {
            const val = estatusSelect.value;
            if (val === 'Concluida') {
                first.estatus = 'Concluida';
                first.avance = 100;
            } else if (val === 'Pendiente') {
                first.estatus = 'Pendiente';
                first.avance = 0;
            } else {
                first.estatus = 'En proceso';
                first.avance = 50;
            }
        }
        updateEtapasView();
    };

    const saveCurrentInputsState = () => {
        const container = document.getElementById('etapas-list-container');
        if (!container) return;
        container.querySelectorAll('.etapa-row-card').forEach(card => {
            const idx = Number(card.dataset.index);
            const idVal = card.querySelector('input[name="etapaId"]').value;
            currentEtapas[idx] = {
                etapaId: idVal ? Number(idVal) : null,
                nombre: card.querySelector('input[name="etapaNombre"]').value,
                responsableId: Number(card.querySelector('select[name="etapaResponsableId"]').value),
                fechaInicio: card.querySelector('input[name="etapaFechaInicio"]').value || null,
                fechaCompromiso: card.querySelector('input[name="etapaFechaCompromiso"]').value || null,
                avance: Number(card.querySelector('input[name="etapaAvance"]').value),
                estatus: card.querySelector('select[name="etapaEstatus"]').value
            };
        });
    };

    const renderEtapaRow = (etapa, index) => {
        const usersOptionsForEtapa = [
            '<option value="">Selecciona responsable</option>',
            ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(etapa.responsableId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
        ].join('');

        return `
            <div class="etapa-row-card" data-index="${index}" style="border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 12px; background: #faf8f9; display: flex; flex-direction: column; gap: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.02);">
                <input type="hidden" name="etapaId" value="${etapa.etapaId || ''}">
                <div style="display: flex; gap: 8px; align-items: center; justify-content: space-between;">
                    <span style="font-weight: 700; font-size: 0.8rem; color: var(--guinda);">Etapa ${index + 1}</span>
                    <div style="display: flex; gap: 6px; align-items: center;">
                        <button type="button" class="btn-move-up-etapa" title="Subir" style="background:none; border:none; padding: 2px; cursor:pointer;"><i class="fa-solid fa-arrow-up" style="font-size:0.8rem; color:#8a0031;"></i></button>
                        <button type="button" class="btn-move-down-etapa" title="Bajar" style="background:none; border:none; padding: 2px; cursor:pointer;"><i class="fa-solid fa-arrow-down" style="font-size:0.8rem; color:#8a0031;"></i></button>
                        <button type="button" class="btn-del-etapa" title="Eliminar" style="background:none; border:none; padding: 2px; cursor:pointer;"><i class="fa-solid fa-trash" style="font-size:0.8rem; color:var(--riesgo);"></i></button>
                    </div>
                </div>
                <div style="display: grid; grid-template-columns: 2fr 1.5fr; gap: 8px;">
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Nombre etapa *</label>
                        <input type="text" name="etapaNombre" required style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box;" value="${escape(etapa.nombre || '')}">
                    </div>
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Responsable *</label>
                        <select name="etapaResponsableId" required style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box;">
                            ${usersOptionsForEtapa}
                        </select>
                    </div>
                </div>
                <div style="display: grid; grid-template-columns: 1fr 1fr 0.8fr 1.2fr; gap: 8px;">
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Inicio</label>
                        <input type="date" name="etapaFechaInicio" style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box;" value="${etapa.fechaInicio || ''}">
                    </div>
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Compromiso *</label>
                        <input type="date" name="etapaFechaCompromiso" required style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box;" value="${etapa.fechaCompromiso || ''}">
                    </div>
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Avance (%)</label>
                        <input type="number" min="0" max="100" name="etapaAvance" readonly style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box; background-color:#f1f5f9; color:#64748b;" value="${etapa.avance ?? 0}">
                    </div>
                    <div class="form-field full" style="margin:0;"><label style="font-size:0.75rem; margin-bottom: 2px;">Estatus</label>
                        <select name="etapaEstatus" style="height:32px; padding:4px 8px; font-size:0.8rem; border-radius:6px; border:1px solid #ccc; width:100%; box-sizing:border-box;">
                            <option value="Pendiente" ${etapa.estatus === 'Pendiente' ? 'selected' : ''}>Pendiente</option>
                            <option value="En proceso" ${etapa.estatus === 'En proceso' ? 'selected' : ''}>En proceso</option>
                            <option value="Concluida" ${etapa.estatus === 'Concluida' ? 'selected' : ''}>Concluida</option>
                        </select>
                    </div>
                </div>
            </div>
        `;
    };

    const updateEtapasView = () => {
        const container = document.getElementById('etapas-list-container');
        if (!container) return;
        container.innerHTML = currentEtapas.map((e, idx) => renderEtapaRow(e, idx)).join('');

        container.querySelectorAll('.etapa-row-card').forEach(card => {
            const idx = Number(card.dataset.index);
            
            card.querySelector('.btn-del-etapa').onclick = () => {
                if (currentEtapas.length <= 1) {
                    toast('Un tema debe tener al menos una etapa.', 'err');
                    return;
                }
                currentEtapas.splice(idx, 1);
                updateEtapasView();
                syncStagesToMain();
            };

            card.querySelector('.btn-move-up-etapa').onclick = () => {
                if (idx === 0) return;
                saveCurrentInputsState();
                const temp = currentEtapas[idx];
                currentEtapas[idx] = currentEtapas[idx - 1];
                currentEtapas[idx - 1] = temp;
                updateEtapasView();
                syncStagesToMain();
            };

            card.querySelector('.btn-move-down-etapa').onclick = () => {
                if (idx === currentEtapas.length - 1) return;
                saveCurrentInputsState();
                const temp = currentEtapas[idx];
                currentEtapas[idx] = currentEtapas[idx + 1];
                currentEtapas[idx + 1] = temp;
                updateEtapasView();
                syncStagesToMain();
            };

            const inputAv = card.querySelector('input[name="etapaAvance"]');
            const selectEst = card.querySelector('select[name="etapaEstatus"]');
            
            const handleStageEstatusChange = () => {
                const est = selectEst.value;
                if (est === 'Concluida') {
                    inputAv.value = 100;
                } else if (est === 'En proceso') {
                    inputAv.value = 50;
                } else {
                    inputAv.value = 0;
                }
                saveCurrentInputsState();
                syncStagesToMain();
            };

            if (selectEst) {
                selectEst.onchange = handleStageEstatusChange;
            }

            card.querySelector('input[name="etapaNombre"]').oninput = () => { saveCurrentInputsState(); };
            card.querySelector('select[name="etapaResponsableId"]').onchange = () => { saveCurrentInputsState(); syncStagesToMain(); };
            card.querySelector('input[name="etapaFechaInicio"]').onchange = () => { saveCurrentInputsState(); syncStagesToMain(); };
            card.querySelector('input[name="etapaFechaCompromiso"]').onchange = () => { saveCurrentInputsState(); syncStagesToMain(); };
        });
    };

    updateEtapasView();
    syncStagesToMain();

    document.getElementById('btn-add-etapa').onclick = () => {
        saveCurrentInputsState();
        currentEtapas.push({
            etapaId: null,
            nombre: '',
            responsableId: selectedUserId || '',
            fechaInicio: '',
            fechaCompromiso: '',
            avance: 0,
            estatus: 'Pendiente'
        });
        updateEtapasView();
        syncStagesToMain();
    };

    if (respSelect) {
        respSelect.addEventListener('change', () => {
            syncMainToStage();
            updateCorresponsablesChecklist();
        });
    }
    if (fInicioInput) fInicioInput.addEventListener('change', syncMainToStage);
    if (fCompromisoInput) fCompromisoInput.addEventListener('change', syncMainToStage);
    if (estatusSelect) estatusSelect.addEventListener('change', syncMainToStage);

    updateCorresponsablesChecklist();

    if (!isNew) {
        document.getElementById('btn-del-tema').onclick = async () => {
            if (!confirm('¿Eliminar tema?')) return;
            await dataService.remove('temas', tema.id);
            toast('Tema eliminado', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}

export function exportarCsv(actividades, temas) {
    const rows = [['Actividad', 'Tema', 'Responsable', 'Inicio', 'Compromiso', 'Estatus', 'Avance', 'Prioridad', 'Bloqueada', 'Evidencia']];
    temas.forEach(t => {
        const a = actividades.find(x => x.id === t.actividadId);
        rows.push([a?.actividad, t.tema, t.responsable, t.fechaInicio, t.fechaCompromiso, t.estatus, t.avance, t.prioridad, t.bloqueada ? 'Sí' : 'No', t.evidenciaUrl]);
    });
    downloadCsv('temas.csv', rows);
    toast('CSV descargado', 'ok');
}

export async function openSendEmailModal(tema) {
    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const html = `
        <form id="compartir-email-form">
            <div class="form-row">
                <div class="form-field full">
                    <p style="margin-bottom: 12px; font-size: 0.9rem; color: var(--texto-suave);">
                        Seleccione uno o más usuarios para enviar los detalles del tema <strong>${escape(tema.clave)} - ${escape(tema.tema)}</strong> por correo electrónico:
                    </p>
                    <div class="usuarios-check-list" style="max-height: 220px; overflow-y: auto; border: 1px solid rgba(138, 0, 49, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="notificarUsuariosIds" value="${u.idUsuario}" style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)} ${u.correo ? `(${escape(u.correo)})` : ''}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
            </div>
            <div class="form-actions" style="margin-top: 15px;">
                <button type="button" class="internal-button" id="btn-cancel-share">Cancelar</button>
                <button type="submit" class="internal-button internal-button--primary">Enviar por correo</button>
            </div>
        </form>
    `;

    const close = openModal(`Enviar tema por correo`, html, null);
    
    document.getElementById('btn-cancel-share').onclick = close;

    const form = document.getElementById('compartir-email-form');
    form.onsubmit = async (e) => {
        e.preventDefault();
        const checkboxes = form.querySelectorAll('input[name="notificarUsuariosIds"]:checked');
        const userIds = Array.from(checkboxes).map(cb => Number(cb.value));

        if (userIds.length === 0) {
            toast('Debe seleccionar al menos un usuario.', 'err');
            return;
        }

        try {
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Enviando...';

            const token = document.querySelector('meta[name="csrf-token"]')?.content ?? '';

            const res = await fetch(`/Gestor/Api/Temas/${tema.id}/Notificar`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ usuarioIds: userIds })
            });

            if (!res.ok) {
                const body = await res.json().catch(() => null);
                throw new Error(body?.error || `Error ${res.status}`);
            }

            toast('Correo(s) enviado(s) con éxito', 'ok');
            close();
        } catch (err) {
            toast(err.message || 'No fue posible enviar el correo', 'err');
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.textContent = 'Enviar por correo';
        }
    };
}

export function openTemaDetalle(t, actividades) {
    if (!t) return;
    const actividad = actividades.find(a => a.id === t.actividadId);
    const sem = semaforo(t);
    const semLabels = { verde: '🟢 A tiempo', amarillo: '🟡 Por vencer', rojo: '🔴 Vencido', gris: '⚫ Bloqueado/Sin fecha' };

    const corrList = t.corresponsables && t.corresponsables.length
        ? t.corresponsables.map(c => `<span style="display:inline-flex;align-items:center;gap:5px;background:rgba(30,91,79,.08);border-radius:6px;padding:2px 8px;font-size:0.78rem;color:#1e5b4f;font-weight:600;">${escape(c.nombre)}</span>`).join(' ')
        : '<span style="color:var(--texto-suave);font-size:0.82rem;">Sin corresponsables</span>';

    // Línea de tiempo de etapas del tema
    let etapasTimelineHtml = '';
    if (t.etapas && t.etapas.length > 0) {
        const activeStage = t.etapas.find(e => e.avance < 100) || t.etapas[t.etapas.length - 1];
        
        etapasTimelineHtml = `
            <div style="background:#f8fafc;border-radius:8px;padding:14px;border:1px solid rgba(138,0,49,0.06);margin-top:10px;">
                <div style="font-size:0.7rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:12px;display:flex;align-items:center;gap:6px;">
                    <i class="fa-solid fa-route"></i> Secuencia y Trazabilidad de Etapas (${t.etapas.length})
                </div>
                <div style="position:relative;padding-left:22px;display:flex;flex-direction:column;gap:16px;">
                    <!-- Línea vertical -->
                    <div style="position:absolute;left:7px;top:6px;bottom:6px;width:2px;background:rgba(138,0,49,0.12);"></div>
                    
                    ${t.etapas.map((e, idx) => {
                        const isCompleted = e.avance === 100 || e.estatus === 'Concluida';
                        const isActive = e === activeStage;
                        const dotColor = isCompleted ? '#027a48' : isActive ? '#d97706' : '#667085';
                        
                        const dateText = e.fechaInicio && e.fechaCompromiso
                            ? `${fmtDate(e.fechaInicio)} al ${fmtDate(e.fechaCompromiso)}`
                            : e.fechaCompromiso ? `Fecha compromiso: ${fmtDate(e.fechaCompromiso)}` : 'Sin fechas';

                        return `
                            <div style="position:relative;display:flex;flex-direction:column;gap:3px;${isActive ? 'background:rgba(138,0,49,0.03);padding:6px 10px;border-radius:6px;border-left:3px solid #8a0031;margin-left:-10px;' : ''}">
                                <!-- Dot -->
                                <div style="position:absolute;left:${isActive ? '-20px' : '-22px'};top:4px;width:10px;height:10px;border-radius:50%;background:${dotColor};border:2px solid #fff;box-shadow:0 0 0 1px ${dotColor};z-index:1;"></div>
                                
                                <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;">
                                    <span style="font-weight:700;font-size:0.82rem;color:#0f172a;">Etapa ${idx + 1}: ${escape(e.nombre)}</span>
                                    <span style="font-size:0.65rem;font-weight:700;padding:1px 5px;border-radius:4px;background:${isCompleted ? 'rgba(2,122,72,0.1)' : isActive ? 'rgba(217,119,6,0.1)' : 'rgba(102,112,133,0.1)'};color:${dotColor};">${escape(e.estatus)}</span>
                                    ${isActive ? '<span style="font-size:0.62rem;font-weight:800;background:#8a0031;color:#fff;padding:1px 5px;border-radius:4px;text-transform:uppercase;letter-spacing:0.05em;display:inline-flex;align-items:center;gap:3px;"><i class="fa-solid fa-play"></i> Etapa Activa</span>' : ''}
                                </div>
                                
                                <div style="font-size:0.75rem;color:var(--texto-suave);font-weight:500;">
                                    <span>👤 Asignado a: <strong>${escape(e.responsableNombre || 'Sin asignar')}</strong></span>
                                    <span style="margin:0 6px;color:#cbd5e1;">|</span>
                                    <span>📅 ${dateText}</span>
                                    <span style="margin:0 6px;color:#cbd5e1;">|</span>
                                    <span>📊 Avance: <strong>${e.avance || 0}%</strong></span>
                                </div>
                            </div>
                        `;
                    }).join('')}
                </div>
            </div>`;
    }

    const html = `
        <div style="display:flex;flex-direction:column;gap:1rem;">
            <div style="display:flex;align-items:center;gap:10px;padding:10px 14px;border-radius:10px;background:rgba(138,0,49,0.04);border-left:4px solid #8a0031;">
                <span class="semaforo ${sem}" style="width:12px;height:12px;flex-shrink:0;margin-top:0;"></span>
                <div>
                    <div style="font-size:0.7rem;text-transform:uppercase;font-weight:700;color:#8a0031;letter-spacing:.05em;">Tema / Tarea</div>
                    <div style="font-weight:700;font-size:1rem;color:#0f172a;">${escape(t.tema)}</div>
                </div>
                <span style="margin-left:auto;font-size:0.78rem;font-weight:700;color:#8a0031;">${semLabels[sem] || sem}</span>
            </div>

            ${t.descripcion ? `<p style="margin:0;font-size:0.87rem;color:#475569;line-height:1.55;">${escape(t.descripcion)}</p>` : ''}

            <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Actividad Asociada</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(actividad?.actividad || '—')}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Responsable Vigente</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(t.responsable || '—')}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Inicio</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${fmtDate(t.fechaInicio)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Compromiso</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${fmtDate(t.fechaCompromiso)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Prioridad · Estatus</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${escape(t.prioridad)} · ${escape(t.estatus)}</div>
                </div>
                <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:3px;">Bloqueada</div>
                    <div style="font-weight:600;font-size:0.87rem;color:#0f172a;">${t.bloqueada ? `Sí (${escape(t.motivoBloqueo || 'Sin motivo')})` : 'No'}</div>
                </div>
            </div>

            <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:6px;">Avance General del Tema</div>
                <div style="display:flex;align-items:center;gap:10px;">
                    <div style="flex:1;height:8px;border-radius:99px;background:rgba(138,0,49,.1);overflow:hidden;">
                        <div style="height:100%;width:${t.avance || 0}%;border-radius:99px;background:${sem==='rojo'?'#c0222a':sem==='amarillo'?'#d97706':'#027a48'};transition:width .4s;"></div>
                    </div>
                    <span style="font-weight:700;font-size:0.9rem;color:#0f172a;">${t.avance || 0}%</span>
                </div>
            </div>

            <div style="background:#f8fafc;border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#8a0031;margin-bottom:6px;">Corresponsables</div>
                <div style="display:flex;flex-wrap:wrap;gap:5px;">${corrList}</div>
            </div>

            ${etapasTimelineHtml}

            ${t.comentarios ? `
            <div style="background:#fffbeb;border-radius:8px;padding:10px 14px;border-left:3px solid #d97706;margin-top:5px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#d97706;margin-bottom:4px;">Comentarios</div>
                <div style="font-size:0.85rem;color:#475569;line-height:1.5;">${escape(t.comentarios)}</div>
            </div>` : ''}

            ${t.evidenciaUrl ? `
            <a href="${escape(t.evidenciaUrl)}" target="_blank" rel="noopener"
               style="display:inline-flex;align-items:center;gap:8px;padding:8px 14px;border-radius:8px;background:rgba(180,137,52,.1);color:#b48934;font-size:0.82rem;font-weight:700;text-decoration:none;align-self:flex-start;margin-top:5px;">
                <i class="fa-solid fa-folder-open"></i> Ver Evidencia
            </a>` : ''}
        </div>`;

    openModal(`Detalle: ${t.tema}`, html, null);
}
