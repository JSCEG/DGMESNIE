import { escape, semaforoTema, avancePromedio, daysFromToday, fmtDate } from './utils.js';
import { renderAllCharts } from './charts.js?v=charts-v3';
import { openTemaDetalle } from './actividades.js?v=etapas-correo-v2';
import { confirmarYEnviarRecordatorio } from './alertas.js?v=recordatorio-v1';

let currentDetailFilter = '';

function trackClass(estatus) {
    if (estatus === 'Vencida') return 'track--issue';
    if (estatus === 'En proceso') return 'track--progress';
    if (estatus === 'Concluida') return 'track--complete';
    return 'track--pending';
}

function pillClass(sem) {
    if (sem === 'rojo') return 'status-pill--issue';
    if (sem === 'amarillo') return 'status-pill--progress';
    if (sem === 'verde') return 'status-pill--complete';
    return '';
}

function statusClass(estatus) {
    if (estatus === 'Concluida') return 'status-pill--complete';
    if (estatus === 'En proceso') return 'status-pill--progress';
    if (estatus === 'Vencida') return 'status-pill--issue';
    return '';
}

function sortByDueDate(a, b) {
    const aDone = a.estatus === 'Concluida';
    const bDone = b.estatus === 'Concluida';
    if (aDone !== bDone) return aDone ? 1 : -1;

    const aDays = daysFromToday(a.fechaCompromiso);
    const bDays = daysFromToday(b.fechaCompromiso);
    const aValue = typeof aDays === 'number' ? aDays : Number.MAX_SAFE_INTEGER;
    const bValue = typeof bDays === 'number' ? bDays : Number.MAX_SAFE_INTEGER;
    return aValue - bValue;
}

function detailConfig(filter) {
    const labels = {
        actividades: {
            title: 'Actividades activas',
            summary: 'Actividades de la Dirección con temas asociados.'
        },
        todos: {
            title: 'Todos los temas',
            summary: 'Temas ordenados por fecha de compromiso.'
        },
        concluidas: {
            title: 'Temas concluidos',
            summary: 'Temas cerrados dentro del periodo seleccionado.'
        },
        'por-vencer': {
            title: 'Temas por vencer',
            summary: 'Temas con compromiso en los próximos 7 días.'
        },
        vencidas: {
            title: 'Temas vencidos',
            summary: 'Temas que requieren atención por fecha de compromiso.'
        },
        avance: {
            title: 'Avance de temas',
            summary: 'Temas ordenados por avance y fecha de compromiso.'
        }
    };

    return labels[filter] || labels.todos;
}

function temasForFilter(filter, actividades, temas) {
    let list = temas.slice();

    if (filter === 'concluidas') {
        list = list.filter(t => t.estatus === 'Concluida');
    } else if (filter === 'por-vencer') {
        list = list.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) >= 0 && daysFromToday(t.fechaCompromiso) <= 7);
    } else if (filter === 'vencidas') {
        list = list.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0);
    } else if (filter === 'avance') {
        return list.sort((a, b) => (a.avance || 0) - (b.avance || 0) || sortByDueDate(a, b));
    }

    return list.sort(sortByDueDate);
}

function openTablaForActividad(actividadId) {
    const tab = document.querySelector('.gestor-tab[data-view="tabla"]');
    if (tab) tab.click();

    setTimeout(() => {
        const filter = document.getElementById('filtro-tabla-tema');
        if (filter) {
            filter.value = actividadId;
            filter.dispatchEvent(new Event('change'));
        }
    }, 0);
}

function renderDetail(filter, actividades, temas) {
    const panel = document.getElementById('dashboard-detail-panel');
    const title = document.getElementById('dashboard-detail-title');
    const summary = document.getElementById('dashboard-detail-summary');
    const tbody = document.querySelector('#tbl-dashboard-detail tbody');
    const thead = document.querySelector('#tbl-dashboard-detail thead tr');
    if (!panel || !title || !summary || !tbody) return;

    currentDetailFilter = filter;
    document.querySelectorAll('[data-dashboard-filter]').forEach(card => {
        card.classList.toggle('is-active', card.dataset.dashboardFilter === filter);
    });

    const cfg = detailConfig(filter);
    const isActividades = filter === 'actividades';
    const list = isActividades
        ? actividades.filter(a => a.estatus === 'Activo')
            .map(a => ({
                ...a,
                temaCount: temas.filter(t => t.actividadId === a.id).length,
                avancePanel: avancePromedio(a, temas),
                semPanel: semaforoTema(a, temas)
            }))
            .sort((a, b) => sortByDueDate(a, b))
        : temasForFilter(filter, actividades, temas);
    title.textContent = cfg.title;
    summary.textContent = `${cfg.summary} ${list.length} registro${list.length === 1 ? '' : 's'}.`;
    if (thead) {
        thead.innerHTML = isActividades
            ? `
                <th>#</th>
                <th>Actividad</th>
                <th>Temas</th>
                <th>Responsable</th>
                <th>Compromiso</th>
                <th>Estado</th>
                <th>Avance</th>
                <th>Acción</th>`
            : `
                <th>#</th>
                <th>Tema</th>
                <th>Actividad</th>
                <th>Responsable</th>
                <th>Compromiso</th>
                <th>Estado</th>
                <th>Avance</th>
                <th>Acción</th>`;
    }

    tbody.innerHTML = isActividades
        ? list.length ? list.map((a, idx) => {
            const semText = a.semPanel === 'rojo' ? 'Vencida' : a.semPanel === 'amarillo' ? 'Por vencer' : a.semPanel === 'verde' ? 'Al corriente' : 'Sin temas';
            const semClass = a.semPanel === 'rojo' ? 'status-pill--issue' : a.semPanel === 'amarillo' ? 'status-pill--progress' : a.semPanel === 'verde' ? 'status-pill--complete' : '';
            const trackCls = a.semPanel === 'rojo' ? 'track--issue' : a.semPanel === 'amarillo' ? 'track--progress' : a.semPanel === 'verde' ? 'track--complete' : 'track--pending';
            return `
                <tr>
                    <td class="tabla-row-id">${idx + 1}</td>
                    <td><strong>${escape(a.actividad || '-')}</strong><br><small class="muted">${escape(a.categoria || '')}</small></td>
                    <td>${a.temaCount} tema${a.temaCount === 1 ? '' : 's'}</td>
                    <td>${escape(a.responsablePrincipal || 'Sin asignar')}</td>
                    <td>${fmtDate(a.fechaCompromiso)}</td>
                    <td><span class="status-pill ${semClass}">${semText}</span></td>
                    <td><div class="track ${trackCls}" style="width:100px"><span style="width:${a.avancePanel || 0}%"></span></div><small class="muted">${a.avancePanel || 0}%</small></td>
                    <td><button type="button" class="internal-button" data-dashboard-activity="${a.id}" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem">Ver temas</button></td>
                </tr>`;
        }).join('') : '<tr><td colspan="8" class="muted" style="text-align:center;padding:1rem">Sin actividades para mostrar</td></tr>'
        : list.length
        ? list.map((t, idx) => {
            const actividad = actividades.find(a => a.id === t.actividadId);
            const rest = daysFromToday(t.fechaCompromiso);
            const showReminder = t.estatus !== 'Concluida' && t.responsableId && typeof rest === 'number' && rest <= 7;
            const dueText = typeof rest === 'number'
                ? rest < 0 ? `Vencido hace ${Math.abs(rest)}d` : `Quedan ${rest}d`
                : 'Sin fecha';
            return `
                <tr>
                    <td class="tabla-row-id">${idx + 1}</td>
                    <td><strong>${escape(t.tema || '-')}</strong></td>
                    <td>${escape(actividad?.actividad || '-')}<br><small class="muted">${escape(actividad?.categoria || '')}</small></td>
                    <td>${escape(t.responsable || 'Sin asignar')}</td>
                    <td>${fmtDate(t.fechaCompromiso)}<br><small class="muted">${escape(dueText)}</small></td>
                    <td><span class="status-pill ${statusClass(t.estatus)}">${escape(t.estatus || 'Pendiente')}</span></td>
                    <td><div class="track ${trackClass(t.estatus)}" style="width:100px"><span style="width:${t.avance || 0}%"></span></div><small class="muted">${t.avance || 0}%</small></td>
                    <td>
                        <span class="tabla-actions-cell">
                            ${showReminder ? `<button type="button" class="tabla-action-btn" data-dashboard-reminder="${t.id}" title="Enviar recordatorio por correo"><i class="bi bi-send"></i></button>` : ''}
                            <button type="button" class="internal-button" data-dashboard-view="${t.id}" style="min-height:32px;padding:.3rem .7rem;font-size:.78rem">Ver</button>
                        </span>
                    </td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="8" class="muted" style="text-align:center;padding:1rem">Sin temas para mostrar</td></tr>';

    tbody.querySelectorAll('[data-dashboard-activity]').forEach(btn => {
        btn.onclick = () => openTablaForActividad(btn.dataset.dashboardActivity);
    });

    tbody.querySelectorAll('[data-dashboard-view]').forEach(btn => {
        btn.onclick = () => openTemaDetalle(temas.find(t => t.id === btn.dataset.dashboardView), actividades);
    });

    tbody.querySelectorAll('[data-dashboard-reminder]').forEach(btn => {
        btn.onclick = () => {
            const tema = temas.find(t => t.id === btn.dataset.dashboardReminder);
            if (tema) confirmarYEnviarRecordatorio(tema, actividades);
        };
    });

    panel.hidden = false;
}

function wireDashboardDetail(actividades, temas) {
    document.querySelectorAll('[data-dashboard-filter]').forEach(card => {
        const open = () => renderDetail(card.dataset.dashboardFilter || 'todos', actividades, temas);
        card.onclick = open;
        card.onkeydown = e => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                open();
            }
        };
    });

    const clear = document.getElementById('dashboard-detail-clear');
    if (clear) {
        clear.onclick = () => {
            const panel = document.getElementById('dashboard-detail-panel');
            if (panel) panel.hidden = true;
            currentDetailFilter = '';
            document.querySelectorAll('[data-dashboard-filter]').forEach(card => card.classList.remove('is-active'));
        };
    }

    const openTabla = document.getElementById('dashboard-detail-open');
    if (openTabla) {
        openTabla.onclick = () => {
            const tab = document.querySelector('.gestor-tab[data-view="tabla"]');
            if (tab) tab.click();
        };
    }

    if (currentDetailFilter) {
        renderDetail(currentDetailFilter, actividades, temas);
    }
}

export function renderDashboard(actividades, temas) {
    const vencidos = temas.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0);
    const porVencer = temas.filter(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) >= 0 && daysFromToday(t.fechaCompromiso) <= 7);
    const concluidos = temas.filter(t => t.estatus === 'Concluida');
    const avance = temas.length
        ? Math.round(temas.reduce((s, t) => s + (t.avance || 0), 0) / temas.length)
        : 0;

    document.getElementById('kpi-temas').textContent = actividades.filter(a => a.estatus === 'Activo').length;
    document.getElementById('kpi-actividades').textContent = temas.length;
    document.getElementById('kpi-vencidas').textContent = vencidos.length;
    document.getElementById('kpi-por-vencer').textContent = porVencer.length;
    document.getElementById('kpi-concluidas').textContent = concluidos.length;
    document.getElementById('kpi-avance').textContent = avance + '%';
    wireDashboardDetail(actividades, temas);

    // Gráficos D3
    renderAllCharts(actividades, temas);

    // Actividades críticas (antes Temas críticos)
    const criticos = actividades
        .map(a => ({ a, sem: semaforoTema(a, temas), avance: avancePromedio(a, temas) }))
        .filter(x => x.sem === 'rojo' || x.sem === 'amarillo')
        .sort((a, b) => (a.sem === 'rojo' ? 0 : 1) - (b.sem === 'rojo' ? 0 : 1));

    const tbody = document.querySelector('#tbl-criticos tbody');
    tbody.innerHTML = criticos.length
        ? criticos.map(({ a, sem, avance }) => {
            const trackCls = sem === 'rojo' ? 'track--issue' : sem === 'amarillo' ? 'track--progress' : 'track--complete';
            const pillTxt = sem === 'rojo' ? 'Vencido' : sem === 'amarillo' ? 'Por vencer' : 'En curso';
            return `
                <tr>
                    <td><strong>${escape(a.actividad)}</strong><br><small class="muted">${escape(a.categoria || '')}</small></td>
                    <td>${escape(a.responsablePrincipal)}</td>
                    <td><div class="track ${trackCls}" style="width:120px"><span style="width:${avance}%"></span></div><small class="muted">${avance}%</small></td>
                    <td>${fmtDate(a.fechaCompromiso)}</td>
                    <td><span class="status-pill ${pillClass(sem)}">${pillTxt}</span></td>
                </tr>`;
        }).join('')
        : '<tr><td colspan="5" class="muted" style="text-align:center;padding:1rem">Sin actividades críticas</td></tr>';
}
