SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Esperadas INT = 2;
DECLARE @ProyectosEsperados INT = 2;
DECLARE @Metodo NVARCHAR(80) = N'investigacion_documental_inventario_v1';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-DOC-20260802-03';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-02';

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE #Seleccion
    (
        ProyectoId BIGINT NOT NULL,
        PEM NVARCHAR(500) NOT NULL,
        RegistroClave NVARCHAR(80) NOT NULL,
        Rol NVARCHAR(60) NOT NULL,
        EsPrincipal BIT NOT NULL,
        Orden INT NOT NULL,
        Evidencia NVARCHAR(2000) NOT NULL
    );

    INSERT #Seleccion (ProyectoId, PEM, RegistroClave, Rol, EsPrincipal, Orden, Evidencia)
    VALUES
      (282, N'P26-BC1', N'dgmesnie_geojson:se:923bf4266ddc9471d74c', N'principal_probable', 1, 1, N'SE Mexicali II 230 kV; el título vigente individualiza exactamente el patio.'),
      (280, N'P26-NT1', N'dgmesnie_geojson:se:21f218f2411888779e49', N'principal_probable', 1, 1, N'SE Durango II 230 kV; se usa un único nodo representativo del patio 230/115 kV.');

    IF (SELECT COUNT(*) FROM #Seleccion) <> @Esperadas
        THROW 51401, N'Preflight: el lote no contiene exactamente 2 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM #Seleccion) <> @ProyectosEsperados
        THROW 51402, N'Preflight: el lote no contiene exactamente 2 proyectos.', 1;
    IF EXISTS (SELECT 1 FROM #Seleccion GROUP BY ProyectoId, RegistroClave HAVING COUNT(*) > 1)
        THROW 51403, N'Preflight: existen relaciones proyecto-elemento duplicadas.', 1;
    IF EXISTS (SELECT 1 FROM #Seleccion GROUP BY ProyectoId HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1)
        THROW 51404, N'Preflight: cada proyecto debe tener exactamente un punto representativo.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Seleccion s
        LEFT JOIN dgmesnie.vw_PAMProyectoVigente p
          ON p.ProyectoId = s.ProyectoId
         AND p.ClaveProyecto = s.PEM
         AND p.EstadoVigenciaCartera = N'Vigente'
        WHERE p.ProyectoId IS NULL
    )
        THROW 51405, N'Preflight: cambió la identidad o vigencia de uno o más proyectos.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Seleccion s
        LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i
          ON i.RegistroClave = s.RegistroClave
         AND i.Activa = 1
         AND i.FuenteCoordenadas = N'dgmesnie_geojson'
         AND i.EstadoValidacion = N'catalogado'
         AND TRY_CONVERT(float, i.Latitud) BETWEEN 14 AND 33.5
         AND TRY_CONVERT(float, i.Longitud) BETWEEN -118 AND -86
        WHERE i.RegistroClave IS NULL
    )
        THROW 51406, N'Preflight: un nodo ya no coincide con el inventario DGMESNIE catalogado.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Seleccion s
        JOIN dgmesnie.PAMProyectoUbicacion u WITH (UPDLOCK, HOLDLOCK)
          ON u.ProyectoId = s.ProyectoId
         AND u.Activa = 1
    )
        THROW 51407, N'Preflight: un proyecto del lote ya tiene una ubicación activa.', 1;

    DECLARE @Insertadas TABLE
    (
        UbicacionId BIGINT,
        ProyectoId BIGINT,
        Etiqueta NVARCHAR(300),
        PrecisionUbicacion NVARCHAR(40),
        EsPrincipal BIT,
        Validada BIT,
        Activa BIT
    );

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta,
           inserted.PrecisionUbicacion, inserted.EsPrincipal,
           inserted.Validada, inserted.Activa
      INTO @Insertadas
    SELECT
        s.ProyectoId,
        i.Nombre,
        N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(NVARCHAR(50), CONVERT(decimal(11,7), i.Longitud)), N',',
               CONVERT(NVARCHAR(50), CONVERT(decimal(10,7), i.Latitud)), N']}'),
        CONVERT(decimal(10,7), i.Latitud),
        CONVERT(decimal(11,7), i.Longitud),
        N'exacta',
        @Metodo,
        N'DGMESNIE · inventario conciliado de subestaciones · expediente PAMRNT · verificación documental oficial',
        p.FechaCorte,
        CONVERT(decimal(8,2), 2),
        s.Orden,
        s.EsPrincipal,
        1,
        1,
        @Usuario,
        LEFT(CONCAT(
            N'Investigación documental confirmada por usuario. Rol=', s.Rol,
            N'; RegistroClave=', i.RegistroClave,
            N'; UniversoClave=', i.UniversoClave,
            N'; Inventario=', COALESCE(CONVERT(NVARCHAR(30), i.TensionKv), N's/d'), N' kV/', COALESCE(i.NivelRed, N's/d'),
            N'; Evidencia=', s.Evidencia,
            N'; FuenteDocumento=', COALESCE(p.FuenteDocumento, N''),
            N'; Lote=', @Lote, N'.'), 1000)
    FROM #Seleccion s
    JOIN dgmesnie.RedElectricaSubestacionInventario i
      ON i.RegistroClave = s.RegistroClave
     AND i.Activa = 1
     AND i.FuenteCoordenadas = N'dgmesnie_geojson'
     AND i.EstadoValidacion = N'catalogado'
    JOIN dgmesnie.vw_PAMProyectoVigente p
      ON p.ProyectoId = s.ProyectoId
     AND p.ClaveProyecto = s.PEM
     AND p.EstadoVigenciaCartera = N'Vigente';

    IF (SELECT COUNT(*) FROM @Insertadas) <> @Esperadas
        THROW 51408, N'Aplicación: no se insertaron exactamente 2 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM @Insertadas) <> @ProyectosEsperados
        THROW 51409, N'Aplicación: no se insertaron exactamente 2 proyectos.', 1;

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS Insertadas,
           COUNT(DISTINCT ProyectoId) AS Proyectos,
           SUM(CASE WHEN Validada = 1 AND Activa = 1 THEN 1 ELSE 0 END) AS ActivasValidadas
    FROM @Insertadas;
    SELECT * FROM @Insertadas ORDER BY ProyectoId, EsPrincipal DESC, UbicacionId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
