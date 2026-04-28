function procesarDatosEnergia() {
    window.dataProcessor = {
        // Determina si un color hexadecimal es claro
        isLight(hex) {
            if (!hex) return false;
            const c = hex.replace('#', '');
            const r = parseInt(c.substr(0, 2), 16);
            const g = parseInt(c.substr(2, 2), 16);
            const b = parseInt(c.substr(4, 2), 16);
            // Perceived luminance
            const l = 0.299 * r + 0.587 * g + 0.114 * b;
            return l > 186; // umbral típico
        },

        // Categoría mejorada para filtros más específicos
        getCategory(nodeData, nodeName) {
            if (!nodeData && !nodeName) return 'Otros';

            // Categorías por nombre de nodo (nodos padre principales)
            if (nodeName) {
                // Fuentes de energía primaria
                if (nodeName.includes('Importación EP') || nodeName.includes('Producción') ||
                    nodeName.includes('V.I. y Dif. Est. EP')) {
                    return 'Fuentes Primarias';
                }

                // Fuentes de energía secundaria  
                if (nodeName.includes('Importación ES') || nodeName.includes('V.I. y Dif. Est. ES')) {
                    return 'Fuentes Secundarias';
                }

                // Transformaciones
                if (nodeName.includes('Coquizadoras') || nodeName.includes('Plantas de Gas') ||
                    nodeName.includes('Refinerías') || nodeName.includes('Centrales Eléctricas')) {
                    return 'Transformación';
                }

                // Sectores de consumo
                if (nodeName.includes('Industrial') || nodeName.includes('Transporte') ||
                    nodeName.includes('Agropecuario') || nodeName.includes('Comercial') ||
                    nodeName.includes('Público') || nodeName.includes('Residencial') ||
                    nodeName.includes('Consumo final')) {
                    return 'Sectores de Consumo';
                }

                // Pérdidas y ajustes
                if (nodeName.includes('Pérdidas') || nodeName.includes('Exportación') ||
                    nodeName.includes('Energía No Aprovechada')) {
                    return 'Pérdidas y Exportaciones';
                }
            }

            // Categorías por tipo de energético (nodos hijo)
            if (nodeData && nodeData.tipo) {
                const t = nodeData.tipo.toLowerCase();
                if (t.includes('primaria')) return 'Energéticos Primarios';
                if (t.includes('secundaria')) return 'Energéticos Secundarios';
            }

            return 'Otros';
        },
        // Determina si un nombre corresponde a un energético específico (no contenedor)
        isSpecificEnergetic(nodeName) {
            const containerKeywords = [
                "Importación", "Exportación", "Producción", "Variación", "Inventarios",
                "Diferencia", "Estadística", "Pérdidas", "Oferta", "Bruta", "Consumo",
                "Propio", "Sector", "Centrales", "Eléctricas", "Coquizadoras", "Hornos",
                "Plantas", "Gas", "Fraccionadoras", "Refinerías", "Despuntadoras",
                "Industrial", "Transporte", "Agropecuario", "Comercial", "Público",
                "Residencial", "Petroquímica", "PEMEX", "Total", "V.I.", "Dif.", "Est.",
            ];
            return !containerKeywords.some((k) => nodeName && nodeName.includes(k));
        },

        // Color correcto según configuración
        getNodeColor(nodeName, nodeData, config, parentNode) {
            if (config.colorBy === 'parent' && parentNode && parentNode.itemStyle) {
                return parentNode.itemStyle.color;
            }
            if (config.colorBy === 'category') {
                const cat = this.getCategory(nodeData);
                return (config.categoryColors && config.categoryColors[cat]) || '#888';
            }
            // colorBy child (default)
            if (this.isSpecificEnergetic(nodeName) && config.energeticColors[nodeName]) {
                return config.energeticColors[nodeName];
            }
            if (nodeData && nodeData["Nodo Hijo"] && this.isSpecificEnergetic(nodeData["Nodo Hijo"])) {
                const c = config.energeticColors[nodeData["Nodo Hijo"]];
                if (c) return c;
            }
            if (config.energeticColors[nodeName]) return config.energeticColors[nodeName];
            return "#888";
        },

        processSankeyData(data, year, config) {
            const y = String(year);
            const nodes = new Map();

            // 1) Registrar nodos definidos en la config (soporta espaciadores)
            config.columnas.forEach((col, colIdx) => {
                let nodosCol = [];
                if (Array.isArray(col.nodos) && col.nodos.length) {
                    nodosCol = col.nodos.filter((n) => n.visible !== false);
                } else if (col.mostrar === "Hijo" && col.filtroTipo) {
                    (data.Datos || []).forEach((padre) => {
                        (padre["Nodos Hijo"] || []).forEach((hijo) => {
                            if (hijo.tipo === col.filtroTipo) {
                                nodosCol.push({ nombre: hijo["Nodo Hijo"], tipo: "Hijo", padre: padre["Nodo Padre"] });
                            }
                        });
                    });
                }

                nodosCol.forEach((nodo) => {
                    if (!nodes.has(nodo.nombre)) {
                        const nodeData = this.findNodeData(data, nodo.nombre, nodo.tipo);
                        const nodeConfig = {
                            ...(nodeData || {}),
                            name: nodo.nombre,
                            description: nodeData ? nodeData.descripcion : "",
                            tipo: nodeData ? nodeData.tipo : nodo.tipo,
                            padre: nodo.padre || null,
                            columna: colIdx,
                            depth: nodo.depth !== undefined ? nodo.depth : colIdx,
                            flow: nodo.flow || "default",
                            esEspaciador: Boolean(nodo.esEspaciador),
                            // Asignar ancho personalizado por columna
                            nodeWidth: config.columnWidths && config.columnWidths[colIdx] ?
                                config.columnWidths[colIdx] : config.layoutConfig.nodeWidth,
                        };

                        if (nodeConfig.esEspaciador) {
                            Object.assign(nodeConfig, {
                                itemStyle: { color: "rgba(0,0,0,0)", borderColor: "rgba(0,0,0,0)", borderWidth: 0, opacity: 0 },
                                label: { show: false },
                                silent: true,
                                value: nodo.valorEspaciador || 0.1, // ocupa espacio vertical
                            });
                        } else {
                            const color = this.getNodeColor(nodo.nombre, nodeData, config);
                            const light = this.isLight(color);
                            nodeConfig.category = this.getCategory(nodeData, nodo.nombre);
                            nodeConfig.itemStyle = {
                                color,
                                borderColor: light ? '#333' : '#fff',
                                borderWidth: light ? 1 : 0,
                                // Aplicar ancho personalizado por columna
                                width: config.columnWidths && config.columnWidths[colIdx] ?
                                    config.columnWidths[colIdx] : config.layoutConfig.nodeWidth
                            };
                            nodeConfig.label = { color: '#000' };
                        }

                        nodes.set(nodo.nombre, nodeConfig);
                    }
                });
            });

            // 2) Construir enlaces a partir de los datos
            const allLinks = [];
            (data.Datos || []).forEach((padre) => {
                const padreNode = nodes.get(padre["Nodo Padre"]);
                if (!padreNode) return;

                (padre["Nodos Hijo"] || []).forEach((hijo) => {
                    if (!nodes.has(hijo["Nodo Hijo"])) return;
                    const raw = hijo[y];
                    const value = typeof raw === "number" ? raw : Number(raw);
                    if (!value || Number.isNaN(value) || value === 0) return;

                    let source, target;
                    if (config.flowPolicy === 'fixedParentToChild') {
                        source = padre["Nodo Padre"];
                        target = hijo["Nodo Hijo"];
                    } else if (value < 0 || (padreNode && padreNode.flow === "sink")) {
                        source = hijo["Nodo Hijo"];
                        target = padre["Nodo Padre"];
                    } else {
                        source = padre["Nodo Padre"];
                        target = hijo["Nodo Hijo"];
                    }

                    const childNode = nodes.get(hijo["Nodo Hijo"]);
                    const sourceNode = nodes.get(source);
                    const targetNode = nodes.get(target);

                    // Obtener colores de los nodos origen y destino
                    const sourceColor = sourceNode && sourceNode.itemStyle ? sourceNode.itemStyle.color : this.getNodeColor(source, sourceNode, config, padreNode);
                    const targetColor = targetNode && targetNode.itemStyle ? targetNode.itemStyle.color : this.getNodeColor(target, targetNode, config, padreNode);

                    const link = {
                        source,
                        target,
                        rawValue: value,
                        value: Math.abs(value),
                        sourceCategory: padreNode ? padreNode.category : undefined,
                        targetCategory: childNode ? childNode.category : undefined,
                        lineStyle: {
                            color: 'gradient',
                            opacity: 0.7
                        }
                    };

                    if (config.curvenessAuto) {
                        const sc = nodes.get(source);
                        const tc = nodes.get(target);
                        if (sc && tc) {
                            const diff = Math.abs((sc.columna || 0) - (tc.columna || 0));
                            if (diff > 1) {
                                link.lineStyle.curveness = Math.max(0.1, config.layoutConfig.curveness - 0.15 * (diff - 1));
                            }
                        }
                    }

                    allLinks.push(link);
                });
            });

            // 3) Calcular inflows/outflows y fijar value del nodo = max(entrada, salida)
            const nodeInflow = new Map();
            const nodeOutflow = new Map();

            allLinks.forEach((lk) => {
                nodeInflow.set(lk.target, (nodeInflow.get(lk.target) || 0) + lk.value);
                nodeOutflow.set(lk.source, (nodeOutflow.get(lk.source) || 0) + lk.value);
            });

            Array.from(nodes.values()).forEach((node) => {
                if (node.esEspaciador) return;
                let inflow = nodeInflow.get(node.name) || 0;
                let outflow = nodeOutflow.get(node.name) || 0;

                if (node.flow === "sink") outflow = 0; // un sink no expulsa
                else if (node.flow === "source") inflow = 0; // un source no recibe

                node.value = Math.max(inflow, outflow);
                node.inflow = inflow;
                node.outflow = outflow;
            });

            const visibleLinks = allLinks.filter(lk => lk.value >= (config.linkMinValue || 0));

            return { nodes: Array.from(nodes.values()), links: visibleLinks };
        },

        findNodeData(data, nodeName, nodeType) {
            if (nodeType === "Padre") {
                return (data.Datos || []).find((d) => d["Nodo Padre"] === nodeName) || null;
            }
            for (const padre of data.Datos || []) {
                const match = (padre["Nodos Hijo"] || []).find((h) => h["Nodo Hijo"] === nodeName);
                if (match) return match;
            }
            return null;
        },
    };
}