SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Esperadas INT = 22;
DECLARE @ProyectosEsperados INT = 18;
DECLARE @Metodo NVARCHAR(80) = N'investigacion_documental_inventario_v1';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-DOC-20260802-02';
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
      (155, N'M20-OR2', N'dgmesnie_geojson:se:5dd2590151c758227301', N'principal_probable',   1, 1, N'SE Tecali 400 kV; coincide con CS2, CS3 y CS4 del expediente oficial.'),
      (201, N'M21-MU1', N'dgmesnie_geojson:se:fa70732064d94666991f', N'principal_probable',   1, 1, N'SE Santa Rosalía 115 kV; mismo patio de la modernización de barras.'),
      (193, N'M21-OR4', N'dgmesnie_geojson:se:770739eda3cb066e5b52', N'extremo_linea',        1, 1, N'Tecamachalco, extremo explícito de la LT Tecamachalco-Tlacotepec 115 kV.'),
      (193, N'M21-OR4', N'dgmesnie_geojson:se:d243a072003c40c00cad', N'extremo_linea',        0, 2, N'Tlacotepec Puebla, extremo explícito; se descarta el homónimo de Guerrero.'),
      (233, N'M22-OR1', N'dgmesnie_geojson:se:3f388a17ca8e65b04ab8', N'principal_probable',   1, 1, N'Patio Pie de la Cuesta; la intervención es el CEV de 230 kV dentro del sitio.'),
      (235, N'M23-NO1', N'dgmesnie_geojson:se:8f64ae18643bcfb7a401', N'principal_probable',   1, 1, N'SE Puerto Peñasco; modernización de barras 115 kV en el mismo patio catalogado.'),
      (248, N'M24-OR1', N'dgmesnie_geojson:se:2ffe443413e2f202fd31', N'principal_probable',   1, 1, N'SE Zocac 230 kV; coincide con la modernización de barras del expediente.'),
      (9,   N'P15-OC1', N'dgmesnie_geojson:se:a2e8aae20012fe46f1cf', N'principal_probable',   1, 1, N'SE Querétaro I; sustitución del Banco 1 230/115 kV en el mismo patio.'),
      (26,  N'P16-OR1', N'dgmesnie_geojson:se:472b0788c58c6c90ed64', N'principal_probable',   1, 1, N'SE Matamoros conciliada como Izúcar de Matamoros por nombre, zona y diagrama.'),
      (54,  N'P17-BS1', N'dgmesnie_geojson:se:2c4d2be7753659884023', N'principal_probable',   1, 1, N'SE Loreto de Baja California Sur; se descartan homónimos de otras entidades.'),
      (46,  N'P17-OC10',N'dgmesnie_geojson:se:43639d8f0dda9720f99a', N'principal_probable',   1, 1, N'SE Querétaro Potencia; Banco 4 230/115 kV documentado.'),
      (22,  N'P17-OR6', N'dgmesnie_geojson:se:0bc99892490dca5ca0cd', N'componente_declarado', 1, 1, N'SE Amozoc, primer banco de compensación explícito del PEM.'),
      (22,  N'P17-OR6', N'dgmesnie_geojson:se:fb6aaf4ecc88f50f4de2', N'componente_declarado', 0, 2, N'SE Acatzingo, segundo banco de compensación explícito del PEM.'),
      (25,  N'P17-OR7', N'dgmesnie_geojson:se:bd301e135898dff641aa', N'principal_probable',   1, 1, N'SE El Esfuerzo; coincidencia nominal y eléctrica del proyecto de compensación.'),
      (29,  N'P18-MU1', N'dgmesnie_geojson:se:fa70732064d94666991f', N'principal_probable',   1, 1, N'SE Santa Rosalía; Banco 2 115/13.8 kV en el mismo patio.'),
      (28,  N'P18-MU3', N'dgmesnie_geojson:se:76ed294a6b5893edb65b', N'principal_probable',   1, 1, N'SE Mezquital de Baja California Sur; traslado de reactor documentado.'),
      (100, N'P18-NE3', N'dgmesnie_geojson:se:c212b070926132a9fefe', N'principal_probable',   1, 1, N'SE San Jerónimo Potencia; Banco 2 400/115 kV en el mismo patio.'),
      (153, N'P20-BS3', N'dgmesnie_geojson:se:80e9ac4aeb22f593a3ac', N'principal_probable',   1, 1, N'SE Villa Constitución; STATCOM documentado en el mismo patio.'),
      (204, N'P21-BS1', N'dgmesnie_geojson:se:1186c5b4ee44d03a8856', N'componente_declarado', 1, 1, N'SE Cabo San Lucas II, primer componente explícito del PEM.'),
      (204, N'P21-BS1', N'dgmesnie_geojson:se:5c81ed40c6388fc2344c', N'componente_declarado', 0, 2, N'SE Cabo Bello, segundo componente explícito del PEM.'),
      (204, N'P21-BS1', N'dgmesnie_geojson:se:b7ff7844a7dc7f133943', N'componente_declarado', 0, 3, N'SE San José del Cabo, tercer componente explícito del PEM.'),
      (264, N'P25-BC1', N'dgmesnie_geojson:se:4d3d190bef6ae1756589', N'principal_probable',   1, 1, N'SE El Rubí; incremento de transformación documentado en el patio.');

    IF (SELECT COUNT(*) FROM #Seleccion) <> @Esperadas
        THROW 51301, N'Preflight: el lote no contiene exactamente 22 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM #Seleccion) <> @ProyectosEsperados
        THROW 51302, N'Preflight: el lote no contiene exactamente 18 proyectos.', 1;
    IF EXISTS (SELECT 1 FROM #Seleccion GROUP BY ProyectoId, RegistroClave HAVING COUNT(*) > 1)
        THROW 51303, N'Preflight: existen relaciones proyecto-elemento duplicadas.', 1;
    IF EXISTS (SELECT 1 FROM #Seleccion GROUP BY ProyectoId HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1)
        THROW 51304, N'Preflight: cada proyecto debe tener exactamente un punto representativo.', 1;
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
        THROW 51305, N'Preflight: cambió la identidad o vigencia de uno o más proyectos.', 1;
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
        THROW 51306, N'Preflight: un nodo ya no coincide con el inventario DGMESNIE catalogado.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Seleccion s
        JOIN dgmesnie.PAMProyectoUbicacion u WITH (UPDLOCK, HOLDLOCK)
          ON u.ProyectoId = s.ProyectoId
         AND u.Activa = 1
    )
        THROW 51307, N'Preflight: un proyecto del lote ya tiene una ubicación activa.', 1;

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
        N'DGMESNIE · inventario conciliado de subestaciones · expediente PAM/PAMRNT · verificación documental oficial',
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
        THROW 51308, N'Aplicación: no se insertaron exactamente 22 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM @Insertadas) <> @ProyectosEsperados
        THROW 51309, N'Aplicación: no se insertaron exactamente 18 proyectos.', 1;

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
