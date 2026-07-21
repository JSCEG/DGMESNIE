
$(document).ready(function () {
    // Asegúrate de que los mapas estén cargados
    if (mapas && mapas.length > 0) {
        // Accede al primer mapa
        var map = [];
        map = mapas[0];
        function pop_Uranio_0(feature, layer) {
            var popupContent = '<table>\
                    <tr>\
                        <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                    </tr>\
                    <tr>\
                        <td colspan="2">' + (feature.properties['concentra'] !== null ? autolinker.link(feature.properties['concentra'].toLocaleString()) : '') + '</td>\
                    </tr>\
                </table>';
            layer.bindPopup(popupContent, { maxHeight: 400 });
        }

        map.on('click', function (e) {
            let lat = e.latlng.lat.toPrecision(8);
            let lon = e.latlng.lng.toPrecision(8);
            const point = L.marker([lat, lon]).addTo(map)
                .bindPopup('<a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + lat + ',' + lon + '&cbp=11,0,0,0,0" target="blank"><b> Ver vista de calle </b></a>').openPopup();
        });

        function style_Uranio_0_0(feature) {
            switch (String(feature.properties['concentra'])) {
                case 'Bajo':
                    return {
                        pane: 'pane_Uranio_0',
                        radius: 5.0,
                        opacity: 1,
                        color: 'rgba(255,255,255,0.65)',
                        dashArray: '',
                        lineCap: 'butt',
                        lineJoin: 'miter',
                        weight: 1,
                        fill: true,
                        fillOpacity: 1,
                        fillColor: 'rgba(245,237,79,0.65)',
                        interactive: false,
                    }
                    break;
                case 'Intermedio':
                    return {
                        pane: 'pane_Uranio_0',
                        radius: 7.0,
                        opacity: 1,
                        color: 'rgba(255,255,255,0.65)',
                        dashArray: '',
                        lineCap: 'butt',
                        lineJoin: 'miter',
                        weight: 1,
                        fill: true,
                        fillOpacity: 1,
                        fillColor: 'rgba(255,56,22,0.65)',
                        interactive: false,
                    }
                    break;
                case 'Alto':
                    return {
                        pane: 'pane_Uranio_0',
                        radius: 9.0,
                        opacity: 1,
                        color: 'rgba(255,255,255,0.65)',
                        dashArray: '',
                        lineCap: 'butt',
                        lineJoin: 'miter',
                        weight: 1,
                        fill: true,
                        fillOpacity: 1,
                        fillColor: 'rgba(194,0,0,0.65)',
                        interactive: false,
                    }
                    break;
            }
        }
        map.createPane('pane_Uranio_0');
        map.getPane('pane_Uranio_0').style.zIndex = 400;
        map.getPane('pane_Uranio_0').style['mix-blend-mode'] = 'normal';
        var layer_Uranio_0 = new L.geoJson(json_Uranio_0, {
            attribution: '',
            interactive: false,
            dataVar: 'json_Uranio_0',
            layerName: 'layer_Uranio_0',
            pane: 'pane_Uranio_0',
            // onEachFeature: pop_Uranio_0,
            pointToLayer: function (feature, latlng) {
                var context = {
                    feature: feature,
                    variables: {}
                };
                return L.circleMarker(latlng, style_Uranio_0_0(feature));
            },
        });
        bounds_group.addLayer(layer_Uranio_0);
        map.addLayer(layer_Uranio_0);
        //setBounds();
    }
});
