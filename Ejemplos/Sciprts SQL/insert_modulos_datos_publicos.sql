
-- Módulo: Precios de Gas L.P.
INSERT INTO [dbo].[Modulos] (
    [SeccionId], [Title], [FundamentoLegalModulo], [Roles], [NombresRoles], [Perfiles],
    [Etapa], [JustificacionOrden], [AyudaContextual], [Controller], [Action], [Desc],
    [Img], [Btn], [ElementosUI], [AyudaVista], [Orden], [Activo]
) VALUES (
    3,
    'Precios de Gas L.P.',
    'Art. 77 Reglamento',
    '0,1,7,8',
    'Ciudadanía, SENER, Representantes Sectorizados, LitioMx',
    'Consulta Pública, Analista de Energía, Economista Sectorial',
    'Consulta Pública',
    'Es un insumo regulado de interés público, con impacto en política social',
    'Consulta precios históricos de Gas L.P.',
    'Indicadores',
    'Precio_Gas_LP',
    'Consulta precios históricos de Gas L.P.',
    'modulos/precio.png',
    'Ver Históricos',
    'Series históricas, gráficos, rangos temporales, exportación',
    'Visualiza la evolución de precios máximos aplicables por zona y periodo',
    4,
    1
);

-- Módulo: Tarifas Eléctricas
INSERT INTO [dbo].[Modulos] (
    [SeccionId], [Title], [FundamentoLegalModulo], [Roles], [NombresRoles], [Perfiles],
    [Etapa], [JustificacionOrden], [AyudaContextual], [Controller], [Action], [Desc],
    [Img], [Btn], [ElementosUI], [AyudaVista], [Orden], [Activo]
) VALUES (
    3,
    'Tarifas Eléctricas',
    'Art. 77 Reglamento',
    '0,1,7,8',
    'Ciudadanía, SENER, Representantes Sectorizados, LitioMx',
    'Consulta Pública, Técnico Regulador, Planeador Eléctrico',
    'Consulta Pública',
    'Promueve transparencia tarifaria y análisis del comportamiento del mercado eléctrico',
    'Consulta tarifas de electricidad a nivel nacional',
    'Tarifas',
    'TarifasMI',
    'Consulta tarifas de electricidad a nivel nacional.',
    'modulos/precio.png',
    'Consultar Tarifas',
    'Tabla por periodo y zona, combo selector, descarga en Excel',
    'Muestra las tarifas promedio aplicables según categoría y región',
    5,
    1
);
