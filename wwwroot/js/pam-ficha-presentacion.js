(function () {
    "use strict";

    const WIDTH = 1333;
    const HEIGHT = 750;
    const PDF_WIDTH_MM = 338.6582;
    const PDF_HEIGHT_MM = 190.5;

    function init() {
        if (window.actualizarPreloaderFicha) {
            window.actualizarPreloaderFicha(88, "Organizando las láminas y los recursos visuales.", "Presentación ejecutiva");
        }
        const page = document.querySelector(".pam-ficha-page");
        const shell = document.getElementById("pam-deck-shell");
        const deck = document.getElementById("pam-deck");
        if (page?.classList.contains('convocatoria-ficha-page')) window.convocatoriaFichaIndice?.actualizar(deck);
        const slides = Array.from(document.querySelectorAll("[data-slide]"));
        const counter = document.querySelector("[data-counter]");
        const exportStatus = document.getElementById("pam-export-status");
        const exportMessage = exportStatus?.querySelector("[data-export-message]");
        const sectionNav = document.querySelector('[data-convocatoria-slide-nav]');

        if (!page || !shell || !deck || slides.length === 0) return;

        let current = 0;
        let busy = false;

        const pad = value => String(value).padStart(2, "0");
        const filename = page.dataset.filename || "Ficha_PAMRNT";

        function setCounter() {
            if (counter) counter.textContent = `${pad(current + 1)} / ${pad(slides.length)}`;
            if (sectionNav) {
                if (sectionNav.options.length !== slides.length) {
                    sectionNav.replaceChildren(...slides.map((slide,index) => new Option(`${pad(index + 1)} · ${slide.dataset.label}`, String(index))));
                }
                sectionNav.value = String(current);
            }
        }

        // Mixtos II conoce el número de geometrías al resolver el KML.
        // No exportar páginas de comparación vacías ni dejar numeración heredada del PAM.
        if (page.classList.contains('convocatoria-ficha-page')) {
            document.addEventListener('convocatoria:territorial-ready', event => {
                const elementCount = event.detail.resultado.elementos.length;
                const activeSlide = slides[current];
                slides.splice(0, slides.length, ...document.querySelectorAll('[data-slide]'));
                for (let index = slides.length - 1; index >= 0; index--) {
                    const comparison = slides[index].querySelector('[data-pam-territorial-comparison]');
                    if (comparison && Number(comparison.dataset.territorialPage) * 3 >= elementCount && (elementCount || Number(comparison.dataset.territorialPage) > 0)) {
                        slides[index].remove();
                        slides.splice(index, 1);
                    }
                }
                window.convocatoriaFichaIndice?.actualizar(deck);
                slides.splice(0, slides.length, ...deck.querySelectorAll('[data-slide]'));
                current = Math.max(0, slides.indexOf(activeSlide));
                setCounter();
                if (!busy) show(current);
            }, { once: true });
        }

        function show(index, updateHash) {
            if (busy) return;
            current = Math.max(0, Math.min(slides.length - 1, Number(index) || 0));
            slides.forEach((slide, slideIndex) => {
                const active = slideIndex === current;
                slide.classList.toggle("is-active", active);
                slide.setAttribute("aria-hidden", active ? "false" : "true");
            });
            setCounter();
            if (updateHash !== false && history.replaceState) {
                history.replaceState(null, "", `#lamina-${pad(current + 1)}`);
            }
            // Avisa a módulos con contenido perezoso (mapas Leaflet) que hay lámina nueva visible.
            document.dispatchEvent(new CustomEvent("pam:slide-shown", { detail: { index: current } }));
        }

        function resizeDeck() {
            const fullscreen = document.fullscreenElement === shell;
            const availableWidth = fullscreen ? window.innerWidth : Math.max(320, page.clientWidth - 44);
            const top = shell.getBoundingClientRect().top;
            const availableHeight = fullscreen ? window.innerHeight : Math.max(280, window.innerHeight - top - 24);
            const scale = fullscreen
                ? Math.min(availableWidth / WIDTH, availableHeight / HEIGHT)
                : Math.min(1, availableWidth / WIDTH, availableHeight / HEIGHT);

            deck.style.transform = `scale(${scale})`;
            if (fullscreen) {
                shell.style.width = "100vw";
                shell.style.height = "100vh";
            } else {
                shell.style.width = `${Math.round(WIDTH * scale)}px`;
                shell.style.height = `${Math.round(HEIGHT * scale)}px`;
            }
        }

        function syncFullscreenControls() {
            const fullscreen = document.fullscreenElement === shell;
            document.querySelectorAll("[data-fullscreen]").forEach(button => {
                button.setAttribute("aria-pressed", fullscreen ? "true" : "false");
                button.title = fullscreen ? "Salir de pantalla completa" : "Pantalla completa";

                const icon = button.querySelector("i");
                if (icon) {
                    icon.classList.toggle("bi-arrows-fullscreen", !fullscreen);
                    icon.classList.toggle("bi-fullscreen-exit", fullscreen);
                }

                const label = button.querySelector("span");
                if (label) label.textContent = fullscreen ? "Salir" : "Presentar";
            });
        }

        function isEditableTarget(target) {
            return target instanceof HTMLElement &&
                (target.matches("input, textarea, select, [contenteditable='true']") || Boolean(target.closest("[contenteditable='true']")));
        }

        async function toggleFullscreen() {
            try {
                if (document.fullscreenElement === shell) {
                    await document.exitFullscreen();
                } else {
                    await shell.requestFullscreen();
                }
            } catch (error) {
                notify("No fue posible abrir la presentación en pantalla completa.", "error");
            }
        }

        function notify(message, icon) {
            if (window.Swal?.fire) {
                return window.Swal.fire({
                    title: icon === "error" ? "No se pudo completar" : "Ficha ejecutiva",
                    text: message,
                    icon: icon || "info",
                    confirmButtonColor: "#9b2247",
                    confirmButtonText: "Entendido"
                });
            }
            window.alert(message);
        }

        function setBusy(value, message) {
            busy = value;
            page.classList.toggle("is-exporting", value);
            document.querySelectorAll("[data-export]").forEach(button => {
                button.disabled = value;
                button.setAttribute("aria-busy", value ? "true" : "false");
            });
            if (exportStatus) exportStatus.hidden = !value;
            if (exportMessage) exportMessage.textContent = message || "";
        }

        function nextPaint() {
            return new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        }

        async function fetchImageBlobAsDataUrl(url) {
            try {
                const corsUrl = url.startsWith("data:")
                    ? url
                    : url + (url.includes("?") ? "&" : "?") + "cors=" + Date.now();
                const res = await fetch(corsUrl, { mode: "cors", cache: "no-store" });
                if (!res.ok) return null;
                const blob = await res.blob();
                return new Promise(resolve => {
                    const reader = new FileReader();
                    reader.onloadend = () => resolve(reader.result);
                    reader.onerror = () => resolve(null);
                    reader.readAsDataURL(blob);
                });
            } catch (e) {
                return null;
            }
        }

        async function convertImgToDataUrl(img) {
            if (!img.src || img.src.startsWith("data:")) return img.src;
            let dataUrl = await fetchImageBlobAsDataUrl(img.src);
            if (dataUrl) return dataUrl;

            return new Promise(resolve => {
                const tempImg = new Image();
                tempImg.crossOrigin = "anonymous";
                tempImg.onload = () => {
                    try {
                        const canvas = document.createElement("canvas");
                        canvas.width = tempImg.naturalWidth || tempImg.width || 800;
                        canvas.height = tempImg.naturalHeight || tempImg.height || 600;
                        const ctx = canvas.getContext("2d");
                        ctx.drawImage(tempImg, 0, 0);
                        resolve(canvas.toDataURL("image/png"));
                    } catch (e) {
                        resolve(null);
                    }
                };
                tempImg.onerror = () => resolve(null);
                tempImg.src = img.src + (img.src.includes("?") ? "&" : "?") + "cors=" + Date.now();
            });
        }

        async function asegurarImagenesPrecargadas(slide) {
            const imgs = Array.from(slide.querySelectorAll("img"));
            for (const img of imgs) {
                if (img.src && !img.dataset.dataUrlCargado) {
                    const dataUrl = await convertImgToDataUrl(img);
                    if (dataUrl && dataUrl.startsWith("data:")) {
                        img.src = dataUrl;
                        img.dataset.dataUrlCargado = "true";
                        if (img.decode) {
                            try { await img.decode(); } catch (e) {}
                        }
                    }
                }
            }
        }

        async function esperarRecursosVisuales(slide) {
            const imagenes = Array.from(slide.querySelectorAll("img"));
            await Promise.all(imagenes.map(async img => {
                if (!img.complete) {
                    await new Promise(resolve => {
                        const finalizar = () => resolve();
                        img.addEventListener("load", finalizar, { once: true });
                        img.addEventListener("error", finalizar, { once: true });
                        setTimeout(finalizar, 5000);
                    });
                }
                if (img.decode) {
                    try { await img.decode(); } catch (e) { }
                }
            }));
            if (document.fonts?.ready) await document.fonts.ready;
            await nextPaint();
        }

        // Leaflet puede conservar canvases auxiliares vacíos (0 × 0) para capas
        // que no dibujaron geometría. html2canvas intenta convertirlos en un
        // patrón y el navegador lanza InvalidStateError, aunque no sean visibles.
        // Se excluyen sólo durante la captura; los mapas y gráficas válidos se
        // conservan completos en el PDF/PPT.
        function omitirCanvasVacios() {
            const marcados = [];
            // html2canvas clona el documento completo antes de aislar la lámina;
            // por eso también deben marcarse los canvases vacíos de otras láminas.
            document.querySelectorAll("canvas").forEach(canvas => {
                if (canvas.width > 0 && canvas.height > 0) return;
                if (!canvas.hasAttribute("data-html2canvas-ignore")) {
                    canvas.setAttribute("data-html2canvas-ignore", "true");
                    marcados.push(canvas);
                }
            });
            return () => marcados.forEach(canvas => canvas.removeAttribute("data-html2canvas-ignore"));
        }

        async function renderSlide(slide, slideNumber) {
            if (window.convocatoriaFichaPreparar) await window.convocatoriaFichaPreparar(slide);
            if (exportMessage) exportMessage.textContent = `Capturando lámina ${slideNumber} de ${slides.length}`;
            // Garantiza que los mapas Leaflet de la lámina existan antes de capturarla.
            if (window.pamFichaMapas && slide.querySelector(".pam-mapa-gcr, .pam-mapa-red")) await window.pamFichaMapas();
            if (window.pamFichaTerritorial && slide.querySelector("[data-pam-territorial-analysis], [data-pam-territorial-comparison], [data-pam-territorial-element], [data-pam-territorial-matrix], [data-pam-territorial-executive]")) await window.pamFichaTerritorial();
            await asegurarImagenesPrecargadas(slide);
            await esperarRecursosVisuales(slide);
            const restaurarCanvas = omitirCanvasVacios();
            try {
                return await window.html2canvas(slide, {
                    backgroundColor: "#ffffff",
                    scale: 2,
                    useCORS: true,
                    allowTaint: false,
                    logging: false,
                    imageTimeout: 15000,
                    width: WIDTH,
                    height: HEIGHT,
                    windowWidth: WIDTH,
                    windowHeight: HEIGHT,
                    scrollX: 0,
                    scrollY: 0,
                    onclone: clonedDocument => {
                        // Compatibilidad con fichas que aún tengan en memoria la
                        // versión anterior de elementos decorativos por gradiente.
                        // La vista nueva usa trazos sólidos/SVG; en la copia se
                        // neutraliza cualquier patrón legado para que html2canvas no
                        // intente crear un patrón con dimensiones subpíxel.
                        clonedDocument.querySelectorAll(".pam-back__barcode").forEach(barcode => {
                            barcode.style.backgroundImage = "none";
                        });
                        clonedDocument.querySelectorAll(".pam-back__divider span, .pam-back__goldline").forEach(line => {
                            line.style.backgroundImage = "none";
                            line.style.backgroundColor = "#c9a24b";
                        });
                    }
                });
            } catch (error) {
                const etiqueta = slide.dataset.label || slide.dataset.screenLabel || String(slideNumber);
                const detalle = error instanceof Error ? error.message : String(error);
                throw new Error(`Lámina ${slideNumber} (${etiqueta}): ${detalle}`, { cause: error });
            } finally {
                restaurarCanvas();
            }
        }

        // Coloca links internos sobre los botones del índice para que el PDF sea navegable.
        function agregarLinksIndicePdf(pdf, slide) {
            if (!slide) return;
            const rectSlide = slide.getBoundingClientRect();
            if (!rectSlide.width || !rectSlide.height) return;
            const escalaX = PDF_WIDTH_MM / rectSlide.width;
            const escalaY = PDF_HEIGHT_MM / rectSlide.height;
            slide.querySelectorAll("[data-goto-label], [data-goto]").forEach(btn => {
                const targetLabel = normalizarLabel(btn.dataset.gotoLabel);
                const destino = btn.dataset.gotoLabel != null
                    ? slides.findIndex(s => normalizarLabel(s.dataset.screenLabel) === targetLabel)
                    : Number(btn.dataset.goto);
                if (!Number.isInteger(destino) || destino < 0 || destino >= slides.length) return;
                const r = btn.getBoundingClientRect();
                if (!r.width || !r.height) return;
                pdf.link(
                    (r.left - rectSlide.left) * escalaX,
                    (r.top - rectSlide.top) * escalaY,
                    r.width * escalaX,
                    r.height * escalaY,
                    { pageNumber: destino + 1 });
            });
        }

        // Coloca links externos de las figuras para abrir la imagen CDN al hacer clic dentro del PDF.
        function agregarLinksFiguraPdf(pdf, slide) {
            if (!slide) return;
            const rectSlide = slide.getBoundingClientRect();
            if (!rectSlide.width || !rectSlide.height) return;
            const escalaX = PDF_WIDTH_MM / rectSlide.width;
            const escalaY = PDF_HEIGHT_MM / rectSlide.height;
            const selector = page.classList.contains('convocatoria-ficha-page') ? 'a[href]' : '.pam-figura-media__link';
            slide.querySelectorAll(selector).forEach(link => {
                let href;
                try {
                    const rawHref = (link.getAttribute('href') || '').trim();
                    // Los controles de Leaflet con href="#" no son documentos.
                    if (!rawHref || rawHref.startsWith('#')) return;
                    const url = new URL(rawHref, location.href);
                    if (!['http:', 'https:'].includes(url.protocol)) return;
                    href = url.href;
                } catch { return; }
                const r = link.getBoundingClientRect();
                if (r.width > 0 && r.height > 0) {
                    pdf.link(
                        (r.left - rectSlide.left) * escalaX,
                        (r.top - rectSlide.top) * escalaY,
                        r.width * escalaX,
                        r.height * escalaY,
                        { url: href }
                    );
                }
            });
        }

        async function exportPdf() {
            const JsPdf = window.jspdf?.jsPDF || window.jsPDF;
            if (!window.html2canvas || !JsPdf) {
                notify("No se cargaron las herramientas de exportación. Recarga la página e inténtalo nuevamente.", "error");
                return;
            }

            const pdf = new JsPdf({
                orientation: "landscape",
                unit: "mm",
                format: [PDF_WIDTH_MM, PDF_HEIGHT_MM],
                compress: true
            });

            for (let index = 0; index < slides.length; index += 1) {
                slides.forEach((slide, slideIndex) => slide.classList.toggle("is-active", slideIndex === index));
                const canvas = await renderSlide(slides[index], index + 1);
                const imageData = canvas.toDataURL("image/jpeg", 0.96);
                if (index > 0) pdf.addPage([PDF_WIDTH_MM, PDF_HEIGHT_MM], "landscape");
                pdf.addImage(imageData, "JPEG", 0, 0, PDF_WIDTH_MM, PDF_HEIGHT_MM, undefined, "FAST");
                agregarLinksIndicePdf(pdf, slides[index]);
                agregarLinksFiguraPdf(pdf, slides[index]);
                canvas.width = 1;
                canvas.height = 1;
            }

            if (exportMessage) exportMessage.textContent = "Generando PDF";
            pdf.save(`${filename}.pdf`);
        }

        async function exportPptx() {
            const Pptx = window.PptxGenJS || window.pptxgen;
            if (!window.html2canvas || !Pptx) {
                notify("No se cargaron las herramientas de PowerPoint. Recarga la página e inténtalo nuevamente.", "error");
                return;
            }

            const pptx = new Pptx();
            pptx.layout = "LAYOUT_WIDE";
            pptx.author = "Secretaría de Energía - DGMESNIE";
            pptx.company = "Secretaría de Energía";
            pptx.subject = `Ficha ejecutiva ${page.dataset.projectKey || "PAMRNT"}`;
            pptx.title = `Ficha ejecutiva ${page.dataset.projectKey || "PAMRNT"}`;
            pptx.lang = "es-MX";
            pptx.theme = {
                headFontFace: "Patria",
                bodyFontFace: "Noto Sans",
                lang: "es-MX"
            };

            for (let index = 0; index < slides.length; index += 1) {
                slides.forEach((slide, slideIndex) => slide.classList.toggle("is-active", slideIndex === index));
                const canvas = await renderSlide(slides[index], index + 1);
                const imageData = canvas.toDataURL("image/jpeg", 0.93);
                const pptSlide = pptx.addSlide();
                pptSlide.background = { color: "FFFFFF" };
                pptSlide.addImage({ data: imageData, x: 0, y: 0, w: 13.333, h: 7.5 });
                pptSlide.addNotes(`Fuente: ${page.dataset.sourceLabel || "ficha ejecutiva PAM/PAMRNT"}. Lámina ${index + 1} de ${slides.length}.`);
                canvas.width = 1;
                canvas.height = 1;
            }

            if (exportMessage) exportMessage.textContent = "Generando PowerPoint";
            await pptx.writeFile({ fileName: `${filename}.pptx` });
        }

        async function exportDeck(format) {
            if (busy) return;
            const previous = slides[current];
            const previousTransform = deck.style.transform;
            setBusy(true, "Preparando las láminas");
            deck.style.transform = "none";

            try {
                if (document.fonts?.ready) await document.fonts.ready;
                if (format === "pdf") await exportPdf();
                else if (format === "pptx") await exportPptx();
            } catch (error) {
                console.error("Error al exportar la ficha PAM:", error);
                notify("La ficha no pudo generarse. Verifica la conexión y vuelve a intentarlo.", "error");
            } finally {
                current = Math.max(0, slides.indexOf(previous));
                slides.forEach((slide, slideIndex) => slide.classList.toggle("is-active", slideIndex === current));
                deck.style.transform = previousTransform;
                setCounter();
                setBusy(false);
                show(current, false);
                resizeDeck();
            }
        }

        // Genera el archivo (sin descargar) y devuelve { base64, nombre } para el envío por correo.
        async function generarBase64(format) {
            if (busy) throw new Error('Espera a que termine la exportación en curso.');
            const previous = slides[current];
            const previousTransform = deck.style.transform;
            setBusy(true, "Generando la ficha para enviar");
            deck.style.transform = "none";
            try {
                if (document.fonts?.ready) await document.fonts.ready;
                if (format === "pptx") {
                    const Pptx = window.PptxGenJS || window.pptxgen;
                    const pptx = new Pptx();
                    pptx.layout = "LAYOUT_WIDE";
                    pptx.author = "Secretaría de Energía - DGMESNIE";
                    pptx.title = `Ficha ejecutiva ${page.dataset.projectKey || "PAMRNT"}`;
                    for (let index = 0; index < slides.length; index += 1) {
                        slides.forEach((slide, si) => slide.classList.toggle("is-active", si === index));
                        const canvas = await renderSlide(slides[index], index + 1);
                        const pptSlide = pptx.addSlide();
                        pptSlide.background = { color: "FFFFFF" };
                        pptSlide.addImage({ data: canvas.toDataURL("image/jpeg", 0.93), x: 0, y: 0, w: 13.333, h: 7.5 });
                        canvas.width = 1; canvas.height = 1;
                    }
                    const base64 = await pptx.write("base64");
                    return { base64, nombre: `${filename}.pptx` };
                }
                const JsPdf = window.jspdf?.jsPDF || window.jsPDF;
                const pdf = new JsPdf({ orientation: "landscape", unit: "mm", format: [PDF_WIDTH_MM, PDF_HEIGHT_MM], compress: true });
                // Adjunto de correo: calidad JPEG 0.85 mantiene la legibilidad y deja fichas largas
                // (60+ láminas) por debajo del límite de 20 MB del servicio de correo.
                for (let index = 0; index < slides.length; index += 1) {
                    slides.forEach((slide, si) => slide.classList.toggle("is-active", si === index));
                    const canvas = await renderSlide(slides[index], index + 1);
                    if (index > 0) pdf.addPage([PDF_WIDTH_MM, PDF_HEIGHT_MM], "landscape");
                    pdf.addImage(canvas.toDataURL("image/jpeg", 0.85), "JPEG", 0, 0, PDF_WIDTH_MM, PDF_HEIGHT_MM, undefined, "FAST");
                    agregarLinksIndicePdf(pdf, slides[index]);
                    agregarLinksFiguraPdf(pdf, slides[index]);
                    canvas.width = 1; canvas.height = 1;
                }
                return { base64: pdf.output("datauristring"), nombre: `${filename}.pdf` };
            } finally {
                current = Math.max(0, slides.indexOf(previous));
                slides.forEach((slide, si) => slide.classList.toggle("is-active", si === current));
                deck.style.transform = previousTransform;
                setCounter();
                setBusy(false);
                show(current, false);
                resizeDeck();
            }
        }

        // Expuesto para el módulo de envío por correo.
        window.pamFichaGenerar = generarBase64;

        // Resuelve una lámina destino: por índice fijo (data-goto) o por su
        // etiqueta de pantalla (data-goto-label), robusto ante láminas ocultas.
        const normalizarLabel = l => (l || "").replace(/[·\s]/g, "-").toLowerCase();
        const indicePorLabel = label => {
            const target = normalizarLabel(label);
            const idx = slides.findIndex(slide => normalizarLabel(slide.dataset.screenLabel) === target);
            return idx >= 0 ? idx : current;
        };
        const destino = button => button.dataset.gotoLabel != null
            ? indicePorLabel(button.dataset.gotoLabel)
            : Number(button.dataset.goto);

        document.querySelectorAll("[data-prev]").forEach(button => button.addEventListener("click", () => show(current - 1)));
        sectionNav?.addEventListener('change', () => show(Number(sectionNav.value)));
        document.querySelectorAll("[data-next]").forEach(button => button.addEventListener("click", () => show(current + 1)));
        page.addEventListener('click', event => {
            const button = event.target.closest?.('[data-goto], [data-goto-label]');
            if (button && page.contains(button)) show(destino(button));
        });
        document.querySelectorAll("[data-fullscreen]").forEach(button => button.addEventListener("click", toggleFullscreen));
        document.querySelectorAll("[data-export]").forEach(button => button.addEventListener("click", () => exportDeck(button.dataset.export)));

        document.addEventListener("keydown", event => {
            if (busy || isEditableTarget(event.target)) return;
            if (["ArrowRight", "PageDown", " "].includes(event.key)) {
                event.preventDefault();
                show(current + 1);
            } else if (["ArrowLeft", "PageUp"].includes(event.key)) {
                event.preventDefault();
                show(current - 1);
            } else if (event.key === "Home") {
                event.preventDefault();
                show(0);
            } else if (event.key === "End") {
                event.preventDefault();
                show(slides.length - 1);
            } else if (event.key === "Escape" && document.fullscreenElement === shell) {
                document.exitFullscreen();
            }
        });

        window.addEventListener("resize", resizeDeck, { passive: true });
        document.addEventListener("fullscreenchange", () => {
            syncFullscreenControls();
            resizeDeck();
        });

        const hashMatch = window.location.hash.match(/lamina-(\d+)/i);
        show(hashMatch ? Number(hashMatch[1]) - 1 : 0, false);
        syncFullscreenControls();
        resizeDeck();

        if (window.actualizarPreloaderFicha) {
            window.actualizarPreloaderFicha(96, "Validando navegación, mapas y opciones de descarga.", "Preparación final");
        }
        requestAnimationFrame(() => requestAnimationFrame(() => {
            if (window.completarPreloaderFicha) window.completarPreloaderFicha();
        }));
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
