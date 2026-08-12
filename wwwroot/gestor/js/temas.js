import { escape, fmtDate, priClass, estClass, semaforoTema, avancePromedio, openModal, toast } from './utils.js';
import { dataService } from './data-service.js';

let _actsState = [];
let _temasState = [];

function normalizeUserName(value) {
    return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .toLowerCase();
}

export function populateActividadesResponsablesFilter(actividades) {
    const sel = document.getElementById('filtro-temas-responsable');
    if (!sel) return;
    const prevValue = sel.value;
    
    const set = new Set();
    actividades.forEach(a => {
        if (a.responsablePrincipal) set.add(a.responsablePrincipal.trim());
        if (a.corresponsables) a.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    
    const responsibles = [...set].sort((x, y) => x.localeCompare(y, 'es-MX', { sensitivity: 'base' }));
    sel.innerHTML = '<option value="">Responsable</option>' +
        responsibles.map(r => `<option value="${escape(r)}">${escape(r)}</option>`).join('');
        
    if (prevValue && responsibles.includes(prevValue)) {
        sel.value = prevValue;
    }
}

export function renderTemas(actividades, temas, filtro = '', responsableFilter = '') {
    _actsState = actividades; _temasState = temas;
    populateActividadesResponsablesFilter(actividades);

    const f = (filtro || '').toLowerCase();
    let filtered = actividades;

    if (f) {
        filtered = filtered.filter(a =>
            a.actividad.toLowerCase().includes(f) ||
            (a.responsablePrincipal || '').toLowerCase().includes(f) ||
            (a.categoria || '').toLowerCase().includes(f)
        );
    }

    if (responsableFilter) {
        filtered = filtered.filter(a =>
            a.responsablePrincipal === responsableFilter ||
            (a.corresponsables && a.corresponsables.some(c => c.nombre === responsableFilter))
        );
    }

    const cont = document.getElementById('cards-temas');
    cont.innerHTML = filtered.length
        ? filtered.map(a => {
            const av = avancePromedio(a, temas);
            const sem = semaforoTema(a, temas);
            const numTemas = temas.filter(t => t.actividadId === a.id).length;
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : sem === 'verde' ? 'track--complete' : 'track--pending';
            
            // Initials avatar
            const initials = (a.responsablePrincipal || '??')
                .split(' ')
                .filter(w => w.length > 0)
                .slice(0, 2)
                .map(w => w[0].toUpperCase())
                .join('');
            
            const calSvg = '<svg viewBox="0 0 16 16"><path d="M5 0v1H3.5A1.5 1.5 0 0 0 2 2.5v11A1.5 1.5 0 0 0 3.5 15h9a1.5 1.5 0 0 0 1.5-1.5v-11A1.5 1.5 0 0 0 12.5 1H11V0h-1v1H6V0H5Zm-2 5h10v8.5a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V5Z"/></svg>';
            const actSvg = '<svg viewBox="0 0 16 16"><path d="M2 2.5A.5.5 0 0 1 2.5 2h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5Zm0 3A.5.5 0 0 1 2.5 5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5Zm.5 2.5a.5.5 0 0 0 0 1h4a.5.5 0 0 0 0-1h-4Zm0 3a.5.5 0 0 0 0 1h3a.5.5 0 0 0 0-1h-3ZM8.5 5a.5.5 0 0 0-.5.5v5a.5.5 0 0 0 .5.5h5a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5h-5Z"/></svg>';
            const hasLink = !!(a.ligaSharePoint);
            const corrNames = a.corresponsables && a.corresponsables.length
                ? ' + ' + a.corresponsables.map(c => c.nombre.split(' ')[0]).join(', ')
                : '';

            return `
                <article class="tema-card s-${sem}" data-id="${a.id}">
                    <div class="tema-card__head">
                        <h3>${escape(a.actividad)}</h3>
                        <span class="tema-card__head-actions">
                            ${hasLink ? `<a href="${escape(a.ligaSharePoint)}" target="_blank" rel="noopener" class="tema-card__link-btn" title="Abrir enlace en SharePoint" style="display: inline-flex; align-items: center; justify-content: center;"><i class="bi bi-folder2-open" style="font-size: 1.15rem; color: #b48934;"></i></a>` : ''}
                            <span class="semaforo ${sem}" title="Semáforo"></span>
                        </span>
                    </div>
                    <div class="tema-card__meta">
                        <span class="chip ${priClass(a.prioridad)}">${escape(a.prioridad)}</span>
                        <span class="chip">${escape(a.estatus)}</span>
                        <span class="chip">${escape(a.categoria || 'Sin categoría')}</span>
                    </div>
                    <p class="tema-card__desc">${escape(a.descripcion || '')}</p>
                    <div class="tema-card__progress">
                        <div class="track ${trackCls}"><span style="width:${av}%"></span></div>
                        <span class="tema-card__pct">${av}%</span>
                    </div>
                    <div class="tema-card__foot">
                        <span class="tema-card__avatar">
                            <span class="tema-card__avatar-circle">${initials}</span>
                            ${escape(a.responsablePrincipal)}<span class="corresponsables-subtext" style="font-size: 0.75rem; color: var(--texto-suave); font-weight: 500;" title="Corresponsables: ${escape(a.corresponsables?.map(c => c.nombre).join(', ') || '')}">${escape(corrNames)}</span>
                        </span>
                        <span class="tema-card__foot-info">
                            ${actSvg} ${numTemas}
                            ${calSvg} ${fmtDate(a.fechaCompromiso)}
                        </span>
                    </div>

                    <!-- ── Barra de acciones ── -->
                    <div class="tema-card__actions" style="display: flex; align-items: center; justify-content: flex-end; gap: 4px; padding: 8px 14px 10px; border-top: 1px solid rgba(155, 34, 71,0.07); margin-top: 6px;">
                        <button type="button" class="tca-btn tca-view" data-id="${a.id}"
                            title="Ver detalle"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(10,94,149,.08);color:#0a5e95;">
                            <i class="bi bi-eye"></i> Ver
                        </button>
                        ${(window.currentUser && window.currentUser.id === 1) ? `
                        <button type="button" class="tca-btn tca-edit" data-id="${a.id}"
                            title="Editar actividad"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(155, 34, 71,.08);color:var(--g-acento, #9B2247);">
                            <i class="bi bi-pencil-square"></i> Editar
                        </button>
                        <button type="button" class="tca-btn tca-delete" data-id="${a.id}"
                            title="Eliminar actividad"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(155, 34, 71,.08);color:#9B2247;">
                            <i class="bi bi-trash"></i> Eliminar
                        </button>
                        ` : ''}
                        <button type="button" class="tca-btn tca-email" data-id="${a.id}"
                            title="Compartir por correo institucional"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(71,85,105,.08);color:var(--g-tinta-suave, #475569);">
                            <i class="bi bi-envelope"></i>
                        </button>
                        <!--
                        <button type="button" class="tca-btn tca-whatsapp" data-id="${a.id}"
                            title="Compartir por WhatsApp (incluye enlace al Gestor)"
                            style="display:inline-flex;align-items:center;gap:5px;padding:5px 10px;border:none;border-radius:7px;font-size:0.75rem;font-weight:600;cursor:pointer;transition:all .18s;background:rgba(37,211,102,.1);color:#128C7E;">
                            <i class="bi bi-whatsapp"></i>
                        </button>
                        -->
                    </div>
                </article>`;
        }).join('')
        : '<p style="grid-column:1/-1;text-align:center;color:var(--g-text-soft);padding:2rem">Sin actividades que coincidan</p>';

    // ── Bind action buttons ──
    cont.querySelectorAll('.tca-view').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openActividadDetalle(act, temas);
        };
    });

    cont.querySelectorAll('.tca-edit').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openActividadModal(act);
        };
    });

    cont.querySelectorAll('.tca-delete').forEach(btn => {
        btn.onclick = async (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (!act) return;
            if (!confirm(`¿Eliminar la actividad "${act.actividad}" y todos sus temas? Esta acción no se puede deshacer.`)) return;
            try {
                const childTemas = _temasState.filter(t => t.actividadId === act.id);
                for (const t of childTemas) await dataService.remove('temas', t.id);
                await dataService.remove('actividades', act.id);
                toast('Actividad eliminada', 'ok');
                window.dispatchEvent(new CustomEvent('gestor:refresh'));
            } catch (err) {
                toast(err.message || 'Error al eliminar', 'err');
            }
        };
    });

    cont.querySelectorAll('.tca-email').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openCompartirCorreoModal(act);
        };
    });

    cont.querySelectorAll('.tca-whatsapp').forEach(btn => {
        btn.onclick = (e) => {
            e.stopPropagation();
            const act = actividades.find(x => x.id === btn.dataset.id);
            if (act) openCompartirWhatsAppModal(act, temas);
        };
    });

    // Click en la tarjeta (fuera de botones) → ver detalle
    cont.querySelectorAll('.tema-card').forEach(card => {
        card.onclick = (e) => {
            if (e.target.closest('.tca-btn, .tema-card__link-btn, a')) return;
            const act = actividades.find(x => x.id === card.dataset.id);
            if (act) openActividadDetalle(act, temas);
        };
    });
}

function getProgressBarEmoji(percent, sem) {
    const total = 10;
    const activeCount = Math.round(Math.max(0, Math.min(100, percent)) / 10);
    const inactiveCount = total - activeCount;
    let emoji = '🟩';
    if (sem === 'rojo') emoji = '🟥';
    else if (sem === 'amarillo') emoji = '🟨';
    else if (sem === 'gris') emoji = '⬜';
    return emoji.repeat(activeCount) + '⬜'.repeat(inactiveCount);
}

function getTemaStatusEmoji(t) {
    if (t.estatus === 'Concluida') return '✅';
    if (t.estatus === 'Vencida') return '🚨';
    if (t.estatus === 'En proceso') return '⏳';
    return '📌';
}

function buildActividadWhatsAppText(actividad, temas) {
    const av = avancePromedio(actividad, temas);
    const sem = semaforoTema(actividad, temas);
    const portalUrl = `${window.location.origin}/Gestor/Index`;
    const corresponsables = actividad.corresponsables?.length
        ? actividad.corresponsables.map(c => c.nombre).join(', ')
        : 'Sin corresponsables';

    const bar = getProgressBarEmoji(av, sem);
    const fCompromiso = fmtDate(actividad.fechaCompromiso);
    const fInicio = fmtDate(actividad.fechaInicio);

    const childTemas = temas.filter(t => t.actividadId === actividad.id);
    let temasListText = '';
    if (childTemas.length > 0) {
        temasListText = [
            '',
            `📋 *Temas asociados (${childTemas.length}):*`,
            ...childTemas.map(t => {
                const icon = getTemaStatusEmoji(t);
                const tAvance = t.avance || 0;
                return `${icon} _${t.tema}_ (${t.responsable || 'Sin responsable'}) - *${tAvance}%*`;
            })
        ].join('\n');
    }

    return [
        '🏛️ *SENER | Gestor de Actividades*',
        '━━━━━━━━━━━━━━━━━━━━',
        `📌 *Actividad:* ${actividad.actividad}`,
        `👤 *Responsable:* ${actividad.responsablePrincipal || 'Sin responsable'}`,
        `👥 *Corresponsables:* ${corresponsables}`,
        `📅 *Inicio:* ${fInicio}`,
        `📅 *Compromiso:* ${fCompromiso}`,
        `⚡ *Prioridad:* ${actividad.prioridad || '—'}`,
        `🔄 *Estatus:* ${actividad.estatus || '—'}`,
        `📊 *Avance:* ${bar} (${av}%)`,
        actividad.descripcion ? `📝 *Descripción:* ${actividad.descripcion}` : '',
        actividad.comentariosEjecutivos ? `💬 *Comentarios:* ${actividad.comentariosEjecutivos}` : '',
        temasListText,
        '━━━━━━━━━━━━━━━━━━━━',
        '🔗 *Ver en el Gestor:*',
        portalUrl
    ].filter(x => x !== '').join('\n');
}

function downloadActividadShareImage(actividad, temas) {
    const av = avancePromedio(actividad, temas);
    const corresponsables = actividad.corresponsables?.length
        ? actividad.corresponsables.map(c => c.nombre).join(', ')
        : 'Sin corresponsables';
    const rows = [
        ['Actividad', actividad.actividad],
        ['Responsable', actividad.responsablePrincipal || 'Sin responsable'],
        ['Corresponsables', corresponsables],
        ['Compromiso', actividad.fechaCompromiso || 'Sin fecha'],
        ['Estatus', actividad.estatus || '—'],
        ['Prioridad', actividad.prioridad || '—'],
        ['Avance', `${av}%`]
    ];

    const canvas = document.createElement('canvas');
    canvas.width = 1200;
    canvas.height = 760;
    const ctx = canvas.getContext('2d');

    ctx.fillStyle = '#f5f1ea';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(56, 56, 1088, 648);
    ctx.fillStyle = '#9B2247';
    ctx.fillRect(56, 56, 1088, 104);
    ctx.fillStyle = '#b48934';
    ctx.fillRect(56, 160, 1088, 8);

    ctx.fillStyle = '#ffffff';
    ctx.font = '700 30px Arial';
    ctx.fillText('SENER | Gestor de Actividades DGMESNIE', 92, 116);
    ctx.font = '600 18px Arial';
    ctx.fillText('Ficha institucional para seguimiento', 92, 144);

    ctx.fillStyle = '#111827';
    ctx.font = '700 28px Arial';
    wrapCanvasText(ctx, actividad.actividad || 'Actividad sin nombre', 92, 220, 1016, 34, 2);

    let y = 310;
    rows.slice(1).forEach(([label, value]) => {
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

    ctx.fillStyle = '#9B2247';
    ctx.font = '700 19px Arial';
    ctx.fillText(`${window.location.origin}/Gestor/Index`, 92, 668);
    ctx.fillStyle = '#6b7280';
    ctx.font = '500 16px Arial';
    ctx.fillText('Generado automáticamente desde el Gestor de Actividades', 92, 694);

    downloadCanvasAsPng(canvas, `actividad-${actividad.id || 'gestor'}.png`);
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

function openCompartirWhatsAppModal(actividad, temas) {
    const text = buildActividadWhatsAppText(actividad, temas);
    const waUrl = `https://wa.me/?text=${encodeURIComponent(text)}`;
    const av = avancePromedio(actividad, temas);
    const corresponsables = actividad.corresponsables?.length
        ? actividad.corresponsables.map(c => c.nombre).join(', ')
        : 'Sin corresponsables';

    const html = `
        <div style="display:flex;flex-direction:column;gap:14px;">
            <div style="border:1px solid rgba(155, 34, 71,.16);border-radius:10px;overflow:hidden;background:#fff;">
                <div style="background:#9B2247;color:#fff;padding:12px 14px;font-weight:800;">SENER | Gestor de Actividades DGMESNIE</div>
                <table style="width:100%;border-collapse:collapse;font-size:.84rem;">
                    <tbody>
                        <tr><th style="width:34%;text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Actividad</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;font-weight:700;">${escape(actividad.actividad)}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Responsable</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.responsablePrincipal || 'Sin responsable')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Corresponsables</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(corresponsables)}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Compromiso</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.fechaCompromiso || 'Sin fecha')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Estatus</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.estatus || '—')}</td></tr>
                        <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;">Avance</th><td style="padding:8px 10px;">${av}%</td></tr>
                    </tbody>
                </table>
            </div>
            <textarea readonly style="width:100%;min-height:150px;border:1px solid rgba(155, 34, 71,.18);border-radius:8px;padding:10px;font-size:.82rem;resize:vertical;box-sizing:border-box;">${escape(text)}</textarea>
            <div style="display:flex;justify-content:flex-end;gap:10px;flex-wrap:wrap;">
                <button type="button" id="btn-descargar-wa-img" class="internal-button">Descargar imagen</button>
                <a href="${waUrl}" target="_blank" rel="noopener" class="internal-button internal-button--primary" style="text-decoration:none;">Abrir WhatsApp</a>
            </div>
        </div>`;

    openModal('Compartir por WhatsApp', html, null);
    document.getElementById('btn-descargar-wa-img').onclick = () => downloadActividadShareImage(actividad, temas);
}

/* ── Modal de compartir por correo ── */
async function openCompartirCorreoModal(actividad) {
    function normalizeUserName(value) {
        return String(value || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim().toLowerCase();
    }

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const av = avancePromedio(actividad, _temasState);
    const corresponsables = actividad.corresponsables?.length
        ? actividad.corresponsables.map(c => c.nombre).join(', ')
        : 'Sin corresponsables';

    const html = `
        <form id="compartir-form" style="margin:0;">
            <p style="margin-bottom:12px; font-size:0.88rem; color:var(--texto-suave);">
                Selecciona uno o más usuarios <strong>y/o</strong> escribe un correo externo para compartir el reporte de esta actividad.
            </p>

            <div class="usuarios-check-list" style="max-height:180px;overflow-y:auto;border:1px solid rgba(155, 34, 71,.15);border-radius:10px;padding:10px;background:#fff;display:flex;flex-direction:column;gap:8px;box-shadow:inset 0 2px 4px rgba(0,0,0,.02);margin-bottom:14px;">
                ${filteredUsers.map(u => `
                    <label style="display:flex;align-items:center;gap:8px;font-weight:500;font-size:0.85rem;color:var(--texto);cursor:pointer;margin:0;">
                        <input type="checkbox" name="usuariosIds" value="${u.idUsuario}" style="width:16px;height:16px;accent-color:var(--guinda);cursor:pointer;">
                        <span>${escape(u.nombre)}${u.correo ? ` <span style="font-size:0.77rem;color:var(--texto-suave);">(${escape(u.correo)})</span>` : ''}</span>
                    </label>
                `).join('')}
            </div>

            <div style="margin-bottom:14px;">
                <label style="font-size:0.82rem;font-weight:700;color:var(--texto);display:block;margin-bottom:5px;">Correo externo (opcional):</label>
                <input type="email" id="correo-libre" placeholder="ejemplo@dominio.gob.mx"
                    style="width:100%;padding:8px 12px;border:1px solid rgba(155, 34, 71,.2);border-radius:8px;font-size:0.85rem;box-sizing:border-box;">
            </div>

            <div style="margin-bottom:1.2rem;">
                <div style="font-size:0.72rem;text-transform:uppercase;color:var(--g-acento, #9B2247);font-weight:700;margin-bottom:7px;">Detalle que se enviará</div>
                <div style="overflow-x:auto;border:1px solid rgba(155, 34, 71,.16);border-radius:9px;background:#fff;">
                    <table style="width:100%;border-collapse:collapse;font-size:0.82rem;">
                        <tbody>
                            <tr><th style="width:34%;text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Actividad</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;font-weight:700;">${escape(actividad.actividad)}</td></tr>
                            <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Responsable</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.responsablePrincipal || 'Sin responsable')}</td></tr>
                            <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Corresponsables</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(corresponsables)}</td></tr>
                            <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Compromiso</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.fechaCompromiso || 'Sin fecha')}</td></tr>
                            <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;border-bottom:1px solid #eadde4;">Estatus</th><td style="padding:8px 10px;border-bottom:1px solid #eadde4;">${escape(actividad.estatus || '—')}</td></tr>
                            <tr><th style="text-align:left;padding:8px 10px;background:#f7ecf1;color:#6b1034;">Avance</th><td style="padding:8px 10px;">${av}%</td></tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <div style="display:flex;justify-content:flex-end;gap:10px;">
                <button type="button" id="btn-cancelar-compartir" class="internal-button" style="min-height:36px;padding:.5rem 1rem;">Cancelar</button>
                <button type="submit" id="btn-enviar-compartir" class="internal-button internal-button--primary" style="min-height:36px;padding:.5rem 1rem;display:inline-flex;align-items:center;gap:8px;">
                    <i class="bi bi-send"></i> Enviar correo
                </button>
            </div>
        </form>`;

    const close = openModal(`Compartir actividad por correo`, html, null);

    document.getElementById('btn-cancelar-compartir').onclick = close;

    document.getElementById('compartir-form').onsubmit = async (e) => {
        e.preventDefault();
        const checks = document.querySelectorAll('#modal-body input[name="usuariosIds"]:checked');
        const usuariosIds = Array.from(checks).map(cb => Number(cb.value));
        const correoLibre = document.getElementById('correo-libre')?.value?.trim() || '';

        if (!usuariosIds.length && !correoLibre) {
            toast('Selecciona al menos un destinatario o escribe un correo.', 'err');
            return;
        }

        const btnEnviar = document.getElementById('btn-enviar-compartir');
        const btnCancel = document.getElementById('btn-cancelar-compartir');
        if (btnEnviar) { btnEnviar.disabled = true; btnEnviar.innerHTML = '<i class="bi bi-arrow-repeat bi-spin"></i> Enviando...'; }
        if (btnCancel) btnCancel.disabled = true;

        try {
            const token = document.querySelector('meta[name="csrf-token"]')?.content ?? '';
            const res = await fetch(`/Gestor/Api/Actividades/${actividad.id}/Compartir`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                body: JSON.stringify({ usuarioIds: usuariosIds, correoLibre })
            });
            if (!res.ok) {
                const body = await res.json().catch(() => null);
                throw new Error(body?.error || `Error ${res.status}`);
            }
            if (btnEnviar) btnEnviar.innerHTML = '<i class="bi bi-check-lg"></i> ¡Enviado!';
            toast('Correo enviado correctamente.', 'ok');
            setTimeout(close, 1200);
        } catch (err) {
            toast(err.message || 'Error al enviar el correo', 'err');
            if (btnEnviar) { btnEnviar.disabled = false; btnEnviar.innerHTML = '<i class="bi bi-send"></i> Enviar correo'; }
            if (btnCancel) btnCancel.disabled = false;
        }
    };
}

/* ── Modal de detalle (solo lectura) ── */
export function openActividadDetalle(a, temas) {
    const av = avancePromedio(a, temas);
    const sem = semaforoTema(a, temas);
    const semLabels = { verde: '🟢 A tiempo', amarillo: '🟡 Por vencer', rojo: '🔴 Vencido', gris: '⚫ Bloqueado/Sin fecha' };
    const numTemas = temas.filter(t => t.actividadId === a.id).length;
    const corrList = a.corresponsables && a.corresponsables.length
        ? a.corresponsables.map(c => `<span style="display:inline-flex;align-items:center;gap:5px;background:rgba(30,91,79,.08);border-radius:6px;padding:2px 8px;font-size:0.78rem;color:#1e5b4f;font-weight:600;">${escape(c.nombre)}</span>`).join(' ')
        : '<span style="color:var(--texto-suave);font-size:0.82rem;">Sin corresponsables</span>';

    // Cargar y ordenar los temas de la actividad
    const childTemas = temas.filter(t => t.actividadId === a.id);
    childTemas.sort((x, y) => (x.fechaInicio || '').localeCompare(y.fechaInicio || ''));

    // Encontrar el primer tema activo (no concluido)
    const activeTema = childTemas.find(t => t.estatus !== 'Concluida');

    let timelineHtml = '';
    if (childTemas.length > 0) {
        timelineHtml = `
            <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:14px;border:1px solid rgba(155, 34, 71,0.06);margin-top:5px;margin-bottom:5px;">
                <div style="font-size:0.7rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:12px;display:flex;align-items:center;gap:6px;">
                    <i class="bi bi-signpost-split"></i> Línea del Tiempo / Secuencia de Temas
                </div>
                <div style="position:relative;padding-left:22px;display:flex;flex-direction:column;gap:16px;">
                    <!-- Línea vertical -->
                    <div style="position:absolute;left:7px;top:6px;bottom:6px;width:2px;background:rgba(155, 34, 71,0.12);"></div>
                    
                    ${childTemas.map((t, idx) => {
                        const tSem = t.estatus === 'Concluida' ? 'verde' : t.estatus === 'Vencida' ? 'rojo' : t.estatus === 'En proceso' ? 'amarillo' : 'gris';
                        const dotColor = tSem === 'verde' ? '#0E7C5A' : tSem === 'amarillo' ? '#d97706' : tSem === 'rojo' ? '#9B2247' : '#6F6B66';
                        const isCurrentActive = activeTema && t.id === activeTema.id;
                        
                        const dateText = t.fechaInicio && t.fechaCompromiso
                            ? `${fmtDate(t.fechaInicio)} al ${fmtDate(t.fechaCompromiso)}`
                            : t.fechaCompromiso ? `Fecha compromiso: ${fmtDate(t.fechaCompromiso)}` : 'Sin fechas registradas';
                            
                        return `
                            <div style="position:relative;display:flex;flex-direction:column;gap:3px;${isCurrentActive ? 'background:rgba(155, 34, 71,0.03);padding:6px 10px;border-radius:6px;border-left:3px solid var(--g-acento, #9B2247);margin-left:-10px;' : ''}">
                                <!-- Dot -->
                                <div style="position:absolute;left:${isCurrentActive ? '-20px' : '-22px'};top:4px;width:10px;height:10px;border-radius:50%;background:${dotColor};border:2px solid #fff;box-shadow:0 0 0 1px ${dotColor};z-index:1;"></div>
                                
                                <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;">
                                    <span style="font-weight:700;font-size:0.82rem;color:var(--g-tinta, #1c1b1a);">${escape(t.tema)}</span>
                                    <span style="font-size:0.65rem;font-weight:700;padding:1px 5px;border-radius:4px;background:${t.estatus==='Concluida'?'rgba(14, 124, 90,0.1)':t.estatus==='En proceso'?'rgba(217,119,6,0.1)':t.estatus==='Vencida'?'rgba(155, 34, 71,0.1)':'rgba(111, 107, 102,0.1)'};color:${dotColor};">${escape(t.estatus)}</span>
                                    ${isCurrentActive ? '<span style="font-size:0.62rem;font-weight:800;background:#9B2247;color:#fff;padding:1px 5px;border-radius:4px;text-transform:uppercase;letter-spacing:0.05em;display:inline-flex;align-items:center;gap:3px;"><i class="bi bi-play-fill"></i> Tema Vigente</span>' : ''}
                                </div>
                                
                                <div style="font-size:0.75rem;color:var(--texto-suave);font-weight:500;">
                                    <span>👤 Asignado a: <strong>${escape(t.responsable || 'Sin responsable')}</strong></span>
                                    <span style="margin:0 6px;color:#cbd5e1;">|</span>
                                    <span>📅 ${dateText}</span>
                                    <span style="margin:0 6px;color:#cbd5e1;">|</span>
                                    <span>📊 Avance: <strong>${t.avance || 0}%</strong></span>
                                </div>
                                ${t.descripcion ? `<div style="font-size:0.72rem;color:var(--g-tinta-suave, #475569);font-style:italic;margin-top:2px;">${escape(t.descripcion)}</div>` : ''}
                            </div>
                        `;
                    }).join('')}
                </div>
            </div>`;
    }

    const html = `
        <div style="display:flex;flex-direction:column;gap:1rem;">
            <div style="display:flex;align-items:center;gap:10px;padding:10px 14px;border-radius:10px;background:rgba(155, 34, 71,0.04);border-left:4px solid var(--g-acento, #9B2247);">
                <span class="semaforo ${sem}" style="width:12px;height:12px;flex-shrink:0;margin-top:0;"></span>
                <div>
                    <div style="font-size:0.7rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);letter-spacing:.05em;">Actividad</div>
                    <div style="font-weight:700;font-size:1rem;color:var(--g-tinta, #1c1b1a);">${escape(a.actividad)}</div>
                </div>
                <span style="margin-left:auto;font-size:0.78rem;font-weight:700;color:var(--g-acento, #9B2247);">${semLabels[sem] || sem}</span>
            </div>

            ${a.descripcion ? `<p style="margin:0;font-size:0.87rem;color:var(--g-tinta-suave, #475569);line-height:1.55;">${escape(a.descripcion)}</p>` : ''}

            <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Responsable</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${escape(a.responsablePrincipal || '—')}</div>
                </div>
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Categoría</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${escape(a.categoria || '—')}</div>
                </div>
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Inicio</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${fmtDate(a.fechaInicio)}</div>
                </div>
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Compromiso</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${fmtDate(a.fechaCompromiso)}</div>
                </div>
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Prioridad · Estatus</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${escape(a.prioridad)} · ${escape(a.estatus)}</div>
                </div>
                <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                    <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:3px;">Temas</div>
                    <div style="font-weight:600;font-size:0.87rem;color:var(--g-tinta, #1c1b1a);">${numTemas} tema${numTemas !== 1 ? 's' : ''}</div>
                </div>
            </div>

            <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:6px;">Avance</div>
                <div style="display:flex;align-items:center;gap:10px;">
                    <div style="flex:1;height:8px;border-radius:99px;background:rgba(155, 34, 71,.1);overflow:hidden;">
                        <div style="height:100%;width:${av}%;border-radius:99px;background:${sem==='rojo'?'#9B2247':sem==='amarillo'?'#d97706':'#0E7C5A'};transition:width .4s;"></div>
                    </div>
                    <span style="font-weight:700;font-size:0.9rem;color:var(--g-tinta, #1c1b1a);">${av}%</span>
                </div>
            </div>

            <div style="background:var(--g-campo, #f8fafc);border-radius:8px;padding:10px 14px;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:var(--g-acento, #9B2247);margin-bottom:6px;">Corresponsables</div>
                <div style="display:flex;flex-wrap:wrap;gap:5px;">${corrList}</div>
            </div>

            ${timelineHtml}

            ${a.comentariosEjecutivos ? `
            <div style="background:#fffbeb;border-radius:8px;padding:10px 14px;border-left:3px solid #d97706;">
                <div style="font-size:0.68rem;text-transform:uppercase;font-weight:700;color:#d97706;margin-bottom:4px;">Comentarios</div>
                <div style="font-size:0.85rem;color:var(--g-tinta-suave, #475569);line-height:1.5;">${escape(a.comentariosEjecutivos)}</div>
            </div>` : ''}

            ${a.ligaSharePoint ? `
            <a href="${escape(a.ligaSharePoint)}" target="_blank" rel="noopener"
               style="display:inline-flex;align-items:center;gap:8px;padding:8px 14px;border-radius:8px;background:rgba(180,137,52,.1);color:#b48934;font-size:0.82rem;font-weight:700;text-decoration:none;align-self:flex-start;">
                <i class="bi bi-folder2-open"></i> Abrir en SharePoint
            </a>` : ''}
        </div>`;

    openModal(`Detalle: ${a.actividad}`, html, null);
}

export async function openActividadModal(actividad) {
    const a = actividad || {
        actividad: '', descripcion: '', categoria: '', prioridad: 'Media', estatus: 'Activo',
        responsablePrincipal: '', responsablePrincipalId: '', fechaInicio: '', fechaCompromiso: '',
        ligaSharePoint: '', comentariosEjecutivos: '', corresponsablesIds: []
    };
    if (!a.corresponsablesIds) a.corresponsablesIds = [];

    const users = await dataService.list('usuarios');
    const filteredUsers = users
        .filter(u => normalizeUserName(u.nombre) !== 'consulta publica')
        .sort((x, y) => x.nombre.localeCompare(y.nombre, 'es-MX', { sensitivity: 'base' }));

    const selectedById = Number(a.responsablePrincipalId);
    const currentById = Number.isFinite(selectedById) && selectedById > 0
        ? filteredUsers.find(u => u.idUsuario === selectedById)
        : null;
    const currentByName = filteredUsers.find(u => u.nombre === a.responsablePrincipal);
    const selectedUserId = (currentById?.idUsuario ?? currentByName?.idUsuario ?? '');

    const responsableOptions = [
        '<option value="">Selecciona responsable principal</option>',
        ...filteredUsers.map(u => `<option value="${u.idUsuario}" ${String(u.idUsuario) === String(selectedUserId) ? 'selected' : ''}>${escape(u.nombre)}</option>`)
    ].join('');

    const isNew = !actividad;
    let etapasHtml = '';
    if (!isNew) {
        const childTemas = _temasState.filter(t => t.actividadId === a.id);
        childTemas.sort((x, y) => (x.fechaInicio || '').localeCompare(y.fechaInicio || ''));
        
        etapasHtml = `
            <div class="form-field full" style="margin-top: 15px; border-top: 1px solid rgba(155, 34, 71,.1); padding-top: 15px;">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:10px;">
                    <label style="font-weight: 700; color: var(--texto); margin: 0;">Secuencia de Temas de la Actividad</label>
                    <span style="font-size:0.75rem; color:var(--texto-suave);">Administrable desde la pestaña de Temas</span>
                </div>
                <div style="display:flex; flex-direction:column; gap:8px;">
                    ${childTemas.map(t => {
                        const tSem = t.estatus === 'Concluida' ? 'verde' : t.estatus === 'Vencida' ? 'rojo' : t.estatus === 'En proceso' ? 'amarillo' : 'gris';
                        const dotColor = tSem === 'verde' ? '#0E7C5A' : tSem === 'amarillo' ? '#d97706' : tSem === 'rojo' ? '#9B2247' : '#6F6B66';
                        return `
                            <div style="display:flex; justify-content:space-between; align-items:center; background:var(--g-campo, #f8fafc); border:1px solid var(--borde); border-radius:8px; padding:8px 12px; font-size:0.8rem;">
                                <div>
                                    <strong style="color:var(--g-tinta, #1c1b1a);">${escape(t.tema)}</strong>
                                    <span style="font-size:0.72rem; color:var(--texto-suave); margin-left:8px;">(${escape(t.responsable || 'Sin responsable')} - <strong>${t.avance || 0}%</strong>)</span>
                                </div>
                                <span style="font-size:0.7rem; font-weight:700; color:${dotColor}; padding:1px 6px; border-radius:4px; background:${
                                    t.estatus === 'Concluida' ? 'rgba(14, 124, 90, 0.08)' :
                                    t.estatus === 'En proceso' ? 'rgba(217, 119, 6, 0.08)' :
                                    t.estatus === 'Vencida' ? 'rgba(155, 34, 71, 0.08)' : 'rgba(111, 107, 102, 0.08)'
                                };">${escape(t.estatus)}</span>
                            </div>
                        `;
                    }).join('') || '<p style="font-size:0.8rem; color:var(--texto-suave); margin:0;">Sin temas registrados para esta actividad.</p>'}
                </div>
            </div>`;
    }

    const html = `
        <form>
            <div class="form-row">
                <div class="form-field full"><label>Actividad *</label><input name="actividad" required value="${escape(a.actividad)}"></div>
                <div class="form-field full"><label>Descripción</label><textarea name="descripcion">${escape(a.descripcion || '')}</textarea></div>
                <div class="form-field"><label>Categoría</label><input name="categoria" value="${escape(a.categoria || '')}"></div>
                <div class="form-field"><label>Prioridad</label>
                    <select name="prioridad">${['Alta', 'Media', 'Baja'].map(p => `<option ${a.prioridad === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Estatus</label>
                    <select name="estatus">${['Activo', 'Pausado', 'Concluido'].map(p => `<option ${a.estatus === p ? 'selected' : ''}>${p}</option>`).join('')}</select>
                </div>
                <div class="form-field"><label>Responsable principal *</label><select name="responsablePrincipalId" required>${responsableOptions}</select></div>
                <div class="form-field"><label>Fecha inicio</label><input type="date" name="fechaInicio" value="${a.fechaInicio || ''}"></div>
                <div class="form-field"><label>Fecha compromiso *</label><input type="date" name="fechaCompromiso" required value="${a.fechaCompromiso || ''}"></div>
                
                <div class="form-field full" style="margin-top: 10px;">
                    <label style="font-weight: 700; color: var(--texto); margin-bottom: 6px; display: block;">Corresponsables</label>
                    <div class="usuarios-check-list" style="max-height: 140px; overflow-y: auto; border: 1px solid rgba(155, 34, 71, 0.15); border-radius: 10px; padding: 10px; background: #fff; display: flex; flex-direction: column; gap: 8px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);">
                        ${filteredUsers.map(u => `
                            <label class="corresponsable-label" data-id="${u.idUsuario}" style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 0.85rem; color: var(--texto); cursor: pointer; margin: 0;">
                                <input type="checkbox" name="corresponsablesIds" value="${u.idUsuario}" ${a.corresponsablesIds.includes(u.idUsuario) ? 'checked' : ''} style="width: 16px; height: 16px; accent-color: var(--guinda); cursor: pointer;">
                                <span>${escape(u.nombre)}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
                <div class="form-field full"><label>Liga SharePoint</label><input type="url" name="ligaSharePoint" value="${escape(a.ligaSharePoint || '')}"></div>
                <div class="form-field full"><label>Comentarios</label><textarea name="comentariosEjecutivos">${escape(a.comentariosEjecutivos || '')}</textarea></div>
                ${etapasHtml}
            </div>
            <!-- Error Alert Area -->
            <div id="actividad-error-alert" style="display:none; color:var(--riesgo); background:rgba(155, 34, 71,0.06); border:1px solid rgba(155, 34, 71,0.15); border-radius:8px; padding:10px 14px; font-size:0.82rem; font-weight:600; margin-top:15px; margin-bottom:5px; align-items:center; gap:8px;">
                <i class="bi bi-exclamation-triangle" style="color:var(--riesgo);"></i>
                <span class="error-msg"></span>
            </div>
            <div class="form-actions">
                ${!isNew ? `<button type="button" class="internal-button" style="color:var(--riesgo);border-color:rgba(180,35,24,.3)" id="btn-del-act">Eliminar</button>` : ''}
                <button type="submit" class="internal-button internal-button--primary">${isNew ? 'Crear' : 'Guardar'}</button>
            </div>
        </form>`;

    const close = openModal(isNew ? 'Nueva actividad' : 'Editar actividad', html, async (data, close) => {
        const submitBtn = document.querySelector('#modal-body form button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = isNew ? 'Creando...' : 'Guardando...';
        }
        try {
            const errorAlert = document.getElementById('actividad-error-alert');
            if (errorAlert) {
                errorAlert.style.display = 'none';
            }
            data.responsablePrincipalId = Number(data.responsablePrincipalId);
            data.fechaUltimaActualizacion = new Date().toISOString().slice(0, 10);
            
            const corresponsablesCbs = document.querySelectorAll('#modal-body form input[name="corresponsablesIds"]:checked');
            data.corresponsablesIds = Array.from(corresponsablesCbs).map(cb => Number(cb.value));

            if (isNew) {
                await dataService.create('actividades', data);
                toast('Actividad creada', 'ok');
            } else {
                await dataService.update('actividades', actividad.id, data);
                toast('Actividad actualizada', 'ok');
            }
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        } catch (err) {
            console.error(err);
            let userMsg = err.message || 'No fue posible guardar la actividad.';
            if (userMsg.includes('blocked') || userMsg.includes('bloqueada')) {
                userMsg = 'Solicitud bloqueada por validación de seguridad (ej. liga con caracteres no permitidos).';
            } else if (userMsg.includes('{')) {
                try {
                    const jsonStr = userMsg.substring(userMsg.indexOf('{'));
                    const parsed = JSON.parse(jsonStr);
                    if (parsed.message) userMsg = parsed.message;
                } catch (_) {}
            }
            
            const errorAlert = document.getElementById('actividad-error-alert');
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

    const respSelect = document.querySelector('#modal-body form select[name="responsablePrincipalId"]');
    let previousResponsibleId = respSelect ? respSelect.value : '';
    const updateCorresponsablesChecklist = () => {
        const selectedId = respSelect ? respSelect.value : '';
        
        // Si el responsable anterior cambia y es válido, se marca automáticamente como corresponsable
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
    
    if (respSelect) {
        respSelect.addEventListener('change', updateCorresponsablesChecklist);
        updateCorresponsablesChecklist();
    }

    if (!isNew) {
        document.getElementById('btn-del-act').onclick = async () => {
            if (!confirm('¿Eliminar actividad y todos sus temas?')) return;
            const childTemas = _temasState.filter(t => t.actividadId === actividad.id);
            for (const t of childTemas) {
                await dataService.remove('temas', t.id);
            }
            await dataService.remove('actividades', actividad.id);
            toast('Actividad eliminada', 'ok');
            close();
            window.dispatchEvent(new CustomEvent('gestor:refresh'));
        };
    }
}
