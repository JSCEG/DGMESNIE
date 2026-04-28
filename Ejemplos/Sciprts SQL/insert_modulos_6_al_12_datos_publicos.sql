
INSERT INTO [dbo].[Modulos] ([SeccionId], [Title], [FundamentoLegalModulo], [Roles], [NombresRoles], [Perfiles], [Etapa], [JustificacionOrden],
[AyudaContextual], [Controller], [Action], [Desc], [Img], [Btn], [ElementosUI], [AyudaVista], [Orden], [Activo])
VALUES
(2, 'Catálogo de Tecnologías', 'Art. 70-V Reglamento', '0,1,24,25,28,31,32', 'SENER, Universidades, Industria',
'Académico/Investigador, Usuario Institucional', 'Consulta Técnica', 'Difunde tecnologías energéticas',
'Catálogo técnico de tecnologías', 'Tecnologias', 'Catalogo', 'Parámetros técnicos de tecnologías', 'modulos/tecnologias.png',
'Ver Catálogo', 'Fichas técnicas filtrables', 'Soporte para decisiones técnicas', 6, 1),

(2, 'Equipos Energéticos', 'Art. 70 Reglamento', '0,1,9,24,25,28,32', 'SENER, CONUEE, Universidades, Industria',
'Académico/Investigador, Usuario Institucional', 'Difusión Eficiente', 'Promueve equipos eficientes',
'Consumo de equipos relevantes', 'Equipos', 'UsoFinal', 'Consumo energético de equipos', 'modulos/equipos.png',
'Consultar', 'Tabla comparativa con filtros', 'Ayuda en selección de equipos', 7, 1),

(2, 'Mercado Eléctrico Mayorista', 'Art. 70-II-b Reglamento', '0,1,5,6,8,14,32,37,39,41,44,48', 'SENER, CFE, CENACE, CNH, Generadores, Medios',
'Usuario Institucional, Académico', 'Análisis de Mercado', 'Informa desempeño del mercado',
'Datos del mercado eléctrico', 'Mercado', 'Mayorista', 'Desempeño del MEM', 'modulos/mercado_mayorista.png',
'Consultar', 'Gráficos de precios nodales', 'Análisis para participantes', 8, 1),

(2, 'Dashboard CELs', 'Art. 77 Reglamento', '0,1,39,41,46,47,48,49', 'SENER, Medios, CELs, Generadores Limpios',
'Público General, Usuario Institucional', 'Rendición de Cuentas', 'Transparencia en CELs',
'Estadísticas de certificados limpios', 'EnergiasLimpias', 'Certificados', 'Dashboard de CELs otorgados', 'modulos/reporte_cels.png',
'Ver CELs', 'Filtros por tecnología/año', 'Cumplimiento regulatorio visible', 9, 1),

(2, 'Indicadores Estratégicos', 'Art. 77 Reglamento', '0,1,23,24,25,28,38,39,41', 'SENER, INEGI, Universidades, Medios',
'Público General, Académico', 'Transparencia', 'Muestra KPIs energéticos',
'Indicadores clave del sector', 'Indicadores', 'Publicos', 'Consulta de KPIs estratégicos', 'modulos/indicadores_publicos.png',
'Ver Indicadores', 'Tarjetas KPI, mapas', 'Monitoreo ciudadano', 10, 1),

(2, 'Calculadora Pública', 'Art. 77 Reglamento', '0,1,28,30,39,40', 'SENER, OSC, Ciudadanos, Organizaciones Sociales',
'Público General', 'Orientación Ciudadana', 'Herramienta educativa',
'Simulador de proyectos energéticos', 'Calculadora', 'Publica', 'Simula viabilidad de proyectos', 'modulos/calculadora_publica.png',
'Simular', 'Formulario interactivo', 'Exploración de alternativas', 11, 1),

(2, 'Mapa de Infraestructura', 'Art. 77 Reglamento', '0,1,5,6,8,26,27,33,34,35,39', 'SENER, CFE, CENACE, Estados, Municipios, Empresas',
'Usuario Institucional, Público', 'Consulta Visual', 'Georreferenciación de activos',
'Visor de infraestructura energética', 'Map', 'Infraestructura_SEM', 'Mapa interactivo de activos', 'modulos/geo.png',
'Explorar', 'Capas filtrables por tipo', 'Planificación territorial', 12, 1);
