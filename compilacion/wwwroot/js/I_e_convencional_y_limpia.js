//Campos Visbles de los popup*@

//Busqueda
var availableTerms = []; //Variable global para almacenar los terminos de búqueda Sugerencia de Terminos
var datosExpendios = []; // variable global para almacenar todos los expendios de Petrolíferos
var camposVisiblesGlobal = [];
var datosExpendiosAcumulados = [];
let tpet = 0;
let tglp = 0;
let tgn = 0;
let te = 0;
var tpetg = [];
var tglpg = [];
var tgng = [];
let teg = [];

//Configuraciones del Mapa Inicial*@

var map = mapas[0];

// Crea los Iconos y Define su tamaño
var iconoBase = L.Icon.extend({
  options: {
    iconSize: [36, 36],
    iconAnchor: [18, 16],
    popupAnchor: [0, -26],
  },
});


var currentMarker = null; // Referencia al marcador actual
// var seleccionado = 'estado'; // Estado inicial
var municipiosFiltrados = null;

//Colores
var initialStyle = {
  color: "#187A8C", // Color de línea
  fillColor: "#187A8C", // Color de relleno
  fillOpacity: 0.3, // Opacidad del relleno
  weight: 3, // Ancho de la línea
};

// Estilo para el hover
var highlightStyle = {
  color: "#FFDB2EC",
  fillColor: "#FFDB2E", // Color de relleno
  fillOpacity: 0.3, // Opacidad del relleno
  weight: 3,
};

// Capa de estados
var estadosLayer = L.geoJSON(estados, {
  style: initialStyle, // Aplicar estilo inicial
  onEachFeature: function (feature, layer) {
    layer.bindTooltip(
      '<div class="custom-tooltip">' + feature.properties.NOMGEO + "</div>"
    );
    layer.on("click", function (e) {
      cargarMunicipios(feature.properties.CVE_ENT);
      map.fitBounds(layer.getBounds()); // Centra el mapa en el estado
    });
    // Efecto de hover
    layer.on("mouseover", function (e) {
      layer.setStyle(highlightStyle);
    });
    layer.on("mouseout", function (e) {
      estadosLayer.resetStyle(layer);
    });
  },
}).addTo(map);

// Capa de municipios (inicialmente vacía)
var municipiosLayer = L.geoJSON(null, {
  style: initialStyle, // Aplicar estilo inicial
  onEachFeature: onEachMunicipio,
}).addTo(map);

function onEachMunicipio(feature, layer) {
  layer.bindTooltip(
    '<div class="custom-tooltip">' +
    feature.properties.NOM_MUN +
    ", " +
    feature.properties.NOMGEO +
    "</div>"
  );
  // Efecto de hover
  layer.on("mouseover", function (e) {
    layer.setStyle(highlightStyle);
  });
  layer.on("mouseout", function (e) {
    municipiosLayer.resetStyle(layer);
  });
}

// Función para cargar los municipios correspondientes a un estado
function cargarMunicipios(cveEnt) {
  municipiosLayer.clearLayers();

  if (currentMarker) {
    map.removeLayer(currentMarker);
    currentMarker = null;
  }

  municipiosFiltrados = {
    type: "FeatureCollection",
    features: municipios_mapa.features.filter(function (feature) {
      return feature.properties.CVE_ENT === cveEnt;
    }),
  };

  municipiosLayer.addData(municipiosFiltrados);
}

//Búsquedas
//Busquedas
var lastSearchedEstadoLayer = null; // para almacenar la última entidad federativa buscada
var lastSearchedMunicipioLayer = null; // para almacenar el último municipio buscado

function buscarGeneral() {
  var terminoBuscado = document
    .getElementById("busquedaGeneralInput")
    .value.trim();

  if (!terminoBuscado) {
    alert("Por favor, introduce un término de búsqueda.");
    return; // Termina la ejecución de la función si el campo está vacío
  }

  // Intenta buscar por número de permiso primero
  var encontrado = false;
  for (var i = 0; i < datosExpendios.length; i++) {
    var expendio = datosExpendios[i];
    if (expendio.numeroPermiso === terminoBuscado) {
      var lat = expendio.latitudGeo;
      var lon = expendio.longitudGeo;
      map.setView([lat, lon], 17);
      encontrado = true;
      break;
    }
  }

  // Si no se encontró por número de permiso, busca por entidad federativa
  if (!encontrado) {
    estadosLayer.eachLayer(function (layer) {
      if (layer.feature.properties.NOMGEO === terminoBuscado) {
        map.fitBounds(layer.getBounds());

        // Si ya había una entidad federativa buscada anteriormente, restablecemos su estilo
        if (lastSearchedEstadoLayer) {
          estadosLayer.resetStyle(lastSearchedEstadoLayer);
        }

        // Resalta la entidad federativa encontrada
        layer.setStyle({
          color: "#FF0000",
          fillColor: "#FF0000",
          fillOpacity: 0.5,
        });

        lastSearchedEstadoLayer = layer;

        // Reiniciar el estilo de la entidad después de 5 segundos
        setTimeout(function () {
          estadosLayer.resetStyle(lastSearchedEstadoLayer);
          lastSearchedEstadoLayer = null;
        }, 5000);

        encontrado = true;
      }
    });
  }

  // Si aún no se encontró, busca por municipio en la fuente de datos completa
  if (!encontrado) {
    // Divide el término de búsqueda en municipio y estado
    var terminos = terminoBuscado.split(",");
    var buscadoMunicipio = terminos[0].trim();
    var buscadoEstado = terminos.length > 1 ? terminos[1].trim() : "";

    for (var i = 0; i < municipios_mapa.features.length; i++) {
      var municipio = municipios_mapa.features[i];
      var nombreMunicipio = municipio.properties.NOM_MUN;
      var nombreEstado = municipio.properties.NOMGEO;

      // Comprueba si el nombre del municipio y del estado coinciden con el término de búsqueda
      if (
        nombreMunicipio === buscadoMunicipio &&
        (buscadoEstado === "" || nombreEstado === buscadoEstado)
      ) {
        var bounds = L.geoJSON(municipio).getBounds();
        map.fitBounds(bounds);

        // Si ya había un municipio buscado anteriormente, lo elimina
        if (lastSearchedMunicipioLayer) {
          map.removeLayer(lastSearchedMunicipioLayer);
        }

        // Agrega el municipio encontrado al mapa y lo resalta
        lastSearchedMunicipioLayer = L.geoJSON(municipio, {
          style: {
            color: "#FF0000",
            fillColor: "#FF0000",
            fillOpacity: 0.5,
          },
        }).addTo(map);

        // Reiniciar el estilo y eliminar el municipio después de 5 segundos
        setTimeout(function () {
          map.removeLayer(lastSearchedMunicipioLayer);
          lastSearchedMunicipioLayer = null;
        }, 5000);

        encontrado = true;
        break;
      }
    }
  }

  if (!encontrado) {
    alert("Término no encontrado.");
  }
}

// Cuando se dibuja una línea, calcula la distancia
map.on("draw:created", function (e) {
  var type = e.layerType,
    layer = e.layer;

  if (type === "polyline") {
    var latlngs = layer.getLatLngs();
    var distance = 0;
    for (var i = 1; i < latlngs.length; i++) {
      distance += latlngs[i - 1].distanceTo(latlngs[i]);
    }
    // Convertir la distancia a km y redondear a 2 decimales
    distance = Math.round((distance / 1000) * 100) / 100;
    // Crear un popup con la distancia
    layer.bindPopup("Distancia: " + distance + " km").openPopup();
    // Añadir la línea al mapa
    layer.addTo(map);
  }
});

//Funciones de los botones y del Mapa*@

//Carga el shape de GCR
function setBounds() { }
function pop_regiones_poligon_0(feature, layer) {
  var popupContent =
    '<table>\
            <tr>\
                <th scope="row">Gerencia: </th>\
                <td>' +
    (feature.properties["region"] !== null
      ? autolinker.link(feature.properties["region"].toLocaleString())
      : "") +
    "</td>\
            </tr>\
        </table>";
  layer.bindPopup(popupContent, { maxHeight: 400 });
}

function style_regiones_poligon_0_0(feature) {
  switch (String(feature.properties["region"])) {
    case "Regional Baja California":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(104,207,143,0.65)",
        interactive: true,
      };
      break;
    case "Regional Baja California Sur":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(63,130,255,0.65)",
        interactive: true,
      };
      break;
    case "Regional Central":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(255,128,0,0.65)",
        interactive: true,
      };
      break;
    case "Regional Noreste":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(255,166,166,0.65)",
        interactive: true,
      };
      break;
    case "Regional Noroeste":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(109,183,226,0.65)",
        interactive: true,
      };
      break;
    case "Regional Norte":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(158,94,218,0.65)",
        interactive: true,
      };
      break;
    case "Regional Occidental":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(128,64,45,0.65)",
        interactive: true,
      };
      break;
    case "Regional Oriental":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(219,42,83,0.65)",
        interactive: true,
      };
      break;
    case "Regional Peninsular":
      return {
        pane: "pane_regiones_poligon_0",
        opacity: 1,
        color: "rgba(35,35,35,0.0)",
        dashArray: "",
        lineCap: "butt",
        lineJoin: "miter",
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: "rgba(255,244,29,0.65)",
        interactive: true,
      };
      break;
  }
}
map.createPane("pane_regiones_poligon_0");
map.getPane("pane_regiones_poligon_0").style.zIndex = 400;
map.getPane("pane_regiones_poligon_0").style["mix-blend-mode"] = "normal";
var layer_regiones_poligon_0 = new L.geoJson(json_regiones_poligon_0, {
  attribution: "",
  interactive: true,
  dataVar: "json_regiones_poligon_0",
  layerName: "layer_regiones_poligon_0",
  pane: "pane_regiones_poligon_0",
  onEachFeature: pop_regiones_poligon_0,
  style: style_regiones_poligon_0_0,
});
bounds_group.addLayer(layer_regiones_poligon_0);
map.addLayer(layer_regiones_poligon_0);
setBounds();

function limpiarMarcadores() {
  // Limpiar todas las capas de marcadores y círculos
  map.eachLayer(function (layer) {
    if (
      layer instanceof L.Marker ||
      layer instanceof L.MarkerClusterGroup ||
      layer instanceof L.Circle
    ) {
      map.removeLayer(layer);
    }
  });
}

// Funciones globales para las GCR
function isMarkerInPolygon(marker, polygon) {
  var latlng = L.latLng(marker.lat, marker.lng);
  return polygon.contains(latlng);
}

function countMarkersByRegion(markers, regions) {
  var counts = {};

  regions.features.forEach(function (region) {
    var regionName = region.properties.region;
    counts[regionName] = { Limpia: 0, Convencional: 0, Importación: 0 };

    markers.forEach(function (marker) {
      var polygon = L.geoJSON(region.geometry);
      if (isMarkerInPolygon(marker, polygon)) {
        counts[regionName][marker.fuenteEnergia]++;
      }
    });
  });

  return counts;
}

function calculatePercentages(counts) {
  var percentages = {};

  Object.keys(counts).forEach(function (region) {
    var total =
      counts[region].Limpia +
      counts[region].Convencional +
      counts[region].Importación;
    percentages[region] = {
      Limpia: (counts[region].Limpia / total) * 100,
      Convencional: (counts[region].Convencional / total) * 100,
      Importación: (counts[region].Importación / total) * 100,
    };
  });

  return percentages;
}

function renderChart(percentages) {
  var categories = Object.keys(percentages);
  var limpiaData = categories.map((region) => percentages[region].Limpia);
  var convencionalData = categories.map(
    (region) => percentages[region].Convencional
  );
  var importacionData = categories.map(
    (region) => percentages[region].Importación
  );

  Highcharts.chart("graficoPorcentajesGCR", {
    chart: { type: "column" },
    title: { text: "Distribución de Permisos por Región" },
    xAxis: { categories: categories },
    yAxis: {
      min: 0,
      title: { text: "Porcentaje (%)" },
      stackLabels: { enabled: true },
    },
    plotOptions: {
      column: { stacking: "percent" },
    },
    series: [
      { name: "Limpia", data: limpiaData, color: "#76c7c0" },
      { name: "Convencional", data: convencionalData, color: "#1a8092" },
      { name: "Importación", data: importacionData, color: "#ff7f50" },
    ],
  });
}

//Fin Funciones globales para las GCR

function handleNull(value) {
  return value ? value : "S/D-Sin Dato";
}
Highcharts.setOptions({
  lang: {
    decimalPoint: ".",
    thousandsSep: ",",
  },
});

//Electricidad
function CargaElectricidad() {
  // Reiniciando availableTerms y las demas variables
  availableTerms = [];
  datosExpendios = [];
  camposVisiblesGlobal = [];
  totalpermisos = 0;

  // Asignando a la búsqueda de términos
  estadosLayer.eachLayer(function (layer) {
    if (layer.feature.properties.NOMGEO) {
      // Asegúrate de que la propiedad existe
      availableTerms.push(layer.feature.properties.NOMGEO);
    }
  });

  for (var i = 0; i < municipios_mapa.features.length; i++) {
    var municipio = municipios_mapa.features[i].properties.NOM_MUN;
    var estado = municipios_mapa.features[i].properties.NOMGEO; // Asumiendo que esta es la propiedad correcta para el estado

    // Asegúrate de que ambas propiedades existen antes de concatenar
    if (municipio && estado) {
      var nombreCompleto = municipio + ", " + estado;
      availableTerms.push(nombreCompleto);
    }
  }

  // Primera llamada AJAX como una promesa
  function cargarCamposVisibles() {
    return new Promise((resolve, reject) => {
      $.ajax({
        url: "/Indicadores/GetCamposVisiblesElectricidad_Infra",
        type: "GET",
        contentType: "application/json",
        success: function (camposVisibles) {
          camposVisiblesGlobal = camposVisibles;
          resolve(camposVisibles); // Resuelve la promesa con los campos visibles
        },
        error: function (error) {
          console.error("Error al obtener campos visibles", error);
          reject(error); // Rechaza la promesa si hay un error
        },
      });
    });
  }
  //No hay ductos

  cargarCamposVisibles()
    .then((camposVisibles) => {
      // Limpiar marcadores existentes
      limpiarMarcadores();
      // Carga los Marcadores y Ductos

      $.ajax({
        url: "/Indicadores/GetExpendiosAutorizadosElectricidad_Infra",
        type: "GET",
        // data: JSON.stringify(datos_mun),
        contentType: "application/json",
        success: function (response) {
          console.log(
            "Estos son los Expendios Autorizados de Electricidad:",
            response
          ); // ver la respuesta en consola
          //Sin Filtrar los permisionarios Electricos
          datosExpendios = response; // Guarda la respuesta en la variable global
          // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
          // datosExpendios = response.filter(function(coordenada) {
          //     return coordenada.fuenteEnergia === "Convencional";
          // });

          for (var i = 0; i < datosExpendios.length; i++) {
            availableTerms.push(datosExpendios[i].numeroPermiso);
            datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
          }

          function generarContenidoPopup(coordenada) {
            var contenido =
              "<style>" +
              ".popup-content {" +
              "    width: 280px;" +
              "    max-height: 180px;" +
              "    overflow-y: auto;" +
              "    padding: 10px;" +
              "}" +
              "h2, h3, h4, p, li {" +
              "    margin: 0 0 10px 0;" +
              "}" +
              "ul {" +
              "    padding-left: 20px;" +
              "}" +
              "img {" +
              "    vertical-align: middle;" +
              "    margin-right: 10px;" +
              "}" +
              "</style>";

            contenido += "<div class='popup-content'>";

            if (camposVisiblesGlobal.includes("RazonSocial")) {
              var imgSrc; // La URL de la imagen que se mostrará en el pop-up
              switch (coordenada.fuenteEnergia) {
                case "Limpia":
                  imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/energia_limpia2.png";
                  break;
                case "Importación":
                  imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/importacion_e.png";
                  break;
                /*case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                default:
                  imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png"; // Ícono por defecto si no hay coincidencia
                  break;
              }
              contenido +=
                "<h2 class='subtitulo'><img src='" +
                imgSrc +
                "' style='height: 24px; width: 24px;'/><strong>" +
                handleNull(coordenada.razonSocial) +
                "</strong></h2><br>";
            }

            contenido += "<ul>";

            if (camposVisiblesGlobal.includes("EfId")) {
              //NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
              contenido +=
                "<li><strong>Clave y Entidad Federativa:</strong> " +
                handleNull(coordenada.efId) +
                "</li>";
            }
            if (camposVisiblesGlobal.includes("NumeroPermiso")) {
              contenido +=
                "<li><strong>NumeroPermiso:</strong> " +
                handleNull(coordenada.numeroPermiso) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("MpoId")) {
              contenido +=
                "<li><strong>Clave y Municipio:</strong> " +
                handleNull(coordenada.mpoId) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
              contenido +=
                "<li><strong>Numero de Expediente:</strong> " +
                handleNull(coordenada.numeroDeExpediente) +
                "</li>";
            }

            //if (camposVisiblesGlobal.includes("EfId")) {
            //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
            //}

            /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/

            if (camposVisiblesGlobal.includes("RazonSocial")) {
              contenido +=
                "<li><strong>RazonSocial:</strong> " +
                handleNull(coordenada.razonSocial) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
              contenido +=
                "<li><strong>Fecha de Otorgamiento:</strong> " +
                handleNull(coordenada.fechaOtorgamiento) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("LatitudGeo")) {
              contenido +=
                "<li><strong>LatitudGeo:</strong> " +
                handleNull(coordenada.latitudGeo) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("LongitudGeo")) {
              contenido +=
                "<li><strong>LongitudGeo:</strong> " +
                handleNull(coordenada.longitudGeo) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Dirección")) {
              contenido +=
                "<li><strong>Dirección:</strong> " +
                handleNull(coordenada.direccion) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Estatus")) {
              contenido +=
                "<li><strong>Estatus:</strong> " +
                handleNull(coordenada.estatus) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("RFC")) {
              contenido +=
                "<li><strong>Rfc:</strong> " +
                handleNull(coordenada.rfc) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("FechaRecepcion")) {
              contenido +=
                "<li><strong>Fecha de Recepción:</strong> " +
                handleNull(coordenada.fechaRecepcion) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
              contenido +=
                "<li><strong>Estatus de Instalación:</strong> " +
                handleNull(coordenada.estatusInstalacion) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("TipoPermiso")) {
              contenido +=
                "<li><strong>Tipo de Permiso:</strong> " +
                handleNull(coordenada.tipoPermiso) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("InicioVigencia")) {
              contenido +=
                "<li><strong>Inicio de Vigencia:</strong> " +
                handleNull(coordenada.inicioVigencia) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("InicioOperaciones")) {
              contenido +=
                "<li><strong>Inicio  de Operaciones:</strong> " +
                handleNull(coordenada.inicioOperaciones) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
              contenido +=
                "<li><strong>Capacidad Autorizada  en MW:</strong> " +
                handleNull(coordenada.capacidadAutorizadaMW) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
              contenido +=
                "<li><strong>Generación Estimada Anual:</strong> " +
                handleNull(coordenada.generacion_estimada_anual) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
              contenido +=
                "<li><strong>Inversión Estimada en mdls:</strong> " +
                handleNull(coordenada.inversion_estimada_mdls) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Energetico_primario")) {
              contenido +=
                "<li><strong>Energético Primario:</strong> " +
                handleNull(coordenada.energetico_primario) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Actividad_economica")) {
              contenido +=
                "<li><strong>Actividad Económica:</strong> " +
                handleNull(coordenada.actividad_economica) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Tecnología")) {
              contenido +=
                "<li><strong>Tecnología:</strong> " +
                handleNull(coordenada.tecnologia) +
                "</li>";
            }
            if (camposVisiblesGlobal.includes("EmpresaLíder")) {
              contenido +=
                "<li><strong>Empresa Líder:</strong> " +
                handleNull(coordenada.empresaLider) +
                "</li>";
            }
            if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
              contenido +=
                "<li><strong>País de Origen:</strong> " +
                handleNull(coordenada.paisDeOrigen) +
                "</li>";
            }
            if (camposVisiblesGlobal.includes("Subasta")) {
              contenido +=
                "<li><strong>Subasta:</strong> " +
                handleNull(coordenada.subasta) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Planta")) {
              contenido +=
                "<li><strong>Planta:</strong> " +
                handleNull(coordenada.planta) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Combustible")) {
              contenido +=
                "<li><strong>Combustible:</strong> " +
                handleNull(coordenada.combustible) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("FuenteEnergía")) {
              contenido +=
                "<li><strong>Fuente de Energía:</strong> " +
                handleNull(coordenada.fuenteEnergia) +
                "</li>";
            }

            if (camposVisiblesGlobal.includes("Comentarios")) {
              contenido +=
                "<li><strong>Comentarios:</strong> " +
                handleNull(coordenada.comentarios) +
                "</li>";
            }
            /*
                        if (camposVisiblesGlobal.includes("SiglasTipo")) {
                            contenido += "<li><strong>SiglasTipo:</strong> " + handleNull(coordenada.siglasTipo) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Otorgamiento")) {
                            contenido += "<li><strong>Otorgamiento:</strong> " + handleNull(coordenada.otorgamiento) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("FechaAcuse")) {
                            contenido += "<li><strong>FechaAcuse:</strong> " + handleNull(coordenada.fechaAcuse) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("EstatusSAT")) {
                            contenido += "<li><strong>EstatusSAT:</strong> " + handleNull(coordenada.estatusSAT) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuspensionInicio")) {
                            contenido += "<li><strong>SuspensionInicio:</strong> " + handleNull(coordenada.suspensionInicio) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuspensionFin")) {
                            contenido += "<li><strong>SuspensionFin:</strong> " + handleNull(coordenada.suspensionFin) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroTanques")) {
                            contenido += "<li><strong>NumeroTanques:</strong> " + handleNull(coordenada.numeroTanques) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("CapacidadLitros")) {
                            contenido += "<li><strong>CapacidadLitros:</strong> " + handleNull(coordenada.capacidadLitros) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroUnidades")) {
                            contenido += "<li><strong>NumeroUnidades:</strong> " + handleNull(coordenada.numeroUnidades) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroDeCentralesDeGuarda")) {
                            contenido += "<li><strong>NumeroDeCentralesDeGuarda:</strong> " + handleNull(coordenada.numeroDeCentralesDeGuarda) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("DomicilioDeGuarda")) {
                            contenido += "<li><strong>DomicilioDeGuarda:</strong> " + handleNull(coordenada.domicilioDeGuarda) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuministroRecepcion")) {
                            contenido += "<li><strong>SuministroRecepcion:</strong> " + handleNull(coordenada.suministroRecepcion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("PermisoSuministro")) {
                            contenido += "<li><strong>PermisoSuministro:</strong> " + handleNull(coordenada.permisoSuministro) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("CompartenTanques")) {
                            contenido += "<li><strong>CompartenTanques:</strong> " + handleNull(coordenada.compartenTanques) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Modificacion")) {
                            contenido += "<li><strong>Modificacion:</strong> " + handleNull(coordenada.modificacion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Asociacion")) {
                            contenido += "<li><strong>Asociacion:</strong> " + handleNull(coordenada.asociacion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Gie")) {
                            contenido += "<li><strong>Gie:</strong> " + handleNull(coordenada.gie) + "</li>";
                        }
                        */
            contenido += "</ul>";

            if (camposVisiblesGlobal.includes("NumeroPermiso")) {
              contenido +=
                "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" +
                coordenada.numeroPermiso +
                "'>Ver detalle</a>";
            }

            contenido +=
              "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" +
              coordenada.latitudGeo +
              "," +
              coordenada.longitudGeo +
              "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";

            contenido += "</div>";

            return contenido;
          }
          //Mapa de Resultados/////////////////////////////////////////////////////////////////////
          // Crea un grupo de marcadores
          var markers = L.markerClusterGroup();
          // Crea los iconos
          var iconoBase = L.Icon.extend({
            options: {
              iconSize: [36, 36],
              iconAnchor: [12, 18],
              popupAnchor: [0, -26],
            },
          });

          var iconoConvencional = new iconoBase({
            iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png",
          });
          var iconoLimpia = new iconoBase({
            iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/energia_limpia2.png",
          });

          var iconoImportacion = new iconoBase({
            iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/importacion_e.png",
          });
          /*     var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
          // Agrega los marcadores para las coordenadas del mapa actual
          for (var j = 0; j < response.length; j++) {
            var coordenada = response[j];
            //Determina el icono a Usar
            // Determinar el ícono basado en tipoPermiso

            //var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
            //var contenidoPopup = generarContenidoPopup(coordenada);
            //marker.bindPopup(contenidoPopup);
            //markers.addLayer(marker);
            //   if (coordenada.fuenteEnergia === "Convencional") {
            var iconoActual;
            // Aquí iría tu código para determinar el icono actual basado en tipoPermiso
            //
            switch (coordenada.fuenteEnergia) {
              case "Importación":
                iconoActual = iconoImportacion;
                break;
              /*  case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                                                                    iconoActual = iconoDistribucion;
                                                                                    break; */
              case "Limpia":
                iconoActual = iconoLimpia;
                break;
              default:
                iconoActual = iconoConvencional; // Ícono por defecto si no hay coincidencia
                break;
            }

            var marker = L.marker(
              [coordenada.latitudGeo, coordenada.longitudGeo],
              { icon: iconoActual }
            );
            var contenidoPopup = generarContenidoPopup(coordenada);
            marker.bindPopup(contenidoPopup);

            markers.addLayer(marker);
            //    }
          }

          map.addLayer(markers);

          //******Tarjetas Totales******///

          // Contar el número total de elementos en el array 'response'
          var totalElementos = datosExpendios.length;
          te = datosExpendios.length;
          //teg = response;
          // Formatear el número con separadores de miles
          var totalFormateado = totalElementos.toLocaleString();

          console.log(
            "Total de elementos en 'response' formateado:",
            totalFormateado
          );

          // Mostrar este total formateado en el elemento span
          var totalElectricidad = document.getElementById("total_electricidad");
          totalElectricidad.textContent =
            "Total de Permisos: " + totalFormateado;

          //Tarjetas de Totales
          // Calcular Totales a Nivel Nacional
          let totalPermisos = datosExpendios.length;
          let totalCapacidad = datosExpendios.reduce(
            (sum, item) => sum + (item.capacidadAutorizadaMW || 0),
            0
          );
          let totalGeneracion = datosExpendios.reduce(
            (sum, item) => sum + (item.generacionEstimadaAnual || 0),
            0
          );

          // Mostrar los totales en las tarjetas
          document.getElementById("totalPermisosAutorizados").textContent =
            totalPermisos.toLocaleString();
          document.getElementById("totalCapacidadAutorizada").textContent =
            totalCapacidad.toLocaleString() + " MW";
          document.getElementById("totalGeneracionEstimada").textContent =
            totalGeneracion.toLocaleString() + " MWh";

          //0.-Core Grafico
          // Función para procesar datos clasificados por tipo y región (detección automática de categorías)
          function procesarDatosPorTipoYRegion(shapeFile, marcadores) {
            const dataPorRegionYTipo = {};
            const categorias = new Set(); // Usaremos un Set para recolectar dinámicamente las categorías

            try {
              // Iterar sobre cada región en el shape file
              shapeFile.features.forEach((feature) => {
                const regionName = feature.properties.region || "Desconocida"; // Usar la propiedad 'region'
                if (!dataPorRegionYTipo[regionName]) {
                  dataPorRegionYTipo[regionName] = {}; // Inicializar la región
                }

                // Iterar sobre cada marcador
                marcadores.forEach((marcador) => {
                  if (marcador.longitudGeo && marcador.latitudGeo) {
                    const point = turf.point([
                      marcador.longitudGeo,
                      marcador.latitudGeo,
                    ]);

                    // Verificar si el marcador está dentro de la región
                    if (turf.booleanPointInPolygon(point, feature)) {
                      const tipo = marcador.fuenteEnergia || "Sin Clasificar"; // Categoría por defecto
                      categorias.add(tipo); // Agregar la categoría al Set

                      // Inicializar la categoría si no existe
                      if (!dataPorRegionYTipo[regionName][tipo]) {
                        dataPorRegionYTipo[regionName][tipo] = {
                          capacidad: 0,
                          generacion: 0,
                        };
                      }

                      // Sumar capacidad y generación
                      dataPorRegionYTipo[regionName][tipo].capacidad +=
                        marcador.capacidadAutorizadaMW || 0;
                      dataPorRegionYTipo[regionName][tipo].generacion +=
                        marcador.generacionEstimadaAnual || 0;
                    }
                  } else {
                    console.warn(
                      "Coordenadas inválidas para marcador:",
                      marcador
                    );
                  }
                });
              });
            } catch (error) {
              console.error(
                "Error al procesar datos por tipo y región:",
                error
              );
            }

            return { dataPorRegionYTipo, categorias: Array.from(categorias) }; // Convertimos el Set a un Array
          }

          // Procesar los datos clasificados
          const { dataPorRegionYTipo, categorias } =
            procesarDatosPorTipoYRegion(
              json_regiones_poligon_0,
              datosExpendios
            );

          const categoriesRegions = Object.keys(dataPorRegionYTipo);

          // Separar series para Capacidad y Generación
          const seriesCapacidad = categorias.map((tipo) => ({
            name: tipo,
            data: categoriesRegions.map(
              (region) => dataPorRegionYTipo[region][tipo]?.capacidad || 0
            ),
          }));

          const seriesGeneracion = categorias.map((tipo) => ({
            name: tipo,
            data: categoriesRegions.map(
              (region) => dataPorRegionYTipo[region][tipo]?.generacion || 0
            ),
          }));

          // 1. Gráfico de Capacidad
          Highcharts.chart("graficoCapacidad", {
            chart: {
              type: "column",
              backgroundColor: "#ffffff",
            },
            title: {
              text: "Capacidad Total Autorizada por Tipo de Energía y Región",
            },
            xAxis: {
              categories: categoriesRegions,
              title: {
                text: "Regiones",
              },
              labels: {
                rotation: -45,
              },
            },
            yAxis: {
              title: {
                text: "Capacidad Total (MW)",
              },
              labels: {
                format: "{value:,0f}",
              },
            },
            tooltip: {
              shared: true,
              pointFormat: "<b>{series.name}</b>: {point.y:,.2f} MW<br/>",
            },
            plotOptions: {
              column: {
                stacking: "normal",
                dataLabels: {
                  enabled: true,
                  format: "{point.y:,.2f}",
                },
              },
            },
            series: seriesCapacidad,
          });

          // 2. Gráfico de Generación
          Highcharts.chart("graficoGeneracion", {
            chart: {
              type: "column",
              backgroundColor: "#ffffff",
            },
            title: {
              text: "Generación Estimada Total por Tipo de Energía y Región",
            },
            xAxis: {
              categories: categoriesRegions,
              title: {
                text: "Regiones",
              },
              labels: {
                rotation: -45,
              },
            },
            yAxis: {
              title: {
                text: "Generación Total (MWh)",
              },
              labels: {
                format: "{value:,0f}",
              },
            },
            tooltip: {
              shared: true,
              pointFormat: "<b>{series.name}</b>: {point.y:,.2f} MWh<br/>",
            },
            plotOptions: {
              column: {
                stacking: "normal",
                dataLabels: {
                  enabled: true,
                  format: "{point.y:,.2f}",
                },
              },
            },
            series: seriesGeneracion,
          });

          //****************************************************************************************** */ Fin Grafico Gen y Capacidad Autorizada por EF

          //Mete el autocompetar ala busqueda
          // Función para inicializar el autocompletar
          function autocomplete(inp, arr) {
            var currentFocus;

            inp.addEventListener("input", function (e) {
              var a,
                b,
                i,
                val = this.value;
              closeAllLists();
              if (!val) {
                return false;
              }
              currentFocus = -1;

              a = document.getElementById("autocomplete-list");
              a.innerHTML = "";

              for (i = 0; i < arr.length; i++) {
                if (
                  arr[i].substr(0, val.length).toUpperCase() ===
                  val.toUpperCase()
                ) {
                  b = document.createElement("DIV");
                  b.innerHTML =
                    "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                  b.innerHTML += arr[i].substr(val.length);
                  b.addEventListener("click", function (e) {
                    inp.value = this.innerText;
                    closeAllLists();
                    buscarGeneral(); // Llama a tu función de búsqueda aquí
                  });
                  a.appendChild(b);
                }
              }
            });

            function closeAllLists(elmnt) {
              var x = document.getElementById("autocomplete-list");
              if (elmnt != x && elmnt != inp) {
                x.innerHTML = "";
              }
            }

            document.addEventListener("click", function (e) {
              closeAllLists(e.target);
            });
          }
          // Usamos un objeto Set para filtrar los duplicados, ya que un Set solo permite valores únicos
          var uniqueTerms = [...new Set(availableTerms)];

          console.log("av", availableTerms);

          // Inicializa el autocompletar
          autocomplete(
            document.getElementById("busquedaGeneralInput"),
            availableTerms
          );
        },
        error: function (error) {
          // Maneja el error si ocurre.
        },
      });
    })
    .catch((error) => {
      // Manejo de errores del primer AJAX
    });
}

CargaElectricidad();

// Funcionalidades de Búsqueda y Menú*@

// Función para inicializar el autocompletar
function autocomplete(inp, arr) {
  var currentFocus;

  inp.addEventListener("input", function (e) {
    var a,
      b,
      i,
      val = this.value;
    closeAllLists();
    if (!val) {
      return false;
    }
    currentFocus = -1;

    a = document.getElementById("autocomplete-list");
    a.innerHTML = "";

    for (i = 0; i < arr.length; i++) {
      if (arr[i].substr(0, val.length).toUpperCase() === val.toUpperCase()) {
        b = document.createElement("DIV");
        b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
        b.innerHTML += arr[i].substr(val.length);
        b.addEventListener("click", function (e) {
          inp.value = this.innerText;
          closeAllLists();
          buscarGeneral(); // Llama a tu función de búsqueda aquí
        });
        a.appendChild(b);
      }
    }
  });

  function closeAllLists(elmnt) {
    var x = document.getElementById("autocomplete-list");
    if (elmnt != x && elmnt != inp) {
      x.innerHTML = "";
    }
  }

  document.addEventListener("click", function (e) {
    closeAllLists(e.target);
  });
}
// Usamos un objeto Set para filtrar los duplicados, ya que un Set solo permite valores únicos
var uniqueTerms = [...new Set(availableTerms)];

console.log("av", availableTerms);

// Inicializa el autocompletar
autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);

//Links activos*@
function activarElemento(elementoID) {
  // Obtén todos los elementos de tu menú
  var elementos = document.querySelectorAll(".navbarmapag a");

  // Itera sobre ellos para eliminar la clase 'active'
  elementos.forEach(function (el) {
    el.classList.remove("active");
  });

  // Añade la clase 'active' al elemento clickeado
  var elementoActivo = document.getElementById(elementoID);
  if (elementoActivo) {
    elementoActivo.classList.add("active");
  }
}
