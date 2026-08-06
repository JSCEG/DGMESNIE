SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT = 24;
DECLARE @PEM NVARCHAR(30) = N'P15-NO3';
DECLARE @RegistroClave NVARCHAR(100) = N'atlas_sen:2d239bebed1464c2f3882606f6113ed3ec3c3140';
DECLARE @Lote NVARCHAR(80) = N'PAM-UBICACION-INDIVIDUAL-20260802-004';
DECLARE @Metodo NVARCHAR(80) = N'investigacion_individual_conciliada_v2';
DECLARE @Usuario NVARCHAR(300) = N'Codex - autorizado por usuario - 2026-08-02';

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.vw_PAMProyectoVigente
        WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM
          AND EstadoVigenciaCartera=N'Vigente'
    )
        THROW 51401, N'Preflight: cambio la identidad o vigencia de P15-NO3.', 1;

    IF EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId=@ProyectoId AND Activa=1
    )
        THROW 51402, N'Preflight: P15-NO3 ya tiene una ubicacion activa.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario
        WHERE RegistroClave=@RegistroClave AND Activa=1
          AND Nombre=N'Esperanza' AND TensionKv=230
          AND FuenteCoordenadas=N'openstreetmap'
          AND EstadoValidacion=N'validada_automatica_fuente_abierta'
          AND TRY_CONVERT(float,Latitud) BETWEEN 28.82 AND 28.83
          AND TRY_CONVERT(float,Longitud) BETWEEN -111.48 AND -111.47
    )
        THROW 51403, N'Preflight: cambio el registro territorial conciliado de Esperanza.', 1;

    DECLARE @Insertada TABLE (UbicacionId BIGINT, ProyectoId BIGINT, Etiqueta NVARCHAR(300), Validada BIT, Activa BIT);

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, Observaciones
    )
    OUTPUT inserted.UbicacionId, inserted.ProyectoId, inserted.Etiqueta, inserted.Validada, inserted.Activa
      INTO @Insertada
    SELECT
        @ProyectoId, N'SE Esperanza', N'Point',
        CONCAT(N'{"type":"Point","coordinates":[',
               CONVERT(NVARCHAR(50),CONVERT(decimal(11,7),i.Longitud)),N',',
               CONVERT(NVARCHAR(50),CONVERT(decimal(10,7),i.Latitud)),N']}'),
        CONVERT(decimal(10,7),i.Latitud), CONVERT(decimal(11,7),i.Longitud),
        N'geocodificada', @Metodo,
        N'CENACE PAMRNT 2020 y 2023; CFE programas de inversion y anexo tecnico; OpenStreetMap ODbL',
        p.FechaCorte, CONVERT(decimal(8,2),0.20), 1, 1,
        1, 1, @Usuario,
        LEFT(CONCAT(N'P15-NO3: patio Esperanza conciliado con OSM way/119237239, operador CFE y tensiones 230/115 kV; reactor declarado 21 MVAr/13.8 kV dentro del patio. RegistroClave=',@RegistroClave,N'; Lote=',@Lote,N'.'),1000)
    FROM dgmesnie.RedElectricaSubestacionInventario i
    CROSS JOIN
    (
        SELECT TOP (1) FechaCorte FROM dgmesnie.vw_PAMProyectoVigente
        WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM
    ) p
    WHERE i.RegistroClave=@RegistroClave AND i.Activa=1;

    IF (SELECT COUNT(*) FROM @Insertada) <> 1
        THROW 51404, N'Aplicacion: no se inserto exactamente una ubicacion.', 1;

    COMMIT TRANSACTION;
    SELECT * FROM @Insertada;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
