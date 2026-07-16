(function () {
    "use strict";

    const WIDTH = 1333;
    const HEIGHT = 750;
    const PDF_WIDTH_MM = 338.6582;
    const PDF_HEIGHT_MM = 190.5;

    function init() {
        const page = document.querySelector(".pam-ficha-page");
        const shell = document.getElementById("pam-deck-shell");
        const deck = document.getElementById("pam-deck");
        const slides = Array.from(document.querySelectorAll("[data-slide]"));
        const counter = document.querySelector("[data-counter]");
        const exportStatus = document.getElementById("pam-export-status");
        const exportMessage = exportStatus?.querySelector("[data-export-message]");

        if (!page || !shell || !deck || slides.length === 0) return;

        let current = 0;
        let busy = false;

        const pad = value => String(value).padStart(2, "0");
        const filename = page.dataset.filename || "Ficha_PAMRNT";

        function setCounter() {
            if (counter) counter.textContent = `${pad(current + 1)} / ${pad(slides.length)}`;
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
        }

        function resizeDeck() {
            const fullscreen = document.fullscreenElement === shell;
            const availableWidth = fullscreen ? window.innerWidth : Math.max(320, page.clientWidth - 44);
            const top = shell.getBoundingClientRect().top;
            const availableHeight = fullscreen ? window.innerHeight : Math.max(280, window.innerHeight - top - 24);
            const scale = Math.min(1, availableWidth / WIDTH, availableHeight / HEIGHT);

            deck.style.transform = `scale(${scale})`;
            if (fullscreen) {
                shell.style.width = "100vw";
                shell.style.height = "100vh";
            } else {
                shell.style.width = `${Math.round(WIDTH * scale)}px`;
                shell.style.height = `${Math.round(HEIGHT * scale)}px`;
            }
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

        async function renderSlide(slide, slideNumber) {
            if (exportMessage) exportMessage.textContent = `Capturando lámina ${slideNumber} de ${slides.length}`;
            await nextPaint();
            return window.html2canvas(slide, {
                backgroundColor: "#ffffff",
                scale: 1.2,
                useCORS: true,
                allowTaint: false,
                logging: false,
                width: WIDTH,
                height: HEIGHT,
                windowWidth: WIDTH,
                windowHeight: HEIGHT,
                scrollX: 0,
                scrollY: 0
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
                const imageData = canvas.toDataURL("image/jpeg", 0.93);
                if (index > 0) pdf.addPage([PDF_WIDTH_MM, PDF_HEIGHT_MM], "landscape");
                pdf.addImage(imageData, "JPEG", 0, 0, PDF_WIDTH_MM, PDF_HEIGHT_MM, undefined, "FAST");
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
                pptSlide.addNotes(`Fuente: ficha ejecutiva PAM/PAMRNT. Lámina ${index + 1} de ${slides.length}.`);
                canvas.width = 1;
                canvas.height = 1;
            }

            if (exportMessage) exportMessage.textContent = "Generando PowerPoint";
            await pptx.writeFile({ fileName: `${filename}.pptx` });
        }

        async function exportDeck(format) {
            if (busy) return;
            const previous = current;
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
                current = previous;
                slides.forEach((slide, slideIndex) => slide.classList.toggle("is-active", slideIndex === current));
                deck.style.transform = previousTransform;
                setCounter();
                setBusy(false);
                resizeDeck();
            }
        }

        // Resuelve una lámina destino: por índice fijo (data-goto) o por su
        // etiqueta de pantalla (data-goto-label), robusto ante láminas ocultas.
        const indicePorLabel = label => {
            const idx = slides.findIndex(slide => slide.dataset.screenLabel === label);
            return idx >= 0 ? idx : 0;
        };
        const destino = button => button.dataset.gotoLabel != null
            ? indicePorLabel(button.dataset.gotoLabel)
            : Number(button.dataset.goto);

        document.querySelectorAll("[data-prev]").forEach(button => button.addEventListener("click", () => show(current - 1)));
        document.querySelectorAll("[data-next]").forEach(button => button.addEventListener("click", () => show(current + 1)));
        document.querySelectorAll("[data-goto], [data-goto-label]").forEach(button => button.addEventListener("click", () => show(destino(button))));
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
        document.addEventListener("fullscreenchange", resizeDeck);

        const hashMatch = window.location.hash.match(/lamina-(\d{1,2})/i);
        show(hashMatch ? Number(hashMatch[1]) - 1 : 0, false);
        resizeDeck();
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
