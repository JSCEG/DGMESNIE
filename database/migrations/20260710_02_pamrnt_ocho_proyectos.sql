SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 2: incorpora los ocho proyectos identificados del PAMRNT 2026-2040.
  Fuente: Cuadro 8.4.3. Principales proyectos identificados de ampliacion de la RNT.
  No crea relaciones padre-hijo: esas se cargaran al identificar cada obra con evidencia documental.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dgmesnie.PAMProyectoVersion', N'PrioridadPrograma') IS NULL
        ALTER TABLE dgmesnie.PAMProyectoVersion ADD PrioridadPrograma INT NULL;

    IF COL_LENGTH(N'dgmesnie.PAMProyectoVersion', N'ZonaAtendida') IS NULL
        ALTER TABLE dgmesnie.PAMProyectoVersion ADD ZonaAtendida NVARCHAR(1000) NULL;

    IF COL_LENGTH(N'dgmesnie.PAMProyectoVersion', N'FechaNecesaria') IS NULL
        ALTER TABLE dgmesnie.PAMProyectoVersion ADD FechaNecesaria NVARCHAR(100) NULL;

    ALTER TABLE dgmesnie.PAMProyectoVersion ALTER COLUMN MontoProyectoMdp DECIMAL(18,3) NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @HashFuente CHAR(64) = 'C433CEE3F92AC30D120C742CD8B6BBFFD325AA8C21A2F6CBE4710D94205B9230';
    DECLARE @FuenteId BIGINT;

    SELECT @FuenteId = FuenteId
    FROM dgmesnie.PAMFuente
    WHERE HashSha256 = @HashFuente;

    IF @FuenteId IS NULL
    BEGIN
        INSERT dgmesnie.PAMFuente
        (
            NombreDocumento, TipoDocumento, FechaDocumento, FechaCorte, RutaArchivo,
            HojaPaginaSeccion, VersionDocumento, HashSha256, Observaciones, UsuarioRegistro
        )
        VALUES
        (
            N'PAMRNT 2026-2040 - Proyectos identificados', N'Markdown extraido', NULL,
            CONVERT(date, '2026-07-10'),
            N'D:\DGMESNIE\PAM Fichas\Proyectos_identificados_PAMRNT_2026_2040.md',
            N'Cuadro 8.4.3. Principales proyectos identificados de ampliacion de la RNT',
            N'Integracion inicial 2026-07-10', @HashFuente,
            N'Extracto estructurado del PAMRNT 2026-2040 usado para identificar los ocho proyectos principales.',
            SUSER_SNAME()
        );
        SET @FuenteId = SCOPE_IDENTITY();
    END;

    UPDATE dgmesnie.PAMFuente
    SET NombreDocumento = N'PAMRNT 2026-2040 - Proyectos identificados',
        TipoDocumento = N'Markdown extraído',
        HojaPaginaSeccion = N'Cuadro 8.4.3. Principales proyectos identificados de ampliación de la RNT',
        Observaciones = N'Extracto estructurado del PAMRNT 2026-2040 usado para identificar los ocho proyectos principales.'
    WHERE FuenteId = @FuenteId;

    DECLARE @CargaId BIGINT;
    SELECT @CargaId = CargaId
    FROM dgmesnie.PAMCarga
    WHERE FuenteId = @FuenteId
      AND TipoCarga = N'Alta proyectos identificados'
      AND EstadoCarga = N'Aplicada';

    IF @CargaId IS NULL
    BEGIN
        INSERT dgmesnie.PAMCarga
        (
            FuenteId, TipoCarga, EstadoCarga, FechaFinUtc, UsuarioCarga,
            RegistrosLeidos, RegistrosNuevos, MensajeResultado
        )
        VALUES
        (
            @FuenteId, N'Alta proyectos identificados', N'Aplicada', SYSUTCDATETIME(),
            SUSER_SNAME(), 8, 0,
            N'Incorporacion de proyectos identificados PAMRNT 2026-2040; sin relaciones padre-hijo en esta fase.'
        );
        SET @CargaId = SCOPE_IDENTITY();
    END;

    UPDATE dgmesnie.PAMCarga
    SET MensajeResultado = N'Incorporación de proyectos identificados PAMRNT 2026-2040; sin relaciones padre-hijo en esta fase.'
    WHERE CargaId = @CargaId;

    DECLARE @Proyectos TABLE
    (
        Prioridad       INT NOT NULL,
        GRT             NVARCHAR(100) NOT NULL,
        ClaveProyecto   NVARCHAR(200) NOT NULL,
        NombreProyecto  NVARCHAR(500) NOT NULL,
        FechaNecesaria  NVARCHAR(100) NOT NULL,
        ZonaAtendida    NVARCHAR(1000) NOT NULL,
        InversionMdp    DECIMAL(18,3) NOT NULL,
        TipoProyecto    NVARCHAR(50) NOT NULL
    );

    INSERT @Proyectos
        (Prioridad, GRT, ClaveProyecto, NombreProyecto, FechaNecesaria, ZonaAtendida, InversionMdp, TipoProyecto)
    VALUES
        (1, N'Peninsular', N'I26-PE1', N'Suministro de energía eléctrica para los estados de Tabasco, Campeche, Yucatán y Quintana Roo', N'abril de 2031', N'Villahermosa, Chontalpa, Los Ríos y GCR Peninsular / Tabasco, Campeche, Yucatán y Quintana Roo', 31293.202, N'Interregional'),
        (2, N'Occidental', N'P26-OC1', N'Suministro de energía en la zona metropolitana de Guadalajara', N'abril de 2031', N'Zona metropolitana de Guadalajara / Jalisco', 3489.949, N'Zonal/local'),
        (3, N'Central', N'P26-CE1', N'Suministro de energía eléctrica en la Ciudad de Pachuca', N'abril de 2030', N'Pachuca / Hidalgo', 601.316, N'Zonal/local'),
        (4, N'Noroeste', N'P26-NO1', N'Solución a la saturación de la transformación en el área suroeste de Hermosillo, Sonora', N'abril de 2030', N'Hermosillo / Sonora', 587.561, N'Zonal/local'),
        (5, N'Occidental', N'P26-OC2', N'Incremento de transformación en la zona Aguascalientes', N'abril de 2031', N'Aguascalientes / Aguascalientes', 320.292, N'Zonal/local'),
        (6, N'Norte', N'P26-NT1', N'Incremento de transformación en la SE Durango Dos', N'abril de 2031', N'Durango / Durango', 282.560, N'Zonal/local'),
        (7, N'Noroeste', N'P26-NO2', N'Incremento en la capacidad de transformación en el área norte de la Zona Culiacán', N'abril de 2030', N'Culiacán / Sinaloa', 516.206, N'Zonal/local'),
        (8, N'Baja California', N'P26-BC1', N'Incremento en la capacidad de transformación en la Subestación Eléctrica Mexicali Dos', N'abril de 2031', N'Mexicali / Baja California', 327.446, N'Zonal/local');

    DECLARE
        @Prioridad INT,
        @GRT NVARCHAR(100),
        @ClaveProyecto NVARCHAR(200),
        @NombreProyecto NVARCHAR(500),
        @FechaNecesaria NVARCHAR(100),
        @ZonaAtendida NVARCHAR(1000),
        @InversionMdp DECIMAL(18,3),
        @TipoProyecto NVARCHAR(50),
        @ProyectoId BIGINT,
        @ProyectoVersionId BIGINT,
        @Nuevos INT = 0;

    DECLARE proyecto_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT Prioridad, GRT, ClaveProyecto, NombreProyecto, FechaNecesaria,
               ZonaAtendida, InversionMdp, TipoProyecto
        FROM @Proyectos
        ORDER BY Prioridad;

    OPEN proyecto_cursor;
    FETCH NEXT FROM proyecto_cursor INTO
        @Prioridad, @GRT, @ClaveProyecto, @NombreProyecto, @FechaNecesaria,
        @ZonaAtendida, @InversionMdp, @TipoProyecto;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ProyectoId = NULL;
        SET @ProyectoVersionId = NULL;

        SELECT @ProyectoId = v.ProyectoId
        FROM dgmesnie.PAMProyectoVersion v
        WHERE v.EsVersionVigente = 1
          AND v.OrigenPrograma = N'PAMRNT'
          AND v.ClaveProyecto = @ClaveProyecto;

        IF @ProyectoId IS NULL
        BEGIN
            INSERT dgmesnie.PAMProyecto (ProyectoModernizacionIdOrigen, FechaAltaUtc, Activo)
            VALUES (NULL, SYSUTCDATETIME(), 1);
            SET @ProyectoId = SCOPE_IDENTITY();

            INSERT dgmesnie.PAMProyectoVersion
            (
                ProyectoId, NumeroVersion, FuenteId, CargaId, VigenteDesde, VigenteHasta,
                EsVersionVigente, OrigenPrograma, Programa, AnioPrograma, TipoProyecto,
                EstadoVigenciaCartera, MotivoCambio, ClaveProyecto, GRT, NombreProyecto,
                EtapaProyecto, MontoProyectoMdp, ClasificacionSener, ActivoEnFuente,
                PrioridadPrograma, ZonaAtendida, FechaNecesaria, HashContenido, UsuarioRegistro
            )
            VALUES
            (
                @ProyectoId, 1, @FuenteId, @CargaId, CONVERT(date, '2026-07-10'), NULL,
                1, N'PAMRNT', N'PAMRNT 2026-2040', 2026, @TipoProyecto,
                N'Vigente', N'Alta como proyecto identificado en el PAMRNT 2026-2040',
                @ClaveProyecto, @GRT, @NombreProyecto, N'Identificado', @InversionMdp,
                N'Proyecto identificado PAMRNT 2026-2040', 1,
                @Prioridad, @ZonaAtendida, @FechaNecesaria,
                CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(@ClaveProyecto, N'|', @NombreProyecto, N'|', @InversionMdp, N'|', @FechaNecesaria)), 2),
                SUSER_SNAME()
            );
            SET @ProyectoVersionId = SCOPE_IDENTITY();

            INSERT dgmesnie.PAMCambio
            (
                CargaId, ProyectoId, ProyectoVersionNuevaId, TipoCambio,
                Campo, ValorNuevo, UsuarioRegistro
            )
            VALUES
            (
                @CargaId, @ProyectoId, @ProyectoVersionId, N'Alta',
                N'Registro completo', CONCAT(@ClaveProyecto, N' - ', @NombreProyecto), SUSER_SNAME()
            );

            SET @Nuevos += 1;
        END;
        ELSE
        BEGIN
            SELECT @ProyectoVersionId = ProyectoVersionId
            FROM dgmesnie.PAMProyectoVersion
            WHERE ProyectoId = @ProyectoId
              AND EsVersionVigente = 1
              AND CargaId = @CargaId
              AND NumeroVersion = 1;

            IF @ProyectoVersionId IS NOT NULL
            BEGIN
                UPDATE dgmesnie.PAMProyectoVersion
                SET GRT = @GRT,
                    NombreProyecto = @NombreProyecto,
                    MontoProyectoMdp = @InversionMdp,
                    PrioridadPrograma = @Prioridad,
                    ZonaAtendida = @ZonaAtendida,
                    FechaNecesaria = @FechaNecesaria,
                    HashContenido = CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(@ClaveProyecto, N'|', @NombreProyecto, N'|', @InversionMdp, N'|', @FechaNecesaria)), 2)
                WHERE ProyectoVersionId = @ProyectoVersionId;

                UPDATE dgmesnie.PAMCambio
                SET ValorNuevo = CONCAT(@ClaveProyecto, N' - ', @NombreProyecto)
                WHERE CargaId = @CargaId
                  AND ProyectoId = @ProyectoId
                  AND ProyectoVersionNuevaId = @ProyectoVersionId
                  AND TipoCambio = N'Alta';
            END;
        END;

        FETCH NEXT FROM proyecto_cursor INTO
            @Prioridad, @GRT, @ClaveProyecto, @NombreProyecto, @FechaNecesaria,
            @ZonaAtendida, @InversionMdp, @TipoProyecto;
    END;

    CLOSE proyecto_cursor;
    DEALLOCATE proyecto_cursor;

    UPDATE dgmesnie.PAMCarga
    SET RegistrosNuevos =
        (
            SELECT COUNT(*)
            FROM dgmesnie.PAMProyectoVersion
            WHERE CargaId = @CargaId AND NumeroVersion = 1
        ),
        RegistrosSinCambio = 8 -
        (
            SELECT COUNT(*)
            FROM dgmesnie.PAMProyectoVersion
            WHERE CargaId = @CargaId AND NumeroVersion = 1
        )
    WHERE CargaId = @CargaId;

    IF (SELECT COUNT(*) FROM dgmesnie.PAMProyectoVersion
        WHERE EsVersionVigente = 1 AND OrigenPrograma = N'PAMRNT'
          AND ClaveProyecto IN (SELECT ClaveProyecto FROM @Proyectos)) <> 8
        THROW 51001, 'No se incorporaron correctamente los ocho proyectos PAMRNT.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'proyecto_cursor') >= 0
    BEGIN
        CLOSE proyecto_cursor;
        DEALLOCATE proyecto_cursor;
    END;
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

EXEC(N'
CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoVigente
AS
SELECT
    p.ProyectoId, p.ProyectoUid, p.ProyectoModernizacionIdOrigen,
    v.ProyectoVersionId, v.NumeroVersion, v.OrigenPrograma, v.Programa, v.AnioPrograma,
    v.TipoProyecto, v.EstadoVigenciaCartera, v.ClaveProyecto, v.Numero, v.NumeroOriginal,
    v.GRT, v.NombreProyecto, v.TipoFinanciamiento, v.AnioInstruccion, v.EtapaProyecto,
    v.MontoProyectoMdp, v.ElementosEquiposAsociados, v.FechaEstimadaInicio,
    v.FeoIndicadaOficioSener, v.FeoFactible, v.PorcentajeAvanceEjecucion,
    v.CircunstanciasAtrasos, v.AccionesMitigacionCorreccion, v.EstadoRealProyecto,
    v.ComentariosNivelPriorizacion, v.ClasificacionSener, v.FechaProgramacionTrimestre,
    v.QuincenaPublicacion, v.UniversoPresentacionPresidencia, v.Mva, v.Mvar, v.KmC,
    v.PrioridadPrograma, v.ZonaAtendida, v.FechaNecesaria,
    v.VigenteDesde, v.ActivoEnFuente, v.FuenteId, f.NombreDocumento AS FuenteDocumento,
    f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
WHERE p.Activo = 1;
');
GO

SELECT OrigenPrograma, EstadoVigenciaCartera, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
GROUP BY OrigenPrograma, EstadoVigenciaCartera
ORDER BY OrigenPrograma, EstadoVigenciaCartera;

SELECT PrioridadPrograma, ClaveProyecto, NombreProyecto, TipoProyecto, MontoProyectoMdp, FechaNecesaria
FROM dgmesnie.vw_PAMProyectoVigente
WHERE OrigenPrograma = N'PAMRNT'
ORDER BY PrioridadPrograma;
