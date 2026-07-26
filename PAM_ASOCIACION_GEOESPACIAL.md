# Reglas y trazabilidad de asociación geoespacial PAM–Red Eléctrica

**Versión de reglas:** `PAM-RED-v1.0`
**Fecha:** 26 de julio de 2026
**Módulo:** `/DashboardProyectos/Index`
**Responsable funcional:** DGMESNIE

## 1. Objetivo

Relacionar los proyectos vigentes del PAM/PAMRNT con subestaciones y líneas
de transmisión georreferenciadas sin inventar coordenadas.

La salida del proceso es una **asociación sugerida y trazable**. Una
asociación automática no sustituye la validación documental. Cuando no existe
una coincidencia de confianza alta, el análisis conserva como respaldo la
geometría de la GCR del proyecto.

## 2. Fuentes

| Catálogo | Geometría | Campos utilizados | Fuente |
|---|---|---|---|
| Subestaciones | `Point` | `name`, `fase`, `voltaje_kv`, coordenadas | `https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson` |
| Líneas de transmisión | `MultiLineString` | `nombre_lt`, `caracteris`, `voltaje_KV`, coordenadas | `https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_lt.geojson` |
| Gerencias de Control Regional | `MultiPolygon` | `Field1`, geometría | `https://cdn.sassoapps.com/Mapas/gerencias_javs_2.geojson` |
| Proyectos PAM | Registro tabular | clave, nombre, GCR/GRT, zona atendida y elementos/equipos asociados | `dgmesnie.vw_PAMProyectoVigente` |

Las URL, umbrales y tolerancias están configurados en
`appsettings.json`, sección `PamTerritorial:AsociacionRed`.

## 3. Normalización

Antes de comparar:

1. Convertir texto a mayúsculas.
2. Eliminar acentos.
3. Convertir puntuación y guiones a espacios comparables.
4. Colapsar espacios repetidos.
5. Eliminar prefijos equivalentes:
   - Subestación: `SE`, `S.E.`, `SUBESTACIÓN`, `SUBESTACIÓN ELÉCTRICA`.
   - Línea: `LT`, `L.T.`, `LÍNEA`, `LÍNEA DE TRANSMISIÓN`.
6. Comparar palabras completas; no usar coincidencias parciales dentro de
   otra palabra.
7. Resolver aliases de GCR:
   - `BC` → Baja California.
   - `BS` → Baja California Sur.
   - `NO` → Noroeste.
   - `NE` → Noreste.
   - `NT` → Norte.
   - `PE` → Peninsular.
   - `OR` y `SE` → Oriental.
   - `OC` → Occidental.
   - `CE` y `VM` → Central.

## 4. Reglas para subestaciones

| Evidencia | Puntaje |
|---|---:|
| Nombre de la subestación presente como palabras completas | +45 |
| Mención explícita en una línea que inicia con `SE` | +15 |
| Nombre presente en el título del proyecto | +10 |
| Punto contenido en la GCR del proyecto | +15 |
| Tensión del catálogo mencionada en el PAM | +10 |
| Nombre único dentro de la GCR | +10 |
| Conectividad confirmada con el extremo de una LT a máximo 3 km | +15 |
| Geometría fuera de la GCR | −35 |
| Tensión incompatible | −15 |
| Nombre homónimo dentro del ámbito | −15 |
| Nombre corto de una sola palabra, menor de siete caracteres | −10 |
| Nombre contenido en otra coincidencia más específica | −20 |

Ejemplo de especificidad: si aparecen `SAN LORENZO` y
`SAN LORENZO POTENCIA`, la segunda coincidencia tiene prioridad cuando su
puntaje es igual o mayor.

## 5. Reglas para líneas de transmisión

| Evidencia | Puntaje |
|---|---:|
| Nombre completo de la línea presente en el PAM | +40 |
| Coinciden ambos extremos, sin importar el sentido A–B o B–A | +30 |
| Coincide solamente un extremo | +10 |
| Mención explícita en una línea que inicia con `LT` | +10 |
| Nombre presente en el título del proyecto | +5 |
| Algún tramo de la geometría está dentro de la GCR | +15 |
| Tensión coincidente | +10 |
| Número de circuitos coincidente | +5 |
| Longitud compatible | +5 |
| Geometría fuera de la GCR | −35 |
| Tensión incompatible | −15 |
| Nombre de línea repetido dentro del ámbito | −10 |

La longitud es compatible cuando la diferencia es menor o igual al mayor
valor entre:

- 3 km; o
- 15 % de la longitud de la línea catalogada.

## 6. Validación topológica

Las subestaciones son nodos y las líneas son conexiones.

Cuando una LT obtiene al menos el umbral de revisión:

1. Se extraen sus extremos nominales.
2. Se buscan las subestaciones con esos nombres.
3. Se calcula la distancia entre cada subestación y los extremos geométricos
   de la línea.
4. Si la distancia es menor o igual a
   `ToleranciaExtremoKm` —3 km por defecto— se agrega la evidencia de
   conectividad.

No se reemplaza una línea por su centroide. Se conserva la geometría
`LineString` o `MultiLineString` completa.

## 7. Clasificación

| Puntaje final | Clasificación | Uso |
|---:|---|---|
| 90–100 | `alta` | Puede mostrarse en el mapa como asociación sugerida |
| 70–89 | `revision` | Sólo detalle y revisión; no define el territorio |
| Menor de 70 | descartada | No se devuelve como candidata |

Una asociación `alta` continúa mostrando la leyenda
**“pendiente de validación”**. Sólo una ubicación aprobada en
`dgmesnie.PAMProyectoUbicacion` tiene `Validada = 1`.

## 8. Geometría utilizada en el análisis

- Una subestación: `Point`, radio sugerido de 20 km.
- Varias subestaciones: conjunto de puntos, buffer sugerido de 10 km.
- Una línea: geometría completa, corredor sugerido de 10 km.
- Varias líneas y subestaciones: `FeatureCollection`.
- Sin asociación de confianza alta: polígono de la GCR, buffer 0.

## 9. Trazabilidad expuesta por la API

Cada asociación devuelve:

- `tipoElementoRed`;
- `claveElementoRed`, hash estable de tipo, nombre, tensión y geometría;
- `nombreElementoRed`;
- tensión, circuitos y longitud disponibles;
- GCR del catálogo;
- puntaje;
- nivel de confianza;
- lista de evidencias y penalizaciones;
- geometría completa;
- URL de la fuente;
- versión de reglas `PAM-RED-v1.0`.

Rutas:

- Detalle territorial:
  `GET /DashboardProyectos/PamTerritorial/{proyectoId}`
- Explicación completa de candidatos:
  `GET /DashboardProyectos/PamTerritorial/{proyectoId}/AsociacionesRed`
- Capa de ubicaciones validadas y asociaciones altas:
  `GET /DashboardProyectos/PamTerritorial/GeoJson`

## 10. Reglas de publicación

1. Una coordenada o geometría manual validada tiene prioridad sobre cualquier
   asociación automática.
2. Si el proyecto tiene ubicación validada, no se publica una asociación
   automática alternativa.
3. El fallo temporal de un GeoJSON no bloquea el catálogo PAM: se conserva el
   respaldo por GCR.
4. Nunca se transforma una coincidencia de texto en `Validada = 1`.
5. La revisión debe conservar usuario, fecha y observaciones en
   `dgmesnie.PAMProyectoUbicacion`.

## 11. Limitaciones conocidas de v1.0

- Los GeoJSON no contienen un identificador institucional compartido con PAM.
- El catálogo de subestaciones no incluye capacidad MVA, bancos ni
  alimentadores; esos campos se conservan como evidencia documental futura.
- Los nombres muy genéricos requieren revisión.
- Los diagramas unifilares y mapas del expediente todavía no participan en el
  puntaje automático.
- La asociación por alias históricos requerirá un catálogo curado adicional.

## 12. Control de cambios

| Versión | Fecha | Cambio |
|---|---|---|
| `PAM-RED-v1.0` | 2026-07-26 | Nombre, GCR, tensión, circuitos, longitud, homónimos, especificidad y conectividad línea–subestación |
