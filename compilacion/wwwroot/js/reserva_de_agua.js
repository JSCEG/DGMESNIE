// Aplicar la configuración a cada mapa en el arreglo
mapas.forEach((mapa) => {
  // Configuraciones específicas para cada mapa
  if (mapa._container.id === "map") {
    // Configuraciones específicas para el mapa con id 'map'
///////////////////////////////////////////////////////////////
    var highlightLayer;
    function highlightFeature(e) {
      highlightLayer = e.target;

      if (e.target.feature.geometry.type === "LineString") {
        highlightLayer.setStyle({
          color: "#rgb(1,208,251,1)",
        });
      } else {
        highlightLayer.setStyle({
          fillColor: "rgb(1,208,251,0.3)",
          fillOpacity: 1,
        });
      }
    }
    function setBounds() {}
        // remove popup's row if "visible-with-data"
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
         }).addTo(mapa);
         var bounds_group = new L.featureGroup([]);
         function setBounds() {
         }
         function pop_Cuencas_Disponibilidad_2023copiar_0(feature, layer) {
             var popupContent = '<table>\
                      <tr>\
                         <th scope="row">Clave de cuenca: </th>\
                         <td>' + (feature.properties['clvcuenca'] !== null ? autolinker.link(feature.properties['clvcuenca'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Nombre de cuenca: </th>\
                         <td>' + (feature.properties['cuenca'] !== null ? autolinker.link(feature.properties['cuenca'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Fecha en el DOF: </th>\
                         <td>' + (feature.properties['fechadof'] !== null ? autolinker.link(feature.properties['fechadof'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Clasificación: </th>\
                         <td>' + (feature.properties['clasificac'] !== null ? autolinker.link(feature.properties['clasificac'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Año: </th>\
                         <td>' + (feature.properties['anio'] !== null ? autolinker.link(feature.properties['anio'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Volumen de extracción(hm³/año): </th>\
                         <td>' + (feature.properties['uc_'] !== null ? autolinker.link(feature.properties['uc_'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Volumen de escurrimiento aguas abajo(hm³/año): </th>\
                         <td>' + (feature.properties['ab_'] !== null ? autolinker.link(feature.properties['ab_'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Volumen comprometido aguas abajo(hm³/año): </th>\
                         <td>' + (feature.properties['rxy_'] !== null ? autolinker.link(feature.properties['rxy_'].toLocaleString()) : '') + '</td>\
                     </tr>\
                     <tr>\
                         <th scope="row">Disponibilidad: </th>\
                         <td>' + (feature.properties['d_'] !== null ? autolinker.link(feature.properties['d_'].toLocaleString()) : '') + '</td>\
                     </tr>\
                 </table>';
             var content = removeEmptyRowsFromPopupContent(popupContent, feature);
       layer.on('popupopen', function(e) {
         addClassToPopupIfMedia(content, e.popup);
       });
       layer.bindPopup(content, { maxHeight: 400 });
         }
 
         function style_Cuencas_Disponibilidad_2023copiar_0_0(feature) {
             if (feature.properties['d_'] >= -1635.390000 && feature.properties['d_'] <= -100.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(127,26,28,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= -100.000000 && feature.properties['d_'] <= -10.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(227,26,28,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= -10.000000 && feature.properties['d_'] <= -1.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(255,154,0,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= -1.000000 && feature.properties['d_'] <= 1.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(247,222,109,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= 1.000000 && feature.properties['d_'] <= 10.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(166,206,227,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= 10.000000 && feature.properties['d_'] <= 100.000000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(64,133,180,0.45)',
                 interactive: true,
             }
             }
             if (feature.properties['d_'] >= 100.000000 && feature.properties['d_'] <= 12199.910000 ) {
                 return {
                 pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
                 opacity: 1,
                 color: 'rgba(0,0,0,0.45)',
                 dashArray: '',
                 lineCap: 'butt',
                 lineJoin: 'miter',
                 weight: 1, 
                 fill: true,
                 fillOpacity: 1,
                 fillColor: 'rgba(13,74,155,0.45)',
                 interactive: true,
             }
             }
         }
         mapa.createPane('pane_Cuencas_Disponibilidad_2023copiar_0');
         mapa.getPane('pane_Cuencas_Disponibilidad_2023copiar_0').style.zIndex = 400;
         mapa.getPane('pane_Cuencas_Disponibilidad_2023copiar_0').style['mix-blend-mode'] = 'normal';
         var layer_Cuencas_Disponibilidad_2023copiar_0 = new L.geoJson(json_Cuencas_Disponibilidad_2023copiar_0, {
             attribution: '',
             interactive: true,
             dataVar: 'json_Cuencas_Disponibilidad_2023copiar_0',
             layerName: 'layer_Cuencas_Disponibilidad_2023copiar_0',
             pane: 'pane_Cuencas_Disponibilidad_2023copiar_0',
             onEachFeature: pop_Cuencas_Disponibilidad_2023copiar_0,
             style: style_Cuencas_Disponibilidad_2023copiar_0_0,
         });
         bounds_group.addLayer(layer_Cuencas_Disponibilidad_2023copiar_0);
         mapa.addLayer(layer_Cuencas_Disponibilidad_2023copiar_0);


//////////////////////////////////////////


    setBounds();
  } else if (mapa._container.id === "map_presionagua") {
  
  
  
    // Configuraciones específicas para el mapa con id 'mapa'
    mapa.on('click', function (e) {
      let lat = e.latlng.lat.toPrecision(8);
      let lon = e.latlng.lng.toPrecision(8);
      const point = L.marker([lat, lon]).addTo(mapa)
          .bindPopup('<a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + lat + ',' + lon + '&cbp=11,0,0,0,0" target="blank"><b> Ver vista de calle </b></a>').openPopup();
    });
    function getColorPresion(d) {
      return d > 100 ? "#FF0000" : "#008000"; // Rojo para valores mayores a 100, verde para valores menores o iguales a 100
    }
    var info = L.control();
    info.onAdd = function (mapa) {
      this._div = L.DomUtil.create("div", "info");
      this.update();
      return this._div;
    };
    info.update = function (props) {
      this._div.innerHTML =
        "<h4>Presión de Agua: </h4>" +
        (props
          ? "<b>" +
            props.NOMGEO +
            "</b><br />" +
            getPresionLabel(props[selectedYear])
          : "Selecciona un estado");
    };
    function getPresionLabel(value) {
      return value > 100 ? "Mayor a 100" : "Menor a 100";
    }
    info.addTo(mapa);
    var legend = L.control({ position: "bottomright" });
    legend.onAdd = function (mapa) {
      var div = L.DomUtil.create("div", "info legend");
      var labels = ["Menor 100", "Mayor a 100"];
      var colors = ["#008000", "#FF0000"];

      for (var i = 0; i < labels.length; i++) {
        div.innerHTML +=
          "<div><span>" +
          labels[i] +
          '</span><i style="background:' +
          colors[i] +
          '"></i></div>';
      }

      return div;
    };
    legend.addTo(mapa);
    var geojsonPresion;





    // Actualiza el año seleccionado
    var selectedYear = "2007"; // Puedes cambiarlo al año inicial que desees

    function updateGeoJSON() {
      geojsonPresion.setStyle(function (feature) {
        return {
          fillColor: getColorPresion(feature.properties[selectedYear]),
          weight: 1,
          opacity: 1,
          color: "white",
          dashArray: "3",
          fillOpacity: 0.3,
        };
      });
    }

    // Crea la capa GeoJSON con un estilo inicial
    geojsonPresion = L.geoJson(estados_presion, {
      style: function (feature) {
        return {
          fillColor: getColorPresion(feature.properties[selectedYear]),
          weight: 1,
          opacity: 1,
          color: "white",
          dashArray: "3",
          fillOpacity: 0.3,
        };
      },
      onEachFeature: onEachFeaturePresion,
    }).addTo(mapa);

    // Obtén las propiedades del primer feature para obtener la lista de campos disponibles
    var featurePropertiesPresion = estados_presion.features[0].properties;
    var campoSelector = document.getElementById("campo-selector");

    // Almacena los campos únicos para evitar repeticiones
    var camposUnicos = [];

    // Agrega dinámicamente las opciones del selector basadas en las propiedades del feature
    for (var campo in featurePropertiesPresion) {
      if (
        featurePropertiesPresion.hasOwnProperty(campo) &&
        campo.match(/^\d{4}$/) &&
        !camposUnicos.includes(campo)
      ) {
        camposUnicos.push(campo);
        var option = document.createElement("option");
        option.value = campo;
        option.text = campo;
        campoSelector.add(option);
      }
    }

    // Agrega un evento para manejar cambios en la selección del campo
    campoSelector.addEventListener("change", function () {
      // Obtén el valor seleccionado
      selectedYear = this.value;

      // Actualiza la información y el estilo del GeoJSON con el nuevo año
      info.update();
      updateGeoJSON();
    });

    function onEachFeaturePresion(feature, layer) {
      layer.on({
        mouseover: highlightFeaturePresion,
        mouseout: resetHighlightPresion,
      });
    }
    function highlightFeaturePresion(e) {
      var layer = e.target;

      layer.setStyle({
        weight: 2,
        color: "#666",
        dashArray: "",
        fillOpacity: 0.6,
      });

      if (!L.Browser.ie && !L.Browser.opera && !L.Browser.edge) {
        layer.bringToFront();
      }

      info.update(layer.feature.properties);
    }
    function resetHighlightPresion(e) {
      geojsonPresion.resetStyle(e.target);
      info.update();
    }
  } else if (mapa._container.id === "mapa_estres_h") {
    // Configuraciones específicas para el mapa con id 'mapaestresh'
    mapa.on('click', function (e) {
      let lat = e.latlng.lat.toPrecision(8);
      let lon = e.latlng.lng.toPrecision(8);
      const point = L.marker([lat, lon]).addTo(mapa)
          .bindPopup('<a class="street-view-link btn btn-cre-verde" href="http://maps.google.com/maps?q=&layer=c&cbll=' + lat + ',' + lon + '&cbp=11,0,0,0,0" target="blank"><b> Ver vista de calle </b></a>').openPopup();
    });
    function getColor(d) {
      if (d < 0.09) {
        return "#008000"; // Verde (Bajo)
      } else if (d >= 0.09 && d <= 0.5) {
        return "#FFFF00"; // Amarillo (Medio)
      } else if (d > 0.5 && d <= 0.91) {
        return "#FFA500"; // Anaranjado (Alto)
      } else if (d > 0.91 && d <= 1) {
        return "#FF0000"; // Rojo (Muy alto)
      } else {
        return "#FFEDA0"; // Valor por defecto (si no coincide con ninguna categoría)
      }
    }

    var info = L.control();

    info.onAdd = function (mapa) {
      this._div = L.DomUtil.create("div", "info");
      this.update();
      return this._div;
    };

    info.update = function (props) {
      this._div.innerHTML =
        "<h4>Calificación: </h4>" +
        (props
          ? "<b>" + props.NOMGEO + "</b><br />" + getStatusLabel(props["2007"]) // Actualiza con el año inicial
          : "Selecciona un estado");
    };

    function getStatusLabel(value) {
      if (value < 0.09) {
        return "Bajo";
      } else if (value >= 0.09 && value <= 0.5) {
        return "Medio";
      } else if (value > 0.5 && value <= 0.91) {
        return "Alto";
      } else if (value > 0.91 && value <= 1) {
        return "Muy Alto";
      } else {
        return "";
      }
    }

    info.addTo(mapa);

    var legend = L.control({ position: "bottomright" });

    legend.onAdd = function (mapa) {
      var div = L.DomUtil.create("div", "info legend");
      var grades = [0, 0.09, 0.5, 0.91, 1];
      var labels = ["Bajo", "Medio", "Alto", "Muy Alto"];

      for (var i = 0; i < grades.length - 1; i++) {
        div.innerHTML +=
          "<div><span>" +
          labels[i] +
          '</span><i style="background:' +
          getColor((grades[i] + grades[i + 1]) / 2) +
          '"></i></div>';
      }

      return div;
    };

    legend.addTo(mapa);

    var geojson = L.geoJson(estados_WSI, {
      style: function (feature) {
        return {
          fillColor: getColor(feature.properties["2007"]),
          weight: 1,
          opacity: 1,
          color: "white",
          dashArray: "3",
          fillOpacity: 0.3,
        };
      },
      onEachFeature: onEachFeature,
    }).addTo(mapa);

    function onEachFeature(feature, layer) {
      layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
      });
    }

    function highlightFeature(e) {
      var layer = e.target;

      layer.setStyle({
        weight: 2,
        color: "#666",
        dashArray: "",
        fillOpacity: 0.6,
      });

      if (!L.Browser.ie && !L.Browser.opera && !L.Browser.edge) {
        layer.bringToFront();
      }

      info.update(layer.feature.properties);
    }

    function resetHighlight(e) {
      geojson.resetStyle(e.target);
      info.update();
    }

    // Obtén las propiedades del primer feature para obtener la lista de campos disponibles
    var featureProperties = estados_WSI.features[0].properties;
    var campoSelector = document.getElementById("campo-selectorwsi");

    // Almacena los campos únicos para evitar repeticiones
    var camposUnicos = [];

    // Agrega dinámicamente las opciones del selector basadas en las propiedades del feature
    for (var campo in featureProperties) {
      if (
        featureProperties.hasOwnProperty(campo) &&
        campo.match(/^\d{4}$/) &&
        !camposUnicos.includes(campo)
      ) {
        camposUnicos.push(campo);
        var option = document.createElement("option");
        option.value = campo;
        option.text = campo;
        campoSelector.add(option);
      }
    }

    // Agrega un evento para manejar cambios en la selección del campo
    campoSelector.addEventListener("change", function () {
      // Obtén el valor seleccionado
      var selectedField = this.value;

      // Actualiza el estilo del GeoJSON con el nuevo campo
      geojson.setStyle(function (feature) {
        return {
          fillColor: getColor(feature.properties[selectedField]),
          weight: 1,
          opacity: 1,
          color: "white",
          dashArray: "3",
          fillOpacity: 0.3,
        };
      });
    });
  }
});
