SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Payload NVARCHAR(MAX) = N'[{"ProyectoId":8,"PEM":"P16-OC2","Etiqueta":"S.E. POTRERILLOS","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:1c2b2f1fc571c5e927e5","UniversoClave":"se:1c2b2f1fc571c5e927e5","NodoClave":"se:1c2b2f1fc571c5e927e5","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-101.7907785,20.9641526]},"Latitud":20.9641526,"Longitud":-101.7907785,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"occidental","GcrCatalogo":"occidental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR occidental (+15); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":13,"PEM":"P16-NO2","Etiqueta":"SE SERI","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:6892bad9d21c34983d9e","UniversoClave":"se:6892bad9d21c34983d9e","NodoClave":"se:6892bad9d21c34983d9e","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-110.9512294,28.9295848]},"Latitud":28.9295848,"Longitud":-110.9512294,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":95,"NivelRed":"transmision","GcrProyecto":"noroeste","GcrCatalogo":"noroeste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR noroeste (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":34,"PEM":"P17-BC14","Etiqueta":"S.E. PANAMERICANA POTENCIA","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:da58753d8690979cc396","UniversoClave":"se:da58753d8690979cc396","NodoClave":"se:da58753d8690979cc396","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-116.9846142,32.4470158]},"Latitud":32.4470158,"Longitud":-116.9846142,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"bcalifornia","GcrCatalogo":"bcalifornia","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR bcalifornia (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":35,"PEM":"M15-CE2","Etiqueta":"S.E. ALMOLOYA","Rol":"extremo_linea","RegistroClave":"dgmesnie_geojson:se:a063af912a9598937c12","UniversoClave":"se:a063af912a9598937c12","NodoClave":"se:a063af912a9598937c12","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-99.760662,19.3887944]},"Latitud":19.3887944,"Longitud":-99.760662,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"central","GcrCatalogo":"central","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); tensión compatible: 400 kV (+10); punto dentro de GCR central (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":42,"PEM":"P18-NE4","Etiqueta":"S.E. RIO ESCONDIDO","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:3556e3b04a7b23340a8f","UniversoClave":"se:3556e3b04a7b23340a8f","NodoClave":"se:3556e3b04a7b23340a8f","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-100.6925238,28.4839207]},"Latitud":28.4839207,"Longitud":-100.6925238,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"noreste","GcrCatalogo":"noreste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); tensión compatible: 400 kV (+10); punto dentro de GCR noreste (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":58,"PEM":"P17-CE2","Etiqueta":"S.E. DEPORTIVA","Rol":"extremo_linea","RegistroClave":"dgmesnie_geojson:se:4a6743e8a69d068dbad0","UniversoClave":"se:4a6743e8a69d068dbad0","NodoClave":"se:4a6743e8a69d068dbad0","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-99.7145885,19.2888196]},"Latitud":19.2888196,"Longitud":-99.7145885,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"central","GcrCatalogo":"central","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); tensión compatible: 230 kV (+10); punto dentro de GCR central (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":58,"PEM":"P17-CE2","Etiqueta":"S.E. TOLUCA","Rol":"extremo_linea","RegistroClave":"dgmesnie_geojson:se:2404d092fe105b286b0c","UniversoClave":"se:2404d092fe105b286b0c","NodoClave":"se:2404d092fe105b286b0c","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-99.6322425,19.2938292]},"Latitud":19.2938292,"Longitud":-99.6322425,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"central","GcrCatalogo":"central","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); tensión compatible: 230 kV (+10); punto dentro de GCR central (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":2,"EsPrincipal":false},{"ProyectoId":71,"PEM":"I16-NE3","Etiqueta":"S.E. REYNOSA","Rol":"extremo_linea","RegistroClave":"dgmesnie_geojson:se:eac90c70d6ddccf78587","UniversoClave":"se:eac90c70d6ddccf78587","NodoClave":"se:eac90c70d6ddccf78587","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-98.3109961,26.0806308]},"Latitud":26.0806308,"Longitud":-98.3109961,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"subtransmision","GcrProyecto":"noreste","GcrCatalogo":"noreste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR noreste (+15); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":98,"PEM":"P15-NO1","Etiqueta":"S.E. LA HIGUERA","Rol":"extremo_linea","RegistroClave":"dgmesnie_geojson:se:a3f19539444bf0e71217","UniversoClave":"se:a3f19539444bf0e71217","NodoClave":"se:a3f19539444bf0e71217","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-107.5072755,24.6978911]},"Latitud":24.6978911,"Longitud":-107.5072755,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":95,"NivelRed":"transmision","GcrProyecto":"noroeste","GcrCatalogo":"noroeste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); nombre presente en el título (+15); tensión compatible: 400 kV (+10); punto dentro de GCR noroeste (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":108,"PEM":"M19-OR1","Etiqueta":"S.E. POZA RICA","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:456fb8c9e95a44aec695","UniversoClave":"se:456fb8c9e95a44aec695","NodoClave":"se:456fb8c9e95a44aec695","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-97.504166,20.5091207]},"Latitud":20.5091207,"Longitud":-97.504166,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR oriental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":146,"PEM":"P20-NE1","Etiqueta":"S.E. NUEVO LAREDO","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:4078e801413068805f38","UniversoClave":"se:4078e801413068805f38","NodoClave":"se:4078e801413068805f38","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-99.5385237,27.4732228]},"Latitud":27.4732228,"Longitud":-99.5385237,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"subtransmision","GcrProyecto":"noreste","GcrCatalogo":"noreste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR noreste (+15); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":156,"PEM":"M20-OR1","Etiqueta":"S.E. MINATITLAN II","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:8cea3d1239578d35667a","UniversoClave":"se:8cea3d1239578d35667a","NodoClave":"se:8cea3d1239578d35667a","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-94.3995222,18.0291344]},"Latitud":18.0291344,"Longitud":-94.3995222,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR oriental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":163,"PEM":"M20-NE2","Etiqueta":"S.E. VILLA DE GARCIA","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:480705dfdbecac60ab0a","UniversoClave":"se:480705dfdbecac60ab0a","NodoClave":"se:480705dfdbecac60ab0a","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-100.5534058,25.7099192]},"Latitud":25.7099192,"Longitud":-100.5534058,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"noreste","GcrCatalogo":"noreste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); punto dentro de GCR noreste (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":167,"PEM":"CFE20-TUC","Etiqueta":"S.E. POZA RICA","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:456fb8c9e95a44aec695","UniversoClave":"se:456fb8c9e95a44aec695","NodoClave":"se:456fb8c9e95a44aec695","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-97.504166,20.5091207]},"Latitud":20.5091207,"Longitud":-97.504166,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); punto dentro de GCR oriental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":false},{"ProyectoId":167,"PEM":"CFE20-TUC","Etiqueta":"S.E. TUXPAN","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:77724fc03dc162d098d9","UniversoClave":"se:77724fc03dc162d098d9","NodoClave":"se:77724fc03dc162d098d9","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-97.3348037,21.0171872]},"Latitud":21.0171872,"Longitud":-97.3348037,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"subtransmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR oriental (+15); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":2,"EsPrincipal":true},{"ProyectoId":167,"PEM":"CFE20-TUC","Etiqueta":"SE POZA RICA II","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:bf75518ba77b3f55f9f3","UniversoClave":"se:bf75518ba77b3f55f9f3","NodoClave":"se:bf75518ba77b3f55f9f3","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-97.5042015,20.5094248]},"Latitud":20.5094248,"Longitud":-97.5042015,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); punto dentro de GCR oriental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":3,"EsPrincipal":false},{"ProyectoId":168,"PEM":"CFE20-VAC-R","Etiqueta":"S.E. ESCARCEGA POTENCIA","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:5cb232d77feb4d8aa31b","UniversoClave":"se:5cb232d77feb4d8aa31b","NodoClave":"se:5cb232d77feb4d8aa31b","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-90.7626808,18.6107787]},"Latitud":18.6107787,"Longitud":-90.7626808,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2025-11-25","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"peninsular","GcrCatalogo":"peninsular","FuenteDocumento":"INF Pormenorizado Tablas Rev 251125 SENER.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); tensión compatible: 400 kV (+10); punto dentro de GCR peninsular (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":189,"PEM":"P21-BC1","Etiqueta":"S.E. METROPOLI POTENCIA","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:9bec5a2b0da198be9e11","UniversoClave":"se:9bec5a2b0da198be9e11","NodoClave":"se:9bec5a2b0da198be9e11","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-116.8782506,32.445103]},"Latitud":32.445103,"Longitud":-116.8782506,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"bcalifornia","GcrCatalogo":"bcalifornia","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); punto dentro de GCR bcalifornia (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":191,"PEM":"M21-CE1","Etiqueta":"S.E. NOPALA","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:5fa2e93a8821aa2552b1","UniversoClave":"se:5fa2e93a8821aa2552b1","NodoClave":"se:5fa2e93a8821aa2552b1","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-99.2833417,19.4817636]},"Latitud":19.4817636,"Longitud":-99.2833417,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"subtransmision","GcrProyecto":"central","GcrCatalogo":"central","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR central (+15); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":215,"PEM":"P22-NO3","Etiqueta":"SE OASIS","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:8896b7457160da98967a","UniversoClave":"se:8896b7457160da98967a","NodoClave":"se:8896b7457160da98967a","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-111.0425419,29.6887941]},"Latitud":29.6887941,"Longitud":-111.0425419,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"subtransmision","GcrProyecto":"noroeste","GcrCatalogo":"noroeste","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); tensión compatible: 115 kV (+10); punto dentro de GCR noroeste (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5); nombre de una sola palabra (-10)","Orden":1,"EsPrincipal":true},{"ProyectoId":224,"PEM":"P23-OC2","Etiqueta":"S.E. COLIMA II","Rol":"componente_declarado","RegistroClave":"dgmesnie_geojson:se:a88ebc56b4b16bffa78d","UniversoClave":"se:a88ebc56b4b16bffa78d","NodoClave":"se:a88ebc56b4b16bffa78d","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-103.7747675,19.2804168]},"Latitud":19.2804168,"Longitud":-103.7747675,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":90,"NivelRed":"transmision","GcrProyecto":"occidental","GcrCatalogo":"occidental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); punto dentro de GCR occidental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true},{"ProyectoId":250,"PEM":"M24-OR2","Etiqueta":"SE LAGUNA VERDE","Rol":"principal_probable","RegistroClave":"dgmesnie_geojson:se:c7985f4c110fa85c79e1","UniversoClave":"se:c7985f4c110fa85c79e1","NodoClave":"se:c7985f4c110fa85c79e1","TipoGeometria":"Point","Geometria":{"type":"Point","coordinates":[-96.4081246,19.7198157]},"Latitud":19.7198157,"Longitud":-96.4081246,"PrecisionUbicacion":"exacta","MetodoUbicacion":"cruce_inventario_subestaciones_v1","Fuente":"DGMESNIE · inventario conciliado de subestaciones","FechaCorte":"2026-07-30","RadioSugeridoKm":2,"Puntaje":100,"NivelRed":"transmision","GcrProyecto":"oriental","GcrCatalogo":"oriental","FuenteDocumento":"INF Pormenorizado Tablas Rev 260630 - 2do Trimestre 2026_ rev.xlsx","Evidencia":"nombre de subestación presente en el expediente (+45); mención explícita como SE (+20); nombre presente en el título (+15); punto dentro de GCR oriental (+15); coordenada conciliada por varias fuentes (+5); inventario georreferenciado con estado trazable (+5)","Orden":1,"EsPrincipal":true}]';
DECLARE @Esperadas INT = 22;
DECLARE @ProyectosEsperados INT = 19;
DECLARE @Metodo NVARCHAR(80) = N'cruce_inventario_subestaciones_v1';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-CAPAS-20260802-01';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-02';

BEGIN TRY
    BEGIN TRANSACTION;

    IF ISJSON(@Payload) <> 1
        THROW 51100, N'El payload territorial no es JSON válido.', 1;

    CREATE TABLE #Entrada
    (
        ProyectoId BIGINT NOT NULL,
        PEM NVARCHAR(500) NULL,
        Etiqueta NVARCHAR(300) NOT NULL,
        Rol NVARCHAR(60) NOT NULL,
        RegistroClave NVARCHAR(80) NOT NULL,
        UniversoClave NVARCHAR(80) NOT NULL,
        NodoClave NVARCHAR(80) NOT NULL,
        TipoGeometria NVARCHAR(30) NOT NULL,
        GeometriaJson NVARCHAR(MAX) NOT NULL,
        Latitud DECIMAL(10,7) NOT NULL,
        Longitud DECIMAL(11,7) NOT NULL,
        PrecisionUbicacion NVARCHAR(40) NOT NULL,
        MetodoUbicacion NVARCHAR(80) NOT NULL,
        Fuente NVARCHAR(500) NOT NULL,
        FechaCorte DATE NULL,
        RadioSugeridoKm DECIMAL(8,2) NOT NULL,
        Puntaje INT NOT NULL,
        NivelRed NVARCHAR(30) NOT NULL,
        GcrProyecto NVARCHAR(40) NOT NULL,
        GcrCatalogo NVARCHAR(80) NOT NULL,
        FuenteDocumento NVARCHAR(1000) NULL,
        Evidencia NVARCHAR(2000) NULL,
        Orden INT NOT NULL,
        EsPrincipal BIT NOT NULL
    );

    INSERT #Entrada
    SELECT *
    FROM OPENJSON(@Payload)
    WITH
    (
        ProyectoId BIGINT '$.ProyectoId',
        PEM NVARCHAR(500) '$.PEM',
        Etiqueta NVARCHAR(300) '$.Etiqueta',
        Rol NVARCHAR(60) '$.Rol',
        RegistroClave NVARCHAR(80) '$.RegistroClave',
        UniversoClave NVARCHAR(80) '$.UniversoClave',
        NodoClave NVARCHAR(80) '$.NodoClave',
        TipoGeometria NVARCHAR(30) '$.TipoGeometria',
        GeometriaJson NVARCHAR(MAX) '$.Geometria' AS JSON,
        Latitud DECIMAL(10,7) '$.Latitud',
        Longitud DECIMAL(11,7) '$.Longitud',
        PrecisionUbicacion NVARCHAR(40) '$.PrecisionUbicacion',
        MetodoUbicacion NVARCHAR(80) '$.MetodoUbicacion',
        Fuente NVARCHAR(500) '$.Fuente',
        FechaCorte DATE '$.FechaCorte',
        RadioSugeridoKm DECIMAL(8,2) '$.RadioSugeridoKm',
        Puntaje INT '$.Puntaje',
        NivelRed NVARCHAR(30) '$.NivelRed',
        GcrProyecto NVARCHAR(40) '$.GcrProyecto',
        GcrCatalogo NVARCHAR(80) '$.GcrCatalogo',
        FuenteDocumento NVARCHAR(1000) '$.FuenteDocumento',
        Evidencia NVARCHAR(2000) '$.Evidencia',
        Orden INT '$.Orden',
        EsPrincipal BIT '$.EsPrincipal'
    );

    IF (SELECT COUNT(*) FROM #Entrada) <> @Esperadas
        THROW 51101, N'Preflight: el lote no contiene exactamente 22 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM #Entrada) <> @ProyectosEsperados
        THROW 51102, N'Preflight: el lote no contiene exactamente 19 proyectos.', 1;
    IF EXISTS
    (
        SELECT 1 FROM #Entrada
        GROUP BY ProyectoId, UniversoClave
        HAVING COUNT(*) > 1
    )
        THROW 51103, N'Preflight: existen relaciones proyecto-elemento duplicadas.', 1;
    IF EXISTS
    (
        SELECT 1 FROM #Entrada
        WHERE TipoGeometria <> N'Point'
           OR MetodoUbicacion <> @Metodo
           OR Rol NOT IN (N'principal_probable',N'componente_declarado',N'extremo_linea',N'referencia_titular')
           OR Puntaje < 90
           OR ISJSON(GeometriaJson) <> 1
           OR Latitud NOT BETWEEN 14 AND 33.5
           OR Longitud NOT BETWEEN -118 AND -86
           OR GcrProyecto <> GcrCatalogo
    )
        THROW 51104, N'Preflight: geometría, rol, GCR, coordenadas o evidencia inválida.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Entrada
        GROUP BY ProyectoId
        HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
    )
        THROW 51105, N'Preflight: cada proyecto debe tener exactamente un punto representativo.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Entrada e
        LEFT JOIN dgmesnie.vw_PAMProyectoVigente p
          ON p.ProyectoId = e.ProyectoId
         AND p.EstadoVigenciaCartera = N'Vigente'
        WHERE p.ProyectoId IS NULL
    )
        THROW 51106, N'Preflight: uno o más proyectos ya no están vigentes.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Entrada e
        LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i
          ON i.RegistroClave = e.RegistroClave
         AND i.UniversoClave = e.UniversoClave
         AND i.Activa = 1
         AND i.FuenteCoordenadas = N'dgmesnie_geojson'
         AND i.EstadoValidacion = N'catalogado'
         AND ABS(CONVERT(float,i.Latitud) - CONVERT(float,e.Latitud)) <= 0.000001
         AND ABS(CONVERT(float,i.Longitud) - CONVERT(float,e.Longitud)) <= 0.000001
        WHERE i.RegistroClave IS NULL
    )
        THROW 51107, N'Preflight: un punto no coincide con el inventario DGMESNIE catalogado.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Entrada e
        JOIN dgmesnie.PAMProyectoUbicacion u WITH (UPDLOCK, HOLDLOCK)
          ON u.ProyectoId = e.ProyectoId
         AND u.Activa = 1
    )
        THROW 51108, N'Preflight: un proyecto del lote ya tiene una ubicación activa.', 1;

    DECLARE @Insertadas TABLE
    (
        UbicacionId BIGINT,
        ProyectoId BIGINT,
        Etiqueta NVARCHAR(300),
        PrecisionUbicacion NVARCHAR(40),
        EsPrincipal BIT,
        Validada BIT,
        Activa BIT
    );

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta,
           inserted.PrecisionUbicacion, inserted.EsPrincipal,
           inserted.Validada, inserted.Activa
      INTO @Insertadas
    SELECT
        e.ProyectoId, e.Etiqueta, e.TipoGeometria, e.GeometriaJson,
        e.Latitud, e.Longitud, e.PrecisionUbicacion, e.MetodoUbicacion,
        e.Fuente, e.FechaCorte, e.RadioSugeridoKm, e.Orden, e.EsPrincipal,
        1, 1, @Usuario,
        LEFT(CONCAT(
            N'Coincidencia de capas confirmada por usuario. Rol=', e.Rol,
            N'; RegistroClave=', e.RegistroClave,
            N'; UniversoClave=', e.UniversoClave,
            N'; Puntaje=', e.Puntaje,
            N'; NivelRed=', e.NivelRed,
            N'; GCR=', e.GcrProyecto,
            N'; FuenteDocumento=', COALESCE(e.FuenteDocumento, N''),
            N'; Lote=', @Lote, N'.'), 1000)
    FROM #Entrada e;

    IF (SELECT COUNT(*) FROM @Insertadas) <> @Esperadas
        THROW 51109, N'Aplicación: no se insertaron exactamente 22 relaciones.', 1;

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS Insertadas,
           COUNT(DISTINCT ProyectoId) AS Proyectos,
           SUM(CASE WHEN Validada = 1 AND Activa = 1 THEN 1 ELSE 0 END) AS ActivasValidadas
    FROM @Insertadas;
    SELECT * FROM @Insertadas ORDER BY ProyectoId, EsPrincipal DESC, UbicacionId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
