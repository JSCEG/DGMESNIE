
///Inicializa el mapa
var map = mapas[0];



map.on('click', function (e) {
    let lat = e.latlng.lat.toPrecision(8);
    let lon = e.latlng.lng.toPrecision(8);
    const point = L.marker([lat, lon]).addTo(map)
        .bindPopup('<a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + lat + ',' + lon + '&cbp=11,0,0,0,0" target="blank"><b> Ver vista de calle </b></a>').openPopup();
});

// Cuando se dibuja una línea, calcula la distancia
map.on('draw:created', function (e) {
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
        // Añadir la línea al mapa
        layer.addTo(map);
    }
});




//Capa de viento*@

var urlRaster = '/Geovisualizador/rasters/viento.png';
var esquinaSupIzq = L.latLng(32.734349540232256, -118.3537828525035);
var esquinaInfDer = L.latLng(14.533412477527252, -86.74104147980864);
var bounds = L.latLngBounds(esquinaSupIzq, esquinaInfDer);
var capaRaster = L.imageOverlay(urlRaster, bounds).addTo(map);




