(function () {
    "use strict";

    // Mapas GeoJSON de la ficha (GCR en diagnóstico y red de transmisión regional).
    // Los contenedores viven en láminas display:none, así que la inicialización es
    // PEREZOSA: el mapa se crea la primera vez que su lámina se muestra (evento
    // pam:slide-shown) o cuando el export lo solicita vía window.pamFichaMapas().
    // Sin tiles base para que html2canvas los capture en el PDF.

    var GUINDA = "#9B2247";
    var GRIS_RELLENO = "#EDE8E2";
    var GRIS_LINEA = "#C9C4BC";
    var creados = { gcr: false, red: false };
    var cargasRed = []; // promesas de capas CDN, para que el export espere antes de capturar

    // Capas oficiales DGMESNIE (mismas del sistema de diseño SENER).
    var CDN_LT = "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_lt.geojson";
    var CDN_SE = "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson";
    var CDN_GERENCIAS = "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_gerencias.json";

    // Nombre de gerencia de un feature sin acoplarse al esquema: prueba claves
    // conocidas y, si no, cualquier propiedad de texto que mapee a una GCR válida.
    function nombreGerenciaDe(props) {
        if (!props) return "";
        var candidatas = ["region", "gerencia", "nombre", "name", "gcr", "nom_gcr", "GERENCIA", "REGION", "NOMBRE"];
        for (var i = 0; i < candidatas.length; i++)
            if (props[candidatas[i]]) return String(props[candidatas[i]]);
        for (var k in props) {
            var v = props[k];
            if (typeof v === "string" && GCR_ALIAS[normaliza(v)]) return v;
        }
        return "";
    }

    // Convención DOF de colores por nivel de tensión (diagramas oficiales):
    // 400 azul · 230 amarillo · 161-138 verde · 115-60 morado/magenta ·
    // 44-13.2 blanco (con contorno para ser visible) · <13.2 naranja.
    var KV_COLORS = [
        { min: 400, color: "#1F4FA3", label: "400 kV" },
        { min: 230, color: "#D9A800", label: "230 kV" },
        { min: 138, color: "#1E8449", label: "161–138 kV" },
        { min: 60, color: "#A23B9E", label: "115–60 kV" },
        { min: 13.2, color: "#FFFFFF", casing: "#9A958E", label: "44–13.2 kV" },
        { min: 0, color: "#E07B26", label: "< 13.2 kV" }
    ];

    function colorPorKv(kv) {
        for (var i = 0; i < KV_COLORS.length; i++) if (kv >= KV_COLORS[i].min) return KV_COLORS[i];
        return KV_COLORS[KV_COLORS.length - 1];
    }

    // Detecta el campo de tensión sin acoplarse al nombre exacto (tension/kv/volt...).
    function kvDe(props) {
        if (!props) return 0;
        for (var k in props) {
            if (/tens|volt|kv/i.test(k)) {
                var v = parseFloat(String(props[k]).replace(/[^\d.]/g, ""));
                if (!isNaN(v) && v > 0) return v;
            }
        }
        return 0;
    }

    function normaliza(txt) {
        return (txt || "")
            .toString()
            .toLowerCase()
            .normalize("NFD")
            .replace(/[̀-ͯ]/g, "")
            .replace(/regional|gerencia|de control|gcr/g, "")
            .replace(/[^a-z]/g, "")
            .trim();
    }

    // GRT de la BD (nombres y códigos legados) → id de gerencia en GCR_PATHS (10 GCR).
    var GCR_ALIAS = {
        bajacalifornia: "bcalifornia", bc: "bcalifornia",
        bajacaliforniasur: "bcsur", bs: "bcsur",
        mulege: "mulege",
        noreste: "noreste", ne: "noreste",
        noroeste: "noroeste", no: "noroeste",
        norte: "norte", nt: "norte",
        occidental: "occidental", oc: "occidental",
        oriental: "oriental", or: "oriental", se: "oriental",
        central: "central", ce: "central", vm: "central",
        peninsular: "peninsular", pe: "peninsular"
    };

    function idGerencia(txt) {
        var n = normaliza(txt);
        return GCR_ALIAS[n] || n;
    }

    function opcionesEstaticas() {
        return {
            zoomControl: false, attributionControl: false, dragging: false,
            scrollWheelZoom: false, doubleClickZoom: false, boxZoom: false,
            keyboard: false, touchZoom: false, preferCanvas: true
        };
    }

    // Lámina de diagnóstico: SVG con las 10 gerencias del sistema de diseño
    // (window.GCR_PATHS de gcr-data.js, viewBox 1000×626.3) — incluye Mulegé,
    // ausente en el GeoJSON local de 9. SVG puro = captura perfecta en el PDF.
    function crearMapaGcr() {
        var el = document.getElementById("pam-mapa-gcr");
        if (!el || creados.gcr) return;
        if (!el.clientWidth || !el.clientHeight) return; // lámina aún oculta
        if (typeof window.GCR_PATHS === "undefined") { crearMapaGcrLeaflet(el); return; }

        creados.gcr = true;
        var idActiva = idGerencia(el.dataset.gcr);
        var svg = ['<svg viewBox="0 0 1000 626.3" style="width:100%;height:100%;display:block" role="img" aria-label="Mapa de gerencias de control regional">'];
        Object.keys(window.GCR_PATHS).forEach(function (id) {
            var esActiva = id === idActiva;
            svg.push('<path d="' + window.GCR_PATHS[id] + '" fill="' + (esActiva ? GUINDA : GRIS_RELLENO) +
                '" stroke="' + (esActiva ? GUINDA : GRIS_LINEA) + '" stroke-width="' + (esActiva ? 1.6 : 0.8) +
                '" fill-opacity="' + (esActiva ? 0.9 : 0.6) + '"></path>');
        });
        svg.push("</svg>");
        el.innerHTML = svg.join("");

        // Datos de valor: generación/consumo (GeoJSON local) + adiciones MW (GCR_DATA) + proyectos.
        var datos = document.getElementById("pam-mapa-datos");
        if (!datos) return;
        var partes = [];
        var claveGeo = normaliza(el.dataset.gcr);
        if (typeof json_GerenciadeControlRegional_23 !== "undefined") {
            var f = json_GerenciadeControlRegional_23.features.find(function (x) { return normaliza(x.properties.region) === claveGeo; });
            if (f) {
                var gen = Number(f.properties.generacion || 0);
                var con = Number(f.properties.consumo || 0);
                if (gen > 0) partes.push("Generación " + gen.toLocaleString("es-MX") + " GWh");
                if (con > 0) partes.push("Consumo " + con.toLocaleString("es-MX") + " GWh");
            }
        }
        if (window.GCR_DATA && window.GCR_DATA.regiones && window.GCR_DATA.regiones[idActiva] && window.GCR_DATA.regiones[idActiva].mw > 0)
            partes.push("Adiciones 2026-2030 " + Number(window.GCR_DATA.regiones[idActiva].mw).toLocaleString("es-MX") + " MW");
        var proyectos = Number(el.dataset.proyectos || 0);
        if (proyectos > 0) partes.push(proyectos + " proyectos vigentes");
        datos.textContent = partes.join(" · ");
    }

    // Respaldo si gcr-data.js no está disponible: Leaflet con el GeoJSON local (9 GCR).
    function crearMapaGcrLeaflet(el) {
        if (typeof L === "undefined" || typeof json_GerenciadeControlRegional_23 === "undefined") return;
        creados.gcr = true;
        var idProyecto = idGerencia(el.dataset.gcr);
        if (idProyecto === "mulege") idProyecto = "bcsur";
        var map = L.map(el, opcionesEstaticas());
        var capa = L.geoJSON(json_GerenciadeControlRegional_23, {
            style: function (f) {
                var esActiva = idGerencia(f.properties.region) === idProyecto && idProyecto !== "";
                return {
                    color: esActiva ? GUINDA : GRIS_LINEA,
                    weight: esActiva ? 1.6 : 0.7,
                    fillColor: esActiva ? GUINDA : GRIS_RELLENO,
                    fillOpacity: esActiva ? 0.85 : 0.55
                };
            }
        }).addTo(map);
        try { map.fitBounds(capa.getBounds(), { padding: [8, 8] }); } catch (e) { }
    }

    function crearMapaRed() {
        var el = document.getElementById("pam-mapa-red");
        if (!el || creados.red || typeof L === "undefined") return;
        if (!el.clientWidth || !el.clientHeight) return; // lámina aún oculta

        creados.red = true;
        var map = L.map(el, opcionesEstaticas());
        map.setView([23.6, -102.5], 5); // vista nacional por defecto; fitBounds la afina al llegar gerencias

        // Gerencias: primero el GeoJSON oficial del CDN (10 GCR, incluye Mulegé);
        // si no responde, cae al local de 9 (con Mulegé aproximada a BCS).
        var pGerencias = fetch(CDN_GERENCIAS)
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })
            .then(function (geo) { return { geo: geo, esCdn: true }; })
            .catch(function (e) {
                console.warn("Gerencias CDN no disponible, uso capa local:", e);
                if (typeof json_GerenciadeControlRegional_23 === "undefined") throw e;
                return { geo: json_GerenciadeControlRegional_23, esCdn: false };
            });
        cargasRed.push(pGerencias.catch(function () { })); // sin rechazo suelto si tampoco hay capa local

        var pBounds = pGerencias.then(function (res) {
            var idProyecto = idGerencia(el.dataset.gcr);
            if (!res.esCdn && idProyecto === "mulege") idProyecto = "bcsur"; // el local de 9 no trae Mulegé

            var gcrActiva = null;
            var capaGcr = L.geoJSON(res.geo, {
                style: function (f) {
                    var esActiva = idGerencia(nombreGerenciaDe(f.properties)) === idProyecto && idProyecto !== "";
                    if (esActiva) gcrActiva = f;
                    return {
                        color: esActiva ? GUINDA : "#D8D2C8",
                        weight: esActiva ? 1.8 : 0.6,
                        fillColor: esActiva ? GUINDA : "#F2EEE9",
                        fillOpacity: esActiva ? 0.12 : 0.35
                    };
                }
            }).addTo(map);

            var bounds = gcrActiva ? L.geoJSON(gcrActiva).getBounds() : capaGcr.getBounds();
            try { map.fitBounds(bounds, { padding: [10, 10] }); } catch (e) { }

            var conteo = document.getElementById("pam-red-conteo");
            if (conteo && gcrActiva) {
                var gen = Number(gcrActiva.properties.generacion || 0);
                conteo.textContent = gen > 0 ? "Generación regional " + gen.toLocaleString("es-MX") + " GWh" : "";
            }

            // Bounds con margen para recortar capas a la región del proyecto.
            return bounds && bounds.isValid && bounds.isValid() ? bounds.pad(0.08) : null;
        });

        // Capas del CDN coloreadas por nivel de tensión; leyenda dinámica con los
        // niveles realmente presentes. Si el CDN no responde, el mapa queda con la GCR.
        var nivelesPresentes = {};

        function pintaLeyenda() {
            var cont = document.getElementById("pam-red-kv");
            if (!cont) return;
            cont.innerHTML = KV_COLORS
                .filter(function (n) { return nivelesPresentes[n.label]; })
                .map(function (n) {
                    var borde = n.casing ? ";border:1px solid " + n.casing : "";
                    return '<span class="pam-red__item"><i style="width:16px;height:3px;background:' + n.color + borde + '"></i>' + n.label + "</span>";
                })
                .join("");
        }

        // LT espera a las gerencias para conservar el orden de pintado
        // (polígonos abajo, líneas arriba) aunque el CDN responda antes.
        cargasRed.push(Promise.all([pBounds.catch(function () { return null; }), fetch(CDN_LT)
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })])
            .then(function (resultados) {
                var geo = resultados[1];
                // Contorno previo para las líneas blancas DOF (44–13.2 kV): sin él serían invisibles.
                L.geoJSON(geo, {
                    filter: function (f) { return !!colorPorKv(kvDe(f.properties)).casing; },
                    style: function (f) {
                        var nivel = colorPorKv(kvDe(f.properties));
                        return { color: nivel.casing, weight: 2.1, opacity: 0.75 };
                    }
                }).addTo(map);

                L.geoJSON(geo, {
                    style: function (f) {
                        var kv = kvDe(f.properties);
                        var nivel = colorPorKv(kv);
                        nivelesPresentes[nivel.label] = true;
                        return {
                            color: nivel.color,
                            weight: kv >= 400 ? 2.0 : kv >= 230 ? 1.5 : kv >= 138 ? 1.2 : 0.9,
                            opacity: kv >= 138 ? 0.92 : 0.7
                        };
                    }
                }).addTo(map);
                pintaLeyenda();
            })
            .catch(function (e) { console.warn("LT CDN no disponible:", e); }));

        cargasRed.push(Promise.all([pBounds.catch(function () { return null; }), fetch(CDN_SE)
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })])
            .then(function (resultados) {
                var boundsRegion = resultados[0];
                var geo = resultados[1];
                var subestacionesRegion = 0;
                L.geoJSON(geo, {
                    // Sólo las subestaciones dentro de la región del proyecto (con margen).
                    filter: function (f) {
                        if (!f.geometry || f.geometry.type !== "Point") return false;
                        if (!boundsRegion) return true;
                        var c = f.geometry.coordinates;
                        var dentro = boundsRegion.contains(L.latLng(c[1], c[0]));
                        if (dentro) subestacionesRegion += 1;
                        return dentro;
                    },
                    pointToLayer: function (f, latlng) {
                        var kv = kvDe(f.properties);
                        var nivel = colorPorKv(kv);
                        return L.circleMarker(latlng, {
                            radius: kv >= 400 ? 3.4 : 2.4,
                            color: "#fff", weight: 0.6,
                            fillColor: nivel.color, fillOpacity: 0.92
                        });
                    }
                }).addTo(map);

                var conteoEl = document.getElementById("pam-red-conteo");
                if (conteoEl && subestacionesRegion > 0) {
                    var previo = conteoEl.textContent ? conteoEl.textContent + " · " : "";
                    conteoEl.textContent = previo + subestacionesRegion.toLocaleString("es-MX") + " subestaciones en la región";
                }
            })
            .catch(function (e) { console.warn("Subestaciones CDN no disponible:", e); }));
    }

    function asegurarMapas() {
        crearMapaGcr();
        crearMapaRed();
    }

    // Init cuando la lámina se muestra (navegación) o al exportar (todas se activan).
    document.addEventListener("pam:slide-shown", function () {
        // Doble rAF: espera a que display:flex aplique y el contenedor tenga tamaño.
        requestAnimationFrame(function () { requestAnimationFrame(asegurarMapas); });
    });

    // Expuesto para que el export garantice mapas renderizados antes de capturar:
    // crea los mapas si faltan y espera a que las capas del CDN terminen de dibujarse.
    window.pamFichaMapas = function () {
        var yaExistian = creados.gcr && creados.red;
        asegurarMapas();
        return Promise.allSettled(cargasRed).then(function () {
            return new Promise(function (resolve) { setTimeout(resolve, yaExistian ? 60 : 350); });
        });
    };

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", asegurarMapas);
    else asegurarMapas();
})();
