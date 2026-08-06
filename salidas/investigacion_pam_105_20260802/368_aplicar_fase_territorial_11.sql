SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-11';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-04';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 174 AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P21-NO2'
          AND v.NombreProyecto = N'Compensación capacitiva al sur de la zona Culiacán'
          AND v.GRT = N'NO'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Instruido y CON priorización'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%SE El Dorado%'
          AND v.ElementosEquiposAsociados LIKE N'%15 MVAr%'
          AND v.ElementosEquiposAsociados LIKE N'%115 kV%'
    )
        THROW 54301, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte de P21-NO2.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 249 AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'M24-BC1'
          AND v.NombreProyecto = N'Incremento en la confiabilidad de suministro en la región de Valle de la Palmas'
          AND v.GRT = N'BC'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Instruido y SIN priorización'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%El Fortín Maniobras%'
          AND v.ElementosEquiposAsociados LIKE N'%Valle de las Palmas%'
          AND v.ElementosEquiposAsociados LIKE N'%Operación inicial 69 kV%'
    )
        THROW 54302, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte de M24-BC1.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (151, 174, 175, 199, 249) AND Activa = 1
    )
        THROW 54303, N'Preflight: uno de los cinco proyectos ya tiene ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54304, N'Preflight: el lote ya fue aplicado.', 1;

    -- P21-NO2: CFE publica coordenadas expresas para la S.E. Reductora Eldorado (ELD).
    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        174,
        N'S.E. Eldorado · P21-NO2',
        N'Point', N'{"type":"Point","coordinates":[-107.374392,24.316984]}',
        CONVERT(decimal(9,6), 24.316984), CONVERT(decimal(10,6), -107.374392), N'Sinaloa',
        N'exacta', N'coordenada_cfe_expresa',
        N'CFE Distribución · S.E. Reductora Eldorado (ELD), 24.3169842055556, -107.374392248199; CFE RGD · 04-EDR-115-1; CENACE · P21-NO2',
        f.FechaCorte, CONVERT(decimal(8,2), 0.25),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'COORDENADA EXACTA DOCUMENTAL: CFE publica para la S.E. Reductora Eldorado (ELD) las coordenadas 24.3169842055556, -107.374392248199. ',
            N'El inventario RGD identifica ELDORADO, banco 04-EDR-115-1, 115/34.5 kV y 30 MVA; el expediente vigente agrega un capacitor de 15 MVAr en 115 kV. ',
            N'OpenStreetMap contiene un polígono 115 kV aproximadamente 1.45 km al noreste; se conserva la coordenada oficial de CFE y se documenta la discrepancia para revisión cartográfica. ',
            N'El punto representa la subestación, no la posición interna del capacitor. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 174 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54305, N'Aplicación: no se insertó exactamente una ubicación para P21-NO2.', 1;

    -- M24-BC1: extremo existente comprobado; no representa la futura S.E. El Fortín ni las trazas.
    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        249,
        N'S.E. Valle de las Palmas · extremo existente M24-BC1',
        N'Point', N'{"type":"Point","coordinates":[-116.741677,32.411211]}',
        CONVERT(decimal(9,6), 32.411211), CONVERT(decimal(10,6), -116.741677), N'Baja California',
        N'exacta', N'infraestructura_abierta_y_corredor_oficial',
        N'CFE · S.E. Valle de las Palmas; CENACE · M24-BC1; OpenStreetMap way 1174316504, subestación 69 kV',
        f.FechaCorte, CONVERT(decimal(8,2), 0.25),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'COBERTURA PARCIAL, EXTREMO EXISTENTE: CFE documenta la S.E. Valle de las Palmas como instalación propia y el expediente vigente M24-BC1 la incluye expresamente, con operación inicial en 69 kV. ',
            N'OpenStreetMap way 1174316504 identifica el polígono de una subestación de 69 kV en Valle de las Palmas, en 32.4112107, -116.7416773. ',
            N'El punto sólo representa el extremo existente S.E. Valle de las Palmas; no se atribuye a la nueva S.E. El Fortín Maniobras ni sustituye las trazas Herradura–Alpha–Valle de las Palmas o Valle de las Palmas–Vallecitos–Valle de Guadalupe. ',
            N'Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 249 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54306, N'Aplicación: no se insertó exactamente una ubicación para M24-BC1.', 1;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (174, 249) AND Activa = 1 AND Validada = 1
          AND Observaciones LIKE N'%' + @Lote + N'%') <> 2
        THROW 54307, N'Aplicación: el lote no produjo exactamente dos ubicaciones activas y validadas.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (151, 175, 199) AND Activa = 1
    )
        THROW 54308, N'Aplicación: un caso pendiente recibió una ubicación no autorizada.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (174, 249) AND Observaciones LIKE N'%' + @Lote + N'%'
          AND (ISJSON(GeometriaJson) <> 1 OR Latitud NOT BETWEEN 14.0 AND 33.5 OR Longitud NOT BETWEEN -118.0 AND -86.0)
    )
        THROW 54309, N'Aplicación: alguna geometría o coordenada no superó la validación territorial de México.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, RadioSugeridoKm,
           Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE Observaciones LIKE N'%' + @Lote + N'%'
    ORDER BY ProyectoId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
