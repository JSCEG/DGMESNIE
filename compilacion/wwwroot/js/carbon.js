
$(document).ready(function () {
    // Asegúrate de que los mapas estén cargados
    if (mapas && mapas.length > 0) {
        // Accede al primer mapa
        var map = mapas[0];
        
        function pop_carbon_0(feature, layer) {
            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">Ubicación:</th>\
                            <td>' + (feature.properties['Ubicación'] !== null ? autolinker.link(feature.properties['Ubicación'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Reservas (M de t): </th>\
                            <td>' + (feature.properties['Reservas'] !== null ? autolinker.link(feature.properties['Reservas'].toLocaleString()) : '') + '</td>\
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

        function style_carbon_0_0() {
            return {
                pane: 'pane_carbon_0',
                radius: 24.0,
                opacity: 1,
                color: 'rgba(0,0,0,0.5)',
                dashArray: '',
                lineCap: 'butt',
                lineJoin: 'miter',
                weight: 2.0,
                fill: true,
                fillOpacity: 1,
                fillColor: 'rgba(120,120,120,0.5)',
                interactive: true,
            }
        }

        map.createPane('pane_carbon_0');
        map.getPane('pane_carbon_0').style.zIndex = 400;
        map.getPane('pane_carbon_0').style['mix-blend-mode'] = 'normal';

        var layer_carbon_0 = new L.geoJson(json_carbon_0, {
            attribution: '',
            interactive: true,
            dataVar: 'json_carbon_0',
            layerName: 'layer_carbon_0',
            pane: 'pane_carbon_0',
            onEachFeature: pop_carbon_0,
            pointToLayer: function (feature, latlng) {
                return L.circleMarker(latlng, style_carbon_0_0(feature));
            },
        });

        bounds_group.addLayer(layer_carbon_0);
        map.addLayer(layer_carbon_0);
    }
});
