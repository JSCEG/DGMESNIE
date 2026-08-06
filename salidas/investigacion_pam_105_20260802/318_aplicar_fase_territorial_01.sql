SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-01';
DECLARE @Metodo NVARCHAR(80) = N'investigacion_individual_conciliada_v3';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE #ProyectoEsperado
    (
        ProyectoId BIGINT NOT NULL PRIMARY KEY,
        ClaveProyecto NVARCHAR(200) NULL,
        NombreProyecto NVARCHAR(500) NOT NULL
    );

    INSERT #ProyectoEsperado (ProyectoId, ClaveProyecto, NombreProyecto)
    VALUES
      (64,  N'M18-NT1|M01-GCRN', N'Modernización de la subestación Cuadro de Maniobras Cerro del Mercado'),
      (141, N'P20-NO4', N'Cerro Cañedo MVAr'),
      (160, N'M20-NT1', N'Cambio de arreglo de la SE Moctezuma en 230 y 115 kV'),
      (192, N'M21-CE2', N'Adecuación de Subestaciones Eléctricas Hidalgo y Cubitos'),
      (270, NULL, N'Red de Transmisión Asociada al Proyecto CFV Puerto Peñasco Secuencia 3 (100 MW) y 4 (300 MW)'),
      (274, N'Sin PEM', N'290 LT Red de Transmisión asociada a la CH Chicoasén II'),
      (293, N'CFE25-FPP', N'Obras de refuerzo del proyecto CFV Puerto Peñasco Secuencia III'),
      (294, N'CFE25-PHC', N'Interconexión PH Chicoasén II'),
      (306, N'M01-GCRN', N'Modernización de la subestación Cuadro de Maniobras Cerro del Mercado');

    IF EXISTS
    (
        SELECT 1
        FROM #ProyectoEsperado e
        LEFT JOIN dgmesnie.PAMProyectoVersion v
          ON v.ProyectoId = e.ProyectoId
         AND v.EsVersionVigente = 1
         AND ISNULL(REPLACE(REPLACE(REPLACE(v.ClaveProyecto, CHAR(13) + CHAR(10), N'|'), CHAR(13), N'|'), CHAR(10), N'|'), N'') = ISNULL(e.ClaveProyecto, N'')
         AND v.NombreProyecto = e.NombreProyecto
         AND v.EstadoVigenciaCartera = N'Vigente'
        WHERE v.ProyectoId IS NULL
    )
        THROW 52001, N'Preflight: cambió la identidad, clave o vigencia de uno de los proyectos auditados.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (141, 160, 192, 306)
          AND Activa = 1
    )
        THROW 52002, N'Preflight: uno de los cuatro proyectos por publicar ya tiene ubicación activa.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE UbicacionId = 4
          AND ProyectoId = 64
          AND Activa = 1
          AND Validada = 0
          AND PrecisionUbicacion = N'geocodificada'
          AND MetodoUbicacion = N'cruce_catalogo_red_v1'
          AND Latitud = CONVERT(decimal(9,6), 24.046495)
          AND Longitud = CONVERT(decimal(10,6), -104.666706)
    )
        THROW 52003, N'Preflight: la ubicación sugerida de Cerro del Mercado cambió y no puede promoverse de forma segura.', 1;

    CREATE TABLE #InventarioEsperado
    (
        RegistroClave NVARCHAR(100) NOT NULL PRIMARY KEY,
        NombreNormalizado NVARCHAR(300) NOT NULL,
        TensionKv decimal(8,3) NOT NULL,
        Latitud decimal(10,7) NOT NULL,
        Longitud decimal(11,7) NOT NULL
    );

    INSERT #InventarioEsperado
      (RegistroClave, NombreNormalizado, TensionKv, Latitud, Longitud)
    VALUES
      (N'dgmesnie_geojson:se:824753341c85009afcde', N'MOCTEZUMA AMPLIACION', 230, 30.1775821, -106.4560239),
      (N'openstreetmap:a5820bd51ebb057cc925e6030d30cede45249f60', N'CUBITOS', 85, 20.1130000, -98.7333500),
      (N'dgmesnie_geojson:se:b0943b6b0edb7166d63b', N'CERRO DEL MERCADO', 115, 24.0464949, -104.6667062);

    IF EXISTS
    (
        SELECT 1
        FROM #InventarioEsperado e
        LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i
          ON i.RegistroClave = e.RegistroClave
         AND i.Activa = 1
         AND i.NombreNormalizado = e.NombreNormalizado
         AND i.TensionKv = e.TensionKv
         AND CONVERT(decimal(10,7), i.Latitud) = e.Latitud
         AND CONVERT(decimal(11,7), i.Longitud) = e.Longitud
        WHERE i.RegistroClave IS NULL
    )
        THROW 52004, N'Preflight: cambió uno de los tres nodos conciliados del inventario eléctrico.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoRelacionVersion WITH (UPDLOCK, HOLDLOCK)
        WHERE EsRelacionVigente = 1
          AND ((ProyectoPadreId = 64 AND ProyectoHijoId = 306)
            OR (ProyectoPadreId = 270 AND ProyectoHijoId = 293)
            OR (ProyectoPadreId = 274 AND ProyectoHijoId = 294))
    )
        THROW 52005, N'Preflight: una de las relaciones ya fue registrada.', 1;

    DECLARE @Insertadas TABLE
    (
        UbicacionId BIGINT,
        ProyectoId BIGINT,
        Etiqueta NVARCHAR(300),
        PrecisionUbicacion NVARCHAR(40),
        Validada BIT
    );

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, FechaValidacionUtc,
        UsuarioValidacion, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta,
           inserted.PrecisionUbicacion, inserted.Validada
      INTO @Insertadas
    SELECT
        141,
        N'SE Cerro Cañedo (CCA)',
        N'Point',
        N'{"type":"Point","coordinates":[-112.1143484,30.7038713]}',
        CONVERT(decimal(9,6), 30.7038713),
        CONVERT(decimal(10,6), -112.1143484),
        N'exacta',
        @Metodo,
        N'CFE Distribución · programa de limpieza de subestaciones 2023; CENACE PAMRNT 2020-2034',
        f.FechaCorte,
        CONVERT(decimal(8,2), 1),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'Coordenada y domicilio publicados por CFE para SUBESTACION CERRO CANEDO (CCA). ',
            N'CENACE vincula P20-NO4 con el banco de 15 MVAr en 115 kV de esa SE. ',
            N'Fuente oficial CFE=https://portales-transparencia.cfe.mx/distribucion/28%20Procedimientos%20de%20Contratacin/Noreste/Culiac%C3%A1n/2023/801059608%20SA.pdf; ',
            N'Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 141 AND v.EsVersionVigente = 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, FechaValidacionUtc,
        UsuarioValidacion, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta,
           inserted.PrecisionUbicacion, inserted.Validada
      INTO @Insertadas
    SELECT
        x.ProyectoId,
        x.Etiqueta,
        N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(NVARCHAR(50), CONVERT(decimal(11,7), i.Longitud)), N',',
               CONVERT(NVARCHAR(50), CONVERT(decimal(10,7), i.Latitud)), N']}'),
        CONVERT(decimal(9,6), i.Latitud),
        CONVERT(decimal(10,6), i.Longitud),
        N'exacta',
        @Metodo,
        x.Fuente,
        f.FechaCorte,
        CONVERT(decimal(8,2), 1),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(x.Evidencia, N' RegistroClave=', i.RegistroClave, N'; Lote=', @Lote, N'.'), 1000)
    FROM
    (
        VALUES
          (160, N'dgmesnie_geojson:se:824753341c85009afcde', N'SE Moctezuma · patio 230/115 kV',
           N'CENACE diagramas unifilares 2020-2028; inventario de subestaciones DGMESNIE',
           N'CENACE ubica M20-NT1 en la SE Moctezuma de la zona Moctezuma, GCR Norte, con niveles 230/115 kV. Se usa el nodo catalogado MOCTEZUMA AMPLIACION; no se duplica el nodo vecino Villa Ahumada.'),
          (192, N'openstreetmap:a5820bd51ebb057cc925e6030d30cede45249f60', N'SE Cubitos · componente comprobado; cobertura parcial',
           N'SENER PRODESEN 2021-2035; CFE contrato de interruptor SF6 85 kV; OSM way/940761293; inventario DGMESNIE',
           N'Componente Cubitos comprobado por identidad, domicilio, operador, tensión 85/23 kV y coordenada abierta. COBERTURA PARCIAL: no representa la SE Hidalgo ni la traza subterránea de 1.1 km; ambas siguen pendientes.'),
          (306, N'dgmesnie_geojson:se:b0943b6b0edb7166d63b', N'SE Cuadro de Maniobras Cerro del Mercado',
           N'CENACE PAMRNT 2018-2038; inventario de subestaciones DGMESNIE',
           N'Coincidencia exacta de clave M01-GCRN/título entre el registro vigente y su antecedente; CENACE identifica el PEM M18-NT1 para la misma SE en 115 kV.')
    ) x (ProyectoId, RegistroClave, Etiqueta, Fuente, Evidencia)
    JOIN dgmesnie.RedElectricaSubestacionInventario i
      ON i.RegistroClave = x.RegistroClave AND i.Activa = 1
    JOIN dgmesnie.PAMProyectoVersion v
      ON v.ProyectoId = x.ProyectoId AND v.EsVersionVigente = 1
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId;

    IF (SELECT COUNT(*) FROM @Insertadas) <> 4
        THROW 52006, N'Aplicación: no se insertaron exactamente cuatro ubicaciones.', 1;

    UPDATE dgmesnie.PAMProyectoUbicacion
       SET PrecisionUbicacion = N'exacta',
           MetodoUbicacion = @Metodo,
           Validada = 1,
           FechaValidacionUtc = SYSUTCDATETIME(),
           UsuarioValidacion = @Usuario,
           Observaciones = LEFT(CONCAT(
               N'Promovida tras conciliación individual. Coincidencia exacta con SE CERRO DEL MERCADO 115 kV y continuidad M18-NT1/M01-GCRN. ',
               N'Estado previo preservado en el reversor. RegistroClave=dgmesnie_geojson:se:b0943b6b0edb7166d63b; Lote=', @Lote, N'.'), 1000)
     WHERE UbicacionId = 4 AND ProyectoId = 64 AND Activa = 1 AND Validada = 0;

    IF @@ROWCOUNT <> 1
        THROW 52007, N'Aplicación: no se promovió exactamente la ubicación legado de Cerro del Mercado.', 1;

    DECLARE @Relaciones TABLE
    (
        ProyectoRelacionVersionId BIGINT,
        ProyectoPadreId BIGINT,
        ProyectoHijoId BIGINT,
        TipoRelacion NVARCHAR(50),
        EstadoValidacion NVARCHAR(30)
    );

    INSERT dgmesnie.PAMProyectoRelacionVersion
    (
        ProyectoPadreId, ProyectoHijoId, TipoRelacion, FuenteId, CargaId,
        VigenteDesde, VigenteHasta, EsRelacionVigente, EstadoValidacion,
        HojaPaginaSeccion, Observaciones, UsuarioRegistro
    )
    OUTPUT inserted.ProyectoRelacionVersionId, inserted.ProyectoPadreId,
           inserted.ProyectoHijoId, inserted.TipoRelacion, inserted.EstadoValidacion
      INTO @Relaciones
    SELECT
        r.ProyectoPadreId, r.ProyectoHijoId, r.TipoRelacion,
        v.FuenteId, v.CargaId, f.FechaCorte, NULL, 1, e.EstadoValidacion,
        r.UbicacionFuente,
        LEFT(CONCAT(r.Observaciones, N' Lote=', @Lote, N'.'), 1000),
        @Usuario
    FROM
    (
        VALUES
          (64, 306, N'Antecedente', N'PAMRNT 2018-2038 / expediente pormenorizado 2026', N'VALIDADA: misma clave M01-GCRN y mismo título; el registro 64 conserva además la clave histórica M18-NT1.'),
          (270, 293, N'Complementario', N'Cuenta Pública CFE 2019 / cartera 2026', N'PENDIENTE: ambos refieren CFV Puerto Peñasco Secuencia III, pero difieren alcance y capacidades; no se declara duplicidad ni sucesión.'),
          (274, 294, N'Complementario', N'Cuenta Pública CFE 2019 / cartera 2026', N'PENDIENTE: ambos refieren la interconexión/transmisión de Chicoasén II, sin folio único de cruce que demuestre sustitución o identidad.')
    ) r (ProyectoPadreId, ProyectoHijoId, TipoRelacion, UbicacionFuente, Observaciones)
    JOIN dgmesnie.PAMProyectoVersion v
      ON v.ProyectoId = r.ProyectoHijoId AND v.EsVersionVigente = 1
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    CROSS APPLY
    (
        SELECT CASE WHEN r.ProyectoPadreId = 64 THEN N'Validada' ELSE N'Pendiente' END
    ) e (EstadoValidacion);

    IF (SELECT COUNT(*) FROM @Relaciones) <> 3
        THROW 52008, N'Aplicación: no se insertaron exactamente tres relaciones.', 1;

    COMMIT TRANSACTION;

    SELECT N'ubicaciones_insertadas' AS Resultado, COUNT(*) AS Cantidad FROM @Insertadas
    UNION ALL SELECT N'ubicacion_promovida', 1
    UNION ALL SELECT N'relaciones_insertadas', COUNT(*) FROM @Relaciones;
    SELECT * FROM @Insertadas ORDER BY ProyectoId;
    SELECT * FROM @Relaciones ORDER BY ProyectoPadreId, ProyectoHijoId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
