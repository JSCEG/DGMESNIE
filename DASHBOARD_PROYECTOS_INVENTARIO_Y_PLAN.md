# Dashboard territorial: inventario de capas y plan de visualización

Fecha de corte del documento: 29 de julio de 2026.

## Principio de diseño

El dashboard tomará como referencia la interacción del Atlas SEN de Batu Energy:

- mapa como espacio principal;
- capas y filtros en un panel compacto;
- gráfica contextual en una bandeja inferior;
- selección de una región o elemento para actualizar mapa, ficha y gráfica;
- línea de tiempo cuando la fuente tenga serie histórica.

La implementación DGMESNIE añade una distinción obligatoria entre:

1. inventario institucional;
2. fuente pública automatizada;
3. referencia secundaria;
4. planeación;
5. evidencia pendiente de validación.

Una central, un proyecto planeado y un permiso no son equivalentes y no se
fusionan en una sola capa.

## Corte técnico de la referencia Atlas SEN

La conexión automatizada disponible al 29 de julio de 2026 reconoce:

| Dataset de referencia | Registros | Tratamiento DGMESNIE |
|---|---:|---|
| Subestaciones de transmisión Atlas | 471 | Contraste con el inventario institucional; no sustituye la BD |
| Subestaciones de distribución OSM | 2,037 | Referencia secundaria con licencia ODbL |
| Trazos de transmisión Atlas | 8,715 | Contraste geométrico y de conectividad |
| Demanda CENACE | 10 series | 9 regiones cartográficas más el agregado SIN |
| MDA CENACE | 108 zonas | Backend y 24 valores horarios por zona |
| Generación privada planeada | 18 proyectos | Referencia de planeación asociada a 20 permisos |
| Divisiones tarifarias | 17 | Usuarios y energía por división |

El repositorio de Atlas publica archivos JSON actualizables, no una API jurídica
de permisos. El servicio DGMESNIE los consume con caché, huella SHA-256 y estado
de revisión; la capa oficial de permisos continúa saliendo de la BD institucional.

## Orden del panel principal

### 1. Operación y mercado

El panel queda preparado para crecer por subsector. El primer bloque interno es
`Eléctrico`; en el futuro podrán añadirse bloques como `Gas natural`,
`Petrolíferos` o `Gasolinas` sin mezclar indicadores incompatibles.

#### Eléctrico

| Orden | Capa o módulo | Estado | Fuente principal | Alcance |
|---:|---|---|---|---|
| 1 | Centrales eléctricas · infraestructura | Integrada | Inventario DGMESNIE publicado en CDN | Activos físicos públicos y privados georreferenciados; no equivale a permisos |
| 2 | Permisos CRE · generación eléctrica | Integrada | BD institucional `vElectricidad_autorizado_mapa` | 1,060 permisos georreferenciados de generación; se excluyen 27 registros de importación/exportación de la visualización principal |
| 3 | Gerencias de control CENACE | Integrada | CENACE/CFE · geometría territorial DGMESNIE | Contexto regional de operación |
| 4 | Líneas de transmisión | Integrada | Inventario conciliado y grafo eléctrico en BD | Aristas del grafo con tensión y extremos |
| 5 | Subestaciones de transmisión | Integrada | Inventario conciliado en BD | Nodos clasificados como transmisión |
| 6 | Subestaciones de distribución | Integrada | Inventario conciliado en BD | Nodos de subtransmisión, distribución o nivel por determinar |
| 7 | Demanda y pronóstico | Mapa y gráfica integrados | CENACE `GraficaDemanda.aspx`, compilado por Atlas SEN | 9 regiones cartográficas más el agregado nacional; demanda, generación y pronóstico horario |
| 8 | Divisiones CFE · usuarios y consumo | Mapa y gráfica integrados | DOF + INEGI + memorias de cálculo CNE, compilado por Atlas SEN | 17 divisiones con color estable; usuarios, GWh e intensidad 2022–2026, referencia completa 2025 |
| 9 | Precios MDA por zona de carga | Backend integrado; mapa/gráfica pendientes | CENACE SWPEND MDA, compilado por Atlas SEN | PND promedio y horario, energía, pérdidas y congestión |

### 2. Planeación y expansión

| Orden | Capa | Estado | Fuente principal | Alcance |
|---:|---|---|---|---|
| 1 | Proyectos PAM / PAMRNT | Integrada | Inventario institucional y fuentes de planeación | Proyectos y asociación sugerida con la red |
| 2 | Generación privada planeada | Integrada como referencia secundaria | CNE/SENER, 1ª Convocatoria de Atención Prioritaria, compilado por Atlas SEN | 18 proyectos asociados a 20 permisos; algunas ubicaciones son aproximadas |
| 3 | Vacíos de red · 2ª convocatoria | Integrada | Evidencia de la Segunda Convocatoria | Coincidencias propuestas; no forman parte del grafo oficial sin validación |
| 4 | Transmisión planeada | Pendiente de contrato de datos | Plan SEN / fuente oficial aplicable | Obras futuras separadas de la red existente |

### 3. Contexto territorial

| Capa | Estado | Fuente |
|---|---|---|
| Estados | Integrada y activa al inicio | INEGI |
| Municipios | Integrada, diferida por peso | INEGI |
| PODECOBI | Integrada | DOF/SIDOF + BD institucional |

## Capas disponibles para análisis, fuera del panel principal

Se mantienen en el buscador y en el flujo de análisis territorial para no
saturar el mapa: ductos de importación, SISTRANGAS, presas, permisos de gas
natural, Gas LP y petrolíferos, ANP federales y estatales, conservación
voluntaria, RAMSAR, pueblos y localidades indígenas, lenguas, patrimonio
arqueológico e histórico y núcleos agrarios.

Estas capas se cargan únicamente cuando el usuario define una cobertura o las
selecciona expresamente.

## Plan de visualización

### Módulo A. Demanda y pronóstico horario

- Bandeja inferior con tres series: demanda, generación y pronóstico.
- Línea vertical para la hora actual.
- Tarjetas de demanda, generación y balance.
- Selección nacional o por gerencia desde el mapa.
- La selección de una región actualiza simultáneamente mapa, ficha y gráfica.
- Estado: implementado y validado en navegador.

### Módulo B. MDA y rampa de calor

- Capa de zonas de carga coloreada por PND promedio o por la hora seleccionada.
- Rampa secuencial para valores normales y acento guinda para valores extremos.
- Deslizador horario y reproducción automática.
- Al seleccionar una zona: curva de PND y desglose de energía, pérdidas y
  congestión.
- No se usarán polígonos inventados como si fueran límites oficiales. Cuando
  sólo exista un centroide, se mostrará como punto o superficie interpolada con
  una leyenda explícita de aproximación.

### Módulo C. Usuarios y consumo CFE

- Selector `Usuarios | Consumo | Intensidad`.
- Coropleta categórica con un color estable por cada una de las 17 divisiones.
- Selector de división.
- Intensidad calculada en kWh por usuario, además de usuarios totales y GWh.
- Serie anual 2022–2026; 2025 se identifica como último año completo.
- Estado: implementado y validado en navegador.

### Módulo D. Generación distribuida

- Capacidad y contratos acumulados por entidad.
- Evolución anual por rango de tamaño.
- Reproducción de la línea de tiempo y comparación estatal.
- Fuente prevista: estadísticas CNE/CRE compiladas por Atlas SEN.

## Reglas de rendimiento

- Inicio con mapa sin teselas y límites estatales; ninguna capa pesada activa.
- Carga diferida por capa y caché del backend.
- Una bandeja gráfica montada sólo cuando se solicita.
- Puntos masivos con Canvas o agrupación controlada; subestaciones del grafo se
  mantienen como elementos individuales.
- Geometrías generalizadas para vista nacional y detalle completo al acercarse.
- Series horarias separadas de las geometrías para evitar redibujar el mapa.

## Tour guiado propuesto

El tour institucional será más corto que el de referencia:

1. organización de capas;
2. centrales, permisos y red;
3. demanda y pronóstico;
4. usuarios, consumo e intensidad;
5. MDA por zona y hora;
6. análisis territorial y generación de reportes.

Cada paso debe poder omitirse y el tour no debe iniciarse automáticamente en
visitas subsecuentes.
