/*
 * Leaflet KML plugin (minified, v0.1.0, https://github.com/shramov/leaflet-plugins)
 * Only for quick KML support. For production, consider using leaflet-omnivore or similar.
 */
(function () {
    L.KML = L.FeatureGroup.extend({
        initialize: function (kml, options) {
            L.FeatureGroup.prototype.initialize.call(this, []);
            L.setOptions(this, options);
            if (typeof kml === 'string') {
                var parser = new DOMParser();
                kml = parser.parseFromString(kml, 'text/xml');
            }
            if (kml) {
                this._parseKML(kml);
            }
        },
        _parseKML: function (xml) {
            var layers = L.KML.parseKML(xml, this.options);
            this._vertexPointCount = layers.vertexPointCount || 0;
            this._vertexPointsSuppressed = layers.vertexPointsSuppressed || 0;
            this._vertexGeometryReconstructed = !!layers.vertexGeometryReconstructed;
            for (var i = 0; i < layers.length; i++) {
                this.addLayer(layers[i]);
            }
        },
        getBounds: function () {
            var bounds = new L.LatLngBounds();
            this.eachLayer(function (layer) {
                if (layer.getBounds) {
                    bounds.extend(layer.getBounds());
                } else if (layer.getLatLng) {
                    bounds.extend(layer.getLatLng());
                }
            });
            return bounds;
        }
    });
    L.KML.parseKML = function (xml, options) {
        options = options || {};
        var layers = [];
        var placemarks = xml.getElementsByTagName('Placemark');
        var pointCount = 0;
        var hasLinearGeometry = false;
        var vertexLatLngs = [];
        var threshold = Number(options.vertexPointThreshold);

        for (var p = 0; p < placemarks.length; p++) {
            pointCount += placemarks[p].getElementsByTagName('Point').length;
            if (placemarks[p].getElementsByTagName('Polygon').length ||
                placemarks[p].getElementsByTagName('LineString').length) {
                hasLinearGeometry = true;
            }
        }

        // Los KML de la ventanilla suelen traer un Placemark por vértice. Tres o más puntos se
        // tratan como contorno: se dibuja el polígono y no un marcador por vértice. Uno o dos
        // puntos siguen siendo marcadores (coordenada principal / referencia).
        var minVertexPoints = Number.isFinite(Number(options.minVertexPoints)) ? Number(options.minVertexPoints) : 3;
        var suppressVertexMarkers = pointCount >= minVertexPoints ||
            (Number.isFinite(threshold) && threshold >= 0 && pointCount > threshold);

        function readPoint(element) {
            var coordinates = element.getElementsByTagName('coordinates')[0];
            if (!coordinates) return null;
            var parts = coordinates.textContent.trim().split(',');
            var latitude = parseFloat(parts[1]);
            var longitude = parseFloat(parts[0]);
            return Number.isFinite(latitude) && Number.isFinite(longitude)
                ? new L.LatLng(latitude, longitude)
                : null;
        }

        function bindPopup(layer, name, desc) {
            if (!name && !desc) return;
            var popup = '';
            if (name) popup += '<b>' + name.textContent + '</b><br>';
            if (desc) popup += desc.textContent;
            layer.bindPopup(popup);
        }

        for (var i = 0; i < placemarks.length; i++) {
            var pl = placemarks[i];
            var name = pl.getElementsByTagName('name')[0];
            var desc = pl.getElementsByTagName('description')[0];
            var point = pl.getElementsByTagName('Point');
            var polygon = pl.getElementsByTagName('Polygon');
            var line = pl.getElementsByTagName('LineString');
            if (point.length) {
                for (var j = 0; j < point.length; j++) {
                    var latlng = readPoint(point[j]);
                    if (!latlng) continue;
                    if (suppressVertexMarkers) {
                        vertexLatLngs.push(latlng);
                    } else {
                        var marker = L.marker(latlng);
                        bindPopup(marker, name, desc);
                        layers.push(marker);
                    }
                }
            }
            if (polygon.length) {
                var coords = polygon[0].getElementsByTagName('coordinates')[0].textContent.trim().split(/\s+/).map(function (c) {
                    var xy = c.split(',');
                    return [parseFloat(xy[1]), parseFloat(xy[0])];
                });
                var poly = L.polygon(coords);
                bindPopup(poly, name, desc);
                layers.push(poly);
            }
            if (line.length) {
                var coords = line[0].getElementsByTagName('coordinates')[0].textContent.trim().split(/\s+/).map(function (c) {
                    var xy = c.split(',');
                    return [parseFloat(xy[1]), parseFloat(xy[0])];
                });
                var polyline = L.polyline(coords);
                bindPopup(polyline, name, desc);
                layers.push(polyline);
            }
        }

        if (suppressVertexMarkers && !hasLinearGeometry && vertexLatLngs.length > 1) {
            // Tres o más vértices describen un predio: se cierra como polígono aunque el último
            // punto no repita el primero. Dos puntos sólo permiten un trazo.
            layers.push(vertexLatLngs.length >= 3 ? L.polygon(vertexLatLngs) : L.polyline(vertexLatLngs));
            layers.vertexGeometryReconstructed = true;
        }

        layers.vertexPointCount = pointCount;
        layers.vertexPointsSuppressed = suppressVertexMarkers ? pointCount : 0;
        return layers;
    };
})();
