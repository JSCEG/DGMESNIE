SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Lote NVARCHAR(80) = N'PAM-TERRITORIAL-REGIONAL-20260806-02';
DECLARE @Metodo NVARCHAR(80) = N'localidad_area_influencia_documentada';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-06';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Esperados TABLE
    (
        ProyectoId BIGINT NOT NULL PRIMARY KEY,
        ProyectoVersionId BIGINT NOT NULL,
        ClaveProyecto NVARCHAR(30) NOT NULL,
        NombreProyecto NVARCHAR(500) NOT NULL
    );

    INSERT @Esperados VALUES
        (175, 1698, N'P21-NO1', N'Compensación capacitiva al noroeste de la zona Mazatlán'),
        (199, 1722, N'M21-NT1', N'Repotenciación de la Línea de Transmisión Cuauhtémoc - 73840 - Maniobras Treinta y Cuatro');

    IF
    (
        SELECT COUNT(*)
        FROM @Esperados e
        INNER JOIN dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
            ON v.ProyectoId = e.ProyectoId
           AND v.ProyectoVersionId = e.ProyectoVersionId
           AND v.ClaveProyecto = e.ClaveProyecto
           AND v.NombreProyecto = e.NombreProyecto
           AND v.EsVersionVigente = 1
           AND v.EstadoVigenciaCartera = N'Vigente'
        INNER JOIN dgmesnie.PAMFuente f
            ON f.FuenteId = v.FuenteId
           AND f.FechaCorte = CONVERT(date, '2026-07-30')
    ) <> 2
        THROW 54661, N'Preflight: cambió la identidad, versión, vigencia o corte de alguno de los dos PEM.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId IN (175, 199)
          AND Activa = 1
    )
        THROW 54662, N'Preflight: alguno de los dos PEM ya tiene ubicación activa.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 54663, N'Preflight: el lote regional ya fue aplicado.', 1;

    DECLARE @Geometrias TABLE
    (
        ProyectoId BIGINT NOT NULL,
        Etiqueta NVARCHAR(300) NOT NULL,
        Latitud DECIMAL(9,6) NOT NULL,
        Longitud DECIMAL(10,6) NOT NULL,
        Entidad NVARCHAR(200) NOT NULL,
        Municipio NVARCHAR(300) NOT NULL,
        Localidad NVARCHAR(300) NOT NULL,
        RadioSugeridoKm DECIMAL(8,2) NOT NULL,
        Orden INT NOT NULL,
        EsPrincipal BIT NOT NULL,
        Fuente NVARCHAR(500) NOT NULL,
        Observacion NVARCHAR(1000) NOT NULL
    );

    INSERT @Geometrias VALUES
        (199, N'Cuauhtémoc · referencia regional de M21-NT1',
         28.400958, -106.866531, N'Chihuahua', N'Cuauhtémoc', N'Cuauhtémoc',
         25.00, 1, 1,
         N'CENACE · PAMRNT 2021-2035, ficha M21-NT1; OpenStreetMap Nominatim · Cuauhtémoc; DGMESNIE · municipios.geojson CVEGEO 08017',
         N'REFERENCIA REGIONAL VALIDADA: CENACE ubica el área de influencia en los municipios de Cuauhtémoc y Chihuahua. Este punto representa la ciudad de Cuauhtémoc, no la traza de la LT 73840 ni la posición de Maniobras Treinta y Cuatro.'),
        (199, N'Chihuahua · segunda referencia regional de M21-NT1',
         28.636867, -106.076745, N'Chihuahua', N'Chihuahua', N'Chihuahua',
         30.00, 2, 0,
         N'CENACE · PAMRNT 2021-2035, ficha M21-NT1; OpenStreetMap Nominatim · Chihuahua; DGMESNIE · municipios.geojson CVEGEO 08019',
         N'REFERENCIA REGIONAL VALIDADA: segundo municipio del área de influencia declarada por CENACE. No representa una subestación, estructura ni coordenada de la línea.'),
        (175, N'La Cruz de Elota · referencia regional de P21-NO1',
         23.921728, -106.892640, N'Sinaloa', N'Elota', N'La Cruz de Elota',
         10.00, 1, 1,
         N'CENACE · Diagrama Unifilar RNT y RGD 2024-2029, zona Mazatlán, P21-NO1/La Cruz; OpenStreetMap Nominatim · La Cruz de Elota; DGMESNIE · municipios.geojson CVEGEO 25008',
         N'REFERENCIA REGIONAL VALIDADA: el diagrama oficial vincula P21-NO1 con La Cruz, 115 kV. El punto representa la localidad La Cruz de Elota; no es la coordenada oficial de la subestación ni del banco de capacitores.');

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        g.ProyectoId, g.Etiqueta, N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(varchar(30), g.Longitud), N',',
               CONVERT(varchar(30), g.Latitud), N']}'),
        g.Latitud, g.Longitud, g.Entidad, g.Municipio, g.Localidad,
        N'regional', @Metodo, g.Fuente, NULL,
        g.RadioSugeridoKm, g.Orden, g.EsPrincipal, 1, 1,
        @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(g.Observacion, N' Lote=', @Lote, N'.'), 1000)
    FROM @Geometrias g;

    IF @@ROWCOUNT <> 3
        THROW 54664, N'Aplicación: no se insertaron exactamente las tres referencias regionales.', 1;

    IF EXISTS
    (
        SELECT ProyectoId
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId IN (175, 199)
          AND Activa = 1
          AND Observaciones LIKE N'%' + @Lote + N'%'
        GROUP BY ProyectoId
        HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
    )
        THROW 54665, N'Aplicación: cada PEM debe conservar exactamente una referencia principal.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
       Entidad, Municipio, Localidad, PrecisionUbicacion,
       RadioSugeridoKm, EsPrincipal, Validada, Activa
FROM dgmesnie.PAMProyectoUbicacion
WHERE Observaciones LIKE N'%' + @Lote + N'%'
ORDER BY ProyectoId, Orden;
