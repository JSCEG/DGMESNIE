SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT = 130;
DECLARE @PEM NVARCHAR(100) = N'CFE20-PCC';
DECLARE @RegistroClave NVARCHAR(100) = N'atlas_sen:f72fa24b33630e726bb793cc4ae9e137ca7de3ba';
DECLARE @Lote NVARCHAR(100) = N'PAM-UBICACION-PROFUNDA-20260802-001';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.vw_PAMProyectoVigente
        WHERE ProyectoId = @ProyectoId
          AND ClaveProyecto = @PEM
          AND EstadoVigenciaCartera = N'Vigente'
    )
        THROW 51501, N'Preflight: cambió la identidad o vigencia de CFE20-PCC.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaSubestacionInventario
        WHERE RegistroClave = @RegistroClave
          AND Activa = 1
          AND NombreNormalizado = N'OLAS ALTAS'
          AND TensionKv = 230
          AND FuenteCoordenadas = N'openstreetmap'
          AND Latitud BETWEEN 24.0348 AND 24.0351
          AND Longitud BETWEEN -110.3375 AND -110.3370
    )
        THROW 51502, N'Preflight: cambió el punto conciliado de Olas Altas.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = @ProyectoId
          AND Activa = 1
    )
        THROW 51503, N'Preflight: CFE20-PCC ya tiene una ubicación activa.', 1;

    DECLARE @Insertada TABLE
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
      INTO @Insertada
    SELECT
        @ProyectoId,
        i.Nombre,
        N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(NVARCHAR(50), CONVERT(decimal(11,7), i.Longitud)), N',',
               CONVERT(NVARCHAR(50), CONVERT(decimal(10,7), i.Latitud)), N']}'),
        CONVERT(decimal(10,7), i.Latitud),
        CONVERT(decimal(11,7), i.Longitud),
        N'geocodificada',
        N'investigacion_individual_conciliada_v2',
        N'CFE y DOF - domicilio oficial; DGMESNIE - convergencia de ocho trazas; Atlas SEN/OpenStreetMap - coordenada',
        p.FechaCorte,
        CONVERT(decimal(8,2), 0.15),
        1,
        1,
        1,
        1,
        N'Codex - investigacion individual autorizada - 2026-08-02',
        CONCAT(
            N'SE Olas Altas 230/115 kV; Banco 2. RegistroClave=', i.RegistroClave,
            N'; ocho trazas DGMESNIE convergen entre 0.0703 y 0.0956 km del punto; ',
            N'domicilio confirmado por CFE/DOF; coordenada de fuente abierta conservada como geocodificada; Lote=', @Lote, N'.')
    FROM dgmesnie.RedElectricaSubestacionInventario i
    CROSS JOIN dgmesnie.vw_PAMProyectoVigente p
    WHERE i.RegistroClave = @RegistroClave
      AND i.Activa = 1
      AND p.ProyectoId = @ProyectoId
      AND p.ClaveProyecto = @PEM
      AND p.EstadoVigenciaCartera = N'Vigente';

    IF (SELECT COUNT(*) FROM @Insertada) <> 1
        THROW 51504, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    COMMIT TRANSACTION;
    SELECT * FROM @Insertada;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
