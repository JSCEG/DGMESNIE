import { escape, fmtDate, semaforo, daysFromToday, uniqueResponsables, openModal, toast } from './utils.js';

export function renderResponsables(temas, actividades) {
    const cont = document.getElementById('responsables-grid');
    if (!cont) return;
    const personas = uniqueResponsables(actividades, temas);

    cont.innerHTML = personas.map(p => {
        const acts = actividades.filter(a => a.responsable === p);
        const activas = acts.filter(a => a.estatus !== 'Concluida');
        const completadas = acts.filter(a => a.estatus === 'Concluida');
        const vencidas = activas.filter(a => daysFromToday(a.fechaCompromiso) < 0);
        const porVencer = activas.filter(a => { const d = daysFromToday(a.fechaCompromiso); return d >= 0 && d <= 7; });
        
        // Sort: active ones first (sorted by compromiso), then completed ones
        const sortedActs = [...activas].sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''))
            .concat(completadas.sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || '')));

        return `
            <div class="resp-card">
                <h4>${escape(p)}</h4>
                <div class="stats" style="margin-bottom: 0.75rem;">
                    <span><strong>${activas.length}</strong> activas</span>
                    <span style="color:var(--ok)"><strong>${completadas.length}</strong> concluidas</span>
                    <span style="color:var(--riesgo)"><strong>${vencidas.length}</strong> vencidas</span>
                    <span style="color:var(--proceso)"><strong>${porVencer.length}</strong> por vencer</span>
                </div>
                <ul>
                    ${sortedActs.length
                        ? sortedActs.map(a => {
                            const isComp = a.estatus === 'Concluida';
                            return `<li style="${isComp ? 'opacity: 0.65;' : ''}">
                                <span>
                                    <span class="semaforo ${semaforo(a)}"></span> 
                                    ${isComp ? `<del>${escape(a.actividad)}</del>` : escape(a.actividad)}
                                </span>
                                <small>${fmtDate(a.fechaCompromiso)}</small>
                            </li>`;
                          }).join('')
                        : '<li style="color:var(--g-text-soft);justify-content:center">Sin actividades registradas</li>'
                    }
                </ul>
                <div style="margin-top: 1rem; border-top: 1px solid var(--borde); padding-top: 0.75rem; display: flex; justify-content: flex-end;">
                    <button class="internal-button btn-ver-desempeno" data-resp="${escape(p)}" style="min-height: 32px; padding: 0.3rem 0.8rem; font-size: 0.78rem; display: inline-flex; align-items: center; gap: 6px;">
                        <i class="fa-solid fa-chart-pie"></i> Ver desempeño
                    </button>
                </div>
            </div>`;
    }).join('');

    // Bind event listeners
    cont.querySelectorAll('.btn-ver-desempeno').forEach(btn => {
        btn.onclick = () => {
            const respName = btn.dataset.resp;
            const acts = actividades.filter(a => a.responsable === respName);
            mostrarMiniDashboard(respName, acts);
        };
    });
}

function mostrarMiniDashboard(responsableName, acts) {
    const total = acts.length;
    const concluidas = acts.filter(a => a.estatus === 'Concluida');
    const activas = acts.filter(a => a.estatus !== 'Concluida');
    const vencidas = activas.filter(a => daysFromToday(a.fechaCompromiso) < 0);
    const avanceProm = total > 0 ? Math.round(acts.reduce((s, a) => s + (a.avance || 0), 0) / total) : 0;

    const html = `
        <div class="mini-dashboard" style="display: flex; flex-direction: column; gap: 1.25rem; font-family: Montserrat, sans-serif; color: var(--texto);">
            <!-- KPI Summary Grid -->
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(110px, 1fr)); gap: 10px;">
                <div style="background: rgba(15, 23, 42, 0.03); border: 1px solid var(--borde); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--texto-suave); font-weight: 700;">Total</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--texto);">${total}</div>
                </div>
                <div style="background: rgba(2, 122, 72, 0.05); border: 1px solid rgba(2, 122, 72, 0.1); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--ok); font-weight: 700;">Concluidas</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--ok);">${concluidas.length}</div>
                </div>
                <div style="background: rgba(138, 0, 49, 0.05); border: 1px solid rgba(138, 0, 49, 0.1); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--guinda); font-weight: 700;">Vencidas</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--riesgo);">${vencidas.length}</div>
                </div>
                <div style="background: rgba(30, 91, 79, 0.05); border: 1px solid rgba(30, 91, 79, 0.1); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--verde); font-weight: 700;">Avance Promedio</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--verde);">${avanceProm}%</div>
                </div>
            </div>

            <!-- Distribution Chart Card -->
            <div style="border: 1px solid var(--borde); border-radius: 16px; padding: 12px; background: #fff;">
                <div id="mini-dashboard-chart" style="height: 180px; width: 100%;"></div>
            </div>

            <!-- Activities Table Card -->
            <div style="border: 1px solid var(--borde); border-radius: 16px; overflow: hidden; background: #fff;">
                <div style="padding: 12px; border-bottom: 1px solid var(--borde); background: rgba(15, 23, 42, 0.02); font-weight: 700; font-size: 0.85rem;">
                    Desglose de Actividades
                </div>
                <div style="max-height: 220px; overflow-y: auto;">
                    <table style="width: 100%; border-collapse: collapse; font-size: 0.8rem; text-align: left;">
                        <thead>
                            <tr style="border-bottom: 1px solid var(--borde); background: rgba(15, 23, 42, 0.01); color: var(--texto-suave);">
                                <th style="padding: 8px 12px; width: 80px;">Clave</th>
                                <th style="padding: 8px 12px;">Actividad</th>
                                <th style="padding: 8px 12px; width: 100px;">Compromiso</th>
                                <th style="padding: 8px 12px; width: 100px;">Estatus</th>
                                <th style="padding: 8px 12px; text-align: right; width: 70px;">Avance</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${acts.length ? acts.map(a => `
                                <tr style="border-bottom: 1px solid rgba(15,23,42,0.05);">
                                    <td style="padding: 8px 12px; font-weight: 700; color: var(--guinda);">${escape(a.clave)}</td>
                                    <td style="padding: 8px 12px; font-weight: 600;">
                                        ${escape(a.actividad)}
                                        ${a.evidenciaUrl ? `
                                            <a href="${escape(a.evidenciaUrl)}" target="_blank" rel="noopener" style="margin-left: 6px; display: inline-flex; align-items: center; color: #b48934;" title="Ver evidencia">
                                                <i class="fa-solid fa-folder-open" style="font-size: 0.95rem;"></i>
                                            </a>
                                        ` : ''}
                                    </td>
                                    <td style="padding: 8px 12px;">${fmtDate(a.fechaCompromiso)}</td>
                                    <td style="padding: 8px 12px;">
                                        <span class="chip" style="font-size: 0.7rem; padding: 2px 8px; text-transform: capitalize; background: ${
                                            a.estatus === 'Concluida' ? 'rgba(2, 122, 72, 0.1)' :
                                            a.estatus === 'En proceso' ? 'rgba(138, 0, 49, 0.1)' :
                                            a.estatus === 'Vencida' ? 'rgba(161, 77, 106, 0.1)' : 'rgba(102, 112, 133, 0.1)'
                                        }; color: ${
                                            a.estatus === 'Concluida' ? 'var(--ok)' :
                                            a.estatus === 'En proceso' ? 'var(--guinda)' :
                                            a.estatus === 'Vencida' ? 'var(--riesgo)' : 'var(--texto-suave)'
                                        };">${a.estatus}</span>
                                    </td>
                                    <td style="padding: 8px 12px; text-align: right; font-weight: 700;">${a.avance}%</td>
                                </tr>
                            `).join('') : `
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 20px; color: var(--texto-suave);">Sin actividades asignadas</td>
                                </tr>
                            `}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;

    openModal(`Desempeño de ${responsableName}`, html, null);

    // Initialize Highcharts Pie Chart inside the modal body
    if (window.Highcharts && total > 0) {
        const counts = acts.reduce((m, a) => { m[a.estatus] = (m[a.estatus] || 0) + 1; return m; }, {});
        const colors = {
            'Concluida': '#027a48',
            'En proceso': '#8a0031',
            'Vencida': '#a14d6a',
            'Pendiente': '#667085'
        };
        const data = Object.entries(counts).map(([name, y]) => ({
            name,
            y,
            color: colors[name] || '#8a0031'
        }));

        Highcharts.chart('mini-dashboard-chart', {
            chart: {
                type: 'pie',
                height: 160,
                spacingTop: 0,
                spacingBottom: 0,
                spacingLeft: 0,
                spacingRight: 0,
                backgroundColor: 'transparent'
            },
            credits: { enabled: false },
            title: { text: '' },
            exporting: { enabled: false },
            tooltip: { pointFormat: '<b>{point.y}</b> ({point.percentage:.0f}%)' },
            plotOptions: {
                pie: {
                    innerSize: '65%',
                    dataLabels: { enabled: false },
                    showInLegend: true
                }
            },
            legend: {
                align: 'right',
                verticalAlign: 'middle',
                layout: 'vertical',
                itemStyle: { fontSize: '11px', fontWeight: '700' }
            },
            series: [{ name: 'Actividades', data }]
        });
    }
}
