import { escape, fmtDate, semaforo, daysFromToday, uniqueResponsables } from './utils.js';

export function renderResponsables(temas, actividades) {
    const cont = document.getElementById('responsables-grid');
    const personas = uniqueResponsables(actividades, temas);

    cont.innerHTML = personas.map(p => {
        const acts = actividades.filter(a => a.responsable === p);
        const activas = acts.filter(a => a.estatus !== 'Concluida');
        const vencidas = activas.filter(a => daysFromToday(a.fechaCompromiso) < 0);
        const porVencer = activas.filter(a => { const d = daysFromToday(a.fechaCompromiso); return d >= 0 && d <= 7; });
        return `
            <div class="resp-card">
                <h4>${escape(p)}</h4>
                <div class="stats">
                    <span><strong>${activas.length}</strong> activas</span>
                    <span style="color:var(--g-riesgo)"><strong>${vencidas.length}</strong> vencidas</span>
                    <span style="color:var(--g-proceso)"><strong>${porVencer.length}</strong> por vencer</span>
                </div>
                <ul>
                    ${activas.length
                        ? activas.sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''))
                            .map(a => `<li><span><span class="semaforo ${semaforo(a)}"></span> ${escape(a.actividad)}</span><small>${fmtDate(a.fechaCompromiso)}</small></li>`).join('')
                        : '<li style="color:var(--g-text-soft);justify-content:center">Sin actividades activas</li>'
                    }
                </ul>
            </div>`;
    }).join('');
}
