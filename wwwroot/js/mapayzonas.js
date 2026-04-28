    //   var map = L.map('map', {
    //         zoomControl: false, maxZoom: 28, minZoom: 1
    //     }).fitBounds([[9.83232073956332, -118.23178909072449], [35.40435712694164, -86.38838648572273]]);
    //     var hash = new L.Hash(map);
    //     map.attributionControl.setPrefix('<a href="https://github.com/tomchadwin/qgis2web" target="_blank">qgis2web</a> &middot; <a href="https://leafletjs.com" title="A JS library for interactive maps">Leaflet</a> &middot; <a href="https://qgis.org">QGIS</a>');
    //     var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });
    //     // remove popup's row if "visible-with-data"
function removeEmptyRowsFromPopupContent(content, feature) {
    var tempDiv = document.createElement('div');
    tempDiv.innerHTML = content;
    var rows = tempDiv.querySelectorAll('tr');
    for (var i = 0; i < rows.length; i++) {
        var td = rows[i].querySelector('td.visible-with-data');
        var key = td ? td.id : '';
        if (td && td.classList.contains('visible-with-data') && feature.properties[key] == null) {
            rows[i].parentNode.removeChild(rows[i]);
        }
    }
    return tempDiv.innerHTML;
   }
   // add class to format popup if it contains media
   function addClassToPopupIfMedia(content, popup) {
       var tempDiv = document.createElement('div');
       tempDiv.innerHTML = content;
       if (tempDiv.querySelector('td img')) {
           popup._contentNode.classList.add('media');
               // Delay to force the redraw
               setTimeout(function() {
                   popup.update();
               }, 10);
       } else {
           popup._contentNode.classList.remove('media');
       }
   }
   var zoomControl = L.control.zoom({
       position: 'topleft'
   }).addTo(map);
   var bounds_group = new L.featureGroup([]);
   function setBounds() {
   }
 //  map.createPane('pane_Positron_0');
 //  map.getPane('pane_Positron_0').style.zIndex = 400;
 //  var layer_Positron_0 = L.tileLayer('https://a.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
//       pane: 'pane_Positron_0',
//       opacity: 1.0,
//       attribution: '<a href="https://cartodb.com/basemaps/">Map tiles by CartoDB, under CC BY 3.0. Data by OpenStreetMap, under ODbL.</a>',
//       minZoom: 1,
//       maxZoom: 28,
//       minNativeZoom: 0,
//       maxNativeZoom: 20
//   });
//   layer_Positron_0;
 //  map.addLayer(layer_Positron_0);
   function pop_RAMSAR_1(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">RAMSAR: </th>\
                   <td>' + (feature.properties['RAMSAR'] !== null ? autolinker.link(String(feature.properties['RAMSAR']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['ESTADO'] !== null ? autolinker.link(String(feature.properties['ESTADO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipios: </th>\
                   <td>' + (feature.properties['MUNICIPIOS'] !== null ? autolinker.link(String(feature.properties['MUNICIPIOS']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fecha: </th>\
                   <td>' + (feature.properties['FECHA'] !== null ? autolinker.link(String(feature.properties['FECHA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Número: </th>\
                   <td>' + (feature.properties['NUMERO'] !== null ? autolinker.link(String(feature.properties['NUMERO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_RAMSAR_1_0() {
       return {
           pane: 'pane_RAMSAR_1',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(178,223,138,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_RAMSAR_1');
   map.getPane('pane_RAMSAR_1').style.zIndex = 401;
   map.getPane('pane_RAMSAR_1').style['mix-blend-mode'] = 'normal';
   var layer_RAMSAR_1 = new L.geoJson(json_RAMSAR_1, {
       attribution: '',
       interactive: true,
       dataVar: 'json_RAMSAR_1',
       layerName: 'layer_RAMSAR_1',
       pane: 'pane_RAMSAR_1',
       onEachFeature: pop_RAMSAR_1,
       style: style_RAMSAR_1_0,
   });
   bounds_group.addLayer(layer_RAMSAR_1);
   function pop_reasnaturalesprotegidas_2(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Nombre: </th>\
                   <td>' + (feature.properties['NOMBRE'] !== null ? autolinker.link(String(feature.properties['NOMBRE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Decreto: </th>\
                   <td>' + (feature.properties['CAT_DECRET'] !== null ? autolinker.link(String(feature.properties['CAT_DECRET']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Manejo: </th>\
                   <td>' + (feature.properties['CAT_MANEJO'] !== null ? autolinker.link(String(feature.properties['CAT_MANEJO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Estados: </th>\
                   <td>' + (feature.properties['ESTADOS'] !== null ? autolinker.link(String(feature.properties['ESTADOS']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipios: </th>\
                   <td>' + (feature.properties['MUNICIPIOS'] !== null ? autolinker.link(String(feature.properties['MUNICIPIOS']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Región: </th>\
                   <td>' + (feature.properties['REGION'] !== null ? autolinker.link(String(feature.properties['REGION']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Superficie: </th>\
                   <td>' + (feature.properties['SUPERFICIE'] !== null ? autolinker.link(String(feature.properties['SUPERFICIE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_reasnaturalesprotegidas_2_0() {
       return {
           pane: 'pane_reasnaturalesprotegidas_2',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(51,160,44,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_reasnaturalesprotegidas_2');
   map.getPane('pane_reasnaturalesprotegidas_2').style.zIndex = 402;
   map.getPane('pane_reasnaturalesprotegidas_2').style['mix-blend-mode'] = 'normal';
   var layer_reasnaturalesprotegidas_2 = new L.geoJson(json_reasnaturalesprotegidas_2, {
       attribution: '',
       interactive: true,
       dataVar: 'json_reasnaturalesprotegidas_2',
       layerName: 'layer_reasnaturalesprotegidas_2',
       pane: 'pane_reasnaturalesprotegidas_2',
       onEachFeature: pop_reasnaturalesprotegidas_2,
       style: style_reasnaturalesprotegidas_2_0,
   });
   bounds_group.addLayer(layer_reasnaturalesprotegidas_2);
   function pop_Altaincidenciaciclones_3(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Incidencia: </th>\
                   <td>' + (feature.properties['Incidencia'] !== null ? autolinker.link(String(feature.properties['Incidencia']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Altaincidenciaciclones_3_0() {
       return {
           pane: 'pane_Altaincidenciaciclones_3',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(217,219,216,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Altaincidenciaciclones_3');
   map.getPane('pane_Altaincidenciaciclones_3').style.zIndex = 403;
   map.getPane('pane_Altaincidenciaciclones_3').style['mix-blend-mode'] = 'normal';
   var layer_Altaincidenciaciclones_3 = new L.geoJson(json_Altaincidenciaciclones_3, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Altaincidenciaciclones_3',
       layerName: 'layer_Altaincidenciaciclones_3',
       pane: 'pane_Altaincidenciaciclones_3',
       onEachFeature: pop_Altaincidenciaciclones_3,
       style: style_Altaincidenciaciclones_3_0,
   });
   bounds_group.addLayer(layer_Altaincidenciaciclones_3);
   function pop_Volcanesactivos_4(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Volcanesactivos_4_0() {
       return {
           pane: 'pane_Volcanesactivos_4',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(227,26,28,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_Volcanesactivos_4');
   map.getPane('pane_Volcanesactivos_4').style.zIndex = 404;
   map.getPane('pane_Volcanesactivos_4').style['mix-blend-mode'] = 'normal';
   var layer_Volcanesactivos_4 = new L.geoJson(json_Volcanesactivos_4, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Volcanesactivos_4',
       layerName: 'layer_Volcanesactivos_4',
       pane: 'pane_Volcanesactivos_4',
       onEachFeature: pop_Volcanesactivos_4,
       style: style_Volcanesactivos_4_0,
   });
   bounds_group.addLayer(layer_Volcanesactivos_4);
   function pop_Aerodromos_5(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Aerodromos_5_0() {
       return {
           pane: 'pane_Aerodromos_5',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(247,251,255,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_Aerodromos_5');
   map.getPane('pane_Aerodromos_5').style.zIndex = 405;
   map.getPane('pane_Aerodromos_5').style['mix-blend-mode'] = 'normal';
   var layer_Aerodromos_5 = new L.geoJson(json_Aerodromos_5, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Aerodromos_5',
       layerName: 'layer_Aerodromos_5',
       pane: 'pane_Aerodromos_5',
       onEachFeature: pop_Aerodromos_5,
       style: style_Aerodromos_5_0,
   });
   bounds_group.addLayer(layer_Aerodromos_5);
   function pop_Rosprincipales_6(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Nombre: </th>\
                   <td>' + (feature.properties['nom_rio'] !== null ? autolinker.link(String(feature.properties['nom_rio']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Rosprincipales_6_0() {
       return {
           pane: 'pane_Rosprincipales_6',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(31,120,180,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Rosprincipales_6');
   map.getPane('pane_Rosprincipales_6').style.zIndex = 406;
   map.getPane('pane_Rosprincipales_6').style['mix-blend-mode'] = 'normal';
   var layer_Rosprincipales_6 = new L.geoJson(json_Rosprincipales_6, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Rosprincipales_6',
       layerName: 'layer_Rosprincipales_6',
       pane: 'pane_Rosprincipales_6',
       onEachFeature: pop_Rosprincipales_6,
       style: style_Rosprincipales_6_0,
   });
   bounds_group.addLayer(layer_Rosprincipales_6);
   function pop_Vasfrreas_7(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Vasfrreas_7_0() {
       return {
           pane: 'pane_Vasfrreas_7',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(0,0,0,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_Vasfrreas_7');
   map.getPane('pane_Vasfrreas_7').style.zIndex = 407;
   map.getPane('pane_Vasfrreas_7').style['mix-blend-mode'] = 'normal';
   var layer_Vasfrreas_7 = new L.geoJson(json_Vasfrreas_7, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Vasfrreas_7',
       layerName: 'layer_Vasfrreas_7',
       pane: 'pane_Vasfrreas_7',
       onEachFeature: pop_Vasfrreas_7,
       style: style_Vasfrreas_7_0,
   });
   bounds_group.addLayer(layer_Vasfrreas_7);
   function pop_ZonaslejanasalaRNT_8(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['CVE_ENT'] !== null ? autolinker.link(String(feature.properties['CVE_ENT']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['NOM_ENT'] !== null ? autolinker.link(String(feature.properties['NOM_ENT']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_ZonaslejanasalaRNT_8_0() {
       return {
           pane: 'pane_ZonaslejanasalaRNT_8',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(175,158,148,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_ZonaslejanasalaRNT_8');
   map.getPane('pane_ZonaslejanasalaRNT_8').style.zIndex = 408;
   map.getPane('pane_ZonaslejanasalaRNT_8').style['mix-blend-mode'] = 'normal';
   var layer_ZonaslejanasalaRNT_8 = new L.geoJson(json_ZonaslejanasalaRNT_8, {
       attribution: '',
       interactive: false,
       dataVar: 'json_ZonaslejanasalaRNT_8',
       layerName: 'layer_ZonaslejanasalaRNT_8',
       pane: 'pane_ZonaslejanasalaRNT_8',
       onEachFeature: pop_ZonaslejanasalaRNT_8,
       style: style_ZonaslejanasalaRNT_8_0,
   });
   bounds_group.addLayer(layer_ZonaslejanasalaRNT_8);
   function pop_Localidadesrurales_9(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Diss'] !== null ? autolinker.link(String(feature.properties['Diss']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Localidadesrurales_9_0() {
       return {
           pane: 'pane_Localidadesrurales_9',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(145,82,45,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_Localidadesrurales_9');
   map.getPane('pane_Localidadesrurales_9').style.zIndex = 409;
   map.getPane('pane_Localidadesrurales_9').style['mix-blend-mode'] = 'normal';
   var layer_Localidadesrurales_9 = new L.geoJson(json_Localidadesrurales_9, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Localidadesrurales_9',
       layerName: 'layer_Localidadesrurales_9',
       pane: 'pane_Localidadesrurales_9',
       onEachFeature: pop_Localidadesrurales_9,
       style: style_Localidadesrurales_9_0,
   });
   bounds_group.addLayer(layer_Localidadesrurales_9);
   function pop_Localidadesurbanas_10(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['OBJECTID'] !== null ? autolinker.link(String(feature.properties['OBJECTID']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STAr'] !== null ? autolinker.link(String(feature.properties['Shape_STAr']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['Shape_STLe'] !== null ? autolinker.link(String(feature.properties['Shape_STLe']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Localidadesurbanas_10_0() {
       return {
           pane: 'pane_Localidadesurbanas_10',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(255,127,0,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_Localidadesurbanas_10');
   map.getPane('pane_Localidadesurbanas_10').style.zIndex = 410;
   map.getPane('pane_Localidadesurbanas_10').style['mix-blend-mode'] = 'normal';
   var layer_Localidadesurbanas_10 = new L.geoJson(json_Localidadesurbanas_10, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Localidadesurbanas_10',
       layerName: 'layer_Localidadesurbanas_10',
       pane: 'pane_Localidadesurbanas_10',
       onEachFeature: pop_Localidadesurbanas_10,
       style: style_Localidadesurbanas_10_0,
   });
   bounds_group.addLayer(layer_Localidadesurbanas_10);
   function pop_Zonamonumentosarqueolgicos_11(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Nombre: </th>\
                   <td>' + (feature.properties['Name'] !== null ? autolinker.link(String(feature.properties['Name']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Zonamonumentosarqueolgicos_11_0() {
       return {
           pane: 'pane_Zonamonumentosarqueolgicos_11',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(152,125,183,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Zonamonumentosarqueolgicos_11');
   map.getPane('pane_Zonamonumentosarqueolgicos_11').style.zIndex = 411;
   map.getPane('pane_Zonamonumentosarqueolgicos_11').style['mix-blend-mode'] = 'normal';
   var layer_Zonamonumentosarqueolgicos_11 = new L.geoJson(json_Zonamonumentosarqueolgicos_11, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Zonamonumentosarqueolgicos_11',
       layerName: 'layer_Zonamonumentosarqueolgicos_11',
       pane: 'pane_Zonamonumentosarqueolgicos_11',
       onEachFeature: pop_Zonamonumentosarqueolgicos_11,
       style: style_Zonamonumentosarqueolgicos_11_0,
   });
   bounds_group.addLayer(layer_Zonamonumentosarqueolgicos_11);
   function pop_Zonamonumentoshistricos_12(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">RPMZH: </th>\
                   <td>' + (feature.properties['RPMZH'] !== null ? autolinker.link(String(feature.properties['RPMZH']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">ZMH: </th>\
                   <td>' + (feature.properties['ZMH'] !== null ? autolinker.link(String(feature.properties['ZMH']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Zonamonumentoshistricos_12_0() {
       return {
           pane: 'pane_Zonamonumentoshistricos_12',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(183,72,75,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Zonamonumentoshistricos_12');
   map.getPane('pane_Zonamonumentoshistricos_12').style.zIndex = 412;
   map.getPane('pane_Zonamonumentoshistricos_12').style['mix-blend-mode'] = 'normal';
   var layer_Zonamonumentoshistricos_12 = new L.geoJson(json_Zonamonumentoshistricos_12, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Zonamonumentoshistricos_12',
       layerName: 'layer_Zonamonumentoshistricos_12',
       pane: 'pane_Zonamonumentoshistricos_12',
       onEachFeature: pop_Zonamonumentoshistricos_12,
       style: style_Zonamonumentoshistricos_12_0,
   });
   bounds_group.addLayer(layer_Zonamonumentoshistricos_12);
   function pop_PotencialElicoms_13(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Velocidad de viento(m/s): </th>\
                   <td>' + (feature.properties['wsp80'] !== null ? autolinker.link(String(feature.properties['wsp80']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialElicoms_13_0(feature) {
       switch(String(feature.properties['wsp80'])) {
           case '6.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(247,251,255,1.0)',
           interactive: true,
       }
               break;
           case '6.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(230,240,249,1.0)',
           interactive: true,
       }
               break;
           case '7.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(212,229,244,1.0)',
           interactive: true,
       }
               break;
           case '7.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(194,217,238,1.0)',
           interactive: true,
       }
               break;
           case '8.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(166,205,228,1.0)',
           interactive: true,
       }
               break;
           case '8.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(133,188,220,1.0)',
           interactive: true,
       }
               break;
           case '9.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(99,169,211,1.0)',
           interactive: true,
       }
               break;
           case '9.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(70,149,200,1.0)',
           interactive: true,
       }
               break;
           case '10.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(46,126,188,1.0)',
           interactive: true,
       }
               break;
           case '10.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(25,103,173,1.0)',
           interactive: true,
       }
               break;
           case '11.0':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 0,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(8,78,152,1.0)',
           interactive: true,
       }
               break;
           case '11.5':
               return {
           pane: 'pane_PotencialElicoms_13',
           opacity: 0,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(8,48,107,1.0)',
           interactive: true,
       }
               break;
       }
   }
   map.createPane('pane_PotencialElicoms_13');
   map.getPane('pane_PotencialElicoms_13').style.zIndex = 413;
   map.getPane('pane_PotencialElicoms_13').style['mix-blend-mode'] = 'normal';
   var layer_PotencialElicoms_13 = new L.geoJson(json_PotencialElicoms_13, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialElicoms_13',
       layerName: 'layer_PotencialElicoms_13',
       pane: 'pane_PotencialElicoms_13',
       onEachFeature: pop_PotencialElicoms_13,
       style: style_PotencialElicoms_13_0,
   });
   bounds_group.addLayer(layer_PotencialElicoms_13);
   function pop_PotencialresiduosForestales_14(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['ESTADO'] !== null ? autolinker.link(String(feature.properties['ESTADO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipio: </th>\
                   <td>' + (feature.properties['MUNICIPIO'] !== null ? autolinker.link(String(feature.properties['MUNICIPIO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Producto: </th>\
                   <td>' + (feature.properties['PRODUCTO'] !== null ? autolinker.link(String(feature.properties['PRODUCTO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialresiduosForestales_14_0() {
       return {
           pane: 'pane_PotencialresiduosForestales_14',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(190,207,80,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_PotencialresiduosForestales_14');
   map.getPane('pane_PotencialresiduosForestales_14').style.zIndex = 414;
   map.getPane('pane_PotencialresiduosForestales_14').style['mix-blend-mode'] = 'normal';
   var layer_PotencialresiduosForestales_14 = new L.geoJson(json_PotencialresiduosForestales_14, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialresiduosForestales_14',
       layerName: 'layer_PotencialresiduosForestales_14',
       pane: 'pane_PotencialresiduosForestales_14',
       onEachFeature: pop_PotencialresiduosForestales_14,
       style: style_PotencialresiduosForestales_14_0,
   });
   bounds_group.addLayer(layer_PotencialresiduosForestales_14);
   function pop_PotencialresiduosIndustriales_15(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['ESTADO'] !== null ? autolinker.link(String(feature.properties['ESTADO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipio: </th>\
                   <td>' + (feature.properties['MUN'] !== null ? autolinker.link(String(feature.properties['MUN']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Localidad: </th>\
                   <td>' + (feature.properties['LOCALIDAD'] !== null ? autolinker.link(String(feature.properties['LOCALIDAD']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Nombre: </th>\
                   <td>' + (feature.properties['NOMBRE'] !== null ? autolinker.link(String(feature.properties['NOMBRE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Energía: </th>\
                   <td>' + (feature.properties['ENERGIA'] !== null ? autolinker.link(String(feature.properties['ENERGIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clasificación: </th>\
                   <td>' + (feature.properties['CLASIFICAC'] !== null ? autolinker.link(String(feature.properties['CLASIFICAC']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Industria: </th>\
                   <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fuente: </th>\
                   <td>' + (feature.properties['FUENTE'] !== null ? autolinker.link(String(feature.properties['FUENTE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Tipo: </th>\
                   <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Proceso: </th>\
                   <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialresiduosIndustriales_15_0() {
       return {
           pane: 'pane_PotencialresiduosIndustriales_15',
           radius: 4.4,
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1,
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(231,113,72,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_PotencialresiduosIndustriales_15');
   map.getPane('pane_PotencialresiduosIndustriales_15').style.zIndex = 415;
   map.getPane('pane_PotencialresiduosIndustriales_15').style['mix-blend-mode'] = 'normal';
   var layer_PotencialresiduosIndustriales_15 = new L.geoJson(json_PotencialresiduosIndustriales_15, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialresiduosIndustriales_15',
       layerName: 'layer_PotencialresiduosIndustriales_15',
       pane: 'pane_PotencialresiduosIndustriales_15',
       onEachFeature: pop_PotencialresiduosIndustriales_15,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_PotencialresiduosIndustriales_15_0(feature));
       },
   });
   bounds_group.addLayer(layer_PotencialresiduosIndustriales_15);
   function pop_PotencialresiduosPecuarios_16(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['Estado'] !== null ? autolinker.link(String(feature.properties['Estado']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipio: </th>\
                   <td>' + (feature.properties['Municipio'] !== null ? autolinker.link(String(feature.properties['Municipio']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Energía: </th>\
                   <td>' + (feature.properties['ENERGIA'] !== null ? autolinker.link(String(feature.properties['ENERGIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clasificación: </th>\
                   <td>' + (feature.properties['CLASIFICAC'] !== null ? autolinker.link(String(feature.properties['CLASIFICAC']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Industria: </th>\
                   <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Tipo: </th>\
                   <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Proceso: </th>\
                   <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fuente: </th>\
                   <td>' + (feature.properties['FUENTE'] !== null ? autolinker.link(String(feature.properties['FUENTE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialresiduosPecuarios_16_0() {
       return {
           pane: 'pane_PotencialresiduosPecuarios_16',
           radius: 4.4,
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1,
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(255,158,23,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_PotencialresiduosPecuarios_16');
   map.getPane('pane_PotencialresiduosPecuarios_16').style.zIndex = 416;
   map.getPane('pane_PotencialresiduosPecuarios_16').style['mix-blend-mode'] = 'normal';
   var layer_PotencialresiduosPecuarios_16 = new L.geoJson(json_PotencialresiduosPecuarios_16, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialresiduosPecuarios_16',
       layerName: 'layer_PotencialresiduosPecuarios_16',
       pane: 'pane_PotencialresiduosPecuarios_16',
       onEachFeature: pop_PotencialresiduosPecuarios_16,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_PotencialresiduosPecuarios_16_0(feature));
       },
   });
   bounds_group.addLayer(layer_PotencialresiduosPecuarios_16);
   function pop_PotencialresiduosUrbanos_17(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['ESTADO'] !== null ? autolinker.link(String(feature.properties['ESTADO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipio: </th>\
                   <td>' + (feature.properties['MUN'] !== null ? autolinker.link(String(feature.properties['MUN']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Localidad: </th>\
                   <td>' + (feature.properties['LOCALIDAD'] !== null ? autolinker.link(String(feature.properties['LOCALIDAD']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Energía: </th>\
                   <td>' + (feature.properties['ENERGIA'] !== null ? autolinker.link(String(feature.properties['ENERGIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clasificación: </th>\
                   <td class="visible-with-data" id="CLASIFICAC">' + (feature.properties['CLASIFICAC'] !== null ? autolinker.link(String(feature.properties['CLASIFICAC']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Industria: </th>\
                   <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Tipo: </th>\
                   <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fuente: </th>\
                   <td>' + (feature.properties['FUENTE'] !== null ? autolinker.link(String(feature.properties['FUENTE']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Proceso: </th>\
                   <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialresiduosUrbanos_17_0() {
       return {
           pane: 'pane_PotencialresiduosUrbanos_17',
           radius: 4.4,
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1,
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(141,90,153,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_PotencialresiduosUrbanos_17');
   map.getPane('pane_PotencialresiduosUrbanos_17').style.zIndex = 417;
   map.getPane('pane_PotencialresiduosUrbanos_17').style['mix-blend-mode'] = 'normal';
   var layer_PotencialresiduosUrbanos_17 = new L.geoJson(json_PotencialresiduosUrbanos_17, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialresiduosUrbanos_17',
       layerName: 'layer_PotencialresiduosUrbanos_17',
       pane: 'pane_PotencialresiduosUrbanos_17',
       onEachFeature: pop_PotencialresiduosUrbanos_17,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_PotencialresiduosUrbanos_17_0(feature));
       },
   });
   bounds_group.addLayer(layer_PotencialresiduosUrbanos_17);
   function pop_PotencialGeotrmico_18(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Temperatura: </th>\
                   <td>' + (feature.properties['TEMPERATUR'] !== null ? autolinker.link(String(feature.properties['TEMPERATUR']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_PotencialGeotrmico_18_0() {
       return {
           pane: 'pane_PotencialGeotrmico_18',
           radius: 4.4,
           opacity: 1,
           color: 'rgba(250,250,248,1.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0,
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(227,26,28,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_PotencialGeotrmico_18');
   map.getPane('pane_PotencialGeotrmico_18').style.zIndex = 418;
   map.getPane('pane_PotencialGeotrmico_18').style['mix-blend-mode'] = 'normal';
   var layer_PotencialGeotrmico_18 = new L.geoJson(json_PotencialGeotrmico_18, {
       attribution: '',
       interactive: true,
       dataVar: 'json_PotencialGeotrmico_18',
       layerName: 'layer_PotencialGeotrmico_18',
       pane: 'pane_PotencialGeotrmico_18',
       onEachFeature: pop_PotencialGeotrmico_18,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_PotencialGeotrmico_18_0(feature));
       },
   });
   bounds_group.addLayer(layer_PotencialGeotrmico_18);
   function pop_GeotermiaWmC_19(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['heat_flow'] !== null ? autolinker.link(String(feature.properties['heat_flow']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_GeotermiaWmC_19_0(feature) {
       if (feature.properties['heat_flow'] >= 3.641980 && feature.properties['heat_flow'] <= 54.000000 ) {
           return {
           pane: 'pane_GeotermiaWmC_19',
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
       if (feature.properties['heat_flow'] >= 54.000000 && feature.properties['heat_flow'] <= 66.000000 ) {
           return {
           pane: 'pane_GeotermiaWmC_19',
           radius: 7.0,
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
       if (feature.properties['heat_flow'] >= 66.000000 && feature.properties['heat_flow'] <= 76.000000 ) {
           return {
           pane: 'pane_GeotermiaWmC_19',
           radius: 10.0,
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
       if (feature.properties['heat_flow'] >= 76.000000 && feature.properties['heat_flow'] <= 91.993719 ) {
           return {
           pane: 'pane_GeotermiaWmC_19',
           radius: 13.0,
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
       if (feature.properties['heat_flow'] >= 91.993719 && feature.properties['heat_flow'] <= 1263.420000 ) {
           return {
           pane: 'pane_GeotermiaWmC_19',
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
   map.createPane('pane_GeotermiaWmC_19');
   map.getPane('pane_GeotermiaWmC_19').style.zIndex = 419;
   map.getPane('pane_GeotermiaWmC_19').style['mix-blend-mode'] = 'normal';
   var layer_GeotermiaWmC_19 = new L.geoJson(json_GeotermiaWmC_19, {
       attribution: '',
       interactive: true,
       dataVar: 'json_GeotermiaWmC_19',
       layerName: 'layer_GeotermiaWmC_19',
       pane: 'pane_GeotermiaWmC_19',
       onEachFeature: pop_GeotermiaWmC_19,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_GeotermiaWmC_19_0(feature));
       },
   });
   bounds_group.addLayer(layer_GeotermiaWmC_19);
   function pop_Cuencas_Disponibilidad_2023_20(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Clave de cuenca: </th>\
                   <td>' + (feature.properties['clvcuenca'] !== null ? autolinker.link(String(feature.properties['clvcuenca']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Cuenca: </th>\
                   <td>' + (feature.properties['cuenca'] !== null ? autolinker.link(String(feature.properties['cuenca']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fecha DOF: </th>\
                   <td>' + (feature.properties['fechadof'] !== null ? autolinker.link(String(feature.properties['fechadof']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clasificación: </th>\
                   <td>' + (feature.properties['clasificac'] !== null ? autolinker.link(String(feature.properties['clasificac']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Año: </th>\
                   <td>' + (feature.properties['anio'] !== null ? autolinker.link(String(feature.properties['anio']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Disponibilidad: </th>\
                   <td>' + (feature.properties['d_'] !== null ? autolinker.link(String(feature.properties['d_']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Cuencas_Disponibilidad_2023_20_0(feature) {
       if (feature.properties['d_'] >= -1635.390000 && feature.properties['d_'] <= -100.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(127,26,28,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= -100.000000 && feature.properties['d_'] <= -10.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(227,26,28,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= -10.000000 && feature.properties['d_'] <= -1.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(255,154,0,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= -1.000000 && feature.properties['d_'] <= 1.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(247,222,109,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= 1.000000 && feature.properties['d_'] <= 10.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(166,206,227,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= 10.000000 && feature.properties['d_'] <= 100.000000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(64,133,180,0.568)',
           interactive: true,
       }
       }
       if (feature.properties['d_'] >= 100.000000 && feature.properties['d_'] <= 12199.910000 ) {
           return {
           pane: 'pane_Cuencas_Disponibilidad_2023_20',
           opacity: 1,
           color: 'rgba(0,0,0,0.568)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(13,74,155,0.568)',
           interactive: true,
       }
       }
   }
   map.createPane('pane_Cuencas_Disponibilidad_2023_20');
   map.getPane('pane_Cuencas_Disponibilidad_2023_20').style.zIndex = 420;
   map.getPane('pane_Cuencas_Disponibilidad_2023_20').style['mix-blend-mode'] = 'normal';
   var layer_Cuencas_Disponibilidad_2023_20 = new L.geoJson(json_Cuencas_Disponibilidad_2023_20, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Cuencas_Disponibilidad_2023_20',
       layerName: 'layer_Cuencas_Disponibilidad_2023_20',
       pane: 'pane_Cuencas_Disponibilidad_2023_20',
       onEachFeature: pop_Cuencas_Disponibilidad_2023_20,
       style: style_Cuencas_Disponibilidad_2023_20_0,
   });
   bounds_group.addLayer(layer_Cuencas_Disponibilidad_2023_20);
   map.createPane('pane_RadiacinsolarkWhmda_21');
   map.getPane('pane_RadiacinsolarkWhmda_21').style.zIndex = 421;
   var img_RadiacinsolarkWhmda_21 = 'data/RadiacinsolarkWhmda_21.png';
   var img_bounds_RadiacinsolarkWhmda_21 = [[14.59,-117.12],[32.71,-86.76]];
   var layer_RadiacinsolarkWhmda_21 = new L.imageOverlay(img_RadiacinsolarkWhmda_21,
                                         img_bounds_RadiacinsolarkWhmda_21,
                                         {pane: 'pane_RadiacinsolarkWhmda_21'});
   bounds_group.addLayer(layer_RadiacinsolarkWhmda_21);
   map.createPane('pane_Velocidaddevientoms_22');
   map.getPane('pane_Velocidaddevientoms_22').style.zIndex = 422;
   var img_Velocidaddevientoms_22 = 'data/Velocidaddevientoms_22.png';
   var img_bounds_Velocidaddevientoms_22 = [[14.53455103699999,-118.357159072],[32.707789712,-86.729280709]];
   var layer_Velocidaddevientoms_22 = new L.imageOverlay(img_Velocidaddevientoms_22,
                                         img_bounds_Velocidaddevientoms_22,
                                         {pane: 'pane_Velocidaddevientoms_22'});
   bounds_group.addLayer(layer_Velocidaddevientoms_22);
   function pop_GerenciadeControlRegional_23(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Gerencia de Control: </th>\
                   <td>' + (feature.properties['region'] !== null ? autolinker.link(String(feature.properties['region']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_GerenciadeControlRegional_23_0(feature) {
       switch(String(feature.properties['region'])) {
           case 'Regional Baja California':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(204,181,106,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Baja California Sur':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(227,30,230,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Central':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(97,192,213,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Noreste':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(37,213,31,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Noroeste':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(217,19,101,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Norte':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(207,62,30,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Occidental':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(70,213,153,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Oriental':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(97,124,218,0.566)',
           interactive: true,
       }
               break;
           case 'Regional Peninsular':
               return {
           pane: 'pane_GerenciadeControlRegional_23',
           opacity: 1,
           color: 'rgba(35,35,35,0.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(170,225,75,0.566)',
           interactive: true,
       }
               break;
       }
   }
   map.createPane('pane_GerenciadeControlRegional_23');
   map.getPane('pane_GerenciadeControlRegional_23').style.zIndex = 423;
   map.getPane('pane_GerenciadeControlRegional_23').style['mix-blend-mode'] = 'normal';
   var layer_GerenciadeControlRegional_23 = new L.geoJson(json_GerenciadeControlRegional_23, {
       attribution: '',
       interactive: true,
       dataVar: 'json_GerenciadeControlRegional_23',
       layerName: 'layer_GerenciadeControlRegional_23',
       pane: 'pane_GerenciadeControlRegional_23',
       onEachFeature: pop_GerenciadeControlRegional_23,
       style: style_GerenciadeControlRegional_23_0,
   });
   bounds_group.addLayer(layer_GerenciadeControlRegional_23);
   function pop_Municipios_24(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Clave geoestadistica: </th>\
                   <td>' + (feature.properties['CVEGEO'] !== null ? autolinker.link(String(feature.properties['CVEGEO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clave de entidad: </th>\
                   <td>' + (feature.properties['CVE_ENT'] !== null ? autolinker.link(String(feature.properties['CVE_ENT']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clave de municipio: </th>\
                   <td>' + (feature.properties['CVE_MUN'] !== null ? autolinker.link(String(feature.properties['CVE_MUN']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Municipio: </th>\
                   <td>' + (feature.properties['NOM_MUN'] !== null ? autolinker.link(String(feature.properties['NOM_MUN']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['NOMGEO'] !== null ? autolinker.link(String(feature.properties['NOMGEO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Municipios_24_0() {
       return {
           pane: 'pane_Municipios_24',
           opacity: 1,
           color: 'rgba(35,35,35,1.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(231,113,72,0.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Municipios_24');
   map.getPane('pane_Municipios_24').style.zIndex = 424;
   map.getPane('pane_Municipios_24').style['mix-blend-mode'] = 'normal';
   var layer_Municipios_24 = new L.geoJson(json_Municipios_24, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Municipios_24',
       layerName: 'layer_Municipios_24',
       pane: 'pane_Municipios_24',
       onEachFeature: pop_Municipios_24,
       style: style_Municipios_24_0,
   });
   bounds_group.addLayer(layer_Municipios_24);
   map.addLayer(layer_Municipios_24);
   function pop_Estados_25(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Clave geoestadistica: </th>\
                   <td>' + (feature.properties['CVEGEO'] !== null ? autolinker.link(String(feature.properties['CVEGEO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Clave de entidad: </th>\
                   <td>' + (feature.properties['CVE_ENT'] !== null ? autolinker.link(String(feature.properties['CVE_ENT']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Estado: </th>\
                   <td>' + (feature.properties['NOMGEO'] !== null ? autolinker.link(String(feature.properties['NOMGEO']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Estados_25_0() {
       return {
           pane: 'pane_Estados_25',
           opacity: 1,
           color: 'rgba(227,26,28,1.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(232,113,141,0.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Estados_25');
   map.getPane('pane_Estados_25').style.zIndex = 425;
   map.getPane('pane_Estados_25').style['mix-blend-mode'] = 'normal';
   var layer_Estados_25 = new L.geoJson(json_Estados_25, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Estados_25',
       layerName: 'layer_Estados_25',
       pane: 'pane_Estados_25',
       onEachFeature: pop_Estados_25,
       style: style_Estados_25_0,
   });
   bounds_group.addLayer(layer_Estados_25);
   map.addLayer(layer_Estados_25);
   function pop_subestaciones_4326_26(feature, layer) {
       var popupContent = '<table>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_subestaciones_4326_26_0() {
       return {
           pane: 'pane_subestaciones_4326_26',
           opacity: 1,
           color: 'rgba(35,35,35,1.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 4.0, 
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(0,0,0,1.0)',
           interactive: false,
       }
   }
   map.createPane('pane_subestaciones_4326_26');
   map.getPane('pane_subestaciones_4326_26').style.zIndex = 426;
   map.getPane('pane_subestaciones_4326_26').style['mix-blend-mode'] = 'normal';
   var layer_subestaciones_4326_26 = new L.geoJson(json_subestaciones_4326_26, {
       attribution: '',
       interactive: false,
       dataVar: 'json_subestaciones_4326_26',
       layerName: 'layer_subestaciones_4326_26',
       pane: 'pane_subestaciones_4326_26',
       onEachFeature: pop_subestaciones_4326_26,
       style: style_subestaciones_4326_26_0,
   });
   bounds_group.addLayer(layer_subestaciones_4326_26);
   function pop_Lneasdetransmisinelctrica_27(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <td colspan="2">' + (feature.properties['fid'] !== null ? autolinker.link(String(feature.properties['fid']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['id'] !== null ? autolinker.link(String(feature.properties['id']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['nom_obj'] !== null ? autolinker.link(String(feature.properties['nom_obj']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['codigo'] !== null ? autolinker.link(String(feature.properties['codigo']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['calif_pos'] !== null ? autolinker.link(String(feature.properties['calif_pos']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['condicion'] !== null ? autolinker.link(String(feature.properties['condicion']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['tipo'] !== null ? autolinker.link(String(feature.properties['tipo']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <td colspan="2">' + (feature.properties['clase_geo'] !== null ? autolinker.link(String(feature.properties['clase_geo']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Lneasdetransmisinelctrica_27_0() {
       return {
           pane: 'pane_Lneasdetransmisinelctrica_27',
           opacity: 1,
           color: 'rgba(0,0,0,1.0)',
           dashArray: '',
           lineCap: 'square',
           lineJoin: 'bevel',
           weight: 1.0,
           fillOpacity: 0,
           interactive: false,
       }
   }
   map.createPane('pane_Lneasdetransmisinelctrica_27');
   map.getPane('pane_Lneasdetransmisinelctrica_27').style.zIndex = 427;
   map.getPane('pane_Lneasdetransmisinelctrica_27').style['mix-blend-mode'] = 'normal';
   var layer_Lneasdetransmisinelctrica_27 = new L.geoJson(json_Lneasdetransmisinelctrica_27, {
       attribution: '',
       interactive: false,
       dataVar: 'json_Lneasdetransmisinelctrica_27',
       layerName: 'layer_Lneasdetransmisinelctrica_27',
       pane: 'pane_Lneasdetransmisinelctrica_27',
       onEachFeature: pop_Lneasdetransmisinelctrica_27,
       style: style_Lneasdetransmisinelctrica_27_0,
   });
   bounds_group.addLayer(layer_Lneasdetransmisinelctrica_27);
   function pop_Subestacioneselctricas_28(feature, layer) {
       var popupContent = '<table>\
               <tr>\
                   <th scope="row">Nombre: </th>\
                   <td>' + (feature.properties['Name'] !== null ? autolinker.link(String(feature.properties['Name']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Fase: </th>\
                   <td>' + (feature.properties['Fase'] !== null ? autolinker.link(String(feature.properties['Fase']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
               <tr>\
                   <th scope="row">Voltaje(KV): </th>\
                   <td>' + (feature.properties['Voltaje_KV'] !== null ? autolinker.link(String(feature.properties['Voltaje_KV']).replace(/'/g, '\'').replace(/"/g, '&quot;').toLocaleString()) : '') + '</td>\
               </tr>\
           </table>';
       var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
           addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
   }

   function style_Subestacioneselctricas_28_0() {
       return {
           pane: 'pane_Subestacioneselctricas_28',
           radius: 4.4,
           opacity: 1,
           color: 'rgba(255,255,255,1.0)',
           dashArray: '',
           lineCap: 'butt',
           lineJoin: 'miter',
           weight: 1.0,
           fill: true,
           fillOpacity: 1,
           fillColor: 'rgba(0,0,0,1.0)',
           interactive: true,
       }
   }
   map.createPane('pane_Subestacioneselctricas_28');
   map.getPane('pane_Subestacioneselctricas_28').style.zIndex = 428;
   map.getPane('pane_Subestacioneselctricas_28').style['mix-blend-mode'] = 'normal';
   var layer_Subestacioneselctricas_28 = new L.geoJson(json_Subestacioneselctricas_28, {
       attribution: '',
       interactive: true,
       dataVar: 'json_Subestacioneselctricas_28',
       layerName: 'layer_Subestacioneselctricas_28',
       pane: 'pane_Subestacioneselctricas_28',
       onEachFeature: pop_Subestacioneselctricas_28,
       pointToLayer: function (feature, latlng) {
           var context = {
               feature: feature,
               variables: {}
           };
           return L.circleMarker(latlng, style_Subestacioneselctricas_28_0(feature));
       },
   });
   bounds_group.addLayer(layer_Subestacioneselctricas_28);
   var overlaysTree = [
   {label: '<b>Infraestructura eléctrica</b>', selectAllCheckbox: true, children: [
       {label: '<img src="/Geovisualizador/legend/Subestacioneselctricas_28.png" /> Subestaciones eléctricas', layer: layer_Subestacioneselctricas_28},
       {label: '<img src="/Geovisualizador/legend/Lneasdetransmisinelctrica_27.png" /> Líneas de transmisión eléctrica', layer: layer_Lneasdetransmisinelctrica_27},
       {label: '<img src="/Geovisualizador/legend/subestaciones_4326_26.png" /> subestaciones_4326', layer: layer_subestaciones_4326_26},]},
   {label: '<b>División política</b>', selectAllCheckbox: true, children: [
       {label: '<img src="/Geovisualizador/legend/Estados_25.png" /> Estados', layer: layer_Estados_25},
       {label: '<img src="/Geovisualizador/legend/Municipios_24.png" /> Municipios', layer: layer_Municipios_24},
       {label: 'Gerencia de Control Regional<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalBajaCalifornia0.png" /></td><td>Regional Baja California</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalBajaCaliforniaSur1.png" /></td><td>Regional Baja California Sur</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalCentral2.png" /></td><td>Regional Central</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalNoreste3.png" /></td><td>Regional Noreste</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalNoroeste4.png" /></td><td>Regional Noroeste</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalNorte5.png" /></td><td>Regional Norte</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalOccidental6.png" /></td><td>Regional Occidental</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalOriental7.png" /></td><td>Regional Oriental</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GerenciadeControlRegional_23_RegionalPeninsular8.png" /></td><td>Regional Peninsular</td></tr></table>', layer: layer_GerenciadeControlRegional_23},]},
   {label: '<b>Zonas con alta calidad</b>', selectAllCheckbox: true, children: [
       {label: "Velocidad de viento (m/s)", layer: layer_Velocidaddevientoms_22},
       {label: "Radiación solar (kWh/m²/día)", layer: layer_RadiacinsolarkWhmda_21},
       {label: 'Cuencas_Disponibilidad_2023<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Déficitdemásde100hm³año0.png" /></td><td>Déficit de más de 100hm³/año</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Déficitdeentre10y100hm³año1.png" /></td><td>Déficit de entre 10 y 100hm³/año</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Déficitdedeentre1y10hm³año2.png" /></td><td>Déficit de de entre 1 y 10hm³/año</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Neutroentre1hm³añoy1hm³año3.png" /></td><td>Neutro (entre -1hm³/año y 1hm³/año)</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Disponibilidadentre1y10hm³año4.png" /></td><td>Disponibilidad entre 1 y 10hm³/año</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Disponibilidadentre10y100hm³año5.png" /></td><td>Disponibilidad entre 10 y 100hm³/año</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/Cuencas_Disponibilidad_2023_20_Disponibilidaddemásde100hm³año6.png" /></td><td>Disponibilidad de más de 100hm³/año</td></tr></table>', layer: layer_Cuencas_Disponibilidad_2023_20},
       {label: 'Geotermia (W/m°C)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GeotermiaWmC_19_4540.png" /></td><td>4 - 54</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GeotermiaWmC_19_54661.png" /></td><td>54 - 66</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GeotermiaWmC_19_66762.png" /></td><td>66 - 76</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GeotermiaWmC_19_76923.png" /></td><td>76 - 92</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/GeotermiaWmC_19_9212634.png" /></td><td>92 - 1263</td></tr></table>', layer: layer_GeotermiaWmC_19},
       {label: '<img src="/Geovisualizador/legend/PotencialGeotrmico_18.png" /> Potencial Geotérmico', layer: layer_PotencialGeotrmico_18},
       {label: '<img src="/Geovisualizador/legend/PotencialresiduosUrbanos_17.png" /> Potencial residuos Urbanos', layer: layer_PotencialresiduosUrbanos_17},
       {label: '<img src="/Geovisualizador/legend/PotencialresiduosPecuarios_16.png" /> Potencial residuos Pecuarios', layer: layer_PotencialresiduosPecuarios_16},
       {label: '<img src="/Geovisualizador/legend/PotencialresiduosIndustriales_15.png" /> Potencial residuos Industriales', layer: layer_PotencialresiduosIndustriales_15},
       {label: '<img src="/Geovisualizador/legend/PotencialresiduosForestales_14.png" /> Potencial residuos Forestales', layer: layer_PotencialresiduosForestales_14},
       {label: 'Potencial Eólico (m/s)<br /><table><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_60.png" /></td><td>6</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_651.png" /></td><td>6.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_72.png" /></td><td>7</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_753.png" /></td><td>7.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_84.png" /></td><td>8</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_855.png" /></td><td>8.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_96.png" /></td><td>9</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_957.png" /></td><td>9.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_108.png" /></td><td>10</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_1059.png" /></td><td>10.5</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_1110.png" /></td><td>11</td></tr><tr><td style="text-align: center;"><img src="/Geovisualizador/legend/PotencialElicoms_13_11511.png" /></td><td>11.5</td></tr></table>', layer: layer_PotencialElicoms_13},]},
   {label: '<b>Mapas utilizados para la evaluación</b>', selectAllCheckbox: true, children: [
       {label: '<img src="/Geovisualizador/legend/Zonamonumentoshistricos_12.png" /> Zona monumentos históricos', layer: layer_Zonamonumentoshistricos_12},
       {label: '<img src="/Geovisualizador/legend/Zonamonumentosarqueolgicos_11.png" /> Zona monumentos arqueológicos', layer: layer_Zonamonumentosarqueolgicos_11},
       {label: '<img src="/Geovisualizador/legend/Localidadesurbanas_10.png" /> Localidades urbanas', layer: layer_Localidadesurbanas_10},
       {label: '<img src="/Geovisualizador/legend/Localidadesrurales_9.png" /> Localidades rurales', layer: layer_Localidadesrurales_9},
       {label: '<img src="/Geovisualizador/legend/ZonaslejanasalaRNT_8.png" /> Zonas lejanas a la RNT', layer: layer_ZonaslejanasalaRNT_8},
       {label: '<img src="/Geovisualizador/legend/Vasfrreas_7.png" /> Vías férreas', layer: layer_Vasfrreas_7},
       {label: '<img src="/Geovisualizador/legend/Rosprincipales_6.png" /> Ríos principales', layer: layer_Rosprincipales_6},
       {label: '<img src="/Geovisualizador/legend/Aerodromos_5.png" /> Aerodromos', layer: layer_Aerodromos_5},
       {label: '<img src="/Geovisualizador/legend/Volcanesactivos_4.png" /> Volcanes activos', layer: layer_Volcanesactivos_4},
       {label: '<img src="/Geovisualizador/legend/Altaincidenciaciclones_3.png" /> Alta incidencia ciclones', layer: layer_Altaincidenciaciclones_3},
       {label: '<img src="/Geovisualizador/legend/reasnaturalesprotegidas_2.png" /> Áreas naturales protegidas', layer: layer_reasnaturalesprotegidas_2},
       {label: '<img src="/Geovisualizador/legend/RAMSAR_1.png" /> RAMSAR', layer: layer_RAMSAR_1},]}
       //{label: "Positron", layer: layer_Positron_0},
       ]
   var lay = L.control.layers.tree(null, overlaysTree,{
       //namedToggle: true,
       //selectorBack: false,
       //closedSymbol: '&#8862; &#x1f5c0;',
       //openedSymbol: '&#8863; &#x1f5c1;',
       //collapseAll: 'Collapse all',
       //expandAll: 'Expand all',
       collapsed: true,
   });
   lay.addTo(map);
   setBounds();
   L.ImageOverlay.include({
       getBounds: function () {
           return this._bounds;
       }
   });
