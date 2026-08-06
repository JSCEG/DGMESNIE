SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-DOCUMENTADO-20260806-03';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-06';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Esperados TABLE
    (
        ProyectoId BIGINT NOT NULL PRIMARY KEY,
        ProyectoVersionId BIGINT NOT NULL,
        ClaveProyecto NVARCHAR(30) NOT NULL,
        NombreProyecto NVARCHAR(500) NOT NULL,
        FechaCorte DATE NOT NULL
    );

    INSERT @Esperados VALUES
        (256, 1778, N'P25-OC3', N'Incremento de transformación en la Zona Tepic', CONVERT(date, '2026-07-30')),
        (259, 1781, N'P25-NT1', N'Incremento de transformación en la Zona Juárez', CONVERT(date, '2026-07-30')),
        (261, 1783, N'P25-NT3', N'Soporte de tensión en la Zona Cuauhtémoc', CONVERT(date, '2026-07-30')),
        (262, 1784, N'P25-NE1', N'Incremento de Capacidad de Transformación de la zona Monclova', CONVERT(date, '2026-07-30')),
        (278, 278, N'P26-NO1', N'Solución a la saturación de la transformación en el área suroeste de Hermosillo, Sonora', CONVERT(date, '2026-07-10')),
        (279, 279, N'P26-OC2', N'Incremento de transformación en la zona Aguascalientes', CONVERT(date, '2026-07-10'));

    IF
    (
        SELECT COUNT(*)
        FROM @Esperados e
        INNER JOIN dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
            ON v.ProyectoId = e.ProyectoId
           AND v.ProyectoVersionId = e.ProyectoVersionId
           AND v.ClaveProyecto = e.ClaveProyecto
           AND v.NombreProyecto = e.NombreProyecto
           AND v.EsVersionVigente = 1
           AND v.EstadoVigenciaCartera = N'Vigente'
        INNER JOIN dgmesnie.PAMFuente f
            ON f.FuenteId = v.FuenteId
           AND f.FechaCorte = e.FechaCorte
    ) <> 6
        THROW 54701, N'Preflight: cambió la identidad, versión, vigencia o corte de alguno de los seis PEM.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (256, 259, 261, 262, 278, 279)
          AND Activa = 1
    )
        THROW 54702, N'Preflight: alguno de los seis PEM ya tiene ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54703, N'Preflight: el lote ya fue aplicado.', 1;

    DECLARE @Geometrias TABLE
    (
        ProyectoId BIGINT NOT NULL,
        Etiqueta NVARCHAR(300) NOT NULL,
        Latitud DECIMAL(9,6) NOT NULL,
        Longitud DECIMAL(10,6) NOT NULL,
        Entidad NVARCHAR(200) NOT NULL,
        Municipio NVARCHAR(300) NOT NULL,
        Localidad NVARCHAR(300) NOT NULL,
        PrecisionUbicacion NVARCHAR(40) NOT NULL,
        MetodoUbicacion NVARCHAR(80) NOT NULL,
        RadioSugeridoKm DECIMAL(8,2) NOT NULL,
        Orden INT NOT NULL,
        EsPrincipal BIT NOT NULL,
        Fuente NVARCHAR(500) NOT NULL,
        Observacion NVARCHAR(1000) NOT NULL
    );

    -- P25-OC3: la alternativa 1 seleccionada actúa sobre infraestructura existente.
    INSERT @Geometrias VALUES
        (256, N'S.E. Tepic II · transformación principal P25-OC3',
         21.581821, -104.936953, N'Nayarit', N'Tepic', N'Tepic',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 1, 1,
         N'CENACE · PAMRNT 2025-2039, P25-OC3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:bdd77168a2310bf91a6a',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 seleccionada sustituye dos bancos 230/115 kV en la SE Tepic II. La coordenada corresponde al inventario de red; no representa la posición interna de los bancos.'),
        (256, N'S.E. Tepic I · adecuaciones P25-OC3',
         21.526711, -104.881988, N'Nayarit', N'Tepic', N'Tepic',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 2, 0,
         N'CENACE · PAMRNT 2025-2039, P25-OC3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:136a5d26b2b9fdd7e9cc',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 incluye recalibración de bus y puentes en la SE Tepic I.'),
        (256, N'S.E. Tepic Industrial · adecuaciones P25-OC3',
         21.484458, -104.842542, N'Nayarit', N'Tepic', N'Tepic',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 3, 0,
         N'CENACE · PAMRNT 2025-2039, P25-OC3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:1817243474087e8763a3',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 incluye recalibración de bus y puentes en la SE Tepic Industrial.'),
        (256, N'S.E. Compostela · adecuaciones P25-OC3',
         21.249105, -104.903142, N'Nayarit', N'Compostela', N'Compostela',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 4, 0,
         N'CENACE · PAMRNT 2025-2039, P25-OC3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:8ba85d7070c3e9412be0',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 incluye recalibración de bus y puentes en la SE Compostela.'),

    -- P25-NT1: Juárez Potencia es nueva y aún no tiene coordenada; se conserva cobertura urbana y extremos existentes.
        (259, N'Juárez · cobertura municipal de la nueva S.E. Juárez Potencia',
         31.690798, -106.425322, N'Chihuahua', N'Juárez', N'Ciudad Juárez',
         N'municipal', N'municipio_area_influencia_documentada', 20.00, 1, 1,
         N'CENACE · PAMRNT 2025-2039, P25-NT1, alternativa 1; DGMESNIE · municipios.geojson CVEGEO 08037; OpenStreetMap Nominatim · Ciudad Juárez',
         N'REFERENCIA MUNICIPAL: la alternativa 1 seleccionada propone una nueva SE Juárez Potencia, pero el documento no publica coordenadas geoespaciales. El punto representa el municipio de Juárez y su cabecera, no la nueva subestación.'),
        (259, N'S.E. Reforma · intercambio de autotransformador P25-NT1',
         31.586507, -106.462892, N'Chihuahua', N'Juárez', N'Ciudad Juárez',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 2, 0,
         N'CENACE · PAMRNT 2025-2039, P25-NT1, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:7ee4229efa385355aa1e',
         N'COBERTURA PARCIAL DOCUMENTADA: la alternativa 1 incluye traslado/intercambio de autotransformadores en SE Reforma y Ascensión II. El inventario disponible identifica Reforma; Juárez Potencia y Ascensión II quedan sin coordenada.'),
        (259, N'S.E. Valle de Juárez · extremo de entronque P25-NT1',
         31.702384, -106.373122, N'Chihuahua', N'Juárez', N'Ciudad Juárez',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 3, 0,
         N'CENACE · PAMRNT 2025-2039, P25-NT1, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:99687aa31718ba72760e',
         N'COBERTURA PARCIAL DOCUMENTADA: la alternativa 1 entronca la LT Valle de Juárez–Samalayuca Sur en la nueva SE Juárez Potencia. El punto sólo representa el extremo existente.'),
        (259, N'S.E. Terranova · extremos de entronque P25-NT1',
         31.554814, -106.406133, N'Chihuahua', N'Juárez', N'Ciudad Juárez',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 4, 0,
         N'CENACE · PAMRNT 2025-2039, P25-NT1, alternativa 1; Atlas SEN · atlas_sen:c2afa96f5a728c4c0095c91252e0b6a9dfc78c78',
         N'COBERTURA PARCIAL DOCUMENTADA: la alternativa 1 entronca las LT Terranova–Patria y Terranova–Granjero en la nueva SE Juárez Potencia. El punto sólo representa el extremo existente.'),

    -- P25-NT3: alternativa 1 seleccionada, con compensación en cuatro SE; tres están conciliadas en el inventario.
        (261, N'S.E. Álvaro Obregón · STATCOM principal P25-NT3',
         28.716503, -106.913153, N'Chihuahua', N'Cuauhtémoc', N'Álvaro Obregón',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 1, 1,
         N'CENACE · PAMRNT 2025-2039, P25-NT3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:b951cfe2ac43a7fb5996',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 seleccionada instala un STATCOM +/-100 MVAr en 115 kV en la SE Álvaro Obregón.'),
        (261, N'S.E. Cuauhtémoc II · capacitor P25-NT3',
         28.446425, -106.813382, N'Chihuahua', N'Cuauhtémoc', N'Cuauhtémoc',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 2, 0,
         N'CENACE · PAMRNT 2025-2039, P25-NT3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:3e0bbb598f198555d5d3',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 instala un capacitor de 45 MVAr en 115 kV en la SE Cuauhtémoc Dos.'),
        (261, N'S.E. Quevedo · capacitor P25-NT3',
         29.036570, -107.420181, N'Chihuahua', N'Cuauhtémoc', N'Quevedo',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 3, 0,
         N'CENACE · PAMRNT 2025-2039, P25-NT3, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:3203f33f0709b17b6c2c',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 instala un capacitor de 45 MVAr en 115 kV en la SE Quevedo. Namiquipa también forma parte de la alternativa, pero no tiene punto conciliado en el inventario actual.'),

    -- P25-NE1: alternativa 1 seleccionada; Frontera contiene la transformación principal.
        (262, N'S.E. Frontera · transformación principal P25-NE1',
         26.926634, -101.503728, N'Coahuila', N'Frontera', N'Frontera',
         N'exacta', N'alternativa_cenace_inventario_red_v1', 0.50, 1, 1,
         N'CENACE · PAMRNT 2025-2039, P25-NE1, alternativa 1; Atlas SEN/DGMESNIE · dgmesnie_geojson:se:173831903a87680d6e82',
         N'INFRAESTRUCTURA DOCUMENTADA: la alternativa 1 seleccionada instala un banco de 99 MVA, 230/115 kV, en la SE Frontera. Regidores y Cuatro Ciénegas también reciben compensación/adecuaciones, pero no tienen punto conciliado en el inventario actual.'),

    -- P26: el insumo vigente sólo identifica zona; no se promueve una SE por proximidad.
        (278, N'Hermosillo · cobertura municipal de P26-NO1',
         29.094821, -110.969220, N'Sonora', N'Hermosillo', N'Hermosillo',
         N'municipal', N'municipio_area_influencia_documentada', 20.00, 1, 1,
         N'PAMRNT 2026-2040 · proyectos identificados, P26-NO1; DGMESNIE · municipios.geojson CVEGEO 26030; OpenStreetMap Nominatim · Hermosillo',
         N'REFERENCIA MUNICIPAL VALIDADA: el insumo ubica el proyecto en el área suroeste del municipio de Hermosillo, pero no identifica subestación ni coordenada. El punto representa el municipio y su cabecera, no una obra específica.'),
        (279, N'Aguascalientes · cobertura municipal de P26-OC2',
         21.880487, -102.296720, N'Aguascalientes', N'Aguascalientes', N'Aguascalientes',
         N'municipal', N'municipio_area_influencia_documentada', 20.00, 1, 1,
         N'PAMRNT 2026-2040 · proyectos identificados, P26-OC2; DGMESNIE · municipios.geojson CVEGEO 01001; OpenStreetMap Nominatim · Aguascalientes',
         N'REFERENCIA MUNICIPAL VALIDADA: el insumo identifica el municipio de Aguascalientes, pero no una subestación concreta. El punto representa el municipio y su cabecera, no una obra específica.');

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        g.ProyectoId, g.Etiqueta, N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(varchar(30), g.Longitud), N',',
               CONVERT(varchar(30), g.Latitud), N']}'),
        g.Latitud, g.Longitud, g.Entidad, g.Municipio, g.Localidad,
        g.PrecisionUbicacion, g.MetodoUbicacion, g.Fuente, f.FechaCorte,
        g.RadioSugeridoKm, g.Orden, g.EsPrincipal, 1, 1,
        @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(g.Observacion, N' Lote=', @Lote, N'.'), 1000)
    FROM @Geometrias g
    INNER JOIN dgmesnie.PAMProyectoVersion v
        ON v.ProyectoId = g.ProyectoId AND v.EsVersionVigente = 1
    INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId;

    IF @@ROWCOUNT <> 14
        THROW 54704, N'Aplicación: no se insertaron exactamente las catorce referencias documentadas.', 1;

    IF EXISTS
    (
        SELECT ProyectoId
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (256, 259, 261, 262, 278, 279)
          AND Activa = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
        GROUP BY ProyectoId
        HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
    )
        THROW 54705, N'Aplicación: cada PEM debe conservar exactamente una referencia principal.', 1;

    IF
    (
        SELECT COUNT(DISTINCT ProyectoId)
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (256, 259, 261, 262, 278, 279)
          AND Activa = 1 AND Validada = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
    ) <> 6
        THROW 54706, N'Aplicación: el lote no cubrió exactamente los seis PEM.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
          AND (ISJSON(GeometriaJson) <> 1
               OR Latitud NOT BETWEEN 14.0 AND 33.5
               OR Longitud NOT BETWEEN -118.0 AND -86.0)
    )
        THROW 54707, N'Aplicación: alguna geometría o coordenada no superó la validación territorial de México.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       Entidad, Municipio, Localidad, PrecisionUbicacion,
       MetodoUbicacion, RadioSugeridoKm, EsPrincipal, Validada, Activa
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%'
ORDER BY ProyectoId, Orden;
