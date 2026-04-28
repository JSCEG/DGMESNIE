//1.-Invoca el Mapa mapmun_I1
//var coloresImJson = @Html.Raw(ViewBag.JsonObjects["ColoresImun"]);
//console.log("desde IM colores->:", coloresImJson);
var mapmun_I1 = L.map('mapmun_I1', {
    zoomControl: true, maxZoom: 28, minZoom: 5
}).fitBounds([[16.515297504744552, -116.01198143543994], [31.280203931152798, -90.79533052556764]]);
var hash = new L.Hash(mapmun_I1);
mapmun_I1.attributionControl.setPrefix('<a href="https://github.com/tomchadwin/qgis2web" target="_blank">qgis2web</a> &middot; <a href="https://leafletjs.com" title="A JS library for interactive maps">Leaflet</a> &middot; <a href="https://qgis.org">QGIS</a>');
var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
var bounds_group = new L.featureGroup([]);
function setBounds() {
}


//2.-Vista Satelite mapmun_I1
mapmun_I1.createPane('pane_GoogleSatellite_0');
mapmun_I1.getPane('pane_GoogleSatellite_0').style.zIndex = 0;
var layer_GoogleSatellite_0 = L.tileLayer('https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}', {
    pane: 'pane_GoogleSatellite_0',
    opacity: 1.0,
    attribution: '<a href="https://www.google.at/permissions/geoguidelines/attr-guide.html">Map data �2015 Google</a>',
    minZoom: 1,
    maxZoom: 28,
    minNativeZoom: 0,
    maxNativeZoom: 20
});
layer_GoogleSatellite_0;
mapmun_I1.addLayer(layer_GoogleSatellite_0);


//3.-Configura los Base Layers mapmun_I1
var baseLayers = {
    "OpenStreetMap": L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }),
    "Blanco y Negro": L.tileLayer('https://stamen-tiles-{s}.a.ssl.fastly.net/toner/{z}/{x}/{y}{r}.png', {
        attribution: 'Map tiles by <a href="http://stamen.com">Stamen Design</a>, ' +
            '<a href="http://creativecommons.org/licenses/by/3.0">CC BY 3.0</a> &mdash; ' +
            'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
        subdomains: 'abcd',
        minZoom: 0,
        maxZoom: 20
    }),
    "Escala de Grises": L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
        attribution: 'Map tiles by <a href="https://carto.com/attribution">Carto</a>, ' +
            'under <a href="https://creativecommons.org/licenses/by/3.0/">CC BY 3.0</a>. ' +
            'Data by <a href="http://openstreetmap.org">OpenStreetMap</a>, ' +
            'under <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC BY SA</a>',
        subdomains: 'abcd',
        maxZoom: 19
    }),

    "Hipsométrico": L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
        maxZoom: 17,
        attribution: 'Map data: &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)'
    }),

    "Oscuro": L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}{r}.png', {
        maxZoom: 20,
        attribution: '&copy; <a href="https://stadiamaps.com/">Stadia Maps</a>, &copy; <a href="https://openmaptiles.org/">OpenMapTiles</a> &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors'
    }),
    "Vista Satélite": layer_GoogleSatellite_0


};



//Info del Estado Shape EF
var info = L.control();

info.onAdd = function (map) {
    this._div = L.DomUtil.create('div', 'info');
    this.update();
    return this._div;
};

info.update = function (props) {
    this._div.innerHTML = (props ?
        '<b>' + props.NOMGEO + '</b><br />' + getStatusLabel(props.a2023) :
        '¡Selecciona un Estado!');
};

function getStatusLabel(value) {
    if (value === -1) {
        return 'Saturado';
    } else if (value === 0) {
        return 'Equilibrado';
    } else if (value === 1) {
        return 'Oportunidad';
    } else {
        return '';
    }
};

info.addTo(mapmun_I1);

//4.- Activa una capa de mosaicos como vista inicial mapmun_I1
baseLayers["OpenStreetMap"].addTo(mapmun_I1);

//5.- Control de capas para seleccionar la vista del mapa mapmun_I1
L.control.layers(baseLayers).addTo(mapmun_I1);
L.control.scale().addTo(mapmun_I1); // Agregar la escala gr�fica al mapa

//6.-Botón de Zoom mapmun_I1
var resetZoomControl = L.control({ position: 'topleft' });

resetZoomControl.onAdd = function (mapmun_I1) {
    var div = L.DomUtil.create('div', 'reset-zoom-control');
    div.innerHTML = '<button class="btn btn-cre-rojo-home" onclick="resetZoom()"><i class="bi bi-house-door"></i></button>';
    return div;
};

resetZoomControl.addTo(mapmun_I1);

function resetZoom() {
    mapmun_I1.setView([24.572503, -101.768257], 5);
}


//7.-Regla de Distancias mapmun_I1
var drawControl = new L.Control.Draw({
    draw: {
        polygon: false,
        polyline: true,
        rectangle: false,
        circle: false,
        marker: false,
        circlemarker: false
    },
    edit: false
});

mapmun_I1.addControl(drawControl);

//8.-Cuando se dibuja una linea, calcula la distancia mapmun_I1
mapmun_I1.on('draw:created', function (e) {
    var type = e.layerType,
        layer = e.layer;

    if (type === 'polyline') {
        var latlngs = layer.getLatLngs();
        var distance = 0;
        for (var i = 1; i < latlngs.length; i++) {
            distance += latlngs[i - 1].distanceTo(latlngs[i]);
        }
        // Convertir la distancia a km y redondear a 2 decimales
        distance = Math.round((distance / 1000) * 100) / 100;
        // Crear un popup con la distancia
        layer.bindPopup('Distancia: ' + distance + ' km').openPopup();
        // A�adir la l�nea al mapa
        layer.addTo(mapmun_I1);
    }
});


//9.-Configuración de la Capa de Entidad Federativa

//Shape de EF colores y capas

//function getColor(d) {
//    return d == 1 ? '#008000' :
//           d == 0 ? '#FFFF00' :
//           d == -1 ? '#FF0000' :
//           d == 2 ? '#34485C' :
//                   '#FFEDA0';
//}

//function style(feature) {
//    return {
//        fillColor: getColor(I1_E),
//        weight: 1,
//        opacity: 1,
//        color: 'white',
//        dashArray: '3',
//        fillOpacity: 0.01
//    };
//}


//function highlightFeature(e) {
//    var layer = e.target;

//    layer.setStyle({
//        weight: 5,
//        color: '#666',
//        dashArray: '',
//        fillOpacity: 0.5
//    });

//    layer.bringToFront();
//    info.update(layer.feature.properties);
//}

//function resetHighlight(e) {
//    geojson.resetStyle(e.target);
//    info.update();
//}

//function zoomToFeature(e) {
//    mapmun_I1.fitBounds(e.target.getBounds());
//}

//function onEachFeature(feature, layer) {
//    layer.on({
//        mouseover: highlightFeature,
//        mouseout: resetHighlight,
//        click: zoomToFeature
//    });
//}

//var geojson = L.geoJson(estados, {
//    style: style,
//    onEachFeature: onEachFeature
//}).addTo(mapmun_I1);



//var geojson = L.geoJson(municipios_mapa, {
//    style: style,
//    onEachFeature: onEachFeature
//}).addTo(mapmun_I1);





// Cargar el shape de estados y aplicar el estilo
//var geojson = L.geoJson(estados, {
//    style: function (feature) {
//        var cve_ent = feature.properties.CVE_ENT;
//        var indicador2 = coloresImJson.ICM.find(function (indicador) {
//            return indicador.cve_ent === cve_ent;
//        });

//        var I1_E = indicador2 ? indicador2.I1_E : 0;

//        return {
//            fillColor: getColor(I1_E),
//            weight: 1,
//            opacity: 1,
//            color: 'white',
//            dashArray: '3',
//            fillOpacity: 0.7
//        };
//    },
//    onEachFeature: onEachFeature
//}).addTo(mapmun_I1);


function getColor(d) {
    return d == 1 ? '#008000' :
        d == 0 ? '#FFFF00' :
            d == -1 ? '#FF0000' :
                '#FFEDA0';
}

function style(feature) {
    return {
        fillColor: getColor(feature.properties.a2015),
        weight: 1,
        opacity: 1,
        color: 'white',
        dashArray: '3',
        fillOpacity: 0.3
    };
}

function highlightFeature(e) {
    var layer = e.target;

    layer.setStyle({
        weight: 5,
        color: '#666',
        dashArray: '',
        fillOpacity: 0.7
    });

    layer.bringToFront();
    info.update(layer.feature.properties);
}

function resetHighlight(e) {
    geojson.resetStyle(e.target);
    info.update();
}

function zoomToFeature(e) {
    var estadoSeleccionado = e.target.feature.properties.NOMGEO;
    var estadoStatus = getStatusLabel(e.target.feature.properties.a2015);

    mapmun_I1.fitBounds(e.target.getBounds());

    var municipiosSeleccionados = municipios_mapa.features.filter(function (feature) {
        return feature.properties.NOMGEO === estadoSeleccionado;
    });

    municipiosLayer.clearLayers();
    municipiosLayer.addData(municipiosSeleccionados);

    info.update({
        estado: {
            nombre: estadoSeleccionado,
            estado: estadoStatus
        },
        municipio: {
            nombre: 'Selecciona un municipio',
            estado: ''
        }
    });
}

function onEachFeature(feature, layer) {
    layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
        click: zoomToFeature
    });
}

var info = L.control();

info.onAdd = function (map) {
    this._div = L.DomUtil.create('div', 'info');
    this.update();
    return this._div;
};

info.update = function (props) {
    var estadoLabel = 'Selecciona un estado';
    var estadoStatusLabel = '';
    var municipioLabel = 'Selecciona un municipio';
    var municipioStatusLabel = '';

    if (props) {
        if (props.estado && props.estado.nombre) {
            estadoLabel = props.estado.nombre;
            estadoStatusLabel = getStatusLabel(props.estado.estado);
        }
        if (props.municipio && props.municipio.nombre) {
            municipioLabel = props.municipio.nombre;
            municipioStatusLabel = getStatusLabel(props.municipio.estado);
        }
    }

    this._div.innerHTML = '<h4>Estado: ' + estadoLabel + '</h4>' +
        '<b>' + estadoStatusLabel + '</b><br/>' +
        '<h4>Municipio: ' + municipioLabel + '</h4>' +
        '<b>' + municipioStatusLabel + '</b>';
};

function getStatusLabel(value) {
    if (value === -1) {
        return 'Saturado';
    } else if (value === 0) {
        return 'Equilibrado';
    } else if (value === 1) {
        return 'Oportunidad';
    } else {
        return '';
    }
}

info.addTo(mapmun_I1);

var legend = L.control({ position: 'bottomright' });

legend.onAdd = function (map) {
    var div = L.DomUtil.create('div', 'info legend');
    var grades = [-1, 0, 1];
    var labels = ['Saturado', 'Equilibrado', 'Oportunidad'];

    for (var i = 0; i < grades.length; i++) {
        div.innerHTML +=
            '<div><span>' + labels[i] + '</span><i style="background:' + getColor(grades[i]) + '"></i></div>';
    }

    return div;
};

legend.addTo(mapmun_I1);



var geojson = L.geoJson(estados, {
    style: style,
    onEachFeature: onEachFeature
}).addTo(map);
var municipiosLayer = L.geoJson(null, {
    style: style,
    onEachFeature: onEachMunicipio
}).addTo(map);
function onEachMunicipio(feature, layer) {
    layer.setStyle({
        fillColor: getColor(feature.properties.a2015m),
        weight: 1,
        opacity: 1,
        color: 'white',
        dashArray: '3',
        fillOpacity: 0.3
    });

    layer.on({
        mouseover: highlightMunicipio,
        mouseout: resetHighlightMunicipio
    });
}

function highlightMunicipio(e) {
    var layer = e.target;

    layer.setStyle({
        weight: 5,
        color: '#666',
        dashArray: '',
        fillOpacity: 0.7
    });

    layer.bringToFront();
    info.update({
        estado: {
            nombre: mapmun_I1.getZoom() <= 5 ? 'Selecciona un estado' : '',
            estado: ''
        },
        municipio: {
            nombre: layer.feature.properties.NOM_MUN,
            estado: getStatusLabel(layer.feature.properties.a2015m)
        }
    });
}

function resetHighlightMunicipio(e) {
    municipiosLayer.resetStyle(e.target);
    info.update({
        estado: {
            nombre: mapmun_I1.getZoom() <= 5 ? 'Selecciona un estado' : '',
            estado: ''
        },
        municipio: {
            nombre: 'Selecciona un municipio',
            estado: ''
        }
    });
}

var resetZoomControl = L.control({ position: 'topleft' });

resetZoomControl.onAdd = function (map) {
    var div = L.DomUtil.create('div', 'reset-zoom-control');
    div.innerHTML = '<button onclick="resetZoom()">Reestablecer</button>';
    return div;
};

resetZoomControl.addTo(mapmun_I1);

function resetZoom() {
    mapmun_I1.setView([24.572503, -101.768257], 5);
    municipiosLayer.clearLayers();
}