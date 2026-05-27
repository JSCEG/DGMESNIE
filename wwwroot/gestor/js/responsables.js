import { escape, fmtDate, semaforo, daysFromToday, uniqueResponsables, openModal, toast } from './utils.js';

export function renderResponsables(actividades, temas) {
    const cont = document.getElementById('responsables-grid');
    if (!cont) return;
    const personas = uniqueResponsables(actividades, temas);

    cont.innerHTML = personas.map(p => {
        const ts = temas.filter(t => t.responsable === p || (t.corresponsables && t.corresponsables.some(c => c.nombre === p)));
        const activas = ts.filter(t => t.estatus !== 'Concluida');
        const completadas = ts.filter(t => t.estatus === 'Concluida');
        const vencidas = activas.filter(t => daysFromToday(t.fechaCompromiso) < 0);
        const porVencer = activas.filter(t => { const d = daysFromToday(t.fechaCompromiso); return d >= 0 && d <= 7; });
        const coCount = ts.filter(t => t.responsable !== p).length;
        
        // Sort: active ones first (sorted by compromiso), then completed ones
        const sortedTs = [...activas].sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || ''))
            .concat(completadas.sort((a, b) => (a.fechaCompromiso || '').localeCompare(b.fechaCompromiso || '')));

        return `
            <div class="resp-card">
                <h4>${escape(p)}</h4>
                <div class="stats" style="margin-bottom: 0.75rem; flex-wrap: wrap;">
                    <span><strong>${activas.length}</strong> activas</span>
                    <span style="color:var(--ok)"><strong>${completadas.length}</strong> concluidas</span>
                    <span style="color:var(--riesgo)"><strong>${vencidas.length}</strong> vencidas</span>
                    <span style="color:var(--proceso)"><strong>${porVencer.length}</strong> por vencer</span>
                    ${coCount > 0 ? `<span style="color:var(--guinda)"><strong>${coCount}</strong> co-responsable</span>` : ''}
                </div>
                <ul>
                    ${sortedTs.length
                        ? sortedTs.map(t => {
                            const isComp = t.estatus === 'Concluida';
                            const isCo = t.responsable !== p;
                            return `<li style="${isComp ? 'opacity: 0.65;' : ''}">
                                <span>
                                    <span class="semaforo ${semaforo(t)}"></span> 
                                    ${isComp ? `<del>${escape(t.tema)}</del>` : escape(t.tema)}
                                    ${isCo ? ' <span class="badge badge-co" style="font-size: 0.65rem; background: rgba(138, 0, 49, 0.08); color: var(--guinda); padding: 1px 4px; border-radius: 4px;">Co</span>' : ''}
                                </span>
                                <small>${fmtDate(t.fechaCompromiso)}</small>
                            </li>`;
                          }).join('')
                        : '<li style="color:var(--g-text-soft);justify-content:center">Sin temas registrados</li>'
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
            const ts = temas.filter(t => t.responsable === respName || (t.corresponsables && t.corresponsables.some(c => c.nombre === respName)));
            mostrarMiniDashboard(respName, ts);
        };
    });
}

function mostrarMiniDashboard(responsableName, ts) {
    const total = ts.length;
    const concluidas = ts.filter(t => t.estatus === 'Concluida');
    const activas = ts.filter(t => t.estatus !== 'Concluida');
    const vencidas = activas.filter(t => daysFromToday(t.fechaCompromiso) < 0);
    const coCount = ts.filter(t => t.responsable !== responsableName).length;
    const avanceProm = total > 0 ? Math.round(ts.reduce((s, t) => s + (t.avance || 0), 0) / total) : 0;

    const html = `
        <div class="mini-dashboard" style="display: flex; flex-direction: column; gap: 1.25rem; font-family: Montserrat, sans-serif; color: var(--texto);">
            <!-- KPI Summary Grid -->
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(110px, 1fr)); gap: 10px;">
                <div style="background: rgba(15, 23, 42, 0.03); border: 1px solid var(--borde); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--texto-suave); font-weight: 700;">Total</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--texto);">${total}</div>
                </div>
                <div style="background: rgba(2, 122, 72, 0.05); border: 1px solid rgba(2, 122, 72, 0.1); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--ok); font-weight: 700;">Concluidos</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--ok);">${concluidas.length}</div>
                </div>
                <div style="background: rgba(138, 0, 49, 0.05); border: 1px solid rgba(138, 0, 49, 0.1); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--riesgo); font-weight: 700;">Vencidos</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--riesgo);">${vencidas.length}</div>
                </div>
                <div style="background: rgba(138, 0, 49, 0.02); border: 1px solid rgba(138, 0, 49, 0.08); border-radius: 12px; padding: 10px; text-align: center;">
                    <span style="font-size: 0.75rem; color: var(--guinda); font-weight: 700;">Corresponsable</span>
                    <div style="font-size: 1.5rem; font-weight: 800; margin-top: 4px; color: var(--guinda);">${coCount}</div>
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

            <!-- Temas Table Card -->
            <div style="border: 1px solid var(--borde); border-radius: 16px; overflow: hidden; background: #fff;">
                <div style="padding: 12px; border-bottom: 1px solid var(--borde); background: rgba(15, 23, 42, 0.02); font-weight: 700; font-size: 0.85rem;">
                    Desglose de Temas
                </div>
                <div style="max-height: 220px; overflow-y: auto;">
                    <table style="width: 100%; border-collapse: collapse; font-size: 0.8rem; text-align: left;">
                        <thead>
                            <tr style="border-bottom: 1px solid var(--borde); background: rgba(15, 23, 42, 0.01); color: var(--texto-suave);">
                                <th style="padding: 8px 12px; width: 80px;">Clave</th>
                                <th style="padding: 8px 12px;">Tema</th>
                                <th style="padding: 8px 12px; width: 100px;">Compromiso</th>
                                <th style="padding: 8px 12px; width: 100px;">Estatus</th>
                                <th style="padding: 8px 12px; text-align: right; width: 70px;">Avance</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${ts.length ? ts.map(t => `
                                <tr style="border-bottom: 1px solid rgba(15,23,42,0.05);">
                                    <td style="padding: 8px 12px; font-weight: 700; color: var(--guinda);">${escape(t.clave)}</td>
                                    <td style="padding: 8px 12px; font-weight: 600;">
                                        ${escape(t.tema)}
                                        ${t.responsable !== responsableName ? ' <span class="badge badge-co" style="font-size: 0.65rem; background: rgba(138, 0, 49, 0.08); color: var(--guinda); padding: 1px 4px; border-radius: 4px; font-weight: bold; display: inline-block; vertical-align: middle;">Co</span>' : ''}
                                        ${t.evidenciaUrl ? `
                                            <a href="${escape(t.evidenciaUrl)}" target="_blank" rel="noopener" style="margin-left: 6px; display: inline-flex; align-items: center; color: #b48934;" title="Ver evidencia">
                                                <i class="fa-solid fa-folder-open" style="font-size: 0.95rem;"></i>
                                            </a>
                                        ` : ''}
                                    </td>
                                    <td style="padding: 8px 12px;">${fmtDate(t.fechaCompromiso)}</td>
                                    <td style="padding: 8px 12px;">
                                        <span class="chip" style="font-size: 0.7rem; padding: 2px 8px; text-transform: capitalize; background: ${
                                            t.estatus === 'Concluida' ? 'rgba(2, 122, 72, 0.1)' :
                                            t.estatus === 'En proceso' ? 'rgba(138, 0, 49, 0.1)' :
                                            t.estatus === 'Vencida' ? 'rgba(161, 77, 106, 0.1)' : 'rgba(102, 112, 133, 0.1)'
                                        }; color: ${
                                            t.estatus === 'Concluida' ? 'var(--ok)' :
                                            t.estatus === 'En proceso' ? 'var(--guinda)' :
                                            t.estatus === 'Vencida' ? 'var(--riesgo)' : 'var(--texto-suave)'
                                        };">${t.estatus}</span>
                                    </td>
                                    <td style="padding: 8px 12px; text-align: right; font-weight: 700;">${t.avance}%</td>
                                </tr>
                            `).join('') : `
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 20px; color: var(--texto-suave);">Sin temas asignados</td>
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
        const counts = ts.reduce((m, t) => { m[t.estatus] = (m[t.estatus] || 0) + 1; return m; }, {});
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
            series: [{ name: 'Temas', data }]
        });
    }
}
