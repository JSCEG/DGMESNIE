//1.-Invoca el Mapa

var mapmun = L.map('mapmun', {
    zoomControl: true, maxZoom: 28, minZoom: 5
}).fitBounds([[16.515297504744552, -116.01198143543994], [31.280203931152798, -90.79533052556764]]);
var hash = new L.Hash(mapmun);
mapmun.attributionControl.setPrefix('<a href="https://github.com/tomchadwin/qgis2web" target="_blank">qgis2web</a> &middot; <a href="https://leafletjs.com" title="A JS library for interactive maps">Leaflet</a> &middot; <a href="https://qgis.org">QGIS</a>');
var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
var bounds_group = new L.featureGroup([]);
function setBounds() {
}


//2.-Crea los Iconos
var iconoBase = L.Icon.extend({
    options: {
        iconSize: [24, 24],
        iconAnchor: [12, 16],
        popupAnchor: [-3, -76]
    }
});

//3.-Asigna Iconos
var iconoOptimas = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Aprobado.png' }),
    iconoViable = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/viable.png' }),
    iconoFactible = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Solicitudes.png' });



//4.-Vista Satelite
mapmun.createPane('pane_GoogleSatellite_0');
mapmun.getPane('pane_GoogleSatellite_0').style.zIndex = 0;
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
mapmun.addLayer(layer_GoogleSatellite_0);


//5.-Configura los Base Layers 
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

// Activa una capa de mosaicos como vista inicial
baseLayers["OpenStreetMap"].addTo(mapmun);

// Control de capas para seleccionar la vista del mapa
L.control.layers(baseLayers).addTo(mapmun);
L.control.scale().addTo(mapmun); // Agregar la escala gr�fica al mapa

//6.-Botón de Zoom
var resetZoomControl = L.control({ position: 'topleft' });

resetZoomControl.onAdd = function (mapmun) {
    var div = L.DomUtil.create('div', 'reset-zoom-control');
    div.innerHTML = '<button class="btn btn-cre-rojo-home" onclick="resetZoom()"><i class="bi bi-house-door"></i></button>';
    return div;
};

resetZoomControl.addTo(mapmun);

function resetZoom() {
    mapmun.setView([24.572503, -101.768257], 5);
}


//7.-Regla de Distancias
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

mapmun.addControl(drawControl);

//8.-Cuando se dibuja una linea, calcula la distancia
mapmun.on('draw:created', function (e) {
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
        layer.addTo(mapmun);
    }
});




//9.-Configuración de la Capa de Entidad Federativa

///Mapa estados
//function getColor(d) {
//    return d == 1 ? '#008000' :
//        d == 0 ? '#FFFF00' :
//            d == -1 ? '#FF0000' :
//                '#FFEDA0';
//}

//function style(feature) {
//    return {
//        //  fillColor: getColor(feature.properties.a2023),
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
//    mapmun.fitBounds(e.target.getBounds());
//}

//function onEachFeature(feature, layer) {
//    layer.on({
//        mouseover: highlightFeature,
//        mouseout: resetHighlight,
//       // click: zoomToFeature
//    });
//}


//var geojson = L.geoJson(estados, {
//    style: style,
//    onEachFeature: onEachFeature
//}).addTo(mapmun);

//Info del Estado Shape EF
//var infomun = L.control();

//infomun.onAdd = function (mapmun) {
//    this._div = L.DomUtil.create('div', 'info');
//    this.update();
//    return this._div;
//};

//infomun.update = function (props) {
//    this._div.innerHTML = (props ?
//        '<b>' + feature.properties.NOMGEO + '</b><br />' + getStatusLabel(props.a2023) :
//        '¡Selecciona un Estado!');
//};

//function getStatusLabel(value) {
//    if (value === -1) {
//        return 'Saturado';
//    } else if (value === 0) {
//        return 'Equilibrado';
//    } else if (value === 1) {
//        return 'Oportunidad';
//    } else {
//        return '';
//    }
//};

//infomun.addTo(mapmun);

