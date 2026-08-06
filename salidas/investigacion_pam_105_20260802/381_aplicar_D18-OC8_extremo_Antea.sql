SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-D18-OC8-ANTEA-20260806';
DECLARE @Metodo NVARCHAR(80) = N'conciliacion_diagrama_cenace_osm';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-06';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 79
          AND v.ProyectoVersionId = 1542
          AND v.EsVersionVigente = 1
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.ClaveProyecto = N'D18-OC8'
          AND v.NombreProyecto = N'Pedregal Banco 1'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 54631, N'Preflight: cambió la identidad, versión, vigencia o corte de D18-OC8.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 79
          AND Activa = 1
    )
        THROW 54632, N'Preflight: D18-OC8 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario
        WHERE RegistroClave = N'openstreetmap:e4875a26c472e16215c4b06e773f1ecf67593e4e'
          AND Activa = 1
          AND Nombre = N'Antea'
          AND TensionKv = CONVERT(decimal(9,3), 115.000)
          AND ABS(Latitud - CONVERT(decimal(10,7), 20.6748200)) < 0.0000001
          AND ABS(Longitud - CONVERT(decimal(11,7), -100.4318200)) < 0.0000001
          AND FuenteCoordenadas = N'openstreetmap'
    )
        THROW 54633, N'Preflight: cambió o desapareció el punto de referencia Antea.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54634, N'Preflight: el lote D18-OC8 ya fue aplicado.', 1;

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
        79,
        N'S.E. Antea · extremo documentado de D18-OC8',
        N'Point',
        N'{"type":"Point","coordinates":[-100.4318200,20.6748200]}',
        CONVERT(decimal(9,6), 20.674820),
        CONVERT(decimal(10,6), -100.431820),
        N'Extremo Antea de la L.T. Pedregal-Antea; no representa la ubicación aún no conciliada de S.E. Pedregal.',
        N'Querétaro',
        N'Querétaro',
        NULL,
        N'geocodificada',
        @Metodo,
        N'CENACE · Diagramas Unifilares RNT y RGD del MEM 2024-2029, zona Querétaro, D18-OC8; OpenStreetMap · power=substation Antea; DGMESNIE · inventario conciliado',
        NULL,
        CONVERT(decimal(8,2), 0.25),
        1,
        1,
        1,
        1,
        @Usuario,
        SYSUTCDATETIME(),
        @Usuario,
        N'COBERTURA PARCIAL VERIFICADA: el diagrama unifilar oficial identifica D18-OC8 en Pedregal y su conexión con Antea a 115 kV; el inventario territorial aporta el punto OSM de Antea con tensión compatible. No se publica la traza Pedregal-Antea ni un punto Pedregal por falta de geometría canónica inequívoca. Lote=PAM-TERRITORIAL-D18-OC8-ANTEA-20260806.'
    );

    IF @@ROWCOUNT <> 1
        THROW 54635, N'Aplicación: no se insertó exactamente el extremo Antea.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId = 79
  AND Observaciones LIKE N'%' + @Lote + N'%';
