
$(document).ready(function () {
    // Asegúrate de que los mapas estén cargados
    if (mapas && mapas.length > 0) {
        // Accede al primer mapa
        var map = mapas[0];

//Hover en los poligonos*@
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

        function pop_Camposderecursosdegas3PMMBPC_0(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 3P</th>\
                            <td>' + (feature.properties['gas_3p'] !== null ? autolinker.link(feature.properties['gas_3p'].toLocaleString()) : '') + '</td>\
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

        function style_Camposderecursosdegas3PMMBPC_0_0(feature) {
            if (feature.properties['gas_3p'] >= 0.000000 && feature.properties['gas_3p'] <= 24.894000) {
                return {
                    pane: 'pane_Camposderecursosdegas3PMMBPC_0',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,251,255,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 24.894000 && feature.properties['gas_3p'] <= 84.420000) {
                return {
                    pane: 'pane_Camposderecursosdegas3PMMBPC_0',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(200,220,240,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 84.420000 && feature.properties['gas_3p'] <= 234.828000) {
                return {
                    pane: 'pane_Camposderecursosdegas3PMMBPC_0',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(115,178,216,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 234.828000 && feature.properties['gas_3p'] <= 680.984000) {
                return {
                    pane: 'pane_Camposderecursosdegas3PMMBPC_0',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(41,121,185,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 680.984000 && feature.properties['gas_3p'] <= 15136.050000) {
                return {
                    pane: 'pane_Camposderecursosdegas3PMMBPC_0',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(8,48,107,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_Camposderecursosdegas3PMMBPC_0');
        map.getPane('pane_Camposderecursosdegas3PMMBPC_0').style.zIndex = 400;
        map.getPane('pane_Camposderecursosdegas3PMMBPC_0').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdegas3PMMBPC_0 = new L.geoJson(json_Camposderecursosdegas3PMMBPC_0, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdegas3PMMBPC_0',
            layerName: 'layer_Camposderecursosdegas3PMMBPC_0',
            pane: 'pane_Camposderecursosdegas3PMMBPC_0',
            onEachFeature: pop_Camposderecursosdegas3PMMBPC_0,
            style: style_Camposderecursosdegas3PMMBPC_0_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdegas3PMMBPC_0);
        map.addLayer(layer_Camposderecursosdegas3PMMBPC_0);
        function pop_Camposderecursosdegas2PMMBPC_1(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 2P</th>\
                            <td>' + (feature.properties['gas_2p'] !== null ? autolinker.link(feature.properties['gas_2p'].toLocaleString()) : '') + '</td>\
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

        function style_Camposderecursosdegas2PMMBPC_1_0(feature) {
            if (feature.properties['gas_2p'] >= 0.000000 && feature.properties['gas_2p'] <= 17.876000) {
                return {
                    pane: 'pane_Camposderecursosdegas2PMMBPC_1',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,251,255,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 17.876000 && feature.properties['gas_2p'] <= 67.672000) {
                return {
                    pane: 'pane_Camposderecursosdegas2PMMBPC_1',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(200,220,240,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 67.672000 && feature.properties['gas_2p'] <= 181.582000) {
                return {
                    pane: 'pane_Camposderecursosdegas2PMMBPC_1',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(115,178,216,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 181.582000 && feature.properties['gas_2p'] <= 550.626000) {
                return {
                    pane: 'pane_Camposderecursosdegas2PMMBPC_1',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(41,121,185,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 550.626000 && feature.properties['gas_2p'] <= 14791.690000) {
                return {
                    pane: 'pane_Camposderecursosdegas2PMMBPC_1',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(8,48,107,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_Camposderecursosdegas2PMMBPC_1');
        map.getPane('pane_Camposderecursosdegas2PMMBPC_1').style.zIndex = 401;
        map.getPane('pane_Camposderecursosdegas2PMMBPC_1').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdegas2PMMBPC_1 = new L.geoJson(json_Camposderecursosdegas2PMMBPC_1, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdegas2PMMBPC_1',
            layerName: 'layer_Camposderecursosdegas2PMMBPC_1',
            pane: 'pane_Camposderecursosdegas2PMMBPC_1',
            onEachFeature: pop_Camposderecursosdegas2PMMBPC_1,
            style: style_Camposderecursosdegas2PMMBPC_1_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdegas2PMMBPC_1);
        map.addLayer(layer_Camposderecursosdegas2PMMBPC_1);
        function pop_Camposderecursosdegas1PMMBPC_2(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 1P</th>\
                            <td>' + (feature.properties['gas_1p'] !== null ? autolinker.link(feature.properties['gas_1p'].toLocaleString()) : '') + '</td>\
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

        function style_Camposderecursosdegas1PMMBPC_2_0(feature) {
            if (feature.properties['gas_1p'] >= 0.000000 && feature.properties['gas_1p'] <= 10.860000) {
                return {
                    pane: 'pane_Camposderecursosdegas1PMMBPC_2',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,251,255,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 10.860000 && feature.properties['gas_1p'] <= 46.666000) {
                return {
                    pane: 'pane_Camposderecursosdegas1PMMBPC_2',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(200,220,240,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 46.666000 && feature.properties['gas_1p'] <= 137.882000) {
                return {
                    pane: 'pane_Camposderecursosdegas1PMMBPC_2',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(115,178,216,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 137.882000 && feature.properties['gas_1p'] <= 438.830000) {
                return {
                    pane: 'pane_Camposderecursosdegas1PMMBPC_2',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(41,121,185,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 438.830000 && feature.properties['gas_1p'] <= 14760.220000) {
                return {
                    pane: 'pane_Camposderecursosdegas1PMMBPC_2',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(8,48,107,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_Camposderecursosdegas1PMMBPC_2');
        map.getPane('pane_Camposderecursosdegas1PMMBPC_2').style.zIndex = 402;
        map.getPane('pane_Camposderecursosdegas1PMMBPC_2').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdegas1PMMBPC_2 = new L.geoJson(json_Camposderecursosdegas1PMMBPC_2, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdegas1PMMBPC_2',
            layerName: 'layer_Camposderecursosdegas1PMMBPC_2',
            pane: 'pane_Camposderecursosdegas1PMMBPC_2',
            onEachFeature: pop_Camposderecursosdegas1PMMBPC_2,
            style: style_Camposderecursosdegas1PMMBPC_2_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdegas1PMMBPC_2);
        map.addLayer(layer_Camposderecursosdegas1PMMBPC_2);
        function pop_reasconreservasdegas3PMMBPC_3(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 3P</th>\
                            <td>' + (feature.properties['gas_3p'] !== null ? autolinker.link(feature.properties['gas_3p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconreservasdegas3PMMBPC_3_0(feature) {
            if (feature.properties['gas_3p'] >= -1.000000 && feature.properties['gas_3p'] <= 151.632000) {
                return {
                    pane: 'pane_reasconreservasdegas3PMMBPC_3',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,252,245,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 151.632000 && feature.properties['gas_3p'] <= 304.264000) {
                return {
                    pane: 'pane_reasconreservasdegas3PMMBPC_3',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(201,234,194,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 304.264000 && feature.properties['gas_3p'] <= 456.896000) {
                return {
                    pane: 'pane_reasconreservasdegas3PMMBPC_3',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(123,199,124,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 456.896000 && feature.properties['gas_3p'] <= 609.528000) {
                return {
                    pane: 'pane_reasconreservasdegas3PMMBPC_3',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(42,146,75,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_3p'] >= 609.528000 && feature.properties['gas_3p'] <= 762.160000) {
                return {
                    pane: 'pane_reasconreservasdegas3PMMBPC_3',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(0,68,27,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_reasconreservasdegas3PMMBPC_3');
        map.getPane('pane_reasconreservasdegas3PMMBPC_3').style.zIndex = 403;
        map.getPane('pane_reasconreservasdegas3PMMBPC_3').style['mix-blend-mode'] = 'normal';
        var layer_reasconreservasdegas3PMMBPC_3 = new L.geoJson(json_reasconreservasdegas3PMMBPC_3, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconreservasdegas3PMMBPC_3',
            layerName: 'layer_reasconreservasdegas3PMMBPC_3',
            pane: 'pane_reasconreservasdegas3PMMBPC_3',
            onEachFeature: pop_reasconreservasdegas3PMMBPC_3,
            style: style_reasconreservasdegas3PMMBPC_3_0,
        });
        bounds_group.addLayer(layer_reasconreservasdegas3PMMBPC_3);
        map.addLayer(layer_reasconreservasdegas3PMMBPC_3);
        function pop_reasconreservasdegas2PMMBPC_4(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 2P</th>\
                            <td>' + (feature.properties['gas_2p'] !== null ? autolinker.link(feature.properties['gas_2p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconreservasdegas2PMMBPC_4_0(feature) {
            if (feature.properties['gas_2p'] >= -1.000000 && feature.properties['gas_2p'] <= 0.000000) {
                return {
                    pane: 'pane_reasconreservasdegas2PMMBPC_4',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,252,245,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 0.000000 && feature.properties['gas_2p'] <= 20.000000) {
                return {
                    pane: 'pane_reasconreservasdegas2PMMBPC_4',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(201,234,194,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 20.000000 && feature.properties['gas_2p'] <= 40.000000) {
                return {
                    pane: 'pane_reasconreservasdegas2PMMBPC_4',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(123,199,124,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 40.000000 && feature.properties['gas_2p'] <= 60.000000) {
                return {
                    pane: 'pane_reasconreservasdegas2PMMBPC_4',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(42,146,75,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_2p'] >= 60.000000 && feature.properties['gas_2p'] <= 553.160000) {
                return {
                    pane: 'pane_reasconreservasdegas2PMMBPC_4',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(0,68,27,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_reasconreservasdegas2PMMBPC_4');
        map.getPane('pane_reasconreservasdegas2PMMBPC_4').style.zIndex = 404;
        map.getPane('pane_reasconreservasdegas2PMMBPC_4').style['mix-blend-mode'] = 'normal';
        var layer_reasconreservasdegas2PMMBPC_4 = new L.geoJson(json_reasconreservasdegas2PMMBPC_4, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconreservasdegas2PMMBPC_4',
            layerName: 'layer_reasconreservasdegas2PMMBPC_4',
            pane: 'pane_reasconreservasdegas2PMMBPC_4',
            onEachFeature: pop_reasconreservasdegas2PMMBPC_4,
            style: style_reasconreservasdegas2PMMBPC_4_0,
        });
        bounds_group.addLayer(layer_reasconreservasdegas2PMMBPC_4);
        map.addLayer(layer_reasconreservasdegas2PMMBPC_4);
        function pop_reasconreservasdegas1PMMBPC_5(feature, layer) {
            //var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
            layer.on({
                mouseout: function (e) {
                    for (i in e.target._eventParents) {
                        e.target._eventParents[i].resetStyle(e.target);
                    }
                },
                mouseover: highlightFeature,
            });

            // Variables para almacenar las coordenadas de clic
            let clickedLat, clickedLon;

            // Función para actualizar el contenido del popup con las coordenadas de clic
            function updatePopupContent() {
                var popupContent = '<table>\
                        <tr>\
                            <th scope="row">FID </th>\
                            <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Nombre </th>\
                            <td>' + (feature.properties['nombre'] !== null ? autolinker.link(feature.properties['nombre'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Superficie </th>\
                            <td>' + (feature.properties['superficie'] !== null ? autolinker.link(feature.properties['superficie'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Ubicación </th>\
                            <td>' + (feature.properties['ubicacion'] !== null ? autolinker.link(feature.properties['ubicacion'].toLocaleString()) : '') + '</td>\
                        </tr>\
                        <tr>\
                            <th scope="row">Gas 1P</th>\
                            <td>' + (feature.properties['gas_1p'] !== null ? autolinker.link(feature.properties['gas_1p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconreservasdegas1PMMBPC_5_0(feature) {
            if (feature.properties['gas_1p'] >= -1.000000 && feature.properties['gas_1p'] <= 0.000000) {
                return {
                    pane: 'pane_reasconreservasdegas1PMMBPC_5',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(247,252,245,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 0.000000 && feature.properties['gas_1p'] <= 20.000000) {
                return {
                    pane: 'pane_reasconreservasdegas1PMMBPC_5',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(201,234,194,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 20.000000 && feature.properties['gas_1p'] <= 40.000000) {
                return {
                    pane: 'pane_reasconreservasdegas1PMMBPC_5',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(123,199,124,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 40.000000 && feature.properties['gas_1p'] <= 60.000000) {
                return {
                    pane: 'pane_reasconreservasdegas1PMMBPC_5',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(42,146,75,0.5)',
                    interactive: true,
                }
            }
            if (feature.properties['gas_1p'] >= 60.000000 && feature.properties['gas_1p'] <= 553.160000) {
                return {
                    pane: 'pane_reasconreservasdegas1PMMBPC_5',
                    opacity: 1,
                    color: 'rgba(35,35,35,0.0)',
                    dashArray: '',
                    lineCap: 'butt',
                    lineJoin: 'miter',
                    weight: 1.0,
                    fill: true,
                    fillOpacity: 1,
                    fillColor: 'rgba(0,68,27,0.5)',
                    interactive: true,
                }
            }
        }
        map.createPane('pane_reasconreservasdegas1PMMBPC_5');
        map.getPane('pane_reasconreservasdegas1PMMBPC_5').style.zIndex = 405;
        map.getPane('pane_reasconreservasdegas1PMMBPC_5').style['mix-blend-mode'] = 'normal';
        var layer_reasconreservasdegas1PMMBPC_5 = new L.geoJson(json_reasconreservasdegas1PMMBPC_5, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconreservasdegas1PMMBPC_5',
            layerName: 'layer_reasconreservasdegas1PMMBPC_5',
            pane: 'pane_reasconreservasdegas1PMMBPC_5',
            onEachFeature: pop_reasconreservasdegas1PMMBPC_5,
            style: style_reasconreservasdegas1PMMBPC_5_0,
        });
        bounds_group.addLayer(layer_reasconreservasdegas1PMMBPC_5);
        map.addLayer(layer_reasconreservasdegas1PMMBPC_5);
        var baseMaps = {};
        L.control.layers(baseMaps, { 'Áreas con reservas de gas 1P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas1PMMBPC_5_100.png" /></td><td>-1 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas1PMMBPC_5_0201.png" /></td><td>0 - 20</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas1PMMBPC_5_20402.png" /></td><td>20 - 40</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas1PMMBPC_5_40603.png" /></td><td>40 - 60</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas1PMMBPC_5_6055324.png" /></td><td>60 - 553.2</td></tr></table>': layer_reasconreservasdegas1PMMBPC_5, 'Áreas con reservas de gas 2P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas2PMMBPC_4_100.png" /></td><td>-1 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas2PMMBPC_4_0201.png" /></td><td>0 - 20</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas2PMMBPC_4_20402.png" /></td><td>20 - 40</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas2PMMBPC_4_40603.png" /></td><td>40 - 60</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas2PMMBPC_4_6055324.png" /></td><td>60 - 553.2</td></tr></table>': layer_reasconreservasdegas2PMMBPC_4, 'Áreas con reservas de gas 3P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas3PMMBPC_3_11520.png" /></td><td>-1 - 152</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas3PMMBPC_3_1523041.png" /></td><td>152 - 304</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas3PMMBPC_3_3044572.png" /></td><td>304 - 457</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas3PMMBPC_3_4576103.png" /></td><td>457 - 610</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconreservasdegas3PMMBPC_3_6107624.png" /></td><td>610 - 762</td></tr></table>': layer_reasconreservasdegas3PMMBPC_3, 'Campos de recursos de gas 1P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas1PMMBPC_2_0110.png" /></td><td>0 - 11</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas1PMMBPC_2_11471.png" /></td><td>11 - 47</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas1PMMBPC_2_471382.png" /></td><td>47 - 138</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas1PMMBPC_2_1384393.png" /></td><td>138 - 439</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas1PMMBPC_2_439147604.png" /></td><td>439 - 14760</td></tr></table>': layer_Camposderecursosdegas1PMMBPC_2, 'Campos de recursos de gas 2P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas2PMMBPC_1_0180.png" /></td><td>0 - 18</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas2PMMBPC_1_18681.png" /></td><td>18 - 68</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas2PMMBPC_1_681822.png" /></td><td>68 - 182</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas2PMMBPC_1_1825513.png" /></td><td>182 - 551</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas2PMMBPC_1_551147924.png" /></td><td>551 - 14792</td></tr></table>': layer_Camposderecursosdegas2PMMBPC_1, 'Campos de recursos de gas 3P (MMMPC)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas3PMMBPC_0_0250.png" /></td><td>0 - 25</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas3PMMBPC_0_25841.png" /></td><td>25 - 84</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas3PMMBPC_0_842352.png" /></td><td>84 - 235</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas3PMMBPC_0_2356813.png" /></td><td>235 - 681</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdegas3PMMBPC_0_681151364.png" /></td><td>681 - 15136</td></tr></table>': layer_Camposderecursosdegas3PMMBPC_0, }).addTo(map);

    }
});