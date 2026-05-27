import { escape, parseDate, semaforo } from './utils.js';
import { openTemaModal } from './actividades.js';

let _ref = new Date(2026, 4, 1); // Mayo 2026

export function renderCalendario(actividades, temas) {
    const label = document.getElementById('cal-label');
    const cont = document.getElementById('calendar');
    if (!label || !cont) return;
    label.textContent = _ref.toLocaleDateString('es-MX', { month: 'long', year: 'numeric' });

    const y = _ref.getFullYear(), m = _ref.getMonth();
    const first = new Date(y, m, 1);
    const startDow = (first.getDay() + 6) % 7; // Lunes=0
    const daysInMonth = new Date(y, m + 1, 0).getDate();
    const daysInPrev = new Date(y, m, 0).getDate();

    const dows = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
    let html = '<div class="cal-grid">';
    dows.forEach(d => html += `<div class="cal-dow">${d}</div>`);

    const totalCells = Math.ceil((startDow + daysInMonth) / 7) * 7;
    for (let i = 0; i < totalCells; i++) {
        let dayNum, dCls = '', dDate;
        if (i < startDow) { dayNum = daysInPrev - startDow + 1 + i; dDate = new Date(y, m - 1, dayNum); dCls = 'other'; }
        else if (i < startDow + daysInMonth) { dayNum = i - startDow + 1; dDate = new Date(y, m, dayNum); }
        else { dayNum = i - startDow - daysInMonth + 1; dDate = new Date(y, m + 1, dayNum); dCls = 'other'; }

        const isoDate = dDate.toISOString().slice(0, 10);
        const events = temas.filter(t => t.fechaCompromiso === isoDate);
        html += `<div class="cal-day ${dCls}"><span class="n">${dayNum}</span>${events.map(t => {
            const sem = semaforo(t);
            const cls = sem === 'rojo' ? 'r-riesgo' : sem === 'amarillo' ? 'r-proceso' : 'r-ok';
            return `<span class="ev ${cls}" data-id="${t.id}" title="${escape(t.tema)}">${escape(t.tema)}</span>`;
        }).join('')}</div>`;
    }
    html += '</div>';
    cont.innerHTML = html;

    cont.querySelectorAll('.ev').forEach(ev => {
        ev.onclick = () => openTemaModal(temas.find(t => t.id === ev.dataset.id), actividades);
    });
}

export function calPrev() { _ref.setMonth(_ref.getMonth() - 1); }
export function calNext() { _ref.setMonth(_ref.getMonth() + 1); }
