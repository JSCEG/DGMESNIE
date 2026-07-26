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

## 13. Grafo eléctrico navegable

La versión `RED-GRAFO-v1.0` materializa la red como nodos y aristas para
consultar conexiones sin alterar la clasificación `PAM-RED-v1.0`.

### Nodos

- Cada subestación catalogada es un nodo real.
- Cada extremo de línea que no puede resolverse de forma confiable se conserva
  como nodo virtual.
- Dos nodos virtuales sólo se fusionan si su tensión es compatible y están a
  no más de 100 metros.
- El nodo conserva identificador estable, nombre, tensión, coordenadas, grado,
  componente conexa, fuente y condición real/virtual.

### Aristas

- Cada tramo `LineString` es una arista y conserva su geometría completa.
- Para cada extremo se evalúan tanto el orden nominal como el orden inverso.
- La resolución usa nombre del extremo, unicidad, distancia geométrica y
  compatibilidad de tensión.
- Una subestación única a menos de 150 metros de un extremo real aporta
  evidencia geométrica adicional.
- El cruce visual de una línea por otra línea o por un punto intermedio no
  crea conectividad.

Estados de una arista:

| Estado | Descripción |
|---|---|
| `conectada` | Ambos extremos fueron resueltos a nodos reales |
| `parcial` | Sólo uno de los extremos fue resuelto |
| `sin_resolver` | Ambos extremos permanecen como nodos virtuales |

Las coincidencias ambiguas o por debajo del umbral alto se guardan en la cola
de revisión. No se convierten en ubicación oficial ni en conexión validada.

### Persistencia

La reconstrucción se versiona en:

- `dgmesnie.RedElectricaVersion`;
- `dgmesnie.RedElectricaNodo`;
- `dgmesnie.RedElectricaArista`;
- `dgmesnie.RedElectricaRevision`.

El identificador de versión combina las reglas y las huellas de los catálogos.
Sólo una versión permanece activa. Si no hay una versión persistida o no puede
leerse, el servicio puede reconstruir el grafo desde los GeoJSON.

Los parámetros se pueden sobrescribir en
`PamTerritorial:GrafoRedElectrica`; sus valores predeterminados son:

- radio de búsqueda: 5 km;
- tolerancia de extremo: 3 km;
- fusión virtual: 0.1 km;
- confianza alta: 85 puntos;
- revisión: 70 puntos;
- margen de ambigüedad: 10 puntos.

### API

- Resumen: `GET /DashboardProyectos/RedElectrica/Grafo/Resumen`
- Grafo JSON: `GET /DashboardProyectos/RedElectrica/Grafo/Json`
- Nodos buscables: `GET /DashboardProyectos/RedElectrica/Grafo/Nodos`
- Vecindad: `GET /DashboardProyectos/RedElectrica/Grafo/Nodos/{nodeId}/Vecinos?depth=1`
- Ruta mínima: `GET /DashboardProyectos/RedElectrica/Grafo/Ruta?origin={nodeId}&destination={nodeId}`
- Revisiones: `GET /DashboardProyectos/RedElectrica/Grafo/Revisiones`
- Capa GeoJSON: `GET /DashboardProyectos/RedElectrica/Grafo/GeoJson`
- Subgrafo PAM: `GET /DashboardProyectos/RedElectrica/Grafo/Pam/{proyectoId}?depth=1`
- Reconstrucción administrativa:
  `POST /DashboardProyectos/RedElectrica/Grafo/Reconstruir?persist=true`

El subgrafo PAM toma únicamente las asociaciones `alta` como semillas y agrega
las subestaciones y líneas vecinas hasta la profundidad solicitada. Esto
permite listar las conexiones alrededor de un PAM sin presentar la sugerencia
automática como una ubicación oficialmente validada.

## 14. Control de cambios del grafo

| Versión | Fecha | Cambio |
|---|---|---|
| `RED-GRAFO-v1.0` | 2026-07-26 | Nodos, aristas, adyacencia, componentes, ruta mínima, revisiones y subgrafo PAM |

## 15. Representación del catálogo PAM completo

La capa del dashboard distingue el proyecto del elemento geográfico que lo
representa. El corte del 26 de julio de 2026 contiene:

- 281 proyectos PAM/PAMRNT en el catálogo;
- 133 proyectos con al menos una asociación de red de confianza alta;
- 148 proyectos sin geometría precisa suficiente, representados por su GCR.

Para los 148 proyectos regionales se genera únicamente en el navegador un
marcador de referencia en el centroide de la GCR. Ese marcador:

1. permite localizar, consultar y abrir la ficha del proyecto;
2. muestra la leyenda `Referencia regional GCR · no es coordenada oficial`;
3. resalta el polígono completo de la GCR al seleccionarlo;
4. ejecuta el análisis con la geometría GCR y buffer 0;
5. no se persiste en `PAMProyectoUbicacion`;
6. no participa como nodo del grafo eléctrico;
7. nunca se presenta como ubicación validada.

Si el campo GCR contiene varias regiones, por ejemplo `CE / OR / OC`, el
análisis usa la unión de las regiones reconocidas. `Varias` se representa como
cobertura multirregional. Un valor desconocido no se sustituye silenciosamente
por todo el territorio nacional.

Los marcadores de asociación de red y los regionales son clicables. El panel
de detalle conserva clave, nombre, GCR, tipo, etapa, estatus, zona atendida,
elementos asociados, fuente, corte y enlace a la ficha PAM.

## 16. Convocatorias, municipio y KML como evidencia adicional

La fuente consolidada de proyectos de convocatorias disponible para
enriquecimiento es:

`https://cdn.sassoapps.com/Mapas/Mixtos/mixtos.geojson`

En el corte revisado contiene puntos de proyecto con folio, nombre,
tecnología, estado, subestación o punto de interconexión, tensión y capacidad.
La hoja de origen también contempla municipio y archivo KMZ. Estos datos
pueden ayudar a localizar infraestructura relacionada, pero un proyecto de
generación de una convocatoria no es automáticamente el mismo objeto que una
obra de transmisión del PAM.

### Jerarquía de evidencia

| Nivel | Coincidencia requerida | Resultado permitido |
|---|---|---|
| A | Mismo folio o identificador PAM y KML explícitamente asociado | Crear candidato geométrico de evidencia alta, pendiente de validación |
| B | Misma subestación o línea, GCR, tensión compatible y municipio coherente | Crear candidato para revisión; no validar automáticamente |
| C | Proyecto cercano sin identidad compartida, aunque use una SE próxima | Contexto territorial solamente |
| D | Cercanía visual o cruce aparente de una línea | No crea asociación |

### Procedimiento propuesto

1. Normalizar folios, nombres de subestación, extremos de línea y tensión.
2. Resolver el municipio de cada punto por intersección con el GeoJSON
   municipal; no depender solamente de texto libre.
3. Comparar contra los elementos/equipos asociados del PAM y el grafo
   eléctrico.
4. Calcular distancia al elemento de red y registrar todas las evidencias.
5. Si existe KMZ/KML, conservar la geometría original y su folio de origen.
6. Enviar las coincidencias A/B a una cola de revisión.
7. Persistir una ubicación oficial sólo después de validación humana.

No se debe usar el centroide regional del icono GCR para calcular cercanía con
convocatorias. Las distancias sólo se calculan a partir de coordenadas,
geometrías KML/KMZ o elementos de red identificados.

## 17. Control de cambios de representación y enriquecimiento

| Versión | Fecha | Cambio |
|---|---|---|
| `PAM-MAPA-v1.1` | 2026-07-26 | Catálogo completo, icono regional clicable, detalle y análisis por GCR |
| `PAM-EVIDENCIA-v1.0` | 2026-07-26 | Reglas de apoyo con convocatorias, municipio, interconexión y KML/KMZ |
