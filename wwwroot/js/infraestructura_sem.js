




//Campos Visbles de los popup*@
//Busqueda
var availableTerms = [];//Variable global para almacenar los terminos de búqueda Sugerencia de Terminos
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
///
var gpet = [];
var gglp = [];
var ggn = [];
var gel = [];


//Configuraciones del Mapa Inicial*@

var map = mapas[0];

// Crea los Iconos y Define su tamaño
var iconoBase = L.Icon.extend({
    options: {
        iconSize: [36, 36],
        iconAnchor: [18, 16],
        popupAnchor: [0, -26]
    }
});

// Asignación de Iconos
//var iconoSolicitudes = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Solicitudes.png' });
//iconoAprobado = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Aprobado.png' });
//iconoNoaprobado = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/NoAprobado.png' });


var currentMarker = null; // Referencia al marcador actual
// var seleccionado = 'estado'; // Estado inicial
var municipiosFiltrados = null;



//Colores
var initialStyle = {
    color: '#187A8C', // Color de línea
    fillColor: '#187A8C', // Color de relleno
    fillOpacity: 0.3, // Opacidad del relleno
    weight: 3 // Ancho de la línea
};

// Estilo para el hover
var highlightStyle = {
    color: '#FFDB2EC',
    fillColor: '#FFDB2E', // Color de relleno
    fillOpacity: 0.3, // Opacidad del relleno
    weight: 3
};

// Capa de estados
var estadosLayer = L.geoJSON(estados, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: function (feature, layer) {
        layer.bindTooltip('<div class="custom-tooltip">' + feature.properties.NOMGEO + '</div>');
        layer.on('click', function (e) {
            cargarMunicipios(feature.properties.CVE_ENT);
            map.fitBounds(layer.getBounds()); // Centra el mapa en el estado
        });
        // Efecto de hover
        layer.on('mouseover', function (e) {
            layer.setStyle(highlightStyle);
        });
        layer.on('mouseout', function (e) {
            estadosLayer.resetStyle(layer);
        });
    }
}).addTo(map);

// Capa de municipios (inicialmente vacía)
var municipiosLayer = L.geoJSON(null, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: onEachMunicipio
}).addTo(map);





function onEachMunicipio(feature, layer) {
    layer.bindTooltip('<div class="custom-tooltip">' + feature.properties.NOM_MUN + ', ' + feature.properties.NOMGEO + '</div>');
    // Efecto de hover
    layer.on('mouseover', function (e) {
        layer.setStyle(highlightStyle);
    });
    layer.on('mouseout', function (e) {
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
        })
    };

    municipiosLayer.addData(municipiosFiltrados);
}

//Búsquedas
//Busquedas
var lastSearchedEstadoLayer = null; // para almacenar la última entidad federativa buscada
var lastSearchedMunicipioLayer = null; // para almacenar el último municipio buscado


function buscarGeneral() {
    var terminoBuscado = document.getElementById('busquedaGeneralInput').value.trim();

    if (!terminoBuscado) {
        alert("Por favor, introduce un término de búsqueda.");
        return;  // Termina la ejecución de la función si el campo está vacío
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
                    color: '#FF0000',
                    fillColor: '#FF0000',
                    fillOpacity: 0.5
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
        var terminos = terminoBuscado.split(',');
        var buscadoMunicipio = terminos[0].trim();
        var buscadoEstado = terminos.length > 1 ? terminos[1].trim() : '';


        for (var i = 0; i < municipios_mapa.features.length; i++) {
            var municipio = municipios_mapa.features[i];
            var nombreMunicipio = municipio.properties.NOM_MUN;
            var nombreEstado = municipio.properties.NOMGEO;

            // Comprueba si el nombre del municipio y del estado coinciden con el término de búsqueda
            if (nombreMunicipio === buscadoMunicipio && (buscadoEstado === '' || nombreEstado === buscadoEstado)) {
                var bounds = L.geoJSON(municipio).getBounds();
                map.fitBounds(bounds);

                // Si ya había un municipio buscado anteriormente, lo elimina
                if (lastSearchedMunicipioLayer) {
                    map.removeLayer(lastSearchedMunicipioLayer);
                }

                // Agrega el municipio encontrado al mapa y lo resalta
                lastSearchedMunicipioLayer = L.geoJSON(municipio, {
                    style: {
                        color: '#FF0000',
                        fillColor: '#FF0000',
                        fillOpacity: 0.5
                    }
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




//Funciones de los botones y del Mapa*@





function limpiarMarcadores() {
    // Limpiar todas las capas de marcadores y círculos
    map.eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            map.removeLayer(layer);
        }
    });


}

// Definimos como arreglo
let capasDuctos = [];

// Función para limpiar todas las capas almacenadas
function limpiarDuctos() {
    if (capasDuctos.length > 0) {
        capasDuctos.forEach(capa => {
            map.removeLayer(capa);
        });
        capasDuctos = []; // opcional, para limpiar el arreglo
    } else {
        console.log("No se han cargado ductos");
    }
}

function handleNull(value) {
    return value ? value : "S/D-Sin Dato";
}
Highcharts.setOptions({
    lang: {
        decimalPoint: '.',
        thousandsSep: ','
    }
});

// Ocultar el mensaje de alerta al cambiar de sección
document.getElementById('alert-message').style.display = 'none';



//Petrolíferos
function CargaPetrolíferos() {
    // Ocultar el mensaje de alerta al cambiar de sección
    document.getElementById('alert-message').style.display = 'none';
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesPET_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //Carga Ductos de Petrolíferos
    function pop_ductos_petroliferos_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                                <tr>\
                                                                    <th scope="row">Región: </th>\
                                                                    <td>' + (feature.properties['regin'] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Ducto: </th>\
                                                                    <td>' + (feature.properties['ducto'] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Servicio: </th>\
                                                                    <td>' + (feature.properties['servicio'] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Longitud (km): </th>\
                                                                    <td>' + (feature.properties['longitud_'] !== null ? autolinker.link(feature.properties['longitud_'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Capacidad nominal (B): </th>\
                                                                    <td>' + (feature.properties['capa_n'] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Capacidad opertiva (B): </th>\
                                                                    <td>' + (feature.properties['capa_o'] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_petroliferos_4326_0_0() {
        return {
            pane: 'pane_ductos_petroliferos_4326_0',
            opacity: 1,
            color: '#a77c50',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 4.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_petroliferos_4326_0');
    map.getPane('pane_ductos_petroliferos_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_petroliferos_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_petroliferos_4326_0 = new L.geoJson(json_ductos_petroliferos_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_petroliferos_4326_0',
        layerName: 'layer_ductos_petroliferos_4326_0',
        pane: 'pane_ductos_petroliferos_4326_0',
        onEachFeature: pop_ductos_petroliferos_4326_0,
        style: style_ductos_petroliferos_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_petroliferos_4326_0);
    map.addLayer(layer_ductos_petroliferos_4326_0);
    capasDuctos.push(layer_ductos_petroliferos_4326_0);
    //setBounds();

    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosPET_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global

                //Contando por tipo de Permiso


                var conteoTiposPermiso = {};
                for (var i = 0; i < datosExpendios.length; i++) {
                    var tipoPermiso = datosExpendios[i].tipoPermiso;
                    conteoTiposPermiso[tipoPermiso] = (conteoTiposPermiso[tipoPermiso] || 0) + 1;
                }



                console.log("Estos son los Conteos:", conteoTiposPermiso);

                // Crear tabla
                var tabla = document.createElement('table');
                tabla.className = 'tabla-permisos'; // Agrega una clase para estilos si es necesario
                var tbody = document.createElement('tbody');

                for (var tipo in conteoTiposPermiso) {
                    var imgSrc;
                    switch (tipo) {

                        case "Expendios":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png';
                            break;
                        case "Autoconsumo":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png';
                            break;
                        case "Almacenamiento en aeródromos":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                            break;
                        case "Almacenamiento":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                            break;
                        case "Distribución por otros medios distintos a ducto":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png';
                            break;

                        default:
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png'; // Ícono por defecto si no hay coincidencia
                            break;

                    }

                    var tr = document.createElement('tr');

                    var tdIcono = document.createElement('td');
                    var img = document.createElement('img');
                    img.src = imgSrc;
                    img.className = 'iconomenu'; // Agregar la clase al ícono
                    tdIcono.appendChild(img);

                    var tdTipo = document.createElement('td');
                    tdTipo.textContent = tipo;

                    var tdConteo = document.createElement('td');
                    tdConteo.textContent = conteoTiposPermiso[tipo].toLocaleString(); // Formateo con separador de miles
                    tdConteo.className = 'numero-destacado'; // Asignar la clase a la celda
                    tr.appendChild(tdIcono);
                    tr.appendChild(tdTipo);
                    tr.appendChild(tdConteo);

                    tbody.appendChild(tr);
                }

                tabla.appendChild(tbody);

                document.getElementById('conteo_tipos_permisos').style.display = 'block';
                var divConteoTiposPermisos = document.getElementById('conteo_tipos_permisos');
                divConteoTiposPermisos.innerHTML = ''; // Limpiar el contenido existente
                divConteoTiposPermisos.appendChild(tabla);


                // Hacer visible el elemento 'total_petroliferos'
                document.getElementById('total_petroliferos').style.display = 'block'; // Si es un elemento de bloque
                document.getElementById('grafico').style.display = 'block';
                document.getElementById('total_infra').style.display = 'none';
                // document.getElementById('grafico2').style.display = 'none';  

                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "Expendios":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png';
                                break;
                            case "Autoconsumo":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png';
                                break;
                            case "Almacenamiento en aeródromos":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                break;
                            case "Almacenamiento":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                break;
                            case "Distribución por otros medios distintos a ducto":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png';
                                break;

                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NúmeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>EF ID:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EfNombre")) {
                        contenido += "<li><strong>EFNombre:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Mpo ID:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Número de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>FechaOtorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong> la titudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CalleNumEs")) {
                        contenido += "<li><strong>CalleNumEs:</strong> " + handleNull(coordenada.calleNumEs) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ColoniaEs")) {
                        contenido += "<li><strong>ColoniaEs:</strong> " + handleNull(coordenada.coloniaEs) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>CodigoPostal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>FechaRecepcion:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>EstatusInstalacion:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CausaSuspension")) {
                        contenido += "<li><strong>CausaSuspension:</strong> " + handleNull(coordenada.causaSuspension) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Marca")) {
                        contenido += "<li><strong>Marca:</strong> " + handleNull(coordenada.marca) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>TipoPermiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>InicioVigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TerminoVigencia")) {
                        contenido += "<li><strong>TerminoVigencia:</strong> " + handleNull(coordenada.terminoVigencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>InicioOperaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaBarriles")) {
                        contenido += "<li><strong>CapacidadAutorizadaBarriles:</strong> " + handleNull(coordenada.capacidadAutorizadaBarriles) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InversionEstimada")) {
                        contenido += "<li><strong>InversionEstimada:</strong> " + handleNull(coordenada.inversionEstimada) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Productos")) {
                        contenido += "<li><strong>Productos:</strong> " + handleNull(coordenada.productos) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoPersona")) {
                        contenido += "<li><strong>TipoPersona:</strong> " + handleNull(coordenada.tipoPersona) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeEstacionesDeServicio")) {
                        contenido += "<li><strong>Número de Estaciones de Servicio:</strong> " + handleNull(coordenada.numeroDeEstacionesDeServicio) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDeEstacion")) {
                        contenido += "<li><strong>Tipo de Estacion:</strong> " + handleNull(coordenada.tipoDeEstacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaDeAcuse")) {
                        contenido += "<li><strong>Fecha de Acuse:</strong> " + handleNull(coordenada.fechaDeAcuse) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaEntregaEstadosFinancieros")) {
                        contenido += "<li><strong>FechaEntregaEstadosFinancieros:</strong> " + handleNull(coordenada.fechaEntregaEstadosFinancieros) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Propietario")) {
                        contenido += "<li><strong>Propietario:</strong> " + handleNull(coordenada.propietario) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadMaximaDeLaBomba")) {
                        contenido += "<li><strong>CapacidadMaxima de  la Bomba:</strong> " + handleNull(coordenada.capacidadMaximaDeLaBomba) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadOperativaReal")) {
                        contenido += "<li><strong>CapacidadOperativaReal:</strong> " + handleNull(coordenada.capacidadOperativaReal) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeRegadera")) {
                        contenido += "<li><strong>Servicio de Regadera:</strong> " + handleNull(coordenada.servicioDeRegadera) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeRestaurante")) {
                        contenido += "<li><strong>Servicio de Restaurante:</strong> " + handleNull(coordenada.servicioDeRestaurante) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeSanitario")) {
                        contenido += "<li><strong>Servicio de Sanitario:</strong> " + handleNull(coordenada.servicioDeSanitario) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("OtrosServicios")) {
                        contenido += "<li><strong>OtrosServicios:</strong> " + handleNull(coordenada.otrosServicios) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TiendaDeConveniencia")) {
                        contenido += "<li><strong>Tienda de Conveniencia:</strong> " + handleNull(coordenada.tiendaDeConveniencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeModulosDespachadores")) {
                        contenido += "<li><strong>Número de Modulos de spachadores:</strong> " + handleNull(coordenada.numeroDeModulosDespachadores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDeEstacionId")) {
                        contenido += "<li><strong>Tipo de Estacion ID:</strong> " + handleNull(coordenada.tipoDeEstacionId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDePersona")) {
                        contenido += "<li><strong>Tipo de Persona:</strong> " + handleNull(coordenada.tipoDePersona) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDePermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoDePermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstadoDePermiso")) {
                        contenido += "<li><strong>Estado de Permiso:</strong> " + handleNull(coordenada.estadoDePermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstatusDeLaInstalacion")) {
                        contenido += "<li><strong>Estatus de  la Instalacion:</strong> " + handleNull(coordenada.estatusDeLaInstalacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ImagenCorporativa")) {
                        contenido += "<li><strong>ImagenCorporativa:</strong> " + handleNull(coordenada.imagenCorporativa) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CausaSuspencionInstalacionId")) {
                        contenido += "<li><strong>CausaSuspencionInstalacion ID:</strong> " + handleNull(coordenada.causaSuspencionInstalacionId) + "</li>";
                    }

                    contenido += "</ul>";

                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";

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
                        iconAnchor: [18, 16],
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png' });
                var iconoAutoConsumo = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png' });
                var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png' });
                var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png' });
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        case "Expendios":
                            iconoActual = iconoExpendio;
                            break;
                        case "Autoconsumo":
                            iconoActual = iconoAutoConsumo;
                            break;
                        case "Almacenamiento en aeródromos":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Almacenamiento":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Distribución por otros medios distintos a ducto":
                            iconoActual = iconoDistribucion;
                            break;

                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);

                    // markersLayer.addLayer(marker); // Agrega el marcador a la capa

                    markers.addLayer(marker);
                }

                map.addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tpet = response.length;
                tpetg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_petroliferos');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos
                // document.getElementById('total_infra').style.display = 'none';
                document.getElementById('total_glp').style.display = 'none';
                document.getElementById('total_gn').style.display = 'none';
                document.getElementById('total_electricidad').style.display = 'none';





                //******Fin Tarjetas Totales******///

                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                response.forEach(function (coordenada) {
                    if (!counts[coordenada.efNombre]) {
                        counts[coordenada.efNombre] = 0;
                    }
                    counts[coordenada.efNombre]++;
                });
                // Paso 2: Extrae categorías y datos
                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                // Paso 3: Configura las opciones del gráfico
                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: '#ffffff'  // Color de fondo del gráfico
                    },
                    title: {
                        text: 'Total de Permisos Vigentes de Petrolíferos por Entidad Federativa'
                    },
                    xAxis: {
                        categories: categories
                    },
                    yAxis: {
                        title: {
                            text: 'Número de permisos'
                        }
                    },
                    series: [{
                        name: 'Permisos',
                        data: dataPermisos,
                        color: '#1a8092ff',
                        dataLabels: {
                            enabled: true,   // Habilita las etiquetas de datos
                            rotation: 0,     // Rota las etiquetas (en este caso, no hay rotación)
                            color: '#000000', // Color del texto de las etiquetas
                            align: 'center',  // Alinea las etiquetas al centro
                            format: '{point.y:,.0f}',  // Formato con separador de miles
                            y: 10, // Posiciona las etiquetas un poco arriba del tope de la columna
                            style: {
                                fontSize: '13px', // Tamaño de la fuente de las etiquetas
                                fontFamily: 'Verdana, sans-serif' // Tipo de letra de las etiquetas
                            }
                        }
                    }],
                    tooltip: {
                        formatter: function () {
                            return '<b>' + this.x + '</b><br/>' +
                                this.series.name + ': ' + Highcharts.numberFormat(this.y, 0);  // Usando separador de miles

                        }
                    }
                };

                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}

//Carga GLP
function CargaGLP() {
    // Ocultar el mensaje de alerta al cambiar de sección
    document.getElementById('alert-message').style.display = 'none';
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
    function cargarCamposVisiblesGLP() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesGLP_Infra', // Asegúrate de actualizar con el nombre de tu controlador
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    // Aquí tienes la lista de campos visibles
                    console.log("campos GLP", camposVisibles);
                    camposVisiblesGlobal = camposVisibles;

                    resolve(camposVisibles); // Agrega esta línea

                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                }
            });
        });
    }
    //Carga Ductos de GLP
    function pop_ductos_glp_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                    <tr>\
                                                        <th scope="row">Región: </th>\
                                                        <td>' + (feature.properties['Región: '] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Ducto: </th>\
                                                        <td>' + (feature.properties['Ducto: '] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Servicio: </th>\
                                                        <td>' + (feature.properties['Servicio: '] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Longitud (km): </th>\
                                                        <td>' + (feature.properties['Longitud (km): '] !== null ? autolinker.link(feature.properties['lon'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Capa nominal (B): </th>\
                                                        <td>' + (feature.properties['Capacidad nominal (B): '] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Capa operativa (B): </th>\
                                                        <td>' + (feature.properties['Capacidad operativa (B): '] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_glp_4326_0_0() {
        return {
            pane: 'pane_ductos_glp_4326_0',
            opacity: 1,
            color: 'rgba(128,64,0,1.0)',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 2.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_glp_4326_0');
    map.getPane('pane_ductos_glp_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_glp_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_glp_4326_0 = new L.geoJson(json_ductos_glp_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_glp_4326_0',
        layerName: 'layer_ductos_glp_4326_0',
        pane: 'pane_ductos_glp_4326_0',
        onEachFeature: pop_ductos_glp_4326_0,
        style: style_ductos_glp_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_glp_4326_0);
    map.addLayer(layer_ductos_glp_4326_0);
    capasDuctos.push(layer_ductos_glp_4326_0);
    //setBounds();

    cargarCamposVisiblesGLP().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGLP_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GLP:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global

                //Contando por tipo de Permiso


                var conteoTiposPermiso = {};
                for (var i = 0; i < datosExpendios.length; i++) {
                    var tipoPermiso = datosExpendios[i].tipoPermiso;
                    conteoTiposPermiso[tipoPermiso] = (conteoTiposPermiso[tipoPermiso] || 0) + 1;
                }



                // Crear tabla
                var tabla = document.createElement('table');
                tabla.className = 'tabla-permisos'; // Agrega una clase para estilos si es necesario
                var tbody = document.createElement('tbody');

                for (var tipo in conteoTiposPermiso) {
                    var imgSrc;
                    switch (tipo) {
                        case "Almacenamiento de Gas Licuado de Petróleo":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png';
                            break;
                        case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                            break;
                        case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                            break;
                        default:
                            imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                            break;
                    }

                    var tr = document.createElement('tr');

                    var tdIcono = document.createElement('td');
                    var img = document.createElement('img');
                    img.src = imgSrc;
                    img.className = 'iconomenu'; // Agregar la clase al ícono
                    tdIcono.appendChild(img);

                    var tdTipo = document.createElement('td');
                    tdTipo.textContent = tipo;

                    var tdConteo = document.createElement('td');
                    tdConteo.textContent = conteoTiposPermiso[tipo].toLocaleString(); // Formateo con separador de miles
                    tdConteo.className = 'numero-destacado'; // Asignar la clase a la celda
                    tr.appendChild(tdIcono);
                    tr.appendChild(tdTipo);
                    tr.appendChild(tdConteo);

                    tbody.appendChild(tr);
                }

                tabla.appendChild(tbody);


                document.getElementById('conteo_tipos_permisos').style.display = 'block';
                var divConteoTiposPermisos = document.getElementById('conteo_tipos_permisos');
                divConteoTiposPermisos.innerHTML = ''; // Limpiar el contenido existente
                divConteoTiposPermisos.appendChild(tabla);

                // Hacer visible el elemento 'total_glp'
                document.getElementById('total_glp').style.display = 'block'; // Si es un elemento de bloque
                document.getElementById('grafico').style.display = 'block';

                document.getElementById('total_infra').style.display = 'none';
                //  document.getElementById('grafico2').style.display = 'none';  
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "Almacenamiento de Gas Licuado de Petróleo":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png';
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break;
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>MpoId:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Expediente")) {
                        contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaDeOtorgamiento")) {
                        contenido += "<li><strong>FechaDeOtorgamiento:</strong> " + handleNull(coordenada.fechaDeOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Calle")) {
                        contenido += "<li><strong>Calle:</strong> " + handleNull(coordenada.calle) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Colonia")) {
                        contenido += "<li><strong>Colonia:</strong> " + handleNull(coordenada.colonia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>CodigoPostal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>FechaRecepcion:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Marca")) {
                        contenido += "<li><strong>Marca:</strong> " + handleNull(coordenada.marca) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>TipoPermiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("inicioOperaciones")) {
                        contenido += "<li><strong>inicioOperaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CapacidadInstalacion")) {
                        contenido += "<li><strong>CapacidadInstalacion:</strong> " + handleNull(coordenada.capacidadInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("VigenciaAnos")) {
                        contenido += "<li><strong>VigenciaAnos:</strong> " + handleNull(coordenada.vigenciaAnos) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroSENER")) {
                        contenido += "<li><strong>NumeroSENER:</strong> " + handleNull(coordenada.numeroSENER) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("SubTipo")) {
                        contenido += "<li><strong>SubTipo:</strong> " + handleNull(coordenada.subTipo) + "</li>";
                    }

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

                    contenido += "</ul>";

                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        iconAnchor: [18, 16],
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png' });
                var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        case "Almacenamiento de Gas Licuado de Petróleo":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                            iconoActual = iconoDistribucion;
                            break;
                        case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);


                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tglp = response.length;
                tglpg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_glp');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos
                document.getElementById('total_petroliferos').style.display = 'none';
                document.getElementById('total_gn').style.display = 'none';
                document.getElementById('total_electricidad').style.display = 'none';

                //******Fin Tarjetas Totales******///

                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                response.forEach(function (coordenada) {
                    if (!counts[coordenada.efNombre]) {
                        counts[coordenada.efNombre] = 0;
                    }
                    counts[coordenada.efNombre]++;
                });

                // Paso 2: Extrae categorías y datos
                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                // Paso 3: Configura las opciones del gráfico
                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: '#ffffff'  // Color de fondo del gráfico
                    },
                    title: {
                        text: 'Total de Permisos de Gas L.P. Vigentes por Entidad Federativa'
                    },
                    xAxis: {
                        categories: categories
                    },
                    yAxis: {
                        title: {
                            text: 'Número de permisos'
                        }
                    },
                    series: [{
                        name: 'Permisos',
                        data: dataPermisos,
                        color: '#1a8092ff',
                        dataLabels: {
                            enabled: true,   // Habilita las etiquetas de datos
                            rotation: 0,     // Rota las etiquetas (en este caso, no hay rotación)
                            color: '#000000', // Color del texto de las etiquetas
                            align: 'center',  // Alinea las etiquetas al centro
                            format: '{point.y:,.0f}',  // Formato con separador de miles
                            y: 10, // Posiciona las etiquetas un poco arriba del tope de la columna
                            style: {
                                fontSize: '13px', // Tamaño de la fuente de las etiquetas
                                fontFamily: 'Verdana, sans-serif' // Tipo de letra de las etiquetas
                            }
                        }
                    }],
                    tooltip: {
                        formatter: function () {
                            return '<b>' + this.x + '</b><br/>' +
                                this.series.name + ': ' + Highcharts.numberFormat(this.y, 0);  // Usando separador de miles

                        }
                    }
                };

                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);
                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}

//Gas Natural
function CargaGN() {
    // Ocultar el mensaje de alerta al cambiar de sección
    document.getElementById('alert-message').style.display = 'none';
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];

    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesGN_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    function pop_ductos_glp_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                            <tr>\
                                                                <th scope="row">Región: </th>\
                                                                <td>' + (feature.properties['Región: '] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Ducto: </th>\
                                                                <td>' + (feature.properties['Ducto: '] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Servicio: </th>\
                                                                <td>' + (feature.properties['Servicio: '] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Longitud (km): </th>\
                                                                <td>' + (feature.properties['Longitud (km): '] !== null ? autolinker.link(feature.properties['lon'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Capa nominal (B): </th>\
                                                                <td>' + (feature.properties['Capacidad nominal (B): '] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Capa operativa (B): </th>\
                                                                <td>' + (feature.properties['Capacidad operativa (B): '] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_glp_4326_0_0() {
        return {
            pane: 'pane_ductos_glp_4326_0',
            opacity: 1,
            color: 'rgba(128,64,0,1.0)',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 2.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_glp_4326_0');
    map.getPane('pane_ductos_glp_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_glp_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_glp_4326_0 = new L.geoJson(json_ductos_glp_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_glp_4326_0',
        layerName: 'layer_ductos_glp_4326_0',
        pane: 'pane_ductos_glp_4326_0',
        onEachFeature: pop_ductos_glp_4326_0,
        style: style_ductos_glp_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_glp_4326_0);
    map.addLayer(layer_ductos_glp_4326_0);
    capasDuctos.push(layer_ductos_glp_4326_0);
    //setBounds();


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGN_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GN:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global

                document.getElementById('conteo_tipos_permisos').style.display = 'none';

                // Hacer visible el elemento 'total_glp'
                document.getElementById('total_gn').style.display = 'block'; // Si es un elemento de bloque
                document.getElementById('grafico').style.display = 'block';

                document.getElementById('total_infra').style.display = 'none';

                // document.getElementById('grafico2').style.display = 'none';  



                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "SD":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }

                    //if (camposVisiblesGlobal.includes("EfId")) {
                    //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    //}

                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/


                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CalleNumEs")) {
                        contenido += "<li><strong>Calle:</strong> " + handleNull(coordenada.calleNumEs) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("ColoniaEs")) {
                        contenido += "<li><strong>Colonia:</strong> " + handleNull(coordenada.coloniaEs) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>Codigo Postal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de la Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InversionEstimada")) {
                        contenido += "<li><strong>InversionEstimada:</strong> " + handleNull(coordenada.inversionEstimada) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("PermisoSuministrador")) {
                        contenido += "<li><strong>Permiso de Suministrador:</strong> " + handleNull(coordenada.permisoSuministrador) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("PermisoDistribuidor")) {
                        contenido += "<li><strong>Permiso de Distribuidor:</strong> " + handleNull(coordenada.permisoDistribuidor) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Numero de Expediente:</strong> " + handleNull(coordenada.numeronumeroDeExpediente) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Suministrador")) {
                        contenido += "<li><strong>Suministrador:</strong> " + handleNull(coordenada.suministrador) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Distribuidor")) {
                        contenido += "<li><strong>Distribuidor:</strong> " + handleNull(coordenada.distribuidor) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Interconexión")) {
                        contenido += "<li><strong>Interconexión:</strong> " + handleNull(coordenada.interconexión) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Compresores")) {
                        contenido += "<li><strong>Compresores:</strong> " + handleNull(coordenada.compresores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDespachadores")) {
                        contenido += "<li><strong>Numero de Despachadores:</strong> " + handleNull(coordenada.numeroDespachadores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadDiseño")) {
                        contenido += "<li><strong>Capacidad  de Diseño:</strong> " + handleNull(coordenada.capacidadDiseño) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Cilindros")) {
                        contenido += "<li><strong>Cilindros:</strong> " + handleNull(coordenada.cilindros) + "</li>";
                    }

                    /* if (camposVisiblesGlobal.includes("SubTipo")) {
                            contenido += "<li><strong>SubTipo:</strong> " + handleNull(coordenada.subTipo) + "</li>";
                        }

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
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        /*  case "SD":
                                iconoActual = iconoAlmacenamiento;
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                iconoActual = iconoDistribucion;
                                break; */
                        case "SD":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);


                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tgn = response.length;
                tgng = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_gn');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos
                document.getElementById('total_petroliferos').style.display = 'none';
                document.getElementById('total_glp').style.display = 'none';
                document.getElementById('total_electricidad').style.display = 'none';

                //******Fin Tarjetas Totales******///


                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                response.forEach(function (coordenada) {
                    if (!counts[coordenada.efId]) {
                        counts[coordenada.efId] = 0;
                    }
                    counts[coordenada.efId]++;
                });

                // Paso 2: Extrae categorías y datos
                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                // Paso 3: Configura las opciones del gráfico
                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: '#ffffff'  // Color de fondo del gráfico
                    },
                    title: {
                        text: 'Total de Permisos de Gas Natural Vigentes por Entidad Federativa'
                    },
                    xAxis: {
                        categories: categories
                    },
                    yAxis: {
                        title: {
                            text: 'Número de permisos'
                        }
                    },
                    series: [{
                        name: 'Permisos',
                        data: dataPermisos,
                        color: '#1a8092ff',
                        dataLabels: {
                            enabled: true,   // Habilita las etiquetas de datos
                            rotation: 0,     // Rota las etiquetas (en este caso, no hay rotación)
                            color: '#000000', // Color del texto de las etiquetas
                            align: 'center',  // Alinea las etiquetas al centro
                            format: '{point.y:,.0f}',  // Formato con separador de miles
                            y: 10, // Posiciona las etiquetas un poco arriba del tope de la columna
                            style: {
                                fontSize: '13px', // Tamaño de la fuente de las etiquetas
                                fontFamily: 'Verdana, sans-serif' // Tipo de letra de las etiquetas
                            }
                        }
                    }],
                    tooltip: {
                        formatter: function () {
                            return '<b>' + this.x + '</b><br/>' +
                                this.series.name + ': ' + Highcharts.numberFormat(this.y, 0);  // Usando separador de miles

                        }
                    }
                };

                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);
                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}

//Electricidad
function CargaElectricidad() {
    // Lógica existente para cargar la sección de electricidad...
    document.getElementById('alert-message').style.display = 'block';
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //No hay ductos


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores();
        limpiarDuctos();
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de Electricidad:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global

                document.getElementById('conteo_tipos_permisos').style.display = 'none';


                // Hacer visible el elemento 'total_glp'
                document.getElementById('total_electricidad').style.display = 'block'; // Si es un elemento de bloque
                document.getElementById('grafico').style.display = 'block';

                document.getElementById('total_infra').style.display = 'none';
                //document.getElementById('grafico2').style.display = 'none';   
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Clave y Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Clave y Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Numero de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }

                    //if (camposVisiblesGlobal.includes("EfId")) {
                    //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    //}

                    /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/


                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>Fecha de Recepción:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>Inicio de Vigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>Inicio  de Operaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
                        contenido += "<li><strong>Capacidad Autorizada  en MW:</strong> " + handleNull(coordenada.capacidadAutorizadaMW) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
                        contenido += "<li><strong>Generación Estimada Anual:</strong> " + handleNull(coordenada.generacion_estimada_anual) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
                        contenido += "<li><strong>Inversión Estimada en mdls:</strong> " + handleNull(coordenada.inversion_estimada_mdls) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Energetico_primario")) {
                        contenido += "<li><strong>Energético Primario:</strong> " + handleNull(coordenada.energetico_primario) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Actividad_economica")) {
                        contenido += "<li><strong>Actividad Económica:</strong> " + handleNull(coordenada.actividad_economica) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Tecnología")) {
                        contenido += "<li><strong>Tecnología:</strong> " + handleNull(coordenada.tecnologia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EmpresaLíder")) {
                        contenido += "<li><strong>Empresa Líder:</strong> " + handleNull(coordenada.empresaLider) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
                        contenido += "<li><strong>País de Origen:</strong> " + handleNull(coordenada.paisDeOrigen) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Subasta")) {
                        contenido += "<li><strong>Subasta:</strong> " + handleNull(coordenada.subasta) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Planta")) {
                        contenido += "<li><strong>Planta:</strong> " + handleNull(coordenada.planta) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Combustible")) {
                        contenido += "<li><strong>Combustible:</strong> " + handleNull(coordenada.combustible) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FuenteEnergía")) {
                        contenido += "<li><strong>Fuente de Energía:</strong> " + handleNull(coordenada.fuenteEnergia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
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
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        /*  case "SD":
                                iconoActual = iconoAlmacenamiento;
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                iconoActual = iconoDistribucion;
                                break; */
                        case "vacío":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                te = response.length;
                teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_electricidad');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos
                document.getElementById('total_petroliferos').style.display = 'none';
                document.getElementById('total_glp').style.display = 'none';
                document.getElementById('total_gn').style.display = 'none';

                //******Fin Tarjetas Totales******///

                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                response.forEach(function (coordenada) {
                    if (!counts[coordenada.efId]) {
                        counts[coordenada.efId] = 0;
                    }
                    counts[coordenada.efId]++;
                });

                // Paso 2: Extrae categorías y datos
                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                // Paso 3: Configura las opciones del gráfico
                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: '#ffffff'  // Color de fondo del gráfico
                    },
                    title: {
                        text: 'Total de Permisos Vigentes de Electricidad por Entidad Federativa'
                    },
                    xAxis: {
                        categories: categories
                    },
                    yAxis: {
                        title: {
                            text: 'Número de permisos'
                        }
                    },
                    series: [{
                        name: 'Permisos',
                        data: dataPermisos,
                        color: '#1a8092ff',
                        dataLabels: {
                            enabled: true,   // Habilita las etiquetas de datos
                            rotation: 0,     // Rota las etiquetas (en este caso, no hay rotación)
                            color: '#000000', // Color del texto de las etiquetas
                            align: 'center',  // Alinea las etiquetas al centro
                            format: '{point.y:,.0f}',  // Formato con separador de miles
                            y: 10, // Posiciona las etiquetas un poco arriba del tope de la columna
                            style: {
                                fontSize: '13px', // Tamaño de la fuente de las etiquetas
                                fontFamily: 'Verdana, sans-serif' // Tipo de letra de las etiquetas
                            }
                        }
                    }],
                    tooltip: {
                        formatter: function () {
                            return '<b>' + this.x + '</b><br/>' +
                                this.series.name + ': ' + Highcharts.numberFormat(this.y, 0);  // Usando separador de miles

                        }
                    }
                };

                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);




                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}


//Inicial
function CargaPetrolíferosI() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Primera llamada AJAX como una promesa
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesPET_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosPET_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global


                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }
                //******Tarjetas Totales******///
                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tpet = response.length;
                tpetg = response;
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;
                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}

function CargaGLPI() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Primera llamada AJAX como una promesa
    function cargarCamposVisiblesGLP() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesGLP_Infra', // Asegúrate de actualizar con el nombre de tu controlador
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    // Aquí tienes la lista de campos visibles
                    console.log("campos GLP", camposVisibles);
                    camposVisiblesGlobal = camposVisibles;

                    resolve(camposVisibles); // Agrega esta línea

                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                }
            });
        });
    }

    cargarCamposVisiblesGLP().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGLP_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GLP:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global


                //  document.getElementById('grafico2').style.display = 'none';  
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }
                //******Tarjetas Totales******///
                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tglp = response.length;
                tglpg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();
                console.log("Total de elementos en 'response' formateado:", totalFormateado);
                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_glp');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;
                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;
                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });
}
//Gas Natural
function CargaGNI() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];

    camposVisiblesGlobal = [];
    totalpermisos = 0;


    // Primera llamada AJAX como una promesa
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesGN_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGN_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GN:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global



                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }



                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tgn = response.length;
                tgng = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_gn');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;


                //******Fin Tarjetas Totales******///


                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}
//Electricidad
function CargaElectricidadI() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;



    // Primera llamada AJAX como una promesa
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //No hay ductos


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores();
        limpiarDuctos();
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de Electricidad:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global


                // Hacer visible el elemento 'total_glp'

                //document.getElementById('grafico2').style.display = 'none';   
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }



                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                te = response.length;
                teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_electricidad');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}


///
//Petrolíferos
function CargaPetrolíferos2() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesPET_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //Carga Ductos de Petrolíferos
    function pop_ductos_petroliferos_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                                <tr>\
                                                                    <th scope="row">Región: </th>\
                                                                    <td>' + (feature.properties['regin'] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Ducto: </th>\
                                                                    <td>' + (feature.properties['ducto'] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Servicio: </th>\
                                                                    <td>' + (feature.properties['servicio'] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Longitud (km): </th>\
                                                                    <td>' + (feature.properties['longitud_'] !== null ? autolinker.link(feature.properties['longitud_'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Capacidad nominal (B): </th>\
                                                                    <td>' + (feature.properties['capa_n'] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                                </tr>\
                                                                <tr>\
                                                                    <th scope="row">Capacidad opertiva (B): </th>\
                                                                    <td>' + (feature.properties['capa_o'] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_petroliferos_4326_0_0() {
        return {
            pane: 'pane_ductos_petroliferos_4326_0',
            opacity: 1,
            color: '#a77c50',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 4.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_petroliferos_4326_0');
    map.getPane('pane_ductos_petroliferos_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_petroliferos_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_petroliferos_4326_0 = new L.geoJson(json_ductos_petroliferos_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_petroliferos_4326_0',
        layerName: 'layer_ductos_petroliferos_4326_0',
        pane: 'pane_ductos_petroliferos_4326_0',
        onEachFeature: pop_ductos_petroliferos_4326_0,
        style: style_ductos_petroliferos_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_petroliferos_4326_0);
    map.addLayer(layer_ductos_petroliferos_4326_0);
    capasDuctos.push(layer_ductos_petroliferos_4326_0);
    //setBounds();

    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosPET_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }
                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "Expendios":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png';
                                break;
                            case "Autoconsumo":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png';
                                break;
                            case "Almacenamiento en aeródromos":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                break;
                            case "Almacenamiento":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                break;
                            case "Distribución por otros medios distintos a ducto":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png';
                                break;

                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NúmeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>EF ID:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EfNombre")) {
                        contenido += "<li><strong>EFNombre:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Mpo ID:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Número de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>FechaOtorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong> la titudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CalleNumEs")) {
                        contenido += "<li><strong>CalleNumEs:</strong> " + handleNull(coordenada.calleNumEs) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ColoniaEs")) {
                        contenido += "<li><strong>ColoniaEs:</strong> " + handleNull(coordenada.coloniaEs) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>CodigoPostal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>FechaRecepcion:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>EstatusInstalacion:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CausaSuspension")) {
                        contenido += "<li><strong>CausaSuspension:</strong> " + handleNull(coordenada.causaSuspension) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Marca")) {
                        contenido += "<li><strong>Marca:</strong> " + handleNull(coordenada.marca) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>TipoPermiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>InicioVigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TerminoVigencia")) {
                        contenido += "<li><strong>TerminoVigencia:</strong> " + handleNull(coordenada.terminoVigencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>InicioOperaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaBarriles")) {
                        contenido += "<li><strong>CapacidadAutorizadaBarriles:</strong> " + handleNull(coordenada.capacidadAutorizadaBarriles) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InversionEstimada")) {
                        contenido += "<li><strong>InversionEstimada:</strong> " + handleNull(coordenada.inversionEstimada) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Productos")) {
                        contenido += "<li><strong>Productos:</strong> " + handleNull(coordenada.productos) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoPersona")) {
                        contenido += "<li><strong>TipoPersona:</strong> " + handleNull(coordenada.tipoPersona) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeEstacionesDeServicio")) {
                        contenido += "<li><strong>Número de Estaciones de Servicio:</strong> " + handleNull(coordenada.numeroDeEstacionesDeServicio) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDeEstacion")) {
                        contenido += "<li><strong>Tipo de Estacion:</strong> " + handleNull(coordenada.tipoDeEstacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaDeAcuse")) {
                        contenido += "<li><strong>Fecha de Acuse:</strong> " + handleNull(coordenada.fechaDeAcuse) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaEntregaEstadosFinancieros")) {
                        contenido += "<li><strong>FechaEntregaEstadosFinancieros:</strong> " + handleNull(coordenada.fechaEntregaEstadosFinancieros) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Propietario")) {
                        contenido += "<li><strong>Propietario:</strong> " + handleNull(coordenada.propietario) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadMaximaDeLaBomba")) {
                        contenido += "<li><strong>CapacidadMaxima de  la Bomba:</strong> " + handleNull(coordenada.capacidadMaximaDeLaBomba) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadOperativaReal")) {
                        contenido += "<li><strong>CapacidadOperativaReal:</strong> " + handleNull(coordenada.capacidadOperativaReal) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeRegadera")) {
                        contenido += "<li><strong>Servicio de Regadera:</strong> " + handleNull(coordenada.servicioDeRegadera) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeRestaurante")) {
                        contenido += "<li><strong>Servicio de Restaurante:</strong> " + handleNull(coordenada.servicioDeRestaurante) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ServicioDeSanitario")) {
                        contenido += "<li><strong>Servicio de Sanitario:</strong> " + handleNull(coordenada.servicioDeSanitario) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("OtrosServicios")) {
                        contenido += "<li><strong>OtrosServicios:</strong> " + handleNull(coordenada.otrosServicios) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TiendaDeConveniencia")) {
                        contenido += "<li><strong>Tienda de Conveniencia:</strong> " + handleNull(coordenada.tiendaDeConveniencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeModulosDespachadores")) {
                        contenido += "<li><strong>Número de Modulos de spachadores:</strong> " + handleNull(coordenada.numeroDeModulosDespachadores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDeEstacionId")) {
                        contenido += "<li><strong>Tipo de Estacion ID:</strong> " + handleNull(coordenada.tipoDeEstacionId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDePersona")) {
                        contenido += "<li><strong>Tipo de Persona:</strong> " + handleNull(coordenada.tipoDePersona) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoDePermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoDePermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstadoDePermiso")) {
                        contenido += "<li><strong>Estado de Permiso:</strong> " + handleNull(coordenada.estadoDePermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstatusDeLaInstalacion")) {
                        contenido += "<li><strong>Estatus de  la Instalacion:</strong> " + handleNull(coordenada.estatusDeLaInstalacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("ImagenCorporativa")) {
                        contenido += "<li><strong>ImagenCorporativa:</strong> " + handleNull(coordenada.imagenCorporativa) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CausaSuspencionInstalacionId")) {
                        contenido += "<li><strong>CausaSuspencionInstalacion ID:</strong> " + handleNull(coordenada.causaSuspencionInstalacionId) + "</li>";
                    }

                    contenido += "</ul>";

                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";

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
                        iconAnchor: [18, 16],
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png' });
                var iconoAutoConsumo = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png' });
                var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png' });
                var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png' });
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        case "Expendios":
                            iconoActual = iconoExpendio;
                            break;
                        case "Autoconsumo":
                            iconoActual = iconoAutoConsumo;
                            break;
                        case "Almacenamiento en aeródromos":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Almacenamiento":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Distribución por otros medios distintos a ducto":
                            iconoActual = iconoDistribucion;
                            break;

                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }
                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);
                    markers.addLayer(marker);
                }
                map.addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tpet = response.length;
                tpetg = response;



                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}



//Carga GLP
function CargaGLP2() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
    function cargarCamposVisiblesGLP() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesGLP_Infra', // Asegúrate de actualizar con el nombre de tu controlador
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    // Aquí tienes la lista de campos visibles
                    console.log("campos GLP", camposVisibles);
                    camposVisiblesGlobal = camposVisibles;

                    resolve(camposVisibles); // Agrega esta línea

                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                }
            });
        });
    }
    //Carga Ductos de GLP
    function pop_ductos_glp_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                    <tr>\
                                                        <th scope="row">Región: </th>\
                                                        <td>' + (feature.properties['Región: '] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Ducto: </th>\
                                                        <td>' + (feature.properties['Ducto: '] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Servicio: </th>\
                                                        <td>' + (feature.properties['Servicio: '] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Longitud (km): </th>\
                                                        <td>' + (feature.properties['Longitud (km): '] !== null ? autolinker.link(feature.properties['lon'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Capa nominal (B): </th>\
                                                        <td>' + (feature.properties['Capacidad nominal (B): '] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                    </tr>\
                                                    <tr>\
                                                        <th scope="row">Capa operativa (B): </th>\
                                                        <td>' + (feature.properties['Capacidad operativa (B): '] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_glp_4326_0_0() {
        return {
            pane: 'pane_ductos_glp_4326_0',
            opacity: 1,
            color: 'rgba(128,64,0,1.0)',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 2.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_glp_4326_0');
    map.getPane('pane_ductos_glp_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_glp_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_glp_4326_0 = new L.geoJson(json_ductos_glp_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_glp_4326_0',
        layerName: 'layer_ductos_glp_4326_0',
        pane: 'pane_ductos_glp_4326_0',
        onEachFeature: pop_ductos_glp_4326_0,
        style: style_ductos_glp_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_glp_4326_0);
    map.addLayer(layer_ductos_glp_4326_0);
    capasDuctos.push(layer_ductos_glp_4326_0);
    //setBounds();

    cargarCamposVisiblesGLP().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGLP_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GLP:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global


                //  document.getElementById('grafico2').style.display = 'none';  
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "Almacenamiento de Gas Licuado de Petróleo":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png';
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break;
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efNombre) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>MpoId:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Expediente")) {
                        contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaDeOtorgamiento")) {
                        contenido += "<li><strong>FechaDeOtorgamiento:</strong> " + handleNull(coordenada.fechaDeOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Calle")) {
                        contenido += "<li><strong>Calle:</strong> " + handleNull(coordenada.calle) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Colonia")) {
                        contenido += "<li><strong>Colonia:</strong> " + handleNull(coordenada.colonia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>CodigoPostal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>FechaRecepcion:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Marca")) {
                        contenido += "<li><strong>Marca:</strong> " + handleNull(coordenada.marca) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>TipoPermiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("inicioOperaciones")) {
                        contenido += "<li><strong>inicioOperaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CapacidadInstalacion")) {
                        contenido += "<li><strong>CapacidadInstalacion:</strong> " + handleNull(coordenada.capacidadInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("VigenciaAnos")) {
                        contenido += "<li><strong>VigenciaAnos:</strong> " + handleNull(coordenada.vigenciaAnos) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroSENER")) {
                        contenido += "<li><strong>NumeroSENER:</strong> " + handleNull(coordenada.numeroSENER) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("SubTipo")) {
                        contenido += "<li><strong>SubTipo:</strong> " + handleNull(coordenada.subTipo) + "</li>";
                    }

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

                    contenido += "</ul>";

                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        iconAnchor: [18, 16],
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png' });
                var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        case "Almacenamiento de Gas Licuado de Petróleo":
                            iconoActual = iconoAlmacenamiento;
                            break;
                        case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                            iconoActual = iconoDistribucion;
                            break;
                        case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);


                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tglp = response.length;
                tglpg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_glp');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;




                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}

//Gas Natural
function CargaGN2() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];

    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesGN_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    function pop_ductos_glp_4326_0(feature, layer) {
        // Variables para almacenar las coordenadas de clic
        let clickedLat, clickedLon;
        function updatePopupContent() {
            var popupContent = '<table>\
                                                            <tr>\
                                                                <th scope="row">Región: </th>\
                                                                <td>' + (feature.properties['Región: '] !== null ? autolinker.link(feature.properties['regin'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Ducto: </th>\
                                                                <td>' + (feature.properties['Ducto: '] !== null ? autolinker.link(feature.properties['ducto'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Servicio: </th>\
                                                                <td>' + (feature.properties['Servicio: '] !== null ? autolinker.link(feature.properties['servicio'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Longitud (km): </th>\
                                                                <td>' + (feature.properties['Longitud (km): '] !== null ? autolinker.link(feature.properties['lon'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Capa nominal (B): </th>\
                                                                <td>' + (feature.properties['Capacidad nominal (B): '] !== null ? autolinker.link(feature.properties['capa_n'].toLocaleString()) : '') + '</td>\
                                                            </tr>\
                                                            <tr>\
                                                                <th scope="row">Capa operativa (B): </th>\
                                                                <td>' + (feature.properties['Capacidad operativa (B): '] !== null ? autolinker.link(feature.properties['capa_o'].toLocaleString()) : '') + '</td>\
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

    function style_ductos_glp_4326_0_0() {
        return {
            pane: 'pane_ductos_glp_4326_0',
            opacity: 1,
            color: 'rgba(128,64,0,1.0)',
            dashArray: '',
            lineCap: 'square',
            lineJoin: 'bevel',
            weight: 2.0,
            fillOpacity: 0,
            interactive: true,
        }
    }
    map.createPane('pane_ductos_glp_4326_0');
    map.getPane('pane_ductos_glp_4326_0').style.zIndex = 400;
    map.getPane('pane_ductos_glp_4326_0').style['mix-blend-mode'] = 'normal';
    var layer_ductos_glp_4326_0 = new L.geoJson(json_ductos_glp_4326_0, {
        attribution: '',
        interactive: true,
        dataVar: 'json_ductos_glp_4326_0',
        layerName: 'layer_ductos_glp_4326_0',
        pane: 'pane_ductos_glp_4326_0',
        onEachFeature: pop_ductos_glp_4326_0,
        style: style_ductos_glp_4326_0_0,
    });
    bounds_group.addLayer(layer_ductos_glp_4326_0);
    map.addLayer(layer_ductos_glp_4326_0);
    capasDuctos.push(layer_ductos_glp_4326_0);
    //setBounds();


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosGN_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de GN:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global



                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "SD":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }

                    //if (camposVisiblesGlobal.includes("EfId")) {
                    //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    //}

                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/


                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CalleNumEs")) {
                        contenido += "<li><strong>Calle:</strong> " + handleNull(coordenada.calleNumEs) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("ColoniaEs")) {
                        contenido += "<li><strong>Colonia:</strong> " + handleNull(coordenada.coloniaEs) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CodigoPostal")) {
                        contenido += "<li><strong>Codigo Postal:</strong> " + handleNull(coordenada.codigoPostal) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de la Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InversionEstimada")) {
                        contenido += "<li><strong>InversionEstimada:</strong> " + handleNull(coordenada.inversionEstimada) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("PermisoSuministrador")) {
                        contenido += "<li><strong>Permiso de Suministrador:</strong> " + handleNull(coordenada.permisoSuministrador) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("PermisoDistribuidor")) {
                        contenido += "<li><strong>Permiso de Distribuidor:</strong> " + handleNull(coordenada.permisoDistribuidor) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Numero de Expediente:</strong> " + handleNull(coordenada.numeronumeroDeExpediente) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Suministrador")) {
                        contenido += "<li><strong>Suministrador:</strong> " + handleNull(coordenada.suministrador) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Distribuidor")) {
                        contenido += "<li><strong>Distribuidor:</strong> " + handleNull(coordenada.distribuidor) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Interconexión")) {
                        contenido += "<li><strong>Interconexión:</strong> " + handleNull(coordenada.interconexión) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Compresores")) {
                        contenido += "<li><strong>Compresores:</strong> " + handleNull(coordenada.compresores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDespachadores")) {
                        contenido += "<li><strong>Numero de Despachadores:</strong> " + handleNull(coordenada.numeroDespachadores) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadDiseño")) {
                        contenido += "<li><strong>Capacidad  de Diseño:</strong> " + handleNull(coordenada.capacidadDiseño) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Cilindros")) {
                        contenido += "<li><strong>Cilindros:</strong> " + handleNull(coordenada.cilindros) + "</li>";
                    }

                    /* if (camposVisiblesGlobal.includes("SubTipo")) {
                            contenido += "<li><strong>SubTipo:</strong> " + handleNull(coordenada.subTipo) + "</li>";
                        }

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
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        /*  case "SD":
                                iconoActual = iconoAlmacenamiento;
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                iconoActual = iconoDistribucion;
                                break; */
                        case "SD":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);


                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                tgn = response.length;
                tgng = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_gn');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;


                //******Fin Tarjetas Totales******///


                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}


//Electricidad
function CargaElectricidad2() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
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
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //No hay ductos


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores();
        limpiarDuctos();
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de Electricidad:", response); // ver la respuesta en consola
                datosExpendios = response; // Guarda la respuesta en la variable global


                // Hacer visible el elemento 'total_glp'

                //document.getElementById('grafico2').style.display = 'none';   
                //Guardo los terminos en la  búsqueda
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
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
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Clave y Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Clave y Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Numero de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }

                    //if (camposVisiblesGlobal.includes("EfId")) {
                    //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    //}

                    /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/


                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Rfc")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>Fecha de Recepción:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>Inicio de Vigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>Inicio  de Operaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
                        contenido += "<li><strong>Capacidad Autorizada  en MW:</strong> " + handleNull(coordenada.capacidadAutorizadaMW) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
                        contenido += "<li><strong>Generación Estimada Anual:</strong> " + handleNull(coordenada.generacion_estimada_anual) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
                        contenido += "<li><strong>Inversión Estimada en mdls:</strong> " + handleNull(coordenada.inversion_estimada_mdls) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Energetico_primario")) {
                        contenido += "<li><strong>Energético Primario:</strong> " + handleNull(coordenada.energetico_primario) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Actividad_economica")) {
                        contenido += "<li><strong>Actividad Económica:</strong> " + handleNull(coordenada.actividad_economica) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Tecnología")) {
                        contenido += "<li><strong>Tecnología:</strong> " + handleNull(coordenada.tecnologia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EmpresaLíder")) {
                        contenido += "<li><strong>Empresa Líder:</strong> " + handleNull(coordenada.empresaLider) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
                        contenido += "<li><strong>País de Origen:</strong> " + handleNull(coordenada.paisDeOrigen) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Subasta")) {
                        contenido += "<li><strong>Subasta:</strong> " + handleNull(coordenada.subasta) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Planta")) {
                        contenido += "<li><strong>Planta:</strong> " + handleNull(coordenada.planta) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Combustible")) {
                        contenido += "<li><strong>Combustible:</strong> " + handleNull(coordenada.combustible) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FuenteEnergía")) {
                        contenido += "<li><strong>Fuente de Energía:</strong> " + handleNull(coordenada.fuenteEnergia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
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
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


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
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;
                    switch (coordenada.tipoPermiso) {
                        /*  case "SD":
                                iconoActual = iconoAlmacenamiento;
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                iconoActual = iconoDistribucion;
                                break; */
                        case "vacío":
                            iconoActual = iconoExpendio;
                            break;
                        default:
                            iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                            break;
                    }



                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);



                    markers.addLayer(marker);
                }

                map.addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = response.length;
                te = response.length;
                teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_electricidad');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;



                //Mete el autocompetar ala busqueda
                // Función para inicializar el autocompletar
                function autocomplete(inp, arr) {
                    var currentFocus;

                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
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

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}







//Toda la Infraestructura
async function CargaInfraestructura() {

    try {
        CargaPetrolíferos();
        CargaGN();
        CargaGLP();
        CargaElectricidad();
    } catch (error) {
        console.error("Se produjo un error en la carga de infraestructura:", error);
    }
}

///AQQQUIIIIII
// Toda total SEM
//Toda total SEM
async function CargaSEM() {
    // Lógica existente para cargar la sección de electricidad...
    document.getElementById('alert-message').style.display = 'block';
    try {
        CargaPetrolíferos2();
        CargaGLP2();
        CargaGN2();
        CargaElectricidad2();


        // Hacer visible el elemento 'total_petroliferos'
        document.getElementById('total_petroliferos').style.display = 'none'; // Si es un elemento de bloque
        document.getElementById('total_glp').style.display = 'none';
        document.getElementById('total_gn').style.display = 'none';
        document.getElementById('total_electricidad').style.display = 'none';
        document.getElementById('total_infra').style.display = 'block';
        //   document.getElementById('grafico2').style.display = 'block';  





        // Suponiendo que 'totalPetroliferos', 'totalGLP', 'totalGN', y 'totalElectricidad' son tus variables globales
        let totalPermisos = tpetg.length + tglpg.length + tgng.length + teg.length;
        // Formatear el número con separadores de miles
        let totalFormateado = totalPermisos.toLocaleString();
        // Mostrar este total formateado en el elemento 'total_infra'
        var totalInfra = document.getElementById('total_infra');
        totalInfra.textContent = "Total de Permisos: " + totalFormateado;


        var conteoFinal = [];
        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosPET_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                conteoFinal[0] = response;
                $.ajax({
                    url: '/Indicadores/GetExpendiosAutorizadosGLP_Infra',
                    type: 'GET',
                    // data: JSON.stringify(datos_mun),
                    contentType: 'application/json',
                    success: function (response) {
                        conteoFinal[1] = response;
                        $.ajax({
                            url: '/Indicadores/GetExpendiosAutorizadosGN_Infra',
                            type: 'GET',
                            // data: JSON.stringify(datos_mun),
                            contentType: 'application/json',
                            success: function (response) {
                                conteoFinal[2] = response;
                                $.ajax({
                                    url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
                                    type: 'GET',
                                    // data: JSON.stringify(datos_mun),
                                    contentType: 'application/json',
                                    success: function (response) {
                                        conteoFinal[3] = response;
                                        //Contando por tipo de Permiso

                                        console.log("Conteos finales: ", conteoFinal);


                                        var conteoTiposPermiso = {};
                                        var datoExtra = {}
                                        datoExtra = conteoFinal[0];
                                        for (var i = 0; i < datoExtra.length; i++) {
                                            var tipoPermiso = datoExtra[i].tipoPermiso;
                                            conteoTiposPermiso[tipoPermiso] = (conteoTiposPermiso[tipoPermiso] || 0) + 1;
                                        }
                                        datoExtra = conteoFinal[1];
                                        for (var i = 0; i < datoExtra.length; i++) {
                                            var tipoPermiso = datoExtra[i].tipoPermiso;
                                            conteoTiposPermiso[tipoPermiso] = (conteoTiposPermiso[tipoPermiso] || 0) + 1;
                                        }
                                        datoExtra = conteoFinal[2];
                                        for (var i = 0; i < datoExtra.length; i++) {
                                            var tipoPermiso = datoExtra[i].tipoPermiso.trim();
                                            conteoTiposPermiso[tipoPermiso] = (conteoTiposPermiso[tipoPermiso] || 0) + 1;
                                        }
                                        datoExtra = conteoFinal[3];
                                        for (var i = 0; i < datoExtra.length; i++) {
                                            var combustible = datoExtra[i].combustible; //Se modifica combustible a tipoPermiso en caso de querer mostrar cada tipo de electricidad
                                            conteoTiposPermiso[combustible] = (conteoTiposPermiso[combustible] || 0) + 1;
                                        }

                                        console.log("Datos extra: ", datoExtra);



                                        console.log("Estos son los Conteos FINALES:", conteoTiposPermiso);

                                        // Crear tabla
                                        var tabla = document.createElement('table');
                                        tabla.className = 'tabla-permisos'; // Agrega una clase para estilos si es necesario
                                        var tbody = document.createElement('tbody');


                                        for (var tipo in conteoTiposPermiso) {
                                            var tipoMostrar;
                                            if (tipo === "SD") { //Se elimina el IF en caso de que se necesite mostrar los tipos de electricidad
                                                tipoMostrar = "Electricidad";
                                            }
                                            var imgSrc;
                                            switch (tipo) {

                                                case "Expendios":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png';
                                                    break;
                                                case "Autoconsumo":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/autoconsumo.png';
                                                    break;
                                                case "Almacenamiento en aeródromos":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                                    break;
                                                case "Almacenamiento":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/alacenamientogasolina.png';
                                                    break;
                                                case "Distribución por otros medios distintos a ducto":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/distribuciongasolina.png';
                                                    break;
                                                case "Almacenamiento de Gas Licuado de Petróleo":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png';
                                                    break;
                                                case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                                    break;
                                                case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                                    break;
                                                case "Expendio al público de gas natural comprimido":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnaturali.png';
                                                    break;
                                                case "EXPENDIOS":
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasolinera.png';
                                                    break;
                                                default:
                                                    imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png'; // Ícono por defecto si no hay coincidencia
                                                    break;

                                            }

                                            var tr = document.createElement('tr');

                                            var tdIcono = document.createElement('td');
                                            var img = document.createElement('img');
                                            img.src = imgSrc;
                                            img.className = 'iconomenu'; // Agregar la clase al ícono
                                            tdIcono.appendChild(img);

                                            var tdTipo = document.createElement('td');
                                            if (tipo === "SD") {
                                                tdTipo.textContent = tipoMostrar; //Se elimina en caso de querer mostrar los tipos de electricidad
                                            } else {
                                                tdTipo.textContent = tipo; //Se deja en todo momento
                                            }


                                            var tdConteo = document.createElement('td');
                                            tdConteo.textContent = conteoTiposPermiso[tipo].toLocaleString(); // Formateo con separador de miles
                                            tdConteo.className = 'numero-destacado'; // Asignar la clase a la celda
                                            tr.appendChild(tdIcono);
                                            tr.appendChild(tdTipo);
                                            tr.appendChild(tdConteo);

                                            tbody.appendChild(tr);
                                        }

                                        tabla.appendChild(tbody);

                                        document.getElementById('conteo_tipos_permisos').style.display = 'block';
                                        var divConteoTiposPermisos = document.getElementById('conteo_tipos_permisos');
                                        divConteoTiposPermisos.innerHTML = ''; // Limpiar el contenido existente
                                        divConteoTiposPermisos.appendChild(tabla);
                                    },
                                    error: function (error) {
                                        // Maneja el error si ocurre.
                                    }
                                });
                            },
                            error: function (error) {
                                // Maneja el error si ocurre.
                            }
                        });
                    },
                    error: function (error) {
                        // Maneja el error si ocurre.
                    }
                });
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });


        console.log("Total de Infra:", totalFormateado);
        $.ajax({
            url: '/Indicadores/ObtenerDatosGrafico',
            method: 'GET',
            success: function (result) {
                var categorias = [];
                var petroliferos = [];
                var electricidad = [];
                var gasNatural = [];
                var gasLP = [];

                result.forEach(function (item) {
                    categorias.push(item.efNombre);
                    petroliferos.push(item.totalPetroliferos);
                    electricidad.push(item.totalElectricidad);
                    gasNatural.push(item.totalGasNatural);
                    gasLP.push(item.totalGasLP);
                });

                // Configuración del gráfico
                var options = {
                    chart: {
                        type: 'line',
                        backgroundColor: '#ffffff'
                    },
                    title: {
                        text: 'Total de Permisos Vigentes por Entidad Federativa'
                    },
                    xAxis: {
                        categories: categorias
                    },
                    yAxis: {
                        title: {
                            text: 'Número de permisos'
                        }
                    },
                    plotOptions: {
                        series: {
                            marker: {
                                enabled: true,
                                symbol: 'circle',
                                radius: 4
                            }
                        }
                    },
                    series: [{
                        name: 'Petrolíferos',
                        data: petroliferos,
                        color: '#1a8092ff'
                    }, {
                        name: 'Electricidad',
                        data: electricidad,
                        color: '#0000ff'
                    }, {
                        name: 'Gas Natural',
                        data: gasNatural,
                        color: '#00ff00'
                    }, {
                        name: 'Gas LP',
                        data: gasLP,
                        color: '#ff0000'
                    }]
                };

                // Renderizar el gráfico
                Highcharts.chart('grafico', options);
            },
            error: function (xhr, status, error) {
                console.error('Error al obtener los datos del gráfico: ', error);
            }
        });





    } catch (error) {
        console.error("Se produjo un error en la carga de infraestructura:", error);
    }
}

///AQUIIIIIIIII/////////FIIIIIIIIIINMNN
CargaPetrolíferosI();
CargaElectricidadI();
CargaGNI();
CargaGLPI();
limpiarMarcadores();
CargaPetrolíferos();
// Funcionalidades de Búsqueda y Menú*@
// Función para inicializar el autocompletar
function autocomplete(inp, arr) {
    var currentFocus;

    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }
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

// Inicializa el autocompletar
autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);

//Links activos*@
function activarElemento(elementoID) {
    // Obtén todos los elementos de tu menú
    var elementos = document.querySelectorAll('.navbarmapag a');

    // Itera sobre ellos para eliminar la clase 'active'
    elementos.forEach(function (el) {
        el.classList.remove('active');
    });

    // Añade la clase 'active' al elemento clickeado
    var elementoActivo = document.getElementById(elementoID);
    if (elementoActivo) {
        elementoActivo.classList.add('active');
    }
}



