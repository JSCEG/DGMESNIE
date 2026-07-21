
$(document).ready(function () {
    // Asegúrate de que los mapas estén cargados
    if (mapas && mapas.length > 0) {
        // Accede al primer mapa
        var map = [];
        map = mapas[0];
        function pop_Geotermica_ok_4326_0(feature, layer) {

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;
            function updatePopupContent(){
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Profundidad </th>\
                            <td>' + (feature.properties['profundida'] !== null ? autolinker.link(feature.properties['profundida'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Flujo de calor </th>\
                            <td>' + (feature.properties['flujo calo'] !== null ? autolinker.link(feature.properties['flujo calo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad (Clave)</th>\
                            <td>' + (feature.properties['entidad(c)'] !== null ? autolinker.link(feature.properties['entidad(c)'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Entidad Federativa </th>\
                            <td>' + (feature.properties['entidad'] !== null ? autolinker.link(feature.properties['entidad'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">CVEGEO </th>\
                            <td>' + (feature.properties['cvegeo'] !== null ? autolinker.link(feature.properties['cvegeo'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio (Clave) </th>\
                            <td>' + (feature.properties['municipio('] !== null ? autolinker.link(feature.properties['municipio('].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Municipio </th>\
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

        function style_Geotermica_ok_4326_0_0(feature) {
            if (feature.properties['flujo calo'] >= 3.641980 && feature.properties['flujo calo'] <= 54.000000) {
                return {
                    pane: 'pane_Geotermica_ok_4326_0',
                    radius: 4.0,
                    opacity: 1,
                    color: 'rgba(255,255,255,1.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(226,29,37,1.0)',
                    interactive: true,
                }
            }
            if (feature.properties['flujo calo'] >= 54.000000 && feature.properties['flujo calo'] <= 66.000000) {
                return {
                    pane: 'pane_Geotermica_ok_4326_0',
                    radius: 5.5,
                    opacity: 1,
                    color: 'rgba(255,255,255,1.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(226,29,37,1.0)',
                    interactive: true,
                }
            }
            if (feature.properties['flujo calo'] >= 66.000000 && feature.properties['flujo calo'] <= 76.000000) {
                return {
                    pane: 'pane_Geotermica_ok_4326_0',
                    radius: 9.0,
                    opacity: 1,
                    color: 'rgba(255,255,255,1.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(226,29,37,1.0)',
                    interactive: true,
                }
            }
            if (feature.properties['flujo calo'] >= 76.000000 && feature.properties['flujo calo'] <= 91.993719) {
                return {
                    pane: 'pane_Geotermica_ok_4326_0',
                    radius: 12.5,
                    opacity: 1,
                    color: 'rgba(255,255,255,1.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(226,29,37,1.0)',
                    interactive: true,
                }
            }
            if (feature.properties['flujo calo'] >= 91.993719 && feature.properties['flujo calo'] <= 1263.420000) {
                return {
                    pane: 'pane_Geotermica_ok_4326_0',
                    radius: 16.0,
                    opacity: 1,
                    color: 'rgba(255,255,255,1.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(226,29,37,1.0)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_Geotermica_ok_4326_0');
        map.getPane('pane_Geotermica_ok_4326_0').style.zIndex = 400;
        map.getPane('pane_Geotermica_ok_4326_0').style['mix-blend-mode'] = 'normal';
        var layer_Geotermica_ok_4326_0 = new L.geoJson(json_Geotermica_ok_4326_0, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Geotermica_ok_4326_0',
            layerName: 'layer_Geotermica_ok_4326_0',
            pane: 'pane_Geotermica_ok_4326_0',
            onEachFeature: pop_Geotermica_ok_4326_0,
            pointToLayer: function (feature, latlng) {
                var context = {
                    feature: feature,
                    variables: {}
                };
                return L.circleMarker(latlng, style_Geotermica_ok_4326_0_0(feature));
            },
        });
        bounds_group.addLayer(layer_Geotermica_ok_4326_0);
        map.addLayer(layer_Geotermica_ok_4326_0);

    }
});
