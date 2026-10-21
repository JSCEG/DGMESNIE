"""Prueba HTTP de solo lectura del contrato CNE, contra localhost o producción.

python tests/permisos-cne-tablas.test.py https://host
"""
import datetime
import json
import sys
import urllib.error
import urllib.parse
import urllib.request

BASE = sys.argv[1].rstrip('/') + '/DashboardProyectos/PermisosEnergeticos/Tablas/'
EXTRAS = {
    'transporte': 'producto medioTransporte modalidad capacidad unidadCapacidad numeroUnidades trayecto longitudKm cobertura',
    'comercializacion': 'producto modalidad',
    'infraestructura': 'producto capacidad unidadCapacidad zonaGeografica longitudSistemaKm cobertura',
    'suministro': 'modalidad cobertura',
    'terceros': 'ambito',
}
COMMON = set('id numeroPermiso numeroPermisoNorm mercado codigoActividad tipoPermiso razonSocial rfc expediente fechaOtorgamiento inicioVigencia terminoVigencia estatus estatusCne estatusInstalacion efId entidad mpoId municipio calle colonia codigoPostal latitud longitud inversionEstimada comentarios fechaActualizacion ligaPermiso ligaResolucion'.split())


def get(endpoint, params, expected=200):
    url = BASE + endpoint + '?' + urllib.parse.urlencode(params)
    try:
        response = urllib.request.urlopen(url, timeout=120)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        body = response.read()
        assert response.status == expected, (url, response.status, body[:250])
        assert 'application/json' in response.headers.get('Content-Type', ''), url
        return json.loads(body)


def date(value, utc=False):
    if value is None:
        return
    if utc:
        assert value.endswith('Z'), value
        datetime.datetime.fromisoformat(value.replace('Z', '+00:00'))
    else:
        assert len(value) == 10, value
        datetime.date.fromisoformat(value)


for tabla, extras in EXTRAS.items():
    rows = get('Lista', {'tabla': tabla})
    assert rows['tabla'] == tabla and isinstance(rows['filas'], list)
    for row in rows['filas']:
        assert set(row) == COMMON | set(extras.split()), (tabla, set(row))
        for key in ('efId', 'mpoId'):
            assert row[key] is None or row[key] == row[key].strip(), row
        for key in ('fechaOtorgamiento', 'inicioVigencia', 'terminoVigencia'):
            date(row[key])
        date(row['fechaActualizacion'], utc=True)

    states = get('Estados', {'tabla': tabla})
    assert states['tabla'] == tabla and isinstance(states['estados'], list)
    for state in states['estados']:
        assert set(state) == {'efId', 'entidad', 'permisos'}
        assert state['efId'] == state['efId'].strip()
    active = [row for row in rows['filas'] if row['estatus'] == 'Vigente']
    assert sum(state['permisos'] for state in states['estados']) == len(active), tabla

    points = get('Puntos', {'tabla': tabla})
    assert points['tabla'] == tabla and isinstance(points['puntos'], list)
    for point in points['puntos']:
        assert set(point) == {'numeroPermiso', 'razonSocial', 'tipoPermiso', 'latitud', 'longitud'}
        assert isinstance(point['latitud'], (int, float)) and isinstance(point['longitud'], (int, float))
    assert len(points['puntos']) == sum(row['latitud'] is not None and row['longitud'] is not None for row in active)

    updated = get('Actualizacion', {'tabla': tabla})
    assert updated['tabla'] == tabla and isinstance(updated['disponible'], bool)
    if updated['disponible']:
        assert set(updated) == {'tabla', 'disponible', 'fechaActualizacion', 'fechaCorteFuente', 'filas'}
        date(updated['fechaActualizacion'], utc=True)
        date(updated['fechaCorteFuente'])
    else:
        assert set(updated) == {'tabla', 'disponible'}

    filtered = get('Lista', {'tabla': tabla, 'estatus': 'Vigente'})['filas']
    assert len(filtered) == len(active)
    assert all(row['estatus'] == 'Vigente' for row in filtered)
    sample = next((row for row in rows['filas'] if row['efId'] and row['tipoPermiso']), None)
    if sample:
        filtered = get('Lista', {'tabla': tabla, 'ef': sample['efId'], 'tipo': sample['tipoPermiso']})['filas']
        assert filtered and all(row['efId'] == sample['efId'] and row['tipoPermiso'] == sample['tipoPermiso'] for row in filtered)
    print(f"OK {tabla}: {len(rows['filas'])} filas, {len(states['estados'])} estados, {len(points['puntos'])} puntos", flush=True)

for endpoint in ('Lista', 'Estados', 'Puntos', 'Actualizacion'):
    for params in ({'tabla': 'otra'}, {}, {'tabla': "transporte]; SELECT 1--"}, [('tabla', 'transporte'), ('tabla', 'terceros')]):
        get(endpoint, params, expected=400)

for invalid in ({'ef': '9'}, {'ef': '090'}, {'ef': 'ab'}, {'ef': '٠٩'}, {'ef': ''},
                {'estatus': 'todos'}, {'estatus': ''}, {'tipo': ''}, {'tipo': 'x' * 401}):
    get('Lista', {'tabla': 'transporte', **invalid}, expected=400)
get('Lista', [('tabla', 'transporte'), ('ef', '09'), ('ef', '10')], expected=400)
assert get('Lista', {'tabla': 'transporte', 'tipo': "' OR 1=1--"})['filas'] == []
print('OK validación 400, filtros parametrizados y contrato JSON', flush=True)
