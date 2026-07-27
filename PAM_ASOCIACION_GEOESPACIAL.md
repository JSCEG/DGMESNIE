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

## 16. Segunda Convocatoria como evidencia adicional

La fuente correcta para este enriquecimiento es el dataset público
`conv2_part`, **2ª convocatoria · Particulares**, registrado en
`wwwroot/tablero/datasets-catalog.json`. Se compone de:

- hoja base publicada como CSV;
- pestaña pública de trazabilidad/minutas;
- filtro por la última decisión `Continúa`;
- relación de trazabilidad `Pre Folio` ↔ `folio`, eliminando el prefijo
  técnico `PRE-` antes de comparar.

No se mezcla esta evidencia con `GAT Mixto`.

Los campos utilizados son:

- folio y nombre del proyecto;
- subestación eléctrica y punto de interconexión declarados;
- nivel de tensión;
- distancia declarada entre proyecto e interconexión;
- GCR, entidad y municipio;
- vértices y KMZ del proyecto;
- vértices y KMZ de la subestación propia del proyecto de generación.

La última geometría no se interpreta como coordenada de la subestación de la
red. Se usa como origen para contrastar la distancia declarada hacia un nodo
catalogado. Esto evita publicar como infraestructura de CFE/CENACE una
subestación elevadora o colectora perteneciente al proyecto privado.

### Jerarquía de evidencia

| Nivel | Coincidencia requerida | Resultado permitido |
|---|---|---|
| A | Mismo folio o identidad de proyecto y geometría explícita | Crear candidato geométrico de evidencia alta, pendiente de validación |
| B | Misma subestación o línea, GCR y tensión compatibles, con nodo/tramo catalogado | Reforzar el elemento de red como candidato pendiente de validación |
| C | Referencia nominal consistente pero elemento ausente o ambiguo en el catálogo | Ingresar al diagnóstico de cobertura para revisión |
| D | Cercanía visual o cruce aparente de una línea | No crea asociación |

### Procedimiento propuesto

1. Cargar y cachear la hoja base y la trazabilidad.
2. Mantener dos universos explícitos:
   - **universo base completo**, para descubrir subestaciones y líneas que
     todavía no aparecen en los catálogos geoespaciales;
   - **subconjunto vigente**, limitado a folios cuya última decisión sea
     `Continúa`, para cruzar evidencia con proyectos PAM.
3. Normalizar folios, nombres de subestación, referencias de línea y tensión.
4. Comparar la subestación declarada con el catálogo de nodos.
5. Cuando exista distancia declarada, calcularla desde la subestación propia
   del proyecto hacia el nodo candidato y usarla para desambiguar.
6. Comparar referencias explícitas de LT con las aristas del catálogo.
7. Clasificar cada elemento como `catalogada`, `revision`, `faltante` o
   `tramo_sin_geometria`.
8. Comparar las evidencias con folio, nombre y elementos asociados de cada PAM.
9. Mostrar coincidencias altas o de revisión sin cambiar `Validada = 0`.
10. Persistir una ubicación oficial sólo después de validación humana.

No se debe usar el centroide regional del icono GCR para calcular cercanía con
convocatorias. Las distancias sólo se calculan a partir de coordenadas,
geometrías KML/KMZ o elementos de red identificados.

### API y capa de revisión

- Evidencia para un PAM:
  `GET /DashboardProyectos/PamTerritorial/{proyectoId}/EvidenciaConvocatoria`
- Diagnóstico completo:
  `GET /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Resumen`
- Capa GeoJSON de elementos revisables con geometría conocida:
  `GET /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/GeoJson`
- Decisiones humanas vigentes:
  `GET /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Validaciones`
- Registro de una decisión:
  `POST /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Validaciones`
- Confirmación institucional en lote de coincidencias firmes:
  `POST /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Validaciones/ConfirmarCoincidenciasAutomaticas?tipoElemento=subestacion`
- Confirmación institucional separada de líneas firmes:
  `POST /DashboardProyectos/PamTerritorial/EvidenciaConvocatoria/Validaciones/ConfirmarCoincidenciasAutomaticas?tipoElemento=linea_transmision`

La capa aparece como **Vacíos de red · 2ª convocatoria** y está apagada por
defecto. Un elemento faltante sin geometría no se dibuja artificialmente; se
conserva en el resumen para revisión.

El menú de capas incluye una **Mesa de revisión · Segunda Convocatoria**. La
mesa separa universo base, vigentes, otros estatus y folios sin decisión; permite
filtrar subestaciones o líneas, estado de resolución, texto y presencia de
proyectos vigentes. Los resultados se paginan en bloques de 50 y pueden
localizarse en el mapa cuando existe geometría.

La mesa distingue siempre el diagnóstico automático del dictamen humano. Las
decisiones permitidas son:

| Decisión | Significado |
|---|---|
| `confirmada` | La referencia corresponde al elemento de catálogo mostrado |
| `faltante_confirmada` | La fuente es válida, pero el elemento no está representado en el catálogo actual |
| `rechazada` | La referencia no corresponde o no debe usarse |
| `pendiente` | Se registró una revisión sin resolución concluyente |

Cada cambio requiere una observación y conserva candidato, fotografía de la
evidencia, usuario, unidad, fecha y fuente. La nueva decisión deja de marcar
como vigente a la anterior, pero no la elimina; la tabla
`dgmesnie.PamConvocatoriaValidacionRed` conserva el historial completo. El
registro está protegido con sesión, antiforgery y autorización para personal
DGMESNIE o administración.

### Fotografía de arranque

El diagnóstico completo descarga la hoja base, la trazabilidad, las
subestaciones, las líneas y las GCR, y después reconstruye todos los candidatos.
Para evitar que cada reinicio de `dotnet watch` deje la mesa esperando varios
minutos, una ejecución exitosa conserva una fotografía versionada en
`App_Data/cache/pam_convocatoria_coverage_v14.json`.

La apertura normal usa esa fotografía durante un máximo configurable de 24
horas. **Volver a consultar** envía `refrescar=true`, vuelve a leer las fuentes
y reemplaza la fotografía de forma atómica. El archivo es sólo caché
reconstruible: las decisiones institucionales continúan almacenadas en
`dgmesnie.PamConvocatoriaValidacionRed`.

### Automatización conservadora de subestaciones

Para reducir la revisión manual sin convertir una proximidad visual en evidencia
eléctrica, una referencia de catálogo sólo se marca como **automática firme**
cuando:

1. todas las filas del grupo resuelven a una sola clave de catálogo;
2. el nombre coincide de forma exacta o alcanza similitud fuerte `>= 0.90`;
3. no existe conflicto de tensión ni de distancia declarada;
4. existe por lo menos una evidencia independiente compatible:
   tensión o distancia declarada;
5. existe un margen mínimo de 15 puntos sobre el segundo candidato;
6. el nombre es único en el catálogo o queda desambiguado dentro de la GCR.

La GCR se usa para separar homónimos o respaldar una coincidencia fuerte. No
anula por sí sola una coincidencia exacta y globalmente única, porque la región
informada por la convocatoria y la región espacial del catálogo pueden provenir
de cortes distintos.

Un homónimo de catálogo se descarta cuando la GCR es incompatible y su punto
queda a 100 km o más de la geometría declarada. El descarte no convierte el
homónimo en una asociación: si la fuente vigente aporta una referencia
georreferenciada consistente, ésta se conserva como `faltante_confirmada`.

Los grupos con el mismo nombre y tensión se separan por GCR y entidad antes de
resolverlos. El subgrupo principal conserva el identificador histórico y los
subgrupos territoriales adicionales reciben un discriminador estable. Así, dos
referencias llamadas `Moctezuma` en entidades distintas no comparten decisión.

Si la subestación no existe en el catálogo de puntos, sólo puede registrarse
automáticamente como `faltante_confirmada` cuando su nombre coincide exactamente
con un extremo nominal de línea, la GCR y la tensión son compatibles, los
extremos observados forman un grupo de hasta 3 km y existe al menos una línea de
soporte identificable. La geometría resultante es evidencia sugerida
`Point/MultiPoint`; no se incorpora automáticamente al catálogo oficial.

También puede registrarse como `faltante_confirmada` una referencia vigente que
no coincide con el catálogo cuando todas sus filas vigentes incluyen nombre,
tensión, GCR y coordenadas en los campos específicos de la subestación, y los
puntos del grupo no se separan más de 3 km. Este criterio sólo confirma que la
fuente georreferencia una subestación privada o propuesta ausente del catálogo.
No demuestra conectividad, no sustituye la validación del propietario de la red
y no crea ni promueve automáticamente un nodo CFE/CENACE. La geometría queda
como candidata trazable para la etapa posterior de consolidación.

La equivalencia nominal del catálogo elimina únicamente artículos y
calificadores controlados de nomenclatura. Se consideran equivalentes, por
ejemplo, `Presa El Cuchillo`/`Cuchillo`, `El Potosí`/`El Potosí Bcos 1 y 2` y
`Soledad de Doblado`/`Soledad Doblado`, siempre que exista un solo candidato,
el margen sea suficiente y GCR y tensión sean compatibles. Se conservan como
significativos `Solar`, `Eólica`, `Maniobras`, `Potencia`, `Nueva`, numerales
romanos y otros calificadores que puedan identificar instalaciones distintas.

La coordenada de la subestación privada nunca aporta por sí sola una
confirmación por cercanía. Cuando hay distancia declarada, la geometría privada
sirve únicamente como origen para calcular la distancia hacia el nodo
catalogado y contrastar ambas magnitudes.

Cuando la fuente distingue explícitamente una instalación privada o propuesta
de un punto público de interconexión, ambos se modelan por separado. El criterio
automático sólo es firme si:

1. todas las filas vigentes del grupo aportan geometría específica de la
   subestación privada, tensión y GCR;
2. el nombre privado contiene un calificador funcional controlado, como
   `Solar`, `Eólica`, `Fotovoltaica`, `Maniobras`, `Privada`, `Colectora` o
   `Elevadora`;
3. el campo de interconexión declara una subestación con nombre distinto;
4. el nodo público resuelve a una sola clave de catálogo por nombre exacto o
   equivalente, con GCR, tensión y margen compatibles; y
5. no existe conflicto con una distancia declarada.

En ese caso, la instalación privada queda como `faltante_confirmada` y conserva
su geometría de fuente. El nodo público se registra únicamente como referencia
de interconexión independiente, con su propia clave y geometría. La aplicación
puede dibujar una línea discontinua entre ambos para explicar la relación
declarada, pero esa línea no representa el trazo físico de una línea de
transmisión ni demuestra conectividad por proximidad. Por ejemplo,
`SE Tecamachalco Solar` no se homologa con `SE Tecamachalco`; permanece como
instalación privada y su interconexión declarada se resuelve contra
`SE Tecamachalco II`.

La tensión de una subestación se modela como un conjunto de niveles y no como
un valor único. El punto GeoJSON puede representar el nivel de entrada de la
RNT, mientras que las líneas conectadas al mismo nodo nominal y espacial
documentan niveles de salida hacia transmisión regional o distribución. Un
nivel distinto al publicado en el punto sólo deja de considerarse conflicto
cuando existe al menos una línea que:

1. usa exactamente el mismo nombre normalizado de la subestación;
2. termina a no más de 3 km del punto catalogado;
3. pertenece a la misma GCR;
4. tiene la tensión declarada por la convocatoria; y
5. cuando la fuente aporta coordenada de subestación, el punto declarado queda
   a no más de 15 km del nodo catalogado.

La evidencia conserva todos los niveles observados en el punto y en sus líneas.
No se presupone el sentido del flujo ni la relación de transformación; sólo se
reconoce que el mismo nodo geoespacial tiene infraestructura documentada en más
de un nivel de tensión.

Cuando los vértices capturados para la subestación son exactamente los mismos
que los del proyecto, esa geometría se marca como repetida. Puede conservarse
como evidencia de la fuente, pero no se usa como coordenada independiente para
descartar una coincidencia exacta de catálogo respaldada por GCR y por un
extremo de línea compatible.

El orden de los vértices de una línea GeoJSON no se presume igual al orden de
los nombres `origen - destino`. Cuando uno o ambos nombres de extremo existen
en el catálogo de subestaciones, la orientación se resuelve comparando los dos
sentidos y exigiendo que al menos un extremo quede a no más de 3 km de su punto
nominal. Si no existe ese anclaje, se conserva el orden de fuente sin convertir
un cruce visual en conectividad.

El lote conserva el usuario institucional que lo autorizó, la regla aplicada,
la evidencia y el historial. Las coincidencias con homónimos, claves múltiples,
conflictos o evidencia insuficiente permanecen en la mesa de revisión. Una
confirmación de esta mesa tampoco cambia automáticamente
`PAMProyectoUbicacion.Validada`; esa promoción sigue siendo una etapa separada.
La confirmación automática en lote sólo persiste candidatos con al menos un
proyecto cuya última decisión sea `Continúa`; el resto del universo permanece
disponible como evidencia histórica sin generar decisiones automáticas.

Para grupos que mezclan folios vigentes e históricos, la resolución automática
se calcula exclusivamente con las filas cuya última decisión sea `Continúa`.
Las filas históricas permanecen en el universo, los folios y la trazabilidad,
pero no pueden introducir una clave, tensión o geometría que bloquee o altere
el dictamen del subconjunto vigente. Si el grupo no tiene proyectos vigentes,
se conserva la evaluación histórica únicamente como diagnóstico.

Una referencia sin coincidencia de catálogo y sin geometría firme también puede
registrarse como `faltante_confirmada` por **corroboración multifuente** cuando:

1. aparece con el mismo nombre normalizado en al menos tres folios vigentes;
2. esos folios corresponden a por lo menos tres proyectos independientes;
3. todas las filas pertenecen a una sola GCR;
4. todas declaran el mismo nivel de tensión; y
5. ninguna resolución del grupo se promueve a una clave de catálogo.

Este criterio confirma únicamente que varias solicitudes independientes
documentan la misma referencia ausente. Las geometrías ausentes, incompletas o
espacialmente inconsistentes se conservan como evidencia, pero no se publican
como ubicación. El criterio no crea geometría, no localiza la subestación, no
confirma el entronque y no demuestra conectividad eléctrica.

Los anexos técnicos públicos pueden cerrar una discrepancia sólo cuando su
evidencia queda registrada en
`wwwroot/data/pam_convocatoria_evidencia_anexos.json` con folio, nombre
declarado, GCR, tensión, coordenadas, URL pública y SHA-256 del archivo y del
documento revisado. El motor vuelve a comprobar esos campos contra la fila
vigente antes de aplicar la regla.

Hay dos modalidades:

1. `catalogo_multitension`: el anexo identifica el nodo público y su tensión, y
   la geometría técnica queda dentro de la tolerancia del punto de catálogo;
2. `referencia_faltante_distinta`: el anexo distingue por función, tensión y
   coordenadas una subestación de maniobras o propuesta respecto del homónimo
   de catálogo.

La primera modalidad permite homologar el mismo nodo físico aun cuando el
GeoJSON publique sólo uno de sus niveles de tensión. La segunda registra una
`faltante_confirmada`, publica exclusivamente el punto documentado por el anexo
y elimina la clave del homónimo; no crea una conexión eléctrica. Un KMZ que
sólo delimita el parque o una ruta, sin identificar el nodo público, no satisface
esta regla.

El orden obligatorio de consolidación es:

1. subestaciones;
2. líneas de transmisión, usando ya los nodos consolidados;
3. asociaciones y ubicación PAM.

Por ello, la primera fase permite confirmar o rechazar únicamente
subestaciones. Las líneas permanecen visibles en modo lectura hasta cerrar los
nodos. Ninguna decisión de esta bitácora modifica por sí sola el catálogo
oficial, crea geometría o cambia `Validada = 0` en un PAM.

Para la fase de líneas, el GeoJSON de líneas es la fuente geométrica maestra y
la Segunda Convocatoria funciona sólo como evidencia nominal, de tensión y de
proyecto. Cada referencia se contrasta contra la geometría completa y contra
sus dos extremos reales:

1. `conectada`: ambos extremos nominales resuelven por nombre exacto o
   equivalencia controlada a subestaciones catalogadas situadas a no más de
   3 km del extremo real;
2. `parcial`: sólo uno de los dos extremos cumple esa regla;
3. `ambigua`: un extremo tiene más de una subestación homónima prácticamente
   equidistante y no existe margen espacial suficiente;
4. `sin_resolver`: la línea existe en el GeoJSON, pero ninguno de sus extremos
   puede anclarse de forma firme; y
5. `sin_geometria`: la convocatoria menciona el tramo, pero no existe una
   geometría homóloga en el catálogo.

Una línea `conectada` recibe soporte topológico adicional en el puntaje; una
línea `parcial` recibe únicamente soporte débil. La tensión no se usa para
rechazar un extremo, porque una subestación puede recibir una tensión de la RNT
y entregar otra hacia transmisión regional o distribución. La tensión se
conserva como atributo y evidencia. Ni la cercanía al centroide, ni el cruce
visual con otra línea, ni el paso por un punto intermedio crean conectividad.

La homologación nominal de líneas admite el sentido inverso `A–B`/`B–A`,
artículos equivalentes, los calificadores `Pot.`/`Potencia`, códigos insertados
entre los extremos y un único calificador adicional, siempre que tensión y
territorio no estén en conflicto. La firma se construye con tokens
normalizados; no se usa similitud abierta.

Los códigos como `A3190`, `73490` o `93580` no son identificadores nacionales
únicos. Sólo pueden identificar un **corredor base** cuando:

1. el código aparece en la fuente y en las características del GeoJSON;
2. queda un solo nombre de línea compatible por tensión y GCR; y
3. la referencia comparte al menos un token nominal con ese corredor.

`corredor_catalogado` significa que se identificó la línea preexistente sobre
la cual la convocatoria declara un entronque, maniobra o entrada/salida. Su
geometría sirve como contexto, pero no se presenta como la geometría del nuevo
tramo. Si el código se repite, carece de anclaje nominal o entra en conflicto,
la referencia queda en `revision`, sin seleccionar una geometría arbitraria.

El lote automático de líneas se ejecuta como fase separada y sólo admite una
referencia de un proyecto vigente cuando:

1. resuelve a una clave de línea del catálogo;
2. su estado es `catalogada`, nunca `corredor_catalogado`;
3. el grafo resuelve sus dos extremos como nodos reales;
4. el par de extremos es firme, o el nombre de catálogo alcanza 90 puntos y la
   conectividad de ambos extremos es firme.

Las líneas parciales, ambiguas, sin resolver, sin geometría y los corredores
base permanecen en revisión. La confirmación registra la homologación
documental; no afirma flujo, capacidad disponible ni condición operativa.

La capa principal **Proyectos PAM / PAMRNT** no espera la descarga de Google
Sheets. Carga primero el catálogo PAM y las asociaciones del catálogo eléctrico;
la evidencia de Segunda Convocatoria se consulta de forma independiente al abrir
el detalle o al encender su capa de revisión. Así, una demora o falla de la hoja
pública no bloquea los iconos ni su interacción.

## 17. Control de cambios de representación y enriquecimiento

| Versión | Fecha | Cambio |
|---|---|---|
| `PAM-MAPA-v1.1` | 2026-07-26 | Catálogo completo, icono regional clicable, detalle y análisis por GCR |
| `PAM-EVIDENCIA-v1.0` | 2026-07-26 | Reglas de apoyo con convocatorias, municipio, interconexión y KML/KMZ |
| `PAM-CONV2-v1.0` | 2026-07-26 | Segunda Convocatoria en línea, filtro por última decisión, cruce PAM y diagnóstico de vacíos de red |
| `PAM-MAPA-v1.2` | 2026-07-26 | Pane interactivo propio, limpieza completa y carga PAM desacoplada de Google Sheets |
| `PAM-CONV2-v1.1` | 2026-07-26 | Universo base completo para diagnóstico, subconjunto Continúa para PAM y mesa paginada de revisión |
| `PAM-CONV2-v1.2` | 2026-07-26 | Bitácora persistente de dictamen humano, autorización DGMESNIE y validación por fases: subestaciones, líneas y PAM |
| `PAM-CONV2-v1.3` | 2026-07-26 | Segunda pasada conservadora: GCR para desambiguar, similitud fuerte con margen y extremos nominales de línea para faltantes trazables |
| `PAM-CONV2-v1.4` | 2026-07-26 | Tercera pasada conservadora: subestaciones privadas/propuestas georreferenciadas por la fuente vigente como faltantes confirmadas, sin inferir conectividad ni promoverlas al catálogo |
| `PAM-CONV2-v1.5` | 2026-07-26 | Equivalencias de nomenclatura controladas para artículos, presa y bancos; conserva calificadores funcionales y numerales para evitar homologaciones incorrectas |
| `PAM-CONV2-v1.6` | 2026-07-27 | Separa la subestación privada/propuesta del nodo público de interconexión, conserva ambas geometrías y evita homologar por contención instalaciones distintas |
| `PAM-CONV2-v1.7` | 2026-07-27 | Modela múltiples niveles de tensión por nodo usando el punto y las líneas conectadas; resuelve discrepancias sólo con nombre, GCR, extremo espacial y distancia fuente compatibles |
| `PAM-CONV2-v1.8` | 2026-07-27 | Descarta homónimos territoriales extremos, separa grupos por GCR y entidad, prioriza evidencia topológica trazable y limita el lote automático a proyectos vigentes |
| `PAM-CONV2-v1.9` | 2026-07-27 | Confirma como faltantes únicamente referencias sin geometría corroboradas por tres o más proyectos vigentes independientes con GCR y tensión consistentes |
| `PAM-CONV2-v1.10` | 2026-07-27 | Separa el universo histórico de las filas vigentes al resolver cada grupo; conserva toda la trazabilidad pero sólo `Continúa` participa en decisiones automáticas |
| `PAM-CONV2-v1.11` | 2026-07-27 | Incorpora anexos técnicos públicos con URL y SHA-256: confirma nodos multivoltaje sólo con coincidencia geométrica y conserva como faltantes distintas las subestaciones de maniobras documentadas |
| `PAM-CONV2-v1.12` | 2026-07-27 | Consolida referencias de línea contra su GeoJSON completo y clasifica la conectividad de sus extremos como conectada, parcial, ambigua, sin resolver o sin geometría; la Segunda Convocatoria permanece como evidencia |
| `PAM-CONV2-v1.13` | 2026-07-27 | Homologa líneas por pares de extremos en ambos sentidos y separa los corredores base identificados por código de los nuevos entronques; los códigos repetidos permanecen sin geometría y en revisión |
| `PAM-CONV2-v1.14` | 2026-07-27 | Resuelve extremos con equivalencias nominales controladas únicamente sobre el extremo físico de la línea: corrige una errata o un nombre extendido a menos de 250 m, conserva empates y asociaciones sin respaldo como pendientes y no infiere conectividad por cruces visuales |
