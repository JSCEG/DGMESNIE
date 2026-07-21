class ShapeLoader {
    constructor(map) {
        this.map = map;
        this.currentLayers = new Map();
        this.bounds_group = new L.featureGroup([]);
    }

    loadRegiones() {
        // Primero limpiamos marcadores existentes
        this.limpiarMarcadores();

        // Crear el panel para las regiones si no existe
        if (!this.map.getPane("pane_regiones_poligon_0")) {
            this.map.createPane("pane_regiones_poligon_0");
            this.map.getPane("pane_regiones_poligon_0").style.zIndex = 400;
            this.map.getPane("pane_regiones_poligon_0").style["mix-blend-mode"] = "normal";
        }

        // Crear la capa de regiones
        const layer_regiones_poligon_0 = new L.geoJson(json_regiones_poligon_0, {
            attribution: "",
            interactive: true,
            dataVar: "json_regiones_poligon_0",
            layerName: "layer_regiones_poligon_0",
            pane: "pane_regiones_poligon_0",
            onEachFeature: this.pop_regiones_poligon_0,
            style: this.style_regiones_poligon_0_0
        });

        // Agregar la capa al grupo y al mapa
        this.bounds_group.addLayer(layer_regiones_poligon_0);
        this.map.addLayer(layer_regiones_poligon_0);
        
        // Guardar referencia a la capa actual
        this.currentLayers.set('regiones', layer_regiones_poligon_0);
    }

    limpiarMarcadores() {
        // Remover capas existentes
        this.currentLayers.forEach((layer, key) => {
            this.map.removeLayer(layer);
        });
        this.currentLayers.clear();
    }

    pop_regiones_poligon_0(feature, layer) {
        const popupContent = `
            <table>
                <tr>
                    <th scope="row">Gerencia: </th>
                    <td>${feature.properties["region"] || ""}</td>
                </tr>
            </table>`;
        layer.bindPopup(popupContent, { maxHeight: 400 });
    }

    style_regiones_poligon_0_0(feature) {
        const styles = {
            "Regional Baja California": {
                fillColor: "rgba(104,207,143,0.65)"
            },
            "Regional Baja California Sur": {
                fillColor: "rgba(63,130,255,0.65)"
            },
            "Regional Central": {
                fillColor: "rgba(255,128,0,0.65)"
            },
            "Regional Noreste": {
                fillColor: "rgba(255,166,166,0.65)"
            },
            "Regional Noroeste": {
                fillColor: "rgba(109,183,226,0.65)"
            },
            "Regional Norte": {
                fillColor: "rgba(158,94,218,0.65)"
            },
            "Regional Occidental": {
                fillColor: "rgba(128,64,45,0.65)"
            },
            "Regional Oriental": {
                fillColor: "rgba(219,42,83,0.65)"
            },
            "Regional Peninsular": {
                fillColor: "rgba(255,244,29,0.65)"
            }
        };

        const baseStyle = {
            pane: "pane_regiones_poligon_0",
            opacity: 1,
            color: "rgba(35,35,35,0.0)",
            dashArray: "",
            lineCap: "butt",
            lineJoin: "miter",
            weight: 1.0,
            fill: true,
            fillOpacity: 1,
            interactive: true
        };

        return {
            ...baseStyle,
            ...styles[feature.properties["region"]]
        };
    }
}