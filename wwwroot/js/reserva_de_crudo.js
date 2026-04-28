
$(document).ready(function () {
    //iniciarMapas();
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


        function pop_Camposderecursosdecrudo3PMMB_0(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 3P</th>\
                            <td>' + (feature.properties['crudo_3p'] !== null ? autolinker.link(feature.properties['crudo_3p'].toLocaleString()) : '') + '</td>\
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
        function style_Camposderecursosdecrudo3PMMB_0_0(feature) {
            if (feature.properties['crudo_3p'] >= 0.000000 && feature.properties['crudo_3p'] <= 0.000000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo3PMMB_0',
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
            if (feature.properties['crudo_3p'] >= 0.000000 && feature.properties['crudo_3p'] <= 14.516000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo3PMMB_0',
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
            if (feature.properties['crudo_3p'] >= 14.516000 && feature.properties['crudo_3p'] <= 120.366000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo3PMMB_0',
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
            if (feature.properties['crudo_3p'] >= 120.366000 && feature.properties['crudo_3p'] <= 543.614000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo3PMMB_0',
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
            if (feature.properties['crudo_3p'] >= 543.614000 && feature.properties['crudo_3p'] <= 30821.070000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo3PMMB_0',
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
        map.createPane('pane_Camposderecursosdecrudo3PMMB_0');
        map.getPane('pane_Camposderecursosdecrudo3PMMB_0').style.zIndex = 400;
        map.getPane('pane_Camposderecursosdecrudo3PMMB_0').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdecrudo3PMMB_0 = new L.geoJson(json_Camposderecursosdecrudo3PMMB_0, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdecrudo3PMMB_0',
            layerName: 'layer_Camposderecursosdecrudo3PMMB_0',
            pane: 'pane_Camposderecursosdecrudo3PMMB_0',
            onEachFeature: pop_Camposderecursosdecrudo3PMMB_0,
            style: style_Camposderecursosdecrudo3PMMB_0_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdecrudo3PMMB_0);
        map.addLayer(layer_Camposderecursosdecrudo3PMMB_0);
        function pop_Camposderecursosdecrudo2PMMB_1(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 2P</th>\
                            <td>' + (feature.properties['crudo_2p'] !== null ? autolinker.link(feature.properties['crudo_2p'].toLocaleString()) : '') + '</td>\
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

        function style_Camposderecursosdecrudo2PMMB_1_0(feature) {
            if (feature.properties['crudo_2p'] >= 0.000000 && feature.properties['crudo_2p'] <= 0.000000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo2PMMB_1',
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
            if (feature.properties['crudo_2p'] >= 0.000000 && feature.properties['crudo_2p'] <= 7.452000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo2PMMB_1',
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
            if (feature.properties['crudo_2p'] >= 7.452000 && feature.properties['crudo_2p'] <= 89.622000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo2PMMB_1',
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
            if (feature.properties['crudo_2p'] >= 89.622000 && feature.properties['crudo_2p'] <= 420.862000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo2PMMB_1',
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
            if (feature.properties['crudo_2p'] >= 420.862000 && feature.properties['crudo_2p'] <= 30806.660000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo2PMMB_1',
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
        map.createPane('pane_Camposderecursosdecrudo2PMMB_1');
        map.getPane('pane_Camposderecursosdecrudo2PMMB_1').style.zIndex = 401;
        map.getPane('pane_Camposderecursosdecrudo2PMMB_1').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdecrudo2PMMB_1 = new L.geoJson(json_Camposderecursosdecrudo2PMMB_1, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdecrudo2PMMB_1',
            layerName: 'layer_Camposderecursosdecrudo2PMMB_1',
            pane: 'pane_Camposderecursosdecrudo2PMMB_1',
            onEachFeature: pop_Camposderecursosdecrudo2PMMB_1,
            style: style_Camposderecursosdecrudo2PMMB_1_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdecrudo2PMMB_1);
        map.addLayer(layer_Camposderecursosdecrudo2PMMB_1);
        function pop_Camposderecursosdecrudo1PMMB_2(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 1P</th>\
                            <td>' + (feature.properties['crudo_1p'] !== null ? autolinker.link(feature.properties['crudo_1p'].toLocaleString()) : '') + '</td>\
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

        function style_Camposderecursosdecrudo1PMMB_2_0(feature) {
            if (feature.properties['crudo_1p'] >= 0.000000 && feature.properties['crudo_1p'] <= 0.000000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo1PMMB_2',
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
            if (feature.properties['crudo_1p'] >= 0.000000 && feature.properties['crudo_1p'] <= 1.630000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo1PMMB_2',
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
            if (feature.properties['crudo_1p'] >= 1.630000 && feature.properties['crudo_1p'] <= 63.834000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo1PMMB_2',
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
            if (feature.properties['crudo_1p'] >= 63.834000 && feature.properties['crudo_1p'] <= 309.220000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo1PMMB_2',
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
            if (feature.properties['crudo_1p'] >= 309.220000 && feature.properties['crudo_1p'] <= 30687.860000) {
                return {
                    pane: 'pane_Camposderecursosdecrudo1PMMB_2',
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
        map.createPane('pane_Camposderecursosdecrudo1PMMB_2');
        map.getPane('pane_Camposderecursosdecrudo1PMMB_2').style.zIndex = 402;
        map.getPane('pane_Camposderecursosdecrudo1PMMB_2').style['mix-blend-mode'] = 'normal';
        var layer_Camposderecursosdecrudo1PMMB_2 = new L.geoJson(json_Camposderecursosdecrudo1PMMB_2, {
            attribution: '',
            interactive: true,
            dataVar: 'json_Camposderecursosdecrudo1PMMB_2',
            layerName: 'layer_Camposderecursosdecrudo1PMMB_2',
            pane: 'pane_Camposderecursosdecrudo1PMMB_2',
            onEachFeature: pop_Camposderecursosdecrudo1PMMB_2,
            style: style_Camposderecursosdecrudo1PMMB_2_0,
        });
        bounds_group.addLayer(layer_Camposderecursosdecrudo1PMMB_2);
        map.addLayer(layer_Camposderecursosdecrudo1PMMB_2);
        function pop_reasconrecursosdecrudo3PMMB_3(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 3P</th>\
                            <td>' + (feature.properties['crudo_3p'] !== null ? autolinker.link(feature.properties['crudo_3p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconrecursosdecrudo3PMMB_3_0(feature) {
            if (feature.properties['crudo_3p'] >= -1.000000 && feature.properties['crudo_3p'] <= 0.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo3PMMB_3',
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
            if (feature.properties['crudo_3p'] >= 0.000000 && feature.properties['crudo_3p'] <= 20.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo3PMMB_3',
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
            if (feature.properties['crudo_3p'] >= 20.000000 && feature.properties['crudo_3p'] <= 40.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo3PMMB_3',
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
            if (feature.properties['crudo_3p'] >= 40.000000 && feature.properties['crudo_3p'] <= 60.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo3PMMB_3',
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
            if (feature.properties['crudo_3p'] >= 60.000000 && feature.properties['crudo_3p'] <= 420.050000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo3PMMB_3',
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
        map.createPane('pane_reasconrecursosdecrudo3PMMB_3');
        map.getPane('pane_reasconrecursosdecrudo3PMMB_3').style.zIndex = 403;
        map.getPane('pane_reasconrecursosdecrudo3PMMB_3').style['mix-blend-mode'] = 'normal';
        var layer_reasconrecursosdecrudo3PMMB_3 = new L.geoJson(json_reasconrecursosdecrudo3PMMB_3, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconrecursosdecrudo3PMMB_3',
            layerName: 'layer_reasconrecursosdecrudo3PMMB_3',
            pane: 'pane_reasconrecursosdecrudo3PMMB_3',
            onEachFeature: pop_reasconrecursosdecrudo3PMMB_3,
            style: style_reasconrecursosdecrudo3PMMB_3_0,
        });
        bounds_group.addLayer(layer_reasconrecursosdecrudo3PMMB_3);
        map.addLayer(layer_reasconrecursosdecrudo3PMMB_3);
        function pop_reasconrecursosdecrudo2PMMB_4(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 2P</th>\
                            <td>' + (feature.properties['crudo_2p'] !== null ? autolinker.link(feature.properties['crudo_2p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconrecursosdecrudo2PMMB_4_0(feature) {
            if (feature.properties['crudo_2p'] >= -1.000000 && feature.properties['crudo_2p'] <= 0.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo2PMMB_4',
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
            if (feature.properties['crudo_2p'] >= 0.000000 && feature.properties['crudo_2p'] <= 20.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo2PMMB_4',
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
            if (feature.properties['crudo_2p'] >= 20.000000 && feature.properties['crudo_2p'] <= 40.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo2PMMB_4',
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
            if (feature.properties['crudo_2p'] >= 40.000000 && feature.properties['crudo_2p'] <= 60.000000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo2PMMB_4',
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
            if (feature.properties['crudo_2p'] >= 60.000000 && feature.properties['crudo_2p'] <= 391.370000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo2PMMB_4',
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
        map.createPane('pane_reasconrecursosdecrudo2PMMB_4');
        map.getPane('pane_reasconrecursosdecrudo2PMMB_4').style.zIndex = 404;
        map.getPane('pane_reasconrecursosdecrudo2PMMB_4').style['mix-blend-mode'] = 'normal';
        var layer_reasconrecursosdecrudo2PMMB_4 = new L.geoJson(json_reasconrecursosdecrudo2PMMB_4, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconrecursosdecrudo2PMMB_4',
            layerName: 'layer_reasconrecursosdecrudo2PMMB_4',
            pane: 'pane_reasconrecursosdecrudo2PMMB_4',
            onEachFeature: pop_reasconrecursosdecrudo2PMMB_4,
            style: style_reasconrecursosdecrudo2PMMB_4_0,
        });
        bounds_group.addLayer(layer_reasconrecursosdecrudo2PMMB_4);
        map.addLayer(layer_reasconrecursosdecrudo2PMMB_4);
        function pop_reasconrecursosdecrudo1PMMB_5(feature, layer) {
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
                            <td>' + (feature.properties['fid'] !== null ? autolinker.link(feature.properties['fid'].toLocaleString()) : '') + '</td>\
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
                            <th scope="row">Crudo 1P</th>\
                            <td>' + (feature.properties['crudo_1p'] !== null ? autolinker.link(feature.properties['crudo_1p'].toLocaleString()) : '') + '</td>\
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

        function style_reasconrecursosdecrudo1PMMB_5_0(feature) {
            if (feature.properties['Crudo 1P'] >= -1.000000 && feature.properties['Crudo 1P'] <= 77.474000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo1PMMB_5',
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
            if (feature.properties['Crudo 1P'] >= 77.474000 && feature.properties['Crudo 1P'] <= 155.948000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo1PMMB_5',
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
            if (feature.properties['Crudo 1P'] >= 155.948000 && feature.properties['Crudo 1P'] <= 234.422000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo1PMMB_5',
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
            if (feature.properties['Crudo 1P'] >= 234.422000 && feature.properties['Crudo 1P'] <= 312.896000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo1PMMB_5',
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
            if (feature.properties['Crudo 1P'] >= 312.896000 && feature.properties['Crudo 1P'] <= 391.370000) {
                return {
                    pane: 'pane_reasconrecursosdecrudo1PMMB_5',
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
        map.createPane('pane_reasconrecursosdecrudo1PMMB_5');
        map.getPane('pane_reasconrecursosdecrudo1PMMB_5').style.zIndex = 405;
        map.getPane('pane_reasconrecursosdecrudo1PMMB_5').style['mix-blend-mode'] = 'normal';
        var layer_reasconrecursosdecrudo1PMMB_5 = new L.geoJson(json_reasconrecursosdecrudo1PMMB_5, {
            attribution: '',
            interactive: true,
            dataVar: 'json_reasconrecursosdecrudo1PMMB_5',
            layerName: 'layer_reasconrecursosdecrudo1PMMB_5',
            pane: 'pane_reasconrecursosdecrudo1PMMB_5',
            onEachFeature: pop_reasconrecursosdecrudo1PMMB_5,
            style: style_reasconrecursosdecrudo1PMMB_5_0,
        });
        bounds_group.addLayer(layer_reasconrecursosdecrudo1PMMB_5);
        map.addLayer(layer_reasconrecursosdecrudo1PMMB_5);
        var baseMaps = {};
        L.control.layers(baseMaps, { 'Áreas con recursos de crudo 1P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo1PMMB_5_1770.png" /></td><td>-1 - 77</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo1PMMB_5_771561.png" /></td><td>77 - 156</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo1PMMB_5_1562342.png" /></td><td>156 - 234</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo1PMMB_5_2343133.png" /></td><td>234 - 313</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo1PMMB_5_3133914.png" /></td><td>313 - 391</td></tr></table>': layer_reasconrecursosdecrudo1PMMB_5, 'Áreas con recursos de crudo 2P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo2PMMB_4_100.png" /></td><td>-1 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo2PMMB_4_0201.png" /></td><td>0 - 20</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo2PMMB_4_20402.png" /></td><td>20 - 40</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo2PMMB_4_40603.png" /></td><td>40 - 60</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo2PMMB_4_60391374.png" /></td><td>60 - 391.37</td></tr></table>': layer_reasconrecursosdecrudo2PMMB_4, 'Áreas con recursos de crudo 3P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo3PMMB_3_100.png" /></td><td>-1 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo3PMMB_3_0201.png" /></td><td>0 - 20</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo3PMMB_3_20402.png" /></td><td>20 - 40</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo3PMMB_3_40603.png" /></td><td>40 - 60</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/reasconrecursosdecrudo3PMMB_3_60420054.png" /></td><td>60 - 420.05</td></tr></table>': layer_reasconrecursosdecrudo3PMMB_3, 'Campos de recursos de crudo 1P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo1PMMB_2_000.png" /></td><td>0 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo1PMMB_2_0161.png" /></td><td>0 - 1.6</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo1PMMB_2_166382.png" /></td><td>1.6 - 63.8</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo1PMMB_2_63830923.png" /></td><td>63.8 - 309.2</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo1PMMB_2_30923068794.png" /></td><td>309.2 - 30687.9</td></tr></table>': layer_Camposderecursosdecrudo1PMMB_2, 'Campos de recursos de crudo 2P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo2PMMB_1_000.png" /></td><td>0 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo2PMMB_1_0751.png" /></td><td>0 - 7.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo2PMMB_1_758962.png" /></td><td>7.5 - 89.6</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo2PMMB_1_89642093.png" /></td><td>89.6 - 420.9</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo2PMMB_1_42093080674.png" /></td><td>420.9 - 30806.7</td></tr></table>': layer_Camposderecursosdecrudo2PMMB_1, 'Campos de recursos de crudo 3P (MMB)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo3PMMB_0_000.png" /></td><td>0 - 0</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo3PMMB_0_0151.png" /></td><td>0 - 15</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo3PMMB_0_151202.png" /></td><td>15 - 120</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo3PMMB_0_1205443.png" /></td><td>120 - 544</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Camposderecursosdecrudo3PMMB_0_544308214.png" /></td><td>544 - 30821</td></tr></table>': layer_Camposderecursosdecrudo3PMMB_0, }).addTo(map);


    }
});
