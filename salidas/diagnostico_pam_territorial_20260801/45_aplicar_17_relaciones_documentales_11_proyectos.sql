SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Esperadas INT = 17;
DECLARE @ProyectosEsperados INT = 11;
DECLARE @Metodo NVARCHAR(80) = N'investigacion_documental_inventario_v1';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-DOC-20260802-01';
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
      (10,  N'P15-CE1', N'dgmesnie_geojson:se:6ffc96dc0f080a641074', N'principal_probable',    1, 1, N'Patio Donato Guerra conciliado; el expediente oficial identifica DOG-400 y reactores de 400 kV. Se usa un solo nodo DGMESNIE para evitar duplicar el patio.'),
      (12,  N'P17-OR4', N'dgmesnie_geojson:se:0a5711f597aad00e6d70', N'extremo_linea',         1, 1, N'Componente explícito de la fase San Jacinto Tlacotepec-Pinotepa Nacional; no representa la fase Jalapa de Díaz-Oaxaca Potencia.'),
      (12,  N'P17-OR4', N'dgmesnie_geojson:se:a0b6eb47c71f7e8e4f47', N'extremo_linea',         0, 2, N'Componente explícito de la fase San Jacinto Tlacotepec-Pinotepa Nacional; no representa la fase Jalapa de Díaz-Oaxaca Potencia.'),
      (36,  N'M18-OR1', N'dgmesnie_geojson:se:64488cca9788d876f423', N'extremo_linea',         1, 1, N'Temascal II es el extremo común de los dos corredores de 400 kV confirmados documentalmente.'),
      (36,  N'M18-OR1', N'dgmesnie_geojson:se:59f7e7a45e3c81d564d7', N'extremo_linea',         0, 2, N'Chinameca Potencia es extremo explícito del corredor Chinameca Potencia-A3260-Temascal II.'),
      (36,  N'M18-OR1', N'dgmesnie_geojson:se:8cea3d1239578d35667a', N'extremo_linea',         0, 3, N'Minatitlán II es extremo explícito del corredor Minatitlán II-A3360-Temascal II.'),
      (43,  N'P18-OC2', N'dgmesnie_geojson:se:bdd77168a2310bf91a6a', N'extremo_linea',         1, 1, N'Tepic II es el primer extremo del enlace Tepic II-Cerro Blanco confirmado por CENACE.'),
      (43,  N'P18-OC2', N'dgmesnie_geojson:se:bc070391e92bd88e6c93', N'extremo_linea',         0, 2, N'Cerro Blanco es el segundo extremo del enlace Tepic II-Cerro Blanco confirmado por CENACE.'),
      (97,  N'P19-BC1', N'dgmesnie_geojson:se:4a1ed6f83da2d5803bef', N'principal_probable',    1, 1, N'El proyecto corresponde al Banco 4 de la SE Tijuana I; coinciden identidad, sistema y patio.'),
      (154, N'M20-CE1', N'dgmesnie_geojson:se:6ffc96dc0f080a641074', N'principal_probable',    1, 1, N'Patio Donato Guerra conciliado; el proyecto interviene CS1, CS2 y CS3 en 400 kV. Se usa un solo nodo DGMESNIE para evitar duplicar el patio.'),
      (176, N'P21-CE1', N'dgmesnie_geojson:se:1ba988b1d58efe32e683', N'extremo_linea',         1, 1, N'Texcoco es extremo explícito del corredor Teotihuacán-Texcoco de 400 kV y se usa como punto representativo.'),
      (176, N'P21-CE1', N'dgmesnie_geojson:se:8e3623d25a1f4b2475a2', N'extremo_linea',         0, 2, N'Teotihuacán es extremo explícito; el inventario local representa el patio catalogado y no afirma por sí solo el nivel 400 kV.'),
      (188, N'P21-PE1', N'dgmesnie_geojson:se:4cca75244a8a99cd59a9', N'principal_probable',    1, 1, N'El expediente identifica un banco 230/115 kV en la SE Lerma y el inventario coincide en sitio y GCR Peninsular.'),
      (243, N'P24-NT1', N'dgmesnie_geojson:se:87aee325a581db88a27a', N'componente_declarado',  1, 1, N'Norte Cereso es componente explícito y cabecera del corredor hacia Samalayuca Sur; el nodo local CERESO representa ese sitio conciliado.'),
      (243, N'P24-NT1', N'dgmesnie_geojson:se:56452a0327b0af76c0aa', N'extremo_linea',         0, 2, N'Samalayuca Sur es extremo explícito de la LT Norte Cereso-93660-Samalayuca Sur. No se afirma cobertura de Terranova, Paso del Norte ni Reforma.'),
      (247, N'P24-BC2', N'dgmesnie_geojson:se:68205751ca0a68e0491e', N'componente_declarado',  1, 1, N'Lomas es un componente explícito con geometría firme; no se afirma cobertura de Valle de Guadalupe, aún sin punto conciliado.'),
      (267, N'M25-PE1', N'dgmesnie_geojson:se:501c03ac09a7966ca4e0', N'principal_probable',    1, 1, N'La intervención corresponde al autotransformador AT7 de la SE Valladolid; coincide el sitio y la GCR Peninsular.');

    IF (SELECT COUNT(*) FROM #Seleccion) <> @Esperadas
        THROW 51201, N'Preflight: el lote no contiene exactamente 17 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM #Seleccion) <> @ProyectosEsperados
        THROW 51202, N'Preflight: el lote no contiene exactamente 11 proyectos.', 1;
    IF EXISTS
    (
        SELECT 1 FROM #Seleccion
        GROUP BY ProyectoId, RegistroClave
        HAVING COUNT(*) > 1
    )
        THROW 51203, N'Preflight: existen relaciones proyecto-elemento duplicadas.', 1;
    IF EXISTS
    (
        SELECT 1 FROM #Seleccion
        GROUP BY ProyectoId
        HAVING SUM(CASE WHEN EsPrincipal = 1 THEN 1 ELSE 0 END) <> 1
    )
        THROW 51204, N'Preflight: cada proyecto debe tener exactamente un punto representativo.', 1;
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
        THROW 51205, N'Preflight: cambió la identidad o vigencia de uno o más proyectos.', 1;
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
        THROW 51206, N'Preflight: un nodo ya no coincide con el inventario DGMESNIE catalogado.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM #Seleccion s
        JOIN dgmesnie.PAMProyectoUbicacion u WITH (UPDLOCK, HOLDLOCK)
          ON u.ProyectoId = s.ProyectoId
         AND u.Activa = 1
    )
        THROW 51207, N'Preflight: un proyecto del lote ya tiene una ubicación activa.', 1;

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
        THROW 51208, N'Aplicación: no se insertaron exactamente 17 relaciones.', 1;
    IF (SELECT COUNT(DISTINCT ProyectoId) FROM @Insertadas) <> @ProyectosEsperados
        THROW 51209, N'Aplicación: no se insertaron exactamente 11 proyectos.', 1;

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
