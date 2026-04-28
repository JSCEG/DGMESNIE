// --- Botón personalizado para centrar el mapa (Home) ---
// function agregarBotonCentrarMapa() {
//     if (!window.L || !mapas || !mapas[0]) return;
//     var homeControl = L.Control.extend({
//         options: { position: 'topleft' },
//         onAdd: function (map) {
//             var container = L.DomUtil.create('div', 'leaflet-control leaflet-bar leaflet-control-custom');
//             container.title = 'Centrar mapa';
//             container.innerHTML = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="12" x2="16" y2="12"/></svg>';
//             container.style.cursor = 'pointer';
//             container.onclick = function () {
//                 // Ajusta los valores a la vista inicial de tu mapa
//                 map.setView([23.6345, -102.5528], 5); // México
//             };
//             return container;
//         }
//     });
//     mapas[0].addControl(new homeControl());
// }
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

console.log("Estados: ", estados);
console.log("Estados Layer: ", estadosLayer);

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

const capasConPermisos = [
    "🌞 Potencial Fotovoltaico",
    "☀️ Radiación Horizontal",
    "💨 Viento 10m",
    "💨 Viento 50m",
    "💨 Viento 100m",
    "💨 Viento 150m",
    "💨 Viento 200m"
];

function hayCapaConPermisosActiva() {
    return capasConPermisos.some(nombre => {
        const capa = rasterBaseLayers[nombre];
        return mapas[0].hasLayer(capa);
    });
}

function actualizarPlaceholderBusqueda(tipo) {
    const input = document.getElementById("busquedaGeneralInput");

    if (tipo === 'SINPERMISO') {
        input.placeholder = "Entidad Federativa o Municipio";
    } else {
        input.placeholder = "Número de Permiso, Entidad Federativa o Municipio";
    }
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
    if (hayCapaConPermisosActiva()) {
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
    } else {
        console.log("Búsqueda de permisos deshabilitada: no hay capas con permisos activas.");
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
                                imgSrc = proxyImg;
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = proxyImg; // Ícono por defecto si no hay coincidencia
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

                var iconoExpendio = new iconoBase({ iconUrl: proxyImg });
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

                // Contar el número total de elementos en el array 'datosExpendios'
                var totalElementos = datosExpendios.length;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de permisos (Solar Fotovoltaica) formateado:", totalFormateado);

                // Actualizar la tarjeta de permisos
                const tarjetaTitulo = document.getElementById('titulo-permiso');
                const tarjetaTotal = document.getElementById('total-permisos');
                const tarjetaIcono = document.getElementById('icono-permiso');

                if (tarjetaTitulo) tarjetaTitulo.textContent = 'Solar Fotovoltaica';
                if (tarjetaIcono) tarjetaIcono.src = proxyImg;
                if (tarjetaTotal) tarjetaTotal.textContent = "Proyectos Autorizados: " + totalFormateado;


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
                        backgroundColor: 'transparent',
                        style: {
                            fontFamily: 'inherit'
                        },
                        borderRadius: 10
                    },
                    title: {
                        text: 'Total de Proyectos Autorizados por Entidad Federativa',
                        style: {
                            color: '#333',
                            fontSize: '18px',
                            fontWeight: 'bold'
                        }
                    },
                    subtitle: {
                        text: 'Fuente: Solar Fotovoltaica',
                        style: {
                            color: '#666'
                        }
                    },
                    xAxis: {
                        categories: convencional.categories,
                        labels: {
                            style: {
                                color: '#666'
                            }
                        }
                    },
                    yAxis: {
                        title: {
                            text: 'Número de Proyectos',
                            style: {
                                color: '#666'
                            }
                        },
                        gridLineColor: '#e6e6e6'
                    },
                    series: [{
                        name: 'Proyectos Autorizados',
                        data: dataPermisos,
                        color: {
                            linearGradient: { x1: 0, x2: 0, y1: 0, y2: 1 },
                            stops: [
                                [0, '#4facfe'],
                                [1, '#00f2fe']
                            ]
                        },
                        dataLabels: {
                            enabled: true,
                            rotation: 0,
                            color: '#333',
                            align: 'center',
                            format: '{point.y:,.0f}',
                            y: -15,
                            style: {
                                fontSize: '11px',
                                fontWeight: 'bold'
                            },
                            backgroundColor: 'rgba(255, 255, 255, 0.7)',
                            padding: 3,
                            borderRadius: 3
                        }
                    }],
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        style: {
                            color: '#f0f0f0'
                        },
                        headerFormat: '<span style="font-size: 12px">{point.key}</span><br/>',
                        pointFormat: '<span style="color:{series.color}">●</span> {series.name}: <b>{point.y:,.0f}</b>'
                    },
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 1000
                            },
                            states: {
                                hover: {
                                    brightness: -0.1
                                }
                            },
                            borderRadius: 5
                        }
                    },
                    credits: {
                        enabled: false
                    }
                };


                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);
                // Usamos un objeto Set para filtrar los duplicados, ya que un Set solo permite valores únicos
                var uniqueTerms = [...new Set(availableTerms)];

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), uniqueTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}


//Funcionalidades de Búsqueda y Menú*@



CargaElectricidad();



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

        // Verificar si hay capas con permisos activas
        var permitirPermisos = hayCapaConPermisosActiva();

        for (i = 0; i < arr.length; i++) {
            var item = arr[i];

            // Si no hay capas activas con permisos, ignorar sugerencias que parecen ser permisos
            var esPermiso = /^E\/\d+\/[A-Z]+\/\d{2,}$/.test(item);
            if (!permitirPermisos && esPermiso) {
                continue; // Salta esta sugerencia
            }

            if (item.substr(0, val.length).toUpperCase() === val.toUpperCase()) {
                b = document.createElement("DIV");
                b.innerHTML = "<strong>" + item.substr(0, val.length) + "</strong>";
                b.innerHTML += item.substr(val.length);
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

//Capa de radiación*@


// --- Definición de capas raster desde CDN ---
var rasterBaseLayers = {};

//Capas de parques fotoeolicos
function pop_Parque_Solar_2022_0(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Nombre: </th>\
                <td>' + (feature.properties['Nombre'] !== null ? autolinker.link(String(feature.properties['Nombre']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Estado: </th>\
                <td>' + (feature.properties['Estado'] !== null ? autolinker.link(String(feature.properties['Estado']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_Parque_Solar_2022_0_0() {
    return {
        pane: 'pane_Parque_Solar_2022_0',
        opacity: 1,
        color: 'rgba(127,26,28,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,0,0,1.0)',
        interactive: true,
    }
}
mapas[0].createPane('pane_Parque_Solar_2022_0');
mapas[0].getPane('pane_Parque_Solar_2022_0').style.zIndex = 400;
mapas[0].getPane('pane_Parque_Solar_2022_0').style['mix-blend-mode'] = 'normal';
var layer_Parque_Solar_2022_0 = new L.geoJson(json_Parque_Solar_2022_0, {
    attribution: '',
    interactive: true,
    dataVar: 'json_Parque_Solar_2022_0',
    layerName: 'layer_Parque_Solar_2022_0',
    pane: 'pane_Parque_Solar_2022_0',
    onEachFeature: pop_Parque_Solar_2022_0,
    style: style_Parque_Solar_2022_0_0,
});

function pop_Parques_Eolicos_Poli_2023_1(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Proyecto: </th>\
                <td>' + (feature.properties['Proyecto'] !== null ? autolinker.link(String(feature.properties['Proyecto']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Municipio: </th>\
                <td>' + (feature.properties['Municipio'] !== null ? autolinker.link(String(feature.properties['Municipio']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Estado: </th>\
                <td>' + (feature.properties['Estado'] !== null ? autolinker.link(String(feature.properties['Estado']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_Parques_Eolicos_Poli_2023_1_0() {
    return {
        pane: 'pane_Parques_Eolicos_Poli_2023_1',
        opacity: 1,
        color: 'rgba(13,74,115,1.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(64,133,180,1.0)',
        interactive: true,
    }
}
mapas[0].createPane('pane_Parques_Eolicos_Poli_2023_1');
mapas[0].getPane('pane_Parques_Eolicos_Poli_2023_1').style.zIndex = 401;
mapas[0].getPane('pane_Parques_Eolicos_Poli_2023_1').style['mix-blend-mode'] = 'normal';
var layer_Parques_Eolicos_Poli_2023_1 = new L.geoJson(json_Parques_Eolicos_Poli_2023_1, {
    attribution: '',
    interactive: true,
    dataVar: 'json_Parques_Eolicos_Poli_2023_1',
    layerName: 'layer_Parques_Eolicos_Poli_2023_1',
    pane: 'pane_Parques_Eolicos_Poli_2023_1',
    onEachFeature: pop_Parques_Eolicos_Poli_2023_1,
    style: style_Parques_Eolicos_Poli_2023_1_0,
});

// Potencial Fotovoltaico (por defecto)
var capaPotencialFotovoltaico = L.imageOverlay(
    proxyUrl,
    L.latLngBounds([32.85, -118.5167], [14.40, -86.5667])
);

var grupoSolar = L.layerGroup([
    capaPotencialFotovoltaico,
    layer_Parque_Solar_2022_0
]);

rasterBaseLayers["🌞 Potencial Fotovoltaico"] = grupoSolar;


// Radiación Horizontal
var capaRadiacionHorizontal = L.imageOverlay(
    proxyRadioUrl,
    L.latLngBounds([32.85, -118.5175], [14.40, -86.5675])
);

var grupoRadiacion = L.layerGroup([
    capaRadiacionHorizontal,
    layer_Parque_Solar_2022_0
]);

rasterBaseLayers["☀️ Radiación Horizontal"] = grupoRadiacion;


// Velocidad del Viento por alturas
var vientoConfig = [
    { altura: 10, pane: 'pane_Velocidaddevientoa10mdealtura_4', zIndex: 200, sufijo: 4 },
    { altura: 50, pane: 'pane_Velocidaddevientoa50mdealtura_3', zIndex: 200, sufijo: 3 },
    { altura: 100, pane: 'pane_Velocidaddevientoa100mdealtura_2', zIndex: 200, sufijo: 2 },
    { altura: 150, pane: 'pane_Velocidaddevientoa150mdealtura_1', zIndex: 200, sufijo: 1 },
    { altura: 200, pane: 'pane_Velocidaddevientoa200mdealtura_0', zIndex: 200, sufijo: 0 }
];
var capasViento = [];
var vientoBounds = [[12.103268747733761, -122.18342731781249], [32.71826874773478, -84.64092731781062]];
vientoConfig.forEach(function (cfg) {
    mapas[0].createPane(cfg.pane);
    mapas[0].getPane(cfg.pane).style.zIndex = cfg.zIndex;

    // Construir la URL del raster de viento
    var imagenOriginal = 'https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa' + cfg.altura + 'mdealtura_' + cfg.sufijo + '.png';

    // Construir la URL del proxy
    var imagenProxy = '/Atlas/ProxyImagen?url=' + encodeURIComponent(imagenOriginal);

    var capaRaster = L.imageOverlay(imagenProxy, vientoBounds, { pane: cfg.pane });

    // Grupo que une la imagen y la capa de parques eólicos
    var grupoConVector = L.layerGroup([
        capaRaster,
        layer_Parques_Eolicos_Poli_2023_1
    ]);

    // Sobrescribimos o creamos la capa en rasterBaseLayers
    rasterBaseLayers["💨 Viento " + cfg.altura + "m"] = grupoConVector;
});


//RESIDUOS INDUSTRIALES
function pop_ResiduosindustrialesEscenario3_13(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosindustrialesEscenario3_13_0(feature) {
    if (feature.properties['GENE_GWha'] >= 0.351620 && feature.properties['GENE_GWha'] <= 0.541403 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario3_13',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 0.541403 && feature.properties['GENE_GWha'] <= 0.832524 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario3_13',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 0.832524 && feature.properties['GENE_GWha'] <= 1.364683 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario3_13',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 1.364683 && feature.properties['GENE_GWha'] <= 3.364898 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario3_13',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 3.364898 && feature.properties['GENE_GWha'] <= 183.417283 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario3_13',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosindustrialesEscenario3_13');
mapas[0].getPane('pane_ResiduosindustrialesEscenario3_13').style.zIndex = 413;
mapas[0].getPane('pane_ResiduosindustrialesEscenario3_13').style['mix-blend-mode'] = 'normal';
var layer_ResiduosindustrialesEscenario3_13 = new L.geoJson(json_ResiduosindustrialesEscenario3_13, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosindustrialesEscenario3_13',
    layerName: 'layer_ResiduosindustrialesEscenario3_13',
    pane: 'pane_ResiduosindustrialesEscenario3_13',
    onEachFeature: pop_ResiduosindustrialesEscenario3_13,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosindustrialesEscenario3_13_0(feature));
    },
});
function pop_ResiduosindustrialesEscenario2_14(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <td colspan="2">' + (feature.properties['ENERGIA'] !== null ? autolinker.link(String(feature.properties['ENERGIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <td colspan="2">' + (feature.properties['CLASIFICAC'] !== null ? autolinker.link(String(feature.properties['CLASIFICAC']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <td colspan="2">' + (feature.properties['FUENTE'] !== null ? autolinker.link(String(feature.properties['FUENTE']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosindustrialesEscenario2_14_0(feature) {
    if (feature.properties['GENE_GWha'] >= 3.553095 && feature.properties['GENE_GWha'] <= 4.238169 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario2_14',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 4.238169 && feature.properties['GENE_GWha'] <= 7.186916 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario2_14',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 7.186916 && feature.properties['GENE_GWha'] <= 10.369197 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario2_14',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 10.369197 && feature.properties['GENE_GWha'] <= 21.767711 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario2_14',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 21.767711 && feature.properties['GENE_GWha'] <= 183.417283 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario2_14',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosindustrialesEscenario2_14');
mapas[0].getPane('pane_ResiduosindustrialesEscenario2_14').style.zIndex = 414;
mapas[0].getPane('pane_ResiduosindustrialesEscenario2_14').style['mix-blend-mode'] = 'normal';
var layer_ResiduosindustrialesEscenario2_14 = new L.geoJson(json_ResiduosindustrialesEscenario2_14, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosindustrialesEscenario2_14',
    layerName: 'layer_ResiduosindustrialesEscenario2_14',
    pane: 'pane_ResiduosindustrialesEscenario2_14',
    onEachFeature: pop_ResiduosindustrialesEscenario2_14,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosindustrialesEscenario2_14_0(feature));
    },
});
function pop_ResiduosindustrialesEscenario1_15(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosindustrialesEscenario1_15_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.038981 && feature.properties['GENE_GWha'] <= 8.282911 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario1_15',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 8.282911 && feature.properties['GENE_GWha'] <= 11.129800 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario1_15',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 11.129800 && feature.properties['GENE_GWha'] <= 15.704259 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario1_15',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 15.704259 && feature.properties['GENE_GWha'] <= 31.249586 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario1_15',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 31.249586 && feature.properties['GENE_GWha'] <= 183.417283 ) {
        return {
        pane: 'pane_ResiduosindustrialesEscenario1_15',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(35,35,35,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(155,155,155,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosindustrialesEscenario1_15');
mapas[0].getPane('pane_ResiduosindustrialesEscenario1_15').style.zIndex = 415;
mapas[0].getPane('pane_ResiduosindustrialesEscenario1_15').style['mix-blend-mode'] = 'normal';
var layer_ResiduosindustrialesEscenario1_15 = new L.geoJson(json_ResiduosindustrialesEscenario1_15, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosindustrialesEscenario1_15',
    layerName: 'layer_ResiduosindustrialesEscenario1_15',
    pane: 'pane_ResiduosindustrialesEscenario1_15',
    onEachFeature: pop_ResiduosindustrialesEscenario1_15,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosindustrialesEscenario1_15_0(feature));
    },
});

var residuosIndustrialesGroup = L.layerGroup([
    layer_ResiduosindustrialesEscenario1_15,
    layer_ResiduosindustrialesEscenario2_14,
    layer_ResiduosindustrialesEscenario3_13
]);

// Agregar a objeto de capas personalizadas
rasterBaseLayers["🛢️ Residuos Industriales"] = residuosIndustrialesGroup;


//RESIDUOS PECUARIOS
function pop_ResiduospecuariosEscenario3_10(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduospecuariosEscenario3_10_0(feature) {
    if (feature.properties['GENE_GWha'] >= 0.420988 && feature.properties['GENE_GWha'] <= 0.793059 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario3_10',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 0.793059 && feature.properties['GENE_GWha'] <= 1.130691 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario3_10',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 1.130691 && feature.properties['GENE_GWha'] <= 1.630576 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario3_10',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 1.630576 && feature.properties['GENE_GWha'] <= 2.871240 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario3_10',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 2.871240 && feature.properties['GENE_GWha'] <= 42.981217 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario3_10',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduospecuariosEscenario3_10');
mapas[0].getPane('pane_ResiduospecuariosEscenario3_10').style.zIndex = 410;
mapas[0].getPane('pane_ResiduospecuariosEscenario3_10').style['mix-blend-mode'] = 'normal';
var layer_ResiduospecuariosEscenario3_10 = new L.geoJson(json_ResiduospecuariosEscenario3_10, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduospecuariosEscenario3_10',
    layerName: 'layer_ResiduospecuariosEscenario3_10',
    pane: 'pane_ResiduospecuariosEscenario3_10',
    onEachFeature: pop_ResiduospecuariosEscenario3_10,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduospecuariosEscenario3_10_0(feature));
    },
});
function pop_ResiduospecuariosEscenario2_11(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduospecuariosEscenario2_11_0(feature) {
    if (feature.properties['GENE_GWha'] >= 3.555643 && feature.properties['GENE_GWha'] <= 3.807949 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario2_11',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 3.807949 && feature.properties['GENE_GWha'] <= 4.570000 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario2_11',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 4.570000 && feature.properties['GENE_GWha'] <= 5.411296 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario2_11',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 5.411296 && feature.properties['GENE_GWha'] <= 8.249838 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario2_11',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 8.249838 && feature.properties['GENE_GWha'] <= 42.981217 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario2_11',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduospecuariosEscenario2_11');
mapas[0].getPane('pane_ResiduospecuariosEscenario2_11').style.zIndex = 411;
mapas[0].getPane('pane_ResiduospecuariosEscenario2_11').style['mix-blend-mode'] = 'normal';
var layer_ResiduospecuariosEscenario2_11 = new L.geoJson(json_ResiduospecuariosEscenario2_11, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduospecuariosEscenario2_11',
    layerName: 'layer_ResiduospecuariosEscenario2_11',
    pane: 'pane_ResiduospecuariosEscenario2_11',
    onEachFeature: pop_ResiduospecuariosEscenario2_11,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduospecuariosEscenario2_11_0(feature));
    },
});
function pop_ResiduospecuariosEscenario1_12(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduospecuariosEscenario1_12_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.655664 && feature.properties['GENE_GWha'] <= 8.176279 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario1_12',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 8.176279 && feature.properties['GENE_GWha'] <= 8.891575 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario1_12',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 8.891575 && feature.properties['GENE_GWha'] <= 9.987827 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario1_12',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 9.987827 && feature.properties['GENE_GWha'] <= 11.988025 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario1_12',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 11.988025 && feature.properties['GENE_GWha'] <= 42.981217 ) {
        return {
        pane: 'pane_ResiduospecuariosEscenario1_12',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,165,0,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduospecuariosEscenario1_12');
mapas[0].getPane('pane_ResiduospecuariosEscenario1_12').style.zIndex = 412;
mapas[0].getPane('pane_ResiduospecuariosEscenario1_12').style['mix-blend-mode'] = 'normal';
var layer_ResiduospecuariosEscenario1_12 = new L.geoJson(json_ResiduospecuariosEscenario1_12, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduospecuariosEscenario1_12',
    layerName: 'layer_ResiduospecuariosEscenario1_12',
    pane: 'pane_ResiduospecuariosEscenario1_12',
    onEachFeature: pop_ResiduospecuariosEscenario1_12,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduospecuariosEscenario1_12_0(feature));
    },
});

var residuosPecuariosGroup = L.layerGroup([
    layer_ResiduospecuariosEscenario1_12,
    layer_ResiduospecuariosEscenario2_11,
    layer_ResiduospecuariosEscenario3_10
]);

// Agregar a objeto de capas personalizadas
rasterBaseLayers["🐄 Residuos Pecuarios"] = residuosPecuariosGroup;


//RESIDUOS URBANOS
function pop_ResiduosurbanosEscenario3_7(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosurbanosEscenario3_7_0(feature) {
    if (feature.properties['GENE_GWha'] >= 0.421716 && feature.properties['GENE_GWha'] <= 0.741142 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario3_7',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 0.741142 && feature.properties['GENE_GWha'] <= 1.417616 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario3_7',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 1.417616 && feature.properties['GENE_GWha'] <= 3.587334 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario3_7',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 3.587334 && feature.properties['GENE_GWha'] <= 8.554866 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario3_7',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 8.554866 && feature.properties['GENE_GWha'] <= 154.616190 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario3_7',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosurbanosEscenario3_7');
mapas[0].getPane('pane_ResiduosurbanosEscenario3_7').style.zIndex = 407;
mapas[0].getPane('pane_ResiduosurbanosEscenario3_7').style['mix-blend-mode'] = 'normal';
var layer_ResiduosurbanosEscenario3_7 = new L.geoJson(json_ResiduosurbanosEscenario3_7, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosurbanosEscenario3_7',
    layerName: 'layer_ResiduosurbanosEscenario3_7',
    pane: 'pane_ResiduosurbanosEscenario3_7',
    onEachFeature: pop_ResiduosurbanosEscenario3_7,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosurbanosEscenario3_7_0(feature));
    },
});
function pop_ResiduosurbanosEscenario2_8(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosurbanosEscenario2_8_0(feature) {
    if (feature.properties['GENE_GWha'] >= 3.585614 && feature.properties['GENE_GWha'] <= 4.481243 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario2_8',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 4.481243 && feature.properties['GENE_GWha'] <= 6.785925 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario2_8',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 6.785925 && feature.properties['GENE_GWha'] <= 10.886466 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario2_8',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 10.886466 && feature.properties['GENE_GWha'] <= 24.665172 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario2_8',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 24.665172 && feature.properties['GENE_GWha'] <= 154.616190 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario2_8',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosurbanosEscenario2_8');
mapas[0].getPane('pane_ResiduosurbanosEscenario2_8').style.zIndex = 408;
mapas[0].getPane('pane_ResiduosurbanosEscenario2_8').style['mix-blend-mode'] = 'normal';
var layer_ResiduosurbanosEscenario2_8 = new L.geoJson(json_ResiduosurbanosEscenario2_8, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosurbanosEscenario2_8',
    layerName: 'layer_ResiduosurbanosEscenario2_8',
    pane: 'pane_ResiduosurbanosEscenario2_8',
    onEachFeature: pop_ResiduosurbanosEscenario2_8,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosurbanosEscenario2_8_0(feature));
    },
});
function pop_ResiduosurbanosEscenario1_9(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosurbanosEscenario1_9_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.026649 && feature.properties['GENE_GWha'] <= 9.732896 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario1_9',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 9.732896 && feature.properties['GENE_GWha'] <= 12.663554 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario1_9',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 12.663554 && feature.properties['GENE_GWha'] <= 22.052249 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario1_9',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 22.052249 && feature.properties['GENE_GWha'] <= 33.563400 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario1_9',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 33.563400 && feature.properties['GENE_GWha'] <= 154.616190 ) {
        return {
        pane: 'pane_ResiduosurbanosEscenario1_9',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(178,34,34,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,87,51,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosurbanosEscenario1_9');
mapas[0].getPane('pane_ResiduosurbanosEscenario1_9').style.zIndex = 409;
mapas[0].getPane('pane_ResiduosurbanosEscenario1_9').style['mix-blend-mode'] = 'normal';
var layer_ResiduosurbanosEscenario1_9 = new L.geoJson(json_ResiduosurbanosEscenario1_9, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosurbanosEscenario1_9',
    layerName: 'layer_ResiduosurbanosEscenario1_9',
    pane: 'pane_ResiduosurbanosEscenario1_9',
    onEachFeature: pop_ResiduosurbanosEscenario1_9,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_ResiduosurbanosEscenario1_9_0(feature));
    },
});

var residuosUrbanosGroup = L.layerGroup([
    layer_ResiduosurbanosEscenario1_9,
    layer_ResiduosurbanosEscenario2_8,
    layer_ResiduosurbanosEscenario3_7
]);

// Agregar a objeto de capas personalizadas
rasterBaseLayers["🗑️ Residuos Urbanos"] = residuosUrbanosGroup;


//RESIDUOS FORESTALES
function pop_ResiduosforestalesEscenario3_4(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosforestalesEscenario3_4_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.220218 && feature.properties['GENE_GWha'] <= 9.947845 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario3_4',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(247,252,245,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 9.947845 && feature.properties['GENE_GWha'] <= 17.398517 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario3_4',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(201,234,194,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 17.398517 && feature.properties['GENE_GWha'] <= 27.141685 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario3_4',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(123,199,124,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 27.141685 && feature.properties['GENE_GWha'] <= 45.967157 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario3_4',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(42,146,75,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 45.967157 && feature.properties['GENE_GWha'] <= 563.718988 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario3_4',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,68,27,0.65)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosforestalesEscenario3_4');
mapas[0].getPane('pane_ResiduosforestalesEscenario3_4').style.zIndex = 404;
mapas[0].getPane('pane_ResiduosforestalesEscenario3_4').style['mix-blend-mode'] = 'normal';
var layer_ResiduosforestalesEscenario3_4 = new L.geoJson(json_ResiduosforestalesEscenario3_4, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosforestalesEscenario3_4',
    layerName: 'layer_ResiduosforestalesEscenario3_4',
    pane: 'pane_ResiduosforestalesEscenario3_4',
    onEachFeature: pop_ResiduosforestalesEscenario3_4,
    style: style_ResiduosforestalesEscenario3_4_0,
});
function pop_ResiduosforestalesEscenario2_5(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: A</th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosforestalesEscenario2_5_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.220218 && feature.properties['GENE_GWha'] <= 9.947845 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario2_5',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(247,252,245,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 9.947845 && feature.properties['GENE_GWha'] <= 17.398517 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario2_5',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(201,234,194,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 17.398517 && feature.properties['GENE_GWha'] <= 27.141685 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario2_5',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(123,199,124,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 27.141685 && feature.properties['GENE_GWha'] <= 45.967157 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario2_5',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(42,146,75,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 45.967157 && feature.properties['GENE_GWha'] <= 563.718988 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario2_5',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,68,27,0.65)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosforestalesEscenario2_5');
mapas[0].getPane('pane_ResiduosforestalesEscenario2_5').style.zIndex = 405;
mapas[0].getPane('pane_ResiduosforestalesEscenario2_5').style['mix-blend-mode'] = 'normal';
var layer_ResiduosforestalesEscenario2_5 = new L.geoJson(json_ResiduosforestalesEscenario2_5, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosforestalesEscenario2_5',
    layerName: 'layer_ResiduosforestalesEscenario2_5',
    pane: 'pane_ResiduosforestalesEscenario2_5',
    onEachFeature: pop_ResiduosforestalesEscenario2_5,
    style: style_ResiduosforestalesEscenario2_5_0,
});
function pop_ResiduosforestalesEscenario1_6(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Industria: </th>\
                <td>' + (feature.properties['INDUSTRIA'] !== null ? autolinker.link(String(feature.properties['INDUSTRIA']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Tipo: </th>\
                <td>' + (feature.properties['TIPO'] !== null ? autolinker.link(String(feature.properties['TIPO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Proceso: </th>\
                <td>' + (feature.properties['PROCESO'] !== null ? autolinker.link(String(feature.properties['PROCESO']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GENE_GWha'] !== null ? autolinker.link(String(feature.properties['GENE_GWha']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_ResiduosforestalesEscenario1_6_0(feature) {
    if (feature.properties['GENE_GWha'] >= 7.220218 && feature.properties['GENE_GWha'] <= 9.947845 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario1_6',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(247,252,245,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 9.947845 && feature.properties['GENE_GWha'] <= 17.398517 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario1_6',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(201,234,194,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 17.398517 && feature.properties['GENE_GWha'] <= 27.141685 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario1_6',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(123,199,124,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 27.141685 && feature.properties['GENE_GWha'] <= 45.967157 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario1_6',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(42,146,75,0.65)',
        interactive: true,
    }
    }
    if (feature.properties['GENE_GWha'] >= 45.967157 && feature.properties['GENE_GWha'] <= 563.718988 ) {
        return {
        pane: 'pane_ResiduosforestalesEscenario1_6',
        opacity: 1,
        color: 'rgba(35,35,35,0.65)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(0,68,27,0.65)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_ResiduosforestalesEscenario1_6');
mapas[0].getPane('pane_ResiduosforestalesEscenario1_6').style.zIndex = 406;
mapas[0].getPane('pane_ResiduosforestalesEscenario1_6').style['mix-blend-mode'] = 'normal';
var layer_ResiduosforestalesEscenario1_6 = new L.geoJson(json_ResiduosforestalesEscenario1_6, {
    attribution: '',
    interactive: true,
    dataVar: 'json_ResiduosforestalesEscenario1_6',
    layerName: 'layer_ResiduosforestalesEscenario1_6',
    pane: 'pane_ResiduosforestalesEscenario1_6',
    onEachFeature: pop_ResiduosforestalesEscenario1_6,
    style: style_ResiduosforestalesEscenario1_6_0,
});

var residuosForestalesGroup = L.layerGroup([
    layer_ResiduosforestalesEscenario1_6,
    layer_ResiduosforestalesEscenario2_5,
    layer_ResiduosforestalesEscenario3_4
]);

// Agregar a objeto de capas personalizadas
rasterBaseLayers["🌲 Residuos Forestales"] = residuosForestalesGroup;


//GEOTÉRMICA
function pop_GeotermicaEscenario4_0(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Temperatura: </th>\
                <td>' + (feature.properties['TEMP'] !== null ? autolinker.link(String(feature.properties['TEMP']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GEN_MAX'] !== null ? autolinker.link(String(feature.properties['GEN_MAX']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_GeotermicaEscenario4_0_0(feature) {
    if (feature.properties['GEN_MAX'] >= 1.989931 && feature.properties['GEN_MAX'] <= 7.959722 ) {
        return {
        pane: 'pane_GeotermicaEscenario4_0',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 7.959722 && feature.properties['GEN_MAX'] <= 19.899306 ) {
        return {
        pane: 'pane_GeotermicaEscenario4_0',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 19.899306 && feature.properties['GEN_MAX'] <= 27.859028 ) {
        return {
        pane: 'pane_GeotermicaEscenario4_0',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 27.859028 && feature.properties['GEN_MAX'] <= 33.231840 ) {
        return {
        pane: 'pane_GeotermicaEscenario4_0',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 33.231840 && feature.properties['GEN_MAX'] <= 56.984375 ) {
        return {
        pane: 'pane_GeotermicaEscenario4_0',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_GeotermicaEscenario4_0');
mapas[0].getPane('pane_GeotermicaEscenario4_0').style.zIndex = 400;
mapas[0].getPane('pane_GeotermicaEscenario4_0').style['mix-blend-mode'] = 'normal';
var layer_GeotermicaEscenario4_0 = new L.geoJson(json_GeotermicaEscenario4_0, {
    attribution: '',
    interactive: true,
    dataVar: 'json_GeotermicaEscenario4_0',
    layerName: 'layer_GeotermicaEscenario4_0',
    pane: 'pane_GeotermicaEscenario4_0',
    onEachFeature: pop_GeotermicaEscenario4_0,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_GeotermicaEscenario4_0_0(feature));
    },
});
function pop_GeotermicaEscenario3_1(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Temperatura: </th>\
                <td>' + (feature.properties['TEMP'] !== null ? autolinker.link(String(feature.properties['TEMP']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GEN_MAX'] !== null ? autolinker.link(String(feature.properties['GEN_MAX']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_GeotermicaEscenario3_1_0(feature) {
    if (feature.properties['GEN_MAX'] >= 0.994965 && feature.properties['GEN_MAX'] <= 14.924479 ) {
        return {
        pane: 'pane_GeotermicaEscenario3_1',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 14.924479 && feature.properties['GEN_MAX'] <= 26.864062 ) {
        return {
        pane: 'pane_GeotermicaEscenario3_1',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 26.864062 && feature.properties['GEN_MAX'] <= 33.828819 ) {
        return {
        pane: 'pane_GeotermicaEscenario3_1',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 33.828819 && feature.properties['GEN_MAX'] <= 43.778472 ) {
        return {
        pane: 'pane_GeotermicaEscenario3_1',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 43.778472 && feature.properties['GEN_MAX'] <= 263.756250 ) {
        return {
        pane: 'pane_GeotermicaEscenario3_1',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_GeotermicaEscenario3_1');
mapas[0].getPane('pane_GeotermicaEscenario3_1').style.zIndex = 401;
mapas[0].getPane('pane_GeotermicaEscenario3_1').style['mix-blend-mode'] = 'normal';
var layer_GeotermicaEscenario3_1 = new L.geoJson(json_GeotermicaEscenario3_1, {
    attribution: '',
    interactive: true,
    dataVar: 'json_GeotermicaEscenario3_1',
    layerName: 'layer_GeotermicaEscenario3_1',
    pane: 'pane_GeotermicaEscenario3_1',
    onEachFeature: pop_GeotermicaEscenario3_1,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_GeotermicaEscenario3_1_0(feature));
    },
});
function pop_GeotermicaEscenario2_2(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Temperatura: </th>\
                <td>' + (feature.properties['TEMP'] !== null ? autolinker.link(String(feature.properties['TEMP']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GEN_MAX'] !== null ? autolinker.link(String(feature.properties['GEN_MAX']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_GeotermicaEscenario2_2_0(feature) {
    if (feature.properties['GEN_MAX'] >= 24.421875 && feature.properties['GEN_MAX'] <= 39.075000 ) {
        return {
        pane: 'pane_GeotermicaEscenario2_2',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 39.075000 && feature.properties['GEN_MAX'] <= 43.778472 ) {
        return {
        pane: 'pane_GeotermicaEscenario2_2',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 43.778472 && feature.properties['GEN_MAX'] <= 45.768403 ) {
        return {
        pane: 'pane_GeotermicaEscenario2_2',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 45.768403 && feature.properties['GEN_MAX'] <= 53.728125 ) {
        return {
        pane: 'pane_GeotermicaEscenario2_2',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 53.728125 && feature.properties['GEN_MAX'] <= 263.756250 ) {
        return {
        pane: 'pane_GeotermicaEscenario2_2',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_GeotermicaEscenario2_2');
mapas[0].getPane('pane_GeotermicaEscenario2_2').style.zIndex = 402;
mapas[0].getPane('pane_GeotermicaEscenario2_2').style['mix-blend-mode'] = 'normal';
var layer_GeotermicaEscenario2_2 = new L.geoJson(json_GeotermicaEscenario2_2, {
    attribution: '',
    interactive: true,
    dataVar: 'json_GeotermicaEscenario2_2',
    layerName: 'layer_GeotermicaEscenario2_2',
    pane: 'pane_GeotermicaEscenario2_2',
    onEachFeature: pop_GeotermicaEscenario2_2,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_GeotermicaEscenario2_2_0(feature));
    },
});
function pop_GeotermicaEscenario1_3(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Temperatura: </th>\
                <td>' + (feature.properties['TEMP'] !== null ? autolinker.link(String(feature.properties['TEMP']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Generación máxima (GWh/a): </th>\
                <td>' + (feature.properties['GEN_MAX'] !== null ? autolinker.link(String(feature.properties['GEN_MAX']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_GeotermicaEscenario1_3_0(feature) {
    if (feature.properties['GEN_MAX'] >= 24.421875 && feature.properties['GEN_MAX'] <= 30.934375 ) {
        return {
        pane: 'pane_GeotermicaEscenario1_3',
        radius: 2.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 30.934375 && feature.properties['GEN_MAX'] <= 36.144375 ) {
        return {
        pane: 'pane_GeotermicaEscenario1_3',
        radius: 5.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 36.144375 && feature.properties['GEN_MAX'] <= 43.959375 ) {
        return {
        pane: 'pane_GeotermicaEscenario1_3',
        radius: 9.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 43.959375 && feature.properties['GEN_MAX'] <= 56.007500 ) {
        return {
        pane: 'pane_GeotermicaEscenario1_3',
        radius: 12.5,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
    if (feature.properties['GEN_MAX'] >= 56.007500 && feature.properties['GEN_MAX'] <= 263.756250 ) {
        return {
        pane: 'pane_GeotermicaEscenario1_3',
        radius: 16.0,
        opacity: 1,
        color: 'rgba(128,17,25,0.43200000000000005)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0,
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(219,30,42,0.43200000000000005)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_GeotermicaEscenario1_3');
mapas[0].getPane('pane_GeotermicaEscenario1_3').style.zIndex = 403;
mapas[0].getPane('pane_GeotermicaEscenario1_3').style['mix-blend-mode'] = 'normal';
var layer_GeotermicaEscenario1_3 = new L.geoJson(json_GeotermicaEscenario1_3, {
    attribution: '',
    interactive: true,
    dataVar: 'json_GeotermicaEscenario1_3',
    layerName: 'layer_GeotermicaEscenario1_3',
    pane: 'pane_GeotermicaEscenario1_3',
    onEachFeature: pop_GeotermicaEscenario1_3,
    pointToLayer: function (feature, latlng) {
        var context = {
            feature: feature,
            variables: {}
        };
        return L.circleMarker(latlng, style_GeotermicaEscenario1_3_0(feature));
    },
});

var geotermicaGroup = L.layerGroup([
    layer_GeotermicaEscenario1_3,
    layer_GeotermicaEscenario2_2,
    layer_GeotermicaEscenario3_1,
    layer_GeotermicaEscenario4_0
]);

// Agregar a objeto de capas personalizadas
rasterBaseLayers["🌋 Geotérmica"] = geotermicaGroup;

function aplicarProxyCDNEnLabels(tree) {
    tree.forEach(nodo => {
        if (nodo.label && typeof nodo.label === 'string') {
            nodo.label = nodo.label.replace(
                /https:\/\/cdn\.sassoapps\.com\/([^"]+)/g,
                (match) => '/Atlas/ProxyImagen?url=' + encodeURIComponent(match)
            );
        }

        if (nodo.children && Array.isArray(nodo.children)) {
            aplicarProxyCDNEnLabels(nodo.children);
        }
    });
}

var overlaysTree = [
        {label: '<b>INDUSTRIALES</b>', collapsed: true, selectAllCheckbox: true, children: [
            {label: 'Residuos industriales (Escenario 1)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_780.png" /></td><td>7 - 8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_8111.png" /></td><td>8 - 11</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_11162.png" /></td><td>11 - 16</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_16313.png" /></td><td>16 - 31</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_311834.png" /></td><td>7 - 8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_8111.png" /></td><td>8 - 11</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_11162.png" /></td><td>11 - 16</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_16313.png" /></td><td>16 - 31</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario1_15_311834.png" /></td><td>31 - 183</td></tr></table>', layer: layer_ResiduosindustrialesEscenario1_15},
            {label: 'Residuos industriales (Escenario 2) <br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario2_14_36420.png" /></td><td>3.6 - 4.2</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario2_14_42721.png" /></td><td>4.2 - 7.2</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario2_14_721042.png" /></td><td>7.2 - 10.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario2_14_1042183.png" /></td><td>10.4 - 21.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario2_14_21818344.png" /></td><td>21.8 - 183.4</td></tr></table>', layer: layer_ResiduosindustrialesEscenario2_14},
            {label: 'Residuos industriales (Escenario 3)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario3_13_04050.png" /></td><td>0.4 - 0.5</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario3_13_05081.png" /></td><td>0.5 - 0.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario3_13_08142.png" /></td><td>0.8 - 1.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario3_13_14343.png" /></td><td>1.4 - 3.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosindustrialesEscenario3_13_3418344.png" /></td><td>3.4 - 183.4</td></tr></table>', layer: layer_ResiduosindustrialesEscenario3_13},]},
        {label: '<b>PECUARIOS</b>', collapsed: true, selectAllCheckbox: true, children: [
            {label: 'Residuos pecuarios (Escenario 1)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario1_12_77820.png" /></td><td>7.7 - 8.2</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario1_12_82891.png" /></td><td>8.2 - 8.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario1_12_89102.png" /></td><td>8.9 - 10</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario1_12_10123.png" /></td><td>10 - 12</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario1_12_12434.png" /></td><td>12 - 43</td></tr></table>', layer: layer_ResiduospecuariosEscenario1_12},
            {label: 'Residuos pecuarios (Escenario 2)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario2_11_3563810.png" /></td><td>3.56 - 3.81</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario2_11_3814571.png" /></td><td>3.81 - 4.57</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario2_11_4575412.png" /></td><td>4.57 - 5.41</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario2_11_5418253.png" /></td><td>5.41 - 8.25</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario2_11_82542984.png" /></td><td>8.25 - 42.98</td></tr></table>', layer: layer_ResiduospecuariosEscenario2_11},
            {label: 'Residuos pecuarios (Escenario 3)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario3_10_0420790.png" /></td><td>0.42 - 0.79</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario3_10_0791131.png" /></td><td>0.79 - 1.13</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario3_10_1131632.png" /></td><td>1.13 - 1.63</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario3_10_1632873.png" /></td><td>1.63 - 2.87</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduospecuariosEscenario3_10_28742984.png" /></td><td>2.87 - 42.98</td></tr></table>', layer: layer_ResiduospecuariosEscenario3_10},]},
        {label: '<b>URBANOS</b>', collapsed: true, selectAllCheckbox: true, children: [
            {label: 'Residuos urbanos (Escenario 1)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario1_9_7970.png" /></td><td>7 - 9.7</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario1_9_971271.png" /></td><td>9.7 - 12.7</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario1_9_1272212.png" /></td><td>12.7 - 22.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario1_9_2213363.png" /></td><td>22.1 - 33.6</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario1_9_33615464.png" /></td><td>33.6 - 154.6</td></tr></table>', layer: layer_ResiduosurbanosEscenario1_9},
            {label: 'Residuos urbanos (Escenario 2)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario2_8_36450.png" /></td><td>3.6 - 4.5</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario2_8_45681.png" /></td><td>4.5 - 6.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario2_8_681092.png" /></td><td>6.8 - 10.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario2_8_1092473.png" /></td><td>10.9 - 24.7</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario2_8_24715464.png" /></td><td>24.7 - 154.6</td></tr></table>', layer: layer_ResiduosurbanosEscenario2_8},
            {label: 'Residuos urbanos (Escenario 3)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario3_7_04070.png" /></td><td>0.4 - 0.7</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario3_7_07141.png" /></td><td>0.7 - 1.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario3_7_14362.png" /></td><td>1.4 - 3.6</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario3_7_36863.png" /></td><td>3.6 - 8.6</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosurbanosEscenario3_7_8615464.png" /></td><td>8.6 - 154.6</td></tr></table>', layer: layer_ResiduosurbanosEscenario3_7},]},
        {label: '<b>FORESTALES</b>', collapsed: true, selectAllCheckbox: true, children: [
            {label: 'Residuos forestales (Escenario 1)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario1_6_72990.png" /></td><td>7.2 - 9.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario1_6_991741.png" /></td><td>9.9 - 17.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario1_6_1742712.png" /></td><td>17.4 - 27.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario1_6_271463.png" /></td><td>27.1 - 46</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario1_6_4656374.png" /></td><td>46 - 563.7</td></tr></table>', layer: layer_ResiduosforestalesEscenario1_6},
            {label: 'Residuos forestales (Escenario 2)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario2_5_72990.png" /></td><td>7.2 - 9.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario2_5_991741.png" /></td><td>9.9 - 17.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario2_5_1742712.png" /></td><td>17.4 - 27.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario2_5_271463.png" /></td><td>27.1 - 46</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario2_5_4656374.png" /></td><td>46 - 563.7</td></tr></table>', layer: layer_ResiduosforestalesEscenario2_5},
            {label: 'Residuos forestales (Escenario 3)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario3_4_72990.png" /></td><td>7.2 - 9.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario3_4_991741.png" /></td><td>9.9 - 17.4</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario3_4_1742712.png" /></td><td>17.4 - 27.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario3_4_271463.png" /></td><td>27.1 - 46</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/ResiduosforestalesEscenario3_4_4656374.png" /></td><td>46 - 563.7</td></tr></table>', layer: layer_ResiduosforestalesEscenario3_4},]},
        {label: '<b>GEOTERMICA</b>', collapsed: true, selectAllCheckbox: true, children: [
            {label: 'Geotermica (Escenario 1)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario1_3_2443090.png" /></td><td>24.4 - 30.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario1_3_3093611.png" /></td><td>30.9 - 36.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario1_3_361442.png" /></td><td>36.1 - 44</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario1_3_44563.png" /></td><td>44 - 56</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario1_3_5626384.png" /></td><td>56 - 263.8</td></tr></table>', layer: layer_GeotermicaEscenario1_3},
            {label: 'Geotermica (Escenario 2)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario2_2_2443910.png" /></td><td>24.4 - 39.1</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario2_2_3914381.png" /></td><td>39.1 - 43.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario2_2_4384582.png" /></td><td>43.8 - 45.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario2_2_4585373.png" /></td><td>45.8 - 53.7</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario2_2_53726384.png" /></td><td>53.7 - 263.8</td></tr></table>', layer: layer_GeotermicaEscenario2_2},
            {label: 'Geotermica (Escenario 3)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario3_1_11490.png" /></td><td>1 - 14.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario3_1_1492691.png" /></td><td>14.9 - 26.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario3_1_2693382.png" /></td><td>26.9 - 33.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario3_1_3384383.png" /></td><td>33.8 - 43.8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario3_1_43826384.png" /></td><td>43.8 - 263.8</td></tr></table>', layer: layer_GeotermicaEscenario3_1},
            {label: 'Geotermica (Escenario 4)<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario4_0_280.png" /></td><td>2 - 8</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario4_0_81991.png" /></td><td>8 - 19.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario4_0_1992792.png" /></td><td>19.9 - 27.9</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario4_0_2793323.png" /></td><td>27.9 - 33.2</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/GeotermicaEscenario4_0_332574.png" /></td><td>33.2 - 57</td></tr></table>', layer: layer_GeotermicaEscenario4_0},]},]
        
            aplicarProxyCDNEnLabels(overlaysTree);
            console.log("Overlays Tree: ", overlaysTree);

            var lay = L.control.layers.tree(null, overlaysTree,{
            //namedToggle: true,
            //selectorBack: false,
            //closedSymbol: '&#8862; &#x1f5c0;',
            //openedSymbol: '&#8863; &#x1f5c1;',
            //collapseAll: 'Collapse all',
            //expandAll: 'Expand all',
            collapsed: true,
        });
        lay.addTo(mapas[0]);

function pop_Cuencas_Disponibilidad_2023_0(feature, layer) {
    var popupContent = '<table>\
            <tr>\
                <th scope="row">Clave de la cuenca: </th>\
                <td>' + (feature.properties['clvcuenca'] !== null ? autolinker.link(String(feature.properties['clvcuenca']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Cuenca: </th>\
                <td>' + (feature.properties['cuenca'] !== null ? autolinker.link(String(feature.properties['cuenca']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
            <tr>\
                <th scope="row">Disponibilidad: </th>\
                <td>' + (feature.properties['d'] !== null ? autolinker.link(String(feature.properties['d']).replace(/'/g, '\'').toLocaleString()) : '') + '</td>\
            </tr>\
        </table>';
    var content = removeEmptyRowsFromPopupContent(popupContent, feature);
    layer.on('popupopen', function(e) {
        addClassToPopupIfMedia(content, e.popup);
    });
    layer.bindPopup(content, { maxHeight: 400 });
}

function style_Cuencas_Disponibilidad_2023_0_0(feature) {
    if (feature.properties['d'] >= -1635.387000 && feature.properties['d'] <= -100.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(127,26,28,1.0)',
        interactive: true,
    }
    }
    if (feature.properties['d'] >= -100.000000 && feature.properties['d'] <= -10.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
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
    if (feature.properties['d'] >= -10.000000 && feature.properties['d'] <= -1.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(255,154,0,1.0)',
        interactive: true,
    }
    }
    if (feature.properties['d'] >= -1.000000 && feature.properties['d'] <= 1.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(247,222,109,1.0)',
        interactive: true,
    }
    }
    if (feature.properties['d'] >= 1.000000 && feature.properties['d'] <= 10.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(166,206,227,1.0)',
        interactive: true,
    }
    }
    if (feature.properties['d'] >= 10.000000 && feature.properties['d'] <= 100.000000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(64,133,180,1.0)',
        interactive: true,
    }
    }
    if (feature.properties['d'] >= 100.000000 && feature.properties['d'] <= 12199.913000 ) {
        return {
        pane: 'pane_Cuencas_Disponibilidad_2023_0',
        opacity: 1,
        color: 'rgba(35,35,35,0.0)',
        dashArray: '',
        lineCap: 'butt',
        lineJoin: 'miter',
        weight: 1.0, 
        fill: true,
        fillOpacity: 1,
        fillColor: 'rgba(13,74,115,1.0)',
        interactive: true,
    }
    }
}
mapas[0].createPane('pane_Cuencas_Disponibilidad_2023_0');
mapas[0].getPane('pane_Cuencas_Disponibilidad_2023_0').style.zIndex = 400;
mapas[0].getPane('pane_Cuencas_Disponibilidad_2023_0').style['mix-blend-mode'] = 'normal';
var layer_Cuencas_Disponibilidad_2023_0 = new L.geoJson(json_Cuencas_Disponibilidad_2023_0, {
    attribution: '',
    interactive: true,
    dataVar: 'json_Cuencas_Disponibilidad_2023_0',
    layerName: 'layer_Cuencas_Disponibilidad_2023_0',
    pane: 'pane_Cuencas_Disponibilidad_2023_0',
    onEachFeature: pop_Cuencas_Disponibilidad_2023_0,
    style: style_Cuencas_Disponibilidad_2023_0_0,
});
rasterBaseLayers["💧 Disponibilidad Hídrica"] = layer_Cuencas_Disponibilidad_2023_0;
var overlaysTreeDC = [
    {label: 'Cuencas_Disponibilidad_2023<br /><table><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Déficitmásde100hm³año0.png" /></td><td>Déficit (más de -100hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Déficitentre10y100hm³año1.png" /></td><td>Déficit (entre -10 y -100hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Déficitentre1y10hm³año2.png" /></td><td>Déficit (entre -1 y -10 hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Neutro1y1hm³año3.png" /></td><td>Neutro (-1 y 1 hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Disponibilidadentre1y10hm³año4.png" /></td><td>Disponibilidad (entre 1 y 10 hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Disponibilidadentre10y100hm³año5.png" /></td><td>Disponibilidad (entre 10 y 100hm³/año)</td></tr><tr><td style="text-align: center;"><img src="https://cdn.sassoapps.com/Geovisualizador/legend/Cuencas_Disponibilidad_2023_0_Disponibilidadmásde100hm³año6.png" /></td><td>Disponibilidad (más de 100hm³/año)</td></tr></table>', layer: layer_Cuencas_Disponibilidad_2023_0},]

        aplicarProxyCDNEnLabels(overlaysTreeDC);

    var layDC = L.control.layers.tree(null, overlaysTreeDC,{
    //namedToggle: true,
    //selectorBack: false,
    //closedSymbol: '&#8862; &#x1f5c0;',
    //openedSymbol: '&#8863; &#x1f5c1;',
    //collapseAll: 'Collapse all',
    //expandAll: 'Expand all',
    collapsed: true,
});
layDC.addTo(mapas[0]);

// --- Control de capas ---

var control = L.control.layers(rasterBaseLayers, null, { collapsed: false, position: 'topright' }).addTo(mapas[0]);



// --- Funciones para cargar y limpiar marcadores ---
// --- Validación segura para carga de KML ---
function validarNombreKML(input) {
    // No permitir vacíos, espacios solo, ni caracteres peligrosos
    var value = input.value.trim();
    // Expresión regular: solo letras, números, guiones, guion bajo, punto y espacios
    var regex = /^[\w\d\-_. ]+$/;
    if (!value) {
        input.setCustomValidity('El nombre no puede estar vacío.');
        return false;
    }
    if (!regex.test(value)) {
        input.setCustomValidity('Nombre inválido: solo letras, números, guiones, guion bajo, punto y espacios.');
        return false;
    }
    // Validación básica para evitar inyección de dependencias (no <, >, ;, |, &, etc)
    if (/[<>;|&$]/.test(value)) {
        input.setCustomValidity('Nombre inválido: no se permiten caracteres especiales.');
        return false;
    }
    input.setCustomValidity('');
    return true;
}

// Si tienes un input para el nombre del KML, agrega el listener:
setTimeout(function () {
    var kmlInput = document.getElementById('kmlFileNameInput');
    if (kmlInput) {
        kmlInput.addEventListener('input', function () {
            validarNombreKML(this);
        });
        kmlInput.addEventListener('blur', function () {
            validarNombreKML(this);
        });
    }
}, 1000);


// --- Links de descarga de capas ---
// Coloca los links directamente en el div desde el JS al cargar la página
function ponerLinksDescargaDirectos() {
    var contenedor = document.getElementById('descarga-capas');
    if (!contenedor) return;
    contenedor.innerHTML = `
        <ul class="list-group">
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🌞 Potencial Fotovoltaico</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/potencialfotovoltaico_4326_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>☀️ Radiación Horizontal</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/radiacionhorizontal_4326_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 10m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa10mdealtura_4.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 50m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa50mdealtura_3.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 100m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa100mdealtura_2.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 150m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa150mdealtura_1.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 200m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa200mdealtura_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🛢️ Residuos Industriales</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/ResiduosindustrialesEscenario3_13.js" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🐄 Residuos Pecuarios</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/ResiduospecuariosEscenario3_10.js" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🗑️ Residuos Urbanos</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/ResiduosurbanosEscenario3_7.js" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🌲 Residuos Forestales</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/ResiduosforestalesEscenario3_4.js" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🌋 Geotérmica</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/GeotermicaEscenario4_0.js" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💧 Disponibilidad Hídrica</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/data/Cuencas_Disponibilidad_2023_0" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
        </ul>
    `;
}

// Al cargar la página, asegurar capa por defecto y links reales
document.addEventListener('DOMContentLoaded', function () {
    // Mostrar panel de descarga de capas si está oculto
    var descargaPanel = document.getElementById('descarga-capas');
    if (descargaPanel) {
        descargaPanel.style.display = '';
    }
    // Coloca los links de descarga directamente
    ponerLinksDescargaDirectos();
    // Forzar que solo el raster de Potencial Fotovoltaico esté activo al cargar la página
    // Elimina todos los overlays raster (imageOverlay) antes de agregar el de fotovoltaico
    // Eliminar todas las capas base raster que puedan estar activas
    for (const nombre in rasterBaseLayers) {
        const capa = rasterBaseLayers[nombre];
        if (mapas[0].hasLayer(capa)) {
            mapas[0].removeLayer(capa);
        }
    }
    if (rasterBaseLayers["🌞 Potencial Fotovoltaico"]) {
        rasterBaseLayers["🌞 Potencial Fotovoltaico"].addTo(mapas[0]);
        rasterActivo = rasterBaseLayers["🌞 Potencial Fotovoltaico"];
    }
    // Mostrar leyenda de Potencial Fotovoltaico
    hideAllColorRamps();
    var defaultRamp = document.getElementById('simbol-fotovoltaico');
    if (defaultRamp) {
        defaultRamp.style.display = '';
    }
    // setTimeout(agregarBotonCentrarMapa, 200);
    // Cargar marcadores fotovoltaico solo si no se ha llamado ya
    if (typeof cargarMarcadoresFotovoltaico === 'function') {
        cargarMarcadoresFotovoltaico();
    }
});


function limpiarMarcadores() {
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            mapas[0].removeLayer(layer);
        }
    });
}

function cargarMarcadoresFotovoltaico() {
    // Limpiar todos los marcadores y clusters antes de cargar los nuevos
    limpiarMarcadores();
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
    // Limpiar todos los marcadores y clusters antes de cargar los nuevos
    limpiarMarcadores();
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
                console.log("Response for Viento:", response); // Log the raw response
                // Solo Eólica
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "Eólica";
                });
                console.log("Filtered datosExpendios for Viento:", datosExpendios); // Log the filtered data
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
                var iconoEolica = new iconoBase({ iconUrl: proxyVientoImg });
                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>"
                        + ".popup-content { width: 280px; max-height: 180px; overflow-y: auto; padding: 10px; }"
                        + "h2, h3, h4, p, li { margin: 0 0 10px 0; }"
                        + "ul { padding-left: 20px; }"
                        + "img { vertical-align: middle; margin-right: 10px; }"
                        + "</style>";
                    contenido += "<div class='popup-content'>";
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<h2 class='subtitulo'><img src='" + proxyVientoImg + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }
                    contenido += "<ul>";
                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>Número de Permiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Número de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>Razón Social:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>Latitud:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>Longitud:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus del Permiso:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RFC")) {
                        contenido += "<li><strong>RFC:</strong> " + handleNull(coordenada.rfc) + "</li>";
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
                        contenido += "<li><strong>Inicio de Operaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
                        contenido += "<li><strong>Capacidad Autorizada (MW):</strong> " + handleNull(coordenada.capacidadAutorizadaMW) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
                        contenido += "<li><strong>Generación Estimada Anual:</strong> " + handleNull(coordenada.generacion_estimada_anual) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
                        contenido += "<li><strong>Inversión Estimada (mdls):</strong> " + handleNull(coordenada.inversion_estimada_mdls) + "</li>";
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

                //******Tarjetas Totales******///
                // Contar el número total de elementos en el array 'datosExpendios'
                var totalElementos = datosExpendios.length;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de permisos (Eólica) formateado:", totalFormateado);

                // Actualizar la tarjeta de permisos
                const tarjetaTitulo = document.getElementById('titulo-permiso');
                const tarjetaTotal = document.getElementById('total-permisos');
                const tarjetaIcono = document.getElementById('icono-permiso');

                if (tarjetaTitulo) tarjetaTitulo.textContent = 'Eólica';
                if (tarjetaIcono) tarjetaIcono.src = proxyVientoImg; // Asegúrate que el ícono exista
                if (tarjetaTotal) tarjetaTotal.textContent = "Proyectos Autorizados: " + totalFormateado;
                //******Fin Tarjetas Totales******///

                // --- INICIO GRÁFICO VIENTO ---
                var counts = {};
                datosExpendios.forEach(function (coordenada) {
                    if (!counts[coordenada.efId]) {
                        counts[coordenada.efId] = 0;
                    }
                    counts[coordenada.efId]++;
                });

                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: 'transparent',
                        style: {
                            fontFamily: 'inherit'
                        },
                        borderRadius: 10
                    },
                    title: {
                        text: 'Total de Proyectos Autorizados por Entidad Federativa',
                        style: {
                            color: '#333',
                            fontSize: '18px',
                            fontWeight: 'bold'
                        }
                    },
                    subtitle: {
                        text: 'Fuente: Eólica',
                        style: {
                            color: '#666'
                        }
                    },
                    xAxis: {
                        categories: categories,
                        labels: {
                            style: {
                                color: '#666'
                            }
                        }
                    },
                    yAxis: {
                        title: {
                            text: 'Número de Proyectos',
                            style: {
                                color: '#666'
                            }
                        },
                        gridLineColor: '#e6e6e6'
                    },
                    series: [{
                        name: 'Proyectos Autorizados',
                        data: dataPermisos,
                        color: {
                            linearGradient: { x1: 0, x2: 0, y1: 0, y2: 1 },
                            stops: [
                                [0, '#2af598'],
                                [1, '#009efd']
                            ]
                        },
                        dataLabels: {
                            enabled: true,
                            rotation: 0,
                            color: '#333',
                            align: 'center',
                            format: '{point.y:,.0f}',
                            y: -15,
                            style: {
                                fontSize: '11px',
                                fontWeight: 'bold'
                            },
                            backgroundColor: 'rgba(255, 255, 255, 0.7)',
                            padding: 3,
                            borderRadius: 3
                        }
                    }],
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        style: {
                            color: '#f0f0f0'
                        },
                        headerFormat: '<span style="font-size: 12px">{point.key}</span><br/>',
                        pointFormat: '<span style="color:{series.color}">●</span> {series.name}: <b>{point.y:,.0f}</b>'
                    },
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 1000
                            },
                            states: {
                                hover: {
                                    brightness: -0.1
                                }
                            },
                            borderRadius: 5
                        }
                    },
                    credits: {
                        enabled: false
                    }
                };
                Highcharts.chart('grafico', options);
                // --- FIN GRÁFICO VIENTO ---


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
var opacityControl = L.control({ position: 'topleft' });
opacityControl.onAdd = function (map) {
    var div = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
    div.style.background = '#fff';
    div.style.padding = '6px 10px 2px 10px';
    div.style.width = '160px';
    div.innerHTML = '<label style="font-size:12px;">Opacidad raster</label><br><input id="raster-opacity-slider" type="range" min="0" max="1" step="0.01" value="1" style="width:130px;">';

    // Evitar que el mapa se mueva al interactuar con el control
    L.DomEvent.disableClickPropagation(div);
    L.DomEvent.disableScrollPropagation(div);

    return div;
};
opacityControl.addTo(mapas[0]);

// Evento para cambiar opacidad
setTimeout(function () {
    var slider = document.getElementById('raster-opacity-slider');
    if (slider) {
        slider.addEventListener('input', function () {
            var valor = parseFloat(this.value);

            if (!rasterActivo) return;

            if (typeof rasterActivo.setOpacity === 'function') {
                rasterActivo.setOpacity(valor);
            } 
            else if (rasterActivo.eachLayer) {
                rasterActivo.eachLayer(function (layer) {
                    if (typeof layer.setOpacity === 'function') {
                        layer.setOpacity(valor);
                    }
                });
            }
        });
    }
}, 500);

function hideAllColorRamps() {
    var rampIds = ['simbol-fotovoltaico', 'simbol-radiacion', 'simbol-viento'];
    rampIds.forEach(function (id) {
        var el = document.getElementById(id);
        if (el) {
            el.style.display = 'none';
        }
    });
}

// Cargar capa y marcadores por defecto
rasterActivo.addTo(mapas[0]);
cargarMarcadoresFotovoltaico();

// Ocultar todas las rampas al inicio y luego mostrar la por defecto
hideAllColorRamps();
var defaultRamp = document.getElementById('simbol-fotovoltaico');
if (defaultRamp) {
    defaultRamp.style.display = '';
}

// --- Evento para cambio de capa raster ---
mapas[0].on('baselayerchange', function (e) {
    // Si es un estilo de mapa y no un raster temático, no limpiar marcadores
    const capasTematicas = [
        "🌞 Potencial Fotovoltaico",
        "☀️ Radiación Horizontal",
        "💨 Viento", // puedes usar startsWith más abajo
        "🛢️ Residuos Industriales",
        "🐄 Residuos Pecuarios",
        "🗑️ Residuos Urbanos",
        "🌲 Residuos Forestales",
        "🌋 Geotérmica",
        "💧 Disponibilidad Hídrica"
    ];

    if (!capasTematicas.some(nombre => e.name.startsWith(nombre.replace(/.$/, "")))) {
        return; // Es un cambio de estilo base → no tocar marcadores
    }

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
        'simbol-fotovoltaico',
        'simbol-radiacion',
        'simbol-viento'
    ];
    tablas.forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.style.display = 'none';
    });

    // Determina qué leyenda/rampa mostrar (solo si existen)
    if (e.name === '🌞 Potencial Fotovoltaico') {
        var t = document.getElementById('simbol-fotovoltaico');
        if (t) t.style.display = '';
        mapas[0].removeControl(lay);
        mapas[0].removeControl(layDC);
        actualizarPlaceholderBusqueda('PERMISO');
    } else if (e.name === '☀️ Radiación Horizontal') {
        var t = document.getElementById('simbol-radiacion');
        if (t) t.style.display = '';
        mapas[0].removeControl(lay);
        mapas[0].removeControl(layDC);
        actualizarPlaceholderBusqueda('PERMISO');
    } else if (e.name && e.name.startsWith('💨 Viento')) {
        var t = document.getElementById('simbol-viento');
        if (t) t.style.display = '';
        mapas[0].removeControl(lay);
        mapas[0].removeControl(layDC);
        actualizarPlaceholderBusqueda('PERMISO');
    } else if (e.name === '🛢️ Residuos Industriales') {
        var t = document.getElementById('simbol-biomasa');
        if (t) t.style.display = '';
        mapas[0].removeControl(layDC);
        lay.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
    } else if (e.name === '🐄 Residuos Pecuarios') {
        var t = document.getElementById('simbol-biomasa');
        if (t) t.style.display = '';
        mapas[0].removeControl(layDC);
        lay.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
    } else if (e.name === '🗑️ Residuos Urbanos') {
        var t = document.getElementById('simbol-biomasa');
        if (t) t.style.display = '';
        mapas[0].removeControl(layDC);
        lay.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
    } else if (e.name === '🌲 Residuos Forestales') {
        var t = document.getElementById('simbol-biomasa');
        if (t) t.style.display = '';
        mapas[0].removeControl(layDC);
        lay.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
    } else if (e.name === '🌋 Geotérmica') {
        var t = document.getElementById('simbol-geotermica');
        if (t) t.style.display = '';
        mapas[0].removeControl(layDC);
        lay.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
    } 
    else if (e.name === '💧 Disponibilidad Hídrica'){
        var t = document.getElementById('simbol-hidrica');
        if (t) t.style.display = '';
        mapas[0].removeControl(lay);
        layDC.addTo(mapas[0]);
        actualizarPlaceholderBusqueda('SINPERMISO');
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

// --- Inicialización del plugin de impresión ---
// Crear preloader
const preloader = document.createElement('div');
preloader.id = 'preloader';
preloader.innerHTML = '<div class="spinner"></div>';
preloader.style.cssText = `
    position: fixed;
    top: 0; left: 0;
    width: 100%; height: 100%;
    background-color: rgba(255,255,255,0.7);
    display: none;
    z-index: 9999;
    justify-content: center;
    align-items: center;
`;
document.body.appendChild(preloader);

// Estilos del spinner
const style = document.createElement('style');
style.innerHTML = `
  .spinner {
    border: 8px solid #f3f3f3;
    border-top: 8px solid #333;
    border-radius: 50%;
    width: 60px;
    height: 60px;
    animation: spin 1s linear infinite;
  }
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
`;
document.head.appendChild(style);

// Inicializar el printer
const printer = L.easyPrint({
    tileLayer: null,
    sizeModes: ['Current', 'A4Portrait', 'A4Landscape'],
    filename: 'captura_mapa',
    exportOnly: true,
    hideControlContainer: true
}).addTo(mapas[0]);

const esperarOpcionesImpresion = setInterval(() => {
    const currentSizeBtn = document.querySelector('.CurrentSize');
    const a4PortraitBtn = document.querySelector('.A4Portrait');
    const a4LandscapeBtn = document.querySelector('.A4Landscape');

    if (currentSizeBtn && a4PortraitBtn && a4LandscapeBtn) {
        console.log('🎯 Botones de opciones de impresión detectados.');

        const mostrarPreloader = () => {
            console.log('⏳ Mostrando preloader...');
            document.getElementById('ajax-preloader').style.display = 'flex';

            // Esperamos unos segundos para ocultarlo (ej. 3 segundos)
            setTimeout(() => {
                document.getElementById('ajax-preloader').style.display = 'none';
            }, 3500);
        };

        currentSizeBtn.addEventListener('click', mostrarPreloader);
        a4PortraitBtn.addEventListener('click', mostrarPreloader);
        a4LandscapeBtn.addEventListener('click', mostrarPreloader);

        clearInterval(esperarOpcionesImpresion); // Ya no es necesario seguir buscando
    }
}, 500);
