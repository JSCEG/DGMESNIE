SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT = 19;
DECLARE @PEM NVARCHAR(30) = N'P16-CE1';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-INDIVIDUAL-20260802-003';
DECLARE @Metodo NVARCHAR(80) = N'investigacion_individual_conciliada_v2';
DECLARE @Usuario NVARCHAR(300) = N'Codex - autorizado por usuario - 2026-08-02';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.vw_PAMProyectoVigente
        WHERE ProyectoId = @ProyectoId
          AND ClaveProyecto = @PEM
          AND EstadoVigenciaCartera = N'Vigente'
    )
        THROW 51301, N'Preflight: cambio la identidad o vigencia de P16-CE1.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = @ProyectoId AND Activa = 1
    )
        THROW 51302, N'Preflight: P16-CE1 ya tiene una ubicacion activa.', 1;

    CREATE TABLE #Geometrias
    (
        Etiqueta NVARCHAR(300) NOT NULL,
        TipoGeometria NVARCHAR(30) NOT NULL,
        GeometriaJson NVARCHAR(MAX) NOT NULL,
        Latitud DECIMAL(10,7) NULL,
        Longitud DECIMAL(11,7) NULL,
        PrecisionUbicacion NVARCHAR(40) NOT NULL,
        RadioSugeridoKm DECIMAL(8,2) NULL,
        Orden INT NOT NULL,
        EsPrincipal BIT NOT NULL,
        Observacion NVARCHAR(1000) NOT NULL
    );

    INSERT #Geometrias
    VALUES
    (
        N'LT Kilometro 110 - Tulancingo 85 kV (73T30)',
        N'LineString',
        N'{"type":"LineString","coordinates":[[-98.303003,19.9809481],[-98.3011227,19.9819508],[-98.3021884,19.9830109],[-98.3028139,19.9836267],[-98.3047542,19.9855457],[-98.3061602,19.9869334],[-98.3074425,19.9882213],[-98.3088372,19.9896001],[-98.3102051,19.990936],[-98.3115541,19.9922821],[-98.3129919,19.9937086],[-98.3141721,19.9948908],[-98.3151404,19.9958385],[-98.3174732,19.9981516],[-98.3186755,19.9993451],[-98.3210646,20.0017042],[-98.3221486,20.0027871],[-98.3238226,20.0038762],[-98.3255184,20.0049852],[-98.3273108,20.0061508],[-98.3304624,20.0082062],[-98.3322702,20.0093794],[-98.3339171,20.0104467],[-98.3354346,20.011441],[-98.3369688,20.012432],[-98.3392802,20.013941],[-98.3406334,20.0148244],[-98.3421964,20.0158381],[-98.3464605,20.0186084],[-98.3483923,20.0198817],[-98.3511744,20.021683],[-98.3535529,20.0231566],[-98.3549564,20.0240305],[-98.3564799,20.0249667],[-98.3582374,20.0260604],[-98.3598004,20.0270255],[-98.3614205,20.0280298],[-98.3630231,20.0290881],[-98.3646311,20.0301541],[-98.3662994,20.0312578],[-98.3678397,20.0322705],[-98.36951,20.0333695],[-98.3713782,20.0346021],[-98.3730458,20.0357061],[-98.3769002,20.0382902],[-98.3779898,20.0390021],[-98.3797755,20.0401826],[-98.3813332,20.0412063],[-98.3826254,20.0420649],[-98.3841904,20.0430854],[-98.3857884,20.0441462],[-98.387344,20.0451737],[-98.3906646,20.0473708],[-98.3938658,20.0494823],[-98.3994676,20.0531761],[-98.402923,20.0554796],[-98.4046141,20.0565756],[-98.4063414,20.0577617],[-98.407875,20.0587204],[-98.4095594,20.0598239],[-98.4112459,20.0609369],[-98.4128653,20.0620165],[-98.4140468,20.0627805],[-98.4162918,20.0642645],[-98.4178783,20.0653176],[-98.4191215,20.0661175],[-98.4195265,20.0665004]]}',
        NULL, NULL, N'geocodificada', CONVERT(DECIMAL(8,2), 0.50), 1, 1,
        N'Corredor 73T30 confirmado por CENACE. Trazo OSM way/109683014: 85 kV, 1 circuito, 67 vertices, 15.817 km; no representa un levantamiento as-built.'
    ),
    (
        N'SE Kilometro 110', N'Point',
        N'{"type":"Point","coordinates":[-98.3018077,19.9808438]}',
        CONVERT(DECIMAL(10,7), 19.9808438), CONVERT(DECIMAL(11,7), -98.3018077),
        N'geocodificada', CONVERT(DECIMAL(8,2), 0.20), 2, 0,
        N'Extremo 73T30 confirmado por CENACE y CFE; patio OSM way/109680812, 230/85 kV.'
    ),
    (
        N'SE Tulancingo', N'Point',
        N'{"type":"Point","coordinates":[-98.4196100,20.0665600]}',
        CONVERT(DECIMAL(10,7), 20.0665600), CONVERT(DECIMAL(11,7), -98.4196100),
        N'geocodificada', CONVERT(DECIMAL(8,2), 0.20), 3, 0,
        N'Extremo 73T30 confirmado por CENACE; patio OSM way/1189464527 y bancos 01-TLG-85-1/2/3 en el inventario CFE RGD 2026.'
    );

    IF (SELECT COUNT(*) FROM #Geometrias) <> 3
        THROW 51303, N'Preflight: se esperaban tres geometrias.', 1;
    IF EXISTS (SELECT 1 FROM #Geometrias WHERE ISJSON(GeometriaJson) <> 1)
        THROW 51304, N'Preflight: existe una geometria JSON invalida.', 1;
    IF (SELECT SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) FROM #Geometrias) <> 1
        THROW 51305, N'Preflight: debe existir una sola geometria principal.', 1;

    DECLARE @Insertadas TABLE (UbicacionId BIGINT, Etiqueta NVARCHAR(300), TipoGeometria NVARCHAR(30), EsPrincipal BIT);

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.Etiqueta, inserted.TipoGeometria, inserted.EsPrincipal
      INTO @Insertadas
    SELECT
        @ProyectoId, g.Etiqueta, g.TipoGeometria, g.GeometriaJson,
        g.Latitud, g.Longitud, g.PrecisionUbicacion, @Metodo,
        N'CENACE PAMRNT y diagramas unifilares; CFE Transparencia y RGD 2026; OpenStreetMap ODbL',
        p.FechaCorte, g.RadioSugeridoKm, g.Orden, g.EsPrincipal,
        1, 1, @Usuario,
        LEFT(CONCAT(g.Observacion, N' Lote=', @Lote, N'.'), 1000)
    FROM #Geometrias g
    CROSS JOIN
    (
        SELECT TOP (1) FechaCorte
        FROM dgmesnie.vw_PAMProyectoVigente
        WHERE ProyectoId = @ProyectoId AND ClaveProyecto = @PEM
    ) p;

    IF (SELECT COUNT(*) FROM @Insertadas) <> 3
        THROW 51306, N'Aplicacion: no se insertaron las tres geometrias.', 1;

    COMMIT TRANSACTION;
    SELECT * FROM @Insertadas ORDER BY EsPrincipal DESC, UbicacionId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
