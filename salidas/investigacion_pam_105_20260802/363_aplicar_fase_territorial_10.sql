SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260804-10';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-04';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 105 AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P19-NO1'
          AND v.NombreProyecto = N'Viñedos MVAr'
          AND v.GRT = N'NO'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Ejecución/Construcción'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%22.5 MVAr%'
          AND v.ElementosEquiposAsociados LIKE N'%115 Kv%'
    )
        THROW 54201, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte de P19-NO1.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 73 AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'D18-NT2'
          AND v.NombreProyecto = N'Sauzal Banco 1'
          AND v.GRT = N'NT'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'En Operación'
          AND f.FechaCorte = CONVERT(date, '2025-11-25')
          AND v.ElementosEquiposAsociados LIKE N'%Zaragoza - Medanos%'
          AND v.ElementosEquiposAsociados LIKE N'%30 MVA - 115/13.8 kV%'
    )
        THROW 54202, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte de D18-NT2.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 81 AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'D18-NT1'
          AND v.NombreProyecto = N'Campo Setenta y Tres Banco 1'
          AND v.GRT = N'NT'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Ejecución/Construcción'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%Menonita - Campo Setenta y Tres%'
          AND v.ElementosEquiposAsociados LIKE N'%Campo Sesenta y Tres Banco 1%'
          AND v.ElementosEquiposAsociados LIKE N'%30 MVA - 115/34.5 kV%'
    )
        THROW 54203, N'Preflight: cambió la identidad, alcance o discrepancia Setenta/Sesenta de D18-NT1.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (73, 81, 105) AND Activa = 1
    )
        THROW 54204, N'Preflight: al menos uno de los tres proyectos ya tiene ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54205, N'Preflight: el lote ya fue aplicado.', 1;

    -- P19-NO1: coordenada expresamente publicada por CFE en la visita de sitio.
    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        105,
        N'S.E. Viñedos · P19-NO1',
        N'Point', N'{"type":"Point","coordinates":[-110.888036,29.279283]}',
        CONVERT(decimal(9,6), 29.279283), CONVERT(decimal(10,6), -110.888036), N'Sonora',
        N'exacta', N'coordenada_cfe_expresa',
        N'CFE · visita de sitio P19-NO1, Carretera San Pedro a Pesqueira km 8.5; CENACE · PRODESEN 2019-2033',
        f.FechaCorte, CONVERT(decimal(8,2), 0.25),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'COORDENADA EXACTA DOCUMENTAL: CFE publica para la S.E. Viñedos las coordenadas 29°16''45.42" N, 110°53''16.93" O, convertidas a 29.279283, -110.888036. ',
            N'El documento identifica expresamente P19-NO1 y CENACE confirma el capacitor de 22.5 MVAr en 115 kV en el área Hermosillo. ',
            N'La ubicación corresponde al predio de la subestación; no representa la posición interna del capacitor. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 105 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54206, N'Aplicación: no se insertó exactamente una ubicación para P19-NO1.', 1;

    -- D18-NT2: referencia territorial de la localidad El Sauzal, Juárez; no es coordenada topográfica de la SE.
    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        73,
        N'El Sauzal, Juárez · referencia territorial D18-NT2',
        N'Point', N'{"type":"Point","coordinates":[-106.322250,31.616620]}',
        CONVERT(decimal(9,6), 31.616620), CONVERT(decimal(10,6), -106.322250), N'Chihuahua',
        N'geocodificada', N'localidad_y_corredor_documentados',
        N'CFE · PAM RGD 2019-2033; CENACE · PAMRNT 2022-2036/2023-2037; GeoNames/OSM · localidad El Sauzal, Juárez',
        f.FechaCorte, CONVERT(decimal(8,2), 2.00),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'REFERENCIA TERRITORIAL VALIDADA, NO COORDENADA TOPOGRÁFICA: CFE ubica Sauzal Banco 1 en la zona Ciudad Juárez y CENACE lo vincula con las sobrecargas Zaragoza-Médanos. ',
            N'El expediente vigente especifica el entronque Zaragoza-Médanos de 2.4 km-C. El punto es el centro de la localidad El Sauzal, Juárez, a 5.71 km de la S.E. Zaragoza del inventario DGMESNIE. ',
            N'Se rechazó expresamente la S.E. El Sauzal de Ensenada, Baja California (31.9065265, -116.7029644), por ser homónima incompatible. ',
            N'Usar el radio de 2 km y no presentar el marcador como predio exacto. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 73 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54207, N'Aplicación: no se insertó exactamente una ubicación para D18-NT2.', 1;

    -- D18-NT1: referencia territorial Campo 73; se conserva la discrepancia Campo 63 del campo de elementos.
    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        81,
        N'Campo Setenta y Tres · referencia territorial D18-NT1',
        N'Point', N'{"type":"Point","coordinates":[-106.786940,29.147780]}',
        CONVERT(decimal(9,6), 29.147780), CONVERT(decimal(10,6), -106.786940), N'Chihuahua',
        N'geocodificada', N'localidad_oficial_y_corredor_documentados',
        N'CFE · PAM RGD 2019-2033; INEGI · localidad 080540039 Campo Setenta y Tres; GeoNames/OSM · Campo 73; inventario DGMESNIE · S.E. Menonita',
        f.FechaCorte, CONVERT(decimal(8,2), 2.00),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'REFERENCIA TERRITORIAL VALIDADA, NO COORDENADA TOPOGRÁFICA: CFE denomina el proyecto Campo Setenta y Tres Banco 1 y lo ubica en la zona Cuauhtémoc; INEGI registra Campo Setenta y Tres como localidad 080540039 de Riva Palacio. ',
            N'GeoNames/OSM sitúa Campo 73 en 29.14778, -106.78694 y el inventario DGMESNIE confirma S.E. Menonita a 25.43 km en línea recta; el expediente declara el corredor Menonita-Campo Setenta y Tres de 36 km-C. ',
            N'Se conserva como incidencia de calidad que el campo ElementosEquiposAsociados dice "SE Campo Sesenta y Tres Banco 1"; no se usó la localidad Campo 63 ni se corrigió la cartera. ',
            N'Usar el radio de 2 km. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 81 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54208, N'Aplicación: no se insertó exactamente una ubicación para D18-NT1.', 1;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (73, 81, 105) AND Activa = 1 AND Validada = 1
          AND Observaciones LIKE N'%' + @Lote + N'%') <> 3
        THROW 54209, N'Aplicación: el lote no produjo exactamente tres ubicaciones activas y validadas.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (73, 81, 105) AND Observaciones LIKE N'%' + @Lote + N'%'
          AND (ISJSON(GeometriaJson) <> 1 OR Latitud NOT BETWEEN 14.0 AND 33.5 OR Longitud NOT BETWEEN -118.0 AND -86.0)
    )
        THROW 54210, N'Aplicación: alguna geometría o coordenada no superó la validación territorial de México.', 1;

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
