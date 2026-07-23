-- ════════════════════════════════════════════════════════════════════════════
-- Stored Procedures — dgmesnie.sp_PODECOBI_*
-- Patrón: usado por RepositorioPODECOBIPolos.cs (Dapper)
-- Convenciones:
--   - Parámetros @Prefijo + CamelCase
--   - Fechas como DATE; auditoría como DATETIME2(0)
--   - Sin SELECT * : siempre columnas explícitas
--   - Estado de verificación: 'VERIFICADO' | 'PENDIENTE' | 'NO_PUBLICADO' | 'NO_APLICA' | 'CONTRADICTORIO'
-- ════════════════════════════════════════════════════════════════════════════
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ── Listar polos (con búsqueda opcional) ──────────────────────────────────
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_Listar') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_Listar;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_Listar
    @Busqueda NVARCHAR(200) = NULL,
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.PoloId, p.Numero, p.NombreOficial, p.NombreManual,
        p.Estado, p.Municipio, p.Activo,
        p.FechaDeclaracion, p.UrlDeclaracion,
        p.AreaOficialHa, p.AreaGeojsonHa, p.GeojsonDeltaPct,
        p.CentroidLon, p.CentroidLat,
        p.Etapa, p.Subetapa, p.FechaRevisionPublica,
        p.UrlProyectosMexico, p.AvanceManualPct,
        p.Inversion, p.Empleos,
        p.DemandaElectrica, p.DemandaElectricaNota, p.DemandaMaxima, p.DemandaMaximaNota,
        p.Tension, p.Conexion,
        p.GasDisponibilidad, p.Ducto,
        p.CorteFuente, p.Verificacion,
        -- Vocaciones agregadas como CSV (sin subconsulta correlacionada costosa)
        STUFF((SELECT ', ' + v.Vocacion
               FROM dgmesnie.PODECOBI_Vocacion v
               WHERE v.PoloId = p.PoloId
               ORDER BY v.Orden
               FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') AS VocacionesCsv,
        -- Contactos resumidos (uno por ámbito)
        (SELECT TOP 1 c.Nombre + ' · ' + ISNULL(c.Correo,'')
         FROM dgmesnie.PODECOBI_Contacto c WHERE c.PoloId = p.PoloId AND c.Ambito = 'FEDERAL') AS ContactoFederal,
        (SELECT TOP 1 c.Nombre + ' · ' + ISNULL(c.Correo,'')
         FROM dgmesnie.PODECOBI_Contacto c WHERE c.PoloId = p.PoloId AND c.Ambito = 'ESTATAL') AS ContactoEstatal
    FROM dgmesnie.PODECOBI_Polo p
    WHERE (@SoloActivos = 0 OR p.Activo = 1)
      AND (
        @Busqueda IS NULL OR LTRIM(RTRIM(@Busqueda)) = ''
        OR p.NombreOficial   LIKE '%' + @Busqueda + '%'
        OR p.NombreManual    LIKE '%' + @Busqueda + '%'
        OR p.Estado          LIKE '%' + @Busqueda + '%'
        OR p.Municipio      LIKE '%' + @Busqueda + '%'
        OR p.Numero         LIKE '%' + @Busqueda + '%'
        OR p.Etapa          LIKE '%' + @Busqueda + '%'
      )
    ORDER BY p.Numero;
END
GO

-- ── Obtener polo por Numero (incluye vocaciones, contactos, geometría) ─────
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_ObtenerPorNumero') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_ObtenerPorNumero;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_ObtenerPorNumero
    @Numero VARCHAR(2)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cabecera del polo
    SELECT
        p.PoloId, p.Numero, p.NombreOficial, p.NombreManual,
        p.Estado, p.Municipio, p.Activo,
        p.FechaDeclaracion, p.UrlDeclaracion, p.Modificacion, p.UrlModificacion,
        p.Convenio, p.UrlConvenio, p.ComiteSesion,
        p.AreaOficialHa, p.AreaManualRaw, p.AreaGeojsonHa,
        p.GeojsonDeltaHa, p.GeojsonDeltaPct,
        p.GeojsonFeatureCount, p.GeojsonValid, p.GeojsonValidity,
        p.CentroidLon, p.CentroidLat,
        p.Etapa, p.Subetapa, p.FechaRevisionPublica, p.UrlProyectosMexico, p.AvanceManualPct,
        p.Inversion, p.Empleos,
        p.DemandaElectrica, p.DemandaElectricaNota, p.DemandaMaxima, p.DemandaMaximaNota,
        p.Tension, p.Conexion,
        p.GasDisponibilidad, p.GasNota, p.Ducto,
        p.CorteFuente, p.Verificacion, p.ComentarioVerificacion,
        p.FechaRegistro, p.FechaActualizacion
    FROM dgmesnie.PODECOBI_Polo p
    WHERE p.Numero = @Numero;

    -- Vocaciones (ordenadas)
    SELECT VocacionId, Vocacion, Orden
    FROM dgmesnie.PODECOBI_Vocacion
    WHERE PoloId = (SELECT PoloId FROM dgmesnie.PODECOBI_Polo WHERE Numero = @Numero)
    ORDER BY Orden;

    -- Contactos (todos los ámbitos)
    SELECT ContactoId, Ambito, Nombre, Cargo, Correo, Telefono, Notas
    FROM dgmesnie.PODECOBI_Contacto
    WHERE PoloId = (SELECT PoloId FROM dgmesnie.PODECOBI_Polo WHERE Numero = @Numero)
    ORDER BY CASE Ambito WHEN 'FEDERAL' THEN 1 WHEN 'ESTATAL' THEN 2 WHEN 'MUNICIPAL' THEN 3 ELSE 9 END;

    -- Fuentes verificadas
    SELECT FuenteId, Tipo, Url, Fecha, Descripcion, Sha256
    FROM dgmesnie.PODECOBI_Fuente
    WHERE PoloId = (SELECT PoloId FROM dgmesnie.PODECOBI_Polo WHERE Numero = @Numero)
    ORDER BY CASE Tipo WHEN 'DOF' THEN 1 WHEN 'SIDOF' THEN 2 WHEN 'PROYECTOS_MX' THEN 3
                        WHEN 'PRESIDENCIA' THEN 4 WHEN 'GEOJSON' THEN 5 WHEN 'MANUAL' THEN 6 ELSE 9 END,
             Fecha DESC;

    -- Geometría (polígono(s) — puede haber 2 si Hermosillo está representado en dos entidades)
    SELECT GeometriaId, FeatureIndex, GeometryType, GeometryJson, PropertiesJson,
           IsValid, ValidityNote, AreaHa, PerimetroM, SourceUrl, Sha256, FechaDescarga
    FROM dgmesnie.PODECOBI_Geometria
    WHERE PoloId = (SELECT PoloId FROM dgmesnie.PODECOBI_Polo WHERE Numero = @Numero)
    ORDER BY FeatureIndex;
END
GO

-- ── Obtener polo por click en mapa (lookup por geometría contenida en lon/lat) ─
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_ObtenerPorCoordenada') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_ObtenerPorCoordenada;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_ObtenerPorCoordenada
    @Lon DECIMAL(18,8),
    @Lat DECIMAL(18,8),
    @TolGrados DECIMAL(9,6) = 0.001   -- ~110 m en el ecuador
AS
BEGIN
    SET NOCOUNT ON;
    -- Estrategia simple: bbox del centroide ± tolerancia, y prueba polígono por polígono
    -- con STIntersects si SQL Server geography está disponible; si no, devuelve el más
    -- cercano por distancia Haversine.
    DECLARE @PoloId INT = NULL;

    -- Intento 1: polígono interseca (requiere geometry GEOGRAPHY en la columna)
    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('dgmesnie.PODECOBI_Geometria')
          AND name = 'Geography'
    )
    BEGIN
        DECLARE @P GEOGRAPHY = geography::Point(@Lat, @Lon, 4326);
        SELECT TOP 1 @PoloId = g.PoloId
        FROM dgmesnie.PODECOBI_Geometria g
        WHERE g.Geography.STIntersects(@P) = 1
        ORDER BY g.Geography.STDistance(@P);
    END

    -- Fallback: bbox por centroide
    IF @PoloId IS NULL
    BEGIN
        SELECT TOP 1 @PoloId = p.PoloId
        FROM dgmesnie.PODECOBI_Polo p
        WHERE p.CentroidLon IS NOT NULL
          AND ABS(p.CentroidLon - @Lon) <= @TolGrados
          AND ABS(p.CentroidLat - @Lat) <= @TolGrados
        ORDER BY (
            (p.CentroidLon - @Lon) * (p.CentroidLon - @Lon)
          + (p.CentroidLat - @Lat) * (p.CentroidLat - @Lat)
        ) ASC;
    END

    IF @PoloId IS NULL RETURN;

    DECLARE @Numero VARCHAR(2);
    SELECT @Numero = Numero FROM dgmesnie.PODECOBI_Polo WHERE PoloId = @PoloId;
    EXEC dgmesnie.sp_PODECOBI_Polo_ObtenerPorNumero @Numero = @Numero;
END
GO

-- ── Listar polos como FeatureCollection (alimenta capa GeoJSON del mapa) ────
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_ListarGeoJSON') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_ListarGeoJSON;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_ListarGeoJSON
AS
BEGIN
    SET NOCOUNT ON;
    -- Devuelve filas listas para que el servidor/API las envuelva como GeoJSON FeatureCollection.
    SELECT
        p.Numero,
        p.NombreOficial,
        p.Estado,
        p.Municipio,
        p.AreaOficialHa,
        p.AreaGeojsonHa,
        p.Etapa,
        p.CentroidLon AS Lon,
        p.CentroidLat AS Lat,
        (
            SELECT g.GeometryJson, g.FeatureIndex, g.GeometryType, g.IsValid, g.AreaHa
            FROM dgmesnie.PODECOBI_Geometria g
            WHERE g.PoloId = p.PoloId
            ORDER BY g.FeatureIndex
            FOR JSON AUTO, INCLUDE_NULL_VALUES
        ) AS GeometriasJson
    FROM dgmesnie.PODECOBI_Polo p
    WHERE p.Activo = 1
    ORDER BY p.Numero;
END
GO

-- ── Catálogo: polos por estado (para filtros del dashboard) ────────────────
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_ListarEstados') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_ListarEstados;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_ListarEstados
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Estado, COUNT(*) AS PoloCount
    FROM dgmesnie.PODECOBI_Polo
    WHERE Activo = 1
    GROUP BY Estado
    ORDER BY Estado;
END
GO

-- ── Resumen métricas nacionales (para KPI del dashboard) ───────────────────
IF OBJECT_ID('dgmesnie.sp_PODECOBI_Polo_ObtenerResumen') IS NOT NULL DROP PROC dgmesnie.sp_PODECOBI_Polo_ObtenerResumen;
GO
CREATE PROC dgmesnie.sp_PODECOBI_Polo_ObtenerResumen
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*)                                                  AS TotalPolos,
        COUNT(DISTINCT Estado)                                    AS EstadosCount,
        SUM(ISNULL(AreaOficialHa, 0))                             AS SuperficieTotalHa,
        SUM(CASE WHEN Etapa = 'EJECUCION' THEN 1 ELSE 0 END)      AS EnEjecucion,
        SUM(CASE WHEN Etapa = 'PREINVERSION' THEN 1 ELSE 0 END)   AS EnPreinversion,
        MAX(CorteFuente)                                          AS UltimoCorte,
        MAX(FechaActualizacion)                                   AS UltimaActualizacion
    FROM dgmesnie.PODECOBI_Polo
    WHERE Activo = 1;
END
GO

PRINT 'sp_podecobi.sql — SPs dgmesnie.sp_PODECOBI_* listos.';
GO
