/*
 * navtree-orden.js — reordenar por arrastre en el árbol de Gestión de Secciones.
 *
 * Los tres niveles (secciones, módulos y vistas) se reordenan igual: el asa está
 * siempre visible, no hay «modo ordenar» ni Guardar/Cancelar, y al soltar se
 * renumera la lista y se persiste. Cada pantalla solo cambia el endpoint y el
 * nombre del identificador que espera el servidor.
 *
 *   activarArrastreNavtree(document.getElementById('navtree'), {
 *       campoId: 'seccionId',        // nombre que espera el DTO del controlador
 *       atributo: 'seccionId',       // data-seccion-id del <section>
 *       url: '/Secciones/ActualizarOrdenSecciones'
 *   });
 */
(function (global) {
    'use strict';

    function avisoBreve(texto) {
        if (typeof Swal === 'undefined') return;
        Swal.fire({
            toast: true, position: 'bottom-end', icon: 'success',
            title: texto, showConfirmButton: false, timer: 1600, timerProgressBar: true
        });
    }

    function activarArrastreNavtree(contenedor, opciones) {
        if (!contenedor) return;

        var selector = opciones.selectorItem || '.navtree-item';
        var arrastrado = null;

        function persistir() {
            var items = Array.prototype.slice.call(contenedor.querySelectorAll(selector));
            var cambios = [];

            items.forEach(function (item, i) {
                var nuevoOrden = i + 1;
                var numero = item.querySelector('.navtree-num');
                if (numero) numero.textContent = String(nuevoOrden).padStart(2, '0');

                if (parseInt(item.dataset.orden, 10) !== nuevoOrden) {
                    var cambio = { nuevoOrden: nuevoOrden };
                    cambio[opciones.campoId] = parseInt(item.dataset[opciones.atributo], 10);
                    cambios.push(cambio);
                }
            });

            if (!cambios.length) return;

            $.ajax({
                url: opciones.url,
                method: 'POST',
                data: JSON.stringify(cambios),
                contentType: 'application/json'
            }).done(function () {
                items.forEach(function (item, i) { item.dataset.orden = i + 1; });
                avisoBreve('Orden guardado');
            }).fail(function () {
                Swal.fire('Error', 'No se pudo guardar el nuevo orden. Recarga la página.', 'error');
            });
        }

        contenedor.querySelectorAll(selector).forEach(function (item) {
            var asa = item.querySelector('.navtree-handle:not(.navtree-handle--placeholder)');
            if (!asa) return;

            // draggable solo mientras el asa está presionada: así el texto de la
            // fila se sigue pudiendo seleccionar con el ratón.
            asa.addEventListener('mousedown', function () { item.draggable = true; });
            asa.addEventListener('touchstart', function () { item.draggable = true; }, { passive: true });
            document.addEventListener('mouseup', function () { item.draggable = false; });

            item.addEventListener('dragstart', function (ev) {
                arrastrado = item;
                item.classList.add('is-dragging');
                ev.dataTransfer.effectAllowed = 'move';
                ev.dataTransfer.setData('text/plain', item.dataset[opciones.atributo] || '');
            });

            item.addEventListener('dragend', function () {
                item.classList.remove('is-dragging');
                item.draggable = false;
                contenedor.querySelectorAll('.is-drop-target').forEach(function (e) {
                    e.classList.remove('is-drop-target');
                });
                if (arrastrado) { arrastrado = null; persistir(); }
            });

            item.addEventListener('dragover', function (ev) {
                if (!arrastrado || arrastrado === item) return;
                ev.preventDefault();
                var caja = item.getBoundingClientRect();
                var despues = (ev.clientY - caja.top) > caja.height / 2;
                item.classList.add('is-drop-target');
                if (despues) item.after(arrastrado); else item.before(arrastrado);
            });

            item.addEventListener('dragleave', function () {
                item.classList.remove('is-drop-target');
            });
        });
    }

    global.activarArrastreNavtree = activarArrastreNavtree;
})(window);
