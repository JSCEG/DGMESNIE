SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-08';
DECLARE @Metodo NVARCHAR(80) = N'poligono_osm_identidad_corrob_oficial';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 17.793519);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -92.971196);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 20
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P17-OR3'
          AND v.NombreProyecto LIKE N'Tabasco Potencia%'
          AND v.GRT = N'SE'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Instruido y CON priorización'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%SE Tabasco Potencia MVAr%'
          AND v.ElementosEquiposAsociados LIKE N'%63.5 MVAr%400 kV%'
          AND v.ElementosEquiposAsociados LIKE N'%Temascal II%'
    )
        THROW 54001, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte del Proyecto 20.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 20 AND Activa = 1
    )
        THROW 54002, N'Preflight: P17-OR3 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54003, N'Preflight: el lote ya fue aplicado.', 1;

    IF @Latitud NOT BETWEEN CONVERT(decimal(9,6), 17.790000) AND CONVERT(decimal(9,6), 17.797000)
       OR @Longitud NOT BETWEEN CONVERT(decimal(10,6), -92.975000) AND CONVERT(decimal(10,6), -92.968000)
        THROW 54004, N'Preflight: el punto representativo no cae dentro del polígono OSM de Tabasco Potencia.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        20,
        N'S.E. Tabasco Potencia · P17-OR3',
        N'Point',
        N'{"type":"Point","coordinates":[-92.971196,17.793519]}',
        @Latitud,
        @Longitud,
        N'Tabasco',
        N'exacta',
        @Metodo,
        N'CFE · descripción S.E. Tabasco Potencia; CENACE · RNT 2026/PRODESEN 2024-2038; OpenStreetMap way 317273421',
        f.FechaCorte,
        CONVERT(decimal(8,2), 0.40),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'UBICACIÓN DE INSTALACIÓN CORROBORADA: el expediente PAM P17-OR3 identifica la S.E. Tabasco Potencia y el traslado de un reactor de 63.5 MVAr desde Temascal II. ',
            N'CFE confirma que Tabasco Potencia es una subestación 400/230 kV en operación; CENACE la registra como TSP-400/TSP-230 y documenta sus enlaces. ',
            N'OpenStreetMap way 317273421 delimita la subestación con nombre Tabasco Potencia, tensión 400/230 kV; el punto publicado es representativo del interior de ese polígono, no del equipo reactor. ',
            N'Versión OSM 3, 2023-06-13. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 20 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 54005, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 20 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND MetodoUbicacion = @Metodo
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND ISJSON(GeometriaJson) = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54006, N'Aplicación: la ubicación no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 20 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
