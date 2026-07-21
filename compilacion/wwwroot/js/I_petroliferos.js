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
function handleNull(value) {
    return value ? value : "S/D-Sin Dato";
}
Highcharts.setOptions({
    lang: {
        decimalPoint: '.',
        thousandsSep: ','
    }
});


//Petrolíferos
function CargaPetrolíferos() {
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
                                                                                </table>';
        layer.bindPopup(popupContent, { maxHeight: 400 });
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

                var divConteoTiposPermisos = document.getElementById('conteo_tipos_permisos');
                divConteoTiposPermisos.innerHTML = ''; // Limpiar el contenido existente
                divConteoTiposPermisos.appendChild(tabla);

                // Hacer visible el elemento 'total_petroliferos'
                document.getElementById('total_petroliferos').style.display = 'block'; // Si es un elemento de bloque


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

                    //Precios
                    const generarPrecioAleatorio = () => (Math.random() * (24 - 19) + 19).toFixed(2);

                    const determinarEstilo = (precio) => {
                        if (precio >= 23) {
                            return { color: "red", icono: "bi bi-arrow-up-circle-fill" }; // Caro
                        } else if (precio <= 20) {
                            return { color: "green", icono: "bi bi-arrow-down-circle-fill" }; // Económico
                        } else {
                            return { color: "orange", icono: "bi bi-exclamation-circle-fill" }; // Promedio
                        }
                    };

                    // Generar precios aleatorios
                    const precioRegular = parseFloat(generarPrecioAleatorio());
                    const precioPremium = parseFloat(generarPrecioAleatorio());
                    const precioDiesel = parseFloat(generarPrecioAleatorio());

                    // Determinar estilos
                    const estiloRegular = determinarEstilo(precioRegular);
                    const estiloPremium = determinarEstilo(precioPremium);
                    const estiloDiesel = determinarEstilo(precioDiesel);

                    // Mostrar precios solo si el rol es 1
                    if (window.userRole === 1) {
                        contenido += `
                            <ul>
                            <li class="popup-precio" style="color: ${estiloRegular.color}">
                                <i class="${estiloRegular.icono}" style="margin-right: 5px;"></i>
                                <strong>Regular:</strong> $${precioRegular} MXN
                            </li>
                            <li class="popup-precio" style="color: ${estiloPremium.color}">
                                <i class="${estiloPremium.icono}" style="margin-right: 5px;"></i>
                                <strong>Premium:</strong> $${precioPremium} MXN
                            </li>
                            <li class="popup-precio" style="color: ${estiloDiesel.color}">
                                <i class="${estiloDiesel.icono}" style="margin-right: 5px;"></i>
                                <strong>Diesel:</strong> $${precioDiesel} MXN
                            </li>
                        </ul>
                                `;
                    }


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
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPetroliferos = document.getElementById('total_petroliferos');
                totalPetroliferos.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos





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

CargaPetrolíferos();
