(function () {
    "use strict";

    // Mapas de la ficha ejecutiva de la cartera (SVG inline, capturable por html2canvas):
    //  - [data-mapa-sistemas]: estados coloreados por sistema eléctrico (SIN, SIBC, SIBCS) con color propio.
    //  - [data-mapa-entidades]: coropleta de capacidad considerada por entidad federativa.
    // La capa de estados es la del Geovisualizador (var estados, propiedad NOMGEO).

    var GUINDA = [155, 34, 71];
    var CREMA = [238, 229, 220];
    var ANCLAS = { SIN: [-101.8, 23.8], SIBC: [-115.3, 30.7], SIBCS: [-111.2, 25.4] };

    function mezcla(t) {
        var c = CREMA.map(function (v, i) { return Math.round(v + (GUINDA[i] - v) * t); });
        return "rgb(" + c.join(",") + ")";
    }

    function esc(s) {
        return String(s == null ? "" : s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/"/g, "&quot;");
    }

    function n0(v) {
        try { return Number(v || 0).toLocaleString("es-MX", { maximumFractionDigits: 0 }); } catch (e) { return String(Math.round(v || 0)); }
    }

    // Clave comparable de una entidad: sin acentos, minúsculas y sin el sufijo "de …" (Coahuila de Zaragoza → coahuila).
    function clave(nombre) {
        var n = String(nombre || "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase().trim();
        if (n.indexOf("ciudad de mexico") >= 0 || n.indexOf("distrito federal") >= 0) return "cdmx";
        if (n === "mexico" || n === "estado de mexico") return "edomex";
        return n.split(" de ")[0].trim();
    }

    function sistemaDe(nombre) {
        var k = clave(nombre);
        if (k === "baja california") return "SIBC";
        if (k === "baja california sur") return "SIBCS";
        return "SIN";
    }

    function anillos(g) {
        if (!g) return [];
        if (g.type === "Polygon") return g.coordinates;
        if (g.type === "MultiPolygon") return g.coordinates.reduce(function (a, p) { return a.concat(p); }, []);
        return [];
    }

    function nombreDe(f) {
        var p = f.properties || {};
        return p.NOMGEO || p.NOM_ENT || p.nombre || p.name || "";
    }

    function cargar(cb) {
        if (window.estados && window.estados.features) return cb(window.estados);
        var s = document.createElement("script");
        s.src = "/Geovisualizador/shapes/estadosminlight.js";
        s.onload = function () { cb(window.estados); };
        s.onerror = function () { cb(null); };
        document.head.appendChild(s);
    }

    // Proyección plana (equirectangular corregida) ajustada al lienzo.
    function proyector(geo, W, H, pad) {
        var cos = Math.cos(24 * Math.PI / 180);
        var minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
        function plano(c) { return [c[0] * cos, -c[1]]; }
        geo.features.forEach(function (f) {
            anillos(f.geometry).forEach(function (ring) {
                ring.forEach(function (c) {
                    var p = plano(c);
                    if (p[0] < minX) minX = p[0]; if (p[0] > maxX) maxX = p[0];
                    if (p[1] < minY) minY = p[1]; if (p[1] > maxY) maxY = p[1];
                });
            });
        });
        var k = Math.min((W - 2 * pad) / (maxX - minX), (H - 2 * pad) / (maxY - minY));
        var ox = pad + ((W - 2 * pad) - (maxX - minX) * k) / 2;
        var oy = pad + ((H - 2 * pad) - (maxY - minY) * k) / 2;
        return function (c) { var p = plano(c); return [((p[0] - minX) * k + ox).toFixed(1), ((p[1] - minY) * k + oy).toFixed(1)]; };
    }

    function trazo(f, tx) {
        return anillos(f.geometry).map(function (ring) {
            return "M" + ring.map(function (c) { return tx(c).join(","); }).join("L") + "Z";
        }).join("");
    }

    function dibujarSistemas(host, geo, sistemas) {
        var W = 640, H = 360, tx = proyector(geo, W, H, 6);
        var porCodigo = {};
        sistemas.forEach(function (s) { porCodigo[s.code] = s; });
        var paths = geo.features.map(function (f) {
            var nombre = nombreDe(f), code = sistemaDe(nombre), s = porCodigo[code] || {};
            return '<path d="' + trazo(f, tx) + '" fill="' + esc(s.color || mezcla(0.5)) + '" stroke="#fff" stroke-width="0.8"><title>' + esc(nombre) + " · " + code + "</title></path>";
        });
        var etiquetas = sistemas.filter(function (s) { return ANCLAS[s.code]; }).map(function (s) {
            var p = tx(ANCLAS[s.code]);
            return '<text class="rs-mapa__code" x="' + p[0] + '" y="' + p[1] + '" text-anchor="middle">' + esc(s.code) + "</text>";
        });
        var leyenda = sistemas.filter(function (s) { return ANCLAS[s.code]; }).map(function (s) {
            return '<span><i style="background:' + esc(s.color || "") + '"></i><b>' + esc(s.code) + "</b> " + esc(s.nombre || "") + (s.lineas && s.lineas.length ? " · " + esc(s.lineas.join(" · ")) : "") + "</span>";
        }).join("");
        host.innerHTML = '<svg viewBox="0 0 ' + W + " " + H + '" role="img" aria-label="Sistemas eléctricos y requerimiento de capacidad">' +
            paths.join("") + etiquetas.join("") + "</svg>" + '<div class="rs-mapa__legend rs-mapa__legend--rows">' + leyenda + "</div>";
    }

    function dibujarEntidades(host, geo, entidades) {
        var W = 640, H = 360, tx = proyector(geo, W, H, 6);
        var porClave = {};
        entidades.forEach(function (e) { porClave[clave(e.nombre)] = e; });
        var max = Math.max.apply(null, entidades.map(function (e) { return Number(e.mw) || 0; })) || 1;
        function tono(mw) { return mw > 0 ? 0.28 + 0.72 * (mw / max) : 0.06; }
        var paths = geo.features.map(function (f) {
            var nombre = nombreDe(f), e = porClave[clave(nombre)], mw = e ? Number(e.mw) || 0 : 0;
            var titulo = esc(nombre) + (e ? " · " + n0(mw) + " MW · " + (e.proyectos || 0) + " proy." : " · sin proyectos considerados");
            return '<path d="' + trazo(f, tx) + '" fill="' + mezcla(tono(mw)) + '" stroke="#fff" stroke-width="0.8"><title>' + titulo + "</title></path>";
        });
        var cortes = [max / 3, 2 * max / 3, max];
        var leyenda = '<span><i style="background:' + mezcla(0.06) + '"></i>Sin proyectos</span>' + cortes.map(function (c, i) {
            var desde = i === 0 ? 1 : cortes[i - 1];
            return '<span><i style="background:' + mezcla(tono((desde + c) / 2)) + '"></i>' + (i === 0 ? "Hasta " : n0(desde) + " a ") + n0(c) + " MW</span>";
        }).join("");
        host.innerHTML = '<svg viewBox="0 0 ' + W + " " + H + '" role="img" aria-label="Capacidad considerada por entidad federativa">' + paths.join("") + "</svg>" +
            '<div class="rs-legend rs-mapa__legend">' + leyenda + "</div>";
    }

    function init() {
        var hostS = document.querySelector("[data-mapa-sistemas]");
        var hostE = document.querySelector("[data-mapa-entidades]");
        if (!hostS && !hostE) return;
        cargar(function (geo) {
            if (!geo || !geo.features) {
                [hostS, hostE].forEach(function (h) { if (h) h.innerHTML = '<p class="conv-note">Mapa no disponible.</p>'; });
                return;
            }
            try {
                if (hostS) dibujarSistemas(hostS, geo, JSON.parse(hostS.getAttribute("data-mapa-sistemas") || "[]"));
                if (hostE) dibujarEntidades(hostE, geo, JSON.parse(hostE.getAttribute("data-mapa-entidades") || "[]"));
            } catch (e) { /* datos inválidos: se deja el marcador de carga */ }
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init); else init();
})();
