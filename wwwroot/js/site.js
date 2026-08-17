// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Navegación superior de escritorio.
//
// El Dashboard de Proyectos registra una capa amplia de eventos y componentes
// interactivos. Para que sus listeners no interfieran con la delegación global
// de Bootstrap, el menú institucional controla de forma explícita únicamente
// sus propios dropdowns.
(function initializeDesktopTopnav() {
    'use strict';

    const navSelector = '#desktopTopnav';
    const triggerSelector = `${navSelector} .desktop-topnav__trigger`;
    const groupSelector = `${navSelector} .desktop-topnav__group`;

    function setGroupOpen(group, shouldOpen) {
        if (!group) return;

        const trigger = group.querySelector(':scope > .desktop-topnav__trigger');
        const menu = group.querySelector(':scope > .desktop-topnav__menu');
        if (!trigger || !menu) return;

        group.classList.toggle('show', shouldOpen);
        trigger.classList.toggle('show', shouldOpen);
        menu.classList.toggle('show', shouldOpen);
        trigger.setAttribute('aria-expanded', shouldOpen ? 'true' : 'false');
    }

    function closeDesktopMenus(exceptGroup) {
        document.querySelectorAll(groupSelector).forEach(group => {
            if (group !== exceptGroup) setGroupOpen(group, false);
        });
    }

    window.addEventListener('click', function (event) {
        const trigger = event.target.closest(triggerSelector);

        if (trigger) {
            const group = trigger.closest(groupSelector);
            const menu = group?.querySelector(':scope > .desktop-topnav__menu');
            const shouldOpen = !menu?.classList.contains('show');

            // Evita que el Data API de Bootstrap procese el mismo clic otra vez.
            event.preventDefault();
            event.stopImmediatePropagation();

            closeDesktopMenus(group);
            setGroupOpen(group, shouldOpen);
            return;
        }

        if (!event.target.closest(groupSelector)) closeDesktopMenus();
    }, true);

    document.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape') return;

        const openTrigger = document.querySelector(`${triggerSelector}[aria-expanded="true"]`);
        if (!openTrigger) return;

        closeDesktopMenus();
        openTrigger.focus();
    });
})();
