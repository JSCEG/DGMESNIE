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
    fillOpacity: 0.01, // Opacidad del relleno
    weight: 3 // Ancho de la línea
};

// Estilo para el hover
var highlightStyle = {
    color: '#FFDB2EC',
    fillColor: '#FFDB2E', // Color de relleno
    fillOpacity: 0.01, // Opacidad del relleno
    weight: 3
};

// Capa de estados
var estadosLayer = L.geoJSON(estados, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: function (feature, layer) {
        layer.bindTooltip('<div class="custom-tooltip">' + feature.properties.NOMGEO + '</div>');
        layer.on('click', function (e) {
            cargarMunicipios(feature.properties.CVE_ENT);
            mapas[0].fitBounds(layer.getBounds()); // Centra el mapa en el estado
        });
        // Efecto de hover
        layer.on('mouseover', function (e) {
            layer.setStyle(highlightStyle);
        });
        layer.on('mouseout', function (e) {
            estadosLayer.resetStyle(layer);
        });
    }
}).addTo(mapas[0]);


// Capa de municipios (inicialmente vacía)
var municipiosLayer = L.geoJSON(null, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: onEachMunicipio
}).addTo(mapas[0]);

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
        mapas[0].removeLayer(currentMarker);
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
            mapas[0].setView([lat, lon], 17);
            encontrado = true;
            break;
        }
    }

    // Si no se encontró por número de permiso, busca por entidad federativa
    if (!encontrado) {
        estadosLayer.eachLayer(function (layer) {
            if (layer.feature.properties.NOMGEO === terminoBuscado) {
                mapas[0].fitBounds(layer.getBounds());

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
                mapas[0].fitBounds(bounds);

                // Si ya había un municipio buscado anteriormente, lo elimina
                if (lastSearchedMunicipioLayer) {
                    mapas[0].removeLayer(lastSearchedMunicipioLayer);
                }

                // Agrega el municipio encontrado al mapa y lo resalta
                lastSearchedMunicipioLayer = L.geoJSON(municipio, {
                    style: {
                        color: '#FF0000',
                        fillColor: '#FF0000',
                        fillOpacity: 0.5
                    }
                }).addTo(mapas[0]);

                // Reiniciar el estilo y eliminar el municipio después de 5 segundos
                setTimeout(function () {
                    mapas[0].removeLayer(lastSearchedMunicipioLayer);
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
mapas[0].on('draw:created', function (e) {
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
        layer.addTo(mapas[0]);
    }
});



//Funciones de los botones y del Mapa*@


function limpiarMarcadores() {
    // Limpiar todas las capas de marcadores y círculos
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            mapas[0].removeLayer(layer);
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


//RyS
function CargaRyS() {
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
                url: '/Indicadores/GetCamposVisiblesRyS_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    console.log("Estos son los campos visibles:", camposVisibles); // ver la respuesta en consola
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
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosRyS_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Esta es la Infraestructura RyS:", response); // ver la respuesta en consola
                // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "RyS";
                });


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

                    if (camposVisiblesGlobal.includes("Nombre")) {
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
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/petroleo.png'; // Ícono por defecto si no hay coincidencia
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


                    if (camposVisiblesGlobal.includes("Nombre")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Latitud")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Longitud")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RFC")) {
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

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/petroleo.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;



                    //var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    //var contenidoPopup = generarContenidoPopup(coordenada);
                    //marker.bindPopup(contenidoPopup);
                    //markers.addLayer(marker);
                    if (coordenada.clasificacion === "RyS") {
                        var iconoActual;
                        // Aquí iría tu código para determinar el icono actual basado en tipoPermiso
                        //
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
                }

                mapas[0].addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = datosExpendios.length;
                te = datosExpendios.length;
                //teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalRyS = document.getElementById('total_rys');
                totalRyS.textContent = "Total: " + totalFormateado;

                // Ocultar los demás elementos
                //document.getElementById('total_petroliferos').style.display = 'none';
                //document.getElementById('total_glp').style.display = 'none';
                //document.getElementById('total_gn').style.display = 'none';

                //******Fin Tarjetas Totales******///



                ////////////////////////////////////////////////////////////////////




                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                //Highcharts.chart('grafico', options);
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


CargaRyS();
CargaCG();
CargaPI();

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
mapas[0].createPane('pane_ductos_petroliferos_4326_0');
mapas[0].getPane('pane_ductos_petroliferos_4326_0').style.zIndex = 400;
mapas[0].getPane('pane_ductos_petroliferos_4326_0').style['mix-blend-mode'] = 'normal';
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
mapas[0].addLayer(layer_ductos_petroliferos_4326_0);

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
mapas[0].createPane('pane_ductos_glp_4326_0');
mapas[0].getPane('pane_ductos_glp_4326_0').style.zIndex = 400;
mapas[0].getPane('pane_ductos_glp_4326_0').style['mix-blend-mode'] = 'normal';
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
mapas[0].addLayer(layer_ductos_glp_4326_0);

//Funcionalidades de Búsqueda y Menú*@

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




//Funciones del Mapa*@


$(document).ready(function () {
    // Obtener datos para el gráfico de Highcharts
    $.ajax({
        url: "/Map/Obtener_TiposRS",
        type: "GET",
        dataType: "json",
        success: function (data) {
            // Crear el gráfico con Highcharts
            Highcharts.chart('graficors', {
                chart: {
                    type: 'line' // o 'column', 'bar', etc.
                },
                title: {
                    text: 'Datos de Refinerías y Despuntadoras en (PJ) por Año'
                },
                xAxis: {
                    categories: data.map(item => item.año)
                },
                yAxis: {
                    title: {
                        text: 'Valores de Energía'
                    }
                },
                series: [

                    {
                        name: 'Refinerías y Despuntadoras en (PJ)',
                        data: data.map(item => item.refyDes)
                    }
                    // Añade más series según sea necesario
                ]
            });
        }
    });
    var table_mun = $('#Tipos').DataTable({
        lengthMenu: [[10, 50, 100, -1], [10, 50, 100, "Todos"]],
        dom: 'Blfrtip',
        buttons: [
            {
                extend: 'copyHtml5',
                title: 'Energeo - Refinerías y Despuntadoras'
            },
            {
                extend: 'excelHtml5',
                title: 'Energeo - Refinerías y Despuntadoras'
            },
            {
                extend: 'csvHtml5',
                title: 'Energeo - Refinerías y Despuntadoras'
            },
            {
                extend: 'pdfHtml5',
                title: 'Energeo - Refinerías y Despuntadoras',
                customize: function (doc) {
                    doc.styles.tableHeader.color = '#9fa1a4';
                    doc.defaultStyle.alignment = 'center';
                    doc.styles.tableHeader.fillColor = '#4c1922';
                }
            }
        ],
        "ajax": {
            "url": "/Map/Obtener_TiposRS", // Asegúrate de que la URL sea correcta
            "type": "GET",
            "datatype": "json",
            "dataSrc": function (json) {
                console.log(json); // Aquí se imprimirán los datos JSON en la consola
                return json; // Asegúrate de devolver los datos para que DataTables los pueda usar
            }
        },
        columns: [
            { data: 'año' }, // 'Año', asegúrate de manejar correctamente los caracteres especiales

            {
                data: 'refyDes',
                render: $.fn.dataTable.render.number(',', '.', 0)
            }

        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json'
        },
        // Otras opciones como columnDefs, etc.
    });
});

//DATOS DE PRUEBA
function CargaCG() {
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
                url: '/Indicadores/GetCamposVisiblesCG_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    console.log("Estos son los campos visibles:", camposVisibles); // ver la respuesta en consola
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
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosCG_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Complejos de gas:", response); // ver la respuesta en consola
                // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "CG";
                });

                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].razonSocial);
                    datosExpendiosAcumulados.push(datosExpendios[i].razonSocial);
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

                    if (camposVisiblesGlobal.includes("Nombre")) {
                        var imgSrc; // La URL de la imagen que se mostrará en el pop-up
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnatural.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/gasnatural.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Clave y Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDePermiso")) {
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


                    if (camposVisiblesGlobal.includes("Nombre")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Latitud")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Longitud")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RFC")) {
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

                    if (camposVisiblesGlobal.includes("Inicio_operaciones")) {
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

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/gasnatural.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;



                    //var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    //var contenidoPopup = generarContenidoPopup(coordenada);
                    //marker.bindPopup(contenidoPopup);
                    //markers.addLayer(marker);
                    if (coordenada.clasificacion === "CG") {
                        var iconoActual;
                        // Aquí iría tu código para determinar el icono actual basado en tipoPermiso
                        //
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
                        // Agrega el marcador directamente al mapa en lugar de agregarlo al grupo de marcadores
                        marker.addTo(mapas[0]);
                        // markers.addLayer(marker);
                    }
                }

                mapas[0].addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = datosExpendios.length;
                te = datosExpendios.length;
                //teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalCG = document.getElementById('total_cg');
                totalCG.textContent = "Total: " + totalFormateado;

                // Ocultar los demás elementos
                //document.getElementById('total_petroliferos').style.display = 'none';
                //document.getElementById('total_glp').style.display = 'none';
                //document.getElementById('total_gn').style.display = 'none';

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

function CargaPI() {
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
                url: '/Indicadores/GetCamposVisiblesPI_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    console.log("Estos son los campos visibles:", camposVisibles); // ver la respuesta en consola
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
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosPI_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Puntos de I.:", response); // ver la respuesta en consola
                // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "PI";
                });

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

                    if (camposVisiblesGlobal.includes("Nombre")) {
                        var imgSrc; // La URL de la imagen que se mostrará en el pop-up
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/s_disponible.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/s_disponible.png'; // Ícono por defecto si no hay coincidencia
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


                    if (camposVisiblesGlobal.includes("Nombre")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Latitud")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Longitud")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RFC")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>Fecha de Recepción:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoDePermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>Inicio de Vigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Localidad")) {
                        contenido += "<li><strong>Inicio  de Operaciones:</strong> " + handleNull(coordenada.localidadSalidaMex) + "</li>";
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

                    //  if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                    //                     contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    //                     } 
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

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/s_disponible.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;



                    //var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    //var contenidoPopup = generarContenidoPopup(coordenada);
                    //marker.bindPopup(contenidoPopup);
                    //markers.addLayer(marker);
                    if (coordenada.clasificacion === "PI") {
                        var iconoActual;
                        // Aquí iría tu código para determinar el icono actual basado en tipoPermiso
                        //
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
                        // Agrega el marcador directamente al mapa en lugar de agregarlo al grupo de marcadores
                        //marker.addTo(map);
                        markers.addLayer(marker);
                    }
                }

                mapas[0].addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'response'
                var totalElementos = datosExpendios.length;
                te = datosExpendios.length;
                //teg = response;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de elementos en 'response' formateado:", totalFormateado);

                // Mostrar este total formateado en el elemento span
                var totalPI = document.getElementById('total_pi');
                totalPI.textContent = "Total: " + totalFormateado;

                // Ocultar los demás elementos
                //document.getElementById('total_petroliferos').style.display = 'none';
                //document.getElementById('total_glp').style.display = 'none';
                //document.getElementById('total_gn').style.display = 'none';

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






