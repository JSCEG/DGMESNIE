
// Función para obtener el estilo de cada municipio basado en su DIVISIÓN
function getDivisionStyle(feature) {
    var division = feature.properties.DIVISIÓN;
    var colors = {
        "Baja California": "#FF0000",
        "Baja California Sur": "#FF8C00",
        "Bajío": "#FFD700",
        "Centro Occidente": "#ADFF2F",
        "Centro Oriente": "#32CD32",
        "Centro Sur": "#00FA9A",
        "Golfo Centro": "#00CED1",
        "Golfo Norte": "#1E90FF",
        "Jalisco": "#0000FF",
        "Noroeste": "#8A2BE2",
        "Norte": "#9400D3",
        "Oriente": "#FF1493",
        "Peninsular": "#C71585",
        "Sureste": "#FF4500",
        "Valle de México Centro": "#2F4F4F",
        "Valle de México Norte": "#708090",
        "Valle de México Sur": "#8B0000"
    };
    return {
        color: colors[division] || "#808080",
        weight: 1,
        fillOpacity: 0.5
    };
}

// Función para resaltar un municipio al pasar el mouse
function highlightFeature(e) {
    var layer = e.target;
    layer.setStyle({
        weight: 3,
        color: '#666',
        fillOpacity: 0.7
    });
    var props = layer.feature.properties;
    layer.bindTooltip("Estado: " + props.nom_ent + "<br>Municipio: " + props.nom_mun, {sticky: true}).openTooltip();
}

// Función para quitar el resaltado y el tooltip al salir del municipio
function resetHighlight(e) {
    municipiosLayer.resetStyle(e.target);
    e.target.closeTooltip();
}

// Configuración del evento de clic en cada municipio
function onEachFeature(feature, layer) {
    // Verificar las propiedades disponibles
    console.log("Propiedades del municipio:", feature.properties);

    layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
        click: function(e) {
            var cvegeo = feature.properties.cvegeo; // Asegúrate de usar 'cvegeo' si así está definido
            console.log("Clic en municipio con CVEGEO:", cvegeo);
            if (cvegeo) {
                showLocalidades(cvegeo);
            } else {
                console.error("CVEGEO no definido para este municipio.");
            }
            map.fitBounds(layer.getBounds());
        }
    });
}

// Crear capa de municipios y agregar al mapa
var municipiosLayer = L.geoJson(municipios_division, {
    style: getDivisionStyle,
    onEachFeature: onEachFeature
}).addTo(map);

// Variable para almacenar la capa de localidades filtradas
var localidadesLayer;

// Función para mostrar las localidades que pertenecen al municipio seleccionado
function showLocalidades(cvegeo) {
    // Remover la capa de localidades anteriores si existen
    if (localidadesLayer) {
        map.removeLayer(localidadesLayer);
    }

    // Filtrar las localidades que tienen el mismo valor en 'union' que el CVEGEO del municipio
    localidadesLayer = L.geoJson(localidades_division, {
        filter: function (feature) {
            console.log("Comparando union:", feature.properties.union, "con CVEGEO:", cvegeo);
            return feature.properties.union === cvegeo;
        },
        style: {
            color: '#0000FF',
            weight: 1,
            fillOpacity: 0.5
        },
        onEachFeature: function (feature, layer) {
            layer.bindTooltip(feature.properties.NOMGEO || "Sin Nombre");
        }
    });

    // Agregar las localidades filtradas al mapa
    if (localidadesLayer.getLayers().length === 0) {
        console.warn("No se encontraron localidades para el CVEGEO:", cvegeo);
    } else {
        console.log("Localidades encontradas:", localidadesLayer.getLayers().length);
        localidadesLayer.addTo(map);
    }
}
