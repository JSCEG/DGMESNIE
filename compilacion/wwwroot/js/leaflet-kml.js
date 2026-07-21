/*
 * Leaflet KML plugin (minified, v0.1.0, https://github.com/shramov/leaflet-plugins)
 * Only for quick KML support. For production, consider using leaflet-omnivore or similar.
 */
(function () {
    L.KML = L.FeatureGroup.extend({
        initialize: function (kml, options) {
            L.FeatureGroup.prototype.initialize.call(this, []);
            if (typeof kml === 'string') {
                var parser = new DOMParser();
                kml = parser.parseFromString(kml, 'text/xml');
            }
            if (kml) {
                this._parseKML(kml);
            }
        },
        _parseKML: function (xml) {
            var layers = L.KML.parseKML(xml);
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
    L.KML.parseKML = function (xml) {
        var layers = [];
        var placemarks = xml.getElementsByTagName('Placemark');
        for (var i = 0; i < placemarks.length; i++) {
            var pl = placemarks[i];
            var name = pl.getElementsByTagName('name')[0];
            var desc = pl.getElementsByTagName('description')[0];
            var point = pl.getElementsByTagName('Point');
            var polygon = pl.getElementsByTagName('Polygon');
            var line = pl.getElementsByTagName('LineString');
            if (point.length) {
                var coords = point[0].getElementsByTagName('coordinates')[0].textContent.trim().split(',');
                var latlng = new L.LatLng(parseFloat(coords[1]), parseFloat(coords[0]));
                var marker = L.marker(latlng);
                if (name || desc) {
                    var popup = '';
                    if (name) popup += '<b>' + name.textContent + '</b><br>';
                    if (desc) popup += desc.textContent;
                    marker.bindPopup(popup);
                }
                layers.push(marker);
            }
            if (polygon.length) {
                var coords = polygon[0].getElementsByTagName('coordinates')[0].textContent.trim().split(/\s+/).map(function (c) {
                    var xy = c.split(',');
                    return [parseFloat(xy[1]), parseFloat(xy[0])];
                });
                var poly = L.polygon(coords);
                if (name || desc) {
                    var popup = '';
                    if (name) popup += '<b>' + name.textContent + '</b><br>';
                    if (desc) popup += desc.textContent;
                    poly.bindPopup(popup);
                }
                layers.push(poly);
            }
            if (line.length) {
                var coords = line[0].getElementsByTagName('coordinates')[0].textContent.trim().split(/\s+/).map(function (c) {
                    var xy = c.split(',');
                    return [parseFloat(xy[1]), parseFloat(xy[0])];
                });
                var polyline = L.polyline(coords);
                if (name || desc) {
                    var popup = '';
                    if (name) popup += '<b>' + name.textContent + '</b><br>';
                    if (desc) popup += desc.textContent;
                    polyline.bindPopup(popup);
                }
                layers.push(polyline);
            }
        }
        return layers;
    };
})();
