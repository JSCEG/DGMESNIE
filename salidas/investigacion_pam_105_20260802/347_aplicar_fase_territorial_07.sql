SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-07';
DECLARE @Metodo NVARCHAR(80) = N'referencia_localidad_oficial_y_diagrama_red';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 25.841410);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -109.019200);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 78
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'D18-NO3'
          AND v.NombreProyecto = N'Compuertas Banco 1'
          AND v.GRT = N'NO'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND v.EtapaProyecto = N'Ejecución/Construcción'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
          AND v.ElementosEquiposAsociados LIKE N'%LT Compuertas Entronque Centenario%Mochis III%'
          AND v.ElementosEquiposAsociados LIKE N'%SE Compuertas Banco 1%'
          AND v.ElementosEquiposAsociados LIKE N'%30 MVA%115/13.8 kV%'
    )
        THROW 53801, N'Preflight: cambió la identidad, alcance, región, etapa, vigencia o corte del Proyecto 78.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 78 AND Activa = 1
    )
        THROW 53802, N'Preflight: D18-NO3 ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 53803, N'Preflight: el lote ya fue aplicado.', 1;

    IF @Latitud NOT BETWEEN CONVERT(decimal(9,6), 25.810000) AND CONVERT(decimal(9,6), 25.870000)
       OR @Longitud NOT BETWEEN CONVERT(decimal(10,6), -109.050000) AND CONVERT(decimal(10,6), -108.980000)
        THROW 53804, N'Preflight: la referencia no cae en Las Compuertas, Ahome, Sinaloa.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario
        WHERE Activa = 1
          AND Nombre = N'S.E. MOCHIS 3'
          AND TensionKv = CONVERT(decimal(9,3), 115.000)
          AND Latitud BETWEEN CONVERT(decimal(9,6), 25.810000) AND CONVERT(decimal(9,6), 25.830000)
          AND Longitud BETWEEN CONVERT(decimal(10,6), -109.005000) AND CONVERT(decimal(10,6), -108.985000)
    )
        THROW 53805, N'Preflight: no está disponible la corroboración de S.E. Mochis III en el inventario de red.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, Direccion, Entidad, Municipio, Localidad,
        PrecisionUbicacion, MetodoUbicacion, Fuente, FechaCorte,
        RadioSugeridoKm, Orden, EsPrincipal, Validada, Activa,
        UsuarioRegistro, FechaValidacionUtc, UsuarioValidacion, Observaciones
    )
    SELECT
        78,
        N'Las Compuertas · referencia territorial D18-NO3',
        N'Point',
        N'{"type":"Point","coordinates":[-109.019200,25.841410]}',
        @Latitud,
        @Longitud,
        N'Localidad Las Compuertas, C.P. 81361',
        N'Sinaloa',
        N'Ahome',
        N'Las Compuertas',
        N'geocodificada',
        @Metodo,
        N'CFE · PAM RGD 2019-2033; CENACE · diagramas unifilares RNT/RGD 2017-2022; INEGI · carta Los Mochis; inventario de red DGMESNIE/OSM',
        f.FechaCorte,
        CONVERT(decimal(8,2), 2.00),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'REFERENCIA TERRITORIAL, NO COORDENADA TOPOGRÁFICA: CFE registra Compuertas Banco 1 en la GCR Noroeste, Zona Los Mochis, con transformación 115/13.8 kV y 30 MVA. ',
            N'CENACE muestra Compuertas como nodo de 115/13.8 kV en el diagrama de la Zona Los Mochis, enlazado al corredor Mochis III-Centenario. ',
            N'El expediente PAM identifica LT Compuertas Entronque Centenario-Mochis III de 115 kV y SE Compuertas Banco 1. ',
            N'INEGI y fuentes abiertas ubican la localidad Las Compuertas en Ahome, Sinaloa. El inventario de red confirma S.E. Mochis 3 a 3.6 km, pero no contiene una geometría independiente de S.E. Compuertas. ',
            N'El punto es el centro de la localidad Las Compuertas (OSM node 1960875376), con radio de 2 km; no representa el centro geométrico exacto de la subestación. ',
            N'Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 78 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 53806, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 78 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'geocodificada'
          AND MetodoUbicacion = @Metodo
          AND RadioSugeridoKm = CONVERT(decimal(8,2), 2.00)
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%REFERENCIA TERRITORIAL, NO COORDENADA TOPOGRÁFICA:%'
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 53807, N'Aplicación: la referencia territorial no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, MetodoUbicacion, RadioSugeridoKm,
           Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 78 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
