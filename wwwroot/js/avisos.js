/*
 * avisos.js — avisos y confirmaciones del sistema.
 *
 * Sustituye a alert() y confirm() del navegador, que hoy se usan 158 y 13
 * veces en las vistas. Los diálogos nativos no se pueden estilar, salen con el
 * nombre del host en el título, bloquean el hilo y no dicen qué se conserva.
 *
 *   Aviso.exito('Dictamen guardado', 'CV-2026-0014 quedó como «Continúa».');
 *   Aviso.problema('Faltan 3 campos obligatorios', 'Capacidad, GCR y fecha.');
 *   Aviso.atencion('Tu sesión expira en 2 minutos');
 *   Aviso.dato('Datos actualizados al 31 de julio de 2026');
 *
 *   const ok = await Aviso.confirmar({
 *       titulo: 'Eliminar el proyecto CV-2026-0071',
 *       texto: 'Se borrarán su expediente, sus 4 comentarios y su dictamen.',
 *       nota: 'La acción no se puede deshacer.',
 *       confirmar: 'Eliminar proyecto'
 *   });
 *   if (ok) { ... }
 */
(function (global) {
    'use strict';

    var ICONOS = {
        exito: 'fa-circle-check',
        atencion: 'fa-triangle-exclamation',
        problema: 'fa-circle-exclamation',
        dato: 'fa-circle-info'
    };

    function contenedor() {
        var c = document.getElementById('avisosPila');
        if (!c) {
            c = document.createElement('div');
            c.id = 'avisosPila';
            c.className = 'avisos-pila';
            document.body.appendChild(c);
        }
        return c;
    }

    function mostrar(tono, titulo, texto, opciones) {
        opciones = opciones || {};
        var el = document.createElement('div');
        el.className = 'aviso aviso--' + tono + ' aviso--flotante';
        el.setAttribute('role', tono === 'problema' ? 'alert' : 'status');

        var cuerpo = '<span class="aviso__titulo">' + escapar(titulo) + '</span>';
        if (texto) cuerpo += escapar(texto);

        el.innerHTML =
            '<i class="aviso__icono fas ' + ICONOS[tono] + '"></i>' +
            '<div class="aviso__cuerpo">' + cuerpo + '</div>' +
            '<button type="button" class="aviso__cerrar" aria-label="Cerrar">' +
            '<i class="fas fa-xmark"></i></button>';

        el.querySelector('.aviso__cerrar').addEventListener('click', function () { quitar(el); });
        contenedor().appendChild(el);

        // Un problema se queda hasta que lo cierren; el resto se va solo.
        var vida = opciones.persistente || tono === 'problema' ? 0 : (opciones.ms || 5000);
        if (vida) setTimeout(function () { quitar(el); }, vida);
        return el;
    }

    function quitar(el) {
        if (!el || !el.parentElement) return;
        el.classList.add('is-saliendo');
        setTimeout(function () { if (el.parentElement) el.remove(); }, 180);
    }

    function escapar(v) {
        var d = document.createElement('div');
        d.textContent = v == null ? '' : String(v);
        return d.innerHTML;
    }

    /* Confirmación destructiva. Devuelve una promesa con true o false, para que
     * el llamador se lea igual que con confirm() pero sin bloquear el hilo. */
    function confirmar(o) {
        o = o || {};
        return new Promise(function (resolver) {
            var el = document.createElement('div');
            el.className = 'modal fade';
            el.tabIndex = -1;
            el.innerHTML =
                '<div class="modal-dialog modal-dialog-centered"><div class="modal-content">' +
                '<div class="modal-sec__header"><div>' +
                '<span class="modal-sec__kicker">' + escapar(o.kicker || 'Confirmar acción') + '</span>' +
                '<h5 class="modal-sec__title">' + escapar(o.titulo || '¿Continuar?') + '</h5>' +
                '</div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button></div>' +
                '<div class="modal-sec__body">' +
                (o.texto ? '<p class="mb-2">' + escapar(o.texto) + '</p>' : '') +
                (o.nota ? '<p class="aviso aviso--atencion mb-0"><i class="aviso__icono fas fa-triangle-exclamation"></i>' +
                          '<span class="aviso__cuerpo">' + escapar(o.nota) + '</span></p>' : '') +
                '</div>' +
                '<div class="modal-sec__foot">' +
                '<button type="button" class="btn-sec" data-bs-dismiss="modal">' + escapar(o.cancelar || 'Cancelar') + '</button>' +
                '<button type="button" class="btn-sec btn-sec--primary" data-confirmar>' + escapar(o.confirmar || 'Continuar') + '</button>' +
                '</div></div></div>';

            // Se cuelga de <body>: dentro de <main>, que trae isolation: isolate,
            // el z-index no compite con el velo y el modal queda por debajo.
            document.body.appendChild(el);
            var modal = new bootstrap.Modal(el);
            var respuesta = false;

            el.querySelector('[data-confirmar]').addEventListener('click', function () {
                respuesta = true;
                modal.hide();
            });
            el.addEventListener('hidden.bs.modal', function () {
                el.remove();
                resolver(respuesta);
            });

            modal.show();
        });
    }

    global.Aviso = {
        exito: function (t, x, o) { return mostrar('exito', t, x, o); },
        atencion: function (t, x, o) { return mostrar('atencion', t, x, o); },
        problema: function (t, x, o) { return mostrar('problema', t, x, o); },
        dato: function (t, x, o) { return mostrar('dato', t, x, o); },
        confirmar: confirmar
    };
})(window);
