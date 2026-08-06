SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-06';
DECLARE @Metodo NVARCHAR(80) = N'referencia_localidad_oficial_y_red';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 19.024860);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -104.311040);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 72
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'D18-OC6'
          AND v.NombreProyecto = N'Campos Banco 1 (SF6)'
          AND v.GRT = N'OC'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'En Operación'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%LT Campos%Terminal de Gas Manzanillo%'
          AND v.ElementosEquiposAsociados LIKE N'%SE Campos Banco 1%'
    )
        THROW 53501, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte del Proyecto 72.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 72 AND Activa = 1
    )
        THROW 53502, N'Preflight: D18-OC6 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 53503, N'Preflight: el lote ya fue aplicado.', 1;

    IF @Latitud NOT BETWEEN CONVERT(decimal(9,6), 19.000000) AND CONVERT(decimal(9,6), 19.060000)
       OR @Longitud NOT BETWEEN CONVERT(decimal(10,6), -104.350000) AND CONVERT(decimal(10,6), -104.280000)
        THROW 53504, N'Preflight: la referencia no cae en Campos, Manzanillo, Colima.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario
        WHERE Activa = 1
          AND Nombre = N'S.E. MANZANILLO I Y II'
          AND TensionKv = CONVERT(decimal(9,3), 400.000)
          AND Latitud BETWEEN CONVERT(decimal(9,6), 19.020000) AND CONVERT(decimal(9,6), 19.035000)
          AND Longitud BETWEEN CONVERT(decimal(10,6), -104.325000) AND CONVERT(decimal(10,6), -104.305000)
    )
        THROW 53505, N'Preflight: no está disponible la corroboración del complejo eléctrico de Manzanillo en el inventario de red.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Direccion, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        72,
        N'Campos · referencia territorial D18-OC6',
        N'Point',
        N'{"type":"Point","coordinates":[-104.311040,19.024860]}',
        @Latitud,
        @Longitud,
        N'Ejido de Campos, C.P. 28809',
        N'Colima',
        N'Manzanillo',
        N'Campos',
        N'geocodificada',
        @Metodo,
        N'CFE · Informe Anual 2023 e Informe de Gestión 2018-2024; DOF · complejo eléctrico Ejido de Campos; CENACE · PRODESEN 2024-2038; inventario de red DGMESNIE/OSM',
        f.FechaCorte,
        CONVERT(decimal(8,2), 1.50),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'REFERENCIA TERRITORIAL, NO COORDENADA TOPOGRÁFICA: CFE ubica Campos Banco 1 (SF6) en el municipio de Manzanillo, Colima, y confirma su entrada en operación el 19-oct-2023. ',
            N'El expediente PAM identifica SE Campos Banco 1 y la LT Campos-Terminal de Gas Manzanillo de 115 kV y 0.1 km-C. ',
            N'El DOF ubica el complejo eléctrico de Manzanillo en Ejido de Campos s/n; el inventario de red confirma S.E. Manzanillo I y II a menos de 1 km de la localidad Campos. ',
            N'La lámina 122 del Informe Anual 2023 contiene una imagen de detalle rotulada SE Quilá MVAr, por lo que se descartó para coordenada exacta. ',
            N'El punto corresponde a la localidad Campos (OSM node 8589835588), con radio de 1.5 km; no representa el centro geométrico exacto de la S.E. Campos. ',
            N'Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 72 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 53506, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 72 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'geocodificada'
          AND MetodoUbicacion = @Metodo
          AND RadioSugeridoKm = CONVERT(decimal(8,2), 1.50)
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%REFERENCIA TERRITORIAL, NO COORDENADA TOPOGRÁFICA:%'
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 53507, N'Aplicación: la referencia territorial no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, RadioSugeridoKm,
           Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 72 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
