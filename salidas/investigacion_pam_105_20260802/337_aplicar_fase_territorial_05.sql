SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-05';
DECLARE @Metodo NVARCHAR(80) = N'ubicacion_documental_oficial_utm';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 24.046717);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -110.294003);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 68
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P16-BS2'
          AND v.NombreProyecto = N'Camino Real MVAr'
          AND v.GRT = N'BC'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'En concurso'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 53201, N'Preflight: cambió la identidad, región, etapa, vigencia o corte del Proyecto 68.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 68 AND Activa = 1
    )
        THROW 53202, N'Preflight: P16-BS2 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 53203, N'Preflight: el lote ya fue aplicado.', 1;

    IF @Latitud NOT BETWEEN CONVERT(decimal(9,6), 24.000000) AND CONVERT(decimal(9,6), 24.100000)
       OR @Longitud NOT BETWEEN CONVERT(decimal(10,6), -110.350000) AND CONVERT(decimal(10,6), -110.250000)
        THROW 53204, N'Preflight: la conversión UTM no cae en el sur de la ciudad de La Paz.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Direccion, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        68,
        N'S.E. Camino Real · P16-BS2',
        N'Point',
        N'{"type":"Point","coordinates":[-110.294003,24.046717]}',
        @Latitud,
        @Longitud,
        N'San Antonio El Zacatal, prolongación avenida del Litoral s/n sur, fraccionamiento Ayuntamiento II, C.P. 23088',
        N'Baja California Sur',
        N'La Paz',
        N'La Paz',
        N'geocodificada',
        @Metodo,
        N'CFE · expediente técnico y domicilio S.E. Camino Real; CENACE · PRODECEN P16-BS2',
        f.FechaCorte,
        CONVERT(decimal(8,2), 0.25),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'UBICACIÓN DOCUMENTAL: CENACE identifica P16-BS2 como compensación en la S.E. Camino Real al sur de La Paz. ',
            N'CFE confirma el domicilio del patio en San Antonio El Zacatal, prolongación avenida del Litoral s/n sur, Ayuntamiento II, C.P. 23088. ',
            N'El expediente técnico reporta coordenadas aproximadas UTM WGS84 zona 12N E=571783, N=2659579; conversión EPSG:32612→4326: 24.046717, -110.294003. ',
            N'Se conserva precision aproximada_documental y radio de 0.25 km. La diferencia histórica 7.5/22.5 MVAr no altera la identidad territorial. ',
            N'Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 68 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 53205, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 68 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'geocodificada'
          AND MetodoUbicacion = @Metodo
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%UBICACIÓN DOCUMENTAL:%'
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 53206, N'Aplicación: la ubicación no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 68 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
