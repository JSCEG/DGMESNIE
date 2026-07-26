(function () {
    "use strict";

    function init() {
        const container = document.getElementById("permiso-ficha-map");
        const config = window.permisoFichaMap || {};
        if (!container || !window.L || !Number.isFinite(config.latitud) || !Number.isFinite(config.longitud)) {
            if (container) {
                container.innerHTML = '<div style="height:100%;display:grid;place-items:center;color:#756d67;font:700 15px Noto Sans,sans-serif">El inventario no contiene coordenadas válidas para este permiso.</div>';
            }
            return;
        }

        const map = L.map(container, {
            zoomControl: true,
            attributionControl: true,
            preferCanvas: true,
            scrollWheelZoom: false
        }).setView([config.latitud, config.longitud], 12);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            crossOrigin: "anonymous",
            attribution: "&copy; OpenStreetMap"
        }).addTo(map);

        const icon = L.divIcon({
            className: "",
            html: '<div class="permiso-location__marker" aria-hidden="true"></div>',
            iconSize: [44, 44],
            iconAnchor: [18, 40],
            popupAnchor: [2, -36]
        });
        L.marker([config.latitud, config.longitud], { icon, riseOnHover: true })
            .addTo(map)
            .bindPopup(
                '<strong style="color:#9B2247">' + escapeHtml(config.permiso) + "</strong><br>" +
                '<span style="font-size:12px">' + escapeHtml(config.titulo) + "</span>"
            );

        const refresh = event => {
            const shown = event?.detail?.index;
            if (shown == null || container.closest("[data-slide]")?.classList.contains("is-active")) {
                window.setTimeout(() => map.invalidateSize(false), 80);
            }
        };
        document.addEventListener("pam:slide-shown", refresh);
        window.setTimeout(() => map.invalidateSize(false), 150);
    }

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
