
//Capas del JS
var highlightLayer;
function highlightFeature(e) {
    highlightLayer = e.target;

    if (e.target.feature.geometry.type === 'LineString') {
        highlightLayer.setStyle({
            color: '#ffff00',
        });
    } else {
        highlightLayer.setStyle({
            fillColor: '#ffff00',
            fillOpacity: 1
        });
    }
}






//Template del Mapa*@


///Inicializa el mapa
var map = mapas[0];



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




//Funciones que cargan los poligonos al Mapa*@


function pop_Residuosindustriales_0(feature, layer) {
layer.on({
    mouseout: function (e) {
        for (i in e.target._eventParents) {
            e.target._eventParents[i].resetStyle(e.target);
        }
    },
    // mouseover: highlightFeature,
});

// Variables para almacenar las coordenadas de clic
let clickedLat, clickedLon;
function updatePopupContent(){
    var popupContent = '<table>\
                        <tr>\
                            <th scope="row">ID</th>\
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Localidad: </th>\
                            <td>' + (feature.properties['localidad'] !== null ? autolinker.link(feature.properties['localidad'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre: </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">TJA: </th>\
                            <td>' + (feature.properties['tja'] !== null ? autolinker.link(feature.properties['tja'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad (Clave): </th>\
                            <td>' + (feature.properties['entidad(c)'] !== null ? autolinker.link(feature.properties['entidad(c)'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad: </th>\
                            <td>' + (feature.properties['entidad'] !== null ? autolinker.link(feature.properties['entidad'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">CVEGEO: </th>\
                            <td>' + (feature.properties['cvegeo'] !== null ? autolinker.link(feature.properties['cvegeo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio (clave): </th>\
                            <td>' + (feature.properties['municipio('] !== null ? autolinker.link(feature.properties['municipio('].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio: </th>\
                            <td>' + (feature.properties['municipio'] !== null ? autolinker.link(feature.properties['municipio'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <td><a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + clickedLat + ',' + clickedLon + '&cbp=11,0,0,0,0" target="_blank"><b> Ver vista de calle </b></a></td>\
                        </tr>\
                    </table>';
    layer.bindPopup(popupContent, { maxHeight: 400 }).openPopup();
}
// Evento para capturar clics en el mapa dentro del área del feature
layer.on('click', function (e) {
    clickedLat = e.latlng.lat.toPrecision(8);
    clickedLon = e.latlng.lng.toPrecision(8);
    updatePopupContent();
});
}

function style_Residuosindustriales_0_0(feature) {
if (feature.properties['tja'] >= 4.166768 && feature.properties['tja'] <= 6.119923) {
    return {
        pane: 'pane_Residuosindustriales_0',
        radius: 4.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(128,128,128,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 6.119923 && feature.properties['tja'] <= 9.255001) {
    return {
        pane: 'pane_Residuosindustriales_0',
        radius: 7.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(128,128,128,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 9.255001 && feature.properties['tja'] <= 16.025057) {
    return {
        pane: 'pane_Residuosindustriales_0',
        radius: 10.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(128,128,128,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 16.025057 && feature.properties['tja'] <= 39.246456) {
    return {
        pane: 'pane_Residuosindustriales_0',
        radius: 13.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(128,128,128,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 39.246456 && feature.properties['tja'] <= 2173.533462) {
    return {
        pane: 'pane_Residuosindustriales_0',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(128,128,128,1.0)',
        interactive: true,
    }
}
}
map.createPane('pane_Residuosindustriales_0');
map.getPane('pane_Residuosindustriales_0').style.zIndex = 400;
map.getPane('pane_Residuosindustriales_0').style['mix-blend-mode'] = 'normal';
var layer_Residuosindustriales_0 = new L.geoJson(json_Residuosindustriales_0, {
attribution: '',
interactive: true,
dataVar: 'json_Residuosindustriales_0',
layerName: 'layer_Residuosindustriales_0',
pane: 'pane_Residuosindustriales_0',
onEachFeature: pop_Residuosindustriales_0,
pointToLayer: function (feature, latlng) {
    var context = {
        feature: feature,
        variables: {}
    };
    return L.circleMarker(latlng, style_Residuosindustriales_0_0(feature));
},
});
bounds_group.addLayer(layer_Residuosindustriales_0);
map.addLayer(layer_Residuosindustriales_0);
function pop_Residuosganados_1(feature, layer) {
layer.on({
    mouseout: function (e) {
        for (i in e.target._eventParents) {
            e.target._eventParents[i].resetStyle(e.target);
        }
    },
    // mouseover: highlightFeature,
});
// Variables para almacenar las coordenadas de clic
let clickedLat, clickedLon;
function updatePopupContent(){
    var popupContent = '<table>\
                        <tr>\
                            <th scope="row">ID: </th>\
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">TJA: </th>\
                            <td>' + (feature.properties['tja'] !== null ? autolinker.link(feature.properties['tja'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad(clave): </th>\
                            <td>' + (feature.properties['entidad(c)'] !== null ? autolinker.link(feature.properties['entidad(c)'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad: </th>\
                            <td>' + (feature.properties['entidad'] !== null ? autolinker.link(feature.properties['entidad'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">CVGEO: </th>\
                            <td>' + (feature.properties['cvegeo'] !== null ? autolinker.link(feature.properties['cvegeo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio (clave): </th>\
                            <td>' + (feature.properties['municipio('] !== null ? autolinker.link(feature.properties['municipio('].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio: </th>\
                            <td>' + (feature.properties['municipio'] !== null ? autolinker.link(feature.properties['municipio'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <td><a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + clickedLat + ',' + clickedLon + '&cbp=11,0,0,0,0" target="_blank"><b> Ver vista de calle </b></a></td>\
                        </tr>\
                    </table>';
    layer.bindPopup(popupContent, { maxHeight: 400 }).openPopup();
}
// Evento para capturar clics en el mapa dentro del área del feature
layer.on('click', function (e) {
    clickedLat = e.latlng.lat.toPrecision(8);
    clickedLon = e.latlng.lng.toPrecision(8);
    updatePopupContent();
});
}

function style_Residuosganados_1_0(feature) {
if (feature.properties['tja'] >= 4.201096 && feature.properties['tja'] <= 7.914040) {
    return {
        pane: 'pane_Residuosganados_1',
        radius: 4.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,153,41,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 7.914040 && feature.properties['tja'] <= 11.283308) {
    return {
        pane: 'pane_Residuosganados_1',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,153,41,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 11.283308 && feature.properties['tja'] <= 16.271722) {
    return {
        pane: 'pane_Residuosganados_1',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,153,41,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 16.271722 && feature.properties['tja'] <= 28.652460) {
    return {
        pane: 'pane_Residuosganados_1',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,153,41,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 28.652460 && feature.properties['tja'] <= 428.914932) {
    return {
        pane: 'pane_Residuosganados_1',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(255,255,255,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,153,41,1.0)',
        interactive: true,
    }
}
}
map.createPane('pane_Residuosganados_1');
map.getPane('pane_Residuosganados_1').style.zIndex = 401;
map.getPane('pane_Residuosganados_1').style['mix-blend-mode'] = 'normal';
var layer_Residuosganados_1 = new L.geoJson(json_Residuosganados_1, {
attribution: '',
interactive: true,
dataVar: 'json_Residuosganados_1',
layerName: 'layer_Residuosganados_1',
pane: 'pane_Residuosganados_1',
onEachFeature: pop_Residuosganados_1,
pointToLayer: function (feature, latlng) {
    var context = {
        feature: feature,
        variables: {}
    };
    return L.circleMarker(latlng, style_Residuosganados_1_0(feature));
},
});
bounds_group.addLayer(layer_Residuosganados_1);
map.addLayer(layer_Residuosganados_1);
function pop_Residuosdebosques_2(feature, layer) {
layer.on({
    mouseout: function (e) {
        for (i in e.target._eventParents) {
            e.target._eventParents[i].resetStyle(e.target);
        }
    },
    //mouseover: highlightFeature,
});
// Variables para almacenar las coordenadas de clic
let clickedLat, clickedLon;
function updatePopupContent(){
    var popupContent = '<table>\
                        <tr>\
                            <th scope="row">ID: </th>\
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">TJA: </th>\
                            <td>' + (feature.properties['tja'] !== null ? autolinker.link(feature.properties['tja'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Tipo: </th>\
                            <td>' + (feature.properties['tipo'] !== null ? autolinker.link(feature.properties['tipo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad (clave): </th>\
                            <td>' + (feature.properties['entidad(c)'] !== null ? autolinker.link(feature.properties['entidad(c)'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad: </th>\
                            <td>' + (feature.properties['entidad'] !== null ? autolinker.link(feature.properties['entidad'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">CVEGEO: </th>\
                            <td>' + (feature.properties['cvegeo'] !== null ? autolinker.link(feature.properties['cvegeo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio (clave): </th>\
                            <td>' + (feature.properties['municipio('] !== null ? autolinker.link(feature.properties['municipio('].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio: </th>\
                            <td>' + (feature.properties['municipio'] !== null ? autolinker.link(feature.properties['municipio'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <td><a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + clickedLat + ',' + clickedLon + '&cbp=11,0,0,0,0" target="_blank"><b> Ver vista de calle </b></a></td>\
                        </tr>\
                    </table>';
    layer.bindPopup(popupContent, { maxHeight: 400 }).openPopup();
}
// Evento para capturar clics en el mapa dentro del área del feature
layer.on('click', function (e) {
    clickedLat = e.latlng.lat.toPrecision(8);
    clickedLon = e.latlng.lng.toPrecision(8);
    updatePopupContent();
});
}

function style_Residuosdebosques_2_0(feature) {
if (feature.properties['tja'] >= 0.193142 && feature.properties['tja'] <= 1336.192276) {
    return {
        pane: 'pane_Residuosdebosques_2',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,255,255,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 1336.192276 && feature.properties['tja'] <= 2672.191410) {
    return {
        pane: 'pane_Residuosdebosques_2',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,191,191,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 2672.191410 && feature.properties['tja'] <= 4008.190544) {
    return {
        pane: 'pane_Residuosdebosques_2',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,128,128,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 4008.190544 && feature.properties['tja'] <= 5344.189678) {
    return {
        pane: 'pane_Residuosdebosques_2',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,64,64,1.0)',
        interactive: true,
    }
}
if (feature.properties['tja'] >= 5344.189678 && feature.properties['tja'] <= 6680.188812) {
    return {
        pane: 'pane_Residuosdebosques_2',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,0,0,1.0)',
        interactive: true,
    }
}
}
map.createPane('pane_Residuosdebosques_2');
map.getPane('pane_Residuosdebosques_2').style.zIndex = 402;
map.getPane('pane_Residuosdebosques_2').style['mix-blend-mode'] = 'normal';
var layer_Residuosdebosques_2 = new L.geoJson(json_Residuosdebosques_2, {
attribution: '',
interactive: true,
dataVar: 'json_Residuosdebosques_2',
layerName: 'layer_Residuosdebosques_2',
pane: 'pane_Residuosdebosques_2',
onEachFeature: pop_Residuosdebosques_2,
style: style_Residuosdebosques_2_0,
});
bounds_group.addLayer(layer_Residuosdebosques_2);
map.addLayer(layer_Residuosdebosques_2);
var baseMaps = {};
L.control.layers(baseMaps, { 'Residuos de bosques<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosdebosques_2_013360.png" /></td><td>0 - 1336</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosdebosques_2_133626721.png" /></td><td>1336 - 2672</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosdebosques_2_267240082.png" /></td><td>2672 - 4008</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosdebosques_2_400853443.png" /></td><td>4008 - 5344</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosdebosques_2_534466804.png" /></td><td>5344 - 6680</td></tr></table>': layer_Residuosdebosques_2, 'Residuos ganados<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosganados_1_42790.png" /></td><td>4.2 - 7.9</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosganados_1_791131.png" /></td><td>7.9 - 11.3</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosganados_1_1131632.png" /></td><td>11.3 - 16.3</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosganados_1_1632873.png" /></td><td>16.3 - 28.7</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosganados_1_28742894.png" /></td><td>28.7 - 428.9</td></tr></table>': layer_Residuosganados_1, 'Residuos industriales<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosindustriales_0_42610.png" /></td><td>4.2 - 6.1</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosindustriales_0_61931.png" /></td><td>6.1 - 9.3</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosindustriales_0_93162.png" /></td><td>9.3 - 16</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosindustriales_0_163923.png" /></td><td>16 - 39.2</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Residuosindustriales_0_392217354.png" /></td><td>39.2 - 2173.5</td></tr></table>': layer_Residuosindustriales_0, }).addTo(map);
setBounds();
