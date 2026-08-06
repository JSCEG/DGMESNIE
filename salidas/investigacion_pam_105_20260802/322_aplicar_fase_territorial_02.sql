SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Lote NVARCHAR(80) = N'PAM-FASE-TERRITORIAL-20260803-02';
DECLARE @Metodo NVARCHAR(80) = N'conciliacion_documental_individual_v2';
DECLARE @Usuario NVARCHAR(300) = N'Codex · autorizado por usuario · 2026-08-03';
DECLARE @Latitud decimal(9,6) = CONVERT(decimal(9,6), 28.166052);
DECLARE @Longitud decimal(10,6) = CONVERT(decimal(10,6), -105.445552);
DECLARE @Punto geography = geography::Point(@Latitud, @Longitud, 4326);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoVersion v WITH (UPDLOCK, HOLDLOCK)
        JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
        WHERE v.ProyectoId = 52
          AND v.EsVersionVigente = 1
          AND v.ClaveProyecto = N'P17-NT5'
          AND v.NombreProyecto = N'Francisco Villa Banco 3'
          AND v.EstadoVigenciaCartera = N'Vigente'
          AND f.FechaCorte = CONVERT(date, '2026-07-30')
    )
        THROW 52301, N'Preflight: cambió la identidad, clave, vigencia o corte del Proyecto 52.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE ProyectoId = 52 AND Activa = 1
    )
        THROW 52302, N'Preflight: Francisco Villa ya tiene una ubicación activa; revisar antes de duplicar.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Observaciones LIKE N'%' + @Lote + N'%'
    )
        THROW 52303, N'Preflight: el lote ya fue aplicado.', 1;

    IF (SELECT COUNT(*) FROM dgmesnie.RedElectricaVersion WHERE Activa = 1) <> 1
        THROW 52304, N'Preflight: no existe una única versión activa del grafo eléctrico.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.RedElectricaNodo n
        JOIN dgmesnie.RedElectricaVersion rv
          ON rv.VersionId = n.VersionId AND rv.Activa = 1
        WHERE n.NombreNormalizado = N'FRANCISCO VILLA'
          AND geography::Point(n.Latitud, n.Longitud, 4326).STDistance(@Punto) <= 500
          AND n.TensionKv IN (115, 230, 400)
    ) < 4
        THROW 52305, N'Preflight: el punto documental ya no concuerda con los cuatro extremos canónicos de Francisco Villa.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.RedElectricaNodo n
        JOIN dgmesnie.RedElectricaVersion rv
          ON rv.VersionId = n.VersionId AND rv.Activa = 1
        WHERE n.NombreNormalizado = N'FRANCISCO VILLA'
          AND n.TensionKv = 115
          AND geography::Point(n.Latitud, n.Longitud, 4326).STDistance(@Punto) <= 50
    )
        THROW 52306, N'Preflight: falta la concordancia de Francisco Villa con el extremo 115 kV a menos de 50 m.', 1;

    INSERT dgmesnie.PAMProyectoUbicacion
    (
        ProyectoId, Etiqueta, TipoGeometria, GeometriaJson,
        Latitud, Longitud, PrecisionUbicacion, MetodoUbicacion,
        Fuente, FechaCorte, RadioSugeridoKm, Orden, EsPrincipal,
        Validada, Activa, UsuarioRegistro, FechaValidacionUtc,
        UsuarioValidacion, Observaciones
    )
    SELECT
        52,
        N'SE Francisco Villa · Banco 3',
        N'Point',
        N'{"type":"Point","coordinates":[-105.445552,28.166052]}',
        @Latitud,
        @Longitud,
        N'exacta',
        @Metodo,
        N'CFE Transmisión AD-OP-ZTC-008/2015; ASF auditoría 2003-160-12; grafo eléctrico DGMESNIE activo',
        f.FechaCorte,
        CONVERT(decimal(8,2), 1),
        1, 1, 1, 1, @Usuario, SYSUTCDATETIME(), @Usuario,
        LEFT(CONCAT(
            N'CFE ubica la SE Francisco Villa dentro del predio de la CT Francisco Villa, carretera Camargo-Delicias km 61, Delicias, Chihuahua. ',
            N'El registro georreferenciado de la auditoría ASF 2003-160-12 aporta la coordenada; el grafo activo la corrobora con cuatro extremos FRANCISCO VILLA a 0.032-0.293 km. ',
            N'Se descarta el homónimo S.E. FRANCISCO VILLA (MATAMOROS) de Baja California, a más de 1,190 km. Lote=', @Lote, N'.'), 1000)
    FROM dgmesnie.PAMProyectoVersion v
    JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE v.ProyectoId = 52 AND v.EsVersionVigente = 1;

    IF @@ROWCOUNT <> 1
        THROW 52307, N'Aplicación: no se insertó exactamente una ubicación.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyectoUbicacion
        WHERE ProyectoId = 52 AND Activa = 1 AND Validada = 1
          AND PrecisionUbicacion = N'exacta'
          AND Latitud = @Latitud AND Longitud = @Longitud
          AND Observaciones LIKE N'%' + @Lote + N'%'
          AND ISJSON(GeometriaJson) = 1
    )
        THROW 52308, N'Aplicación: la ubicación insertada no superó la verificación interna.', 1;

    COMMIT TRANSACTION;

    SELECT UbicacionId, ProyectoId, Etiqueta, Latitud, Longitud,
           PrecisionUbicacion, Validada, Activa, Fuente
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE ProyectoId = 52 AND Observaciones LIKE N'%' + @Lote + N'%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
