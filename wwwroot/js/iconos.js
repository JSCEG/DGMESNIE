/* Traductor de nombres de icono.
 *
 * La plataforma pinta con Bootstrap Icons, pero los bloques ModuleInfo de las
 * vistas guardan el nombre del icono como dato ("user-shield", "chart-line",
 * "hammer"...) y muchos de esos nombres vienen de Font Awesome. Son cientos de
 * vistas, así que en vez de reescribir el dato en todas, se traduce al pintar.
 *
 * Uso:  <i class="${Icono.clase(r.icon)}"></i>
 *
 * Si el nombre ya es válido en Bootstrap Icons pasa tal cual. Si no está ni en
 * la tabla ni en Bootstrap Icons se pinta un punto, que es visible y no rompe
 * la alineación: mejor eso que un hueco en blanco.
 */
(function (global) {
    'use strict';

    var TABLA = {
        'atom': 'radioactive',
        'balance-scale': 'bank',
        'battery-three-quarters': 'battery-half',
        'bolt': 'lightning-charge',
        'book-open': 'book',
        'certificate': 'award',
        'charging-station': 'ev-station',
        'chart': 'bar-chart',
        'chart-bar': 'bar-chart',
        'chart-line': 'graph-up',
        'chart-network': 'diagram-3',
        'check-double': 'check-all',
        'city': 'buildings',
        'clipboard-list': 'clipboard-data',
        'comments': 'chat-dots',
        'dollar-sign': 'currency-dollar',
        'edit': 'pencil-square',
        'exchange-alt': 'arrow-left-right',
        'file-alt': 'file-earmark-text',
        'file-signature': 'file-earmark-post',
        'flask': 'thermometer-half',
        'graduation-cap': 'mortarboard',
        'industry': 'buildings',
        'leaf': 'tree',
        'lightning-bolt': 'lightning-charge',
        'map-marked-alt': 'geo-alt',
        'map-marker-alt': 'geo-alt',
        'microscope': 'binoculars',
        'oil-well': 'droplet',
        'project-diagram': 'diagram-3',
        'shield-alt': 'shield',
        'solar-panel': 'sun',
        'university': 'bank',
        'user-check': 'person-check',
        'user-cog': 'person-gear',
        'user-shield': 'person-lock',
        'user-tie': 'person-badge',
        'users': 'people',
        'users-cog': 'person-gear',
        'vote-yea': 'check2-square'
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
