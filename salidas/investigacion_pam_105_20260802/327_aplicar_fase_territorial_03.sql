SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-03';
DECLARE @Metodo NVARCHAR(80) = N'documental_oficial_cfe_cobertura_parcial';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 16.985489);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -93.160639);
DECLARE @Punto geography = geography::Point(@Latitud, @Longitud, 4326);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 294
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'CFE25-PHC'
          AND v.NombreProyecto = N'Interconexión PH Chicoasén II'
          AND v.GRT = N'SE'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 52601, N'Preflight: cambió la identidad, región, vigencia o corte del Proyecto 294.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 294 AND Activa = 1
    )
        THROW 52602, N'Preflight: Chicoasén II ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 52603, N'Preflight: el lote ya fue aplicado.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario i
        WHERE i.RegistroClave = N'dgmesnie_geojson:se:2caeebfaebcb38adb3fd'
          AND i.Activa = 1
          AND i.NombreNormalizado = N'CHICOASEN MANUEL MORENO TORRES'
          AND geography::Point(i.Latitud, i.Longitud, 4326).STDistance(@Punto)
              BETWEEN 8000 AND 9500
    )
        THROW 52604, N'Preflight: cambió la referencia independiente de Manuel Moreno Torres usada para distinguir ambos sitios.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, FechaValidacionUtc,
        UsuarioValidacion, Observaciones
    )
    SELECT
        294,
        N'PH Chicoasén II · origen; interconexión pendiente',
        N'Point',
        N'{"type":"Point","coordinates":[-93.160639,16.985489]}',
        @Latitud,
        @Longitud,
        N'exacta',
        @Metodo,
        N'CFE · Términos de Referencia PH Chicoasén II · zona de obras 16°59''07.76" N, 93°09''38.30" O',
        f.FechaCorte,
        CONVERT(decimal(8,2), 0.75),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'COBERTURA PARCIAL: punto oficial de la zona de obras del PH Chicoasén II, complejo que incluye subestación elevadora. ',
            N'No representa la traza ni los extremos todavía no publicados de la interconexión CFE25-PHC. ',
            N'CFE ubica el sitio 8.5 km aguas abajo de la CH Manuel Moreno Torres; el inventario DGMESNIE confirma que esa instalación distinta está a 8.624 km. ',
            N'No modifica la relación pendiente 274→294. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 294 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 52605, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 294 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND MetodoUbicacion = @Metodo
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%COBERTURA PARCIAL:%'
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 52606, N'Aplicación: la ubicación parcial no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 294 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
