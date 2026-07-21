// Utilidades compartidas para modo presentación en Gestor.

export function setPresentationControls(visible, controlsId = 'presentationControls') {
    const controls = document.getElementById(controlsId);
    if (!controls) return;
    controls.style.display = visible ? 'flex' : 'none';
    controls.setAttribute('aria-hidden', visible ? 'false' : 'true');
}

export function fitSlideToViewport(slide, zoomFactor = 1, afterFit) {
    if (!slide) return;
    const baseW = slide.offsetWidth || 1280;
    const baseH = slide.offsetHeight || 720;
    const baseScale = Math.min(window.innerWidth / baseW, window.innerHeight / baseH);
    const scale = Math.max(0.2, baseScale * zoomFactor);
    slide.style.setProperty('--presentation-scale', scale.toFixed(4));
    if (typeof afterFit === 'function') afterFit(slide);
}

export function renderPresentationSlides(slides, activeIndex, options = {}) {
    if (!slides?.length) return;
    const { counterId = 'presentationCounter', zoomFactor = 1, afterFit } = options;
    slides.forEach((slide, idx) => {
        slide.classList.toggle('active', idx === activeIndex);
        if (idx !== activeIndex) slide.style.removeProperty('--presentation-scale');
    });
    const counter = document.getElementById(counterId);
    if (counter) counter.textContent = `${activeIndex + 1}/${slides.length}`;
    fitSlideToViewport(slides[activeIndex], zoomFactor, afterFit);
}

export function cleanupPresentationSlides(slides) {
    if (!slides?.length) return;
    slides.forEach(slide => {
        slide.classList.remove('active');
        slide.style.removeProperty('--presentation-scale');
    });
}

export async function enterFullscreen() {
    if (!document.fullscreenElement && document.documentElement.requestFullscreen) {
        try { await document.documentElement.requestFullscreen(); } catch (_) { }
    }
}

export async function exitFullscreen() {
    if (document.fullscreenElement && document.exitFullscreen) {
        try { await document.exitFullscreen(); } catch (_) { }
    }
}

export function isForwardPresentationKey(key) {
    return ['ArrowRight', 'PageDown', ' '].includes(key);
}

export function isBackwardPresentationKey(key, includeBackspace = true) {
    const keys = includeBackspace ? ['ArrowLeft', 'PageUp', 'Backspace'] : ['ArrowLeft', 'PageUp'];
    return keys.includes(key);
}
