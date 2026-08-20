# Tablero de Planeación Vinculante — plan de arquitectura e implementación

**Estado:** implementación técnica de Fase 1 en curso; pendiente de validación institucional  
**Fecha de inicio:** 19 de agosto de 2026  
**Última actualización:** 19 de agosto de 2026  
**Versión del plan:** 1.0  
**Producto:** integración PLADESE · PVIRCE · PROSENER · PAMRNT/PAMRGD · CENACE · VUPE/convocatorias  
**Módulo propuesto:** `PlaneacionVinculante`  
**Responsable funcional:** por definir  
**Responsable de datos:** por definir

Este documento es el plan vivo del nuevo tablero. Debe actualizarse al cerrar cada tarea, cambiar una regla de negocio o incorporar una fuente. El markdown recibido el 19 de agosto de 2026 se tomó como insumo conceptual; las decisiones y tareas siguientes fueron adaptadas a la arquitectura y a la base de datos reales de DGMESNIE.

---

## 1. Resultado que debe producir el sistema

El tablero debe responder, con fecha de corte y evidencia:

> ¿Qué exige la planeación, qué parte ya está cubierta por centrales y obras concretas, qué está realmente instrumentado, qué red lo habilita, cuánto cuesta, qué falta y qué decisión pone en riesgo el cumplimiento?

La cadena auditable será:

```text
PROSENER / PLADESE
        │
        ▼
Meta sectorial o vinculante
        │
        ▼
Requerimiento PVIRCE de capacidad
        ├── tecnología, región, año y MW
        │
        ▼
Cobertura explícita y validada
        │
        ▼
Proyecto de generación (core.Proyecto)
        │
        ├── folio VUPE / convocatoria / permiso / contrato
        ├── avance, hitos, riesgos y costos
        └── dependencia de red
                    │
                    ▼
             obra PAMRNT/PAMRGD
                    │
                    ├── subestación / nodo / línea
                    ├── refuerzo o interconexión
                    ├── costo y responsable
                    └── seguimiento físico-financiero
```

Regla rectora: **ningún MW contará como cumplimiento sólo por compartir tecnología, entidad, GCR o cercanía geográfica**. Debe existir una asignación respaldada y validada.

---

## 2. Alcance y separación respecto del tablero territorial

### Incluido

- Metas sectoriales y de planeación de PROSENER y PLADESE.
- Necesidades de instalación, sustitución y retiro del PVIRCE.
- Centrales o carteras que cubren esas necesidades.
- Folios VUPE, convocatorias, participantes, resultados, permisos y contratos.
- Obras PAMRNT y PAMRGD, subestaciones, líneas y puntos de interconexión.
- Obras de refuerzo e interconexión y sus costos.
- Seguimiento físico, financiero, regulatorio, contractual y de fechas.
- Brechas, dependencias críticas, riesgos y decisiones requeridas.
- Un mapa contextual con capas encendibles y apagables.

### Fuera del primer alcance

- Buffers, intersecciones territoriales, análisis ambiental o social automático.
- Inferir automáticamente que una central cubre una meta.
- Inferir automáticamente una dependencia de red sólo por proximidad.
- Sustituir los sistemas fuente de CENACE, CFE, CNE o VUPE.
- Pronósticos de cumplimiento con modelos estadísticos antes de contar con cortes históricos suficientes.

### Relación con el tablero territorial

El tablero territorial actual conserva su propósito de exploración, capas, contexto y análisis espacial. El nuevo tablero será una aplicación distinta, enfocada en planeación vinculante, trazabilidad y gestión. Reutilizará servicios y componentes, pero no crecerá dentro de `Views/DashboardProyectos/Index.cshtml`.

---

## 3. Decisión de arquitectura

### ADR-001: módulo independiente sobre servicios y datos compartidos

**Estado:** propuesto  
**Decisión:** crear `PlaneacionVinculanteController`, servicio, repositorio, vistas y JavaScript propios. Reutilizar los servicios existentes mediante contratos estables y conservar el tablero territorial sin mezclar responsabilidades.

| Opción | Complejidad | Velocidad inicial | Mantenibilidad | Decisión |
|---|---:|---:|---:|---|
| Ampliar el tablero territorial monolítico | Baja al inicio | Alta | Baja | Descartada |
| Nuevo módulo MVC compartiendo servicios y BD | Media | Media/alta | Alta | **Seleccionada** |
| Aplicación externa o microservicio nuevo | Alta | Baja | Media/alta | Pospuesta |

Consecuencias:

- Se evita seguir aumentando una vista territorial de más de doce mil líneas.
- Los endpoints de red, PAM, demanda y convocatorias pueden reutilizarse.
- Las métricas ejecutivas saldrán del backend y de datos versionados, no de hojas consultadas directamente por el navegador.
- La extracción de componentes visuales compartidos se hará sólo cuando exista un caso real de reutilización; no se bloqueará el MVP por una refactorización general.

### ADR-002: BD institucional como fuente de verdad del tablero

**Estado:** propuesto  
**Decisión:** Excel, Google Sheets, PDFs y APIs son fuentes de entrada. Toda cifra de cumplimiento debe provenir de una carga versionada, validada y trazable en SQL Server.

No se utilizará el acceso directo del frontend a Google Sheets para calcular KPIs oficiales. El catálogo actual puede seguir alimentando el tablero territorial, pero el nuevo módulo consumirá APIs internas.

### ADR-003: `core.Proyecto` como identidad de centrales y carteras concretas

**Estado:** propuesto  
**Decisión:** una central o proyecto concreto tendrá identidad canónica en `core.Proyecto`. Los folios VUPE, nombres históricos y claves externas se registrarán como identificadores. Los requerimientos agregados y las obras de red no se convertirán artificialmente en centrales.

### ADR-004: separar metas, escenarios, requerimientos y ejecución

**Estado:** propuesto  
**Decisión:** el tablero conservará cuatro granos de información distintos y los relacionará mediante asignaciones explícitas:

1. **Meta sectorial:** indicador PROSENER, con línea base, fórmula y trayectoria anual.
2. **Escenario de planeación:** demanda, consumo, capacidad, generación, reserva, costos y supuestos de PLADESE.
3. **Requerimiento vinculante:** necesidad de capacidad PVIRCE o de red, aún sin asumir que corresponde a un proyecto individual.
4. **Ejecución:** central, folio, contrato, obra PAM, avance físico-financiero y evidencia.

Una cifra de escenario no contará como meta; una fila PVIRCE agregada no se convertirá automáticamente en central; y una necesidad de refuerzo no se considerará obra PAM confirmada hasta validar la relación.

### ADR-005: edición anual y cortes inmutables de un programa acumulativo

**Estado:** propuesto  
**Contexto:** el PLADESE tiene horizonte móvil de 15 años y actualización anual. El PVIRCE es acumulativo: durante la preparación de una misma edición se agregan, corrigen, reprograman o reclasifican proyectos y después se incorporan años adicionales. Cada edición integra PVIRCE y PAM, pero también pueden existir cortes de trabajo previos a la publicación y actualizaciones operativas posteriores del PAM. PROSENER 2025–2030 es un programa sexenal con trayectorias y resultados anuales.

**Decisión:** conservar como inmutable cada publicación y cada corte de trabajo que se use para una decisión o comparación. Una edición o corte nuevo no actualizará filas históricas: creará una nueva versión y relaciones explícitas de integración, actualización o sustitución. Los cortes de trabajo de una edición se identificarán como borradores y no adquirirán carácter oficial por estar cargados en el sistema.

```text
Edición/ciclo de planeación             Seguimiento de ejecución
PLADESE/PVIRCE 2026–2040                corte mensual 2026-08
├── corte de trabajo 2026-05-16         ├── proyecto/folio
├── corte de trabajo 2026-08-04         ├── avance y fechas
├── publicación oficial, cuando exista  ├── costo y riesgo
└── PAM, escenarios y requerimientos    └── evidencia observada

PLADESE/PVIRCE siguiente
└── amplía el horizonte; no sobrescribe cortes ni publicaciones anteriores
```

Reglas:

- El selector principal distinguirá `Edición de planeación` y `Corte/Fotografía`; por defecto mostrará la última versión oficial, con opción explícita para consultar borradores autorizados.
- Siempre existirá una vista `Foto original` que reproduce lo publicado en cada edición.
- El seguimiento mensual se asociará a la edición vigente en la fecha del corte, sin alterar la fuente anual.
- La comparación entre cortes o ediciones explicará altas, bajas, reprogramaciones, cambios de MW, tecnología, costo y horizonte.
- Ningún KPI sumará dos cortes del mismo programa; cada cálculo usará exactamente una versión seleccionada.
- La vista ejecutiva abrirá en `Periodo de la administración 2025–2030`: 2024 será línea base, los registros anteriores a 2026 se mostrarán como ejecutados/base, 2026–2030 como cartera de gestión y 2031–2040 como continuidad de largo plazo.
- PROSENER conservará una versión del programa, sus metas anuales 2025–2030 y los valores observados por año; una reforma o sustitución generará otra versión.

### ADR-006: separar entidades por semántica y conservar llaves reales

**Estado:** propuesto  
**Contexto:** una sola tabla de “metas” con un clasificador puede terminar mezclando metas normativas, requerimientos PVIRCE/PAM, escenarios, líneas base y supuestos. Las relaciones polimórficas `TipoEntidad + EntidadId` también debilitan la integridad referencial.

**Decisión:** separar físicamente los conceptos principales:

- `PlaneacionMeta`: sólo metas sectoriales o vinculantes con criterio de cumplimiento.
- `PlaneacionRequerimientoCapacidad`: necesidad PVIRCE agregada o particular.
- `PlaneacionRequerimientoRed`: necesidad documental de red antes de confirmar una obra.
- `PlaneacionEscenario` y `PlaneacionSerie`: proyecciones, líneas base y valores observados.
- `PlaneacionSupuestoCosto`: supuestos técnicos o económicos.

`ClaseRegistroPlaneacion` seguirá siendo obligatoria en staging para clasificar cada registro fuente, pero no sustituirá la separación del modelo normalizado. Las asociaciones con proyecto, PAM, nodo, subestación, arista, procedimiento y dependencia utilizarán llaves foráneas reales y restricciones de consistencia.

**Consecuencias:** habrá más tablas y una migración inicial ligeramente mayor, pero las fórmulas, validaciones, auditoría y consultas quedarán menos ambiguas. El seguimiento transversal usará encabezado de corte más detalle de entidades con referencias explícitas, sin duplicar los cortes PAM existentes.

---

## 4. Línea base verificada

Corte técnico consultado en `BDPruebasSNIER` el 19 de agosto de 2026.

### Disponible y reutilizable

| Dominio | Evidencia actual | Uso propuesto |
|---|---:|---|
| PAM / PAMRNT | 285 proyectos vigentes, 293 identidades y 778 versiones | Catálogo e historia de obras de red |
| Seguimiento de transmisión | 282 unidades, 286 detalles de corte y 292 relaciones con proyectos PAM | Fechas, avances, concurso, adjudicación, contrato, construcción y FEO |
| Fuentes PAM | 7 fuentes registradas con corte, versión, ruta y hash | Trazabilidad documental |
| Red eléctrica | versión activa con 3,133 nodos y 3,036 aristas | Capas, nodos, rutas y contexto de conexión |
| Inventario de subestaciones | 4,768 registros multifuente | Capas RNT/RGD y conciliación de subestaciones |
| Cartera de convocatorias | 67 registros; 65 activos; 10,196.21 MW activos | Folios, proyectos, tecnología, GCR, subestación y punto de interconexión |
| Validación convocatoria–red | 122 registros | Candidatos y evidencia de relación con red |
| Demanda CENACE | series horarias ya integradas en el tablero territorial | Contexto operativo, no meta de planeación |
| Estados, municipios y GCR | capas ya integradas | Filtros y mapa contextual |
| PLADESE 2025-2039 en SIDOF/DOF | Texto oficial completo, capítulos 1–4 y anexos | Fuente primaria para línea base, escenarios, PVIRCE, PAM y refuerzos vinculantes |
| PROSENER 2025-2030 en DOF | Documento oficial con 6 indicadores, líneas base y metas | Fuente primaria de metas sectoriales; priorizar los 4 indicadores eléctricos |

La versión activa de red debe usarse para indicadores. Los totales físicos de las tablas incluyen tres versiones históricas y no deben sumarse como si fueran una sola red.

### Disponible, pero requiere transformación

| Fuente/componente | Situación | Acción |
|---|---|---|
| Excel `PVIRCE2026-2040_SQ.xlsx` | Archivo local y script de 449 filas disponibles | Cargar como fuente versionada y clasificar la granularidad de cada fila |
| Excel `PVIRCE.xlsx` | Primer corte de trabajo 2026: 459 filas y 24 columnas; 73,238.049 MW brutos | Conservar como fotografía técnica y conciliar contra el PVIRCE oficial integrado al PLADESE 2025 antes de marcarla validada |
| Excel `PVIRCE_20260804_vcorta.xlsx` | Borrador en revisión de 485 filas y 36 columnas; 81,798.279 MW instalados | Registrar como corte de trabajo no oficial y calcular cambios contra la línea base, sin sobrescribirla |
| Excel `PLADESE Cap 4.1 TABLAS.xlsx` | 191 registros consolidados de tablas 4.1–4.8 y 34 registros de la tabla 4.9 | Ingerir como extracción estructurada del DOF, separando requerimientos agregados y requerimientos de particulares |
| Dataset PVIRCE en Google Sheets | Registrado en `datasets-catalog.json` | Usarlo como fuente de ingestión/contraste, no como fuente directa del KPI |
| `dgmesnie.PvirseCentralesElectricas` | Tabla creada, 0 registros | Tratarla como importación heredada; no como modelo canónico definitivo |
| `staging.ExcelRaw_PVIRCE` | Tabla creada, 0 registros | Reutilizar o sustituir con staging versionado y auditable |
| Cartera de convocatorias | 0 de 67 registros vinculados con `core.Proyecto` | Resolver identidad antes de calcular cobertura |
| Costos PAM | Hay `MontoProyectoMdp` e `ImporteMdp` en fuentes PAM | Separar estimado, refuerzo, interconexión, contrato, devengado y pagado |

### Faltantes estructurales

- No existen objetos SQL específicos para PLADESE o PROSENER.
- No existe una tabla de metas vinculantes versionadas.
- No existe asignación formal requerimiento PVIRCE ↔ proyecto ↔ MW.
- No existe relación canónica proyecto de generación ↔ obra PAMRNT/PAMRGD.
- Los folios de convocatoria no están vinculados a `core.Proyecto`.
- No existe un modelo uniforme de procedimientos, fallos, permisos y contratos.
- No existe un registro normalizado de costos de refuerzo e interconexión.
- La demanda horaria CENACE existente no sustituye la proyección de demanda de PLADESE por escenario, región y año.
- Falta aprobar institucionalmente cuáles indicadores PROSENER se mostrarán como metas directas y cuáles sólo como contexto.

### Hallazgos que cambian la estrategia de carga

El Excel recibido y el texto oficial permiten arrancar sin esperar otro archivo del PLADESE, pero deben manejarse como fuentes con granos distintos:

| Bloque | Registros/cifras de control | Tratamiento |
|---|---:|---|
| Tablas PLADESE 4.1–4.8 | 191 filas; 75,564 MW de adiciones; 1,805 MW de sustituciones; 73,759 MW netos | Metas/necesidades agregadas por GCR, tecnología, movimiento y año |
| Tabla PLADESE 4.9 | 34 filas; 5,970 MW; 11,990 MDP visibles | Requerimientos de particulares con región de transmisión, subestación, tensión, entidad y costo de refuerzo |
| Texto previo a tabla 4.9 | 7,405 MW | Excepción de conciliación: no sustituir ni sumar con los 5,970 MW hasta documentar su alcance |
| Capítulo 4.3, tabla 4.23 | 80,825.57 MDP de red vinculante: 33,269.93 privados, 45,561.67 CFE y 1,993.97 PEMEX | Control de alcance y costo; no equivale al subtotal visible de la tabla 4.9 |
| Fuente PVIRCE de 449 filas | Granularidad pendiente de validar contra las tablas oficiales | Fuente candidata de proyectos/carteras; no reemplaza la meta agregada |

La tabla 4.9 ya contiene el primer puente documental `PVIRCE → región de transmisión → subestación → refuerzo → costo`, pero todavía no demuestra por sí sola qué folio VUPE ni qué obra PAM específica materializa cada renglón.

### Matriz de extracción oficial priorizada

| Prioridad | Fuente oficial | Datos a estructurar | Uso en el tablero |
|---:|---|---|---|
| 1 | PROSENER, sección 8 | Indicador, fórmula, línea base, metas 2025–2030, unidad, periodicidad y responsable | Portada de metas sectoriales |
| 1 | PLADESE, capítulo 4.1 | Tablas 4.1–4.9 y notas metodológicas | Meta PVIRCE y requerimientos particulares |
| 1 | PLADESE, capítulos 4.2 y 4.3 | Tablas 4.10–4.40: PAM, metas físicas, proyectos, refuerzos e inversiones | Relación generación–red–costo |
| 2 | PLADESE, capítulo 2 y anexo A1 | Capacidad, generación, demanda, consumo, pérdidas, RNT/RGD y línea base 2024 | Diagnóstico y denominadores |
| 2 | PLADESE, capítulo 3 | Demanda/consumo 2025–2039, capacidad, generación, SAE, reserva, costos y combustibles | Escenario base y comparación con ejecución |
| 2 | PLADESE, anexos A2 y A3 | Tecnologías y costos unitarios de generación, transmisión y distribución | Catálogos, supuestos y controles de costo |

#### Metas PROSENER candidatas para el alcance eléctrico

| Indicador | Línea base | Meta/trayectoria | Decisión propuesta |
|---|---:|---:|---|
| 1.2 Participación estatal en generación neta | 55.40% en 2024 | al menos 54% cada año 2025–2030 | Meta directa de portada |
| 2.1 Participación de energías limpias | 23.19% en 2023; serie reporta 23.40% en 2024 | mayor a 23.40% en 2025–2029 y 38% en 2030 | Meta directa, preservando la diferencia de líneas base |
| 3.1 Población con acceso al servicio eléctrico | 99.64% en 2024 | mayor a 99.64% en 2025–2029 y 99.99% en 2030 | Meta directa, con desglose territorial sólo si existe fuente válida |
| 3.2 Capacidad renovable para reducir pobreza energética | 0 kW en 2024 | 5,000; 18,000; 54,000; 108,000; 144,000; 180,000 kW | Meta directa, separada del PVIRCE de gran escala |
| 2.2 Variación de intensidad energética | -0.47% reportado para 2024 | -2.9% en 2030 | Contexto sectorial; no asignar a centrales u obras |
| 1.1 Producción nacional de hidrocarburos líquidos | PROSENER | trayectoria PROSENER | Fuera del tablero eléctrico salvo decisión institucional |

---

## 5. Qué se reutiliza y qué no

### Reutilización directa

- `RedElectricaGraphController`: nodos, aristas, rutas, vecinos, subgrafo PAM y GeoJSON.
- `RedElectricaSubstationInventoryController`: resumen, registros y GeoJSON por nivel de red.
- `PamTerritorialController`: proyectos, asociaciones candidatas, validación y GeoJSON.
- `IPamSeguimientoTransmisionService` y vistas `vw_PAMSeguimientoActual` / `vw_PAMSeguimientoProyectoActual`.
- `DashboardProyectos/Convocatorias/Dataset` como contrato transitorio de lectura.
- Capas de GCR, estados, municipios, centrales, líneas y subestaciones.
- Patrones institucionales de header, tarjetas, filtros, tablas, gráficas y exportación.
- Carga diferida, caché y geometrías simplificadas del tablero territorial.

### Reutilización con adaptación

- `PvirseImportService`: sirve para leer el Excel, pero debe producir una carga versionada y validaciones, no borrar y reemplazar silenciosamente la tabla.
- `PvirseCentralesElectricas`: puede conservarse como compatibilidad temporal o vista de lectura; el modelo nuevo debe separar registro fuente, meta y proyecto.
- `CarteraConvocatoriaProyecto`: conservar la tabla y completar `ProyectoCoreId`; no duplicar sus 67 folios.
- PAM y grafo: consumir la versión activa y sus identificadores; no copiar sus registros a nuevas tablas.
- Demanda CENACE: mostrarla como contexto operativo y comparar posteriormente con proyecciones oficiales, manteniendo ambas series separadas.

### No reutilizar como núcleo

- La vista monolítica `DashboardProyectos/Index.cshtml` como base del nuevo módulo.
- El cálculo de KPIs en JavaScript a partir de CSV remotos.
- Nombres libres como llave de relación.
- Coincidencias geográficas como evidencia definitiva.
- La suma directa de montos con moneda, año base o alcance distintos.

---

## 6. Modelo de datos objetivo

Las tablas nuevas seguirán la convención del proyecto: esquema `dgmesnie`, nombres CamelCase y migraciones idempotentes.

### 6.1 Primera migración: fundamento de planeación

| Tabla propuesta | Propósito |
|---|---|
| `PlaneacionInstrumentoVersion` | Edición, clase de versión, fecha de corte, archivo, hash, oficialidad y estado de PVIRCE/PLADESE/PROSENER/PAM |
| `PlaneacionInstrumentoVersionRelacion` | Relación entre versiones: integra, actualiza, sustituye o fue vigente junto con otra |
| `PlaneacionCarga` | Resultado de cada ingestión, conteos, validaciones y usuario |
| `PlaneacionRegistroFuente` | Copia íntegra y auditable de cada fila/registro original en JSON, con `ClaseRegistroPlaneacion` obligatoria |
| `PlaneacionIndicador` | Ficha de indicador PROSENER/PLADESE: objetivo, definición, fórmula, unidad, frecuencia y responsable |
| `PlaneacionIndicadorValor` | Línea base, meta anual y valor observado por indicador y corte |
| `PlaneacionEscenario` | Versión y escenario PLADESE: base, alto, bajo o sensibilidad |
| `PlaneacionSerie` | Serie de demanda, consumo, capacidad, generación, SAE, reserva u otra métrica, con dimensiones y unidad |
| `PlaneacionSerieValor` | Valor anual o temporal y rol: línea base, meta, escenario, observado o estimación provisional |
| `PlaneacionMeta` | Sólo meta sectorial o vinculante, con criterio de cumplimiento, unidad, valor, año y fuente |
| `PlaneacionRequerimientoCapacidad` | Necesidad PVIRCE agregada o particular por GCR, tecnología, movimiento, año y MW |
| `PlaneacionMetaRequerimiento` | Relación trazable entre una meta y los requerimientos que contribuyen a cumplirla |
| `PlaneacionProyectoCobertura` | Asignación validada entre `core.Proyecto` y un requerimiento de capacidad |
| `PlaneacionCatalogoMapeo` | Homologación de tecnologías, GCR, estatus, unidades y variantes de texto |
| `PlaneacionCambioVersion` | Diferencia explicada entre cortes o ediciones: alta, baja, renombre, reprogramación, cambio de MW, tecnología, estatus, costo o alcance |

`PlaneacionRegistroFuente` evita perder información o convertir prematuramente cada fila PVIRCE en una central. Durante la clasificación, cada fila debe quedar como uno de estos tipos:

- meta sectorial;
- meta vinculante;
- requerimiento de capacidad;
- requerimiento de red;
- escenario;
- línea base;
- supuesto;
- central o unidad identificable;
- cartera o bloque;
- sustitución;
- retiro;
- optimización sin proyecto individual;
- fila excluida, con motivo.

Campos obligatorios de `PlaneacionInstrumentoVersion`: `TipoInstrumento`, `AnioEdicion`, `HorizonteInicio`, `HorizonteFin`, `ClaseVersion`, `EstadoPublicacion`, `EsOficial`, `FechaCorteFuente`, `FechaPublicacion`, `VigenteDesde`, `VigenteHasta`, `EsFotoLineaBase`, `VersionPadreId`, `VersionSustituidaId`, `FuenteUri`, `HashFuente` y `EstadoValidacion`. `ClaseVersion` distinguirá al menos `PUBLICACION_OFICIAL`, `CORTE_BASE`, `BORRADOR_REVISION` y `CORTE_OPERATIVO`.

`PlaneacionProyectoCobertura` quedará preparado desde la primera migración con: `CapacidadAsignadaMw`, `CapacidadFirmeMw`, `CapacidadNetaAsignadaMw`, `TipoAportacion`, `MetodoAsignacion`, `MetodologiaCapacidadVersionId`, `NivelCerteza`, `EstadoValidacion`, `FechaValidacion`, `ValidadoPorUsuarioId` y `FuenteRegistroId`. En el MVP sólo `CapacidadAsignadaMw` será exigible; capacidad firme o neta permanecerá `NULL` hasta contar con metodología aprobada y versionada.

Las entidades versionables y auditables distinguirán tiempo de negocio y tiempo de sistema mediante `VigenteDesde`, `VigenteHasta`, `FechaCorte`, `FuentePublicadaEn`, `RegistradoEn` y `ActualizadoEn`. No todos los campos serán modificables: registros fuente, fotos anuales y cargas cerradas serán inmutables.

### 6.2 Segunda migración: instrumentación y folios

| Tabla propuesta | Propósito |
|---|---|
| `PlaneacionProcedimiento` | Convocatoria, VUPE, permiso, fallo, contrato u otro mecanismo |
| `PlaneacionProyectoProcedimiento` | Relación proyecto–folio–etapa–capacidad afectada |
| `PlaneacionProcedimientoHito` | Calendario programado y real de cada etapa |
| `PlaneacionParticipante` | Participante, promovente, adjudicatario o responsable, con rol y vigencia |

Mientras se migra, `CarteraConvocatoriaProyecto` seguirá siendo la fuente operativa de folios. Su `ProyectoCoreId` será el enlace hacia el modelo canónico.

### 6.3 Tercera migración: dependencias y costos de red

| Tabla propuesta | Propósito |
|---|---|
| `PlaneacionDependenciaRed` | Proyecto de generación ↔ proyecto PAM/obra/nodo, tipo, criticidad, MW y fechas |
| `PlaneacionRequerimientoRed` | Necesidad documental de red de tablas 4.9 y 4.23–4.40, antes de confirmar una obra PAM |
| `PlaneacionObraCosto` | Refuerzo, interconexión, expansión RNT/RGD, estimado/contratado/devengado/pagado |
| `PlaneacionSupuestoCosto` | Costo unitario, moneda, año base, tecnología/elemento y versión de PLADESE |
| `PlaneacionDependenciaEvidencia` | Fuente, página, folio, documento y validación de la relación |

`PlaneacionDependenciaRed` evitará identificadores polimórficos sin llave. Tendrá referencias explícitas y opcionales: `ProyectoCoreId`, `RequerimientoCapacidadId`, `PamProyectoId`, `NodoRedId`, `SubestacionId`, `AristaRedId` y `RequerimientoRedId`. Una restricción `CHECK` exigirá al menos una referencia del lado generación/requerimiento y al menos una referencia del lado red; podrán coexistir PAM, subestación y nodo cuando describan la misma dependencia.

Tipos mínimos de dependencia:

- conexión directa;
- obra de interconexión;
- obra de refuerzo;
- expansión RNT;
- expansión RGD;
- condicionante sistémica;
- complementaria.

Las fechas conservarán semántica explícita: `FechaObjetivoPlaneacion`, `FechaNecesariaOperacion`, `FechaProgramadaContrato`, `FechaEstimacionVigente`, `FechaRealOperacion` y `FechaCorte`. La etiqueta fuente `FEO` se preservará en el registro original y se normalizará sólo cuando se conozca si corresponde a objetivo, compromiso contractual, estimación vigente o fecha real.

```text
Desviación de planeación = FechaEstimacionVigente - FechaObjetivoPlaneacion
Desviación contractual   = FechaEstimacionVigente - FechaProgramadaContrato
Retraso real             = FechaRealOperacion - FechaObjetivoPlaneacion
```

### 6.4 Cuarta migración: seguimiento, riesgos y decisiones

| Tabla propuesta | Propósito |
|---|---|
| `PlaneacionSeguimientoCorte` | Encabezado mensual: fecha, fuente, publicación, cierre y responsable |
| `PlaneacionSeguimientoEntidad` | Detalle transversal con FK real a proyecto, PAM, procedimiento o dependencia, avance, estatus y fechas |
| `PlaneacionRiesgo` | Probabilidad, impacto, urgencia, MW/monto afectado, responsable y mitigación |
| `PlaneacionDecision` | Decisión requerida, nivel de escalamiento, fecha compromiso y cierre |

No se duplicarán los cortes PAM existentes. El nuevo corte referenciará la entidad fuente y agregará únicamente la lectura transversal de planeación.

`PlaneacionSeguimientoEntidad` tendrá `FuenteSeguimiento`, `EsDatoConfirmado` y referencias opcionales `ProyectoCoreId`, `PamProyectoId`, `ProcedimientoId` y `DependenciaRedId`, con restricción de exactamente una entidad seguida por renglón. Así se obtiene una vista común sin perder integridad referencial.

---

## 7. Reglas de identidad y asociación

### Identidad de proyecto

1. Buscar identificador exacto: folio VUPE, clave PAM, permiso, ID SEPI o clave oficial.
2. Buscar alias histórico normalizado.
3. Generar candidatos por nombre, capacidad, tecnología, GCR, entidad, subestación y punto de interconexión.
4. La coincidencia automática sólo crea un candidato.
5. Una persona responsable acepta o rechaza el vínculo.
6. El vínculo validado conserva usuario, fecha, fuente, algoritmo/regla y nivel de confianza.

### Asignación a meta

- Una meta puede ser cubierta por varios proyectos.
- Un proyecto puede participar en varias metas sólo si la metodología lo permite.
- La suma de MW asignados no puede exceder la meta sin justificación aprobada.
- `Identificado`, `convocado`, `adjudicado`, `contratado`, `construcción` y `operación` son estados distintos.
- Una convocatoria sin fallo no cuenta como capacidad contratada.
- Un proyecto cancelado deja de cubrir la meta en la proyección conservadora.

### Asociación con red

- La relación debe priorizar subestación, nodo, estudio, anexo técnico y obra explícita.
- El grafo puede sugerir rutas o vecinos, pero no validar la dependencia.
- La compatibilidad se evalúa contra fecha requerida de la central y fecha prevista de la obra.
- Deben registrarse por separado obras de refuerzo e interconexión, incluso cuando aparezcan en un mismo documento.

---

## 8. Métricas iniciales

Todas las métricas se calcularán en el backend y devolverán numerador, denominador, fecha de corte y definición.

### Cobertura

```text
MW requeridos          = requerimientos PVIRCE vigentes del corte
MW identificados       = asignaciones validadas a proyectos
MW convocados          = proyectos con procedimiento publicado
MW adjudicados         = proyectos con fallo válido
MW contratados         = proyectos con contrato vigente
MW en construcción     = proyectos con inicio real confirmado
MW en operación        = proyectos con FEO real confirmada
Brecha instrumentación = MW requeridos - MW comprometidos válidos
Brecha operación       = MW requeridos al corte - MW en operación
```

### Red

```text
MW con red compatible  = proyectos cuyas dependencias críticas estarán listas a tiempo
MW bloqueados por red  = proyectos con dependencia crítica tardía, sin contrato o sin identificar
Desfase de red          = fecha prevista de obra - fecha requerida por la central
```

### Prevalencia del Estado

La prevalencia será un indicador jurídico propio, distinto de la prelación de proyectos privados. Su nombre de tablero será **Prevalencia del Estado / no prevalencia de particulares**.

```text
Participación del Estado (%) = Generación de Electricidad Inyectada por el Estado
                              / Generación de Electricidad Inyectada Total × 100
Participación particular (%) = 100% - Participación del Estado
Margen de prevalencia        = Participación del Estado - 54 puntos porcentuales
```

Base jurídica y alcance:

- Constitución, artículo 27: las actividades particulares no pueden tener prevalencia sobre la empresa pública del Estado; no fija ahí un porcentaje.
- Ley del Sector Eléctrico, artículo 12, fracción VI: el Estado debe mantener al menos 54% del promedio de la energía inyectada a la red en un año calendario.
- Reglamento de la Ley del Sector Eléctrico, artículos 2, fracciones XIII–XIV, y 9: define numerador, denominador y cálculo anual; SENER debe efectuar la evaluación a más tardar el último día hábil de febrero de cada año.
- El 46% no es una cuota mínima o fija para particulares: es el máximo complementario cuando el Estado se encuentra exactamente en 54%; si el Estado supera 54%, la participación particular es menor a 46%.

La definición reglamentaria del numerador estatal incluye la generación neta inyectada por centrales de la Empresa Pública del Estado, la proporción estatal en centrales con participación mixta y centrales de otras entidades estatales, gobiernos estatales y órganos político-administrativos de la Ciudad de México. CFE, FONADIN y PEMEX corresponden a la integración reportada en la línea base PROSENER 2024, pero el modelo no limitará jurídicamente el universo sólo a esas tres entidades.

El tablero mostrará por año:

- barra 100% apilada Estado/particulares y línea de referencia 54%/46%;
- piso legal y meta PROSENER: Estado al menos 54%;
- línea base 2024: 55.40%;
- trayectoria del escenario PLADESE, que alcanza aproximadamente 59% en 2030;
- resultado anual oficial SENER y, por separado, estimación provisional del año en curso;
- margen sobre el 54%, tendencia y alerta si el Estado baja de 54% o los particulares superan 46%.

Sólo el cálculo anual oficial tendrá estatus de cumplimiento jurídico. Los cortes mensuales servirán como estimación de gestión y deberán identificarse como provisionales. Para prospectiva se conservarán los escenarios de demanda, expansión, avance de proyectos y el factor de ajuste por riesgo exigido por el Reglamento.

### Energías limpias

- La meta principal será participación de **generación limpia**, calculada en GWh/TWh sobre generación neta total.
- Se mostrarán la línea base, la meta PROSENER de 38% en 2030, la trayectoria PLADESE y el valor observado anual.
- En otra visual se mostrará la composición de **capacidad** PVIRCE en MW limpia, renovable, almacenamiento y convencional; no se confundirá con participación de generación.
- Todo factor de planta o conversión MW→GWh tendrá valor, fuente y versión explícitos.
- Las metas porcentuales no se sumarán con metas de capacidad.
- La clasificación `limpia`, `renovable`, `almacenamiento` y `convencional` será versionada porque las categorías no son intercambiables.
- Cada indicador conservará fórmula, periodicidad, responsable y trayectoria anual; el tablero no reconstruirá una fórmula distinta.

### Prelación de proyectos

- `Con prelación` identificará proyectos particulares firmes o con contrato de interconexión conforme a la fuente y edición correspondiente.
- No se utilizará `prelación` como sinónimo de prevalencia ni como evidencia automática de operación.
- La portada podrá separar `MW con prelación`, `MW de requerimiento vinculante para particulares`, `MW convocados`, `MW adjudicados` y `MW en operación`.

### Demanda, suficiencia y escenarios

- La demanda horaria CENACE será dato observado y la proyección PLADESE será una serie separada por versión y escenario.
- La comparación `demanda proyectada ↔ capacidad ejecutable` incluirá reserva, disponibilidad y energía cuando corresponda; no se calculará como una resta simple de MW nominales.
- El tablero distinguirá línea base 2024, escenario 2025–2039, meta vinculante y avance real a la fecha de corte.

### Costos

- Separar estimado inicial, estimado vigente, costo de refuerzo, costo de interconexión, convocado, adjudicado, contratado, devengado y pagado.
- Registrar moneda, año base, IVA, fecha de corte, fuente y alcance.
- Un dato desconocido será `NULL`/“sin dato”, nunca cero.

---

## 9. Producto y navegación

**Título de interfaz:** `Planeación Vinculante del SEN`  
**Subtítulo:** `Metas, proyectos, red, inversión y riesgos de cumplimiento`

Pregunta rectora de portada:

> Al corte seleccionado: ¿qué parte de la meta está operando, ejecutándose, contratada, en proceso o aún pendiente de instrumentar?

### Portada ejecutiva

- Selector de `Periodo`, con `Administración 2025–2030` por defecto y opción `Horizonte completo`.
- Selector de `Edición` y `Corte/Fotografía`, con distintivo visible `Oficial`, `Corte de trabajo` o `En revisión`.
- Comparación predeterminada `PVIRCE oficial en PLADESE ↔ cortes de trabajo 2026`.
- Primera franja `Resultados PROSENER`: prevalencia, energías limpias, acceso eléctrico y capacidad renovable contra pobreza energética; cada tarjeta separa base, meta, observado, escenario, corte y fuente.
- Segunda franja `Ejecución PLADESE`: meta PVIRCE, MW identificados, comprometidos/construcción, brecha, MW bloqueados por red e inversión comparable.
- Meta, cobertura, avance y brecha por corte.
- Curva anual requerida vs. identificada vs. ejecutable vs. operativa.
- Embudo de instrumentación.
- MW bloqueados por red.
- Principales desviaciones de plazo y costo.
- Riesgos y decisiones con mayor impacto.
- Comparador: qué cambió entre la publicación oficial, el corte base y cualquier borrador/edición posterior de PLADESE/PVIRCE/PAM.

### Secciones

```text
Planeación vinculante
├── Resumen ejecutivo
├── Metas PROSENER
├── Transición y energías limpias
├── PLADESE: escenarios y necesidades
├── Generación / PVIRCE
├── Proyectos, VUPE y convocatorias
├── Red / PAMRNT y PAMRGD
├── Seguimiento físico-financiero
├── Riesgos y decisiones
├── Mapa integral
└── Fuentes, calidad y versiones
```

La navegación será lateral en escritorio, con contadores o semáforos para riesgos, brechas y obras críticas. En móvil se convertirá en navegación compacta sin perder el contexto de edición, corte y filtros.

### Fichas y salidas ejecutivas

- La vista principal abrirá una ficha ejecutiva navegable construida con los mismos datos y etiquetas del tablero.
- La ficha podrá descargarse en PDF 16:9 y PowerPoint, reutilizando el motor institucional de láminas PAM.
- La ficha podrá enviarse por correo a usuarios vigentes del directorio DGMESNIE mediante el servicio SMTP/SendGrid existente.
- El correo y el archivo indicarán edición, corte, periodo, fuente y estado de validación; ninguna cifra provisional se presentará como oficial.
- Más adelante, cada indicador, proyecto y obra tendrá su ficha de detalle con el mismo contrato de exportación y envío.

### Mapa del MVP

El mapa sólo permitirá contexto y consulta:

- encender/apagar GCR, estados y municipios;
- centrales existentes y proyectos identificados;
- proyectos PVIRCE con nivel de precisión visible;
- líneas RNT;
- subestaciones de transmisión y distribución;
- obras PAMRNT/PAMRGD;
- demanda CENACE como contexto operativo;
- seleccionar un elemento y abrir su ficha de trazabilidad.

No se incluirán buffers ni análisis territorial en el MVP.

---

## 10. Diseño técnico del módulo

### Backend propuesto

```text
Controllers/PlaneacionVinculanteController.cs
Servicios/IPlaneacionVinculanteService.cs
Servicios/PlaneacionVinculanteService.cs
Servicios/IRepositorioPlaneacionVinculante.cs
Servicios/RepositorioPlaneacionVinculante.cs
Models/PlaneacionVinculante/*.cs
Views/PlaneacionVinculante/Index.cshtml
Views/PlaneacionVinculante/Fuentes.cshtml
Views/PlaneacionVinculante/Ficha.cshtml
Views/PlaneacionVinculante/Proyecto.cshtml
wwwroot/js/planeacion-vinculante/
wwwroot/css/planeacion-vinculante.css
wwwroot/css/planeacion-vinculante-ficha.css
database/migrations/202608xx_31_planeacion_vinculante_base.sql
```

Endpoints iniciales:

| Método | Ruta | Uso |
|---|---|---|
| GET | `/PlaneacionVinculante` | Vista principal |
| GET | `/PlaneacionVinculante/Fuentes` | Fuentes, calidad y previsualización segura |
| GET | `/PlaneacionVinculante/Ficha` | Ficha ejecutiva navegable y exportable |
| POST | `/PlaneacionVinculante/Ficha/Enviar` | Envío de PDF/PPT generado en cliente a destinatarios validados |
| GET | `/PlaneacionVinculante/Api/Resumen` | KPIs y brechas por corte |
| GET | `/PlaneacionVinculante/Api/Metas` | Matriz año × GCR × tecnología |
| GET | `/PlaneacionVinculante/Api/Proyectos` | Cartera canónica y estados |
| GET | `/PlaneacionVinculante/Api/Procedimientos` | Folios y etapas |
| GET | `/PlaneacionVinculante/Api/DependenciasRed` | Obras habilitantes y bloqueos |
| GET | `/PlaneacionVinculante/Api/Fuentes` | Calidad, evidencia y versiones |

Los endpoints aceptarán `fechaCorte`, `instrumentoVersion`, `gcr`, `anio`, `tecnologia` y `estatus`, según corresponda.

### Requisitos no funcionales

- Consultas parametrizadas y cancelables.
- Paginación para tablas detalladas.
- Caché corta para catálogos y capas; KPIs por corte con invalidación al publicar una carga.
- Carga diferida del mapa y de las series pesadas.
- Toda carga se ejecuta dentro de una transacción y no publica datos hasta pasar validaciones.
- Registro de usuario, fecha, fuente y versión para cambios manuales.
- Exportación ejecutiva PDF/PPT y envío por correo desde la primera portada, reutilizando los componentes institucionales existentes.

---

## 11. Estrategia de implementación por fases

### Fase 0 — definiciones, fuentes y contrato de datos

**Objetivo:** impedir que el tablero institucionalice métricas ambiguas.

Entregables:

- Diccionario de `meta`, `cobertura`, `comprometido`, `ejecutable` y `operación`.
- Diccionario separado de `prevalencia`, `prelación`, `energía limpia`, `renovable` y `almacenamiento`.
- Lista oficial de metas PROSENER/PLADESE a seguir.
- Jerarquía de fuentes: publicación oficial → extracción estructurada versionada → fuente operativa → evidencia interna.
- Reglas de capacidad limpia, renovable y almacenamiento.
- Estados normalizados y reglas de conteo.
- Fecha de corte y responsables de validación.
- Regla de vigencia anual y selección de la foto oficial de línea base.
- Clasificación de información pública, interna o reservada.

**Puerta de salida:** las áreas responsables aprueban por escrito las reglas de conteo.

### Fase 1 — fundación de instrumentos, PVIRCE y escenarios

**Objetivo:** obtener una línea base versionada sin perder el contenido original.

Entregables:

- Migración de instrumento, carga, registro fuente, indicador, serie, meta, mapeos y asignación.
- Versionado anual inmutable y relaciones entre PLADESE, PVIRCE, PAM y PROSENER.
- Carga del Excel del capítulo 4.1 como dos conjuntos: 191 requerimientos agregados y 34 requerimientos de particulares.
- Conciliación documentada de 75,564 MW de adiciones, 1,805 MW de sustituciones y la diferencia 5,970/7,405 MW de la tabla 4.9.
- Perfil separado de las fuentes PVIRCE por granularidad y calidad, sin mezclarlas con los requerimientos agregados: archivo previo de 449 filas, corte base de 459 y borrador de 485.
- Conciliación `PLADESE 2025 oficial ↔ corte base PVIRCE`: la sustitución ya coincide prácticamente (1,805 frente a 1,805.028 MW), mientras que la diferencia de adiciones debe explicarse antes de validar la línea base.
- Libro de cambios `corte base ↔ borrador 2026` con altas, bajas, renombres, reprogramaciones, cambios de capacidad/estatus y evidencia de revisión.
- Segmentación temporal obligatoria: línea base/ejecutado anterior a 2026, cartera de la administración 2026–2030 y continuidad 2031–2040.
- Extracción inicial de los indicadores PROSENER y de las series prioritarias de PLADESE.
- Catálogos de GCR, tecnología, estatus, tipo de necesidad y unidad.
- Clasificación de filas: meta, proyecto, cartera, sustitución, retiro u optimización.
- Primera matriz de metas y brechas sin inventar relaciones.
- Primera foto reproducible `PLADESE 2025–2039` y mecanismo preparado para `2026–2040`.

**Puerta de salida:** 100% de las filas conserva fuente, página y clasificación; cada discrepancia está registrada y los totales reproducibles se concilian con el documento oficial.

### Fase 2 — identidad de centrales y folios VUPE

**Objetivo:** conectar PVIRCE con proyectos concretos y con la instrumentación.

Entregables:

- Vinculación de los 65 registros activos de convocatorias con `core.Proyecto` o una resolución explícita de no coincidencia.
- Identificadores VUPE, permiso, razón social, subestación y punto de interconexión.
- Flujo de candidatos y validación manual.
- Matriz proyecto–meta con MW y nivel de certeza.
- Modelo de procedimiento e hitos.

**Piloto recomendado:** validar primero 10 proyectos de una GCR, incluyendo casos factibles, no viables y con obras onerosas.

**Puerta de salida:** cada MW “identificado” puede abrir el proyecto, el folio y la evidencia que lo respalda.

### Fase 3 — MVP ejecutivo

**Objetivo:** demostrar `meta → cobertura → brecha` sin depender del mapa.

Entregables:

- Nuevo módulo MVC y registro en menú/roles.
- Portada ejecutiva.
- Tarjetas de prevalencia estatal y energías limpias con base/meta/observado/escenario.
- Selector de foto anual y comparación básica entre ediciones.
- Matriz año × GCR × tecnología.
- Embudo de instrumentación.
- Tabla y ficha de proyecto.
- Filtros por versión, corte, año, GCR, tecnología y estatus.
- Filtro de prelación de proyectos separado del indicador de prevalencia.
- Panel de calidad del dato.
- Ficha ejecutiva navegable, exportación PDF/PPT y envío por correo con datos del corte.

**Puerta de salida:** todas las cifras de la portada son reproducibles desde SQL y tienen definición visible.

### Fase 4 — dependencias PAMRNT/PAMRGD y costos

**Objetivo:** calcular qué capacidad es realmente habilitable por red.

Entregables:

- Relación central–subestación–obra PAM.
- Ingestión de requerimientos de red PLADESE 4.9 y 4.23–4.40 como necesidades documentales separadas de las obras confirmadas.
- Conciliación de tablas PLADESE 4.10–4.22 con el catálogo PAM ya integrado.
- Distinción refuerzo/interconexión/expansión.
- Fecha requerida vs. fecha prevista.
- Costos comparables y fuentes.
- Indicador de MW con red compatible, en riesgo o no identificada.
- Flujo de validación de relaciones candidatas.

**Puerta de salida:** ninguna dependencia crítica se considera confirmada sin evidencia o validación técnica.

### Fase 5 — mapa contextual reutilizado

**Objetivo:** visualizar las relaciones ya validadas.

Entregables:

- Capas encendibles de GCR, estados, municipios, centrales, proyectos, líneas, subestaciones y obras PAM.
- Simbología de planeado, contratado, construcción, operación y riesgo.
- Ficha al seleccionar central u obra.
- Aviso visible del nivel de precisión geográfica.

**Puerta de salida:** el mapa no modifica las reglas de conteo ni crea asociaciones implícitas.

### Fase 6 — seguimiento mensual, riesgos y finanzas

**Objetivo:** convertir el tablero en herramienta de gestión.

Entregables:

- Cortes mensuales de generación y dependencias.
- Comparativo programado vs. real.
- Desviaciones de fecha y costo.
- Riesgos, responsables, mitigaciones y decisiones.
- Reporte ejecutivo exportable.

### Fase 7 — escenarios

Se habilitará sólo con historia y calidad suficientes:

- probabilidad de cumplimiento a 2030;
- sensibilidad por retraso de obras críticas;
- proyección de GWh limpios;
- comparación demanda proyectada vs. capacidad ejecutable;
- concentración tecnológica, regional o por participante.

---

## 12. Primer bloque de trabajo recomendado

Orden estricto:

1. Registrar como versiones fuente el PLADESE 2025–2039, el PROSENER 2025–2030, el Excel del capítulo 4.1 y el PAM vigente.
2. Validar definiciones institucionales y aprobar los cuatro indicadores eléctricos PROSENER propuestos.
3. Crear la migración base con fotos anuales, indicadores, series, metas, requerimientos, cobertura de proyectos y registro fuente.
4. Importar las 191 filas de tablas 4.1–4.8 como requerimientos agregados y las 34 filas de tabla 4.9 como requerimientos particulares separados.
5. Publicar el reporte de conciliación, incluida la diferencia 5,970/7,405 MW y los costos faltantes de tabla 4.9.
6. Registrar sin transformar la publicación PLADESE 2025, el corte base `PVIRCE.xlsx` y el borrador `PVIRCE_20260804_vcorta.xlsx`, cada uno con hash, clase y estado de publicación.
7. Conciliar el corte base de 459 filas con PLADESE 2025 y documentar toda diferencia de adiciones, sustituciones, año, GCR y tecnología.
8. Construir el libro de cambios entre el corte base y el borrador: altas, bajas, renombres, año, MW, estatus, CENACE y clasificación macro.
9. Clasificar las fuentes PVIRCE de 449, 459 y 485 filas como meta/proyecto/cartera/retiro/sustitución/optimización, sin sumarlas entre sí.
10. Extraer PROSENER sección 8 y PLADESE capítulos 2, 3, 4.2, 4.3 y anexos prioritarios.
11. Publicar la foto base 2025–2039, `PlaneacionMeta`, `PlaneacionRequerimientoCapacidad`, `PlaneacionIndicadorValor` y las series de prevalencia y energías limpias.
12. Conciliar los proyectos y metas físicas de PLADESE 4.2 con los 285 proyectos PAM vigentes en BD.
13. Crear candidatos de identidad para los 65 folios activos, validar un piloto de 10 proyectos y enlazar sus requerimientos de red.
14. Construir el endpoint `Resumen`, las tarjetas de prevalencia/energías limpias, los selectores de periodo/edición/corte, el comparador, la matriz de cobertura y la ficha antes de integrar el mapa.

Este orden reduce el riesgo principal: una visualización convincente basada en relaciones no verificadas.

---

## 13. Tracking de tareas

Estados permitidos: `PENDIENTE`, `EN_CURSO`, `BLOQUEADA`, `HECHA`, `DESCARTADA`.

| ID | Fase | Tarea | Estado | Evidencia / nota |
|---|---:|---|---|---|
| PV-000 | Descubrimiento | Leer y adaptar el plan recibido | HECHA | Documento fuente revisado el 2026-08-19 |
| PV-001 | Descubrimiento | Auditar componentes reutilizables del tablero territorial | HECHA | Servicios, controladores, migraciones y catálogo revisados |
| PV-002 | Descubrimiento | Auditar BD de PVIRCE | HECHA | Tablas operativa y staging con 0 filas; carga preparada de 449 |
| PV-003 | Descubrimiento | Medir PAM, red, seguimiento y convocatorias | HECHA | Línea base incluida en la sección 4 |
| PV-004 | Arquitectura | Definir separación entre tablero territorial y vinculante | HECHA | ADR-001 |
| PV-005 | Arquitectura | Definir BD institucional como fuente de KPI | HECHA | ADR-002 |
| PV-006 | Arquitectura | Definir identidad canónica de proyecto | HECHA | ADR-003 |
| PV-007 | Descubrimiento | Perfilar Excel `PLADESE Cap 4.1 TABLAS.xlsx` | HECHA | 12 hojas; 191 filas 4.1–4.8 y 34 filas 4.9; controles documentados |
| PV-008 | Descubrimiento | Catalogar contenido oficial PLADESE 2025–2039 | HECHA | SIDOF/DOF contiene capítulos 1–4 y anexos completos |
| PV-009 | Descubrimiento | Localizar PROSENER oficial y sus metas | HECHA | DOF 22-12-2025; 6 indicadores, 4 de alcance eléctrico directo |
| PV-010 | Fase 0 | Aprobar definiciones de meta, cobertura y cumplimiento | PENDIENTE | Requiere áreas responsables |
| PV-011 | Fase 0 | Aprobar indicadores PROSENER del tablero | PENDIENTE | Propuesta: 1.2, 2.1, 3.1 y 3.2; 2.2 como contexto |
| PV-012 | Fase 0 | Aprobar catálogo de estatus y reglas de conteo | PENDIENTE | Debe preceder los KPI |
| PV-013 | Fase 0 | Definir corte, frecuencia y responsables | PENDIENTE | Propuesta: corte mensual |
| PV-014 | Fase 0 | Clasificar información pública/interna/reservada | PENDIENTE | Requisito de acceso y exportación |
| PV-015 | Fase 0 | Aprobar jerarquía y versiones de fuentes | PENDIENTE | DOF/SIDOF como fuente primaria; Excel como extracción trazable |
| PV-016 | Fase 0 | Aprobar tratamiento de discrepancias documentales | PENDIENTE | No ocultar diferencias ni forzar totales |
| PV-017 | Fase 0 | Aprobar definición de prevalencia y prelación | PENDIENTE | Prevalencia = participación estatal; prelación = condición de proyecto |
| PV-018 | Arquitectura | Definir ediciones y cortes inmutables | HECHA | ADR-005; publicación oficial, corte base, borradores y seguimiento separados |
| PV-019 | Arquitectura | Revisar recomendaciones de normalización y seguimiento | HECHA | ADR-006; separar metas, requerimientos, series y supuestos |
| PV-020 | Descubrimiento | Comparar corte base y borrador PVIRCE 2026 | HECHA | 459/24 frente a 485/36; 369 coincidencias canónicas, 141 nombres compartidos con cambios; diagnóstico separado |
| PV-021 | Arquitectura | Definir PVIRCE acumulativo y seguimiento entre cortes | HECHA | ADR-005 v0.6; publicación, línea base y borrador se conservan por separado |
| PV-022 | Fase 0 | Validar línea base PVIRCE contra PLADESE 2025 | EN_CURSO | Sustituciones concilian; falta explicar diferencia de adiciones y cambios de horizonte |
| PV-023 | Fase 0/2 | Resolver identificador estable y folio VUPE | PENDIENTE | El borrador no contiene ID/folio y `Nombre real` no es único |
| PV-024 | Fase 1 | Separar ejecutado/base, cartera 2026–2030 y largo plazo | PENDIENTE | Vista principal de la administración 2025–2030; 2031–2040 queda disponible |
| PV-025 | Fase 1 | Validar MW instalados contra MW de interconexión | PENDIENTE | 38 filas difieren; brecha neta 424.66 MW en el borrador |
| PV-026 | Fase 1/2 | Reparar integración de estatus CENACE | PENDIENTE | 472 fórmulas apuntan a `#REF!`; sólo 12 valores visibles |
| PV-027 | Fase 0/1 | Validar clasificación macro del borrador | PENDIENTE | 125 filas/33,232.548 MW sin `Status Macro Final`; no usar como prevalencia jurídica |
| PV-100 | Fase 1 | Crear migración de fundación de planeación | HECHA | `20260819_31_planeacion_vinculante_fundacion.sql`: instrumentos, versiones, relaciones, cargas, registros fuente y cambios; creada e idempotente, pendiente de aplicar a BD compartida |
| PV-101 | Fase 1 | Importar tablas PLADESE 4.1–4.9 | PENDIENTE | Dos cargas: 191 requerimientos agregados y 34 requerimientos particulares |
| PV-102 | Fase 1 | Crear reporte de conciliación capítulo 4.1 | PENDIENTE | Incluye 75,564/1,805 MW y diferencia 5,970/7,405 MW |
| PV-103 | Fase 1 | Crear catálogos y mapeos homologados | PENDIENTE | Tecnología, GCR, estatus y unidad |
| PV-104 | Fase 1 | Publicar primera línea base de metas | PENDIENTE | Requiere validación de totales |
| PV-105 | Fase 1 | Importar y clasificar las fuentes PVIRCE | PENDIENTE | Cargas separadas de 449, 459 y 485 filas; no sobrescribir ni sumar versiones |
| PV-106 | Fase 1 | Extraer indicadores y trayectorias PROSENER | EN_CURSO | Controles de 1.2, 2.1, 3.1 y 3.2 ya visibles; faltan ficha completa, fórmula, metas anuales, responsable y resultados observados versionados |
| PV-107 | Fase 1 | Extraer línea base y series PLADESE | PENDIENTE | Capítulos 2, 3 y anexo A1 |
| PV-108 | Fase 1 | Extraer costos y supuestos tecnológicos | PENDIENTE | Tablas 3.1–3.2 y anexos A2–A3 |
| PV-109 | Fase 1/4 | Extraer tablas PLADESE 4.10–4.40 | PENDIENTE | PAM, metas físicas, proyectos, refuerzos y costos vinculantes |
| PV-110 | Fase 1 | Implementar relación entre versiones de instrumentos | PENDIENTE | Integra/actualiza/sustituye/vigente-con |
| PV-111 | Fase 1 | Construir foto base PLADESE 2025–2039 | PENDIENTE | Reproducible e inmutable |
| PV-112 | Fase 1 | Implementar comparación entre fotos anuales | PENDIENTE | Altas, bajas, cambios de MW, año, tecnología y costo |
| PV-113 | Fase 1 | Estructurar indicador jurídico de prevalencia estatal | PENDIENTE | Estado/total anual; base 55.40%, piso 54%, escenario y resultado oficial |
| PV-114 | Fase 1 | Estructurar indicador de energías limpias | PENDIENTE | GWh/TWh y porcentaje separados de capacidad MW |
| PV-115 | Fase 1/2 | Clasificar proyectos con prelación | PENDIENTE | Fuente, edición, contrato y vigencia obligatorios |
| PV-116 | Fase 1 | Implementar universo estatal reglamentario | PENDIENTE | EPE, participación proporcional, otras entidades y gobiernos subnacionales |
| PV-117 | Fase 1 | Registrar evaluación anual oficial de SENER | PENDIENTE | Separada de estimaciones mensuales provisionales |
| PV-118 | Fase 1 | Implementar metas y requerimientos separados | PENDIENTE | Meta, requerimiento capacidad, requerimiento red, escenario y supuesto |
| PV-119 | Fase 1 | Implementar control de capacidad de cobertura | PENDIENTE | MW asignados; firme/neta nullable; método, certeza y validación |
| PV-120 | Fase 1 | Implementar vigencia y auditoría temporal | PENDIENTE | Vigencia, corte, publicación, registro y actualización |
| PV-121 | Fase 1 | Implementar previsualización segura de PVIRCE | HECHA | Lectura XLSX en memoria, SHA-256, perfil, fórmulas y puerta de calidad; validada contra los cortes reales de 459 y 485 registros |
| PV-122 | Fase 1/3 | Crear vista independiente de Planeación Vinculante | HECHA | `/PlaneacionVinculante`: portada ejecutiva por defecto, sin formulario de carga y sin modificar el tablero territorial |
| PV-123 | Fase 1/3 | Integrar resumen operativo de BD en modo lectura | HECHA | La portada consulta PAM vigente, inversión, red activa, inventario de subestaciones y folios de convocatoria sin escribir en SQL |
| PV-124 | Fase 1/3 | Separar fuentes y cargas del tablero ejecutivo | HECHA | `/PlaneacionVinculante/Fuentes`: acción, modelo y vista dedicados; el POST de previsualización regresa a esa vista y no escribe en BD |
| PV-125 | Diseño | Incorporar especificación integral de 11 pestañas | HECHA | Navegación objetivo y prioridad tomadas del mockup/especificación del 19-ago-2026; PROSENER ocupa la primera franja y PLADESE la segunda |
| PV-126 | Fase 3 | Construir armazón visual de 11 secciones | HECHA | Navegación lateral responsive con contraste accesible, filtros globales, acción de ficha y contenido a la derecha; diseño plano sin degradados y secciones sin datos marcadas sin cifras simuladas |
| PV-200 | Fase 2 | Crear candidatos folio ↔ `core.Proyecto` | PENDIENTE | 65 activos; 0 vinculados hoy |
| PV-201 | Fase 2 | Validar piloto de 10 proyectos | PENDIENTE | Una GCR y casos diversos |
| PV-202 | Fase 2 | Completar matriz proyecto–meta | PENDIENTE | MW y certeza obligatorios |
| PV-203 | Fase 2 | Modelar procedimiento e hitos VUPE | PENDIENTE | Convocatoria, fallo, permiso y contrato |
| PV-300 | Fase 3 | Crear controlador, servicio y repositorio | EN_CURSO | Controlador y servicio de previsualización creados; repositorio se incorpora al habilitar el registro confirmado |
| PV-301 | Fase 3 | Crear APIs de resumen, metas y proyectos | PENDIENTE | Cálculo backend |
| PV-302 | Fase 3 | Construir portada ejecutiva | EN_CURSO | Resumen funcional dentro del armazón lateral con meta oficial, cortes PVIRCE, PROSENER explícito, horizonte 2025–2030, VUPE y PAM; falta sustituir controles documentales por cargas versionadas SQL |
| PV-303 | Fase 3 | Construir matriz y ficha de proyecto | PENDIENTE | Drill-down a evidencia |
| PV-304 | Fase 3 | Registrar menú y roles | PENDIENTE | Sección Seguimientos/Planeación |
| PV-305 | Fase 3 | Crear selector y comparador de fotos anuales | PENDIENTE | Vigente por defecto; histórico accesible |
| PV-306 | Fase 3 | Crear bloque de metas eléctricas PROSENER | EN_CURSO | 1.2, 2.1, 3.1 y 3.2 visibles como propuesta directa; 2.2 separado como contexto; faltan aprobación, fichas completas, series observadas y evaluación anual oficial |
| PV-307 | Fase 3 | Reutilizar fichas, exportación y correo para Planeación | EN_CURSO | Vista `/PlaneacionVinculante/Ficha`, PDF y PPT reales de 5 láminas, modal y directorio validados en navegador. Pendiente únicamente un envío controlado autorizado |
| PV-400 | Fase 4 | Crear modelo de dependencia de red | PENDIENTE | Central ↔ PAM/nodo/obra |
| PV-401 | Fase 4 | Importar requerimientos y validar dependencias conocidas | PENDIENTE | Tabla 4.9 y 4.23–4.40; no aplicar sólo por proximidad |
| PV-402 | Fase 4 | Crear modelo de costos | PENDIENTE | Refuerzo/interconexión/contrato |
| PV-403 | Fase 4 | Calcular compatibilidad de fechas | PENDIENTE | Fecha central vs. fecha red |
| PV-404 | Fase 4 | Conciliar PLADESE 4.2 con catálogo PAM | PENDIENTE | Identidad, metas físicas, costo, estatus y versión |
| PV-405 | Fase 4 | Implementar dependencias con llaves reales | PENDIENTE | FK a proyecto/requerimiento/PAM/nodo/subestación/arista y `CHECK` |
| PV-500 | Fase 5 | Integrar mapa de capas reutilizadas | PENDIENTE | Sin análisis geoespacial |
| PV-501 | Fase 5 | Crear fichas de central y obra desde mapa | PENDIENTE | Incluye nivel de precisión |
| PV-600 | Fase 6 | Crear corte mensual transversal | PENDIENTE | Referenciar seguimiento PAM existente |
| PV-601 | Fase 6 | Crear riesgos y decisiones | PENDIENTE | MW/monto impactado y responsable |
| PV-602 | Fase 6 | Integrar costos y avance financiero | PENDIENTE | Sin sumar bases incompatibles |
| PV-603 | Fase 6 | Crear seguimiento transversal por entidad | PENDIENTE | Encabezado de corte + detalle con exactamente una FK real |
| PV-604 | Fase 6 | Implementar fechas semánticas y desviaciones | PENDIENTE | Objetivo/necesaria/contrato/estimada/real/corte |
| PV-700 | QA | Pruebas de reconciliación de totales | PENDIENTE | SQL vs. documento fuente |
| PV-701 | QA | Pruebas de reglas de conteo | PENDIENTE | Casos cancelado/reprogramado/sin fallo |
| PV-702 | QA | Validación de seguridad y roles | PENDIENTE | Datos internos y exportaciones |
| PV-703 | QA | Validación ejecutiva con usuarios | PENDIENTE | Lectura en menos de dos minutos |
| PV-704 | QA | Prueba de humo de previsualización PVIRCE | HECHA | 459/73,238.048540 MW y 485/81,798.278540 MW reproducidos; 472 `#REF!` y 38 diferencias detectadas |

---

## 14. Riesgos del proyecto

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Confundir filas PVIRCE agregadas con centrales | KPI falso y proyectos duplicados | Staging íntegro, clasificación y validación |
| Relacionar por nombre o proximidad | Cobertura y red ficticias | Identificadores, evidencia y aprobación humana |
| Usar CSV remoto para cifras oficiales | Cambios sin historia ni auditoría | Ingestión versionada a SQL |
| Mezclar demanda horaria con proyección PLADESE | Comparación metodológicamente inválida | Series y fuentes separadas |
| Sobrescribir la edición anterior con el PLADESE/PVIRCE/PAM nuevo | Se pierde la foto institucional y no puede explicarse qué cambió | Versiones anuales inmutables y comparador de cambios |
| Confundir prevalencia con prelación | KPI de política mezclado con estatus de proyectos privados | Indicador y catálogo separados |
| Tratar una estimación mensual como evaluación jurídica de prevalencia | Declaración de cumplimiento sin el cálculo anual de SENER | Etiqueta provisional y registro separado del resultado oficial anual |
| Limitar el numerador estatal sólo a CFE | Porcentaje contrario a la definición reglamentaria | Catálogo de titularidad/participación estatal y cálculo proporcional |
| Confundir MW limpios con porcentaje de generación limpia | Apariencia falsa de cumplimiento de la meta | MW y GWh/TWh en visuales y fórmulas distintas |
| Mezclar meta, requerimiento, escenario y línea base | Fórmulas y estados de cumplimiento ambiguos | Entidades normalizadas y clasificación obligatoria en staging |
| Usar relaciones polimórficas sin FK | Referencias huérfanas y consultas frágiles | Columnas explícitas, llaves foráneas y restricciones `CHECK` |
| Usar una sola FEO o fecha genérica | Desviaciones contractuales y de planeación incorrectas | Fechas con semántica y fuente preservada |
| Confundir vigencia, publicación y corte | Comparaciones históricas inconsistentes | Temporalidad de negocio y auditoría de sistema separadas |
| Sumar costos incompatibles | Inversión sobreestimada | Tipo de monto, moneda, base, IVA y corte |
| Extender la vista territorial monolítica | Alta deuda y regresiones | Módulo independiente |
| Presentar convocatoria como contrato | Sobreestimación de cobertura | Catálogo de etapas y reglas backend |
| Intentar integrar todo antes del primer resultado | Retraso y baja validación | Cortes verticales y puertas de salida |

---

## 15. Decisiones institucionales pendientes

Decisiones funcionales ya confirmadas:

- El PVIRCE oficial de línea base es el integrado en el PLADESE 2025–2039; `PVIRCE.xlsx` se conservará como primer corte de trabajo 2026 y deberá conciliarse contra esa publicación.
- `PVIRCE_20260804_vcorta.xlsx` se mostrará como borrador en revisión, nunca como cifra oficial mientras no se publique y valide.
- El seguimiento requerido es el movimiento entre fotos: altas, bajas, renombres, reprogramaciones, MW, estatus y clasificación.
- La vista predeterminada cubrirá los años de planeación de la administración 2025–2030; 2024 será línea base y 2031–2040 continuidad de largo plazo.

1. ¿Se aprueba como línea base el PLADESE 2025–2039 publicado en DOF el 17 de octubre de 2025?
2. ¿Se aprueba vincular directamente PROSENER 1.2, 2.1, 3.1 y 3.2, dejando 2.2 como contexto y 1.1 fuera del alcance eléctrico?
3. ¿Cuándo un MW se considera comprometido: fallo, permiso, contrato o una combinación?
4. ¿Quién valida la asignación proyecto–meta?
5. ¿Quién valida la dependencia central–obra de red?
6. ¿Qué costos pueden mostrarse y a qué nivel de acceso?
7. ¿Qué fuente define la fecha oficial de entrada en operación?
8. ¿Qué nivel de desagregación de demanda PLADESE será oficial para seguimiento: SEN, sistema, GCR o región de transmisión?
9. ¿En qué fecha una nueva edición anual sustituye a la vigente para los KPI, conservando la edición anterior como foto histórica?
10. ¿En qué publicación o conjunto de datos de SENER se incorporará la evaluación anual oficial de prevalencia, usando CENACE como insumo cuando corresponda?
11. ¿Qué catálogo normativo definirá por edición las tecnologías limpias, renovables y el tratamiento del almacenamiento?

Estas preguntas no bloquean el diseño de la migración de staging, pero sí bloquean la publicación de KPIs de cumplimiento.

---

## 16. Criterios de aceptación del MVP

- Puede seleccionarse una versión del instrumento y una fecha de corte.
- La pantalla distingue sin ambigüedad el PVIRCE oficial integrado al PLADESE, el primer corte de trabajo 2026 y el borrador en revisión.
- El periodo inicial es 2025–2030 y separa ejecutado/base anterior a 2026, cartera 2026–2030 y continuidad 2031–2040.
- Una nueva edición anual no cambia los totales ni las filas de la foto anterior.
- Puede compararse publicación, línea base, corte o edición y explicar altas, bajas, renombres, reprogramaciones y cambios de MW/estatus.
- Ningún KPI suma filas de dos cortes del mismo PVIRCE.
- Cada KPI expone definición, numerador, denominador y fuente.
- Prevalencia muestra Estado y particulares sobre el mismo total, piso 54%/máximo complementario 46%, escenario, evaluación anual oficial, estimación provisional y margen.
- El cálculo jurídico anual incluye todo el universo estatal definido en el Reglamento y no sólo CFE.
- Ningún corte mensual se etiqueta como cumplimiento jurídico definitivo.
- Energías limpias distingue participación de generación en GWh/TWh de composición de capacidad en MW.
- Prelación aparece como condición del proyecto y nunca como sinónimo de prevalencia.
- Metas, requerimientos de capacidad, requerimientos de red, escenarios, líneas base y supuestos conservan semántica separada.
- Cada MW asignado a un proyecto expone método, nivel de certeza, validación y fuente; MW firme o neto sin metodología permanece sin dato.
- Toda dependencia de red tiene al menos una FK real de origen y una FK real de red.
- Las desviaciones distinguen fecha objetivo, necesaria, contractual, estimada y real.
- Vigencia del dato, publicación de la fuente y corte operativo pueden consultarse por separado.
- La suma de metas coincide con el documento oficial validado.
- Cada MW identificado baja a una asignación proyecto–meta.
- Cada proyecto muestra folio/identificador, estatus y evidencia disponible.
- Las convocatorias sin fallo no aparecen como contratadas.
- La ausencia de red identificada se muestra como brecha de información.
- El mapa puede apagarse sin impedir la lectura ejecutiva.
- Ninguna capa geográfica crea relaciones de negocio por sí sola.
- El historial permite explicar qué cambió entre dos cortes.

---

## 17. Registro de cambios

| Fecha | Cambio | Autor |
|---|---|---|
| 2026-08-19 | Creación del plan, arquitectura, línea base y backlog inicial | Codex |
| 2026-08-19 | Versión 0.2: perfil del Excel 4.1, fuentes oficiales PLADESE/PROSENER, modelo de indicadores/series, discrepancias y backlog de extracción | Codex |
| 2026-08-19 | Versión 0.3: fotos anuales inmutables, comparador entre ediciones, prevalencia estatal, energías limpias y separación de prelación | Codex |
| 2026-08-19 | Versión 0.4: precisión constitucional, legal y reglamentaria de prevalencia; Estado/particulares, universo estatal y evaluación anual oficial | Codex |
| 2026-08-19 | Versión 0.5: separación física de metas y requerimientos, control de capacidad, llaves reales, fechas semánticas y seguimiento transversal | Codex |
| 2026-08-19 | Versión 0.6: PVIRCE acumulativo, línea base contra PLADESE 2025, borrador 2026 en revisión, comparador entre cortes y periodo de administración 2025–2030 | Codex |
| 2026-08-19 | Versión 0.7: migración de fundación, previsualizador PVIRCE validado con los dos cortes reales y nueva vista diagnóstica independiente; sin aplicar aún cambios a la BD compartida | Codex |
| 2026-08-19 | Versión 0.8: portada ejecutiva como vista inicial, navegación por metas/PVIRCE/red/fuentes y resumen operativo de PAM, red, subestaciones y convocatorias leído desde BD | Codex |
| 2026-08-19 | Versión 0.9: fuentes y cargas separadas en una ruta dedicada; bloque PROSENER 2025–2030 con cuatro indicadores eléctricos y 2.2 como contexto; nomenclatura corregida entre PVIRCE oficial y cortes de trabajo 2026 | Codex |
| 2026-08-19 | Versión 1.0: incorporada la especificación de 11 pestañas y la jerarquía PROSENER → PLADESE → PVIRCE/PAM → ejecución; ficha navegable con exportación PDF/PPT y envío por correo reutilizando el flujo institucional | Codex |
| 2026-08-19 | Validación de la ficha: exportaciones PDF y PPT institucionales de 5 láminas verificadas; se validaron navegación, modal de envío y carga del directorio sin enviar correo real | Codex |
| 2026-08-19 | Armazón del mockup implementado: 11 secciones en navegación lateral responsive, filtros globales y Resumen ejecutivo conservando los módulos PROSENER, PVIRCE y red ya funcionales | Codex |
| 2026-08-19 | Ajuste visual validable: eliminados todos los degradados del CSS del tablero, reforzado el contraste blanco sobre guinda y aclarada la clasificación PROSENER de 4 indicadores directos + 1 contextual | Codex |

## 18. Referencias del repositorio

- `DASHBOARD_PROYECTOS_INVENTARIO_Y_PLAN.md`
- `database/migrations/20260819_31_planeacion_vinculante_fundacion.sql`
- `planes/planeacion-vinculante/DIAGNOSTICO_PVIRCE_CORTES_2026.md`
- `Documentacion/PVIRCE2026-2040_SQ.xlsx`
- `C:/Users/User/Downloads/PLADESE Cap 4.1 TABLAS.xlsx` (insumo externo; pendiente de incorporación versionada)
- `https://sidof.segob.gob.mx/notas/docFuente/5770297` (PLADESE 2025–2039 oficial)
- `https://diariooficial.gob.mx/abrirPDF.php?anio=2025&archivo=22122025-MAT.pdf&repo=` (PROSENER 2025–2030 oficial, páginas 5–49 del ejemplar)
- `https://www.diputados.gob.mx/LeyesBiblio/pdf/LPTE.pdf` (artículo 32: actualización anual del PLADESE)
- `https://www.dof.gob.mx/nota_detalle.php?codigo=5769157&fecha=03/10/2025` (Reglamento LPTE: instrumentos, prevalencia y proyectos PVIRCE/PAM)
- `https://www.diputados.gob.mx/LeyesBiblio/pdf/CPEUM.pdf` (artículo 27: no prevalencia de particulares sobre la empresa pública del Estado)
- `https://www.diputados.gob.mx/LeyesBiblio/pdf/LSE.pdf` (artículo 12, fracción VI: mínimo estatal de 54% anual)
- `https://www.diputados.gob.mx/LeyesBiblio/regley/Reg_LSE.pdf` (artículos 2 y 9: universo, fórmula y evaluación anual de prevalencia)
- `Documentacion/DGMESNIE_PVIRSE_CARGA_INICIAL.sql`
- `Servicios/PvirseImportService.cs`
- `Servicios/RepositorioInversionDesarrolloEnergetico.cs`
- `Controllers/DashboardProyectosController.cs`
- `Controllers/PamTerritorialController.cs`
- `Controllers/RedElectricaGraphController.cs`
- `Controllers/RedElectricaSubstationInventoryController.cs`
- `database/migrations/20260710_01_pam_trazabilidad_inicial.sql`
- `database/migrations/20260726_19_red_electrica_grafo.sql`
- `database/migrations/20260728_20_red_electrica_inventario_subestaciones.sql`
- `database/migrations/20260804_27_cartera_convocatoria_seguimiento.sql`
- `database/migrations/20260810_29_pam_seguimiento_transmision.sql`
- `wwwroot/tablero/datasets-catalog.json`
