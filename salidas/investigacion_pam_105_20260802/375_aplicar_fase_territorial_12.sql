SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-12';
DECLARE @Metodo NVARCHAR(80) = N'conciliacion_pamrnt_2025_grafo_canonico';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-04';
DECLARE @VersionRed BIGINT =
(
    SELECT TOP (1) VersionId
    FROM dgmesnie.RedElectricaVersion
    WHERE Activa = 1
    ORDER BY VersionId DESC
);

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Esperados TABLE
    (
        ProyectoId BIGINT NOT NULL PRIMARY KEY,
        ClaveProyecto NVARCHAR(30) NOT NULL,
        NombreProyecto NVARCHAR(500) NOT NULL,
        GRT NVARCHAR(20) NOT NULL,
        EtapaProyecto NVARCHAR(150) NOT NULL
    );

    INSERT @Esperados VALUES
        (252, N'P25-OR2', N'Incremento de capacidad de suministro hacia la Zona San Cristóbal', N'SE', N'Instruido y SIN priorización'),
        (253, N'P24-OR2', N'Suministro de energía en la zona de carga Tehuantepec', N'SE', N'Instruido y CON priorización'),
        (254, N'P25-OC1', N'Soporte de tensión para la Zona Matehuala', N'OC', N'Instruido y CON priorización'),
        (257, N'P25-NO1', N'Incremento en la capacidad de transformación de la Zona Navojoa', N'NO', N'Instruido y CON priorización'),
        (265, N'P25-BC2', N'Cambio de tensión de operación de 69 kV a 115 kV al oriente de la ciudad de Tijuana y Tecate', N'BC', N'Instruido y SIN priorización');

    IF
    (
        SELECT COUNT(*)
        FROM @Esperados e
        JOIN dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
          ON v.ProyectoId = e.ProyectoId
         AND v.EsVersionVigente = 1
         AND v.ClaveProyecto = e.ClaveProyecto
         AND v.NombreProyecto = e.NombreProyecto
         AND v.GRT = e.GRT
         AND v.EtapaProyecto = e.EtapaProyecto
         AND v.EstadoVigenciaCartera = N'Vigente'
        JOIN dgmesnie.PAMFuente f
          ON f.FuenteId = v.FuenteId
         AND f.FechaCorte = CONVERT(date, '2026-07-30')
    ) <> 5
        THROW 54401, N'Preflight: cambió la identidad, región, etapa, vigencia o corte de alguno de los cinco PEM.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (252, 253, 254, 257, 265)
          AND Activa = 1
    )
        THROW 54402, N'Preflight: alguno de los cinco PEM ya tiene ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54403, N'Preflight: el lote ya fue aplicado.', 1;

    DECLARE @Activos TABLE
    (
        RegistroClave NVARCHAR(80) NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(500) NOT NULL,
        TensionKv DECIMAL(9,3) NOT NULL,
        Latitud DECIMAL(10,7) NOT NULL,
        Longitud DECIMAL(11,7) NOT NULL
    );

    INSERT @Activos VALUES
        (N'dgmesnie_geojson:se:5f7b7165c3beb7ac6160', N'S.E. SAN CRISTOBAL ORIENTE', 115, 16.7376419, -92.6005978),
        (N'dgmesnie_geojson:se:2caeebfaebcb38adb3fd', N'S.E. CHICOASEN(MANUEL MORENO TORRES)', 400, 16.9394862, -93.0952863),
        (N'dgmesnie_geojson:se:0ab3f417b469228177f5', N'SE JUCHITAN II', 230, 16.5176573, -94.9661315),
        (N'dgmesnie_geojson:se:398e1f63822f148f872e', N'SE TRANSÍSTMICA', 115, 16.3925371, -95.1173774),
        (N'dgmesnie_geojson:se:4ed78fafdce5b164f4c9', N'S.E. CHARCAS POTENCIA', 115, 23.0985847, -101.1032989),
        (N'dgmesnie_geojson:se:985f4a8077ccd67540fe', N'S.E. MATEHUALA', 115, 23.6664550, -100.6458879),
        (N'dgmesnie_geojson:se:400548f7b75980f122a4', N'SE PUEBLO NUEVO', 115, 27.0701112, -109.3740516),
        (N'dgmesnie_geojson:se:ecace335ebb282004455', N'SE EL MAYO', 115, 26.8641498, -109.3782811),
        (N'dgmesnie_geojson:se:f2d336619a5d83370124', N'S.E. EL FLORIDO', 115, 32.4723276, -116.7839520),
        (N'dgmesnie_geojson:se:7d2b68cc601a063f1ad0', N'S.E. FRANCISCO VILLA (MATAMOROS)', 115, 32.4872849, -116.8411590),
        (N'dgmesnie_geojson:se:61fb9da01178c24c8586', N'SE TECATE II', 115, 32.5416793, -116.6277321),
        (N'dgmesnie_geojson:se:5708c628bdefae915605', N'SE EL ENCINAL', 115, 32.5568600, -116.5440611);

    IF
    (
        SELECT COUNT(*)
        FROM @Activos a
        JOIN dgmesnie.RedElectricaSubestacionInventario i
          ON i.RegistroClave = a.RegistroClave
         AND i.Activa = 1
         AND i.Nombre = a.Nombre
         AND i.TensionKv = a.TensionKv
         AND ABS(i.Latitud - a.Latitud) < 0.000001
         AND ABS(i.Longitud - a.Longitud) < 0.000001
         AND i.EstadoConciliacion = N'canonico_dgmesnie'
         AND i.EstadoValidacion = N'catalogado'
    ) <> 12
        THROW 54404, N'Preflight: cambió o desapareció alguno de los doce activos canónicos requeridos.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaArista a
        WHERE a.VersionId = @VersionRed
          AND a.AristaClave = N'ar:1df87cf899825491cb08'
          AND a.Nombre = N'L.T. MATEHUALA - CHARCAS POT. 73720'
          AND a.TensionKv = 115
          AND a.EstadoConexion = N'conectada'
          AND a.ConfianzaOrigen >= 85
          AND a.ConfianzaDestino >= 85
          AND JSON_VALUE(a.GeometriaJson, '$.type') = N'LineString'
          AND a.LongitudGeometriaKm BETWEEN 97 AND 99
    )
        THROW 54405, N'Preflight: cambió la traza canónica Charcas Potencia–Matehuala.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaArista a
        WHERE a.VersionId = @VersionRed
          AND a.AristaClave = N'ar:fd14e41135ea00ba900c'
          AND a.Nombre = N'LT METROPOLI - TIJUANA I'
          AND a.TensionKv = 69
          AND a.EstadoConexion = N'conectada'
          AND a.ConfianzaOrigen >= 85
          AND a.ConfianzaDestino >= 85
          AND JSON_VALUE(a.GeometriaJson, '$.type') = N'LineString'
          AND a.LongitudGeometriaKm BETWEEN 8 AND 8.6
    )
        THROW 54406, N'Preflight: cambió la traza canónica Metrópoli–Tijuana I.', 1;

    CREATE TABLE #Geometrias
    (
        ProyectoId BIGINT NOT NULL,
        Etiqueta NVARCHAR(300) NOT NULL,
        TipoGeometria NVARCHAR(30) NOT NULL,
        GeometriaJson NVARCHAR(MAX) NOT NULL,
        Latitud DECIMAL(9,6) NULL,
        Longitud DECIMAL(10,6) NULL,
        Entidad NVARCHAR(200) NULL,
        PrecisionUbicacion NVARCHAR(40) NOT NULL,
        RadioSugeridoKm DECIMAL(8,2) NULL,
        Orden INT NOT NULL,
        EsPrincipal BIT NOT NULL,
        Observacion NVARCHAR(1000) NOT NULL
    );

    INSERT #Geometrias VALUES
        (252, N'S.E. San Cristóbal Oriente · extremo P25-OR2', N'Point', N'{"type":"Point","coordinates":[-92.6005978,16.7376419]}', 16.737642, -92.600598, N'Chiapas', N'geocodificada', 0.25, 1, 1, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 seleccionada para P25-OR2 considera el segundo circuito Manuel Moreno Torres–San Cristóbal Oriente; este punto representa el extremo San Cristóbal Oriente, no toda la obra.'),
        (252, N'S.E. Manuel Moreno Torres (Chicoasén) · extremo P25-OR2', N'Point', N'{"type":"Point","coordinates":[-93.0952863,16.9394862]}', 16.939486, -93.095286, N'Chiapas', N'geocodificada', 0.25, 2, 0, N'COBERTURA PARCIAL VERIFICADA: extremo Manuel Moreno Torres del segundo circuito seleccionado; no se publica como traza la línea local porque su otro extremo nominal no distingue San Cristóbal Oriente.'),

        (253, N'S.E. Juchitán II · transformación P24-OR2', N'Point', N'{"type":"Point","coordinates":[-94.9661315,16.5176573]}', 16.517657, -94.966132, N'Oaxaca', N'geocodificada', 0.25, 1, 1, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 seleccionada para P24-OR2 instala un autotransformador 230/115 kV de 100 MVA en Juchitán II; el punto representa la subestación, no la posición interna del banco.'),
        (253, N'S.E. Transístmica · compensación P24-OR2', N'Point', N'{"type":"Point","coordinates":[-95.1173774,16.3925371]}', 16.392537, -95.117377, N'Oaxaca', N'geocodificada', 0.25, 2, 0, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 seleccionada incluye un banco de capacitores de 7.5 MVAr en 115 kV en Transístmica. No se publica Tapanatepec ni la nueva LT Tecnológico–entronque por falta de geometría canónica inequívoca.'),

        (257, N'S.E. Pueblo Nuevo · transformación P25-NO1', N'Point', N'{"type":"Point","coordinates":[-109.3740516,27.0701112]}', 27.070111, -109.374052, N'Sonora', N'geocodificada', 0.25, 1, 1, N'COBERTURA PARCIAL VERIFICADA: P25-NO1 selecciona un nuevo autotransformador de 225 MVA en Pueblo Nuevo y el traslado a esta SE del banco de 225 MVA de El Mayo.'),
        (257, N'S.E. El Mayo · reubicación P25-NO1', N'Point', N'{"type":"Point","coordinates":[-109.3782811,26.8641498]}', 26.864150, -109.378281, N'Sonora', N'geocodificada', 0.25, 2, 0, N'COBERTURA PARCIAL VERIFICADA: P25-NO1 traslada el autotransformador de 225 MVA de El Mayo a Pueblo Nuevo y los dos bancos de 100 MVA de Pueblo Nuevo a El Mayo.'),

        (265, N'S.E. El Florido · transformación P25-BC2', N'Point', N'{"type":"Point","coordinates":[-116.7839520,32.4723276]}', 32.472328, -116.783952, N'Baja California', N'geocodificada', 0.25, 2, 0, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 de P25-BC2 incluye bancos 115/69 kV y 115/13.8 kV en El Florido; el punto representa la subestación.'),
        (265, N'S.E. Francisco Villa (Matamoros) · transformación P25-BC2', N'Point', N'{"type":"Point","coordinates":[-116.8411590,32.4872849]}', 32.487285, -116.841159, N'Baja California', N'geocodificada', 0.25, 3, 0, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 de P25-BC2 incluye un banco de 30 MVA, 115/13.8 kV, en Francisco Villa; la identidad territorial coincide con Francisco Villa (Matamoros) del inventario canónico.'),
        (265, N'S.E. Tecate II · transformación P25-BC2', N'Point', N'{"type":"Point","coordinates":[-116.6277321,32.5416793]}', 32.541679, -116.627732, N'Baja California', N'geocodificada', 0.25, 4, 0, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 de P25-BC2 incluye un banco de 30 MVA, 115/13.8 kV, en Tecate Dos; el inventario canónico lo identifica como Tecate II.'),
        (265, N'S.E. El Encinal · compensación P25-BC2', N'Point', N'{"type":"Point","coordinates":[-116.5440611,32.5568600]}', 32.556860, -116.544061, N'Baja California', N'geocodificada', 0.25, 5, 0, N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 de P25-BC2 incluye un banco de capacitores de 30 MVAr en 115 kV en Encinal; el punto representa la subestación.');

    INSERT #Geometrias
    SELECT
        254,
        N'Corredor Charcas Potencia–Matehuala · segundo circuito P25-OC1',
        N'LineString',
        a.GeometriaJson,
        NULL, NULL, N'San Luis Potosí', N'geocodificada', 0.50, 1, 1,
        N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 seleccionada tiende un segundo circuito Charcas Potencia–Matehuala sobre estructuras existentes, estimado oficialmente en 96 km. La traza canónica mide 98.195 km; se conserva como referencia del corredor, no como levantamiento as-built de la futura obra.'
    FROM dgmesnie.RedElectricaArista a
    WHERE a.VersionId = @VersionRed
      AND a.AristaClave = N'ar:1df87cf899825491cb08';

    INSERT #Geometrias VALUES
        (254, N'S.E. Charcas Potencia · extremo P25-OC1', N'Point', N'{"type":"Point","coordinates":[-101.1032989,23.0985847]}', 23.098585, -101.103299, N'San Luis Potosí', N'geocodificada', 0.25, 2, 0, N'Extremo verificado del corredor seleccionado Charcas Potencia–Matehuala; no representa por sí solo todo P25-OC1.'),
        (254, N'S.E. Matehuala · extremo P25-OC1', N'Point', N'{"type":"Point","coordinates":[-100.6458879,23.6664550]}', 23.666455, -100.645888, N'San Luis Potosí', N'geocodificada', 0.25, 3, 0, N'Extremo verificado del corredor seleccionado Charcas Potencia–Matehuala; no representa por sí solo todo P25-OC1.');

    INSERT #Geometrias
    SELECT
        265,
        N'Corredor Metrópoli–Tijuana I · cambio 69/115 kV P25-BC2',
        N'LineString',
        a.GeometriaJson,
        NULL, NULL, N'Baja California', N'geocodificada', 0.50, 1, 1,
        N'COBERTURA PARCIAL VERIFICADA: la Alternativa 1 seleccionada contempla el tendido del tercer circuito Metrópoli Potencia–Tijuana I, estimado en 8 km, y el cambio de operación de 69 a 115 kV. La traza canónica existente mide 8.291 km; no representa el tercer circuito construido ni la totalidad de P25-BC2.'
    FROM dgmesnie.RedElectricaArista a
    WHERE a.VersionId = @VersionRed
      AND a.AristaClave = N'ar:fd14e41135ea00ba900c';

    IF (SELECT COUNT(*) FROM #Geometrias) <> 14
        THROW 54407, N'Preflight: se esperaban exactamente catorce geometrías.', 1;

    IF EXISTS
    (
        SELECT 1 FROM #Geometrias
        WHERE ISJSON(GeometriaJson) <> 1
           OR JSON_VALUE(GeometriaJson, '$.type') <> TipoGeometria
    )
        THROW 54408, N'Preflight: existe una geometría JSON inválida o con tipo incongruente.', 1;

    IF EXISTS
    (
        SELECT ProyectoId
        FROM #Geometrias
        GROUP BY ProyectoId
        HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
    )
        THROW 54409, N'Preflight: cada PEM debe tener exactamente una geometría principal.', 1;

    DECLARE @Insertadas TABLE
    (
        UbicacionId BIGINT,
        ProyectoId BIGINT,
        Etiqueta NVARCHAR(300),
        TipoGeometria NVARCHAR(30),
        EsPrincipal BIT
    );

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta,
           inserted.TipoGeometria, inserted.EsPrincipal
      INTO @Insertadas
    SELECT
        g.ProyectoId, g.Etiqueta, g.TipoGeometria, g.GeometriaJson,
        g.Latitud, g.Longitud, g.Entidad,
        g.PrecisionUbicacion, @Metodo,
        LEFT(CONCAT(
            N'CENACE · PAMRNT 2025-2039, ficha ', e.ClaveProyecto,
            N'; DGMESNIE · inventario y grafo eléctrico canónico'), 500),
        f.FechaCorte, g.RadioSugeridoKm, g.Orden, g.EsPrincipal,
        1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(g.Observacion, N' Lote=', @Lote, N'.'), 1000)
    FROM #Geometrias g
    JOIN @Esperados e ON e.ProyectoId = g.ProyectoId
    JOIN dgmesnie.PAMProyectoVersion v
      ON v.ProyectoId = g.ProyectoId
     AND v.EsVersionVigente = 1
     AND v.ClaveProyecto = e.ClaveProyecto
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId;

    IF (SELECT COUNT(*) FROM @Insertadas) <> 14
        THROW 54410, N'Aplicación: no se insertaron exactamente las catorce geometrías.', 1;

    IF
    (
        SELECT COUNT(DISTINCT ProyectoId)
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (252, 253, 254, 257, 265)
          AND Activa = 1 AND Validada = 1
          AND MetodoUbicacion = @Metodo
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 5
        THROW 54411, N'Aplicación: el lote no dejó los cinco PEM representados.', 1;

    COMMIT TRANSACTION;

    SELECT * FROM @Insertadas
    ORDER BY ProyectoId, EsPrincipal DESC, UbicacionId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
