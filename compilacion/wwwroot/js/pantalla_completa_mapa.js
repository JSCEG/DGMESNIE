document.addEventListener('DOMContentLoaded', () => {
    const mapElement = document.getElementById('map');
    const fullscreenButton = document.getElementById('fullscreen-btn');
    const searchContainer = document.getElementById('search-container');
    const mainContainer = document.getElementById('main-container');
    const fullscreenElements = document.createElement('div');
    fullscreenElements.id = 'fullscreen-elements';

    const originalMapParent = mapElement.parentNode;
    const originalSearchParent = searchContainer.parentNode;
    const originalMapNextSibling = mapElement.nextSibling;
    const originalSearchNextSibling = searchContainer.nextSibling;

    fullscreenButton.addEventListener('click', () => {
      if (!document.fullscreenElement) {
        mainContainer.appendChild(fullscreenElements);
        fullscreenElements.appendChild(searchContainer);
        fullscreenElements.appendChild(mapElement);
        fullscreenElements.requestFullscreen().catch(err => {
          alert(`Error al habilitar pantalla completa: ${err.message} (${err.name})`);
        });
      } else {
        document.exitFullscreen();
      }
    });

    document.addEventListener('fullscreenchange', () => {
      if (document.fullscreenElement) {
        fullscreenButton.textContent = "Salir de Pantalla Completa";
        mainContainer.classList.add('fullscreen-active');
        mapElement.style.height = "calc(100vh - 60px)";
        searchContainer.style.backgroundColor = "white";
      } else {
        fullscreenButton.textContent = "Pantalla Completa";
        if (originalSearchNextSibling) {
          originalSearchParent.insertBefore(searchContainer, originalSearchNextSibling);
        } else {
          originalSearchParent.appendChild(searchContainer);
        }
        if (originalMapNextSibling) {
          originalMapParent.insertBefore(mapElement, originalMapNextSibling);
        } else {
          originalMapParent.appendChild(mapElement);
        }
        mainContainer.classList.remove('fullscreen-active');
        mapElement.style.height = "500px";
        fullscreenElements.style.height = 'auto';
      }
    });
  });