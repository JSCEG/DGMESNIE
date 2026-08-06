SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-P20-BS1-20260806';
DECLARE @Metodo NVARCHAR(80) = N'centroide_municipal_documentado';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-06';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 151
          AND v.ProyectoVersionId = 1677
          AND v.EsVersionVigente = 1
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.ClaveProyecto = N'P20-BS1'
          AND v.NombreProyecto = N'Compensación Capacitiva en Zona Los Cabos'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 54601, N'Preflight: cambió la identidad, versión, vigencia o corte de P20-BS1.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 151
          AND Activa = 1
    )
        THROW 54602, N'Preflight: P20-BS1 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54603, N'Preflight: el lote P20-BS1 ya fue aplicado.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Direccion, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    VALUES
    (
        151,
        N'Los Cabos · cobertura municipal documentada de P20-BS1',
        N'Point',
        N'{"type":"Point","coordinates":[-109.753272,23.276622]}',
        CONVERT(decimal(9,6), 23.276622),
        CONVERT(decimal(10,6), -109.753272),
        N'Área de influencia: San José del Cabo, Santiago y Buenavista; no representa la coordenada de las subestaciones.',
        N'Baja California Sur',
        N'Los Cabos',
        NULL,
        N'municipal',
        @Metodo,
        N'CFE · Roadshow Plan de Expansión 2025-2030 V1, lámina 13; CENACE · PRODESEN 2020-2034, ficha P20-BS1; DGMESNIE · municipios.geojson, CVEGEO 03008',
        CONVERT(date, '2026-03-19'),
        CONVERT(decimal(8,2), 50.00),
        1,
        1,
        1,
        1,
        @Usuario,
        SYSUTCDATETIME(),
        @Usuario,
        N'REFERENCIA MUNICIPAL VALIDADA: las fuentes identifican P20-BS1 y sus obras en las SE Monte Real y Buena Vista, 115 kV, dentro de Los Cabos. El punto es el centroide calculado de la geometría municipal CVEGEO 03008 y el radio cubre el municipio; no es la coordenada oficial de ninguna subestación. Lote=PAM-TERRITORIAL-P20-BS1-20260806.'
    );

    IF @@ROWCOUNT <> 1
        THROW 54604, N'Aplicación: no se insertó exactamente una referencia municipal.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    UbicacionId, ProyectoId, Etiqueta, TipoGeometria, Latitud, Longitud,
    Entidad, Municipio, PrecisionUbicacion, MetodoUbicacion,
    RadioSugeridoKm, EsPrincipal, Validada, Activa, Fuente
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 151
  AND Observaciones LIKE N'%' + @Lote + N'%';
