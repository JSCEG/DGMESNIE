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

export function daysBetween(a, b) {
    const ms = parseDate(b) - parseDate(a);
    return Math.round(ms / 86400000);
}

export function daysFromToday(s) {
    const d = parseDate(s); if (!d) return null;
    return Math.round((d - TODAY) / 86400000);
}

export function semaforo(actividad) {
    if (actividad.estatus === 'Concluida') return 'verde';
    if (actividad.bloqueada) return 'gris';
    const dr = daysFromToday(actividad.fechaCompromiso);
    if (dr === null) return 'gris';
    if (dr < 0) return 'rojo';
    if (dr <= 7) return 'amarillo';
    return 'verde';
}

export function semaforoTema(tema, actividades) {
    const acts = actividades.filter(a => a.temaId === tema.id);
    if (!acts.length) return 'gris';
    if (acts.some(a => a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) < 0)) return 'rojo';
    if (acts.some(a => a.estatus !== 'Concluida' && daysFromToday(a.fechaCompromiso) <= 7)) return 'amarillo';
    return 'verde';
}

export function avancePromedio(tema, actividades) {
    const acts = actividades.filter(a => a.temaId === tema.id);
    if (!acts.length) return tema.avanceGeneral || 0;
    return Math.round(acts.reduce((s, a) => s + (a.avance || 0), 0) / acts.length);
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
    actividades.forEach(a => { if (a.responsable) set.add(a.responsable); });
    temas.forEach(t => { if (t.responsablePrincipal) set.add(t.responsablePrincipal); });
    return [...set].sort();
}

export function downloadCsv(filename, rows) {
    const csv = rows.map(r => r.map(c => `"${String(c ?? '').replace(/"/g, '""')}"`).join(',')).join('\n');
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob); a.download = filename; a.click();
    setTimeout(() => URL.revokeObjectURL(a.href), 500);
}
