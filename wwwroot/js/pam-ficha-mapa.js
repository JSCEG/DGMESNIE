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
    var proyectoTerritorialPorUrl = {};
    var gerenciasContextoPromise = null;

    function cargarProyectoTerritorial(url) {
        if (!url) return Promise.resolve(null);
        if (!proyectoTerritorialPorUrl[url]) {
            proyectoTerritorialPorUrl[url] = fetch(url, {
                credentials: "same-origin",
                cache: "no-store"
            }).then(function (r) {
                if (!r.ok) throw new Error("HTTP " + r.status);
                return r.json();
            });
        }
        return proyectoTerritorialPorUrl[url];
    }

    function cargarGerencias() {
        if (!gerenciasContextoPromise) {
            gerenciasContextoPromise = fetch(CDN_GERENCIAS)
                .then(function (r) {
                    if (!r.ok) throw new Error("HTTP " + r.status);
                    return r.json();
                })
                .then(function (geo) { return { geo: geo, esCdn: true }; })
                .catch(function (e) {
                    console.warn("Gerencias CDN no disponible, uso capa local:", e);
                    if (typeof json_GerenciadeControlRegional_23 === "undefined") throw e;
                    return { geo: json_GerenciadeControlRegional_23, esCdn: false };
                });
        }
        return gerenciasContextoPromise;
    }

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

    var GCR_CODIGO = {
        bcalifornia: "BC", bcsur: "BS", mulege: "MG",
        noreste: "NE", noroeste: "NO", norte: "NT",
        occidental: "OC", oriental: "OR", central: "CE", peninsular: "PE"
    };

    function codigoGerencia(txt) {
        var id = idGerencia(txt);
        return GCR_CODIGO[id] || (txt || "").toString().trim();
    }

    function puntoEnAnillo(punto, anillo) {
        if (!Array.isArray(anillo) || anillo.length < 3) return false;
        var x = punto[0];
        var y = punto[1];
        var dentro = false;
        for (var i = 0, j = anillo.length - 1; i < anillo.length; j = i++) {
            var xi = Number(anillo[i][0]);
            var yi = Number(anillo[i][1]);
            var xj = Number(anillo[j][0]);
            var yj = Number(anillo[j][1]);
            var cruza = ((yi > y) !== (yj > y)) &&
                (x < ((xj - xi) * (y - yi) / ((yj - yi) || Number.EPSILON)) + xi);
            if (cruza) dentro = !dentro;
        }
        return dentro;
    }

    function puntoEnPoligono(punto, poligono) {
        if (!Array.isArray(poligono) || !poligono.length || !puntoEnAnillo(punto, poligono[0])) return false;
        for (var i = 1; i < poligono.length; i++) {
            if (puntoEnAnillo(punto, poligono[i])) return false;
        }
        return true;
    }

    function puntoEnGeometria(punto, geometria) {
        if (!geometria || !Array.isArray(geometria.coordinates)) return false;
        if (geometria.type === "Polygon") return puntoEnPoligono(punto, geometria.coordinates);
        if (geometria.type === "MultiPolygon") {
            return geometria.coordinates.some(function (poligono) {
                return puntoEnPoligono(punto, poligono);
            });
        }
        return false;
    }

    function enriquecerGerenciasUbicaciones(ubicaciones, geoGerencias) {
        if (!Array.isArray(ubicaciones) || !geoGerencias || !Array.isArray(geoGerencias.features)) return ubicaciones || [];
        ubicaciones.forEach(function (ubicacion) {
            var lon = Number(ubicacion.longitud);
            var lat = Number(ubicacion.latitud);
            if (!Number.isFinite(lon) || !Number.isFinite(lat)) return;
            var feature = geoGerencias.features.find(function (gcr) {
                return puntoEnGeometria([lon, lat], gcr.geometry);
            });
            if (!feature) return;
            ubicacion.gcrCalculada = codigoGerencia(nombreGerenciaDe(feature.properties));
            ubicacion.gcrNombre = nombreGerenciaDe(feature.properties);
        });
        return ubicaciones;
    }

    function idsGerenciasUbicaciones(ubicaciones, respaldo) {
        var candidatas = (ubicaciones || []).filter(function (ubicacion) { return ubicacion.validada; });
        if (!candidatas.length) candidatas = ubicaciones || [];
        var ids = {};
        candidatas.forEach(function (ubicacion) {
            var id = idGerencia(ubicacion.gcrNombre || ubicacion.gcrCalculada);
            if (id) ids[id] = true;
        });
        // La GCR declarada en la cartera representa el alcance del proyecto y no
        // necesariamente contiene todos sus puntos. En proyectos multirregionales
        // se conserva junto con las GCR calculadas espacialmente para cada ubicación.
        if (respaldo) ids[idGerencia(respaldo)] = true;
        return ids;
    }

    function opcionesEstaticas() {
        return {
            zoomControl: false, attributionControl: false, dragging: false,
            scrollWheelZoom: false, doubleClickZoom: false, boxZoom: false,
            keyboard: false, touchZoom: false, preferCanvas: true
        };
    }

    // Lámina de diagnóstico: usa la ubicación validada del dashboard cuando existe.
    // La GCR permanece como respaldo para proyectos aún sin geometría validada.
    function crearMapaGcr() {
        var el = document.getElementById("pam-mapa-gcr");
        if (!el || creados.gcr) return;
        if (!el.clientWidth || !el.clientHeight) return; // lámina aún oculta
        creados.gcr = true;
        var carga = cargarProyectoTerritorial(el.dataset.territorialUrl)
            .then(function (proyecto) {
                var ubicacionesValidadas = proyecto && Array.isArray(proyecto.ubicaciones)
                    ? proyecto.ubicaciones.filter(function (ubicacion) {
                        return ubicacion.validada && (ubicacion.geometria || (
                            Number.isFinite(Number(ubicacion.latitud)) &&
                            Number.isFinite(Number(ubicacion.longitud))));
                    })
                    : [];

                if (ubicacionesValidadas.length && typeof L !== "undefined") {
                    return crearMapaUbicacionPrincipal(el, ubicacionesValidadas);
                }
                crearMapaGcrRegional(el);
                return null;
            })
            .catch(function (e) {
                console.warn("Ubicación territorial no disponible; se conserva la GCR:", e);
                crearMapaGcrRegional(el);
            });
        cargasRed.push(carga);
    }

    // Respaldo regional: SVG con las 10 gerencias del sistema de diseño
    // (window.GCR_PATHS de gcr-data.js, viewBox 1000×626.3) — incluye Mulegé.
    function crearMapaGcrRegional(el) {
        var caption = document.getElementById("pam-mapa-caption");
        var resumen = document.getElementById("pam-mapa-resumen");
        if (caption) caption.textContent = "Gerencia de Control Regional";
        if (resumen) resumen.textContent = el.dataset.gcr || "Región por definir";
        if (typeof window.GCR_PATHS === "undefined") { crearMapaGcrLeaflet(el); return; }

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

    function crearMapaUbicacionPrincipal(el, ubicaciones) {
        var caption = document.getElementById("pam-mapa-caption");
        var resumen = document.getElementById("pam-mapa-resumen");
        var datos = document.getElementById("pam-mapa-datos");
        if (caption) caption.textContent = "Ubicación territorial validada";
        if (resumen) resumen.textContent = ubicaciones.length.toLocaleString("es-MX") + (ubicaciones.length === 1 ? " ubicación" : " ubicaciones");
        if (datos) {
            datos.textContent = ubicaciones
                .slice(0, 3)
                .map(function (ubicacion) { return ubicacion.etiqueta || ubicacion.claveElementoRed || "Ubicación PAM"; })
                .join(" · ") + (ubicaciones.length > 3 ? " · +" + (ubicaciones.length - 3) : "");
        }

        var map = L.map(el, opcionesEstaticas());
        map.setView([23.6, -102.5], 5);

        var pContexto = cargarGerencias()
            .then(function (res) {
                var geo = res.geo;
                enriquecerGerenciasUbicaciones(ubicaciones, geo);
                var idsActivos = idsGerenciasUbicaciones(ubicaciones, el.dataset.gcr);
                var codigos = Object.keys(idsActivos).map(function (id) { return GCR_CODIGO[id] || id.toUpperCase(); });
                if (resumen) {
                    resumen.textContent = ubicaciones.length.toLocaleString("es-MX") +
                        (ubicaciones.length === 1 ? " ubicación" : " ubicaciones") +
                        (codigos.length ? " · " + codigos.join(" · ") : "");
                }
                if (datos) {
                    datos.textContent = ubicaciones
                        .slice(0, 3)
                        .map(function (ubicacion) {
                            var nombre = ubicacion.etiqueta || ubicacion.claveElementoRed || "Ubicación PAM";
                            return nombre + (ubicacion.gcrCalculada ? " (" + ubicacion.gcrCalculada + ")" : "");
                        })
                        .join(" · ") + (ubicaciones.length > 3 ? " · +" + (ubicaciones.length - 3) : "");
                }
                L.geoJSON(geo, {
                    style: function (f) {
                        var esActiva = !!idsActivos[idGerencia(nombreGerenciaDe(f.properties))];
                        return {
                            color: esActiva ? GUINDA : GRIS_LINEA,
                            weight: esActiva ? 1.3 : 0.65,
                            fillColor: esActiva ? GUINDA : GRIS_RELLENO,
                            fillOpacity: esActiva ? 0.1 : 0.36
                        };
                    }
                }).addTo(map);
            });

        var capaProyecto = L.featureGroup().addTo(map);
        ubicaciones.forEach(function (ubicacion, indice) {
            var geometria = ubicacion.geometria || {
                type: "Point",
                coordinates: [Number(ubicacion.longitud), Number(ubicacion.latitud)]
            };
            L.geoJSON({ type: "Feature", geometry: geometria, properties: ubicacion }, {
                style: function () {
                    return {
                        color: GUINDA,
                        weight: 4,
                        opacity: 0.96,
                        fillColor: GUINDA,
                        fillOpacity: 0.18
                    };
                },
                pointToLayer: function (feature, latlng) {
                    return L.circleMarker(latlng, {
                        radius: ubicacion.esPrincipal ? 8 : 6.5,
                        color: "#A57F2C",
                        weight: 3,
                        fillColor: GUINDA,
                        fillOpacity: 0.96
                    });
                },
                onEachFeature: function (feature, layer) {
                    var etiqueta = document.createElement("span");
                    etiqueta.textContent = (indice + 1) + ". " + (ubicacion.etiqueta || ubicacion.claveElementoRed || "Ubicación PAM");
                    layer.bindTooltip(etiqueta, {
                        permanent: ubicaciones.length <= 4,
                        direction: "top",
                        className: "pam-ubicacion-tooltip",
                        opacity: 0.95
                    });
                }
            }).addTo(capaProyecto);
        });

        var bounds = capaProyecto.getBounds();
        if (bounds && bounds.isValid()) {
            try {
                map.fitBounds(bounds.pad(ubicaciones.length === 1 ? 1.15 : 0.3), {
                    padding: [24, 24],
                    maxZoom: 8
                });
            } catch (e) { }
        }
        return pContexto.then(function () {
            capaProyecto.eachLayer(function (layer) {
                if (layer.bringToFront) layer.bringToFront();
            });
        });
    }

    function crearMapaRed() {
        var el = document.getElementById("pam-mapa-red");
        if (!el || creados.red || typeof L === "undefined") return;
        if (!el.clientWidth || !el.clientHeight) return; // lámina aún oculta

        creados.red = true;
        var map = L.map(el, opcionesEstaticas());
        map.setView([23.6, -102.5], 5); // vista nacional por defecto; fitBounds la afina al llegar gerencias
        var capas = {
            gcr: L.layerGroup().addTo(map),
            lineas: L.layerGroup().addTo(map),
            subestaciones: L.layerGroup().addTo(map),
            proyecto: L.featureGroup().addTo(map)
        };

        document.querySelectorAll("[data-pam-layer]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var capa = capas[boton.dataset.pamLayer];
                if (!capa) return;
                var activa = map.hasLayer(capa);
                if (activa) map.removeLayer(capa);
                else map.addLayer(capa);
                boton.classList.toggle("is-active", !activa);
                boton.setAttribute("aria-pressed", activa ? "false" : "true");
            });
        });

        var pProyecto = el.dataset.territorialUrl
            ? cargarProyectoTerritorial(el.dataset.territorialUrl)
            : Promise.resolve(null);

        // Gerencias: primero el GeoJSON oficial del CDN (10 GCR, incluye Mulegé);
        // si no responde, cae al local de 9. Para proyectos multirregionales se
        // resaltan la GCR declarada y todas las que contienen una ubicación validada.
        var pGerencias = cargarGerencias();
        cargasRed.push(pGerencias.catch(function () { })); // sin rechazo suelto si tampoco hay capa local

        var pBounds = Promise.all([pGerencias, pProyecto.catch(function () { return null; })]).then(function (resultados) {
            var res = resultados[0];
            var proyecto = resultados[1];
            var idProyecto = idGerencia(el.dataset.gcr);
            if (!res.esCdn && idProyecto === "mulege") idProyecto = "bcsur"; // el local de 9 no trae Mulegé
            var ubicaciones = proyecto && Array.isArray(proyecto.ubicaciones) ? proyecto.ubicaciones : [];
            enriquecerGerenciasUbicaciones(ubicaciones, res.geo);
            var idsActivos = idsGerenciasUbicaciones(ubicaciones, idProyecto);

            var gcrActivas = [];
            var capaGcr = L.geoJSON(res.geo, {
                style: function (f) {
                    var esActiva = !!idsActivos[idGerencia(nombreGerenciaDe(f.properties))];
                    if (esActiva) gcrActivas.push(f);
                    return {
                        color: esActiva ? GUINDA : "#D8D2C8",
                        weight: esActiva ? 1.8 : 0.6,
                        fillColor: esActiva ? GUINDA : "#F2EEE9",
                        fillOpacity: esActiva ? 0.12 : 0.35
                    };
                }
            }).addTo(capas.gcr);

            var bounds = gcrActivas.length
                ? L.geoJSON({ type: "FeatureCollection", features: gcrActivas }).getBounds()
                : capaGcr.getBounds();
            try { map.fitBounds(bounds, { padding: [10, 10] }); } catch (e) { }

            var conteo = document.getElementById("pam-red-conteo");
            var etiquetaGcr = document.getElementById("pam-red-gcr-label");
            if (conteo && gcrActivas.length) {
                var codigos = Object.keys(idsActivos).map(function (id) { return GCR_CODIGO[id] || id.toUpperCase(); });
                if (etiquetaGcr) etiquetaGcr.textContent = "GCR " + codigos.join(" · ");
                conteo.textContent = codigos.length > 1
                    ? codigos.length.toLocaleString("es-MX") + " GCR del proyecto · " + codigos.join(" · ")
                    : "GCR " + (codigos[0] || el.dataset.gcr || "por definir");
            }

            // Bounds con margen para recortar capas a la región del proyecto.
            return bounds && bounds.isValid && bounds.isValid() ? bounds.pad(0.08) : null;
        });

        function mostrarUbicaciones(proyecto) {
            var contenedor = document.getElementById("pam-red-ubicaciones");
            var meta = document.getElementById("pam-red-ubicacion-meta");
            if (!contenedor || !meta) return;

            var ubicaciones = proyecto && Array.isArray(proyecto.ubicaciones)
                ? proyecto.ubicaciones
                : [];
            var validadas = ubicaciones.filter(function (u) { return u.validada; }).length;
            meta.textContent = ubicaciones.length
                ? ubicaciones.length.toLocaleString("es-MX") + " · " + validadas.toLocaleString("es-MX") + " validadas"
                : "Sin geometría";
            contenedor.innerHTML = "";

            if (!ubicaciones.length) {
                var vacia = document.createElement("p");
                vacia.className = "pam-red__vacia";
                vacia.textContent = "La ficha conserva la cobertura GCR mientras se valida una ubicación puntual en el dashboard.";
                contenedor.appendChild(vacia);
                return;
            }

            ubicaciones.slice(0, 3).forEach(function (ubicacion, indice) {
                var tarjeta = document.createElement("article");
                tarjeta.className = "pam-red__ubicacion" + (ubicacion.validada ? "" : " is-sugerida");

                var numero = document.createElement("span");
                numero.className = "pam-red__ubicacion-numero";
                numero.textContent = String(indice + 1);

                var texto = document.createElement("div");
                var nombre = document.createElement("strong");
                nombre.textContent = ubicacion.etiqueta || ubicacion.claveElementoRed || "Ubicación territorial";
                var detalle = document.createElement("small");
                var partes = [
                    ubicacion.gcrCalculada ? "GCR " + ubicacion.gcrCalculada : "",
                    ubicacion.validada ? "Validada" : "Sugerida",
                    [ubicacion.municipio, ubicacion.entidad].filter(Boolean).join(", ")
                ].filter(Boolean);
                detalle.textContent = partes.join(" · ");
                texto.appendChild(nombre);
                texto.appendChild(detalle);
                tarjeta.appendChild(numero);
                tarjeta.appendChild(texto);
                contenedor.appendChild(tarjeta);
            });

            if (ubicaciones.length > 3) {
                var adicionales = document.createElement("p");
                adicionales.className = "pam-red__vacia";
                adicionales.textContent = "+ " + (ubicaciones.length - 3).toLocaleString("es-MX") + " ubicación(es) adicional(es) representada(s) en el mapa.";
                contenedor.appendChild(adicionales);
            }
        }

        function agregarGeometriasProyecto(proyecto) {
            var ubicaciones = proyecto && Array.isArray(proyecto.ubicaciones)
                ? proyecto.ubicaciones
                : [];
            ubicaciones.forEach(function (ubicacion, indice) {
                var geometria = ubicacion.geometria;
                if (!geometria && Number.isFinite(Number(ubicacion.latitud)) && Number.isFinite(Number(ubicacion.longitud))) {
                    geometria = {
                        type: "Point",
                        coordinates: [Number(ubicacion.longitud), Number(ubicacion.latitud)]
                    };
                }
                if (!geometria) return;

                var color = ubicacion.validada ? GUINDA : "#A57F2C";
                L.geoJSON({
                    type: "Feature",
                    geometry: geometria,
                    properties: ubicacion
                }, {
                    style: function () {
                        return {
                            color: color,
                            weight: ubicacion.validada ? 4 : 3,
                            opacity: 0.95,
                            dashArray: ubicacion.validada ? null : "6 4",
                            fillColor: color,
                            fillOpacity: ubicacion.validada ? 0.18 : 0.1
                        };
                    },
                    pointToLayer: function (feature, latlng) {
                        return L.circleMarker(latlng, {
                            radius: ubicacion.esPrincipal ? 8 : 6.5,
                            color: ubicacion.validada ? "#A57F2C" : color,
                            weight: ubicacion.validada ? 3 : 2,
                            dashArray: ubicacion.validada ? null : "3 3",
                            fillColor: color,
                            fillOpacity: ubicacion.validada ? 0.95 : 0.45
                        });
                    },
                    onEachFeature: function (feature, layer) {
                        var etiqueta = document.createElement("span");
                        etiqueta.textContent = (indice + 1) + ". " + (ubicacion.etiqueta || ubicacion.claveElementoRed || "Ubicación PAM");
                        layer.bindTooltip(etiqueta, {
                            permanent: ubicaciones.length <= 6,
                            direction: "top",
                            className: "pam-red__tooltip",
                            opacity: 0.94
                        });
                    }
                }).addTo(capas.proyecto);
            });

            if (!capas.proyecto.getLayers().length) return null;
            var bounds = capas.proyecto.getBounds();
            if (!bounds || !bounds.isValid()) return null;
            try {
                map.fitBounds(bounds.pad(ubicaciones.length === 1 ? 1.1 : 0.28), {
                    padding: [34, 34],
                    maxZoom: 9
                });
            } catch (e) { }
            return bounds.pad(0.35);
        }

        var pUbicaciones = Promise.all([
            pBounds.catch(function () { return null; }),
            pProyecto.catch(function () { return null; })
        ])
            .then(function (resultados) {
                var proyecto = resultados[1];
                mostrarUbicaciones(proyecto);
                return agregarGeometriasProyecto(proyecto) || resultados[0];
            })
            .catch(function (e) {
                console.warn("Ubicación territorial PAM no disponible:", e);
                mostrarUbicaciones(null);
                return null;
            });
        cargasRed.push(pUbicaciones);

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
                }).addTo(capas.lineas);

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
                }).addTo(capas.lineas);
                pintaLeyenda();
            })
            .catch(function (e) { console.warn("LT CDN no disponible:", e); }));

        cargasRed.push(Promise.all([pUbicaciones.catch(function () { return null; }), fetch(CDN_SE)
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })])
            .then(function (resultados) {
                var boundsRegion = resultados[0];
                var geo = resultados[1];
                var subestacionesRegion = 0;
                L.geoJSON(geo, {
                    // Sólo las subestaciones dentro del ámbito de las ubicaciones del proyecto (con margen).
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
                }).addTo(capas.subestaciones);

                var conteoEl = document.getElementById("pam-red-conteo");
                if (conteoEl && subestacionesRegion > 0) {
                    var previo = conteoEl.textContent ? conteoEl.textContent + " · " : "";
                    conteoEl.textContent = previo + subestacionesRegion.toLocaleString("es-MX") + " subestaciones en el ámbito";
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
