//Campos Visbles de los popup*@

//



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
    color: '#222', // Color de línea (negro/gris oscuro)
    fillColor: '#222', // Color de relleno (gris oscuro, opacidad baja)
    fillOpacity: 0.01, // Opacidad del relleno
    weight: 3 // Ancho de la línea
};

// Estilo para el hover
var highlightStyle = {
    color: '#555', // Gris medio para hover
    fillColor: '#555', // Gris medio para hover
    fillOpacity: 0.01,
    weight: 3
};

// Se eliminó el marcador de vista de calle al hacer clic en el mapa por solicitud del usuario.

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


//Electricidad
function CargaElectricidad() {
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
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de Electricidad:", response); // ver la respuesta en consola
                // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "Solar Fotovoltaica";
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

                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        var imgSrc; // La URL de la imagen que se mostrará en el pop-up
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png'; // Ícono por defecto si no hay coincidencia
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

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png' });
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
                    if (coordenada.clasificacion === "Solar Fotovoltaica") {
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
                var totalElectricidad = document.getElementById('total_electricidad');
                totalElectricidad.textContent = "Total de Permisos: " + totalFormateado;

                // Ocultar los demás elementos
                //document.getElementById('total_petroliferos').style.display = 'none';
                //document.getElementById('total_glp').style.display = 'none';
                //document.getElementById('total_gn').style.display = 'none';

                //******Fin Tarjetas Totales******///

                // Función para procesar los datos
                function processData(response, key) {
                    var counts = {};
                    response.forEach(function (item) {
                        // Elimina los números al principio si el key es 'efId'
                        if (key === 'efId') {
                            item[key] = item[key].replace(/^\d+\s/, '');
                        }

                        if (!counts[item[key]]) {
                            counts[item[key]] = 0;
                        }
                        counts[item[key]]++;
                    });

                    var categories = [];
                    var data = [];
                    for (var entidad in counts) {
                        categories.push(entidad);
                        data.push(counts[entidad]);
                    }

                    return { categories, data };
                }
                var convencional = processData(datosExpendios, 'efId');
                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                datosExpendios.forEach(function (coordenada) {
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
                        categories: convencional.categories
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


CargaElectricidad();



//Funcionalidades de Búsqueda y Menú*@



// Función para inicializar el autocompletar (solo una vez, limpia listeners previos)
function autocomplete(inp, arr) {
    // Remueve listeners previos usando clonación del nodo
    var oldInput = inp;
    var newInput = oldInput.cloneNode(true);
    oldInput.parentNode.replaceChild(newInput, oldInput);
    inp = newInput;

    var currentFocus;
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        a = document.getElementById("autocomplete-list");
        if (!a) {
            a = document.createElement("div");
            a.setAttribute("id", "autocomplete-list");
            a.setAttribute("class", "autocomplete-items");
            inp.parentNode.appendChild(a);
        }
        a.innerHTML = "";
        for (i = 0; i < arr.length; i++) {
            if (arr[i].substr(0, val.length).toUpperCase() === val.toUpperCase()) {
                b = document.createElement("DIV");
                b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                b.innerHTML += arr[i].substr(val.length);
                b.addEventListener("click", function (e) {
                    inp.value = this.innerText;
                    closeAllLists();
                    buscarGeneral();
                });
                a.appendChild(b);
            }
        }
    });
    function closeAllLists(elmnt) {
        var x = document.getElementById("autocomplete-list");
        if (x && elmnt != x && elmnt != inp) {
            x.innerHTML = "";
        }
    }
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

// Inicializa el autocompletar al cargar la página
setTimeout(function () {
    autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
}, 500);

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




//Capa de radiación*@


// --- Definición de capas raster desde CDN ---
var rasterBaseLayers = {};

// Potencial Fotovoltaico (por defecto)
rasterBaseLayers["🌞 Potencial Fotovoltaico"] = L.imageOverlay(
    'https://cdn.sassoapps.com/Geovisualizador/rasters/potencialfotovoltaico_4326_0.png',
    L.latLngBounds([32.85, -118.5167], [14.40, -86.5667])
);

// Radiación Horizontal
rasterBaseLayers["☀️ Radiación Horizontal"] = L.imageOverlay(
    'https://cdn.sassoapps.com/Geovisualizador/rasters/radiacionhorizontal_4326_0.png',
    L.latLngBounds([32.85, -118.5175], [14.40, -86.5675])
);

// Velocidad del Viento por alturas
var vientoConfig = [
    { altura: 10, pane: 'pane_Velocidaddevientoa10mdealtura_4', zIndex: 200, sufijo: 4 },
    { altura: 50, pane: 'pane_Velocidaddevientoa50mdealtura_3', zIndex: 200, sufijo: 3 },
    { altura: 100, pane: 'pane_Velocidaddevientoa100mdealtura_2', zIndex: 200, sufijo: 2 },
    { altura: 150, pane: 'pane_Velocidaddevientoa150mdealtura_1', zIndex: 200, sufijo: 1 },
    { altura: 200, pane: 'pane_Velocidaddevientoa200mdealtura_0', zIndex: 200, sufijo: 0 }
];
var vientoBounds = [[12.103268747733761, -122.18342731781249], [32.71826874773478, -84.64092731781062]];
vientoConfig.forEach(function (cfg) {
    mapas[0].createPane(cfg.pane);
    mapas[0].getPane(cfg.pane).style.zIndex = cfg.zIndex;
    // Usar el sufijo correcto para el PNG de cada altura
    rasterBaseLayers["💨 Viento " + cfg.altura + "m"] = L.imageOverlay(
        'https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa' + cfg.altura + 'mdealtura_' + cfg.sufijo + '.png',
        vientoBounds,
        { pane: cfg.pane }
    );
});

// --- Control de capas ---

var control = L.control.layers(rasterBaseLayers, null, { collapsed: false, position: 'topright' }).addTo(mapas[0]);

// --- Funciones para cargar y limpiar marcadores ---
function limpiarMarcadores() {
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            mapas[0].removeLayer(layer);
        }
    });
}

function cargarMarcadoresFotovoltaico() {
    // Limpiar input del buscador
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    // Lógica original de marcadores fotovoltaico
    if (typeof CargaElectricidad === 'function') {
        CargaElectricidad();
        // Actualiza el autocompletado después de cargar datos
        setTimeout(function () {
            autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
        }, 500);
    }
}

function cargarMarcadoresViento() {
    // Limpiar input del buscador
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    // Lógica de marcadores, buscador y gráfico para eólica
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) {
            availableTerms.push(layer.feature.properties.NOMGEO);
        }
    });
    for (var i = 0; i < municipios_mapa.features.length; i++) {
        var municipio = municipios_mapa.features[i].properties.NOM_MUN;
        var estado = municipios_mapa.features[i].properties.NOMGEO;
        if (municipio && estado) {
            var nombreCompleto = municipio + ", " + estado;
            availableTerms.push(nombreCompleto);
        }
    }

    // AJAX para campos visibles
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);
                }
            });
        });
    }

    cargarCamposVisibles().then(camposVisibles => {
        limpiarMarcadores();
        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            contentType: 'application/json',
            success: function (response) {
                // Solo Eólica
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "Eólica";
                });
                // Limpiar availableTerms y solo agregar los de viento
                availableTerms = [];
                estadosLayer.eachLayer(function (layer) {
                    if (layer.feature.properties.NOMGEO) {
                        availableTerms.push(layer.feature.properties.NOMGEO);
                    }
                });
                for (var i = 0; i < municipios_mapa.features.length; i++) {
                    var municipio = municipios_mapa.features[i].properties.NOM_MUN;
                    var estado = municipios_mapa.features[i].properties.NOMGEO;
                    if (municipio && estado) {
                        var nombreCompleto = municipio + ", " + estado;
                        availableTerms.push(nombreCompleto);
                    }
                }
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                // --- AGREGAR MARCADORES DE VIENTO AL MAPA ---
                var markers = L.markerClusterGroup();
                var iconoBase = L.Icon.extend({
                    options: {
                        iconSize: [36, 36],
                        iconAnchor: [12, 18],
                        popupAnchor: [0, -26]
                    }
                });
                var iconoEolica = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/eolica.png' });
                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>"
                        + ".popup-content { width: 280px; max-height: 180px; overflow-y: auto; padding: 10px; }"
                        + "h2, h3, h4, p, li { margin: 0 0 10px 0; }"
                        + "ul { padding-left: 20px; }"
                        + "img { vertical-align: middle; margin-right: 10px; }"
                        + "</style>";
                    contenido += "<div class='popup-content'>";
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<h2 class='subtitulo'><img src='https://cdn.sassoapps.com/img_snier/vistas/eolica.png' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }
                    contenido += "<ul>";
                    if (camposVisiblesGlobal.includes("EfId")) {
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
                    contenido += "</ul>";
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }
                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";
                    contenido += "</div>";
                    return contenido;
                }
                for (var j = 0; j < datosExpendios.length; j++) {
                    var coordenada = datosExpendios[j];
                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoEolica });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);
                    markers.addLayer(marker);
                }
                mapas[0].addLayer(markers);
                // --- FIN AGREGAR MARCADORES DE VIENTO ---

                setTimeout(function () {
                    autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
                }, 500);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });
}


// --- Control de opacidad para raster activo ---
var rasterActivo = rasterBaseLayers["🌞 Potencial Fotovoltaico"];

// Crear el control visual (slider)
var opacityControl = L.control({ position: 'topright' });
opacityControl.onAdd = function (map) {
    var div = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
    div.style.background = '#fff';
    div.style.padding = '6px 10px 2px 10px';
    div.style.width = '160px';
    div.innerHTML = '<label style="font-size:12px;">Opacidad raster</label><br><input id="raster-opacity-slider" type="range" min="0" max="1" step="0.01" value="1" style="width:130px;">';
    return div;
};
opacityControl.addTo(mapas[0]);

// Evento para cambiar opacidad
setTimeout(function () {
    var slider = document.getElementById('raster-opacity-slider');
    if (slider) {
        slider.addEventListener('input', function () {
            if (rasterActivo && typeof rasterActivo.setOpacity === 'function') {
                rasterActivo.setOpacity(parseFloat(this.value));
            }
        });
    }
}, 500);

// Cargar capa y marcadores por defecto
rasterActivo.addTo(mapas[0]);
cargarMarcadoresFotovoltaico();

// --- Evento para cambio de capa raster ---
mapas[0].on('baselayerchange', function (e) {
    limpiarMarcadores();
    // Quitar opacidad al raster anterior
    if (rasterActivo && typeof rasterActivo.setOpacity === 'function') {
        rasterActivo.setOpacity(1);
    }
    rasterActivo = e.layer;
    // Sincronizar slider con la opacidad actual
    var slider = document.getElementById('raster-opacity-slider');
    if (slider && rasterActivo && typeof rasterActivo.options.opacity !== 'undefined') {
        slider.value = rasterActivo.options.opacity;
    } else if (slider) {
        slider.value = 1;
    }

    // Oculta todas las leyendas y rampas (solo si existen)
    var tablas = [
        'simbol-fotovoltaico', 'rampa-fotovoltaico',
        'simbol-radiacion', 'rampa-radiacion',
        'simbol-viento', 'rampa-viento'
    ];
    tablas.forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.style.display = 'none';
    });

    // Determina qué leyenda/rampa mostrar (solo si existen)
    if (e.name === '🌞 Potencial Fotovoltaico') {
        var t = document.getElementById('simbol-fotovoltaico');
        var r = document.getElementById('rampa-fotovoltaico');
        if (t) t.style.display = '';
        if (r) r.style.display = '';
    } else if (e.name === '☀️ Radiación Horizontal') {
        var t = document.getElementById('simbol-radiacion');
        var r = document.getElementById('rampa-radiacion');
        if (t) t.style.display = '';
        if (r) r.style.display = '';
    } else if (e.name && e.name.startsWith('💨 Viento')) {
        var t = document.getElementById('simbol-viento');
        var r = document.getElementById('rampa-viento');
        if (t) t.style.display = '';
        if (r) r.style.display = '';
    }

    // Limpiar input del buscador y términos
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    availableTerms = [];

    if (e.name === "🌞 Potencial Fotovoltaico" || e.name === "☀️ Radiación Horizontal") {
        cargarMarcadoresFotovoltaico();
    } else if (e.name && e.name.startsWith("💨 Viento")) {
        cargarMarcadoresViento();
    }
    // El autocompletado se actualiza dentro de las funciones de carga
});