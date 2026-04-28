// ======================================
// ESTILOS DINÁMICOS
//Inyectar estilos CSS para controles, header y footer responsivo
var link = document.createElement('link');
link.rel = 'stylesheet';
link.type = 'text/css';
link.href = '/css/calculos_zonas.css';
document.head.appendChild(link);


// CONTROLES EN LA ESQUINA SUPERIOR IZQUIERDA
// ======================================
// HEADER DEL MAPA
// Función principal para agregar el header al mapa
function addHeaderToMap(map) {
  // Crear contenedor principal del header
  var header = document.createElement('div');
  header.id = "map-header";

  // ======================================
  // COLUMNA 1: Controles de mapa
  var col1 = document.createElement('div');
  col1.id = "header-col1";
  col1.style.cssText = "flex:1;display:flex;flex-direction:row;align-items:center;";

  // Botón de pantalla completa
  var fullscreenBtn = document.createElement('button');
  fullscreenBtn.style.cssText = "background:white;border:none;cursor:pointer;font-size:8px;padding:2px;";
  fullscreenBtn.title = "Pantalla Completa";
  fullscreenBtn.innerHTML = '<i class="bi bi-arrows-fullscreen"></i>';
  fullscreenBtn.addEventListener('click', function (e) {
    L.DomEvent.stopPropagation(e);
    L.DomEvent.preventDefault(e);
    if (!document.fullscreenElement) {
      map.getContainer().requestFullscreen().catch(err => {
        alert(`Error al habilitar pantalla completa: ${err.message} (${err.name})`);
      });
    } else {
      document.exitFullscreen();
    }
  });

  // Selector de mapas base
  var basemapSelect = document.createElement('select');
  basemapSelect.id = "headerBasemapSelector";
  basemapSelect.style.cssText = "font-size:8px;margin-left:5px;";
  var basemapOpts = { osm: 'Calles (OSM)', satellite: 'Satélite', topo: 'Topográfico' };
  for (var key in basemapOpts) {
    var option = document.createElement('option');
    option.value = key;
    option.text = basemapOpts[key];
    basemapSelect.appendChild(option);
  }
  basemapSelect.addEventListener('change', function (e) {
    var sel = e.target.value;
    for (var key in baseMaps) {
      if (map.hasLayer(baseMaps[key])) { map.removeLayer(baseMaps[key]); }
    }
    baseMaps[sel].addTo(map);
  });

  // Agregar elementos a columna 1
  col1.appendChild(fullscreenBtn);
  col1.appendChild(basemapSelect);

  // ======================================
  // COLUMNA 2: Buscador
  var col2 = document.createElement('div');
  col2.id = "header-col2";
  col2.style.cssText = "flex:1;display:flex;flex-direction:row;align-items:center;justify-content:center;";

  // Input de búsqueda
  var searchInput = document.createElement('input');
  searchInput.type = "text";
  searchInput.id = "busquedaGeneralInput";
  searchInput.placeholder = "Permiso, Entidad o Municipio";
  searchInput.style.cssText = "font-size:8px;width:70%;";

  // Contenedor para autocompletado
  var autocompleteList = document.createElement('div');
  autocompleteList.id = "autocomplete-list";
  autocompleteList.className = "autocomplete-items";

  // Botón de búsqueda
  var searchBtn = document.createElement('button');
  searchBtn.id = "search-btn";
  searchBtn.style.cssText = "font-size:8px;margin-left:5px;";
  searchBtn.innerHTML = '<i class="bi bi-search"></i>';
  searchBtn.addEventListener('click', function () {
    buscarGeneral();
  });

  // Agregar elementos a columna 2
  col2.appendChild(searchInput);
  col2.appendChild(searchBtn);
  col2.appendChild(autocompleteList);

  // ======================================
  // COLUMNA 3: Totales
  var col3 = document.createElement('div');
  col3.id = "header-col3";
  col3.style.cssText = "flex:1;text-align:center;font-size:8px;";
  col3.innerHTML = `
    <div style="display:inline-block;vertical-align:top;">
      <div style="text-align:center;margin-bottom:2px;">
        <img src="https://cdn.sassoapps.com/img_snier/vistas/s_energia.png" alt="Permisos" style="width:16px;height:16px;"><br>
        <span style="font-size:8px;">Permisos</span><br>
        <span id="totalPermisosAutorizados">0</span>
      </div>
    </div>
    <div style="display:inline-block;vertical-align:top;">
      <div style="text-align:center;margin-bottom:2px;">
        <img src="https://cdn.sassoapps.com/img_snier/vistas/sectore.png" alt="Capacidad" style="width:16px;height:16px;"><br>
        <span style="font-size:8px;">Capacidad (MW)</span><br>
        <span id="totalCapacidadAutorizada">0</span>
      </div>
    </div>
    <div style="display:inline-block;vertical-align:top;">
      <div style="text-align:center;">
        <img src="https://cdn.sassoapps.com/img_snier/vistas/casas.png" alt="Generación" style="width:16px;height:16px;"><br>
        <span style="font-size:8px;">Generación (MWh)</span><br>
        <span id="totalGeneracionEstimada">0</span>
      </div>
    </div>
  `;

  // Ensamblar el header
  header.appendChild(col1);
  header.appendChild(col2);
  header.appendChild(col3);

  // Prevenir propagación de eventos del mapa
  L.DomEvent.disableClickPropagation(header);

  // Agregar header al contenedor del mapa
  map.getContainer().appendChild(header);
}
// // Inicializar el header
addHeaderToMap(map);


