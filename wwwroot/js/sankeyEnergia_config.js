async function cargarColoresEnergeticos() {
    const res = await fetch('/SankeySener/GetEnergeticColor');
    const colors = await res.json();

    const energeticColors = {};

    colors.forEach(c => {
        energeticColors[c.nombre] = c.colorHex;
    });

    console.log("Colores cargados desde BD:", energeticColors);

    window.sankeyConfig = {

        energeticColors: energeticColors,

        // Colores para indicadores de eficiencia
        "efficiencyColors": {
            "efficient": "#38a169",
            "warning": "#dd6b20",
            "critical": "#e53e3e"
        },
        "layoutConfig": {
            "nodeAlign": "left",
            "nodeGap": 15,
            "nodeWidth": 30,
            "layoutIterations": 0,
            "curveness": 0.7
        },
        "columnWidths": {
            "0": 15,
            "1": 35,
            "2": 30,
            "3": 25,
            "4": 1,
            "5": 20
        },
        "colorBy": "child",
        "categoryColors": {
            "Fuentes Primarias": "#2E7D32",
            "Fuentes Secundarias": "#1565C0",
            "Energéticos Primarios": "#388E3C",
            "Energéticos Secundarios": "#1976D2",
            "Transformación": "#F57C00",
            "Sectores de Consumo": "#D32F2F",
            "Pérdidas y Exportaciones": "#7B1FA2",
            "Otros": "#616161"
        },
        "decalEnabled": true,
        "linkMinValue": 0,
        "flowPolicy": "bySign",
        "normalizeBy": "year",
        "curvenessAuto": false,
        "columnas": [
            {
                "nombre": "Origen",
                "mostrar": "Padre",
                "filtroTipo": "Energía Primaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_IEP_1",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 300
                    },
                    {
                        "nombre": "Importación EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "y": 100,
                        "flow": "source"
                    },
                    {
                        "nombre": "V.I. y Dif. Est. EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 3,
                        "flow": "source"
                    },
                    {
                        "nombre": "SPACER_BIG_IEP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 2000
                    },
                    {
                        "nombre": "Producción",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2,
                        "y": 300,
                        "flow": "source"
                    }
                ]
            },
            {
                "nombre": "Energía Primaria",
                "mostrar": "Hijo",
                "filtroTipo": "Energía Primaria",
                "padre": "Oferta Interna Bruta EP",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_EPP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 2500
                    },
                    {
                        "nombre": "Petróleo crudo",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 0
                    },
                    {
                        "nombre": "Gas natural",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 1
                    },
                    {
                        "nombre": "Carbón mineral",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 2
                    },
                    {
                        "nombre": "Condensados",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 3
                    },
                    {
                        "nombre": "Energía Nuclear",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 4
                    },
                    {
                        "nombre": "Energía Hidráulica",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 5
                    },
                    {
                        "nombre": "Energía Geotérmica",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 6
                    },
                    {
                        "nombre": "Energía solar",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 7
                    },
                    {
                        "nombre": "Energía eólica",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 8
                    },
                    {
                        "nombre": "Bagazo de caña",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 9
                    },
                    {
                        "nombre": "Leña",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 10
                    },
                    {
                        "nombre": "Biogás",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 11
                    }
                ]
            },
            {
                "nombre": "Salidas de Energía Primaria",
                "mostrar": "Padre",
                "filtroTipo": "Energía Primaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "depth": 2,
                        "esEspaciador": true,
                        "valorEspaciador": 15500
                    },
                    {
                        "nombre": "Exportación EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "depth": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Energía No Aprovechada EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2,
                        "depth": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Consumo Propio del Sector EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 3,
                        "depth": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Pérdidas técnicas por transporte, transmisión y distribución EP",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 4,
                        "depth": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Variación de Inventarios EP (-)",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 4,
                        "depth": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Diferencia Estadística EP (-)",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 5,
                        "depth": 2,
                        "flow": "sink"
                    }
                ]
            },
            {
                "nombre": "Transformaciones",
                "mostrar": "Padre",
                "filtroTipo": "Energía Primaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_3",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 7500
                    },
                    {
                        "nombre": "Coquizadoras y Hornos",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "depth": 3
                    },
                    {
                        "nombre": "Plantas de Gas y Fraccionadoras",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2,
                        "depth": 3
                    },
                    {
                        "nombre": "Refinerías y Despuntadoras",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 3,
                        "depth": 3
                    }
                ]
            },
            {
                "nombre": "Origen Energía Secundaria",
                "mostrar": "Padre",
                "filtroTipo": "Energía Secundaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_4",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 1000
                    },
                    {
                        "nombre": "V.I. y Dif. Est. ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "flow": "source"
                    },
                    {
                        "nombre": "Importación ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "flow": "source"
                    }
                ]
            },
            {
                "nombre": "Energía Secundaria",
                "mostrar": "Hijo",
                "filtroTipo": "Energía Secundaria",
                "padre": "Oferta Interna Bruta EP",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_5",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 4000
                    },
                    {
                        "nombre": "Coque de carbón",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 0,
                        "depth": 5
                    },
                    {
                        "nombre": "Coque de petróleo",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 1,
                        "depth": 5
                    },
                    {
                        "nombre": "Gas licuado de petróleo",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 2,
                        "depth": 5
                    },
                    {
                        "nombre": "Gasolinas y naftas",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 3,
                        "depth": 5
                    },
                    {
                        "nombre": "Querosenos",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 4,
                        "depth": 5
                    },
                    {
                        "nombre": "Diesel",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 5,
                        "depth": 5
                    },
                    {
                        "nombre": "Combustóleo",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 6,
                        "depth": 5
                    },
                    {
                        "nombre": "Otros energéticos",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 7,
                        "depth": 5
                    },
                    {
                        "nombre": "Gas Seco",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 8,
                        "depth": 5
                    },
                    {
                        "nombre": "SPACER_BIG_5B",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 9,
                        "esEspaciador": true,
                        "valorEspaciador": 3000
                    }
                ]
            },
            {
                "nombre": "Transformaciones a Centrales Eléctricas",
                "mostrar": "Padre",
                "filtroTipo": "Todos",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_7",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "esEspaciador": true,
                        "valorEspaciador": 11500
                    },
                    {
                        "nombre": "Centrales Eléctricas",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2
                    }
                ]
            },
            {
                "nombre": "Energía Secundaria",
                "mostrar": "Hijo",
                "filtroTipo": "Energía Secundaria",
                "padre": "Oferta Interna Bruta EP",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_E",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 11500
                    },
                    {
                        "nombre": "Energía eléctrica",
                        "tipo": "Hijo",
                        "visible": true,
                        "padre": "Oferta Interna Bruta EP",
                        "posicion": 9
                    }
                ]
            },
            {
                "nombre": "Salidas de Energía Secundaria",
                "mostrar": "Padre",
                "filtroTipo": "Energía Secundaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 16000
                    },
                    {
                        "nombre": "Exportación ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Energía No Aprovechada ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Consumo Propio del Sector ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 3,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Pérdidas técnicas por transporte, transmisión y distribución ES",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 4,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Variación de Inventarios ES (-)",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 4,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Diferencia Estadística ES (-)",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 5,
                        "flow": "sink"
                    }
                ]
            },
            {
                "nombre": "Sectores de Consumo",
                "mostrar": "Padre",
                "filtroTipo": "Energía Secundaria",
                "alineacionVertical": "abajo",
                "nodos": [
                    {
                        "nombre": "SPACER_BIG_sect",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "esEspaciador": true,
                        "valorEspaciador": 3500
                    },
                    {
                        "nombre": "Industrial",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 0,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Transporte",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 1,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Agropecuario",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 2,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Comercial",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 3,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Público",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 4,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Residencial",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 5,
                        "flow": "sink"
                    },
                    {
                        "nombre": "Consumo final no energético",
                        "tipo": "Padre",
                        "visible": true,
                        "posicion": 7,
                        "flow": "sink"
                    }
                ]
            }
        ],
        "enlaces": [],
        "columnLabels": []
    };

}