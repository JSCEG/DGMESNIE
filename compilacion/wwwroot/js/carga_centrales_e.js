//Para los Marcadores
//Campos Visbles de los popup*@
var capaActiva = 'estados';  // Valor por defecto cuando carga el mapa

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

// Crea los Iconos y Define su tamaño
var iconoBase = L.Icon.extend({
  options: {
    iconSize: [36, 36],
    iconAnchor: [18, 16],
    popupAnchor: [0, -26],
  },
});

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

  if (!encontrado) {
    alert("Término no encontrado.");
  }
}

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
  // 1. Inicialización de variables
  availableTerms = [];
  datosExpendios = [];
  camposVisiblesGlobal = [];
  totalpermisos = 0;

  // 2. Definición de iconos y configuraciones
  const iconoBase = L.Icon.extend({
    options: {
      iconSize: [36, 36],
      iconAnchor: [12, 18],
      popupAnchor: [0, -26]
    }
  });

  const iconos = {
    convencional: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png" }),
    hidro: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/hidro.png" }),
    bio: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/biomasai.png" }),
    eolica: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/eolica.png" }),
    vapor: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/vapor.png" }),
    combustion: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/combustion.png" }),
    cogeneracion: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/cogeneracion.png" }),
    cogeneracionEficiente: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/cogeneracion_eficiente.png" }),
    cicloCombinado: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/ciclo_combinado.png" }),
    carboelectrica: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/carboeléctrica.png" }),
    geotermica: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/geotermica.png" }),
    solar: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png" }),
    cinetica: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/cinetica.png" }),
    nuclear: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/nuclear.png" }),
    lecho: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/lecho.png" }),
    importacion: new iconoBase({ iconUrl: "https://cdn.sassoapps.com/img_snier/vistas/importacion_e.png" })
  };

  // 3. Carga de campos visibles mediante promesa
  function cargarCamposVisibles() {
    return new Promise((resolve, reject) => {
      $.ajax({
        url: "/Indicadores/GetCamposVisiblesElectricidad_Infra",
        type: "GET",
        contentType: "application/json",
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

  // 4. Función para generar contenido del popup
  function generarContenidoPopup(coordenada) {
    let contenido = `
      <style>
        .popup-content { width: 280px; max-height: 180px; overflow-y: auto; padding: 10px; }
        h2, h3, h4, p, li { margin: 0 0 10px 0; }
        ul { padding-left: 20px; }
        img { vertical-align: middle; margin-right: 10px; }
      </style>
      <div class='popup-content'>
    `;

    if (camposVisiblesGlobal.includes("RazonSocial")) {
      let imgSrc;
      switch (coordenada.clasificacion) {
        case "Hidroeléctrica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/hidro.png"; break;
        case "Bioenergía": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/biomasai.png"; break;
        case "Eólica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/eolica.png"; break;
        case "Turbo Gas y Vapor": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/vapor.png"; break;
        case "Combustión Interna": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/combustion.png"; break;
        case "Cogeneración": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/cogeneracion.png"; break;
        case "Cogeneración Eficiente": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/cogeneracion_eficiente.png"; break;
        case "Ciclo combinado": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/ciclo_combinado.png"; break;
        case "Carboeléctrica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/carboeléctrica.png"; break;
        case "Solar Fotovoltaica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png"; break;
        case "Energía Cinética": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/cinetica.png"; break;
        case "Nucleoeléctrica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/nuclear.png"; break;
        case "Lecho Fluidizado": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/lecho.png"; break;
        case "Importación": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/importacion_e.png"; break;
        case "Geotérmica": imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/geotermica.png"; break;
        default: imgSrc = "https://cdn.sassoapps.com/img_snier/vistas/central_electrica.png"; break;
      }
      contenido += `<h2 class='razon-social-popup'><img src='${imgSrc}' style='height: 24px; width: 24px;'/><strong>${handleNull(coordenada.razonSocial)}</strong></h2><br>`;
    }

    contenido += "<ul>";

    if (camposVisiblesGlobal.includes("EfId")) {
      contenido += `<li><strong>Clave y Entidad Federativa:</strong> ${handleNull(coordenada.efId)}</li>`;
    }
    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
      contenido += `<li><strong>NumeroPermiso:</strong> ${handleNull(coordenada.numeroPermiso)}</li>`;
    }
    if (camposVisiblesGlobal.includes("MpoId")) {
      contenido += `<li><strong>Clave y Municipio:</strong> ${handleNull(coordenada.mpoId)}</li>`;
    }
    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
      contenido += `<li><strong>Numero de Expediente:</strong> ${handleNull(coordenada.numeroDeExpediente)}</li>`;
    }
    if (camposVisiblesGlobal.includes("RazonSocial")) {
      contenido += `<li><strong>RazonSocial:</strong> ${handleNull(coordenada.razonSocial)}</li>`;
    }
    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
      contenido += `<li><strong>Fecha de Otorgamiento:</strong> ${handleNull(coordenada.fechaOtorgamiento)}</li>`;
    }
    if (camposVisiblesGlobal.includes("LatitudGeo")) {
      contenido += `<li><strong>LatitudGeo:</strong> ${handleNull(coordenada.latitudGeo)}</li>`;
    }
    if (camposVisiblesGlobal.includes("LongitudGeo")) {
      contenido += `<li><strong>LongitudGeo:</strong> ${handleNull(coordenada.longitudGeo)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Dirección")) {
      contenido += `<li><strong>Dirección:</strong> ${handleNull(coordenada.direccion)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Estatus")) {
      contenido += `<li><strong>Estatus:</strong> ${handleNull(coordenada.estatus)}</li>`;
    }
    if (camposVisiblesGlobal.includes("RFC")) {
      contenido += `<li><strong>Rfc:</strong> ${handleNull(coordenada.rfc)}</li>`;
    }
    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
      contenido += `<li><strong>Fecha de Recepción:</strong> ${handleNull(coordenada.fechaRecepcion)}</li>`;
    }
    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
      contenido += `<li><strong>Estatus de Instalación:</strong> ${handleNull(coordenada.estatusInstalacion)}</li>`;
    }
    if (camposVisiblesGlobal.includes("TipoPermiso")) {
      contenido += `<li><strong>Tipo de Permiso:</strong> ${handleNull(coordenada.tipoPermiso)}</li>`;
    }
    if (camposVisiblesGlobal.includes("InicioVigencia")) {
      contenido += `<li><strong>Inicio de Vigencia:</strong> ${handleNull(coordenada.inicioVigencia)}</li>`;
    }
    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
      contenido += `<li><strong>Inicio de Operaciones:</strong> ${handleNull(coordenada.inicioOperaciones)}</li>`;
    }
    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
      contenido += `<li><strong>Capacidad Autorizada en MW:</strong> ${handleNull(coordenada.capacidadAutorizadaMW)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
      contenido += `<li><strong>Generación Estimada Anual:</strong> ${handleNull(coordenada.generacion_estimada_anual)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
      contenido += `<li><strong>Inversión Estimada en mdls:</strong> ${handleNull(coordenada.inversion_estimada_mdls)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Energetico_primario")) {
      contenido += `<li><strong>Energético Primario:</strong> ${handleNull(coordenada.energetico_primario)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Actividad_economica")) {
      contenido += `<li><strong>Actividad Económica:</strong> ${handleNull(coordenada.actividad_economica)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Tecnología")) {
      contenido += `<li><strong>Tecnología:</strong> ${handleNull(coordenada.tecnologia)}</li>`;
    }
    if (camposVisiblesGlobal.includes("EmpresaLíder")) {
      contenido += `<li><strong>Empresa Líder:</strong> ${handleNull(coordenada.empresaLider)}</li>`;
    }
    if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
      contenido += `<li><strong>País de Origen:</strong> ${handleNull(coordenada.paisDeOrigen)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Subasta")) {
      contenido += `<li><strong>Subasta:</strong> ${handleNull(coordenada.subasta)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Planta")) {
      contenido += `<li><strong>Planta:</strong> ${handleNull(coordenada.planta)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Combustible")) {
      contenido += `<li><strong>Combustible:</strong> ${handleNull(coordenada.combustible)}</li>`;
    }
    if (camposVisiblesGlobal.includes("FuenteEnergía")) {
      contenido += `<li><strong>Fuente de Energía:</strong> ${handleNull(coordenada.fuenteEnergia)}</li>`;
    }
    if (camposVisiblesGlobal.includes("Comentarios")) {
      contenido += `<li><strong>Comentarios:</strong> ${handleNull(coordenada.comentarios)}</li>`;
    }

    contenido += "</ul>";

    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
      contenido += `<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=${coordenada.numeroPermiso}'>Ver detalle</a>`;
    }
    contenido += `<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=${coordenada.latitudGeo},${coordenada.longitudGeo}&cbp=11,0,0,0,0' target='_blank'><b>Ver vista de calle</b></a>`;

    contenido += "</div>";
    return contenido;
  }

  // 5. Ejecución principal
  cargarCamposVisibles()
    .then(() => {
      limpiarMarcadores();

      return $.ajax({
        url: "/Indicadores/GetExpendiosAutorizadosElectricidad_Infra",
        type: "GET",
        contentType: "application/json"
      });
    })
    .then(response => {
      console.log("Expendios Autorizados de Electricidad:", response);
      datosExpendios = response;

      // Procesar datos para búsqueda
      response.forEach(item => {
        availableTerms.push(item.numeroPermiso);
        datosExpendiosAcumulados.push(item.numeroPermiso);
      });

      // Crear marcadores en el mapa
      const markers = L.markerClusterGroup();
      response.forEach(coordenada => {
        let iconoActual = iconos.convencional;
        switch (coordenada.clasificacion) {
          case "Hidroeléctrica": iconoActual = iconos.hidro; break;
          case "Bioenergía": iconoActual = iconos.bio; break;
          case "Eólica": iconoActual = iconos.eolica; break;
          case "Turbo Gas y Vapor": iconoActual = iconos.vapor; break;
          case "Combustión Interna": iconoActual = iconos.combustion; break;
          case "Cogeneración": iconoActual = iconos.cogeneracion; break;
          case "Cogeneración Eficiente": iconoActual = iconos.cogeneracionEficiente; break;
          case "Ciclo combinado": iconoActual = iconos.cicloCombinado; break;
          case "Carboeléctrica": iconoActual = iconos.carboelectrica; break;
          case "Solar Fotovoltaica": iconoActual = iconos.solar; break;
          case "Energía Cinética": iconoActual = iconos.cinetica; break;
          case "Nucleoeléctrica": iconoActual = iconos.nuclear; break;
          case "Lecho Fluidizado": iconoActual = iconos.lecho; break;
          case "Importación": iconoActual = iconos.importacion; break;
          case "Geotérmica": iconoActual = iconos.geotermica; break;
        }

        const marker = L.marker(
          [coordenada.latitudGeo, coordenada.longitudGeo],
          { icon: iconoActual }
        );
        marker.bindPopup(generarContenidoPopup(coordenada));
        markers.addLayer(marker);
      });
      map.addLayer(markers);

      // Actualizar totales
      const totalElementos = datosExpendios.length;
      te = totalElementos;
      const totalFormateado = totalElementos.toLocaleString();

      // Calcular y mostrar totales
      const totalPermisos = datosExpendios.length;
      const totalCapacidad = datosExpendios.reduce((sum, item) => sum + (item.capacidadAutorizadaMW || 0), 0);
      const totalGeneracion = datosExpendios.reduce((sum, item) => sum + (item.generacionEstimadaAnual || 0), 0);

      document.getElementById("totalPermisosAutorizados").textContent = totalPermisos.toLocaleString();
      document.getElementById("totalCapacidadAutorizada").textContent = totalCapacidad.toLocaleString() + " MW";
      document.getElementById("totalGeneracionEstimada").textContent = totalGeneracion.toLocaleString() + " MWh";

      // Configurar autocompletado
      const uniqueTerms = [...new Set(availableTerms)];
      autocomplete(document.getElementById("busquedaGeneralInput"), uniqueTerms);
    })
    .catch(error => {
      console.error("Error en CargaElectricidad:", error);
    });
}

CargaElectricidad();

// Funciones de Autocompletado
function autocomplete(inputElement, terms) {
  let currentFocus = -1;

  // Eventos del input
  inputElement.addEventListener("input", handleInput);

  // Evento global para cerrar listas
  document.addEventListener("click", e => closeAllLists(e.target));

  function handleInput(e) {
    const searchValue = this.value;

    // Limpiar resultados anteriores
    closeAllLists();
    if (!searchValue) return false;

    // Obtener y limpiar contenedor de sugerencias
    const suggestionList = document.getElementById("autocomplete-list");
    suggestionList.innerHTML = "";

    // Generar sugerencias
    terms.forEach(term => {
      if (term.substr(0, searchValue.length).toUpperCase() === searchValue.toUpperCase()) {
        const suggestionItem = createSuggestionItem(term, searchValue);
        suggestionList.appendChild(suggestionItem);
      }
    });
  }

  function createSuggestionItem(term, searchValue) {
    const div = document.createElement("DIV");
    div.innerHTML = `
      <strong>${term.substr(0, searchValue.length)}</strong>
      ${term.substr(searchValue.length)}
    `;

    // Manejar selección de sugerencia
    div.addEventListener("click", () => {
      inputElement.value = term;
      closeAllLists();
      buscarGeneral();
    });

    return div;
  }

  function closeAllLists(element) {
    const suggestionList = document.getElementById("autocomplete-list");
    if (element !== suggestionList && element !== inputElement) {
      suggestionList.innerHTML = "";
    }
  }
}

// Inicialización del autocompletado
function initializeAutocomplete() {
  // Filtrar términos duplicados
  const uniqueTerms = [...new Set(availableTerms)];
  console.log("Términos disponibles:", availableTerms);

  // Inicializar autocompletado
  const searchInput = document.getElementById("busquedaGeneralInput");
  if (searchInput) {
    autocomplete(searchInput, uniqueTerms);
  }
}

// Gestión de navegación
function activarElemento(elementoID) {
  // Remover clase activa de todos los elementos
  const menuItems = document.querySelectorAll(".navbarmapag a");
  menuItems.forEach(item => item.classList.remove("active"));

  // Activar elemento seleccionado
  const elementoActivo = document.getElementById(elementoID);
  if (elementoActivo) {
    elementoActivo.classList.add("active");
  }
}


