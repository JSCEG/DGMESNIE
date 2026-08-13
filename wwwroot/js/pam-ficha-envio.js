(function () {
    "use strict";

    // Envío de la ficha por correo: abre el modal, filtra destinatarios, genera el
    // archivo (PDF/PPT) en el cliente reutilizando window.pamFichaGenerar y lo POSTea
    // al endpoint Ficha/Enviar, que adjunta el archivo y manda el correo de saludo.

    function init() {
        const modal = document.getElementById("pam-envio-modal");
        const abrir = document.getElementById("pam-btn-enviar");
        const form = document.getElementById("pam-envio-form");
        if (!modal || !abrir || !form) return;

        const buscar = document.getElementById("pam-envio-buscar");
        const lista = document.getElementById("pam-envio-lista");
        const feedback = document.getElementById("pam-envio-feedback");
        const btnEnviar = document.getElementById("pam-envio-enviar");
        const items = [...lista.querySelectorAll(".pam-envio-item")];

        const cerrar = () => { modal.hidden = true; };
        const mostrar = () => { modal.hidden = false; buscar.value = ""; filtrar(""); buscar.focus(); };

        abrir.addEventListener("click", mostrar);
        modal.querySelectorAll("[data-envio-cerrar]").forEach(el => el.addEventListener("click", cerrar));
        document.addEventListener("keydown", e => { if (e.key === "Escape" && !modal.hidden) cerrar(); });

        function filtrar(q) {
            const t = q.trim().toLowerCase();
            items.forEach(it => {
                const match = !t || it.dataset.nombre.includes(t) || it.dataset.correo.includes(t);
                it.style.display = match ? "" : "none";
            });
        }
        buscar?.addEventListener("input", () => filtrar(buscar.value));

        function setFeedback(msg, tipo) {
            feedback.hidden = false;
            feedback.className = "pam-envio-feedback is-" + (tipo || "info");
            const icono = tipo === "ok" ? '<i class="bi bi-check-circle"></i>'
                : tipo === "error" ? '<i class="bi bi-exclamation-triangle"></i>'
                : '<span class="pam-envio-spin" aria-hidden="true"></span>';
            feedback.innerHTML = `${icono}<span>${msg}</span>`;
        }

        function setEnviando(activo, texto) {
            btnEnviar.disabled = activo;
            btnEnviar.innerHTML = activo
                ? '<span class="pam-envio-spin pam-envio-spin--btn" aria-hidden="true"></span> ' + (texto || "Enviando…")
                : '<i class="bi bi-send" aria-hidden="true"></i> Enviar ficha';
        }

        form.addEventListener("submit", async event => {
            event.preventDefault();
            const seleccion = [...form.querySelectorAll("input[name='usuario']:checked")].map(c => Number(c.value));
            if (seleccion.length === 0) { setFeedback("Selecciona al menos un destinatario.", "error"); return; }
            if (typeof window.pamFichaGenerar !== "function") { setFeedback("El generador de la ficha no está disponible. Recarga la página.", "error"); return; }

            const formato = form.querySelector("input[name='formato']:checked")?.value || "pdf";
            const clave = form.dataset.clave || "";
            const endpoint = form.dataset.endpoint || "/InformePormenorizado/ProyectosIdentificados/Ficha/Enviar";
            const mensaje = document.getElementById("pam-envio-mensaje")?.value || "";
            const token = form.querySelector("input[name='__RequestVerificationToken']")?.value;

            setEnviando(true, "Generando ficha…");
            setFeedback("Generando la ficha en " + formato.toUpperCase() + "… esto toma unos segundos.", "info");

            let etapa = "generacion";
            try {
                const archivo = await window.pamFichaGenerar(formato);
                etapa = "envio";
                setEnviando(true, "Enviando…");
                setFeedback(`Enviando la ficha a ${seleccion.length} destinatario(s)…`, "info");
                const payload = {
                    clavePem: clave,
                    formato: formato,
                    archivoBase64: archivo.base64,
                    nombreArchivo: archivo.nombre,
                    usuarioIds: seleccion,
                    mensajeAdicional: mensaje
                };
                if (form.dataset.tipo) payload.tipo = form.dataset.tipo;
                if (form.dataset.numeroPermiso) payload.numeroPermiso = form.dataset.numeroPermiso;

                const respuesta = await fetch(endpoint, {
                    method: "POST",
                    headers: { "Content-Type": "application/json", "RequestVerificationToken": token },
                    body: JSON.stringify(payload)
                });

                let data = {};
                try { data = await respuesta.json(); } catch { /* respuesta no-JSON */ }

                if (respuesta.ok && data.ok) {
                    setFeedback(data.mensaje || "Ficha enviada.", "ok");
                    form.querySelectorAll("input[name='usuario']:checked").forEach(c => { c.checked = false; });
                    setTimeout(() => { if (!modal.hidden) cerrar(); }, 2200);
                } else {
                    setFeedback(data.mensaje || "No fue posible enviar la ficha.", "error");
                }
            } catch (error) {
                console.error("Error al enviar la ficha:", error);
                setFeedback(
                    etapa === "generacion"
                        ? "No se pudo generar el archivo adjunto. Recarga la ficha e inténtalo nuevamente."
                        : "El archivo se generó, pero no fue posible enviarlo. Verifica el servicio de correo e inténtalo nuevamente.",
                    "error");
            } finally {
                setEnviando(false);
            }
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
