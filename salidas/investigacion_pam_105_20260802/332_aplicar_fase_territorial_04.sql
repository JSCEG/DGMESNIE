SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-04';
DECLARE @Metodo NVARCHAR(80) = N'conciliacion_documental_oficial_topologica';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 22.094586);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -100.907603);
DECLARE @Punto geography = geography::Point(@Latitud, @Longitud, 4326);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 39
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P18-OC1'
          AND v.NombreProyecto = N'San Luis Potosí Banco 3 (traslado)'
          AND v.GRT = N'OC'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'En Operación'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 52901, N'Preflight: cambió la identidad, región, etapa, vigencia o corte del Proyecto 39.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 39 AND Activa = 1
    )
        THROW 52902, N'Preflight: P18-OC1 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 52903, N'Preflight: el lote ya fue aplicado.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario i
        WHERE i.RegistroClave = N'dgmesnie_geojson:se:b83bfc7077e57d646d9a'
          AND i.Activa = 1
          AND i.NombreNormalizado = N'SAN LUIS I'
          AND i.TensionKv = CONVERT(decimal(8,2), 115)
          AND geography::Point(i.Latitud, i.Longitud, 4326).STDistance(@Punto) < 50
    )
        THROW 52904, N'Preflight: cambió o desapareció la referencia canónica S.E. San Luis I.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaArista a
        WHERE a.VersionId = (SELECT MAX(VersionId) FROM dgmesnie.RedElectricaVersion WHERE Activa = 1)
          AND a.NombreNormalizado = N'SAN LUIS I LA PILA'
          AND a.TensionKv = CONVERT(decimal(8,2), 115)
          AND a.EstadoConexion = N'conectada'
          AND (a.NodoOrigenClave = N'se:b83bfc7077e57d646d9a'
               OR a.NodoDestinoClave = N'se:b83bfc7077e57d646d9a')
    )
        THROW 52905, N'Preflight: la topología vigente ya no confirma San Luis I - La Pila a 115 kV.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaArista a
        WHERE a.VersionId = (SELECT MAX(VersionId) FROM dgmesnie.RedElectricaVersion WHERE Activa = 1)
          AND a.NombreNormalizado = N'EL POTOSI SAN LUIS I'
          AND a.TensionKv = CONVERT(decimal(8,2), 230)
          AND a.EstadoConexion = N'conectada'
          AND (a.NodoOrigenClave = N'se:b83bfc7077e57d646d9a'
               OR a.NodoDestinoClave = N'se:b83bfc7077e57d646d9a')
    )
        THROW 52906, N'Preflight: la topología vigente ya no confirma El Potosí - San Luis I a 230 kV.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad, Municipio,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        39,
        N'S.E. San Luis Potosí / San Luis I · Banco 3',
        N'Point',
        N'{"type":"Point","coordinates":[-100.907603,22.094586]}',
        @Latitud,
        @Longitud,
        N'San Luis Potosí',
        N'San Luis Potosí',
        N'exacta',
        @Metodo,
        N'CFE · coordenadas geodésicas S.E. San Luis I; CENACE · P18-OC1 y topología oficial; inventario eléctrico DGMESNIE',
        f.FechaCorte,
        CONVERT(decimal(8,2), 0.10),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'CONCILIACIÓN DE IDENTIDAD: CENACE ubica P18-OC1 en la S.E. San Luis Potosí y describe su red de 115 kV hacia La Pila. ',
            N'PRODESEN identifica San Luis I como instalación de 230 kV de la región San Luis Potosí. ',
            N'CFE publicó para S.E. San Luis I 22°05''40.51" N, 100°54''27.37" O; el nodo canónico queda a 18.3 m. ',
            N'La topología vigente confirma enlaces San Luis I-La Pila 115 kV y El Potosí-San Luis I 230 kV. ',
            N'No representa las tres acometidas reubicadas como trazas individuales. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 39 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 52907, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 39 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND MetodoUbicacion = @Metodo
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%CONCILIACIÓN DE IDENTIDAD:%'
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 52908, N'Aplicación: la ubicación no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 39 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
