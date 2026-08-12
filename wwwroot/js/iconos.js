/* Traductor de nombres de icono.
 *
 * La plataforma pinta con Bootstrap Icons, pero los bloques ModuleInfo de las
 * vistas guardan el nombre del icono como dato ("user-shield", "chart-line",
 * "hammer"...) y muchos de esos nombres vienen de Font Awesome. Son cientos de
 * vistas, así que en vez de reescribir el dato en todas, se traduce al pintar.
 *
 * Uso:  <i class="${Icono.clase(r.icon)}"></i>
 *
 * La tabla trae los 114 nombres de Font Awesome que la plataforma llegó a usar.
 * Un nombre que no esté en ella pasa tal cual, porque lo más probable es que ya
 * sea un nombre válido de Bootstrap Icons; si no lo es, no pinta nada. Por eso,
 * cuando agregues un icono nuevo escríbelo con su nombre de Bootstrap Icons y
 * no con el de Font Awesome.
 */
(function (global) {
    'use strict';

    var TABLA = {
        'arrow-down': 'arrow-down',
        'arrow-left': 'arrow-left',
        'arrow-right': 'arrow-right',
        'arrow-up': 'arrow-up',
        'arrows-up-down': 'arrow-down-up',
        'atom': 'radioactive',
        'balance-scale': 'bank',
        'battery-three-quarters': 'battery-half',
        'bell': 'bell',
        'bolt': 'lightning-charge',
        'book-open': 'book',
        'calendar-days': 'calendar3',
        'certificate': 'award',
        'charging-station': 'ev-station',
        'chart': 'bar-chart',
        'chart-bar': 'bar-chart',
        'chart-line': 'graph-up',
        'chart-network': 'diagram-3',
        'chart-simple': 'bar-chart',
        'check': 'check-lg',
        'check-circle': 'check-circle',
        'check-double': 'check-all',
        'chevron-down': 'chevron-down',
        'chevron-left': 'chevron-left',
        'chevron-right': 'chevron-right',
        'circle-info': 'info-circle',
        'circle-question': 'question-circle',
        'city': 'buildings',
        'clipboard-list': 'clipboard-data',
        'cogs': 'gear-wide-connected',
        'comments': 'chat-dots',
        'compass': 'compass',
        'copy': 'copy',
        'cube': 'box',
        'cubes': 'boxes',
        'diagram-project': 'diagram-3',
        'dollar-sign': 'currency-dollar',
        'down-left-and-up-right-to-center': 'fullscreen-exit',
        'download': 'download',
        'edit': 'pencil-square',
        'ellipsis': 'three-dots',
        'envelope': 'envelope',
        'exchange-alt': 'arrow-left-right',
        'exclamation-triangle': 'exclamation-triangle',
        'eye': 'eye',
        'file-alt': 'file-earmark-text',
        'file-image': 'file-earmark-image',
        'file-import': 'file-earmark-arrow-down',
        'file-lines': 'file-earmark-text',
        'file-pdf': 'file-earmark-pdf',
        'file-signature': 'file-earmark-post',
        'flask': 'thermometer-half',
        'folder-open': 'folder2-open',
        'gas-pump': 'fuel-pump',
        'globe-americas': 'globe-americas',
        'graduation-cap': 'mortarboard',
        'grip-vertical': 'grip-vertical',
        'id-card': 'person-vcard',
        'inbox': 'inbox',
        'industry': 'buildings',
        'info-circle': 'info-circle',
        'key': 'key',
        'layer-group': 'layers',
        'leaf': 'tree',
        'lightning-bolt': 'lightning-charge',
        'list': 'list-ul',
        'list-check': 'list-check',
        'location-dot': 'geo-alt',
        'map-marked-alt': 'geo-alt',
        'map-marker-alt': 'geo-alt',
        'microscope': 'binoculars',
        'oil-well': 'droplet',
        'palette': 'palette',
        'paper-plane': 'send',
        'pen': 'pencil',
        'pen-to-square': 'pencil-square',
        'play': 'play-fill',
        'plus': 'plus-lg',
        'presentation-play': 'easel2',
        'project-diagram': 'diagram-3',
        'question-circle': 'question-circle',
        'rotate-right': 'arrow-clockwise',
        'route': 'signpost-split',
        'save': 'save',
        'scale-balanced': 'bank',
        'search': 'search',
        'shield-alt': 'shield',
        'shield-halved': 'shield-check',
        'sign-in-alt': 'box-arrow-in-right',
        'sitemap': 'diagram-3',
        'solar-panel': 'sun',
        'sort-amount-up': 'sort-down-alt',
        'spinner': 'arrow-repeat',
        'sync-alt': 'arrow-repeat',
        'table-columns': 'layout-three-columns',
        'tachometer-alt': 'speedometer2',
        'tasks': 'list-task',
        'tools': 'tools',
        'trash': 'trash',
        'trash-can': 'trash',
        'triangle-exclamation': 'exclamation-triangle',
        'university': 'bank',
        'up-right-and-down-left-from-center': 'arrows-fullscreen',
        'up-right-from-square': 'box-arrow-up-right',
        'user-check': 'person-check',
        'user-cog': 'person-gear',
        'user-plus': 'person-plus',
        'user-shield': 'person-lock',
        'user-tie': 'person-badge',
        'users': 'people',
        'users-cog': 'person-gear',
        'vote-yea': 'check2-square',
        'whatsapp': 'whatsapp',
        'xmark': 'x-lg'
    };

    /* Los que Bootstrap Icons ya trae con ese mismo nombre y por tanto no
       necesitan traducción. Se listan para poder distinguir «no hace falta
       traducirlo» de «no lo conozco». */
    var PROPIOS = ('bar-chart bar-chart-line building calculator calendar-event check ' +
        'check-circle check-square clipboard-check clock clock-history cloud database ' +
        'download exclamation-triangle eye file-code fire globe graph-up graph-up-arrow ' +
        'hammer lightbulb lightning lightning-charge list map newspaper person-badge ' +
        'search shield-check signal sun upload water wind').split(' ');

    var Icono = {
        /** Nombre de Bootstrap Icons para un nombre cualquiera. */
        nombre: function (n) {
            if (!n) return 'dot';
            n = String(n).trim().replace(/^(?:fa[srlbd]?-|bi-)/, '');
            if (TABLA[n]) return TABLA[n];
            if (PROPIOS.indexOf(n) !== -1) return n;
            return n; /* puede ser un icono válido que no listamos; el CSS decide */
        },
        /** Atributo class listo para usar. */
        clase: function (n) {
            return 'bi bi-' + Icono.nombre(n);
        }
    };

    global.Icono = Icono;
})(window);
