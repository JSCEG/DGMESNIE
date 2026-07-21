// Utilidades comunes.

function todayAtMidnight() {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
}

export const TODAY = todayAtMidnight();

export function parseDate(s) {
    if (!s) return null;
    const d = new Date(s + 'T00:00:00');
    return isNaN(d) ? null : d;
}

export function fmtDate(s) {
    const d = parseDate(s);
    if (!d) return '—';
    return d.toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function toDateOnly(d) {
    if (!(d instanceof Date) || isNaN(d)) return '';
    const pad = n => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function getPeriodRange(period, options = {}) {
    const { asDate = false } = options;
    const today = todayAtMidnight();
    const year = today.getFullYear();
    const month = today.getMonth();
    let start, end;

    switch (period) {
        case 'semana_actual': {
            const day = today.getDay();
            start = new Date(today);
            start.setDate(today.getDate() - (day === 0 ? 6 : day - 1));
            end = new Date(start);
            end.setDate(start.getDate() + 6);
            break;
        }
        case 'mes_actual':
            start = new Date(year, month, 1);
            end = new Date(year, month + 1, 0);
            break;
        case 'mes_anterior':
            start = new Date(year, month - 1, 1);
            end = new Date(year, month, 0);
            break;
        case 'dos_meses_atras':
            start = new Date(year, month - 2, 1);
            end = new Date(year, month - 1, 0);
            break;
        case 'anio_actual':
            start = new Date(year, 0, 1);
            end = new Date(year, 11, 31);
            break;
        default:
            return null;
    }

    start.setHours(0, 0, 0, 0);
    end.setHours(23, 59, 59, 999);
    return asDate ? { start, end } : { start: toDateOnly(start), end: toDateOnly(end) };
}

export function getPeriodLabel(value) {
    const labels = {
        '': 'Todo',
        semana_actual: 'Semana en curso',
        mes_actual: 'Mes en curso',
        mes_anterior: 'Mes anterior',
        dos_meses_atras: 'Dos meses atrás',
        anio_actual: 'Año en curso',
        '7': 'Próximos 7 días',
        '30': 'Próximos 30 días',
        vencidas: 'Solo vencidas'
    };
    return labels[value] || 'Todo';
}

export function daysBetween(a, b) {
    const ms = parseDate(b) - parseDate(a);
    return Math.round(ms / 86400000);
}

export function daysFromToday(s) {
    const d = parseDate(s); if (!d) return null;
    return Math.round((d - TODAY) / 86400000);
}

export function semaforo(tema) {
    if (tema.estatus === 'Concluida') return 'verde';
    if (tema.bloqueada) return 'gris';
    const dr = daysFromToday(tema.fechaCompromiso);
    if (dr === null) return 'gris';
    if (dr < 0) return 'rojo';
    if (dr <= 7) return 'amarillo';
    return 'verde';
}

export function semaforoTema(actividad, temas) {
    const ts = temas.filter(t => t.actividadId === actividad.id);
    if (!ts.length) return 'gris';
    if (ts.some(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) < 0)) return 'rojo';
    if (ts.some(t => t.estatus !== 'Concluida' && daysFromToday(t.fechaCompromiso) <= 7)) return 'amarillo';
    return 'verde';
}

export function avancePromedio(actividad, temas) {
    const ts = temas.filter(t => t.actividadId === actividad.id);
    if (!ts.length) return actividad.avanceGeneral || 0;
    return Math.round(ts.reduce((s, t) => s + (t.avance || 0), 0) / ts.length);
}

export function priClass(p) {
    return p === 'Alta' ? 'p-alta' : p === 'Media' ? 'p-media' : 'p-baja';
}
export function estClass(s) {
    if (s === 'Activo') return 's-activo';
    if (s === 'Pausado') return 's-pausa';
    if (s === 'Concluido') return 's-concluido';
    return '';
}

export function escape(s) {
    return String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
}

export function toast(msg, kind = '') {
    const root = document.getElementById('toast-root');
    if (!root) return;
    const el = document.createElement('div');
    el.className = `toast ${kind}`;
    el.textContent = msg;
    root.appendChild(el);
    setTimeout(() => el.remove(), 3000);
}

export function openModal(title, html, onSubmit) {
    const root = document.getElementById('modal-root');
    document.getElementById('modal-title').textContent = title;
    document.getElementById('modal-body').innerHTML = html;
    root.hidden = false;
    const close = () => { root.hidden = true; };
    document.getElementById('modal-close').onclick = close;
    root.querySelector('.modal-backdrop').onclick = close;
    const form = document.querySelector('#modal-body form');
    if (form && onSubmit) {
        form.onsubmit = async (e) => {
            e.preventDefault();
            const data = Object.fromEntries(new FormData(form).entries());
            await onSubmit(data, close);
        };
    }
    return close;
}

export function uniqueResponsables(actividades, temas) {
    const set = new Set();
    actividades.forEach(a => {
        const resp = a.responsablePrincipal || a.responsable;
        if (resp) set.add(resp.trim());
        if (a.corresponsables) a.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
    });
    temas.forEach(t => {
        const resp = t.responsablePrincipal || t.responsable;
        if (resp) set.add(resp.trim());
        if (t.corresponsables) t.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
        if (Array.isArray(t.etapas)) {
            t.etapas.forEach(e => {
                if (e.responsableNombre) set.add(e.responsableNombre.trim());
                if (e.corresponsables) e.corresponsables.forEach(c => { if (c.nombre) set.add(c.nombre.trim()); });
            });
        }
    });
    return [...set].filter(Boolean).sort((x, y) => x.localeCompare(y, 'es-MX', { sensitivity: 'base' }));
}

export function downloadCsv(filename, rows) {
    const csv = rows.map(r => r.map(c => `"${String(c ?? '').replace(/"/g, '""')}"`).join(',')).join('\n');
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob); a.download = filename; a.click();
    setTimeout(() => URL.revokeObjectURL(a.href), 500);
}
