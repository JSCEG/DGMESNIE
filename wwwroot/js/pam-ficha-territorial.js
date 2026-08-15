(function () {
    "use strict";

    var GUINDA = "#9B2247";
    var DORADO = "#E0A12E";
    var VERDE = "#0E8A6E";
    var CIAN = "#1E9CB8";
    var URLS = {
        gerencias: "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_gerencias.json",
        lineas: "/DashboardProyectos/RedElectrica/Grafo/GeoJson/Aristas",
        subestacionesTransmision: "/DashboardProyectos/RedElectrica/InventarioSubestaciones/GeoJson?networkLevel=transmision",
        subestacionesDistribucion: "/DashboardProyectos/RedElectrica/InventarioSubestaciones/GeoJson?networkLevel=subtransmision%2Cdistribucion%2Cindeterminado",
        municipios: "https://cdn.sassoapps.com/Mapas/Electricidad/municipios.geojson",
        anp: "https://cdn.sassoapps.com/Mapas/ANP2025.geojson",
        anpEstatal: "https://cdn.sassoapps.com/Gabvy/anp_estatal.geojson",
        ramsar: "https://cdn.sassoapps.com/Gabvy/ramsar.geojson",
        regionesIndigenas: "https://cdn.sassoapps.com/Gabvy/regionesindigenas.geojson",
        ductosImportacion: "https://cdn.sassoapps.com/Mapas/Electricidad/Ductos%20de%20Importacion.geojson",
        ductosSistrangas: "https://cdn.sassoapps.com/Mapas/Electricidad/Ductos%20integrados%20a%20SISTRANGAS.geojson",
        demanda: "/DashboardProyectos/AtlasSen/Demanda/GeoJson",
        tarifas: "/DashboardProyectos/AtlasSen/Divisiones/GeoJson",
        centrales: "https://cdn.sassoapps.com/geojson/Centrales_El%C3%A9ctricas_privadas_y_de_CFE.geojson",
        presas: "https://cdn.sassoapps.com/Mapas/Electricidad/presas.geojson",
        generacionPrivada: "/DashboardProyectos/AtlasSen/GeneracionPrivada/GeoJson",
        conservacion: "https://cdn.sassoapps.com/Mapas/areas_destinadas_voluntariamentea_la_conservaci%C3%B3n.geojson",
        sitiosArqueologicos: "https://cdn.sassoapps.com/Gabvy/sitio_arqueologico.geojson",
        zonasArqueologicas: "https://cdn.sassoapps.com/Gabvy/ZA_publico.geojson",
        zonasHistoricas: "https://cdn.sassoapps.com/Gabvy/z_historicos.geojson"
    };

    var cargas = {};
    var analisisPorUrl = {};
    var inegiPorClave = {};
    var mapa = null;
    var mapaListo = false;
    var mapasElemento = {};
    var bboxCache = typeof WeakMap !== "undefined" ? new WeakMap() : null;

    var GCR_ALIAS = {
        bajacalifornia: "BC", bc: "BC",
        bajacaliforniasur: "BS", bs: "BS",
        mulege: "MG", mg: "MG",
        noreste: "NE", ne: "NE",
        noroeste: "NO", no: "NO",
        norte: "NT", nt: "NT",
        occidental: "OC", oc: "OC",
        oriental: "OR", or: "OR", se: "OR",
        central: "CE", ce: "CE", vm: "CE",
        peninsular: "PE", pe: "PE"
    };

    var ESTADOS = {
        aguascalientes: "01", bajacalifornia: "02", bajacaliforniasur: "03", campeche: "04",
        coahuila: "05", colima: "06", chiapas: "07", chihuahua: "08", ciudadmexico: "09",
        durango: "10", guanajuato: "11", guerrero: "12", hidalgo: "13", jalisco: "14",
        mexico: "15", michoacan: "16", morelos: "17", nayarit: "18", nuevoleon: "19",
        oaxaca: "20", puebla: "21", queretaro: "22", quintanaroo: "23", sanluispotosi: "24",
        sinaloa: "25", sonora: "26", tabasco: "27", tamaulipas: "28", tlaxcala: "29",
        veracruz: "30", yucatan: "31", zacatecas: "32"
    };

    function normaliza(valor) {
        return String(valor || "").toLowerCase().normalize("NFD")
            .replace(/[̀-ͯ]/g, "")
            .replace(/gerencia|regional|de control|gcr/g, "")
            .replace(/[^a-z0-9]/g, "");
    }

    function textoSeguro(valor) {
        return String(valor == null ? "" : valor)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;")
            .replace(/>/g, "&gt;").replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function numero(valor, decimales) {
        var n = Number(valor);
        if (!Number.isFinite(n)) return "—";
        return n.toLocaleString("es-MX", {
            minimumFractionDigits: decimales || 0,
            maximumFractionDigits: decimales || 0
        });
    }

    function fetchJson(url) {
        if (!url) return Promise.reject(new Error("URL territorial vacía"));
        if (!cargas[url]) {
            cargas[url] = fetch(url, { credentials: url.charAt(0) === "/" ? "same-origin" : "omit", cache: "force-cache" })
                .then(function (respuesta) {
                    if (!respuesta.ok) throw new Error("HTTP " + respuesta.status);
                    return respuesta.json();
                });
        }
        return cargas[url];
    }

    function obtenerPropiedad(propiedades, expresion) {
        if (!propiedades) return "";
        var claves = Object.keys(propiedades);
        for (var i = 0; i < claves.length; i++) {
            if (expresion.test(claves[i]) && propiedades[claves[i]] != null) return propiedades[claves[i]];
        }
        return "";
    }

    function nombreFeature(feature, respaldo) {
        var p = feature && feature.properties || {};
        return String(
            obtenerPropiedad(p, /^(nombre|name|nomgeo|nom_gcr|gerencia|region|denominacion|subestacion|se)$/i) ||
            obtenerPropiedad(p, /nombre|denomin|subest|gerencia|region/i) || respaldo || "Elemento territorial"
        ).trim();
    }

    function codigoGcr(nombre) {
        return GCR_ALIAS[normaliza(nombre)] || "";
    }

    function codigoGcrFeature(feature) {
        var nombre = nombreFeature(feature, "");
        var codigo = codigoGcr(nombre);
        if (codigo) return codigo;
        var p = feature && feature.properties || {};
        var valores = Object.keys(p).map(function (k) { return p[k]; });
        for (var i = 0; i < valores.length; i++) {
            codigo = codigoGcr(valores[i]);
            if (codigo) return codigo;
        }
        return "";
    }

    function featureUbicacion(ubicacion) {
        var geometria = ubicacion && ubicacion.geometria;
        if (!geometria && Number.isFinite(Number(ubicacion && ubicacion.longitud)) && Number.isFinite(Number(ubicacion && ubicacion.latitud))) {
            geometria = { type: "Point", coordinates: [Number(ubicacion.longitud), Number(ubicacion.latitud)] };
        }
        return geometria ? { type: "Feature", properties: ubicacion || {}, geometry: geometria } : null;
    }

    function centroFeature(feature) {
        if (!feature || !feature.geometry || !window.turf) return null;
        try {
            if (feature.geometry.type === "Point") return window.turf.point(feature.geometry.coordinates);
            return window.turf.centroid(feature);
        } catch (e) {
            return null;
        }
    }

    function radioUbicacion(ubicacion, predeterminado) {
        var precision = normaliza([ubicacion.precisionUbicacion, ubicacion.metodoUbicacion].filter(Boolean).join(" "));
        var aproximada = /municip|regional|localidad|referencia|aproxim|influencia|geocodific/.test(precision);
        var sugerido = Number(ubicacion.radioSugeridoKm);
        if (aproximada && Number.isFinite(sugerido) && sugerido > 0) return Math.min(100, Math.max(1, sugerido));
        return Math.min(100, Math.max(1, Number(predeterminado) || 20));
    }

    function coberturaFeature(feature, radioKm) {
        if (!feature || !window.turf) return null;
        try {
            if (feature.geometry.type === "Point") return window.turf.circle(feature.geometry.coordinates, radioKm, { steps: 64, units: "kilometers" });
            return window.turf.buffer(feature, radioKm, { steps: 24, units: "kilometers" });
        } catch (e) {
            var centro = centroFeature(feature);
            return centro ? window.turf.circle(centro.geometry.coordinates, radioKm, { steps: 64, units: "kilometers" }) : null;
        }
    }

    function intersecta(cobertura, feature) {
        if (!cobertura || !feature || !feature.geometry || !window.turf) return false;
        try { return window.turf.booleanIntersects(cobertura, feature); }
        catch (e) {
            try {
                var centro = centroFeature(feature);
                return centro ? window.turf.booleanPointInPolygon(centro, cobertura) : false;
            } catch (ignorado) { return false; }
        }
    }

    function featuresGeo(geo) {
        return geo && Array.isArray(geo.features) ? geo.features : [];
    }

    function claveFeature(feature, indice, prefijo) {
        var p = feature && feature.properties || {};
        var id = feature && feature.id || obtenerPropiedad(p, /^(id|clave|cvegeo|codigo|num|folio)$/i) ||
            obtenerPropiedad(p, /clave|codigo|cve|folio/i) || nombreFeature(feature, "");
        return prefijo + ":" + String(id || indice);
    }

    function tensionKv(feature) {
        var p = feature && feature.properties || {};
        var valor = obtenerPropiedad(p, /tens|volt|kv/i);
        var n = parseFloat(String(valor || "").replace(/[^\d.]/g, ""));
        return Number.isFinite(n) ? n : null;
    }

    function distanciaKm(punto, feature) {
        if (!punto || !feature || !feature.geometry || !window.turf) return null;
        try {
            var tipo = feature.geometry.type;
            if (tipo === "Point") return window.turf.distance(punto, feature, { units: "kilometers" });
            if (tipo === "LineString") return window.turf.pointToLineDistance(punto, feature, { units: "kilometers" });
            if (tipo === "MultiLineString") {
                var lineas = feature.geometry.coordinates.map(function (coordenadas) { return window.turf.lineString(coordenadas); });
                return Math.min.apply(null, lineas.map(function (linea) {
                    return window.turf.pointToLineDistance(punto, linea, { units: "kilometers" });
                }));
            }
            return window.turf.distance(punto, window.turf.centroid(feature), { units: "kilometers" });
        } catch (e) { return null; }
    }

    function masCercano(punto, features) {
        var mejor = null;
        (features || []).forEach(function (feature) {
            var distancia = distanciaKm(punto, feature);
            if (!Number.isFinite(distancia)) return;
            if (!mejor || distancia < mejor.distancia) mejor = { feature: feature, distancia: distancia };
        });
        return mejor;
    }

    function gcrDelPunto(punto, gerencias, respaldo) {
        if (!punto || !window.turf) return codigoGcr(respaldo) || String(respaldo || "").toUpperCase();
        var encontrada = featuresGeo(gerencias).find(function (feature) {
            try { return window.turf.booleanPointInPolygon(punto, feature); } catch (e) { return false; }
        });
        return encontrada ? codigoGcrFeature(encontrada) : (codigoGcr(respaldo) || String(respaldo || "").toUpperCase());
    }

    function municipioDelPunto(punto, municipios, ubicacion) {
        var encontrada = featuresGeo(municipios).find(function (feature) {
            try { return window.turf.booleanPointInPolygon(punto, feature); } catch (e) { return false; }
        });
        if (!encontrada) {
            return {
                entidad: ESTADOS[normaliza(ubicacion && ubicacion.entidad)] || "",
                municipio: "",
                nombre: ubicacion && ubicacion.municipio || "",
                entidadNombre: ubicacion && ubicacion.entidad || "",
                areaKm2: null
            };
        }
        var p = encontrada.properties || {};
        var cvegeo = String(p.CVEGEO || p.cvegeo || "");
        var entidad = String(p.CVE_ENT || p.cve_ent || cvegeo.slice(0, 2)).padStart(2, "0");
        var municipio = String(p.CVE_MUN || p.cve_mun || cvegeo.slice(-3)).padStart(3, "0");
        var area = null;
        try { area = window.turf.area(encontrada) / 1000000; } catch (e) { }
        return {
            entidad: entidad,
            municipio: municipio,
            nombre: String(p.NOMGEO || p.nombre || ubicacion.municipio || "Municipio"),
            entidadNombre: ubicacion && ubicacion.entidad || "",
            areaKm2: Number.isFinite(area) ? area : null
        };
    }

    function cargarInegi(municipio) {
        if (!municipio || !municipio.entidad) return Promise.resolve(null);
        var clave = municipio.entidad + (municipio.municipio || "");
        if (inegiPorClave[clave]) return inegiPorClave[clave];
        var params = new URLSearchParams({ entidad: municipio.entidad });
        if (municipio.municipio) params.set("municipio", municipio.municipio);
        if (municipio.areaKm2) params.set("areaKm2", municipio.areaKm2.toFixed(2));
        inegiPorClave[clave] = fetch("/DashboardProyectos/IndicadoresInegi?" + params.toString(), {
            credentials: "same-origin", headers: { Accept: "application/json" }
        }).then(function (respuesta) {
            if (!respuesta.ok) throw new Error("HTTP " + respuesta.status);
            return respuesta.json();
        }).then(function (payload) {
            return payload && payload.ok ? payload.data : null;
        }).catch(function () { return null; });
        return inegiPorClave[clave];
    }

    function indicador(inegi, clave) {
        return inegi && Array.isArray(inegi.indicators)
            ? inegi.indicators.find(function (item) { return item.key === clave; })
            : null;
    }

    function mapaLimitado(items, limite, tarea) {
        var resultados = new Array(items.length);
        var siguiente = 0;
        function trabajador() {
            var indice = siguiente++;
            if (indice >= items.length) return Promise.resolve();
            return Promise.resolve(tarea(items[indice], indice))
                .then(function (resultado) { resultados[indice] = resultado; })
                .then(trabajador);
        }
        var trabajadores = [];
        for (var i = 0; i < Math.min(limite, items.length); i++) trabajadores.push(trabajador());
        return Promise.all(trabajadores).then(function () { return resultados; });
    }

    function fuenteConEstado(promesa, nombre) {
        return promesa.then(function (geo) { return { nombre: nombre, geo: geo, disponible: true }; })
            .catch(function (error) {
                console.warn("Capa territorial no disponible: " + nombre, error);
                return { nombre: nombre, geo: { type: "FeatureCollection", features: [] }, disponible: false };
            });
    }

    function geoVacio() {
        return { type: "FeatureCollection", features: [] };
    }

    function bboxProyecto(proyecto, radioPredeterminado) {
        if (!proyecto || !window.turf) return null;
        var ubicaciones = Array.isArray(proyecto.ubicaciones) ? proyecto.ubicaciones : [];
        var coberturas = ubicaciones.map(function (ubicacion) {
            var feature = featureUbicacion(ubicacion);
            return feature ? coberturaFeature(feature, radioUbicacion(ubicacion, radioPredeterminado)) : null;
        }).filter(Boolean);
        if (!coberturas.length) return null;
        try {
            var bbox = window.turf.bbox(window.turf.featureCollection(coberturas));
            return {
                minLon: Math.max(-119, bbox[0]), minLat: Math.max(14, bbox[1]),
                maxLon: Math.min(-86, bbox[2]), maxLat: Math.min(33.5, bbox[3])
            };
        } catch (e) { return null; }
    }

    function urlPermisos(tipo, bbox) {
        if (!bbox) return "";
        var params = new URLSearchParams({
            tipo: tipo,
            minLat: bbox.minLat.toFixed(5), minLon: bbox.minLon.toFixed(5),
            maxLat: bbox.maxLat.toFixed(5), maxLon: bbox.maxLon.toFixed(5)
        });
        return "/DashboardProyectos/PermisosEnergeticos?" + params.toString();
    }

    function fuentePermisos(tipo, bbox, nombre) {
        var url = urlPermisos(tipo, bbox);
        return url
            ? fuenteConEstado(fetchJson(url), nombre)
            : Promise.resolve({ nombre: nombre, geo: geoVacio(), disponible: false });
    }

    function elementosEnCobertura(cobertura, features) {
        var bboxCobertura = null;
        try { bboxCobertura = cobertura && window.turf ? window.turf.bbox(cobertura) : null; } catch (e) { bboxCobertura = null; }
        return (features || []).map(function (feature, indice) {
            return { feature: feature, indice: indice };
        }).filter(function (item) {
            if (bboxCobertura && bboxCache && item.feature && typeof item.feature === "object") {
                var bboxFeature = bboxCache.get(item.feature);
                if (!bboxFeature) {
                    try {
                        bboxFeature = window.turf.bbox(item.feature);
                        bboxCache.set(item.feature, bboxFeature);
                    } catch (e) { bboxFeature = null; }
                }
                if (bboxFeature && (bboxFeature[2] < bboxCobertura[0] || bboxFeature[0] > bboxCobertura[2] ||
                    bboxFeature[3] < bboxCobertura[1] || bboxFeature[1] > bboxCobertura[3])) return false;
            }
            return intersecta(cobertura, item.feature);
        });
    }

    function contextoDelPunto(punto, features) {
        if (!punto || !window.turf) return null;
        return (features || []).find(function (feature) {
            try { return window.turf.booleanPointInPolygon(punto, feature); }
            catch (e) { return false; }
        }) || null;
    }

    function propiedadFeature(feature, expresion, respaldo) {
        var valor = obtenerPropiedad(feature && feature.properties || {}, expresion);
        return valor == null || valor === "" ? (respaldo == null ? "" : respaldo) : valor;
    }

    // Progreso del análisis. Son dieciocho capas y varias vienen del CDN: el
    // conjunto tarda decenas de segundos, y hasta ahora la lámina sólo decía
    // "Analizando…", sin manera de saber si avanzaba o se habia colgado.
    var avanceSuscriptores = [];
    var avance = { hechas: 0, total: 0, ultima: "" };

    function alAvanzar(fn) {
        avanceSuscriptores.push(fn);
        fn(avance);
    }

    function anunciarAvance() {
        avanceSuscriptores.forEach(function (fn) {
            try { fn(avance); } catch (e) { }
        });
    }

    // Envuelve cada fuente para contar cuando termina, sin cambiar su valor.
    function contando(promesa, nombre) {
        avance.total += 1;
        anunciarAvance();
        return promesa.then(function (valor) {
            avance.hechas += 1;
            avance.ultima = nombre;
            anunciarAvance();
            return valor;
        }, function (error) {
            // Una capa caida tambien es progreso: el analisis sigue sin ella.
            avance.hechas += 1;
            avance.ultima = nombre;
            anunciarAvance();
            throw error;
        });
    }

    function analizar(url, radioPredeterminado, gcrRespaldo) {
        if (analisisPorUrl[url]) return analisisPorUrl[url];
        analisisPorUrl[url] = fetchJson(url).then(function (proyecto) {
            var bbox = bboxProyecto(proyecto, radioPredeterminado);
            return Promise.all([
                Promise.resolve(proyecto),
                contando(fuenteConEstado(fetchJson(URLS.gerencias), "Gerencias CENACE"), "Gerencias CENACE"),
                contando(fuenteConEstado(fetchJson(URLS.subestacionesTransmision), "Subestaciones de transmisión"), "Subestaciones de transmisión"),
                contando(fuenteConEstado(fetchJson(URLS.subestacionesDistribucion), "Subestaciones de distribución"), "Subestaciones de distribución"),
                contando(fuenteConEstado(fetchJson(URLS.lineas), "Líneas RNT"), "Líneas RNT"),
                contando(fuenteConEstado(fetchJson(URLS.municipios), "Municipios INEGI"), "Municipios INEGI"),
                contando(fuenteConEstado(fetchJson(URLS.anp), "ANP federales"), "ANP federales"),
                contando(fuenteConEstado(fetchJson(URLS.anpEstatal), "ANP estatales"), "ANP estatales"),
                contando(fuenteConEstado(fetchJson(URLS.ramsar), "Humedales RAMSAR"), "Humedales RAMSAR"),
                contando(fuenteConEstado(fetchJson(URLS.regionesIndigenas), "Regiones indígenas"), "Regiones indígenas"),
                contando(fuenteConEstado(fetchJson(URLS.ductosImportacion), "Ductos de importación"), "Ductos de importación"),
                contando(fuenteConEstado(fetchJson(URLS.ductosSistrangas), "Ductos SISTRANGAS"), "Ductos SISTRANGAS"),
                contando(fuenteConEstado(fetchJson(URLS.demanda), "Demanda CENACE"), "Demanda CENACE"),
                contando(fuenteConEstado(fetchJson(URLS.tarifas), "Divisiones CFE"), "Divisiones CFE"),
                contando(fuentePermisos("electricidad", bbox, "Permisos eléctricos"), "Permisos eléctricos"),
                contando(fuentePermisos("gas-natural", bbox, "Permisos de gas natural"), "Permisos de gas natural"),
                contando(fuentePermisos("gas-lp", bbox, "Permisos de gas LP"), "Permisos de gas LP"),
                contando(fuentePermisos("petroliferos", bbox, "Permisos de petrolíferos"), "Permisos de petrolíferos")
            ]);
        }).then(function (datos) {
            var proyecto = datos[0];
            var capas = {
                gerencias: datos[1], subestacionesTransmision: datos[2], subestacionesDistribucion: datos[3],
                lineas: datos[4], municipios: datos[5], anp: datos[6], anpEstatal: datos[7],
                ramsar: datos[8], regionesIndigenas: datos[9], ductosImportacion: datos[10],
                ductosSistrangas: datos[11], demanda: datos[12], tarifas: datos[13],
                permisosElectricidad: datos[14], permisosGasNatural: datos[15],
                permisosGasLp: datos[16], permisosPetroliferos: datos[17]
            };
            var todas = proyecto && Array.isArray(proyecto.ubicaciones) ? proyecto.ubicaciones : [];
            var validadas = todas.filter(function (ubicacion) { return ubicacion.validada; });
            var ubicaciones = validadas.length ? validadas : todas;
            var seFeatures = featuresGeo(capas.subestacionesTransmision.geo)
                .concat(featuresGeo(capas.subestacionesDistribucion.geo));
            var ltFeatures = featuresGeo(capas.lineas.geo);
            var ductosImportacion = featuresGeo(capas.ductosImportacion.geo);
            var ductosSistrangas = featuresGeo(capas.ductosSistrangas.geo);
            var demandaFeatures = featuresGeo(capas.demanda.geo);
            var tarifaFeatures = featuresGeo(capas.tarifas.geo);
            var permisosFeatures = {
                electricidad: featuresGeo(capas.permisosElectricidad.geo),
                gasNatural: featuresGeo(capas.permisosGasNatural.geo),
                gasLp: featuresGeo(capas.permisosGasLp.geo),
                petroliferos: featuresGeo(capas.permisosPetroliferos.geo)
            };

            var elementos = ubicaciones.map(function (ubicacion, indice) {
                var feature = featureUbicacion(ubicacion);
                if (!feature) return null;
                var centro = centroFeature(feature);
                var radio = radioUbicacion(ubicacion, radioPredeterminado);
                var cobertura = coberturaFeature(feature, radio);
                var gcr = gcrDelPunto(centro, capas.gerencias.geo, proyecto.gcr || gcrRespaldo);
                var municipio = municipioDelPunto(centro, capas.municipios.geo, ubicacion);
                var seDentro = elementosEnCobertura(cobertura, seFeatures);
                var ltDentro = elementosEnCobertura(cobertura, ltFeatures);
                var anp = elementosEnCobertura(cobertura, featuresGeo(capas.anp.geo));
                var anpEstatal = elementosEnCobertura(cobertura, featuresGeo(capas.anpEstatal.geo));
                var ramsar = elementosEnCobertura(cobertura, featuresGeo(capas.ramsar.geo));
                var regionesIndigenas = elementosEnCobertura(cobertura, featuresGeo(capas.regionesIndigenas.geo));
                return {
                    indice: indice,
                    ubicacion: ubicacion,
                    feature: feature,
                    centro: centro,
                    cobertura: cobertura,
                    radioKm: radio,
                    gcr: gcr,
                    municipio: municipio,
                    seDentro: seDentro,
                    ltDentro: ltDentro,
                    seCercana: masCercano(centro, seFeatures),
                    ltCercana: masCercano(centro, ltFeatures),
                    anp: anp,
                    anpEstatal: anpEstatal,
                    ramsar: ramsar,
                    regionesIndigenas: regionesIndigenas,
                    ductosImportacion: elementosEnCobertura(cobertura, ductosImportacion),
                    ductosSistrangas: elementosEnCobertura(cobertura, ductosSistrangas),
                    ductoImportacionCercano: masCercano(centro, ductosImportacion),
                    ductoSistrangasCercano: masCercano(centro, ductosSistrangas),
                    permisos: {
                        electricidad: elementosEnCobertura(cobertura, permisosFeatures.electricidad),
                        gasNatural: elementosEnCobertura(cobertura, permisosFeatures.gasNatural),
                        gasLp: elementosEnCobertura(cobertura, permisosFeatures.gasLp),
                        petroliferos: elementosEnCobertura(cobertura, permisosFeatures.petroliferos)
                    },
                    demanda: contextoDelPunto(centro, demandaFeatures),
                    tarifa: contextoDelPunto(centro, tarifaFeatures),
                    inegi: null
                };
            }).filter(Boolean);

            return mapaLimitado(elementos, 2, function (elemento) {
                return cargarInegi(elemento.municipio).then(function (inegi) {
                    elemento.inegi = inegi;
                    return elemento;
                });
            }).then(function () {
                var gcrs = Array.from(new Set(elementos.map(function (item) { return item.gcr; }).filter(Boolean)));
                var gcrRegional = codigoGcr(proyecto.gcr || gcrRespaldo);
                if (!elementos.length && gcrRegional) gcrs.push(gcrRegional);
                var lineal = elementos.some(function (item) { return /LineString|Polygon/.test(item.feature.geometry.type); });
                var clasificacion;
                if (!elementos.length) {
                    clasificacion = {
                        clave: "regional",
                        titulo: "Referencia regional GCR",
                        detalle: "Cobertura de referencia en la GCR " + (gcrs[0] || "por precisar") + ", sin geometría de red confirmada."
                    };
                } else if (elementos.length === 1) {
                    clasificacion = { clave: "unico", titulo: lineal ? "Proyecto lineal" : "Proyecto de ubicación única", detalle: "Cobertura territorial concentrada en una geometría." };
                } else if (gcrs.length > 1) {
                    clasificacion = { clave: "interregional", titulo: lineal ? "Proyecto lineal interregional" : "Proyecto interregional", detalle: elementos.length + " elementos distribuidos en " + gcrs.length + " GCR." };
                } else {
                    clasificacion = { clave: "multielemento", titulo: lineal ? "Proyecto lineal multielemento" : "Proyecto multielemento", detalle: elementos.length + " elementos con cobertura en la GCR " + (gcrs[0] || "por precisar") + "." };
                }
                return {
                    proyecto: proyecto,
                    elementos: elementos,
                    gcrs: gcrs,
                    clasificacion: clasificacion,
                    esReferenciaRegional: !elementos.length && gcrs.length > 0,
                    capas: capas
                };
            });
        });
        return analisisPorUrl[url];
    }

    function conjuntosUnicos(resultado) {
        var conjuntos = { se: new Set(), lt: new Set(), anp: new Set(), anpEstatal: new Set(), ramsar: new Set(), regionesIndigenas: new Set() };
        resultado.elementos.forEach(function (elemento) {
            ["se", "lt", "anp", "anpEstatal", "ramsar", "regionesIndigenas"].forEach(function (clave) {
                var fuente = clave === "se" ? elemento.seDentro : clave === "lt" ? elemento.ltDentro : elemento[clave];
                (fuente || []).forEach(function (item) { conjuntos[clave].add(claveFeature(item.feature, item.indice, clave)); });
            });
        });
        return conjuntos;
    }

    function itemsUnicos(elementos, propiedades, prefijo) {
        var vistos = new Set();
        var salida = [];
        elementos.forEach(function (elemento) {
            propiedades.forEach(function (propiedad) {
                (elemento[propiedad] || []).forEach(function (item) {
                    var clave = claveFeature(item.feature, item.indice, prefijo || propiedad);
                    if (vistos.has(clave)) return;
                    vistos.add(clave);
                    salida.push(item.feature);
                });
            });
        });
        return salida;
    }

    function contextosUnicos(elementos, propiedad, prefijo) {
        var vistos = new Set();
        return elementos.map(function (elemento) { return elemento[propiedad]; }).filter(Boolean).filter(function (feature, indice) {
            var clave = claveFeature(feature, indice, prefijo);
            if (vistos.has(clave)) return false;
            vistos.add(clave);
            return true;
        });
    }

    function analizarIntensivo(resultado) {
        if (resultado.intensivoPromise) return resultado.intensivoPromise;
        resultado.intensivoPromise = Promise.all([
            fuenteConEstado(fetchJson(URLS.centrales), "Centrales eléctricas"),
            fuenteConEstado(fetchJson(URLS.presas), "Presas"),
            fuenteConEstado(fetchJson(URLS.generacionPrivada), "Generación privada planeada"),
            fuenteConEstado(fetchJson(URLS.conservacion), "Conservación voluntaria"),
            fuenteConEstado(fetchJson(URLS.sitiosArqueologicos), "Sitios arqueológicos"),
            fuenteConEstado(fetchJson(URLS.zonasArqueologicas), "Zonas arqueológicas"),
            fuenteConEstado(fetchJson(URLS.zonasHistoricas), "Zonas históricas")
        ]).then(function (datos) {
            var fuentes = {
                centrales: datos[0], presas: datos[1], generacionPrivada: datos[2],
                conservacion: datos[3], sitiosArqueologicos: datos[4],
                zonasArqueologicas: datos[5], zonasHistoricas: datos[6]
            };
            var features = {};
            Object.keys(fuentes).forEach(function (clave) { features[clave] = featuresGeo(fuentes[clave].geo); });
            resultado.elementos.forEach(function (elemento) {
                elemento.intensivo = {};
                Object.keys(features).forEach(function (clave) {
                    elemento.intensivo[clave] = elementosEnCobertura(elemento.cobertura, features[clave]);
                });
            });
            return { fuentes: fuentes };
        });
        return resultado.intensivoPromise;
    }

    function numeroPropiedad(feature, expresion) {
        var valor = propiedadFeature(feature, expresion, null);
        if (valor == null || valor === "") return null;
        var n = Number(String(valor).replace(/,/g, ""));
        return Number.isFinite(n) ? n : null;
    }

    function barraEjecutiva(etiqueta, valor, maximo, unidad, clase) {
        if (!Number.isFinite(valor)) return "";
        var proporcion = maximo > 0 ? Math.max(2, Math.min(100, (valor / maximo) * 100)) : 0;
        return '<div class="pam-ejecutivo-barra ' + (clase || "") + '">' +
            '<div><span>' + textoSeguro(etiqueta) + '</span><strong>' + numero(valor, unidad === "GWh" ? 1 : 0) + ' ' + unidad + '</strong></div>' +
            '<i><b style="width:' + proporcion.toFixed(2) + '%"></b></i>' +
        '</div>';
    }

    function graficaDemanda(resultado) {
        var regiones = contextosUnicos(resultado.elementos, "demanda", "demanda").slice(0, 3);
        if (!regiones.length) return '<div class="pam-ejecutivo-vacio">Sin contexto regional de demanda disponible.</div>';
        var series = regiones.map(function (feature) {
            return {
                nombre: String(propiedadFeature(feature, /^(region|nombre)$/i, "Región CENACE")),
                demanda: numeroPropiedad(feature, /demand_mw|demanda.*mw/i),
                generacion: numeroPropiedad(feature, /generation_mw|generacion.*mw/i),
                pronostico: numeroPropiedad(feature, /forecast_mw|pronostico.*mw/i)
            };
        });
        var maximo = Math.max.apply(null, series.reduce(function (lista, item) {
            return lista.concat([item.demanda, item.generacion, item.pronostico].filter(Number.isFinite));
        }, [1]));
        return series.map(function (item) {
            return '<div class="pam-ejecutivo-serie"><h4>' + textoSeguro(item.nombre) + '</h4>' +
                barraEjecutiva("Demanda", item.demanda, maximo, "MW", "is-demanda") +
                barraEjecutiva("Generación", item.generacion, maximo, "MW", "is-generacion") +
                barraEjecutiva("Pronóstico", item.pronostico, maximo, "MW", "is-pronostico") +
            '</div>';
        }).join("");
    }

    function graficaTarifaria(resultado) {
        var divisiones = contextosUnicos(resultado.elementos, "tarifa", "tarifa").slice(0, 3);
        if (!divisiones.length) return '<div class="pam-ejecutivo-vacio">Sin contexto de división tarifaria disponible.</div>';
        var series = divisiones.map(function (feature) {
            return {
                nombre: "División " + String(propiedadFeature(feature, /^division$/i, "CFE")),
                anio: propiedadFeature(feature, /reference_year|anio|año/i, ""),
                usuarios: numeroPropiedad(feature, /users_total|usuarios/i),
                energia: numeroPropiedad(feature, /energy_gwh|energia.*gwh/i)
            };
        });
        var maxUsuarios = Math.max.apply(null, series.map(function (item) { return item.usuarios; }).filter(Number.isFinite).concat([1]));
        var maxEnergia = Math.max.apply(null, series.map(function (item) { return item.energia; }).filter(Number.isFinite).concat([1]));
        return series.map(function (item) {
            return '<div class="pam-ejecutivo-serie"><h4>' + textoSeguro(item.nombre) + (item.anio ? '<small>' + textoSeguro(item.anio) + '</small>' : '') + '</h4>' +
                barraEjecutiva("Usuarios", item.usuarios, maxUsuarios, "usuarios", "is-usuarios") +
                barraEjecutiva("Energía", item.energia, maxEnergia, "GWh", "is-energia") +
            '</div>';
        }).join("");
    }

    function kpiEjecutivo(etiqueta, valor, detalle, clase) {
        return '<article class="pam-ejecutivo-kpi ' + (clase || "") + '"><span>' + textoSeguro(etiqueta) + '</span><strong>' + textoSeguro(valor) + '</strong><small>' + textoSeguro(detalle) + '</small></article>';
    }

    function nombresFeatures(features, maximo) {
        return (features || []).map(function (feature) { return nombreFeature(feature, ""); })
            .filter(Boolean).filter(function (nombre, indice, lista) { return lista.indexOf(nombre) === indice; })
            .slice(0, maximo || 3);
    }

    function renderEjecutivo(resultado, contenedor) {
        if (!contenedor || contenedor.dataset.rendered) return Promise.resolve();
        return analizarIntensivo(resultado).then(function () {
            var unicos = conjuntosUnicos(resultado);
            var ductos = itemsUnicos(resultado.elementos, ["ductosImportacion", "ductosSistrangas"], "ducto");
            var permisos = [];
            ["electricidad", "gasNatural", "gasLp", "petroliferos"].forEach(function (tipo) {
                permisos = permisos.concat(itemsUnicos(resultado.elementos.map(function (elemento) {
                    return { permisosTemporales: elemento.permisos[tipo] || [] };
                }), ["permisosTemporales"], "permiso-" + tipo));
            });
            var centrales = itemsUnicos(resultado.elementos.map(function (elemento) { return { items: elemento.intensivo.centrales }; }), ["items"], "central");
            var presas = itemsUnicos(resultado.elementos.map(function (elemento) { return { items: elemento.intensivo.presas }; }), ["items"], "presa");
            var generacion = itemsUnicos(resultado.elementos.map(function (elemento) { return { items: elemento.intensivo.generacionPrivada }; }), ["items"], "generacion");
            var conservacion = itemsUnicos(resultado.elementos.map(function (elemento) { return { items: elemento.intensivo.conservacion }; }), ["items"], "conservacion");
            var patrimonio = [];
            ["sitiosArqueologicos", "zonasArqueologicas", "zonasHistoricas"].forEach(function (clave) {
                patrimonio = patrimonio.concat(itemsUnicos(resultado.elementos.map(function (elemento) { return { items: elemento.intensivo[clave] }; }), ["items"], clave));
            });
            var ambiente = unicos.anp.size + unicos.anpEstatal.size + unicos.ramsar.size + conservacion.length;
            var social = unicos.regionesIndigenas.size + patrimonio.length;
            var hallazgos = nombresFeatures(centrales.concat(presas, generacion), 2)
                .concat(nombresFeatures(conservacion, 2), nombresFeatures(patrimonio, 2));

            contenedor.innerHTML = '<div class="pam-ejecutivo-layout">' +
                '<div class="pam-ejecutivo-graficas">' +
                    '<section class="pam-ejecutivo-panel"><header><span>Balance regional</span><strong>Demanda y generación</strong><small>MW</small></header>' + graficaDemanda(resultado) + '</section>' +
                    '<section class="pam-ejecutivo-panel"><header><span>Mercado eléctrico</span><strong>Contexto tarifario</strong><small>CFE</small></header>' + graficaTarifaria(resultado) + '</section>' +
                '</div>' +
                '<aside class="pam-ejecutivo-resumen">' +
                    '<div class="pam-ejecutivo-resumen__head"><span>Incidencia en coberturas</span><strong>Lectura territorial consolidada</strong><small>' + numero(resultado.elementos.length) + ' elemento(s) · ' + textoSeguro(resultado.gcrs.length ? "GCR " + resultado.gcrs.join(" · ") : "ámbito por precisar") + '</small></div>' +
                    '<div class="pam-ejecutivo-kpis">' +
                        kpiEjecutivo("Red eléctrica", numero(unicos.se.size + unicos.lt.size), numero(unicos.se.size) + " SE · " + numero(unicos.lt.size) + " LT", "is-red") +
                        kpiEjecutivo("Permisos", numero(permisos.length), "energéticos en cobertura", "is-permisos") +
                        kpiEjecutivo("Ductos", numero(ductos.length), "tramos coincidentes", "is-ductos") +
                        kpiEjecutivo("Generación", numero(centrales.length + presas.length + generacion.length), numero(centrales.length) + " centrales · " + numero(presas.length) + " presas", "is-generacion") +
                        kpiEjecutivo("Ambiente", numero(ambiente), "ANP, RAMSAR y conservación", "is-ambiente") +
                        kpiEjecutivo("Social y patrimonio", numero(social), "regiones y sitios culturales", "is-patrimonio") +
                    '</div>' +
                    (hallazgos.length ? '<div class="pam-ejecutivo-hallazgos">' + hallazgos.map(function (nombre) { return '<span>' + textoSeguro(nombre) + '</span>'; }).join("") + '</div>' : '') +
                    '<p>Coincidencias dentro de las áreas de proximidad; constituyen una señal ejecutiva para priorizar revisión técnica, ambiental y social.</p>' +
                '</aside>' +
            '</div>';
            contenedor.dataset.rendered = "true";
        }).catch(function (error) {
            console.error("No fue posible preparar el perfil territorial ejecutivo", error);
            contenedor.innerHTML = '<div class="pam-territorial__empty"><strong>Perfil ejecutivo no disponible</strong><span>Las capas temáticas no pudieron consultarse en esta sesión.</span></div>';
        });
    }

    function nombresUnicos(elementos, propiedad, maximo) {
        var nombres = [];
        elementos.forEach(function (elemento) {
            (elemento[propiedad] || []).forEach(function (item) {
                var nombre = nombreFeature(item.feature, "");
                if (nombre && nombres.indexOf(nombre) < 0) nombres.push(nombre);
            });
        });
        return nombres.slice(0, maximo || 4);
    }

    function renderResumen(resultado, contenedor, status) {
        if (!resultado.elementos.length) {
            var cobertura = resultado.gcrs.length ? resultado.gcrs.join(" · ") : "Por precisar";
            contenedor.innerHTML =
                '<div class="pam-territorial__tipo"><span>Clasificación territorial</span><strong>' + textoSeguro(resultado.clasificacion.titulo) + '</strong><small>' + textoSeguro(resultado.clasificacion.detalle) + '</small></div>' +
                '<div class="pam-territorial__kpis pam-territorial__kpis--regional">' +
                    kpi("Cobertura territorial", cobertura, resultado.gcrs.length ? "GCR de referencia" : "ámbito por precisar") +
                    kpi("Ubicación de red", "Pendiente", "sin geometría confirmada") +
                '</div>' +
                (resultado.gcrs.length ? '<div class="pam-territorial__hallazgos"><span class="pam-territorial__chip"><i></i>GCR ' + textoSeguro(cobertura) + '</span></div>' : '') +
                '<p class="pam-territorial__nota">La cobertura regional permite contextualizar el proyecto; la caracterización de proximidad se incorporará cuando exista una ubicación de red confirmada.</p>';
            if (status) status.textContent = resultado.gcrs.length ? "Referencia regional · GCR " + cobertura : "Cobertura por precisar";
            return;
        }
        var unicos = conjuntosUnicos(resultado);
        var municipios = Array.from(new Set(resultado.elementos.map(function (item) { return item.municipio.nombre; }).filter(Boolean)));
        var anpNombres = nombresUnicos(resultado.elementos, "anp", 2).concat(nombresUnicos(resultado.elementos, "anpEstatal", 2));
        var indNombres = nombresUnicos(resultado.elementos, "regionesIndigenas", 2);
        var poblacion = resultado.elementos.length === 1 ? indicador(resultado.elementos[0].inegi, "population_total") : null;
        var inegiContextos = new Set(resultado.elementos.map(function (item) {
            return item.municipio.entidad + item.municipio.municipio;
        }).filter(Boolean)).size;
        var chips = resultado.gcrs.map(function (gcr) { return "GCR " + gcr; })
            .concat(municipios.slice(0, 3))
            .concat(anpNombres)
            .concat(indNombres);
        contenedor.innerHTML =
            '<div class="pam-territorial__tipo"><span>Clasificación territorial</span><strong>' + textoSeguro(resultado.clasificacion.titulo) + '</strong><small>' + textoSeguro(resultado.clasificacion.detalle) + '</small></div>' +
            '<div class="pam-territorial__kpis">' +
                kpi("Elementos analizados", numero(resultado.elementos.length), resultado.elementos.length === 1 ? "geometría" : "geometrías") +
                kpi("Cobertura territorial", resultado.gcrs.length ? resultado.gcrs.join(" · ") : "—", resultado.gcrs.length === 1 ? "una GCR" : resultado.gcrs.length + " GCR") +
                kpi("Subestaciones RNT", numero(unicos.se.size), "en coberturas") +
                kpi("Líneas RNT", numero(unicos.lt.size), "tramos intersectados") +
                kpi("Contexto INEGI", poblacion ? numero(poblacion.value) : numero(inegiContextos), poblacion ? "personas · municipio" : "ámbitos municipales") +
                kpi("Sensibilidad territorial", numero(unicos.anp.size + unicos.anpEstatal.size + unicos.ramsar.size + unicos.regionesIndigenas.size), "hallazgos temáticos") +
            '</div>' +
            (chips.length ? '<div class="pam-territorial__hallazgos">' + chips.slice(0, 7).map(function (chip) { return '<span class="pam-territorial__chip"><i></i>' + textoSeguro(chip) + '</span>'; }).join("") + '</div>' : "") +
            '<p class="pam-territorial__nota">La sensibilidad territorial identifica coincidencias preliminares con capas ambientales y sociales; requiere verificación específica para el proyecto.</p>';
        if (status) {
            var disponibles = Object.keys(resultado.capas).filter(function (clave) { return resultado.capas[clave].disponible; }).length;
            status.textContent = resultado.elementos.length + " elemento(s) · " + (resultado.gcrs.length ? "GCR " + resultado.gcrs.join(" · ") : disponibles + " capas de contexto");
        }
    }

    function kpi(etiqueta, valor, nota) {
        return '<article class="pam-territorial__kpi"><span>' + textoSeguro(etiqueta) + '</span><strong>' + textoSeguro(valor) + '</strong><small>' + textoSeguro(nota) + '</small></article>';
    }

    function descripcionCercano(cercano, tipo) {
        if (!cercano) return "Sin dato";
        var nombre = nombreFeature(cercano.feature, tipo);
        var kv = tensionKv(cercano.feature);
        return nombre + (kv ? " · " + numero(kv) + " kV" : "") + " · " + numero(cercano.distancia, 1) + " km";
    }

    function resumenInegi(elemento) {
        if (!elemento.inegi) return "Contexto INEGI no disponible para esta ubicación.";
        var poblacion = indicador(elemento.inegi, "population_total");
        var indigena = indicador(elemento.inegi, "indigenous_language_share");
        var unidades = indicador(elemento.inegi, "economic_units");
        var partes = [];
        if (poblacion) partes.push(numero(poblacion.value) + " habitantes");
        if (indigena) partes.push(numero(indigena.value, 1) + "% hablante de lengua indígena");
        if (unidades) partes.push(numero(unidades.value) + " unidades económicas");
        return partes.length ? partes.join(" · ") : "INEGI no devolvió indicadores comparables.";
    }

    function renderComparativas(resultado) {
        document.querySelectorAll("[data-pam-territorial-comparison]").forEach(function (contenedor) {
            var pagina = Number(contenedor.dataset.territorialPage) || 0;
            var inicio = pagina * 3;
            var elementos = resultado.elementos.slice(inicio, inicio + 3);
            if (!elementos.length) {
                contenedor.innerHTML = '<div class="pam-territorial__empty"><strong>Sin elementos para comparar</strong><span>No se encontraron geometrías territoriales utilizables en esta página.</span></div>';
                return;
            }
            contenedor.innerHTML = '<div class="pam-territorial__cards">' + elementos.map(function (elemento, local) {
                var u = elemento.ubicacion;
                var precision = [u.precisionUbicacion, u.validada ? "validada" : "sugerida"].filter(Boolean).join(" · ");
                var sensibilidad = elemento.anp.length + elemento.anpEstatal.length + elemento.ramsar.length;
                var social = elemento.regionesIndigenas.length;
                return '<article class="pam-territorial-card">' +
                    '<div class="pam-territorial-card__head"><span class="pam-territorial-card__num">' + numero(inicio + local + 1) + '</span><div><strong>' + textoSeguro(u.etiqueta || u.claveElementoRed || "Elemento territorial") + '</strong><small>' + textoSeguro([elemento.municipio.nombre, elemento.municipio.entidadNombre].filter(Boolean).join(", ") || "Ámbito por precisar") + '</small></div><span class="pam-territorial-card__gcr">GCR ' + textoSeguro(elemento.gcr || "—") + '</span></div>' +
                    '<dl class="pam-territorial-card__facts">' +
                        '<div><dt>Geometría / precisión</dt><dd>' + textoSeguro((elemento.feature.geometry.type || "Geometría") + " · " + (precision || "sin clasificar")) + '</dd></div>' +
                        '<div><dt>Cobertura aplicada</dt><dd>' + numero(elemento.radioKm, elemento.radioKm % 1 ? 1 : 0) + ' km</dd></div>' +
                        '<div><dt>Subestación más cercana</dt><dd>' + textoSeguro(descripcionCercano(elemento.seCercana, "Subestación")) + '</dd></div>' +
                        '<div><dt>Línea más cercana</dt><dd>' + textoSeguro(descripcionCercano(elemento.ltCercana, "Línea RNT")) + '</dd></div>' +
                        '<div><dt>Infraestructura en cobertura</dt><dd>' + numero(elemento.seDentro.length) + ' SE · ' + numero(elemento.ltDentro.length) + ' LT</dd></div>' +
                        '<div><dt>Sensibilidad territorial</dt><dd>' + numero(sensibilidad) + ' ambiental · ' + numero(social) + ' indígena</dd></div>' +
                    '</dl>' +
                    '<div class="pam-territorial-card__inegi"><b>Contexto municipal · INEGI</b><p>' + textoSeguro(resumenInegi(elemento)) + '</p></div>' +
                    '<p class="pam-territorial-card__source">Fuente: ' + textoSeguro(u.fuente || "Repositorio territorial PAM") + ' · contexto municipal INEGI de referencia.</p>' +
                '</article>';
            }).join("") + '</div>';
        });
    }

    function nombresItems(items, maximo) {
        return (items || []).map(function (item) {
            return nombreFeature(item.feature, "");
        }).filter(Boolean).filter(function (nombre, indice, lista) {
            return lista.indexOf(nombre) === indice;
        }).slice(0, maximo || 2);
    }

    function tarjetaElemento(icono, titulo, valor, detalle, clase) {
        return '<article class="pam-elemento-card ' + (clase || "") + '">' +
            '<div class="pam-elemento-card__head"><i class="' + icono + '"></i><span>' + textoSeguro(titulo) + '</span></div>' +
            '<strong>' + textoSeguro(valor) + '</strong>' +
            (detalle ? '<p>' + textoSeguro(detalle) + '</p>' : '') +
        '</article>';
    }

    function resumenPermisos(elemento) {
        var grupos = [
            ["electricidad", "eléctricos"], ["gasNatural", "gas natural"],
            ["gasLp", "gas LP"], ["petroliferos", "petrolíferos"]
        ];
        var partes = [];
        var nombres = [];
        grupos.forEach(function (grupo) {
            var items = elemento.permisos[grupo[0]] || [];
            if (!items.length) return;
            partes.push(numero(items.length) + " " + grupo[1]);
            nombres = nombres.concat(items.map(function (item) {
                return String(propiedadFeature(item.feature, /numeroPermiso|numero_permiso|permiso/i, "")).trim();
            }).filter(Boolean));
        });
        return { total: partes.length, valor: partes.join(" · "), detalle: nombres.slice(0, 3).join(" · ") };
    }

    function resumenDemanda(feature) {
        if (!feature) return null;
        var region = propiedadFeature(feature, /^(region|nombre)$/i, "Región CENACE");
        var demanda = propiedadFeature(feature, /demand_mw|demanda.*mw/i, null);
        var pronostico = propiedadFeature(feature, /forecast_mw|pronostico.*mw/i, null);
        return {
            valor: String(region),
            detalle: [
                demanda != null ? "Demanda " + numero(demanda) + " MW" : "",
                pronostico != null ? "Pronóstico " + numero(pronostico) + " MW" : ""
            ].filter(Boolean).join(" · ")
        };
    }

    function resumenTarifa(feature) {
        if (!feature) return null;
        var division = propiedadFeature(feature, /^division$/i, "División CFE");
        var usuarios = propiedadFeature(feature, /users_total|usuarios/i, null);
        var energia = propiedadFeature(feature, /energy_gwh|energia.*gwh/i, null);
        return {
            valor: "División " + division,
            detalle: [
                usuarios != null ? numero(usuarios) + " usuarios" : "",
                energia != null ? numero(energia, 1) + " GWh" : ""
            ].filter(Boolean).join(" · ")
        };
    }

    function renderElementosDetallados(resultado) {
        document.querySelectorAll("[data-pam-territorial-element]").forEach(function (contenedor) {
            var indice = Number(contenedor.dataset.territorialElementIndex);
            var elemento = resultado.elementos[indice];
            if (!elemento) {
                contenedor.innerHTML = '<div class="pam-territorial__empty"><strong>Elemento sin geometría operativa</strong><span>La cobertura regional permanece disponible en el resumen territorial.</span></div>';
                return;
            }
            if (!contenedor.dataset.rendered) {
                var permisos = resumenPermisos(elemento);
                var demanda = resumenDemanda(elemento.demanda);
                var tarifa = resumenTarifa(elemento.tarifa);
                var municipio = [elemento.municipio.nombre, elemento.municipio.entidadNombre].filter(Boolean).join(", ") || "Ámbito municipal por precisar";
                var seNombres = nombresItems(elemento.seDentro, 2);
                var ltNombres = nombresItems(elemento.ltDentro, 2);
                var sensibilidad = elemento.anp.length + elemento.anpEstatal.length + elemento.ramsar.length + elemento.regionesIndigenas.length;
                var ductosTotal = elemento.ductosImportacion.length + elemento.ductosSistrangas.length;
                var ductoCercano = elemento.ductoSistrangasCercano || elemento.ductoImportacionCercano;
                var tarjetas = [];

                tarjetas.push(tarjetaElemento(
                    "bi bi-lightning-charge", "Red eléctrica",
                    numero(elemento.seDentro.length) + " SE · " + numero(elemento.ltDentro.length) + " LT",
                    seNombres.concat(ltNombres).slice(0, 3).join(" · ") || "Infraestructura RNT dentro de la cobertura",
                    "is-electricidad"));
                if (permisos.total) {
                    tarjetas.push(tarjetaElemento("bi bi-file-earmark-check", "Permisos energéticos", permisos.valor, permisos.detalle, "is-permisos"));
                }
                if (ductosTotal || (ductoCercano && Number.isFinite(ductoCercano.distancia))) {
                    tarjetas.push(tarjetaElemento(
                        "bi bi-signpost-split", "Gas y ductos",
                        ductosTotal ? numero(ductosTotal) + " trazo(s) en cobertura" : "Infraestructura próxima",
                        ductosTotal
                            ? nombresItems(elemento.ductosImportacion.concat(elemento.ductosSistrangas), 3).join(" · ")
                            : nombreFeature(ductoCercano.feature, "Ducto") + " · " + numero(ductoCercano.distancia, 1) + " km",
                        "is-gas"));
                }
                if (demanda) tarjetas.push(tarjetaElemento("bi bi-graph-up", "Demanda CENACE", demanda.valor, demanda.detalle, "is-demanda"));
                if (tarifa) tarjetas.push(tarjetaElemento("bi bi-people", "División CFE", tarifa.valor, tarifa.detalle, "is-tarifa"));
                if (elemento.inegi) {
                    tarjetas.push(tarjetaElemento("bi bi-buildings", "Contexto INEGI", municipio, resumenInegi(elemento), "is-inegi"));
                }
                if (sensibilidad) {
                    tarjetas.push(tarjetaElemento(
                        "bi bi-tree", "Sensibilidad territorial",
                        numero(sensibilidad) + " coincidencia(s)",
                        nombresItems(elemento.anp.concat(elemento.anpEstatal, elemento.ramsar, elemento.regionesIndigenas), 4).join(" · "),
                        "is-sensibilidad"));
                }

                var u = elemento.ubicacion;
                contenedor.innerHTML =
                    '<div class="pam-elemento__layout">' +
                        '<div class="pam-elemento__map" data-territorial-element-map="' + indice + '" aria-label="Mapa de ' + textoSeguro(u.etiqueta || "elemento territorial") + '"></div>' +
                        '<aside class="pam-elemento__panel">' +
                            '<div class="pam-elemento__identity"><span>Elemento ' + numero(indice + 1) + ' de ' + numero(resultado.elementos.length) + '</span><strong>' + textoSeguro(u.etiqueta || u.claveElementoRed || "Elemento territorial") + '</strong><small>' + textoSeguro(municipio + " · GCR " + (elemento.gcr || "—")) + '</small>' + '<em class="pam-elemento__radio">' + textoSeguro("Área de análisis · " + numero(elemento.radioKm, elemento.radioKm % 1 ? 1 : 0) + ' km alrededor') + '</em></div>' +
                            '<div class="pam-elemento__cards">' + tarjetas.join("") + '</div>' +
                            '<p class="pam-elemento__source">Fuentes: cartera PAM, red eléctrica DGMESNIE, CRE, CENACE, CFE e INEGI. Coincidencias calculadas sobre la cobertura del elemento.</p>' +
                        '</aside>' +
                    '</div>';
                contenedor.dataset.rendered = "true";
            }
            var lamina = contenedor.closest("[data-slide]");
            if (lamina && lamina.classList.contains("is-active")) crearMapaElemento(resultado, elemento, contenedor, indice);
        });
    }

    function renderMatricesTerritoriales(resultado) {
        document.querySelectorAll("[data-pam-territorial-matrix]").forEach(function (contenedor) {
            if (contenedor.dataset.rendered) return;
            var pagina = Number(contenedor.dataset.territorialPage) || 0;
            var inicio = pagina * 5;
            var elementos = resultado.elementos.slice(inicio, inicio + 5);
            if (!elementos.length) {
                contenedor.innerHTML = '<div class="pam-territorial__empty"><strong>Sin elementos en esta página</strong><span>La matriz se ajusta al inventario territorial vigente.</span></div>';
                return;
            }
            var filas = elementos.map(function (elemento, local) {
                var permisos = Object.keys(elemento.permisos).reduce(function (total, clave) {
                    return total + elemento.permisos[clave].length;
                }, 0);
                var ductos = elemento.ductosImportacion.length + elemento.ductosSistrangas.length;
                var demanda = resumenDemanda(elemento.demanda);
                var tarifa = resumenTarifa(elemento.tarifa);
                var sensibilidad = elemento.anp.length + elemento.anpEstatal.length + elemento.ramsar.length + elemento.regionesIndigenas.length;
                var municipio = [elemento.municipio.nombre, elemento.municipio.entidadNombre].filter(Boolean).join(", ") || "Ámbito por precisar";
                return '<article class="pam-elemento-matriz__row">' +
                    '<div><b>' + numero(inicio + local + 1) + '</b><span><strong>' + textoSeguro(elemento.ubicacion.etiqueta || elemento.ubicacion.claveElementoRed || "Elemento territorial") + '</strong><small>' + textoSeguro(municipio + " · GCR " + (elemento.gcr || "—")) + '</small></span></div>' +
                    '<span><strong>' + numero(elemento.seDentro.length) + ' SE · ' + numero(elemento.ltDentro.length) + ' LT</strong><small>en cobertura</small></span>' +
                    '<span><strong>' + numero(permisos) + ' permiso(s)</strong><small>' + (ductos ? numero(ductos) + ' ducto(s)' : 'sin ductos intersectados') + '</small></span>' +
                    '<span><strong>' + textoSeguro(demanda ? demanda.valor : "Demanda no disponible") + '</strong><small>' + textoSeguro(tarifa ? tarifa.valor : "División CFE no disponible") + '</small></span>' +
                    '<span><strong>' + numero(sensibilidad) + ' sensibilidad(es)</strong><small>' + textoSeguro(elemento.inegi ? resumenInegi(elemento) : "Contexto INEGI no disponible") + '</small></span>' +
                '</article>';
            }).join("");
            contenedor.innerHTML =
                '<div class="pam-elemento-matriz__head"><span>Elemento y cobertura</span><span>Red eléctrica</span><span>Permisos y ductos</span><span>Demanda y tarifas</span><span>Entorno territorial</span></div>' +
                '<div class="pam-elemento-matriz__body">' + filas + '</div>';
            contenedor.dataset.rendered = "true";
        });
    }

    function agregarGeo(layer, features, opciones) {
        if (!features || !features.length) return;
        L.geoJSON({ type: "FeatureCollection", features: features }, opciones).addTo(layer);
    }

    function coordenadaPunto(feature) {
        if (!feature || !feature.geometry || feature.geometry.type !== "Point") return null;
        var coordenadas = feature.geometry.coordinates;
        return Array.isArray(coordenadas) && coordenadas.length >= 2
            ? [Number(coordenadas[0]), Number(coordenadas[1])]
            : null;
    }

    function claveCoordenada(feature) {
        var coordenadas = coordenadaPunto(feature);
        if (!coordenadas || !Number.isFinite(coordenadas[0]) || !Number.isFinite(coordenadas[1])) return "";
        // Cinco decimales equivalen aproximadamente a un metro. Las ubicaciones
        // que realmente coinciden se ordenan visualmente sin alterar su geometría.
        return coordenadas[0].toFixed(5) + "|" + coordenadas[1].toFixed(5);
    }

    function crearMapa(resultado) {
        var el = document.getElementById("pam-territorial-mapa");
        if (!el || mapaListo || typeof L === "undefined" || !window.turf) return;
        if (!el.clientWidth || !el.clientHeight) return;
        mapaListo = true;
        mapa = L.map(el, {
            zoomControl: false, attributionControl: false, dragging: false, scrollWheelZoom: false,
            doubleClickZoom: false, boxZoom: false, keyboard: false, touchZoom: false, preferCanvas: true
        });
        mapa.setView([23.6, -102.5], 5);

        var gcrActivas = new Set(resultado.gcrs);
        var grupoGerencias = L.geoJSON({ type: "FeatureCollection", features: featuresGeo(resultado.capas.gerencias.geo) }, {
            style: function (feature) {
                var activa = gcrActivas.has(codigoGcrFeature(feature));
                return { color: activa ? GUINDA : "#d7d1ca", weight: activa ? 1.7 : .55, fillColor: activa ? GUINDA : "#f1ede8", fillOpacity: activa ? .08 : .22 };
            }
        }).addTo(mapa);

        if (!resultado.elementos.length) {
            var gerenciasActivas = L.geoJSON({
                type: "FeatureCollection",
                features: featuresGeo(resultado.capas.gerencias.geo).filter(function (feature) {
                    return gcrActivas.has(codigoGcrFeature(feature));
                })
            });
            try {
                var limitesGcr = gerenciasActivas.getBounds();
                if (limitesGcr && limitesGcr.isValid()) mapa.fitBounds(limitesGcr.pad(.08), { padding: [12, 12], maxZoom: 7 });
                else if (grupoGerencias.getBounds().isValid()) mapa.fitBounds(grupoGerencias.getBounds(), { padding: [8, 8] });
            } catch (e) { }
            setTimeout(function () { if (mapa) mapa.invalidateSize(false); }, 80);
            return;
        }

        var ambientales = [];
        var indigenas = [];
        var lineas = [];
        var subestaciones = [];
        resultado.elementos.forEach(function (elemento) {
            ambientales = ambientales.concat(elemento.anp.map(function (x) { return x.feature; }), elemento.anpEstatal.map(function (x) { return x.feature; }), elemento.ramsar.map(function (x) { return x.feature; }));
            indigenas = indigenas.concat(elemento.regionesIndigenas.map(function (x) { return x.feature; }));
            lineas = lineas.concat(elemento.ltDentro.map(function (x) { return x.feature; }));
            subestaciones = subestaciones.concat(elemento.seDentro.map(function (x) { return x.feature; }));
        });
        agregarGeo(mapa, ambientales, { style: { color: VERDE, weight: 1, fillColor: VERDE, fillOpacity: .08 } });
        agregarGeo(mapa, indigenas, { style: { color: "#B24C6C", weight: 1, dashArray: "4 3", fillColor: "#B24C6C", fillOpacity: .05 } });

        var grupoCoberturas = L.featureGroup().addTo(mapa);
        var gruposCoincidentes = new Map();
        resultado.elementos.forEach(function (elemento, indice) {
            var clave = claveCoordenada(elemento.feature);
            if (!clave) return;
            if (!gruposCoincidentes.has(clave)) gruposCoincidentes.set(clave, []);
            gruposCoincidentes.get(clave).push(indice);
        });
        var posicionesCoincidentes = new Map();
        gruposCoincidentes.forEach(function (indices) {
            if (indices.length < 2) return;
            indices.forEach(function (indice, posicion) {
                posicionesCoincidentes.set(indice, { posicion: posicion, total: indices.length });
            });
        });

        resultado.elementos.forEach(function (elemento, indice) {
            if (elemento.cobertura) {
                L.geoJSON(elemento.cobertura, { style: { color: DORADO, weight: 1.8, dashArray: "7 5", fillColor: DORADO, fillOpacity: .08 } }).addTo(grupoCoberturas);
            }
            var coincide = posicionesCoincidentes.has(indice);
            L.geoJSON(elemento.feature, {
                style: { color: GUINDA, weight: 4, fillColor: GUINDA, fillOpacity: .2 },
                pointToLayer: function (feature, latlng) {
                    return L.circleMarker(latlng, { radius: coincide ? 3.5 : 7, color: "#fff", weight: coincide ? 1.2 : 2, fillColor: GUINDA, fillOpacity: 1 });
                },
                onEachFeature: function (feature, layer) {
                    if (coincide) return;
                    layer.bindTooltip((indice + 1) + ". " + (elemento.ubicacion.etiqueta || elemento.ubicacion.claveElementoRed || "Elemento PAM") + " · GCR " + elemento.gcr, {
                        permanent: resultado.elementos.length <= 4, direction: "top", className: "pam-territorial__tooltip", opacity: .96
                    });
                }
            }).addTo(grupoCoberturas);
        });
        agregarGeo(mapa, lineas, { style: function (feature) { return { color: CIAN, weight: tensionKv(feature) >= 400 ? 2.6 : 1.5, opacity: .82 }; } });
        agregarGeo(mapa, subestaciones, {
            pointToLayer: function (feature, latlng) { return L.circleMarker(latlng, { radius: 2.7, color: "#fff", weight: .7, fillColor: GUINDA, fillOpacity: .9 }); }
        });
        try {
            var bounds = grupoCoberturas.getBounds();
            if (bounds && bounds.isValid()) mapa.fitBounds(bounds.pad(.08), { padding: [12, 12], maxZoom: 9 });
        } catch (e) { }

        // Cuando dos o más elementos comparten coordenada, el punto real queda
        // marcado en su sitio y cada rótulo se abre en columnas laterales. Las
        // líneas guía permiten leer la ficha sin fingir ubicaciones distintas.
        if (posicionesCoincidentes.size) {
            var capaCoincidencias = L.layerGroup().addTo(mapa);
            resultado.elementos.forEach(function (elemento, indice) {
                var coincidencia = posicionesCoincidentes.get(indice);
                var coordenadas = coordenadaPunto(elemento.feature);
                if (!coincidencia || !coordenadas) return;

                var ancla = L.latLng(coordenadas[1], coordenadas[0]);
                var puntoAncla = mapa.latLngToLayerPoint(ancla);
                var ladoDerecho = coincidencia.posicion % 2 === 1;
                var fila = Math.floor(coincidencia.posicion / 2);
                var filasDelLado = Math.ceil((coincidencia.total - (ladoDerecho ? 1 : 0)) / 2);
                var desplazamientoY = (fila - (filasDelLado - 1) / 2) * 34;
                var desplazamientoX = ladoDerecho ? 42 : -42;
                var puntoRotulo = L.point(puntoAncla.x + desplazamientoX, puntoAncla.y + desplazamientoY);
                var posicionRotulo = mapa.layerPointToLatLng(puntoRotulo);

                L.polyline([ancla, posicionRotulo], {
                    color: GUINDA, weight: 1.1, opacity: .62, dashArray: "3 3", interactive: false
                }).addTo(capaCoincidencias);

                L.circleMarker(posicionRotulo, {
                    radius: 6, color: "#fff", weight: 2, fillColor: GUINDA, fillOpacity: 1
                }).bindTooltip(
                    (indice + 1) + ". " + (elemento.ubicacion.etiqueta || elemento.ubicacion.claveElementoRed || "Elemento PAM") + " · GCR " + elemento.gcr,
                    {
                        permanent: true,
                        direction: ladoDerecho ? "right" : "left",
                        className: "pam-territorial__tooltip pam-territorial__tooltip--separado",
                        opacity: .96,
                        offset: ladoDerecho ? [5, 0] : [-5, 0]
                    }
                ).addTo(capaCoincidencias);
            });
        }
        setTimeout(function () { if (mapa) mapa.invalidateSize(false); }, 80);
    }

    // Cada aviso de carga que siga en pantalla muestra el avance real.
    alAvanzar(function (estado) {
        if (!estado.total) return;
        var porcentaje = Math.round(estado.hechas / estado.total * 100);
        document.querySelectorAll(".pam-territorial__loading").forEach(function (aviso) {
            var detalle = aviso.querySelector("span");
            if (!detalle) return;
            if (estado.hechas >= estado.total) {
                detalle.textContent = "Cruzando la cobertura con cada capa…";
                return;
            }
            detalle.textContent = estado.hechas + " de " + estado.total + " capas · " + porcentaje + "%"
                + (estado.ultima ? " · " + estado.ultima : "");
            var barra = aviso.querySelector(".pam-territorial__avance i");
            if (!barra) {
                var pista = document.createElement("div");
                pista.className = "pam-territorial__avance";
                pista.appendChild(document.createElement("i"));
                aviso.appendChild(pista);
                barra = pista.firstChild;
            }
            barra.style.width = porcentaje + "%";
        });
    });

    function crearMapaElemento(resultado, elemento, contenedor, indice) {
        var el = contenedor.querySelector("[data-territorial-element-map]");
        if (!el || typeof L === "undefined" || !window.turf || !el.clientWidth || !el.clientHeight) return;
        if (mapasElemento[indice]) {
            setTimeout(function () { mapasElemento[indice].invalidateSize(false); }, 40);
            return;
        }
        var mapaElemento = L.map(el, {
            zoomControl: false, attributionControl: false, dragging: false, scrollWheelZoom: false,
            doubleClickZoom: false, boxZoom: false, keyboard: false, touchZoom: false, preferCanvas: true
        });
        mapasElemento[indice] = mapaElemento;
        mapaElemento.setView([23.6, -102.5], 5);

        var gcrActiva = elemento.gcr;
        agregarGeo(mapaElemento, featuresGeo(resultado.capas.gerencias.geo), {
            style: function (feature) {
                var activa = codigoGcrFeature(feature) === gcrActiva;
                return { color: activa ? GUINDA : "#d7d1ca", weight: activa ? 1.5 : .45, fillColor: activa ? GUINDA : "#f1ede8", fillOpacity: activa ? .07 : .16 };
            }
        });

        agregarGeo(mapaElemento, elemento.anp.map(function (x) { return x.feature; })
            .concat(elemento.anpEstatal.map(function (x) { return x.feature; }), elemento.ramsar.map(function (x) { return x.feature; })), {
            style: { color: VERDE, weight: 1, fillColor: VERDE, fillOpacity: .1 }
        });
        agregarGeo(mapaElemento, elemento.regionesIndigenas.map(function (x) { return x.feature; }), {
            style: { color: "#B24C6C", weight: 1, dashArray: "4 3", fillColor: "#B24C6C", fillOpacity: .06 }
        });
        agregarGeo(mapaElemento, elemento.ltDentro.map(function (x) { return x.feature; }), {
            style: function (feature) { return { color: CIAN, weight: tensionKv(feature) >= 400 ? 3 : 1.8, opacity: .9 }; }
        });
        agregarGeo(mapaElemento, elemento.seDentro.map(function (x) { return x.feature; }), {
            pointToLayer: function (feature, latlng) { return L.circleMarker(latlng, { radius: 3.4, color: "#fff", weight: 1, fillColor: GUINDA, fillOpacity: .95 }); }
        });
        agregarGeo(mapaElemento, elemento.ductosImportacion.map(function (x) { return x.feature; }), {
            style: { color: "#626B75", weight: 2.2, dashArray: "8 4", opacity: .9 }
        });
        agregarGeo(mapaElemento, elemento.ductosSistrangas.map(function (x) { return x.feature; }), {
            style: { color: "#2E6FB0", weight: 2.4, opacity: .9 }
        });

        var coloresPermiso = { electricidad: GUINDA, gasNatural: "#2E6FB0", gasLp: DORADO, petroliferos: "#C0552E" };
        Object.keys(elemento.permisos).forEach(function (clave) {
            agregarGeo(mapaElemento, elemento.permisos[clave].map(function (x) { return x.feature; }), {
                pointToLayer: function (feature, latlng) {
                    return L.circleMarker(latlng, { radius: 4, color: "#fff", weight: 1.2, fillColor: coloresPermiso[clave], fillOpacity: .95 });
                }
            });
        });

        var grupoPrincipal = L.featureGroup().addTo(mapaElemento);
        // La cobertura es el area de busqueda, no el proyecto. Iba con el mismo
        // peso que la ubicacion y ocupando todo el encuadre, asi que se leia
        // como si el circulo fuera el elemento: ahora queda claramente detras
        // -mas tenue, mas fina- y la ubicacion gana un halo que la destaca.
        if (elemento.cobertura) {
            L.geoJSON(elemento.cobertura, {
                style: { color: DORADO, weight: 1.2, dashArray: "5 5", fillColor: DORADO, fillOpacity: .045, interactive: false }
            }).addTo(grupoPrincipal);
        }
        L.geoJSON(elemento.feature, {
            style: { color: GUINDA, weight: 4.5, fillColor: GUINDA, fillOpacity: .22 },
            pointToLayer: function (feature, latlng) {
                var halo = L.circleMarker(latlng, { radius: 13, color: GUINDA, weight: 1, opacity: .35, fillColor: GUINDA, fillOpacity: .12, interactive: false });
                halo.addTo(grupoPrincipal);
                return L.circleMarker(latlng, { radius: 7.5, color: "#fff", weight: 2.4, fillColor: GUINDA, fillOpacity: 1 });
            }
        }).addTo(grupoPrincipal);

        // Sin leyenda las dos figuras se confunden: una dice donde esta el
        // elemento y la otra hasta donde se busco a su alrededor.
        var leyenda = L.control({ position: "bottomleft" });
        leyenda.onAdd = function () {
            var caja = L.DomUtil.create("div", "pam-elemento__leyenda");
            caja.innerHTML =
                '<span><i class="es-ubicacion"></i>Ubicación del elemento</span>' +
                '<span><i class="es-cobertura"></i>Área de análisis · ' + numero(elemento.radioKm, elemento.radioKm % 1 ? 1 : 0) + ' km</span>';
            return caja;
        };
        leyenda.addTo(mapaElemento);
        try {
            var bounds = grupoPrincipal.getBounds();
            if (bounds && bounds.isValid()) mapaElemento.fitBounds(bounds.pad(.08), { padding: [12, 12], maxZoom: 10 });
        } catch (e) { }
        setTimeout(function () { mapaElemento.invalidateSize(false); }, 80);
    }

    function obtenerConfiguracion() {
        var principal = document.querySelector("[data-pam-territorial-analysis]");
        if (!principal) return null;
        return {
            principal: principal,
            url: principal.dataset.territorialUrl,
            radio: Number(principal.dataset.defaultRadius) || 20,
            gcr: principal.dataset.projectGcr || ""
        };
    }

    function activar() {
        var configuracion = obtenerConfiguracion();
        if (!configuracion || !window.turf) return Promise.resolve();
        var promesa = analizar(configuracion.url, configuracion.radio, configuracion.gcr);
        return promesa.then(function (resultado) {
            var resumen = configuracion.principal.querySelector(".pam-territorial__resumen");
            var status = configuracion.principal.querySelector("[data-territorial-status]");
            if (resumen && !resumen.dataset.rendered) {
                renderResumen(resultado, resumen, status);
                resumen.dataset.rendered = "true";
            }
            var metodo = configuracion.principal.querySelector("[data-territorial-method]");
            if (metodo && resultado.esReferenciaRegional) {
                metodo.textContent = "Cobertura regional GCR; proximidad pendiente de una ubicación de red confirmada.";
            }
            renderComparativas(resultado);
            renderElementosDetallados(resultado);
            renderMatricesTerritoriales(resultado);
            var ejecutivo = document.querySelector(".pam-slide.is-active [data-pam-territorial-executive]");
            var renderEjecutivoPromise = ejecutivo ? renderEjecutivo(resultado, ejecutivo) : Promise.resolve();
            crearMapa(resultado);
            if (mapa) setTimeout(function () { mapa.invalidateSize(false); }, 60);
            return renderEjecutivoPromise.then(function () { return resultado; });
        }).catch(function (error) {
            console.error("No fue posible completar el análisis territorial de la ficha", error);
            var resumen = configuracion.principal.querySelector(".pam-territorial__resumen");
            if (resumen) resumen.innerHTML = '<div class="pam-territorial__empty"><strong>Contexto territorial no disponible</strong><span>No fue posible consultar las capas territoriales en esta sesión.</span></div>';
        });
    }

    document.addEventListener("pam:slide-shown", function () {
        var activa = document.querySelector(".pam-slide.is-active");
        if (activa && activa.querySelector("[data-pam-territorial-analysis], [data-pam-territorial-comparison], [data-pam-territorial-element], [data-pam-territorial-matrix], [data-pam-territorial-executive]")) {
            requestAnimationFrame(function () { requestAnimationFrame(activar); });
        }
    });

    window.pamFichaTerritorial = activar;

    function iniciarSiVisible() {
        var activa = document.querySelector(".pam-slide.is-active");
        if (activa && activa.querySelector("[data-pam-territorial-analysis], [data-pam-territorial-comparison], [data-pam-territorial-element], [data-pam-territorial-matrix], [data-pam-territorial-executive]")) activar();
    }
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", iniciarSiVisible);
    else iniciarSiVisible();
})();
