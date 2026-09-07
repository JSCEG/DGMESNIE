# Dashboard territorial: inventario de capas y plan de visualización

Inventario inicial: 18 de agosto de 2026. Seguimiento de Mixtos II actualizado al 5 de septiembre de 2026; las validaciones históricas no sustituyen el cierre de la etapa documental actual.

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
| 2 | Generación privada planeada | Integrada; capa temporalmente comentada | CNE/SENER, 1ª Convocatoria de Atención Prioritaria, compilado por Atlas SEN | 18 proyectos asociados a 20 permisos; algunas ubicaciones son aproximadas. No se expone durante la revisión de Mixtos II |
| 3 | Vacíos de red · 2ª convocatoria | Integrada; control heredado aún visible | Evidencia de la Segunda Convocatoria | Coincidencias propuestas; no forman parte del grafo oficial sin validación. Ocultar capa y botón de revisión está pendiente de confirmar |
| 4 | Cartera Mixtos II · firmes | Integrada como dataset interno | BD `dgmesnie.CarteraConvocatoriaProyecto`, fuente `Mixtos II` | Sólo proyectos firmes en el dashboard; revisión/no va permanecen en seguimiento. Coordenadas y KML del corte, prelación, historial y expediente por folio |
| 5 | Transmisión planeada | Pendiente de contrato de datos | Plan SEN / fuente oficial aplicable | Obras futuras separadas de la red existente |

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

## Seguimiento de implementación · Mixtos II

### Corte 4 de septiembre de 2026

- [x] Identificada la fuente viva `Mixtos II - 03092026_V4.xlsx` y sus 291 registros de catálogo.
- [x] Definida `Considerar` como llave rectora: `1` es firme; los vacíos se separan entre revisión y no va mediante `ESTATUS UNIVERSO`.
- [x] Creado el catálogo de decisión `firme`, `revisión` y `no va`, con historial por carga y huella SHA-256 para impedir duplicados exactos.
- [x] Implementada la carga transaccional del Excel, incluida la detección de altas, bajas y cambios de consideración entre cortes.
- [x] Integradas coordenadas de proyecto y subestación, referencias KML/KMZ, costos agregados y conteos de obras del catálogo. El desglose documental se incorpora en la etapa de expediente.
- [x] Sustituido el mapa esquemático de la Segunda Convocatoria por mapa Leaflet con encendido de geometrías bajo demanda.
- [x] Expuesta en el dashboard territorial únicamente la cartera firme por defecto, conservando revisión y no va en la sección de seguimiento.
- [x] Validada la carga V4 en desarrollo: 248 firmes, 4 en revisión, 39 no va; 166 KML de proyecto, 291 KML de subestación y 289 ubicaciones utilizables.
- [x] Validada compilación completa sin advertencias ni errores.
- [x] Validar visualmente filtros, ficha, carga repetida y geometrías KML en navegador.
- [x] Publicar la capa `Cartera Mixtos II · firmes` dentro de `Planeación y expansión` en el dashboard territorial.
- [x] Registrar la baja instruida por CENACE para `CFE-CM2-0320-2026` —Ixtaltepec—: pasa de `firme/continúa` a `no-va/no-continúa`, con historial; universo firme vigente: 247 proyectos.
- [x] Agregar análisis por GCR, clasificación y tecnología arriba del mapa de Segunda Convocatoria; las tres visualizaciones obedecen y aplican los filtros de la vista.
- [x] Habilitar búsqueda nacional por folio y nombre de los proyectos firmes Mixtos II aunque la capa se encuentre apagada.
- [x] Añadir acciones `Ver ficha` y `Añadir al análisis` en el popup y panel del proyecto; replica la lógica PAM: fija el proyecto como objetivo, abre las capas recomendadas y permite ajustar la selección antes de correr.
- [x] Crear ficha individual navegable con proyecto, localización, KML, interconexión, obras, costos y trazabilidad; exportable en PDF y PowerPoint.
- [x] Suprimir marcadores masivos cuando un KML codifica una geometría como vértices y reconstruir el trazo/polígono cuando sea necesario.
- [x] Sustituir MapTiler y el fondo satelital anterior por OpenStreetMap, CARTO y Esri, sin API key.
- [x] Compilar el corte integrado con 0 advertencias y 0 errores y reiniciar el servicio local en el puerto 5099.
- [ ] Revalidar visualmente en navegador las gráficas, ficha, búsqueda y fondos del corte actual; la conexión SQL se recuperó el 5 de septiembre y la revisión está en curso.
- [x] Habilitar `Enviar` en la ficha individual con el catálogo institucional de usuarios del PAM, validación del folio firme, límite de 20 MB y comprobación del contenido PDF/PPTX.
- [x] Validar las ligas KML de `CFE-CM2-0400-2026`: Ventanilla responde KML/XML con 504 polígonos; proyecto y subestación comparten la misma liga, por lo que la vista deduplica el dibujo sin modificar la fuente.
- [ ] Acordar el tratamiento institucional de los cuatro registros con `Considerar` vacío y `ESTATUS UNIVERSO = CONSIDERADO`.

### Ficha Mixtos II · datos propios y análisis completo · 4 de septiembre de 2026

- [x] Ficha construida desde el registro vigente en BD: sólo fuente Mixtos II y `Considerar = firme`. Los demás registros se conservan para seguimiento.
- [x] Panorama nacional y aporte del proyecto a su GCR calculados desde BD, sin fijar 247 en el código. Corte probado: 247 firmes / 37,506.98 MW. Los MW son capacidad de cartera, no generación observada ni demanda cubierta.
- [x] Integrar clasificación, factibilidad del Excel, decisión, observaciones, promovente, grupo, municipio, firma, corte e historial; corregir el mapeo de subestación del repositorio.
- [x] Mostrar obras y costos de red MDP/MDD, costo por MW y relación MXN/USD implícita (no tipo oficial); variante sin costos. Reportar cobertura de datos: 42/247 con costos y obras. Sumas de obras aún no deduplicadas contra PAM.
- [x] Reutilizar el motor geoespacial y la exportación PAM: KML completo, punto alternativo del corte, buffer, red, municipios, ambiente, permisos, energía y contexto INEGI. Exportar espera el análisis real y reporta fuentes pendientes.
- [x] Completar en Mixtos II las capas del selector: RAN, atlas/lenguas/localidades indígenas, Ruta Wixárika, generación distribuida y divisiones tarifarias; no activar estas cargas adicionales en fichas PAM.
- [x] Conectar marcador / lista / ficha con `Elegir capas de análisis` en el dashboard; el reporte vuelve a la ficha del mismo folio. Prueba intensiva del ejemplo: 28 capas y 102 elementos territoriales.
- [x] Pausar la capa de primera convocatoria y las fuentes tabulares anteriores de segunda convocatoria/GAT, sin borrar registros. Persisten dos controles de evidencia de segunda convocatoria en el dashboard; su ocultamiento está pendiente de confirmar.
- [x] Pruebas de geometría en `tests/convocatoria-geometria.test.cjs`: coordenadas nulas, KML no disponible/vacío, conservación de componentes, caché, reintentos y limpieza de descripciones.
- [x] Compilar en `compilacion/mixtos-ficha` con 0 advertencias / 0 errores y levantar con `dotnet run` en 7105, 5154 y 5099.
- [x] Cerrar comprobación final PDF/PPT, variantes de costo y diálogo de destinatarios; evidencias locales en `output/playwright`. SAN ISIDRO II: 29 láminas, 32/32 fuentes consultadas; PDF 9,418,311 bytes y PPTX 8,045,246 bytes / ZIP íntegro. No se envió correo en esta validación.
- [x] Incorporar PAM y PODECOBI al selector territorial y a la ficha; excluir referencias PAM sólo regionales de los cruces. Mostrar coincidencias como candidatas por validar, sin sumar sus costos al proyecto.
- [x] Agregar detalle paginado completo de indicadores y desgloses INEGI disponibles, permisos eléctricos/GN/GLP/petrolíferos, subestaciones y líneas. Selector de secciones, flechas y retorno a la sección original tras exportación.
- [x] Recorrer las 29 láminas; corregir recortes detectados en costos y fuentes. Mapas comprobados visualmente en el PDF y en navegador; capas PAM/PODECOBI/RAN/localidades con control de encendido.
- [x] Validar ATLACOMULCO SOLAR `CFE-CM2-0385-2026`: 665.2702 MDP, 35.5759 MDD, relación implícita 18.7000 MXN/USD y 10 obras. El campo descriptivo contiene sólo `10`: se informa ausencia de desglose, sin fabricar precios unitarios. Variante sin costos verificada; Ixtaltepec permanece fuera de la ficha firme (404).
- [x] Complementar en el expediente documental los campos antes omitidos: permiso/liga, datos de almacenamiento existentes, evaluaciones sociales/ambientales, excluyentes, tipo/fuente de cambio y desglose por obra. No se completa con supuestos la fecha cambiaria, las unidades o valores ausentes de almacenamiento/inversión ni las seis solicitudes SAEE sin llave; ver pendientes de la etapa documental.
- [ ] Conciliar las obras reportadas con obras canónicas PAM y documentar costos compartidos antes de sumar inversión vinculante.

La proximidad geoespacial no acredita afectación social, permiso ambiental, exclusión jurídica ni capacidad de interconexión. Los datos ausentes se presentan como pendientes, no como cero ni como cumplimiento.

Graphify V4 completado: 1,560 archivos revisados, salida 0 y sin cambios de topología. Conserva `graphify-out/graph.json` y `GRAPH_REPORT.md`, con 16,138 nodos, 31,675 aristas y 1,764 comunidades. La visualización HTML se omitió por superar el límite de 5,000 nodos.

### Expediente documental Mixtos II · continuación del 4 de septiembre

Auditoría y seguimiento detallado: [Expediente y ficha](planes/mixtos-ii/20260904-auditoria-expediente.md).

- [x] Confirmar que las hojas BD sí contienen el desglose CFE, evaluación social, factibilidad y documentos omitidos por el primer importador.
- [x] Complementar 291 expedientes desde siete hojas; conservar todos los registros por folio y 362 obras. No cambiar Considerar: 247 firmes e Ixtaltepec no va.
- [x] Separar evaluación social documental del análisis geoespacial; incluir riesgos, EVIS/MISSE, MIA, comunidades, consulta, excluyentes y observaciones completas.
- [x] Incorporar tipo de cambio documentado, unidades USD/MXN/MDD/MDP y detalle de costos; no sumar nuevamente costo de red y total de obras.
- [x] Incorporar documentos/KML y 17 calculadoras originales con descarga autenticada.
- [x] Sustituir encabezados explicativos por títulos institucionales, y los iconos ausentes por los mismos de PAM.
- [x] Agregar versión documental 2 con fechas ISO y advertencias; carga de 291 versiones y repetición idempotente con cero inserciones. La cartera y sus decisiones permanecieron sin cambios.
- [x] Homologar barra, modal y controles horizontales con PAM: cuatro botones principales, índice en lugar de selector exclusivo y estilos compartidos. Comparación estática aprobada; costos/capas conservados como acciones auxiliares.
- [x] Implementar índice paginado dinámico y contraportada usando clases PAM. Prueba sintética y navegación real aprobadas: San Isidro 47 láminas/42 entradas y Los Nogales 51 láminas/46 entradas; todos los destinos válidos y retorno al índice correcto.
- [x] Validar estructuralmente el adjunto Mixtos II, con mínimo de dos páginas PDF. Corte del 5 de septiembre: 17 comprobaciones smoke y 13 pruebas Node aprobadas; serialización MIME SMTP y SendGrid conserva 9,418,311 bytes del PDF de referencia. No hubo envío SMTP real.
- [x] Activar hot reload con `dotnet watch` / `dotnet run` en HTTPS 7105 y HTTP 5154/5099. Compilación posterior: cero advertencias y cero errores.
- [x] Validar controles e índices de San Isidro y Los Nogales, fuentes de iconos cargadas y ausencia de superposición cuerpo/pie. Mapa OSM de Los Nogales visible; documentos ME/LX/MA, permiso, KML y calculadora presentes.
- [x] Validar Los Nogales sin costos: 48 láminas, sin tres láminas financieras, importes 296.64/15.86 ni inversión social. Conserva obras y no incluye prelación.
- [x] Generar y revisar PDF actual de San Isidro: 13,277,723 bytes, 47 páginas, 84 enlaces internos válidos y ninguna página vacía.
- [x] Concluir QA del PDF de Los Nogales anterior a V4: regenerado con 15,465,386 bytes, 51 páginas, 92 enlaces internos válidos, nueve externos y cero vínculos vacíos `#`. Mapa centrado y completo.
- [ ] Comprobar envío/recepción real con adjunto de extremo a extremo. No se efectuó envío SMTP real.
- [x] Confirmar Graphify V4: 1,560 archivos, salida 0 y sin cambios de topología; conserva 16,138 nodos, 31,675 aristas y 1,764 comunidades. HTML omitido por el límite de 5,000 nodos.

Conectividad restablecida el 5 de septiembre: `SELECT 1` y PREVIEW de expedientes confirmaron acceso SQL, 291 proyectos, 247 firmes e Ixtaltepec en no va. El error 40615 previo por firewall de la IP `187.193.193.170` ya no es el bloqueo vigente. V4 compiló sin advertencias ni errores y hot reload volvió a responder HTTP 200 en HTTPS 7105. Navegación y PDF anteriores a V4 verificados; UI/PDF de las nuevas ligas económicas en revisión.

Pendientes de información: dos folios sociales no vinculables a Mixtos II; seis solicitudes SAEE sin Folio LLAVE; fecha del tipo de cambio; archivos locales de calculadoras CFE restantes/nuevas versiones; discrepancia de 19 obras CFE frente a 11 detalladas en `CFE-CM2-0155-2026`; conciliación de obras y costos compartidos contra PAM. Las 291 ligas de calculadora económica en línea de KV ya están incorporadas en V4.

Alcance de navegación: la primera convocatoria está oculta y las fuentes tabulares anteriores de segunda convocatoria están deshabilitadas. Queda por confirmar ocultar los dos controles heredados de evidencia de segunda convocatoria. **La ruta `/ProyectosPrivados/SegundaConvocatoria` es hoy el acceso a Mixtos II y se conserva.**

### Ficha general de proyecto · 5 de septiembre de 2026

- [x] Ordenar el contenido: portada → índice dinámico → participación nacional/GCR → datos técnicos → costos/desglose → factibilidad → social → localización → seguimiento → territorial → documentos → contraportada.
- [x] Destacar nombre, folio, MW y participación del proyecto; gráficas institucionales planas, sin prelación ni textos de desarrollo. No convertir GCR ausente en cero.
- [x] Añadir lámina de datos técnicos con tecnología, almacenamiento reportado, ubicación, subestación, permiso/solicitud y operación comercial prevista. Se conservan unidades originales y datos faltantes.
- [x] Validar PREVIEW documental V3 de `BD_MIXTOS_II`: ME estatus del permiso, LX oficio, MA pago y NE desahogo.
- [x] Aplicar V3: 291 expedientes nuevos, misma SHA-256 auditada, 17 calculadoras, 247 firmes e Ixtaltepec en no va. Los Nogales conserva ocho obras y tipo de cambio 18.70.
- [x] Confirmar repetición idempotente de V3: cero inserciones, 247 firmes, Ixtaltepec no va y 17 calculadoras conservados.
- [x] Ejecutar y revisar SAN ISIDRO II (`CFE-CM2-0400-2026`): análisis y detalle completos, 47 láminas, cuatro páginas de índice/42 entradas y PDF verificado de 47 páginas. Incluye CENACE, divisiones CFE, centrales, generación distribuida, permisos, PAM, PODECOBI e INEGI.
- [x] Validar navegación real de Los Nogales (51 láminas/46 entradas), mapa OSM, documentos y variante sin costos (48 láminas); sin superposición cuerpo/pie ni prelación.
- [x] Concluir QA del PDF de Los Nogales antes de V4: 51 páginas, 15,465,386 bytes, 92 enlaces internos, nueve externos, sin vínculos vacíos ni páginas vacías. La nueva liga económica exige revalidación V4.
- [ ] Verificar envío y recepción reales del adjunto actual; no se ha realizado envío SMTP real en esta validación.

Controles de datos: SAN ISIDRO II (`CFE-CM2-0400-2026`) carece de costos CFE y obras en la fuente; Los Nogales (`CFE-CM2-0038-2026`) tiene ocho obras, no once. El tipo de cambio 18.70 MXN/USD proviene del Excel, sin fecha de mercado documentada. Fuente de video no incorporada; nuevo insumo CENACE y decisión de visibilidad de otras convocatorias pendientes. Ninguno de estos faltantes se completa con supuestos.

Mejoras menores pendientes, detectadas en QA PDF:

- [ ] Mostrar la etiqueta regional en contexto de demanda, que actualmente expone el identificador `4`.
- [ ] Añadir identificadores de tramo a líneas de transmisión con nombres compartidos.
- [x] Excluir enlaces de controles Leaflet `href="#"`: PDF regenerado de Los Nogales con cero vínculos vacíos. Los enlaces del índice se mantienen válidos.

Los dos primeros hallazgos permanecen pendientes; los vínculos vacíos de Leaflet ya se corrigieron y verificaron. El envío SMTP real, el nuevo archivo CENACE y la confirmación de visibilidad de otras convocatorias siguen pendientes.

### Calculadora económica en línea · V4 · 5 de septiembre de 2026

- [x] Localizar 291 ligas HTTPS antes omitidas en `BD_MIXTOS_II!KV`, encabezado fila 2 `Calculadora economica`, y ampliar el contrato técnico de importación.
- [x] Mostrar el botón `Calculadora económica en línea` con la URL original del folio y validación de dominio/HTTPS; conservar separado `Calculadora de costos CFE` del archivo local autenticado. No se consideran documentos equivalentes.
- [x] HEAD de las ligas Los Nogales (`67c…`, XLSM) y San Isidro (`2711…`, `2026.08.07 - Calculadora Aljaval - vSent _San IsidroII.xlsm`) responde HTTP 200. No se ejecutaron macros.
- [x] PREVIEW validado y V4 aplicada: 291 expedientes nuevos, 291 ligas económicas, 17 calculadoras CFE locales, 247 firmes e Ixtaltepec no va.
- [x] Confirmar repetición idempotente V4: cero inserciones, 291 ligas en línea, 17 archivos CFE locales, 247 firmes e Ixtaltepec en no va.
- [x] Compilación V4: cero advertencias/cero errores; watch HTTPS 7105 responde HTTP 200.
- [x] Validar UI real San Isidro V4: 47 láminas, análisis/detalle completos y lámina 46 con la URL exacta de `BD_MIXTOS_II!KV189`, sin superposición. HTML de Los Nogales conserva diferenciadas la calculadora económica original y la calculadora local CFE.
- [x] Regresión Node: 15 pruebas aprobadas.
- [x] Regenerar PDF San Isidro V4: 13,285,320 bytes, 47 páginas.
- [x] Concluir QA del PDF San Isidro V4: 47 páginas, 84 enlaces internos válidos y siete externos; URI económica original de KV189 presente en página 46, mapa centrado en página 21, sin páginas vacías ni enlaces de controles `#`. Evidencia: `output/pdf/qa-mixtos0400-v4-20260905/revision.md`.
- [x] Confirmar Graphify V4: 1,560 archivos, salida 0 y sin cambios de topología; archivos generados intactos con 16,138 nodos, 31,675 aristas y 1,764 comunidades.

No se envió correo en esta etapa. El PDF V4 de San Isidro sí fue revisado; esto no acredita entrega real. Copia validada: `output/pdf/Ficha_MixtosII_San_Isidro_II_20260905.pdf`. Los Nogales conserva en pantalla ambas calculadoras sin superposición; su PDF previo V4 permanece como evidencia del ajuste cartográfico, no como versión con la nueva liga.
